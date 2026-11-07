process.on('unhandledRejection',e=>{console.log('REJECT',e&&e.stack?e.stack.split('\n').slice(0,3).join(' | '):JSON.stringify(e));console.log(global.__out.join('\n'));process.exit(1)});
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,Math.min(ms||0,3));
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;globalThis.T4=T4;');
const T=globalThis.T4.ar;const C=new globalThis.__C();const sl=ms=>new Promise(r=>st(r,ms||40));
const out=global.__out=[];const ok=(c,m)=>{out.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals(),vm=()=>rv().vm;
const login=async(k)=>{C.signOut();C.resetMock();C.setState({persona:k});await C.doSignin();await sl(250);return C.state.scr==='app'};
const goto=async(p)=>{C.go(p);await sl(80);await C.enter();await sl(40)};
const E=(n)=>C.db.emps.find(e=>e.en===n);
(async()=>{
 await login('hr');
 ok(rv().canAdd===true&&rv().finOK===true&&rv().kp.length===5,'HR: search visible, finance widgets visible, 5 KPIs');
 ok(Number(rv().kp[1].t)===C.db.emps.filter(e=>e.status==='active').length,'dashboard employee KPI reads the shared mock');
 C.onQ&&0;const q=rv().onQ({target:{value:'سارة'}});await sl(120);ok(C.state.page==='emp'&&C.state.flt.eq==='سارة','HR search navigates to Employees with the Employee_List search filter');
 // Escape closes dialogs
 C.dlgOpen({title:'x',fields:[],okLabel:'ok',run:async()=>({m:'x'})});ok(rv().hasDlg,'dialog opened');rv().dlgKey({key:'Tab'});ok(rv().hasDlg,'other keys keep dialog open');rv().dlgKey({key:'Escape'});ok(!rv().hasDlg,'Escape closes the dialog');
 // lang attribute
 C.setState({lang:'en'});ok(rv().lcode==='en','lang attribute follows UI language (en)');C.setState({lang:'ar'});ok(rv().lcode==='ar','lang attribute follows UI language (ar)');
 // aria-invalid
 C.dlgOpen({title:'x',fields:[{k:'a',l:'A',req:1,max:5}],okLabel:'ok',run:async()=>({m:'x'})});C.dlgGo();await sl(20);ok(rv().dlgv.fields[0].invalid===true&&rv().dlgv.fields[0].err.length>0,'failed validation sets aria-invalid + message');C.dlgClose();
 // assign employees (S-EMP-06)
 await goto('emp');const mgr=E('Hassan Ali');C.toSub('file',mgr.id);await sl(200);
 const tool=vm().tools.find(t=>t.l===T.aAssign);ok(!!tool,'department manager file offers “assign employees”');
 tool.go();await sl(30);ok(rv().hasDlg&&rv().dlgv.fields[0].isCk&&rv().dlgv.fields[0].opts.length>=10&&rv().dlgv.fields[0].opts.every(o=>o.v!==mgr.id),'dialog lists employees except the manager');
 C.dlgGo();await sl(30);ok(rv().dlgv.fields[0].err,'at least one employee is required');
 const hib=E('Hiba Al-Saadi');C.setF('ids',[hib.id]);C.setF('reason','إعادة هيكلة');C.dlgGo();await sl(250);ok(!rv().hasDlg&&C.db.emps.find(e=>e.id===hib.id).mgr===mgr.id,'Manager_AssignEmployees applied (Hiba now reports to Hassan)');
 const r2=await C.call('Manager_AssignEmployees',{id:mgr.id,employeeIds:[hib.id]});ok(r2.assignedCount===0&&r2.alreadyAssignedCount===1,'idempotent: already-assigned counted separately');
 const emp0=E('Omar Khalid');C.toSub('file',emp0.id);await sl(150);ok(!vm().tools.some(t=>t.l===T.aAssign),'no assign tool on a non-manager file');
 // finance gating
 await login('emp');ok(rv().canAdd===false&&rv().finOK===false&&rv().kp.length===4&&rv().kp[0].big==='big','employee: no search, no payroll widgets, KPIs re-flowed');
 await login('mgr');ok(rv().finOK===false,'department manager: no finance widgets');
 await login('xd');ok(rv().finOK===true,'executive: finance widgets visible');
 await login('acc').catch(()=>0);
 console.log(out.join('\n'));
})();
