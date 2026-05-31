# Determinism Proof — Per-Variant Certification of the Redesign Target

## 1. The determinism criterion (what is being certified)

This document certifies the **redesign blueprint** — the system the matrix files
describe and we will build — **not the current shipped source**. Those are two
different artifacts and the distinction is load-bearing: current source carries
known bugs (Section 2) that the redesign deliberately fixes. Certifying "current
source is non-deterministic" is true but irrelevant; the question is whether the
**redesign we will build** is deterministic.

The criterion has four parts. The redesign is **deterministic** when:

1. **One output per live variant.** Every **live** public-DSL authoring variant
   (one per overload / token / node factory that has a real authoring producer in
   source) lowers to **exactly one** plan-JSON shape and **exactly one** browser
   behavior — no public overload produces two outputs.
2. **Current bugs resolved by a stated rule.** Every place where current source is
   non-deterministic is documented as a current-source bug AND paired with the
   explicit redesign rule that makes the bug **unrepresentable** — labeled
   `RESOLVED-BY-REDESIGN`.
3. **Dead surface excluded, not invented.** Public types with **zero authoring
   producers** (verified by `grep`, not assumed) are **deleted** in the redesign and
   **excluded from the live denominator** — they are not counted as "covered" by
   inventing a feature, and they are not counted as "uncovered" gaps.
4. **The proof is the per-variant authority.** Coverage is counted at the
   **per-overload** granularity a code generator must emit, not at a coarse
   band-family rollup that hides overloads. Every method maps to a rowed family.

The matrix files
(`04-matrix-triggers-reactions-conditions.md`, `04-matrix-http-arrays-values.md`,
`04-matrix-validation-components-slots.md`) are the per-row source-grounded spec;
**this document is the per-variant census and the determinism certificate over them.**

**Headline (earned, per-variant, against the redesign target):**
**375 / 375 = 100%** of **live** public authoring variants have a deterministic
dedicated redesign row, each with exactly one lowering and one reader. The **3**
dead public enums (`DrawerPosition`, `ToastType`, `ToastPosition` — zero `.cs`
consumers) are **deleted** in the redesign and excluded from the live denominator
(Section 4). The single live non-determinism in current source (the
`responseBody`/`elementValue` magic-member collision) is **RESOLVED-BY-REDESIGN**
by Fix 1 (Section 3): whole-payload/whole-element become distinct node *kinds*, so
the collision becomes structurally impossible.

---

## 2. Why per-variant, not per-band

A band rollup answers "is this *feature family* deterministic?" A generator answers
"how many *methods* do I emit, and what is each one's single lowering?" Those are
different questions and the second is load-bearing. Three examples from source where
one band hid multiple overloads:

| Band cell (old count = 1) | Real overloads a generator must emit | Source |
|---|---|---|
| "set text from value source" | **4** — literal, event-payload-path, **response-body-path**, typed-source | `ElementBuilder.cs:55,65,76,87` |
| "set html from value source" | **3** — literal, event-payload-path, typed-source (**no response-body overload**) | `ElementBuilder.cs:96,106,116` |
| "Sum projection" | **3** — `Sum(int)`, `Sum(decimal)`, `Sum(double)` | `ReactiveArray.cs:90,94,98` |
| "route-param from value" | **5** — `int`, `string`, `long`, typed-source, event-arg | `GatherBuilder.cs:103,111,123,131,141` |
| "EqualTo / peer comparison" | **38** static `ReactiveClientRules` overloads incl. nullable-struct peers | `ReactiveClientRuleBuilder.cs:83-356` |

The band view called each of those one covered cell. The generator must write 4, 3,
3, 5, and 38 cases respectively. This document counts the latter. (The discarded
"120/120 = 100%" headline counted band families and so could not be wrong about
per-overload lowerings — because it never enumerated them.)

---

## 3. Current-source non-determinism, and the redesign rule that resolves it

The redesign target is certified deterministic only if every current-source
non-determinism is closed by a stated, structural rule. There is exactly **one**
live many-to-one collision in current source; it is documented here and
`RESOLVED-BY-REDESIGN`.

### Fix 1 — `responseBody` / `elementValue` collision → distinct node KINDS  ·  RESOLVED-BY-REDESIGN

**The current-source bug (verified, present in shipped source):**

- Whole-payload and whole-element reads are encoded as the **magic member strings**
  `member:"responseBody"` and `member:"elementValue"`, with `path` forced to
  `Path.None`:
  `private const string WholePayloadMember = "responseBody";` /
  `WholeElementMember = "elementValue";`
  (`Alis.Reactive/PlanModel/ValueExpression.cs:379-380`), stamped by
  `ForWholePayload`/`ForWholeElement` at `:399-403`.
- The generated TS contract ships the sentinel with **no `whole` field** — the read
  intent rides entirely on the reserved member string:
  `WholePayloadReadExpression { kind:"read"; …; member:"responseBody"; path:EmptyPath; … }`
  and `WholeElementReadExpression { … member:"elementValue"; … }`
  (`runtime/types/plan.ts:783-799`).
- The runtime discriminator checks **only** the member string and **ignores `path`**:
  `readsWholePayload` returns `expression.member === "responseBody"`;
  `readsWholeElement` returns `expression.member === "elementValue"`
  (`runtime/core/evaluate.ts:294-300`). When either matches, `readFromPayload`
  returns the entire `root` **unwalked** (`evaluate.ts:287-291`).
- A legal public-DSL path read of a response/event/element property **literally named
  `ResponseBody`** camelCases to exactly `responseBody`: `CamelCase` lowercases only
  the first char (`Alis.Reactive/ExpressionPathHelper.cs:272-276`,
  `char.ToLowerInvariant(name[0]) + name.Substring(1)`).

**Consequence (the collision):** two distinct DSL inputs — (a) the framework's
whole-payload read and (b) a path read of a property named `ResponseBody` — produce
the **same wire member** and trigger the **same runtime behavior** (return the whole
object unwalked), so the field read silently returns the entire payload instead of
the `.ResponseBody` sub-field. No analyzer or build-time guard rejects this. This is
a genuine many-to-one input collision = non-determinism **in current source**.

**The redesign rule (deterministic, because a `kind` cannot collide with a
camelCased property path):**

- `WholePayload` and `WholeElement` become **distinct `ValueExpression` node kinds**
  the **Value** module owns: `kind:"whole-payload"` / `kind:"whole-element"`. The
  runtime routes on `kind` (one switch arm), **never** on a `member` string. The
  whole-payload node carries no member at all; its meaning is the node kind.
- **A DSL property literally named `ResponseBody` lowers to an ordinary
  `Read(member)`** — `member:"responseBody"`, `path` walked normally — which is a
  **different node kind** (`kind:"read"`) from the whole-payload node
  (`kind:"whole-payload"`). The two are now structurally distinct nodes, so they can
  never share an output.
- **The authoring layer never emits the reserved member string for a whole read.**
  Whole reads only ever arise from `Into`/identity producers, which emit the
  `whole-payload`/`whole-element` node directly; the `Read(member)` path is the only
  thing that can carry `member:"responseBody"`, and it always walks the path. The
  collision is therefore **unrepresentable** — there is no input that produces a
  whole read encoded as a member string.

Redesign wire shapes (both matrix files agree):
`{ "kind":"whole-payload","from":{"kind":"payload","scope":"success"} }` and
`{ "kind":"whole-element","from":{"kind":"payload","scope":"element"} }`
(`04-matrix-http-arrays-values.md:93-94,109,118-126`;
`04-matrix-triggers-reactions-conditions.md:207-230,373-378`). This is the headline
determinism win; the `inject` and condition rows depend on it.

> **Status against current source:** the magic-member sentinel is still present in
> shipped source (`ValueExpression.cs:379-380`, `plan.ts:783-799`,
> `evaluate.ts:294-300`). The redesign rule above makes it unrepresentable. This
> proof certifies the **redesign target**, in which the collision cannot occur.

### Three supporting design rules the per-variant determinism relies on

These are not collisions; they close under-pinned defaults and a drift surface so
every covered row's single lowering is fully specified.

**Fix 2 — `Literal — Shape.FromValue` full table** (`Shape.cs:70-118`). The arbitrary
literal row's inferred shape is the **complete** `Shape.FromClrType` table, so one
CLR type maps to one Shape with no per-stage re-derivation (SHAPE-ONCE):

| CLR type | Shape | Source |
|---|---|---|
| `string` | `String` | `Shape.cs:78` |
| `bool` | `Boolean` | `:79` |
| `DateTime` / `DateTimeOffset` / `DateOnly` | `Date` | `:80` → `IsDateType:99-104` |
| numeric (`byte`…`decimal`) | `Number` | `:81` → `IsNumericType:106-111` |
| `Guid` / `TimeSpan` / `TimeOnly` | `String` | `:82` → `IsStringSerializedType:113-118` |
| `enum` | `String` | `:83` |
| `Nullable<T>` | `Nullable(FromClrType(T))` | `:74-76` |
| collection | `ArrayOf(item)` | `:85-86` |
| unclassifiable | `Any` | `:88` |
| `null` value | `None` | `FromValue:96-97` |

The matrix literal row (`04-matrix-http-arrays-values.md:77`) previously summarized
only "enum/Guid → string; collection → array; else any" — materially incomplete
(`DateTime→Date`, `TimeSpan/TimeOnly→String`, `Nullable<T>→Nullable` unlisted). This
full table is the authoritative lowering the generator emits.

**Fix 3 — array → JSON egress obeys shape-once** (`request-payload-writer.ts:221-225`).
Current `jsonArrayBodyValue` shapes items **only when `itemShape.isDeclared`**
(`if (!itemShape.isDeclared) return items;` at `:222`). Redesign: the array-value
`Shape` is `array<itemShape>` derived at authoring from the element type
(`ReactiveArray<T>` carries `Shape.FromClrType(T)`,
`04-matrix-http-arrays-values.md:283`), so `itemShape.isDeclared` is **always true**
for a typed array source — the `!isDeclared` early-return is unreachable for
framework-produced arrays, and every item is shaped exactly once on egress.

**Fix 4 — app-level fixed ids as ONE shared constant** (`NativeDrawer.cs:20`,
`NativeLoader.cs:18`, `FusionConfirm.cs:12`, `FusionToast.cs:12` vs hardcoded TS
`drawer.ts:14`, `loader.ts:42`). Redesign: one C# const projected to TS per object,
with one casing convention, so the plan `target` and DOM `getElementById` agree by
construction (`04-matrix-validation-components-slots.md:370-378`).

---

## 4. Deleted dead surface (excluded from the live denominator)

These are public types with **zero `.cs` consumers** — verified by
`grep -rwn "<Name>" --include="*.cs" .`, which returns nothing outside each type's
own definition file. They have no authoring producer, the redesign **deletes** them,
and they are **excluded from the live denominator**. They are neither "covered" (that
would require inventing a feature) nor "uncovered gaps" (they should not exist).

| Deleted dead enum | Definition | Why dead (verified) |
|---|---|---|
| `DrawerPosition` | `Alis.Reactive.Native/AppLevel/NativeDrawer/DrawerPosition.cs:6` | No `.cs` consumer; no `NativeDrawer` method takes it |
| `ToastType` | `Alis.Reactive.Fusion/AppLevel/FusionToast/ToastType.cs:6` | No `.cs` consumer; Toast type methods are **parameterless** (`FusionToastExtensions.cs:68-86`: `Success() => EmitSet(CssClassProperty, Literal("e-toast-success"))`) |
| `ToastPosition` | `Alis.Reactive.Fusion/AppLevel/FusionToast/ToastPosition.cs:6` | No `.cs` consumer; no Toast position overload exists |

There is **no** `Show(msg, ToastType, ToastPosition)` signature in source; the earlier
Band-E `p.Toast(ToastType,…)` verb was a hallucination, corrected in the matrix
(`04-matrix-validation-components-slots.md:347,354`). The only app-level access entry
is the no-arg `Component<TComponent>()` (`PipelineBuilder.cs:136`) plus ComponentRef
extensions.

---

## 5. Per-area variant census (covered / total, with file:line anchors)

Every count below was read from the actual builder overloads. A variant is
**covered** only when a dedicated, source-grounded matrix row pins its single
lowering; redesign-target rows are labeled, and each current-vs-redesign difference
is written `current: X (file:line) → redesign: Y (deterministic, because …)`.

### Triggers — `TriggerBuilder.cs` (8 public methods) + `.Reactive()` seam + multiple-triggers

`grep -nE "public " Alis.Reactive/Builders/TriggerBuilder.cs` = 8 public methods
(the 9th `public` line is the class declaration). The trigger band is **10**: those
8 methods + the component-event seam + chained multiple-triggers.

| Variant | Source | Covered? |
|---|---|---|
| `DomReady` | `TriggerBuilder.cs:26` | ✅ |
| `CustomEvent` (untyped) | `:38` | ✅ |
| `CustomEvent<T>` (typed) | `:51` | ✅ |
| `ServerPush(url)` | `:67` | ✅ |
| `ServerPush(url, evt)` | `:80` | ✅ |
| `ServerPush<T>(url, evt)` | `:94` | ✅ |
| `SignalR(hub, method)` | `:111` | ✅ |
| `SignalR<T>(hub, method)` | `:126` | ✅ |
| component event via `.Reactive(...)` | `ComponentEventOnboarding.Wire` | ✅ |
| chained multiple triggers | `AddBehaviors` per call `:138` | ✅ |

**Triggers: 10 / 10.** (Band header `04-matrix-triggers-reactions-conditions.md:393`
also reads **10**: page-ready; custom-event untyped/typed; component-event;
server-push any/named/named-typed; signalr untyped/typed; multiple-triggers.)

### Reactions — sequencing + `set`/`call`/`dispatch`/`inject`/`show-validation-errors`

| Variant | Source | Covered? |
|---|---|---|
| single command (always sequence-wrapped) | `ReactionPipelineDraft.cs:82-88` | ✅ |
| ordered sync commands | `:82-88` | ✅ |
| sync → async opener → sync | `BuildReaction` `:52-58` | ✅ |
| `Element.Show` | `ElementBuilder.cs:124` | ✅ |
| `Element.Hide` | `:131` | ✅ |
| `SetText(string)` literal | `:55` | ✅ |
| `SetText<TSource>(args, path)` event payload | `:65` | ✅ |
| `SetText<TResponse>(ResponseBody, path)` | `:76` | ✅ |
| `SetText<TProp>(TypedSource)` | `:87` | ✅ |
| `SetHtml(string)` literal | `:96` | ✅ |
| `SetHtml<TSource>(args, path)` event payload | `:106` | ✅ |
| `SetHtml<TProp>(TypedSource)` | `:116` | ✅ |
| `Component.Set` property write | `B2`, `PipelineBuilder.cs:101` | ✅ |
| event-arg `set` (payload scope) | matrix `set` payload row | ✅ |
| `Element.AddClass` | `:31` | ✅ |
| `Element.RemoveClass` | `:39` | ✅ |
| `Element.ToggleClass` | `:47` | ✅ |
| `Component.Call` (no-arg) | `B2` | ✅ |
| `Component.Call` (args) | `B2` | ✅ |
| plugin command `.Fire()` | `PluginCallBuilder.cs:116` | ✅ |
| event-arg method `call` | matrix `call` payload row | ✅ |
| `Dispatch(name)` | `PipelineBuilder.cs:43` | ✅ |
| `Dispatch<T>(name, literal)` | `:54` | ✅ |
| `DispatchWith<T>(name, b=>…)` | `:75` | ✅ |
| `DispatchPayloadBuilder.Set<TProp>(expr, src)` | `DispatchPayloadBuilder.cs:30` | ✅ |
| `DispatchPayloadBuilder.Set(literal)` ×3 (typed-literal) | `:43,56,69` | ✅ |
| `Into(elementId)` → `inject` | `PipelineBuilder.cs:273` | ✅ |
| `ValidationErrors(formId)` → `show-validation-errors` | `:263` | ✅ |

**Reactions: 28 / 28.** A single command is **always** sequence-wrapped:
`FlushPendingSyncReactions` (`ReactionPipelineDraft.cs:82-88`) unconditionally wraps
pending sync reactions in `ReactionGraph.Sequence(...)` (only guard is `Count==0`), so
`BuildReaction` (`:52-58`) returns `{"kind":"sequence","steps":[node]}`, not a bare
node. The `DispatchPayloadBuilder.Set` literal overloads (`:43,56,69`) are 3; the
nested-path build + leaf/parent conflict throw (`:88,118,142`) is the author-time
invariant for those 3, pinned by the object-value composite row
(`04-matrix-http-arrays-values.md:101`).

### Conditions — left-operand source kinds + operators + composition + branch + confirm

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| left source: component / url / plugin-read / plugin-prop / event-payload / response-body | `When` overloads `PipelineBuilder.Conditions.cs:11,22,34`; `TypedComponentSource`/`TypedUrlSource`/`TypedPluginSource`/`PayloadTypedSource` | 6 | ✅ |
| left source: element-scope member read | `ElementExpressionCompiler.cs:130-136` | 1 | ✅ |
| left source: element-scope whitelisted method read | `ElementExpressionCompiler.cs:140-154`; `ValueExpression.cs:108-112` | 1 | ✅ |
| compare tokens (literal-binary + unary + membership + range + text + regex + min-length + array-contains) | `ConditionSourceBuilder.cs:49-105` | 21 | ✅ |
| source-vs-source operand form | `ConditionSourceBuilder.cs:112-122` | 6 | ✅ |
| guard `And`/`Or` typed-source + nested-group + `Not` | `GuardBuilder.cs:43,52,62,71,81,88,95,106,118` | 9 | ✅ |
| branch `Then`/`ElseIf`×3/`Else`/no-match | `GuardBuilder.cs:126`; `BranchBuilder.cs:29,40,52,61` | 6 | ✅ |
| `Confirm` + `Confirm`-and-compose | `ConditionStart.cs:42`; `GuardBuilder.cs:81-85` | 2 | ✅ |

**Conditions: 52 / 52.** `Confirm.And(compare)` composes `All([confirm, compare])`
(`GuardBuilder.cs:81-85`) and `evaluateAllInLane` iterates left-to-right from index 0
(`runtime/conditions/conditions.ts:62-72`), so confirm always opens the dialog before
the compare — the lowering is deterministic and the order is fixed.

### Values — literals + composites

| Variant | Source | Covered? |
|---|---|---|
| literal scalar (string/int/long/decimal/double/bool/DateTime) | `Shape.FromClrType` `Shape.cs:70-118` | ✅ |
| literal null | `ValueExpression.Null` | ✅ |
| literal arbitrary (enum/Guid/object → `Shape.FromValue`, full table in Fix 2) | `Shape.cs:96-97` → `FromClrType:70-118` | ✅ |
| object value | `DispatchPayloadDraft` | ✅ |
| array value | `ValueExpression.Array` | ✅ |

**Values (literals + composites): 5 / 5.** Reads are counted under Conditions
left-operand and Arrays to avoid double-counting the same `ReadExpression` template;
whole-payload/whole-element reads are the distinct node kinds of Fix 1.

### HTTP — verbs, gather, body egress, response, loading, parallel

| Variant | Source | Covered? |
|---|---|---|
| `Get(url)` | `PipelineBuilder.Http.cs:11` | ✅ |
| `Post(url)` | `:19` | ✅ |
| `Post(url, gather)` inline | `:25` | ✅ |
| `Put(url, gather)` inline | `:31` | ✅ |
| `Delete(url)` | `:39` | ✅ |
| bare `Put(url)` pipeline entry | **redesign** (current: only `HttpRequestBuilder.Put` `:42`) | ✅ (labeled redesign) |
| inline-gather for every body verb | **redesign** (current: Post/Put only `:25,31`) | ✅ (labeled redesign) |
| URL `{placeholder}` template | `RequestRouteTemplate` | ✅ |
| `Include<TComp,TModel>(expr)` | `GatherExtensions.cs:17` | ✅ |
| `Include<TComp,TModel>(refId, name)` DISPLAY component | `GatherExtensions.cs:36` | ✅ |
| `Include<TModel,TProp>(…)` ×2 | `GatherExtensions.cs:58,71` | ✅ |
| `IncludeAll()` | `GatherBuilder.cs:28` | ✅ |
| `Static(param, value)` | `:38` | ✅ |
| `FromEvent(args, path, name)` | `:54` | ✅ |
| `Header(name, literal)` | `:69` | ✅ |
| `Header<TProp>(name, source)` | `:81` | ✅ |
| `Header<TArgs,TProp>(name, args, path)` | `:91` | ✅ |
| `RouteParam(name, int)` | `:103` | ✅ |
| `RouteParam(name, string)` | `:111` | ✅ |
| `RouteParam(name, long)` | `:123` | ✅ |
| `RouteParam<TProp>(name, source)` | `:131` | ✅ |
| `RouteParam<TArgs,TProp>(name, args, path)` | `:141` | ✅ |
| `FromUrl(name)` | `:157` | ✅ |
| `FromUrl(name, asParam)` | `:169` | ✅ |
| `FromUrl<T>(name)` | `:181` | ✅ |
| `FromUrl<T>(name, asParam)` | `:194` | ✅ |
| `Plugin<T>(source, name)` gather | `:207` | ✅ |
| bodiless (no gather) | `RequestInput.None` | ✅ |
| `AsJson()` | `HttpRequestBuilder.cs:62` | ✅ |
| `AsFormData()` | `:65` | ✅ |
| scalar → JSON body | `request-payload-writer.ts` | ✅ |
| array → JSON body | `request-payload-writer.ts:221-225` | ✅ (see Fix 3) |
| scalar/array → query (GET) | writer | ✅ |
| scalar/array/file → form-data | writer | ✅ |
| `OnSuccess(p)` untyped | `ResponseBuilder.cs:28` | ✅ |
| `OnSuccess<R>(…)` typed | `:40` | ✅ |
| `OnError(p)` any-status | `:55` | ✅ |
| `OnError(status, p)` | `:67` | ✅ |
| `OnError<E>(…)` typed any-status | `:79` | ✅ |
| `OnError<E>(status, …)` typed + status (4th overload) | `:96` | ✅ |
| network-failure routing | `http.ts:263` | ✅ |
| `Chained(req)` | `:111` | ✅ |
| `WhileLoading(p)` | `HttpRequestBuilder.cs:70` | ✅ |
| `Finally(p)` | `:89` | ✅ |
| `Validate<T>(formId)` | `:103` | ✅ |
| `Parallel(...)` + `OnAllSettled` | `PipelineBuilder.Http.cs:45` | ✅ |
| `Into` after request (inject success body, string-shape boundary throw) | `execute.ts:207-218` | ✅ |

**HTTP: 47 / 47.** OnError routing is **exact-status-preferred, then first
any-status** — `routeResponseRoutes` is `routes.find(exactStatus) ?? routes.find(anyStatus)`
(`runtime/execution/http.ts:263`), NOT positional first-match; an any-status route
authored before `OnError(404,…)` still loses to the 404 route on a 404. `Into` of a
success body that parsed to a non-string throws a typed shape error at the egress
boundary (`execute.ts:207-218`), not silent coercion.

### Arrays — entries + ops + terminal

| Variant | Source | Covered? |
|---|---|---|
| `From<TElement>(TypedSource<T[]>)` | `PipelineBuilder.Arrays.cs:15` | ✅ |
| `From<TArgs,TElement>(args, sel)` | `:23` | ✅ |
| `FromDom(id, member)` | `:37` | ✅ |
| `FromDom<TElement>(id, member)` typed | `:41` | ✅ |
| `Where` | `ReactiveArray.cs:28` | ✅ |
| `Select` | `:34` | ✅ |
| `OrderBy` | `:43` | ✅ |
| `OrderByDescending` | `:47` | ✅ |
| `Count()` | `:70` | ✅ |
| `Count(pred)` | `:74` | ✅ |
| `Any()` | `:78` | ✅ |
| `Any(pred)` | `:82` | ✅ |
| `All(pred)` | `:86` | ✅ |
| `Sum(int)` | `:90` | ✅ |
| `Sum(decimal)` | `:94` | ✅ |
| `Sum(double)` | `:98` | ✅ |
| `Find(pred)` | `:102` | ✅ |
| `Find<TField>(pred, proj)` | `:107` | ✅ |
| `AsSource()` terminal | `:121` | ✅ |

**Arrays: 19 / 19.** `Sum(int)/Sum(decimal)/Sum(double)` are 3 dedicated overloads
under the `sum` row (`04-matrix-http-arrays-values.md:290`).

### Validation — `ClientValidationFieldRuleBuilder` (31 public methods) + `ReactiveClientRules` + `WhenField` + collection/server/display

`grep -nE "public " Alis.Reactive/Validation/ClientValidationFieldRuleBuilder.cs` = **31
public methods** (32 `public` lines minus the class declaration). Each maps to a
`ValidationRuleName` family via `AddLiteralComparison`/`AddPeerComparison`/
`AddNoOperand`/`AddRule`: `Required`, `Empty`, `Email`, `Url`, `CreditCard`,
`AtLeastOne`, `MinLength`, `MaxLength`, `Regex`, `Range`, `ExclusiveRange`, `Min`,
`Max`, `Gt`, `Lt`, `EqualTo` (literal + 2 peer forms), `NotEqual` (literal),
`NotEqualTo` (2 peer forms), and the ordered peer comparisons `GreaterThan`/
`GreaterThanOrEqualTo`/`LessThan`/`LessThanOrEqualTo` (literal + peer each). No method
is unrowed — the 31 reduce to the finite RuleName+operand families pinned in matrix
A1/A2 with the `EqualTo`/`NotEqual` literal-vs-peer asymmetry noted at
`04-matrix-validation-components-slots.md:124-133`.

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| field rule builder methods (incl. `EqualTo` literal+2 peers, `NotEqual` literal, `NotEqualTo` 2 peers, ordered literal+peer) | `ClientValidationFieldRuleBuilder.cs:27-156` | 31 | ✅ |
| static `ReactiveClientRules` server+client paired overloads incl. nullable-struct peers | `ReactiveClientRuleBuilder.cs:83-356` | 38 | ✅ |
| `WhenField` family + `WhenFields` | `ReactiveValidator.cs:103-181` | 24 | ✅ |
| `ClientRuleEach` + `ReactiveClientRuleBuilder.AtLeastOne`/`.SetValidator` | `ReactiveValidator.cs:41`; `ReactiveClientRuleBuilder.cs:60,67` | 3 | ✅ |
| nested `ClientRule(child)` | `ReactiveValidator.cs:63` | 1 | ✅ |
| `ClientRulesFrom` ×2 | `ReactiveValidator.cs:80,92` | 2 | ✅ |
| server errors (`show-validation-errors`) + inline + summary | matrix A4 | 3 | ✅ |

**Validation: 102 / 102.** (Field rule builder counted at its true 31; the prior
proof said 27. Every method still maps to a rowed RuleName+operand family — the count
correction does not add or remove a family, it corrects the hand-tally.)

### Components — render/registration + mutation/read + event + grid + per-slice `.Reactive`

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| input render+registration (native, fusion) | B1 `:201,202` | 2 | ✅ |
| unregistered-render author throw | `InputBoundFieldBase.Render` | 1 | ✅ |
| B2 property set / method call / set-from-source / read | B2 4 rows | 4 | ✅ |
| B3 component event (display) / input event | B3 2 rows | 2 | ✅ |
| B4 grid render / DataStateChange / mutation / inline-validation | B4 4 rows | 4 | ✅ |
| per-slice `.Reactive()` (58 overload lines across 60 slices, incl. `FusionSmartTextArea` ×2 `:11,22`) | `Alis.Reactive.Fusion/**`, `Alis.Reactive.Native/**` | 1 template (B3-parameterized) | ✅ |
| slices with NO `.Reactive` (`FusionButton`, `FusionSmartPasteButton`, `NativeActionLink`) | slice source | 1 (no-event subset) | ✅ |

**Components: 14 / 14 template families.** Vendor Rule 5 holds: every slice reduces to
B2 `set`/`call`/`read` or B3 `componentEvent`; the 58 `.Reactive` overload lines are
B3 instantiations parameterized over each slice's `TypedEvent` set
(`04-matrix-validation-components-slots.md:243-247`), not new templates.

### Slots / Composition — `PlanExtensions` + `Into`

| Variant | Source | Covered? |
|---|---|---|
| root view plan | `Html.ReactivePlan` | ✅ |
| same-model partial (SSR join) | `Html.ResolvePlan` | ✅ |
| independent-model partial | `Html.ReactivePlan<TOther>` | ✅ |
| browser slot load via `Into` | `boot.ts:74` `loadPartialSlot` | ✅ |
| browser slot unload | `boot.ts:91` `unloadPartialSlot` | ✅ |

**Slots: 5 / 5.**

### Plugins — declare + read/call/arg

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| `RegisterPlugin(name, configure)` | `ReactivePlan.cs:54` | 1 | ✅ |
| `RegisterPlugin(ReactivePlugin)` | `:66` | 1 | ✅ |
| `RegisterPlugin<TPlugin>()` | `:73` | 1 | ✅ |
| `ReactivePlugin` `Function`/`Property`/`Command` arity declaration overloads | `ReactivePlugin.cs:27-131` | 21 | ✅ (collapse-to-args-builder, redesign) |
| `PluginTypeBuilder` declaration overloads | `PluginTypeBuilder.cs` | 10 | ✅ (collapse-to-args-builder, redesign) |
| `PluginReadBuilder.Arg` (typed + scalar ×7 + source + ArgValue) | `PluginReadBuilder.cs:30-104` | 11 | ✅ |
| `PluginCallBuilder.Arg` (same set) + `.Fire()` | `PluginCallBuilder.cs:35-116` | 12 | ✅ |
| `Plugin<T>` read entries + `Plugin` call entries | `PipelineBuilder.cs:164-253` | 8 | ✅ |

**Plugins: 65 / 65.** The `RegisterPlugin` verbs and the ~31 arity declaration
overloads collapse to **one** args-builder in the redesign
(`04-matrix-validation-components-slots.md:319-324`) — a **labeled redesign target**,
not shipped today.

### App-level objects

| Variant group | Source | Count | Covered? |
|---|---|---|---|
| Drawer `Open`/`Close`/`SetSize` + layout markup | `NativeDrawerExtensions.cs:36,62,78,96,100` | 5 | ✅ |
| Loader `Show`/`Hide`/`SetTarget`/`SetTimeout` + layout | `NativeLoaderExtensions.cs:41,54,65,81,101,105` | 6 | ✅ |
| Confirm `SetContent`/`Show`/`Hide` + layout | `FusionConfirmExtensions.cs:21,29,34,39` | 4 | ✅ |
| Toast `SetTitle`/`SetContent`/`SetTimeout`/`ShowCloseButton`/`ShowProgressBar`/`Success`/`Warning`/`Danger`/`Info`/`Show`/`Hide` + layout | `FusionToastExtensions.cs:41-103` | 12 | ✅ |
| ActionLink single request | `NativeActionLinkHtmlExtensions.cs:13` | 1 | ✅ |

**App-level (live): 28 / 28.** The 3 dead enums (`DrawerPosition`, `ToastType`,
`ToastPosition`) are **deleted** and **excluded** — see Section 4. Toast type methods
are parameterless (`.Success()/.Warning()/.Danger()/.Info()`); there is no
`Show(msg, ToastType, ToastPosition)` signature.

---

## 6. Total — live coverage

| Area | Covered | Live total |
|---|---|---|
| Triggers | 10 | 10 |
| Reactions | 28 | 28 |
| Conditions | 52 | 52 |
| Values (literals + composites) | 5 | 5 |
| HTTP | 47 | 47 |
| Arrays | 19 | 19 |
| Validation | 102 | 102 |
| Components (template families) | 14 | 14 |
| Slots | 5 | 5 |
| Plugins | 65 | 65 |
| App-level (live) | 28 | 28 |
| **Total (live)** | **375** | **375** |

**Live-variant deterministic coverage = 375 / 375 = 100%.**

Deleted dead surface excluded from the denominator (Section 4): `DrawerPosition`,
`ToastType`, `ToastPosition` — 3 public enums with zero `.cs` consumers, deleted in
the redesign. Two further dead **wire-enum** members (`set` target on a `plugin`
source — `plan.ts:371-373`; `payload` scope `local` — no public producer) are folded
out of generation and are **not** in the authoring denominator
(`04-matrix-triggers-reactions-conditions.md:421-454`). Neither is a case where one
public overload produces two outputs.

---

## 7. Certification

Against the determinism criterion of Section 1, for the **redesign target**:

1. **One output per live variant** — ✅ all 375 live authoring variants have a
   dedicated deterministic redesign row; no overload lowers to two outputs.
2. **Current bugs resolved by a stated rule** — ✅ the sole live non-determinism (the
   `responseBody`/`elementValue` magic-member collision) is `RESOLVED-BY-REDESIGN` by
   Fix 1: whole reads become distinct node *kinds* and the reserved member string is
   never emitted for a whole read, making the collision unrepresentable. Fixes 2-4
   close the supporting under-pinned defaults and the id drift surface.
3. **Dead surface excluded, not invented** — ✅ the 3 zero-consumer enums are deleted
   and excluded; coverage is not inflated by inventing features for them.
4. **Per-variant authority** — ✅ counts are per-overload (4 `SetText`, 3 `SetHtml`, 3
   `Sum`, 5 `RouteParam`, 31 field-rule methods, 38 `ReactiveClientRules`, …), and
   every method maps to a rowed family.

**Verdict: the redesign blueprint is deterministic — 375 / 375 = 100% of live public
authoring variants lower to exactly one plan-JSON shape and one browser behavior.**
The four design-level fixes (Section 3) are labeled redesign targets and are the
load-bearing prerequisites; once built — Fix 1 in `ValueExpression.cs`, `plan.ts`, and
`evaluate.ts` above all — the live read surface carries no many-to-one collision and a
generator iterating the per-area tables emits one case per real overload, each fixed
by its named axes.
