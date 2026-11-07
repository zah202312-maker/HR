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
const U=(n)=>C.UID(n);
(async()=>{
 ok(await login('hr'),'HR signed in');await goto('con');
 ok(nav().includes('العقود')&&tabs().join('|')==='تنتهي قريباً|كل العقود','HR tabs: expiring + all contracts');
 const al=await C.call('Contract_List',{maxResultCount:100});ok(al.items.length>=10&&al.items.every(c=>c.employee.fullName&&c.contractType&&typeof c.daysRemaining==='number'&&typeof c.isExpired==='boolean'),'all contracts with days remaining and expiry flag ('+al.totalCount+')');
 ok(al.items.every((c,i,a)=>i===0||a[i-1].endDate<=c.endDate),'sorted by end date');
 ok((await C.call('Contract_List',{status:'renewed'})).items.every(c=>c.status==='renewed')&&(await C.call('Contract_List',{employeeId:U('14')})).items.length===2,'filters (status, employee)');
 // expiring
 const ex=await C.call('Contract_GetExpiring',{});ok(ex.alertDays===30&&ex.items.length>=5&&ex.items.every(c=>['active','pendingRecommendation','pendingDecision'].includes(c.status)&&c.daysRemaining<=30),'expiring list: undecided contracts ending within 30 days, including expired ones');
 ok(ex.items.some(c=>c.isExpired)&&ex.items.every((c,i,a)=>i===0||a[i-1].endDate<=c.endDate),'includes expired-without-decision; ordered by end date');
 ok((await C.call('Contract_GetExpiring',{withinDays:5})).items.length<ex.items.length,'withinDays narrows the list');
 ok(!ex.items.some(c=>['renewed','notRenewed'].includes(c.status)),'decided contracts never appear as expiring');
 const od=ex.items.find(c=>c.isDecisionOverdue);ok(!!od&&od.status==='pendingDecision','a late decision is flagged (isDecisionOverdue) but stays open');
 // recommend / decide flow
 const pr=ex.items.find(c=>c.status==='pendingRecommendation'&&c.employee.id!==U('13'));
 ok(await code('Contract_Decide',{id:pr.id,decision:'renew'})==='Hr:Contract:InvalidStatus','cannot decide before a recommendation (InvalidStatus)');
 ok(await code('Contract_Recommend',{id:pr.id,employeePosition:'',recommendation:'renew'})==='validation','position required');
 const rc=await C.call('Contract_Recommend',{id:pr.id,employeePosition:'أداء متميز',recommendation:'renew',note:'ملاحظة'});
 ok(rc.status==='pendingDecision'&&rc.recommendation.recommendedBy.displayName&&rc.decisionDueDate>=C.todayS(),'recommendation moves the contract to pendingDecision and sets a due date');
 const dueOk=(()=>{let n=0,d=C.todayS();while(d<rc.decisionDueDate){d=C.dAdd(d,1);if(C.isWorkDay(d))n++}return n})();ok(dueOk===5,'due date is 5 working days after submission (company calendar) ['+dueOk+']');
 ok(await code('Contract_Recommend',{id:pr.id,employeePosition:'x',recommendation:'renew'})==='Hr:Contract:InvalidStatus','recommendation only once (InvalidStatus)');
 const dc=await C.call('Contract_Decide',{id:pr.id,decision:'doNotRenew',note:'قرار الإدارة',employeeMessage:'نشكرك على جهودك'});
 ok(dc.status==='notRenewed'&&dc.decision.decision==='doNotRenew'&&dc.recommendation.recommendation==='renew'&&dc.employeeNotifiedAt,'the decision may differ from the recommendation; both are kept; the employee notification time is recorded');
 ok(await code('Contract_Decide',{id:pr.id,decision:'renew'})==='Hr:Contract:InvalidStatus','cannot decide twice');
 ok(C.d8.notes.some(n=>n.user===pr.employee.id&&n.code==='contractDecided'&&n.rid===pr.id&&n.rt==='contract'),'the employee is notified automatically (contractDecided)');
 ok(!C.d8.notes.some(n=>n.user===pr.employee.id&&/contract(Expiring|Recommend)/.test(n.code)),'nothing about the recommendation reaches the employee');
 // self action
 const mine=al.items.find(c=>c.employee.id===U('13'));ok(!!mine&&await code('Contract_Recommend',{id:mine.id,employeePosition:'x',recommendation:'renew'})==='Hr:Contract:SelfActionNotAllowed','HR cannot recommend on their own contract');
 const hrOwn=C.d12.cons.find(c=>c.emp===U('13'));hrOwn.status='pendingDecision';ok(await code('Contract_Decide',{id:hrOwn.id,decision:'renew'})==='Hr:Contract:SelfActionNotAllowed','…nor decide it');hrOwn.status='pendingRecommendation';
 // create
 ok(await code('Contract_Create',{employeeId:U('99'),contractType:'x',startDate:'2026-01-01',endDate:'2027-01-01'})==='Hr:Contract:EmployeeNotFound','EmployeeNotFound');
 ok(await code('Contract_Create',{employeeId:U('11'),contractType:'x',startDate:'2027-01-01',endDate:'2027-01-01'})==='Hr:Contract:InvalidDateRange','InvalidDateRange');
 const sara=al.items.find(c=>c.employee.id===U('11'));ok(await code('Contract_Create',{employeeId:U('11'),contractType:'x',startDate:sara.startDate,endDate:sara.endDate})==='Hr:Contract:OverlappingContract','OverlappingContract');
 const ren=al.items.find(c=>c.status==='renewed');ok(await code('Contract_Create',{employeeId:U('11'),contractType:'x',startDate:'2030-01-01',endDate:'2031-01-01',previousContractId:ren.id})==='Hr:Contract:InvalidPreviousContract','InvalidPreviousContract (another employee’s contract)');
 ok(await code('Contract_Create',{employeeId:ren.employee.id,contractType:'x',startDate:'2030-01-01',endDate:'2031-01-01',previousContractId:sara.id})==='Hr:Contract:InvalidPreviousContract','InvalidPreviousContract (not renewed)');
 const far=await C.call('Contract_Create',{employeeId:U('24'),contractType:'سنوي',startDate:C.todayS(),endDate:C.dAdd(C.todayS(),365)});ok(far.status==='active'&&far.employee.fullName,'a long contract starts active');
 const near=await C.call('Contract_Create',{employeeId:U('22'),contractType:'قصير',startDate:C.dAdd(C.todayS(),366),endDate:C.dAdd(C.todayS(),380)});ok(near.status==='active','a future contract outside the alert window stays active');
 const soon=await C.call('Contract_Create',{employeeId:U('16'),contractType:'قصير',startDate:C.dAdd(C.todayS(),-300),endDate:C.dAdd(C.todayS(),10)});ok(soon.status==='pendingRecommendation','a contract ending inside the alert window starts as pendingRecommendation');
 ok(C.d8.notes.filter(n=>n.code==='contractDecided'&&n.rid===far.id).length===0,'no notification to the employee on creation');
 // renewal contract linked to a renewed one
 const yo=(await C.call('Contract_List',{employeeId:U('20')})).items.find(c=>c.status==='pendingDecision');const yd=await C.call('Contract_Decide',{id:yo.id,decision:'renew',employeeMessage:'تم التجديد'});ok(yd.status==='renewed','a decision to renew marks the contract renewed');
 const rn=await C.call('Contract_Create',{employeeId:U('20'),contractType:'تجديد',startDate:C.dAdd(yo.endDate,1),endDate:C.dAdd(yo.endDate,366),previousContractId:yo.id});ok(rn.previousContractId===yo.id&&rn.status==='active'&&rn.employee.id===U('20'),'the renewal contract is created by HR and linked to the renewed one');
 ok((await C.call('Contract_Get',{id:pr.id})).decision&&await code('Contract_Get',{id:U('98')})==='Hr:Contract:NotFound','full details with both steps; unknown id -> 404');
 // reports
 const hc=await C.call('Report_GetHrContracts',{});ok(hc.items.length>=10&&hc.items.every(r=>r.typeName&&r.statusName&&r.employee.fullName&&!('recommendation' in r)),'HR contracts report (no financial or recommendation data)');
 ok((await C.call('Report_GetHrContracts',{endFrom:C.todayS(),endTo:C.dAdd(C.todayS(),40)})).items.every(r=>r.endDate>=C.todayS()&&r.endDate<=C.dAdd(C.todayS(),40)),'report end-date filter');
 ok((await C.call('Report_Export',{code:'hrContracts',format:'excel'})).fileName.endsWith('.xlsx'),'hrContracts export');
 ok(await code('Report_GetHrContracts',{})==='OK'&&await code('Contract_GetMyList',{})==='forbidden','HR has no “my contracts”');
 // UI
 await goto('con');ok(rows().length>=3&&vm().tools.length===1&&vm().blocks[0].hasKpi,'expiring tab: KPI banner, table, New contract');
 await tab('all');ok(rows().length>=10&&vm().flt.length===3,'all tab with filters');
 const some=C.d12.cons.find(c=>c.status==='pendingDecision');C.toSub('one',some.id);await sl(250);ok(C.state.sub==='one'&&vm().tools.some(t=>t.l===T.aCtDec)&&vm().blocks.length===3,'details: contract, recommendation, decision blocks + Decide');
 const rcn=C.d12.cons.find(c=>c.status==='renewed');C.toSub('one',rcn.id);await sl(250);ok(vm().tools.some(t=>t.l===T.aCtRenew),'a renewed contract offers “create renewal contract”');
 vm().tools.find(t=>t.l===T.aCtRenew).go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===5&&rv().dlgv.fields[0].value===rcn.emp&&rv().dlgv.fields[2].value===C.dAdd(rcn.end,1),'renewal dialog is prefilled (employee, start day after the previous end, previous contract)');C.dlgClose();
 await goto('rep');await tab('hcon');ok(rows().length>=10&&vm().flt.length===4,'reports: contracts tab for HR');
 // ---------- employee
 ok(await login('emp',true),'employee signed in');await goto('con');ok(tabs().join('|')==='عقودي','employee tab: my contracts');
 const my=await C.call('Contract_GetMyList',{});ok(my.items.length===1&&Object.keys(my.items[0]).sort().join()==='contractType,decidedAt,employeeMessage,endDate,id,isExpired,startDate,status','my contract view: no position, recommendation or decision details');
 ok((await C.call('Contract_GetMy',{id:my.items[0].id})).status==='active'&&await code('Contract_GetMy',{id:al.items.find(c=>c.employee.id===U('18')).id})==='Hr:Contract:NotFound','my contract by id; someone else’s -> 404');
 ok(await code('Contract_List',{})==='forbidden'&&await code('Contract_GetExpiring',{})==='forbidden'&&await code('Contract_Recommend',{id:rc.id,employeePosition:'x',recommendation:'renew'})==='forbidden','employee cannot use HR contract operations');
 await tab('my');ok(rows().length===1&&vm().blocks.length===2,'my contracts UI with the privacy note');
 rows()[0].acts[0].go();await sl(200);ok(C.state.sub==='mine'&&vm().blocks[0].kv.length===5,'my contract details');
 // decided employees see the message
 await login('xd',true);ok(await code('Contract_GetMyList',{})==='forbidden'&&await code('Contract_List',{})==='forbidden','executive and department managers are not part of the contract path');
 await login('mgr',true);ok(await code('Contract_List',{})==='forbidden'&&!nav().includes('العقود'),'department manager: no contracts (not a party to the path)');
 // ---------- settings
 ok(await login('adm',true),'system admin signed in');const cf=await C.call('SystemSettings_GetContracts',{});ok(cf.alertDays===30,'alert period starts at 30 days');
 ok(await code('SystemSettings_UpdateContracts',{alertDays:0,concurrencyStamp:cf.concurrencyStamp})==='validation'&&await code('SystemSettings_UpdateContracts',{alertDays:400,concurrencyStamp:cf.concurrencyStamp})==='validation','alertDays 1–365');
 ok(await code('SystemSettings_UpdateContracts',{alertDays:60,concurrencyStamp:'stale'})==='conflict','stale stamp -> 409');
 const up=await C.call('SystemSettings_UpdateContracts',{alertDays:60,concurrencyStamp:cf.concurrencyStamp});ok(up.alertDays===60&&up.lastModificationTime,'saved');
 const hub=await C.call('SystemSettings_ListSections',{});ok(hub.items.find(z=>z.code==='contracts').isEditableByCaller,'the hub lists contracts as editable by the system admin');
 await goto('set');ok(vm().blocks.find(b=>b.t===T.secL.contracts)&&vm().blocks.find(b=>b.t===T.secL.contracts).hasActs,'hub card for contracts is openable');C.nav2('ctr');await sl(200);ok(vm().blocks.length===2&&vm().blocks[1].hasForm,'contract alert screen: value + form');C.setF('a','45');await vm().blocks[1].acts.find(a=>a.cls==='p').go();await sl(250);ok(C.d12.alert===45,'form saves the alert period');
 await login('hr',true);const ex2=await C.call('Contract_GetExpiring',{});ok(ex2.alertDays===45,'the new period applies to the expiring list');
 ok(C.d8.notes.some(n=>n.user===U('13')&&n.code==='contractExpiring'),'HR got a contractExpiring notification (once)');
 await C.ntOpen({id:C.d8.notes.find(n=>n.user===U('13')&&n.code==='contractExpiring').id,templateCode:'contractExpiring',resourceId:null,isRead:true});await sl(400);ok(C.state.page==='con'&&C.state.tab==='exp','opening the notification goes to expiring contracts');
 for(const pr of ['hr','emp','adm']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const [pg,tb,sb] of [['con','exp','list'],['con','all','list'],['con','my','list'],['set','data','ctr'],['rep','hcon','list']]){if(pg==='con'&&tb!=='my'&&pr!=='hr')continue;if(pg==='con'&&tb==='my'&&pr!=='emp')continue;if(pg==='set'&&pr!=='adm')continue;if(pg==='rep'&&pr!=='hr')continue;C.setState({page:pg,tab:tb,sub:sb,sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render '+pg+'/'+tb+'/'+sb+' '+pr+' '+L)}catch(e){ok(false,'render '+pg+'/'+tb+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
