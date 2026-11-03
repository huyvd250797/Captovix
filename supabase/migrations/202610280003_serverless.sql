-- Additive upgrade from V1.29.0. Run AFTER 202610280001_foundation.sql.
-- No customer/media reset. Private schema must NOT be exposed through Data API.
begin;
create table if not exists captovix_license.admins(user_id uuid primary key,created_at timestamptz not null default now());
create table if not exists captovix_license.rate_buckets(bucket text primary key,window_at timestamptz not null,hits integer not null);
alter table captovix_license.audit add column if not exists actor_id uuid;
alter table captovix_license.admins enable row level security;
alter table captovix_license.rate_buckets enable row level security;
revoke all on captovix_license.admins,captovix_license.rate_buckets from public,anon,authenticated;

create or replace function public.cvx_license_ready() returns jsonb
language sql security definer set search_path='' set timezone='UTC' as $$
 select jsonb_build_object('ready',true,'migration','1.29.1','licenses',exists(select 1 from captovix_license.licenses limit 1));
$$;

create or replace function public.cvx_license_rate(p_bucket text,p_limit integer) returns boolean
language plpgsql security definer set search_path='' set timezone='UTC' as $$
declare n integer; w timestamptz:=date_trunc('minute',clock_timestamp());
begin
 if length(p_bucket)>100 or p_limit not between 1 and 1000 then raise exception 'RATE_INPUT';end if;
 delete from captovix_license.rate_buckets where window_at<w-interval '2 minutes';
 insert into captovix_license.rate_buckets as b values(p_bucket,w,1)
 on conflict(bucket) do update set window_at=w,hits=case when b.window_at=w then b.hits+1 else 1 end returning hits into n;
 return n<=p_limit;
end;$$;

create or replace function public.cvx_license_admin_allowed(p_actor uuid) returns boolean
language sql security definer set search_path='' set timezone='UTC' as $$select exists(select 1 from captovix_license.admins where user_id=p_actor);$$;

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
 return jsonb_build_object('claims',jsonb_build_object('schema',1,'product','HuyVo.Captovix','licenseId',coalesce(l.id,'trial'),'activationId',d.id,'deviceId',fp,'publicKeyHash',p_public_hash,'kind',case when l.id is null then 'trial' else 'perpetual' end,'status',st,'issuedAt',t,'leaseUntil',lease,'trialUntil',d.trial_until),'recoveryCode',recovery);
end;$$;

create or replace function public.cvx_license_admin(p_actor uuid,p_action text,p_input jsonb default '{}') returns jsonb
language plpgsql security definer set search_path='' set timezone='UTC' as $$
declare
 q text:=coalesce(p_input->>'query',''); pg integer:=coalesce((p_input->>'page')::integer,0);
 v_id text:=p_input->>'id'; reason text:=btrim(coalesce(p_input->>'reason','')); v_seats integer; v_days integer;
 key text; recovery text; raw text; result jsonb; affected integer;
begin
 if not exists(select 1 from captovix_license.admins where user_id=p_actor) then raise exception 'ADMIN_FORBIDDEN';end if;
 if length(q)>200 or pg not between 0 and 100000 then raise exception 'ADMIN_INPUT_INVALID';end if;
 perform pg_advisory_xact_lock(12928001);
 if p_action='list' then
  select coalesce(jsonb_agg(to_jsonb(a)),'[]') into result from (
   select l.id,l.key_hint as "keyHint",l.customer,l.seats,l.revoked,l.created_at as "createdAt",l.recovered_at as "recoveredAt",
    (select coalesce(jsonb_agg(to_jsonb(v)),'[]') from (select d.id,d.name,d.fingerprint,d.public_hash as "publicHash",d.released,d.seen_at as "seenAt",d.created_at as "createdAt" from captovix_license.devices d where d.license_id=l.id order by d.released asc,d.created_at desc limit 100) v) as devices
   from captovix_license.licenses l where l.customer ilike '%'||q||'%' or l.id=q or l.key_hint=q order by l.created_at desc limit 50 offset pg*50
  ) a; return result;
 elsif p_action='trials' then
  select coalesce(jsonb_agg(to_jsonb(a)),'[]') into result from (select d.id,d.name,d.fingerprint,d.public_hash as "publicHash",d.trial_until as "trialUntil",d.released,d.seen_at as "seenAt" from captovix_license.devices d where d.license_id is null and (d.id=q or d.fingerprint=q or d.name ilike '%'||q||'%') order by d.created_at desc limit 50 offset pg*50) a;return result;
 elsif p_action='audit' then
  select coalesce(jsonb_agg(to_jsonb(a)),'[]') into result from (select id,created_at as "createdAt",action,subject,detail,actor_id as "actorId" from captovix_license.audit order by id desc limit 100 offset pg*100) a;return result;
 elsif p_action='create' then
  v_seats:=coalesce((p_input->>'seats')::integer,1);
  if length(btrim(coalesce(p_input->>'customer',''))) not between 1 and 200 or v_seats not between 1 and 100 then raise exception 'ADMIN_INPUT_INVALID';end if;
  raw:=upper(substring(encode(sha256(convert_to(gen_random_uuid()::text||gen_random_uuid()::text,'UTF8')),'hex'),1,40));
  key:='CVX-'||substring(raw,1,5)||'-'||substring(raw,6,5)||'-'||substring(raw,11,5)||'-'||substring(raw,16,5)||'-'||substring(raw,21,5)||'-'||substring(raw,26,5)||'-'||substring(raw,31,5)||'-'||substring(raw,36,5);
  recovery:=substring(encode(sha256(convert_to(gen_random_uuid()::text||gen_random_uuid()::text,'UTF8')),'hex'),1,48);
  v_id:=replace(gen_random_uuid()::text,'-','');
  insert into captovix_license.licenses(id,key_hash,key_hint,customer,seats,recovery_hash) values(v_id,encode(sha256(convert_to(replace(key,'-',''),'UTF8')),'hex'),right(raw,5),btrim(p_input->>'customer'),v_seats,encode(sha256(convert_to(recovery,'UTF8')),'hex'));
  insert into captovix_license.audit(action,subject,detail,actor_id) values('license.created',v_id,'seats='||v_seats,p_actor);
  return jsonb_build_object('id',v_id,'key',key,'recoveryCode',recovery);
 end if;
 if length(reason) not between 1 and 300 or v_id!~'^[a-f0-9]{32}$' then raise exception 'REASON_REQUIRED';end if;
 if p_action in ('revoke','restore') then update captovix_license.licenses set revoked=(p_action='revoke') where licenses.id=v_id;
 elsif p_action='seats' then
  v_seats:=(p_input->>'seats')::integer;
  if v_seats is null or v_seats not between 1 and 100 then raise exception 'ADMIN_INPUT_INVALID';end if;
  if (select count(*) from captovix_license.devices where license_id=v_id and not released)>v_seats then raise exception 'RELEASE_SEATS_FIRST';end if;
  update captovix_license.licenses set seats=v_seats where licenses.id=v_id;
 elsif p_action='release-device' then update captovix_license.devices set released=true where devices.id=v_id and license_id is not null;
 elsif p_action='rotate-recovery' then
  recovery:=substring(encode(sha256(convert_to(gen_random_uuid()::text||gen_random_uuid()::text,'UTF8')),'hex'),1,48);
  update captovix_license.licenses set recovery_hash=encode(sha256(convert_to(recovery,'UTF8')),'hex') where licenses.id=v_id;
 elsif p_action='repair-trial' then
  if coalesce(p_input->>'devicePublicHash','')!~'^[a-fA-F0-9]{64}$' then raise exception 'ADMIN_INPUT_INVALID';end if;
  update captovix_license.devices set public_hash=lower(p_input->>'devicePublicHash') where devices.id=v_id and license_id is null;
 elsif p_action in ('revoke-trial','restore-trial') then update captovix_license.devices set released=(p_action='revoke-trial') where devices.id=v_id and license_id is null;
 elsif p_action='extend-trial' then
  v_days:=(p_input->>'days')::integer;if v_days is null or v_days not between 1 and 30 then raise exception 'ADMIN_INPUT_INVALID';end if;
  update captovix_license.devices set trial_until=greatest(trial_until,clock_timestamp())+make_interval(days=>v_days) where devices.id=v_id and license_id is null;
 else raise exception 'ADMIN_ACTION_INVALID';end if;
 get diagnostics affected=row_count;if affected<>1 then raise exception 'NOT_FOUND';end if;
 insert into captovix_license.audit(action,subject,detail,actor_id) values('admin.'||p_action,v_id,reason,p_actor);
 return jsonb_build_object('ok',true,'recoveryCode',recovery);
end;$$;

revoke all on function public.cvx_license_ready() from public,anon,authenticated;
revoke all on function public.cvx_license_rate(text,integer) from public,anon,authenticated;
revoke all on function public.cvx_license_admin_allowed(uuid) from public,anon,authenticated;
revoke all on function public.cvx_license_operation(text,jsonb,text,text,integer,integer) from public,anon,authenticated;
revoke all on function public.cvx_license_admin(uuid,text,jsonb) from public,anon,authenticated;
grant execute on function public.cvx_license_ready(),public.cvx_license_rate(text,integer),public.cvx_license_admin_allowed(uuid),public.cvx_license_operation(text,jsonb,text,text,integer,integer),public.cvx_license_admin(uuid,text,jsonb) to service_role;
notify pgrst,'reload schema';
commit;
