import json,glob,re,sys,yaml,collections
d=yaml.safe_load(open('/home/claude/api/api-contract/openapi.yaml',encoding='utf-8'))
sch=d['components']['schemas']
OP={}
for path,item in d['paths'].items():
    for m,o in item.items():
        if isinstance(o,dict) and 'operationId' in o:
            OP[o['operationId'].replace('_','_')]=(path,m,o)
# operation ids in contract are like Employee_Get? check naming
def ref(s):
    while isinstance(s,dict) and '$ref' in s: s=sch[s['$ref'].split('/')[-1]]
    return s
def pname(s): return s['$ref'].split('/')[-1] if isinstance(s,dict) and '$ref' in s else None
UU=re.compile(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$')
DT=re.compile(r'^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})$');DA=re.compile(r'^\d{4}-\d{2}-\d{2}$')
def check(v,s,path,errs,depth=0):
    if depth>12:return
    raw=s;s0=s
    name=pname(raw);s=ref(raw)
    if not isinstance(s,dict):return
    if v is None and (s.get('nullable') or (isinstance(raw,dict) and raw.get('nullable'))): return
    if 'allOf' in s:
        for a in s['allOf']: check(v,a,path,errs,depth+1)
        return
    if v is None:
        if not s.get('nullable') and not (isinstance(raw,dict) and raw.get('nullable')): errs.append(f'{path}: null but not nullable')
        return
    t=s.get('type')
    if 'enum' in s and v not in s['enum']: errs.append(f'{path}: {v!r} not in enum {s["enum"]}')
    if t=='object' or 'properties' in s:
        if not isinstance(v,dict): errs.append(f'{path}: expected object got {type(v).__name__}');return
        props=s.get('properties',{})
        for r in s.get('required',[]):
            if r not in v: errs.append(f'{path}.{r}: required field missing')
        for k,x in v.items():
            if k not in props:
                if s.get('additionalProperties') is False or props: errs.append(f'{path}.{k}: not in schema')
            else: check(x,props[k],path+'.'+k,errs,depth+1)
    elif t=='array':
        if not isinstance(v,list): errs.append(f'{path}: expected array');return
        for i,x in enumerate(v[:30]): check(x,s.get('items',{}),f'{path}[{i}]',errs,depth+1)
    elif t=='string':
        if not isinstance(v,str): errs.append(f'{path}: expected string got {type(v).__name__}');return
        f=s.get('format')
        if f=='uuid' and not UU.match(v): errs.append(f'{path}: bad uuid {v[:40]!r}')
        if f=='date-time' and not DT.match(v): errs.append(f'{path}: bad date-time {v[:40]!r}')
        if f=='date' and not DA.match(v): errs.append(f'{path}: bad date {v[:40]!r}')
        if 'pattern' in s and not re.search(s['pattern'],v): errs.append(f'{path}: pattern {s["pattern"]} fails for {v[:30]!r}')
        if 'maxLength' in s and len(v)>s['maxLength']: errs.append(f'{path}: too long')
    elif t in('integer','number'):
        if isinstance(v,bool) or not isinstance(v,(int,float)): errs.append(f'{path}: expected number got {type(v).__name__}')
        elif t=='integer' and v!=int(v): errs.append(f'{path}: not integer {v}')
        elif 'minimum' in s and v<s['minimum']: errs.append(f'{path}: below minimum')
    elif t=='boolean':
        if not isinstance(v,bool): errs.append(f'{path}: expected boolean')
rec=[]
for f in glob.glob('rec/out_*.json'): rec+=json.load(open(f))
stat=collections.defaultdict(lambda:{'n':0,'bad':0});viol=collections.defaultdict(set);qbad=collections.defaultdict(set);seen=set()
for r in rec:
    if not r['ok'] or r['op'] not in OP: continue
    path,m,o=OP[r['op']];st=stat[r['op']];st['n']+=1
    # response
    resp=None
    for code,rs in o['responses'].items():
        if str(code).startswith('2'):
            c=rs.get('content',{}).get('application/json');
            if c: resp=c['schema']
    errs=[]
    if resp is not None: check(r['r'],resp,'response',errs)
    elif r['r'] not in(None,{}) and 'octet' not in json.dumps(o['responses']) : pass
    # request keys
    allowed={(d['components']['parameters'][p['$ref'].split('/')[-1]]['name'] if '$ref' in p else p['name']) for p in o.get('parameters',[])}
    rb=o.get('requestBody',{}).get('content',{})
    body=None
    for ct,c in rb.items(): body=c['schema'];break
    bprops=set();breq=set()
    if body is not None:
        bs=ref(body);bprops=set(bs.get('properties',{}));breq=set(bs.get('required',[]))
    x=r.get('x') or {}
    unknown=[k for k in x if k not in allowed and k not in bprops and k not in('skipCount','maxResultCount','sorting','Idempotency-Key')]
    if unknown and not (op:=r['op']).startswith('LeaveRequest_UploadAttachment') and not op.startswith('Complaint_UploadAttachment') and 'Upload' not in op: errs.append('request: unknown keys '+','.join(unknown))
    for e in errs: viol[r['op']].add(e[:230])
    if errs: st['bad']+=1
print('recorded calls',len(rec),'| successful validated ops',len(stat),'| responses/requests with violations',sum(v['bad'] for v in stat.values()),'of',sum(v['n'] for v in stat.values()))
for op,es in sorted(viol.items()):
    print(op,len(es));[print('   ',e) for e in sorted(es)[:4]]
json.dump({'stat':stat,'viol':{k:sorted(v) for k,v in viol.items()},'calls':len(rec),'checked':sum(x['n'] for x in stat.values()),'bad':sum(x['bad'] for x in stat.values())},open('validation.json','w'))
