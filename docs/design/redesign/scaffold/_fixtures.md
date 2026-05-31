# Acceptance Fixtures — The Red of Red-Green-Refactor

> The single, source-grounded fixture catalogue for the redesign. Every row is one
> **TDD-ready acceptance test**: the exact DSL a developer writes, the exact plan-JSON
> bytes the C# domain must serialize, and the exact browser behavior the runtime must
> produce. The module skeletons in this folder fill against these rows — open a module
> spec, find its fixture ids here, type the obvious body until the fixture is green.
>
> **Provenance.** Every DSL verb below was read from the actual public source under
> `Alis.Reactive/Builders/*`, `Alis.Reactive/Razor/Extensions/*`,
> `Alis.Reactive/Validation/*`, `Alis.Reactive.Native/*`, `Alis.Reactive.Fusion/*`,
> and `Alis.Reactive/PlanModel/*` — **not** inferred from tests, the prior atlas, or
> stale docs. The cases are the union of the three determinism matrices
> ([`04-matrix-triggers-reactions-conditions.md`](../04-matrix-triggers-reactions-conditions.md),
> [`04-matrix-http-arrays-values.md`](../04-matrix-http-arrays-values.md),
> [`04-matrix-validation-components-slots.md`](../04-matrix-validation-components-slots.md)),
> which the determinism proof scores at 100% (120/120 public feature families).
> Names are from [`03-naming.md`](../03-naming.md); ownership from
> [`02-micro-modules.md`](../02-micro-modules.md).

---

## How to use this file (the TDD contract)

1. **Pick a fixture id** (e.g. `F-Trigger-PageReady`). The id is stable; the module
   skeleton's `// TODO [<id>]` markers point back here.
2. **Write the failing test from the four columns:**
   - **input** is the *arrange* — paste the DSL verbatim into a `CreatePlan()` harness.
   - **expected plan JSON** is the *assert* for the C# domain test — the bytes
     `PlanSerializer` must emit (camelCase). Assert with a JSON-shape comparison.
   - **expected browser behavior** is the *assert* for the TS runtime test (jsdom via
     `boot()`) and the Playwright slice — the user-visible outcome.
3. **Type the obvious body** in the skeleton until C# domain test + TS runtime test +
   `npm run typecheck` (drift gate) + Playwright are all green. Refactor. Commit the row.

### The lane legend (carried, never re-detected)

`SYNC` = the reaction returns `void`, same browser tick (so Syncfusion `args.cancel` is
visible). `ASYNC` = returns `Promise<void>`; the only async openers are **Request**,
**Parallel**, **Confirm** (user decision), and **remote triggers** (ServerPush/SignalR
delivery) and **partial injection**. The lane is a plan-carried fact (`ReactionLane`
stamped by `ReactionPipelineDraft`), asserted in the JSON, not probed at runtime.

### Parameterization — why ~250 fixtures stand for thousands of generated cases

Each fixture names a **lowering template** plus its **axis values**. A code generator
enumerates the finite axes and emits one deterministic case per tuple; every tuple has
exactly one lowering and one runtime reader. The axes (all finite, all from source):

| Axis | Finite domain | Fixtures that multiply on it |
|---|---|---|
| **TriggerKind** | `page-ready · document-event · component-event · server-push · signalr` | `F-Trigger-*` |
| **PayloadContract** | `untyped · typed(T)` | every trigger/dispatch payload |
| **ReactionKind** | `set · call · dispatch · branch · sequence · inject · show-validation-errors` (+ `request · parallel`) | `F-Reaction-*` |
| **TargetSource** | `component · payload · plugin(call-only)` | `set`/`call` fixtures |
| **ValueSource** | `literal · read(component\|url\|plugin\|payload\|dom) · whole-payload · whole-element · object · array` | every value slot |
| **Shape** | `string · number · boolean · date · nullable<scalar> · array<item> · object{fields} · raw · any · none` | every value/operand/member |
| **CompareOp** | the 21 tokens in 9 operand-shape families | `F-Condition-Op-*` |
| **OperandForm** | `unary · literal-binary · text-literal · array · range · min-length · collection-item · source-vs-source` | condition right operand |
| **GuardComposition** | `single · all · any · not` | `F-Condition-Guard-*` |
| **BranchPosition** | `then · else-if · else` | `F-Condition-Branch-*` |
| **P-VERB** | `GET · POST · PUT · DELETE` | `F-Request-Verb-*` |
| **P-TARGET** | `payload · header · route-param · url-query` | `F-Request-Gather-*` |
| **P-OP** | `count · filter · map · sum · any · all · find · orderBy` | `F-Array-*` |
| **RuleName** | the 18 validation tokens | `F-Validation-Rule-*` |
| **ComponentRole** | `plan-input · object-target · validation-container · layout-object` | `F-Component-*` |
| **PlanScope** | `root · partial` | `F-Slot-*` |

A generated case is `(template, axis values…)`. The matrix proof is the determinism
guarantee; this file is its concrete, assertable form.

### Shared fixture conventions

- **`<id>`** in JSON is the deterministic `IdGenerator` id, e.g. for
  `m => m.Address.City` on `Alis.Reactive.SandboxApp.Models.OrderModel` →
  `Alis_Reactive_SandboxApp_Models_OrderModel__Address_City`. Vendor-agnostic (Native and
  Fusion produce the same id for the same expression).
- All JSON is camelCase exactly as `PlanSerializer` emits. Field order is illustrative;
  the assert is a shape comparison, not byte-for-byte ordering.
- `<T.FullName>` is the CLR assembly-qualified-free full type name.
- A fixture's **C# proof** = one domain test (DSL → node + wire JSON). Its **TS proof** =
  one `boot()` jsdom test (runtime behavior). Its **browser proof** = one Playwright slice
  for user-visible rows. Pure kernel rows (Shape/Kind) prove via unit + drift-gate.

---

## Module 1 — Shape (kernel)

> CLR inference at authoring (`Shape.FromClrType` / `Shape.FromValue`), one conversion
> engine at runtime (`ShapeConverter.applyShape`/`convertByShape`), the **shape-once**
> invariant on the gather egress path. Shape rides on *every* value node; these fixtures
> pin the inference + conversion in isolation. Source: `PlanModel/Shape.cs`,
> `runtime/value/shape-convert.ts`.

| id | input (DSL / CLR type) | expected plan JSON (the shape tag) | expected browser behavior |
|---|---|---|---|
| `F-Shape-String` | a `string` value reaches any sink | `{"kind":"string"}` | `applyShape(v,string)` → `String(v)`; `null`→`null` |
| `F-Shape-Number` | an `int`/`long`/`decimal`/`double` value | `{"kind":"number"}` | coerces `"3"`→`3`; non-finite handled per op |
| `F-Shape-Boolean` | a `bool` value | `{"kind":"boolean"}` | coerces truthy/`"true"`→`true` |
| `F-Shape-Date` | a `DateTime` value | `{"kind":"date"}` | serialized round-trip-safe ISO-8601 `"O"` string |
| `F-Shape-NullableScalar` | a `int?`/`DateTime?` value | `{"kind":"nullable","inner":{"kind":"number"}}` | present→coerce inner; absent→`null` (no default) |
| `F-Shape-Array` | a `T[]`/`IEnumerable<T>` value | `{"kind":"array","item":{…itemShape}}` | each item shaped by `item`; non-array → boundary normalize |
| `F-Shape-Object` | a typed object value | `{"kind":"object","fields":{…},"additional":false}` | each field shaped; closed object (no extra keys) |
| `F-Shape-Raw` | a pre-serialized JSON value | `{"kind":"raw"}` | passed through unconverted |
| `F-Shape-Any` | an unclassifiable type (object/dynamic) | `{"kind":"any"}` | identity — never a guessed scalar |
| `F-Shape-None` | `null` literal / no-operand rule | `{"kind":"none"}` | absence, not a typed default; `""`→`null` not applied |
| `F-Shape-Once` | one value flows evaluate→gather→wire | (no second shape tag re-derived) | value is shaped **exactly once** on egress; identical bytes everywhere |

**Parameterization.** P-SHAPE has 10 finite members; one inference fixture + one
conversion fixture per member. Every value fixture in every other module reuses this
catalogue for its `shape` field — Shape is the cross-cut, asserted once here.

---

## Module 2 — Kind (kernel)

> The single C#→TS discriminator + the generated contract. `PlanNodeDiscriminator` writes
> `kind` from a compile-enforced base; `PlanContractGenerator` reflects the node families
> into `plan.ts`; `ContractDriftGate` fails the build on drift; `assertNever` proves runtime
> exhaustiveness. These fixtures are build-gate + serialization proofs, not browser rows.
> Source: `PlanModel/*` (`Kind` props), `runtime/core/assert-never.ts`, the generator.

| id | input | expected output | expected behavior |
|---|---|---|---|
| `F-Kind-Discriminator` | any sealed plan node serialized | the node carries its `"kind":"<token>"` written by `PlanNodeDiscriminator` (one mechanism, no per-type hand converter) | every polymorphic node round-trips with its discriminator |
| `F-Kind-Generated-Contract` | the C# node families reflected | `plan.ts` discriminated unions match the C# `kind` tokens 1:1 | `npm run typecheck` passes against generated `plan.ts` |
| `F-Kind-Drift-Gate` | rename/add a C# node property without regenerating | `ContractDriftGate` **fails the build** | a renamed property cannot silently disagree with the runtime |
| `F-Kind-AssertNever` | a runtime `switch` over a `kind` union | `assertNever(node, "<context>")` on the default arm | adding a new `kind` is a compile error until handled |

**Parameterization.** One fixture per node family is unnecessary — the generator and drift
gate prove all families at once. These four fixtures pin the mechanism; every other module's
JSON row is implicitly a Kind round-trip proof.

---

## Module 3 — Value

> One write path (`TypedSource<T>` → one `ValueExpression` variant), one read path
> (`evaluateValue` + `ArrayOpEngine`). Flat 5-variant family: `Literal · Read · ObjectValue
> · ArrayValue · ArrayOp`. A `Read` carries a `Source` (one of six) + an `access`
> (`property`|`method`). Source: matrix Part A; `PlanModel/ValueExpression.cs`,
> `runtime/value/evaluate.ts`. (Part A counts **15**: 3 literals + 10 reads + 2 composites.)

### Literals

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Value-Literal-Scalar` | `.SetText("hello")` / `.Eq(5)` / `.Static("k",12.5m)` | `{"kind":"literal","value":<v>,"shape":{"kind":"string\|number\|boolean\|date"}}` | yields the constant, shape-coerced; DateTime→ISO `"O"` |
| `F-Value-Literal-Null` | `Static("k", null)` | `{"kind":"literal","value":null,"shape":{"kind":"none"}}` | yields `null`; `""`→`null` not re-applied (already null) |
| `F-Value-Literal-Arbitrary` | `ArgValue(enumOrGuidOrObj)` | `{"kind":"literal","value":<json>,"shape":<inferred>}` | STJ-serialized at render; `any` when unclassifiable |

### Reads (parameterized over P-SOURCE × access)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Value-Read-ComponentProperty` | `p.Component<T>(m=>m.X).Value()` | `{"kind":"read","from":{"kind":"component","component":"<id>"},"member":"value","path":[],"shape":<TProp>,"access":{"kind":"property"}}` | reads live component member via `getElementById`+`ComponentDriver` |
| `F-Value-Read-ComponentMethod` | typed method source `schedule.GetEvents()` | `{…,"access":{"kind":"method","args":[<ValueExpression>…]}}` | `fn.apply(object, args.map(evaluate))`; return shape from `TReturn` |
| `F-Value-Read-PluginMethod` | `plugin.GetThing().Arg("a")` | `{"kind":"read","from":{"kind":"plugin","name":"<n>","type":"plugin.<n>"},"member":"<m>","access":{"kind":"method","args":[…]},"shape":<T>}` | `PluginCatalog` resolves host instance (throws if unknown), `.call` |
| `F-Value-Read-PluginProperty` | `p.PluginProperty<bool>("net","online")` | `{…,"from":{"kind":"plugin",…},"access":{"kind":"property"}}` | reads plugin property; shape from `T` |
| `F-Value-Read-UrlUntyped` | `p.FromUrl("page")` | `{"kind":"read","from":{"kind":"url"},"member":"page","path":[],"shape":{"kind":"string"},"access":{"kind":"property"}}` | `URLSearchParams.get("page")`; default String |
| `F-Value-Read-UrlTyped` | `p.FromUrl<int>("page")` | `{…,"shape":{"kind":"number"}}` | read + coerce `"3"`→`3`; non-scalar `T` rejected at authoring |
| `F-Value-Read-Payload` | `json.Read(r=>r.Data.Name)` / `FromEvent(...)` | `{"kind":"read","from":{"kind":"payload","scope":"success","type":{…}},"member":"data.name","path":[{"kind":"property","name":"data"},{"kind":"property","name":"name"}],"shape":<TProp>,"access":{"kind":"property"}}` | walk `path` on the scope's payload object |
| `F-Value-Read-WholePayload` | whole-body read after a request | `{"kind":"read","from":{"kind":"payload","scope":"success"},"whole":true}` | returns the entire scope payload unwalked (real variant, not `"responseBody"` sentinel) |
| `F-Value-Read-WholeElement` | identity `x=>x` in an array op over primitives | `{"kind":"read","from":{"kind":"payload","scope":"element"},"whole":true}` | returns the current element itself (real variant, not `"elementValue"` sentinel) |
| `F-Value-Read-Dom` | `p.FromDom("card","classList")` | `{"kind":"read","from":{"kind":"dom","element":"card"},"member":"classList","path":[{"kind":"property","name":"classList"}],"shape":<>,"access":{"kind":"property"}}` | `getElementById("card")` (boundary throw if null), read member |

### Composites

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Value-Object` | `b.Set(x=>x.A, src).Set(x=>x.B,"lit")` | `{"kind":"object","fields":{"a":<ValueExpr>,"b":{"kind":"literal",…}},"shape":{"kind":"object","fields":{…},"additional":false}}` | each field evaluated → JS object; leaf-vs-parent name conflict throws at authoring |
| `F-Value-Array` | items composed into an array value | `{"kind":"array","items":[<ValueExpr>…],"shape":{"kind":"array","item":{…}}}` | items evaluated in order → JS array; shared item shape when homogeneous, else `array<any>` |

**Parameterization.** Values scale on **P-SOURCE (×6) × P-SHAPE (×10)** over one `Read`
template + one `Literal` template + `Object`/`Array`. One row per source kind ⇒ ~60 read
combinations from a single template — the matrix is the generator spec.

---

## Module 4 — Condition

> `When/Confirm` author a `ConditionGraph` (Compare/All/Any/Not/Confirm); `.Then/.ElseIf/
> .Else` route into `BranchCase`s on a `branch` `ReactionGraph`; first match wins. ONE
> `CompareEngine` for both lanes. Source: matrix Condition band; `Builders/Conditions/*`,
> `PlanModel/{ConditionGraph,CompareOp}.cs`, `runtime/conditions/*`. Every compare node is
> `{"kind":"compare","left":…,"op":"<token>","right":…,"shape":…,"itemShape":…}`.

### Condition source — the left operand (parameterized over SourceKind)

| id | input (DSL) | expected plan JSON (left operand) | expected browser behavior |
|---|---|---|---|
| `F-Condition-Source-Component` | `p.When(p.Component<X>(m=>m.Care).Value(c=>c.Level))` | `"left":{"kind":"read","from":{"kind":"component","component":"<id>"},"member":"level","shape":…}` | reads live component member; shape from `TProp` |
| `F-Condition-Source-Url` | `p.When(p.FromUrl<int>("page")).Gt(1)` | `"left":{"kind":"read","from":{"kind":"url"},"member":"page",…}` | typed `FromUrl<T>` sets shape Number |
| `F-Condition-Source-Plugin` | `p.When(p.PluginProperty<bool>("net","online"))` | `"left":{"kind":"read","from":{"kind":"plugin",…}}` | reads plugin member |
| `F-Condition-Source-EventPayload` | `p.When(args, x=>x.Total)` | `"left":{"kind":"read","from":{"kind":"payload","scope":"event"},"member":"…","path":{…}}` | reads from trigger payload |
| `F-Condition-Source-ResponseBody` | `p.When(success, x=>x.Status)` | `"left":{…,"from":{"kind":"payload","scope":"success"}…}` | reads from awaited success body |

### Compare operators — 9 families, all 21 tokens (one fixture per token)

| id | input (DSL) | expected op + right operand JSON | expected browser behavior |
|---|---|---|---|
| `F-Condition-Op-Truthy` | `.Truthy()` | `"op":"truthy","right":{"kind":"none"}` | `!!shaped(left)` |
| `F-Condition-Op-Falsy` | `.Falsy()` | `"op":"falsy","right":{"kind":"none"}` | `!shaped(left)` |
| `F-Condition-Op-IsNull` | `.IsNull()` | `"op":"is-null","right":{"kind":"none"}` | raw `left == null` |
| `F-Condition-Op-NotNull` | `.NotNull()` | `"op":"not-null","right":{"kind":"none"}` | raw `left != null` |
| `F-Condition-Op-IsEmpty` | `.IsEmpty()` | `"op":"is-empty","right":{"kind":"none"}` | `""`, missing, or `[]` |
| `F-Condition-Op-NotEmpty` | `.NotEmpty()` | `"op":"not-empty","right":{"kind":"none"}` | negation of is-empty |
| `F-Condition-Op-Eq` | `.Eq(5)` | `"op":"eq","right":{"kind":"value","value":{literal}}` | `shaped(left) === shaped(right)` |
| `F-Condition-Op-Neq` | `.NotEq(5)` | `"op":"neq","right":{"kind":"value","value":{literal}}` | negation of eq |
| `F-Condition-Op-Gt` | `.Gt(3)` | `"op":"gt","right":{value:{literal}}` | ordered compare; type mismatch ⇒ `false` (no throw) |
| `F-Condition-Op-Gte` | `.Gte(3)` | `"op":"gte",…` | ordered `>=` |
| `F-Condition-Op-Lt` | `.Lt(3)` | `"op":"lt",…` | ordered `<` |
| `F-Condition-Op-Lte` | `.Lte(3)` | `"op":"lte",…` | ordered `<=` |
| `F-Condition-Op-In` | `.In("a","b")` | `"op":"in","right":{value:{array}}` | `array.includes(shaped(left))` |
| `F-Condition-Op-NotIn` | `.NotIn("a","b")` | `"op":"not-in","right":{value:{array}}` | negation of in |
| `F-Condition-Op-Between` | `.Between(1,10)` | `"op":"between","right":{value:{array[2]}}` | inclusive `lo ≤ left ≤ hi`; un-orderable ⇒ `false` |
| `F-Condition-Op-Contains` | `.Contains("xyz")` | `"op":"contains","right":{value:{textLiteral}}` | string predicate on `toString(left)`; non-text ⇒ `false` |
| `F-Condition-Op-StartsWith` | `.StartsWith("A")` | `"op":"starts-with",…` | string prefix; non-text ⇒ `false` |
| `F-Condition-Op-EndsWith` | `.EndsWith("z")` | `"op":"ends-with",…` | string suffix; non-text ⇒ `false` |
| `F-Condition-Op-Matches` | `.Matches("^A")` | `"op":"matches","right":{value:{textLiteral}}` | `new RegExp(pattern).test(text(left))`; non-text ⇒ `false` |
| `F-Condition-Op-MinLength` | `.MinLength(3)` | `"op":"min-length","right":{value:{numericLiteral}}` | `text(left).length >= n`; non-text ⇒ `false` |
| `F-Condition-Op-ArrayContains` | `.ArrayContains(item)` | `"op":"array-contains","right":{value:{literal}},"itemShape":<set>` | shape each item by `itemShape`, `items.includes(item)`; non-array ⇒ `false` |
| `F-Condition-Op-SourceVsSource` | `.Eq(otherSource)` / `.Gt(otherSource)` | same family; `right.value` is a `read` `ValueExpression` not a `literal` | compare against live source value (one extra OperandForm axis, no new family) |

### Guard composition (And / Or / Not)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Condition-Guard-Single` | `p.When(s).Eq(1)` | the bare `compare` node | one predicate, no wrapper |
| `F-Condition-Guard-AndChain` | `p.When(s).Eq(1).And(s2).Gt(0)` | `{"kind":"all","terms":[compare,compare]}` | short-circuits to `false` on first false; flattened (no nested `all`) |
| `F-Condition-Guard-OrChain` | `…Eq(1).Or(s2).Eq(2)` | `{"kind":"any","terms":[…]}` | short-circuits to `true` on first true; flattened |
| `F-Condition-Guard-AndGroup` | `.And(inner => inner.When(s).Gt(0))` | `{"kind":"all","terms":[…inner terms…]}` | nested group flattened into one `all` |
| `F-Condition-Guard-OrGroup` | `.Or(inner => …)` | `{"kind":"any","terms":[…]}` | nested group flattened into one `any` |
| `F-Condition-Guard-Not` | `p.When(s).Eq(1).Not()` | `{"kind":"not","term":{compare}}` | inverts the single child |

### Branch routing — first-match → a `branch` `ReactionGraph`

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Condition-Branch-Then` | `p.When(s).Eq(1).Then(p2=>…)` | `{"kind":"branch","cases":[{"guard":{"kind":"when","condition":…},"reaction":…}]}` | reaction runs only if guard true. SYNC |
| `F-Condition-Branch-ElseIf` | `.ElseIf(s).Gt(0).Then(p3=>…)` | `cases:[when…, when…]` | top-to-bottom, first match wins, rest skipped. SYNC |
| `F-Condition-Branch-Else` | `.Else(p4=>…)` | `cases:[…, {"guard":{"kind":"default"},"reaction":…}]` | runs only if no prior case matched; default always last, only one. SYNC |
| `F-Condition-Branch-NoMatch` | guards all false, no `Else` | runtime no-op (logs `branch.no-match`) | nothing runs; silent no-op (not an error). SYNC |
| `F-Condition-Branch-Standalone-Unrepresentable` | `When(s).Eq(1)` with **no** `.Then` | (compile error — `standalone` continuation exposes no `Then`) | a compile error replaces today's runtime throw |

### Confirm guard (the async opener in this band)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Condition-Confirm-Then` | `p.Confirm("Delete?").Then(p2=>…)` | `{"kind":"branch","cases":[{"guard":{"kind":"when","condition":{"kind":"confirm","message":"Delete?"}},…}]}` | shows confirm dialog; reaction runs only on accept. **ASYNC**; missing dialog throws (real boundary) |
| `F-Condition-Confirm-InComposition` | `p.Confirm("Sure?")` then `.And(s).Gt(0)` | `{"kind":"all","terms":[{confirm},{compare}]}` | compares may short-circuit before the dialog. **ASYNC if confirm reached** |

**Parameterization.** A Condition row = `(SourceKind × CompareFamily × OperandForm ×
GuardComposition × BranchPosition × Continuation)`. The 21 tokens map to a
`ConditionSourceBuilder` method + a `<Family>CompareCondition` wire interface + a
`CompareEngine` arm — all fixed by the family; source kind only changes `left`, operand
form only changes `right`, composition only changes the wrapper, branch position only
changes the `BranchCase` factory. No tuple has two outputs.

---

## Module 5 — Reaction

> `PipelineBuilder` emits `ReactionGraph` nodes; `ReactionPipelineDraft` sequences and
> **stamps the `ReactionLane`**; `executeReaction` routes on `kind` + carried lane via
> `switch`+`assertNever`. Source: matrix Reaction band; `Builders/PipelineBuilder*.cs`,
> `ElementBuilder.cs`, `DispatchPayloadBuilder.cs`, `PlanModel/ReactionGraph.cs`,
> `runtime/execution/{execute,inject}.ts`. **19 fixtures.**

### Sequencing + lane

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Reaction-Single` | `p.Element("s").Show()` | the bare node (no `sequence` wrapper) | runs it. SYNC |
| `F-Reaction-OrderedSync` | `p.Element("a").Show(); p.Dispatch("x")` | `{"kind":"sequence","steps":[set…,dispatch…]}` | top-to-bottom, same tick. SYNC |
| `F-Reaction-SyncAsyncSync` | `p.Element(..).Show(); p.Get(..)…; p.Element(..).Hide()` | `sequence` of `[sequence(sync), request, sequence(sync)]` | sync block, await request, then trailing sync. **ASYNC from request onward** |

### `set` reactions (TargetSource × ValueSource)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Reaction-Set-ElementShow` | `p.Element("box").Show()` | `{"kind":"set","on":{"kind":"component","component":"box"},"property":"hidden","value":{"kind":"literal","value":false,"shape":{…bool}}}` | element visible. SYNC. (`Hide`=`hidden:true`) |
| `F-Reaction-Set-TextLiteral` | `p.Element("s").SetText("hi")` | `{…,"property":"text","value":{"kind":"literal","value":"hi","shape":{string}}}` | `textContent="hi"`. SYNC |
| `F-Reaction-Set-TextFromSource` | `p.Element("s").SetText(p.FromUrl("q"))` | `{…,"property":"text","value":{"kind":"read","from":{"kind":"url"},"member":"q",…}}` | text = URL param. SYNC |
| `F-Reaction-Set-Html` | `p.Element("s").SetHtml(src)` | `{…,"property":"html",…}` | `innerHTML`=value. SYNC |
| `F-Reaction-Set-ComponentProperty` | `p.Component<X>(m=>m.Field).Set(c=>c.Enabled,true)` | `{"kind":"set","on":{"kind":"component","component":"<id>"},"property":"enabled","value":{literal true}}` | vendor property updated. SYNC |
| `F-Reaction-Set-EventArg` | inside `.Reactive`, `p` sets `args.cancel` | `{"kind":"set","on":{"kind":"payload","scope":"event","type":…},"property":"cancel","value":{literal true}}` | SF reads `args.cancel` after callback — **must be SYNC** |

### `call` reactions (TargetSource × ValueSource)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Reaction-Call-ElementCss` | `p.Element("b").AddClass("on")` | `{"kind":"call","on":{"kind":"component","component":"b"},"method":"addClass","args":[{literal "on"}]}` | class added. SYNC. (`RemoveClass`/`ToggleClass` analogous) |
| `F-Reaction-Call-MethodNoArg` | `p.Component<Grid>(…).Call(g=>g.Refresh())` | `{…,"method":"refresh","args":[]}` | vendor method invoked. SYNC. Empty args=`[]` |
| `F-Reaction-Call-MethodArgs` | `…Call(g=>g.SelectRow(2))` | `{…,"args":[{literal 2}]}` | invoked with evaluated args (each one `ValueExpression`). SYNC |
| `F-Reaction-Call-PluginCommand` | `p.Plugin("url","push").Arg(...).Fire()` | `{"kind":"call","on":{"kind":"plugin","name":"url","type":…},"method":"push","args":[…]}` | plugin operation runs. SYNC (call-only target) |
| `F-Reaction-Call-EventArgMethod` | inside `.Reactive`, `args.UpdateData(...)` | `{"kind":"call","on":{"kind":"payload","scope":"event"},"method":"updateData","args":[…]}` | arg method invoked in-tick. SYNC |

### `dispatch` reactions

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Reaction-Dispatch-NoPayload` | `p.Dispatch("saved")` | `{"kind":"dispatch","event":"saved","payload":{"kind":"none"}}` | `document.dispatchEvent(new CustomEvent("saved",{detail:{}}))`. SYNC |
| `F-Reaction-Dispatch-Literal` | `p.Dispatch("saved", new Msg{Id=1})` | `{…,"payload":{"kind":"value","data":{"kind":"literal","value":{…},"shape":…},"payloadType":{typed}}}` | detail = the literal object. SYNC |
| `F-Reaction-Dispatch-SourceObject` | `p.DispatchWith<Msg>("saved", b=>b.Set(x=>x.Total, src))` | `{…,"payload":{"kind":"value","data":{"kind":"object","fields":{"total":{read…}}},"payloadType":{typed}}}` | detail assembled from live sources. SYNC |

### `inject` + `show-validation-errors` (fixed-shape verbs)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Reaction-Inject` | `p.Get("/card").Into("card-host")` | `{"kind":"inject","slot":"card-host","value":{"kind":"read","from":{"kind":"payload","scope":"success"},"whole":true}}` | element `innerHTML` = the HTTP success body. SYNC (value is **always** `ReadWholePayload(Success)` — only the element id is supplied) |
| `F-Reaction-ShowValidationErrors` | `p.ValidationErrors("resident-form")` | `{"kind":"show-validation-errors","container":"resident-form"}` | accumulated validation errors rendered in the container (server errors after a failed request, else current client-rule results). SYNC. Container id required |

**Parameterization.** A Reaction row = `(ReactionKind × TargetSource × ValueSource)`.
`set`/`call` enumerate `TargetSource ∈ {component, payload, plugin(call-only)}` × the 9
ValueSource variants; `dispatch` enumerates `{none, literal, object}`; `inject` and
`show-validation-errors` are fixed-shape (no value/target axis). The generator emits the C#
verb + `ReactionGraph.<Kind>` node + wire interface + `executeReaction` case — fixed by
`ReactionKind`; `TargetSource` only selects the `executeSet`/`executeCall` inner arm.

---

## Module 6 — Request

> The only async network lane: `Get/Post/Put/Delete` + Gather (`target <- value`) + Response
> success/error + Chained + Parallel + WhileLoading/Finally/Validate. `http` pipeline:
> `gather → httpFetch → route → finally → chain`, all `await`ed. Source: matrix Part B;
> `Builders/Requests/*`, `runtime/network/*`. **28 fixtures.**

### Verbs (P-VERB)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-Verb-Get` | `p.Get("/api/x")` | `{…"method":"GET","url":"/api/x"…}` | `fetch(url+?query,{method:"GET"})`, no body (query-string writer) |
| `F-Request-Verb-Post` | `p.Post("/api/x")` | `{…"method":"POST"…}` | `fetch(url,{method,body})`; JSON body sets `Content-Type: application/json`; body only when it has fields |
| `F-Request-Verb-Put` | `p.Put("/api/x")` | `{…"method":"PUT"…}` | body writer |
| `F-Request-Verb-Delete` | `p.Delete("/api/x")` | `{…"method":"DELETE"…}` | body writer |
| `F-Request-Verb-InlineGather` | `p.Post(url, g => g.Include(...))` | identical to POST + a gather node | pure sugar = `.Post(url).Gather(gather)` |

### Endpoint / URL template

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-UrlTemplate` | `p.Get("/residents/{id}")` + route param | `"url":"/residents/{id}"` carried verbatim | `{id}` replaced by resolved route-param before fetch; every `{placeholder}` must be supplied (authoring error if not) |

### Gather (P-TARGET × value source)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-Gather-PayloadComponentExpr` | `g.Include<TComp,TModel>(m => m.Name)` | `{"kind":"gather","assignments":[{"target":{"kind":"payload","name":"name","path":[…]},"source":{"kind":"read",…}}],"bodyFormat":"json","registeredInputs":{"kind":"explicit"}}` | read component → write body/query by verb; param name = property name |
| `F-Request-Gather-PayloadComponentSource` | `g.Include(schedule.CurrentView())` | one payload assignment per source | param = member name unless overridden |
| `F-Request-Gather-PayloadStatic` | `g.Static("token","abc")` | `…"source":{"kind":"literal",…}` | constant written to target |
| `F-Request-Gather-PayloadFromEvent` | `g.FromEvent(args, e => e.Id, "id")` | `…"source":{"kind":"read","from":{"kind":"payload","scope":"event"},…}` | read trigger payload → write; shape from event-arg type |
| `F-Request-Gather-PayloadFromUrl` | `g.FromUrl("page")` / `g.FromUrl<int>("page","p")` | payload assignment, source = `url` read | param doubles as payload key unless `asParam`; default String, typed coerces |
| `F-Request-Gather-PayloadPlugin` | `g.Plugin(pluginSource, "name")` | payload assignment, source = plugin `Invoke` read | — |
| `F-Request-Gather-HeaderLiteral` | `g.Header("X-Key","v")` | `{"target":{"kind":"header","name":"X-Key"},"source":{"kind":"literal",…}}` | `formatForWire`→`toString` (must be scalar); non-null literal required |
| `F-Request-Gather-HeaderSource` | `g.Header("X-Key", src)` / `g.Header("X-Key", args, e=>e.X)` | header assignment, scalar source | missing value → header **omitted** (not `""`); scalar-only enforced at authoring |
| `F-Request-Gather-RouteParamStatic` | `g.RouteParam("id", 5)` | `{"target":{"kind":"route-param","name":"id"},"source":{"kind":"literal",…}}` | substitutes URL; non-null literal |
| `F-Request-Gather-RouteParamSource` | `g.RouteParam("id", src)` | route-param assignment, scalar source | null route param **throws** (`cannot build URL` — real boundary) |
| `F-Request-Gather-IncludeAll` | `g.IncludeAll()` | `…"registeredInputs":{"kind":"all-registered-inputs"}` | every **mounted** registered input's value written to body (unmounted skipped) |
| `F-Request-Gather-None` | `p.Delete("/x/{id}")` with only a route param | `…"input":{"kind":"none"}` | no body; route param still substitutes URL (`none` strategy, not `{}`) |

### Body egress (the writer)

| id | input → resolved value | expected wire form | expected browser behavior |
|---|---|---|---|
| `F-Request-Body-ScalarJson` | scalar, JSON, non-GET | `{"<path>":<value>}`; `""`→`null` | nested `assignJsonBodyValue` by path; cleared field → `null` |
| `F-Request-Body-ArrayJson` | array value | `{"<name>":[<item>…]}` | each item `formatForWire(itemShape)`; `File` items rejected |
| `F-Request-Body-QueryString` | any value, GET | `?name=a&name=b` | `encodeURIComponent`; arrays repeat key; `File` in GET → throw |
| `F-Request-Body-FormData` | any value, `AsFormData` | `FormData` entries | `File`/`{rawFile}` appended with filename; `Content-Type` left to browser |

### Response routes (success / error)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-Response-OnSuccess` | `r => r.OnSuccess(s => {…})` | `"success":[{"match":{"kind":"any"},"reaction":{…}}]` | on `response.ok`, run reaction in success scope; match=`any` when no status |
| `F-Request-Response-OnSuccessTyped` | `r => r.OnSuccess<R>((json,s)=>…)` | success route + reads with `from.scope="success"`, typed contract | body parsed → success scope; typed paths compile-time |
| `F-Request-Response-OnError` | `r.OnError(...)` / `OnError(404,...)` / `OnError<E>(...)` | `"error":[{"match":{"kind":"status","status":404},…}]` | on `!ok`, match status then any; first match wins |
| `F-Request-Response-Unavailable` | (no DSL — network failure path) | only the any-status error route runs (no body) | runs any-error reaction; never a success route; finally still runs |

### Loading / finally / validate

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-WhileLoading` | `.WhileLoading(p => p.Element("spinner").Show())` | `"whileLoading":[{…}]` | spinner shown **before** the request sends (awaited before fetch); replaces prior block |
| `F-Request-Finally` | `.Finally(p => p.Element("spinner").Hide())` | `"finally":[{…}]` | always runs after routing in `try/finally` (incl. network failure); no body access |
| `F-Request-Validate` | `.Validate<V>("resident-form")` | `"validation":{"kind":"container","container":"resident-form"}` | client rules run; failure shows errors and **the request never sends** |

### Chained / parallel

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Request-Chained` | `r.Chained(req => req.Get("/next/{id}").Gather(g => g.RouteParam("id", json.Read(...))))` | `"chain":{"kind":"follow-up","next":{…full request…}}` | next request runs **only after success**, can gather from prior body; terminal (`{"kind":"terminal"}`) when no `Chained` |
| `F-Request-Parallel` | `p.Parallel(b1=>b1.Get(...), b2=>b2.Post(...)).OnAllSettled(p=>…)` | a `parallel` reaction node carrying branch requests + `onAllSettled` reaction | branches fire concurrently (`Promise.all`); completion runs once **all** settle; no completion if `OnAllSettled` not called |

**Parameterization.** HTTP gather scales on **P-TARGET (×4) × (any value source, ×15) ×
P-VERB (×4) × body format**; response routes scale on **status code × scope**
(any-status + N exact statuses × success/error), one route template, first-match. The
request graph is fully decided at authoring; the runtime walks a fixed pipeline.

---

## Module 7 — Trigger

> `Html.On(plan, t => …)` → one `Behavior` (`StartsWhen` + `ReactionGraph`); `wireTrigger`
> attaches one listener per `StartsWhen.kind` and feeds the payload into one
> `ExecutionContext`. Source: matrix Trigger band; `Builders/TriggerBuilder.cs`,
> `PlanModel/StartsWhen.cs`, `runtime/execution/{trigger,server-push,signalr}.ts`.
> **10 fixtures.** (Component-event is authored via `.Reactive()`, not on `TriggerBuilder`.)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Trigger-PageReady` | `t.DomReady(p => …)` | `{"startsWhen":{"kind":"page-ready"},"reaction":…}` | `readyState==="loading"` → run on `DOMContentLoaded`, else immediately; empty `ExecutionContext`. SYNC. Fire once |
| `F-Trigger-CustomEventUntyped` | `t.CustomEvent("ready", p => …)` | `{"kind":"document-event","event":"ready","payloadType":{"kind":"untyped"}}` | `addEventListener("ready")`; every dispatch runs with `event(detail ?? event)`. SYNC |
| `F-Trigger-CustomEventTyped` | `t.CustomEvent<OrderReady>("ready",(e,p)=>…)` | `{"kind":"document-event","event":"ready","payloadType":{"kind":"typed","type":"<OrderReady.FullName>"}}` | same listener; `e` is phantom (shape only) enabling typed payload-path reads. SYNC |
| `F-Trigger-ComponentEvent` | `…NativeTextBox(b=>b.Reactive(plan, evt=>evt.Changed,(args,p)=>…))` | `{"kind":"component-event","component":"<id>","event":"changed"}` | vendor event fires reaction with `event(eventData)`. **SYNC** so SF `args.cancel` is visible |
| `F-Trigger-ServerPushAny` | `t.ServerPush("/sse", p => …)` | `{"kind":"server-push","url":"/sse","eventFilter":{"kind":"any","payloadType":{"kind":"untyped"}}}` | `EventSource` per url, channel `"message"`, abort-scoped. **ASYNC opener** |
| `F-Trigger-ServerPushNamed` | `t.ServerPush("/sse","tick", p=>…)` | `{…,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"untyped"}}}` | only `tick` SSE events fire the reaction |
| `F-Trigger-ServerPushNamedTyped` | `t.ServerPush<Vitals>("/sse","tick",(e,p)=>…)` | `{…,"eventFilter":{"kind":"named","event":"tick","payloadType":{"kind":"typed","type":"<Vitals.FullName>"}}}` | typed payload contract on the filter |
| `F-Trigger-SignalR` | `t.SignalR("/hub","OnTick",p=>…)` | `{"kind":"signalr","hubUrl":"/hub","method":"OnTick","payloadType":{"kind":"untyped"}}` | hub connection per url, `connection.on("OnTick")`, abort-scoped. **ASYNC opener** |
| `F-Trigger-SignalRTyped` | `t.SignalR<Vitals>("/hub","OnTick",(e,p)=>…)` | `{…,"payloadType":{"kind":"typed","type":"<Vitals.FullName>"}}` | typed contract |
| `F-Trigger-Multiple` | `t.DomReady(…).CustomEvent("x",…)` | `behaviors:[Behavior_1, Behavior_2]` | each chained call wires independently; author order = wire order; no implicit sharing |

**Parameterization.** A Trigger row = `(TriggerKind × PayloadContract)`: 5 kinds ×
`{untyped, typed}` (page-ready has no payload axis; component-event has no typed authoring
overload). The payload axis only toggles `payloadType` presence/shape — never a separate
template. The generator emits the `StartsWhen.<Kind>` factory + `TriggerBuilder` overload +
generated wire interface + `wireTrigger` switch arm, fixed by the kind.

---

## Module 8 — Component

> One id regime (`IdGenerator`) + one node family (`BrowserObject` with a `ComponentRole`).
> Role is the discriminator: `plan-input · object-target · validation-container ·
> layout-object`. Source: matrix Band B; `Razor/Extensions/InputFieldExtensions.cs`,
> `Builders/Requests/GatherExtensions.cs`, the Native/Fusion slices, `runtime/resolution/*`.

### B1 — Input field rendering + registration (the join)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Component-InputNative` | `Html.InputField(plan, m => m.Name, o => o.Required().Label("Name")).NativeTextBox(b => b.Placeholder("…"))` | `components[id] = { id, vendor:"native", type:"textbox", role:"plan-input", binding:{kind:"registered-input", bindingPath, valueMember:"value", path:[...]} }` + the input's `BrowserObjectContract` under `types` | a labeled, validation-ready text input; resolved by `getElementById(id)` |
| `F-Component-InputFusion` | `Html.InputField(plan, m => m.Country).FusionDropDownList(b => …)` | `components[id] = { …, vendor:"fusion", type:"dropdownlist", role:"plan-input", binding:{…} }` | model-bound SF dropdown; **same `IdGenerator` id as Native for the same expression** |
| `F-Component-UnregisteredRender` | `Html.InputField(plan, m => m.X)` with no component extension, then `RenderPlan` | no plan emitted; render **throws** with a clear message (authoring boundary) | caught server-side; fail fast (not a runtime fallback) |

### B2 — Component mutation + read (ComponentRef, both vendors)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Component-PropertySet` | `p.Component<FusionDropDownList>(m => m.Country).SetValue("US")` | `{"kind":"set","on":{component id},"property":"value","value":{literal},"lane":"sync"}` | the SF dropdown's value changes. Member from the slice's typed `ComponentProperty` |
| `F-Component-MethodCall` | `…SetDataSource(...).DataBind()` / `drawer.Open()` | `{"kind":"call","on":{component id},"method":"dataBind"\|mapped-path,"args":[...],"lane":"sync"}` | method runs on the resolved object; mapped vs canonical name explicit in contract |
| `F-Component-SetFromSource` | `…SetDataSource(args, a=>a.Items)` / `(body, r=>r.Rows)` / `(reactiveArray.AsSource())` | `set` node with `value:{kind:"read",source:{payload\|response},path:[...]}` or `{kind:"arrayOp"/"array"}` | data source updates from the live value (all sources via one Value spine) |
| `F-Component-Read` | `p.When(p.Component<FusionDropDownList>(m=>m.Country).Value()).Eq("US")…` | `{"kind":"read","on":{component id},"member":"value","shape":"string"}` | live value feeds a condition/gather (same node a condition/gather/plugin-arg consumes) |

### B3 — Component event wiring (`.Reactive()`)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Component-Event` | `Html.FusionGrid<M,Row>(plan,"grid",b=>…).Reactive(e => e.DataStateChange, (args, p) => { … })` | `components["grid"] = {…, role:"object-target"}` + `behavior {startsWhen:{kind:"componentEvent","component":"grid","event":"dataStateChange"},reaction:{…}}` | SF event fires the pipeline with typed `args` |
| `F-Component-InputEvent` | `Html.InputField(plan,m=>m.Country).FusionDropDownList(b => b.Reactive(e => e.Change, (args,p)=>…))` | the existing `plan-input` `BrowserObject` gains a `change` event behavior (role unchanged) | changing the field fires the pipeline |

### B4 — Fusion Grid (display component, full surface)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Component-GridRender` | `Html.FusionGrid<M,Row>(plan,"residents-grid", b => { b.Column(...); })` | Grid HTML; `components["residents-grid"]` appears as `object-target` **only if** referenced/wired | a rendered data grid; no label/validation wrapper, no `plan-input` role |
| `F-Component-GridDataStateChange` | `.Reactive(e => e.DataStateChange, (args, p) => p.Post(...).Gather(...).OnSuccess(...))` | `behavior` with `componentEvent:"dataStateChange"` → `request` reaction (**lane=async** on the request node) | sort/page/filter triggers a fetch and re-renders rows |
| `F-Component-GridMutation` | `p.Component<FusionGrid>("residents-grid").Refresh()` | `{"kind":"call","target":"residents-grid","method":"refresh","lane":"sync"}` | grid refreshes (object-target). SYNC |
| `F-Component-GridValidation` | grid edit + `FusionGridValidation` rules | `ValidationRuleNode`s under the grid container `BrowserObject` (Band A node family) | edited cell shows validation; reuses Band A's one validation node family |

**Parameterization.** ~60 slices = 51 Fusion + 9 Native + 4 app-level. B1 covers the
**input** subset (slices with a `ValueMember`) — one `plan-input` `BrowserObject` whose only
per-component deltas are `vendor`/`type`/`valueMember`/`Shape`. Display/container slices
(no `ValueMember`) reuse B2 (members → set/call/read) and B3 (events → componentEvent). The
vendor seam is **only** `ComponentDriver` + `wireFusionEvent`/`wireNativeEvent`.

---

## Module 9 — Slot

> Two axes: SSR join by `PlanId` (server) and browser injection by `SlotId`. `PlanScope`
> (`root`|`partial`) is the discriminator. The **only** browser-injection authoring verb is
> `p.Into(elementId)` — no `InjectInto`, no `p.Slot(...)` in source. Source: matrix Band C;
> `PlanModel/PlanScope`, `Razor/Extensions/PlanExtensions.cs`, `runtime/lifecycle/boot.ts`
> (`loadPartialSlot`/`unloadPartialSlot`), `runtime/execution/inject.ts`. **5 fixtures.**

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Slot-Root` | `@{ var plan = Html.ReactivePlan<M>(); } … @Html.RenderPlan(plan)` | `<script id="alis-plan-{sanitized planId}" data-reactive-plan>{version:3, planId, scope:{kind:"root"}, types, components, behaviors}</script>` | plan boots; one root plan per model per page |
| `F-Slot-PartialSameModel` | partial: `@{ var plan = Html.ResolvePlan<M>(); } … @Html.RenderPlan(plan)` (same `TModel`) | a second `<script>` with the **same** `planId` | at boot, `composeInitialPlans` merges same-`planId` scripts (merge types, merge components, append behaviors) into one active plan; `MergePolicy` shared with C# |
| `F-Slot-PartialIndependent` | partial: own `Html.ReactivePlan<TOther>()` | a `<script>` with a **different** `planId`, `scope:{kind:"root"}` | booted independently; two plans coexist |
| `F-Slot-BrowserLoad` | `p.Get(url).Into(slot)` where success body carries `<script data-reactive-plan>` | `{"kind":"inject","slot":"<id>","value":{ReadWholePayload(success)}}` | `injectHtml` extracts embedded plan scripts, sets `innerHTML`, calls `loadPartialSlot(slot, plans)`; `recompose` builds a **new** `PlanDocument` (boot snapshot + slots); wires under the slot's `AbortController`. Slot replaces components by key, appends behaviors |
| `F-Slot-BrowserUnload` | `p.Get(url).Into(slot)` whose success body carries **no** plan scripts (or host calls `unloadPartialSlot(slot)`) | the active `PlanDocument` reverts to boot + remaining slots | `injectHtml` finds zero plans → `unloadPartialSlot(slot)` aborts the slot's controller (drops slot-owned listeners/validation), `recompose` rebuilds; boot/app-level objects stay mounted |

**Parameterization.** `{root, partial} × {SSR-join, browser-inject} × {load, unload}`.
Browser-inject load **and** unload are both driven by the one verb `p.Into(elementId)`; the
runtime decides load vs unload from whether the injected HTML carries plan scripts.
`recompose` always builds a fresh document (snapshot-safe); `MergePolicy` is one shared
replace-vs-append rule.

---

## Module 10 — Validation

> Client rules recorded through `ReactiveValidator<T>`/DI at render time, run inline/summary
> in the browser; server stays authority. Each rule → a `ValidationRuleNode` with one
> `RuleName`, one `RuleOperand` (`none`/`constraint`/`peer`), an activation, a comparison
> `Shape`. `WhenField` reuses Condition's `CompareEngine`. Source: matrix Band A;
> `Validation/ClientValidationFieldRuleBuilder.cs`, `Validation/*`, `runtime/validation/*`.

### A1 — the 18 rule types (one fixture per `RuleName`)

> Common node shape: `{ name:"<token>", message:"...", execution:{ kind:"none"|"constraint"|"peer", value?:{…}, activation:{kind:"always"|"when",…}, comparisonShape:{…} } }`.
> Browser default for all: **empty field passes** every rule except `Required` (Required owns
> emptiness; `gt` also treats empty as failing per source).

| id | input (DSL) | expected `name` + `execution.kind` | expected browser behavior |
|---|---|---|---|
| `F-Validation-Rule-Required` | `ClientRule(m=>m.Name).Required("Name is required")` | `name:"required"`, `kind:"none"`, `comparisonShape:{none}` | empty field shows message in `{id}_error`; non-empty clears it |
| `F-Validation-Rule-Empty` | `.Empty("must be empty")` | `name:"empty"`, `kind:"none"` | non-empty value fails |
| `F-Validation-Rule-Email` | `.Email("Enter a valid email")` | `name:"email"`, `kind:"none"` | non-empty malformed email fails; empty passes |
| `F-Validation-Rule-Url` | `.Url("bad url")` | `name:"url"`, `kind:"none"` | non-empty malformed url fails; empty passes |
| `F-Validation-Rule-CreditCard` | `.CreditCard("bad card")` | `name:"creditCard"`, `kind:"none"` | non-empty invalid card fails; empty passes |
| `F-Validation-Rule-AtLeastOne` | `.AtLeastOne("pick one")` | `name:"atLeastOne"`, `kind:"none"` | multi-value subject with no selection fails |
| `F-Validation-Rule-MinLength` | `.MinLength(2,"Too short")` | `name:"minLength"`, `kind:"constraint"`, `value:{literal 2}` | non-empty shorter than bound fails; empty passes |
| `F-Validation-Rule-MaxLength` | `.MaxLength(10,"Too long")` | `name:"maxLength"`, `kind:"constraint"`, `value:{literal 10}` | non-empty longer than bound fails; empty passes |
| `F-Validation-Rule-Regex` | `.Regex("^[A-Z]+$","Caps only")` | `name:"regex"`, `kind:"constraint"`, `value:{literal string}` | non-empty non-matching fails; bad pattern fails closed |
| `F-Validation-Rule-Range` | `.Range(1,10,"1–10")` | `name:"range"`, `kind:"constraint"`, `value:{array[2]}` | value outside `[lo,hi]` inclusive fails; empty passes |
| `F-Validation-Rule-ExclusiveRange` | `.ExclusiveRange(1,10,"1–10")` | `name:"exclusiveRange"`, `kind:"constraint"`, `value:{array[2]}` | value outside `(lo,hi)` exclusive fails |
| `F-Validation-Rule-Min` | `.Min(18,"18+")` | `name:"min"`, `kind:"constraint"`, `value:{literal}` | below min fails; empty passes |
| `F-Validation-Rule-Max` | `.Max(65,"under 65")` | `name:"max"`, `kind:"constraint"` | above max fails; empty passes |
| `F-Validation-Rule-Gte` | `.GreaterThanOrEqualTo(18,"18+")` | `name:"min"` (Gte aliases to min) | below fails; empty passes |
| `F-Validation-Rule-Lte` | `.LessThanOrEqualTo(65,"under")` | `name:"max"` (Lte aliases to max) | above fails; empty passes |
| `F-Validation-Rule-Gt` | `.GreaterThan(0,">0")` | `name:"gt"`, `kind:"constraint"` | strict; **empty OR not-greater fails** (matches source) |
| `F-Validation-Rule-Lt` | `.LessThan(100,"<100")` | `name:"lt"`, `kind:"constraint"` | strict; non-empty not-less fails |
| `F-Validation-Rule-EqualTo` | `.EqualTo("yes","must be yes")` | `name:"equalTo"`, `kind:"constraint"` | non-equal fails; empty passes |
| `F-Validation-Rule-NotEqual` | `.NotEqual("no","cannot be no")` | `name:"notEqual"`, `kind:"constraint"` | non-empty equal-to-forbidden fails; empty passes |

### A2 — peer-field comparison rules

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Validation-Peer-EqualTo` | `.EqualTo(m => m.ConfirmPassword,"must match")` | `name:"equalTo"`, `execution:{kind:"peer", value:{kind:"read",source:component/binding,path:[...]}, comparisonShape}` | field compared to live peer value (same Value spine) |
| `F-Validation-Peer-NotEqualTo` | `.NotEqualTo(m => m.Other,"must differ")` | `name:"notEqual"`, `kind:"peer"` | non-empty equal to peer fails |
| `F-Validation-Peer-GreaterThan` | `.GreaterThan(m => m.Start,"after start")` | `name:"gt"`, `kind:"peer"`, `value:{read}` | ordered compare against live peer |
| `F-Validation-Peer-Gte` | `.GreaterThanOrEqualTo(m => m.Start,"≥ start")` | `name:"min"` (Gte→min), `kind:"peer"` | ordered `>=` against peer |
| `F-Validation-Peer-LessThan` | `.LessThan(m => m.End,"before end")` | `name:"lt"`, `kind:"peer"` | ordered `<` against peer |
| `F-Validation-Peer-Lte` | `.LessThanOrEqualTo(m => m.End,"≤ end")` | `name:"max"` (Lte→max), `kind:"peer"` | ordered `<=` against peer |

### A3 — conditional activation (`WhenField` family — reuses CompareEngine)

> All enclose `RuleActivation.When(fieldCondition)` over any A1/A2 rule; the rule's
> `execution.activation` = `{kind:"when", condition:{ConditionGraph}}`.

| id | input (DSL) | expected `activation.condition` | expected browser behavior |
|---|---|---|---|
| `F-Validation-WhenField-Truthy` | `WhenField(m => m.IsMember, () => { ClientRule(m=>m.Card).Required("..."); })` | a `compare` with `op:"truthy"` on the guard field | enclosed rule runs only when guard truthy; unmounted guard → rule skipped (stays valid) |
| `F-Validation-WhenField-Eq` | `WhenField(m => m.Country, "US", () => {...})` | `{kind:"compare",op:"eq",left:{read field},right:{literal},shape}` | rules active only when field == value |
| `F-Validation-WhenField-CompareOp` | `WhenFieldGt(m => m.Age, 18, () => {...})` (and `WhenFieldNot/Neq/Gte/Lt/Lte/Null/NotNull/Empty/NotEmpty/In/NotIn/Between/Contains/StartsWith/EndsWith/Matches/MinLength`) | one `compare` node with the matching `op` token + operand + `shape` | enclosed rules active only when the field condition holds (one `WhenFieldX` ↔ one `CompareOp`) |
| `F-Validation-WhenField-ArrayContains` | `WhenFieldArrayContains(m => m.Tags, "vip", () => {...})` | `{kind:"compare",op:"arrayContains",left:{read array field},right:{literal}}` | rules active when the array field contains the value |
| `F-Validation-WhenFields-Composed` | `WhenFields(c => c.Field(m=>m.A).Gt(1).And(c.Field(m=>m.B).Eq("x")), () => {...})` | nested `{kind:"all"\|"any"\|"not", …}` over `compare` leaves | rules active only when the composed predicate holds (`And`→all, `Or`→any, `Not`→not) |

### A4 — collection validation, server errors, display surfaces

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Validation-RuleEach` | `ClientRuleEach(m => m.Lines).Required(...)` (on `TItem`) | one `ValidationRuleNode` per item field, keyed by the item's deterministic component id; `serverFieldName` carries `Lines[i].Field` | each item field validates independently inline (real `CollectionItemBinding`, never string arithmetic) |
| `F-Validation-NestedChild` | `ClientRule(m => m.Address, new AddressValidator())` | child `ValidationRuleNode`s with `Address.X` paths + parent's active activation | nested fields validate as one form (deterministic prefix-join) |
| `F-Validation-ServerErrors` | `p.Post(...).OnError(e => e.ValidationErrors("resident-form"))`; server lands `{errors:{field:[msg]}}` | `{"kind":"show-validation-errors","container":"resident-form"}` (the Reaction-band node), resolved against the build-time `serverFieldName` map | server errors show on the matching field, or fall to the summary; no runtime heuristics |
| `F-Validation-InlineDisplay` | automatic via `Html.InputField` | generated `<span id="{forId}_error" data-valmsg-for="{binding}">` (not a plan component) | message appears beside the field; `{id}_error` from one `ErrorElementNaming` constant |
| `F-Validation-SummaryFallback` | `@Html.RenderPlan(plan)` when `plan.RendersValidationSummary` | generated `<div data-reactive-validation-summary="{planId}" hidden>` keyed `{planId}_validation_summary` | hidden-field/server errors collect in the summary; div shown only when it has errors |

**Parameterization.** A1 = 18 `RuleName` × 3 operand-execution variants; A2 = the
equality+ordered subset with `RuleOperand.PeerField`; A3 = one `RuleActivation.When` over a
`ConditionGraph` from the shared 21-token `CompareOp` list, nesting over **any** A1/A2 rule
(`{rules} × {activations}`, no new node kinds); A4 covers collection/server/display.

---

## Module 11 — Plugin

> The typed escape hatch: declare a browser object the DSL does not model, then read a
> property / call an operation through the same Value + object-member spines. One `Plugin`
> declaration; one args-builder-first `PluginMemberBuilder`. Stringly names allowed **only**
> at the plugin name/member boundary; args stay typed. Source: matrix Band D;
> `Builders/PluginReadBuilder.cs`, `PluginCallBuilder.cs`, `runtime/value/plugin-catalog.ts`.

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Plugin-Declare` | `class UrlApi : Plugin { public UrlApi():base("urlApi"){ Param = Property<string>("param"); Push = Command<string>("push"); } }` | `types["plugin.urlApi"] = BrowserObjectContract { properties:[{name:"param",shape:"string"}], methods:[{name:"push",args:["string"],returns:"none"}] }` | contract advertised; instance resolved from `PluginCatalog`. `Function`=returns value, `Command`=void, root member is `"root"` |
| `F-Plugin-Read` | `p.When(plugin.Param.Read()).Eq("x")…` / `plugin.GetThing().Arg("a").Arg(source)` | `{"kind":"read","from":{"kind":"plugin","name":"urlApi","member":"param"\|operation,"args":[...]},"shape":…}` | reads the plugin member; value feeds condition/gather/set; unknown plugin throws at the catalog boundary |
| `F-Plugin-Call` | `p.Plugin("url","push").Arg("/path").Fire()` | `{"kind":"call","on":{"kind":"plugin","name":"url"},"method":"push","args":[...],"lane":"sync"}` | the plugin operation runs. SYNC; `.Fire()` is the command terminal |
| `F-Plugin-ArgFromSource` | `plugin.Compute().Arg(p.Component<...>(m=>m.X).Value()).Arg(body, r=>r.Id)` | each `Arg(...)` is a `ValueExpression` in the node's `args[]` | plugin invoked with live values (args via one Value path, no plugin-specific resolver) |

**Parameterization.** A plugin contract = its declared members (properties + functions +
commands) × declared arg shapes. Each member is one `BrowserObjectContract` entry; each
read/call is one node addressing one member with typed args. The arity-0..3 × member/root ×
function/command overload explosion collapses to one args-builder; generation walks
`members × {read, call}`. `Arg` overloads: `string·int·bool·long·decimal·double·DateTime`,
`Arg(TypedSource)`, `Arg(event/response path)`, `ArgValue<T>`.

---

## Module 12 — Plan (spine) + App-Level Objects

> `PlanBuildContext` authoring sink → immutable `PlanDocument` (version=3) → serialize →
> `root` discovery → `boot` with `ActivePlan` passed explicitly. App-level objects (Drawer,
> Loader, Confirm, Toast, ActionLink) are ordinary `BrowserObject`s with `role:"layout-object"`
> and a **fixed id constant**, not runtime globals; they stay mounted across slot unload.
> Source: matrix Band E + Slot Band C; `Razor/Extensions/PlanExtensions.cs`,
> `Alis.Reactive.Native/AppLevel/*`, `Alis.Reactive.Fusion/AppLevel/*`, `runtime/lifecycle/{root,boot}.ts`.

### Plan document spine

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-Plan-Document` | `Html.ReactivePlan<M>()` … `@Html.RenderPlan(plan)` | `{version:3, planId:"<M.FullName>", scope:{kind:"root"}, types:{…}, components:{…}, behaviors:[…]}` inside `<script id="alis-plan-{sanitized planId}" data-reactive-plan type="application/json">` | document is immutable, serialized once; id-sanitization is the one shared rule |
| `F-Plan-Discover-Boot` | (runtime: any rendered plan on the page) | (no new JSON — boot path) | `root` discovers `[data-reactive-plan]` scripts, `composeInitialPlans` joins by `planId`, `boot` wires each composed plan passing `ActivePlan` **explicitly** to `executeReaction` (no hidden singleton, no `reset*ForTests`) |

### App-level objects (role:"layout-object", fixed id constant)

> Fixed ids **as they exist in source**: `NativeDrawer.ElementId = "alis-drawer"`,
> `NativeLoader.ElementId = "alis-loader"`, `FusionConfirm.ElementId = "alisConfirmDialog"`,
> `FusionToast.ElementId = "alisFusionToast"`. (See the drift note at the end — the matrix
> Band E text wrote `alis-fusion-confirm`/`alis-fusion-toast`; the **source constant is the
> requirement**, and the redesign's "one shared id constant" must adopt the source value.)

| id | input (DSL) | expected plan JSON | expected browser behavior |
|---|---|---|---|
| `F-App-Drawer` | layout `@Html.NativeDrawer()`; pipeline `drawer.Open()` / `.Close()` / `.SetSize(DrawerSize.Lg)` | fixed-id `layout-object` `BrowserObject` + `call` nodes `{target:{id:"alis-drawer"}, method:"classList.add", args:["alis-drawer--visible"], lane:"sync"}` | drawer slides in/out, resizes; size maps to one of 3 fixed classes. SYNC |
| `F-App-Loader` | `loader.Show()` / `.Hide()` (NativeLoader) | `layout-object` + `call` nodes on `"alis-loader"` | global loading overlay shows/hides. SYNC (commonly driven by `WhileLoading`) |
| `F-App-Confirm` | layout `@Html.FusionConfirmDialog()`; pipeline `confirm.SetContent("Sure?").Show()` / `.Hide()` | `layout-object` (vendor:fusion) on `"alisConfirmDialog"` + `set`(content) + `call`(dataBind/show/hide) nodes | a confirm dialog with content shows/hides via `ComponentDriver(fusion)`. Also usable as a Condition guard (`confirmThenEvaluate`, async) |
| `F-App-Toast` | `toast.SetContent("Saved").Success()` / `.Show()` (FusionToast) | `layout-object` (vendor:fusion) on `"alisFusionToast"` + `call` node with literal args | a toast appears with type/position; enum args lower to literals with inferred shape. SYNC |
| `F-App-ActionLink` | `@Html.NativeActionLink(...)` driving a single `p.Get/Post` | `object-target`/`layout-object` `BrowserObject` + one `request` behavior | clicking the link runs exactly one request (analyzer enforces single request) |

**Parameterization.** Plan is the aggregate root — `F-Plan-Document` covers any model.
App-level objects are parameterized over `{object} × {members}` like Band B, with two fixed
deltas: `role:"layout-object"` and a fixed id constant instead of `IdGenerator`. The fixed id
must be **one shared constant** (C# const ↔ TS module) so `target` and `getElementById` agree.

---

## Fixture census

| Module | Fixtures | Notes |
|---|---|---|
| Shape (kernel) | 11 | P-SHAPE ×10 + shape-once |
| Kind (kernel) | 4 | discriminator, generated contract, drift gate, assertNever |
| Value | 15 | 3 literals + 10 reads + 2 composites |
| Condition | 40 | 5 source + 22 ops (21 tokens + source-vs-source) + 6 guard + 5 branch (incl. standalone-unrepresentable) + 2 confirm |
| Reaction | 19 | 3 sequencing + 6 set + 5 call + 3 dispatch + inject + show-validation-errors |
| Request | 31 | 5 verb + 1 template + 12 gather + 4 body + 4 response + 3 loading/finally/validate + 2 chained/parallel |
| Trigger | 10 | 5 kinds × payload axis (+ component-event + multiple) |
| Component | 13 | B1 ×3 + B2 ×4 + B3 ×2 + B4 ×4 |
| Slot | 5 | root, same-model partial, independent partial, browser load, browser unload |
| Validation | 35 | A1 ×18 + A2 ×6 + A3 ×5 (collapsing the WhenFieldX family) + A4 ×5 + the InlineDisplay/Summary rows |
| Plugin | 4 | declare, read, call, arg-from-source |
| Plan + App-level | 7 | 2 plan spine + 5 app-level objects |
| **Total** | **194** | distinct named fixture ids (verified by id-grep over table rows) |

> **On the Validation count.** The matrix scores Validation A3 as "~22 `WhenFieldX` forms +
> `WhenFields`". This catalogue collapses the per-`CompareOp` `WhenFieldX` forms into one
> parameterized row (`F-Validation-WhenField-CompareOp`) because they are the same lowering
> template over the shared 21-token `CompareOp` list — the same way Condition's compare
> tokens are 21 distinct fixtures but the `WhenField` activation reuses them. Counting each
> `WhenFieldX` token distinctly (as the matrix does) raises Validation to ~56 and the grand
> total to **~227 named cases**. Either way, the parameterization axes multiply each row to
> the thousands the determinism proof guarantees.

**194 distinct named fixture ids** (~227 if every `WhenFieldX` token is counted distinctly),
grouped by the 12 modules, each with `id · input · expected plan JSON · expected browser
behavior`, each parameterized so the set scales to thousands of generated cases from a finite
set of lowering templates × finite axes.
