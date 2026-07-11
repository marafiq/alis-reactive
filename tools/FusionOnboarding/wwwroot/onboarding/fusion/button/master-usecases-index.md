# FusionButton Master Use Cases

Status: audited and closed. `FusionButton` is fully onboarded: raw EJ2 trace,
primitive mapping, C# name decisions, vertical slice, implementation, typed proof
matrix, Playwright proof, and audit closeout all agree end to end.

This file is the entry point for the deterministic Fusion onboarding of
`FusionButton`. Existing C#, sandbox, tests, docs, and memory are evidence only
after raw EJ2 discovery and primitive mapping prove them.

Syncfusion target: `ej.buttons.Button`

Every accepted API member is proven end to end:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

## Current Counts

| Item | Count |
|---|---:|
| Static JS members | 13 |
| Static event members | 1 |
| Event payload entries | 1 |
| Typed C# API rows | 14 |
| Supplemental audit rows | 0 |
| Total typed coverage matrix rows | 14 |

## Use Case Rows

| Use Case | API Members | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|
| open the personalised, locked check-in | `FusionButton(...)`, `SetContent`, `Content` | render is builder-config; runtime write/read are slice | property write + read + render | `FusionButtonHtmlExtensions`, `FusionButtonExtensions` | row-proven |
| unlock / lock the action | `SetDisabled`, `Disabled` | post-render write/read | property write + read | `FusionButtonExtensions` | row-proven |
| report readiness from the action's state | `Disabled` | post-render read | `When(...).Eq(...)` condition over `Read` | `FusionButtonExtensions` | row-proven |
| set the visit's priority | `SetIcon`, `SetCssClass`, `SetPrimary`, `SetToggle` | post-render writes | property writes + `dataBind` | `FusionButtonExtensions` | row-proven |
| trigger / focus the action for the resident | `Click`, `FocusIn` | post-render methods | `EmitCall` | `FusionButtonExtensions` | row-proven |
| record the check-in (gather the action state) | `Content`, `CssClass`, `IsPrimary`, `IsToggle` | post-render reads | `Read` consumed by `Gather` | `FusionButtonExtensions` | row-proven |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- [MVC builder coverage](discovery/mvc-builder-coverage.md)
- [Blazor candidates](discovery/blazor-candidates.md)
- [Public API surface](discovery/public-api-surface.json)
- [Event payload surface](discovery/event-payload-surface.json)
- [Parity accounting](discovery/parity-accounting.json)
- [Raw EJ2 core probe](probes/raw-ej2-core.html)
- [Raw EJ2 core trace](traces/raw-ej2-core.trace.json)
- [Primitive map](mapping/primitive-map.md)
- [C# name decisions](mapping/csharp-name-decisions.md)
- [Vertical slice plan](mapping/vertical-slice-plan.md)
- [Typed API coverage matrix](proof/typed-api-coverage-matrix.md)
- [Behavioral coverage](proof/behavioral-coverage.json)
- [Playwright proof](proof/playwright-proof.md)
- [Audit report](proof/audit-report.md)
