# Fusion Onboarding Artifacts

This is the committed root for deterministic Fusion onboarding and audit
artifacts.

Each component writes durable evidence under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Use `.claude/skills/onboard-fusion-component/SKILL.md` for the required stage
order. Do not place new workflow artifacts under `tools/SyncfusionOnboarding`;
that older tree is not authoritative unless a current proof pass validates a
specific file as vendor evidence.

Create or refresh Stage 1 inventory artifacts with:

```bash
node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write
```

Inventory artifacts are not API proof. They only record current repo surfaces
before raw EJ2 discovery, primitive mapping, implementation, and behavior proof.

Write component-scoped static discovery artifacts from exact current Syncfusion
d.ts/JS/XML evidence with:

```bash
node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs \
  --component grid \
  --fusion-type FusionGrid \
  --class Grid \
  --namespace grids \
  --dts node_modules/@syncfusion/ej2-grids/src/grid/base/grid.d.ts \
  --js node_modules/@syncfusion/ej2-grids/src/grid/base/grid.js \
  --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/33.2.10/lib/net10.0/Syncfusion.EJ2.xml \
  --blazor-package Syncfusion.Blazor.Grids \
  --blazor-version 33.2.10 \
  --write
```

Static discovery artifacts are not runtime proof. Raw EJ2 traces, primitive
mapping, vertical slice design, Playwright proof, and audit closeout remain
separate required stages. Final Playwright proof belongs on the typed Fusion DSL
after the API row has been onboarded.

Before claiming a component is onboarded or audited, run:

```bash
node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs \
  --component grid
```

The verifier is fail-closed. Missing trace, mapping, proof, or audit artifacts
mean the component is incomplete.
