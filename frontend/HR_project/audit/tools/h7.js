const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+';globalThis.__C=Component;');const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const login=async(k)=>{C.signOut();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
(async()=>{
 await login('emp');const my=await C.call('Attendance_GetMy',{});const d=my.items.find(z=>['present','late'].includes(z.status)&&!z.isCorrected);
 const cr=await C.call('AttendanceCorrection_Create',{date:d.date,punchType:'checkIn',correctTime:'08:00',reason:'سهو بصمة'});ok(cr.status==='pendingManager','correction created in module 3');
 const mine=await C.call('Request_GetMy',{});ok(mine.items.some(x=>x.id===cr.requestId&&x.type.code==='attendanceCorrection'),'correction appears in the unified requests list');
 await login('mgr');let ib=(await C.call('Request_GetInbox',{})).items;ok(ib.some(x=>x.id===cr.requestId),'correction appears in the manager inbox');
 const a1=await C.call('Request_Approve',{id:cr.requestId});ok(a1.status==='pendingHr'&&a1.detailsSummary.length>0,'manager approves correction');
 await login('hr');const a2=await C.call('Request_Approve',{id:cr.requestId});ok(a2.status==='approved','HR final approval');
 const day=C.dayOf(C.db.emps.find(e=>e.en==='Sara Al-Hasani'),d.date);ok(day.isCorrected===true,'attendance day is now marked corrected (module 3 updated by module 5)');
 const c2=await C.call('AttendanceCorrection_Get',{id:cr.requestId});ok(c2.status==='approved','AttendanceCorrection_Get reflects the decision');
 // HR own leave -> executive
 const w=(()=>{let x=C.dAdd(C.todayS(),70);for(let i=0;i<10;i++){const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(x+'T12:00:00Z').getUTCDay()];if(!['friday','saturday'].includes(dn))return x;x=C.dAdd(x,1)}return x})();
 const hl=await C.call('LeaveRequest_Create',{kind:'annual',startDate:w,endDate:C.dAdd(w,1)});ok(hl.status==='pendingExecutive','HR manager leave goes to the executive');
 await login('xd');ib=(await C.call('Request_GetInbox',{})).items;ok(ib.some(x=>x.id===hl.requestId),'executive inbox has the HR leave request');
 const ex=await C.call('Request_Approve',{id:hl.requestId});ok(ex.status==='approved'&&ex.steps.length===1,'executive approves -> approved');
 await login('hr');const lv=C.d4.reqs.find(r=>r.id===hl.requestId);ok(lv.effect==='deducted'&&lv.status==='approved','HR leave balance deducted after executive approval');
 const cx=await C.call('Request_Cancel',{id:hl.requestId});
 console.log(out.join('\n'));
})().catch(e=>{console.log('ERR',e&&e.code||e&&e.stack||JSON.stringify(e));console.log(out.join('\n'))});
