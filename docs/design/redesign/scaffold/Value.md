# Value — Implementation Spec (scaffold)

> **How to use this file.** This is a mechanical coding spec. Open it, read §1
> (what you are building), copy §6 (the compile-ready skeleton) into the files in
> §5, fill each `// TODO` body using the matrix fixture it names in §7, and the
> module is done. Every type name comes from
> [`03-naming.md`](../03-naming.md); every responsibility/dependency from
> [`02-micro-modules.md`](../02-micro-modules.md); every behavior fixture from
> [`04-matrix-http-arrays-values.md`](../04-matrix-http-arrays-values.md). Every
> signature below was read from the actual source (`Alis.Reactive/PlanModel/*`,
> `Alis.Reactive/Builders/*`, `Alis.Reactive.Assets/runtime/*`) — not invented.
> Where the redesign changes a shape, the change is called out inline as
> **(redesign)**.

---

## 1. Responsibility, ownership, dependencies

**Responsibility (one sentence).** Value is the value spine: every readable value
is authored through one `TypedSource<T>`, lowered to one flat `ValueExpression`
node, and read back by one slim `evaluateValue` dispatcher — one write path, one
read path, pure-core (no IO, no DOM mutation, no Promise).

**What it owns**

`→` C# authoring / plan side:

- `TypedSource<T>` and the concrete source families collapsed onto it:
  `TypedComponentSource<TProp>`, `TypedPluginSource<TProp>`,
  `TypedPluginPropertySource<TProp>`, `TypedUrlSource<TProp>`,
  `PayloadTypedSource<TPayload,TProp>`, and the array terminals
  `ReactiveValue<TValue>` / `ReactiveArraySource<TElement>`.
- `ValueExpression` — the **flat five-variant** family: `LiteralExpression` ·
  `ReadExpression` · `ObjectExpression` · `ArrayExpression` ·
  `ArrayOperationExpression`. Plus the read sub-model `ValueRead` /
  `ValueReadTarget` and the access discriminator `ValueReadAccess`
  (`PropertyValueReadAccess` / `MethodValueReadAccess`).
- `Source` and its variants (the read origin): `ComponentSource`, `PluginSource`,
  `UrlSource`, `DomSource`, `PayloadSource` (+ the `RuntimeObjectSource` marker).
- The static lowering factory methods on `ValueExpression` (`Literal*`, `Read*`,
  `ReadUrl`, `ReadDom`, `ReadPayload`, `ReadWholePayload`, `ReadWholeElement`,
  `Invoke`, `InvokeElement`, `Object`, `Array`, and the eight `Array*` op
  factories).
- **(redesign)** `WholePayload` / `WholeElement` become **real read variants**,
  not the `"responseBody"` / `"elementValue"` magic-member sentinels.

`⇒` TS runtime side (`Alis.Reactive.Assets/runtime/`):

- `evaluateValue` — the slim dispatcher over the five `ValueExpression` kinds
  (`literal` / `read` / `object` / `array` / `array-op`), with `assertNever`.
- `ArrayOpEngine` — **(redesign)** the eight array ops
  (`count·filter·map·sum·any·all·find·orderBy/orderByDescending`), extracted out
  of today's 300-line `evaluate.ts` god-class into its own module.
- The read sub-handlers: `readFromUrl`, `readFromPayload`, `readFromDom`,
  `readFromRuntimeObject` (component/plugin), `readElementMethod`.
- The runtime value helpers it consumes: `RuntimeValue` / `RuntimePath` (shape
  the read result; walk the path).

**What it depends on** (from the module graph; all downward, acyclic):

- **Shape** (kernel) — `Shape.FromClrType` infers the structural tag at authoring;
  the runtime applies it via `RuntimeValue` / `RuntimeShape` (`applyShape` /
  `applyShapeWhenPresent`). Value never re-derives a shape — **shape-once**.
- **Kind** (kernel) — every node carries its `kind`; `PlanSerializer` writes it,
  `PlanContractGenerator` reflects it into `plan.ts`, `assertNever` proves the
  runtime switch is exhaustive.

Value depends on **nothing else**. It is depended on by Condition, Reaction,
Request, Component, Plugin, and Validation. **Keep it leaf-clean:** no `import`
of Condition/Request/Reaction from the value read path. (Today `evaluate.ts`
imports `evaluateSyncCondition` for array-op predicates — see §3 note; in the
redesign the per-element predicate/projection callback is **passed in** to
`ArrayOpEngine`, so Value still does not depend up on Condition.)

---

## 2. Public surface (exact types + signatures)

> Visibility rule (Rule 8): plan-node classes are `public sealed` (so STJ + the
> contract generator can reflect them) with **`internal` constructors** and
> factory methods. Builder/source classes a developer chains are `public`;
> their constructors are `internal`. Nothing here is constructed in app code —
> developers reach it through `Html.*` / `p.*` builder verbs.

### 2.1 Authoring surface — `TypedSource<T>` and its families (`→`)

```csharp
// Alis.Reactive/Builders/Conditions/TypedSource.cs
/// <summary>The one typed authoring surface for any readable value. Preserves the
/// property type through the condition, mutation, gather, and dispatch pipelines.</summary>
public abstract class TypedSource<TProp>
{
    internal abstract ValueExpression ToValueExpression();   // the single lowering entry
    internal Shape Shape => Shape.FromClrType(typeof(TProp));        // shape-once at authoring
    internal Shape ElementShape => Shape.CollectionItemShapeOrNone(typeof(TProp));
}
```

Concrete families (each `public sealed`, `internal` ctor, override
`ToValueExpression()`):

| Type | Source kind it lowers to | Public verb that hands it back |
|---|---|---|
| `TypedComponentSource<TProp>` | `ComponentSource` property; `FromMethod(...)` → method | typed component slice read; `p.Component<T>(...)` member |
| `TypedPluginSource<TProp>` | `PluginSource` method (`Invoke`) | `p.Plugin<T>(name[,member])` (implicit) |
| `TypedPluginPropertySource<TProp>` | `PluginSource` property | `p.PluginProperty<T>(name, member)` |
| `TypedUrlSource<TProp>` | `UrlSource` query param | `p.FromUrl(name)` / `p.FromUrl<T>(name)` |
| `PayloadTypedSource<TPayload,TProp>` | `PayloadSource` path read | `g.FromEvent(args, e => e.X)` / `json.Read(...)` |
| `ReactiveValue<TValue>` | wraps any composed `ValueExpression` (array terminal) | `arr.Count()/Sum()/Any()/Find()` |
| `ReactiveArraySource<TElement>` | wraps a composed `array-op` node | `arr.AsSource()` |

### 2.2 Plan node family — `ValueExpression` (`→`)

`public abstract class ValueExpression` exposes `internal abstract Shape OutputShape`
and the **static lowering factories** (all `internal`). These are the only way a
slice produces a value node:

```csharp
// Literals
internal static ValueExpression Literal(bool|string|int|long|decimal|double|DateTime value);
internal static ValueExpression Null();                                  // → Shape.None
internal static ValueExpression LiteralRaw(object? value, Shape shape);
internal static ValueExpression LiteralFromValue(object? value);         // null→Null(); else Shape.FromValue

// Reads — component / plugin / url / dom / payload
internal static ValueExpression Read(Source from, string member);                 // Shape.None
internal static ValueExpression Read(Source from, string member, Path path);      // Shape.None
internal static ValueExpression Read(Source from, string member, Shape shape);
internal static ValueExpression Read(Source from, string member, Path path, Shape shape);
internal static ValueExpression ReadUrl(string paramName);                        // Shape.String default
internal static ValueExpression ReadUrl(string paramName, Shape shape);
internal static ValueExpression ReadDom(string elementId, string member, Shape shape);
internal static ValueExpression ReadPayload(PayloadSource from, string path);
internal static ValueExpression ReadPayload(PayloadSource from, string path, Shape shape);
internal static ValueExpression ReadWholePayload(PayloadSource from[, Shape shape]);   // (redesign) variant
internal static ValueExpression ReadWholeElement([Shape shape]);                       // (redesign) variant
internal static ValueExpression Invoke(RuntimeObjectSource from, string method, Shape returns, IReadOnlyList<ValueExpression> args);
internal static ValueExpression InvokeElement(string receiverPath, string method, Shape returns, IReadOnlyList<ValueExpression> args);

// Composites
internal static ObjectExpression Object(IReadOnlyDictionary<string, ValueExpression> fields[, Shape shape]);
internal static ValueExpression Array(IReadOnlyList<ValueExpression> items[, Shape shape]);

// Array ops (one ArrayOperationExpression each; op string is the sub-discriminator)
internal static ValueExpression ArrayCount   (ValueExpression source, Shape itemShape);
internal static ValueExpression ArrayFilter  (ValueExpression source, ConditionGraph predicate, Shape itemShape);
internal static ValueExpression ArrayMap     (ValueExpression source, ValueExpression projection, Shape itemShape, Shape resultItemShape);
internal static ValueExpression ArraySum     (ValueExpression source, ValueExpression? projection, Shape itemShape);
internal static ValueExpression ArrayAny     (ValueExpression source, ConditionGraph? predicate, Shape itemShape);
internal static ValueExpression ArrayAll     (ValueExpression source, ConditionGraph predicate, Shape itemShape);
internal static ValueExpression ArrayFind    (ValueExpression source, ConditionGraph? predicate, ValueExpression? projection, Shape itemShape, Shape resultShape);
internal static ValueExpression ArrayOrderBy (ValueExpression source, ValueExpression key, Shape itemShape, bool descending);
```

The five concrete node classes (each `public sealed`, `string Kind` literal,
`public` getters, `internal` ctor, `internal override Shape OutputShape`):

| Class | `Kind` | Public getters (camelCase on wire) |
|---|---|---|
| `LiteralExpression` | `"literal"` | `object? Value`, `Shape Shape` |
| `ReadExpression` | `"read"` | `Source From`, `string Member`, `Path Path`, `Shape Shape`, `ValueReadAccess Access` |
| `ObjectExpression` | `"object"` | `IReadOnlyDictionary<string,ValueExpression> Fields`, `Shape Shape` |
| `ArrayExpression` | `"array"` | `IReadOnlyList<ValueExpression> Items`, `Shape Shape` |
| `ArrayOperationExpression` | `"array-op"` | `string Op`, `ValueExpression Source`, `ConditionGraph? Predicate`, `ValueExpression? Projection`, `Shape ItemShape`, `Shape Shape` |

Access discriminator (`public abstract class ValueReadAccess`, `Kind` getter):
`PropertyValueReadAccess` (`"property"`) · `MethodValueReadAccess` (`"method"`,
`IReadOnlyList<ValueExpression> Args`).

`Source` family (`public abstract class Source`, each variant `public sealed`,
`Kind` literal, `internal` factory `Of(...)`):

| Class | `Kind` | Public getters |
|---|---|---|
| `ComponentSource : RuntimeObjectSource` | `"component"` | `string Component` |
| `PluginSource : RuntimeObjectSource` | `"plugin"` | `string Name`, `string Type` |
| `UrlSource` | `"url"` | — (singleton `Instance`) |
| `DomSource` | `"dom"` | `string Element` |
| `PayloadSource` | `"payload"` | `string Scope`, `PayloadContract Type` |

### 2.3 Runtime counterpart — TS contract + executor (`⇒`)

The contract types in `runtime/types/plan.ts` are **generated by `PlanContractGenerator`**
(Kind kernel) — do not hand-edit them. They are listed here only so the executor
skeleton type-checks against them. The discriminated unions (exact, from source):

```ts
export type ValueExpression =
  | LiteralExpression | ReadExpression | ObjectExpression
  | ArrayExpression | ArrayOperationExpression;

export type ReadExpression =
  | ObjectPropertyReadExpression | ObjectMethodReadExpression
  | UrlParameterReadExpression | PayloadPathReadExpression
  | WholePayloadReadExpression | WholeElementReadExpression
  | DomPropertyReadExpression | ElementMethodReadExpression;

export interface LiteralExpression { kind: "literal"; value: JsonValue; shape: Shape; }
export interface ObjectExpression  { kind: "object"; fields: Record<string, ValueExpression>; shape: Shape; }
export interface ArrayExpression   { kind: "array"; items: ValueExpression[]; shape: Shape; }
export type    ArrayOp            = "count"|"filter"|"map"|"sum"|"any"|"all"|"find"|"orderBy"|"orderByDescending";
export interface ArrayOperationExpression {
  kind: "array-op"; op: ArrayOp; source: ValueExpression;
  predicate?: ValidationCondition; projection?: ValueExpression;
  itemShape: Shape; shape: Shape;
}
// WholePayloadReadExpression.member: "responseBody"; WholeElementReadExpression.member: "elementValue"
```

The runtime entry signature (kept exactly):

```ts
export function evaluateValue(expression: ValueExpression, plan: PlanDocument, ctx?: ExecContext): unknown;
```

The new extracted engine entry (redesign):

```ts
// runtime/value/array-op-engine.ts
export function runArrayOp(
  expression: ArrayOperationExpression,
  source: unknown[],
  evaluateInElement: (e: ValueExpression, item: unknown) => unknown,   // projection in element scope
  elementMatches: (predicate: ValidationCondition, item: unknown) => boolean,  // sync predicate
): unknown;
```

---

## 3. Input → Output contract + invariants

**Flows in (author):** a CLR-typed value request — a literal, a typed source
read (`TypedSource<T>.ToValueExpression()`), a composite (object/array), or an
array transform (`ReactiveArray<T>` op chain).

**Produces (domain):** exactly one `ValueExpression` node, carrying its `Shape`
(inferred once via `Shape.FromClrType`/`FromValue`), its `Kind` discriminator, and
for reads a `Source` + `ValueReadAccess` + `Path`.

**Flows in (runtime):** that node + `PlanDocument` + optional `ExecContext`.

**Produces (runtime):** the read JS value, shape-applied **once** on read
(`applyShapeWhenPresent`). Pure: no fetch, no Promise, no DOM write.

**Invariants — enforced by construction (value objects), not by guarding plans:**

- **Null is unrepresentable by construction.** A C# null value lowers to
  `ValueExpression.Null()` → `LiteralExpression(null, Shape.None)` — a real node
  with shape `none`, never a `null` node reference. The constructors reject a
  `null` *shape* / `null` *child node* with `ArgumentNullException` because those
  are authoring (developer DSL) errors at a real boundary — **not** plan-validation
  of framework output. (Source: `LiteralExpression`, `ReadExpression`,
  `ObjectExpression`, `ArrayExpression`, `ArrayOperationExpression` ctors all
  `?? throw new ArgumentNullException`.)
- **Shape is inferred once.** `TypedSource<T>.Shape` and the `Literal(...)`
  overloads pin the shape from the CLR type. `DateTime` → ISO `"O"` string +
  `Shape.Date`. URL params default `Shape.String`. The runtime never re-derives.
- **Object shape is a closed contract.** `Object(fields)` derives
  `Shape.ObjectOf(field→OutputShape)`; empty/whitespace field name or null field
  value → authoring throw.
- **Array shape collapses deterministically.** Homogeneous items → `array<item>`;
  mixed/empty → `array<any>` (`ArrayShape`). Null item → authoring throw.
- **`WholePayload`/`WholeElement` are explicit variants** (redesign): the read
  has no member-path; the runtime returns the scope root unwalked. On the wire
  the discriminating member stays `"responseBody"`/`"elementValue"` (the contract
  generator emits the literal-typed `member`), but the C# author cannot fat-finger
  the string — only the dedicated factory produces it.
- **Array-op predicate/projection presence is per-op** (redesign): `count` carries
  neither; `filter/any/all/find` carry a predicate; `map/sum/orderBy` carry a
  projection; `find` may carry both. The matrix names which fields each op uses
  (§7 Part C). Predicates are the **sync condition subset** (compare/all/any/not —
  never confirm), so the whole array DSL stays on the immediate lane.
- **OrderBy key must be a sortable scalar** — enforced at authoring in
  `ReactiveArray<T>.Order` (`string|number|boolean|date|nullable`); a non-scalar
  key throws `InvalidOperationException`, preventing a `"[object Object]"` mis-sort.

> **Boundary-only runtime throws (allowed, not validators):** `getElementById`
> null for a dom source; array-op source not iterable (`normalizeToArray`);
> `find` with no predicate (generation invariant); a non-function element method.
> These are real external edges, exactly as the existing source already handles
> them. Do **not** add any other runtime guard against the generated plan shape.

---

## 4. Sync / async lane

**Always sync.** `evaluateValue` and `ArrayOpEngine` are pure reads — zero
Promise, zero IO, zero DOM mutation. Value never opens the async lane. (Request
opens async; Value is read inside both lanes but is itself synchronous.)

---

## 5. File layout (files to create / where)

`→` C# (`Alis.Reactive/`) — already organized; the redesign keeps these homes:

```
Alis.Reactive/
├── PlanModel/
│   ├── ValueExpression.cs        # base + 5 node classes + static factories + ValueRead/Target + ValueReadAccess
│   └── Source.cs                 # Source + ComponentSource/PluginSource/UrlSource/DomSource/PayloadSource (+ RuntimeObjectSource)
└── Builders/
    ├── Conditions/
    │   ├── TypedSource.cs                 # TypedSource<T> base
    │   ├── TypedComponentSource.cs
    │   ├── TypedPluginSource.cs           # TypedPluginSource<T> + TypedPluginPropertySource<T>
    │   ├── TypedUrlSource.cs
    │   └── PayloadTypedSource.cs
    └── Arrays/
        ├── ReactiveArray.cs               # ReactiveArray<T> op chain + ReactiveArraySource<T> (AsSource)
        ├── ReactiveValue.cs               # ReactiveValue<T> scalar terminal
        └── ElementExpressionCompiler.cs   # lambda → ConditionGraph predicate / ValueExpression projection (element scope)
```

`⇒` TS (`Alis.Reactive.Assets/runtime/`):

```
runtime/
├── core/
│   └── evaluate.ts               # evaluateValue dispatcher (5 kinds) + read sub-handlers; (redesign) array-op delegated out
├── value/
│   └── array-op-engine.ts        # (redesign — NEW) runArrayOp: the 8 ops, extracted from evaluate.ts
├── domain/
│   ├── runtime-value.ts          # RuntimeValue / applyShapeWhenPresent / isMissingRuntimeValue (kept)
│   ├── runtime-path.ts           # RuntimePath read/call (kept)
│   └── runtime-shape.ts          # RuntimeShape.apply (kept; Shape kernel)
└── types/
    └── plan.ts                   # GENERATED Value union (do not hand-edit; Kind kernel owns it)
```

---

## 6. Compile-ready skeleton

> Fill each `// TODO(<fixture>)` body using the named §7 fixture. Signatures,
> `Kind` strings, ctor visibility, and getter names are fixed — type them as-is.

### 6.1 `→` `ValueExpression` factories (lowering) — `PlanModel/ValueExpression.cs`

```csharp
public abstract class ValueExpression
{
    private protected ValueExpression() { }
    internal abstract Shape OutputShape { get; }

    internal static ValueExpression Literal(string value) =>
        new LiteralExpression(value, Shape.String);            // TODO(A.1 Literal — scalar): mirror for bool/int/long/decimal/double
    internal static ValueExpression Literal(DateTime value) =>
        new LiteralExpression(value.ToString("O"), Shape.Date); // TODO(A.1 Literal — scalar): ISO "O" round-trip
    internal static ValueExpression Null() =>
        new LiteralExpression(null, Shape.None);                // TODO(A.1 Literal — null): Shape.None, not a typed default
    internal static ValueExpression LiteralFromValue(object? value) =>
        value == null ? Null() : LiteralRaw(value, Shape.FromValue(value)); // TODO(A.1 Literal — arbitrary)

    internal static ValueExpression Read(Source from, string member, Shape shape) =>
        new ReadExpression(ValueRead.Property(from, member, shape));        // TODO(A.2 Read — component property)
    internal static ValueExpression Invoke(RuntimeObjectSource from, string method, Shape returns, IReadOnlyList<ValueExpression> args) =>
        new ReadExpression(ValueRead.Method(from, method, returns, args));  // TODO(A.2 Read — component/plugin method)
    internal static ValueExpression ReadUrl(string paramName, Shape shape) =>
        Read(UrlSource.Instance, paramName, shape);                         // TODO(A.2 Read — URL query param)
    internal static ValueExpression ReadDom(string elementId, string member, Shape shape) =>
        Read(DomSource.Of(elementId), member, Path.Parse(member), shape);   // TODO(A.2 Read — DOM member)
    internal static ValueExpression ReadPayload(PayloadSource from, string path, Shape shape) =>
        Read(from, path, Path.Parse(path), shape);                          // TODO(A.2 Read — payload)
    internal static ValueExpression ReadWholePayload(PayloadSource from, Shape shape) =>
        new ReadExpression(ValueRead.WholePayload(from, shape));            // TODO(A.2 Read — WHOLE payload): real variant
    internal static ValueExpression ReadWholeElement(Shape shape) =>
        new ReadExpression(ValueRead.WholeElement(PayloadSource.Element(), shape)); // TODO(A.2 Read — WHOLE element)

    internal static ObjectExpression Object(IReadOnlyDictionary<string, ValueExpression> fields) =>
        /* TODO(A.3 Object value): derive closed Shape.ObjectOf(field→OutputShape); reject null/blank name + null value */ default!;
    internal static ValueExpression Array(IReadOnlyList<ValueExpression> items) =>
        /* TODO(A.3 Array value): homogeneous→array<item>, mixed/empty→array<any>; reject null item */ default!;

    internal static ValueExpression ArrayCount(ValueExpression source, Shape itemShape) =>
        new ArrayOperationExpression("count", source, itemShape, Shape.Number);      // TODO(C count): no predicate
    internal static ValueExpression ArrayFilter(ValueExpression source, ConditionGraph predicate, Shape itemShape) =>
        new ArrayOperationExpression("filter", source, itemShape, Shape.ArrayOf(itemShape), predicate);  // TODO(C filter)
    internal static ValueExpression ArrayMap(ValueExpression source, ValueExpression projection, Shape itemShape, Shape resultItemShape) =>
        new ArrayOperationExpression("map", source, itemShape, Shape.ArrayOf(resultItemShape), projection: projection); // TODO(C map)
    internal static ValueExpression ArraySum(ValueExpression source, ValueExpression? projection, Shape itemShape) =>
        new ArrayOperationExpression("sum", source, itemShape, Shape.Number, projection: projection);    // TODO(C sum)
    internal static ValueExpression ArrayAny(ValueExpression source, ConditionGraph? predicate, Shape itemShape) =>
        new ArrayOperationExpression("any", source, itemShape, Shape.Boolean, predicate: predicate);     // TODO(C any)
    internal static ValueExpression ArrayAll(ValueExpression source, ConditionGraph predicate, Shape itemShape) =>
        new ArrayOperationExpression("all", source, itemShape, Shape.Boolean, predicate: predicate);     // TODO(C all)
    internal static ValueExpression ArrayFind(ValueExpression source, ConditionGraph? predicate, ValueExpression? projection, Shape itemShape, Shape resultShape) =>
        new ArrayOperationExpression("find", source, itemShape, resultShape, predicate, projection);     // TODO(C find)
    internal static ValueExpression ArrayOrderBy(ValueExpression source, ValueExpression key, Shape itemShape, bool descending) =>
        new ArrayOperationExpression(descending ? "orderByDescending" : "orderBy", source, itemShape, Shape.ArrayOf(itemShape), projection: key); // TODO(C orderBy)
}

public sealed class LiteralExpression : ValueExpression
{
    public string Kind => "literal";
    [JsonInclude] public object? Value { get; }
    public Shape Shape { get; }
    internal LiteralExpression(object? value, Shape shape) { Value = value; Shape = shape ?? throw new ArgumentNullException(nameof(shape)); }
    internal override Shape OutputShape => Shape;
}

public sealed class ReadExpression : ValueExpression
{
    private readonly ValueRead _read;
    public string Kind => "read";
    public Source From => _read.From;
    public string Member => _read.Member.Value;
    public Path Path => _read.Path;
    public Shape Shape => _read.Shape;
    public ValueReadAccess Access => _read.Access;
    internal ReadExpression(ValueRead read) { _read = read ?? throw new ArgumentNullException(nameof(read)); }
    internal override Shape OutputShape => Shape;
}

public sealed class ObjectExpression : ValueExpression   // public getters: Fields, Shape; Kind="object"; internal ctor
public sealed class ArrayExpression  : ValueExpression   // public getters: Items, Shape;  Kind="array";  internal ctor
public sealed class ArrayOperationExpression : ValueExpression // Op, Source, Predicate?, Projection?, ItemShape, Shape; Kind="array-op"
// Predicate/Projection keep [JsonIgnore(WhenWritingNull)] — audited per-op nullability, see §3.

public abstract class ValueReadAccess { public abstract string Kind { get; } /* Property singleton + Method(args) */ }
public sealed class PropertyValueReadAccess : ValueReadAccess { public override string Kind => "property"; }
public sealed class MethodValueReadAccess  : ValueReadAccess { public override string Kind => "method"; public IReadOnlyList<ValueExpression> Args => _args; }
```

### 6.2 `→` a `TypedSource` family member — `Builders/Conditions/TypedComponentSource.cs`

```csharp
public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
{
    private readonly ValueExpression _value;
    private readonly string _readMember;
    internal TypedComponentSource(string componentId, string valueMember)
        : this(valueMember, ValueExpression.Read(ComponentSource.Of(componentId), valueMember, Shape.FromClrType(typeof(TProp)))) { }
    private TypedComponentSource(string readMember, ValueExpression value) { _readMember = readMember; _value = value; }
    internal override ValueExpression ToValueExpression() => _value;          // TODO(A.2 Read — component property)
    internal string DefaultPayloadName => _readMember;                        // gather default key (B.3)
    internal static TypedComponentSource<TProp> FromMethod(ComponentSource component, string method, IReadOnlyList<ValueExpression> args) =>
        new(method, ValueExpression.Invoke(component, method, Shape.FromClrType(typeof(TProp)), args)); // TODO(A.2 Read — component method)
}
// Mirror the same one-liner shape for TypedPluginSource<T>, TypedPluginPropertySource<T>,
// TypedUrlSource<T>, PayloadTypedSource<TPayload,TProp>, ReactiveValue<T>, ReactiveArraySource<T>.
```

### 6.3 `⇒` runtime dispatcher — `core/evaluate.ts`

```ts
export function evaluateValue(expression: ValueExpression, plan: PlanDocument, ctx?: ExecContext): unknown {
  return ValueEvaluation.from(plan, ctx).evaluate(expression);
}

class ValueEvaluation {
  evaluate(expression: ValueExpression): unknown {
    switch (expression.kind) {
      case "literal":  return applyShape(expression.value, expression.shape);      // TODO(A.1 Literal — scalar/null/arbitrary)
      case "read":     return this.evaluateRead(expression);                        // TODO(A.2 Read — all P-SOURCE)
      case "object":   return RuntimeValue.declared(this.evaluateObject(expression.fields), expression.shape).usingDeclaredShape(); // TODO(A.3 Object)
      case "array":    return RuntimeValue.declared(expression.items.map(i => this.evaluate(i)), expression.shape).usingDeclaredShape(); // TODO(A.3 Array)
      case "array-op": return runArrayOp(expression, this.sourceArray(expression),
                          (e, item) => this.inElement(item).evaluate(e),
                          (p, item) => this.elementMatches(p, item));               // TODO(C all ops): delegate to ArrayOpEngine
      default:         return assertNever(expression, "value expression kind");
    }
  }

  private evaluateRead(expression: ReadExpression): unknown {
    if (isObjectRead(expression)) return this.readFromRuntimeObject(expression, expression.from); // TODO(A.2 component/plugin property+method)
    if (isUrlRead(expression))    return readFromUrl(expression, this.plan.urlParameters());       // TODO(A.2 URL)
    if (isPayloadRead(expression)) {
      if (isElementMethodRead(expression)) return this.readElementMethod(expression);              // TODO(A.2 element method)
      return readFromPayload(expression, this.context.resolvePayload(expression.from));            // TODO(A.2 payload + WHOLE payload/element)
    }
    if (isDomRead(expression))    return readFromDom(expression);                                   // TODO(A.2 DOM member)
    return assertNever(expression, "read expression");
  }
  // readFromRuntimeObject / readElementMethod / evaluateObject / inElement / elementMatches: as today.
}
// readFromUrl / readFromPayload / readFromDom + the isXRead type guards: kept as in current evaluate.ts.
// readsWholePayload === member "responseBody"; readsWholeElement === member "elementValue".
```

### 6.4 `⇒` array-op engine (redesign, NEW) — `value/array-op-engine.ts`

```ts
export function runArrayOp(
  expression: ArrayOperationExpression,
  source: unknown[],
  project: (e: ValueExpression, item: unknown) => unknown,
  elementMatches: (p: ValidationCondition, item: unknown) => boolean,
): unknown {
  const items = normalizeToArray(source, expression.op);     // boundary normalize (kept)
  switch (expression.op) {
    case "count":  return items.length;                                                 // TODO(C count)
    case "filter": return shaped(items.filter(i => elementMatches(req(expression.predicate), i)), expression); // TODO(C filter)
    case "map":    return shaped(items.map(i => project(req(expression.projection), i)), expression);          // TODO(C map)
    case "sum":    return items.reduce<number>((t, i) => t + toNumber(self(expression.projection, i, project)), 0); // TODO(C sum)
    case "any":    return expression.predicate === undefined ? items.length > 0 : items.some(i => elementMatches(expression.predicate!, i)); // TODO(C any)
    case "all":    return items.every(i => elementMatches(req(expression.predicate), i));           // TODO(C all)
    case "find":   return findElement(expression, items, project, elementMatches);                  // TODO(C find): null when none; throw if no predicate
    case "orderBy":           return shaped(ordered(items, expression.projection, false, project), expression); // TODO(C orderBy)
    case "orderByDescending": return shaped(ordered(items, expression.projection, true,  project), expression); // TODO(C orderBy)
    default:       return assertNever(expression.op, "array-op kind");
  }
}
// normalizeToArray / toNumber / compareKeys / ordered / findElement / shaped: lift verbatim from today's evaluate.ts.
```

---

## 7. Acceptance fixtures (matrix cases this module must satisfy)

Every named row below is the fixture a body in §6 must reproduce. Source:
[`04-matrix-http-arrays-values.md`](../04-matrix-http-arrays-values.md). A body
is done when its case's exact plan JSON + browser behavior matches the row.

**Part A — Values (A.1 literals · A.2 reads ×P-SOURCE · A.3 composites):**

1. `Literal — scalar` (string/int/long/decimal/double/bool/DateTime; DateTime→ISO "O" + `Shape.Date`)
2. `Literal — null` (`Shape.None`, not a typed default)
3. `Literal — arbitrary value` (enum/Guid/object → `Shape.FromValue`; `any` when unclassifiable)
4. `Read — component property`
5. `Read — component method`
6. `Read — plugin method`
7. `Read — plugin property`
8. `Read — URL query param (untyped)` (default `Shape.String`)
9. `Read — URL query param (typed)` (scalar coercion)
10. `Read — payload (event/success/error/request/dispatch)` (parameterized over PayloadScope)
11. `Read — WHOLE payload` (redesign: real variant, not `"responseBody"` sentinel)
12. `Read — WHOLE element` (redesign: real variant, not `"elementValue"` sentinel)
13. `Read — DOM member`
14. `Object value` (closed object shape; field-name conflict → authoring throw)
15. `Array value (literal items)` (homogeneous→`array<item>`, mixed/empty→`array<any>`)

**Part C — Arrays (the eight ops + entries + composition):**

16. `Source — From(TypedSource<T[]>)`  ·  17. `Source — From(event arg array)`  ·  18. `Source — FromDom(id, member)`
19. `count (unconditional)`  ·  20. `count (predicated)` (sugar = filter→count)
21. `filter`  ·  22. `map`  ·  23. `sum` (projection optional)
24. `any (unconditional)` (non-empty)  ·  25. `any (predicated)`  ·  26. `all` (vacuously true on empty)
27. `find (element)` (null when none; predicate required — runtime throw if absent)  ·  28. `find (projected field)`
29. `orderBy / orderByDescending` (ascending default; non-scalar key → authoring throw)
30. `Chained ops` (each op wraps the prior as its `source`)  ·  31. `Terminal as TypedSource` (`AsSource()` — closes the gather hole)

**Cross-boundary fixtures (Value read used by another band — assert the read node only):**

32. `Gather payload — typed component (by expression)` (B.3) — proves `TypedSource → ValueExpression` reaches gather (one read path)
33. `Gather payload — from event arg` (B.3) and `Gather payload — from URL query` (B.3) — same read node shapes as A.2

> **Determinism proof to keep green:** every readable value funnels through one
> `ValueExpression` variant and is read back by one `evaluateValue` case; the two
> old representable-but-invalid holes (`responseBody`/`elementValue` sentinels and
> the gather-source hole) are closed. Prove with: C# domain test (DSL call → node
> shape, fixtures 1–31), TS runtime test (`evaluateValue` behavior in jsdom),
> `npm run typecheck` (generated `plan.ts` agrees), and a Playwright slice for the
> page-visible read (e.g. component property → `SetText`).
