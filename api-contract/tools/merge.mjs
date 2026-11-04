import { createRequire } from 'module';
import fs from 'fs'; import path from 'path';
const [deps, root] = process.argv.slice(2);
const yaml = createRequire(deps + '/package.json')('js-yaml');
const dir = path.join(root,'modules');
const order=['common','identity-org','time-leave','workflow-store','complaints-evaluation','hr-finance-system'];
const docs={}; const issues=[];
for (const m of order){ const f=path.join(dir,m+'.yaml'); const st=fs.statSync(f); docs[m]=yaml.load(fs.readFileSync(f,'utf8')); console.log('loaded',m,(st.size/1024|0)+'KB',Object.keys(docs[m].paths||{}).length,'paths',Object.keys(docs[m].components?.schemas||{}).length,'schemas'); }
const METH=['get','post','put','patch','delete'];
const out={openapi:'3.0.3',info:{title:'HR System API',version:'1.0.0',description:'Contract generated from متطلبات_وتصميم_النظام.md. See api-contract/README.md.'},servers:[{url:'https://localhost:44382',description:'Local development'}],security:[{oauth2:['openid','profile','offline_access','HR_Mangment2']}],tags:[],paths:{},components:{securitySchemes:{},parameters:{},responses:{},schemas:{}}};
const owner={};
const rewrite=o=>{ if(Array.isArray(o)) return o.map(rewrite); if(o&&typeof o==='object'){ const r={}; for(const[k,v] of Object.entries(o)){ if(k==='$ref'&&typeof v==='string'){ const i=v.indexOf('#'); r[k]='#'+v.slice(i+1);} else r[k]=rewrite(v);} return r;} return o; };
const seenTags=new Set(); const opIds={}; 
for(const m of order){ const d=docs[m];
  for(const t of d.tags||[]) if(!seenTags.has(t.name)){seenTags.add(t.name);out.tags.push(t);}
  for(const sec of ['securitySchemes','parameters','responses','schemas']) for(const[n,v] of Object.entries(d.components?.[sec]||{})){ const key=sec+'/'+n; if(out.components[sec][n]){ const same=JSON.stringify(rewrite(v))===JSON.stringify(out.components[sec][n]); issues.push(`DUP_COMPONENT ${key} ${owner[key]} vs ${m} ${same?'(identical)':'(DIFFERENT)'}`); } else { out.components[sec][n]=rewrite(v); owner[key]=m; } }
  for(const[p,item] of Object.entries(d.paths||{})){ out.paths[p]??={}; for(const[k,v] of Object.entries(item)){ if(METH.includes(k)&&out.paths[p][k]) issues.push(`DUP_ROUTE ${k.toUpperCase()} ${p} ${out.paths[p][k]['x-module']} vs ${m}`); else { const nv=rewrite(v); if(METH.includes(k)) nv['x-module']=m; out.paths[p][k]=nv; } } }
}

// --- post-processing ---
const R1TAGS=new Set(['WorkShift','Holiday','Attendance','AttendanceDevice','AttendanceImport','AttendanceSettings','AttendanceCorrection','LeaveBalance','LeaveRequest','OtherLeaveType','LeaveCalendar','Delegations']);
for(const it of Object.values(out.paths)) for(const k of METH) if(it[k]&&R1TAGS.has((it[k].tags||[])[0])) it[k]['x-release']=1;
const typeOfRef=r=>{const s=out.components.schemas[r.split('/').pop()]; if(!s) return 'object'; if(s.type) return s.type; if(s.allOf) for(const a of s.allOf){ if(a.$ref) return typeOfRef(a.$ref); if(a.type) return a.type;} return 'object';};
let nullFixed=0; (function w(o){ if(Array.isArray(o)) return o.forEach(w); if(o&&typeof o==='object'){ if(o.nullable===true&&!o.type){ const a=(o.allOf||[]).find(x=>x.$ref); o.type=a?typeOfRef(a.$ref):'object'; nullFixed++; } Object.values(o).forEach(w);} })(out);
console.log('nullable types added',nullFixed);
const ops=[]; for(const[p,it] of Object.entries(out.paths)) for(const k of METH) if(it[k]) ops.push({p,k,op:it[k]});
const roles=new Set(out.components.schemas.AppRole.enum); const rel={}; const oq=[];
for(const {p,k,op} of ops){ const id=`${k.toUpperCase()} ${p}`;
  if(!op.operationId) issues.push('NO_OPID '+id); else if(opIds[op.operationId]) issues.push(`DUP_OPID ${op.operationId} ${id} & ${opIds[op.operationId]}`); else opIds[op.operationId]=id;
  for(const x of ['x-source','x-release','x-allowed-roles','x-permission','x-data-scope','x-error-codes']) if(op[x]===undefined) issues.push(`MISSING ${x} ${id}`);
  for(const r of op['x-allowed-roles']||[]) if(!roles.has(r)) issues.push(`BAD_ROLE ${r} ${id}`);
  rel[op['x-release']]=(rel[op['x-release']]||0)+1;
  if(op['x-open-question']) oq.push({id,opId:op.operationId,q:op['x-open-question']});
  const has422=!!op.responses?.['422']; const ec=(op['x-error-codes']||[]).length; if(ec&&!has422) issues.push('CODES_NO_422 '+id);
}
// refs
const refs=new Set(); (function w(o){ if(Array.isArray(o)) o.forEach(w); else if(o&&typeof o==='object'){ if(typeof o.$ref==='string') refs.add(o.$ref); Object.values(o).forEach(w);} })(out);
for(const r of refs){ const parts=r.replace('#/','').split('/'); let c=out; for(const x of parts) c=c?.[x]; if(c===undefined) issues.push('BROKEN_REF '+r); }
const used=new Set([...refs].map(r=>r.split('/').pop())); const unused=Object.keys(out.components.schemas).filter(n=>!used.has(n));
fs.writeFileSync(path.join(root,'openapi.yaml'),yaml.dump(out,{lineWidth:120,noRefs:true,sortKeys:false}));
fs.writeFileSync(path.join(root,'open-questions.json'),JSON.stringify(oq,null,1));
console.log('operations',ops.length,'paths',Object.keys(out.paths).length,'schemas',Object.keys(out.components.schemas).length);
console.log('release',JSON.stringify(rel)); console.log('open questions',oq.length,'unused schemas',unused.length, unused.slice(0,20).join(','));
const c={}; for(const i of issues){const t=i.split(' ')[0];c[t]=(c[t]||0)+1;} console.log('issue counts',JSON.stringify(c)); issues.slice(0,60).forEach(i=>console.log(' ',i));
