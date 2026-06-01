# Grammar Critique — Validation (ReactiveValidator / ClientRule / WhenField / rule verbs / FieldGuard)

PL-architect hardening pass over the Validation cluster of the green-field DSL.
Every current shape is cited from the AST grammar
`ast-grammar-value-arrays-validation.md` (real `Receiver -> Member(params) -> Return`
with `file:line`), cross-checked against the conditions grammar
`ast-grammar-conditions.md`, and reconciled to the finalized names in
`09-dsl-naming-sheet.md` (§3.6) and the determinism discoveries in
`08-determinism-formalization.md` (§3.9, §6.3).

**Bar.** The DSL must be EASY TO WRITE and read TALL (one call per line, vertical
fluent chains). Judged on: orthogonality, composability, TALL-reading,
least-surprise, discoverability, consistency, easy-to-write.

**Zero feature loss.** Every adjustment preserves every capability. No rule verb,
no presence operator, no peer-comparison overload, no activation shape is dropped.

**Reconciliation rule used throughout.** The AST grammar tables record the *current*
source type names (`ClientValidation*`). The naming sheet §3.6 has already decided
to drop the redundant `Validation` filler (`ClientValidation* -> Client*`) and to
spell out the `WhenField` comparison suffixes. Where a wart is *already* fixed by a
decided rename, this critique CONFIRMS it (so it is not re-litigated) and does NOT
count it as a new adjustment. The numbered adjustments below are the warts the
naming sheet does **not** yet close — structural / orthogonality / least-surprise
gaps that need a grammar shape change, not just a token rename.

---

## A. What is ALREADY GOOD — do not churn

These read well cold and compose cleanly. Touching them is churn, not improvement.

1. **The rule-verb chain is the model TALL surface.** `ClientValidationFieldRuleBuilder`
   returns *itself* from every verb (`Required`/`Email`/`MinLength`/`Range`/… —
   `ast-grammar-value-arrays-validation.md:131-161`, all `ReturnsSelf=yes`). That is
   exactly the property that lets a field's rules stack one-per-line:

   ```csharp
   .Field(m => m.Password)
       .Required("Password is required")
       .MinLength(8, "At least 8 characters")
       .Matches("[A-Z]", "Needs an uppercase letter")
   ```

   This is the gold standard for TALL-reading. Keep it verbatim.

2. **Spelled-out constraint verbs on the rule builder.** `GreaterThanOrEqualTo` /
   `LessThanOrEqualTo` / `EqualTo` (`:144-156`) read in one breath in the constraint
   lane. The naming sheet (§3.6) correctly keeps these spelled out rather than
   collapsing to `Gte`/`Lte`. Cold-correct. Keep.

3. **Peer-field overload reuses the verb name.** `EqualTo(TValue)` vs
   `EqualTo(Expression<Func<TModel,TValue>>)` vs `EqualTo(ClientValidationFieldToken<…>)`
   (`:148-150`) — one concept (equality), three operand kinds (literal / live model
   expression / pre-built token). One verb, overloaded on operand. Correct
   orthogonality. Keep.

4. **`When(conditionFactory, define)` recurses into the same rules builder.**
   `ClientValidationRulesBuilder.When(Func<…ConditionBuilder…,…Condition…>, Action<…RulesBuilder…>)`
   (`:120`) hands the `define` callback back **this same builder** — a clean
   recursion point, so conditionally-activated rules nest with the identical
   one-per-line shape as top-level rules. `cod` fits `dom`. Excellent composability.
   Keep the recursion shape.

5. **`And`/`Or`/`Not` on the completed condition are self-returning.**
   `ClientValidationCondition.And/Or/Not` all return
   `ClientValidationCondition<TModel>` (`:212-214`, `ReturnsSelf=yes`) — guards
   chain/repeat. This already matches the conditions-area `GuardBuilder.And/Or/Not`
   self-returning shape (`ast-grammar-conditions.md:81-83`). Consistent across areas.
   Keep the self-returning composition.

6. **`ClientValidationFieldToken.For(expr)`** (`:224`) is a clean, discoverable
   factory that lets one field reference be built once and reused across peer
   comparisons and conditions. Good DRY seam. Keep.

7. **The terse condition-operator vocabulary on the condition start**
   (`Truthy/Falsy/Eq/Neq/Gt/Gte/Lt/Lte/IsNull/NotNull/IsEmpty/NotEmpty/In/NotIn/
   Between/Contains/StartsWith/EndsWith/Matches/MinLength/ArrayContains` —
   `:181-201`) is the **same** vocabulary the conditions area uses
   (`ast-grammar-conditions.md:37-57`). One activation engine, one operator set
   (the "one-engine law", `08-determinism-formalization.md:747`). Do not fork it.
   Keep.

Confirmed-decided fixes (already in the naming sheet §3.6 — NOT re-counted here):
`ClientValidation* -> Client*` filler drop; `WhenFieldGt/Gte/Lt/Lte ->
WhenFieldGreaterThan/…OrEqualTo/LessThan/…OrEqualTo`; the app-level trio
`ClientConditionBuilder -> ClientFieldConditionStart -> ClientCondition` aligned to
the FluentValidator trio so a dev meets ONE And/Or vocabulary; `notEqual` /
`notEqualTo` kept distinct (D1).

---

## The Adjustments

Eight numbered adjustments. Each: BEFORE (cited shape) -> AFTER, the PL-architect
property it improves, and proof that no capability is lost.

---

### Adjustment 1 — `When(conditionFactory, define)`: two callbacks in one wide call is the one non-TALL seam

**BEFORE** (`ast-grammar-value-arrays-validation.md:120`):

```
ClientValidationRulesBuilder<TModel>
  | When(Func<ClientValidationConditionBuilder<TModel>, ClientValidationCondition<TModel>> condition,
         Action<ClientValidationRulesBuilder<TModel>> define) -> void
```

This is a **two-argument call where the first argument is itself a lambda that must
return a condition** and the second is the body. In practice it reads:

```csharp
.When(c => c.Field(m => m.IsMember).Truthy(), rules =>
{
    rules.Field(m => m.MemberId).Required("Member id required");
});
```

The condition predicate (`c => …`) and the activated rules (`rules => …`) sit on the
**same call** as two positional lambdas. That is the one place in the whole cluster
that reads WIDE: the reader must hold the first lambda's closing `,` in their head
while parsing the second.

**PROPERTY HURT:** TALL-reading + least-surprise. Every other nesting point in the
cluster (rule verbs, `And`/`Or`) reads one-call-per-line; this one forces a
two-lambda positional call. It is also inconsistent with the conditions area, where
the guard is built first and `Then(pipeline)` opens the body as a *separate* call
(`ast-grammar-conditions.md:84`).

**AFTER** — split the two callbacks into two chained calls, mirroring the
conditions area's `When(...).Then(...)` shape:

```csharp
.When(c => c.Field(m => m.IsMember).Truthy())
    .Then(rules =>
    {
        rules.Field(m => m.MemberId).Required("Member id required");
    });
```

`When(conditionFactory)` returns a small `ClientWhenBuilder<TModel>` whose only
member is `Then(Action<ClientValidationRulesBuilder<TModel>>)`. This makes the
validation `When` read **identically** to the conditions `When().Then()` — same
concept, same shape (CONSISTENCY), and each callback gets its own line
(TALL-reading). The recursion target (the rules builder handed to `Then`) is
unchanged, so no nesting capability is lost.

**Capability preserved:** identical activation semantics (`When = combineActivation`,
`08-determinism-formalization.md:727`); the `define` body is the same
`Action<ClientValidationRulesBuilder<TModel>>` recursion point, just reached via
`.Then(...)` instead of a second positional arg.

---

### Adjustment 2 — `When` returns `void`, killing the chain; sibling guarded blocks cannot stack TALL

**BEFORE** (`:120`): `When(...) -> void`.

Because `When` is `void`, you cannot place a second guarded block on the next line of
the same chain. Two independent conditional groups force two **separate statements**:

```csharp
b.When(c => c.Field(m => m.IsMember).Truthy(), rules => { rules.Field(m => m.MemberId).Required("…"); });
b.When(c => c.Field(m => m.HasCar).Truthy(),   rules => { rules.Field(m => m.Plate).Required("…"); });
```

The chain breaks; the reader loses the single-builder spine.

**PROPERTY HURT:** composability + TALL-reading. A self-returning `When` would let
guarded blocks stack vertically on one builder, the same way `Field(...)` blocks do.

**AFTER** — combined with Adjustment 1, return the rules builder from `Then` so
sibling guarded blocks chain:

```
ClientWhenBuilder<TModel>.Then(Action<ClientValidationRulesBuilder<TModel>> define)
    -> ClientValidationRulesBuilder<TModel>   // was void
```

```csharp
b.When(c => c.Field(m => m.IsMember).Truthy()).Then(r => r.Field(m => m.MemberId).Required("…"))
 .When(c => c.Field(m => m.HasCar).Truthy()).Then(r => r.Field(m => m.Plate).Required("…"));
```

**PROPERTY IMPROVED:** composability (`cod(Then) = dom(When)` — the builder hands back
itself so the next intent attaches), least-surprise (a builder method that does not
terminate should return the builder).

**Capability preserved:** the body recursion is unchanged; this only widens the
return so the outer chain continues. Matches the kept-good self-returning shape of
`And`/`Or`/`Not` (A.5) and the rule verbs (A.1).

---

### Adjustment 3 — `Field` for rules and `Field` for conditions are two unrelated builders with the same name and a colliding overload set

**BEFORE** — two different `Field<TValue>` entry points:

```
ClientValidationRulesBuilder<TModel>.Field<TValue>(Expression<Func<TModel,TValue>>)
    -> ClientValidationFieldRuleBuilder<TModel,TValue>            (:118)
ClientValidationConditionBuilder<TModel>.Field<TValue>(Expression<Func<TModel,TValue>>)
    -> ClientValidationFieldConditionStart<TModel,TValue>         (:170)
```

Same verb `Field`, same parameter, **different return** depending on which builder
you happen to be holding. A dev who has internalized "`Field(expr)` gives me rule
verbs" gets, inside a `When` condition factory, a *condition start* instead — a
silent context switch.

**PROPERTY HURT:** least-surprise + discoverability. The grammar is actually
consistent here (both "open a typed view of a field"), but the **return-type fork on
the same name** means the dev cannot predict what `Field(expr)` yields without
knowing the receiver. This is acceptable *if and only if* the two builders are named
so the receiver is obvious at the call site.

**AFTER** — keep the verb `Field` (it is the right, screaming-cold name for "name a
model field"), but make the two receivers' names disambiguate via the §3.6
filler-drop already decided, AND add a doc-confirmed invariant that the **condition**
`Field` start exposes only the terse condition operators (it does) while the **rule**
`Field` builder exposes only the constraint verbs (it does). No rename needed beyond
§3.6; the adjustment is to **lock the two return surfaces as disjoint** so the fork is
intentional, not accidental:

- `ClientValidationRulesBuilder.Field -> ClientFieldRuleBuilder` (constraint verbs only)
- `ClientConditionBuilder.Field -> ClientFieldConditionStart` (condition operators only)

and document that the rule-lane `Field` never exposes `Eq/Gt/Truthy` and the
condition-lane `Field` never exposes `Required/Email/Range`. The disjointness is what
makes the shared verb safe.

**PROPERTY IMPROVED:** least-surprise (the receiver name now tells you which surface
you get), orthogonality (two disjoint operator sets, no overlap to confuse).

**Capability preserved:** both `Field` overloads (expression + token, `:118-119` and
`:170-171`) stay on both builders; nothing is removed.

---

### Adjustment 4 — `When` exposes the raw `ClientValidationConditionBuilder`; the dev must learn a second factory just to start a condition

**BEFORE** (`:120` + `:170`): the `When` condition argument is
`Func<ClientValidationConditionBuilder<TModel>, ClientValidationCondition<TModel>>`,
and the only way to start a condition is `conditionBuilder.Field(expr)`
(`ast-grammar-value-arrays-validation.md:170`). So the dev writes
`c => c.Field(m => m.X).Truthy()` — the `c.Field` ceremony is a second spelling of
the thing they already write everywhere else (`b.Field(...)` for rules).

**PROPERTY HURT:** easy-to-write (extra ceremony: `c =>` then `c.Field`),
discoverability (two `Field`-bearing builders to learn). It is the same friction the
conditions area accepts via `ConditionStart`, but here the wrapper adds nothing the
naming-sheet trio rename does not already cover.

**AFTER** — let the condition factory parameter type be the **conditions-area
`ConditionStart<TModel>`** vocabulary instead of a parallel
`ClientConditionBuilder` (per §3.6 the app-level trio is being aligned to the
FluentValidator trio anyway). Concretely: the `When` predicate factory is
`Func<ClientFieldConditionStart-opener, ClientCondition<TModel>>`, and the opener
type is the **single** condition-start the whole framework uses, so the dev writes
the *same* `c.Field(m => m.X).Truthy()` they already know from conditions — one
vocabulary, learned once.

**PROPERTY IMPROVED:** consistency (one condition-start vocabulary across conditions
and validation, the "one-engine law" made visible at authoring time, not just at
runtime — `08-determinism-formalization.md:747,914`), discoverability.

**Capability preserved:** every operator (`Truthy`…`ArrayContains`, `:181-201`) and
`And/Or/Not` composition (`:212-214`) remains; only the *opener type* is unified, not
the operator set.

---

### Adjustment 5 — `And`/`Or` take only a pre-built `ClientValidationCondition`; the cluster is MISSING the nested-grouping callback that conditions has

**BEFORE** (`:212-213`):

```
ClientValidationCondition<TModel>.And(ClientValidationCondition<TModel> other) -> ClientValidationCondition<TModel>
ClientValidationCondition<TModel>.Or(ClientValidationCondition<TModel> other)  -> ClientValidationCondition<TModel>
```

`And`/`Or` accept **only** a fully-built sibling condition. To express
`(a OR b) AND c` the dev must build `a.Or(b)` first as a separate expression, name it,
then `.And(c)`. The conditions area, by contrast, has a **nested-grouping callback
shape** — `GuardBuilder.And(Func<ConditionStart,GuardBuilder>)` /
`Or(Func<…>)` (`ast-grammar-conditions.md:81-82`, `ReturnsSelf=yes`) — precisely so a
grouped sub-condition reads inline and TALL.

The naming sheet §1.1 explicitly mandates **two** And/Or shapes everywhere — the flat
value shape AND the nested grouping shape — and names the grouping shape as the one
"the flat shape cannot express" (`09-dsl-naming-sheet.md:90-93`). The validation
cluster currently has only the flat shape; the grouping shape is **absent from
source** for `ClientValidationCondition`.

**PROPERTY HURT:** composability + consistency. This is a real `cod`/`dom` gap: the
nested-grouping capability that the And/Or grammar promises (§1.1) does not exist on
this surface, so validation activation cannot author grouped boolean logic the way
conditions can.

**AFTER** — add the nested-grouping overload so it mirrors conditions exactly:

```
ClientCondition<TModel>.And(Func<ClientFieldConditionStart-opener, ClientCondition<TModel>> nested)
    -> ClientCondition<TModel>
ClientCondition<TModel>.Or(Func<…opener…, ClientCondition<TModel>> nested)
    -> ClientCondition<TModel>
```

```csharp
.When(c => c.Field(m => m.A).Truthy()
            .Or(g => g.Field(m => m.B).Truthy())   // (A OR B)
            .And(g => g.Field(m => m.C).Truthy()))  // … AND C
```

**PROPERTY IMPROVED:** composability (grouping reads inline and TALL),
consistency (validation And/Or now has the SAME two shapes — flat + nested — as
conditions, per §1.1's "single And/Or vocabulary across the Conditions area, the
FluentValidator side, and the app-level validation builder",
`09-dsl-naming-sheet.md:94-96`).

**Capability preserved:** the existing flat `And(condition)`/`Or(condition)` overloads
stay (they are the canonical flat shape per §1.1); this **adds** the grouping shape
that was missing. The internal n-ary lowering (`FieldCondition.All/Any/Not`,
`09-dsl-naming-sheet.md:96`) absorbs both without a new plan kind. Zero feature loss,
one feature gained to reach parity.

---

### Adjustment 6 — `NotEqual` (literal) vs `NotEqualTo` (peer) is correct but UNDISCOVERABLE; the asymmetry needs a compile-time signpost, not a silent naming convention

**BEFORE** (`:151-153`):

```
ClientValidationFieldRuleBuilder.NotEqual(TValue forbidden, string message)                       -> self
ClientValidationFieldRuleBuilder.NotEqualTo(Expression<Func<TModel,TValue>> peerField, string)     -> self
ClientValidationFieldRuleBuilder.NotEqualTo(ClientValidationFieldToken<TModel,TValue>, string)      -> self
```

`NotEqual` is literal-only; `NotEqualTo` is peer-only. The determinism algebra
**requires** they stay distinct (D1, `08-determinism-formalization.md:743-745`;
`09-dsl-naming-sheet.md:374`). But note `EqualTo` is **overloaded** for both literal
and peer (`:148-150`), while not-equals splits into two *differently-spelled* verbs.
A dev who knows `EqualTo(literal)` and `EqualTo(peer)` will reach for
`NotEqualTo(literal)` and get a compile error with no hint that the literal verb is
spelled `NotEqual`.

**PROPERTY HURT:** discoverability + least-surprise. The naming asymmetry is the
*only* signal of the literal/peer split, and it points the wrong way (the dev's mental
model from `EqualTo` says "one verb, two overloads").

**AFTER** — keep BOTH names (D1 forbids merging), but make the split **discoverable by
overload symmetry** rather than by spelling: keep `NotEqualTo` as the **peer** verb
(it pairs with `EqualTo`'s peer overload), and make `NotEqual` carry an XML-doc
`<summary>` and a distinct *parameter name* (`forbidden`) — already present (`:151`) —
that the IDE surfaces, plus add a doc-cross-reference. The hardening is: **the
literal not-equals verb name must visibly differ from the peer one AND the docs on
each must name its operand kind in the first sentence** ("…against a fixed value" vs
"…against another field"). This turns the asymmetry from a trap into a signpost.

> No rename (the algebra pins both tokens). The adjustment is a discoverability
> hardening: parameter naming + first-sentence operand-kind doc on each verb, so the
> IDE tooltip disambiguates at the call site.

**PROPERTY IMPROVED:** discoverability (IDE tooltip names the operand kind), while
honoring D1 (no merge).

**Capability preserved:** both verbs and all three overloads stay verbatim.

---

### Adjustment 7 — `EqualTo`/peer comparisons accept `Expression` AND `Token` (two overloads each); the `Expression` form is the redundant spelling once `For(expr)` exists

**BEFORE** — every peer comparison ships **two** overloads, e.g. (`:149-150`,
`:154-161`):

```
GreaterThan(Expression<Func<TModel,TValue>> peerField, string)      -> self   (:154)
GreaterThan(ClientValidationFieldToken<TModel,TValue> peerField, string) -> self   (:155)
```

and the same doubling for `EqualTo`, `NotEqualTo`, `GreaterThanOrEqualTo`,
`LessThan`, `LessThanOrEqualTo` (`:149-161`). That is **12 extra overloads** whose only
difference is `Expression` vs pre-built `Token`. But `ClientValidationFieldToken.For(expr)`
(`:224`) already builds a token from an expression — so `Token` is a *superset*: any
`Expression` can be wrapped with one call.

**PROPERTY HURT:** orthogonality (two spellings for "compare against another field" —
"one clear way per intent; kill redundant spellings"). The doubled surface also bloats
discoverability (IntelliSense shows two of every peer verb).

**AFTER** — keep the **`Expression` overload as the primary** ergonomic spelling
(devs write `m => m.Other` inline; it is TALL and zero-ceremony), and **demote the
`Token` overload** to a single shared concept: the `Token` form survives ONLY where a
field reference is genuinely reused across multiple rules (its real value, A.6). The
decision: do not delete capability, but stop *requiring* two parallel overloads per
verb — implement the `Token` path once via an **implicit conversion**
`ClientValidationFieldToken<TModel,TValue> <- Expression<Func<TModel,TValue>>` (or the
reverse), so every peer verb declares ONE overload that accepts the token, and the
expression flows in via the conversion.

```
// AFTER: one overload per peer verb
GreaterThan(ClientFieldToken<TModel,TValue> peerField, string message) -> self
// callable both ways, no second overload:
.GreaterThan(m => m.MinAge, "…")            // Expression -> token via implicit conversion
.GreaterThan(reusedToken, "…")              // pre-built token, reused
```

**PROPERTY IMPROVED:** orthogonality (one overload per peer verb instead of two),
discoverability (IntelliSense halves), while the inline-expression ergonomics are
fully preserved via the conversion.

**Capability preserved:** both call shapes (inline `m => …` and reused token) still
compile; the literal overload (`:148`, `EqualTo(TValue)`) is untouched. The peer
comparison set (`GreaterThan/GreaterThanOrEqualTo/LessThan/LessThanOrEqualTo/EqualTo/
NotEqualTo`) is unchanged. This is a *surface deduplication*, not a feature cut — it
collapses a redundant spelling exactly as §1.1 collapsed the three And/Or shapes to
two.

---

### Adjustment 8 — `AddReactiveClientValidation` + `Add<TSource,TModel>` registration is a DI-shaped entry that hides the validator; widen `Field` source intake to abstract `TypedSource` per §6.3

**BEFORE** (`:97`, `:107`):

```
IServiceCollection.AddReactiveClientValidation(Action<ReactiveClientValidationBuilder> configure) -> IServiceCollection
ReactiveClientValidationBuilder.Add<TValidationSource,TModel>(Action<ClientValidationRulesBuilder<TModel>> define) -> self
```

Two issues:

(a) **Discoverability.** The path to *any* client rule is
`services.AddReactiveClientValidation(v => v.Add<MySource,MyModel>(b => b.Field(...)...))`
— a DI-registration idiom where the model rules are buried two callbacks deep behind a
generic `Add<TSource,TModel>`. A dev looking for "where do I declare browser
validation rules" has to learn the DI ceremony first. (The `Add` self-return is good —
multiple sources chain — keep that.)

(b) **§6.3 seam (the load-bearing one).** The naming sheet/determinism work mandates
that the gather `Include` intake be widened from concrete source families to the
abstract `TypedSource<TProp>` so `cod(AsArraySource) = dom(Include)`
(`08-determinism-formalization.md:1031-1047`). The validation peer-comparison verbs
have the **same** latent seam: a peer operand is today an `Expression` or a `Token`
(`:149-161`), i.e. a *model-field* reference only. But the algebra says the peer read
"reuses the single Value spine" (`08-determinism-formalization.md:747`). A peer
comparison whose right side is a **`ReactiveValue<T>` array fold** (e.g.
`Max(m => m.Items, x => x.Price)`) — itself a `TypedSource<T>`
(`ast-grammar-value-arrays-validation.md:74`) — cannot be expressed, because the peer
overloads are typed to `Expression`/`Token`, not abstract `TypedSource`. That is the
identical `cod ⊄ dom` hole §6.3 fixes for `Include`.

**PROPERTY HURT:** (a) discoverability + easy-to-write; (b) composability (the value
spine does not actually reach the validation peer slot — a real seam bug by §6.3's
own test).

**AFTER:**

(a) Keep the DI registration (it is the correct app-level singleton wiring per the
naming sheet's app-level-objects model), but confirm the §3.6-decided rename so the
*intent* surfaces: the configure callback hands back the builder whose verbs are
`Field(...)` rule chains — and document the entry as "declare this validator's
browser rules", so the DI idiom is the wiring, not the discovery surface. (No new
adjustment to the DI shape itself — it is genuinely an app-level service registration.)

(b) **Widen the peer-comparison operand intake to abstract `TypedSource<TValue>`**, the
direct validation analogue of §6.3:

```
// BEFORE (per-verb): Expression OR Token (model-field only)
GreaterThan(Expression<Func<TModel,TValue>> peerField, string) -> self
GreaterThan(ClientValidationFieldToken<TModel,TValue> peerField, string) -> self

// AFTER: one abstract TypedSource intake (subsumes field-expr, token, AND value-spine reads)
GreaterThan(TypedSource<TValue> right, string message) -> self
```

Combined with Adjustment 7's implicit `Expression -> Token` conversion and the fact
that `ClientFieldToken` and `ReactiveValue<T>` both *are* `TypedSource<T>`
(`ast-grammar-value-arrays-validation.md:70,76-85`), every peer comparison now reads
through the **one** value spine — closing the same mis-cut seam §6.3 closes for
`Include`.

**PROPERTY IMPROVED:** composability (`cod(AsArraySource)/cod(ReactiveValue) =
dom(peer-comparison)` — the value spine genuinely reaches the validation peer slot),
consistency (validation peer reads use the SAME abstract `TypedSource` intake that
gather `Include` uses post-§6.3).

**Capability preserved:** field-expression and token peer comparisons still compile
(they are `TypedSource<T>` subtypes); this only *widens* the accepted set, never
narrows it. Strictly additive — every existing peer comparison still works, and
value-spine reads become newly expressible.

> Note: the validation peer read must stay in the **sync** lane (it reuses the
> Condition `CompareEngine`, not the async HTTP lane). `TypedSource<T>` carries only
> deterministic reads here; `Confirm`/async sources are not part of the validation
> activation surface, so widening to `TypedSource` introduces no async leak — same
> guarantee §6.3 relies on for `Include`.

---

## Summary table

| # | Wart (cited) | Property | BEFORE -> AFTER (one line) |
|---|---|---|---|
| 1 | `When(condFactory, define)` two positional lambdas (`:120`) | TALL-reading, consistency | `When(cond, body)` -> `When(cond).Then(body)` (mirror conditions) |
| 2 | `When(...) -> void` breaks the chain (`:120`) | composability | `Then(...) -> RulesBuilder` so guarded blocks stack TALL |
| 3 | `Field` forks return type by receiver (`:118`,`:170`) | least-surprise | Lock the two `Field` surfaces as disjoint operator sets |
| 4 | `When` exposes a parallel `ClientConditionBuilder` (`:120`,`:170`) | consistency, discoverability | Unify the condition-start opener with the conditions-area one |
| 5 | `And`/`Or` take only pre-built condition; nested-grouping shape MISSING (`:212-213`) | composability | Add `And/Or(Func<…,Condition>)` nested shape (§1.1 parity) |
| 6 | `NotEqual`(literal)/`NotEqualTo`(peer) split undiscoverable (`:151-153`) | discoverability | Keep both (D1); operand-kind first-sentence docs + param names |
| 7 | 12 doubled `Expression`+`Token` peer overloads (`:149-161`) | orthogonality | One overload per peer verb via implicit `Expression -> Token` |
| 8 | Peer operand typed `Expression`/`Token`, not value spine (`:149-161`) | composability (§6.3) | Widen peer intake to abstract `TypedSource<TValue>` |

**Total proposed adjustments: 8.** All strictly additive or shape-aligning; zero
capability removed (rule verbs, presence operators, peer comparisons, activation
shapes, DI registration all preserved). Adjustments 1–2 and 4–5 bring validation into
exact shape-parity with the conditions area (`ast-grammar-conditions.md`); Adjustment
8 applies the §6.3 `TypedSource`-widening discovery to the validation peer slot.
