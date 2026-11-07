const fs=require('fs');
global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}
global.DCLogic=DCLogic;
const src=fs.readFileSync('script.js','utf8')+'\n;globalThis.__C=Component;';
(0,eval)(src);
const C=new globalThis.__C();
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const log=[];const ok=(c,m)=>{log.push((c?'PASS ':'FAIL ')+m);if(!c)process.exitCode=1};
const rv=()=>C.renderVals();
(async()=>{
 ok(rv().authScr&&rv().isSignin&&!rv().inApp,'initial state is sign-in, shell hidden');
 // new employee: must change password
 C.setState({persona:'nw'});await C.doSignin();await sleep(1700);
 ok(C.state.scr==='pwd'&&C.state.me.accessGate==='passwordChangeRequired','new employee routed to forced password change');
 try{await C.call('Me_GetInboxSummary');ok(false,'gate must block')}catch(e){ok(e.status===403&&e.code==='Hr:Identity:PasswordChangeRequired','403 PasswordChangeRequired on blocked op')}
 C.setState({pw1:'wrong',pw2:'Newpass123',pw3:'Newpass123'});await C.submitPw();await sleep(900);
 ok(rv().hasErr&&rv().aerr.includes('غير صحيحة'),'wrong current password -> localized 422 message');
 C.setState({pw1:'Temp#2026',pw2:'Temp#2026',pw3:'Temp#2026'});await C.submitPw();await sleep(900);
 ok(C.state.aerr==='Hr:Identity:NewPasswordSameAsCurrent','same password rejected');
 C.setState({pw1:'Temp#2026',pw2:'short',pw3:'short'});await C.submitPw();await sleep(900);
 ok(C.state.aerr==='Hr:Identity:PasswordPolicyViolation','policy violation code surfaced');
 C.setState({pw1:'Temp#2026',pw2:'abc',pw3:'abd'});await C.submitPw();ok(C.state.aerr==='pwMis','client confirm mismatch');
 C.setState({pw1:'Temp#2026',pw2:'Newpass123',pw3:'Newpass123'});await C.submitPw();await sleep(2200);
 ok(C.state.scr==='app'&&C.state.me.accessGate==='none'&&rv().inApp,'password changed -> gate lifted -> app');
 console.log('DBG',JSON.stringify([rv().inbTotal,rv().noInb,rv().ev.dept]));ok(rv().noInb&&rv().ev.dept==='الهندسة','home data loaded (empty inbox, employee file)');
 (C.signOut(),C.resetMock());ok(rv().isSignin,'sign out returns to sign-in');
 // accountant: MFA enrollment
 C.setState({persona:'acc'});await C.doSignin();await sleep(1700);
 ok(C.state.scr==='mfa','accountant routed to MFA enrollment');
 try{await C.call('Me_GetInboxSummary');ok(false,'x')}catch(e){ok(e.code==='Hr:Identity:MfaEnrollmentRequired','403 MfaEnrollmentRequired on blocked op')}
 await C.submitMfa();ok(C.state.aerr==='mfaBad','empty code rejected client-side (pattern ^[0-9]{6}$)');
 await C.startMfa();await sleep(700);ok(rv().hasKey&&rv().mkKey.length>8,'enrollment key returned');
 C.setState({code:'000000'});await C.submitMfa();await sleep(700);ok(C.state.aerr==='Hr:Identity:MfaCodeInvalid','invalid TOTP code -> 422 code');
 C.setState({code:'123456'});await C.submitMfa();await sleep(700);
 ok(C.state.scr==='codes'&&C.state.codes.length===10,'recovery codes shown (10)');
 rv().contGate();ok(true,'continue blocked until saved: '+(C.state.scr==='codes'));
 C.setState({saved:true});rv().contGate();await sleep(1700);
 ok(C.state.scr==='app'&&rv().mfaOn&&rv().mfaRel,'MFA enabled, app unlocked, status loaded');
 ok(rv().mfaRem==='١٠'||rv().mfaRem==='10','recovery codes remaining = 10');
 // regenerate
 C.setState({scr:'rc',code:'123456'});await C.submitRc();await sleep(700);ok(C.state.scr==='codes','recovery codes regenerated');
 (C.signOut(),C.resetMock());
 // manager: log + inbox + paging + filter
 C.setState({persona:'mgr'});await C.doSignin();await sleep(2200);
 ok(rv().isMgr&&rv().logRows.length===10&&C.state.log.totalCount===23,'manager task log paged (10 of 23)');
 await C.loadLog(10);await sleep(600);ok(rv().logRows.length===10&&rv().canPrev&&rv().canNext,'page 2');
 await C.loadLog(0,{tlType:'requestApproved'});await sleep(600);ok(C.state.log.items.every(x=>x.taskType==='requestApproved'),'taskType filter');
 ok(rv().hasInb&&rv().inbTotal.length>0&&rv().inbItems.some(x=>x.d),'inbox summary with delegated count');
 ok(!rv().inbItems.some(x=>/complaint|شكو/i.test(x.l)),'inbox never contains complaints');
 // language switch via Me_Update
 C.setLang('en');await sleep(1500);ok(C.state.me.preferredLanguage==='en'&&rv().meName==='Hassan Ali','language switch syncs Me_Update and refreshes names');
 C.go&&0;
 // employee cannot call MFA / task log
 (C.signOut(),C.resetMock());C.setState({persona:'emp'});await C.doSignin();await sleep(2000);
 try{await C.call('Mfa_GetStatus');ok(false,'x')}catch(e){ok(e.status===403,'non-MFA role rejected on Mfa_GetStatus')}
 ok(!rv().isMgr&&!rv().mfaRel,'employee: no task log, no MFA card');
 // render every page state without throwing
 for(const p of['dash','emp','leave','me','att']){C.setState({page:p});try{rv();ok(true,'renderVals page '+p)}catch(e){ok(false,'renderVals '+p+' '+e.message)}}
 for(const sc of['signin','pwd','mfa','rc','codes']){C.setState({scr:sc});try{rv();ok(true,'renderVals scr '+sc)}catch(e){ok(false,e.message)}}
 console.log(log.join('\n'));
})();
