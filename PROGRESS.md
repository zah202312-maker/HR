# PROGRESS — نظام الموارد البشرية

اقرأ هذا الملف مع `CLAUDE.md` ببداية كل جلسة، وحدّثه بنهايتها.

## القرارات المعتمدة (زينب، 2026-11-05)
| # | القرار |
|---|---|
| 1 | هيكلية layered باسم `HR_Mangment2`، مع موديولَي `Complaints` و`Finance` فارغين. الـ migrations موحدة بالـ Host DbContext، ولكل موديول schema واسم connection string منفصل. RLS وحساب القاعدة المنفصل بخطوة الشكاوى |
| 2 | OpenIddict داخل الـ Host |
| 3 | Multi-Tenancy مفعّل بشركة وحدة، والخادم يحددها من `App:TenantName`. العميل ما يقدر يغيّرها |
| 4 | إخفاء routes موديولات ABP، والإبقاء على مسارات العقد وصفحات الدخول |
| 5 | الاختبارات على PostgreSQL 17 المحلي (منفذ 5433) بقواعد `HR_Mangment2_Test_*`، وTestcontainers لاحقاً بالـ CI |
| 6 | `git init` بعد نقل الأسرار، حتى ما تدخل التاريخ. الشغل على فرع `backend/step-1-foundation` |
| 7 | ترتيب البناء المعدّل (تحت) |

زينب طلبت (2026-11-05): إكمال كل الخطوات بدون توقف، وأي شي يحتاج قرار ينكتب هنا ويستمر الشغل.

## ترتيب البناء المعتمد
| # | الخطوة | الحالة |
|---|---|---|
| 1 | الأساس | ✅ |
| 2 | Auth: OpenIddict، `application-configuration`، `me`، الأدوار، MFA، حاجز الوصول | ✅ (`UserAccount_*` عدا ResetMfa بالخطوة 5) |
| 3 | الإعدادات ونواة الإشعارات | ✅ |
| 4 | الأقسام والدوام والعطل | ✅ (`EmployeeShift_*` بالخطوة 5، والأثر على الحضور بالخطوة 6) |
| 5 | الموظفون والحسابات وربط المديرين والوثائق والتفويض | ✅ (`Me_GetInboxSummary` بالخطوة 7، وفحص الفيروسات الحقيقي ينتظر ClamAV) |
| 6 | الحضور والأجهزة والاستيراد | ✅ (الأثر من الإجازات وسجل التصحيح بالخطوة 8) |
| 7 | الطلبات ومسار الموافقة | ✅ (جهة المخزن تكتمل بالخطوة 13، وانتقال المطالعات والبلاغات عند تغيير المدير بالخطوة 9) |
| 8 | الإجازات والأرصدة وتصحيح البصمة | ✅ (التصحيح العكسي بعد الاعتماد ينتظر D16، وشهر مغلق ينتظر الخطوة 14) |
| 9 | المطالعات والبلاغات | ✅ (`item` يبقى null لحد الخطوة 13) |
| 10 | الشكاوى ومعالجوها | ✅ (خزنة المفاتيح السحابية Q77، وتشغيل الخدمة فعلياً بالحساب المقيّد ما انجرّب) |
| 11 | سجل التدقيق وبقية الإشعارات | ✅ (نوع `read` يكتمل مع المالية بالخطوة 14) |
| 12 | تقارير الإصدار 1 | ✅ (العهد والتقييم والاتساق وجزء الأقسام بالخطوة 13، وتصدير العقود والإنذارات بالخطوة 14) |
| 13 | الإصدار 2: المخزن، العهد، التقييم، التقرير الشهري، التعاميم | ✅ (مصروف تكلفة الشراء ينربط بالخطوة 14، وD28 إلى D33 تنتظر قرار) |
| 14 | الإصدار 3: العقود، الإنذارات، كتب الشكر، المحاسبة | ✅ (D34 إلى D38 وQ118 إلى Q124 تنتظر قرار) |
| 15 | تطبيق قرارات زينب (قسم «قرارات معتمدة للتطبيق»)، بعد الخطوة 14 | ✅ (قيمة `hr.invalid` باقية لحسابات الـ seed فقط، راجع تقرير الخطوة) |

## العمليات المنجزة
القائمة الملزمة بـ `test/HR_Mangment2.HttpApi.Tests/implemented-operations.txt`، وtest الـ routes يفحصها.

راجع `implemented-operations.txt` للقائمة الكاملة. الفرونت: الدفعة 1 بقسم «الفرونت» تحت، والباقي ⬜.

## الفرونت (Flutter، `frontend/`)
زينب طلبت (2026-11-06): «ابني فرونت» على أساس `frontend/HR_project` ومربوط بالباك. لأن الأسئلة ما انجاوبت كلها، اعتمدت التوصيات وتنتظر تأكيدك:
- **إدارة الحالة:** Riverpod + go_router + Dio (CLAUDE.md القسم 9 يطلب السؤال قبل التثبيت). ⏳ تأكيد
- **نطاق الدفعة 1:** الأساس + نواة الإصدار 1. ⏳ تأكيد ترتيب الدفعات الجاية
- **العميل مولّد** بـ `frontend/tool/generate_api.py` من `openapi.yaml`: 499 schema و309 عملية، واسم كل دالة هو الـ `operationId` الكامل، مع `operations.dart` (الصلاحية والأدوار والإصدار وأكواد الخطأ لكل عملية). ما استخدمت openapi-generator لأن بيئتي ممنوعة من maven وpub.dev، والمولّد الخاص يطبق `allOf`+`nullable` والـ multipart والتنزيلات حسب `x-server-requirements`.
- **ما انجرّب:** ما قدرت أشغّل Flutter ولا `flutter analyze` ولا الاختبارات (بيئتي ممنوعة من تنزيل Flutter، وماكو shell على جهازك). الأقواس كلها متوازنة والأسماء مطابقة للمولَّد، بس **لا تعتبريه يشتغل** قبل `flutter analyze` و`flutter test` (الأوامر بـ `frontend/README.md`).

### الدفعة 1: الشاشات والعمليات (فرونت ✅ مكتوب، ⏳ ينتظر البناء والتشغيل)
| الشاشة | العمليات |
|---|---|
| الدخول PKCE والجلسة | `OpenIdConfiguration_Get`, `ConnectToken_Issue` (رمز وتجديد), `ApplicationConfiguration_Get`, `Me_Get` |
| حاجز الوصول | `Account_ChangePassword`, `Mfa_GetStatus`, `Mfa_StartEnrollment`, `Mfa_ConfirmEnrollment`, `Mfa_RegenerateRecoveryCodes` |
| الرئيسية | `Me_GetInboxSummary`, `LeaveBalance_GetMy`, `Notification_GetUnreadCount` |
| ملفي | `Me_GetEmployeeProfile`, `Me_Update` |
| الطلبات | `Request_GetMy`, `Request_GetInbox`, `Request_Get`, `Request_Submit`, `Request_Approve`, `Request_Reject`, `Request_Cancel`, `RequestType_GetList`, `Item_Lookup` |
| الإجازات | `LeaveRequest_ListMy`, `LeaveRequest_Preview`, `LeaveRequest_Create`, `LeaveRequest_UploadAttachment`, `OtherLeaveType_ListAvailable` |
| الحضور | `Attendance_GetMy`, `AttendanceCorrection_Create` |
| الإشعارات | `Notification_List`, `Notification_MarkRead` |

الثيم من `HR-Design-System.md` (فاتح وداكن، RTL افتراضي، Cairo/Inter، الحالات بنص مو لون بس)، والتخطيط: شريط جانبي ≥1280، rail مطوي 768–1279، drawer تحت 768. رسائل أخطاء 200 كود مأخوذة من حزمة الـ UI/UX.

### الدفعة 2 (2026-11-06): الموظفون والأقسام والحسابات والأدوار والتفويض وفريقي — فرونت ✅ مكتوب ومفحوص
`flutter analyze` بدون مشاكل و`flutter test` 8 من 8 على جهاز زينب. التجربة الفعلية على الباك ⏳ بعد ما تكمل زينب التحقق بخطوتين بحساب `hrmanager`.

| الشاشة | العمليات |
|---|---|
| الموظفون وإضافة موظف | `Employee_List`, `Employee_Create`, `Employee_Lookup`, `Department_Lookup`, `WorkShift_List`, `Role_List` |
| ملف الموظف | `Employee_Get`, `Employee_Update`, `Employee_CorrectHireDate`, `Employee_ChangeManager`, `Employee_GetManagerHistory`, `EmployeeShift_Assign`, `EmployeeShift_GetHistory`, `Employee_GetAdministrativeTaskLog`, `EmployeeDocument_List`, `EmployeeDocument_Upload`, `EmployeeDocument_GetDownloadUrl`, `EmployeeDocument_Delete`, `UserAccount_Create` |
| الأقسام | `Department_List`, `Department_Create`, `Department_Update`, `Department_Delete` |
| الحسابات | `UserAccount_List`, `UserAccount_Get`, `UserAccount_SetRoles`, `UserAccount_ResetPassword`, `UserAccount_ResetMfa`, `UserAccount_GetPermissions`, `UserAccount_UpdatePermissions` |
| الأدوار | `Role_List`, `Role_Create`, `Role_Get`, `Role_Update`, `Role_Delete`, `Role_GetPermissions`, `Role_UpdatePermissions` |
| التفويض | `Delegation_GetMy`, `Delegation_List`, `Delegation_Create` (مع `fromUserId` للموارد البشرية), `Delegation_Cancel`, `Delegation_GetCandidates`, `UserAccount_Lookup` |
| فريقي | `Manager_GetMyTeam`, `Manager_GetTeamMember`, `Me_GetAdministrativeTaskLog`, `LeaveBalance_GetByEmployee` |

ما انبنى بعد من هالمجموعة: `Manager_AssignEmployees` (ربط عدة موظفين بمدير).

### الدفعة 3 (2026-11-06): الدوام والعطل والحضور والأجهزة والاستيراد والأرصدة — فرونت ✅ مكتوب ومفحوص
`flutter analyze` بدون مشاكل و`flutter test` 8 من 8. التجربة الفعلية على الباك ⏳.

| الشاشة | العمليات |
|---|---|
| قوالب الدوام | `WorkShift_List`, `WorkShift_Get`, `WorkShift_Create`, `WorkShift_Update` |
| العطل والعطلة الأسبوعية | `Holiday_List`, `Holiday_Create`, `Holiday_Update`, `Holiday_Delete`, `WeeklyOffSetting_Get`, `WeeklyOffSetting_Update` |
| حضور الموظفين والفريق وتفاصيل اليوم | `Attendance_GetTeam`, `Attendance_GetByEmployee`, `Attendance_GetDayDetail` |
| أجهزة البصمة والربط | `AttendanceDevice_List`, `AttendanceDevice_Create`, `AttendanceDevice_Update`, `AttendanceDevice_RotateKey`, `DeviceUserMap_List`, `DeviceUserMap_Create`, `DeviceUserMap_Delete`, `DeviceUserMap_ListUnmapped` |
| استيراد الحضور | `AttendanceImport_Create`, `AttendanceImport_List`, `AttendanceImport_Get` |
| إعدادات الحضور | `AttendanceSettings_Get`, `AttendanceSettings_Update` |
| أرصدة الإجازات (الموارد البشرية) | `LeaveBalance_GetByEmployee`, `LeaveBalance_SetEntitlement`, `LeaveBalance_Adjust`, `LeaveLedger_GetByEmployee`, `LeaveRequest_ListByEmployee`, `LeaveRequest_Reverse` |
| أنواع الإجازات الأخرى | `OtherLeaveType_List`, `OtherLeaveType_Create`, `OtherLeaveType_Update` |
| تقويم إجازات الفريق | `LeaveCalendar_GetTeam` |

- F9 (بسيط): ماكو `AttendanceDevice_Get` بالعقد، فشاشة الجهاز تاخذه من `AttendanceDevice_List`. يكفي هسه لأن الأجهزة قليلة.
- F10: بعد Shift+R طلعت صفحة Edge بيضة. ⏳ ينتظر صورة الكونسول من زينب.

### الدفعة 4 (2026-11-06): المطالعات والبلاغات والشكاوى ومعالجوها — فرونت ✅ مكتوب ومفحوص
`flutter analyze` بدون مشاكل و`flutter test` 8 من 8. التجربة الفعلية على الباك ⏳.

| الشاشة | العمليات |
|---|---|
| المطالعات | `Memo_Create`, `Memo_GetMy`, `Memo_GetInbox`, `Memo_Get`, `Memo_Forward`, `Memo_Convert`, `Memo_Close` |
| بلاغات الأعطال | `Incident_Create`, `Incident_GetMy`, `Incident_GetInbox`, `Incident_Get`, `Incident_Forward`, `Incident_UpdateStatus` |
| الشكاوى (مقدمها) | `Complaint_GetRespondents`, `Complaint_PreviewRouting`, `Complaint_Submit`, `Complaint_GetMyList`, `Complaint_GetMyDetail`, `Complaint_RaiseLevel`, `Complaint_UploadAttachment`, `Complaint_DownloadAttachment` |
| الشكاوى (المعالج) | `Complaint_GetAssignedList`, `Complaint_GetDetail`, `Complaint_AddAction`, `Complaint_Forward`, `Complaint_Close`, `Complaint_GetAccessLog` |
| معالجو الشكاوى | `ComplaintHandler_Request`, `ComplaintHandler_GetList`, `ComplaintHandler_Approve`, `ComplaintHandler_Reject`, `ComplaintHandler_Revoke` |

- صندوق المعالجة يظهر لمن عنده الصلاحية أو لصاحب تعيين معتمد (فحص بطلب واحد)، وإشعار `secureItem` يجرب `my/{id}` ثم `{id}` حسب العقد.
- F11 (بسيط بالعقد): وصف `MemoConvertInputDto.itemId` يذكر `target = storeItem`، و`MemoConvertTarget` بيه `leaveRequest|incident` بس. الفرونت يلتزم بالتعداد.

### الدفعة 5 (2026-11-06): التقارير والإعدادات وسجل التدقيق — فرونت ✅ مكتوب ومفحوص
`flutter analyze` بدون مشاكل و`flutter test` 8 من 8. التجربة الفعلية على الباك ⏳.

| الشاشة | العمليات |
|---|---|
| التقارير (مدير القسم) | `Report_GetTeamAttendance`, `Report_GetTeamLatenessAbsence`, `Report_GetTeamLeaves`, `Report_GetTeamPending` |
| التقارير (الموارد البشرية) | `Report_GetHrAttendance`, `Report_GetHrLeaveBalances`, `Report_GetHrRequests` |
| لوحة المراجع | `Report_GetReviewDashboard` (أعداد الشكاوى فقط بدون أسماء، وإنجاز الأقسام) |
| التصدير | `Report_Export` بصيغتي PDF وExcel بنفس مرشحات الشاشة |
| إعدادات النظام | `SystemSettings_ListSections` و Get/Update لـ reminders وcomplaints وhourlyLeave، ومعها contracts وfinance (إصدار 3) تظهر حسب الصلاحية. الأقسام الثانية تفتح شاشاتها |
| أنواع الطلبات | `RequestType_GetAdminList`, `RequestType_Create`, `RequestType_Update` (الوجهة مقفلة للنوع النظامي) |
| سجل التدقيق | `AuditLog_List` بمرشحات الفترة والإجراء والنتيجة والكيان |
| تعيين موظفين لمدير | `Manager_AssignEmployees` من تبويب المدير بملف الموظف |

- تقارير الإصدار 2 و3 (العهد والتقييمات والاتساق والعقود والإنذارات) تنضاف مع وحداتها.

### الدفعة 6 (2026-11-06): الإصداران 2 و3 — فرونت ✅ مكتوب، ⏳ `check.bat`
| الوحدة | الشاشات والعمليات |
|---|---|
| المخزن | الأصناف (`Item_GetList/Create/Update`)، طلبات الصرف (`StockIssue_GetList`, `Acknowledge`, `MarkPurchasing`, `Deliver`)، الإدخال والتصحيح والحركات (`Stock_Receive`, `Stock_Adjust`, `Stock_GetMovements`) |
| العهد | عهدتي وعهد الفريق وكل العهد (`Custody_GetMy/GetTeam/GetList/Create/Acknowledge/Return`)، الجرد (`CustodyCount_Create/GetList`) |
| التقييمات | الدورة الحالية، نموذج المدير (`Evaluation_GetManagerForm/SaveDraft/Submit`)، عرض الموظف (`GetMyList/GetMyDetail/Acknowledge/AddResponse`)، عرض الموارد البشرية والبت بالاعتراض (`GetHrList/GetHrView/DecideObjection`)، الملاحظات الداخلية (القائمة والمسودة والإضافة)، الملخص السنوي |
| معايير التقييم | `EvaluationCriterion_*`, `EvaluationGrade_*` |
| التقارير الشهرية للأقسام | `DepartmentReport_GetList/Get/UpdateNotes/Submit` |
| التعاميم | الواردة والمرسلة والإرسال وحالة الاستلام والتأكيد (`Circular_*`) |
| العقود | قاربت على الانتهاء، كل العقود، عقودي، التوصية والقرار وعقد التجديد (`Contract_*`) |
| الإنذارات | التوصية بمرفقات، توصياتي، كل الإنذارات، الاعتراضات، الإبلاغ والقرار والبت، رد الموظف واعتراضه (`Warning_*`) |
| كتب الشكر | الإصدار بملف، كتبي والتأكيد والتنزيل، كل الكتب (`AppreciationLetter_*`) |
| المالية | البيانات المالية ونسخها، الإضافات والاستقطاعات وعكسها، اقتراحات الحضور، كشوف الرواتب (إعداد، إعادة حساب، طلب اعتماد، اعتماد، إعادة، إغلاق، تصدير)، المصروفات وتصديرها، طلبات المحاسب، التقارير المالية وتصديرها، قسائم الراتب للموظف |
| تقارير الإصدار 2 و3 | `Report_GetTeamCustody`, `Report_GetTeamEvaluations`, `Report_GetEvaluationConsistency`, `Report_GetHrContracts`, `Report_GetHrWarnings` مع التصدير |

- ملف الموظف (وصفحة عضو الفريق) فيه تبويب «السجل الوظيفي»: العقود وسجل الإنذارات وكتب الشكر والعهد حسب الصلاحية.
- الإشعارات تفتح شاشات الوحدات الجديدة. إشعار التقييم يجرب عرض الموظف ثم نموذج المدير ثم عرض الموارد البشرية، لأن العقد يرد 404 لغير صاحب العرض.
- افتراض: العملة الافتراضية بنماذج المالية `IQD` وتنعدل باليد. ⏳ تأكيد العملة المعتمدة.

### ملاحظات تشغيل (2026-11-06)
- المشروع انتقل لحاسبة ثانية: PostgreSQL 16 على المنفذ 5432 (عُدّل `appsettings.secrets.json` للـ Host والـ DbMigrator)، والقاعدة انشأت من الـ DbMigrator. Flutter بـ `D:\development\flutter` ومو بالـ PATH، والسكربتات تضيفه.
- لازم `frontend/tool/build_backend.bat` بعد أي نقل، لأن البناء القديم يشير لـ `D:\projects\HR\HR_Mangment2` وحزمة `MagicOnion.Client` كانت ناقصة.
- التشغيل: `run_dev.bat` للباك (ينتظر الجاهزية) و`run_web.bat` للويب.
- F8 (جديد): الواجهة ما نبهت على شروط كلمة السر قبل الإرسال، فالأغلب إن `application-configuration` ما يرسل إعدادات `Abp.Identity.Password.*` مع إن العقد يطلبها. ⏳ يحتاج فحص بالباك.
- F12 ✅ (بموافقة المالكة 2026-11-06): صفحات الحساب بالباك صارت بتصميم الفرونت، مع فاتح/داكن وعربي/إنجليزي. ملفات جديدة بـ `src/HR_Mangment2.HttpApi.Host`: `Themes/Basic/Layouts/Account.cshtml` (يحل محل تخطيط الحساب بثيم Basic)، `Pages/Account/Login.cshtml` (نفس `LoginModel` ونفس `Action=Login`)، وأُعيد تصميم `Pages/Account/LoginTotp.cshtml` بدون تغيير منطقه، و`wwwroot/hr-account/` (CSS وJS والشعار). الفرونت يرسل `hr_theme` مع `ui_locales` بطلب الدخول، والصفحة تتبع لغة التطبيق مرة لكل تبويب. يحتاج `build_backend.bat`.
- F13 ✅: صفحة الدخول الجديدة ما بيها رابط «نسيت كلمة السر؟»، وبمكانه تنبيه بأن إعادة التعيين من الموارد البشرية.

### نواقص تحتاج قرار (الفرونت)
| # | الموضوع | الخيارات | التوصية |
|---|---|---|---|
| F1 | **`App:CorsOrigins` فارغ بـ `appsettings.json` للـ Host**، فـ Flutter Web من أي origin ثاني ينمنع | أ) إضافة `http://localhost:5000` لإعدادات التطوير ب) تقديم Flutter Web من نفس الـ Host | أ (تعديل إعدادات بـ `src`، يحتاج موافقتك) |
| F2 | **`redirectUris.web` بالعقد `https://localhost:44382/auth/callback.html` على origin الباك، وماكو صفحة بهالمسار.** مكتبة الدخول على الويب تحتاج صفحة callback على نفس origin التطبيق | أ) إضافة `http://localhost:5000/auth/callback.html` لـ `OpenIddict:Applications:HR_Mangment2_Flutter:RedirectUris` بالـ DbMigrator (العقد يسمح باستبدال رابط الويب لكل بيئة) ب) بناء Flutter Web داخل `wwwroot` للـ Host | أ. الصفحة جاهزة بـ `frontend/web/auth/callback.html` |
| F3 | الشعار الرسمي (SVG) والخط العربي (Cairo أو Tajawal) غير مسلّمين. هسه الشعار مقصوص من صور الهوية الرسمية بدون رسم | تسليم الملف الأصلي | — |
| F4 | الباك فيه SignalR hub `/signalr-hubs/notification`، والعقد ما يذكره | أ) إضافته للعقد ب) الإبقاء على polling كل دقيقة | ب هسه، وأ لاحقاً |
| F5 | تسجيل الخروج: العقد ما بيه عملية end-session. الفرونت يستخدم `end_session_endpoint` من `OpenIdConfigurationDto`، ويحتاج تفعيل logout endpoint بـ OpenIddict | تأكيد إنه مفعّل | — |
| F6 | إشعارات FCM (`DeviceToken_Register`) تحتاج مشروع Firebase | تأجيل | تأجيل |
| F7 | التقديم نيابة عن موظف (مدير القسم) وتنزيل مرفق الإجازة ما انبنوا بالدفعة 1 | الدفعة 2 | — |

### الدفعات الجاية (مقترح)
2: الموظفون والأقسام والحسابات والأدوار والتفويض. 3: الدوام والعطل والأجهزة والاستيراد وفريقي. 4: المطالعات والبلاغات والشكاوى. 5: التقارير والإعدادات والتدقيق. بعدها الإصداران 2 و3.

## إضافة 2026-11-07: مدير المراجع وتقرير المتابعة (باك ✅ مكتوب، فرونت ✅ مكتوب، ⏳ `tool/review_report_check.bat`)
**قرارات زينب (2026-11-07):**
| # | القرار |
|---|---|
| R1 | مدير المراجع المباشر هو المدير التنفيذي أو مدير الموارد البشرية (أي واحد منهم). مدير القسم ما يصلح |
| R2 | طلب المراجع (إجازة، تصحيح بصمة، طلب عام) يبدأ من `pendingHr` بخطوة وحدة، بدون خطوة مدير. مطالعته وبلاغه يبدآن من الموارد البشرية. من يحمل `departmentManager` مع `reviewer` يمشي بصف مدير القسم |
| R3 | تقرير متابعة المراجع: يومي أو أسبوعي أو شهري، صف لكل فريق (مدير القسم ومن يرتبط به) بنسبة إنجاز يكتبها المراجع وملاحظات وملاحظات مشاريع، وملخص عام |
| R4 | دورة التقرير: مسودة ← رفع للمدير التنفيذي ← اطلاع (`viewedAt` أول فتح). المرفوع ما يتعدل ولا يُحذف. التنفيذي ما يشوف المسودات |
| R5 | تحديث العقد ثم الكود بنفس الجلسة |

**العقد:** `x-global-rules.manager-link` صف `reviewer` (و`managerMayHoldRoleInstead: hrManager`)، صف المراجع بمصفوفة الموافقات (بالمكانين)، tag `ReviewReports` و6 عمليات `x-release: 2`: `ReviewReport_GetList` و`ReviewReport_Create` (201) و`ReviewReport_Get` و`ReviewReport_Update` و`ReviewReport_Delete` (204) و`ReviewReport_Submit`. 10 schemas `ReviewReport*`. `NotificationTemplateCode.reviewReportSubmitted` و`NotificationResourceType.reviewReport`. أكواد `Hr:ReviewReports:{AlreadySubmitted,IncompleteTeams,InvalidPeriod,NoTeams,UnknownTeam}`. العقد صار 315 عملية.

**الباك:**
- `ManagerLinkService.AllowedManagerRoles` (بدل `RequiredManagerRole`) يرجع الأدوار المقبولة، ويكفي أن يحمل المدير واحد منها. انعدلت `UserAccountAppService` و`EmployeeAppService`.
- `RequestOwnerRow.Reviewer` (آخر قيمة بالـ enum، تنحفظ نص بالمطالعات). `RequestRoutePlanner` و`MemoRoutePlanner` يبدآن من `pendingHr`. البلاغ يروح للموارد تلقائياً لأن الصف مو `Employee`.
- وحدة `ReviewReports`: `ReviewReport` (FullAudited، soft delete للمسودة) و`ReviewReportTeam`، جدولا `AppReviewReports` و`AppReviewReportTeams`. الفرق تنملا عند الإنشاء من حاملي `departmentManager` النشطين بملف نشط، عدا المراجع نفسه. الصلاحيتان `Hr.ReviewReports.Manage` (reviewer) و`Hr.ReviewReports.Read` (reviewer، executiveDirector) وتنزرع بالـ DbMigrator.
- migration `Hr_ReviewReports` يتولد بـ `tool/review_report_check.bat` (`dotnet ef`).
- اختبارات: `ReviewReportModuleTests` (3)، وحالات جديدة بـ `RequestRoutePlannerTests`، وتعديل `EmployeeModuleTests` لأن المراجع ما يصير تحت مدير قسم.

**الفرونت:** `features/eval/review_reports_page.dart` (القائمة وإنشاء تقرير والتفاصيل: نسبة لكل فريق وملاحظات ومشاريع، حفظ ورفع وحذف المسودة، وعرض للتنفيذي)، المسار `/review-reports`، عنصر بالقائمة «تقارير المتابعة»، فتح إشعار `reviewReport`، رسائل الأخطاء، وتلميح قواعد الربط بشاشة إضافة موظف. العميل انولّد من جديد (315 عملية). اختبار `test/review_report_test.dart`.

**أثر على البيانات الموجودة:** أي مراجع حالي مربوط بمدير قسم يبقى كما هو، بس أول تعديل لأدواره أو مديره يطلب مدير صالح (التنفيذي أو الموارد).

## الخطوة 15: شنو انطبق
`openapi.yaml` أولاً ثم الكود ثم الاختبارات. العقد صار 309 عملية بزيادة `Employee_CorrectHireDate` و`LeaveRequest_Reverse`.

- **تغيّر السلوك:** Q9 (البريد والهاتف عبر job بأولوية منخفضة تنضاف بنفس المعاملة)، Q17 (فهرس فريد و409)، Q18، Q38 (120 بالدقيقة لكل IP)، Q45، Q63، Q69، البريد الإلزامي، D1، D2، D6، D7، D8، D12، D15، D16، D22، D23، D28، D29، D30، والفرقان الصغيران بـ `application-configuration`.
- **D2:** سبع صلاحيات جديدة: `Hr.Identity.Role.ReadDetail`، `Hr.Request.Reject`، `Hr.Memo.Forward`، `Hr.Incident.Forward`، `Hr.Complaints.Forward`، `Hr.Warning.ReadAll`، `Hr.AppreciationLetter.ReadAll`. `Hr.Circulars.Send` ما احتاج فصل بعد D6. **أثر ظاهر:** مسؤول المخزن يأخذ 403 على `Request_Approve` بدل `UseStockIssueFlow`، ومدير الموارد يأخذ 403 على `Memo_Forward`، وهذا حسب أدوار العقد.
- **توثيق بالعقد فقط، لأن الكود كان يطبقها:** D3، D4، D5، D9، D10، D11، D13، D14، D17، D18، D19، D20، D21، D24، D25، D27، D31، D32، D33.

**خلل انكشف وانصلح:**
- تغيير البريد مع `concurrencyStamp` قديم كان يرجع 403 بدل 409، لأن الحفظ يمر عبر مدير المستخدمين. صار الفحص أول `Employee_Update`. ما كان ينكشف لأن الاختبارات كانت بلا بريد.
- **البطء:** مساعد `RunJobsAsync` بالاختبارات كان يقرا كل طابور المهام بكل دورة، وقاعدة الاختبار تحتفظ بالطابور بين التشغيلات. بعد إضافة jobs الإشعارات وصل تشغيل جزئي إلى ساعة. بعد الإصلاح وتنظيف jobs الإشعارات ببداية التشغيل، التشغيل الكامل صار 6.5 دقيقة. **سبب الـ 39 دقيقة القديمة مو مثبت**، بس ما رجع بالتشغيلين الأخيرين.

**انختبر:** 362 اختبار بمشروع الـ API (بيها `ApprovedDecisionsTests`) + 3 بالـ EF، وكلها تمر. الـ migration `Hr_ApprovedDecisions` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 301 معروض ومعلن، 0 غير معلن، 8 باقية (6 GPS مؤجلة بالعقد، و`ConnectToken_Issue` و`OpenIdConfiguration_Get` خارج Swagger)، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ناقص أو مو مكتمل:**
- **`@hr.invalid` ما انحذف كلياً:** الموظفون الجدد ما يستخدمونه، بس `HrTenantBootstrapper` يبقى يعطيه لحسابات الـ seed بلا بريد بالإعدادات، و`AccountProvisioner` و`GetRealEmail` يبقيان يتعرفان عليه للحسابات القديمة. حذفه الكامل يحتاج البيانات الحقيقية للحسابات الأولى (Q15).
- **D1 لا يعيد بناء أرصدة الإجازة:** يصحح التاريخ ويعيد حساب الحضور ويحفظ السبب، وسنوات الإجازة الجديدة تتبع التاريخ، بس سنة مفتوحة وحركاتها تبقى، والفرق يصلحه مدير الموارد بـ `LeaveBalance_Adjust`. القرار 6 يكول «يعيد الحساب»، فهذا أضيق منه، والعقد موثق بهالشكل.
- `Hr.Complaints.Forward` بالكتالوج والعقد، والقرار الفعلي لـ `ComplaintAccessPolicy` مثل بقية عمليات الشكاوى (D24)، فما يوجد فحص صلاحية منفصل عليها.
- **قاعدة موجودة:** الـ seed يضيف الصلاحيات الجديدة وما يسحب القديمة، فأي قاعدة قديمة يبقى بيها `Hr.Request.Decide` لمسؤول المخزن. قاعدة الإنتاج لسه ما موجودة، بس قاعدة التطوير بيها هذا الفرق.

**ما انختبر:**
- D30 (يحتاج مرور 5 أيام دوام)، وQ38 (حد 120)، وD12 (ملف استيراد برقم طويل).
- المسار الناجح لـ D7 وD15 وD29: اختبر الرفض للموظف العادي بـ D15، وما انجرب تنفيذ المدير التنفيذي على مدير الموارد فعلياً.
- D28 بدرجة صحيحة: انفحص رفض الدرجات مع `rejected` والمعيار الخطأ، وما انفحص تغيير الدرجة نفسها.
- Q45: ما انفحص إن التنفيذي يشوف التفاصيل فعلاً.
- إرسال البريد الحقيقي: الـ job انفحص مع `CapturingEmailSender`، وFirebase وSMTP ينتظران البيانات (Q8).

## الخطوة 14: شنو انبنى
66 عملية: `Contract_*` (8)، `Warning_*` (14)، `AppreciationLetter_*` (7)، `Report_GetHrContracts` و`Report_GetHrWarnings`، والمحاسبة (35: `SalaryProfile_*` و`Adjustment_*` و`AdjustmentSuggestion_*` و`PayrollRun_*` و`Payslip_*` و`Expense_*` و`AccountantRequest_*` و`FinanceReport_*`).

- العقود والإنذارات وكتب الشكر بالمشروع الرئيسي. `EmploymentRecordsWorker` كل ساعة ينقل العقد لمدة التنبيه وينتهي مهلة الرد، وكلاهما آمن عند التكرار. `currentContract` صار فعلي، وتصدير الرمزين صار فعلي بدل 404.
- المحاسبة بموديول `modules/HR_Mangment2.Finance` وschema `finance` بلا FK للخارج. الموديول صار يعتمد على طبقات المشروع الرئيسي مثل الشكاوى.
- `PayrollCalculator` يبني البنود من النسخة السارية بآخر يوم بالفترة والحركات `open`. بنود الكشف نسخة ثابتة، ومعرّف البند هو معرّف القسيمة.
- البدائل الفارغة انتهت: `PayrollClosedMonthReader` و`FinanceStockExpenseGateway`. فصار `affectsClosedMonth` و`settlementPeriod` ومصروف الشراء فعليين.
- `UploadedFileGuard` جديد يطبق حدود الوثائق والفحص قبل التخزين. `EmployeeDocumentAppService` ما لمسته، فبقى عنده نسخته.
- `EmploymentRecordInboxSummaryContributor` يضيف `warningResponse` و`warningDecision` و`contractDecision` و`appreciationLetterAcknowledgement`.
- **سجل الاطلاع:** كل قراءة مالية تكتب صف `read` بسجل التدقيق بلا قيم، وكل تصدير صف `export`.
- **خلل انكشف وانصلح قبل التشغيل:** جداول ABP للتدقيق كانت راح تحفظ الرواتب والمبالغ كنص صريح (معاملات الطلب والقيم القديمة والجديدة). `HrAuditingStore.RemoveFinancialValues` يمسحها ويبقي أسماء الحقول. **نفس الخطر باقي بعمود `TotalCost` على حركة المخزن، لأن كيانها مو بنطاق المالية، وما لمسته.**
- **خلل بتصميمي انكشف بالاختبار:** فحص الفترة المقفلة كان يمنع أي تاريخ قبل آخر فترة مقفلة، فكشف واحد بالمستقبل يوقف كل تعديلات الرواتب. صار حسب نص العقد: فترة تاريخ السريان نفسها.

**انختبر:** 351 اختبار بمشروع الـ API (بيها `EmploymentRecordsModuleTests` 9 و`FinanceModuleTests` 11) + 3 بالـ EF، وكلها تمر. الـ migrations `Hr_EmploymentRecords` و`Hr_Finance` انطبقا على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 299 معروض ومعلن، 0 غير معلن، 8 باقية (6 عمليات GPS مؤجلة بالعقد، و`ConnectToken_Issue` و`OpenIdConfiguration_Get` خارج Swagger)، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ما انختبر:**
- **توليد الاقتراحات فعلياً:** الاختبار يدخل اقتراحات مباشرة بالقاعدة. `AdjustmentSuggestionGenerator` وعامله وتسويات تصحيح الحضور ما اشتغلوا ولا مرة.
- `Contract_Decide` بمهلة متأخرة و`contractDecisionDue`: `ContractAlertService` انفحص لدخول مدة التنبيه فقط.
- رفض الاعتراض بعد 5 أيام دوام، وعكس مشتريات المخزن (`OnPurchaseReversedAsync`)، و`PayrollApproverNotConfigured` و`ApproverIsPreparer`، واعتماد المدير التنفيذي بعد تغيير الإعداد.
- عمل `Finance` عند ربطه بحساب قاعدة منفصل: الاتصال الحالي نفس الافتراضي.
- محتوى ملفات التصدير: انفحص كود 200 ونوع الملف فقط.

## الخطوة 13: شنو انبنى
56 عملية: `Item_*` (5)، `Stock_*` (3)، `StockIssue_*` (5)، `Custody_*` (7)، `CustodyCount_*` (2)، `EvaluationCycle_*` (2)، `Evaluation_*` (11)، `EvaluationInternalNote_*` (3)، `EvaluationAnnualSummary_*` (2)، `EvaluationCriterion_*` (3)، `EvaluationGrade_*` (3)، `DepartmentReport_*` (4)، `Circular_*` (6).

**المخزن والعهد:**
- `StockLedger` الوحيد اللي يكتب الرصيد، والحركة والرصيد يتغيران سوية. `StoreRequestStockGateway` و`StockIssueStatusReader` بدل البدائل الفارغة، فصار طلب المخزن يمشي من التقديم للتسليم، و`item` بالبلاغ يطلع.
- `RequestWorkflowService` صار بيه `RecordStoreProgressAsync` و`CompleteAtStoreAsync`، والخط الزمني يسجل `stockIssueAcknowledged` و`stockIssuePurchasing` و`stockIssueDelivered`.
- التكلفة تنحفظ بعمود على الحركة وما تطلع بأي DTO للمخزن. `IStockExpenseGateway` بدليل فارغ لحد الخطوة 14.
- `HasSharedCustody` صار فعلي، و`Report_GetTeamCustody` وتصديره صارا يقرآن العهد.

**التقييم والدورة الشهرية:**
- `EvaluationCycleService` مع `EvaluationCycleWorker` كل 15 دقيقة: مجدولة، ثم تفتح بآخر يوم دوام، ثم تثبت وتولّد التقارير بأول يوم دوام، مع تذكير لحد المهلة وتصعيد مرة وحدة. آمنة عند التكرار.
- المعايير والدرجات تنسخ للدورة عند فتحها، فالتعديل يسري من الدورة التالية.
- النموذج ما يخزن مديره قبل الرفع: يتبع مدير الموظف الحالي، فتغيير المدير ينقله لوحده بدرجاته، ومسودة الملاحظة الداخلية تبقى عند كاتبها.
- الملاحظة الداخلية لها جدولان ومسارات منفصلة، وما بيها مكان بأي من DTOs التقييم الثلاثة.
- `MonthlyCompletionReader` تعريف واحد للإنجاز والتأخر يخدم الدورة والتقرير والمراجع والتذكير.
- لوحة المراجع صار بيها جزء الأقسام، و`Report_GetTeamEvaluations` و`Report_GetEvaluationConsistency` وتصديرهما فعلية.
- `AttendanceSummaryReader` جديد بالـ Domain، وانتقل له حساب أرقام الحضور من `ReportQueries` حتى ما يتكرر.
- `MonthlyInboxSummaryContributor` يضيف فئات التقييم والتقرير والاعتراض وتأكيد التقييم والتعميم والعهدة.

**خلل انكشف وانصلح:** سطرا `stockIssueDelivered` و`completed` كانا بنفس الوقت، فترتيبهما بالخط الزمني مو ثابت. `completed` صار بعده بمللي ثانية. **نفس الوضع باقي بطلب المحاسب بـ `ApproveAsync`** (`approved` و`completed`)، وما لمسته.

**انختبر:** 331 اختبار بمشروع الـ API (بيها `StoreModuleTests` 8 و`EvaluationModuleTests` 11) + 3 بالـ EF، وكلها تمر. الـ migrations `Hr_StoreAndCustody` و`Hr_EvaluationsReportsCirculars` انطبقا على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 233 معروض ومعلن، 0 غير معلن، 74 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف. غيّرت اختبار `ReportModuleTests` اللي كان يفترض إن التقارير فارغة، لأن هذا كان سلوك مؤقت.

**اختباران هشّان انكشفا وانصلحا:** `Delegation_lifecycle_rules_and_task_log` و`Forbidden_targets_are_refused_and_never_listed` كانا يدوران بأول 200 صف، وقاعدة الاختبار صارت أكبر من هذا حسب ترتيب التشغيل، ففشلا بتشغيل كامل. صارا يمشيان على كل الصفحات. **نفس النمط باقي بغيرها** (`EmployeeModuleTests` الأسطر 137 و144 و149، و`ComplaintModuleTests` السطر 411، و`ReportModuleTests` السطر 303)، فممكن تفشل مستقبلاً بدون خلل بالكود.

**بطء (راجع الخطوة 15):** `dotnet test HR_Mangment2.slnx` أخذ 39 دقيقة مرة و16 مرة، بينما مشروع الـ API لوحده أخذ 7. النتيجة النهائية عبر الـ solution: 331 + 3 كلها تمر. السبب غير معروف، فخلي مهلة أي أمر تشغيل أطول من 40 دقيقة.

**ما انختبر:**
- تشغيل `EvaluationCycleWorker` الفعلي: الاختبارات تنادي `ProcessAsync` مباشرة بوقت تختاره.
- `isLate` عبر الـ API: دورات الاختبار بسنة 2040، فالساعة الحقيقية قبل موعد التأخر. انفحص `MonthlyCompletionReader` مباشرة بوقت محدد، بس فلتر `isLate` بـ `Evaluation_GetHrList` و`DepartmentReport_GetList` ما اشتغل بقيمة true.
- **أرقام لقطة التقرير الشهري:** انفحص صف الموظف وأعداد التقييم، بس أرقام العهد والتعاميم والطلبات المعلقة والحضور بقيم غير صفر ما انفحصت. التعاميم بالذات تنعد حسب الشهر، ودورة 2040 ما يقع بيها تعميم مرسل اليوم.
- رفض الاعتراض بعد 5 أيام دوام: يحتاج مرور وقت.
- توقف رصيدين بنفس اللحظة: الحماية عبر `concurrencyStamp` الصنف، وما انجرّب تزامن فعلي.
- إرسال التعميم من المدير التنفيذي، وتقرير مدير موظفوه بعدة أقسام (`department` فارغ)، وفاصل تذكير أكبر من يوم، وصرف عهدة مشتركة لقسم بلا مدير.
- تصدير `teamCustody` انفحص عبر الكود فقط، والتصديران الثانيان انفحص كود 200 بدون قراءة الملف.

## الخطوتان 11 و12: شنو انبنى
**سجل التدقيق (`AuditLog_List`):**
- `HrAuditEntry` جدول للإضافة فقط بأسماء الحقول بلا قيم. `HrAuditEntryBuilder` يحول كل طلب كتابة إلى صف، والدخول يجي من `HrSecurityLogStore`. كيانات الشكاوى ما تدخله.
- التنبيه الناقص من الخطوة 5 انبنى: الدخول الثاني بكلمة سر مؤقتة يرسل `accountSecurityChanged` إضافي عند تغييرها.
- **بقية الإشعارات:** كل قوالب الإصدار 1 مربوطة بحدث. 26 قالب بلا مرسل، وكلها تخص وحدات الإصدارين 2 و3 (مخزن، عهد، تقييم، تعاميم، عقود، إنذارات، كتب شكر، مالية).

**التقارير (12 عملية `Report_*`):**
- `ReportQueries` مصدر واحد للصفوف، يخدم الـ JSON والتصدير، فالملف ما يقدر يحمل حقل أو شخص مو بالتقرير. `ReportFileWriter` يحول الجدول إلى Excel (ClosedXML) أو PDF (QuestPDF من الوثيقة).
- `IReviewComplaintCounter` ينفذه موديول الشكاوى، ويرجع أعداد فقط بلا فك تشفير، ويستثني أي شكوى المراجع طرف بيها.
- كل تصدير ناجح يكتب صف `export` بسجل التدقيق، لأن ABP ما يدقق طلبات GET.

**خلل انكشف وانصلح:** ABP ما يفحص الـ ModelState للعمليات اللي ترجع `IActionResult`، فرمز تقرير غير معروف كان ينقرأ كأول تقرير. `Report_Export` صار ينادي `ValidateModel()`. **نفس الثغرة ممكن تكون بغيرها من العمليات اللي ترجع `IActionResult` وعندها enum بالـ query أو المسار، وما فحصتها كلها.**

**انختبر:** 312 اختبار بمشروع الـ API (بيها `AuditLogTests` و`ReportModuleTests`) + 3 بالـ EF. الـ migration `Hr_AuditEntries` انطبق على قاعدة التطوير مرتين بنجاح، والتقارير ما تحتاج migration. فحصت صفحة PDF عربية كصورة: الحروف متصلة والجدول من اليمين. تقرير Swagger: 177 معروض ومعلن من 307.

**اختبار غير مستقر:** `RequestModuleTests.Details_of_an_accountant_request_are_absent...` فشل مرة بتشغيل كامل واحد، ونجح بالتشغيل الكامل التالي وبثلاث مرات لوحده. رسالة الفشل ما انحفظت، فالسبب **غير معروف**. ما يلمس كود التقارير بشكل مباشر، بس ما أقدر أجزم إنه مو خلل حقيقي.

**ناقص، ولازم ينربط لاحقاً:**
- الخطوة 13: `Report_GetTeamCustody` و`Report_GetTeamEvaluations` و`Report_GetEvaluationConsistency` ترجع قوائم فارغة حسب العقد، وجزء الأقسام بلوحة المراجع فارغ.
- الخطوة 14: `Report_GetHrContracts` و`Report_GetHrWarnings`، وتصدير رمزيهما يرجع 404 لحد ذاك (Q97). ونوع `read` بسجل التدقيق يبدأ مع المالية.
- خط عربي لازم ينصّب على خادم Linux (Q96).

**ما انختبر:**
- `HrLeaveBalances` بعد ترحيل سنة فعلي، و`usedHours` بقيمة غير صفر، وصفوف الإجازة الزمنية بـ `TeamLeaves`.
- أداء تصدير كبير: الأرصدة تقرأ لكل موظف عدة استعلامات، وهذا مقبول لـ 200 موظف بس ما انقاس.
- `lateNormalCount` بقيمة غير صفر: انفحص إنه ما يتحرك، والسريتان انفحصتا بزيادة فعلية.

**حادثة بهاي الجلسة:** نسخة أولى من مساعد اختبار بـ `ReportModuleTests` قرأت connection string من الإعدادات بدل قاعدة الاختبار، وشغّلت `UPDATE` على `complaints."Complaints"` بمعرّف شكوى جديد. رجع 0 صفوف، فما تغيّر شي، والأرجح إنه وصل قاعدة التطوير. انصلح إلى `_host.ConnectionString`.

## الخطوة 10: شنو انبنى
- 19 عملية: `Complaint_*` (14)، `ComplaintHandler_*` (5). الكود بموديول `modules/HR_Mangment2.Complaints`، والموديول صار يعتمد على طبقات المشروع الرئيسي، وما يعتمد عليه أحد من الـ Domain.
- `ComplaintRoutingService` جدول 5.8 قواعد صافية بالكود، و`ComplaintAccessPolicy` يحدد المعالج ويفحص كل عملية من القاعدة. أي رفض يرجع 404 أو قائمة فارغة، و403 يبقى لبوابات الحساب وصلاحيات التقديم وإدارة المعالجين.
- التشفير: AES-256-GCM بمفتاح لكل شكوى (الموضوع والمحتوى والقرار والملاحظات واسم المرفق وملفه)، ومغلف بمفتاح الشركة، وكل قيمة مربوطة بمكانها فما تنتقل لشكوى أو حقل ثاني.
- القاعدة: schema `complaints` بلا FK للجداول الثانية، والـ migration ينشئ الحساب `hr_complaints_app` (NOLOGIN) بلا DELETE، وبلا UPDATE على السجلات الإضافية، مع RLS على `TenantId`. الاتصال يضبط `hr.tenant_id` عند الفتح.
- التدقيق: `[DisableAuditing]` على الخدمة والـ controller والكيانات. **وهذا ما كفى:** طلب الشكوى كان يكتب صف بسجل التدقيق بسبب الإشعار، فـ `ComplaintAuditingStore` يمنع أي طلب شكوى من الوصول لهذا السجل، وأي تعديل يخص `complaint-handler` يبقى مسجلاً.
- `ComplaintManagerChangeEffects` يحول المعلقة عند المدير للموارد البشرية ولا يعدّها، و`ComplaintHandlerMfaRequirementContributor` يفرض التحقق بخطوتين، و`ComplaintHandlerExpiryWorker` يسحب الشاشات عند انتهاء التعيين، و`ComplaintOverdueReminderWorker` يذكّر بالتأخر.
- `WorkCalendarService.AddWorkingDaysAsync` جديد لحساب `dueAt`. مساهمة بصندوق `Me_GetInboxSummary`: ماكو، وهذا مقصود.
- انختبر: 29 اختبار بـ `ComplaintModuleTests`، بيها جدول التوجيه، و404 لكل الأطراف بكل العمليات، وغياب المحتوى عن سجل التدقيق وعن الجداول والـ BLOB كنص صريح، وحدود الحساب المقيّد عبر `SET ROLE`. الـ migration `Hr_Complaints` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 164 معروض ومعلن، 0 غير معلن، 143 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ناقص من هذا الجزء، ولازم ينربط قبل الإنتاج:**
- خزنة المفاتيح السحابية (Q77). الموجود مؤقت.
- إعطاء `hr_complaints_app` كلمة سر وربطه بـ connection string اسمه `Complaints` (Q78). بدونه الخدمة تشتغل بحساب المالك، وRLS ما يطبّق عليه.

**ما انختبر:**
- تشغيل الخدمة كاملة بالحساب المقيّد: انفحص الحساب بـ `SET ROLE` بس. عمل الاتصالين بمعاملتين منفصلتين، و`set_config` على اتصال حقيقي، ما انجرّب.
- `NoEligibleHandler`: يحتاج شركة بلا مدير موارد أو تنفيذي فعال، والاختبارات تشارك قاعدة وحدة.
- `AttachmentTooLarge`، و`overdueReminderIntervalWorkingDays` بقيمة فعلية (انفحص تذكير واحد بدونه)، والتأخر لمستوى سري جداً.

**ملاحظة للفرونت:** `application-configuration` يضع كوكي `XSRF-TOKEN`، وABP يرفض بعدها أي POST بدون ترويسة المطابقة بـ 400 إذا الكوكي رجع مع الطلب. Flutter على الموبايل ما يتأثر، وFlutter على الويب لازم ما يرسل الكوكيز أو يرسل الترويسة.

## الخطوة 9: شنو انبنى
- 13 عملية: `Memo_*` (7)، `Incident_*` (6).
- `Memo` يحفظ صف الكاتب عند الكتابة، ومنه تتحدد المرحلة الأخيرة. جدول `MemoHandlers` يسجل كل من وصلته المطالعة، ومنه الاطلاع والصندوق مع `status`.
- التحويل إلى إجازة يمر بنفس `LeavePlanner`. منطق التقديم انتقل من `LeaveRequestAppService` إلى `LeaveSubmissionService` حتى يخدم الطريقين، و`RequestSubmission` صار بيه `ManagerStepApprovedByUserId` لتسجيل `approvedBySubmission`.
- التفويض بنطاق `memosAndIncidents` أو `allApprovals` عبر `RequestDeciderReader.GetForMemosAndIncidentsAsync`.
- `IManagerChangeEffects` صار متعدد: `ManagerLinkService` يشغّل كل المنفذين ويجمع العدد، فالشكاوى تضيف منفذها بالخطوة 10 إذا احتاجت.
- مساهمان بصندوق `Me_GetInboxSummary`: `memo` و`incident`.
- **خلل قديم انكشف وانصلح:** `Request_Approve` و`Request_Cancel` جسمهما اختياري بالعقد، وكانا يرجعان 400 بدون جسم من الخطوة 7. فحصت كل العقد: باقي `StockIssue_MarkPurchasing` و`StockIssue_Deliver` بنفس الحالة، وانتبه لهم بالخطوة 13. ABP يرفض `null` بالخدمة، فالـ controller يمرر `new NoteInputDto()`.
- انختبر: 16 اختبار تكامل جديد بـ `MemoIncidentModuleTests`. المجموع 237 ناجح (234 + 3). الـ migration `Hr_MemosAndIncidents` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 145 معروض ومعلن، 0 غير معلن، 162 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- `item` يرجع null دائماً، لأن `IRequestStockGateway.IsItemActiveAsync` يرجع false، فأي `itemId` يرفض. الخطوة 13 تملأه.
- نص `Hr:Memo:LeaveDetailsInvalid` عام، فما يوضح السبب الفعلي (Q69).

**ما انختبر:**
- `Hr:Request:NoApproverAvailable` أثناء التحويل (D17): الكود يمرره، وماكو اختبار له.
- إشعار `memoUpdate` للمفوضين: انفحص وصوله لصاحب الخطوة والكاتب فقط.

## الخطوة 8: شنو انبنى
- 20 عملية: `LeaveBalance_*` (4)، `LeaveLedger_*` (2)، `LeaveRequest_*` (7)، `OtherLeaveType_*` (4)، `LeaveCalendar_GetTeam`، `AttendanceCorrection_*` (2).
- الدفتر هو مصدر الرصيد: `LeaveBalanceManager` الوحيد اللي يكتبه، والرصيد مجموع حركاته. عند الاعتماد تنكتب حركتا `release` و`deduct` حتى يبقى المجموع صحيح.
- `LeavePlanner` يخدم المعاينة والتقديم بنفس الحساب، و`LeaveAllocator` و`LeaveYearMath` قواعد صافية مختبرة بدون قاعدة بيانات.
- الترحيل: `LeaveMaintenanceWorker` كل ساعة، وآمن عند التكرار. يحذف كذلك المرفقات غير المستخدمة بعد 24 ساعة.
- الآثار المعلقة من قبل اكتملت: `onLeave` و`hourlyLeaveMinutes` و`leaveDays` بالحضور، و`corrections` بتفاصيل اليوم، و`HireDateLocked` صار فعلي. انحذف `NullAttendanceLeaveReader` و`NullLeaveLedgerReader`.
- `CanViewAsync` انتقل من `RequestAppService` إلى `RequestWorkflowService` حتى تستخدمه الوحدتان.
- انختبر: 41 اختبار جديد (`LeaveRulesTests` و`LeaveModuleTests` و`LeaveAttendanceEffectTests`)، بيها مثال الوثيقة 24 − 18 → 6 + 21 = 27، والإجازة الممتدة بين سنتين، والرفض بعد إغلاق السنة، والوردية الليلية. الـ migration `Hr_LeavesAndCorrections` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 132 معروض ومعلن، 0 غير معلن، 175 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- `IAttendanceClosedMonthReader` بدليل فارغ لحد الخطوة 14: `affectsClosedMonth` دائماً false و`settlementPeriod` دائماً null.
- حركة `reverse` ما تنكتب أبداً، لأن العقد ما بيه مسار لتصحيح إجازة بعد اعتمادها (D16). هسه الموارد تستخدم `adjust`، والحضور يبقى `onLeave`.
- `AdministrativeTaskType` ما بيه نوع لتعديل الرصيد، فهاي التعديلات ما تدخل سجل المهام. الأثر موجود بالدفتر.

**ما انختبر:**
- `HourlySettingsNotConfigured` و`isConfigured = false`: الإعداد الأولي 8 ساعات. الكود مكتوب والمسار ما اشتغل بالاختبارات.
- حذف المرفقات غير المستخدمة بعد 24 ساعة: يحتاج مرور وقت حقيقي، فما انفحص إلا بالبناء.
- `Two_submissions_at_once` يتأكد إن الأيام ما تنصرف مرتين، بس ما يضمن إن التعارض صار فعلاً بكل تشغيل، لأنه يعتمد على التوقيت.
- استثناء الزمنية من التأخر مختبر من الخطوة 6 بقيم مباشرة. هنا انفحص بس إن `hourlyLeaveMinutes` يطلع على اليوم.

## الخطوة 7: شنو انبنى
- 14 عملية: `RequestType_*` (5)، `Request_*` (8)، `Me_GetInboxSummary`.
- `RequestRoutePlanner` مصفوفة الموافقات قواعد صافية بدون قاعدة بيانات، و`RequestWorkflowService` يبني المسار ويحركه وينهيه، ووحدتا الإجازة وتصحيح البصمة تقدمان عبره بالخطوة 8.
- التفويض صار له أثر: `RequestDeciderReader` يقرأ الأدوار والتفويضات الساريّة من القاعدة بكل طلب. القرار يتسجل باسم المفوض مع صاحب الصلاحية.
- تغيير المدير ينقل الطلبات المعلقة عند `pendingManager` للمدير الجديد، ويظهر `approverReassigned`، و`movedPendingItemCount` صار فعلياً.
- التذكير: `RequestReminderWorker` كل ساعة، و`WorkCalendarService` يحسب أيام الدوام من العطلة الأسبوعية والعطل الرسمية، ويخدم مهل الوحدات الجاية.
- الأنواع الخمسة المبذورة بـ `HrTenantBootstrapper`، و`storeItem` غير مفعل حسب العقد.
- انختبر: 44 اختبار جديد (`RequestRoutePlannerTests` كل خلايا المصفوفة، و`RequestModuleTests` 20 تكامل). المجموع 180 ناجح. الـ migration `Hr_Requests` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 112 معروض ومعلن، 0 غير معلن، 195 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- `IRequestLifecycleHandler` ماله منفذ لحد الخطوة 8: الموافقة والرفض والإلغاء ما يأثرون بعد على رصيد ولا بصمة.
- `IRequestStockGateway` و`IStockIssueStatusReader` بدليل فارغ لحد الخطوة 13: كل صنف يرجع `ItemInactive`، فما ينقدم طلب مخزن. منطق جهة المخزن انكتب، بس **ما انجرّب تكاملياً** لأن ما يمكن الوصول له بدون وحدة المخزن. المصفوفة نفسها مختبرة.
- `RequestManagerChangeEffects` ينقل الطلبات فقط. المطالعات والبلاغات تنضاف إله بالخطوة 9.
- `IInboxSummaryContributor` بيه مساهم الطلبات فقط. كل وحدة لاحقة تضيف فئتها، والشكاوى ما تنضاف أبداً.
- طلب معلق عند مدير مسمى توقف حسابه يبقى معلق لحد ما الموارد تغيّر المدير. وثيقة المتطلبات ما تحدد غير هذا، ولا يوجد تصعيد تلقائي.
- `AttendanceCorrection` و`LeaveRequest` يحتاجان بناء نص `detailsSummary` باللغتين عند التقديم.

## الخطوة 6: شنو انبنى
- 18 عملية: `Attendance_*` (4)، `AttendanceDevice_*` (5 مع استقبال البصمة)، `DeviceUserMap_*` (4)، `AttendanceImport_*` (3)، `AttendanceSettings_*` (2).
- حساب الأيام: `AttendanceDayEvaluator` قواعد صافية بدون قاعدة بيانات، و`AttendanceDayCalculator` يحمّل ويخزن. الحساب بالخلفية عبر ABP Background Jobs، وينضاف للطابور بنفس معاملة التغيير. `AttendanceDailyWorker` كل 30 دقيقة ينتج الغياب و`incomplete` لآخر 7 أيام.
- `ICompanyClock` صار async ويقرأ التوقيت من `AttendanceSettings`، و`App:TimeZone` ما عاد مستخدم.
- انختبر: 36 اختبار جديد (`AttendanceDayEvaluatorTests` و`AttendanceModuleTests`)، بيها تشغيل الـ job worker الحقيقي من البصمة لليوم. المجموع 136 ناجح. الـ migration `Hr_AttendanceAndUtcTimestamps` انطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 98 معروض ومعلن، 0 غير معلن، 209 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.

**خلل قديم انكشف وانصلح (Q42):** مفتاح `Npgsql.EnableLegacyTimestampBehavior` مع أعمدة `timestamp without time zone` كان يخزن UTC بتوقيت الجهاز ويقراه كأنه UTC، فكل قراءة تزيد فرق التوقيت (+3). هذا يأثر على كل حقول `date-time` من الخطوة 1، بيها `creationTime`، وأي مقارنة بالذاكرة مثل انتهاء الروابط. انحذف المفتاح وصارت كل الأعمدة `timestamptz` (64 عمود)، مع test يمنع رجوعه.

**حادثة بهاي الجلسة:** سكربت استبدال مني خرّب 4 ملفات غير متتبَّعة بـ git: `ManagerLinkService` و`HrTenantBootstrapper` و`EmployeeShiftAppService` و`UserAccountAppService`. انرجعت سطر بسطر من نصوص الجلسات السابقة، وسطر واحد باليد. تحققت بالبناء والاختبارات، وبمقارنة النصوص الثابتة مع DLL محفوظ قبل الضرر. التعليقات ما تنفحص بهالمقارنة. **`src/` كله بعد مو committed، وهذا الخطر باقي لحد أول commit.**

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- `IAttendanceLeaveReader` بدليل فارغ: `onLeave` و`leaveDays` و`hourlyLeaveMinutes` ما تطلع لحد الخطوة 8.
- `corrections` يرجع `[]` دائماً، و`CorrectedCheckIn`/`CorrectedCheckOut` ما يتعبون لحد الخطوة 8. الحساب يحترمهم من هسه.
- تغيير `hireDate` ما يعيد الحساب، وصفوف قبل التاريخ الجديد تبقى. تغيير المنطقة الزمنية يأثر على آخر 7 أيام فقط (Q40).
- ماكو تاريخ إنهاء خدمة بملف الموظف (Q41).

## الخطوة 5: شنو انبنى
- 32 عملية: `Employee_*` (8)، `Manager_*` (3)، `Me_GetEmployeeProfile` و`Me_GetAdministrativeTaskLog`، `EmployeeShift_*` (2)، `EmployeeDocument_*` (4)، `UserAccount_*` الباقية (8)، `Delegation_*` (5).
- `ManagerLinkService` يطبق `x-global-rules.manager-link`، و`AccountProvisioner` يطبق قواعد الأدوار المحمية والتركيبات والأدوار المفردة.
- الحسابات الأولى صار لها ملفات موظفين، وهاي قمة سلسلة المديرين (تنفيذي، مسؤول نظام، مدير قسم، موظف). تشغيل الـ seed مرتين ما يكرر شي.
- `IDepartmentUsageReader` و`IWorkShiftUsageReader` و`IMyAccountEmployeeReader` صارت قراءة فعلية.
- إعادة تعيين كلمة السر عبر رابط بالبريد: `HrAccountAppService` يلغي التغيير الإجباري والقفل والجلسات عند استخدام الرابط.
- انختبر: 19 اختبار تكامل جديد بـ `EmployeeModuleTests`. المجموع 100 ناجح. الـ migration `Hr_EmployeesDocumentsDelegations`، وانطبق على قاعدة التطوير مرتين بنجاح. تقرير Swagger: 80 معروض ومعلن، 0 غير معلن، 227 باقية، وماكو فرق بالحقول عدا `ApplicationConfigurationDto` المعروف.
- أصلحت test قديم غير مستقر: `UniqueDate` بـ `OrganizationSetupTests` كان أحياناً يتجاوز سنة 9999.

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- انتقال المعلقات عند تغيير المدير: الطلبات انربطت بالخطوة 7. المطالعات والبلاغات بالخطوة 9، والشكاوى بالخطوة 10.
- `ILeaveLedgerReader` بدليل فارغ: `HireDateLocked` ما ينطلق أبداً لحد الخطوة 8.
- `IAttendanceRecalculator.RecalculateEmployeeFromAsync` فارغ لحد الخطوة 6.
- `HasSharedCustody` يرجع false لحد الخطوة 13. منع إنهاء الخدمة بسبب العهدة ومسار تصحيح تاريخ المباشرة (القرار 6) ينتظران D1.
- `currentContract` يرجع null دائماً لحد الخطوة 14.
- `AdministrativeTaskLogger` يسجل `delegationCreated` فقط هسه. كل وحدة لاحقة تضيف نوعها.
- **«يُسجل أول دخول، ويُشعر الموظف بأي نشاط سبق تغييرها» (`UserAccount_Create`) ما انبنى.** الأثر: ماكو تنبيه إذا أحد استخدم كلمة السر المؤقتة قبل صاحبها. مرشح للخطوة 11.
- تغيير `hireDate` ما يحرك أول ربط دوام أو مدير، فممكن يبقى الربط يبدأ قبل التاريخ الجديد.
- روابط التنزيل المنتهية تبقى بالجدول، وتحتاج Worker تنظيف. الرابط يبقى يشتغل لحد انتهائه حتى لو الحساب توقف.

## الخطوة 4: شنو انبنى
- الأقسام (6 عمليات) بحذف ناعم، وقوالب الدوام (4)، والعطل (4) بحذف فعلي وتاريخ غير مكرر، والعطلة الأسبوعية (2).
- مخزن `Idempotency-Key` عام لكل مستخدم وعملية لمدة 24 ساعة: `IdempotencyStore`. يخزن المفتاح والبصمة ومعرف المورد فقط.
- `HrConflictException` يرجع 409 بلا كود، وتعارض الفهرس الفريد بين طلبين متزامنين يرجع 409 كذلك.
- `HrSorting`: الترتيب من قائمة حقول مسموحة بكل قائمة، وغيرها يرجع 400.
- انختبر: 22 اختبار تكامل جديد بـ `OrganizationSetupTests`. المجموع 81 ناجح. الـ migration `Hr_DepartmentsShiftsHolidays`.

**ناقص من هذا الجزء، ولازم ينربط لاحقاً:**
- `IDepartmentUsageReader` بدليل فارغ: `employeeCount` يرجع 0 دائماً، وفحصا `HasEmployees` و`HasSharedCustody` ما يمنعان حذف أي قسم لحد ما تتبدل. الموظفون بالخطوة 5، والعهد بالخطوة 13.
- `IWorkShiftUsageReader` بدليل فارغ: `assignedEmployeeCount` يرجع 0 لحد الخطوة 5.
- `IAttendanceRecalculator` بدليل فارغ: تغيير العطلة ما يعيد حساب الحضور لحد الخطوة 6.
- «يُسجل في سجل التدقيق»: الكيانات عليها `[Audited]`، بس عرض السجل وفحص محتواه بالخطوة 11.

## الخطوتان 2 و3: شنو انبنى
- الدخول الفعلي: Code + PKCE، وrefresh، ورفض password grant، وصفحة دخول باللغتين مع خطوة TOTP ورموز استرداد. عميل Flutter منزرع.
- حاجز الوصول من القاعدة بكل طلب، والأدوار الثمانية بصلاحياتها الـ168 من العقد، وإدارة الأدوار والصلاحيات مع الحماية.
- الإعدادات الخمسة، والإشعارات بـ40 قالب باللغتين، وSignalR، وتسجيل أجهزة FCM.
- 401 و403 بصيغة `RemoteServiceErrorResponse`. ملاحظة: 404 لمسار `/Account` غير موجود تحوّل المتصفح لصفحة الخطأ.

**ناقص من هذا الجزء:**
- `Me_GetEmployeeProfile` و`Me_GetInboxSummary` و`Me_GetAdministrativeTaskLog` تعتمد على وحدات لاحقة.
- `includeLocalizationResources` افتراضيه بـ ABP true وبالعقد false.
- `extraProperties` للزائر كائن فارغ، والعقد يكول null.

## الخطوة 1: شنو انبنى
- 21 مشروع: الطبقات، وموديولا Complaints وFinance، ومشاريع الاختبار.
- الأخطاء: `Hr:*` ترجع 422، والحاجزان 403، و`details` فارغ خارج Development، و`members` مسارات camelCase.
- JSON: enums نصوص camelCase وترفض الأرقام، `HH:mm`، `DateOnly`، UTC. القوائم 20 افتراضياً و200 حد أقصى.
- اللغة: `ar` افتراضية و`en`، وأي لغة ثانية ترجع `ar`.
- routes: `ContractRouteConvention` يحذف أي `/api` غير معلن، وauto API controllers متوقفة.
- FluentValidation وMapperly مفعّلان. MiniExcel وAngular وBook/Author والعميل المولّد وhealth-ui انحذفت.
- الأسرار بـ `appsettings.secrets.json` فقط (مستبعد من git).
- قاعدة التطوير انعادت على PostgreSQL 17.

**تقرير Swagger مقابل العقد:** 307 بالعقد، 1 معروض ومعلن، 0 غير معلن، 306 مو مبنية بعد. `ApplicationConfigurationDto` فيه حقول ABP زيادة (`clock`, `currentTenant`, `features`, `globalFeatures`, `multiTenancy`, `objectExtensions`, `timing`)، والعقد نفسه يقول العميل يتجاهلها.

## قرارات معتمدة للتطبيق (زينب، 2026-11-05)
**انطبقت بالخطوة 15.** الجداول تحت تبقى كسجل، وقيمها المؤقتة لهذه البنود ما عادت سارية.

زينب فوّضت اختيار الأنسب من القرارات والبدائل. تُطبق بالخطوة 15 بعد الخطوة 14، بهذا الترتيب: `openapi.yaml` أولاً، ثم الكود، ثم الاختبارات.

**Q المعتمدة بالبديل، وكل Q ثانية تبقى بقيمتها الحالية:**
| # | المعتمد |
|---|---|
| Q9 | البريد والهاتف عبر ABP Background Jobs تنضاف للطابور بنفس المعاملة، حتى ما يضيع إشعار، بدون Outbox كامل |
| Q17 | `employeeNumber` فريد، والتكرار يرجع 409 |
| Q18 | إضافة `Hr:Shift:ShiftInactive` إلى `Employee_Create` بالعقد |
| Q38 | حد استقبال البصمة 120 بالدقيقة لكل IP |
| Q45 | المدير التنفيذي يشوف تفاصيل طلب المحاسب اللي يعتمده |
| Q63 | حمل عدة فترات زمنية باليوم، حتى كل زمنية تعذر التأخر بوقتها |
| Q69 | إضافة أكواد الإجازة إلى `Memo_Convert` بالعقد، حتى يوصل السبب الفعلي |

**D المعتمدة:** D1 إلى D25 وD27 إلى D33، بالمقترح المكتوب بجدولها، مع التوضيحات التالية:
- D10: توثيق 409 بدون كود جديد.
- D25: حذف `Document:FileTooLarge` من `Complaint_UploadAttachment`.
- D32: توثيق إن التفويض ما يشمل التنفيذ.
- D26 ما يلزم، لأن الخطوة 14 تبني التصديرين.

**فروقات صغيرة عن العقد تنصلح بنفس الخطوة:** افتراضي `includeLocalizationResources` يصير false، و`extraProperties` للزائر يصير null.

**البريد إلزامي لكل موظف (قرار زينب، يلغي Q16):** `email` يصير required وغير nullable بـ `EmployeeCreateDto` و`EmployeeUpdateDto` بالعقد، والكود يرفض الفارغ بـ 400، وتنحذف قيمة `<userName>@hr.invalid`. إذا `UserAccountCreateDto.email` فارغ ياخذ بريد ملف الموظف، و`Hr:Identity:NoVerifiedEmail` يبقى للحسابات القديمة بلا بريد.

**ينتظر بيانات من زينب، وما ينحل بالكود:** SMTP وFirebase (Q8)، خادم ClamAV (Q22، و**بدونه كل مرفقات الإجازة ترفض بالإنتاج**، Q57)، S3 (Q23)، خزنة المفاتيح ونسخة احتياطية للمفتاح (Q77)، كلمة سر حساب الشكاوى المقيّد (Q78)، شرط رخصة QuestPDF (Q96)، بيانات الحسابات الأولى الحقيقية (Q15).

## قرارات تنتظر زينب
اشتغلت بالقيمة المذكورة حتى تقرر. تغييرها سهل ما لم يُذكر غير ذلك.

| # | الموضوع | القيمة المؤقتة | البديل |
|---|---|---|---|
| Q1 | سياسة كلمة السر | طول 8 مع شروط التعقيد الافتراضية بـ ABP (حرف كبير وصغير ورقم ورمز) | إعدادات أخف |
| Q2 | الحسابات الأولى | `executive` و`hrmanager` و`sysadmin` بكلمة سر من `Seed:InitialPassword`، وتغييرها إجباري | أسماء ثانية |
| Q3 | حساب `admin` الأصلي | معطّل بكل البيئات | إبقاؤه بالتطوير |
| Q4 | صفحات الحساب | حذف Register وForgotPassword وManage، وبقي `/Account/ResetPassword` لرابط إعادة التعيين | صفحة بفرونت Flutter |
| Q5 | `Hr:Identity:NoVerifiedEmail` | يعني ماكو بريد بالحساب، لأن العقد ما بيه مسار تأكيد بريد | إضافة مسار تأكيد |
| Q6 | قيم الإعدادات الأولية | حسب QUESTIONS (تذكير كل يومي دوام، 15 يوم عمل، مهلة 10، 8 ساعات)، مع إن وصف `SystemSettings_GetReminders` يكول null. `overdueReminderIntervalWorkingDays` يبقى null لأن ما له قيمة بالقرارات | null لحد ما يتحدد |
| Q7 | حد المحاولات (429) | 20 بالدقيقة لكلمة السر وMFA، و60 لـ `/connect/token`. العقد ما يحدد رقم | أرقام ثانية |
| Q8 | قنوات الإشعارات | الداخلي وSignalR يشتغلان. البريد عبر `IEmailSender` (لا يرسل فعلياً بـ Debug وينتظر SMTP). FCM واجهة `IPushSender` بلا إرسال، وتنتظر بيانات Firebase | تزويد البيانات |
| Q9 | Outbox | سطر الإشعار الداخلي بنفس المعاملة، والبريد والهاتف بعد الـ commit بدون إعادة محاولة | إضافة ABP Outbox |
| Q10 | ترجمة أكواد الأخطاء | كل كود له نص إنجليزي مولّد من اسمه، والعربي يكتمل مع بناء كل وحدة | — |
| Q11 | العطلة الأسبوعية الأولية | الجمعة والسبت، من مثال العقد. الوثيقة ما تحدد قيمة | أيام ثانية |
| Q12 | `overtimeAfterHours` غير المطابق لسياسة العمل الإضافي | يرجع 422 `Hr:Shift:InvalidSchedule`، لأن العقد ما يذكر كود ثاني | 400 |
| Q13 | ترتيب بحقل غير مسموح | يرجع 400 على `sorting`. العقد ما يحدد السلوك | تجاهله والرجوع للافتراضي |
| Q14 | تكرار أسماء الأقسام وقوالب الدوام | مسموح، لأن ماكو قاعدة ولا كود يمنعه | منعه بـ 409 |
| Q15 | ملفات الحسابات الأولى | أسماء ومسميات مؤقتة (المدير التنفيذي، مدير الموارد البشرية، مسؤول النظام)، بقسم «الإدارة العامة» ودوام افتراضي أحد إلى خميس 08:00 إلى 16:00، وتاريخ مباشرة يوم الـ seed | بيانات حقيقية منك |
| Q16 | موظف بلا بريد | ABP يفرض بريد، فالحساب ياخذ `<userName>@hr.invalid`. ما يظهر بأي DTO، وما ينرسل عليه، وإعادة التعيين ترجع `NoVerifiedEmail` | جعل البريد إلزامياً |
| Q17 | التكرار عند إنشاء موظف أو حساب | اسم المستخدم أو البريد المكرر يرجع 409. `employeeNumber` مو فريد لأن ماكو قاعدة | جعله فريداً |
| Q18 | دوام غير فعال بـ `Employee_Create` | يرجع `Hr:Employee:WorkShiftNotFound`، لأن العملية ما بيها `ShiftInactive` | إضافة الكود للعقد |
| Q19 | `EmployeeShift_Assign` بنفس تاريخ بداية الربط الحالي | يرجع `StartDateBeforeCurrentAssignment`، لأن إغلاق الربط يوماً قبله يخليه ينتهي قبل ما يبدي | استبدال الربط الحالي |
| Q20 | تغيير البريد أثناء صلاحية رابط إعادة التعيين | يرجع 409 بلا كود، لأن `Employee_Update` ما بيها كود لهذا (D10) | كود جديد |
| Q21 | أنواع الوثائق المسموحة | `application/pdf` و`image/jpeg` و`image/png`، مع فحص توقيع الملف. الحجم 10MB من مثال العقد | أنواع ثانية |
| Q22 | **فحص الفيروسات** | **ClamAV مو مربوط.** البديل يكشف توقيع EICAR فقط وبالتطوير والاختبار فقط. بغيرها يرفض الفحص فتبقى الوثيقة `pending` وما تنزل. **لازم ينربط قبل الإنتاج** | تزويد خادم ClamAV |
| Q23 | تخزين الملفات | ABP BLOB بقاعدة البيانات مؤقتاً، والوثيقة تطلب S3. التبديل بالإعدادات بدون تغيير الكود | تزويد بيانات S3 |
| Q24 | رابط التنزيل | `GET /files/{token}` خارج `/api`، لأن العقد يعتبر الرابط معتم. صالح 15 دقيقة، ويتكرر استخدامه خلالها، وينلغي بحذف الوثيقة. يسري على روابط الوحدات الجاية | مدة ثانية أو مرة واحدة |
| Q25 | رابط إعادة تعيين كلمة السر | صالح ساعتين، وطلبه يفك قفل الدخول | مدة ثانية |
| Q26 | المحاسب بنطاق غير `requests` | يرجع `Hr:Delegation:DelegateNotEligible`، لأن العقد ما يذكر كود ثاني | كود جديد |
| Q27 | بداية التفويض | سماحية 5 دقائق بالماضي، لأن «الآن» توصل متأخرة شوية | صفر |
| Q28 | توقيت الشركة | من `AttendanceSettingsDto.timeZone`، وافتراضيه `Asia/Baghdad` من مثال العقد | — |
| Q29 | `Manager_AssignEmployees` لمدير غير موجود | يرجع 404، لأن العملية ما بيها `ManagerNotFound` | 422 |
| Q30 | **أي يوم تنحسب عليه البصمة** | بين ورديتين: منتصف الفاصل بينهما. بعد آخر وردية: نهايتها + 6 ساعات، ومو قبل منتصف الليل. قبل أول وردية: بدايتها − 6 ساعات، ومو بعد منتصف الليل. غيرها منتصف الليل. الوثيقة تحدد بس إن الليلية تنسب ليوم بدايتها | أرقام ثانية |
| Q31 | **دخول أو خروج** | الجهاز ما يحدد الاتجاه: الأولى دخول والأخيرة خروج. بصمتان بفرق أقل من 5 دقائق تعتبران وحدة. البصمة المفردة تنحسب للطرف الأقرب من الوردية | تعديل الأرقام |
| Q32 | **يوم الراحة** | الوردية تحدد الدوام (وثيقة 5.2)، فاليوم خارج أيامها `weeklyOff` حتى لو مو ضمن إعداد العطلة الأسبوعية، واليوم ضمن أيامها دوام حتى لو ضمنه. العطلة الرسمية تغلب الوردية | إعداد العطلة الأسبوعية يغلب الوردية |
| Q33 | إنتاج الغياب | Worker كل 30 دقيقة لآخر 7 أيام. الأقدم ما ينحسب تلقائياً، حتى ما ينكتب غياب على فترة قبل تركيب الجهاز. يوم دوام جاري بلا بصمة ما له صف | مدد ثانية |
| Q34 | الدوام بيوم راحة أو عطلة | تبقى الحالة `weeklyOff` أو `holiday` مع احتساب `workedMinutes`. `afterShiftEnd` يعطي 0 لأن ماكو نهاية مجدولة، و`afterDailyHours` ينحسب عادي | احتسابها كلها إضافي |
| Q35 | تكرار `AttendanceDevice_Create` بنفس `Idempotency-Key` | يصدر مفتاح جديد ويبطل السابق، لأن المخزن هو الـ hash فقط، والرد يلزم بيه `deviceKey` | 409 |
| Q36 | بصمة غير صالحة | من الجهاز: رقم فارغ أو أطول من 64، أو وقت بعد الآن بأكثر من 5 دقائق، تنحسب `invalid` ويبقى الرد 200. بالملف: الوقت المستقبلي بدون سماحية، والرقم الطويل يرجع `emptyDeviceUserId` لأن ماكو كود أنسب | كود جديد |
| Q37 | ملف الاستيراد | وقت الرفع يُفحص الامتداد والحجم وتوقيع xlsx فقط. الملف التالف يرجع 202 ثم `failed`. أول 1000 خطأ تنحفظ والعدد يبقى كامل. الملف ينحذف بعد المعالجة. CSV يقبل `,` أو `;` | حفظ الملف |
| Q38 | حد المحاولات لاستقبال البصمة | نفس سياسة Q7: 20 بالدقيقة لكل IP، والوسيط يعيد المحاولة عند 429. العقد ما يحدد رقم | رقم أعلى |
| Q39 | `DeviceUserMap_Create` لموظف غير موجود | يرجع 404، لأن العملية ما بيها كود | 422 |
| Q40 | تغيير توقيت الشركة | ما يعيد حساب الأيام الأقدم من 7 أيام | إعادة حساب كل شي |
| Q41 | موظف منتهي الخدمة | ماكو حقل تاريخ إنهاء، فما تنضاف له أيام فارغة جديدة، بس بصماته تنحسب إذا وصلت | إضافة تاريخ إنهاء |
| Q42 | **أعمدة الوقت** | كلها `timestamptz` بدل المفتاح القديم. تغيير Schema لـ 64 عمود، ويشمل جداول ABP. الـ migration يفسر القيم القديمة بتوقيت جلسة القاعدة، وهو `Asia/Baghdad` هنا | ماكو بديل صحيح يبقي القيم الحالية |
| Q43 | مكتبة Excel | ClosedXML 0.105.1 لقراءة xlsx، مع فحص الحجم بعد فك الضغط (200MB). مرشحة كذلك لتصدير التقارير | مكتبة ثانية |
| Q44 | معنى `Hr:Request:NoApproverAvailable` | العقد ما يعرّفه. يرجع عند التقديم إذا أي خطوة بالمسار ما لها صاحب فعال، مو الأولى فقط. مثلاً: ماكو محاسب بالنظام يرفض طلب المحاسب من البداية | فحص الخطوة الأولى فقط |
| Q45 | **من يشوف تفاصيل طلب المحاسب** | حرفياً حسب العقد: الموارد البشرية والمحاسب وصاحب الطلب. **فالمدير التنفيذي يعتمد طلب مدير الموارد ومسؤول النظام وهو يشوف النوع فقط**، وكذلك مقدمه نيابة ومفوض الموارد أو المحاسب. وأخفيت كذلك `rejectionReason` وملاحظات الخط الزمني لأنها نفس النص. ومن يجمع دور محاسب مع مدير قسم أو مسؤول مخزن ما يشوف التفاصيل | إضافة التنفيذي للمسموح لهم، لأنه يعتمد بدون ما يعرف شنو يعتمد |
| Q46 | مفوض بلا صلاحية | أي موظف فعال يصلح مفوضاً (`Delegation_GetCandidates`)، بس `Hr.Request.Inbox` و`Hr.Request.Decide` ما عنده. فالتفويض الساري يكفي للدخول، والفحص الفعلي على الخطوة. راجع D13 | حصر المفوضين بحاملي الصلاحية |
| Q47 | `detailsSummary` للطلب العام | أول 200 حرف من الوصف بنفس النص للغتين، لأن الصندوق ما بيه `description` والمعتمد يحتاج يعرف الموضوع | اسم النوع فقط |
| Q48 | توقيت التذكير | Worker كل ساعة، ويرسل بعد الساعة 8 صباحاً بتوقيت الشركة حتى ما يوصل منتصف الليل. يوصل لصاحب الخطوة ومفوضيه. العد يبدأ من يوم الدوام التالي | ساعة ثانية |
| Q49 | حساب بلا ملف موظف يقدم طلباً | يرجع 403، لأن ماكو مسار له ولا كود بالعقد. عملياً كل الحسابات لها ملفات | كود جديد |
| Q50 | `statusLabel` بالتفاصيل | صاحب الطلب ومقدمه يشوفان نص `employeeStatus`، وغيرهم نص `status` الفعلي | نص واحد للكل |
| Q51 | تفعيل نوع طلب المخزن | يبقى غير مفعل عند الزرع حتى بعد بناء الوحدة (Q101) ومسؤول النظام يفعّله. العقد ما يمنع التفعيل بكود | تفعيله عند الزرع |

| Q52 | حقول `error.data` لنقص الرصيد (INT-21) | `availableDays` و`requestedDays` و`shortageDays` أرقام، حسب وصف `LeaveRequest_Create`. العقد ما بيه schema لها | تعريف schema |
| Q53 | **زمنية بلا استحقاق** | ترجع `EntitlementNotSet` حتى لو الساعات ضمن الحصة، لأن حركة الدفتر تلزمها سنة. والأنواع الأخرى اللي ما تخصم تنقبل بلا استحقاق | السماح بالزمنية ضمن الحصة |
| Q54 | **إجازة بتاريخ ماضي بسنة منتهية** | تنخصم من رصيد السنة الحالية (المضاف أولاً)، لأن متبقي تلك السنة انرحّل أصلاً، وتظهر بـ `segments` بسنتها الفعلية. قبل تاريخ المباشرة يرجع `InvalidDateRange` | رفض التواريخ بسنة مغلقة |
| Q55 | معنى `CorrectionTimeInvalid` | العقد ما يعرّفه. يرجع إذا اليوم قبل المباشرة، أو الوقت بالمستقبل، أو الدخول مو قبل الخروج الحالي، أو الخروج مو بعد الدخول الحالي | تعريف ثاني |
| Q56 | التداخل | زمنيتان بنفس اليوم تتعارضان فقط إذا تداخل الوقت. غيرها يتعارض بالتواريخ، حتى لو اليوم المشترك راحة | احتساب أيام الدوام فقط |
| Q57 | **فحص المرفقات** | يصير أثناء الرفع، وإذا الفاحص ما يجاوب يرجع `AttachmentRejected`. **فبالإنتاج بدون ClamAV (Q22) كل مرفقات الإجازة ترفض، والأنواع اللي تتطلب مرفق ما تنقدم** | تزويد ClamAV |
| Q58 | تقديمان بنفس اللحظة | الثاني ياخذ 409 ويعيد المحاولة، عبر `concurrencyStamp` بحساب الموظف. وأي حركة رصيد تغيّر الـ stamp، فشاشة الموارد المفتوحة تاخذ 409 كذلك | قفل صريح |
| Q59 | `includePending` بتقويم الفريق | افتراضيه true، لأن الوصف يذكر المعتمدة والمعلقة. العقد ما يحدد افتراضي | false |
| Q60 | تخفيض استحقاق السنة القادمة تحت المحجوز عليها | مسموح، لأن ماكو كود يمنعه. الوعاء ممكن يصير سالب بعد الترحيل، و`remainingDays` يبقى صفر كحد أدنى | منعه بكود جديد |
| Q61 | الترحيل عند القراءة | أي قراءة للرصيد ترحّل السنة إذا انتهت وما لحقها الـ Worker، حتى ما يشوف أحد أرقام السنة الماضية. فهذا GET يكتب مرة وحدة بالسنة | الاعتماد على الـ Worker فقط |
| Q62 | نوع طلب `leave` أو `attendanceCorrection` معطّل | ما يمنع التقديم، لأن عمليتيهما ما بيها `TypeInactive` | منعه بكود جديد |
| Q63 | زمنيتان معتمدتان بيوم واحد | الدقائق تنجمع، وبس أبكرهما تعذر التأخر، لأن `AttendanceLeaveCoverage` يحمل فترة وحدة | حمل عدة فترات |
| Q64 | ترتيب قوائم الإجازات | `creationTime` و`startDate` و`endDate` و`kind`. غيرها 400 حسب Q13، و`status` منها، لأن الحالة بجدول الطلبات | إضافة status |
| Q65 | `itemId` غير موجود بالبلاغ أو التحويل | يرجع 400 على `itemId`، لأن العمليتين ما بيهما كود لهذا. بالإصدار 1 ماكو أصناف أصلاً | كود جديد |
| Q66 | **الطلب الناتج من مطالعة** | `submittedBy` هو من حوّلها و`isOnBehalf` = false، لأن العقد يحصر النيابة بمدير القسم. فمن حوّلها يقدر يلغي الطلب، حسب قاعدة «صاحب الطلب أو مقدمه الفعلي» | منع الإلغاء عنه |
| Q67 | خطوة المدير بعد تحويل الموارد البشرية | تتسجل `approvedBySubmission` باسم من حوّل، حتى لو الموارد البشرية، حسب نص العقد | تسجيلها باسم المدير الذي مرت به |
| Q68 | **الموارد البشرية والمطالعة الأصلية** | إذا حوّلها المدير مباشرة فهي ما مرت بالموارد، فتشوف `sourceMemoId` بالطلب وتاخذ 403 عند فتحها، حسب «الكاتب ومن مرت به» | السماح لها بالاطلاع |
| Q69 | أخطاء الإجازة عند التحويل | كل رفض من حساب الإجازة (تداخل، نوع غير فعال، بلا دوام...) يرجع `Hr:Memo:LeaveDetailsInvalid`، لأن العقد يذكر `Leave:InsufficientBalance` فقط. فالسبب الفعلي ما يوصل للشاشة | إضافة الأكواد للعقد |
| Q70 | **حدود مفوض المطالعات والبلاغات** | يقدر كلشي يقدره الأصيل: يحوّل ويغلق ويحدّث حالة البلاغ، وأي تفويض ساري بهذا النطاق يكفي للدخول. راجع D18 | حصره بالتحويل |
| Q71 | ما ينعد بالصندوق للبلاغات | الموارد البشرية `pendingHr` و`inProgress`، والمدير `pendingManager` عنده. قائمة صندوق الموارد تعرض كذلك `pendingManager` لأن الوصف يكول «بلاغات الشركة» | حصرها بما وصلها |
| Q72 | مدير الموارد يحدّث بلاغه | مسموح، لأن العملية ما بيها كود منع، ومنع الاعتماد الذاتي يخص الطلبات | منعه |
| Q73 | **مطالعة مسؤول النظام عند التنفيذي** | التنفيذي يقدر يحيلها للموارد (`forward`) أو يحولها، وما يقدر يغلقها لأن مرحلتها الأخيرة الموارد البشرية. راجع D19 | السماح بالإغلاق |
| Q74 | تغيير المدير | ما ينضاف سطر بالخط الزمني، لأن قيم `action` بالعقد ما بيها إعادة إسناد. المدير القديم يبقى يقرأ المطالعة لأنها مرت به، ويفقد البلاغ لأن الوصول يتبع الربط الحالي | توحيدهما |
| Q75 | `NoManagerLinked` لمن مساره التنفيذي | يرجع إذا ماكو مدير تنفيذي فعال. والمطالعة والبلاغ عند الموارد البشرية ما ينفحص وجود صاحب للدور، فتبقى لحد ما ينعيّن | فحص الموارد كذلك |
| Q76 | ملاحظات الإجازة الناتجة | `notes` تبقى فارغة، ونص المطالعة يوصل عبر `sourceMemoId`، لأن العقد ما يذكر نقله | نسخ النص |
| Q77 | **مفاتيح الشكاوى** | الوثيقة تطلب خزنة سحابية. مؤقتاً: مفتاح رئيسي من `Complaints:MasterKey` بملف الأسرار، ومفتاح كل شركة مشتق منه (HKDF). بدونه الخدمة ترفض بدل ما تخزن بلا تشفير. **ضياع هذا المفتاح يعني ضياع كل الشكاوى، ولازم تنحفظ له نسخة، ويتبدل بخزنة قبل الإنتاج** | تزويد خزنة |
| Q78 | **الحساب المقيّد** | الـ migration ينشئ `hr_complaints_app` بدون دخول، وهذا يحتاج `CREATEROLE` لحساب الـ migrations. الدخول وكلمة السر وconnection string `Complaints` خطوة تشغيل، ولحد ذاك الخدمة بحساب المالك | تأجيل إنشاء الحساب لسكربت تشغيل |
| Q79 | **شكوى مدير الموارد البشرية** | القرار 34 يوديها للتنفيذي، والتنفيذي يشوف السرية جداً فقط، فتصير سرية جداً تلقائياً مع `levelRaisedBySystem` | إبقاء المستوى المختار والسماح للتنفيذي |
| Q80 | من يقدم شكوى | الصلاحية تسمح لمن يحمل `employee` أو `departmentManager`، فالمدير التنفيذي يرجع 403 حتى لو عنده دور ثاني. ومسؤول النظام إذا عنده `employee` يقدر يقدم | منعه |
| Q81 | **معالج بتعيين لشكوى عادية عند مدير القسم** | يشوف العادية بعد وصولها للموارد فقط، مثل مدير الموارد. نص العقد «شكاوى مستوى تعيينه» أوسع، بس الأضيق أحفظ للسرية | كل شكاوى المستوى |
| Q82 | مدير غير صالح | إذا مدير الموظف غير فعال أو ما عنده دور مدير قسم، أو عنده دور مسؤول نظام، تذهب العادية للموارد مباشرة ويبين هذا بالمعاينة | رفضها بـ `NoEligibleHandler` |
| Q83 | **إضافة إجراء أو مرفق** | ما تغيّر `concurrencyStamp`، لأن ردهما ما بيه stamp، وحتى تبقى الشاشة المفتوحة قادرة تحوّل أو تغلق. ABP يغيّره لوحده إذا كانت الشكوى متتبَّعة، فتنحمّل بلا تتبع | stamp جديد بالرد |
| Q84 | `dueAt` | يتحسب عند التقديم من الإعداد وقتها: نهاية يوم الدوام الأخير بتوقيت الشركة. تغيير الإعداد ما يعيد حساب الشكاوى القديمة، والشكوى المقدمة والإعداد فارغ تبقى بلا مهلة | إعادة الحساب |
| Q85 | التذكير بالتأخر | كل ساعة بعد الساعة 8. بلا `overdueReminderIntervalWorkingDays` يرسل مرة وحدة. الشكوى المتأخرة عند مدير القسم تُذكّر الموارد بدون رابط، لأنها ما تقدر تفتحها، ولا تتحول إليها | تحويلها تلقائياً |
| Q86 | أكواد الحجم والنوع للمرفق | الحجم `Complaints:AttachmentTooLarge` والنوع `Document:FileTypeNotAllowed`، و`Document:FileTooLarge` ما يستخدم، لأن العقد يذكر الاثنين للحجم | استخدام Document للحجم |
| Q87 | اسم المستخدم بالرد على الشكوى | `handler` للموارد أو التنفيذي أول حامل فعال للدور، لأن الدور مو شخص محدد، والشكوى ما تخزن معالج غير مدير القسم | تخزين اسم |
| Q88 | بداية تعيين المعالج | سماحية 5 دقائق بالماضي، مثل Q27. `InvalidAssignmentPeriod` إذا أقدم، أو `to` مو بعد `from` | صفر |
| Q89 | `actor` عند تحويل الشكوى بسبب تغيير المدير | من غيّر المدير، لأن الحقل إلزامي بالعقد. فمعالج الشكوى يشوف اسم من نفذ التغيير | الكاتب |
| Q90 | `employeeId` خارج الفريق بالتقارير | يرجع قائمة فارغة مثل `Attendance_GetTeam`، فما يكشف وجود الشخص. العقد يكول «ضمن النطاق» بدون سلوك محدد | 403 أو 404 |
| Q91 | `from` و`to` بتقرير إجازات الفريق | الإجازة بالأيام: التاريخ عند منتصف الليل UTC، مثل مثال العقد. الزمنية: الوقت الفعلي محولاً من توقيت الشركة إلى UTC | توقيت الشركة للكل |
| Q92 | أي إجازات تظهر بتقرير الفريق | المعلقة والمعتمدة، والمرفوضة والملغاة لا، مثل تقويم الفريق | إظهار الكل مع الحالة |
| Q93 | حساب أرقام الحضور | `scheduledWorkingDays` مجموع الحاضر والمتأخر والغائب وغير المكتمل والإجازة. `lateCount` أيام `lateMinutes > 0`، حتى لو الحالة غير مكتمل. منتهي الخدمة بلا أيام بالفترة ما يطلع، والأرصدة تستثني منتهي الخدمة | تعريف ثاني |
| Q94 | التنفيذي بتقارير الشركة | ما له صف بالحضور والأرصدة لأنه خارجهما. يتحدد بأن `ManagerEmployeeId` فارغ، وهذا الشرط يلزم العقد بيه | فحص الدور |
| Q95 | `usedHours` و`closedAt` | `usedHours` مجموع السنة الشخصية، مو الشهر. `closedAt` آخر حركة بالخط الزمني، لأن الطلب ما بيه تاريخ إغلاق | حقل مخزن |
| Q96 | **رخصة QuestPDF وخط PDF** | الوثيقة تطلب QuestPDF، ومضبوطة على رخصة Community، وهي مجانية بشرط إيراد الشركة السنوي تحت مليون دولار حسب شروط المكتبة. **لازم تتأكدين من الشرط قبل الإنتاج.** على Linux لازم خط عربي مثل Noto، وبدونه الحروف تطلع مربعات بدون خطأ | رخصة مدفوعة أو مكتبة ثانية |
| Q97 | تصدير `hrContracts` و`hrWarnings` قبل الإصدار 3 | يرجع 404، لأن ملف فارغ يوحي إن ماكو عقود. العقد يطلب القائمة الفارغة لرموز الإصدار 2 فقط | ملف فارغ |
| Q98 | ترتيب التقارير | الحضور `date` و`lateMinutes`، الإجازات `from` و`to`، الطلبات `creationTime`، التجميعية الاسم والأرقام، الأرصدة الاسم فقط. غيرها 400 حسب Q13 | حقول أكثر |
| Q99 | طلبات المحاسب بتقرير الفريق | تظهر بالنوع والحالة بلا تفاصيل. نص العقد «ولا طلبات المحاسب بتفاصيلها المالية» يحتمل استثناءها كلها | استثناؤها |
| Q100 | تدقيق التصدير | التصدير الناجح فقط يتسجل. المرفوض بـ 403 أو 422 ما يطلع بالسجل | تسجيل المحاولات |
| Q101 | **تشغيل طلب المخزن** | نوع `storeItem` يبقى غير مفعل بعد البناء، فلازم مسؤول النظام يفعّله قبل أول استخدام. قاعدة موجودة ما كانت تتغير بالزرع أصلاً | تفعيله تلقائياً |
| Q102 | رصيد الصنف وختم التعديل | كل حركة تغيّر `concurrencyStamp` الصنف، لأن هذا اللي يمنع حركتين يتجاوزان الرصيد. فشاشة تعديل صنف مفتوحة تاخذ 409 إذا صارت حركة | جدول رصيد منفصل |
| Q103 | عكس حركة | `reversesMovementId` لازم نفس الصنف وبالكمية المعاكسة تماماً، وغيره يرجع 400 لأن العقد ما بيه كود. العكس الجزئي غير مسموح. عكس حركة `issue` ما يغيّر العهدة الناتجة منها | السماح بالجزئي |
| Q104 | قفل نوع الصنف | ينقفل كذلك إذا عنده صرف أو عهدة بدون حركة، مو بس بعد أول حركة، لأن عهدة مباشرة بلا خصم تثبت نوعه | الحركات فقط |
| Q105 | ملاحظة الإرجاع | `note` بالعهدة يعرض ملاحظة الإرجاع إذا موجودة، وإلا ملاحظة التسجيل، لأن الـ DTO بيه حقل واحد | حقلان |
| Q106 | عهدة باسم موظف منتهي الخدمة | ترجع `InvalidHolder`. العقد ما يذكر هذه الحالة | السماح |
| Q107 | من يؤكد العهدة المشتركة | من يحمل دور مدير قسم وعنده موظف غير منتهي الخدمة بالقسم (القرار 31). مسؤول النظام مو منهم حتى لو مديري الأقسام يتبعونه | أي مدير مرتبط |
| Q108 | وقت فتح وإغلاق الدورة | الدورة تبقى مفتوحة بعد المهلة حتى يسمح الرفع المتأخر، وتنغلق عند فتح الدورة التالية. بعدها يرجع `CycleNotOpen` | إغلاقها بتاريخ محدد |
| Q109 | تغيير المهل بالإعدادات | يسري على الدورات غير المغلقة. موعد فتح دورة فتحت، وتثبيت دورة ثبتت، ما يتغيران | الدورات الجاية فقط |
| Q110 | مدة العمل للتقييم (`evaluationMinWorkedDays`) | تنحسب بأيام دوام الشركة من تاريخ المباشرة لآخر الشهر، ومن باشر قبل بداية الشهر يدخل دائماً. الموقوف ما يتولد له نموذج | تعريف ثاني |
| Q111 | معنى «متأخر» | من موعد التصعيد: إذا الإنجاز ما اكتمل، أو اكتمل بعده. فآخر رفع هو اللي يحدد | من `dueAt` |
| Q112 | «الدرجة نفسها للجميع» | لكل معيار، كل الموظفين نفس الدرجة، بشرط تقييمين مرفوعين. فمدير يعطي الكل جيد بمعيار وممتاز بمعيار ينكشف كذلك | درجة وحدة بكل المعايير |
| Q113 | **اعتراض ثاني** | مسموح بعد صدور قرار الأول، ضمن المهلة، لأن العقد يمنع الاعتراض المعلق فقط | اعتراض واحد للتقييم |
| Q114 | التقرير والمدير الجديد | التقرير يبقى باسم المدير وقت التثبيت وبموظفيه وقتها، حتى لو تغيّر المدير بعدها. فالإنجاز يتبع تقييمات هؤلاء | نقل التقرير |
| Q115 | تعميم `company` و`departmentManagers` | المستلمون بحساب وملف فعال، والمرسل ما يستلم تعميمه. ملف بلا حساب ما يوصله شي | إدخالهم |
| Q116 | تعديل ملاحظة التقرير بعد رفع التقرير | مرفوض بـ `AlreadySubmitted`، والتقييم يبقى يترفع بعده، فالجزء المثبت باللقطة يبقى كما كان وقت الرفع، و`completion` يتحدث | تحديث اللقطة |
| Q117 | نص `Hr:Evaluations:*` والإشعارات | كل الأكواد لها نص عربي، والإنجليزي المولّد باقي لأغلبها | صياغة كاملة |
| Q118 | **عكس حركة ما دخلت بكشف** | الحركة العكسية تنكتب بحالة `reversed`، فما تدخل أي كشف، لأن الأصل ما انصرف. نص العقد «يحسب من `open` فقط» مع جعل الأصل `reversed` كان راح يخصم مبلغ ما انصرف أصلاً. الحركة المشمولة بكشف تبقى عكسها `open` وتنسوى بالفترة المحددة | توثيقها بالعقد |
| Q119 | **مبلغ الاقتراح من الحضور** | `suggestedAmount` دائماً null والمحاسب يدخل المبلغ، لأن الوثيقة ما تحدد معادلة. وأساس `overtime` ما يتولد لأن الحضور ما يحسب إضافي | معادلة من زينب |
| Q120 | عملة مصروف المخزن | من `Finance:DefaultCurrency`، وافتراضيه `IQD`، لأن حركة الشراء تسجل التكلفة بلا عملة | إضافة عملة لـ `stock/receive` |
| Q121 | موعد توليد الاقتراحات | عامل كل 6 ساعات للشهر السابق، لحد ما يتقفل. الاقتراح غير المراجع يتبع الحضور، والمراجع ما يتغير | توليد عند إنشاء الكشف |
| Q122 | من يدخل بالكشف | كل موظف غير منتهي الخدمة باشر قبل نهاية الفترة، بيهم المدير التنفيذي إذا له بيانات مالية. ماكو تاريخ إنهاء خدمة (Q41) | استثناءات |
| Q123 | انتهاء مهلة الاعتراض على إنذار | يرجع `Warning:InvalidStatus` و`canObject = false`، لأن العقد ما بيه كود لها (القرار 54) | كود جديد |
| Q124 | `contractDecisionDue` وعقد بلا مدة تنبيه | التذكير مرة وحدة بيوم الموعد أو بعده. العقد المنشأ داخل مدة التنبيه ينبّه بالتشغيل التالي، وموظف منتهي الخدمة يرجع `EmployeeNotFound` للإنذار والعقد والكتاب | تكرار التذكير |

## تعديلات عقد مقترحة (diff بانتظار الموافقة)
ما تغيّر `openapi.yaml`. لحد الموافقة، الكود يلتزم بالعقد الحالي.

| # | الموضوع | المقترح |
|---|---|---|
| D1 | القرار 6: ماكو مسار لتصحيح تاريخ المباشرة، ولا كود لمنع الإنهاء بسبب العهدة | إضافة مسار وكود |
| D2 | 8 صلاحيات تجمع عمليات بأدوار مختلفة: `Hr.Identity.Role.Read`, `Hr.Request.Decide`, `Hr.Memo.Process`, `Hr.Incident.Process`, `Hr.Complaints.Handle`, `Hr.Circulars.Send`, `Hr.Warning.Read`, `Hr.AppreciationLetter.Read` | فصل الصلاحيات |
| D3 | `Report_GetTeamCustody` و`Report_GetTeamEvaluations` و`Report_GetEvaluationConsistency` بالإصدار 1 وتعتمد على الإصدار 2 | نقلها للإصدار 2 |
| D4 | `Mfa_*` مكتوبة للأدوار الأربعة، والقرار (ب) يلزم معالج الشكاوى كمان | تعديل الوصف والأدوار |
| D5 | نصوص متناقضة: «كل نجاح يرد 200» مقابل 9 عمليات 201، ومعنى `x-release` | تنظيف النص |
| D6 | `Circular_GetSentList` و`Circular_GetReceipts` ما يسمحون للتنفيذي (معروف) | إضافته |
| D7 | تغيير من يحمل `hrManager` محصور بالمدير التنفيذي، بس التنفيذي مو ضمن `x-allowed-roles` لـ `UserAccount_SetRoles` و`Employee_Create`. فبالصلاحيات الافتراضية **ماكو أحد يقدر يعيّن أو يغيّر مدير الموارد البشرية عبر النظام** | إضافة `executiveDirector` إلى `UserAccount_SetRoles` |
| D8 | القرار 10 يسمح للموارد البشرية بإنشاء تفويض عن معتمد غائب، و`DelegationCreateDto` ما بيه `fromUserId`. الكود يلتزم بالعقد: التفويض من المستخدم الحالي فقط | إضافة `fromUserId` اختياري للموارد البشرية مع تسجيله |
| D9 | `EmployeeDocument_List` بيه `x-data-scope-by-role.systemAdmin: company`، ووصفها والقرار («العادية: الموارد البشرية والموظف ومديره») ما يعطونه وصول عام. الكود يطبق الأضيق: مسؤول النظام يشوف بس بصفته مدير مباشر | تغييرها إلى `team` |
| D10 | `UserAccount_ResetPassword` يمنع تغيير البريد أثناء صلاحية الرابط، و`Employee_Update` ما بيها كود لهذا | إضافة كود أو توثيق 409 |
| D11 | `AttendanceDevice_Create` يعلن `Idempotency-Key` (التكرار يعيد المورد) و«المفتاح يُعرض مرة واحدة»، والاثنان يتعارضان بالرد المكرر. الكود يطبق Q35 | توثيق إصدار مفتاح جديد عند التكرار |
| D12 | `AttendanceImportErrorCode` ما بيه كود لرقم مستخدم أطول من 64 | إضافة `invalidDeviceUserId` |
| D13 | `Request_GetInbox` و`Request_Approve` و`Request_Reject` تعلن `x-permission` و`x-allowed-roles` بدون استثناء للمفوض، و`Delegation_Create` ينقل حق القرار لأي موظف. الكود يطبق Q46 | توثيق إن التفويض الساري بديل عن الصلاحية بهالعمليات الثلاث |
| D14 | `Hr:Request:NoApproverAvailable` بلا تعريف بـ `error-codes-meaning`، وسياسة التفاصيل ما تذكر المدير التنفيذي مع إنه معتمد بالمسار (Q44 وQ45) | إضافة التعريف وتوضيح جمهور التفاصيل |

| D15 | `LeaveBalance_SetEntitlement` و`LeaveBalance_Adjust` يكولان «يُحال التغيير إلى المدير التنفيذي» عند التعديل الذاتي، بس صلاحيتهما `hrManager` فقط. فبالصلاحيات الافتراضية **ماكو أحد يقدر يحدد أو يعدل رصيد مدير الموارد البشرية، فما يقدر يقدم إجازة من الرصيد** | إضافة `executiveDirector` للعمليتين، ومحصور بمدير الموارد |
| D16 | القرار 17 «لا إلغاء بعد الاعتماد إلا بتصحيح من الموارد البشرية»، و`LeaveLedgerType.reverse` موجود، وماكو مسار لهذا التصحيح. و`adjust` يصلح الرصيد بس ما يرجع أيام الحضور | إضافة `POST /api/app/leave-request/{id}/reverse` مع السبب |
| D17 | `LeaveRequest_Create` و`AttendanceCorrection_Create` و`Memo_Convert` ما يذكرون `Hr:Request:NoApproverAvailable`، وهو يطلع منهم إذا ماكو معتمد فعال | إضافته إلى `x-error-codes` |
| D18 | `Memo_*` و`Incident_*` ما تذكر المفوض بـ `x-allowed-roles`، و`DelegationScope.memosAndIncidents` موجود. نفس مشكلة D13 (Q70) | توثيق إن التفويض الساري بديل عن الصلاحية |
| D19 | `Memo_Forward` يكول «من pendingManager» بس، وأدواره فيها المدير التنفيذي، ومسار مسؤول النظام يبدأ `pendingExecutive` (Q73) | إضافة `pendingExecutive` لوصف العملية |
| D20 | `ManagerChangeResultDto.movedPendingItemCount` موصوف «Pending requests»، والقرار 7 ينقل المطالعات والبلاغات، والكود يعدّها كلها | تعديل الوصف إلى «pending items» |
| D21 | `MemoConvertInputDto.itemId` ما يذكر سلوكه مع `target = leaveRequest`. الكود يرجع `LeaveDetailsInvalid` | توثيقه |
| D22 | **القرار 35 «يرى مقدم الشكوى القرار النهائي»، و`ComplaintComplainantDetailDto` ما بيه حقل للقرار.** فمقدم الشكوى هسه يعرف إنها انغلقت بس، وما يشوف القرار | إضافة `closureDecision` إلى `ComplaintComplainantDetailDto` |
| D23 | القرار 33 يعامل المحاسب مثل مسؤول المخزن، و`ComplaintRespondentCategory` ما بيه `accountant`. الكود يرجع `storeOfficer` للمحاسب، فالمعالج يشوف فئة مو دقيقة | إضافة `accountant` |
| D24 | `Complaint_Close` و`Complaint_AddAction` يذكران `x-allowed-roles` بأدوار ثابتة مع إن صاحب التعيين مسموح له، والعقد نفسه يعتبرها شرطاً أولياً، و`Complaint_GetMyList` فيها `x-permission` مع «لا يرد 403». الكود يتبع النص: `[Authorize]` فقط والرد 404 أو قائمة فارغة | توضيح إن `x-permission` للشاشة هنا |
| D25 | `Document:FileTooLarge` و`Complaints:AttachmentTooLarge` كلاهما بـ `Complaint_UploadAttachment` لنفس الحالة (Q86) | حذف واحد |
| D26 | `Report_Export` إصداره 1 وبيه رمزا `hrContracts` و`hrWarnings` من الإصدار 3، بدون وصف لسلوكهما قبل إطلاق وحدتيهما (Q97) | توثيق 404 لحد الإطلاق |
| D27 | «يجب أن يكون ضمن نطاق المستدعي» بفلتر `employeeId` بتقارير الفريق بدون تحديد الرد (Q90) | توثيق القائمة الفارغة |
| D28 | **`Evaluation_DecideObjection` يذكر «يعدل مدير الموارد البشرية الدرجة مع سبب»، و`EvaluationObjectionDecideDto` ما بيه درجات.** فقبول الاعتراض هسه ما يغيّر أي درجة، وهذا قرار معتمد ما يتطبق | إضافة `scores` اختياري عند `accepted` |
| D29 | **نفس العملية تكول «يبت المدير التنفيذي في اعتراضه»، وأدوارها `hrManager` فقط.** فاعتراض مدير الموارد البشرية يبقى معلق للأبد، لأنه ما يقدر يبت بيه | إضافة `executiveDirector`، محصور باعتراض مدير الموارد |
| D30 | **القرار 41 يحدد مهلة الاعتراض 5 أيام دوام، و`Evaluation_AddResponse` ما بيها كود لانتهائها.** الكود يرجع 400 على `type` | إضافة `Hr:Evaluations:ObjectionWindowClosed` |
| D31 | وصف `acknowledgementRequired` «true لتعميمات departmentManagers»، و`Circular_Create` يكول «مطلوب لكل التعاميم» كقرار معتمد. الكود يرجع true دائماً حسب القرار | تعديل وصف الحقل |
| D32 | `StockIssue_*` محصورة بصلاحيات `storeOfficer`، وصندوق الطلبات يعرض طلب المخزن لمفوض مسؤول المخزن. فالمفوض يشوفه ويقدر يرفضه، وما يقدر يؤشر «اطلعت» أو يسلّم | توثيق إن التفويض ما يشمل التنفيذ، أو إضافته |
| D33 | `Custody_GetTeam` و`CustodyCount_*` يذكرون أقسام المدير بدون تعريف. الكود: أقسام موظفيه المباشرين غير منتهي الخدمة | توثيق التعريف |
| D34 | **القراران 48 و53: المدير التنفيذي يقرر بعقد مدير الموارد وبإنذار يخصه، و`Contract_Recommend` و`Contract_Decide` و`Warning_Decide` و`Warning_Notify` أدوارها `hrManager` فقط.** الكود يرجع `SelfActionNotAllowed` و`SelfDecisionNotAllowed`، فهالحالات **تبقى معلقة للأبد** | إضافة `executiveDirector`، محصور بمدير الموارد |
| D35 | القرار 52 يسمح برفض توصية بلا إبلاغ الموظف، و`Warning_Decide` يكول الحالة `responded` أو `responseExpired` فقط. الكود يطبق القرار: `dismiss` مسموح من `recommended` ولا يصل إشعار | تعديل وصف العملية |
| D36 | القرار 51 «يرى الموظف مستندات الواقعة»، و`WarningEmployeeDto` ما بيه مرفقات ولا مسار تنزيل للموظف. الكود يلتزم بالعقد، فالموظف **ما يشوف المستندات** | إضافة `attachments` ومسار تنزيل |
| D37 | `PayrollRun_ReturnForChanges` يحفظ السبب و`PayrollRunDto` ما بيه حقل له ولا قالب إشعار، فالمحاسب **ما يعرف ليش انرجع الكشف** | إضافة `lastReturnReason` وإشعار |
| D38 | أمثلة العقد تستخدم `requestTypeCode: accountantRequest`، والكود من الخطوة 7 يزرع `accountant` | توحيد القيمة |

## دليل المتابعة (لأي جلسة تكمل الشغل)
**اقرأ العقد عبر:** `node tools/contract-digest.mjs tags` ثم `ops <Tag>` و`refs <Tag>`.

**نمط كل وحدة:**
- الثوابت والتعدادات: `src/HR_Mangment2.Domain.Shared/<Module>/`. الصلاحيات جاهزة بـ `HrPermissions.<Group>Permissions.<Name>`، وأكواد الأخطاء بـ `HrErrorCodes.<Module>.<Name>`، وكلاهما مولّد من العقد.
- الكيانات: `src/HR_Mangment2.Domain/<Module>/`. التسجيل: `DbSet` بـ `HR_Mangment2DbContext` (بدونه ما ينسجل الـ repository) وmapping بـ `HrModelConfiguration`. بعدها `dotnet ef migrations add <Name>` من `src/HR_Mangment2.EntityFrameworkCore`.
- الـ DTOs بأسماء schemas العقد وواجهات الخدمات: `Application.Contracts/<Module>/`. الخدمات: `Application/<Module>/` تحت `HR_Mangment2AppService` مع `[Authorize(HrPermissions...)]`. العمليات اللي `x-permission` بيها null تاخذ `[Authorize]`.
- الـ Controllers: `HttpApi/Controllers/<Module>/` تحت `HR_Mangment2Controller`، بـ `[Route]` و`[Http*(..., Name = "<operationId>")]` و`:guid`. الـ 201 بـ `StatusCode(201, dto)`، والـ 204 بـ `NoContent()`.
- الوحدات الموجودة بموديول (الشكاوى، المالية) تستخدم `modules/HR_Mangment2.<Module>/`، ولكل جدول `<Module>DbProperties.DbSchema`، وتُستدعى `Configure<Module>()` من الـ DbContext الرئيسي.
- مدخلات فيها «الحقول غير المعرفة ترد 400»: `[JsonUnmappedMemberHandling(Disallow)]`.
- الإشعارات: `HrNotificationPublisher.PublishAsync`، والنصوص جاهزة باللغتين. الإعدادات: `HrSettingsStore.GetValueAsync<T>()`.
- الأخطاء: `throw new BusinessException(HrErrorCodes...)`، وأضف النص العربي للكود بـ `ar.json` (النص الحالي إنجليزي مولّد).
- إذا صار ربط موظف أو ملف: `IMyAccountEmployeeReader` بديله يتسجل بالخطوة 5.

**الاختبارات:** `test/HR_Mangment2.HttpApi.Tests` يشغّل الخادم الحقيقي على قاعدة `HR_Mangment2_Test_Api`. المساعدات: `TestUsers.CreateAsync`، `TestUsers.SignedInWithMfaAsync`، `TestAuth.SignInAsync`، `TestAuth.ClientFor`. أضف كل عملية منجزة إلى `implemented-operations.txt`، وtest الـ routes يفحص الـ route والـ `:guid` واسم كل عملية مقابل العقد.

**قبل «خلص» بكل خطوة:** `dotnet build HR_Mangment2.slnx` ثم `dotnet test HR_Mangment2.slnx`، وبعد أي migration شغّل `dotnet run` من داخل مجلد `src/HR_Mangment2.DbMigrator`، لأن الإعدادات والأسرار تنقرا من المجلد الحالي، ومن الجذر يفشل بـ `ConnectionString has not been initialized`.

**أدوات جاهزة للوحدات الجاية:**
- `Idempotency-Key`: استخدم `IdempotencyStore`، ومثاله `WorkShiftAppService.CreateAsync`.
- الموظف والمستخدم: `EmployeeReferenceReader` يبني `EmployeeRefDto` و`UserRefDto` و`DepartmentRefDto` والدوام الحالي. `ICompanyClock` لتاريخ اليوم بتوقيت الشركة.
- الملفات والمرفقات: `IBlobContainer<HrFilesContainer>` و`FileDownloadLinkManager` و`IVirusScanner`، ومثالها `EmployeeDocumentAppService`.
- سجل المهام الإدارية: `AdministrativeTaskLogger.LogAsync`.
- الاختبارات: `EmployeeTestKit` يبني سلسلة المديرين ويدخل كموظف جديد. `CapturingEmailSender` يقرا البريد المرسل. انتبه: `ReadFromJsonAsync` يفشل إذا انقرا نفس الرد بعد قراءة سابقة.

**ملاحظات للخطوة 8:**
- قدّم الإجازة والتصحيح عبر `RequestWorkflowService.ResolveOwnerAsync` ثم `SubmitAsync` بـ `RequestSubmission` مع `Description = null` ونص الملخص باللغتين. النوع من `RequestTypeCodes.Leave` و`AttendanceCorrection`.
- سجّل منفذ `IRequestLifecycleHandler` لكل وحدة، ويشتغل على كل الطلبات فلازم يفحص `RequestTypeId` أولاً. `OnApprovedAsync` يحول الحجز لخصم، و`OnRejectedAsync` و`OnCancelledAsync` يفكان الحجز.
- بدّل `NullAttendanceLeaveReader` بتنفيذ فعلي، وبعد كل قرار إجازة نادِ `IAttendanceRecalculator.RecalculateRangesAsync`.
- تصحيح البصمة: أضف لـ `AttendanceDay` دالة تضبط `CorrectedCheckIn`/`CorrectedCheckOut`، واملأ `corrections` بـ `AttendanceAppService.GetDayDetailAsync`.
- الأعمال بالخلفية: `AttendanceRecalculationJob` مثال جيد، وكل موظف بـ UoW مستقل. بالاختبارات الـ jobs ما تشتغل لوحدها، فاستخدم `RunAttendanceJobsAsync` بـ `AttendanceModuleTests`.
- أي `DateTime` جديد لازم يكون UTC، وإلا Npgsql يرفض الكتابة. لا ترجّع المفتاح القديم.

**فخاخ ABP انكشفت بالخطوة 10:**
- الكلاس ينعرض تلقائياً لواجهة `IFoo` بس إذا اسمه ينتهي بـ `Foo`. بغيره ما ينسجل، وما يطلع أي خطأ.
- الكود اللي ينادى من middleware (مثل حاجز الوصول) لازم `[UnitOfWork]` إذا يستخدم `AnyAsync` أو `CountAsync` على repository.
- إضافة ابن لكيان متتبَّع تغيّر `ConcurrencyStamp` الأب. استخدم `DisableTracking()` إذا ما تريد هذا.
- `[DisableAuditing]` ما يمنع صف سجل التدقيق إذا الطلب غيّر كيان ثاني. الحل بـ `ComplaintAuditingStore`، ونفس الشي يلزم للبيانات المالية بالخطوة 14.

**تحذير لأي سكربت PowerShell:** `@(@('a','b'))` تنفك لمصفوفة وحدة، فلا تستخدمها لعمل استبدال. استخدم أداة Edit.

## ملاحظات
- `abp install-libs` ينادي بعد أي clone، لأن `wwwroot/libs` مستبعد من git.
