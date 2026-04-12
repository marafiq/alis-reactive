# Final Reactive Schema — End-to-End Replacement

## Why

One reasoning model. The plan schema IS the domain model. Invalid plans are unrepresentable.

```
Frozen DSL (intent) → Domain Model (plan shape) → JSON → Dumb Runtime (execute)
```

## HARD RULE: Surgical Delete, Not Refactor

Every old concept is **deleted** — not renamed, not adapted, not wrapped. Every test testing old code is **deleted** — not fixed, not updated. The old schema is **replaced** — not versioned alongside.

- References a deleted term → delete it
- Tests internal construction of old plan classes → delete it
- Verifies old naming conventions → delete it
- Proves old merge logic → delete it
- Teaches old vocabulary → delete it

No trace. No backward compatibility. No "was previously X." VerifyJson snapshot tests are gone — domain model is the truth.

## Worktree

Create worktree `codex/schema-capability-fresh` from `origin/main` (remote main).

## The Schema

```ts
interface Plan {
  version: 3
  planId: string
  partId?: string
  types: Record<string, JsType>
  components: Record<string, Component>
  behaviors: Behavior[]
}

// ─── JsType ───

interface JsType {
  properties?: Record<string, Property>
  methods?: Record<string, Method>
  events?: Record<string, Event>
  defaultValue?: DefaultValue
}
interface Property { path: Path; shape: Shape; access: "read" | "write" | "readwrite" }
interface Method { path: Path; args?: Shape[]; returns?: Shape }
interface Event { channel: string; payloadType?: string }
interface DefaultValue { kind: "property" | "method"; member: string; shape: Shape }

// ─── Component ───

interface Component {
  id: string
  vendor: "native" | "fusion"
  type: string
  container?: ContainerScope
}
interface ContainerScope {
  components: string[]
  validationRules?: ComponentValidation[]
}
interface ComponentValidation { component: string; rules: ValidationRule[] }
interface ValidationRule {
  name: ValidationRuleName; message: string; constraint?: ValueProducer
  otherComponent?: string; when?: Condition; shape?: Shape
}
type ValidationRuleName =
  | "required" | "empty" | "minLength" | "maxLength"
  | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange" | "min" | "max" | "gt" | "lt"
  | "equalTo" | "notEqual" | "notEqualTo" | "atLeastOne"

// ─── Two Sources ───

type ComponentSource = { kind: "component"; component: string }
type PayloadSource = { kind: "payload"; scope: "event" | "success" | "error" | "request" | "dispatch" | "local"; type?: string }

// ─── Behavior ───

interface Behavior { startsWhen: StartsWhen; reaction: Reaction }
type StartsWhen =
  | { kind: "page-ready" }
  | { kind: "document-event"; event: string; payloadType?: string }
  | { kind: "component-event"; component: string; event: string }
  | { kind: "server-push"; url: string; event?: string; payloadType?: string }
  | { kind: "signalr"; hubUrl: string; method: string; payloadType?: string }

// ─── Reaction ───

type Reaction =
  | { kind: "sequence"; steps: Reaction[] }
  | { kind: "parallel"; steps: Reaction[]; onSettled?: Reaction }
  | { kind: "branch"; cases: BranchCase[] }
  | { kind: "set"; on: ComponentSource | PayloadSource; property: string; value: ValueProducer }
  | { kind: "call"; on: ComponentSource | PayloadSource; method: string; args?: ValueProducer[] }
  | { kind: "request"; request: Request }
  | { kind: "dispatch"; event: string; data?: ValueProducer; payloadType?: string }
  | { kind: "inject"; component: string; value: ValueProducer }
  | { kind: "show-validation-errors"; container: string }
interface BranchCase { when?: Condition; reaction: Reaction }

// ─── Request ───

interface Request {
  method: "GET" | "POST" | "PUT" | "DELETE" | "PATCH"
  url: string
  container?: string
  input?: RequestInput
  before?: Reaction[]              // WhileLoading: show spinner, disable buttons
  success?: ResponseHandler[]
  error?: ResponseHandler[]
  complete?: Reaction[]            // Always runs (revert WhileLoading state)
  next?: Request
}
type RequestInput =
  | { kind: "gather"; components: GatherField[]; transport: "query" | "json" | "form-data" }
  | { kind: "value"; value: ValueProducer; transport: "query" | "json" | "form-data" }
interface GatherField { component: string; key: string }
interface ResponseHandler { status?: number; reaction: Reaction }

// ─── ValueProducer (no unknowns — Shape types everything) ───

type ValueProducer =
  | { kind: "literal"; value: string | number | boolean | null; shape?: Shape }
  | { kind: "read"; from: ComponentSource | PayloadSource; member: string; path?: Path; shape?: Shape }
  | { kind: "object"; fields: Record<string, ValueProducer>; shape?: Shape }
  | { kind: "array"; items: ValueProducer[]; shape?: Shape }

// ─── Condition ───

type Condition =
  | { kind: "compare"; left: ValueProducer; op: CompareOp; right?: ValueProducer; shape?: Shape; itemShape?: Shape }
  | { kind: "all"; terms: Condition[] }
  | { kind: "any"; terms: Condition[] }
  | { kind: "not"; term: Condition }
  | { kind: "confirm"; message: string }
type CompareOp =
  | "eq" | "neq" | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy" | "is-null" | "not-null" | "is-empty" | "not-empty"
  | "in" | "not-in" | "between" | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length"

// ─── Shape (declaration + conversion: Shape → Shape) ───

type Shape =
  | { kind: "string" } | { kind: "number" } | { kind: "boolean" }
  | { kind: "date" } | { kind: "raw" }
  | { kind: "array"; item: Shape }
  | { kind: "object"; fields?: Record<string, Shape>; additional?: boolean }
  | { kind: "nullable"; inner: Shape }    // decimal?, DateTime?, etc.
  | { kind: "any" }

// ─── Path ───

type Path = Array<{ kind: "property"; name: string } | { kind: "index"; index: number }>
```

Shape serves two roles:
- **Declaration**: `Property.shape: date` → this IS a date
- **Conversion**: `read(..., shape: date)` → give me this AS a date (Shape → Shape)

## Two Separate API Surfaces

### Frozen DSL (user-facing, does NOT change)

This is what developers write in views. It expresses **intent**. It has no knowledge of the domain model:

```csharp
// Developer writes this — frozen surface
var plan = Html.ReactivePlan<ResidentModel>();

Html.InputFor(m => m.FirstName).FusionTextBox();
Html.InputFor(m => m.CareLevel).FusionDropDownList(items);

Html.On(plan, t => {
    t.DomReady(p => {
        p.Element("badge").Hide();
    });
    t.CustomEvent("care-level-changed", p => {
        p.Post("/api/residents/save")
         .Gather().IncludeAll()
         .OnSuccess(s => s.Element("status").SetText("Saved"));
    });
});

Html.RenderPlan(plan);
```

The DSL is FROZEN. Users never see plan internals.

### Domain Model (internal, this IS the redesign)

The domain model is what the DSL calls into. It is a proper DDD model — not serialization DTOs.

#### Design Rules

- **No magic strings** — IDs from property expressions, vendors from enums, ops from enums
- **No public setters** — set at construction, immutable after
- **Value objects** — Shape, Path, Source, GatherField: immutable, equality by value
- **Factory methods on abstract base** — `Reaction.Set(...)`, `Condition.Compare(...)`, `Shape.String()` — matches BCL convention (`TimeSpan.FromSeconds`)
- **Abstract class + sealed subtypes** — C# has no `abstract sealed`. Base class with `private protected` constructor prevents external subclassing. Sealed subtypes.
- **Plan is sole aggregate root** — JsType, Component, Behavior are entities within Plan. ContainerScope is composition on Component. Behavior owns StartsWhen + Reaction (composition).
- **Association by key** — Component references JsType by key (string). Payload types also live in types collection without a Component. The key lookup is validated at serialization boundary, not during incremental construction (needed for partial merging).
- **Mutable during construction, validated at boundary** — `With*` methods return `this`. `Plan.Validate()` checks referential integrity before serialization. PlanAuthoringContext is the factory/orchestrator.
- **Subtype naming**: `KindBase` — `CompareCondition`, `SequenceReaction`, `SetReaction`, `ComponentEventTrigger`

#### Enums (not strings)

```csharp
internal enum Vendor { Native, Fusion }
internal enum Access { Read, Write, ReadWrite }
internal enum Transport { Query, Json, FormData }
internal enum HttpMethod { Get, Post, Put, Delete, Patch }
internal enum PayloadScope { Event, Success, Error, Request, Dispatch, Local }
internal enum CompareOp
{
    Eq, Neq, Gt, Gte, Lt, Lte,
    Truthy, Falsy, IsNull, NotNull, IsEmpty, NotEmpty,
    In, NotIn, Between, ArrayContains,
    Contains, StartsWith, EndsWith, Matches, MinLength
}
```

Serialization: enums use a `ToSchemaString()` extension (e.g., `CompareOp.IsNull` → `"is-null"`) because no built-in naming policy handles kebab-case. Domain never touches the string.

#### Value Objects (immutable, equality by value)

```csharp
// Shape — single sealed class, not a hierarchy. All 9 variants share same 4 optional fields.
// Scalar shapes are cached singletons → reference equality.
// Structural equality via IEquatable<Shape>.
// No WriteOnlyPolymorphicConverter needed — serializes directly.
internal sealed class Shape : IEquatable<Shape>
{
    // Cached singletons — Shape.String == Shape.String is reference equality
    internal static readonly Shape String = new("string");
    internal static readonly Shape Number = new("number");
    internal static readonly Shape Boolean = new("boolean");
    internal static readonly Shape Date = new("date");
    internal static readonly Shape Raw = new("raw");
    internal static readonly Shape Any = new("any");

    // Structural factories — require valid children
    internal static Shape ArrayOf(Shape item)               // { kind: "array", item }
    internal static Shape ObjectOf(Dictionary<string, Shape> fields)  // { kind: "object", fields }
    internal static Shape OpenObject()                       // { kind: "object", additional: true }
    internal static Shape Nullable(Shape inner)              // { kind: "nullable", inner }

    // Properties — readonly, set at construction
    // Kind ordering: WriteOnlyPolymorphicConverter writes "kind" first automatically.
    // No [JsonPropertyOrder] attributes scattered across classes.
    public string Kind { get; }

    // None pattern — no C# null, no JsonIgnore attributes
    public Shape? Item { get; }         // array

    // None pattern — no C# null, no JsonIgnore attributes
    public Shape? Inner { get; }        // nullable

    // None pattern — no C# null, no JsonIgnore attributes
    public Dictionary<string, Shape>? Fields { get; }  // object

    // None pattern — no C# null, no JsonIgnore attributes
    public bool? Additional { get; }    // object

    // None — explicit absence, not null. Lends well in TS: shape.kind !== "none"
    internal static readonly Shape None = new("none");

    private Shape(string kind) { Kind = kind; }

    // Equality: kind + structural comparison of Item/Inner/Fields/Additional
    public bool Equals(Shape? other) { /* structural */ }
    public override int GetHashCode() { /* structural */ }
}

// C# code never uses null for Shape — always Shape.None
// TS code checks shape.kind !== "none" — never undefined
// JSON carries { "kind": "none" } for absent shapes — explicit, not omitted
```

#### None Pattern (no C# nulls, no JsonIgnore attributes)

Every domain type that can be "absent" has an explicit `.None` static instance. No C# null. No `[JsonIgnore(WhenWritingNull)]`. The converter skips None values during serialization.

```csharp
Shape.None              // No shape information (vs Shape.Any = "any shape is fine")
DefaultValue.None       // Not an input component
ContainerScope.None     // Not a container
Path.None               // No path navigation needed

// C# properties are NEVER null — always a value or None:
public Shape Shape { get; }              // Shape.String or Shape.None, never null
public ContainerScope Container { get; } // ContainerScope.Of(...) or ContainerScope.None, never null
public DefaultValue DefaultValue { get; } // DefaultValue.Property(...) or DefaultValue.None, never null

// WriteOnlyPolymorphicConverter checks IsNone and skips the property.
// No [JsonIgnore] attributes anywhere in the domain model.
```

Benefits:
- C#: no null checks, no `?.` chains, no NRE risk. Every property always has a value.
- TS: discriminated union covers all cases. `shape.kind !== "none"` is explicit.
- JSON: None values are omitted by the converter — clean output.
- No scattered `[JsonIgnore]` attributes — the None pattern handles absence uniformly.

// Path — built from property expressions, never from raw strings
// The DSL layer calls Path.From(Expression<Func<T, TProp>>)
internal sealed class Path : IEquatable<Path>
{
    internal static Path Property(string name) => new([new PropertySegment(name)]);
    internal Path Then(string name)             // chain: path.Then("value").Then("text")
    internal Path AtIndex(int index)            // array access

    internal IReadOnlyList<PathSegment> Segments { get; }
}

// Source — two kinds, sealed hierarchy
internal abstract sealed class Source
internal sealed class ComponentSource : Source
{
    internal string Component { get; }          // from property expression, not magic string
    internal static ComponentSource Of(string component)
}
internal sealed class PayloadSource : Source
{
    internal PayloadScope Scope { get; }
    internal string? Type { get; }              // references JsType key
    internal static PayloadSource Event(string? type = null)
    internal static PayloadSource Success(string? type = null)
    internal static PayloadSource Error(string? type = null)
    internal static PayloadSource Request(string? type = null)
    internal static PayloadSource Local()
}

// GatherField — what to read and where to put it in the payload
internal sealed class GatherField : IEquatable<GatherField>
{
    internal string Component { get; }          // which component to read (from expression)
    internal string Key { get; }                // payload key (from expression, e.g., "Address.Street")
    internal static GatherField Of(string component, string key)
}
```

#### Aggregates

```csharp
// JsType — aggregate, owns its members. Builder pattern for construction.
internal sealed class JsType
{
    internal static JsType Create() => new();

    internal JsType WithProperty(string name, Path path, Shape shape, Access access)
    internal JsType WithMethod(string name, Path path, Shape[]? args = null, Shape? returns = null)
    internal JsType WithEvent(string name, string channel, string? payloadType = null)
    internal JsType WithDefaultValue(string member, Shape shape)

    // Read-only access to members after construction
    internal IReadOnlyDictionary<string, Property> Properties { get; }
    internal IReadOnlyDictionary<string, Method> Methods { get; }
    internal IReadOnlyDictionary<string, Event> Events { get; }
    internal DefaultValue? DefaultValue { get; }
}

// Component — entity with deterministic identity from property expression
// Type is an association by key — validated at Plan.Validate(), not at construction
// (needed because partials may add types and components in any order)
internal sealed class Component
{
    internal static Component Create(string id, Vendor vendor, string type)
    internal Component WithContainer(ContainerScope container)

    public string Id { get; }                   // deterministic from expression
    public Vendor Vendor { get; }               // serializes as "native" | "fusion"
    public string Type { get; }                 // references key in Plan.Types (association)
    // None pattern — no C# null, no JsonIgnore attributes
    public ContainerScope? Container { get; }
}

// Behavior — aggregate, owns trigger + reaction. Cannot exist without both.
internal sealed class Behavior
{
    internal static Behavior On(StartsWhen trigger, Reaction reaction)

    internal StartsWhen Trigger { get; }
    internal Reaction Reaction { get; }
}
```

#### Sealed Hierarchies (discriminated unions)

```csharp
// StartsWhen — what triggers a behavior
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<StartsWhen>))]
internal abstract sealed class StartsWhen
{
    internal static StartsWhen PageReady()
    internal static StartsWhen DocumentEvent(string eventName, string? payloadType = null)
    internal static StartsWhen ComponentEvent(string component, string eventName)
    internal static StartsWhen ServerPush(string url, string? eventName = null, string? payloadType = null)
    internal static StartsWhen SignalR(string hubUrl, string method, string? payloadType = null)
}

// Reaction — the executable tree
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<Reaction>))]
internal abstract sealed class Reaction
{
    internal static Reaction Sequence(params Reaction[] steps)
    internal static Reaction Parallel(Reaction[] steps, Reaction? onSettled = null)
    internal static Reaction Branch(params BranchCase[] cases)
    internal static Reaction Set(Source on, string property, ValueProducer value)
    internal static Reaction Call(Source on, string method, params ValueProducer[] args)
    internal static Reaction Request(Request request)
    internal static Reaction Dispatch(string eventName, ValueProducer? data = null, string? payloadType = null)
    internal static Reaction Inject(string component, ValueProducer value)
    internal static Reaction ShowValidationErrors(string container)
}

// ValueProducer — four kinds only, no unknowns
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueProducer>))]
internal abstract sealed class ValueProducer
{
    internal static ValueProducer Literal(bool value)      // typed overloads
    internal static ValueProducer Literal(string value)
    internal static ValueProducer Literal(int value)
    internal static ValueProducer Literal(decimal value)
    internal static ValueProducer Literal(DateTime value)
    internal static ValueProducer Null()

    internal static ValueProducer Read(Source from, string member, Path? path = null, Shape? shape = null)
    internal static ValueProducer Object(IReadOnlyDictionary<string, ValueProducer> fields, Shape? shape = null)
    internal static ValueProducer Array(IReadOnlyList<ValueProducer> items, Shape? shape = null)
}

// Condition — guards and predicates
[JsonConverter(typeof(WriteOnlyPolymorphicConverter<Condition>))]
internal abstract sealed class Condition
{
    internal static Condition Compare(ValueProducer left, CompareOp op, ValueProducer? right = null, Shape? shape = null, Shape? itemShape = null)
    internal static Condition All(params Condition[] terms)
    internal static Condition Any(params Condition[] terms)
    internal static Condition Not(Condition term)
    internal static Condition Confirm(string message)
}
```

#### Plan — Aggregate Root

```csharp
internal sealed class Plan
{
    internal static Plan Create(string planId, string? partId = null)

    internal Plan WithType(string key, JsType type)
    internal Plan WithComponent(string key, Component component)
    internal Plan WithBehavior(Behavior behavior)

    internal string PlanId { get; }
    internal string? PartId { get; }
    internal IReadOnlyDictionary<string, JsType> Types { get; }
    internal IReadOnlyDictionary<string, Component> Components { get; }
    internal IReadOnlyList<Behavior> Behaviors { get; }
}

// Serialization is direct projection — no adapters
// JsonSerializer.Serialize(plan) produces valid schema-conforming JSON
```

#### How the DSL Translates Intent → Domain Model (with Shape everywhere)

```csharp
// ─── Element visibility ───
// Developer writes:
p.Element("badge").Hide();

// Domain model (Shape declares the property type):
Reaction.Set(
    ComponentSource.Of("badge"),
    "hidden",                           // property name on JsType
    ValueProducer.Literal(true))        // Shape.Boolean — inferred from typed overload

// ─── Reading with Shape (date comparison) ───
// Developer writes:
p.When(m => m.AdmissionDate).IsNotNull().Then(...)

// Domain model:
Condition.Compare(
    left: ValueProducer.Read(
        ComponentSource.Of("MyApp__AdmissionDate"),
        member: "value",
        shape: Shape.Nullable(Shape.Date)),  // Shape → Shape: read as nullable date
    op: CompareOp.NotNull)

// ─── Source vs Source (Shape enables correct comparison) ───
// Developer writes:
p.When(m => m.EndDate).GreaterThan(m => m.StartDate).Then(...)

// Domain model:
Condition.Compare(
    left: ValueProducer.Read(ComponentSource.Of("EndDate"), "value", shape: Shape.Date),
    op: CompareOp.Gt,
    right: ValueProducer.Read(ComponentSource.Of("StartDate"), "value", shape: Shape.Date),
    shape: Shape.Date)                  // compare AS dates, not as strings

// ─── Gather with Shape on every component ───
// Developer writes:
p.Post("/api/save").Gather().IncludeAll()

// Domain model — each GatherField's component carries Shape via DefaultValue:
Reaction.Request(Request.Post("/api/save")
    .WithGather(
        GatherField.Of("MyApp__FirstName", "FirstName"),   // DefaultValue.shape = Shape.String
        GatherField.Of("MyApp__BirthDate", "BirthDate"),   // DefaultValue.shape = Shape.Nullable(Shape.Date)
        GatherField.Of("MyApp__CareLevel", "CareLevel"))   // DefaultValue.shape = Shape.String
    .WithTransport(Transport.Json))
// Runtime reads each component's DefaultValue, applies Shape for serialization

// ─── Reading component's data source (array of objects) ───
// Developer writes:
args.UpdateData(p, json, j => j.Items)

// Domain model:
Reaction.Call(
    ComponentSource.Of("MyApp__Grid"),
    "updateData",
    ValueProducer.Read(
        PayloadSource.Success(),
        member: "Items",
        shape: Shape.ArrayOf(Shape.ObjectOf(...)))) // Shape tells runtime: array of objects

// ─── Dispatch with typed payload ───
// Developer writes:
p.Dispatch<CareChangePayload>("care-changed", payload)

// Domain model:
Reaction.Dispatch(
    "care-changed",
    data: ValueProducer.Object(new Dictionary<string, ValueProducer> {
        ["level"] = ValueProducer.Read(ComponentSource.Of("CareLevel"), "value", shape: Shape.String),
        ["rate"] = ValueProducer.Read(ComponentSource.Of("Rate"), "value", shape: Shape.Number)
    }),
    payloadType: "CareChangePayload") // receiver uses this to resolve PayloadSource

// ─── Validation with Shape (already fully supported — preserving feature parity) ───
// FluentValidator extraction already supports conditional rules end-to-end.
// Shape replaces the old coerceAs string — proper type contract.

// Unconditional rule:
// RuleFor(m => m.BirthDate).NotNull().LessThan(DateTime.Today)
ValidationRule { name: "required", message: "Birth date required", shape: Shape.Nullable(Shape.Date) }
ValidationRule { name: "lt", message: "Must be in past", constraint: ValueProducer.Literal(today), shape: Shape.Date }

// Conditional rule (already supported, preserved):
// RuleFor(m => m.Email).NotEmpty().When(m => m.ContactMethod == "email")
ValidationRule {
    name: "required", message: "Email required",
    when: Condition.Compare(
        ValueProducer.Read(ComponentSource.Of("ContactMethod"), "value", shape: Shape.String),
        CompareOp.Eq,
        ValueProducer.Literal("email")),
    shape: Shape.String }

// Cross-field rule (already supported, preserved):
// RuleFor(m => m.EndDate).GreaterThan(m => m.StartDate)
ValidationRule {
    name: "gt", message: "End date must be after start",
    otherComponent: "StartDate",
    shape: Shape.Date }
```

The DSL is the human-friendly surface. The domain model is the machine that builds the plan shape. No magic strings cross the boundary — property expressions resolve to typed IDs.

#### DSL Surface Verification (70+ methods traced)

Traced every DSL entry point. Result: **all frozen, zero blocked**.

| Category | Methods | Status |
|---|---|---|
| Html helpers (ReactivePlan, On, RenderPlan, InputField) | 5 | Frozen, map 1:1 |
| Trigger builder (DomReady, CustomEvent, ServerPush, SignalR) | 8 | Frozen, map 1:1 |
| Pipeline builder (Element, Component, Dispatch, Parallel, When) | 7 | Frozen, implementation simplifies |
| Element builder (class ops, text/html, visibility) | 10 | Frozen, map 1:1 |
| ComponentRef (Set, Call) | 5 | Frozen, internal simplifies |
| HTTP pipeline (verbs, Gather, Validate, Response) | 14 | Frozen, implementation simplifies |
| Conditions (21 ops, And/Or/Not, Confirm) | 25+ | Frozen, map 1:1 |
| Value expressions (Event, Component, Response) | 4 | Frozen, Source-based internally |
| .Reactive() extensions (per component) | 20+ | Frozen, implementation simplifies |

`PlanAuthoringContext` shrinks from 850 lines to ~200. ValueProducer kinds drop from 8 to 4. Triple indirection eliminated.

## Core Principle: Everything Is a Component

Once any module resolves an object, it gets a JS object with properties, methods, events. The interaction is ALWAYS the same — Read a property, Set a property, Call a method. Input components additionally carry DefaultValue.

Every module uses the same resolution → interaction path:
- **Conditions** — resolve source (component or payload), read property, compare via Shape
- **Confirm** — resolve component, call method
- **HTTP** — gather from components (read defaultValue), send, route response as payload
- **Events** — dispatch creates payload, listeners receive PayloadSource
- **Payload access** — resolve PayloadSource, read property via JsType paths
- **Source vs Source** — both resolve to JS objects, both use Read/Set/Call

No module has special resolution logic. No gimmicks. No hacks.

## Plan Merge AND Removal

Both merge and removal stay and work better:
- **Merge**: partial plans merge `types` (by key), `components` (by key), `behaviors` (concatenate). Simpler than merging contracts/objects/bindings (triple indirection gone).
- **Removal**: when a partial unloads, its `partId` identifies which types/components/behaviors to remove. Clean separation.
- Container's component list is updated at merge time when partials add components.
- Runtime boot wires all merged behaviors — same as today, cleaner data.

## Validation Enrichment

FluentValidator extracts rules using property expressions — same expressions as `Html.Input*`. The validator doesn't know the vendor, but within a plan, component IDs are always unique and the property expression in `Html.Input*` matches the validation rule's property expression. So enrichment at plan level matches rules to components by expression.

Shape replaces the old coerceAs cruft in validation rules — `Shape.Date()` for date comparisons, `Shape.Number()` for range checks, `Shape.Nullable(Shape.Date())` for `DateTime?`. The runtime gets proper type information, not string hints.

## Capabilities Anywhere — Uniform Behavior

Since every component resolves the same way (JS API surface: properties, methods, events), every module can hook any capability at any stage:

- `.Reactive()` on a component event → resolve component, read/set/call
- `Html.On` with Dispatch → same resolution, same interaction
- Conditions comparing source vs source → both resolve the same way
- HTTP gather/validation → resolve components, read defaultValues
- Event handlers → PayloadSource resolves the same way as ComponentSource

The old struggle of wiring capabilities at specific stages is eliminated. Mix & match of all modules works uniformly because the resolution and interaction model is shared.

## Event Payload Resolution

For native DOM events (like `change`), the raw event object IS the payload. The plan carries a payloadType on the Event declaration. That payloadType references a JsType whose properties describe the extraction paths:

```ts
// JsType for a native change event payload:
types: {
  "native.change.payload": {
    properties: {
      "value": { path: [{ kind: "property", name: "currentTarget" }, { kind: "property", name: "value" }], shape: { kind: "string" }, access: "read" }
    }
  }
}

// Event declaration on the component's JsType:
events: {
  "change": { channel: "change", payloadType: "native.change.payload" }
}

// In a reaction, reading the event payload:
{ kind: "read", from: { kind: "payload", scope: "event", type: "native.change.payload" }, member: "value" }
```

The JsType's property paths absorb vendor-specific extraction. No separate `data` mapping. The type system IS the normalization layer.

## Server Error Routing and Live-Clear

Component ID is deterministic from the property expression (`fullnamespace__fullpropexpr`). The C# DSL generates the same ID that ASP.NET model validation uses. So:

- **Server returns** `{ errors: { "FirstName": ["Required"] } }` 
- **Runtime** looks up component by matching the error key to GatherField.key (which maps component → payload key)
- **Shows error** on the component's element (Component.id → DOM element)

Live-clear: when user types in a component, runtime clears its error using Component.id directly. No binding indirection.

Both paths are one hop: error key → GatherField → component → element. Not three hops through bindings.

## REPEATED RULE: Old concept encountered → DELETE, not patch

If at ANY point during implementation you encounter an old concept, name, or filename:
- **DELETE IT.** Do not patch it. Do not rename it. Do not adapt it.
- Old class name? Delete the class. Write the new one from scratch.
- Old file name? Delete the file. Create a new file with the correct name.
- Old term in a comment? Delete the comment.
- Old term in a test? Delete the test. Write a new one that tests the domain model.
- Old term in a doc? Delete the section. Write it fresh.

This rule applies at EVERY step. Not just Step 1. Not just the schema. EVERY file touched.

## End-to-End Implementation

### Step 1: Schema + Design Spec (this session)

**Files:**
- Replace `Alis.Reactive/Schemas/reactive-plan.schema.json` — JSON Schema 2020-12, version 3
- Write `docs/superpowers/specs/2026-04-04-final-reactive-schema-design.md`

### Step 2: C# Domain Model

**File renames (old names carry old concepts — not tolerated):**
- `ReactivePlanV2.cs` → `PlanModel.cs` (the V2 name is gone)
- `PlanAuthoringContext.cs` → stays (name is fine, content rewrites)

**Files rewritten in-place:**
- `Alis.Reactive/PlanModel/PlanModel.cs` → the domain model
  - DELETED classes: FieldBinding, BindingValueExpr, BindingMapValueExpr, ConvertValueExpr, ActionTarget, EventObjectReference, ScalarValueShape, ArrayValueShape, ObjectValueShape, AnyValueShape, Workflow, PlanSubscription (all subtypes), PlanAction (all subtypes), PlanPredicate (all subtypes), BranchCase
  - NEW classes: JsType, Component, Behavior, StartsWhen (subtypes), Reaction (subtypes), ValueProducer (subtypes), Condition (subtypes), Shape (single class), ComponentSource, PayloadSource, GatherField, ContainerScope, DefaultValue
  - All sealed, internal constructors, factory methods, enums for all string values

- `Alis.Reactive/PlanModel/PlanAuthoringContext.cs` → simplify dramatically
  - Delete: ShapeFromCoerce(), ResolverForVendor(), BuildMemberName()
  - Delete: ComponentObjectName(), ElementObjectName(), ContractKey() — synthetic naming
  - Delete: MergeContracts(), PromoteObjectContract(), IsContractReferenced()
  - Delete: All EnsureContract/EnsureObject/EnsureElementObject triple-indirection
  - Replace with: EnsureComponent (single method), types are per-instance

- `Alis.Reactive/ComponentRegistration.cs` → remove CoerceAs string, use Shape directly

**Tests: DELETE any test that tests internals. No patching. No adapting. Gone.**
- Tests constructing internal plan model classes → DELETE
- VerifyJson snapshot tests → DELETE (domain model is the truth)
- Tests verifying naming conventions → DELETE
- Tests verifying contract/object/binding creation → DELETE
- Tests verifying merge logic → DELETE
- Tests referencing ANY deleted class name → DELETE

**New tests written fresh:**
- Domain model unit tests: factory methods produce correct plans
- Schema conformance: domain model output validates against schema
- DSL integration: DSL → domain model → JSON → schema validation
- End-to-end: DSL → JSON → boot → runtime behavior

### Step 3: TS Types + Runtime

**Files to replace:**
- `Scripts/types/plan.ts` → new types matching schema exactly

**Files to simplify:**
- `Scripts/resolution/contracts.ts`
  - Delete: getBindingValue(), getBindingShape(), tryGetElementIdForBinding()
  - Simplify: resolveObjectRoot() → two vendor paths only (native → DOM, fusion → ej2)
  - Delete: MemberValueProducer/BindingValueProducer/ContextValueProducer evaluation paths

- `Scripts/resolution/values.ts`
  - Delete: evaluateBindingMap(), resolveContextScope()
  - Delete: evaluateRequestValue() (duplicate of evaluateValue)
  - Simplify: evaluateValue() → literal, read, object, array only

- `Scripts/execution/execute.ts`
  - Set/Call use Source directly — no ActionTarget lookup
  - Shape applied at assignment boundary (read member shape from JsType)

- `Scripts/execution/trigger.ts`
  - Delete: $eventObject contract injection
  - component-event resolves component directly

- `Scripts/conditions/conditions.ts`
  - shape replaces `as`, itemShape replaces `itemAs`

- `Scripts/validation/` — component-based instead of binding-based
  - Error display uses component ID → element ID (direct, no binding indirection)
  - Live-clear uses component ID directly

**Tests to delete:**
- vitest tests verifying old plan types
- vitest tests verifying binding resolution
- vitest tests verifying contract merging

**Tests to rewrite:**
- All vitest → new types, new resolution paths

### Step 4: Playwright (should NOT change)

Playwright tests prove user-visible behavior. If they break, the implementation is wrong — fix the implementation, not the test. These tests are the proof that the rewrite preserves behavior.

## Checkpoints — Am I On The Right Path?

Before proceeding past any step, verify these invariants. If ANY fails, stop and redesign.

### Checkpoint 1: Schema Integrity
- [ ] Shape is ONE shared type, structurally identical everywhere (JsType, ValueProducer, Condition, Validation, Gather)
- [ ] Two sources only (ComponentSource, PayloadSource) — no third source
- [ ] Everything is a component — elements, inputs, containers all resolve the same way
- [ ] No deleted terms appear anywhere (contracts, objects, bindings, coerceAs, readExpr, etc.)
- [ ] Schema describes what the domain model produces — correctness comes from SOLID domain model, not schema constraints
- [ ] No `unknown` in any type definition

### Checkpoint 2: Domain Model Integrity
- [ ] Every enum serializes to its schema string — no raw strings in domain code
- [ ] Shape.None exists — no C# null for Shape values
- [ ] Factory methods enforce invariants — can't construct invalid Reaction/Condition/ValueProducer
- [ ] WriteOnlyPolymorphicConverter writes kind first — no [JsonPropertyOrder] attributes
- [ ] PlanAuthoringContext is under 200 lines
- [ ] **ZERO TRACE CHECK**: `grep -r` for EVERY deleted term across ENTIRE repo. If ANY match found → fix before proceeding.

**ZERO TRACE CHECK — run after EVERY step:**
```bash
grep -r --include="*.cs" --include="*.ts" --include="*.json" --include="*.md" \
  "CapabilityContract\|RuntimeObject\|FieldBinding\|BindingValue\|ConvertValue\|ActionTarget\|coerceAs\|readExpr\|ValueMemberPath\|EventObjectReference\|BindingMap\|PlanAction\|PlanPredicate\|PlanSubscription\|Workflow\b\|ReactivePlanV2\|ScalarValueShape\|ArrayValueShape\|ObjectValueShape\|AnyValueShape" \
  --exclude-dir=node_modules --exclude-dir=bin --exclude-dir=obj --exclude-dir=.worktrees \
  /path/to/repo
```
If this returns ANY result → that file needs attention. Either delete it or rewrite it. Do not proceed to the next step until this returns zero results.

### Checkpoint 3: Runtime Integrity
- [ ] TS types match schema exactly — generated or verified
- [ ] Resolution is uniform — same path for ComponentSource and PayloadSource
- [ ] No binding-related code remains
- [ ] Shape applied at every boundary (set, call, gather, compare)
- [ ] Plan merge and removal both work

### Checkpoint 4: Behavior Preservation — Feature Parity
- [ ] All Playwright tests pass WITHOUT modification
- [ ] DSL surface is frozen — no public API change
- [ ] Every current feature maps to the new schema (112 features verified by reviewer)
- [ ] **Stage parity**: Dispatch, Html.On, .Reactive — each stage has full capability access
- [ ] **Mix & match parity**: inside any stage, all modules combine freely (Element + Condition + HTTP + Dispatch + Component)
- [ ] **Condition interleaving preserved**: unconditional → conditional → unconditional steps in any order. Sequence naturally supports this (branch reactions between non-branch reactions). SOLID improvement: pipeline builder treats conditional and unconditional steps uniformly.
- [ ] **Branch semantics preserved**: first matching case wins, default (no `when`) is the else branch
- [ ] **Parallel preserved**: steps + onSettled guarantees cleanup (WhileLoading pattern)
- [ ] **Request chaining preserved**: request.next chains, each with its own response handlers
- [ ] **Source vs Source preserved**: conditions can compare ComponentSource vs ComponentSource, ComponentSource vs PayloadSource, PayloadSource vs literal

## Verification at Each Step

| Step | Verification |
|---|---|
| 1. Schema | JSON Schema lints clean. Schema documents domain model output. No deleted terms. |
| 2. C# Model | `dotnet build` compiles. `dotnet test` — domain model tests + DSL integration tests pass. VerifyJson gone. |
| 3. TS Runtime | `npm run build:all` compiles. `npm run typecheck` clean. `npm test` — all vitest pass. |
| 4. Playwright | All Playwright tests pass unchanged. Behavior is preserved. |
