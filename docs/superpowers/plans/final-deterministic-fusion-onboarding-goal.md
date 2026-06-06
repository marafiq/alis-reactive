# Final Deterministic Fusion Onboarding Goal

## Purpose

Make the Fusion onboarding skill deterministic for every Fusion component,
existing or new, from zero vendor discovery to 100% typed API behavior proof.

This is not a Grid-only goal. Grid is a complex proof/stress component. The
process must also audit existing onboarded components and onboard future
components without hand-waving, stale artifacts, missing member coverage, or
one-happy-path Playwright proof.

## Correction From Prior Attempt

A prior commit updated skill documentation and helper output, then incorrectly
marked the broader goal complete. Treat that commit as draft evidence only.
Completion requires real end-to-end proof artifacts and synchronized validation,
not just process prose.

## Terminology

- Fusion means the Alis workflow, artifacts, skill, C# vertical slice, sandbox,
  and Playwright behavior proof.
- Syncfusion EJ2 means vendor evidence only: JavaScript, d.ts, XML, Blazor
  packages, docs, shipped assets, and raw browser runtime behavior.
- Existing `tools/SyncfusionOnboarding` is not trusted workflow authority unless
  independently validated from current evidence.

## Hard Rules

- The workflow applies to every Fusion component, existing or new.
- Audits are first-class. Existing C#, sandbox pages, tests, docs, and memories
  are evidence only after raw discovery and primitive mapping prove them.
- No public stringly Fusion DSL.
- No DSL primitive changes during component onboarding or audit.
- If a member appears unmappable, assume discovery, mapping, or DSL reading is
  wrong first. Re-read current DSL source before escalating.
- Blazor metadata is naming evidence only, never proof.
- Playwright must prove user-visible behavior for every typed onboarded API
  member, not one representative behavior.
- Raw HTML probe execution is vendor discovery evidence only. It may prove that
  a trace row is true, but it is not final Playwright behavior proof.
- Do not add product sandbox routes, public component APIs, or normal
  Playwright tests only to make raw probe execution convenient.
- All artifacts must hold shape end to end. A defect means at least one artifact
  row is missing, stale, or wrong.

## Required Artifact Root

Every component onboarding or audit writes durable evidence under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Required files:

```text
master-usecases-index.md
discovery/source-inventory.md
discovery/mvc-builder-coverage.md
discovery/blazor-candidates.md
discovery/public-api-surface.json
discovery/event-payload-surface.json
probes/raw-ej2-{api-set}.html
traces/raw-ej2-{api-set}.trace.json
mapping/primitive-map.md
mapping/csharp-name-decisions.md
mapping/vertical-slice-plan.md
proof/typed-api-coverage-matrix.md
proof/playwright-proof.md
proof/audit-report.md
```

`master-usecases-index.md` is the component entry point. It must link every
discovery, mapping, implementation, proof, and audit row.

## Deterministic Workflow

The skill must enforce this order:

1. Component inventory.
2. Raw EJ2 probe generation.
3. Shipped JS/d.ts/XML discovery.
4. Event payload discovery.
5. Blazor NuGet naming candidate review.
6. Authoritative primitive mapping.
7. Vertical slice design.
8. Implementation rules.
9. 100% typed API proof matrix.
10. Playwright behavior proof.
11. Audit report.

Do not design or edit C# before the discovery, trace, mapping, and name-decision
artifacts exist for the affected rows.

For each row, the non-skippable proof chain is:

```text
raw EJ2 HTML -> raw EJ2 trace JSON -> committed artifact row ->
typed Fusion C# DSL vertical slice -> authoritative primitive mapping ->
Playwright behavior over typed Fusion DSL -> audit report
```

No later step can claim progress unless the previous artifact exists, is
committed, and is linked from `master-usecases-index.md`.

## Reviewer Simulation

Before implementation, run multiple skeptical reviewer passes and capture their
findings:

- Principal DSL reviewer: confirms no primitive changes and no missed existing
  DSL capability.
- Fusion discovery reviewer: confirms raw EJ2 probes and traces are complete.
- C# API reviewer: confirms typed public names and rejects stringly or broad
  shortcuts.
- Vertical slice reviewer: confirms component organization, event placement,
  naming conventions, and partial-class judgment for large components.
- Playwright behavior reviewer: confirms every typed onboarded API member has a
  user-visible behavior proof.
- Artifact consistency reviewer: confirms all artifacts agree on member shape,
  payload shape, args, names, primitive, implementation, and tests.

## Discovery Requirements

Raw EJ2 probes must instantiate Syncfusion EJ2 directly, not Alis wrappers.
They must trace:

- exact JS API names;
- property read/write behavior;
- method names, argument order, and return shapes;
- overload shapes;
- object and array args;
- event names and payload keys;
- writable payload fields;
- payload methods;
- nested payloads;
- arrays through the proper array primitive and typed array sources;
- lifecycle timing;
- visible/runtime effects;
- builder-owned exclusions.

## Blazor Metadata Rule

Read Syncfusion Blazor NuGet metadata and decompiled code when available. Use
Blazor names only when they match proven EJ2 behavior and improve the Alis API.
Never copy Blazor bridge-computed or Blazor-owned state behavior blindly.

## Primitive Mapping Rule

Every accepted API member must map through the authoritative primitive matrix.
The mapping must name the primitive used for:

- component property read/write;
- component method call;
- component method return source;
- event payload read;
- event payload mutation;
- event payload method call;
- nested component or payload paths;
- arrays through the proper array primitive and typed array sources;
- builder-owned exclusions.

If a mapping seems impossible, stop the component pass and re-read the current
DSL source before escalating. Do not add primitives to make onboarding easier.

## Vertical Slice Rule

Preserve current component vertical slice organization:

- `Fusion{Component}.cs`
- `Fusion{Component}Builder.cs`
- `Fusion{Component}HtmlExtensions.cs`
- `Fusion{Component}Extensions.cs`
- `Fusion{Component}Events.cs`
- `Fusion{Component}ReactiveExtensions.cs`
- `Events/*`

For complex components, split large extension files into coherent partials by
use case. Use judgment, but do not hide large APIs in one unreadable file and do
not add indirection for small components.

## Proof Rule

Generate `proof/typed-api-coverage-matrix.md` from the implemented public API.
Every typed public Fusion member must have a behavior proof row.

The coverage matrix must fail closed. Any typed public member without a linked
raw trace row, primitive-map row, vertical-slice row, and Playwright behavior
row keeps the component unaudited.

Playwright tests must be behavior tests through `scripts/playwright.sh`. They
must prove user-visible behavior, realistic request/response behavior, or
visible runtime state. Internal plan JSON can support diagnosis, but cannot be
the only proof.

Tests over raw EJ2 HTML probes do not count toward typed API coverage. They are
allowed only as disposable tooling checks or committed trace-generation tooling.
Completion proof must run through the typed Fusion DSL after the row has
discovery, mapping, C# naming, and vertical slice artifacts.

If Playwright fails, restart at the earliest contradicted artifact in this
order: raw EJ2 trace, shipped source discovery, event payload discovery,
Blazor/name decision, primitive map, vertical-slice plan, implementation,
sandbox proof. Do not patch only the test or implementation.

If a test fails or a component issue is discovered, restart the affected row at
zero discovery. Do not patch only the implementation or test. The failure means
one of these may be wrong:

- raw EJ2 discovery;
- trace capture;
- d.ts/XML/Blazor interpretation;
- primitive mapping;
- C# API shape;
- vertical slice implementation;
- sandbox proof;
- Playwright proof.

## Required Validation Before Completion

Completion requires all of this:

- The skill workflow is updated inline.
- The artifact workflow is defined and tracked.
- Existing and future components are both covered by the process.
- Audit mode is first-class.
- At least one complex component, with Grid as the preferred stress component,
  has real committed discovery, mapping, proof, and audit artifacts.
- Kanban and Schedule are stress-reviewed to prove the workflow handles complex
  events, stateful workflows, nested payloads, proper array primitives, method
  returns, overloads, and large vertical slices.
- All artifacts for validated components agree end to end.
- Playwright behavior proof covers 100% of typed onboarded API for accepted
  rows.
- Stale `syncfusion` workflow naming is removed except where describing vendor
  evidence.
- Appropriate docs/script/build/test checks pass.
- Deferred concerns are rare, evidence-backed, and do not affect deterministic
  onboarding or audit correctness.

## Final Deliverable

One final committed slice only after the criteria above are true.

Final response must include:

- commit hash;
- changed files;
- artifact trees created or audited;
- reviewer findings summary;
- validation commands run;
- explicit deferred concerns, or `none`.
