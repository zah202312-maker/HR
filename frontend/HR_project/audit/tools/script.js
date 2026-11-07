

const T={ar:{
nav:['لوحة التحكم','الموظفون','الحضور والانصراف','الإجازات','الرواتب','الأقسام','التوظيف','تقييم الأداء','المستندات','التقارير','الإعلانات','الإعدادات'],
d:{eng:'الهندسة',ops:'العمليات',sales:'المبيعات',hr:'الموارد البشرية',fin:'المالية'},
ti:{dev:'مهندس برمجيات',mgr:'مدير',rep:'مندوب مبيعات',spec:'أخصائي',acc:'محاسب'},
st:{active:'نشط',leave:'في إجازة',inactive:'غير نشط'},lt:{ann:'سنوية',sick:'مرضية'},
greet:'صباح الخير يا علي،',greet2:'فريقك في أفضل حالاته',sub:'بيانات تجريبية — لا تمثل عمليات حقيقية',date:'الأربعاء 4 تشرين الثاني 2026',
srch:'ابحث عن موظف أو قسم…',add:'إضافة موظف',co:'شركة نكسس',kp:['الرواتب الشهرية','إجمالي الموظفين','الحاضرون اليوم','طلبات إجازة معلّقة','التعيينات الجديدة'],suf:' مليون د.ع',
gr:'نمو الموظفين',grs:'آخر 12 شهراً',at:'الحضور',w:'أسبوع',m:'شهر',dp:'توزيع الأقسام',act:'النشاط الأخير',pa:'موافقات معلّقة',pay:'تحليل الرواتب',
acts:['وافق المدير على إجازة زينب','تم تعيين Hassan Ali','اكتمل مسير رواتب تشرين الأول','3 عقود تنتهي قريباً'],
emp:'دليل الموظفين',all:'كل الأقسام',nm:'الاسم',dpt:'القسم',tt:'المسمى',stt:'الحالة',id:'الرقم',none:'لا توجد نتائج',none2:'جرّب بحثاً أو فلتراً مختلفاً',
save:'حفظ',cancel:'إلغاء',req:'هذا الحقل مطلوب',newe:'موظف جديد',added:'تمت إضافة الموظف (تجريبي)',del:'حذف',deld:'تم حذف الموظف (تجريبي)',
lv:'إدارة الإجازات',bal:['سنوية','مرضية','بدون راتب'],ap:'موافقة',rj:'رفض',ap2:'موافق عليه',rj2:'مرفوض',pe:'معلّق',days:'أيام',okA:'تمت الموافقة على الطلب',okR:'تم رفض الطلب',
soon:'هذه الشاشة قيد التصميم',soon2:'لم تُبنَ بعد في هذه النسخة. الشاشات المتاحة: لوحة التحكم، الموظفون، الإجازات.',back:'العودة للوحة التحكم',
col:'طي القائمة',menu:'القائمة',role:'مدير الموارد البشرية',nt:'الإشعارات',nts:['طلب إجازة جديد من سارة','انتهاء عقد قريباً','اكتمل مسير الرواتب'],
b:'الأساسي',bo:'المكافآت',ov:'الإضافي',de:'الاستقطاعات',nme:'الاسم الكامل',ttl:'المسمى الوظيفي'},
en:{
nav:['Dashboard','Employees','Attendance','Leave','Payroll','Departments','Recruitment','Performance','Documents','Reports','Announcements','Settings'],
d:{eng:'Engineering',ops:'Operations',sales:'Sales',hr:'Human Resources',fin:'Finance'},
ti:{dev:'Software Engineer',mgr:'Manager',rep:'Sales Rep',spec:'Specialist',acc:'Accountant'},
st:{active:'Active',leave:'On leave',inactive:'Inactive'},lt:{ann:'Annual',sick:'Sick'},
greet:'Good morning, Ali —',greet2:'your team is at its best',sub:'Demo data — not real operations',date:'Wednesday, 4 November 2026',
srch:'Search people or departments…',add:'Add employee',co:'Nexus Co.',kp:['Monthly payroll','Total employees','Present today','Pending leave','New hires'],suf:'M IQD',
gr:'Headcount growth',grs:'Last 12 months',at:'Attendance',w:'Week',m:'Month',dp:'Departments',act:'Recent activity',pa:'Pending approvals',pay:'Payroll breakdown',
acts:['Manager approved Zaynab’s leave','Hassan Ali was hired','October payroll completed','3 contracts expire soon'],
emp:'Employee directory',all:'All departments',nm:'Name',dpt:'Department',tt:'Title',stt:'Status',id:'ID',none:'No results',none2:'Try a different search or filter',
save:'Save',cancel:'Cancel',req:'This field is required',newe:'New employee',added:'Employee added (demo)',del:'Delete',deld:'Employee deleted (demo)',
lv:'Leave management',bal:['Annual','Sick','Unpaid'],ap:'Approve',rj:'Reject',ap2:'Approved',rj2:'Rejected',pe:'Pending',days:'days',okA:'Request approved',okR:'Request rejected',
soon:'This screen is still in design',soon2:'Not built in this version yet. Available screens: Dashboard, Employees, Leave.',back:'Back to dashboard',
col:'Collapse',menu:'Menu',role:'HR Director',nt:'Notifications',nts:['New leave request from Sara','Contract expiring soon','Payroll run completed'],
b:'Base',bo:'Bonuses',ov:'Overtime',de:'Deductions',nme:'Full name',ttl:'Job title'}};
const IC=['M3 3h7v9H3zM14 3h7v5h-7zM14 12h7v9h-7zM3 16h7v5H3z','M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 3a4 4 0 1 0 0 8 4 4 0 0 0 0-8M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75','M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20M12 6v6l4 2','M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2M16 2v4M8 2v4M3 10h18','M4 6h16a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2M12 10a2 2 0 1 0 0 4 2 2 0 0 0 0-4','M6 2h12a2 2 0 0 1 2 2v18H4V4a2 2 0 0 1 2-2M9 22v-4h6v4M8 6h.01M12 6h.01M16 6h.01M8 10h.01M12 10h.01M16 10h.01','M4 7h16a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2','M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20M12 6a6 6 0 1 0 0 12 6 6 0 0 0 0-12M12 10a2 2 0 1 0 0 4 2 2 0 0 0 0-4','M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7zM14 2v4a2 2 0 0 0 2 2h4','M3 3v18h18M18 17V9M13 17V5M8 17v-3','m3 11 18-5v12L3 14zM11.6 16.8a3 3 0 1 1-5.8-1.6','M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6'];
const AV=['#00C49F','#3B82F6','#10B981','#F59E0B','#14B8A6'];
const PAGES=['dash','emp','att','leave','pay','dept','rec','perf','doc','rep','ann','set'];
const T2={ar:{
aTitle:'تسجيل الدخول إلى HR',aSub:'نظام إدارة الموارد البشرية',aBtn:'متابعة إلى تسجيل الدخول',
mockT:'جلسة تجريبية (لا يوجد خادم)',mockD:'تحاكي الدخول عبر Authorization Code + PKCE. اختر مستخدماً تجريبياً لتجربة حواجز الوصول.',
pl:{emp:['موظف','حساب عادي'],mgr:['مدير القسم','يرى سجل المهام الإداري'],hr:['مدير الموارد البشرية','التحقق بخطوتين مفعّل'],acc:['المحاسب','يحتاج تفعيل التحقق بخطوتين'],nw:['موظف جديد','يحتاج تغيير كلمة السر']},
pwT:'تغيير كلمة السر',pwF:'تغيير كلمة السر إلزامي قبل المتابعة.',pwCur:'كلمة السر الحالية',pwNew:'كلمة السر الجديدة',pwConf:'تأكيد كلمة السر الجديدة',pwMis:'التأكيد لا يطابق كلمة السر الجديدة',pwRq:'هذا الحقل مطلوب',pwHint:'سياسة كلمات السر يحددها الخادم وتظهر عند الرفض.',pwSave:'حفظ ومتابعة',pwMock:'تجريبي: كلمة السر الحالية هي Temp#2026',pwOk:'تم تغيير كلمة السر (تجريبي)',
mfaT:'تفعيل التحقق بخطوتين',mfaD:'التحقق بخطوتين إلزامي لدورك. أضف المفتاح إلى تطبيق المصادقة ثم أدخل الرمز.',mfaStart:'ابدأ التفعيل',mfaKey:'مفتاح الإعداد',qrP:'مكان رمز QR (يُولَّد من رابط المصادقة في النسخة الفعلية)',mfaCode:'رمز التحقق (6 أرقام)',mfaCf:'تأكيد',mfaMock:'تجريبي: الرمز الصحيح هو 123456',mfaBad:'أدخل 6 أرقام',
cdT:'رموز الاسترداد',cdD:'تُعرض هذه الرموز مرة واحدة فقط. احفظها في مكان آمن.',cdSv:'حفظت الرموز',cont:'متابعة',
rcT:'إعادة إصدار رموز الاسترداد',rcD:'أدخل رمز التحقق الحالي. سيبطل ذلك الرموز السابقة.',rcGo:'إصدار رموز جديدة',
meT:'ملفي',acc:'الحساب',empf:'الملف الوظيفي',sec:'الأمان',lng:'لغة الإشعارات',rls:'الأدوار',un:'اسم المستخدم',em:'البريد',drs:'الموظفون المرتبطون بي',fnm:'الاسم',dep:'القسم',jt:'المسمى',hd:'تاريخ المباشرة',mg:'المدير',shf:'الدوام الحالي',ctr:'العقد الحالي',num:'الرقم الوظيفي',gn:'الجنس',ph:'الهاتف',adr:'العنوان',dob:'تاريخ الميلاد',ro:'للقراءة فقط. التعديل عند الموارد البشرية.',
mfaS:'التحقق بخطوتين',mfaRq:'إلزامي',mfaEn:'الحالة',mfaOn:'مفعّل',mfaOff:'غير مفعّل',mfaSn:'مفعّل منذ',mfaRm:'رموز الاسترداد المتبقية',yes:'نعم',no:'لا',chpw:'تغيير كلمة السر',out:'تسجيل الخروج',outN:'يُنهي الجلسة في الواجهة فقط؛ لا توجد عملية لإلغاء الرمز في العقد.',
tl:'سجل مهامي الإدارية',tlTy:'النوع',tlAll:'كل الأنواع',tlE:'لا توجد مهام في هذه الفترة',tlTm:'الوقت',tlTi:'المهمة',prv:'السابق',nxt:'التالي',frm:'من',to:'إلى',
inb:'بانتظار إجرائي',inbT:'الإجمالي',inbD:'عبر تفويض',inbE:'لا شيء ينتظر إجراءك حالياً',inbN:'لا يشمل الشكاوى',
er:{'Hr:Identity:CurrentPasswordIncorrect':'كلمة السر الحالية غير صحيحة','Hr:Identity:PasswordPolicyViolation':'كلمة السر الجديدة لا تستوفي سياسة كلمات السر','Hr:Identity:NewPasswordSameAsCurrent':'كلمة السر الجديدة مطابقة للحالية','Hr:Identity:MfaCodeInvalid':'رمز التحقق غير صحيح','Hr:Identity:MfaAlreadyEnabled':'التحقق بخطوتين مفعّل مسبقاً','Hr:Identity:MfaEnrollmentNotStarted':'لم يبدأ التفعيل بعد','Hr:Identity:MfaNotEnabled':'التحقق بخطوتين غير مفعّل',gen:'تعذر إكمال العملية. حاول مجدداً'},
rl:{employee:'موظف',departmentManager:'مدير القسم',hrManager:'مدير الموارد البشرية',executiveDirector:'المدير التنفيذي',reviewer:'المراجع',accountant:'المحاسب',storeOfficer:'مسؤول المخزن',systemAdmin:'مسؤول النظام'},
cat:{request:'طلبات',memo:'مطالعات',incident:'بلاغات',stockIssue:'صرف من المخزن',evaluation:'تقييمات',departmentReport:'تقرير القسم',evaluationObjection:'اعتراضات على التقييم',evaluationAcknowledgement:'تأكيد اطلاع على تقييم',circularAcknowledgement:'تأكيد استلام تعميم',custodyAcknowledgement:'تأكيد استلام عهدة',warningResponse:'رد على إنذار',warningDecision:'قرار إنذار',contractDecision:'قرار عقد',appreciationLetterAcknowledgement:'تأكيد استلام كتاب شكر'},
tt:{requestApproved:'موافقة على طلب',requestRejected:'رفض طلب',requestSubmittedOnBehalf:'رفع طلب نيابة عن موظف',memoProcessed:'معالجة مطالعة',incidentForwarded:'تحويل بلاغ',evaluationSubmitted:'رفع تقييم',departmentReportSubmitted:'رفع التقرير الشهري',circularSent:'إرسال تعميم',custodyCountRecorded:'تسجيل جرد عهدة',warningRecommended:'توصية إنذار',delegationCreated:'إنشاء تفويض'},
es:{active:'نشط',suspended:'موقوف',ended:'منتهي'},gd:{male:'ذكر',female:'أنثى'}},
en:{
aTitle:'Sign in to HR',aSub:'Human Resources Management System',aBtn:'Continue to sign in',
mockT:'Mock session (no backend)',mockD:'Simulates Authorization Code + PKCE sign-in. Pick a mock user to try the access gates.',
pl:{emp:['Employee','Regular account'],mgr:['Department manager','Sees administrative task log'],hr:['HR manager','MFA already enabled'],acc:['Accountant','Must enroll in MFA'],nw:['New employee','Must change password']},
pwT:'Change password',pwF:'You must change your password before continuing.',pwCur:'Current password',pwNew:'New password',pwConf:'Confirm new password',pwMis:'Confirmation does not match the new password',pwRq:'This field is required',pwHint:'The password policy is set by the server and shown when a password is rejected.',pwSave:'Save and continue',pwMock:'Mock: the current password is Temp#2026',pwOk:'Password changed (mock)',
mfaT:'Set up two-step verification',mfaD:'Two-step verification is mandatory for your role. Add the key to your authenticator app, then enter the code.',mfaStart:'Start setup',mfaKey:'Setup key',qrP:'QR code placeholder (generated from the authenticator URI in the live version)',mfaCode:'Verification code (6 digits)',mfaCf:'Confirm',mfaMock:'Mock: the valid code is 123456',mfaBad:'Enter 6 digits',
cdT:'Recovery codes',cdD:'These codes are shown only once. Store them somewhere safe.',cdSv:'I have saved the codes',cont:'Continue',
rcT:'Regenerate recovery codes',rcD:'Enter your current verification code. Previous codes will stop working.',rcGo:'Generate new codes',
meT:'My profile',acc:'Account',empf:'Employee file',sec:'Security',lng:'Notification language',rls:'Roles',un:'Username',em:'Email',drs:'People reporting to me',fnm:'Name',dep:'Department',jt:'Job title',hd:'Hire date',mg:'Manager',shf:'Current shift',ctr:'Current contract',num:'Employee number',gn:'Gender',ph:'Phone',adr:'Address',dob:'Date of birth',ro:'Read-only. Changes are made by HR.',
mfaS:'Two-step verification',mfaRq:'Required',mfaEn:'Status',mfaOn:'Enabled',mfaOff:'Not enabled',mfaSn:'Enabled since',mfaRm:'Recovery codes remaining',yes:'Yes',no:'No',chpw:'Change password',out:'Sign out',outN:'Ends the session in the UI only; the contract has no token-revocation operation.',
tl:'My administrative task log',tlTy:'Type',tlAll:'All types',tlE:'No tasks in this period',tlTm:'Time',tlTi:'Task',prv:'Previous',nxt:'Next',frm:'From',to:'To',
inb:'Pending my action',inbT:'Total',inbD:'via delegation',inbE:'Nothing is waiting for your action',inbN:'Complaints are never included',
er:{'Hr:Identity:CurrentPasswordIncorrect':'The current password is incorrect','Hr:Identity:PasswordPolicyViolation':'The new password does not meet the password policy','Hr:Identity:NewPasswordSameAsCurrent':'The new password is the same as the current one','Hr:Identity:MfaCodeInvalid':'The verification code is not valid','Hr:Identity:MfaAlreadyEnabled':'Two-step verification is already enabled','Hr:Identity:MfaEnrollmentNotStarted':'Setup has not been started','Hr:Identity:MfaNotEnabled':'Two-step verification is not enabled',gen:'The operation could not be completed. Try again'},
rl:{employee:'Employee',departmentManager:'Department manager',hrManager:'HR manager',executiveDirector:'Executive director',reviewer:'Reviewer',accountant:'Accountant',storeOfficer:'Store officer',systemAdmin:'System admin'},
cat:{request:'Requests',memo:'Memos',incident:'Fault reports',stockIssue:'Stock issues',evaluation:'Evaluations',departmentReport:'Department report',evaluationObjection:'Evaluation objections',evaluationAcknowledgement:'Evaluation acknowledgements',circularAcknowledgement:'Circular acknowledgements',custodyAcknowledgement:'Custody acknowledgements',warningResponse:'Warning responses',warningDecision:'Warning decisions',contractDecision:'Contract decisions',appreciationLetterAcknowledgement:'Appreciation letter acknowledgements'},
tt:{requestApproved:'Request approved',requestRejected:'Request rejected',requestSubmittedOnBehalf:'Request submitted on behalf',memoProcessed:'Memo processed',incidentForwarded:'Incident forwarded',evaluationSubmitted:'Evaluation submitted',departmentReportSubmitted:'Department report submitted',circularSent:'Circular sent',custodyCountRecorded:'Custody count recorded',warningRecommended:'Warning recommended',delegationCreated:'Delegation created'},
es:{active:'Active',suspended:'Suspended',ended:'Ended'},gd:{male:'Male',female:'Female'}}};
const PS={
emp:{u:'s.hasani',dn:['سارة الحسني','Sara Al-Hasani'],roles:['employee'],dept:['الهندسة','Engineering'],jt:['مهندسة برمجيات','Software Engineer'],hire:'2024-03-15',mg:['حسن علي','Hassan Ali'],num:'E-1042',g:'female',dr:0,must:false,mfaR:false,mfaE:false,id:'01',inb:[['evaluationAcknowledgement',1,0],['circularAcknowledgement',2,0]]},
mgr:{u:'h.ali',dn:['حسن علي','Hassan Ali'],roles:['departmentManager'],dept:['العمليات','Operations'],jt:['مدير','Manager'],hire:'2022-09-01',mg:['ناصر الكعبي','Nasser Al-Kaabi'],num:'E-1045',g:'male',dr:5,must:false,mfaR:false,mfaE:false,id:'02',inb:[['request',4,1],['memo',2,0],['evaluation',5,0],['departmentReport',1,0],['circularAcknowledgement',1,0]]},
hr:{u:'m.rawi',dn:['منى الراوي','Muna Al-Rawi'],roles:['hrManager'],dept:['الموارد البشرية','Human Resources'],jt:['مدير الموارد البشرية','HR Manager'],hire:'2021-01-10',mg:['ليث العبيدي','Layth Al-Obaidi'],num:'E-1048',g:'female',dr:0,must:false,mfaR:true,mfaE:true,id:'03',inb:[['request',7,0],['incident',2,0],['evaluationObjection',1,0],['contractDecision',3,0]]},
acc:{u:'k.saleh',dn:['كريم صالح','Karim Saleh'],roles:['accountant'],dept:['المالية','Finance'],jt:['محاسب','Accountant'],hire:'2023-06-01',mg:['حسن علي','Hassan Ali'],num:'E-1047',g:'male',dr:0,must:false,mfaR:true,mfaE:false,id:'04',inb:[]},
nw:{u:'n.tamimi',dn:['نور التميمي','Noor Al-Tamimi'],roles:['employee'],dept:['الهندسة','Engineering'],jt:['مهندسة برمجيات','Software Engineer'],hire:'2026-11-01',mg:['حسن علي','Hassan Ali'],num:'E-1046',g:'female',dr:0,must:true,mfaR:false,mfaE:false,id:'05',inb:[]}};
const TLP=[['requestApproved','موافقة على إجازة موظف','Approved an employee leave request'],['memoProcessed','معالجة مطالعة','Processed a memo'],['evaluationSubmitted','رفع تقييم شهري','Submitted a monthly evaluation'],['circularSent','إرسال تعميم للفريق','Sent a circular to the team'],['requestRejected','رفض طلب','Rejected a request'],['custodyCountRecorded','تسجيل جرد عهدة القسم','Recorded a department custody count']];
const OPR={"UserAccount_List": "hm", "UserAccount_Create": "h", "UserAccount_Get": "hm", "UserAccount_ResetPassword": "hx", "UserAccount_SetRoles": "hm", "UserAccount_GetPermissions": "m", "UserAccount_UpdatePermissions": "m", "Role_List": "hm", "Role_Create": "m", "Role_Get": "m", "Role_Update": "m", "Role_Delete": "m", "Role_GetPermissions": "m", "Role_UpdatePermissions": "m", "Department_List": "h", "Department_Create": "h", "Department_Lookup": "edhxvasm", "Department_Get": "h", "Department_Update": "h", "Department_Delete": "h", "Employee_List": "h", "Employee_Create": "h", "Employee_Lookup": "shxavm", "Employee_Get": "h", "Employee_Update": "h", "Employee_ChangeManager": "h", "Employee_GetManagerHistory": "h", "Employee_GetAdministrativeTaskLog": "h", "Manager_AssignEmployees": "h", "Manager_GetMyTeam": "dm", "Manager_GetTeamMember": "dm", "EmployeeDocument_List": "edmh", "EmployeeDocument_Upload": "h", "EmployeeDocument_GetDownloadUrl": "edmh", "EmployeeDocument_Delete": "h", "UserAccount_Lookup": "xhm", "UserAccount_ResetMfa": "mx"};
const PERMS={"Hr.AppreciationLetter.Acknowledge": "e", "Hr.AppreciationLetter.Issue": "x", "Hr.AppreciationLetter.Read": "dehx", "Hr.AppreciationLetter.ReadEmployeeFile": "dhx", "Hr.AppreciationLetter.ReadOwn": "e", "Hr.Attendance.Correction.Create": "dehm", "Hr.Attendance.Correction.View": "dehmx", "Hr.Attendance.Device.Manage": "m", "Hr.Attendance.Device.View": "hm", "Hr.Attendance.DeviceMap.Manage": "hm", "Hr.Attendance.GeoFence.Manage": "m", "Hr.Attendance.Gps.Punch": "dehm", "Hr.Attendance.Gps.ReviewFlagged": "h", "Hr.Attendance.Import": "h", "Hr.Attendance.Settings.Manage": "m", "Hr.Attendance.Settings.View": "dehm", "Hr.Attendance.ViewDayDetail": "dehm", "Hr.Attendance.ViewEmployee": "dh", "Hr.Attendance.ViewOwn": "dehm", "Hr.Attendance.ViewTeam": "d", "Hr.AuditLog.Read": "mx", "Hr.Circulars.Read": "adehmsvx", "Hr.Circulars.Send": "dhx", "Hr.Complaints.Attachment": "dehx", "Hr.Complaints.Close": "hx", "Hr.Complaints.Handle": "dhx", "Hr.Complaints.Handlers.Approve": "x", "Hr.Complaints.Handlers.Read": "hx", "Hr.Complaints.Handlers.Request": "h", "Hr.Complaints.RaiseLevel": "dehx", "Hr.Complaints.Submit": "de", "Hr.Complaints.ViewAccessLog": "dhx", "Hr.Complaints.ViewOwn": "de", "Hr.Contract.Create": "h", "Hr.Contract.Decide": "h", "Hr.Contract.Read": "h", "Hr.Contract.ReadOwn": "e", "Hr.Contract.Recommend": "h", "Hr.Custody.Acknowledge": "adehmsvx", "Hr.Custody.Count": "d", "Hr.Custody.Create": "s", "Hr.Custody.Return": "s", "Hr.Custody.View": "adehmsvx", "Hr.Custody.ViewAll": "hs", "Hr.Custody.ViewCount": "dhs", "Hr.Custody.ViewOwn": "adehmsvx", "Hr.Custody.ViewTeam": "d", "Hr.Delegation.Manage": "adhmx", "Hr.Delegation.ReadAll": "h", "Hr.Department.Lookup": "adehmsvx", "Hr.Department.Manage": "h", "Hr.Department.Read": "h", "Hr.DepartmentReports.Manage": "d", "Hr.DepartmentReports.Read": "dh", "Hr.Employee.AssignManager": "h", "Hr.Employee.Create": "h", "Hr.Employee.Document.Delete": "h", "Hr.Employee.Document.Read": "dehm", "Hr.Employee.Document.Upload": "h", "Hr.Employee.Lookup": "ahmsvx", "Hr.Employee.Read": "h", "Hr.Employee.ReadManagerHistory": "h", "Hr.Employee.ReadOwnTaskLog": "d", "Hr.Employee.ReadSelf": "adehmsvx", "Hr.Employee.ReadTaskLog": "h", "Hr.Employee.ReadTeam": "dm", "Hr.Employee.Update": "h", "Hr.EmployeeShift.Assign": "h", "Hr.EmployeeShift.View": "h", "Hr.Evaluations.AnnualSummary.Read": "dh", "Hr.Evaluations.Criteria.Manage": "m", "Hr.Evaluations.Cycle.Read": "dhv", "Hr.Evaluations.DecideObjection": "h", "Hr.Evaluations.Grades.Manage": "m", "Hr.Evaluations.HrRead": "h", "Hr.Evaluations.InternalNote.Read": "dh", "Hr.Evaluations.InternalNote.Write": "d", "Hr.Evaluations.Manage": "d", "Hr.Evaluations.ViewOwn": "e", "Hr.Finance.AccountantRequest.Read": "a", "Hr.Finance.Adjustment.Create": "a", "Hr.Finance.Adjustment.Read": "a", "Hr.Finance.Adjustment.Review": "a", "Hr.Finance.Expense.Create": "a", "Hr.Finance.Expense.Read": "a", "Hr.Finance.PayrollRun.Approve": "hx", "Hr.Finance.PayrollRun.Close": "a", "Hr.Finance.PayrollRun.Export": "ahx", "Hr.Finance.PayrollRun.Prepare": "a", "Hr.Finance.PayrollRun.Read": "ahx", "Hr.Finance.Payslip.ReadOwn": "e", "Hr.Finance.Report.Read": "a", "Hr.Finance.SalaryProfile.Read": "a", "Hr.Finance.SalaryProfile.Update": "a", "Hr.Holiday.Manage": "h", "Hr.Holiday.View": "adehmsvx", "Hr.Identity.Permission.Manage": "m", "Hr.Identity.Role.Manage": "m", "Hr.Identity.Role.Read": "hm", "Hr.Identity.UserAccount.Create": "h", "Hr.Identity.UserAccount.Lookup": "hmx", "Hr.Identity.UserAccount.Read": "hm", "Hr.Identity.UserAccount.ResetMfa": "mx", "Hr.Identity.UserAccount.ResetPassword": "hx", "Hr.Identity.UserAccount.SetRoles": "hm", "Hr.Incident.Create": "adehmsvx", "Hr.Incident.Process": "dh", "Hr.Incident.UpdateStatus": "h", "Hr.Incident.View": "adehmsvx", "Hr.Incident.ViewOwn": "adehmsvx", "Hr.Leave.Balance.Adjust": "h", "Hr.Leave.Balance.SetEntitlement": "h", "Hr.Leave.Balance.View": "dhmx", "Hr.Leave.Balance.ViewOwn": "dehm", "Hr.Leave.Calendar.ViewTeam": "d", "Hr.Leave.Ledger.View": "dhmx", "Hr.Leave.Ledger.ViewOwn": "dehm", "Hr.Leave.OtherType.Manage": "m", "Hr.Leave.Request.Create": "dehm", "Hr.Leave.Request.View": "dehmx", "Hr.Leave.Request.ViewEmployee": "dh", "Hr.Leave.Request.ViewOwn": "dehm", "Hr.Memo.Close": "hx", "Hr.Memo.Create": "adehmsv", "Hr.Memo.Process": "dhmx", "Hr.Memo.View": "adehmsvx", "Hr.Memo.ViewOwn": "adehmsv", "Hr.Notification.Device": "adehmsvx", "Hr.Notification.Read": "adehmsvx", "Hr.Reports.Export": "dhv", "Hr.Reports.Hr": "h", "Hr.Reports.Review": "v", "Hr.Reports.Team": "d", "Hr.Request.Cancel": "adehmsvx", "Hr.Request.Create": "adehmsvx", "Hr.Request.Decide": "adhmsx", "Hr.Request.Inbox": "adhmsx", "Hr.Request.Type.Manage": "m", "Hr.Request.Type.View": "adehmsvx", "Hr.Request.View": "adehmsvx", "Hr.Request.ViewList": "dh", "Hr.Request.ViewOwn": "adehmsvx", "Hr.Settings.Complaints": "m", "Hr.Settings.Contracts": "m", "Hr.Settings.Finance": "m", "Hr.Settings.HourlyLeave": "m", "Hr.Settings.Read": "hm", "Hr.Settings.Reminders": "m", "Hr.Store.Issue.Deliver": "s", "Hr.Store.Issue.Process": "s", "Hr.Store.Issue.View": "s", "Hr.Store.Item.Lookup": "adehmsvx", "Hr.Store.Item.Manage": "s", "Hr.Store.Item.View": "s", "Hr.Store.Stock.Adjust": "s", "Hr.Store.Stock.Receive": "s", "Hr.Store.Stock.View": "s", "Hr.Warning.Decide": "h", "Hr.Warning.Notify": "h", "Hr.Warning.Object": "e", "Hr.Warning.Read": "hx", "Hr.Warning.ReadEmployeeFile": "dh", "Hr.Warning.ReadOwn": "e", "Hr.Warning.Recommend": "dv", "Hr.Warning.ResolveObjection": "x", "Hr.Warning.Respond": "e", "Hr.WorkShift.Manage": "hm", "Hr.WorkShift.View": "hm"};
const RAB={"e": "employee", "d": "departmentManager", "h": "hrManager", "x": "executiveDirector", "v": "reviewer", "a": "accountant", "s": "storeOfficer", "m": "systemAdmin"};
const T4={ar:{
denT:'لا تملك صلاحية الوصول إلى هذه الشاشة',denD:'ليس لدورك عملية مرتبطة بهذه الشاشة. الفحص الحقيقي يتم في الخادم.',retry:'إعادة المحاولة',ldg:'جارٍ التحميل',
open:'فتح',edit:'تعديل',del:'حذف',back:'رجوع',next:'التالي',prev2:'السابق',finish:'إنشاء',save2:'حفظ',cncl:'إلغاء',cnf:'تأكيد',add:'إضافة',
pgOf:'من',
// titles
eT:'الموظفون',eSub:'دليل الموظفين',eNew:'إضافة موظف',eFile:'ملف الموظف',eEdit:'تعديل بيانات موظف',tT:'موظفوني',tSub:'الموظفون المرتبطون بك',
dT:'الأقسام',dNew:'قسم جديد',aT:'الحسابات',aD:'تفاصيل الحساب',rT:'الأدوار',rNew:'دور جديد',rE:'صلاحيات الدور',sT:'الإعدادات',sSub:'أقسام الإعدادات حسب دورك',
// tabs
tData:'البيانات',tDocs:'الوثائق',tHist:'السجل',tTask:'المهام الإدارية',tPerm:'الصلاحيات',
// filters
fSearch:'بحث',fDept:'القسم',fStatus:'الحالة',fMgr:'المدير',fRole:'الدور',fAll:'الكل',fActive:'نشط',fInactive:'غير نشط',fMust:'تغيير كلمة السر مطلوب',fType:'النوع',
// columns
cName:'الاسم',cDept:'القسم',cTitle:'المسمى',cHire:'المباشرة',cStatus:'الحالة',cMgr:'المدير',cRoles:'الأدوار',cAcc:'حساب',cUser:'اسم المستخدم',cEmail:'البريد',cMfa:'التحقق بخطوتين',cCnt:'الموظفون',cUsers:'المستخدمون',cStatic:'النوع',cFile:'الملف',cSize:'الحجم',cSens:'الحساسية',cScan:'الفحص',cFrom:'من',cTo:'إلى',cBy:'بواسطة',cReason:'السبب',cTime:'الوقت',cTask:'المهمة',cLock:'مقفل',
// kv
kName:'الاسم',kNameAr:'الاسم بالعربية',kNameEn:'الاسم بالإنجليزية',kNum:'الرقم الوظيفي',kEmail:'البريد',kPhone:'الهاتف',kDob:'تاريخ الميلاد',kAddr:'العنوان',kGen:'الجنس',kShift:'الدوام الحالي',kContract:'العقد الحالي',kReports:'الموظفون المرتبطون',kAcct:'الحساب',kCreated:'أُنشئ',kUpd:'آخر تعديل',kActive:'نشط',kLocked:'مقفل',kMust:'تغيير كلمة السر مطلوب',kMfaReq:'التحقق بخطوتين إلزامي',kMfaOn:'التحقق بخطوتين مفعّل',kNoAcct:'لا يوجد حساب',
yes:'نعم',no:'لا',none:'—',male:'ذكر',female:'أنثى',
// form labels
lFullNameAr:'الاسم بالعربية',lFullNameEn:'الاسم بالإنجليزية',lEmail:'البريد الإلكتروني',lPhone:'الهاتف',lDob:'تاريخ الميلاد',lAddr:'العنوان',lDept:'القسم',lTitle:'المسمى الوظيفي',lHire:'تاريخ المباشرة',lShift:'الدوام',lMgr:'المدير',lUser:'اسم المستخدم',lRoles:'الأدوار',lGen:'الجنس',lNum:'الرقم الوظيفي',lStatus:'الحالة',lNameAr:'الاسم بالعربية',lNameEn:'الاسم بالإنجليزية',lKey:'المعرّف (إنجليزي)',lReason:'السبب',lDocType:'نوع الوثيقة',lDocTitle:'عنوان الوثيقة',lSens:'الحساسية',lFile:'اسم الملف (تجريبي)',lEmps:'الموظفون',
hReq:'مطلوب',hKey:'حروف إنجليزية وأرقام، تبدأ بحرف صغير',hFile:'PDF أو JPEG أو PNG حتى 10 ميجابايت',
// steps
st1:'البيانات الشخصية والوظيفية',st2:'الدوام والمدير والأدوار',st3:'الحساب والمراجعة',
// actions
aChgMgr:'تغيير المدير',aAssign:'ربط موظفين',aNewAcc:'إنشاء حساب',aRoles:'تعديل الأدوار',aReset:'إعادة تعيين كلمة السر',aMfa:'إعادة ضبط التحقق بخطوتين',aPerm:'الصلاحيات المباشرة',aUp:'رفع وثيقة',aDl:'تنزيل',aSavePerm:'حفظ الصلاحيات',aTask:'تحميل',
// dialogs
dChgMgr:'تغيير مدير الموظف',dAssign:'ربط موظفين بمدير',dNewAcc:'إنشاء حساب للموظف',dRoles:'تعديل أدوار المستخدم',dReset:'إعادة تعيين كلمة السر',dResetB:'سيصل رابط لمرة واحدة إلى بريد الموظف، ولا تظهر كلمة السر لأحد.',dMfa:'إعادة ضبط التحقق بخطوتين',dMfaB:'يُلغى مفتاح TOTP ورموز الاسترداد وتُلغى جلسات المستخدم.',dDel:'تأكيد الحذف',dDelB:'لا يمكن التراجع عن هذا الإجراء.',dCred:'بيانات الدخول المؤقتة',dCredB:'تُعرض مرة واحدة فقط. سلّمها للموظف؛ تغيير كلمة السر إلزامي عند أول دخول.',dUp:'رفع وثيقة',
// toasts
tCreated:'تم الإنشاء (تجريبي)',tSaved:'تم الحفظ (تجريبي)',tDeleted:'تم الحذف (تجريبي)',tMoved:'تم نقل الموظفين (تجريبي)',tSent:'أُرسل رابط إعادة التعيين (تجريبي)',tMfaReset:'تمت إعادة ضبط التحقق بخطوتين (تجريبي)',tAssigned:'تم الربط (تجريبي)',
// empty
emE:'لا يوجد موظفون',emE2:'جرّب تغيير الفلاتر',emD:'لا توجد وثائق',emH:'لا يوجد سجل',emA:'لا توجد حسابات',emR:'لا توجد أدوار',emDp:'لا توجد أقسام',emT:'لا يوجد موظفون مرتبطون بك',emTask:'لا توجد مهام',
req:'هذا الحقل مطلوب',min:'قصير جداً',pat:'صيغة غير صحيحة',
notes:'التحميل التجريبي: لا يوجد خادم. البيانات المعروضة تجريبية وتحاكي العقد.',
setCards:{roles:['الأدوار والصلاحيات','إدارة الأدوار وصلاحياتها غير المحمية'],dept:['الأقسام','إدارة الأقسام'],later:['أقسام أخرى','تُبنى في وحدات لاحقة']},
sOpen:'فتح',
ste:{active:'نشط',suspended:'موقوف',ended:'منتهي'},sens:{normal:'عادية',sensitive:'حساسة'},scan:{pending:'قيد الفحص',clean:'سليم',infected:'مصاب'},dt:{identity:'هوية',passport:'جواز',contract:'عقد',certificate:'شهادة',medical:'طبية',other:'أخرى'},stat:{true:'ثابت',false:'مخصص'},
er2:{'Hr:Employee:ManagerRequired':'يجب تحديد مدير لهذا الموظف','Hr:Employee:ExecutiveCannotHaveManager':'المدير التنفيذي لا مدير له','Hr:Employee:ManagerNotFound':'المدير غير موجود','Hr:Employee:ManagerNotActive':'المدير غير نشط','Hr:Employee:InvalidManagerForRole':'هذا المدير لا يصلح لأدوار الموظف','Hr:Employee:CannotManageSelf':'لا يمكن ربط الموظف بنفسه','Hr:Employee:ManagerCycle':'هذا الربط يكوّن حلقة','Hr:Employee:EmployeeEnded':'الموظف منتهي الخدمة','Hr:Employee:ManagerUnchanged':'المدير الحالي هو نفسه','Hr:Employee:WorkShiftRequired':'يجب تحديد الدوام','Hr:Employee:HasDirectReports':'لديه موظفون مرتبطون به','Hr:Employee:HireDateLocked':'تاريخ المباشرة مقفل','Hr:Identity:SingletonRoleAlreadyAssigned':'هذا الدور بيد مستخدم آخر نشط','Hr:Identity:RoleNotFound':'الدور غير موجود','Hr:Identity:ProtectedRoleChangeRequiresApproval':'تغيير حامل هذا الدور من صلاحية المدير التنفيذي فقط','Hr:Identity:ForbiddenRoleCombination':'جمع الأدوار هذا غير مسموح','Hr:Employee:EmployeeAlreadyHasAccount':'للموظف حساب بالفعل','Hr:Identity:CannotChangeOwnRoles':'لا يمكنك تغيير أدوارك','Hr:Identity:LastSystemAdminCannotBeRemoved':'لا يمكن إزالة آخر مسؤول نظام','Hr:Identity:ManagerRoleHasDirectReports':'لا يمكن إزالة دور المدير وله موظفون','Hr:Identity:CannotResetOwnPassword':'لا يمكنك إعادة تعيين كلمة سرك هنا','Hr:Identity:PasswordResetNotAllowedForTarget':'لا يُسمح بإعادة تعيين كلمة سر هذا المستخدم','Hr:Identity:AccountInactive':'الحساب غير نشط','Hr:Identity:NoVerifiedEmail':'لا يوجد بريد موثّق للموظف','Hr:Identity:CannotResetOwnMfa':'لا يمكنك إعادة ضبط تحققك','Hr:Identity:MfaResetNotAllowedForTarget':'لا يُسمح بإعادة ضبط تحقق هذا المستخدم','Hr:Identity:MfaNotEnabled':'التحقق بخطوتين غير مفعّل لهذا المستخدم','Hr:Identity:StaticRoleCannotBeDeleted':'لا يمكن حذف دور ثابت','Hr:Identity:RoleInUse':'الدور مستخدم','Hr:Identity:ProtectedPermissionNotGrantable':'هذه الصلاحية محمية ولا تُمنح','Hr:Identity:FinancialPermissionNotGrantable':'الصلاحيات المالية لا تُمنح لهذا الدور','Hr:Department:HasEmployees':'في القسم موظفون','Hr:Department:HasSharedCustody':'في القسم عهدة مشتركة','Hr:Document:FileTooLarge':'الملف أكبر من 10 ميجابايت','Hr:Document:FileTypeNotAllowed':'نوع الملف غير مسموح','Hr:Department:NotFound':'القسم غير موجود','Hr:Document:NotAvailable':'الوثيقة غير متاحة','Hr:Employee:EmployeeNotFound':'الموظف غير موجود','Hr:Employee:WorkShiftNotFound':'الدوام غير موجود','Hr:Identity:CannotChangeOwnPermissions':'لا يمكنك تغيير صلاحياتك','Hr:Identity:PermissionNotFound':'الصلاحية غير موجودة','Hr:Identity:ProtectedPermission':'صلاحية محمية','conflict':'عُدّل هذا السجل من مكان آخر. أعد التحميل ثم كرر التعديل.',forbidden:'غير مسموح لدورك'}},
en:{
denT:'You do not have access to this screen',denD:'Your role has no operation tied to this screen. The real check happens on the server.',retry:'Retry',ldg:'Loading',
open:'Open',edit:'Edit',del:'Delete',back:'Back',next:'Next',prev2:'Previous',finish:'Create',save2:'Save',cncl:'Cancel',cnf:'Confirm',add:'Add',pgOf:'of',
eT:'Employees',eSub:'Employee directory',eNew:'Add employee',eFile:'Employee file',eEdit:'Edit employee',tT:'My team',tSub:'Employees linked to you',
dT:'Departments',dNew:'New department',aT:'Accounts',aD:'Account details',rT:'Roles',rNew:'New role',rE:'Role permissions',sT:'Settings',sSub:'Settings sections for your role',
tData:'Data',tDocs:'Documents',tHist:'History',tTask:'Administrative tasks',tPerm:'Permissions',
fSearch:'Search',fDept:'Department',fStatus:'Status',fMgr:'Manager',fRole:'Role',fAll:'All',fActive:'Active',fInactive:'Inactive',fMust:'Must change password',fType:'Type',
cName:'Name',cDept:'Department',cTitle:'Title',cHire:'Hire date',cStatus:'Status',cMgr:'Manager',cRoles:'Roles',cAcc:'Account',cUser:'Username',cEmail:'Email',cMfa:'MFA',cCnt:'Employees',cUsers:'Users',cStatic:'Type',cFile:'File',cSize:'Size',cSens:'Sensitivity',cScan:'Scan',cFrom:'From',cTo:'To',cBy:'By',cReason:'Reason',cTime:'Time',cTask:'Task',cLock:'Locked',
kName:'Name',kNameAr:'Name (Arabic)',kNameEn:'Name (English)',kNum:'Employee number',kEmail:'Email',kPhone:'Phone',kDob:'Date of birth',kAddr:'Address',kGen:'Gender',kShift:'Current shift',kContract:'Current contract',kReports:'Direct reports',kAcct:'Account',kCreated:'Created',kUpd:'Last modified',kActive:'Active',kLocked:'Locked out',kMust:'Must change password',kMfaReq:'MFA required',kMfaOn:'MFA enabled',kNoAcct:'No account',
yes:'Yes',no:'No',none:'—',male:'Male',female:'Female',
lFullNameAr:'Name (Arabic)',lFullNameEn:'Name (English)',lEmail:'Email',lPhone:'Phone',lDob:'Date of birth',lAddr:'Address',lDept:'Department',lTitle:'Job title',lHire:'Hire date',lShift:'Work shift',lMgr:'Manager',lUser:'Username',lRoles:'Roles',lGen:'Gender',lNum:'Employee number',lStatus:'Status',lNameAr:'Name (Arabic)',lNameEn:'Name (English)',lKey:'Key (English)',lReason:'Reason',lDocType:'Document type',lDocTitle:'Document title',lSens:'Sensitivity',lFile:'File name (mock)',lEmps:'Employees',
hReq:'Required',hKey:'Letters and digits, starts with a lowercase letter',hFile:'PDF, JPEG or PNG up to 10 MB',
st1:'Personal and job data',st2:'Shift, manager and roles',st3:'Account and review',
aChgMgr:'Change manager',aAssign:'Assign employees',aNewAcc:'Create account',aRoles:'Edit roles',aReset:'Reset password',aMfa:'Reset MFA',aPerm:'Direct permissions',aUp:'Upload document',aDl:'Download',aSavePerm:'Save permissions',aTask:'Load',
dChgMgr:'Change employee manager',dAssign:'Assign employees to manager',dNewAcc:'Create account for employee',dRoles:'Edit user roles',dReset:'Reset password',dResetB:'A one-time link is sent to the employee’s email. Nobody sees the password.',dMfa:'Reset two-step verification',dMfaB:'Cancels the TOTP key and recovery codes and revokes the user’s sessions.',dDel:'Confirm deletion',dDelB:'This cannot be undone.',dCred:'Temporary credentials',dCredB:'Shown only once. Hand them to the employee; a password change is mandatory at first sign-in.',dUp:'Upload document',
tCreated:'Created (mock)',tSaved:'Saved (mock)',tDeleted:'Deleted (mock)',tMoved:'Employees moved (mock)',tSent:'Reset link sent (mock)',tMfaReset:'MFA reset (mock)',tAssigned:'Assigned (mock)',
emE:'No employees',emE2:'Try changing the filters',emD:'No documents',emH:'No history',emA:'No accounts',emR:'No roles',emDp:'No departments',emT:'Nobody is linked to you',emTask:'No tasks',
req:'This field is required',min:'Too short',pat:'Invalid format',
notes:'Mock mode: no backend. Data shown is demo data that mirrors the contract.',
setCards:{roles:['Roles and permissions','Manage roles and their non-protected permissions'],dept:['Departments','Manage departments'],later:['Other sections','Built in later modules']},
sOpen:'Open',
ste:{active:'Active',suspended:'Suspended',ended:'Ended'},sens:{normal:'Normal',sensitive:'Sensitive'},scan:{pending:'Pending',clean:'Clean',infected:'Infected'},dt:{identity:'Identity',passport:'Passport',contract:'Contract',certificate:'Certificate',medical:'Medical',other:'Other'},stat:{true:'Built-in',false:'Custom'},
er2:{'Hr:Employee:ManagerRequired':'A manager is required for this employee','Hr:Employee:ExecutiveCannotHaveManager':'The executive has no manager','Hr:Employee:ManagerNotFound':'Manager not found','Hr:Employee:ManagerNotActive':'The manager is not active','Hr:Employee:InvalidManagerForRole':'This manager is not valid for the employee’s roles','Hr:Employee:CannotManageSelf':'An employee cannot manage themselves','Hr:Employee:ManagerCycle':'This link would create a loop','Hr:Employee:EmployeeEnded':'The employee’s service has ended','Hr:Employee:ManagerUnchanged':'This is already the current manager','Hr:Employee:WorkShiftRequired':'A work shift is required','Hr:Employee:HasDirectReports':'The employee has direct reports','Hr:Employee:HireDateLocked':'The hire date is locked','Hr:Identity:SingletonRoleAlreadyAssigned':'This role is held by another active user','Hr:Identity:RoleNotFound':'Role not found','Hr:Identity:ProtectedRoleChangeRequiresApproval':'Only the executive director can change who holds this role','Hr:Identity:ForbiddenRoleCombination':'This role combination is not allowed','Hr:Employee:EmployeeAlreadyHasAccount':'The employee already has an account','Hr:Identity:CannotChangeOwnRoles':'You cannot change your own roles','Hr:Identity:LastSystemAdminCannotBeRemoved':'The last system admin cannot be removed','Hr:Identity:ManagerRoleHasDirectReports':'The manager role cannot be removed while there are direct reports','Hr:Identity:CannotResetOwnPassword':'You cannot reset your own password here','Hr:Identity:PasswordResetNotAllowedForTarget':'Resetting this user’s password is not allowed','Hr:Identity:AccountInactive':'The account is inactive','Hr:Identity:NoVerifiedEmail':'The employee has no verified email','Hr:Identity:CannotResetOwnMfa':'You cannot reset your own verification','Hr:Identity:MfaResetNotAllowedForTarget':'Resetting this user’s verification is not allowed','Hr:Identity:MfaNotEnabled':'MFA is not enabled for this user','Hr:Identity:StaticRoleCannotBeDeleted':'A built-in role cannot be deleted','Hr:Identity:RoleInUse':'The role is in use','Hr:Identity:ProtectedPermissionNotGrantable':'This permission is protected and cannot be granted','Hr:Identity:FinancialPermissionNotGrantable':'Financial permissions cannot be granted to this role','Hr:Department:HasEmployees':'The department has employees','Hr:Department:HasSharedCustody':'The department has shared custody','Hr:Document:FileTooLarge':'The file is larger than 10 MB','Hr:Document:FileTypeNotAllowed':'The file type is not allowed','Hr:Department:NotFound':'Department not found','Hr:Document:NotAvailable':'The document is not available','Hr:Employee:EmployeeNotFound':'Employee not found','Hr:Employee:WorkShiftNotFound':'Work shift not found','Hr:Identity:CannotChangeOwnPermissions':'You cannot change your own permissions','Hr:Identity:PermissionNotFound':'Permission not found','Hr:Identity:ProtectedPermission':'Protected permission','conflict':'This record was changed elsewhere. Reload and repeat the edit.',forbidden:'Not allowed for your role'}}};
PS.adm={u:'n.kaabi',dn:['ناصر الكعبي','Nasser Al-Kaabi'],roles:['systemAdmin'],dept:['العمليات','Operations'],jt:['مسؤول النظام','System Admin'],hire:'2020-05-01',mg:['ليث العبيدي','Layth Al-Obaidi'],num:'E-1001',g:'male',dr:0,must:false,mfaR:true,mfaE:true,id:'06',inb:[]};
T2.ar.pl.adm=['مسؤول النظام','يدير الأدوار والحسابات'];T2.en.pl.adm=['System admin','Manages roles and accounts'];
IC.push('M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1','M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75');PAGES.push('acc','team');T.ar.nav.push('الحسابات','موظفوني');T.en.nav.push('Accounts','My team');
const T5={ar:{
t:'نظام التصميم',sub:'شاشة تحقق داخلية مؤقتة. ليست وحدة أعمال.',replay:'إعادة تشغيل الحركات',theme:'المظهر',lang:'اللغة',
s1:'الرموز: الألوان والخطوط والزوايا',s2:'هيكل التطبيق',s3:'عرض البيانات',s4:'المدخلات',s5:'الطبقات المنبثقة',s6:'الحالات',s7:'الحركة',s8:'شبكة HR التنظيمية',s9:'مصفوفة المظهر والاتجاه',s10:'السلوك المتجاوب',
bg:'الخلفية',surface:'السطح',text:'النص',text2:'نص ثانوي',accent:'اللون الأساسي',border:'الحد',ok:'نجاح',warn:'تنبيه',dng:'خطر',inf:'معلومة',
tH1:'عنوان الصفحة 28/36',tH2:'عنوان البطاقة 18/24',tB:'نص أساسي 14/20',tC:'توضيح 12/16',rad:'زوايا: بطاقة 16، عنصر تحكم 10، شارة 20',
bc1:'لوحة التحكم',bc2:'الموظفون',bc3:'ملف الموظف',ph:'عنوان الصفحة',phSub:'وصف قصير للصفحة',phAct:'إجراء رئيسي',mini:'القائمة الجانبية (انتقال نابض)',miniT:'طي/فتح',
k1:'الحاضرون',k2:'طلبات معلقة',k3:'نسبة الإنجاز',kn:'بيانات تجريبية',
bn1:'بطاقة كبيرة',bn2:'بطاقة متوسطة',bn3:'بطاقة صغيرة',spot:'مرّر المؤشر لرؤية الإضاءة والميلان',glow:'حد زمردي متوهج',
tN:'الاسم',tD:'القسم',tS:'الحالة',tP:'التقدم',sortN:'ترتيب حسب الاسم',
st1:'نشط',st2:'معلّق',st3:'مرفوض',st4:'قيد المراجعة',st5:'مسودة',
ch1:'خط',ch2:'أعمدة',ch3:'حلقة',
tl:['تم رفع الطلب','وافق مدير القسم','بانتظار الموارد البشرية','مكتمل'],tlw:['قبل ساعتين','قبل ساعة','الآن','—'],
pr1:'شريط التقدم',pr2:'تقدم دائري',pr3:'خطوات',inc:'زيادة',dec:'إنقاص',
av:'مجموعة الصور الرمزية',
iTx:'حقل نص',iTxh:'نص مساعد',iErr:'حقل به خطأ',iErrT:'هذا الحقل مطلوب',iOk:'حقل صحيح',iOkT:'تم التحقق',iDis:'حقل معطّل',iRo:'للقراءة فقط',
iSel:'قائمة اختيار',iSrch:'بحث',iSrchP:'ابحث عن موظف…',iDate:'التاريخ',iDateH:'التواريخ بتوقيت مقر الشركة',iCk:'مربعات اختيار',iRd:'أزرار اختيار',iSw:'مفاتيح تبديل',
o1:'الخيار الأول',o2:'الخيار الثاني',o3:'الخيار الثالث',sw1:'إشعارات البريد',sw2:'وضع الصيانة',ckA:'أوافق على الشروط',ckB:'تذكيري لاحقاً',
up:'رفع ملف',upH:'اسحب الملف أو اضغط للاختيار',upC:'PDF أو JPEG أو PNG حتى 10 ميجابايت',upRm:'إزالة',
vS:'ملخص أخطاء النموذج',vS1:'الاسم مطلوب',vS2:'البريد غير صالح',
oModal:'نافذة منبثقة',oConf:'نافذة تأكيد',oDr:'درج جانبي',oDd:'قائمة منسدلة',oTip:'تلميح',oPop:'نافذة صغيرة',oToast:'إشعارات',
mT:'نافذة زجاجية',mB:'تظهر النافذة بانتقال زجاجي مع حجز التركيز.',cT:'تأكيد الحذف',cB:'لا يمكن التراجع عن هذا الإجراء.',cY:'حذف',cN:'إلغاء',drT:'درج التفاصيل',drB:'يتحرك من حافة الاتجاه ويُغلق بالنقر خارجه.',close:'إغلاق',
dd1:'تعديل',dd2:'نسخ',dd3:'حذف',tipT:'هذا تلميح مساعد',popT:'عنوان النافذة الصغيرة',popB:'محتوى إضافي بدون حجب الصفحة.',
tk1:'نجاح',tk2:'معلومة',tk3:'تنبيه',tk4:'خطأ',tm1:'تم الحفظ',tm2:'رسالة معلوماتية',tm3:'تحقق من البيانات',tm4:'تعذر إكمال العملية',
sL:'جارٍ التحميل',sSk:'هيكل التحميل',sE:'لا توجد بيانات',sEd:'ابدأ بإضافة عنصر',sEr:'حدث خطأ',sErd:'تعذر تحميل البيانات',sRt:'إعادة المحاولة',sOk:'تمت العملية بنجاح',sOkd:'تم حفظ التغييرات',sDis:'معطّل',sRo:'للقراءة فقط',sAD:'لا تملك صلاحية الوصول',sADd:'ليس لدورك عملية مرتبطة بهذه الشاشة',
a1:'معلومة',a2:'نجاح',a3:'تنبيه',a4:'خطأ',
mo:['دخول البطاقات المتتابع','عدّاد المؤشرات','انتقال القائمة الجانبية النابض','انتقال الصفحة','ظهور الرسوم البيانية','تبويبات متحركة','انتقال النافذة الزجاجية','انتقال الدرج','تأثير الأزرار','إضاءة المؤشر','ميلان ثلاثي الأبعاد','حد زمردي متوهج','انتقالات الإشعارات','وميض التحميل','تقدم متحرك','شبكة HR التنظيمية'],
moD:'كل تأثير أدناه مفعّل فعلياً. عند تفعيل «تقليل الحركة» في النظام تُزال الحركة غير الأساسية وتبقى الحالة واضحة.',pg:'مرر أو أعد التشغيل لرؤية انتقال الصفحة',
ntw:'شبكة تنظيمية زخرفية: عقدة مركزية، أقسام، وموظفون كتجريدات وليست أعداداً حقيقية.',
dk:'داكن',lt:'فاتح',rtl:'عربي RTL',ltr:'إنجليزي LTR',
vp:'حجم العرض',vd:'سطح المكتب',vt:'لوحي',vm:'هاتف',vN:'القائمة',vC:'بطاقة',
use:'الاستخدام',
tab:['نظرة عامة','التفاصيل','السجل']},
en:{
t:'Design system',sub:'Temporary internal validation view. Not a business module.',replay:'Replay animations',theme:'Theme',lang:'Language',
s1:'Tokens: colors, type, radius',s2:'Application shell',s3:'Data display',s4:'Inputs',s5:'Overlays',s6:'States',s7:'Motion',s8:'HR Organizational Network',s9:'Theme and direction matrix',s10:'Responsive behavior',
bg:'Background',surface:'Surface',text:'Text',text2:'Secondary text',accent:'Accent',border:'Border',ok:'Success',warn:'Warning',dng:'Danger',inf:'Info',
tH1:'Page title 28/36',tH2:'Card title 18/24',tB:'Body 14/20',tC:'Caption 12/16',rad:'Radius: card 16, control 10, pill 20',
bc1:'Dashboard',bc2:'Employees',bc3:'Employee file',ph:'Page title',phSub:'Short page description',phAct:'Primary action',mini:'Sidebar (spring transition)',miniT:'Collapse / expand',
k1:'Present today',k2:'Pending requests',k3:'Completion',kn:'Demo data',
bn1:'Large card',bn2:'Medium card',bn3:'Small card',spot:'Move the pointer to see spotlight and tilt',glow:'Emerald glowing border',
tN:'Name',tD:'Department',tS:'Status',tP:'Progress',sortN:'Sort by name',
st1:'Active',st2:'Pending',st3:'Rejected',st4:'In review',st5:'Draft',
ch1:'Line',ch2:'Bars',ch3:'Donut',
tl:['Request submitted','Manager approved','Waiting for HR','Completed'],tlw:['2 h ago','1 h ago','Now','—'],
pr1:'Progress bar',pr2:'Circular progress',pr3:'Steps',inc:'Increase',dec:'Decrease',
av:'Avatar group',
iTx:'Text field',iTxh:'Helper text',iErr:'Field with error',iErrT:'This field is required',iOk:'Valid field',iOkT:'Verified',iDis:'Disabled field',iRo:'Read-only',
iSel:'Select',iSrch:'Search',iSrchP:'Search people…',iDate:'Date',iDateH:'Dates use the company time zone',iCk:'Checkboxes',iRd:'Radio buttons',iSw:'Switches',
o1:'First option',o2:'Second option',o3:'Third option',sw1:'Email notifications',sw2:'Maintenance mode',ckA:'I accept the terms',ckB:'Remind me later',
up:'File upload',upH:'Drag a file here or click to choose',upC:'PDF, JPEG or PNG up to 10 MB',upRm:'Remove',
vS:'Form error summary',vS1:'Name is required',vS2:'Email is not valid',
oModal:'Modal',oConf:'Confirmation dialog',oDr:'Drawer',oDd:'Dropdown',oTip:'Tooltip',oPop:'Popover',oToast:'Toasts',
mT:'Glass modal',mB:'The modal enters with a glass transition and traps focus.',cT:'Confirm deletion',cB:'This cannot be undone.',cY:'Delete',cN:'Cancel',drT:'Details drawer',drB:'Slides from the inline-end edge; closes on outside click.',close:'Close',
dd1:'Edit',dd2:'Copy',dd3:'Delete',tipT:'This is a helpful tooltip',popT:'Popover title',popB:'Extra content without blocking the page.',
tk1:'Success',tk2:'Info',tk3:'Warning',tk4:'Error',tm1:'Saved',tm2:'Informational message',tm3:'Check the data',tm4:'The operation could not be completed',
sL:'Loading',sSk:'Skeleton',sE:'No data',sEd:'Start by adding an item',sEr:'Something went wrong',sErd:'Data could not be loaded',sRt:'Retry',sOk:'Operation succeeded',sOkd:'Your changes were saved',sDis:'Disabled',sRo:'Read-only',sAD:'You do not have access',sADd:'Your role has no operation tied to this screen',
a1:'Info',a2:'Success',a3:'Warning',a4:'Error',
mo:['Staggered card entrance','KPI count-up','Spring sidebar transition','Smooth page transition','Chart reveal','Animated tabs','Glass modal transition','Drawer transition','Button hover and press','Pointer spotlight','Subtle 3D tilt','Emerald border glow','Notification transitions','Loading shimmer','Animated progress','HR Organizational Network'],
moD:'Every effect below is live. When the system asks for reduced motion, non-essential movement is removed and state stays clear.',pg:'Replay to see the page transition',
ntw:'Decorative organizational network: central node, departments and employees as abstractions, not real counts.',
dk:'Dark',lt:'Light',rtl:'Arabic RTL',ltr:'English LTR',
vp:'Viewport',vd:'Desktop',vt:'Tablet',vm:'Mobile',vN:'Menu',vC:'Card',
use:'Usage',
tab:['Overview','Details','History']}};
IC.push('M12 3a9 9 0 1 0 0 18c1 0 2-.8 2-2 0-.6-.3-1-.6-1.4-.3-.4-.4-.8-.4-1.2 0-1 .8-2 2-2h2a3 3 0 0 0 3-3c0-4.4-4-8.4-8-8.4M7.5 10.5h.01M12 7.5h.01M16.5 10.5h.01');PAGES.push('ds');T.ar.nav.push('نظام التصميم');T.en.nav.push('Design system');
const OPR3={"WorkShift_List": "hm", "WorkShift_Create": "hm", "WorkShift_Get": "hm", "WorkShift_Update": "hm", "EmployeeShift_Assign": "h", "EmployeeShift_GetHistory": "h", "Holiday_List": "edhxvasm", "Holiday_Create": "h", "Holiday_Update": "h", "Holiday_Delete": "h", "WeeklyOffSetting_Get": "edhxvasm", "WeeklyOffSetting_Update": "h", "Attendance_GetMy": "edhm", "Attendance_GetTeam": "d", "Attendance_GetByEmployee": "dh", "Attendance_GetDayDetail": "edhm", "AttendanceDevice_IngestPunches": "", "AttendanceImport_Create": "h", "AttendanceImport_List": "h", "AttendanceImport_Get": "h", "AttendanceSettings_Get": "edhm", "AttendanceSettings_Update": "m", "AttendanceDevice_List": "mh", "AttendanceDevice_Create": "m", "AttendanceDevice_Update": "m", "AttendanceDevice_RotateKey": "m", "DeviceUserMap_List": "mh", "DeviceUserMap_Create": "mh", "DeviceUserMap_Delete": "mh", "DeviceUserMap_ListUnmapped": "mh", "AttendanceCorrection_Create": "edhm", "AttendanceCorrection_Get": "edhxm"};
const NAVR={"att": "edhxvasm", "leave": "edhxm", "pay": "ehxa", "perf": "edhvm", "doc": "edhm", "rep": "dhv", "ann": "edhxvasm", "dept": "h", "rec": "", "emp": "h", "acc": "hxm", "team": "dm", "set": "hm", "dash": "edhxvasm", "ds": "edhxvasm"};
const T6={ar:{
attT:'الحضور والانصراف',attSub:'الدوام والحضور والعطل والأجهزة',
tMy:'حضوري',tTeam:'حضور الفريق',tEmp:'حضور موظف',tCorr:'طلبات التصحيح',tShifts:'قوالب الدوام',tHol:'العطل',tImp:'استيراد الحضور',tDev:'أجهزة البصمة',tMeth:'طريقة الحضور',
cDate:'التاريخ',cShift:'الدوام',cSched:'المجدول',cIn:'الدخول',cOut:'الخروج',cLate:'التأخر (د)',cWorked:'ساعات العمل',cOT:'إضافي (د)',cCorr:'مصحح',cDays:'الأيام',cGrace:'السماح (د)',cOtp:'العمل الإضافي',cCyc:'دورة الأيام',cAssigned:'الموظفون',cDev:'الجهاز',cTot:'الصفوف',cAccd:'مقبول',cDup:'مكرر',cUnm:'غير مربوط',cErr:'أخطاء',cSerial:'الرقم التسلسلي',cSeen:'آخر اتصال',cMapped:'مربوط',cUnmapped:'غير مربوط',cDevUser:'معرّف جهاز',cPunches:'عدد البصمات',cFirst:'أول بصمة',cLast:'آخر بصمة',cPunchT:'نوع',cReq:'الطلب',cVal:'القيمة',cSrc:'المصدر',cRow:'الصف',cCol:'العمود',cCode:'الرمز',cMsg:'الرسالة',cStart:'البداية',cEnd:'النهاية',
fFrom:'من تاريخ',fTo:'إلى تاريخ',fEmp:'الموظف',fOn:'مفعّل',
dst:{present:'حاضر',late:'متأخر',absent:'غائب',incomplete:'ناقص',onLeave:'في إجازة',holiday:'عطلة',weeklyOff:'إجازة أسبوعية',noShift:'بلا دوام'},
styp:{fixed:'ثابت',rotating:'متناوب',overnight:'ليلي'},otp:{none:'بلا عمل إضافي',afterShiftEnd:'بعد نهاية الوردية',afterDailyHours:'بعد عدد ساعات'},mth:{device:'بصمة',gps:'GPS',both:'الاثنان'},
ist:{queued:'في الانتظار',processing:'قيد المعالجة',completed:'مكتمل',completedWithErrors:'مكتمل بأخطاء',failed:'فشل'},
ierr:{missingColumn:'عمود ناقص',invalidDateTime:'تاريخ/وقت غير صالح',emptyDeviceUserId:'معرّف فارغ',futurePunch:'بصمة مستقبلية'},
psrc:{device:'جهاز',file:'ملف',gps:'GPS'},ptyp:{checkIn:'دخول',checkOut:'خروج'},
rst:{pendingManager:'بانتظار مدير القسم',pendingHr:'بانتظار الموارد البشرية',pendingExecutive:'بانتظار المدير التنفيذي',pendingDestination:'بانتظار الجهة المنفذة',approved:'موافق عليه',rejected:'مرفوض',completed:'مكتمل',cancelled:'ملغى'},
wd:{sunday:'الأحد',monday:'الاثنين',tuesday:'الثلاثاء',wednesday:'الأربعاء',thursday:'الخميس',friday:'الجمعة',saturday:'السبت'},
aDetail:'التفاصيل',aReq:'طلب تصحيح',aReqEmp:'تصحيح لموظف',aNewShift:'قالب جديد',aEditShift:'تعديل',aAssign:'ربط موظف بدوام',aHist:'سجل الدوام',aNewHol:'عطلة جديدة',aNewImp:'استيراد ملف',aNewDev:'جهاز جديد',aRotate:'تدوير المفتاح',aMap:'ربط المستخدمين',aAddMap:'ربط مستخدم',aSaveW:'حفظ الإجازة الأسبوعية',aRefresh:'تحديث',aMethSave:'حفظ الطريقة',aMapBtn:'ربط',aBack:'رجوع',
dShiftN:'قالب دوام جديد',dShiftE:'تعديل قالب الدوام',dAssign:'ربط موظف بدوام',dHol:'عطلة',dImp:'استيراد ملف حضور',dImpD:'تفاصيل الاستيراد',dDev:'جهاز جديد',dDevE:'تعديل الجهاز',dKey:'مفتاح الجهاز',dKeyB:'يُعرض مرة واحدة فقط. ضعه في وسيط البصمة.',dRot:'تدوير مفتاح الجهاز',dRotB:'سيتوقف المفتاح القديم عن العمل.',dMapN:'ربط مستخدم جهاز بموظف',dCorr:'طلب تصحيح بصمة',dCorrD:'يُنشأ طلب تمر موافقته عبر مسار الطلبات.',
lGrace:'السماح بالتأخير (دقيقة)',lOtp:'احتساب العمل الإضافي',lOtH:'ساعات العمل اليومية',lCyc:'طول دورة التناوب (أيام)',lWork:'أيام الدوام',lCycD:'أرقام أيام العمل في الدورة (مثل 1,2)',lStart:'وقت البداية HH:mm',lEnd:'وقت النهاية HH:mm',lActive:'الحالة',lPunch:'نوع البصمة',lTime:'الوقت الصحيح HH:mm',lDate:'التاريخ',lDevice:'الجهاز',lFile:'اسم الملف (csv أو xlsx أو txt)',lDevUser:'معرّف مستخدم الجهاز',lSerial:'الرقم التسلسلي',lDevName:'اسم الجهاز',lMethod:'الطريقة',lTz:'المنطقة الزمنية',lShiftS:'الدوام',lStartD:'تاريخ البداية',
hTime:'صيغة HH:mm',hTz:'مثل Asia/Baghdad',hFileM:'تجريبي: اكتب اسماً فيه bad لتجربة الأخطاء',
emAtt:'لا توجد أيام ضمن هذه الفترة',emShift:'لا توجد قوالب دوام',emHol:'لا توجد عطل',emImp:'لا توجد عمليات استيراد',emDev:'لا توجد أجهزة',emMap:'لا توجد روابط',emUn:'لا يوجد مستخدمون غير مربوطين',emCorr:'لم تُرفع طلبات تصحيح في هذه الجلسة',emHist:'لا يوجد سجل',
nExec:'المدير التنفيذي غير مشمول بالحضور.',nCorr:'لا توجد عملية قائمة لطلبات التصحيح في العقد؛ تظهر هنا طلبات هذه الجلسة. الموافقة تتم في وحدة الطلبات.',nGps:'GPS غير متاح حتى إطلاق الميزة.',nTz:'الأوقات تُعرض بتوقيت الشركة.',nRo:'للقراءة فقط لدورك.',nWk:'الإجازة الأسبوعية',nMapT:'ربط مستخدمي الجهاز',nUnT:'مستخدمون غير مربوطين',nHistT:'سجل دوام الموظف',
kDay:'اليوم',kOrigIn:'الدخول الأصلي',kOrigOut:'الخروج الأصلي',kRaw:'البصمات الخام',kCorrs:'سجل التصحيحات',kCorrOld:'القيمة القديمة',kCorrNew:'القيمة الجديدة',kBy:'بواسطة',
kMethod:'الطريقة',kGps:'GPS متاح',kTz:'المنطقة الزمنية',kCreated2:'أُنشئ',
tShiftSaved:'تم حفظ القالب (تجريبي)',tImpDone:'اكتمل الاستيراد (تجريبي)',tCorrSent:'رُفع طلب التصحيح (تجريبي)',tAssigned2:'تم ربط الدوام (تجريبي)',tKeyRot:'دُوّر المفتاح (تجريبي)',
fx:{title:'بحث عن مستخدم',sub:'للمدير التنفيذي: ابحث ثم أعد تعيين كلمة السر أو التحقق بخطوتين',srch:'ابحث بالاسم',emp:'اكتب اسماً للبحث',act1:'إعادة تعيين كلمة السر',act2:'إعادة ضبط التحقق بخطوتين'},
er2:{'Hr:Shift:InvalidSchedule':'جدول الدوام غير صالح','Hr:Shift:CrossesMidnightMismatch':'علامة تجاوز منتصف الليل لا تطابق الأوقات','Hr:Shift:ShiftInactive':'هذا الدوام غير مفعّل','Hr:Shift:StartDateBeforeHireDate':'تاريخ البداية قبل تاريخ المباشرة','Hr:Shift:StartDateBeforeCurrentAssignment':'تاريخ البداية قبل بداية الربط الحالي','Hr:Shift:NotFound':'الدوام غير موجود','Hr:Attendance:ExecutiveDirectorNotCovered':'المدير التنفيذي غير مشمول بالحضور','Hr:Attendance:CorrectionDateInFuture':'لا يمكن تصحيح تاريخ مستقبلي','Hr:Attendance:CorrectionAlreadyPending':'يوجد طلب تصحيح قائم لنفس اليوم والنوع','Hr:Attendance:CorrectionTimeInvalid':'وقت التصحيح غير صالح لهذا اليوم','Hr:Attendance:ImportFileInvalid':'ملف الاستيراد غير صالح','Hr:Attendance:DeviceInactive':'الجهاز غير مفعّل','Hr:Attendance:GpsNotAvailable':'GPS غير متاح بعد','Hr:Attendance:NotFound':'غير موجود'}},
en:{
attT:'Attendance',attSub:'Shifts, attendance, holidays and devices',
tMy:'My attendance',tTeam:'Team attendance',tEmp:'Employee attendance',tCorr:'Correction requests',tShifts:'Shift templates',tHol:'Holidays',tImp:'Attendance import',tDev:'Devices',tMeth:'Attendance method',
cDate:'Date',cShift:'Shift',cSched:'Scheduled',cIn:'Check-in',cOut:'Check-out',cLate:'Late (min)',cWorked:'Worked',cOT:'Overtime (min)',cCorr:'Corrected',cDays:'Days',cGrace:'Grace (min)',cOtp:'Overtime',cCyc:'Cycle',cAssigned:'Employees',cDev:'Device',cTot:'Rows',cAccd:'Accepted',cDup:'Duplicates',cUnm:'Unmapped',cErr:'Errors',cSerial:'Serial number',cSeen:'Last seen',cMapped:'Mapped',cUnmapped:'Unmapped',cDevUser:'Device user',cPunches:'Punches',cFirst:'First punch',cLast:'Last punch',cPunchT:'Type',cReq:'Request',cVal:'Value',cSrc:'Source',cRow:'Row',cCol:'Column',cCode:'Code',cMsg:'Message',cStart:'Start',cEnd:'End',
fFrom:'From date',fTo:'To date',fEmp:'Employee',fOn:'Active',
dst:{present:'Present',late:'Late',absent:'Absent',incomplete:'Incomplete',onLeave:'On leave',holiday:'Holiday',weeklyOff:'Weekly off',noShift:'No shift'},
styp:{fixed:'Fixed',rotating:'Rotating',overnight:'Overnight'},otp:{none:'No overtime',afterShiftEnd:'After shift end',afterDailyHours:'After daily hours'},mth:{device:'Fingerprint device',gps:'GPS',both:'Both'},
ist:{queued:'Queued',processing:'Processing',completed:'Completed',completedWithErrors:'Completed with errors',failed:'Failed'},
ierr:{missingColumn:'Missing column',invalidDateTime:'Invalid date/time',emptyDeviceUserId:'Empty device user id',futurePunch:'Future punch'},
psrc:{device:'Device',file:'File',gps:'GPS'},ptyp:{checkIn:'Check-in',checkOut:'Check-out'},
rst:{pendingManager:'Waiting for manager',pendingHr:'Waiting for HR',pendingExecutive:'Waiting for executive',pendingDestination:'Waiting for destination',approved:'Approved',rejected:'Rejected',completed:'Completed',cancelled:'Cancelled'},
wd:{sunday:'Sunday',monday:'Monday',tuesday:'Tuesday',wednesday:'Wednesday',thursday:'Thursday',friday:'Friday',saturday:'Saturday'},
aDetail:'Details',aReq:'Request correction',aReqEmp:'Correct for employee',aNewShift:'New template',aEditShift:'Edit',aAssign:'Assign employee',aHist:'Shift history',aNewHol:'New holiday',aNewImp:'Import file',aNewDev:'New device',aRotate:'Rotate key',aMap:'User mapping',aAddMap:'Map user',aSaveW:'Save weekly off',aRefresh:'Refresh',aMethSave:'Save method',aMapBtn:'Map',aBack:'Back',
dShiftN:'New shift template',dShiftE:'Edit shift template',dAssign:'Assign employee to shift',dHol:'Holiday',dImp:'Import attendance file',dImpD:'Import details',dDev:'New device',dDevE:'Edit device',dKey:'Device key',dKeyB:'Shown only once. Put it in the fingerprint agent.',dRot:'Rotate device key',dRotB:'The old key stops working.',dMapN:'Map device user to employee',dCorr:'Punch correction request',dCorrD:'Creates a request that is approved through the requests workflow.',
lGrace:'Grace minutes',lOtp:'Overtime calculation',lOtH:'Daily working hours',lCyc:'Rotation cycle (days)',lWork:'Working days',lCycD:'Working cycle day numbers (e.g. 1,2)',lStart:'Start time HH:mm',lEnd:'End time HH:mm',lActive:'Status',lPunch:'Punch type',lTime:'Correct time HH:mm',lDate:'Date',lDevice:'Device',lFile:'File name (csv, xlsx or txt)',lDevUser:'Device user id',lSerial:'Serial number',lDevName:'Device name',lMethod:'Method',lTz:'Time zone',lShiftS:'Shift',lStartD:'Start date',
hTime:'Format HH:mm',hTz:'e.g. Asia/Baghdad',hFileM:'Mock: use a name containing "bad" to try errors',
emAtt:'No days in this period',emShift:'No shift templates',emHol:'No holidays',emImp:'No imports',emDev:'No devices',emMap:'No mappings',emUn:'No unmapped users',emCorr:'No correction requests in this session',emHist:'No history',
nExec:'The executive director is not covered by attendance.',nCorr:'The contract has no list operation for correction requests; this session’s requests appear here. Approval happens in the Requests module.',nGps:'GPS is not available until the feature ships.',nTz:'Times are shown in the company time zone.',nRo:'Read-only for your role.',nWk:'Weekly off',nMapT:'Device user mapping',nUnT:'Unmapped users',nHistT:'Employee shift history',
kDay:'Day',kOrigIn:'Original check-in',kOrigOut:'Original check-out',kRaw:'Raw punches',kCorrs:'Correction history',kCorrOld:'Old value',kCorrNew:'New value',kBy:'By',
kMethod:'Method',kGps:'GPS available',kTz:'Time zone',kCreated2:'Created',
tShiftSaved:'Template saved (mock)',tImpDone:'Import finished (mock)',tCorrSent:'Correction request submitted (mock)',tAssigned2:'Shift assigned (mock)',tKeyRot:'Key rotated (mock)',
fx:{title:'Find a user',sub:'For the executive director: search, then reset a password or two-step verification',srch:'Search by name',emp:'Type a name to search',act1:'Reset password',act2:'Reset MFA'},
er2:{'Hr:Shift:InvalidSchedule':'The shift schedule is not valid','Hr:Shift:CrossesMidnightMismatch':'The crosses-midnight flag does not match the times','Hr:Shift:ShiftInactive':'This shift is inactive','Hr:Shift:StartDateBeforeHireDate':'The start date is before the hire date','Hr:Shift:StartDateBeforeCurrentAssignment':'The start date is before the current assignment','Hr:Shift:NotFound':'Shift not found','Hr:Attendance:ExecutiveDirectorNotCovered':'The executive director is not covered by attendance','Hr:Attendance:CorrectionDateInFuture':'A future date cannot be corrected','Hr:Attendance:CorrectionAlreadyPending':'A correction for the same day and type is already pending','Hr:Attendance:CorrectionTimeInvalid':'The correction time is not valid for this day','Hr:Attendance:ImportFileInvalid':'The import file is not valid','Hr:Attendance:DeviceInactive':'The device is inactive','Hr:Attendance:GpsNotAvailable':'GPS is not available yet','Hr:Attendance:NotFound':'Not found'}}};
Object.keys(T6).forEach(l=>{Object.assign(T4[l].er2,T6[l].er2);const o=Object.assign({},T6[l]);delete o.er2;Object.assign(T4[l],o)});
PS.xd={u:'l.obaidi',dn:['ليث العبيدي','Layth Al-Obaidi'],roles:['executiveDirector'],dept:['العمليات','Operations'],jt:['المدير التنفيذي','Executive Director'],hire:'2019-01-01',mg:null,num:'E-1000',g:'male',dr:0,must:false,mfaR:true,mfaE:true,id:'07',inb:[]};
PS.rv={u:'r.hilli',dn:['رغد الحلي','Raghad Al-Hilli'],roles:['reviewer'],dept:['المالية','Finance'],jt:['مراجعة','Reviewer'],hire:'2022-04-01',mg:['حسن علي','Hassan Ali'],num:'E-1051',g:'female',dr:0,must:false,mfaR:false,mfaE:false,id:'08',inb:[]};
PS.so={u:'a.zubaidi',dn:['أحمد الزبيدي','Ahmed Al-Zubaidi'],roles:['storeOfficer'],dept:['العمليات','Operations'],jt:['مسؤول مخزن','Store Officer'],hire:'2023-10-01',mg:['حسن علي','Hassan Ali'],num:'E-1050',g:'male',dr:0,must:false,mfaR:false,mfaE:false,id:'09',inb:[['stockIssue',2,0]]};
T2.ar.skipT='تخطي إلى المحتوى';T2.en.skipT='Skip to content';T2.ar.mainNavT='التنقل الرئيسي';T2.en.mainNavT='Main navigation';T2.ar.rsT='إعادة ضبط البيانات التجريبية';T2.en.rsT='Reset demo data';T2.ar.rsD='أُعيد ضبط البيانات التجريبية';T2.en.rsD='Demo data was reset';T2.ar.rsH='تبقى البيانات التجريبية بين المستخدمين لتجربة المسارات (مثلاً: يرفع موظف وتوافق مديره).';T2.en.rsH='Demo data persists across users so you can follow a workflow (e.g. an employee submits and the manager approves).';
T2.ar.pl.xd=['المدير التنفيذي','يبحث عن مستخدم ويعيد التعيين'];T2.en.pl.xd=['Executive director','Finds a user and resets credentials'];
T2.ar.pl.rv=['المراجع','تقارير بلا أسماء'];T2.en.pl.rv=['Reviewer','Counts without names'];
T2.ar.pl.so=['مسؤول المخزن','يرى الموظف بالاسم والقسم فقط'];T2.en.pl.so=['Store officer','Sees employee name and department only'];
const OPR4={"LeaveBalance_GetMy": "edhm", "LeaveBalance_GetByEmployee": "dhmx", "LeaveBalance_SetEntitlement": "h", "LeaveBalance_Adjust": "h", "LeaveLedger_GetByEmployee": "dhmx", "LeaveLedger_GetMy": "edhm", "LeaveRequest_Preview": "edhm", "LeaveRequest_Create": "edhm", "LeaveRequest_ListMy": "edhm", "LeaveRequest_ListByEmployee": "dh", "LeaveRequest_Get": "edhxm", "LeaveRequest_UploadAttachment": "edhm", "LeaveRequest_GetAttachmentDownloadUrl": "edhxm", "OtherLeaveType_List": "m", "OtherLeaveType_Create": "m", "OtherLeaveType_Update": "m", "OtherLeaveType_ListAvailable": "edhm", "LeaveCalendar_GetTeam": "d", "SystemSettings_GetHourlyLeave": "m", "SystemSettings_UpdateHourlyLeave": "m"};
const T7={ar:{
lvT:'الإجازات',lvSub:'الأرصدة وطلبات الإجازة والتقويم',
tLBal:'أرصدتي',tLMy:'طلبات إجازتي',tLEmp:'أرصدة موظف',tLCal:'تقويم الفريق',tLTypes:'أنواع الإجازات الأخرى',tLHourly:'إعدادات الزمنيات',
lk:{hourly:'زمنية',annual:'من الرصيد',other:'أخرى'},be:{none:'بلا أثر',held:'محجوز',deducted:'مخصوم',released:'مفكوك'},
cls:{workingDay:'يوم دوام',weeklyOff:'راحة أسبوعية',holiday:'عطلة'},lt:{grant:'منح',carryIn:'ترحيل',hold:'حجز',release:'فك حجز',deduct:'خصم',adjust:'تعديل',reverse:'عكس'},lb:{carried:'المضاف',entitlement:'الاستحقاق',hourly:'الزمنية'},cat:{all:'الجميع',male:'ذكور',female:'إناث'},
bYear:'سنة الإجازة',bEnt:'استحقاق السنة',bCar:'الأيام المضافة',bAdj:'التعديلات',bUsed:'المستخدم',bHeld:'المحجوز',bRem:'المتبقي',bCarRem:'المتبقي من المضاف',bCurRem:'المتبقي من السنة',bFut:'محجوز من السنة القادمة',bNext:'استحقاق السنة القادمة',bNotSet:'لم يُحدد استحقاق هذا الموظف بعد.',bDays:'يوم',
hT:'الزمنية هذا الشهر',hQ:'الحصة (ساعة)',hU:'المستخدم',hH:'المحجوز',hR:'المتبقي',hPD:'ساعات اليوم',hNC:'لم تُضبط إعدادات الزمنيات.',
oT:'الأنواع الأخرى',oU:'المستخدم',oH:'المحجوز',oM:'الحد',oR:'المتبقي',lgT:'سجل الحركات',lgDays:'الأيام',lgBucket:'الوعاء',lgReason:'السبب',lgType:'الحركة',
cReqDate:'التاريخ',cKind:'النوع',cType:'نوع الإجازة',cWD:'أيام الدوام',cHours:'الساعات',cEffect:'أثر الرصيد',cCreated:'أُنشئ',cTimes:'الوقت',
aNewReq:'طلب إجازة جديد',aEntitle:'تحديد الاستحقاق',aAdjust:'تعديل الرصيد',aNewType:'نوع جديد',aSaveH:'حفظ إعدادات الزمنيات',aPreview:'احسب',aSubmit:'إرسال الطلب',
w1:'نوع الإجازة',w2:'التفاصيل',w3:'المراجعة والإرسال',wKind:'اختر نوع الإجازة',wFor:'لموظف من فريقي (اختياري)',
fKind:'النوع',fDate:'التاريخ',fFromT:'من الساعة HH:mm',fToT:'إلى الساعة HH:mm',fStartD:'من تاريخ',fEndD:'إلى تاريخ',fOther:'نوع الإجازة الأخرى',fNotes:'ملاحظات',fAtt:'المرفقات',fAttH:'PDF أو JPEG أو PNG حتى 10 ميجابايت',fInc:'تشمل المعلّق',
pvT:'نتيجة الحساب من الخادم',pvCal:'أيام تقويمية',pvWork:'أيام دوام محسوبة',pvWo:'راحة أسبوعية مستثناة',pvHol:'عطل مستثناة',pvHrs:'الساعات المطلوبة',pvQ:'من حصة الشهر',pvEx:'زيادة تُخصم من السنوي',pvAnn:'أيام تُخصم من السنوي',pvFC:'من المضاف',pvFE:'من الاستحقاق',pvBef:'المتاح قبل',pvAft:'المتبقي بعد',pvPaid:'مدفوعة',pvAtt:'تتطلب مرفقاً',pvSeg:'التوزيع على السنوات',pvBr:'تفصيل الأيام',pvShort:'نقص الرصيد (أيام)',pvOk:'الرصيد كافٍ',pvNo:'لا يمكن الإرسال',pvFut:'سنة قادمة',pvCnt:'محسوب',
dEnt:'تحديد الاستحقاق',dEntB:'يبدأ التغيير من سنة الموظف القادمة ولا يغير السنة الحالية. أول تحديد يفتح سنته الحالية.',dAdj:'تعديل الرصيد',dAdjB:'أضف أيامًا بقيمة موجبة أو اخصم بقيمة سالبة، مع سبب إلزامي.',fDaysN:'عدد الأيام',fDaysA:'الأيام (موجب أو سالب)',fRsn:'السبب',
dType:'نوع إجازة',fNameAr:'الاسم بالعربية',fNameEn:'الاسم بالإنجليزية',fPaid:'مدفوعة',fMax:'الحد الأقصى (أيام لكل سنة)',fReqAtt:'تتطلب مرفقاً',fCat:'الفئة المستحقة',fDed:'تخصم من الرصيد السنوي',fAct:'الحالة',
hsT:'إعدادات الزمنيات',fMonthly:'الساعات المسموحة في الشهر',fHpd:'ساعات اليوم',nHS:'تُحوَّل الزيادة عن الحصة إلى أيام بقسمتها على ساعات اليوم.',
rdT:'تفاصيل طلب الإجازة',rdFor:'الموظف',rdBy:'المقدّم',rdOnb:'نيابة عن الموظف',rdNotes:'ملاحظات',rdAtt:'المرفقات',
emL:'لا توجد طلبات',emC:'لا توجد إجازات في هذه الفترة',emTy:'لا توجد أنواع',emLg:'لا توجد حركات',nExecL:'المدير التنفيذي غير مشمول بالإجازات.',nPick:'اختر موظفاً لعرض رصيده.',nCalR:'اختر فترة التقويم.',nRoL:'للقراءة فقط لدورك.',
tLSub:'أُرسل طلب الإجازة (تجريبي)',tEnt:'حُفظ الاستحقاق (تجريبي)',tAdj:'عُدّل الرصيد (تجريبي)',
er2:{'Hr:Leave:ExecutiveDirectorNotEligible':'المدير التنفيذي غير مشمول بالإجازات','Hr:Leave:EmployeeNotActive':'الموظف غير نشط','Hr:Leave:NoShiftAssigned':'لا يوجد دوام مرتبط بالموظف','Hr:Leave:InvalidDateRange':'الفترة أو الأوقات غير صالحة','Hr:Leave:EntitlementNotSet':'لم يُحدد استحقاق الموظف','Hr:Leave:NoWorkingDaysInRange':'لا توجد أيام دوام ضمن الفترة','Hr:Leave:RangeBeyondNextYear':'الفترة تتجاوز سنة الموظف القادمة','Hr:Leave:InsufficientBalance':'الرصيد لا يكفي','Hr:Leave:OverlappingLeave':'تتداخل مع إجازة أخرى','Hr:Leave:HourlySettingsNotConfigured':'إعدادات الزمنيات غير مضبوطة','Hr:Leave:HourlyOutsideShift':'الوقت خارج ساعات الدوام','Hr:Leave:OtherLeaveTypeInactive':'نوع الإجازة غير مفعّل','Hr:Leave:NotEligibleForLeaveType':'الموظف غير مؤهل لهذا النوع','Hr:Leave:OtherLeaveTypeMaxExceeded':'تجاوز الحد الأقصى للنوع','Hr:Leave:AttachmentRequired':'هذا النوع يتطلب مرفقاً','Hr:Leave:InvalidAttachment':'مرفق غير صالح','Hr:Leave:AttachmentRejected':'رُفض المرفق (النوع أو الحجم)','Hr:Leave:SelfAdjustmentNotAllowed':'لا يعدل أحد بياناته بنفسه','Hr:Leave:AdjustmentExceedsBalance':'الخصم أكبر من الرصيد المتاح','Hr:Leave:NotFound':'غير موجود'}},
en:{
lvT:'Leave',lvSub:'Balances, leave requests and calendar',
tLBal:'My balances',tLMy:'My leave requests',tLEmp:'Employee balances',tLCal:'Team calendar',tLTypes:'Other leave types',tLHourly:'Hourly leave settings',
lk:{hourly:'Hourly',annual:'From balance',other:'Other'},be:{none:'No effect',held:'Held',deducted:'Deducted',released:'Released'},
cls:{workingDay:'Working day',weeklyOff:'Weekly off',holiday:'Holiday'},lt:{grant:'Grant',carryIn:'Carry-in',hold:'Hold',release:'Release',deduct:'Deduct',adjust:'Adjust',reverse:'Reverse'},lb:{carried:'Carried',entitlement:'Entitlement',hourly:'Hourly'},cat:{all:'Everyone',male:'Male',female:'Female'},
bYear:'Leave year',bEnt:'Year entitlement',bCar:'Carried days',bAdj:'Adjustments',bUsed:'Used',bHeld:'Held',bRem:'Remaining',bCarRem:'Carried remaining',bCurRem:'Year remaining',bFut:'Held from next year',bNext:'Next-year entitlement',bNotSet:'This employee’s entitlement has not been set yet.',bDays:'days',
hT:'Hourly leave this month',hQ:'Quota (h)',hU:'Used',hH:'Held',hR:'Remaining',hPD:'Hours per day',hNC:'Hourly leave settings are not configured.',
oT:'Other leave types',oU:'Used',oH:'Held',oM:'Limit',oR:'Remaining',lgT:'Ledger',lgDays:'Days',lgBucket:'Bucket',lgReason:'Reason',lgType:'Entry',
cReqDate:'Date',cKind:'Kind',cType:'Leave type',cWD:'Working days',cHours:'Hours',cEffect:'Balance effect',cCreated:'Created',cTimes:'Time',
aNewReq:'New leave request',aEntitle:'Set entitlement',aAdjust:'Adjust balance',aNewType:'New type',aSaveH:'Save hourly settings',aPreview:'Calculate',aSubmit:'Submit request',
w1:'Leave kind',w2:'Details',w3:'Review and submit',wKind:'Choose the leave kind',wFor:'For a team member (optional)',
fKind:'Kind',fDate:'Date',fFromT:'From time HH:mm',fToT:'To time HH:mm',fStartD:'From date',fEndD:'To date',fOther:'Other leave type',fNotes:'Notes',fAtt:'Attachments',fAttH:'PDF, JPEG or PNG up to 10 MB',fInc:'Include pending',
pvT:'Server calculation',pvCal:'Calendar days',pvWork:'Counted working days',pvWo:'Weekly-off days excluded',pvHol:'Holidays excluded',pvHrs:'Requested hours',pvQ:'From monthly quota',pvEx:'Excess deducted from annual',pvAnn:'Days deducted from annual',pvFC:'From carried',pvFE:'From entitlement',pvBef:'Available before',pvAft:'Remaining after',pvPaid:'Paid',pvAtt:'Requires attachment',pvSeg:'Split across leave years',pvBr:'Day breakdown',pvShort:'Shortage (days)',pvOk:'Balance is sufficient',pvNo:'Cannot be submitted',pvFut:'Next year',pvCnt:'Counted',
dEnt:'Set entitlement',dEntB:'The change starts from the employee’s next leave year and does not change the current year. A first entitlement opens the current year.',dAdj:'Adjust balance',dAdjB:'Add days with a positive value or deduct with a negative value; a reason is mandatory.',fDaysN:'Number of days',fDaysA:'Days (positive or negative)',fRsn:'Reason',
dType:'Leave type',fNameAr:'Name (Arabic)',fNameEn:'Name (English)',fPaid:'Paid',fMax:'Maximum (days per year)',fReqAtt:'Requires attachment',fCat:'Eligible category',fDed:'Deducts from annual balance',fAct:'Status',
hsT:'Hourly leave settings',fMonthly:'Allowed hours per month',fHpd:'Hours per day',nHS:'Hours beyond the quota are converted to days by dividing by hours per day.',
rdT:'Leave request details',rdFor:'Employee',rdBy:'Submitted by',rdOnb:'On behalf of employee',rdNotes:'Notes',rdAtt:'Attachments',
emL:'No requests',emC:'No leave in this period',emTy:'No types',emLg:'No ledger entries',nExecL:'The executive director is not covered by leave.',nPick:'Choose an employee to see their balance.',nCalR:'Choose the calendar period.',nRoL:'Read-only for your role.',
tLSub:'Leave request submitted (mock)',tEnt:'Entitlement saved (mock)',tAdj:'Balance adjusted (mock)',
er2:{'Hr:Leave:ExecutiveDirectorNotEligible':'The executive director is not covered by leave','Hr:Leave:EmployeeNotActive':'The employee is not active','Hr:Leave:NoShiftAssigned':'The employee has no shift','Hr:Leave:InvalidDateRange':'The period or times are not valid','Hr:Leave:EntitlementNotSet':'The employee’s entitlement is not set','Hr:Leave:NoWorkingDaysInRange':'There are no working days in the range','Hr:Leave:RangeBeyondNextYear':'The range goes beyond the employee’s next leave year','Hr:Leave:InsufficientBalance':'The balance is not sufficient','Hr:Leave:OverlappingLeave':'Overlaps with another leave','Hr:Leave:HourlySettingsNotConfigured':'Hourly leave settings are not configured','Hr:Leave:HourlyOutsideShift':'The time is outside shift hours','Hr:Leave:OtherLeaveTypeInactive':'The leave type is inactive','Hr:Leave:NotEligibleForLeaveType':'The employee is not eligible for this type','Hr:Leave:OtherLeaveTypeMaxExceeded':'The type’s maximum is exceeded','Hr:Leave:AttachmentRequired':'This type requires an attachment','Hr:Leave:InvalidAttachment':'Invalid attachment','Hr:Leave:AttachmentRejected':'The attachment was rejected (type or size)','Hr:Leave:SelfAdjustmentNotAllowed':'Nobody may change their own data','Hr:Leave:AdjustmentExceedsBalance':'The deduction exceeds the available balance','Hr:Leave:NotFound':'Not found'}}};
Object.keys(T7).forEach(l=>{Object.assign(T4[l].er2,T7[l].er2);const o=Object.assign({},T7[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined&&typeof T4[l][k]!=='object')throw new Error('T7 key clash '+k);});Object.assign(T4[l],o)});
const OPR5={"Delegation_List": "h", "Delegation_Create": "dhxma", "Delegation_GetMy": "edhxvasm", "Delegation_GetCandidates": "dhxma", "Delegation_Cancel": "dhxma", "RequestType_GetList": "edhxvasm", "RequestType_Create": "m", "RequestType_GetAdminList": "m", "RequestType_Get": "m", "RequestType_Update": "m", "Request_Submit": "edhxvasm", "Request_GetList": "hd", "Request_GetMy": "edhxvasm", "Request_GetInbox": "dhxmas", "Request_Get": "edhxvasm", "Request_Approve": "dhxma", "Request_Reject": "dhxmas", "Request_Cancel": "edhxvasm", "Memo_Create": "edhvasm", "Memo_GetMy": "edhvasm", "Memo_GetInbox": "dhxm", "Memo_Get": "edhxvasm", "Memo_Forward": "dmx", "Memo_Convert": "dhxm", "Memo_Close": "hx", "Incident_Create": "edhxvasm", "Incident_GetMy": "edhxvasm", "Incident_GetInbox": "dh", "Incident_Get": "edhxvasm", "Incident_UpdateStatus": "h", "Incident_Forward": "d"};
NAVR.req="edhxvasm";
const T8={ar:{
qT:'الطلبات والمسارات',qSub:'بانتظار إجرائي والطلبات والمطالعات والبلاغات والتفويض',
tQIn:'بانتظار إجرائي',tQMy:'طلباتي',tQAll:'كل الطلبات',tQMemo:'المطالعات',tQInc:'بلاغات الأعطال',tQDel:'التفويض',tQTypes:'أنواع الطلبات',
cQType:'النوع',cQEmp:'الموظف',cQBy:'المقدّم',cQSum:'التفاصيل',cQStat:'الحالة',cQRole:'المرحلة',cQWait:'منذ',cQDeleg:'عبر تفويض من',cQRej:'سبب الرفض',cQCr:'الوقت',cQOrd:'#',cQApr:'المعتمد',cQDec:'القرار',cQDBy:'قرر بواسطة',cQNote:'ملاحظة',cQAct:'الإجراء',cQKind:'النوع',cQPrev:'المحتوى',cQFrom:'من',cQTo:'إلى',cQScope:'النطاق',cQPer:'المدة',cQCode:'الرمز',cQDest:'الجهة',cQSys:'نظام',cQAct2:'مفعّل',
aQOpen:'فتح',aQNew:'طلب جديد',aQApp:'موافقة',aQRej:'رفض',aQCan:'إلغاء الطلب',aQNewM:'مطالعة جديدة',aQNewI:'بلاغ جديد',aQFwd:'تحويل للموارد البشرية',aQConv:'تحويل',aQCls:'إغلاق',aQUpd:'تحديث الحالة',aQNewD:'تفويض جديد',aQCanD:'إلغاء التفويض',aQNewT:'نوع جديد',aQBack:'رجوع',
dQNew:'طلب جديد',dQNewB:'الإجازة وتصحيح البصمة لهما شاشتان مخصصتان ولا تُرفعان من هنا.',fQType:'نوع الطلب',fQFor:'نيابة عن موظف من فريقي (اختياري)',fQDesc:'الوصف',fQNote:'ملاحظة (اختياري)',fQRsn:'سبب الرفض',dQApp:'الموافقة على الطلب',dQRej:'رفض الطلب',dQRejB:'يظهر السبب للموظف وينهي الطلب.',dQCan:'إلغاء الطلب',dQCanB:'يمكن الإلغاء قبل بدء التنفيذ فقط.',
fQKind:'نوع المطالعة',fQCont:'المحتوى',fQDescI:'وصف العطل',dQMemo:'مطالعة جديدة',dQInc:'بلاغ عطل جديد',dQIncB:'اختيار الصنف اختياري.',dQFwd:'تحويل إلى الموارد البشرية',dQConv:'تحويل المطالعة',dQCls:'إغلاق المطالعة',dQUpd:'تحديث حالة البلاغ',
fQTarget:'التحويل إلى',fQLK:'نوع الإجازة',fQLDate:'تاريخ الإجازة الزمنية',fQTgt:'الحالة الجديدة',
fQTo:'المفوَّض إليه',fQScp:'النطاق',fQSD:'تاريخ البداية',fQST:'وقت البداية HH:mm',fQED:'تاريخ النهاية',fQET:'وقت النهاية HH:mm',fQDR:'سبب التفويض',dQDel:'تفويض جديد',dQDelB:'التفويض لنفسك فقط. القرار يُسجل باسم المفوَّض مع صاحب الصلاحية.',fQCode:'الرمز (إنجليزي)',fQDst:'الجهة',dQType:'نوع طلب',
mk:{leave:'إجازة',problem:'مشكلة'},mtg:{leaveRequest:'طلب إجازة',incident:'بلاغ'},
mst:{pendingManager:'بانتظار المدير',pendingHr:'بانتظار الموارد البشرية',convertedToRequest:'حُوّلت إلى طلب',convertedToIncident:'حُوّلت إلى بلاغ',closed:'مغلقة',pendingExecutive:'بانتظار المدير التنفيذي'},
ist2:{pendingManager:'بانتظار المدير',pendingHr:'بانتظار الموارد البشرية',inProgress:'قيد المعالجة',resolved:'تم الحل'},ist2t:{inProgress:'قيد المعالجة',resolved:'تم الحل'},
dsts:{scheduled:'مجدول',active:'ساري',expired:'منتهي',cancelled:'ملغى'},dscp:{allApprovals:'كل الموافقات',requests:'الطلبات',memosAndIncidents:'المطالعات والبلاغات'},ddir:{given:'أعطيتها',received:'استلمتها'},
dec:{notReached:'لم تصل',pending:'بانتظار القرار',approved:'موافق',approvedBySubmission:'موافقة بالتقديم',rejected:'مرفوض'},dest:{none:'بلا جهة تنفيذ',store:'المخزن',accountant:'المحاسب'},
qSteps:'خطوات الموافقة',qTl:'الخط الزمني',qDet:'تفاصيل الطلب',kQDesc:'الوصف',kQAR:'المرحلة الحالية',kQMemo:'مطالعة الأصل',kQHdl:'الجهة المعالجة',kQCls:'سبب الإغلاق',kQConv:'تحويل إلى',
emQ:'لا شيء بانتظار إجراءك',emQR:'لا توجد طلبات',emQM:'لا توجد مطالعات',emQI:'لا توجد بلاغات',emQD:'لا توجد تفويضات',emQT:'لا توجد أنواع',emQS:'لا توجد خطوات',
nQDeleg:'العقد لا يتيح للموارد البشرية إنشاء تفويض نيابة عن غيرها (لا حقل لصاحب الصلاحية في الطلب).',nQMemoI:'صندوق المطالعات',nQMemoM:'مطالعاتي',nQIncI:'صندوق البلاغات',nQIncM:'بلاغاتي',nQDelA:'كل التفويضات',nQDelM:'تفويضاتي',nQExec:'المدير التنفيذي لا يرفع طلباً بلا جهة تنفيذ.',
tQSub:'رُفع الطلب (تجريبي)',tQApp:'تمت الموافقة (تجريبي)',tQRej:'تم الرفض (تجريبي)',tQCan:'أُلغي الطلب (تجريبي)',tMS:'رُفعت المطالعة (تجريبي)',tIS:'رُفع البلاغ (تجريبي)',tQFwd:'تم التحويل (تجريبي)',tQConv:'تم تحويل المطالعة (تجريبي)',tQCls:'أُغلقت المطالعة (تجريبي)',tQUpd:'حُدّثت الحالة (تجريبي)',tDS:'أُنشئ التفويض (تجريبي)',tQDelC:'أُلغي التفويض (تجريبي)',
er2:{'Hr:Request:SelfApprovalNotAllowed':'لا يعتمد أحد طلبه الشخصي','Hr:Request:NotPending':'الطلب ليس بانتظار قرار','Hr:Request:UseStockIssueFlow':'يُنفذ هذا الطلب من مسار الصرف','Hr:Request:CannotCancelAtThisStage':'لا يمكن الإلغاء في هذه المرحلة','Hr:Request:TypeInactive':'نوع الطلب غير مفعّل','Hr:Request:UseDedicatedEndpoint':'هذا النوع له شاشته المخصصة','Hr:Request:ExecutiveCannotSubmit':'المدير التنفيذي لا يرفع طلباً بلا جهة تنفيذ','Hr:Request:OnBehalfNotAllowed':'لا يجوز الرفع نيابة عن هذا الموظف','Hr:Request:ItemFieldsInvalid':'بيانات الصنف غير صالحة','Hr:Request:ItemInactive':'الصنف غير مفعّل','Hr:Request:NoApproverAvailable':'لا يوجد معتمد متاح','Hr:Request:TypeCodeAlreadyExists':'الرمز مستخدم','Hr:Request:SystemTypeDestinationLocked':'جهة النوع النظامي مقفلة','Hr:Request:NotFound':'غير موجود','Hr:Memo:NoManagerLinked':'لا يوجد مدير مرتبط بك','Hr:Memo:InvalidState':'حالة المطالعة لا تسمح بهذا الإجراء','Hr:Memo:SelfActionNotAllowed':'لا يعمل أحد على مطالعته','Hr:Memo:ConvertTargetMismatch':'نوع المطالعة لا يطابق جهة التحويل','Hr:Memo:LeaveDetailsInvalid':'تفاصيل الإجازة غير مكتملة','Hr:Memo:LeaveAttachmentRequired':'هذا النوع يتطلب مرفقاً ولا يُحوَّل من مطالعة','Hr:Memo:NotFound':'غير موجود','Hr:Incident:NoManagerLinked':'لا يوجد مدير مرتبط بك','Hr:Incident:InvalidTransition':'لا يمكن الانتقال لهذه الحالة','Hr:Incident:InvalidState':'حالة البلاغ لا تسمح بهذا الإجراء','Hr:Incident:NotFound':'غير موجود','Hr:Delegation:CannotDelegateToSelf':'لا يمكن التفويض لنفسك','Hr:Delegation:InvalidPeriod':'مدة التفويض غير صالحة','Hr:Delegation:DelegateNotEligible':'المفوَّض إليه غير مؤهل','Hr:Delegation:OverlappingDelegation':'يتداخل مع تفويض آخر لنفس النطاق','Hr:Delegation:AlreadyEnded':'انتهى التفويض مسبقاً','Hr:Delegation:NotFound':'غير موجود'}},
en:{
qT:'Requests & workflow',qSub:'Pending actions, requests, memos, fault reports and delegation',
tQIn:'Pending my action',tQMy:'My requests',tQAll:'All requests',tQMemo:'Memos',tQInc:'Fault reports',tQDel:'Delegation',tQTypes:'Request types',
cQType:'Type',cQEmp:'Employee',cQBy:'Submitted by',cQSum:'Details',cQStat:'Status',cQRole:'Stage',cQWait:'Waiting since',cQDeleg:'Via delegation from',cQRej:'Rejection reason',cQCr:'Time',cQOrd:'#',cQApr:'Approver',cQDec:'Decision',cQDBy:'Decided by',cQNote:'Note',cQAct:'Action',cQKind:'Kind',cQPrev:'Content',cQFrom:'From',cQTo:'To',cQScope:'Scope',cQPer:'Period',cQCode:'Code',cQDest:'Destination',cQSys:'System',cQAct2:'Active',
aQOpen:'Open',aQNew:'New request',aQApp:'Approve',aQRej:'Reject',aQCan:'Cancel request',aQNewM:'New memo',aQNewI:'New fault report',aQFwd:'Forward to HR',aQConv:'Convert',aQCls:'Close',aQUpd:'Update status',aQNewD:'New delegation',aQCanD:'Cancel delegation',aQNewT:'New type',aQBack:'Back',
dQNew:'New request',dQNewB:'Leave and punch correction have their own screens and are not submitted from here.',fQType:'Request type',fQFor:'On behalf of a team member (optional)',fQDesc:'Description',fQNote:'Note (optional)',fQRsn:'Rejection reason',dQApp:'Approve request',dQRej:'Reject request',dQRejB:'The reason is shown to the employee and ends the request.',dQCan:'Cancel request',dQCanB:'You can cancel only before execution starts.',
fQKind:'Memo kind',fQCont:'Content',fQDescI:'Fault description',dQMemo:'New memo',dQInc:'New fault report',dQIncB:'Choosing an item is optional.',dQFwd:'Forward to HR',dQConv:'Convert memo',dQCls:'Close memo',dQUpd:'Update fault-report status',
fQTarget:'Convert to',fQLK:'Leave kind',fQLDate:'Hourly leave date',fQTgt:'New status',
fQTo:'Delegate',fQScp:'Scope',fQSD:'Start date',fQST:'Start time HH:mm',fQED:'End date',fQET:'End time HH:mm',fQDR:'Delegation reason',dQDel:'New delegation',dQDelB:'You can delegate your own approvals only. The decision is recorded in the delegate’s name with the original holder.',fQCode:'Code (English)',fQDst:'Destination',dQType:'Request type',
mk:{leave:'Leave',problem:'Problem'},mtg:{leaveRequest:'Leave request',incident:'Fault report'},
mst:{pendingManager:'Waiting for manager',pendingHr:'Waiting for HR',convertedToRequest:'Converted to request',convertedToIncident:'Converted to fault report',closed:'Closed',pendingExecutive:'Waiting for executive'},
ist2:{pendingManager:'Waiting for manager',pendingHr:'Waiting for HR',inProgress:'In progress',resolved:'Resolved'},ist2t:{inProgress:'In progress',resolved:'Resolved'},
dsts:{scheduled:'Scheduled',active:'Active',expired:'Expired',cancelled:'Cancelled'},dscp:{allApprovals:'All approvals',requests:'Requests',memosAndIncidents:'Memos and fault reports'},ddir:{given:'Given by me',received:'Received by me'},
dec:{notReached:'Not reached',pending:'Waiting for decision',approved:'Approved',approvedBySubmission:'Approved by submission',rejected:'Rejected'},dest:{none:'No executing party',store:'Store',accountant:'Accountant'},
qSteps:'Approval steps',qTl:'Timeline',qDet:'Request details',kQDesc:'Description',kQAR:'Current stage',kQMemo:'Source memo',kQHdl:'Handled by',kQCls:'Close reason',kQConv:'Converted to',
emQ:'Nothing is waiting for your action',emQR:'No requests',emQM:'No memos',emQI:'No fault reports',emQD:'No delegations',emQT:'No types',emQS:'No steps',
nQDeleg:'The contract does not let HR create a delegation on behalf of someone else (the request has no holder field).',nQMemoI:'Memo inbox',nQMemoM:'My memos',nQIncI:'Fault-report inbox',nQIncM:'My fault reports',nQDelA:'All delegations',nQDelM:'My delegations',nQExec:'The executive director cannot submit a request without an executing party.',
tQSub:'Request submitted (mock)',tQApp:'Approved (mock)',tQRej:'Rejected (mock)',tQCan:'Request cancelled (mock)',tMS:'Memo submitted (mock)',tIS:'Fault report submitted (mock)',tQFwd:'Forwarded (mock)',tQConv:'Memo converted (mock)',tQCls:'Memo closed (mock)',tQUpd:'Status updated (mock)',tDS:'Delegation created (mock)',tQDelC:'Delegation cancelled (mock)',
er2:{'Hr:Request:SelfApprovalNotAllowed':'Nobody approves their own request','Hr:Request:NotPending':'The request is not waiting for a decision','Hr:Request:UseStockIssueFlow':'This request is executed from the stock-issue flow','Hr:Request:CannotCancelAtThisStage':'It cannot be cancelled at this stage','Hr:Request:TypeInactive':'The request type is inactive','Hr:Request:UseDedicatedEndpoint':'This type has its own screen','Hr:Request:ExecutiveCannotSubmit':'The executive director cannot submit a request without an executing party','Hr:Request:OnBehalfNotAllowed':'Submitting on behalf of this employee is not allowed','Hr:Request:ItemFieldsInvalid':'The item fields are not valid','Hr:Request:ItemInactive':'The item is inactive','Hr:Request:NoApproverAvailable':'No approver is available','Hr:Request:TypeCodeAlreadyExists':'The code is already used','Hr:Request:SystemTypeDestinationLocked':'The destination of a system type is locked','Hr:Request:NotFound':'Not found','Hr:Memo:NoManagerLinked':'You have no linked manager','Hr:Memo:InvalidState':'The memo state does not allow this action','Hr:Memo:SelfActionNotAllowed':'Nobody acts on their own memo','Hr:Memo:ConvertTargetMismatch':'The memo kind does not match the target','Hr:Memo:LeaveDetailsInvalid':'The leave details are incomplete','Hr:Memo:LeaveAttachmentRequired':'This type needs an attachment and cannot be converted from a memo','Hr:Memo:NotFound':'Not found','Hr:Incident:NoManagerLinked':'You have no linked manager','Hr:Incident:InvalidTransition':'This status transition is not allowed','Hr:Incident:InvalidState':'The fault-report state does not allow this action','Hr:Incident:NotFound':'Not found','Hr:Delegation:CannotDelegateToSelf':'You cannot delegate to yourself','Hr:Delegation:InvalidPeriod':'The delegation period is not valid','Hr:Delegation:DelegateNotEligible':'The delegate is not eligible','Hr:Delegation:OverlappingDelegation':'Overlaps another delegation for the same scope','Hr:Delegation:AlreadyEnded':'The delegation has already ended','Hr:Delegation:NotFound':'Not found'}}};
Object.keys(T8).forEach(l=>{Object.assign(T4[l].er2,T8[l].er2);const o=Object.assign({},T8[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T8 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M9 11l3 3L22 4M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11');PAGES.push('req');T.ar.nav.push('الطلبات والمسارات');T.en.nav.push('Requests & workflow');
const OPR6={"Complaint_GetRespondents": "ed", "Complaint_PreviewRouting": "ed", "Complaint_Submit": "ed", "Complaint_GetMyList": "ed", "Complaint_GetMyDetail": "ed", "Complaint_GetAssignedList": "dhx", "Complaint_GetDetail": "dhx", "Complaint_RaiseLevel": "edhx", "Complaint_AddAction": "dhx", "Complaint_Forward": "d", "Complaint_Close": "hx", "Complaint_GetAccessLog": "dhx", "Complaint_UploadAttachment": "edhx", "Complaint_DownloadAttachment": "edhx", "ComplaintHandler_Request": "h", "ComplaintHandler_GetList": "hx", "ComplaintHandler_Approve": "x", "ComplaintHandler_Reject": "x", "ComplaintHandler_Revoke": "x", "SystemSettings_GetComplaints": "m", "SystemSettings_UpdateComplaints": "m"};
NAVR.cmp="edhxm";
const T9={ar:{
zT:'الشكاوى',zSub:'الشكاوى السرية: كل وصول مخوَّل يُسجَّل، وغير المخوَّل لا يرى أي أثر',
tZMy:'شكاواي',tZIn:'صندوق الشكاوى',tZHd:'تعيين المعالجين',tZCf:'مهلة الشكاوى',
zl:{normal:'عادية',confidential:'سرية',highlyConfidential:'سرية جداً'},zs:{pendingDepartmentManager:'بانتظار مدير القسم',pendingHrManager:'بانتظار مدير الموارد البشرية',pendingExecutiveDirector:'بانتظار المدير التنفيذي',closed:'مغلقة'},zk:{employee:'ضد موظف',general:'شكوى عامة'},zc:{colleague:'زميل',departmentManager:'مدير قسم',storeOfficer:'مسؤول مخزن',hrManager:'مدير الموارد البشرية',reviewer:'مراجع'},
za:{note:'ملاحظة',meeting:'اجتماع',investigation:'تحقيق',followUp:'متابعة',submitted:'تقديم',levelRaised:'رفع المستوى',forwarded:'تحويل',closed:'إغلاق'},zal:{viewDetail:'اطلاع على التفاصيل',downloadAttachment:'تنزيل مرفق',uploadAttachment:'رفع مرفق',viewAccessLog:'اطلاع على السجل',addAction:'إضافة إجراء',raiseLevel:'رفع المستوى',forward:'تحويل',close:'إغلاق'},zas:{pendingApproval:'بانتظار الموافقة',approved:'معتمد',rejected:'مرفوض',revoked:'مسحوب'},
cZRef:'الرقم المرجعي',cZLvl:'المستوى',cZStat:'الحالة',cZSub:'الموضوع',cZAt:'تاريخ التقديم',cZDue:'الموعد',cZLate:'متأخرة',cZClosed:'أُغلقت',cZUser:'المستخدم',cZFrom:'من',cZTo:'إلى',cZReason:'السبب',cZReq:'قدّمه',cZDec:'قرار',cZAct:'الإجراء',cZNote:'الملاحظة',cZActor:'بواسطة',cZTime:'الوقت',cZFile:'الملف',cZSize:'الحجم',cZPath:'المسار',cZMin:'الحد الأدنى',
aZNew:'شكوى جديدة',aZOpen:'فتح',aZRaise:'رفع المستوى',aZAct:'إضافة إجراء',aZFwd:'تحويل لمدير الموارد البشرية',aZClose:'إغلاق الشكوى',aZLog:'سجل الاطلاع',aZDl:'تنزيل',aZBack:'رجوع',aZReqH:'طلب تعيين معالج',aZAppr:'موافقة',aZRej:'رفض',aZRev:'سحب التعيين',aZPrev:'احسب المسار',aZSubmit:'تقديم الشكوى',aZSaveC:'حفظ المهل',
wZ1:'المشتكى عليه',wZ2:'المستوى والتفاصيل',wZ3:'المراجعة والتقديم',fZKind:'نوع الشكوى',fZResp:'المشتكى عليه',fZLvl:'المستوى',fZSub:'الموضوع',fZCont:'المحتوى',fZAtt:'المرفقات',fZAttH:'PDF أو JPEG أو PNG. تُفحص وتُشفَّر ولا تُحذف.',fZType:'نوع الإجراء',fZNote:'الملاحظة',fZDec:'القرار',fZRsn:'السبب',fZUser:'المستخدم',fZLvlH:'المستوى المعيَّن',fZFrom:'تاريخ البداية',fZFromT:'وقت البداية HH:mm',fZTo:'تاريخ النهاية (اختياري)',fZToT:'وقت النهاية HH:mm',fZDelay:'مهلة التأخر (أيام دوام)',fZInt:'فاصل التذكير (أيام دوام)',fZFlt:'متأخرة',
kZPriv:'خصوصية',pZ1:'اسمك يظهر للمعالج؛ لا توجد شكوى مجهولة.',pZ2:'لا يُبلَّغ المشتكى عليه، ولا يطّلع عليها مهما كانت أدواره.',pZ3:'الإشعارات عامة تماماً («لديك تحديث جديد») دون اسم أو موضوع أو مستوى.',pZ4:'لا يمكن خفض المستوى، والشكاوى لا تُحذف.',pZ5:'كل اطلاع مخوَّل على الشكوى يُسجَّل في سجل الاطلاع.',pZ6:'القرار النهائي: العقد لا يعيد نص القرار لمقدم الشكوى (لا حقل له في الاستجابة).',pZ7:'سجل الاطلاع إضافة فقط ولا يحتوي محتوى الشكوى.',pZ8:'المهل لا تمنح أي وصول إلى المحتوى.',pZ9:'التعيين المعتمد يلزم صاحبه بالتحقق بخطوتين (غير محاكى في هذا العرض).',pZ10:'لا تفويض عام يمنح أي وصول إلى الشكاوى.',
zRaisedSys:'رفع النظام المستوى تلقائياً إلى الحد الأدنى.',zMinL:'الحد الأدنى للمستوى',zSelL:'المستويات المتاحة',zRoutes:'مسار كل مستوى',
emZ:'لا توجد شكاوى',emZI:'لا شكاوى مسندة إليك',emZH:'لا توجد تعيينات',emZL:'لا يوجد سجل',emZA:'لا توجد إجراءات',emZF:'لا توجد مرفقات',
tZSub:'قُدّمت الشكوى (تجريبي)',tZRaise:'رُفع المستوى (تجريبي)',tZAct:'أُضيف الإجراء (تجريبي)',tZFwd:'تم التحويل (تجريبي)',tZClose:'أُغلقت الشكوى (تجريبي)',tZUp:'رُفع المرفق (تجريبي)',tZDl:'نُزّل المرفق (تجريبي)',tZReq:'طُلب التعيين (تجريبي)',tZApp:'اعتُمد التعيين (تجريبي)',tZRej:'رُفض التعيين (تجريبي)',tZRev:'سُحب التعيين (تجريبي)',
dZRaise:'رفع مستوى الشكوى',dZRaiseB:'الرفع فقط؛ لا يمكن النزول عن المستوى الحالي.',dZAct:'إضافة إجراء',dZActB:'سجل إضافة فقط، لا يُعدَّل ولا يُحذف، ولا يظهر لمقدم الشكوى.',dZFwd:'تحويل إلى مدير الموارد البشرية',dZFwdB:'ينتهي وصولك بعد التحويل ولا يمكنك إغلاقها.',dZClose:'إغلاق الشكوى بقرار',dZCloseB:'الشكاوى محفوظة دائماً ولا يوجد مسار لإعادة الفتح.',dZReqH:'طلب تعيين جهة معالجة',dZReqHB:'لا يمنح وصولاً قبل موافقة المدير التنفيذي.',dZDec:'قرار على طلب التعيين',
er2:{'Hr:Complaints:RespondentRequired':'يجب اختيار المشتكى عليه','Hr:Complaints:RespondentNotAllowed':'لا يمكن تقديم شكوى ضد هذا الاختيار','Hr:Complaints:SelfComplaintNotAllowed':'لا يمكن تقديم شكوى ضد نفسك','Hr:Complaints:NoEligibleHandler':'لا توجد جهة معالجة مؤهلة','Hr:Complaints:LevelNotHigher':'يجب أن يكون المستوى أعلى من الحالي','Hr:Complaints:ComplaintClosed':'الشكوى مغلقة','Hr:Complaints:InvalidTransition':'هذا التحويل غير مسموح','Hr:Complaints:AttachmentRejected':'رُفض المرفق بعد الفحص','Hr:Complaints:AttachmentTooLarge':'حجم المرفق كبير','Hr:Complaints:HandlerCannotBeSystemAdmin':'مسؤول النظام لا يُعيَّن معالجاً','Hr:Complaints:InvalidAssignmentPeriod':'فترة التعيين غير صالحة','Hr:Complaints:HandlerRoleNotAllowed':'دور هذا المستخدم لا يسمح بهذا التعيين','Hr:Complaints:AssignmentNotPending':'الطلب ليس بانتظار الموافقة','Hr:Complaints:AssignmentNotActive':'التعيين ليس ساري المفعول','Hr:Complaints:NotFound':'غير موجود'}},
en:{
zT:'Complaints',zSub:'Confidential complaints: every authorized access is logged, and unauthorized users see no trace',
tZMy:'My complaints',tZIn:'Complaint inbox',tZHd:'Handler assignments',tZCf:'Complaint deadlines',
zl:{normal:'Normal',confidential:'Confidential',highlyConfidential:'Highly confidential'},zs:{pendingDepartmentManager:'Waiting for department manager',pendingHrManager:'Waiting for HR manager',pendingExecutiveDirector:'Waiting for executive director',closed:'Closed'},zk:{employee:'Against an employee',general:'General complaint'},zc:{colleague:'Colleague',departmentManager:'Department manager',storeOfficer:'Store officer',hrManager:'HR manager',reviewer:'Reviewer'},
za:{note:'Note',meeting:'Meeting',investigation:'Investigation',followUp:'Follow-up',submitted:'Submitted',levelRaised:'Level raised',forwarded:'Forwarded',closed:'Closed'},zal:{viewDetail:'Viewed details',downloadAttachment:'Downloaded attachment',uploadAttachment:'Uploaded attachment',viewAccessLog:'Viewed the log',addAction:'Added action',raiseLevel:'Raised level',forward:'Forwarded',close:'Closed'},zas:{pendingApproval:'Pending approval',approved:'Approved',rejected:'Rejected',revoked:'Revoked'},
cZRef:'Reference',cZLvl:'Level',cZStat:'Status',cZSub:'Subject',cZAt:'Submitted',cZDue:'Due',cZLate:'Late',cZClosed:'Closed',cZUser:'User',cZFrom:'From',cZTo:'To',cZReason:'Reason',cZReq:'Requested by',cZDec:'Decision',cZAct:'Action',cZNote:'Note',cZActor:'By',cZTime:'Time',cZFile:'File',cZSize:'Size',cZPath:'Path',cZMin:'Minimum',
aZNew:'New complaint',aZOpen:'Open',aZRaise:'Raise level',aZAct:'Add action',aZFwd:'Forward to HR manager',aZClose:'Close complaint',aZLog:'Access log',aZDl:'Download',aZBack:'Back',aZReqH:'Request handler',aZAppr:'Approve',aZRej:'Reject',aZRev:'Revoke',aZPrev:'Calculate route',aZSubmit:'Submit complaint',aZSaveC:'Save deadlines',
wZ1:'Respondent',wZ2:'Level and details',wZ3:'Review and submit',fZKind:'Complaint kind',fZResp:'Respondent',fZLvl:'Level',fZSub:'Subject',fZCont:'Content',fZAtt:'Attachments',fZAttH:'PDF, JPEG or PNG. Scanned, encrypted and never deleted.',fZType:'Action type',fZNote:'Note',fZDec:'Decision',fZRsn:'Reason',fZUser:'User',fZLvlH:'Assigned level',fZFrom:'Start date',fZFromT:'Start time HH:mm',fZTo:'End date (optional)',fZToT:'End time HH:mm',fZDelay:'Delay deadline (working days)',fZInt:'Reminder interval (working days)',fZFlt:'Late',
kZPriv:'Privacy',pZ1:'Your name is shown to the handler; there is no anonymous complaint.',pZ2:'The respondent is not notified and can never see it, whatever their roles.',pZ3:'Notifications are fully generic (“You have a new update”) with no name, subject or level.',pZ4:'The level can never be lowered, and complaints are never deleted.',pZ5:'Every authorized view of a complaint is recorded in the access log.',pZ6:'Final decision: the contract does not return the decision text to the complainant (no field in the response).',pZ7:'The access log is append-only and holds no complaint content.',pZ8:'Deadlines grant no access to content.',pZ9:'An approved assignment makes two-step verification mandatory for its holder (not simulated here).',pZ10:'A general delegation never grants access to complaints.',
zRaisedSys:'The system raised the level to the minimum automatically.',zMinL:'Minimum level',zSelL:'Selectable levels',zRoutes:'Route per level',
emZ:'No complaints',emZI:'No complaints are assigned to you',emZH:'No assignments',emZL:'No log entries',emZA:'No actions',emZF:'No attachments',
tZSub:'Complaint submitted (mock)',tZRaise:'Level raised (mock)',tZAct:'Action added (mock)',tZFwd:'Forwarded (mock)',tZClose:'Complaint closed (mock)',tZUp:'Attachment uploaded (mock)',tZDl:'Attachment downloaded (mock)',tZReq:'Assignment requested (mock)',tZApp:'Assignment approved (mock)',tZRej:'Assignment rejected (mock)',tZRev:'Assignment revoked (mock)',
dZRaise:'Raise complaint level',dZRaiseB:'Raise only; the level can never go down.',dZAct:'Add action',dZActB:'Append-only record; never edited or deleted; not shown to the complainant.',dZFwd:'Forward to HR manager',dZFwdB:'Your access ends after forwarding and you cannot close it.',dZClose:'Close the complaint with a decision',dZCloseB:'Complaints are always kept and there is no reopen path.',dZReqH:'Request a handler assignment',dZReqHB:'No access is granted before the executive director approves.',dZDec:'Decision on the assignment request',
er2:{'Hr:Complaints:RespondentRequired':'Choose the respondent','Hr:Complaints:RespondentNotAllowed':'A complaint cannot be filed against this choice','Hr:Complaints:SelfComplaintNotAllowed':'You cannot file a complaint against yourself','Hr:Complaints:NoEligibleHandler':'No eligible handler is available','Hr:Complaints:LevelNotHigher':'The level must be higher than the current one','Hr:Complaints:ComplaintClosed':'The complaint is closed','Hr:Complaints:InvalidTransition':'This transition is not allowed','Hr:Complaints:AttachmentRejected':'The attachment was rejected after scanning','Hr:Complaints:AttachmentTooLarge':'The attachment is too large','Hr:Complaints:HandlerCannotBeSystemAdmin':'A system admin cannot be a handler','Hr:Complaints:InvalidAssignmentPeriod':'The assignment period is not valid','Hr:Complaints:HandlerRoleNotAllowed':'This user’s role does not allow this assignment','Hr:Complaints:AssignmentNotPending':'The request is not pending approval','Hr:Complaints:AssignmentNotActive':'The assignment is not active','Hr:Complaints:NotFound':'Not found'}}};
Object.keys(T9).forEach(l=>{Object.assign(T4[l].er2,T9[l].er2);const o=Object.assign({},T9[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T9 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M12 2L3 6v6c0 5 3.5 9 9 10 5.5-1 9-5 9-10V6zM9 12l2 2 4-4');PAGES.push('cmp');T.ar.nav.push('الشكاوى');T.en.nav.push('Complaints');
const OPR7={"Report_GetTeamAttendance": "d", "Report_GetTeamLatenessAbsence": "d", "Report_GetTeamLeaves": "d", "Report_GetTeamPending": "d", "Report_GetTeamCustody": "d", "Report_GetTeamEvaluations": "d", "Report_GetReviewDashboard": "v", "Report_GetEvaluationConsistency": "v", "Report_GetHrAttendance": "h", "Report_GetHrLeaveBalances": "h", "Report_GetHrRequests": "h", "Report_GetHrContracts": "h", "Report_GetHrWarnings": "h", "Report_Export": "dvh"};
const T10={ar:{
rptT:'التقارير',rSub:'تقارير الفريق والموارد البشرية ولوحة المراجع. لا بيانات مالية ولا ملاحظات داخلية ولا محتوى شكاوى',
tRAtt:'حضور الفريق',tRLate:'التأخر والغياب',tRLeave:'إجازات الفريق',tRPend:'الطلبات المعلقة',tRCust:'العهد',tREval:'التقييمات',tRDash:'لوحة المراجع',tRCons:'اتساق التقييم',tRHAtt:'حضور الشركة',tRHBal:'أرصدة الإجازات',tRHReq:'الطلبات',
rcIn:'الدخول',rcOut:'الخروج',rcLateM:'دقائق التأخر',rcLateC:'مرات التأخر',rcAbs:'أيام الغياب',rcType:'النوع',rcFrom:'من',rcTo:'إلى',rcDays:'الأيام',rcHours:'الساعات',rcSt:'الحالة',rcReq:'نوع الطلب',rcCr:'تاريخ الرفع',rcCl:'تاريخ الإغلاق',rcSch:'أيام الدوام',rcPres:'أيام الحضور',rcLeaveD:'أيام الإجازة',rcYear:'بداية سنة الإجازة',rcEnt:'الاستحقاق',rcCar:'المضاف',rcUsed:'المستخدم',rcHeld:'المحجوز',rcRem:'المتبقي',rcUsedH:'ساعات مستخدمة',rcDept:'القسم',rcMgr:'المدير',rcExp:'المتوقع',rcSub:'المرفوع',rcRatio:'نسبة الإنجاز',rcSame:'درجة واحدة للجميع',
kRPres:'أيام الحضور',kRLate:'مرات التأخر',kRAbs:'أيام الغياب',kRLM:'مجموع دقائق التأخر',kRRows:'عدد السجلات',kRLD:'أيام الإجازة',kRRem:'مجموع المتبقي',kRCnf:'شكاوى سرية مقدمة',kRHi:'شكاوى سرية جداً مقدمة',kRLN:'متأخرة (عادية)',kRLC:'متأخرة (سرية)',kRLH:'متأخرة (سرية جداً)',kRDone:'أقسام منجزة',kRLateD:'أقسام متأخرة',
fRYear:'السنة',fRMonth:'الشهر',fRDept:'القسم',fRKind:'نوع العهد',rkind:{personal:'شخصية',shared:'مشتركة'},
aRExp:'تصدير',dRExp:'تصدير التقرير',dRExpB:'بنفس المرشحات والحقول المعروضة. لا يحتوي ملاحظات داخلية ولا شكاوى ولا بيانات مالية.',fRFmt:'الصيغة',rfmt:{pdf:'PDF',excel:'Excel'},tRExp:'جُهّز الملف (تجريبي)',
nRPriv:'لا بيانات مالية، ولا ملاحظات داخلية، ولا محتوى شكاوى في أي تقرير أو تصدير.',nRR2:'تمتلئ مع وحدتها في الإصدار 2، وترجع الآن قائمة فارغة.',nRDash:'أعداد فقط على مستوى الشركة. بلا أسماء أو تفاصيل أو تفصيل حسب قسم أو فئة أو شخص، ولا يمكن فتح أي شكوى من هنا.',nRDash2:'جزء الأقسام (إنجاز التقييم والتقرير والتأخر) يكتمل في الإصدار 2 مع وحدة التقييم. الآن لا توجد أقسام ولا دورة.',nRAsOf:'محدّث حتى',
emRp:'لا توجد بيانات للمرشحات المحددة',emRD:'لا توجد أقسام (تتوفر في الإصدار 2)',
er2:{'Hr:Reports:InvalidPeriod':'الفترة غير صالحة (النهاية قبل البداية أو أطول من سنة)','Hr:Reports:MissingFilter':'مرشح مطلوب غير موجود لهذا التقرير','Hr:Reports:NotFound':'التقرير غير موجود'}},
en:{
rptT:'Reports',rSub:'Team and HR reports and the reviewer dashboard. No financial data, internal notes or complaint content',
tRAtt:'Team attendance',tRLate:'Lateness and absence',tRLeave:'Team leaves',tRPend:'Pending requests',tRCust:'Custody',tREval:'Evaluations',tRDash:'Reviewer dashboard',tRCons:'Evaluation consistency',tRHAtt:'Company attendance',tRHBal:'Leave balances',tRHReq:'Requests',
rcIn:'Check-in',rcOut:'Check-out',rcLateM:'Late minutes',rcLateC:'Late count',rcAbs:'Absent days',rcType:'Type',rcFrom:'From',rcTo:'To',rcDays:'Days',rcHours:'Hours',rcSt:'Status',rcReq:'Request type',rcCr:'Created',rcCl:'Closed',rcSch:'Scheduled days',rcPres:'Present days',rcLeaveD:'Leave days',rcYear:'Leave year start',rcEnt:'Entitlement',rcCar:'Carried',rcUsed:'Used',rcHeld:'Held',rcRem:'Remaining',rcUsedH:'Hours used',rcDept:'Department',rcMgr:'Manager',rcExp:'Expected',rcSub:'Submitted',rcRatio:'Completion',rcSame:'Same grade for all',
kRPres:'Present days',kRLate:'Late count',kRAbs:'Absent days',kRLM:'Total late minutes',kRRows:'Records',kRLD:'Leave days',kRRem:'Total remaining',kRCnf:'Confidential submitted',kRHi:'Highly confidential submitted',kRLN:'Late (normal)',kRLC:'Late (confidential)',kRLH:'Late (highly confidential)',kRDone:'Completed departments',kRLateD:'Late departments',
fRYear:'Year',fRMonth:'Month',fRDept:'Department',fRKind:'Custody kind',rkind:{personal:'Personal',shared:'Shared'},
aRExp:'Export',dRExp:'Export report',dRExpB:'With the same filters and fields shown. It never contains internal notes, complaints or financial data.',fRFmt:'Format',rfmt:{pdf:'PDF',excel:'Excel'},tRExp:'File prepared (mock)',
nRPriv:'No financial data, internal notes or complaint content in any report or export.',nRR2:'Fills with its module in release 2 and currently returns an empty list.',nRDash:'Counts only, company-wide. No names, details or breakdown by department, category or person, and no complaint can be opened from here.',nRDash2:'The departments part (evaluation and report completion, lateness) completes in release 2 with the evaluation module. For now there are no departments and no cycle.',nRAsOf:'As of',
emRp:'No data for the selected filters',emRD:'No departments (available in release 2)',
er2:{'Hr:Reports:InvalidPeriod':'The period is not valid (end before start or longer than a year)','Hr:Reports:MissingFilter':'A required filter is missing for this report','Hr:Reports:NotFound':'Report not found'}}};
Object.keys(T10).forEach(l=>{Object.assign(T4[l].er2,T10[l].er2);const o=Object.assign({},T10[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T10 key clash '+k)});Object.assign(T4[l],o)});
const OPR8={"Notification_List": "edhxvasm", "Notification_GetUnreadCount": "edhxvasm", "Notification_MarkRead": "edhxvasm", "DeviceToken_Register": "edhxvasm", "DeviceToken_Unregister": "edhxvasm", "SystemSettings_ListSections": "mh", "SystemSettings_GetReminders": "m", "SystemSettings_UpdateReminders": "m", "AuditLog_List": "mx"};
NAVR.aud="mx";NAVR.notif="edhxvasm";
const T11={ar:{
ntT:'الإشعارات',ntSub:'تحديثات حسابك. كل إشعار عام ولا يحتوي اسماً أو موضوعاً أو مستوى شكوى',ntEmpty:'لا توجد إشعارات',ntAll:'عرض كل الإشعارات',ntMarkAll:'تحديد الكل كمقروء',ntOpen:'فتح',ntMark:'تحديد كمقروء',ntRead:'مقروء',ntUnread:'غير مقروء',ntTitle:'العنوان',ntBody:'النص',ntState:'الحالة',ntTime:'الوقت',ntFlt:'الحالة',ntDev:'هذا الجهاز مسجّل لاستلام الإشعارات الفورية (تجريبي، رمز ويب).',ntLive:'التحديث اللحظي (SignalR) محاكى: تظهر الإشعارات الجديدة بعد الحدث بثوانٍ.',ntNote:'لا تحتوي الإشعارات اسماً ولا موضوعاً ولا مستوى شكوى.',tNtAll:'حُدّد الكل كمقروء (تجريبي)',
auT:'سجل التدقيق',auSub:'سجل للإضافة فقط. بلا قيم بيانات وبلا محتوى شكاوى',auTime:'الوقت',auUser:'المستخدم',auAct:'الإجراء',auEnt:'الكيان',auEid:'المعرّف',auFields:'الحقول المتغيرة',auIp:'العنوان',auOk:'النجاح',auUserF:'المستخدم',auActF:'الإجراء',auEntF:'الكيان',auOkF:'النجاح',
aua:{create:'إنشاء',update:'تعديل',delete:'حذف',read:'اطلاع',export:'تصدير',login:'دخول',loginFailed:'فشل دخول',passwordReset:'إعادة ضبط كلمة السر',roleChanged:'تغيير أدوار',permissionChanged:'تغيير صلاحيات',settingChanged:'تغيير إعداد',complaintHandlerAssignment:'تعيين معالج شكاوى'},
auNote:'لا تُعرض قيم البيانات، ولا كيانات الشكاوى (لها سجل اطلاع خاص). تعيين معالج الشكاوى يظهر دون أي بيانات عن شكوى.',emAu:'لا توجد قيود',
remT:'مواعيد التذكير والتصعيد',remSub:'تذكيرات نهاية الشهر وتصعيد التأخر',fRemOpen:'وقت فتح نماذج نهاية الشهر HH:mm',fRemSub:'مهلة رفع التقييم والتقرير (أيام دوام)',fRemInt:'فاصل التذكير (أيام دوام)',fRemEsc:'يوم التصعيد (يوم دوام)',fRemAppr:'فاصل تذكير الموافقات (أيام دوام)',fRemMin:'أقل مدة عمل للتقييم (أيام دوام)',remSave:'حفظ المواعيد',remNote:'التذكير لا ينفذ أي إجراء تلقائي على الطلبات، والتغيير يسري من التشغيل التالي للمهام، ولا يمنح حقاً في معالجة الشكاوى. القيمتان الأخيرتان لا قيمة أولية لهما في الوثيقة: تظهر القيم المعتمدة (2 و15) مقترحة حتى تُحفظ.',remRo:'للاطلاع فقط: التعديل لمسؤول النظام.',
secEd:'قابل للتعديل لك',secRo:'للاطلاع',secSoon:'يتوفر مع وحدته في إصدار لاحق',
secL:{reminders:'مواعيد التذكير والتصعيد',contracts:'مدة تنبيه العقد',complaints:'مهل الشكاوى',attendance:'طريقة الحضور المعتمدة',hourlyLeave:'إعدادات الإجازة الزمنية',finance:'الإعدادات المالية',rolesAndPermissions:'الأدوار والصلاحيات',requestTypes:'أنواع الطلبات',otherLeaveTypes:'أنواع الإجازات الأخرى',attendanceDevices:'أجهزة البصمة',evaluationCriteria:'معايير التقييم',employeeLeaveBalances:'أرصدة إجازات الموظفين',workShifts:'قوالب الدوام',holidays:'العطل',departmentsAndManagerLinks:'الأقسام وربط الموظفين بالمديرين'},
secD:{reminders:'تذكير وتصعيد نهاية الشهر',contracts:'قبل انتهاء العقد',complaints:'مهلة تأخر الشكاوى وفاصل التذكير',attendance:'بصمة جهاز أو استيراد ملف',hourlyLeave:'حصة الشهر وساعات اليوم',finance:'إعدادات الرواتب والمصروفات',rolesAndPermissions:'الأدوار والصلاحيات العادية',requestTypes:'الأنواع والجهة المنفذة',otherLeaveTypes:'مرضية، أمومة، حداد وغيرها',attendanceDevices:'الأجهزة وربط المستخدمين',evaluationCriteria:'المعايير وسلم الدرجات',employeeLeaveBalances:'الاستحقاق والتعديل لكل موظف',workShifts:'قوالب الدوام وتعيينها',holidays:'العطل والإجازة الأسبوعية',departmentsAndManagerLinks:'الأقسام وربط الموظفين'},
er2:{'Hr:Settings:InvalidEscalationDay':'يوم التصعيد يجب أن يكون بعد مهلة الرفع','Hr:Notification:NotFound':'الإشعار غير موجود'}},
en:{
ntT:'Notifications',ntSub:'Updates on your account. Every notification is generic and never contains a name, subject or complaint level',ntEmpty:'No notifications',ntAll:'View all notifications',ntMarkAll:'Mark all as read',ntOpen:'Open',ntMark:'Mark as read',ntRead:'Read',ntUnread:'Unread',ntTitle:'Title',ntBody:'Text',ntState:'Status',ntTime:'Time',ntFlt:'Status',ntDev:'This device is registered for push notifications (mock, web token).',ntLive:'Live updates (SignalR) are simulated: new notifications appear seconds after the event.',ntNote:'Notifications never contain a name, a subject or a complaint level.',tNtAll:'All marked as read (mock)',
auT:'Audit log',auSub:'Append-only. No data values and no complaint content',auTime:'Time',auUser:'User',auAct:'Action',auEnt:'Entity',auEid:'Identifier',auFields:'Changed fields',auIp:'Address',auOk:'Succeeded',auUserF:'User',auActF:'Action',auEntF:'Entity',auOkF:'Succeeded',
aua:{create:'Create',update:'Update',delete:'Delete',read:'Read',export:'Export',login:'Sign-in',loginFailed:'Failed sign-in',passwordReset:'Password reset',roleChanged:'Roles changed',permissionChanged:'Permissions changed',settingChanged:'Setting changed',complaintHandlerAssignment:'Complaint handler assignment'},
auNote:'Data values are never shown, and complaint entities are excluded (they have their own access log). A complaint-handler assignment appears without any complaint data.',emAu:'No entries',
remT:'Reminder and escalation schedule',remSub:'Month-end reminders and lateness escalation',fRemOpen:'Month-end forms open time HH:mm',fRemSub:'Submission deadline (working days)',fRemInt:'Reminder interval (working days)',fRemEsc:'Escalation day (working day)',fRemAppr:'Approval reminder interval (working days)',fRemMin:'Minimum worked days for evaluation (working days)',remSave:'Save schedule',remNote:'A reminder never performs an automatic action on requests; a change applies from the next job run; it grants no right to handle complaints. The last two values have no initial value in the document: the approved decisions (2 and 15) are shown as suggestions until saved.',remRo:'Read-only: only the system administrator can edit.',
secEd:'Editable by you',secRo:'View only',secSoon:'Available with its module in a later release',
secL:{reminders:'Reminder and escalation schedule',contracts:'Contract alert period',complaints:'Complaint deadlines',attendance:'Approved attendance method',hourlyLeave:'Hourly leave settings',finance:'Finance settings',rolesAndPermissions:'Roles and permissions',requestTypes:'Request types',otherLeaveTypes:'Other leave types',attendanceDevices:'Fingerprint devices',evaluationCriteria:'Evaluation criteria',employeeLeaveBalances:'Employee leave balances',workShifts:'Work shifts',holidays:'Holidays',departmentsAndManagerLinks:'Departments and manager links'},
secD:{reminders:'Month-end reminders and escalation',contracts:'Before a contract ends',complaints:'Lateness deadline and reminder interval',attendance:'Device punches or file import',hourlyLeave:'Monthly quota and hours per day',finance:'Payroll and expense settings',rolesAndPermissions:'Ordinary roles and permissions',requestTypes:'Types and executing party',otherLeaveTypes:'Sick, maternity, bereavement and others',attendanceDevices:'Devices and user mapping',evaluationCriteria:'Criteria and grade scale',employeeLeaveBalances:'Entitlement and adjustment per employee',workShifts:'Shift templates and assignment',holidays:'Holidays and weekly off',departmentsAndManagerLinks:'Departments and employee links'},
er2:{'Hr:Settings:InvalidEscalationDay':'The escalation day must come after the submission deadline','Hr:Notification:NotFound':'Notification not found'}}};
Object.keys(T11).forEach(l=>{Object.assign(T4[l].er2,T11[l].er2);const o=Object.assign({},T11[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T11 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M9 12h6M9 16h6M9 8h6M5 3h14v18H5z');PAGES.push('aud');T.ar.nav.push('سجل التدقيق');T.en.nav.push('Audit log');
const OPR9={"EvaluationCycle_GetCurrent": "dhv", "Evaluation_GetTeamList": "d", "Evaluation_GetManagerForm": "d", "Evaluation_SaveDraft": "d", "Evaluation_Submit": "d", "Evaluation_GetMyList": "e", "Evaluation_GetMyDetail": "e", "Evaluation_Acknowledge": "e", "Evaluation_AddResponse": "e", "Evaluation_DecideObjection": "h", "Evaluation_GetHrList": "h", "Evaluation_GetHrView": "h", "EvaluationInternalNote_GetList": "dh", "EvaluationInternalNote_Add": "d", "EvaluationInternalNote_SaveDraft": "d", "EvaluationAnnualSummary_GetMy": "e", "EvaluationAnnualSummary_GetForEmployee": "dh", "EvaluationCriterion_GetList": "m", "EvaluationCriterion_Create": "m", "EvaluationCriterion_Update": "m", "EvaluationGrade_GetList": "m", "EvaluationGrade_Create": "m", "EvaluationGrade_Update": "m", "DepartmentReport_GetList": "dh", "DepartmentReport_Get": "dh", "DepartmentReport_UpdateNotes": "d", "DepartmentReport_Submit": "d", "EvaluationCycle_GetList": "dhv"};
NAVR.perf="edhvm";
const T12={ar:{
evT:'التقييم الشهري',evSub:'دورة التقييم والتقرير الشهري. السلم لفظي بلا نسب، والملاحظة الداخلية لا يراها الموظف أبداً',
tEvMy:'تقييماتي',tEvSum:'ملخصي السنوي',tEvTeam:'تقييمات فريقي',tEvRep:'التقرير الشهري',tEvTSum:'ملخص سنوي لموظف',tEvHr:'تقييمات الشركة',tEvHRep:'تقارير الأقسام',tEvHSum:'ملخص سنوي لموظف',tEvCyc:'الدورات',tEvCrit:'المعايير والسلم',
evst:{draft:'مسودة',submitted:'مرفوع',acknowledged:'مؤكَّد'},evob:{none:'بلا اعتراض',pending:'اعتراض معلّق',accepted:'اعتراض مقبول',rejected:'اعتراض مرفوض'},evcy:{scheduled:'مجدولة',open:'مفتوحة',closed:'مغلقة'},evrst:{draft:'مسودة',submitted:'مرفوع'},evrt:{comment:'تعليق',objection:'اعتراض'},evdc:{accepted:'مقبول',rejected:'مرفوض'},
evCycle:'الدورة',evPeriod:'الشهر',evEmp:'الموظف',evEval:'المقيِّم',evSt:'الحالة',evOb:'الاعتراض',evSubAt:'رُفع في',evAckAt:'أُكّد في',evLate:'متأخر',evCrit:'المعيار',evGrade:'الدرجة',evCnt:'العدد',evTime:'الوقت',evContent:'النص',evDec:'القرار',evDecR:'سبب القرار',evExp:'المتوقع',evSubm:'المرفوع',evRem:'المتبقي',evOpens:'تُفتح النماذج',evFreeze:'تثبيت البيانات',evDue:'المهلة',evEsc:'التصعيد',evDone:'منجز',evCompl:'الإنجاز',evMgr:'المدير',evDept:'القسم',evNotes:'ملاحظات الموظف',evNone:'—',
evPres:'أيام الحضور',evAbs:'أيام الغياب',evLateC:'مرات التأخر',evLateM:'دقائق التأخر',evLeaveD:'أيام الإجازة',evSched:'أيام الدوام',evFrozen:'بيانات مثبّتة',evMonths:'أشهر مقيَّمة',evAcked:'مؤكَّدة',evObjs:'اعتراضات',evPend:'طلبات معلقة',
aEvOpen:'فتح',aEvAck:'تأكيد الاطلاع',aEvCmt:'إضافة تعليق',aEvObj:'اعتراض',aEvSave:'حفظ المسودة',aEvSubmit:'رفع التقييم',aEvNote:'إضافة ملاحظة داخلية',aEvSaveN:'حفظ مسودة الملاحظة',aEvDecide:'البت في الاعتراض',aEvRSave:'حفظ الملاحظات',aEvRSubmit:'رفع التقرير',aEvNewC:'معيار جديد',aEvNewG:'درجة جديدة',aEvEdit:'تعديل',
fEvYear:'السنة',fEvGrade:'الدرجة',fEvEmpNotes:'ملاحظات للموظف (تظهر له)',fEvInternal:'ملاحظة داخلية (لا يراها الموظف)',fEvInternalDraft:'مسودة الملاحظة الداخلية',fEvContent:'النص',fEvDecision:'القرار',fEvReason:'السبب',fEvNameAr:'الاسم بالعربية',fEvNameEn:'الاسم بالإنجليزية',fEvOrder:'الترتيب',fEvActive:'فعّال',fEvCode:'الرمز (إنجليزي)',fEvRNotes:'ملاحظات المدير على التقرير',
dEvCmt:'تعليق على التقييم',dEvCmtB:'يراه مديرك والموارد البشرية ولا يحتاج قراراً.',dEvObj:'اعتراض على التقييم',dEvObjB:'يصل إلى مدير الموارد البشرية ويبتّ فيه. الاعتراض الواحد المعلّق في كل مرة. المهلة المعتمدة 5 أيام دوام من تاريخ الرفع.',dEvDec:'البت في الاعتراض',dEvDecB:'يُبنى القرار على الدرجات والملاحظات الظاهرة والوقائع الموثقة، لا على الملاحظة الداخلية، ولا تُذكر في السبب.',dEvNote:'ملاحظة داخلية',dEvNoteB:'تُضاف بعد الرفع باسمك ولا تُعدَّل ولا تُحذف. يراها كاتبها ومدير الموارد البشرية فقط.',dEvCrit:'معيار تقييم',dEvGradeD:'درجة لفظية',dEvCritB:'يسري التغيير من الدورة التالية ولا يغيّر دورة مفتوحة ولا تقييماً سابقاً. لا حذف: الإيقاف فقط.',
nEvInternal:'الملاحظات الداخلية: يراها كاتبها ومدير الموارد البشرية فقط، ولا تظهر في أي عرض أو إشعار أو تصدير، ولا يُبنى عليها قرار.',nEvNew:'لا تظهر الدرجات للموظف قبل الرفع، والمسودة لا تُرسل إشعاراً.',nEvRep:'تقرير ثابت يولَّد في أول يوم دوام من الشهر التالي. لا ملاحظات داخلية ولا بيانات مالية ولا شكاوى.',nEvDone:'لا يُعدّ القسم منجزاً إلا برفع كل التقييمات والتقرير معاً.',nEvAnnual:'ملخص آلي بعدد مرات كل درجة، بلا نسب وبلا ملاحظات داخلية.',nEvPick:'اختر موظفاً لعرض ملخصه.',nEvDemo:'الدورة الأخيرة تبقى مفتوحة في هذا العرض ليمكن التعديل. الإغلاق الفعلي من الخادم.',
emEv:'لا توجد تقييمات',emEvR:'لا توجد تقارير',emEvC:'لا توجد ردود',emEvN:'لا توجد ملاحظات',emEvK:'لا توجد معايير',emEvG:'لا توجد درجات',emEvS:'لا توجد بيانات لهذه السنة',
tEvAck:'أُكّد الاطلاع (تجريبي)',tEvCmt:'أُضيف التعليق (تجريبي)',tEvObj:'سُجّل الاعتراض (تجريبي)',tEvDraft:'حُفظت المسودة (تجريبي)',tEvSubm:'رُفع التقييم (تجريبي)',tEvNote:'أُضيفت الملاحظة (تجريبي)',tEvDecided:'تم البت في الاعتراض (تجريبي)',tEvRSub:'رُفع التقرير (تجريبي)',
er2:{'Hr:Evaluations:NotEditable':'التقييم غير قابل للتعديل','Hr:Evaluations:CycleNotOpen':'الدورة غير مفتوحة','Hr:Evaluations:InvalidCriterion':'معيار غير صالح','Hr:Evaluations:InvalidGrade':'درجة غير صالحة','Hr:Evaluations:IncompleteScores':'قيّم كل المعايير قبل الرفع','Hr:Evaluations:ObjectionAlreadyPending':'لديك اعتراض معلّق على هذا التقييم','Hr:Evaluations:NotAnObjection':'هذا الرد ليس اعتراضاً','Hr:Evaluations:ObjectionAlreadyDecided':'بُتّ في هذا الاعتراض مسبقاً','Hr:Evaluations:SelfDecisionNotAllowed':'لا يبتّ أحد في اعتراضه','Hr:Evaluations:EvaluationNotSubmitted':'يُستخدم مسار المسودة قبل الرفع','Hr:Evaluations:LastActiveCriterion':'يجب أن يبقى معيار فعّال واحد على الأقل','Hr:Evaluations:MinimumActiveGrades':'يجب أن تبقى درجتان فعّالتان على الأقل','Hr:DepartmentReports:AlreadySubmitted':'التقرير مرفوع ولا يمكن تعديله','Hr:Evaluations:NotFound':'غير موجود'}},
en:{
evT:'Monthly evaluation',evSub:'Evaluation cycle and monthly report. The scale is verbal with no percentages, and the internal note is never visible to the employee',
tEvMy:'My evaluations',tEvSum:'My annual summary',tEvTeam:'My team’s evaluations',tEvRep:'Monthly report',tEvTSum:'Employee annual summary',tEvHr:'Company evaluations',tEvHRep:'Department reports',tEvHSum:'Employee annual summary',tEvCyc:'Cycles',tEvCrit:'Criteria and scale',
evst:{draft:'Draft',submitted:'Submitted',acknowledged:'Acknowledged'},evob:{none:'No objection',pending:'Objection pending',accepted:'Objection accepted',rejected:'Objection rejected'},evcy:{scheduled:'Scheduled',open:'Open',closed:'Closed'},evrst:{draft:'Draft',submitted:'Submitted'},evrt:{comment:'Comment',objection:'Objection'},evdc:{accepted:'Accepted',rejected:'Rejected'},
evCycle:'Cycle',evPeriod:'Month',evEmp:'Employee',evEval:'Evaluator',evSt:'Status',evOb:'Objection',evSubAt:'Submitted',evAckAt:'Acknowledged',evLate:'Late',evCrit:'Criterion',evGrade:'Grade',evCnt:'Count',evTime:'Time',evContent:'Text',evDec:'Decision',evDecR:'Decision reason',evExp:'Expected',evSubm:'Submitted',evRem:'Remaining',evOpens:'Forms open',evFreeze:'Data frozen',evDue:'Deadline',evEsc:'Escalation',evDone:'Completed',evCompl:'Completion',evMgr:'Manager',evDept:'Department',evNotes:'Employee notes',evNone:'—',
evPres:'Present days',evAbs:'Absent days',evLateC:'Late count',evLateM:'Late minutes',evLeaveD:'Leave days',evSched:'Scheduled days',evFrozen:'Frozen data',evMonths:'Evaluated months',evAcked:'Acknowledged',evObjs:'Objections',evPend:'Pending requests',
aEvOpen:'Open',aEvAck:'Acknowledge',aEvCmt:'Add comment',aEvObj:'Object',aEvSave:'Save draft',aEvSubmit:'Submit evaluation',aEvNote:'Add internal note',aEvSaveN:'Save note draft',aEvDecide:'Decide objection',aEvRSave:'Save notes',aEvRSubmit:'Submit report',aEvNewC:'New criterion',aEvNewG:'New grade',aEvEdit:'Edit',
fEvYear:'Year',fEvGrade:'Grade',fEvEmpNotes:'Notes for the employee (visible to them)',fEvInternal:'Internal note (never visible to the employee)',fEvInternalDraft:'Internal note draft',fEvContent:'Text',fEvDecision:'Decision',fEvReason:'Reason',fEvNameAr:'Name (Arabic)',fEvNameEn:'Name (English)',fEvOrder:'Order',fEvActive:'Active',fEvCode:'Code (English)',fEvRNotes:'Manager notes on the report',
dEvCmt:'Comment on the evaluation',dEvCmtB:'Your manager and HR see it; it needs no decision.',dEvObj:'Object to the evaluation',dEvObjB:'It goes to the HR manager, who decides. Only one pending objection at a time. The approved window is 5 working days from the submission date.',dEvDec:'Decide the objection',dEvDecB:'The decision rests on the grades, visible notes and documented facts, never on the internal note, and the reason must not mention it.',dEvNote:'Internal note',dEvNoteB:'Added after submission in your name and never edited or deleted. Only its author and the HR manager can see it.',dEvCrit:'Evaluation criterion',dEvGradeD:'Verbal grade',dEvCritB:'A change applies from the next cycle and does not alter an open cycle or a past evaluation. No deletion: deactivate only.',
nEvInternal:'Internal notes: visible only to their author and the HR manager. They appear in no view, notification or export, and no decision rests on them.',nEvNew:'The employee sees no grades before submission, and a draft sends no notification.',nEvRep:'A frozen report generated on the first working day of the next month. No internal notes, financial data or complaints.',nEvDone:'A department counts as completed only when all evaluations and the report are submitted.',nEvAnnual:'An automatic summary of how often each grade was given, with no percentages and no internal notes.',nEvPick:'Choose an employee to see their summary.',nEvDemo:'The latest cycle stays open in this demo so it can be edited. Real closing is the server’s job.',
emEv:'No evaluations',emEvR:'No reports',emEvC:'No responses',emEvN:'No notes',emEvK:'No criteria',emEvG:'No grades',emEvS:'No data for this year',
tEvAck:'Acknowledged (mock)',tEvCmt:'Comment added (mock)',tEvObj:'Objection recorded (mock)',tEvDraft:'Draft saved (mock)',tEvSubm:'Evaluation submitted (mock)',tEvNote:'Note added (mock)',tEvDecided:'Objection decided (mock)',tEvRSub:'Report submitted (mock)',
er2:{'Hr:Evaluations:NotEditable':'The evaluation is not editable','Hr:Evaluations:CycleNotOpen':'The cycle is not open','Hr:Evaluations:InvalidCriterion':'Invalid criterion','Hr:Evaluations:InvalidGrade':'Invalid grade','Hr:Evaluations:IncompleteScores':'Grade every criterion before submitting','Hr:Evaluations:ObjectionAlreadyPending':'You already have a pending objection on this evaluation','Hr:Evaluations:NotAnObjection':'This response is not an objection','Hr:Evaluations:ObjectionAlreadyDecided':'This objection was already decided','Hr:Evaluations:SelfDecisionNotAllowed':'Nobody decides their own objection','Hr:Evaluations:EvaluationNotSubmitted':'Use the draft route before submission','Hr:Evaluations:LastActiveCriterion':'At least one active criterion must remain','Hr:Evaluations:MinimumActiveGrades':'At least two active grades must remain','Hr:DepartmentReports:AlreadySubmitted':'The report is submitted and cannot be edited','Hr:Evaluations:NotFound':'Not found'}}};
Object.keys(T12).forEach(l=>{Object.assign(T4[l].er2,T12[l].er2);const o=Object.assign({},T12[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T12 key clash '+k)});Object.assign(T4[l],o)});
const OPR10={"Circular_GetReceivedList": "edhxvasm", "Circular_Create": "dhx", "Circular_GetSentList": "dh", "Circular_Get": "edhxvasm", "Circular_GetReceipts": "dh", "Circular_Acknowledge": "edhxvasm"};
NAVR.ann="edhxvasm";
const T13={ar:{
crT:'التعاميم',crSub:'تعاميم رسمية: لا تُعدَّل ولا تُحذف بعد الإرسال، وتأكيد الاستلام مطلوب لكلها',
tCrRecv:'الواردة',tCrSent:'المرسلة',
crcDate:'تاريخ الإرسال',crcTitle:'العنوان',crcSender:'المرسل',crcScope:'النطاق',crcSt:'الحالة',crcRcp:'المستلمون',crcAck:'المؤكِّدون',crcAckAt:'وقت التأكيد',crcRec:'المستلم',crcContent:'النص',
crs:{team:'فريقي',departmentManagers:'مديرو الأقسام',company:'كل الشركة'},crAcked:'مؤكَّد',crNeed:'بانتظار التأكيد',
aCrNew:'تعميم جديد',aCrOpen:'فتح',aCrAck:'تأكيد الاستلام',aCrRcpt:'حالة الاستلام',aCrSend:'إرسال التعميم',
fCrScope:'النطاق',fCrTitle:'العنوان',fCrContent:'نص التعميم',
dCrNew:'تعميم جديد',dCrNewB:'يُثبَّت المستلمون وقت الإرسال، ولا يُعدَّل التعميم ولا يُحذف بعد الإرسال. مدير القسم يرسل لفريقه، ومدير الموارد البشرية والمدير التنفيذي لمديري الأقسام أو لكل الشركة.',
nCrExec:'المدير التنفيذي يرسل التعاميم، لكن العقد لا يتيح له قائمة المرسلة ولا حالة الاستلام (للمدير الموارد البشرية ومدير القسم فقط).',nCrRcpt:'المستلمون مثبتون وقت الإرسال: من ارتبط بعده لا يراه.',
emCr:'لا توجد تعاميم',emCrR:'لا يوجد مستلمون',
tCrDone:'أُرسل التعميم (تجريبي)',tCrAck:'أُكّد الاستلام (تجريبي)',
er2:{'Hr:Circulars:ScopeNotAllowed':'لا تملك هذا النطاق','Hr:Circulars:NoRecipients':'لا يوجد مستلمون لهذا النطاق','Hr:Circulars:NotFound':'غير موجود'}},
en:{
crT:'Circulars',crSub:'Official circulars: never edited or deleted after sending, and acknowledgement is required for all of them',
tCrRecv:'Received',tCrSent:'Sent',
crcDate:'Sent',crcTitle:'Title',crcSender:'Sender',crcScope:'Scope',crcSt:'Status',crcRcp:'Recipients',crcAck:'Acknowledged',crcAckAt:'Acknowledged at',crcRec:'Recipient',crcContent:'Text',
crs:{team:'My team',departmentManagers:'Department managers',company:'Whole company'},crAcked:'Acknowledged',crNeed:'Needs acknowledgement',
aCrNew:'New circular',aCrOpen:'Open',aCrAck:'Acknowledge receipt',aCrRcpt:'Receipt status',aCrSend:'Send circular',
fCrScope:'Scope',fCrTitle:'Title',fCrContent:'Circular text',
dCrNew:'New circular',dCrNewB:'Recipients are fixed at the moment of sending, and a circular is never edited or deleted afterwards. A department manager sends to the team; the HR manager and the executive director send to department managers or the whole company.',
nCrExec:'The executive director sends circulars, but the contract gives no sent list or receipt status to that role (HR manager and department manager only).',nCrRcpt:'Recipients are fixed at sending: anyone linked afterwards does not see it.',
emCr:'No circulars',emCrR:'No recipients',
tCrDone:'Circular sent (mock)',tCrAck:'Receipt acknowledged (mock)',
er2:{'Hr:Circulars:ScopeNotAllowed':'You do not hold this scope','Hr:Circulars:NoRecipients':'There are no recipients for this scope','Hr:Circulars:NotFound':'Not found'}}};
Object.keys(T13).forEach(l=>{Object.assign(T4[l].er2,T13[l].er2);const o=Object.assign({},T13[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T13 key clash '+k)});Object.assign(T4[l],o)});
const OPR11={"Item_GetList": "s", "Item_Create": "s", "Item_Lookup": "edhxvasm", "Item_Get": "s", "Item_Update": "s", "Stock_Receive": "s", "Stock_Adjust": "s", "Stock_GetMovements": "s", "StockIssue_GetList": "s", "StockIssue_Get": "s", "StockIssue_Acknowledge": "s", "StockIssue_MarkPurchasing": "s", "StockIssue_Deliver": "s", "Custody_Create": "s", "Custody_GetList": "sh", "Custody_GetMy": "edhxvasm", "Custody_GetTeam": "d", "Custody_Get": "edhxvasm", "Custody_Acknowledge": "edhxvasm", "Custody_Return": "s", "CustodyCount_Create": "d", "CustodyCount_GetList": "dsh"};
NAVR.inv="s";NAVR.cus="edhxvasm";
const T14={ar:{
svT:'المخزن',svSub:'الأصناف والحركات وطلبات الصرف. التكلفة تُدخل عند الاستلام ويراها المحاسب فقط ولا تظهر هنا',tSvItems:'الأصناف',tSvMoves:'حركات المخزون',tSvIssues:'طلبات الصرف',
cuT:'العهد',cuSub:'عهدتي وعهد الفريق وسجل الجرد',tCuMy:'عهدتي',tCuTeam:'عهد الفريق والقسم',tCuAll:'كل العهد',tCuCount:'الجرد',
sk:{returnable:'يُسترد (عهدة)',consumable:'مستهلك'},sist:{pendingAcknowledgement:'بانتظار اطلاع المخزن',acknowledged:'اطّلع المخزن',purchasing:'بانتظار الشراء',delivered:'سُلّم',cancelled:'أُلغي'},cust:{pendingAcknowledgement:'بانتظار تأكيد الاستلام',acknowledged:'مؤكَّدة',returned:'أُعيدت'},ckind:{personal:'شخصية',shared:'مشتركة'},mvt:{receive:'استلام',issue:'صرف',return:'إرجاع',adjustment:'تعديل'},cres:{matched:'مطابق',discrepancy:'فرق'},
svcName:'الصنف',svcKind:'النوع',svcMin:'الحد الأدنى',svcQty:'الرصيد',svcBelow:'تحت الحد',svcAct:'فعّال',svcType:'الحركة',svcAfter:'الرصيد بعدها',svcBy:'بواسطة',svcNote:'الملاحظة',svcQ:'الكمية',svcAvail:'متوفر',svcEmp:'الموظف',svcAt:'الوقت',
cucItem:'الصنف',cucHolder:'صاحب العهدة',cucKind:'النوع',cucQty:'الكمية',cucSt:'الحالة',cucAt:'تاريخ التسليم',cucAck:'تاريخ التأكيد',cucRet:'تاريخ الإرجاع',cucBy:'سلّمها',cucNote:'الملاحظة',cucDept:'القسم',cucExp:'المتوقع',cucCnt:'المعدود',cucRes:'النتيجة',cucDate:'تاريخ الجرد',cucCby:'أجراه',
aSvNew:'صنف جديد',aSvRecv:'استلام مخزون',aSvAdj:'تعديل رصيد',aSvEdit:'تعديل',aSvAck:'اطلعت',aSvPur:'بانتظار الشراء',aSvDel:'تسليم',aSvOpenIssue:'فتح طلب الصرف',aCuNew:'عهدة مباشرة',aCuAck:'تأكيد الاستلام',aCuRet:'إرجاع',aCuDet:'تفاصيل',aCuNewC:'جرد جديد',aCuSave:'حفظ الجرد',
fSvNameAr:'الاسم بالعربية',fSvNameEn:'الاسم بالإنجليزية',fSvKind:'نوع الصنف',fSvMin:'الحد الأدنى',fSvActive:'فعّال',fSvItem:'الصنف',fSvQty:'الكمية',fSvCost:'التكلفة الإجمالية (للمحاسب فقط)',fSvIssue:'طلب صرف بانتظار الشراء (اختياري)',fSvNote:'ملاحظة',fSvReason:'السبب',fSvRev:'يعكس الحركة (اختياري)',fSvDelKind:'نوع العهدة',
fCuItem:'الصنف',fCuKind:'نوع العهدة',fCuEmp:'الموظف (للشخصية)',fCuDept:'القسم (للمشتركة)',fCuQty:'الكمية',fCuDeduct:'خصم من المخزون',fCuNote:'ملاحظة',fCuRestock:'إعادة الكمية للمخزون',fCuDate:'تاريخ الجرد',
dSvItem:'صنف',dSvItemB:'النوع لا يتغير بعد أول حركة. لا حذف: يُوقَف الصنف.',dSvRecv:'استلام مخزون',dSvRecvB:'التكلفة تُنشئ مصروف مشتريات عند المحاسب ولا تعود في أي استجابة. عند الشراء لطلب صرف معيّن تكون التكلفة إلزامية.',dSvAdj:'تعديل رصيد بحركة',dSvAdjB:'الحركات لا تُحذف ولا تُعدّل. أدخل كمية موجبة أو سالبة بسبب إلزامي، ولا يصير الرصيد سالباً.',dSvPur:'بانتظار الشراء',dSvDel:'تسليم الصرف',dSvDelB:'خصم الكمية يتم هنا فقط. الصنف المسترد يُنشئ عهدة بانتظار تأكيد الموظف، والمستهلك لا ينشئ عهدة.',
dCuNew:'عهدة مباشرة',dCuNewB:'عهدة شخصية باسم موظف أو مشتركة باسم قسم، وليس الاثنين. الصنف يجب أن يكون مسترداً.',dCuRet:'إرجاع عهدة',dCuRetB:'تُرجَع الكمية كاملة. إن لم تعد للمخزون (تالف) فالملاحظة إلزامية.',dCuDet:'تفاصيل العهدة',
nSvMoves:'حركات المخزون لا تُحذف ولا تُعدّل، والتصحيح بحركة تعديل بسبب إلزامي.',nSvIssues:'تظهر الطلبات هنا بعد انتهاء موافقاتها، أو مباشرة لطلبات الموارد البشرية والمدير التنفيذي. الرفض التلقائي غير موجود: عدم التوفر يعني «بانتظار الشراء».',nCuAll:'كل العهد لمسؤول المخزن ومدير الموارد البشرية.',nCuTeam:'تأكيد العهدة المشتركة يسجّل اسمك ولا يحمّلك مسؤولية شخصية عنها.',nCuCount:'الجرد لا يغيّر العهد ولا الرصيد ولا يحمّل المدير مسؤولية شخصية. تظهر نتيجته في التقرير الشهري، والفرق يُشعر مسؤول المخزن.',
fQItem:'القطعة (لنوع طلب قطعة من المخزن)',fQQty:'الكمية (للقطعة)',svIssueSt:'طلب الصرف',
emSvI:'لا توجد أصناف',emSvM:'لا توجد حركات',emSvS:'لا توجد طلبات صرف',emCuC:'لا توجد عهد',emCuN:'لا يوجد جرد',
tSvItem:'حُفظ الصنف (تجريبي)',tSvMove:'سُجّلت الحركة (تجريبي)',tSvAck:'سُجّل الاطلاع (تجريبي)',tSvPur:'سُجّل بانتظار الشراء (تجريبي)',tSvDel:'تم التسليم (تجريبي)',tCuNewT:'سُجّلت العهدة (تجريبي)',tCuAckT:'أُكّد الاستلام (تجريبي)',tCuRetT:'سُجّل الإرجاع (تجريبي)',tCuCntT:'سُجّل الجرد (تجريبي)',
er2:{'Hr:Item:NameAlreadyExists':'اسم الصنف مستخدم','Hr:Item:KindLocked':'لا يتغير النوع بعد أول حركة','Hr:Item:NotFound':'الصنف غير موجود','Hr:Stock:ItemInactive':'الصنف موقوف','Hr:Stock:PurchaseCostRequired':'التكلفة إلزامية عند الشراء لطلب صرف','Hr:Stock:IssueNotPurchasing':'طلب الصرف ليس بانتظار الشراء','Hr:Stock:IssueItemMismatch':'الصنف لا يطابق طلب الصرف','Hr:Stock:InsufficientStock':'الرصيد غير كافٍ','Hr:Stock:ZeroQuantity':'الكمية لا يمكن أن تكون صفراً','Hr:Stock:MovementAlreadyReversed':'عُكست هذه الحركة مسبقاً','Hr:Stock:IssueInvalidState':'حالة الصرف لا تسمح بهذا الإجراء','Hr:Stock:NotFound':'غير موجود','Hr:Custody:InvalidHolder':'العهدة لموظف أو لقسم، وليس الاثنين','Hr:Custody:ItemNotReturnable':'الصنف مستهلك وليس مسترداً','Hr:Custody:AlreadyAcknowledged':'أُكّدت العهدة مسبقاً','Hr:Custody:AlreadyReturned':'أُعيدت العهدة مسبقاً','Hr:Custody:ReturnNoteRequired':'الملاحظة إلزامية إن لم تعد الكمية للمخزون','Hr:Custody:CountLinesIncomplete':'يجب عدّ كل عهد القسم','Hr:Custody:CountLineInvalid':'سطر جرد غير صالح','Hr:Custody:DepartmentNotInScope':'القسم خارج نطاقك','Hr:Custody:NotFound':'العهدة غير موجودة'}},
en:{
svT:'Store',svSub:'Items, movements and stock-issue requests. Cost is entered on receipt and seen only by the accountant, never here',tSvItems:'Items',tSvMoves:'Stock movements',tSvIssues:'Stock-issue requests',
cuT:'Custody',cuSub:'My custody, team custody and count history',tCuMy:'My custody',tCuTeam:'Team and department custody',tCuAll:'All custody',tCuCount:'Counts',
sk:{returnable:'Returnable (custody)',consumable:'Consumable'},sist:{pendingAcknowledgement:'Waiting for the store',acknowledged:'Seen by the store',purchasing:'Waiting for purchase',delivered:'Delivered',cancelled:'Cancelled'},cust:{pendingAcknowledgement:'Waiting for acknowledgement',acknowledged:'Acknowledged',returned:'Returned'},ckind:{personal:'Personal',shared:'Shared'},mvt:{receive:'Receive',issue:'Issue',return:'Return',adjustment:'Adjustment'},cres:{matched:'Matched',discrepancy:'Discrepancy'},
svcName:'Item',svcKind:'Kind',svcMin:'Minimum',svcQty:'On hand',svcBelow:'Below minimum',svcAct:'Active',svcType:'Movement',svcAfter:'Balance after',svcBy:'By',svcNote:'Note',svcQ:'Quantity',svcAvail:'Available',svcEmp:'Employee',svcAt:'Time',
cucItem:'Item',cucHolder:'Holder',cucKind:'Kind',cucQty:'Quantity',cucSt:'Status',cucAt:'Assigned',cucAck:'Acknowledged',cucRet:'Returned',cucBy:'Assigned by',cucNote:'Note',cucDept:'Department',cucExp:'Expected',cucCnt:'Counted',cucRes:'Result',cucDate:'Count date',cucCby:'Counted by',
aSvNew:'New item',aSvRecv:'Receive stock',aSvAdj:'Adjust balance',aSvEdit:'Edit',aSvAck:'Seen',aSvPur:'Waiting for purchase',aSvDel:'Deliver',aSvOpenIssue:'Open stock issue',aCuNew:'Direct custody',aCuAck:'Acknowledge receipt',aCuRet:'Return',aCuDet:'Details',aCuNewC:'New count',aCuSave:'Save count',
fSvNameAr:'Name (Arabic)',fSvNameEn:'Name (English)',fSvKind:'Item kind',fSvMin:'Minimum quantity',fSvActive:'Active',fSvItem:'Item',fSvQty:'Quantity',fSvCost:'Total cost (accountant only)',fSvIssue:'Stock issue waiting for purchase (optional)',fSvNote:'Note',fSvReason:'Reason',fSvRev:'Reverses movement (optional)',fSvDelKind:'Custody kind',
fCuItem:'Item',fCuKind:'Custody kind',fCuEmp:'Employee (personal)',fCuDept:'Department (shared)',fCuQty:'Quantity',fCuDeduct:'Deduct from stock',fCuNote:'Note',fCuRestock:'Return the quantity to stock',fCuDate:'Count date',
dSvItem:'Item',dSvItemB:'The kind cannot change after the first movement. No deletion: deactivate the item.',dSvRecv:'Receive stock',dSvRecvB:'A cost creates a stock-purchase expense for the accountant and is never returned in any response. When buying for a specific issue the cost is mandatory.',dSvAdj:'Adjust balance by movement',dSvAdjB:'Movements are never deleted or edited. Enter a positive or negative quantity with a mandatory reason; the balance never goes negative.',dSvPur:'Waiting for purchase',dSvDel:'Deliver the issue',dSvDelB:'The quantity is deducted here only. A returnable item creates a custody waiting for the employee to acknowledge; a consumable creates none.',
dCuNew:'Direct custody',dCuNewB:'A personal custody for an employee or a shared one for a department, not both. The item must be returnable.',dCuRet:'Return custody',dCuRetB:'The full quantity is returned. If it does not go back to stock (damaged) a note is mandatory.',dCuDet:'Custody details',
nSvMoves:'Stock movements are never deleted or edited; a correction is an adjustment movement with a mandatory reason.',nSvIssues:'Requests appear here after their approvals finish, or directly for HR and executive requests. There is no automatic rejection: unavailability means “waiting for purchase”.',nCuAll:'All custody, for the store officer and the HR manager.',nCuTeam:'Acknowledging a shared custody records your name and does not make you personally responsible for it.',nCuCount:'A count changes neither custody nor stock and puts no personal responsibility on the manager. Its result appears in the monthly report, and a difference notifies the store officer.',
fQItem:'Item (for a store-item request type)',fQQty:'Quantity (for the item)',svIssueSt:'Stock issue',
emSvI:'No items',emSvM:'No movements',emSvS:'No stock-issue requests',emCuC:'No custody',emCuN:'No counts',
tSvItem:'Item saved (mock)',tSvMove:'Movement recorded (mock)',tSvAck:'Marked as seen (mock)',tSvPur:'Marked waiting for purchase (mock)',tSvDel:'Delivered (mock)',tCuNewT:'Custody recorded (mock)',tCuAckT:'Receipt acknowledged (mock)',tCuRetT:'Return recorded (mock)',tCuCntT:'Count recorded (mock)',
er2:{'Hr:Item:NameAlreadyExists':'The item name is already used','Hr:Item:KindLocked':'The kind cannot change after the first movement','Hr:Item:NotFound':'Item not found','Hr:Stock:ItemInactive':'The item is inactive','Hr:Stock:PurchaseCostRequired':'The cost is mandatory when buying for a stock issue','Hr:Stock:IssueNotPurchasing':'The stock issue is not waiting for purchase','Hr:Stock:IssueItemMismatch':'The item does not match the stock issue','Hr:Stock:InsufficientStock':'The stock is not sufficient','Hr:Stock:ZeroQuantity':'The quantity cannot be zero','Hr:Stock:MovementAlreadyReversed':'This movement was already reversed','Hr:Stock:IssueInvalidState':'The issue state does not allow this action','Hr:Stock:NotFound':'Not found','Hr:Custody:InvalidHolder':'A custody belongs to an employee or a department, not both','Hr:Custody:ItemNotReturnable':'The item is consumable, not returnable','Hr:Custody:AlreadyAcknowledged':'The custody was already acknowledged','Hr:Custody:AlreadyReturned':'The custody was already returned','Hr:Custody:ReturnNoteRequired':'A note is mandatory when the quantity does not return to stock','Hr:Custody:CountLinesIncomplete':'Every custody of the department must be counted','Hr:Custody:CountLineInvalid':'A count line is not valid','Hr:Custody:DepartmentNotInScope':'The department is outside your scope','Hr:Custody:NotFound':'Custody not found'}}};
Object.keys(T14).forEach(l=>{Object.assign(T4[l].er2,T14[l].er2);const o=Object.assign({},T14[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T14 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M4 7h16M4 12h16M4 17h10');IC.push('M12 2l8 4v6c0 5-3.5 8-8 10-4.5-2-8-5-8-10V6z');PAGES.push('inv');PAGES.push('cus');T.ar.nav.push('المخزن','العهد');T.en.nav.push('Store','Custody');
const OPR12={"Contract_List": "h", "Contract_Create": "h", "Contract_GetExpiring": "h", "Contract_Get": "h", "Contract_Recommend": "h", "Contract_Decide": "h", "Contract_GetMyList": "e", "Contract_GetMy": "e", "SystemSettings_GetContracts": "m", "SystemSettings_UpdateContracts": "m"};
NAVR.con="eh";
const T15={ar:{
ctT:'العقود',ctSub:'دورة التجديد: توصية ثم قرار موثقان. الموقف والتوصية لا يصلان إلى الموظف أبداً',tCtExp:'تنتهي قريباً',tCtAll:'كل العقود',tCtMy:'عقودي',
cst:{active:'ساري',pendingRecommendation:'بانتظار التوصية',pendingDecision:'بانتظار القرار',renewed:'مُجدَّد',notRenewed:'غير مُجدَّد'},est:{active:'ساري',renewed:'مُجدَّد',notRenewed:'غير مُجدَّد'},copt:{renew:'تجديد',doNotRenew:'عدم التجديد'},
ctcEmp:'الموظف',ctcType:'نوع العقد',ctcStart:'البداية',ctcEnd:'النهاية',ctcDays:'المتبقي (يوم)',ctcSt:'الحالة',ctcDue:'موعد القرار',ctcOver:'متأخر',ctcPos:'موقف الموظف',ctcRecm:'التوصية',ctcNote:'الملاحظة',ctcDecn:'القرار',ctcMsg:'رسالة الموظف',ctcPrev:'يجدّد العقد',ctcNot:'أُبلغ الموظف',ctcExp:'منتهٍ',ctcBy:'بواسطة',ctcAt:'الوقت',
aCtNew:'عقد جديد',aCtOpen:'فتح',aCtRec:'رفع التوصية',aCtDec:'اتخاذ القرار',aCtRenew:'إنشاء عقد التجديد',
fCtEmp:'الموظف',fCtType:'نوع العقد',fCtStart:'تاريخ البداية',fCtEnd:'تاريخ النهاية',fCtPrev:'العقد السابق (للتجديد)',fCtPos:'موقف الموظف',fCtOpt:'التوصية',fCtNote:'ملاحظة',fCtMsg:'رسالة للموظف (تصله فور القرار)',fCtDays:'مدة تنبيه العقد (أيام)',fCtWithin:'خلال (أيام)',
dCtNew:'عقد جديد',dCtNewB:'لا يصل إشعار إلى الموظف عند الإنشاء. عقد التجديد يُنشأ هنا بربطه بالعقد السابق المُجدَّد.',dCtRec:'التوصية (الخطوة الموثقة الأولى)',dCtRecB:'لا يصل شيء منها إلى الموظف. تنقل العقد إلى «بانتظار القرار» وتحدد موعد القرار بعد 5 أيام دوام.',dCtDec:'القرار (الخطوة الموثقة الثانية)',dCtDecB:'القرار قد يخالف التوصية ويبقى كلاهما محفوظاً. يُبلَّغ الموظف تلقائياً وفوراً بقرارك ورسالتك.',
nCtFlow:'التأخر في القرار لا يمنع إصداره: تذكير وعلامة «متأخر» فقط.',nCtEmp:'هذا العرض لا يتضمن الموقف ولا التوصية.',nCtSelf:'لا يتخذ أحد خطوة على عقده الشخصي.',
emCt:'لا توجد عقود',ctDaysLeft:'يوم',
tCtNew:'أُنشئ العقد (تجريبي)',tCtRec:'رُفعت التوصية (تجريبي)',tCtDec:'صدر القرار وأُبلغ الموظف (تجريبي)',
ctrT:'مدة تنبيه العقد',ctrSub:'عدد الأيام قبل انتهاء العقد التي يدخل بعدها مسار التوصية والقرار',ctrNote:'القيمة الأولية 30 يوماً، ويسري التغيير من التشغيل اليومي التالي. لا تمنح هذه الشاشة أي حق في معالجة الشكاوى.',ctrSave:'حفظ',
tRHCon:'العقود',
er2:{'Hr:Contract:EmployeeNotFound':'الموظف غير موجود','Hr:Contract:InvalidDateRange':'يجب أن تكون النهاية بعد البداية','Hr:Contract:OverlappingContract':'يتداخل مع عقد قائم للموظف','Hr:Contract:InvalidPreviousContract':'العقد السابق غير صالح للتجديد','Hr:Contract:InvalidStatus':'حالة العقد لا تسمح بهذه الخطوة','Hr:Contract:SelfActionNotAllowed':'لا يتخذ أحد خطوة على عقده الشخصي','Hr:Contract:NotFound':'العقد غير موجود'}},
en:{
ctT:'Contracts',ctSub:'Renewal cycle: a documented recommendation then a decision. The position and recommendation never reach the employee',tCtExp:'Expiring soon',tCtAll:'All contracts',tCtMy:'My contracts',
cst:{active:'Active',pendingRecommendation:'Waiting for recommendation',pendingDecision:'Waiting for decision',renewed:'Renewed',notRenewed:'Not renewed'},est:{active:'Active',renewed:'Renewed',notRenewed:'Not renewed'},copt:{renew:'Renew',doNotRenew:'Do not renew'},
ctcEmp:'Employee',ctcType:'Contract type',ctcStart:'Start',ctcEnd:'End',ctcDays:'Days left',ctcSt:'Status',ctcDue:'Decision due',ctcOver:'Overdue',ctcPos:'Employee position',ctcRecm:'Recommendation',ctcNote:'Note',ctcDecn:'Decision',ctcMsg:'Employee message',ctcPrev:'Renews contract',ctcNot:'Employee notified',ctcExp:'Expired',ctcBy:'By',ctcAt:'Time',
aCtNew:'New contract',aCtOpen:'Open',aCtRec:'Submit recommendation',aCtDec:'Decide',aCtRenew:'Create renewal contract',
fCtEmp:'Employee',fCtType:'Contract type',fCtStart:'Start date',fCtEnd:'End date',fCtPrev:'Previous contract (for a renewal)',fCtPos:'Employee position',fCtOpt:'Recommendation',fCtNote:'Note',fCtMsg:'Message to the employee (sent immediately on decision)',fCtDays:'Contract alert period (days)',fCtWithin:'Within (days)',
dCtNew:'New contract',dCtNewB:'No notification reaches the employee on creation. A renewal contract is created here by linking it to the previous renewed contract.',dCtRec:'Recommendation (first documented step)',dCtRecB:'Nothing of it reaches the employee. It moves the contract to “waiting for decision” and sets the decision due date 5 working days later.',dCtDec:'Decision (second documented step)',dCtDecB:'The decision may differ from the recommendation and both stay on record. The employee is notified automatically and immediately with your decision and message.',
nCtFlow:'A late decision does not prevent issuing it: only a reminder and a “late” flag.',nCtEmp:'This view contains neither the position nor the recommendation.',nCtSelf:'Nobody takes a step on their own contract.',
emCt:'No contracts',ctDaysLeft:'days',
tCtNew:'Contract created (mock)',tCtRec:'Recommendation submitted (mock)',tCtDec:'Decision issued and the employee notified (mock)',
ctrT:'Contract alert period',ctrSub:'Days before a contract ends after which it enters the recommendation and decision path',ctrNote:'The initial value is 30 days and a change applies from the next daily run. This screen grants no right to handle complaints.',ctrSave:'Save',
tRHCon:'Contracts',
er2:{'Hr:Contract:EmployeeNotFound':'The employee was not found','Hr:Contract:InvalidDateRange':'The end must come after the start','Hr:Contract:OverlappingContract':'It overlaps an existing contract of the employee','Hr:Contract:InvalidPreviousContract':'The previous contract is not valid for a renewal','Hr:Contract:InvalidStatus':'The contract status does not allow this step','Hr:Contract:SelfActionNotAllowed':'Nobody takes a step on their own contract','Hr:Contract:NotFound':'Contract not found'}}};
Object.keys(T15).forEach(l=>{Object.assign(T4[l].er2,T15[l].er2);const o=Object.assign({},T15[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T15 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M9 12h6M9 16h6M14 3H6a1 1 0 0 0-1 1v16a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V8zM14 3v5h5');PAGES.push('con');T.ar.nav.push('العقود');T.en.nav.push('Contracts');
const OPR13={"Report_GetHrWarnings": "h", "Warning_Create": "dv", "Warning_List": "h", "Warning_GetObjections": "x", "Warning_GetRecommendedByMe": "dv", "Warning_GetMyList": "e", "Warning_GetMy": "e", "Warning_GetEmployeeFile": "dh", "Warning_Get": "hx", "Warning_GetAttachmentDownloadUrl": "hx", "Warning_Notify": "h", "Warning_Respond": "e", "Warning_Decide": "h", "Warning_Object": "e", "Warning_ResolveObjection": "x"};
NAVR.wrn="edhvx";
const T16={ar:{
wrT:'الإنذارات',wrSub:'توصية ثم إبلاغ وردّ ثم قرار الموارد البشرية، ويُعترض على القرار أمام المدير التنفيذي. التوصية لا تعني إنذاراً',
tWrMy:'إنذاراتي',tWrRec:'توصياتي',tWrFile:'سجل موظف',tWrHr:'كل الإنذارات',tWrObj:'الاعتراضات',
wlv:{first:'أولى',final:'نهائية'},wst:{recommended:'توصية بانتظار المراجعة',notified:'أُبلغ الموظف',responded:'رد الموظف',responseExpired:'انتهت مهلة الرد',issued:'صدر الإنذار',dismissed:'حُفظ',objected:'اعتراض معلّق',objectionUpheld:'اعتراض مرفوض (الإنذار قائم)',objectionOverturned:'اعتراض مقبول (أُلغي الإنذار)'},wstg:{underReview:'قيد المراجعة',issued:'صدر',notIssued:'لم يصدر'},wdo:{issue:'إصدار الإنذار',dismiss:'حفظ (عدم الإصدار)'},wob:{uphold:'تأييد القرار',overturn:'إلغاء الإنذار'},wrr:{departmentManager:'مدير قسم',reviewer:'مراجع'},
wrcEmp:'الموظف',wrcLvl:'الدرجة',wrcDate:'تاريخ الواقعة',wrcSt:'الحالة',wrcBy:'صاحب التوصية',wrcDue:'مهلة الرد',wrcDesc:'وصف الواقعة',wrcStage:'المرحلة',wrcResp:'رد الموظف',wrcDec:'القرار',wrcObj:'الاعتراض',wrcNote:'الملاحظة',wrcAt:'الوقت',wrcFile:'المستند',wrcSize:'الحجم',wrcRole:'دور صاحب التوصية',wrcNot:'أُبلغ في',wrcLate:'انتهت',
aWrNew:'توصية إنذار',aWrOpen:'فتح',aWrNotify:'تسجيل الواقعة وإبلاغ الموظف',aWrDec:'القرار',aWrResp:'الرد على الإنذار',aWrObj:'الاعتراض على القرار',aWrRes:'البتّ في الاعتراض',aWrDl:'تنزيل',
fWrEmp:'الموظف',fWrLvl:'الدرجة المقترحة',fWrDate:'تاريخ الواقعة',fWrDesc:'وصف الواقعة',fWrAtt:'المستندات (اختياري، حتى 10)',fWrAttH:'PDF أو JPEG أو PNG حتى 10 ميجابايت',fWrDecision:'القرار',fWrIssued:'درجة الإنذار الصادر',fWrNote:'الملاحظة',fWrContent:'النص',fWrOutcome:'القرار النهائي',fWrPending:'قيد البت',
dWrNew:'توصية إنذار',dWrNewB:'توصية وليست إنذاراً: لا يصدر الإنذار بمجرد التوصية، ولا يصل شيء إلى الموظف في هذه الخطوة. تصل توصيتك إلى مدير الموارد البشرية.',dWrNotify:'تسجيل الواقعة وإبلاغ الموظف',dWrNotifyB:'يُبلَّغ الموظف بإشعار رسمي وله الرد خلال 3 أيام دوام ولمرة واحدة.',dWrDec:'قرار الموارد البشرية',dWrDecB:'لا يُصدَر القرار قبل رد الموظف أو انتهاء مهلته. القرار والرد يُحفظان في ملف الموظف ولا يُحذفان.',dWrResp:'الرد على الإنذار',dWrRespB:'يمكنك الرد مرة واحدة حتى نهاية يوم مهلة الرد.',dWrObj:'الاعتراض على القرار',dWrObjB:'مرة واحدة، ويبقى الإنذار نافذاً حتى البت. المدة المعتمدة 5 أيام دوام من إبلاغك بالقرار. يبتّ فيه المدير التنفيذي.',dWrRes:'البتّ في الاعتراض',dWrResB:'قرارك نهائي ولا يُحذف السجل في أي حالة.',
nWrMy:'ما أُبلغتَ به فقط. التوصية قبل الإبلاغ لا تظهر لك.',nWrFile:'سجل الإنذارات بعد القرار. مدير القسم يرى الإنذارات النافذة لموظفيه فقط، والموارد البشرية ترى الكل بما فيها المحفوظة والملغاة.',nWrExec:'المدير التنفيذي يرى الإنذارات التي عليها اعتراض فقط.',nWrRec:'ترى مرحلة توصياتك فقط، دون رد الموظف أو اعتراضه أو ملاحظات القرار.',nWrFlow:'مهلة الرد 3 أيام دوام من الإبلاغ.',nWrPick:'اختر موظفاً لعرض سجله.',
emWr:'لا توجد إنذارات',emWrF:'لا توجد سجلات',emWrA:'لا توجد مستندات',
tWrNew:'رُفعت التوصية (تجريبي)',tWrNot:'سُجّلت الواقعة وأُبلغ الموظف (تجريبي)',tWrDec:'صدر القرار (تجريبي)',tWrResp:'سُجّل ردك (تجريبي)',tWrObjD:'سُجّل اعتراضك (تجريبي)',tWrRes:'تم البتّ في الاعتراض (تجريبي)',tWrDl:'جُهّز رابط التنزيل (تجريبي)',
tRHWrn:'الإنذارات',
er2:{'Hr:Warning:EmployeeNotFound':'الموظف غير موجود','Hr:Warning:TargetNotInTeam':'الموظف ليس من فريقك','Hr:Warning:TargetIsDepartmentManager':'لا يوصي مدير القسم على مدير قسم','Hr:Warning:TargetNotDepartmentManager':'المراجع يوصي على مدير قسم فقط','Hr:Warning:SelfRecommendationNotAllowed':'لا يمكن التوصية على نفسك','Hr:Warning:IncidentDateInFuture':'تاريخ الواقعة بعد اليوم','Hr:Warning:AttachmentRejected':'رُفض المستند بعد الفحص','Hr:Warning:InvalidStatus':'حالة الإنذار لا تسمح بهذه الخطوة','Hr:Warning:SelfDecisionNotAllowed':'لا يتخذ أحد إجراء على إنذاره الشخصي','Hr:Warning:ResponseWindowExpired':'انتهت مهلة الرد','Hr:Warning:ResponseAlreadySubmitted':'سبق أن ردّ الموظف','Hr:Warning:ResponseWindowOpen':'مهلة الرد ما زالت مفتوحة','Hr:Warning:LevelRequired':'حدّد درجة الإنذار عند الإصدار','Hr:Warning:ObjectionAlreadySubmitted':'سبق تقديم اعتراض','Hr:Warning:NotFound':'غير موجود'}},
en:{
wrT:'Warnings',wrSub:'Recommendation, notice and response, then the HR decision; the decision can be objected to before the executive director. A recommendation is not a warning',
tWrMy:'My warnings',tWrRec:'My recommendations',tWrFile:'Employee record',tWrHr:'All warnings',tWrObj:'Objections',
wlv:{first:'First',final:'Final'},wst:{recommended:'Recommendation under review',notified:'Employee notified',responded:'Employee responded',responseExpired:'Response window expired',issued:'Warning issued',dismissed:'Dismissed',objected:'Objection pending',objectionUpheld:'Objection rejected (warning stands)',objectionOverturned:'Objection accepted (warning cancelled)'},wstg:{underReview:'Under review',issued:'Issued',notIssued:'Not issued'},wdo:{issue:'Issue the warning',dismiss:'Dismiss (do not issue)'},wob:{uphold:'Uphold the decision',overturn:'Cancel the warning'},wrr:{departmentManager:'Department manager',reviewer:'Reviewer'},
wrcEmp:'Employee',wrcLvl:'Level',wrcDate:'Incident date',wrcSt:'Status',wrcBy:'Recommended by',wrcDue:'Response due',wrcDesc:'Incident description',wrcStage:'Stage',wrcResp:'Employee response',wrcDec:'Decision',wrcObj:'Objection',wrcNote:'Note',wrcAt:'Time',wrcFile:'Document',wrcSize:'Size',wrcRole:'Recommender role',wrcNot:'Notified at',wrcLate:'Expired',
aWrNew:'Recommend a warning',aWrOpen:'Open',aWrNotify:'Record the incident and notify',aWrDec:'Decide',aWrResp:'Respond to the warning',aWrObj:'Object to the decision',aWrRes:'Decide the objection',aWrDl:'Download',
fWrEmp:'Employee',fWrLvl:'Recommended level',fWrDate:'Incident date',fWrDesc:'Incident description',fWrAtt:'Documents (optional, up to 10)',fWrAttH:'PDF, JPEG or PNG up to 10 MB',fWrDecision:'Decision',fWrIssued:'Issued warning level',fWrNote:'Note',fWrContent:'Text',fWrOutcome:'Final decision',fWrPending:'Pending',
dWrNew:'Warning recommendation',dWrNewB:'A recommendation, not a warning: a warning is not issued by recommending it, and nothing reaches the employee at this step. Your recommendation goes to the HR manager.',dWrNotify:'Record the incident and notify the employee',dWrNotifyB:'The employee receives an official notice and may respond within 3 working days, once.',dWrDec:'HR decision',dWrDecB:'The decision cannot be issued before the employee responds or the window expires. The decision and response are kept in the employee’s record and never deleted.',dWrResp:'Respond to the warning',dWrRespB:'You can respond once until the end of the response due date.',dWrObj:'Object to the decision',dWrObjB:'Once, and the warning stands until decided. The approved window is 5 working days from being notified of the decision. The executive director decides it.',dWrRes:'Decide the objection',dWrResB:'Your decision is final and the record is never deleted.',
nWrMy:'Only what you were notified of. A recommendation before notification is not visible to you.',nWrFile:'Warning record after the decision. A department manager sees only the effective warnings of their employees; HR sees everything including dismissed and cancelled ones.',nWrExec:'The executive director sees only warnings with an objection.',nWrRec:'You see only the stage of your recommendations, without the employee’s response, objection or decision notes.',nWrFlow:'The response window is 3 working days from notification.',nWrPick:'Choose an employee to see their record.',
emWr:'No warnings',emWrF:'No records',emWrA:'No documents',
tWrNew:'Recommendation submitted (mock)',tWrNot:'Incident recorded and the employee notified (mock)',tWrDec:'Decision issued (mock)',tWrResp:'Your response was recorded (mock)',tWrObjD:'Your objection was recorded (mock)',tWrRes:'Objection decided (mock)',tWrDl:'Download link prepared (mock)',
tRHWrn:'Warnings',
er2:{'Hr:Warning:EmployeeNotFound':'The employee was not found','Hr:Warning:TargetNotInTeam':'The employee is not in your team','Hr:Warning:TargetIsDepartmentManager':'A department manager does not recommend on another department manager','Hr:Warning:TargetNotDepartmentManager':'A reviewer recommends on a department manager only','Hr:Warning:SelfRecommendationNotAllowed':'You cannot recommend on yourself','Hr:Warning:IncidentDateInFuture':'The incident date is after today','Hr:Warning:AttachmentRejected':'The document was rejected after scanning','Hr:Warning:InvalidStatus':'The warning status does not allow this step','Hr:Warning:SelfDecisionNotAllowed':'Nobody acts on their own warning','Hr:Warning:ResponseWindowExpired':'The response window expired','Hr:Warning:ResponseAlreadySubmitted':'The employee already responded','Hr:Warning:ResponseWindowOpen':'The response window is still open','Hr:Warning:LevelRequired':'Choose the warning level when issuing','Hr:Warning:ObjectionAlreadySubmitted':'An objection was already submitted','Hr:Warning:NotFound':'Not found'}}};
Object.keys(T16).forEach(l=>{Object.assign(T4[l].er2,T16[l].er2);const o=Object.assign({},T16[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T16 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M12 3l9 16H3zM12 10v4M12 17h.01');PAGES.push('wrn');T.ar.nav.push('الإنذارات');T.en.nav.push('Warnings');
const OPR14={"AppreciationLetter_Create": "x", "AppreciationLetter_List": "hx", "AppreciationLetter_GetMyList": "e", "AppreciationLetter_GetEmployeeFile": "dhx", "AppreciationLetter_Get": "edhx", "AppreciationLetter_GetDownloadUrl": "edhx", "AppreciationLetter_Acknowledge": "e"};
NAVR.apr="edhx";
const T17={ar:{
apT:'كتب الشكر',apSub:'يصدرها المدير التنفيذي بالتنسيق مع الإدارة وتُحفظ في ملف الموظف، ويؤكد الموظف الاستلام',tApMy:'كتب الشكر لي',tApAll:'كل الكتب',tApFile:'سجل موظف',
apcEmp:'الموظف',apcTitle:'العنوان',apcNote:'الملاحظة',apcBy:'أصدره',apcAt:'تاريخ الإصدار',apcSt:'الحالة',apcFile:'الملف',apcAckAt:'وقت التأكيد',apAcked:'مؤكَّد',apNeed:'بانتظار التأكيد',
aApNew:'إصدار كتاب شكر',aApOpen:'فتح',aApAck:'تأكيد الاستلام',aApDl:'تنزيل الملف',
fApEmp:'الموظف',fApTitle:'العنوان',fApNote:'ملاحظة (اختياري)',fApFile:'ملف الكتاب',fApFileH:'PDF أو JPEG أو PNG حتى 10 ميجابايت. الكتاب يُوقَّع خارج النظام ثم يُرفع هنا.',
dApNew:'إصدار كتاب شكر',dApNewB:'يُحفظ في ملف الموظف ويصله إشعار. لا يقترحه مدير القسم ولا توجد عملية توصية.',
nApMy:'الكتب الموجهة إليك فقط.',nApFile:'سجل كتب الشكر. مدير القسم يرى موظفيه فقط.',nApPick:'اختر موظفاً لعرض سجله.',
emAp:'لا توجد كتب',tApNewT:'أُصدر الكتاب (تجريبي)',tApAckT:'أُكّد الاستلام (تجريبي)',tApDlT:'جُهّز رابط التنزيل (تجريبي)',
er2:{'Hr:Appreciation:EmployeeNotFound':'الموظف غير موجود','Hr:Appreciation:SelfIssueNotAllowed':'لا يُصدَر كتاب شكر لنفسك','Hr:Appreciation:FileRejected':'رُفض الملف بعد الفحص','Hr:Appreciation:NotFound':'غير موجود'}},
en:{
apT:'Appreciation letters',apSub:'Issued by the executive director in coordination with management and kept in the employee record; the employee acknowledges receipt',tApMy:'My letters',tApAll:'All letters',tApFile:'Employee record',
apcEmp:'Employee',apcTitle:'Title',apcNote:'Note',apcBy:'Issued by',apcAt:'Issued',apcSt:'Status',apcFile:'File',apcAckAt:'Acknowledged at',apAcked:'Acknowledged',apNeed:'Waiting for acknowledgement',
aApNew:'Issue a letter',aApOpen:'Open',aApAck:'Acknowledge receipt',aApDl:'Download file',
fApEmp:'Employee',fApTitle:'Title',fApNote:'Note (optional)',fApFile:'Letter file',fApFileH:'PDF, JPEG or PNG up to 10 MB. The letter is signed outside the system, then uploaded here.',
dApNew:'Issue an appreciation letter',dApNewB:'It is kept in the employee record and the employee is notified. A department manager does not propose it and there is no recommendation operation.',
nApMy:'Only letters addressed to you.',nApFile:'Appreciation-letter record. A department manager sees their employees only.',nApPick:'Choose an employee to see their record.',
emAp:'No letters',tApNewT:'Letter issued (mock)',tApAckT:'Receipt acknowledged (mock)',tApDlT:'Download link prepared (mock)',
er2:{'Hr:Appreciation:EmployeeNotFound':'The employee was not found','Hr:Appreciation:SelfIssueNotAllowed':'You cannot issue a letter to yourself','Hr:Appreciation:FileRejected':'The file was rejected after scanning','Hr:Appreciation:NotFound':'Not found'}}};
Object.keys(T17).forEach(l=>{Object.assign(T4[l].er2,T17[l].er2);const o=Object.assign({},T17[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T17 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M12 2l3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1z');PAGES.push('apr');T.ar.nav.push('كتب الشكر');T.en.nav.push('Appreciation');
const OPR15={"SalaryProfile_List": "a", "SalaryProfile_Get": "a", "SalaryProfile_Update": "a", "SalaryProfile_GetHistory": "a", "Adjustment_List": "a", "Adjustment_Create": "a", "Adjustment_Reverse": "a", "Adjustment_GetAttachmentDownloadUrl": "a", "AdjustmentSuggestion_List": "a", "AdjustmentSuggestion_Approve": "a", "AdjustmentSuggestion_Reject": "a", "Expense_List": "a", "Expense_Create": "a", "Expense_Export": "a", "Expense_Get": "a", "Expense_Reverse": "a", "Expense_GetAttachmentDownloadUrl": "a", "AccountantRequest_List": "a", "AccountantRequest_Get": "a", "SystemSettings_GetFinance": "m", "SystemSettings_UpdateFinance": "m"};
NAVR.fin="a";
const T18={ar:{
fiT:'المالية',fiSub:'بيانات حساسة: كل اطلاع يُسجَّل في سجل التدقيق ولا يراها غير المحاسب. لا يعدّل أحد بياناته المالية بنفسه',
tFiSal:'الرواتب',tFiAdj:'الإضافات والاستقطاعات',tFiSug:'اقتراحات الحضور',tFiExp:'المصروفات',tFiReq:'طلبات المحاسب',
fadt:{addition:'إضافة',deduction:'استقطاع'},fads:{manual:'يدوي',attendanceSuggestion:'اقتراح حضور',reversal:'عكس'},fadst:{open:'مفتوحة',includedInRun:'ضمن كشف',reversed:'معكوسة'},fsgb:{lateness:'تأخر',absence:'غياب',overtime:'عمل إضافي',attendanceCorrectionSettlement:'تسوية تصحيح حضور'},fsgu:{minutes:'دقيقة',hours:'ساعة',days:'يوم'},fsgs:{pendingReview:'بانتظار المراجعة',approved:'موافَق عليه',rejected:'مرفوض'},fexs:{general:'عام',storePurchase:'مشتريات المخزن'},fexst:{active:'نشط',reversed:'معكوس'},farq:{pendingAccountant:'بانتظار المحاسب',completed:'مكتمل',rejected:'مرفوض'},fcomp:{allowance:'بدل',fixedDeduction:'استقطاع ثابت'},fpar:{hrManager:'مدير الموارد البشرية',executiveDirector:'المدير التنفيذي'},
ficEmp:'الموظف',ficProf:'له بيانات مالية',ficBase:'الراتب الأساسي',ficCur:'العملة',ficFrom:'سارٍ من',ficComp:'المخصصات والاستقطاعات',ficName:'الاسم',ficType:'النوع',ficAmt:'المبلغ',ficPer:'الفترة',ficReason:'السبب',ficSrc:'المصدر',ficSt:'الحالة',ficBy:'بواسطة',ficAt:'الوقت',ficBasis:'الأساس',ficQty:'الكمية',ficSug:'المبلغ المقترح',ficSet:'تسوية لفترة',ficCat:'التصنيف',ficDate:'التاريخ',ficDesc:'الوصف',ficArr:'وصل في',ficDec:'حُسم في',ficLinked:'حركات مرتبطة',ficHist:'نسخ البيانات المالية',ficNone:'لا بيانات',
aFiOpen:'فتح',aFiUpd:'نسخة جديدة',aFiNewA:'حركة جديدة',aFiNewE:'مصروف جديد',aFiRev:'عكس',aFiDl:'المرفق',aFiApp:'موافقة',aFiRej:'رفض',aFiPdf:'تصدير PDF',aFiXls:'تصدير Excel',aFiAppr:'موافقة على الطلب',aFiRejr:'رفض الطلب',
fFiBase:'الراتب الأساسي',fFiCur:'العملة (3 أحرف)',fFiFrom:'تاريخ السريان',fFiComps:'المخصصات (سطر لكل واحد: الاسم | بدل أو استقطاع | المبلغ)',fFiPer:'الفترة (سنة-شهر)',fFiType:'النوع',fFiAmt:'المبلغ',fFiReason:'السبب',fFiAtt:'المرفق (إلزامي)',fFiReq:'طلب مرتبط بخطوة المحاسب (اختياري)',fFiCat:'التصنيف',fFiDate:'التاريخ',fFiDesc:'الوصف',fFiOver:'مبلغ معدّل (اختياري)',fFiRole:'معتمد الكشف الشهري',fFiHas:'له بيانات',
dFiUpd:'نسخة جديدة من البيانات المالية',dFiUpdB:'نسخة جديدة بتاريخ سريان بعد آخر نسخة، ولا تُعدَّل النسخ السابقة. الكشوف المسودة المتأثرة تصبح قديمة.',dFiAdj:'حركة يدوية',dFiAdjB:'إضافة أو استقطاع بسبب ومرفق إلزاميين. العملة من البيانات المالية السارية، ولا تُقبل فترة لها كشف معتمد أو مغلق (استخدم الفترة التالية).',dFiRev:'عكس حركة',dFiRevB:'تُنشأ حركة مقابلة بنوع معاكس في الفترة التي تحددها، ولا حذف أو تعديل.',dFiApp:'موافقة على الاقتراح',dFiAppB:'موافقتك هي الطريق الوحيد لتطبيق الاقتراح. اترك المبلغ فارغاً لاعتماد المقترح.',dFiRej:'رفض الاقتراح',dFiExp:'مصروف عام',dFiExpB:'مصروف عام بتصنيف ومرفق. مشتريات المخزن تصل تلقائياً من استلام المخزون ولا تُنشأ هنا.',dFiRevE:'عكس مصروف',dFiRevEB:'تُنشأ حركة بمبلغ سالب ويصبح الأصل معكوساً.',
nFiSal:'كل قراءة لبيانات مالية تُسجَّل في سجل التدقيق باسمك، بلا قيم.',nFiSug:'الاقتراحات مولَّدة من الحضور ولا تؤثر في أي كشف قبل موافقتك. تصحيح الحضور بعد إغلاق الشهر يولّد تسوية في الشهر التالي.',nFiExp:'مشتريات المخزن تصل تلقائياً بمصدر «مشتريات المخزن». التكلفة تُدخل في استلام المخزون وتظهر هنا فقط.',nFiReq:'طلبات المحاسب التي وصلت إلى خطوته فقط. الموافقة أو الرفض عبر مسار الطلبات نفسه.',nFiCur:'العملة الافتراضية في التجربة: IQD. العملة والبلد لم يُحسما بعد.',
emFi:'لا توجد بيانات',emFiH:'لا توجد نسخ',emFiT:'لا توجد أحداث',
tFiUpd:'حُفظت النسخة الجديدة (تجريبي)',tFiAdjT:'سُجّلت الحركة (تجريبي)',tFiRevT:'سُجّل العكس (تجريبي)',tFiAppT:'طُبّق الاقتراح (تجريبي)',tFiRejT:'رُفض الاقتراح (تجريبي)',tFiExpT:'سُجّل المصروف (تجريبي)',tFiDlT:'جُهّز رابط التنزيل (تجريبي)',tFiXp:'جُهّز الملف (تجريبي)',tFiSet:'حُفظ معتمد الكشف (تجريبي)',
finT:'إعدادات المالية',finSub:'من يعتمد الكشف الشهري',finNote:'لا تتضمن هذه الشاشة أي بيانات مالية، ولا يؤثر التغيير في الكشوف المعتمدة سابقاً. لا تمنح حقاً في معالجة الشكاوى.',
er2:{'Hr:Finance:EffectiveDateNotAfterLatest':'تاريخ السريان يجب أن يكون بعد آخر نسخة','Hr:Finance:EffectiveDateInLockedPeriod':'فترة السريان لها كشف معتمد أو مغلق','Hr:Finance:DuplicateComponentName':'اسم مخصص مكرر','Hr:Finance:SelfChangeNotAllowed':'لا يعدّل أحد بياناته بنفسه، يُحال إلى المدير التنفيذي','Hr:Finance:EmployeeNotFound':'الموظف غير موجود','Hr:Finance:SalaryProfileMissing':'لا توجد بيانات مالية سارية للموظف','Hr:Finance:PeriodLocked':'للفترة كشف معتمد أو مغلق، استخدم الفترة التالية','Hr:Finance:AttachmentRejected':'رُفض المرفق بعد الفحص','Hr:Finance:RequestNotAtAccountantStep':'الطلب ليس عند خطوة المحاسب','Hr:Finance:AdjustmentAlreadyReversed':'عُكست الحركة مسبقاً','Hr:Finance:SuggestionAlreadyReviewed':'سبقت مراجعة الاقتراح','Hr:Finance:SuggestionAmountRequired':'أدخل المبلغ: لا مبلغ مقترحاً','Hr:Finance:ExpenseAlreadyReversed':'عُكس المصروف مسبقاً','Hr:Finance:NotFound':'غير موجود'}},
en:{
fiT:'Finance',fiSub:'Sensitive data: every read is recorded in the audit log and only the accountant sees it. Nobody changes their own financial data',
tFiSal:'Salaries',tFiAdj:'Additions and deductions',tFiSug:'Attendance suggestions',tFiExp:'Expenses',tFiReq:'Accountant requests',
fadt:{addition:'Addition',deduction:'Deduction'},fads:{manual:'Manual',attendanceSuggestion:'Attendance suggestion',reversal:'Reversal'},fadst:{open:'Open',includedInRun:'In a run',reversed:'Reversed'},fsgb:{lateness:'Lateness',absence:'Absence',overtime:'Overtime',attendanceCorrectionSettlement:'Attendance correction settlement'},fsgu:{minutes:'min',hours:'h',days:'days'},fsgs:{pendingReview:'Pending review',approved:'Approved',rejected:'Rejected'},fexs:{general:'General',storePurchase:'Store purchase'},fexst:{active:'Active',reversed:'Reversed'},farq:{pendingAccountant:'Waiting for the accountant',completed:'Completed',rejected:'Rejected'},fcomp:{allowance:'Allowance',fixedDeduction:'Fixed deduction'},fpar:{hrManager:'HR manager',executiveDirector:'Executive director'},
ficEmp:'Employee',ficProf:'Has a profile',ficBase:'Base salary',ficCur:'Currency',ficFrom:'Effective from',ficComp:'Allowances and deductions',ficName:'Name',ficType:'Type',ficAmt:'Amount',ficPer:'Period',ficReason:'Reason',ficSrc:'Source',ficSt:'Status',ficBy:'By',ficAt:'Time',ficBasis:'Basis',ficQty:'Quantity',ficSug:'Suggested amount',ficSet:'Settlement for period',ficCat:'Category',ficDate:'Date',ficDesc:'Description',ficArr:'Arrived',ficDec:'Decided',ficLinked:'Linked entries',ficHist:'Financial profile versions',ficNone:'No data',
aFiOpen:'Open',aFiUpd:'New version',aFiNewA:'New entry',aFiNewE:'New expense',aFiRev:'Reverse',aFiDl:'Attachment',aFiApp:'Approve',aFiRej:'Reject',aFiPdf:'Export PDF',aFiXls:'Export Excel',aFiAppr:'Approve request',aFiRejr:'Reject request',
fFiBase:'Base salary',fFiCur:'Currency (3 letters)',fFiFrom:'Effective date',fFiComps:'Components (one per line: name | allowance or deduction | amount)',fFiPer:'Period (year-month)',fFiType:'Type',fFiAmt:'Amount',fFiReason:'Reason',fFiAtt:'Attachment (mandatory)',fFiReq:'Request at the accountant step (optional)',fFiCat:'Category',fFiDate:'Date',fFiDesc:'Description',fFiOver:'Adjusted amount (optional)',fFiRole:'Monthly payroll approver',fFiHas:'Has a profile',
dFiUpd:'New version of the financial profile',dFiUpdB:'A new version with an effective date after the latest one; earlier versions are never edited. Affected draft payroll runs become stale.',dFiAdj:'Manual entry',dFiAdjB:'An addition or deduction with a mandatory reason and attachment. The currency comes from the profile in effect; a period with an approved or closed run is not accepted (use the next period).',dFiRev:'Reverse an entry',dFiRevB:'An opposite entry is created in the period you choose; nothing is deleted or edited.',dFiApp:'Approve the suggestion',dFiAppB:'Your approval is the only way to apply a suggestion. Leave the amount empty to accept the suggested one.',dFiRej:'Reject the suggestion',dFiExp:'General expense',dFiExpB:'A general expense with a category and attachment. Store purchases arrive automatically from stock receipts and are not created here.',dFiRevE:'Reverse an expense',dFiRevEB:'A negative-amount entry is created and the original becomes reversed.',
nFiSal:'Every read of financial data is recorded in the audit log in your name, without values.',nFiSug:'Suggestions are generated from attendance and affect no payroll run before you approve. An attendance correction after month close generates a settlement in the next month.',nFiExp:'Store purchases arrive automatically with the source “store purchase”. The cost is entered on stock receipt and appears only here.',nFiReq:'Only accountant requests that reached your step. Approve or reject through the normal requests route.',nFiCur:'The default currency in this demo is IQD. Country and currency are not decided yet.',
emFi:'No data',emFiH:'No versions',emFiT:'No events',
tFiUpd:'New version saved (mock)',tFiAdjT:'Entry recorded (mock)',tFiRevT:'Reversal recorded (mock)',tFiAppT:'Suggestion applied (mock)',tFiRejT:'Suggestion rejected (mock)',tFiExpT:'Expense recorded (mock)',tFiDlT:'Download link prepared (mock)',tFiXp:'File prepared (mock)',tFiSet:'Payroll approver saved (mock)',
finT:'Finance settings',finSub:'Who approves the monthly payroll',finNote:'This screen contains no financial data, and a change does not affect previously approved runs. It grants no right to handle complaints.',
er2:{'Hr:Finance:EffectiveDateNotAfterLatest':'The effective date must be after the latest version','Hr:Finance:EffectiveDateInLockedPeriod':'The effective period has an approved or closed run','Hr:Finance:DuplicateComponentName':'Duplicate component name','Hr:Finance:SelfChangeNotAllowed':'Nobody changes their own data; it goes to the executive director','Hr:Finance:EmployeeNotFound':'The employee was not found','Hr:Finance:SalaryProfileMissing':'The employee has no financial profile in effect','Hr:Finance:PeriodLocked':'The period has an approved or closed run, use the next period','Hr:Finance:AttachmentRejected':'The attachment was rejected after scanning','Hr:Finance:RequestNotAtAccountantStep':'The request is not at the accountant step','Hr:Finance:AdjustmentAlreadyReversed':'The entry was already reversed','Hr:Finance:SuggestionAlreadyReviewed':'The suggestion was already reviewed','Hr:Finance:SuggestionAmountRequired':'Enter an amount: there is no suggested amount','Hr:Finance:ExpenseAlreadyReversed':'The expense was already reversed','Hr:Finance:NotFound':'Not found'}}};
Object.keys(T18).forEach(l=>{Object.assign(T4[l].er2,T18[l].er2);const o=Object.assign({},T18[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T18 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6');PAGES.push('fin');T.ar.nav.push('المالية');T.en.nav.push('Finance');
const OPR16={"PayrollRun_List": "ahx", "PayrollRun_Create": "a", "PayrollRun_Get": "ahx", "PayrollRun_Recalculate": "a", "PayrollRun_RequestApproval": "a", "PayrollRun_Approve": "hx", "PayrollRun_Close": "a", "PayrollRun_Export": "ahx", "Payslip_GetMyList": "e", "Payslip_GetMy": "e", "Payslip_DownloadMyPdf": "e", "FinanceReport_GetMonthlyStatement": "a", "FinanceReport_GetAdjustments": "a", "FinanceReport_GetStoreExpenses": "a", "FinanceReport_Export": "a", "PayrollRun_ReturnForChanges": "hx"};
NAVR.fin="ahx";NAVR.pay="edhxvasm";
const T19={ar:{
tFiRun:'دورات الرواتب',tFiRep:'تقارير مالية',payT:'قسيمة الراتب',paySub:'قسائم الكشوف المغلقة فقط. كل اطلاع يُسجَّل في سجل التدقيق',
pys:{draft:'مسودة',approved:'معتمد',closed:'مغلق'},pyl:{baseSalary:'الراتب الأساسي',allowance:'بدل',fixedDeduction:'استقطاع ثابت',addition:'إضافة',deduction:'استقطاع'},pyr:{monthlyStatement:'الكشف الشهري',adjustments:'الإضافات والاستقطاعات',storeExpenses:'مشتريات المخزن'},
pycPer:'الفترة',pycSt:'الحالة',pycPrep:'أعدّه',pycReq:'طُلب الاعتماد',pycApp:'اعتمده',pycCl:'أُغلق',pycCalc:'آخر احتساب',pycStale:'قديم',pycCnt:'عدد الموظفين',pycNet:'الصافي',pycGross:'الإجمالي',pycAdd:'الإضافات',pycDed:'الاستقطاعات',pycCur:'العملة',pycEmp:'الموظف',pycLine:'البند',pycKind:'النوع',pycAmt:'المبلغ',pycNoProf:'بلا بيانات مالية',pycPend:'اقتراحات غير مراجعة',pycRep:'التقرير',pycFrom:'من فترة',pycTo:'إلى فترة',pycDF:'من تاريخ',pycDT:'إلى تاريخ',pycTot:'المجاميع',pycTyp:'النوع',pycClosed:'تاريخ الإغلاق',pycYes:'نعم',
aPyNew:'كشف جديد',aPyOpen:'فتح',aPyRec:'إعادة الاحتساب',aPyReq:'طلب الاعتماد',aPyApp:'اعتماد الكشف',aPyRet:'إعادة للمحاسب',aPyCl:'إغلاق الكشف',aPyPdf:'تصدير PDF',aPyXls:'تصدير Excel',aPyLines:'البنود',aPyDl:'تنزيل PDF',aPyRun:'عرض التقرير',
fPyPer:'الفترة (سنة-شهر)',fPyNote:'ملاحظة (اختياري)',fPyReason:'السبب',
dPyNew:'كشف شهري جديد',dPyNewB:'مسودة تُحسب من البيانات المالية السارية في الفترة ومن الحركات المفتوحة فقط. الاقتراحات غير المراجعة لا تدخل. كشف واحد لكل فترة.',dPyRec:'إعادة الاحتساب',dPyRecB:'تُحسب المسودة من جديد، وتُلغى أي طلب اعتماد سابق.',dPyReq:'طلب الاعتماد',dPyReqB:'يصل الطلب إلى معتمد الكشف حسب الإعدادات، وتبقى الحالة مسودة.',dPyApp:'اعتماد الكشف',dPyAppB:'تثبَّت حركات الفترة ولا تُقبل حركات جديدة لها. لا يعتمد المستخدم كشفاً أعدّه بنفسه.',dPyRet:'إعادة الكشف للمحاسب',dPyRetB:'يعود الكشف إلى المحاسب مع السبب ويبقى مسودة.',dPyCl:'إغلاق الكشف',dPyClB:'نهائي: تصبح القسائم متاحة لكل موظف في الكشف، ويصله إشعار بلا مبالغ. أي أثر مالي لاحق يظهر تسوية في الشهر التالي.',dPyLines:'بنود القسيمة',
nPyRun:'كشف واحد لكل فترة: مسودة ثم اعتماد ثم إغلاق. يعتمده مدير الموارد البشرية أو المدير التنفيذي حسب إعدادات المالية.',nPyNot:'الكشف الشهري لا يخص دورك حسب إعدادات المعتمد.',nPyStale:'الكشف قديم: تغيرت بيانات مالية أو حركات بعد احتسابه. أعد الاحتساب قبل طلب الاعتماد.',nPyRet:'عند الإعادة للمحاسب يبقى الكشف مسودة. العقد لا يتيح للمحاسب رؤية سبب الإعادة بعد.',nPyRep:'المجاميع تشمل كل النتائج لا الصفحة الحالية.',nPyPick:'أدخل الفلاتر المطلوبة لعرض التقرير.',nPyPay:'يراها الموظف بعد الإغلاق فقط. مدير القسم يرى قسيمته من هنا أيضاً.',
emPy:'لا توجد كشوف',emPyI:'لا توجد بنود',emPyS:'لا توجد قسائم',
tPyNew:'أُنشئ الكشف (تجريبي)',tPyRec:'أُعيد الاحتساب (تجريبي)',tPyReq:'طُلب الاعتماد (تجريبي)',tPyApp:'اعتُمد الكشف (تجريبي)',tPyRet:'أُعيد الكشف للمحاسب (تجريبي)',tPyCl:'أُغلق الكشف وصارت القسائم متاحة (تجريبي)',tPyXp:'جُهّز الملف (تجريبي)',
er2:{'Hr:Finance:InvalidPayrollRunStatus':'حالة الكشف لا تسمح بهذه الخطوة','Hr:Finance:PayrollRunStale':'الكشف قديم، أعد الاحتساب أولاً','Hr:Finance:PayrollApproverNotConfigured':'لم يُضبط معتمد الكشف','Hr:Finance:ApprovalNotRequested':'لم يُطلب الاعتماد بعد','Hr:Finance:NotConfiguredPayrollApprover':'دورك ليس معتمد الكشف حسب الإعدادات','Hr:Finance:ApproverIsPreparer':'لا يعتمد المستخدم كشفاً أعدّه بنفسه','Hr:Finance:PayrollRunNotFoundForPeriod':'لا يوجد كشف لهذه الفترة','Hr:Finance:ReportFilterRequired':'فلاتر التقرير المطلوبة ناقصة'}},
en:{
tFiRun:'Payroll runs',tFiRep:'Finance reports',payT:'Payslip',paySub:'Payslips of closed runs only. Every read is recorded in the audit log',
pys:{draft:'Draft',approved:'Approved',closed:'Closed'},pyl:{baseSalary:'Base salary',allowance:'Allowance',fixedDeduction:'Fixed deduction',addition:'Addition',deduction:'Deduction'},pyr:{monthlyStatement:'Monthly statement',adjustments:'Additions and deductions',storeExpenses:'Store purchases'},
pycPer:'Period',pycSt:'Status',pycPrep:'Prepared by',pycReq:'Approval requested',pycApp:'Approved by',pycCl:'Closed',pycCalc:'Last calculated',pycStale:'Stale',pycCnt:'Employees',pycNet:'Net',pycGross:'Gross',pycAdd:'Additions',pycDed:'Deductions',pycCur:'Currency',pycEmp:'Employee',pycLine:'Line',pycKind:'Kind',pycAmt:'Amount',pycNoProf:'Without a financial profile',pycPend:'Unreviewed suggestions',pycRep:'Report',pycFrom:'From period',pycTo:'To period',pycDF:'From date',pycDT:'To date',pycTot:'Totals',pycTyp:'Type',pycClosed:'Closed on',pycYes:'Yes',
aPyNew:'New run',aPyOpen:'Open',aPyRec:'Recalculate',aPyReq:'Request approval',aPyApp:'Approve the run',aPyRet:'Return to the accountant',aPyCl:'Close the run',aPyPdf:'Export PDF',aPyXls:'Export Excel',aPyLines:'Lines',aPyDl:'Download PDF',aPyRun:'Show report',
fPyPer:'Period (year-month)',fPyNote:'Note (optional)',fPyReason:'Reason',
dPyNew:'New monthly run',dPyNewB:'A draft calculated from the financial profiles in effect for the period and open entries only. Unreviewed suggestions are not included. One run per period.',dPyRec:'Recalculate',dPyRecB:'The draft is calculated again and any earlier approval request is cancelled.',dPyReq:'Request approval',dPyReqB:'The request goes to the payroll approver set in the settings; the status stays draft.',dPyApp:'Approve the run',dPyAppB:'The period’s entries are fixed and no new entries are accepted for it. Nobody approves a run they prepared.',dPyRet:'Return the run to the accountant',dPyRetB:'The run goes back to the accountant with the reason and stays a draft.',dPyCl:'Close the run',dPyClB:'Final: payslips become available to every employee in the run, with a notification that carries no amounts. Any later financial effect appears as a settlement in the next month.',dPyLines:'Payslip lines',
nPyRun:'One run per period: draft, approval, then close. The HR manager or the executive director approves it according to the finance settings.',nPyNot:'The monthly run does not concern your role according to the approver setting.',nPyStale:'The run is stale: financial data or entries changed after it was calculated. Recalculate before requesting approval.',nPyRet:'A returned run stays a draft. The contract does not yet let the accountant see the return reason.',nPyRep:'Totals include all results, not the current page.',nPyPick:'Enter the required filters to show the report.',nPyPay:'The employee sees it after closing only. A department manager sees their own payslip here too.',
emPy:'No runs',emPyI:'No lines',emPyS:'No payslips',
tPyNew:'Run created (mock)',tPyRec:'Recalculated (mock)',tPyReq:'Approval requested (mock)',tPyApp:'Run approved (mock)',tPyRet:'Run returned to the accountant (mock)',tPyCl:'Run closed; payslips are now available (mock)',tPyXp:'File prepared (mock)',
er2:{'Hr:Finance:InvalidPayrollRunStatus':'The run status does not allow this step','Hr:Finance:PayrollRunStale':'The run is stale, recalculate first','Hr:Finance:PayrollApproverNotConfigured':'The payroll approver is not set','Hr:Finance:ApprovalNotRequested':'Approval was not requested yet','Hr:Finance:NotConfiguredPayrollApprover':'Your role is not the payroll approver per the settings','Hr:Finance:ApproverIsPreparer':'Nobody approves a run they prepared','Hr:Finance:PayrollRunNotFoundForPeriod':'There is no run for this period','Hr:Finance:ReportFilterRequired':'Required report filters are missing'}}};
Object.keys(T19).forEach(l=>{Object.assign(T4[l].er2,T19[l].er2);const o=Object.assign({},T19[l]);delete o.er2;Object.keys(o).forEach(k=>{if(T4[l][k]!==undefined)throw new Error('T19 key clash '+k)});Object.assign(T4[l],o)});
IC.push('M3 10h18M5 6l7-3 7 3M6 10v8M10 10v8M14 10v8M18 10v8M4 20h16');T.ar.nav[PAGES.indexOf('pay')]='قسيمة الراتب';T.en.nav[PAGES.indexOf('pay')]='Payslip';
class Component extends DCLogic{
state={dsx:{show:true,tab:0,sw1:true,sw2:false,ck1:true,ck2:false,rd:'b',sel:'0',q:'',date:'',txv:'',prog:45,vp:'d',sortAsc:true,mini:false,dd:false,pop:false,modal:false,conf:false,dr:false,file:null,fp:0},sub:'list',sid:null,tab:'data',fv:{},fe:{},data:{},ldg:false,derr:'',denied:false,dlg:null,derr2:'',step:0,flt:{},skip:{},dr:false,scr:'signin',persona:'emp',me:null,inb:null,emp:null,mfa:null,log:null,busy:false,aerr:'',vol:false,pw1:'',pw2:'',pw3:'',mfaKey:null,code:'',codes:null,saved:false,tlFrom:'',tlTo:'',tlType:'',tlSkip:0,th:'dark',thm:false,leaving:false,page:'dash',lang:'ar',col:false,q:'',df:'all',asc:true,range:'w',modal:false,saving:0,err:false,bell:false,fn:'',ft:'',fd:'eng',toasts:[],
emps:[{id:'E-1042',n:['سارة الحسني','Sara Al-Hasani'],d:'eng',t:'dev',s:'active'},{id:'E-1043',n:['عمر خالد','Omar Khalid'],d:'sales',t:'rep',s:'leave'},{id:'E-1044',n:['زينب الجبوري','Zaynab Al-Jubouri'],d:'hr',t:'spec',s:'active'},{id:'E-1045',n:['حسن علي','Hassan Ali'],d:'ops',t:'mgr',s:'active'},{id:'E-1046',n:['نور التميمي','Noor Al-Tamimi'],d:'eng',t:'dev',s:'active'},{id:'E-1047',n:['كريم صالح','Karim Saleh'],d:'fin',t:'acc',s:'inactive'},{id:'E-1048',n:['منى الراوي','Muna Al-Rawi'],d:'hr',t:'mgr',s:'active'},{id:'E-1049',n:['يوسف النجار','Yousif Al-Najjar'],d:'ops',t:'spec',s:'leave'}],
lv:[{id:1,n:['سارة الحسني','Sara Al-Hasani'],t:'ann',dy:3,s:'p'},{id:2,n:['عمر خالد','Omar Khalid'],t:'sick',dy:2,s:'p'},{id:3,n:['يوسف النجار','Yousif Al-Najjar'],t:'ann',dy:5,s:'a'}]};
go=(p)=>{const R0={sub:'list',sid:null,tab:'data',data:{},fv:{},fe:{},dlg:null,step:0,skip:{},derr:'',denied:false};if(p===this.state.page){this.setState({bell:false,dr:false,...R0});this.later();return}this.setState({leaving:true,bell:false,dr:false});setTimeout(()=>{this.setState({page:p,leaving:false,...R0});this.enter()},240)};
toast=(m,k)=>{const id=Date.now()+Math.random();this.setState(s=>({toasts:[...s.toasts,{id,m,k:k||'ok'}]}));setTimeout(()=>this.setState(s=>({toasts:s.toasts.filter(x=>x.id!==id)})),3000)};
srv=null;
UID=(n)=>'00000000-0000-4000-8000-'+('000000000000'+n).slice(-12);
gateOf=(P,v)=>v.must?'passwordChangeRequired':(P.mfaR&&!v.mfaE?'mfaEnrollmentRequired':'none');
mkCodes=()=>Array.from({length:10},()=>Math.random().toString(36).slice(2,7)+'-'+Math.random().toString(36).slice(2,7));
callRaw=(op,x)=>new Promise((ok,no)=>setTimeout(()=>{
 /* MOCK ADAPTER: mirrors openapi.yaml operation ids, DTO shapes, access gates and error codes. No backend. */
 const v=this.srv,ix=this.state.lang==='ar'?0:1,P=v?PS[v.p]:null;
 const R=(status,code)=>{throw {status,code}};
 try{
  if(op==='ConnectToken_Issue'){ok({access_token:'mock-access-token',token_type:'Bearer',expires_in:3600});return}
  const gate=this.gateOf(P,v);
  const allow={passwordChangeRequired:['ApplicationConfiguration_Get','Me_Get','Account_ChangePassword'],mfaEnrollmentRequired:['ApplicationConfiguration_Get','Me_Get','Account_ChangePassword','Mfa_GetStatus','Mfa_StartEnrollment','Mfa_ConfirmEnrollment']};
  if(gate!=='none'&&!allow[gate].includes(op)) R(403,gate==='passwordChangeRequired'?'Hr:Identity:PasswordChangeRequired':'Hr:Identity:MfaEnrollmentRequired');
  const me=()=>({userId:this.UID(P.id),userName:P.u,displayName:P.dn[ix],email:P.u+'@example.test',preferredLanguage:v.lang,roleNames:P.roles,employee:{id:this.UID(this.EID[v.p]),fullName:P.dn[ix],departmentName:P.dept[ix]},directReportCount:this.db?this.db.emps.filter(e=>e.mgr===this.UID(this.EID[v.p])&&e.status==='active').length:P.dr,accessGate:gate,mustChangePassword:v.must,mfaRequired:P.mfaR,mfaEnabled:v.mfaE});
  if(op==='ApplicationConfiguration_Get'){ok({localization:{currentCulture:{cultureName:ix?'en':'ar',name:ix?'en':'ar',displayName:ix?'English':'العربية',twoLetterIsoLanguageName:ix?'en':'ar',isRightToLeft:!ix},defaultResourceName:null,languages:[{cultureName:'ar',uiCultureName:'ar',displayName:'العربية'},{cultureName:'en',uiCultureName:'en',displayName:'English'}],values:{}},auth:{grantedPolicies:{}},setting:{values:{}},currentUser:{isAuthenticated:true,id:this.UID(P.id),userName:P.u,roles:P.roles},extraProperties:{accessGate:gate,mustChangePassword:v.must,mfaRequired:P.mfaR,mfaEnabled:v.mfaE}});return}
  if(op==='Me_Get'){ok(me());return}
  if(op==='Me_Update'){v.lang=x.preferredLanguage;ok(me());return}
  if(op==='Me_GetEmployeeProfile'){ok({id:this.UID(this.EID[v.p]),fullName:P.dn[ix],fullNameAr:P.dn[0],fullNameEn:P.dn[1],email:P.u+'@example.test',phoneNumber:null,dateOfBirth:null,address:null,department:{id:this.UID('9'),name:P.dept[ix]},jobTitle:P.jt[ix],hireDate:P.hire,status:'active',manager:P.mg?{id:this.UID('8'),fullName:P.mg[ix],departmentName:null}:null,currentShift:{workShiftId:this.UID('7'),name:ix?'Fixed day shift':'دوام ثابت صباحي',effectiveFrom:P.hire},currentContract:{contractId:this.UID('6'),startDate:P.hire,endDate:null},roleNames:P.roles,gender:P.g,employeeNumber:P.num});return}
  if(op==='Me_GetInboxSummary'){const cnt=(o,q)=>{try{return this.route5(o,q,P,v,ix,R)}catch(er){return null}};const ri=cnt('Request_GetInbox',{}),mi=cnt('Memo_GetInbox',{}),ii=cnt('Incident_GetInbox',{});
   const items=[...(ri&&ri.totalCount?[{category:'request',count:ri.totalCount,delegatedCount:ri.items.filter(z=>z.delegatedFrom).length}]:[]),...(mi&&mi.totalCount?[{category:'memo',count:mi.items.filter(z=>['pendingManager','pendingHr','pendingExecutive'].includes(z.status)).length,delegatedCount:0}]:[]),...(ii&&ii.items.filter(z=>['pendingManager','pendingHr','inProgress'].includes(z.status)).length?[{category:'incident',count:ii.items.filter(z=>['pendingManager','pendingHr','inProgress'].includes(z.status)).length,delegatedCount:0}]:[]),...P.inb.filter(a=>!['request','memo','incident'].includes(a[0])).map(a=>({category:a[0],count:a[1],delegatedCount:a[2]}))].filter(z=>z.count>0);
   ok({totalCount:items.reduce((n,a)=>n+a.count,0),items});return}
  if(op==='Me_GetAdministrativeTaskLog'){
   if(!P.roles.includes('departmentManager')) R(403,'forbidden');
   let all=Array.from({length:23},(_,n)=>{const p=TLP[n%6],d=new Date(Date.UTC(2026,10,4-Math.floor(n/2),9-(n%3),15));return{occurredAt:d.toISOString(),taskType:p[0],title:p[1+ix],referenceId:null}});
   if(x.fromDate)all=all.filter(r=>r.occurredAt.slice(0,10)>=x.fromDate);
   if(x.toDate)all=all.filter(r=>r.occurredAt.slice(0,10)<=x.toDate);
   if(x.taskType)all=all.filter(r=>r.taskType===x.taskType);
   ok({totalCount:all.length,items:all.slice(x.skipCount||0,(x.skipCount||0)+(x.maxResultCount||20))});return}
  if(op==='Account_ChangePassword'){
   if(x.currentPassword!==v.pw) R(422,'Hr:Identity:CurrentPasswordIncorrect');
   if(x.newPassword===x.currentPassword) R(422,'Hr:Identity:NewPasswordSameAsCurrent');
   if(x.newPassword.length<8||!/[A-Za-z]/.test(x.newPassword)||!/[0-9]/.test(x.newPassword)) R(422,'Hr:Identity:PasswordPolicyViolation'); /* mock policy: real policy is server-defined */
   v.pw=x.newPassword;v.must=false;ok({accessGate:this.gateOf(P,v),changedAt:new Date().toISOString()});return}
  if(op.indexOf('Mfa_')===0&&!P.mfaR) R(403,'forbidden');
  if(op==='Mfa_GetStatus'){ok({isRequired:P.mfaR,isEnabled:v.mfaE,enabledAt:v.mfaAt||null,recoveryCodesRemaining:v.rem});return}
  if(op==='Mfa_StartEnrollment'){if(v.mfaE) R(422,'Hr:Identity:MfaAlreadyEnabled');v.pending=true;ok({sharedKey:'JBSWY3DPEHPK3PXP',authenticatorUri:'otpauth://totp/HR:'+P.u+'?secret=JBSWY3DPEHPK3PXP&issuer=HR'});return}
  if(op==='Mfa_ConfirmEnrollment'){
   if(!v.pending) R(422,'Hr:Identity:MfaEnrollmentNotStarted');if(v.mfaE) R(422,'Hr:Identity:MfaAlreadyEnabled');
   if(x.code!=='123456') R(422,'Hr:Identity:MfaCodeInvalid');
   v.mfaE=true;v.pending=false;v.mfaAt=new Date().toISOString();v.rem=10;ok({recoveryCodes:this.mkCodes(),accessGate:this.gateOf(P,v)});return}
  if(op==='Mfa_RegenerateRecoveryCodes'){if(!v.mfaE) R(422,'Hr:Identity:MfaNotEnabled');if(x.code!=='123456') R(422,'Hr:Identity:MfaCodeInvalid');v.rem=10;ok({recoveryCodes:this.mkCodes(),accessGate:'none'});return}
  let r2=this.route2(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route3(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route4(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route5(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route6(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route7(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route8(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route9(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route10(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route11(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route12(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route13(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route14(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route15(op,x||{},P,v,ix,R);if(r2===undefined)r2=this.route16(op,x||{},P,v,ix,R);if(r2!==undefined){ok(r2);return}R(404,'unknown-operation');
 }catch(e){no(e)}},420));
fail=(e)=>{
 if(e&&e.status===403&&/PasswordChangeRequired|MfaEnrollmentRequired/.test(e.code||'')){this.boot();return}
 this.setState({busy:false,aerr:(e&&e.code)||'gen'})};
boot=async()=>{
 try{await this.call('ApplicationConfiguration_Get');this.bellSoon();const me=await this.call('Me_Get');
  const g=me.accessGate;this.setState({me,busy:false,aerr:'',vol:false,scr:g==='passwordChangeRequired'?'pwd':g==='mfaEnrollmentRequired'?'mfa':'app'});
  if(g==='none')this.loadHome(me)}catch(e){this.fail(e)}};
loadHome=async(me0)=>{
 const me=me0||this.state.me;
 try{const [inb,emp]=await Promise.all([this.call('Me_GetInboxSummary'),this.call('Me_GetEmployeeProfile')]);this.setState({inb,emp});
  if(me.mfaRequired){const mfa=await this.call('Mfa_GetStatus');this.setState({mfa})}
  if(me.roleNames.includes('departmentManager'))this.loadLog(0)}catch(e){this.fail(e)}};
loadLog=async(skip,over)=>{
 const s={...this.state,...(over||{})};
 try{const log=await this.call('Me_GetAdministrativeTaskLog',{fromDate:s.tlFrom||undefined,toDate:s.tlTo||undefined,taskType:s.tlType||undefined,skipCount:skip,maxResultCount:10});this.setState({log,tlSkip:skip})}catch(e){this.fail(e)}};
doSignin=async()=>{
 if(this.state.busy)return;const k=this.state.persona,P=PS[k];
 this.srv={p:k,lang:this.state.lang,must:P.must,mfaE:P.mfaE,pending:false,pw:'Temp#2026',rem:P.mfaE?10:0,mfaAt:P.mfaE?'2026-09-01T08:00:00Z':null};
 this.setState({busy:true,aerr:''});
 try{await this.call('ConnectToken_Issue',{grant_type:'authorization_code'});this.boot()}catch(e){this.fail(e)}};
submitPw=async()=>{
 const s=this.state;if(s.busy)return;
 if(!s.pw1||!s.pw2||!s.pw3){this.setState({aerr:'pwRq'});return}
 if(s.pw2!==s.pw3){this.setState({aerr:'pwMis'});return}
 this.setState({busy:true,aerr:''});
 try{await this.call('Account_ChangePassword',{currentPassword:s.pw1,newPassword:s.pw2});this.setState({pw1:'',pw2:'',pw3:''});this.toast(T2[s.lang].pwOk);this.boot()}catch(e){this.fail(e)}};
startMfa=async()=>{
 if(this.state.busy)return;this.setState({busy:true,aerr:''});
 try{const k=await this.call('Mfa_StartEnrollment');this.setState({mfaKey:k,busy:false})}catch(e){this.fail(e)}};
submitMfa=async()=>{
 const s=this.state;if(s.busy)return;
 if(!/^[0-9]{6}$/.test(s.code)){this.setState({aerr:'mfaBad'});return}
 this.setState({busy:true,aerr:''});
 try{const r=await this.call('Mfa_ConfirmEnrollment',{code:s.code});this.setState({codes:r.recoveryCodes,scr:'codes',busy:false,code:'',mfaKey:null,saved:false})}catch(e){this.fail(e)}};
submitRc=async()=>{
 const s=this.state;if(s.busy)return;
 if(!/^[0-9]{6}$/.test(s.code)){this.setState({aerr:'mfaBad'});return}
 this.setState({busy:true,aerr:''});
 try{const r=await this.call('Mfa_RegenerateRecoveryCodes',{code:s.code});this.setState({codes:r.recoveryCodes,scr:'codes',busy:false,code:'',saved:false})}catch(e){this.fail(e)}};
setLang=(l)=>{
 this.setState({lang:l});
 if(this.srv&&this.state.me){this.srv.lang=l;this.call('Me_Update',{preferredLanguage:l}).then(me=>{this.setState({me});this.loadHome(me);this.enter()}).catch(this.fail)}};
resetMock=()=>{this.db=null;this.d3=null;this.d4=null;this.d5=null;this.d6=null;this.d8=null;this.d9=null;this.d10=null;this.d11=null;this.d12=null;this.d13=null;this.d14=null;this.d15=null;this.d16=null;this.toast(T2[this.state.lang].rsD,'in')};
signOut=()=>{try{this.dtUnreg()}catch(e){}this.srv=null;this.setState({scr:'signin',me:null,inb:null,emp:null,mfa:null,log:null,page:'dash',aerr:'',busy:false,vol:false})};
EID={emp:'11',mgr:'12',hr:'13',acc:'14',nw:'15',adm:'16',xd:'17',rv:'22',so:'21'};
db=null;
mkDb=()=>{
 const U=this.UID;
 const D=[['51','الهندسة','Engineering'],['52','العمليات','Operations'],['53','المبيعات','Sales'],['54','الموارد البشرية','Human Resources'],['55','المالية','Finance']].map(d=>({id:U(d[0]),nameAr:d[1],nameEn:d[2],created:'2024-01-01T08:00:00Z',stamp:'a'}));
 const dp=n=>D[n-1].id;
 const acct=(u,o)=>({u,active:true,must:false,mfaE:false,locked:false,created:'2024-02-01T08:00:00Z',...o});
 const E=(n,ar,en,d,ja,je,hire,mgr,roles,g,num,u,o)=>({id:U(n),ar,en,dept:dp(d),jt:[ja,je],hire,status:'active',mgr:mgr?U(mgr):null,roles,g,num,email:u?u+'@example.test':null,acc:u?acct(u,o):null,stamp:'a',created:hire+'T08:00:00Z'});
 const emps=[
 E('11','سارة الحسني','Sara Al-Hasani',1,'مهندسة برمجيات','Software Engineer','2024-03-15','12',['employee'],'female','E-1042','s.hasani'),
 E('12','حسن علي','Hassan Ali',2,'مدير','Manager','2022-09-01','16',['departmentManager'],'male','E-1045','h.ali'),
 E('13','منى الراوي','Muna Al-Rawi',4,'مدير الموارد البشرية','HR Manager','2021-01-10','17',['hrManager'],'female','E-1048','m.rawi',{mfaE:true}),
 E('14','كريم صالح','Karim Saleh',5,'محاسب','Accountant','2023-06-01','12',['accountant'],'male','E-1047','k.saleh'),
 E('15','نور التميمي','Noor Al-Tamimi',1,'مهندسة برمجيات','Software Engineer','2026-11-01','12',['employee'],'female','E-1046','n.tamimi',{must:true}),
 E('16','ناصر الكعبي','Nasser Al-Kaabi',2,'مسؤول النظام','System Admin','2020-05-01','17',['systemAdmin'],'male','E-1001','n.kaabi',{mfaE:true}),
 E('17','ليث العبيدي','Layth Al-Obaidi',2,'المدير التنفيذي','Executive Director','2019-01-01',null,['executiveDirector'],'male','E-1000','l.obaidi',{mfaE:true}),
 E('18','عمر خالد','Omar Khalid',3,'مندوب مبيعات','Sales Rep','2023-02-20','12',['employee'],'male','E-1043','o.khalid'),
 E('19','زينب الجبوري','Zaynab Al-Jubouri',4,'أخصائية','Specialist','2024-07-01','12',['employee'],'female','E-1044','z.jubouri'),
 E('20','يوسف النجار','Yousif Al-Najjar',2,'أخصائي','Specialist','2024-09-10','12',['employee'],'male','E-1049','y.najjar'),
 E('21','أحمد الزبيدي','Ahmed Al-Zubaidi',2,'مسؤول مخزن','Store Officer','2023-10-01','12',['storeOfficer'],'male','E-1050','a.zubaidi'),
 E('22','رغد الحلي','Raghad Al-Hilli',5,'مراجعة','Reviewer','2022-04-01','12',['reviewer'],'female','E-1051','r.hilli',{mfaE:false}),
 E('23','سلمى الدليمي','Salma Al-Dulaimi',3,'مديرة المبيعات','Sales Manager','2022-01-15','16',['departmentManager'],'female','E-1052','s.dulaimi'),
 E('24','هبة السعدي','Hiba Al-Saadi',3,'مندوبة مبيعات','Sales Rep','2025-01-05','23',['employee'],'female','E-1053',null),
 E('25','علي جاسم','Ali Jasim',1,'مهندس','Engineer','2021-03-01','12',['employee'],'male','E-1054',null)];
 emps[14].status='ended';
 const mfaR=['hrManager','executiveDirector','accountant','systemAdmin'];
 emps.forEach(e=>{if(e.acc)e.acc.mfaR=e.roles.some(r=>mfaR.includes(r))});
 const rn=['employee','departmentManager','hrManager','executiveDirector','reviewer','accountant','storeOfficer','systemAdmin'];
 const roles=rn.map((n,i)=>({id:U('6'+(i+1)),name:n,nameAr:T2.ar.rl[n],nameEn:T2.en.rl[n],isStatic:true,stamp:'a'}));
 const hist={};emps.forEach(e=>{hist[e.id]=e.mgr?[{id:U('8'+e.id.slice(-2)),mgr:e.mgr,from:e.hire,to:null,by:'—',reason:null}]:[]});
 const docs=[{id:U('71'),emp:U('11'),type:'contract',title:'عقد العمل|Employment contract',file:'contract.pdf',ct:'application/pdf',size:218000,sens:'normal',scan:'clean',at:'2024-03-15T08:00:00Z'},{id:U('72'),emp:U('11'),type:'identity',title:'الهوية|ID card',file:'id.jpg',ct:'image/jpeg',size:412000,sens:'sensitive',scan:'clean',at:'2024-03-15T08:05:00Z'},{id:U('73'),emp:U('12'),type:'certificate',title:'شهادة|Certificate',file:'cert.pdf',ct:'application/pdf',size:96000,sens:'normal',scan:'clean',at:'2022-09-02T08:00:00Z'}];
 this.db={D,emps,roles,hist,docs,grants:{},seq:100};
};
nid=()=>this.UID(String(++this.db.seq));
has=(roles,r)=>roles.includes(r);
route2=(op,x,P,v,ix,R)=>{
 if(!(op in OPR)) return undefined;
 if(!this.db) this.mkDb();
 const db=this.db,U=this.UID,meId=U(this.EID[v.p]),AB={};Object.keys(RAB).forEach(k=>AB[RAB[k]]=k);
 if(!P.roles.some(r=>OPR[op].includes(AB[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,dnm=id=>{const d=db.D.find(z=>z.id===id);return d?(ix?d.nameEn:d.nameAr):'—'};
 const ref=id=>{const e=E(id);return e?{id:e.id,fullName:nmo(e),departmentName:dnm(e.dept)}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const sr=(a,q,f)=>q.search?a.filter(z=>f(z).toLowerCase().includes(String(q.search).toLowerCase())):a;
 const stamp=()=>Math.random().toString(36).slice(2,8),today=()=>new Date().toISOString().slice(0,10);
 const need=(e)=>{if(!e)R(404,'Hr:Employee:EmployeeNotFound');return e};
 const chkS=(o,s)=>{if(o.stamp!==s)R(409,'conflict')};
 const drs=e=>db.emps.filter(z=>z.mgr===e.id&&z.status==='active').length;
 const sh={'41':[ix?'Fixed day shift':'دوام ثابت صباحي'],'42':[ix?'Rotating shift':'دوام متناوب']};
 const hr=e=>({id:e.id,fullName:nmo(e),fullNameAr:e.ar,fullNameEn:e.en,email:e.email,phoneNumber:e.ph||null,dateOfBirth:e.dob||null,address:e.addr||null,department:{id:e.dept,name:dnm(e.dept)},jobTitle:e.jt[ix],hireDate:e.hire,status:e.status,manager:ref(e.mgr),currentShift:{workShiftId:U('41'),name:sh['41'][0],effectiveFrom:e.hire},currentContract:{contractId:U('45'),startDate:e.hire,endDate:null},account:e.acc?{userId:e.id,userName:e.acc.u,isActive:e.acc.active,mustChangePassword:e.acc.must,mfaRequired:e.acc.mfaR,mfaEnabled:e.acc.mfaE,roleNames:e.roles}:null,directReportCount:drs(e),creationTime:e.created,lastModificationTime:null,concurrencyStamp:e.stamp,gender:e.g,employeeNumber:e.num});
 const ad=e=>({id:e.id,userName:e.acc.u,email:e.email,employee:ref(e.id),isActive:e.acc.active,isLockedOut:e.acc.locked,mustChangePassword:e.acc.must,mfaRequired:e.acc.mfaR,mfaEnabled:e.acc.mfaE,roleNames:e.roles,creationTime:e.acc.created,lastModificationTime:null,concurrencyStamp:e.stamp});
 const dd=d=>({id:d.id,name:ix?d.nameEn:d.nameAr,nameAr:d.nameAr,nameEn:d.nameEn,employeeCount:db.emps.filter(e=>e.dept===d.id).length,creationTime:d.created,lastModificationTime:null,concurrencyStamp:d.stamp});
 const rd=r=>({id:r.id,name:r.name,displayName:ix?r.nameEn:r.nameAr,nameAr:r.nameAr,nameEn:r.nameEn,isStatic:r.isStatic,userCount:db.emps.filter(e=>e.acc&&e.roles.includes(r.name)).length,concurrencyStamp:r.stamp});
 const mc=(e,roles,mid)=>{const h=r=>roles.includes(r);
  if(h('executiveDirector')){if(mid)return 'Hr:Employee:ExecutiveCannotHaveManager';return null}
  if(!mid)return 'Hr:Employee:ManagerRequired';const m=E(mid);if(!m)return 'Hr:Employee:ManagerNotFound';
  if(m.status!=='active')return 'Hr:Employee:ManagerNotActive';if(e&&m.id===e.id)return 'Hr:Employee:CannotManageSelf';
  const nd=(h('hrManager')||h('systemAdmin'))?'executiveDirector':h('departmentManager')?'systemAdmin':'departmentManager';
  if(!m.roles.includes(nd))return 'Hr:Employee:InvalidManagerForRole';
  let c=m;while(c&&e){if(c.id===e.id)return 'Hr:Employee:ManagerCycle';c=E(c.mgr)}return null};
 const roleChk=(roles,e,actorExec)=>{
  roles.forEach(r=>{if(!db.roles.some(z=>z.name===r))R(422,'Hr:Identity:RoleNotFound')});
  ['hrManager','executiveDirector'].forEach(r=>{if(roles.includes(r)){if(!(e&&e.roles.includes(r))&&!actorExec)R(422,'Hr:Identity:ProtectedRoleChangeRequiresApproval');
    if(db.emps.some(z=>z.status==='active'&&z.roles.includes(r)&&(!e||z.id!==e.id)))R(422,'Hr:Identity:SingletonRoleAlreadyAssigned')}});
  if(roles.includes('executiveDirector')&&roles.length>1)R(422,'Hr:Identity:ForbiddenRoleCombination')};
 const actorExec=P.roles.includes('executiveDirector'),me=E(meId);
 const grp=(grant,roleLetters,lock)=>{const g={};Object.keys(PERMS).forEach(n=>{const k=n.split('.').slice(0,2).join('.');(g[k]=g[k]||[]).push({name:n,displayName:n.split('.').slice(2).join('.')||n,parentName:null,isGranted:(grant[n]===undefined)?roleLetters.some(l=>PERMS[n].includes(l)):grant[n],isDirectlyGranted:grant[n]===true,grantedByRoleNames:[]})});
  return {groups:Object.keys(g).map(k=>({name:k,displayName:k,permissions:g[k]}))}};
 const setP=(key,ch,roles,self)=>{if(self)R(422,'Hr:Identity:CannotChangeOwnPermissions');db.grants[key]=db.grants[key]||{};
  ch.permissions.forEach(c=>{if(!(c.name in PERMS))R(422,'Hr:Identity:PermissionNotFound');if(c.name.indexOf('Hr.Complaint')===0)R(422,'Hr:Identity:ProtectedPermissionNotGrantable');
   if(c.name.indexOf('Hr.Finance')===0&&roles.some(r=>r==='departmentManager'||r==='storeOfficer'))R(422,'Hr:Identity:FinancialPermissionNotGrantable');db.grants[key][c.name]=c.isGranted})};
 const L=(r)=>r.map(z=>z).join('');
 const letters=roles=>roles.map(r=>AB[r]);
 switch(op){
 case 'Department_List':return pg(sr(db.D,x,d=>d.nameAr+d.nameEn),x).items&&{totalCount:sr(db.D,x,d=>d.nameAr+d.nameEn).length,items:pg(sr(db.D,x,d=>d.nameAr+d.nameEn),x).items.map(dd)};
 case 'Department_Lookup':return {totalCount:db.D.length,items:db.D.map(d=>({id:d.id,name:ix?d.nameEn:d.nameAr}))};
 case 'Department_Get':{const d=db.D.find(z=>z.id===x.id);if(!d)R(404,'Hr:Department:NotFound');return dd(d)}
 case 'Department_Create':{const d={id:this.nid(),nameAr:x.nameAr,nameEn:x.nameEn,created:new Date().toISOString(),stamp:stamp()};db.D.push(d);return dd(d)}
 case 'Department_Update':{const d=db.D.find(z=>z.id===x.id);if(!d)R(404,'Hr:Department:NotFound');chkS(d,x.concurrencyStamp);d.nameAr=x.nameAr;d.nameEn=x.nameEn;d.stamp=stamp();return dd(d)}
 case 'Department_Delete':{const d=db.D.find(z=>z.id===x.id);if(!d)R(404,'Hr:Department:NotFound');if(db.emps.some(e=>e.dept===d.id))R(422,'Hr:Department:HasEmployees');db.D=db.D.filter(z=>z!==d);return null}
 case 'Employee_List':{let a=sr(db.emps,x,e=>e.ar+e.en+e.num);if(x.departmentId)a=a.filter(e=>e.dept===x.departmentId);if(x.managerEmployeeId)a=a.filter(e=>e.mgr===x.managerEmployeeId);if(x.status)a=a.filter(e=>e.status===x.status);if(x.roleName)a=a.filter(e=>e.roles.includes(x.roleName));
  a=a.slice().sort((p,q)=>nmo(p).localeCompare(nmo(q)));const p=pg(a,x);return {totalCount:p.totalCount,items:p.items.map(e=>({id:e.id,fullName:nmo(e),department:{id:e.dept,name:dnm(e.dept)},jobTitle:e.jt[ix],hireDate:e.hire,status:e.status,manager:ref(e.mgr),roleNames:e.roles,hasAccount:!!e.acc}))}}
 case 'Employee_Lookup':{let a=sr(db.emps.filter(e=>e.status==='active'),x,e=>e.ar+e.en);if(x.departmentId)a=a.filter(e=>e.dept===x.departmentId);return {totalCount:a.length,items:a.map(e=>ref(e.id))}}
 case 'Employee_Get':return hr(need(E(x.id)));
 case 'Employee_Create':{
  if(!db.D.some(d=>d.id===x.departmentId))R(404,'Hr:Department:NotFound');roleChk(x.roleNames,null,actorExec);
  if(!x.roleNames.includes('executiveDirector')&&!x.workShiftId)R(422,'Hr:Employee:WorkShiftRequired');
  const c=mc(null,x.roleNames,x.managerEmployeeId||null);if(c)R(422,c);
  const e={id:this.nid(),ar:x.fullNameAr,en:x.fullNameEn||x.fullNameAr,dept:x.departmentId,jt:[x.jobTitle,x.jobTitle],hire:x.hireDate,status:'active',mgr:x.managerEmployeeId||null,roles:x.roleNames,g:x.gender||null,num:x.employeeNumber||null,email:x.email||null,ph:x.phoneNumber||null,dob:x.dateOfBirth||null,addr:x.address||null,stamp:stamp(),created:new Date().toISOString(),acc:{u:x.userName,active:true,must:true,mfaE:false,locked:false,created:new Date().toISOString(),mfaR:x.roleNames.some(r=>['hrManager','executiveDirector','accountant','systemAdmin'].includes(r))}};
  db.emps.push(e);db.hist[e.id]=e.mgr?[{id:this.nid(),mgr:e.mgr,from:e.hire,to:null,by:me?nmo(me):'—',reason:null}]:[];
  return {employee:hr(e),credentials:{userId:e.id,userName:e.acc.u,temporaryPassword:'Tmp-'+Math.random().toString(36).slice(2,8)+'9',mustChangePassword:true}}}
 case 'Employee_Update':{const e=need(E(x.id));chkS(e,x.concurrencyStamp);if(x.status==='ended'&&drs(e)>0)R(422,'Hr:Employee:HasDirectReports');
  e.ar=x.fullNameAr;e.en=x.fullNameEn||x.fullNameAr;e.email=x.email||null;e.dept=x.departmentId;e.jt=[x.jobTitle,x.jobTitle];e.hire=x.hireDate;e.status=x.status;e.g=x.gender||null;e.num=x.employeeNumber||null;e.ph=x.phoneNumber||null;e.dob=x.dateOfBirth||null;e.addr=x.address||null;e.stamp=stamp();return hr(e)}
 case 'Employee_ChangeManager':{const e=need(E(x.id));chkS(e,x.concurrencyStamp);if(e.status==='ended')R(422,'Hr:Employee:EmployeeEnded');if(e.mgr===x.managerEmployeeId)R(422,'Hr:Employee:ManagerUnchanged');
  const c=mc(e,e.roles,x.managerEmployeeId);if(c)R(422,c);const prev=e.mgr;(db.hist[e.id]||[]).forEach(h=>{if(!h.to)h.to=today()});(db.hist[e.id]=db.hist[e.id]||[]).push({id:this.nid(),mgr:x.managerEmployeeId,from:today(),to:null,by:me?nmo(me):'—',reason:x.reason||null});e.mgr=x.managerEmployeeId;e.stamp=stamp();
  return {employee:ref(e.id),previousManager:ref(prev),newManager:ref(e.mgr),effectiveFrom:today(),movedPendingItemCount:0,concurrencyStamp:e.stamp}}
 case 'Employee_GetManagerHistory':{const h=(db.hist[x.id]||[]).slice().reverse();return pg(h.map(z=>({id:z.id,employeeId:x.id,manager:ref(z.mgr),fromDate:z.from,toDate:z.to,changedBy:{id:meId,displayName:z.by},reason:z.reason})),x)}
 case 'Employee_GetAdministrativeTaskLog':{const e=need(E(x.id));const t=e.roles.includes('departmentManager')?TLP.map((p,n)=>({occurredAt:new Date(Date.UTC(2026,10,4-n,9,0)).toISOString(),taskType:p[0],title:p[1+ix],referenceId:null})):[];return pg(t.filter(r=>!x.taskType||r.taskType===x.taskType),x)}
 case 'Manager_AssignEmployees':{const m=need(E(x.id));let a=0,al=0;const items=[];
  x.employeeIds.forEach(id=>{const e=E(id);if(!e)R(404,'Hr:Employee:EmployeeNotFound');if(e.status==='ended')R(422,'Hr:Employee:EmployeeEnded');if(e.mgr===m.id){al++;return}const c=mc(e,e.roles,m.id);if(c)R(422,c)});
  x.employeeIds.forEach(id=>{const e=E(id);if(e.mgr===m.id)return;const prev=e.mgr;(db.hist[e.id]||[]).forEach(h=>{if(!h.to)h.to=today()});(db.hist[e.id]=db.hist[e.id]||[]).push({id:this.nid(),mgr:m.id,from:today(),to:null,by:me?nmo(me):'—',reason:x.reason||null});e.mgr=m.id;e.stamp=stamp();a++;items.push({employee:ref(e.id),previousManager:ref(prev),newManager:ref(m.id),effectiveFrom:today(),movedPendingItemCount:0,concurrencyStamp:e.stamp})});
  return {manager:ref(m.id),assignedCount:a,alreadyAssignedCount:al,movedPendingItemCount:0,items}}
 case 'Manager_GetMyTeam':{let a=db.emps.filter(e=>e.mgr===meId);a=sr(a,x,e=>e.ar+e.en);if(x.status)a=a.filter(e=>e.status===x.status);const p=pg(a,x);return {totalCount:p.totalCount,items:p.items.map(e=>({id:e.id,fullName:nmo(e),email:e.email,phoneNumber:e.ph||null,department:{id:e.dept,name:dnm(e.dept)},jobTitle:e.jt[ix],hireDate:e.hire,status:e.status,currentShift:{workShiftId:U('41'),name:sh['41'][0],effectiveFrom:e.hire}}))}}
 case 'Manager_GetTeamMember':{const e=E(x.id);if(!e||e.mgr!==meId)R(404,'Hr:Employee:EmployeeNotFound');return {id:e.id,fullName:nmo(e),email:e.email,phoneNumber:e.ph||null,department:{id:e.dept,name:dnm(e.dept)},jobTitle:e.jt[ix],hireDate:e.hire,status:e.status,currentShift:{workShiftId:U('41'),name:sh['41'][0],effectiveFrom:e.hire}}}
 case 'UserAccount_List':{let a=db.emps.filter(e=>e.acc);a=sr(a,x,e=>e.acc.u+e.ar+e.en);if(x.roleName)a=a.filter(e=>e.roles.includes(x.roleName));if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(e=>e.acc.active===(x.isActive===true||x.isActive==='true'));if(x.mustChangePassword)a=a.filter(e=>e.acc.must);const p=pg(a,x);return {totalCount:p.totalCount,items:p.items.map(ad)}}
 case 'UserAccount_Lookup':{const a=sr(db.emps.filter(e=>e.acc),x,e=>e.ar+e.en);return {totalCount:a.length,items:pg(a,x).items.map(e=>({id:e.id,displayName:nmo(e)}))}}
 case 'UserAccount_Get':{const e=need(E(x.id));if(!e.acc)R(404,'Hr:Employee:EmployeeNotFound');return ad(e)}
 case 'UserAccount_Create':{const e=need(E(x.employeeId));if(e.status==='ended')R(422,'Hr:Employee:EmployeeEnded');if(e.acc)R(422,'Hr:Employee:EmployeeAlreadyHasAccount');roleChk(x.roleNames,null,actorExec);
  e.roles=x.roleNames;e.email=x.email||e.email;e.acc={u:x.userName,active:true,must:true,mfaE:false,locked:false,created:new Date().toISOString(),mfaR:x.roleNames.some(r=>['hrManager','executiveDirector','accountant','systemAdmin'].includes(r))};e.stamp=stamp();
  return {account:ad(e),credentials:{userId:e.id,userName:x.userName,temporaryPassword:'Tmp-'+Math.random().toString(36).slice(2,8)+'9',mustChangePassword:true}}}
 case 'UserAccount_ResetPassword':{const e=need(E(x.id));if(e.id===meId)R(422,'Hr:Identity:CannotResetOwnPassword');if(e.roles.includes('hrManager')&&!actorExec)R(422,'Hr:Identity:PasswordResetNotAllowedForTarget');if(!e.acc||!e.acc.active)R(422,'Hr:Identity:AccountInactive');if(!e.email)R(422,'Hr:Identity:NoVerifiedEmail');
  return {userId:e.id,deliveredVia:'email',expiresAt:new Date(Date.now()+864e5).toISOString()}}
 case 'UserAccount_SetRoles':{const e=need(E(x.id));chkS(e,x.concurrencyStamp);if(e.id===meId)R(422,'Hr:Identity:CannotChangeOwnRoles');if(e.status==='ended')R(422,'Hr:Employee:EmployeeEnded');
  const was=e.roles,now=x.roleNames;['hrManager','executiveDirector'].forEach(r=>{if(was.includes(r)!==now.includes(r)&&!actorExec)R(422,'Hr:Identity:ProtectedRoleChangeRequiresApproval')});
  roleChk(now,e,true);if(was.includes('systemAdmin')&&!now.includes('systemAdmin')&&!db.emps.some(z=>z!==e&&z.status==='active'&&z.roles.includes('systemAdmin')))R(422,'Hr:Identity:LastSystemAdminCannotBeRemoved');
  if(was.includes('departmentManager')&&!now.includes('departmentManager')&&drs(e)>0)R(422,'Hr:Identity:ManagerRoleHasDirectReports');
  const c=mc(e,now,x.managerEmployeeId||e.mgr);if(c)R(422,c);if(x.managerEmployeeId&&x.managerEmployeeId!==e.mgr){(db.hist[e.id]||[]).forEach(h=>{if(!h.to)h.to=today()});(db.hist[e.id]=db.hist[e.id]||[]).push({id:this.nid(),mgr:x.managerEmployeeId,from:today(),to:null,by:me?nmo(me):'—',reason:null});e.mgr=x.managerEmployeeId}
  if(now.includes('executiveDirector'))e.mgr=null;e.roles=now;e.acc.mfaR=now.some(r=>['hrManager','executiveDirector','accountant','systemAdmin'].includes(r));e.stamp=stamp();return ad(e)}
 case 'UserAccount_ResetMfa':{const e=need(E(x.id));if(e.id===meId)R(422,'Hr:Identity:CannotResetOwnMfa');if(!e.acc||!e.acc.mfaE)R(422,'Hr:Identity:MfaNotEnabled');if(!actorExec&&!(e.roles.includes('accountant')||e.roles.includes('systemAdmin')))R(422,'Hr:Identity:MfaResetNotAllowedForTarget');e.acc.mfaE=false;return null}
 case 'UserAccount_GetPermissions':{const e=need(E(x.id));return grp(db.grants['u:'+e.id]||{},letters(e.roles))}
 case 'UserAccount_UpdatePermissions':{const e=need(E(x.id));setP('u:'+e.id,x,e.roles,e.id===meId);return grp(db.grants['u:'+e.id]||{},letters(e.roles))}
 case 'Role_List':{const a=sr(db.roles,x,r=>r.nameAr+r.nameEn+r.name);const p=pg(a,x);return {totalCount:p.totalCount,items:p.items.map(rd)}}
 case 'Role_Get':{const r=db.roles.find(z=>z.id===x.id);if(!r)R(404,'Hr:Identity:RoleNotFound');return rd(r)}
 case 'Role_Create':{if(!/^[a-z][a-zA-Z0-9]{1,63}$/.test(x.name))R(400,'validation');const r={id:this.nid(),name:x.name,nameAr:x.nameAr,nameEn:x.nameEn,isStatic:false,stamp:stamp()};db.roles.push(r);return rd(r)}
 case 'Role_Update':{const r=db.roles.find(z=>z.id===x.id);if(!r)R(404,'Hr:Identity:RoleNotFound');chkS(r,x.concurrencyStamp);r.nameAr=x.nameAr;r.nameEn=x.nameEn;r.stamp=stamp();return rd(r)}
 case 'Role_Delete':{const r=db.roles.find(z=>z.id===x.id);if(!r)R(404,'Hr:Identity:RoleNotFound');if(r.isStatic)R(422,'Hr:Identity:StaticRoleCannotBeDeleted');if(db.emps.some(e=>e.roles.includes(r.name)))R(422,'Hr:Identity:RoleInUse');db.roles=db.roles.filter(z=>z!==r);return null}
 case 'Role_GetPermissions':{const r=db.roles.find(z=>z.id===x.id);if(!r)R(404,'Hr:Identity:RoleNotFound');return grp(db.grants['r:'+r.id]||{},r.isStatic?[AB[r.name]]:[])}
 case 'Role_UpdatePermissions':{const r=db.roles.find(z=>z.id===x.id);if(!r)R(404,'Hr:Identity:RoleNotFound');
  const cur=grp(db.grants['r:'+r.id]||{},r.isStatic?[AB[r.name]]:[]);const all={};cur.groups.forEach(g=>g.permissions.forEach(p=>all[p.name]=p.isGranted));db.grants['r:'+r.id]=all;setP('r:'+r.id,x,[r.name],false);return grp(db.grants['r:'+r.id],[])}
 case 'EmployeeDocument_List':{const e=need(E(x.id));const mine=e.id===meId,hrM=P.roles.includes('hrManager');if(P.roles.includes('employee')&&!hrM&&!P.roles.includes('departmentManager')&&!mine)R(403,'forbidden');
  let a=db.docs.filter(d=>d.emp===e.id);if(!(mine||hrM))a=a.filter(d=>d.sens==='normal');if(x.type)a=a.filter(d=>d.type===x.type);
  return pg(a.map(d=>({id:d.id,employeeId:d.emp,type:d.type,title:d.title.split('|')[ix]||d.title,fileName:d.file,contentType:d.ct,sizeBytes:d.size,sensitivity:d.sens,scanStatus:d.scan,uploadedBy:null,creationTime:d.at})),x)}
 case 'EmployeeDocument_Upload':{const e=need(E(x.id));if(e.status==='ended')R(422,'Hr:Employee:EmployeeEnded');if(!/\.(pdf|jpe?g|png)$/i.test(x.file))R(422,'Hr:Document:FileTypeNotAllowed');if((x.size||0)>10485760)R(422,'Hr:Document:FileTooLarge');
  const d={id:this.nid(),emp:e.id,type:x.type,title:x.title+'|'+x.title,file:x.file,ct:/pdf$/i.test(x.file)?'application/pdf':'image/jpeg',size:x.size||120000,sens:x.sensitivity,scan:'clean',at:new Date().toISOString()};db.docs.push(d);return {id:d.id,employeeId:d.emp,type:d.type,title:x.title,fileName:d.file,contentType:d.ct,sizeBytes:d.size,sensitivity:d.sens,scanStatus:d.scan,uploadedBy:null,creationTime:d.at}}
 case 'EmployeeDocument_GetDownloadUrl':{if(!db.docs.some(d=>d.id===x.id))R(422,'Hr:Document:NotAvailable');return {url:'https://files.example.test/'+x.id,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'EmployeeDocument_Delete':{db.docs=db.docs.filter(d=>d.id!==x.id);return null}
 }
 return undefined};
can=(p)=>{const r=NAVR[p];if(r===undefined)return true;const m=this.state.me;return !!m&&m.roleNames.some(x=>r.includes(this.AB3[x]))};
later=()=>setTimeout(()=>this.enter(),0);
nav2=(sub,sid,tab)=>{this.setState({sub,sid:sid||null,tab:tab||'data',data:{},fv:{},fe:{},dlg:null,step:0,skip:{},derr:''});this.later()};
setFlt=(k,v)=>{this.setState(s=>({flt:{...s.flt,[k]:v},skip:{}}));this.later()};
setF=(k,v)=>this.setState(s=>({fv:{...s.fv,[k]:v},fe:{...s.fe,[k]:''}}));
togg=(k,val,dflt)=>this.setState(s=>{const cur=s.fv[k]!==undefined?s.fv[k]:(dflt||[]);return{fv:{...s.fv,[k]:cur.includes(val)?cur.filter(z=>z!==val):[...cur,val]},fe:{...s.fe,[k]:''}}});
enter=async()=>{
 const s=this.state,p=s.page;if(s.scr!=='app'||!['emp','dept','acc','team','set','att','leave','req','cmp','rep','notif','aud','perf','ann','inv','cus','con','wrn','apr','fin','pay'].includes(p)||!this.can(p))return;
 this.setState({ldg:true,derr:'',denied:false});
 const q=(k)=>({skipCount:s.skip[k]||0,maxResultCount:10}),f=s.flt,D={};const exe0=!!(s.me&&s.me.roleNames.includes('executiveDirector')&&!s.me.roleNames.includes('hrManager')&&!s.me.roleNames.includes('systemAdmin'));
 try{
  if(p==='emp'){
   if(s.sub==='list'){D.list=await this.call('Employee_List',{...q('l'),search:f.eq||undefined,departmentId:f.ed||undefined,status:f.es||undefined,roleName:f.er||undefined});D.dl=await this.call('Department_Lookup',{});D.rl=await this.call('Role_List',{})}
   else if(s.sub==='file'){D.one=await this.call('Employee_Get',{id:s.sid});D.lk=await this.call('Employee_Lookup',{});
    if(s.tab==='docs')D.docs=await this.call('EmployeeDocument_List',{id:s.sid,...q('d')});
    if(s.tab==='hist')D.hist=await this.call('Employee_GetManagerHistory',{id:s.sid,...q('h')});
    if(s.tab==='task')D.task=await this.call('Employee_GetAdministrativeTaskLog',{id:s.sid,...q('t')})}
   else{D.dl=await this.call('Department_Lookup',{});D.lk=await this.call('Employee_Lookup',{});D.rl=await this.call('Role_List',{});if(s.sub==='edit'&&s.sid){D.one=await this.call('Employee_Get',{id:s.sid});if(s.fv.fullNameAr===undefined)this.prefill(D.one)}}
  }
else if(p==='rep'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['att','d'],['late','d'],['leave','d'],['pend','d'],['cust','d'],['eval','d'],['dash','v'],['cons','v'],['hatt','h'],['hbal','h'],['hreq','h'],['hcon','h'],['hwrn','h']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'att');
 const ms=this.todayS().slice(0,8)+'01',fr=f.rf||ms,to=f.rt||this.todayS();
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 const emp=f.re||undefined;
 if(hasL('d')&&['att','late','leave','pend','cust','eval'].includes(at)){D.tm=await this.call('Manager_GetMyTeam',{})}
 if(hasL('h')&&['hatt','hbal','hreq'].includes(at)){await tr('lk','Employee_Lookup',{});await tr('dp','Department_List',{})}
 if(at==='att')D.rep=await this.call('Report_GetTeamAttendance',{...q('l'),from:fr,to,employeeId:emp});
 else if(at==='late')D.rep=await this.call('Report_GetTeamLatenessAbsence',{...q('l'),from:fr,to,employeeId:emp});
 else if(at==='leave')D.rep=await this.call('Report_GetTeamLeaves',{...q('l'),from:fr,to,employeeId:emp});
 else if(at==='pend')D.rep=await this.call('Report_GetTeamPending',{...q('l'),employeeId:emp});
 else if(at==='cust')D.rep=await this.call('Report_GetTeamCustody',{...q('l'),employeeId:emp,kind:f.rk||undefined});
 else if(at==='eval')D.rep=await this.call('Report_GetTeamEvaluations',{...q('l'),employeeId:emp});
 else if(at==='dash')D.rep=await this.call('Report_GetReviewDashboard',{year:f.ry?Number(f.ry):undefined,month:f.rm?Number(f.rm):undefined});
 else if(at==='cons')D.rep=await this.call('Report_GetEvaluationConsistency',{...q('l')});
 else if(at==='hatt')D.rep=await this.call('Report_GetHrAttendance',{...q('l'),from:fr,to,departmentId:f.rd||undefined,employeeId:emp});
 else if(at==='hbal')D.rep=await this.call('Report_GetHrLeaveBalances',{...q('l'),departmentId:f.rd||undefined,employeeId:emp});
 else if(at==='hwrn')D.rep=await this.call('Report_GetHrWarnings',{...q('l'),from:fr,to,departmentId:f.rd||undefined,employeeId:f.re||undefined});
 else if(at==='hcon')D.rep=await this.call('Report_GetHrContracts',{...q('l'),endFrom:f.rf||undefined,endTo:f.rt||undefined,departmentId:f.rd||undefined,employeeId:f.re||undefined});
 else if(at==='hreq')D.rep=await this.call('Report_GetHrRequests',{...q('l'),from:fr,to,departmentId:f.rd||undefined,employeeId:emp});
}
else if(p==='cmp'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','ed'],['inbox','edhx'],['handlers','hx'],['cfg','m']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 if(s.sub==='mine'){D.one=await this.call('Complaint_GetMyDetail',{id:s.sid})}
 else if(s.sub==='handle'){D.one=await this.call('Complaint_GetDetail',{id:s.sid})}
 else if(s.sub==='log'){D.log=await this.call('Complaint_GetAccessLog',{id:s.sid,...q('l')})}
 else if(s.sub==='new'){D.rs=await this.call('Complaint_GetRespondents',{})}
 else if(at==='my'){D.list=await this.call('Complaint_GetMyList',{...q('l'),status:f.zs||undefined,level:f.zl||undefined})}
 else if(at==='inbox'){D.list=await this.call('Complaint_GetAssignedList',{...q('l'),status:f.zs||undefined,level:f.zl||undefined,isLate:f.zt===undefined||f.zt===''?undefined:f.zt==='true'})}
 else if(at==='handlers'){D.list=await this.call('ComplaintHandler_GetList',{...q('l'),status:f.zs||undefined,level:f.zl||undefined});if(hasL('h'))await tr('us','UserAccount_Lookup',{})}
 else if(at==='cfg'){D.cfg=await this.call('SystemSettings_GetComplaints',{})}
}
else if(p==='notif'){D.list=await this.call('Notification_List',{...q('l'),isRead:f.nr===undefined||f.nr===''?undefined:f.nr==='true'})}
else if(p==='aud'){try{D.lk=await this.call('UserAccount_Lookup',{})}catch(er){if(!(er&&er.code==='forbidden'))throw er}
 D.list=await this.call('AuditLog_List',{...q('l'),from:f.af?f.af+'T00:00:00.000Z':undefined,to:f.at2?f.at2+'T23:59:59.999Z':undefined,userId:f.au||undefined,action:f.aa||undefined,entityType:f.ae||undefined,succeeded:f.ao===undefined||f.ao===''?undefined:f.ao==='true'})}
else if(p==='perf'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','e'],['sum','e'],['team','d'],['rep','d'],['tsum','d'],['hr','h'],['hrep','h'],['hsum','h'],['cyc','v'],['crit','m']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 const yr=Number(f.sy||new Date().getFullYear());
 if(hasL('d')||hasL('h')||hasL('v'))await tr('cy','EvaluationCycle_GetCurrent',{});
 if(s.sub==='mine'){D.one=await this.call('Evaluation_GetMyDetail',{id:s.sid})}
 else if(s.sub==='form'){D.one=await this.call('Evaluation_GetManagerForm',{id:s.sid});D.notes=await this.call('EvaluationInternalNote_GetList',{id:s.sid})}
 else if(s.sub==='hrv'){D.one=await this.call('Evaluation_GetHrView',{id:s.sid});D.notes=await this.call('EvaluationInternalNote_GetList',{id:s.sid})}
 else if(s.sub==='rpt'){D.one=await this.call('DepartmentReport_Get',{id:s.sid})}
 else if(at==='my'){D.list=await this.call('Evaluation_GetMyList',{...q('l')})}
 else if(at==='sum'){D.sum=await this.call('EvaluationAnnualSummary_GetMy',{year:yr})}
 else if(at==='team'){D.list=await this.call('Evaluation_GetTeamList',{...q('l'),cycleId:f.ec||undefined,status:f.es||undefined,employeeId:f.ee||undefined});D.cl=await this.call('EvaluationCycle_GetList',{});D.tm=await this.call('Manager_GetMyTeam',{})}
 else if(at==='rep'){D.list=await this.call('DepartmentReport_GetList',{...q('l'),cycleId:f.ec||undefined,status:f.rs||undefined});D.cl=await this.call('EvaluationCycle_GetList',{})}
 else if(at==='tsum'){D.tm=await this.call('Manager_GetMyTeam',{});if(f.se)D.sum=await this.call('EvaluationAnnualSummary_GetForEmployee',{employeeId:f.se,year:yr})}
 else if(at==='hr'){D.list=await this.call('Evaluation_GetHrList',{...q('l'),cycleId:f.ec||undefined,status:f.es||undefined,objectionStatus:f.eo||undefined,departmentId:f.ed||undefined,employeeId:f.ee||undefined,isLate:f.el===undefined||f.el===''?undefined:f.el==='true'});D.cl=await this.call('EvaluationCycle_GetList',{});await tr('dp','Department_List',{});await tr('lk','Employee_Lookup',{})}
 else if(at==='hrep'){D.list=await this.call('DepartmentReport_GetList',{...q('l'),cycleId:f.ec||undefined,status:f.rs||undefined,isLate:f.el===undefined||f.el===''?undefined:f.el==='true'});D.cl=await this.call('EvaluationCycle_GetList',{})}
 else if(at==='hsum'){await tr('lk','Employee_Lookup',{});if(f.se)D.sum=await this.call('EvaluationAnnualSummary_GetForEmployee',{employeeId:f.se,year:yr})}
 else if(at==='cyc'){D.cl=await this.call('EvaluationCycle_GetList',{})}
 else if(at==='crit'){D.cr=await this.call('EvaluationCriterion_GetList',{maxResultCount:100});D.gr=await this.call('EvaluationGrade_GetList',{maxResultCount:100})}
}
else if(p==='ann'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['recv','edhxvasm'],['sent','dh']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:'recv';
 const ack=f.ra===undefined||f.ra===''?undefined:f.ra==='true';
 if(s.sub==='one')D.one=await this.call('Circular_Get',{id:s.sid});
 else if(s.sub==='rcpt'){D.sent=await this.call('Circular_GetSentList',{maxResultCount:100});D.list=await this.call('Circular_GetReceipts',{id:s.sid,...q('l'),isAcknowledged:ack})}
 else if(at==='recv')D.list=await this.call('Circular_GetReceivedList',{...q('l'),isAcknowledged:ack});
 else if(at==='sent')D.list=await this.call('Circular_GetSentList',{...q('l')});
}
else if(p==='inv'){
 const at=['items','moves','issues'].includes(s.tab)?s.tab:'items';
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 const bo=(v)=>v===undefined||v===''?undefined:v==='true';
 await tr('it','Item_Lookup',{maxResultCount:200});await tr('pur','StockIssue_GetList',{status:'purchasing',maxResultCount:100});
 if(at==='items')D.list=await this.call('Item_GetList',{...q('l'),kind:f.vk||undefined,isActive:bo(f.va),belowMinimum:bo(f.vb),filter:f.vn||undefined});
 else if(at==='moves'){D.list=await this.call('Stock_GetMovements',{...q('l'),itemId:f.vi||undefined,type:f.vt||undefined,fromDate:f.vf||undefined,toDate:f.vto||undefined});D.mv=await this.call('Stock_GetMovements',{maxResultCount:100})}
 else if(at==='issues'){D.list=await this.call('StockIssue_GetList',{...q('l'),status:f.vs||undefined,itemId:f.vi||undefined,employeeId:f.ve||undefined});await tr('lk','Employee_Lookup',{})}
}
else if(p==='cus'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','edhxvasm'],['team','d'],['all','sh'],['count','dsh']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:'my';
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 if(s.sub==='cnew'){D.sh=await this.call('Custody_GetTeam',{kind:'shared',maxResultCount:200})}
 else if(at==='my')D.list=await this.call('Custody_GetMy',{...q('l'),status:f.cs||undefined});
 else if(at==='team'){D.list=await this.call('Custody_GetTeam',{...q('l'),kind:f.ck||undefined,status:f.cs||undefined,employeeId:f.ce||undefined});D.tm=await this.call('Manager_GetMyTeam',{})}
 else if(at==='all'){D.list=await this.call('Custody_GetList',{...q('l'),kind:f.ck||undefined,status:f.cs||undefined,employeeId:f.ce||undefined,departmentId:f.cd||undefined,itemId:f.ci||undefined});await tr('lk','Employee_Lookup',{});await tr('dp','Department_List',{});await tr('it','Item_Lookup',{maxResultCount:200});await tr('ix','Item_Lookup',{kind:'returnable',maxResultCount:200})}
 else if(at==='count'){D.list=await this.call('CustodyCount_GetList',{...q('l'),departmentId:f.cd||undefined,result:f.cr||undefined,fromDate:f.xf||undefined,toDate:f.xt||undefined});await tr('dp','Department_List',{})}
}
else if(p==='con'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join('');
 const TBK=[['exp','h'],['all','h'],['my','e']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 if(s.sub==='one'){D.one=await this.call('Contract_Get',{id:s.sid});await tr('lk','Employee_Lookup',{});await tr('ren','Contract_List',{status:'renewed',maxResultCount:100})}
 else if(s.sub==='mine'){D.one=await this.call('Contract_GetMy',{id:s.sid})}
 else if(at==='exp'){D.list=await this.call('Contract_GetExpiring',{...q('l'),withinDays:f.cw?Number(f.cw):undefined,departmentId:f.cd||undefined});await tr('dp','Department_List',{});await tr('lk','Employee_Lookup',{});await tr('ren','Contract_List',{status:'renewed',maxResultCount:100})}
 else if(at==='all'){D.list=await this.call('Contract_List',{...q('l'),status:f.cs||undefined,employeeId:f.ce||undefined,departmentId:f.cd||undefined,endDateFrom:f.xf||undefined,endDateTo:f.xt||undefined});await tr('dp','Department_List',{});await tr('lk','Employee_Lookup',{});await tr('ren','Contract_List',{status:'renewed',maxResultCount:100})}
 else if(at==='my'){D.list=await this.call('Contract_GetMyList',{...q('l')})}
}
else if(p==='wrn'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','e'],['rec','dv'],['file','dh'],['hr','h'],['obj','x']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 const bo=(v)=>v===undefined||v===''?undefined:v==='true';
 if(s.sub==='mine')D.one=await this.call('Warning_GetMy',{id:s.sid});
 else if(s.sub==='one')D.one=await this.call('Warning_Get',{id:s.sid});
 else if(at==='my')D.list=await this.call('Warning_GetMyList',{...q('l')});
 else if(at==='rec'){D.list=await this.call('Warning_GetRecommendedByMe',{...q('l'),stage:f.ws||undefined});if(hasL('d'))await tr('tm','Manager_GetMyTeam',{});await tr('lk','Employee_Lookup',{})}
 else if(at==='file'){if(hasL('h'))await tr('lk','Employee_Lookup',{});else await tr('tm','Manager_GetMyTeam',{});if(f.we)D.list=await this.call('Warning_GetEmployeeFile',{employeeId:f.we,...q('l')})}
 else if(at==='hr'){D.list=await this.call('Warning_List',{...q('l'),status:f.ws||undefined,level:f.wl||undefined,employeeId:f.we||undefined,departmentId:f.wd||undefined,incidentDateFrom:f.xf||undefined,incidentDateTo:f.xt||undefined});await tr('lk','Employee_Lookup',{});await tr('dp','Department_List',{})}
 else if(at==='obj'){D.list=await this.call('Warning_GetObjections',{...q('l'),isPending:bo(f.wp)})}
}
else if(p==='apr'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','e'],['all','hx'],['file','dhx']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 const ack=f.aa===undefined||f.aa===''?undefined:f.aa==='true';
 if(s.sub==='one')D.one=await this.call('AppreciationLetter_Get',{id:s.sid});
 else if(at==='my')D.list=await this.call('AppreciationLetter_GetMyList',{...q('l')});
 else if(at==='all'){D.list=await this.call('AppreciationLetter_List',{...q('l'),employeeId:f.ae||undefined,isAcknowledged:ack});await tr('lk','Employee_Lookup',{})}
 else if(at==='file'){if(hasL('d')&&!hasL('h')&&!hasL('x'))await tr('tm','Manager_GetMyTeam',{});else await tr('lk','Employee_Lookup',{});if(f.ae)D.list=await this.call('AppreciationLetter_GetEmployeeFile',{employeeId:f.ae,...q('l'),isAcknowledged:ack})}
}
else if(p==='fin'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),TBK=[['sal','a'],['adj','a'],['sug','a'],['exp','a'],['req','a'],['run','ahx'],['rep','a']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'sal');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&(er.code==='forbidden'||er.status===404)))throw er}};
 const bo=(v)=>v===undefined||v===''?undefined:v==='true';
 if(s.sub==='sal'){await tr('prof','SalaryProfile_Get',{employeeId:s.sid});D.hist=await this.call('SalaryProfile_GetHistory',{employeeId:s.sid});D.emp=null}
 else if(s.sub==='arq'){D.one=await this.call('AccountantRequest_Get',{id:s.sid})}
 else if(s.sub==='rn'){D.one=await this.call('PayrollRun_Get',{id:s.sid})}
 else if(at==='sal'){D.list=await this.call('SalaryProfile_List',{...q('l'),hasProfile:bo(f.fh),filter:f.fn||undefined})}
 else if(at==='adj'){D.list=await this.call('Adjustment_List',{...q('l'),employeeId:f.fe||undefined,period:f.fp||undefined,type:f.ft||undefined,source:f.fs||undefined,status:f.fa||undefined});await tr('lk','Employee_Lookup',{});await tr('arq','AccountantRequest_List',{status:'pendingAccountant',maxResultCount:100})}
 else if(at==='sug'){D.list=await this.call('AdjustmentSuggestion_List',{...q('l'),period:f.fp||undefined,status:f.fa||undefined,basis:f.fb||undefined})}
 else if(at==='exp'){D.list=await this.call('Expense_List',{...q('l'),source:f.fs||undefined,category:f.fc||undefined,dateFrom:f.fd1||undefined,dateTo:f.fd2||undefined,status:f.fa||undefined});await tr('arq','AccountantRequest_List',{status:'pendingAccountant',maxResultCount:100})}
 else if(at==='req'){D.list=await this.call('AccountantRequest_List',{...q('l'),status:f.fa||undefined})}
 else if(at==='run'){try{D.list=await this.call('PayrollRun_List',{...q('l'),status:f.ps||undefined})}catch(er){if(er&&er.code==='forbidden')D.runForbidden=true;else throw er}}
 else if(at==='rep'){const code=f.fr||'monthlyStatement';try{if(code==='monthlyStatement'&&f.fq)D.rep=await this.call('FinanceReport_GetMonthlyStatement',{period:f.fq});else if(code==='adjustments'&&f.fa&&f.fb){await tr('lk','Employee_Lookup',{});D.rep=await this.call('FinanceReport_GetAdjustments',{...q('l'),periodFrom:f.fa,periodTo:f.fb,type:f.ft||undefined,employeeId:f.fe||undefined})}else if(code==='storeExpenses'&&f.fc&&f.fd)D.rep=await this.call('FinanceReport_GetStoreExpenses',{...q('l'),dateFrom:f.fc,dateTo:f.fd})}catch(er){if(er&&er.status===404)D.repMissing=true;else if(!(er&&er.code==='validation'))throw er}}
}
else if(p==='pay'){if(s.sub==='one')D.one=await this.call('Payslip_GetMy',{id:s.sid});else D.list=await this.call('Payslip_GetMyList',{...q('l')})}
else if(p==='req'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['inbox','dhxmas'],['my','edhxvasm'],['all','hd'],['memos','edhvasmx'],['inc','edhxvasm'],['deleg','edhxvasm'],['types','m']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'my');
 const tr=async(k,op,x)=>{try{D[k]=await this.call(op,x)}catch(er){if(!(er&&er.code==='forbidden'))throw er}};
 if(s.sub==='req'){D.one=await this.call('Request_Get',{id:s.sid})}
 else if(s.sub==='memo'){D.memo=await this.call('Memo_Get',{id:s.sid});await tr('av','OtherLeaveType_ListAvailable',{employeeId:D.memo.employee.id})}
 else if(s.sub==='inc'){D.inc=await this.call('Incident_Get',{id:s.sid})}
 else if(at==='inbox'){D.list=await this.call('Request_GetInbox',{...q('l'),requestTypeId:f.qt||undefined,status:f.qs||undefined});D.ty=await this.call('RequestType_GetList',{})}
 else if(at==='my'){D.list=await this.call('Request_GetMy',{...q('l'),employeeStatus:f.qs||undefined,requestTypeId:f.qt||undefined});D.ty=await this.call('RequestType_GetList',{});await tr('ity','Item_Lookup',{maxResultCount:200});if(hasL('d'))D.tm=await this.call('Manager_GetMyTeam',{})}
 else if(at==='all'){D.list=await this.call('Request_GetList',{...q('l'),status:f.qs||undefined,requestTypeId:f.qt||undefined,employeeId:f.qe||undefined,fromDate:f.xf||undefined,toDate:f.xt||undefined});D.ty=await this.call('RequestType_GetList',{});D.lk=hasL('h')?await this.call('Employee_Lookup',{}):await this.call('Manager_GetMyTeam',{})}
 else if(at==='memos'){await tr('mm','Memo_GetMy',{...q('l'),status:f.qs||undefined,kind:f.qk||undefined});await tr('mi','Memo_GetInbox',{...q('i')})}
 else if(at==='inc'){await tr('ity','Item_Lookup',{maxResultCount:200});await tr('im','Incident_GetMy',{...q('l'),status:f.qs||undefined});await tr('ii','Incident_GetInbox',{...q('i')})}
 else if(at==='deleg'){D.dg=await this.call('Delegation_GetMy',{...q('l'),direction:f.qd||undefined,status:f.qs||undefined});await tr('da','Delegation_List',{...q('i')});await tr('cd','Delegation_GetCandidates',{})}
 else if(at==='types'){D.list=await this.call('RequestType_GetAdminList',{...q('l')})}
}
else if(p==='leave'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['bal','edhm'],['my','edhm'],['emp','dhmx'],['cal','d'],['types','m'],['hourly','m']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'bal');
 const rg={fromDate:f.lf||undefined,toDate:f.lt||undefined};
 if(s.sub==='new'){D.av=await this.call('OtherLeaveType_ListAvailable',{employeeId:s.fv.forEmp||undefined});if(hasL('d'))D.tm=await this.call('Manager_GetMyTeam',{})}
 else if(at==='bal'){D.bal=await this.call('LeaveBalance_GetMy',{});D.led=await this.call('LeaveLedger_GetMy',{...q('g'),type:f.lg||undefined,...rg})}
 else if(at==='my'){D.list=await this.call('LeaveRequest_ListMy',{...q('l'),status:f.ls||undefined,kind:f.lk||undefined,...rg})}
 else if(at==='emp'){D.lk=(hasL('h')||hasL('m')||hasL('x'))?await this.call('Employee_Lookup',{}):await this.call('Manager_GetMyTeam',{});
  if(f.le){D.bal=await this.call('LeaveBalance_GetByEmployee',{id:f.le});D.led=await this.call('LeaveLedger_GetByEmployee',{id:f.le,...q('g'),type:f.lg||undefined,...rg});if(hasL('d')||hasL('h'))D.list=await this.call('LeaveRequest_ListByEmployee',{id:f.le,...q('l'),status:f.ls||undefined})}}
 else if(at==='cal'){D.tm=await this.call('Manager_GetMyTeam',{});const n=new Date(),ms=this.todayS().slice(0,8)+'01',me2=this.dAdd(this.dAdd(ms,32).slice(0,8)+'01',-1);D.cal=await this.call('LeaveCalendar_GetTeam',{fromDate:f.cf||ms,toDate:f.ct||me2,employeeId:f.ce||undefined,includePending:f.cp!=='false',...q('l')})}
 else if(at==='types'){D.list=await this.call('OtherLeaveType_List',{...q('l')})}
 else if(at==='hourly'){D.hs=await this.call('SystemSettings_GetHourlyLeave',{})}
}
else if(p==='att'){
 const rl3=(s.me?s.me.roleNames:[]).map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TBK=[['my','edhm'],['team','d'],['emp','dh'],['corr','edhm'],['shifts','hm'],['hol','edhxvasm'],['imp','h'],['dev','mh'],['meth','edhm']].filter(z=>[...z[1]].some(c=>rl3.includes(c))).map(z=>z[0]);
 const at=TBK.includes(s.tab)?s.tab:(TBK[0]||'hol');
 const rg={fromDate:f.xf||undefined,toDate:f.xt||undefined};
 if(hasL('e')||hasL('d')||hasL('h')||hasL('m'))D.cfg=await this.call('AttendanceSettings_Get',{});
 if(s.sub==='day'){D.day=await this.call('Attendance_GetDayDetail',{id:s.sid})}
 else if(s.sub==='map'){D.maps=await this.call('DeviceUserMap_List',{id:s.sid,...q('l')});D.un=await this.call('DeviceUserMap_ListUnmapped',{id:s.sid});D.lk=await this.call('Employee_Lookup',{})}
 else if(at==='my'){D.list=await this.call('Attendance_GetMy',{...q('l'),...rg,status:f.xs||undefined})}
 else if(at==='team'){D.tm=await this.call('Manager_GetMyTeam',{});D.list=await this.call('Attendance_GetTeam',{...q('l'),...rg,employeeId:f.xe||undefined,status:f.xs||undefined})}
 else if(at==='emp'){D.lk=hasL('h')?await this.call('Employee_Lookup',{}):await this.call('Manager_GetMyTeam',{});if(f.xe)D.list=await this.call('Attendance_GetByEmployee',{id:f.xe,...q('l'),...rg,status:f.xs||undefined})}
 else if(at==='shifts'){D.list=await this.call('WorkShift_List',{...q('l'),type:f.xh||undefined,isActive:f.xa===undefined||f.xa===''?undefined:f.xa==='true'});if(hasL('h')){D.lk=await this.call('Employee_Lookup',{});if(f.xi)D.hist=await this.call('EmployeeShift_GetHistory',{id:f.xi})}}
 else if(at==='hol'){D.list=await this.call('Holiday_List',{...q('l'),...rg});D.wk=await this.call('WeeklyOffSetting_Get',{})}
 else if(at==='imp'){D.list=await this.call('AttendanceImport_List',{...q('l'),status:f.xd||undefined});D.dv=await this.call('AttendanceDevice_List',{})}
 else if(at==='dev'){D.list=await this.call('AttendanceDevice_List',{...q('l'),isActive:f.xv===undefined||f.xv===''?undefined:f.xv==='true'})}
}
else if(p==='dept'){D.list=await this.call('Department_List',{...q('l')})}
  else if(p==='acc'&&exe0){D.list=await this.call('UserAccount_Lookup',{search:f.fq||undefined,...q('l')})}
  else if(p==='acc'){
   if(s.sub==='list'){D.list=await this.call('UserAccount_List',{...q('l'),search:f.aq||undefined,roleName:f.ar||undefined,isActive:f.aa===''||f.aa===undefined?undefined:f.aa==='1',mustChangePassword:f.am==='1'?true:undefined});D.rl=await this.call('Role_List',{})}
   else{D.one=await this.call('UserAccount_Get',{id:s.sid});D.rl=await this.call('Role_List',{});D.lk=await this.call('Employee_Lookup',{});if(s.tab==='perm')D.perm=await this.call('UserAccount_GetPermissions',{id:s.sid})}}
  else if(p==='team'){if(s.sub==='list')D.list=await this.call('Manager_GetMyTeam',{...q('l'),search:f.tq||undefined,status:f.ts||undefined});else D.one=await this.call('Manager_GetTeamMember',{id:s.sid})}
  else if(p==='set'){if(s.sub==='hub'||s.sub==='list')D.sec=await this.call('SystemSettings_ListSections',{});if(s.sub==='fin')D.fns=await this.call('SystemSettings_GetFinance',{});if(s.sub==='ctr')D.ctr=await this.call('SystemSettings_GetContracts',{});if(s.sub==='rem')D.rem=await this.call('SystemSettings_GetReminders',{});if(s.sub==='roles')D.list=await this.call('Role_List',{...q('l')});if(s.sub==='role'){D.one=await this.call('Role_Get',{id:s.sid});D.perm=await this.call('Role_GetPermissions',{id:s.sid})}}
  this.setState(x=>({data:{...x.data,...D},ldg:false}))
 }catch(e){this.setState({ldg:false,denied:!!(e&&e.code==='forbidden'),derr:(e&&e.code!=='forbidden')?(e.code||'gen'):''})}};
prefill=(e)=>this.setState({fv:{fullNameAr:e.fullNameAr,fullNameEn:e.fullNameEn||'',email:e.email||'',phoneNumber:e.phoneNumber||'',dateOfBirth:e.dateOfBirth||'',address:e.address||'',departmentId:e.department.id,jobTitle:e.jobTitle,hireDate:e.hireDate,status:e.status,gender:e.gender||'',employeeNumber:e.employeeNumber||''}});
vld=(specs)=>{const s=this.state,T=T4[s.lang],fe={};let ok=true;
 specs.forEach(sp=>{const v=s.fv[sp.k]!==undefined?s.fv[sp.k]:sp.dflt;const em=(v===undefined||v===null||v===''||(Array.isArray(v)&&!v.length));
  if(sp.req&&em){fe[sp.k]=T.req;ok=false;return}if(!em&&sp.min&&String(v).length<sp.min){fe[sp.k]=T.min;ok=false}if(!em&&sp.pat&&!sp.pat.test(String(v))){fe[sp.k]=T.pat;ok=false}});
 this.setState({fe});return ok};
fvm=(sp)=>{const s=this.state,v=s.fv[sp.k]!==undefined?s.fv[sp.k]:sp.dflt;
 return{invalid:!!s.fe[sp.k],isFile:sp.t==='file',items:sp.t==='file'?(Array.isArray(v)?v:[]).map(a=>({l:a.fileName+' · '+Math.max(1,Math.round(a.sizeBytes/1024))+' KB',rm:()=>this.setF(sp.k,v.filter(z=>z.id!==a.id))})):[],onFile:sp.t==='file'?(sp.up?(e=>sp.up(e)):(e=>this.lvUpload(e,sp.k))):null,k:sp.k,l:sp.l,isTx:!sp.t||sp.t==='text'||sp.t==='email'||sp.t==='date',isTa:sp.t==='ta',isSel:sp.t==='sel',isCk:sp.t==='ck',type:sp.t==='email'?'email':sp.t==='date'?'date':'text',value:Array.isArray(v)?'':(v==null?'':v),max:sp.max||524288,on:e=>this.setF(sp.k,e.target.value),
 opts:sp.t==='sel'?[{v:'',l:'—'},...sp.opts]:(sp.t==='ck'?sp.opts.map(o=>({v:o.v,l:o.l,on:(v||[]).includes(o.v),toggle:()=>this.togg(sp.k,o.v,sp.dflt)})):[]),err:s.fe[sp.k]||'',help:sp.h||'',req:!!sp.req}};
etx=(code)=>{const L=this.state.lang,k=T4[L].er2[code]||T2[L].er[code];return k?k:(T2[L].er.gen+(code&&code!=='gen'?' ('+code+')':''))};
dlgOpen=(c)=>{const fv={};(c.fields||[]).forEach(f=>{if(f.init!==undefined)fv[f.k]=f.init});this.setState({dlg:c,fv,fe:{},derr2:'',busy:false})};
dlgClose=()=>this.setState({dlg:null,derr2:'',busy:false});
dlgGo=async()=>{const s=this.state,c=s.dlg;if(!c||s.busy)return;if(!c.run){this.dlgClose();return}
 if(c.fields&&c.fields.length&&!this.vld(c.fields))return;
 this.setState({busy:true,derr2:''});
 try{const r=await c.run(this.state.fv);this.setState({dlg:null,busy:false});
  if(r&&r.next){this.dlgOpen(r.next)}if(r&&r.m)this.toast(r.m);if(r&&r.go)r.go();else this.enter()}
 catch(e){if(e&&e.status===403&&/Required$/.test(e.code||'')){this.boot();return}this.setState({busy:false,derr2:(e&&e.code)||'gen'})}};
act=async(op,x,okMsg,then)=>{ /* page-level action with error banner */
 if(this.state.busy)return;this.setState({busy:true,derr:''});
 try{const r=await this.call(op,x);this.setState({busy:false});if(okMsg)this.toast(okMsg);if(then)then(r);else this.enter()}catch(e){this.setState({busy:false,derr:(e&&e.code)||'gen'})}};
specEmp=(step)=>{const s=this.state,T=T4[s.lang],d=s.data,dl=(d.dl?d.dl.items:[]).map(x=>({v:x.id,l:x.name})),lk=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName})),rl=(d.rl?d.rl.items:[]).map(x=>({v:x.name,l:x.displayName}));
 const sh=[{v:'41',l:s.lang==='ar'?'دوام ثابت صباحي':'Fixed day shift'},{v:'42',l:s.lang==='ar'?'دوام متناوب':'Rotating shift'}];
 const gd=[{v:'male',l:T.male},{v:'female',l:T.female}];
 const a=[{k:'fullNameAr',l:T.lFullNameAr,req:1,max:200},{k:'fullNameEn',l:T.lFullNameEn,max:200},{k:'email',l:T.lEmail,t:'email'},{k:'phoneNumber',l:T.lPhone,max:32},{k:'dateOfBirth',l:T.lDob,t:'date'},{k:'address',l:T.lAddr,t:'ta',max:500},{k:'departmentId',l:T.lDept,t:'sel',opts:dl,req:1},{k:'jobTitle',l:T.lTitle,req:1,max:128},{k:'hireDate',l:T.lHire,t:'date',req:1},{k:'gender',l:T.lGen,t:'sel',opts:gd},{k:'employeeNumber',l:T.lNum,max:32}];
 const b=[{k:'workShiftId',l:T.lShift,t:'sel',opts:sh,h:'WorkShift_List (module 3, mock list)'},{k:'roleNames',l:T.lRoles,t:'ck',opts:rl,req:1},{k:'managerEmployeeId',l:T.lMgr,t:'sel',opts:lk}];
 const c=[{k:'userName',l:T.lUser,req:1,min:3,max:64}];
 return step===0?a:step===1?b:c};
startCreate=()=>{this.setState({page:'emp',sub:'new',sid:null,tab:'data',data:{},fv:{},fe:{},step:0,dlg:null,skip:{}});this.later()};
fmtD=(d,ar)=>d?new Date(String(d).length>10?d:d+'T00:00:00').toLocaleDateString(ar?'ar-IQ':'en-GB',{year:'numeric',month:'short',day:'numeric'}):'—';
fmtT=(d,ar)=>d?new Date(d).toLocaleString(ar?'ar-IQ':'en-GB',{dateStyle:'medium',timeStyle:'short'}):'—';
m2=(f)=>{try{return this.m2x(f)}catch(err){const T=T4[this.state.lang];return{isOrg:['emp','dept','acc','team','set','att','leave','req','cmp','rep','notif','aud','perf','ann','inv','cus','con','wrn','apr','fin','pay'].includes(this.state.page)&&this.state.scr==='app',vm:{crumbs:[],denT:T.denT,denD:T.denD,retryL:T.retry,prevL:T.prev2,nextL:T.next,title:'',sub:'',tools:[],hasFlt:false,flt:[],hasTabs:false,tabs:[],loading:false,denied:false,err:true,errTx:this.etx('gen'),retry:()=>this.enter(),blocks:[],hasPg:false,pgRange:'',canPrev:false,canNext:false,prev:()=>{},next:()=>{}},hasDlg:false,dlgv:{}}}};
m2x=(f)=>{
 const s=this.state,L=s.lang,ar=L==='ar',T=T4[L],T3=T2[L],d=s.data||{},p=s.page,me=s.me,rl=me?me.roleNames:[];
 const isR=r=>rl.includes(r);
 const vm={crumbs:[],denT:T.denT,denD:T.denD,retryL:T.retry,prevL:T.prev2,nextL:T.next,title:'',sub:'',tools:[],hasFlt:false,flt:[],hasTabs:false,tabs:[],loading:s.ldg,denied:false,err:false,errTx:'',retry:()=>this.enter(),blocks:[],hasPg:false,pgRange:'',canPrev:false,canNext:false,prev:()=>{},next:()=>{}};
 const isOrg=['emp','dept','acc','team','set','att','leave','req','cmp','rep','notif','aud','perf','ann','inv','cus','con','wrn','apr','fin','pay'].includes(p)&&s.scr==='app';
 if(!isOrg)return{isOrg:false,vm,hasDlg:false,dlgv:{}};
 if(!this.can(p)||s.denied){vm.denied=true;vm.title=T.denT;return{isOrg:true,vm,hasDlg:false,dlgv:{},...this.dl2(T,f)}}
 if(s.derr){vm.err=true;vm.errTx=this.etx(s.derr)}
 const dt=(x)=>this.fmtD(x,ar),tm=(x)=>this.fmtT(x,ar),nv=x=>x==null||x===''?'—':x;
 const col=(...h)=>h.map(x=>({h:x})),row=(c,a)=>({cells:c.map(v=>({v:nv(v)})),acts:a||[]}),ac=(l,go)=>({l,go});
 const flt=(k,l,t,o)=>({k,l,isSel:t==='sel',isTx:t==='tx',v:s.flt[k]||'',on:e=>this.setFlt(k,e.target.value),opts:[{v:'',l:T.fAll},...(o||[])]});
 const kvB=(t,sb,kv)=>({hasT:!!t,t,s:sb||'',hasKv:true,kv:kv.map(a=>({k:a[0],v:nv(a[1])})),hasTbl:false,hasForm:false,hasActs:false,fields:[],acts:[],cols:[],rows:[],empty:false,emptyTx:''});
 const tbB=(t,cols,rows,et)=>({hasT:!!t,t,s:'',hasKv:false,kv:[],hasTbl:true,cols,rows,empty:rows.length===0&&!s.ldg,emptyTx:et,hasForm:false,hasActs:false,fields:[],acts:[]});
 const fmB=(t,specs,acts,extra)=>({hasT:!!t,t,s:'',hasKv:false,kv:[],hasTbl:false,cols:[],rows:[],empty:false,emptyTx:'',hasForm:true,fields:specs.map(z=>this.fvm(z)),hasActs:acts.length>0,acts:acts.map(a=>({l:a.l,cls:a.cls||'',go:a.go})),...(extra||{})});
 const pgr=(k,pd)=>{const sk=s.skip[k]||0,tot=pd?pd.totalCount:0;vm.hasPg=!!pd&&tot>10;vm.pgRange=tot?f(sk+1)+'–'+f(Math.min(sk+10,tot))+' / '+f(tot):'';vm.canPrev=sk>0;vm.canNext=sk+10<tot;vm.prev=()=>{this.setState(x=>({skip:{...x.skip,[k]:Math.max(0,sk-10)}}));this.later()};vm.next=()=>{this.setState(x=>({skip:{...x.skip,[k]:sk+10}}));this.later()}};
 const ste=x=>T.ste[x]||x,rn=r=>T3.rl[r]||r;
 const rlOpts=(d.rl?d.rl.items:[]).map(x=>({v:x.name,l:x.displayName})),dlOpts=(d.dl?d.dl.items:[]).map(x=>({v:x.id,l:x.name}));
 const tab=(k,l)=>({l,cls:s.tab===k?'on':'',go:()=>{this.setState({tab:k,skip:{}});this.later()}});
 const delc=(msg,op,x,after)=>()=>this.dlgOpen({title:T.dDel,body:msg||T.dDelB,okLabel:T.del,danger:true,fields:[],run:async()=>{await this.call(op,x);return{m:T.tDeleted,go:after}}});
 const exe=isR('executiveDirector'),hrm=isR('hrManager'),adm=isR('systemAdmin');
 if(p==='emp'){
  if(s.sub==='list'){vm.title=T.eT;vm.sub=T.eSub;vm.tools=[{l:T.eNew,cls:'p',go:this.startCreate}];vm.hasFlt=true;
   vm.flt=[flt('eq',T.fSearch,'tx'),flt('ed',T.fDept,'sel',dlOpts),flt('es',T.fStatus,'sel',['active','suspended','ended'].map(x=>({v:x,l:ste(x)}))),flt('er',T.fRole,'sel',rlOpts)];
   const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
   vm.blocks=[tbB('',col(T.cName,T.cDept,T.cTitle,T.cHire,T.cStatus,T.cMgr,T.cRoles,T.cAcc,''),l.items.map(e=>row([e.fullName,e.department.name,e.jobTitle,dt(e.hireDate),ste(e.status),e.manager?e.manager.fullName:'—',e.roleNames.map(rn).join('، '),e.hasAccount?T.yes:T.no],[ac(T.open,()=>this.nav2('file',e.id))])),T.emE)]}
  else if(s.sub==='file'){const e=d.one;vm.title=e?e.fullName:'…';vm.sub=T.eFile;
   vm.tools=[{l:T.back,cls:'',go:()=>this.nav2('list')}];
   if(e){vm.tools.push({l:T.edit,cls:'',go:()=>{this.setState({sub:'edit',fv:{},fe:{}});this.later()}},{l:T.aChgMgr,cls:'',go:()=>this.dlgOpen({title:T.dChgMgr,fields:[{k:'managerEmployeeId',l:T.lMgr,t:'sel',req:1,opts:(d.lk?d.lk.items:[]).filter(z=>z.id!==e.id).map(z=>({v:z.id,l:z.fullName}))},{k:'reason',l:T.lReason,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('Employee_ChangeManager',{id:e.id,managerEmployeeId:v.managerEmployeeId,reason:v.reason||undefined,concurrencyStamp:e.concurrencyStamp});return{m:T.tSaved+' · '+f(r.movedPendingItemCount)}}})});
    if(e.account&&e.account.roleNames.includes('departmentManager'))vm.tools.push({l:T.aAssign,go:()=>this.dlgOpen({title:T.dAssign,fields:[{k:'ids',l:T.lEmps,t:'ck',req:1,opts:(d.lk?d.lk.items:[]).filter(z=>z.id!==e.id).map(z=>({v:z.id,l:z.fullName}))},{k:'reason',l:T.lReason,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('Manager_AssignEmployees',{id:e.id,employeeIds:v.ids,reason:v.reason||undefined});return{m:T.tAssigned+' · '+f(r.assignedCount)+' / '+f(r.alreadyAssignedCount)}}})});
    if(!e.account)vm.tools.push({l:T.aNewAcc,cls:'p',go:()=>this.dlgOpen({title:T.dNewAcc,fields:[{k:'userName',l:T.lUser,req:1,min:3,max:64},{k:'email',l:T.lEmail,t:'email',init:e.email||''},{k:'roleNames',l:T.lRoles,t:'ck',req:1,opts:rlOpts}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('UserAccount_Create',{employeeId:e.id,userName:v.userName,email:v.email||undefined,roleNames:v.roleNames});return{next:this.credDlg(T,r.credentials)}}})})}
   vm.hasTabs=true;vm.tabs=[tab('data',T.tData),tab('docs',T.tDocs),tab('hist',T.tHist)];if(e&&e.account&&0)vm.tabs.push(tab('task',T.tTask));
   if(e&&(e.directReportCount>0))vm.tabs.push(tab('task',T.tTask));
   if(s.tab==='data'&&e)vm.blocks=[kvB(T.tData,T.notes,[[T.kNameAr,e.fullNameAr],[T.kNameEn,e.fullNameEn],[T.kNum,e.employeeNumber],[T.cDept,e.department.name],[T.cTitle,e.jobTitle],[T.cHire,dt(e.hireDate)],[T.cStatus,ste(e.status)],[T.cMgr,e.manager?e.manager.fullName:'—'],[T.kEmail,e.email],[T.kPhone,e.phoneNumber],[T.kDob,dt(e.dateOfBirth)],[T.kAddr,e.address],[T.kGen,e.gender?T[e.gender]:'—'],[T.kShift,e.currentShift?e.currentShift.name+' · '+dt(e.currentShift.effectiveFrom):'—'],[T.kContract,e.currentContract?dt(e.currentContract.startDate)+' → '+(e.currentContract.endDate?dt(e.currentContract.endDate):'—'):'—'],[T.kReports,f(e.directReportCount)],[T.kCreated,tm(e.creationTime)]]),
    kvB(T.kAcct,'',e.account?[[T.cUser,e.account.userName],[T.kActive,e.account.isActive?T.yes:T.no],[T.kMust,e.account.mustChangePassword?T.yes:T.no],[T.kMfaReq,e.account.mfaRequired?T.yes:T.no],[T.kMfaOn,e.account.mfaEnabled?T.yes:T.no],[T.cRoles,e.account.roleNames.map(rn).join('، ')]]:[[T.kAcct,T.kNoAcct]])];
   if(s.tab==='docs'){const l=d.docs||{items:[],totalCount:0};pgr('d',d.docs);
    if(e)vm.tools.push({l:T.aUp,cls:'p',go:()=>this.dlgOpen({title:T.dUp,fields:[{k:'type',l:T.lDocType,t:'sel',req:1,opts:Object.keys(T.dt).map(x=>({v:x,l:T.dt[x]}))},{k:'title',l:T.lDocTitle,req:1,max:200},{k:'sensitivity',l:T.lSens,t:'sel',req:1,opts:Object.keys(T.sens).map(x=>({v:x,l:T.sens[x]}))},{k:'file',l:T.lFile,req:1,h:T.hFile}],okLabel:T.aUp,run:async(v)=>{await this.call('EmployeeDocument_Upload',{id:e.id,file:v.file,size:120000,type:v.type,title:v.title,sensitivity:v.sensitivity});return{m:T.tCreated}}})});
    vm.blocks=[tbB(T.tDocs,col(T.cFile,T.lDocTitle,T.lDocType,T.cSize,T.cSens,T.cScan,''),l.items.map(x=>row([x.fileName,x.title,T.dt[x.type],f(Math.round(x.sizeBytes/1024))+' KB',T.sens[x.sensitivity],T.scan[x.scanStatus]],[ac(T.aDl,()=>this.act('EmployeeDocument_GetDownloadUrl',{id:x.id},T.tSaved.replace(T.tSaved,'↓ (mock)'),()=>{})),...(hrm?[ac(T.del,delc('','EmployeeDocument_Delete',{id:x.id},()=>this.enter()))]:[])])),T.emD)]}
   if(s.tab==='hist'){const l=d.hist||{items:[],totalCount:0};pgr('h',d.hist);vm.blocks=[tbB(T.tHist,col(T.cMgr,T.cFrom,T.cTo,T.cBy,T.cReason),l.items.map(x=>row([x.manager.fullName,dt(x.fromDate),dt(x.toDate),x.changedBy?x.changedBy.displayName:'—',x.reason])),T.emH)]}
   if(s.tab==='task'){const l=d.task||{items:[],totalCount:0};pgr('t',d.task);vm.blocks=[tbB(T.tTask,col(T.cTime,T.cTask),l.items.map(x=>row([tm(x.occurredAt),T3.tt[x.taskType]+' — '+x.title])),T.emTask)]}}
  else{ /* new or edit */
   const ed=s.sub==='edit',e=d.one;vm.title=ed?T.eEdit:T.eNew;vm.sub=ed&&e?e.fullName:'';
   const steps=[T.st1,T.st2,T.st3];
   if(!ed){vm.hasTabs=true;vm.tabs=steps.map((x,i)=>({l:(i+1)+'. '+x,cls:s.step===i?'on':'',go:()=>{}}))}
   const cancel={l:T.cncl,go:()=>this.nav2(ed?'file':'list',ed?s.sid:null)};
   if(ed){const sp=[...this.specEmp(0),{k:'status',l:T.lStatus,t:'sel',req:1,opts:['active','suspended','ended'].map(x=>({v:x,l:ste(x)}))}];
    vm.blocks=[fmB('',sp,[cancel,{l:T.save2,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;await this.act('Employee_Update',{id:s.sid,fullNameAr:v.fullNameAr,fullNameEn:v.fullNameEn||null,email:v.email||null,phoneNumber:v.phoneNumber||null,dateOfBirth:v.dateOfBirth||null,address:v.address||null,departmentId:v.departmentId,jobTitle:v.jobTitle,hireDate:v.hireDate,status:v.status,concurrencyStamp:e.concurrencyStamp,gender:v.gender||null,employeeNumber:v.employeeNumber||null},T.tSaved,()=>this.nav2('file',s.sid))}}])]}
   else{const sp=this.specEmp(s.step);
    if(s.step<2)vm.blocks=[fmB(steps[s.step],sp,[...(s.step>0?[{l:T.prev2,go:()=>this.setState({step:s.step-1})}]:[cancel]),{l:T.next,cls:'p',go:()=>{if(this.vld(sp)){if(s.step===1&&!(this.state.fv.roleNames||[]).includes('executiveDirector')&&!this.state.fv.workShiftId){this.setState({fe:{workShiftId:T.req}});return}this.setState({step:s.step+1})}}}])];
    else{const v=s.fv,lab=(o,k)=>{const x=o.find(z=>z.v===k);return x?x.l:'—'};const a=this.specEmp(0),b=this.specEmp(1);
     const rev=[[T.lFullNameAr,v.fullNameAr],[T.lFullNameEn,v.fullNameEn],[T.lEmail,v.email],[T.lDept,lab(a[6].opts,v.departmentId)],[T.lTitle,v.jobTitle],[T.lHire,dt(v.hireDate)],[T.lShift,lab(b[0].opts,v.workShiftId)],[T.lMgr,lab(b[2].opts,v.managerEmployeeId)],[T.lRoles,(v.roleNames||[]).map(rn).join('، ')]];
     vm.blocks=[fmB(steps[2],sp,[{l:T.prev2,go:()=>this.setState({step:1})},{l:T.finish,cls:'p',go:async()=>{if(!this.vld(sp))return;const x=this.state.fv;await this.act('Employee_Create',{fullNameAr:x.fullNameAr,fullNameEn:x.fullNameEn||undefined,email:x.email||undefined,phoneNumber:x.phoneNumber||undefined,dateOfBirth:x.dateOfBirth||undefined,address:x.address||undefined,departmentId:x.departmentId,jobTitle:x.jobTitle,hireDate:x.hireDate,workShiftId:x.workShiftId||undefined,managerEmployeeId:x.managerEmployeeId||undefined,userName:x.userName,roleNames:x.roleNames,gender:x.gender||undefined,employeeNumber:x.employeeNumber||undefined},null,(r)=>{this.nav2('file',r.employee.id);this.dlgOpen(this.credDlg(T,r.credentials))})}}]),kvB(T.st3,'',rev)]}}}
 }
 if(p==='dept'){vm.title=T.dT;vm.tools=[{l:T.dNew,cls:'p',go:()=>this.dlgOpen(this.deptDlg(T,null))}];const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
  vm.blocks=[tbB('',col(T.lNameAr,T.lNameEn,T.cCnt,''),l.items.map(x=>row([x.nameAr,x.nameEn,f(x.employeeCount)],[ac(T.edit,()=>this.dlgOpen(this.deptDlg(T,x))),ac(T.del,delc('','Department_Delete',{id:x.id}))])),T.emDp)]}
 if(p==='acc'&&exe&&!hrm&&!adm){const F=T.fx;vm.title=F.title;vm.sub=F.sub;vm.hasFlt=true;vm.flt=[{k:'fq',l:F.srch,isSel:false,isTx:true,isDt:false,v:s.flt.fq||'',on:e=>this.setFlt('fq',e.target.value),opts:[]}];const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
 vm.blocks=[tbB('',col(T.cName,''),l.items.map(u=>row([u.displayName],[ac(F.act1,()=>this.dlgOpen({title:T.dReset,body:T.dResetB,fields:[{k:'reason',l:T.lReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('UserAccount_ResetPassword',{id:u.id,reason:v.reason});return{m:T.tSent}}})),ac(F.act2,()=>this.dlgOpen({title:T.dMfa,body:T.dMfaB,fields:[{k:'reason',l:T.lReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('UserAccount_ResetMfa',{id:u.id,reason:v.reason});return{m:T.tMfaReset}}}))])),F.emp)]}
 if(p==='acc'&&!(exe&&!hrm&&!adm)){
  if(s.sub==='list'){vm.title=T.aT;vm.hasFlt=true;vm.flt=[flt('aq',T.fSearch,'tx'),flt('ar',T.fRole,'sel',rlOpts),flt('aa',T.fStatus,'sel',[{v:'1',l:T.fActive},{v:'0',l:T.fInactive}]),flt('am',T.fMust,'sel',[{v:'1',l:T.yes}])];const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
   vm.blocks=[tbB('',col(T.cUser,T.cName,T.kEmail,T.cRoles,T.kActive,T.cMfa,T.kMust,''),l.items.map(a=>row([a.userName,a.employee?a.employee.fullName:'—',a.email,a.roleNames.map(rn).join('، '),a.isActive?T.yes:T.no,a.mfaRequired?(a.mfaEnabled?T.yes:T.no):'—',a.mustChangePassword?T.yes:T.no],[ac(T.open,()=>this.nav2('file',a.id))])),T.emA)]}
  else{const a=d.one;vm.title=a?a.userName:'…';vm.sub=T.aD;vm.tools=[{l:T.back,go:()=>this.nav2('list')}];
   if(a){if(hrm||exe)vm.tools.push({l:T.aReset,go:()=>this.dlgOpen({title:T.dReset,body:T.dResetB,fields:[{k:'reason',l:T.lReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('UserAccount_ResetPassword',{id:a.id,reason:v.reason});return{m:T.tSent}}})});
    if(adm||exe)vm.tools.push({l:T.aMfa,go:()=>this.dlgOpen({title:T.dMfa,body:T.dMfaB,fields:[{k:'reason',l:T.lReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('UserAccount_ResetMfa',{id:a.id,reason:v.reason});return{m:T.tMfaReset}}})});
    vm.tools.push({l:T.aRoles,cls:'p',go:()=>this.dlgOpen({title:T.dRoles,fields:[{k:'roleNames',l:T.lRoles,t:'ck',req:1,init:a.roleNames,opts:rlOpts},{k:'managerEmployeeId',l:T.lMgr,t:'sel',opts:(d.lk?d.lk.items:[]).filter(z=>z.id!==a.id).map(z=>({v:z.id,l:z.fullName}))}],okLabel:T.cnf,run:async(v)=>{await this.call('UserAccount_SetRoles',{id:a.id,roleNames:v.roleNames,managerEmployeeId:v.managerEmployeeId||undefined,concurrencyStamp:a.concurrencyStamp});return{m:T.tSaved}}})})}
   if(adm){vm.hasTabs=true;vm.tabs=[tab('data',T.tData),tab('perm',T.tPerm)]}
   if(a&&s.tab==='data')vm.blocks=[kvB(T.aD,T.notes,[[T.cUser,a.userName],[T.cName,a.employee?a.employee.fullName:'—'],[T.kEmail,a.email],[T.cRoles,a.roleNames.map(rn).join('، ')],[T.kActive,a.isActive?T.yes:T.no],[T.kLocked,a.isLockedOut?T.yes:T.no],[T.kMust,a.mustChangePassword?T.yes:T.no],[T.kMfaReq,a.mfaRequired?T.yes:T.no],[T.kMfaOn,a.mfaEnabled?T.yes:T.no],[T.kCreated,tm(a.creationTime)]])];
   if(a&&s.tab==='perm'&&d.perm)vm.blocks=this.permB(T,d.perm,'UserAccount_UpdatePermissions',a.id,fmB)}}
 if(p==='team'){
  if(s.sub==='list'){vm.title=T.tT;vm.sub=T.tSub;vm.hasFlt=true;vm.flt=[flt('tq',T.fSearch,'tx'),flt('ts',T.fStatus,'sel',['active','suspended','ended'].map(x=>({v:x,l:ste(x)})))];const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
   vm.blocks=[tbB('',col(T.cName,T.cDept,T.cTitle,T.cHire,T.cStatus,T.kShift,''),l.items.map(e=>row([e.fullName,e.department.name,e.jobTitle,dt(e.hireDate),ste(e.status),e.currentShift?e.currentShift.name:'—'],[ac(T.open,()=>this.nav2('file',e.id))])),T.emT)]}
  else{const e=d.one;vm.title=e?e.fullName:'…';vm.sub=T.eFile;vm.tools=[{l:T.back,go:()=>this.nav2('list')}];if(e)vm.blocks=[kvB(T.tData,T.notes,[[T.cDept,e.department.name],[T.cTitle,e.jobTitle],[T.cHire,dt(e.hireDate)],[T.cStatus,ste(e.status)],[T.kEmail,e.email],[T.kPhone,e.phoneNumber],[T.kShift,e.currentShift?e.currentShift.name:'—']])]}}
 if(p==='set'){
  if(s.sub==='hub'||s.sub==='list'){vm.title=T.sT;vm.sub=T.sSub;const secs=d.sec?d.sec.items:[];
   vm.blocks=secs.map(z=>{const dst=this.secDest(z.code),adminOnly=['reminders','contracts','finance','complaints','hourlyLeave','requestTypes','otherLeaveTypes','evaluationCriteria'].includes(z.code);const usable=dst&&this.can(dst.page)&&(z.isEditableByCaller||!adminOnly);
    const b=kvB(T.secL[z.code],T.secD[z.code]+' · '+(T3.rl[z.ownerRole]||z.ownerRole)+' · '+(z.isEditableByCaller?T.secEd:T.secRo)+(dst?'':' · '+T.secSoon),[]);b.hasKv=false;
    if(usable){b.hasActs=true;b.acts=[{l:T.sOpen,cls:'p',go:()=>dst.sub?this.nav2(dst.sub):this.goTab(dst.page,dst.tab)}]}return b})}
  else if(s.sub==='fin'){vm.title=T.finT;vm.sub=T.finSub;vm.tools=[{l:T.back,go:()=>this.nav2('hub')}];const r=d.fns;
   if(r){const sp=[{k:'r',l:T.fFiRole,t:'sel',req:1,dflt:r.payrollApproverRole||'hrManager',opts:Object.keys(T.fpar).map(x=>({v:x,l:T.fpar[x]}))}];vm.blocks=[kvB(T.finT,T.finNote,[[T.fFiRole,r.payrollApproverRole?T.fpar[r.payrollApproverRole]:'—']])];
    if(adm)vm.blocks.push(fmB(T.finT,sp,[{l:T.ctrSave,cls:'p',go:async()=>{const v=this.state.fv;await this.act('SystemSettings_UpdateFinance',{payrollApproverRole:v.r!==undefined?v.r:(r.payrollApproverRole||'hrManager'),concurrencyStamp:r.concurrencyStamp},T.tFiSet,()=>{this.setState({fv:{}});this.enter()})}}]))}}
  else if(s.sub==='ctr'){vm.title=T.ctrT;vm.sub=T.ctrSub;vm.tools=[{l:T.back,go:()=>this.nav2('hub')}];const r=d.ctr;
   if(r){const sp=[{k:'a',l:T.fCtDays,req:1,max:3,pat:/^[1-9]\d{0,2}$/,dflt:String(r.alertDays)}];vm.blocks=[kvB(T.ctrT,T.ctrNote,[[T.fCtDays,f(r.alertDays)]])];
    if(adm)vm.blocks.push(fmB(T.ctrSave,sp,[{l:T.ctrSave,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;const n=Number(v.a!==undefined?v.a:r.alertDays);if(n<1||n>365){this.setState({fe:{a:T.pat}});return}await this.act('SystemSettings_UpdateContracts',{alertDays:n,concurrencyStamp:r.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}]))}}
  else if(s.sub==='rem'){vm.title=T.remT;vm.sub=T.remSub;vm.tools=[{l:T.back,go:()=>this.nav2('hub')}];const r=d.rem;
   if(r){const num=/^[1-9]\d{0,2}$/,sp=[{k:'o',l:T.fRemOpen,req:1,max:5,pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/,dflt:r.monthEndOpenTime},{k:'s',l:T.fRemSub,req:1,max:3,pat:num,dflt:String(r.monthlySubmissionWorkingDays)},{k:'i',l:T.fRemInt,req:1,max:3,pat:num,dflt:String(r.monthlyReminderIntervalWorkingDays)},{k:'e',l:T.fRemEsc,req:1,max:3,pat:/^([2-9]|[1-9]\d{1,2})$/,dflt:String(r.monthlyEscalationWorkingDay)},{k:'a',l:T.fRemAppr,req:1,max:3,pat:num,dflt:r.approvalReminderIntervalWorkingDays!=null?String(r.approvalReminderIntervalWorkingDays):'2'},{k:'m',l:T.fRemMin,req:1,max:3,pat:num,dflt:r.evaluationMinWorkedDays!=null?String(r.evaluationMinWorkedDays):'15'}];
    const kv=kvB(T.remT,T.remNote,[[T.fRemOpen,r.monthEndOpenTime],[T.fRemSub,f(r.monthlySubmissionWorkingDays)],[T.fRemInt,f(r.monthlyReminderIntervalWorkingDays)],[T.fRemEsc,f(r.monthlyEscalationWorkingDay)],[T.fRemAppr,r.approvalReminderIntervalWorkingDays!=null?f(r.approvalReminderIntervalWorkingDays):'—'],[T.fRemMin,r.evaluationMinWorkedDays!=null?f(r.evaluationMinWorkedDays):'—']]);
    vm.blocks=[kv];
    if(adm)vm.blocks.push(fmB(T.remSave,sp,[{l:T.remSave,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv,g=(k,d0)=>Number(v[k]!==undefined?v[k]:d0);await this.act('SystemSettings_UpdateReminders',{monthEndOpenTime:v.o!==undefined?v.o:r.monthEndOpenTime,monthlySubmissionWorkingDays:g('s',r.monthlySubmissionWorkingDays),monthlyReminderIntervalWorkingDays:g('i',r.monthlyReminderIntervalWorkingDays),monthlyEscalationWorkingDay:g('e',r.monthlyEscalationWorkingDay),approvalReminderIntervalWorkingDays:g('a',r.approvalReminderIntervalWorkingDays!=null?r.approvalReminderIntervalWorkingDays:2),evaluationMinWorkedDays:g('m',r.evaluationMinWorkedDays!=null?r.evaluationMinWorkedDays:15),concurrencyStamp:r.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}]));else{const ro=kvB(T.remRo,'',[]);ro.hasKv=false;vm.blocks.push(ro)}}}
  else if(s.sub==='roles'){vm.title=T.rT;vm.tools=[{l:T.back,go:()=>this.nav2('hub')}];if(adm)vm.tools.push({l:T.rNew,cls:'p',go:()=>this.dlgOpen({title:T.rNew,fields:[{k:'name',l:T.lKey,req:1,pat:/^[a-z][a-zA-Z0-9]{1,63}$/,max:64,h:T.hKey},{k:'nameAr',l:T.lNameAr,req:1,max:128},{k:'nameEn',l:T.lNameEn,req:1,max:128}],okLabel:T.finish,run:async(v)=>{await this.call('Role_Create',{name:v.name,nameAr:v.nameAr,nameEn:v.nameEn});return{m:T.tCreated}}})});
   const l=d.list||{items:[],totalCount:0};pgr('l',d.list);
   vm.blocks=[tbB('',col(T.lKey,T.lNameAr,T.lNameEn,T.cStatic,T.cUsers,''),l.items.map(r=>row([r.name,r.nameAr,r.nameEn,T.stat[String(r.isStatic)],f(r.userCount)],[ac(adm?T.edit:T.open,()=>this.nav2('role',r.id)),...(adm&&!r.isStatic?[ac(T.del,delc('','Role_Delete',{id:r.id}))]:[])])),T.emR)]}
  else{const r=d.one;vm.title=r?r.displayName:'…';vm.sub=T.rE;vm.tools=[{l:T.back,go:()=>this.nav2('roles')}];
   if(r&&adm){const sp=[{k:'nameAr',l:T.lNameAr,req:1,max:128,dflt:r.nameAr},{k:'nameEn',l:T.lNameEn,req:1,max:128,dflt:r.nameEn}];vm.blocks=[fmB(T.rE,sp,[{l:T.save2,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;await this.act('Role_Update',{id:r.id,nameAr:v.nameAr!==undefined?v.nameAr:r.nameAr,nameEn:v.nameEn!==undefined?v.nameEn:r.nameEn,concurrencyStamp:r.concurrencyStamp},T.tSaved)}}])]}
   if(d.perm)vm.blocks=[...vm.blocks,...this.permB(T,d.perm,'Role_UpdatePermissions',s.sid,fmB)]}}
 if(p==='perf'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tEvMy','e'],['sum','tEvSum','e'],['team','tEvTeam','d'],['rep','tEvRep','d'],['tsum','tEvTSum','d'],['hr','tEvHr','h'],['hrep','tEvHRep','h'],['hsum','tEvHSum','h'],['cyc','tEvCyc','v'],['crit','tEvCrit','m']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.evT;vm.sub=T.evSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const kpiB=(t,sb,k,pw)=>({hasT:!!t,t,s:sb||'',hasKpi:true,kpis:k,hasPw:pw!=null,pw:(pw||0)+'%',hasKv:false,kv:[],hasTbl:false,hasForm:false,hasActs:false,fields:[],acts:[],cols:[],rows:[],empty:false,emptyTx:''});
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const per=(x)=>x.year+'/'+x.month;
 const cl=(d.cl?d.cl.items:[]).map(c=>({v:c.id,l:per(c)+' · '+T.evcy[c.status]}));
 const stO=Object.keys(T.evst).map(x=>({v:x,l:T.evst[x]})),obO=Object.keys(T.evob).map(x=>({v:x,l:T.evob[x]}));
 const cyBlock=()=>{const c=d.cy;if(!c)return[];const m=c.managerProgress;const kp=m?[{l:T.evExp,v:f(m.expectedCount),u:'',c:'#3B82F6'},{l:T.evSubm,v:f(m.submittedCount),u:'',c:'#10B981'},{l:T.evRem,v:f(m.remainingCount),u:'',c:m.remainingCount?'#F59E0B':'#10B981'},{l:T.evDone,v:m.isCompleted?T.yes:T.no,u:m.isLate?T.evLate:'',c:m.isLate?'#EF4444':'#10B981'}]:[];
  const b=kpiB(T.evCycle+' '+per(c)+' · '+T.evcy[c.status],T.nEvDemo,kp,m&&m.expectedCount?Math.round(m.submittedCount/m.expectedCount*100):null);if(!kp.length)b.hasKpi=false;
  return[b,kvB('',T.nEvDone,[[T.evOpens,tm(c.opensAt)],[T.evFreeze,tm(c.dataFreezeAt)],[T.evDue,tm(c.dueAt)],[T.evEsc,tm(c.escalatesAt)]])]};
 const attKpi=(a)=>kpiB(T.evFrozen+' · '+dt(a.periodFrom)+' → '+dt(a.periodTo),'',[{l:T.evSched,v:f(a.scheduledWorkingDays),u:'',c:'#3B82F6'},{l:T.evPres,v:f(a.presentDays),u:'',c:'#10B981'},{l:T.evAbs,v:f(a.absentDays),u:'',c:'#EF4444'},{l:T.evLateC,v:f(a.lateCount),u:'',c:'#F59E0B'},{l:T.evLateM,v:f(a.lateMinutes),u:'',c:'#F59E0B'},{l:T.evLeaveD,v:f(a.leaveDays),u:'',c:'#3B82F6'}],null);
 const respTbl=(rs)=>tbB(T.aEvCmt+' / '+T.aEvObj,col(T.evTime,T.evCrit,T.evContent,T.evDec,T.evDecR),rs.map(r=>row([tm(r.creationTime),T.evrt[r.type],r.content,r.decision?T.evdc[r.decision]:'—',r.decisionReason||'—'])),T.emEvC);
 const scoreTbl=(sc)=>tbB(T.evCrit,col(T.evCrit,T.evGrade),sc.map(z=>row([z.criterionName,z.gradeName||'—'])),T.emEv);
 const sumBlocks=(sm)=>sm?[kpiB(sm.employee.fullName+' · '+sm.year,T.nEvAnnual,[{l:T.evMonths,v:f(sm.evaluatedMonthsCount),u:'',c:'#3B82F6'},{l:T.evAcked,v:f(sm.acknowledgedCount),u:'',c:'#10B981'},{l:T.evObjs,v:f(sm.objectionsCount),u:'',c:'#F59E0B'}],null),tbB('',col(T.evCrit,T.evGrade+' × '+T.evCnt),sm.criteria.map(c=>row([c.criterionName,c.gradeCounts.map(g=>g.gradeName+' ×'+g.count).join('، ')])),T.emEvS)]:[];
 const yrF=()=>{const z=flt('sy',T.fEvYear,'tx');z.v=s.flt.sy||String(new Date().getFullYear());return z};
 const rowsRep=(items)=>items.map(r=>row([per(r),r.manager.displayName,r.department?r.department.name:'—',T.evrst[r.status],f(r.completion.evaluationsSubmitted)+'/'+f(r.completion.evaluationsExpected),r.completion.isCompleted?T.evDone:(r.completion.isLate?T.evLate:'—')],[ac(T.aEvOpen,()=>this.toSub('rpt',r.id))]));
 const repCols=()=>col(T.evPeriod,T.evMgr,T.evDept,T.evSt,T.evCompl,'',''); 
 if(s.sub==='mine'){const o=d.one;vm.tools=[back];vm.title=T.evT;
  if(o){vm.sub=per(o)+' · '+T.evst[o.status];if(o.status==='submitted')vm.tools.push({l:T.aEvAck,cls:'p',go:()=>this.act('Evaluation_Acknowledge',{id:o.id},T.tEvAck,()=>this.enter())});vm.tools.push({l:T.aEvCmt,go:()=>this.dlgOpen(this.evRespDlg(T,o.id,'comment'))});if(o.objectionStatus!=='pending')vm.tools.push({l:T.aEvObj,go:()=>this.dlgOpen(this.evRespDlg(T,o.id,'objection'))});
   vm.blocks=[kvB(per(o),'',[[T.evEval,o.evaluator.displayName],[T.evSt,T.evst[o.status]],[T.evOb,T.evob[o.objectionStatus]],[T.evNotes,o.employeeNotes||'—'],[T.evSubAt,tm(o.submittedAt)],[T.evAckAt,o.acknowledgedAt?tm(o.acknowledgedAt):'—']]),scoreTbl(o.scores),respTbl(o.responses),note(T.nEvInternal)];vm.blocks[3].t=''}}
 else if(s.sub==='form'){const o=d.one;vm.tools=[back];
  if(o){vm.sub=o.employee.fullName+' · '+per(o)+' · '+T.evst[o.status];
   const gO=o.availableGrades.map(g=>({v:g.code,l:g.name}));
   const sp=[...o.scores.map(z=>({k:'sc_'+z.criterionId,l:z.criterionName,t:'sel',opts:gO,dflt:z.gradeCode||''})),{k:'en',l:T.fEvEmpNotes,t:'ta',max:4000,dflt:o.employeeNotes||''}];
   const saved=async()=>{const v=this.state.fv;const sc=o.scores.map(z=>({criterionId:z.criterionId,gradeCode:v['sc_'+z.criterionId]!==undefined?v['sc_'+z.criterionId]:(z.gradeCode||'')})).filter(z=>z.gradeCode);return this.call('Evaluation_SaveDraft',{id:o.id,scores:sc,employeeNotes:v.en!==undefined?v.en:(o.employeeNotes||''),concurrencyStamp:o.concurrencyStamp})};
   const dn=((d.notes?d.notes.items:[]).find(n=>n.isDraft))||null;
   vm.blocks=[attKpi(o.attendance)];
   if(o.isEditable){vm.blocks.push(fmB(T.evCrit,sp,[{l:T.aEvSave,go:async()=>{try{await saved();this.toast(T.tEvDraft);this.enter()}catch(er){this.setState({derr:(er&&er.code)||'gen'})}}},{l:T.aEvSubmit,cls:'p',go:async()=>{this.setState({busy:true,derr:''});try{const r=await saved();await this.call('Evaluation_Submit',{id:o.id,concurrencyStamp:r.concurrencyStamp});this.setState({busy:false});this.toast(T.tEvSubm);this.enter()}catch(er){this.setState({busy:false,derr:(er&&er.code)||'gen'})}}}]));
    const nf=[{k:'dn',l:T.fEvInternalDraft,t:'ta',max:4000,dflt:dn?dn.content:''}];vm.blocks.push(fmB(T.fEvInternal,nf,[{l:T.aEvSaveN,go:async()=>{const v=this.state.fv;await this.act('EvaluationInternalNote_SaveDraft',{id:o.id,content:v.dn!==undefined?v.dn:(dn?dn.content:'')},T.tEvDraft,()=>{this.setState({fv:{}});this.enter()})}}]));vm.blocks.push(note(T.nEvNew))}
   else{vm.tools.push({l:T.aEvNote,cls:'p',go:()=>this.dlgOpen(this.evNoteDlg(T,o.id))});vm.blocks.push(scoreTbl(o.scores),kvB('',T.nEvInternal,[[T.evNotes,o.employeeNotes||'—'],[T.evOb,T.evob[o.objectionStatus]],[T.evSubAt,o.submittedAt?tm(o.submittedAt):'—'],[T.evAckAt,o.acknowledgedAt?tm(o.acknowledgedAt):'—']]),respTbl(o.responses))}
   vm.blocks.push(tbB(T.fEvInternal,col(T.evTime,T.evEval,T.evContent,T.evSt),(d.notes?d.notes.items:[]).map(n=>row([tm(n.creationTime),n.author.displayName,n.content,n.isDraft?T.evst.draft:T.evst.submitted])),T.emEvN))}}
 else if(s.sub==='hrv'){const o=d.one;vm.tools=[back];
  if(o){vm.sub=o.employee.fullName+' · '+per(o);const pend=o.responses.find(r=>r.type==='objection'&&!r.decision);if(pend)vm.tools.push({l:T.aEvDecide,cls:'p',go:()=>this.dlgOpen(this.evDecDlg(T,o,pend))});
   vm.blocks=[attKpi(o.attendance),kvB(per(o),'',[[T.evEval,o.evaluator.displayName],[T.evSt,T.evst[o.status]],[T.evOb,T.evob[o.objectionStatus]],[T.evNotes,o.employeeNotes||'—'],[T.evSubAt,tm(o.submittedAt)],[T.evAckAt,o.acknowledgedAt?tm(o.acknowledgedAt):'—']]),scoreTbl(o.scores),respTbl(o.responses),tbB(T.fEvInternal,col(T.evTime,T.evEval,T.evContent),(d.notes?d.notes.items:[]).map(n=>row([tm(n.creationTime),n.author.displayName,n.content])),T.emEvN),note(T.nEvInternal)]}}
 else if(s.sub==='rpt'){const o=d.one;vm.tools=[back];
  if(o){vm.sub=per(o)+' · '+o.manager.displayName;const sn=o.snapshot,ev=sn.evaluations,mine=hasL('d'),draft=o.status==='draft';const n2=x=>f(Math.round((x||0)*100)/100);
   vm.blocks=[kpiB(T.evCompl+' · '+T.evrst[o.status],T.nEvDone,[{l:T.evExp,v:f(ev.expectedCount),u:'',c:'#3B82F6'},{l:T.evSubm,v:f(ev.submittedCount),u:'',c:'#10B981'},{l:T.evAcked,v:f(ev.acknowledgedCount),u:'',c:'#10B981'},{l:T.evOb,v:f(ev.pendingObjectionsCount),u:'',c:'#F59E0B'}],null),
    tbB(T.evEmp,col(T.evEmp,T.evPres,T.evAbs,T.evLateC,T.evLateM,T.evLeaveD,T.evPend,T.evSt),sn.employees.map(z=>row([z.employee.fullName,f(z.presentDays),f(z.absentDays),f(z.lateCount),f(z.lateMinutes),n2(z.leaveDays),f(z.pendingRequestsCount),z.evaluationStatus?T.evst[z.evaluationStatus]:'—'])),T.emEv),
    tbB(T.evGrade,col(T.evGrade,T.evCnt),ev.gradeCounts.map(g=>row([g.gradeName,f(g.count)])),T.emEv),note(T.nEvRep)];
   if(mine&&draft){const sp=[{k:'rn',l:T.fEvRNotes,t:'ta',max:4000,dflt:o.notes||''}];vm.blocks.push(fmB(T.fEvRNotes,sp,[{l:T.aEvRSave,go:async()=>{const v=this.state.fv;await this.act('DepartmentReport_UpdateNotes',{id:o.id,notes:v.rn!==undefined?v.rn:(o.notes||''),concurrencyStamp:o.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}]));vm.tools.push({l:T.aEvRSubmit,cls:'p',go:()=>this.act('DepartmentReport_Submit',{id:o.id,concurrencyStamp:o.concurrencyStamp},T.tEvRSub,()=>this.enter())})}
   else vm.blocks.push(kvB(T.fEvRNotes,'',[[T.fEvRNotes,o.notes||'—']]))}}
 else if(at==='my'){pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.evPeriod,T.evSt,T.evOb,T.evSubAt,T.evAckAt,''),l.items.map(x=>row([per(x),T.evst[x.status],T.evob[x.objectionStatus],tm(x.submittedAt),x.acknowledgedAt?tm(x.acknowledgedAt):'—'],[ac(T.aEvOpen,()=>this.toSub('mine',x.id))])),T.emEv),note(T.nEvInternal)]}
 else if(at==='sum'){vm.hasFlt=true;vm.flt=[yrF()];vm.blocks=sumBlocks(d.sum)}
 else if(at==='team'){vm.hasFlt=true;vm.flt=[flt('ec',T.evCycle,'sel',cl),flt('es',T.evSt,'sel',stO),flt('ee',T.evEmp,'sel',(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[...cyBlock(),tbB('',col(T.evPeriod,T.evEmp,T.evSt,T.evOb,T.evSubAt,''),l.items.map(x=>row([per(x),x.employee.fullName,T.evst[x.status],T.evob[x.objectionStatus],x.submittedAt?tm(x.submittedAt):'—'],[ac(T.aEvOpen,()=>this.toSub('form',x.id))])),T.emEv)]}
 else if(at==='rep'||at==='hrep'){vm.hasFlt=true;vm.flt=[flt('ec',T.evCycle,'sel',cl),flt('rs',T.evSt,'sel',Object.keys(T.evrst).map(x=>({v:x,l:T.evrst[x]})))];if(at==='hrep')vm.flt.push(flt('el',T.evLate,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}]));pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[...cyBlock(),tbB('',col(T.evPeriod,T.evMgr,T.evDept,T.evSt,T.evCompl,T.evDone,''),l.items.map(r=>row([per(r),r.manager.displayName,r.department?r.department.name:'—',T.evrst[r.status],f(r.completion.evaluationsSubmitted)+'/'+f(r.completion.evaluationsExpected),r.completion.isCompleted?T.evDone:(r.completion.isLate?T.evLate:'—')],[ac(T.aEvOpen,()=>this.toSub('rpt',r.id))])),T.emEvR),note(T.nEvRep)]}
 else if(at==='tsum'||at==='hsum'){vm.hasFlt=true;const op=(at==='tsum'?(d.tm?d.tm.items:[]):(d.lk?d.lk.items:[])).map(x=>({v:x.id,l:x.fullName}));vm.flt=[flt('se',T.evEmp,'sel',op),yrF()];vm.blocks=s.flt.se?sumBlocks(d.sum):[note(T.nEvPick)]}
 else if(at==='hr'){vm.hasFlt=true;vm.flt=[flt('ec',T.evCycle,'sel',cl),flt('es',T.evSt,'sel',stO),flt('eo',T.evOb,'sel',obO),flt('ed',T.evDept,'sel',(d.dp?d.dp.items:[]).map(x=>({v:x.id,l:x.name}))),flt('ee',T.evEmp,'sel',(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName}))),flt('el',T.evLate,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}])];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.evPeriod,T.evEmp,T.evEval,T.evSt,T.evOb,T.evLate,''),l.items.map(x=>row([per(x),x.employee.fullName,x.evaluator.displayName,T.evst[x.status],T.evob[x.objectionStatus],x.isLate?T.yes:T.no],x.status==='draft'?[]:[ac(T.aEvOpen,()=>this.toSub('hrv',x.id))])),T.emEv),note(T.nEvInternal)]}
 else if(at==='cyc'){const l=d.cl||{items:[]};vm.blocks=[...cyBlock(),tbB('',col(T.evPeriod,T.evSt,T.evOpens,T.evDue,T.evEsc),l.items.map(c=>row([per(c),T.evcy[c.status],tm(c.opensAt),tm(c.dueAt),tm(c.escalatesAt)])),T.emEv)]}
 else if(at==='crit'){const kr=d.cr||{items:[]},gr=d.gr||{items:[]};vm.tools=[{l:T.aEvNewC,cls:'p',go:()=>this.dlgOpen(this.evCritDlg(T,null))},{l:T.aEvNewG,go:()=>this.dlgOpen(this.evGradeDlg(T,null))}];
  vm.blocks=[tbB(T.evCrit,col(T.fEvOrder,T.fEvNameAr,T.fEvNameEn,T.fEvActive,''),kr.items.map(c=>row([f(c.sortOrder),c.nameAr,c.nameEn,c.isActive?T.yes:T.no],[ac(T.aEvEdit,()=>this.dlgOpen(this.evCritDlg(T,c)))])),T.emEvK),tbB(T.evGrade,col(T.fEvOrder,T.fEvCode,T.fEvNameAr,T.fEvNameEn,T.fEvActive,''),gr.items.map(g=>row([f(g.sortOrder),g.code,g.nameAr,g.nameEn,g.isActive?T.yes:T.no],[ac(T.aEvEdit,()=>this.dlgOpen(this.evGradeDlg(T,g)))])),T.emEvG),note(T.dEvCritB)]}
 if(!TB.length)vm.denied=true;
}
if(p==='ann'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['recv','tCrRecv','edhxvasm'],['sent','tCrSent','dh']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:'recv';
 vm.title=T.crT;vm.sub=T.crSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const canSend=hasL('d')||hasL('h')||hasL('x');
 const scopes=[...(hasL('d')?['team']:[]),...((hasL('h')||hasL('x'))?['departmentManagers','company']:[])];
 const ackF=()=>flt('ra',T.crcSt,'sel',[{v:'true',l:T.crAcked},{v:'false',l:T.crNeed}]);
 if(canSend&&s.sub==='list')vm.tools.push({l:T.aCrNew,cls:'p',go:()=>this.dlgOpen({title:T.dCrNew,body:T.dCrNewB,fields:[{k:'sc',l:T.fCrScope,t:'sel',req:1,opts:scopes.map(x=>({v:x,l:T.crs[x]}))},{k:'t',l:T.fCrTitle,req:1,max:200},{k:'c',l:T.fCrContent,t:'ta',req:1,max:8000}],okLabel:T.aCrSend,run:async(v)=>{const r=await this.call('Circular_Create',{scope:v.sc,title:v.t,content:v.c});return{m:T.tCrDone+' · '+f(r.recipientsCount)}}})});
 if(s.sub==='one'){const c=d.one;vm.tools=[back];
  if(c){vm.sub=c.title;if(!c.acknowledgedAt)vm.tools.push({l:T.aCrAck,cls:'p',go:()=>this.act('Circular_Acknowledge',{id:c.id},T.tCrAck,()=>this.enter())});
   vm.blocks=[kvB(c.title,'',[[T.crcSender,c.sender.displayName],[T.crcScope,T.crs[c.scope]],[T.crcDate,tm(c.sentAt)],[T.crcSt,c.acknowledgedAt?T.crAcked+' · '+tm(c.acknowledgedAt):T.crNeed],[T.crcContent,c.content]])]}}
 else if(s.sub==='rcpt'){const c=(d.sent?d.sent.items:[]).find(z=>z.id===s.sid);vm.tools=[back];vm.hasFlt=true;vm.flt=[ackF()];pgr('l',d.list);const l=d.list||{items:[]};
  vm.sub=c?c.title:'';vm.blocks=[...(c?[kvB(c.title,T.nCrRcpt,[[T.crcScope,T.crs[c.scope]],[T.crcDate,tm(c.sentAt)],[T.crcRcp,f(c.recipientsCount)],[T.crcAck,f(c.acknowledgedCount)]])]:[]),tbB('',col(T.crcRec,T.crcSt,T.crcAckAt),l.items.map(r=>row([r.recipient.fullName,r.acknowledgedAt?T.crAcked:T.crNeed,r.acknowledgedAt?tm(r.acknowledgedAt):'—'])),T.emCrR)]}
 else if(at==='recv'){vm.hasFlt=true;vm.flt=[ackF()];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.crcDate,T.crcTitle,T.crcSender,T.crcScope,T.crcSt,''),l.items.map(c=>row([tm(c.sentAt),c.title,c.sender.displayName,T.crs[c.scope],c.acknowledgedAt?T.crAcked:T.crNeed],[ac(T.aCrOpen,()=>this.toSub('one',c.id)),...(c.acknowledgedAt?[]:[ac(T.aCrAck,()=>this.act('Circular_Acknowledge',{id:c.id},T.tCrAck,()=>this.enter()))])])),T.emCr)];
  if(hasL('x')&&!hasL('h'))vm.blocks.push(note(T.nCrExec))}
 else if(at==='sent'){pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.crcDate,T.crcTitle,T.crcScope,T.crcRcp,T.crcAck,''),l.items.map(c=>row([tm(c.sentAt),c.title,T.crs[c.scope],f(c.recipientsCount),f(c.acknowledgedCount)],[ac(T.aCrRcpt,()=>this.toSub('rcpt',c.id))])),T.emCr),note(T.nCrRcpt)]}
}
if(p==='inv'){
 const at=['items','moves','issues'].includes(s.tab)?s.tab:'items';
 vm.title=T.svT;vm.sub=T.svSub;vm.hasTabs=true;
 vm.tabs=[['items','tSvItems'],['moves','tSvMoves'],['issues','tSvIssues']].map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const n2=x=>f(Math.round((x||0)*100)/100);
 const mkR=(a,b)=>{const x=[flt(a,T.fFrom,'tx'),flt(b,T.fTo,'tx')];x.forEach(z=>{z.isTx=false;z.isDt=true});return x};
 const its=d.it?d.it.items:[],pur=d.pur?d.pur.items:[];
 vm.tools.push({l:T.aSvRecv,cls:'p',go:()=>this.dlgOpen(this.svRecvDlg(T,its,pur))},{l:T.aSvAdj,go:()=>this.dlgOpen(this.svAdjDlg(T,its,d.mv?d.mv.items:[]))});
 if(at==='items'){vm.tools.push({l:T.aSvNew,go:()=>this.dlgOpen(this.svItemDlg(T,null))});vm.hasFlt=true;vm.flt=[flt('vk',T.svcKind,'sel',Object.keys(T.sk).map(x=>({v:x,l:T.sk[x]}))),flt('va',T.svcAct,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}]),flt('vb',T.svcBelow,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}]),flt('vn',T.svcName,'tx')];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.svcName,T.svcKind,T.svcMin,T.svcQty,T.svcBelow,T.svcAct,''),l.items.map(i=>row([i.nameAr+' / '+i.nameEn,T.sk[i.kind],n2(i.minQuantity),n2(i.quantityOnHand),i.isBelowMinimum?T.yes:T.no,i.isActive?T.yes:T.no],[ac(T.aSvEdit,()=>this.dlgOpen(this.svItemDlg(T,i))),ac(T.aSvRecv,()=>this.dlgOpen(this.svRecvDlg(T,its,pur,i.id)))])),T.emSvI),note(T.dSvItemB)]}
 else if(at==='moves'){vm.hasFlt=true;vm.flt=[flt('vi',T.svcName,'sel',its.map(z=>({v:z.id,l:z.name}))),flt('vt',T.svcType,'sel',Object.keys(T.mvt).map(x=>({v:x,l:T.mvt[x]}))),...mkR('vf','vto')];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.svcAt,T.svcName,T.svcType,T.svcQ,T.svcAfter,T.svcEmp,T.svcBy,T.svcNote),l.items.map(m=>row([tm(m.creationTime),m.item.name,T.mvt[m.type],n2(m.quantity),n2(m.balanceAfter),m.employee?m.employee.fullName:'—',m.performedBy.displayName,m.note||'—'])),T.emSvM),note(T.nSvMoves)]}
 else if(at==='issues'){vm.hasFlt=true;vm.flt=[flt('vs',T.cucSt,'sel',Object.keys(T.sist).map(x=>({v:x,l:T.sist[x]}))),flt('vi',T.svcName,'sel',its.map(z=>({v:z.id,l:z.name}))),flt('ve',T.svcEmp,'sel',(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.svcAt,T.svcEmp,T.svcName,T.svcQ,T.svcQty,T.svcAvail,T.cucSt,''),l.items.map(i=>row([tm(i.creationTime),i.employee.fullName,i.item.name,n2(i.quantity),n2(i.quantityOnHand),i.isAvailable?T.yes:T.no,i.statusLabel],[
   ...(i.status==='pendingAcknowledgement'?[ac(T.aSvAck,()=>this.act('StockIssue_Acknowledge',{id:i.id},T.tSvAck,()=>this.enter()))]:[]),
   ...(i.status==='acknowledged'?[ac(T.aSvPur,()=>this.dlgOpen(this.svNoteDlg(T,T.dSvPur,'StockIssue_MarkPurchasing',i.id,T.tSvPur)))]:[]),
   ...(['acknowledged','purchasing'].includes(i.status)?[ac(T.aSvDel,()=>this.dlgOpen(this.svDelDlg(T,i)))]:[])])),T.emSvS),note(T.nSvIssues)]}
}
if(p==='cus'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tCuMy','edhxvasm'],['team','tCuTeam','d'],['all','tCuAll','sh'],['count','tCuCount','dsh']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:'my';
 vm.title=T.cuT;vm.sub=T.cuSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const n2=x=>f(Math.round((x||0)*100)/100);
 const holder=(c)=>c.employee?c.employee.fullName:(c.department?c.department.name:'—');
 const stF=()=>flt('cs',T.cucSt,'sel',Object.keys(T.cust).map(x=>({v:x,l:T.cust[x]})));
 const kF=()=>flt('ck',T.cucKind,'sel',Object.keys(T.ckind).map(x=>({v:x,l:T.ckind[x]})));
 const ar2=s.lang==='ar';
 const crow=(c,acts)=>row([c.item.name,T.ckind[c.kind],holder(c),n2(c.quantity),T.cust[c.status],dt(c.assignedAt.slice(0,10))],acts);
 const ackAct=(c)=>c.status==='pendingAcknowledgement'?[ac(T.aCuAck,()=>this.act('Custody_Acknowledge',{id:c.id},T.tCuAckT,()=>this.enter()))]:[];
 const detAct=(c)=>ac(T.aCuDet,()=>this.dlgOpen(this.cuDetDlg(T,c,ar2)));
 if(s.sub==='cnew'){vm.tools=[{l:T.back,go:()=>this.toSub('list',null,{tab:'count',fv:{}})}];vm.title=T.aCuNewC;
  const sh=(d.sh?d.sh.items:[]).filter(c=>c.status!=='returned'&&c.department);const deps=[...new Map(sh.map(c=>[c.department.id,c.department])).values()];
  const depId=s.fv.dep||(deps[0]?deps[0].id:'');const lines=sh.filter(c=>c.department.id===depId);
  const sp=[{k:'dep',l:T.cucDept,t:'sel',req:1,dflt:depId,opts:deps.map(z=>({v:z.id,l:z.name}))},{k:'dt',l:T.fCuDate,t:'date',req:1,dflt:this.todayS()},...lines.map(c=>({k:'ln_'+c.id,l:c.item.name+' · '+T.cucExp+': '+n2(c.quantity),req:1,max:9,pat:/^\d+(\.\d+)?$/,dflt:String(c.quantity)})),{k:'nn',l:T.fCuNote,t:'ta',max:2000}];
  vm.blocks=[fmB(T.dCuCount?T.dCuCount:T.aCuNewC,sp,[{l:T.aCuSave,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;const dep=v.dep!==undefined?v.dep:depId;const ls=sh.filter(c=>c.department.id===dep);await this.act('CustodyCount_Create',{departmentId:dep,countDate:v.dt!==undefined?v.dt:this.todayS(),note:v.nn||undefined,lines:ls.map(c=>({custodyId:c.id,countedQuantity:Number(v['ln_'+c.id]!==undefined?v['ln_'+c.id]:c.quantity)}))},T.tCuCntT,()=>this.toSub('list',null,{tab:'count',fv:{}}))}}]),note(T.nCuCount)]}
 else if(at==='my'){vm.hasFlt=true;vm.flt=[stF()];pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.cucItem,T.cucKind,T.cucHolder,T.cucQty,T.cucSt,T.cucAt,''),l.items.map(c=>crow(c,[...ackAct(c),detAct(c)])),T.emCuC)]}
 else if(at==='team'){vm.hasFlt=true;vm.flt=[kF(),stF(),flt('ce',T.svcEmp,'sel',(d.tm?d.tm.items:[]).map(z=>({v:z.id,l:z.fullName})))];pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.cucItem,T.cucKind,T.cucHolder,T.cucQty,T.cucSt,T.cucAt,''),l.items.map(c=>crow(c,[...(c.kind==='shared'?ackAct(c):[]),detAct(c)])),T.emCuC),note(T.nCuTeam)]}
 else if(at==='all'){const its=d.ix?d.ix.items:[];vm.hasFlt=true;vm.flt=[kF(),stF(),flt('ce',T.svcEmp,'sel',(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName}))),flt('cd',T.cucDept,'sel',(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name}))),flt('ci',T.cucItem,'sel',(d.it?d.it.items:[]).map(z=>({v:z.id,l:z.name})))];pgr('l',d.list);const l=d.list||{items:[]};
  if(hasL('s'))vm.tools.push({l:T.aCuNew,cls:'p',go:()=>this.dlgOpen(this.cuNewDlg(T,its,(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName})),(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name}))))});
  vm.blocks=[tbB('',col(T.cucItem,T.cucKind,T.cucHolder,T.cucQty,T.cucSt,T.cucAt,''),l.items.map(c=>crow(c,[detAct(c),...(hasL('s')&&c.status!=='returned'?[ac(T.aCuRet,()=>this.dlgOpen(this.cuRetDlg(T,c)))]:[])])),T.emCuC),note(T.nCuAll)]}
 else if(at==='count'){vm.hasFlt=true;vm.flt=[flt('cd',T.cucDept,'sel',(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name}))),flt('cr',T.cucRes,'sel',Object.keys(T.cres).map(x=>({v:x,l:T.cres[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  if(hasL('d'))vm.tools.push({l:T.aCuNewC,cls:'p',go:()=>{this.setState({sub:'cnew',fv:{},data:{},derr:''});this.later()}});
  vm.blocks=[tbB('',col(T.cucDate,T.cucDept,T.cucRes,T.cucCby,T.cucNote,T.cucExp+' / '+T.cucCnt),l.items.map(c=>row([dt(c.countDate),c.department.name,T.cres[c.result],c.countedBy.displayName,c.note||'—',c.lines.map(z=>z.item.name+': '+n2(z.expectedQuantity)+'/'+n2(z.countedQuantity)).join('، ')])),T.emCuN),note(T.nCuCount)]}
 if(!TB.length)vm.denied=true;
}
if(p==='con'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['exp','tCtExp','h'],['all','tCtAll','h'],['my','tCtMy','e']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.ctT;vm.sub=T.ctSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const emps=(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName})),rens=d.ren?d.ren.items:[];
 const kpiB=(t,sb,k)=>({hasT:!!t,t,s:sb||'',hasKpi:true,kpis:k,hasPw:false,pw:'0%',hasKv:false,kv:[],hasTbl:false,hasForm:false,hasActs:false,fields:[],acts:[],cols:[],rows:[],empty:false,emptyTx:''});
 const dayTx=(c)=>c.isExpired?T.ctcExp+' ('+f(Math.abs(c.daysRemaining))+')':f(c.daysRemaining);
 const crow=(c)=>row([c.employee.fullName,c.contractType,dt(c.startDate),dt(c.endDate),dayTx(c),T.cst[c.status]+(c.isDecisionOverdue?' · '+T.ctcOver:''),c.decisionDueDate?dt(c.decisionDueDate):'—'],[ac(T.aCtOpen,()=>this.toSub('one',c.id))]);
 const cols=()=>col(T.ctcEmp,T.ctcType,T.ctcStart,T.ctcEnd,T.ctcDays,T.ctcSt,T.ctcDue,'');
 if(hasL('h')&&s.sub==='list')vm.tools.push({l:T.aCtNew,cls:'p',go:()=>this.dlgOpen(this.ctNewDlg(T,emps,rens,null))});
 if(s.sub==='one'){const c=d.one;vm.tools=[back];
  if(c){vm.sub=c.employee.fullName;if(c.status==='pendingRecommendation')vm.tools.push({l:T.aCtRec,cls:'p',go:()=>this.dlgOpen(this.ctRecDlg(T,c))});if(c.status==='pendingDecision')vm.tools.push({l:T.aCtDec,cls:'p',go:()=>this.dlgOpen(this.ctDecDlg(T,c))});if(c.status==='renewed')vm.tools.push({l:T.aCtRenew,go:()=>this.dlgOpen(this.ctNewDlg(T,emps,rens,c))});
   vm.blocks=[kvB(c.contractType,T.nCtFlow,[[T.ctcEmp,c.employee.fullName],[T.ctcSt,T.cst[c.status]+(c.isDecisionOverdue?' · '+T.ctcOver:'')],[T.ctcStart,dt(c.startDate)],[T.ctcEnd,dt(c.endDate)+' · '+dayTx(c)],[T.ctcPrev,c.previousContractId?'✓':'—'],[T.ctcDue,c.decisionDueDate?dt(c.decisionDueDate):'—'],[T.ctcNot,c.employeeNotifiedAt?tm(c.employeeNotifiedAt):'—']]),
    kvB(T.dCtRec,'',c.recommendation?[[T.ctcPos,c.recommendation.employeePosition],[T.ctcRecm,T.copt[c.recommendation.recommendation]],[T.ctcNote,c.recommendation.note||'—'],[T.ctcBy,c.recommendation.recommendedBy.displayName+' · '+tm(c.recommendation.recommendedAt)]]:[[T.ctcRecm,'—']]),
    kvB(T.dCtDec,'',c.decision?[[T.ctcDecn,T.copt[c.decision.decision]],[T.ctcNote,c.decision.note||'—'],[T.ctcMsg,c.decision.employeeMessage||'—'],[T.ctcBy,c.decision.decidedBy.displayName+' · '+tm(c.decision.decidedAt)]]:[[T.ctcDecn,'—']])]}}
 else if(s.sub==='mine'){const c=d.one;vm.tools=[back];if(c){vm.sub=c.contractType;vm.blocks=[kvB(c.contractType,T.nCtEmp,[[T.ctcStart,dt(c.startDate)],[T.ctcEnd,dt(c.endDate)],[T.ctcSt,T.est[c.status]+(c.isExpired?' · '+T.ctcExp:'')],[T.ctcMsg,c.employeeMessage||'—'],[T.ctcAt,c.decidedAt?tm(c.decidedAt):'—']])]}}
 else if(at==='exp'){vm.hasFlt=true;const w=flt('cw',T.fCtWithin,'tx');w.v=s.flt.cw||'';vm.flt=[w,flt('cd',T.fRDept,'sel',(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name})))];pgr('l',d.list);const l=d.list||{items:[]};
  const cnt=(st)=>l.items.filter(c=>c.status===st).length;
  vm.blocks=[kpiB(T.tCtExp,T.nCtFlow,[{l:T.fCtDays,v:f(l.alertDays||0),u:'',c:'#3B82F6'},{l:T.cst.pendingRecommendation,v:f(cnt('pendingRecommendation')),u:'',c:'#F59E0B'},{l:T.cst.pendingDecision,v:f(cnt('pendingDecision')),u:'',c:'#F59E0B'},{l:T.ctcOver,v:f(l.items.filter(c=>c.isDecisionOverdue).length),u:'',c:'#EF4444'}]),tbB('',cols(),l.items.map(crow),T.emCt)]}
 else if(at==='all'){vm.hasFlt=true;vm.flt=[flt('cs',T.ctcSt,'sel',Object.keys(T.cst).map(x=>({v:x,l:T.cst[x]}))),flt('ce',T.ctcEmp,'sel',emps),flt('cd',T.fRDept,'sel',(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name})))];pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',cols(),l.items.map(crow),T.emCt)]}
 else if(at==='my'){pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.ctcType,T.ctcStart,T.ctcEnd,T.ctcSt,T.ctcMsg,''),l.items.map(c=>row([c.contractType,dt(c.startDate),dt(c.endDate),T.est[c.status]+(c.isExpired?' · '+T.ctcExp:''),c.employeeMessage||'—'],[ac(T.aCtOpen,()=>this.toSub('mine',c.id))])),T.emCt),note(T.nCtEmp)]}
 if(!TB.length)vm.denied=true;
}
if(p==='wrn'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tWrMy','e'],['rec','tWrRec','dv'],['file','tWrFile','dh'],['hr','tWrHr','h'],['obj','tWrObj','x']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.wrT;vm.sub=T.wrSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const stO=Object.keys(T.wst).map(x=>({v:x,l:T.wst[x]}));
 const resp=(r)=>r?r.content+' · '+tm(r.submittedAt):'—';
 if(s.sub==='mine'){const w=d.one;vm.tools=[back];
  if(w){vm.sub=T.wst[w.status];if(w.canRespond)vm.tools.push({l:T.aWrResp,cls:'p',go:()=>this.dlgOpen(this.wrTextDlg(T,T.dWrResp,T.dWrRespB,'Warning_Respond',w.id,T.tWrResp))});if(w.canObject)vm.tools.push({l:T.aWrObj,go:()=>this.dlgOpen(this.wrTextDlg(T,T.dWrObj,T.dWrObjB,'Warning_Object',w.id,T.tWrObjD))});
   vm.blocks=[kvB(T.wlv[w.level],T.nWrFlow,[[T.wrcDate,dt(w.incidentDate)],[T.wrcDesc,w.description],[T.wrcSt,T.wst[w.status]],[T.wrcNot,tm(w.notifiedAt)],[T.wrcDue,dt(w.responseDueDate)],[T.wrcResp,resp(w.myResponse)],[T.wrcDec,w.decisionNote?w.decisionNote+' · '+tm(w.decidedAt):'—'],[T.wrcObj,w.myObjection?w.myObjection.content+(w.myObjection.outcome?' → '+T.wob[w.myObjection.outcome]+(w.myObjection.resolutionNote?' · '+w.myObjection.resolutionNote:''):' · '+T.fWrPending):'—']])]}}
 else if(s.sub==='one'){const w=d.one;vm.tools=[back];
  if(w){vm.sub=w.employee.fullName+' · '+T.wst[w.status];
   if(hasL('h')&&w.status==='recommended')vm.tools.push({l:T.aWrNotify,cls:'p',go:()=>this.dlgOpen(this.wrNotifyDlg(T,w))});
   if(hasL('h')&&['responded','responseExpired'].includes(w.status))vm.tools.push({l:T.aWrDec,cls:'p',go:()=>this.dlgOpen(this.wrDecDlg(T,w))});
   if(hasL('x')&&w.status==='objected')vm.tools.push({l:T.aWrRes,cls:'p',go:()=>this.dlgOpen(this.wrResDlg(T,w))});
   vm.blocks=[kvB(w.employee.fullName,'',[[T.wrcLvl,T.wlv[w.recommendedLevel]],[T.wrcDate,dt(w.incidentDate)],[T.wrcDesc,w.description],[T.wrcSt,T.wst[w.status]],[T.wrcBy,w.recommendedBy.displayName+' · '+T.wrr[w.recommenderRole]],[T.wrcNot,w.notifiedAt?tm(w.notifiedAt):'—'],[T.wrcDue,w.responseDueDate?dt(w.responseDueDate):'—']]),
    tbB(T.fWrAtt,col(T.wrcFile,T.wrcSize,''),w.attachments.map(a=>row([a.fileName,f(Math.max(1,Math.round(a.sizeBytes/1024)))+' KB'],[ac(T.aWrDl,()=>this.act('Warning_GetAttachmentDownloadUrl',{id:w.id,attachmentId:a.id},T.tWrDl,()=>{}))])),T.emWrA),
    kvB(T.wrcResp,'',[[T.wrcResp,resp(w.employeeResponse)]]),
    kvB(T.wrcDec,'',w.decision?[[T.wrcDec,T.wdo[w.decision.outcome]+(w.decision.issuedLevel?' · '+T.wlv[w.decision.issuedLevel]:'')],[T.wrcNote,w.decision.note],[T.wrcAt,w.decision.decidedBy.displayName+' · '+tm(w.decision.decidedAt)]]:[[T.wrcDec,'—']]),
    kvB(T.wrcObj,'',w.objection?[[T.wrcObj,w.objection.content+' · '+tm(w.objection.submittedAt)],[T.fWrOutcome,w.objection.outcome?T.wob[w.objection.outcome]:T.fWrPending],[T.wrcNote,w.objection.resolutionNote||'—'],[T.wrcAt,w.objection.resolvedBy?w.objection.resolvedBy.displayName+' · '+tm(w.objection.resolvedAt):'—']]:[[T.wrcObj,'—']])]}}
 else if(at==='my'){pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.wrcDate,T.wrcLvl,T.wrcSt,T.wrcDue,''),l.items.map(w=>row([dt(w.incidentDate),T.wlv[w.level],T.wst[w.status],dt(w.responseDueDate)+(w.canRespond?' ✎':'')],[ac(T.aWrOpen,()=>this.toSub('mine',w.id))])),T.emWr),note(T.nWrMy)]}
 else if(at==='rec'){vm.hasFlt=true;vm.flt=[flt('ws',T.wrcStage,'sel',Object.keys(T.wstg).map(x=>({v:x,l:T.wstg[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  const emps=hasL('d')&&!hasL('v')?(d.tm?d.tm.items:[]):(d.lk?d.lk.items:[]);vm.tools.push({l:T.aWrNew,cls:'p',go:()=>this.dlgOpen(this.wrNewDlg(T,emps.map(z=>({v:z.id,l:z.fullName}))))});
  vm.blocks=[tbB('',col(T.wrcEmp,T.wrcLvl,T.wrcDate,T.wrcStage,T.wrcDec),l.items.map(w=>row([w.employee.fullName,T.wlv[w.recommendedLevel],dt(w.incidentDate),T.wstg[w.stage],w.issuedLevel?T.wlv[w.issuedLevel]:'—'])),T.emWr),note(T.nWrRec)]}
 else if(at==='file'){vm.hasFlt=true;const op=(hasL('h')?(d.lk?d.lk.items:[]):(d.tm?d.tm.items:[])).map(z=>({v:z.id,l:z.fullName}));vm.flt=[flt('we',T.wrcEmp,'sel',op)];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=s.flt.we?[tbB('',col(T.wrcDate,T.wrcLvl,T.wrcDesc,T.wrcSt,T.wrcDec,T.wrcResp,T.wrcObj),l.items.map(w=>row([dt(w.incidentDate),T.wlv[w.level],w.description,T.wst[w.status],w.decisionNote+' · '+tm(w.decidedAt),w.employeeResponse?w.employeeResponse.content:'—',w.objectionOutcome?T.wob[w.objectionOutcome]:'—'])),T.emWrF),note(T.nWrFile)]:[note(T.nWrPick)]}
 else if(at==='hr'){vm.hasFlt=true;vm.flt=[flt('ws',T.wrcSt,'sel',stO),flt('wl',T.wrcLvl,'sel',Object.keys(T.wlv).map(x=>({v:x,l:T.wlv[x]}))),flt('we',T.wrcEmp,'sel',(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName}))),flt('wd',T.fRDept,'sel',(d.dp?d.dp.items:[]).map(z=>({v:z.id,l:z.name})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.wrcEmp,T.wrcLvl,T.wrcDate,T.wrcSt,T.wrcBy,T.wrcDue,''),l.items.map(w=>row([w.employee.fullName,T.wlv[w.recommendedLevel],dt(w.incidentDate),T.wst[w.status],w.recommendedBy.displayName,w.responseDueDate?dt(w.responseDueDate):'—'],[ac(T.aWrOpen,()=>this.toSub('one',w.id))])),T.emWr)]}
 else if(at==='obj'){vm.hasFlt=true;vm.flt=[flt('wp',T.wrcObj,'sel',[{v:'true',l:T.fWrPending},{v:'false',l:T.fWrOutcome}])];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.wrcEmp,T.wrcLvl,T.wrcDate,T.wrcSt,T.wrcObj,''),l.items.map(w=>row([w.employee.fullName,T.wlv[w.decision&&w.decision.issuedLevel?w.decision.issuedLevel:w.recommendedLevel],dt(w.incidentDate),T.wst[w.status],w.objection.outcome?T.wob[w.objection.outcome]:T.fWrPending],[ac(T.aWrOpen,()=>this.toSub('one',w.id))])),T.emWr),note(T.nWrExec)]}
 if(!TB.length)vm.denied=true;
}
if(p==='apr'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tApMy','e'],['all','tApAll','hx'],['file','tApFile','dhx']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.apT;vm.sub=T.apSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const ackF=()=>flt('aa',T.apcSt,'sel',[{v:'true',l:T.apAcked},{v:'false',l:T.apNeed}]);
 const emps=(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName}));
 const dl=(l)=>ac(T.aApDl,()=>this.act('AppreciationLetter_GetDownloadUrl',{id:l.id},T.tApDlT,()=>{}));
 if(hasL('x')&&s.sub==='list'&&at==='all')vm.tools.push({l:T.aApNew,cls:'p',go:()=>this.dlgOpen(this.apNewDlg(T,emps))});
 if(s.sub==='one'){const l=d.one;vm.tools=[back];
  if(l){vm.sub=l.title;if(hasL('e')&&!l.isAcknowledged)vm.tools.push({l:T.aApAck,cls:'p',go:()=>this.act('AppreciationLetter_Acknowledge',{id:l.id},T.tApAckT,()=>this.enter())});vm.tools.push({l:T.aApDl,go:()=>this.act('AppreciationLetter_GetDownloadUrl',{id:l.id},T.tApDlT,()=>{})});
   vm.blocks=[kvB(l.title,'',[[T.apcEmp,l.employee.fullName],[T.apcNote,l.note||'—'],[T.apcBy,l.issuedBy.displayName],[T.apcAt,tm(l.issuedAt)],[T.apcFile,l.file.fileName+' · '+f(Math.max(1,Math.round(l.file.sizeBytes/1024)))+' KB'],[T.apcSt,l.isAcknowledged?T.apAcked+' · '+tm(l.acknowledgedAt):T.apNeed]])]}}
 else if(at==='my'){pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.apcAt,T.apcTitle,T.apcBy,T.apcSt,''),l.items.map(x=>row([tm(x.issuedAt),x.title,x.issuedBy.displayName,x.isAcknowledged?T.apAcked:T.apNeed],[ac(T.aApOpen,()=>this.toSub('one',x.id)),...(x.isAcknowledged?[]:[ac(T.aApAck,()=>this.act('AppreciationLetter_Acknowledge',{id:x.id},T.tApAckT,()=>this.enter()))]),dl(x)])),T.emAp),note(T.nApMy)]}
 else if(at==='all'){vm.hasFlt=true;vm.flt=[flt('ae',T.apcEmp,'sel',emps),ackF()];pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.apcAt,T.apcEmp,T.apcTitle,T.apcBy,T.apcSt,''),l.items.map(x=>row([tm(x.issuedAt),x.employee.fullName,x.title,x.issuedBy.displayName,x.isAcknowledged?T.apAcked:T.apNeed],[ac(T.aApOpen,()=>this.toSub('one',x.id)),dl(x)])),T.emAp)]}
 else if(at==='file'){vm.hasFlt=true;const op=((hasL('d')&&!hasL('h')&&!hasL('x'))?(d.tm?d.tm.items:[]):(d.lk?d.lk.items:[])).map(z=>({v:z.id,l:z.fullName}));vm.flt=[flt('ae',T.apcEmp,'sel',op),ackF()];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=s.flt.ae?[tbB('',col(T.apcAt,T.apcTitle,T.apcNote,T.apcBy,T.apcSt,''),l.items.map(x=>row([tm(x.issuedAt),x.title,x.note||'—',x.issuedBy.displayName,x.isAcknowledged?T.apAcked:T.apNeed],[ac(T.aApOpen,()=>this.toSub('one',x.id)),dl(x)])),T.emAp),note(T.nApFile)]:[note(T.nApPick)]}
 if(!TB.length)vm.denied=true;
}
if(p==='fin'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['sal','tFiSal','a'],['adj','tFiAdj','a'],['sug','tFiSug','a'],['exp','tFiExp','a'],['req','tFiReq','a'],['run','tFiRun','ahx'],['rep','tFiRep','a']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'sal');
 vm.title=T.fiT;vm.sub=T.fiSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.back,go:()=>this.toSub('list',null,{tab:at})};
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const money=(n,c)=>this.fnMoney(n,c);
 const emps=(d.lk?d.lk.items:[]).map(z=>({v:z.id,l:z.fullName}));
 const reqs=(d.arq?d.arq.items:[]).map(z=>({v:z.id,l:z.employee.fullName+' · '+z.requestTypeName}));
 const mkR=(a,b)=>{const x=[flt(a,T.fFrom,'tx'),flt(b,T.fTo,'tx')];x.forEach(z=>{z.isTx=false;z.isDt=true});return x};
 const dl=(op,id)=>ac(T.aFiDl,()=>this.act(op,{id},T.tFiDlT,()=>{}));
 if(s.sub==='sal'){vm.tools=[back];const e=(d.list&&d.list.items.find(z=>z.employee.id===s.sid))||null;const pf=d.prof;vm.title=T.tFiSal;
  vm.sub=(pf?pf.employee.fullName:'');const empRef=pf?pf.employee:{id:s.sid,fullName:'—'};
  vm.tools.push({l:T.aFiUpd,cls:'p',go:()=>this.dlgOpen(this.fiUpdDlg(T,empRef,pf,d.hist))});
  vm.blocks=[kvB(pf?pf.employee.fullName:T.ficNone,T.nFiSal,pf?[[T.ficBase,money(pf.baseSalary,pf.currency)],[T.ficFrom,dt(pf.effectiveFrom)]]:[[T.ficProf,T.no]]),
   tbB(T.ficComp,col(T.ficName,T.ficType,T.ficAmt),(pf?pf.components:[]).map(c=>row([c.name,T.fcomp[c.type],money(c.amount,pf.currency)])),T.emFi),
   tbB(T.ficHist,col(T.ficFrom,T.ficBase,T.ficCur,T.ficComp),(d.hist?d.hist.items:[]).map(h=>row([dt(h.effectiveFrom),money(h.baseSalary),h.currency,h.components.map(c=>c.name+' '+money(c.amount)).join('، ')||'—'])),T.emFiH)]}

 else if(s.sub==='rn'){const r=d.one;vm.tools=[back];
  if(r){vm.sub=r.period+' · '+T.pys[r.status];const acc=hasL('a'),ap=hasL('h')||hasL('x');
   const xp=(fm)=>({l:fm==='pdf'?T.aPyPdf:T.aPyXls,go:()=>this.act('PayrollRun_Export',{id:r.id,format:fm},T.tPyXp,()=>{})});
   if(acc&&r.status==='draft'){vm.tools.push({l:T.aPyRec,go:()=>this.dlgOpen(this.pyActDlg(T,r,'PayrollRun_Recalculate',T.dPyRec,T.dPyRecB,T.tPyRec))});if(!r.approvalRequestedAt)vm.tools.push({l:T.aPyReq,cls:'p',go:()=>this.dlgOpen(this.pyActDlg(T,r,'PayrollRun_RequestApproval',T.dPyReq,T.dPyReqB,T.tPyReq))})}
   if(acc&&r.status==='approved')vm.tools.push({l:T.aPyCl,cls:'p',go:()=>this.dlgOpen(this.pyActDlg(T,r,'PayrollRun_Close',T.dPyCl,T.dPyClB,T.tPyCl,true))});
   if(ap&&r.status==='draft'&&r.approvalRequestedAt)vm.tools.push({l:T.aPyApp,cls:'p',go:()=>this.dlgOpen(this.pyActDlg(T,r,'PayrollRun_Approve',T.dPyApp,T.dPyAppB,T.tPyApp,true))},{l:T.aPyRet,go:()=>this.dlgOpen(this.pyRetDlg(T,r))});
   vm.tools.push(xp('pdf'),xp('excel'));
   vm.blocks=[kvB(r.period,r.isStale?T.nPyStale:T.nPyRet,[[T.pycSt,T.pys[r.status]+(r.isStale?' · '+T.pycStale:'')],[T.pycPrep,r.preparedBy.displayName],[T.pycCalc,tm(r.lastCalculatedAt)],[T.pycReq,r.approvalRequestedAt?tm(r.approvalRequestedAt):'—'],[T.pycApp,r.approvedBy?r.approvedBy.displayName+' · '+tm(r.approvedAt):'—'],[T.pycClosed,r.closedAt?tm(r.closedAt):'—'],[T.pycCnt,f(r.employeeCount)],[T.pycPend,f(r.pendingSuggestionCount)],[T.pycNoProf,r.employeesWithoutProfile.map(z=>z.fullName).join('، ')||'—']]),
    tbB(T.pycTot,col(T.pycCur,T.pycGross,T.pycAdd,T.pycDed,T.pycNet),r.totals.map(t=>row([t.currency,money(t.gross),money(t.additions),money(t.deductions),money(t.net)])),T.emPy),
    tbB('',col(T.pycEmp,T.pycGross,T.pycAdd,T.pycDed,T.pycNet,''),r.items.map(i=>row([i.employee.fullName,money(i.gross,i.currency),money(i.additions),money(i.deductions),money(i.net)],[ac(T.aPyLines,()=>this.dlgOpen(this.pyLinesDlg(T,i)))])),T.emPyI)]}} else if(s.sub==='arq'){const a=d.one;vm.tools=[back];
  if(a){vm.sub=a.employee.fullName;if(a.status==='pendingAccountant')vm.tools.push({l:T.aFiAppr,cls:'p',go:()=>this.dlgOpen(this.qDecDlg(T,a.id,true))},{l:T.aFiRejr,go:()=>this.dlgOpen(this.qDecDlg(T,a.id,false))});
   vm.blocks=[kvB(a.requestTypeName,T.nFiReq,[[T.ficEmp,a.employee.fullName],[T.ficBy,a.submittedBy.displayName],[T.ficDesc,a.description||'—'],[T.ficSt,T.farq[a.status]],[T.ficArr,tm(a.arrivedAt)],[T.ficDec,a.decidedAt?tm(a.decidedAt):'—'],[T.ficLinked,f(a.linkedAdjustmentIds.length+a.linkedExpenseIds.length)]]),tbB('',col(T.ficAt,T.ficReason,T.ficBy),a.timeline.map(z=>row([tm(z.occurredAt),z.action,z.actor?z.actor.displayName:'—'])),T.emFiT)]}}
 else if(at==='sal'){vm.hasFlt=true;vm.flt=[flt('fn',T.ficEmp,'tx'),flt('fh',T.ficProf,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}])];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.ficEmp,T.ficProf,T.ficBase,T.ficCur,T.ficFrom,''),l.items.map(r=>row([r.employee.fullName,r.hasProfile?T.yes:T.no,r.baseSalary!=null?money(r.baseSalary):'—',r.currency||'—',r.effectiveFrom?dt(r.effectiveFrom):'—'],[ac(T.aFiOpen,()=>this.toSub('sal',r.employee.id))])),T.emFi),note(T.nFiSal)]}
 else if(at==='adj'){vm.hasFlt=true;vm.flt=[flt('fe',T.ficEmp,'sel',emps),flt('fp',T.ficPer,'tx'),flt('ft',T.ficType,'sel',Object.keys(T.fadt).map(x=>({v:x,l:T.fadt[x]}))),flt('fs',T.ficSrc,'sel',Object.keys(T.fads).map(x=>({v:x,l:T.fads[x]}))),flt('fa',T.ficSt,'sel',Object.keys(T.fadst).map(x=>({v:x,l:T.fadst[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.tools.push({l:T.aFiNewA,cls:'p',go:()=>this.dlgOpen(this.fiAdjDlg(T,emps,reqs))});
  vm.blocks=[tbB('',col(T.ficEmp,T.ficPer,T.ficType,T.ficAmt,T.ficReason,T.ficSrc,T.ficSt,''),l.items.map(a=>row([a.employee.fullName,a.period,T.fadt[a.type],money(a.amount,a.currency),a.reason,T.fads[a.source],T.fadst[a.status]],[...(a.status!=='reversed'&&a.source!=='reversal'?[ac(T.aFiRev,()=>this.dlgOpen(this.fiRevDlg(T,a)))]:[]),...(a.attachment?[dl('Adjustment_GetAttachmentDownloadUrl',a.id)]:[])])),T.emFi),note(T.nFiSal)]}
 else if(at==='sug'){vm.hasFlt=true;vm.flt=[flt('fp',T.ficPer,'tx'),flt('fa',T.ficSt,'sel',Object.keys(T.fsgs).map(x=>({v:x,l:T.fsgs[x]}))),flt('fb',T.ficBasis,'sel',Object.keys(T.fsgb).map(x=>({v:x,l:T.fsgb[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.ficEmp,T.ficPer,T.ficType,T.ficBasis,T.ficQty,T.ficSug,T.ficSet,T.ficSt,''),l.items.map(z=>row([z.employee.fullName,z.period,T.fadt[z.type],T.fsgb[z.basis],f(z.quantity)+' '+T.fsgu[z.quantityUnit],z.suggestedAmount!=null?money(z.suggestedAmount,z.currency):'—',z.settlementForPeriod||'—',T.fsgs[z.status]+(z.reviewNote?' · '+z.reviewNote:'')],z.status==='pendingReview'?[ac(T.aFiApp,()=>this.dlgOpen(this.fiSugDlg(T,z,true))),ac(T.aFiRej,()=>this.dlgOpen(this.fiSugDlg(T,z,false)))]:[])),T.emFi),note(T.nFiSug)]}
 else if(at==='exp'){vm.hasFlt=true;vm.flt=[flt('fs',T.ficSrc,'sel',Object.keys(T.fexs).map(x=>({v:x,l:T.fexs[x]}))),flt('fc',T.ficCat,'tx'),...mkR('fd1','fd2'),flt('fa',T.ficSt,'sel',Object.keys(T.fexst).map(x=>({v:x,l:T.fexst[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.tools.push({l:T.aFiNewE,cls:'p',go:()=>this.dlgOpen(this.fiExpDlg(T,reqs))},{l:T.aFiPdf,go:()=>this.act('Expense_Export',{format:'pdf',source:s.flt.fs||undefined,category:s.flt.fc||undefined,dateFrom:s.flt.fd1||undefined,dateTo:s.flt.fd2||undefined},T.tFiXp,()=>{})},{l:T.aFiXls,go:()=>this.act('Expense_Export',{format:'excel',source:s.flt.fs||undefined,category:s.flt.fc||undefined,dateFrom:s.flt.fd1||undefined,dateTo:s.flt.fd2||undefined},T.tFiXp,()=>{})});
  vm.blocks=[tbB('',col(T.ficDate,T.ficCat,T.ficAmt,T.ficDesc,T.ficSrc,T.ficSt,''),l.items.map(e=>row([dt(e.date),e.category,money(e.amount,e.currency),e.description||'—',T.fexs[e.source],T.fexst[e.status]],[...(e.status==='active'&&!e.reversalOfExpenseId?[ac(T.aFiRev,()=>this.dlgOpen(this.fiExpRevDlg(T,e)))]:[]),...(e.attachment?[dl('Expense_GetAttachmentDownloadUrl',e.id)]:[])])),T.emFi),note(T.nFiExp)]}

 else if(at==='run'){vm.hasFlt=true;vm.flt=[flt('ps',T.pycSt,'sel',Object.keys(T.pys).map(x=>({v:x,l:T.pys[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  if(hasL('a'))vm.tools.push({l:T.aPyNew,cls:'p',go:()=>this.dlgOpen(this.pyNewDlg(T))});
  vm.blocks=d.runForbidden?[note(T.nPyNot)]:[tbB('',col(T.pycPer,T.pycSt,T.pycPrep,T.pycReq,T.pycApp,T.pycCl,T.pycCnt,T.pycNet,''),l.items.map(r=>row([r.period,T.pys[r.status]+(r.isStale?' · '+T.pycStale:''),r.preparedBy.displayName,r.approvalRequestedAt?tm(r.approvalRequestedAt):'—',r.approvedBy?r.approvedBy.displayName:'—',r.closedAt?tm(r.closedAt):'—',f(r.employeeCount),r.totals.map(t=>money(t.net,t.currency)).join(' / ')||'—'],[ac(T.aPyOpen,()=>this.toSub('rn',r.id))])),T.emPy),note(T.nPyRun)]}
 else if(at==='rep'){const code=s.flt.fr||'monthlyStatement';const rp=d.rep;
  const pf=[flt('fr',T.pycRep,'sel',Object.keys(T.pyr).map(x=>({v:x,l:T.pyr[x]})))];
  const dfl=(a,b)=>{const x=[flt(a,T.pycDF,'tx'),flt(b,T.pycDT,'tx')];x.forEach(z=>{z.isTx=false;z.isDt=true});return x};
  if(code==='monthlyStatement')pf.push(flt('fq',T.pycPer,'tx'));else if(code==='adjustments')pf.push(flt('fa',T.pycFrom,'tx'),flt('fb',T.pycTo,'tx'),flt('ft',T.pycTyp,'sel',Object.keys(T.fadt).map(x=>({v:x,l:T.fadt[x]}))),flt('fe',T.pycEmp,'sel',emps));else pf.push(...dfl('fc','fd'));
  vm.hasFlt=true;vm.flt=pf;
  const q0=code==='monthlyStatement'?{period:s.flt.fq}:(code==='adjustments'?{periodFrom:s.flt.fa,periodTo:s.flt.fb,type:s.flt.ft||undefined,employeeId:s.flt.fe||undefined}:{dateFrom:s.flt.fc,dateTo:s.flt.fd});
  const ok0=code==='monthlyStatement'?!!s.flt.fq:(code==='adjustments'?!!(s.flt.fa&&s.flt.fb):!!(s.flt.fc&&s.flt.fd));
  if(ok0)vm.tools.push({l:T.aPyPdf,go:()=>this.act('FinanceReport_Export',{code,format:'pdf',...q0},T.tPyXp,()=>{})},{l:T.aPyXls,go:()=>this.act('FinanceReport_Export',{code,format:'excel',...q0},T.tPyXp,()=>{})});
  if(!ok0||!rp)vm.blocks=[note(d.repMissing?T.er2['Hr:Finance:PayrollRunNotFoundForPeriod']:T.nPyPick)];
  else if(code==='monthlyStatement')vm.blocks=[kvB(rp.period,'',[[T.pycSt,T.pys[rp.runStatus]],[T.pycCnt,f(rp.items.length)]]),tbB(T.pycTot,col(T.pycCur,T.pycGross,T.pycAdd,T.pycDed,T.pycNet),rp.totals.map(t=>row([t.currency,money(t.gross),money(t.additions),money(t.deductions),money(t.net)])),T.emPy),tbB('',col(T.pycEmp,T.pycGross,T.pycAdd,T.pycDed,T.pycNet),rp.items.map(i=>row([i.employee.fullName,money(i.gross,i.currency),money(i.additions),money(i.deductions),money(i.net)])),T.emPyI)];
  else if(code==='adjustments'){pgr('l',rp);vm.blocks=[tbB(T.pycTot,col(T.pycCur,T.pycAdd,T.pycDed),rp.totals.map(t=>row([t.currency,money(t.additions),money(t.deductions)])),T.emPy),tbB('',col(T.pycEmp,T.pycPer,T.pycTyp,T.pycAmt,T.ficReason,T.ficSrc),rp.items.map(a=>row([a.employee.fullName,a.period,T.fadt[a.type],money(a.amount,a.currency),a.reason,T.fads[a.source]])),T.emFi),note(T.nPyRep)]}
  else{pgr('l',rp);vm.blocks=[tbB(T.pycTot,col(T.pycCur,T.pycAmt),rp.totals.map(t=>row([t.currency,money(t.amount)])),T.emPy),tbB('',col(T.ficDate,T.ficCat,T.pycAmt,T.ficSt),rp.items.map(e=>row([dt(e.date),e.category,money(e.amount,e.currency),T.fexst[e.status]])),T.emFi),note(T.nPyRep)]}} else if(at==='req'){vm.hasFlt=true;vm.flt=[flt('fa',T.ficSt,'sel',Object.keys(T.farq).map(x=>({v:x,l:T.farq[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.ficEmp,T.ficType,T.ficSt,T.ficArr,T.ficDec,''),l.items.map(r=>row([r.employee.fullName,r.requestTypeName,T.farq[r.status],tm(r.arrivedAt),r.decidedAt?tm(r.decidedAt):'—'],[ac(T.aFiOpen,()=>this.toSub('arq',r.id))])),T.emFi),note(T.nFiReq)]}
}
if(p==='pay'){
 vm.title=T.payT;vm.sub=T.paySub;vm.hasTabs=false;const money=(n,c)=>this.fnMoney(n,c);
 const note=(t)=>{const b=kvB(t,'',[]);b.hasKv=false;return b};
 const dl=(id)=>this.act('Payslip_DownloadMyPdf',{id},T.tPyXp,()=>{});
 if(s.sub==='one'){const z=d.one;vm.tools=[{l:T.back,go:()=>this.toSub('list',null,{})}];
  if(z){vm.sub=z.period;vm.tools.push({l:T.aPyDl,cls:'p',go:()=>dl(z.id)});vm.blocks=[kvB(z.period,'',[[T.pycEmp,z.employee.fullName],[T.pycGross,money(z.gross,z.currency)],[T.pycAdd,money(z.additions,z.currency)],[T.pycDed,money(z.deductions,z.currency)],[T.pycNet,money(z.net,z.currency)],[T.pycClosed,tm(z.closedAt)]]),tbB(T.aPyLines,col(T.pycKind,T.pycLine,T.pycAmt),z.lines.map(l=>row([T.pyl[l.kind],l.label,money(l.amount,z.currency)])),T.emPyI)]}}
 else{pgr('l',d.list);const l=d.list||{items:[]};vm.blocks=[tbB('',col(T.pycPer,T.pycGross,T.pycAdd,T.pycDed,T.pycNet,T.pycClosed,''),l.items.map(z=>row([z.period,money(z.gross,z.currency),money(z.additions),money(z.deductions),money(z.net),tm(z.closedAt)],[ac(T.aPyOpen,()=>this.toSub('one',z.id)),ac(T.aPyDl,()=>dl(z.id))])),T.emPyS),note(T.nPyPay)]}
}
if(p==='notif'){vm.title=T.ntT;vm.sub=T.ntSub;vm.hasFlt=true;vm.flt=[flt('nr',T.ntFlt,'sel',[{v:'false',l:T.ntUnread},{v:'true',l:T.ntRead}])];pgr('l',d.list);const l=d.list||{items:[]};
 vm.tools=[{l:T.ntMarkAll,go:async()=>{const un=((this.state.data.list||{items:[]}).items).filter(x=>!x.isRead);for(const x of un)await this.call('Notification_MarkRead',{id:x.id});this.toast(T.tNtAll);this.enter()}}];
 const nb=kvB(T.ntNote,T.ntDev+' '+T.ntLive,[]);nb.hasKv=false;
 vm.blocks=[tbB('',col(T.ntTime,T.ntTitle,T.ntBody,T.ntState,''),l.items.map(x=>row([tm(x.creationTime),x.title,x.body,x.isRead?T.ntRead:T.ntUnread],[ac(T.ntOpen,()=>this.ntOpen(x)),...(x.isRead?[]:[ac(T.ntMark,()=>this.act('Notification_MarkRead',{id:x.id},null,()=>this.enter()))])])),T.ntEmpty),nb]}
if(p==='aud'){vm.title=T.auT;vm.sub=T.auSub;vm.hasFlt=true;
 const a=flt('af',T.fFrom,'tx'),b=flt('at2',T.fTo,'tx');[a,b].forEach(z=>{z.isTx=false;z.isDt=true});
 const us=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName||x.displayName}));
 vm.flt=[a,b,flt('au',T.auUserF,'sel',us),flt('aa',T.auActF,'sel',Object.keys(T.aua).map(x=>({v:x,l:T.aua[x]}))),flt('ae',T.auEntF,'tx'),flt('ao',T.auOkF,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}])];pgr('l',d.list);const l=d.list||{items:[]};
 const nb=kvB(T.auNote,'',[]);nb.hasKv=false;
 vm.blocks=[tbB('',col(T.auTime,T.auUser,T.auAct,T.auEnt,T.auEid,T.auFields,T.auIp,T.auOk),l.items.map(z=>row([tm(z.occurredAt),z.user?z.user.displayName:'—',T.aua[z.action],z.entityType||'—',z.entityId?String(z.entityId).slice(0,13):'—',z.changedFields.join('، ')||'—',z.ipAddress||'—',z.succeeded?T.yes:T.no])),T.emAu),nb]}
if(p==='rep'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['att','tRAtt','d'],['late','tRLate','d'],['leave','tRLeave','d'],['pend','tRPend','d'],['cust','tRCust','d'],['eval','tREval','d'],['dash','tRDash','v'],['cons','tRCons','v'],['hatt','tRHAtt','h'],['hbal','tRHBal','h'],['hreq','tRHReq','h'],['hcon','tRHCon','h'],['hwrn','tRHWrn','h']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'att');
 vm.title=T.rptT;vm.sub=T.rSub;vm.hasTabs=true;
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const kpiB=(t,sb,k,pw)=>({hasT:!!t,t,s:sb||'',hasKpi:true,kpis:k,hasPw:pw!=null,pw:(pw||0)+'%',hasKv:false,kv:[],hasTbl:false,hasForm:false,hasActs:false,fields:[],acts:[],cols:[],rows:[],empty:false,emptyTx:''});
 const n2=x=>f(Math.round((x||0)*100)/100);
 const ms=this.todayS().slice(0,8)+'01';
 const dfl=(k,lbl,v0)=>{const z=flt(k,lbl,'tx');z.isTx=false;z.isDt=true;z.v=s.flt[k]||v0;return z};
 const rg=()=>[dfl('rf',T.fFrom,ms),dfl('rt',T.fTo,this.todayS())];
 const tmO=(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName})),lkO=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName})),dpO=(d.dp?d.dp.items:[]).map(x=>({v:x.id,l:x.name||x.nameAr}));
 const items=d.rep&&d.rep.items?d.rep.items:[];
 const fromTo=()=>({from:s.flt.rf||ms,to:s.flt.rt||this.todayS()});
 const exp=(code,extra)=>vm.tools.push({l:T.aRExp,go:()=>this.dlgOpen({title:T.dRExp,body:T.dRExpB,fields:[{k:'g',l:T.fRFmt,t:'sel',req:1,init:'pdf',opts:Object.keys(T.rfmt).map(x=>({v:x,l:T.rfmt[x]}))}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('Report_Export',{code,format:v.g,...extra});return{m:T.tRExp+' · '+r.fileName}}})});
 const priv=kvB(T.nRPriv,'',[]);priv.hasKv=false;
 const rr2=(t)=>{const b=kvB(T[t],T.nRR2,[]);b.hasKv=false;return b};
 const eName=z=>z.employee.fullName;
 if(at==='att'){vm.hasFlt=true;vm.flt=[...rg(),flt('re',T.fEmp,'sel',tmO)];pgr('l',d.rep);const ft=fromTo();exp('teamAttendance',{...ft,employeeId:s.flt.re||undefined});
  vm.blocks=[kpiB('','',[{l:T.kRRows,v:f(d.rep?d.rep.totalCount:0),u:'',c:'#00C49F'},{l:T.kRPres,v:f(items.filter(z=>z.checkIn).length),u:'',c:'#10B981'},{l:T.kRLate,v:f(items.filter(z=>z.lateMinutes>0).length),u:'',c:'#F59E0B'},{l:T.kRLM,v:f(items.reduce((n,z)=>n+z.lateMinutes,0)),u:'',c:'#F59E0B'}],null),tbB('',col(T.cName,T.cDate,T.rcIn,T.rcOut,T.rcLateM,T.rcSt),items.map(z=>row([eName(z),dt(z.date),z.checkIn?tm(z.checkIn):'—',z.checkOut?tm(z.checkOut):'—',f(z.lateMinutes),z.statusName])),T.emRp),priv]}
 else if(at==='late'){vm.hasFlt=true;vm.flt=[...rg(),flt('re',T.fEmp,'sel',tmO)];pgr('l',d.rep);exp('teamLatenessAbsence',{...fromTo(),employeeId:s.flt.re||undefined});
  vm.blocks=[kpiB('','',[{l:T.kRLate,v:f(items.reduce((n,z)=>n+z.lateCount,0)),u:'',c:'#F59E0B'},{l:T.kRLM,v:f(items.reduce((n,z)=>n+z.lateMinutes,0)),u:'',c:'#F59E0B'},{l:T.kRAbs,v:f(items.reduce((n,z)=>n+z.absentDays,0)),u:'',c:'#EF4444'}],null),tbB('',col(T.cName,T.rcLateC,T.rcLateM,T.rcAbs),items.map(z=>row([eName(z),f(z.lateCount),f(z.lateMinutes),f(z.absentDays)])),T.emRp),priv]}
 else if(at==='leave'){vm.hasFlt=true;vm.flt=[...rg(),flt('re',T.fEmp,'sel',tmO)];pgr('l',d.rep);exp('teamLeaves',{...fromTo(),employeeId:s.flt.re||undefined});
  vm.blocks=[tbB('',col(T.cName,T.rcType,T.rcFrom,T.rcTo,T.rcDays,T.rcHours,T.rcSt),items.map(z=>row([eName(z),z.leaveTypeName,tm(z.from),tm(z.to),z.days!=null?n2(z.days):'—',z.hours!=null?n2(z.hours):'—',z.statusName])),T.emRp),priv]}
 else if(at==='pend'){vm.hasFlt=true;vm.flt=[flt('re',T.fEmp,'sel',tmO)];pgr('l',d.rep);exp('teamPending',{employeeId:s.flt.re||undefined});
  vm.blocks=[tbB('',col(T.cName,T.rcReq,T.rcSt,T.rcCr),items.map(z=>row([eName(z),z.requestTypeName,z.statusName,tm(z.creationTime)])),T.emRp),priv]}
 else if(at==='cust'||at==='eval'){vm.hasFlt=true;vm.flt=at==='cust'?[flt('re',T.fEmp,'sel',tmO),flt('rk',T.fRKind,'sel',Object.keys(T.rkind).map(x=>({v:x,l:T.rkind[x]})))]:[flt('re',T.fEmp,'sel',tmO)];exp(at==='cust'?'teamCustody':'teamEvaluations',{employeeId:s.flt.re||undefined});
  vm.blocks=at==='cust'?[tbB('',col(T.cName,T.cucItem,T.cucKind,T.cucAt,T.cucAck,T.cucSt),items.map(z=>row([z.employee?z.employee.fullName:'—',z.itemName,T.ckind[z.kind],tm(z.assignedAt),z.acknowledgedAt?tm(z.acknowledgedAt):'—',z.statusName])),T.emRp),priv]:[tbB('',col(T.cName,T.evPeriod,T.evSt,T.evOb,T.evGrade),items.map(z=>row([eName(z),z.year+'/'+z.month,T.evst[z.status],T.evob[z.objectionStatus],z.scores.map(c=>c.criterionName+': '+c.gradeName).join('، ')])),T.emRp),priv]}
 else if(at==='dash'){const r=d.rep;vm.hasFlt=true;const yy=flt('ry',T.fRYear,'tx');yy.v=s.flt.ry||'';vm.flt=[yy,flt('rm',T.fRMonth,'sel',[1,2,3,4,5,6,7,8,9,10,11,12].map(n=>({v:String(n),l:String(n)})))];exp('reviewDashboard',{year:s.flt.ry?Number(s.flt.ry):undefined,month:s.flt.rm?Number(s.flt.rm):undefined});
  if(r){const c=r.complaintCounts;vm.blocks=[kpiB(T.tRDash+' · '+r.year+'/'+r.month,T.nRAsOf+' '+tm(c.asOf),[{l:T.kRCnf,v:f(c.confidentialSubmittedCount),u:'',c:'#3B82F6'},{l:T.kRHi,v:f(c.highlyConfidentialSubmittedCount),u:'',c:'#3B82F6'},{l:T.kRLN,v:f(c.lateNormalCount),u:'',c:'#F59E0B'},{l:T.kRLC,v:f(c.lateConfidentialCount),u:'',c:'#F59E0B'},{l:T.kRLH,v:f(c.lateHighlyConfidentialCount),u:'',c:'#EF4444'},{l:T.kRDone,v:f(r.completedDepartmentsCount),u:'',c:'#10B981'},{l:T.kRLateD,v:f(r.lateDepartmentsCount),u:'',c:'#F59E0B'}],null),kvB(T.nRDash,'',[]),tbB('',col(T.rcMgr,T.rcDept,T.rcExp,T.rcSub,T.rcSt),r.departments.map(z=>row([z.manager.displayName,z.department?z.department.name:'—',f(z.completion.evaluationsExpected),f(z.completion.evaluationsSubmitted),z.completion.isCompleted?T.yes:(z.completion.isLate?T.cZLate:'—')])),T.emRD)];vm.blocks[1].hasKv=false;vm.blocks[1].s=''}}
 else if(at==='cons'){vm.hasFlt=false;exp('evaluationConsistency',{});vm.blocks=[tbB('',col(T.rcMgr,T.rcDept,T.rcExp,T.rcSub,T.rcRatio,T.rcSame,T.evGrade),items.map(z=>row([z.manager.displayName,z.department?z.department.name:'—',f(z.expectedCount),f(z.submittedCount),n2(z.completionRatio),z.sameGradeForAll?T.yes:T.no,z.gradeCounts.map(g=>g.gradeName+' ×'+g.count).join('، ')||'—'])),T.emRp)]}
 else if(at==='hatt'){vm.hasFlt=true;vm.flt=[...rg(),flt('rd',T.fRDept,'sel',dpO),flt('re',T.fEmp,'sel',lkO)];pgr('l',d.rep);exp('hrAttendance',{...fromTo(),departmentId:s.flt.rd||undefined,employeeId:s.flt.re||undefined});
  vm.blocks=[kpiB('','',[{l:T.kRRows,v:f(d.rep?d.rep.totalCount:0),u:'',c:'#00C49F'},{l:T.kRPres,v:f(items.reduce((n,z)=>n+z.presentDays,0)),u:'',c:'#10B981'},{l:T.kRAbs,v:f(items.reduce((n,z)=>n+z.absentDays,0)),u:'',c:'#EF4444'},{l:T.kRLM,v:f(items.reduce((n,z)=>n+z.lateMinutes,0)),u:'',c:'#F59E0B'}],null),tbB('',col(T.cName,T.rcSch,T.rcPres,T.rcAbs,T.rcLateC,T.rcLateM,T.rcLeaveD),items.map(z=>row([eName(z),f(z.scheduledWorkingDays),f(z.presentDays),f(z.absentDays),f(z.lateCount),f(z.lateMinutes),n2(z.leaveDays)])),T.emRp),priv]}
 else if(at==='hbal'){vm.hasFlt=true;vm.flt=[flt('rd',T.fRDept,'sel',dpO),flt('re',T.fEmp,'sel',lkO)];pgr('l',d.rep);exp('hrLeaveBalances',{departmentId:s.flt.rd||undefined,employeeId:s.flt.re||undefined});
  vm.blocks=[kpiB('','',[{l:T.kRRows,v:f(d.rep?d.rep.totalCount:0),u:'',c:'#00C49F'},{l:T.kRRem,v:n2(items.reduce((n,z)=>n+z.remainingDays,0)),u:'',c:'#10B981'}],null),tbB('',col(T.cName,T.rcYear,T.rcEnt,T.rcCar,T.rcUsed,T.rcHeld,T.rcRem,T.rcUsedH),items.map(z=>row([eName(z),dt(z.leaveYearStart),n2(z.entitlementDays),n2(z.carriedInDays),n2(z.usedDays),n2(z.heldDays),n2(z.remainingDays),n2(z.usedHours)])),T.emRp),priv]}
 else if(at==='hwrn'){vm.hasFlt=true;vm.flt=[...rg(),flt('rd',T.fRDept,'sel',dpO),flt('re',T.fEmp,'sel',lkO)];pgr('l',d.rep);exp('hrWarnings',{...fromTo(),departmentId:s.flt.rd||undefined,employeeId:s.flt.re||undefined});
  vm.blocks=[tbB('',col(T.cName,T.wrcLvl,T.wrcDate,T.wrcSt),items.map(z=>row([eName(z),z.levelName,dt(z.incidentDate),z.statusName])),T.emRp),priv]}
 else if(at==='hcon'){vm.hasFlt=true;const a0=flt('rf',T.fFrom,'tx'),b0=flt('rt',T.fTo,'tx');[a0,b0].forEach(z=>{z.isTx=false;z.isDt=true});vm.flt=[a0,b0,flt('rd',T.fRDept,'sel',dpO),flt('re',T.fEmp,'sel',lkO)];pgr('l',d.rep);exp('hrContracts',{endFrom:s.flt.rf||undefined,endTo:s.flt.rt||undefined,departmentId:s.flt.rd||undefined,employeeId:s.flt.re||undefined});
  vm.blocks=[tbB('',col(T.cName,T.fCtType,T.ctcStart,T.ctcEnd,T.ctcSt),items.map(z=>row([eName(z),z.typeName,dt(z.startDate),dt(z.endDate),z.statusName])),T.emRp),priv]}
 else if(at==='hreq'){vm.hasFlt=true;vm.flt=[...rg(),flt('rd',T.fRDept,'sel',dpO),flt('re',T.fEmp,'sel',lkO)];pgr('l',d.rep);exp('hrRequests',{...fromTo(),departmentId:s.flt.rd||undefined,employeeId:s.flt.re||undefined});
  vm.blocks=[tbB('',col(T.cName,T.rcReq,T.rcSt,T.rcCr,T.rcCl),items.map(z=>row([eName(z),z.requestTypeName,z.statusName,tm(z.creationTime),z.closedAt?tm(z.closedAt):'—'])),T.emRp),priv]}
 if(!TB.length)vm.denied=true;
}
if(p==='cmp'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tZMy','ed'],['inbox','tZIn','edhx'],['handlers','tZHd','hx'],['cfg','tZCf','m']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.zT;vm.sub=T.zSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.aZBack,go:()=>this.toSub('list',null,{tab:at})};
 const lvO=Object.keys(T.zl).map(x=>({v:x,l:T.zl[x]})),stO=Object.keys(T.zs).map(x=>({v:x,l:T.zs[x]}));
 const roleP=(arr)=>arr.map(r=>T3.rl[r]||r).join(' → ');
 const priv=(ks)=>{const b=kvB(T.kZPriv,'',ks.map(k=>['•',T[k]]));return b};
 const attTbl=(c,mine)=>tbB(T.fZAtt,col(T.cZFile,T.cZSize,T.cZTime,''),c.attachments.map(a=>row([a.fileName,f(Math.max(1,Math.round(a.sizeBytes/1024)))+' KB',tm(a.creationTime)],[ac(T.aZDl,()=>this.act('Complaint_DownloadAttachment',{id:c.id,attachmentId:a.id},T.tZDl,()=>{}))])),T.emZF);
 if(s.sub==='new'){
  vm.title=T.aZNew;vm.hasTabs=true;vm.tabs=[T.wZ1,T.wZ2,T.wZ3].map((x,i)=>({l:(i+1)+'. '+x,cls:s.step===i?'on':'',go:()=>{}}));
  const rs=(d.rs?d.rs.items:[]).map(x=>({v:x.id,l:x.fullName+(x.departmentName?' · '+x.departmentName:'')}));
  const cancel={l:T.cncl,go:()=>this.toSub('list',null,{tab:'my',step:0,fv:{},cmpPrev:null})};
  if(s.step===0){const sp=[{k:'targetKind',l:T.fZKind,t:'sel',req:1,opts:Object.keys(T.zk).map(x=>({v:x,l:T.zk[x]}))},{k:'respondent',l:T.fZResp,t:'sel',opts:rs,h:T.zRaisedSys.length?'':''}];
   vm.blocks=[fmB(T.wZ1,sp,[cancel,{l:T.aZPrev,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;if(v.targetKind==='employee'&&!v.respondent){this.setState({fe:{respondent:T.req}});return}await this.act('Complaint_PreviewRouting',{targetKind:v.targetKind,respondentId:v.targetKind==='employee'?v.respondent:undefined},null,(r)=>this.setState({cmpPrev:r,step:1}))}}]),priv(['pZ1','pZ2'])]}
  else if(s.step===1){const pv=s.cmpPrev;if(pv){const sp=[{k:'level',l:T.fZLvl,t:'sel',req:1,dflt:pv.minimumLevel,opts:pv.selectableLevels.map(x=>({v:x,l:T.zl[x]}))},{k:'subject',l:T.fZSub,req:1,max:200},{k:'content',l:T.fZCont,t:'ta',req:1,max:8000}];
   vm.blocks=[kvB(T.zRoutes,'',[[T.zMinL,T.zl[pv.minimumLevel]],[T.zSelL,pv.selectableLevels.map(x=>T.zl[x]).join('، ')]]),tbB('',col(T.cZLvl,T.cZPath),pv.routes.map(z=>row([T.zl[z.level],roleP(z.handlerPath)])),T.emZ),
    fmB(T.wZ2,sp,[{l:T.prev2,go:()=>this.setState({step:0})},{l:T.next,cls:'p',go:()=>{if(this.vld(sp)){const v=this.state.fv;if(v.level===undefined)this.setF('level',pv.minimumLevel);this.setState({step:2})}}}])]}}
  else{const pv=s.cmpPrev,v=s.fv,lvl=v.level!==undefined?v.level:(pv?pv.minimumLevel:'normal'),rn=(rs.find(x=>x.v===v.respondent)||{}).l||'—';
   vm.blocks=[kvB(T.wZ3,'',[[T.fZKind,T.zk[v.targetKind]],[T.fZResp,v.targetKind==='employee'?rn:'—'],[T.fZLvl,T.zl[lvl]],[T.fZSub,v.subject],[T.fZCont,v.content]]),priv(['pZ1','pZ2','pZ3','pZ4']),
    fmB('',[],[{l:T.prev2,go:()=>this.setState({step:1})},{l:T.aZSubmit,cls:'p',go:async()=>{const x=this.state.fv;await this.act('Complaint_Submit',{targetKind:x.targetKind,respondentId:x.targetKind==='employee'?x.respondent:undefined,level:x.level!==undefined?x.level:pv.minimumLevel,subject:x.subject,content:x.content},null,(r)=>{this.toast(T.tZSub+(r.levelRaisedBySystem?' · '+T.zRaisedSys:''));this.toSub('mine',r.id,{tab:'my',step:0,fv:{},cmpPrev:null})})}}])];vm.blocks[1].hasForm=false;vm.blocks[2].hasT=false}
 }else if(s.sub==='mine'){const c=d.one;vm.tools=[back];vm.title=T.cZRef;
  if(c){vm.sub=c.referenceNumber+' · '+T.zs[c.status];if(c.status!=='closed')vm.tools.push({l:T.aZRaise,go:()=>this.dlgOpen(this.zRaiseDlg(T,c,'my'))});
   vm.blocks=[kvB(c.subject,'',[[T.cZRef,c.referenceNumber],[T.fZKind,T.zk[c.targetKind]],[T.fZResp,c.respondent?c.respondent.fullName:'—'],[T.cZLvl,T.zl[c.level]],[T.cZStat,T.zs[c.status]],[T.fZCont,c.content],[T.cZAt,tm(c.submittedAt)],...(c.closedAt?[[T.cZClosed,tm(c.closedAt)]]:[])]),attTbl(c,true)];
   if(c.status!=='closed'){const up=fmB(T.fZAtt,[{k:'up',l:T.fZAtt,t:'file',h:T.fZAttH,up:(e)=>this.cmpUpload(e,c.id)}],[]);up.hasT=false;vm.blocks.splice(1,0,up)}
   vm.blocks.push(priv(c.status==='closed'?['pZ5','pZ6','pZ2']:['pZ5','pZ2','pZ3']))}}
 else if(s.sub==='handle'){const c=d.one;vm.tools=[back];vm.title=T.cZRef;
  if(c){vm.sub=c.referenceNumber+' · '+T.zs[c.status];const open=c.status!=='closed';
   if(open){vm.tools.push({l:T.aZAct,cls:'p',go:()=>this.dlgOpen(this.zActDlg(T,c))},{l:T.aZRaise,go:()=>this.dlgOpen(this.zRaiseDlg(T,c,'inbox'))});if(hasL('d')&&c.status==='pendingDepartmentManager')vm.tools.push({l:T.aZFwd,go:()=>this.dlgOpen(this.zFwdDlg(T,c))});vm.tools.push({l:T.aZClose,go:()=>this.dlgOpen(this.zCloseDlg(T,c))})}
   vm.tools.push({l:T.aZLog,go:()=>this.toSub('log',c.id)});
   vm.blocks=[kvB(c.subject,T.pZ5,[[T.cZRef,c.referenceNumber],[T.fZUser,c.complainant.fullName],[T.fZKind,T.zk[c.targetKind]],[T.fZResp,c.respondent?c.respondent.fullName+(c.respondentCategory?' ('+T.zc[c.respondentCategory]+')':''):'—'],[T.cZLvl,T.zl[c.level]+' · '+T.cZMin+': '+T.zl[c.minimumLevel]],[T.cZStat,T.zs[c.status]],[T.cZUser+' *',c.handler?c.handler.displayName:'—'],[T.fZCont,c.content],[T.cZAt,tm(c.submittedAt)],[T.cZDue,c.dueAt?tm(c.dueAt)+(c.isLate?' · '+T.cZLate:''):'—'],...(c.closedAt?[[T.cZClosed,tm(c.closedAt)],[T.cZDec,c.closureDecision]]:[])]),attTbl(c,false),
    tbB(T.cZAct,col(T.cZTime,T.cZAct,T.cZActor,T.cZNote),c.actions.map(a=>row([tm(a.creationTime),T.za[a.type],a.actor.displayName,a.note])),T.emZA),priv(['pZ2','pZ3','pZ4'])];
   if(open){const up=fmB(T.fZAtt,[{k:'up',l:T.fZAtt,t:'file',h:T.fZAttH,up:(e)=>this.cmpUpload(e,c.id)}],[]);up.hasT=false;vm.blocks.splice(1,0,up)}}}
 else if(s.sub==='log'){vm.tools=[{l:T.aZBack,go:()=>this.toSub('handle',s.sid)}];vm.title=T.aZLog;vm.sub=T.pZ7;pgr('l',d.log);const l=d.log||{items:[]};vm.blocks=[tbB('',col(T.cZTime,T.cZUser,T.cZAct),l.items.map(z=>row([tm(z.at),z.user.displayName,T.zal[z.action]])),T.emZL),priv(['pZ7','pZ5'])]}
 else if(at==='my'){vm.hasFlt=true;vm.flt=[flt('zs',T.cZStat,'sel',stO),flt('zl',T.cZLvl,'sel',lvO)];pgr('l',d.list);const l=d.list||{items:[]};
  vm.tools.push({l:T.aZNew,cls:'p',go:()=>{this.setState({sub:'new',step:0,fv:{},cmpPrev:null,data:{},derr:''});this.later()}});
  vm.blocks=[tbB('',col(T.cZRef,T.cZSub,T.cZLvl,T.cZStat,T.cZAt,T.cZClosed,''),l.items.map(x=>row([x.referenceNumber,x.subject,T.zl[x.level],T.zs[x.status],tm(x.submittedAt),x.closedAt?tm(x.closedAt):'—'],[ac(T.aZOpen,()=>this.toSub('mine',x.id))])),T.emZ),priv(['pZ1','pZ2','pZ3'])]}
 else if(at==='inbox'){vm.hasFlt=true;vm.flt=[flt('zs',T.cZStat,'sel',stO),flt('zl',T.cZLvl,'sel',lvO),flt('zt',T.fZFlt,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}])];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.cZRef,T.cZLvl,T.cZStat,T.cZAt,T.cZDue,T.cZLate,''),l.items.map(x=>row([x.referenceNumber,T.zl[x.level],T.zs[x.status],tm(x.submittedAt),x.dueAt?tm(x.dueAt):'—',x.isLate?T.yes:T.no],[ac(T.aZOpen,()=>this.toSub('handle',x.id))])),T.emZI),priv(['pZ8','pZ10','pZ5'])]}
 else if(at==='handlers'){vm.hasFlt=true;vm.flt=[flt('zs',T.cZStat,'sel',Object.keys(T.zas).map(x=>({v:x,l:T.zas[x]}))),flt('zl',T.cZLvl,'sel',lvO)];pgr('l',d.list);const l=d.list||{items:[]};
  if(hasL('h'))vm.tools.push({l:T.aZReqH,cls:'p',go:()=>this.dlgOpen(this.zAsgReqDlg(T,d.us?d.us.items:[]))});
  vm.blocks=[tbB('',col(T.cZUser,T.cZLvl,T.cZStat,T.cZFrom,T.cZTo,T.cZReason,T.cZReq,T.cZDec,''),l.items.map(a=>row([a.user.displayName,T.zl[a.level],T.zas[a.status],tm(a.from),a.to?tm(a.to):'—',a.reason,a.requestedBy.displayName,a.decidedBy?a.decidedBy.displayName+(a.decisionReason?' · '+a.decisionReason:''):'—'],hasL('x')?[...(a.status==='pendingApproval'?[ac(T.aZAppr,()=>this.dlgOpen(this.zAsgDecDlg(T,a,'approve'))),ac(T.aZRej,()=>this.dlgOpen(this.zAsgDecDlg(T,a,'reject')))]:[]),...(a.status==='approved'?[ac(T.aZRev,()=>this.dlgOpen(this.zAsgDecDlg(T,a,'revoke')))]:[])]:[])),T.emZH),priv(['pZ9','pZ10'])]}
 else if(at==='cfg'){const h=d.cfg;if(h){const sp=[{k:'dl',l:T.fZDelay,req:1,max:3,pat:/^[1-9]\d{0,2}$/,dflt:String(h.delayDeadlineWorkingDays)},{k:'iv',l:T.fZInt,req:1,max:3,pat:/^[1-9]\d{0,2}$/,dflt:String(h.overdueReminderIntervalWorkingDays)}];
   vm.blocks=[kvB(T.tZCf,T.pZ8,[[T.fZDelay,f(h.delayDeadlineWorkingDays)],[T.fZInt,f(h.overdueReminderIntervalWorkingDays)]]),fmB(T.aZSaveC,sp,[{l:T.aZSaveC,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;await this.act('SystemSettings_UpdateComplaints',{delayDeadlineWorkingDays:Number(v.dl!==undefined?v.dl:h.delayDeadlineWorkingDays),overdueReminderIntervalWorkingDays:Number(v.iv!==undefined?v.iv:h.overdueReminderIntervalWorkingDays),concurrencyStamp:h.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}])]}}
 if(!TB.length)vm.denied=true;
}
if(p==='req'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['inbox','tQIn','dhxmas'],['my','tQMy','edhxvasm'],['all','tQAll','hd'],['memos','tQMemo','edhvasmx'],['inc','tQInc','edhxvasm'],['deleg','tQDel','edhxvasm'],['types','tQTypes','m']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'my');
 vm.title=T.qT;vm.sub=T.qSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const back={l:T.aQBack,go:()=>this.toSub('list',null)};
 const roleL=r=>r?(T3.rl[r]||r):'—';
 const myEmp=s.me&&s.me.employee?s.me.employee.id:'';
 const tyOpts=(d.ty?d.ty.items:[]).map(x=>({v:x.id,l:x.name}));
 const stOpts=Object.keys(T.rst).map(x=>({v:x,l:T.rst[x]}));
 const open=(id)=>ac(T.aQOpen,()=>this.toSub('req',id));
 const mkR=(a,b)=>{const x=[flt(a,T.fFrom,'tx'),flt(b,T.fTo,'tx')];x.forEach(z=>{z.isTx=false;z.isDt=true});return x};
 if(s.sub==='req'){const r=d.one;vm.tools=[back];vm.title=T.qDet;
  if(r){vm.sub=r.type.name+' · '+r.employee.fullName;
   if(r.canApprove)vm.tools.push({l:T.aQApp,cls:'p',go:()=>this.dlgOpen(this.qDecDlg(T,r.id,true))});
   if(r.canReject)vm.tools.push({l:T.aQRej,go:()=>this.dlgOpen(this.qDecDlg(T,r.id,false))});
   if(r.canCancel)vm.tools.push({l:T.aQCan,go:()=>this.dlgOpen(this.qCanDlg(T,r.id))});
   vm.blocks=[kvB(T.qDet,'',[[T.cQType,r.type.name],[T.cQEmp,r.employee.fullName],[T.cQBy,r.submittedBy.displayName+(r.isOnBehalf?' ('+T.rdOnb+')':'')],[T.kQDesc,r.detailsSummary],[T.cQStat,r.statusLabel+' · '+T.rst[r.status]],[T.kQAR,roleL(r.currentApproverRole)],...(r.stockIssue?[[T.svIssueSt,r.stockIssue.statusLabel+' · '+r.stockIssue.item.name+' × '+r.stockIssue.quantity]]:[]),...(r.rejectionReason?[[T.cQRej,r.rejectionReason]]:[]),...(r.sourceMemoId?[[T.kQMemo,r.sourceMemoId]]:[]),[T.cQCr,tm(r.creationTime)]]),
    tbB(T.qSteps,col(T.cQOrd,T.cQRole,T.cQApr,T.cQDec,T.cQDBy,T.cQDeleg,T.cQCr,T.cQNote),r.steps.map(z=>row([f(z.order),roleL(z.approverRole),z.approver?z.approver.displayName:'—',T.dec[z.decision],z.decidedBy?z.decidedBy.displayName:'—',z.delegatedFrom?z.delegatedFrom.displayName:'—',tm(z.decidedAt),z.note])),T.emQS),
    tbB(T.qTl,col(T.cQCr,T.cQAct,T.cQBy,T.cQNote),r.timeline.map(z=>row([tm(z.occurredAt),z.action,z.actor?z.actor.displayName:'—',z.note])),T.emQS)]}}
 else if(s.sub==='memo'){const m=d.memo;vm.tools=[back];vm.title=T.tQMemo;
  if(m){const own=m.employee.id===myEmp,pend=['pendingManager','pendingHr','pendingExecutive'].includes(m.status);vm.sub=m.employee.fullName;
   if(!own&&pend){if((hasL('d')||hasL('m')||hasL('x'))&&['pendingManager','pendingExecutive'].includes(m.status))vm.tools.push({l:T.aQFwd,go:()=>this.dlgOpen(this.qNoteDlg(T,T.dQFwd,'Memo_Forward',m.id,T.tQFwd,false))});
    if(hasL('d')||hasL('h')||hasL('x')||hasL('m'))vm.tools.push({l:T.aQConv,cls:'p',go:()=>this.dlgOpen(this.qConvDlg(T,m,d.av?d.av.items:[]))});
    if(hasL('h')||hasL('x'))vm.tools.push({l:T.aQCls,go:()=>this.dlgOpen(this.qNoteDlg(T,T.dQCls,'Memo_Close',m.id,T.tQCls,true))})}
   vm.blocks=[kvB(T.tQMemo,'',[[T.cQEmp,m.employee.fullName],[T.cQKind,T.mk[m.kind]],[T.cQPrev,m.content],[T.cQStat,T.mst[m.status]],[T.kQHdl,roleL(m.currentHandlerRole)],...(m.closeReason?[[T.kQCls,m.closeReason]]:[]),...(m.convertedRequestId?[[T.kQConv,T.mtg.leaveRequest]]:[]),...(m.convertedIncidentId?[[T.kQConv,T.mtg.incident]]:[]),[T.cQCr,tm(m.creationTime)]]),
    tbB(T.qTl,col(T.cQCr,T.cQAct,T.cQBy,T.cQNote),m.timeline.map(z=>row([tm(z.occurredAt),z.action,z.actor?z.actor.displayName:'—',z.note])),T.emQS)]}}
 else if(s.sub==='inc'){const i=d.inc;vm.tools=[back];vm.title=T.tQInc;
  if(i){vm.sub=i.employee.fullName;const own=i.employee.id===myEmp;
   if(!own&&hasL('d')&&i.status==='pendingManager')vm.tools.push({l:T.aQFwd,go:()=>this.dlgOpen(this.qNoteDlg(T,T.dQFwd,'Incident_Forward',i.id,T.tQFwd,false))});
   if(hasL('h')&&['pendingHr','inProgress'].includes(i.status))vm.tools.push({l:T.aQUpd,cls:'p',go:()=>this.dlgOpen({title:T.dQUpd,fields:[{k:'g',l:T.fQTgt,t:'sel',req:1,opts:Object.keys(T.ist2t).map(x=>({v:x,l:T.ist2t[x]}))},{k:'n',l:T.fQNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Incident_UpdateStatus',{id:i.id,status:v.g,note:v.n||undefined,concurrencyStamp:i.concurrencyStamp});return{m:T.tQUpd}}})});
   vm.blocks=[kvB(T.tQInc,'',[[T.cQEmp,i.employee.fullName],[T.fQItem,i.item?i.item.name:'—'],[T.kQDesc,i.description],[T.cQStat,T.ist2[i.status]],[T.kQHdl,roleL(i.currentHandlerRole)],...(i.sourceMemoId?[[T.kQMemo,i.sourceMemoId]]:[]),[T.cQCr,tm(i.creationTime)]]),
    tbB(T.qTl,col(T.cQCr,T.cQAct,T.cQBy,T.cQNote),i.timeline.map(z=>row([tm(z.occurredAt),z.action,z.actor?z.actor.displayName:'—',z.note])),T.emQS)]}}
 else if(at==='inbox'){vm.hasFlt=true;vm.flt=[flt('qt',T.cQType,'sel',tyOpts),flt('qs',T.cStatus,'sel',stOpts)];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.cQType,T.cQEmp,T.cQBy,T.cQSum,T.cQStat,T.cQRole,T.cQWait,T.cQDeleg,''),l.items.map(x=>row([x.type.name,x.employee.fullName,x.submittedBy.displayName+(x.isOnBehalf?' ('+T.rdOnb+')':''),x.detailsSummary,T.rst[x.status],roleL(x.currentApproverRole),tm(x.waitingSince),x.delegatedFrom?x.delegatedFrom.displayName:'—'],[x.actionKind==='stockIssue'?ac(T.aSvOpenIssue,()=>this.goTab('inv','issues')):open(x.id)])),T.emQ)]}
 else if(at==='my'){vm.hasFlt=true;vm.flt=[flt('qt',T.cQType,'sel',tyOpts),flt('qs',T.cStatus,'sel',['pendingManager','pendingHr','pendingExecutive','approved','rejected','completed','cancelled'].map(x=>({v:x,l:T.rst[x]})))];pgr('l',d.list);const l=d.list||{items:[]};
  const team=hasL('d')?(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName})):[];
  vm.tools.push({l:T.aQNew,cls:'p',go:()=>this.dlgOpen(this.qNewDlg(T,d.ty?d.ty.items:[],team,d.ity?d.ity.items:[]))});
  vm.blocks=[tbB('',col(T.cQType,T.cQSum,T.cQStat,T.cQRole,T.cQRej,T.cQBy,T.cQCr,''),l.items.map(x=>row([x.type.name,x.detailsSummary,x.statusLabel,roleL(x.currentStageRole),x.rejectionReason,x.submittedBy.displayName+(x.isOnBehalf?' ('+T.rdOnb+')':''),tm(x.creationTime)],[open(x.id)])),T.emQR)]}
 else if(at==='all'){vm.hasFlt=true;const op=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName}));vm.flt=[flt('qt',T.cQType,'sel',tyOpts),flt('qs',T.cStatus,'sel',stOpts),flt('qe',T.fEmp,'sel',op),...mkR('xf','xt')];pgr('l',d.list);const l=d.list||{items:[]};
  vm.blocks=[tbB('',col(T.cQType,T.cQEmp,T.cQBy,T.cQSum,T.cQStat,T.cQRole,T.cQCr,''),l.items.map(x=>row([x.type.name,x.employee.fullName,x.submittedBy.displayName+(x.isOnBehalf?' ('+T.rdOnb+')':''),x.detailsSummary,T.rst[x.status],roleL(x.currentApproverRole),tm(x.creationTime)],[open(x.id)])),T.emQR)]}
 else if(at==='memos'){vm.hasFlt=true;vm.flt=[flt('qk',T.cQKind,'sel',Object.keys(T.mk).map(x=>({v:x,l:T.mk[x]}))),flt('qs',T.cStatus,'sel',Object.keys(T.mst).map(x=>({v:x,l:T.mst[x]})))];pgr('l',d.mm);
  if(hasL('e')||hasL('d')||hasL('h')||hasL('v')||hasL('a')||hasL('s')||hasL('m'))vm.tools.push({l:T.aQNewM,cls:'p',go:()=>this.dlgOpen(this.qMemoDlg(T))});
  const ro=(x)=>row([x.employee.fullName,T.mk[x.kind],x.contentPreview,T.mst[x.status],tm(x.creationTime)],[ac(T.aQOpen,()=>this.toSub('memo',x.id))]);
  vm.blocks=[];if(d.mm)vm.blocks.push(tbB(T.nQMemoM,col(T.cQEmp,T.cQKind,T.cQPrev,T.cQStat,T.cQCr,''),d.mm.items.map(ro),T.emQM));if(d.mi)vm.blocks.push(tbB(T.nQMemoI,col(T.cQEmp,T.cQKind,T.cQPrev,T.cQStat,T.cQCr,''),d.mi.items.map(ro),T.emQM))}
 else if(at==='inc'){vm.hasFlt=true;vm.flt=[flt('qs',T.cStatus,'sel',Object.keys(T.ist2).map(x=>({v:x,l:T.ist2[x]})))];pgr('l',d.im);
  vm.tools.push({l:T.aQNewI,cls:'p',go:()=>this.dlgOpen(this.qIncDlg(T,d.ity?d.ity.items:[]))});
  const ro=(x)=>row([x.employee.fullName,x.descriptionPreview,T.ist2[x.status],tm(x.creationTime)],[ac(T.aQOpen,()=>this.toSub('inc',x.id))]);
  vm.blocks=[];if(d.im)vm.blocks.push(tbB(T.nQIncM,col(T.cQEmp,T.cQPrev,T.cQStat,T.cQCr,''),d.im.items.map(ro),T.emQI));if(d.ii)vm.blocks.push(tbB(T.nQIncI,col(T.cQEmp,T.cQPrev,T.cQStat,T.cQCr,''),d.ii.items.map(ro),T.emQI))}
 else if(at==='deleg'){vm.hasFlt=true;vm.flt=[flt('qd',T.cQKind,'sel',Object.keys(T.ddir).map(x=>({v:x,l:T.ddir[x]}))),flt('qs',T.cStatus,'sel',Object.keys(T.dsts).map(x=>({v:x,l:T.dsts[x]})))];pgr('l',d.dg);
  if(hasL('d')||hasL('h')||hasL('x')||hasL('m')||hasL('a'))vm.tools.push({l:T.aQNewD,cls:'p',go:()=>this.dlgOpen(this.qDelDlg(T,d.cd?d.cd.items:[]))});
  const rowD=(x,can)=>row([x.fromUser.displayName,x.toUser.displayName,T.dscp[x.scope],tm(x.startsAt)+' → '+tm(x.endsAt),x.reason,T.dsts[x.status]],can&&x.fromUser.id===myEmp&&['active','scheduled'].includes(x.status)?[ac(T.aQCanD,delc('','Delegation_Cancel',{id:x.id}))]:[]);
  vm.blocks=[];if(d.dg)vm.blocks.push(tbB(T.nQDelM,col(T.cQFrom,T.cQTo,T.cQScope,T.cQPer,T.fQDR,T.cStatus,''),d.dg.items.map(x=>rowD(x,true)),T.emQD));
  if(d.da){const b3=tbB(T.nQDelA,col(T.cQFrom,T.cQTo,T.cQScope,T.cQPer,T.fQDR,T.cStatus,''),d.da.items.map(x=>rowD(x,false)),T.emQD);b3.s=T.nQDeleg;vm.blocks.push(b3)}}
 else if(at==='types'){pgr('l',d.list);const l=d.list||{items:[]};vm.tools.push({l:T.aQNewT,cls:'p',go:()=>this.dlgOpen(this.qTypeDlg(T,null))});
  vm.blocks=[tbB('',col(T.cQCode,T.fNameAr,T.fNameEn,T.cQDest,T.cQSys,T.cQAct2,''),l.items.map(t=>row([t.code,t.nameAr,t.nameEn,T.dest[t.destination],t.isSystem?T.yes:T.no,t.isActive?T.yes:T.no],[ac(T.edit,()=>this.dlgOpen(this.qTypeDlg(T,t)))])),T.emQT)]}
 if(!TB.length)vm.denied=true;
}
if(p==='leave'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['bal','tLBal','edhm'],['my','tLMy','edhm'],['emp','tLEmp','dhmx'],['cal','tLCal','d'],['types','tLTypes','m'],['hourly','tLHourly','m']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'bal');
 vm.title=T.lvT;vm.sub=T.lvSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const n2=x=>f(Math.round((x||0)*100)/100);
 const kpiB=(t,sb,k,pw)=>({hasT:!!t,t,s:sb||'',hasKpi:true,kpis:k,hasPw:pw!=null,pw:(pw||0)+'%',hasKv:false,kv:[],hasTbl:false,hasForm:false,hasActs:false,fields:[],acts:[],cols:[],rows:[],empty:false,emptyTx:''});
 const mkR=(a,b)=>{const x=[flt(a,T.fFrom,'tx'),flt(b,T.fTo,'tx')];x.forEach(z=>{z.isTx=false;z.isDt=true});return x};
 const kinds=Object.keys(T.lk).map(x=>({v:x,l:T.lk[x]}));
 const per=r=>r.kind==='hourly'?dt(r.date)+' '+r.fromTime+'–'+r.toTime:dt(r.startDate)+' → '+dt(r.endDate);
 const reqRows=items=>items.map(r=>row([per(r),T.lk[r.kind],r.otherLeaveType?r.otherLeaveType.name:'—',r.kind==='hourly'?n2(r.requestedHours)+' h':n2(r.workingDays),T.rst[r.status],T.be[r.balanceEffect],tm(r.creationTime)],[ac(T.aDetail,()=>this.dlgOpen(this.lvReqDlg(T,r,ar)))]));
 const reqCols=()=>col(T.cReqDate,T.cKind,T.cType,T.cWD,T.cStatus,T.cEffect,T.cCreated,'');
 const balBlocks=(b,led)=>{const total=b.carriedDays+b.currentYearEntitlementDays+b.adjustmentDays;const pw=total>0?Math.max(0,Math.min(100,Math.round(b.remainingDays/total*100))):0;
  const bl=[kpiB(T.bYear+': '+dt(b.yearStartDate)+' → '+dt(b.yearEndDate),b.isEntitlementSet?'':T.bNotSet,[{l:T.bRem,v:n2(b.remainingDays),u:T.bDays,c:'#00C49F'},{l:T.bCarRem,v:n2(b.carriedRemainingDays),u:T.bDays,c:'#3B82F6'},{l:T.bCurRem,v:n2(b.currentYearRemainingDays),u:T.bDays,c:'#10B981'},{l:T.bUsed,v:n2(b.usedDays),u:T.bDays,c:'#F59E0B'},{l:T.bHeld,v:n2(b.heldDays),u:T.bDays,c:'#F59E0B'},{l:T.bFut,v:n2(b.futureYearHeldDays),u:T.bDays,c:'#3B82F6'}],pw),
   kvB(T.bEnt,'',[[T.bEnt,n2(b.currentYearEntitlementDays)],[T.bCar,n2(b.carriedDays)],[T.bAdj,n2(b.adjustmentDays)],[T.bNext,b.nextYearEntitlementDays!=null?n2(b.nextYearEntitlementDays):'—']])];
  const h=b.hourly;bl.push(h.isConfigured?kpiB(T.hT,h.month,[{l:T.hQ,v:n2(h.quotaHours),u:'',c:'#00C49F'},{l:T.hU,v:n2(h.usedHours),u:'',c:'#F59E0B'},{l:T.hH,v:n2(h.heldHours),u:'',c:'#F59E0B'},{l:T.hR,v:n2(h.remainingHours),u:'',c:'#10B981'},{l:T.hPD,v:n2(h.hoursPerDay||0),u:'',c:'#3B82F6'}],h.quotaHours>0?Math.round(h.remainingHours/h.quotaHours*100):0):kvB(T.hT,T.hNC,[]));
  bl.push(tbB(T.oT,col(T.cType,T.oU,T.oH,T.oM,T.oR),b.otherLeaveUsages.map(o=>row([o.leaveTypeName,n2(o.usedDays),n2(o.heldDays),o.maxDays!=null?n2(o.maxDays):'—',o.remainingDays!=null?n2(o.remainingDays):'—'])),T.emTy));
  bl.push(tbB(T.lgT,col(T.cDate,T.lgType,T.lgBucket,T.lgDays,T.lgReason),led.items.map(z=>row([dt(z.creationTime.slice(0,10)),T.lt[z.type],T.lb[z.bucket],z.days!=null?n2(z.days):'—',z.reason])),T.emLg));return bl};
 const ledFlt=()=>[flt('lg',T.lgType,'sel',Object.keys(T.lt).map(x=>({v:x,l:T.lt[x]}))),...mkR('lf','lt')];
 if(s.sub==='new'){
  vm.title=T.aNewReq;vm.hasTabs=true;vm.tabs=[T.w1,T.w2,T.w3].map((x,i)=>({l:(i+1)+'. '+x,cls:s.step===i?'on':'',go:()=>{}}));
  const team=(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName})),k=s.fv.kind;
  const cancel={l:T.cncl,go:()=>this.toSub('list',null,{step:0,lvPrev:null,fv:{}})};
  if(s.step===0){const sp=[{k:'kind',l:T.wKind,t:'sel',req:1,opts:kinds},...(hasL('d')?[{k:'forEmp',l:T.wFor,t:'sel',opts:team}]:[])];vm.blocks=[fmB(T.w1,sp,[cancel,{l:T.next,cls:'p',go:()=>{if(this.vld(sp)){this.setState({step:1,derr:''});this.later()}}}])]}
  else if(s.step===1){const av=(d.av?d.av.items:[]).map(x=>({v:x.id,l:x.name+(x.isPaid?'':' ('+T.no+' '+T.pvPaid+')')})),tr=/^([01][0-9]|2[0-3]):[0-5][0-9]$/;
   const sp=k==='hourly'?[{k:'date',l:T.fDate,t:'date',req:1},{k:'fromTime',l:T.fFromT,req:1,max:5,pat:tr,h:T.hTime},{k:'toTime',l:T.fToT,req:1,max:5,pat:tr,h:T.hTime}]:[...(k==='other'?[{k:'otherLeaveTypeId',l:T.fOther,t:'sel',req:1,opts:av}]:[]),{k:'startDate',l:T.fStartD,t:'date',req:1},{k:'endDate',l:T.fEndD,t:'date',req:1}];
   vm.blocks=[fmB(T.w2+' · '+(T.lk[k]||''),sp,[{l:T.prev2,go:()=>{this.setState({step:0});this.later()}},{l:T.aPreview,cls:'p',go:async()=>{if(!this.vld(sp))return;await this.act('LeaveRequest_Preview',this.lvInput(this.state.fv),null,(r)=>{this.setState({lvPrev:r,step:2})})}}])]}
  else{const pv=s.lvPrev;if(pv){const kp=[{l:T.pvWork,v:n2(pv.workingDays),u:'',c:'#00C49F'},{l:T.pvWo,v:f(pv.excludedWeeklyOffDays),u:'',c:'#3B82F6'},{l:T.pvHol,v:f(pv.excludedHolidayDays),u:'',c:'#3B82F6'},{l:T.pvAnn,v:n2(pv.annualDays),u:'',c:'#F59E0B'},{l:T.pvFC,v:n2(pv.fromCarriedDays),u:'',c:'#10B981'},{l:T.pvFE,v:n2(pv.fromEntitlementDays),u:'',c:'#10B981'},{l:T.pvBef,v:n2(pv.availableDaysBefore),u:'',c:'#3B82F6'},{l:T.pvAft,v:n2(pv.remainingDaysAfter),u:'',c:'#00C49F'}];
   if(pv.requestedHours!=null)kp.push({l:T.pvHrs,v:n2(pv.requestedHours),u:'',c:'#00C49F'},{l:T.pvQ,v:n2(pv.hourlyQuotaHoursUsed),u:'',c:'#10B981'},{l:T.pvEx,v:n2(pv.hourlyExcessHours),u:'',c:'#F59E0B'});
   const need=pv.requiresAttachment&&!((s.fv.attach||[]).length);const can2=pv.canSubmit&&!need;
   const iss=pv.issues.map(i=>[i.code.split(':').pop(),T.er2[i.code]||i.message]);
   vm.blocks=[kpiB(T.pvT,pv.isBalanceSufficient?T.pvOk:T.pvShort+': '+n2(pv.shortageDays),kp,null),...(iss.length?[kvB(T.pvNo,'',iss)]:[]),
    tbB(T.pvSeg,col(T.cStart,T.cEnd,T.cWD,T.pvFC,T.pvFE),pv.segments.map(g=>row([dt(g.startDate)+(g.isFutureYear?' ('+T.pvFut+')':''),dt(g.endDate),n2(g.workingDays),n2(g.fromCarriedDays),n2(g.fromEntitlementDays)])),T.emC),
    tbB(T.pvBr,col(T.cDate,T.cStatus,T.pvCnt),pv.dayBreakdown.map(z=>row([dt(z.date),T.cls[z.classification]+(z.holidayName?' — '+z.holidayName:''),z.isCounted?T.yes:T.no])),T.emC),
    fmB(T.w3,[{k:'attach',l:T.fAtt+(pv.requiresAttachment?' *':''),t:'file',h:T.fAttH,dflt:[]},{k:'notes',l:T.fNotes,t:'ta',max:2000}],[{l:T.prev2,go:()=>{this.setState({step:1});this.later()}},{l:T.aSubmit,cls:'p'+(can2?'':' busy'),go:async()=>{const v=this.state.fv;await this.act('LeaveRequest_Create',{...this.lvInput(v),attachmentIds:(v.attach||[]).map(a=>a.id),notes:v.notes||undefined},T.tLSub,()=>this.toSub('list',null,{tab:'my',step:0,lvPrev:null,fv:{},flt:{}}))}}])]}}
 }else if(at==='bal'){vm.hasFlt=true;vm.flt=ledFlt();pgr('g',d.led);if(d.bal)vm.blocks=balBlocks(d.bal,d.led||{items:[]});
  vm.tools.push({l:T.aNewReq,cls:'p',go:()=>{this.setState({sub:'new',step:0,fv:{},lvPrev:null,data:{},derr:''});this.later()}})}
 else if(at==='my'){vm.hasFlt=true;vm.flt=[flt('ls',T.cStatus,'sel',Object.keys(T.rst).map(x=>({v:x,l:T.rst[x]}))),flt('lk',T.cKind,'sel',kinds),...mkR('lf','lt')];pgr('l',d.list);const l=d.list||{items:[]};
  vm.tools.push({l:T.aNewReq,cls:'p',go:()=>{this.setState({sub:'new',step:0,fv:{},lvPrev:null,data:{},derr:''});this.later()}});vm.blocks=[tbB('',reqCols(),reqRows(l.items),T.emL)]}
 else if(at==='emp'){vm.hasFlt=true;const op=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName}));vm.flt=[flt('le',T.fEmp,'sel',op),...ledFlt()];
  if(!s.flt.le){vm.blocks=[kvB(T.tLEmp,T.nPick,[])];vm.blocks[0].hasKv=false}
  else if(d.bal){pgr('g',d.led);vm.blocks=balBlocks(d.bal,d.led||{items:[]});const e=s.flt.le;
   if(hasL('h')){vm.tools.push({l:T.aEntitle,cls:'p',go:()=>this.dlgOpen(this.lvEntDlg(T,e,d.bal))},{l:T.aAdjust,go:()=>this.dlgOpen(this.lvAdjDlg(T,e,d.bal))})}
   if(d.list)vm.blocks.push(tbB(T.tLMy,reqCols(),reqRows(d.list.items),T.emL))}}
 else if(at==='cal'){const ms=this.todayS().slice(0,8)+'01',me2=this.dAdd(this.dAdd(ms,32).slice(0,8)+'01',-1);vm.hasFlt=true;const tm2=(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName}));
   const a=flt('cf',T.fFrom,'tx'),b2=flt('ct',T.fTo,'tx');[a,b2].forEach(z=>{z.isTx=false;z.isDt=true});a.v=s.flt.cf||ms;b2.v=s.flt.ct||me2;
   vm.flt=[a,b2,flt('ce',T.fEmp,'sel',tm2),flt('cp',T.fInc,'sel',[{v:'true',l:T.yes},{v:'false',l:T.no}])];pgr('l',d.cal);const l=d.cal||{items:[]};
   vm.blocks=[tbB('',col(T.cName,T.cKind,T.cType,T.cStart,T.cEnd,T.cTimes,T.cStatus,T.cWD),l.items.map(z=>row([z.employee.fullName,T.lk[z.kind],z.otherLeaveTypeName,dt(z.startDate),dt(z.endDate),z.fromTime?z.fromTime+'–'+z.toTime:'—',T.rst[z.status],z.workingDays!=null?n2(z.workingDays):(z.requestedHours!=null?n2(z.requestedHours)+' h':'—')])),T.emC)]}
 else if(at==='types'){pgr('l',d.list);const l=d.list||{items:[]};vm.tools.push({l:T.aNewType,cls:'p',go:()=>this.dlgOpen(this.lvTypeDlg(T,null))});
   vm.blocks=[tbB('',col(T.cName,T.fPaid,T.fMax,T.fReqAtt,T.fCat,T.fDed,T.fAct,''),l.items.map(t=>row([t.name,t.isPaid?T.yes:T.no,t.maxDays!=null?n2(t.maxDays):'—',t.requiresAttachment?T.yes:T.no,T.cat[t.eligibleCategory],t.deductsFromAnnualBalance?T.yes:T.no,t.isActive?T.fOn:T.fInactive],[ac(T.edit,()=>this.dlgOpen(this.lvTypeDlg(T,t)))])),T.emTy)]}
 else if(at==='hourly'){const h=d.hs;if(h){const sp=[{k:'m',l:T.fMonthly,req:1,max:6,pat:/^\d+(\.\d+)?$/,dflt:String(h.monthlyAllowedHours)},{k:'h',l:T.fHpd,req:1,max:5,pat:/^\d+(\.\d+)?$/,dflt:String(h.hoursPerDay)}];
   vm.blocks=[kvB(T.hsT,T.nHS,[[T.fMonthly,n2(h.monthlyAllowedHours)],[T.fHpd,n2(h.hoursPerDay)]]),fmB(T.aSaveH,sp,[{l:T.aSaveH,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;const mv=Number(v.m!==undefined?v.m:h.monthlyAllowedHours),hv=Number(v.h!==undefined?v.h:h.hoursPerDay);if(!(mv>=0.01)||!(hv>=0.01&&hv<=24)){this.setState({fe:{h:T.pat}});return}await this.act('SystemSettings_UpdateHourlyLeave',{monthlyAllowedHours:mv,hoursPerDay:hv,concurrencyStamp:h.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}])]}}
 if(!TB.length)vm.denied=true;
}
if(p==='att'){
 const rl3=rl.map(r=>this.AB3[r]).join(''),hasL=l=>rl3.includes(l);
 const TB=[['my','tMy','edhm'],['team','tTeam','d'],['emp','tEmp','dh'],['corr','tCorr','edhm'],['shifts','tShifts','hm'],['hol','tHol','edhxvasm'],['imp','tImp','h'],['dev','tDev','mh'],['meth','tMeth','edhm']].filter(z=>[...z[2]].some(c=>rl3.includes(c)));
 const at=TB.some(z=>z[0]===s.tab)?s.tab:(TB[0]?TB[0][0]:'hol');
 vm.title=T.attT;vm.sub=T.attSub;vm.hasTabs=s.sub==='list';
 vm.tabs=TB.map(z=>({l:T[z[1]],cls:at===z[0]?'on':'',go:()=>{this.setState({tab:z[0],skip:{},data:{},flt:{},derr:''});this.later()}}));
 const tz=d.cfg?d.cfg.timeZone:'Asia/Baghdad';
 const tT=x=>x?new Date(x).toLocaleTimeString(ar?'ar-IQ':'en-GB',{hour:'2-digit',minute:'2-digit',timeZone:tz}):'—';
 const tD=x=>x?new Date(x).toLocaleString(ar?'ar-IQ':'en-GB',{dateStyle:'medium',timeStyle:'short',timeZone:tz}):'—';
 const hmm=n=>{const h=Math.floor(n/60),m=n%60;return f(h)+':'+(m<10?f(0):'')+f(m)};
 const back={l:T.aBack,go:()=>this.toSub('list',null)};
 const stOpts=Object.keys(T.dst).map(x=>({v:x,l:T.dst[x]}));
 const dayRows=(items,showEmp,mine)=>items.map(z=>row([dt(z.date),...(showEmp?[z.employee.fullName]:[]),z.shift?z.shift.name:'—',z.scheduledStart?tT(z.scheduledStart)+' – '+tT(z.scheduledEnd):'—',tT(z.checkIn),tT(z.checkOut),T.dst[z.status],f(z.lateMinutes),hmm(z.workedMinutes),f(z.overtimeMinutes),z.isCorrected?T.yes:T.no],[ac(T.aDetail,()=>this.toSub('day',z.id)),...((mine||(at==='team'))&&['present','late','absent','incomplete'].includes(z.status)?[ac(mine?T.aReq:T.aReqEmp,()=>this.dlgOpen(this.corrDlg(T,z,mine?null:z.employee.id)))]:[])]));
 const dayCols=(showEmp)=>col(T.cDate,...(showEmp?[T.cName]:[]),T.cShift,T.cSched,T.cIn,T.cOut,T.cStatus,T.cLate,T.cWorked,T.cOT,T.cCorr,'');
 const rng=[flt('xf',T.fFrom,'tx'),flt('xt',T.fTo,'tx')];rng[0].isTx=false;rng[1].isTx=false;rng[0].isDt=true;rng[1].isDt=true;
 if(s.sub==='day'){
  const dd=d.day,z=dd?dd.day:null;vm.tools=[back];vm.title=T.aDetail;vm.sub=z?dt(z.date)+(z.employee?' · '+z.employee.fullName:''):'';
  if(z)vm.blocks=[kvB(T.kDay,T.nTz,[[T.cDate,dt(z.date)],[T.cName,z.employee.fullName],[T.cShift,z.shift?z.shift.name:'—'],[T.cSched,z.scheduledStart?tT(z.scheduledStart)+' – '+tT(z.scheduledEnd):'—'],[T.cIn,tT(z.checkIn)],[T.cOut,tT(z.checkOut)],[T.kOrigIn,tT(dd.originalCheckIn)],[T.kOrigOut,tT(dd.originalCheckOut)],[T.cStatus,T.dst[z.status]],[T.cLate,f(z.lateMinutes)],[T.cWorked,hmm(z.workedMinutes)],[T.cOT,f(z.overtimeMinutes)],[T.cCorr,z.isCorrected?T.yes:T.no]]),
   tbB(T.kRaw,col(T.cDate,T.cSrc,T.cDev),dd.rawPunches.map(x=>row([tD(x.punchTime),T.psrc[x.source],x.deviceName])),T.emHist),
   tbB(T.kCorrs,col(T.cPunchT,T.kCorrOld,T.kCorrNew,T.cReason,T.kBy),dd.corrections.map(x=>row([T.ptyp[x.punchType],tD(x.oldValue),tD(x.newValue),x.reason,x.requestedBy.displayName])),T.emHist)];
 }else if(s.sub==='map'){
  const dv=s.sdev||{};vm.tools=[back];vm.title=T.nMapT;vm.sub=dv.name||'';
  const lk=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName}));
  const mapDlg=(du)=>({title:T.dMapN,fields:[{k:'deviceUserId',l:T.lDevUser,req:1,max:64,init:du||''},{k:'employeeId',l:T.fEmp,t:'sel',req:1,opts:lk}],okLabel:T.cnf,run:async(v)=>{await this.call('DeviceUserMap_Create',{id:dv.id,deviceUserId:v.deviceUserId,employeeId:v.employeeId});return{m:T.tCreated}}});
  vm.tools.push({l:T.aAddMap,cls:'p',go:()=>this.dlgOpen(mapDlg(''))});
  const ml=d.maps||{items:[],totalCount:0},ul=d.un||{items:[],totalCount:0};
  vm.blocks=[tbB(T.nMapT,col(T.cDevUser,T.cName,T.cDate,''),ml.items.map(m=>row([m.deviceUserId,m.employee.fullName,dt(m.creationTime)],[ac(T.del,delc('','DeviceUserMap_Delete',{id:dv.id,mapId:m.id}))])),T.emMap),
   tbB(T.nUnT,col(T.cDevUser,T.cPunches,T.cFirst,T.cLast,''),ul.items.map(u=>row([u.deviceUserId,f(u.punchCount),tD(u.firstPunchTime),tD(u.lastPunchTime)],[ac(T.aMapBtn,()=>this.dlgOpen(mapDlg(u.deviceUserId)))])),T.emUn)];
 }else{
  const l=d.list||{items:[],totalCount:0};
  if(at==='my'){vm.hasFlt=true;vm.flt=[...rng,flt('xs',T.cStatus,'sel',stOpts)];pgr('l',d.list);vm.blocks=[tbB('',dayCols(false),dayRows(l.items,false,true),T.emAtt)];
   if(rl.includes('executiveDirector')&&!rl.some(r=>r!=='executiveDirector')){vm.blocks=[kvB(T.tMy,T.nExec,[])];vm.blocks[0].hasKv=false}}
  if(at==='team'){vm.hasFlt=true;const tm2=(d.tm?d.tm.items:[]).map(x=>({v:x.id,l:x.fullName}));vm.flt=[...rng,flt('xe',T.fEmp,'sel',tm2),flt('xs',T.cStatus,'sel',stOpts)];pgr('l',d.list);vm.blocks=[tbB('',dayCols(true),dayRows(l.items,true,false),T.emAtt)]}
  if(at==='emp'){vm.hasFlt=true;const op=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName||x.fullName}));vm.flt=[flt('xe',T.fEmp,'sel',op),...rng,flt('xs',T.cStatus,'sel',stOpts)];pgr('l',d.list);vm.blocks=[tbB('',dayCols(false),dayRows(l.items,false,false),T.emAtt)]}
  if(at==='corr'){const cl=s.corrList||[];vm.blocks=[kvB(T.tCorr,T.nCorr,[]),tbB('',col(T.cDate,T.cName,T.cPunchT,T.lTime,T.cStatus,T.cReason,''),cl.map(c=>row([dt(c.date),c.employee.fullName,T.ptyp[c.punchType],c.correctTime,T.rst[c.status],c.reason],[ac(T.aRefresh,()=>this.act('AttendanceCorrection_Get',{id:c.requestId},null,(r)=>this.setState(x=>({corrList:(x.corrList||[]).map(z=>z.requestId===r.requestId?r:z)}))))])),T.emCorr)];vm.blocks[0].hasKv=false}
  if(at==='shifts'){
   const wr=hasL('h')||hasL('m');vm.hasFlt=true;
   const lk=(d.lk?d.lk.items:[]).map(x=>({v:x.id,l:x.fullName}));
   vm.flt=[flt('xh',T.fType,'sel',Object.keys(T.styp).map(x=>({v:x,l:T.styp[x]}))),flt('xa',T.cStatus,'sel',[{v:'true',l:T.fOn},{v:'false',l:T.fInactive}]),...(hasL('h')?[flt('xi',T.nHistT,'sel',lk)]:[])];
   pgr('l',d.list);
   if(wr)vm.tools.push({l:T.aNewShift,cls:'p',go:()=>this.dlgOpen(this.shiftDlg(T,null))});
   if(hasL('h'))vm.tools.push({l:T.aAssign,go:()=>this.dlgOpen(this.assignDlg(T,l.items,lk))});
   const dsum=z=>z.type==='rotating'?(z.rotationCycleDays?f(z.rotationCycleDays)+' · ':'')+z.days.map(x=>f(x.cycleDayNumber)).join(','):z.days.map(x=>T.wd[x.dayOfWeek]).join('، ');
   vm.blocks=[tbB('',col(T.cName,T.fType,T.cDays,T.cStart,T.cGrace,T.cOtp,T.cStatus,T.cAssigned,''),l.items.map(z=>row([z.name,T.styp[z.type],dsum(z),(z.days[0]?z.days[0].startTime+'–'+z.days[0].endTime:'—'),f(z.graceMinutes),T.otp[z.overtimePolicy]+(z.overtimeAfterHours?' ('+f(z.overtimeAfterHours)+')':''),z.isActive?T.fOn:T.fInactive,f(z.assignedEmployeeCount)],wr?[ac(T.aEditShift,()=>this.dlgOpen(this.shiftDlg(T,z)))]:[])),T.emShift)];
   if(s.flt.xi&&d.hist)vm.blocks.push(tbB(T.nHistT,col(T.cShift,T.cStart,T.cEnd,T.cBy),d.hist.items.map(h=>row([h.shift.name,dt(h.startDate),dt(h.endDate),h.createdBy?h.createdBy.displayName:'—'])),T.emHist))}
  if(at==='hol'){
   const w=hasL('h');vm.hasFlt=true;vm.flt=rng;pgr('l',d.list);
   if(w)vm.tools.push({l:T.aNewHol,cls:'p',go:()=>this.dlgOpen(this.holDlg(T,null))});
   vm.blocks=[tbB('',col(T.cDate,T.cName,''),l.items.map(h=>row([dt(h.date),h.name],w?[ac(T.edit,()=>this.dlgOpen(this.holDlg(T,h))),ac(T.del,delc('','Holiday_Delete',{id:h.id}))]:[])),T.emHol)];
   const wk=d.wk;if(wk){const sp=[{k:'wk',l:T.nWk,t:'ck',opts:Object.keys(T.wd).map(x=>({v:x,l:T.wd[x]})),dflt:wk.days}];
    vm.blocks.push(w?fmB(T.nWk,sp,[{l:T.aSaveW,cls:'p',go:async()=>{const v=this.state.fv.wk!==undefined?this.state.fv.wk:wk.days;await this.act('WeeklyOffSetting_Update',{days:v,concurrencyStamp:wk.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}]):kvB(T.nWk,T.nRo,[[T.cDays,wk.days.map(x=>T.wd[x]).join('، ')]]))}}
  if(at==='imp'){vm.hasFlt=true;vm.flt=[flt('xd',T.cStatus,'sel',Object.keys(T.ist).map(x=>({v:x,l:T.ist[x]})))];pgr('l',d.list);
   const dvs=(d.dv?d.dv.items:[]);vm.tools.push({l:T.aNewImp,cls:'p',go:()=>this.dlgOpen(this.impDlg(T,dvs))});
   vm.blocks=[tbB('',col(T.cFile,T.cDev,T.cStatus,T.cTot,T.cAccd,T.cDup,T.cUnm,T.cErr,T.cDate,''),l.items.map(i=>row([i.fileName,(dvs.find(z=>z.id===i.deviceId)||{}).name||'—',T.ist[i.status],f(i.totalRows),f(i.acceptedCount),f(i.duplicateCount),f(i.unmappedCount),f(i.errorCount),tD(i.creationTime)],[ac(T.aDetail,()=>this.dlgOpen({title:T.dImpD,fields:[],kv:[{k:T.cFile,v:i.fileName},{k:T.cStatus,v:T.ist[i.status]},{k:T.cTot,v:f(i.totalRows)},{k:T.cAccd,v:f(i.acceptedCount)},{k:T.cDup,v:f(i.duplicateCount)},{k:T.cUnm,v:f(i.unmappedCount)},...i.errors.map(e=>({k:T.cRow+' '+f(e.rowNumber)+' · '+(e.columnName||'—'),v:T.ierr[e.code]+': '+e.message}))],okLabel:T.cnf,run:null}))])),T.emImp)]}
  if(at==='dev'){vm.hasFlt=true;vm.flt=[flt('xv',T.cStatus,'sel',[{v:'true',l:T.fOn},{v:'false',l:T.fInactive}])];pgr('l',d.list);const am=hasL('m');
   if(am)vm.tools.push({l:T.aNewDev,cls:'p',go:()=>this.dlgOpen({title:T.dDev,fields:[{k:'serialNumber',l:T.lSerial,req:1,max:128},{k:'name',l:T.lDevName,req:1,max:128}],okLabel:T.finish,run:async(v)=>{const r=await this.call('AttendanceDevice_Create',{serialNumber:v.serialNumber,name:v.name});return{next:this.keyDlg(T,r.deviceKey)}}})});
   vm.blocks=[tbB('',col(T.cName,T.cSerial,T.cStatus,T.cSeen,T.cMapped,T.cUnmapped,''),l.items.map(z=>row([z.name,z.serialNumber,z.isActive?T.fOn:T.fInactive,tD(z.lastSeenAt),f(z.mappedUserCount),f(z.unmappedUserCount)],[ac(T.aMap,()=>this.toSub('map',z.id,{sdev:z})),...(am?[ac(T.edit,()=>this.dlgOpen({title:T.dDevE,fields:[{k:'name',l:T.lDevName,req:1,max:128,init:z.name},{k:'isActive',l:T.lActive,t:'sel',req:1,init:String(z.isActive),opts:[{v:'true',l:T.fOn},{v:'false',l:T.fInactive}]}],okLabel:T.save2,run:async(v)=>{await this.call('AttendanceDevice_Update',{id:z.id,name:v.name,isActive:v.isActive==='true',concurrencyStamp:z.concurrencyStamp});return{m:T.tSaved}}})),ac(T.aRotate,()=>this.dlgOpen({title:T.dRot,body:T.dRotB,fields:[],okLabel:T.cnf,danger:true,run:async()=>{const r=await this.call('AttendanceDevice_RotateKey',{id:z.id});return{next:this.keyDlg(T,r.deviceKey)}}}))]:[])])),T.emDev)]}
  if(at==='meth'){const c=d.cfg;if(c){const am=hasL('m');const sp=[{k:'method',l:T.lMethod,t:'sel',req:1,dflt:c.method,opts:Object.keys(T.mth).map(x=>({v:x,l:T.mth[x]}))},{k:'timeZone',l:T.lTz,req:1,max:64,dflt:c.timeZone,h:T.hTz}];
   vm.blocks=[kvB(T.tMeth,T.nTz,[[T.kMethod,T.mth[c.method]],[T.kGps,c.gpsAvailable?T.yes:T.no],[T.kTz,c.timeZone]])];
   if(am)vm.blocks.push(fmB(T.aMethSave,sp,[{l:T.aMethSave,cls:'p',go:async()=>{if(!this.vld(sp))return;const v=this.state.fv;const tzv=v.timeZone!==undefined?v.timeZone:c.timeZone;try{new Intl.DateTimeFormat('en',{timeZone:tzv})}catch(e){this.setState({fe:{timeZone:T.pat}});return}await this.act('AttendanceSettings_Update',{method:v.method!==undefined?v.method:c.method,timeZone:tzv,concurrencyStamp:c.concurrencyStamp},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}]));else vm.blocks.push(kvB(T.nRo,'',[]));vm.blocks[vm.blocks.length-1].hasKv=false}}
  if(!TB.length)vm.denied=true;
 }
}

{const mt={emp:T.eT,dept:T.dT,acc:T.aT,team:T.tT,set:T.sT,att:T.attT,leave:T.lvT,req:T.qT,cmp:T.zT,rep:T.rptT,notif:T.ntT,aud:T.auT,perf:T.evT,ann:T.crT,inv:T.svT,cus:T.cuT,con:T.ctT,wrn:T.wrT,apr:T.apT,fin:T.fiT,pay:T.payT}[p];vm.crumbs=[{l:T3.nav?T3.nav:'HR',cur:'',go:()=>this.go('dash')},{l:mt,cur:vm.title===mt?'cur':'',go:()=>this.nav2(p==='set'?'hub':'list')}];if(vm.title&&vm.title!==mt)vm.crumbs.push({l:vm.title,cur:'cur',go:()=>{}})}
 return{isOrg:true,vm,...this.dl2(T,f)}};
deptDlg=(T,x)=>({title:x?T.edit:T.dNew,fields:[{k:'nameAr',l:T.lNameAr,req:1,max:128,init:x?x.nameAr:''},{k:'nameEn',l:T.lNameEn,req:1,max:128,init:x?x.nameEn:''}],okLabel:T.save2,run:async(v)=>{if(x)await this.call('Department_Update',{id:x.id,nameAr:v.nameAr,nameEn:v.nameEn,concurrencyStamp:x.concurrencyStamp});else await this.call('Department_Create',{nameAr:v.nameAr,nameEn:v.nameEn});return{m:x?T.tSaved:T.tCreated}}});
credDlg=(T,c)=>({title:T.dCred,body:T.dCredB,fields:[],kv:[{k:T.lUser,v:c.userName},{k:'Temporary password',v:c.temporaryPassword}],okLabel:T.cnf,run:null});
permB=(T,perm,op,id,fmB)=>{const sp=perm.groups.map(g=>({k:'perm:'+g.name,l:g.displayName,t:'ck',opts:g.permissions.map(x=>({v:x.name,l:x.displayName})),dflt:g.permissions.filter(x=>x.isGranted).map(x=>x.name)}));
 return [fmB(T.tPerm,sp,[{l:T.aSavePerm,cls:'p',go:async()=>{const v=this.state.fv,ch=[];perm.groups.forEach(g=>{const cur=v['perm:'+g.name];if(cur===undefined)return;g.permissions.forEach(x=>{const now=cur.includes(x.name);if(now!==x.isGranted)ch.push({name:x.name,isGranted:now})})});if(!ch.length){this.toast(T.tSaved);return}await this.act(op,{id,permissions:ch},T.tSaved,()=>{this.setState({fv:{}});this.enter()})}}])]};
dl2=(T,f)=>{const s=this.state,c=s.dlg;const e=s.derr2?this.etx(s.derr2):'';
 return{hasDlg:!!c,dlgv:{cncl:T.cncl,title:c?c.title:'',hasBody:!!(c&&c.body),body:c&&c.body?c.body:'',fields:c&&c.fields?c.fields.map(z=>this.fvm(z)):[],hasKv:!!(c&&c.kv),kv:c&&c.kv?c.kv:[],hasErr:!!e,err:e,ok:c?c.okLabel:'',cls:(c&&c.danger?'dng ':'')+(s.busy?'busy':''),hasOk:!!(c&&c.run)||!!c,busy:s.busy,idle:!s.busy}}};
dsSet=(k,v)=>this.setState(s=>({dsx:{...s.dsx,[k]:v}}));
dsTog=(k)=>this.setState(s=>({dsx:{...s.dsx,[k]:!s.dsx[k]}}));
dsReplay=()=>{this.dsSet('show',false);setTimeout(()=>this.dsSet('show',true),90)};
dsFile=(e)=>{const fl=e.target.files&&e.target.files[0];if(!fl)return;this.dsSet('file',fl.name+' · '+Math.max(1,Math.round(fl.size/1024))+' KB');this.dsSet('fp',0);
 let p=0;const iv=setInterval(()=>{p+=20;this.dsSet('fp',Math.min(p,100));if(p>=100)clearInterval(iv)},180)};
dsVm=(f)=>{
 const s=this.state,x=s.dsx,L=s.lang,ar=L==='ar',D=T5[L],AVc=['#00C49F','#3B82F6','#10B981','#F59E0B','#14B8A6'];
 const names=ar?['سارة','عمر','زينب','حسن','نور','كريم']:['Sara','Omar','Zaynab','Hassan','Noor','Karim'];
 const rows=[[names[0],ar?'الهندسة':'Engineering','ok',D.st1,82],[names[1],ar?'المبيعات':'Sales','wr',D.st2,45],[names[2],ar?'الموارد البشرية':'HR','in',D.st4,63],[names[3],ar?'العمليات':'Operations','er',D.st3,18],[names[4],ar?'الهندسة':'Engineering','nu',D.st5,30]];
 const sorted=rows.slice().sort((a,b)=>(x.sortAsc?1:-1)*a[0].localeCompare(b[0],ar?'ar':'en'));
 const pts=[12,30,22,48,40,66,58,82],lp='M'+pts.map((v,i)=>(i*40+10)+' '+(100-v)).join(' L');
 const bars=[40,62,50,78,66,90,72].map((h,i)=>({h,d:(i*.07)+'s',c:AVc[i%5]}));
 const segs=[[42,'#00C49F'],[28,'#3B82F6'],[18,'#F59E0B'],[12,'#EF4444']];let off=0;const ring=segs.map(g=>{const r={s:g[0]-1.5,o:-off,c:g[1]};off+=g[0];return r});
 const tl=D.tl.map((a,i)=>({a,w:D.tlw[i],cls:i<2?'done':i===2?'now':'',d:(i*.12)+'s'}));
 const sw=[['bg','--bg'],['surface','--card'],['text','--tx'],['text2','--tx2'],['accent','--ac'],['border','rgba(var(--w),.14)'],['ok','--ok'],['warn','--wr'],['dng','--er'],['inf','#3B82F6']].map(a=>({l:D[a[0]],c:(a[1].indexOf('#')===0||a[1].indexOf('rgba')===0)?a[1]:'var('+a[1]+')'}));
 const vpw={d:'100%',t:'768px',m:'390px'}[x.vp];
 return{
 escKey:(e)=>{if(e&&e.key==='Escape'){this.dsSet('modal',false);this.dsSet('conf',false);this.dsSet('dr',false);this.dsSet('dd',false);this.dsSet('pop',false)}},t:D.t,sub:D.sub,replay:D.replay,replayF:this.dsReplay,show:x.show,D,miniItems:[0,1,3,4].map(k=>({l:T[L].nav[k],ic:IC[k],cls:k===1?'on':''})),
 sw,names,
 crumbs:[{l:D.bc1,cur:'',go:()=>this.go('dash')},{l:D.bc2,cur:'',go:()=>{}},{l:D.bc3,cur:'cur',go:()=>{}}],
 mini:x.mini,miniCls:x.mini?'cl':'',miniTog:()=>this.dsTog('mini'),
 rows:sorted.map((r,i)=>({n:r[0],dp:r[1],bc:'bg '+r[2],st:r[3],p:r[4]+'%',pv:f(r[4])+'%',ini:Array.from(r[0])[0],col:AVc[i%5],d:(i*.06)+'s'})),
 sortTog:()=>this.dsTog('sortAsc'),sortArrow:x.sortAsc?'▲':'▼',
 lineD:lp,bars,ring,tl,
 prog:x.prog,progT:f(x.prog)+'%',progInc:()=>this.dsSet('prog',Math.min(100,x.prog+15)),progDec:()=>this.dsSet('prog',Math.max(0,x.prog-15)),
 steps:[0,1,2,3].map(i=>({cls:x.prog>=(i+1)*25?'done':(x.prog>=i*25?'now':''),l:String(i+1)})),
 avs:names.slice(0,5).map((n,i)=>({ini:Array.from(n)[0],col:AVc[i%5],n})),avMore:'+'+f(3),
 tabs:D.tab.map((l,i)=>({l,cls:x.tab===i?'on':'',go:()=>this.dsSet('tab',i)})),tabI:x.tab,tabText:D.tab[x.tab],
 sw1:x.sw1,sw1c:x.sw1?'on':'',sw1t:()=>this.dsTog('sw1'),sw2:x.sw2,sw2c:x.sw2?'on':'',sw2t:()=>this.dsTog('sw2'),
 ck1:x.ck1,ck1t:()=>this.dsTog('ck1'),ck2:x.ck2,ck2t:()=>this.dsTog('ck2'),
 rds:['a','b','c'].map((k,i)=>({k,l:[D.o1,D.o2,D.o3][i],on:x.rd===k,pick:()=>this.dsSet('rd',k)})),
 selOpts:[D.o1,D.o2,D.o3].map((l,i)=>({v:String(i),l})),selV:x.sel,onSel:e=>this.dsSet('sel',e.target.value),
 q:x.q,onQ:e=>this.dsSet('q',e.target.value),qClear:()=>this.dsSet('q',''),hasQ:!!x.q,
 date:x.date,onDate:e=>this.dsSet('date',e.target.value),dateTx:x.date?this.fmtD(x.date,ar):'—',
 txv:x.txv,onTx:e=>this.dsSet('txv',e.target.value),
 onFile:this.dsFile,file:x.file,hasFile:!!x.file,noFile:!x.file,fp:(x.fp||0)+'%',fpDone:(x.fp||0)>=100,fileRm:()=>{this.dsSet('file',null);this.dsSet('fp',0)},
 modal:x.modal,modalO:()=>this.dsSet('modal',true),modalC:()=>this.dsSet('modal',false),
 conf:x.conf,confO:()=>this.dsSet('conf',true),confC:()=>this.dsSet('conf',false),confY:()=>{this.dsSet('conf',false);this.toast(D.tm1,'ok')},
 drCls:x.dr?'open':'',drO:()=>this.dsSet('dr',true),drC:()=>this.dsSet('dr',false),
 dd:x.dd,ddT:()=>this.dsTog('dd'),ddC:()=>this.dsSet('dd',false),ddItems:[D.dd1,D.dd2,D.dd3].map((l,i)=>({l,cls:i===2?'dng':'',go:()=>{this.dsSet('dd',false);this.toast(l,i===2?'er':'in')}})),
 pop:x.pop,popT:()=>this.dsTog('pop'),
 tks:[['ok','tk1','tm1'],['in','tk2','tm2'],['wr','tk3','tm3'],['er','tk4','tm4']].map(a=>({k:a[0],l:D[a[1]],go:()=>this.toast(D[a[2]],a[0])})),
 mo:D.mo.map((l,i)=>({n:f(i+1),l,d:(i*.05)+'s'})),
 pgKey:x.show,
 vpw,vpd:x.vp==='d'?'on':'',vpt:x.vp==='t'?'on':'',vpm:x.vp==='m'?'on':'',setVd:()=>this.dsSet('vp','d'),setVt:()=>this.dsSet('vp','t'),setVm:()=>this.dsSet('vp','m'),
 kc:[[412,'#00C49F'],[27,'#F59E0B'],[86,'#3B82F6']].map((k,i)=>({t:k[0],c:k[1],l:[D.k1,D.k2,D.k3][i],d:(i*.12+.2)+'s'}))
 }};
AB3={employee:'e',departmentManager:'d',hrManager:'h',executiveDirector:'x',reviewer:'v',accountant:'a',storeOfficer:'s',systemAdmin:'m'};
d3=null;
pu=(str)=>{let h=2166136261;for(const c of str){h^=c.charCodeAt(0);h=Math.imul(h,16777619)}const a=(h>>>0).toString(16).padStart(8,'0');let g=Math.imul(h,2654435761)>>>0;return '00000000-0000-4000-8000-'+a+(g.toString(16).padStart(8,'0')).slice(0,4)};
hsh=(s)=>{let h=2166136261;for(const c of s){h^=c.charCodeAt(0);h=Math.imul(h,16777619)}return h>>>0};
dAdd=(d,n)=>{const t=new Date(d+'T12:00:00Z');t.setUTCDate(t.getUTCDate()+n);return t.toISOString().slice(0,10)};
todayS=()=>{const n=new Date();return n.getFullYear()+'-'+String(n.getMonth()+1).padStart(2,'0')+'-'+String(n.getDate()).padStart(2,'0')};
mk3=()=>{
 if(!this.db)this.mkDb();const U=this.UID,td=this.todayS();const wd=(dw,s,e)=>dw.map(d=>({dayOfWeek:d,cycleDayNumber:null,startTime:s,endTime:e,crossesMidnight:e<s}));
 const sun=['sunday','monday','tuesday','wednesday','thursday'];
 const shifts=[{id:U('41'),nameAr:'دوام ثابت صباحي',nameEn:'Fixed day shift',type:'fixed',grace:15,ot:'afterShiftEnd',otH:null,cyc:null,days:wd(sun,'08:00','16:00'),active:true,stamp:'a',created:'2024-01-01T08:00:00Z'},
 {id:U('42'),nameAr:'دوام متناوب',nameEn:'Rotating shift',type:'rotating',grace:10,ot:'afterDailyHours',otH:8,cyc:4,days:[1,2].map(n=>({dayOfWeek:null,cycleDayNumber:n,startTime:'07:00',endTime:'15:00',crossesMidnight:false})),active:true,stamp:'a',created:'2024-01-01T08:00:00Z'},
 {id:U('43'),nameAr:'دوام ليلي',nameEn:'Night shift',type:'overnight',grace:10,ot:'none',otH:null,cyc:null,days:wd(sun,'22:00','06:00'),active:true,stamp:'a',created:'2024-01-01T08:00:00Z'}];
 const es={};this.db.emps.forEach(e=>{es[e.id]=[{id:this.pu('es'+e.id),shift:shifts[0].id,start:e.hire,end:null,by:'—'}]});
 const now=new Date().toISOString();
 this.d3={shifts,es,weekly:{days:['friday','saturday'],stamp:'a'},hol:[{id:U('81'),date:this.dAdd(td,-3),ar:'عطلة رسمية (تجريبية)',en:'Public holiday (mock)',stamp:'a'},{id:U('82'),date:this.dAdd(td,6),ar:'عطلة وطنية (تجريبية)',en:'National day (mock)',stamp:'a'},{id:U('83'),date:this.dAdd(td,21),ar:'عطلة رسمية (تجريبية)',en:'Public holiday (mock)',stamp:'a'}],
  devs:[{id:U('91'),sn:'SN-0001',name:'Main gate',active:true,seen:new Date(Date.now()-3e5).toISOString(),stamp:'a',created:'2024-01-01T08:00:00Z'},{id:U('92'),sn:'SN-0002',name:'Warehouse',active:false,seen:null,stamp:'a',created:'2024-06-01T08:00:00Z'}],
  maps:[],unm:{[U('91')]:[{u:'999',n:12,f:now,l:now},{u:'998',n:3,f:now,l:now}]},imps:[],corr:[],cfg:{method:'device',gps:false,tz:'Asia/Baghdad',stamp:'a'},dayIdx:{},applied:{},seq:0};
 ['11','12','13','14','15','18','19','20'].forEach((n,i)=>this.d3.maps.push({id:U('A'+i),dev:U('91'),du:String(100+i),emp:U(n),at:'2024-02-01T08:00:00Z'}));
};
dayOf=(e,d)=>{ /* generate one AttendanceDayDto for employee e on date d, or null */
 const D=this.d3,ix=this.state.lang==='ar'?0:1;if(e.status==='ended'||d<e.hire||e.roles.includes('executiveDirector'))return null;
 const sh=D.shifts.find(z=>z.id===(D.es[e.id].slice(-1)[0].shift))||D.shifts[0];
 const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];
 const ref={id:e.id,fullName:ix?e.en:e.ar,departmentName:null},id=this.pu(e.id+d),sref={id:sh.id,name:ix?sh.nameEn:sh.nameAr,type:sh.type};
 const base={id,employee:ref,date:d,shift:sref,scheduledStart:null,scheduledEnd:null,checkIn:null,checkOut:null,status:'noShift',lateMinutes:0,workedMinutes:0,overtimeMinutes:0,hourlyLeaveMinutes:null,leaveRequestId:null,isCorrected:!!D.applied[e.id+'|'+d]};
 D.dayIdx[id]={e,d};
 if(D.hol.some(h=>h.date===d)){base.status='holiday';return base}
 if(D.weekly.days.includes(dn)){base.status='weeklyOff';return base}
 const sd=sh.days.find(z=>z.dayOfWeek===dn)||sh.days[0];const st=sd.startTime,en=sd.endTime;
 const iso=(dd,t)=>new Date(dd+'T'+t+':00+03:00').toISOString();const eD=en<st?this.dAdd(d,1):d;
 base.scheduledStart=iso(d,st);base.scheduledEnd=iso(eD,en);
 const r=this.hsh(e.id+d)%100,tdy=d===this.todayS();
 const mins=(x)=>Math.round(x/60000);
 if(r>=95-0&&r<100&&!tdy){base.status='onLeave';base.leaveRequestId=this.pu('lv'+e.id+d);return base}
 if(r>=84&&r<90||tdy&&r>50){base.status='incomplete';base.checkIn=new Date(new Date(base.scheduledStart).getTime()+((r%9)-3)*6e4).toISOString();return base}
 if(r>=78&&r<84&&!tdy){base.status='absent';return base}
 const late=r<62?((r%12)-8):(10+((r*7)%31));const ci=new Date(new Date(base.scheduledStart).getTime()+late*6e4);
 base.checkIn=ci.toISOString();const co=new Date(new Date(base.scheduledEnd).getTime()+((r%5)*9-5)*6e4);base.checkOut=co.toISOString();
 base.lateMinutes=late>sh.grace?late:0;base.status=base.lateMinutes>0?'late':'present';base.workedMinutes=mins(co-ci);
 base.overtimeMinutes=sh.ot==='afterShiftEnd'?Math.max(0,mins(co-new Date(base.scheduledEnd))):(sh.ot==='afterDailyHours'&&sh.otH?Math.max(0,base.workedMinutes-sh.otH*60):0);
 return base};
daysOf=(e,x)=>{const td=this.todayS();let to=x.toDate||td,fr=x.fromDate||this.dAdd(to,-29);if(fr>to)fr=to;const a=[];for(let d=to;d>=fr&&a.length<400;d=this.dAdd(d,-1)){const z=this.dayOf(e,d);if(z&&(!x.status||z.status===x.status))a.push(z)}return a};
route3=(op,x,P,v,ix,R)=>{
 if(!(op in OPR3)) return undefined;
 if(!this.d3) this.mk3();
 const D=this.d3,db=this.db,U=this.UID,meId=U(this.EID[v.p]);
 if(!P.roles.some(r=>OPR3[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar;
 const ref=id=>{const e=E(id);return e?{id:e.id,fullName:nmo(e),departmentName:null}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),chk=(o,s)=>{if(o.stamp!==s)R(409,'conflict')};
 const nm=(o)=>ix?o.nameEn:o.nameAr,hasR=r=>P.roles.includes(r),me=E(meId);
 const sd=s=>({id:s.id,name:nm(s),nameAr:s.nameAr,nameEn:s.nameEn,type:s.type,graceMinutes:s.grace,overtimePolicy:s.ot,overtimeAfterHours:s.otH,rotationCycleDays:s.cyc,days:s.days,isActive:s.active,assignedEmployeeCount:db.emps.filter(e=>D.es[e.id]&&D.es[e.id].slice(-1)[0].shift===s.id&&e.status==='active').length,creationTime:s.created,lastModificationTime:null,concurrencyStamp:s.stamp});
 const vS=(q)=>{const d=q.days||[];if(!d.length)R(422,'Hr:Shift:InvalidSchedule');
  if(q.type==='rotating'){if(!q.rotationCycleDays||d.some(z=>!z.cycleDayNumber||z.cycleDayNumber>q.rotationCycleDays))R(422,'Hr:Shift:InvalidSchedule')}else if(d.some(z=>!z.dayOfWeek))R(422,'Hr:Shift:InvalidSchedule');
  d.forEach(z=>{if((z.endTime<z.startTime)!==!!z.crossesMidnight)R(422,'Hr:Shift:CrossesMidnightMismatch')});
  if(q.type==='overnight'&&!d.some(z=>z.crossesMidnight))R(422,'Hr:Shift:CrossesMidnightMismatch')};
 const dd=h=>({id:h.id,date:h.date,name:ix?h.en:h.ar,nameAr:h.ar,nameEn:h.en,concurrencyStamp:h.stamp});
 const dv=d=>({id:d.id,serialNumber:d.sn,name:d.name,isActive:d.active,lastSeenAt:d.seen,mappedUserCount:D.maps.filter(m=>m.dev===d.id).length,unmappedUserCount:(D.unm[d.id]||[]).length,creationTime:d.created,concurrencyStamp:d.stamp});
 const imp=i=>({id:i.id,fileName:i.file,deviceId:i.dev,status:i.status,totalRows:i.total,acceptedCount:i.acc,duplicateCount:i.dup,unmappedCount:i.unm,errorCount:i.errs.length,errors:i.errs,uploadedBy:{id:meId,displayName:me?nmo(me):'—'},creationTime:i.at,completedAt:i.at});
 const cor=c=>({requestId:c.id,employee:ref(c.emp),submittedBy:{id:c.by,displayName:(E(c.by)?nmo(E(c.by)):'—')},isOnBehalf:c.onb,date:c.date,punchType:c.pt,correctTime:c.t,reason:c.reason,status:c.status,originalValue:c.orig,appliedValue:null,appliedAt:null,affectsClosedMonth:null,settlementPeriod:null,creationTime:c.at});
 const need=(o,c)=>{if(!o)R(404,c||'Hr:Attendance:NotFound');return o};
 const team=()=>db.emps.filter(e=>e.mgr===meId);
 const exe=e=>e&&e.roles.includes('executiveDirector');
 switch(op){
 case 'WorkShift_List':{let a=D.shifts;if(x.type)a=a.filter(s=>s.type===x.type);if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(s=>s.active===(x.isActive===true||x.isActive==='true'));return pg(a,x)&&{totalCount:a.length,items:pg(a,x).items.map(sd)}}
 case 'WorkShift_Get':return sd(need(D.shifts.find(s=>s.id===x.id)));
 case 'WorkShift_Create':{vS(x);const s={id:this.nid(),nameAr:x.nameAr,nameEn:x.nameEn,type:x.type,grace:x.graceMinutes,ot:x.overtimePolicy,otH:x.overtimeAfterHours??null,cyc:x.rotationCycleDays??null,days:x.days,active:true,stamp:stamp(),created:new Date().toISOString()};D.shifts.push(s);return sd(s)}
 case 'WorkShift_Update':{const s=need(D.shifts.find(z=>z.id===x.id));chk(s,x.concurrencyStamp);vS(x);Object.assign(s,{nameAr:x.nameAr,nameEn:x.nameEn,type:x.type,grace:x.graceMinutes,ot:x.overtimePolicy,otH:x.overtimeAfterHours??null,cyc:x.rotationCycleDays??null,days:x.days,active:x.isActive,stamp:stamp()});return sd(s)}
 case 'EmployeeShift_Assign':{const e=need(E(x.id),'Hr:Employee:EmployeeNotFound');if(exe(e))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');const s=need(D.shifts.find(z=>z.id===x.shiftId),'Hr:Shift:NotFound');if(!s.active)R(422,'Hr:Shift:ShiftInactive');if(x.startDate<e.hire)R(422,'Hr:Shift:StartDateBeforeHireDate');
  const h=D.es[e.id],cur=h[h.length-1];if(x.startDate<=cur.start)R(422,'Hr:Shift:StartDateBeforeCurrentAssignment');cur.end=this.dAdd(x.startDate,-1);const n={id:this.nid(),shift:s.id,start:x.startDate,end:null,by:me?nmo(me):'—'};h.push(n);
  return {id:n.id,employee:ref(e.id),shift:{id:s.id,name:nm(s),type:s.type},startDate:n.start,endDate:null,creationTime:new Date().toISOString(),createdBy:{id:meId,displayName:n.by}}}
 case 'EmployeeShift_GetHistory':{const e=need(E(x.id),'Hr:Employee:EmployeeNotFound');const a=(D.es[e.id]||[]).slice().reverse().map(r=>{const s=D.shifts.find(z=>z.id===r.shift);return{id:r.id,employee:ref(e.id),shift:{id:s.id,name:nm(s),type:s.type},startDate:r.start,endDate:r.end,creationTime:r.start+'T08:00:00Z',createdBy:{id:meId,displayName:r.by}}});return pg(a,x)&&{totalCount:a.length,items:pg(a,x).items}}
 case 'Holiday_List':{let a=D.hol.slice().sort((p,q)=>p.date.localeCompare(q.date));if(x.fromDate)a=a.filter(h=>h.date>=x.fromDate);if(x.toDate)a=a.filter(h=>h.date<=x.toDate);return {totalCount:a.length,items:pg(a,x).items.map(dd)}}
 case 'Holiday_Create':{const h={id:this.nid(),date:x.date,ar:x.nameAr,en:x.nameEn,stamp:stamp()};D.hol.push(h);return dd(h)}
 case 'Holiday_Update':{const h=need(D.hol.find(z=>z.id===x.id));chk(h,x.concurrencyStamp);Object.assign(h,{date:x.date,ar:x.nameAr,en:x.nameEn,stamp:stamp()});return dd(h)}
 case 'Holiday_Delete':{D.hol=D.hol.filter(z=>z.id!==x.id);return null}
 case 'WeeklyOffSetting_Get':return {days:D.weekly.days,concurrencyStamp:D.weekly.stamp};
 case 'WeeklyOffSetting_Update':{chk(D.weekly,x.concurrencyStamp);D.weekly.days=x.days;D.weekly.stamp=stamp();return {days:D.weekly.days,concurrencyStamp:D.weekly.stamp}}
 case 'Attendance_GetMy':{if(!me||exe(me))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');const a=this.daysOf(me,x);return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}
 case 'Attendance_GetTeam':{let t=team();if(x.employeeId)t=t.filter(e=>e.id===x.employeeId);let a=[];t.forEach(e=>{a=a.concat(this.daysOf(e,x))});a.sort((p,q)=>q.date.localeCompare(p.date)||p.employee.fullName.localeCompare(q.employee.fullName));return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}
 case 'Attendance_GetByEmployee':{const e=need(E(x.id),'Hr:Employee:EmployeeNotFound');if(exe(e))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');if(!hasR('hrManager')&&e.mgr!==meId)R(404,'Hr:Employee:EmployeeNotFound');const a=this.daysOf(e,x);return {totalCount:a.length,items:pg(a,x).items,summary:this.attSum(a)}}
 case 'Attendance_GetDayDetail':{if(!D.dayIdx[x.id]){const x2=0}const k=need(D.dayIdx[x.id]);const own=k.e.id===meId;if(!own&&!hasR('hrManager')&&!hasR('systemAdmin')&&k.e.mgr!==meId)R(404,'Hr:Attendance:NotFound');
  const day=this.dayOf(k.e,k.d);const raw=[];if(day.checkIn)raw.push({id:this.pu('r1'+x.id),punchTime:day.checkIn,source:'device',deviceName:'Main gate',importId:null});if(day.checkOut)raw.push({id:this.pu('r2'+x.id),punchTime:day.checkOut,source:'device',deviceName:'Main gate',importId:null});
  const cs=D.corr.filter(c=>c.emp===k.e.id&&c.date===k.d&&c.status==='approved').map(c=>({id:c.id,requestId:c.id,punchType:c.pt,oldValue:c.orig,newValue:day.checkIn||c.orig||new Date().toISOString(),reason:c.reason,appliedAt:c.at,requestedBy:{id:c.by,displayName:'—'},approvedBy:null,affectsClosedMonth:false,settlementPeriod:null}));
  return {day,originalCheckIn:day.checkIn,originalCheckOut:day.checkOut,rawPunches:raw,corrections:cs}}
 case 'AttendanceCorrection_Create':{const tgt=x.employeeId?need(E(x.employeeId),'Hr:Employee:EmployeeNotFound'):me;if(exe(tgt))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');
  if(x.employeeId&&x.employeeId!==meId&&(!hasR('departmentManager')||tgt.mgr!==meId))R(403,'forbidden');
  if(x.date>this.todayS())R(422,'Hr:Attendance:CorrectionDateInFuture');if(!/^([01][0-9]|2[0-3]):[0-5][0-9]$/.test(x.correctTime))R(422,'Hr:Attendance:CorrectionTimeInvalid');
  const day=this.dayOf(tgt,x.date);if(!day||['holiday','weeklyOff','noShift'].includes(day.status))R(422,'Hr:Attendance:CorrectionTimeInvalid');
  if(D.corr.some(c=>c.emp===tgt.id&&c.date===x.date&&c.pt===x.punchType&&!['rejected','cancelled'].includes(c.status)))R(422,'Hr:Attendance:CorrectionAlreadyPending');
  const onb=!!x.employeeId&&x.employeeId!==meId;const status=onb?'pendingHr':(hasR('hrManager')||hasR('systemAdmin'))&&!hasR('employee')?'pendingExecutive':'pendingManager';
  const c={id:this.nid(),emp:tgt.id,by:meId,onb,date:x.date,pt:x.punchType,t:x.correctTime,reason:x.reason,status,orig:x.punchType==='checkIn'?day.checkIn:day.checkOut,at:new Date().toISOString()};D.corr.push(c);return cor(c)}
 case 'AttendanceCorrection_Get':{const c=need(D.corr.find(z=>z.id===x.id));if(c.by!==meId&&c.emp!==meId&&!hasR('hrManager')&&!hasR('systemAdmin')&&!hasR('executiveDirector')&&(E(c.emp)||{}).mgr!==meId)R(404,'Hr:Attendance:NotFound');return cor(c)}
 case 'AttendanceImport_Create':{const dv0=need(D.devs.find(z=>z.id===x.deviceId),'Hr:Attendance:DeviceInactive');if(!dv0.active)R(422,'Hr:Attendance:DeviceInactive');if(!/\.(csv|xlsx|txt)$/i.test(x.file||''))R(422,'Hr:Attendance:ImportFileInvalid');
  const bad=/bad/i.test(x.file),errs=bad?[{rowNumber:3,columnName:'punchTime',code:'invalidDateTime',message:ix?'Invalid date/time value':'قيمة تاريخ/وقت غير صالحة'},{rowNumber:7,columnName:'deviceUserId',code:'emptyDeviceUserId',message:ix?'Device user id is empty':'معرّف مستخدم الجهاز فارغ'},{rowNumber:11,columnName:'punchTime',code:'futurePunch',message:ix?'Punch time is in the future':'وقت البصمة في المستقبل'}]:[];
  const i={id:this.nid(),file:x.file,dev:x.deviceId,status:bad?'completedWithErrors':'completed',total:40,acc:bad?30:36,dup:bad?4:3,unm:bad?3:1,errs,at:new Date().toISOString()};D.imps.push(i);return imp(i)}
 case 'AttendanceImport_List':{let a=D.imps.slice().reverse();if(x.status)a=a.filter(i=>i.status===x.status);return {totalCount:a.length,items:pg(a,x).items.map(imp)}}
 case 'AttendanceImport_Get':return imp(need(D.imps.find(z=>z.id===x.id)));
 case 'AttendanceSettings_Get':return {method:D.cfg.method,gpsAvailable:D.cfg.gps,timeZone:D.cfg.tz,concurrencyStamp:D.cfg.stamp};
 case 'AttendanceSettings_Update':{chk(D.cfg,x.concurrencyStamp);if((x.method==='gps'||x.method==='both')&&!D.cfg.gps)R(422,'Hr:Attendance:GpsNotAvailable');D.cfg.method=x.method;D.cfg.tz=x.timeZone;D.cfg.stamp=stamp();return {method:D.cfg.method,gpsAvailable:D.cfg.gps,timeZone:D.cfg.tz,concurrencyStamp:D.cfg.stamp}}
 case 'AttendanceDevice_List':{let a=D.devs;if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(d=>d.active===(x.isActive===true||x.isActive==='true'));return {totalCount:a.length,items:pg(a,x).items.map(dv)}}
 case 'AttendanceDevice_Create':{const d={id:this.nid(),sn:x.serialNumber,name:x.name,active:true,seen:null,stamp:stamp(),created:new Date().toISOString()};D.devs.push(d);return {device:dv(d),deviceKey:'dk_'+Math.random().toString(36).slice(2,14)}}
 case 'AttendanceDevice_Update':{const d=need(D.devs.find(z=>z.id===x.id));chk(d,x.concurrencyStamp);d.name=x.name;d.active=x.isActive;d.stamp=stamp();return dv(d)}
 case 'AttendanceDevice_RotateKey':{const d=need(D.devs.find(z=>z.id===x.id));return {device:dv(d),deviceKey:'dk_'+Math.random().toString(36).slice(2,14)}}
 case 'DeviceUserMap_List':{let a=D.maps.filter(m=>m.dev===x.id);if(x.employeeId)a=a.filter(m=>m.emp===x.employeeId);return {totalCount:a.length,items:pg(a,x).items.map(m=>({id:m.id,deviceId:m.dev,deviceUserId:m.du,employee:ref(m.emp),creationTime:m.at}))}}
 case 'DeviceUserMap_Create':{const e=need(E(x.employeeId),'Hr:Employee:EmployeeNotFound');if(exe(e))R(422,'Hr:Attendance:ExecutiveDirectorNotCovered');const m={id:this.nid(),dev:x.id,du:x.deviceUserId,emp:e.id,at:new Date().toISOString()};D.maps.push(m);D.unm[x.id]=(D.unm[x.id]||[]).filter(u=>u.u!==x.deviceUserId);return {id:m.id,deviceId:m.dev,deviceUserId:m.du,employee:ref(m.emp),creationTime:m.at}}
 case 'DeviceUserMap_Delete':{D.maps=D.maps.filter(m=>m.id!==x.mapId);return null}
 case 'DeviceUserMap_ListUnmapped':{const a=D.unm[x.id]||[];return {totalCount:a.length,items:pg(a,x).items.map(u=>({deviceUserId:u.u,punchCount:u.n,firstPunchTime:u.f,lastPunchTime:u.l}))}}
 }
 return undefined};
toSub=(sub,sid,extra)=>{this.setState({sub,sid:sid||null,data:{},fv:{},fe:{},dlg:null,skip:{},derr:'',...(extra||{})});this.later()};
keyDlg=(T,k)=>({title:T.dKey,body:T.dKeyB,fields:[],kv:[{k:T.dKey,v:k}],okLabel:T.cnf,run:null});
corrDlg=(T,z,empId)=>({title:T.dCorr,body:T.dCorrD+' · '+this.fmtD(z.date,this.state.lang==='ar'),fields:[{k:'punchType',l:T.lPunch,t:'sel',req:1,opts:Object.keys(T.ptyp).map(x=>({v:x,l:T.ptyp[x]}))},{k:'correctTime',l:T.lTime,req:1,max:5,pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/,h:T.hTime},{k:'reason',l:T.cReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('AttendanceCorrection_Create',{employeeId:empId||undefined,date:z.date,punchType:v.punchType,correctTime:v.correctTime,reason:v.reason});this.setState(x=>({corrList:[r,...(x.corrList||[])]}));return{m:T.tCorrSent}}});
holDlg=(T,h)=>({title:T.dHol,fields:[{k:'date',l:T.lDate,t:'date',req:1,init:h?h.date:''},{k:'nameAr',l:T.lNameAr,req:1,max:128,init:h?h.nameAr:''},{k:'nameEn',l:T.lNameEn,req:1,max:128,init:h?h.nameEn:''}],okLabel:T.save2,run:async(v)=>{if(h)await this.call('Holiday_Update',{id:h.id,date:v.date,nameAr:v.nameAr,nameEn:v.nameEn,concurrencyStamp:h.concurrencyStamp});else await this.call('Holiday_Create',{date:v.date,nameAr:v.nameAr,nameEn:v.nameEn});return{m:h?T.tSaved:T.tCreated}}});
impDlg=(T,dvs)=>({title:T.dImp,fields:[{k:'deviceId',l:T.lDevice,t:'sel',req:1,opts:dvs.map(z=>({v:z.id,l:z.name+(z.isActive?'':' (—)')}))},{k:'file',l:T.lFile,req:1,max:200,h:T.hFileM}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('AttendanceImport_Create',{file:v.file,deviceId:v.deviceId});return{m:T.tImpDone+' · '+T.ist[r.status]}}});
assignDlg=(T,shifts,emps)=>({title:T.dAssign,fields:[{k:'emp',l:T.fEmp,t:'sel',req:1,opts:emps},{k:'shiftId',l:T.lShiftS,t:'sel',req:1,opts:shifts.map(z=>({v:z.id,l:z.name+(z.isActive?'':' (—)')}))},{k:'startDate',l:T.lStartD,t:'date',req:1}],okLabel:T.cnf,run:async(v)=>{await this.call('EmployeeShift_Assign',{id:v.emp,shiftId:v.shiftId,startDate:v.startDate});return{m:T.tAssigned2}}});
shiftDlg=(T,z)=>{const D7=Object.keys(T.wd).map(x=>({v:x,l:T.wd[x]}));
 return{title:z?T.dShiftE:T.dShiftN,fields:[{k:'nameAr',l:T.lNameAr,req:1,max:128,init:z?z.nameAr:''},{k:'nameEn',l:T.lNameEn,req:1,max:128,init:z?z.nameEn:''},{k:'type',l:T.fType,t:'sel',req:1,init:z?z.type:'fixed',opts:Object.keys(T.styp).map(x=>({v:x,l:T.styp[x]}))},
 {k:'graceMinutes',l:T.lGrace,req:1,max:3,init:z?String(z.graceMinutes):'15',pat:/^(?:[0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-3][0-9]|240)$/},{k:'overtimePolicy',l:T.lOtp,t:'sel',req:1,init:z?z.overtimePolicy:'none',opts:Object.keys(T.otp).map(x=>({v:x,l:T.otp[x]}))},{k:'overtimeAfterHours',l:T.lOtH,max:6,init:z&&z.overtimeAfterHours?String(z.overtimeAfterHours):'',pat:/^\d+(\.\d+)?$/},
 {k:'rotationCycleDays',l:T.lCyc,max:3,init:z&&z.rotationCycleDays?String(z.rotationCycleDays):'',pat:/^(?:[2-9]|[1-9]\d|[12]\d\d|3[0-5]\d|36[0-6])$/},{k:'workDays',l:T.lWork,t:'ck',opts:D7,init:z&&z.type!=='rotating'?z.days.map(x=>x.dayOfWeek):['sunday','monday','tuesday','wednesday','thursday']},{k:'cycleDays',l:T.lCycD,max:60,init:z&&z.type==='rotating'?z.days.map(x=>x.cycleDayNumber).join(','):'',pat:/^\d+(,\d+)*$/},
 {k:'startTime',l:T.lStart,req:1,max:5,init:z&&z.days[0]?z.days[0].startTime:'08:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/,h:T.hTime},{k:'endTime',l:T.lEnd,req:1,max:5,init:z&&z.days[0]?z.days[0].endTime:'16:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/,h:T.hTime},
 ...(z?[{k:'isActive',l:T.lActive,t:'sel',req:1,init:String(z.isActive),opts:[{v:'true',l:T.fOn},{v:'false',l:T.fInactive}]}]:[])],okLabel:T.save2,
 run:async(v)=>{const cm=v.endTime<v.startTime;let days;if(v.type==='rotating'){days=String(v.cycleDays||'').split(',').filter(Boolean).map(n=>({dayOfWeek:null,cycleDayNumber:Number(n),startTime:v.startTime,endTime:v.endTime,crossesMidnight:cm}))}else{days=(v.workDays||[]).map(dw=>({dayOfWeek:dw,cycleDayNumber:null,startTime:v.startTime,endTime:v.endTime,crossesMidnight:cm}))}
  const body={nameAr:v.nameAr,nameEn:v.nameEn,type:v.type,graceMinutes:Number(v.graceMinutes),overtimePolicy:v.overtimePolicy,overtimeAfterHours:v.overtimeAfterHours?Number(v.overtimeAfterHours):null,rotationCycleDays:v.rotationCycleDays?Number(v.rotationCycleDays):null,days};
  if(z)await this.call('WorkShift_Update',{id:z.id,...body,isActive:v.isActive==='true',concurrencyStamp:z.concurrencyStamp});else await this.call('WorkShift_Create',body);return{m:T.tShiftSaved}}}};
d4=null;
mk4=()=>{
 if(!this.d3)this.mk3();const U=this.UID,db=this.db,td=this.todayS();
 const ent={'11':24,'12':24,'13':24,'14':21,'16':24,'18':21,'19':24,'20':21,'21':24,'22':24,'23':24},car={'11':6,'12':3,'13':2,'19':4};
 const y={};db.emps.forEach(e=>{const k=e.id.slice(-2);if(e.status==='ended'||e.roles.includes('executiveDirector'))return;y[e.id]={set:ent[k]!=null,ent:ent[k]||0,car:car[k]||0,adj:0,nextEnt:null,led:[]}});
 const types=[['B1','مرضية','Sick',true,30,true,'all'],['B2','أمومة','Maternity',true,98,true,'female'],['B3','حداد','Bereavement',true,3,false,'all'],['B4','بدون راتب','Unpaid',false,null,false,'all']].map(a=>({id:U(a[0]),ar:a[1],en:a[2],paid:a[3],max:a[4],att:a[5],cat:a[6],ded:false,active:true,stamp:'a',created:'2024-01-01T08:00:00Z'}));
 this.d4={y,reqs:[],types,cfg:{monthly:8,hpd:8,stamp:'a'},atts:[],seq:0};
 const D=this.d4;
 Object.keys(y).forEach(id=>{const e=db.emps.find(z=>z.id===id),yr=this.lvYear(e);const L=y[id].led;if(y[id].set)L.push({id:this.nid(),type:'grant',bucket:'entitlement',days:y[id].ent,reason:null,req:null,at:yr.start+'T08:00:00Z',by:'—'});if(y[id].car)L.push({id:this.nid(),type:'carryIn',bucket:'carried',days:y[id].car,reason:null,req:null,at:yr.start+'T08:00:00Z',by:'—'})});
 const seed=(k,kind,s,e2,st,eff,extra)=>{try{const e=db.emps.find(z=>z.id===U(k));const p=this.lvPreview(e,{kind,startDate:s,endDate:e2,...(extra||{})},true);if(!p)return;const r=this.lvMake(e,{kind,startDate:s,endDate:e2,...(extra||{})},p,e.id,false);r.status=st;r.effect=eff;return r}catch(err){return null}};
 seed('11','annual',this.dAdd(td,-24),this.dAdd(td,-19),'approved','deducted');
 seed('11','annual',this.dAdd(td,7),this.dAdd(td,11),'pendingManager','held');
 seed('18','other',this.dAdd(td,-12),this.dAdd(td,-7),'approved','none',{otherLeaveTypeId:types[0].id});
 seed('12','annual',this.dAdd(td,14),this.dAdd(td,19),'pendingManager','held');
 seed('19','annual',this.dAdd(td,2),this.dAdd(td,7),'pendingHr','held');
};
lvYear=(e)=>{const td=this.todayS(),h=e.hire;let sy=Number(td.slice(0,4));let st=sy+h.slice(4);if(st>td)st=(sy-1)+h.slice(4);const en=this.dAdd((Number(st.slice(0,4))+1)+st.slice(4),-1);return{start:st,end:en,next:this.dAdd(en,1),nextEnd:this.dAdd((Number(en.slice(0,4))+1)+en.slice(4),0)}};
lvWork=(e,from,to)=>{const D3=this.d3,sh=D3.shifts.find(z=>z.id===D3.es[e.id].slice(-1)[0].shift)||D3.shifts[0];const a=[];
 for(let d=from;d<=to&&a.length<800;d=this.dAdd(d,1)){const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];
  const hol=D3.hol.find(h=>h.date===d);let c='workingDay';
  if(D3.weekly.days.includes(dn))c='weeklyOff';else if(hol)c='holiday';else if(sh.type==='rotating'){const n=Math.round((new Date(d)-new Date('2024-01-01'))/864e5)%sh.cyc+1;if(!sh.days.some(z=>z.cycleDayNumber===n))c='weeklyOff'}else if(!sh.days.some(z=>z.dayOfWeek===dn))c='weeklyOff';
  a.push({date:d,classification:c,holidayName:hol?(this.state.lang==='ar'?hol.ar:hol.en):null,isCounted:c==='workingDay'})}
 return{a,sh}};
lvBal=(e)=>{const Y=this.d4.y[e.id];if(!Y)return null;const yr=this.lvYear(e),R=this.d4.reqs.filter(r=>r.emp===e.id&&['held','deducted'].includes(r.effect));
 let fc=0,fe=0,used=0,held=0,fut=0;R.forEach(r=>{fc+=r.cur.fc;fe+=r.cur.fe;fut+=r.fut.fc+r.fut.fe;if(r.effect==='deducted')used+=r.cur.fc+r.cur.fe;else held+=r.cur.fc+r.cur.fe});
 const carRem=Math.max(0,Y.car-fc),entRem=Y.ent+Y.adj-fe;return{yr,Y,carRem,entRem,rem:carRem+Math.max(0,entRem),used,held,fut,fc,fe}};
lvHours=(e,month,excludeId)=>{const C=this.d4.cfg;let u=0,h=0;this.d4.reqs.filter(r=>r.emp===e.id&&r.kind==='hourly'&&r.date&&r.date.slice(0,7)===month&&['held','deducted','none'].includes(r.effect)&&!['rejected','cancelled'].includes(r.status)&&r.id!==excludeId).forEach(r=>{if(r.status==='approved'||r.status==='completed')u+=r.quota;else h+=r.quota});return{quota:C.monthly,used:u,held:h,rem:Math.max(0,C.monthly-u-h)}};
lvPreview=(e,x,seedMode)=>{ /* returns LeaveRequestPreviewDto or throws {status,code} */
 const R=(st,c)=>{throw {status:st,code:c}};const D=this.d4,Y=D.y[e.id],b=this.lvBal(e),C=D.cfg;
 if(e.roles.includes('executiveDirector'))R(422,'Hr:Leave:ExecutiveDirectorNotEligible');if(e.status!=='active')R(422,'Hr:Leave:EmployeeNotActive');
 if(!this.d3.es[e.id])R(422,'Hr:Leave:NoShiftAssigned');
 const out={kind:x.kind,calendarDays:0,workingDays:0,excludedWeeklyOffDays:0,excludedHolidayDays:0,requestedHours:null,hourlyQuotaHoursUsed:null,hourlyExcessHours:null,deductsFromAnnualBalance:false,annualDays:0,fromCarriedDays:0,fromEntitlementDays:0,isPaid:null,requiresAttachment:false,segments:[],dayBreakdown:[],availableDaysBefore:b?b.rem:0,remainingDaysAfter:b?b.rem:0,availableHourlyHoursBefore:null,isBalanceSufficient:true,shortageDays:0,canSubmit:true,issues:[]};
 const issue=(c,m)=>{out.issues.push({code:c,message:m});out.canSubmit=false};
 let from,to,ded=false,type=null,hoursDays=0;
 if(x.kind==='hourly'){
  if(!x.date||!x.fromTime||!x.toTime||x.toTime<=x.fromTime)R(422,'Hr:Leave:InvalidDateRange');
  if(!C.monthly||!C.hpd)R(422,'Hr:Leave:HourlySettingsNotConfigured');
  const sh=this.d3.shifts.find(z=>z.id===this.d3.es[e.id].slice(-1)[0].shift),sd=sh.days[0];if(x.fromTime<sd.startTime||x.toTime>sd.endTime)R(422,'Hr:Leave:HourlyOutsideShift');
  const wk=this.lvWork(e,x.date,x.date).a[0];if(wk.classification!=='workingDay')R(422,'Hr:Leave:NoWorkingDaysInRange');
  const hrs=((Number(x.toTime.slice(0,2))*60+Number(x.toTime.slice(3)))-(Number(x.fromTime.slice(0,2))*60+Number(x.fromTime.slice(3))))/60;
  const hb=this.lvHours(e,x.date.slice(0,7));const q=Math.min(hrs,hb.rem),ex=hrs-q;
  Object.assign(out,{calendarDays:1,workingDays:0,requestedHours:hrs,hourlyQuotaHoursUsed:q,hourlyExcessHours:ex,availableHourlyHoursBefore:hb.rem,dayBreakdown:[wk]});
  const exd=ex/C.hpd;from=to=x.date;hoursDays=exd;out.deductsFromAnnualBalance=exd>0;out.annualDays=exd;
 }else{
  if(!x.startDate||!x.endDate||x.endDate<x.startDate)R(422,'Hr:Leave:InvalidDateRange');from=x.startDate;to=x.endDate;
  if(x.kind==='other'){type=D.types.find(t=>t.id===x.otherLeaveTypeId);if(!type)R(422,'Hr:Leave:OtherLeaveTypeInactive');if(!type.active)R(422,'Hr:Leave:OtherLeaveTypeInactive');if(type.cat!=='all'&&type.cat!==e.g)R(422,'Hr:Leave:NotEligibleForLeaveType');ded=type.ded;out.isPaid=type.paid;out.requiresAttachment=type.att}
  else ded=true;
  const yr=this.lvYear(e);if(to>yr.nextEnd)R(422,'Hr:Leave:RangeBeyondNextYear');
  const w=this.lvWork(e,from,to);out.dayBreakdown=w.a;out.calendarDays=w.a.length;out.workingDays=w.a.filter(z=>z.isCounted).length;out.excludedWeeklyOffDays=w.a.filter(z=>z.classification==='weeklyOff').length;out.excludedHolidayDays=w.a.filter(z=>z.classification==='holiday').length;
  if(!out.workingDays)R(422,'Hr:Leave:NoWorkingDaysInRange');
  if(type&&type.max!=null){const usedT=D.reqs.filter(r=>r.emp===e.id&&r.type===type.id&&!['rejected','cancelled'].includes(r.status)).reduce((n,r)=>n+r.workingDays,0);if(usedT+out.workingDays>type.max)R(422,'Hr:Leave:OtherLeaveTypeMaxExceeded')}
  out.deductsFromAnnualBalance=ded;
 }
 if(D.reqs.some(r=>r.emp===e.id&&!['rejected','cancelled'].includes(r.status)&&r.kind!=='hourly'&&x.kind!=='hourly'&&!(to<r.from||from>r.to)))R(422,'Hr:Leave:OverlappingLeave');
 const yr=this.lvYear(e);
 if(out.deductsFromAnnualBalance&&x.kind!=='hourly'||x.kind==='hourly'&&hoursDays>0){
  if(!Y||!Y.set){if(seedMode)return null;R(422,'Hr:Leave:EntitlementNotSet')}
  const days=x.kind==='hourly'?hoursDays:out.workingDays;
  const curDays=x.kind==='hourly'?days:out.dayBreakdown.filter(z=>z.isCounted&&z.date<=yr.end).length,futDays=x.kind==='hourly'?0:days-curDays;
  const fc=Math.min(b.carRem,curDays),fe=curDays-fc;const leftCar=b.carRem-fc,leftEnt=Math.max(0,b.entRem-fe);
  const nextEnt=Y.nextEnt!=null?Y.nextEnt:Y.ent;const fcN=Math.min(leftCar+leftEnt,futDays),feN=futDays-fcN;
  const availCur=b.rem,sh1=Math.max(0,fe-Math.max(0,b.entRem))+0;
  out.annualDays=days;out.fromCarriedDays=fc+fcN;out.fromEntitlementDays=fe+feN;
  if(curDays)out.segments.push({yearStartDate:yr.start,yearEndDate:yr.end,isFutureYear:false,startDate:from,endDate:futDays?yr.end:to,workingDays:curDays,fromCarriedDays:fc,fromEntitlementDays:fe});
  if(futDays)out.segments.push({yearStartDate:yr.next,yearEndDate:yr.nextEnd,isFutureYear:true,startDate:yr.next,endDate:to,workingDays:futDays,fromCarriedDays:fcN,fromEntitlementDays:feN});
  const shortCur=Math.max(0,fe-Math.max(0,b.entRem)),shortFut=Math.max(0,feN-nextEnt);const short=shortCur+shortFut;
  out.availableDaysBefore=b.rem;out.remainingDaysAfter=Math.max(0,b.rem-curDays);out.shortageDays=short;out.isBalanceSufficient=short===0;
  if(short>0)issue('Hr:Leave:InsufficientBalance',this.state.lang==='ar'?'الرصيد لا يكفي':'The balance is not sufficient');
 }else if(Y){out.availableDaysBefore=b?b.rem:0;out.remainingDaysAfter=out.availableDaysBefore}
 if(out.requiresAttachment)out.issues.push({code:'Hr:Leave:AttachmentRequired',message:this.state.lang==='ar'?'هذا النوع يتطلب مرفقاً':'This type requires an attachment'});
 out._from=from;out._to=to;out._type=type?type.id:null;return out};
lvMake=(e,x,p,by,onb)=>{const D=this.d4,seg=p.segments,cur=seg.find(s=>!s.isFutureYear),fut=seg.find(s=>s.isFutureYear);
 const r={id:this.nid(),emp:e.id,by,onb,kind:x.kind,date:x.date||null,fromTime:x.fromTime||null,toTime:x.toTime||null,from:p._from,to:p._to,startDate:x.startDate||null,endDate:x.endDate||null,type:p._type,notes:x.notes||null,status:'pendingManager',effect:(p.annualDays>0)?'held':'none',workingDays:p.workingDays,hours:p.requestedHours,quota:p.hourlyQuotaHoursUsed||0,excess:p.hourlyExcessHours||0,annualDays:p.annualDays,cur:{fc:cur?cur.fromCarriedDays:(x.kind==='hourly'?p.fromCarriedDays:0),fe:cur?cur.fromEntitlementDays:(x.kind==='hourly'?p.fromEntitlementDays:0)},fut:{fc:fut?fut.fromCarriedDays:0,fe:fut?fut.fromEntitlementDays:0},segs:seg,atts:(x.attachmentIds||[]).map(id=>D.atts.find(a=>a.id===id)).filter(Boolean),at:new Date().toISOString()};
 D.reqs.push(r);const Y=D.y[e.id];if(Y&&r.effect==='held')Y.led.push({id:this.nid(),type:'hold',bucket:'entitlement',days:-(r.cur.fc+r.cur.fe),reason:null,req:r.id,at:r.at,by:'—'});return r};
route4=(op,x,P,v,ix,R)=>{
 if(!(op in OPR4)) return undefined;
 if(!this.d4) this.mk4();
 const D=this.d4,db=this.db,U=this.UID,meId=U(this.EID[v.p]);
 if(!P.roles.some(r=>OPR4[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,me=E(meId),hasR=r=>P.roles.includes(r);
 const ref=id=>{const e=E(id);return e?{id:e.id,fullName:nmo(e),departmentName:null}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),chk=(o,s)=>{if(o.stamp!==s)R(409,'conflict')};
 const tn=t=>ix?t.en:t.ar,exe=e=>e&&e.roles.includes('executiveDirector');
 const need=(o,c)=>{if(!o)R(404,c||'Hr:Leave:NotFound');return o};
 const target=(id)=>{const e=id?need(E(id),'Hr:Employee:EmployeeNotFound'):me;if(exe(e))R(422,'Hr:Leave:ExecutiveDirectorNotEligible');return e};
 const scope=(e)=>{if(e.id===meId)return;if(hasR('hrManager')||hasR('systemAdmin')||hasR('executiveDirector'))return;if(hasR('departmentManager')&&e.mgr===meId)return;R(404,'Hr:Employee:EmployeeNotFound')};
 const opt=t=>({id:t.id,name:tn(t),isPaid:t.paid,maxDays:t.max,requiresAttachment:t.att,deductsFromAnnualBalance:t.ded});
 const td=t=>({id:t.id,name:tn(t),nameAr:t.ar,nameEn:t.en,isPaid:t.paid,maxDays:t.max,requiresAttachment:t.att,eligibleCategory:t.cat,deductsFromAnnualBalance:t.ded,isActive:t.active,creationTime:t.created,concurrencyStamp:t.stamp});
 const bd=(e)=>{const b=this.lvBal(e),yr=this.lvYear(e),cm=this.todayS().slice(0,7),C=D.cfg;const Y=b?b.Y:null;if(Y&&!Y.stamp)Y.stamp='a';const hb=this.lvHours(e,cm);
  return{employee:ref(e.id),isEntitlementSet:!!(Y&&Y.set),leaveYearId:Y&&Y.set?this.pu('ly'+e.id+yr.start):null,yearStartDate:yr.start,yearEndDate:yr.end,nextYearStartDate:yr.next,currentYearEntitlementDays:Y?Y.ent:0,carriedDays:Y?Y.car:0,adjustmentDays:Y?Y.adj:0,usedDays:b?b.used:0,heldDays:b?b.held:0,remainingDays:b?b.rem:0,carriedRemainingDays:b?b.carRem:0,currentYearRemainingDays:b?Math.max(0,b.entRem):0,futureYearHeldDays:b?b.fut:0,nextYearEntitlementDays:Y&&Y.nextEnt!=null?Y.nextEnt:null,
  hourly:{isConfigured:!!(C.monthly&&C.hpd),month:cm,quotaHours:hb.quota,usedHours:hb.used,heldHours:hb.held,remainingHours:hb.rem,hoursPerDay:C.hpd},
  otherLeaveUsages:D.types.map(t=>{const rs=D.reqs.filter(r=>r.emp===e.id&&r.type===t.id&&!['rejected','cancelled'].includes(r.status));const u=rs.filter(r=>r.status==='approved'||r.status==='completed').reduce((n,r)=>n+r.workingDays,0),h=rs.filter(r=>!(r.status==='approved'||r.status==='completed')).reduce((n,r)=>n+r.workingDays,0);return{leaveTypeId:t.id,leaveTypeName:tn(t),usedDays:u,heldDays:h,maxDays:t.max,remainingDays:t.max==null?null:Math.max(0,t.max-u-h)}}).filter(o=>o.usedDays||o.heldDays||true),concurrencyStamp:Y?Y.stamp:'a'}};
 const ld=(r)=>{const t=D.types.find(z=>z.id===r.type);return{requestId:r.id,employee:ref(r.emp),submittedBy:{id:r.by,displayName:E(r.by)?nmo(E(r.by)):'—'},isOnBehalf:r.onb,kind:r.kind,date:r.date,fromTime:r.fromTime,toTime:r.toTime,startDate:r.startDate,endDate:r.endDate,otherLeaveType:t?opt(t):null,notes:r.notes,status:r.status,balanceEffect:r.effect,workingDays:r.workingDays,requestedHours:r.hours,hourlyQuotaHoursUsed:r.kind==='hourly'?r.quota:null,hourlyExcessHours:r.kind==='hourly'?r.excess:null,annualDays:r.annualDays,fromCarriedDays:r.cur.fc+r.fut.fc,fromEntitlementDays:r.cur.fe+r.fut.fe,segments:r.segs,attachments:r.atts,creationTime:r.at}};
 const lst=(a,q)=>{if(q.status)a=a.filter(r=>r.status===q.status);if(q.kind)a=a.filter(r=>r.kind===q.kind);if(q.fromDate)a=a.filter(r=>r.to>=q.fromDate);if(q.toDate)a=a.filter(r=>r.from<=q.toDate);a=a.slice().sort((p,z)=>z.from.localeCompare(p.from));return{totalCount:a.length,items:pg(a,q).items.map(ld)}};
 const led=(e,q)=>{const Y=D.y[e.id];const yr=this.lvYear(e);let a=(Y?Y.led:[]).slice().reverse();if(q.type)a=a.filter(z=>z.type===q.type);if(q.fromDate)a=a.filter(z=>z.at.slice(0,10)>=q.fromDate);if(q.toDate)a=a.filter(z=>z.at.slice(0,10)<=q.toDate);
  return{totalCount:a.length,items:pg(a,q).items.map(z=>({id:z.id,employeeId:e.id,leaveYearId:this.pu('ly'+e.id+yr.start),yearStartDate:yr.start,yearEndDate:yr.end,type:z.type,bucket:z.bucket,days:z.days,hours:null,requestId:z.req,reason:z.reason,createdBy:z.by&&z.by!=='—'?{id:meId,displayName:z.by}:null,creationTime:z.at}))}};
 switch(op){
 case 'LeaveBalance_GetMy':return bd(target());
 case 'LeaveBalance_GetByEmployee':{const e=target(x.id);scope(e);return bd(e)}
 case 'LeaveLedger_GetMy':return led(target(),x);
 case 'LeaveLedger_GetByEmployee':{const e=target(x.id);scope(e);return led(e,x)}
 case 'LeaveBalance_SetEntitlement':{const e=target(x.id);if(e.id===meId)R(422,'Hr:Leave:SelfAdjustmentNotAllowed');const Y=D.y[e.id];if(!Y)R(422,'Hr:Leave:ExecutiveDirectorNotEligible');if(!Y.stamp)Y.stamp='a';chk(Y,x.concurrencyStamp);
  if(!Y.set){Y.set=true;Y.ent=x.entitlementDays;Y.led.push({id:this.nid(),type:'grant',bucket:'entitlement',days:x.entitlementDays,reason:x.reason||null,req:null,at:new Date().toISOString(),by:me?nmo(me):'—'})}else Y.nextEnt=x.entitlementDays;Y.stamp=stamp();return bd(e)}
 case 'LeaveBalance_Adjust':{const e=target(x.id);if(e.id===meId)R(422,'Hr:Leave:SelfAdjustmentNotAllowed');const Y=D.y[e.id];if(!Y||!Y.set)R(422,'Hr:Leave:EntitlementNotSet');const b=this.lvBal(e);if(x.days<0&&-x.days>b.rem)R(422,'Hr:Leave:AdjustmentExceedsBalance');Y.adj+=x.days;Y.stamp=stamp();Y.led.push({id:this.nid(),type:'adjust',bucket:'entitlement',days:x.days,reason:x.reason,req:null,at:new Date().toISOString(),by:me?nmo(me):'—'});return bd(e)}
 case 'LeaveRequest_Preview':{const e=target(x.employeeId);if(x.employeeId&&x.employeeId!==meId&&!(hasR('departmentManager')&&e.mgr===meId))R(403,'forbidden');const p=this.lvPreview(e,x);const o={...p};delete o._from;delete o._to;delete o._type;return o}
 case 'LeaveRequest_Create':{const e=target(x.employeeId);const onb=!!x.employeeId&&x.employeeId!==meId;if(onb&&!(hasR('departmentManager')&&e.mgr===meId))R(403,'forbidden');
  const ids=x.attachmentIds||[];ids.forEach(id=>{if(!D.atts.some(a=>a.id===id))R(422,'Hr:Leave:InvalidAttachment')});
  const p=this.lvPreview(e,x);if(p.requiresAttachment&&!ids.length)R(422,'Hr:Leave:AttachmentRequired');if(!p.isBalanceSufficient)R(422,'Hr:Leave:InsufficientBalance');
  const r=this.lvMake(e,x,p,meId,onb);r.status=onb?'pendingHr':((hasR('hrManager')||hasR('systemAdmin'))&&!hasR('employee')?'pendingExecutive':'pendingManager');return ld(r)}
 case 'LeaveRequest_ListMy':return lst(D.reqs.filter(r=>r.emp===meId),x);
 case 'LeaveRequest_ListByEmployee':{const e=target(x.id);scope(e);return lst(D.reqs.filter(r=>r.emp===e.id),x)}
 case 'LeaveRequest_Get':{const r=need(D.reqs.find(z=>z.id===x.id));const e=E(r.emp);if(r.emp!==meId&&r.by!==meId&&!hasR('hrManager')&&!hasR('systemAdmin')&&!hasR('executiveDirector')&&!(hasR('departmentManager')&&e.mgr===meId))R(404,'Hr:Leave:NotFound');return ld(r)}
 case 'LeaveRequest_UploadAttachment':{if(!/\.(pdf|jpe?g|png)$/i.test(x.file||''))R(422,'Hr:Leave:AttachmentRejected');if((x.size||0)>10485760)R(422,'Hr:Leave:AttachmentRejected');const a={id:this.nid(),fileName:x.file,contentType:/pdf$/i.test(x.file)?'application/pdf':'image/jpeg',sizeBytes:x.size||120000,creationTime:new Date().toISOString()};D.atts.push(a);return a}
 case 'LeaveRequest_GetAttachmentDownloadUrl':{need(D.reqs.find(z=>z.id===x.id));return{url:'https://files.example.test/'+x.attachmentId,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'OtherLeaveType_List':{let a=D.types;if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(t=>t.active===(x.isActive===true||x.isActive==='true'));return{totalCount:a.length,items:pg(a,x).items.map(td)}}
 case 'OtherLeaveType_Create':{const t={id:this.nid(),ar:x.nameAr,en:x.nameEn,paid:x.isPaid,max:x.maxDays??null,att:x.requiresAttachment,cat:x.eligibleCategory,ded:!!x.deductsFromAnnualBalance,active:x.isActive,stamp:stamp(),created:new Date().toISOString()};D.types.push(t);return td(t)}
 case 'OtherLeaveType_Update':{const t=need(D.types.find(z=>z.id===x.id));chk(t,x.concurrencyStamp);Object.assign(t,{ar:x.nameAr,en:x.nameEn,paid:x.isPaid,max:x.maxDays??null,att:x.requiresAttachment,cat:x.eligibleCategory,ded:!!x.deductsFromAnnualBalance,active:x.isActive,stamp:stamp()});return td(t)}
 case 'OtherLeaveType_ListAvailable':{const e=target(x.employeeId);return{items:D.types.filter(t=>t.active&&(t.cat==='all'||t.cat===e.g)).map(opt)}}
 case 'LeaveCalendar_GetTeam':{if(!x.fromDate||!x.toDate||x.fromDate>x.toDate)R(422,'Hr:Leave:InvalidDateRange');const team=db.emps.filter(e=>e.mgr===meId);let a=D.reqs.filter(r=>team.some(t=>t.id===r.emp)&&!['rejected','cancelled'].includes(r.status)&&!(r.to<x.fromDate||r.from>x.toDate));if(x.employeeId)a=a.filter(r=>r.emp===x.employeeId);if(x.includePending===false||x.includePending==='false')a=a.filter(r=>r.status==='approved'||r.status==='completed');
  a=a.sort((p,z)=>p.from.localeCompare(z.from));return{totalCount:a.length,items:pg(a,x).items.map(r=>({requestId:r.id,employee:ref(r.emp),kind:r.kind,otherLeaveTypeName:r.type?tn(D.types.find(t=>t.id===r.type)):null,startDate:r.from,endDate:r.to,fromTime:r.fromTime,toTime:r.toTime,status:r.status,workingDays:r.workingDays,requestedHours:r.hours}))}}
 case 'SystemSettings_GetHourlyLeave':return{monthlyAllowedHours:D.cfg.monthly,hoursPerDay:D.cfg.hpd,concurrencyStamp:D.cfg.stamp,lastModificationTime:new Date().toISOString()};
 case 'SystemSettings_UpdateHourlyLeave':{chk(D.cfg,x.concurrencyStamp);D.cfg.monthly=x.monthlyAllowedHours;D.cfg.hpd=x.hoursPerDay;D.cfg.stamp=stamp();return{monthlyAllowedHours:D.cfg.monthly,hoursPerDay:D.cfg.hpd,concurrencyStamp:D.cfg.stamp,lastModificationTime:new Date().toISOString()}}
 }
 return undefined};
lvUpload=async(e,k)=>{const fl=e.target.files&&e.target.files[0];if(!fl)return;this.setState({busy:true,derr:''});
 try{const a=await this.call('LeaveRequest_UploadAttachment',{file:fl.name,size:fl.size});this.setState(s=>({busy:false,fv:{...s.fv,[k]:[...(Array.isArray(s.fv[k])?s.fv[k]:[]),a]}}))}catch(er){this.setState({busy:false,derr:(er&&er.code)||'gen'})}};
lvInput=(v)=>{const o={kind:v.kind};if(v.forEmp)o.employeeId=v.forEmp;if(v.kind==='hourly'){o.date=v.date;o.fromTime=v.fromTime;o.toTime=v.toTime}else{o.startDate=v.startDate;o.endDate=v.endDate;if(v.kind==='other')o.otherLeaveTypeId=v.otherLeaveTypeId}return o};
lvEntDlg=(T,e,b)=>({title:T.dEnt,body:T.dEntB,fields:[{k:'d',l:T.fDaysN,req:1,max:6,pat:/^\d+(\.\d+)?$/,init:b.nextYearEntitlementDays!=null?String(b.nextYearEntitlementDays):(b.isEntitlementSet?'':'')},{k:'r',l:T.fRsn,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('LeaveBalance_SetEntitlement',{id:e,entitlementDays:Number(v.d),reason:v.r||undefined,concurrencyStamp:b.concurrencyStamp});return{m:T.tEnt}}});
lvAdjDlg=(T,e,b)=>({title:T.dAdj,body:T.dAdjB,fields:[{k:'d',l:T.fDaysA,req:1,max:7,pat:/^-?\d+(\.\d+)?$/},{k:'r',l:T.fRsn,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('LeaveBalance_Adjust',{id:e,days:Number(v.d),reason:v.r});return{m:T.tAdj}}});
lvTypeDlg=(T,t)=>{const yn=[{v:'true',l:T.yes},{v:'false',l:T.no}];return{title:T.dType,fields:[{k:'nameAr',l:T.fNameAr,req:1,max:128,init:t?t.nameAr:''},{k:'nameEn',l:T.fNameEn,req:1,max:128,init:t?t.nameEn:''},{k:'isPaid',l:T.fPaid,t:'sel',req:1,init:String(t?t.isPaid:true),opts:yn},{k:'maxDays',l:T.fMax,max:6,pat:/^\d+(\.\d+)?$/,init:t&&t.maxDays!=null?String(t.maxDays):''},{k:'requiresAttachment',l:T.fReqAtt,t:'sel',req:1,init:String(t?t.requiresAttachment:false),opts:yn},{k:'eligibleCategory',l:T.fCat,t:'sel',req:1,init:t?t.eligibleCategory:'all',opts:Object.keys(T.cat).map(x=>({v:x,l:T.cat[x]}))},{k:'deductsFromAnnualBalance',l:T.fDed,t:'sel',req:1,init:String(t?t.deductsFromAnnualBalance:false),opts:yn},{k:'isActive',l:T.fAct,t:'sel',req:1,init:String(t?t.isActive:true),opts:yn}],okLabel:T.save2,
 run:async(v)=>{const b={nameAr:v.nameAr,nameEn:v.nameEn,isPaid:v.isPaid==='true',maxDays:v.maxDays?Number(v.maxDays):null,requiresAttachment:v.requiresAttachment==='true',eligibleCategory:v.eligibleCategory,deductsFromAnnualBalance:v.deductsFromAnnualBalance==='true',isActive:v.isActive==='true'};if(t)await this.call('OtherLeaveType_Update',{id:t.id,...b,concurrencyStamp:t.concurrencyStamp});else await this.call('OtherLeaveType_Create',b);return{m:t?T.tSaved:T.tCreated}}}};
lvReqDlg=(T,r,ar,tfn)=>{const dt=x=>this.fmtD(x,ar),n=x=>this.fmtN?this.fmtN(x):x;const per=r.kind==='hourly'?dt(r.date)+' '+r.fromTime+'–'+r.toTime:dt(r.startDate)+' → '+dt(r.endDate);
 return{title:T.rdT,fields:[],kv:[{k:T.rdFor,v:r.employee.fullName},{k:T.rdBy,v:r.submittedBy.displayName+(r.isOnBehalf?' ('+T.rdOnb+')':'')},{k:T.cKind,v:T.lk[r.kind]},{k:T.cReqDate,v:per},...(r.otherLeaveType?[{k:T.cType,v:r.otherLeaveType.name}]:[]),{k:T.cStatus,v:T.rst[r.status]},{k:T.cEffect,v:T.be[r.balanceEffect]},{k:T.cWD,v:String(r.workingDays)},{k:T.pvAnn,v:String(r.annualDays)},{k:T.pvFC,v:String(r.fromCarriedDays)},{k:T.pvFE,v:String(r.fromEntitlementDays)},...(r.requestedHours!=null?[{k:T.pvHrs,v:String(r.requestedHours)},{k:T.pvQ,v:String(r.hourlyQuotaHoursUsed)},{k:T.pvEx,v:String(r.hourlyExcessHours)}]:[]),...r.segments.map(s=>({k:T.pvSeg,v:dt(s.startDate)+' → '+dt(s.endDate)+' · '+s.workingDays+(s.isFutureYear?' ('+T.pvFut+')':'')})),...(r.notes?[{k:T.rdNotes,v:r.notes}]:[]),...r.attachments.map(a=>({k:T.rdAtt,v:a.fileName}))],okLabel:T.cnf,run:null}};
d5=null;
mk5=()=>{
 if(!this.d4)this.mk4();const U=this.UID,db=this.db,D3=this.d3,D4=this.d4;
 const types=[['C1','leave','إجازة','Leave','none',true,false,true],['C2','attendanceCorrection','تصحيح بصمة','Punch correction','none',true,false,true],['C3','administrativeNeed','احتياج إداري','Administrative need','none',false,false,true],['C4','accountantRequest','طلب من المحاسب','Accountant request','accountant',false,false,true],['C5','storeItem','قطعة من المخزن','Store item','store',false,true,true]].map(a=>({id:U(a[0]),code:a[1],ar:a[2],en:a[3],dest:a[4],ded:a[5],item:a[6],active:a[7],sys:['leave','attendanceCorrection','storeItem'].includes(a[1]),stamp:'a',created:'2024-01-01T08:00:00Z'}));
 this.d5={reqs:[],memos:[],incs:[],dels:[],types,seq:0};const D=this.d5;
 const tid=c=>types.find(t=>t.code===c).id,now=Date.now();
 const g=(k,code,desc,st)=>{const e=db.emps.find(z=>z.id===U(k));const r={id:this.nid(),kind:'gen',type:code,emp:e.id,by:e.id,onb:false,desc,status:st,at:new Date(now-864e5).toISOString(),tl:[],reason:null};D.reqs.push(r);return r};
 g('11','administrativeNeed','كراسي إضافية لغرفة الاجتماعات','pendingManager');
 g('18','accountantRequest','طلب سلفة بقيمة 300,000 د.ع','pendingHr');
 g('12','administrativeNeed','طابعة جديدة لقسم العمليات','pendingManager');g('24','administrativeNeed','مروحة لمكتب المبيعات','pendingManager');D.dels.push({id:this.nid(),from:U('23'),to:U('12'),scope:'allApprovals',s:new Date(now-36e5).toISOString(),e:new Date(now+6048e5).toISOString(),reason:'إجازة سنوية',status:'ok',at:new Date(now-864e5).toISOString()});
 const m=(k,kind,content,st,hr)=>{const e=db.emps.find(z=>z.id===U(k));D.memos.push({id:this.nid(),emp:e.id,kind,content,status:st,close:null,convReq:null,convInc:null,tl:[{at:new Date(now-72e5).toISOString(),action:'submitted',actor:e.id,note:null}],at:new Date(now-72e5).toISOString()})};
 m('11','leave','أرغب بإجازة يومين الأسبوع القادم لظرف عائلي','pendingManager');m('20','problem','جهاز الحاسوب في مكتبي يتعطل باستمرار','pendingHr');
 const inc=(k,desc,st)=>{const e=db.emps.find(z=>z.id===U(k));D.incs.push({id:this.nid(),emp:e.id,desc,status:st,srcMemo:null,tl:[{at:new Date(now-9e6).toISOString(),action:'submitted',actor:e.id,note:null}],stamp:'a',at:new Date(now-9e6).toISOString()})};
 inc('11','المكيف في الطابق الثاني لا يعمل','pendingManager');inc('18','انقطاع الإنترنت في قسم المبيعات','pendingHr');
};
attSum=(a)=>({presentDays:a.filter(z=>z.status==='present'||z.status==='late').length,lateDays:a.filter(z=>z.status==='late').length,totalLateMinutes:a.reduce((n,z)=>n+(z.lateMinutes||0),0),absentDays:a.filter(z=>z.status==='absent').length,incompleteDays:a.filter(z=>z.status==='incomplete').length,leaveDays:a.filter(z=>z.status==='onLeave').length,workedMinutes:a.reduce((n,z)=>n+(z.workedMinutes||0),0),overtimeMinutes:a.reduce((n,z)=>n+(z.overtimeMinutes||0),0)});
rqAll=()=>{const D3=this.d3,D4=this.d4,D5=this.d5;return[...D4.reqs.map(s=>({k:'leave',s})),...D3.corr.map(s=>({k:'corr',s})),...D5.reqs.map(s=>({k:'gen',s}))]};
rqType=(r)=>{const c=r.k==='leave'?'leave':r.k==='corr'?'attendanceCorrection':r.s.type;return this.d5.types.find(t=>t.code===c)};
rqHolder=(role)=>this.db.emps.find(e=>e.status==='active'&&e.roles.includes(role));
rqSteps=(r)=>{ /* ensure steps from the approval matrix (contract info.description) */
 const S=r.s;if(S.steps)return S.steps;const db=this.db,e=db.emps.find(z=>z.id===S.emp),dest=this.rqType(r).dest;const has=x=>e.roles.includes(x);
 const mgr=db.emps.find(z=>z.id===e.mgr),hid=x=>(this.rqHolder(x)||{}).id||null;const sp=[];
 const st=(status,role,uid)=>sp.push({order:sp.length+1,st:status,role,uid:uid||null,decision:'notReached',by:null,from:null,at:null,note:null});
 if(has('executiveDirector')){}
 else if(has('hrManager')){if(dest!=='store')st('pendingExecutive','executiveDirector',hid('executiveDirector'))}
 else if(has('systemAdmin')){st('pendingExecutive','executiveDirector',hid('executiveDirector'))}
 else if(has('departmentManager')){st('pendingManager','systemAdmin',hid('systemAdmin'));st('pendingHr','hrManager',hid('hrManager'))}
 else{st('pendingManager','departmentManager',mgr?mgr.id:null);st('pendingHr','hrManager',hid('hrManager'))}
 if(dest!=='none')sp.push({order:sp.length+1,st:'pendingDestination',role:dest==='store'?'storeOfficer':'accountant',uid:null,decision:'notReached',by:null,from:null,at:null,note:null});
 const cur=S.status,idx=sp.findIndex(z=>z.st===cur);
 sp.forEach((z,i)=>{if(['approved','completed'].includes(cur))z.decision=(i===0&&S.onb)?'approvedBySubmission':'approved';
  else if(cur==='rejected'){const k=S.rejIdx!=null?S.rejIdx:0;z.decision=i<k?'approved':(i===k?'rejected':'notReached')}
  else if(cur==='cancelled')z.decision='notReached';
  else if(idx>=0)z.decision=i<idx?((i===0&&S.onb)?'approvedBySubmission':'approved'):(i===idx?'pending':'notReached')});
 S.steps=sp;return sp};
rqCur=(r)=>{const sp=this.rqSteps(r);return sp.find(z=>z.decision==='pending')||null};
rqEmpSt=(s)=>s==='pendingDestination'?'approved':s;
rqDeleg=(userId,scope)=>{const n=Date.now();return this.d5.dels.filter(d=>d.to===userId&&d.status!=='cancelled'&&new Date(d.s).getTime()<=n&&new Date(d.e).getTime()>=n&&(d.scope==='allApprovals'||d.scope===scope))};
rqMine=(r,actorId,actorRoles)=>{ /* returns {ok,from} if the request is assigned to the actor right now */
 const cur=this.rqCur(r);if(!cur)return null;if(r.s.emp===actorId)return null;
 if(cur.uid===actorId)return{from:null,cur};
 if(!cur.uid&&actorRoles.includes(cur.role))return{from:null,cur};
 for(const d of this.rqDeleg(actorId,'requests')){if(cur.uid===d.from&&r.s.emp!==d.from)return{from:d.from,cur}}
 return null};
rqLabel=(st)=>{const L=T4[this.state.lang];return L.rst[st]||st};
rqEmpLabel=(st)=>{const ix=this.state.lang==='en';const m={pendingManager:ix?'Waiting for department manager':'بانتظار مدير القسم',pendingHr:ix?'Waiting for HR':'بانتظار الموارد البشرية',pendingExecutive:ix?'Waiting for executive':'بانتظار المدير التنفيذي',approved:ix?'Approved':'موافق عليه',rejected:ix?'Rejected':'مرفوض',completed:ix?'Completed':'مكتمل',cancelled:ix?'Cancelled':'ملغى'};return m[st]};
rqSummary=(r)=>{const L=T4[this.state.lang],ar=this.state.lang==='ar';
 if(r.k==='leave'){const q=r.s;return L.lk[q.kind]+' · '+(q.kind==='hourly'?q.date+' '+q.fromTime+'–'+q.toTime:q.from+' → '+q.to)}
 if(r.k==='corr'){const c=r.s;return L.ptyp[c.pt]+' · '+c.date+' · '+c.t}
 if(r.s.itemId&&this.d11){const it=this.d11.items.find(z=>z.id===r.s.itemId);if(it)return(this.state.lang==='en'?it.en:it.ar)+' × '+r.s.qty+(r.s.desc?' — '+r.s.desc:'')}return r.s.desc};
rqFinalEffects=(r,approved)=>{
 if(r.k==='leave'){const q=r.s,Y=this.d4.y[q.emp];if(approved){if(q.effect==='held'){q.effect='deducted';if(Y)Y.led.push({id:this.nid(),type:'deduct',bucket:'entitlement',days:-(q.cur.fc+q.cur.fe),reason:null,req:q.id,at:new Date().toISOString(),by:'—'})}}
  else if(q.effect==='held'){q.effect='released';if(Y)Y.led.push({id:this.nid(),type:'release',bucket:'entitlement',days:q.cur.fc+q.cur.fe,reason:null,req:q.id,at:new Date().toISOString(),by:'—'})}}
 if(r.k==='corr'&&approved){const c=r.s;this.d3.applied[c.emp+'|'+c.date]=true}};
rqTl=(r)=>{const S=r.s,db=this.db,ix=this.state.lang==='en';const nm=id=>{const e=db.emps.find(z=>z.id===id);return e?{id:e.id,displayName:ix?e.en:e.ar}:null};
 const tl=[{occurredAt:S.at,action:ix?'Submitted':'تم رفع الطلب',actor:nm(S.by),note:S.onb?(ix?'On behalf of the employee':'نيابة عن الموظف'):null}];
 (S.tl||[]).forEach(t=>tl.push({occurredAt:t.at,action:t.action,actor:nm(t.actor),note:t.note}));return tl};
route5=(op,x,P,v,ix,R)=>{
 if(!(op in OPR5)) return undefined;
 if(!this.d5) this.mk5();
 const D=this.d5,db=this.db,U=this.UID,meId=U(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR5[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),me=E(meId),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const ref=id=>{const e=E(id);return e?{id:e.id,fullName:nmo(e),departmentName:null}:null},uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),now=()=>new Date().toISOString();
 const tn=t=>ix?t.en:t.ar,tref=t=>({id:t.id,code:t.code,name:tn(t),destination:t.dest});
 const need=(o,c)=>{if(!o)R(404,c||'Hr:Request:NotFound');return o};
 const team=()=>db.emps.filter(e=>e.mgr===meId);
 const involved=(r)=>{const S=r.s;if(S.emp===meId||S.by===meId)return true;if(hasR('hrManager')||hasR('systemAdmin')||hasR('executiveDirector'))return true;const e=E(S.emp);if(hasR('departmentManager')&&e&&e.mgr===meId)return true;if(this.rqMine(r,meId,roles))return true;return this.rqSteps(r).some(z=>z.uid===meId||(!z.uid&&roles.includes(z.role)&&z.decision!=='notReached'))};
 const det=(r)=>{this.siEnsure(r.s.id);const S=r.s,t=this.rqType(r),sp=this.rqSteps(r),cur=sp.find(z=>z.decision==='pending'),mine=this.rqMine(r,meId,roles),es=this.rqEmpSt(S.status);
  return{id:S.id,type:tref(t),employee:ref(S.emp),submittedBy:uref(S.by),isOnBehalf:!!S.onb,description:r.k==='gen'?S.desc:null,detailsSummary:this.rqSummary(r),status:S.status,employeeStatus:es,statusLabel:this.rqEmpLabel(es),currentApproverRole:cur?cur.role:null,rejectionReason:S.reason||null,
  steps:sp.map(z=>({order:z.order,approverRole:z.role,approver:z.uid?uref(z.uid):null,decision:z.decision,decidedBy:z.by?uref(z.by):null,delegatedFrom:z.from?uref(z.from):null,decidedAt:z.at,note:z.note})),timeline:this.rqTl(r),stockIssue:this.siStatusDto(S.id),sourceMemoId:S.srcMemo||null,
  canApprove:!!mine&&!(cur&&cur.st==='pendingDestination'&&cur.role==='storeOfficer'),canReject:!!mine,canCancel:(S.emp===meId||S.by===meId)&&['pendingManager','pendingHr','pendingExecutive'].includes(S.status),creationTime:S.at,lastModificationTime:null}};
 const sm=(r)=>{const S=r.s,cur=this.rqSteps(r).find(z=>z.decision==='pending');return{id:S.id,type:tref(this.rqType(r)),employee:ref(S.emp),submittedBy:uref(S.by),isOnBehalf:!!S.onb,detailsSummary:this.rqSummary(r),status:S.status,currentApproverRole:cur?cur.role:null,creationTime:S.at,lastModificationTime:null}};
 const decide=(r,approve,text)=>{const S=r.s,sp=this.rqSteps(r),cur=sp.find(z=>z.decision==='pending');if(!cur)R(422,'Hr:Request:NotPending');if(S.emp===meId)R(422,'Hr:Request:SelfApprovalNotAllowed');
  const mine=this.rqMine(r,meId,roles);if(!mine)R(403,'forbidden');if(approve&&cur.st==='pendingDestination'&&cur.role==='storeOfficer')R(422,'Hr:Request:UseStockIssueFlow');
  const idx=sp.indexOf(cur);cur.by=meId;cur.from=mine.from;cur.at=now();cur.note=text||null;
  if(approve){cur.decision='approved';const nx=sp[idx+1];if(nx){nx.decision='pending';S.status=nx.st}else{S.status=this.rqType(r).dest!=='none'?'completed':'approved';this.rqFinalEffects(r,true)}(S.tl=S.tl||[]).push({at:cur.at,action:ix?'Approved':'تمت الموافقة',actor:meId,note:text||null})}
  else{cur.decision='rejected';S.rejIdx=idx;S.status='rejected';S.reason=text;this.rqFinalEffects(r,false);(S.tl=S.tl||[]).push({at:cur.at,action:ix?'Rejected':'تم الرفض',actor:meId,note:text})}
  return det(r)};
 const mdto=(m)=>{const hdl=m.status==='pendingHr'?'hrManager':m.status==='pendingExecutive'?'executiveDirector':m.status==='pendingManager'?(m.hdl||'departmentManager'):null;return{id:m.id,employee:ref(m.emp),kind:m.kind,content:m.content,status:m.status,statusLabel:T4[this.state.lang].mst?T4[this.state.lang].mst[m.status]:m.status,currentHandlerRole:hdl,closeReason:m.close,convertedRequestId:m.convReq,convertedIncidentId:m.convInc,timeline:m.tl.map(t=>({occurredAt:t.at,action:t.action,actor:uref(t.actor),note:t.note})),creationTime:m.at,lastModificationTime:null}};
 const mli=(m)=>({id:m.id,employee:ref(m.emp),kind:m.kind,contentPreview:m.content.slice(0,80),status:m.status,statusLabel:T4[this.state.lang].mst?T4[this.state.lang].mst[m.status]:m.status,creationTime:m.at});
 const mHandler=(m)=>{const e=E(m.emp);if(m.status==='pendingManager'){if(m.hdl==='systemAdmin')return hasR('systemAdmin');return hasR('departmentManager')&&e.mgr===meId}if(m.status==='pendingHr')return hasR('hrManager');if(m.status==='pendingExecutive')return hasR('executiveDirector');return false};
 const mVis=(m)=>m.emp===meId||mHandler(m)||hasR('hrManager')||hasR('executiveDirector')||hasR('systemAdmin');
 const idto=(i)=>({id:i.id,employee:ref(i.emp),item:i.item?this.stRef(i.item):null,description:i.desc,status:i.status,statusLabel:T4[this.state.lang].ist2?T4[this.state.lang].ist2[i.status]:i.status,currentHandlerRole:i.status==='pendingManager'?'departmentManager':(i.status==='pendingHr'||i.status==='inProgress')?'hrManager':null,sourceMemoId:i.srcMemo,timeline:i.tl.map(t=>({occurredAt:t.at,action:t.action,actor:uref(t.actor),note:t.note})),concurrencyStamp:i.stamp,creationTime:i.at,lastModificationTime:null});
 const ili=(i)=>({id:i.id,employee:ref(i.emp),item:i.item?this.stRef(i.item):null,descriptionPreview:i.desc.slice(0,80),status:i.status,statusLabel:T4[this.state.lang].ist2?T4[this.state.lang].ist2[i.status]:i.status,creationTime:i.at});
 const dst=(d)=>{const n=Date.now();if(d.status==='cancelled')return'cancelled';if(new Date(d.e).getTime()<n)return'expired';if(new Date(d.s).getTime()>n)return'scheduled';return'active'};
 const ddto=(d)=>({id:d.id,fromUser:uref(d.from),toUser:uref(d.to),scope:d.scope,startsAt:d.s,endsAt:d.e,reason:d.reason,status:dst(d),cancelledAt:d.cAt||null,creationTime:d.at});
 const addTl=(o,action,note)=>{o.tl.push({at:now(),action,actor:meId,note:note||null})};
 switch(op){
 case 'RequestType_GetList':{const a=D.types.filter(t=>t.active);return{totalCount:a.length,items:pg(a,x).items.map(t=>({id:t.id,code:t.code,name:tn(t),destination:t.dest,usesDedicatedEndpoint:t.ded,requiresItem:t.item}))}}
 case 'RequestType_GetAdminList':{let a=D.types;if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(t=>t.active===(x.isActive===true||x.isActive==='true'));const dto=t=>({id:t.id,code:t.code,nameAr:t.ar,nameEn:t.en,destination:t.dest,isActive:t.active,isSystem:t.sys,concurrencyStamp:t.stamp,creationTime:t.created,lastModificationTime:null});return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'RequestType_Get':{const t=need(D.types.find(z=>z.id===x.id));return{id:t.id,code:t.code,nameAr:t.ar,nameEn:t.en,destination:t.dest,isActive:t.active,isSystem:t.sys,concurrencyStamp:t.stamp,creationTime:t.created,lastModificationTime:null}}
 case 'RequestType_Create':{if(!/^[a-z][A-Za-z0-9]{1,63}$/.test(x.code))R(400,'validation');if(D.types.some(t=>t.code===x.code))R(422,'Hr:Request:TypeCodeAlreadyExists');const t={id:this.nid(),code:x.code,ar:x.nameAr,en:x.nameEn,dest:x.destination,ded:false,item:false,active:x.isActive,sys:false,stamp:stamp(),created:now()};D.types.push(t);return{id:t.id,code:t.code,nameAr:t.ar,nameEn:t.en,destination:t.dest,isActive:t.active,isSystem:false,concurrencyStamp:t.stamp,creationTime:t.created,lastModificationTime:null}}
 case 'RequestType_Update':{const t=need(D.types.find(z=>z.id===x.id));if(t.stamp!==x.concurrencyStamp)R(409,'conflict');if(t.sys&&t.dest!==x.destination)R(422,'Hr:Request:SystemTypeDestinationLocked');Object.assign(t,{ar:x.nameAr,en:x.nameEn,dest:x.destination,active:x.isActive,stamp:stamp()});return{id:t.id,code:t.code,nameAr:t.ar,nameEn:t.en,destination:t.dest,isActive:t.active,isSystem:t.sys,concurrencyStamp:t.stamp,creationTime:t.created,lastModificationTime:now()}}
 case 'Request_Submit':{const t=need(D.types.find(z=>z.id===x.requestTypeId),'Hr:Request:TypeInactive');if(!t.active)R(422,'Hr:Request:TypeInactive');if(t.ded)R(422,'Hr:Request:UseDedicatedEndpoint');if(hasR('executiveDirector')&&t.dest==='none')R(422,'Hr:Request:ExecutiveCannotSubmit');
  let emp=me;const onb=!!x.onBehalfOfEmployeeId&&x.onBehalfOfEmployeeId!==meId;if(onb){if(!hasR('departmentManager'))R(403,'forbidden');emp=need(E(x.onBehalfOfEmployeeId),'Hr:Employee:EmployeeNotFound');if(emp.mgr!==meId)R(403,'forbidden');if(emp.roles.some(r=>['hrManager','systemAdmin','executiveDirector','departmentManager'].includes(r)))R(422,'Hr:Request:OnBehalfNotAllowed')}
  if(t.item&&(!x.itemId||!(x.quantity>0)))R(422,'Hr:Request:ItemFieldsInvalid');if(t.item){if(!this.d11)this.mk11();const it0=this.d11.items.find(z=>z.id===x.itemId);if(!it0||!it0.active)R(422,'Hr:Request:ItemInactive')}
  if(!emp.roles.some(r=>['hrManager','systemAdmin','executiveDirector','departmentManager'].includes(r))&&!emp.mgr)R(422,'Hr:Request:NoApproverAvailable');
  const has=q=>emp.roles.includes(q);const first=has('executiveDirector')?(t.dest!=='none'?'pendingDestination':null):has('hrManager')?(t.dest==='store'?'pendingDestination':'pendingExecutive'):has('systemAdmin')?'pendingExecutive':'pendingManager';
  const S={id:this.nid(),kind:'gen',type:t.code,emp:emp.id,by:meId,onb,desc:x.description,itemId:t.item?x.itemId:null,qty:t.item?x.quantity:null,status:onb?'pendingHr':first,at:now(),tl:[],reason:null};D.reqs.push(S);const r={k:'gen',s:S};this.rqSteps(r);return det(r)}
 case 'Request_GetMy':{let a=this.rqAll().filter(r=>r.s.emp===meId);if(x.employeeStatus)a=a.filter(r=>this.rqEmpSt(r.s.status)===x.employeeStatus);if(x.requestTypeId)a=a.filter(r=>this.rqType(r).id===x.requestTypeId);a=a.sort((p,q)=>q.s.at.localeCompare(p.s.at));
  return{totalCount:a.length,items:pg(a,x).items.map(r=>{const S=r.s,cur=this.rqSteps(r).find(z=>z.decision==='pending'),es=this.rqEmpSt(S.status);return{id:S.id,type:tref(this.rqType(r)),detailsSummary:this.rqSummary(r),employeeStatus:es,statusLabel:this.rqEmpLabel(es),currentStageRole:cur?cur.role:null,stockIssueStatusLabel:(this.siStatusDto(S.id)||{}).statusLabel||null,rejectionReason:S.reason||null,isOnBehalf:!!S.onb,submittedBy:uref(S.by),creationTime:S.at,lastModificationTime:null}})}}
 case 'Request_GetInbox':{let a=this.rqAll().map(r=>({r,m:this.rqMine(r,meId,roles)})).filter(z=>z.m);if(x.requestTypeId)a=a.filter(z=>this.rqType(z.r).id===x.requestTypeId);if(x.status)a=a.filter(z=>z.r.s.status===x.status);a=a.sort((p,q)=>p.r.s.at.localeCompare(q.r.s.at));
  return{totalCount:a.length,items:pg(a,x).items.map(z=>{const S=z.r.s;return{id:S.id,type:tref(this.rqType(z.r)),employee:ref(S.emp),submittedBy:uref(S.by),isOnBehalf:!!S.onb,detailsSummary:this.rqSummary(z.r),status:S.status,currentApproverRole:z.m.cur.role,actionKind:z.m.cur.st==='pendingDestination'&&z.m.cur.role==='storeOfficer'?'stockIssue':'decide',stockIssueId:(this.d11&&(this.d11.issues.find(q=>q.req===S.id)||{}).id)||null,delegatedFrom:z.m.from?uref(z.m.from):null,waitingSince:S.at,creationTime:S.at}})}}
 case 'Request_GetList':{let a=this.rqAll();if(!hasR('hrManager'))a=a.filter(r=>{const e=E(r.s.emp);return e&&e.mgr===meId});if(x.status)a=a.filter(r=>r.s.status===x.status);if(x.requestTypeId)a=a.filter(r=>this.rqType(r).id===x.requestTypeId);if(x.employeeId)a=a.filter(r=>r.s.emp===x.employeeId);if(x.fromDate)a=a.filter(r=>r.s.at.slice(0,10)>=x.fromDate);if(x.toDate)a=a.filter(r=>r.s.at.slice(0,10)<=x.toDate);a=a.sort((p,q)=>q.s.at.localeCompare(p.s.at));return{totalCount:a.length,items:pg(a,x).items.map(sm)}}
 case 'Request_Get':{const r=need(this.rqAll().find(z=>z.s.id===x.id));if(!involved(r))R(404,'Hr:Request:NotFound');return det(r)}
 case 'Request_Approve':{const r=need(this.rqAll().find(z=>z.s.id===x.id));if(!involved(r))R(404,'Hr:Request:NotFound');return decide(r,true,x.note)}
 case 'Request_Reject':{const r=need(this.rqAll().find(z=>z.s.id===x.id));if(!involved(r))R(404,'Hr:Request:NotFound');return decide(r,false,x.reason)}
 case 'Request_Cancel':{const r=need(this.rqAll().find(z=>z.s.id===x.id));if(!(r.s.emp===meId||r.s.by===meId))R(404,'Hr:Request:NotFound');if(!['pendingManager','pendingHr','pendingExecutive'].includes(r.s.status))R(422,'Hr:Request:CannotCancelAtThisStage');r.s.status='cancelled';this.rqFinalEffects(r,false);(r.s.tl=r.s.tl||[]).push({at:now(),action:ix?'Cancelled':'تم الإلغاء',actor:meId,note:x.note||null});this.rqSteps(r).forEach(z=>{if(z.decision==='pending')z.decision='notReached'});return det(r)}
 // ---- memos
 case 'Memo_Create':{const isDm=hasR('departmentManager'),top=hasR('hrManager')||hasR('systemAdmin');if(!top&&!me.mgr)R(422,'Hr:Memo:NoManagerLinked');
  const m={id:this.nid(),emp:meId,kind:x.kind,content:x.content,status:top?'pendingExecutive':'pendingManager',hdl:isDm&&!top?'systemAdmin':null,close:null,convReq:null,convInc:null,tl:[{at:now(),action:ix?'Submitted':'تم رفع المطالعة',actor:meId,note:null}],at:now()};D.memos.push(m);return mdto(m)}
 case 'Memo_GetMy':{let a=D.memos.filter(m=>m.emp===meId);if(x.status)a=a.filter(m=>m.status===x.status);if(x.kind)a=a.filter(m=>m.kind===x.kind);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(mli)}}
 case 'Memo_GetInbox':{let a=D.memos.filter(m=>m.emp!==meId&&mHandler(m));if(x.status)a=a.filter(m=>m.status===x.status);if(x.kind)a=a.filter(m=>m.kind===x.kind);if(x.employeeId)a=a.filter(m=>m.emp===x.employeeId);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(mli)}}
 case 'Memo_Get':{const m=need(D.memos.find(z=>z.id===x.id),'Hr:Memo:NotFound');if(!mVis(m))R(404,'Hr:Memo:NotFound');return mdto(m)}
 case 'Memo_Forward':{const m=need(D.memos.find(z=>z.id===x.id),'Hr:Memo:NotFound');if(m.emp===meId)R(422,'Hr:Memo:SelfActionNotAllowed');if(!['pendingManager','pendingExecutive'].includes(m.status)||!mHandler(m))R(422,'Hr:Memo:InvalidState');m.status='pendingHr';m.hdl=null;addTl(m,ix?'Forwarded to HR':'حُوّلت إلى الموارد البشرية',x.note);return mdto(m)}
 case 'Memo_Convert':{const m=need(D.memos.find(z=>z.id===x.id),'Hr:Memo:NotFound');if(m.emp===meId)R(422,'Hr:Memo:SelfActionNotAllowed');if(['closed','convertedToRequest','convertedToIncident'].includes(m.status)||!mHandler(m))R(422,'Hr:Memo:InvalidState');
  if((m.kind==='leave')!==(x.target==='leaveRequest'))R(422,'Hr:Memo:ConvertTargetMismatch');const e=E(m.emp);
  if(x.target==='leaveRequest'){const L=x.leave;if(!L)R(422,'Hr:Memo:LeaveDetailsInvalid');if(!this.d4)this.mk4();const inp=L.kind==='hourly'?{kind:'hourly',date:L.leaveDate,fromTime:L.fromTime,toTime:L.toTime}:{kind:L.kind,startDate:L.startDate,endDate:L.endDate,otherLeaveTypeId:L.otherLeaveTypeId};
   if(L.kind==='hourly'?!(L.leaveDate&&L.fromTime&&L.toTime):!(L.startDate&&L.endDate))R(422,'Hr:Memo:LeaveDetailsInvalid');if(L.kind==='other'&&!L.otherLeaveTypeId)R(422,'Hr:Memo:LeaveDetailsInvalid');
   const p=this.lvPreview(e,inp);if(p.requiresAttachment)R(422,'Hr:Memo:LeaveAttachmentRequired');if(!p.isBalanceSufficient)R(422,'Hr:Leave:InsufficientBalance');
   const lr=this.lvMake(e,inp,p,meId,true);lr.status=hasR('executiveDirector')&&m.status==='pendingExecutive'?'pendingExecutive':'pendingHr';lr.srcMemo=m.id;m.convReq=lr.id;m.status='convertedToRequest'}
  else{const i={id:this.nid(),emp:m.emp,desc:m.content,status:'pendingHr',srcMemo:m.id,tl:[{at:now(),action:ix?'Created from memo':'أُنشئ من مطالعة',actor:meId,note:x.note||null}],stamp:stamp(),at:now()};D.incs.push(i);m.convInc=i.id;m.status='convertedToIncident'}
  m.hdl=null;addTl(m,ix?'Converted':'تم التحويل',x.note);return mdto(m)}
 case 'Memo_Close':{const m=need(D.memos.find(z=>z.id===x.id),'Hr:Memo:NotFound');if(m.emp===meId)R(422,'Hr:Memo:SelfActionNotAllowed');if(['closed','convertedToRequest','convertedToIncident'].includes(m.status)||!mHandler(m))R(422,'Hr:Memo:InvalidState');m.status='closed';m.close=x.reason;m.hdl=null;addTl(m,ix?'Closed':'أُغلقت',x.reason);return mdto(m)}
 // ---- incidents
 case 'Incident_Create':{const top=hasR('hrManager')||hasR('systemAdmin')||hasR('executiveDirector');const dm=hasR('departmentManager');if(!top&&!dm&&!me.mgr)R(422,'Hr:Incident:NoManagerLinked');const i={id:this.nid(),emp:meId,desc:x.description,item:x.itemId||null,status:(top||dm)?'pendingHr':'pendingManager',srcMemo:null,tl:[{at:now(),action:ix?'Submitted':'تم رفع البلاغ',actor:meId,note:null}],stamp:stamp(),at:now()};D.incs.push(i);return idto(i)}
 case 'Incident_GetMy':{let a=D.incs.filter(i=>i.emp===meId);if(x.status)a=a.filter(i=>i.status===x.status);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(ili)}}
 case 'Incident_GetInbox':{let a=D.incs.filter(i=>i.emp!==meId&&((hasR('hrManager')&&['pendingHr','inProgress','resolved'].includes(i.status))||(hasR('departmentManager')&&i.status==='pendingManager'&&(E(i.emp)||{}).mgr===meId)));if(x.status)a=a.filter(i=>i.status===x.status);if(x.employeeId)a=a.filter(i=>i.emp===x.employeeId);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(ili)}}
 case 'Incident_Get':{const i=need(D.incs.find(z=>z.id===x.id),'Hr:Incident:NotFound');const e=E(i.emp);if(!(i.emp===meId||hasR('hrManager')||hasR('systemAdmin')||hasR('executiveDirector')||(hasR('departmentManager')&&e.mgr===meId)))R(404,'Hr:Incident:NotFound');return idto(i)}
 case 'Incident_UpdateStatus':{const i=need(D.incs.find(z=>z.id===x.id),'Hr:Incident:NotFound');if(i.stamp!==x.concurrencyStamp)R(409,'conflict');const ok2=(i.status==='pendingHr'&&x.status==='inProgress')||(i.status==='inProgress'&&x.status==='resolved')||(i.status==='pendingHr'&&x.status==='resolved');if(!ok2)R(422,'Hr:Incident:InvalidTransition');i.status=x.status;i.stamp=stamp();addTl(i,x.status==='resolved'?(ix?'Resolved':'تم الحل'):(ix?'In progress':'قيد المعالجة'),x.note);return idto(i)}
 case 'Incident_Forward':{const i=need(D.incs.find(z=>z.id===x.id),'Hr:Incident:NotFound');const e=E(i.emp);if(!e||e.mgr!==meId)R(404,'Hr:Incident:NotFound');if(i.status!=='pendingManager')R(422,'Hr:Incident:InvalidState');i.status='pendingHr';i.stamp=stamp();addTl(i,ix?'Forwarded to HR':'حُوّل إلى الموارد البشرية',x.note);return idto(i)}
 // ---- delegations
 case 'Delegation_GetCandidates':{let a=db.emps.filter(e=>e.status==='active'&&e.id!==meId&&e.acc&&e.roles.some(r=>['departmentManager','hrManager','executiveDirector','systemAdmin','accountant'].includes(r)));if(x.search)a=a.filter(e=>(e.ar+e.en).toLowerCase().includes(String(x.search).toLowerCase()));return{totalCount:a.length,items:pg(a,x).items.map(e=>uref(e.id))}}
 case 'Delegation_Create':{if(x.toUserId===meId)R(422,'Hr:Delegation:CannotDelegateToSelf');const s=new Date(x.startsAt).getTime(),e2=new Date(x.endsAt).getTime();if(!(e2>s)||e2<Date.now())R(422,'Hr:Delegation:InvalidPeriod');const to=E(x.toUserId);if(!to||to.status!=='active'||!to.roles.some(r=>['departmentManager','hrManager','executiveDirector','systemAdmin','accountant'].includes(r)))R(422,'Hr:Delegation:DelegateNotEligible');
  if(D.dels.some(d=>d.from===meId&&d.scope===x.scope&&d.status!=='cancelled'&&!(new Date(d.e).getTime()<s||new Date(d.s).getTime()>e2)))R(422,'Hr:Delegation:OverlappingDelegation');const d={id:this.nid(),from:meId,to:x.toUserId,scope:x.scope,s:x.startsAt,e:x.endsAt,reason:x.reason,status:'ok',at:now()};D.dels.push(d);return ddto(d)}
 case 'Delegation_GetMy':{let a=D.dels.filter(d=>x.direction==='given'?d.from===meId:x.direction==='received'?d.to===meId:(d.from===meId||d.to===meId));if(x.status)a=a.filter(d=>dst(d)===x.status);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(ddto)}}
 case 'Delegation_List':{let a=D.dels.slice().reverse();if(x.fromUserId)a=a.filter(d=>d.from===x.fromUserId);if(x.toUserId)a=a.filter(d=>d.to===x.toUserId);if(x.status)a=a.filter(d=>dst(d)===x.status);return{totalCount:a.length,items:pg(a,x).items.map(ddto)}}
 case 'Delegation_Cancel':{const d=need(D.dels.find(z=>z.id===x.id),'Hr:Delegation:NotFound');if(d.from!==meId)R(404,'Hr:Delegation:NotFound');if(['expired','cancelled'].includes(dst(d)))R(422,'Hr:Delegation:AlreadyEnded');d.status='cancelled';d.cAt=now();return null}
 }
 return undefined};
qNewDlg=(T,types,team,items)=>({title:T.dQNew,body:T.dQNewB,fields:[{k:'t',l:T.fQType,t:'sel',req:1,opts:types.filter(z=>!z.usesDedicatedEndpoint).map(z=>({v:z.id,l:z.name}))},...(team.length?[{k:'f',l:T.fQFor,t:'sel',opts:team}]:[]),{k:'i',l:T.fQItem,t:'sel',opts:(items||[]).map(z=>({v:z.id,l:z.name}))},{k:'q',l:T.fQQty,max:9,pat:/^\d+(\.\d+)?$/},{k:'d',l:T.fQDesc,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{const ty=types.find(z=>z.id===v.t);const st=ty&&ty.requiresItem;await this.call('Request_Submit',{requestTypeId:v.t,onBehalfOfEmployeeId:v.f||undefined,description:v.d,itemId:st?(v.i||undefined):undefined,quantity:st&&v.q?Number(v.q):undefined});return{m:T.tQSub,go:()=>this.toSub('list',null,{tab:'my'})}}});
qDecDlg=(T,id,approve)=>({title:approve?T.dQApp:T.dQRej,body:approve?'':T.dQRejB,fields:[approve?{k:'n',l:T.fQNote,t:'ta',max:2000}:{k:'n',l:T.fQRsn,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:!approve,run:async(v)=>{if(approve)await this.call('Request_Approve',{id,note:v.n||undefined});else await this.call('Request_Reject',{id,reason:v.n});return{m:approve?T.tQApp:T.tQRej}}});
qCanDlg=(T,id)=>({title:T.dQCan,body:T.dQCanB,fields:[{k:'n',l:T.fQNote,t:'ta',max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Request_Cancel',{id,note:v.n||undefined});return{m:T.tQCan}}});
qMemoDlg=(T)=>({title:T.dQMemo,fields:[{k:'k',l:T.fQKind,t:'sel',req:1,opts:Object.keys(T.mk).map(x=>({v:x,l:T.mk[x]}))},{k:'c',l:T.fQCont,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{await this.call('Memo_Create',{kind:v.k,content:v.c});return{m:T.tMS,go:()=>this.toSub('list',null,{tab:'memos'})}}});
qIncDlg=(T,items)=>({title:T.dQInc,body:T.dQIncB,fields:[{k:'i',l:T.fQItem,t:'sel',opts:(items||[]).map(z=>({v:z.id,l:z.name}))},{k:'d',l:T.fQDescI,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{await this.call('Incident_Create',{itemId:v.i||undefined,description:v.d});return{m:T.tIS,go:()=>this.toSub('list',null,{tab:'inc'})}}});
qNoteDlg=(T,title,op,id,msg,req)=>({title,fields:[{k:'n',l:req?T.fQRsn:T.fQNote,t:'ta',req:req?1:0,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call(op,req?{id,reason:v.n}:{id,note:v.n||undefined});return{m:msg}}});
qConvDlg=(T,m,av)=>({title:T.dQConv,fields:[{k:'tg',l:T.fQTarget,t:'sel',req:1,init:m.kind==='leave'?'leaveRequest':'incident',opts:Object.keys(T.mtg).map(x=>({v:x,l:T.mtg[x]}))},{k:'lk',l:T.fQLK,t:'sel',init:'annual',opts:Object.keys(T.lk).map(x=>({v:x,l:T.lk[x]}))},{k:'ot',l:T.fOther,t:'sel',opts:(av||[]).map(z=>({v:z.id,l:z.name}))},{k:'sd',l:T.fStartD,t:'date'},{k:'ed',l:T.fEndD,t:'date'},{k:'ld',l:T.fQLDate,t:'date'},{k:'ft',l:T.fFromT,max:5,pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'tt',l:T.fToT,max:5,pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'n',l:T.fQNote,t:'ta',max:2000}],okLabel:T.cnf,
 run:async(v)=>{const body={target:v.tg,note:v.n||undefined};if(v.tg==='leaveRequest'){const L={kind:v.lk};if(v.lk==='hourly'){L.leaveDate=v.ld||undefined;L.fromTime=v.ft||undefined;L.toTime=v.tt||undefined}else{L.startDate=v.sd||undefined;L.endDate=v.ed||undefined;if(v.lk==='other')L.otherLeaveTypeId=v.ot||undefined}body.leave=L}await this.call('Memo_Convert',{id:m.id,...body});return{m:T.tQConv}}});
qDelDlg=(T,cands)=>{const td=this.todayS();return{title:T.dQDel,body:T.dQDelB,fields:[{k:'to',l:T.fQTo,t:'sel',req:1,opts:cands.map(z=>({v:z.id,l:z.displayName}))},{k:'sc',l:T.fQScp,t:'sel',req:1,init:'allApprovals',opts:Object.keys(T.dscp).map(x=>({v:x,l:T.dscp[x]}))},{k:'sd',l:T.fQSD,t:'date',req:1,init:td},{k:'st',l:T.fQST,req:1,max:5,init:'08:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'ed',l:T.fQED,t:'date',req:1,init:this.dAdd(td,3)},{k:'et',l:T.fQET,req:1,max:5,init:'17:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'r',l:T.fQDR,t:'ta',req:1,max:2000}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Delegation_Create',{toUserId:v.to,scope:v.sc,startsAt:new Date(v.sd+'T'+v.st+':00').toISOString(),endsAt:new Date(v.ed+'T'+v.et+':00').toISOString(),reason:v.r});return{m:T.tDS}}}};
qTypeDlg=(T,t)=>({title:T.dQType,fields:[...(t?[]:[{k:'c',l:T.fQCode,req:1,max:64,pat:/^[a-z][A-Za-z0-9]{1,63}$/}]),{k:'nameAr',l:T.fNameAr,req:1,max:128,init:t?t.nameAr:''},{k:'nameEn',l:T.fNameEn,req:1,max:128,init:t?t.nameEn:''},{k:'d',l:T.fQDst,t:'sel',req:1,init:t?t.destination:'none',opts:Object.keys(T.dest).map(x=>({v:x,l:T.dest[x]}))},{k:'a',l:T.fAct,t:'sel',req:1,init:String(t?t.isActive:true),opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]}],okLabel:T.save2,
 run:async(v)=>{if(t)await this.call('RequestType_Update',{id:t.id,nameAr:v.nameAr,nameEn:v.nameEn,destination:v.d,isActive:v.a==='true',concurrencyStamp:t.concurrencyStamp});else await this.call('RequestType_Create',{code:v.c,nameAr:v.nameAr,nameEn:v.nameEn,destination:v.d,isActive:v.a==='true'});return{m:t?T.tSaved:T.tCreated}}});
d6=null;
mk6=()=>{
 if(!this.db)this.mkDb();if(!this.d3)this.mk3();const U=this.UID,db=this.db,now=Date.now();
 this.d6={cs:[],asg:[],cfg:{delay:10,interval:2,stamp:'a'},seq:0};const D=this.d6;
 const mk=(cm,kind,resp,level,status,subject,content,ago,extra)=>{const c={id:this.nid(),ref:'CMP-2026-'+String(++D.seq).padStart(4,'0'),cm:U(cm),kind,resp:resp?U(resp):null,level,minLevel:level,status,hdl:null,subject,content,atts:[],actions:[],at:new Date(now-ago*864e5).toISOString(),closedAt:null,decision:null,stamp:'a',log:[],...(extra||{})};
  c.cat=c.resp?this.cmpCat(db.emps.find(e=>e.id===c.resp)):null;const h=this.cmpFirst(c);if(c.status!=='closed'){c.status=h.status;c.hdl=h.user}c.actions.push({id:this.nid(),type:'submitted',actor:c.cm,note:null,at:c.at});D.cs.push(c);return c};
 mk('18','employee','20','normal','x','انتهاك لقواعد المكتب','تفاصيل الشكوى (بيانات تجريبية)',3);
 mk('11','employee','12','confidential','x','سلوك غير لائق من المدير','تفاصيل سرية (بيانات تجريبية)',2);
 mk('19','general',null,'highlyConfidential','x','مخالفة جسيمة عامة','تفاصيل سرية جداً (بيانات تجريبية)',1);
 mk('20','employee','18','normal','x','تأخر في تسليم العمل','تفاصيل (بيانات تجريبية)',22);
 mk('24','employee','11','normal','x','خلاف مع زميل','تفاصيل (بيانات تجريبية)',1);
 const cl=mk('11','employee','19','normal','x','طلب توضيح','تفاصيل (بيانات تجريبية)',30);cl.status='closed';cl.hdl=null;cl.closedAt=new Date(now-20*864e5).toISOString();cl.decision='تمت المعالجة وإفهام الأطراف';cl.actions.push({id:this.nid(),type:'closed',actor:U('13'),note:cl.decision,at:cl.closedAt});
};
cmpCat=(e)=>e.roles.includes('hrManager')?'hrManager':e.roles.includes('reviewer')?'reviewer':e.roles.includes('departmentManager')?'departmentManager':e.roles.includes('storeOfficer')?'storeOfficer':'colleague';
LV=['normal','confidential','highlyConfidential'];
cmpRoute=(kind,resp,cmRoles)=>{ /* ComplaintRoutingService: rules fixed in code (requirements 5.8) */
 const cat=resp?this.cmpCat(resp):'colleague';const dm=cmRoles.includes('departmentManager');let min='normal',routes;
 const nrm=dm?['hrManager']:['departmentManager','hrManager'];
 if(cat==='colleague')routes={normal:nrm,confidential:['hrManager'],highlyConfidential:['executiveDirector']};
 else if(cat==='departmentManager'){min='confidential';routes={confidential:['hrManager'],highlyConfidential:['executiveDirector']}}
 else if(cat==='storeOfficer')routes={normal:['hrManager'],confidential:['hrManager'],highlyConfidential:['executiveDirector']};
 else{min='highlyConfidential';routes={highlyConfidential:['executiveDirector']}}
 const sel=this.LV.filter(l=>this.LV.indexOf(l)>=this.LV.indexOf(min));return{min,sel,routes:sel.map(l=>({level:l,handlerPath:routes[l]})),cat}};
cmpHolder=(role)=>this.db.emps.find(e=>e.status==='active'&&e.roles.includes(role));
cmpFirst=(c)=>{const db=this.db,cmE=db.emps.find(e=>e.id===c.cm),resp=c.resp?db.emps.find(e=>e.id===c.resp):null;const r=this.cmpRoute(c.kind,resp,cmE.roles);const path=(r.routes.find(x=>x.level===c.level)||r.routes[0]).handlerPath;const f=path[0];
 if(f==='departmentManager'){const m=db.emps.find(e=>e.id===cmE.mgr);return{status:'pendingDepartmentManager',user:m?m.id:null}}
 if(f==='hrManager')return{status:'pendingHrManager',user:(this.cmpHolder('hrManager')||{}).id||null};
 return{status:'pendingExecutiveDirector',user:(this.cmpHolder('executiveDirector')||{}).id||null}};
cmpAsgOk=(userId,level)=>{const D=this.d6,n=Date.now(),e=this.db.emps.find(z=>z.id===userId);if(!e||e.roles.includes('systemAdmin')||e.roles.includes('reviewer'))return false;if(level==='normal'&&e.roles.includes('executiveDirector'))return false;if(level!=='normal'&&e.roles.includes('departmentManager')&&!e.roles.includes('hrManager')&&!e.roles.includes('executiveDirector'))return false;
 return D.asg.some(a=>a.user===userId&&a.level===level&&a.status==='approved'&&new Date(a.from).getTime()<=n&&(!a.to||new Date(a.to).getTime()>=n))};
cmpCan=(c,uid,roles)=>{ /* ComplaintAccessPolicy */
 if(c.cm===uid||c.resp===uid)return false;if(roles.includes('systemAdmin'))return false;
 if(this.cmpAsgOk(uid,c.level))return true;
 if(c.status==='pendingDepartmentManager')return roles.includes('departmentManager')&&c.hdl===uid&&c.level==='normal';
 if(c.status==='pendingHrManager')return roles.includes('hrManager');
 if(c.status==='pendingExecutiveDirector')return roles.includes('executiveDirector');
 if(c.status==='closed')return(c.level!=='highlyConfidential'&&roles.includes('hrManager'))||(c.level==='highlyConfidential'&&roles.includes('executiveDirector'));
 return false};
cmpDue=(c)=>{let d=c.at.slice(0,10),n=this.d6.cfg.delay;while(n>0){d=this.dAdd(d,1);const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];if(!this.d3.weekly.days.includes(dn))n--}return d+'T23:59:00.000Z'};
cmpLog=(c,uid,action)=>{c.log.push({id:this.nid(),user:uid,action,at:new Date().toISOString()})};
route6=(op,x,P,v,ix,R)=>{
 if(!(op in OPR6)) return undefined;
 if(!this.d6) this.mk6();
 const D=this.d6,db=this.db,U=this.UID,meId=U(this.EID[v.p]),roles=P.roles;
 const E=id=>db.emps.find(e=>e.id===id),me=E(meId),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id);if(!e)return null;const d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const roleOk=(letters)=>roles.some(r=>letters.includes(this.AB3[r]));
 const NF='Hr:Complaints:NotFound';
 const settings=op.indexOf('SystemSettings')===0;
 if(settings&&!hasR('systemAdmin'))R(403,'forbidden');
 if(op==='ComplaintHandler_GetList'&&!roleOk('hx'))R(403,'forbidden');
 if(/ComplaintHandler_(Approve|Reject|Revoke)/.test(op)&&!hasR('executiveDirector'))R(403,'forbidden');
 const isLate=(c)=>c.status!=='closed'&&new Date(this.cmpDue(c)).getTime()<Date.now();
 // ----- id-based ops: every permission failure is 404 (existence never revealed)
 const byId=()=>D.cs.find(z=>z.id===x.id);
 const hasAsg=()=>D.asg.some(a=>a.user===meId&&a.status==='approved');
 const handlerAllowed=(letters)=>roleOk(letters)||hasAsg();
 const forHandler=(letters)=>{if(!handlerAllowed(letters))R(404,NF);const c=byId();if(!c||!this.cmpCan(c,meId,roles))R(404,NF);return c};
 const closedChk=(c)=>{if(c.status==='closed')R(422,'Hr:Complaints:ComplaintClosed')};
 const stampChk=(c,s)=>{if(c.stamp!==s)R(409,'conflict')};
 const attDto=(a)=>({id:a.id,fileName:a.fileName,contentType:a.contentType,sizeBytes:a.sizeBytes,creationTime:a.creationTime});
 const actDto=(a)=>({id:a.id,type:a.type,actor:uref(a.actor),note:a.note,creationTime:a.at});
 const trans=(c,callerKeeps)=>({id:c.id,level:c.level,status:c.status,closedAt:c.closedAt,callerRetainsAccess:callerKeeps,concurrencyStamp:c.stamp});
 const asgDto=(a)=>({id:a.id,user:uref(a.user),level:a.level,status:a.status,from:a.from,to:a.to||null,reason:a.reason,requestedBy:uref(a.by),requestedAt:a.at,decidedBy:a.decBy?uref(a.decBy):null,decidedAt:a.decAt||null,decisionReason:a.decReason||null,concurrencyStamp:a.stamp});
 const respRule=(kind,respId)=>{if(kind==='employee'&&!respId)R(422,'Hr:Complaints:RespondentRequired');if(kind==='general'&&respId)R(422,'Hr:Complaints:RespondentNotAllowed');let rs=null;if(respId){rs=E(respId);if(!rs)R(422,'Hr:Complaints:RespondentNotAllowed');if(rs.id===meId)R(422,'Hr:Complaints:SelfComplaintNotAllowed');if(rs.status!=='active'||rs.roles.includes('executiveDirector')||rs.roles.includes('systemAdmin'))R(422,'Hr:Complaints:RespondentNotAllowed')}return rs};
 switch(op){
 case 'Complaint_GetRespondents':{if(!roleOk('ed'))R(403,'forbidden');let a=db.emps.filter(e=>e.status==='active'&&e.id!==meId&&!e.roles.includes('executiveDirector')&&!e.roles.includes('systemAdmin'));if(x.filter)a=a.filter(e=>(e.ar+e.en).toLowerCase().includes(String(x.filter).toLowerCase()));return{totalCount:a.length,items:pg(a,x).items.map(e=>eref(e.id))}}
 case 'Complaint_PreviewRouting':{if(!roleOk('ed'))R(403,'forbidden');const rs=respRule(x.targetKind,x.respondentId);const r=this.cmpRoute(x.targetKind,rs,roles);return{minimumLevel:r.min,selectableLevels:r.sel,routes:r.routes}}
 case 'Complaint_Submit':{if(!roleOk('ed'))R(403,'forbidden');const rs=respRule(x.targetKind,x.respondentId);const r=this.cmpRoute(x.targetKind,rs,roles);const raised=this.LV.indexOf(x.level)<this.LV.indexOf(r.min);const level=raised?r.min:x.level;
  const c={id:this.nid(),ref:'CMP-2026-'+String(++D.seq).padStart(4,'0'),cm:meId,kind:x.targetKind,resp:rs?rs.id:null,cat:rs?this.cmpCat(rs):null,level,minLevel:r.min,status:'x',hdl:null,subject:x.subject,content:x.content,atts:[],actions:[],at:nowIso(),closedAt:null,decision:null,stamp:stamp(),log:[]};
  const h=this.cmpFirst(c);if(!h.user||h.user===meId||h.user===c.resp)R(422,'Hr:Complaints:NoEligibleHandler');c.status=h.status;c.hdl=h.user;c.actions.push({id:this.nid(),type:'submitted',actor:meId,note:null,at:c.at});D.cs.push(c);
  return{id:c.id,referenceNumber:c.ref,level:c.level,levelRaisedBySystem:raised,status:c.status,submittedAt:c.at}}
 case 'Complaint_GetMyList':{if(!roleOk('ed'))return{totalCount:0,items:[]};let a=D.cs.filter(c=>c.cm===meId);if(x.status)a=a.filter(c=>c.status===x.status);if(x.level)a=a.filter(c=>c.level===x.level);a=a.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(c=>({id:c.id,referenceNumber:c.ref,subject:c.subject,level:c.level,status:c.status,submittedAt:c.at,closedAt:c.closedAt}))}}
 case 'Complaint_GetMyDetail':{if(!roleOk('ed'))R(404,NF);const c=byId();if(!c||c.cm!==meId)R(404,NF);this.cmpLog(c,meId,'viewDetail');return{id:c.id,referenceNumber:c.ref,targetKind:c.kind,respondent:c.resp?eref(c.resp):null,level:c.level,status:c.status,subject:c.subject,content:c.content,attachments:c.atts.filter(a=>a.by===meId).map(attDto),submittedAt:c.at,closedAt:c.closedAt,concurrencyStamp:c.stamp}}
 case 'Complaint_GetAssignedList':{if(!handlerAllowed('dhx'))return{totalCount:0,items:[]};let a=D.cs.filter(c=>this.cmpCan(c,meId,roles));if(x.status)a=a.filter(c=>c.status===x.status);if(x.level)a=a.filter(c=>c.level===x.level);if(x.isLate!==undefined&&x.isLate!=='')a=a.filter(c=>isLate(c)===(x.isLate===true||x.isLate==='true'));a=a.slice().reverse();
  return{totalCount:a.length,items:pg(a,x).items.map(c=>({id:c.id,referenceNumber:c.ref,level:c.level,status:c.status,submittedAt:c.at,dueAt:c.status==='closed'?null:this.cmpDue(c),isLate:isLate(c),closedAt:c.closedAt}))}}
 case 'Complaint_GetDetail':{const c=forHandler('dhx');this.cmpLog(c,meId,'viewDetail');return{id:c.id,referenceNumber:c.ref,complainant:eref(c.cm),targetKind:c.kind,respondent:c.resp?eref(c.resp):null,respondentCategory:c.cat,level:c.level,minimumLevel:c.minLevel,status:c.status,handler:c.hdl?uref(c.hdl):null,subject:c.subject,content:c.content,attachments:c.atts.map(attDto),actions:c.actions.map(actDto),submittedAt:c.at,dueAt:c.status==='closed'?null:this.cmpDue(c),isLate:isLate(c),closedAt:c.closedAt,closureDecision:c.decision,concurrencyStamp:c.stamp}}
 case 'Complaint_RaiseLevel':{const c=byId();if(!c)R(404,NF);const own=c.cm===meId&&roleOk('ed');if(!own){if(!handlerAllowed('edhx')||!this.cmpCan(c,meId,roles))R(404,NF)}closedChk(c);stampChk(c,x.concurrencyStamp);if(this.LV.indexOf(x.level)<=this.LV.indexOf(c.level))R(422,'Hr:Complaints:LevelNotHigher');
  c.level=x.level;const h=this.cmpFirst(c);c.status=h.status;c.hdl=h.user;c.stamp=stamp();c.actions.push({id:this.nid(),type:'levelRaised',actor:meId,note:x.reason||null,at:nowIso()});this.cmpLog(c,meId,'raiseLevel');return trans(c,own?true:this.cmpCan(c,meId,roles))}
 case 'Complaint_AddAction':{const c=forHandler('dhx');closedChk(c);const a={id:this.nid(),type:x.type,actor:meId,note:x.note,at:nowIso()};c.actions.push(a);c.stamp=stamp();this.cmpLog(c,meId,'addAction');return actDto(a)}
 case 'Complaint_Forward':{if(!roleOk('d'))R(404,NF);const c=forHandler('d');closedChk(c);stampChk(c,x.concurrencyStamp);if(c.status!=='pendingDepartmentManager'||c.level!=='normal')R(422,'Hr:Complaints:InvalidTransition');c.status='pendingHrManager';c.hdl=(this.cmpHolder('hrManager')||{}).id||null;c.stamp=stamp();c.actions.push({id:this.nid(),type:'forwarded',actor:meId,note:x.note||null,at:nowIso()});this.cmpLog(c,meId,'forward');return trans(c,false)}
 case 'Complaint_Close':{if(!handlerAllowed('hx'))R(404,NF);const c=forHandler('hx');closedChk(c);stampChk(c,x.concurrencyStamp);const asg=this.cmpAsgOk(meId,c.level);if(!asg&&!((c.level!=='highlyConfidential'&&hasR('hrManager'))||(c.level==='highlyConfidential'&&hasR('executiveDirector'))))R(404,NF);
  c.status='closed';c.closedAt=nowIso();c.decision=x.decision;c.hdl=null;c.stamp=stamp();c.actions.push({id:this.nid(),type:'closed',actor:meId,note:x.decision,at:c.closedAt});this.cmpLog(c,meId,'close');return trans(c,this.cmpCan(c,meId,roles))}
 case 'Complaint_GetAccessLog':{const c=forHandler('dhx');this.cmpLog(c,meId,'viewAccessLog');const a=c.log.slice().reverse();return{totalCount:a.length,items:pg(a,x).items.map(z=>({id:z.id,user:uref(z.user),action:z.action,at:z.at}))}}
 case 'Complaint_UploadAttachment':{const c=byId();if(!c)R(404,NF);const own=c.cm===meId&&roleOk('ed');if(!own){if(!handlerAllowed('edhx')||!this.cmpCan(c,meId,roles))R(404,NF)}closedChk(c);
  if((x.size||0)>10485760)R(422,'Hr:Complaints:AttachmentTooLarge');if(!/\.(pdf|jpe?g|png)$/i.test(x.file||''))R(422,'Hr:Document:FileTypeNotAllowed');if(/virus|infected/i.test(x.file))R(422,'Hr:Complaints:AttachmentRejected');
  const a={id:this.nid(),fileName:x.file,contentType:/pdf$/i.test(x.file)?'application/pdf':'image/jpeg',sizeBytes:x.size||120000,creationTime:nowIso(),by:meId};c.atts.push(a);this.cmpLog(c,meId,'uploadAttachment');return attDto(a)}
 case 'Complaint_DownloadAttachment':{const c=byId();if(!c)R(404,NF);const a=c.atts.find(z=>z.id===x.attachmentId);const own=c.cm===meId&&roleOk('ed');
  if(own){if(!a||a.by!==meId)R(404,NF)}else{if(!handlerAllowed('edhx')||!this.cmpCan(c,meId,roles)||!a)R(404,NF)}this.cmpLog(c,meId,'downloadAttachment');return{fileName:a.fileName,contentType:a.contentType,sizeBytes:a.sizeBytes,content:'(decrypted by the server and streamed directly; no shared temporary link)'}}
 // ----- handler assignments (no complaint content)
 case 'ComplaintHandler_Request':{if(!hasR('hrManager'))R(403,'forbidden');const u=E(x.userId);if(!u||u.status!=='active')R(422,'Hr:Complaints:HandlerRoleNotAllowed');if(u.roles.includes('systemAdmin'))R(422,'Hr:Complaints:HandlerCannotBeSystemAdmin');
  const bad=u.roles.includes('reviewer')||(x.level==='normal'&&u.roles.includes('executiveDirector'))||(x.level!=='normal'&&u.roles.includes('departmentManager')&&!u.roles.includes('hrManager')&&!u.roles.includes('executiveDirector'));if(bad)R(422,'Hr:Complaints:HandlerRoleNotAllowed');
  if(x.to&&new Date(x.to).getTime()<=new Date(x.from).getTime())R(422,'Hr:Complaints:InvalidAssignmentPeriod');const a={id:this.nid(),user:u.id,level:x.level,status:'pendingApproval',from:x.from,to:x.to||null,reason:x.reason,by:meId,at:nowIso(),stamp:stamp()};D.asg.push(a);return asgDto(a)}
 case 'ComplaintHandler_GetList':{let a=D.asg.slice().reverse();if(x.status)a=a.filter(z=>z.status===x.status);if(x.level)a=a.filter(z=>z.level===x.level);return{totalCount:a.length,items:pg(a,x).items.map(asgDto)}}
 case 'ComplaintHandler_Approve':{const a=D.asg.find(z=>z.id===x.id);if(!a)R(404,NF);if(a.stamp!==x.concurrencyStamp)R(409,'conflict');if(a.status!=='pendingApproval')R(422,'Hr:Complaints:AssignmentNotPending');const u=E(a.user);if(u.roles.includes('systemAdmin'))R(422,'Hr:Complaints:HandlerCannotBeSystemAdmin');
  if(u.roles.includes('reviewer')||(a.level==='normal'&&u.roles.includes('executiveDirector')))R(422,'Hr:Complaints:HandlerRoleNotAllowed');a.status='approved';a.decBy=meId;a.decAt=nowIso();a.stamp=stamp();return asgDto(a)}
 case 'ComplaintHandler_Reject':{const a=D.asg.find(z=>z.id===x.id);if(!a)R(404,NF);if(a.stamp!==x.concurrencyStamp)R(409,'conflict');if(a.status!=='pendingApproval')R(422,'Hr:Complaints:AssignmentNotPending');a.status='rejected';a.decBy=meId;a.decAt=nowIso();a.decReason=x.reason;a.stamp=stamp();return asgDto(a)}
 case 'ComplaintHandler_Revoke':{const a=D.asg.find(z=>z.id===x.id);if(!a)R(404,NF);if(a.stamp!==x.concurrencyStamp)R(409,'conflict');if(a.status!=='approved')R(422,'Hr:Complaints:AssignmentNotActive');a.status='revoked';a.decBy=meId;a.decAt=nowIso();a.decReason=x.reason;a.stamp=stamp();return asgDto(a)}
 case 'SystemSettings_GetComplaints':return{delayDeadlineWorkingDays:D.cfg.delay,overdueReminderIntervalWorkingDays:D.cfg.interval,concurrencyStamp:D.cfg.stamp,lastModificationTime:nowIso()};
 case 'SystemSettings_UpdateComplaints':{if(D.cfg.stamp!==x.concurrencyStamp)R(409,'conflict');D.cfg.delay=x.delayDeadlineWorkingDays;D.cfg.interval=x.overdueReminderIntervalWorkingDays;D.cfg.stamp=stamp();return{delayDeadlineWorkingDays:D.cfg.delay,overdueReminderIntervalWorkingDays:D.cfg.interval,concurrencyStamp:D.cfg.stamp,lastModificationTime:nowIso()}}
 }
 return undefined};
cmpUpload=async(e,id)=>{const fl=e.target.files&&e.target.files[0];if(!fl)return;this.setState({busy:true,derr:''});const T=T4[this.state.lang];
 try{await this.call('Complaint_UploadAttachment',{id,file:fl.name,size:fl.size});this.setState({busy:false});this.toast(T.tZUp);this.enter()}catch(er){this.setState({busy:false,derr:(er&&er.code)||'gen'})}};
zRaiseDlg=(T,c,home)=>{const order=['normal','confidential','highlyConfidential'],hi=order.filter(l=>order.indexOf(l)>order.indexOf(c.level));return{title:T.dZRaise,body:T.dZRaiseB,fields:[{k:'l',l:T.fZLvl,t:'sel',req:1,opts:hi.map(x=>({v:x,l:T.zl[x]}))},{k:'r',l:T.fZRsn,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('Complaint_RaiseLevel',{id:c.id,level:v.l,reason:v.r||undefined,concurrencyStamp:c.concurrencyStamp});return{m:T.tZRaise,go:r.callerRetainsAccess?()=>this.enter():()=>this.toSub('list',null,{tab:home})}}}};
zActDlg=(T,c)=>({title:T.dZAct,body:T.dZActB,fields:[{k:'t',l:T.fZType,t:'sel',req:1,opts:['note','meeting','investigation','followUp'].map(x=>({v:x,l:T.za[x]}))},{k:'n',l:T.fZNote,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{await this.call('Complaint_AddAction',{id:c.id,type:v.t,note:v.n});return{m:T.tZAct}}});
zFwdDlg=(T,c)=>({title:T.dZFwd,body:T.dZFwdB,fields:[{k:'n',l:T.fZNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Complaint_Forward',{id:c.id,note:v.n||undefined,concurrencyStamp:c.concurrencyStamp});return{m:T.tZFwd,go:()=>this.toSub('list',null,{tab:'inbox'})}}});
zCloseDlg=(T,c)=>({title:T.dZClose,body:T.dZCloseB,fields:[{k:'d',l:T.fZDec,t:'ta',req:1,max:4000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Complaint_Close',{id:c.id,decision:v.d,concurrencyStamp:c.concurrencyStamp});return{m:T.tZClose}}});
zAsgReqDlg=(T,users)=>{const td=this.todayS();return{title:T.dZReqH,body:T.dZReqHB,fields:[{k:'u',l:T.fZUser,t:'sel',req:1,opts:users.map(z=>({v:z.id,l:z.displayName}))},{k:'l',l:T.fZLvlH,t:'sel',req:1,init:'normal',opts:Object.keys(T.zl).map(x=>({v:x,l:T.zl[x]}))},{k:'fd',l:T.fZFrom,t:'date',req:1,init:td},{k:'ft',l:T.fZFromT,req:1,max:5,init:'08:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'td',l:T.fZTo,t:'date'},{k:'tt',l:T.fZToT,max:5,init:'17:00',pat:/^([01][0-9]|2[0-3]):[0-5][0-9]$/},{k:'r',l:T.fZRsn,t:'ta',req:1,max:2000}],okLabel:T.cnf,
 run:async(v)=>{await this.call('ComplaintHandler_Request',{userId:v.u,level:v.l,from:new Date(v.fd+'T'+v.ft+':00').toISOString(),to:v.td?new Date(v.td+'T'+(v.tt||'17:00')+':00').toISOString():undefined,reason:v.r});return{m:T.tZReq}}}};
zAsgDecDlg=(T,a,kind)=>({title:T.dZDec,fields:kind==='approve'?[]:[{k:'r',l:T.fZRsn,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:kind!=='approve',
 run:async(v)=>{if(kind==='approve')await this.call('ComplaintHandler_Approve',{id:a.id,concurrencyStamp:a.concurrencyStamp});else await this.call(kind==='reject'?'ComplaintHandler_Reject':'ComplaintHandler_Revoke',{id:a.id,reason:v.r,concurrencyStamp:a.concurrencyStamp});return{m:kind==='approve'?T.tZApp:kind==='reject'?T.tZRej:T.tZRev}}});
route7=(op,x,P,v,ix,R)=>{
 if(!(op in OPR7)) return undefined;
 if(!this.d6) this.mk6();if(!this.d5) this.mk5();
 const db=this.db,U=this.UID,meId=U(this.EID[v.p]),roles=P.roles,L=T4[this.state.lang];
 if(!roles.some(r=>OPR7[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar;
 const eref=e=>{const d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const team=()=>db.emps.filter(e=>e.mgr===meId&&e.status!=='ended'&&!e.roles.includes('executiveDirector'));
 const tsel=()=>{let t=team();if(x.employeeId)t=t.filter(e=>e.id===x.employeeId);return t};
 const per=()=>{if(!x.from||!x.to)R(400,'validation');if(x.to<x.from||(new Date(x.to)-new Date(x.from))/864e5>366)R(422,'Hr:Reports:InvalidPeriod')};
 const work=['present','late','absent','incomplete','onLeave'];
 const daysIn=(e)=>this.daysOf(e,{fromDate:x.from,toDate:x.to}).filter(z=>work.includes(z.status));
 const isoD=(d,t)=>new Date(d+'T'+(t||'00:00')+':00+03:00').toISOString();
 const lvName=(r)=>r.kind==='hourly'?L.lk.hourly:(r.kind==='other'?(((this.d4.types.find(t=>t.id===r.type))||{})[ix?'en':'ar']||L.lk.other):L.lk.annual);
 const typeName=(r)=>{const t=this.rqType(r);return ix?t.en:t.ar};
 const pend=['pendingManager','pendingHr','pendingExecutive','pendingDestination'];
 const rows=(code)=>{
  switch(code){
  case 'teamAttendance':{per();let a=[];tsel().forEach(e=>{daysIn(e).forEach(z=>a.push({employee:eref(e),date:z.date,checkIn:z.checkIn,checkOut:z.checkOut,lateMinutes:z.lateMinutes,statusName:L.dst[z.status]}))});return a.sort((p,q)=>q.date.localeCompare(p.date)||p.employee.fullName.localeCompare(q.employee.fullName))}
  case 'teamLatenessAbsence':{per();return tsel().map(e=>{const d=daysIn(e);return{employee:eref(e),lateCount:d.filter(z=>z.status==='late').length,lateMinutes:d.reduce((n,z)=>n+z.lateMinutes,0),absentDays:d.filter(z=>z.status==='absent').length}})}
  case 'teamLeaves':{per();const ids=tsel().map(e=>e.id);return this.d4.reqs.filter(r=>ids.includes(r.emp)&&!['rejected','cancelled'].includes(r.status)&&!(r.to<x.from||r.from>x.to)).sort((p,q)=>q.from.localeCompare(p.from)).map(r=>({employee:eref(E(r.emp)),leaveTypeName:lvName(r),from:isoD(r.from,r.fromTime),to:isoD(r.to,r.toTime),days:r.kind==='hourly'?null:r.workingDays,hours:r.kind==='hourly'?r.hours:null,statusName:L.rst[r.status]}))}
  case 'teamPending':{const ids=tsel().map(e=>e.id);return this.rqAll().filter(r=>ids.includes(r.s.emp)&&pend.includes(r.s.status)&&this.rqType(r).code!=='accountantRequest').sort((p,q)=>q.s.at.localeCompare(p.s.at)).map(r=>({requestId:r.s.id,employee:eref(E(r.s.emp)),requestTypeName:typeName(r),statusName:L.rst[r.s.status],creationTime:r.s.at}))}
  case 'teamCustody':return this.csTeamRows(x,meId,ix);case 'teamEvaluations':return this.evTeamRows(x,meId,ix);case 'evaluationConsistency':return this.evConsRows(x,ix);
  case 'hrAttendance':{per();let es=db.emps.filter(e=>e.status!=='ended'&&!e.roles.includes('executiveDirector'));if(x.departmentId)es=es.filter(e=>e.dept===x.departmentId);if(x.employeeId)es=es.filter(e=>e.id===x.employeeId);
   return es.map(e=>{const d=daysIn(e);return{employee:eref(e),scheduledWorkingDays:d.length,presentDays:d.filter(z=>z.status==='present'||z.status==='late').length,absentDays:d.filter(z=>z.status==='absent').length,lateCount:d.filter(z=>z.status==='late').length,lateMinutes:d.reduce((n,z)=>n+z.lateMinutes,0),leaveDays:d.filter(z=>z.status==='onLeave').length}})}
  case 'hrLeaveBalances':{let es=db.emps.filter(e=>e.status!=='ended'&&this.d4.y[e.id]&&this.d4.y[e.id].set);if(x.departmentId)es=es.filter(e=>e.dept===x.departmentId);if(x.employeeId)es=es.filter(e=>e.id===x.employeeId);
   return es.map(e=>{const b=this.lvBal(e),h=this.d4.reqs.filter(r=>r.emp===e.id&&r.kind==='hourly'&&['approved','completed'].includes(r.status)).reduce((n,r)=>n+(r.quota||0),0);return{employee:eref(e),leaveYearStart:b.yr.start,entitlementDays:b.Y.ent,carriedInDays:b.Y.car,usedDays:b.used,heldDays:b.held,remainingDays:b.rem,usedHours:h}})}
  case 'hrWarnings':{per();if(!this.d13)this.mk13();this.wrSync();let a=this.d13.ws.filter(w=>w.date>=x.from&&w.date<=x.to);if(x.employeeId)a=a.filter(w=>w.emp===x.employeeId);if(x.departmentId)a=a.filter(w=>(E(w.emp)||{}).dept===x.departmentId);return a.sort((p,q)=>q.date.localeCompare(p.date)).map(w=>({warningId:w.id,employee:eref(E(w.emp)),levelName:L.wlv[w.dec&&w.dec.issuedLevel?w.dec.issuedLevel:w.lvl],incidentDate:w.date,statusName:L.wst[w.status]}))}
  case 'hrContracts':{if(!this.d12)this.mk12();this.ctSync();let a=this.d12.cons.slice();if(x.endFrom)a=a.filter(c=>c.end>=x.endFrom);if(x.endTo)a=a.filter(c=>c.end<=x.endTo);if(x.departmentId)a=a.filter(c=>(E(c.emp)||{}).dept===x.departmentId);if(x.employeeId)a=a.filter(c=>c.emp===x.employeeId);return a.sort((p,q)=>p.end.localeCompare(q.end)).map(c=>({contractId:c.id,employee:eref(E(c.emp)),typeName:c.type,startDate:c.start,endDate:c.end,statusName:L.cst[c.status]}))}
  case 'hrRequests':{per();let a=this.rqAll().filter(r=>r.s.at.slice(0,10)>=x.from&&r.s.at.slice(0,10)<=x.to);if(x.employeeId)a=a.filter(r=>r.s.emp===x.employeeId);if(x.departmentId)a=a.filter(r=>(E(r.s.emp)||{}).dept===x.departmentId);
   return a.sort((p,q)=>q.s.at.localeCompare(p.s.at)).map(r=>{const closed=['approved','rejected','completed','cancelled'].includes(r.s.status);const last=(r.s.tl||[]).slice(-1)[0];return{requestId:r.s.id,employee:eref(E(r.s.emp)),requestTypeName:typeName(r),statusName:L.rst[r.s.status],creationTime:r.s.at,closedAt:closed?(last?last.at:r.s.at):null}})}
  }};
 const map={Report_GetTeamAttendance:'teamAttendance',Report_GetTeamLatenessAbsence:'teamLatenessAbsence',Report_GetTeamLeaves:'teamLeaves',Report_GetTeamPending:'teamPending',Report_GetTeamCustody:'teamCustody',Report_GetTeamEvaluations:'teamEvaluations',Report_GetEvaluationConsistency:'evaluationConsistency',Report_GetHrAttendance:'hrAttendance',Report_GetHrLeaveBalances:'hrLeaveBalances',Report_GetHrRequests:'hrRequests',Report_GetHrContracts:'hrContracts',Report_GetHrWarnings:'hrWarnings'};
 if(map[op]){const a=rows(map[op]);return pg(a,x)}
 if(op==='Report_GetReviewDashboard'){const n=new Date(),yr=x.year||n.getFullYear(),mo=x.month||(n.getMonth()+1);const mine=c=>c.cm===meId||c.resp===meId;const cs=this.d6.cs.filter(c=>!mine(c));const inM=c=>{const d=new Date(c.at);return d.getFullYear()===yr&&d.getMonth()+1===mo};const late=(c)=>c.status!=='closed'&&new Date(this.cmpDue(c)).getTime()<Date.now();
  return{year:yr,month:mo,complaintCounts:{confidentialSubmittedCount:cs.filter(c=>c.level==='confidential'&&inM(c)).length,highlyConfidentialSubmittedCount:cs.filter(c=>c.level==='highlyConfidential'&&inM(c)).length,lateNormalCount:cs.filter(c=>c.level==='normal'&&late(c)).length,lateConfidentialCount:cs.filter(c=>c.level==='confidential'&&late(c)).length,lateHighlyConfidentialCount:cs.filter(c=>c.level==='highlyConfidential'&&late(c)).length,asOf:new Date().toISOString()},...this.evDash(ix)}}
 if(op==='Report_Export'){const code=x.code,allow={dept:['teamAttendance','teamLatenessAbsence','teamLeaves','teamPending','teamCustody','teamEvaluations'],rev:['reviewDashboard','evaluationConsistency'],hr:['hrAttendance','hrLeaveBalances','hrRequests','hrContracts','hrWarnings']};
  const ok=(roles.includes('departmentManager')&&allow.dept.includes(code))||(roles.includes('reviewer')&&allow.rev.includes(code))||(roles.includes('hrManager')&&allow.hr.includes(code));if(!ok)R(403,'forbidden');
  if(!['pdf','excel'].includes(x.format))R(400,'validation');
  if(['teamAttendance','teamLatenessAbsence','teamLeaves','hrAttendance','hrRequests','hrWarnings'].includes(code)){if(!x.from||!x.to)R(422,'Hr:Reports:MissingFilter');per()}
  const n=rows(code)?rows(code).length:0;return{fileName:code+'_'+this.todayS()+(x.format==='pdf'?'.pdf':'.xlsx'),contentType:x.format==='pdf'?'application/pdf':'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',sizeBytes:2048+n*96}}
 R(404,'Hr:Reports:NotFound')};
d8=null;
NT={requestPendingAction:['طلب بانتظار إجراءك','لديك طلب جديد بانتظار قرارك.','A request is waiting for your action','You have a new request waiting for your decision.'],requestStatusChanged:['تحديث على طلبك','تغيرت حالة أحد طلباتك.','Update on your request','The status of one of your requests changed.'],requestSubmittedOnBehalf:['رُفع طلب باسمك','رُفع طلب نيابةً عنك.','A request was submitted for you','A request was submitted on your behalf.'],requestApprovalReminder:['تذكير بطلب معلّق','لديك طلبات ما زالت بانتظار قرارك.','Reminder: pending request','You still have requests waiting for your decision.'],memoUpdate:['تحديث على مطالعة','هناك تحديث جديد على إحدى المطالعات.','Memo update','There is a new update on a memo.'],incidentUpdate:['تحديث على بلاغ','هناك تحديث جديد على أحد بلاغات الأعطال.','Fault-report update','There is a new update on a fault report.'],delegationUpdated:['تحديث على التفويض','تغيّر تفويض يخصك.','Delegation update','A delegation that concerns you changed.'],managerChanged:['تغيّر المدير','تغيّر مديرك المباشر.','Manager changed','Your direct manager changed.'],leaveBalanceAdjusted:['تعديل على رصيد الإجازات','عُدّل رصيد إجازاتك.','Leave balance adjusted','Your leave balance was adjusted.'],accountPasswordReset:['إعادة ضبط كلمة السر','أُعيد ضبط كلمة سر حسابك.','Password reset','Your account password was reset.'],accountSecurityChanged:['تغيير في أمان الحساب','تغيّرت إعدادات أمان حسابك.','Account security changed','Your account security settings changed.'],secureItemUpdateForOwner:['لديك تحديث جديد','افتح التطبيق للاطلاع.','You have a new update','Open the app to view it.'],secureItemUpdateForHandler:['لديك تحديث جديد','افتح التطبيق للاطلاع.','You have a new update','Open the app to view it.'],secureHandlerAssignmentUpdate:['لديك تحديث جديد','افتح التطبيق للاطلاع.','You have a new update','Open the app to view it.']};
mk8=()=>{
 if(!this.db)this.mkDb();const U=this.UID,now=Date.now();
 this.d8={notes:[],tok:{},aud:[],rem:{open:'10:00',sub:3,int:1,esc:4,appr:null,min:null,stamp:'a'}};const D=this.d8;
 const n=(k,code,rt,ago)=>D.notes.push({id:this.nid(),user:U(k),code,rt,rid:null,at:new Date(now-ago*36e5).toISOString(),read:null,cnt:null});
 n('12','requestPendingAction','request',3);n('12','delegationUpdated','none',20);n('13','requestPendingAction','request',5);n('13','secureItemUpdateForHandler','secureItem',4);n('11','requestStatusChanged','request',30);n('11','leaveBalanceAdjusted','none',50);n('16','accountSecurityChanged','none',70);n('21','requestStatusChanged','request',9);
 D.notes[5].read=new Date(now-40*36e5).toISOString();
 const ev=(h,k,action,et,eid,fields,ok,ip)=>D.aud.push({id:this.nid(),at:new Date(now-h*36e5).toISOString(),user:k?U(k):null,action,et,eid,fields:fields||[],ip:ip||'203.0.113.'+(10+(h*7)%200),ok:ok!==false});
 ev(1,'16','login',null,null,[],true);ev(2,'13','login',null,null,[],true);ev(3,null,'loginFailed',null,null,[],false);ev(5,'16','settingChanged','SystemSettings','complaints',['delayDeadlineWorkingDays'],true);ev(8,'13','create','Employee',U('21'),['fullNameAr','fullNameEn','hireDate'],true);ev(12,'16','roleChanged','UserAccount',U('12'),['roles'],true);ev(30,'13','update','WorkShift',null,['name','days'],true);ev(40,'16','passwordReset','UserAccount',U('18'),[],true);ev(48,null,'loginFailed',null,null,[],false);
};
dtReg=()=>{if(!this.srv)return;if(!this.d8)this.mk8();this._tok=this._tok||'web-'+Math.random().toString(36).slice(2,12);this.d8.tok[this._tok]={user:this.UID(this.EID[this.srv.p]),platform:'web'}};
dtRegOp=()=>{if(!this.srv)return;this._tok=this._tok||'web-'+Math.random().toString(36).slice(2,12);this.callRaw('DeviceToken_Register',{token:this._tok,platform:'web'}).catch(()=>0)};
signOutUI=async()=>{try{if(this._tok)await this.callRaw('DeviceToken_Unregister',{token:this._tok})}catch(e){}this.signOut()};
dtUnreg=()=>{if(this.d8&&this._tok&&this.srv&&this.d8.tok[this._tok]&&this.d8.tok[this._tok].user===this.UID(this.EID[this.srv.p]))delete this.d8.tok[this._tok]};
ntf=(user,code,rt,rid)=>{if(!user)return;if(!this.d8)this.mk8();this.d8.notes.push({id:this.nid(),user,code,rt,rid:rid||null,at:new Date().toISOString(),read:null,cnt:null})};
ntApprovers=(rec)=>{const cur=this.rqCur(rec);if(!cur)return[];const emp=rec.s.emp;const ids=cur.uid?[cur.uid]:this.db.emps.filter(e=>e.status==='active'&&e.roles.includes(cur.role)).map(e=>e.id);return ids.filter(i=>i!==emp)};
ntReq=(id,code)=>{const rec=this.rqAll().find(z=>z.s.id===id);if(!rec)return;const cu0=this.rqCur(rec);if(cu0&&cu0.st==='pendingDestination'&&cu0.role==='storeOfficer')return;this.ntApprovers(rec).forEach(u=>this.ntf(u,code||'requestPendingAction','request',id))};
ntfAfter=(op,x,r,me)=>{
 if(!this.d8)this.mk8();const db=this.db,E=id=>db.emps.find(e=>e.id===id),hold=(role)=>(db.emps.find(e=>e.status==='active'&&e.roles.includes(role))||{}).id;x=x||{};
 switch(op){
 case 'Request_Submit':this.ntReq(r.id);if(r.isOnBehalf)this.ntf(r.employee.id,'requestSubmittedOnBehalf','request',r.id);break;
 case 'LeaveRequest_Create':case 'AttendanceCorrection_Create':this.ntReq(r.requestId||r.id);if(r.isOnBehalf&&r.employee)this.ntf(r.employee.id,'requestSubmittedOnBehalf','request',r.requestId||r.id);break;
 case 'Request_Approve':case 'Request_Reject':case 'Request_Cancel':{const rec=this.rqAll().find(z=>z.s.id===r.id);if(!rec)break;if(rec.s.emp!==me)this.ntf(rec.s.emp,'requestStatusChanged','request',r.id);if(op==='Request_Approve')this.ntReq(r.id);break}
 case 'Memo_Create':{const m=this.d5.memos.find(z=>z.id===r.id);if(!m)break;const e=E(m.emp);const u=m.status==='pendingExecutive'?hold('executiveDirector'):(m.hdl==='systemAdmin'?hold('systemAdmin'):(e&&e.mgr));this.ntf(u,'memoUpdate','memo',r.id);break}
 case 'Memo_Forward':this.ntf(hold('hrManager'),'memoUpdate','memo',r.id);this.ntf(r.employee.id,'memoUpdate','memo',r.id);break;
 case 'Memo_Convert':case 'Memo_Close':this.ntf(r.employee.id,'memoUpdate','memo',r.id);if(r.convertedRequestId)this.ntReq(r.convertedRequestId);break;
 case 'Incident_Create':{const i=this.d5.incs.find(z=>z.id===r.id);if(!i)break;const e=E(i.emp);this.ntf(i.status==='pendingHr'?hold('hrManager'):(e&&e.mgr),'incidentUpdate','incident',r.id);break}
 case 'Incident_Forward':this.ntf(hold('hrManager'),'incidentUpdate','incident',r.id);this.ntf(r.employee.id,'incidentUpdate','incident',r.id);break;
 case 'Incident_UpdateStatus':this.ntf(r.employee.id,'incidentUpdate','incident',r.id);break;
 case 'Delegation_Create':this.ntf(r.toUser.id,'delegationUpdated','none');break;
 case 'Delegation_Cancel':{const d=this.d5.dels.find(z=>z.id===x.id);if(d)this.ntf(d.to,'delegationUpdated','none');break}
 case 'Employee_ChangeManager':this.ntf(x.id,'managerChanged','none');break;
 case 'Manager_AssignEmployees':(x.employeeIds||[]).forEach(i=>this.ntf(i,'managerChanged','none'));break;
 case 'LeaveBalance_Adjust':case 'LeaveBalance_SetEntitlement':this.ntf(x.id,'leaveBalanceAdjusted','none');break;
 case 'UserAccount_ResetPassword':this.ntf(x.id,'accountPasswordReset','none');break;
 case 'UserAccount_ResetMfa':case 'UserAccount_SetRoles':case 'UserAccount_UpdatePermissions':this.ntf(x.id,'accountSecurityChanged','none');break;
 // complaints: generic text only, never the respondent
 case 'Complaint_Submit':{const c=this.d6.cs.find(z=>z.id===r.id);if(c)this.ntf(c.hdl,'secureItemUpdateForHandler','secureItem',c.id);break}
 case 'Complaint_RaiseLevel':case 'Complaint_Forward':{const c=this.d6.cs.find(z=>z.id===r.id);if(!c)break;this.ntf(c.hdl,'secureItemUpdateForHandler','secureItem',c.id);if(c.cm!==me)this.ntf(c.cm,'secureItemUpdateForOwner','secureItem',c.id);break}
 case 'Complaint_Close':{const c=this.d6.cs.find(z=>z.id===r.id);if(c)this.ntf(c.cm,'secureItemUpdateForOwner','secureItem',c.id);break}
 case 'Evaluation_Submit':{const ev=this.d9.evs.find(z=>z.id===r.id);if(ev)this.ntf(ev.emp,'evaluationPublished','evaluation',ev.id);break}
 case 'Evaluation_AddResponse':{const ev=this.d9.evs.find(z=>z.id===x.id);if(!ev)break;if(x.type==='objection')this.ntf(hold('hrManager'),'evaluationResponseReceived','evaluation',ev.id);const mg=this.evMgrOf(ev);if(mg)this.ntf(mg,'evaluationResponseReceived','evaluation',ev.id);break}
 case 'Evaluation_DecideObjection':{const ev=this.d9.evs.find(z=>z.id===x.id);if(ev){this.ntf(ev.emp,'evaluationObjectionDecided','evaluation',ev.id);const mg=this.evMgrOf(ev);if(mg)this.ntf(mg,'evaluationObjectionDecided','evaluation',ev.id)}break}
 case 'Circular_Create':{const c=this.d10.circs.find(z=>z.id===r.id);if(c)c.rcp.forEach(i=>this.ntf(i,'circularReceived','circular',c.id));break}
 case 'AppreciationLetter_Create':this.ntf(r.employee.id,'appreciationLetterIssued','appreciationLetter',r.id);break
 case 'PayrollRun_RequestApproval':this.db.emps.filter(e=>e.status==='active'&&e.roles.includes(this.d15.fin.role)).forEach(e=>this.ntf(e.id,'payrollRunPendingApproval','payrollRun',r.id));break
 case 'PayrollRun_Approve':this.db.emps.filter(e=>e.status==='active'&&e.roles.includes('accountant')).forEach(e=>this.ntf(e.id,'payrollRunApproved','payrollRun',r.id));break
 case 'PayrollRun_Close':{const rn=this.d16.runs.find(z=>z.id===r.id);if(rn)rn.items.forEach(i=>this.ntf(i.emp,'payslipAvailable','payslip',i.id));break}
 case 'Warning_Create':{const hr=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'));if(hr)this.ntf(hr.id,'warningRecommended','warning',r.id);break}
 case 'Warning_Notify':this.ntf(r.employee.id,'warningNotified','warning',r.id);break;
 case 'Warning_Respond':{const hr=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'));if(hr)this.ntf(hr.id,'warningReadyForDecision','warning',r.id);break}
 case 'Warning_Decide':this.ntf(r.employee.id,'warningDecided','warning',r.id);break;
 case 'Warning_Object':{const ex=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('executiveDirector'));if(ex)this.ntf(ex.id,'warningObjectionSubmitted','warning',r.id);break}
 case 'Warning_ResolveObjection':{this.ntf(r.employee.id,'warningObjectionResolved','warning',r.id);const hr=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'));if(hr)this.ntf(hr.id,'warningObjectionResolvedForHr','warning',r.id);break}
 case 'Contract_Decide':{const c=this.d12.cons.find(z=>z.id===r.id);if(c)this.ntf(c.emp,'contractDecided','contract',c.id);break}
 case 'StockIssue_Acknowledge':case 'StockIssue_MarkPurchasing':{const i=this.d11.issues.find(z=>z.id===r.id);if(i)this.ntf(i.emp,'requestStatusChanged','request',i.req);break}
 case 'StockIssue_Deliver':{const i=this.d11.issues.find(z=>z.id===r.id);if(!i)break;if(i.cust){const c=this.d11.custs.find(z=>z.id===i.cust);this.stHolders(c).forEach(u=>this.ntf(u,'custodyAssigned','custody',c.id))}this.ntf(i.emp,'requestStatusChanged','request',i.req);break}
 case 'Custody_Create':{const c=this.d11.custs.find(z=>z.id===r.id);if(c)this.stHolders(c).forEach(u=>this.ntf(u,'custodyAssigned','custody',c.id));break}
 case 'Custody_Return':{const c=this.d11.custs.find(z=>z.id===r.id);if(c)this.stHolders(c).forEach(u=>this.ntf(u,'custodyReturned','custody',c.id));break}
 case 'CustodyCount_Create':{const c=this.d11.counts.find(z=>z.id===r.id);if(c&&c.result==='discrepancy')this.db.emps.filter(e=>e.status==='active'&&e.roles.includes('storeOfficer')).forEach(e=>this.ntf(e.id,'custodyCountDifference','custody',c.id));break}
 case 'ComplaintHandler_Request':this.ntf(hold('executiveDirector'),'secureHandlerAssignmentUpdate','secureItem');break;
 case 'ComplaintHandler_Approve':case 'ComplaintHandler_Reject':case 'ComplaintHandler_Revoke':this.ntf(r.user.id,'secureHandlerAssignmentUpdate','secureItem');break;
 }};
audAfter=(op,x,r,ok,err,me)=>{
 if(!this.d8)this.mk8();const D=this.d8;const[ent,verb]=op.split('_');
 if(ent==='Complaint')return;                                   /* complaint entities never appear here */
 const fr0=this.FIN_READS.includes(op);if(!fr0&&/^(Get|List|Lookup|Preview|Download|Search)/.test(verb)&&!this.FIN_EXPORTS.includes(op))return;
 if(ent==='Notification'||ent==='DeviceToken'||ent==='ApplicationConfiguration')return;
 if(!ok&&err&&err.code==='validation')return;
 let action;
 if(this.FIN_READS.includes(op))action='read';
 else if(ent==='ComplaintHandler')action='complaintHandlerAssignment';
 else if(this.FIN_EXPORTS.includes(op))action='export';
 else if(op==='UserAccount_ResetPassword')action='passwordReset';
 else if(op==='UserAccount_SetRoles')action='roleChanged';
 else if(op==='Role_UpdatePermissions'||op==='UserAccount_UpdatePermissions')action='permissionChanged';
 else if(ent==='SystemSettings')action='settingChanged';
 else if(/^(Create|Add|Submit|Upload|Request|Import|Assign|Register|Open|Start|Confirm)/.test(verb))action='create';
 else if(/^(Delete|Cancel|Revoke|Unregister|Remove)/.test(verb))action='delete';
 else action='update';
 const fields=Object.keys(x||{}).filter(k=>!['id','concurrencyStamp','skipCount','maxResultCount','sorting','Idempotency-Key'].includes(k));
 D.aud.push({id:this.nid(),at:new Date().toISOString(),user:me,action,et:ent==='ComplaintHandler'?'ComplaintHandlerAssignment':ent,eid:ent==='ComplaintHandler'?((r&&r.id)||x&&x.id||null):(x&&x.id)||(r&&typeof r==='object'&&r.id)||(op==='Report_Export'?x.code:null),fields,ip:'203.0.113.'+(20+D.aud.length%180),ok})};
call=async(op,x)=>{
 const me=this.srv?this.UID(this.EID[this.srv.p]):null;let r;
 try{r=await this.callRaw(op,x)}catch(e){try{this.audAfter(op,x,null,false,e,me)}catch(z){}throw e}
 try{this.ntfAfter(op,x,r,me);if(op==='Request_Submit'||op==='Request_Approve')this.siEnsure(r.id);this.audAfter(op,x,r,true,null,me);if(op==='ApplicationConfiguration_Get'&&this.srv&&!this.srv.logged){this.srv.logged=true;this.d8||this.mk8();this.d8.aud.push({id:this.nid(),at:new Date().toISOString(),user:me,action:'login',et:null,eid:null,fields:[],ip:'203.0.113.'+(30+this.d8.aud.length%170),ok:true});this.dtRegOp()}
  if(!/^(Notification|DeviceToken)_/.test(op))this.bellSoon()}catch(z){}
 return r};
bellSoon=()=>{clearTimeout(this._bt);this._bt=setTimeout(()=>this.bellSync(),30)};
bellSync=async()=>{if(!this.srv)return;try{const c=await this.callRaw('Notification_GetUnreadCount',{});const l=this.state.bell?await this.callRaw('Notification_List',{maxResultCount:6}):null;this.setState(s=>({ntB:{count:c.count,items:l?l.items:(s.ntB?s.ntB.items:[])}}))}catch(e){}};
goTab=(page,tab,sub,sid)=>{if(!this.can(page))return;this.setState({leaving:true,bell:false,dr:false});setTimeout(()=>{this.setState({page,tab:tab||'data',sub:sub||'list',sid:sid||null,data:{},fv:{},fe:{},dlg:null,step:0,skip:{},derr:'',denied:false,leaving:false,flt:{}});this.enter()},240)};
ntOpen=async(n)=>{
 try{if(!n.isRead)await this.call('Notification_MarkRead',{id:n.id})}catch(e){}
 const c=n.templateCode,id=n.resourceId;
 if(c==='requestPendingAction'||c==='requestApprovalReminder')return this.goTab('req','inbox',id?'req':null,id);
 if(c==='requestStatusChanged'||c==='requestSubmittedOnBehalf')return this.goTab('req','my',id?'req':null,id);
 if(c==='memoUpdate')return this.goTab('req','memos');if(c==='incidentUpdate')return this.goTab('req','inc');if(c==='delegationUpdated')return this.goTab('req','deleg');
 if(c==='leaveBalanceAdjusted')return this.goTab('leave','bal');
 if(c==='managerChanged'||c==='accountPasswordReset'||c==='accountSecurityChanged')return this.go('me');
 if(c==='secureItemUpdateForOwner'||c==='secureItemUpdateForHandler'){if(!id)return this.goTab('cmp','inbox');try{await this.call('Complaint_GetMyDetail',{id});return this.goTab('cmp','my','mine',id)}catch(e){return this.goTab('cmp','inbox','handle',id)}}
 if(c==='secureHandlerAssignmentUpdate')return this.goTab('cmp','handlers');
 if(c==='payrollRunPendingApproval'||c==='payrollRunApproved')return this.goTab('fin','run',id?'rn':null,id);
 if(c==='payslipAvailable')return this.goTab('pay','list',id?'one':null,id);
 if(c==='adjustmentSuggestionsReady')return this.goTab('fin','sug');
 if(c==='appreciationLetterIssued')return this.goTab('apr','my',id?'one':null,id);
 if(c.indexOf('warning')===0){const rr2=(this.state.me?this.state.me.roleNames:[]);if(c==='warningObjectionSubmitted')return this.goTab('wrn','obj',id?'one':null,id);if(rr2.includes('hrManager')&&c!=='warningNotified'&&c!=='warningDecided'&&c!=='warningObjectionResolved')return this.goTab('wrn','hr',id?'one':null,id);return this.goTab('wrn','my',id?'mine':null,id)}
 if(c==='contractExpiring'||c==='contractDecisionDue')return this.goTab('con','exp');
 if(c==='contractDecided')return this.goTab('con','my');
 if(c==='stockIssuePending')return this.goTab('inv','issues');
 if(c==='custodyAssigned'||c==='custodyReturned')return this.goTab('cus','my');
 if(c==='custodyCountDifference')return this.goTab('cus','count');
 if(c==='circularReceived')return this.goTab('ann','recv',id?'one':null,id);
 const rr=(this.state.me?this.state.me.roleNames:[]);
 if(c==='evaluationPublished')return this.goTab('perf','my',id?'mine':null,id);
 if(c.indexOf('evaluation')===0){if(rr.includes('hrManager'))return this.goTab('perf','hr',id?'hrv':null,id);if(rr.includes('departmentManager'))return this.goTab('perf','team',id?'form':null,id);return this.goTab('perf','my',id?'mine':null,id)}
 if(c==='monthEndCycleOpened'||c==='monthlyCycleReminder'||c==='monthlyCycleEscalated')return this.goTab('perf',rr.includes('departmentManager')?'team':(rr.includes('hrManager')?'hrep':'cyc'));
 this.go('dash')};
secDest=(code)=>({reminders:{page:'set',sub:'rem'},contracts:{page:'set',sub:'ctr'},finance:{page:'set',sub:'fin'},complaints:{page:'cmp',tab:'cfg'},attendance:{page:'att',tab:'meth'},hourlyLeave:{page:'leave',tab:'hourly'},rolesAndPermissions:{page:'set',sub:'roles'},requestTypes:{page:'req',tab:'types'},otherLeaveTypes:{page:'leave',tab:'types'},attendanceDevices:{page:'att',tab:'dev'},employeeLeaveBalances:{page:'leave',tab:'emp'},evaluationCriteria:{page:'perf',tab:'crit'},workShifts:{page:'att',tab:'shifts'},holidays:{page:'att',tab:'hol'},departmentsAndManagerLinks:{page:'dept'}}[code]||null);
route8=(op,x,P,v,ix,R)=>{
 if(!(op in OPR8)) return undefined;
 if(!this.d8) this.mk8();
 const D=this.d8,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles,L=ix?1:0;
 if(!roles.some(r=>OPR8[op].includes(this.AB3[r]))) R(403,'forbidden');
 const nowIso=()=>new Date().toISOString(),stamp=()=>Math.random().toString(36).slice(2,8);
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const uref=id=>{const e=db.emps.find(z=>z.id===id);return e?{id:e.id,displayName:ix?e.en:e.ar}:null};
 const dto=(n)=>{const t=this.NT[n.code]||this.NT.secureItemUpdateForOwner;return{id:n.id,templateCode:n.code,resourceType:n.rt,resourceId:n.rid,count:n.cnt,title:t[L*2],body:t[L*2+1],isRead:!!n.read,readAt:n.read,creationTime:n.at}};
 switch(op){
 case 'Notification_List':{let a=D.notes.filter(n=>n.user===meId);if(x.isRead!==undefined&&x.isRead!=='')a=a.filter(n=>!!n.read===(x.isRead===true||x.isRead==='true'));a=a.sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'Notification_GetUnreadCount':return{count:D.notes.filter(n=>n.user===meId&&!n.read).length};
 case 'Notification_MarkRead':{const n=D.notes.find(z=>z.id===x.id&&z.user===meId);if(!n)R(404,'Hr:Notification:NotFound');if(!n.read)n.read=nowIso();return dto(n)}
 case 'DeviceToken_Register':{if(!x.token||x.token.length>4096||!['android','ios','web'].includes(x.platform))R(400,'validation');D.tok[x.token]={user:meId,platform:x.platform};return null}
 case 'DeviceToken_Unregister':{if(!x.token||x.token.length>4096)R(400,'validation');if(D.tok[x.token]&&D.tok[x.token].user===meId)delete D.tok[x.token];return null}
 case 'SystemSettings_ListSections':{const own={reminders:'systemAdmin',contracts:'systemAdmin',complaints:'systemAdmin',attendance:'systemAdmin',hourlyLeave:'systemAdmin',finance:'systemAdmin',rolesAndPermissions:'systemAdmin',requestTypes:'systemAdmin',otherLeaveTypes:'systemAdmin',attendanceDevices:'systemAdmin',evaluationCriteria:'systemAdmin',employeeLeaveBalances:'hrManager',workShifts:'hrManager',holidays:'hrManager',departmentsAndManagerLinks:'hrManager'};return{items:Object.keys(own).map(c=>({code:c,ownerRole:own[c],isEditableByCaller:roles.includes(own[c])}))}}
 case 'SystemSettings_GetReminders':return{monthEndOpenTime:D.rem.open,monthlySubmissionWorkingDays:D.rem.sub,monthlyReminderIntervalWorkingDays:D.rem.int,monthlyEscalationWorkingDay:D.rem.esc,approvalReminderIntervalWorkingDays:D.rem.appr,evaluationMinWorkedDays:D.rem.min,concurrencyStamp:D.rem.stamp,lastModificationTime:D.rem.mod||null};
 case 'SystemSettings_UpdateReminders':{const k=['monthEndOpenTime','monthlySubmissionWorkingDays','monthlyReminderIntervalWorkingDays','monthlyEscalationWorkingDay','approvalReminderIntervalWorkingDays','evaluationMinWorkedDays','concurrencyStamp'];if(k.some(f=>x[f]===undefined||x[f]===null)||!/^([01][0-9]|2[0-3]):[0-5][0-9]$/.test(x.monthEndOpenTime)||[x.monthlySubmissionWorkingDays,x.monthlyReminderIntervalWorkingDays,x.approvalReminderIntervalWorkingDays,x.evaluationMinWorkedDays].some(n=>!Number.isInteger(n)||n<1)||!Number.isInteger(x.monthlyEscalationWorkingDay)||x.monthlyEscalationWorkingDay<2)R(400,'validation');
  if(D.rem.stamp!==x.concurrencyStamp)R(409,'conflict');if(x.monthlyEscalationWorkingDay<=x.monthlySubmissionWorkingDays)R(422,'Hr:Settings:InvalidEscalationDay');
  Object.assign(D.rem,{open:x.monthEndOpenTime,sub:x.monthlySubmissionWorkingDays,int:x.monthlyReminderIntervalWorkingDays,esc:x.monthlyEscalationWorkingDay,appr:x.approvalReminderIntervalWorkingDays,min:x.evaluationMinWorkedDays,stamp:stamp(),mod:nowIso()});return this.route8('SystemSettings_GetReminders',{},P,v,ix,R)}
 case 'AuditLog_List':{let a=D.aud.slice();if(x.from)a=a.filter(e=>e.at>=x.from);if(x.to)a=a.filter(e=>e.at<=x.to);if(x.userId)a=a.filter(e=>e.user===x.userId);if(x.action)a=a.filter(e=>e.action===x.action);if(x.entityType)a=a.filter(e=>(e.et||'').toLowerCase()===String(x.entityType).toLowerCase());if(x.entityId)a=a.filter(e=>e.eid===x.entityId);if(x.succeeded!==undefined&&x.succeeded!=='')a=a.filter(e=>e.ok===(x.succeeded===true||x.succeeded==='true'));
  a=a.sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(e=>({id:e.id,occurredAt:e.at,user:e.user?uref(e.user):null,action:e.action,entityType:e.et,entityId:e.eid,changedFields:e.fields,ipAddress:e.ip,succeeded:e.ok}))}}
 }
 return undefined};
d9=null;
EV_WORK=['present','late','absent','incomplete','onLeave'];
isWorkDay=(d)=>{const dn=['sunday','monday','tuesday','wednesday','thursday','friday','saturday'][new Date(d+'T12:00:00Z').getUTCDay()];return!this.d3.weekly.days.includes(dn)&&!this.d3.hol.some(h=>h.date===d)};
evNth=(start,n)=>{let x=start,k=0;for(let i=0;i<120;i++){if(this.isWorkDay(x)){k++;if(k===n)return x}x=this.dAdd(x,1)}return x};
cyDates=(y,m)=>{const pad=n=>String(n).padStart(2,'0'),last=new Date(Date.UTC(y,m,0)).getUTCDate();let d=y+'-'+pad(m)+'-'+pad(last);const ld=d;while(!this.isWorkDay(d))d=this.dAdd(d,-1);
 const ny=m===12?y+1:y,nm=m===12?1:m+1,c=ny+'-'+pad(nm)+'-01',R=this.d8.rem;const first=this.evNth(c,1);
 return{opensAt:new Date(d+'T10:00:00+03:00').toISOString(),dataFreezeAt:new Date(first+'T00:00:00+03:00').toISOString(),dueAt:new Date(this.evNth(c,R.sub)+'T23:59:00+03:00').toISOString(),escalatesAt:new Date(this.evNth(c,R.esc)+'T00:00:00+03:00').toISOString(),from:y+'-'+pad(m)+'-01',to:ld}};
evAtt=(e,cy)=>{const a=this.daysOf(e,{fromDate:cy.from,toDate:cy.to}).filter(z=>this.EV_WORK.includes(z.status));return{periodFrom:cy.from,periodTo:cy.to,scheduledWorkingDays:a.length,presentDays:a.filter(z=>z.status==='present'||z.status==='late').length,absentDays:a.filter(z=>z.status==='absent').length,lateCount:a.filter(z=>z.status==='late').length,lateMinutes:a.reduce((n,z)=>n+z.lateMinutes,0),leaveDays:a.filter(z=>z.status==='onLeave').length,isFrozen:true,frozenAt:cy.dataFreezeAt}};
evCycle=(id)=>this.d9.cycles.find(c=>c.id===id);
evMgrOf=(ev)=>(this.db.emps.find(e=>e.id===ev.emp)||{}).mgr;
evEligible=(e,cy)=>{if(e.status==='ended'||!e.mgr)return false;if(e.roles.some(r=>['departmentManager','executiveDirector','systemAdmin','hrManager'].includes(r)))return false;const m=this.db.emps.find(z=>z.id===e.mgr);if(!m)return false;const min=this.d8.rem.min||0;return e.hire<=this.dAdd(cy.to,-Math.ceil(min*1.4))};
evGenForms=(cy)=>{const D=this.d9;this.db.emps.filter(e=>this.evEligible(e,cy)).forEach(e=>{if(D.evs.some(v=>v.cyc===cy.id&&v.emp===e.id))return;
  D.evs.push({id:this.nid(),cyc:cy.id,emp:e.id,status:'draft',scores:{},notes:null,by:null,at:null,ack:null,objection:'none',resp:[],fixed:[],draft:{},stamp:'a',crit:D.crit.filter(c=>c.active).map(c=>c.id),grades:D.grades.filter(g=>g.active).map(g=>g.code)})})};
evSeedScores=(ev,k)=>{const D=this.d9;D.crit.filter(c=>ev.crit.includes(c.id)).forEach((c,i)=>{ev.scores[c.id]=ev.grades[(k+i*2+this.hsh(ev.emp+c.id))%ev.grades.length]})};
evSubmitNow=(ev,mgrId,when)=>{ev.status='submitted';ev.by=mgrId;ev.at=when||new Date().toISOString();if(ev.draft[mgrId]){ev.fixed.push({id:this.nid(),author:mgrId,content:ev.draft[mgrId],at:ev.at});delete ev.draft[mgrId]}};
mk9=()=>{
 if(!this.d3)this.mk3();if(!this.d4)this.mk4();if(!this.d5)this.mk5();if(!this.d8)this.mk8();
 const U=this.UID,db=this.db,now=new Date();
 Object.assign(this.NT,{monthEndCycleOpened:['فُتحت نماذج نهاية الشهر','فُتحت نماذج التقييم والتقرير الشهري.','Month-end forms are open','The evaluation and monthly report forms are open.'],monthlyCycleReminder:['تذكير نهاية الشهر','ما زال لديك تقييمات أو تقرير غير مرفوع.','Month-end reminder','You still have evaluations or a report to submit.'],monthlyCycleEscalated:['تصعيد نهاية الشهر','تأخر رفع تقييمات أو تقرير.','Month-end escalation','Evaluations or a report are overdue.'],evaluationPublished:['رُفع تقييمك','رُفع تقييمك الشهري.','Your evaluation was published','Your monthly evaluation was published.'],evaluationResponseReceived:['ردّ جديد على تقييم','وصل ردّ جديد على أحد التقييمات.','New response on an evaluation','A new response arrived on an evaluation.'],evaluationObjectionDecided:['بُتّ في اعتراض','صدر قرار في اعتراض على تقييم.','An objection was decided','A decision was made on an evaluation objection.']});
 const crit=[['E1','الالتزام بالدوام','Attendance commitment'],['E2','جودة العمل','Work quality'],['E3','إنجاز المهام','Task completion'],['E4','التعاون والتواصل','Cooperation and communication'],['E5','المسؤولية والمبادرة','Responsibility and initiative']].map((a,i)=>({id:U(a[0]),ar:a[1],en:a[2],ord:i+1,active:true,stamp:'a'}));
 const grades=[['weak','ضعيف','Weak'],['acceptable','مقبول','Acceptable'],['good','جيد','Good'],['veryGood','جيد جداً','Very good'],['excellent','ممتاز','Excellent']].map((a,i)=>({id:U('F'+(i+1)),code:a[0],ar:a[1],en:a[2],ord:i+1,active:true,stamp:'a'}));
 this.d9={crit,grades,cycles:[],evs:[],reps:[],seq:0};const D=this.d9;
 // the most recent month-end cycle is the previous calendar month; two older cycles are closed
 for(let k=1;k<=3;k++){const dt=new Date(Date.UTC(now.getUTCFullYear(),now.getUTCMonth()-k,1));const y=dt.getUTCFullYear(),m=dt.getUTCMonth()+1;const cy={id:this.nid(),year:y,month:m,status:k===1?'open':'closed',...this.cyDates(y,m)};D.cycles.push(cy);this.evGenForms(cy)}
 const mgr=n=>U(n);
 // older cycles: submitted by each employee's manager
 D.cycles.slice(1).forEach((cy,ci)=>{D.evs.filter(v=>v.cyc===cy.id).forEach((v,i)=>{this.evSeedScores(v,i+ci);v.notes=i%2?'ملاحظة للموظف: استمر على هذا المستوى.':null;this.evSubmitNow(v,this.evMgrOf(v),cy.dataFreezeAt);if(i%2===0)v.ack=cy.dataFreezeAt;
   if(i%3===0)v.fixed.push({id:this.nid(),author:this.evMgrOf(v),content:'ملاحظة داخلية: تحتاج متابعة في الربع القادم.',at:cy.dataFreezeAt})})});
 const old=D.cycles[1];const ob=D.evs.find(v=>v.cyc===old.id&&v.emp===U('18'));if(ob){ob.resp.push({id:this.nid(),type:'objection',content:'أعترض على درجة جودة العمل لأن المهام أُنجزت بالكامل.',at:old.dataFreezeAt,decision:null,reason:null,by:null,decAt:null});ob.objection='pending'}
 // current cycle: one submitted, one partial draft, the rest empty
 const cur=D.cycles[0];const cv=D.evs.filter(v=>v.cyc===cur.id);const s1=cv.find(v=>v.emp===U('18'));if(s1){this.evSeedScores(s1,2);this.evSubmitNow(s1,this.evMgrOf(s1),new Date(now.getTime()-864e5).toISOString())}
 const s2=cv.find(v=>v.emp===U('20'));if(s2){s2.scores[crit[0].id]='good';s2.scores[crit[1].id]='veryGood';s2.notes='أداء جيد هذا الشهر.';s2.draft[this.evMgrOf(s2)]='مسودة ملاحظة داخلية'}
 ['12','23'].forEach(k=>this.ntf(U(k),'monthEndCycleOpened','none'));
};
evFmt=(ev)=>{const cy=this.evCycle(ev.cyc);return{cy,e:this.db.emps.find(z=>z.id===ev.emp)}};
evMgrProgress=(mgrId)=>{const D=this.d9,cy=D.cycles[0],team=D.evs.filter(v=>v.cyc===cy.id&&this.evMgrOf(v)===mgrId);const sub=team.filter(v=>v.status!=='draft').length;const rp=this.evReport(cy,mgrId);return{expectedCount:team.length,submittedCount:sub,remainingCount:team.length-sub,reportId:rp?rp.id:null,reportStatus:rp?rp.status:null,isCompleted:team.length>0&&sub===team.length&&!!rp&&rp.status==='submitted',isLate:(Date.now()>=new Date(cy.escalatesAt).getTime())&&!(team.length>0&&sub===team.length&&!!rp&&rp.status==='submitted')}};
evCompletion=(cy,mgrId)=>{const D=this.d9,team=D.evs.filter(v=>v.cyc===cy.id&&this.evMgrOf(v)===mgrId),sub=team.filter(v=>v.status!=='draft').length,rp=D.reps.find(r=>r.cyc===cy.id&&r.mgr===mgrId);const done=team.length>0&&sub===team.length&&!!rp&&rp.status==='submitted';return{evaluationsExpected:team.length,evaluationsSubmitted:sub,reportSubmitted:!!rp&&rp.status==='submitted',isCompleted:done,isLate:Date.now()>=new Date(cy.escalatesAt).getTime()&&!done}};
evReport=(cy,mgrId)=>{const D=this.d9;let r=D.reps.find(x=>x.cyc===cy.id&&x.mgr===mgrId);if(r)return r;const m=this.db.emps.find(e=>e.id===mgrId);if(!m||!m.roles.includes('departmentManager'))return null;
 const team=this.db.emps.filter(e=>e.mgr===mgrId&&e.status!=='ended'),evs=D.evs.filter(v=>v.cyc===cy.id&&this.evMgrOf(v)===mgrId);const gc={};evs.filter(v=>v.status!=='draft').forEach(v=>Object.values(v.scores).forEach(g=>{gc[g]=(gc[g]||0)+1}));
 const pend=['pendingManager','pendingHr','pendingExecutive','pendingDestination'];
 r={id:this.nid(),cyc:cy.id,mgr:mgrId,status:'draft',notes:null,at:null,stamp:'a',snap:{periodFrom:cy.from,periodTo:cy.to,frozenAt:cy.dataFreezeAt,employees:team.map(e=>{const a=this.evAtt(e,cy),ev=evs.find(v=>v.emp===e.id);return{emp:e.id,presentDays:a.presentDays,absentDays:a.absentDays,lateCount:a.lateCount,lateMinutes:a.lateMinutes,leaveDays:a.leaveDays,pending:this.rqAll().filter(q=>q.s.emp===e.id&&pend.includes(q.s.status)).length,ev:ev?ev.status:null}}),ex:evs.length,sub:evs.filter(v=>v.status!=='draft').length,ack:evs.filter(v=>v.ack).length,pobj:evs.filter(v=>v.objection==='pending').length,gc},circ:this.circSummary(mgrId,cy),cus:this.csSummary(mgrId)};D.reps.push(r);return r};
evTeamRows=(x,meId,ix)=>{if(!this.d9)this.mk9();const D=this.d9;const cy=x.cycleId?this.evCycle(x.cycleId):null;let a=D.evs.filter(v=>v.status!=='draft'&&this.evMgrOf(v)===meId);if(x.cycleId)a=a.filter(v=>v.cyc===x.cycleId);if(x.year)a=a.filter(v=>this.evCycle(v.cyc).year===x.year);if(x.employeeId)a=a.filter(v=>v.emp===x.employeeId);
 return a.map(v=>{const c=this.evCycle(v.cyc),e=this.db.emps.find(z=>z.id===v.emp),d=this.db.D.find(z=>z.id===e.dept);return{evaluationId:v.id,employee:{id:e.id,fullName:ix?e.en:e.ar,departmentName:d?(ix?d.nameEn:d.nameAr):null},year:c.year,month:c.month,status:v.status,objectionStatus:v.objection,scores:D.crit.filter(k=>v.crit.includes(k.id)&&v.scores[k.id]).map(k=>{const g=D.grades.find(z=>z.code===v.scores[k.id]);return{criterionId:k.id,criterionName:ix?k.en:k.ar,gradeCode:g.code,gradeName:ix?g.en:g.ar}})}})};
evConsRows=(x,ix)=>{if(!this.d9)this.mk9();const D=this.d9;const mgrs=this.db.emps.filter(e=>e.roles.includes('departmentManager'));const cyId=x.cycleId||D.cycles[0].id;
 return mgrs.filter(m=>!x.departmentId||m.dept===x.departmentId).map(m=>{const d=this.db.D.find(z=>z.id===m.dept),evs=D.evs.filter(v=>v.cyc===cyId&&this.evMgrOf(v)===m.id),sub=evs.filter(v=>v.status!=='draft');const all=sub.flatMap(v=>Object.values(v.scores));const gc=D.grades.map(g=>({gradeCode:g.code,gradeName:ix?g.en:g.ar,count:all.filter(z=>z===g.code).length})).filter(z=>z.count>0);
  return{manager:{id:m.id,displayName:ix?m.en:m.ar},department:d?{id:d.id,name:ix?d.nameEn:d.nameAr}:null,expectedCount:evs.length,submittedCount:sub.length,completionRatio:evs.length?Math.round(sub.length/evs.length*100)/100:0,distinctGradesUsed:gc.length,sameGradeForAll:sub.length>1&&gc.length===1,gradeCounts:gc}})};
evDash=(ix)=>{if(!this.d9)this.mk9();const D=this.d9;const cy=D.cycles[0];const mgrs=this.db.emps.filter(e=>e.roles.includes('departmentManager'));this.evGen=1;
 const deps=mgrs.map(m=>{this.evReport(cy,m.id);const d=this.db.D.find(z=>z.id===m.dept);return{manager:{id:m.id,displayName:ix?m.en:m.ar},department:d?{id:d.id,name:ix?d.nameEn:d.nameAr}:null,completion:this.evCompletion(cy,m.id)}});
 return{cycleId:cy.id,completedDepartmentsCount:deps.filter(z=>z.completion.isCompleted).length,lateDepartmentsCount:deps.filter(z=>z.completion.isLate).length,departments:deps}};
route9=(op,x,P,v,ix,R)=>{
 if(!(op in OPR9)) return undefined;
 if(!this.d9) this.mk9();
 const D=this.d9,db=this.db,U=this.UID,meId=U(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR9[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id),d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const dref=id=>{const d=db.D.find(z=>z.id===id);return d?{id:d.id,name:ix?d.nameEn:d.nameAr}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const NF='Hr:Evaluations:NotFound',need=(o)=>{if(!o)R(404,NF);return o};
 const cyDto=(c,withProg)=>({id:c.id,year:c.year,month:c.month,status:c.status,opensAt:c.opensAt,dataFreezeAt:c.dataFreezeAt,dueAt:c.dueAt,escalatesAt:c.escalatesAt,managerProgress:withProg&&hasR('departmentManager')?this.evMgrProgress(meId):null});
 const gname=(code)=>{const g=D.grades.find(z=>z.code===code);return g?(ix?g.en:g.ar):code};
 const crit=(id)=>D.crit.find(z=>z.id===id);
 const scoresOf=(ev,submitted)=>D.crit.filter(c=>ev.crit.includes(c.id)).map(c=>({criterionId:c.id,criterionName:ix?c.en:c.ar,gradeCode:ev.scores[c.id]||null,gradeName:ev.scores[c.id]?gname(ev.scores[c.id]):null})).filter(z=>!submitted||z.gradeCode);
 const subScores=(ev)=>scoresOf(ev,true).map(z=>({criterionId:z.criterionId,criterionName:z.criterionName,gradeCode:z.gradeCode,gradeName:z.gradeName}));
 const respDto=(r)=>({id:r.id,type:r.type,content:r.content,creationTime:r.at,decision:r.decision||null,decisionReason:r.reason||null,decidedBy:r.by?uref(r.by):null,decidedAt:r.decAt||null});
 const isLate=(ev)=>{const c=D.cycles.find(z=>z.id===ev.cyc);return ev.status==='draft'?Date.now()>new Date(c.dueAt).getTime():new Date(ev.at).getTime()>new Date(c.dueAt).getTime()};
 const mgrDto=(ev)=>{const c=D.cycles.find(z=>z.id===ev.cyc),e=E(ev.emp);return{id:ev.id,cycleId:ev.cyc,year:c.year,month:c.month,employee:eref(e.id),status:ev.status,isEditable:ev.status==='draft'&&c.status==='open',attendance:this.evAtt(e,c),availableGrades:D.grades.filter(g=>ev.grades.includes(g.code)).map(g=>({code:g.code,name:ix?g.en:g.ar})),scores:scoresOf(ev,false),employeeNotes:ev.notes,submittedAt:ev.at,acknowledgedAt:ev.ack,objectionStatus:ev.objection,responses:ev.resp.map(respDto),concurrencyStamp:ev.stamp}};
 const empDto=(ev)=>{const c=D.cycles.find(z=>z.id===ev.cyc);return{id:ev.id,year:c.year,month:c.month,evaluator:uref(ev.by),status:ev.status,scores:subScores(ev),employeeNotes:ev.notes,submittedAt:ev.at,acknowledgedAt:ev.ack,objectionStatus:ev.objection,responses:ev.resp.map(respDto)}};
 const hrDto=(ev)=>{const c=D.cycles.find(z=>z.id===ev.cyc),e=E(ev.emp);return{id:ev.id,cycleId:ev.cyc,year:c.year,month:c.month,employee:eref(e.id),evaluator:uref(ev.by),status:ev.status,attendance:this.evAtt(e,c),scores:subScores(ev),employeeNotes:ev.notes,submittedAt:ev.at,acknowledgedAt:ev.ack,objectionStatus:ev.objection,responses:ev.resp.map(respDto),concurrencyStamp:ev.stamp}};
 const mine=(ev)=>this.evMgrOf(ev)===meId;
 const evById=(id)=>need(D.evs.find(z=>z.id===id));
 const forMgr=(id)=>{const ev=evById(id);if(!hasR('departmentManager')||!mine(ev))R(404,NF);return ev};
 const forEmp=(id,allowDraft)=>{const ev=evById(id);if(ev.emp!==meId||(!allowDraft&&ev.status==='draft'))R(404,NF);return ev};
 const forHr=(id)=>{const ev=evById(id);if(ev.status==='draft'||ev.emp===meId)R(404,NF);return ev};
 const stampChk=(o,s)=>{if(o.stamp!==s)R(409,'conflict')};
 const noteDto=(n,draft)=>({id:n.id,author:uref(n.author),content:n.content,isDraft:!!draft,creationTime:n.at});
 const annual=(empId,year)=>{const e=E(empId);const evs=D.evs.filter(v=>v.emp===empId&&v.status!=='draft'&&D.cycles.find(c=>c.id===v.cyc).year===year);const ids=[...new Set(evs.flatMap(v=>Object.keys(v.scores)))];
  return{employee:eref(empId),year,evaluatedMonthsCount:evs.length,acknowledgedCount:evs.filter(v=>v.ack).length,objectionsCount:evs.filter(v=>v.resp.some(r=>r.type==='objection')).length,criteria:D.crit.filter(c=>ids.includes(c.id)).map(c=>({criterionId:c.id,criterionName:ix?c.en:c.ar,gradeCounts:D.grades.map(g=>({gradeCode:g.code,gradeName:ix?g.en:g.ar,count:evs.filter(v=>v.scores[c.id]===g.code).length})).filter(z=>z.count>0)}))}};
 const repSum=(r)=>{const c=D.cycles.find(z=>z.id===r.cyc),m=E(r.mgr);return{id:r.id,cycleId:r.cyc,year:c.year,month:c.month,manager:uref(r.mgr),department:dref(m.dept),status:r.status,submittedAt:r.at,completion:this.evCompletion(c,r.mgr)}};
 const repDto=(r)=>{const c=D.cycles.find(z=>z.id===r.cyc),m=E(r.mgr),s=r.snap;return{...repSum(r),snapshot:{periodFrom:s.periodFrom,periodTo:s.periodTo,frozenAt:s.frozenAt,employees:s.employees.map(z=>({employee:eref(z.emp),presentDays:z.presentDays,absentDays:z.absentDays,lateCount:z.lateCount,lateMinutes:z.lateMinutes,leaveDays:z.leaveDays,pendingRequestsCount:z.pending,evaluationStatus:z.ev})),evaluations:{expectedCount:s.ex,submittedCount:s.sub,acknowledgedCount:s.ack,pendingObjectionsCount:s.pobj,gradeCounts:D.grades.map(g=>({gradeCode:g.code,gradeName:ix?g.en:g.ar,count:s.gc[g.code]||0})).filter(z=>z.count>0)},custody:r.cus||{sharedItemsCount:0,personalItemsCount:0,unacknowledgedItemsCount:0,lastCountDate:null},circulars:r.circ||{circularsSentCount:0,recipientsCount:0,acknowledgedCount:0}},notes:r.notes,concurrencyStamp:r.stamp}};
 const repsFor=()=>{D.cycles.forEach(c=>db.emps.filter(e=>e.roles.includes('departmentManager')).forEach(m=>this.evReport(c,m.id)));return D.reps};
 const hrLike=(ev)=>{const c=D.cycles.find(z=>z.id===ev.cyc),e=E(ev.emp);return{id:ev.id,cycleId:ev.cyc,year:c.year,month:c.month,employee:eref(e.id),evaluator:ev.by?uref(ev.by):uref(this.evMgrOf(ev)),status:ev.status,objectionStatus:ev.objection,submittedAt:ev.at,isLate:isLate(ev)}};
 switch(op){
 case 'EvaluationCycle_GetCurrent':return cyDto(D.cycles[0],true);
 case 'EvaluationCycle_GetList':{let a=D.cycles.slice();if(x.year)a=a.filter(c=>c.year===x.year);return{totalCount:a.length,items:a.map(c=>cyDto(c,false))}}
 case 'Evaluation_GetTeamList':{let a=D.evs.filter(mine);if(x.cycleId)a=a.filter(v=>v.cyc===x.cycleId);if(x.status)a=a.filter(v=>v.status===x.status);if(x.employeeId)a=a.filter(v=>v.emp===x.employeeId);
  const cyi=(v)=>D.cycles.findIndex(c=>c.id===v.cyc);a=a.sort((p,q)=>cyi(p)-cyi(q)||nmo(E(p.emp)).localeCompare(nmo(E(q.emp))));return{totalCount:a.length,items:pg(a,x).items.map(v=>{const c=D.cycles.find(z=>z.id===v.cyc);return{id:v.id,cycleId:v.cyc,year:c.year,month:c.month,employee:eref(v.emp),status:v.status,objectionStatus:v.objection,submittedAt:v.at}})}}
 case 'Evaluation_GetManagerForm':return mgrDto(forMgr(x.id));
 case 'Evaluation_SaveDraft':{const ev=forMgr(x.id);const c=D.cycles.find(z=>z.id===ev.cyc);if(ev.status!=='draft')R(422,'Hr:Evaluations:NotEditable');if(c.status!=='open')R(422,'Hr:Evaluations:CycleNotOpen');stampChk(ev,x.concurrencyStamp);
  (x.scores||[]).forEach(s=>{if(!ev.crit.includes(s.criterionId))R(422,'Hr:Evaluations:InvalidCriterion');if(!ev.grades.includes(s.gradeCode))R(422,'Hr:Evaluations:InvalidGrade')});
  (x.scores||[]).forEach(s=>{ev.scores[s.criterionId]=s.gradeCode});if(x.employeeNotes!==undefined)ev.notes=x.employeeNotes||null;ev.stamp=stamp();return mgrDto(ev)}
 case 'Evaluation_Submit':{const ev=forMgr(x.id);const c=D.cycles.find(z=>z.id===ev.cyc);if(ev.status!=='draft')R(422,'Hr:Evaluations:NotEditable');if(c.status!=='open')R(422,'Hr:Evaluations:CycleNotOpen');stampChk(ev,x.concurrencyStamp);
  if(ev.crit.some(k=>!ev.scores[k]))R(422,'Hr:Evaluations:IncompleteScores');this.evSubmitNow(ev,meId,nowIso());ev.stamp=stamp();return mgrDto(ev)}
 case 'Evaluation_GetMyList':{let a=D.evs.filter(v=>v.emp===meId&&v.status!=='draft');if(x.year)a=a.filter(v=>D.cycles.find(c=>c.id===v.cyc).year===x.year);a=a.sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(v=>{const c=D.cycles.find(z=>z.id===v.cyc);return{id:v.id,year:c.year,month:c.month,status:v.status,objectionStatus:v.objection,submittedAt:v.at,acknowledgedAt:v.ack}})}}
 case 'Evaluation_GetMyDetail':return empDto(forEmp(x.id));
 case 'Evaluation_Acknowledge':{const ev=forEmp(x.id);if(!ev.ack){ev.ack=nowIso();ev.status='acknowledged'}return empDto(ev)}
 case 'Evaluation_AddResponse':{const ev=forEmp(x.id);if(!['comment','objection'].includes(x.type)||!x.content||x.content.length>2000)R(400,'validation');if(x.type==='objection'&&ev.objection==='pending')R(422,'Hr:Evaluations:ObjectionAlreadyPending');
  const r={id:this.nid(),type:x.type,content:x.content,at:nowIso(),decision:null,reason:null,by:null,decAt:null};ev.resp.push(r);if(x.type==='objection')ev.objection='pending';return respDto(r)}
 case 'Evaluation_DecideObjection':{const ev=forHr(x.id);const r=need(ev.resp.find(z=>z.id===x.rid));if(r.type!=='objection')R(422,'Hr:Evaluations:NotAnObjection');if(r.decision)R(422,'Hr:Evaluations:ObjectionAlreadyDecided');if(ev.emp===meId)R(422,'Hr:Evaluations:SelfDecisionNotAllowed');stampChk(ev,x.concurrencyStamp);
  if(!['accepted','rejected'].includes(x.decision)||!x.reason)R(400,'validation');r.decision=x.decision;r.reason=x.reason;r.by=meId;r.decAt=nowIso();ev.objection=x.decision;ev.stamp=stamp();return hrDto(ev)}
 case 'Evaluation_GetHrList':{let a=D.evs.filter(v=>v.emp!==meId);if(x.cycleId)a=a.filter(v=>v.cyc===x.cycleId);if(x.status)a=a.filter(v=>v.status===x.status);if(x.objectionStatus)a=a.filter(v=>v.objection===x.objectionStatus);if(x.departmentId)a=a.filter(v=>E(v.emp).dept===x.departmentId);if(x.employeeId)a=a.filter(v=>v.emp===x.employeeId);if(x.isLate!==undefined&&x.isLate!=='')a=a.filter(v=>isLate(v)===(x.isLate===true||x.isLate==='true'));
  const cyi=(v)=>D.cycles.findIndex(c=>c.id===v.cyc);a=a.sort((p,q)=>cyi(p)-cyi(q)||nmo(E(p.emp)).localeCompare(nmo(E(q.emp))));return{totalCount:a.length,items:pg(a,x).items.map(hrLike)}}
 case 'Evaluation_GetHrView':return hrDto(forHr(x.id));
 case 'EvaluationInternalNote_GetList':{const ev=evById(x.id);if(ev.emp===meId)R(404,NF);let a;
  if(hasR('hrManager')){a=ev.fixed.map(n=>noteDto(n,false))}
  else{if(!mine(ev))R(404,NF);a=ev.fixed.filter(n=>n.author===meId).map(n=>noteDto(n,false));if(ev.draft[meId])a.push(noteDto({id:this.pu('dn'+ev.id+meId),author:meId,content:ev.draft[meId],at:ev.at||nowIso()},true))}
  a.sort((p,q)=>q.creationTime.localeCompare(p.creationTime));return{totalCount:a.length,items:a}}
 case 'EvaluationInternalNote_Add':{const ev=forMgr(x.id);if(ev.status==='draft')R(422,'Hr:Evaluations:EvaluationNotSubmitted');if(!x.content||x.content.length>4000)R(400,'validation');const n={id:this.nid(),author:meId,content:x.content,at:nowIso()};ev.fixed.push(n);return noteDto(n,false)}
 case 'EvaluationInternalNote_SaveDraft':{const ev=forMgr(x.id);if(ev.status!=='draft')R(422,'Hr:Evaluations:NotEditable');if(x.content&&x.content.length>4000)R(400,'validation');const c=x.content||'';if(c)ev.draft[meId]=c;else delete ev.draft[meId];return noteDto({id:this.pu('dn'+ev.id+meId),author:meId,content:c,at:nowIso()},true)}
 case 'EvaluationAnnualSummary_GetMy':{if(!x.year)R(400,'validation');return annual(meId,x.year)}
 case 'EvaluationAnnualSummary_GetForEmployee':{if(!x.year)R(400,'validation');const e=need(E(x.employeeId));if(e.id===meId&&!hasR('employee'))R(404,NF);if(!hasR('hrManager')&&!(hasR('departmentManager')&&e.mgr===meId))R(404,NF);if(hasR('hrManager')&&e.id===meId)R(404,NF);return annual(e.id,x.year)}
 case 'EvaluationCriterion_GetList':{let a=D.crit.slice();if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(c=>c.active===(x.isActive===true||x.isActive==='true'));a.sort((p,q)=>p.ord-q.ord);return{totalCount:a.length,items:pg(a,x).items.map(c=>({id:c.id,name:ix?c.en:c.ar,nameAr:c.ar,nameEn:c.en,sortOrder:c.ord,isActive:c.active,concurrencyStamp:c.stamp}))}}
 case 'EvaluationCriterion_Create':{if(!x.nameAr||!x.nameEn)R(400,'validation');if(D.crit.some(c=>c.ar===x.nameAr||c.en.toLowerCase()===x.nameEn.toLowerCase()))R(409,'conflict');const c={id:this.nid(),ar:x.nameAr,en:x.nameEn,ord:x.sortOrder,active:x.isActive,stamp:stamp()};D.crit.push(c);return{id:c.id,name:ix?c.en:c.ar,nameAr:c.ar,nameEn:c.en,sortOrder:c.ord,isActive:c.active,concurrencyStamp:c.stamp}}
 case 'EvaluationCriterion_Update':{const c=need(D.crit.find(z=>z.id===x.id));stampChk(c,x.concurrencyStamp);if(!x.isActive&&c.active&&D.crit.filter(z=>z.active&&z.id!==c.id).length===0)R(422,'Hr:Evaluations:LastActiveCriterion');Object.assign(c,{ar:x.nameAr,en:x.nameEn,ord:x.sortOrder,active:x.isActive,stamp:stamp()});return{id:c.id,name:ix?c.en:c.ar,nameAr:c.ar,nameEn:c.en,sortOrder:c.ord,isActive:c.active,concurrencyStamp:c.stamp}}
 case 'EvaluationGrade_GetList':{let a=D.grades.slice();if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(g=>g.active===(x.isActive===true||x.isActive==='true'));a.sort((p,q)=>p.ord-q.ord);return{totalCount:a.length,items:pg(a,x).items.map(g=>({id:g.id,code:g.code,name:ix?g.en:g.ar,nameAr:g.ar,nameEn:g.en,sortOrder:g.ord,isActive:g.active,concurrencyStamp:g.stamp}))}}
 case 'EvaluationGrade_Create':{if(!/^[a-z][A-Za-z0-9]*$/.test(x.code||'')||!x.nameAr||!x.nameEn)R(400,'validation');if(D.grades.some(g=>g.code===x.code))R(409,'conflict');const g={id:this.nid(),code:x.code,ar:x.nameAr,en:x.nameEn,ord:x.sortOrder,active:x.isActive,stamp:stamp()};D.grades.push(g);return{id:g.id,code:g.code,name:ix?g.en:g.ar,nameAr:g.ar,nameEn:g.en,sortOrder:g.ord,isActive:g.active,concurrencyStamp:g.stamp}}
 case 'EvaluationGrade_Update':{const g=need(D.grades.find(z=>z.id===x.id));stampChk(g,x.concurrencyStamp);if(!x.isActive&&g.active&&D.grades.filter(z=>z.active&&z.id!==g.id).length<2)R(422,'Hr:Evaluations:MinimumActiveGrades');Object.assign(g,{ar:x.nameAr,en:x.nameEn,ord:x.sortOrder,active:x.isActive,stamp:stamp()});return{id:g.id,code:g.code,name:ix?g.en:g.ar,nameAr:g.ar,nameEn:g.en,sortOrder:g.ord,isActive:g.active,concurrencyStamp:g.stamp}}
 case 'DepartmentReport_GetList':{let a=repsFor().slice();if(!hasR('hrManager'))a=a.filter(r=>r.mgr===meId);if(x.cycleId)a=a.filter(r=>r.cyc===x.cycleId);if(x.status)a=a.filter(r=>r.status===x.status);if(x.departmentId)a=a.filter(r=>E(r.mgr).dept===x.departmentId);if(x.isLate!==undefined&&x.isLate!=='')a=a.filter(r=>this.evCompletion(D.cycles.find(c=>c.id===r.cyc),r.mgr).isLate===(x.isLate===true||x.isLate==='true'));
  const cyi=(r)=>D.cycles.findIndex(c=>c.id===r.cyc);a=a.sort((p,q)=>cyi(p)-cyi(q));return{totalCount:a.length,items:pg(a,x).items.map(repSum)}}
 case 'DepartmentReport_Get':{const r=need(repsFor().find(z=>z.id===x.id));if(!hasR('hrManager')&&r.mgr!==meId)R(404,NF);return repDto(r)}
 case 'DepartmentReport_UpdateNotes':{const r=need(repsFor().find(z=>z.id===x.id));if(r.mgr!==meId)R(404,NF);if(r.status==='submitted')R(422,'Hr:DepartmentReports:AlreadySubmitted');stampChk(r,x.concurrencyStamp);if(x.notes&&x.notes.length>4000)R(400,'validation');r.notes=x.notes||null;r.stamp=stamp();return repDto(r)}
 case 'DepartmentReport_Submit':{const r=need(repsFor().find(z=>z.id===x.id));if(r.mgr!==meId)R(404,NF);if(r.status==='submitted')R(422,'Hr:DepartmentReports:AlreadySubmitted');stampChk(r,x.concurrencyStamp);r.status='submitted';r.at=nowIso();r.stamp=stamp();return repDto(r)}
 }
 return undefined};
evRespDlg=(T,id,type)=>({title:type==='comment'?T.dEvCmt:T.dEvObj,body:type==='comment'?T.dEvCmtB:T.dEvObjB,fields:[{k:'c',l:T.fEvContent,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Evaluation_AddResponse',{id,type,content:v.c});return{m:type==='comment'?T.tEvCmt:T.tEvObj}}});
evDecDlg=(T,ev,r)=>({title:T.dEvDec,body:T.dEvDecB,fields:[{k:'d',l:T.fEvDecision,t:'sel',req:1,opts:['accepted','rejected'].map(x=>({v:x,l:T.evdc[x]}))},{k:'r',l:T.fEvReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Evaluation_DecideObjection',{id:ev.id,rid:r.id,decision:v.d,reason:v.r,concurrencyStamp:ev.concurrencyStamp});return{m:T.tEvDecided}}});
evNoteDlg=(T,id)=>({title:T.dEvNote,body:T.dEvNoteB,fields:[{k:'c',l:T.fEvContent,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{await this.call('EvaluationInternalNote_Add',{id,content:v.c});return{m:T.tEvNote}}});
evCritDlg=(T,c)=>({title:T.dEvCrit,body:T.dEvCritB,fields:[{k:'a',l:T.fEvNameAr,req:1,max:200,init:c?c.nameAr:''},{k:'e',l:T.fEvNameEn,req:1,max:200,init:c?c.nameEn:''},{k:'o',l:T.fEvOrder,req:1,max:3,pat:/^\d{1,3}$/,init:c?String(c.sortOrder):''},{k:'t',l:T.fEvActive,t:'sel',req:1,init:String(c?c.isActive:true),opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]}],okLabel:T.save2,
 run:async(v)=>{const b={nameAr:v.a,nameEn:v.e,sortOrder:Number(v.o),isActive:v.t==='true'};if(c)await this.call('EvaluationCriterion_Update',{id:c.id,...b,concurrencyStamp:c.concurrencyStamp});else await this.call('EvaluationCriterion_Create',b);return{m:c?T.tSaved:T.tCreated}}});
evGradeDlg=(T,g)=>({title:T.dEvGradeD,body:T.dEvCritB,fields:[...(g?[]:[{k:'c',l:T.fEvCode,req:1,max:50,pat:/^[a-z][A-Za-z0-9]*$/}]),{k:'a',l:T.fEvNameAr,req:1,max:100,init:g?g.nameAr:''},{k:'e',l:T.fEvNameEn,req:1,max:100,init:g?g.nameEn:''},{k:'o',l:T.fEvOrder,req:1,max:3,pat:/^\d{1,3}$/,init:g?String(g.sortOrder):''},{k:'t',l:T.fEvActive,t:'sel',req:1,init:String(g?g.isActive:true),opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]}],okLabel:T.save2,
 run:async(v)=>{const b={nameAr:v.a,nameEn:v.e,sortOrder:Number(v.o),isActive:v.t==='true'};if(g)await this.call('EvaluationGrade_Update',{id:g.id,...b,concurrencyStamp:g.concurrencyStamp});else await this.call('EvaluationGrade_Create',{code:v.c,...b});return{m:g?T.tSaved:T.tCreated}}});
d10=null;
mk10=()=>{
 if(!this.d9)this.mk9();const U=this.UID,db=this.db,now=Date.now();
 Object.assign(this.NT,{circularReceived:['تعميم جديد','وصلك تعميم جديد.','New circular','You received a new circular.']});
 this.d10={circs:[]};const D=this.d10;
 const act=()=>db.emps.filter(e=>e.status==='active');
 const mk=(sender,scope,title,content,at,ackP)=>{const rcp=this.circRecipients(sender,scope);const c={id:this.nid(),scope,title,content,sender,at,rcp,acks:{}};rcp.forEach((id,i)=>{if(i%ackP===0)c.acks[id]=new Date(new Date(at).getTime()+36e5*(i+2)).toISOString()});D.circs.push(c);return c};
 const cy=this.d9.cycles[0],mid=new Date(cy.from+'T10:00:00+03:00');mid.setUTCDate(mid.getUTCDate()+9);
 mk(U('12'),'team','اجتماع الفريق الشهري','يُعقد اجتماع الفريق يوم الأحد القادم في تمام العاشرة صباحاً.',mid.toISOString(),2);
 mk(U('13'),'company','تعليمات الدوام الرسمي','يُرجى الالتزام بأوقات الدوام الرسمي وتسجيل البصمة عند الدخول والخروج.',new Date(now-12*864e5).toISOString(),3);
 const last=mk(U('13'),'departmentManagers','إقفال نهاية الشهر','يُرجى رفع التقييمات والتقرير الشهري ضمن المهلة المحددة.',new Date(now-3*864e5).toISOString(),99);
 last.rcp.forEach(id=>{if(id===U('12')||id===U('23'))this.ntf(id,'circularReceived','circular',last.id)});
 const c2=D.circs[1];[U('11'),U('18')].forEach(id=>{delete c2.acks[id];delete D.circs[0].acks[id]});[U('11'),U('18')].forEach(id=>{if(c2.rcp.includes(id)&&!c2.acks[id])this.ntf(id,'circularReceived','circular',c2.id)});
};
circRecipients=(senderId,scope)=>{const db=this.db,a=db.emps.filter(e=>e.status==='active'&&e.id!==senderId);
 if(scope==='team')return a.filter(e=>e.mgr===senderId).map(e=>e.id);
 if(scope==='departmentManagers')return a.filter(e=>e.roles.includes('departmentManager')).map(e=>e.id);
 return a.map(e=>e.id)};
circSummary=(mgrId,cy)=>{if(!this.d10)this.mk10();const a=this.d10.circs.filter(c=>c.sender===mgrId&&c.at.slice(0,10)>=cy.from&&c.at.slice(0,10)<=cy.to);return{circularsSentCount:a.length,recipientsCount:a.reduce((n,c)=>n+c.rcp.length,0),acknowledgedCount:a.reduce((n,c)=>n+Object.keys(c.acks).length,0)}};
route10=(op,x,P,v,ix,R)=>{
 if(!(op in OPR10)) return undefined;
 if(!this.d10) this.mk10();
 const D=this.d10,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR10[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id),d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const NF='Hr:Circulars:NotFound',need=(o)=>{if(!o)R(404,NF);return o};
 const rcvDto=(c)=>({id:c.id,scope:c.scope,title:c.title,content:c.content,sender:uref(c.sender),sentAt:c.at,acknowledgementRequired:true,acknowledgedAt:c.acks[meId]||null});
 const sentDto=(c)=>({id:c.id,scope:c.scope,title:c.title,content:c.content,sentAt:c.at,recipientsCount:c.rcp.length,acknowledgedCount:Object.keys(c.acks).length});
 const flag=(a,f)=>f===undefined||f===''?a:a.filter(f2=>f2);
 switch(op){
 case 'Circular_GetReceivedList':{let a=D.circs.filter(c=>c.rcp.includes(meId));if(x.isAcknowledged!==undefined&&x.isAcknowledged!=='')a=a.filter(c=>!!c.acks[meId]===(x.isAcknowledged===true||x.isAcknowledged==='true'));a=a.slice().sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(rcvDto)}}
 case 'Circular_Get':{const c=need(D.circs.find(z=>z.id===x.id));if(!c.rcp.includes(meId))R(404,NF);return rcvDto(c)}
 case 'Circular_Acknowledge':{const c=need(D.circs.find(z=>z.id===x.id));if(!c.rcp.includes(meId))R(404,NF);if(!c.acks[meId])c.acks[meId]=new Date().toISOString();return rcvDto(c)}
 case 'Circular_Create':{const allowed=[...(hasR('departmentManager')?['team']:[]),...((hasR('hrManager')||hasR('executiveDirector'))?['departmentManagers','company']:[])];
  if(!['team','departmentManagers','company'].includes(x.scope)||!x.title||x.title.length>200||!x.content||x.content.length>8000)R(400,'validation');
  if(!allowed.includes(x.scope))R(422,'Hr:Circulars:ScopeNotAllowed');const rcp=this.circRecipients(meId,x.scope);if(!rcp.length)R(422,'Hr:Circulars:NoRecipients');
  const c={id:this.nid(),scope:x.scope,title:x.title,content:x.content,sender:meId,at:new Date().toISOString(),rcp,acks:{}};D.circs.push(c);return sentDto(c)}
 case 'Circular_GetSentList':{let a=D.circs.filter(c=>c.sender===meId).sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(sentDto)}}
 case 'Circular_GetReceipts':{const c=need(D.circs.find(z=>z.id===x.id));if(c.sender!==meId)R(404,NF);let a=c.rcp.map(id=>({id,at:c.acks[id]||null}));if(x.isAcknowledged!==undefined&&x.isAcknowledged!=='')a=a.filter(r=>!!r.at===(x.isAcknowledged===true||x.isAcknowledged==='true'));
  a.sort((p,q)=>nmo(E(p.id)).localeCompare(nmo(E(q.id))));return{totalCount:a.length,items:pg(a,x).items.map(r=>({recipient:eref(r.id),acknowledgedAt:r.at}))}}
 }
 return undefined};
d11=null;
mk11=()=>{
 if(!this.d10)this.mk10();const U=this.UID,db=this.db,now=Date.now(),SO=(db.emps.find(e=>e.roles.includes('storeOfficer'))||{}).id;
 Object.assign(this.NT,{stockIssuePending:['طلب صرف جديد','وصل طلب قطعة بانتظار اطلاعك.','New stock-issue request','A request for an item is waiting for you.'],custodyAssigned:['عهدة جديدة','سُجّلت عهدة جديدة بانتظار تأكيد الاستلام.','New custody','A new custody item is waiting for your acknowledgement.'],custodyReturned:['إرجاع عهدة','سُجّل إرجاع عهدة.','Custody returned','A custody return was recorded.'],custodyCountDifference:['فرق في جرد العهدة','سُجّل جرد عهدة فيه فرق.','Custody count difference','A custody count was recorded with a difference.']});
 this.d11={items:[],moves:[],issues:[],custs:[],counts:[],expenses:[],reversed:{}};const D=this.d11;
 const ago=(h)=>new Date(now-h*36e5).toISOString();
 const it=(k,ar,en,kind,min,qty)=>{const i={id:U(k),ar,en,kind,min,stamp:'a',active:true,created:ago(2000),mod:null};D.items.push(i);if(qty)this.stMove(i.id,'receive',qty,{note:'رصيد افتتاحي',by:SO},2000);return i};
 const lap=it('D1','حاسوب محمول','Laptop','returnable',2,8),mon=it('D2','شاشة','Monitor','returnable',3,9),pho=it('D3','هاتف','Mobile phone','returnable',2,2),prn=it('D4','طابعة','Printer','returnable',1,3),pap=it('D5','ورق A4','A4 paper','consumable',20,40),pen=it('D6','أقلام','Pens','consumable',30,100),ton=it('D7','حبر طابعة','Toner','consumable',3,2);
 const cu=(item,kind,emp,dept,qty,status,h,o)=>{const c={id:this.nid(),item:item.id,kind,emp:emp||null,dept:dept||null,qty,status,at:ago(h),by:SO,ackAt:null,ackBy:null,retAt:null,req:null,issue:null,note:null,...(o||{})};if(status!=='pendingAcknowledgement'){c.ackAt=ago(h-2);c.ackBy=kind==='personal'?emp:U('12')}D.custs.push(c);const m=this.stMove(item.id,'issue',-qty,{cust:c.id,emp:emp||null,by:SO,note:'تسليم عهدة'},h);return c};
 const hd=(db.emps.find(e=>e.id===U('12'))||{}).dept;
 cu(lap,'personal',U('11'),null,1,'acknowledged',1500);cu(mon,'personal',U('18'),null,1,'pendingAcknowledgement',30);
 const ret=cu(pho,'personal',U('20'),null,1,'acknowledged',900);ret.status='returned';ret.retAt=ago(500);this.stMove(pho.id,'return',1,{cust:ret.id,emp:U('20'),by:SO,note:'إرجاع'},500);
 cu(prn,'shared',null,hd,1,'acknowledged',700,{note:'طابعة القسم'});cu(mon,'shared',null,hd,2,'acknowledged',600,{note:'شاشات غرفة الاجتماعات'});
 D.counts.push({id:this.nid(),dept:hd,date:new Date(now-20*864e5).toISOString().slice(0,10),result:'matched',note:null,by:U('12'),at:ago(480),lines:D.custs.filter(c=>c.kind==='shared'&&c.dept===hd).map(c=>({cust:c.id,exp:c.qty,cnt:c.qty,note:null}))});
 // stock issues: requests of type storeItem that reached the store officer
 const rq=(emp,item,qty,st,extra)=>{const S={id:this.nid(),kind:'gen',type:'storeItem',emp,by:emp,onb:false,desc:'للاستخدام في العمل',itemId:item.id,qty,status:'pendingDestination',at:ago(48),tl:[],reason:null};this.d5.reqs.push(S);const i={id:this.nid(),req:S.id,item:item.id,qty,emp,status:st,ackAt:null,ackBy:null,purAt:null,delAt:null,delBy:null,cust:null,move:null,at:ago(47),...(extra||{})};D.issues.push(i);return i};
 rq(U('11'),pap,5,'pendingAcknowledgement');
 rq(U('18'),pho,3,'purchasing',{ackAt:ago(30),ackBy:SO,purAt:ago(29)});
 rq(U('19'),ton,4,'acknowledged',{ackAt:ago(20),ackBy:SO});
 this.ntf(SO,'stockIssuePending','stockIssue',D.issues[0].id);this.ntf(SO,'stockIssuePending','stockIssue',D.issues[2].id);
 this.ntf(U('18'),'custodyAssigned','none');
};
stQty=(itemId)=>this.d11.moves.filter(m=>m.item===itemId).reduce((n,m)=>n+m.qty,0);
stMove=(itemId,type,qty,o,hoursAgo)=>{const D=this.d11,after=this.stQty(itemId)+qty;const m={id:this.nid(),item:itemId,type,qty,after,req:null,issue:null,cust:null,rev:null,emp:null,by:null,note:null,cost:null,at:new Date(Date.now()-(hoursAgo||0)*36e5).toISOString(),...(o||{})};D.moves.push(m);return m};
stRef=(id)=>{const i=this.d11.items.find(z=>z.id===id);const ix=this.state.lang==='en';return i?{id:i.id,name:ix?i.en:i.ar,kind:i.kind}:null};
siEnsure=(reqId)=>{const rec=this.rqAll().find(z=>z.s.id===reqId);if(!rec||rec.k!=='gen'||rec.s.type!=='storeItem'||rec.s.status!=='pendingDestination')return;if(!this.d11)this.mk11();const D=this.d11;if(D.issues.some(i=>i.req===reqId))return;
 const i={id:this.nid(),req:reqId,item:rec.s.itemId,qty:rec.s.qty,emp:rec.s.emp,status:'pendingAcknowledgement',ackAt:null,ackBy:null,purAt:null,delAt:null,delBy:null,cust:null,move:null,at:new Date().toISOString()};D.issues.push(i);
 this.db.emps.filter(e=>e.status==='active'&&e.roles.includes('storeOfficer')).forEach(e=>this.ntf(e.id,'stockIssuePending','stockIssue',i.id))};
siStatusDto=(reqId)=>{if(!this.d11)return null;const i=this.d11.issues.find(z=>z.req===reqId);if(!i)return null;return{id:i.id,item:this.stRef(i.item),quantity:i.qty,status:i.status,statusLabel:T4[this.state.lang].sist[i.status],deliveredAt:i.delAt||null,custodyId:i.cust||null}};
csTeamRows=(x,meId,ix)=>{if(!this.d11)this.mk11();const D=this.d11,db=this.db,L=T4[this.state.lang];const team=db.emps.filter(e=>e.mgr===meId&&e.status!=='ended').map(e=>e.id),depts=[...new Set(db.emps.filter(e=>e.mgr===meId&&e.status!=='ended').map(e=>e.dept))];
 let a=D.custs.filter(c=>(c.kind==='personal'&&team.includes(c.emp))||(c.kind==='shared'&&depts.includes(c.dept)));if(x.employeeId)a=a.filter(c=>c.emp===x.employeeId);if(x.kind)a=a.filter(c=>c.kind===x.kind);
 return a.map(c=>{const e=c.emp?db.emps.find(z=>z.id===c.emp):null;const d=e?db.D.find(z=>z.id===e.dept):null;return{custodyId:c.id,itemName:this.stRef(c.item).name,kind:c.kind,employee:e?{id:e.id,fullName:ix?e.en:e.ar,departmentName:d?(ix?d.nameEn:d.nameAr):null}:null,assignedAt:c.at,acknowledgedAt:c.ackAt,statusName:L.cust[c.status]}})};
csSummary=(mgrId)=>{if(!this.d11)this.mk11();const D=this.d11,db=this.db;const team=db.emps.filter(e=>e.mgr===mgrId&&e.status!=='ended'),ids=team.map(e=>e.id),depts=[...new Set(team.map(e=>e.dept))];
 const sh=D.custs.filter(c=>c.kind==='shared'&&depts.includes(c.dept)&&c.status!=='returned'),pe=D.custs.filter(c=>c.kind==='personal'&&ids.includes(c.emp)&&c.status!=='returned');const cn=D.counts.filter(c=>depts.includes(c.dept)).map(c=>c.date).sort().slice(-1)[0]||null;
 return{sharedItemsCount:sh.length,personalItemsCount:pe.length,unacknowledgedItemsCount:sh.concat(pe).filter(c=>c.status==='pendingAcknowledgement').length,lastCountDate:cn}};
route11=(op,x,P,v,ix,R)=>{
 if(!(op in OPR11)) return undefined;
 if(!this.d11) this.mk11();
 const D=this.d11,db=this.db,U=this.UID,meId=U(this.EID[v.p]),roles=P.roles,L=T4[this.state.lang];
 if(!roles.some(r=>OPR11[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id),d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const dref=id=>{const d=db.D.find(z=>z.id===id);return d?{id:d.id,name:ix?d.nameEn:d.nameAr}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const NF='Hr:Stock:NotFound',need=(o,c)=>{if(!o)R(404,c||NF);return o};
 const bool=(v0)=>v0===true||v0==='true';
 const item=(id)=>need(D.items.find(z=>z.id===id),'Hr:Item:NotFound');
 const itemDto=(i)=>{const q=this.stQty(i.id);return{id:i.id,nameAr:i.ar,nameEn:i.en,kind:i.kind,minQuantity:i.min,quantityOnHand:q,isBelowMinimum:q<i.min,isActive:i.active,concurrencyStamp:i.stamp,creationTime:i.created,lastModificationTime:i.mod}};
 const moveDto=(m)=>({id:m.id,item:this.stRef(m.item),type:m.type,quantity:m.qty,balanceAfter:m.after,requestId:m.req,stockIssueId:m.issue,custodyId:m.cust,reversesMovementId:m.rev,employee:m.emp?eref(m.emp):null,performedBy:uref(m.by),note:m.note,creationTime:m.at});
 const issueDto=(i)=>{const q=this.stQty(i.item);return{id:i.id,requestId:i.req,item:this.stRef(i.item),quantity:i.qty,quantityOnHand:q,isAvailable:q>=i.qty,status:i.status,statusLabel:L.sist[i.status],employee:eref(i.emp),acknowledgedAt:i.ackAt,acknowledgedBy:i.ackBy?uref(i.ackBy):null,purchasingAt:i.purAt,deliveredAt:i.delAt,deliveredBy:i.delBy?uref(i.delBy):null,custodyId:i.cust,stockMovementId:i.move,creationTime:i.at}};
 const custDto=(c)=>({id:c.id,item:this.stRef(c.item),kind:c.kind,employee:c.emp?eref(c.emp):null,department:c.dept?dref(c.dept):null,quantity:c.qty,status:c.status,assignedAt:c.at,assignedBy:uref(c.by),acknowledgedAt:c.ackAt,acknowledgedBy:c.ackBy?uref(c.ackBy):null,returnedAt:c.retAt,requestId:c.req,stockIssueId:c.issue,note:c.note});
 const countDto=(c)=>({id:c.id,department:dref(c.dept),countDate:c.date,result:c.result,note:c.note,countedBy:uref(c.by),lines:c.lines.map(l=>{const cu=D.custs.find(z=>z.id===l.cust);return{custodyId:l.cust,item:this.stRef(cu.item),expectedQuantity:l.exp,countedQuantity:l.cnt,note:l.note}}),creationTime:c.at});
 const myTeam=()=>db.emps.filter(e=>e.mgr===meId&&e.status!=='ended');
 const myDepts=()=>[...new Set(myTeam().map(e=>e.dept))];
 const mgrOfDept=(dept)=>db.emps.filter(e=>e.status==='active'&&e.roles.includes('departmentManager')&&db.emps.some(z=>z.mgr===e.id&&z.dept===dept&&z.status!=='ended')).map(e=>e.id);
 const holderUsers=(c)=>c.kind==='personal'?[c.emp]:mgrOfDept(c.dept);
 const storeUsers=()=>db.emps.filter(e=>e.status==='active'&&e.roles.includes('storeOfficer')).map(e=>e.id);
 const custFilter=(a,q)=>{if(q.kind)a=a.filter(c=>c.kind===q.kind);if(q.status)a=a.filter(c=>c.status===q.status);if(q.employeeId)a=a.filter(c=>c.emp===q.employeeId);if(q.departmentId)a=a.filter(c=>c.dept===q.departmentId);if(q.itemId)a=a.filter(c=>c.item===q.itemId);return a.slice().sort((p,z)=>z.at.localeCompare(p.at))};
 const closeReq=(i)=>{const rec=this.rqAll().find(z=>z.s.id===i.req);if(!rec)return;const cur=this.rqCur(rec);if(cur){cur.decision='approved';cur.by=meId;cur.at=nowIso()}rec.s.status='completed';(rec.s.tl=rec.s.tl||[]).push({at:nowIso(),action:ix?'Delivered':'تم التسليم',actor:meId,note:null})};
 switch(op){
 case 'Item_GetList':{let a=D.items.slice();if(x.kind)a=a.filter(i=>i.kind===x.kind);if(x.isActive!==undefined&&x.isActive!=='')a=a.filter(i=>i.active===bool(x.isActive));if(x.belowMinimum!==undefined&&x.belowMinimum!=='')a=a.filter(i=>(this.stQty(i.id)<i.min)===bool(x.belowMinimum));if(x.filter)a=a.filter(i=>(i.ar+i.en).toLowerCase().includes(String(x.filter).toLowerCase()));return{totalCount:a.length,items:pg(a,x).items.map(itemDto)}}
 case 'Item_Create':{if(!x.nameAr||!x.nameEn||!['returnable','consumable'].includes(x.kind)||!(x.minQuantity>=0))R(400,'validation');if(D.items.some(i=>i.ar===x.nameAr||i.en.toLowerCase()===x.nameEn.toLowerCase()))R(409,'Hr:Item:NameAlreadyExists');const i={id:this.nid(),ar:x.nameAr,en:x.nameEn,kind:x.kind,min:x.minQuantity,stamp:stamp(),active:true,created:nowIso(),mod:null};D.items.push(i);return itemDto(i)}
 case 'Item_Lookup':{let a=D.items.filter(i=>i.active);if(x.kind)a=a.filter(i=>i.kind===x.kind);if(x.filter)a=a.filter(i=>(i.ar+i.en).toLowerCase().includes(String(x.filter).toLowerCase()));return{totalCount:a.length,items:pg(a,x).items.map(i=>this.stRef(i.id))}}
 case 'Item_Get':return itemDto(item(x.id));
 case 'Item_Update':{const i=item(x.id);if(i.stamp!==x.concurrencyStamp)R(409,'conflict');if(x.kind!==i.kind&&D.moves.some(m=>m.item===i.id))R(422,'Hr:Item:KindLocked');if(D.items.some(z=>z.id!==i.id&&(z.ar===x.nameAr||z.en.toLowerCase()===x.nameEn.toLowerCase())))R(409,'Hr:Item:NameAlreadyExists');Object.assign(i,{ar:x.nameAr,en:x.nameEn,kind:x.kind,min:x.minQuantity,active:x.isActive,stamp:stamp(),mod:nowIso()});return itemDto(i)}
 case 'Stock_Receive':{const i=item(x.itemId);if(!i.active)R(422,'Hr:Stock:ItemInactive');if(!(x.quantity>0))R(400,'validation');let is=null;
  if(x.stockIssueId){is=need(D.issues.find(z=>z.id===x.stockIssueId));if(is.status!=='purchasing')R(422,'Hr:Stock:IssueNotPurchasing');if(is.item!==i.id)R(422,'Hr:Stock:IssueItemMismatch');if(!(x.totalCost>0))R(422,'Hr:Stock:PurchaseCostRequired')}
  const m=this.stMove(i.id,'receive',x.quantity,{by:meId,note:x.note||null,issue:is?is.id:null,cost:x.totalCost||null});if(x.totalCost>0)D.expenses.push({id:this.nid(),move:m.id,amount:x.totalCost,category:'stockPurchases',reversed:false});return moveDto(m)}
 case 'Stock_Adjust':{const i=item(x.itemId);if(!x.reason||x.reason.length>2000)R(400,'validation');if(!x.quantity)R(422,'Hr:Stock:ZeroQuantity');if(this.stQty(i.id)+x.quantity<0)R(422,'Hr:Stock:InsufficientStock');
  if(x.reversesMovementId){need(D.moves.find(z=>z.id===x.reversesMovementId));if(D.reversed[x.reversesMovementId])R(422,'Hr:Stock:MovementAlreadyReversed')}
  const m=this.stMove(i.id,'adjustment',x.quantity,{by:meId,note:x.reason,rev:x.reversesMovementId||null});if(x.reversesMovementId){D.reversed[x.reversesMovementId]=m.id;const ex=D.expenses.find(z=>z.move===x.reversesMovementId);if(ex)ex.reversed=true}return moveDto(m)}
 case 'Stock_GetMovements':{let a=D.moves.slice();if(x.itemId)a=a.filter(m=>m.item===x.itemId);if(x.type)a=a.filter(m=>m.type===x.type);if(x.fromDate)a=a.filter(m=>m.at.slice(0,10)>=x.fromDate);if(x.toDate)a=a.filter(m=>m.at.slice(0,10)<=x.toDate);a.sort((p,z)=>z.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(moveDto)}}
 case 'StockIssue_GetList':{let a=D.issues.slice();if(x.status)a=a.filter(i=>i.status===x.status);if(x.itemId)a=a.filter(i=>i.item===x.itemId);if(x.employeeId)a=a.filter(i=>i.emp===x.employeeId);a.sort((p,z)=>z.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(issueDto)}}
 case 'StockIssue_Get':return issueDto(need(D.issues.find(z=>z.id===x.id)));
 case 'StockIssue_Acknowledge':{const i=need(D.issues.find(z=>z.id===x.id));if(i.status!=='pendingAcknowledgement')R(422,'Hr:Stock:IssueInvalidState');i.status='acknowledged';i.ackAt=nowIso();i.ackBy=meId;return issueDto(i)}
 case 'StockIssue_MarkPurchasing':{const i=need(D.issues.find(z=>z.id===x.id));if(i.status!=='acknowledged')R(422,'Hr:Stock:IssueInvalidState');i.status='purchasing';i.purAt=nowIso();i.pnote=x.note||null;return issueDto(i)}
 case 'StockIssue_Deliver':{const i=need(D.issues.find(z=>z.id===x.id));if(!['acknowledged','purchasing'].includes(i.status))R(422,'Hr:Stock:IssueInvalidState');const it=item(i.item);if(this.stQty(it.id)<i.qty)R(422,'Hr:Stock:InsufficientStock');
  const m=this.stMove(it.id,'issue',-i.qty,{by:meId,req:i.req,issue:i.id,emp:i.emp,note:x.note||null});let c=null;
  if(it.kind==='returnable'){const kind=x.custodyKind||'personal',e=E(i.emp);c={id:this.nid(),item:it.id,kind,emp:kind==='personal'?i.emp:null,dept:kind==='shared'?e.dept:null,qty:i.qty,status:'pendingAcknowledgement',at:nowIso(),by:meId,ackAt:null,ackBy:null,retAt:null,req:i.req,issue:i.id,note:x.note||null};D.custs.push(c);m.cust=c.id;i.cust=c.id}
  i.status='delivered';i.delAt=nowIso();i.delBy=meId;i.move=m.id;closeReq(i);return issueDto(i)}
 case 'Custody_Create':{const it=item(x.itemId);if(it.kind!=='returnable')R(422,'Hr:Custody:ItemNotReturnable');const ok1=x.kind==='personal'?(x.employeeId&&!x.departmentId):(x.kind==='shared'?(x.departmentId&&!x.employeeId):false);if(!ok1)R(422,'Hr:Custody:InvalidHolder');if(!(x.quantity>0))R(400,'validation');
  if(x.kind==='personal')need(E(x.employeeId),'Hr:Employee:EmployeeNotFound');else need(db.D.find(z=>z.id===x.departmentId),'Hr:Department:NotFound');
  if(x.deductFromStock&&this.stQty(it.id)<x.quantity)R(422,'Hr:Stock:InsufficientStock');
  const c={id:this.nid(),item:it.id,kind:x.kind,emp:x.employeeId||null,dept:x.departmentId||null,qty:x.quantity,status:'pendingAcknowledgement',at:nowIso(),by:meId,ackAt:null,ackBy:null,retAt:null,req:null,issue:null,note:x.note||null};D.custs.push(c);
  if(x.deductFromStock){const m=this.stMove(it.id,'issue',-x.quantity,{by:meId,cust:c.id,emp:c.emp,note:x.note||null})}return custDto(c)}
 case 'Custody_GetList':{const a=custFilter(D.custs.slice(),x);return{totalCount:a.length,items:pg(a,x).items.map(custDto)}}
 case 'Custody_GetMy':{let a=D.custs.filter(c=>c.kind==='personal'&&c.emp===meId);if(x.status)a=a.filter(c=>c.status===x.status);a.sort((p,z)=>z.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(custDto)}}
 case 'Custody_GetTeam':{const ids=myTeam().map(e=>e.id),ds=myDepts();let a=D.custs.filter(c=>(c.kind==='personal'&&ids.includes(c.emp))||(c.kind==='shared'&&ds.includes(c.dept)));a=custFilter(a,x);return{totalCount:a.length,items:pg(a,x).items.map(custDto)}}
 case 'Custody_Get':{const c=need(D.custs.find(z=>z.id===x.id),'Hr:Custody:NotFound');const ok1=(c.kind==='personal'&&c.emp===meId)||hasR('storeOfficer')||hasR('hrManager')||(hasR('departmentManager')&&((c.kind==='personal'&&(E(c.emp)||{}).mgr===meId)||(c.kind==='shared'&&myDepts().includes(c.dept))));if(!ok1)R(403,'forbidden');return custDto(c)}
 case 'Custody_Acknowledge':{const c=need(D.custs.find(z=>z.id===x.id),'Hr:Custody:NotFound');const ok1=c.kind==='personal'?c.emp===meId:myDepts().includes(c.dept);if(!ok1)R(403,'forbidden');if(c.status==='returned')R(422,'Hr:Custody:AlreadyReturned');if(c.status==='acknowledged')R(422,'Hr:Custody:AlreadyAcknowledged');c.status='acknowledged';c.ackAt=nowIso();c.ackBy=meId;return custDto(c)}
 case 'Custody_Return':{const c=need(D.custs.find(z=>z.id===x.id),'Hr:Custody:NotFound');if(c.status==='returned')R(422,'Hr:Custody:AlreadyReturned');if(typeof x.restock!=='boolean')R(400,'validation');if(!x.restock&&!(x.note&&x.note.trim()))R(422,'Hr:Custody:ReturnNoteRequired');
  c.status='returned';c.retAt=nowIso();if(x.note)c.rnote=x.note;if(x.restock)this.stMove(c.item,'return',c.qty,{by:meId,cust:c.id,emp:c.emp,note:x.note||null});return custDto(c)}
 case 'CustodyCount_Create':{if(!x.lines||!x.lines.length||!x.departmentId||!x.countDate)R(400,'validation');if(!myDepts().includes(x.departmentId))R(422,'Hr:Custody:DepartmentNotInScope');
  const open=D.custs.filter(c=>c.kind==='shared'&&c.dept===x.departmentId&&c.status!=='returned');const seen={};
  x.lines.forEach(l=>{const c=D.custs.find(z=>z.id===l.custodyId);if(!c||c.kind!=='shared'||c.dept!==x.departmentId||c.status==='returned'||seen[l.custodyId]||!(l.countedQuantity>=0))R(422,'Hr:Custody:CountLineInvalid');seen[l.custodyId]=1});
  if(open.some(c=>!seen[c.id]))R(422,'Hr:Custody:CountLinesIncomplete');
  const lines=x.lines.map(l=>({cust:l.custodyId,exp:D.custs.find(z=>z.id===l.custodyId).qty,cnt:l.countedQuantity,note:l.note||null}));const c={id:this.nid(),dept:x.departmentId,date:x.countDate,result:lines.every(l=>l.exp===l.cnt)?'matched':'discrepancy',note:x.note||null,by:meId,at:nowIso(),lines};D.counts.push(c);return countDto(c)}
 case 'CustodyCount_GetList':{let a=D.counts.slice();if(hasR('departmentManager')&&!hasR('storeOfficer')&&!hasR('hrManager'))a=a.filter(c=>myDepts().includes(c.dept));if(x.departmentId)a=a.filter(c=>c.dept===x.departmentId);if(x.result)a=a.filter(c=>c.result===x.result);if(x.fromDate)a=a.filter(c=>c.date>=x.fromDate);if(x.toDate)a=a.filter(c=>c.date<=x.toDate);a.sort((p,z)=>z.date.localeCompare(p.date)||z.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(countDto)}}
 }
 return undefined};
svItemDlg=(T,i)=>({title:T.dSvItem,body:T.dSvItemB,fields:[{k:'a',l:T.fSvNameAr,req:1,max:128,init:i?i.nameAr:''},{k:'e',l:T.fSvNameEn,req:1,max:128,init:i?i.nameEn:''},{k:'k',l:T.fSvKind,t:'sel',req:1,init:i?i.kind:'returnable',opts:Object.keys(T.sk).map(x=>({v:x,l:T.sk[x]}))},{k:'m',l:T.fSvMin,req:1,max:9,pat:/^\d+(\.\d+)?$/,init:i?String(i.minQuantity):'0'},...(i?[{k:'t',l:T.fSvActive,t:'sel',req:1,init:String(i.isActive),opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]}]:[])],okLabel:T.save2,
 run:async(v)=>{const b={nameAr:v.a,nameEn:v.e,kind:v.k,minQuantity:Number(v.m)};if(i)await this.call('Item_Update',{id:i.id,...b,isActive:v.t==='true',concurrencyStamp:i.concurrencyStamp});else await this.call('Item_Create',b);return{m:T.tSvItem}}});
svRecvDlg=(T,items,pur,it0)=>({title:T.dSvRecv,body:T.dSvRecvB,fields:[{k:'i',l:T.fSvItem,t:'sel',req:1,init:it0||'',opts:items.map(z=>({v:z.id,l:z.name}))},{k:'q',l:T.fSvQty,req:1,max:9,pat:/^\d+(\.\d+)?$/},{k:'c',l:T.fSvCost,max:12,pat:/^\d+(\.\d+)?$/},{k:'s',l:T.fSvIssue,t:'sel',opts:pur.map(z=>({v:z.id,l:z.item.name+' × '+z.quantity+' · '+z.employee.fullName}))},{k:'n',l:T.fSvNote,t:'ta',max:2000}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Stock_Receive',{itemId:v.i,quantity:Number(v.q),totalCost:v.c?Number(v.c):undefined,stockIssueId:v.s||undefined,note:v.n||undefined});return{m:T.tSvMove}}});
svAdjDlg=(T,items,moves)=>({title:T.dSvAdj,body:T.dSvAdjB,fields:[{k:'i',l:T.fSvItem,t:'sel',req:1,opts:items.map(z=>({v:z.id,l:z.name}))},{k:'q',l:T.fSvQty,req:1,max:10,pat:/^-?\d+(\.\d+)?$/},{k:'r',l:T.fSvReason,t:'ta',req:1,max:2000},{k:'v',l:T.fSvRev,t:'sel',opts:moves.map(z=>({v:z.id,l:T.mvt[z.type]+' '+z.quantity+' · '+z.item.name}))}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Stock_Adjust',{itemId:v.i,quantity:Number(v.q),reason:v.r,reversesMovementId:v.v||undefined});return{m:T.tSvMove}}});
svNoteDlg=(T,title,op,id,msg)=>({title,fields:[{k:'n',l:T.fSvNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call(op,{id,note:v.n||undefined});return{m:msg}}});
svDelDlg=(T,i)=>({title:T.dSvDel,body:T.dSvDelB,fields:[...(i.item.kind==='returnable'?[{k:'k',l:T.fSvDelKind,t:'sel',req:1,init:'personal',opts:Object.keys(T.ckind).map(x=>({v:x,l:T.ckind[x]}))}]:[]),{k:'n',l:T.fSvNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('StockIssue_Deliver',{id:i.id,custodyKind:v.k||undefined,note:v.n||undefined});return{m:T.tSvDel}}});
cuNewDlg=(T,items,emps,depts)=>({title:T.dCuNew,body:T.dCuNewB,fields:[{k:'i',l:T.fCuItem,t:'sel',req:1,opts:items.map(z=>({v:z.id,l:z.name}))},{k:'k',l:T.fCuKind,t:'sel',req:1,init:'personal',opts:Object.keys(T.ckind).map(x=>({v:x,l:T.ckind[x]}))},{k:'e',l:T.fCuEmp,t:'sel',opts:emps},{k:'d',l:T.fCuDept,t:'sel',opts:depts},{k:'q',l:T.fCuQty,req:1,max:9,pat:/^\d+(\.\d+)?$/,init:'1'},{k:'s',l:T.fCuDeduct,t:'sel',req:1,init:'true',opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]},{k:'n',l:T.fCuNote,t:'ta',max:2000}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Custody_Create',{itemId:v.i,kind:v.k,employeeId:v.e||undefined,departmentId:v.d||undefined,quantity:Number(v.q),deductFromStock:v.s==='true',note:v.n||undefined});return{m:T.tCuNewT}}});
cuRetDlg=(T,c)=>({title:T.dCuRet,body:T.dCuRetB,fields:[{k:'r',l:T.fCuRestock,t:'sel',req:1,init:'true',opts:[{v:'true',l:T.yes},{v:'false',l:T.no}]},{k:'n',l:T.fCuNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Custody_Return',{id:c.id,restock:v.r==='true',note:v.n||undefined});return{m:T.tCuRetT}}});
cuDetDlg=(T,c,ar)=>({title:T.dCuDet,fields:[],kv:[{k:T.cucItem,v:c.item.name+' ('+T.sk[c.item.kind]+')'},{k:T.cucKind,v:T.ckind[c.kind]},{k:T.cucHolder,v:c.employee?c.employee.fullName:(c.department?c.department.name:'—')},{k:T.cucQty,v:String(c.quantity)},{k:T.cucSt,v:T.cust[c.status]},{k:T.cucAt,v:this.fmtD(c.assignedAt.slice(0,10),ar)+' · '+c.assignedBy.displayName},{k:T.cucAck,v:c.acknowledgedAt?this.fmtD(c.acknowledgedAt.slice(0,10),ar)+' · '+c.acknowledgedBy.displayName:'—'},{k:T.cucRet,v:c.returnedAt?this.fmtD(c.returnedAt.slice(0,10),ar):'—'},{k:T.cucNote,v:c.note||'—'}],okLabel:T.cnf,run:null});
stHolders=(c)=>{if(c.kind==='personal')return[c.emp];return this.db.emps.filter(e=>e.status==='active'&&e.roles.includes('departmentManager')&&this.db.emps.some(z=>z.mgr===e.id&&z.dept===c.dept&&z.status!=='ended')).map(e=>e.id)};
d12=null;
mk12=()=>{
 if(!this.d11)this.mk11();const U=this.UID,db=this.db,td=this.todayS();
 Object.assign(this.NT,{contractExpiring:['عقود تقترب من الانتهاء','هناك عقود دخلت مدة التنبيه بانتظار التوصية.','Contracts approaching expiry','Some contracts entered the alert window and wait for a recommendation.'],contractDecisionDue:['قرار عقد مستحق','هناك عقد بانتظار القرار.','A contract decision is due','A contract is waiting for its decision.'],contractDecided:['قرار بخصوص عقدك','صدر قرار بخصوص عقدك.','A decision on your contract','A decision was issued on your contract.']});
 this.d12={cons:[],alert:30,stamp:'a',astamp:'a'};const D=this.d12;
 const ago=(d)=>this.dAdd(td,d);
 const mk=(k,type,start,end,st,o)=>{const c={id:this.nid(),emp:U(k),type,start,end,prev:null,status:st,rec:null,dec:null,due:null,notified:null,created:start+'T08:00:00Z',...(o||{})};D.cons.push(c);return c};
 const rec=(c,opt,pos,by)=>{c.rec={position:pos,option:opt,note:null,by,at:new Date().toISOString()};c.due=this.evNth(this.dAdd(td,-1),5)};
 const HR=(db.emps.find(e=>e.roles.includes('hrManager'))||{}).id;
 mk('11','سنوي',ago(-165),ago(200),'active');
 mk('12','سنوي',ago(-340),ago(25),'pendingRecommendation');
 mk('18','سنوي',ago(-350),ago(15),'pendingRecommendation');
 const y=mk('20','سنوي',ago(-355),ago(10),'pendingDecision');rec(y,'renew','أداء جيد والتزام ممتاز',HR);
 const z=mk('19','سنوي',ago(-368),ago(-3),'pendingDecision');rec(z,'doNotRenew','يحتاج تحسيناً بالأداء',HR);z.due=this.dAdd(td,-1);
 const k=mk('14','سنوي',ago(-400),ago(-35),'renewed');rec(k,'renew','ممتاز',HR);k.dec={decision:'renew',note:'اعتُمد التجديد',msg:'تم تجديد عقدك لسنة قادمة.',by:HR,at:ago(-40)+'T09:00:00Z'};k.notified=ago(-40)+'T09:00:00Z';
 mk('14','سنوي',ago(-34),ago(331),'active',{prev:k.id});
 const n=mk('21','تحت التجربة',ago(-200),ago(-20),'notRenewed');rec(n,'doNotRenew','لم يجتز فترة التجربة',HR);n.dec={decision:'doNotRenew',note:null,msg:'نأسف، لن يُجدَّد العقد.',by:HR,at:ago(-25)+'T09:00:00Z'};n.notified=ago(-25)+'T09:00:00Z';
 mk('13','سنوي',ago(-330),ago(20),'pendingRecommendation');
 mk('22','سنوي',ago(-60),ago(305),'active');mk('23','سنوي',ago(-100),ago(265),'active');
 this.ntf(HR,'contractExpiring','none');
};
ctSync=()=>{if(!this.d12)this.mk12();const D=this.d12,td=this.todayS(),lim=this.dAdd(td,D.alert);let moved=0;D.cons.forEach(c=>{if(c.status==='active'&&c.end<=lim){c.status='pendingRecommendation';moved++}});
 if(moved){const hr=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'));if(hr)this.ntf(hr.id,'contractExpiring','none')}};
ctDto=(c,ix)=>{const E=id=>this.db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,td=this.todayS();const e=E(c.emp),d=this.db.D.find(z=>z.id===e.dept);const uref=id=>{const u=E(id);return u?{id:u.id,displayName:nmo(u)}:null};
 return{id:c.id,employee:{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null},contractType:c.type,startDate:c.start,endDate:c.end,previousContractId:c.prev,status:c.status,daysRemaining:Math.round((new Date(c.end+'T12:00:00Z')-new Date(td+'T12:00:00Z'))/864e5),isExpired:c.end<td,
  recommendation:c.rec?{employeePosition:c.rec.position,recommendation:c.rec.option,note:c.rec.note,recommendedBy:uref(c.rec.by),recommendedAt:c.rec.at}:null,decision:c.dec?{decision:c.dec.decision,note:c.dec.note,employeeMessage:c.dec.msg,decidedBy:uref(c.dec.by),decidedAt:c.dec.at}:null,decisionDueDate:c.due,isDecisionOverdue:c.status==='pendingDecision'&&!!c.due&&td>c.due,employeeNotifiedAt:c.notified,creationTime:c.created,lastModificationTime:null}};
route12=(op,x,P,v,ix,R)=>{
 if(!(op in OPR12)) return undefined;
 if(!this.d12) this.mk12();
 const D=this.d12,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR12[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id);const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const NF='Hr:Contract:NotFound',need=(o,c)=>{if(!o)R(404,c||NF);return o};
 const dto=(c)=>this.ctDto(c,ix);
 const myDto=(c)=>{const st=c.status==='renewed'?'renewed':c.status==='notRenewed'?'notRenewed':'active';return{id:c.id,contractType:c.type,startDate:c.start,endDate:c.end,status:st,isExpired:c.end<this.todayS(),employeeMessage:c.dec?c.dec.msg:null,decidedAt:c.dec?c.dec.at:null}};
 if(op!=='SystemSettings_GetContracts'&&op!=='SystemSettings_UpdateContracts'&&op.indexOf('GetMy')<0)this.ctSync();
 switch(op){
 case 'Contract_List':{let a=D.cons.slice();if(x.employeeId)a=a.filter(c=>c.emp===x.employeeId);if(x.departmentId)a=a.filter(c=>(E(c.emp)||{}).dept===x.departmentId);if(x.status)a=a.filter(c=>c.status===x.status);if(x.endDateFrom)a=a.filter(c=>c.end>=x.endDateFrom);if(x.endDateTo)a=a.filter(c=>c.end<=x.endDateTo);a.sort((p,q)=>p.end.localeCompare(q.end));return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'Contract_Create':{const e=need(E(x.employeeId),'Hr:Contract:EmployeeNotFound');if(!x.contractType||x.contractType.length>100||!x.startDate||!x.endDate)R(400,'validation');if(x.endDate<=x.startDate)R(422,'Hr:Contract:InvalidDateRange');
  let pv=null;if(x.previousContractId){pv=D.cons.find(c=>c.id===x.previousContractId);if(!pv||pv.emp!==e.id||pv.status!=='renewed')R(422,'Hr:Contract:InvalidPreviousContract')}
  if(D.cons.some(c=>c.emp===e.id&&c.status!=='notRenewed'&&(!pv||c.id!==pv.id)&&!(x.endDate<c.start||x.startDate>c.end)))R(422,'Hr:Contract:OverlappingContract');
  const lim=this.dAdd(this.todayS(),D.alert);const c={id:this.nid(),emp:e.id,type:x.contractType,start:x.startDate,end:x.endDate,prev:pv?pv.id:null,status:x.endDate<=lim?'pendingRecommendation':'active',rec:null,dec:null,due:null,notified:null,created:nowIso()};D.cons.push(c);return dto(c)}
 case 'Contract_GetExpiring':{const n=x.withinDays||D.alert,lim=this.dAdd(this.todayS(),n);let a=D.cons.filter(c=>['active','pendingRecommendation','pendingDecision'].includes(c.status)&&c.end<=lim);if(x.departmentId)a=a.filter(c=>(E(c.emp)||{}).dept===x.departmentId);a.sort((p,q)=>p.end.localeCompare(q.end));return{totalCount:a.length,items:pg(a,x).items.map(dto),alertDays:D.alert}}
 case 'Contract_Get':return dto(need(D.cons.find(z=>z.id===x.id)));
 case 'Contract_Recommend':{const c=need(D.cons.find(z=>z.id===x.id));if(c.emp===meId)R(422,'Hr:Contract:SelfActionNotAllowed');if(c.status!=='pendingRecommendation')R(422,'Hr:Contract:InvalidStatus');if(!x.employeePosition||x.employeePosition.length>2000||!['renew','doNotRenew'].includes(x.recommendation))R(400,'validation');
  c.rec={position:x.employeePosition,option:x.recommendation,note:x.note||null,by:meId,at:nowIso()};c.status='pendingDecision';c.due=this.evNth(this.dAdd(this.todayS(),1),5);return dto(c)}
 case 'Contract_Decide':{const c=need(D.cons.find(z=>z.id===x.id));if(c.emp===meId)R(422,'Hr:Contract:SelfActionNotAllowed');if(c.status!=='pendingDecision')R(422,'Hr:Contract:InvalidStatus');if(!['renew','doNotRenew'].includes(x.decision))R(400,'validation');
  c.dec={decision:x.decision,note:x.note||null,msg:x.employeeMessage||null,by:meId,at:nowIso()};c.status=x.decision==='renew'?'renewed':'notRenewed';c.notified=nowIso();return dto(c)}
 case 'Contract_GetMyList':{let a=D.cons.filter(c=>c.emp===meId).sort((p,q)=>q.end.localeCompare(p.end));return{totalCount:a.length,items:pg(a,x).items.map(myDto)}}
 case 'Contract_GetMy':return myDto(need(D.cons.find(z=>z.id===x.id&&z.emp===meId)));
 case 'SystemSettings_GetContracts':return{alertDays:D.alert,concurrencyStamp:D.astamp,lastModificationTime:D.amod||null};
 case 'SystemSettings_UpdateContracts':{if(!Number.isInteger(x.alertDays)||x.alertDays<1||x.alertDays>365)R(400,'validation');if(D.astamp!==x.concurrencyStamp)R(409,'conflict');D.alert=x.alertDays;D.astamp=stamp();D.amod=nowIso();return{alertDays:D.alert,concurrencyStamp:D.astamp,lastModificationTime:D.amod}}
 }
 return undefined};
ctNewDlg=(T,emps,rens,pre)=>({title:T.dCtNew,body:T.dCtNewB,fields:[{k:'e',l:T.fCtEmp,t:'sel',req:1,init:pre?pre.employee.id:'',opts:emps},{k:'t',l:T.fCtType,req:1,max:100,init:pre?pre.contractType:''},{k:'s',l:T.fCtStart,t:'date',req:1,init:pre?this.dAdd(pre.endDate,1):''},{k:'n',l:T.fCtEnd,t:'date',req:1},{k:'p',l:T.fCtPrev,t:'sel',init:pre?pre.id:'',opts:rens.map(z=>({v:z.id,l:z.employee.fullName+' · '+z.endDate}))}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Contract_Create',{employeeId:v.e,contractType:v.t,startDate:v.s,endDate:v.n,previousContractId:v.p||undefined});return{m:T.tCtNew}}});
ctRecDlg=(T,c)=>({title:T.dCtRec,body:T.dCtRecB,fields:[{k:'p',l:T.fCtPos,t:'ta',req:1,max:2000},{k:'o',l:T.fCtOpt,t:'sel',req:1,opts:Object.keys(T.copt).map(x=>({v:x,l:T.copt[x]}))},{k:'n',l:T.fCtNote,t:'ta',max:2000}],okLabel:T.cnf,run:async(v)=>{await this.call('Contract_Recommend',{id:c.id,employeePosition:v.p,recommendation:v.o,note:v.n||undefined});return{m:T.tCtRec}}});
ctDecDlg=(T,c)=>({title:T.dCtDec,body:T.dCtDecB,fields:[{k:'d',l:T.fCtOpt,t:'sel',req:1,init:c.recommendation?c.recommendation.recommendation:'',opts:Object.keys(T.copt).map(x=>({v:x,l:T.copt[x]}))},{k:'n',l:T.fCtNote,t:'ta',max:2000},{k:'m',l:T.fCtMsg,t:'ta',max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Contract_Decide',{id:c.id,decision:v.d,note:v.n||undefined,employeeMessage:v.m||undefined});return{m:T.tCtDec}}});
d13=null;
mk13=()=>{
 if(!this.d12)this.mk12();const U=this.UID,db=this.db,td=this.todayS();
 Object.assign(this.NT,{warningRecommended:['توصية إنذار جديدة','وصلتك توصية إنذار للمراجعة.','New warning recommendation','A warning recommendation is waiting for your review.'],warningNotified:['إشعار رسمي','لديك إشعار رسمي بانتظار ردّك. افتح التطبيق للاطلاع.','Official notice','You have an official notice waiting for your response. Open the app to view it.'],warningReadyForDecision:['إنذار جاهز للقرار','هناك إنذار جاهز لقرارك.','A warning is ready for decision','A warning is ready for your decision.'],warningDecided:['صدر قرار','صدر قرار بخصوص الإشعار الرسمي.','A decision was issued','A decision was issued on the official notice.'],warningObjectionSubmitted:['اعتراض جديد','وصلك اعتراض على إنذار.','New objection','An objection on a warning was submitted.'],warningObjectionResolved:['بُتّ في اعتراضك','صدر قرار نهائي في اعتراضك.','Your objection was decided','A final decision was issued on your objection.'],warningObjectionResolvedForHr:['بُتّ في اعتراض','صدر قرار نهائي في اعتراض على إنذار.','An objection was decided','A final decision was issued on a warning objection.']});
 this.d13={ws:[]};const D=this.d13;const REV=(db.emps.find(e=>e.roles.includes('reviewer'))||{}).id,HR=(db.emps.find(e=>e.roles.includes('hrManager'))||{}).id,EX=(db.emps.find(e=>e.roles.includes('executiveDirector'))||{}).id;
 const ago=(d)=>new Date(Date.now()-d*864e5).toISOString();
 const mk=(emp,lvl,desc,by,role,st,o)=>{const w={id:this.nid(),emp:U(emp),lvl,date:this.dAdd(td,-10),desc,atts:[],by,role,status:st,notifiedAt:null,due:null,resp:null,dec:null,obj:null,created:ago(9),...(o||{})};D.ws.push(w);return w};
 const att=(w,name)=>w.atts.push({id:this.nid(),fileName:name,contentType:'application/pdf',sizeBytes:48213,creationTime:w.created});
 const w1=mk('18','first','تأخر متكرر عن مواعيد تسليم التقارير الأسبوعية.',U('12'),'departmentManager','recommended');att(w1,'تقرير_التأخير.pdf');
 mk('20','first','عدم الالتزام بتعليمات السلامة في الموقع.',U('12'),'departmentManager','notified',{notifiedAt:ago(1),due:this.evNth(this.dAdd(td,1),2)});
 mk('19','final','مخالفة لسياسة استخدام الأجهزة.',U('12'),'departmentManager','responded',{notifiedAt:ago(2),due:this.dAdd(td,1),resp:{content:'أعتذر عن الواقعة وأتعهد بعدم تكرارها.',at:ago(1)}});
 mk('14','first','غياب بدون إشعار مسبق.',U('12'),'departmentManager','notified',{notifiedAt:ago(8),due:this.dAdd(td,-2)});
 const w5=mk('11','first','إخلال بسرية المعلومات الداخلية.',U('12'),'departmentManager','issued',{date:this.dAdd(td,-40),notifiedAt:ago(30),due:this.dAdd(td,-26),resp:{content:'لم أقصد ذلك.',at:ago(28)},dec:{outcome:'issue',issuedLevel:'first',note:'ثبتت المخالفة بعد المراجعة.',by:HR,at:ago(20)}});
 const w6=mk('22','first','سلوك غير لائق مع زميل.',U('12'),'departmentManager','objected',{date:this.dAdd(td,-50),notifiedAt:ago(40),due:this.dAdd(td,-36),resp:{content:'أعترض على توصيف الواقعة.',at:ago(38)},dec:{outcome:'issue',issuedLevel:'first',note:'صدر الإنذار.',by:HR,at:ago(30)},obj:{content:'أعترض على القرار لأن الواقعة غير دقيقة.',at:ago(24),outcome:null,note:null,by:null,at2:null}});
 mk('21','first','تأخر في تسليم عهدة.',U('12'),'departmentManager','dismissed',{date:this.dAdd(td,-60),notifiedAt:ago(50),due:this.dAdd(td,-46),resp:{content:'سُلّمت العهدة في موعدها.',at:ago(48)},dec:{outcome:'dismiss',issuedLevel:null,note:'لم تثبت الواقعة.',by:HR,at:ago(40)}});
 mk('12','first','تأخر في رفع التقارير الشهرية.',REV,'reviewer','recommended');
 this.ntf(HR,'warningRecommended','warning',w1.id);this.ntf(HR,'warningReadyForDecision','warning',D.ws[2].id);this.ntf(EX,'warningObjectionSubmitted','warning',w6.id);
};
wrEnd=(due)=>new Date(due+'T23:59:59+03:00').getTime();
wrSync=()=>{if(!this.d13)this.mk13();const hr=this.db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'));this.d13.ws.forEach(w=>{if(w.status==='notified'&&Date.now()>this.wrEnd(w.due)){w.status='responseExpired';if(hr)this.ntf(hr.id,'warningReadyForDecision','warning',w.id)}})};
wrStage=(w)=>['issued','objected','objectionUpheld'].includes(w.status)?'issued':(['dismissed','objectionOverturned'].includes(w.status)?'notIssued':'underReview');
route13=(op,x,P,v,ix,R)=>{
 if(!(op in OPR13)) return undefined;
 if(!this.d13) this.mk13();
 const D=this.d13,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR13[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r),td=this.todayS();
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id),d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const nowIso=()=>new Date().toISOString();const NF='Hr:Warning:NotFound',need=(o)=>{if(!o)R(404,NF);return o};
 const L=T4[this.state.lang];
 this.wrSync();
 const ref=(w)=>w;const hrId=()=>(db.emps.find(e=>e.status==='active'&&e.roles.includes('hrManager'))||{}).id;
 const aDto=(a)=>({id:a.id,fileName:a.fileName,contentType:a.contentType,sizeBytes:a.sizeBytes,creationTime:a.creationTime});
 const dto=(w)=>({id:w.id,employee:eref(w.emp),recommendedLevel:w.lvl,incidentDate:w.date,description:w.desc,attachments:w.atts.map(aDto),recommendedBy:uref(w.by),recommenderRole:w.role,status:w.status,notifiedAt:w.notifiedAt,responseDueDate:w.due,employeeResponse:w.resp?{content:w.resp.content,submittedAt:w.resp.at}:null,decision:w.dec?{outcome:w.dec.outcome,issuedLevel:w.dec.issuedLevel,note:w.dec.note,decidedBy:uref(w.dec.by),decidedAt:w.dec.at}:null,objection:w.obj?{content:w.obj.content,submittedAt:w.obj.at,outcome:w.obj.outcome,resolutionNote:w.obj.note,resolvedBy:w.obj.by?uref(w.obj.by):null,resolvedAt:w.obj.at2}:null,creationTime:w.created});
 const empDto=(w)=>({id:w.id,level:w.dec&&w.dec.issuedLevel?w.dec.issuedLevel:w.lvl,incidentDate:w.date,description:w.desc,status:w.status,notifiedAt:w.notifiedAt,responseDueDate:w.due,canRespond:w.status==='notified'&&Date.now()<=this.wrEnd(w.due),canObject:w.status==='issued'&&!w.obj,myResponse:w.resp?{content:w.resp.content,submittedAt:w.resp.at}:null,decisionNote:w.dec?w.dec.note:null,decidedAt:w.dec?w.dec.at:null,myObjection:w.obj?{content:w.obj.content,submittedAt:w.obj.at,outcome:w.obj.outcome,resolutionNote:w.obj.note,resolvedAt:w.obj.at2}:null});
 const recDto=(w)=>({id:w.id,employee:eref(w.emp),recommendedLevel:w.lvl,incidentDate:w.date,description:w.desc,stage:this.wrStage(w),issuedLevel:w.dec&&w.dec.issuedLevel?w.dec.issuedLevel:null,decidedAt:w.dec?w.dec.at:null,creationTime:w.created});
 const mineW=(id)=>{const w=need(D.ws.find(z=>z.id===id));if(w.emp!==meId||w.status==='recommended')R(404,NF);return w};
 const hrW=(id)=>need(D.ws.find(z=>z.id===id));
 const execOk=(w)=>{if(hasR('hrManager'))return;if(!(hasR('executiveDirector')&&w.obj))R(404,NF)};
 const team=()=>db.emps.filter(e=>e.mgr===meId&&e.status!=='ended');
 switch(op){
 case 'Warning_Create':{const e=E(x.employeeId);if(!e)R(404,'Hr:Warning:EmployeeNotFound');
  if(!['first','final'].includes(x.recommendedLevel)||!x.incidentDate||!x.description||x.description.length>4000||(x.attachments&&x.attachments.length>10))R(400,'validation');
  if(e.id===meId)R(422,'Hr:Warning:SelfRecommendationNotAllowed');
  if(hasR('reviewer')){if(!e.roles.includes('departmentManager'))R(422,'Hr:Warning:TargetNotDepartmentManager')}
  else{if(e.roles.includes('departmentManager'))R(422,'Hr:Warning:TargetIsDepartmentManager');if(e.mgr!==meId)R(422,'Hr:Warning:TargetNotInTeam')}
  if(x.incidentDate>td)R(422,'Hr:Warning:IncidentDateInFuture');
  const atts=(x.attachments||[]).map(a=>{if(!/\.(pdf|jpe?g|png)$/i.test(a.file||''))R(422,'Hr:Document:FileTypeNotAllowed');if((a.size||0)>10485760)R(422,'Hr:Document:FileTooLarge');if(/virus|infected/i.test(a.file))R(422,'Hr:Warning:AttachmentRejected');return{id:this.nid(),fileName:a.file,contentType:/pdf$/i.test(a.file)?'application/pdf':'image/jpeg',sizeBytes:a.size||120000,creationTime:nowIso()}});
  const w={id:this.nid(),emp:e.id,lvl:x.recommendedLevel,date:x.incidentDate,desc:x.description,atts,by:meId,role:hasR('reviewer')?'reviewer':'departmentManager',status:'recommended',notifiedAt:null,due:null,resp:null,dec:null,obj:null,created:nowIso()};D.ws.push(w);return recDto(w)}
 case 'Warning_List':{let a=D.ws.slice();if(x.employeeId)a=a.filter(w=>w.emp===x.employeeId);if(x.departmentId)a=a.filter(w=>(E(w.emp)||{}).dept===x.departmentId);if(x.status)a=a.filter(w=>w.status===x.status);if(x.level)a=a.filter(w=>w.lvl===x.level);if(x.incidentDateFrom)a=a.filter(w=>w.date>=x.incidentDateFrom);if(x.incidentDateTo)a=a.filter(w=>w.date<=x.incidentDateTo);a.sort((p,q)=>q.created.localeCompare(p.created));return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'Warning_GetObjections':{let a=D.ws.filter(w=>w.obj);if(x.isPending!==undefined&&x.isPending!=='')a=a.filter(w=>!w.obj.outcome===(x.isPending===true||x.isPending==='true'));a.sort((p,q)=>q.obj.at.localeCompare(p.obj.at));return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'Warning_GetRecommendedByMe':{let a=D.ws.filter(w=>w.by===meId);if(x.stage)a=a.filter(w=>this.wrStage(w)===x.stage);a.sort((p,q)=>q.created.localeCompare(p.created));return{totalCount:a.length,items:pg(a,x).items.map(recDto)}}
 case 'Warning_GetMyList':{let a=D.ws.filter(w=>w.emp===meId&&w.status!=='recommended').sort((p,q)=>q.notifiedAt.localeCompare(p.notifiedAt));return{totalCount:a.length,items:pg(a,x).items.map(empDto)}}
 case 'Warning_GetMy':return empDto(mineW(x.id));
 case 'Warning_GetEmployeeFile':{const e=need(E(x.employeeId));if(!hasR('hrManager')&&!(hasR('departmentManager')&&e.mgr===meId))R(404,NF);let a=D.ws.filter(w=>w.emp===e.id&&w.dec);if(!hasR('hrManager'))a=a.filter(w=>['issued','objected','objectionUpheld'].includes(w.status));a.sort((p,q)=>q.dec.at.localeCompare(p.dec.at));
  return{totalCount:a.length,items:pg(a,x).items.map(w=>({id:w.id,level:w.dec.issuedLevel||w.lvl,incidentDate:w.date,description:w.desc,status:w.status,decisionNote:w.dec.note,decidedAt:w.dec.at,employeeResponse:w.resp?{content:w.resp.content,submittedAt:w.resp.at}:null,objectionOutcome:w.obj?w.obj.outcome:null}))}}
 case 'Warning_Get':{const w=hrW(x.id);execOk(w);return dto(w)}
 case 'Warning_GetAttachmentDownloadUrl':{const w=hrW(x.id);execOk(w);const a=need(w.atts.find(z=>z.id===x.attachmentId));return{url:'https://files.example.test/warning/'+a.id,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'Warning_Notify':{const w=hrW(x.id);if(w.emp===meId)R(422,'Hr:Warning:SelfDecisionNotAllowed');if(w.status!=='recommended')R(422,'Hr:Warning:InvalidStatus');w.status='notified';w.notifiedAt=nowIso();w.due=this.evNth(this.dAdd(td,1),3);return dto(w)}
 case 'Warning_Respond':{const w=mineW(x.id);if(!['notified','responded','responseExpired'].includes(w.status))R(422,'Hr:Warning:InvalidStatus');if(w.status==='responded'||w.resp)R(422,'Hr:Warning:ResponseAlreadySubmitted');if(w.status==='responseExpired'||(w.status==='notified'&&Date.now()>this.wrEnd(w.due)))R(422,'Hr:Warning:ResponseWindowExpired');if(!x.content||x.content.length>4000)R(400,'validation');
  w.resp={content:x.content,at:nowIso()};w.status='responded';return empDto(w)}
 case 'Warning_Decide':{const w=hrW(x.id);if(w.emp===meId)R(422,'Hr:Warning:SelfDecisionNotAllowed');if(w.status==='notified'&&Date.now()<=this.wrEnd(w.due))R(422,'Hr:Warning:ResponseWindowOpen');if(!['responded','responseExpired'].includes(w.status))R(422,'Hr:Warning:InvalidStatus');
  if(!['issue','dismiss'].includes(x.decision)||!x.note||x.note.length>2000)R(400,'validation');if(x.decision==='issue'&&!['first','final'].includes(x.issuedLevel))R(422,'Hr:Warning:LevelRequired');
  w.dec={outcome:x.decision,issuedLevel:x.decision==='issue'?x.issuedLevel:null,note:x.note,by:meId,at:nowIso()};w.status=x.decision==='issue'?'issued':'dismissed';return dto(w)}
 case 'Warning_Object':{const w=mineW(x.id);if(w.obj)R(422,'Hr:Warning:ObjectionAlreadySubmitted');if(w.status!=='issued')R(422,'Hr:Warning:InvalidStatus');if(!x.content||x.content.length>4000)R(400,'validation');w.obj={content:x.content,at:nowIso(),outcome:null,note:null,by:null,at2:null};w.status='objected';return empDto(w)}
 case 'Warning_ResolveObjection':{const w=hrW(x.id);if(!w.obj)R(404,NF);if(w.status!=='objected')R(422,'Hr:Warning:InvalidStatus');if(!['uphold','overturn'].includes(x.outcome)||!x.note||x.note.length>2000)R(400,'validation');
  w.obj.outcome=x.outcome;w.obj.note=x.note;w.obj.by=meId;w.obj.at2=nowIso();w.status=x.outcome==='uphold'?'objectionUpheld':'objectionOverturned';return dto(w)}
 case 'Report_GetHrWarnings':{if(!x.from||!x.to)R(400,'validation');if(x.to<x.from||(new Date(x.to)-new Date(x.from))/864e5>366)R(422,'Hr:Reports:InvalidPeriod');let a=D.ws.filter(w=>w.date>=x.from&&w.date<=x.to);if(x.employeeId)a=a.filter(w=>w.emp===x.employeeId);if(x.departmentId)a=a.filter(w=>(E(w.emp)||{}).dept===x.departmentId);
  return pg(a.sort((p,q)=>q.date.localeCompare(p.date)).map(w=>({warningId:w.id,employee:eref(w.emp),levelName:L.wlv[w.dec&&w.dec.issuedLevel?w.dec.issuedLevel:w.lvl],incidentDate:w.date,statusName:L.wst[w.status]})),x)}
 }
 return undefined};
wrNewDlg=(T,emps)=>({title:T.dWrNew,body:T.dWrNewB,fields:[{k:'e',l:T.fWrEmp,t:'sel',req:1,opts:emps},{k:'l',l:T.fWrLvl,t:'sel',req:1,init:'first',opts:Object.keys(T.wlv).map(x=>({v:x,l:T.wlv[x]}))},{k:'d',l:T.fWrDate,t:'date',req:1,init:this.todayS()},{k:'c',l:T.fWrDesc,t:'ta',req:1,max:4000},{k:'att',l:T.fWrAtt,t:'file',h:T.fWrAttH,up:(e)=>{const fl=e.target.files&&e.target.files[0];if(fl)this.setF('att',[...(Array.isArray(this.state.fv.att)?this.state.fv.att:[]),{id:fl.name+':'+fl.size,fileName:fl.name,sizeBytes:fl.size}])}}],okLabel:T.cnf,
 run:async(v)=>{await this.call('Warning_Create',{employeeId:v.e,recommendedLevel:v.l,incidentDate:v.d,description:v.c,attachments:(Array.isArray(v.att)?v.att:[]).map(a=>({file:a.fileName,size:a.sizeBytes}))});return{m:T.tWrNew,go:()=>this.toSub('list',null,{tab:'rec'})}}});
wrNotifyDlg=(T,w)=>({title:T.dWrNotify,body:T.dWrNotifyB,fields:[],okLabel:T.cnf,run:async()=>{await this.call('Warning_Notify',{id:w.id});return{m:T.tWrNot}}});
wrDecDlg=(T,w)=>({title:T.dWrDec,body:T.dWrDecB,fields:[{k:'d',l:T.fWrDecision,t:'sel',req:1,opts:Object.keys(T.wdo).map(x=>({v:x,l:T.wdo[x]}))},{k:'l',l:T.fWrIssued,t:'sel',init:w.recommendedLevel,opts:Object.keys(T.wlv).map(x=>({v:x,l:T.wlv[x]}))},{k:'n',l:T.fWrNote,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Warning_Decide',{id:w.id,decision:v.d,issuedLevel:v.d==='issue'?(v.l||undefined):undefined,note:v.n});return{m:T.tWrDec}}});
wrTextDlg=(T,title,body,op,id,msg)=>({title,body,fields:[{k:'c',l:T.fWrContent,t:'ta',req:1,max:4000}],okLabel:T.cnf,run:async(v)=>{await this.call(op,{id,content:v.c});return{m:msg}}});
wrResDlg=(T,w)=>({title:T.dWrRes,body:T.dWrResB,fields:[{k:'o',l:T.fWrOutcome,t:'sel',req:1,opts:Object.keys(T.wob).map(x=>({v:x,l:T.wob[x]}))},{k:'n',l:T.fWrNote,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Warning_ResolveObjection',{id:w.id,outcome:v.o,note:v.n});return{m:T.tWrRes}}});
d14=null;
mk14=()=>{
 if(!this.d13)this.mk13();const U=this.UID,db=this.db;
 Object.assign(this.NT,{appreciationLetterIssued:['كتاب شكر جديد','أُصدر لك كتاب شكر وتقدير.','New appreciation letter','An appreciation letter was issued to you.']});
 this.d14={ls:[]};const D=this.d14,EX=(db.emps.find(e=>e.roles.includes('executiveDirector'))||{}).id;
 const ago=(d)=>new Date(Date.now()-d*864e5).toISOString();
 const mk=(k,title,note,d,ack)=>{const l={id:this.nid(),emp:U(k),title,note,by:EX,at:ago(d),file:{id:this.nid(),fileName:'appreciation-'+k+'.pdf',contentType:'application/pdf',sizeBytes:84000,creationTime:ago(d)},ackAt:ack?ago(d-1):null};D.ls.push(l);return l};
 mk('11','شكر وتقدير على الجهد المتميز','نقدّر التزامك وإنجازك في مشروع الربع الأخير.',40,true);
 mk('18','شكر على المبادرة','مبادرتك في تحسين إجراءات العمل محل تقدير.',12,false);
 mk('20','تقدير لجهود الفريق',null,5,false);
 mk('19','شكر على الالتزام','',90,true);
 this.ntf(U('18'),'appreciationLetterIssued','appreciationLetter',D.ls[1].id);this.ntf(U('20'),'appreciationLetterIssued','appreciationLetter',D.ls[2].id);
};
route14=(op,x,P,v,ix,R)=>{
 if(!(op in OPR14)) return undefined;
 if(!this.d14) this.mk14();
 const D=this.d14,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles;
 if(!roles.some(r=>OPR14[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar,hasR=r=>roles.includes(r);
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const eref=id=>{const e=E(id),d=db.D.find(z=>z.id===e.dept);return{id:e.id,fullName:nmo(e),departmentName:d?(ix?d.nameEn:d.nameAr):null}};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const NF='Hr:Appreciation:NotFound',need=(o)=>{if(!o)R(404,NF);return o};
 const dto=(l)=>({id:l.id,employee:eref(l.emp),title:l.title,note:l.note||null,issuedBy:uref(l.by),issuedAt:l.at,file:{id:l.file.id,fileName:l.file.fileName,contentType:l.file.contentType,sizeBytes:l.file.sizeBytes,creationTime:l.file.creationTime},isAcknowledged:!!l.ackAt,acknowledgedAt:l.ackAt});
 const inScope=(l)=>l.emp===meId&&hasR('employee')||(hasR('departmentManager')&&(E(l.emp)||{}).mgr===meId)||hasR('hrManager')||hasR('executiveDirector');
 const flag=(a,q)=>{if(q.isAcknowledged!==undefined&&q.isAcknowledged!=='')a=a.filter(l=>!!l.ackAt===(q.isAcknowledged===true||q.isAcknowledged==='true'));return a.slice().sort((p,z)=>z.at.localeCompare(p.at))};
 switch(op){
 case 'AppreciationLetter_Create':{const e=E(x.employeeId);if(!e)R(404,'Hr:Appreciation:EmployeeNotFound');if(e.id===meId)R(422,'Hr:Appreciation:SelfIssueNotAllowed');if(!x.title||x.title.length>200||(x.note&&x.note.length>2000)||!x.file)R(400,'validation');
  if(!/\.(pdf|jpe?g|png)$/i.test(x.file.name||''))R(422,'Hr:Document:FileTypeNotAllowed');if((x.file.size||0)>10485760)R(422,'Hr:Document:FileTooLarge');if(/virus|infected/i.test(x.file.name))R(422,'Hr:Appreciation:FileRejected');
  const l={id:this.nid(),emp:e.id,title:x.title,note:x.note||null,by:meId,at:new Date().toISOString(),file:{id:this.nid(),fileName:x.file.name,contentType:/pdf$/i.test(x.file.name)?'application/pdf':'image/jpeg',sizeBytes:x.file.size||84000,creationTime:new Date().toISOString()},ackAt:null};D.ls.push(l);return dto(l)}
 case 'AppreciationLetter_List':{let a=D.ls.slice();if(x.employeeId)a=a.filter(l=>l.emp===x.employeeId);a=flag(a,x);return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'AppreciationLetter_GetMyList':{let a=flag(D.ls.filter(l=>l.emp===meId),x);return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'AppreciationLetter_GetEmployeeFile':{const e=need(E(x.employeeId));if(!hasR('hrManager')&&!hasR('executiveDirector')&&!(hasR('departmentManager')&&e.mgr===meId))R(404,NF);let a=flag(D.ls.filter(l=>l.emp===e.id),x);return{totalCount:a.length,items:pg(a,x).items.map(dto)}}
 case 'AppreciationLetter_Get':{const l=need(D.ls.find(z=>z.id===x.id));if(!inScope(l))R(404,NF);return dto(l)}
 case 'AppreciationLetter_GetDownloadUrl':{const l=need(D.ls.find(z=>z.id===x.id));if(!inScope(l))R(404,NF);return{url:'https://files.example.test/appreciation/'+l.file.id,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'AppreciationLetter_Acknowledge':{const l=need(D.ls.find(z=>z.id===x.id&&z.emp===meId));if(!l.ackAt)l.ackAt=new Date().toISOString();return dto(l)}
 }
 return undefined};
apNewDlg=(T,emps)=>({title:T.dApNew,body:T.dApNewB,fields:[{k:'e',l:T.fApEmp,t:'sel',req:1,opts:emps},{k:'t',l:T.fApTitle,req:1,max:200},{k:'n',l:T.fApNote,t:'ta',max:2000},{k:'f',l:T.fApFile,t:'file',h:T.fApFileH,up:(e)=>{const fl=e.target.files&&e.target.files[0];if(fl)this.setF('f',[{id:fl.name+':'+fl.size,fileName:fl.name,sizeBytes:fl.size}])}}],okLabel:T.cnf,
 run:async(v)=>{const fl=(Array.isArray(v.f)?v.f:[])[0];await this.call('AppreciationLetter_Create',{employeeId:v.e,title:v.t,note:v.n||undefined,file:fl?{name:fl.fileName,size:fl.sizeBytes}:undefined});return{m:T.tApNewT,go:()=>this.toSub('list',null,{tab:'all'})}}});
d15=null;
FIN_READS=['SalaryProfile_List','SalaryProfile_Get','SalaryProfile_GetHistory','Adjustment_List','Adjustment_GetAttachmentDownloadUrl','AdjustmentSuggestion_List','Expense_List','Expense_Get','Expense_GetAttachmentDownloadUrl','PayrollRun_List','PayrollRun_Get','Payslip_GetMyList','Payslip_GetMy','Payslip_DownloadMyPdf','FinanceReport_GetMonthlyStatement','FinanceReport_GetAdjustments','FinanceReport_GetStoreExpenses'];
fnPeriod=(d)=>d.slice(0,7);
fnPrev=(p,n)=>{let y=Number(p.slice(0,4)),m=Number(p.slice(5,7))+n;while(m<1){m+=12;y--}while(m>12){m-=12;y++}return y+'-'+String(m).padStart(2,'0')};
fnLast=(p)=>{const y=Number(p.slice(0,4)),m=Number(p.slice(5,7));return new Date(Date.UTC(y,m,0)).toISOString().slice(0,10)};
fnProf=(emp,date)=>{const a=this.d15.profs.filter(p=>p.emp===emp&&p.from<=date).sort((x,z)=>z.from.localeCompare(x.from));return a[0]||null};
mk15=()=>{
 if(!this.d14)this.mk14();const U=this.UID,db=this.db,td=this.todayS(),cur=this.fnPeriod(td);
 Object.assign(this.NT,{adjustmentSuggestionsReady:['اقتراحات تسوية جاهزة','وُلّدت اقتراحات جديدة من بيانات الحضور بانتظار مراجعتك.','Adjustment suggestions are ready','New suggestions generated from attendance data are waiting for your review.']});
 this.d15={profs:[],adjs:[],sugs:[],exps:[],fin:{role:'hrManager',stamp:'a',mod:null},locked:[this.fnPrev(cur,-3),this.fnPrev(cur,-2)],stale:{}};const D=this.d15;
 const ACC=(db.emps.find(e=>e.roles.includes('accountant'))||{}).id,ago=(d)=>new Date(Date.now()-d*864e5).toISOString();
 const base={'11':1400000,'12':2100000,'13':2400000,'14':1500000,'16':1800000,'18':1100000,'19':1250000,'20':1200000,'21':1000000,'23':2100000};
 Object.keys(base).forEach(k=>{const comp=[{id:this.nid(),name:'بدل نقل',type:'allowance',amount:75000},{id:this.nid(),name:'استقطاع ضمان',type:'fixedDeduction',amount:Math.round(base[k]*0.05)}];D.profs.push({id:this.nid(),emp:U(k),base:base[k],cur:'IQD',from:this.dAdd(td,-400),comps:comp,stamp:'a'+k,created:ago(400)})});
 ['11','18'].forEach(k=>{D.profs.push({id:this.nid(),emp:U(k),base:base[k]+150000,cur:'IQD',from:this.dAdd(td,-60),comps:[{id:this.nid(),name:'بدل نقل',type:'allowance',amount:75000}],stamp:'b'+k,created:ago(60)})});
 const A=(k,per,type,amt,reason,src,st,o)=>{const a={id:this.nid(),emp:U(k),per,type,amt,cur:'IQD',reason,src,st,att:null,req:null,sug:null,rev:null,by:ACC,at:ago(20),...(o||{})};D.adjs.push(a);return a};
 const att=(n)=>({id:this.nid(),fileName:n,contentType:'application/pdf',sizeBytes:52000,creationTime:ago(20)});
 A('11',this.fnPrev(cur,-1),'addition',100000,'مكافأة إنجاز','manual','open',{att:att('bonus.pdf')});
 const dd=A('18',this.fnPrev(cur,-1),'deduction',50000,'خصم تأخر معتمد','manual','reversed',{att:att('late.pdf')});
 A('18',cur,'addition',50000,'عكس: '+dd.reason,'reversal','open',{rev:dd.id});
 A('20',this.fnPrev(cur,-2),'deduction',30000,'استقطاع سلفة','manual','includedInRun',{att:att('advance.pdf')});
 // attendance based suggestions for the previous period (generated once)
 const pp=this.fnPrev(cur,-1),from=pp+'-01',to=this.fnLast(pp);
 db.emps.filter(e=>e.status==='active'&&!e.roles.includes('executiveDirector')&&D.profs.some(p=>p.emp===e.id)).forEach(e=>{const days=this.daysOf(e,{fromDate:from,toDate:to}).filter(z=>this.EV_WORK.includes(z.status));const late=days.reduce((n,z)=>n+z.lateMinutes,0),abs=days.filter(z=>z.status==='absent').length,ot=days.reduce((n,z)=>n+z.overtimeMinutes,0)/60;const pf=this.fnProf(e.id,to);const dr=pf?pf.base/30:null;
  const S=(basis,type,qty,unit,amt)=>D.sugs.push({id:this.nid(),emp:e.id,per:pp,type,basis,qty,unit,amt:amt==null?null:Math.round(amt),cur:amt==null?null:'IQD',settle:null,st:'pendingReview',adj:null,by:null,at2:null,note:null,created:ago(3)});
  if(late>=30)S('lateness','deduction',late,'minutes',dr?dr*late/480:null);if(abs>=1)S('absence','deduction',abs,'days',dr?dr*abs:null);if(ot>=1)S('overtime','addition',Math.round(ot*10)/10,'hours',dr?dr/8*1.25*ot:null)});
 D.sugs.push({id:this.nid(),emp:U('19'),per:cur,type:'deduction',basis:'attendanceCorrectionSettlement',qty:1,unit:'days',amt:42000,cur:'IQD',settle:this.fnPrev(cur,-2),st:'pendingReview',adj:null,by:null,at2:null,note:null,created:ago(2)});
 const rj=D.sugs.find(s=>s.basis==='lateness');if(rj){rj.st='rejected';rj.by=ACC;rj.at2=ago(1);rj.note='تأخر مبرر'}
 D.exps.push({id:this.nid(),cat:'ضيافة',amt:185000,cur:'IQD',date:this.dAdd(td,-10),desc:'ضيافة اجتماع الإدارة',src:'general',st:'active',att:att('receipt-1.pdf'),move:null,req:null,rev:null,by:ACC,at:ago(10)});
 const e2={id:this.nid(),cat:'صيانة',amt:420000,cur:'IQD',date:this.dAdd(td,-25),desc:'صيانة المكيفات',src:'general',st:'reversed',att:att('receipt-2.pdf'),move:null,req:null,rev:null,by:ACC,at:ago(25)};D.exps.push(e2);
 D.exps.push({id:this.nid(),cat:'صيانة',amt:-420000,cur:'IQD',date:this.dAdd(td,-24),desc:'عكس: خطأ في الفاتورة',src:'general',st:'active',att:null,move:null,req:null,rev:e2.id,by:ACC,at:ago(24)});
 // accountant requests that reached the accountant step
 const rq=(k,desc,st)=>{const S={id:this.nid(),kind:'gen',type:'accountantRequest',emp:U(k),by:U(k),onb:false,desc,status:st,at:ago(st==='pendingDestination'?2:15),tl:[],reason:null};this.d5.reqs.push(S);return S};
 rq('11','طلب سلفة 300,000 د.ع','pendingDestination');rq('19','طلب شهادة راتب','pendingDestination');const c=rq('20','طلب تسوية كشف','completed');c.tl.push({at:ago(12),action:'تمت الموافقة',actor:ACC,note:null});
 this.ntf(ACC,'adjustmentSuggestionsReady','none');
};
fnRef=(id)=>{const e=this.db.emps.find(z=>z.id===id),ix=this.state.lang==='en',d=e?this.db.D.find(z=>z.id===e.dept):null;return e?{id:e.id,fullName:ix?e.en:e.ar,departmentName:d?(ix?d.nameEn:d.nameAr):null}:null};
route15=(op,x,P,v,ix,R)=>{
 if(!(op in OPR15)) return undefined;
 if(!this.d15) this.mk15();
 const D=this.d15,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles,td=this.todayS();
 if(!roles.some(r=>OPR15[op].includes(this.AB3[r]))) R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar;
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const NF='Hr:Finance:NotFound',need=(o,c)=>{if(!o)R(404,c||NF);return o};
 const PER=/^[0-9]{4}-(0[1-9]|1[0-2])$/,CUR=/^[A-Z]{3}$/;
 const att=(a)=>a?{id:a.id,fileName:a.fileName,contentType:a.contentType,sizeBytes:a.sizeBytes,creationTime:a.creationTime}:null;
 const chkFile=(f)=>{if(!f)R(400,'validation');if(!/\.(pdf|jpe?g|png)$/i.test(f.name||''))R(422,'Hr:Document:FileTypeNotAllowed');if((f.size||0)>10485760)R(422,'Hr:Document:FileTooLarge');if(/virus|infected/i.test(f.name))R(422,'Hr:Finance:AttachmentRejected');return{id:this.nid(),fileName:f.name,contentType:/pdf$/i.test(f.name)?'application/pdf':'image/jpeg',sizeBytes:f.size||60000,creationTime:nowIso()}};
 const locked=(p)=>D.locked.includes(p);
 const pDto=(p)=>({id:p.id,employee:this.fnRef(p.emp),baseSalary:p.base,currency:p.cur,effectiveFrom:p.from,components:p.comps.map(c=>({id:c.id,name:c.name,type:c.type,amount:c.amt?c.amt:c.amount})),concurrencyStamp:p.stamp,creationTime:p.created});
 const aDto=(a)=>({id:a.id,employee:this.fnRef(a.emp),period:a.per,type:a.type,amount:a.amt,currency:a.cur,reason:a.reason,source:a.src,status:a.st,attachment:att(a.att),sourceRequestId:a.req,suggestionId:a.sug,reversalOfAdjustmentId:a.rev,createdBy:uref(a.by),creationTime:a.at});
 const sDto=(s)=>({id:s.id,employee:this.fnRef(s.emp),period:s.per,type:s.type,basis:s.basis,quantity:s.qty,quantityUnit:s.unit,suggestedAmount:s.amt,currency:s.cur,settlementForPeriod:s.settle,status:s.st,adjustmentId:s.adj,reviewedBy:s.by?uref(s.by):null,reviewedAt:s.at2,reviewNote:s.note,creationTime:s.created});
 const storeExp=()=>{if(!this.d11)return[];const out=[];this.d11.expenses.forEach(e=>{const m=this.d11.moves.find(z=>z.id===e.move);const base={id:e.id,cat:'مشتريات المخزن',amt:e.amount,cur:'IQD',date:m?m.at.slice(0,10):td,desc:null,src:'storePurchase',st:e.reversed?'reversed':'active',att:null,move:e.move,req:null,rev:null,by:m?m.by:null,at:m?m.at:nowIso()};out.push(base);if(e.reversed)out.push({...base,id:this.pu('rev'+e.id),amt:-e.amount,st:'active',rev:e.id})});return out};
 const eDto=(e)=>({id:e.id,category:e.cat,amount:e.amt,currency:e.cur,date:e.date,description:e.desc||null,source:e.src,status:e.st,attachment:att(e.att),stockMovementId:e.move,sourceRequestId:e.req,reversalOfExpenseId:e.rev,createdBy:e.by?uref(e.by):null,creationTime:e.at});
 const allExp=()=>[...D.exps,...storeExp()];
 const self=(emp)=>{if(emp===meId)R(422,'Hr:Finance:SelfChangeNotAllowed')};
 const arq=(r)=>{const S=r.s,sp=this.rqSteps(r),dest=sp.find(z=>z.role==='accountant');const reached=S.status==='pendingDestination'||(dest&&dest.decision!=='notReached');if(!reached)return null;const st=S.status==='pendingDestination'?'pendingAccountant':(S.status==='completed'?'completed':'rejected');return{S,st,dest}};
 const arqSum=(r,m)=>({id:m.S.id,employee:this.fnRef(m.S.emp),requestTypeCode:'accountantRequest',requestTypeName:ix?'Accountant request':'طلب من المحاسب',status:m.st,arrivedAt:m.S.at,decidedAt:m.st==='pendingAccountant'?null:((m.S.tl||[]).slice(-1)[0]||{at:m.S.at}).at});
 const reqStep=(id)=>{if(!id)return;const rec=this.rqAll().find(z=>z.s.id===id);const m=rec&&arq(rec);if(!m||m.st!=='pendingAccountant')R(422,'Hr:Finance:RequestNotAtAccountantStep')};
 const me=E(meId);
 switch(op){
 case 'SalaryProfile_List':{let es=db.emps.filter(e=>e.status!=='ended'&&!e.roles.includes('executiveDirector'));if(x.departmentId)es=es.filter(e=>e.dept===x.departmentId);if(x.filter)es=es.filter(e=>(e.ar+e.en).toLowerCase().includes(String(x.filter).toLowerCase()));
  const rows=es.map(e=>({e,p:this.fnProf(e.id,td)}));let a=rows;if(x.hasProfile!==undefined&&x.hasProfile!=='')a=a.filter(r=>!!r.p===(x.hasProfile===true||x.hasProfile==='true'));
  return{totalCount:a.length,items:pg(a,x).items.map(r=>({employee:this.fnRef(r.e.id),hasProfile:!!r.p,baseSalary:r.p?r.p.base:null,currency:r.p?r.p.cur:null,effectiveFrom:r.p?r.p.from:null}))}}
 case 'SalaryProfile_Get':{const e=need(E(x.employeeId),'Hr:Finance:EmployeeNotFound');const p=this.fnProf(e.id,x.asOf||td);return pDto(need(p))}
 case 'SalaryProfile_Update':{const e=need(E(x.employeeId),'Hr:Finance:EmployeeNotFound');self(e.id);if(!(x.baseSalary>=0.01)||!CUR.test(x.currency||'')||!x.effectiveFrom||!Array.isArray(x.components)||x.components.some(c=>!c.name||!(c.amount>=0.01)||!['allowance','fixedDeduction'].includes(c.type)))R(400,'validation');
  const vs=D.profs.filter(p=>p.emp===e.id).sort((p,q)=>q.from.localeCompare(p.from)),latest=vs[0]||null;
  if(latest){if((x.concurrencyStamp||null)!==latest.stamp)R(409,'conflict')}else if(x.concurrencyStamp)R(409,'conflict');
  if(latest&&x.effectiveFrom<=latest.from)R(422,'Hr:Finance:EffectiveDateNotAfterLatest');if(locked(this.fnPeriod(x.effectiveFrom)))R(422,'Hr:Finance:EffectiveDateInLockedPeriod');
  const names=x.components.map(c=>c.name.trim().toLowerCase());if(new Set(names).size!==names.length)R(422,'Hr:Finance:DuplicateComponentName');
  const p={id:this.nid(),emp:e.id,base:x.baseSalary,cur:x.currency,from:x.effectiveFrom,comps:x.components.map(c=>({id:this.nid(),name:c.name,type:c.type,amt:c.amount})),stamp:stamp(),created:nowIso()};D.profs.push(p);D.stale[this.fnPeriod(x.effectiveFrom)]=true;return pDto(p)}
 case 'SalaryProfile_GetHistory':{const e=need(E(x.employeeId),'Hr:Finance:EmployeeNotFound');const a=D.profs.filter(p=>p.emp===e.id).sort((p,q)=>q.from.localeCompare(p.from));return{totalCount:a.length,items:pg(a,x).items.map(pDto)}}
 case 'Adjustment_List':{let a=D.adjs.slice();if(x.employeeId)a=a.filter(z=>z.emp===x.employeeId);if(x.departmentId)a=a.filter(z=>(E(z.emp)||{}).dept===x.departmentId);if(x.period)a=a.filter(z=>z.per===x.period);if(x.type)a=a.filter(z=>z.type===x.type);if(x.source)a=a.filter(z=>z.src===x.source);if(x.status)a=a.filter(z=>z.st===x.status);a.sort((p,q)=>q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(aDto)}}
 case 'Adjustment_Create':{const e=E(x.employeeId);if(!e)R(404,'Hr:Finance:EmployeeNotFound');self(e.id);if(!PER.test(x.period||'')||!['addition','deduction'].includes(x.type)||!(x.amount>=0.01)||!x.reason||x.reason.length>2000)R(400,'validation');
  const a0=chkFile(x.attachment);if(locked(x.period))R(422,'Hr:Finance:PeriodLocked');const pf=this.fnProf(e.id,this.fnLast(x.period));if(!pf)R(422,'Hr:Finance:SalaryProfileMissing');reqStep(x.sourceRequestId);
  const a={id:this.nid(),emp:e.id,per:x.period,type:x.type,amt:x.amount,cur:pf.cur,reason:x.reason,src:'manual',st:'open',att:a0,req:x.sourceRequestId||null,sug:null,rev:null,by:meId,at:nowIso()};D.adjs.push(a);D.stale[x.period]=true;return aDto(a)}
 case 'Adjustment_Reverse':{const o=need(D.adjs.find(z=>z.id===x.id));self(o.emp);if(o.st==='reversed')R(422,'Hr:Finance:AdjustmentAlreadyReversed');if(!x.reason||!PER.test(x.period||''))R(400,'validation');if(locked(x.period))R(422,'Hr:Finance:PeriodLocked');
  const a={id:this.nid(),emp:o.emp,per:x.period,type:o.type==='addition'?'deduction':'addition',amt:o.amt,cur:o.cur,reason:x.reason,src:'reversal',st:'open',att:null,req:null,sug:null,rev:o.id,by:meId,at:nowIso()};o.st='reversed';D.adjs.push(a);D.stale[x.period]=true;return aDto(a)}
 case 'Adjustment_GetAttachmentDownloadUrl':{const a=need(D.adjs.find(z=>z.id===x.id));if(!a.att)R(404,NF);return{url:'https://files.example.test/finance/'+a.att.id,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'AdjustmentSuggestion_List':{let a=D.sugs.slice();if(x.period)a=a.filter(z=>z.per===x.period);if(x.employeeId)a=a.filter(z=>z.emp===x.employeeId);if(x.departmentId)a=a.filter(z=>(E(z.emp)||{}).dept===x.departmentId);if(x.status)a=a.filter(z=>z.st===x.status);if(x.basis)a=a.filter(z=>z.basis===x.basis);a.sort((p,q)=>q.created.localeCompare(p.created));return{totalCount:a.length,items:pg(a,x).items.map(sDto)}}
 case 'AdjustmentSuggestion_Approve':{const s=need(D.sugs.find(z=>z.id===x.id));self(s.emp);if(s.st!=='pendingReview')R(422,'Hr:Finance:SuggestionAlreadyReviewed');const amt=x.amount!=null?x.amount:s.amt;if(amt==null)R(422,'Hr:Finance:SuggestionAmountRequired');if(x.amount!=null&&!(x.amount>=0.01))R(400,'validation');
  if(locked(s.per))R(422,'Hr:Finance:PeriodLocked');const pf=this.fnProf(s.emp,this.fnLast(s.per));if(!pf)R(422,'Hr:Finance:SalaryProfileMissing');
  const a={id:this.nid(),emp:s.emp,per:s.per,type:s.type,amt,cur:pf.cur,reason:x.reason||('اقتراح حضور: '+s.basis),src:'attendanceSuggestion',st:'open',att:null,req:null,sug:s.id,rev:null,by:meId,at:nowIso()};D.adjs.push(a);s.st='approved';s.adj=a.id;s.by=meId;s.at2=nowIso();s.note=x.reason||null;D.stale[s.per]=true;return aDto(a)}
 case 'AdjustmentSuggestion_Reject':{const s=need(D.sugs.find(z=>z.id===x.id));self(s.emp);if(s.st!=='pendingReview')R(422,'Hr:Finance:SuggestionAlreadyReviewed');if(!x.reason)R(400,'validation');s.st='rejected';s.by=meId;s.at2=nowIso();s.note=x.reason;return sDto(s)}
 case 'Expense_List':{let a=allExp();if(x.source)a=a.filter(e=>e.src===x.source);if(x.category)a=a.filter(e=>e.cat===x.category);if(x.dateFrom)a=a.filter(e=>e.date>=x.dateFrom);if(x.dateTo)a=a.filter(e=>e.date<=x.dateTo);if(x.status)a=a.filter(e=>e.st===x.status);a.sort((p,q)=>q.date.localeCompare(p.date)||q.at.localeCompare(p.at));return{totalCount:a.length,items:pg(a,x).items.map(eDto)}}
 case 'Expense_Create':{if(!x.category||x.category.length>100||!(x.amount>=0.01)||!CUR.test(x.currency||'')||!x.date||(x.description&&x.description.length>2000))R(400,'validation');const a0=chkFile(x.attachment);reqStep(x.sourceRequestId);
  const e={id:this.nid(),cat:x.category,amt:x.amount,cur:x.currency,date:x.date,desc:x.description||null,src:'general',st:'active',att:a0,move:null,req:x.sourceRequestId||null,rev:null,by:meId,at:nowIso()};D.exps.push(e);return eDto(e)}
 case 'Expense_Get':return eDto(need(allExp().find(z=>z.id===x.id)));
 case 'Expense_Reverse':{const o=need(allExp().find(z=>z.id===x.id));if(o.st==='reversed'||o.rev)R(422,'Hr:Finance:ExpenseAlreadyReversed');if(!x.reason)R(400,'validation');
  const e={id:this.nid(),cat:o.cat,amt:-o.amt,cur:o.cur,date:td,desc:x.reason,src:o.src,st:'active',att:null,move:o.move,req:null,rev:o.id,by:meId,at:nowIso()};const orig=D.exps.find(z=>z.id===o.id);if(orig){orig.st='reversed';D.exps.push(e)}else{const se=this.d11.expenses.find(z=>z.id===o.id);if(se)se.reversed=true}return eDto(e)}
 case 'Expense_GetAttachmentDownloadUrl':{const e=need(allExp().find(z=>z.id===x.id));if(!e.att)R(404,NF);return{url:'https://files.example.test/finance/'+e.att.id,expiresAt:new Date(Date.now()+6e5).toISOString()}}
 case 'Expense_Export':{if(!['pdf','excel'].includes(x.format))R(400,'validation');let a=allExp();if(x.source)a=a.filter(e=>e.src===x.source);if(x.category)a=a.filter(e=>e.cat===x.category);if(x.dateFrom)a=a.filter(e=>e.date>=x.dateFrom);if(x.dateTo)a=a.filter(e=>e.date<=x.dateTo);return{fileName:'expenses_'+td+(x.format==='pdf'?'.pdf':'.xlsx'),contentType:x.format==='pdf'?'application/pdf':'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',sizeBytes:2048+a.length*120}}
 case 'AccountantRequest_List':{let a=this.rqAll().filter(r=>this.rqType(r).code==='accountantRequest').map(r=>({r,m:arq(r)})).filter(z=>z.m);if(x.status)a=a.filter(z=>z.m.st===x.status);if(x.employeeId)a=a.filter(z=>z.m.S.emp===x.employeeId);a.sort((p,q)=>q.m.S.at.localeCompare(p.m.S.at));return{totalCount:a.length,items:pg(a,x).items.map(z=>arqSum(z.r,z.m))}}
 case 'AccountantRequest_Get':{const r=this.rqAll().find(z=>z.s.id===x.id&&this.rqType(z)&&this.rqType(z).code==='accountantRequest');const m=r&&arq(r);if(!m)R(404,NF);const base=arqSum(r,m);return{id:base.id,employee:base.employee,submittedBy:uref(m.S.by),requestTypeCode:base.requestTypeCode,requestTypeName:base.requestTypeName,description:m.S.desc||null,status:base.status,arrivedAt:base.arrivedAt,decidedAt:base.decidedAt,timeline:this.rqTl(r),linkedAdjustmentIds:D.adjs.filter(a=>a.req===m.S.id).map(a=>a.id),linkedExpenseIds:D.exps.filter(e=>e.req===m.S.id).map(e=>e.id)}}
 case 'SystemSettings_GetFinance':return{payrollApproverRole:D.fin.role,concurrencyStamp:D.fin.stamp,lastModificationTime:D.fin.mod};
 case 'SystemSettings_UpdateFinance':{if(!['hrManager','executiveDirector'].includes(x.payrollApproverRole))R(400,'validation');if(D.fin.stamp!==x.concurrencyStamp)R(409,'conflict');D.fin.role=x.payrollApproverRole;D.fin.stamp=stamp();D.fin.mod=nowIso();return{payrollApproverRole:D.fin.role,concurrencyStamp:D.fin.stamp,lastModificationTime:D.fin.mod}}
 }
 return undefined};
fnParse=(txt,T)=>{const out=[];String(txt||'').split('\n').map(z=>z.trim()).filter(Boolean).forEach(l=>{const p=l.split('|').map(z=>z.trim());if(p.length!==3)throw {code:'validation'};const ty=/^(allowance|بدل)$/i.test(p[1])?'allowance':(/^(fixedDeduction|استقطاع|استقطاع ثابت|deduction)$/i.test(p[1])?'fixedDeduction':null);const am=Number(p[2]);if(!p[0]||!ty||!(am>=0.01))throw {code:'validation'};out.push({name:p[0],type:ty,amount:am})});return out};
fnMoney=(n,c)=>new Intl.NumberFormat(this.state.lang==='ar'?'ar-IQ':'en-US',{maximumFractionDigits:2}).format(n)+(c?' '+c:'');
fiUpdDlg=(T,e,cur,hist)=>({title:T.dFiUpd,body:T.dFiUpdB+' '+T.nFiCur,fields:[{k:'b',l:T.fFiBase,req:1,max:14,pat:/^\d+(\.\d+)?$/,init:cur?String(cur.baseSalary):''},{k:'c',l:T.fFiCur,req:1,max:3,pat:/^[A-Z]{3}$/,init:cur?cur.currency:'IQD'},{k:'d',l:T.fFiFrom,t:'date',req:1,init:this.todayS()},{k:'m',l:T.fFiComps,t:'ta',max:4000,init:cur?cur.components.map(z=>z.name+' | '+z.type+' | '+z.amount).join('\n'):''}],okLabel:T.cnf,
 run:async(v)=>{let comps;try{comps=this.fnParse(v.m,T)}catch(er){throw er}await this.call('SalaryProfile_Update',{employeeId:e.id,baseSalary:Number(v.b),currency:v.c,effectiveFrom:v.d,components:comps,concurrencyStamp:hist&&hist.items[0]?hist.items[0].concurrencyStamp:null});return{m:T.tFiUpd}}});
fiAdjDlg=(T,emps,reqs)=>({title:T.dFiAdj,body:T.dFiAdjB,fields:[{k:'e',l:T.ficEmp,t:'sel',req:1,opts:emps},{k:'p',l:T.fFiPer,req:1,max:7,pat:/^[0-9]{4}-(0[1-9]|1[0-2])$/,init:this.todayS().slice(0,7)},{k:'t',l:T.fFiType,t:'sel',req:1,opts:Object.keys(T.fadt).map(x=>({v:x,l:T.fadt[x]}))},{k:'a',l:T.fFiAmt,req:1,max:14,pat:/^\d+(\.\d+)?$/},{k:'r',l:T.fFiReason,t:'ta',req:1,max:2000},{k:'f',l:T.fFiAtt,t:'file',h:'PDF/JPEG/PNG ≤ 10MB',up:(e)=>{const fl=e.target.files&&e.target.files[0];if(fl)this.setF('f',[{id:fl.name+':'+fl.size,fileName:fl.name,sizeBytes:fl.size}])}},{k:'q',l:T.fFiReq,t:'sel',opts:reqs}],okLabel:T.cnf,
 run:async(v)=>{const fl=(Array.isArray(v.f)?v.f:[])[0];await this.call('Adjustment_Create',{employeeId:v.e,period:v.p,type:v.t,amount:Number(v.a),reason:v.r,attachment:fl?{name:fl.fileName,size:fl.sizeBytes}:undefined,sourceRequestId:v.q||undefined});return{m:T.tFiAdjT}}});
fiRevDlg=(T,a)=>({title:T.dFiRev,body:T.dFiRevB,fields:[{k:'p',l:T.fFiPer,req:1,max:7,pat:/^[0-9]{4}-(0[1-9]|1[0-2])$/,init:this.todayS().slice(0,7)},{k:'r',l:T.fFiReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Adjustment_Reverse',{id:a.id,reason:v.r,period:v.p});return{m:T.tFiRevT}}});
fiSugDlg=(T,s,approve)=>({title:approve?T.dFiApp:T.dFiRej,body:approve?T.dFiAppB:'',fields:approve?[{k:'a',l:T.fFiOver,max:14,pat:/^\d+(\.\d+)?$/},{k:'r',l:T.fFiReason,t:'ta',max:2000}]:[{k:'r',l:T.fFiReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:!approve,
 run:async(v)=>{if(approve)await this.call('AdjustmentSuggestion_Approve',{id:s.id,amount:v.a?Number(v.a):undefined,reason:v.r||undefined});else await this.call('AdjustmentSuggestion_Reject',{id:s.id,reason:v.r});return{m:approve?T.tFiAppT:T.tFiRejT}}});
fiExpDlg=(T,reqs)=>({title:T.dFiExp,body:T.dFiExpB,fields:[{k:'c',l:T.fFiCat,req:1,max:100},{k:'a',l:T.fFiAmt,req:1,max:14,pat:/^\d+(\.\d+)?$/},{k:'u',l:T.fFiCur,req:1,max:3,pat:/^[A-Z]{3}$/,init:'IQD'},{k:'d',l:T.fFiDate,t:'date',req:1,init:this.todayS()},{k:'s',l:T.fFiDesc,t:'ta',max:2000},{k:'f',l:T.fFiAtt,t:'file',h:'PDF/JPEG/PNG ≤ 10MB',up:(e)=>{const fl=e.target.files&&e.target.files[0];if(fl)this.setF('f',[{id:fl.name+':'+fl.size,fileName:fl.name,sizeBytes:fl.size}])}},{k:'q',l:T.fFiReq,t:'sel',opts:reqs}],okLabel:T.cnf,
 run:async(v)=>{const fl=(Array.isArray(v.f)?v.f:[])[0];await this.call('Expense_Create',{category:v.c,amount:Number(v.a),currency:v.u,date:v.d,description:v.s||undefined,attachment:fl?{name:fl.fileName,size:fl.sizeBytes}:undefined,sourceRequestId:v.q||undefined});return{m:T.tFiExpT}}});
fiExpRevDlg=(T,e)=>({title:T.dFiRevE,body:T.dFiRevEB,fields:[{k:'r',l:T.fFiReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('Expense_Reverse',{id:e.id,reason:v.r});return{m:T.tFiRevT}}});
d16=null;
FIN_EXPORTS=['Report_Export','Expense_Export','PayrollRun_Export','FinanceReport_Export'];
pyCalc=(period,incl)=>{const D=this.d15,db=this.db,last=this.fnLast(period);const emps=db.emps.filter(e=>e.status!=='ended'&&!e.roles.includes('executiveDirector'));const items=[],without=[];
 emps.forEach(e=>{const pf=this.fnProf(e.id,last);if(!pf){without.push(this.fnRef(e.id));return}
  const L=[{kind:'baseSalary',label:this.state.lang==='en'?'Base salary':'الراتب الأساسي',amount:pf.base,adjustmentId:null}];let gross=pf.base,add=0,ded=0;
  pf.comps.forEach(c=>{const a=c.amt!=null?c.amt:c.amount;if(c.type==='allowance'){gross+=a;L.push({kind:'allowance',label:c.name,amount:a,adjustmentId:null})}else{ded+=a;L.push({kind:'fixedDeduction',label:c.name,amount:a,adjustmentId:null})}});
  D.adjs.filter(a=>a.emp===e.id&&a.per===period&&(a.st==='open'||(incl&&a.st==='includedInRun'))).forEach(a=>{if(a.type==='addition'){add+=a.amt;L.push({kind:'addition',label:a.reason,amount:a.amt,adjustmentId:a.id})}else{ded+=a.amt;L.push({kind:'deduction',label:a.reason,amount:a.amt,adjustmentId:a.id})}});
  items.push({id:this.nid(),emp:e.id,cur:pf.cur,gross,add,ded,net:gross+add-ded,lines:L})});
 return{items,without}};
pyTotals=(items)=>{const m={};items.forEach(i=>{const t=m[i.cur]||(m[i.cur]={currency:i.cur,gross:0,additions:0,deductions:0,net:0});t.gross+=i.gross;t.additions+=i.add;t.deductions+=i.ded;t.net+=i.net});return Object.values(m)};
mk16=()=>{
 if(!this.d15)this.mk15();const U=this.UID,db=this.db,td=this.todayS(),cur=this.fnPeriod(td),ACC=(db.emps.find(e=>e.roles.includes('accountant'))||{}).id,HR=(db.emps.find(e=>e.roles.includes('hrManager'))||{}).id;
 Object.assign(this.NT,{payrollRunPendingApproval:['كشف رواتب بانتظار الاعتماد','طُلب اعتماد كشف الرواتب الشهري.','Payroll run waiting for approval','Approval of the monthly payroll run was requested.'],payrollRunApproved:['اعتُمد كشف الرواتب','اعتُمد الكشف الشهري ويمكنك إغلاقه.','Payroll run approved','The monthly run was approved and can be closed.'],payslipAvailable:['قسيمة الراتب متاحة','أصبحت قسيمة راتبك متاحة للاطلاع.','Payslip available','Your payslip is now available.']});
 this.d16={runs:[]};const D=this.d16,ago=(d)=>new Date(Date.now()-d*864e5).toISOString();
 const mkRun=(per,status,days)=>{const c=this.pyCalc(per,true);const r={id:this.nid(),period:per,status,prep:ACC,reqAt:ago(days+2),appBy:HR,appAt:ago(days+1),clAt:status==='closed'?ago(days):null,calcAt:ago(days+3),items:c.items,without:c.without.map(z=>z.id),pend:0,stamp:'r'+per};D.runs.push(r);return r};
 mkRun(this.fnPrev(cur,-3),'closed',70);mkRun(this.fnPrev(cur,-2),'approved',20);
};
pyRef=(id)=>this.fnRef(id);
route16=(op,x,P,v,ix,R)=>{
 if(!(op in OPR16)) return undefined;
 if(!this.d16) this.mk16();
 const D=this.d16,F=this.d15,db=this.db,meId=this.UID(this.EID[v.p]),roles=P.roles,td=this.todayS(),L=T4[this.state.lang];
 const isA=roles.includes('accountant'),isH=roles.includes('hrManager'),isX=roles.includes('executiveDirector');
 const PAY=op.indexOf('Payslip_')===0;
 if(!PAY){const base=OPR16[op];if(!roles.some(r=>base.includes(this.AB3[r])))R(403,'forbidden')}
 const appr=F.fin.role;const roleOk=(isH&&appr==='hrManager')||(isX&&appr==='executiveDirector');
 if(['PayrollRun_List','PayrollRun_Get','PayrollRun_Export','PayrollRun_ReturnForChanges'].includes(op)&&!isA&&!roleOk)R(403,'forbidden');
 const E=id=>db.emps.find(e=>e.id===id),nmo=e=>ix?e.en:e.ar;
 const uref=id=>{const e=E(id);return e?{id:e.id,displayName:nmo(e)}:null};
 const pg=(a,q)=>({totalCount:a.length,items:a.slice(q.skipCount||0,(q.skipCount||0)+(q.maxResultCount||20))});
 const stamp=()=>Math.random().toString(36).slice(2,8),nowIso=()=>new Date().toISOString();
 const NF='Hr:Finance:NotFound',need=(o,c)=>{if(!o)R(404,c||NF);return o};
 const PER=/^[0-9]{4}-(0[1-9]|1[0-2])$/;
 const lineDto=(l)=>({kind:l.kind,label:l.label,amount:l.amount,adjustmentId:l.adjustmentId});
 const itemDto=(i)=>({id:i.id,employee:this.fnRef(i.emp),currency:i.cur,gross:i.gross,additions:i.add,deductions:i.ded,net:i.net,lines:i.lines.map(lineDto)});
 const totalsDto=(items)=>this.pyTotals(items);
 const stale=(r)=>r.status==='draft'&&!!F.stale[r.period];
 const sumDto=(r)=>({id:r.id,period:r.period,status:r.status,preparedBy:uref(r.prep),approvalRequestedAt:r.reqAt||null,approvedBy:r.appBy?uref(r.appBy):null,approvedAt:r.appAt||null,closedAt:r.clAt||null,lastCalculatedAt:r.calcAt,isStale:stale(r),employeeCount:r.items.length,totals:totalsDto(r.items),concurrencyStamp:r.stamp});
 const fullDto=(r)=>({...sumDto(r),items:r.items.map(itemDto),employeesWithoutProfile:r.without.map(id=>this.fnRef(id)),pendingSuggestionCount:F.sugs.filter(s=>s.st==='pendingReview'&&s.per===r.period).length});
 const run=(id)=>need(D.runs.find(z=>z.id===id));
 const chk=(r,x)=>{if(x.concurrencyStamp!==r.stamp)R(409,'conflict')};
 const apprUsers=()=>db.emps.filter(e=>e.status==='active'&&e.roles.includes(appr));
 switch(op){
 case 'PayrollRun_List':{let a=D.runs.slice();if(x.status)a=a.filter(r=>r.status===x.status);if(x.periodFrom)a=a.filter(r=>r.period>=x.periodFrom);if(x.periodTo)a=a.filter(r=>r.period<=x.periodTo);a.sort((p,q)=>q.period.localeCompare(p.period));return{totalCount:a.length,items:pg(a,x).items.map(sumDto)}}
 case 'PayrollRun_Create':{if(!PER.test(x.period||''))R(400,'validation');if(D.runs.some(r=>r.period===x.period))R(409,'conflict');const c=this.pyCalc(x.period,false);
  const r={id:this.nid(),period:x.period,status:'draft',prep:meId,reqAt:null,appBy:null,appAt:null,clAt:null,calcAt:nowIso(),items:c.items,without:c.without.map(z=>z.id),pend:0,stamp:stamp()};D.runs.push(r);delete F.stale[x.period];return fullDto(r)}
 case 'PayrollRun_Get':return fullDto(run(x.id));
 case 'PayrollRun_Recalculate':{const r=run(x.id);if(r.status!=='draft')R(422,'Hr:Finance:InvalidPayrollRunStatus');chk(r,x);const c=this.pyCalc(r.period,false);r.items=c.items;r.without=c.without.map(z=>z.id);r.calcAt=nowIso();r.reqAt=null;r.stamp=stamp();delete F.stale[r.period];return fullDto(r)}
 case 'PayrollRun_RequestApproval':{const r=run(x.id);if(r.status!=='draft')R(422,'Hr:Finance:InvalidPayrollRunStatus');chk(r,x);if(!appr)R(422,'Hr:Finance:PayrollApproverNotConfigured');if(stale(r))R(422,'Hr:Finance:PayrollRunStale');r.reqAt=nowIso();r.stamp=stamp();return fullDto(r)}
 case 'PayrollRun_Approve':{const r=run(x.id);if(!appr)R(422,'Hr:Finance:PayrollApproverNotConfigured');if(!roleOk)R(422,'Hr:Finance:NotConfiguredPayrollApprover');if(r.status!=='draft')R(422,'Hr:Finance:InvalidPayrollRunStatus');chk(r,x);if(r.prep===meId)R(422,'Hr:Finance:ApproverIsPreparer');if(!r.reqAt)R(422,'Hr:Finance:ApprovalNotRequested');if(stale(r))R(422,'Hr:Finance:PayrollRunStale');
  r.status='approved';r.appBy=meId;r.appAt=nowIso();r.stamp=stamp();F.adjs.forEach(a=>{if(a.per===r.period&&a.st==='open')a.st='includedInRun'});if(!F.locked.includes(r.period))F.locked.push(r.period);return fullDto(r)}
 case 'PayrollRun_ReturnForChanges':{const r=run(x.id);if(!r.reqAt)R(422,'Hr:Finance:ApprovalNotRequested');if(!x.reason||x.reason.length>2000)R(400,'validation');if(r.status!=='draft')R(422,'Hr:Finance:InvalidPayrollRunStatus');r.reqAt=null;r.returnReason=x.reason;r.stamp=stamp();return fullDto(r)}
 case 'PayrollRun_Close':{const r=run(x.id);if(r.status!=='approved')R(422,'Hr:Finance:InvalidPayrollRunStatus');chk(r,x);r.status='closed';r.clAt=nowIso();r.stamp=stamp();return fullDto(r)}
 case 'PayrollRun_Export':{const r=run(x.id);if(!['pdf','excel'].includes(x.format))R(400,'validation');return{fileName:'payroll_'+r.period+(x.format==='pdf'?'.pdf':'.xlsx'),contentType:x.format==='pdf'?'application/pdf':'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',sizeBytes:4096+r.items.length*180}}
 case 'Payslip_GetMyList':{const a=[];D.runs.filter(r=>r.status==='closed').forEach(r=>r.items.filter(i=>i.emp===meId).forEach(i=>a.push({r,i})));a.sort((p,q)=>q.r.period.localeCompare(p.r.period));return{totalCount:a.length,items:pg(a,x).items.map(z=>({id:z.i.id,period:z.r.period,currency:z.i.cur,gross:z.i.gross,additions:z.i.add,deductions:z.i.ded,net:z.i.net,closedAt:z.r.clAt}))}}
 case 'Payslip_GetMy':case 'Payslip_DownloadMyPdf':{let f=null;D.runs.filter(r=>r.status==='closed').forEach(r=>r.items.forEach(i=>{if(i.id===x.id&&i.emp===meId)f={r,i}}));if(!f)R(404,NF);
  if(op==='Payslip_DownloadMyPdf')return{fileName:'payslip_'+f.r.period+'.pdf',contentType:'application/pdf',sizeBytes:30000};
  return{id:f.i.id,employee:this.fnRef(meId),period:f.r.period,currency:f.i.cur,gross:f.i.gross,additions:f.i.add,deductions:f.i.ded,net:f.i.net,lines:f.i.lines.map(lineDto),closedAt:f.r.clAt}}
 case 'FinanceReport_GetMonthlyStatement':{if(!PER.test(x.period||''))R(400,'validation');const r=D.runs.find(z=>z.period===x.period);if(!r)R(404,'Hr:Finance:PayrollRunNotFoundForPeriod');let it=r.items;if(x.departmentId)it=it.filter(i=>(E(i.emp)||{}).dept===x.departmentId);return{period:r.period,runId:r.id,runStatus:r.status,items:it.map(itemDto),totals:totalsDto(it)}}
 case 'FinanceReport_GetAdjustments':{if(!PER.test(x.periodFrom||'')||!PER.test(x.periodTo||''))R(400,'validation');let a=F.adjs.filter(z=>z.per>=x.periodFrom&&z.per<=x.periodTo);if(x.employeeId)a=a.filter(z=>z.emp===x.employeeId);if(x.departmentId)a=a.filter(z=>(E(z.emp)||{}).dept===x.departmentId);if(x.type)a=a.filter(z=>z.type===x.type);a=a.filter(z=>z.st!=='reversed');
  const m={};a.forEach(z=>{const t=m[z.cur]||(m[z.cur]={currency:z.cur,additions:0,deductions:0});if(z.type==='addition')t.additions+=z.amt;else t.deductions+=z.amt});a.sort((p,q)=>q.at.localeCompare(p.at));
  const att=(z)=>z?{id:z.id,fileName:z.fileName,contentType:z.contentType,sizeBytes:z.sizeBytes,creationTime:z.creationTime}:null;
  return{totalCount:a.length,items:pg(a,x).items.map(z=>({id:z.id,employee:this.fnRef(z.emp),period:z.per,type:z.type,amount:z.amt,currency:z.cur,reason:z.reason,source:z.src,status:z.st,attachment:att(z.att),sourceRequestId:z.req,suggestionId:z.sug,reversalOfAdjustmentId:z.rev,createdBy:uref(z.by),creationTime:z.at})),totals:Object.values(m)}}
 case 'FinanceReport_GetStoreExpenses':{if(!x.dateFrom||!x.dateTo)R(400,'validation');const all=this.route15('Expense_List',{source:'storePurchase',dateFrom:x.dateFrom,dateTo:x.dateTo,maxResultCount:1000},P,v,ix,R);const m={};all.items.forEach(e=>{const t=m[e.currency]||(m[e.currency]={currency:e.currency,amount:0});t.amount+=e.amount});return{totalCount:all.totalCount,items:all.items.slice(x.skipCount||0,(x.skipCount||0)+(x.maxResultCount||20)),totals:Object.values(m)}}
 case 'FinanceReport_Export':{if(!['pdf','excel'].includes(x.format)||!['monthlyStatement','adjustments','storeExpenses'].includes(x.code))R(400,'validation');
  if((x.code==='monthlyStatement'&&!x.period)||(x.code==='adjustments'&&(!x.periodFrom||!x.periodTo))||(x.code==='storeExpenses'&&(!x.dateFrom||!x.dateTo)))R(422,'Hr:Finance:ReportFilterRequired');if(x.code==='monthlyStatement'&&!D.runs.some(r=>r.period===x.period))R(404,'Hr:Finance:PayrollRunNotFoundForPeriod');
  return{fileName:x.code+'_'+td+(x.format==='pdf'?'.pdf':'.xlsx'),contentType:x.format==='pdf'?'application/pdf':'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',sizeBytes:3072}}
 }
 return undefined};
pyNewDlg=(T)=>({title:T.dPyNew,body:T.dPyNewB,fields:[{k:'p',l:T.fPyPer,req:1,max:7,pat:/^[0-9]{4}-(0[1-9]|1[0-2])$/,init:this.fnPrev(this.todayS().slice(0,7),-1)}],okLabel:T.cnf,run:async(v)=>{const r=await this.call('PayrollRun_Create',{period:v.p});return{m:T.tPyNew,go:()=>this.toSub('rn',r.id)}}});
pyActDlg=(T,r,op,title,body,msg,danger)=>({title,body,fields:[{k:'n',l:T.fPyNote,t:'ta',max:2000}],okLabel:T.cnf,danger:!!danger,run:async(v)=>{await this.call(op,{id:r.id,concurrencyStamp:r.concurrencyStamp,note:v.n||undefined});return{m:msg}}});
pyRetDlg=(T,r)=>({title:T.dPyRet,body:T.dPyRetB,fields:[{k:'r',l:T.fPyReason,t:'ta',req:1,max:2000}],okLabel:T.cnf,danger:true,run:async(v)=>{await this.call('PayrollRun_ReturnForChanges',{id:r.id,reason:v.r});return{m:T.tPyRet}}});
pyLinesDlg=(T,i)=>({title:T.dPyLines+' · '+i.employee.fullName,fields:[],kv:[...i.lines.map(l=>({k:T.pyl[l.kind]+' · '+l.label,v:this.fnMoney(l.amount,i.currency)})),{k:T.pycNet,v:this.fnMoney(i.net,i.currency)}],okLabel:T.cnf,run:null});
renderVals(){
const s=this.state,L=s.lang,ar=L==='ar',i=ar?0:1,t={...T[L],...T2[L]},f=n=>Number(n).toLocaleString(ar?'ar-IQ':'en-US');
const pk0=PAGES.indexOf(s.page);const navI=t.nav.map((label,k)=>({label,k,key:PAGES[k]})).filter(n=>this.can(n.key));const pk=navI.findIndex(n=>n.key===s.page);
const nav=navI.map((n,j)=>({label:n.label,ic:IC[n.k],cls:j===pk?'on':'',d:(j*.05+.15)+'s',go:()=>this.go(n.key)}));
const kp0=[[412,'#00C49F',IC[4],'0,24 15,20 30,21 45,14 60,16 80,8 100,3'],[this.db?this.db.emps.filter(e=>e.status==='active').length:248,'#3B82F6',IC[1],'0,18 15,16 30,17 45,12 60,13 80,7 100,4'],[211,'#10B981',IC[2],'0,12 20,8 40,14 60,6 80,9 100,5'],[this.d4?this.d4.reqs.filter(r=>['pendingManager','pendingHr','pendingExecutive'].includes(r.status)).length:s.lv.filter(x=>x.s==='p').length,'#F59E0B',IC[3],'0,14 20,10 40,12 60,6 80,8 100,4'],[9,'#3B82F6',IC[6],'0,20 20,16 40,18 60,10 80,8 100,3']].map((k,n)=>({lb:t.kp[n],t:k[0],suf:n===0?t.suf:'',c:k[1],ic:k[2],pts:k[3],big:n===0?'big':'',d:(n*.08+.3)+'s',tr:(n===3?'':'▲ ')+['2.6%','4.2%','1.8%','','12%'][n]}));
const fin0=!!(s.me&&s.me.roleNames.some(r=>['hrManager','executiveDirector','accountant'].includes(r)));const kp=fin0?kp0:kp0.slice(1).map((k,n)=>({...k,big:n===0?'big':''}));
const rm=typeof matchMedia!=='undefined'&&matchMedia('(prefers-reduced-motion: reduce)').matches;
const unspot=(e)=>{const c=e.currentTarget.style;c.setProperty('--rx','0deg');c.setProperty('--ry','0deg')};
const mag=(e)=>{if(rm)return;const r=e.currentTarget.getBoundingClientRect(),c=e.currentTarget.style;c.setProperty('--tx',((e.clientX-r.left)/r.width-.5)*10+'px');c.setProperty('--ty',((e.clientY-r.top)/r.height-.5)*8+'px')};
const unmag=(e)=>{const c=e.currentTarget.style;c.setProperty('--tx','0px');c.setProperty('--ty','0px')};
const spot=(e)=>{const r0=e.currentTarget.getBoundingClientRect();if(!rm){e.currentTarget.style.setProperty('--ry',(((e.clientX-r0.left)/r0.width-.5)*7).toFixed(2)+'deg');e.currentTarget.style.setProperty('--rx',(-((e.clientY-r0.top)/r0.height-.5)*7).toFixed(2)+'deg')}const r=e.currentTarget.getBoundingClientRect();e.currentTarget.style.setProperty('--mx',(e.clientX-r.left)+'px');e.currentTarget.style.setProperty('--my',(e.clientY-r.top)+'px')};
const H={w:[62,70,66,78,74,40,35,82,88,80,92,85],m:[70,64,80,72,86,78,90,68,84,76,88,94]};
const bars=H[s.range].map((h,n)=>({h,v:f(h)+'%',d:(n*.03+.3)+'s'}));
const segs=[[38,'eng'],[26,'ops'],[20,'sales'],[10,'hr'],[6,'fin']];let off=0;const cols=AV;
const ring=segs.map((g,n)=>{const r={s:g[0]-1.5,o:-off,c:cols[n],lbl:t.d[g[1]],p:f(g[0])+'%'};off+=g[0];return r});
const pay=[[t.b,82,'#00C49F'],[t.bo,34,'#3B82F6'],[t.ov,18,'#10B981'],[t.de,24,'#EF4444']].map((p,n)=>({l:p[0],w:p[1]+'%',c:p[2],d:(.4+n*.1)+'s'}));
const acts=t.acts.map((a,n)=>({a,w:f([12,60,180,1440][n])+(ar?' د':'m'),c:AV[[2,1,0,3][n]]}));
const q=s.q.trim().toLowerCase();
let rows=s.emps.filter(e=>(s.df==='all'||e.d===s.df)&&(!q||e.n[i].toLowerCase().includes(q)||e.id.toLowerCase().includes(q)));
rows=rows.sort((a,b)=>(s.asc?1:-1)*a.n[i].localeCompare(b.n[i]));
const trows=rows.map((e,n)=>({id:e.id,nm:e.n[i],ini:e.n[i][0],dp:t.d[e.d],ti:e.ct||t.ti[e.t],st:t.st[e.s],sc:'bg '+({active:'ok',leave:'wr',inactive:'er'}[e.s]),col:AV[n%5],d:Math.min(n,9)*.05+'s',del:()=>{this.setState(x=>({emps:x.emps.filter(z=>z.id!==e.id)}));this.toast(t.deld)}}));
const depts=[{v:'all',l:t.all},...Object.keys(t.d).map(k=>({v:k,l:t.d[k]}))].map(o=>({...o,sel:s.df===o.v}));
const lvs=s.lv.map(r=>({nm:r.n[i],ty:t.lt[r.t],dy:f(r.dy)+' '+t.days,pend:r.s==='p',stl:r.s==='a'?t.ap2:r.s==='r'?t.rj2:t.pe,sc:'bg '+(r.s==='a'?'ok':r.s==='r'?'er':'wr'),
 ok:()=>{this.setState(x=>({lv:x.lv.map(z=>z.id===r.id?{...z,s:'a'}:z)}));this.toast(t.okA)},no:()=>{this.setState(x=>({lv:x.lv.map(z=>z.id===r.id?{...z,s:'r'}:z)}));this.toast(t.okR)}}));
const bal=[[14,21,'#00C49F'],[8,10,'#3B82F6'],[0,10,'#10B981']].map((b,n)=>({l:t.bal[n],v:f(b[0]),m:'/ '+f(b[1]),w:(b[0]/b[1]*100)+'%',c:b[2],d:(n*.08+.2)+'s'}));
const pend=lvs.filter(x=>x.pend);
const submit=()=>{if(s.saving)return;if(!s.fn.trim()){this.setState({err:true});return}
 this.setState({saving:1,err:false});setTimeout(()=>{this.setState(x=>({saving:2,emps:[{id:'E-'+(1050+x.emps.length),n:[x.fn,x.fn],d:x.fd,t:'spec',ct:x.ft||'—',s:'active'},...x.emps]}));this.toast(t.added)},700);
 setTimeout(()=>this.setState({modal:false,saving:0,fn:'',ft:''}),1300)};
const ncm=(e)=>{const r=e.currentTarget.getBoundingClientRect(),c=e.currentTarget.style;c.setProperty('--px',(((e.clientX-r.left)/r.width-.5)*2).toFixed(2));c.setProperty('--py',(((e.clientY-r.top)/r.height-.5)*2).toFixed(2))};
const ncl=(e)=>{e.currentTarget.style.setProperty('--px','0');e.currentTarget.style.setProperty('--py','0')};
const T3=T2[L],me=s.me,emp=s.emp,inb=s.inb,mfa=s.mfa,log=s.log;
const dfmt=(d)=>d?new Date(d+'T00:00:00').toLocaleDateString(ar?'ar-IQ':'en-GB',{year:'numeric',month:'short',day:'numeric'}):'—';
const tfmt=(d)=>d?new Date(d).toLocaleString(ar?'ar-IQ':'en-GB',{dateStyle:'medium',timeStyle:'short'}):'—';
const nv=(x)=>x==null||x===''?'—':x;
const first=me?me.displayName.split(' ')[0]:'';
const personas=Object.keys(PS).map(k=>({n:T3.pl[k][0],d:T3.pl[k][1],cls:s.persona===k?'on':'',sel:s.persona===k,pick:()=>this.setState({persona:k,aerr:''})}));
const roles=me?me.roleNames.map(r=>({l:T3.rl[r]||r})):[];
const ev=emp?{fullName:emp.fullName,dept:emp.department.name,title:emp.jobTitle,hire:dfmt(emp.hireDate),st:T3.es[emp.status],stc:emp.status==='active'?'ok':emp.status==='suspended'?'wr':'er',mgr:emp.manager?emp.manager.fullName:'—',shift:emp.currentShift?emp.currentShift.name+' · '+dfmt(emp.currentShift.effectiveFrom):'—',contract:emp.currentContract?dfmt(emp.currentContract.startDate)+' → '+(emp.currentContract.endDate?dfmt(emp.currentContract.endDate):'—'):'—',num:nv(emp.employeeNumber),gen:emp.gender?T3.gd[emp.gender]:'—',email:nv(emp.email),phone:nv(emp.phoneNumber),addr:nv(emp.address),dob:dfmt(emp.dateOfBirth)}:{fullName:'',dept:'',title:'',hire:'',st:'',stc:'',mgr:'',shift:'',contract:'',num:'',gen:'',email:'',phone:'',addr:'',dob:''};
const inbItems=inb?inb.items.map(x=>({l:T3.cat[x.category]||x.category,c:f(x.count),d:x.delegatedCount?f(x.delegatedCount)+' '+T3.inbD:''})):[];
const logRows=log?log.items.map(r=>({when:tfmt(r.occurredAt),ty:T3.tt[r.taskType]||r.taskType,ti:r.title})):[];
const tlTypes=[{v:'',l:T3.tlAll},...Object.keys(T3.tt).map(k=>({v:k,l:T3.tt[k]}))];
const aerr=s.aerr?(T3[s.aerr]||T3.er[s.aerr]||T3.er.gen):'';
const mfaOn=!!(mfa&&mfa.isEnabled);
const m1={authScr:s.scr!=='app',inApp:s.scr==='app'&&!!me,isSignin:s.scr==='signin',isPwd:s.scr==='pwd',isMfa:s.scr==='mfa',isRc:s.scr==='rc',isCodes:s.scr==='codes',personas,doSignin:this.doSignin,aerr,hasErr:!!aerr,busy:s.busy,idle:!s.busy,vol:s.vol,notVol:!s.vol,
pw1:s.pw1,pw2:s.pw2,pw3:s.pw3,onPw1:e=>this.setState({pw1:e.target.value,aerr:''}),onPw2:e=>this.setState({pw2:e.target.value,aerr:''}),onPw3:e=>this.setState({pw3:e.target.value,aerr:''}),submitPw:this.submitPw,cancelPw:()=>this.setState({scr:'app',aerr:'',pw1:'',pw2:'',pw3:'',vol:false}),
hasKey:!!s.mfaKey,noKey:!s.mfaKey,mkKey:s.mfaKey?s.mfaKey.sharedKey:'',startMfa:this.startMfa,code:s.code,onCode:e=>this.setState({code:e.target.value.replace(/\D/g,'').slice(0,6),aerr:''}),submitMfa:this.submitMfa,submitRc:this.submitRc,cancelRc:()=>this.setState({scr:'app',aerr:'',code:''}),
codes:(s.codes||[]).map(c=>({c})),saved:s.saved,onSaved:e=>this.setState({saved:e.target.checked}),savedCls:s.saved?'':'busy',contGate:()=>{if(s.saved)this.boot()},
isMe:s.page==='me',meName:me?me.displayName:'',meRole:roles.map(r=>r.l).join(' · '),meIni:me?Array.from(me.displayName)[0]:'',userName:me?me.userName:'',meEmail:nv(me&&me.email),roles,drs:me?f(me.directReportCount):'0',hasDrs:!!(me&&me.directReportCount>0),
lAr:s.lang==='ar'?'on':'',lEn:s.lang==='en'?'on':'',setAr:()=>this.setLang('ar'),setEn:()=>this.setLang('en'),ev,mfaRel:!!(me&&me.mfaRequired),mfaOn,mfaReqTxt:mfa?(mfa.isRequired?T3.yes:T3.no):'—',mfaStat:mfaOn?T3.mfaOn:T3.mfaOff,mfaStc:mfaOn?'ok':'wr',mfaSince:mfa&&mfa.enabledAt?tfmt(mfa.enabledAt):'—',mfaRem:mfa?f(mfa.recoveryCodesRemaining):'—',
openPw:()=>this.setState({scr:'pwd',vol:true,pw1:'',pw2:'',pw3:'',aerr:''}),openRc:()=>this.setState({scr:'rc',code:'',aerr:''}),signOut:this.signOut,goMe:()=>this.go('me'),
isMgr:!!(me&&me.roleNames.includes('departmentManager')),logRows,logCount:log?f(log.totalCount):'',noLog:!!log&&log.items.length===0,tlTypes,tlFrom:s.tlFrom,tlTo:s.tlTo,
onTlFrom:e=>{this.setState({tlFrom:e.target.value});this.loadLog(0,{tlFrom:e.target.value})},onTlTo:e=>{this.setState({tlTo:e.target.value});this.loadLog(0,{tlTo:e.target.value})},onTlType:e=>{this.setState({tlType:e.target.value});this.loadLog(0,{tlType:e.target.value})},
canPrev:s.tlSkip>0,canNext:!!log&&s.tlSkip+10<log.totalCount,prevLog:()=>this.loadLog(Math.max(0,s.tlSkip-10)),nextLog:()=>this.loadLog(s.tlSkip+10),logRange:log&&log.totalCount?f(s.tlSkip+1)+'–'+f(Math.min(s.tlSkip+10,log.totalCount))+' / '+f(log.totalCount):'',
inbItems,hasInb:!!inb&&inb.totalCount>0,noInb:!!inb&&inb.totalCount===0,inbTotal:inb?f(inb.totalCount):'',greeting:me?(ar?'صباح الخير يا '+first+'،':'Good morning, '+first+' —'):t.greet,indOp:pk<0?0:1};
const m2o={signOutUI:this.signOutUI,ntCount:(s.ntB&&s.ntB.count)?String(s.ntB.count>99?'99+':s.ntB.count):'',ntNone:!(s.ntB&&s.ntB.items&&s.ntB.items.length),ntItems:((s.ntB&&s.ntB.items)||[]).map(n=>({t:n.title,b:n.body,time:new Date(n.creationTime).toLocaleString(L==='ar'?'ar-IQ':'en-GB'),cls:n.isRead?'':'unr',c:n.isRead?'#64748B':'#00C49F',go:()=>this.ntOpen(n)})),ntAllGo:()=>{this.setState({bell:false});this.goTab('notif')},dlgKey:(e)=>{if(e&&e.key==='Escape'){this.dlgClose()}},lcode:L,resetMock:this.resetMock,...this.m2(f),canAdd:this.can('emp'),isDs:s.page==='ds',ds:this.dsVm(f),dlgClose:this.dlgClose,dlgGo:this.dlgGo};return{finOK:fin0,...m1,...m2o,ncm,ncl,th:s.th,thmCls:s.thm?'thm':'',themeIc:s.th==='dark'?'M12 8a4 4 0 1 0 0 8 4 4 0 0 0 0-8M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4':'M21 12.8A9 9 0 1 1 11.2 3a7 7 0 0 0 9.8 9.8z',toggleTheme:()=>{this.setState({th:s.th==='dark'?'light':'dark',thm:true});setTimeout(()=>this.setState({thm:false}),520)},unspot,mag,unmag,outCls:s.leaving?'out':'',segCls:s.range,dir:ar?'rtl':'ltr',sgn:ar?1:-1,tx:t,nav,indY:Math.max(pk,0)*48,colCls:(s.col?'cl':'')+(s.dr?' dr':''),kp,spot,bars,ring,pay,acts,trows,depts,lvs,bal,pend,hasPend:pend.length>0,
isDash:s.page==='dash',isEmp:s.page==='emp',isLeave:s.page==='leave',isSoon:!['dash','emp','leave','me','dept','acc','team','set','ds','att','req','cmp','rep','notif','aud','perf','ann','inv','cus','con','wrn','apr','fin','pay'].includes(s.page),soonTitle:t.nav[pk0],noRows:rows.length===0,
q:s.q,fn:s.fn,ft:s.ft,fd:s.fd,modal:s.modal,err:s.err,bell:s.bell,toasts:s.toasts,savingNone:s.saving===0,saving1:s.saving===1,saving2:s.saving===2,
rw:s.range==='w'?'on':'',rm:s.range==='m'?'on':'',setW:()=>this.setState({range:'w'}),setM:()=>this.setState({range:'m'}),
langLbl:ar?'EN':'ع',toggleLang:()=>this.setLang(ar?'en':'ar'),toggleCol:()=>this.setState({col:!s.col}),toggleDr:()=>this.setState({dr:!s.dr}),toggleBell:()=>{const nb=!s.bell;this.setState({bell:nb});if(nb)this.callRaw('Notification_List',{maxResultCount:6}).then(l=>this.setState(z=>({ntB:{count:z.ntB?z.ntB.count:0,items:l.items}}))).catch(()=>0)},
onQ:e=>{const v=e.target.value;this.setState({q:v});if(this.can('emp')){this.setState({page:'emp',sub:'list',sid:null,tab:'data',flt:{eq:v},skip:{},data:{}});this.later()}},onDf:e=>this.setState({df:e.target.value}),sortCls:s.asc?'▲':'▼',toggleSort:()=>this.setState({asc:!s.asc}),
openM:()=>this.startCreate(),closeM:()=>this.setState({modal:false,saving:0}),stop:e=>e.stopPropagation(),
onFn:e=>this.setState({fn:e.target.value,err:false}),onFt:e=>this.setState({ft:e.target.value}),onFd:e=>this.setState({fd:e.target.value}),submit,
goDash:()=>this.go('dash'),goLeave:()=>this.go('leave'),nts:t.nts.map((x,n)=>({x,c:AV[n]})),f,
cnt:f(s.lv.filter(x=>x.s==='p').length),totalEmp:f(s.emps.length)}}}
