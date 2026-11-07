# DEVELOPER_HANDOFF.md

## 1. What this artifact is
A single interactive Design artifact (`project/Main.dc.html`) implementing the HR front end against an **in-page mock adapter** that mirrors the OpenAPI contract (v1.0.0, 307 operations). There is no real backend call anywhere. Release 1 (175 operations), release 2 (56) and release 3 (70) are built as screens; the six GPS operations (release “later”) are not. See COVERAGE_MATRIX_OPERATIONS.md for the status of every operation.

## 2. Code layout (inside the artifact script)
- `OPR2…OPR16` and `NAVR`: operation-to-roles maps generated from the contract (`x-allowed-roles`, one constant per module) and the navigation role map.
- String tables `T` (shell), `T2` (shell extras), `T4` (all module strings, merged from many blocks `T6…T19`). Arabic and English have identical keys (checked). Error messages live in `T4[lang].er2` (and the older tables) keyed by the contract error code (`Hr:Module:Code`).
- `class Component`: state machine (`page`, `tab`, `sub`, `sid`, `step`, `flt`, `fv`, `data`), loaders in `enter()` (one branch per page), view-model builders in `renderVals()` (blocks: key-value, table, form, KPI cards), dialogs through `dlgOpen(spec)`.
- Mock server: `mkDb/mk2…mk16` (datasets, created lazily on first use) and `route2…route16` (one `switch` per operation). `call(op, x)` is the only entry point; it applies role gates, access gates (password change, MFA), then dispatches to the routes. Hooks after a successful call create notifications (`ntfAfter`) and audit entries.
- Pages (`page` key): `dash, emp, att, leave, pay (payslips), dept, perf (evaluation), rep (reports), ann (circulars), inv (store), cus (custody), con (contracts), wrn (warnings), apr (appreciation), fin (finance), req, cmp, notif, aud, set, acc, team, ds`.

## 3. Replacing the mock with the real API
1. Keep the signature `call(op, x)` and generate a client from `openapi.yaml`: operation id → method + path; split `x` into path, query and body using the schema.
2. Throw `{status, code}` on failures, where `code` is the contract error code (`Hr:…`) or `forbidden`, `conflict`, `validation`. The UI already maps these.
3. Add what the mock ignores: `Idempotency-Key` (31 operations), the `sorting` query parameter, bearer tokens and refresh, multipart upload for every attachment (leave, complaints, warnings, letters, adjustments, expenses, documents), and binary download/export for every file endpoint.
4. Replace the persona picker in S-AUTH-01 with the authorization-code + PKCE flow; read `accessGate` from application configuration and `/me`.
5. Use `grantedPolicies` from application configuration to refine visibility (the demo derives visibility from roles only, with one role per persona).

## 4. Rules to preserve (the mock’s functions are the reference)
- **Complaints:** every permission failure on a complaint resource returns 404, never 403; non-handlers get an empty list; notifications are generic; the access log is append-only (`cmpCan`).
- **Finance:** only the accountant (and, for payroll runs, the configured approver) may read financial data; every other role gets 403; every read is audited as a value-free “read” entry and every export as “export”; nobody changes their own financial data; periods with an approved or closed run accept no new entries; amounts never appear in notifications (`payslipAvailable`).
- **Payroll:** one run per period; draft → approval requested → approved → closed; stale drafts cannot be sent for approval; the approver cannot be the preparer; returning a run keeps it a draft.
- **Store and custody:** stock moves only on delivery, adjustment or return; a purchase cost is entered on receipt and is never returned by any operation; consumables never create custody; counts change neither custody nor stock; the store officer executes requests through the stock-issue flow, not `Request_Approve`.
- **Contracts, warnings, letters:** the employee never sees contract recommendations or the recommender of a warning; the executive director sees only warnings with an objection; managers see only effective warnings and letters of their own team; hidden resources return 404.

## 5. Design tokens and CSS
Theme blocks `.app[data-th=dark|light]` define the Design System V3 values plus accessible text tokens `--act`, `--ert`, `--okt` (use these for small text, not `--ac/--er/--ok`). All spacing/direction uses logical properties and the `--s` sign variable; `prefers-reduced-motion` disables animation. The design-system showcase page shows every pattern.

## 6. Tests and audit tooling (in `tools/`)
- `h1.js … h20.js`: Node harnesses that load the component script with a stub base class and drive the state machine (no DOM). 2,021 checks in 20 suites (h20 closes the validation-coverage gap for operations the other suites do not call).
- `validate.py`: validates recorded mock requests/responses against the OpenAPI schemas (0 violations in 4,421 calls over 299 operations).
- `parity.js`: Arabic/English key parity. `errcodes.js`: dumps the error codes that have messages. `gen_audit.py`, `narr.py`, `gen_final.py`, `upd_audit.py`, `audit_fix*.py`: generators of the audit files and the exact fixes applied.
- The scripts expect the component script extracted into `script.js`, the recorded traffic in `rec/out_*.json` (run each harness with the recording hook), and the Phase 1/2 generator modules (`common.py`, `screens.py`, `flows.py`) in `/home/claude`.
These are logic-level tests. They do not render pixels, run a screen reader or call a real server.

## 7. Known deviations and open decisions
See UNRESOLVED_ISSUES.md (P0–P3). The most important: no visual QA was possible; the logo is a placeholder; delivery channels, files and exports are simulated; accounting formulas and the default currency are assumptions; several contract gaps (executive circular receipts, payroll return reason, payslip roles, evaluation grade adjustment, objection windows) need owner decisions.
