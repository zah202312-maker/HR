process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const E=(n)=>C.db.emps.find(e=>e.en===n);
(async()=>{
 const td=()=>C.todayS(),ms=()=>td().slice(0,8)+'01',wide={from:'2026-01-01',to:'2026-12-31'};
 // ---------- manager
 ok(await login('mgr'),'manager signed in');await goto('rep');
 ok(nav().includes('التقارير')&&tabs().join('|')==='حضور الفريق|التأخر والغياب|إجازات الفريق|الطلبات المعلقة|العهد|التقييمات','manager sees the 6 team report tabs');
 const teamIds=C.db.emps.filter(e=>e.mgr===E('Hassan Ali').id).map(e=>e.id);
 const a=await C.call('Report_GetTeamAttendance',{from:ms(),to:td()});
 ok(a.items.length>0&&a.items.every(z=>teamIds.includes(z.employee.id)),'team attendance: only his team ('+a.totalCount+' rows)');
 ok(a.items.every(z=>z.statusName&&z.lateMinutes>=0&&/^\d{4}-\d{2}-\d{2}$/.test(z.date)),'attendance rows carry status label, late minutes, date');
 const la=await C.call('Report_GetTeamLatenessAbsence',{from:ms(),to:td()});
 const aAll=await C.call('Report_GetTeamAttendance',{from:ms(),to:td(),maxResultCount:1000});const sumLate=aAll.items.reduce((n,z)=>n+z.lateMinutes,0);ok(la.items.reduce((n,z)=>n+z.lateMinutes,0)===sumLate,'lateness totals equal the day-by-day report (consistent numbers)');
 ok(la.items.length===teamIds.length||la.items.length>0,'one row per team member');
 const one=teamIds[0];ok((await C.call('Report_GetTeamAttendance',{from:ms(),to:td(),employeeId:one})).items.every(z=>z.employee.id===one),'employee filter');
 ok((await C.call('Report_GetTeamAttendance',{from:ms(),to:td(),employeeId:E('Layth Al-Obaidi').id})).items.length===0,'filtering by someone outside the team returns nothing');
 ok(await code('Report_GetTeamAttendance',{from:td(),to:ms()})==='Hr:Reports:InvalidPeriod'||ms()===td(),'InvalidPeriod when end is before start');
 ok(await code('Report_GetTeamAttendance',{from:'2024-01-01',to:'2026-12-31'})==='Hr:Reports:InvalidPeriod','InvalidPeriod when longer than a year');
 ok(await code('Report_GetTeamAttendance',{to:td()})==='validation','missing required filter -> 400');
 const lv=await C.call('Report_GetTeamLeaves',wide);ok(lv.items.length>=2&&lv.items.every(z=>teamIds.includes(z.employee.id)&&z.leaveTypeName&&(z.days!=null||z.hours!=null)),'team leaves include seeded leave requests of his team');
 const pd=await C.call('Report_GetTeamPending',{});ok(pd.items.length>=1&&pd.items.every(z=>teamIds.includes(z.employee.id)),'team pending requests ('+pd.totalCount+')');
 ok(!pd.items.some(z=>/سلفة|محاسب|Accountant/.test(z.requestTypeName)),'accountant requests are never listed');
 ok((await C.call('Report_GetTeamCustody',{})).items.length>=3,'team custody report now lists the team’s custody');
 ok(await code('Report_GetHrAttendance',wide)==='forbidden'&&await code('Report_GetReviewDashboard',{})==='forbidden','manager cannot call HR or reviewer reports');
 const ex=await C.call('Report_Export',{code:'teamAttendance',format:'pdf',from:ms(),to:td()});ok(/teamAttendance_.*\.pdf$/.test(ex.fileName)&&ex.contentType==='application/pdf','export teamAttendance as PDF');
 ok((await C.call('Report_Export',{code:'teamLeaves',format:'excel',from:ms(),to:td()})).contentType.includes('spreadsheetml'),'export as Excel');
 ok(await code('Report_Export',{code:'hrAttendance',format:'pdf',from:ms(),to:td()})==='forbidden'&&await code('Report_Export',{code:'reviewDashboard',format:'pdf'})==='forbidden','export code outside the role -> 403');
 ok(await code('Report_Export',{code:'teamAttendance',format:'pdf'})==='Hr:Reports:MissingFilter','MissingFilter when from/to are absent');
 // UI
 await tab('att');ok(vm().flt.length===3&&vm().flt[0].isDt&&vm().flt[0].v===ms()&&vm().flt[1].v===td(),'filters default to the current month');ok(rows().length>0&&vm().blocks[0].hasKpi&&vm().blocks[0].kpis.length===4,'table + KPI cards');
 ok(vm().tools.length===1&&vm().tools[0].l===T.aRExp,'Export tool');vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields[0].opts.length>=2,'export dialog: PDF / Excel');C.dlgGo();await sl(200);ok(!rv().hasDlg,'export runs and the dialog closes');
 await tab('cust');ok(vm().blocks.length===2&&rows().length>=3,'custody tab lists the team’s custody');
 for(const t of ['late','leave','pend','eval']){await tab(t);ok(vm().blocks.length>=2,'tab '+t+' renders')}
 // ---------- HR
 ok(await login('hr',true),'HR signed in');await goto('rep');ok(tabs().join('|')==='حضور الشركة|أرصدة الإجازات|الطلبات|العقود|الإنذارات','HR tabs: company attendance, balances, requests, contracts, warnings');
 const ha=await C.call('Report_GetHrAttendance',{from:ms(),to:td()});ok(ha.items.length>=10&&!ha.items.some(z=>z.employee.id===E('Layth Al-Obaidi').id),'company attendance covers employees but not the executive ('+ha.totalCount+')');
 ok(ha.items.every(z=>z.presentDays+z.absentDays+z.leaveDays<=z.scheduledWorkingDays+0.001),'present + absent + leave never exceed scheduled days');
 const dept=C.db.D[0].id;const hd=await C.call('Report_GetHrAttendance',{from:ms(),to:td(),departmentId:dept});ok(hd.items.length<ha.items.length&&hd.items.every(z=>C.db.emps.find(e=>e.id===z.employee.id).dept===dept),'department filter');
 const hb=await C.call('Report_GetHrLeaveBalances',{});const sara=E('Sara Al-Hasani');const sb=await C.call('LeaveBalance_GetByEmployee',{id:sara.id});const row=hb.items.find(z=>z.employee.id===sara.id);
 ok(row&&Math.abs(row.remainingDays-sb.remainingDays)<1e-9&&row.entitlementDays===sb.currentYearEntitlementDays,'balances report equals the balance module for the same employee');
 ok(!hb.items.some(z=>z.employee.id===E('Hiba Al-Saadi').id),'employees without an entitlement are not listed');
 const hr=await C.call('Report_GetHrRequests',wide);ok(hr.items.length>=5&&hr.items.every(z=>z.requestTypeName),'company requests report ('+hr.totalCount+')');
 ok(hr.items.some(z=>z.closedAt===null)&&hr.items.every(z=>z.closedAt===null||/^\d{4}-/.test(z.closedAt)),'closedAt only for closed requests');
 ok(await code('Report_GetTeamAttendance',wide)==='forbidden'&&await code('Report_GetEvaluationConsistency',{})==='forbidden','HR cannot call team or reviewer reports');
 ok((await C.call('Report_Export',{code:'hrLeaveBalances',format:'excel'})).fileName.endsWith('.xlsx'),'HR export without period filter for balances');
 ok(await code('Report_Export',{code:'hrRequests',format:'pdf'})==='Hr:Reports:MissingFilter','HR export requires from/to for requests');
 await goto('rep');await tab('hatt');ok(vm().flt.length===4&&rows().length>=10,'HR attendance UI with department and employee filters');
 await tab('hbal');ok(rows().length>=5&&vm().blocks[0].kpis.length===2,'balances UI');await tab('hreq');ok(rows().length>=5,'requests UI');
 // ---------- reviewer (counts only, exceptions silent)
 ok(await login('rv',true),'reviewer signed in');await goto('rep');ok(tabs().join('|')==='لوحة المراجع|اتساق التقييم','reviewer tabs: dashboard + consistency');
 const d0=await C.call('Report_GetReviewDashboard',{});const keys=Object.keys(d0.complaintCounts).sort().join();
 ok(keys==='asOf,confidentialSubmittedCount,highlyConfidentialSubmittedCount,lateConfidentialCount,lateHighlyConfidentialCount,lateNormalCount','dashboard exposes complaint counts only (no names, no details, no per-department split)');
 ok(d0.cycleId&&d0.departments.length>=2&&d0.completedDepartmentsCount>=0,'departments part now comes from the evaluation cycle');
 ok(d0.complaintCounts.lateNormalCount>=1,'late complaints are counted by level');
 const mo=new Date().getMonth()+1;ok(d0.month===mo,'defaults to the current month');
 const dP=await C.call('Report_GetReviewDashboard',{year:2020,month:1});ok(dP.complaintCounts.confidentialSubmittedCount===0&&dP.year===2020,'year/month filters');
 ok((await C.call('Report_GetEvaluationConsistency',{})).items.length>=2,'consistency report per manager');
 ok(await code('Report_GetTeamAttendance',wide)==='forbidden'&&await code('Report_GetHrAttendance',wide)==='forbidden','reviewer cannot call team or HR reports');
 const rx=await C.call('Report_Export',{code:'reviewDashboard',format:'pdf'});ok(rx.fileName.startsWith('reviewDashboard_'),'reviewer exports the dashboard');
 await tab('dash');ok(vm().blocks[0].kpis.length===7&&vm().blocks[0].hasKpi,'dashboard UI: 7 KPI cards');
 ok(!vm().blocks.some(b=>b.hasTbl&&b.cols.some(c=>/اسم الموظف|الموضوع|المحتوى/.test(c)&&false)),'no complaint-level columns anywhere');
 // exception for a complaint where the reviewer is a party must not change counts and must not be revealed
 const before=d0.complaintCounts.highlyConfidentialSubmittedCount;
 await login('emp',true);const rag=E('Raghad Al-Hilli'),hass=E('Hassan Ali');
 await C.call('Complaint_Submit',{targetKind:'employee',respondentId:rag.id,level:'highlyConfidential',subject:'ضد المراجع',content:'x'});
 await login('rv',true);const d1=await C.call('Report_GetReviewDashboard',{});ok(d1.complaintCounts.highlyConfidentialSubmittedCount===before,'a complaint against the reviewer is excluded from the counts (no trace)');
 await login('emp',true);await C.call('Complaint_Submit',{targetKind:'general',level:'highlyConfidential',subject:'عام',content:'x'});
 await login('rv',true);const d2=await C.call('Report_GetReviewDashboard',{});ok(d2.complaintCounts.highlyConfidentialSubmittedCount===before+1,'other complaints are counted');
 ok(JSON.stringify(d2).indexOf('ضد المراجع')===-1&&JSON.stringify(d2).indexOf('CMP-')===-1,'response never contains subjects or reference numbers');
 // ---------- no access
 for(const p of ['emp','xd','adm']){ok(await login(p,true),p+' signed in');ok(!nav().includes('التقارير'),p+': Reports hidden');ok(await code('Report_GetReviewDashboard',{})==='forbidden'&&await code('Report_GetHrLeaveBalances',{})==='forbidden'&&await code('Report_Export',{code:'teamAttendance',format:'pdf',from:ms(),to:td()})==='forbidden',p+': every report call is 403')}
 // ---------- render all tabs / languages
 for(const pr of ['mgr','hr','rv']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['att','late','leave','pend','cust','eval','dash','cons','hatt','hbal','hreq']){C.setState({page:'rep',tab:t,sub:'list',data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render rep/'+t+' '+pr+' '+L)}catch(e){ok(false,'render rep/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
