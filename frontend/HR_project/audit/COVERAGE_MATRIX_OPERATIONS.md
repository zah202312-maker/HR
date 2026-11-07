# COVERAGE_MATRIX_OPERATIONS.md

All 307 contract operations. “Schema-validated” means the requests the UI sent and the responses the mock returned were checked against the OpenAPI schema during the test run (required fields, types, enums, formats, unknown keys).

| Status | Count |
|---|---|
| UI-wired + interactive mock, schema-validated | 290 |
| Mock only (no UI trigger), schema-validated | 9 |
| Later (not in scope) | 6 |
| Not applicable here: OIDC discovery (auth library) | 1 |
| Not applicable here: machine-to-machine (device agent) | 1 |

| Operation | Module | Release | Roles | Screens | Status |
|---|---|---|---|---|---|
| OpenIdConfiguration_Get | Authentication & session | 1 |  | S-AUTH-01 | Not applicable here: OIDC discovery (auth library) |
| ConnectToken_Issue | Authentication & session | 1 |  | S-AUTH-01 | UI-wired + interactive mock, schema-validated |
| ApplicationConfiguration_Get | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-APP-01 | UI-wired + interactive mock, schema-validated |
| Me_Get | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-APP-01, S-ME-01 | UI-wired + interactive mock, schema-validated |
| Me_Update | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-ME-03 | UI-wired + interactive mock, schema-validated |
| Me_GetEmployeeProfile | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-ME-01 | UI-wired + interactive mock, schema-validated |
| Me_GetInboxSummary | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-HOME-01, S-REQ-04 | UI-wired + interactive mock, schema-validated |
| Me_GetAdministrativeTaskLog | Authentication & session | 1 | MGR | S-ME-04 | UI-wired + interactive mock, schema-validated |
| Account_ChangePassword | Authentication & session | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-AUTH-02, S-ME-02 | UI-wired + interactive mock, schema-validated |
| Mfa_GetStatus | Authentication & session | 1 | HR,EXE,ACC,ADM | S-AUTH-03, S-ME-02 | UI-wired + interactive mock, schema-validated |
| Mfa_StartEnrollment | Authentication & session | 1 | HR,EXE,ACC,ADM | S-AUTH-03 | UI-wired + interactive mock, schema-validated |
| Mfa_ConfirmEnrollment | Authentication & session | 1 | HR,EXE,ACC,ADM | S-AUTH-03, S-AUTH-04 | UI-wired + interactive mock, schema-validated |
| Mfa_RegenerateRecoveryCodes | Authentication & session | 1 | HR,EXE,ACC,ADM | S-AUTH-04 | UI-wired + interactive mock, schema-validated |
| UserAccount_ResetMfa | Authentication & session | 1 | ADM,EXE | S-ACC-06 | UI-wired + interactive mock, schema-validated |
| UserAccount_List | Identity & organization | 1 | HR,ADM | S-ACC-01 | UI-wired + interactive mock, schema-validated |
| UserAccount_Create | Identity & organization | 1 | HR | S-ACC-03 | UI-wired + interactive mock, schema-validated |
| UserAccount_Get | Identity & organization | 1 | HR,ADM | S-ACC-02 | UI-wired + interactive mock, schema-validated |
| UserAccount_ResetPassword | Identity & organization | 1 | HR,EXE | S-ACC-05 | UI-wired + interactive mock, schema-validated |
| UserAccount_SetRoles | Identity & organization | 1 | HR,ADM | S-ACC-04 | UI-wired + interactive mock, schema-validated |
| UserAccount_GetPermissions | Identity & organization | 1 | ADM | S-ACC-02, S-ACC-07 | UI-wired + interactive mock, schema-validated |
| UserAccount_UpdatePermissions | Identity & organization | 1 | ADM | S-ACC-07 | UI-wired + interactive mock, schema-validated |
| Role_List | Identity & organization | 1 | HR,ADM | S-EMP-03, S-ACC-03, S-ACC-04, S-ROLE-01 | UI-wired + interactive mock, schema-validated |
| Role_Create | Identity & organization | 1 | ADM | S-ROLE-01 | UI-wired + interactive mock, schema-validated |
| Role_Get | Identity & organization | 1 | ADM | S-ROLE-02 | UI-wired + interactive mock, schema-validated |
| Role_Update | Identity & organization | 1 | ADM | S-ROLE-02 | UI-wired + interactive mock, schema-validated |
| Role_Delete | Identity & organization | 1 | ADM | S-ROLE-01 | UI-wired + interactive mock, schema-validated |
| Role_GetPermissions | Identity & organization | 1 | ADM | S-ROLE-02 | UI-wired + interactive mock, schema-validated |
| Role_UpdatePermissions | Identity & organization | 1 | ADM | S-ROLE-02 | UI-wired + interactive mock, schema-validated |
| Department_List | Identity & organization | 1 | HR | S-DEP-01 | UI-wired + interactive mock, schema-validated |
| Department_Create | Identity & organization | 1 | HR | S-DEP-01 | UI-wired + interactive mock, schema-validated |
| Department_Lookup | Identity & organization | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-EMP-01, S-EMP-03, S-EMP-04 | UI-wired + interactive mock, schema-validated |
| Department_Get | Identity & organization | 1 | HR | S-DEP-01 | Mock only (no UI trigger), schema-validated |
| Department_Update | Identity & organization | 1 | HR | S-DEP-01 | UI-wired + interactive mock, schema-validated |
| Department_Delete | Identity & organization | 1 | HR | S-DEP-01 | UI-wired + interactive mock, schema-validated |
| Employee_List | Identity & organization | 1 | HR | S-EMP-01, S-EMP-06 | UI-wired + interactive mock, schema-validated |
| Employee_Create | Identity & organization | 1 | HR | S-EMP-03 | UI-wired + interactive mock, schema-validated |
| Employee_Lookup | Identity & organization | 1 | STO,HR,EXE,ACC,REV,ADM | S-EMP-03, S-EMP-05, S-EMP-10 | UI-wired + interactive mock, schema-validated |
| Employee_Get | Identity & organization | 1 | HR | S-EMP-02 | UI-wired + interactive mock, schema-validated |
| Employee_Update | Identity & organization | 1 | HR | S-EMP-04 | UI-wired + interactive mock, schema-validated |
| Employee_ChangeManager | Identity & organization | 1 | HR | S-EMP-05 | UI-wired + interactive mock, schema-validated |
| Employee_GetManagerHistory | Identity & organization | 1 | HR | S-EMP-02 | UI-wired + interactive mock, schema-validated |
| Employee_GetAdministrativeTaskLog | Identity & organization | 1 | HR | S-EMP-08 | UI-wired + interactive mock, schema-validated |
| Manager_AssignEmployees | Identity & organization | 1 | HR | S-EMP-06 | UI-wired + interactive mock, schema-validated |
| Manager_GetMyTeam | Identity & organization | 1 | MGR,ADM | S-EMP-07, S-REQ-06 | UI-wired + interactive mock, schema-validated |
| Manager_GetTeamMember | Identity & organization | 1 | MGR,ADM | S-EMP-08 | UI-wired + interactive mock, schema-validated |
| EmployeeDocument_List | Identity & organization | 1 | EMP,MGR,ADM,HR | S-EMP-09 | UI-wired + interactive mock, schema-validated |
| EmployeeDocument_Upload | Identity & organization | 1 | HR | S-EMP-09 | UI-wired + interactive mock, schema-validated |
| EmployeeDocument_GetDownloadUrl | Identity & organization | 1 | EMP,MGR,ADM,HR | S-EMP-09 | UI-wired + interactive mock, schema-validated |
| EmployeeDocument_Delete | Identity & organization | 1 | HR | S-EMP-09 | UI-wired + interactive mock, schema-validated |
| Delegation_List | Requests & workflow | 1 | HR | S-DEL-01 | UI-wired + interactive mock, schema-validated |
| Delegation_Create | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC | S-DEL-01 | UI-wired + interactive mock, schema-validated |
| Delegation_GetMy | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-DEL-01 | UI-wired + interactive mock, schema-validated |
| Delegation_GetCandidates | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC | S-DEL-01 | UI-wired + interactive mock, schema-validated |
| Delegation_Cancel | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC | S-DEL-01 | UI-wired + interactive mock, schema-validated |
| WorkShift_List | Shifts & attendance | 1 | HR,ADM | S-EMP-03, S-SHF-01, S-SHF-03 | UI-wired + interactive mock, schema-validated |
| WorkShift_Create | Shifts & attendance | 1 | HR,ADM | S-SHF-02 | UI-wired + interactive mock, schema-validated |
| WorkShift_Get | Shifts & attendance | 1 | HR,ADM | S-SHF-01, S-SHF-02 | Mock only (no UI trigger), schema-validated |
| WorkShift_Update | Shifts & attendance | 1 | HR,ADM | S-SHF-02 | UI-wired + interactive mock, schema-validated |
| EmployeeShift_Assign | Shifts & attendance | 1 | HR | S-SHF-03 | UI-wired + interactive mock, schema-validated |
| EmployeeShift_GetHistory | Shifts & attendance | 1 | HR | S-EMP-02, S-SHF-03 | UI-wired + interactive mock, schema-validated |
| Holiday_List | Shifts & attendance | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| Holiday_Create | Shifts & attendance | 1 | HR | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| Holiday_Update | Shifts & attendance | 1 | HR | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| Holiday_Delete | Shifts & attendance | 1 | HR | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| WeeklyOffSetting_Get | Shifts & attendance | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| WeeklyOffSetting_Update | Shifts & attendance | 1 | HR | S-HOL-01 | UI-wired + interactive mock, schema-validated |
| Attendance_GetMy | Shifts & attendance | 1 | EMP,MGR,HR,ADM | S-ATT-01 | UI-wired + interactive mock, schema-validated |
| Attendance_GetTeam | Shifts & attendance | 1 | MGR | S-ATT-02 | UI-wired + interactive mock, schema-validated |
| Attendance_GetByEmployee | Shifts & attendance | 1 | MGR,HR | S-ATT-03 | UI-wired + interactive mock, schema-validated |
| Attendance_GetDayDetail | Shifts & attendance | 1 | EMP,MGR,HR,ADM | S-ATT-01, S-ATT-04 | UI-wired + interactive mock, schema-validated |
| AttendanceDevice_IngestPunches | Shifts & attendance | 1 |  | — | Not applicable here: machine-to-machine (device agent) |
| AttendanceImport_Create | Shifts & attendance | 1 | HR | S-ATT-06 | UI-wired + interactive mock, schema-validated |
| AttendanceImport_List | Shifts & attendance | 1 | HR | S-ATT-06 | UI-wired + interactive mock, schema-validated |
| AttendanceImport_Get | Shifts & attendance | 1 | HR | S-ATT-06 | Mock only (no UI trigger), schema-validated |
| AttendanceSettings_Get | Shifts & attendance | 1 | EMP,MGR,HR,ADM | S-ATT-08 | UI-wired + interactive mock, schema-validated |
| AttendanceSettings_Update | Shifts & attendance | 1 | ADM | S-ATT-08 | UI-wired + interactive mock, schema-validated |
| AttendanceDevice_List | Shifts & attendance | 1 | ADM,HR | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| AttendanceDevice_Create | Shifts & attendance | 1 | ADM | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| AttendanceDevice_Update | Shifts & attendance | 1 | ADM | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| AttendanceDevice_RotateKey | Shifts & attendance | 1 | ADM | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| DeviceUserMap_List | Shifts & attendance | 1 | ADM,HR | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| DeviceUserMap_Create | Shifts & attendance | 1 | ADM,HR | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| DeviceUserMap_Delete | Shifts & attendance | 1 | ADM,HR | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| DeviceUserMap_ListUnmapped | Shifts & attendance | 1 | ADM,HR | S-ATT-07 | UI-wired + interactive mock, schema-validated |
| AttendanceCorrection_Create | Shifts & attendance | 1 | EMP,MGR,HR,ADM | S-ATT-05 | UI-wired + interactive mock, schema-validated |
| AttendanceCorrection_Get | Shifts & attendance | 1 | EMP,MGR,HR,EXE,ADM | S-ATT-05 | UI-wired + interactive mock, schema-validated |
| Attendance_CreateGpsPunch | GPS attendance (later) | later | EMP,MGR,HR,ADM | S-ATT-09 | Later (not in scope) |
| Attendance_ListFlaggedGpsPunches | GPS attendance (later) | later | HR | S-ATT-09 | Later (not in scope) |
| GeoFence_List | GPS attendance (later) | later | ADM | S-ATT-09 | Later (not in scope) |
| GeoFence_Create | GPS attendance (later) | later | ADM | S-ATT-09 | Later (not in scope) |
| GeoFence_Update | GPS attendance (later) | later | ADM | S-ATT-09 | Later (not in scope) |
| GeoFence_Delete | GPS attendance (later) | later | ADM | S-ATT-09 | Later (not in scope) |
| LeaveBalance_GetMy | Leave | 1 | EMP,MGR,HR,ADM | S-LV-01 | UI-wired + interactive mock, schema-validated |
| LeaveBalance_GetByEmployee | Leave | 1 | MGR,HR,ADM,EXE | S-LV-05 | UI-wired + interactive mock, schema-validated |
| LeaveBalance_SetEntitlement | Leave | 1 | HR | S-LV-06 | UI-wired + interactive mock, schema-validated |
| LeaveBalance_Adjust | Leave | 1 | HR | S-LV-07 | UI-wired + interactive mock, schema-validated |
| LeaveLedger_GetByEmployee | Leave | 1 | MGR,HR,ADM,EXE | S-LV-05 | UI-wired + interactive mock, schema-validated |
| LeaveLedger_GetMy | Leave | 1 | EMP,MGR,HR,ADM | S-LV-01 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_Preview | Leave | 1 | EMP,MGR,HR,ADM | S-LV-02 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_Create | Leave | 1 | EMP,MGR,HR,ADM | S-LV-02 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_ListMy | Leave | 1 | EMP,MGR,HR,ADM | S-LV-03 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_ListByEmployee | Leave | 1 | MGR,HR | S-LV-05 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_Get | Leave | 1 | EMP,MGR,HR,EXE,ADM | S-LV-03 | Mock only (no UI trigger), schema-validated |
| LeaveRequest_UploadAttachment | Leave | 1 | EMP,MGR,HR,ADM | S-LV-02 | UI-wired + interactive mock, schema-validated |
| LeaveRequest_GetAttachmentDownloadUrl | Leave | 1 | EMP,MGR,HR,EXE,ADM | S-LV-03 | Mock only (no UI trigger), schema-validated |
| OtherLeaveType_List | Leave | 1 | ADM | S-LV-08 | UI-wired + interactive mock, schema-validated |
| OtherLeaveType_Create | Leave | 1 | ADM | S-LV-08 | UI-wired + interactive mock, schema-validated |
| OtherLeaveType_Update | Leave | 1 | ADM | S-LV-08 | UI-wired + interactive mock, schema-validated |
| OtherLeaveType_ListAvailable | Leave | 1 | EMP,MGR,HR,ADM | S-LV-02 | UI-wired + interactive mock, schema-validated |
| LeaveCalendar_GetTeam | Leave | 1 | MGR | S-LV-04 | UI-wired + interactive mock, schema-validated |
| RequestType_GetList | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-REQ-01 | UI-wired + interactive mock, schema-validated |
| RequestType_Create | Requests & workflow | 1 | ADM | S-REQ-08 | UI-wired + interactive mock, schema-validated |
| RequestType_GetAdminList | Requests & workflow | 1 | ADM | S-REQ-08 | UI-wired + interactive mock, schema-validated |
| RequestType_Get | Requests & workflow | 1 | ADM | S-REQ-08 | Mock only (no UI trigger), schema-validated |
| RequestType_Update | Requests & workflow | 1 | ADM | S-REQ-08 | UI-wired + interactive mock, schema-validated |
| Request_Submit | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-REQ-01, S-REQ-06 | UI-wired + interactive mock, schema-validated |
| Request_GetList | Requests & workflow | 1 | HR,MGR | S-REQ-07 | UI-wired + interactive mock, schema-validated |
| Request_GetMy | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-REQ-02 | UI-wired + interactive mock, schema-validated |
| Request_GetInbox | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC,STO | S-REQ-04 | UI-wired + interactive mock, schema-validated |
| Request_Get | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-REQ-03 | UI-wired + interactive mock, schema-validated |
| Request_Approve | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC | S-REQ-05 | UI-wired + interactive mock, schema-validated |
| Request_Reject | Requests & workflow | 1 | MGR,HR,EXE,ADM,ACC,STO | S-REQ-05 | UI-wired + interactive mock, schema-validated |
| Request_Cancel | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-REQ-03 | UI-wired + interactive mock, schema-validated |
| Memo_Create | Requests & workflow | 1 | EMP,MGR,HR,REV,ACC,STO,ADM | S-MEM-01 | UI-wired + interactive mock, schema-validated |
| Memo_GetMy | Requests & workflow | 1 | EMP,MGR,HR,REV,ACC,STO,ADM | S-MEM-02 | UI-wired + interactive mock, schema-validated |
| Memo_GetInbox | Requests & workflow | 1 | MGR,HR,EXE,ADM | S-MEM-03 | UI-wired + interactive mock, schema-validated |
| Memo_Get | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-MEM-02, S-MEM-03 | UI-wired + interactive mock, schema-validated |
| Memo_Forward | Requests & workflow | 1 | MGR,ADM,EXE | S-MEM-04 | UI-wired + interactive mock, schema-validated |
| Memo_Convert | Requests & workflow | 1 | MGR,HR,EXE,ADM | S-MEM-04 | UI-wired + interactive mock, schema-validated |
| Memo_Close | Requests & workflow | 1 | HR,EXE | S-MEM-04 | UI-wired + interactive mock, schema-validated |
| Incident_Create | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-INC-01 | UI-wired + interactive mock, schema-validated |
| Incident_GetMy | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-INC-02 | UI-wired + interactive mock, schema-validated |
| Incident_GetInbox | Requests & workflow | 1 | MGR,HR | S-INC-03 | UI-wired + interactive mock, schema-validated |
| Incident_Get | Requests & workflow | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-INC-02, S-INC-03 | UI-wired + interactive mock, schema-validated |
| Incident_UpdateStatus | Requests & workflow | 1 | HR | S-INC-04 | UI-wired + interactive mock, schema-validated |
| Incident_Forward | Requests & workflow | 1 | MGR | S-INC-04 | UI-wired + interactive mock, schema-validated |
| Item_GetList | Store & custody | 2 | STO | S-STO-01 | UI-wired + interactive mock, schema-validated |
| Item_Create | Store & custody | 2 | STO | S-STO-01 | UI-wired + interactive mock, schema-validated |
| Item_Lookup | Store & custody | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-STO-01 | UI-wired + interactive mock, schema-validated |
| Item_Get | Store & custody | 2 | STO | S-STO-01 | Mock only (no UI trigger), schema-validated |
| Item_Update | Store & custody | 2 | STO | S-STO-01 | UI-wired + interactive mock, schema-validated |
| Stock_Receive | Store & custody | 2 | STO | S-STO-02 | UI-wired + interactive mock, schema-validated |
| Stock_Adjust | Store & custody | 2 | STO | S-STO-02 | UI-wired + interactive mock, schema-validated |
| Stock_GetMovements | Store & custody | 2 | STO | S-STO-03 | UI-wired + interactive mock, schema-validated |
| StockIssue_GetList | Store & custody | 2 | STO | S-STO-04 | UI-wired + interactive mock, schema-validated |
| StockIssue_Get | Store & custody | 2 | STO | S-STO-04 | Mock only (no UI trigger), schema-validated |
| StockIssue_Acknowledge | Store & custody | 2 | STO | S-STO-05 | UI-wired + interactive mock, schema-validated |
| StockIssue_MarkPurchasing | Store & custody | 2 | STO | S-STO-05 | UI-wired + interactive mock, schema-validated |
| StockIssue_Deliver | Store & custody | 2 | STO | S-STO-05 | UI-wired + interactive mock, schema-validated |
| Custody_Create | Store & custody | 2 | STO | S-CUS-03 | UI-wired + interactive mock, schema-validated |
| Custody_GetList | Store & custody | 2 | STO,HR | S-CUS-03 | UI-wired + interactive mock, schema-validated |
| Custody_GetMy | Store & custody | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CUS-01 | UI-wired + interactive mock, schema-validated |
| Custody_GetTeam | Store & custody | 2 | MGR | S-CUS-02 | UI-wired + interactive mock, schema-validated |
| Custody_Get | Store & custody | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CUS-03 | Mock only (no UI trigger), schema-validated |
| Custody_Acknowledge | Store & custody | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CUS-01 | UI-wired + interactive mock, schema-validated |
| Custody_Return | Store & custody | 2 | STO | S-CUS-03 | UI-wired + interactive mock, schema-validated |
| CustodyCount_Create | Store & custody | 2 | MGR | S-CUS-04 | UI-wired + interactive mock, schema-validated |
| CustodyCount_GetList | Store & custody | 2 | MGR,STO,HR | S-CUS-04 | UI-wired + interactive mock, schema-validated |
| Complaint_GetRespondents | Complaints | 1 | EMP,MGR | S-CMP-01 | UI-wired + interactive mock, schema-validated |
| Complaint_PreviewRouting | Complaints | 1 | EMP,MGR | S-CMP-01 | UI-wired + interactive mock, schema-validated |
| Complaint_Submit | Complaints | 1 | EMP,MGR | S-CMP-01 | UI-wired + interactive mock, schema-validated |
| Complaint_GetMyList | Complaints | 1 | EMP,MGR | S-CMP-02 | UI-wired + interactive mock, schema-validated |
| Complaint_GetMyDetail | Complaints | 1 | EMP,MGR | S-CMP-02 | UI-wired + interactive mock, schema-validated |
| Complaint_GetAssignedList | Complaints | 1 | MGR,HR,EXE | S-CMP-03 | UI-wired + interactive mock, schema-validated |
| Complaint_GetDetail | Complaints | 1 | MGR,HR,EXE | S-CMP-04 | UI-wired + interactive mock, schema-validated |
| Complaint_RaiseLevel | Complaints | 1 | EMP,MGR,HR,EXE | S-CMP-05 | UI-wired + interactive mock, schema-validated |
| Complaint_AddAction | Complaints | 1 | MGR,HR,EXE | S-CMP-05 | UI-wired + interactive mock, schema-validated |
| Complaint_Forward | Complaints | 1 | MGR | S-CMP-05 | UI-wired + interactive mock, schema-validated |
| Complaint_Close | Complaints | 1 | HR,EXE | S-CMP-05 | UI-wired + interactive mock, schema-validated |
| Complaint_GetAccessLog | Complaints | 1 | MGR,HR,EXE | S-CMP-06 | UI-wired + interactive mock, schema-validated |
| Complaint_UploadAttachment | Complaints | 1 | EMP,MGR,HR,EXE | S-CMP-01 | UI-wired + interactive mock, schema-validated |
| Complaint_DownloadAttachment | Complaints | 1 | EMP,MGR,HR,EXE | S-CMP-04 | UI-wired + interactive mock, schema-validated |
| ComplaintHandler_Request | Complaints | 1 | HR | S-CMP-07 | UI-wired + interactive mock, schema-validated |
| ComplaintHandler_GetList | Complaints | 1 | HR,EXE | S-CMP-07 | UI-wired + interactive mock, schema-validated |
| ComplaintHandler_Approve | Complaints | 1 | EXE | S-CMP-07 | UI-wired + interactive mock, schema-validated |
| ComplaintHandler_Reject | Complaints | 1 | EXE | S-CMP-07 | UI-wired + interactive mock, schema-validated |
| ComplaintHandler_Revoke | Complaints | 1 | EXE | S-CMP-07 | UI-wired + interactive mock, schema-validated |
| EvaluationCycle_GetCurrent | Evaluation & monthly report | 2 | MGR,HR,REV | S-EVA-01 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetTeamList | Evaluation & monthly report | 2 | MGR | S-EVA-01 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetManagerForm | Evaluation & monthly report | 2 | MGR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| Evaluation_SaveDraft | Evaluation & monthly report | 2 | MGR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| Evaluation_Submit | Evaluation & monthly report | 2 | MGR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetMyList | Evaluation & monthly report | 2 | EMP | S-EVA-03 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetMyDetail | Evaluation & monthly report | 2 | EMP | S-EVA-03 | UI-wired + interactive mock, schema-validated |
| Evaluation_Acknowledge | Evaluation & monthly report | 2 | EMP | S-EVA-03 | UI-wired + interactive mock, schema-validated |
| Evaluation_AddResponse | Evaluation & monthly report | 2 | EMP | S-EVA-03 | UI-wired + interactive mock, schema-validated |
| Evaluation_DecideObjection | Evaluation & monthly report | 2 | HR | S-EVA-04 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetHrList | Evaluation & monthly report | 2 | HR | S-EVA-04 | UI-wired + interactive mock, schema-validated |
| Evaluation_GetHrView | Evaluation & monthly report | 2 | HR | S-EVA-04 | UI-wired + interactive mock, schema-validated |
| EvaluationInternalNote_GetList | Evaluation & monthly report | 2 | MGR,HR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| EvaluationInternalNote_Add | Evaluation & monthly report | 2 | MGR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| EvaluationInternalNote_SaveDraft | Evaluation & monthly report | 2 | MGR | S-EVA-02 | UI-wired + interactive mock, schema-validated |
| EvaluationAnnualSummary_GetMy | Evaluation & monthly report | 2 | EMP | S-EVA-03 | UI-wired + interactive mock, schema-validated |
| EvaluationAnnualSummary_GetForEmployee | Evaluation & monthly report | 2 | MGR,HR | S-EVA-04 | UI-wired + interactive mock, schema-validated |
| EvaluationCriterion_GetList | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| EvaluationCriterion_Create | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| EvaluationCriterion_Update | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| EvaluationGrade_GetList | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| EvaluationGrade_Create | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| EvaluationGrade_Update | Evaluation & monthly report | 2 | ADM | S-EVA-05 | UI-wired + interactive mock, schema-validated |
| DepartmentReport_GetList | Evaluation & monthly report | 2 | MGR,HR | S-EVA-06 | UI-wired + interactive mock, schema-validated |
| DepartmentReport_Get | Evaluation & monthly report | 2 | MGR,HR | S-EVA-06 | UI-wired + interactive mock, schema-validated |
| DepartmentReport_UpdateNotes | Evaluation & monthly report | 2 | MGR | S-EVA-06 | UI-wired + interactive mock, schema-validated |
| DepartmentReport_Submit | Evaluation & monthly report | 2 | MGR | S-EVA-06 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamAttendance | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamLatenessAbsence | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamLeaves | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamPending | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamCustody | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetTeamEvaluations | Reports | 1 | MGR | S-RPT-01 | UI-wired + interactive mock, schema-validated |
| Report_GetReviewDashboard | Reports | 1 | REV | S-RPT-02 | UI-wired + interactive mock, schema-validated |
| Report_GetEvaluationConsistency | Reports | 1 | REV | S-RPT-02 | UI-wired + interactive mock, schema-validated |
| Report_GetHrAttendance | Reports | 1 | HR | S-RPT-03 | UI-wired + interactive mock, schema-validated |
| Report_GetHrLeaveBalances | Reports | 1 | HR | S-RPT-03 | UI-wired + interactive mock, schema-validated |
| Report_GetHrRequests | Reports | 1 | HR | S-RPT-03 | UI-wired + interactive mock, schema-validated |
| Report_GetHrContracts | Reports | 3 | HR | S-RPT-03 | UI-wired + interactive mock, schema-validated |
| Report_GetHrWarnings | Reports | 3 | HR | S-RPT-03 | UI-wired + interactive mock, schema-validated |
| Report_Export | Reports | 1 | MGR,REV,HR | S-RPT-04 | UI-wired + interactive mock, schema-validated |
| Circular_GetReceivedList | Circulars | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CIR-01 | UI-wired + interactive mock, schema-validated |
| Circular_Create | Circulars | 2 | MGR,HR,EXE | S-CIR-02 | UI-wired + interactive mock, schema-validated |
| Circular_GetSentList | Circulars | 2 | MGR,HR | S-CIR-03 | UI-wired + interactive mock, schema-validated |
| Circular_Get | Circulars | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CIR-01 | UI-wired + interactive mock, schema-validated |
| Circular_GetReceipts | Circulars | 2 | MGR,HR | S-CIR-03 | UI-wired + interactive mock, schema-validated |
| Circular_Acknowledge | Circulars | 2 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-CIR-01 | UI-wired + interactive mock, schema-validated |
| Contract_List | Contracts, warnings & appreciation | 3 | HR | S-CON-01 | UI-wired + interactive mock, schema-validated |
| Contract_Create | Contracts, warnings & appreciation | 3 | HR | S-CON-03 | UI-wired + interactive mock, schema-validated |
| Contract_GetExpiring | Contracts, warnings & appreciation | 3 | HR | S-CON-01 | UI-wired + interactive mock, schema-validated |
| Contract_Get | Contracts, warnings & appreciation | 3 | HR | S-CON-02 | UI-wired + interactive mock, schema-validated |
| Contract_Recommend | Contracts, warnings & appreciation | 3 | HR | S-CON-02 | UI-wired + interactive mock, schema-validated |
| Contract_Decide | Contracts, warnings & appreciation | 3 | HR | S-CON-02 | UI-wired + interactive mock, schema-validated |
| Contract_GetMyList | Contracts, warnings & appreciation | 3 | EMP | S-CON-04 | UI-wired + interactive mock, schema-validated |
| Contract_GetMy | Contracts, warnings & appreciation | 3 | EMP | S-CON-04 | UI-wired + interactive mock, schema-validated |
| Warning_Create | Contracts, warnings & appreciation | 3 | MGR,REV | S-WRN-01 | UI-wired + interactive mock, schema-validated |
| Warning_List | Contracts, warnings & appreciation | 3 | HR | S-WRN-02 | UI-wired + interactive mock, schema-validated |
| Warning_GetObjections | Contracts, warnings & appreciation | 3 | EXE | S-WRN-03 | UI-wired + interactive mock, schema-validated |
| Warning_GetRecommendedByMe | Contracts, warnings & appreciation | 3 | MGR,REV | S-WRN-05 | UI-wired + interactive mock, schema-validated |
| Warning_GetMyList | Contracts, warnings & appreciation | 3 | EMP | S-WRN-04 | UI-wired + interactive mock, schema-validated |
| Warning_GetMy | Contracts, warnings & appreciation | 3 | EMP | S-WRN-04 | UI-wired + interactive mock, schema-validated |
| Warning_GetEmployeeFile | Contracts, warnings & appreciation | 3 | MGR,HR | S-WRN-06 | UI-wired + interactive mock, schema-validated |
| Warning_Get | Contracts, warnings & appreciation | 3 | HR,EXE | S-WRN-02 | UI-wired + interactive mock, schema-validated |
| Warning_GetAttachmentDownloadUrl | Contracts, warnings & appreciation | 3 | HR,EXE | S-WRN-02 | UI-wired + interactive mock, schema-validated |
| Warning_Notify | Contracts, warnings & appreciation | 3 | HR | S-WRN-03 | UI-wired + interactive mock, schema-validated |
| Warning_Respond | Contracts, warnings & appreciation | 3 | EMP | S-WRN-04 | UI-wired + interactive mock, schema-validated |
| Warning_Decide | Contracts, warnings & appreciation | 3 | HR | S-WRN-03 | UI-wired + interactive mock, schema-validated |
| Warning_Object | Contracts, warnings & appreciation | 3 | EMP | S-WRN-04 | UI-wired + interactive mock, schema-validated |
| Warning_ResolveObjection | Contracts, warnings & appreciation | 3 | EXE | S-WRN-03 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_Create | Contracts, warnings & appreciation | 3 | EXE | S-APR-01 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_List | Contracts, warnings & appreciation | 3 | HR,EXE | S-APR-02 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_GetMyList | Contracts, warnings & appreciation | 3 | EMP | S-APR-03 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_GetEmployeeFile | Contracts, warnings & appreciation | 3 | MGR,HR,EXE | S-APR-04 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_Get | Contracts, warnings & appreciation | 3 | EMP,MGR,HR,EXE | S-APR-02 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_GetDownloadUrl | Contracts, warnings & appreciation | 3 | EMP,MGR,HR,EXE | S-APR-02 | UI-wired + interactive mock, schema-validated |
| AppreciationLetter_Acknowledge | Contracts, warnings & appreciation | 3 | EMP | S-APR-03 | UI-wired + interactive mock, schema-validated |
| SalaryProfile_List | Accounting | 3 | ACC | S-FIN-01 | UI-wired + interactive mock, schema-validated |
| SalaryProfile_Get | Accounting | 3 | ACC | S-FIN-01 | UI-wired + interactive mock, schema-validated |
| SalaryProfile_Update | Accounting | 3 | ACC | S-FIN-01 | UI-wired + interactive mock, schema-validated |
| SalaryProfile_GetHistory | Accounting | 3 | ACC | S-FIN-01 | UI-wired + interactive mock, schema-validated |
| Adjustment_List | Accounting | 3 | ACC | S-FIN-02 | UI-wired + interactive mock, schema-validated |
| Adjustment_Create | Accounting | 3 | ACC | S-FIN-02 | UI-wired + interactive mock, schema-validated |
| Adjustment_Reverse | Accounting | 3 | ACC | S-FIN-02 | UI-wired + interactive mock, schema-validated |
| Adjustment_GetAttachmentDownloadUrl | Accounting | 3 | ACC | S-FIN-02 | UI-wired + interactive mock, schema-validated |
| AdjustmentSuggestion_List | Accounting | 3 | ACC | S-FIN-03 | UI-wired + interactive mock, schema-validated |
| AdjustmentSuggestion_Approve | Accounting | 3 | ACC | S-FIN-03 | UI-wired + interactive mock, schema-validated |
| AdjustmentSuggestion_Reject | Accounting | 3 | ACC | S-FIN-03 | UI-wired + interactive mock, schema-validated |
| PayrollRun_List | Accounting | 3 | ACC,HR,EXE | S-FIN-04 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Create | Accounting | 3 | ACC | S-FIN-04 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Get | Accounting | 3 | ACC,HR,EXE | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Recalculate | Accounting | 3 | ACC | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| PayrollRun_RequestApproval | Accounting | 3 | ACC | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Approve | Accounting | 3 | HR,EXE | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Close | Accounting | 3 | ACC | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| PayrollRun_Export | Accounting | 3 | ACC,HR,EXE | S-FIN-05 | UI-wired + interactive mock, schema-validated |
| Payslip_GetMyList | Accounting | 3 | EMP | S-FIN-06 | UI-wired + interactive mock, schema-validated |
| Payslip_GetMy | Accounting | 3 | EMP | S-FIN-06 | UI-wired + interactive mock, schema-validated |
| Payslip_DownloadMyPdf | Accounting | 3 | EMP | S-FIN-06 | UI-wired + interactive mock, schema-validated |
| Expense_List | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| Expense_Create | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| Expense_Export | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| Expense_Get | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| Expense_Reverse | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| Expense_GetAttachmentDownloadUrl | Accounting | 3 | ACC | S-FIN-07 | UI-wired + interactive mock, schema-validated |
| AccountantRequest_List | Accounting | 3 | ACC | S-REQ-09 | UI-wired + interactive mock, schema-validated |
| AccountantRequest_Get | Accounting | 3 | ACC | S-REQ-09 | UI-wired + interactive mock, schema-validated |
| FinanceReport_GetMonthlyStatement | Accounting | 3 | ACC | S-FIN-08 | UI-wired + interactive mock, schema-validated |
| FinanceReport_GetAdjustments | Accounting | 3 | ACC | S-FIN-08 | UI-wired + interactive mock, schema-validated |
| FinanceReport_GetStoreExpenses | Accounting | 3 | ACC | S-FIN-08 | UI-wired + interactive mock, schema-validated |
| FinanceReport_Export | Accounting | 3 | ACC | S-FIN-08 | UI-wired + interactive mock, schema-validated |
| Notification_List | Notifications | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-NOT-01 | UI-wired + interactive mock, schema-validated |
| Notification_GetUnreadCount | Notifications | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-HOME-01, S-NOT-01 | UI-wired + interactive mock, schema-validated |
| Notification_MarkRead | Notifications | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-NOT-01 | UI-wired + interactive mock, schema-validated |
| DeviceToken_Register | Notifications | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-NOT-02 | UI-wired + interactive mock, schema-validated |
| DeviceToken_Unregister | Notifications | 1 | EMP,MGR,HR,EXE,REV,ACC,STO,ADM | S-NOT-02 | UI-wired + interactive mock, schema-validated |
| SystemSettings_ListSections | Settings & audit | 1 | ADM,HR | S-SET-01 | UI-wired + interactive mock, schema-validated |
| SystemSettings_GetReminders | Settings & audit | 1 | ADM | S-SET-02 | UI-wired + interactive mock, schema-validated |
| SystemSettings_UpdateReminders | Settings & audit | 1 | ADM | S-SET-02 | UI-wired + interactive mock, schema-validated |
| SystemSettings_GetContracts | Settings & audit | 3 | ADM | S-SET-03 | UI-wired + interactive mock, schema-validated |
| SystemSettings_UpdateContracts | Settings & audit | 3 | ADM | S-SET-03 | UI-wired + interactive mock, schema-validated |
| SystemSettings_GetComplaints | Settings & audit | 1 | ADM | S-SET-05 | UI-wired + interactive mock, schema-validated |
| SystemSettings_UpdateComplaints | Settings & audit | 1 | ADM | S-SET-05 | UI-wired + interactive mock, schema-validated |
| SystemSettings_GetHourlyLeave | Settings & audit | 1 | ADM | S-SET-04 | UI-wired + interactive mock, schema-validated |
| SystemSettings_UpdateHourlyLeave | Settings & audit | 1 | ADM | S-SET-04 | UI-wired + interactive mock, schema-validated |
| SystemSettings_GetFinance | Settings & audit | 3 | ADM | S-SET-06 | UI-wired + interactive mock, schema-validated |
| SystemSettings_UpdateFinance | Settings & audit | 3 | ADM | S-SET-06 | UI-wired + interactive mock, schema-validated |
| AuditLog_List | Settings & audit | 1 | ADM,EXE | S-AUD-01 | UI-wired + interactive mock, schema-validated |
| UserAccount_Lookup | Identity & organization | 1 | EXE,HR,ADM | S-ACC-01 | UI-wired + interactive mock, schema-validated |
| EvaluationCycle_GetList | Evaluation & monthly report | 2 | MGR,HR,REV | S-EVA-04 | UI-wired + interactive mock, schema-validated |
| PayrollRun_ReturnForChanges | Accounting | 3 | HR,EXE | S-FIN-05 | UI-wired + interactive mock, schema-validated |
