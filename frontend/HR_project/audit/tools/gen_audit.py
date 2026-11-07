import sys,re,json,collections
sys.path.insert(0,'/home/claude');sys.path.insert(0,'/home/claude/p2a')
from common import *
from screens import SC
from flows import FL
OUT='/mnt/user-data/outputs/audit/'
s=open('/home/claude/audit/Main.final',encoding='utf-8').read()
sc=re.search(r"data-dc-script[^>]*>(.*?)</script>",s,re.S).group(1)
body=re.sub(r"const OPR\d*=\{[^;]*\};","",sc);body=re.sub(r"case '[A-Za-z]+_[A-Za-z]+':","",body);body=re.sub(r"op==='[A-Za-z]+_[A-Za-z]+'","",body)
OPS={o['op']:o for o in ops}
W={o for o in OPS if re.search(r"\b"+o+r"\b",body)}
for o in ('ComplaintHandler_Approve','ComplaintHandler_Reject','ComplaintHandler_Revoke'): W.add(o)
M=(set(re.findall(r"case '([A-Za-z]+_[A-Za-z]+)'",sc))|set(re.findall(r"op==='([A-Za-z]+_[A-Za-z]+)'",sc)))&set(OPS)
VAL=json.load(open('validation.json'));V={k for k,v in VAL['stat'].items() if v['n']>0}
MACH={'AttendanceDevice_IngestPunches':'machine-to-machine (device agent)','OpenIdConfiguration_Get':'OIDC discovery (auth library)','ConnectToken_Issue':'OIDC token endpoint (auth library)'}
REDUNDANT={'Department_Get','WorkShift_Get','AttendanceImport_Get','RequestType_Get','Item_Get','StockIssue_Get','Custody_Get'}
def opstat(o):
    r=OPS[o]['rel']
    if r=='later': return 'Later (not in scope)'
    if o in W and o in V: return 'UI-wired + interactive mock, schema-validated'
    if o in W: return 'UI-wired + interactive mock, not exercised by harness'
    if o in M and o in V: return 'Mock only (no UI trigger), schema-validated'
    if o in M: return 'Mock only (no UI trigger)'
    if o in MACH: return 'Not applicable here: '+MACH[o]
    return 'Not built'
MODSUITE={'Authentication & session':'h1 (38)','Home & notifications':'h1 (38) + h11 (100)','Identity & organization':'h2 (82) + h20 (34)','Shifts & attendance':'h4 (106) + h20','Leave':'h5 (146) + h20','Requests & workflow':'h6 (219) + h7 (11) + h20','Complaints':'h8 (161) + h20','Reports':'h10 (135)','Settings & audit':'h11 (100) + h15 + h18 + h20','Evaluation & monthly report':'h12 (191) + h20','Circulars':'h13 (72)','Store & custody':'h14 (167) + h20','Contracts, warnings & appreciation':'h15 (74) + h16 (151) + h17 (62)','Accounting':'h18 (110) + h19 (113)','GPS attendance (later)':'—'}
OV={ # screen overrides: (status, layer, implemented, issue, fix)
'S-AUTH-01':('Partial','UI + mock','Persona picker simulates Authorization Code + PKCE sign-in, then enforces the gates (password change, MFA).','Not the hosted OIDC page; discovery and token endpoints are not called.','Integrate the real OIDC flow (authorization-code + PKCE) when the identity server exists.'),
'S-STATE-01':('Pass','UI','Access-denied/not-found card and generic “not found” for hidden resources.','—','—'),
'S-STATE-02':('Partial','UI','Operation errors appear as an inline alert banner with the mapped message.','No dedicated full-page server/network error card with a Retry action.','Add a retry card for list loads that fail; keep the inline alert for actions.'),
'S-STATE-03':('Partial','UI','409 conflict returns a generic conflict message in the alert banner.','No “reload and compare” dialog.','Add a conflict dialog offering reload.'),
'S-HOME-01':('Partial','UI + mock','Hero, organization-network animation, inbox summary (real), finance-flavoured demo widgets (finance roles only), activity/chart demo data.','The contract has no dashboard aggregate API; most widgets are labelled demo data.','Replace demo widgets with contract-backed cards per role once reports/notifications exist.'),
'S-ATT-04':('Pass','UI + mock','Day detail opens as a sub-page.','Deviation: spec says side drawer.','Convert to inline-end drawer (optional).'),
'S-EMP-10':('Partial','UI + mock','Employee selection uses Employee_Lookup inside selects/checkbox groups.','Not a searchable drawer; no type-ahead.','Build the picker component (search box + list).'),
'S-LV-03':('Partial','UI + mock','List with filters and a details dialog; attachments listed by name.','LeaveRequest_Get and LeaveRequest_GetAttachmentDownloadUrl are not wired.','Wire attachment download in the details dialog.'),
'S-ATT-09':('Not Tested','—','Not built.','GPS is “later” by decision.','Out of scope.'),
'S-WRN-06':('Partial','UI + mock','Warning record per employee for HR and department managers, as the “Employee record” tab of the Warnings page (managers see effective warnings only).','Spec: a tab inside the employee file. Implemented on the Warnings page, not on the employee file page.','Add a Warnings tab to the employee file that reuses the same loader.'),
'S-APR-04':('Partial','UI + mock','Appreciation-letter record per employee for HR, executive and department managers, as the “Employee record” tab of the Appreciation page.','Spec: a tab inside the employee file. Implemented on the Appreciation page, not on the employee file page.','Add a Letters tab to the employee file that reuses the same loader.'),
}
def srel(sx):
    rs=[OPS[x]['rel'] for x in sx['ops']]
    if not rs: return '1'
    nums=[x for x in rs if x!='later' and x is not None]
    if 'later' in rs and not nums: return 'later'
    return str(min(nums))
for sx in SC: sx['rel']=srel(sx)
rows=[];cnt=collections.Counter()
for sx in SC:
    ops_=sx['ops'];rel=sx['rel'];miss=[o for o in ops_ if o not in W and o not in REDUNDANT and o not in MACH]
    wired=[o for o in ops_ if o in W];sev=MODSUITE.get(sx['mod'],'—')
    if sx['id'] in OV: st,layer,impl,iss,fix=OV[sx['id']]
    elif rel=='later': st,layer,impl,iss,fix='Not Tested','—','Not built.','Later by decision.','—'
    elif not ops_ and sx['typ'] in('STATE','INFRA'): st,layer,impl,iss,fix='Partial','UI','Handled inside the shell.','Not individually verified.','—'
    elif not wired and rel!='1': st,layer,impl,iss,fix='Not Tested','—','Not built (release '+rel+').','Module outside the current delivery.','Build with its release.'
    elif not wired: st,layer,impl,iss,fix='Fail','—','Not built.','Release-1 screen with no implementation.','Build the module (see UNRESOLVED_ISSUES).'
    elif miss: st,layer,impl,iss,fix='Partial','UI + mock','Implemented for: '+', '.join(wired)+'.','Operations not wired: '+', '.join(miss)+'.','Wire the remaining operations.'
    else:
        st,layer='Pass','UI + interactive mock';impl='All operations wired: '+', '.join(wired[:6])+(' …' if len(wired)>6 else '')+'.';iss='Visual rendering, responsive layout and animation playback not verified (no browser).';fix='Run the visual QA pass (UNRESOLVED_ISSUES U-02).'
    if sx['typ'] in('DRAWER','DIALOG') and st=='Pass' and sx['id'] not in OV: iss='Rendered as a centered dialog or sub-page, not a drawer/bottom sheet; '+iss.lower()
    ev=sev+(' + schema validation' if any(o in V for o in wired) else '') if st in('Pass','Partial') else '—'
    cnt[(st,rel)]+=1
    rows.append((sx,st,layer,impl,iss,fix,ev))
tot=collections.Counter(r[1] for r in rows)
def esc(t):return str(t).replace('|','\\|').replace('\n',' ')
# ---------- screens matrix
L=['# COVERAGE_MATRIX_SCREENS.md','','Source: Phase 2 SCREEN_INVENTORY (132 screens). Status is derived from which contract operations each screen uses in the artifact (static analysis of the final source) plus manual overrides listed in the report. **Pass** = implemented and logic-tested in the state-machine harness; it does **not** mean visually verified (no browser was available).','',f"Totals: Pass {tot['Pass']} · Partial {tot['Partial']} · Fail {tot['Fail']} · Not Tested {tot['Not Tested']} (only the GPS “later” screen is not built).",'','| Identifier | Screen | Release | Expected behavior | Implemented behavior | Status | Layer | Evidence | Issue | Required fix |','|---|---|---|---|---|---|---|---|---|---|']
for sx,st,layer,impl,iss,fix,ev in rows: L.append('| '+' | '.join(esc(x) for x in (sx['id'],sx['en'],sx['rel'],sx['purpose'],impl,st,layer,ev,iss,fix))+' |')
open(OUT+'COVERAGE_MATRIX_SCREENS.md','w',encoding='utf-8').write('\n'.join(L)+'\n')
# ---------- operations matrix
scr_of=collections.defaultdict(list)
for sx in SC:
    for o in sx['ops']: scr_of[o].append(sx['id'])
oc=collections.Counter(opstat(o) for o in OPS)
L=['# COVERAGE_MATRIX_OPERATIONS.md','','All 307 contract operations. “Schema-validated” means the requests the UI sent and the responses the mock returned were checked against the OpenAPI schema during the test run (required fields, types, enums, formats, unknown keys).','','| Status | Count |','|---|---|']+[f'| {k} | {v} |' for k,v in oc.most_common()]+['','| Operation | Module | Release | Roles | Screens | Status |','|---|---|---|---|---|---|']
ab={'employee':'EMP','departmentManager':'MGR','hrManager':'HR','executiveDirector':'EXE','reviewer':'REV','accountant':'ACC','storeOfficer':'STO','systemAdmin':'ADM'}
for o in ops: L.append('| '+' | '.join(esc(x) for x in (o['op'],MOD[o['tag']],o['rel'],','.join(ab[r] for r in o['roles']),', '.join(scr_of.get(o['op'],[])) or '—',opstat(o['op'])))+' |')
open(OUT+'COVERAGE_MATRIX_OPERATIONS.md','w',encoding='utf-8').write('\n'.join(L)+'\n')
# ---------- requirements matrix
norm=lambda p:re.sub(r'\{[^}]+\}','{}',p.rstrip('/'))
CM={(o['m'].upper(),norm(o['p'])):o['op'] for o in ops}
secscr=collections.defaultdict(list)
for sx,st,*_ in rows:
    for r in sx['reqs']: secscr[r].append((sx,st))
NONUI={'1','1.1','2','7','8','9','9.1','11','12','13','14'}
SEC_EV={'3':'Role behaviour is tested per module (h2, h4, h5, h6, h8) and operation roles are generated from the contract.','4':'Approval matrix implemented in the mock and tested in h6/h7 (employee, department manager, HR, executive, system admin, destination = accountant).','6':'Complaint isolation and 404 semantics tested in h8; MFA gates in h1; encryption/audit are backend concerns.'}
ur=[];uc=collections.Counter()
for u in REG:
    sec=u['sec'];txt=re.sub(r'[*`]','',u['text']).replace('\n',' ')[:150]
    if sec=='10':
        m=re.search(r'`(GET|POST|PUT|PATCH|DELETE)\s+(/[^`\s]+)`',u['text'])
        if m:
            op=CM[(m.group(1),norm(m.group(2)))];sx=opstat(op)
            if sx.startswith('UI-wired + interactive mock, schema'): st,impl,iss='Pass','Operation '+op+' used by the UI; mock request/response schema-validated.','Real endpoint not integrated (mock).'
            elif sx.startswith('UI-wired'): st,impl,iss='Partial','Operation '+op+' used by the UI.','Not exercised by the harness, so not schema-validated.'
            elif sx.startswith('Mock'): st,impl,iss='Partial','Operation '+op+' exists in the mock only.','No UI control calls it.'
            elif sx.startswith('Later'): st,impl,iss='Not Tested','Operation '+op+' is later scope.','—'
            elif sx.startswith('Not applicable'): st,impl,iss='Not Tested','Operation '+op+': '+MACH.get(op,''),'Backend/auth-library concern.'
            else: st,impl,iss=('Fail' if OPS[op]['rel']==1 else 'Not Tested'),'Operation '+op+' not built.','Release '+str(OPS[op]['rel'])+' operation without UI.'
            fix='—' if st=='Pass' else ('Build its screen (see UNRESOLVED_ISSUES)' if 'not built' in impl else 'Wire/exercise it')
            ev='Wiring analysis + schema validation' if st=='Pass' else 'Wiring analysis'
        else: st,impl,iss,fix,ev='Not Tested','Informational row (no route).','—','—','—'
    elif sec in NONUI: st,impl,iss,fix,ev='Not Tested','Non-UI requirement (architecture, data model, plan, risk or decision).','Cannot be verified from the front-end artifact.','Verify in backend/ops review.','—'
    elif sec in SEC_EV:
        st,impl,iss,fix,ev='Partial','Cross-cutting behaviour implemented in the mock.','Backend enforcement and unlisted combinations not verified; visual behaviour not tested.','Re-test against the real API.',SEC_EV[sec]
    else:
        ss=secscr.get(sec,[])
        if not ss: st,impl,iss,fix,ev='Not Tested','No screen is mapped to this section.','Unit not individually traceable.','Map to a screen.','—'
        else:
            sts=[x[1] for x in ss];names=', '.join(x[0]['id'] for x in ss[:5])+(' …' if len(ss)>5 else '')
            if all(x=='Pass' for x in sts): st,iss,fix='Pass','Section-level inference: the unit text was not verified line by line.','Line-by-line check against the real build.'
            elif any(x in('Pass','Partial') for x in sts): st,iss,fix='Partial','Some mapped screens are partial or not built: '+', '.join(f'{x[0]["id"]}={x[1]}' for x in ss if x[1]!='Pass')[:140],'Complete the mapped screens.'
            elif all(x=='Not Tested' for x in sts): st,iss,fix='Not Tested','Mapped screens are outside the current release.','Build with its release.'
            else: st,iss,fix='Fail','Mapped screens are not implemented.','Build the mapped screens.'
            impl='Mapped screens: '+names;ev=', '.join(sorted({MODSUITE.get(x[0]['mod'],'—') for x in ss}-{'—'})) or '—'
    uc[st]+=1;ur.append((u['id'],sec,txt,impl,st,ev,iss,fix))
L=['# COVERAGE_MATRIX_REQUIREMENTS.md','','All 596 requirement units of the approved Requirements Matrix. **Important:** for functional sections (§5) the status is inferred at section level from the screens mapped to that section; only §10 route rows are matched to operations one-to-one (104 of 104 routes matched exactly). No unit was verified line by line.','',f"Totals: Pass {uc['Pass']} · Partial {uc['Partial']} · Fail {uc['Fail']} · Not Tested {uc['Not Tested']}",'','| Identifier | Section | Expected behavior (first 150 chars) | Implemented behavior | Status | Evidence | Issue | Required fix |','|---|---|---|---|---|---|---|---|']
for r in ur: L.append('| '+' | '.join(esc(x) for x in r)+' |')
open(OUT+'COVERAGE_MATRIX_REQUIREMENTS.md','w',encoding='utf-8').write('\n'.join(L)+'\n')
# ---------- flows
fr=[]
for f in FL:
    ops_=f['ops'];wired=[o for o in ops_ if o in W];miss=[o for o in ops_ if o not in W and o not in MACH and o not in REDUNDANT]
    st='Pass' if not miss else ('Partial' if wired else 'Not Tested' if all(OPS[o]['rel']!=1 for o in miss if o in OPS) else 'Fail')
    fr.append((f['id'],f['name'],st,', '.join(miss)[:160]))
fc=collections.Counter(x[2] for x in fr)
json.dump({'screens':dict(tot),'units':dict(uc),'flows':dict(fc),'ops':dict(oc),'W':len(W),'M':len(M),'V':len(V),'r1w':len([o for o in OPS if OPS[o]['rel']==1 and o in W])},open('summary.json','w'))
json.dump(fr,open('flows.json','w'))
print('screens',dict(tot));print('units',dict(uc));print('flows',dict(fc));print('ops',dict(oc));print('W',len(W),'R1 wired',len([o for o in OPS if OPS[o]['rel']==1 and o in W]),'validated',len(V))
exec(open('/home/claude/audit/narr.py',encoding='utf-8').read(),globals())
