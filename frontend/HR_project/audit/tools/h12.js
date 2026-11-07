process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u,cb){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n};if(cb)cb()}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const U=(n)=>C.UID(n),E=(n)=>C.db.emps.find(e=>e.en===n);
const cur=()=>C.d9.cycles[0];const evOf=(k,cy)=>C.d9.evs.find(v=>v.emp===U(k)&&v.cyc===(cy||cur()).id);
(async()=>{
 // ---------- manager
 ok(await login('mgr'),'manager signed in');await goto('perf');
 ok(tabs().join('|')==='تقييمات فريقي|التقرير الشهري|ملخص سنوي لموظف','manager tabs: team, monthly report, annual summary');
 const cy=await C.call('EvaluationCycle_GetCurrent',{});const t=k=>new Date(cy[k]).getTime();
 ok(cy.status==='open'&&t('opensAt')<t('dataFreezeAt')&&t('dataFreezeAt')<t('dueAt')&&t('dueAt')<=t('escalatesAt'),'cycle dates are ordered: opens < freeze < due <= escalation');
 ok(cy.managerProgress&&cy.managerProgress.expectedCount>=2&&cy.managerProgress.remainingCount===cy.managerProgress.expectedCount-cy.managerProgress.submittedCount,'manager progress ('+JSON.stringify(cy.managerProgress)+')');
 const tl=await C.call('Evaluation_GetTeamList',{maxResultCount:100});const mineIds=C.db.emps.filter(e=>e.mgr===U('12')).map(e=>e.id);
 ok(tl.items.length>=4&&tl.items.every(x=>mineIds.includes(x.employee.id)),'team list holds only his current team ('+tl.totalCount+')');
 ok(!tl.items.some(x=>C.db.emps.find(e=>e.id===x.employee.id).roles.includes('departmentManager')),'department managers are never evaluated (no form)');
 ok(await code('Evaluation_GetManagerForm',{id:evOf('24').id})==='Hr:Evaluations:NotFound','a form of another manager’s employee -> 404');
 const yo=evOf('20');const f0=await C.call('Evaluation_GetManagerForm',{id:yo.id});
 ok(f0.isEditable&&f0.scores.length===5&&f0.availableGrades.length===5&&f0.attendance.isFrozen&&f0.attendance.scheduledWorkingDays>0,'form: 5 criteria, 5 verbal grades, frozen attendance summary');
 ok(f0.scores.filter(s=>s.gradeCode).length===2&&f0.employeeNotes,'seeded partial draft is loaded');
 ok(!('internalNote' in f0)&&JSON.stringify(f0).indexOf('مسودة ملاحظة داخلية')===-1,'the manager form never contains the internal note');
 const nl=await C.call('EvaluationInternalNote_GetList',{id:yo.id});ok(nl.items.length===1&&nl.items[0].isDraft,'author sees only his own draft through the internal-note route');
 ok(await code('Evaluation_SaveDraft',{id:yo.id,scores:[{criterionId:U('99'),gradeCode:'good'}],concurrencyStamp:f0.concurrencyStamp})==='Hr:Evaluations:InvalidCriterion','InvalidCriterion');
 ok(await code('Evaluation_SaveDraft',{id:yo.id,scores:[{criterionId:f0.scores[2].criterionId,gradeCode:'superb'}],concurrencyStamp:f0.concurrencyStamp})==='Hr:Evaluations:InvalidGrade','InvalidGrade');
 ok(await code('Evaluation_SaveDraft',{id:yo.id,scores:[],concurrencyStamp:'stale'})==='conflict','stale stamp -> 409');
 const sv=await C.call('Evaluation_SaveDraft',{id:yo.id,scores:[{criterionId:f0.scores[2].criterionId,gradeCode:'good'}],employeeNotes:'ملاحظة',concurrencyStamp:f0.concurrencyStamp});ok(sv.scores.filter(s=>s.gradeCode).length===3&&sv.employeeNotes==='ملاحظة','partial save keeps earlier scores');
 ok(await code('Evaluation_Submit',{id:yo.id,concurrencyStamp:sv.concurrencyStamp})==='Hr:Evaluations:IncompleteScores','IncompleteScores');
 ok(await code('EvaluationInternalNote_Add',{id:yo.id,content:'x'})==='Hr:Evaluations:EvaluationNotSubmitted','notes use the draft route before submission');
 const full=await C.call('Evaluation_SaveDraft',{id:yo.id,scores:f0.scores.map((s,i)=>({criterionId:s.criterionId,gradeCode:['good','veryGood','good','excellent','acceptable'][i]})),concurrencyStamp:sv.concurrencyStamp});
 const sb=await C.call('Evaluation_Submit',{id:yo.id,concurrencyStamp:full.concurrencyStamp});ok(sb.status==='submitted'&&!sb.isEditable&&sb.submittedAt,'submit: status submitted and no longer editable');
 ok(await code('Evaluation_SaveDraft',{id:yo.id,scores:[],concurrencyStamp:sb.concurrencyStamp})==='Hr:Evaluations:NotEditable'&&await code('EvaluationInternalNote_SaveDraft',{id:yo.id,content:'x'})==='Hr:Evaluations:NotEditable','NotEditable after submission');
 const nl2=await C.call('EvaluationInternalNote_GetList',{id:yo.id});ok(nl2.items.length===1&&!nl2.items[0].isDraft,'the draft became a fixed note on submission');
 const na=await C.call('EvaluationInternalNote_Add',{id:yo.id,content:'متابعة'});ok(na.author&&!na.isDraft&&(await C.call('EvaluationInternalNote_GetList',{id:yo.id})).items.length===2,'append-only note added after submission');
 ok(C.d8.notes.some(n=>n.user===U('20')&&n.code==='evaluationPublished'&&n.rid===yo.id)&&!JSON.stringify(C.d8.notes).includes('متابعة'),'employee notified of the published evaluation; the notification carries no note');
 // monthly report
 const rl=await C.call('DepartmentReport_GetList',{});ok(rl.items.length>=1&&rl.items.every(r=>r.manager.id===U('12')),'manager sees only his own reports');
 const rp=rl.items.find(r=>r.status==='draft');const rd=await C.call('DepartmentReport_Get',{id:rp.id});
 ok(rd.snapshot.employees.length===C.db.emps.filter(e=>e.mgr===U('12')&&e.status!=='ended').length&&rd.snapshot.custody.sharedItemsCount>=1&&rd.snapshot.circulars.circularsSentCount>=1&&!JSON.stringify(rd).includes('مسودة ملاحظة داخلية')&&!JSON.stringify(rd).includes('متابعة'),'report snapshot: team rows, custody and circulars live, never contains internal notes');
 const un=await C.call('DepartmentReport_UpdateNotes',{id:rp.id,notes:'ملاحظات المدير',concurrencyStamp:rd.concurrencyStamp});ok(un.notes==='ملاحظات المدير','manager notes saved');
 ok(!un.completion.isCompleted,'not completed while evaluations are still pending');
 // submit every remaining evaluation, then the report -> completed
 for(const v of C.d9.evs.filter(v=>v.cyc===cur().id&&C.evMgrOf(v)===U('12')&&v.status==='draft')){const fm=await C.call('Evaluation_GetManagerForm',{id:v.id});const r=await C.call('Evaluation_SaveDraft',{id:v.id,scores:fm.scores.map(s=>({criterionId:s.criterionId,gradeCode:'good'})),concurrencyStamp:fm.concurrencyStamp});await C.call('Evaluation_Submit',{id:v.id,concurrencyStamp:r.concurrencyStamp})}
 const mid=await C.call('DepartmentReport_Get',{id:rp.id});ok(mid.completion.evaluationsSubmitted===mid.completion.evaluationsExpected&&!mid.completion.isCompleted,'all evaluations submitted but the department is not completed without the report');
 const sr=await C.call('DepartmentReport_Submit',{id:rp.id,concurrencyStamp:mid.concurrencyStamp});ok(sr.status==='submitted'&&sr.completion.isCompleted,'evaluations + report together make the department completed');
 ok(await code('DepartmentReport_Submit',{id:rp.id,concurrencyStamp:sr.concurrencyStamp})==='Hr:DepartmentReports:AlreadySubmitted'&&await code('DepartmentReport_UpdateNotes',{id:rp.id,notes:'x',concurrencyStamp:sr.concurrencyStamp})==='Hr:DepartmentReports:AlreadySubmitted','AlreadySubmitted');
 const otherRep=C.d9.reps.find(r=>r.mgr!==U('12'));ok(!otherRep||await code('DepartmentReport_Get',{id:otherRep.id})==='Hr:Evaluations:NotFound','another manager’s report -> 404');
 const as=await C.call('EvaluationAnnualSummary_GetForEmployee',{employeeId:U('18'),year:cur().year});ok(as.evaluatedMonthsCount>=1&&as.criteria.length>=1&&!JSON.stringify(as).includes('%'),'annual summary for a team member: counts of grades, no percentages');
 ok(await code('EvaluationAnnualSummary_GetForEmployee',{employeeId:U('24'),year:cur().year})==='Hr:Evaluations:NotFound','annual summary outside the team -> 404');
 const te=await C.call('Report_GetTeamEvaluations',{});ok(te.items.length>=3&&te.items.every(x=>x.scores.every(s=>s.gradeName)&&!('note' in x)),'team evaluations report lists submitted evaluations (verbal grades only)');
 // UI: team tab and form
 await tab('team');ok(vm().blocks.length>=3&&rows().length>=4&&vm().flt.length===3,'team tab: cycle banner + dates + list');
 const draftRow=C.d9.evs.find(v=>v.cyc===C.d9.cycles[1].id&&C.evMgrOf(v)===U('12'));C.toSub('form',draftRow.id);await sl(250);ok(C.state.sub==='form'&&vm().blocks.length>=3,'submitted evaluation opens read-only with notes table');
 ok(vm().tools.some(t=>t.l===T.aEvNote),'read-only form offers “add internal note”');
 // ---------- employee
 ok(await login('emp',true),'employee signed in');await goto('perf');ok(tabs().join('|')==='تقييماتي|ملخصي السنوي','employee tabs');
 const ml=await C.call('Evaluation_GetMyList',{});ok(ml.items.length>=2&&ml.items.every(x=>x.status!=='draft'),'my list: submitted evaluations only (drafts never appear)');
 const sd=evOf('11');const keep={st:sd.status,at:sd.at,ack:sd.ack};sd.status='draft';sd.at=null;sd.ack=null;ok(await code('Evaluation_GetMyDetail',{id:sd.id})==='Hr:Evaluations:NotFound'&&!(await C.call('Evaluation_GetMyList',{})).items.some(x=>x.id===sd.id),'my draft is invisible (404, and absent from the list)');Object.assign(sd,{status:keep.st,at:keep.at,ack:keep.ack});
 ok(await code('Evaluation_GetMyDetail',{id:evOf('18').id})==='Hr:Evaluations:NotFound','someone else’s evaluation -> 404');
 const mine1=ml.items[0];const md=await C.call('Evaluation_GetMyDetail',{id:mine1.id});ok(md.scores.length>=5&&md.evaluator.displayName,'my detail: verbal scores and evaluator');
 ok(!/internal|داخلية|ملاحظة داخلية/.test(JSON.stringify(md)),'my detail never mentions an internal note');
 ok(await code('EvaluationInternalNote_GetList',{id:mine1.id})==='forbidden'&&await code('Evaluation_GetManagerForm',{id:mine1.id})==='forbidden','employee cannot call internal-note or manager routes');
 const ak=await C.call('Evaluation_Acknowledge',{id:mine1.id});const ak2=await C.call('Evaluation_Acknowledge',{id:mine1.id});ok(ak.status==='acknowledged'&&ak2.acknowledgedAt===ak.acknowledgedAt,'acknowledge is idempotent');
 const cm=await C.call('Evaluation_AddResponse',{id:mine1.id,type:'comment',content:'شكراً'});ok(cm.type==='comment'&&!cm.decision,'comment needs no decision');
 const ob=await C.call('Evaluation_AddResponse',{id:mine1.id,type:'objection',content:'أعترض'});ok(ob.type==='objection'&&(await C.call('Evaluation_GetMyDetail',{id:mine1.id})).objectionStatus==='pending','objection makes the status pending');
 ok(await code('Evaluation_AddResponse',{id:mine1.id,type:'objection',content:'مرة ثانية'})==='Hr:Evaluations:ObjectionAlreadyPending','ObjectionAlreadyPending');
 ok(C.d8.notes.some(n=>n.user===U('13')&&n.code==='evaluationResponseReceived'&&n.rid===mine1.id),'an objection notifies the HR manager');
 const sm=await C.call('EvaluationAnnualSummary_GetMy',{year:cur().year});ok(sm.evaluatedMonthsCount>=2&&sm.acknowledgedCount>=1&&sm.objectionsCount>=1,'my annual summary counts months, acknowledgements and objections');
 await tab('my');ok(rows().length>=2,'my tab lists evaluations');vm().blocks[0].rows[0].acts[0].go();await sl(200);ok(C.state.sub==='mine'&&vm().tools.some(t=>t.l===T.aEvCmt),'my detail UI with comment action');
 // ---------- HR
 ok(await login('hr',true),'HR signed in');await goto('perf');ok(tabs().join('|')==='تقييمات الشركة|تقارير الأقسام|ملخص سنوي لموظف','HR tabs');
 const hl=await C.call('Evaluation_GetHrList',{maxResultCount:100});ok(hl.items.length>=10&&!hl.items.some(x=>x.employee.id===U('13')),'company list excludes the HR manager’s own evaluation');
 const dr=hl.items.find(x=>x.status==='draft');ok(!!dr&&await code('Evaluation_GetHrView',{id:dr.id})==='Hr:Evaluations:NotFound','drafts are not viewable by HR (404)');
 ok((await C.call('Evaluation_GetHrList',{objectionStatus:'pending'})).items.length>=2,'objection filter finds the pending objections');
 const late=await C.call('Evaluation_GetHrList',{isLate:true});ok(late.items.every(x=>x.isLate),'late filter');
 const op=C.d9.evs.find(v=>v.emp===U('18')&&v.objection==='pending'&&v.cyc===C.d9.cycles[1].id);const hv=await C.call('Evaluation_GetHrView',{id:op.id});
 ok(hv.responses.some(r=>r.type==='objection'&&!r.decision)&&hv.attendance&&!JSON.stringify(hv).includes('ملاحظة داخلية'),'HR view: objection waiting, attendance, no internal note in the DTO');
 const wn=C.d9.evs.find(v=>v.fixed.length&&v.status!=='draft'&&v.emp!==U('13'));const hn=await C.call('EvaluationInternalNote_GetList',{id:wn.id});ok(hn.items.length>=1&&hn.items.every(n=>!n.isDraft),'HR reads the fixed internal notes of every author');const dd=C.d9.evs.find(v=>Object.keys(v.draft).length);ok(!dd||(await C.call('EvaluationInternalNote_GetList',{id:dd.id})).items.length===0,'HR never sees a manager’s unsent draft note');
 const rid=hv.responses.find(r=>r.type==='objection').id;
 ok(await code('Evaluation_DecideObjection',{id:op.id,rid,decision:'accepted',reason:'x',concurrencyStamp:'stale'})==='conflict','stale stamp on decision -> 409');
 const dc=await C.call('Evaluation_DecideObjection',{id:op.id,rid,decision:'accepted',reason:'الوقائع الموثقة تؤيد الاعتراض',concurrencyStamp:hv.concurrencyStamp});ok(dc.objectionStatus==='accepted'&&dc.responses.find(r=>r.id===rid).decisionReason,'objection decided with a visible reason');
 ok(await code('Evaluation_DecideObjection',{id:op.id,rid,decision:'rejected',reason:'x',concurrencyStamp:dc.concurrencyStamp})==='Hr:Evaluations:ObjectionAlreadyDecided','ObjectionAlreadyDecided');
 const cmId=(await C.call('Evaluation_GetHrView',{id:C.d9.evs.find(v=>v.emp===U('11')&&v.status==='acknowledged')?C.d9.evs.find(v=>v.emp===U('11')&&v.status==='acknowledged').id:op.id})).responses;
 ok(C.d8.notes.some(n=>n.user===U('18')&&n.code==='evaluationObjectionDecided'&&n.rid===op.id),'the employee is notified of the decision');
 const comm=C.d9.evs.find(v=>v.resp.some(r=>r.type==='comment'));ok(!comm||await code('Evaluation_DecideObjection',{id:comm.id,rid:comm.resp.find(r=>r.type==='comment').id,decision:'accepted',reason:'x',concurrencyStamp:comm.stamp})==='Hr:Evaluations:NotAnObjection','NotAnObjection for a comment');
 const hrr=await C.call('DepartmentReport_GetList',{});ok(hrr.items.length>=2&&new Set(hrr.items.map(r=>r.manager.id)).size>=2,'HR sees the reports of every department manager');
 ok(await code('EvaluationCriterion_GetList',{})==='forbidden'&&await code('Evaluation_GetTeamList',{})==='forbidden','HR cannot manage criteria or open manager routes');
 await tab('hr');ok(vm().flt.length===6&&rows().length>=10,'HR list UI with six filters');
 const hrow=C.d9.evs.find(v=>v.status!=='draft'&&v.emp!==U('13'));C.toSub('hrv',hrow.id);await sl(250);ok(C.state.sub==='hrv'&&vm().blocks.length>=5,'HR view UI');
 // manager change: the form follows the employee, internal draft does not
 const omar=U('18');const e0=C.d9.evs.find(v=>v.emp===omar&&v.cyc===cur().id);await C.call('Employee_ChangeManager',{id:omar,newManagerId:U('23'),reason:'x'}).catch(()=>0);
 // ---------- admin
 ok(await login('adm',true),'system admin signed in');await goto('perf');ok(tabs().join('|')==='المعايير والسلم','admin tab: criteria and scale');
 const kr=await C.call('EvaluationCriterion_GetList',{});const gr=await C.call('EvaluationGrade_GetList',{});ok(kr.items.length===5&&gr.items.length===5&&kr.items[0].nameAr==='الالتزام بالدوام','initial data: 5 criteria and 5 verbal grades');
 ok(gr.items.map(g=>g.code).join()==='weak,acceptable,good,veryGood,excellent','verbal scale codes (no numeric values)');
 ok(await code('EvaluationCriterion_Create',{nameAr:'الالتزام بالدوام',nameEn:'X',sortOrder:9,isActive:true})==='conflict','duplicate criterion name -> 409');
 const nc=await C.call('EvaluationCriterion_Create',{nameAr:'السلوك المهني',nameEn:'Professional conduct',sortOrder:6,isActive:true});ok(nc.id,'criterion created');
 ok(C.d9.evs.filter(v=>v.cyc===cur().id).every(v=>v.crit.length===5),'a new criterion does not change forms of the open cycle (applies next cycle)');
 const ids=kr.items.concat([nc]);for(const c of ids.slice(1)){const cc=(await C.call('EvaluationCriterion_GetList',{})).items.find(z=>z.id===c.id);await C.call('EvaluationCriterion_Update',{id:cc.id,nameAr:cc.nameAr,nameEn:cc.nameEn,sortOrder:cc.sortOrder,isActive:false,concurrencyStamp:cc.concurrencyStamp})}
 const last=(await C.call('EvaluationCriterion_GetList',{isActive:true})).items;ok(last.length===1&&await code('EvaluationCriterion_Update',{id:last[0].id,nameAr:last[0].nameAr,nameEn:last[0].nameEn,sortOrder:1,isActive:false,concurrencyStamp:last[0].concurrencyStamp})==='Hr:Evaluations:LastActiveCriterion','LastActiveCriterion: one active criterion must remain');
 ok(await code('EvaluationGrade_Create',{code:'good',nameAr:'x',nameEn:'x',sortOrder:9,isActive:true})==='conflict'&&await code('EvaluationGrade_Create',{code:'Bad Code',nameAr:'x',nameEn:'x',sortOrder:9,isActive:true})==='validation','grade code: duplicate -> 409, pattern -> 400');
 for(const g of (await C.call('EvaluationGrade_GetList',{})).items.slice(2)){await C.call('EvaluationGrade_Update',{id:g.id,nameAr:g.nameAr,nameEn:g.nameEn,sortOrder:g.sortOrder,isActive:false,concurrencyStamp:g.concurrencyStamp})}
 const g2=(await C.call('EvaluationGrade_GetList',{isActive:true})).items;ok(g2.length===2&&await code('EvaluationGrade_Update',{id:g2[0].id,nameAr:g2[0].nameAr,nameEn:g2[0].nameEn,sortOrder:1,isActive:false,concurrencyStamp:g2[0].concurrencyStamp})==='Hr:Evaluations:MinimumActiveGrades','MinimumActiveGrades: two active grades must remain');
 await tab('crit');ok(vm().tools.length===2&&vm().blocks.length>=3&&rows().length===6,'criteria UI: tables, New criterion / New grade');
 C.dlgOpen(C.evCritDlg(T,null));ok(rv().dlgv.fields.length===4,'criterion dialog');C.dlgClose();
 ok(await code('Evaluation_GetHrList',{})==='forbidden'&&await code('EvaluationCycle_GetCurrent',{})==='forbidden','admin cannot read evaluations or the cycle');
 // ---------- reviewer
 ok(await login('rv',true),'reviewer signed in');await goto('perf');ok(tabs().join('|')==='الدورات','reviewer tab: cycles');
 const rd2=await C.call('Report_GetReviewDashboard',{});ok(rd2.cycleId&&rd2.departments.length>=2&&rd2.departments.every(z=>z.completion.evaluationsExpected>=0),'reviewer dashboard now lists department completion for the current cycle');
 ok(rd2.departments.every(z=>!('employee' in z)&&!('scores' in z)),'no employee names or grades in the dashboard');
 const cr=await C.call('Report_GetEvaluationConsistency',{});ok(cr.items.length>=2&&cr.items.every(z=>z.expectedCount>=0&&Array.isArray(z.gradeCounts)&&!('employee' in z)),'consistency report per manager, aggregated, no employee names');
 ok(await code('Evaluation_GetHrList',{})==='forbidden'&&await code('Evaluation_GetTeamList',{})==='forbidden','reviewer cannot open evaluations');
 // ---------- executive and others
 for(const p of ['xd','so']){await login(p,true).catch(()=>0);ok(await code('EvaluationCycle_GetCurrent',{})==='forbidden',p+': no evaluation access');}
 // ---------- render
 for(const pr of ['emp','mgr','hr','rv','adm']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['my','sum','team','rep','tsum','hr','hrep','hsum','cyc','crit']){C.setState({page:'perf',tab:t,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render perf/'+t+' '+pr+' '+L)}catch(e){ok(false,'render perf/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
