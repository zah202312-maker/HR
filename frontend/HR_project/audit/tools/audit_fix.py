import re
s=open('/home/claude/audit/Main.cur',encoding='utf-8').read()
def rep(o,n,c=1):
    global s
    assert s.count(o)==c,(o[:90],s.count(o));s=s.replace(o,n)
# 1 accessible text tokens (measured: light accent text 3.02:1, light ok 3.77:1, dark error 3.89:1)
rep("--ac:#00C49F;","--ac:#00C49F;--act:#00C49F;--ert:#F87171;--okt:#34D399;")
rep("--ac:#00A887;","--ac:#00A887;--act:#007A63;--ert:#DC2626;--okt:#047857;")
n_ac=s.count('color:var(--ac)');n_er=s.count('color:var(--er)');n_ok=s.count('color:var(--ok)')
s=s.replace('color:var(--ac)','color:var(--act)').replace('color:var(--er)','color:var(--ert)').replace('color:var(--ok)','color:var(--okt)')
print('token swaps',n_ac,n_er,n_ok)
# 2 spinner
rep("border-top-color:#fff;border-radius:50%;animation:spn","border-top-color:currentColor;border-radius:50%;animation:spn")
# 3 table header scope
n_th=s.count('<th>');s=s.replace('<th>','<th scope="col">');print('th scope',n_th)
# 4 form control names (both field templates)
rep('<input class="in" type="{{fd.type}}" maxlength="{{fd.max}}" value="{{fd.value}}" onChange="{{fd.on}}">','<input class="in" type="{{fd.type}}" maxlength="{{fd.max}}" value="{{fd.value}}" aria-label="{{fd.l}}" aria-invalid="{{fd.invalid}}" onChange="{{fd.on}}">',2)
rep('<textarea class="in" style="min-height:84px;padding:10px 14px" maxlength="{{fd.max}}" value="{{fd.value}}" onChange="{{fd.on}}">','<textarea class="in" style="min-height:84px;padding:10px 14px" maxlength="{{fd.max}}" value="{{fd.value}}" aria-label="{{fd.l}}" aria-invalid="{{fd.invalid}}" onChange="{{fd.on}}">',2)
rep('<select class="in" value="{{fd.value}}" onChange="{{fd.on}}">','<select class="in" value="{{fd.value}}" aria-label="{{fd.l}}" aria-invalid="{{fd.invalid}}" onChange="{{fd.on}}">',2)
rep('<input class="in" type="file" onChange="{{fd.onFile}}">','<input class="in" type="file" aria-label="{{fd.l}}" onChange="{{fd.onFile}}">',2)
rep('<div class="ckg">','<div class="ckg" role="group" aria-label="{{fd.l}}">',2)
rep("return{isFile:sp.t==='file',","return{invalid:!!s.fe[sp.k],isFile:sp.t==='file',")
# 5 Escape closes dialogs / drawers / popups (handler on the dialog itself; no global key listener)
rep('aria-label="{{dlgv.title}}" onClick="{{stop}}"','aria-label="{{dlgv.title}}" onClick="{{stop}}" onKeyDown="{{dlgKey}}"')
rep('aria-label="{{ds.D.mT}}" onClick="{{stop}}"','aria-label="{{ds.D.mT}}" onClick="{{stop}}" onKeyDown="{{ds.escKey}}"')
rep('aria-label="{{ds.D.cT}}" onClick="{{stop}}"','aria-label="{{ds.D.cT}}" onClick="{{stop}}" onKeyDown="{{ds.escKey}}"')
rep('<aside class="drw {{ds.drCls}}" role="dialog" aria-label="{{ds.D.drT}}">','<aside class="drw {{ds.drCls}}" role="dialog" aria-label="{{ds.D.drT}}" onKeyDown="{{ds.escKey}}">')
rep(" t:D.t,sub:D.sub,"," escKey:(e)=>{if(e&&e.key==='Escape'){this.dsSet('modal',false);this.dsSet('conf',false);this.dsSet('dr',false);this.dsSet('dd',false);this.dsSet('pop',false)}},t:D.t,sub:D.sub,")
rep("const m2o={resetMock:this.resetMock,","const m2o={dlgKey:(e)=>{if(e&&e.key==='Escape'){this.dlgClose()}},lcode:L,resetMock:this.resetMock,")
# 6 language attribute follows the UI language
rep('<div class="app {{thmCls}}" dir="{{dir}}" data-th="{{th}}"','<div class="app {{thmCls}}" lang="{{lcode}}" dir="{{dir}}" data-th="{{th}}"')
# 7 top-bar search: only where it has a contract-backed target (Employee_List search), localized label
rep('<label class="srch"><svg class="i" viewBox="0 0 24 24" aria-hidden="true"><path d="M11 3a8 8 0 1 0 0 16 8 8 0 0 0 0-16M21 21l-4.3-4.3"/></svg><input placeholder="{{tx.srch}}" aria-label="search" value="{{q}}" onChange="{{onQ}}"><kbd>⌘K</kbd></label>','<sc-if value="{{canAdd}}" hint-placeholder-val="{{false}}"><label class="srch"><svg class="i" viewBox="0 0 24 24" aria-hidden="true"><path d="M11 3a8 8 0 1 0 0 16 8 8 0 0 0 0-16M21 21l-4.3-4.3"/></svg><input placeholder="{{tx.srch}}" aria-label="{{tx.srch}}" value="{{q}}" onChange="{{onQ}}"></label></sc-if>')
rep("onQ:e=>this.setState({q:e.target.value}),","onQ:e=>{const v=e.target.value;this.setState({q:v});if(this.can('emp')){this.setState({page:'emp',sub:'list',sid:null,tab:'data',flt:{eq:v},skip:{},data:{}});this.later()}},")
# 8 notifications bell: remove the fake unread badge; label the sample list honestly
rep('.nt::after{content:"4";','.nt::after{content:"";display:none!important;')
rep('<b style="display:block;padding:6px 8px 10px">{{tx.nt}}</b><sc-for list="{{nts}}" as="n">','<b style="display:block;padding:6px 8px 2px">{{tx.nt}}</b><small style="display:block;padding:0 8px 10px;color:var(--tx2)">{{tx.bellNote}}</small><sc-for list="{{nts}}" as="n">')
rep("T2.ar.rsT=","T2.ar.bellNote='عينة تجريبية: وحدة الإشعارات (Notification_*) لم تُبنَ بعد';T2.en.bellNote='Sample only: the notifications module (Notification_*) is not built yet';T2.ar.rsT=")
# 9 remove dead legacy 'new employee' modal markup
a=s.index('<sc-if value="{{modal}}"');i=a;d=0
for m in re.finditer(r'<sc-if\b|</sc-if>',s[a:]):
    d+=1 if m.group(0)=='<sc-if' else -1
    if d==0:b=a+m.end();break
print('dead modal markup removed:',b-a,'chars');s=s[:a]+s[b:]
# 10 employee file: bulk-assign employees to a department manager (Manager_AssignEmployees, S-EMP-06)
rep("else if(s.sub==='file'){D.one=await this.call('Employee_Get',{id:s.sid});\n","else if(s.sub==='file'){D.one=await this.call('Employee_Get',{id:s.sid});D.lk=await this.call('Employee_Lookup',{});\n")
rep("if(!e.account)vm.tools.push({l:T.aNewAcc","if(e.account&&e.account.roleNames.includes('departmentManager'))vm.tools.push({l:T.aAssign,go:()=>this.dlgOpen({title:T.dAssign,fields:[{k:'ids',l:T.lEmps,t:'ck',req:1,opts:(d.lk?d.lk.items:[]).filter(z=>z.id!==e.id).map(z=>({v:z.id,l:z.fullName}))},{k:'reason',l:T.lReason,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('Manager_AssignEmployees',{id:e.id,employeeIds:v.ids,reason:v.reason||undefined});return{m:T.tAssigned+' · '+f(r.assignedCount)+' / '+f(r.alreadyAssignedCount)}}})});\n    if(!e.account)vm.tools.push({l:T.aNewAcc")
# 11 dashboard: finance-flavoured demo widgets only for roles that may see finance data; counts from the shared mock where available
rep("const kp=[[412,","const kp0=[[412,")
rep("[248,'#3B82F6',IC[1]","[this.db?this.db.emps.filter(e=>e.status==='active').length:248,'#3B82F6',IC[1]")
rep("[s.lv.filter(x=>x.s==='p').length,'#F59E0B'","[this.d4?this.d4.reqs.filter(r=>['pendingManager','pendingHr','pendingExecutive'].includes(r.status)).length:s.lv.filter(x=>x.s==='p').length,'#F59E0B'")
rep("['2.6%','4.2%','1.8%','','12%'][n]}));","['2.6%','4.2%','1.8%','','12%'][n]}));\nconst fin0=!!(s.me&&s.me.roleNames.some(r=>['hrManager','executiveDirector','accountant'].includes(r)));const kp=fin0?kp0:kp0.slice(1).map((k,n)=>({...k,big:n===0?'big':''}));")
rep("return{...m1,","return{finOK:fin0,...m1,")
i=s.index('<h2 class="h">{{tx.pay}}</h2>');cs=s.rindex('<div class="card" style="--d:.66s"',0,i);ce=s.index('</sc-for></div>',i)+len('</sc-for></div>')
s=s[:cs]+'<sc-if value="{{finOK}}" hint-placeholder-val="{{true}}">'+s[cs:ce]+'</sc-if>'+s[ce:]
open('/home/claude/audit/Main.fixed','w',encoding='utf-8').write(s)
sc=re.search(r"data-dc-script[^>]*>(.*?)</script>",s,re.S).group(1);open('/home/claude/audit/script.js','w').write(sc);print(len(s))
