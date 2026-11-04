# Integration Readiness Audit

Scope: `openapi.yaml` (OpenAPI 3.0.3), read as if it were the only input to two separate tools, one building ASP.NET Core + ABP and one building Flutter. Scripts are in `C:\Users\zainab\AppData\Local\Temp\claude\0f00105c989e41e3\7f15f226-d57e-4beb-a22a-b0854714edd2\scratchpad\audit_integration\`.

## Summary

| Severity | Count | Meaning |
|---|---:|---|
| BLOCKER | 6 | Following the contract as written breaks client and server integration |
| MAJOR | 24 | A tool has to guess, or needs a decision or configuration the contract does not give |
| MINOR | 22 | Polish and consistency |
| **Total** | **52** | |

**The three most serious findings**
1. **INT-3, INT-4:** the 422 error mapping and the access-gate rules. All 194 business codes return 403 on ABP by default. The gate rules that must stay 403 are missing from `openapi.yaml`.
2. **INT-1, INT-2:** the wire formats. The stated enum converter emits PascalCase, and .NET's default date and time formats break the declared `format: date` and `HH:mm`.
3. **INT-5, INT-6:** notification deep links. Four templates send the employee or HR to a route their role cannot read.

### Verification limits

- **Run:** Redocly lint and all the scripts below, across all 308 operations, 252 paths and 496 schemas.
- **Not run:** `openapi-generator`, NSwag and an ABP server, because none was available and nothing could be installed. Every statement about generator output or ABP runtime behaviour comes from my knowledge of those tools and is marked **[not run]**. Confirm these with a generation smoke test and one integration test.
- **Descriptions:** I read the operation descriptions in part: the first 233 of 936 lines in full, then targeted extracts for the state-machine operations. The enum reachability scan is a literal text match, so an Arabic description of a transition counts as "not named".
- **Requirements document:** I did not cross-check it for business rules. INT-46 therefore remains a question.

### Measured results

| Check | Result |
|---|---|
| Redocly errors / warnings | 0 / 151 |
| operationId format / duplicates / case-insensitive duplicates | 0 / 0 / 0 |
| Inline body schemas | 10, all `format: binary` download responses, so 0 violations |
| `oneOf` / `anyOf` / `discriminator` | 0 |
| Paged result schemas | 86, all with `totalCount` int64 and `items`; 0 bad |
| Paged responses missing a paging parameter | 0 |
| Missing Accept-Language | 0 |
| Missing 400 / 401 / 403 / 404 | 3 / 0 / 11 / 0 |
| 422 without codes, or codes without 422 | 0 |
| `required` naming a property that does not exist | 0 |
| Path parameters that disagree with the path | 0 |
| Required `x-*` extensions missing | 0 |
| Inline enums | 4 properties: 3 OAuth (exempt, fixed by the standard) and 1 real (INT-33) |
| Enum values not camelCase | 9, all OAuth and exempt |
| id, date, time, decimal format violations | 0 real. `deviceUserId` (5 fields) and `AuditLogEntryDto.entityId` are plain strings by design |
| Time-only fields with the `HH:mm` pattern | 16 |
| Input with `concurrencyStamp` but no 409 | 0 |
| 409 declared without a stamp (non-create) | 0 |
| PUT/PATCH with no stamp | 5: `Me_Update`, `UserAccount_UpdatePermissions`, `Role_UpdatePermissions`, `EmployeeShift_Assign`, `EvaluationInternalNote_SaveDraft`. Acceptable, as none of their read DTOs has a stamp |
| Examples checked by my own validator (null under `allOf` allowed) | 392, with 1 mismatch (INT-34) |
| Examples missing | 3 request, 18 response (INT-35) |
| Distinct error codes / uses | 194 / 297 |
| Error codes breaking `Hr:<Module>:<PascalCaseName>` | 0 |
| `x-open-question` operations | 72 |

### Redocly warnings by class

| Rule | Count | Verdict |
|---|---:|---|
| `no-invalid-media-type-examples`, null under `allOf` + `nullable` | 123 | Spec-strict rejection, not a data error. See below and INT-28 |
| `no-invalid-media-type-examples`, "exclusiveMinimum value must be number" | 3 | **Tool limitation.** `exclusiveMinimum: true` is valid in 3.0. Validation aborts, so lint did not check examples for `Request_Submit`, `Stock_Receive`, `Custody_Create`. My validator checked them and they pass |
| `no-invalid-media-type-examples`, missing `file` | 1 | **Real defect** (INT-34) |
| `no-ambiguous-paths` | 20 | No runtime conflict (INT-50) |
| `info-license`, `no-server-example.com`, `operation-4xx-response`, `no-unused-components` | 1 each | Noise. The last two are `OpenIdConfiguration_Get` and `NotificationPushPayloadDto`, which is the FCM payload and has no HTTP operation by design |

Redocly's JSON output is capped at 100 problems by default. Use `--max-problems` to see all 151.

### Is `allOf` + `nullable` a linter limitation?

Not purely.
- **Under OAS 3.0.3:** `nullable: true` only takes effect when `type` is in the same Schema Object. `allOf: [$ref]` with a sibling `nullable: true` has no such `type`, and the `$ref` target does not allow null. The linter's strict reading is defensible.
- **In practice:** the form is the common idiom for a nullable reference, and Swashbuckle and NSwag emit it themselves **[not run]**. Generators generally honour the sibling `nullable`.
- **Recommendation:** keep the form. Do not put `nullable: true` on the shared target, as that makes every use nullable. Do not move to `oneOf` with null, which conventions forbid and which generates poor Dart. Filter this warning class in CI, then prove the behaviour with the smoke test in INT-28.

### State machines

| Machine | Followable from operations? |
|---|---|
| Request / leave / correction | Yes. `Request_Approve`, `_Reject` and `_Cancel`, with routes per submitter |
| Warning | Yes. Every transition is stated, including the daily job to `responseExpired` |
| Contract | Yes for the stated transitions. `active` to `pendingRecommendation` is mentioned only in `Contract_Create` and `Contract_GetExpiring` |
| Stock issue, custody, incident, handler assignment | Yes |
| Payroll run | Yes, forward only (INT-46) |
| Memo | **Gap** (INT-23) |
| Complaint, evaluation | Yes for what I read. `ComplaintStatus.pendingDepartmentManager` is not named in any operation text (INT-47) |

### ASP.NET routing with ambiguous paths

- **Same-verb overlaps:** 30 pairs of same-length paths have a literal facing a parameter. Only 4 cross each other, all GET: `complaint/my/{id}` with `complaint/{id}/access-log`, `evaluation/my/{id}` with `{id}/hr-view` and with `{id}/internal-note`, and `appreciation-letter/employee/{employeeId}` with `{id}/download-url`.
- **With `{id:guid}`:** no overlap is possible, since `my` and `employee` are not GUIDs.
- **Without the constraint:** endpoint routing precedence still prefers the literal first segment, and a real client never sends a non-GUID **[not run]**.
- **Consequence:** the 20 warnings do not need route changes.

### ABP fit

Matches: `/api/app` on 304 of 308 operations (the other 4 are the OIDC discovery and token routes, ABP's own configuration route and the device route allowed by convention), the `RemoteServiceErrorResponse` envelope (`code`, `message`, `details`, `data`, `validationErrors[].message/members`), and the `skipCount`, `maxResultCount`, `sorting` names. Items that need explicit server configuration are in INT-1, 2, 3, 15, 18, 19, 20, 27 and 48.

## Findings

| ID | Severity | Operation or schema | Problem | Exact fix |
|---|---|---|---|---|
| INT-1 | BLOCKER | All 92 named enums; `CONVENTIONS.md` | The rule says only "register `JsonStringEnumConverter`". With no naming policy it writes PascalCase (`PendingManager`), while the contract and the Dart client expect `pendingManager` **[not run]**. Every status parse fails on the client. | State in the contract: `new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)`, registered for both MVC JSON and ABP's JSON options. Mechanical. |
| INT-2 | BLOCKER | 16 `HH:mm` fields; all `format: date` and `date-time` fields | .NET's `TimeOnly`/`TimeSpan` writes `08:30:00`, which breaks the pattern. A `DateTime` used for a date writes `2026-03-15T00:00:00`, which breaks `format: date`. Without UTC clock settings, `date-time` loses its `Z` **[not run]**. The contract gives no server type or converter. | Add a server note: use `DateOnly` for `date`; use a custom `HH:mm` converter for `TimeOnly`; set `AbpClockOptions.Kind = Utc`. Add one round-trip test per format. |
| INT-3 | BLOCKER | 194 codes in `x-error-codes`; `Hr:Identity:PasswordChangeRequired`, `Hr:Identity:MfaEnrollmentRequired` | ABP returns 403 for business exceptions by default, and its code-to-status mapping is by exact code **[not run]**. Clients would see 403 for every rule failure. A blanket `Hr:*` to 422 rule would also wrongly turn the two gate codes into 422, though they must be 403. | Require a custom `IHttpExceptionStatusCodeFinder`: `Hr:` codes give 422, except an explicit 403 list naming the gate codes. Publish that list in the contract. |
| INT-4 | BLOCKER | `x-global-rules`; `AccessGate`, `EmployeeCreateDto`, `Employee_Create`, `Employee_ChangeManager`, `Manager_AssignEmployees` | These cite `x-global-rules.access-gates` and `.manager-link`, but `openapi.yaml` has no such key. It exists only in `modules/identity-org.yaml`, and the bundler dropped it. A tool given the contract alone has neither the gate's allowed-operation list nor the rule for who can manage whom, nor the `error-codes-meaning` dictionary. | Copy `x-global-rules` to the root of `openapi.yaml` in the bundle step. Mechanical. |
| INT-5 | BLOCKER | `NotificationTemplateCode`: `evaluationPublished`, `evaluationObjectionDecided`, `evaluationResponseReceived` | The table routes all three to `GET /api/app/evaluation/{resourceId}`, which is `Evaluation_GetManagerForm`, allowed for `departmentManager` only. The recipients are the employee (two templates) and HR (one), so the link fails. | Employee templates: `GET /api/app/evaluation/my/{resourceId}`. `evaluationResponseReceived`: `GET /api/app/evaluation/{resourceId}/hr-view`. Mechanical. |
| INT-6 | BLOCKER | `complaintUpdate`; `Complaint_GetDetail`, `Complaint_GetMyDetail`; `NotificationDto` | One template serves both complainant and handler and points to `GET /api/app/complaint/{resourceId}`. That route excludes `employee` and returns 404 to a complainant, for example after `Complaint_Close`. The payload has no field telling the client which route to use. | Split into `complaintUpdateForComplainant` (`/complaint/my/{id}`) and `complaintUpdateForHandler` (`/complaint/{id}`). Neither reveals level or subject. |
| INT-7 | MAJOR | 11 `Complaint_*` operations | They omit 403 by design, to avoid revealing existence. The access gate says it blocks every operation with 403, so these routes can return an undeclared status. The contract does not say which wins. | Declare 403 on the 11 for gate codes only, and state that authorization failures stay 404. Or state that the gate returns 404 here, though then the client cannot route to the gate screen. |
| INT-8 | MAJOR | `AttendanceSettings_Get/_Update`, `SystemSettings_GetAttendance/_UpdateAttendance` | Two routes and two DTOs for one value. They have separate `concurrencyStamp` fields and different error codes (`Hr:Attendance:GpsNotAvailable`, `Hr:Settings:GpsAttendanceNotAvailable`). The open question in `AttendanceSettings_Get` still cites a `fingerprint` value that no longer exists anywhere else in the contract. | Remove `SystemSettings_*Attendance`, its DTOs and code. Keep `attendance` in `SystemSettingsSectionCode` pointing to `/attendance-settings`. Rewrite the open question. |
| INT-9 | MAJOR | `Me_Update`, `NotificationPreference_Get/_Update` | Both write the same stored language. One has a stamp and the other does not. `NotificationPreferenceDto` has no field beyond the language and the stamp. | Remove `NotificationPreference_*` and read `MyAccountDto.preferredLanguage`. Update the wording in `NotificationPushPayloadDto`. |
| INT-10 | MAJOR | `ConnectToken_Issue`; `oauth2` scheme | The Flutter `client_id` and `redirect_uri` values are never given, and neither is the production issuer (`servers` has localhost only). The two tools would invent values that do not match the OpenIddict seed. | Add `x-oauth-client` with `clientId`, the redirect URI per platform (Android, iOS, web) and scopes. Add the environment URLs to `servers`. |
| INT-11 | MAJOR | `UserAccount_ResetMfa`, `UserAccount_ResetPassword`, `AuditLog_List.userId` | `executiveDirector` may call these with a user id, but `UserAccount_List/_Get` allow only `hrManager` and `systemAdmin`. `Employee_Lookup` returns employee ids, not user ids. The only source is `Delegation_GetCandidates`, which exists for another purpose. | Add `GET /api/app/user-account/lookup` returning `UserRefDto`, for `executiveDirector`, `hrManager`, `systemAdmin`. |
| INT-12 | MAJOR | `cycleId` filter on 6 operations; `EvaluationCycle_GetCurrent` | The only cycle route returns the current one, so a past cycle cannot be chosen from a list. Ids appear only indirectly in evaluation rows and `ReviewDashboardDto`. | Add `EvaluationCycle_GetList`, paged, filter `year`, with the roles of `EvaluationCycle_GetCurrent`. |
| INT-13 | MAJOR | `AccountantRequestDto.attachments` | The field is required, but no route downloads these attachments. `RequestSubmitInputDto` and `RequestDetailsDto` have no attachment field, so nothing can create them. | Either remove the field, or add `Request_UploadAttachment`, `attachmentIds` on submit, and `AccountantRequest_GetAttachmentDownloadUrl`. |
| INT-14 | MAJOR | 8 multipart operations plus downloads | There are five attachment models: upload first then ids (leave), inline array (warning), inline single (letter, adjustment, expense), upload after create (complaint), and own DTO (employee document). 5 operations state no numeric size or type limit: `Complaint_UploadAttachment`, `Warning_Create`, `AppreciationLetter_Create`, `Adjustment_Create`, `Expense_Create`. | Keep the models if intended, and document why. For the 5, state that `Hr.Document.MaxFileSizeBytes` and `Hr.Document.AllowedContentTypes` apply, or give the limits. Unify the "too large" and "rejected" codes (INT-42). |
| INT-15 | MAJOR | `Warning_Create`, `AppreciationLetter_Create`, `Adjustment_Create`, `Expense_Create`, `EmployeeDocument_Upload`, `AttendanceImport_Create` | No multipart body declares `encoding`. Number and date text formats, repeated part naming for `attachments[]` and the treatment of null optional parts are all undefined. ABP DTOs holding `IRemoteStreamContent` also need `FormBodyBindingIgnoredTypes` **[not run]**, and dart-dio handling of arrays of binary is a known weak spot **[not run]**. | Add `encoding` and a rule: invariant decimal, ISO date, omit null parts, repeat `attachments` per file. Add the ABP binding note. Include `Warning_Create` in the smoke test. |
| INT-16 | MAJOR | `FileDownloadUrlDto`; 5 of 6 binary downloads | The contract does not say whether the short-lived URL needs the bearer token, or whether it may be on another host (a client that adds the header may break a signed URL). Only `Complaint_DownloadAttachment` mentions `Content-Disposition`; the 5 others do not define a file name. | Describe the URL as "no Authorization header; lifetime N minutes". Declare the `Content-Disposition` header on all 6 binary responses. |
| INT-17 | MAJOR | `x-allowed-roles` vs `x-permission`; `Role_Create`, `UserAccount_UpdatePermissions`, `Role_UpdatePermissions` | A custom role or direct grant can hold a permission on an operation whose `x-allowed-roles` excludes it. Conventions call the role list a contract for backend checks, while the client hides UI by `grantedPolicies`. Neither says which decides. | State: `x-permission` is the server check; `x-allowed-roles` is the default seed of role to permission. List any role checks that are truly fixed, such as `ComplaintHandler_Approve`. |
| INT-18 | MAJOR | `IdempotencyKey`; 31 operations | ABP has no built-in handling **[not run]**. Undefined: key scope (per user?), retention, the replayed status and body, a repeated key with a different body, and concurrent duplicates. 9 of the 201 operations have no key; only `Employee_Create` explains why. | Add to the parameter description: per user + operation, 24h retention, replay original status and body, different body gives 422 `Hr:Common:IdempotencyKeyReused`. Say the omission is intended on the other 8, or add the key. |
| INT-19 | MAJOR | `SkipCount`, `MaxResultCount` | The contract says default 20, maximum 200. ABP's `PagedResultRequestDto` defaults are 10 and 1000 **[not run]**. An unconfigured server silently returns 10 rows. | Add a server note: set `DefaultMaxResultCount = 20`, `MaxMaxResultCount = 200` at startup. |
| INT-20 | MAJOR | 32 operations with 201 or 202; all custom routes | ABP conventional controllers return 200 for non-void methods and cannot derive routes such as `/finance/payslip/my/{id}/pdf` **[not run]**. | Require explicit `[Route]`, `[Http*]` and status results on these controllers, with `{id:guid}`. Add a route-table test comparing the running server with this file. |
| INT-21 | MAJOR | `RemoteServiceErrorInfo.data`; `LeaveRequest_Create`, `Manager_AssignEmployees` | `error.data.availableDays`, `requestedDays`, `shortageDays` and `invalidEmployeeIds` exist only in Arabic text; `data` is an untyped map. ABP also sends exception data only under its options **[not run]**. | Add `x-error-data` per code, with field names and types. State the ABP setting that sends it. |
| INT-22 | MAJOR | `RemoteServiceValidationErrorInfo.members` | The format is undefined. ASP.NET model state may give `input.Name` or `Name`, while the client's fields are camelCase **[not run]**. The client cannot attach errors to form fields. | Define `members` as camelCase JSON paths, and require normalizing on the server. |
| INT-23 | MAJOR | `MemoStatus`; `Memo_Create`, `Memo_Forward`, `Memo_Close`, `Memo_Convert` | Memos from `hrManager` and `systemAdmin` go to the executive director, but `MemoStatus` has no executive value, unlike `RequestStatus`. The status for that stage is unstated, and the `status` filter is ambiguous. | Add `pendingExecutive` to `MemoStatus`, or state: status stays `pendingManager` with `currentHandlerRole = executiveDirector`. |
| INT-24 | MAJOR | `Employee_ChangeManager`, `Complaint_RaiseLevel`, `Complaint_Forward`, `Complaint_Close` | Each consumes a stamp but its response has no new one. A second action with the old stamp gets 409. Separately, `LeaveBalance_Adjust` and `Incident_Forward` change a stamped aggregate but take no stamp. | Add `concurrencyStamp` to `ManagerChangeResultDto` and `ComplaintTransitionResultDto`. Add a stamp to the other two inputs, or state last-write-wins. Mechanical. |
| INT-25 | MAJOR | `WarningDecideInputDto`, `WarningDecisionDto`, `WarningResolveObjectionInputDto`, `ContractDecideInputDto`, `EvaluationObjectionDecideDto`, `ComplaintCloseInputDto` | The same concept has different shapes. Warning input `decision` is read back as `outcome`, while its objection input uses `outcome`. Rationale is `note` in some and `reason` in others (read back as `decisionReason`). `ComplaintCloseInputDto.decision` is free text, whereas every other `decision` is an enum. | Standardize on `decision` (enum) plus `reason` (text) for input, and the same names on read. Rename the complaint field `closureText`. Mechanical once chosen. |
| INT-26 | MAJOR | `AttendancePeriodSummaryDto.leaveDays` | Integer here, `number: decimal` in `EvaluationAttendanceSummaryDto`, `DepartmentReportEmployeeRowDto` and `HrAttendanceReportRowDto`. A fractional value would fail Dart parsing for an `int` field. | Change to `number`, `format: decimal`. Mechanical. |
| INT-27 | MAJOR | `Unauthorized` response | Declared as JSON with `RemoteServiceErrorResponse`, but a JWT bearer challenge usually gives 401 with an empty body **[not run]**. Client parsing would fail on the error path. | Say the body may be empty on 401, or add a challenge handler that writes the envelope. Test with an expired token. |
| INT-28 | MAJOR | Nullable references, 123 example warnings | Generator behaviour for `allOf` + `nullable` is unproven here **[not run]**. If one drops the flag, a required nullable field would fail on null at runtime. | Generate Dart (dart-dio) and C# in CI, and assert fields such as `RequestStepDto.approver` are nullable. If one fails, add a `type` sibling or a generator template. |
| INT-29 | MAJOR | 8 templates: `memoUpdate`, `incidentUpdate`, `stockIssuePending`, `custodyAssigned`, `circularReceived`, `monthEndCycleOpened`, `monthlyCycleReminder`, `monthlyCycleEscalated` | The table names a screen rather than an operation. `evaluationCycle` has no get-by-id route, so `resourceId` cannot be used. | Name a route for each, for example `GET /api/app/memo/{resourceId}`. For the cycle templates, either add a get-by-id or set `resourceType` to none and use `EvaluationCycle_GetCurrent`. |
| INT-30 | MAJOR | 72 operations with `x-open-question` | Each is an unresolved assumption that both tools may read differently. I did not check which the owner has already answered. | Close each one, record the answer in the description and remove the key. Treat the rest as change risks. |
| INT-31 | MINOR | `Complaint_Submit`, `Complaint_AddAction`, `Complaint_UploadAttachment`, `ComplaintHandler_Request`, `Evaluation_AddResponse`, `EvaluationInternalNote_Add`, `EvaluationCriterion_Create`, `EvaluationGrade_Create`, `Circular_Create` | These 9 create a record and return 200, while 31 others return 201. `LeaveRequest_UploadAttachment` is 201 and `Complaint_UploadAttachment` is 200. Dio accepts any 2xx by default **[not run]**. | Change to 201. Mechanical. |
| INT-32 | MINOR | `EmployeeShift_GetHistory`, `DeviceUserMap_ListUnmapped`, `GeoFence_List` | Paging parameters but no 400. | Add the `BadRequest` response. Mechanical. |
| INT-33 | MINOR | `IncidentStatusUpdateInputDto.status`; `Incident_UpdateStatus` | Inline enum, and a status change by PATCH rather than a verb segment. | Name it `IncidentStatusTarget`. Optionally move to `POST /incident/{id}/status`. Mechanical. |
| INT-34 | MINOR | `EmployeeDocument_Upload` | The example lacks required `file`. | Add a `file` placeholder value. Mechanical. |
| INT-35 | MINOR | Request: `AttendanceImport_Create`, `LeaveRequest_UploadAttachment`, `Complaint_UploadAttachment`. Response: `ComplaintHandler_GetList/_Approve/_Reject/_Revoke`, `Evaluation_SaveDraft/_Submit/_DecideObjection/_GetHrList`, `EvaluationInternalNote_SaveDraft`, `EvaluationAnnualSummary_GetForEmployee`, `EvaluationCriterion_Create/_Update`, `EvaluationGrade_Create/_Update`, `DepartmentReport_GetList/_UpdateNotes/_Submit`, `Circular_GetSentList` | Examples are missing. The main reads among them break the conventions. | Add examples. Mechanical. |
| INT-36 | MINOR | Query parameters | Seven date-range pairs: `fromDate/toDate` (15 operations), `from/to` (8), `dateFrom/dateTo` (4), `periodFrom/periodTo` (3), `endFrom/endTo` (2), `endDateFrom/endDateTo` (1), `incidentDateFrom/To` (1). `from/to` is `date` on reports but `date-time` on `AuditLog_List`. Text search is `search` (8) or `filter` (4). | Use `fromDate/toDate` for dates, a named prefix only where needed, and `search` everywhere. Mechanical. |
| INT-37 | MINOR | Several DTOs | Synonyms: `shiftId` vs `workShiftId`; `leaveDate` vs `date`; `startDate` vs `effectiveFrom`; `currentApproverRole`, `currentStageRole`, `currentHandlerRole`; `contractType` vs `typeName`; `LeaveRequestDto.requestId` vs `id`. | Pick one name per concept. Mechanical. |
| INT-38 | MINOR | Path parameters; `UserRefDto`, `EmployeeRefDto` | An employee is `{id}` on 17 routes and `{employeeId}` on 6. `{id}` means a user on `/user-account` and an employee on `/manager`. User and employee ids have the same type, so a wrong one compiles. | Use `{employeeId}` and `{userId}` wherever the path is not the resource itself. Route binding stays the same. |
| INT-39 | MINOR | `ReportCustodyKind`, `FinanceExportFormat` | Identical to `CustodyKind` and `ReportExportFormat`. Of the 13 identical-shape groups, the rest are mostly coincidental. | Delete the duplicates and point to the originals. Mechanical. |
| INT-40 | MINOR | `StockMovementType.return`, `WarningLevel.final`, `AttendanceDayStatus.late`, `AuditActionType.export`, `WorkShiftType.fixed` | Dart keywords or built-in identifiers (`fixed` and `return` in lowercase C#). Generators should escape them, giving names such as `return_` **[not run]**. No property names collide, none start with a digit, tags do not collide and no schema name clashes with the Dart core names I checked. | Optionally add `x-enum-varnames`. Keep the wire values. |
| INT-41 | MINOR | `TeamAttendanceRowDto`, `TeamLeaveRowDto`, `TeamPendingRequestRowDto`, `TeamCustodyRowDto`, `HrRequestReportRowDto`, `HrContractReportRowDto`, `HrWarningReportRowDto` | `statusName` is localized text with no enum, so the client cannot color or branch on it. | Add the enum beside the label. |
| INT-42 | MINOR | `x-error-codes` | 184 of 194 codes are not named in any operation description, and the meaning dictionary is outside the bundle. Without a catalogue, I could not verify that equal short names mean the same thing. Module segments mix plural and singular (`Complaints`, `Evaluations`, `Reports`, `Circulars` vs `Contract`, `Warning`). Near-duplicates: `Hr:Leave:InvalidAttachment`/`AttachmentRejected`, `Hr:Document:FileTooLarge`/`Hr:Complaints:AttachmentTooLarge`, `Hr:Appreciation:FileRejected`, `Hr:Attendance:GpsNotAvailable`/`GpsMethodDisabled`, `Hr:Memo:InvalidState`, `Hr:Stock:IssueInvalidState`, `Hr:Warning:InvalidStatus`, `Hr:Incident:InvalidTransition`. Referenced-entity failures are sometimes 422 (`Hr:Department:NotFound`, 5 `EmployeeNotFound` codes) and sometimes 404. | Publish a catalogue: code, meaning, `ar` and `en` message, `data`. Normalize module names, and set one rule for a missing referenced id. |
| INT-43 | MINOR | `x-data-scope`; 20 operations such as `Request_GetList`, `Attendance_GetByEmployee` | `company` is listed while `departmentManager` is allowed with team scope only. A single value cannot express this. | Allow a per-role map, for example `{hrManager: company, departmentManager: team}`. |
| INT-44 | MINOR | `x-permission` | 12 operations use null and 1 uses `none`, with no stated difference. | Use null only, and define it as "authenticated, no permission check". Mechanical. |
| INT-45 | MINOR | Routes | Conventions ask for singular, yet `documents`, `punches`, `movements`, `respondents`, `suggestions`, `roles`, `recovery-codes` are plural. Also `/employee/{id}/documents` vs `/document/{id}`. | Rename to singular, or add an exception to the conventions. |
| INT-46 | MINOR | `PayrollRun_Approve`, `PayrollRun_Recalculate` | The approver has no reject or return route. Only the accountant's recalculation cancels a request. May be intended; I did not check it against the document. | Confirm with the owner, and add `PayrollRun_ReturnForChanges` if needed. |
| INT-47 | MINOR | Enums | 119 values are not named in any operation text or example. Most are data values such as `AuditActionType` or `EmployeeDocumentType`. Status values worth stating: `ComplaintStatus.pendingDepartmentManager`, `MemoStatus.convertedToIncident`, `AttendanceImportStatus.processing/failed`, `LeaveBalanceEffect.deducted/released`, `ComplaintHandlerAssignmentStatus.revoked`, `DelegationStatus.expired`. The scan is literal (see limits). | Name the English value in the transition text. |
| INT-48 | MINOR | `Accept-Language` on all 308 operations | An inline enum on every operation adds a typed parameter to every generated method, and a device locale like `ar-SA` is out of range. The server also needs its supported cultures set **[not run]**. | Set it in one Dio interceptor, and document the fallback for unknown values. Configure `ar` and `en` with `ar` as default on the server. |
| INT-49 | MINOR | `info.description`; 4 lint warnings | Cites `api-contract/README.md`, which does not exist. | Add the file or remove the reference. Set `info.license` or filter the rule. Mechanical. |
| INT-50 | MINOR | 20 `no-ambiguous-paths` warnings | No runtime conflict (see routing above), but the contract does not state the constraint. | State that all uuid path parameters use `:guid`, and filter the rule in CI. |
| INT-51 | MINOR | `DeviceUserMap_Create`, `DeviceUserMap_List.employeeId` | `systemAdmin` is allowed but excluded from `Employee_Lookup` and `Employee_List`. The only path is indirect, through `UserAccount_List` `employee`, and only for employees with accounts. | Add `systemAdmin` to `Employee_Lookup`, and update its description. |
| INT-52 | MINOR | `leaveYearId` filter on `LeaveLedger_GetMy`, `LeaveLedger_GetByEmployee` | No route lists an employee's leave years. Ids come only from the current balance or from existing ledger rows. | Add a list route, or replace the filter with `yearStartDate`. |

### Generator readiness, in short

No construct in this contract is a hard break by static inspection: no digit-leading property names, no `oneOf`, no collisions between operationIds or tags. The risks are INT-15 (arrays of binary), INT-28 (nullable references) and INT-40 (keyword enum values). `format: decimal` probably maps to Dart `num`, which is a double, so check money precision in the smoke test **[not run]**. Stripping the `X_` prefix from operationIds and grouping by tag would produce 25 method-name collisions, so keep the full operationId as the method name.
