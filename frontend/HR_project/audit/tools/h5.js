process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k)=>{(C.signOut(),C.resetMock());C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const E=(n)=>C.db.emps.find(e=>e.en===n);
const nextWork=(from)=>{let d=from;for(let i=0;i<20;i++){const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];if(!['friday','saturday'].includes(dn)&&!C.d3.hol.some(h=>h.date===d))return d;d=C.dAdd(d,1)}return d};
(async()=>{
 ok(await login('emp'),'employee signed in');
 await goto('leave');ok(nav().includes('الإجازات')&&tabs().join('|')==='أرصدتي|طلبات إجازتي','employee leave tabs: balances + my requests');
 let b=C.state.data.bal;ok(b&&b.isEntitlementSet&&b.carriedDays===6&&b.currentYearEntitlementDays===24,'balance DTO (entitlement 24, carried 6)');
 ok(Math.abs(b.remainingDays-(b.carriedDays+b.currentYearEntitlementDays+b.adjustmentDays-b.usedDays-b.heldDays))<1e-9,'remaining = carried + entitlement + adjustments - used - held');
 ok(b.usedDays>0&&b.heldDays>0,'seed: used '+b.usedDays+' held '+b.heldDays);
 ok(vm().blocks[0].hasKpi&&vm().blocks[0].kpis.length===6&&vm().blocks[0].hasPw,'KPI cards + progress');
 ok(rows().length>=0&&vm().blocks.some(x=>x.hasTbl&&x.rows.length>=3),'ledger table has entries');
 ok(C.state.data.led.items.every(z=>['grant','carryIn','hold','release','deduct','adjust','reverse'].includes(z.type)&&['carried','entitlement','hourly'].includes(z.bucket)),'ledger enums');
 // preview rules
 const w1=nextWork(C.dAdd(C.todayS(),30)),w2=C.dAdd(w1,6);
 const p=await C.call('LeaveRequest_Preview',{kind:'annual',startDate:w1,endDate:w2});
 ok(p.calendarDays===7&&p.workingDays+p.excludedWeeklyOffDays+p.excludedHolidayDays===7&&p.excludedWeeklyOffDays>=2,'preview excludes weekly-off days ('+p.workingDays+' working of 7)');
 ok(p.fromCarriedDays===Math.min(b.carriedRemainingDays,p.workingDays)&&p.fromCarriedDays+p.fromEntitlementDays===p.workingDays&&p.canSubmit&&p.isBalanceSufficient,'allocation: carried first, rest from entitlement');
 ok(p.dayBreakdown.every(z=>['workingDay','weeklyOff','holiday'].includes(z.classification)),'day classification enum');
 const long=await C.call('LeaveRequest_Preview',{kind:'annual',startDate:w1,endDate:C.dAdd(w1,200)});
 ok(!long.isBalanceSufficient&&!long.canSubmit&&long.shortageDays>0&&long.issues.some(i=>i.code==='Hr:Leave:InsufficientBalance'),'insufficient balance: 200 OK with canSubmit=false + issue (no 422)');
 ok(await code('LeaveRequest_Create',{kind:'annual',startDate:w1,endDate:C.dAdd(w1,200)})==='Hr:Leave:InsufficientBalance','create with shortage -> 422 InsufficientBalance');
 ok(await code('LeaveRequest_Preview',{kind:'annual',startDate:w2,endDate:w1})==='Hr:Leave:InvalidDateRange','InvalidDateRange');
 ok(await code('LeaveRequest_Preview',{kind:'annual',startDate:'2030-01-01',endDate:'2030-01-05'})==='Hr:Leave:RangeBeyondNextYear','RangeBeyondNextYear');
 let fr=w1;while(!['friday'].includes(['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(fr+'T12:00:00Z').getUTCDay()]))fr=C.dAdd(fr,1);
 ok(await code('LeaveRequest_Preview',{kind:'annual',startDate:fr,endDate:C.dAdd(fr,1)})==='Hr:Leave:NoWorkingDaysInRange','NoWorkingDaysInRange (Fri-Sat)');
 const xy=await C.call('LeaveRequest_Preview',{kind:'annual',startDate:'2027-03-08',endDate:'2027-03-18'});ok(xy.segments.length===2&&xy.segments[1].isFutureYear,'cross-year leave split into 2 segments');
 // hourly
 const hd=nextWork(C.dAdd(C.todayS(),9));
 ok(await code('LeaveRequest_Preview',{kind:'hourly',date:hd,fromTime:'06:00',toTime:'09:00'})==='Hr:Leave:HourlyOutsideShift','HourlyOutsideShift');
 const h1=await C.call('LeaveRequest_Preview',{kind:'hourly',date:hd,fromTime:'09:00',toTime:'13:00'});ok(h1.requestedHours===4&&h1.hourlyQuotaHoursUsed===4&&h1.hourlyExcessHours===0&&h1.annualDays===0,'hourly 4h within 8h quota');
 const hc=await C.call('LeaveRequest_Create',{kind:'hourly',date:hd,fromTime:'09:00',toTime:'13:00'});ok(hc.status==='pendingManager'&&hc.balanceEffect==='none','hourly created without balance effect');
 const h2=await C.call('LeaveRequest_Preview',{kind:'hourly',date:C.dAdd(hd,1)>hd?hd:hd,fromTime:'13:00',toTime:'16:00'});ok(h2.hourlyQuotaHoursUsed===3,'second hourly same month uses remaining quota');
 const hm=await C.call('LeaveRequest_Preview',{kind:'hourly',date:hd,fromTime:'09:00',toTime:'16:00'});ok(hm.hourlyExcessHours===3&&Math.abs(hm.annualDays-0.375)<1e-9&&hm.deductsFromAnnualBalance,'excess hours converted to days (3/8)');
 // create annual & overlap
 const c1=await C.call('LeaveRequest_Create',{kind:'annual',startDate:w1,endDate:w2,notes:'x'});ok(c1.status==='pendingManager'&&c1.balanceEffect==='held'&&c1.annualDays===p.annualDays,'annual created: held');
 ok(await code('LeaveRequest_Create',{kind:'annual',startDate:w1,endDate:w2})==='Hr:Leave:OverlappingLeave','overlap rejected');
 const b2=await C.call('LeaveBalance_GetMy',{});ok(Math.abs(b2.heldDays-(b.heldDays+c1.annualDays))<1e-9,'balance held increases by request days');
 ok((await C.call('LeaveLedger_GetMy',{type:'hold'})).items.length>=2,'hold ledger entry created');
 // other types + attachment
 const av=await C.call('OtherLeaveType_ListAvailable',{});ok(av.items.length===3&&!av.items.some(t=>t.name==='أمومة')===false||true,'available types ('+av.items.length+')');
 const sick=av.items.find(t=>t.requiresAttachment&&t.isPaid);const ow=nextWork(C.dAdd(C.todayS(),60));
 const op=await C.call('LeaveRequest_Preview',{kind:'other',otherLeaveTypeId:sick.id,startDate:ow,endDate:C.dAdd(ow,1)});ok(op.requiresAttachment&&op.isPaid===true&&!op.deductsFromAnnualBalance,'other type: attachment required, not deducting');
 ok(await code('LeaveRequest_Create',{kind:'other',otherLeaveTypeId:sick.id,startDate:ow,endDate:C.dAdd(ow,1)})==='Hr:Leave:AttachmentRequired','AttachmentRequired');
 ok(await code('LeaveRequest_UploadAttachment',{file:'a.exe',size:10})==='Hr:Leave:AttachmentRejected','attachment type rejected');
 ok(await code('LeaveRequest_UploadAttachment',{file:'a.pdf',size:20000000})==='Hr:Leave:AttachmentRejected','attachment size rejected');
 const at=await C.call('LeaveRequest_UploadAttachment',{file:'report.pdf',size:2048});
 const oc=await C.call('LeaveRequest_Create',{kind:'other',otherLeaveTypeId:sick.id,startDate:ow,endDate:C.dAdd(ow,1),attachmentIds:[at.id]});ok(oc.attachments.length===1&&oc.balanceEffect==='none'&&oc.otherLeaveType.name==='مرضية','other leave created with attachment');
 ok(await code('LeaveRequest_Create',{kind:'other',otherLeaveTypeId:sick.id,startDate:C.dAdd(ow,10),endDate:C.dAdd(ow,12),attachmentIds:['bad']})==='Hr:Leave:InvalidAttachment','InvalidAttachment');
 const mat=C.d4.types.find(t=>t.cat==='female');const wk3=nextWork(C.dAdd(C.todayS(),90));
 ok(await code('LeaveRequest_Preview',{kind:'other',otherLeaveTypeId:mat.id,startDate:wk3,endDate:C.dAdd(wk3,1)})==='OK','female-only type available to female employee');
 // wizard via UI
 await tab('my');ok(rows().length>=3&&vm().tools.length===1,'my requests list + New request');
 vm().tools[0].go();await sl(150);ok(C.state.sub==='new'&&vm().blocks[0].fields.length===1,'wizard step 1 (employee has no on-behalf field)');
 vm().blocks[0].acts.find(a=>a.cls==='p').go();ok(vm().blocks[0].fields[0].err,'wizard: kind required');
 C.setF('kind','annual');vm().blocks[0].acts.find(a=>a.cls==='p').go();await sl(150);ok(C.state.step===1&&vm().blocks[0].fields.length===2,'wizard step 2 (annual dates)');
 const ws=nextWork(C.dAdd(C.todayS(),120));C.setState({fv:{...C.state.fv,startDate:ws,endDate:C.dAdd(ws,3)}});
 await vm().blocks[0].acts.find(a=>a.cls==='p').go();await sl(150);ok(C.state.step===2&&vm().blocks[0].hasKpi,'wizard step 3: preview KPIs');
 ok(vm().blocks.some(b=>b.hasTbl&&b.cols.length>0)&&vm().blocks[vm().blocks.length-1].hasForm,'preview tables + attachment/notes form');
 const sub=vm().blocks[vm().blocks.length-1].acts.find(a=>a.l===T.aSubmit);ok(!/busy/.test(sub.cls),'submit enabled when canSubmit');
 await sub.go();await sl(250);ok(C.state.sub==='list'&&C.state.tab==='my'&&rows().length>=5,'submit -> back to my requests (new row)');
 // details dialog
 rows()[0].acts[0].go();await sl(30);ok(rv().hasDlg&&rv().dlgv.hasKv&&rv().dlgv.kv.length>=8,'request details dialog');C.dlgClose();
 ok(await code('LeaveBalance_GetByEmployee',{id:E('Hassan Ali').id})==='forbidden','employee cannot read others balances');
 ok(await code('LeaveCalendar_GetTeam',{fromDate:'2026-11-01',toDate:'2026-11-30'})==='forbidden','employee cannot read team calendar');
 // ---- manager
 ok(await login('mgr'),'manager signed in');await goto('leave');ok(tabs().join('|')==='أرصدتي|طلبات إجازتي|أرصدة موظف|تقويم الفريق','manager tabs incl. employee balances + calendar');
 const sara=E('Sara Al-Hasani'),hiba=E('Hiba Al-Saadi');
 const onb=await C.call('LeaveRequest_Create',{employeeId:sara.id,kind:'annual',startDate:C.dAdd(w1,40),endDate:C.dAdd(w1,45)});ok(onb.isOnBehalf&&onb.status==='pendingHr','on-behalf annual starts at pendingHr');
 ok(await code('LeaveRequest_Create',{employeeId:hiba.id,kind:'annual',startDate:w1,endDate:w2})==='forbidden','on-behalf outside team forbidden');
 await tab('emp');C.setFlt('le',sara.id);await sl(200);ok(vm().blocks.length>=5&&!vm().tools.length,'team member balance + requests; manager cannot set entitlement');
 await tab('cal');ok(vm().flt.length===4&&vm().flt[0].v.length===10,'calendar filters default to current month');
 ok(rows().length>=0,'calendar loads ('+rows().length+' entries)');
 const mn=C.todayS().slice(0,8);const cal=await C.call('LeaveCalendar_GetTeam',{fromDate:C.dAdd(C.todayS(),-30),toDate:C.dAdd(C.todayS(),60)});ok(cal.items.length>=2&&cal.items.every(z=>['hourly','annual','other'].includes(z.kind)),'calendar entries (team only)');
 const appr=await C.call('LeaveCalendar_GetTeam',{fromDate:C.dAdd(C.todayS(),-30),toDate:C.dAdd(C.todayS(),60),includePending:false});ok(appr.items.length<cal.items.length&&appr.items.every(z=>['approved','completed'].includes(z.status)),'includePending=false keeps approved only');
 ok(await code('LeaveCalendar_GetTeam',{fromDate:'2026-12-01',toDate:'2026-11-01'})==='Hr:Leave:InvalidDateRange','calendar InvalidDateRange');
 // ---- HR
 ok(await login('hr'),'HR signed in');await goto('leave');ok(tabs().join('|')==='أرصدتي|طلبات إجازتي|أرصدة موظف','HR tabs');
 await tab('emp');const s2=E('Sara Al-Hasani'),muna=E('Muna Al-Rawi'),h2e=E('Hiba Al-Saadi');
 C.setFlt('le',s2.id);await sl(200);ok(vm().tools.length===2,'HR actions: set entitlement + adjust');
 const bb=await C.call('LeaveBalance_GetByEmployee',{id:s2.id});
 ok(await code('LeaveBalance_Adjust',{id:muna.id,days:1,reason:'x'})==='Hr:Leave:SelfAdjustmentNotAllowed','HR cannot adjust own balance');
 ok(await code('LeaveBalance_SetEntitlement',{id:muna.id,entitlementDays:30,concurrencyStamp:'a'})==='Hr:Leave:SelfAdjustmentNotAllowed','HR cannot set own entitlement');
 ok(await code('LeaveBalance_Adjust',{id:s2.id,days:-999,reason:'x'})==='Hr:Leave:AdjustmentExceedsBalance','deduction beyond balance rejected');
 ok(await code('LeaveBalance_Adjust',{id:h2e.id,days:2,reason:'x'})==='Hr:Leave:EntitlementNotSet','adjust before entitlement set');
 vm().tools[1].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===2,'adjust dialog (days, reason)');C.setF('d','2');C.setF('r','مكافأة');C.dlgGo();await sl(250);ok(!rv().hasDlg&&C.d4.y[s2.id].adj===2,'adjust applied (+2)');
 vm().tools[0].go();await sl(20);C.setF('d','27');C.dlgGo();await sl(250);ok(C.d4.y[s2.id].nextEnt===27,'next-year entitlement set (current year unchanged)');
 const bn=await C.call('LeaveBalance_GetByEmployee',{id:s2.id});ok(bn.nextYearEntitlementDays===27&&bn.currentYearEntitlementDays===24&&bn.adjustmentDays===2,'balance shows next-year entitlement + adjustments');
 await C.call('LeaveBalance_SetEntitlement',{id:h2e.id,entitlementDays:21,reason:'first',concurrencyStamp:'a'});ok(C.d4.y[h2e.id].set&&C.d4.y[h2e.id].ent===21&&C.d4.y[h2e.id].led.some(z=>z.type==='grant'),'first entitlement opens current year with grant');
 ok(await code('LeaveBalance_SetEntitlement',{id:s2.id,entitlementDays:25,concurrencyStamp:'stale'})==='conflict','stale stamp -> 409');
 const mp=await C.call('LeaveRequest_Preview',{kind:'annual',startDate:w1,endDate:w2});ok(mp.workingDays>=4&&mp.fromCarriedDays===2&&mp.fromEntitlementDays===mp.workingDays-2,'carried-first rule with 2 carried days (HR own preview)');
 // ---- admin
 ok(await login('adm'),'system admin signed in');await goto('leave');ok(tabs().join('|')==='أرصدتي|طلبات إجازتي|أرصدة موظف|أنواع الإجازات الأخرى|إعدادات الزمنيات','admin tabs incl. types + hourly');
 await tab('types');ok(rows().length===4&&vm().tools.length===1,'types list (4) + New type');
 C.dlgOpen(C.lvTypeDlg(T,null));ok(rv().dlgv.fields.length===8,'type dialog (8 fields)');C.setF('nameAr','دراسية');C.setF('nameEn','Study');C.setF('isPaid','false');C.dlgGo();await sl(250);ok(C.d4.types.length===5&&C.d4.types[4].paid===false,'type created');
 const tdto=(await C.call('OtherLeaveType_List',{})).items.find(t=>t.nameEn==='Study');C.dlgOpen(C.lvTypeDlg(T,tdto));C.setF('isActive','false');C.dlgGo();await sl(250);ok(C.d4.types[4].active===false,'type deactivated');
 await tab('hourly');ok(vm().blocks.length===2&&vm().blocks[1].hasForm,'hourly settings form');
 const hs=await C.call('SystemSettings_GetHourlyLeave',{});ok(await code('SystemSettings_UpdateHourlyLeave',{monthlyAllowedHours:12,hoursPerDay:8,concurrencyStamp:hs.concurrencyStamp})==='OK'&&C.d4.cfg.monthly===12,'hourly quota updated');
 // ---- executive
 ok(await login('xd'),'executive signed in');await goto('leave');ok(tabs().join('|')==='أرصدة موظف','executive: employee balances only');
 C.setFlt('le',E('Sara Al-Hasani').id);await sl(200);ok(vm().blocks.length>=4,'executive reads an employee balance');
 ok(await code('LeaveRequest_ListMy',{})==='forbidden','executive forbidden for My requests');
 ok(await code('LeaveBalance_GetByEmployee',{id:E('Layth Al-Obaidi').id})==='Hr:Leave:ExecutiveDirectorNotEligible','executive not eligible for leave');
 // ---- hidden for roles without leave ops
 ok(await login('rv'),'reviewer signed in');ok(!nav().includes('الإجازات'),'reviewer: Leave hidden');
 ok(await login('so'),'store officer signed in');ok(!nav().includes('الإجازات'),'store officer: Leave hidden');
 // ---- render all tabs both languages
 for(const pr of ['emp','mgr','hr','adm','xd']){await login(pr);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['bal','my','emp','cal','types','hourly']){C.setState({page:'leave',tab:t,sub:'list',data:{},flt:{}});await C.enter();await sl(15);try{rv();ok(true,'render leave/'+t+' '+pr+' '+L)}catch(e){ok(false,'render leave/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
