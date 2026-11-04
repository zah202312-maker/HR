// Applies the owner's approved decisions (2026-11-04) from audit/01-coverage.md and QUESTIONS.md.
import { createRequire } from 'module'; import fs from 'fs';
const [deps,root]=process.argv.slice(2); const y=createRequire(deps+'/package.json')('js-yaml');
const d=y.load(fs.readFileSync(root+'/openapi.yaml','utf8')); const s=d.components.schemas; const METH=['get','post','put','patch','delete'];
const idx=()=>{const o={};for(const[p,i] of Object.entries(d.paths))for(const k of METH)if(i[k])o[i[k].operationId]={p,k,op:i[k]};return o;}; const ops=idx(); const miss=[];
const O=id=>{const e=ops[id]; if(!e){miss.push(id);return null;} return e.op;};
const note=(ids,t)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o.description=(o.description||'')+'\n\n'+t;});
const addRoles=(ids,rs)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o['x-allowed-roles']=[...new Set([...o['x-allowed-roles'],...rs])];});
const delRoles=(ids,rs)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o['x-allowed-roles']=o['x-allowed-roles'].filter(r=>!rs.includes(r));});

// approved defaults, one per open question
const t=fs.readFileSync(root+'/audit/01-coverage.md','utf8');
const sec=t.slice(t.indexOf('### NEEDS-OWNER'),t.indexOf('## Appendix'));
const rows=sec.split('\n').filter(l=>/^\| \d+ \|/.test(l)).map(l=>{const c=l.split('|').map(x=>x.trim()); return {n:+c[1],op:c[2].replace(/`/g,''),def:c[4]};});
for(const r of rows){ const o=O(r.op); if(!o) continue; const def=r.n===4?'يستطيع مسؤول النظام إنشاء أدوار جديدة وتعديل الأدوار وصلاحياتها، ضمن الصلاحيات غير المحمية.':r.def; o.description+=`\n\n**قرار معتمد:** ${def}`; delete o['x-open-question']; }

// Q4: custom roles allowed
note('Role_Create','لا يمكن إسناد الصلاحيات المحمية إلى دور مخصص، وتبقى الأدوار الثمانية الأساسية موجودة دائمًا.');
// Q5: employee number
for(const n of ['EmployeeCreateDto','EmployeeUpdateDto','EmployeeHrDto','EmployeeSelfDto']) s[n].properties.employeeNumber={type:'string',maxLength:32,nullable:true,description:'الرقم الوظيفي.'};
// Q22, COV-5
note(['Request_Approve','Request_GetInbox'],'استثناء من منع الاعتماد الذاتي: يجوز للمحاسب أو مسؤول المخزن تنفيذ طلبه عند pendingDestination بعد موافقة الموارد البشرية، ويُسجل كتنفيذ وليس اعتمادًا.');
// Q43
delRoles('EvaluationInternalNote_Add',['hrManager']);
// Q44
for(const i of ['EvaluationGrade_Create','EvaluationGrade_Update']){const o=O(i); if(o)o['x-allowed-roles']=['systemAdmin'];}
// Q45, Q46
addRoles('Report_Export',['reviewer','hrManager']); addRoles('Circular_Create',['executiveDirector']);
note('Circular_Create','يرسل المدير التنفيذي إلى مديري الأقسام وكل الشركة. وتأكيد الاستلام مطلوب لكل التعاميم.');
// Q57
{const o=O('PayrollRun_Close'); if(o)o['x-allowed-roles']=['accountant'];}
// Q56: return for changes
{const a=ops['PayrollRun_Approve']; const base=a.p.replace(/\/approve$/,'');
 d.paths[base+'/return']={post:{operationId:'PayrollRun_ReturnForChanges',tags:a.op.tags,summary:'Return a payroll run to the accountant',description:'يعيد المعتمد الكشف إلى المحاسب مع سبب، ويبقى بالحالة draft.','x-source':['5.16'],'x-release':3,'x-allowed-roles':a.op['x-allowed-roles'],'x-permission':a.op['x-permission'],'x-data-scope':a.op['x-data-scope'],'x-error-codes':['Hr:Finance:ApprovalNotRequested'],'x-added-route':'تصحيح خطأ يجده المعتمد في الكشف',parameters:[{$ref:'#/components/parameters/Id'},{$ref:'#/components/parameters/AcceptLanguage'}],requestBody:{required:true,content:{'application/json':{schema:{$ref:'#/components/schemas/ReasonInputDto'},example:{reason:'يوجد خطأ في استقطاع أحد الموظفين.'}}}},responses:{200:a.op.responses['200'],400:{$ref:'#/components/responses/BadRequest'},401:{$ref:'#/components/responses/Unauthorized'},403:{$ref:'#/components/responses/Forbidden'},404:{$ref:'#/components/responses/NotFound'},422:{$ref:'#/components/responses/BusinessRuleViolation'}}}};}

// extra decisions: a (SEC-5)
for(const[n,v] of Object.entries(s)) if(/^Request/.test(n)&&v.properties?.detailsSummary){ v.required=(v.required||[]).filter(x=>!['detailsSummary','description'].includes(x)); if(!v.required.length) delete v.required; }
note(['Request_Get','Request_GetList','Request_GetInbox','Request_Approve','Request_Reject'],'للطلبات الموجهة إلى المحاسب: مدير القسم ومفوضه ومسؤول النظام يرون النوع فقط، وتُحذف الحقول description وdetailsSummary وملاحظات الخطوات من الرد. التفاصيل للموارد البشرية والمحاسب وصاحب الطلب.');
// b (SEC-14)
note(['ComplaintHandler_Approve','Mfa_ConfirmEnrollment'],'التعيين المعتمد لمعالجة الشكاوى يجعل التحقق بخطوتين إلزاميًا لصاحبه.');
// c (SEC-31)
note(['UserAccount_Create','Employee_Create'],'كلمة السر الأولى مؤقتة وتغييرها إجباري عند أول دخول. يُسجل أول دخول، ويُشعر الموظف بأي نشاط سبق تغييرها.');
// e (COV-26) and f (COV-27) keep current contract; d (COV-24) keeps attachments optional
note('Warning_Create','المستندات تُرفق إن وجدت، وليست إلزامية.');

fs.writeFileSync(root+'/openapi.yaml',y.dump(d,{lineWidth:120,noRefs:true,sortKeys:false}));
const oq=[];let n=0;for(const[p,i] of Object.entries(d.paths))for(const k of METH)if(i[k]){n++; if(i[k]['x-open-question'])oq.push({id:`${k.toUpperCase()} ${p}`,opId:i[k].operationId,q:i[k]['x-open-question']});}
fs.writeFileSync(root+'/open-questions.json',JSON.stringify(oq,null,1));
console.log('ops',n,'open left',oq.length,oq.map(x=>x.opId).join(','),'missing',miss.join(',')||'none');
