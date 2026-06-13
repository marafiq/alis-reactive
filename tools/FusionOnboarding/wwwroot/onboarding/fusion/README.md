# Fusion Onboarding Artifacts

This is the committed root for deterministic Fusion onboarding and audit
artifacts.

Each component writes durable evidence under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Use `.claude/skills/onboard-fusion-component/SKILL.md` for the required stage
order.

The master index is the source index for the onboarding slice. It must contain
one API row per event variant, property, or method being onboarded/audited.
Finish one row end to end before starting the next. For complex components,
events come first, then properties, then methods.

Audit is a skill-accuracy loop. When a component audit discovers a mismatch,
missing field, invalid probe assumption, or unsupported proof claim, update or
confirm the reusable pattern in
`tools/FusionOnboarding/wwwroot/onboarding/fusion/_skill/pattern-map.md` before
editing the component implementation. The point is to remove the chance of the
same mistake in the next component, not to patch one page.

For event rows, payload shape is required evidence. Do not map an event from the
event name alone. Record exact keys, missing/optional keys, nested objects,
array element shapes through the proper array primitive, writable fields,
payload methods, and the typed C# event arg names.

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

Generate committed raw EJ2 traces through onboarding-only tooling:

```bash
node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs \
  --component grid \
  --api-set core
```

The trace runner drives a browser and writes deterministic
`traces/raw-ej2-{api-set}.trace.json` artifacts. It does not add product sandbox
routes and does not count as typed Fusion DSL behavior proof.

Generate a fail-closed typed API coverage matrix from current C# source with:

```bash
node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs \
  --component grid \
  --fusion-type FusionGrid \
  --write
```

Check that the committed matrix is still generated from current C# source and
artifact judgment decisions with:

```bash
node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs \
  --component grid \
  --fusion-type FusionGrid \
  --check
```

Refresh the cross-component onboarding dashboard after matrix or artifact count
changes:

```bash
node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write
```

Check that the committed dashboard is current with:

```bash
node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --check
```

Before claiming a component is onboarded or audited, run:

```bash
node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs \
  --component grid
```

The verifier is fail-closed. Missing trace, mapping, proof, audit artifacts,
summary count rows, cited Playwright TRX files, a stale generated matrix, or a
stale cross-component onboarding dashboard mean the component is incomplete.
For row-proven edit-action exclusion rows, the verifier also compares the
generated matrix member list against the vertical-slice proof narrative so
artifact prose cannot claim only a subset of the exclusions.
