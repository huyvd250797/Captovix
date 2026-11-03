// One file, zero third-party runtime dependencies: usable in Dashboard editor.
// verify_jwt=false is intentional. Desktop routes authenticate RSA device proof;
// admin routes authenticate Supabase Auth AND the private admin allowlist.
type Env = Record<string, string | undefined>;
type Json = Record<string, any>;
type Rpc = (name: string, input?: Json) => Promise<any>;
declare const Deno: {env: {toObject(): Env}; serve(handler: (r: Request) => Promise<Response>): void};
const enc = new TextEncoder();
const hex = (b: ArrayBuffer | Uint8Array) => [...new Uint8Array(b instanceof Uint8Array ? b.buffer.slice(b.byteOffset,b.byteOffset+b.byteLength) as ArrayBuffer : b)].map(x=>x.toString(16).padStart(2,"0")).join("");
const bytes = (value: string) => Uint8Array.from(atob(value), c=>c.charCodeAt(0));
const base64 = (value: ArrayBuffer | Uint8Array) => btoa(String.fromCharCode(...new Uint8Array(value instanceof Uint8Array ? value : value)));
export const hash = async (value: string) => hex(await crypto.subtle.digest("SHA-256",enc.encode(value)));
export class Fault extends Error { code: string; status: number; constructor(code: string,status = 400){super(code);this.code=code;this.status=status;} }
function fail(code: string,status = 400): never {throw new Fault(code,status);}
const codes = new Set(["REQUEST_REPLAY","REQUEST_INVALID","USE_LICENSE_ACTIVATION_OR_RECOVERY","TRIAL_ALREADY_STARTED_CONTACT_SUPPORT","KEY_INVALID_OR_REVOKED","RECOVERY_INVALID","RECOVERY_COOLDOWN","SEATS_FULL_USE_TRANSFER_OR_RECOVERY","DEVICE_NOT_FOUND","TRIAL_CANNOT_TRANSFER","ADMIN_FORBIDDEN","ADMIN_INPUT_INVALID","REASON_REQUIRED","RELEASE_SEATS_FIRST","NOT_FOUND","ADMIN_ACTION_INVALID"]);
function text(value: unknown,max: number,min=0): value is string {return typeof value==="string"&&value.length>=min&&value.length<=max;}
function policy(value: string | undefined,fallback: number,max: number){const n=Number(value??fallback);if(!Number.isInteger(n)||n<1||n>max)fail("CONFIG_INVALID",503);return n;}
function pem(value: string,label: string){const pattern=new RegExp(`^-----BEGIN ${label}-----[\\s\\S]*-----END ${label}-----\\s*$`);if(!pattern.test(value))fail("PROOF_INVALID");return bytes(value.replace(new RegExp(`-----[^-]+-----`,"g"),"").replace(/\s/g,""));}
export async function verifyProof(p: Json,operation: string,now=Date.now()): Promise<{input: Json;publicHash: string}> {
  if(!p||!text(p.payload,4096,1)||!text(p.publicKey,1200,1)||!text(p.signature,1500,1)||!text(p.nonce,48,48)||!/^[0-9a-f]{48}$/i.test(p.nonce)||!text(p.timestamp,64,1))fail("REQUEST_INVALID_OR_CLOCK");
  const time=Date.parse(p.timestamp);if(!Number.isFinite(time)||Math.abs(now-time)>300000)fail("REQUEST_INVALID_OR_CLOCK");
  let key: CryptoKey;try{key=await crypto.subtle.importKey("spki",pem(p.publicKey,"PUBLIC KEY"),{name:"RSA-PSS",hash:"SHA-256"},false,["verify"]);}catch{fail("PROOF_INVALID");}
  const length=(key!.algorithm as RsaHashedKeyAlgorithm).modulusLength;if(length<2048||length>4096)fail("PROOF_INVALID");
  const signed=enc.encode(`captovix-license-v1\n${operation}\n${Math.floor(time/1000)}\n${p.nonce}\n${await hash(p.payload)}`);
  let ok=false;try{ok=await crypto.subtle.verify({name:"RSA-PSS",saltLength:32},key!,bytes(p.signature),signed);}catch{}if(!ok)fail("PROOF_INVALID");
  let input: Json={};try{input=JSON.parse(p.payload);}catch{fail("REQUEST_INVALID");}
  if(!input||typeof input!=="object"||Array.isArray(input))fail("REQUEST_INVALID");
  input={...input,key:input.key??"",recoveryCode:input.recoveryCode??"",activationId:input.activationId??""};
  if(!text(input.deviceId,64,64)||!/^[0-9a-f]{64}$/i.test(input.deviceId)||!text(input.deviceName,80,1)||!text(input.key,128)||!text(input.recoveryCode,128)||!text(input.activationId,32)||![0,32].includes(input.activationId.length))fail("REQUEST_INVALID");
  input.key=input.key.trim();input.recoveryCode=input.recoveryCode.trim();
  return {input:input,publicHash:await hash(p.publicKey)};
}
export async function importSigner(b64: string){
  try{const key=await crypto.subtle.importKey("pkcs8",bytes(b64),{name:"RSA-PSS",hash:"SHA-256"},false,["sign"]);if((key.algorithm as RsaHashedKeyAlgorithm).modulusLength<3072)fail("CONFIG_INVALID",503);return key;}catch{fail("CONFIG_INVALID",503);}
}
export async function signReply(key: CryptoKey,claims: Json,issuer: string,recoveryCode: string|null=null){
  const payload=base64(enc.encode(JSON.stringify({...claims,issuer})));
  const signature=base64(await crypto.subtle.sign({name:"RSA-PSS",saltLength:32},key!,enc.encode(payload)));
  return {certificate:{payload,signature},recoveryCode};
}
// Injectable transport allows contract/DB integration tests to execute this exact
// production handler without mocking entitlement decisions or using Docker.
export function createHandler(env: Env, fetcher: typeof fetch=fetch, overrideRpc?: Rpc) {
  const url=env.SUPABASE_URL?.replace(/\/$/,"")??"";
  const service=env.SUPABASE_SERVICE_ROLE_KEY??"";
  const origins=(env.ADMIN_ORIGINS??"").split(",").map(s=>s.trim()).filter(Boolean);
  let signerPromise: Promise<CryptoKey>|undefined;
  const signer=()=>signerPromise??=importSigner(env.LICENSE_SIGNING_KEY_B64??"");
  const rpc: Rpc=overrideRpc??(async(name,input={})=>{
    const response=await fetcher(url+"/rest/v1/rpc/"+name,{method:"POST",headers:{apikey:service,Authorization:"Bearer "+service,"Content-Type":"application/json"},body:JSON.stringify(input),signal:AbortSignal.timeout(12000)});
    const data=await response.json();if(!response.ok){if(codes.has(data.message))fail(data.message,data.message==="ADMIN_FORBIDDEN"?403:400);fail("SERVICE_UNAVAILABLE",503);}return data;
  });
  async function admin(request: Request){
    const auth=request.headers.get("authorization")??"";if(!/^Bearer [A-Za-z0-9_.-]+$/.test(auth)||auth.length>8192)fail("ADMIN_UNAUTHORIZED",401);
    const response=await fetcher(url+"/auth/v1/user",{headers:{apikey:service,Authorization:auth},signal:AbortSignal.timeout(10000)});
    if(response.status===401||response.status===403)fail("ADMIN_UNAUTHORIZED",401);if(!response.ok)fail("SERVICE_UNAVAILABLE",503);
    const user=await response.json();if(!text(user.id,36,36)||!await rpc("cvx_license_admin_allowed",{p_actor:user.id}))fail("ADMIN_FORBIDDEN",403);
    if(!await rpc("cvx_license_rate",{p_bucket:"admin:"+user.id,p_limit:30}))fail("RATE_LIMIT",429);return user.id;
  }
  async function body(request: Request){
    if(!request.headers.get("content-type")?.toLowerCase().startsWith("application/json"))fail("JSON_REQUIRED",415);
    // Bound actual bytes, not just a client-controlled Content-Length header.
    if(!request.body)fail("REQUEST_INVALID");const reader=request.body.getReader();let size=0;const parts: Uint8Array[]=[];
    try{while(true){const {value,done}=await reader.read();if(done)break;size+=value.length;if(size>16384){await reader.cancel();fail("REQUEST_TOO_LARGE",413);}parts.push(value);}}finally{reader.releaseLock();}
    const joined=new Uint8Array(size);let at=0;for(const part of parts){joined.set(part,at);at+=part.length;}try{const data=JSON.parse(new TextDecoder().decode(joined));if(!data||Array.isArray(data)||typeof data!=="object")fail("REQUEST_INVALID");return data;}catch{fail("REQUEST_INVALID");}
  }
  return async(request: Request): Promise<Response>=>{
    const origin=request.headers.get("origin");const allowed=origin!==null&&origins.includes(origin);
    const headers: Record<string,string>={"Content-Type":"application/json; charset=utf-8","Cache-Control":"no-store","X-Content-Type-Options":"nosniff","Vary":"Origin"};
    if(allowed){headers["Access-Control-Allow-Origin"]=origin!;headers["Access-Control-Allow-Headers"]="authorization,content-type,apikey";headers["Access-Control-Allow-Methods"]="GET,POST,OPTIONS";headers["Access-Control-Max-Age"]="600";}
    const respond=(value: unknown,status=200)=>new Response(status===204?null:JSON.stringify(value),{status,headers});
    try{
      if(origin&&!allowed)fail("ORIGIN_FORBIDDEN",403);
      if(request.method==="OPTIONS")return respond(null,204);
      const path=new URL(request.url).pathname.replace(/^\/(?:functions\/v1\/)?licensing(?=\/|$)/,"")||"/";
      if(path==="/health"&&request.method==="GET")return respond({service:"Captovix Serverless Licensing",version:"1.29.1"});
      if(!/^https:\/\//.test(url)||!service||!env.LICENSE_ISSUER||env.LICENSE_ISSUER.length>100)fail("CONFIG_INVALID",503);
      if(path==="/ready"&&request.method==="GET"){await signer();return respond(await rpc("cvx_license_ready"));}
      if(!await rpc("cvx_license_rate",{p_bucket:"global",p_limit:600}))fail("RATE_LIMIT",429);
      const operation=/^\/v1\/(trial|activate|refresh|release|recover)$/.exec(path)?.[1];
      if(operation){
        if(request.method!=="POST")fail("METHOD_NOT_ALLOWED",405);
        const key=await signer();const proof=await body(request);const verified=await verifyProof(proof,operation);
        if(!await rpc("cvx_license_rate",{p_bucket:"device:"+verified.publicHash,p_limit:30}))fail("RATE_LIMIT",429);
        const data=await rpc("cvx_license_operation",{p_op:operation,p_input:verified.input,p_public_hash:verified.publicHash,p_nonce:proof.nonce,p_days:policy(env.PERPETUAL_LEASE_DAYS,30,30),p_cooldown:policy(env.RECOVERY_COOLDOWN_HOURS,72,720)});
        return respond(await signReply(key!,data.claims,env.LICENSE_ISSUER,data.recoveryCode??null));
      }
      if(path.startsWith("/admin/")){
        const actor=await admin(request);const query=new URL(request.url).searchParams;
        const page=Number(query.get("page")??0);if(!Number.isInteger(page)||page<0||page>100000)fail("ADMIN_INPUT_INVALID");
        let input: Json={query:query.get("query")??"",page};if(input.query.length>200)fail("ADMIN_INPUT_INVALID");let action="";
        if(request.method==="GET")action=({"/admin/licenses":"list","/admin/trials":"trials","/admin/audit":"audit","/admin/me":"me"} as Json)[path]??"";
        else if(request.method==="POST"){
          input={...input,...await body(request)};
          if(path==="/admin/licenses")action="create";
          else {const match=/^\/admin\/licenses\/([a-f0-9]{32})$/.exec(path);if(match){input.id=match[1];action=input.action;}}
          if(action==="create"&&(!text(input.customer,200,1)||!Number.isInteger(input.seats)||input.seats<1||input.seats>100))fail("ADMIN_INPUT_INVALID");
          if(action!=="create"&&(!text(input.reason,300,1)||!text(action,40,1)))fail("ADMIN_INPUT_INVALID");
        }
        if(action==="me")return respond({userId:actor});if(!action)fail("NOT_FOUND",404);
        // SQL independently checks membership again, including on mutations.
        return respond(await rpc("cvx_license_admin",{p_actor:actor,p_action:action,p_input:input}));
      }
      fail("NOT_FOUND",404);
    }catch(e){const fault=e instanceof Fault?e:new Fault("SERVICE_UNAVAILABLE",503);if(fault.status===429)headers["Retry-After"]="60";return respond({code:fault.code},fault.status);}
  };
}
if(typeof Deno!=="undefined")Deno.serve(createHandler(Deno.env.toObject()));
