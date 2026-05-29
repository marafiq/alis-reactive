# Automation Gates

Use this as the final checklist before saying a Syncfusion component or member is onboarded.

## Gate 1: Source Facts

| Required | Evidence |
|---|---|
| d.ts class found | `discover-syncfusion-component.mjs` output |
| JS source found/read | exact `node_modules/@syncfusion/.../*.js` path |
| MVC builder coverage known | Syncfusion XML builder member list |
| raw global constructor known | `new ej.{namespace}.{ClassName}` works |
| instance host known | `ej2_instances[0]` location recorded |
| browser data casing known | rendered JSON shape matches builder field names |

## Gate 2: Surface Matrix

Run:

```bash
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-surface.mjs \
  --class {ClassName} \
  --dts {path-to-class.d.ts} \
  --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/netstandard2.0/Syncfusion.EJ2.xml
```

Then classify each row:

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

Then prove payload properties/methods in raw HTML. Event payload contracts are not complete until method calls and writable properties have visible proof.

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

No core runtime/DSL changes are allowed during component onboarding unless the current plan model cannot represent a source-proven JS object behavior.

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

Run build-enabled Playwright once after C#/sandbox changes:

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests/Alis.Reactive.PlaywrightTests.csproj \
  --filter "FullyQualifiedName~WhenUsingFusion{ComponentName}" \
  --logger "console;verbosity=normal"
```

Then verify the running sandbox URL manually or with browser automation:

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
