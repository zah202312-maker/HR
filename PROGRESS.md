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
| ~~معايير التقييم~~ | ~~`EvaluationCriterion_*`, `EvaluationGrade_*`~~ انحذفت 2026-11-11 |
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

## تطوير 2026-11-11: نوافذ المحاسب: تسليم الراتب والنثريات وترتيب المالية (قرار زينب) — باك ✅ فرونت ✅، ⏳ التجربة بالمتصفح
- **القرارات:** تسليم الراتب تأشير لكل موظف بكشف مغلق (تاريخ ومن سلّم)، والموظف يشوفه بقسيمته. النثرية منفصلة عن الراتب (ما تدخل الكشف ولا القسيمة): أنواع (طعام...)، ومبلغ شهري ثابت لكل موظف من شهر لحد ما يتوقف ويتعدل، وكل شهر قائمة يأشر بيها المحاسب «سلّمتها»، وكل تسليم ينشئ مصروف بتصنيف `pettyCash`. النثرية للمحاسب بس.
- **العقد:** 392 → 404 عملية `x-release: 3`: `PayrollRun_Deliver`، `PayrollRun_UndoDelivery`، `PettyCashType_GetList/Create/Update`، `PettyCashAssignment_GetList/Create/Update`، `PettyCashEntry_GetMonth/Update/Deliver/UndoDelivery`. `deliveredOn`/`deliveredBy` بـ `PayrollItemDto`، و`deliveredCount` بـ `PayrollRunSummaryDto` و`PayrollRunDto`، و`deliveredOn` بالقسائم، و`pettyCash` بـ `ExpenseSource`. صلاحيات `Hr.Finance.PayrollRun.Deliver` و`Hr.Finance.PettyCash.{Read,Manage}` للمحاسب. أكواد `Hr:Finance:{DeliveryDateInFuture,EmployeeNotInPayroll,FuturePeriod,InvalidPeriodRange,PettyCashAssignmentOverlap,PettyCashEntryNotDelivered,PettyCashEntryNotPending,PettyCashTypeInactive,PettyCashTypeNameAlreadyExists}`.
- **الباك (موديول Finance):** `PettyCashEntities.cs` (`PettyCashType`، `PettyCashAssignment`، `PettyCashEntry`)، `PayrollItem.DeliveredOn/DeliveredByUserId`، `PettyCashAppService` (أسطر الشهر تنشأ عند أول قراءة من النثريات السارية، تعديل المبلغ يمشي على غير المسلَّم، الإيقاف ما يقطع شهر مسلَّم، إلغاء التسليم يعكس المصروف)، و`PayrollRunAppService.DeliverAsync/UndoDeliveryAsync`، و`PettyCashController`. migration `Hr_PettyCashAndSalaryDelivery` وانطبق على قاعدة التطوير.
- **الفرونت:** «المالية» تطلع للمحاسب بعد «الرئيسية». تبويبات المالية بالترتيب: رواتب الموظفين، كشوف الرواتب والتسليم، الإضافات والاستقطاعات، النثريات (تسليم الشهر بتحديد متعدد، نثريات الموظفين، الأنواع)، ثم الباقي. صفحة الكشف المغلق: شارة «سُلّم س/ص»، زر «تسليم رواتب الباقين»، وتأشير أو إلغاء لكل موظف. قسيمة الموظف تبين التسليم.
- **اختبار:** `PettyCashTests` و`FinanceModuleTests` (التسليم بالكشف) ✅. 79 من 80 (الفشل القديم بمعايير التقييم). `flutter analyze` ✅، `flutter test` ✅، `flutter build web` ✅.

## تطوير 2026-11-11: المخزن (مشتريات بوصل، مواد بتصنيف ووحدة وسعر، فرق، سند صرف مباشر، حركة، تقارير، لوحة) — باك ✅ فرونت ✅، ⏳ التجربة بالمتصفح
- **القرارات (زينب):**
  1. **(ب)** مسؤول المخزن يشوف أسعار الوحدات وقيمة المخزون وتكلفة الشراء. هذا يلغي القرار 29 بـ `QUESTIONS.md` ويستثني الأسعار من قاعدة «البيانات المالية ما تظهر لمسؤول المخزن» بقرار المالكة. الرواتب والمالية الثانية ما تتغير، ومدير القسم ما يشوف الأسعار.
  2. **(أ)** «الفريق» كيان جديد، يرتبط بقسم بشكل اختياري. صرف مادة تُسترد لفريق يسوي عهدة مشتركة على قسمه، والفريق بلا قسم يرفضها.
  3. **(أ)** سند الصرف المباشر مسار ثاني لمسؤول المخزن، ومسار الطلبات (`StockIssue_*`) يبقى مثل ما هو.
  4. **مخزن واحد.** ماكو جدول مخازن ولا اختيار مخزن.
- **افتراضات ما اعترضت عليها:** سعر الوحدة متوسط مرجّح لتكلفة الشراء. الشراء والصرف مسودة ثم اعتماد يحدّث الرصيد، والمعتمد ما يتعدل والتصحيح بالعكس. رقم الوصل ورقم السند فريدين بالشركة. المورد نص حر. التصنيف والوحدة قوائم يديرها مسؤول المخزن. السند بيه عدة مواد. الصنف بلا حركات ينحذف، وبحركات ينأرشف (`isActive = false`).
- **العقد (وافقت زينب):** 356 → 392 عملية، كلها `x-release: 2` وtag `Store` ولـ `storeOfficer`. تعديلان بعد الموافقة لما انكتب الكود: كود `Hr:StoreDocument:AttachmentRejected` لعمليتي الرفع (فحص الفيروسات)، ومصروف المحاسب صار لكل مادة بالوصل (مرتبط بحركتها) بدل مصروف واحد للوصل، لأن المصروف بوحدة المالية مربوط بحركة وحدة.
  - جديدة (36): `ItemCategory_GetList/Create/Update`، `ItemUnit_GetList/Create/Update`، `Item_Delete`، `StorePurchase_GetList/Create/Get/Update/Delete/Approve/Reverse/UploadAttachment/DeleteAttachment/GetAttachmentDownloadUrl`، `StoreTeam_GetList/Create/Update`، `StoreIssueVoucher_*` بنفس عمليات الشراء، `StoreDashboard_Get`، `StoreReport_GetPurchases/GetStock/GetIssuesByTeam/GetItemMovement/Export`.
  - معدلة: `ItemDto` (+`sku`، `category`، `unit`، `averageUnitCost`، `stockValue`، `stockStatus`)، مدخلات المادة (+`sku`، `categoryId`، `unitId` إلزامية)، `Item_GetList` (+`categoryId`، `stockStatus`)، `StockMovementType` (+`purchase`)، `StockMovementDto` (+`purchaseId`، `issueVoucherId`، `documentNumber`، `team`، `unitCost`، `totalCost`)، `Stock_GetMovements` (+`teamId`، `filter`)، أوصاف `Stock_Receive` وقاعدة البيانات المالية بـ `x-module-rules`.
  - صلاحيات جديدة: `Hr.Store.Purchase.{View,Manage,Approve,Reverse}`، `Hr.Store.Team.{View,Manage}`، `Hr.Store.IssueVoucher.{View,Manage,Approve,Reverse}`، `Hr.Store.Report.{View,Export}`.
  - أكواد جديدة: `Hr:Item:{HasMovements,SkuAlreadyExists,CategoryInactive,UnitInactive}`، `Hr:ItemCategory:NameAlreadyExists`، `Hr:ItemUnit:NameAlreadyExists`، `Hr:StorePurchase:{ReceiptNumberAlreadyExists,InvalidDates,DuplicateItem,NotDraft,NotApproved}`، `Hr:StoreTeam:{NameAlreadyExists,DepartmentInvalid}`، `Hr:StoreIssueVoucher:{VoucherNumberAlreadyExists,InvalidDate,DuplicateItem,TeamInactive,EmployeeNotActive,TeamHasNoDepartment,NotDraft,NotApproved,CustodyAlreadyReturned}`.
  - المواد القديمة تاخذ رمز `ITM-0001` وما بعده بالـ migration، وتصنيفها ووحدتها null لحد ما تتعدل.
- **الباك:** `Store/StoreEntities.cs` (حقول المادة، `ItemCategory`، `ItemUnit`، `StoreTeam`، `Item.ApplyMovement` بالمتوسط المرجّح)، `Store/StoreDocumentEntities.cs` (`StorePurchase`، `StoreIssueVoucher` وسطورهم، `StoreDocumentAttachment`)، `StockLedger` يقيّم كل حركة (سعر الشراء للداخل ومتوسط التكلفة للخارج). الخدمات: `StoreMasterDataAppServices.cs`، `StoreDocumentAppServices.cs`، `StoreReportAppService.cs`، و`StoreDevelopmentControllers.cs`. الاعتماد والعكس بمعاملة وحدة (UoW). `Stock_Adjust` يرفض عكس حركة تابعة لوصل أو سند. migration `Hr_StoreDevelopment` (8 جداول وأعمدة جديدة، رموز `ITM-` للمواد القديمة، ومتوسط أولي من الإدخالات القديمة اللي بيها تكلفة) وانطبق على قاعدة التطوير بالـ DbMigrator مع الصلاحيات.
- **الفرونت (موبايل وويب):** صفحة «المخزن» (`/store`) صارت 9 تبويبات حسب الصلاحية: لوحة التحكم (5 بطاقات)، المواد (رمز وتصنيف ووحدة وسعر وقيمة وحالة، حذف وأرشفة)، المشتريات (وصل بعدة مواد ومجاميع حية، مرفق، اعتماد وعكس)، صرف المواد (سند لفريق وموظف اختياري)، الفرق (مع سجل ما استلمه كل فريق)، حركة المخزون (فلتر بالنوع والمادة والفريق ورقم الوصل أو السند)، التقارير (السبعة مع Excel وPDF)، طلبات الصرف (الموجودة)، والتصنيفات والوحدات. الملفات: `features/ops/store_page.dart` و`store_shared.dart` و`store_documents.dart` و`store_admin.dart`.
- **السايد بار (قرار زينب، نفس اليوم):** «المخزن» يطلع لمسؤول المخزن بعد «الرئيسية» مباشرة (`navLayout` بـ `shell.dart`). مدير الموارد اللي ماسك المخزن يبقى بترتيبه الخاص.
- **العميل المولّد:** `frontend/tool/generate_api.mjs` نسخة Node من `generate_api.py` (Python مو منصّب)، بنفس الناتج، وتزيد عليه إرجاع قائمة مطبوعة لما الرد مصفوفة schema. تشغيله: `node frontend/tool/generate_api.mjs <مجلد فيه node_modules/js-yaml>`. إعادة التوليد كمّلت عمليات كانت ناقصة من التحديث اليدوي (مثل `SystemSettings_*HourlyLeave` و`Contract_GetAttachmentDownloadUrl`) بدون ما تكسر شي.
- **اختبار:** `StoreDevelopmentTests` (5: المادة والرمز والتصنيف والحذف/الأرشفة، الشراء من المسودة للاعتماد والمتوسط المرجّح والمصروف والعكس، المرفق، سند الصرف والعهدة المشتركة والشخصية والعكس، اللوحة والتقارير والتصدير والصلاحيات)، وتحدّثت `StoreModuleTests` (التكلفة صارت تظهر لمسؤول المخزن) و`StoreTestKit` لإنشاء المادة. 86 اختبار ✅ (المخزن والمالية والقرارات والعقد والـ routes). الفشل الوحيد `EvaluationModuleTests.The_scale_is_fixed_and_has_no_management_routes` قديم من حذف معايير التقييم. `flutter analyze` ✅، `flutter test` ✅ (11، و`api_client_test` صار يتوقع 392)، `flutter build web` ✅. ⏳ ما انجرب بالمتصفح.

## تعديل 2026-11-11: التقرير اليومي وإنشاء غرف بدل تقارير المتابعة (قرار زينب) — باك ✅ فرونت ✅، ⏳ اختبارات الباك
- **القرارات:** التقرير اليومي يحل محل تقارير المتابعة (لا فترات ولا نسب). المتابع (ونائبه بتفويض reviewReports) ينشئ «غرفة» لكل فريق: مدير قسم + أفراد من الموظفين النشطين المرتبطين به، وغرفة وحدة لكل مدير عند نفس المتابع، وللغرفة اسم. تقرير واحد لكل متابع باليوم (مو بالمستقبل)، فيه نص كامل عام وصف لكل غرفة: تقدير عمل الباك وعمل الفرونت من سلم `ReviewerGrade` ونص لكل واحد، وملاحظات. الصفوف مرتبة حسب اسم مسؤول الغرفة، ولقطة من وقت إنشاء التقرير. الرفع يدوي للمدير التنفيذي ويشترط النص وتقديرَي كل غرفة. التنفيذي يشوف الغرف داخل التقارير بس. التقارير القديمة انحذفت.
- **العقد:** tag `ReviewRooms` و6 عمليات `x-release: 2` (`ReviewRoom_GetCandidates/GetList/Create/Get/Update/Delete`، صلاحية `Hr.ReviewReports.Manage`)، وschemas `ReviewRoom*`. `ReviewReport*`: انحذف `ReviewReportPeriodType` والفترات و`summary` و`averageCompletion` و`completionPercent` و`projectNotes` و`membersCount`، وانضاف `reportDate` و`content` و`room` و`members` و`backendGrade/backendNotes/frontendGrade/frontendNotes`، والتعديل بـ `roomId`، وفلاتر القائمة `dateFrom/dateTo`. أكواد جديدة `Hr:ReviewReports:{InvalidDate,DateAlreadyReported,NoRooms,ContentRequired}` و`Hr:ReviewRooms:{NotADepartmentManager,MemberNotLinked,ManagerAlreadyHasRoom}`، وانحذف `InvalidPeriod` و`NoTeams`. العقد صار 356 عملية.
- **الباك:** `ReviewRoom` (FullAudited، الأفراد `uuid[]`)، `ReviewReport`/`ReviewReportTeam` بالحقول الجديدة، `ReviewerAccess` (المتابع أو نائبه) مشترك بين `ReviewRoomAppService` و`ReviewReportAppService`، و`ReviewRoomController`. فهارس فريدة filtered على `IsDeleted`: (المتابع، اليوم) و(المتابع، المدير). migration `Hr_DailyReviewReports` يحذف التقارير القديمة وإشعاراتها، وانطبق على قاعدة التطوير بالـ DbMigrator (`AppReviewRooms` موجود، و`AppReviewReports` بيه `ReportDate` و`Content`).
- **الفرونت (موبايل وويب):** «التقرير اليومي» (`/review-reports`: فلتر من/إلى يوم والحالة، تقرير جديد بيوم ونص، والتفاصيل بالنص الكامل وبطاقة لكل غرفة بتقديرَي الباك والفرونت والنصوص والملاحظات، وعرض للقراءة بشارات ملونة) و«إنشاء غرف» (`/review-rooms`: بطاقات بتعديل وحذف، وحوار بالاسم ومدير القسم واختيار الأفراد). النائب يشوف الصفحتين خلال التفويض. العميل المولّد تحدث يدوياً (`review_rooms_api.dart` جديد، وmodels وoperations وhr_api وreview_reports_api).
- **اختبار:** `ReviewReportModuleTests` (الغرف وقواعدها، التقرير اليومي كامل، تحرير اليوم والترتيب) و`ReviewerStandInTests` (النائب ينشئ غرفة وتقرير). البناء: Domain وApplication وEF وHttpApi ✅. ⏳ اختبارات الباك تنتظر إيقاف الباك. `flutter analyze` ✅، `flutter test` ✅ (كلها، و`api_client_test` صار يتوقع 356)، `flutter build web` ✅. ما انجرب بالمتصفح بعد.

## تعديل 2026-11-11: حذف إدارة معايير التقييم والسلم اللفظي (قرار زينب) — باك ✅ فرونت ✅، ⏳ اختبارات الباك
- **القرار:** صفحة «معايير التقييم» (`/evaluation-admin`) انحذفت من الموارد، ومعها الـ API كله. المعايير الخمسة والدرجات الخمس تبقى ثابتة من الـ seed (الكيانات والجداول ما تغيرت، فماكو migration). هذا يلغي قرار 2026-11-10 اللي نقلها للموارد، ويخالف جدول 5.18 بالوثيقة (معايير التقييم كإعداد) بقرار المالكة.
- **العقد:** انحذفت 6 عمليات (`EvaluationCriterion_GetList/Create/Update`، `EvaluationGrade_GetList/Create/Update`)، وtag `EvaluationAdmin`، و8 schemas (`EvaluationCriterion*Dto`، `EvaluationGrade*Dto`)، وقسم `evaluationCriteria` من `SystemSettingsSectionCode`. وانضاف `availableGrades` (مطلوب، `EvaluationGradeOptionDto[]`) لـ `EvaluationHrDto` (خيار أ)، حتى تختار الموارد من سلم الدورة لما تقبل اعتراض، بدل ما تعتمد على `EvaluationGrade_GetList`.
- **الباك:** انحذفت `EvaluationCriterionAppService` و`EvaluationGradeAppService` والـ validators والـ DTOs والواجهات والـ controllers، والصلاحيتان `Hr.Evaluations.Criteria.Manage` و`Hr.Evaluations.Grades.Manage`، والكودان `Hr:Evaluations:LastActiveCriterion` و`MinimumActiveGrades`، ونصوصهم بالترجمة، وقسم الإعدادات. `HrTenantBootstrapper.RevokedDefaults` يسحب المنحتين من `systemAdmin` و`hrManager` بالقواعد الموجودة. `ToHrDtoAsync` يملي `AvailableGrades` من `cycle.Grades` (نفس مصدر نموذج المدير).
- **الفرونت (موبايل وويب):** انحذفت `features/eval/evaluation_admin_page.dart` والـ route وعنصر القائمة ورابط إعدادات النظام. نافذة البت بالاعتراض تاخذ الدرجات من `e.availableGrades`. العميل المولّد تحدث يدوياً لأن Python مو منصّب (`evaluation_admin_api.dart` انحذف، وmodels وoperations وhr_api). ⏳ شغّلي `generate_api.py` لما يتوفر Python حتى نتأكد إن الناتج يطابق.
- **اختبار:** `The_scale_starts_with_five...` انبدل بـ `The_scale_is_fixed_and_has_no_management_routes` (المسارات ترد 404)، و`An_objection_goes_to_hr...` يتحقق من `availableGrades`. البناء: Domain وApplication وHttpApi وEF ✅ (الـ Host مقفول لأن الباك شغال). `flutter analyze` ✅. `flutter test`: فشلان قديمان مو من هالتعديل (`api_client_test` يتوقع 315 عملية والمولّد كان 356 وهسه 350، و`review_report_test` بـ `ReviewReportDto.fromJson`). ⏳ اختبارات الباك تنتظر إيقاف الباك.

## تعديل 2026-11-11: تقييم المراجع للأفراد بتقديرات (قرار زينب) — باك ✅ فرونت ✅
- **القاعدة:** نفس فكرة تقييم الموارد بالضبط، بس بدل الأرقام تقدير (ضعيف، مقبول، متوسط، جيد، جيد جدًا، ممتاز) مع سبب اختياري لكل عنوان، وتاريخ تقييم يختاره المراجع، وبدون أي رقم ولا معدل. عناوين المراجع منفصلة عن عناوين الموارد. يقيّم أي موظف فعّال عدا نفسه والمدير التنفيذي ومسؤول النظام فقط. الموظف ما يشوفه. الإرسال: زر لكل تقييم بتأكيد، وزر للباقي سوية، وإشعار للمدير التنفيذي.
- **العقد:** tag `ReviewerEvaluations` و10 عمليات (`ReviewerEvaluationCriterion_GetList/Create/Update`، `ReviewerEvaluation_GetList/Create/Get/Send`، `ReviewerEvaluationReport_GetList/Send/Get`)، enum `ReviewerGrade`، وschemas `ReviewerEvaluation*`، وقالب `reviewerEvaluationReportSent` ونوع مورد `reviewerEvaluationReport`. أكواد `Hr:ReviewerEvaluation:*` (CriterionNotFound، EmployeeNotAllowed، ItemsIncomplete، NothingToSend، AlreadySent). صلاحيات `Hr.ReviewerEvaluation.Manage` (المراجع) و`Hr.ReviewerEvaluation.ReadReports` (المراجع والمدير التنفيذي).
- **الباك:** `ReviewerEvaluations/ReviewerEvaluationEntities.cs` (العنوان، التقييم، البند، التقرير)، `ReviewerEvaluationAppServices.cs` و`ReviewerEvaluationControllers.cs`، migration `Added_ReviewerEvaluations` (4 جداول، التقدير يتخزن نص) وانطبق على قاعدة التطوير مع الصلاحيات.
- **الفرونت (موبايل وويب):** `features/reviewer_evaluations/reviewer_evaluations_page.dart`: «تقييمات المراجع» (تقييم جديد بتاريخ وتقدير وسبب لكل عنوان، زر إرسال لكل تقييم، إرسال الباقي)، «عناوين تقييم المراجع»، «تقارير تقييم المراجع» وتفاصيلها (التقدير كشارة ملونة والسبب تحته). التقارير تطلع للمدير التنفيذي أول القائمة تحت «تقارير التقييم»، والإشعار يفتحها. العميل المولّد تحدث يدوياً (`reviewer_evaluations_api.dart` وmodels وoperations).
- **إصلاح المنتقي (نفس اليوم):** `Employee_Lookup` يرجّع للمراجع مديري الأقسام بس (لتوصية الإنذار)، فكان المراجع يشوف حسين بس. زينب: المراجع يتابع الأفراد ومديري الأقسام ويقيمهم فرديًا، فلازم يطلعوله كل الموظفين. انضافت `ReviewerEvaluation_GetCandidates` (`GET /api/app/reviewer-evaluation/candidate`) ترجّع كل الموظفين الفعالين عدا المراجع والمدير التنفيذي ومسؤول النظام فقط، ونافذة التقييم تستخدمها. منتقي الإنذار ما تغير.
- **اختبار:** `ReviewerEvaluationModuleTests` (المنتقي يشمل الموظف العادي ومدير القسم ولا يشمل التنفيذي، والموارد 403، الصلاحيات، اكتمال العناوين، تقدير غير معروف 400، التاريخ، ماكو averageScore، الترتيب والأسباب، الموظف ما يشوف، الإرسال الفردي والجماعي، AlreadySent وNothingToSend، إشعار التنفيذي وقراءته، الموارد 403). 22 اختبار مرتبط ✅، `flutter analyze` ✅، `flutter build web` ✅. ما انجرب بالمتصفح بعد.

## تعديل 2026-11-11: المراجع يرسل تبليغات (قرار زينب) — باك ✅ فرونت ✅
- **العقد:** `reviewer` انضاف لـ `x-allowed-roles` بـ `Notice_Create` و`Notice_GetSentList` و`Notice_GetReceipts`، ووصف `Notice_Create` ووصف tag Notices. نفس قواعد الموارد: أي موظف فعّال صالح.
- **الباك:** `Hr.Notices.Send` للمراجع بـ `DefaultRoles`، والزرع انطبق على قاعدة التطوير.
- **الفرونت (موبايل وويب):** زر «إرسال تبليغ» بصفحة «تبليغاتي» لكل من يملك `Notice_Create`، وصفحة «إرسال تبليغ» تطلع للمراجع بالقائمة حسب الصلاحية.
- **اختبار:** `NoticeModuleTests` (المراجع يرسل ويشوف المرسلة والإيصالات والمستلم يستلم) ✅. `flutter analyze` ✅، `flutter build web` ✅.

## تعديل 2026-11-11: نائب المراجع لتقارير المتابعة (قرار زينب) — باك ✅ فرونت ✅
- **العقد:** قيمة `reviewReports` بـ `DelegationScope` (للمراجع بس، وما تدخل بـ allApprovals). `reviewer` انضاف لـ `Delegation_Create` و`Delegation_GetCandidates` و`Delegation_Cancel`، ووصف القاعدة بـ `Delegation_Create`. عمليات `ReviewReport_*` الست صارت تقبل النائب بتفويض فعّال ولو ما عنده x-permission. `onBehalfOfUserId` بـ `ReviewReportCreateDto`، و`preparedBy` بـ `ReviewReportDto` و`ReviewReportSummaryDto`.
- **الباك:** `DelegationAppService`: المراجع يفوض `reviewReports` بس، وغيره ما يستخدمه (`DelegateNotEligible`)، والتداخل ما يحسب `reviewReports` ضد allApprovals. `ReviewReportAppService`: `DelegatingReviewersAsync` (تفويضات reviewReports الفعالة للمستخدم)، والقائمة والإنشاء والقراءة والتعديل والحذف والإرسال تقبل المراجع أو نائبه. `preparedBy` = منشئ التقرير (`CreatorId` الموجود، فما احتاج migration). `Hr.Delegation.Manage` للمراجع بالزرع وانطبق على قاعدة التطوير.
- **الفرونت (موبايل وويب):** صفحة «التفويض» تعرض النطاقات المسموحة لكل شخص بس (المراجع: «تقارير المتابعة»). `standInForProvider` (تفويضات reviewReports الواصلة الفعالة): «تقارير المتابعة» تطلع للنائب بالقائمة خلال مدة التفويض (`navLayout(alsoVisible)`)، وبالإنشاء يختار «عن المراجع»، ويكدر يعدّل ويحذف ويرسل المسودة. القائمة والتفاصيل تبين «أعدّه» إذا غير المراجع.
- **اختبار:** `ReviewerStandInTests` (قبل التفويض 403، المراجع ما يفوض غير reviewReports، الموارد ما تستخدمه، النائب ينشئ ويقيّم ويرسل وpreparedBy صحيح، المراجع يشوفه، الغريب 403، بعد إلغاء التفويض 403). 18 اختبار ✅ عدا `viewedAt` القديم. `flutter analyze` ✅، `flutter build web` ✅. ما انجرب بالمتصفح بعد.

## تعديل 2026-11-11: تقارير التقييم أول السايد بار للمدير التنفيذي (قرار زينب) — فرونت فقط
- `shell.dart`: `executiveTopPaths` (`/review-reports` تقارير المتابعة من المراجع، و`/hr-evaluation-reports` تقارير تقييم الموارد) تطلع للمدير التنفيذي بعد «الرئيسية» مباشرة تحت عنوان «تقارير التقييم»، وبعدها باقي القائمة تحت «القائمة». `flutter analyze` ✅، `flutter build web` ✅.
- «المراجع يرسل تقييم أسبوعي وتقرير يومي»: أكدت زينب إن المقصود تقارير المتابعة الموجودة (daily وweekly وmonthly). ما احتاج تغيير.

## تعديل 2026-11-11: داشبورد المدير التنفيذي (قرار زينب) — باك ✅ فرونت ✅
- **العقد:** `Report_GetTodayAttendanceSummary` (`GET /api/app/report/today-attendance-summary`، `Hr.Reports.TodaySummary`، للمدير التنفيذي بس)، schema `TodayAttendanceSummaryDto` (التاريخ، الحاضرون ومنهم المتأخرون، الغائبون، المجازون). أعداد بس بدون أسماء.
- **الباك:** `ReportQueries.GetTodaySummaryAsync` بنفس حساب `GetHrAttendanceAsync` ليوم واحد، و`ReportAppService.GetTodayAttendanceSummaryAsync`. الصلاحية بالزرع وانطبقت على قاعدة التطوير.
- **الفرونت (موبايل وويب):** `home_page.dart`: للمدير التنفيذي 4 بوكسات بس: بانتظار إجرائي، الشكاوى الواصلة، الحاضرون اليوم (ومنهم كم متأخر)، بإجازة اليوم. «غير مقروءة» انشال (الرقم على الجرس). بوكسات الحضور ما تفتح صفحة لأن ما عنده حضور الموظفين.
- **اختبار:** `ExecutiveOverviewTests` يقارن الأعداد بمجموع تقرير حضور الموارد لنفس اليوم، و403 للموارد ومدير القسم. ✅ (الفشل الوحيد `ReviewReportModuleTests` القديم). `flutter analyze` ✅، `flutter build web` ✅.

## تعديل 2026-11-11: رواتب الموظفين وعقود الموظفين للمدير التنفيذي (قرار زينب) — باك ✅ فرونت ✅
- **العقد:** `SalaryProfile_GetOverview` (`GET /api/app/finance/salary-profile/overview`، `Hr.Finance.SalaryProfile.ReadOverview`) و`Contract_GetOverview` (`GET /api/app/contract/overview`، `Hr.Contract.ReadOverview`)، للمدير التنفيذي بس وللقراءة. schemas: `EmployeeSalaryOverviewDto` (الموظف، الراتب الأساسي، العملة، تاريخ النفاذ؛ بدون مخصصات ولا تاريخ) و`EmployeeContractOverviewDto` (الموظف، نوع العقد الحالي، البداية، النهاية، الحالة؛ بدون مرفقات ولا توصية ولا قرار)، والموظف بلا راتب أو عقد يطلع والحقول null. بحث بالاسم أو الرقم الوظيفي وتقسيم صفحات. الموظفون الفعالون فقط.
- **الباك:** `SalaryProfileAppService.GetOverviewAsync` (موديول المالية، وكل قراءة تتسجل كاطلاع على بيانات مالية) و`ContractAppService.GetOverviewAsync` (العقد الحالي = آخر بداية). الصلاحيتان بـ `HrPermissions.DefaultRoles` للمدير التنفيذي، والزرع انطبق على قاعدة التطوير.
- **الفرونت (موبايل وويب):** `features/hrx/executive_overview_pages.dart`: «رواتب الموظفين» (`/employee-salaries`) و«عقود الموظفين» (`/employee-contracts`)، كل سطر صورة واسم وقسم ويمه الراتب أو العقد وحالته، مع بحث. بالقائمة حسب الصلاحية فتطلع للمدير التنفيذي بس.
- **اختبار:** `ExecutiveOverviewTests` (الحقول المسموحة فقط، القيم، null بلا راتب أو عقد، العقد الأحدث، 403 للموارد والمحاسب ومدير القسم، والمدير التنفيذي ما يوصل للملف المالي الكامل ولا قائمة العقود). 31 اختبار ✅، `flutter analyze` ✅، `flutter build web` ✅.

## تعديل 2026-11-11: صفحات مخفية عن المدير التنفيذي (قرار زينب) — فرونت فقط
- المدير التنفيذي (`Session.isExecutive`) ما يشوف بالقائمة: طلباتي، الإجازات، حضوري، الإنذارات، العطل، أرصدة الإجازات، التقييمات (تقييماتي)، وزر «طلب جديد» بالرئيسية.
- `shell.dart`: `executiveHiddenExactPaths` (`/requests`، `/requests/new`، `/warnings`، `/evaluations`) و`executiveHiddenPaths` (`/leaves`، `/attendance`، `/holidays`، `/leave-balances`). `router.dart` يرجّع الروابط المخفية للرئيسية. التفاصيل اللي تنفتح من الوارد أو الإشعار تبقى شغالة: `/requests/:id` (الموافقة)، `/warnings/:id` (`Warning_Get`)، `/evaluations/open/:id` (`Evaluation_DecideObjection`).
- صلاحيات الخادم ما تغيرت. `flutter analyze` ✅، `flutter build web` ✅. ما انشاف بالمتصفح بعد.
- **المطالعات للمدير التنفيذي:** تبويب «بانتظاري» بس، بدون «مطالعاتي» وبدون زر «مطالعة جديدة» (`memos_page.dart`). الخادم ما تغير.
- **قسائم الراتب والعقود:** انخفت عن المدير التنفيذي (`/payslips` و`/contracts` بـ `executiveHiddenPaths`)، لأنها صفحات الموظف لراتبه وعقده، والمدير التنفيذي ما عنده راتب ولا عقد، وما عنده أي عملية عقود ثانية بالعقد.
- **الشكاوى للمدير التنفيذي:** ما يقدّم شكوى: انشال زر «شكوى جديدة» وتبويب «شكاواي»، ويبقى «صندوق المعالجة» للشكاوى اللي توصله (`complaints_page.dart`). `/complaints/new` انضاف لـ `executiveHiddenExactPaths`. الخادم ما تغير.

## تعديل 2026-11-11: صفحات مخفية عن المحاسب ومسؤول المخزن (قرار زينب) — باك ✅ فرونت ✅
- **القاعدة:** المحاسب أو مسؤول المخزن اللي ما عنده دور عمل غير `accountant` و`storeOfficer` و`employee` (`IsBackOfficeOnly`) ما يشوف: التعاميم، التقييمات (تقييماتي)، الإنذارات، كتب الشكر، تبليغاتي، العطل. ولا يوصله تعميم ولا تبليغ. إذا عنده دور ثاني (مثل مدير الموارد ماسك المخزن) يبقى يشوفها ويستلمها.
- **العقد:** وصف `CircularScope` والمستلم الصالح بـ `Notice_Create`.
- **الباك:** `AccountProvisioner.IsBackOfficeOnly` و`ReceivesNoCirculars` (مسؤول نظام فقط أو محاسب/مخزن فقط). `CircularAppService.ResolveRecipientsAsync` يستثنيهم، و`NoticeAppService` يرفضهم بـ `Hr:Notices:RecipientNotAllowed`.
- **الفرونت (موبايل وويب):** `Session.isBackOfficeOnly`، و`backOfficeHiddenPaths` بـ `shell.dart` (القائمة) و`router.dart` (الروابط المباشرة ترجع للرئيسية). صلاحيات الخادم لصفحات التقييمات والإنذارات وكتب الشكر والعطل ما تغيرت (إخفاء بالواجهة).
- **اختبارات:** `BackOfficeRecipientRuleTests` (القاعدة لكل تركيبة أدوار)، و`NoticeModuleTests` (رفض المحاسب ومسؤول المخزن كمستلمين). 29 اختبار ✅، `flutter analyze` ✅، `flutter build web` ✅. ⏳ ما انضاف اختبار API للتعميم نفسه (يستخدم نفس القاعدة).

## تعديل 2026-11-11: رقم الجرس وتمييز الإشعار الجديد (قرار زينب) — فرونت فقط
- **القرار:** الرقم ينقص واحد مع كل ضغطة على إشعار (ما يصير صفر بمجرد فتح الصفحة، فما احتجنا عملية بالعقد).
- `unread_count.dart`: `unreadCountProvider` صار `StreamNotifierProvider` (`UnreadCount`) بيه `markedOneRead()`: ينقص الرقم فوراً بعد `Notification_MarkRead` الناجح، وبعدها يقرا العدد من الخادم مرة ثانية.
- `notifications_page.dart`: الإشعار غير المقروء ببطاقة ملونة بلون التمييز وإطار، وأيقونة جرس فعّال، وعنوان عريض، وشارة «جديد». القائمة تتحدث وحدها إذا وصل إشعار جديد والصفحة مفتوحة. `HrCard` صار بيه `highlighted`.
- `flutter analyze` ✅، `flutter build web` ✅. ما انجرب بالمتصفح بعد. ملاحظة: الرقم يزيد إذا وصلت إشعارات جديدة (مثل تنبيهات معدل التقييم أول ما اشتغل العامل).

## تعديل 2026-11-11: ترتيب السايد بار لمدير الموارد (قرار زينب) — فرونت فقط
- `shell.dart`: `hrManagerSections` و`navLayout(session)`. لمدير الموارد (بدون مسؤول نظام) القائمة صارت 7 مجموعات بعناوين وبالترتيب اللي حددته زينب: القائمة الرئيسية، الخدمات الوظيفية، التواصل والمتابعة، الإدارة والموظفون، التقييم والأداء، المالية والتقارير، النظام. تسجيل الخروج يبقى مثبت تحت. كل صفحة تطلع بس إذا عنده صلاحيتها، وأي صفحة ثانية مسموحة له ومو بالقائمة (من دور ثاني مثل المخزن) تطلع تحت «أخرى». بالشريط المصغر العناوين تصير فواصل. باقي الأدوار ما تغيرت.
- `flutter analyze` ✅، `flutter build web` ✅. ما انشاف بالمتصفح بعد.

## تعديل 2026-11-11: زر إرسال لكل تقييم موارد (قرار زينب) — باك ✅ فرونت ✅
- **العقد:** `HrEvaluation_Send` (`POST /api/app/hr-evaluation/{id}/send`، Idempotency-Key، 201 `HrEvaluationReportDto`) يرسل تقييم واحد بتقرير مستقل ويوصل `hrEvaluationReportSent` للمدير التنفيذي. تقييم مدير موارد ثاني 404، والمرسل قبل `Hr:HrEvaluation:AlreadySent` (كود جديد). زر «إرسال للمدير التنفيذي» العلوي (`HrEvaluationReport_Send`) يبقى للباقي سوية.
- **الباك:** `HrEvaluationAppService.SendOneAsync`، والإرسال المشترك انفصل بـ `SendAsync`. الرسالة بالعربي والإنجليزي وبـ `error_messages.dart`.
- **الفرونت (موبايل وويب):** بقائمة «تقييم الموظفين» صار كل تقييم بيه زر «إرسال» بدل الشارة. الزر يفتح تأكيد «هل أنت متأكد من إرسال هذا التقييم إلى المدير التنفيذي؟» ويعرض الموظف والدرجات والمعدل والملاحظات. بـ «نعم، أرسل» ينرسل، والزر يصير معطّل ومكتوب «تم الإرسال» (والمرسلة قبل كذلك).
- **اختبار:** `HrEvaluationModuleTests.One_evaluation_is_sent_alone_after_confirmation_and_only_once`. اختبارات تقييم الموارد والعقد (10) ✅، `flutter analyze` ✅، `flutter build web` ✅.

## تعديل 2026-11-11: ترتيب الرئيسية وإلغاء دقائق التأخر (قرار زينب) — فرونت فقط
- انشال بوكس «دقائق التأخر» من الرئيسية (`home_page.dart`). مخطط «حالة أيام الشهر» بقى مثل ما هو.
- كل البوكسات صارت بشبكة وحدة وبنفس العرض والارتفاع (220): بانتظار إجرائي، غير مقروءة، الشكاوى الواصلة، حضور اليوم لمن أتابعهم، ثم حضوري ورصيدي. من 700 بكسل: 3 أو 2 أعمدة حسب الأقل فراغاً (4 بوكسات = 2×2). على الموبايل تنرص فوق بعض بنفس الارتفاع.
- `flutter analyze` ✅، `flutter build web` ✅. ما انشاف بالمتصفح بعد.

## تعديل 2026-11-11: صورة الحساب الشخصية (قرار زينب) — باك ✅ فرونت ✅
- **القاعدة:** صاحب الحساب بس يرفع صورته أو يغيرها أو يحذفها (من «حسابي»). الكل يشوفها بجانب الاسم، حتى مسؤول المخزن. التطبيق يقصها مربع ويصغرها لـ 512 بكسل JPEG قبل الرفع (حزمة `image`).
- **العقد:** `Me_UploadPhoto` (`POST /api/app/me/photo` multipart، JPG/PNG/WebP لحد 1MB، الخادم يفحص أول البايتات + فحص الفيروسات، الأخطاء `Hr:Document:FileTooLarge` و`Hr:Document:FileTypeNotAllowed`)، `Me_DeletePhoto` (204)، `ProfilePhoto_Get` (`GET /api/app/profile-photo/{id}`، أي مستخدم داخل، `Cache-Control: private, max-age=31536000, immutable`). حقل `photoId` اختياري بـ `EmployeeRefDto` (وصفه صار يستثني photoId من «Never add other fields») و`UserRefDto` و`MyAccountDto`، وكذلك `EmployeeListItemDto` و`EmployeeHrDto` و`EmployeeSelfDto` و`EmployeeTeamMemberDto` (ضمن نطاق التصميم المعتمد: ملف الموظف والقوائم وفريقي).
- **الباك:** كيان `ProfilePhoto` (جدول `AppProfilePhotos`، فهرس فريد على `UserId`، الملف بـ `HrFilesContainer` باسم `profile-photos/{id}`). كل رفع معرّف جديد ويحذف القديم (صف وملف). `ProfilePhotoReader` يجيب الصور دفعة وحدة، ويستخدمه `EmployeeReferenceReader` (كل `EmployeeRefDto`/`UserRefDto`) و`EmployeeAppService` و`DelegationAppService`. migration `Added_ProfilePhotos` وانطبق على قاعدة التطوير. `[DisableAuditing]` على الرفع والقراءة.
- **الفرونت (موبايل وويب):** `core/widgets/person_avatar.dart` (`PersonAvatar` و`PersonName`، الصورة تنقرى مرة وحدة لكل معرّف وتبقى بالذاكرة، وإذا ماكو صورة يطلع الحرف). بطاقة «صورتي» بـ «حسابي» (`profile_photo_card.dart`). الصورة تطلع: الشريط العلوي، المنتقيات كلها (`showSearchPicker` تلقائياً)، قائمة الموظفين، فريقي، ملف الموظف وسجل المدراء، الوارد، تفاصيل الطلب وخطوات الموافقة، المطالعات، البلاغات، الحضور، الإجازات، التفويض، مستلمو التعاميم والتبليغات، بطاقات العقود والإنذارات وكتب الشكر والتقييمات وتقييم الموارد وتنبيهاتها والمالية والرواتب. `OpsTile` صار بيه `avatar` و`InfoRow` صار بيه `leading`.
- ⏳ **ما تغطى بعد:** الأسماء داخل سطور ثانوية («بواسطة فلان · التاريخ») وجداول التقارير والتصدير وعناوين بعض صفحات التفاصيل (`SectionTitle`). تبقى نص.
- **اختبارات:** `ProfilePhotoTests` (الرفض حسب النوع والحجم والمحتوى، الرفع والقراءة من المدير ومسؤول المخزن والموارد، 401 بدون دخول، الظهور بـ me وملف الموظف وفريقي والقائمة، الاستبدال يحذف القديم، الحذف، ومحد غير صاحبها يغيرها). اختبارات الحقول بـ `EmployeeModuleTests` و`ComplaintModuleTests` (قائمة المشتكى عليهم) صارت تقبل `photoId`.
- **التحقق:** build ✅، كل الاختبارات 392: نجحت عدا 3 قديمة معروفة (`Create_enforces_the_contract_rules`، `The_hr_managers_leave_memo...`، `ReviewReportModuleTests` دقة `viewedAt`). `flutter analyze` ✅، `flutter build web` ✅. ما انجرب يدوياً بالتطبيق بعد.

## تعديل 2026-11-11: تنبيه المعدل الشهري لتقييم الموارد (قرار زينب) — باك ✅ فرونت ✅
- **القاعدة:** بنهاية كل دورة (`windowWeeks`، افتراضياً 4 أسابيع) يحسب الخادم لكل موظف فعّال ولكل عنوان فعّال: مجموع الدرجات ÷ عددها، بشرط عنده `minEvaluations` تقييمات أو أكثر بالدورة (افتراضياً 4). إذا المعدل `threshold` أو أقل (افتراضياً 7) ينشأ `HrEvaluationAlert` ويوصل `hrEvaluationLowAverage` لكل مدير موارد فعّال (عدا الموظف نفسه إذا كان مدير موارد). الدورات متتالية فنفس الدرجات ما تنحسب مرتين. إشعار بس، بدون أزرار إجراء؛ الإجراء من الصفحات الثانية (مثل «إرسال تبليغ»).
- **العقد:** `HrEvaluationAlert_GetList` و`HrEvaluationAlert_Get` (`/api/app/hr-evaluation-alert`، صلاحية `Hr.HrEvaluation.Manage`)، و`SystemSettings_GetHrEvaluationAlert` و`SystemSettings_UpdateHrEvaluationAlert` (`/api/app/system-settings/hr-evaluation-alert`، صلاحية جديدة `Hr.Settings.HrEvaluationAlert` لمدير الموارد)، وschemas `HrEvaluationAlertDto` و`HrEvaluationAlertPagedResultDto` و`SystemSettingsHrEvaluationAlertDto` و`...UpdateInputDto`، وقيم `hrEvaluationLowAverage` و`hrEvaluationAlert` (نوع مورد وقسم إعدادات).
- **الباك:** `HrEvaluations/HrEvaluationAlerts.cs` (الكيان + `HrEvaluationAlertService` + `HrEvaluationAlertWorker` كل ساعة)، `HrEvaluationAlertAppService` + `HrEvaluationAlertController`، `HrEvaluationAlertSettings` وحالة داخلية `HrEvaluationAlertState` (آخر يوم دورة) بـ `HrSettingsStore`. أول دورة تنتهي باليوم اللي قبل أول فحص. migration `Added_HrEvaluationAlerts` وانطبق على قاعدة التطوير.
- **الفرونت (موبايل وويب):** صفحة «تنبيهات التقييم» `/hr-evaluation-alerts` وتفاصيلها (زر «فتح ملف الموظف»)، الإشعار يفتح التفاصيل، وبطاقة «تنبيه معدل التقييم» بالإعدادات. الكلاينت المولّد تحدث يدوياً.
- **اختبارات:** `HrEvaluationAlertTests` (المثال 10/9/6/7 ما يطلع تنبيه و7/7/6/7=6.75 يطلع، أقل من 4 تقييمات ما ينحسب، الإشعار والصلاحيات، التكرار ما يضيف، الدورة تنحسب مرة وحدة، الإعداد والتحقق و409). `SettingsAndNotificationTests` صار 17 قسم.
- **التحقق:** build ✅، 105 اختبار (تقييم الموارد + الإعدادات + العقد) ✅ عدا `Create_enforces_the_contract_rules` القديم، `flutter analyze` ✅، `flutter build web` ✅.

## تعديل 2026-11-11: صوت تنبيه مع كل إشعار جديد (قرار زينب) — فرونت فقط
- كل إشعار جديد (تعميم، تبليغ، تذكير، طلب...) يطلع ويّاه صوت قصير، على الموبايل والويب. الفحص صار كل 20 ثانية بدل دقيقة (`unreadPollInterval` بـ `unread_count.dart`)، والصوت يطلع لما يزيد عدد غير المقروءة بين قراءتين (`NotificationSound`، والمستمع بـ `_TopActions` بـ `shell.dart`). أول قراءة بعد الدخول ونقصان العدد بلا صوت.
- مكتبة `audioplayers: ^6.8.1` انضافت (موافقة زينب)، والنغمة `assets/sounds/notification.wav` سويتها بنفسي (نغمتان قصيرتان، بلا حقوق).
- **حدود:** يشتغل والتطبيق مفتوح بس. لما يكون مسكّر يحتاج إشعارات push (Firebase، Q8 ينتظر البيانات). المتصفح يمنع الصوت لحد أول ضغطة بالصفحة. `flutter analyze` نظيف و`flutter build web` نجح. ما انجرّب الصوت فعلياً على جهاز.

## تعديل 2026-11-11: المحاسب ومسؤول المخزن تحت المدير التنفيذي وبلا دوام (قرار زينب) — باك ✅ فرونت ✅
- **القرار:** المدير المباشر للمحاسب ولمسؤول المخزن هو المدير التنفيذي. وما عندهم دوام ولا حضور ولا إجازة ولا تصحيح بصمة، مثل مدير الموارد. طلباتهم الأخرى (مخزن، محاسب، احتياج إداري) تمر بالتنفيذي كمدير ثم بمسارها. مدير الموارد يجوز له يحمل دوري المخزن والمحاسب سوية، وغيره لا.
- **العقد:** صفان جديدان بـ `x-global-rules.manager-link` (`accountant` و`storeOfficer` → `executiveDirector`) قبل `anyOther`. وصف قاعدة التركيبات بـ `Employee_Create` و`UserAccount_Create` و`UserAccount_SetRoles` يذكر استثناء `hrManager`، ووصف من بلا إجازة، ومعنى `Hr:Employee:WorkShiftRequired`.
- **الباك:** `ManagerLinkService.AllowedManagerRoles` (بعد صف المراجع) و`IsOutsideSchedule` يشملان الدورين، فكل فحوصات الحضور والدوام والإجازة والرصيد تطبق عليهم تلقائياً. `AccountProvisioner.EnsureAllowedCombination` يسمح بالمخزن+المحاسب لحامل `hrManager`. الزرع (`MoveAccountantsAndStoreOfficersToExecutiveAsync`) ينقل الموجودين للتنفيذي مع نقل معلقاتهم، ويغلق دوامهم.
- **الفرونت:** `Session.isOutsideSchedule` يخفي «إجازاتي» و«حضوري» عنهم، ونموذج إضافة موظف ما يطلب دوام لهم، ونص قواعد الربط تحدث. صفحات المخزن والمحاسبة تنفتح لمدير الموارد إذا حمل الدور، لأنها تمشي بالصلاحية.
- **الاختبارات:** `EmployeeTestKit.AddAsync` يربط المحاسب ومسؤول المخزن بالتنفيذي تلقائياً. `EmployeeModuleTests`: رفض مدير القسم كمدير لمسؤول مخزن، بلا دوام، تغيير الدور يحتاج التنفيذي ثم يشيل الدوام والرصيد، و`Only_the_hr_manager_may_be_store_officer_and_accountant_together`. اختبارات الموظفين والمخزن والمالية والطلبات والشكاوى والقرارات والمصفوفة تمر (عدا `Create_enforces_the_contract_rules` القديم). الـ DbMigrator اشتغل على قاعدة التطوير.

## تعديل 2026-11-10: الرصيد السنوي الافتراضي بدل التحديد اليدوي (قرار زينب) — باك ✅ فرونت ✅
**القرار (يغيّر الوثيقة 5.4):** ماكو تحديد عدد أيام لكل موظف. رقم واحد للشركة (أول قيمة 24) يعدّله مدير الموارد من الإعدادات. سنة الموظف الجديد تنفتح بيه من مباشرته، والموجودين بلا سنة ياخذونه تلقائياً. تغيير الرقم يوصل الكل من بداية سنته الجاية، والسنة الحالية ما تتغير. الفرق لشخص واحد بـ `LeaveBalance_Adjust` مع السبب. المدير التنفيذي ومدير الموارد ومسؤول النظام ما تنفتحلهم سنة (بلا إجازة).

- **العقد:** قسم `annualLeave` (hrManager) و`SystemSettings_GetAnnualLeave`/`SystemSettings_UpdateAnnualLeave` على `/api/app/system-settings/annual-leave` (`SystemSettingsAnnualLeaveDto` و`...UpdateInputDto`، 0–365، 409 للـ stamp القديم)، صلاحية `Hr.Settings.AnnualLeave`. **انشال:** `LeaveBalance_SetEntitlement` و`LeaveEntitlementUpdateDto` وصلاحية `Hr.Leave.Balance.SetEntitlement`. أوصاف `isEntitlementSet` و`currentYearEntitlementDays` و`nextYearEntitlementDays` و`Employee_Create` تحدثت. المجموع 328 عملية.
- **الباك:** `AnnualLeaveSettings` بـ `HrSettingValues.cs`. `LeaveBalanceManager.RollOverAsync` يفتح أول سنة بالافتراضي إذا ماكو سنة (لمن عنده إجازة: فعّال، مباشر، مو من الأدوار الثلاثة)، ويمنح كل سنة جديدة بالافتراضي وقت بدايتها. `LeaveBalanceSnapshot.NextYearEntitlement` صار الافتراضي الحالي. عامل الإجازات (كل ساعة) يفتح سنة كل موظف فعّال بلا سنة. عمود `NextYearEntitlementDays` بقى بالجدول بلا استخدام (بلا migration). D15 (التنفيذي يحدد رصيد مدير الموارد) انتهى مع العملية.
- **الفرونت:** بطاقة «الإجازة السنوية» بإعدادات النظام لمدير الموارد. زر «تحديد الاستحقاق» انشال من «أرصدة الإجازات» (يبقى «تعديل الرصيد»)، وعنصر القائمة صار على `LeaveBalance_Adjust`.
- **الاختبارات:** مساعد مشترك `AnnualLeaveDefault` (يضبط الافتراضي ثم يقرا الرصيد فتنفتح السنة بيه) بدل التحديد اليدوي بأربع ملفات. اختباران جديدان بـ `LeaveModuleTests`: سنة الموظف الجديد تنفتح بالافتراضي وتقبل إجازة، وتغيير الافتراضي يوصل السنة الجاية بس (و409 و403 و400). مثال الوثيقة (6 + 21 = 27) صار يغيّر الافتراضي بدل التحديد.

## تعديل 2026-11-10: إخفاء «الأدوار والصلاحيات» عن مدير الموارد (قرار زينب) — فرونت فقط
- مدير الموارد اللي مو مسؤول نظام (`Session.isHrWithoutSystemAdmin`) ما يشوف `/roles` بالقائمة، وإذا فتح الرابط يرجع للرئيسية (`hrManagerHiddenPaths` بـ `shell.dart` والـ redirect بـ `router.dart`).
- **صلاحية الخادم ما تغيرت:** `Hr.Identity.Role.Read` تبقى لمدير الموارد لأن منتقي الأدوار بنموذج إضافة موظف والحسابات يقرا `Role_List`. العقد ما تغير (`x-allowed-roles` لـ `Role_List` بيه `hrManager`). إذا تريدين سحب الصلاحية نفسها، يحتاج تعديل العقد وبديل لمنتقي الأدوار.

## إضافة 2026-11-10: تقييم الموارد البشرية (قرار زينب) — باك ✅ فرونت ✅
**القرارات:** تقييم منفصل عن التقييم الشهري لمدير القسم. مدير الموارد يختار موظف ويعطي درجة من 1 لـ 10 لكل عنوان من قائمة عناوين خاصة بيه (يضيفها بصفحة)، مع ملاحظات. بأي وقت وبدون حد. الموظف ما يشوفه. مدير الموارد يرسل التقييمات غير المرسلة (أسبوعياً) بتقرير للمدير التنفيذي، والتنفيذي يشوف كل شي بيها الملاحظات.

- **العقد:** 9 عمليات (المجموع 327): `HrEvaluationCriterion_GetList/Create/Update`، `HrEvaluation_GetList/Create/Get`، `HrEvaluationReport_GetList/Send/Get`. Tag `HrEvaluations`، schemas `HrEvaluation*Dto`، صلاحيتان `Hr.HrEvaluation.Manage` (الموارد) و`Hr.HrEvaluation.ReadReports` (الموارد والتنفيذي)، أكواد `Hr:HrEvaluation:ScoresIncomplete/CriterionNotFound/EmployeeNotAllowed/NothingToSend`، إشعار `hrEvaluationReportSent` ونوع مورد `hrEvaluationReport`.
- **القواعد:** درجة صحيحة 1–10 لكل عنوان فعّال مرة وحدة. العناوين ما تنحذف، تتعطّل، والتقييمات القديمة تحتفظ بأسمائها. ما يتقيّم: غير الفعّال، المقيّم نفسه، المدير التنفيذي، مسؤول النظام بلا دور عمل. التقييم ما يتعدل. المعدل يحسبه الخادم (منزلتين). الإرسال يجمع كل غير المرسل للمرسل نفسه، والتنفيذي يشوف كل التقارير، ومدير الموارد يشوف تقاريره بس (غيرها 404).
- **الباك:** `Domain/HrEvaluations/HrEvaluationEntities.cs` (4 كيانات، `[Audited]`)، `Application/HrEvaluations/HrEvaluationAppServices.cs`، `HttpApi/Controllers/HrEvaluations/HrEvaluationControllers.cs`، migration `Hr_HrEvaluations` (4 جداول).
- **الفرونت (موبايل وويب):** `features/hr_evaluations/hr_evaluations_page.dart`: «عناوين تقييم الموارد» `/hr-evaluation-criteria`، «تقييم الموظفين» `/hr-evaluations` (نافذة تقييم: الموظف، قائمة 1–10 لكل عنوان، الملاحظات؛ وزر «إرسال للمدير التنفيذي»)، «تقارير تقييم الموارد» `/hr-evaluation-reports` وتفاصيله. الإشعار يفتح التقرير.
- **ملف الموظف (فرونت فقط):** تبويب «التقييمات» أول تبويبات السجل (`employee_records_tab.dart`، لمن عنده `HrEvaluation_GetList`): تقييمات هذا الموظف من `HrEvaluation_GetList?employeeId`، وزر «إضافة تقييم» يفتح نفس النافذة والموظف ثابت (`showHrEvaluationDialog` صارت دالة مشتركة). ما يطلع بصفحة عضو الفريق لمدير القسم. بلا تغيير بالعقد أو الباك.
- **انختبر:** `HrEvaluationModuleTests` (403 للموظف والتنفيذي على التقييم، العناوين و409، النقص والتكرار والمجهول والمعطّل، رفض التنفيذي ومسؤول النظام والمجهول، 400 للدرجة 0 و11، المعدل 7.5، الموظف ما يوصله شي، الإرسال والـ NothingToSend، إشعار التنفيذي وقراءته للملاحظات، و404 لمدير موارد ثاني). العقد وSwagger تمر. الـ DbMigrator طبّق الـ migration ومنح الصلاحيات على قاعدة التطوير. `flutter analyze` نظيف. الواجهة ما انشافت فعلياً.

## إضافة 2026-11-10: فترة بتقرير أرصدة الإجازات (قرار زينب) — باك ✅ فرونت ✅
- **المعنى:** الأرصدة تبقى للسنة الحالية، والفترة الاختيارية تضيف عمودين: `periodUsedDays` (أيام الإجازة السنوية المعتمدة داخل الفترة؛ الإجازة الكاملة داخلها تنحسب بأيامها الثابتة وقت التقديم، واللي تعبر حد الفترة تنحسب أيام دوامها داخل الفترة من جدول الدوام) و`periodUsedHours` (ساعات الإجازة الزمنية المعتمدة بتاريخ داخل الفترة). الإجازات الأخرى (مرضية...) ما تنحسب لأنها ما تمس الرصيد السنوي. المعلقة ما تنحسب.
- **العقد:** `Report_GetHrLeaveBalances` صار بيه `from` و`to` اختياريين، وكود `Hr:Reports:InvalidPeriod` (أحدهما بس أو `to` قبل `from`)، والعمودان nullable بالصف. وصف `Report_Export` يذكر إن الفترة اختيارية لهذا الرمز.
- **الباك:** `ReportQueries.GetPeriodUsageAsync` (يستخدم `EmployeeScheduleReader`)، و`ReportAppService.OptionalPeriod`، والتصدير يضيف العمودين وعنوان الفترة لما تنرسل.
- **الفرونت:** حقلا «من» و«إلى» بصفحة التقرير فاضيين بالبداية، واختيار أي تاريخ يفعّل الفترة، والمسح يلغيها. العمودان يطلعان `—` بدون فترة. التصدير يرسل الفترة لما تكون مفعلة.
- **انختبر:** `ReportModuleTests.Hr_leave_balances_agree_with_the_employees_own_balance` توسّع: null بدون فترة، 2 لإجازة داخل الشهر، 1 لفترة تقطع الإجازة، 0 لشهر قبلها، المعلقة ما تنحسب، 422 لتاريخ واحد أو مقلوب، والتصدير 200. كل `ReportModuleTests` والعقد تمر. `flutter analyze` نظيف. الواجهة ما انشافت فعلياً.

## إضافة 2026-11-10: مرفقات العقود (قرار زينب) — باك ✅ فرونت ✅
**القرارات:** مدير الموارد يرفق نسخة العقد الورقي مع إنشاء العقد (بنفس الطلب، فما ينضاف مرفق لعقد موجود)، الموظف يشوفه وينزله بـ«عقودي»، والملف الغلط ينحذف حذف ناعم ويتسجل.

- **العقد:** `Contract_Create` صار `multipart/form-data` (تغيير كاسر لشكل الطلب) مع `attachments` (حتى 10، binary) وأكواد `Hr:Contract:AttachmentRejected` و`Hr:Document:FileTooLarge` و`Hr:Document:FileTypeNotAllowed`. `ContractDto.attachments` و`ContractEmployeeDto.attachments` (`AttachmentDto[]` إلزامي). 3 عمليات جديدة: `Contract_GetAttachmentDownloadUrl` (الموارد، `Hr.Contract.Read`)، `Contract_DeleteAttachment` (‏204، `Hr.Contract.Create`)، `Contract_GetMyAttachmentDownloadUrl` (الموظف لعقده، `Hr.Contract.ReadOwn`). المجموع 318 عملية.
- **الباك:** كيان `ContractAttachment` (`[Audited]`، `ISoftDelete` مع `DeletedAt` و`DeletedByUserId`) بجدول `AppContractAttachments` بدون تغيير على `EmploymentContract`. `ContractCreateInput` بالخدمة و`ContractCreateInputDto` (form) بالـ controller مثل الإنذارات. نفس `UploadedFileGuard` وحاوية الملفات وروابط التنزيل. Migration `Hr_ContractAttachments`.
- **الفرونت (موبايل وويب):** نافذة «عقد جديد» فيها «إرفاق العقد الورقي» (حتى 10)، تفاصيل العقد فيها بطاقة «مرفقات العقد» بتنزيل وحذف بتأكيد، و«عقودي» فيها زر تنزيل لكل مرفق.
- **انختبر:** `EmploymentRecordsModuleTests.Contract_attachments_are_uploaded_on_create_shown_downloaded_and_soft_deleted` (رفض الملف يوقف إنشاء العقد، العرض، التنزيل، 404 لمرفق عقد ثاني أو مجهول، 404 للموظف الثاني، 403 للموظف على مسارات الموارد، الحذف 204 ثم 404، اختفاؤه من كل العروض، وبقاء الصف معلّم محذوف)، واختبارات العقود القديمة صارت multipart وتمر. `ContractRouteTests` و`SwaggerContractReportTests` تمر. الـ migration انطبق على قاعدة التطوير. `flutter analyze` نظيف.
- **ما انختبر:** الواجهة فعلياً بالمتصفح والموبايل.

## اختبارات فاشلة من قبل (بالتشغيل الكامل 2026-11-10، مو من التبليغات ولا المرفقات)
382 ناجح و3 فاشلة، والثلاثة تخص تعديلات الصبح (مسؤول النظام والموارد بلا دوام) أو هشاشة قديمة، وما لمستها:
- `EmployeeModuleTests.Create_enforces_the_contract_rules` السطر 122: يقرا `roleNames` من `EmployeeDto` وهالحقل مو موجود بالرد (`KeyNotFoundException`).
- `MemoIncidentModuleTests.The_hr_managers_leave_memo_starts_at_the_executive_when_converted`: يحدد رصيد إجازة لمدير الموارد، وهو صار بلا إجازة ولا رصيد، فيرجع 422.
- `ReviewReportModuleTests.The_reviewer_writes_a_team_report_and_the_executive_reads_it_once_submitted`: يقارن `viewedAt` كنص، والقاعدة تقص لأجزاء المايكروثانية (`...4305374Z` مقابل `...430537Z`). هشاشة بالاختبار مو بالكود.

## إضافة 2026-11-10: التبليغات (قرار زينب) — باك ✅ فرونت ✅
**القرارات:** صفحة «إرسال تبليغ» لمدير الموارد البشرية والمدير التنفيذي، يختار موظف أو أكثر بالاسم ويكتب العنوان والنص. الموظف يشوفه بصفحة مستقلة «تبليغاتي» ويأكد الاطلاع، والمرسل يشوف منو اطّلع.

- **العقد أولاً:** 6 عمليات جديدة (المجموع 315): `Notice_GetReceivedList` و`Notice_Create` و`Notice_GetSentList` و`Notice_Get` و`Notice_GetReceipts` و`Notice_Acknowledge`، Tag `Notices`، schemas `Notice*Dto`، صلاحيتان `Hr.Notices.Read` (كل الأدوار) و`Hr.Notices.Send` (الموارد والتنفيذي)، كود `Hr:Notices:RecipientNotAllowed`، وقيم جديدة: `InboxCategory.noticeAcknowledgement`، `AdministrativeTaskType.noticeSent`، `NotificationTemplateCode.noticeReceived`، `NotificationResourceType.notice`. `x-release: 2`.
- **القواعد:** المستلم موظف فعّال له حساب، مو المرسل، ومو مسؤول نظام بلا دور عمل. أي مستلم غير صالح يرفض الطلب كله (422) وما ينرسل لأحد. 1 إلى 200 مستلم بدون تكرار (400). ما يتعدل ولا ينحذف، والمستلمون يتثبتون وقت الإرسال. إشعار لكل مستلم، وفئة بـ«بانتظار إجرائي» لحد التأكيد، وسجل المهام (`noticeSent`) والتدقيق (`[Audited]`). `Notice_Get` يرد 404 لغير المستلم حتى لو هو المرسل، والإيصالات 404 لغير المرسل.
- **الباك:** `Domain/Notices/NoticeEntities.cs`، `Application.Contracts/Notices/NoticeDtos.cs`، `Application/Notices/NoticeAppService.cs` (مع `NoticeCreateDtoValidator` و`NoticeInboxSummaryContributor`)، `HttpApi/Controllers/Notices/NoticeController.cs`، جدولا `AppNotices` و`AppNoticeRecipients` (DbSet + `ConfigureNotices`)، migration `Hr_Notices`، ونصوص ar/en.
- **الفرونت (موبايل وويب):** `features/notices/notices_page.dart`: «تبليغاتي» `/notices` وتفاصيله `/notices/:id` بزر «اطّلعت»، و«إرسال تبليغ» `/send-notice` (اختيار الموظفين عبر `Employee_Lookup`، والقائمة المرسلة مع `اطّلع/المستلمون` ونافذة الإيصالات). القائمة الجانبية، وفتح الإشعار، وبطاقة «بانتظار إجرائي»، ورسالة الخطأ. `/notices` مخفية عن مسؤول النظام بلا دور عمل. الملفات المولدة (`models.dart`، `operations.dart`، `notices_api.dart`، `hr_api.dart`) تحدثت يدوياً لأن Python مو منصّب.
- **انختبر:** `NoticeModuleTests` (2): الصلاحيات (403 للموظف ومدير القسم)، الإرسال والـ Idempotency-Key، 404 لغير المستلم وللمرسل كقارئ، الإشعار وفئة الصندوق، التأكيد المكرر، الإيصالات وفلترها و404 لغير المرسل، القائمة المرسلة، إرسال التنفيذي، رفض مسؤول النظام والمعرّف المجهول والملف بلا حساب بدون إرسال شي، و400 للقائمة الفارغة والمكررة والعنوان الفارغ والنص الطويل. `ContractRouteTests` و`ErrorContractTests` و`SwaggerContractReportTests` تمر. الـ migration انطبق على قاعدة التطوير (وطبّق وياه `Hr_ReviewReports` اللي كان معلق عليها من 2026-11-07). `flutter analyze` نظيف.
- **ما انختبر:** الواجهة بالمتصفح والموبايل فعلياً. وقاعدة التطوير: الـ DbMigrator فشل بكلمة سر قديمة بـ `src/HR_Mangment2.DbMigrator/appsettings.secrets.json`، فالـ migration انطبق بـ `dotnet ef database update --connection` من إعدادات الـ Host، ومنح الصلاحيات الجديدة للأدوار يعتمد على زرع الـ Host عند التشغيل.

## تعديل 2026-11-10: واجهات مدير الموارد البشرية (قرار زينب)
- **ملف الموظف:** تبويب «السجل الوظيفي» انقسم لتبويبات مستقلة جنب «الوثائق»: العقود، الإنذارات، كتب الشكر، العهد، الشكاوى (كل وحدة حسب صلاحيتها، `employeeRecordTabs` بـ `employee_records_tab.dart`). صفحة عضو الفريق (`team_page.dart`) نفس الشي بدون الشكاوى. **الشكاوى:** `Complaint_GetAssignedList` صار يقبل `complainantEmployeeId` (العقد + `ComplaintAssignedListInput` + `ComplaintAppService` + `complaints_api.dart`)، والفلترة داخل `ApplyHandlerScope` نفسه، فمدير الموارد يشوف بس شكاوى الموظف اللي ضمن نطاقه (normal اللي وصلته وconfidential)، وفتح الشكوى يتسجل بسجل الاطلاع كالعادة. ⏳ ما انضاف اختبار للفلتر.
- **مدير القسم مديره المباشر مدير الموارد البشرية (مو مسؤول النظام):** صف `departmentManager` بـ manager-link صار `hrManager`. طلبات مدير القسم (إجازة، تصحيح بصمة، مخزن، محاسب، احتياج إداري) ومطالعاته تبدأ من `pendingHr` مباشرة، مثل المراجع (خطوة وحدة للموارد). `RequestRoutePlanner` و`MemoDomainServices` و`ManagerLinkService` تحدثت. `Manager_GetMyTeam` و`Manager_GetTeamMember` و`Hr.Employee.ReadTeam` صارت لـ `departmentManager` و`hrManager` بدل `systemAdmin`، والزرع يسحب `ReadTeam` من مسؤول النظام. الزرع ينقل أي موظف مرتبط بمسؤول النظام إلى ملف مدير الموارد الفعال (`MoveSystemAdminReportsToHrManagerAsync`)، والطلبات والمطالعات المعلقة عند مسؤول النظام تنتقل وياهم بخطوة المدير القديمة. العقد: المصفوفة (بمكانين)، القاعدة الملزمة، وصف الإجازة وتصحيح البصمة والمطالعة وفريقي. الاختبارات: `EmployeeTestKit.HrManagerEmployeeId` و`AddDepartmentManagerAsync` يربط بيه، وتحدثت اختبارات المصفوفة والطلب والإجازة والمطالعة والربط. ملاحظة: نص `Request:Status:pendingSystemAdmin` و`Memo:Status:pendingSystemAdmin` بقى للعناصر القديمة بس.
- **الراتب المتفق عليه بإضافة موظف (اختياري):** `EmployeeCreateDto` صار بيه `baseSalary` و`salaryCurrency` (الأخير مع الراتب بس، وإذا غاب يستخدم `Finance:DefaultCurrency` أو IQD). الخادم يحوّله لأول نسخة بالملف المالي (`SalaryProfile`) بتاريخ سريان `hireDate` وبلا مخصصات، عن طريق `IInitialSalaryGateway` (بالمشروع الرئيسي، وتنفيذه `FinanceInitialSalaryGateway` بموديول المالية، مثل `IStockExpenseGateway`). إذا شهر المباشرة له كشف معتمد أو مغلق يرجع `Hr:Finance:EffectiveDateInLockedPeriod` وينلغي الإنشاء كله. الراتب ما يرجع بأي استجابة للموارد، وعليه `[DisableAuditing]` حتى ما يدخل سجل التدقيق. العقد والـ models.dart والـ operations.dart تحدثت يدوياً. الفرونت: حقل «الراتب المتفق عليه (اختياري)» بعد المسمى الوظيفي. اختبار: `EmployeeModuleTests.The_agreed_salary_becomes_the_first_salary_version_for_the_accountant`. سجل التدقيق المخصص (`HrAuditEntryBuilder`) يخزن أسماء الحقول المتغيرة بس، مو القيم.
- **صفحة الموظفين:** `Employee_List` ما يدرج ملفات المدير التنفيذي ومدير الموارد ومسؤول النظام (أي حامل لهالأدوار)، حتى لو انطلب `roleName`. تبقى متاحة بـ `Employee_Get` وبـ `Employee_Lookup` (منتقي المدير). العقد يذكرها. `EmployeeTestKit` صار يجيب الملفات المزروعة من قاعدة البيانات بدل البحث بالقائمة (`FindSeededEmployeeIdAsync`)، واختبار بـ `EmployeeModuleTests` يتأكد إن القائمة ما بيها هالأدوار.
- **الرئيسية (فرونت فقط، `features/home/home_page.dart`):** بطاقات بصفوف من ثلاثة بنفس العرض والارتفاع (220) من عرض 700 فما فوق، وعلى الموبايل فوق بعض. الصف الأول: «بانتظار إجرائي» (قائمته تتمرر داخل البطاقة)، «غير مقروءة»، «الشكاوى الواصلة» (لمن عنده `Complaint_GetAssignedList`: المفتوحة = الكل ناقص `closed`، والمتأخرة من `isLate=true`، أعداد بس). الصف الثاني لحضور اليوم: «الحاضرون اليوم» (منهم المتأخرين)، «الغائبون اليوم»، «بإجازة اليوم»، لمدير الموارد من `Report_GetHrAttendance` لكل الشركة (صفحات 200)، ولمدير القسم من `Attendance_GetTeam` لفريقه. بعدها بطاقات الحضور والإجازة الشخصية. بلا تغيير بالعقد أو الباك. ⏳ ما انشافت بالمتصفح ولا بالموبايل.

## تعديل 2026-11-10: تنظيف واجهة مسؤول النظام (قرار زينب)
- **دور employee:** مسؤول النظام اللي ما عنده دور ثاني ما يحمل `employee` (مو موظف بالشركة). `AccountProvisioner.ResolveRolesAsync` يشيله بالإنشاء وتغيير الأدوار، والزرع ما يعطيه للحساب `sysadmin` ويشيله من القاعدة الموجودة (`RemoveEmployeeRoleFromSystemAdminsAsync`). يبقى عنده ملف موظف لأن مديري الأقسام مرتبطين بيه. يخسر العمليات الذاتية للموظف (شكاوى، تقييمي، عقودي، إنذاراتي، كتب الشكر، قسائمي)، والعقد يذكر الاستثناء. اختبار: `EmployeeModuleTests` يتأكد إن أدوار المزروع `systemAdmin` بس. إذا انطى دور عمل ثاني (مثلاً مدير قسم) يرجعله `employee`.
- **التعاميم (قرار زينب النهائي 2026-11-10، بعد إلغاء ورجوع بنفس اليوم):** مسؤول النظام بلا دور عمل (أدواره `systemAdmin` و`employee` فقط، `AccountProvisioner.IsSystemAdminOnly`) ما يستلم أي تعميم من أي نطاق (`CircularAppService.ResolveRecipientsAsync`)، لأنه مو موظف بالشركة. إذا عنده دور عمل ثاني (مثلاً مدير قسم) يستلمه. وصف `CircularScope` بالعقد يذكرها، وصفحة `/circulars` مخفية عنه بالموبايل والويب (`systemAdminHiddenPaths`). التعاميم اللي وصلته قبل تبقى بقاعدة البيانات. ⏳ ما انضاف اختبار لها بعد.
- **فرونت:** مسؤول النظام بدون دور عمل (`isSystemAdminOnly`) ما يشوف: «طلباتي» (`/requests` و`/requests/new` فقط، وتفاصيل الطلب `/requests/:id` تبقى مفتوحة من الوارد والإشعارات)، «التفويض» `/delegations`، «قوالب الدوام» `/shifts`، «فريقي» `/team`، «معايير التقييم» `/evaluation-admin`، «التعاميم» `/circulars` مع تفاصيلها (و`/evaluations` مخفية من قبل). وزر «طلب جديد» بالرئيسية ينشال له. الملفات: `app/shell.dart`، `features/home/home_page.dart`.
- **العقد + الباك:** إدارة معايير التقييم والسلم اللفظي (`EvaluationCriterion_*` و`EvaluationGrade_*`) انتقلت من `systemAdmin` إلى `hrManager`: `x-allowed-roles` بالعقد، وقسم `evaluationCriteria` بـ `SystemSettingsSectionCode`، و`HrPermissions.DefaultRoles`، و`SystemSettingsAppService.Sections`، و`operations.dart`. و`HrTenantBootstrapper` يسحب المنحتين القديمتين من دور `systemAdmin` بقاعدة البيانات الموجودة (`RevokedDefaults`).
- **اختبار:** `EvaluationModuleTests.The_scale_starts_with_five_of_each_and_keeps_its_minimums` صار بمدير الموارد، ويتأكد إن مسؤول النظام يرجع 403.
- ⏳ يحتاج: `dotnet build` + `dotnet test` + `flutter analyze`.
- ملاحظة: الإخفاء بالواجهة فقط لطلباتي والتفويض والدوام وفريقي؛ صلاحيات الخادم لهذي ما تغيرت (العقد يسمح لمسؤول النظام بيها).

## تعديل 2026-11-10: مسؤول النظام بلا مدير مباشر ولا دوام (قرار زينب)
- **تحديث ثالث بنفس اليوم:** **مدير الموارد البشرية كذلك بلا دوام**، ومثل التنفيذي بالحضور والإجازات (لا حضور ولا إجازة ولا رصيد)، **ويبقى مديره المدير التنفيذي**. فانفصلت القاعدتان: `AllowedManagerRoles` (من بلا مدير: التنفيذي ومسؤول النظام) و`IsOutsideSchedule` (من بلا دوام: التنفيذي ومدير الموارد ومسؤول النظام). `EmployeeAppService.CreateAsync` يفحص الدوام والمدير كل واحد لحاله، و`DetachFromScheduleAsync(employee, roles, today)` يغلق الربط بس إذا الأدوار بلا مدير ويغلق الدوام إذا بلا دوام. الزرع ينشئ «الدوام الافتراضي» حتى لو ما أحد من الثلاثة ياخذه، ويغلق دوام مدير الموارد ومسؤول النظام بالقاعدة الموجودة. `GetTargetForChangeAsync` يفحص «ما أحد يعدل رصيده» قبل الأهلية. `EmployeeTestKit.ShiftId` صار يُقرأ مباشرة من «Default Shift». الفرونت: `Session.isOutsideSchedule` يخفي «إجازاتي» و«حضوري» وبطاقتيهما وزر الإجازة الجديدة بالرئيسية للثلاثة. ملاحظة: D15 (التنفيذي يعدّل رصيد مدير الموارد فقط) صار بلا فائدة لأن مدير الموارد بلا رصيد.
- **تحديث لاحق بنفس اليوم:** مسؤول النظام كذلك **بلا دوام**، ويعامَل مثل المدير التنفيذي بالضبط: غير مشمول بالحضور (لا بصمة ولا حساب أيام ولا تصحيح) ولا إجازة ولا رصيد له. القاعدة بالكود صارت `ManagerLinkService.IsOutsideSchedule(roles)` = صف manager-link بلا مدير، وتعوّض كل فحوصات `Contains(ExecutiveDirector)` بالحضور والدوام والإجازات (`AttendanceAppService`، `AttendanceDeviceAppService`، `AttendanceDayCalculator`، `EmployeeShiftAppService`، `LeaveDomainServices`، `LeaveBalanceAppService`). الكودان `Hr:Attendance:ExecutiveDirectorNotCovered` و`Hr:Leave:ExecutiveDirectorNotEligible` نفسهما ومعناهما يشمل الاثنين. `DetachFromScheduleAsync` يغلق الربط والدوام الحاليين، ويستخدمه الزرع (لقاعدة البيانات الموجودة) وتغيير الأدوار. شاشة إضافة موظف تخفي الدوام والمدير. `EmployeeTestKit.ShiftId` صار يُقرأ من مدير الموارد المزروع.
- **القاعدة (الأولى):** المدير التنفيذي ومسؤول النظام ما عندهم مدير مباشر. صف `systemAdmin` بـ `x-global-rules.manager-link` صار `managerMustHoldRole: null`، ومن يجمع `hrManager` معه يتبع صف الموارد. أي ربط يرجع `Hr:Employee:ExecutiveCannotHaveManager` (نفس الكود، ومعناه صار يشمل الاثنين). ~~مسؤول النظام يبقى عنده دوام~~ (تغيّر، شوف التحديث أعلاه).
- **ما يتأثر:** مسارات الموافقة لطلبات ومطالعات مسؤول النظام تمشي بالدور (`pendingExecutive`) مو بالربط، فما تتغير. ومديرو الأقسام يبقون مرتبطين بمسؤول النظام.
- **العقد:** صف manager-link، ووصف `Employee_Create` و`Employee_ChangeManager` و`managerEmployeeId`، ومعنى `Hr:Employee:ManagerRequired`.
- **الباك:** `ManagerLinkService.AllowedManagerRoles`، و`EmployeeAppService.CreateAsync` (المدير مو مطلوب لمسؤول النظام والدوام مطلوب)، و`HrTenantBootstrapper`: يزرع مسؤول النظام بلا مدير، ويغلق ربطه القديم بالتنفيذي بقاعدة البيانات الموجودة (`CloseLinksOfUnmanagedRolesAsync`). وتغيير الأدوار عبر الحسابات كان يغلق الربط تلقائياً لما يصير الدور بلا مدير.
- **الفرونت:** شاشة إضافة موظف تخفي حقل المدير لمسؤول النظام، ونص قواعد الربط ورسالة الخطأ تحدثت.
- **اختبارات:** `RequestRoutePlannerTests` (صف مسؤول النظام صار null)، `EmployeeModuleTests` (الإنشاء وتغيير المدير لمسؤول النظام يُرفضان، والمزروع بلا مدير).
- ⏳ يحتاج: `dotnet build` + `dotnet test` + `flutter analyze`. وتحديث `متطلبات_وتصميم_النظام.md` القسم 4 إذا يذكر ربط مسؤول النظام بالتنفيذي.

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
