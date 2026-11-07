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
const U=(n)=>C.UID(n),E=(n)=>C.db.emps.find(e=>e.en===n);
const itemBy=(en)=>C.d11.items.find(i=>i.en===en);const qty=(en)=>C.stQty(itemBy(en).id);
(async()=>{
 // ---------- employee submits a store request (full approval chain)
 ok(await login('emp'),'employee signed in');
 const ty=await C.call('RequestType_GetList',{});const store=ty.items.find(t=>t.code==='storeItem');ok(store&&store.requiresItem&&store.destination==='store','the store-item request type is now active');
 const lk=await C.call('Item_Lookup',{});ok(lk.items.length===7&&lk.items.every(i=>Object.keys(i).sort().join()==='id,kind,name'),'Item_Lookup: active items, no quantities');
 ok(await code('Request_Submit',{requestTypeId:store.id,description:'x'})==='Hr:Request:ItemFieldsInvalid','item and quantity are required for this type');
 const pens=itemBy('Pens');const rq=await C.call('Request_Submit',{requestTypeId:store.id,description:'للمكتب',itemId:pens.id,quantity:10});
 ok(rq.status==='pendingManager'&&rq.steps.map(s=>s.approverRole).join()==='departmentManager,hrManager,storeOfficer'&&/Pens|أقلام/.test(rq.detailsSummary),'steps: manager -> HR -> store officer; summary shows the item and quantity');
 ok(await code('Custody_GetList',{})==='forbidden'&&await code('Item_GetList',{})==='forbidden'&&await code('Stock_GetMovements',{})==='forbidden','employee cannot read stock, items or all custody');
 const my=await C.call('Custody_GetMy',{});ok(my.items.length>=1&&my.items.every(c=>c.kind==='personal'&&c.employee.id===U('11')),'my custody: personal only');
 const lap=my.items[0];ok(await code('Custody_Acknowledge',{id:lap.id})==='Hr:Custody:AlreadyAcknowledged','AlreadyAcknowledged');
 ok((await C.call('Custody_Get',{id:lap.id})).item.name,'own custody opens');
 const others=C.d11.custs.find(c=>c.emp&&c.emp!==U('11'));ok(await code('Custody_Get',{id:others.id})==='forbidden','another employee’s custody -> 403');
 // manager approves
 await login('mgr',true);await C.call('Request_Approve',{id:rq.id});
 await login('hr',true);const ap=await C.call('Request_Approve',{id:rq.id});ok(ap.status==='pendingDestination'&&ap.employeeStatus==='approved'&&ap.stockIssue&&ap.stockIssue.status==='pendingAcknowledgement','after HR the request waits for the store and a stock issue exists');
 const si=C.d11.issues.find(i=>i.req===rq.id);const so=E('Ahmed Al-Zubaidi');
 ok(C.d8.notes.some(n=>n.user===so.id&&n.code==='stockIssuePending'&&n.rid===si.id)&&!C.d8.notes.some(n=>n.user===so.id&&n.code==='requestPendingAction'&&n.rid===rq.id),'store officer gets stockIssuePending (not a duplicate approval notification)');
 // HR request goes straight to the store
 const hrq=await C.call('Request_Submit',{requestTypeId:store.id,description:'hr',itemId:itemBy('Laptop').id,quantity:1});ok(hrq.status==='pendingDestination'&&C.d11.issues.some(i=>i.req===hrq.id),'an HR request reaches the store directly (no approvals)');
 ok(await code('Request_Approve',{id:rq.id})==='forbidden','HR has no decision at the store step (403)');
 // ---------- store officer
 ok(await login('so',true),'store officer signed in');await goto('inv');
 ok(nav().includes('المخزن')&&nav().includes('العهد')&&tabs().join('|')==='الأصناف|حركات المخزون|طلبات الصرف','store officer: Store + Custody; 3 store tabs');
 const inb=(await C.call('Request_GetInbox',{})).items;const ix=inb.find(z=>z.id===rq.id);ok(ix&&ix.actionKind==='stockIssue'&&ix.stockIssueId===si.id&&ix.currentApproverRole==='storeOfficer','request inbox: store items open the stock-issue flow (actionKind stockIssue)');
 ok(await code('Request_Approve',{id:rq.id})==='forbidden','the store officer has no Request_Approve call: the stock-issue flow is the only way to execute the request');
 const il=await C.call('Item_GetList',{});ok(il.items.length===7&&il.items.every(i=>!('cost' in i)&&!('totalCost' in i)&&typeof i.quantityOnHand==='number'&&typeof i.isBelowMinimum==='boolean'),'item list: kind, on hand, minimum, below-minimum flag; no cost');
 ok((await C.call('Item_GetList',{belowMinimum:true})).items.some(i=>i.nameEn==='Toner'),'below-minimum filter finds toner');
 ok((await C.call('Item_GetList',{kind:'consumable'})).items.length===3&&(await C.call('Item_GetList',{filter:'laptop'})).items.length===1,'kind and name filters');
 ok(await code('Item_Create',{nameAr:'ورق A4',nameEn:'X',kind:'consumable',minQuantity:1})==='Hr:Item:NameAlreadyExists','duplicate item name -> 409');
 const ni=await C.call('Item_Create',{nameAr:'ماوس',nameEn:'Mouse',kind:'returnable',minQuantity:2});ok(ni.quantityOnHand===0&&ni.isBelowMinimum&&ni.concurrencyStamp,'new item starts at zero');
 const nu=await C.call('Item_Update',{id:ni.id,nameAr:'ماوس',nameEn:'Mouse',kind:'consumable',minQuantity:2,isActive:true,concurrencyStamp:ni.concurrencyStamp});ok(nu.kind==='consumable','kind can change before the first movement');
 await C.call('Stock_Receive',{itemId:ni.id,quantity:3});const g=await C.call('Item_Get',{id:ni.id});
 ok(await code('Item_Update',{id:ni.id,nameAr:'ماوس',nameEn:'Mouse',kind:'returnable',minQuantity:2,isActive:true,concurrencyStamp:g.concurrencyStamp})==='Hr:Item:KindLocked','KindLocked after the first movement');
 ok(await code('Item_Update',{id:ni.id,nameAr:'ماوس',nameEn:'Pens',kind:'consumable',minQuantity:2,isActive:true,concurrencyStamp:g.concurrencyStamp})==='Hr:Item:NameAlreadyExists'&&await code('Item_Update',{id:ni.id,nameAr:'ماوس',nameEn:'Mouse',kind:'consumable',minQuantity:2,isActive:true,concurrencyStamp:'stale'})==='conflict','name clash and stale stamp on update');
 await C.call('Item_Update',{id:ni.id,nameAr:'ماوس',nameEn:'Mouse',kind:'consumable',minQuantity:2,isActive:false,concurrencyStamp:g.concurrencyStamp});ok(await code('Stock_Receive',{itemId:ni.id,quantity:1})==='Hr:Stock:ItemInactive'&&!(await C.call('Item_Lookup',{})).items.some(i=>i.id===ni.id),'inactive item: no receipts and hidden from the lookup (no deletion exists)');
 // issue flow: consumable
 const before=qty('Pens');const iss=await C.call('StockIssue_Get',{id:si.id});ok(iss.status==='pendingAcknowledgement'&&iss.isAvailable&&iss.quantityOnHand===before&&iss.employee.fullName&&!('cost' in iss),'issue: availability and on-hand shown');
 const ak=await C.call('StockIssue_Acknowledge',{id:si.id});ok(ak.status==='acknowledged'&&ak.acknowledgedBy&&qty('Pens')===before,'acknowledge: status changes, quantity does not');
 ok(await code('StockIssue_Acknowledge',{id:si.id})==='Hr:Stock:IssueInvalidState'&&await code('StockIssue_MarkPurchasing',{id:C.d11.issues[0].id})!=='Hr:Nothing','state machine guards');
 const dv=await C.call('StockIssue_Deliver',{id:si.id,note:'سُلّم'});ok(dv.status==='delivered'&&dv.deliveredBy&&dv.stockMovementId&&!dv.custodyId&&qty('Pens')===before-10,'deliver (consumable): quantity deducted here only, no custody created');
 const mv=(await C.call('Stock_GetMovements',{itemId:pens.id})).items;ok(mv[0].type==='issue'&&mv[0].quantity===-10&&mv[0].balanceAfter===before-10&&mv[0].requestId===rq.id&&mv[0].stockIssueId===si.id,'issue movement is linked to the request and the stock issue');
 ok(await code('StockIssue_Deliver',{id:si.id})==='Hr:Stock:IssueInvalidState','cannot deliver twice');
 await login('emp',true);const mine=(await C.call('Request_GetMy',{})).items.find(r=>r.id===rq.id);ok(mine.employeeStatus==='completed'&&/سُلّم|Delivered/.test(mine.stockIssueStatusLabel||''),'the employee sees the request completed with the store status');
 ok(C.d8.notes.some(n=>n.user===U('11')&&n.rid===rq.id&&n.code==='requestStatusChanged'),'employee notified');
 // purchasing flow
 await login('so',true);const ph=C.d11.issues.find(i=>i.status==='purchasing');const phi=await C.call('StockIssue_Get',{id:ph.id});ok(!phi.isAvailable&&phi.purchasingAt,'seeded purchasing issue is unavailable (on hand '+phi.quantityOnHand+' < '+phi.quantity+')');
 ok(await code('StockIssue_Deliver',{id:ph.id})==='Hr:Stock:InsufficientStock','InsufficientStock on delivery');
 ok(await code('Stock_Receive',{itemId:ph.item,quantity:5,stockIssueId:ph.id})==='Hr:Stock:PurchaseCostRequired','cost is mandatory when buying for an issue');
 ok(await code('Stock_Receive',{itemId:pens.id,quantity:5,totalCost:100,stockIssueId:ph.id})==='Hr:Stock:IssueItemMismatch','IssueItemMismatch');
 ok(await code('Stock_Receive',{itemId:ph.item,quantity:5,totalCost:100,stockIssueId:C.d11.issues.find(i=>i.status==='acknowledged').id})==='Hr:Stock:IssueNotPurchasing','IssueNotPurchasing');
 const rc=await C.call('Stock_Receive',{itemId:ph.item,quantity:5,totalCost:250000,stockIssueId:ph.id,note:'شراء'});ok(rc.type==='receive'&&rc.quantity===5&&!('cost' in rc)&&!('totalCost' in rc)&&JSON.stringify(rc).indexOf('250000')===-1,'receive: the cost is never returned');
 ok(C.d11.expenses.some(e=>e.move===rc.id&&e.amount===250000&&e.category==='stockPurchases'),'the cost became a stock-purchase expense for the accountant');
 ok((await C.call('StockIssue_Get',{id:ph.id})).status==='purchasing','the issue stays purchasing until delivery');
 const dv2=await C.call('StockIssue_Deliver',{id:ph.id,custodyKind:'shared'});const cu2=C.d11.custs.find(c=>c.id===dv2.custodyId);ok(dv2.status==='delivered'&&cu2.kind==='shared'&&cu2.dept&&!cu2.emp&&cu2.status==='pendingAcknowledgement','deliver (returnable, shared): custody for the employee’s department, pending acknowledgement');
 const mk=C.d11.issues.find(i=>i.status==='acknowledged'&&i.id!==si.id);const mp=await C.call('StockIssue_MarkPurchasing',{id:mk.id,note:'غير متوفر'});ok(mp.status==='purchasing'&&mp.purchasingAt,'unavailable item: marked waiting for purchase, never auto-rejected');
 ok(C.d8.notes.some(n=>n.user===mk.emp&&n.rid===mk.req),'employee notified of the status');
 // adjustments
 const tonerId=itemBy('Toner').id;
 ok(await code('Stock_Adjust',{itemId:tonerId,quantity:0,reason:'x'})==='Hr:Stock:ZeroQuantity'&&await code('Stock_Adjust',{itemId:tonerId,quantity:-999,reason:'x'})==='Hr:Stock:InsufficientStock'&&await code('Stock_Adjust',{itemId:tonerId,quantity:1})==='validation','adjust: zero, negative balance and reason checks');
 const rc3=await C.call('Stock_Receive',{itemId:pens.id,quantity:3,totalCost:900});const ad=await C.call('Stock_Adjust',{itemId:pens.id,quantity:-3,reason:'عكس الاستلام',reversesMovementId:rc3.id});ok(ad.type==='adjustment'&&ad.reversesMovementId===rc3.id&&ad.quantity===-3,'reversal is a new adjustment movement');
 ok(C.d11.expenses.find(e=>e.move===rc3.id).reversed===true,'reversing a purchase reverses its expense for the accountant');
 ok(await code('Stock_Adjust',{itemId:pens.id,quantity:-1,reason:'x',reversesMovementId:rc3.id})==='Hr:Stock:MovementAlreadyReversed','MovementAlreadyReversed');
 const mvs=await C.call('Stock_GetMovements',{type:'adjustment'});ok(mvs.items.every(m=>m.type==='adjustment')&&(await C.call('Stock_GetMovements',{itemId:tonerId})).items.every(m=>m.item.id===tonerId),'movement filters; movements are never edited or deleted');
 // direct custody
 const paper=itemBy('A4 paper'),mon=itemBy('Monitor');
 ok(await code('Custody_Create',{itemId:paper.id,kind:'personal',employeeId:U('11'),quantity:1,deductFromStock:true})==='Hr:Custody:ItemNotReturnable','consumables never become custody');
 ok(await code('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('11'),departmentId:C.db.D[0].id,quantity:1,deductFromStock:false})==='Hr:Custody:InvalidHolder'&&await code('Custody_Create',{itemId:mon.id,kind:'shared',quantity:1,deductFromStock:false})==='Hr:Custody:InvalidHolder','InvalidHolder: an employee or a department, never both or neither');
 const q0=qty('Monitor');const cc=await C.call('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('19'),quantity:2,deductFromStock:true,note:'للمكتب'});ok(cc.status==='pendingAcknowledgement'&&cc.employee.id===U('19')&&qty('Monitor')===q0-2,'direct custody with stock deduction');
 const q1=qty('Monitor');const cc2=await C.call('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('19'),quantity:1,deductFromStock:false});ok(qty('Monitor')===q1,'direct custody without deduction (already with the employee) leaves stock unchanged');
 ok(await code('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('19'),quantity:9999,deductFromStock:true})==='Hr:Stock:InsufficientStock','InsufficientStock on a deducting custody');
 ok(C.d8.notes.some(n=>n.user===U('19')&&n.code==='custodyAssigned'&&n.rid===cc.id),'the holder is notified');
 await C.call('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('24'),quantity:1,deductFromStock:true});
 const cl=await C.call('Custody_GetList',{});ok(cl.items.length>=7&&(await C.call('Custody_GetList',{kind:'shared'})).items.every(c=>c.kind==='shared')&&(await C.call('Custody_GetList',{status:'returned'})).items.every(c=>c.status==='returned'),'all custody with kind and status filters');
 // returns
 const q2=qty('Monitor');ok(await code('Custody_Return',{id:cc2.id,restock:false})==='Hr:Custody:ReturnNoteRequired','a non-restocked return needs a note');
 const r1=await C.call('Custody_Return',{id:cc2.id,restock:false,note:'تالف'});ok(r1.status==='returned'&&r1.returnedAt&&qty('Monitor')===q2,'damaged return: closed without touching stock');
 const r2=await C.call('Custody_Return',{id:cc.id,restock:true});ok(r2.status==='returned'&&qty('Monitor')===q2+2&&C.d11.moves.some(m=>m.type==='return'&&m.cust===cc.id),'restocked return adds a return movement for the full quantity');
 ok(await code('Custody_Return',{id:cc.id,restock:true})==='Hr:Custody:AlreadyReturned'&&await code('Custody_Acknowledge',{id:cc.id})==='forbidden','AlreadyReturned; the store officer cannot acknowledge for others (403)');
 ok(C.d8.notes.some(n=>n.user===U('19')&&n.code==='custodyReturned'),'the holder is notified of the return');
 // ---------- manager: team custody, shared acknowledgement, counts
 await login('mgr',true);await goto('cus');ok(tabs().join('|')==='عهدتي|عهد الفريق والقسم|الجرد','manager custody tabs');
 const tm=await C.call('Custody_GetTeam',{maxResultCount:100});const teamIds=C.db.emps.filter(e=>e.mgr===U('12')).map(e=>e.id),dept=C.db.emps.find(e=>e.id===U('12')).dept;
 ok(tm.items.length>=5&&tm.items.every(c=>c.kind==='personal'?teamIds.includes(c.employee.id):C.db.emps.filter(e=>e.mgr===U('12')).some(e=>e.dept===c.department.id)),'team custody: team personal + shared of his departments');
 ok((await C.call('Custody_GetMy',{})).items.every(c=>c.employee&&c.employee.id===U('12')),'shared custody is not listed under “my custody”');
 const pend=tm.items.find(c=>c.kind==='shared'&&c.status==='pendingAcknowledgement');ok(!!pend,'the delivered shared custody waits for the manager');
 const ack=await C.call('Custody_Acknowledge',{id:pend.id});ok(ack.status==='acknowledged'&&ack.acknowledgedBy.id===U('12'),'shared custody: the department manager acknowledges and his name is recorded');
 ok(await code('Custody_Get',{id:C.d11.custs.find(c=>c.kind==='personal'&&(C.db.emps.find(e=>e.id===c.emp)||{}).mgr!==U('12')&&c.emp!==U('12')).id})==='forbidden'&&await code('Custody_GetList',{})==='forbidden','custody outside the team -> 403; no all-custody list');
 // counts
 const sh=tm.items.filter(c=>c.kind==='shared'&&c.status!=='returned');const shD=sh.filter(c=>c.department.id===dept);const outDept=U('99');
 ok(await code('CustodyCount_Create',{departmentId:outDept,countDate:C.todayS(),lines:[{custodyId:sh[0].id,countedQuantity:1}]})==='Hr:Custody:DepartmentNotInScope','DepartmentNotInScope');
 ok(await code('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),lines:[]})==='validation','at least one line');
 ok(await code('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),lines:[{custodyId:shD[0].id,countedQuantity:shD[0].quantity}]})==='Hr:Custody:CountLinesIncomplete','CountLinesIncomplete: every open shared custody must be counted');
 ok(await code('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),lines:shD.map(c=>({custodyId:c.id,countedQuantity:c.quantity})).concat([{custodyId:lap.id,countedQuantity:1}])})==='Hr:Custody:CountLineInvalid','CountLineInvalid for a personal custody');
 const q3={mon:qty('Monitor'),prn:qty('Printer')};const cn=await C.call('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),note:'جرد شهري',lines:shD.map(c=>({custodyId:c.id,countedQuantity:c.quantity}))});
 ok(cn.result==='matched'&&cn.lines.length===shD.length&&cn.countedBy.displayName&&qty('Monitor')===q3.mon&&qty('Printer')===q3.prn,'matching count: result matched; custody and stock unchanged');
 const dc=await C.call('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),lines:shD.map((c,i)=>({custodyId:c.id,countedQuantity:i===0?0:c.quantity}))});ok(dc.result==='discrepancy','a difference makes the result discrepancy');
 ok(C.d8.notes.some(n=>n.user===so.id&&n.code==='custodyCountDifference'&&n.rid===dc.id)&&!C.d8.notes.some(n=>n.user===so.id&&n.code==='custodyCountDifference'&&n.rid===cn.id),'only a discrepancy notifies the store officer');
 ok((await C.call('CustodyCount_GetList',{})).items.length>=3&&(await C.call('CustodyCount_GetList',{result:'discrepancy'})).items.every(c=>c.result==='discrepancy'),'count history with a result filter');
 const rp=await C.call('DepartmentReport_GetList',{});const rd=await C.call('DepartmentReport_Get',{id:rp.items[0].id});ok(rd.snapshot.custody.sharedItemsCount>=2&&rd.snapshot.custody.lastCountDate,'monthly report: custody summary (shared count and last count date)');
 const rc2=await C.call('Report_GetTeamCustody',{});ok(rc2.items.length>=5&&rc2.items.every(r=>r.itemName&&r.statusName)&&(await C.call('Report_GetTeamCustody',{kind:'shared'})).items.every(r=>r.kind==='shared'),'team custody report');
 // ---------- HR and others
 await login('hr',true);ok((await C.call('Custody_GetList',{})).items.length>=7&&(await C.call('CustodyCount_GetList',{})).items.length>=3,'HR reads all custody and the count history');
 ok(await code('CustodyCount_Create',{departmentId:dept,countDate:C.todayS(),lines:[{custodyId:sh[0].id,countedQuantity:1}]})==='forbidden'&&await code('Custody_Create',{itemId:mon.id,kind:'personal',employeeId:U('11'),quantity:1,deductFromStock:false})==='forbidden'&&await code('Stock_Receive',{itemId:mon.id,quantity:1})==='forbidden','HR cannot count, create custody or receive stock');
 for(const p of ['adm','xd','rv']){await login(p,true);ok(await code('Item_GetList',{})==='forbidden'&&await code('Custody_GetList',{})==='forbidden'&&(await C.call('Custody_GetMy',{})).items.length===0,p+': no store access; own custody list is empty')}
 // ---------- incident with an item
 await login('emp',true);const inc=await C.call('Incident_Create',{itemId:itemBy('Laptop').id,description:'الحاسوب بطيء'});ok(inc.item&&inc.item.name&&inc.item.kind==='returnable','a fault report can reference an item');
 // ---------- UI
 await login('so',true);await goto('inv');await tab('items');ok(rows().length===8&&vm().tools.length===3&&vm().flt.length===4,'items tab: table, filters, Receive / Adjust / New item');
 C.dlgOpen(C.svItemDlg(T,null));ok(rv().dlgv.fields.length===4,'item dialog');C.dlgClose();
 vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===5&&rv().dlgv.fields[2].l.includes('محاسب'),'receive dialog: the cost field says accountant only');C.dlgClose();
 await tab('moves');ok(rows().length>=10&&vm().flt.length===4,'movements tab');await tab('issues');ok(rows().length>=3,'issues tab lists requests');
 const row0=vm().blocks[0].rows.find(r=>r.acts.some(a=>a.l===T.aSvAck));ok(!!row0,'a waiting issue offers the “Seen” action');
 await goto('cus');ok(tabs().join('|')==='عهدتي|كل العهد|الجرد','store officer custody tabs');await tab('all');ok(rows().length>=7&&vm().tools.length===1,'all-custody tab with New custody');
 rows()[0].acts[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.hasKv,'custody details dialog');C.dlgClose();
 await login('mgr',true);await goto('cus');await tab('count');ok(rows().length>=3&&vm().tools.length===1,'count tab with New count');
 vm().tools[0].go();await sl(200);ok(C.state.sub==='cnew'&&vm().blocks[0].hasForm&&vm().blocks[0].fields.length>=4,'count form: department, date, one line per open shared custody');
 await login('emp',true);await goto('req');await tab('my');vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.some(f=>f.l.includes('القطعة')),'new-request dialog offers an item and quantity for store requests');C.dlgClose();
 // ---------- render all pages in both languages
 for(const pr of ['so','mgr','hr','emp','adm']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const [pg,tb] of [['inv','items'],['inv','moves'],['inv','issues'],['cus','my'],['cus','team'],['cus','all'],['cus','count']]){C.setState({page:pg,tab:tb,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render '+pg+'/'+tb+' '+pr+' '+L)}catch(e){ok(false,'render '+pg+'/'+tb+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
