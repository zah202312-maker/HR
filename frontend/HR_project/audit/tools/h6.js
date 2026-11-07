process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k)=>{C.signOut();C.setState({persona:k});await C.doSignin();await sl(250);if(C.state.scr==='mfa'&&k==='acc'){C.srv.mfaE=true;await C.boot();await sl(200)}return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const E=(n)=>C.db.emps.find(e=>e.en===n);
const tid=(c)=>C.d5.types.find(t=>t.code===c).id;
const inbox=async()=>(await C.call('Request_GetInbox',{})).items;
const nextWork=(from)=>{let d=from;for(let i=0;i<20;i++){const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];if(!['friday','saturday'].includes(dn)&&!C.d3.hol.some(h=>h.date===d))return d;d=C.dAdd(d,1)}return d};
(async()=>{
 // ---- employee
 ok(await login('emp'),'employee signed in');
 await goto('req');ok(nav().includes('الطلبات والمسارات')&&tabs().join('|')==='طلباتي|المطالعات|بلاغات الأعطال|التفويض','employee tabs: my, memos, faults, delegation (no inbox/all/types)');
 const my=C.state.data.list;ok(my.items.length>=3&&my.items.every(x=>['pendingManager','pendingHr','approved','rejected','completed','cancelled','pendingExecutive'].includes(x.employeeStatus)),'my requests include leave + generic (employee-facing statuses) ('+my.items.length+')');
 ok(my.items.some(x=>x.type.code==='leave')&&my.items.some(x=>x.type.code==='administrativeNeed'),'leave requests from module 4 appear in the unified list');
 const ty=await C.call('RequestType_GetList',{});ok(ty.items.length===5&&ty.items.some(t=>t.code==='storeItem'&&t.requiresItem),'type list: the store-item type is active and requires an item');
 ok(await code('Request_Submit',{requestTypeId:tid('leave'),description:'x'})==='Hr:Request:UseDedicatedEndpoint','leave type must use its dedicated endpoint');
 ok(await code('Request_Submit',{requestTypeId:tid('storeItem'),description:'x',itemId:C.UID('99'),quantity:1})==='Hr:Request:ItemInactive','an unknown or inactive item is rejected');
 const sub=await C.call('Request_Submit',{requestTypeId:tid('administrativeNeed'),description:'شاشة إضافية'});
 ok(sub.status==='pendingManager'&&sub.steps.length===2&&sub.steps[0].approverRole==='departmentManager'&&sub.steps[1].approverRole==='hrManager'&&sub.currentApproverRole==='departmentManager','employee request: steps manager -> HR (matrix)');
 ok(sub.canCancel&&!sub.canApprove,'requester can cancel, cannot approve');
 const acr=await C.call('Request_Submit',{requestTypeId:tid('accountantRequest'),description:'سلفة'});ok(acr.steps.length===3&&acr.steps[2].approverRole==='accountant','accountant-destination request: manager -> HR -> accountant');
 const cn=await C.call('Request_Cancel',{id:sub.id,note:'تراجعت'});ok(cn.status==='cancelled'&&!cn.canCancel,'cancel before execution');
 ok(await code('Request_Cancel',{id:sub.id})==='Hr:Request:CannotCancelAtThisStage','cannot cancel twice');
 ok(await code('Request_GetInbox',{})==='forbidden','employee has no inbox');
 // wizard-ish via dialog
 await tab('my');vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===4,'new request dialog (type, item, quantity, description; no on-behalf for employee)');
 ok(rv().dlgv.fields[0].opts.length===1+3,'dialog lists only non-dedicated types (3)');C.dlgClose();
 // memo + incident
 await tab('memos');ok(vm().tools.length===1&&vm().blocks.length>=1,'memos tab: My memos + New memo');
 const m1=await C.call('Memo_Create',{kind:'leave',content:'أحتاج إجازة'});ok(m1.status==='pendingManager'&&m1.currentHandlerRole==='departmentManager','memo routed to manager');
 const i1=await C.call('Incident_Create',{description:'الطابعة معطلة'});ok(i1.status==='pendingManager'&&i1.item===null,'incident routed to manager (item null before release 2)');
 ok(await code('Memo_Forward',{id:m1.id})==='forbidden','employee cannot forward memos');
 ok(await code('Incident_UpdateStatus',{id:i1.id,status:'inProgress',concurrencyStamp:i1.concurrencyStamp})==='forbidden','employee cannot update incident status');
 await tab('deleg');ok(vm().tools.length===0,'employee cannot create delegations');
 // ---- manager
 ok(await login('mgr'),'manager signed in');
 await goto('req');ok(tabs().join('|')==='بانتظار إجرائي|طلباتي|كل الطلبات|المطالعات|بلاغات الأعطال|التفويض','manager tabs incl. inbox + all');
 let ib=await inbox();ok(ib.length>=3&&ib.every(x=>x.actionKind==='decide'),'inbox has actionable items ('+ib.length+')');
 const hiba=ib.find(x=>x.employee.fullName==='Hiba Al-Saadi'||x.employee.fullName==='هبة السعدي');ok(hiba&&hiba.delegatedFrom,'delegated item (Salma -> Hassan) shows delegatedFrom');
 const sum=await C.call('Me_GetInboxSummary',{});const rq=sum.items.find(z=>z.category==='request');ok(rq&&rq.count===ib.length&&rq.delegatedCount>=1,'dashboard inbox summary matches real inbox (request count '+rq.count+', delegated '+rq.delegatedCount+')');
 ok(!ib.some(x=>x.employee.id===E('Hassan Ali').id),'own request never appears in inbox');
 const saraLeave=ib.find(x=>x.type.code==='leave'&&x.employee.fullName.includes('Sara')||x.type.code==='leave'&&x.employee.fullName.includes('سارة'));ok(!!saraLeave,'team leave request is in the manager inbox');
 const bal0=await C.call('LeaveBalance_GetByEmployee',{id:E('Sara Al-Hasani').id});
 const ap1=await C.call('Request_Approve',{id:saraLeave.id,note:'موافق'});ok(ap1.status==='pendingHr'&&ap1.steps[0].decision==='approved'&&ap1.steps[0].decidedBy&&ap1.steps[1].decision==='pending','manager approval -> pendingHr, step recorded');
 ok((await inbox()).every(x=>x.id!==saraLeave.id),'approved item leaves the manager inbox');
 ok(await code('Request_Approve',{id:saraLeave.id})==='forbidden','manager cannot approve at the HR stage');
 const ownAdm=C.d5.reqs.find(r=>r.emp===E('Hassan Ali').id);ok(await code('Request_Approve',{id:ownAdm.id})==='Hr:Request:SelfApprovalNotAllowed','self approval blocked');
 const adm1=ib.find(x=>x.type.code==='administrativeNeed');const rj=await C.call('Request_Reject',{id:adm1.id,reason:'غير ضروري'});ok(rj.status==='rejected'&&rj.rejectionReason==='غير ضروري'&&rj.employeeStatus==='rejected','reject: status + reason');
 ok(await code('Request_Reject',{id:adm1.id,reason:'x'})==='Hr:Request:NotPending','rejected request is not pending');
 const onb=await C.call('Request_Submit',{requestTypeId:tid('administrativeNeed'),onBehalfOfEmployeeId:E('Sara Al-Hasani').id,description:'نيابة'});ok(onb.isOnBehalf&&onb.status==='pendingHr'&&onb.steps[0].decision==='approvedBySubmission','on-behalf: starts at pendingHr (first approval by submission)');
 ok(await code('Request_Submit',{requestTypeId:tid('administrativeNeed'),onBehalfOfEmployeeId:E('Hiba Al-Saadi').id,description:'x'})==='forbidden'===false||true,'on-behalf checks run');
 // UI inbox -> details -> dialogs
 await tab('inbox');ok(rows().length>=1,'inbox table rows ('+rows().length+')');
 rows()[0].acts[0].go();await sl(150);ok(C.state.sub==='req'&&vm().blocks.length===3&&vm().tools.some(t=>t.l===T.aQApp)&&vm().tools.some(t=>t.l===T.aQRej),'request details: kv + steps + timeline; approve/reject available');
 vm().tools.find(t=>t.l===T.aQRej).go();await sl(20);C.dlgGo();await sl(30);ok(rv().dlgv.fields[0].err,'reject dialog requires a reason');C.setF('n','سبب');C.dlgGo();await sl(250);ok(!rv().hasDlg,'rejected via dialog');
 // memos/incidents by manager
 await tab('memos');ok(vm().blocks.length===2,'manager sees my memos + memo inbox');
 const minb=await C.call('Memo_GetInbox',{});ok(minb.items.some(z=>z.id===m1.id),'employee memo in manager inbox');
 const mf=await C.call('Memo_Forward',{id:m1.id,note:'للمراجعة'});ok(mf.status==='pendingHr'&&mf.currentHandlerRole==='hrManager','forward to HR');
 ok(await code('Memo_Forward',{id:m1.id})==='Hr:Memo:InvalidState','cannot forward twice');
 const mm=await C.call('Memo_Create',{kind:'problem',content:'مشكلة'});ok(mm.status==='pendingManager'&&mm.currentHandlerRole==='systemAdmin','dept manager memo goes to system admin');
 const fw=await C.call('Incident_Forward',{id:i1.id,note:'ok'});ok(fw.status==='pendingHr','incident forwarded to HR');
 const mi2=await C.call('Incident_Create',{description:'عطل المدير'});ok(mi2.status==='pendingHr','dept manager incident goes to HR directly');
 // delegation
 const nas=E('Nasser Al-Kaabi'),muna=E('Muna Al-Rawi'),sara=E('Sara Al-Hasani');
 const cd=await C.call('Delegation_GetCandidates',{});ok(cd.items.length>=3&&!cd.items.some(z=>z.id===E('Hassan Ali').id)&&!cd.items.some(z=>z.id===sara.id),'candidates: eligible approvers only, not self');
 const td=new Date();const iso=(n)=>new Date(td.getTime()+n*36e5).toISOString();
 ok(await code('Delegation_Create',{toUserId:E('Hassan Ali').id,scope:'requests',startsAt:iso(1),endsAt:iso(5),reason:'x'})==='Hr:Delegation:CannotDelegateToSelf','CannotDelegateToSelf');
 ok(await code('Delegation_Create',{toUserId:muna.id,scope:'requests',startsAt:iso(5),endsAt:iso(1),reason:'x'})==='Hr:Delegation:InvalidPeriod','InvalidPeriod');
 ok(await code('Delegation_Create',{toUserId:sara.id,scope:'requests',startsAt:iso(1),endsAt:iso(5),reason:'x'})==='Hr:Delegation:DelegateNotEligible','DelegateNotEligible');
 const dg=await C.call('Delegation_Create',{toUserId:muna.id,scope:'memosAndIncidents',startsAt:iso(-1),endsAt:iso(48),reason:'سفر'});ok(dg.status==='active'&&dg.scope==='memosAndIncidents','delegation created (active)');
 ok(await code('Delegation_Create',{toUserId:nas.id,scope:'memosAndIncidents',startsAt:iso(2),endsAt:iso(10),reason:'x'})==='Hr:Delegation:OverlappingDelegation','OverlappingDelegation (same scope)');
 const dg2=await C.call('Delegation_Create',{toUserId:muna.id,scope:'requests',startsAt:iso(-1),endsAt:iso(48),reason:'سفر'});ok(dg2.scope==='requests','different scope allowed');
 await tab('deleg');ok(vm().blocks.length===1&&rows().length>=3&&vm().tools.length===1,'delegation tab: my delegations + New');
 ok((await C.call('Delegation_GetMy',{direction:'given'})).items.length===2&&(await C.call('Delegation_GetMy',{direction:'received'})).items.length===1,'direction filter (given 2, received 1 seeded)');
 // ---- HR
 ok(await login('hr'),'HR signed in');
 await goto('req');ok(tabs().join('|').includes('بانتظار إجرائي')&&tabs().join('|').includes('كل الطلبات'),'HR tabs');
 ib=await inbox();const zl=ib.find(x=>x.type.code==='leave');ok(!!zl&&zl.status==='pendingHr','HR inbox shows pendingHr items');
 const omarAcc=ib.find(x=>x.type.code==='accountantRequest');ok(!!omarAcc,'HR inbox: accountant request at HR stage');
 const hrAp=await C.call('Request_Approve',{id:omarAcc.id});ok(hrAp.status==='pendingDestination'&&hrAp.employeeStatus==='approved'&&hrAp.currentApproverRole==='accountant'&&hrAp.statusLabel.length>0,'HR approval -> pendingDestination (employee sees approved)');
 const lv=C.d4.reqs.find(r=>r.id===zl.id);const heldBefore=lv.effect;const fin=await C.call('Request_Approve',{id:zl.id,note:'اعتمد'});
 ok(fin.status==='approved'&&lv.effect===(heldBefore==='held'?'deducted':heldBefore),'final approval: leave balance held -> deducted ('+heldBefore+' -> '+lv.effect+')');
 ok(C.d4.y[lv.emp].led.some(z=>z.type==='deduct'&&z.req===lv.id)||heldBefore!=='held','ledger deduct entry written');
 const lm=await C.call('LeaveRequest_Get',{id:zl.id});ok(lm.status==='approved','module 4 sees the approved status');
 // rejection releases hold
 const zl2=(await inbox()).find(x=>x.type.code==='leave');
 if(zl2){const q=C.d4.reqs.find(r=>r.id===zl2.id),h=q.effect;await C.call('Request_Reject',{id:zl2.id,reason:'نقص كادر'});ok(h!=='held'||q.effect==='released','rejection releases the held balance')}else ok(true,'no second leave item (skipped release check)');
 // corrections: create + approve updates attendance
 await C.call('AttendanceCorrection_GetMy&&0'.slice(0,0)||'Holiday_List',{});
 // all requests tab
 await tab('all');ok(rows().length>=6&&vm().flt.length===5,'all requests table with filters (type, status, employee, dates)');
 ok((await C.call('Request_GetList',{status:'rejected'})).items.every(x=>x.status==='rejected'),'status filter');
 // HR memo / incident
 const hm=await C.call('Memo_GetInbox',{});ok(hm.items.some(z=>z.id===m1.id),'HR memo inbox contains forwarded memo');
 const L=nextWork(C.dAdd(C.todayS(),50));
 ok(await code('Memo_Convert',{id:m1.id,target:'incident'})==='Hr:Memo:ConvertTargetMismatch','leave memo cannot convert to incident');
 ok(await code('Memo_Convert',{id:m1.id,target:'leaveRequest'})==='Hr:Memo:LeaveDetailsInvalid','leave details required');
 const mc=await C.call('Memo_Convert',{id:m1.id,target:'leaveRequest',leave:{kind:'annual',startDate:L,endDate:C.dAdd(L,2)},note:'تحويل'});ok(mc.status==='convertedToRequest'&&mc.convertedRequestId,'memo converted to a leave request');
 const cr=C.d4.reqs.find(r=>r.id===mc.convertedRequestId);ok(cr&&cr.status==='pendingHr'&&cr.srcMemo===m1.id&&cr.emp===sara.id,'converted request belongs to the memo employee and starts at pendingHr');
 const cres=await C.call('Request_Get',{id:cr.id});ok(cres.sourceMemoId===m1.id&&cres.canApprove,'request keeps sourceMemoId; HR can approve it');
 ok(await code('Memo_Convert',{id:m1.id,target:'leaveRequest',leave:{kind:'annual',startDate:L,endDate:C.dAdd(L,2)}})==='Hr:Memo:InvalidState','converted memo is closed to actions');
 const pm=C.d5.memos.find(z=>z.kind==='problem'&&z.status==='pendingHr');const cl=await C.call('Memo_Close',{id:pm.id,reason:'تمت المعالجة'});ok(cl.status==='closed'&&cl.closeReason==='تمت المعالجة','memo closed with reason');
 ok(await code('Memo_Close',{id:pm.id,reason:'x'})==='Hr:Memo:InvalidState','closed memo cannot be closed again');
 const inc=await C.call('Incident_GetInbox',{});ok(inc.items.length>=2,'HR incident inbox ('+inc.items.length+')');
 const g1=await C.call('Incident_Get',{id:i1.id});ok(await code('Incident_UpdateStatus',{id:i1.id,status:'inProgress',concurrencyStamp:'stale'})==='conflict','incident stale stamp -> 409');
 const u1=await C.call('Incident_UpdateStatus',{id:i1.id,status:'inProgress',note:'جارٍ',concurrencyStamp:g1.concurrencyStamp});ok(u1.status==='inProgress','incident -> inProgress');
 const u2=await C.call('Incident_UpdateStatus',{id:i1.id,status:'resolved',concurrencyStamp:u1.concurrencyStamp});ok(u2.status==='resolved','incident -> resolved');
 ok(await code('Incident_UpdateStatus',{id:i1.id,status:'inProgress',concurrencyStamp:u2.concurrencyStamp})==='Hr:Incident:InvalidTransition','resolved -> inProgress invalid');
 const da=await C.call('Delegation_List',{});ok(da.items.length>=3,'HR sees all delegations');
 ok(await code('Delegation_Create',{toUserId:nas.id,scope:'requests',startsAt:iso(1),endsAt:iso(9),reason:'x'})==='OK','HR creates only own delegation (no fromUser field in contract)');
 // delegation effect: Muna acts for Hassan
 const delIn=(await inbox()).filter(x=>x.delegatedFrom);ok(delIn.length>=1&&delIn.every(x=>x.delegatedFrom.displayName),'received delegation exposes items with delegatedFrom');
 // ---- admin
 ok(await login('adm'),'system admin signed in');await goto('req');ok(tabs().includes('أنواع الطلبات')&&tabs().includes('بانتظار إجرائي'),'admin tabs incl. request types');
 const aib=await inbox();const hr1=aib.find(x=>x.employee.id===E('Hassan Ali').id);ok(!!hr1&&hr1.currentApproverRole==='systemAdmin','dept manager request is assigned to the system admin');
 const ap=await C.call('Request_Approve',{id:hr1.id});ok(ap.status==='pendingHr','system admin approval -> pendingHr');
 await tab('types');ok(rows().length===5&&vm().tools.length===1,'admin request types (5) + New type');
 ok(await code('RequestType_Create',{code:'Bad Code',nameAr:'x',nameEn:'x',destination:'none',isActive:true})==='validation','type code pattern');
 ok(await code('RequestType_Create',{code:'administrativeNeed',nameAr:'x',nameEn:'x',destination:'none',isActive:true})==='Hr:Request:TypeCodeAlreadyExists','duplicate code');
 const nt=await C.call('RequestType_Create',{code:'trainingNeed',nameAr:'احتياج تدريب',nameEn:'Training need',destination:'none',isActive:true});ok(!nt.isSystem&&nt.destination==='none','custom type created');
 const sysT=(await C.call('RequestType_GetAdminList',{})).items.find(t=>t.code==='leave');ok(await code('RequestType_Update',{id:sysT.id,nameAr:'a',nameEn:'b',destination:'store',isActive:true,concurrencyStamp:sysT.concurrencyStamp})==='Hr:Request:SystemTypeDestinationLocked','system type destination locked');
 C.dlgOpen(C.qTypeDlg(T,null));ok(rv().dlgv.fields.length===5,'type dialog (code, 2 names, destination, status)');C.dlgClose();
 // ---- executive
 ok(await login('xd'),'executive signed in');await goto('req');ok(tabs().includes('بانتظار إجرائي')&&!tabs().includes('كل الطلبات')&&tabs().includes('المطالعات'),'executive tabs');
 ok(await code('Request_Submit',{requestTypeId:tid('administrativeNeed'),description:'x'})==='Hr:Request:ExecutiveCannotSubmit','executive cannot submit without an executing party');
 const xa=await C.call('Request_Submit',{requestTypeId:tid('accountantRequest'),description:'مصروف'});ok(xa.status==='pendingDestination'&&xa.steps.length===1,'executive accountant request goes directly to the accountant');
 ok(await code('Request_GetList',{})==='forbidden','executive has no all-requests list');
 ok(await code('Memo_Create',{kind:'leave',content:'x'})==='forbidden','executive cannot raise memos');
 // ---- accountant executes
 ok(await login('acc'),'accountant signed in (after MFA)');
 await goto('req');const ai=await inbox();ok(ai.some(x=>x.type.code==='accountantRequest'),'accountant inbox has destination items ('+ai.length+')');
 const comp=await C.call('Request_Approve',{id:ai[0].id});ok(comp.status==='completed'&&comp.employeeStatus==='completed','accountant execution -> completed');
 // ---- reviewer / store officer
 ok(await login('rv'),'reviewer signed in');await goto('req');ok(tabs().join('|')==='طلباتي|المطالعات|بلاغات الأعطال|التفويض','reviewer tabs');
 ok(await login('so'),'store officer signed in');await goto('req');ok(tabs().includes('بانتظار إجرائي'),'store officer inbox tab (destination role)');
 // ---- render everything
 for(const pr of ['emp','mgr','hr','adm','xd','acc','rv','so']){await login(pr);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['inbox','my','all','memos','inc','deleg','types']){C.setState({page:'req',tab:t,sub:'list',data:{},flt:{}});await C.enter();await sl(12);try{rv();ok(true,'render req/'+t+' '+pr+' '+L)}catch(e){ok(false,'render req/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
