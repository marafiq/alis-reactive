# Plugin — Implementation Spec (scaffold)

> Mechanical build spec for the **Plugin** micro-module. A developer opens this
> file, reads the surface + skeleton + fixtures, and types the obvious body.
> Every claim is grounded in actual source, cited inline. Names are from
> [`03-naming.md`](../03-naming.md); responsibility/ownership from
> [`02-micro-modules.md`](../02-micro-modules.md); acceptance fixtures from
> [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
> Band D (and the cross-references in `04-matrix-http-arrays-values.md` read rows
> + `05-determinism-proof.md` §10).
>
> Source read as the requirement (not inferred): `Alis.Reactive/ReactivePlugin.cs`,
> `Alis.Reactive/Builders/PluginTypeBuilder.cs`, `Alis.Reactive/Builders/PluginReadBuilder.cs`,
> `Alis.Reactive/Builders/PluginCallBuilder.cs`, `Alis.Reactive/Builders/PluginArguments.cs`,
> `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs`,
> `Alis.Reactive/Builders/PipelineBuilder.cs:160–258`, `Alis.Reactive/ReactivePlan.cs:53–79`,
> `Alis.Reactive/PlanModel/PluginContract.cs`, `Alis.Reactive/PlanModel/Source.cs:100–118`,
> `Alis.Reactive/PlanModel/ValueExpression.cs:102`, `Alis.Reactive/PlanModel/PlanTerms.cs:138`,
> `Alis.Reactive.Assets/runtime/core/plugin-catalog.ts`,
> `Alis.Reactive.Assets/runtime/domain/runtime-plan.ts:46–82,170–188`,
> `Alis.Reactive.Assets/runtime/execution/execute.ts:175–187`,
> `Alis.Reactive.Assets/runtime/types/plan.ts:360–398,536–541,747–763`.

---

## 1. Responsibility, Ownership, Dependencies

**Responsibility (one sentence).** The intentional typed escape hatch: declare a
browser object the DSL does not model — typed properties + functions + commands —
then read a property/function or call a command through the **same** `Value` and
object-member spines, with stringly names allowed **only** at the plugin-name /
member boundary.

**What Plugin owns** (from `02-micro-modules.md` Plugin row; names from
`03-naming.md` Plugin table):

| Side | Owns |
|---|---|
| `→` C# authoring | **ONE** plugin-declaration API — `Plugin` (folds today's `ReactivePlugin` typed-subclass + `PluginTypeBuilder` stringly-inline into one); **ONE** args-builder-first read/call surface — `PluginMemberBuilder` (folds the ~95%-identical `PluginReadBuilder`/`PluginCallBuilder` + the arity-0..3 × member/root × function/command overload explosion ~30 methods into one); the `Plugin → PluginContract → BrowserObjectContract` mapping. |
| `→` plan/contract model | `PluginContract` (+ its `PluginOperationContract`/`PluginPropertyContract`, `PluginOperationId`/`PluginPropertyId`, `ObjectMemberKey`) and `PluginSource` — the value-source node that says "read/call this named plugin object". |
| `⇒` TS runtime | `PluginCatalog` (the host-registered instance store; `register`/`resolve`/`clear`) — **a true lookup at a real external edge**: `resolve` throws when the host never registered the named plugin. |
| contract (generated, **not** hand-written) | the `PluginSource` interface in `types/plan.ts` (`{ kind:"plugin"; name; type }`) — emitted by **Kind**'s `PlanContractGenerator`. |

**What Plugin depends on** (module-dependency graph, acyclic — `00-design.md` §2):

- **Value** — every read lowers to a `ValueExpression` (`Invoke`/`Read`) and every
  arg is a `ValueExpression`; the runtime read goes through `evaluateValue`. Plugin
  adds **no** second value resolver. (`Plugin → Value`.)
- **Component** — a `PluginContract` *maps to* a `BrowserObjectContract` and the
  runtime joins it as an ordinary `RuntimeObject`; the catalog instance is the
  object's vendor-agnostic root. Plugin reads these as join keys; it does not own
  `BrowserObjectContract`. (`Plugin → Component`.)
- **Shape** — each property/function-return/arg carries a `Shape` from
  `Shape.FromClrType(typeof(T))`. (`Plugin → Shape`.)
- **Kind** — `PluginSource` carries `kind:"plugin"`; the TS interface is generated.
  (`Plugin → Kind`.)

**What Plugin does NOT own / must not invent.**

- No new reaction node: a plugin **call** is the existing `ReactionGraph.Call` node
  with a `PluginSource` target (`PluginCallBuilder.Fire` →
  `ReactionGraph.Call(PluginSource.Of(name), method, args)` — `execute.ts:178–187`
  routes `case "plugin"` through the same `objectForSource(...).call`). Plugin does
  **not** add a `kind:"plugin-call"` node.
- No new value node: a plugin **read** is the existing `read` `ValueExpression`
  (`ValueExpression.Invoke`/`Read` with a `PluginSource` `from`).
- **No `set` on a plugin.** `SetTargetSource = component | payload`
  (`plan.ts:371–373`); `CallTargetSource` includes `plugin` (`plan.ts:375–378`).
  There is no `Plugin(...).Set(...)` verb in source, so there is no `set × plugin`
  tuple to lower (`05-determinism-proof.md` §"Variants I Could NOT Make…" item 1).
  Do not add one.
- No runtime fallback. The **one** throw is `PluginCatalog.resolve` for an
  unregistered name — a genuine external boundary (the host owns the instance).

---

## 2. Public Surface

> "Public" = the surface authors call. Per Rule 8 the *plan-model* types
> (`PluginContract`, `PluginSource`, ids) are `internal` with factory entry points;
> the *authoring* types a developer writes (`Plugin` base, `PluginMemberBuilder`,
> `PluginFunction<T>`/`PluginProperty<T>`/`PluginCommand`, `p.Plugin*` on the
> pipeline, `RegisterPlugin` on the plan) are `public` because they are the frozen
> DSL. The TS `PluginCatalog` is `export`ed because the host registers instances
> across the runtime boundary.

### 2a. C# authoring — `Plugin` (the ONE declaration API)

Today there are two declaration APIs: the typed subclass `ReactivePlugin`
(`ReactivePlugin.cs:13`) and the stringly-inline `PluginTypeBuilder`
(`PluginTypeBuilder.cs:10`, reached via `plan.RegisterPlugin("name", b => …)`). The
redesign keeps **one**: the typed `Plugin` base (renamed from `ReactivePlugin` —
`03-naming.md`). Inline string declaration stays available only through
`RegisterPlugin("name", b => …)`, which configures the **same** members onto a
`Plugin`-shaped contract (no second member vocabulary).

```csharp
namespace Alis.Reactive;

/// <summary>
/// Declares a browser plugin: a named browser object the DSL does not model,
/// exposing typed readable properties, value-returning functions, and void
/// commands. A plugin is the intentional escape hatch — use it for URL/DOM APIs
/// or array work the deterministic DSL does not express. Subclass it, name the
/// plugin in the base constructor, and declare members in the constructor body.
/// </summary>
public abstract class Plugin     // was: ReactivePlugin
{
    protected Plugin(string name);                          // PluginName.Of(name) — empty/whitespace rejected at the boundary

    public string Name { get; }                             // the runtime plugin name

    // ── declare members (each records a Shape from T) ──
    protected PluginProperty<TValue> Property<TValue>(string member);
    protected PluginFunction<TReturn> Function<TReturn>();                 // root function, "$call"
    protected PluginFunction<TReturn> Function<TReturn>(string member);
    protected PluginFunction<TReturn> Function<TReturn>(Action<PluginArgumentTypes> args);
    protected PluginFunction<TReturn> Function<TReturn>(string member, Action<PluginArgumentTypes> args);
    protected PluginCommand Command();                                     // root command, "$call"
    protected PluginCommand Command(string member);
    protected PluginCommand Command(Action<PluginArgumentTypes> args);
    protected PluginCommand Command(string member, Action<PluginArgumentTypes> args);

    internal PluginContract ToContract();                   // PluginMemberDeclarations.ToContract(name)
}
```

> **Decision baked in:** `Function<T>` = "returns a value" (`Shape.FromClrType(T)`);
> `Command` = "returns nothing" (`Shape.None`); root member is `"root"`
> (`ObjectMemberKey.RootCall`, key `"$call"`). The matrix Band D good-default row.
>
> **Folded away:** the arity overloads `Function<TReturn, TArg1[, TArg2[, TArg3]]>`
> / `Command<TArg1…>` (`ReactivePlugin.cs:61–132`) and the
> `Void`/`Method`/`Function<…>` arity ladder on `PluginTypeBuilder`
> (`PluginTypeBuilder.cs:45–216`) collapse into the **args-builder** form:
> `Function<T>(a => a.Arg<string>().Arg<int>())`. One way to declare arity, not ~30.

The per-member descriptors a developer captures and later references stay typed:

```csharp
/// <summary>A readable plugin object property; its plan shape comes from <typeparamref name="TValue"/>.</summary>
public sealed class PluginProperty<TValue>          // unchanged shape
{
    public string PluginName { get; }
    public string Member { get; }
    internal Shape Shape { get; }
    internal PluginPropertyId PropertyId { get; }
    internal PluginPropertyContract ToContract();
}

/// <summary>A plugin function that returns a typed value. Chain <c>.Arg&lt;T&gt;()</c> / <c>.Args(...)</c> to set the argument contract.</summary>
public sealed class PluginFunction<TReturn> : PluginOperation
{
    public PluginFunction<TReturn> Arg<TArg>();
    public PluginFunction<TReturn> Args(Action<PluginArgumentTypes> arguments);
}

/// <summary>A plugin command that returns no value. Chain <c>.Arg&lt;T&gt;()</c> / <c>.Args(...)</c> to set the argument contract.</summary>
public sealed class PluginCommand : PluginOperation
{
    public PluginCommand Arg<TArg>();
    public PluginCommand Args(Action<PluginArgumentTypes> arguments);
}

/// <summary>Builds an exact plugin argument contract without an arity-specific overload.</summary>
public sealed class PluginArgumentTypes
{
    public PluginArgumentTypes Arg<T>();                 // _shapes.Add(Shape.FromClrType(typeof(T)))
}
```

### 2b. C# authoring — registration on the plan (`ReactivePlan.cs:53–79`, unchanged)

```csharp
/// <summary>Registers a plugin's typed member contract; call before any p.Plugin(...) reference.</summary>
public void RegisterPlugin(string pluginName, Action<PluginTypeBuilder> configure);   // stringly-inline form
public void RegisterPlugin(Plugin plugin);                                            // typed-subclass form (was ReactivePlugin)
public TPlugin RegisterPlugin<TPlugin>() where TPlugin : Plugin, new();               // create + register, returns the instance
```

### 2c. C# authoring — `PluginMemberBuilder` (the ONE read/call surface)

Today the read path (`PluginReadBuilder<TReturn, TModel>`) and the call path
(`PluginCallBuilder<TModel>`) are ~95% identical: same eight `Arg(...)` overloads,
same `PluginArguments` accumulation. They differ only in the **terminal**: read
implicit-converts to `TypedPluginSource<TReturn>`; call has a `.Fire()`. The
redesign keeps **one** builder with both terminals; the type parameter records
whether a return value exists.

The pipeline entry points (`PipelineBuilder.cs:160–258`) are unchanged in name and
overload set — they open a `PluginMemberBuilder`:

```csharp
// on PipelineBuilder<TModel> — reads (return a source) ──────────────────────
public PluginMemberBuilder<T, TModel> Plugin<T>(string pluginName, string member);   // function read by name
public PluginMemberBuilder<T, TModel> Plugin<T>(string pluginName);                  // root function read
public TypedPluginPropertySource<T>   PluginProperty<T>(string pluginName, string member);
public PluginMemberBuilder<T, TModel> Plugin<T>(PluginFunction<T> function);         // typed function read
public TypedPluginPropertySource<T>   Plugin<T>(PluginProperty<T> property);         // typed property read

// on PipelineBuilder<TModel> — calls (void command) ─────────────────────────
public PluginCallBuilder<TModel>      Plugin(string pluginName, string member);      // command by name
public PluginCallBuilder<TModel>      Plugin(string pluginName);                     // root command
public PluginCallBuilder<TModel>      Plugin(PluginCommand command);                 // typed command
```

> **Note on the builder unification.** `PluginMemberBuilder<TReturn, TModel>` is the
> single `Arg`-accumulating body; the *read* face exposes the implicit conversion
> and the *call* face exposes `.Fire()`. Whether you keep two thin public faces over
> one shared body or one builder with both terminals is an implementation detail —
> the **constraint the matrix fixes** is one args vocabulary and one `Value`-lowering
> path, not two near-duplicate classes. Either way the eight `Arg(...)` overloads
> below are declared **once**.

```csharp
/// <summary>
/// Collects typed arguments for a plugin read or call. Each Arg lowers to a
/// ValueExpression over the shared Value spine. A read implicitly converts to
/// TypedPluginSource&lt;TReturn&gt;; a call ends with Fire().
/// </summary>
public sealed class PluginMemberBuilder<TReturn, TModel> where TModel : class
{
    // response-body path arg (carries success/error scope)
    public PluginMemberBuilder<TReturn, TModel> Arg<TResponse, TProp>(
        ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path) where TResponse : class;
    // event-args path arg
    public PluginMemberBuilder<TReturn, TModel> Arg<TArgs, TProp>(
        TArgs args, Expression<Func<TArgs, TProp>> path);
    // any typed source arg (component read, URL param, another plugin read)
    public PluginMemberBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source);
    // scalar literals (shape inferred per overload)
    public PluginMemberBuilder<TReturn, TModel> Arg(string value);
    public PluginMemberBuilder<TReturn, TModel> Arg(int value);
    public PluginMemberBuilder<TReturn, TModel> Arg(bool value);
    public PluginMemberBuilder<TReturn, TModel> Arg(long value);
    public PluginMemberBuilder<TReturn, TModel> Arg(decimal value);
    public PluginMemberBuilder<TReturn, TModel> Arg(double value);
    public PluginMemberBuilder<TReturn, TModel> Arg(DateTime value);
    // literal whose shape is derived from TValue
    public PluginMemberBuilder<TReturn, TModel> ArgValue<TValue>(TValue value);

    // read terminal — no Build(): the source IS the builder
    public static implicit operator TypedPluginSource<TReturn>(PluginMemberBuilder<TReturn, TModel> b);
}

/// <summary>Collects typed arguments for a plugin command; Fire() emits the call. Same Arg surface as the read builder.</summary>
public sealed class PluginCallBuilder<TModel> where TModel : class
{
    // …the same eight Arg(...) overloads + ArgValue<TValue>…
    public void Fire();   // _emitter.AddStep(ReactionGraph.Call(PluginSource.Of(name), method, args))
}
```

### 2d. C# plan model — `PluginContract` and `PluginSource` (`internal`, unchanged shape)

```csharp
namespace Alis.Reactive.PlanModel;

/// <summary>The declared member contract for one plugin: properties + operations, mapped to a BrowserObjectContract.</summary>
internal sealed class PluginContract
{
    internal PluginName Name { get; }
    internal TypeKey TypeKey => TypeKey.Plugin(Name);                 // "plugin." + name (PlanTerms.cs:138)
    internal static PluginContract Create(PluginName name,
        IEnumerable<PluginPropertyContract> properties, IEnumerable<PluginOperationContract> operations);
    internal BrowserObjectContract ToBrowserObjectContract();        // Declare each property/operation onto a fresh contract
    // invariant: a member cannot be declared as both a property and a function (EnsureNoPropertyMethodCollision)
}

/// <summary>Reads/calls a value from a named, application-registered plugin object.</summary>
public sealed class PluginSource : RuntimeObjectSource
{
    public string Kind => "plugin";
    public string Name { get; }                                      // _name.Value
    public string Type { get; }                                      // TypeKey.Plugin(name).Value = "plugin.<name>"
    internal static PluginSource Of(string name);
}
```

### 2e. TS runtime — `PluginCatalog` (`core/plugin-catalog.ts`, the one host store)

Crosses the contract boundary; the host registers instances, the runtime resolves
them. This is the **one** legitimate registry-and-throw in the module: a real
external edge (the browser-provided implementation lives outside the plan).

```ts
type BrowserPluginFunction = (...args: unknown[]) => unknown;
export type BrowserPluginRoot = object | BrowserPluginFunction;

export class PluginCatalog {                  // class today named BrowserPluginCatalog — 03-naming keeps PluginCatalog
  /** Records a host-provided plugin instance under a name; rejects empty/whitespace names and non-object/function instances; rejects a duplicate name. */
  register(name: string, instance: unknown): void;
  /** Returns the registered instance; THROWS at this external boundary when the name was never registered. */
  resolve(name: string): BrowserPluginRoot;
  /** Drops all instances. */
  clear(): void;
}

export const browserPlugins: PluginCatalog;
export function registerPlugin(name: string, instance: unknown): void;   // host entry
export function resolvePlugin(name: string): BrowserPluginRoot;          // runtime entry
```

> The runtime *join* (`RuntimePlugins.object(name, type)` in
> `domain/runtime-plan.ts:170–188`) builds an ordinary `RuntimeObject` from
> `objectContracts.contract(type)` + `catalog.resolve(name)`; `objectForSource`
> routes `case "plugin"` to it (`runtime-plan.ts:70–77`). That join belongs to the
> **Component** module (`RuntimeObject`/`RuntimeComponents`); Plugin only owns the
> *catalog of instances* it feeds in. There is **no** plugin-specific read/call
> evaluator — `evaluateValue` (read) and `executeCall` (call) handle it through the
> same `RuntimeObject.read`/`.call` every component uses.

### 2f. TS contract counterpart (generated, do NOT hand-write)

```ts
export interface PluginSource { kind: "plugin"; name: string; type: string; }   // plan.ts:394–398
export type RuntimeObjectSource = ComponentSource | PluginSource;               // plan.ts:367–369
export type CallTargetSource   = ComponentSource | PayloadSource | PluginSource;// plan.ts:375–378 (plugin is call-able)
export type SetTargetSource    = ComponentSource | PayloadSource;               // plan.ts:371–373 (plugin is NOT set-able)
```

Plugin reads these; it never edits `plan.ts`.

---

## 3. Input → Output Contract

| Path | Input | Output | Invariants (by construction, not guarded) |
|---|---|---|---|
| **Declare** | a `Plugin` subclass (or `RegisterPlugin("name", b => …)`) declaring properties/functions/commands, each typed `TValue`/`TReturn` + arg `Shape`s | `PluginContract` → `BrowserObjectContract` registered into the plan `types["plugin.<name>"]` | Every member carries a `Shape` from `Shape.FromClrType` — never null (`Shape.None` is the command-return sentinel, not null). A member name is unique per plugin and **cannot** be both a property and a function (`PluginContract.EnsureNoPropertyMethodCollision` throws — an authoring boundary). Root member key = `"$call"`, label `"root"`. |
| **Read property** | `p.PluginProperty<T>(name, member)` / `p.Plugin<T>(property)` | `TypedPluginPropertySource<T>` → `ValueExpression.Read(PluginSource.Of(name), member, shape)` | `access:"property"`. Read is a `TypedSource<T>` — usable anywhere a source is (condition operand, gather, set value, another plugin's arg). Shape from `T`. |
| **Read function** | `p.Plugin<T>(name, member).Arg(...)` / `p.Plugin<T>(name)` (root) / `p.Plugin<T>(function)` | `PluginMemberBuilder<T,TModel>` → (implicit) `TypedPluginSource<T>` → `ValueExpression.Invoke(PluginSource.Of(name), method, returns, args)` | `access:"method"`. Each `Arg(...)` is accepted against the declared `MethodArgumentContract` at authoring time (`PluginArguments.Add` → `AcceptInvocationArgument`); `Complete()` checks arity (`AcceptInvocationComplete`). `args:[]` default. |
| **Call command** | `p.Plugin(name, member).Arg(...).Fire()` / `p.Plugin(name)` (root) / `p.Plugin(command)` | `ReactionGraph.Call(PluginSource.Of(name), method, args)` — `kind:"call"`, **SYNC** | One terminal `.Fire()`. No `set` on a plugin (no such target). A command's return shape is `Shape.None`. |
| **Arg from any value source** | `.Arg(component.Value())`, `.Arg(body, r=>r.Id)`, `.Arg(args, e=>e.Data)`, `.Arg("lit")`, `.ArgValue<T>(v)` | each → a `ValueExpression` in the node's `args[]` via `PluginInvocationArgument` (`FromSource`/`FromResponse`/`FromEvent`/`Literal`) | Args flow through the **one** `Value` spine — no plugin-specific resolver. `DateTime` literal uses `ValueExpression.Literal(dateTime)`; others `LiteralRaw(value, shape)`. |
| **Runtime read** | a `read` node with `from:{kind:"plugin",…}` | the property/function value | `evaluateValue` → `objectForSource(plugin)` → `RuntimePlugins.object(name, type)` = `RuntimeObject(contract, catalog.resolve(name))` → `.read`/`.call`. **Unknown plugin → `PluginCatalog.resolve` throws** (external boundary). |
| **Runtime call** | a `call` node with `on:{kind:"plugin",…}` | the command runs (void) | `executeCall` `case "plugin"` → same `objectForSource(...).call(method, args)` (`execute.ts:178–187`). **SYNC** unless the host function itself returns a Promise (the escape hatch's own concern). |

**Value-object / construction invariants (null is unrepresentable by construction,
not guarded by exceptions):**

- A `Shape` is **never null** on any plugin member or arg — `Shape.FromClrType`
  is total and `Shape.None` is the explicit "no value" sentinel for command returns.
  There is no nullable-`Shape` to defend; the constructor `?? throw` guards in
  `PluginOperation`/`PluginProperty<>` are **authoring-boundary** asserts (a
  framework bug if hit), not normal-execution null defense.
- A member name is modeled by the `PluginName` / `MemberName` / `ObjectMemberKey`
  value objects (each `Of(...)` rejects empty/whitespace at the boundary). The plan
  never carries a "maybe-named" member.
- Duplicate / contradictory declarations (same member as property *and* function,
  or twice with different contracts) are **authoring** errors surfaced by
  `PluginContract` / `Plugin*Contracts.From` throws — the developer's mistake, not
  a runtime-plan defense.
- The **single runtime throw** is `PluginCatalog.resolve` for an unregistered name.
  That is correct: the instance is host-provided, external to the framework plan —
  the real external edge the design permits.

---

## 4. File Layout

Plugin's authoring + plan-model files sit together under `Alis.Reactive/`; the one
runtime file is the catalog. (The runtime *join* and *read/call execution* are
owned by Component/Value/Reaction and are only touched at the noted call sites.)

| Layer | File | Action | Contents |
|---|---|---|---|
| C# authoring | `Alis.Reactive/Plugin.cs` | rename of `ReactivePlugin.cs` | `Plugin` base, `PluginOperation`, `PluginFunction<T>`, `PluginCommand`, `PluginProperty<T>`, `PluginMemberDeclarations` |
| C# authoring | `Alis.Reactive/Builders/PluginMemberBuilder.cs` | merge of `PluginReadBuilder.cs` + `PluginCallBuilder.cs` | `PluginMemberBuilder<TReturn,TModel>` (read terminal) + `PluginCallBuilder<TModel>` (`.Fire()`) over one `Arg` body |
| C# authoring | `Alis.Reactive/Builders/PluginTypeBuilder.cs` | trim | inline `RegisterPlugin("name", …)` form configuring the **same** members via args-builder (drop the arity ladder) |
| C# authoring | `Alis.Reactive/Builders/PluginArguments.cs` | kept | `PluginInvocationArgument` (`FromSource`/`FromResponse`/`FromEvent`/`Literal`) + `PluginArguments` (contract-checked accumulation) |
| C# authoring | `Alis.Reactive/Builders/Conditions/TypedPluginSource.cs` | kept | `TypedPluginSource<T>` (function → `Invoke`), `TypedPluginPropertySource<T>` (property → `Read`) |
| C# plan model | `Alis.Reactive/PlanModel/PluginContract.cs` | kept | `PluginContract`, `PluginOperation/PropertyContract(s)`, `PluginOperationId`/`PluginPropertyId`, `ObjectMemberKey`, requirements |
| C# plan model | `Alis.Reactive/PlanModel/Source.cs` | kept (`PluginSource`) | `PluginSource : RuntimeObjectSource` (`kind:"plugin"`, `name`, `type`) |
| TS runtime | `Alis.Reactive.Assets/runtime/core/plugin-catalog.ts` | kept (class renamed to `PluginCatalog`) | `PluginCatalog`, `browserPlugins`, `registerPlugin`/`resolvePlugin` |
| Contract (generated) | `Alis.Reactive.Assets/runtime/types/plan.ts` (`PluginSource`, ~394–398) | **not** hand-edited | `PluginSource` interface — emitted by Kind's `PlanContractGenerator` |
| C# tests | `tests/Alis.Reactive.UnitTests/Plugins/PluginTests.cs` | new | §6 A/B/C/D fixtures |
| TS tests | `Alis.Reactive.Assets/runtime/__tests__/plugin-catalog.test.ts` | kept/extend | §6 E fixtures (`register`/`resolve`/boundary throw) |
| Playwright | `tests/Alis.Reactive.PlaywrightTests/HttpPipeline/WhenArrayPluginManipulates.cs` | kept | browser proof: read feeds a gather/array op |

> Plugin introduces **no** new wire node and **no** edit to `plan.ts`,
> `execute.ts`, or `evaluate.ts` beyond the class-name rename in the catalog. The
> read/call already route through the shared `RuntimeObject`. If you find yourself
> adding a `kind:"plugin-call"` node or a plugin-only evaluator, stop — it belongs
> to Reaction (`call`) or Value (`read`).

---

## 5. Compile-Ready Skeleton

Bodies are `// TODO` referencing the §6 fixtures and the source the dev mirrors.

### `Plugin.cs` (the ONE declaration API — decision points only)

```csharp
namespace Alis.Reactive;

public abstract class Plugin
{
    private readonly PluginName _name;
    private readonly PluginMemberDeclarations _members = new PluginMemberDeclarations();

    protected Plugin(string name) => _name = PluginName.Of(name);     // boundary: empty/whitespace rejected by PluginName.Of
    public string Name => _name.Value;

    protected PluginProperty<TValue> Property<TValue>(string member)
    {
        // TODO: var p = new PluginProperty<TValue>(Name, member); _members.Add(p); return p;
        //   shape = Shape.FromClrType(typeof(TValue))  — fixture: declare_property_records_shape_from_T
    }

    protected PluginFunction<TReturn> Function<TReturn>()                                    // root
    {
        // TODO: new PluginFunction<TReturn>(Name) (root, returns Shape.FromClrType(T)); Add; return
        //   fixture: declare_root_function_member_is_root
    }
    protected PluginFunction<TReturn> Function<TReturn>(string member)
    {
        // TODO: new PluginFunction<TReturn>(Name, member); Add; return  — fixture: declare_function_returns_value_shape
    }
    protected PluginFunction<TReturn> Function<TReturn>(Action<PluginArgumentTypes> args)
        => Function<TReturn>().Args(args);
    protected PluginFunction<TReturn> Function<TReturn>(string member, Action<PluginArgumentTypes> args)
        => Function<TReturn>(member).Args(args);                                              // fixture: declare_function_args_via_builder

    protected PluginCommand Command()                                                        // root, Shape.None
    {
        // TODO: new PluginCommand(Name) (returns Shape.None); Add; return  — fixture: declare_command_returns_none_shape
    }
    protected PluginCommand Command(string member)
    {
        // TODO: new PluginCommand(Name, member); Add; return
    }
    protected PluginCommand Command(Action<PluginArgumentTypes> args)               => Command().Args(args);
    protected PluginCommand Command(string member, Action<PluginArgumentTypes> args) => Command(member).Args(args);

    internal PluginContract ToContract() => _members.ToContract(_name);             // fixture: to_contract_maps_to_browser_object_contract
}
```

> `PluginOperation` / `PluginFunction<T>` / `PluginCommand` / `PluginProperty<T>` /
> `PluginMemberDeclarations` / `PluginArgumentTypes` are **pure mechanism** — copy
> verbatim from `ReactivePlugin.cs:198–342` (drop the arity overloads §2a notes).
> Each records a `Shape.FromClrType` and `ToContract()`s into the plan; there is no
> decision point in them.

### `Builders/PluginMemberBuilder.cs` (the ONE Arg body)

```csharp
namespace Alis.Reactive.Builders;

public sealed class PluginMemberBuilder<TReturn, TModel> where TModel : class
{
    private readonly PluginOperationId _operation;
    private readonly PluginArguments _args;

    internal PluginMemberBuilder(PluginOperationId operation, MethodArgumentContract arguments)
    {
        _operation = operation;                       // ?? throw — authoring boundary
        _args = new PluginArguments(operation, arguments);
    }
    internal PluginMemberBuilder(PluginFunction<TReturn> function)
        : this(PluginOperationId.Of(function), function.ArgumentContract) { }

    // ── the eight Arg overloads + ArgValue — each: AddArg(PluginInvocationArgument.X(...)); return this ──
    public PluginMemberBuilder<TReturn, TModel> Arg<TResponse, TProp>(ResponseBody<TResponse> body, Expression<Func<TResponse, TProp>> path) where TResponse : class
    { /* TODO AddArg(FromResponse(body, path)) — fixture: arg_from_response_body */ }
    public PluginMemberBuilder<TReturn, TModel> Arg<TArgs, TProp>(TArgs args, Expression<Func<TArgs, TProp>> path)
    { /* TODO AddArg(FromEvent(path)) — fixture: arg_from_event_path */ }
    public PluginMemberBuilder<TReturn, TModel> Arg<TArg>(TypedSource<TArg> source)
    { /* TODO AddArg(FromSource(source)) — fixture: arg_from_typed_source */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(string value)   { /* TODO Literal(value) — fixture: arg_string_literal */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(int value)      { /* TODO Literal(value) */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(bool value)     { /* TODO Literal(value) */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(long value)     { /* TODO Literal(value) */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(decimal value)  { /* TODO Literal(value) */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(double value)   { /* TODO Literal(value) */ }
    public PluginMemberBuilder<TReturn, TModel> Arg(DateTime value) { /* TODO Literal(value) → ValueExpression.Literal(dateTime) */ }
    public PluginMemberBuilder<TReturn, TModel> ArgValue<TValue>(TValue value) { /* TODO Literal(value) — fixture: arg_value_shape_from_T */ }

    // read terminal — no Build()
    public static implicit operator TypedPluginSource<TReturn>(PluginMemberBuilder<TReturn, TModel> b) =>
        new TypedPluginSource<TReturn>(b._operation, b._args.Complete());      // fixture: read_function_lowers_to_invoke

    private void AddArg(PluginInvocationArgument argument) => _args.Add(argument);   // contract-checks arg shape + arity
}

public sealed class PluginCallBuilder<TModel> where TModel : class
{
    // …same eight Arg overloads + ArgValue, over the same PluginArguments…
    public void Fire()
    {
        // TODO: _emitter.AddStep(ReactionGraph.Call(
        //          PluginSource.Of(_operation.PluginNameValue),
        //          _operation.PlanMethodNameValue, _args.Complete()));
        // fixture: call_command_emits_call_reaction_sync
    }
}
```

### `core/plugin-catalog.ts` (rename class → `PluginCatalog`; bodies are mechanism)

```ts
// plugin-catalog.ts — host-provided plugin instance store. The plan declares the
// callable contract; this catalog owns only the browser implementations.
type BrowserPluginFunction = (...args: unknown[]) => unknown;
export type BrowserPluginRoot = object | BrowserPluginFunction;

export class PluginCatalog {
  private readonly plugins = new Map<string, BrowserPluginRoot>();

  register(name: string, instance: unknown): void {
    // TODO: assertPluginName(name); plugin = requireBrowserPlugin(name, instance);
    //   if (this.plugins.has(name)) throw `[alis] plugin "${name}" already registered`;
    //   this.plugins.set(name, plugin).  — fixtures: register_stores_instance, register_duplicate_throws
  }
  resolve(name: string): BrowserPluginRoot {
    // TODO: assertPluginName(name); const p = this.plugins.get(name);
    //   if (!p) throw `[alis] plugin not found: "${name}"`;  return p.
    //   — fixtures: resolve_returns_registered, resolve_unknown_throws_at_boundary
  }
  clear(): void { this.plugins.clear(); }
}

export const browserPlugins = new PluginCatalog();
export function registerPlugin(name: string, instance: unknown): void { browserPlugins.register(name, instance); }
export function resolvePlugin(name: string): BrowserPluginRoot { return browserPlugins.resolve(name); }
// assertPluginName / requireBrowserPlugin: pure boundary guards — copy verbatim from source.
```

---

## 6. Acceptance Fixtures (matrix cases this module satisfies)

From [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)
**Band D — Plugins** (4 deterministic variants, parameterized over
`pluginMembers × {read, call}`), cross-checked against the read rows in
`04-matrix-http-arrays-values.md:88–89,152` and `05-determinism-proof.md` §10. Each
Band D row becomes one named acceptance fixture; supporting fixtures prove the
redesign's stated Plugin fixes (one declaration API, one args path, boundary-only
throw, no `set`).

### A. Declare a plugin (Band D row 1)

| Matrix row | Fixture name | Asserts |
|---|---|---|
| **Declare a plugin** | `declare_property_records_shape_from_T` | `Property<string>("param")` records `Shape.String`. |
| **Declare a plugin** | `declare_function_returns_value_shape` | `Function<int>("compute")` records return `Shape.Number`. |
| **Declare a plugin** | `declare_command_returns_none_shape` | `Command("push")` records return `Shape.None`. |
| **Declare a plugin** | `declare_root_function_member_is_root` | `Function<T>()` / `Command()` member label is `"root"` (key `"$call"`). |
| **Declare a plugin** | `declare_function_args_via_builder` | `Function<int>(a => a.Arg<string>().Arg<int>())` records arg shapes `[string, number]` — the args-builder replaces the arity ladder. |
| **Declare a plugin** | `to_contract_maps_to_browser_object_contract` | `ToContract().ToBrowserObjectContract()` exposes the declared properties + methods; `types["plugin.<name>"]` carries it. |
| **Declare a plugin** | `declare_member_as_property_and_function_throws` | declaring the same member as both property and function throws (authoring boundary, `EnsureNoPropertyMethodCollision`). |
| **Declare a plugin** | `register_plugin_inline_matches_typed_subclass` | `RegisterPlugin("urlApi", b => …)` and a `class : Plugin` declaring the same members produce an **equal** `BrowserObjectContract` (one declaration vocabulary). |

### B. Read a plugin property/function (Band D row 2)

| Matrix row | Fixture name | Asserts |
|---|---|---|
| **Read property** | `read_property_lowers_to_read_node` | `p.PluginProperty<string>("urlApi","param")` → `{kind:"read", from:{kind:"plugin",name:"urlApi",type:"plugin.urlApi"}, member:"param", access:{kind:"property"}, shape:{kind:"string"}}`. |
| **Read function** | `read_function_lowers_to_invoke` | `p.Plugin<int>("urlApi","compute").Arg("a")` (implicit `TypedPluginSource<int>`) → `{kind:"read", from:{kind:"plugin",…}, member:"compute", access:{kind:"method"}, args:[…], shape:{kind:"number"}}`. |
| **Read function** | `read_root_function_member_is_root` | `p.Plugin<int>("urlApi")` reads the root function (`member`/path = root). |
| **Read property** | `read_property_usable_as_condition_source` | the property source feeds `p.When(plugin.Param).Eq("x")` as an ordinary `TypedSource` — read = a source like any other. |

### C. Call a plugin command (Band D row 3)

| Matrix row | Fixture name | Asserts |
|---|---|---|
| **Call command** | `call_command_emits_call_reaction_sync` | `p.Plugin("urlApi","push").Arg("/path").Fire()` → `ReactionGraph.Call` = `{kind:"call", on:{kind:"plugin",name:"urlApi",type:"plugin.urlApi"}, method:"push", args:[…]}`, lane **SYNC**. |
| **Call command** | `call_root_command_fires` | `p.Plugin("urlApi").Arg(…).Fire()` calls the root command. |
| **Call command** | `no_set_target_on_plugin` | there is **no** `Plugin(...).Set(...)` verb and `SetTargetSource` excludes `plugin` (compile-level proof; `plan.ts:371–373`) — the `set × plugin` tuple is unrepresentable, not a runtime guard. |

### D. Plugin arg from any value source (Band D row 4)

| Matrix row | Fixture name | Asserts |
|---|---|---|
| **Arg from source** | `arg_from_typed_source` | `.Arg(p.Component<…>(m=>m.X).Value())` → arg is the component-read `ValueExpression`. |
| **Arg from source** | `arg_from_response_body` | `.Arg(body, r=>r.Id)` → arg is a payload read in success/error scope (`PluginInvocationArgument.FromResponse`). |
| **Arg from source** | `arg_from_event_path` | `.Arg(args, e=>e.Data)` → arg is an event-payload read (`FromEvent`). |
| **Arg from source** | `arg_string_literal` / `arg_value_shape_from_T` | scalar `Arg("x")` / `ArgValue<decimal>(v)` lower to literals with the inferred shape — args flow the one `Value` path, no plugin-specific resolver. |
| **Arg from source** | `arg_arity_checked_against_contract` | adding an arg of the wrong count/shape against the declared `MethodArgumentContract` throws at authoring (`PluginArguments.Add`/`Complete` → `AcceptInvocation*`). |

### E. Runtime catalog (TS — `PluginCatalog`)

| Fixture name | Asserts |
|---|---|
| `register_stores_instance` | `register("urlApi", impl)` then `resolve("urlApi")` returns `impl`. |
| `register_rejects_empty_name` / `register_rejects_non_object` | boundary guards reject empty/whitespace names and non-object/function instances. |
| `register_duplicate_throws` | registering the same name twice throws. |
| `resolve_returns_registered` | a known name resolves to its instance, joined by `RuntimePlugins.object` into a `RuntimeObject`. |
| `resolve_unknown_throws_at_boundary` | an unregistered name throws `[alis] plugin not found` — the **one** legitimate runtime throw (external edge). |
| `runtime_read_invokes_catalog_instance` | a `read` node with `from:{kind:"plugin"}` evaluates via `objectForSource → RuntimePlugins.object → RuntimeObject.read/.call` (browser proof: the read feeds a gather/array op — `WhenArrayPluginManipulates`). |
| `runtime_call_invokes_catalog_instance` | a `call` node with `on:{kind:"plugin"}` runs the command via `executeCall case "plugin"` (`execute.ts:178–187`). |

### Coverage gate

Every Band D variant (declare, read, call, arg-from-source) maps to at least one
A/B/C/D fixture; the read node appears in both `access:"property"` (B) and
`access:"method"` (B) forms; the call node appears as `kind:"call"` with a
`PluginSource` target (C); the args path is proven across all five `Arg`
families (D); the catalog's only-legitimate-throw is proven (E
`resolve_unknown_throws_at_boundary`). The **no-`set`-on-plugin** non-variant is
proven by `no_set_target_on_plugin` (compile-level, not a runtime guard). A new
plugin member is not done until it appears in the `PluginContract`/`BrowserObjectContract`
(A), a read or call node (B/C), and — if it crosses the wire — the generated
`PluginSource` consumption is unchanged (no `plan.ts` edit). No Band D row is left
uncovered.
