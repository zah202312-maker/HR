const fs=require('fs');global.matchMedia=()=>({matches:false});
class DCLogic{constructor(){this.props={}}setState(){}};global.DCLogic=DCLogic;
(0,eval)(fs.readFileSync('script.js','utf8')+';globalThis.X={T:typeof T!=="undefined"?T:null,T2:typeof T2!=="undefined"?T2:null,T4:T4}');
const flat=(o,p='')=>Object.keys(o).flatMap(k=>o[k]&&typeof o[k]==='object'&&!Array.isArray(o[k])?flat(o[k],p+k+'.'):[p+k]);
const res={};
for(const n of ['T','T2','T4']){const t=X[n];if(!t){res[n]='absent';continue}const a=flat(t.ar),b=flat(t.en);res[n]={ar:a.length,en:b.length,onlyAr:a.filter(k=>!b.includes(k)),onlyEn:b.filter(k=>!a.includes(k))}}
const get=(o,k)=>k.split('.').reduce((v,p)=>v&&v[p],o);
res.emptyAr=flat(X.T4.ar).filter(k=>get(X.T4.ar,k)==='').length;res.emptyEn=flat(X.T4.en).filter(k=>get(X.T4.en,k)==='').length;
fs.writeFileSync('parity.json',JSON.stringify(res));console.log(JSON.stringify(res).slice(0,600));
