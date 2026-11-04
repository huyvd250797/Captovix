-- Source: supabase/migrations/202611040001_activation_timestamp.sql
-- Captovix V1.38.0 -> V1.39.0. Run AFTER 202610280003_serverless.sql.
-- Add signed activation metadata from the existing device row. No table/data reset.
-- Existing trials, keys, seats, rate limits, request replay and recovery policy unchanged.
-- CREATE OR REPLACE preserves the existing function grants; private tables stay private.
begin;
create or replace function public.cvx_license_operation(p_op text,p_input jsonb,p_public_hash text,p_nonce text,p_days integer default 30,p_cooldown integer default 72)
returns jsonb language plpgsql security definer set search_path='' set timezone='UTC' as $$
declare
 t timestamptz; d captovix_license.devices%rowtype; l captovix_license.licenses%rowtype;
 fp text:=p_input->>'deviceId'; nm text:=p_input->>'deviceName'; activation text:=coalesce(p_input->>'activationId','');
 kh text:=encode(sha256(convert_to(upper(replace(btrim(coalesce(p_input->>'key','')),'-','')),'UTF8')),'hex');
 recovery text; active_count integer; st text; lease timestamptz;
begin
 if p_op is null or jsonb_typeof(p_input) is distinct from 'object' or fp is null or nm is null or p_public_hash is null or p_nonce is null or p_days is null or p_cooldown is null or p_op not in ('trial','activate','refresh','release','recover') or fp!~'^[a-fA-F0-9]{64}$' or length(nm) not between 1 and 80 or p_public_hash!~'^[a-f0-9]{64}$' or p_nonce!~'^[a-fA-F0-9]{48}$' or p_days not between 1 and 30 or p_cooldown not between 1 and 720 then raise exception 'REQUEST_INVALID';end if;
 perform pg_advisory_xact_lock(12928001); t:=clock_timestamp();
 delete from captovix_license.nonces where expires_at<t;
 insert into captovix_license.nonces values(p_nonce,t+interval '10 minutes') on conflict do nothing;
 if not found then raise exception 'REQUEST_REPLAY';end if;
 if p_op='trial' then
  if exists(select 1 from captovix_license.devices where fingerprint=fp and license_id is not null) then raise exception 'USE_LICENSE_ACTIVATION_OR_RECOVERY';end if;
  select * into d from captovix_license.devices where fingerprint=fp and license_id is null;
  if found and d.public_hash<>p_public_hash then raise exception 'TRIAL_ALREADY_STARTED_CONTACT_SUPPORT';end if;
  if d.id is null then
   insert into captovix_license.devices(id,fingerprint,public_hash,name,trial_until)
   values(replace(gen_random_uuid()::text,'-',''),fp,p_public_hash,nm,t+interval '720 hours') returning * into d;
   insert into captovix_license.audit(action,subject,detail) values('trial.started',d.id,'30 days');
  end if;
 elsif p_op in ('activate','recover') then
  select * into l from captovix_license.licenses where key_hash=kh;
  if not found or l.revoked then raise exception 'KEY_INVALID_OR_REVOKED';end if;
  if p_op='recover' then
   if l.recovery_hash<>encode(sha256(convert_to(btrim(coalesce(p_input->>'recoveryCode','')),'UTF8')),'hex') then raise exception 'RECOVERY_INVALID';end if;
   if l.recovered_at is not null and t<l.recovered_at+make_interval(hours=>p_cooldown) then raise exception 'RECOVERY_COOLDOWN';end if;
   recovery:=substring(encode(sha256(convert_to(gen_random_uuid()::text||gen_random_uuid()::text,'UTF8')),'hex'),1,48);
   update captovix_license.devices set released=true where license_id=l.id;
   update captovix_license.licenses set recovery_hash=encode(sha256(convert_to(recovery,'UTF8')),'hex'),recovered_at=t where id=l.id;
   insert into captovix_license.audit(action,subject,detail) values('license.recovered',l.id,'All old seats released; recovery rotated');
  end if;
  select * into d from captovix_license.devices where license_id=l.id and not released and fingerprint=fp and public_hash=p_public_hash;
  if d.id is null then
   select count(*) into active_count from captovix_license.devices where license_id=l.id and not released;
   if active_count>=l.seats or exists(select 1 from captovix_license.devices where license_id=l.id and not released and fingerprint=fp) then raise exception 'SEATS_FULL_USE_TRANSFER_OR_RECOVERY';end if;
   insert into captovix_license.devices(id,license_id,fingerprint,public_hash,name)
   values(replace(gen_random_uuid()::text,'-',''),l.id,fp,p_public_hash,nm) returning * into d;
   insert into captovix_license.audit(action,subject,detail) values('device.activated',l.id,d.id);
  end if;
 else
  if activation<>'' then
   select * into d from captovix_license.devices where id=activation and fingerprint=fp and public_hash=p_public_hash;
  else
   select * into d from captovix_license.devices where fingerprint=fp and public_hash=p_public_hash order by (license_id is not null) desc,created_at desc limit 1;
  end if;
  if d.id is null then raise exception 'DEVICE_NOT_FOUND';end if;
  if d.license_id is not null then select * into l from captovix_license.licenses where id=d.license_id;end if;
  if p_op='release' then
   if l.id is null then raise exception 'TRIAL_CANNOT_TRANSFER';end if;
   update captovix_license.devices set released=true where id=d.id; d.released:=true;
   insert into captovix_license.audit(action,subject,detail) values('device.released',l.id,d.id);
  end if;
 end if;
 update captovix_license.devices set seen_at=t where id=d.id;
 st:=case when d.released then 'released' when coalesce(l.revoked,false) or d.trial_until<=t then 'revoked' else 'active' end;
 lease:=t+case when l.id is null then interval '48 hours' else make_interval(days=>p_days) end;
 if st='active' and d.trial_until is not null then lease:=least(lease,d.trial_until);end if;
 return jsonb_build_object('claims',jsonb_build_object('schema',1,'product','HuyVo.Captovix','licenseId',coalesce(l.id,'trial'),'activationId',d.id,'deviceId',fp,'publicKeyHash',p_public_hash,'kind',case when l.id is null then 'trial' else 'perpetual' end,'status',st,'issuedAt',t,'activatedAt',d.created_at,'leaseUntil',lease,'trialUntil',d.trial_until),'recoveryCode',recovery);
end;$$;

commit;
