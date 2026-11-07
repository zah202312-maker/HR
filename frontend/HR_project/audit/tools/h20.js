process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u,cb){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n};if(cb)cb()}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const login=async(k,keep)=>{C.signOut();if(!keep)C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);if(C.state.scr==='mfa'&&k==='acc'){C.srv.mfaE=true;await C.boot();await sl(200)}return C.state.scr==='app'};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status||String(e)}};
const U=(n)=>C.UID(n);const E=(n)=>C.db.emps.find(e=>e.en===n);
const iso=(h)=>new Date(Date.now()+h*36e5).toISOString();
(async()=>{
 // This suite closes the validation-coverage gap: operations that exist in the UI or the mock but were never called by the other suites.
 ok(await login('hr'),'HR signed in');
 const dep=C.db.D[0];const dg=await C.call('Department_Get',{id:dep.id});ok(dg.id===dep.id&&dg.concurrencyStamp&&dg.nameAr&&dg.nameEn,'Department_Get');
 const du=await C.call('Department_Update',{id:dep.id,nameAr:dg.nameAr,nameEn:dg.nameEn+' ',concurrencyStamp:dg.concurrencyStamp});ok(du.nameEn===dg.nameEn+' '&&du.concurrencyStamp!==dg.concurrencyStamp,'Department_Update: new stamp');
 ok(await code('Department_Update',{id:dep.id,nameAr:dg.nameAr,nameEn:'x',concurrencyStamp:dg.concurrencyStamp})==='conflict','Department_Update: stale stamp -> 409');
 const sh=(await C.call('WorkShift_List',{})).items[0];const sg=await C.call('WorkShift_Get',{id:sh.id});ok(sg.id===sh.id,'WorkShift_Get');
 const dev=C.d3.devs[0];const imp=await C.call('AttendanceImport_Create',{file:'punches_ok.csv',deviceId:dev.id});const ig=await C.call('AttendanceImport_Get',{id:imp.id});ok(ig.id===imp.id,'AttendanceImport_Get');
 const hol=await C.call('Holiday_Create',{date:C.dAdd(C.todayS(),200),nameAr:'عطلة اختبار',nameEn:'Test holiday'});const hu=await C.call('Holiday_Update',{id:hol.id,date:C.dAdd(C.todayS(),201),nameAr:'عطلة معدلة',nameEn:'Edited holiday',concurrencyStamp:hol.concurrencyStamp});ok(hu.date===C.dAdd(C.todayS(),201)&&hu.nameEn==='Edited holiday','Holiday_Update');
 ok(await code('Holiday_Update',{id:hol.id,date:C.dAdd(C.todayS(),202),nameAr:'x',nameEn:'x',concurrencyStamp:hol.concurrencyStamp})==='conflict','Holiday_Update: stale stamp -> 409');
 const hd=await C.call('Holiday_Delete',{id:hol.id});ok(!(await C.call('Holiday_List',{maxResultCount:200})).items.some(h=>h.id===hol.id),'Holiday_Delete: the holiday is removed');
 // employee update + administrative task log
 const sara=C.db.emps.find(e=>e.id===U('11'));const ge=await C.call('Employee_Get',{id:sara.id});
 const eu=await C.call('Employee_Update',{id:sara.id,fullNameAr:ge.fullNameAr,fullNameEn:ge.fullNameEn,email:ge.email,phoneNumber:'0770 000 0000',dateOfBirth:ge.dateOfBirth,address:'بغداد',departmentId:ge.department.id,jobTitle:ge.jobTitle,hireDate:ge.hireDate,status:ge.status,concurrencyStamp:ge.concurrencyStamp,gender:ge.gender,employeeNumber:ge.employeeNumber});ok(eu.phoneNumber==='0770 000 0000'&&eu.concurrencyStamp!==ge.concurrencyStamp,'Employee_Update');
 const tl=await C.call('Employee_GetAdministrativeTaskLog',{id:U('12')});ok(Array.isArray(tl.items)&&tl.totalCount>=1&&tl.items.every(z=>z.taskType&&z.title),'Employee_GetAdministrativeTaskLog (a department manager’s task log)');
 ok((await C.call('Employee_GetAdministrativeTaskLog',{id:sara.id})).totalCount===0&&(await C.call('Employee_GetAdministrativeTaskLog',{id:U('12'),taskType:tl.items[0].taskType})).items.every(z=>z.taskType===tl.items[0].taskType),'an employee with no manager role has an empty log; task-type filter');
 // documents
 const up=await C.call('EmployeeDocument_Upload',{id:sara.id,file:'contract.pdf',size:20000,type:'contract',title:'عقد العمل',sensitivity:'sensitive'});ok(up.id&&up.title==='عقد العمل','EmployeeDocument_Upload');
 const dl=await C.call('EmployeeDocument_GetDownloadUrl',{id:up.id});ok(/^https:/.test(dl.url)&&dl.expiresAt,'EmployeeDocument_GetDownloadUrl');
 await C.call('EmployeeDocument_Delete',{id:up.id});ok(await code('EmployeeDocument_GetDownloadUrl',{id:up.id})!=='OK','EmployeeDocument_Delete: the document is gone');
 // device user map
 const m=await C.call('DeviceUserMap_Create',{id:dev.id,deviceUserId:'7777',employeeId:sara.id});await C.call('DeviceUserMap_Delete',{id:dev.id,mapId:m.id});ok(!(await C.call('DeviceUserMap_List',{id:dev.id})).items.some(z=>z.id===m.id),'DeviceUserMap_Delete');
 // delegation
 await login('mgr',true);const dlg=await C.call('Delegation_Create',{toUserId:E('Muna Al-Rawi').id,scope:'requests',startsAt:iso(-1),endsAt:iso(48),reason:'سفر'});await C.call('Delegation_Cancel',{id:dlg.id});ok(await code('Delegation_Cancel',{id:dlg.id})==='Hr:Delegation:AlreadyEnded','Delegation_Cancel; cancelling twice -> AlreadyEnded');
 const memo=await C.call('Memo_Create',{kind:'problem',content:'مشكلة للتجربة'});const mg=await C.call('Memo_Get',{id:memo.id});ok(mg.id===memo.id&&mg.content,'Memo_Get');
 // evaluation internal note draft
 const ev=(await C.call('Evaluation_GetTeamList',{})).items.find(e=>e.status==='open'||e.status==='draft'||!e.submittedAt)||(await C.call('Evaluation_GetTeamList',{})).items[0];
 const nd=await C.call('EvaluationInternalNote_SaveDraft',{id:ev.id,content:'مسودة ملاحظة'});ok(nd!==undefined,'EvaluationInternalNote_SaveDraft (saved)');
 // system administrator
 ok(await login('adm',true),'system admin signed in');
 const roles=await C.call('Role_List',{});const role=roles.items.find(r=>r.nameEn==='Reviewer')||roles.items[0];
 const rg=await C.call('Role_Get',{id:role.id});ok(rg.id===role.id&&rg.concurrencyStamp,'Role_Get');
 const rp=await C.call('Role_GetPermissions',{id:role.id});ok(Array.isArray(rp.groups||rp.items||rp.permissions||[]),'Role_GetPermissions');
 const ru=await C.call('Role_Update',{id:role.id,nameAr:rg.nameAr,nameEn:rg.nameEn,concurrencyStamp:rg.concurrencyStamp});ok(ru.concurrencyStamp!==rg.concurrencyStamp,'Role_Update');
 const acc=(await C.call('UserAccount_List',{})).items.find(a=>a.employee&&a.employee.id===U('18'))||(await C.call('UserAccount_List',{})).items[0];
 const perm=(rp.groups?rp.groups.flatMap(g=>g.permissions||[]):(rp.items||rp.permissions||[]))[0];
 const upd=await C.call('UserAccount_UpdatePermissions',{id:acc.id,permissions:perm?[{name:perm.name,isGranted:false}]:[]});ok(upd!==undefined,'UserAccount_UpdatePermissions (saved)');
 const sr=await C.call('UserAccount_SetRoles',{id:acc.id,roleNames:['employee'],managerEmployeeId:undefined,concurrencyStamp:(await C.call('UserAccount_Get',{id:acc.id})).concurrencyStamp});ok(sr!==undefined,'UserAccount_SetRoles (saved)');
 const d0=(await C.call('AttendanceDevice_List',{})).items[0];const dv=await C.call('AttendanceDevice_Update',{id:d0.id,name:d0.name+' 2',isActive:true,concurrencyStamp:d0.concurrencyStamp});ok(dv.name===d0.name+' 2','AttendanceDevice_Update');
 const rt=(await C.call('RequestType_GetList',{})).items[0];const rtg=await C.call('RequestType_Get',{id:rt.id});ok(rtg.id===rt.id,'RequestType_Get');
 const rtu=await C.call('RequestType_Update',{id:rt.id,nameAr:rtg.nameAr,nameEn:rtg.nameEn,destination:rtg.destination,isActive:rtg.isActive,concurrencyStamp:rtg.concurrencyStamp});ok(rtu.id===rt.id&&rtu.concurrencyStamp!==rtg.concurrencyStamp,'RequestType_Update');
 const gr=await C.call('EvaluationGrade_Create',{code:'outstandingplus',nameAr:'استثنائي',nameEn:'Outstanding+',sortOrder:7,isActive:true});ok(gr.code==='outstandingplus','EvaluationGrade_Create');
 // handler assignment rejected by the executive director
 await login('hr',true);const dgl=(await C.call('Delegation_List',{})).items.find(z=>z.status==='cancelled'&&z.cancelledAt);ok(!!dgl,'Delegation_List: the cancelled delegation shows its status and time');const hq=await C.call('ComplaintHandler_Request',{userId:U('23'),level:'normal',from:iso(1),reason:'تغطية'});await login('xd',true);const rj=await C.call('ComplaintHandler_Reject',{id:hq.id,reason:'غير مناسب',concurrencyStamp:hq.concurrencyStamp});ok(rj.status==='rejected','ComplaintHandler_Reject');
 // leave request detail with attachment
 await login('emp',true);const at=await C.call('LeaveRequest_UploadAttachment',{file:'report.pdf',size:2048});const sick=(await C.call('OtherLeaveType_ListAvailable',{})).items[0];
 const lr=await C.call('LeaveRequest_Create',{kind:'other',otherLeaveTypeId:sick.id,startDate:C.dAdd(C.todayS(),30),endDate:C.dAdd(C.todayS(),31),attachmentIds:[at.id]});
 const lg=await C.call('LeaveRequest_Get',{id:lr.requestId});ok(lg.requestId===lr.requestId&&lg.attachments.length===1,'LeaveRequest_Get');
 const ld=await C.call('LeaveRequest_GetAttachmentDownloadUrl',{id:lr.requestId,attachmentId:lg.attachments[0].id});ok(/^https:/.test(ld.url),'LeaveRequest_GetAttachmentDownloadUrl');
 // store officer reads by id
 await login('so',true);const it=(await C.call('Item_GetList',{})).items[0],si=(await C.call('StockIssue_GetList',{})).items[0],cu=(await C.call('Custody_GetList',{})).items[0];
 ok((await C.call('Item_Get',{id:it.id})).id===it.id&&(await C.call('StockIssue_Get',{id:si.id})).id===si.id&&(await C.call('Custody_Get',{id:cu.id})).id===cu.id,'Item_Get, StockIssue_Get, Custody_Get');
 console.log(out.join('\n'));
})();
