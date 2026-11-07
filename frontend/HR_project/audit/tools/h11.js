process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u,cb){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n};if(cb)cb()}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm,rows=()=>{const b=vm().blocks.find(z=>z.hasTbl);return b?b.rows:[]},nav=()=>rv().nav.map(n=>n.label);
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const E=(n)=>C.db.emps.find(e=>e.en===n);
const unread=async()=>(await C.call('Notification_GetUnreadCount',{})).count;
const list=async(x)=>(await C.call('Notification_List',{maxResultCount:100,...(x||{})})).items;
(async()=>{
 // ---------- employee: reading and marking
 ok(await login('emp'),'employee signed in');
 const l0=await list();ok(l0.length>=2&&l0.every((n,i,a)=>i===0||a[i-1].creationTime>=n.creationTime),'list is newest first ('+l0.length+')');
 ok(l0.every(n=>n.title&&n.body&&n.templateCode&&/^\d{4}-/.test(n.creationTime)),'each notification has a server-built title and body');
 const c0=await unread();ok(c0===l0.filter(n=>!n.isRead).length&&c0>=1,'unread count matches the list ('+c0+')');
 ok((await list({isRead:true})).every(n=>n.isRead)&&(await list({isRead:false})).every(n=>!n.isRead),'isRead filter');
 const first=l0.find(n=>!n.isRead);const m1=await C.call('Notification_MarkRead',{id:first.id});ok(m1.isRead&&m1.readAt,'MarkRead sets readAt');
 const m2=await C.call('Notification_MarkRead',{id:first.id});ok(m2.readAt===m1.readAt,'MarkRead is idempotent (readAt unchanged)');
 ok(await unread()===c0-1,'unread count drops by one');
 ok(await code('Notification_MarkRead',{id:C.d8.notes.find(n=>n.user!==E('Sara Al-Hasani').id).id})==='Hr:Notification:NotFound','another user’s notification -> 404');
 // language: titles are built per Accept-Language
 C.setState({lang:'en'});const le=await list();ok(le.some(n=>/update|request|balance|manager/i.test(n.title)),'English titles when the UI language is English');C.setState({lang:'ar'});
 // ---------- event chain: request -> manager -> HR
 const hass=E('Hassan Ali'),muna=E('Muna Al-Rawi'),sara=E('Sara Al-Hasani');
 const types=await C.call('RequestType_GetList',{});const adm=types.items.find(t=>t.code==='administrativeNeed');
 const rq=await C.call('Request_Submit',{requestTypeId:adm.id,description:'طلب للتجربة'});
 const notesOf=(uid,code)=>C.d8.notes.filter(n=>n.user===uid&&(!code||n.code===code));
 ok(notesOf(hass.id,'requestPendingAction').some(n=>n.rid===rq.id),'submitting a request notifies the manager (requestPendingAction + resourceId)');
 await login('mgr',true);const mu=await unread();ok(mu>=3,'manager unread count reflects it ('+mu+')');
 await C.call('Request_Approve',{id:rq.id});ok(notesOf(sara.id,'requestStatusChanged').some(n=>n.rid===rq.id)&&notesOf(muna.id,'requestPendingAction').some(n=>n.rid===rq.id),'approval notifies the employee and the next approver');
 ok(!notesOf(hass.id,'requestStatusChanged').some(n=>n.rid===rq.id),'the actor is not notified of his own action');
 // memos/incidents/delegation/manager change/balance/password
 await login('emp',true);const memo=await C.call('Memo_Create',{kind:'problem',content:'x'});ok(notesOf(hass.id,'memoUpdate').some(n=>n.rid===memo.id),'memo notifies its handler');
 await login('hr',true);await C.call('LeaveBalance_Adjust',{id:sara.id,days:1,reason:'x'});ok(notesOf(sara.id,'leaveBalanceAdjusted').length>=2,'balance adjustment notifies the employee');
 await C.call('Delegation_Create',{toUserId:E('Nasser Al-Kaabi').id,scope:'requests',startsAt:new Date(Date.now()-36e5).toISOString(),endsAt:new Date(Date.now()+864e5).toISOString(),reason:'x'});ok(notesOf(E('Nasser Al-Kaabi').id,'delegationUpdated').length>=1,'delegation notifies the delegate');
 await C.call('UserAccount_ResetPassword',{id:sara.id}).catch(()=>0);
 // ---------- complaints: generic text, never the respondent
 await login('emp',true);const sub='سر الموضوع الفريد-9f3';const cs=await C.call('Complaint_Submit',{targetKind:'employee',respondentId:hass.id,level:'confidential',subject:sub,content:'محتوى سري'});
 ok(notesOf(muna.id,'secureItemUpdateForHandler').some(n=>n.rid===cs.id),'complaint notifies the new handler only (HR, because the respondent is a department manager)');
 ok(!C.d8.notes.some(n=>n.user===hass.id&&/secure/.test(n.code)),'the respondent receives no notification at all');
 await login('hr',true);const mh=await list();const sec=mh.filter(n=>/^secure/.test(n.templateCode));ok(sec.length>=1&&sec.every(n=>n.title===T_title()&&n.resourceType==='secureItem'),'secure-item notifications use the generic text and resource type secureItem');
 function T_title(){return 'لديك تحديث جديد'}
 const hd=await C.call('Complaint_GetDetail',{id:cs.id});await C.call('Complaint_Close',{id:cs.id,decision:'تم',concurrencyStamp:hd.concurrencyStamp});
 ok(notesOf(sara.id,'secureItemUpdateForOwner').some(n=>n.rid===cs.id),'closing notifies the complainant (secureItemUpdateForOwner)');
 ok(JSON.stringify(C.d8.notes).indexOf(sub)===-1&&JSON.stringify(C.d8.notes).indexOf('محتوى سري')===-1&&JSON.stringify(C.d8.notes).indexOf(cs.referenceNumber)===-1,'no notification contains a name, subject, content or reference number');
 const all=JSON.stringify(await (async()=>{await login('emp',true);return list()})());ok(!/Hassan|حسن علي|CMP-/.test(all),'listed notifications never name anyone');
 // ---------- device tokens
 await login('emp',true);const tk=C._tok;ok(tk&&C.d8.tok[tk]&&C.d8.tok[tk].user===sara.id,'sign-in registers this device for the signed-in user');
 await login('mgr',true);ok(C.d8.tok[tk]&&C.d8.tok[tk].user===hass.id,'registering the same token for another user moves it (no notifications to the wrong person)');
 ok(await C.call('DeviceToken_Register',{token:'abc',platform:'android'})===null&&C.d8.tok.abc.user===hass.id,'DeviceToken_Register returns no content and is idempotent');
 ok(await code('DeviceToken_Register',{token:'',platform:'web'})==='validation'&&await code('DeviceToken_Register',{token:'x',platform:'desktop'})==='validation','token and platform are validated');
 await login('emp',true);await C.call('DeviceToken_Unregister',{token:'abc'});ok(C.d8.tok.abc&&C.d8.tok.abc.user===hass.id,'unregister by a non-owner changes nothing');
 const tk2=C._tok;await rv().signOutUI();ok(!C.d8.tok[tk2]&&C.state.scr!=='app','the sign-out button unregisters the device token first, then signs out');await login('emp',true);const tk3=C._tok;C.signOut();ok(!C.d8.tok[tk3],'programmatic sign-out also removes the token');
 // ---------- bell and notifications page (UI)
 await login('mgr',true);await sl(80);const cnt=await unread();ok(rv().ntCount===String(cnt)&&cnt>0,'bell badge shows the real unread count ('+cnt+')');
 rv().toggleBell();await sl(120);ok(rv().ntItems.length>0&&rv().ntItems.length<=6&&rv().ntNone===false,'dropdown lists the latest notifications');
 const nb=await unread();rv().ntItems[0].go();await sl(300);ok(await unread()<=nb,'opening a notification marks it read');
 ok(['req','leave','me','cmp','dash'].includes(C.state.page),'it navigates to the related screen ('+C.state.page+')');
 await goto('notif');ok(rows().length>=3&&vm().tools.length===1&&vm().flt.length===1,'notifications page: list, filter, mark-all');
 vm().tools[0].go();await sl(300);ok(await unread()===0,'mark-all-as-read clears the unread count');
 // owner vs handler routing for secure items
 await login('emp',true);const sc2=await C.call('Complaint_Submit',{targetKind:'general',level:'normal',subject:'x',content:'y'});
 await login('mgr',true);await C.call('Complaint_GetAssignedList',{});const hn=(await list()).find(n=>n.templateCode==='secureItemUpdateForHandler'&&n.resourceId===sc2.id);ok(!!hn,'handler received the secure-item notification');
 await C.ntOpen(hn);await sl(300);ok(C.state.page==='cmp'&&C.state.sub==='handle'&&C.state.sid===sc2.id,'handler: the my-detail route returns 404, so the app falls back to the handler route');
 await login('emp',true);await C.call('Complaint_RaiseLevel',{id:sc2.id,level:'confidential',concurrencyStamp:(await C.call('Complaint_GetMyDetail',{id:sc2.id})).concurrencyStamp});
 // ---------- settings hub + reminders
 ok(await login('hr',true),'HR signed in');
 const sections=await C.call('SystemSettings_ListSections',{});ok(sections.items.length===15&&sections.items.filter(z=>z.isEditableByCaller).map(z=>z.code).sort().join()==='departmentsAndManagerLinks,employeeLeaveBalances,holidays,workShifts','15 sections; HR edits only its four');
 ok(await code('SystemSettings_GetReminders',{})==='forbidden','HR cannot read reminders');
 await goto('set');ok(vm().blocks.length===15,'hub lists all 15 sections from the API');
 const hubOpen=vm().blocks.filter(b=>b.hasActs).length;ok(hubOpen>=4&&hubOpen<15,'hub shows Open only where the caller can go ('+hubOpen+')');
 ok(await login('adm',true),'system admin signed in');await goto('set');ok(vm().blocks.length===15&&vm().blocks.filter(b=>b.hasActs).length>=9,'admin hub: more sections are openable');
 const r0=await C.call('SystemSettings_GetReminders',{});ok(r0.monthEndOpenTime==='10:00'&&r0.monthlySubmissionWorkingDays===3&&r0.monthlyEscalationWorkingDay===4&&r0.approvalReminderIntervalWorkingDays===null&&r0.evaluationMinWorkedDays===null,'defaults from requirement 5.10; the last two are null until set');
 C.nav2('rem');await sl(200);ok(vm().blocks.length===2&&vm().blocks[1].hasForm&&vm().blocks[1].fields.length===6,'reminders screen: values + form (6 fields)');
 const f=vm().blocks[1].fields;ok(f[4].value==='2'&&f[5].value==='15','approved decisions (2 and 15) are suggested for the empty fields');
 const base={monthEndOpenTime:'10:00',monthlySubmissionWorkingDays:3,monthlyReminderIntervalWorkingDays:1,monthlyEscalationWorkingDay:3,approvalReminderIntervalWorkingDays:2,evaluationMinWorkedDays:15,concurrencyStamp:r0.concurrencyStamp};
 ok(await code('SystemSettings_UpdateReminders',base)==='Hr:Settings:InvalidEscalationDay','escalation day must exceed the submission deadline');
 ok(await code('SystemSettings_UpdateReminders',{...base,monthlyEscalationWorkingDay:1})==='validation'&&await code('SystemSettings_UpdateReminders',{...base,evaluationMinWorkedDays:null,monthlyEscalationWorkingDay:5})==='validation','minimums and required fields -> 400');
 ok(await code('SystemSettings_UpdateReminders',{...base,monthlyEscalationWorkingDay:5,concurrencyStamp:'stale'})==='conflict','stale stamp -> 409');
 const up=await C.call('SystemSettings_UpdateReminders',{...base,monthlyEscalationWorkingDay:5});ok(up.monthlyEscalationWorkingDay===5&&up.approvalReminderIntervalWorkingDays===2&&up.evaluationMinWorkedDays===15&&up.lastModificationTime,'saved; the two previously empty values are now set');
 // UI save path
 await goto('set');C.nav2('rem');await sl(200);C.setF('e','6');await vm().blocks[1].acts.find(a=>a.cls==='p').go();await sl(250);ok(C.d8.rem.esc===6,'form save updates the schedule');
 // ---------- audit log
 const au=await C.call('AuditLog_List',{maxResultCount:200});ok(au.items.length>=10&&au.items.every((e,i,a)=>i===0||a[i-1].occurredAt>=e.occurredAt),'audit log newest first ('+au.totalCount+')');
 ok(au.items.every(e=>Array.isArray(e.changedFields)&&e.changedFields.every(x=>typeof x==='string'&&!/\d{2}:\d{2}$/.test(x))),'changed fields are names only (no values)');
 ok(au.items.some(e=>e.action==='settingChanged'&&e.entityType==='SystemSettings'&&e.changedFields.includes('monthEndOpenTime')),'a settings change is recorded with field names');
 ok(au.items.some(e=>e.action==='login')&&au.items.some(e=>e.action==='loginFailed'&&e.succeeded===false),'sign-ins and failed sign-ins are recorded');
 ok(au.items.some(e=>e.action==='settingChanged'&&e.succeeded===false),'a rejected change is recorded as failed');
 ok(au.items.some(e=>e.action==='roleChanged')&&au.items.some(e=>e.action==='passwordReset'),'role changes and password resets appear');
 ok(!au.items.some(e=>/complaint/i.test(e.entityType||'')&&e.action!=='complaintHandlerAssignment')&&!JSON.stringify(au.items).includes('CMP-'),'no complaint entities in the audit log');
 ok(au.items.every(e=>e.action!=='read'),'no read entries (financial reads only; no finance yet)');
 ok((await C.call('AuditLog_List',{action:'login'})).items.every(e=>e.action==='login')&&(await C.call('AuditLog_List',{succeeded:false})).items.every(e=>!e.succeeded),'action and succeeded filters');
 const hrId=muna.id;ok((await C.call('AuditLog_List',{userId:hrId})).items.every(e=>e.user&&e.user.id===hrId),'user filter');
 ok((await C.call('AuditLog_List',{entityType:'systemsettings'})).items.length>=1,'entity type filter (case-insensitive)');
 ok((await C.call('AuditLog_List',{from:new Date(Date.now()-60000).toISOString()})).items.length>=1&&(await C.call('AuditLog_List',{to:new Date(Date.now()-864e6).toISOString()})).items.length===0,'period filters');
 await login('hr',true);await C.call('ComplaintHandler_Request',{userId:E('Omar Khalid').id,level:'normal',from:new Date(Date.now()+36e5).toISOString(),reason:'تغطية'});await C.call('Report_Export',{code:'hrLeaveBalances',format:'pdf'}).catch(()=>0);
 await login('adm',true);const au2=await C.call('AuditLog_List',{maxResultCount:200});const ch=au2.items.find(e=>e.action==='complaintHandlerAssignment');ok(ch&&ch.entityType==='ComplaintHandlerAssignment'&&!JSON.stringify(ch).includes('تغطية'),'handler assignments appear without complaint data or reasons');
 ok(au2.items.some(e=>e.action==='export'&&e.entityId==='hrLeaveBalances'),'exports are recorded');
 await goto('aud');ok(rows().length>=10&&vm().flt.length===6&&nav().includes('سجل التدقيق'),'audit page: table, 6 filters, nav item');
 ok(await login('xd',true)&&(await C.call('AuditLog_List',{})).items.length>=10,'executive can read the audit log');
 for(const p of ['emp','mgr','hr','rv','so']){await login(p,true);ok(await code('AuditLog_List',{})==='forbidden',p+': audit log forbidden');if(p!=='emp')ok(!nav().includes('سجل التدقيق'),p+': audit nav hidden')}
 // ---------- render every page in both languages
 for(const pr of ['emp','mgr','hr','adm','xd']){await login(pr,true);for(const L of ['ar','en']){C.setState({lang:L});for(const pg of ['notif','aud','set']){if(pg==='aud'&&!['adm','xd'].includes(pr))continue;if(pg==='set'&&!['hr','adm'].includes(pr))continue;const subs=pg==='set'?['hub','rem']:['list'];for(const sb of subs){C.setState({page:pg,tab:'data',sub:sb,sid:null,data:{},flt:{}});await C.enter();await sl(10);try{rv();ok(true,'render '+pg+'/'+sb+' '+pr+' '+L)}catch(e){ok(false,'render '+pg+'/'+sb+' '+pr+' '+L+' '+e.message)}}}}}
 console.log(out.join('\n'));
})();
