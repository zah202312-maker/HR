# Coverage Audit: API Contract vs Requirements

Audited: `api-contract/openapi.yaml` (308 operations, 496 schemas), against `متطلبات_وتصميم_النظام.md`, `CONVENTIONS.md` and `open-questions.json`. The contract, its modules and the document were not edited.

## Summary

| Measure | Count |
|---|---|
| Findings, total | 31 |
| CRITICAL | 3 |
| MAJOR | 8 |
| MINOR | 20 |
| Open questions, total | 72 |
| ANSWERED | 11 |
| NEEDS-OWNER | 61 |

### Known cross-module issues

- **`fingerprint` vs `device`:** the value conflict is no longer in the schemas. Both attendance setting routes use `AttendanceCaptureMethod` (`device|gps|both`), and `AttendancePunchSource` is `device|file|gps`, matching section 9.1. The word `fingerprint` survives only in the stale `x-open-question` on `AttendanceSettings_Get`. The two write routes remain (COV-12).
- **`gender`:** confirmed. The string `gender` has 0 hits in `openapi.yaml`, while `OtherLeaveEligibleCategory` offers `male` and `female` (COV-3).

### What was checked

| Check | Method | Result |
|---|---|---|
| Section 10 routes (136 method and path pairs) | Script | 134 match literally. `GET, PUT system-settings/{section}` is implemented as 6 concrete sections. |
| Added routes (162) and allowed roles | Read all 308 descriptions | COV-5, 6, 11, 25, 26, 27, 28 |
| `x-source` | Script against the document's section numbers | 0 invalid numbers, 0 empty. Accuracy issues: COV-18, COV-19 |
| `x-release` | Script by tag, then read by hand | COV-10, 13 to 17 |
| Approval matrix (15 cells) | Row by row | All 15 correct in `modules/workflow-store.yaml`, but absent from `openapi.yaml` (COV-1) |
| Complaint routing (10 rows) | Row by row against the `Complaint_Submit` table | All 10 correct. Access gaps: COV-2, COV-6 |
| Leave rules (9 rows) and 3 leave kinds | Row by row | All present. One row only implied (COV-31) |
| Store and custody flow | Step by step | Complete: acknowledge, purchasing, cost to expenses, deduct on delivery, custody, receipt |
| Cycle timing (4 rows) | Row by row against `EvaluationCycleDto` and reminders settings | Defaults correct: 10:00, 3 days, daily, day 4. Snapshot issue: COV-8 |
| Contract and warning flows | Step by step | Complete, apart from COV-24 |
| Settings ownership in 5.18 (13 items) | Item by item | All have the stated owner. Shifts also allow `systemAdmin`, because 5.2 and 5.18 disagree (NEEDS-OWNER #11) |
| Reports in 5.11 (18 reports) | Report by report | All have a route. Release issues: COV-10, COV-15 |
| Features deferred in 5.19 | Search | GPS is `later`. No routes for team channel, AI assistant or SMS, as expected |

### Limits of this audit

- Business rules live in description text, so this checks what the contract states, not how a backend would behave.
- All 308 operations were read in summary form (roles, scope, sources, request and response names, error codes, descriptions). About 70 of the 496 schemas were read field by field, chosen for the rules under audit. The rest were not reviewed individually.
- Examples and mechanical `CONVENTIONS.md` rules (required headers, response lists) were not audited.
- All `x-source` numbers were checked for existence. Whether each cited section justifies its operation was judged by reading the operation, not proved citation by citation.
- `api-contract/README.md` was absent from the folder when checked.

## Findings

| ID | Severity | Document section | operationId or schema | Problem | Exact fix |
|---|---|---|---|---|---|
| COV-1 | CRITICAL | 4 | `Request_Submit`, `Request_Approve`, `info.description` | `Request_Submit` says the steps come from the matrix "(انظر info.description)", but merging replaced `info.description` with one generic line. The store and accountant columns (10 of 15 cells), and the binding rules under the matrix, exist only in `modules/workflow-store.yaml`. A reader of `openapi.yaml` cannot find the routing for store or accountant requests. | Copy the matrix and its binding rules from `modules/workflow-store.yaml` into the `Request_Submit` description and merged `info.description`. Make the merge step keep them. |
| COV-2 | CRITICAL | 3 (protected rule 4), 7 | `UserAccount_SetRoles`, `Complaint_GetAssignedList`, `Complaint_GetDetail` | Rule 4 requires executive approval to appoint a complaint handler. `UserAccount_SetRoles` lets `systemAdmin` replace any role set, and no error code protects `hrManager` or `executiveDirector`. The singleton check only blocks a second active holder, so a system admin can remove the current holder and give the role to someone else. `Complaint_GetAssignedList` then gives the `hrManager` holder normal and confidential complaints by role. The identity text says the opposite: granting roles gives no complaint access, because access comes from the handler table. No seed row for the default handlers is defined. Either the new holder gains access without approval, or the default handlers have none. | In `UserAccount_SetRoles`, `UserAccount_Create` and `Employee_Create`: only `executiveDirector` may change who holds `hrManager`; add `Hr:Identity:ProtectedRoleChange` (422) for anyone else, and define how the first holders are seeded. Bar `systemAdmin` from changing `executiveDirector`. Reword the identity text: role grants give no access except the default routing in 5.8, and a change of `hrManager` needs executive approval and goes to the audit log. |
| COV-3 | CRITICAL | 5.4 | `OtherLeaveType_Create`, `OtherLeaveEligibleCategory`, `EmployeeCreateDto`, `EmployeeHrDto`, `EmployeeSelfDto` | Eligibility values `male` and `female` cannot be evaluated, because no employee schema has a gender field. `Hr:Leave:NotEligibleForLeaveType` and `OtherLeaveType_ListAvailable` cannot apply the rule. | After the owner answers NEEDS-OWNER #19: add `gender` (enum `male|female`) to `EmployeeCreateDto`, `EmployeeUpdateDto`, `EmployeeHrDto` and `EmployeeSelfDto`, and keep it absent from `EmployeeTeamMemberDto` and `EmployeeRefDto`. |
| COV-4 | MAJOR | 3 | `Employee_Create`, `Employee_ChangeManager`, `Manager_AssignEmployees`, `MyAccountDto` | Five references point to `x-global-rules`, which is absent from `openapi.yaml` and exists only in `modules/identity-org.yaml`. Lost with it: the manager eligibility table, the allowed operation lists for the password and MFA access gates, and error meanings. Other modules' `info.description` rules were dropped too, including `x-data-scope-by-role`, complaint rules, and the note that store types stay off before release 2. | Have the merge copy root `x-global-rules` into `openapi.yaml`, and keep each module's rule text under a named root extension such as `x-module-rules`. |
| COV-5 | MAJOR | 3 (rule 1), 4 | `Request_Approve`, `Request_GetInbox`, `Request_Reject` | The contract contradicts itself. The `x-open-question` on `Request_Approve` assumes the accountant and store officer may carry out their own request, but its description bans anyone approving their own request, and the inbox never shows it. The accountant finishes a request only through `Request_Approve`, so the accountant's own request stops at `pendingDestination` permanently, with no delegate available (COV-11). | After the owner answers NEEDS-OWNER #22: either allow execution of an accountant's own request at `pendingDestination`, recorded as execution and shown in the inbox, or route it to a named alternate. State the exception in `Request_Approve` and `Request_GetInbox`. |
| COV-6 | MAJOR | 3, 5.8 | `ComplaintHandler_Request`, `ComplaintHandler_Approve`, `ComplaintHandlerAssignmentCreateDto` | The only role check is `HandlerCannotBeSystemAdmin`. An approved assignment works "أيًا كانت أدواره", so these become possible: a reviewer as handler (section 3 limits the reviewer to counts, and the dashboard text says the reviewer has no handler route), a department manager at confidential level (forbidden by 5.8 and section 3), and the executive at normal level (5.8: does not see normal complaints "حتى كأعداد"). | Add `Hr:Complaints:HandlerRoleNotAllowed` to both operations. Reject reviewers at all levels, `departmentManager` at `confidential` or above, and `executiveDirector` at `normal`. Re-check at each access, as already done for system admins. |
| COV-7 | MAJOR | 5.1 | `Memo_GetInbox`, `Employee_Get` | 5.1 lists memos among the employee file records, and `Employee_Get` says records come from their modules by `employeeId`. No memo route accepts an employee: `Memo_GetInbox` filters only `status` and `kind`, and `Memo_GetMy` is self only. | Add `GET /api/app/memo/employee/{id}` for `hrManager` and the direct manager, with `x-added-route`, or add an `employeeId` filter to `Memo_GetInbox` that returns all statuses for `hrManager`. |
| COV-8 | MAJOR | 5.10 | `DepartmentReport_Get`, `DepartmentReportSnapshotDto`, `DepartmentReportEvaluationSummaryDto`, `DepartmentReportEmployeeRowDto` | The snapshot is fixed on the first working day ("ثابتة بعد التثبيت ولا يمكن تعديلها"), but evaluations may still be submitted through working day 3 and later. Evaluation counts, grade counts and `evaluationStatus` will usually be outdated in the report the manager submits, although 5.10 says the report includes evaluations. | Move evaluation fields out of the frozen snapshot into a live `evaluations` property on `DepartmentReportDto`, computed on read and frozen at `DepartmentReport_Submit`. Leave attendance, leave, custody and circular data frozen. |
| COV-9 | MAJOR | 5.9, 5.8 | `NotificationTemplateCode`, `Evaluation_GetManagerForm`, `Complaint_GetMyDetail` | The destination table sends users to routes that return 404. `evaluationPublished` and `evaluationObjectionDecided` go to `GET /evaluation/{id}` for the employee, and `evaluationResponseReceived` goes there for HR, but `Evaluation_GetManagerForm` returns 404 for both. `complaintUpdate` lists only `GET /complaint/{id}`, which returns 404 to the complainant, whereas `Complaint_GetMyDetail` says to try `my/{id}` first. | Set destinations: employee `GET /api/app/evaluation/my/{resourceId}`; HR `GET /api/app/evaluation/{resourceId}/hr-view`; complaint `my/{resourceId}` first, then `/{resourceId}` on 404. |
| COV-10 | MAJOR | 5.11 | `Report_GetHrAttendance`, `Report_GetHrLeaveBalances`, `Report_GetHrRequests` | The 5.11 table header marks all these reports as first version: "التقارير (النسخة الأولى **[قرار]**)". These three are tagged release 2, though their data all exists in release 1. The agreed mapping names only manager and reviewer reports for release 1, so it does not settle HR reports; the owner decision in 5.11 does. | Set these three to `x-release: 1`. Keep `Report_GetHrContracts` and `Report_GetHrWarnings` at 3 because their modules ship then, with a note that this is a dependency. If the owner wants HR reports later, record that in the document. |
| COV-11 | MAJOR | 4, 13 | `Delegation_Create`, `Delegation_Cancel`, `Delegation_GetCandidates` | Section 4 says "مفوض بديل اختياري لكل معتمد", and section 13 names delegates as the mitigation for one-person dependency. `accountant` is a single-holder role that decides through `Request_Approve`, but cannot create a delegation. `DelegationScope` also says finance can never be delegated, without separating accountant requests from financial data. | Add `accountant` to all three operations, limited to the accountant request step (`scope = requests`) with no access to finance screens. If this is unacceptable, record the exception in the document. |
| COV-12 | MINOR | 5.3, 10 | `AttendanceSettings_Update`, `SystemSettings_UpdateAttendance`, `AttendanceSettings_Get` | Two routes write the same value, with different error codes for one rule: `Hr:Attendance:GpsNotAvailable` and `Hr:Settings:GpsAttendanceNotAvailable`. The open question still says `fingerprint`, which is stale. | Keep `attendance-settings`, named in section 10. Remove `SystemSettings_GetAttendance` and `SystemSettings_UpdateAttendance`, point the `attendance` row of `SystemSettingsSectionCode` to it, and delete the `fingerprint` sentence. |
| COV-13 | MINOR | 5.5 | `Me_GetInboxSummary` | Tagged release 2, but it serves the unified inbox in 5.5, and requests are release 1. | Set `x-release: 1`. |
| COV-14 | MINOR | 5.1 | `Me_GetAdministrativeTaskLog`, `Employee_GetAdministrativeTaskLog` | Tagged release 2, but employees are release 1. | Set `x-release: 1`; entries from later modules appear when those ship. |
| COV-15 | MINOR | 5.11 | `Report_GetTeamCustody`, `Report_GetTeamEvaluations`, `Report_GetEvaluationConsistency` | Tagged release 2. The mapping puts manager and reviewer reports in release 1, while their data modules are release 2. | Set `x-release: 1` and return an empty list until the module ships, as `Report_GetReviewDashboard` does, or have the owner exempt them from the mapping. |
| COV-16 | MINOR | 4, 5.10 | `SystemSettings_GetReminders`, `SystemSettings_UpdateReminders` | Tagged release 1, while the mapping places reminders in release 2. Approval reminders are needed in release 1, but monthly cycle fields are not. | Keep `x-release: 1` and state that monthly fields take effect in release 2, or split `approvalReminderIntervalWorkingDays` into a release 1 section and tag the rest 2. |
| COV-17 | MINOR | 5.5, 5.6 | `Item_Lookup`, `IncidentCreateInputDto`, `Request_Submit` | `Incident_Create` (release 1) uses `itemId` from `Item_Lookup` (release 2), and store request types can be submitted in release 1. The note covering this exists only in the module text. | Add to `Incident_Create` and `Request_Submit`: `itemId` is null and store-destination types stay inactive until release 2. |
| COV-18 | MINOR | 10 | 41 operations, listed in the appendix | The route is in section 10, but `x-source` omits `10`. | Append `"10"` to `x-source` on these 41 operations. |
| COV-19 | MINOR | various | `AttendanceDevice_RotateKey`, `EmployeeDocument_Delete`, `LeaveRequest_ListMy`, `Complaint_UploadAttachment`, `Evaluation_GetMyList`, `Circular_Get`, `Warning_List`, `Warning_GetRecommendedByMe`, `PayrollRun_List`, `Payslip_DownloadMyPdf`, `AccountantRequest_List`, `AccountantRequest_Get`, `DeviceToken_Unregister` | `x-added-route` text cites a section missing from `x-source`. `AttendanceDevice_RotateKey` cites section 10, which has no such route. | Add the cited sections to `x-source`. For `AttendanceDevice_RotateKey`, change "(10، 11)" to "(5.18، 11)". |
| COV-20 | MINOR | none | `info.description`, 10 lines of descriptions | `info.description` points to `api-contract/README.md`, which does not exist. Descriptions name module files (`hr-finance-system.yaml` on 7 lines, `identity-org.yaml` on 3), which mean nothing to a reader of the merged file. | Create the README or remove the reference. Replace file names with operationIds or paths during the merge. |
| COV-21 | MINOR | 5.8 | `ComplaintHandlerSummaryDto`, `ComplaintHandlerDetailDto`, `SystemSettingsComplaintsDto` | `dueAt` is required and non-null, but `delayDeadlineWorkingDays` is null until set, with no initial value, so `dueAt` has nothing to return. | Mark `dueAt` nullable, or set an initial deadline once the owner answers NEEDS-OWNER #59. |
| COV-22 | MINOR | 3, 5.17 | `NotificationTemplateCode` | Descriptions promise notifications that have no template code: delegation created or cancelled, manager change, leave balance adjustment, password change, MFA changes, custody return, custody count difference, handler assignment. Also `Evaluation_DecideObjection` notifies the manager, while `evaluationObjectionDecided` lists only the employee. | Add template codes with recipient and destination, and add the manager as a recipient of `evaluationObjectionDecided`, opening `GET /api/app/evaluation/{resourceId}`. |
| COV-23 | MINOR | 5.16 | `AccountantRequestDto`, `RequestSubmitInputDto` | `AccountantRequestDto.attachments` is required, but no route or field lets anyone attach a file to a general request, so it is always empty. | Remove the field, since the document does not require attachments here, or add an upload route and `attachmentIds` as for leave. |
| COV-24 | MINOR | 5.14 | `WarningCreateInputDto` | 5.14 says the recommendation includes date, description "ومستنداتها", but `attachments` is optional. | Add `attachments` to `required` with `minItems: 1`, or have the owner confirm that documents are included only when they exist. |
| COV-25 | MINOR | 4 | `LeaveBalance_GetByEmployee`, `LeaveLedger_GetByEmployee` | `systemAdmin` approves a department manager's leave, and `executiveDirector` approves that of HR and the system admin, but neither can read the applicant's balance. | Allow these two roles, limited to applicants with a request at their step, or confirm that `LeaveRequestDto` is enough and record that. |
| COV-26 | MINOR | 3, 5.1 | `Custody_GetList`, `CustodyCount_GetList` | `hrManager` is allowed company-wide custody reads. Section 3 and the HR rows in 5.1 and 5.11 do not include custody. | Remove `hrManager`, or add it to the document as a stated requirement. |
| COV-27 | MINOR | 5.16 | `PayrollRun_RequestApproval`, `PayrollRun_Approve` | The document's flow is draft, approve, close. `Hr:Finance:ApprovalNotRequested` adds a mandatory precondition the document does not state. | Have the owner accept it, or make the call a notification only and remove the error code from `PayrollRun_Approve`. |
| COV-28 | MINOR | 3 | Self-service operations, `Complaint_GetAssignedList`, `Complaint_Close` | `x-allowed-roles` follows three patterns for the same case: `employee` alone (`Payslip_GetMyList`), all eight (`Request_GetMy`), and a subset (`Attendance_GetMy`). It also omits assignment holders with other roles, though module text calls the list a first check and says holders work "أيًا كانت أدواره". | Use `employee` for self-service operations and define that everyone with a profile holds it. Add `x-access-policy: ComplaintAccessPolicy` on handler operations and state that a holder passes regardless of role. |
| COV-29 | MINOR | none | `Report_Export`, `DepartmentReport_GetList`, `DepartmentReport_Get`, `EvaluationAnnualSummary_GetForEmployee`, `Circular_Create`, `Warning_Create` | `x-data-scope: team` although `hrManager` or `reviewer` gets company scope. The module rule that scope states the widest value was lost in merging. | Set `x-data-scope: company`, add `x-data-scope-by-role`, and document that extension in `CONVENTIONS.md`. |
| COV-30 | MINOR | 4 | `RequestEmployeeStatus` | Section 4 lists five states. The enum has seven, adding `pendingExecutive` and `cancelled`. `pendingExecutive` is needed for HR and system admin requests, and `cancelled` depends on NEEDS-OWNER #23. | Keep both, and add them to section 4 once #23 is answered. |
| COV-31 | MINOR | 5.4, 9 | `LeaveBalanceDto`, `LeaveBalance_GetMy` | "الترحيل: كل المتبقي، دون حد أقصى" appears only as "رُحِّل كاملًا" on `carriedDays`. The contract states no year-end rollover behavior: trigger date, no cap, or safe repetition. | Add to `LeaveBalance_GetMy`: on `nextYearStartDate` a daily job, safe to repeat, carries all remaining days with a `carryIn` entry, with no cap. Include the 5.4 example (24 − 18 + 21 = 27). |

## Open questions

### ANSWERED (11)

| # | opId | Section | Answer from the document |
|---|---|---|---|
| 1 | `UserAccount_SetRoles` | 3 | The column is "العدد الحالي", a current count and not a limit. The limit is not binding on reviewer or accountant. |
| 2 | `EmployeeShift_Assign` | 13, item 5 | "**الافتراض الحالي:** غير مشمول". The executive director is outside attendance unless the owner changes it. |
| 3 | `WeeklyOffSetting_Get` | 5.2, 5.4 | The calendar sets administrative dates, "بينما تحدد وردية الموظف حضوره". Leave counts "أيام دوام الموظف فقط". The contract's reading is correct. |
| 4 | `Attendance_GetMy` | 13, item 5 | Same as #2: "غير مشمول". |
| 5 | `LeaveBalance_SetEntitlement` | 5.4 | The example "باشر في 15/3/2026 برصيد 24 يومًا" shows the first value applies to the current year. |
| 6 | `LeaveBalance_Adjust` | 5.4 | "غير مسموح، إلا إذا أضاف مدير الموارد البشرية أيامًا". Adding means days to the balance. |
| 7 | `Report_GetEvaluationConsistency` | 5.11 | HR's row is "الحضور، الأرصدة، الطلبات، العقود، الإنذارات", and consistency is in the reviewer's row. Not available to HR in the first version. |
| 8 | `Adjustment_Create` | 5.16 | "يدوية مع السبب والمرفق". The attachment is required for manual entries. |
| 9 | `Payslip_GetMyList` | 3, 5.16 | "تكون صلاحياته مجموع أدواره", and the payslip "يراها الموظف بعد الإغلاق". A department manager sees their own payslip through the employee role. |
| 10 | `NotificationPreference_Update` | 5.17, 8 | "تُرسل بلغة المستلم" needs one stored language. This is contract cleanup, not an owner decision: keep one route on the single stored value. |
| 11 | `SystemSettings_UpdateAttendance` | 10 | Section 10 names `GET, PUT /api/app/attendance-settings` with "طريقة الحضور". Keep that route. See COV-12. |

### NEEDS-OWNER (61)

Where the document already answers part of an entry, the question says so and asks only the remaining decision.

| # | opId | Question for the owner | Recommended default |
|---|---|---|---|
| 1 | `UserAccount_ResetMfa` | من يعيد ضبط التحقق بخطوتين إذا فقد المستخدم جهازه؟ | المدير التنفيذي لأي مستخدم، ومسؤول النظام للمحاسب ومسؤولي النظام فقط. |
| 2 | `UserAccount_Create` | من هم الأشخاص الذين تُنشأ حساباتهم عند أول تشغيل، قبل أن يبدأ مدير الموارد البشرية عمله؟ | المدير التنفيذي ومدير الموارد البشرية ومسؤول النظام. |
| 3 | `UserAccount_ResetPassword` | من يعيد تعيين كلمة سر مدير الموارد البشرية نفسه؟ | المدير التنفيذي. |
| 4 | `Role_Create` | هل يحتاج مسؤول النظام إلى إنشاء أدوار جديدة، أم يكفي تعديل صلاحيات الأدوار الثمانية؟ | الاكتفاء بالأدوار الثمانية الآن. |
| 5 | `Employee_Create` | ما الحقول الشخصية المطلوبة في ملف الموظف، وهل نضيف رقم الهوية والرقم الوظيفي؟ (ارتباط المحاسب والمراجع بمدير محسوم في القسم 3: «لكل موظف مدير مرتبط».) | إضافة الرقم الوظيفي فقط. |
| 6 | `Employee_Update` | إذا كان تاريخ المباشرة خاطئًا وتوجد حركات رصيد، فهل نعيد حساب السنوات؟ وهل نمنع إنهاء خدمة من عليه عهدة؟ | تصحيح بمسار خاص مع سبب يعيد الحساب، ومنع الإنهاء حتى إرجاع العهدة. |
| 7 | `Employee_ChangeManager` | عند تغيير المدير، هل تنتقل المطالعات والبلاغات المعلقة إلى المدير الجديد، وأين تذهب الشكوى العادية المعلقة؟ | تنتقل المطالعات والبلاغات، وتذهب الشكوى العادية إلى الموارد البشرية. |
| 8 | `Manager_AssignEmployees` | عند ربط عدة موظفين بمدير دفعة واحدة، هل يطبق قرار البند السابق نفسه؟ | نعم، القرار نفسه. |
| 9 | `EmployeeDocument_Upload` | من يرى الوثيقة العادية ومن يرى الوثيقة الحساسة؟ | العادية: الموارد البشرية والموظف ومديره. الحساسة: الموارد البشرية والموظف فقط. |
| 10 | `Delegation_Create` | ما الذي يمكن تفويضه، وهل يمكن للموارد البشرية إنشاء تفويض عن معتمد غائب؟ | ثلاثة نطاقات كما في العقد، ويُسمح للموارد البشرية بذلك عند الغياب مع تسجيله. |
| 11 | `WorkShift_Create` | من يضيف قوالب الدوام، وما طرق احتساب العمل الإضافي، وهل يكفي التناوب بدورة أيام؟ | الموارد البشرية ومسؤول النظام، والطرق: بلا عمل إضافي، أو بعد نهاية الوردية، أو بعد عدد ساعات. ودورة الأيام تكفي. |
| 12 | `Holiday_Create` | إذا أُضيفت عطلة بعد تقديم إجازة تشملها، هل يُعاد حساب الإجازة تلقائيًا؟ | لا، ويصححها مدير الموارد البشرية بتعديل مع السبب. |
| 13 | `AttendanceDevice_IngestPunches` | ما شركة جهاز البصمة وطرازه وطريقة اتصاله؟ | وسيط محلي حتى يتأكد الطراز. |
| 14 | `AttendanceSettings_Get` | ما توقيت الشركة الرسمي الذي تُحسب به أوقات الحضور؟ (المسار والقيمة محسومان: `attendance-settings` و`device`.) | توقيت مقر الشركة، ويديره مسؤول النظام. |
| 15 | `DeviceUserMap_List` | من يربط مستخدمي جهاز البصمة بالموظفين؟ | مسؤول النظام ومدير الموارد البشرية. |
| 16 | `LeaveRequest_Preview` | كيف نحسب المتاح لجزء من الإجازة يقع في السنة القادمة، وهل نسمح بتواريخ ماضية؟ (الأنواع الأخرى تُحسب بأيام الدوام وفق جدول 5.4.) | المتبقي المتوقع ترحيله مع استحقاق السنة القادمة، ونعم للتواريخ الماضية. |
| 17 | `LeaveRequest_Create` | هل يمكن إلغاء إجازة بعد اعتمادها، وأين يذهب الرصيد المفكوك إذا رُفض طلب بعد إغلاق سنته؟ | لا إلغاء بعد الاعتماد إلا بتصحيح من الموارد البشرية، ويُضاف المفكوك إلى الأيام المضافة. |
| 18 | `LeaveRequest_UploadAttachment` | ما الحد الأقصى لحجم مرفق الإجازة وأنواعه ومدة حفظ غير المستخدم؟ | 10 ميجابايت، وPDF وJPEG وPNG، و24 ساعة، كما اقترح العقد. |
| 19 | `OtherLeaveType_Create` | ما معنى «الفئة المستحقة»، وهل الحد الأقصى لكل سنة أم لكل طلب؟ | الكل أو ذكور أو إناث مع إضافة حقل الجنس، والحد لكل سنة موظف. |
| 20 | `RequestType_Create` | كيف يسير الاحتياج الإداري، وهو غير موجود في المصفوفة؟ | مثل عمود الإجازة وتصحيح البصمة. |
| 21 | `Request_Submit` | أي صف في المصفوفة يطبق على من له عدة أدوار، وهل يقدم المدير التنفيذي احتياجًا إداريًا؟ | الأولوية: التنفيذي، الموارد البشرية، مسؤول النظام، مدير القسم، الموظف. ولا يقدم التنفيذي طلبًا بلا جهة تنفيذ. |
| 22 | `Request_Approve` | هل يجوز للمحاسب أو مسؤول المخزن تنفيذ طلبه الشخصي بعد موافقة الموارد البشرية؟ | نعم، لأنه تنفيذ وليس اعتمادًا، مع تسجيله. |
| 23 | `Request_Cancel` | متى يستطيع الموظف إلغاء طلبه، وهل تظهر له حالة «ملغى»؟ | قبل بدء التنفيذ فقط، وتظهر الحالة. |
| 24 | `Memo_Create` | إلى من تذهب مطالعة مدير القسم ومدير الموارد البشرية ومسؤول النظام، وهل يرفع المدير التنفيذي مطالعة؟ | مدير القسم إلى مسؤول النظام، والآخران إلى التنفيذي، والتنفيذي لا يرفع. |
| 25 | `Memo_Convert` | من يحول المطالعة إلى طلب أو بلاغ؟ | من هي عنده الآن، ويُعد تحويل المدير موافقته الأولى. |
| 26 | `Memo_Close` | هل يمكن إغلاق المطالعة دون تحويلها؟ | نعم، مع سبب يراه الكاتب. |
| 27 | `Incident_Create` | إلى من يذهب بلاغ مدير القسم؟ | مباشرة إلى الموارد البشرية. |
| 28 | `Incident_UpdateStatus` | ما حالات البلاغ بعد وصوله إلى الموارد البشرية؟ | قيد المعالجة ثم تم الحل. |
| 29 | `Stock_Receive` | هل يرى مسؤول المخزن تكلفة ما يدخله بعد حفظها؟ | لا، يدخلها فقط ويراها المحاسب. |
| 30 | `Custody_Create` | هل تخصم العهدة المباشرة من رصيد المخزن؟ | يختار المسؤول: تخصم عند التسليم الفعلي، ولا تخصم لما هو عند الموظف أصلًا. |
| 31 | `Custody_Acknowledge` | من يؤكد استلام العهدة المشتركة، خصوصًا إذا كان في القسم أكثر من مدير؟ | أي مدير له موظفون في القسم، ويُسجل اسمه. |
| 32 | `Custody_Return` | من يسجل إرجاع العهدة، وماذا عن التالف والإرجاع الجزئي؟ | مسؤول المخزن للكمية كاملة، والتالف لا يعود إلى الرصيد. |
| 33 | `Complaint_PreviewRouting` | ما مسار الشكوى على المحاسب؟ | مثل مسؤول المخزن: إلى الموارد البشرية مباشرة دون مدير القسم. |
| 34 | `Complaint_Submit` | إلى من تذهب الشكوى إذا كان مقدمها هو جهة معالجتها، مثل مدير الموارد البشرية أو المدير التنفيذي؟ | شكوى مدير الموارد البشرية تذهب إلى المدير التنفيذي استثناءً، ولا يقدم المدير التنفيذي شكوى داخل النظام. |
| 35 | `Complaint_GetMyDetail` | هل يرى مقدم الشكوى قرار الإغلاق؟ | يرى القرار النهائي فقط، دون الإجراءات واسم المعالج. |
| 36 | `Complaint_GetAssignedList` | إلى أين تذهب الشكوى العادية المعلقة عند تغيير المدير؟ | إلى الموارد البشرية مباشرة، حفاظًا على السرية. |
| 37 | `Complaint_RaiseLevel` | هل يستطيع المعالج رفع مستوى الشكوى أيضًا؟ | نعم، مقدم الشكوى والمعالج الحالي. |
| 38 | `Complaint_Forward` | هل يغلق مدير القسم الشكوى العادية بنفسه، وهل يبقى له وصول بعد تحويلها؟ | لا يغلقها، وينتهي وصوله بعد التحويل. |
| 39 | `ComplaintHandler_Request` | من يقدم طلب تعيين معالج الشكاوى؟ | مدير الموارد البشرية فقط، والموافقة للمدير التنفيذي. |
| 40 | `EvaluationCycle_GetCurrent` | من يقيّم موظفًا مديره ليس مدير قسم، مثل مدير الموارد البشرية؟ | لا يُقيَّم حاليًا. |
| 41 | `Evaluation_AddResponse` | هل توجد مهلة للاعتراض على التقييم؟ | 5 أيام دوام من تاريخ الرفع. |
| 42 | `Evaluation_DecideObjection` | هل يغير قبول الاعتراض الدرجات، ومن يبت في اعتراض مدير الموارد البشرية نفسه؟ | يعدل مدير الموارد البشرية الدرجة مع سبب، ويبت المدير التنفيذي في اعتراضه. |
| 43 | `EvaluationInternalNote_Add` | هل يكتب مدير الموارد البشرية ملاحظات داخلية بنفسه؟ | لا، يكتب المقيِّم ويقرأ مدير الموارد البشرية فقط. |
| 44 | `EvaluationGrade_Update` | من يعدل سلم التقييم؟ | مسؤول النظام. |
| 45 | `Report_Export` | هل يصدّر المراجع والموارد البشرية تقاريرهم PDF وExcel أيضًا؟ | نعم، بالقيود نفسها. |
| 46 | `Circular_Create` | هل يرسل المدير التنفيذي تعاميم أيضًا؟ | نعم، إلى مديري الأقسام وكل الشركة. |
| 47 | `Circular_Acknowledge` | هل تأكيد الاستلام مطلوب لتعاميم الفريق والشركة أيضًا؟ | نعم لكل التعاميم، لأن التقرير الشهري يجمع التأكيدات. |
| 48 | `Contract_Recommend` | من يوصي ويقرر في عقد مدير الموارد البشرية نفسه؟ | المدير التنفيذي. |
| 49 | `Contract_Decide` | ما أثر تجاوز مهلة الأيام الخمسة، وهل ينشأ العقد الجديد تلقائيًا عند التجديد؟ | تذكير وعلامة تأخر فقط، وينشئ مدير الموارد البشرية العقد الجديد. |
| 50 | `Warning_Create` | هل يجب أن يسبق الإنذار الأول الإنذار النهائي؟ | لا يُشترط، ويبقى القرار للموارد البشرية. |
| 51 | `Warning_GetMy` | هل يرى الموظف مستندات الواقعة واسم صاحب التوصية؟ | يرى المستندات دون اسم صاحب التوصية. |
| 52 | `Warning_Notify` | هل تستطيع الموارد البشرية رفض توصية دون إبلاغ الموظف؟ | نعم، مع سبب مسجل. |
| 53 | `Warning_Decide` | من يقرر إذا كان مدير الموارد البشرية هو صاحب التوصية؟ | المدير التنفيذي. |
| 54 | `Warning_Object` | هل توجد مهلة للاعتراض على الإنذار؟ | 5 أيام دوام من الإبلاغ بالقرار. |
| 55 | `AppreciationLetter_Create` | هل يحتاج «التنسيق مع الإدارة» خطوة داخل النظام؟ | لا، يتم خارج النظام. |
| 56 | `PayrollRun_Approve` | ماذا يحدث إذا وجد المعتمد خطأ في الكشف؟ | يعيده إلى المحاسب مع سبب، ويبقى مسودة. |
| 57 | `PayrollRun_Close` | من يغلق الكشف الشهري؟ | المحاسب بعد الاعتماد. |
| 58 | `SystemSettings_UpdateReminders` | كل كم يوم يصل تذكير الموافقات، وما أقل مدة عمل لتقييم من باشر في منتصف الشهر؟ | كل يومي دوام، و15 يوم عمل. |
| 59 | `SystemSettings_UpdateComplaints` | بعد كم يوم تُعد الشكوى متأخرة، وهل تختلف المدة حسب المستوى؟ | 10 أيام دوام لكل المستويات. |
| 60 | `SystemSettings_UpdateHourlyLeave` | من يدير إعدادات الزمنيات، وكم ساعة مسموحة في الشهر، وكم ساعة تساوي يومًا؟ | مسؤول النظام، و8 ساعات في الشهر، و8 ساعات لليوم. |
| 61 | `SystemSettings_UpdateFinance` | من يعتمد الكشف الشهري: مدير الموارد البشرية أم المدير التنفيذي؟ ومن يغير هذا الإعداد؟ | مدير الموارد البشرية، ويغيره مسؤول النظام. |

The numeric defaults in items 41, 54, 58, 59 and 60 are starting suggestions from this audit, not values from the document.

## Appendix: operations for COV-18

`Request_Submit`, `Request_GetMy`, `Request_GetInbox`, `Request_Get`, `Request_Approve`, `Request_Reject`, `Memo_Create`, `Memo_Convert`, `Incident_Create`, `Stock_Receive`, `Complaint_GetRespondents`, `Complaint_PreviewRouting`, `Complaint_Submit`, `Complaint_GetMyList`, `Complaint_GetAssignedList`, `Complaint_GetDetail`, `Complaint_RaiseLevel`, `Complaint_AddAction`, `Complaint_Close`, `Complaint_GetAccessLog`, `ComplaintHandler_Request`, `ComplaintHandler_Approve`, `EvaluationCycle_GetCurrent`, `Evaluation_GetManagerForm`, `Evaluation_SaveDraft`, `Evaluation_Submit`, `Evaluation_Acknowledge`, `Evaluation_AddResponse`, `Evaluation_DecideObjection`, `DepartmentReport_Get`, `DepartmentReport_UpdateNotes`, `DepartmentReport_Submit`, `Report_GetTeamAttendance`, `Report_GetTeamLeaves`, `Report_GetTeamPending`, `Report_GetReviewDashboard`, `Report_GetEvaluationConsistency`, `Report_Export`, `Circular_GetReceivedList`, `Circular_Create`, `Circular_Acknowledge`
