import { createRequire } from 'module'; import fs from 'fs';
const [deps,root]=process.argv.slice(2); const y=createRequire(deps+'/package.json')('js-yaml');
const d=y.load(fs.readFileSync(root+'/openapi.yaml','utf8')); const s=d.components.schemas; const METH=['get','post','put','patch','delete'];
const MODS=['identity-org','time-leave','workflow-store','complaints-evaluation','hr-finance-system']; const mods=Object.fromEntries(MODS.map(m=>[m,y.load(fs.readFileSync(`${root}/modules/${m}.yaml`,'utf8'))]));
const idx=()=>{const o={};for(const[p,i] of Object.entries(d.paths))for(const k of METH)if(i[k])o[i[k].operationId]={p,k,op:i[k]};return o;}; let ops=idx(); const miss=[];
const O=id=>{const e=ops[id]; if(!e){miss.push(id);return null;} return e.op;};
const note=(ids,t)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o.description=(o.description||'')+'\n\n'+t;});
const codes=(ids,cs)=>[].concat(ids).forEach(i=>{const o=O(i); if(!o)return; o['x-error-codes']=[...new Set([...(o['x-error-codes']||[]),...cs])]; o.responses['422']??={$ref:'#/components/responses/BusinessRuleViolation'};});
const release=(ids,r)=>ids.forEach(i=>{const o=O(i); if(o)o['x-release']=r;});
const addRoles=(ids,rs)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o['x-allowed-roles']=[...new Set([...o['x-allowed-roles'],...rs])];});
const delRoles=(ids,rs)=>[].concat(ids).forEach(i=>{const o=O(i); if(o)o['x-allowed-roles']=o['x-allowed-roles'].filter(r=>!rs.includes(r));});
const remove=id=>{const e=ops[id]; if(!e){miss.push(id);return;} delete d.paths[e.p][e.k]; if(!METH.some(k=>d.paths[e.p][k])) delete d.paths[e.p]; delete ops[id];};

// root rules (COV-1, COV-4, INT-4, INT-49)
d['x-global-rules']=mods['identity-org']['x-global-rules'];
d['x-module-rules']=Object.fromEntries(MODS.map(m=>[m,mods[m].info.description]));
d.info.description='عقد API لنظام الإدارة والموارد البشرية، مأخوذ من متطلبات_وتصميم_النظام.md. راجع README.md وCONVENTIONS.md.\n\n'+mods['workflow-store'].info.description;
d['x-oauth-client']={clientId:'HR_Mangment2_Flutter',type:'public (no secret, PKCE required)',scopes:['openid','profile','offline_access','HR_Mangment2'],redirectUris:{android:'com.hrsystem.app://oauth/callback',ios:'com.hrsystem.app://oauth/callback',web:'https://localhost:44382/auth/callback.html'},note:'Seed the same values in OpenIddict. Replace the web and production URLs per environment.'};
d['x-server-requirements']={
 enums:'Register new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) in MVC JSON and ABP JSON options.',
 dates:'Use DateOnly for format date. Use a custom HH:mm converter for TimeOnly. Set AbpClockOptions.Kind = Utc so date-time values end in Z.',
 errorStatus:'Use a custom IHttpExceptionStatusCodeFinder: codes starting with Hr: return 422, except these which return 403: Hr:Identity:PasswordChangeRequired, Hr:Identity:MfaEnrollmentRequired. ABP defaults business exceptions to 403.',
 errorBody:'error.details is null in production. Complaint operations send no error.data. validationErrors[].members are camelCase JSON paths. A 401 from the JWT challenge may have an empty body unless a handler writes the envelope; clients must tolerate both.',
 paging:'Set default MaxResultCount = 20 and maximum = 200 at startup; ABP defaults differ.',
 routing:'Use explicit [Route], [Http*] and status results to match this file. All uuid path parameters use the :guid constraint, which removes the ambiguity linters report between /my/{id} and /{id}/action. Verify with a test comparing the running route table to this file.',
 multipart:'Form values: invariant decimals, ISO dates, omit null parts, repeat the attachments part per file. ABP DTOs with IRemoteStreamContent need FormBodyBindingIgnoredTypes configured.',
 downloads:'A FileDownloadUrlDto.url is pre-signed: the client calls it with no Authorization header. Binary responses send Content-Disposition with a filename.',
 language:'Supported cultures are ar and en, default ar. Unknown Accept-Language values fall back to ar. Clients set the header in one interceptor.',
 authorization:'x-permission is the server check; x-allowed-roles is the default role seed and the UI contract. Complaint access additionally uses ComplaintAccessPolicy. x-permission null means authenticated with no permission check.',
 nullableReferences:'allOf with a sibling nullable is the deliberate form for nullable references. Generators must treat these as nullable; prove it with a generation smoke test. Linters reject null examples for this form; that warning class is accepted.',
 clients:'Use the full operationId as the generated method name.'};

// common
d.components.parameters.IdempotencyKey.description='UUID من العميل. النطاق لكل مستخدم وعملية، والمدة 24 ساعة. لا يحفظ المخزن إلا المفتاح وبصمة الطلب ومعرف المورد، ولا يحفظ أي محتوى. التكرار يعيد قراءة المورد عبر فحص الصلاحية المعتاد. المفتاح نفسه بمحتوى مختلف يرجع 422 بالكود Hr:Common:IdempotencyKeyReused.';
d.components.responses.TooManyRequests={description:'تجاوز حد المحاولات. يتضمن الرد ترويسة Retry-After.',headers:{'Retry-After':{schema:{type:'integer'}}}};

// SEC-1, COV-2, SEC-4
const PROT='**قواعد الأدوار المحمية:** تغيير من يحمل hrManager أو executiveDirector لا ينفذه إلا المدير التنفيذي، ودور executiveDirector نفسه لا يتغير إلا بإجراء موثق خارج النظام (ProtectedRoleChangeRequiresApproval). لا يغير أحد أدوار نفسه (CannotChangeOwnRoles). تُرفض التركيبات: executiveDirector مع reviewer، وdepartmentManager أو storeOfficer مع accountant (ForbiddenRoleCombination). الحصول على الأدوار لا يعطي وصولًا للشكاوى إلا حسب مسارات القسم 5.8 لحاملي hrManager وexecutiveDirector، وغيرهم عبر تعيين معتمد فقط. من حمل systemAdmin يبقى ممنوعًا من الشكاوى ما لم يوافق المدير التنفيذي.';
note(['UserAccount_SetRoles','UserAccount_Create','Employee_Create'],PROT);
codes(['UserAccount_SetRoles','UserAccount_Create','Employee_Create'],['Hr:Identity:ProtectedRoleChangeRequiresApproval','Hr:Identity:ForbiddenRoleCombination']); codes('UserAccount_SetRoles',['Hr:Identity:CannotChangeOwnRoles']);

// SEC-2
s.PasswordResetDelivery={type:'string',enum:['email']};
s.PasswordResetResultDto={type:'object',required:['userId','deliveredVia','expiresAt'],properties:{userId:{type:'string',format:'uuid'},deliveredVia:{$ref:'#/components/schemas/PasswordResetDelivery'},expiresAt:{type:'string',format:'date-time'}}};
{const o=O('UserAccount_ResetPassword'); o.description='يرسل رابطًا لمرة واحدة وبمدة محددة إلى بريد صاحب الحساب مباشرة، ولا يرى منفذ العملية أي كلمة سر. يُسجل في التدقيق، ويُشعر صاحب الحساب. لا يعيد أحد تعيين كلمة سره بنفسه عبر هذا المسار، ويعيد المدير التنفيذي تعيين كلمة سر مدير الموارد البشرية. لا يُسمح بتغيير بريد الموظف عبر Employee_Update أثناء صلاحية الرابط.';
 o.responses['200']={description:'أُرسل الرابط.',content:{'application/json':{schema:{$ref:'#/components/schemas/PasswordResetResultDto'},example:{userId:'3f2c1a9e-0000-4000-8000-000000000001',deliveredVia:'email',expiresAt:'2026-11-05T10:00:00Z'}}}}; codes('UserAccount_ResetPassword',['Hr:Identity:NoVerifiedEmail']);}

// SEC-3
note(['UserAccount_UpdatePermissions','Role_UpdatePermissions'],'**صلاحيات محمية:** تُخفى عند القراءة وتُرفض عند الكتابة: Hr.Complaint*, Hr.Finance*, Hr.Evaluations.HrRead, Hr.Evaluations.InternalNote.*, Hr.Reports.Review, Hr.Warning.Read, Hr.Identity.*. ولا يعدل أحد صلاحيات نفسه.');
codes(['UserAccount_UpdatePermissions','Role_UpdatePermissions'],['Hr:Identity:ProtectedPermission']); codes('UserAccount_UpdatePermissions',['Hr:Identity:CannotChangeOwnPermissions']);
note('Report_GetReviewDashboard','يرجع 403 لأي مستخدم يحمل executiveDirector، مهما كانت أدواره الأخرى.');

// SEC-6, SEC-7, COV-6
delRoles(['ComplaintHandler_GetList','ComplaintHandler_Request'],['systemAdmin']);
codes(['ComplaintHandler_Request','ComplaintHandler_Approve'],['Hr:Complaints:HandlerRoleNotAllowed']);
note(['ComplaintHandler_Request','ComplaintHandler_Approve'],'يُرفض تعيين reviewer بأي مستوى، وdepartmentManager في confidential فأعلى، وexecutiveDirector في normal، ويُعاد الفحص عند كل وصول.');
['Complaint_GetAssignedList','Complaint_GetDetail','Complaint_AddAction','Complaint_Close','Complaint_GetAccessLog','Complaint_UploadAttachment','Complaint_DownloadAttachment','Complaint_RaiseLevel'].forEach(i=>{const o=O(i); if(o)o['x-complaint-handler-assignee']=true;});
d['x-global-rules']['complaint-handler-assignee']='العمليات التي تحمل x-complaint-handler-assignee: true متاحة أيضًا لصاحب تعيين معتمد ساري، أيًا كان دوره، ضمن قيود ComplaintAccessPolicy.';

// SEC-8
note(['Complaint_DownloadAttachment','LeaveRequest_GetAttachmentDownloadUrl','Warning_GetAttachmentDownloadUrl'],'يجب أن يتبع attachmentId المورد id في الاستعلام نفسه الذي يفحص الصلاحية، وإلا 404.');

// SEC-10
const scopeMap=o=>Object.fromEntries(o['x-allowed-roles'].map(r=>[r,r==='employee'?'self':r==='departmentManager'?'team':'company']));
['EmployeeDocument_List','EmployeeDocument_GetDownloadUrl','Attendance_GetByEmployee','Attendance_GetDayDetail','LeaveBalance_GetByEmployee','LeaveLedger_GetByEmployee','LeaveRequest_ListByEmployee','Request_GetList','CustodyCount_GetList','Warning_GetEmployeeFile','AppreciationLetter_GetEmployeeFile','AppreciationLetter_Get','AppreciationLetter_GetDownloadUrl','EvaluationAnnualSummary_GetForEmployee','DepartmentReport_GetList','DepartmentReport_Get','Report_Export','Circular_Create','Warning_Create','Memo_Forward'].forEach(i=>{const o=O(i); if(o)o['x-data-scope-by-role']=scopeMap(o);});
['Complaint_RaiseLevel','Complaint_UploadAttachment','Complaint_DownloadAttachment'].forEach(i=>{const o=O(i); if(o)o['x-data-scope-by-role']={...scopeMap(o),employee:'self'};});

// SEC-11
codes(['LeaveBalance_Adjust','LeaveBalance_SetEntitlement'],['Hr:Leave:SelfAdjustmentNotAllowed']); codes(['SalaryProfile_Update','Adjustment_Create','AdjustmentSuggestion_Approve','AdjustmentSuggestion_Reject','Adjustment_Reverse'],['Hr:Finance:SelfChangeNotAllowed']);
note(['LeaveBalance_Adjust','LeaveBalance_SetEntitlement','SalaryProfile_Update','Adjustment_Create'],'لا يعدل أحد بياناته بنفسه. يُحال التغيير إلى المدير التنفيذي.');

// SEC-12, INT-14
const up=['Complaint_UploadAttachment','Warning_Create','AppreciationLetter_Create','Adjustment_Create','Expense_Create']; note(up,'تطبق حدود الإعدادين Hr.Document.MaxFileSizeBytes وHr.Document.AllowedContentTypes.'); codes(up,['Hr:Document:FileTooLarge','Hr:Document:FileTypeNotAllowed']);

// SEC-13
note(['Employee_ChangeManager','Manager_AssignEmployees','Complaint_GetAssignedList'],'عند تغيير المدير: تذهب الشكوى العادية المعلقة إلى الموارد البشرية مباشرة، ويفقد المدير القديم وصوله فورًا. movedPendingItemCount لا يحسب الشكاوى أبدًا. [افتراض بانتظار تأكيد المالك]');

// SEC-15, INT-7
for(const e of Object.values(ops)) if((e.op.tags||[])[0]==='Complaints'&&!e.op.responses['403']){ e.op.responses['403']={$ref:'#/components/responses/Forbidden'}; e.op.description+='\n\nيُستخدم 403 هنا فقط لبوابات الحساب (تغيير كلمة السر والتحقق بخطوتين) قبل أي بحث عن الشكوى. فشل الصلاحية على الشكوى يرجع 404 دائمًا.'; }

// SEC-24
['ConnectToken_Issue','Account_ChangePassword','Mfa_ConfirmEnrollment','Mfa_RegenerateRecoveryCodes','AttendanceDevice_IngestPunches'].forEach(i=>{const o=O(i); if(o)o.responses['429']={$ref:'#/components/responses/TooManyRequests'};});

// notifications: INT-5, INT-6, INT-29, SEC-16, COV-9, COV-22
{const n=s.NotificationTemplateCode; let t=n.description;
 const row=(c,r)=>{t=t.replace(new RegExp(`\\| ${c} \\|.*\\n`),r+'\n');};
 row('evaluationPublished','| evaluationPublished | evaluation | الموظف المُقيَّم | GET /api/app/evaluation/my/{resourceId} |');
 row('evaluationResponseReceived','| evaluationResponseReceived | evaluation | مدير الموارد البشرية | GET /api/app/evaluation/{resourceId}/hr-view |');
 row('evaluationObjectionDecided','| evaluationObjectionDecided | evaluation | الموظف المُقيَّم، ومديره | الموظف: GET /api/app/evaluation/my/{resourceId}. المدير: GET /api/app/evaluation/{resourceId} |');
 row('complaintUpdate','| secureItemUpdateForOwner | secureItem | مقدم الشكوى؛ نصه دائمًا «لديك تحديث جديد» | GET /api/app/complaint/my/{resourceId} |\n| secureItemUpdateForHandler | secureItem | معالج الشكوى؛ نصه دائمًا «لديك تحديث جديد» | GET /api/app/complaint/{resourceId}، و404 لغير المخول |\n| secureHandlerAssignmentUpdate | none | طرفا طلب تعيين معالج، دون مستوى أو اسم | GET /api/app/complaint-handler |');
 const dest=(c,route)=>{t=t.replace(new RegExp(`\\| ${c} \\|([^|]*)\\|([^|]*)\\|.*\\n`),`| ${c} |$1|$2| ${route} |\n`);};
 dest('memoUpdate','GET /api/app/memo/{resourceId}'); dest('incidentUpdate','GET /api/app/incident/{resourceId}'); dest('stockIssuePending','GET /api/app/stock-issue/{resourceId}'); dest('custodyAssigned','GET /api/app/custody/{resourceId}'); dest('circularReceived','GET /api/app/circular/{resourceId}');
 for(const c of ['monthEndCycleOpened','monthlyCycleReminder','monthlyCycleEscalated']) t=t.replace(new RegExp(`\\| ${c} \\|[^|]*\\|([^|]*)\\|.*\\n`),`| ${c} | none |$1| GET /api/app/evaluation-cycle/current، وresourceId فارغ |\n`);
 t+='\n| delegationUpdated | none | طرفا التفويض | شاشة التفويضات |\n| managerChanged | none | الموظف ومديراه | شاشة الملف الشخصي |\n| leaveBalanceAdjusted | none | صاحب الرصيد | GET /api/app/leave-balance/my |\n| accountSecurityChanged | none | صاحب الحساب | شاشة الإشعارات فقط |\n| custodyReturned | custody | مستلم العهدة | GET /api/app/custody/{resourceId} |\n| custodyCountDifference | none | مدير الموارد البشرية | شاشة الجرد |';
 n.description=t; n.enum=[...n.enum.filter(v=>v!=='complaintUpdate'),'secureItemUpdateForOwner','secureItemUpdateForHandler','secureHandlerAssignmentUpdate','delegationUpdated','managerChanged','leaveBalanceAdjusted','accountSecurityChanged','custodyReturned','custodyCountDifference'];
 const rt=Object.entries(s).find(([k,v])=>/Notification.*Resource.*Type/.test(k)&&v.enum); if(rt){rt[1].enum=[...new Set(rt[1].enum.filter(v=>v!=='complaint'&&v!=='evaluationCycle').concat(['secureItem','none']))];}
}

// gender COV-3
s.AppGender={type:'string',enum:['male','female'],description:'يُستخدم فقط لأهلية أنواع الإجازات. لا يظهر للمدير أو في EmployeeRefDto.'};
['EmployeeCreateDto','EmployeeUpdateDto','EmployeeHrDto','EmployeeSelfDto'].forEach(n=>{s[n].properties.gender={allOf:[{$ref:'#/components/schemas/AppGender'}],nullable:true,type:'string'};});

// releases
release(['Report_GetHrAttendance','Report_GetHrLeaveBalances','Report_GetHrRequests','Me_GetInboxSummary','Me_GetAdministrativeTaskLog','Employee_GetAdministrativeTaskLog','Report_GetTeamCustody','Report_GetTeamEvaluations','Report_GetEvaluationConsistency'],1);
note(['Report_GetTeamCustody','Report_GetTeamEvaluations','Report_GetEvaluationConsistency'],'يرجع قائمة فارغة إلى أن تُطلق وحدته في الإصدار 2.');
note(['Incident_Create','Request_Submit'],'في الإصدار 1 يكون itemId فارغًا، وتبقى أنواع طلبات المخزن غير مفعلة حتى الإصدار 2.');

// duplicates COV-12, INT-8, INT-9
['SystemSettings_GetAttendance','SystemSettings_UpdateAttendance','NotificationPreference_Get','NotificationPreference_Update'].forEach(remove);
if(s.SystemSettingsSectionCode) s.SystemSettingsSectionCode.description=(s.SystemSettingsSectionCode.description||'')+'\nقسم attendance يُدار عبر /api/app/attendance-settings.';

// other coverage
addRoles(['Delegation_Create','Delegation_Cancel','Delegation_GetCandidates'],['accountant']); note('Delegation_Create','يفوض المحاسب خطوة طلبات المحاسب فقط (scope = requests)، دون وصول لشاشات المالية.');
{const o=O('Memo_GetInbox'); if(o){o.parameters.push({name:'employeeId',in:'query',required:false,schema:{type:'string',format:'uuid'},description:'يعرض كل مطالعات موظف لملفه، لمدير الموارد البشرية ومديره المباشر فقط، بكل الحالات.'});}}
note('DepartmentReport_Get','بيانات الحضور والإجازات والعهد والتعاميم ثابتة بعد التثبيت. أما حقول التقييم فتُحسب عند القراءة وتثبت عند DepartmentReport_Submit.');
for(const n of ['ComplaintHandlerSummaryDto','ComplaintHandlerDetailDto']) if(s[n]?.properties?.dueAt){s[n].properties.dueAt.nullable=true; s[n].properties.dueAt.description='فارغ إلى أن يحدد مسؤول النظام مهلة الشكوى.';}
if(s.AccountantRequestDto){delete s.AccountantRequestDto.properties.attachments; s.AccountantRequestDto.required=s.AccountantRequestDto.required.filter(x=>x!=='attachments');}
addRoles(['LeaveBalance_GetByEmployee','LeaveLedger_GetByEmployee'],['systemAdmin','executiveDirector']); note(['LeaveBalance_GetByEmployee','LeaveLedger_GetByEmployee'],'مسؤول النظام والمدير التنفيذي: فقط لمقدم طلب عند خطوتهما الحالية، وإلا 404.');
note('LeaveBalance_GetMy','عند nextYearStartDate تعمل مهمة يومية آمنة عند التكرار، فترحّل كل المتبقي دون حد بحركة carryIn. مثال 5.4: 24 − 18 = 6 مرحّلة، ومع 21 جديدة يصبح الرصيد 27.');

// answered questions
const ans={UserAccount_SetRoles:'«العدد الحالي» ليس حدًا ملزمًا للمراجع والمحاسب.',EmployeeShift_Assign:'المدير التنفيذي غير مشمول بالحضور (البند 5 في القسم 13).',WeeklyOffSetting_Get:'التقويم للمواعيد الإدارية، والوردية تحدد الحضور وأيام الإجازة.',Attendance_GetMy:'المدير التنفيذي غير مشمول بالحضور.',LeaveBalance_SetEntitlement:'تطبق القيمة الأولى على السنة الحالية (مثال 5.4).',LeaveBalance_Adjust:'الإضافة أيام إلى الرصيد (5.4).',Report_GetEvaluationConsistency:'للمراجع فقط في النسخة الأولى (5.11).',Adjustment_Create:'المرفق مطلوب للإدخال اليدوي (5.16).',Payslip_GetMyList:'مدير القسم يرى قسيمته عبر دور الموظف (5.16).'};
for(const[i,t] of Object.entries(ans)){const o=O(i); if(o){delete o['x-open-question']; o.description+='\n\n'+t;}}
{const o=O('AttendanceSettings_Get'); if(o)o['x-open-question']='ما توقيت الشركة الرسمي الذي تُحسب به أوقات الحضور؟';}

// integration
d.paths['/api/app/user-account/lookup']={get:{operationId:'UserAccount_Lookup',tags:['UserAccounts'],summary:'Look up user accounts by name',description:'يعطي المعرفات اللازمة لإعادة ضبط الحسابات والتحقق بخطوتين وتصفية سجل التدقيق، دون أي بيانات أخرى.','x-source':['3'],'x-release':1,'x-allowed-roles':['executiveDirector','hrManager','systemAdmin'],'x-permission':'Hr.Identity.UserAccount.Lookup','x-data-scope':'company','x-error-codes':[],'x-added-route':'الحصول على userId للعمليات الإدارية على الحساب',parameters:[{name:'search',in:'query',required:false,schema:{type:'string'}},{$ref:'#/components/parameters/SkipCount'},{$ref:'#/components/parameters/MaxResultCount'},{$ref:'#/components/parameters/AcceptLanguage'}],responses:{200:{description:'OK',content:{'application/json':{schema:{$ref:'#/components/schemas/UserRefPagedResultDto'},example:{totalCount:1,items:[{id:'3f2c1a9e-0000-4000-8000-000000000001',displayName:'موظف تجريبي'}]}}}},400:{$ref:'#/components/responses/BadRequest'},401:{$ref:'#/components/responses/Unauthorized'},403:{$ref:'#/components/responses/Forbidden'}}}};
s.UserRefPagedResultDto={type:'object',required:['totalCount','items'],properties:{totalCount:{type:'integer',format:'int64'},items:{type:'array',items:{$ref:'#/components/schemas/UserRefDto'}}}};
{const cur=O('EvaluationCycle_GetCurrent'); d.paths['/api/app/evaluation-cycle']={get:{operationId:'EvaluationCycle_GetList',tags:cur.tags,summary:'List evaluation cycles',description:'يعطي معرفات الدورات السابقة لتصفية التقارير والتقييمات.','x-source':['5.10'],'x-release':2,'x-allowed-roles':cur['x-allowed-roles'],'x-permission':cur['x-permission'],'x-data-scope':cur['x-data-scope'],'x-error-codes':[],'x-added-route':'اختيار دورة سابقة في التقارير',parameters:[{name:'year',in:'query',required:false,schema:{type:'integer',format:'int32'}},{$ref:'#/components/parameters/SkipCount'},{$ref:'#/components/parameters/MaxResultCount'},{$ref:'#/components/parameters/Sorting'},{$ref:'#/components/parameters/AcceptLanguage'}],responses:{200:{description:'OK',content:{'application/json':{schema:{$ref:'#/components/schemas/EvaluationCyclePagedResultDto'}}}},400:{$ref:'#/components/responses/BadRequest'},401:{$ref:'#/components/responses/Unauthorized'},403:{$ref:'#/components/responses/Forbidden'}}}};
 s.EvaluationCyclePagedResultDto={type:'object',required:['totalCount','items'],properties:{totalCount:{type:'integer',format:'int64'},items:{type:'array',items:{$ref:'#/components/schemas/EvaluationCycleDto'}}}};}
addRoles('Employee_Lookup',['systemAdmin']);
s.MemoStatus.enum=[...new Set([...s.MemoStatus.enum,'pendingExecutive'])]; s.MemoStatus.description+=' pendingExecutive: عند المدير التنفيذي.';
for(const n of ['ManagerChangeResultDto','ComplaintTransitionResultDto']){s[n].properties.concurrencyStamp={type:'string'}; s[n].required.push('concurrencyStamp');}
if(s.AttendancePeriodSummaryDto?.properties?.leaveDays) s.AttendancePeriodSummaryDto.properties.leaveDays={type:'number',format:'decimal'};
['Complaint_Submit','Complaint_AddAction','Complaint_UploadAttachment','ComplaintHandler_Request','Evaluation_AddResponse','EvaluationInternalNote_Add','EvaluationCriterion_Create','EvaluationGrade_Create','Circular_Create'].forEach(i=>{const o=O(i); if(o&&o.responses['200']&&!o.responses['201']){o.responses['201']=o.responses['200']; delete o.responses['200'];}});
['EmployeeShift_GetHistory','DeviceUserMap_ListUnmapped','GeoFence_List'].forEach(i=>{const o=O(i); if(o)o.responses['400']??={$ref:'#/components/responses/BadRequest'};});
if(s.IncidentStatusUpdateInputDto?.properties?.status?.enum){s.IncidentStatusTarget={type:'string',enum:s.IncidentStatusUpdateInputDto.properties.status.enum}; s.IncidentStatusUpdateInputDto.properties.status={$ref:'#/components/schemas/IncidentStatusTarget'};}
{const c=O('EmployeeDocument_Upload')?.requestBody?.content?.['multipart/form-data']; if(c?.example)c.example.file='(binary)';}
for(const e of Object.values(ops)) if(e.op['x-permission']==='none') e.op['x-permission']=null;
ops=idx();

// COV-18
for(const i of 'Request_Submit Request_GetMy Request_GetInbox Request_Get Request_Approve Request_Reject Memo_Create Memo_Convert Incident_Create Stock_Receive Complaint_GetRespondents Complaint_PreviewRouting Complaint_Submit Complaint_GetMyList Complaint_GetAssignedList Complaint_GetDetail Complaint_RaiseLevel Complaint_AddAction Complaint_Close Complaint_GetAccessLog ComplaintHandler_Request ComplaintHandler_Approve EvaluationCycle_GetCurrent Evaluation_GetManagerForm Evaluation_SaveDraft Evaluation_Submit Evaluation_Acknowledge Evaluation_AddResponse Evaluation_DecideObjection DepartmentReport_Get DepartmentReport_UpdateNotes DepartmentReport_Submit Report_GetTeamAttendance Report_GetTeamLeaves Report_GetTeamPending Report_GetReviewDashboard Report_GetEvaluationConsistency Report_Export Circular_GetReceivedList Circular_Create Circular_Acknowledge'.split(' ')){const o=O(i); if(o)o['x-source']=[...new Set([...o['x-source'].map(String),'10'])];}

// text cleanup
const fix=v=>v.replace(/(hr-finance-system|identity-org|workflow-store|complaints-evaluation|time-leave)\.yaml/g,'هذا العقد').replace(/\bcomplaintUpdate\b/g,'secureItemUpdateForOwner').replace(/\+96477\d{8}/g,'+964000000000').replace(/api-contract\/README\.md/g,'README.md');
(function w(o){ if(Array.isArray(o)){o.forEach((v,i)=>{ if(typeof v==='string') o[i]=fix(v); else w(v);}); return;} if(o&&typeof o==='object') for(const[k,v] of Object.entries(o)){ if(k==='enum') continue; if(typeof v==='string') o[k]=fix(v); else w(v);} })(d);

// examples for the new concurrencyStamp
const stamp=o=>{ if(Array.isArray(o)) return o.forEach(stamp); if(o&&typeof o==='object'){ if(('movedPendingItemCount' in o&&'newManager' in o)||'callerRetainsAccess' in o) o.concurrencyStamp??='9f1c2e7ab34d4c10'; if('linkedAdjustmentIds' in o) delete o.attachments; Object.values(o).forEach(stamp);} };
for(const e of Object.values(ops)) for(const r of Object.values(e.op.responses)) stamp(r.content?.['application/json']?.example);

// prune unused schemas
for(let pass=0;pass<5;pass++){ const refs=new Set(); (function w(o){ if(Array.isArray(o))o.forEach(w); else if(o&&typeof o==='object'){ if(typeof o.$ref==='string')refs.add(o.$ref.split('/').pop()); Object.values(o).forEach(w);} })(d); for(const n of Object.keys(s)) if(!refs.has(n)&&n!=='NotificationPushPayloadDto'){ delete s[n]; console.log('pruned',n);} }

fs.writeFileSync(root+'/openapi.yaml',y.dump(d,{lineWidth:120,noRefs:true,sortKeys:false}));
const oq=[];let n=0;const rel={};for(const[p,i] of Object.entries(d.paths))for(const k of METH)if(i[k]){n++;rel[i[k]['x-release']]=(rel[i[k]['x-release']]||0)+1; if(i[k]['x-open-question'])oq.push({id:`${k.toUpperCase()} ${p}`,opId:i[k].operationId,q:i[k]['x-open-question']});}
fs.writeFileSync(root+'/open-questions.json',JSON.stringify(oq,null,1));
console.log('ops',n,'schemas',Object.keys(s).length,'release',JSON.stringify(rel),'open',oq.length,'missingIds',miss.join(',')||'none');
