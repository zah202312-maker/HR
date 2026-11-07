process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u,cb){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n};if(cb)cb()}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const U=(n)=>C.UID(n);const W=(k)=>C.d13.ws.find(w=>w.emp===U(k));const E=(n)=>C.db.emps.find(e=>e.en===n);
(async()=>{
 const td=()=>C.todayS();
 // ---------- department manager recommends
 ok(await login('mgr'),'department manager signed in');await goto('wrn');
 ok(nav().includes('الإنذارات')&&tabs().join('|')==='توصياتي|سجل موظف','manager tabs: my recommendations, employee record');
 ok(await code('Warning_Create',{employeeId:U('99'),recommendedLevel:'first',incidentDate:td(),description:'x'})==='Hr:Warning:EmployeeNotFound','EmployeeNotFound');
 ok(await code('Warning_Create',{employeeId:U('12'),recommendedLevel:'first',incidentDate:td(),description:'x'})==='Hr:Warning:SelfRecommendationNotAllowed','SelfRecommendationNotAllowed');
 ok(await code('Warning_Create',{employeeId:U('23'),recommendedLevel:'first',incidentDate:td(),description:'x'})==='Hr:Warning:TargetIsDepartmentManager','a department manager cannot recommend on another department manager');
 ok(await code('Warning_Create',{employeeId:U('24'),recommendedLevel:'first',incidentDate:td(),description:'x'})==='Hr:Warning:TargetNotInTeam','TargetNotInTeam');
 ok(await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:C.dAdd(td(),3),description:'x'})==='Hr:Warning:IncidentDateInFuture','IncidentDateInFuture');
 ok(await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'bad',incidentDate:td(),description:'x'})==='validation'&&await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:td(),description:''})==='validation','level and description validated');
 ok(await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:td(),description:'x',attachments:[{file:'a.exe',size:10}]})==='Hr:Document:FileTypeNotAllowed'&&await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:td(),description:'x',attachments:[{file:'a.pdf',size:20000000}]})==='Hr:Document:FileTooLarge'&&await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:td(),description:'x',attachments:[{file:'virus.pdf',size:10}]})==='Hr:Warning:AttachmentRejected','attachment checks: type, size, scan');
 const many=Array.from({length:11},(_,i)=>({file:'f'+i+'.pdf',size:10}));ok(await code('Warning_Create',{employeeId:U('11'),recommendedLevel:'first',incidentDate:td(),description:'x',attachments:many})==='validation','at most 10 documents');
 const nw=await C.call('Warning_Create',{employeeId:U('11'),recommendedLevel:'final',incidentDate:C.dAdd(td(),-1),description:'واقعة جديدة سرية الوصف',attachments:[{file:'evidence.pdf',size:2048}]});
 ok(nw.stage==='underReview'&&nw.issuedLevel===null&&nw.decidedAt===null,'recommendation, not a warning: stage underReview');
 const wnew=C.d13.ws.find(w=>w.id===nw.id);ok(wnew.status==='recommended'&&wnew.atts.length===1,'stored as “recommended” with its document');
 ok(!C.d8.notes.some(n=>n.user===U('11')&&n.rid===nw.id),'nothing reaches the employee at the recommendation step');
 ok(C.d8.notes.some(n=>n.user===E('Muna Al-Rawi').id&&n.code==='warningRecommended'&&n.rid===nw.id),'warningRecommended goes to the HR manager');
 const mine=await C.call('Warning_GetRecommendedByMe',{});ok(mine.items.length>=5&&mine.items.every(w=>w.stage&&!('employeeResponse' in w)&&!('objection' in w)&&!('decision' in w)),'my recommendations show only the stage (no response, objection or decision notes)');
 ok((await C.call('Warning_GetRecommendedByMe',{stage:'issued'})).items.every(w=>w.stage==='issued')&&(await C.call('Warning_GetRecommendedByMe',{stage:'notIssued'})).items.length>=1,'stage filter');
 ok(await code('Warning_List',{})==='forbidden'&&await code('Warning_Notify',{id:nw.id})==='forbidden'&&await code('Warning_Decide',{id:nw.id,decision:'dismiss',note:'x'})==='forbidden'&&await code('Warning_Get',{id:nw.id})==='forbidden','a department manager cannot read or decide warnings');
 // employee file
 const f1=await C.call('Warning_GetEmployeeFile',{employeeId:U('11')});ok(f1.items.length===1&&f1.items[0].status==='issued'&&f1.items[0].decisionNote,'employee record for a manager: effective warnings only, with the decision');
 ok((await C.call('Warning_GetEmployeeFile',{employeeId:U('21')})).items.length===0&&(await C.call('Warning_GetEmployeeFile',{employeeId:U('22')})).items.length===1,'dismissed warnings are hidden from the manager; objected ones still count');
 ok(await code('Warning_GetEmployeeFile',{employeeId:U('24')})==='Hr:Warning:NotFound','record of someone outside the team -> 404');
 await tab('file');ok(vm().flt.length===1&&vm().blocks.length===1,'record tab asks for an employee first');C.setFlt('we',U('11'));await sl(250);ok(rows().length===1,'record tab lists the effective warning');
 await tab('rec');ok(rows().length>=5&&vm().tools.length===1,'recommendations tab with New recommendation');
 vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===5&&rv().dlgv.fields[4].isFile,'recommendation dialog has a documents field');C.dlgClose();
 // ---------- reviewer recommends on a department manager only
 ok(await login('rv',true),'reviewer signed in');
 ok(await code('Warning_Create',{employeeId:U('18'),recommendedLevel:'first',incidentDate:td(),description:'x'})==='Hr:Warning:TargetNotDepartmentManager','a reviewer recommends on department managers only');
 const rw=await C.call('Warning_Create',{employeeId:U('23'),recommendedLevel:'first',incidentDate:td(),description:'تأخر'});ok(rw.stage==='underReview'&&C.d13.ws.find(w=>w.id===rw.id).role==='reviewer','reviewer recommendation on a department manager');
 ok((await C.call('Warning_GetRecommendedByMe',{})).items.length>=2,'reviewer sees their recommendations');
 ok(await code('Warning_GetEmployeeFile',{employeeId:U('12')})==='forbidden','reviewer has no employee record');
 // ---------- HR reviews
 ok(await login('hr',true),'HR signed in');await goto('wrn');ok(tabs().join('|')==='سجل موظف|كل الإنذارات','HR tabs: record + all warnings');
 const all=await C.call('Warning_List',{maxResultCount:100});ok(all.items.length>=10&&all.items.every(w=>w.recommendedBy.displayName&&w.recommenderRole),'HR sees every recommendation with the recommender');
 ok((await C.call('Warning_List',{status:'recommended'})).items.every(w=>w.status==='recommended')&&(await C.call('Warning_List',{level:'final'})).items.every(w=>w.recommendedLevel==='final')&&(await C.call('Warning_List',{employeeId:U('11')})).items.every(w=>w.employee.id===U('11')),'filters (status, level, employee)');
 const sync=(await C.call('Warning_List',{employeeId:U('14')})).items[0];ok(sync.status==='responseExpired','the daily job moved the unanswered warning to responseExpired');
 ok(C.d8.notes.some(n=>n.user===E('Muna Al-Rawi').id&&n.code==='warningReadyForDecision'&&n.rid===sync.id),'…and notified HR that it is ready for decision');
 const g=await C.call('Warning_Get',{id:nw.id});ok(g.attachments.length===1&&g.description,'full view with documents');
 const dl=await C.call('Warning_GetAttachmentDownloadUrl',{id:nw.id,attachmentId:g.attachments[0].id});ok(/^https:/.test(dl.url)&&dl.expiresAt,'temporary download link');
 ok(await code('Warning_GetAttachmentDownloadUrl',{id:nw.id,attachmentId:W('18').atts[0].id})==='Hr:Warning:NotFound','an attachment of another warning -> 404');
 ok(await code('Warning_Decide',{id:nw.id,decision:'dismiss',note:'x'})==='Hr:Warning:InvalidStatus','cannot decide before notifying (InvalidStatus)');
 const nt=await C.call('Warning_Notify',{id:nw.id});ok(nt.status==='notified'&&nt.notifiedAt&&nt.responseDueDate>td(),'notify: status notified, response due date set');
 const n3=(()=>{let n=0,d=td();while(d<nt.responseDueDate){d=C.dAdd(d,1);if(C.isWorkDay(d))n++}return n})();ok(n3===3,'response window is 3 working days (company calendar) ['+n3+']');
 ok(await code('Warning_Notify',{id:nw.id})==='Hr:Warning:InvalidStatus','notify once');
 const nn=C.d8.notes.find(n=>n.user===U('11')&&n.code==='warningNotified'&&n.rid===nw.id);ok(!!nn&&!JSON.stringify(C.NT.warningNotified).includes('واقعة'),'the employee gets warningNotified with generic text (no incident details)');
 ok(await code('Warning_Decide',{id:nw.id,decision:'issue',issuedLevel:'first',note:'x'})==='Hr:Warning:ResponseWindowOpen','ResponseWindowOpen: no decision while the window is open');
 // employee responds
 ok(await login('emp',true),'employee signed in');await goto('wrn');ok(tabs().join('|')==='إنذاراتي','employee tab');
 const ml=await C.call('Warning_GetMyList',{});ok(ml.items.length>=2&&!ml.items.some(w=>w.id===W('11').id&&W('11').status==='recommended')&&ml.items.every(w=>w.status!=='recommended'),'my list: only what I was notified of (recommended never shown)');
 ok(Object.keys(ml.items[0]).sort().join()==='canObject,canRespond,decidedAt,decisionNote,id,incidentDate,description,level,myObjection,myResponse,notifiedAt,responseDueDate,status'.split(',').sort().join(),'employee view: no recommender, documents or HR notes');
 const rs=C.d13.ws.filter(w=>w.emp===U('11'));const cur=await C.call('Warning_GetMy',{id:nw.id});ok(cur.canRespond&&!cur.canObject&&cur.status==='notified','the new warning waits for my response');
 ok(await code('Warning_GetMy',{id:W('18').id})==='Hr:Warning:NotFound'&&await code('Warning_Respond',{id:W('18').id,content:'x'})==='Hr:Warning:NotFound','someone else’s warning -> 404');
 ok(await code('Warning_Respond',{id:nw.id,content:''})==='validation','response text required');
 const rp=await C.call('Warning_Respond',{id:nw.id,content:'أوضح ما حدث...'});ok(rp.status==='responded'&&rp.myResponse&&!rp.canRespond,'response recorded once');
 ok(await code('Warning_Respond',{id:nw.id,content:'مرة أخرى'})==='Hr:Warning:ResponseAlreadySubmitted','ResponseAlreadySubmitted');
 ok(C.d8.notes.some(n=>n.user===E('Muna Al-Rawi').id&&n.code==='warningReadyForDecision'&&n.rid===nw.id),'HR notified that it is ready for decision');
 // window expired
 const pend=C.d13.ws.find(w=>w.status==='notified'&&w.emp===U('11'));ok(!pend,'no other open warning for the employee');
 const tmp=C.d13.ws.find(w=>w.id===nw.id);tmp.status='notified';tmp.resp=null;tmp.due=C.dAdd(td(),-1);
 ok(await code('Warning_Respond',{id:nw.id,content:'متأخر'})==='Hr:Warning:ResponseWindowExpired','ResponseWindowExpired after the due day');
 tmp.status='responded';tmp.resp={content:'أوضح ما حدث...',at:new Date().toISOString()};tmp.due=C.dAdd(td(),3);
 // objection by Sara on her issued warning
 const iss=W('11');const issW=C.d13.ws.find(w=>w.emp===U('11')&&w.status==='issued');const im=await C.call('Warning_GetMy',{id:issW.id});ok(im.canObject&&!im.canRespond&&im.decisionNote,'an issued warning can be objected to, not responded to');
 ok(await code('Warning_Respond',{id:issW.id,content:'x'})==='Hr:Warning:InvalidStatus','InvalidStatus');
 const ob=await C.call('Warning_Object',{id:issW.id,content:'أعترض لأن...'});ok(ob.status==='objected'&&ob.myObjection&&!ob.canObject,'objection recorded; the warning stays in force until decided');
 ok(await code('Warning_Object',{id:issW.id,content:'مرة ثانية'})==='Hr:Warning:ObjectionAlreadySubmitted','one objection only');
 ok(C.d8.notes.some(n=>n.user===E('Layth Al-Obaidi').id&&n.code==='warningObjectionSubmitted'&&n.rid===issW.id),'the executive director is notified of the objection');
 await tab('my');ok(rows().length>=2&&vm().blocks.length===2,'my warnings UI');
 const sub=C.d13.ws.find(w=>w.emp===U('11')&&w.status==='responded');C.toSub('mine',sub.id);await sl(250);ok(C.state.sub==='mine'&&vm().blocks[0].hasKv&&vm().tools.length===1,'detail page (read-only while waiting for HR)');
 // ---------- HR decides
 await login('hr',true);
 ok(await code('Warning_Decide',{id:nw.id,decision:'issue',note:'x'})==='Hr:Warning:LevelRequired','LevelRequired when issuing');
 ok(await code('Warning_Decide',{id:nw.id,decision:'issue',issuedLevel:'first',note:''})==='validation','a decision note is required');
 const dc=await C.call('Warning_Decide',{id:nw.id,decision:'issue',issuedLevel:'first',note:'ثبتت الواقعة'});ok(dc.status==='issued'&&dc.decision.issuedLevel==='first'&&dc.decision.decidedBy.displayName&&dc.employeeResponse,'issue: the decision, level and the response are kept together');
 ok(C.d8.notes.some(n=>n.user===U('11')&&n.code==='warningDecided'&&n.rid===nw.id),'warningDecided reaches the employee');
 ok(await code('Warning_Decide',{id:nw.id,decision:'dismiss',note:'x'})==='Hr:Warning:InvalidStatus','cannot decide twice');
 const rz=C.d13.ws.find(w=>w.status==='responseExpired');const dz=await C.call('Warning_Decide',{id:rz.id,decision:'dismiss',note:'لا ضرورة'});ok(dz.status==='dismissed'&&dz.decision.issuedLevel===null,'an expired-window warning can be decided; dismiss does not issue');
 const rsp=C.d13.ws.find(w=>w.status==='responded');ok(!!rsp,'a responded warning is ready for decision');
 const hw=C.d13.ws.find(w=>w.id===nw.id);const hrId=E('Muna Al-Rawi').id;const keep=hw.emp;hw.emp=hrId;hw.status='recommended';ok(await code('Warning_Notify',{id:hw.id})==='Hr:Warning:SelfDecisionNotAllowed','HR cannot notify on a warning about HR');hw.status='responded';ok(await code('Warning_Decide',{id:hw.id,decision:'dismiss',note:'x'})==='Hr:Warning:SelfDecisionNotAllowed','…nor decide it');hw.emp=keep;hw.status='issued';
 const fh=await C.call('Warning_GetEmployeeFile',{employeeId:U('21')});ok(fh.items.length===1&&fh.items[0].status==='dismissed','HR record includes dismissed warnings');
 const fn=await C.call('Warning_GetEmployeeFile',{employeeId:U('22')});ok(fn.items.length===1&&fn.items[0].objectionOutcome===null,'objected warning is in the record with a pending objection');
 await goto('wrn');await tab('hr');ok(rows().length>=10&&vm().flt.length===4,'HR list UI with filters');
 const some=C.d13.ws.find(w=>w.status==='recommended');C.toSub('one',some.id);await sl(250);ok(C.state.sub==='one'&&vm().tools.some(t=>t.l===T.aWrNotify)&&vm().blocks.length===5,'details: Notify action for a recommendation');
 const resp2=C.d13.ws.find(w=>w.status==='responded'&&w.emp!==U('11'));C.toSub('one',resp2.id);await sl(250);ok(vm().tools.some(t=>t.l===T.aWrDec),'details: Decide action for a responded warning');
 vm().tools.find(t=>t.l===T.aWrDec).go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===3,'decision dialog');C.dlgClose();
 // ---------- reports
 const hw2=await C.call('Report_GetHrWarnings',{from:C.dAdd(td(),-90),to:td()});ok(hw2.items.length>=8&&hw2.items.every(r=>r.levelName&&r.statusName&&!('description' in r)),'HR warnings report: level and status, never the incident description');
 ok(await code('Report_GetHrWarnings',{from:td(),to:C.dAdd(td(),-5)})==='Hr:Reports:InvalidPeriod'&&await code('Report_GetHrWarnings',{from:td()})==='validation','period validation');
 ok((await C.call('Report_Export',{code:'hrWarnings',format:'pdf',from:C.dAdd(td(),-90),to:td()})).fileName.startsWith('hrWarnings_'),'hrWarnings export');
 await goto('rep');await tab('hwrn');ok(rows().length>=1&&vm().flt.length===4,'reports: warnings tab for HR');
 // ---------- executive: objections only
 ok(await login('xd',true),'executive signed in');await goto('wrn');ok(tabs().join('|')==='الاعتراضات','executive tab: objections only');
 const ob2=await C.call('Warning_GetObjections',{});ok(ob2.items.length>=2&&ob2.items.every(w=>w.objection),'executive sees only warnings with an objection');
 ok((await C.call('Warning_GetObjections',{isPending:true})).items.every(w=>!w.objection.outcome),'pending filter');
 ok(await code('Warning_Get',{id:C.d13.ws.find(w=>!w.obj).id})==='Hr:Warning:NotFound','a warning without an objection -> 404 for the executive');
 const ow=ob2.items.find(w=>w.status==='objected');ok((await C.call('Warning_Get',{id:ow.id})).objection.content,'objected warning opens for the executive');
 ok(ow.attachments.length===0||(await C.call('Warning_GetAttachmentDownloadUrl',{id:ow.id,attachmentId:ow.attachments[0].id})).url,'the same restriction applies to documents');
 ok(await code('Warning_List',{})==='forbidden'&&await code('Warning_Decide',{id:ow.id,decision:'dismiss',note:'x'})==='forbidden','the executive cannot list or decide warnings');
 await tab('obj');ok(rows().length>=2&&vm().flt.length===1,'objections tab');C.toSub('one',ow.id);await sl(250);ok(vm().tools.some(t=>t.l===T.aWrRes),'Resolve objection action');
 ok(await code('Warning_ResolveObjection',{id:ow.id,outcome:'maybe',note:'x'})==='validation','outcome validated');
 const rzv=await C.call('Warning_ResolveObjection',{id:ow.id,outcome:'overturn',note:'الوقائع غير كافية'});ok(rzv.status==='objectionOverturned'&&rzv.objection.outcome==='overturn'&&rzv.objection.resolvedBy.displayName,'overturn: the warning is cancelled, the record stays');
 ok(await code('Warning_ResolveObjection',{id:ow.id,outcome:'uphold',note:'x'})==='Hr:Warning:InvalidStatus','final: cannot resolve twice');
 ok(C.d13.ws.some(w=>w.id===ow.id)&&C.d8.notes.some(n=>n.user===ow.employee.id&&n.code==='warningObjectionResolved')&&C.d8.notes.some(n=>n.user===E('Muna Al-Rawi').id&&n.code==='warningObjectionResolvedForHr'),'nothing is deleted; the employee and HR are notified');
 await login('hr',true);ok((await C.call('Warning_GetEmployeeFile',{employeeId:ow.employee.id})).items.some(e=>e.status==='objectionOverturned'),'HR record keeps the overturned warning');
 await login('mgr',true);ok((await C.call('Warning_GetEmployeeFile',{employeeId:ow.employee.id})).items.every(e=>e.status!=='objectionOverturned'),'the manager no longer sees a cancelled warning');
 // other roles
 for(const p of ['adm','so']){await login(p,true);ok(await code('Warning_List',{})==='forbidden'&&await code('Warning_GetMyList',{})==='forbidden'&&!nav().includes('الإنذارات'),p+': no warnings access')}
 // notification routing
 await login('hr',true);const note=C.d8.notes.find(n=>n.user===E('Muna Al-Rawi').id&&n.code==='warningReadyForDecision');await C.ntOpen({id:note.id,templateCode:note.code,resourceId:note.rid,isRead:true});await sl(400);ok(C.state.page==='wrn'&&C.state.sub==='one'&&C.state.sid===note.rid,'HR notification opens the warning');
 for(const pr of ['emp','mgr','rv','hr','xd']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['my','rec','file','hr','obj']){C.setState({page:'wrn',tab:t,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render wrn/'+t+' '+pr+' '+L)}catch(e){ok(false,'render wrn/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
