---
name: onboard-fusion-component
description: Onboards Syncfusion EJ2 components into Alis Reactive by proving the real JS object API in raw HTML, tracing properties/methods/events, then implementing a typed Fusion vertical slice. Use when adding a new Fusion component, adding props/methods/events to an existing Fusion component, or validating whether a Syncfusion API should become typed DSL.
disable-model-invocation: true
---

# Onboard Fusion Component

## Operating Rule

Do not onboard from memory, docs alone, or inferred API names. The source of truth is Syncfusion's shipped JS source plus a running raw HTML probe that instantiates the object, calls the exact API, captures event payloads, and records method return behavior.

The Syncfusion MVC builder remains the configuration surface for initial render. Do not duplicate builder-covered static properties in the reactive DSL unless they must be read or mutated after render. The Fusion slice adds typed runtime behavior: post-render property reads/writes, method calls, method return sources, and event-to-plan wiring.

The public DSL remains typed. Internal plan member names may be strings; developer-facing APIs must not expose arbitrary member strings except plugin escape hatches.

Loose typing in Syncfusion contracts is not permission to add loose typing in Alis. Public shapes such as `object`, `Record<string, any>`, or broad event args are candidate evidence only. The onboarding decision must narrow them through JS source and an HTML execution trace before exposing a typed C# API. If a member cannot be narrowed and proven, keep it out of the public Fusion slice.

## Workflow

1. **Pick the exact Syncfusion object**
   - Identify package, global namespace, class name, styles, and required scripts.
   - Example: `@syncfusion/ej2-interactive-chat` -> `ej.interactivechat.AIAssistView`.
   - If Syncfusion ships an official ASP.NET Core agent skill for the component, use it as a documentation accelerator only. It can suggest setup and builder usage, but it does not replace JS source, builder coverage, raw probe, or Playwright proof.
   - If any source path is unknown, read [Source discovery](references/source-discovery.md) and run the discovery helper before continuing:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/discover-syncfusion-component.mjs --class ChipList
     ```

2. **Generate the source-grounded surface matrix**
   - Prefer the helper before designing the slice:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-surface.mjs \
       --class AIAssistView \
       --dts node_modules/@syncfusion/ej2-interactive-chat/src/ai-assistview/ai-assistview.d.ts \
       --xml ~/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/netstandard2.0/Syncfusion.EJ2.xml
     ```
   - Treat the output as a candidate matrix. The final decision still depends on runtime need and browser proof.

3. **Use Blazor metadata as a typed candidate map**
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

4. **Create a raw HTML probe**
   - Prefer the helper:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/create-fusion-probe.mjs \
       --component ai-assistview \
       --namespace interactivechat \
       --class AIAssistView \
       --id ai-assist
     ```
   - Open the generated `/sf-ai-assistview-probe.html` in the sandbox.
   - See [HTML probe and API trace](references/html-probe-api-trace.md).
   - Record where the Syncfusion instance is actually stored. Most components keep
     `ej2_instances` on the rendered host. Target-attached components such as
     Mention can use a disposable MVC host while the stable runtime join key must
     stay elsewhere. If the MVC host can disappear, add a component-slice bridge
     that preserves the exact Syncfusion root under the developer-facing component id
     before reactive boot.

5. **Trace the API before C#**
   - Build a matrix with one row per member:
     `JS expression -> member kind -> builder coverage -> args -> return -> event payload -> visual/runtime proof -> typed C# API`.
   - A row is not accepted until the HTML page proves the behavior.
   - Syncfusion public d.ts contracts often use loose shapes such as
     `Record<string, any>` or broad event arg objects. Do not copy that looseness
     into Alis. Use the HTML probe to execute the member, print the actual
     payload/return shape, and record the trace that justifies the typed C# shape.
   - Accept public JS API first. Inspector output may include hidden/internal members; skip those unless there is no public API for the real behavior and the exception is explicitly documented in the matrix.
   - Prove property reads and writes separately. A readable Syncfusion property is not automatically a valid reactive write API for every builder configuration.
   - If a property write requires a public flush method such as `dataBind()`, capture that in the matrix and encapsulate it in the typed Fusion API. Do not expose a write method when the raw browser proof is not stable.
   - For each `EmitType<TArgs>` event selected, read [Event payload contracts](references/event-payload-contracts.md), inspect `TArgs`, and prove payload properties, writable properties, and methods in raw HTML:
     ```bash
     node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-event-payload.mjs --type FilteringEventArgs
     ```
   - If a payload property is an array, keep it typed as `List<T>`/typed array in the C# event contract. Prove it either through a typed indexed read (`args => args.Data[0].Summary`) or a whole-array gather into a real HTTP workflow. Do not add untyped element accessors to component slices. If the app needs dynamic array projection/filtering/reduction beyond the typed member path, use a plugin escape hatch.

6. **Apply the builder coverage gate**
   - Keep initial render options on `Syncfusion.EJ2.*Builder`.
   - Onboard only runtime gaps: methods, method-return reads, event payload access, event arg mutation/calls, and properties that reactive plans need to read/write after render.
   - If a member is only static configuration and the builder already exposes it, do not add a Fusion reactive extension for it.

7. **Map traced API to the current model**
   - Read [JS object DSL primitive matrix](references/js-object-dsl-primitive-matrix.md)
     before designing the public C# API.
   - JS property read: `ComponentProperty<T>.Named/Mapped(...)` + `self.Read(property)`.
   - JS property write: `ComponentProperty<T>` + `self.EmitSet(property, ValueExpression...)`.
   - JS void method: `ComponentMethod.Named/Mapped(...).WithArgs<...>()` + `self.EmitCall(method, args)`.
   - JS method returning value: `ComponentMethod...` + `self.Read<TReturn>(method, args)`.
   - JS event/callback: `TypedEvent<TArgs>` + `ComponentEventOnboarding.Wire(...)`.
   - Event arg mutation/call: emit `ReactionGraph.Set/Call` on `PayloadSource.Event()`.
   - JS overloaded methods: use distinct typed C# methods and distinct plan member names mapped to the same JS path, so contract merge remains deterministic.

8. **Implement the vertical slice**
   - Keep files under `Alis.Reactive.Fusion/Components/FusionXxx/`.
   - Preserve the component isolation pattern:
     `FusionXxx.cs`, `FusionXxxBuilder.cs`, `FusionXxxHtmlExtensions.cs`, `FusionXxxExtensions.cs`, `FusionXxxEvents.cs`, `FusionXxxReactiveExtensions.cs`, `Events/*`.
   - Input components implement the input registration pattern. App/display components do not register form bindings.
   - If Syncfusion ships no MVC builder, a Fusion slice may own a typed render helper.
     Keep that helper typed and bounded to real options, endpoints, and event payloads;
     do not expose raw JavaScript strings/functions as a public configuration surface.

9. **Prove through sandbox and Playwright**
   - Add a real sandbox view using the typed API.
   - For stateful components, make the sandbox HTTP-backed and SQLite-backed by default. Use normal verbs (`POST`, `PUT`, `DELETE`) and prove reload after create/update/delete/move so the test covers a real app workflow. Use in-memory storage only for throwaway probes, not for final onboarding proof.
   - Write behavior tests against visible behavior and trace output, not internal plan JSON shortcuts.
   - Prove every onboarded member. If a typed method/property/event is added, it gets a sandbox behavior and a Playwright assertion.
   - When a member is a value source for gather, conditions, or HTTP, prove at least one consumer path. A displayed raw value is useful, but a source is not fully onboarded until a realistic pipeline consumes it.
   - After changing sandbox views/controllers/component slices, run Playwright once with
     build enabled so the SandboxApp assembly is recopied to the test output. Use
     `--no-build` only for repeat runs after that rebuilt output is known current.
   - Before closing the slice, run through [Automation gates](references/automation-gates.md).

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
