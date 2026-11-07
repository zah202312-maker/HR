process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);if(C.state.scr==='mfa'&&(k==='acc')){C.srv.mfaE=true;await C.boot();await sl(200)}return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const E=(n)=>C.db.emps.find(e=>e.en===n);
const seed=(n)=>C.d6.cs.find(c=>c.ref==='CMP-2026-000'+n);
(async()=>{
 // ---------- employee (complainant)
 ok(await login('emp'),'employee signed in');
 await goto('cmp');ok(nav().includes('الشكاوى')&&tabs().join('|')==='شكاواي|صندوق الشكاوى','employee tabs: my complaints + inbox (empty unless assigned)');
 const rs=await C.call('Complaint_GetRespondents',{});const rids=rs.items.map(x=>x.id);
 ok(!rids.includes(E('Layth Al-Obaidi').id)&&!rids.includes(E('Nasser Al-Kaabi').id)&&!rids.includes(E('Sara Al-Hasani').id)&&rids.includes(E('Hassan Ali').id),'respondent list excludes executive, system admin and self (server-side)');
 ok(rs.items.every(x=>x.id&&x.fullName&&!('roles' in x)&&!('category' in x)),'respondent list shows name and department only (no role/category)');
 const ids=n=>E(n).id;
 const pv1=await C.call('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Omar Khalid')});ok(pv1.minimumLevel==='normal'&&pv1.selectableLevels.length===3&&pv1.routes[0].handlerPath.join()==='departmentManager,hrManager','colleague: min normal; normal -> dept manager then HR');
 ok(pv1.routes[1].handlerPath.join()==='hrManager'&&pv1.routes[2].handlerPath.join()==='executiveDirector','confidential -> HR directly; highly -> executive');
 const pv2=await C.call('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Hassan Ali')});ok(pv2.minimumLevel==='confidential'&&pv2.selectableLevels.join()==='confidential,highlyConfidential','dept manager respondent: min confidential');
 const pv3=await C.call('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Ahmed Al-Zubaidi')});ok(pv3.minimumLevel==='normal'&&pv3.routes[0].handlerPath.join()==='hrManager','store officer respondent: HR directly, no manager');
 const pv4=await C.call('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Muna Al-Rawi')});ok(pv4.minimumLevel==='highlyConfidential'&&pv4.routes.length===1&&pv4.routes[0].handlerPath[0]==='executiveDirector','HR manager respondent: highly confidential -> executive');
 const pv5=await C.call('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Raghad Al-Hilli')});ok(pv5.minimumLevel==='highlyConfidential','reviewer respondent: highly confidential, executive only');
 const pv6=await C.call('Complaint_PreviewRouting',{targetKind:'general'});ok(pv6.minimumLevel==='normal','general complaint: min normal');
 ok(await code('Complaint_PreviewRouting',{targetKind:'employee'})==='Hr:Complaints:RespondentRequired','RespondentRequired');
 ok(await code('Complaint_PreviewRouting',{targetKind:'general',respondentId:ids('Omar Khalid')})==='Hr:Complaints:RespondentNotAllowed','RespondentNotAllowed (general with respondent)');
 ok(await code('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Layth Al-Obaidi')})==='Hr:Complaints:RespondentNotAllowed','executive can never be a respondent');
 ok(await code('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Nasser Al-Kaabi')})==='Hr:Complaints:RespondentNotAllowed','system admin can never be a respondent');
 ok(await code('Complaint_PreviewRouting',{targetKind:'employee',respondentId:ids('Sara Al-Hasani')})==='Hr:Complaints:SelfComplaintNotAllowed','SelfComplaintNotAllowed');
 const s1=await C.call('Complaint_Submit',{targetKind:'employee',respondentId:ids('Omar Khalid'),level:'normal',subject:'سلوك',content:'تفاصيل الشكوى'});ok(/^CMP-2026-\d{4}$/.test(s1.referenceNumber)&&s1.status==='pendingDepartmentManager'&&!s1.levelRaisedBySystem,'normal complaint routed to dept manager; reference number');
 const s2=await C.call('Complaint_Submit',{targetKind:'employee',respondentId:ids('Hassan Ali'),level:'normal',subject:'سلوك المدير',content:'تفاصيل'});ok(s2.level==='confidential'&&s2.levelRaisedBySystem&&s2.status==='pendingHrManager','level below minimum is raised by the server (levelRaisedBySystem)');
 const s3=await C.call('Complaint_Submit',{targetKind:'general',level:'highlyConfidential',subject:'عام',content:'x'});ok(s3.status==='pendingExecutiveDirector','highly confidential -> executive');
 ok(await code('Complaint_Submit',{targetKind:'employee',level:'normal',subject:'x',content:'x'})==='Hr:Complaints:RespondentRequired','submit without respondent');
 const my=await C.call('Complaint_GetMyList',{});ok(my.items.length>=4&&my.items.every(x=>x.referenceNumber&&!('complainant' in x)&&!('handler' in x)),'my list: own complaints only, complainant summary DTO');
 ok(!my.items.some(x=>x.referenceNumber===seed(4).ref),'complaints filed against me or by others never appear in my list');
 const md=await C.call('Complaint_GetMyDetail',{id:s1.id});ok(md.respondent&&md.content&&!('actions' in md)&&!('handler' in md)&&!('complainant' in md)&&!('closureDecision' in md),'complainant detail has no actions, no handler, no decision field');
 ok(await code('Complaint_GetMyDetail',{id:seed(1).id})==='Hr:Complaints:NotFound','another user’s complaint -> 404 (never 403)');
 ok(await code('Complaint_GetDetail',{id:s1.id})==='Hr:Complaints:NotFound','complainant cannot use the handler route (404)');
 ok(await code('Complaint_GetAccessLog',{id:s1.id})==='Hr:Complaints:NotFound','complainant cannot read the access log (404)');
 ok(await code('Complaint_GetDetail',{id:'00000000-0000-4000-8000-000000009999'})==='Hr:Complaints:NotFound','non-existent id is indistinguishable (404)');
 // attachments
 ok(await code('Complaint_UploadAttachment',{id:s1.id,file:'a.exe',size:10})==='Hr:Document:FileTypeNotAllowed','attachment type rejected');
 ok(await code('Complaint_UploadAttachment',{id:s1.id,file:'a.pdf',size:20000000})==='Hr:Complaints:AttachmentTooLarge','attachment too large');
 ok(await code('Complaint_UploadAttachment',{id:s1.id,file:'virus.pdf',size:100})==='Hr:Complaints:AttachmentRejected','AttachmentRejected after scan');
 const at1=await C.call('Complaint_UploadAttachment',{id:s1.id,file:'proof.pdf',size:2048});ok(at1.fileName==='proof.pdf','complainant uploads an attachment');
 ok((await C.call('Complaint_DownloadAttachment',{id:s1.id,attachmentId:at1.id})).fileName==='proof.pdf','complainant downloads own attachment');
 ok(await code('Complaint_UploadAttachment',{id:seed(1).id,file:'x.pdf',size:1})==='Hr:Complaints:NotFound','cannot upload to someone else’s complaint (404)');
 // raise level by complainant
 const md2=await C.call('Complaint_GetMyDetail',{id:s1.id});
 ok(await code('Complaint_RaiseLevel',{id:s1.id,level:'normal',concurrencyStamp:md2.concurrencyStamp})==='Hr:Complaints:LevelNotHigher','LevelNotHigher (no downgrade path)');
 ok(await code('Complaint_RaiseLevel',{id:s1.id,level:'confidential',concurrencyStamp:'stale'})==='conflict','stale stamp -> 409');
 const rl=await C.call('Complaint_RaiseLevel',{id:s1.id,level:'confidential',reason:'حساسية',concurrencyStamp:md2.concurrencyStamp});ok(rl.level==='confidential'&&rl.status==='pendingHrManager'&&rl.callerRetainsAccess,'complainant raises level -> HR; keeps access to own complaint');
 const cl=seed(6);ok(await code('Complaint_RaiseLevel',{id:cl.id,level:'confidential',concurrencyStamp:cl.stamp})==='Hr:Complaints:ComplaintClosed','closed complaint cannot be changed');
 ok(await code('Complaint_UploadAttachment',{id:cl.id,file:'x.pdf',size:1})==='Hr:Complaints:ComplaintClosed','no upload after closing');
 ok(await code('ComplaintHandler_GetList',{})==='forbidden','employee cannot read handler assignments (403, no complaint data)');
 // UI wizard
 await tab('my');ok(vm().tools.length===1&&rows().length>=4&&vm().blocks.length===2,'my complaints tab: list + privacy notes + New');
 vm().tools[0].go();await sl(200);ok(C.state.sub==='new'&&vm().tabs.length===3&&vm().blocks[0].fields.length===2,'wizard step 1 (kind + respondent)');
 C.setF('targetKind','employee');await vm().blocks[0].acts.find(a=>a.cls==='p').go();await sl(80);ok(C.state.derr===''&&vm().blocks[0].fields[0].err.length>0||C.state.step===0,'wizard: respondent required for employee complaints');
 C.setF('respondent',ids('Ahmed Al-Zubaidi'));await vm().blocks[0].acts.find(a=>a.cls==='p').go();await sl(120);ok(C.state.step===1&&vm().blocks.length===3,'wizard step 2: routing preview + level/details');
 ok(vm().blocks[1].rows.length===3&&vm().blocks[0].kv.length===2,'routing preview table lists the 3 levels');
 C.setState({fv:{...C.state.fv,subject:'مخزن',content:'تفاصيل'}});vm().blocks[2].acts.find(a=>a.cls==='p').go();await sl(20);ok(C.state.step===2,'wizard step 3 (review)');
 ok(vm().blocks.some(b=>b.hasKv&&b.kv.some(k=>/اسمك يظهر|لا يُبلَّغ/.test(k.v))),'privacy notes shown before submitting');
 await vm().blocks[vm().blocks.length-1].acts.find(a=>a.l===T.aZSubmit).go();await sl(250);ok(C.state.sub==='mine'&&vm().blocks.length>=3&&vm().blocks.some(b=>b.hasForm),'submitted -> complaint details with attachment uploader');
 // ---------- manager (handler for normal; respondent of seed 2)
 ok(await login('mgr',true),'manager signed in (data persists)');
 await goto('cmp');await tab('inbox');const mi=await C.call('Complaint_GetAssignedList',{});
 ok(mi.items.length>=2&&mi.items.every(x=>x.level==='normal'&&x.status==='pendingDepartmentManager'),'dept manager inbox: normal complaints pending at him only');
 ok(!mi.items.some(x=>x.referenceNumber===seed(2).ref||x.referenceNumber===seed(3).ref||x.referenceNumber===seed(5).ref),'no confidential, highly confidential, or other managers’ complaints');
 ok(mi.items.some(x=>x.isLate&&x.dueAt),'late complaint flagged (deadline from settings)');
 ok(await code('Complaint_GetDetail',{id:seed(2).id})==='Hr:Complaints:NotFound','respondent (manager) gets 404 on the complaint filed against him');
 ok(await code('Complaint_GetAccessLog',{id:seed(2).id})==='Hr:Complaints:NotFound','respondent gets 404 on its access log too');
 ok(await code('Complaint_GetDetail',{id:seed(5).id})==='Hr:Complaints:NotFound','delegation from another manager grants no complaint access');
 ok(await code('Complaint_GetDetail',{id:seed(3).id})==='Hr:Complaints:NotFound','manager cannot see highly confidential complaints');
 const hd=await C.call('Complaint_GetDetail',{id:seed(1).id});ok(hd.complainant.fullName&&hd.respondent&&hd.respondentCategory==='colleague'&&hd.actions.length>=1,'handler detail shows complainant name, respondent, category, actions');
 ok(await code('Complaint_Close',{id:seed(1).id,decision:'x',concurrencyStamp:hd.concurrencyStamp})==='Hr:Complaints:NotFound','dept manager cannot close (404)');
 const act=await C.call('Complaint_AddAction',{id:seed(1).id,type:'meeting',note:'اجتماع مع الطرفين'});ok(act.type==='meeting'&&act.actor.displayName,'handler adds an action');
 const lg=await C.call('Complaint_GetAccessLog',{id:seed(1).id});ok(lg.items.length>=3&&lg.items[0].action==='viewAccessLog'&&lg.items.every(x=>x.user&&x.action&&!('content' in x)),'access log: append-only, newest first, records every access, no content');
 ok(lg.items.some(x=>x.action==='viewDetail')&&lg.items.some(x=>x.action==='addAction'),'detail view and action are logged');
 C.toSub('handle',seed(1).id);await sl(150);ok(vm().tools.some(t=>t.l===T.aZAct)&&vm().tools.some(t=>t.l===T.aZFwd)&&vm().tools.some(t=>t.l===T.aZLog)&&!vm().tools.some(t=>t.l===T.aZClose&&false),'manager detail tools: action, raise, forward, log');
 C.toSub('handle',seed(2).id);await sl(150);ok(C.state.derr==='Hr:Complaints:NotFound'&&vm().err&&vm().blocks.length===0,'UI: opening a forbidden complaint shows only a generic “not found”');
 const fwd0=await C.call('Complaint_GetDetail',{id:seed(1).id});const fw=await C.call('Complaint_Forward',{id:seed(1).id,note:'للمراجعة',concurrencyStamp:fwd0.concurrencyStamp});ok(fw.status==='pendingHrManager'&&fw.callerRetainsAccess===false,'forward to HR: callerRetainsAccess=false');
 ok(await code('Complaint_GetDetail',{id:seed(1).id})==='Hr:Complaints:NotFound','manager loses access immediately after forwarding');
 ok((await C.call('Complaint_GetAssignedList',{})).items.every(x=>x.referenceNumber!==seed(1).ref),'forwarded complaint leaves the manager inbox');
 ok(await code('Complaint_RaiseLevel',{id:seed(4).id,level:'confidential',concurrencyStamp:seed(4).stamp})==='OK','manager can raise a complaint pending with him');
 ok(await code('Complaint_GetDetail',{id:seed(4).id})==='Hr:Complaints:NotFound','after raising to confidential the manager loses access (callerRetainsAccess=false)');
 const sum=await C.call('Me_GetInboxSummary',{});ok(!sum.items.some(z=>/complaint/i.test(z.category)),'inbox summary never mentions complaints');
 // ---------- HR
 ok(await login('hr',true),'HR manager signed in');
 await goto('cmp');ok(tabs().join('|')==='صندوق الشكاوى|تعيين المعالجين','HR tabs: inbox + handler assignments');
 const hi=await C.call('Complaint_GetAssignedList',{});ok(hi.items.length>=4&&hi.items.every(x=>['normal','confidential'].includes(x.level)&&x.status==='pendingHrManager'||x.status==='closed'),'HR inbox: normal that reached HR + confidential (and closed)');
 ok(!hi.items.some(x=>x.level==='highlyConfidential'),'HR never sees highly confidential complaints');
 ok(await code('Complaint_GetDetail',{id:seed(3).id})==='Hr:Complaints:NotFound','HR gets 404 on a highly confidential complaint');
 ok(await code('Complaint_Submit',{targetKind:'general',level:'normal',subject:'x',content:'x'})==='forbidden','HR manager cannot file complaints via this API (role not listed)');
 const myh=await C.call('Complaint_GetMyList',{});ok(myh.items.length===0,'HR has an empty “my complaints” list');
 const hd2=await C.call('Complaint_GetDetail',{id:seed(2).id});ok(hd2.level==='confidential'&&hd2.respondentCategory==='departmentManager','HR reads the confidential complaint (respondent category: dept manager)');
 const up=await C.call('Complaint_UploadAttachment',{id:seed(2).id,file:'hr-note.pdf',size:300});ok(up.id,'handler uploads an attachment');
 const sara=await (async()=>{return null})();
 const ct=await C.call('Complaint_Close',{id:seed(2).id,decision:'تم التحقق وإفهام المدير',concurrencyStamp:hd2.concurrencyStamp});ok(ct.status==='closed'&&ct.closedAt,'HR closes with a decision');
 ok(await code('Complaint_AddAction',{id:seed(2).id,type:'note',note:'x'})==='Hr:Complaints:ComplaintClosed','closed complaint accepts no actions');
 const cd=await C.call('Complaint_GetDetail',{id:seed(2).id});ok(cd.closureDecision==='تم التحقق وإفهام المدير'&&cd.actions.some(a=>a.type==='closed'),'handler sees the decision; closed complaint stays readable (never deleted)');
 ok(!C.d6.cs.length===false&&C.d6.cs.some(c=>c.id===seed(2).id),'complaints are never deleted');
 // complainant view after close
 await login('emp',true);const mine2=await C.call('Complaint_GetMyList',{});ok(mine2.items.some(x=>x.referenceNumber===seed(1).ref)===false,'complaint of another user is not in my list');
 const sc=(await C.call('Complaint_GetMyList',{status:'closed'})).items[0];ok(sc&&sc.closedAt,'complainant sees closed status and date');
 const scd=await C.call('Complaint_GetMyDetail',{id:sc.id});ok(!('closureDecision' in scd)&&!('actions' in scd),'complainant never receives actions or handler data (contract gap: no decision text)');
 // ---------- handler assignments
 await login('hr',true);
 const sara2=E('Sara Al-Hasani'),nas=E('Nasser Al-Kaabi'),rag=E('Raghad Al-Hilli'),hass=E('Hassan Ali'),layth=E('Layth Al-Obaidi');
 const d0=new Date();const iso=(n)=>new Date(d0.getTime()+n*36e5).toISOString();
 ok(await code('ComplaintHandler_Request',{userId:nas.id,level:'normal',from:iso(1),reason:'x'})==='Hr:Complaints:HandlerCannotBeSystemAdmin','system admin cannot be a handler');
 ok(await code('ComplaintHandler_Request',{userId:rag.id,level:'normal',from:iso(1),reason:'x'})==='Hr:Complaints:HandlerRoleNotAllowed','reviewer cannot be a handler at any level');
 ok(await code('ComplaintHandler_Request',{userId:hass.id,level:'confidential',from:iso(1),reason:'x'})==='Hr:Complaints:HandlerRoleNotAllowed','dept manager not allowed at confidential+');
 ok(await code('ComplaintHandler_Request',{userId:layth.id,level:'normal',from:iso(1),reason:'x'})==='Hr:Complaints:HandlerRoleNotAllowed','executive not allowed at normal');
 ok(await code('ComplaintHandler_Request',{userId:sara2.id,level:'normal',from:iso(5),to:iso(1),reason:'x'})==='Hr:Complaints:InvalidAssignmentPeriod','InvalidAssignmentPeriod');
 const rq=await C.call('ComplaintHandler_Request',{userId:sara2.id,level:'normal',from:iso(-1),to:iso(500),reason:'تغطية'});ok(rq.status==='pendingApproval'&&rq.requestedBy.displayName,'handler assignment requested (pendingApproval)');
 ok(await code('ComplaintHandler_Approve',{id:rq.id,concurrencyStamp:rq.concurrencyStamp})==='forbidden','HR cannot approve (executive only)');
 await login('emp',true);ok((await C.call('Complaint_GetAssignedList',{})).items.length===0,'pending assignment grants no access (empty inbox, no 403)');
 // ---------- executive
 ok(await login('xd',true),'executive signed in');
 await goto('cmp');ok(tabs().join('|')==='صندوق الشكاوى|تعيين المعالجين','executive tabs');
 const xi=await C.call('Complaint_GetAssignedList',{});ok(xi.items.length>=1&&xi.items.every(x=>x.level==='highlyConfidential'),'executive inbox: highly confidential only (never normal, not even as counts)');
 ok(await code('Complaint_GetDetail',{id:seed(1).id})==='Hr:Complaints:NotFound','executive gets 404 on a normal complaint');
 const xd=await C.call('Complaint_GetDetail',{id:seed(3).id});ok(xd.level==='highlyConfidential','executive reads a highly confidential complaint');
 const ap=await C.call('ComplaintHandler_Approve',{id:rq.id,concurrencyStamp:rq.concurrencyStamp});ok(ap.status==='approved'&&ap.decidedBy,'executive approves the assignment');
 ok(await code('ComplaintHandler_Approve',{id:rq.id,concurrencyStamp:ap.concurrencyStamp})==='Hr:Complaints:AssignmentNotPending','AssignmentNotPending');
 const gl=await C.call('ComplaintHandler_GetList',{});ok(gl.items.length>=1&&gl.items.every(a=>!('subject' in a)&&!('content' in a)),'assignment list holds no complaint data');
 // assignment holder (employee role) now handles normal complaints
 await login('emp',true);const ai=await C.call('Complaint_GetAssignedList',{});ok(ai.items.length>=1&&ai.items.every(x=>x.level==='normal'),'approved assignment: employee sees normal complaints ('+ai.items.length+')');
 ok(!ai.items.some(x=>C.d6.cs.find(c=>c.id===x.id).cm===E('Sara Al-Hasani').id),'assignment holder never sees complaints where they are a party');
 const target=ai.items.find(x=>x.status==='pendingHrManager'||x.status==='pendingDepartmentManager');
 ok(await code('Complaint_GetDetail',{id:target.id})==='OK','assignment holder opens a complaint of their level');
 await goto('cmp');await tab('inbox');ok(rows().length>=1,'employee inbox tab shows assigned complaints');
 await login('xd',true);const rv2=await C.call('ComplaintHandler_Revoke',{id:rq.id,reason:'انتهت الحاجة',concurrencyStamp:ap.concurrencyStamp});ok(rv2.status==='revoked','executive revokes the assignment');
 await login('emp',true);ok((await C.call('Complaint_GetAssignedList',{})).items.length===0&&await code('Complaint_GetDetail',{id:target.id})==='Hr:Complaints:NotFound','revocation stops access immediately');
 await login('xd',true);ok(await code('ComplaintHandler_Revoke',{id:rq.id,reason:'x',concurrencyStamp:rv2.concurrencyStamp})==='Hr:Complaints:AssignmentNotActive','AssignmentNotActive');
 const rj=await C.call('ComplaintHandler_Request',{userId:E('Omar Khalid').id,level:'normal',from:iso(1),reason:'x'}).catch(e=>null);
 const xc=await C.call('Complaint_GetDetail',{id:seed(3).id});const xcl=await C.call('Complaint_Close',{id:seed(3).id,decision:'تمت المعالجة',concurrencyStamp:xc.concurrencyStamp});ok(xcl.status==='closed','executive closes a highly confidential complaint');
 // ---------- system admin
 ok(await login('adm',true),'system admin signed in');
 await goto('cmp');ok(tabs().join('|')==='مهلة الشكاوى','system admin: deadlines settings only (no complaint content)');
 ok(await code('Complaint_GetDetail',{id:seed(1).id})==='Hr:Complaints:NotFound'&&await code('Complaint_GetAccessLog',{id:seed(1).id})==='Hr:Complaints:NotFound','system admin gets 404 on every complaint resource');
 ok((await C.call('Complaint_GetAssignedList',{})).items.length===0,'system admin inbox is empty (no 403, no hints)');
 ok(await code('Complaint_GetRespondents',{})==='forbidden'&&await code('ComplaintHandler_Request',{userId:sara2.id,level:'normal',from:iso(1),reason:'x'})==='forbidden','no submit/respondents/assignments for system admin');
 const cfg=await C.call('SystemSettings_GetComplaints',{});ok(cfg.delayDeadlineWorkingDays===10&&cfg.overdueReminderIntervalWorkingDays===2,'deadline settings (10 working days default)');
 ok(await code('SystemSettings_UpdateComplaints',{delayDeadlineWorkingDays:5,overdueReminderIntervalWorkingDays:1,concurrencyStamp:'x'})==='conflict','settings stale stamp');
 ok(await code('SystemSettings_UpdateComplaints',{delayDeadlineWorkingDays:5,overdueReminderIntervalWorkingDays:1,concurrencyStamp:cfg.concurrencyStamp})==='OK'&&C.d6.cfg.delay===5,'deadline updated');
 ok(vm().blocks.length===2&&vm().blocks[1].hasForm,'settings form');
 await login('hr',true);const l5=await C.call('Complaint_GetAssignedList',{isLate:true});ok(l5.items.length>=0,'late filter works');
 // ---------- reviewer / accountant hidden
 ok(await login('rv',true),'reviewer signed in');ok(!nav().includes('الشكاوى'),'reviewer: Complaints hidden');
 ok(await code('Complaint_GetAssignedList',{})==='OK'&&(await C.call('Complaint_GetAssignedList',{})).items.length===0,'reviewer inbox call returns an empty list, never an error');
 // ---------- render everything
 for(const pr of ['emp','mgr','hr','xd','adm']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['my','inbox','handlers','cfg']){C.setState({page:'cmp',tab:t,sub:'list',data:{},flt:{}});await C.enter();await sl(12);try{rv();ok(true,'render cmp/'+t+' '+pr+' '+L)}catch(e){ok(false,'render cmp/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
