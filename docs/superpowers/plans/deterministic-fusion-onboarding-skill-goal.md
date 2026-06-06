# Deterministic Fusion Onboarding Skill Goal

> Superseded by `docs/superpowers/plans/final-deterministic-fusion-onboarding-goal.md`.
> Use the final goal document as the authority. This earlier draft is historical
> reviewer context only; do not use any stale array wording here to override the
> current proper array primitive rule.

## Objective

Create a deterministic, auditable Fusion onboarding workflow that can onboard a
new component or audit an existing component from zero to 100 percent typed API
coverage without guessing.

Pilot the workflow with `Grid`, then validate the workflow against the complex
reference components `Kanban`, `Schedule`, and `Grid`.

The workflow must produce source-grounded discovery artifacts before any C#
Fusion API is designed. The authoritative primitive matrix is already available
at:

```text
.claude/skills/onboard-fusion-component/references/js-object-dsl-primitive-matrix.md
```

This goal is to automate and harden the process around that matrix.

## Terminology

- `Fusion` means the Alis.Reactive component onboarding surface, artifact tree,
  skill workflow, C# vertical slice, sandbox proof, and Playwright proof.
- `Syncfusion EJ2` means the external vendor JavaScript, d.ts, XML, Blazor
  package, and documentation used as evidence.
- Do not name new workflow artifacts `syncfusion`. Use `fusion` in committed
  paths and process language.
- Do not reuse older onboarding artifact folders merely because they exist.
  Build fresh Fusion artifacts unless a proof pass validates the old files.

## Reviewer Simulation

### Reviewer: Principal Alis.Reactive DSL Reviewer

Task attempted:
Confirm the workflow cannot accidentally invent new primitives or bypass the
current DSL.

Files inspected:

- `.claude/skills/onboard-fusion-component/references/js-object-dsl-primitive-matrix.md`
- `Alis.Reactive/Components/Contracts/ComponentRef.cs`
- `Alis.Reactive/Components/Contracts/ComponentMember.cs`
- `Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs`
- `Alis.Reactive/PlanModel/Values/ValueExpression.cs`

Concrete confusion:
The existing skill can say "No core runtime/DSL changes are allowed" while also
allowing a stop condition for helper-surface gaps. The deterministic workflow
must clarify that component onboarding may not change primitives. If a gap seems
to exist, the agent must re-read the DSL source and prove the missing mapping
before escalating.

Why it matters for developers:
Without this rule, a component-specific failure can turn into an accidental DSL
expansion. That hides a bad discovery trace or wrong primitive mapping behind a
new abstraction.

Recommended action:
Add an explicit DSL freeze gate: component onboarding cannot add, modify, or
rename primitives. Suspected gaps become proof notes and separate architecture
work, not part of the component slice.

Keep / rewrite / delete / defer:
Keep the current primitive matrix. Rewrite the onboarding workflow so it treats
the matrix as the mapping authority after discovery.

### Reviewer: Fusion Discovery Reviewer

Task attempted:
Determine whether a complex Fusion component's public API can be discovered
systematically before any C# names are chosen.

Files inspected:

- `.claude/skills/onboard-fusion-component/SKILL.md`
- `.claude/skills/onboard-fusion-component/references/source-discovery.md`
- `.claude/skills/onboard-fusion-component/references/html-probe-api-trace.md`
- `.claude/skills/onboard-fusion-component/references/blazor-metadata.md`
- proposed new artifact root: `tools/FusionOnboarding/wwwroot`

Concrete confusion:
The skill currently requires raw HTML proof, d.ts inspection, MVC builder
coverage, and Blazor metadata, but it does not define a durable per-component
artifact layout where all discovered public API facts live.

Why it matters for developers:
For Grid, Kanban, and Schedule, the public surface is too large to keep in chat,
scratch files, an existing onboarding folder, or one Playwright test. If
discovery output is not indexed by component and API set, later work will miss
events, payload variants, overloads, or builder-owned members.

Recommended action:
Create a new committed discovery artifact tree under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Each component must have `master-usecases-index.md` plus per-API-set evidence
files for raw EJ2 probes, trace JSON, event payload matrices, builder coverage,
Blazor name candidates, primitive mapping, vertical-slice decisions, and proof
status.

Keep / rewrite / delete / defer:
Keep raw HTML probing. Rewrite the process so raw probes and trace outputs are
first-class committed evidence.

### Reviewer: C# Fusion API Reviewer

Task attempted:
Verify that discovered Fusion APIs map to developer-facing C# names and
vertical slice organization without becoming stringly DSL.

Files inspected:

- `Alis.Reactive.Fusion/Components/FusionGrid/`
- `Alis.Reactive.Fusion/Components/FusionKanban/`
- `Alis.Reactive.Fusion/Components/FusionSchedule/`
- `.claude/skills/onboard-fusion-component/references/js-object-dsl-primitive-matrix.md`
- `.claude/skills/onboard-fusion-component/references/blazor-metadata.md`

Concrete confusion:
Blazor metadata is a useful naming candidate, but the current skill needs a
stronger rule: use Blazor names only when they match direct EJ2 behavior and
improve the Alis developer API. Do not blindly copy bridge-computed Blazor
members or vendor implementation names.

Why it matters for developers:
The Fusion package is public DSL. Names, file placement, access modifiers, XML
docs, and overload shape become developer contracts. Bad names or broad `object`
parameters make the component technically onboarded but unpleasant and unsafe to
use.

Recommended action:
Add a name-decision stage after discovery and before primitive mapping. Require
each public C# member to cite raw JS proof, d.ts evidence, optional Blazor
candidate evidence, and the chosen Alis name with rationale.

Keep / rewrite / delete / defer:
Keep vertical slices. Rewrite the workflow to enforce organization, naming, and
file-size decisions before implementation.

### Reviewer: Playwright Behavior Coverage Reviewer

Task attempted:
Decide whether Playwright proves the whole typed onboarded API instead of one
happy-path behavior.

Files inspected:

- `.claude/skills/onboard-fusion-component/references/automation-gates.md`
- `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/`
- `docs/superpowers/plans/fusion-playwright-slice-inventory.md`
- `docs/developer-cli.md`

Concrete confusion:
The current automation gates say one Playwright assertion is not enough, but the
workflow needs a coverage manifest that ties every typed public API member to a
behavior proof. Otherwise "Grid works" can mean one behavior worked while half
the typed API was never exercised.

Why it matters for developers:
If an onboarded method or event payload extension is not proven, later users
will discover the bug in a real application. The test suite should prove the
typed API contract, not internal JSON shape or a demo-only path.

Recommended action:
Generate a per-component proof matrix from the discovered API set. Playwright
must cover every onboarded public member with behavior-visible assertions. If a
test fails, restart at discovery and mapping; do not patch the test around the
failure.

Keep / rewrite / delete / defer:
Keep behavior-first Playwright. Rewrite the gate so full typed API coverage is
the done condition.

### Reviewer: Tooling and Automation Reviewer

Task attempted:
Turn the skill into a repeatable command workflow for new components and audits.

Files inspected:

- `.claude/skills/onboard-fusion-component/scripts/`
- `.claude/skills/onboard-fusion-component/references/automation-gates.md`
- proposed new artifact root: `tools/FusionOnboarding/wwwroot`
- `docs/developer-cli.md`

Concrete confusion:
The existing scripts help discover pieces of the surface, but the workflow does
not yet define a single deterministic state machine with durable inputs,
outputs, gates, and restart behavior.

Why it matters for developers:
Without a state machine, agents can skip from "surface found" to implementation
without proving API completeness, event payload variants, builder coverage, or
Blazor name candidates.

Recommended action:
Build or document a workflow command that advances through fixed stages:
inventory, raw EJ2 probe, d.ts/XML/JS source discovery, payload discovery,
Blazor candidate naming, primitive mapping, vertical slice plan, implementation,
proof matrix, Playwright proof, audit report.

Keep / rewrite / delete / defer:
Keep existing scripts as helpers. Rewrite the skill around a deterministic
artifact-driven pipeline.

## Non-Negotiables

- Do not change framework behavior while designing the workflow.
- Do not add, remove, or alter DSL primitives during component onboarding.
- Do not expose public stringly component APIs to make discovery easier.
- Do not treat Syncfusion EJ2 docs, Blazor metadata, or d.ts broad shapes as
  final truth without raw browser proof.
- Do not call one behavior test enough when multiple typed APIs were onboarded.
- Do not collapse complex components into one flat file when partial classes or
  event files are needed for maintainability.
- Do not replace the intentional vertical slice organization.
- Do not use redesign docs as authority for current behavior unless current
  source agrees.
- When evidence conflicts, the highest-authority proof wins in this order:
  running raw EJ2 browser trace, shipped JS source, d.ts/XML metadata, Blazor
  metadata, Syncfusion EJ2 docs, previous repo docs.
- If the skill is more accurate than older docs, update the older docs or add a
  conflict note. The target is actual 100 percent correctness, not "more
  correct than before."

## Required Artifact Layout

For each component under discovery, create a committed artifact tree:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
  master-usecases-index.md
  discovery/
    source-inventory.md
    mvc-builder-coverage.md
    blazor-candidates.md
    public-api-surface.json
    event-payload-surface.json
  probes/
    raw-ej2-{api-set}.html
  traces/
    raw-ej2-{api-set}.trace.json
  mapping/
    primitive-map.md
    csharp-name-decisions.md
    vertical-slice-plan.md
  proof/
    typed-api-coverage-matrix.md
    playwright-proof.md
    audit-report.md
```

`master-usecases-index.md` is the entry point. It must list every discovered
public API candidate by use case, status, and proof file:

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| Sorting | `sortColumn`, `clearSorting`, `dataStateChange.action` | `sorted[]`, `action.*` | mixed | component method + payload read | `FusionGridQueryExtensions` + `FusionGridDataStateChangeArgs` | proven/not proven |

## Deterministic Workflow

### Stage 0: Component Selection and Current Inventory

Pick one exact component and exact Syncfusion class. For the pilot, use `Grid`.

Inventory current repo state before discovery:

- existing `Alis.Reactive.Fusion/Components/Fusion{Component}/` files;
- existing sandbox controller/model/view files;
- existing Playwright files under
  `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/{Component}/`;
- existing Fusion artifact tree under
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/`.
  Do not reuse `tools/SyncfusionOnboarding`; treat it as possibly stale or
  corrupted evidence unless a later proof pass explicitly validates it.

Output:

- `discovery/source-inventory.md`;
- current public API and test coverage summary;
- explicit statement whether this is new onboarding or audit of an existing
  component.

### Stage 1: Raw EJ2 Discovery Harness

Write raw HTML under `probes/` using EJ2 JavaScript, not Alis wrappers.

The harness must:

- load the same Syncfusion assets used by the sandbox;
- instantiate `new ej.{namespace}.{ClassName}(options)`;
- expose the exact runtime object and instance host;
- group API by real use case, not alphabetic member dump;
- execute property reads, property writes, void methods, return-value methods,
  events, payload reads, payload writes, and payload calls;
- record argument order, return shape, payload keys, method names, nested paths,
  array element shapes, lifecycle timing, and visible effect.

Output:

- one `raw-ej2-{api-set}.html` per API set or coherent use-case group;
- one matching `raw-ej2-{api-set}.trace.json`;
- trace rows linked from `master-usecases-index.md`.

### Stage 2: Public API Surface Discovery

Use shipped Syncfusion evidence to build the complete candidate surface:

- shipped JS source for lifecycle and hidden requirements;
- d.ts for public members, events, payload types, args, and returns;
- Syncfusion MVC XML/builder coverage for builder-owned static configuration;
- Syncfusion EJ2 docs only as setup accelerators;
- raw HTML trace as the acceptance proof.

Discovery must classify every candidate:

| Status | Meaning |
|---|---|
| builder-owned | static render configuration, do not onboard as reactive API |
| runtime property source | component property read needed by reactive consumers |
| runtime property write | component property write needed after render |
| runtime method | component method command |
| method return source | component method result used as typed value source |
| event | component event to wire through `.Reactive(...)` |
| payload read | event payload property/nested/array read |
| payload mutation/call | event payload write or method call during event lifecycle |
| skip | hidden, internal, duplicate, unproven, or not a runtime gap |
| deferred proof | promising but not accepted yet |

Output:

- `discovery/public-api-surface.json`;
- `discovery/event-payload-surface.json`;
- updated `master-usecases-index.md`.

### Stage 3: Blazor Name Candidate Review

Inspect the matching Syncfusion Blazor NuGet package when available.

Use Blazor metadata as a typed naming candidate, not as proof of EJ2 behavior.
Read XML and decompiled code where needed. For each useful candidate, classify:

| Blazor Candidate Type | Action |
|---|---|
| direct EJ2 overlap | may inform C# naming after raw proof |
| bridge-computed browser behavior | use only if Alis intentionally owns the same bridge and proves it |
| Blazor-owned state behavior | do not copy into Fusion |
| naming only | may inform public API words when it improves clarity |

Output:

- `discovery/blazor-candidates.md`;
- `mapping/csharp-name-decisions.md` with one row per public C# member.

### Stage 4: Authoritative Primitive Mapping

Map every accepted candidate through:

```text
.claude/skills/onboard-fusion-component/references/js-object-dsl-primitive-matrix.md
```

Mapping must cover:

- component scalar/object/array property reads;
- component scalar/object/array property writes;
- nested component paths;
- component methods with no arg, one arg, two args, three args, and four-plus
  args;
- component methods returning void or value;
- component overloads;
- component object and array arguments;
- component events;
- event payload scalar/nested/indexed/whole-array reads;
- event payload writes;
- event payload method calls with no arg, one arg, and multiple args;
- event payload consumer path: condition, gather body/header/route, plugin arg,
  array transform, DOM helper;
- builder-owned exclusions;
- unproven or hidden/internal exclusions.

No primitive changes are allowed in this stage. If a member appears unmappable,
the default conclusion is discovery or mapping error. Re-read current DSL source:

- `ComponentRef<TComponent, TModel>`;
- `ComponentProperty<T>`;
- `ComponentMethod`;
- `TypedEvent<TArgs>`;
- `PayloadSource.Event()`;
- `ReactionGraph.Set` and `ReactionGraph.Call`;
- `ValueExpression`;
- `GatherBuilder`;
- condition builders;
- plugin argument builders;
- `ReactiveArray<T>`.

Output:

- `mapping/primitive-map.md`;
- unresolved conflicts or stop conditions in `proof/audit-report.md`.

### Stage 5: Vertical Slice Design

Before editing implementation, generate `mapping/vertical-slice-plan.md`.

The plan must name exact files and organization:

| Concern | Preferred Location |
|---|---|
| component marker/metadata | `Alis.Reactive.Fusion/Components/Fusion{Component}/Fusion{Component}.cs` |
| MVC builder wrapper | `Fusion{Component}HtmlExtensions.cs` |
| non-standard builder carrier | `Fusion{Component}Builder.cs` |
| component property/method APIs | `Fusion{Component}Extensions.cs` or coherent partial files |
| large API families | partial files by use case, for example query/editing/tooling |
| event selectors | `Fusion{Component}Events.cs` |
| event payload types/extensions | `Events/Fusion{Component}{Event}Args.cs` |
| sandbox proof | sandbox controller/model/view under the component slice |
| Playwright proof | `tests/.../Components/Fusion/{Component}/` |

Use partial classes when the component is complex enough that a single extension
file hides use-case boundaries. Do not add indirection for small components.
Keep the intentional vertical slice per component.

Output:

- file-by-file implementation plan;
- public API name table;
- XML doc rule: IntelliSense must explain API contract and useful examples, not
  internal Syncfusion quirks already encapsulated by the Fusion API.

### Stage 6: Implementation

Implement only the accepted mapped surface. Each public C# member must trace
back to:

```text
raw trace row -> discovery classification -> Blazor/name decision if used ->
primitive-map row -> vertical slice file
```

Do not implement unproven candidates. Do not preserve dead helpers because old
tests reference them. Do not add public `string memberName`, broad `object`, or
escape hatches unless the API is intentionally a plugin boundary.

Output:

- typed Fusion implementation;
- sandbox workflows with realistic behavior;
- updated proof matrix.

### Stage 7: Behavior Proof and 100 Percent Typed API Coverage

Generate `proof/typed-api-coverage-matrix.md` from the implemented public API.
Every onboarded public member must have a behavior proof.

Minimum proof rules:

| Onboarded API | Required Behavior Proof |
|---|---|
| component property source | condition, gather, HTTP payload, DOM text, or component binding consumes it |
| component property write | visible component/runtime state changes |
| component void method | visible component/runtime state changes |
| component method return source | condition, gather, HTTP payload, DOM text, or component binding consumes it |
| event payload property | visible text or HTTP payload from typed event read |
| event payload mutation | visible Syncfusion lifecycle behavior changes |
| event payload method | visible popup/data/component behavior changes |
| array member/source | proper array primitive, whole-array gather, or typed array source consumed by behavior |
| stateful workflow | HTTP-backed create/update/delete/move/reload proof when applicable |

Playwright tests must be behavior tests. They must run through
`scripts/playwright.sh`, not raw `dotnet test`.

If any proof fails, restart from discovery and mapping. A failure means one of
these is wrong:

- raw HTML discovery missed or misread the API;
- trace did not capture the correct runtime behavior;
- d.ts/XML/Blazor evidence was over-trusted;
- primitive mapping was incomplete or wrong;
- C# vertical slice mapped the right primitive incorrectly;
- Playwright is testing the wrong user behavior.

Do not patch around the failure by weakening the test.

### Stage 8: Audit Existing Components

The same workflow must audit existing onboarded components.

For an audit, treat existing C# and tests as evidence only. Rebuild the discovery
artifact tree, map every existing public API member, and decide:

| Audit Result | Action |
|---|---|
| proven and correctly mapped | keep |
| public API exists but is unproven | add discovery/proof or remove/defer |
| public API uses wrong name | fix with compatibility judgment |
| public API is stringly or too broad | replace with typed facade |
| public API duplicates builder static config | remove or deprecate intentionally |
| test covers only one behavior | add missing behavior proof |

## Verification Gates

### Documentation and Artifact Gates

- `master-usecases-index.md` exists and links every artifact.
- Raw HTML probes exist for every accepted API set.
- Trace JSON exists and is generated from browser execution.
- Public API surface and event payload JSON exist.
- Blazor candidate review exists or records package absence.
- Primitive map has no unmapped accepted candidates.
- Vertical slice plan names every implementation file.
- Typed API proof matrix has one row per public member.

### Build and Test Gates

Use `docs/developer-cli.md` wrappers.

For docs/artifact-only workflow edits:

```text
git diff --check
```

For C#/sandbox/component implementation:

```text
scripts/test.sh --no-e2e
scripts/playwright.sh --filter "<component focused filter>"
```

For broad component or runtime-impacting changes:

```text
npm run typecheck
npm run build:all
npm test
dotnet build
scripts/test.sh --no-e2e
scripts/playwright.sh --no-build
```

Never use raw Playwright `dotnet test` as the primary proof.

## Commit Boundaries

Use one commit per closed stage when the patch is large:

1. artifact workflow and skill documentation;
2. Grid discovery artifact skeleton and source inventory;
3. Grid raw EJ2 probe and trace generator;
4. Grid public API and event payload discovery;
5. Grid Blazor candidate review and name decisions;
6. Grid primitive map and vertical slice plan;
7. Grid implementation slice;
8. Grid behavior proof and audit report;
9. workflow validation against Kanban and Schedule.

Each commit must be auditable from history and must leave no half-renamed
vocabulary or unlinked artifact.

## Done Definition

This goal is complete only when:

- the onboarding skill defines the deterministic state machine;
- the artifact tree convention is documented and enforced;
- `Grid` has a committed `master-usecases-index.md` and associated discovery,
  mapping, and proof artifacts;
- raw EJ2 discovery can reproduce exact component API and payload shapes;
- Blazor metadata review is integrated as a naming-candidate stage;
- the authoritative primitive matrix is the only mapping authority after
  discovery;
- vertical slice organization and naming conventions are enforced;
- behavior proof requires 100 percent coverage of typed onboarded API;
- failures restart at discovery/mapping instead of weakening Playwright tests;
- the same workflow can audit existing onboarded components;
- `Kanban`, `Schedule`, and `Grid` are used to validate that the workflow handles
  complex events, stateful workflows, nested payloads, arrays, method returns,
  overloads, and large vertical slices.
