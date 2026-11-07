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
const U=(n)=>C.UID(n);const E=(n)=>C.db.emps.find(e=>e.en===n);
(async()=>{
 ok(await login('xd'),'executive signed in');await goto('apr');
 ok(nav().includes('كتب الشكر')&&tabs().join('|')==='كل الكتب|سجل موظف','executive tabs: all letters + employee record');
 const f0={name:'letter.pdf',size:50000};
 ok(await code('AppreciationLetter_Create',{employeeId:U('99'),title:'x',file:f0})==='Hr:Appreciation:EmployeeNotFound','EmployeeNotFound');
 ok(await code('AppreciationLetter_Create',{employeeId:E('Layth Al-Obaidi').id,title:'x',file:f0})==='Hr:Appreciation:SelfIssueNotAllowed','SelfIssueNotAllowed');
 ok(await code('AppreciationLetter_Create',{employeeId:U('11'),title:'',file:f0})==='validation'&&await code('AppreciationLetter_Create',{employeeId:U('11'),title:'x'})==='validation','title and file are required');
 ok(await code('AppreciationLetter_Create',{employeeId:U('11'),title:'x',file:{name:'a.exe',size:10}})==='Hr:Document:FileTypeNotAllowed'&&await code('AppreciationLetter_Create',{employeeId:U('11'),title:'x',file:{name:'a.pdf',size:20000000}})==='Hr:Document:FileTooLarge'&&await code('AppreciationLetter_Create',{employeeId:U('11'),title:'x',file:{name:'virus.pdf',size:10}})==='Hr:Appreciation:FileRejected','file checks: type, size, scan');
 const nl=await C.call('AppreciationLetter_Create',{employeeId:U('24'),title:'شكر على الجهد',note:'ملاحظة',file:f0});
 ok(nl.employee.id===U('24')&&nl.issuedBy.displayName&&nl.file.fileName==='letter.pdf'&&nl.isAcknowledged===false&&nl.acknowledgedAt===null,'a letter is issued with its file, unacknowledged');
 ok(C.d8.notes.some(n=>n.user===U('24')&&n.code==='appreciationLetterIssued'&&n.rid===nl.id&&n.rt==='appreciationLetter'),'the employee is notified (appreciationLetterIssued)');
 const al=await C.call('AppreciationLetter_List',{maxResultCount:100});ok(al.items.length>=5&&al.items.every((l,i,a)=>i===0||a[i-1].issuedAt>=l.issuedAt),'all letters, newest first ('+al.totalCount+')');
 ok((await C.call('AppreciationLetter_List',{isAcknowledged:true})).items.every(l=>l.isAcknowledged)&&(await C.call('AppreciationLetter_List',{employeeId:U('11')})).items.every(l=>l.employee.id===U('11')),'filters (acknowledged, employee)');
 ok((await C.call('AppreciationLetter_GetEmployeeFile',{employeeId:U('18')})).items.length===1,'employee record');
 const dl=await C.call('AppreciationLetter_GetDownloadUrl',{id:nl.id});ok(/^https:/.test(dl.url)&&dl.expiresAt,'temporary download link');
 ok(await code('AppreciationLetter_GetMyList',{})==='forbidden'&&await code('AppreciationLetter_Acknowledge',{id:nl.id})==='forbidden','the executive cannot acknowledge or list “my letters”');
 await tab('all');ok(rows().length>=5&&vm().tools.length===1&&vm().flt.length===2,'all-letters tab: table, filters, Issue');
 vm().tools[0].go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===4&&rv().dlgv.fields[3].isFile,'issue dialog has a file field');C.dlgClose();
 C.dlgOpen(C.apNewDlg(T,[{v:U('22'),l:'Noor'}]));C.setF('e',U('22'));C.setF('t','شكر');C.setF('f',[{id:'a:1',fileName:'letter2.pdf',sizeBytes:1234}]);C.dlgGo();await sl(300);ok(C.d14.ls.some(l=>l.emp===U('22')&&l.file.fileName==='letter2.pdf'),'issuing through the dialog sends the chosen file');
 // ---------- HR
 ok(await login('hr',true),'HR signed in');await goto('apr');ok(tabs().join('|')==='كل الكتب|سجل موظف','HR tabs');
 ok((await C.call('AppreciationLetter_List',{})).items.length>=5&&await code('AppreciationLetter_Create',{employeeId:U('11'),title:'x',file:f0})==='forbidden','HR reads all letters but cannot issue (executive only)');
 await tab('all');ok(vm().tools.length===0,'no Issue button for HR');
 // ---------- department manager
 ok(await login('mgr',true),'department manager signed in');await goto('apr');ok(tabs().join('|')==='سجل موظف','manager tab: employee record only');
 ok((await C.call('AppreciationLetter_GetEmployeeFile',{employeeId:U('18')})).items.length>=1&&await code('AppreciationLetter_GetEmployeeFile',{employeeId:U('24')})==='Hr:Appreciation:NotFound','record of a team member; outside the team -> 404');
 const own=C.d14.ls.find(l=>l.emp===U('18'));ok((await C.call('AppreciationLetter_Get',{id:own.id})).title&&await code('AppreciationLetter_Get',{id:nl.id})==='Hr:Appreciation:NotFound'&&await code('AppreciationLetter_GetDownloadUrl',{id:nl.id})==='Hr:Appreciation:NotFound','manager opens team letters; others -> 404 (also for the download link)');
 ok(await code('AppreciationLetter_List',{})==='forbidden'&&await code('AppreciationLetter_Create',{employeeId:U('18'),title:'x',file:f0})==='forbidden','a department manager cannot propose or issue (no recommendation operation)');
 await tab('file');ok(vm().flt.length===2&&vm().blocks.length===1,'record tab asks for an employee');C.setFlt('ae',U('18'));await sl(250);ok(rows().length>=1,'record tab lists letters');
 // ---------- employee
 ok(await login('emp',true),'employee signed in');await goto('apr');ok(tabs().join('|')==='كتب الشكر لي','employee tab');
 const my=await C.call('AppreciationLetter_GetMyList',{});ok(my.items.length===1&&my.items[0].employee.id===U('11')&&my.items[0].isAcknowledged,'my letters only');
 ok(await code('AppreciationLetter_Get',{id:own.id})==='Hr:Appreciation:NotFound'&&await code('AppreciationLetter_Acknowledge',{id:own.id})==='Hr:Appreciation:NotFound'&&await code('AppreciationLetter_GetEmployeeFile',{employeeId:U('18')})==='forbidden','another employee’s letter -> 404');
 // acknowledge: use a new letter for Sara
 await login('xd',true);const sl2=await C.call('AppreciationLetter_Create',{employeeId:U('11'),title:'شكر جديد',file:f0});
 await login('emp',true);const a1=await C.call('AppreciationLetter_Acknowledge',{id:sl2.id});const a2=await C.call('AppreciationLetter_Acknowledge',{id:sl2.id});ok(a1.isAcknowledged&&a1.acknowledgedAt&&a2.acknowledgedAt===a1.acknowledgedAt,'acknowledge records the time once; repeating changes nothing');
 await goto('apr');await tab('my');ok(rows().length===2&&vm().blocks.length===2,'my letters UI');rows()[0].acts[0].go();await sl(200);ok(C.state.sub==='one'&&vm().tools.some(t=>t.l===T.aApDl),'letter details with download');
 // notification routing
 await login('mgr',true).catch(()=>0);await login('emp',true);C.d8.notes.push({id:'x1',user:U('11'),code:'appreciationLetterIssued',rt:'appreciationLetter',rid:sl2.id,at:new Date().toISOString(),read:null,cnt:null});await C.ntOpen({id:'x1',templateCode:'appreciationLetterIssued',resourceId:sl2.id,isRead:true});await sl(400);ok(C.state.page==='apr'&&C.state.sub==='one'&&C.state.sid===sl2.id,'the notification opens the letter');
 for(const p of ['adm','so','rv']){await login(p,true);ok(await code('AppreciationLetter_List',{})==='forbidden'&&await code('AppreciationLetter_GetMyList',{})==='forbidden'&&!nav().includes('كتب الشكر'),p+': no access')}
 for(const pr of ['xd','hr','mgr','emp']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const t of ['my','all','file']){C.setState({page:'apr',tab:t,sub:'list',sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render apr/'+t+' '+pr+' '+L)}catch(e){ok(false,'render apr/'+t+' '+pr+' '+L+' '+e.message)}}}}
 console.log(out.join('\n'));
})();
