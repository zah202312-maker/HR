
const fs=require('fs');const st=setTimeout;global.setTimeout=(f,ms)=>st(f,3);global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(u){const n=typeof u==='function'?u(this.state):u;this.state={...this.state,...n}}}global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+';globalThis.__C=Component;');const C=new globalThis.__C();
const keys=new Set();const add=()=>Object.keys(C.renderVals()).forEach(k=>keys.add(k));
(async()=>{add();C.setState({persona:'hr'});await C.doSignin();await new Promise(r=>st(r,300));
for(const p of ['dash','emp','leave','me','ds','dept','acc','team','set','att']){C.setState({page:p});add()}
for(const sc of ['signin','pwd','mfa','rc','codes']){C.setState({scr:sc});add()}
console.log(JSON.stringify([...keys]))})();
