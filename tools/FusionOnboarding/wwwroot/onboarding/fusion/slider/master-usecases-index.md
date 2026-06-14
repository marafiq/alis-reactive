# FusionSlider Master Use Cases

Status: audited. `FusionSlider` is fully onboarded: raw EJ2 trace, primitive
mapping, C# name decisions, vertical slice, typed proof matrix, and Playwright
behavior proof are all closed. The component row is complete.

This file is the entry point for the deterministic Fusion onboarding of
`FusionSlider`. Existing C#, sandbox, tests, docs, and memory became evidence
only after raw EJ2 discovery and primitive mapping proved them.

Syncfusion target: `ej.inputs.Slider`

Every accepted API member is proven end to end:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

## Current Counts

| Item | Count |
|---|---:|
| Static JS members | 28 |
| Static event members | 6 |
| Event payload entries | 6 |
| Typed C# API rows | 14 |
| Supplemental audit rows | 0 |
| Total typed coverage matrix rows | 14 |

## Journey

The "Comfort & Care Preferences" journey: a resident adjusts their room
temperature (scalar slider) and their afternoon rest window (range slider), can
apply the care team's recommendations, then saves. The journey exercises the full
typed `FusionSlider` runtime surface through real handle gestures and button
clicks.

## Use Case Rows

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | current Fusion source, sandbox, and tests inventoried | n/a | n/a | n/a | n/a | inventory artifact linked |
| shipped EJ2 static discovery | [public-api-surface.json](discovery/public-api-surface.json) | [event-payload-surface.json](discovery/event-payload-surface.json) | [mvc-builder-coverage.md](discovery/mvc-builder-coverage.md) | [primitive-map.md](mapping/primitive-map.md) | [csharp-name-decisions.md](mapping/csharp-name-decisions.md) | audited |
| raw EJ2 core probe + trace | [raw-ej2-core.html](probes/raw-ej2-core.html) | [raw-ej2-core.trace.json](traces/raw-ej2-core.trace.json) | n/a | [primitive-map.md](mapping/primitive-map.md) | [csharp-name-decisions.md](mapping/csharp-name-decisions.md) | audited |
| scalar value read/write | `Value()`, `SetValue(double)` | n/a (component property) | no (post-render runtime) | component property read + write with `dataBind` repaint | `FusionSliderExtensions` | row-proven |
| range value read/write | `RangeValue()`, `SetRangeValue(double, double)` | n/a (component property) | no (post-render runtime) | component property read + write (number array) with `dataBind` repaint | `FusionSliderExtensions` | row-proven |
| change events | `Change`, `Changed` | `Value`, `PreviousValue`, `Text`, `Action`, `IsInteracted` | no (typed runtime events) | component event trigger + event payload reads | `FusionSliderEvents`, `FusionSliderChangeArgs` | row-proven |
| field render | `FusionSlider(...)` | n/a | initial options on `SliderBuilder` | input component registration + builder render | `FusionSliderHtmlExtensions` | row-proven |

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
