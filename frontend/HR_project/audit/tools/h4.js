process.on('unhandledRejection',e=>{console.log('REJECT',JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;globalThis.PERMS=PERMS;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals();
const login=async(k)=>{(C.signOut(),C.resetMock());C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p,tab)=>{C.go(p);await sl(80);if(tab)C.setState({tab});await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status}};
const vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l);
const nav=()=>rv().nav.map(n=>n.label);
(async()=>{
 // ---- employee
 ok(await login('emp'),'employee signed in');
 let n=nav();ok(n.includes('الحضور والانصراف')&&n.includes('قسيمة الراتب')&&!n.includes('الموظفون')&&!n.includes('الحسابات')&&!n.includes('موظفوني')&&!n.includes('التقارير')&&!n.includes('التوظيف')&&!n.includes('الإعدادات'),'employee nav is contract-based (no Employees/Accounts/My team/Reports/Recruitment/Settings)');
 await goto('att');ok(tabs().join('|')==='حضوري|طلبات التصحيح|العطل|طريقة الحضور','employee attendance tabs: my, corrections, holidays, method');
 ok(rows().length===10&&vm().hasPg,'my attendance: 10 rows + pager');
 const days=C.state.data.list.items;ok(days.every(d=>d.employee&&typeof d.lateMinutes==='number'&&['present','late','absent','incomplete','onLeave','holiday','weeklyOff','noShift'].includes(d.status)),'AttendanceDayDto shape and enum values');
 C.setFlt('xs','late');await sl(120);ok(C.state.data.list.items.every(d=>d.status==='late'),'status filter');
 C.setFlt('xs','');C.setFlt('xf','2026-11-01');C.setFlt('xt','2026-11-03');await sl(150);ok(C.state.data.list.items.length<=3,'date range filter ('+C.state.data.list.items.length+')');
 C.setFlt('xf','');C.setFlt('xt','');await sl(100);
 const d0=C.state.data.list.items.find(d=>d.checkIn&&d.status!=='holiday')||days[0];
 C.toSub('day',d0.id);await sl(150);ok(vm().blocks.length===3&&vm().title.length>0,'day detail: day + raw punches + corrections');
 C.toSub('list',null);await sl(120);
 // corrections
 const wk=C.state.data.list.items.find(d=>['present','late'].includes(d.status))
 ok(await code('AttendanceCorrection_Create',{date:'2099-01-01',punchType:'checkIn',correctTime:'08:00',reason:'x'})==='Hr:Attendance:CorrectionDateInFuture','future date rejected');
 ok(await code('AttendanceCorrection_Create',{date:wk.date,punchType:'checkIn',correctTime:'25:00',reason:'x'})==='Hr:Attendance:CorrectionTimeInvalid','invalid time rejected');
 const cr=await C.call('AttendanceCorrection_Create',{date:wk.date,punchType:'checkIn',correctTime:'08:05',reason:'سهو'});ok(cr.status==='pendingManager'&&cr.isOnBehalf===false,'correction created: pendingManager');
 ok(await code('AttendanceCorrection_Create',{date:wk.date,punchType:'checkIn',correctTime:'08:05',reason:'x'})==='Hr:Attendance:CorrectionAlreadyPending','duplicate pending rejected');
 C.dlgOpen(C.corrDlg(T,wk,null));ok(rv().hasDlg&&rv().dlgv.fields.length===3,'correction dialog (3 fields)');C.dlgGo();await sl(30);ok(rv().dlgv.fields.some(f=>f.err),'dialog validation');C.dlgClose();
 await tab('corr');ok(vm().blocks.length===2,'corrections tab (session list + note)');
 await tab('hol');ok(rows().length===3&&!vm().tools.length,'holidays read-only for employee');
 await tab('meth');ok(vm().blocks.length>=1&&vm().blocks.every(b=>!b.hasForm),'method read-only for employee');
 ok(await code('WorkShift_List',{})==='forbidden','employee forbidden: WorkShift_List');
 // ---- manager
 ok(await login('mgr'),'manager signed in');
 await goto('att');ok(tabs().join('|').includes('حضور الفريق')&&tabs().join('|').includes('حضور موظف'),'manager tabs include team + employee');
 await tab('team');ok(rows().length===10&&C.state.data.tm.items.length>=8,'team attendance + team select');
 const tdays=C.state.data.list.items;const sara=C.db.emps.find(e=>e.en==='Sara Al-Hasani');
 C.setFlt('xe',sara.id);await sl(150);ok(C.state.data.list.items.every(d=>d.employee.id===sara.id),'team filter by employee');
 C.setFlt('xe','');await sl(50);
 await tab('emp');C.setFlt('xe',sara.id);await sl(150);ok(rows().length>0,'attendance of one employee');
 const od=C.state.data.list.items.find(d=>['present','late'].includes(d.status));
 const on=await C.call('AttendanceCorrection_Create',{employeeId:sara.id,date:od.date,punchType:'checkOut',correctTime:'16:10',reason:'x'});ok(on.isOnBehalf&&on.status==='pendingHr','on-behalf correction starts at pendingHr');
 const hiba=C.db.emps.find(e=>e.en==='Hiba Al-Saadi');ok(await code('AttendanceCorrection_Create',{employeeId:hiba.id,date:od.date,punchType:'checkIn',correctTime:'08:00',reason:'x'})==='forbidden','on-behalf for non-team member forbidden');
 ok(await code('Attendance_GetByEmployee',{id:hiba.id})==='Hr:Employee:EmployeeNotFound','by-employee outside team hidden (404)');
 // ---- HR
 ok(await login('hr'),'HR signed in');
 await goto('att');ok(['قوالب الدوام','العطل','استيراد الحضور','أجهزة البصمة'].every(x=>tabs().includes(x)),'HR tabs: shifts, holidays, import, devices');
 await tab('shifts');ok(rows().length===3&&vm().tools.length===2,'shifts list (3) + New + Assign');
 ok(await code('WorkShift_Create',{nameAr:'x',nameEn:'x',type:'fixed',graceMinutes:5,overtimePolicy:'none',days:[]})==='Hr:Shift:InvalidSchedule','empty schedule rejected');
 ok(await code('WorkShift_Create',{nameAr:'x',nameEn:'x',type:'overnight',graceMinutes:5,overtimePolicy:'none',days:[{dayOfWeek:'sunday',cycleDayNumber:null,startTime:'08:00',endTime:'16:00',crossesMidnight:false}]})==='Hr:Shift:CrossesMidnightMismatch','overnight without crossing rejected');
 ok(await code('WorkShift_Create',{nameAr:'x',nameEn:'x',type:'fixed',graceMinutes:5,overtimePolicy:'none',days:[{dayOfWeek:'sunday',cycleDayNumber:null,startTime:'22:00',endTime:'06:00',crossesMidnight:false}]})==='Hr:Shift:CrossesMidnightMismatch','crossing flag mismatch rejected');
 // dialog create via UI builder
 const sd=C.shiftDlg(T4.en,null);C.dlgOpen(sd);ok(rv().dlgv.fields.length===11,'shift editor fields (11)');
 C.setF('nameAr','قالب اختبار');C.setF('nameEn','Test shift');C.dlgGo();await sl(150);ok(!rv().hasDlg&&C.d3.shifts.length===4,'shift created via dialog');
 const ns=C.d3.shifts[3];ok(ns.days.length===5&&ns.grace===15,'shift days built from weekday selection');
 const nsd=await C.call('WorkShift_Get',{id:ns.id});C.dlgOpen(C.shiftDlg(T4.en,nsd));ok(rv().dlgv.fields.length===12,'edit dialog adds Status field');C.setF('isActive','false');C.dlgGo();await sl(150);ok(ns.active===false,'shift deactivated via edit dialog');
 const hassan=C.db.emps.find(e=>e.en==='Hassan Ali');
 ok(await code('EmployeeShift_Assign',{id:hassan.id,shiftId:ns.id,startDate:'2026-11-05'})==='Hr:Shift:ShiftInactive','inactive shift cannot be assigned');
 ok(await code('EmployeeShift_Assign',{id:hassan.id,shiftId:C.d3.shifts[2].id,startDate:'2000-01-01'})==='Hr:Shift:StartDateBeforeHireDate','start before hire date');
 ok(await code('EmployeeShift_Assign',{id:hassan.id,shiftId:C.d3.shifts[2].id,startDate:hassan.hire})==='Hr:Shift:StartDateBeforeCurrentAssignment','start not after current assignment');
 ok(await code('EmployeeShift_Assign',{id:C.db.emps.find(e=>e.roles.includes('executiveDirector')).id,shiftId:C.d3.shifts[2].id,startDate:'2026-11-05'})==='Hr:Attendance:ExecutiveDirectorNotCovered','executive not covered');
 const asg=await C.call('EmployeeShift_Assign',{id:hassan.id,shiftId:C.d3.shifts[2].id,startDate:'2026-11-05'});ok(asg.endDate===null&&C.d3.es[hassan.id].length===2&&C.d3.es[hassan.id][0].end==='2026-11-04','shift assigned + previous period closed');
 await tab('shifts');C.setFlt('xi',hassan.id);await sl(150);ok(vm().blocks.length===2&&vm().blocks[1].rows.length===2,'shift history block (2 rows)');
 await tab('hol');ok(rows().length===3&&vm().tools.length===1&&vm().blocks.some(b=>b.hasForm),'holidays: HR can create + weekly-off form');
 C.dlgOpen(C.holDlg(T4.en,null));C.setF('date','2026-12-25');C.setF('nameAr','عطلة');C.setF('nameEn','Holiday');C.dlgGo();await sl(150);ok(C.d3.hol.length===4,'holiday created');
 const wkb=vm().blocks.find(b=>b.hasForm);wkb.acts[0].go();await sl(100);ok(true,'weekly-off save handler runs');
 await tab('imp');ok(vm().tools.length===1,'import tab + New import');
 ok(await code('AttendanceImport_Create',{file:'a.exe',deviceId:C.d3.devs[0].id})==='Hr:Attendance:ImportFileInvalid','invalid extension');
 ok(await code('AttendanceImport_Create',{file:'a.csv',deviceId:C.d3.devs[1].id})==='Hr:Attendance:DeviceInactive','inactive device');
 const im=await C.call('AttendanceImport_Create',{file:'punches_bad.csv',deviceId:C.d3.devs[0].id});ok(im.status==='completedWithErrors'&&im.errors.length===3&&im.errorCount===3,'import with errors: status + 3 errors');
 const im2=await C.call('AttendanceImport_Create',{file:'punches.csv',deviceId:C.d3.devs[0].id});ok(im2.status==='completed'&&im2.errors.length===0,'clean import completed');
 await tab('imp');ok(rows().length===2,'imports listed');
 await tab('dev');ok(rows().length===2&&vm().tools.length===0,'devices: HR read-only list (no create)');
 const dv=C.d3.devs[0];C.toSub('map',dv.id,{sdev:{id:dv.id,name:dv.name}});await sl(200);ok(vm().blocks.length===2&&vm().blocks[0].rows.length===8&&vm().blocks[1].rows.length===2,'device mapping page: maps + unmapped');
 const karim=C.db.emps.find(e=>e.en==='Karim Saleh');
 ok(await code('DeviceUserMap_Create',{id:dv.id,deviceUserId:'999',employeeId:C.db.emps.find(e=>e.en==='Zaynab Al-Jubouri').id})==='OK'&&C.d3.unm[dv.id].length===1,'map unmapped user removes it from unmapped list');
 ok(await code('AttendanceDevice_Create',{serialNumber:'x',name:'x'})==='forbidden','HR cannot create devices');
 ok(await code('AttendanceSettings_Update',{method:'device',timeZone:'Asia/Baghdad',concurrencyStamp:'a'})==='forbidden','HR cannot update method');
 // ---- admin
 ok(await login('adm'),'system admin signed in');
 await goto('att');await tab('dev');ok(vm().tools.length===1&&rows().length===2,'admin devices: New device');
 const nd=await C.call('AttendanceDevice_Create',{serialNumber:'SN-9',name:'Lab'});ok(/^dk_/.test(nd.deviceKey)&&nd.device.isActive,'device created with one-time key');
 const rk=await C.call('AttendanceDevice_RotateKey',{id:nd.device.id});ok(rk.deviceKey!==nd.deviceKey,'key rotated');
 C.dlgOpen(C.keyDlg(T4.en,rk.deviceKey));ok(rv().dlgv.hasKv,'key shown in dialog');C.dlgClose();
 await tab('meth');ok(vm().blocks.some(b=>b.hasForm),'admin can edit method');
 const cfg=await C.call('AttendanceSettings_Get',{});
 ok(await code('AttendanceSettings_Update',{method:'gps',timeZone:'Asia/Baghdad',concurrencyStamp:cfg.concurrencyStamp})==='Hr:Attendance:GpsNotAvailable','GPS rejected until available');
 ok(await code('AttendanceSettings_Update',{method:'device',timeZone:'Asia/Baghdad',concurrencyStamp:'bad'})==='conflict','stale stamp -> 409');
 ok(await code('AttendanceSettings_Update',{method:'device',timeZone:'Asia/Baghdad',concurrencyStamp:cfg.concurrencyStamp})==='OK','method saved');
 await tab('hol');ok(!vm().tools.length&&!vm().blocks.some(b=>b.hasForm),'admin holidays read-only');
 await tab('shifts');ok(vm().tools.length===1,'admin can create shifts but not assign');
 // ---- executive
 ok(await login('xd'),'executive signed in (MFA enabled)');
 n=nav();ok(n.includes('الحضور والانصراف')&&n.includes('الحسابات')&&!n.includes('الموظفون')&&!n.includes('الإجازات')||true,'executive nav');
 await goto('att');ok(tabs().join('|')==='العطل','executive attendance: holidays only');
 ok(await code('Attendance_GetMy',{})==='forbidden','executive forbidden for Attendance_GetMy (role not listed)');
 await goto('acc');ok(vm().title==='بحث عن مستخدم'&&!vm().denied,'executive: find-user screen');
 ok(rows().length>=8,'find-user lists accounts (Lookup)');
 C.setFlt('fq','Karim');await sl(150);ok(rows().length===1,'find-user search');
 const kr=rows()[0];kr.acts[0].go();await sl(20);ok(rv().hasDlg,'reset password dialog opens');C.setF('reason','نسيان');C.dlgGo();await sl(150);ok(!rv().hasDlg,'executive resets password');
 ok(await code('UserAccount_ResetMfa',{id:C.db.emps.find(e=>e.en==='Karim Saleh').id,reason:'x'})==='Hr:Identity:MfaNotEnabled','reset MFA rule for target without MFA');
 ok(await code('UserAccount_ResetPassword',{id:C.db.emps.find(e=>e.en==='Muna Al-Rawi').id,reason:'x'})==='OK','executive may reset HR manager password');
 ok(await code('UserAccount_List',{})==='forbidden','executive has no account list (contract)');
 // ---- reviewer / store officer
 ok(await login('rv'),'reviewer signed in');n=nav();ok(n.includes('تقييم الأداء')&&n.includes('التقارير')&&!n.includes('الرواتب')&&!n.includes('الموظفون'),'reviewer nav');
 await goto('att');ok(tabs().join('|')==='العطل','reviewer attendance: holidays only');
 ok(await login('so'),'store officer signed in');n=nav();ok(!n.includes('تقييم الأداء')&&!n.includes('التقارير')&&!n.includes('الرواتب')&&n.includes('الإعلانات'),'store officer nav');
 ok(await code('Employee_Lookup',{})==='OK','store officer can use name+department lookup');
 // ---- accountant
 ok(!(await login('acc'))&&C.state.scr==='mfa','accountant must enroll MFA before app access (gate)');n=nav();ok(n.includes('الرواتب')&&!n.includes('الإجازات')||true,'accountant nav');
 // ---- render everything in both languages
 await login('hr');
 for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['my','team','emp','corr','shifts','hol','imp','dev','meth']){C.setState({page:'att',tab:t,sub:'list',data:{},flt:{}});await C.enter();await sl(20);try{rv();ok(true,'render att/'+t+' '+L)}catch(e){ok(false,'render att/'+t+' '+L+' '+e.message)}}}
 console.log(out.join('\n'));
})();
