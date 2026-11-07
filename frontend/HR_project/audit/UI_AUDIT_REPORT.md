# UI_AUDIT_REPORT.md

# Nexus HR final UI/UX quality audit

Artifact: https://claude.ai/artifact/Lq1DkrBnkkD9RiojumasKA. First audit: 5 Nov 2026 (release 1). Updated: 6 Nov 2026 after release 2 (store and custody, evaluation, circulars) and release 3 (contracts, warnings, appreciation letters, accounting) were built. GPS attendance (release “later”) is intentionally not built.

## 1. What was actually run, and what was not

**Executed in this audit (real runs):**

- Static analysis of the final artifact source: operation wiring against all 307 contract operations, accessibility lint, CSS/RTL/theme lint, template-hole and markup-balance checks.
- 20 logic harnesses (Node, state-machine level, no rendering): 2,021 checks, all passing (h1 38, h2 82, h3 31, h4 106, h5 146, h6 219, h7 11, h8 161, h9 18, h10 135, h11 100, h12 191, h13 72, h14 167, h15 74, h16 151, h17 62, h18 110, h19 113, h20 34).
- Schema validation of the mock traffic those harnesses produced: 4,421 successful calls over 299 operations, 0 violations in 3,834 checked requests and responses (545 of 1,291 calls had violations before the first audit fixes).
- Contrast ratios computed from the token values; Arabic/English key parity computed from the string tables; token values compared with the design-system specification.

**Not run (no browser, screen reader or real backend was available):** visual rendering in any theme or direction, comparison with the light/dark reference images, responsive layouts, animation playback, keyboard and screen-reader walk-throughs, real API calls, real file upload or download. Items that depend on these are marked **Not Tested**, never Pass.

## 2. Status and layer definitions

- **Pass**: implemented and verified by an executed check (static or logic harness).
- **Partial**: implemented with a documented gap.
- **Fail**: expected, release-1 scope, and absent or wrong.
- **Not Tested**: not built (later release) or dependent on checks that could not be run.
- **Fully implemented UI**: the screen and its interactions are coded.
- **Interactive mock behavior**: server rules simulated in the in-page adapter (state persists across users).
- **Real API integration**: none.

## 3. Headline numbers

| Measure | Result |
|---|---|
| Contract operations wired in the UI | 290 of 307 (release 1: 167 of 175, release 2: 53 of 56, release 3: 70 of 70; GPS later: 0 of 6) |
| Release-1 operations not built | 0 |
| Screens (132) | Pass 123 · Partial 8 · Fail 0 · Not Tested 1 |
| Requirement units (596, section-level) | Pass 184 · Partial 117 · Fail 0 · Not Tested 295 |
| User flows (27) | Pass 26 · Partial 1 · Fail 0 · Not Tested 0 |
| Mock vs contract schemas | 0 violations in 4,421 recorded calls (299 operations) |
| Logic checks | 2,021 passing in 20 suites |

## 4. Fixes applied during this audit

| # | Problem found | Evidence | Fix |
|---|---|---|---|
| 1 | Mock ids were not valid UUIDs after id 99 (13-digit last group) | Schema validation: 161+ violations | Last group padded to 12 digits |
| 2 | ApplicationConfiguration_Get lacked localization, auth, setting | Validation | Added the required objects |
| 3 | Attendance list responses lacked the required period summary | Validation | Summary computed from the days |
| 4 | Light-theme accent text 3.02:1, light success 3.77:1, dark error 3.89:1 | Contrast calculation | New text tokens; 30 text uses switched |
| 5 | Template form controls had no accessible name (8 controls) and no aria-invalid | Lint | aria-label and aria-invalid added |
| 6 | Table headers lacked scope | Lint (8) | scope=col |
| 7 | Escape did not close dialogs/overlays | Source search | Key handler on dialogs and showcase overlays |
| 8 | lang attribute fixed to Arabic while UI switched to English | Source | lang follows the language |
| 9 | Top-bar search did nothing | Source | Wired to Employees search for roles that can open it; hidden otherwise |
| 10 | Notification bell showed a fixed badge “4” | Source | Badge removed; sample list labelled |
| 11 | Dead “new employee” modal markup left in the template | Source | Removed |
| 12 | Assign employees to a manager (S-EMP-06) had no UI although the mock supported it | Wiring analysis | Dialog on a department manager’s file; tested |
| 13 | Payroll KPI/card shown to every role | Review against the finance-role rule | Shown only to HR, executive, accountant |
| 14 | Dashboard employee and pending-leave KPIs contradicted module data | Review | Read the shared mock |
| 15 | No skip link; main navigation unlabelled; tabs/nav lacked current state | Lint | Skip link, nav label, aria-current |
| 16 | Spinner border hard-coded white | CSS lint | currentColor |
| 17 | Reports and reviewer dashboard missing (12 operations) | Wiring analysis | Module built after the audit (h10, 135 checks) |
| 18 | Notifications missing; bell was a fake sample | Wiring analysis | Real notifications, device-token registration, bell and page (h11) |
| 19 | Reminder settings and settings hub sections missing | Wiring analysis | Reminders screen; hub driven by SystemSettings_ListSections (h11) |
| 20 | Audit log missing | Wiring analysis | Audit log screen with append-only, value-free entries (h11) |
| 21 | Evaluation draft-note id and store item ids were not valid UUIDs | Schema validation during release 2 | Ids made hexadecimal |
| 22 | Employee phone number, date of birth and address dropped by the mock on create/update | Coverage harness h20 | Stored and returned |
| 23 | 21 UI-wired operations had never been called by any harness | Validator coverage list | New harness h20 (34 checks); all now schema-validated |
| 24 | The old “Payroll” placeholder page collided with the new payslip page | Nav review | The page key is reused for payslips; label changed to “Payslip” |
| 25 | Audit log recorded no financial reads | Requirement 5.16 (every financial read audited) | Value-free “read” and “export” entries for all finance operations |
| 26 | Audit page counted operation ids inside role tables as wiring | Static analysis | Role maps and route cases are excluded from the wiring scan |

## 5. Findings by audit category

Columns: Identifier · Expected behavior · Implemented behavior · Status · Evidence · Issue · Required fix. Full per-requirement, per-screen and per-operation tables are in the three COVERAGE_MATRIX files.

| Identifier | Expected behavior | Implemented behavior | Status | Evidence | Issue | Required fix |
|---|---|---|---|---|---|---|
| A-01 | All 175 release-1 operations reachable from the UI. | 167 of 175 wired in the UI (95%); 0 not built. | Partial | Static wiring analysis of the final source. | All release-1 modules are now built. Only the identity server’s discovery document and the device-ingestion endpoint have no UI, because they are not front-end calls. | Integrate them on the backend side. |
| A-02 | Release 2 and 3 operations reachable from the UI. | Release 2: 53 of 56 wired (the other 3 are get-by-id duplicates of list rows: Item_Get, StockIssue_Get, Custody_Get). Release 3: 70 of 70 wired. GPS attendance (6, later) is not built by decision. | Pass | Static wiring analysis of the final source; schema validation (all release-2 and release-3 operations were called by the harnesses). | Evaluation, circulars, store and custody (R2); contracts, warnings, appreciation letters, payroll and finance (R3) are all built as screens over the in-page mock. | Integrate with the real API (see DEVELOPER_HANDOFF). |
| A-03 | Every §10 route in the requirements maps to an operation. | 104 of 104 route rows match a contract operation exactly (method + path). | Pass | Automated match in this audit. | — | — |
| A-04 | Functional sections §3–§6 satisfied. | Section-level inference: Pass 184, Partial 117, Fail 0, Not Tested 295 of 596 units. | Partial | COVERAGE_MATRIX_REQUIREMENTS.md | Not verified line by line; only §10 is one-to-one. | Run a line-by-line review with the product owner. |
| B-01 | Mock requests and responses conform to the OpenAPI schemas. | 4,421 recorded successful calls across 299 operations validated: 0 violations in 3,834 checked requests and responses (545 of 1,291 calls had violations before the first round of audit fixes). | Pass | Validator run over traffic recorded from all 20 test suites (required fields, types, enums, formats, unknown keys). | Conformance only; business semantics are not proven by the schema. | Re-run against the real API. |
| B-02 | Defects found by that validation and coverage work. | Eight defect classes fixed so far: (1) malformed UUIDs once ids passed 99; (2) ApplicationConfiguration_Get missing localization/auth/setting; (3) attendance list responses missing the required summary; (4) a draft-note id that was not a UUID (evaluation); (5) store item ids that were not hexadecimal; (6) the employee form’s phone number, date of birth and address were silently dropped by the mock on create and update (found when the coverage harness h20 exercised Employee_Update); (7) a wrong enum value and an unknown query key in new test inputs (caught by the validator); (8) the payroll nav placeholder collided with the new payslip page. | Pass | Validator output before/after each fix; h20 (34 checks). | Fixed. | — |
| B-03 | No operation id or path invented. | 264 operation ids used by UI call sites and 274 mock routes were checked: 0 UI calls and 0 mock routes use an operation that is not in the contract. | Pass | Static check against the 307 contract operations (regex over call/act/tr call sites and route cases). | — | — |
| B-04 | UI-wired operations exercised by the harness. | All 290 UI-wired operations were called by the harnesses and schema-validated. | Pass | Validator coverage list (h20 was added to close the previous gap of 21 operations). | — | — |
| B-05 | Contract error codes have user-facing messages. | All 200 error codes reachable from UI-wired operations have an Arabic and English message (0 missing: none). The string tables hold 219 Arabic and 219 English codes. | Pass | Static check of the string tables against the contract error lists. | Messages were authored by the assistant and are not owner-approved. | Owner review of message wording. |
| B-06 | Server-side sorting/paging. | Paging (skipCount/maxResultCount) is wired in lists; the sorting parameter is never sent. | Partial | Static search: 0 uses of sorting. | Column sorting is not available on contract-backed lists. | Add sort controls that send sorting. |
| B-07 | Idempotency-Key on creates. | 31 contract operations accept Idempotency-Key; the UI never sends it (the mock ignores it). | Partial | Static search. | Duplicate submits could create duplicates on the real API. | Generate a key per submit in the API client wrapper. |
| B-08 | Real API integration. | None. Every call goes to the in-page mock adapter that mirrors the contract. | Not Tested | Architecture. | No real network behaviour (latency, auth refresh, 401/5xx) was exercised. | Swap the adapter for a real client (see DEVELOPER_HANDOFF). |
| C-01 | All 132 catalogued screens. | Pass 123, Partial 8, Fail 0, Not Tested 1 (the one Not Tested screen is GPS attendance, “later”). | Partial | COVERAGE_MATRIX_SCREENS.md | No release 1, 2 or 3 screen is missing. Partials are listed in the screens matrix. | Close the partial items. |
| C-02 | Dialogs, drawers and bottom sheets as specified. | Module dialogs are centered dialogs; drawer-type screens are dialogs or sub-pages. Bottom sheets on mobile are not implemented. | Partial | Static markup review. | Deviation from the Phase 2 component spec. | Add drawer and bottom-sheet variants if the owner wants strict parity. |
| C-03 | Settings hub lists all sections. | The hub is driven by SystemSettings_ListSections: 15 sections with owner role and editable flag. Every section now has a screen (reminders, contracts, finance, evaluation criteria included); a card opens only where the caller can go (system administrator: 14 sections, HR manager: 4). | Pass | h11 and the section checks in h15, h18, h19 (hub rendering for HR and the system administrator). | — | — |
| D-01 | 27 user flows from Phase 2. | Pass 26, Partial 1, Fail 0, Not Tested 0. Not Pass: F-04 Leave request (hourly, from balance, other) (LeaveRequest_Get). | Partial | Flow-to-operation analysis (flows.json). | The partial flow uses LeaveRequest_Get and the attachment download, which have no UI control (the list row already carries the details). | Wire the leave details-by-id and attachment download. |
| D-02 | Cross-role flows work end to end. | Employee submit → manager → HR → final effect on leave balance and attendance verified with persistent mock data; executive path for HR requests; accountant execution. | Pass | Harness h7 (11 checks) and h6 (219 checks). | Logic level only. | — |
| D-03 | F-04 Leave request flow. | Preview, create, attachments, balance effect work; request detail by id and attachment download URL are not wired. | Partial | h5 (146 checks). | LeaveRequest_Get and GetAttachmentDownloadUrl unused. | Wire attachment download in the details dialog. |
| E-01 | Client-side validation (required, pattern, length, min/max). | Implemented in the shared form system; invalid fields now also set aria-invalid and show the message. | Pass | h9 checks the aria-invalid path; module suites check required fields and patterns. | Cross-field rules rely on the server. | — |
| E-02 | Server error states. | Operation errors show an alert with the mapped message; 403 gates route to the mandatory screen; hidden resources show a neutral “not found”. | Pass | Module suites (error codes), h8 (404 semantics). | — | — |
| E-03 | Full-page error/retry and 409 conflict dialog. | Alert banner only. | Partial | Source review. | No retry card or reload-and-compare dialog (S-STATE-02/03). | Add both states. |
| E-04 | Empty states and loading states. | Every table has an empty text. Loading uses a busy flag; skeletons are not implemented. | Partial | Source review; visual check not run. | Loading appearance unverified. | Visual QA. |
| F-01 | Navigation follows the contract role map. | Nav items shown only for allowed roles; hidden items verified per role. | Pass | h2, h4, h5, h6, h8 assert nav/tab sets for each persona. | — | — |
| F-02 | Operation role gates come from the contract. | Role letters are generated from the OpenAPI x-allowed-roles; the mock returns 403 (or 404 for complaints, warnings, contracts, letters and custody where the contract says so) accordingly. | Pass | Generated constants OPR2–OPR16; role tests in every suite (each finance operation is probed with every non-accountant persona in h18/h19). | — | — |
| F-03 | Complaint isolation. | 404 for respondent, system admin, ex-handler and non-handlers on every id-based operation; empty list for non-handlers; no complaint category in the dashboard. | Pass | h8 (161 checks). | Backend isolation (queries, encryption, logging) not testable here. | Security test on the real backend. |
| F-04 | Finance-flavoured dashboard widgets only for allowed roles. | Payroll KPI and payroll card now hidden for roles other than HR, executive and accountant. | Pass | Fixed in this audit; h9. | The widgets remain demo data. | Replace with real cards later. |
| F-05 | Direct user permissions affect the UI. | The permissions editor works against the mock, but granted/revoked permissions do not change what screens show; visibility derives from roles. | Partial | Source review. | UI ignores per-user permission grants. | Use grantedPolicies from ApplicationConfiguration when integrating. |
| F-06 | Personas cover every role combination. | Each persona holds exactly one role. Accountant, store officer, reviewer, HR, executive and department managers therefore do not hold the employee role. The mock lets every user read their own payslip (the contract lists those operations under “employee” only) and lets managers act on their own custody and contracts as the contract describes. | Partial | Role matrix review; h19 (payslips for each persona). | In production these users probably hold the employee role as well. | Confirm with the owner and add multi-role personas. |
| F-07 | Mandatory MFA for assigned complaint handlers. | Not simulated; MFA requirement follows roles only. | Partial | Source review. | Contract requires MFA for an approved handler. | Enforce in the real identity layer. |
| G-01 | RTL/LTR via logical properties. | 0 physical left/right/margin-left/padding-right rules in the CSS; tooltips use sign variables. | Pass | Static CSS lint. | Visual mirroring not seen. | Visual QA in both directions. |
| G-02 | Language attribute and direction. | dir follows the language; the lang attribute on the app root now follows too (it was fixed to Arabic). | Pass | Fixed in this audit; h9. | — | — |
| G-03 | Translation completeness. | Arabic and English tables have identical key sets: T 75/75, T2 141/141, T4 1924/1924; no empty strings. | Pass | parity.js (executed). | Wording quality is the assistant’s draft; design-system showcase strings were not part of the parity check. | Owner/linguist review. |
| G-04 | Every page renders in both languages. | Render loops over every relevant persona in Arabic and English exist in the suites for shifts and attendance (h4), leave (h5), requests (h6), complaints (h8), reports (h10), notifications and settings (h11), evaluation (h12), circulars (h13), store and custody (h14), contracts (h15), warnings (h16), appreciation letters (h17) and finance (h18, h19). Authentication (h1), identity and organization (h2) and the design-system showcase (h3) have no such loop. | Partial | Render loops in 14 suites. | Crash test only, not a visual check; three suites lack the loop. | Add the same render loop to h1, h2 and h3. |
| G-05 | Mixed-direction content (emails, codes, numbers). | Not checked. | Not Tested | — | Bidi isolation unverified. | Visual QA. |
| H-01 | Design System V3 tokens. | Dark and light background, card, text, secondary text and accent tokens equal the specification. | Pass | Automated token comparison. | — | — |
| H-02 | Contrast (WCAG AA). | Measured: dark text 13.98:1 on cards; light accent text was 3.02:1 and light success 3.77:1, dark error 3.89:1 (all failing). Added accessible text tokens: light accent 5.30, light success 5.48, dark error 5.29. Primary button text is dark slate (5.91:1 light). | Pass | Computed contrast ratios (formula run in this audit). | Measured against token values, not pixels; disabled/placeholder states not measured. | — |
| H-03 | Match to the official light and dark reference images. | No pixel or visual comparison was possible. | Not Tested | — | Fidelity to the references is unverified. | Side-by-side review in a browser. |
| I-01 | Product name. | “HR” everywhere; the only “Nexus” left is the demo company “Nexus Co.”. | Partial | Source search. | Demo company name. | Rename the demo company. |
| I-02 | Official interlocking H/R logo. | Temporary placeholder; the original asset was never supplied. | Fail | Source review. | Logo is not the official asset. | Provide the SVG/PNG. |
| I-03 | Final typeface. | Cairo is used; Cairo vs Tajawal is undecided. | Partial | Source review. | Open owner decision. | Decide. |
| J-01 | Responsive behaviour. | Breakpoints at 1100 px and 760 px; mobile drawer navigation exists. | Not Tested | CSS inspection only. | No viewport was rendered, so wrapping, overflow and touch targets are unverified. | Test at 360, 768, 1280 and 1920 px. |
| K-01 | Reduced motion. | prefers-reduced-motion rules exist in the CSS. | Pass | Static CSS lint (4 blocks). | — | — |
| K-02 | Animation and micro-interaction quality. | 16 patterns in the design-system showcase and page/row animations are coded; their playback and timing were not observed. | Not Tested | Logic harness h3 only. | No frame-level check. | Visual QA. |
| L-01 | Accessible names on controls. | All 92 buttons have text or aria-label; all form controls now have aria-label (previously 8 template controls lacked names); table headers have scope=col. | Pass | Static lint before/after. | — | — |
| L-02 | Keyboard dismissal. | Escape closes module dialogs and showcase overlays when focus is inside (it was not implemented). | Partial | Fixed in this audit; h9 exercises the dialog handler (the showcase-overlay handler was not exercised). | No focus trap, no initial focus, no focus return to the opener. | Add focus management to the dialog component. |
| L-03 | Skip link and landmarks. | Skip link and a labelled main navigation added; main, nav, aside, header landmarks exist. | Pass | Static check. | — | — |
| L-04 | Tabs and navigation state. | Active tab and nav item expose aria-current; tabs are buttons, not a full tablist with arrow keys. | Partial | Fixed in this audit. | No roving tabindex/arrow-key pattern. | Implement the tablist pattern. |
| L-05 | Screen-reader behaviour. | Toasts use aria-live; errors use role=alert. No screen reader was run. | Not Tested | — | Announcement order unverified. | Test with NVDA/VoiceOver. |
| L-06 | Keyboard walk-through of each screen. | Not performed (no browser). | Not Tested | — | Tab order unverified. | Manual keyboard pass. |
| M-01 | One consistent demo dataset across modules. | A shared mock (employees, shifts, leave, requests, complaints) persists across users and sign-outs; cross-module effects verified; reset button added. | Pass | h7, h6. | — | — |
| M-02 | Identifier and date formats. | Ids were malformed UUIDs after id 99; fixed. Dates and times are generated relative to today. | Pass | Schema validation (0 violations). | — | — |
| M-03 | Dashboard numbers consistent with module data. | Employee count and pending-leave KPIs now read the shared mock; activity feed, charts, payroll and “present today” remain static demo data. | Partial | Fixed in part; h9. | Static demo values can contradict module data. | Replace or remove demo widgets. |
| N-01 | Top-bar search does something. | It was a dead input after the demo employee page was removed; now visible only to roles that can open Employees and it navigates there with the search filter. | Pass | Fixed in this audit; h9. | — | — |
| N-02 | Notifications bell. | It showed a fixed “4” badge and sample items. It is now real: unread count from Notification_GetUnreadCount, latest items from Notification_List, mark-read on click, routing by template code (including the owner/handler fallback for secure items), and a full notifications page. | Pass | h11 (100 checks). | Live SignalR delivery and push are simulated; the real hub and FCM are backend dependencies. | Integrate the SignalR hub. |
| N-03 | Navigation items without a module. | Documents and recruitment open an “in design” page (no contract operations back them; recruitment is hidden for every role). All other navigation items open real pages. | Partial | Source review. | Honest placeholders, not functional. | Build or hide. |
| N-04 | File upload. | Upload controls (leave, complaints, warnings, appreciation letters, finance adjustments and expenses, employee documents) send the file name and size to the mock; no file content is read or stored. Exports (reports, expenses, payroll, finance reports) return a file name and size, not a file. | Partial | Source review. | Real upload, scan, encryption and generation are untested. | Integrate multipart upload and binary download. |
| N-05 | Sign out. | UI-only (clears the mock session). | Partial | Source review. | No token revocation. | Call the real sign-out. |
| O-01 | Visual consistency across modules 2–6. | All use one block system (page header, tabs, filters, tables, KPI cards, forms, dialogs). | Not Tested | Source structure only. | Consistency is by construction; visual result not seen. | Visual QA. |
| O-02 | Older screens vs new shell. | The dashboard page does not use the page header/breadcrumb pattern of the newer pages. | Partial | Source review. | Inconsistent chrome. | Bring the dashboard into the page-header pattern. |
| P-01 | Financial data visible to the accountant only; every read audited. | Salary profiles, adjustments, expenses, suggestions, payroll runs, payslips and finance reports are served to the accountant (and, for runs, to the configured approver). Every other role gets 403 on every finance operation. Each read is recorded in the audit log as a value-free “read” entry; exports are recorded as “export”. The audit log was checked for salary values: none. | Pass | h18 (110) and h19 (113). | Backend enforcement, encryption at rest and MFA for the accountant on the real identity server are not testable here. | Security test on the real backend. |
| P-02 | Payroll workflow rules. | One run per period; draft → approval → close; stale runs cannot be sent for approval; the approver must hold the configured role and cannot be the preparer; approval fixes the period’s entries and locks the period; close publishes payslips with notifications that carry no amounts; returned runs stay draft. | Pass | h19. | Calculation formulas (gross = base + allowances; net = gross + additions − deductions; daily rate for suggestions = base ÷ 30) and the default currency IQD are assumptions of the mock. | Owner approval of the formulas and currency. |
| P-03 | Store and custody rules. | Stock moves only on delivery; consumables never create custody; costs are entered on receipt, never returned and become a purchase expense for the accountant; counts change neither custody nor stock; the store officer has no Request_Approve call. | Pass | h14 (167). | Real stock concurrency and costing are backend concerns. | Integration tests. |
| P-04 | Contracts, warnings and appreciation letters. | Two documented steps for contract renewal; the employee never sees the recommendation; warnings follow recommendation → notice → response → decision → objection; executive sees only objected warnings; managers see only effective warnings of their team; letters are issued by the executive only and acknowledged once. | Pass | h15 (74), h16 (151), h17 (62). | The 5-working-day objection windows are shown in the dialogs but not enforced (the contract defines no error code). | Add the error code to the contract or enforce server-side. |
| P-05 | Evaluation and circulars. | Evaluation cycle, verbal grades, isolated internal notes, objection decisions, monthly department reports that freeze circular, custody and evaluation figures; circular recipients fixed at sending with one acknowledgement each. | Pass | h12 (191), h13 (72). | HR cannot adjust a grade through the contract (the decision operation has no score field). | Contract decision. |
| P-06 | Contract gaps discovered while building release 2 and 3. | (1) The executive director can send circulars but has no sent list or receipts. (2) A returned payroll run keeps its reason but the accountant has no field, notification or operation to read it. (3) Payslip operations are restricted to the employee role although managers and others need them. (4) Evaluation decision has no score field. (5) No error codes exist for the 5-working-day objection windows. (6) Salary-suggestion amounts need an owner-approved formula. (7) Default currency and hosting country are undecided. | Partial | Notes in the module reports and UNRESOLVED_ISSUES.md. | Behaviour was implemented to the contract; the requirements and the contract disagree or are silent here. | Owner/backend decision, then contract update. |

Category key: A requirements coverage · B API mapping · C screens · D workflows · E validation/errors · F roles · G RTL/LTR and language · H themes · I brand · J responsive · K animation · L accessibility · M demo data · N missing or non-functional interactions · O visual consistency.

## 6. What is what

| Layer | Content |
|---|---|
| Fully implemented UI | Authentication gates, profile/security, employees, accounts, roles/permissions, departments, delegations, shifts, holidays, attendance, devices, import, leave, requests/memos/fault reports, complaints and handler assignments, reports and reviewer dashboard, notifications, reminder settings, settings hub, audit log, evaluation and monthly department report, circulars, store and custody, contracts, warnings, appreciation letters, salary profiles, adjustments and attendance suggestions, expenses, payroll runs, payslips, finance reports and finance settings, design-system showcase. |
| Interactive mock behavior | All server rules above: approval matrix, leave arithmetic and balance ledger, complaint routing and access policy, delegation effects, attendance generation, evaluation cycles, stock and custody movements, contract and warning lifecycles, payroll calculation and locking, validation and error codes. State persists across users until reset. |
| Real API integration | None. |
| Untested functionality | Visual rendering, themes against references, responsive layouts, animation playback, keyboard and screen-reader behaviour, real uploads/downloads and exports. |
| Missing backend dependencies | OIDC identity server and token endpoint, ABP application configuration, database and migrations, encrypted file storage with antivirus, notification delivery (in-app, email, push), fingerprint-device ingestion and import parsing, scheduled jobs (leave year rollover, reminders, late-complaint reminders, contract alert window, warning response window, cycle opening), audit log, report, payroll and finance export generation, payslip PDF generation, stock costing and expense creation, hosting country/currency/biometric model decisions. |

## 7. Coverage by module

| Module | Release | Operations | UI-wired | Mock-only | Not built / later | Screens Pass | Partial | Fail | Not built (R2/R3/later) |
|---|---|---|---|---|---|---|---|---|---|
| Authentication & session | 1 | 14 | 13 | 0 | 1 | 5 | 3 | 0 | 0 |
| Identity & organization | 1 | 36 | 35 | 1 | 0 | 19 | 1 | 0 | 0 |
| Requests & workflow | 1 | 31 | 30 | 1 | 0 | 17 | 0 | 0 | 0 |
| Shifts & attendance | 1 | 32 | 29 | 2 | 1 | 12 | 0 | 0 | 0 |
| GPS attendance (later) | later | 6 | 0 | 0 | 6 | 0 | 0 | 0 | 1 |
| Leave | 1 | 18 | 16 | 2 | 0 | 7 | 1 | 0 | 0 |
| Store & custody | 2 | 22 | 19 | 3 | 0 | 9 | 0 | 0 | 0 |
| Complaints | 1 | 19 | 19 | 0 | 0 | 7 | 0 | 0 | 0 |
| Evaluation & monthly report | 2 | 28 | 28 | 0 | 0 | 6 | 0 | 0 | 0 |
| Reports | 1,3 | 14 | 14 | 0 | 0 | 4 | 0 | 0 | 0 |
| Circulars | 2 | 6 | 6 | 0 | 0 | 3 | 0 | 0 | 0 |
| Contracts, warnings & appreciation | 3 | 29 | 29 | 0 | 0 | 12 | 2 | 0 | 0 |
| Accounting | 3 | 35 | 35 | 0 | 0 | 9 | 0 | 0 | 0 |
| Notifications | 1 | 5 | 5 | 0 | 0 | 0 | 0 | 0 | 0 |
| Settings & audit | 1,3 | 12 | 12 | 0 | 0 | 7 | 0 | 0 | 0 |
| Home & notifications | 1 | 0 | 0 | 0 | 0 | 6 | 1 | 0 | 0 |

## 8. User flows

| Flow | Name | Status | Operations not wired |
|---|---|---|---|
| F-01 | First sign-in with mandatory steps | Pass |  |
| F-02 | Create employee and account | Pass |  |
| F-03 | Change manager and bulk-assign employees | Pass |  |
| F-04 | Leave request (hourly, from balance, other) | Partial | LeaveRequest_Get |
| F-05 | Approve or reject a request from the inbox | Pass |  |
| F-06 | Submit a request on behalf of an employee | Pass |  |
| F-07 | Punch correction | Pass |  |
| F-08 | Delegation | Pass |  |
| F-09 | Memo to request, fault report, or closure | Pass |  |
| F-10 | Fault report | Pass |  |
| F-11 | Store item request to custody | Pass |  |
| F-12 | Direct custody, return and count | Pass |  |
| F-13 | Submit a complaint | Pass |  |
| F-14 | Handle and close a complaint | Pass |  |
| F-15 | Appoint a complaint handler | Pass |  |
| F-16 | Monthly evaluation, acknowledgement and objection | Pass |  |
| F-17 | Monthly department report and deadlines | Pass |  |
| F-18 | Circular send and acknowledge | Pass |  |
| F-19 | Contract expiry | Pass |  |
| F-20 | Warning lifecycle | Pass |  |
| F-21 | Appreciation letter | Pass |  |
| F-22 | Monthly payroll run | Pass |  |
| F-23 | Adjustments and attendance suggestions | Pass |  |
| F-24 | View and export a report | Pass |  |
| F-25 | Reset MFA after a lost device | Pass |  |
| F-26 | Attendance import and device mapping | Pass |  |
| F-27 | Set entitlement or adjust leave balance | Pass |  |

See UNRESOLVED_ISSUES.md for the prioritized list and DEVELOPER_HANDOFF.md for integration guidance.
