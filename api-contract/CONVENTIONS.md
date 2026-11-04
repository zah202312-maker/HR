# API Contract Conventions

These rules are mandatory for every module. The source of requirements is `متطلبات_وتصميم_النظام.md`. If the document and these rules disagree on a business rule, the document wins. If they disagree on API mechanics, these rules win.

## Format

- OpenAPI **3.0.3**, YAML, one file per module in `modules/`.
- Each module is a complete OpenAPI file with `openapi`, `info`, `paths`, and `components.schemas`.
- All shared components come from `common.yaml`. Reference them as `./common.yaml#/components/...`. Never redefine them.
- A schema used by another module is referenced as `./<module>.yaml#/components/schemas/<Name>`.
- Schema names are unique across all modules. Use a domain word in the name, for example `LeaveRequestDto`.

## URLs and methods

- Business routes start with `/api/app/`, in kebab-case and singular form (ABP convention), for example `/api/app/leave-request/{id}`.
- Use the routes in section 10 of the document. Add a route only when a requirement needs it.
- `GET` reads, `POST` creates or runs an action, `PUT` replaces, `PATCH` is partial, `DELETE` removes.
- Actions use a verb segment: `POST /api/app/request/{id}/approve`.

## JSON

- camelCase property names. Identifiers are in English.
- Ids: `string`, `format: uuid`.
- Date only: `string`, `format: date` (`2026-03-15`).
- Date and time: `string`, `format: date-time`, UTC.
- Time only: `string`, `pattern: '^([01][0-9]|2[0-3]):[0-5][0-9]$'` (`08:30`).
- Money and leave quantities: `number`, `format: decimal`.
- Enums are **strings**, camelCase, defined as named schemas. The backend must register `new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)`. See `x-server-requirements` in `openapi.yaml`.
- Every nullable field is marked `nullable: true`. Every object lists `required` accurately.
- Bilingual reference data has `nameAr` and `nameEn` for writes and admin reads, and a localized `name` in read DTOs for ordinary users.
- Updatable aggregates expose `concurrencyStamp` in read DTOs and require it in update DTOs.
- Audit fields where useful: `creationTime`, `lastModificationTime`.

## Lists

- List endpoints take `skipCount`, `maxResultCount`, `sorting` from `common.yaml`, plus explicit filters.
- List responses use a named schema `<Name>PagedResultDto` with `totalCount` (int64) and `items`.

## Security and errors

- Default security: `oauth2`. Only `POST /api/attendance-devices/punches` uses `deviceKey`.
- Every operation accepts the `Accept-Language` header parameter from `common.yaml`.
- Every operation declares these responses, referencing `common.yaml`, when applicable:
  - `400` BadRequest
  - `401` Unauthorized
  - `403` Forbidden
  - `404` NotFound
  - `409` Conflict
  - `422` BusinessRuleViolation
- Business rule failures are `422` with a stable `error.code` of the form `Hr:<Module>:<PascalCaseName>`.
- The backend must map these codes to 422 through ABP exception status-code options, since ABP's default for business exceptions is 403.

## Required extensions on every operation

```yaml
operationId: LeaveRequest_Create      # Unique. <Resource>_<Action> in PascalCase.
tags: [Leave]
summary: Create a leave request       # Short English.
description: >-                       # Arabic: business rules, state changes, side effects, notifications.
x-source: ["5.4"]                     # Document section numbers that justify this operation.
x-release: 1                          # 1, 2, 3, or later, from sections 5.19 and 12 of the document.
x-allowed-roles: [employee]           # Values of AppRole in common.yaml.
x-permission: Hr.Leave.Request.Create # ABP permission name.
x-data-scope: self                    # self | team | company | assigned | public
x-error-codes: [Hr:Leave:InsufficientBalance]   # Every 422 code. Use [] if none.
```

- `x-allowed-roles` must follow the document exactly. It is the default role seed and the UI contract. `x-permission` is the server check.
- `x-data-scope-by-role` overrides `x-data-scope` where scope differs by role, for example `{hrManager: company, departmentManager: team}`.
- `x-complaint-handler-assignee: true` also allows the holder of an approved complaint handler assignment, whatever their role.
- If a requirement is unclear, do not invent behavior silently. Add `x-open-question: "<question in Arabic>"` to that operation, and choose the document's stated assumption where one exists.
- If you add a route that is not in section 10, add `x-added-route: "<requirement it satisfies>"`.

## Examples

- Every create or action request body, and every main read response, has a realistic `example`.
- Use realistic Arabic text where a value is human text. Never include real people's data.

## Privacy rules that shape the contract

- Separate DTOs per audience where visibility differs. Do not send a field and rely on the UI to hide it.
- Fields a role must not see are **absent from that role's DTO**, not null.
- Complaints: return `404`, not `403`, to users with no right to a complaint, so its existence is not revealed.
- Notification payloads never contain complaint names, subjects, or levels.
- `EvaluationInternalNote` never appears in any employee-facing DTO, export, or notification.
- Store officer sees employees only through `EmployeeRefDto`.
- Financial fields never appear in DTOs for department managers or store officers.

## Style limits

- No `oneOf`/`anyOf`/`discriminator` unless truly needed, since these generate poor Dart clients. Prefer explicit fields plus an enum.
- Inline object schemas are not allowed for request and response bodies. Use named schemas.
- File upload uses `multipart/form-data` with `format: binary`.
- Binary downloads and exports declare their real content types.
