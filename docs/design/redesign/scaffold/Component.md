# Implementation Spec — `Component` Micro-Module

> **How to use this file.** Open it, read §1–§3 to learn the surface, copy the
> §6 skeleton into the named files in §5, then fill each `// TODO` body using the
> fixtures named in §7. Every type and signature below is grounded in actual
> source (cited by `file:line`); nothing is invented. When a body decision is not
> obvious, the matrix case in §7 fixes it — there are no open design questions.
>
> **Source of truth read for this spec** (all under repo root):
> `PlanModel/ComponentObject.cs`, `PlanModel/ComponentObjects.cs`,
> `PlanModel/BrowserObjectContract.cs`, `PlanModel/PlanTerms.cs`,
> `IdGenerator.cs`, `IComponent.cs`, `ComponentRegistration.cs`,
> `RegisteredInputComponents.cs`, `RegisteredComponentIdentity.cs`,
> `InputComponentRegistrationProfile.cs`,
> `ComponentOnboarding/ModelBoundInputComponentSlot.cs`,
> `ComponentOnboarding/ComponentObjectTarget.cs`, `ComponentRef.cs`,
> `Razor/Extensions/InputFieldExtensions.cs`,
> `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxHtmlExtensions.cs`;
> runtime: `Alis.Reactive.Assets/runtime/domain/runtime-plan.ts`,
> `runtime/domain/runtime-object.ts`, `runtime/domain/component-runtime.ts`,
> `runtime/resolution/resolver.ts`, `runtime/types/plan.ts` (lines 82–179, 367–398).
>
> Design inputs: [`00-design.md`](../00-design.md),
> [`02-micro-modules.md`](../02-micro-modules.md) (row **Component**),
> [`03-naming.md`](../03-naming.md) (§Component), and
> [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
> **Band B** (B1–B4) + **Band E** (app-level objects).

---

## 1. Responsibility, Ownership, Dependencies

**Responsibility (one sentence).** Component is the **component-id + browser-object
spine**: it turns a model expression into one deterministic id, registers each
browser object (id / vendor / type / role / binding) with its declared member
contract, and resolves that object once at runtime via `getElementById` + the
sole vendor driver — so every other slice reads, sets, calls, validates, and
gathers against the same id.

**What it owns** (from `02-micro-modules.md` row Component; names from `03-naming.md` §Component):

| Side | Owned concept | New name | Today (replaced) |
|---|---|---|---|
| `→` author | Deterministic id from `(model type, expression)` | `IdGenerator` *(kept)* | `IdGenerator.cs` |
| `→` author | The one model-bound input render slot | `ModelBoundInputComponentSlot` + `InputBoundField` + `Html.InputField` | kept |
| `→` author | The render-time input registration profile | `InputComponentRegistrationProfile` + `ComponentRegistration` + `RegisteredInputComponents` | kept |
| `→` plan | A registered browser object | `BrowserObject` | `ComponentObject` (god-file, 677 lines — split) |
| `→` plan | What an object entry is *for* | `ComponentRole` *(kept)* | `ComponentRole` |
| `→` plan | The model property a plan-input binds to | `InputBinding` (`RegisteredInputBinding` / `NoInputBinding`) | kept |
| `→` plan | The repository of declared objects + same-vendor invariant | `BrowserObjects` | `ComponentObjects` |
| `→` plan | The vendor-agnostic member contract | `BrowserObjectContract` *(kept)* + `ObjectProperty`/`ObjectMethod`/`ObjectEvent` | kept |
| `→` plan | The `(vendor, kind, id)` identity value object | `BrowserObjectId` | `TypeKey` (opaque parsed string — renamed) |
| `→` plan | The non-input/app-level object target | `ComponentObjectTarget` (`ObjectTarget` / `LayoutObjectTarget`) | kept |
| `→` plan | The pipeline-side reference for set/call/read | `ComponentRef<TComponent, TModel>` | kept |
| `⇒` runtime | The active plan's component lookup | `RuntimeComponents` (memoized in `RuntimePlan`) | `runtime-plan.ts` |
| `⇒` runtime | One resolved object (DOM element + vendor root) | `RuntimeObject` *(kept; memoized)* | rebuilt per read |
| `⇒` runtime | The per-vendor driver — **the sole vendor seam** | `ComponentDriver` | `ComponentRuntime` (`component-runtime.ts`) |
| `⇒` runtime | Vendor event wiring — the only vendor-aware code | `wireFusionEvent` / `wireNativeEvent` | `event-fusion.ts` / `event-native.ts` (kept; `resolver.ts` Rule-5 claim fixed) |

**What it depends on** (acyclic — `02-micro-modules.md` dependency graph):

- **Value** — a `plan-input`'s value reads via `ValueExpression.Read(ComponentSource)`;
  `ComponentRef.Read<T>()` returns a `TypedComponentSource<T>` that *is* a `TypedSource`.
- **Shape** — `Shape.FromClrType(typeof(TProp))` infers the value shape at
  registration; the member contract carries `Shape` on every property/method/arg.
- **Kind** — every plan node Component emits (`ComponentRole`, `InputBinding`,
  `MethodArgumentContract`) carries a `kind` discriminator written by
  `PlanNodeDiscriminator` → `PlanSerializer`; the runtime switches end in `assertNever`.

Component depends on **nothing else**. It does **not** depend on Reaction,
Request, Condition, Validation, Plugin, Trigger, or Plan. (Validation,
Reaction, Trigger, Request, and Plugin depend *on* Component.)

---

## 2. Public Surface

> "Public" here means **the surface other modules and the author seam call** —
> per Rule 8 the plan-model constructors stay `internal`; the only `public`
> C# types are the developer-facing ones (`IdGenerator`, `IComponent`/
> `IInputComponent`/`IAppLevelComponent`, `ComponentRole`, `InputBinding`,
> `ValidationContainerBinding`, `ComponentRegistration`, `ComponentRef<,>`).
> The TS surface is what crosses the contract.

### 2.1 C# author seam (developer-facing)

```csharp
// IdGenerator.cs (kept verbatim — the ONE id regime)
public static class IdGenerator
{
    public static string For<TModel>(Expression<Func<TModel, object?>> expression);
    public static string For<TModel, TProp>(Expression<Func<TModel, TProp>> expression);
    public static string For(Type modelType, string propertyPath);
    public static string TypeScope(Type type);
}

// IComponent.cs (kept — vendor marker interfaces a slice implements)
public interface IComponent              { string Vendor { get; } }
public interface IInputComponent : IComponent      { string ValueMember { get; } }
public interface IAppLevelComponent : IComponent   { string DefaultId  { get; } }
```

`Html.InputField(plan, m => m.Prop, configure?)` (`InputFieldExtensions.cs:33,55`)
opens an `InputBoundField<TModel, TProp>` setup; the slice extension
(`.NativeTextBox(...)`, `.FusionDropDownList(...)`) calls
`setup.RegisterInputComponent(IInputComponent.Registration)` then `setup.Render(builder)`
(`NativeTextBoxHtmlExtensions.cs:33-41`). **Component owns the registration +
slot; each vendor slice owns its own builder (duplication over abstraction).**

### 2.2 C# plan-model surface (called by other slices via `PlanBuildContext`)

The repository — every other slice declares objects through it:

```csharp
internal sealed class BrowserObjects            // renamed from ComponentObjects
{
    internal BrowserObjects(BrowserObjectContracts objectContracts,
                            RegisteredInputComponents registrations);

    internal IReadOnlyDictionary<string, BrowserObject> Snapshot();
    internal BrowserObject Get(ComponentKey key);
    internal void          Set(ComponentKey key, BrowserObject component);

    internal ComponentKey DeclareElement(string elementId);                 // native element, object-target
    internal ComponentKey DeclareObjectTarget(string componentId, string vendor);
    internal ComponentKey DeclareLayoutObject(string componentId, string vendor);
    internal ComponentKey DeclareInputComponent(InputComponentPlanBinding input);
    internal void         RegisterInputComponents();                        // flush registrations → plan-inputs

    internal void         DeclareProperty(ComponentKey key, ObjectPropertyContract c);
    internal ObjectMethod DeclareMethod  (ComponentKey key, ObjectMethodContract c);
    internal void         DeclareEvent   (ComponentKey key, ObjectEventContract c);

    internal ComponentRegistration RequireRegistrationById(string componentId, RegisteredInputValueRead read);
}
```

The object node + its factories (one role per factory — no public ctor):

```csharp
internal sealed class BrowserObject              // renamed from ComponentObject
{
    public string        Id        { get; }      // BrowserObjectId.Id
    public string        Vendor    { get; }      // BrowserObjectId.Vendor
    public string        Type      { get; }      // BrowserObjectId value
    public ComponentRole Role      { get; }
    public InputBinding  Binding   { get; }
    public ValidationContainerBinding Container  { get; }

    internal static BrowserObject Element     (ComponentId id, ComponentVendor vendor, BrowserObjectId type);
    internal static BrowserObject LayoutObject(ComponentId id, ComponentVendor vendor, BrowserObjectId type);
    internal static BrowserObject PlanInput   (ComponentId id, ComponentVendor vendor, BrowserObjectId type, InputBinding binding);

    internal BrowserObject WithBindingIfAbsent     (InputBinding binding);          // first registration wins
    internal BrowserObject WithContainer           (ContainerScope container);
    internal BrowserObject WithValidationRulesMerged(IReadOnlyList<ComponentValidation> rules);
}
```

The identity value object (renamed from `TypeKey` — `03-naming.md` collision row):

```csharp
internal sealed class BrowserObjectId : PlanString    // was TypeKey
{
    internal static BrowserObjectId Of(string value);
    internal static BrowserObjectId NativeElement   (ComponentId id);                       // "native.element.{id}"
    internal static BrowserObjectId ComponentObject (ComponentVendor vendor, ComponentId id);// "{vendor}.component.{id}"
    internal static BrowserObjectId Plugin          (PluginName name);                       // "plugin.{name}"
}
```

The pipeline-side reference (called by every slice's mutation/read extensions):

```csharp
public class ComponentRef<TComponent, TModel>     // kept; ctors internal
{
    internal string IdForJson { get; }
    internal PipelineBuilder<TModel> Pipeline { get; }

    internal ComponentRef<TComponent, TModel> EmitSet<TValue>(ComponentProperty member, ValueExpression value);
    internal ComponentRef<TComponent, TModel> EmitCall(ComponentMethod method);
    internal ComponentRef<TComponent, TModel> EmitCall(ComponentMethod method, IReadOnlyList<ValueExpression> args);
    internal TypedComponentSource<TValue>     Read<TValue>(ComponentProperty member /* + overloads */);
}
```

The role + binding value objects (kept names, public abstract bases, internal factories):

```csharp
public sealed class ComponentRole
{
    public string Kind { get; }                       // "object-target" | "plan-input" | "validation-container" | "layout-object"
    internal static ComponentRole ObjectTarget        { get; }
    internal static ComponentRole PlanInput           { get; }
    internal static ComponentRole ValidationContainer { get; }
    internal static ComponentRole LayoutObject        { get; }
}

[JsonConverter(typeof(WriteOnlyPolymorphicConverter<InputBinding>))]
public abstract class InputBinding
{
    public abstract string Kind { get; }              // "none" | "registered-input"
    internal static InputBinding None { get; }
    internal static InputBinding RegisteredInput(BindingPath bindingPath, MemberName valueMember);
    internal abstract InputBinding FillIfAbsent(InputBinding incoming);
}
```

### 2.3 TS runtime surface (crosses the contract)

The contract `plan.ts` types Component produces (kept; `plan.ts:82-179`):

```ts
export type ComponentObject =
  | ObjectTargetComponent | PlanInputComponent
  | ValidationContainerComponentDefinition | LayoutObjectComponent;
// each: { id: string; vendor: Vendor; type: string; role: ComponentRole; binding: InputBinding; container: ValidationContainerBinding }

export interface ComponentSource { kind: "component"; component: string }   // plan.ts:380
```

The runtime readers (split out of today's `runtime-plan.ts`; memoized):

```ts
// RuntimeComponents — lookup of all components in the active plan
export class RuntimeComponents {
  find(componentKey: string): RuntimeComponent | undefined;
  entries(): RuntimeComponent[];
  component(componentKey: string): RuntimeComponent;        // throws RuntimeResolutionError if not active
  element(componentKey: string): HTMLElement;
}

// RuntimeComponent — one entry; resolves DOM + vendor root
export class RuntimeComponent {
  get id(): string;
  element(): HTMLElement;                                   // getElementById — throws if missing
  tryElement(): HTMLElement | undefined;
  root(): unknown;                                          // vendor root via ComponentDriver
  object(): RuntimeObject;                                  // DOM + contract, ready for read/set/call
  runtime(): ComponentDriver;                               // the per-vendor driver
}

// RuntimeObject — read/set/call against a resolved object (runtime-object.ts, kept)
export class RuntimeObject {
  read(member: string): RuntimeValue;
  set (member: string, value: unknown): void;
  call(member: string, args: unknown[]): RuntimeValue;
}

// ComponentDriver — THE sole vendor seam (renamed from ComponentRuntime)
export interface ComponentDriver {
  resolveRoot(element: HTMLElement): unknown;
  wireEvent(root: unknown, channel: string, handler: (d: unknown) => void, opts?: AddEventListenerOptions): void;
}
export function registerComponentDriver(vendor: Vendor, driver: ComponentDriver): void;

// resolver.ts — the only public event-wiring entry (Trigger/Reaction call it)
export function wireEvent(plan: PlanDocument, componentKey: string, channel: string,
                          handler: (d: unknown) => void, opts?: AddEventListenerOptions): void;
```

---

## 3. Input → Output Contract

### 3.1 Author → plan (the `→` write path)

| In | Step | Out |
|---|---|---|
| `(typeof TModel, m => m.Prop)` | `IdGenerator.For` → `TypeScope + "__" + path` | deterministic `componentId` string (vendor-agnostic) |
| `Html.InputField(plan, m=>m.Prop)` + `.XxxInput(...)` | builds `ModelBoundInputComponentSlot` (`id`, `BindingPath`, `Shape.FromClrType`); `RegisterInputComponent(slice.Registration)` → `RegisteredInputComponents.Add(bindingPath, ComponentRegistration)` | a queued `ComponentRegistration` |
| `BrowserObjects.RegisterInputComponents()` at build flush | each registration → `DeclareInputComponent(PlanBinding)` | `components[id] = BrowserObject.PlanInput(...)` + its `BrowserObjectContract` under `types` |
| `p.Component<TComp>(m=>m.Prop)` / `p.Component<TComp>("id")` | `ComponentObjectTarget.DeclareIn` → `DeclareObjectTarget` | `components[id] = BrowserObject.Element(...)` (`object-target`) |
| `@Html.NativeDrawer()` + `p.Drawer()` | `ComponentObjectTarget.ForLayout<T>()` → `DeclareLayoutObject` | `components[fixedId] = BrowserObject.LayoutObject(...)` (`layout-object`) |
| `ref.EmitSet/EmitCall` / `ref.Read<T>()` | adds `ReactionGraph.Set/Call` (Reaction owns) / returns a `ValueExpression.Read(ComponentSource)` (Value owns) | a node addressing `{component id, member}` |

### 3.2 Plan → runtime (the `⇒` read path)

| In (plan JSON) | Step | Out (browser) |
|---|---|---|
| `components[key]` + `RuntimeObjectSource{kind:"component", component:key}` | `RuntimeComponents.component(key)` → `RuntimeComponent` | resolved entry (memoized via `RuntimePlan` `WeakMap` cache) |
| `RuntimeComponent.element()` | `document.getElementById(id)` | `HTMLElement` (throws `RuntimeResolutionError.elementNotFound` if absent — real DOM boundary) |
| `RuntimeComponent.root()` | `ComponentDriver.resolveRoot(element)` | native: the element itself; fusion: `element.ej2_instances[0]` (throws `RuntimeComponentReadinessError` if absent) |
| `RuntimeObject.read/set/call(member,…)` | look up member in `objectContract`, walk `RuntimePath`, shape once | value read / member set / method called |

### 3.3 Invariants (value-object constructor invariants; **null unrepresentable by construction**)

- **Same vendor.** A component referenced twice with differing vendors throws
  `InvalidOperationException` ("a component cannot change vendor") —
  `ComponentObjects.RequireSameVendor` (`ComponentObjects.cs:143-149`). This is an
  **authoring boundary**, not a runtime guard.
- **First registration wins.** `WithBindingIfAbsent` keeps an existing binding;
  `RegisteredInputBinding.FillIfAbsent` returns `this`, `NoInputBinding.FillIfAbsent`
  returns the incoming (`ComponentObject.cs:73-74,130-157`).
- **Idempotent duplicate registration.** Same binding path + same contract = no-op;
  conflicting contract throws (`RegisteredInputComponents.cs:21-33`).
- **Vendor token shape.** `ComponentVendor` enforces `^[a-zA-Z][a-zA-Z0-9_-]*$` in
  its ctor (`PlanTerms.cs:255-268`) — an invalid vendor is unrepresentable, not
  defended later.
- **Null is structural, not exceptional.** A non-input object carries
  `InputBinding.None` (the `none` variant) and `ValidationContainerBinding.None`,
  never C# `null`. There is no "missing binding" sentinel: the *kind* says
  `none`. The runtime narrows on `binding.kind === "none"` (`runtime-plan.ts:15,132-137`),
  never on a null check. **Do not add `?`/`?? fallback` to any new member** —
  every absent value already has a `None`/empty variant (Rule 6 null-escape-hatch gate).
- **Memoize, don't rebuild.** `RuntimePlan.from(plan)` caches by `PlanDocument`
  in a `WeakMap` (`runtime-plan.ts:13,57-64`); a sequence of sets resolves the DOM
  element + vendor root through the same `RuntimeObject`, not per step.

---

## 4. Vendor Isolation (Rule 5 — the sole seam)

`ComponentDriver` + `wireFusionEvent` (`event-fusion.ts`) + `wireNativeEvent`
(`event-native.ts`) are the **only** vendor-aware code. `resolver.ts` is a thin
caller that delegates — its old "sole vendor seam" comment is corrected to name
`ComponentDriver` honestly (`03-naming.md` collision row). **A third vendor adds
exactly one `resolution/event-{vendor}.ts` + one `registerComponentDriver` call;
zero changes anywhere else.** No `if (vendor === "fusion")` check may appear in
`RuntimeObject`, `RuntimeComponents`, `runtime-plan.ts`, or any other module.

---

## 5. File Layout

> **C# — keep the existing folders; the work is a split + rename, not new homes.**
> The Component slice's plan-model files live under `Alis.Reactive/PlanModel/`,
> the author/registration files under `Alis.Reactive/` and
> `Alis.Reactive/ComponentOnboarding/`. The vertical-slice vendor extensions stay
> in their own component folders (`Alis.Reactive.Native/Components/*`,
> `Alis.Reactive.Fusion/Components/*`) — Component does **not** own them.

```
Alis.Reactive/
├── IdGenerator.cs                                  (kept verbatim)
├── IComponent.cs                                   (kept)
├── ComponentRef.cs                                 (kept)
├── ComponentRegistration.cs                        (kept)
├── RegisteredInputComponents.cs                    (kept)
├── RegisteredComponentIdentity.cs                  (kept)
├── RegisteredInputBinding.cs                       (kept)
├── InputComponentRegistrationProfile.cs            (kept)
├── ComponentOnboarding/
│   ├── ModelBoundInputComponentSlot.cs             (kept)
│   ├── ComponentObjectTarget.cs                    (kept)
│   └── ComponentEventOnboarding.cs                 (kept; Trigger/Reaction join)
└── PlanModel/
    ├── BrowserObject.cs        ← SPLIT OUT of ComponentObject.cs: BrowserObject + ComponentRole + InputBinding + InputValueContract + InputComponentPlanBinding only
    ├── BrowserObjects.cs       ← renamed from ComponentObjects.cs (ComponentObject → BrowserObject, TypeKey → BrowserObjectId)
    ├── BrowserObjectId.cs      ← EXTRACTED from PlanTerms.cs (was TypeKey)
    ├── BrowserObjectContract.cs (kept; ObjectProperty/Method/Event/MethodSignature/MethodArgumentContract)
    └── BrowserObjectContracts.cs (kept)
    # ContainerScope / ContainerValidations / ComponentValidation / ValidationRule*
    #   MOVE to the Validation slice's home (ValidationGraph) — they leave ComponentObject.cs.
```

```
Alis.Reactive.Assets/runtime/
├── domain/
│   ├── runtime-plan.ts        (kept; RuntimePlan + RuntimeComponents + RuntimeComponent + memoize cache)
│   ├── runtime-object.ts      (kept verbatim)
│   └── component-driver.ts    ← renamed from component-runtime.ts (ComponentRuntime → ComponentDriver, register* renamed)
├── resolution/
│   ├── resolver.ts            (kept; wireEvent delegate — fix the stale Rule-5 comment)
│   ├── event-fusion.ts        (kept; wireFusionEvent)
│   └── event-native.ts        (kept; wireNativeEvent)
└── types/
    └── plan.ts                (Component section generated by Kind's PlanContractGenerator — do NOT hand-edit)
```

---

## 6. Compile-Ready Skeleton

> Type declarations + stubs. Bodies are `// TODO (fixture: …)` — fill each from the
> named §7 fixture. Where a body is mechanical and already exists in source, the
> TODO says **copy** and cites the line.

### 6.1 `PlanModel/BrowserObjectId.cs`

```csharp
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Identifies one browser object: the <c>(vendor, kind, id)</c> token a
    /// component's member contract is keyed by. Was <c>TypeKey</c>; renamed because
    /// "TypeKey" never said what it keyed.</summary>
    internal sealed class BrowserObjectId : PlanString
    {
        private BrowserObjectId(string value) : base(value, nameof(value)) { }

        internal static BrowserObjectId Of(string value) => new BrowserObjectId(value);

        /// <summary>A native HTML element addressed only by id.</summary>
        internal static BrowserObjectId NativeElement(ComponentId componentId) =>
            Of("native.element." + componentId.Value);          // TODO copy PlanTerms.cs:136

        /// <summary>A vendor component object addressed by id.</summary>
        internal static BrowserObjectId ComponentObject(ComponentVendor vendor, ComponentId componentId) =>
            Of(vendor.Value + ".component." + componentId.Value); // TODO copy PlanTerms.cs:137

        /// <summary>A declared plugin object.</summary>
        internal static BrowserObjectId Plugin(PluginName pluginName) =>
            Of("plugin." + pluginName.Value);                    // TODO copy PlanTerms.cs:138
    }
}
```

### 6.2 `PlanModel/BrowserObject.cs` (the split target)

```csharp
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>A registered browser object the runtime talks to: an id, a vendor,
    /// a type, a role, and (for inputs) a model binding. Was <c>ComponentObject</c>;
    /// validation-container concerns moved to the Validation slice.</summary>
    internal sealed class BrowserObject
    {
        private readonly ComponentId _id;
        private readonly ComponentVendor _vendor;
        private readonly BrowserObjectId _type;
        private readonly ComponentRole _role;
        private readonly InputBinding _binding;
        private readonly ValidationContainerBinding _container;

        public string Id => _id.Value;
        public string Vendor => _vendor.Value;
        public string Type => _type.Value;
        public ComponentRole Role => _role;
        public InputBinding Binding => _binding;
        public ValidationContainerBinding Container => _container;

        private BrowserObject(/* …six args… */)
        {
            // TODO (fixture: invariant_all_join_keys_non_null) — copy ctor null-guards from ComponentObject.cs:33-38
        }

        internal static BrowserObject Element(ComponentId id, ComponentVendor vendor, BrowserObjectId type)
        {
            // TODO (fixture: object_target_render) — role=ObjectTarget, binding=None, container=None (ComponentObject.cs:41-48)
            throw new System.NotImplementedException();
        }

        internal static BrowserObject LayoutObject(ComponentId id, ComponentVendor vendor, BrowserObjectId type)
        {
            // TODO (fixture: drawer_render_open_close_size) — role=LayoutObject, binding=None, container=None
            throw new System.NotImplementedException();
        }

        internal static BrowserObject PlanInput(ComponentId id, ComponentVendor vendor, BrowserObjectId type, InputBinding binding)
        {
            // TODO (fixture: inputfield_native_input) — role=PlanInput, binding=binding, container=None
            throw new System.NotImplementedException();
        }

        internal BrowserObject WithBindingIfAbsent(InputBinding binding)
        {
            // TODO (fixture: invariant_first_registration_wins) — _binding.FillIfAbsent(binding), role stays PlanInput (ComponentObject.cs:73-74)
            throw new System.NotImplementedException();
        }

        internal BrowserObject WithContainer(ContainerScope container) =>
            // TODO (fixture: server_validation_errors) — role=ValidationContainer, container=Scoped(container)
            throw new System.NotImplementedException();

        internal BrowserObject WithValidationRulesMerged(IReadOnlyList<ComponentValidation> rules) =>
            // TODO (fixture: nested_child_validator) — role=ValidationContainer, container.WithValidationRulesMerged(rules)
            throw new System.NotImplementedException();
    }

    /// <summary>What a browser-object entry is for. The deterministic role discriminator.</summary>
    public sealed class ComponentRole
    {
        private readonly string _kind;
        private ComponentRole(string kind) { /* TODO null-guard (ComponentObject.cs:88-91) */ }

        public string Kind => _kind;

        internal static ComponentRole ObjectTarget        { get; } = new ComponentRole("object-target");
        internal static ComponentRole PlanInput           { get; } = new ComponentRole("plan-input");
        internal static ComponentRole ValidationContainer { get; } = new ComponentRole("validation-container");
        internal static ComponentRole LayoutObject        { get; } = new ComponentRole("layout-object");
    }

    /// <summary>The model property a <c>plan-input</c> binds to, or <c>None</c> for a non-input object.
    /// Absence is the <c>none</c> variant — never C# null.</summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(Serialization.WriteOnlyPolymorphicConverter<InputBinding>))]
    public abstract class InputBinding
    {
        private protected InputBinding() { }
        public abstract string Kind { get; }

        internal static InputBinding None { get; } = new NoInputBinding();
        internal static InputBinding RegisteredInput(BindingPath bindingPath, MemberName valueMember) =>
            new RegisteredInputBinding(bindingPath, valueMember);   // TODO copy ComponentObject.cs:118-121

        internal abstract InputBinding FillIfAbsent(InputBinding incoming);
    }

    internal sealed class NoInputBinding : InputBinding
    {
        public override string Kind => "none";
        internal override InputBinding FillIfAbsent(InputBinding incoming) => incoming; // TODO copy ComponentObject.cs:130-134
    }

    internal sealed class RegisteredInputBinding : InputBinding
    {
        // fields: BindingPath, MemberName
        public override string Kind => "registered-input";
        public string BindingPath { get; }   // TODO from BindingPath.Value
        public Path   Path        { get; }    // TODO from BindingPath.Path
        public string ValueMember { get; }    // TODO from MemberName.Value
        internal override InputBinding FillIfAbsent(InputBinding incoming) => this; // TODO copy ComponentObject.cs:153-157
    }

    // InputValueContract + InputComponentPlanBinding: copy verbatim from ComponentObject.cs:160-248
    // (they belong to Component — they build the plan-input contract + binding).
}
```

### 6.3 `PlanModel/BrowserObjects.cs` (renamed `ComponentObjects.cs`)

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Alis.Reactive.PlanModel
{
    /// <summary>The repository of declared browser objects, with the same-vendor
    /// invariant. Was <c>ComponentObjects</c>.</summary>
    internal sealed class BrowserObjects
    {
        private readonly BrowserObjectContracts _objectContracts;
        private readonly Dictionary<string, BrowserObject> _components = new();
        private readonly RegisteredInputComponents _registrations;

        internal BrowserObjects(BrowserObjectContracts objectContracts, RegisteredInputComponents registrations)
        { /* TODO assign (ComponentObjects.cs:13-19) */ }

        internal IReadOnlyDictionary<string, BrowserObject> Snapshot() =>
            new Dictionary<string, BrowserObject>(_components);
        internal BrowserObject Get(ComponentKey key) => _components[key.Value];
        internal void Set(ComponentKey key, BrowserObject component) => _components[key.Value] = component;

        internal ComponentKey DeclareElement(string elementId)
        {
            // TODO (fixture: object_target_render) — copy ComponentObjects.cs:30-40 (idempotent; NativeElement id; DeclareObject)
            throw new NotImplementedException();
        }

        internal ComponentKey DeclareObjectTarget(string componentId, string vendor) =>
            DeclareComponentObject(componentId, vendor, BrowserObject.Element);     // ComponentObjects.cs:42
        internal ComponentKey DeclareLayoutObject(string componentId, string vendor) =>
            DeclareComponentObject(componentId, vendor, BrowserObject.LayoutObject); // ComponentObjects.cs:45

        private ComponentKey DeclareComponentObject(string componentId, string vendor,
            Func<ComponentId, ComponentVendor, BrowserObjectId, BrowserObject> createUnregistered)
        {
            // TODO (fixtures: object_target_render, component_property_set, invariant_vendor_cannot_change)
            //   copy ComponentObjects.cs:48-76: same-vendor check, registration upgrade-to-plan-input, else unregistered.
            throw new NotImplementedException();
        }

        internal ComponentKey DeclareInputComponent(InputComponentPlanBinding input)
        {
            // TODO (fixture: inputfield_native_input / inputfield_fusion_input)
            //   copy ComponentObjects.cs:78-98: idempotent merge w/ same-vendor + WithBindingIfAbsent; else PlanInput.
            throw new NotImplementedException();
        }

        internal void RegisterInputComponents()
        {
            // TODO (fixture: inputfield_native_input) — flush each registration via DeclareInputComponent (ComponentObjects.cs:118-122)
        }

        internal void         DeclareProperty(ComponentKey key, ObjectPropertyContract c) { /* TODO copy :100-104 */ }
        internal ObjectMethod DeclareMethod  (ComponentKey key, ObjectMethodContract c)  { /* TODO copy :106-110 */ throw new NotImplementedException(); }
        internal void         DeclareEvent   (ComponentKey key, ObjectEventContract c)    { /* TODO copy :112-116 */ }

        internal ComponentRegistration RequireRegistrationById(string componentId, RegisteredInputValueRead read) =>
            // TODO copy :124-141
            throw new NotImplementedException();

        private bool TryFindRegistration(ComponentId id, [NotNullWhen(true)] out ComponentRegistration? r) =>
            _registrations.TryFindForComponent(id, out r);

        private static void RequireSameVendor(BrowserObject existing, ComponentId id, ComponentVendor vendor)
        {
            // TODO (fixture: invariant_vendor_cannot_change) — copy ComponentObjects.cs:143-149 (throw on mismatch)
        }

        private void EnrichExistingComponent(BrowserObject existing, ComponentId id) { /* TODO copy :151-159 */ }
    }
}
```

### 6.4 `runtime/domain/component-driver.ts` (renamed `component-runtime.ts`)

```ts
import type { Vendor } from "../types";
import { wire as wireFusionEvent } from "../resolution/event-fusion";
import { wire as wireNativeEvent } from "../resolution/event-native";

/** The per-vendor driver — THE sole place vendor knowledge lives. Was ComponentRuntime. */
export interface ComponentDriver {
  resolveRoot(element: HTMLElement): unknown;
  wireEvent(root: unknown, channel: string, handler: (data: unknown) => void,
            opts: AddEventListenerOptions | undefined): void;
}

export class RuntimeComponentReadinessError extends Error {
  // TODO copy component-runtime.ts:20-41 (vendorRootMissing + is)
}

const drivers = new Map<Vendor, ComponentDriver>();

export function registerComponentDriver(vendor: Vendor, driver: ComponentDriver): void {
  // TODO (fixture: vendor_isolation_third_vendor) — throw if already registered (component-runtime.ts:88-92)
}

export function requireComponentDriver(componentId: string, vendor: Vendor): ComponentDriver {
  // TODO copy component-runtime.ts:81-86 (throw "not registered" at the real boundary)
  throw new Error("TODO");
}

const nativeDriver: ComponentDriver = {
  resolveRoot: element => element,                       // TODO copy :94-97
  wireEvent: wireNativeEvent,
};

const fusionDriver: ComponentDriver = {
  resolveRoot: element => {
    // TODO (fixture: fusion_root_resolution) — ej2_instances[0] or throw readiness (component-runtime.ts:99-106)
    throw new Error("TODO");
  },
  wireEvent: wireFusionEvent,
};

registerComponentDriver("native", nativeDriver);
registerComponentDriver("fusion", fusionDriver);
```

### 6.5 `runtime/domain/runtime-plan.ts` (RuntimeComponents / RuntimeComponent — memoized)

```ts
const cache = new WeakMap<PlanDocument, RuntimePlan>();

export class RuntimePlan {
  readonly components: RuntimeComponents;
  // …objectContracts, plugins…
  static from(plan: PlanDocument): RuntimePlan {
    // TODO (fixture: invariant_runtime_object_memoized) — return cached or build+cache (runtime-plan.ts:57-64)
    throw new Error("TODO");
  }
  objectForSource(source: RuntimeObjectSource): RuntimeObject {
    // TODO (fixture: component_read_source) — switch on source.kind: "component" | "plugin"; assertNever
    throw new Error("TODO");
  }
}

export class RuntimeComponents {
  find(componentKey: string): RuntimeComponent | undefined { /* TODO :98-102 */ throw 0; }
  component(componentKey: string): RuntimeComponent {
    // TODO (fixture: component_not_active_boundary) — throw RuntimeResolutionError.componentNotActive if undefined (:109-114)
    throw new Error("TODO");
  }
  element(componentKey: string): HTMLElement { return this.component(componentKey).element(); }
}

export class RuntimeComponent {
  get id(): string { return this.definition.id; }
  element(): HTMLElement {
    // TODO (fixture: element_not_found_boundary) — getElementById or throw elementNotFound (:139-143)
    throw new Error("TODO");
  }
  root(): unknown {
    // TODO (fixture: fusion_root_resolution) — this.runtime().resolveRoot(this.element()) (:149-151)
    throw new Error("TODO");
  }
  object(): RuntimeObject {
    // TODO (fixture: component_property_set / component_read_source) — new RuntimeObject(label, root, contract) (:157-163)
    throw new Error("TODO");
  }
  runtime(): ComponentDriver {
    // TODO — requireComponentDriver(this.id, this.definition.vendor)  [was ComponentRuntime.for] (:165-167)
    throw new Error("TODO");
  }
}
```

---

## 7. Acceptance Fixtures (matrix cases this module must satisfy)

> Listed **by name** per the Coverage Completeness Gate. Each name maps to one
> matrix row in `04-matrix-validation-components-slots.md`; a fixture passes when
> the §6 body it is cited from produces exactly the row's plan-JSON + behavior.
> Component owns the **B** band rows and the structural half of the **E** band.

### Band B — Components (`04-matrix-…components-slots.md` §B1–B4)

| Fixture name | Matrix row | Proves |
|---|---|---|
| `inputfield_native_input` | B1 row 1 | `Html.InputField(...).NativeTextBox(...)` → `components[id]={role:"plan-input", vendor:"native", type:"textbox", binding:{kind:"registered-input", valueMember:"value", …}}` + contract under `types` |
| `inputfield_fusion_input` | B1 row 2 | same path, `vendor:"fusion"`, **same `IdGenerator` id for the same expression** (vendor-agnostic id) |
| `unregistered_render_developer_error` | B1 row 3 | `Html.InputField` with no component extension → render throws at the **authoring boundary**, no plan emitted (not a runtime fallback) |
| `input_subset_of_60_slices` | B1 parameterization | each input slice differs only by `vendor`/`type`/`valueMember`/`Shape`; display/container slices (no `ValueMember`) do **not** register as `plan-input` |
| `component_property_set` | B2 row 1 | `p.Component<FusionDropDownList>(m=>m.Country).SetValue("US")` → `object-target` + `set` node `{member:"value", value:{literal}, lane:"sync"}` |
| `component_method_call` | B2 row 2 | `…DataBind()` / `drawer.Open()` → `call` node `{method, args, lane:"sync"}` |
| `component_set_from_payload` | B2 row 3 | `SetDataSource(body, r=>r.Rows)` → `set` node `value:{kind:"read", source:{payload|response}}` — one Value spine |
| `component_read_source` | B2 row 4 | `ref.Read<T>()` → `TypedComponentSource<T>` → `{kind:"read", source:{component id}, member, shape}` consumed by condition/gather/plugin-arg |
| `component_event_to_reaction` | B3 row 1 | `.Reactive(e=>e.DataStateChange, …)` → `object-target` + `componentEvent` behavior; vendor seam only `ComponentDriver`+`wire*Event` |
| `input_component_event` | B3 row 2 | `.FusionDropDownList(b=>b.Reactive(e=>e.Change,…))` → existing `plan-input` gains an event behavior, role unchanged |
| `grid_render_non_input` | B4 row 1 | `Html.FusionGrid<…>` → grid HTML; no `plan-input` registration (no `ValueMember`); becomes `object-target` only when referenced |
| `grid_mutation_refresh` | B4 row 3 | `p.Component<FusionGrid>("id").Refresh()` → `call` node `{method:"refresh", lane:"sync"}` |
| `grid_no_new_plan_node` | B4 parameterization | the 50+ Fusion surface reuses `set`/`call`/`read`/`componentEvent` — proves no display component needs a new plan-node kind |

### Band E — App-level objects (structural half: fixed-id `layout-object`)

| Fixture name | Matrix row | Proves |
|---|---|---|
| `drawer_render_open_close_size` | E row 1 | `@Html.NativeDrawer()` + `p.Drawer().Open()` → fixed-id `layout-object` `{id:"alis-drawer"}` + `call` nodes; one shared id constant C# ↔ TS |
| `layout_object_persists_across_unload` | E note | `layout-object` stays mounted when a slot unloads (Slot owns unload; Component proves the role survives) |

### Module invariants (constructor + runtime)

| Fixture name | Source | Proves |
|---|---|---|
| `invariant_vendor_cannot_change` | `ComponentObjects.cs:143-149` | re-referencing a component with a different vendor throws (authoring boundary) |
| `invariant_first_registration_wins` | `ComponentObject.cs:73-74,130-157` | `WithBindingIfAbsent` keeps the existing binding |
| `invariant_idempotent_duplicate_registration` | `RegisteredInputComponents.cs:21-33` | same contract = no-op; conflicting contract throws |
| `invariant_all_join_keys_non_null` | `ComponentObject.cs:33-38` | id/vendor/type/role/binding/container guaranteed by ctor (null unrepresentable, not guarded later via `?`) |
| `invariant_runtime_object_memoized` | `runtime-plan.ts:13,57-64` | `RuntimePlan.from` caches per `PlanDocument`; a set-sequence resolves DOM+root once |
| `element_not_found_boundary` | `runtime-plan.ts:139-143` | missing DOM element throws `RuntimeResolutionError.elementNotFound` (real boundary) |
| `component_not_active_boundary` | `runtime-plan.ts:109-114` | a component absent from the active plan throws `RuntimeResolutionError.componentNotActive` |
| `fusion_root_resolution` | `component-runtime.ts:99-106` | fusion root = `ej2_instances[0]`, else `RuntimeComponentReadinessError` |
| `vendor_isolation_third_vendor` | `component-runtime.ts:88-92` + Rule 5 | adding a vendor touches only `event-{vendor}.ts` + one `registerComponentDriver`; double-register throws |

---

## 8. Pass Protocol Row (paste at the top of the implementing commit)

```text
Close matrix row: Html.InputField(plan, m=>m.Prop).NativeTextBox(...)
  -> BrowserObject(role=plan-input, vendor=native, binding=registered-input)
  -> RuntimeComponents.component(id).object() resolves via getElementById + ComponentDriver(native)
```

- **Sync/async lane:** all Component reads/sets/calls are **sync** (the lane is
  stamped by Reaction, not Component). Component opens no async boundary.
- **API surface:** unchanged — the only `public` types stay `IdGenerator`,
  `IComponent`/`IInputComponent`/`IAppLevelComponent`, `ComponentRole`,
  `InputBinding`, `ValidationContainerBinding`, `ComponentRegistration`,
  `ComponentRef<,>`. All plan-model ctors stay `internal`.
- **Generated TS:** the `plan.ts` Component section is emitted by Kind's
  `PlanContractGenerator`; when the C# node shape changes, regenerate and
  `npm run typecheck` — never hand-edit.
- **Commit boundary:** one fixture name per commit (e.g. close
  `inputfield_native_input` first, then `component_property_set`, …).
