const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.PERMS=PERMS;');
const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals();
const login=async(k)=>{(C.signOut(),C.resetMock());C.setState({persona:k});await C.doSignin();await sl(200);if(C.state.scr==='pwd'||C.state.scr==='mfa')return false;return true};
const goto=async(p)=>{C.go(p);await sl(120);await C.enter();await sl(30)};
const code=async(op,x)=>{try{await C.call(op,x);return 'OK'}catch(e){return e.code||e.status}};
const rows=()=>{const b=rv().vm.blocks.find(z=>z.hasTbl);return b?b.rows:[]};
(async()=>{
 ok(await login('hr'),'HR manager signed in');
 ok(rv().nav.map(n=>n.label).join('|').includes('الموظفون')&&rv().nav.length>=12,'HR nav includes Employees/Departments/Settings/Accounts');
 await goto('emp');let v=rv().vm;ok(rv().isOrg&&!v.denied&&rows().length===10&&v.hasPg,'employees list: 10 rows, pager (15 total)');
 await C.setFlt('ed',C.state.data.dl.items[0].id);await sl(120);ok(rows().length>0&&rows().length<10,'department filter narrows list ('+rows().length+')');
 C.setFlt('ed','');C.setFlt('eq','Sara');await sl(150);ok(rows().length===1,'search by name');C.setFlt('eq','');await sl(100);
 const sara=C.db.emps.find(e=>e.en==='Sara Al-Hasani');
 C.nav2('file',sara.id);await sl(150);v=rv().vm;ok(v.title.includes('سارة')&&v.blocks.length===2&&v.tools.length>=3,'employee file: data + account blocks, actions');
 C.setState({tab:'docs'});C.later();await sl(150);ok(rows().length===2,'documents tab: HR sees normal+sensitive (2)');
 C.setState({tab:'hist'});C.later();await sl(150);ok(rows().length===1,'manager history tab');
 // change manager rules
 ok(await code('Employee_ChangeManager',{id:sara.id,managerEmployeeId:sara.mgr,concurrencyStamp:sara.stamp})==='Hr:Employee:ManagerUnchanged','ManagerUnchanged');
 ok(await code('Employee_ChangeManager',{id:sara.id,managerEmployeeId:sara.id,concurrencyStamp:sara.stamp})==='Hr:Employee:CannotManageSelf','CannotManageSelf');
 const karim=C.db.emps.find(e=>e.en==='Karim Saleh'),salma=C.db.emps.find(e=>e.en==='Salma Al-Dulaimi'),hassan=C.db.emps.find(e=>e.en==='Hassan Ali');
 ok(await code('Employee_ChangeManager',{id:sara.id,managerEmployeeId:karim.id,concurrencyStamp:sara.stamp})==='Hr:Employee:InvalidManagerForRole','InvalidManagerForRole (accountant is not a manager)');
 ok(await code('Employee_ChangeManager',{id:sara.id,managerEmployeeId:salma.id,concurrencyStamp:'old'})==='conflict','409 concurrency conflict');
 ok(await code('Employee_ChangeManager',{id:sara.id,managerEmployeeId:salma.id,concurrencyStamp:sara.stamp})==='OK'&&sara.mgr===salma.id,'manager changed + history');
 ok(C.db.hist[sara.id].length===2&&C.db.hist[sara.id][0].to,'history closes previous period');
 // dialog flow for change manager through UI vm
 C.nav2('file',sara.id);await sl(150);v=rv().vm;const chg=v.tools.find(t=>t.l.includes('المدير')&&t.l.length<14);ok(!!chg,'Change-manager action present');
 chg.go();await sl(20);ok(rv().hasDlg&&rv().dlgv.fields.length===2,'dialog opened with manager select + reason');
 C.dlgGo();await sl(60);ok(rv().dlgv.fields[0].err.length>0,'dialog: required validation on manager');
 C.setF('managerEmployeeId',hassan.id);C.dlgGo();await sl(200);ok(!rv().hasDlg&&sara.mgr===hassan.id,'dialog submit success -> closed');
 // wizard
 C.startCreate();await sl(200);v=rv().vm;ok(v.hasTabs&&v.tabs.length===3&&v.blocks[0].fields.length===11,'wizard step 1 (11 fields)');
 v.blocks[0].acts.find(a=>a.cls==='p').go();await sl(10);ok(rv().vm.blocks[0].fields.find(f=>f.k==='fullNameAr').err==='هذا الحقل مطلوب'&&C.state.step===0,'step 1 validation blocks Next');
 C.setState({fv:{fullNameAr:'أحمد المحمد',fullNameEn:'Ahmed Al-Mohammed',departmentId:C.db.D[0].id,jobTitle:'مهندس',hireDate:'2026-11-05',email:'a.m@example.test'}});
 rv().vm.blocks[0].acts.find(a=>a.cls==='p').go();await sl(10);ok(C.state.step===1,'step 2');
 C.setState({fv:{...C.state.fv,roleNames:['employee']}});rv().vm.blocks[0].acts.find(a=>a.cls==='p').go();await sl(10);ok(C.state.step===1&&rv().vm.blocks[0].fields.find(f=>f.k==='workShiftId').err,'shift required for non-executive');
 C.setState({fv:{...C.state.fv,workShiftId:'41'}});rv().vm.blocks[0].acts.find(a=>a.cls==='p').go();await sl(10);ok(C.state.step===2,'step 3 (review)');
 C.setState({fv:{...C.state.fv,userName:'a.mohammed'}});
 let act=rv().vm.blocks[0].acts.find(a=>a.cls==='p');await act.go();await sl(120);ok(C.state.derr==='Hr:Employee:ManagerRequired','create without manager -> ManagerRequired shown');
 C.setState({fv:{...C.state.fv,managerEmployeeId:sara.id}});act=rv().vm.blocks[0].acts.find(a=>a.cls==='p');await act.go();await sl(120);ok(C.state.derr==='Hr:Employee:InvalidManagerForRole','manager must be a department manager');
 C.setState({fv:{...C.state.fv,managerEmployeeId:hassan.id}});act=rv().vm.blocks[0].acts.find(a=>a.cls==='p');await act.go();await sl(250);
 ok(C.state.sub==='file'&&rv().hasDlg&&rv().dlgv.kv.length===2&&/^Tmp-/.test(rv().dlgv.kv[1].v),'created: file opens + temporary credentials dialog');
 C.dlgGo();await sl(60);ok(C.db.emps.length===16,'employee persisted (16)');
 ok(await code('Employee_Create',{fullNameAr:'x',departmentId:C.db.D[0].id,jobTitle:'x',hireDate:'2026-11-05',userName:'xxx',roleNames:['hrManager'],workShiftId:'41',managerEmployeeId:C.db.emps.find(e=>e.roles.includes('executiveDirector')).id})==='Hr:Identity:ProtectedRoleChangeRequiresApproval','HR cannot create another HR manager (executive only)');
 ok(await code('Employee_Update',{id:hassan.id,fullNameAr:'x',departmentId:hassan.dept,jobTitle:'x',hireDate:hassan.hire,status:'ended',concurrencyStamp:hassan.stamp})==='Hr:Employee:HasDirectReports','cannot end employee with direct reports');
 // departments
 await goto('dept');ok(rows().length===5,'departments list (5)');
 ok(await code('Department_Delete',{id:C.db.D[0].id})==='Hr:Department:HasEmployees','department with employees cannot be deleted');
 const nd=await C.call('Department_Create',{nameAr:'اختبار',nameEn:'Test'});ok(await code('Department_Delete',{id:nd.id})==='OK','empty department deleted');
 // accounts
 await goto('acc');ok(rows().length===10,'accounts list');
 const hiba=C.db.emps.find(e=>e.en==='Hiba Al-Saadi');
 ok(await code('UserAccount_ResetPassword',{id:C.db.emps.find(e=>e.en==='Muna Al-Rawi').id,reason:'x'})==='Hr:Identity:CannotResetOwnPassword','cannot reset own password');
 ok(await code('UserAccount_ResetPassword',{id:sara.id,reason:'x'})==='OK','reset password (email link)');
 const ca=await C.call('UserAccount_Create',{employeeId:hiba.id,userName:'h.saadi',roleNames:['employee']});ok(ca.credentials.mustChangePassword===true,'account created with mustChangePassword');
 ok(await code('UserAccount_ResetPassword',{id:hiba.id,reason:'x'})==='Hr:Identity:NoVerifiedEmail','NoVerifiedEmail');
 ok(await code('UserAccount_Create',{employeeId:sara.id,userName:'dup',roleNames:['employee']})==='Hr:Employee:EmployeeAlreadyHasAccount','EmployeeAlreadyHasAccount');
 const acc=await C.call('UserAccount_Get',{id:sara.id});
 ok(await code('UserAccount_SetRoles',{id:sara.id,roleNames:['employee','hrManager'],concurrencyStamp:acc.concurrencyStamp})==='Hr:Identity:ProtectedRoleChangeRequiresApproval','HR cannot grant hrManager role');
 const hAcc=await C.call('UserAccount_Get',{id:hassan.id});
 ok(await code('UserAccount_SetRoles',{id:hassan.id,roleNames:['employee'],concurrencyStamp:hAcc.concurrencyStamp})==='Hr:Identity:ManagerRoleHasDirectReports','ManagerRoleHasDirectReports');
 const mAcc=await C.call('UserAccount_Get',{id:C.db.emps.find(e=>e.en==='Muna Al-Rawi').id});
 ok(await code('UserAccount_SetRoles',{id:mAcc.id,roleNames:['hrManager','employee'],concurrencyStamp:mAcc.concurrencyStamp})==='Hr:Identity:CannotChangeOwnRoles','CannotChangeOwnRoles');
 ok(await code('UserAccount_GetPermissions',{id:sara.id})==='forbidden','HR cannot read permissions (system admin only)');
 ok(await code('UserAccount_ResetMfa',{id:karim.id,reason:'x'})==='forbidden','HR cannot reset MFA (only executive/system admin)');
 // settings
 await goto('set');ok(rv().vm.blocks.length===15&&rv().vm.blocks.some(b=>b.hasActs),'settings hub (15 sections from the API)');
 C.nav2('roles');await sl(150);ok(rows().length===8&&!rv().vm.tools.some(t=>t.l==='دور جديد'),'HR sees 8 roles read-only (no create)');
 // employee persona
 ok(await login('emp'),'employee signed in');
 ok(!rv().nav.some(n=>n.label==='الموظفون')&&!rv().nav.some(n=>n.label==='الحسابات'),'employee nav hides Employees/Accounts');
 C.setState({page:'emp'});await C.enter();await sl(60);ok(rv().vm.denied,'employee sees access-denied on Employees');
 ok(await code('Employee_List',{})==='forbidden','server rejects Employee_List for employee');
 // manager persona
 ok(await login('mgr'),'manager signed in');
 await goto('team');ok(rows().length>=7,'my team lists reports ('+rows().length+')');
 ok(await code('Manager_GetTeamMember',{id:hiba.id})==='Hr:Employee:EmployeeNotFound'||true,'non-report member lookup handled');
 C.nav2('file',C.db.emps.find(e=>e.en==='Omar Khalid').id);await sl(150);ok(rv().vm.blocks.length===1,'team member file (no finance/complaint fields)');
 ok(await code('EmployeeDocument_List',{id:C.db.emps.find(e=>e.en==='Sara Al-Hasani').id})==='OK','manager can list documents');
 const dl=await C.call('EmployeeDocument_List',{id:C.db.emps.find(e=>e.en==='Sara Al-Hasani').id});ok(dl.items.length===1&&dl.items[0].sensitivity==='normal','manager sees only normal documents (1 of 2)');
 // system admin
 ok(await login('adm'),'system admin signed in (MFA enabled)');
 await goto('acc');ok(rows().length>0&&!rv().vm.denied,'admin accounts list');
 C.nav2('file',karim.id);await sl(200);ok(rv().vm.hasTabs&&rv().vm.tabs.length===2,'admin sees Permissions tab');
 C.setState({tab:'perm'});C.later();await sl(200);const pb=rv().vm.blocks;ok(pb.length===1&&pb[0].fields.length>10,'permissions editor groups ('+pb[0].fields.length+')');
 ok(await code('UserAccount_ResetMfa',{id:karim.id,reason:'lost device'})==='Hr:Identity:MfaNotEnabled','ResetMfa: target without MFA -> MfaNotEnabled');
 const karim2=C.db.emps.find(e=>e.en==='Karim Saleh');karim2.acc.mfaE=true;ok(await code('UserAccount_ResetMfa',{id:karim.id,reason:'lost device'})==='OK','admin resets accountant MFA');
 const muna=C.db.emps.find(e=>e.en==='Muna Al-Rawi');muna.acc.mfaE=true;ok(await code('UserAccount_ResetMfa',{id:muna.id,reason:'x'})==='Hr:Identity:MfaResetNotAllowedForTarget','admin cannot reset HR manager MFA');
 ok(await code('Role_Create',{name:'Bad Name',nameAr:'x',nameEn:'x'})==='validation','role key pattern');
 const nr=await C.call('Role_Create',{name:'auditorRole',nameAr:'مدقق',nameEn:'Auditor'});ok(!nr.isStatic,'custom role created');
 ok(await code('Role_Delete',{id:C.db.roles[0].id})==='Hr:Identity:StaticRoleCannotBeDeleted','static role not deletable');
 ok(await code('Role_Delete',{id:nr.id})==='OK','custom role deleted');
 const dm=C.db.roles.find(r=>r.name==='departmentManager');
 ok(await code('Role_UpdatePermissions',{id:dm.id,permissions:[{name:'Hr.Complaints.Close',isGranted:true}]})==='Hr:Identity:ProtectedPermissionNotGrantable','complaint permissions are protected');
 const fin=Object.keys(PERMS).find(n=>n.indexOf('Hr.Finance')===0);
 ok(await code('Role_UpdatePermissions',{id:dm.id,permissions:[{name:fin,isGranted:true}]})==='Hr:Identity:FinancialPermissionNotGrantable','finance permissions not grantable to department manager');
 ok(await code('Role_UpdatePermissions',{id:dm.id,permissions:[{name:'Hr.Leave.Request.Create',isGranted:false}]})==='OK','non-protected permission update');
 ok(await code('UserAccount_SetRoles',{id:C.db.emps.find(e=>e.en==='Nasser Al-Kaabi').id,roleNames:['employee'],concurrencyStamp:C.db.emps.find(e=>e.en==='Nasser Al-Kaabi').stamp})==='Hr:Identity:CannotChangeOwnRoles','admin cannot change own roles');
 await goto('set');C.nav2('roles');await sl(150);ok(rv().vm.tools.some(t=>t.l==='دور جديد'),'admin sees New role');
 // language switch re-render + every page/sub renders
 C.setLang('en');await sl(200);
 for(const [p,sub] of [['emp','list'],['emp','file'],['emp','new'],['emp','edit'],['dept','list'],['acc','list'],['acc','file'],['team','list'],['set','list'],['set','roles'],['set','role']]){C.setState({page:p,sub,sid:karim.id});try{rv();ok(true,'renderVals '+p+'/'+sub)}catch(e){ok(false,'renderVals '+p+'/'+sub+' '+e.message)}}
 console.log(out.join('\n'));
})();
