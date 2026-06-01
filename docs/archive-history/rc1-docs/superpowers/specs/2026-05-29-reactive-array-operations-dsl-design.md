# Reactive Array Operations DSL — Design Proposal

Date: 2026-05-29
Status: APPROVED & VALIDATED — senior-architect panel: clean approval (0 blocking). JS ground-truth (real EJ2 + DOM): true-with-additions (§14.11 array-like normalization, starts-with→v1 core; dataset stays plugin). Cleared for implementation per §8.
Author: design session, grounded in first-hand DSL source study

## 1. One-line statement

Add a **typed, custom array-operations DSL** — `ReactiveArray<T>` — that captures
what the author expresses in C# (predicates, projections, aggregations) as
**expression trees**, compiles them into **closed plan-JSON nodes**, and executes
them through the framework's **deterministic runtime interpreter**. No
developer-authored JavaScript. No plugin escape hatch for these operations. The
array source is any existing value: a `.Reactive()` event payload (`e.Data` of
`T[]`), a component property/method that returns an array, or an HTTP response
body path.

## 2. Problem and intent

Today, "complex array manipulation" is explicitly delegated to the **plugin
escape hatch** (root `CLAUDE.md`: *"Plugin is the intentional escape hatch when
deterministic JSON DSL is not enough, such as URL APIs, DOM APIs, or complex
array manipulation."*). The sandbox proves the demand and the cost: the
`ArrayPlugin` (`Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml:170-186`)
hand-registers `count`, `pluck`, `filter`, `sum`, `some` as **opaque JS
functions** in `sandbox-plugins.ts`. That logic is JavaScript that "stands" — it
is **not** in the plan graph, not visible to the plan contract, not in the
coverage matrix, and not deterministic by construction.

The intent: promote a deterministic, **closed** set of array operations out of
the plugin escape hatch and into **first-class plan vocabulary**. After this
change, `filter`/`count`/`sum`/etc. are plan nodes the runtime interprets, not
JS the developer wrote. The plugin remains for its own distinct
job — integrating 3rd-party JS libraries/objects and composing multiple browser
calls into one named operation — not as a fallback for array work.

### Non-goals

- **Not** an arbitrary JS engine. The author cannot express "any JS." The node
  set is closed; anything outside it throws at C# build time.
- **Not** `IQueryable`/LINQ. We deliberately avoid LINQ to prevent server-side
  execution and name collision (see §4).
- **Not** a new reaction node. Array operations are **values**, consumed by the
  reactions that already exist (set/call/dispatch/condition/gather/inject).

## 3. Source-grounded architecture (what the study established)

All claims below were verified first-hand or by five parallel code-explorer
passes over actual source.

1. **The value path is universal — one resolver.** `evaluateValue()`
   (`runtime/core/evaluate.ts:21`) is the single recursive entry point for every
   value: set, call, dispatch, condition, gather, inject all call it. The switch
   at `evaluate.ts:35` handles exactly four kinds — `literal`, `read`, `object`,
   `array` — with `assertNever(expression, "value expression kind")` at line 54.
   A new value kind is **new cases in this one switch**; every consumer inherits
   it for free. This is why array operations belong as **values**, not reactions.

2. **Array sources already exist and already carry element shape.**
   - `.Reactive()` event payload: `e.Data`/`e.Value` typed `T[]` compiles to a
     `read` from `PayloadSource{scope:"event"}` carrying `Shape.ArrayOf(item)`
     (`PayloadTypedSource.cs`, `ExpressionPathHelper.ToEventPath`). Runtime reads
     `ctx.event.value` — a live JS array (`trigger.ts`, `evaluate.ts:126`).
   - Component property/method: `multiSelect.Value()` → `TypedComponentSource<string[]>`;
     `schedule.GetEvents()` → `TypedComponentSource<FusionScheduleEventData[]>`
     (`ComponentRef.Read<T>`, `FusionScheduleExtensions.cs:87`).
   - HTTP response body path: `responseBody.Read(r => r.Items)` (`scope:"success"`).
   - Element shape round-trips fully: C# `Shape.ArrayOf(inner)` → JSON
     `{kind:"array", item: inner}` → runtime `applyShape(item, shape.item)`
     (`Shape.cs:40-47`, `shape-convert.ts:62-64`).

3. **The runtime scope model is immutable and frame-based.**
   `ExecutionContext` (`execution-context.ts`) grows by spreading a new frame:
   `withRequest(...)` / `withResponse(...)` (lines 39-45). Scopes resolved in
   `resolvePayload` (line 47): `event`, `dispatch`, `success`, `error`,
   `request`, `local`. **`local` is reserved but unused** (`Source.cs:94`,
   `execution-context.ts:58`) — the slot for ad-hoc scoped values was
   anticipated. A per-element binding is the same pattern: a `withElement(item)`
   frame.

4. **The one deliberately-closed door — the expression compiler.**
   `ExpressionPathHelper` translates `x => x.Address.City` to a dot-path but
   **throws on computed expressions** (`ExpressionPathHelper.cs:122-124`:
   *"only supports property-access chains and MVC indexer paths… Got unsupported
   expression node"*). To let an author write `x => x.Price * x.Qty` or
   `x => x.Status == "active" && x.Age > 65` and have it become **plan JSON**, we
   need a *new, richer* expression compiler. This build-time throw discipline is
   exactly what keeps the custom DSL from degrading into arbitrary JS.

5. **Predicates already have a deterministic algebra: `ConditionGraph`.**
   `CompareCondition` carries `op` over 21 operators (`CompareOp.cs`), and
   `conditions.ts` evaluates them through the *same* `evaluateValue` (one
   resolver, confirmed). `all`/`any`/`not` compose them. So a `Where` predicate
   *is* a `ConditionGraph` whose leaf reads target the element scope — no new
   boolean algebra needed.

6. **Adding a value kind is a known, low-cost, well-fenced change.** The TS
   contract is hand-emitted in `PlanModel/PlanTypeScriptContract.cs` (not
   reflection); `WriteOnlyPolymorphicConverter` needs **zero** changes (it
   delegates to `value.GetType()`, and the public `Kind` property becomes the
   discriminator automatically). `assertNever` at every switch forces the runtime
   to handle the new kind or fail to compile.

## 4. Authoring surface — the capture constraint (load-bearing)

If `e.Data` is `IReadOnlyList<Resident>` and the author writes `.Where(x => …)`,
that binds to **`System.Linq.Enumerable.Where`**: it takes a `Func<>`, executes
**server-side**, and returns a filtered list. Nothing is captured; nothing
reaches the plan. That is unacceptable — we must **collect what the author
expressed**, not run it.

The resolution is a single design choice that solves both "typed" and "no LINQ
collision":

- The source is wrapped in a **dedicated, strongly-typed builder**,
  `ReactiveArray<T>`, that **does not implement `IEnumerable<T>` or
  `IQueryable<T>`**. Therefore LINQ extension methods are not candidates at all —
  zero collision, and no stray `using System.Linq` can hijack a call.
- Every operator parameter is **`Expression<Func<T, …>>`, not `Func<T, …>`** — so
  the lambda is **captured as an expression tree**, never invoked.
- The element type **flows through transforms, fully typed**:
  - `Where(Expression<Func<T,bool>>)            → ReactiveArray<T>`
  - `Select<TOut>(Expression<Func<T,TOut>>)     → ReactiveArray<TOut>`
  - `Count() / Count(Expression<Func<T,bool>>)  → ReactiveValue<int>`
  - `Sum(Expression<Func<T,decimal>>)           → ReactiveValue<decimal>`
  - `Any(Expression<Func<T,bool>>)              → ReactiveValue<bool>`
  - `OrderBy<TKey>(Expression<Func<T,TKey>>)    → ReactiveArray<T>`

Both `ReactiveArray<T>` (array-valued) and `ReactiveValue<TScalar>` (scalar-valued)
are thin builders that **carry an accumulating `ValueExpression`** and expose
`.ToValueExpression()` internally. They plug into every place a `TypedSource<T>`
is accepted today (set, dispatch payload, condition operand, gather, element text).

```csharp
// p.From captures e => e.Data as an Expression and returns OUR builder (not IEnumerable):
ReactiveArray<Resident> src = p.From(args, e => e.Data);

ReactiveArray<Resident> active =
    src.Where(x => x.Status == "active" && x.Age > 65);          // CAPTURED, not run

ReactiveArray<ResidentRow> rows =
    active.OrderByDescending(x => x.LastVisit)
          .Select(x => new ResidentRow(x.Name, x.Price * x.Qty)); // typed projection, CAPTURED

// Consumed by an existing reaction — array result written to a component property:
p.Component<FusionGrid>(m => m.Grid).SetDataSource(rows);

// Scalar result into element text:
p.Element("active-count").SetText(src.Count(x => x.Status == "active"));
```

Method names (`Where`/`Select`/…) are familiar but they live on a
non-enumerable type, so there is no collision regardless. Naming is open (see §10).

## 5. Plan domain additions — reuse first, add only what is missing

| Captured C# | Compiles to (plan node) | Reused or new |
|---|---|---|
| `x.Status` (member on element) | `read` from `PayloadSource{scope:"element"}`, path `status` | **reuse** `ReadExpression` |
| `x.Status == "active" && x.Age > 65` (predicate) | `ConditionGraph` (`all[compare(eq…), compare(gt…)]`), leaves read element scope | **reuse** `ConditionGraph` |
| `new ResidentRow(x.Name, …)` / `new { … }` (projection) | `object` `ValueExpression` over element reads | **reuse** `ObjectExpression` |
| `x.Price * x.Qty` (arithmetic) | `compute` value node (`op:"mul"`, left, right) | **NEW** — no arithmetic node today |
| `x.Age > 65` *as a projected value* | `condition-value` bridge wrapping a `ConditionGraph` | **NEW** — small bridge |
| `.Where/.Select/.Count/.Sum/.Any/.OrderBy…` | `array-op` value node (source + op + predicate/projection/key) | **NEW** — the array-op family |

### 5.1 Element scope (the one new scope)

- C#: `PayloadScope.Element` → JSON `scope:"element"` (`Source.cs`).
  `PayloadSource.Element()` factory.
- Runtime: `ExecutionContext.withElement(item)` — same immutable spread as
  `withResponse`; maintained as a **stack** so a nested op binds its own element.
  `resolvePayload` gains `case "element": return top-of-element-stack`.
- A per-element member read is then **just a normal `read`** from
  `PayloadSource{scope:"element"}` — no new read kind.

### 5.2 Array-operation node — one kind, op sub-discriminator

Mirror `CompareCondition`'s proven pattern (one `kind`, an `op` field, op-specific
payload), rather than one class per operation:

```
ArrayOperationExpression : ValueExpression   // Kind => "array-op"
  Op        : string            // "filter" | "map" | "count" | "sum" | "any" | ...
  Source    : ValueExpression   // produces the input array
  Predicate : ValidationCondition?  // SYNC subset (compare/all/any/not); Confirm excluded — see §14.2
  Projection: ValueExpression?  // map/select; sum/avg/min/max key; orderBy key
  ItemShape : Shape             // element shape, carried for the runtime
  Shape     : Shape             // declared output shape (array<TOut>, number, bool, T)
```

Runtime: one `case "array-op"` in `evaluate.ts`, which switches on `op` with its
own `assertNever` over the closed op set — exactly how `conditions.ts` switches
on `condition.op`. For element-wise ops it iterates the evaluated source, pushes
`withElement(item)`, evaluates predicate/projection against that frame.

### 5.3 Scalar computation nodes (projections / arithmetic)

```
ComputeExpression : ValueExpression          // Kind => "compute"
  Op    : string         // "add" | "sub" | "mul" | "div" | "mod" | "neg" | "concat"
  Left  : ValueExpression
  Right : ValueExpression?  // null for unary "neg"
  Shape : Shape

ConditionValueExpression : ValueExpression    // Kind => "condition-value"
  Condition : ValidationCondition  // SYNC subset (compare/all/any/not); Confirm excluded
  Shape     : Shape  // Boolean
```

`ConditionValueExpression` is the one bridge that lets a projection yield a
boolean (`Select(x => x.Age > 65)`) while keeping a **single** boolean algebra
(`ConditionGraph`). It is intentionally tiny.

## 6. The C# expression compiler (the real engineering core)

A new `ExpressionVisitor`-based translator — call it `ValueExpressionCompiler` —
that converts a captured `Expression<Func<T, …>>` into the node tree above, given
an element scope. It handles a **whitelist**:

- `ParameterExpression` (the element `x`) → element scope marker.
- `MemberExpression` chain on the element → `read` from `scope:"element"` with a
  dot/index path (reuse `ExpressionPathHelper`'s chain extraction).
- `ConstantExpression` → `literal`.
- `BinaryExpression`:
  - arithmetic (`+ - * / %`) → `compute`.
  - comparison (`== != > >= < <=`) → `CompareCondition`.
  - logical (`&& ||`) → `all`/`any` `ConditionGraph`.
- `UnaryExpression` `!` → `not`; numeric negate → `compute{neg}`.
- `ConditionalExpression` (ternary) → **throws at C# build time in v1** (deferred
  to v1.1 as a `condition-value` + select or a dedicated `case` node). The throw is
  mandatory — silently ignoring ternary would break the closed-boundary guarantee
  of §2.
- `NewExpression` / `MemberInitExpression` (projection `new {…}` / `new Row(…)`)
  → `object`.
- A small **whitelisted** `MethodCallExpression` set (e.g. `string.Contains`,
  `string.StartsWith`, `Math.Min/Max/Abs`) mapped to known nodes.

**Anything else throws at C# build time** with a precise message naming the
unsupported node — the same contract `ExpressionPathHelper.cs:122` already
enforces. This throw is the boundary that guarantees the plan only ever carries
closed, deterministic nodes.

## 7. Generated TS + runtime

1. `PlanTypeScriptContract.cs`: add `"ArrayOperationExpression"`,
   `"ComputeExpression"`, `"ConditionValueExpression"` to the `ValueExpression`
   union and declare their interfaces. Run
   `npm run generate:plan-types -w Alis.Reactive.Assets` (do not hand-edit
   `plan.ts`).
2. `evaluate.ts`: new `case`s before `assertNever`. Array-op case maintains the
   element-scope stack and delegates predicate evaluation to the existing
   conditions evaluator (shared `evaluateValue`).
3. `execution-context.ts`: `withElement(item)` + `element` scope resolution +
   the stack.
4. `WriteOnlyPolymorphicConverter`: **no change** (delegates to `GetType()`; the
   public `Kind` property is the discriminator).

## 8. Vertical slice / build sequence (closes one matrix row at a time)

Following the repo's 10-step new-primitive checklist and "one matrix row per
commit" rule. Each row: `<DSL source call> -> <domain term> -> <runtime behavior>`.

1. **Element scope** — `PayloadScope.Element`, `withElement`, `resolvePayload`
   case, runtime test. (Foundation; no DSL surface yet.)
2. **`compute` node** — arithmetic value node + `evaluate.ts` case + tests.
3. **`array-op: count`** — simplest op (no predicate). `ReactiveValue<int>` from a
   `read` array source → `SetText`. C# `VerifyJson` + vitest + Playwright +
   sandbox view.
4. **`array-op: filter`** — predicate via `ConditionGraph` + element scope. Prove
   `filter(...).Count()` composition.
5. **`array-op: map` + projection** — `object` projection reading element scope;
   `condition-value` bridge.
6. **Aggregates** — `sum`/`average`/`min`/`max`/`any`/`all`/`find`.
7. **Ordering / slicing / distinct** — `orderBy`/`thenBy`/`take`/`skip`/`distinct`.
8. **`ValueExpressionCompiler`** — the expression-tree visitor, fronting all of
   the above so the author writes natural lambdas. (Can be built incrementally:
   member-only first, then arithmetic, then method whitelist.)
9. **`groupBy`** — most complex; last.

A working slice is `count` end-to-end (steps 1-3): `.Reactive()` event delivers
`e.Data` (`Resident[]`) → `p.From(args,e=>e.Data).Count(x=>x.Status=="active")`
→ plan JSON `array-op{op:count, predicate: compare(eq, read(element,status),
"active")}` → runtime iterates, counts → `SetText` shows the number. Fully
deterministic, zero developer JS.

## 9. Decisions made (defaults, open to change)

- **D1 — Predicates reuse `ConditionGraph`.** `Where`/`Any`/`All`/`Find`/`Count(pred)`
  predicates compile to condition graphs evaluated per-element, rather than a
  parallel boolean algebra. Rationale: smallest new surface, every node maps to
  an existing DSL graph node, conditions already share `evaluateValue`.
  *Alternative:* one unified new expression tree (rejected — duplicates a tested
  algebra; violates "smallest clear set of concepts").
- **D2 — v1 operation closure (tiered), locked after the SF acceptance test.**
  - **v1 core (must-have, the SF surface demands all of these):**
    - element scope + **element-self read** (`x => x`, §14.4) — without the
      self-read, no `string[]`/`int[]` surface works.
    - `array-op`: `Where, Select, Count, Sum, Any, All, Find/First` (find/first
      carry an optional projection, §14.5), `OrderBy/OrderByDescending`.
    - `compute` (arithmetic `add/sub/mul/div/mod`, unary `neg`, string `concat`).
    - `condition-value` bridge.
    - per-component `ReactiveArray<T>` builder overloads (§14.8) — `SetDataSource`
      on Grid/MultiSelect/DropDownList/AutoComplete/MultiColumnComboBox/Schedule/
      PivotView/Kanban; `SetValue` on NativeCheckList/MultiSelect; `SelectByIndexes`
      on ChipList.
  - **v1.1 fast-follow:** `Average, Min, Max, ThenBy, Take, Skip, Distinct,
    Contains`; date/string `compute` ops (`date-diff-days`, …); member-of-scalar
    via a named-local scope.
  - **v2:** `SelectMany, GroupBy`, ternary projections, cross-level (outer)
    element access in nested ops.
- **D3 — Single-level element scope in v1.** A predicate/projection reads its own
  array's element. Nested ops (e.g. inner op referencing the outer element) are a
  documented v1 limitation; the scope is modeled as a **stack** so the extension
  (named/depth-addressed frames) is additive, not a rewrite.
- **D4 — `array-op` is one node with an `op` sub-discriminator** (mirrors
  `CompareCondition`), not one class per operation. Keeps the TS union and the
  runtime switch small.

## 10. Resolved decisions (were open questions)

1. **Builder naming — LOCKED.** Authoring surface: `ReactiveArray<T>` (array-valued)
   and `ReactiveValue<T>` (scalar-valued). Plan nodes: `ArrayOperationExpression`,
   `ComputeExpression`, `ConditionValueExpression`. (`Query` avoided to not imply
   LINQ.)
2. **First proof slice — LOCKED.** `Count() → SetText` (smallest visible proof),
   then `Where(...) → SetDataSource` on a grid.
3. **Source entry — BOTH, LOCKED.** `p.From(args, e => e.Data)` for event-payload
   arrays, AND the operators are usable as extensions on the existing
   `TypedComponentSource<T[]>` / `PayloadTypedSource<…, T[]>` so they compose with
   what authors already write (`multiSelect.Value().Count()`).
4. **Compiler method-whitelist — LOCKED (refined by JS ground-truth).**
   `string.Contains/StartsWith/EndsWith` ARE **v1 core** — they compile to the
   existing `CompareOp` operators `contains`/`starts-with`/`ends-with`
   (`evaluateTextComparison`, `conditions.ts:354-372`), so they add no new node.
   `Math.Min/Max/Abs` (which would need a compute/method node) stay v1.1. Anything
   outside the set throws at build time.

## 11. Risks

- **R1 — "rebuilding a scripting interpreter" drift.** The repo philosophy
  forbids generic interpreters. Mitigation: the node set is **closed and named**;
  the compiler **throws** on anything outside it; every op maps to a matrix row.
  If the closure starts growing toward "any expression," stop — that is the
  signal to push the case to the plugin hatch instead.
- **R2 — expression-compiler surface area.** The `ExpressionVisitor` is the
  hardest, highest-risk piece. Mitigation: build it last (step 8), after the plan
  nodes and runtime are proven via explicit construction in tests; ship
  member-only translation first, grow the whitelist behind build-time throws.
- **R3 — element-shape fidelity for object arrays.** `Shape.ArrayOf(item)` over
  `ObjectOf` must carry projected field shapes so downstream reads stay typed.
  Mitigation: derive output `Shape` in the C# builder from `TOut` at authoring
  time (the builder knows the type); covered by `VerifyJson` snapshots.
- **R4 — nested-op scope (D3).** Cross-level element access is deferred; if real
  demand appears early, the stack model already accommodates depth addressing.

## 12. Closure checklist (per repo standard)

A module (op) is done only when: the DSL source row exists in the blueprint; the
C# domain names match; generated `plan.ts` carries the concept; the runtime
executes it without defensive policy; focused C# (`VerifyJson`) + vitest +
Playwright tests prove behavior; and the slice is committed. Before
implementation, update `docs/reactive-plan-source-blueprint.md`,
`docs/reactive-plan-domain-language.md`, and
`docs/design/dsl-graph-coverage-matrix.md` with the new rows.

## 13. Strategic arc — one deterministic algebra over the whole browser object model

The array-operations DSL is not a special case. It is the first concrete instance
of a unifying principle the domain language already states
(`reactive-plan-domain-language.md`): *"Components, DOM elements, app-level
objects, plugins, event payloads, and HTTP responses are all modeled by how the
plan reads or writes those objects."* A browser object is a JS object with
properties, methods, and events. An array is a JS object. A DOM element is a JS
object. A complex SF component API is a JS object. Same shape.

The three primitives that make array element-operations work are object-agnostic,
so they generalize without a new mechanism — only new member contracts:

- **Object member read** (`ValueExpression`) — already universal via `evaluateValue`.
- **The focus scope** (`element`) — "the current object under operation": the
  array item, or the DOM node under a walk.
- **The value algebra** (`compute`, `condition-value`, `array-op`) — deterministic
  operations over reads.

The arc:

- **Phase 1 — Arrays (this proposal).** Operate on / read from elements the
  onboarded components only *bind* today. Proves element scope + value algebra.
- **Phase 2 — Compute over complex component APIs.** SF components expose rich
  members (`grid.getSelectedRecords()`, `scheduler.getEvents()`, aggregates).
  Today you can read them but cannot deterministically compute over them without a
  plugin. The same algebra closes that:
  `grid.getSelectedRecords().Count(r => r.Status == "active")` becomes a plan
  node, not JS.
- **Phase 3 — DOM as a browser object.** A DOM element gets a declarable member
  contract (`textContent`, `value`, `classList`, `dataset`, `children`,
  `checked`…), modeled exactly as components are — resolved by `getElementById`
  (NOT scanning; plan-driven IDs preserved). The value algebra then reads and
  operates on DOM members deterministically, in plan JSON, with no inline JS in
  views.

This is where the framework shines: one value algebra, one runtime interpreter,
one plan contract — covering components, arrays, and the DOM uniformly, all
deterministic, no developer JS.

### Guardrails that keep the arc working with (not fighting) the architecture

- **Plan-driven IDs, no DOM scanning (Rule 7).** DOM-as-object resolves via
  `getElementById`; the plan carries the id. No `querySelectorAll`, no traversal
  to discover objects.
- **Closed node set; build-time throw.** Each phase adds named deterministic
  nodes; the C# compiler rejects anything outside them at build time.
- **The plugin endures as the integration + orchestration boundary.** Its job is
  not "whatever the algebra can't make deterministic." It is (a) onboarding
  **3rd-party JS** libraries/objects the framework has not modeled, and (b)
  **composing several browser calls** into one named operation (imperative
  orchestration not worth modeling as plan nodes). The value algebra and the
  plugin are complementary, not competing: the algebra expresses deterministic
  reads/operations over *modeled* browser objects; the plugin integrates *foreign*
  objects and combines calls. The arc clarifies the division of labor — it does
  not push the plugin toward zero.

## 14. Caveats ironed out — surgical resolutions

Each resolution below is grounded in the two verification passes (architecture-fit:
viable, 0 blocking; SF acceptance: passes-with-extensions) and cites the constraint
it satisfies. This section is authoritative where it refines §5–§7.

### 14.1 Import graph — no cycle (the one real risk, resolved)

Today: `conditions/conditions.ts:31` imports `evaluateValue` from `core/evaluate`
(one-way); `core/evaluate.ts` imports nothing from conditions (it is a leaf);
`execution/execute.ts` imports both. Making the array-op predicate reuse condition
evaluation would otherwise force `evaluate → conditions → evaluate`.

Resolution — **dependency injection through a leaf module.** Extract the sync
condition evaluator into `conditions/sync-condition.ts` whose entry takes the
value-evaluator as a parameter:
`evaluateSyncCondition(cond, plan, ctx, evalValue): boolean`. It imports nothing
from `core/evaluate`. Then `core/evaluate.ts` (array-op case) imports
`evaluateSyncCondition` and passes its own `evaluate`; `conditions/conditions.ts`
delegates its sync subset to the same module, passing `evaluateValue`. Resulting
graph: `evaluate.ts → sync-condition.ts` (leaf); `conditions.ts →
{sync-condition.ts, evaluate.ts}`. No cycle. The single-resolver invariant holds —
all operands still resolve through the one `evaluateValue`.

### 14.2 Predicates are the SYNC condition subset — Confirm excluded

`ConditionGraph` includes `ConfirmCondition` (async → `Promise<boolean>`); a Promise
inside a synchronous `.filter()`/`.every()` loop is silently truthy → wrong results.
Resolution: an array-op predicate is typed as the **sync `ValidationCondition`
subset** (compare/all/any/not), never the full `ConditionGraph`. Naturally enforced
(a per-element lambda cannot express a user confirm) and locked at the type level.
**C# gate:** `ValidationCondition` exists today only as a generated TS union — the
C# plan model has no sync-subset base. To make the C# compiler a gate (not only the
expression-compiler whitelist), `ArrayOperationExpression.Predicate` is typed against
an internal C# sync-condition marker (a narrow base or internal factory that accepts
only `Compare`/`All`/`Any`/`Not`), so a `ConfirmCondition` cannot be assigned even by
internal builder code. `evaluateSyncCondition`
handles only compare/all/any/not with its own `assertNever`. Array ops stay 100% on
the immediate lane — confirmed compatible with `executeSequence` async promotion
(array-op evaluation is internal to a synchronous `evaluateValue` and never returns
a Promise, so it cannot trigger lane promotion).

### 14.3 Element binding lives on `ExecContext` as a STACK

`execute.ts` passes `context.raw` (the plain `ExecContext`) to `evaluateValue`,
stripping the `ExecutionContext` class wrapper — a class-only field would be
invisible to evaluation. Resolution: add `element?: readonly unknown[]` (a stack)
to the `ExecContext` interface (`types/context.ts`). `withElement(item)` returns a
new context spreading `{ ...raw, element: [...(raw.element ?? []), item] }` (same
immutable pattern as `withRequest`/`withResponse`). `resolvePayload` adds
`case "element": return values.element?.[values.element.length - 1]` (innermost /
top of stack). v1 reads the innermost element; outer-element access in nested ops is
v2 (depth-addressed) and additive on the same stack. The C# side registers the scope
in three coupled places: `PlanTerms.cs` `PayloadScope.Known` dict, `Source.cs`
`PayloadSource.Element()` factory, and the `PayloadScope` literal union in
`PlanTypeScriptContract.cs` (then regenerate `plan.ts`).

### 14.4 Element-self read for primitive arrays (`string[]`/`int[]`) — `x => x`

The decision that unlocks the most SF surfaces. `MultiSelect.Value()`,
`ChipList.SelectedChipValues()`, `NativeCheckList.Value()`, and event arrays are
`string[]`/`int[]` — the element has no member, so filter/any/map must read the
element ITSELF. Resolution: the identity lambda `x => x` (a bare
`ParameterExpression`) compiles to a **whole-element read** — a `read` from
`PayloadSource{scope:"element"}` with empty path — exactly analogous to the existing
`ReadWholePayload` / `readsWholePayload` (`evaluate.ts:136`). So
`multiSelect.Value().Any(x => x == "fall-risk")` and `checklist.Value().Count()` are
expressible.

**Sentinel mechanism (decided, per panel nit).** A `PayloadPathReadExpression`
cannot carry an empty path (`StructuredPath` is a non-empty tuple, `plan.ts:1113`).
So the element-self read is a **dedicated `WholeElementReadExpression`** — `read`
from `PayloadSource{scope:"element"}` with sentinel `member: "elementValue"` and
empty path — added to the `ReadExpression` union and declared in
`PlanTypeScriptContract.cs`, mirroring `WholePayloadReadExpression` (which uses the
`responseBody` sentinel). The runtime adds a `readsWholeElement` gate (sibling to
`readsWholePayload`) that returns the resolved element root directly, then runs
`applyShapeWhenPresent` exactly as payload reads do — so primitive element shapes
(`string`/`number`) round-trip identically to payload values.

### 14.5 `Find`/`First` carry an optional projection (folds "find then read a field")

The `Data[0].field` idiom reads a member from a scalar op result, which a plain
`read`-from-source cannot do. Resolution: `find`/`first`/`last` carry an optional
`Projection : ValueExpression?`. `arr.Find(pred).Read(x => x.Id)` compiles to a single
`find` op with `predicate` + `projection: read(element.id)`; the builder folds the
trailing `.Read(...)` into the find node's projection. Arbitrary member-of-any-scalar
beyond this fold is v1.1 (named-local scope).

### 14.6 `ValueEvaluation` evaluates per element via a child context

`ValueEvaluation` holds a fixed `this.context`; the array-op case must not mutate it.
Resolution (decided, per panel nit): per element, the array-op case constructs a
**fresh child `ValueEvaluation`** — `new ValueEvaluation(this.plan,
this.context.withElement(item))` — and evaluates predicate/projection through it.
This mirrors how the existing `array` literal case recurses with `this.evaluate`
(`evaluate.ts:48-51`) and keeps `this.context` immutable. Purely synchronous
recursion; no async promotion. (The alternative of threading raw `ExecContext` is
rejected to avoid two evaluation entry shapes.)

### 14.7 New-node `OutputShape` + projected object shape

The three new C# nodes implement the abstract `ValueExpression.OutputShape`:
`ArrayOperationExpression` → `ArrayOf(itemOrProjectedShape)` for
filter/map/orderBy/take/skip/distinct, `Number` for count/sum/avg/min/max, `Boolean`
for any/all, element-or-projected shape for find; `ComputeExpression` → its declared
numeric/string shape; `ConditionValueExpression` → `Boolean`. Because
`Shape.FromClrType(customClass)` returns `Shape.Any` (not `ObjectOf`),
`ReactiveArray<T>.Select(x => new Row(...))` builds the projected `Shape.ObjectOf(fields)`
**explicitly** from the captured projection's members/types, so array<object> outputs
keep field-level shapes for downstream typed reads.

### 14.8 Per-component builder overloads (no new plan nodes)

Binding a computed `ReactiveArray<T>`/`ReactiveValue<T>` to a component is a plain
`set`/`call`. v1 adds one builder overload per onboarded array surface
(`SetDataSource(ReactiveArray<T>)`, `SetValue(ReactiveArray<string>)`,
`SelectByIndexes(ReactiveArray<int>)`) — same `EmitSet`/`EmitCall` pattern as the
existing `SetDataSource(ResponseBody<T>, path)` overloads, zero runtime/plan-node
change.

### 14.9 `compute` scope for v1

`compute` v1 ops = `add/sub/mul/div/mod` + unary `neg` + string `concat`. Date
arithmetic (`date-diff-days`, `date-add-days`) and richer string ops are v1.1 —
additive op strings on the same node, no structural change.

### 14.10 Inventory completeness (from the SF acceptance critic)

Surfaces the inventory initially missed but which the v1 set covers once §14.4 and
§14.8 land: `FusionAIAssistViewPromptRequestArgs.PromptSuggestions` (string[]),
`FusionKanbanQueryCellInfoArgs.Data`, `FusionChipList.SetSelectedChipIndexes`
(literal overload already exists), `FusionAutoComplete.UpdateData`,
`FusionKanbanDataBindingArgs.Result`. None require new plan nodes.

### 14.11 Array-like → `Array` normalization at the array-op input boundary

Confirmed by the JS ground-truth pass against the real EJ2 typings and DOM APIs.
Real JS sources are not all `Array`: `multiSelect.value` is `string[] | null`;
`chipList.selectedChips` is a scalar `number` in single-select mode; DOM `classList`
(DOMTokenList), `children`/`selectedOptions` (HTMLCollection), `files` (FileList),
`querySelectorAll` (NodeList) are **iterable but `Array.isArray` is false**. The
array-op case normalizes at its **input boundary**, before the op sub-switch, via a
private `normalizeToArray(value, label)` in `evaluate.ts` (or a boundary-local module
imported only by evaluate.ts). Decision tree:

1. `Array.isArray(v)` → use as-is (EJ2 `dataSource`/`getEvents`/`value`; every
   intermediate array-op result is a real array).
2. `v == null` → `[]` (true boundary: `multiSelect.value` null, `files` null).
3. `typeof v === "number" | "string"` → `[v]` (scalar-to-singleton: chipList
   single-select).
4. `typeof v === "object" && Symbol.iterator in v` → `Array.from(v)` (DOMTokenList/
   HTMLCollection/FileList/NodeList; snapshots live collections at entry, which is
   the correct iteration semantics).
5. else → **throw** `[alis] array-op source is not iterable: <label>` — fail-fast
   boundary error (e.g. `DOMStringMap`/`dataset`, which has no `Symbol.iterator`).

**Why this does not corrupt the framework:** it is NOT in `shape-convert.ts:toArray`
(changing that alters scalar→singleton coercion for conditions/gather/execute — SRP).
It is NOT a plan validator or fallback — it normalizes the underdetermined union the
browser/EJ2 JS API returns, which C#'s `T[]` type cannot constrain at authoring time;
this is the same external-boundary category as `getElementById` returning null
(`runtime-path.ts:88-92 requireMember` precedent). Plan-driven IDs are preserved (the
value is already in hand via `getElementById` + member read — no scanning). **`dataset`/
DOMStringMap stays plugin**: step 5 throws rather than silently `Array.from`-ing to
`[]`; its only enumeration is `Object.entries` (a distinct bridge, not iterable
normalization), so it remains the plugin escape hatch's job (§13 division of labor).
