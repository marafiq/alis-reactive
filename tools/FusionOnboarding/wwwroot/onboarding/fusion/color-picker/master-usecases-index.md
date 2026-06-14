# FusionColorPicker Master Use Cases

Status: static-discovery. Runtime trace, primitive mapping, vertical slice
decision, implementation proof, and audit closeout are still pending.

This file is the entry point for deterministic Fusion onboarding or audit of
`FusionColorPicker`. Existing C#, sandbox, tests, docs, and memory are evidence
only after raw EJ2 discovery and primitive mapping prove them.

Syncfusion target: `ej.inputs.ColorPicker`

No API member is accepted until the row is proven end to end:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

## Current Counts

| Item | Count |
|---|---:|
| Static JS members | 26 |
| Static event members | 9 |
| Event payload entries | 9 |

## Use Case Rows

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | current Fusion source, sandbox, and tests inventoried | n/a | n/a | n/a | n/a | inventory artifact linked |
| shipped EJ2 static discovery | [public-api-surface.json](discovery/public-api-surface.json) | [event-payload-surface.json](discovery/event-payload-surface.json) | [mvc-builder-coverage.md](discovery/mvc-builder-coverage.md) | pending mapping | pending naming | static-discovery only |
| raw EJ2 core probe | [raw-ej2-core.html](probes/raw-ej2-core.html) | pending runtime gesture traces | pending runtime confirmation | pending mapping | pending naming | trace not yet executed |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- [MVC builder coverage](discovery/mvc-builder-coverage.md)
- [Blazor candidates](discovery/blazor-candidates.md)
- [Public API surface](discovery/public-api-surface.json)
- [Event payload surface](discovery/event-payload-surface.json)
- [Raw EJ2 core probe](probes/raw-ej2-core.html)
- `traces/raw-ej2-core.trace.json` pending real browser execution
- `mapping/primitive-map.md` pending authoritative primitive mapping
- `mapping/csharp-name-decisions.md` pending Blazor candidate review and raw trace proof
- `mapping/vertical-slice-plan.md` pending vertical slice design
- `proof/typed-api-coverage-matrix.md` pending implemented public API inventory
- `proof/playwright-proof.md` pending behavior proof
- `proof/audit-report.md` pending audit closeout
