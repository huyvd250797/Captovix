// Requires an isolated migrated PostgreSQL database. Never use production.
import postgres from 'postgres';import assert from 'node:assert/strict';import {mkdtemp,readFile,writeFile,rm} from 'node:fs/promises';import {tmpdir} from 'node:os';import {join,resolve} from 'node:path';import {spawnSync} from 'node:child_process';
import {createHandler,Fault} from '../../supabase/functions/licensing/index.ts';import {identity,proof,authority,request} from './helpers.mjs';
if(!process.env.LICENSE_TEST_DB)throw Error('Set LICENSE_TEST_DB to an isolated migrated test database.');
const sql=postgres(process.env.LICENSE_TEST_DB,{max:8});const owner=postgres(process.env.LICENSE_TEST_OWNER_DB||process.env.LICENSE_TEST_DB,{max:2});let n=0;
function check(ok,label){assert(ok,label);console.log('PASS '+label);n++}
const methods={cvx_license_ready:[[],[]],cvx_license_rate:[['p_bucket','p_limit'],['text','integer']],cvx_license_admin_allowed:[['p_actor'],['uuid']],cvx_license_operation:[['p_op','p_input','p_public_hash','p_nonce','p_days','p_cooldown'],['text','jsonb','text','text','integer','integer']],cvx_license_admin:[['p_actor','p_action','p_input'],['uuid','text','jsonb']]};
async function rpc(name,input={}){const [names,types]=methods[name]||[];if(!names)throw Error('Unknown RPC');const args=names.map((key,i)=>types[i]==='jsonb'?sql.json(input[key]):input[key]);try{return (await sql.unsafe(`select public.${name}(${types.map((type,i)=>'$'+(i+1)+'::'+type).join(',')}) as value`,args))[0].value}catch(e){if(e.code!=='P0001')console.error('SQL_FIXTURE_ERROR',e.code,e.message);if(e.code==='P0001')throw new Fault(e.message,e.message==='ADMIN_FORBIDDEN'?403:400);throw e}}
const adminId='00000000-0000-4000-8000-000000000001';const outsider='00000000-0000-4000-8000-000000000002';
const auth=await authority();const a=await identity('A-'+crypto.randomUUID());const b=await identity('B-'+crypto.randomUUID());
const env={SUPABASE_URL:'https://project.supabase.co',SUPABASE_SERVICE_ROLE_KEY:'test-service-only',LICENSE_ISSUER:'test',LICENSE_SIGNING_KEY_B64:auth.b64,ADMIN_ORIGINS:'https://admin.pages.dev'};
const fakeAuth=async(url,options)=>new Response(JSON.stringify({id:options?.headers?.Authorization==='Bearer admin.session'?adminId:outsider}));
const handler=createHandler(env,fakeAuth,rpc);
async function call(path,body,admin=false){const response=await handler(request(path,body,admin?{Authorization:'Bearer admin.session',Origin:'https://admin.pages.dev'}:{}));return {status:response.status,data:await response.json()}}
async function device(id,operation,overrides={}){return call('/v1/'+operation,await proof(id,operation,overrides))}
const claims=response=>JSON.parse(Buffer.from(response.data.certificate.payload,'base64').toString());
async function action(id,action,extra={}){return call('/admin/licenses/'+id,{action,reason:'Isolated integration test',...extra},true)}
try{
 await owner`insert into captovix_license.admins(user_id) values(${adminId}) on conflict do nothing`;
 check((await rpc('cvx_license_ready')).ready,'Additive schema ready');
 check((await owner`select count(*)::int n from captovix_license.licenses where id='11111111111111111111111111111111'`)[0].n===1,'V1.29.0 license survives serverless migration');
 let denied=false;try{await sql`select * from captovix_license.licenses`}catch(e){denied=e.code==='42501'}check(denied,'Service role has RPC access, not direct private-table grants in fixture');
 const anon=postgres(process.env.LICENSE_TEST_DB.replace('service_role','anon'));try{await anon`select public.cvx_license_ready()`;check(false,'Anon must not execute privileged RPC')}catch(e){check(e.code==='42501','Anonymous role cannot call privileged RPC')}finally{await anon.end()}
 const authenticated=postgres(process.env.LICENSE_TEST_DB.replace('service_role','authenticated'));try{await authenticated`select public.cvx_license_admin_allowed(${adminId}::uuid)`;check(false,'Authenticated must not execute admin RPC')}catch(e){check(e.code==='42501','Ordinary authenticated role cannot call admin RPC')}finally{await authenticated.end()}
 check((await handler(request('/admin/licenses',null,{Authorization:'Bearer outsider.session'}))).status===403,'Valid Auth user without private membership is forbidden');
 const started=Date.now();const trialProof=await proof(a,'trial');const trial=await call('/v1/trial',trialProof);check(trial.status===200,'Trial request reaches real transactional RPC');let tc=claims(trial);
 check(Math.abs(Date.parse(tc.trialUntil)-started-30*86400000)<5000,'Server UTC starts exactly 30 days');
 check(Date.parse(tc.leaseUntil)-Date.parse(tc.issuedAt)<=48*3600000,'Trial offline lease capped at 48h');
 check((await call('/v1/trial',trialProof)).data.code==='REQUEST_REPLAY','Replayed proof cannot consume a second activation');
 const again=await device(a,'trial');check(claims(again).trialUntil===tc.trialUntil,'Retry does not reset 30-day clock');
 const changedKey=await identity('changed');changedKey.deviceId=a.deviceId;
 check((await device(changedKey,'trial')).data.code==='TRIAL_ALREADY_STARTED_CONTACT_SUPPORT','Trial cannot silently replace device private key');
 check((await action(tc.activationId,'repair-trial',{devicePublicHash:changedKey.publicHash})).status===200,'Verified admin repairs trial identity');
 check(claims(await device(changedKey,'trial')).trialUntil===tc.trialUntil,'Repair keeps original trial deadline');
 await action(tc.activationId,'repair-trial',{devicePublicHash:a.publicHash});
 await action(tc.activationId,'revoke-trial');check(claims(await device(a,'refresh',{activationId:tc.activationId})).status==='released','Admin can lock trial');
 await action(tc.activationId,'restore-trial');check(claims(await device(a,'refresh',{activationId:tc.activationId})).status==='active','Admin can restore trial');
 await owner`update captovix_license.devices set trial_until=clock_timestamp()-interval '1 second' where id=${tc.activationId}`;
 check(claims(await device(a,'refresh',{activationId:tc.activationId})).status==='revoked','Expired trial never renews as active');
 check((await action(tc.activationId,'extend-trial',{days:0})).status===400,'Invalid trial extension rejected');
 check((await action(tc.activationId,'extend-trial',{days:7})).status===200,'Manual audited trial extension works');
 check(claims(await device(a,'refresh',{activationId:tc.activationId})).status==='active','Extended trial can renew');
 await owner`delete from captovix_license.rate_buckets`;
 const created=await call('/admin/licenses',{customer:'Customer test',seats:1},true);check(created.status===200&&/^CVX-(?:[0-9A-F]{5}-){7}[0-9A-F]{5}$/.test(created.data.key),'Admin creates random activation key');const {id,key,recoveryCode}=created.data;
 const secrets=await owner`select key_hash,recovery_hash from captovix_license.licenses where id=${id}`;check(secrets[0].key_hash!==key&&secrets[0].recovery_hash!==recoveryCode,'Database stores credential hashes');
 check((await device(a,'activate',{key:'wrong'})).data.code==='KEY_INVALID_OR_REVOKED','Unknown key rejected');
 const active=await device(a,'activate',{key:' \t'+key+'\r\n'});const ac=claims(active);check(ac.kind==='perpetual'&&ac.status==='active','Paid activation returns active certificate');
 check(Date.parse(ac.leaseUntil)-Date.parse(ac.issuedAt)===30*86400000,'Perpetual lease defaults to 30 days');
 check((await device(b,'activate',{key})).data.code==='SEATS_FULL_USE_TRANSFER_OR_RECOVERY','Seat limit blocks another machine');
 check((await device(b,'refresh',{activationId:ac.activationId})).data.code==='DEVICE_NOT_FOUND','Activation ID remains bound to exact device proof');
 check((await device(a,'trial')).data.code==='USE_LICENSE_ACTIVATION_OR_RECOVERY','Paid history cannot evade restrictions via old trial');
 check(claims(await device(a,'release',{activationId:ac.activationId})).status==='released','User returns exact current machine slot');
 const newMachine=await device(b,'activate',{key});check(newMachine.status===200,'Freed seat can move to another machine');
 check((await device(a,'recover',{key,recoveryCode:'wrong'})).data.code==='RECOVERY_INVALID','Wrong recovery secret rejected');
 const recovered=await device(a,'recover',{key,recoveryCode});check(recovered.status===200&&recovered.data.recoveryCode!==recoveryCode,'Recovery activates new machine and rotates secret');
 check(claims(await device(b,'refresh',{activationId:claims(newMachine).activationId})).status==='released','Recovery releases every old seat');
 check((await device(b,'recover',{key,recoveryCode})).data.code==='RECOVERY_INVALID','Old recovery secret cannot be reused');
 check((await device(b,'recover',{key,recoveryCode:recovered.data.recoveryCode})).data.code==='RECOVERY_COOLDOWN','Recovery cooldown enforced by database');
 const current=claims(recovered).activationId;await action(id,'revoke');check(claims(await device(a,'refresh',{activationId:current})).status==='revoked','License revocation produces signed restricted state');await action(id,'restore');check(claims(await device(a,'refresh',{activationId:current})).status==='active','Restore renews paid access');
 await device(a,'release',{activationId:current});const parallel=await Promise.all([device(a,'activate',{key}),device(b,'activate',{key})]);check(parallel.filter(r=>r.status===200).length===1,'Concurrent requests cannot exceed one licensed seat');
 check((await action(id,'seats',{seats:0})).status===400,'Invalid seat count rejected');
 check((await call('/admin/licenses/'+id,{action:'revoke',reason:''},true)).status===400,'Mutation requires audit reason');
 const list=await call('/admin/licenses?query=Customer',null,true);const listText=JSON.stringify(list.data);check(list.status===200&&!listText.includes('key_hash')&&!listText.includes('recovery_hash')&&!listText.includes(key),'Admin listing excludes raw credentials and hashes');
 const audit=await call('/admin/audit',null,true);check(audit.data.some(r=>r.action==='license.created'&&r.actorId===adminId),'Audit records verified admin UUID');
 check(!JSON.stringify(audit.data).includes(key)&&!JSON.stringify(audit.data).includes(recoveryCode),'Audit does not store credentials from successful operations');
 check(await rpc('cvx_license_rate',{p_bucket:'quota-test',p_limit:1})===true&&await rpc('cvx_license_rate',{p_bucket:'quota-test',p_limit:1})===false,'Rate counters shared through PostgreSQL');
 check((await call('/admin/trials?query='+a.deviceId,null,true)).data.length===1,'Trial lookup by full machine fingerprint');
 // WebCrypto signature, then production .NET certificate verification.
 check(await crypto.subtle.verify({name:'RSA-PSS',saltLength:32},auth.keys.publicKey,Buffer.from(active.data.certificate.signature,'base64'),new TextEncoder().encode(active.data.certificate.payload)),'Edge certificate uses interoperable RSA-PSS salt length');
 if(process.env.DOTNET_EXE&&process.env.INTEROP_DLL){const folder=await mkdtemp(join(tmpdir(),'captovix-interop-'));try{const fixture=join(folder,'proof.json');let proc=spawnSync(process.env.DOTNET_EXE,[resolve(process.env.INTEROP_DLL),'emit',fixture],{encoding:'utf8'});assert.equal(proc.status,0,proc.stderr);const dotnet=JSON.parse(await readFile(fixture,'utf8'));const response=await call('/v1/trial',dotnet.proof);check(response.status===200,'.NET device proof accepted by production TypeScript handler');const path=join(folder,'reply.json');await writeFile(path,JSON.stringify({reply:response.data,publicKey:auth.publicPem,deviceId:dotnet.deviceId,publicKeyHash:dotnet.publicKeyHash}));proc=spawnSync(process.env.DOTNET_EXE,[resolve(process.env.INTEROP_DLL),'verify',path],{encoding:'utf8'});check(proc.status===0,'Production .NET verifier accepts TypeScript/SQL certificate');response.data.certificate.payload=Buffer.from('{}').toString('base64');await writeFile(path,JSON.stringify({reply:response.data,publicKey:auth.publicPem,deviceId:dotnet.deviceId,publicKeyHash:dotnet.publicKeyHash}));proc=spawnSync(process.env.DOTNET_EXE,[resolve(process.env.INTEROP_DLL),'verify',path],{encoding:'utf8'});check(proc.status!==0,'Production .NET verifier rejects edited serverless certificate')}finally{await rm(folder,{recursive:true,force:true})}}else console.log('CROSS_LANGUAGE_PENDING: provide DOTNET_EXE and INTEROP_DLL');
 await owner`delete from captovix_license.admins where user_id=${adminId}`;check((await call('/admin/licenses',null,true)).status===403,'Removing admin membership blocks existing session immediately');
 console.log(`${n} serverless SQL/handler integration checks passed. Auth transport is a test fixture; hosted Supabase/Cloudflare/Windows runtime still require acceptance.`);
}finally{await sql.end();await owner.end()}
