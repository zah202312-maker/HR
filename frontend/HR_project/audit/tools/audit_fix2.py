import re
s=open('/home/claude/audit/Main.fixed',encoding='utf-8').read()
def rep(o,n,c=1):
    global s
    assert s.count(o)==c,(o[:80],s.count(o));s=s.replace(o,n)
# contract-accuracy fixes found by schema validation of the recorded mock traffic
rep("UID=(n)=>'00000000-0000-4000-8000-0000000000'+n","UID=(n)=>'00000000-0000-4000-8000-'+('000000000000'+n).slice(-12)")
rep("ok({currentUser:{isAuthenticated:true,id:this.UID(P.id),userName:P.u,roles:P.roles},extraProperties:","ok({localization:{currentCulture:{cultureName:ix?'en':'ar',name:ix?'en':'ar',displayName:ix?'English':'العربية',twoLetterIsoLanguageName:ix?'en':'ar',isRightToLeft:!ix},defaultResourceName:null,languages:[{cultureName:'ar',uiCultureName:'ar',displayName:'العربية'},{cultureName:'en',uiCultureName:'en',displayName:'English'}],values:{}},auth:{grantedPolicies:{}},setting:{values:{}},currentUser:{isAuthenticated:true,id:this.UID(P.id),userName:P.u,roles:P.roles},extraProperties:")
rep("case 'Attendance_GetMy':{if(!me||exe(me))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');const a=this.daysOf(me,x);return {totalCount:a.length,items:pg(a,x).items}}","case 'Attendance_GetMy':{if(!me||exe(me))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');const a=this.daysOf(me,x);return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}")
rep("a.sort((p,q)=>q.date.localeCompare(p.date)||p.employee.fullName.localeCompare(q.employee.fullName));return {totalCount:a.length,items:pg(a,x).items}}","a.sort((p,q)=>q.date.localeCompare(p.date)||p.employee.fullName.localeCompare(q.employee.fullName));return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}")
rep("const a=this.daysOf(e,x);return {totalCount:a.length,items:pg(a,x).items}}\n case 'Attendance_GetDayDetail'","const a=this.daysOf(e,x);return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}\n case 'Attendance_GetDayDetail'")
rep("rqAll=()=>{","attSum=(a)=>({presentDays:a.filter(z=>z.status==='present'||z.status==='late').length,lateDays:a.filter(z=>z.status==='late').length,totalLateMinutes:a.reduce((n,z)=>n+(z.lateMinutes||0),0),absentDays:a.filter(z=>z.status==='absent').length,incompleteDays:a.filter(z=>z.status==='incomplete').length,leaveDays:a.filter(z=>z.status==='onLeave').length,workedMinutes:a.reduce((n,z)=>n+(z.workedMinutes||0),0),overtimeMinutes:a.reduce((n,z)=>n+(z.overtimeMinutes||0),0)});\nrqAll=()=>{")
open('/home/claude/audit/Main.fixed','w',encoding='utf-8').write(s)
sc=re.search(r"data-dc-script[^>]*>(.*?)</script>",s,re.S).group(1);open('/home/claude/audit/script.js','w').write(sc);print('ok',len(s))
