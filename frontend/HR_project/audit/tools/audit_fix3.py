import re
s=open('/home/claude/audit/Main.fixed',encoding='utf-8').read()
def rep(o,n,c=1):
    global s
    assert s.count(o)==c,(o[:80],s.count(o));s=s.replace(o,n)
rep('<main class="{{outCls}}">','<main id="main" tabindex="-1" class="{{outCls}}">')
rep('<div class="app {{thmCls}}" lang="{{lcode}}" dir="{{dir}}" data-th="{{th}}" style="--s:{{sgn}}">','<div class="app {{thmCls}}" lang="{{lcode}}" dir="{{dir}}" data-th="{{th}}" style="--s:{{sgn}}"><a class="skip" href="#main">{{tx.skipT}}</a>')
rep('<nav class="nv">','<nav class="nv" aria-label="{{tx.mainNavT}}">')
rep(".srch{display:flex;",".skip{position:absolute;inset-inline-start:12px;top:-60px;z-index:200;padding:10px 16px;border-radius:12px;background:var(--ac);color:#0F172A;font-weight:700;text-decoration:none;transition:top .15s}.skip:focus{top:10px}\n.srch{display:flex;")
rep("T2.ar.rsT=","T2.ar.skipT='تخطي إلى المحتوى';T2.en.skipT='Skip to content';T2.ar.mainNavT='التنقل الرئيسي';T2.en.mainNavT='Main navigation';T2.ar.rsT=")
open('/home/claude/audit/Main.fixed','w',encoding='utf-8').write(s)
sc=re.search(r"data-dc-script[^>]*>(.*?)</script>",s,re.S).group(1);open('/home/claude/audit/script.js','w').write(sc);print('ok')
