---
name: validation-roadmap
description: Validation module gaps and input component onboarding roadmap identified during HTTP pipeline + validation work session
type: project
---

Validation module has nuanced gaps identified during 2026-03-19 session.

## Priority 1: Conditional Validation Rule Parity
All rules supported in unconditional blocks must also work under `WhenField()` conditional blocks. The extractor kills reflection intentionally — verify that conditional extraction doesn't silently drop rule types.

**Why:** If `NotEmpty`, `MinLength`, `GreaterThan` work unconditionally but some fail under `WhenField()`, developers get silent validation gaps.

**How to apply:** Check `FluentValidationAdapter.cs` conditional extraction path vs unconditional. Write test for each rule type under `WhenField()`.

## Priority 2: Date Range Validation
Currently `required` is supported for dates. Need proper date range semantics — min date, max date, date comparison.

**Why:** Senior living domain uses admission dates, medication schedules — date validation is critical.

## Priority 3: SF Grid Inline Edit Research
Research if SF Grid inline editing can be onboarded like input components via vertical slices. Bulk edit support will come with this.

**Why:** Grid is a major component for data management in senior living apps.

## Priority 4: SF Server-Side Filtering
Dropdown/AutoComplete components need server-side filtering support via proper vertical slice and research. Close the input components story.

**Why:** Large datasets (residents, facilities, physicians) need server-side search, not client-side filtering.

## Completed This Session
- HTTP pipeline redesign (fully async, error boundaries, ResolvedFetch)
- `gt`/`lt` rule types for GreaterThan/LessThan extraction
- Native compound component inline init (radio group + checklist)
- Live re-validation on blur/change (industry standard pattern)
- Radio group live-clear fix
