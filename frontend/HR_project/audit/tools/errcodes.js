const fs=require('fs');global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(){}};global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+';globalThis.X={T:typeof T!=="undefined"?T:{ar:{},en:{}},T2:typeof T2!=="undefined"?T2:{ar:{},en:{}},T4:T4}');
const k=(o,a=[])=>{Object.keys(o).forEach(x=>{if(/^Hr:/.test(x))a.push(x);else if(o[x]&&typeof o[x]==='object')k(o[x],a)});return a};
const all=(l)=>[...new Set([...k(X.T[l]),...k(X.T2[l]),...k(X.T4[l])])];
fs.writeFileSync('errcodes.json',JSON.stringify({ar:all('ar'),en:all('en')}));
