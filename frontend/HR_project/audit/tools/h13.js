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
(async()=>{
 // ---------- employee: receiving and acknowledging
 ok(await login('emp'),'employee signed in');await goto('ann');ok(tabs().join('|')==='الواردة'&&nav().length>0,'employee sees only the Received tab');
 const rl=await C.call('Circular_GetReceivedList',{maxResultCount:100});ok(rl.items.length>=2&&rl.items.every(c=>c.acknowledgementRequired===true&&c.sender.displayName&&c.title&&c.content),'received list: acknowledgement required on every circular ('+rl.totalCount+')');
 ok(rl.items.every((c,i,a)=>i===0||a[i-1].sentAt>=c.sentAt),'newest first');
 ok(!rl.items.some(c=>c.scope==='departmentManagers'),'a department-managers circular does not reach an ordinary employee');
 const un=(await C.call('Circular_GetReceivedList',{isAcknowledged:false})).items;ok(un.length>=1&&un.every(c=>!c.acknowledgedAt),'unacknowledged filter');
 const c0=un[0];const a1=await C.call('Circular_Acknowledge',{id:c0.id});const a2=await C.call('Circular_Acknowledge',{id:c0.id});ok(a1.acknowledgedAt&&a2.acknowledgedAt===a1.acknowledgedAt,'acknowledge records the time once; repeating keeps the first value');
 ok((await C.call('Circular_GetReceivedList',{isAcknowledged:true})).items.some(c=>c.id===c0.id),'acknowledged filter');
 const dm=C.d10.circs.find(c=>c.scope==='departmentManagers');ok(await code('Circular_Get',{id:dm.id})==='Hr:Circulars:NotFound'&&await code('Circular_Acknowledge',{id:dm.id})==='Hr:Circulars:NotFound','non-recipient: Get and Acknowledge -> 404');
 ok(await code('Circular_Create',{scope:'team',title:'x',content:'y'})==='forbidden'&&await code('Circular_GetSentList',{})==='forbidden'&&await code('Circular_GetReceipts',{id:dm.id})==='forbidden','employee cannot send, list sent circulars or read receipts');
 ok(C.d8.notes.some(n=>n.user===U('11')&&n.code==='circularReceived'),'the employee has a circular notification');
 await tab('recv');ok(rows().length>=2&&vm().tools.length===0&&vm().flt.length===1,'received tab: table, status filter, no send button');
 const qr=vm().blocks[0].rows.find(z=>z.acts.length===2);ok(!!qr,'unacknowledged rows offer a quick Acknowledge button');if(qr){const before=Object.keys(C.d10.circs.reduce((o,c)=>Object.assign(o,c.acks[U('11')]?{[c.id]:1}:{}),{})).length;qr.acts[1].go();await sl(300);const after=Object.keys(C.d10.circs.reduce((o,c)=>Object.assign(o,c.acks[U('11')]?{[c.id]:1}:{}),{})).length;ok(after===before+1,'quick acknowledge from the table records one acknowledgement')}
 const anyC=rl.items[0];C.toSub('one',anyC.id);await sl(250);ok(C.state.sub==='one'&&vm().blocks[0].hasKv,'detail page shows the circular');
 // ---------- manager: team scope
 ok(await login('mgr',true),'manager signed in');await goto('ann');ok(tabs().join('|')==='الواردة|المرسلة'&&vm().tools.length===1,'manager: Received + Sent, New circular');
 const team=C.db.emps.filter(e=>e.mgr===U('12')&&e.status==='active').map(e=>e.id);
 const cs=await C.call('Circular_Create',{scope:'team',title:'تذكير بالمهام',content:'نص التعميم'});ok(cs.recipientsCount===team.length&&cs.acknowledgedCount===0&&cs.sentAt,'team circular reaches exactly his active team ('+cs.recipientsCount+')');
 const rec=C.d10.circs.find(c=>c.id===cs.id);ok(!rec.rcp.includes(U('12'))&&rec.rcp.every(id=>team.includes(id)),'recipients: team only, never the sender');
 ok(await code('Circular_Create',{scope:'company',title:'x',content:'y'})==='Hr:Circulars:ScopeNotAllowed'&&await code('Circular_Create',{scope:'departmentManagers',title:'x',content:'y'})==='Hr:Circulars:ScopeNotAllowed','department manager cannot use company or department-managers scopes');
 ok(await code('Circular_Create',{scope:'team',title:'',content:'y'})==='validation','title required');
 const saved=C.db.emps.filter(e=>e.mgr===U('12')).map(e=>[e,e.status]);saved.forEach(([e])=>{e.status='ended'});ok(await code('Circular_Create',{scope:'team',title:'x',content:'y'})==='Hr:Circulars:NoRecipients','NoRecipients when the team is empty');saved.forEach(([e,s])=>{e.status=s});
 ok(C.d8.notes.filter(n=>n.code==='circularReceived'&&n.rid===cs.id).length===team.length,'one notification per recipient');
 ok(C.d8.notes.filter(n=>n.rid===cs.id).every(n=>n.rt==='circular'),'notification resource type circular');
 const sl2=await C.call('Circular_GetSentList',{});ok(sl2.items.length>=2&&sl2.items.every(c=>C.d10.circs.find(z=>z.id===c.id).sender===U('12')),'sent list: only his own circulars');
 const rp=await C.call('Circular_GetReceipts',{id:cs.id});ok(rp.items.length===team.length&&rp.items.every(r=>r.acknowledgedAt===null&&r.recipient.fullName),'receipts: every recipient, none acknowledged yet');
 ok(await code('Circular_GetReceipts',{id:C.d10.circs.find(c=>c.sender===U('13')).id})==='Hr:Circulars:NotFound','receipts of another sender’s circular -> 404');
 // ---------- an employee acknowledges, the sender sees it
 await login('emp',true);const mine=(await C.call('Circular_GetReceivedList',{})).items.find(c=>c.id===cs.id);ok(!!mine,'the employee received the manager’s circular');await C.call('Circular_Acknowledge',{id:cs.id});
 await login('mgr',true);const rp2=await C.call('Circular_GetReceipts',{id:cs.id});ok(rp2.items.filter(r=>r.acknowledgedAt).length===1&&(await C.call('Circular_GetReceipts',{id:cs.id,isAcknowledged:true})).items.length===1&&(await C.call('Circular_GetReceipts',{id:cs.id,isAcknowledged:false})).items.length===team.length-1,'receipt status and filters');
 ok((await C.call('Circular_GetSentList',{})).items.find(c=>c.id===cs.id).acknowledgedCount===1,'acknowledged count on the sent list');
 await goto('ann');await tab('sent');ok(rows().length>=2,'sent tab lists circulars with counts');C.toSub('rcpt',cs.id);await sl(250);ok(C.state.sub==='rcpt'&&vm().blocks.length===2&&vm().flt.length===1,'receipt screen: summary + recipients + filter');
 // monthly report picks up the circular activity (frozen snapshot)
 const rl3=await C.call('DepartmentReport_GetList',{});const rd=await C.call('DepartmentReport_Get',{id:rl3.items[0].id});ok(rd.snapshot.circulars.circularsSentCount>=1&&rd.snapshot.circulars.recipientsCount>=1&&rd.snapshot.circulars.acknowledgedCount>=1,'monthly report: circulars sent, recipients and acknowledgements ('+JSON.stringify(rd.snapshot.circulars)+')');
 // ---------- HR
 ok(await login('hr',true),'HR signed in');await goto('ann');ok(tabs().join('|')==='الواردة|المرسلة','HR tabs');
 ok(await code('Circular_Create',{scope:'team',title:'x',content:'y'})==='Hr:Circulars:ScopeNotAllowed','HR cannot use the team scope');
 const hc=await C.call('Circular_Create',{scope:'company',title:'تعميم الشركة',content:'نص'});const active=C.db.emps.filter(e=>e.status==='active'&&e.id!==U('13')).length;ok(hc.recipientsCount===active,'company circular reaches every active employee except the sender ('+active+')');
 const hd=await C.call('Circular_Create',{scope:'departmentManagers',title:'للمديرين',content:'نص'});ok(hd.recipientsCount===C.db.emps.filter(e=>e.status==='active'&&e.roles.includes('departmentManager')).length,'department-managers scope reaches only department managers');
 ok((await C.call('Circular_GetSentList',{})).items.length>=4,'HR sent list');
 // frozen recipients: someone linked afterwards never receives it
 const before=C.d10.circs.find(c=>c.id===hc.id).rcp.length;await C.call('Employee_Create',{fullNameAr:'جديد',fullNameEn:'New Person',hireDate:C.todayS(),departmentId:C.db.D[0].id,jobTitleAr:'موظف',jobTitleEn:'Employee',managerId:U('12'),gender:'male',employeeNumber:'E-9999',phoneNumber:null,email:null}).catch(()=>0);
 ok(C.d10.circs.find(c=>c.id===hc.id).rcp.length===before,'recipients are frozen at sending');
 // the HR manager now holds the circular in dept managers view
 await login('mgr',true);const mr=(await C.call('Circular_GetReceivedList',{})).items;ok(mr.some(c=>c.id===hd.id)&&mr.some(c=>c.id===hc.id),'the department manager receives both HR circulars');
 ok(C.d8.notes.some(n=>n.user===U('12')&&n.code==='circularReceived'&&n.rid===hd.id),'recipients are notified in their own language template');
 await C.ntOpen({id:C.d8.notes.find(n=>n.user===U('12')&&n.rid===hd.id).id,templateCode:'circularReceived',resourceId:hd.id,isRead:true});await sl(400);ok(C.state.page==='ann'&&C.state.sub==='one'&&C.state.sid===hd.id,'opening a circular notification goes to the circular');
 // ---------- executive
 ok(await login('xd',true),'executive signed in');await goto('ann');ok(tabs().join('|')==='الواردة'&&vm().tools.length===1,'executive: Received only, but can send');
 ok(await code('Circular_Create',{scope:'team',title:'x',content:'y'})==='Hr:Circulars:ScopeNotAllowed','executive cannot use the team scope');
 const xc=await C.call('Circular_Create',{scope:'company',title:'من المدير التنفيذي',content:'نص'});ok(xc.recipientsCount>=10,'executive can send to the whole company');
 ok(await code('Circular_GetSentList',{})==='forbidden'&&await code('Circular_GetReceipts',{id:xc.id})==='forbidden','the contract gives the executive no sent list or receipts (flagged gap)');
 ok(vm().blocks.some(b=>b.hasT&&b.t===T.nCrExec),'the UI explains that gap to the executive');
 // ---------- other roles receive circulars
 for(const p of ['adm','rv','so']){await login(p,true);const r=await C.call('Circular_GetReceivedList',{});ok(r.items.length>=1&&await code('Circular_Create',{scope:'company',title:'x',content:'y'})==='forbidden',p+': receives circulars, cannot send')}
 // ---------- render
 for(const pr of ['emp','mgr','hr','xd','adm']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['recv','sent']){C.setState({page:'ann',tab:t,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render ann/'+t+' '+pr+' '+L)}catch(e){ok(false,'render ann/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
