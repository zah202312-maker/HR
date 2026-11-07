process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u,cb){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n};if(cb)cb()}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},tabs=()=>vm().tabs.map(t=>t.l),nav=()=>rv().nav.map(n=>n.label);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);if(C.state.scr==='mfa'&&k==='acc'){C.srv.mfaE=true;await C.boot();await sl(200)}return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const tab=async(t)=>{C.setState({tab:t,skip:{},data:{},flt:{},sub:'list'});await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const U=(n)=>C.UID(n);const E=(n)=>C.db.emps.find(e=>e.en===n);
(async()=>{
 const td=()=>C.todayS(),cur=()=>td().slice(0,7),p1=()=>C.fnPrev(cur(),-1);
 const f0={name:'doc.pdf',size:50000};
 ok(await login('acc'),'accountant signed in');await goto('fin');
 ok(tabs().join('|')==='الرواتب|الإضافات والاستقطاعات|اقتراحات الحضور|المصروفات|طلبات المحاسب|دورات الرواتب|تقارير مالية','accountant: 7 finance tabs (payroll runs + reports added)');
 const me=E('Karim Saleh').id,hr=E('Muna Al-Rawi').id,ex=E('Layth Al-Obaidi').id;
 // ---------- list
 const rl=await C.call('PayrollRun_List',{});ok(rl.items.length===2&&rl.items[0].period>rl.items[1].period&&rl.items.every(r=>r.totals.length===1&&r.totals[0].currency==='IQD'&&r.employeeCount>=8&&r.concurrencyStamp),'seeded runs, newest first, with totals per currency');
 ok(rl.items.map(r=>r.status).join()==='approved,closed','one approved and one closed run');
 ok((await C.call('PayrollRun_List',{status:'closed'})).items.length===1&&(await C.call('PayrollRun_List',{periodFrom:rl.items[0].period})).items.length===1&&(await C.call('PayrollRun_List',{periodTo:rl.items[1].period})).items.length===1,'filters (status, period range)');
 const closed=rl.items[1],approved=rl.items[0];
 const cg=await C.call('PayrollRun_Get',{id:closed.id});ok(cg.items.every(i=>Math.abs(i.net-(i.gross+i.additions-i.deductions))<0.01&&i.lines[0].kind==='baseSalary'),'every item: net = gross + additions - deductions; the first line is the base salary');
 ok(Math.abs(cg.totals[0].net-cg.items.reduce((n,i)=>n+i.net,0))<0.01,'totals equal the sum of the items');
 // ---------- create
 ok(await code('PayrollRun_Create',{period:'2026-13'})==='validation','period validated');
 const nr=await C.call('PayrollRun_Create',{period:p1()});
 ok(nr.status==='draft'&&nr.preparedBy.displayName&&nr.isStale===false&&nr.approvalRequestedAt===null&&nr.items.length>=8,'a new draft run for the previous month ('+nr.items.length+' employees)');
 ok(await code('PayrollRun_Create',{period:p1()})==='conflict','one run per period (409)');
 ok(!nr.items.some(i=>i.employee.id===ex),'the executive director is not in the run');
 ok(nr.employeesWithoutProfile.some(z=>z.id===U('24')),'employees without a financial profile are listed, not calculated');
 const sara=nr.items.find(i=>i.employee.id===U('11'));ok(sara.lines[0].amount===1550000,'Sara’s base comes from the version in effect in that period (the raise)');
 ok(sara.lines.some(l=>l.kind==='addition'&&l.adjustmentId&&l.amount===100000),'an open addition enters the run with its adjustment id');
 const adj18=C.d15.adjs.filter(a=>a.emp===U('18')&&a.per===p1());ok(adj18.some(a=>a.st==='reversed')&&nr.items.find(i=>i.employee.id===U('18')).lines.filter(l=>l.adjustmentId).length===adj18.filter(a=>a.st==='open').length,'a reversed adjustment never enters the run');
 ok(nr.pendingSuggestionCount>=1,'unreviewed suggestions are counted but not included ('+nr.pendingSuggestionCount+')');
 ok(nr.items.every(i=>Math.abs(i.net-(i.gross+i.additions-i.deductions))<0.01),'draft arithmetic is consistent');
 // ---------- stale / recalc / request
 await C.call('Adjustment_Create',{employeeId:U('20'),period:p1(),type:'addition',amount:10000,reason:'مكافأة',attachment:f0});
 const g1=await C.call('PayrollRun_Get',{id:nr.id});ok(g1.isStale===true,'a new adjustment makes the draft stale');
 ok(await code('PayrollRun_RequestApproval',{id:nr.id,concurrencyStamp:g1.concurrencyStamp})==='Hr:Finance:PayrollRunStale','a stale run cannot be sent for approval');
 ok(await code('PayrollRun_Recalculate',{id:nr.id,concurrencyStamp:'stale'})==='conflict','recalculate: stale stamp -> 409');
 const rc=await C.call('PayrollRun_Recalculate',{id:nr.id,concurrencyStamp:g1.concurrencyStamp,note:'بعد المكافأة'});ok(rc.isStale===false&&rc.approvalRequestedAt===null&&rc.concurrencyStamp!==g1.concurrencyStamp&&rc.items.find(i=>i.employee.id===U('20')).lines.some(l=>l.amount===10000),'recalculate: fresh data, not stale, new stamp');
 ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:rc.concurrencyStamp})==='forbidden','the accountant cannot approve');
 await login('hr',true);
 ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:rc.concurrencyStamp})==='Hr:Finance:ApprovalNotRequested','ApprovalNotRequested');
 ok(await code('PayrollRun_ReturnForChanges',{id:nr.id,reason:'x'})==='Hr:Finance:ApprovalNotRequested','cannot return a run that was not sent for approval');
 await login('acc',true);
 const rq=await C.call('PayrollRun_RequestApproval',{id:nr.id,concurrencyStamp:rc.concurrencyStamp,note:'جاهز'});ok(rq.status==='draft'&&rq.approvalRequestedAt,'approval requested; the status stays draft');
 ok(await code('PayrollRun_RequestApproval',{id:nr.id,concurrencyStamp:rc.concurrencyStamp})==='conflict','the stamp changed after the request');
 ok(C.d8.notes.some(n=>n.user===hr&&n.code==='payrollRunPendingApproval'&&n.rid===nr.id&&n.rt==='payrollRun')&&!C.d8.notes.some(n=>n.user===ex&&n.code==='payrollRunPendingApproval'),'payrollRunPendingApproval goes to the approver role set in the settings (HR manager)');
 ok(await code('PayrollRun_RequestApproval',{id:approved.id,concurrencyStamp:approved.concurrencyStamp})==='Hr:Finance:InvalidPayrollRunStatus','an approved run cannot be sent again (InvalidPayrollRunStatus)');
 ok(await code('PayrollRun_Recalculate',{id:approved.id,concurrencyStamp:approved.concurrencyStamp})==='Hr:Finance:InvalidPayrollRunStatus'&&await code('PayrollRun_Close',{id:nr.id,concurrencyStamp:rq.concurrencyStamp})==='Hr:Finance:InvalidPayrollRunStatus','recalculate only a draft; close only an approved run');
 // ---------- approver
 await login('hr',true);
 ok((await C.call('PayrollRun_List',{})).items.length===3&&(await C.call('PayrollRun_Get',{id:nr.id})).items.length>=8,'HR reads runs while HR is the configured approver');
 ok(await code('PayrollRun_Create',{period:C.fnPrev(cur(),-5)})==='forbidden'&&await code('PayrollRun_Recalculate',{id:nr.id,concurrencyStamp:rq.concurrencyStamp})==='forbidden'&&await code('PayrollRun_Close',{id:approved.id,concurrencyStamp:approved.concurrencyStamp})==='forbidden','HR cannot create, recalculate or close');
 const own=C.d16.runs.find(r=>r.id===nr.id);own.prep=hr;ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:rq.concurrencyStamp})==='Hr:Finance:ApproverIsPreparer','ApproverIsPreparer: nobody approves a run they prepared');own.prep=me;
 ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:'old'})==='conflict','approve: stale stamp -> 409');
 // return for changes
 ok(await code('PayrollRun_ReturnForChanges',{id:nr.id,reason:''})==='validation','a return needs a reason');
 const rt=await C.call('PayrollRun_ReturnForChanges',{id:nr.id,reason:'يرجى مراجعة الاستقطاعات'});ok(rt.status==='draft'&&rt.approvalRequestedAt===null,'returned: stays draft, the request is cancelled');
 await login('acc',true);const g2=await C.call('PayrollRun_Get',{id:nr.id});const rq2=await C.call('PayrollRun_RequestApproval',{id:nr.id,concurrencyStamp:g2.concurrencyStamp});
 // settings: the approver role switches to the executive director
 await login('adm',true);const fs0=await C.call('SystemSettings_GetFinance',{});await C.call('SystemSettings_UpdateFinance',{payrollApproverRole:'executiveDirector',concurrencyStamp:fs0.concurrencyStamp});
 await login('hr',true);ok(await code('PayrollRun_List',{})==='forbidden'&&await code('PayrollRun_Get',{id:nr.id})==='forbidden'&&await code('PayrollRun_Export',{id:nr.id,format:'pdf'})==='forbidden','when the approver is the executive director, HR gets 403 on runs');
 ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:rq2.concurrencyStamp})==='Hr:Finance:NotConfiguredPayrollApprover','NotConfiguredPayrollApprover: HR is not the configured approver');
 await goto('fin');ok(tabs().join('|')==='دورات الرواتب','HR sees only the payroll-run tab');await tab('run');ok(vm().blocks.length===1&&vm().blocks[0].t===T.nPyNot,'…with a note that the run does not concern HR');
 await login('xd',true);ok((await C.call('PayrollRun_List',{})).items.length===3,'the executive director reads runs when configured as the approver');
 await goto('fin');ok(tabs().join('|')==='دورات الرواتب'&&nav().includes('المالية'),'executive: payroll-run tab');await tab('run');ok(rows().length===3&&vm().tools.length===0,'run list for the approver (no New run)');
 C.toSub('rn',nr.id);await sl(250);ok(C.state.sub==='rn'&&vm().tools.some(t=>t.l===T.aPyApp)&&vm().tools.some(t=>t.l===T.aPyRet)&&vm().blocks.length===3,'run details: Approve / Return for the approver');
 const ap=await C.call('PayrollRun_Approve',{id:nr.id,concurrencyStamp:rq2.concurrencyStamp});ok(ap.status==='approved'&&ap.approvedBy.displayName&&ap.approvedAt,'approved by the configured approver');
 ok(C.d15.adjs.filter(a=>a.per===p1()&&a.st==='open').length===0&&C.d15.adjs.some(a=>a.per===p1()&&a.st==='includedInRun'),'the period’s open adjustments are now includedInRun');
 ok(C.d15.locked.includes(p1()),'the period is locked');
 ok(C.d8.notes.some(n=>n.user===me&&n.code==='payrollRunApproved'&&n.rid===nr.id),'payrollRunApproved reaches the accountant');
 ok(await code('PayrollRun_Approve',{id:nr.id,concurrencyStamp:ap.concurrencyStamp})==='Hr:Finance:InvalidPayrollRunStatus','cannot approve twice');
 await login('adm',true);const fs1=await C.call('SystemSettings_GetFinance',{});await C.call('SystemSettings_UpdateFinance',{payrollApproverRole:'hrManager',concurrencyStamp:fs1.concurrencyStamp});
 await login('acc',true);
 ok(await code('Adjustment_Create',{employeeId:U('11'),period:p1(),type:'addition',amount:5,reason:'x',attachment:f0})==='Hr:Finance:PeriodLocked','an approved period accepts no new adjustments');
 ok(await code('SalaryProfile_Update',{employeeId:U('18'),baseSalary:1200000,currency:'IQD',effectiveFrom:C.dAdd(td(),-5),components:[],concurrencyStamp:(await C.call('SalaryProfile_GetHistory',{employeeId:U('18')})).items[0].concurrencyStamp})!=='Hr:Finance:EffectiveDateInLockedPeriod','a profile effective in an open period is fine');
 // ---------- close and payslips
 const before=C.d8.notes.filter(n=>n.code==='payslipAvailable').length;
 const cl=await C.call('PayrollRun_Close',{id:nr.id,concurrencyStamp:ap.concurrencyStamp});ok(cl.status==='closed'&&cl.closedAt,'closed after approval by the accountant');
 const pn=C.d8.notes.filter(n=>n.code==='payslipAvailable'&&n.rt==='payslip');ok(pn.length-before===cl.items.length&&pn.every(n=>!/\d{4,}/.test(JSON.stringify(C.NT.payslipAvailable))),'payslipAvailable for every employee in the run, without amounts');
 ok(await code('PayrollRun_Close',{id:nr.id,concurrencyStamp:cl.concurrencyStamp})==='Hr:Finance:InvalidPayrollRunStatus','cannot close twice');
 const cl2=await C.call('PayrollRun_Close',{id:approved.id,concurrencyStamp:approved.concurrencyStamp});ok(cl2.status==='closed','the seeded approved run can also be closed');
 // exports + audit
 const xp=await C.call('PayrollRun_Export',{id:nr.id,format:'pdf'}),xe=await C.call('PayrollRun_Export',{id:nr.id,format:'excel'});ok(xp.fileName.endsWith('.pdf')&&xe.fileName.endsWith('.xlsx')&&await code('PayrollRun_Export',{id:nr.id,format:'csv'})==='validation','export PDF / Excel');
 ok(C.d8.aud.some(e=>e.action==='export'&&e.et==='PayrollRun'&&e.user===me),'the export is recorded in the audit log');
 ok(C.d8.aud.some(e=>e.action==='read'&&e.et==='PayrollRun')&&JSON.stringify(C.d8.aud).indexOf('1550000')===-1,'run reads are audited without values');
 // ---------- payslips
 await login('emp',true);const ps=await C.call('Payslip_GetMyList',{});ok(ps.items.length===3&&ps.items.every(z=>z.closedAt&&z.net>0)&&ps.items[0].period>ps.items[1].period,'my payslips: only closed runs, newest first ('+ps.items.length+')');
 const one=await C.call('Payslip_GetMy',{id:ps.items[0].id});ok(one.lines.length>=2&&one.employee.id===U('11')&&Math.abs(one.net-(one.gross+one.additions-one.deductions))<0.01&&one.closedAt,'payslip details with the same lines as the run');
 ok(await code('Payslip_GetMy',{id:cl.items.find(i=>i.employee.id===U('18')).id})==='Hr:Finance:NotFound','someone else’s payslip -> 404');
 ok((await C.call('Payslip_DownloadMyPdf',{id:ps.items[0].id})).fileName.endsWith('.pdf')&&await code('Payslip_DownloadMyPdf',{id:U('97')})==='Hr:Finance:NotFound','PDF download with the same restriction');
 ok(C.d8.aud.some(e=>e.action==='read'&&e.et==='Payslip'&&e.user===U('11')),'payslip reads are audited');
 ok(C.d8.notes.some(n=>n.user===U('11')&&n.code==='payslipAvailable'),'the employee got payslipAvailable');
 await goto('pay');ok(nav().includes('قسيمة الراتب')&&rows().length===3,'payslip page: list');rows()[0].acts[0].go();await sl(200);ok(C.state.sub==='one'&&vm().blocks.length===2&&vm().tools.length===2,'payslip details with Download PDF');
 await C.ntOpen({id:C.d8.notes.find(n=>n.user===U('11')&&n.code==='payslipAvailable').id,templateCode:'payslipAvailable',resourceId:ps.items[0].id,isRead:true});await sl(400);ok(C.state.page==='pay'&&C.state.sub==='one','the notification opens the payslip');
 for(const p of ['mgr','hr','so','rv']){await login(p,true);const l=await C.call('Payslip_GetMyList',{});ok(Array.isArray(l.items),p+': own payslips available (everyone is an employee)')}
 await login('xd',true);ok((await C.call('Payslip_GetMyList',{})).items.length===0,'the executive director has no payroll profile, so no payslips');
 // ---------- reports
 await login('acc',true);
 const ms=await C.call('FinanceReport_GetMonthlyStatement',{period:closed.period});ok(ms.runId===closed.id&&ms.runStatus==='closed'&&ms.items.length===cg.items.length&&ms.totals.length===1,'monthly statement for a period');
 const dp=C.db.D[0].id;const ms2=await C.call('FinanceReport_GetMonthlyStatement',{period:closed.period,departmentId:dp});ok(ms2.items.length<ms.items.length&&ms2.items.every(i=>(C.db.emps.find(e=>e.id===i.employee.id)||{}).dept===dp)&&Math.abs(ms2.totals[0].net-ms2.items.reduce((n,i)=>n+i.net,0))<0.01,'department filter recomputes the totals');
 ok(await code('FinanceReport_GetMonthlyStatement',{period:C.fnPrev(cur(),-9)})==='Hr:Finance:PayrollRunNotFoundForPeriod'&&await code('FinanceReport_GetMonthlyStatement',{})==='validation','PayrollRunNotFoundForPeriod; period required');
 const ar=await C.call('FinanceReport_GetAdjustments',{periodFrom:C.fnPrev(cur(),-3),periodTo:cur(),maxResultCount:1});const full=await C.call('FinanceReport_GetAdjustments',{periodFrom:C.fnPrev(cur(),-3),periodTo:cur(),maxResultCount:100});
 ok(ar.items.length===1&&ar.totalCount>1&&JSON.stringify(ar.totals)===JSON.stringify(full.totals)&&ar.totals[0].currency==='IQD','adjustment report: the totals cover all results, not the current page');
 ok(Math.abs(full.totals[0].additions-full.items.filter(a=>a.type==='addition').reduce((n,a)=>n+a.amount,0))<0.01&&full.items.every(a=>a.status!=='reversed'),'additions total matches the listed rows; reversed originals are excluded');
 ok((await C.call('FinanceReport_GetAdjustments',{periodFrom:C.fnPrev(cur(),-3),periodTo:cur(),type:'deduction'})).items.every(a=>a.type==='deduction')&&await code('FinanceReport_GetAdjustments',{periodFrom:cur()})==='validation','type filter; both periods required');
 await login('so',true);await C.call('Stock_Receive',{itemId:C.d11.items.find(i=>i.en==='Pens').id,quantity:5,totalCost:40000});await login('acc',true);
 const se=await C.call('FinanceReport_GetStoreExpenses',{dateFrom:C.dAdd(td(),-5),dateTo:td()});ok(se.items.length>=1&&se.items.every(e=>e.source==='storePurchase')&&se.totals[0].amount>=40000&&await code('FinanceReport_GetStoreExpenses',{dateFrom:td()})==='validation','store purchases report with totals per currency');
 ok((await C.call('FinanceReport_Export',{code:'monthlyStatement',format:'pdf',period:closed.period})).fileName.startsWith('monthlyStatement_')&&(await C.call('FinanceReport_Export',{code:'adjustments',format:'excel',periodFrom:cur(),periodTo:cur()})).fileName.endsWith('.xlsx')&&(await C.call('FinanceReport_Export',{code:'storeExpenses',format:'pdf',dateFrom:td(),dateTo:td()})).fileName.startsWith('storeExpenses_'),'finance report export for each report code');
 ok(await code('FinanceReport_Export',{code:'monthlyStatement',format:'pdf'})==='Hr:Finance:ReportFilterRequired'&&await code('FinanceReport_Export',{code:'adjustments',format:'pdf',periodFrom:cur()})==='Hr:Finance:ReportFilterRequired'&&await code('FinanceReport_Export',{code:'monthlyStatement',format:'pdf',period:C.fnPrev(cur(),-9)})==='Hr:Finance:PayrollRunNotFoundForPeriod'&&await code('FinanceReport_Export',{code:'x',format:'pdf'})==='validation','ReportFilterRequired; PayrollRunNotFoundForPeriod; unknown code');
 ok(C.d8.aud.filter(e=>e.action==='export'&&e.et==='FinanceReport').length>=3,'each finance report export is audited');
 // ---------- access
 for(const p of ['mgr','so','emp','rv','adm']){await login(p,true);ok(await code('PayrollRun_List',{})==='forbidden'&&await code('PayrollRun_Get',{id:nr.id})==='forbidden'&&await code('FinanceReport_GetMonthlyStatement',{period:closed.period})==='forbidden'&&await code('FinanceReport_Export',{code:'adjustments',format:'pdf',periodFrom:cur(),periodTo:cur()})==='forbidden',p+': no payroll or finance-report access')}
 // ---------- UI (accountant)
 await login('acc',true);await goto('fin');await tab('run');ok(rows().length===3&&vm().tools.length===1&&vm().flt.length===1,'runs tab: table, status filter, New run');
 vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===1,'new-run dialog');C.dlgClose();
 const dr=await C.call('PayrollRun_Create',{period:C.fnPrev(cur(),-6)});C.toSub('rn',dr.id);await sl(250);ok(vm().tools.some(t=>t.l===T.aPyRec)&&vm().tools.some(t=>t.l===T.aPyReq)&&vm().tools.some(t=>t.l===T.aPyPdf)&&vm().blocks.length===3,'draft run: Recalculate, Request approval, Export + summary, totals, employees');
 vm().blocks[2].rows[0].acts[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.hasKv,'employee lines dialog');C.dlgClose();
 C.toSub('rn',nr.id);await sl(250);ok(!vm().tools.some(t=>t.l===T.aPyReq)&&!vm().tools.some(t=>t.l===T.aPyCl),'a closed run offers only export');
 await tab('rep');ok(vm().flt.length===2&&vm().blocks.length===1,'reports tab asks for the filters first');C.setFlt('fq',closed.period);await sl(300);ok(vm().blocks.length===3&&vm().tools.length===2,'monthly statement report with totals and Export');
 C.setFlt('fr','adjustments');await sl(100);C.setFlt('fa',C.fnPrev(cur(),-3));C.setFlt('fb',cur());await sl(300);ok(vm().blocks.length===3&&vm().flt.length===5,'adjustment report: totals, rows, note');
 C.setFlt('fr','storeExpenses');await sl(100);C.setFlt('fc',C.dAdd(td(),-5));C.setFlt('fd',td());await sl(300);ok(vm().blocks.length===3,'store expenses report');
 for(const pr of ['acc','hr','xd','emp','mgr']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const [pg,tb] of [['fin','run'],['fin','rep'],['pay','list']]){if(pg==='fin'&&!['acc','hr','xd'].includes(pr))continue;if(pg==='fin'&&tb==='rep'&&pr!=='acc')continue;C.setState({page:pg,tab:tb,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render '+pg+'/'+tb+' '+pr+' '+L)}catch(e){ok(false,'render '+pg+'/'+tb+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
