# Automation Gates

Use this as the final checklist before saying a Fusion component or member is onboarded.

## Gate 1: Source Facts

| Required | Evidence |
|---|---|
| d.ts class found | `discover-syncfusion-component.mjs` output |
| JS source found/read | exact `node_modules/@syncfusion/.../*.js` path |
| MVC builder coverage known | Syncfusion XML builder member list |
| raw global constructor known | `new ej.{namespace}.{ClassName}` works |
| instance host known | `ej2_instances[0]` location recorded |
| browser data casing known | rendered JSON shape matches builder field names |
| artifact root created | `tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/master-usecases-index.md` |

## Gate 2: Surface Matrix

Run:

```bash
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-surface.mjs \
  --class {ClassName} \
  --dts {path-to-class.d.ts} \
  --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/netstandard2.0/Syncfusion.EJ2.xml
```

Then classify each row and write the result to
`discovery/public-api-surface.json` plus linked rows in
`master-usecases-index.md`:

| Decision | Meaning |
|---|---|
| builder-owned | keep on MVC builder |
| runtime property source | typed `Read` source, prove consumer |
| runtime property write | typed setter, prove DOM/runtime effect |
| runtime method | typed method, prove effect |
| method return source | typed source, prove consumer |
| event payload read | typed event arg property, prove in sandbox |
| event payload mutation/call | typed event arg extension, prove effect |
| skip | hidden/internal/lifecycle-only or no proven use |

## Gate 3: Event Payload Matrix

For every event selected from `EmitType<TArgs>`:

```bash
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-event-payload.mjs \
  --type {TArgs}
```

Then prove payload properties/methods in raw HTML and write the result to
`discovery/event-payload-surface.json`. Event payload contracts are not complete
until method calls and writable properties have visible proof.

For payloads whose shape varies by gesture, capture every gesture the typed event claims to support. Grid `dataStateChange` is the reference case: sorting, paging, filtering, searching, and grouping can produce different nested payload shapes. A single payload sample is not enough.

## Gate 4: Typed Slice

| File | Required When |
|---|---|
| `FusionXxx.cs` | every component |
| `FusionXxxHtmlExtensions.cs` | MVC builder wrapper or typed render helper |
| `FusionXxxBuilder.cs` | non-standard builder return shape |
| `FusionXxxExtensions.cs` | post-render component members |
| `FusionXxxEvents.cs` | event selectors |
| `Events/FusionXxxOn*.cs` | typed payload and event-arg extensions |
| sandbox controller/model/view | every onboarded behavior |
| Playwright tests | every onboarded behavior |

No core runtime/DSL primitive changes are allowed during component onboarding.
If the current plan model appears unable to represent a source-proven JS object
behavior, stop the component slice, re-read the current DSL source, record the
conflict in `proof/audit-report.md`, and open a separate plan/runtime design
pass with its own DSL graph and matrix.

Typed Syncfusion templates are part of the component proof when the component's
normal app usage depends on templated content. Use `FusionTemplate.Create<T>()`
and prove the rendered template in Playwright instead of raw template strings.

## Gate 5: Runtime Consumer Proof

| Onboarded Member | Consumer Proof |
|---|---|
| property source | `SetText`, condition, gather, or HTTP payload |
| method return source | `SetText`, condition, gather, or HTTP payload |
| void method | visible DOM/component state change |
| property write | visible DOM/component state change |
| event payload property | visible text or HTTP payload from `FromEvent` |
| event payload method | visible popup/data/component behavior |
| array/indexed payload | typed indexed path, whole-array gather, or array transform consumed by behavior |
| builder-owned exclusion | builder XML or source row linked from the audit report |

## Gate 6: Stateful App Proof

If the component normally edits or moves domain data, prove it against an
in-memory HTTP-backed sandbox instead of only local button mutations.

| Behavior | Required Proof |
|---|---|
| create | `POST` endpoint mutates server state, component updates, reload preserves it |
| update | `PUT` endpoint mutates server state, component updates, reload preserves it |
| delete | `DELETE` endpoint mutates server state, component updates, reload preserves it |
| move/reorder | drag/drop or reorder event gathers typed payload, HTTP persists, reload preserves it |
| event-driven persistence | event payload goes through `FromEvent` into HTTP, not manual JS |

This gate is mandatory for boards, grids, schedulers, lists, tree views, and
other components where the normal user workflow changes data.

## Gate 7: Build and Test

Run build-enabled Playwright once after C#/sandbox changes through the root
wrapper:

```bash
scripts/playwright.sh --filter "FullyQualifiedName~WhenUsingFusion{ComponentName}"
```

Use `--no-build` only for repeat runs after build output is known current. Then
verify the running sandbox URL manually or with browser automation:

```text
http://localhost:5220/Sandbox/Components/{ComponentName}
```

## Done Means Done

A member is not onboarded when only one of these is true:

| Not Enough | Missing |
|---|---|
| d.ts says it exists | browser proof |
| raw HTML works | typed Fusion API |
| typed API compiles | sandbox behavior |
| sandbox displays raw value | realistic consumer proof |
| one Playwright assertion passes | all onboarded members proved |

## Documentation Gates

Before closing an onboarding or audit:

| Required Artifact | Gate |
|---|---|
| `master-usecases-index.md` | links every discovery, mapping, proof, and audit file |
| raw EJ2 probes | one per accepted API set or coherent use-case group |
| trace JSON | generated from browser execution, not hand-written inference |
| `public-api-surface.json` | every shipped candidate classified |
| `event-payload-surface.json` | every selected event payload member classified |
| `blazor-candidates.md` | names package absence or classifies candidates as naming evidence only |
| `primitive-map.md` | no accepted candidate remains unmapped |
| `vertical-slice-plan.md` | every implementation, sandbox, and Playwright file named |
| `typed-api-coverage-matrix.md` | one row per public Fusion API member |
| `audit-report.md` | exact commands, exclusions, deferred proof, and commit boundary |

## Defect Protocol

When an issue is discovered in a Fusion component, the failed behavior becomes a
new source-grounded audit row. Do not patch only the failing implementation or
test. Rebuild the row from raw EJ2 discovery and keep the artifacts synchronized:

```text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
```

The audit report must say what went wrong: wrong source, wrong payload shape,
wrong argument order, builder-owned member treated as runtime API, primitive
mapping error, C# API mismatch, sandbox proof gap, or Playwright proof gap.
Closure requires every artifact to state the same corrected shape end to end.
