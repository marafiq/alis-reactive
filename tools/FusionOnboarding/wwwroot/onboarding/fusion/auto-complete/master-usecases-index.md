# FusionAutoComplete Master Use Cases

Status: inventory-only.

This file is the entry point for deterministic Fusion onboarding or audit of
`FusionAutoComplete`. Existing C#, sandbox, tests, docs, and memory are
evidence only after raw EJ2 discovery and primitive mapping prove them.

No API member is accepted until the row is proven end to end:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | pending discovery | pending discovery | pending discovery | pending mapping | pending design | inventory-only |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- `discovery/public-api-surface.json` pending raw EJ2 and shipped source discovery
- `discovery/event-payload-surface.json` pending event payload discovery
- `mapping/primitive-map.md` pending authoritative primitive mapping
- `mapping/csharp-name-decisions.md` pending Blazor candidate review
- `mapping/vertical-slice-plan.md` pending vertical slice design
- `proof/typed-api-coverage-matrix.md` pending implementation inventory
- `proof/playwright-proof.md` pending behavior proof
- `proof/audit-report.md` pending audit closeout
