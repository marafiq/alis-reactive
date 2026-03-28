# Validation Architecture — Original Prompt (NEVER LOSE)

## Flow
1. Developer defines `TModelValidator` using FluentValidation
2. FluentValidationAdapter extracts **simple rules** (no HTTP/DB) including conditional rules → rules schema in plan
3. Rules become part of Reactive Plan via `.Validate<TFluentModelValidator>()` stage after HTTP verb
4. Pipeline: `.Post(url).Validate<TValidator>().Response(...)` — validation runs before request fires

## Key Principles
- **Explicit IDs, no fallbacks** — TS only works with explicit id, never guesses. Anytime you add fallbacks or wide patterns = wrong thinking, trash code
- **Validation rules ≠ Conditions module** — completely separate abstractions
- **SOLID design** — validation.ts is a standalone module, not mixed with forms/container concepts
- **Vendor-aware value reading** — TS knows component vendor (native/fusion) and how to read values. Native: `.value`, checkbox: `.checked`. Fusion: `readExpr` (e.g., `el.ej2_instances[0].value`)
- **`Html.Field()` renders validation span** — use the existing abstraction, not raw HTML
- **Build ID from name** — TS knows native uses `_` for dots, syncfusion removes dots. Can derive element ID from field name in Problem Details JSON
- **Container, not form** — in future, validate inside any container id, not just `<form>`
- **Interactive clearing** — as soon as user interacts or blurs, server validation messages disappear
- **No register() step** — stateless `validate(desc)` takes descriptor directly
- **First-failing-rule-wins** — stop at first failure per field

## Server Errors (400 ProblemDetails)
- Server returns 400 with `{ errors: { "FieldName": ["message"] } }`
- TS builds element ID from field name using vendor convention
- Display messages inline at the correct fields
- Clear on user interaction/blur

## What to Delete
- `forms.ts` — badly designed, mixed concerns with form container
- `errorId` field — replaced by deriving ID from field name + vendor convention
- All fallback patterns in evalCondition(), showServerFieldError()
- `register()` + global `forms` Map

## What to Keep
- 11 rule types: required, minLength, maxLength, email, regex, url, range, min, max, equalTo, atLeastOne
- ValidationCondition: 4 ops (truthy, falsy, eq, neq) — field-level, NOT pipeline conditions
- readValue() vendor pattern (native vs fusion + readExpr)
- isHidden() check (skip hidden fields)

## Design for Extension
- Multiple vendors (native, fusion, future vendors)
- Use resolver module for value reading
- Gather module should also be SOLID, extensible without breaking
- Response handling uses existing modules (JSON path walking in future)
