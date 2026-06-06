---
name: onboard-fusion-component
description: Onboards Fusion components into Alis Reactive by proving the Syncfusion EJ2 JS object API in raw HTML, tracing properties/methods/events, then implementing a typed Fusion vertical slice. Use when adding a new Fusion component, adding props/methods/events to an existing Fusion component, or validating whether vendor API evidence should become typed Fusion DSL.
disable-model-invocation: true
---

# Onboard Fusion Component

## Operating Rule

Do not onboard from memory, docs alone, old generated artifacts, or inferred API names. The source of truth is Syncfusion EJ2 shipped JS source plus a running raw HTML probe that instantiates the object, calls the exact API, captures event payloads, and records method return behavior.

The Syncfusion MVC builder remains the configuration surface for initial render. Do not duplicate builder-covered static properties in the reactive DSL unless they must be read or mutated after render. The Fusion slice adds typed runtime behavior: post-render property reads/writes, method calls, method return sources, and event-to-plan wiring.

The public DSL remains typed. Internal plan member names may be strings; developer-facing APIs must not expose arbitrary member strings except plugin escape hatches.

Loose typing in Syncfusion contracts is not permission to add loose typing in Alis. Public shapes such as `object`, `Record<string, any>`, or broad event args are candidate evidence only. The onboarding decision must narrow them through JS source and an HTML execution trace before exposing a typed C# API. If a member cannot be narrowed and proven, keep it out of the public Fusion slice.

## Terminology And Artifact Root

- `Fusion` means the Alis workflow, durable artifacts, skill process, C# vertical slice, sandbox behavior, and Playwright proof.
- `Syncfusion EJ2` means vendor evidence only: JavaScript, d.ts, XML, Blazor packages, docs, shipped assets, and raw browser behavior.
- New durable workflow artifacts must use `fusion` in paths and process language.
- Existing `tools/SyncfusionOnboarding` files are not workflow authority. Treat them as possibly corrupted unless a current proof pass validates a specific file as vendor evidence.
- Every new onboarding or audit writes its durable evidence under:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/
```

Each component artifact tree must contain:

```text
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

`master-usecases-index.md` is the entry point. It lists every discovered public API candidate by use case, API member, event payload, builder ownership, primitive, C# target, artifact links, and proof status.

## Workflow

The workflow is a deterministic state machine. Do not skip stages, and do not design C# before the discovery, trace, mapping, and name-decision artifacts exist.

All artifacts must hold shape end to end. If a defect is found in a Fusion
component, do not patch the C# slice, sandbox, or Playwright test in isolation.
Restart the affected row from zero discovery, prove what went wrong in raw EJ2
and shipped evidence, then update every linked artifact:

```text
master-usecases-index.md
-> discovery/source-inventory.md
-> discovery/public-api-surface.json
-> discovery/event-payload-surface.json
-> probes/raw-ej2-{api-set}.html
-> traces/raw-ej2-{api-set}.trace.json
-> mapping/primitive-map.md
-> mapping/csharp-name-decisions.md
-> mapping/vertical-slice-plan.md
-> implementation
-> proof/typed-api-coverage-matrix.md
-> proof/playwright-proof.md
-> proof/audit-report.md
```

The component row is not fixed until those artifacts agree on the same member
shape, payload shape, argument order, sync/async lane, C# API, runtime behavior,
and Playwright proof.

1. **Component inventory**
   - Pick the exact Fusion component and exact Syncfusion EJ2 class.
   - Inventory current repo state before discovery:
     - `Alis.Reactive.Fusion/Components/Fusion{Component}/`
     - sandbox controller/model/view files
     - Playwright files under `tests/Alis.Reactive.PlaywrightTests/Components/Fusion/{Component}/`
     - existing `tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/`
   - State whether this is new onboarding or an audit.
   - Output `discovery/source-inventory.md` and update `master-usecases-index.md`.

2. **Raw EJ2 probe generation**
   - Identify package, global namespace, class name, styles, and required scripts.
   - Example: `@syncfusion/ej2-interactive-chat` -> `ej.interactivechat.AIAssistView`.
   - If Syncfusion ships an official ASP.NET Core agent skill for the component, use it as a documentation accelerator only. It can suggest setup and builder usage, but it does not replace JS source, builder coverage, raw probe, or Playwright proof.
   - If any source path is unknown, read [Source discovery](references/source-discovery.md) and run the discovery helper before continuing:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/discover-syncfusion-component.mjs --class ChipList
     ```
   - Generate raw HTML under `probes/`, using EJ2 JavaScript directly, not Alis wrappers:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/create-fusion-probe.mjs \
       --component ai-assistview \
       --namespace interactivechat \
       --class AIAssistView \
       --id ai-assist \
       --api-set prompts
     ```
   - The probe must load the same EJ2 assets used by the sandbox, instantiate `new ej.{namespace}.{ClassName}(options)`, expose `window.__fusionProbe.ej2`, group APIs by use case, and record argument order, return shape, payload keys, nested paths, array element shapes, lifecycle timing, and visible effect.
   - Output one `probes/raw-ej2-{api-set}.html` and one `traces/raw-ej2-{api-set}.trace.json` per API set.

3. **Shipped JS/d.ts/XML discovery**
   - Generate the source-grounded candidate surface before designing the slice:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-surface.mjs \
       --class AIAssistView \
       --dts node_modules/@syncfusion/ej2-interactive-chat/src/ai-assistview/ai-assistview.d.ts \
       --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/netstandard2.0/Syncfusion.EJ2.xml
     ```
   - Treat the output as a candidate matrix. The final decision still depends on runtime need and raw browser proof.
   - Classify every candidate as `builder-owned`, `runtime property source`, `runtime property write`, `runtime method`, `method return source`, `event`, `payload read`, `payload mutation/call`, `skip`, or `deferred proof`.
   - Output `discovery/public-api-surface.json`, `discovery/mvc-builder-coverage.md`, and updated `master-usecases-index.md`.

4. **Event payload discovery**
   - For each `EmitType<TArgs>` event selected, read [Event payload contracts](references/event-payload-contracts.md), inspect `TArgs`, and prove payload properties, writable properties, methods, nested payloads, and arrays in raw HTML:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-event-payload.mjs --type FilteringEventArgs
     ```
   - Capture every gesture the typed event claims to support. Grid `dataStateChange` is the reference case: sorting, paging, filtering, searching, and grouping can produce different nested payload shapes.
   - If a payload property is an array, keep it typed as `List<T>` or a typed array in the C# event contract. Prove it through a typed indexed read, a whole-array gather, or a typed array transform consumed by behavior. Do not add untyped element accessors to component slices.
   - Output `discovery/event-payload-surface.json` and raw trace links.

5. **Blazor NuGet naming candidate review**
   - When Syncfusion ships a matching Blazor package, inspect its XML and IL with ILSpy before finalizing C# names:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-blazor-metadata.mjs \
       --package Syncfusion.Blazor.Kanban \
       --version 32.2.8 \
       --component Kanban \
       --decompiled /tmp/alis-syncfusion-blazor-kanban/decompiled/Syncfusion.Blazor.Kanban.SfKanban`1.decompiled.cs
     ```
   - Blazor often reveals the clean C# vocabulary for methods and event payloads. It is not the final runtime contract for Alis.
   - Classify every Blazor candidate as direct EJ2 overlap, bridge-computed browser behavior, or Blazor-owned state behavior. Direct overlap can become a normal Fusion API after raw HTML proof. Bridge-computed behavior can become a Fusion API only through an explicit Alis bridge that reproduces the same browser facts and is proven in raw HTML/Playwright. Blazor-owned state behavior stays out of Fusion unless Alis intentionally owns the same state concept.
   - Read [Blazor metadata](references/blazor-metadata.md) before using ILSpy output to shape a component slice.
   - Output `discovery/blazor-candidates.md` and one row per public C# member in `mapping/csharp-name-decisions.md`.

6. **Authoritative primitive mapping**
   - Read [JS object DSL primitive matrix](references/js-object-dsl-primitive-matrix.md) before designing the public C# API.
   - Component onboarding cannot add, remove, rename, or broaden DSL primitives. If a member appears unmappable, assume discovery or mapping is wrong first; re-read the current DSL source before escalating:
     - `Alis.Reactive/Components/Contracts/ComponentRef.cs`
     - `Alis.Reactive/Components/Contracts/ComponentMember.cs`
     - `Alis.Reactive/PlanAuthoring/Events/TypedEvent.cs`
     - `Alis.Reactive/Components/Onboarding/ComponentEventOnboarding.cs`
     - `Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs`
     - `Alis.Reactive/PlanModel/Values/ValueExpression.cs`
     - `Alis.Reactive/PlanAuthoring/Requests/GatherBuilder.cs`
     - condition, plugin, and array builders
   - JS property read: `ComponentProperty<T>.Named/Mapped(...)` + `self.Read(property)`.
   - JS property write: `ComponentProperty<T>` + `self.EmitSet(property, ValueExpression...)`.
   - JS void method: `ComponentMethod.Named/Mapped(...).WithArgs<...>()` + `self.EmitCall(method, args)`.
   - JS method returning value: `ComponentMethod...` + `self.Read<TReturn>(method, args)`.
   - JS event/callback: `TypedEvent<TArgs>` + `ComponentEventOnboarding.Wire(...)`.
   - Event arg mutation/call: emit `ReactionGraph.Set/Call` on `PayloadSource.Event()`.
   - JS overloaded methods: use distinct typed C# methods and distinct plan member names mapped to the same JS path, so contract merge remains deterministic.
   - Output `mapping/primitive-map.md`. Unresolved conflicts or stop conditions go in `proof/audit-report.md`.

7. **Vertical slice design**
   - Keep initial render options on `Syncfusion.EJ2.*Builder`.
   - Onboard only runtime gaps: methods, method-return reads, event payload access, event arg mutation/calls, and properties that reactive plans need to read/write after render.
   - If a member is only static configuration and the builder already exposes it, do not add a Fusion reactive extension for it.
   - Keep files under `Alis.Reactive.Fusion/Components/FusionXxx/`.
   - Preserve the component isolation pattern:
     `FusionXxx.cs`, `FusionXxxBuilder.cs`, `FusionXxxHtmlExtensions.cs`, `FusionXxxExtensions.cs`, `FusionXxxEvents.cs`, `FusionXxxReactiveExtensions.cs`, `Events/*`.
   - Input components implement the input registration pattern. App/display components do not register form bindings.
   - If Syncfusion ships no MVC builder, a Fusion slice may own a typed render helper.
     Keep that helper typed and bounded to real options, endpoints, and event payloads;
     do not expose raw JavaScript strings/functions as a public configuration surface.
   - Complex components may split extension partials by use case, such as query, editing, selection, row-data, tooling, and columns. Do not collapse a large slice into one flat file.
   - Output `mapping/vertical-slice-plan.md`.

8. **Implementation rules**
   - Implement only accepted mapped surface. Each public C# member must trace back to:
     `raw trace row -> discovery classification -> Blazor/name decision if used -> primitive-map row -> vertical slice file`.
   - Do not implement unproven candidates.
   - Do not preserve dead helpers because old tests reference them.
   - Do not add public `string memberName`, broad `object`, or escape hatches unless the API is intentionally a plugin boundary.
   - Do not change DSL primitives during component onboarding. Suspected primitive gaps are separate architecture work with their own DSL graph and matrix.

9. **100 percent typed API proof matrix**
   - Generate `proof/typed-api-coverage-matrix.md` from the implemented public API.
   - Every onboarded public member must have one row and a behavior proof.
   - A source member must be consumed by condition, gather, HTTP payload/header/route, DOM text/html, plugin argument, array transform, or component binding.
   - A write or void method must visibly change component/runtime state.
   - A method return source must be consumed by a realistic pipeline.
   - Event payload properties, writable payload properties, payload methods, nested payloads, arrays, and indexed paths must be proven through the typed event contract.
   - Builder-owned exclusions must be listed with the builder evidence that owns initial render configuration.

10. **Playwright behavior proof**
   - Add a real sandbox view using the typed API.
   - For stateful components, make the sandbox HTTP-backed and SQLite-backed by default. Use normal verbs (`POST`, `PUT`, `DELETE`) and prove reload after create/update/delete/move so the test covers a real app workflow. Use in-memory storage only for throwaway probes, not for final onboarding proof.
   - Write behavior tests against visible behavior and trace output, not internal plan JSON shortcuts.
   - Prove every onboarded member. If a typed method/property/event is added, it gets a sandbox behavior and a Playwright assertion.
   - When a member is a value source for gather, conditions, or HTTP, prove at least one consumer path. A displayed raw value is useful, but a source is not fully onboarded until a realistic pipeline consumes it.
   - After changing sandbox views/controllers/component slices, run Playwright once with
     build enabled so the SandboxApp assembly is recopied to the test output. Use
     `--no-build` only for repeat runs after that rebuilt output is known current.
   - Before closing the slice, run through [Automation gates](references/automation-gates.md).

11. **Audit report**
   - Write `proof/audit-report.md`.
   - For a new component, the report states all accepted members, excluded candidates, deferred proof, exact commands run, and the commit boundary.
   - For an existing component audit, treat existing C# and tests as evidence only. Rebuild the discovery artifact tree, map every public API member, and classify each as proven/correct, unproven, wrong name, stringly or too broad, builder duplicate, missing behavior proof, or deferred proof.
   - For a defect fix, include a "what went wrong" section with the wrong artifact row, the raw EJ2 proof that corrected it, every artifact updated, and the behavior proof that now closes the row.
   - Validate the workflow against the current onboarded component inventory before committing skill/process changes. Use [Workflow validation](references/workflow-validation.md) and explicitly stress-test Grid, Kanban, and Schedule.

## Capability Matrix

| JS Object API | Supported | Current Pattern |
|---|---:|---|
| `ej2.prop` read | Yes, when reactive state source is needed | `self.Read(ComponentProperty<T>)` |
| `ej2.prop = value` | Yes, when post-render mutation is needed | `self.EmitSet(ComponentProperty<T>, ValueExpression)` |
| `ej2.method()` | Yes | `self.EmitCall(ComponentMethod)` |
| `ej2.method(arg1, arg2, arg3)` | Yes | `ComponentMethod.WithArgs<T1,T2,T3>()` + `EmitCall` |
| `ej2.method(...) -> value` | Yes | `self.Read<TReturn>(ComponentMethod, args)` |
| `eventArgs.prop` read | Yes | typed event args + `FromEvent` / conditions |
| `eventArgs.array[index].prop` read | Yes | typed event args with `List<T>` and expression index path |
| `eventArgs.prop = value` | Yes | `ReactionGraph.Set(PayloadSource.Event(), ...)` |
| `eventArgs.method(args)` | Yes | `ReactionGraph.Call(PayloadSource.Event(), ...)` |
| builder-only static option | No | keep on Syncfusion MVC builder |
| arbitrary untyped member access in public DSL | No | onboard typed API or use plugin intentionally |

## Do / Do Not

| Do | Do Not |
|---|---|
| Start with raw HTML and real browser traces | Start by copying docs into C# |
| Read shipped JS source and d.ts files | Infer from builder names alone |
| Use Blazor XML/ILSpy to find clean C# vocabulary | Treat Blazor bridge-only fields as direct EJ2 payloads |
| Use official Syncfusion skills as extra context | Treat generated examples as source of truth |
| Run the surface inspector before hand-designing APIs | Build a prose-only inventory |
| Exclude builder-covered static configuration | Rewrap the MVC builder as reactive methods |
| Keep a trace matrix beside the work while designing | Rely on old unit tests or plan JSON assertions |
| Use exact Syncfusion runtime names for object paths | Invent friendlier JS member names without mapping |
| Expose typed C# methods/properties/events | Add `string methodName` APIs to component slices |
| Add only proven members | Onboard a broad component surface blindly |
| Prefer public JS members from source/prototype traces | Promote hidden/internal Syncfusion members found by the inspector |
| Trace event payload methods inside the live event lifecycle | Assume payload methods work because d.ts lists them |
| Keep runtime vendor-neutral | Add Syncfusion checks to execution/gather/conditions |
| Preserve sync behavior for normal component actions | Turn sync component calls into async work |

## Reference Files

- `Alis.Reactive.Fusion/Components/FusionSchedule/FusionScheduleExtensions.cs` - method return source and multi-arg method examples.
- `Alis.Reactive.Fusion/Components/FusionAccordion/FusionAccordionExtensions.cs` - two-arg component method call.
- `Alis.Reactive.Fusion/Components/FusionTooltip/` - non-input display component slice.
- `Alis.Reactive.Fusion/Components/FusionAutoComplete/` - input component with filtering event behavior.
- `Alis.Reactive/ComponentRef.cs` - `EmitSet`, `EmitCall`, and `Read<T>` domain entry points.
- `Alis.Reactive.Assets/runtime/domain/runtime-object.ts` - vendor-neutral property/method execution.
- `scripts/inspect-syncfusion-surface.mjs` - extracts JS surface plus MVC builder coverage.
- `scripts/inspect-syncfusion-blazor-metadata.mjs` - extracts Blazor typed metadata and ILSpy bridge clues from a local NuGet package.
- `scripts/inspect-syncfusion-event-payload.mjs` - extracts event payload properties/methods from Syncfusion d.ts files.
- `scripts/discover-syncfusion-component.mjs` - finds class package, d.ts, JS source, MVC builder, and next commands.
- `scripts/create-fusion-probe.mjs` - creates a temporary raw HTML probe.
- `references/source-discovery.md` - deterministic source-finding workflow.
- `references/blazor-metadata.md` - how to use Blazor packages as typed candidate maps without copying bridge-only behavior.
- `references/js-object-dsl-primitive-matrix.md` - authoritative JS object to DSL primitive mapping for component members, events, event payloads, methods, arrays, and stop conditions.
- `references/event-payload-contracts.md` - event payload property/method tracing and C# mapping.
- `references/automation-gates.md` - done criteria for a fully onboarded member/component.
- `references/workflow-validation.md` - current workflow/audit validation against onboarded Fusion components, with Grid/Kanban/Schedule stress cases.
