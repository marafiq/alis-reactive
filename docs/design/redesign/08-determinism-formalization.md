# 08 — Determinism, Formalized

> The whole pipeline is a finite composite of deterministic functions over an acyclic
> 12-module dependency graph. This document proves that, names the algebra each module
> already implements, and ranks the four design changes the math forces.

This is the formal companion to the verified census in
[`05-determinism-proof.md`](05-determinism-proof.md) and the proof-by-execution in
[`07-determinism-certificate.md`](07-determinism-certificate.md). Where those count
375/375 live variants and run 36 plans byte-identical twice, this document explains
*why* that count is a theorem and not a coincidence.

**How to read it as a .NET dev.** Every Greek/blackboard symbol below maps to a C#
type you can open. `𝕊` is `Shape`, `𝕍` is `ValueExpression`, `ℝ` is `ReactionGraph`,
`ℂ` is `ConditionGraph`. A "kind-tagged sum" `Σ_{k∈K} P_k` is exactly a base class with
a `Kind => "x"` discriminator and one sealed subclass per `k` — the same thing the JSON
`kind` field and the TypeScript discriminated union encode. A function `f: A → B` is a
method; `f: A ⇀ B` (harpoon) is a method that is allowed to fail *only at a named
boundary*. `⊥` ("bottom") is "no proper value" — a thrown boundary error or a DOM miss,
**never** a normal `None`/`Null` value. Read the equations as the laws the C# already
obeys; read §6 as the bug list the laws expose.

---

## 1. Notation and the Law Schema

### 1.1 The alphabet

| Symbol | Carrier set | C# / runtime type |
|--------|-------------|-------------------|
| `𝕊` | Shape | `Shape` (`PlanModel/Shape.cs`) |
| `𝕍` | ValueExpression | `ValueExpression` |
| `ℂ` | ConditionGraph | `ConditionGraph` |
| `ℝ` | ReactionGraph | `ReactionGraph` |
| `ℛq` | RequestPlan | `RequestPlan` |
| `𝕋` | StartsWhen (trigger) | `StartsWhen` |
| `𝙲𝚘𝚖𝚙` | BrowserObject (component) | `BrowserObject` |
| `𝚅` | ValidationRuleNode | `ValidationRuleNode` |
| `𝕡` | Plugin member / contract | `Plugin`, `PluginContract` |
| `𝔻` | PlanDocument (aggregate root) | `PlanDocument` |
| `𝕁` | JSON values (the wire) | the serialized plan |
| `𝔼` | browser-effect monoid | DOM mutations, in order |

Lowercase letters range over elements: `s ∈ 𝕊`, `v ∈ 𝕍`. `K` is a finite set of `kind`
tags; `K_V`, `K_C`, `K_R`, … are the per-family tag sets.

### 1.2 Functions, partiality, and the discipline of `⊥`

- `f: A → B` is **total** (defined for every `a`).
- `f: A ⇀ B` is **partial**; `dom(f) ⊆ A` is where it is defined. `f(a)↓` means
  "converges" (defined), `f(a)↑` means "diverges" (undefined).
- Lift every carrier to `A⊥ = A ⊎ {⊥}`. `⊥` is the **single** "no proper value"
  element, used for two things only:
  1. divergence — `f(a) = ⊥ ⇔ f(a)↑`;
  2. genuine absence at a real boundary — `getElementById` miss, non-iterable source,
     missing payload scope, network down.

> **The `⊥` discipline (the formal statement of the project's "is this null really a
> bottom?" rule).** `⊥` is *only* divergence/boundary-failure. Domain absence that the
> plan deliberately represents is **not** `⊥` — it is a **named variant**:
> `Shape.None` (`𝗇𝗈𝗇𝖾`), `ValueExpression.Null()` `= (null, None)`, `RequestInput.None`,
> `ResponseRouting`'s `Terminal` chain, `NoDispatchPayload`, `AlwaysActiveValidationRule`,
> `PluginCommand`'s `None` return. Each of these is a *first-class inhabited element* of
> its carrier. `⊥ ⇔ undefined/boundary-failure`; a `𝗇𝗈𝗇𝖾`-style sentinel `⇔` an
> inhabited variant the algebra must reason about.

### 1.3 Sums, products, and the kind-tagged sum

- `A × B` — product (a record carrying both fields).
- `A ⊎ B` (or `Σ`) — disjoint/tagged sum.
- `𝒫(A)` — powerset; `A*` — finite sequences (the free monoid); `A ⇀_fin B` — finite maps.
- `[a₁,…,aₙ]` — a sequence; `⟨k: payload⟩` — a tagged value.

Every polymorphic plan family is a **kind-tagged sum**: a value is a pair
`(k, payload_k)` with `k ∈ K` and `payload_k` that arm's product. The tag `k` is one
symbol with three representations:

```
C# concrete type   ─┐
JSON "kind" string  ├─►  one tag  k ∈ K
TS union literal   ─┘
```

Concretely (the families this document proves):

```
𝕍 = Σ_{k∈K_V} P_k   K_V = {literal, read, object, array, array-op  (+ whole-payload, whole-element)}
ℂ = Σ_{k∈K_C} P_k   K_C = {compare, all, any, not, confirm}
ℝ = Σ_{k∈K_R} P_k   K_R = {set, call, dispatch, inject, show-validation-errors, sequence, branch, request, parallel}
𝕊 = Σ_{k∈K_S} P_k   K_S = {string, number, boolean, date, raw, any, none, array, nullable, object}
𝕋 = Σ_{k∈K_T} P_k   K_T = {page-ready, document-event, component-event, server-push, signalr}
```

### 1.4 The two seam functions, named once

- `ser : 𝔻 → 𝕁` — serialize (the C#→JSON boundary). **Total, deterministic.**
- `gen : NodeFamilies → TSContract` — the reflection/curation-driven `PlanContractGenerator`.

For each concept `C`:

- `lower_C` — the **lowerer**: authoring DSL `↝` plan node.
- `read_C` — the **reader**: plan node `⇒` value/effect.

"One write path, one read path" *is* "one `lower_C`, one `read_C` per concept." We write
`a ↝ b` for "`a` lowers to `b`" and `b ⇒ c` for "`b` is read back as `c`."

### 1.5 The law schema

Every module instantiates the same eight laws. Stating them once here means each module
section only needs to *cite which arms* satisfy them.

| Code | Law | The one-line invariant |
|------|-----|------------------------|
| **E** | Equivalence + congruence | `Equals` (`≅`) is an equivalence relation **and** a congruence: `x ≅ y ⇒ obs(x) = obs(y)` for every observation `obs ∈ {hash, ser, evaluate}`. |
| **M** | Merge = closed **union**, not a lattice join | Commutative + idempotent; identity `Any`; annihilator `None` (short-circuits **before** the `Any` branch); **non-associative** on nested nullables. Do **not** strengthen to a join. |
| **A** | Accept / compatibility | Reflexive; `Any` is simultaneously top and bottom; **not** transitive. Used pointwise at one call site, never as a closure. |
| **C** | Closed-union characterization | `¬accept(merge(s,t), s)` in general (`merge(object{a},object{b}) = object{a,b}` is a union, not an upper bound). But `s ≅ t ⇒ merge(s,t) ≅ s` — the only branch the collision-free-id invariant ever exercises. |
| **P** | aPply/convert (egress) | Total, **idempotent (shape-once)**, coherent: `apply(apply(v,s),s) = apply(v,s)`, and the Result engine agrees with the lenient engine on the success branch. |
| **S** | Serialize | Deterministic (`ser(x)` byte-identical on repeat), congruent (`x ≅ y ⇒ ser(x) = ser(y)`), injective-up-to-`≅`. |
| **F** | FromClrType / inference | Total + deterministic: every CLR type maps to exactly one `Shape`, computed **once** at authoring and ridden on the node. |
| **X** | eXhaustiveness over a Kind sum | (1) `lower_C` is **surjective** onto the live tags; (2) `read_C` is defined on **every** arm and ends in `assertNever(k)`; (3) **distinctness** — arms are structurally disjoint in `𝕁` (a `kind` can never collide with a camelCased member path). |

The headline structural facts are the **negative** laws, because they tell you what
*not* to assume:

```
M5 (negative)  ∃ a,b,c.  merge(merge(a,b),c) ≇ merge(a,merge(b,c))   — associativity is NOT a law
A4 (negative)  ∃ a,b,c.  accept(a,b) ∧ accept(b,c) ∧ ¬accept(a,c)    — witness (string, any, number)
C1 (negative)  ∃ s,t.    ¬accept(merge(s,t), s)                       — merge is a UNION, not a join
```

Each negative law is **safe** because of a domain invariant:

> **Merge-domain invariant.** Collision-free deterministic `IdGenerator` ids ⇒ same id ⇒
> same member ⇒ same `FromClrType` shape ⇒ `merge` always hits the `existing == incoming`
> fast path (`ShapeContractCompatibility.cs:20`, **verified**). So `M5`'s order-sensitive
> branch is robustness-only; the only branch ever exercised in practice is `C2`
> (`s ≅ t ⇒ merge(s,t) = s`).

> **Accept-domain invariant.** `accept` is called pointwise at one site (argument
> compatibility), never as a transitive closure, so `A4` is a quirk, not a defect.

---

## 2. The Module Category, Determinism, and the Composition Lemma

### 2.1 The category `𝓜`

**Objects** are the module carrier sets, lifted with `⊥`: `𝕊⊥`, `𝕍⊥`, `ℂ⊥ × 𝔹⊥`,
`ℝ⊥ × 𝔼`, `ℛq⊥ × Exchange`, `𝙲𝚘𝚖𝚙 × Contract`, `𝕋 × Ctx`, `𝔻`, `Rules × Report`,
`Contract × Catalog`, plus the DSL surfaces `DSL_C` and the runtime world `𝕁`/`𝔼` so that
**authoring and execution are morphisms in the same category.**

**Morphisms** come in three classes, all composable in one category:

1. **Pure-core** — total or partial functions with no effect: `FromClrType`, `merge`,
   `accept`, `apply`/`convert`, `ser`, `gen`, every `lower_C`, the `CompareEngine`, the
   `ArrayOpEngine`. These compose by ordinary function composition.
2. **Effect / seam** — typed boundary operations whose codomain folds the browser-effect
   monoid `𝔼`: `read_Component` (`getElementById` + vendor driver), the HTTP pipeline,
   `confirm`, `inject`, `wireTrigger`. Written `A → 𝔼 × B` (a Kleisli arrow over `𝔼`).
3. **The two clean-cut seams** as named morphisms: `ser : 𝔻 → 𝕁` and
   `gen : NodeFamilies → TSContract`, plus the four proven clean cuts
   (`domain ↦ serialize`, `serialize ↦ plan.ts`, `plan.ts ↦ runtime`,
   `evaluate ↦ apply ↦ formatForWire`).

**Composition.** Identity `1_A : A → A` for every object. For pure arrows,
`(g ∘ f)(a) = g(f(a))`. For effect/seam arrows it is **Kleisli composition** over the
monoid `(𝔼, ·, ε)`:

```
(g ∘ f)(a) = let (e₁, b) = f(a), (e₂, c) = g(b) in (e₁ · e₂, c)
```

This is associative because `𝔼` is a monoid, and `⊥` is **absorbing** (`⊥ · e = e · ⊥ = ⊥`),
which models exactly "a boundary failure halts the reaction." Identity and associativity
hold, so `𝓜` is a genuine category.

### 2.2 The dependency DAG is a functor constraint, not a cycle

The 12-module dependency graph is **acyclic** (`02-micro-modules.md:82-177`):

```
                         ┌─────────────────────────────────────────────┐
                         │                   Plan (root 𝔻)             │
                         └───┬──────┬──────┬──────┬──────┬───────┬──────┘
                  ┌──────────┘      │      │      │      │       └────────┐
               Trigger   ┌──────► Reaction ◄──────┤   Validation     Slot ──► Plan (down only)
                  │      │       │   │   │  │      │      │              │
                  └─►Component   │   │   │  └──► Request  │      Component│
                         │       │   │   └─────────┼──────┘              │
                         │   Condition  Value      │                     │
                         │       │       │ │       │                     │
                         └───────┴───►  Value ◄────┘                     │
                                         │ │                             │
                                       Shape ◄── Kind   (well-founded bottom)
```

A morphism in module `C` may invoke only morphisms of modules `C` depends on.
**Acyclicity is what makes composition well-founded** — no morphism is defined in terms
of itself through a cycle. This is the formal replacement for today's `boot ↔ browser-plans`
callback cycle and the sync-condition DI threading: `Slot → Plan` is a *downward* edge
(composition happens through the `Reaction.inject` handler, not an upward boot callback).

### 2.3 Determinism, defined

A morphism `f: A → B` is **deterministic** iff:

- **(D1) Single-valued.** `f` is a function, not a relation. For a kind-tagged family
  this is law **X(3) distinctness**: no input lowers to two outputs, and no two distinct
  inputs collapse to one arm. *(The `responseBody`/`elementValue` collision is precisely
  a D1 violation — see §6.1.)*
- **(D2) Total-on-its-domain.** `dom(f)` is an explicit, type-level set and `f(a)↓` for
  every `a ∈ dom(f)`. `f` may be partial **only** where the missing region is a true
  external boundary, and there `f(a) = ⊥` is the single, named, correct outcome — not a
  hidden fallback. Inside the generated plan graph `f` is total. This is law
  **X(1) + X(2)**.
- **(D3) Referentially transparent.** `f(a)` depends only on `a` — no hidden mutable
  state, no wall-clock, no randomness, no ambient singleton. *(This is what forces
  passing the `ActivePlan` explicitly rather than reading the `activeRuntimePlan`
  singleton — see §6.2.)*

> **Lane note.** Determinism is lane-agnostic. A sync morphism `A → B` and an async
> morphism `A → Promise B` are *both* deterministic iff D1–D3 hold. The lane
> (`sync`-void vs `async`-`Promise`) is a **plan-carried fact** (`ReactionLane`, stamped
> by the draft sequencer), so the runtime routes on the carried tag rather than
> re-detecting via `instanceof Promise`. Determinism of *which lane* is itself D1 over
> the lane tag.

### 2.4 The Composition Lemma

> **Lemma.** If `f: A → B` and `g: B → C` are deterministic, then `g ∘ f: A → C` is
> deterministic, with `dom(g ∘ f) = { a ∈ dom(f) : f(a) ∈ dom(g) }`.
>
> **Proof.**
> - *(D1)* `f`, `g` single-valued ⇒ for fixed `a`, `f(a)` is unique, hence `g(f(a))` is
>   unique ⇒ `g ∘ f` is single-valued.
> - *(D2)* By construction `a ∈ dom(g ∘ f) ⇒ f(a)↓ ∧ g(f(a))↓ ⇒ (g ∘ f)(a)↓`. Outside
>   that set the composite is `⊥`, and `⊥` absorbs under Kleisli composition (`⊥ · e = ⊥`),
>   so a boundary failure in `f` deterministically halts before `g` — no speculative
>   recovery.
> - *(D3)* `(g ∘ f)(a) = g(f(a))` is built only from `a` via two referentially-transparent
>   steps ⇒ referentially transparent. ∎

> **Corollary (whole-pipeline determinism).** Because every module morphism is
> deterministic and the 12-module dependency graph is **acyclic**, the end-to-end pipeline
> `lower ⨾ ser ⨾ gen-checked-transport ⨾ read` is a finite composite of deterministic
> morphisms and is therefore deterministic. This is the formal content of "375/375 live
> variants lower to exactly one plan JSON and one browser behavior." The only surviving
> non-determinism is, by D2, confined to explicitly named external boundaries
> (network presence, mounted-input set, live JS array source) — never to a DSL feature.

---

## 3. Per-Module Algebras

Each section gives the **carrier**, the **operations** with their defining equations, how
the operations relate to the **375 census**, and the **laws** with source anchors. Modules
are presented bottom-up along the DAG.

---

### 3.1 Shape (kernel)

**Carrier.** `𝕊 = Σ_{k∈K_S} P_k`, `K_S = {string, number, boolean, date, raw, any, none,
array, nullable, object}`. Scalar arms carry the unit `P_k = 𝟙` (kind alone). The three
recursive arms:

```
P_array    = 𝕊 ∖ {none}                         (item; none-item rejected at construction, Shape.cs:44-45)
P_nullable = 𝕊 ∖ {none}                         (inner; Shape.cs:65-66)
P_object   = (Name ⇀_fin 𝕊) × 𝔹                 (fields × the open/closed `additional` bit)
```

Absence is the named arm `𝗇𝗈𝗇𝖾 = ⟨none⟩` (`Shape.None`, `Shape.cs:38`), **not** `⊥`.

**Operations (defining equations).**

```
FromClrType(Nullable<T>) = nullable(FromClrType(T))     FromClrType(string) = string
IsDateType(τ)            ⇒ date                          IsNumericType(τ)    ⇒ number
TryGetCollectionItem(τ)=i⇒ array(i)                      else                ⇒ any        (Shape.cs:70-89)
FromValue(null) = none ;  FromValue(v) = FromClrType(v.GetType())                          (Shape.cs:96-97)

merge(s,s)                = s                            [== fast path]                    (ShapeContractCompatibility.cs:20)
merge(none, ·)            = conflict (⊥)                 [BEFORE the Any branch]           (:21)
merge(any, s)             = s ;  merge(s, any) = s                                         (:22-23)
merge(nullable(a), a)     = nullable(a)                  [single-level collapse]           (:24-25)
merge(array(a),array(b))  = array(merge(a,b))                                              (:26)
merge(object{f},object{g})= object{f ∪ₚₜ g} or open-object on field conflict              (:27)
else                      = ⊥ (Conflict)

accept(s,s)=true ; accept(any,·)=accept(·,any)=true ; accept(·,none)=false ; structural recursion (:31-43)
```

`apply ≡ applyShape : 𝕁⊥ × 𝕊 → 𝕁⊥` is the egress converter
(`shape-convert.ts:24-93`); `convert ≡ convertByShape` is its `Result`-returning sibling.

**Verified** (this pass): `ShapeContractCompatibility.cs:12-29` matches the equations
exactly — the `None` annihilator is tested **before** the `Any` identity, and the
nullable collapse is single-level.

**Census tie.** Shape is the type-tag **kernel** ridden by every one of the 375 census
variants — it is not its own census band but the codomain of `FromClrType`/`FromValue`
invoked at each literal (Values 05:300-305), each typed read shape (`P-SHAPE × 10`), each
gather assignment, each compare operand, each contract member, each validation
`comparisonShape`. The literal-arbitrary row's inferred shape *is* the complete 10-arm
`FromClrType` table — one CLR type ↦ exactly one `𝕊` arm.

**Laws.**

- **E1–E4** — `Equals` is reflexive/symmetric/transitive (`Shape.cs:291-295`) **and**
  congruent for `hash` (`:301-314`) and `ser` (`:11-23`). Proven (07:43-44).
- **M1** idempotent (`== fast path`), **M2** commutative (symmetric arms), **M3** identity
  `Any` (with the `None` exception), **M4** annihilator `None`, **M5** non-associative
  (single-level nullable collapse, `Shape.cs:427`). Safe under the merge-domain invariant.
- **A1** reflexive, **A2** `Any` top & bottom, **A3** `None` edge, **A4** not transitive
  (witness `string ⟶ any ⟶ number`).
- **C1** closed-union (`merge(object{a},object{b}) = object{a,b}`), **C2** `s ≅ t ⇒
  merge(s,t) = s`.
- **P1** total/lenient, **P2** idempotent shape-once (spy = 1, 07:23), **P3** coherent.
- **S1–S3** deterministic / congruent / injective-up-to-`≅` (`additional` byte
  disambiguates open vs closed-empty, `:558`).
- **F1** total (`Shape.cs:88` `else any`), **F2** deterministic.
- **X** — `applyShape` ends in `const _: never = shape` (`shape-convert.ts:36-39`);
  `ContractDriftGate` keeps the TS `Shape` union 1:1 with `K_S`.

**Anchors.** `Shape.cs:30-118, 289-314, 427`;
`ShapeContractCompatibility.cs:12-43`; `shape-convert.ts:24-93`;
`07-determinism-certificate.md:38-85`.

---

### 3.2 Kind (kernel)

**Carrier.** `K = ⨄_{family} K_family` — the finite universe of `kind` discriminators
across all polymorphic plan families (18 `WriteOnlyPolymorphic`/`PlanNodeDiscriminator`
bases). `𝕁` = the camelCase JSON alphabet. The morphisms live **between** carriers:
`ser : 𝔻 → 𝕁` and `gen : NodeFamilies → TSContract`. Each value of a family is the pair
`(k, payload_k)`, `k` emitted verbatim.

Census of live tags: **55** C# `Kind` literals (07:21); `plan.ts` carries 114 `kind:`
literal occurrences across 137 interfaces. `⊥` = a kind present in C# but absent in TS
(drift) — made unrepresentable by `ContractDriftGate`, never inhabited.

**Operations.**

```
ser       : 𝔻 → 𝕁              delegates to the concrete type's  Kind => "x"  getter
gen       : NodeFamilies → TSContract   one union per base, one interface per concrete node (CURATED, not raw reflection)
driftCheck: TSContract × NodeFamilies → 𝔹   build-time gate failing on any C# kind absent from TS
assertNever: never → ⊥         the runtime exhaustiveness guard in every reader's switch default
```

**Census tie.** Kind is the discriminator kernel: the 375 census variants each lower to
exactly one `(k, payload_k)` and the JSON `kind` string is the **sole** discriminant.

**Laws.**

- **X(1) surjectivity** — `gen` enumerates exactly the tag set; all 55 C# kinds present in
  TS, `comm -23` empty (07:21).
- **X(2) reader-totality** — every switch ends in `assertNever(k)`; `npm run typecheck`
  exits 0 ⇒ switches exhaustive (07:22).
- **X(3) distinctness** — arms structurally disjoint in `𝕁`; a kind string can never
  collide with a camelCased member path. *(The `responseBody` collision was a D1
  violation — §6.1.)*
- **S1** — 36 public-DSL plans render byte-identical twice (07:20); regenerating `plan.ts`
  is byte-identical to committed (07:21). **GEN-COHERENCE**: every C# `kind` literal has a
  matching TS arm.
- *Negative* — Kind adds **no** ordering on tags. It is a flat set; no tag subsumes another.

**Anchors.** `PlanContractGenerator.cs:10-123`; `ContractDriftGate.cs`;
`PlanSerializer.cs`; `core/assert-never.ts`; `types/plan.ts`;
`07-determinism-certificate.md:18-23`.

---

### 3.3 Value — the value spine (`TypedSource ↝ ValueExpression ⇒ evaluateValue`)

**Carrier.** `𝕍 = Σ_{k∈K_V} P_k`. Shipped: `K_V = {literal, read, object, array, array-op}`;
redesign (after Fix 1): `+ {whole-payload, whole-element}`.

```
P_literal  = 𝕁⊥ × 𝕊                          (LiteralExpression; Null() = (null, None))
P_read     = Source × MemberName × Path × 𝕊 × Access
             Source = component ⊎ plugin ⊎ url ⊎ payload ⊎ dom        (the 6-element P-SOURCE axis)
             Access = property ⊎ method(𝕍*)                            (the recursion makes 𝕍 an inductive term carrier)
             Path   = PathSegment*                                     (free monoid)
P_object   = (Name ⇀_fin 𝕍) × 𝕊
P_array    = 𝕍* × 𝕊
P_array-op = Op × 𝕍(source) × 𝕊(item) × 𝕊(out) × ℂ⊥(predicate) × 𝕍⊥(projection)
             Op ∈ {count, filter, map, sum, any, all, find, orderBy, orderByDescending}
```

Named absence (not `⊥`): `Null` literal `= (null, none)`; array-op `Predicate`/`Projection`
absence is the missing arm of the `ℂ⊥`/`𝕍⊥` lift (`JsonIgnore`-when-null,
`ArrayOperationExpression.cs:556,565`). `⊥` = a read whose `Source` resolves to no live
object (`getElementById` miss, `evaluate.ts:172-173`) or a non-iterable array-op source.

**Operations.** `lower_literal`/`lower_read`/`lower_whole`/`lower_object`/`lower_array`/
`lower_arrayop` lower DSL to `𝕍` (`ValueExpression.cs:19-198`); `ser : 𝕍 → 𝕁` is the
write-only emit; `read_value ≡ evaluateValue : 𝕍 ⇀ 𝕁⊥` (`evaluate.ts:40-67`) dispatches:

```
literal   ⇒ applyShape(value, shape)                              (:43)
read      ⇒ evaluateRead → {component/plugin: RuntimeObject.read|call; url: params.get;
                            payload: RuntimePath.read(root); dom: getElementById + RuntimePath}
object    ⇒ map fields recursively, usingDeclaredShape
array     ⇒ items.map(evaluate), shaped
array-op  ⇒ runArrayOp(...)
default   ⇒ assertNever(expression)                               (:67)
```

**Census tie.** Value-module live count = 5 literals/composites + 19 array-ops = 24,
plus the read template (counted once, instantiated 8× under the read axis). Per Fix 1,
`whole-payload`/`whole-element` become 2 further distinct arms.

**Laws.** **E** (structural `Equals`, congruent for `ser`); **F** (each leaf gets its
`Shape` once via the kernel oracle, never recomputed); **P** (every `read_value` leaf
funnels through `applyShape` exactly once — `evaluate.ts:43,176,195`; spy = 1, 07:23);
**S** (kind verbatim; `plan.ts:724-856` enumerates exactly `K_V`); **X** (switch ends in
`assertNever`, every arm + every `Source` covered).

> **The one live D1 violation.** Today `read_value` discriminates on
> `expression.member === "responseBody"`/`"elementValue"` (**verified this pass**,
> `evaluate.ts:199,203`), so a property *literally named* `ResponseBody` camelCases to the
> sentinel and collides with a whole-payload read: two inputs ↦ one wire member ↦ one
> behavior. **Resolved by Fix 1** (§6.1) — promote whole-reads to distinct kinds.

*Negative* — there is **no** merge/lattice on `𝕍`. Value composition is the free term
constructors `object`/`array`; do not impose associativity or `accept()` on values.

**Anchors.** `ValueExpression.cs:13-198, 272-589`; `Source.cs:10-152`;
`evaluate.ts:25-204`; `plan.ts:724-856`; `04-matrix-http-arrays-values.md:55-128`;
`05-determinism-proof.md:76-138, 298-394`.

---

### 3.4 Condition — the predicate spine (`When/Then/ElseIf/Else ↝ ConditionGraph`)

**Carrier.** `ℂ = Σ_{k∈K_C} P_k`, `K_C = {compare, all, any, not, confirm}`.

```
P_compare = 𝕍(left) × CompareOp × ComparisonRightOperand × 𝕊 × 𝕊(itemShape)
            ComparisonRightOperand = Present(𝕍) ⊎ Absent           (Absent = the NAMED unary "none" operand)
            CompareOp ∈ the 21-token set, partitioned into 9 families  (verified: 21 tokens, CompareOp.cs:11-42)
P_all/P_any = ℂ*           P_not = ℂ           P_confirm = string(message)
```

The **sync subset** `ℂ_sync = Σ over {compare, all, any, not} ⊂ ℂ` is the array-op
predicate carrier and the validation carrier (`plan.ts:872-882`, no `confirm`). `confirm`
is the **only** arm that lifts `𝔹 → Promise⟨𝔹⟩`.

**Operations.** `lower_compare`/`lower_all`/`lower_any`/`lower_not`/`lower_confirm`
(`ConditionGraph.cs:17-31`); `read_sync ≡ evaluateSyncCondition` — the **one** pure
`CompareEngine` (`compare-engine.ts:62-80`); `read_compare ≡ evaluateCompare` — one switch
arm per of the 9 operand-shape families, ends in `assertNever` (`:84-156`); `read_lane ≡
evaluateConditionInLane` — the async wrapper that delegates the sync subset and adds
`confirm` (`conditions.ts:41-121`).

**Census tie.** 52 variants (05:280-294): 6 left-source kinds + element-scope member/method
reads; 21 compare tokens in 9 families; 6 source-vs-source operand forms; 9 guard
And/Or/nested/Not; 6 branch positions; 2 confirm forms. Source-vs-source is one extra
`OperandForm` axis (right is a `read` `𝕍`), **not** a new family.

**Laws.** **E**, **P** (shape-once on left `:165` and right `:190`), **S** (Present/Absent
disambiguated by its own `kind` byte), **X** (`read_sync`, `read_compare`, `read_lane` each
end in `assertNever`; all 21 ops covered; the 21 tokens come from **one** `CompareOp`
source).

> **Order law (first-match + confirm-first).** `all`/`any` evaluate left-to-right
> (`.every`/`.some`); branch cases evaluate top-to-bottom, first match wins, `Else` is
> always last/once. `Confirm` composed via `And` is flattened to index 0
> (`GuardBuilder.cs:81-85`), so the dialog **always** opens before any later compare —
> "compares short-circuit ahead of the dialog" is proven impossible (04-conditions:344,379).

> **Lane law (D1 over the lane tag).** A branch whose every guard is in `ℂ_sync` is stamped
> sync; a branch reaching `confirm` is stamped async. The lane is plan-carried; one
> `CompareEngine` serves both. `Standalone.Then` is **unrepresentable** in the redesign — a
> compile error replaces the old runtime throw.

*Determinism note.* Un-orderable/non-text operands yield deterministic `false`
(`compare-engine.ts:311,329,389`), **not** `⊥`; the only `⊥` is a missing confirm dialog
(`conditions.ts:113-115`).

**Anchors.** `ConditionGraph.cs:13-251`; `CompareOp.cs:11-42`; `compare-engine.ts:62-394`;
`conditions.ts:28-121`; `04-matrix-triggers-reactions-conditions.md:268-411`;
`05-determinism-proof.md:278-294`.

---

### 3.5 Reaction

**Carrier.** `ℝ = Σ_{k∈K_R} P_k`, `K_R = {sequence, parallel, branch, set, call, request,
dispatch, inject, show-validation-errors}` (9 arms; `plan.ts:471-480`).

```
P_sequence = ℝ*                                              (free monoid)
P_parallel = ℝ* × ParallelCompletion,  ParallelCompletion = ⟨none⟩ ⊎ ⟨on-settled⟩ × ℝ
P_branch   = BranchCase*,  BranchCase = BranchGuard × ℝ,  BranchGuard = ⟨default⟩ ⊎ ⟨when⟩ × ℂ
P_set      = Source(on) × MemberName × 𝕍                     Source ∈ {component, payload}
P_call     = Source × MemberName × 𝕍*                        Source ∈ {component, plugin, payload}
P_request  = ℛq
P_dispatch = EventName × DispatchPayload,  DispatchPayload = ⟨none⟩ ⊎ ⟨value⟩ × 𝕍 × PayloadContract
P_inject   = ComponentKey × 𝕍                                (value fixed = ReadWholePayload(Success))
P_show     = ComponentId
```

Each node carries a `ReactionLane ∈ {sync, async}` (plan-carried). No null carrier: "no
payload" is `⟨none⟩`, "no completion" is `⟨none⟩`, "no branch match" is the empty
fall-through. `⊥` only at a boundary halt (`⊥` absorbs under Kleisli composition).

**Operations.** `lower_Reaction` is the **sequencer** `ReactionPipelineDraft.BuildReaction`
(`:52-58`): a run of sync commands collapses to **one** `SequenceReaction`
(`FlushPendingSyncReactions`, `:82-88`); an async opener becomes a `Request`/`Parallel`
**Kind** arm (the lane stamp, `:14-30`). `read_Reaction ≡ executeReaction` (`execute.ts:60-101`)
switches on `kind`, ending in `assertNever(reaction)`; lane equations:

```
read(set | call | dispatch | inject | show-validation-errors) = void
read(request) = Promise ;  read(parallel) = Promise (Promise.allSettled)
read(sequence) = void until a step crossesAsync, then .then-chains the remainder
read(branch)   = first guard whose branchGuardMatches is true (author order)
```

**Census tie.** 28 variants (05:236-276): Element show/hide/AddClass/…/SetText×4/SetHtml×3
are Set/Call arms differing only in their `𝕍` source; Dispatch×3 over the `DispatchPayload`
sub-sum; `Into → inject`; `ValidationErrors → show-validation-errors`. A row is
`(ReactionKind × TargetSource × ValueSource)`.

**Laws.** **X** (switch ends in `assertNever`, `:99`; 9 arms covered — *note* the inner
`executeSet`/`executeCall` `on.kind` switches lack `assertNever`, total-by-enumeration
today, `:162,178`, flagged 07:34-36); **S** (kind verbatim; 36 plans byte-identical);
**Sequence-collapse** (`ℝ*` is the free monoid under append, identity `[]`; *not* the
commutative Shape-merge — order is significant); **First-match** (returns on first matching
guard; `Else` last/once; no-match = silent no-op, not `⊥`); **Inject shape law** (value is
always `ReadWholePayload(Success)`; a non-string evaluated value throws a typed shape error
at egress, a boundary).

> **No merge/accept/apply/FromClrType algebra applies to `ℝ`.** Reactions are not unioned,
> composed by `PlanId`, or shaped. Composition of reactions is *sequencing* — the
> free-monoid collapse — not the closed-union merge of Shape. Asserting M/A/C/P/F here
> would be a false law.

> **D3 gap (flagged for removal).** `read_Reaction` is referentially transparent *except*
> for the mutable `activeRuntimePlan` singleton and `crossedAsyncBoundary`'s
> `instanceof Promise` lane re-detection — **verified this pass** at `execute.ts:28-42`
> and `execute.ts:287`. The redesign removes both (§6.2).

**Anchors.** `ReactionGraph.cs:13-456`; `ReactionPipelineDraft.cs:14-147`;
`execute.ts:60-353`; `plan.ts:471-577`; `05-determinism-proof.md:236-276`;
`04-matrix-triggers-reactions-conditions.md:116-264`.

---

### 3.6 Request

**Carrier.** `ℛq = RequestEndpoint × RequestInput × RequestReactions × ResponseRouting ×
RequestValidationTarget`.

```
RequestEndpoint   = HttpMethodName × RequestUrl          HttpMethodName ∈ {GET, POST, PUT, DELETE}
RequestInput      = ⟨none⟩ ⊎ ⟨gather⟩(𝔸* × RequestBodyFormat × RegisteredInputSelection)
                    𝔸 = RequestInputTarget × 𝕍,  RequestInputTarget ∈ {payload, header, route-param}
                    RequestBodyFormat ∈ {json, form-data}
ResponseRouting   = ResponseRoute*(success) × ResponseRoute*(error) × RequestChain
                    ResponseRoute = ResponseStatusMatch × ℝ,  ResponseStatusMatch = ⟨any⟩ ⊎ ⟨status⟩
RequestChain      = ⟨terminal⟩ ⊎ ⟨follow-up⟩ × ℛq         (RECURSIVE in ℛq)
RequestValidationTarget = ⟨none⟩ ⊎ ⟨container⟩ × ComponentId
```

`read_Request : ℛq → 𝔼 × Promise⟨void⟩` — **always async** (the one async lane). Named
absence (not `⊥`): `RequestInput.None`, `RequestValidationTarget.None`, `RequestChain.Terminal`.
`⊥` reserved for boundaries: route-param `= null` (throws "cannot build URL"), network
failure folded to the named outcome `response-unavailable`, non-string `Into` egress.

**Operations.** `lower_Request ≡ HttpRequestBuilder.BuildRequest`; gather assignment
lowerers (`Include`/`Static`/`FromEvent`/`Header`/`RouteParam`/`FromUrl`/`Plugin`) over the
shared value spine; `read_Request` = the fixed pipeline `requestCanSend ⨾
runRequestReactions(whileLoading) ⨾ resolveInput ⨾ fetch ⨾ routeOutcome ⨾ finally ⨾ chain`.

> **Response-selection law (verified this pass, `http.ts:263`).**
> ```
> routeResponseRoutes(routes, status) = routes.find(matchesStatus(status))
>                                       ?? routes.find(matchesAnyStatus)
> ```
> This is **exact-status-preferred-then-first-any**, *not* positional first-match: an
> any-status route authored *before* `OnError(404)` still loses to `404` on a 404. Single-
> valued: one status ↦ one route.

**Census tie.** 47 variants (05:310-367): 3+ verbs, URL template, 11 gather targets, 4
body-egress forms, 6 response routes + network-failure, chained, parallel, WhileLoading,
Finally, Validate, Into.

**Laws.** **X** (every sub-sum closed, every reader ends in `assertNever`); **S** (kind
verbatim, fixed key order); **P** (shape-once egress — `formatForWire` called once per value,
`request-payload-writer.ts:85,99,111`; `jsonBodyValue('') = null` is the one named
cleared-field policy); **Chained-on-success-only + Finally-always**; **D2** with `⊥`
confined to the named network/route-param/Into boundaries.

> **No merge/accept/FromClrType algebra on `ℛq` itself.** The only compose-style operation
> is response routing's single-chain accumulation (`ContinueWith` rejects a second chain) —
> a guarded assignment, not a commutative/idempotent merge.

**Anchors.** `RequestPlan.cs:8-274`; `RequestInput.cs`/`GatherRequestInput.cs`;
`http.ts:28-306`; `gather.ts:37-176`; `request-payload-writer.ts:32-224`;
`05-determinism-proof.md:310-367`; `04-matrix-http-arrays-values.md:132-256`.

---

### 3.7 Trigger

**Carrier.** `𝕋 = Σ_{k∈K_T} P_k`, `K_T = {page-ready, document-event, component-event,
server-push, signalr}`.

```
P_page-ready      = 𝟙                              P_document-event = EventName × PayloadContract
P_component-event = ComponentKey × EventName       (NO payload-contract arm — can never collide with payload-bearing arms)
P_server-push     = RequestUrl × EventFilter,  EventFilter = ⟨any⟩ × PayloadContract ⊎ ⟨named⟩ × EventName × PayloadContract
P_signalr         = RequestUrl × MemberName × PayloadContract
PayloadContract   = ⟨untyped⟩ ⊎ ⟨typed⟩ × TypeFullName    (Untyped is the absence VARIANT, never ⊥)
```

A `Behavior = 𝕋 × ℝ`; a `BehaviorGraph = Behavior*` (append-only free monoid). The runtime
trigger payload object is the `ExecutionContext`; its `absent()` element is the deliberate
named no-context variant for page-ready, **not** `⊥`.

**Operations.** `lower_Trigger ≡ Html.On + TriggerBuilder` pairs `StartsWhen × ℝ` into one
`Behavior`; `read_Trigger ≡ wireTrigger : 𝕋 × ℝ × 𝔻 → 𝔼` routes on `StartsWhen.kind`,
ending in `assertNever`, feeding the originating payload into **one** `ExecutionContext`;
`resolvePayload` folds 7 scopes onto backing fields (`execution-context.ts:53-73`).

**Census tie.** 10 variants (05:213-234): a row is `(TriggerKind × PayloadContract)` — 5
kinds × {untyped, typed}, page-ready having no payload axis.

**Laws.** **X** (`wireTrigger` ends in `assertNever`; `EventFilter` and `PayloadContract`
guarded); **D1** (one `t.<Kind>(args, pipeline)` ↦ one `Behavior` ↦ one listener);
**BehaviorGraph monoid** (append-only, wiring order = author order, with the single
deliberate page-ready deferral so document-event listeners exist before page-ready
dispatches); **Lane law** (page-ready/document-event/component-event are sync openers;
server-push/signalr are async openers).

> **D3 note (carried connection-pool fact).** `wireServerPush`/`wireSignalR` keep
> module-level connection pools keyed by url (`server-push.ts:27`, `signalr.ts:23`), so
> they are stateful at the effect layer — deterministic per url, but a real external
> resource, not a pure morphism.

> *No merge/accept algebra.* Triggers are never merged; `Behavior` append is
> multiset-accumulation (duplicates kept), **not** idempotent union.

**Anchors.** `StartsWhen.cs:8-152`; `Behavior.cs:5-18`; `BehaviorGraph.cs:6-33`;
`PlanTerms.cs:397-447`; `trigger.ts:21-105`; `execution-context.ts:10-89`;
`05-determinism-proof.md:213-234`; `04-matrix-triggers-reactions-conditions.md:82-112`.

---

### 3.8 Component

**Carrier.** `𝙲𝚘𝚖𝚙 = Id × Ven × ObjId × Role × Bind × CBind`.

```
Id    = ComponentId    (IdGenerator,  {Ns_Type}__{MemberPath})
Ven   = ComponentVendor ∈ {native, fusion, …}
ObjId = BrowserObjectId  ((vendor, kind, id) join token: native.element.{id} ⊎ {vendor}.component.{id} ⊎ plugin.{name})
Role  = Σ {object-target, plan-input, validation-container, layout-object}
Bind  = ⟨none⟩(NoInputBinding) ⊎ ⟨registered-input⟩ × BindingPath × Path × MemberName
CBind = ⟨none⟩ ⊎ ⟨validation-container⟩ × ContainerScope
```

Contract `𝙲 = (Name ⇀_fin Property) × (Name ⇀_fin Method) × (Name ⇀_fin Event)`, each
member shape `∈ 𝕊`. `Bind.none`/`CBind.none` are **named** inhabited variants, distinct
from `⊥` (the `getElementById` miss = `RuntimeResolutionError`).

**Operations.** `idFor : CLRType × PropExpr ⇀ Id` (deterministic, collision-free);
`declareElement`/`declareInputComponent`/`declareLayoutObject`; `withBindingIfAbsent`
(first-registration-wins, idempotent); `mergeProperty`/`mergeShape` (the Shape closed
union); `widenAccess` (the **one genuine semilattice** — idempotent + commutative + top
`readwrite` on the 3-element access set); `resolve`/`read`/`set`/`call` (effect/seam,
memoized).

**Census tie.** 14 template families (05:424-439): B1 input render+registration ×2 + 1
unregistered-throw; B2 set/call/set-from-source/read ×4; B3 component-event + input-event;
B4 grid render/DataStateChange/mutation/inline-validation. The 58 `.Reactive()` overload
lines across 60 slices are B3 *instantiations* parameterized over each slice's TypedEvent
set — elements of B3's domain, not new templates.

**Laws.** **E** (Id value-equality), **F** (binding `Shape = FromClrType` once),
**M** (contract merge = closed union, M1–M4; M5 not asserted), **M-Access** (the lone
legitimate join), **A** (reflexive, pointwise, not transitive), **X** (`Role`/`Bind`/
`MethodArgumentContract` closed; `prepareMethodArguments` ends in `assertNever`).

> **The id law (D1) is the keystone invariant.** `IdGenerator.For(τ, expr)` is a
> deterministic function — same expression ⇒ same id across vendors, collision-free by
> construction. *This is the merge-domain invariant that makes Shape's M1/C2 fast path the
> only branch ever exercised.*

> **Vendor-isolation law (LSP).** No vendor check downstream of `ComponentDriver` — a third
> vendor touches only the driver registry + `event-{vendor}.ts`.

**Anchors.** `IdGenerator.cs:29-78`; `BrowserObject.cs:8-157`; `BrowserObjects.cs:30-160`;
`BrowserObjectContract.cs:18-361`; `ShapeContractCompatibility.cs:12-43`;
`PlanTerms.cs:238-278`; `runtime-plan.ts:13-168`; `runtime-object.ts:14-56`;
`resolver.ts:10-25`; `04-matrix-validation-components-slots.md:187-263`;
`05-determinism-proof.md:424-439`.

---

### 3.9 Validation

**Carrier.** `𝚅 = ValidationRuleNode = RuleName × ValidationMessage × ValidationRuleExecution`.

```
RuleName = Known, |Known| = 18    {required, empty, minLength, maxLength, email, regex, url, creditCard,
                                    range, exclusiveRange, min, max, gt, lt, equalTo, notEqual, notEqualTo, atLeastOne}
ValidationRuleExecution = ⟨none⟩(NoOperand) ⊎ ⟨constraint⟩(LiteralExpression) ⊎ ⟨peer⟩(ReadExpression)
                          each arm carries Activation × comparisonShape(𝕊)
ValidationRuleActivation = ⟨always⟩ ⊎ ⟨when⟩ × ℂ
```

The runtime narrows **18 RuleName → 6 evaluation families** {no-operand, length, regex,
range, ordered-comparison, equality} (`rule-engine.ts:57-99`). `⊥` only at the
unmounted/hidden-field boundary (orchestrator skips, field stays valid) and the authoring
boundary (`ClientRule` inside a server `When` throws).

**Operations.** `clientRule`/`addNoOperand`/`addLiteral`/`addRange`/`addPeerComparison`;
`lower_Operand` (asserts `LiteralExpression`/`ReadExpression`, else `⊥`); `combineActivation`
(`Always.Combine(x) = x`, `Conditional(c).Combine(x) = When(All(c, x.cond))`); `prefix`
(nested child validator path-join); `narrow` (the gen-derived 18→6 map); `ruleFails`
(pure core, ends in `assertNever`); `isRuleActive` (reuses Condition's `CompareEngine`);
`readSubject` (reuses the Value spine).

**Census tie.** 102 variants (05:396-422): 31 builder methods, 38 static paired overloads,
24 WhenField + WhenFields, ClientRuleEach + AtLeastOne/SetValidator, nested ClientRule,
ClientRulesFrom×2, server-errors/inline/summary.

**Laws.** **X** (`RuleName.From` surjective/throws on unknown; `fieldRuleFails` +
`peerTargetRuleFails` end in `assertNever`; the 18→6 narrowing is *generated* from C#,
`ContractDriftGate` fails on drift); **F** (`comparisonShape` once; range bounds require
equal inferred `𝕊`); **M** (nested child merge = the Component closed union; activation
combination is commutative-idempotent nesting, `Always` is identity, **not** a join);
**S** (kind verbatim, write-only).

> **D1 — do NOT collapse the asymmetry.** `notEqual` (literal-only) and `notEqualTo`
> (peer-only) route to distinct arms (`rule-engine.ts:75-76` literal / `:97-98` peer).
> Collapsing them would create a many-to-one D1 violation (04-validation:132).

> **One-engine law.** Activation reuses Condition's single `CompareEngine`; the peer read
> uses the single Value spine — no second evaluator, no second resolver.

**Anchors.** `ValidationTerms.cs:91-108`; `ClientValidationFieldRuleBuilder.cs:27-235`;
`ValidationRuleNode.cs:8-56`; `ValidationRuleExecution.cs:5-122`;
`ValidationRuleActivation.cs:7-61`; `RuleOperand.cs:8-244`; `rule-engine.ts:46-120`;
`orchestrator.ts:14-113`; `04-matrix-validation-components-slots.md:64-184`;
`05-determinism-proof.md:396-422`.

---

### 3.10 Plugin

**Carrier.** `𝕄_plugin = Σ_{k∈K_M} P_k`, `K_M = {property, function, command}`.

```
P_property = PluginName × MemberName × 𝕊            (shape = FromClrType(TValue))
P_function = OpId × 𝕊(returns) × 𝕊*                  (returns = FromClrType(TReturn))
P_command  = OpId × {None} × 𝕊*                      (returns ≡ Shape.None — the named void variant, not ⊥)
ObjectMemberKey = MemberName ⊎ {RootCall}            (RootCall is FIRST-CLASS, not ⊥)
PluginContract  = PluginName × 𝒫_fin(P_property) × 𝒫_fin(P_op)
𝔸               = the image of PluginInvocationArgument ⊆ 𝕍   (every Arg lowers over the shared Value spine)
```

`resolve : Name → (𝔼_cat)⊥` — a catalog miss is the one genuine `⊥` (external edge,
`plugin-catalog.ts:22`).

**Operations.** `lower_decl` (Property/Function/Command, member or root); `Arg-append`
(arity sugar is `Function<R>(m).Arg<A1>().Arg<A2>()` — arity fully encoded in `args[]`);
`toContract`/`registerPlugin`/`toBrowserObjectContract`; `accept_arg` (pointwise,
non-transitive A4); `read_plugin`/`call_plugin` (route through the Value `read` arm and the
Reaction `call` arm, both over `PluginSource`).

**Census tie.** 65 variants (05:454-469): `RegisterPlugin` ×3 (all land **one** contract
per name), ~31 arity declaration overloads (collapse to **one** args-builder),
`PluginReadBuilder.Arg` ×11, `PluginCallBuilder.Arg + Fire` ×12, `Plugin<T>` read/call ×8.

**Laws.** **X-Kind** (`lower_decl` surjects onto `K_M`; runtime split
`{function(returns≠None), command(returns=None)}` decided by the returns shape); **F**
(every shape rides `FromClrType` once); **C2 / dedup** (re-registered same-contract member
is a no-op; a different contract throws — closed-union accumulation, **not** a join);
**A** (reflexive, pointwise; open contract is the `Any` wildcard); **D1** (every arity
overload reaches the same `(member, returns, argShapes)` triple; the `plugin` source kind
is structurally disjoint from component/url/dom/payload).

> **Merge-by-equality (a design discovery).** The two ~95%-identical builders
> (`PluginReadBuilder`/`PluginCallBuilder`) and the two declaration APIs are extensionally
> equal on `(member, returns, argShapes)` ⇒ the algebra mandates merging them (§6.4).

> **Boundary law.** Stringly names are allowed **only** at the plugin name/member boundary;
> arguments stay typed. The typed/stringly seam is the plugin escape hatch, not a general
> one.

**Anchors.** `Plugin.cs:15-280`; `PluginContract.cs:6-262`; `PluginMemberBuilder.cs:38-255`;
`PluginArguments.cs:9-87`; `PluginTypeBuilder.cs:13-216`; `TypedPluginSource.cs:21-43`;
`Source.cs:101-107`; `plugin-catalog.ts:9-24`; `05-determinism-proof.md:454-469`;
`04-matrix-validation-components-slots.md:304-352`.

---

### 3.11 Slot

**Carrier.** Two faces of one carrier `𝔻` under composition:

```
PlanScope        = ⟨root⟩ ⊎ ⟨partial⟩           (the SSR-join discriminant)
𝔻                = ℕ(version=3) × PlanId × PlanScope × Types × Components × BehaviorGraph
emptyPlan(id)    = ε_id = (3, id, ⟨root⟩, ∅, ∅, [])    — the IDENTITY element, NAMED empty, not ⊥
PartialSlotLoad  = AbortController × 𝔻*
Σ_state          = (active: PlanId⇀𝔻) × (boot: PlanId⇀𝔻) × (slots: SlotId⇀PartialSlotLoad)
```

A `planId` with no boot snapshot and no slot plans is *deleted* from active — absence is a
missing map key; an injected HTML carrying zero plans is the deliberate **unload** signal.

**Operations.** `scope` (`ReactivePlan ↦ root`, `ResolvePlan ↦ partial`, same `PlanId`);
`ser ≡ RenderPlan`; `discover`; `composeBootPlanInto`/`composeSlotPlanInto` — the **one**
`⊕`/MergePolicy (**verified this pass**, `merge-policy.ts:9-18`): types merge via
`mergeObjectContracts`, components join/replace by key, **behaviors append**;
`composeInitialPlans` (the SSR-join fold from `ε_id`); `recomposePlan` (resets target to
`ε_id` then re-folds boot + slots from scratch); `loadPartialSlot`/`unloadPartialSlot`/
`injectPartial`.

**Census tie.** 5 variants (05:443-451): root view plan, same-model partial (SSR join),
independent-model partial, browser slot load, browser slot unload.

**Laws.** **M** (type-contract face is the Shape closed union, M1/C2; `ε_id` is the merge
identity); **X** (`PlanScope` is the closed 2-arm union); **D2** (recompose total; unload of
an unloaded slot is a named no-op).

> **Recompose-idempotence invariant — the headline Slot law.** The `behaviors` field
> *appends* (`⊕` is **not** idempotent as a whole, since re-merging duplicates behaviors).
> Idempotence is restored at the **recompose boundary**: `recomposePlan` resets to `ε_id`
> and re-folds from scratch every time, so it is a pure **function** of
> `(boot[id], slotsFor(id))` and `recompose ∘ recompose = recompose`. This is the formal
> content of "build a NEW `PlanDocument` on recompose, not in-place mutation": the boot
> snapshot is never mutated under a running plan (D3).

> **M2 non-commutativity (witness).** `ε ⊕ p₁ ⊕ p₂` has behaviors `[b₁, b₂]`; `ε ⊕ p₂ ⊕ p₁`
> has `[b₂, b₁]`. Determinism is preserved because the fold *order* is itself a
> deterministic function of discovery/slot-insertion order — a carried fact, not
> commutativity (the document-level instance of M5).

> **Slot dependency law (acyclic).** `Slot → Plan` is downward-only; composition is via the
> `Reaction.inject` handler, not an upward boot callback. This replaces the
> `boot ↔ browser-plans` cycle.

**Anchors.** `PlanExtensions.cs:39-127`; `applied-plans.ts:24-163`; `merge-policy.ts:9-66`;
`component-merge.ts:5-138`; `boot.ts:33-102`; `inject.ts:22-49`; `root.ts:41-71`;
`05-determinism-proof.md:443-451`; `04-matrix-validation-components-slots.md:282-300`;
`07-determinism-certificate.md:88-91`.

---

### 3.12 Plan (the spine / aggregate root)

**Carrier.** `𝔻 = Version(3) × PlanId × PlanScope × Types × Components × Behaviors`. The
build sink `PlanBuildContext` (narrow `Declare`/`Wire`/`Register` verbs) accumulates into
the immutable `𝔻`. The two seam morphisms `ser : 𝔻 → 𝕁` and
`gen : NodeFamilies → TSContract` live here. `⊥` = a discovery miss (boot no-op, not a
failure).

**Operations.** `createPlan`/`buildIdentity`; `buildPlan` (snapshots the mutable sink into
the immutable document); `ser ≡ Render` (pure function of `𝔻`); `renderScript`;
`discover` (the one justified `[data-reactive-plan]` query); `compose` (`composeInitialPlans`
= the closed monoid fold realizing the `PlanId`-join); `boot` (Kleisli over `𝔼`, passing the
active plan **explicitly** into `wireTrigger`); `loadPartialSlot`/`unloadPartialSlot`.

**Census tie.** Plan adds no census band; it is the spine through which **all 375 census
variants** flow: `lower_C ⨾ BuildPlan ⨾ ser ⨾ gen-checked-transport ⨾ read_C`.

**Laws.** **S1** (`Render` byte-identical twice, 36 plans, 07:20); **S3**
injective-up-to-`≅`; **X** (`ser` emits every kind verbatim; `gen` enumerates all 55 — the
GEN-COHERENCE law); **Acyclicity** (the functor constraint that makes the composition lemma
applicable end-to-end); **P2** (the egress cut `evaluate → apply → formatForWire` is
idempotent-clean, 07:23).

> **D3 — the headline Plan law (partially violated today).** The `ActivePlan` must be
> passed explicitly to `executeReaction`. **Verified this pass**: `boot.ts:58` already
> threads `plan` into `wireTrigger`, but `boot.ts:42` still calls `setActivePlan` and
> `execute.ts:40` (`runtimePlanFor`) falls back to the `activeRuntimePlan` singleton. The
> singleton fallback is the D3 violation — removable because every wire path already
> carries the plan (§6.2).

**Anchors.** `PlanDocument.cs:9-32`; `PlanBuildContext.cs:30-35`; `PlanSerializer.cs:13-26`;
`ReactivePlan.cs:17-201`; `PlanExtensions.cs:39-160`; `root.ts:41-76`; `boot.ts:33-102`;
`applied-plans.ts:24-163`; `merge-policy.ts:9-66`; `02-micro-modules.md:82-177`;
`07-determinism-certificate.md:14-33`.

---

## 4. Cross-Module Seam Morphisms

A seam composes **cleanly** iff `cod(lower_C) = dom(read_C)` up to `⊥`-lift, with no
many-to-one collision (D1) and the arm-set agreeing across C#/JSON/TS (X). Three seams
fail this test today — those become the design discoveries in §6.

| Edge | Composes cleanly? | Seam condition / status |
|------|-------------------|--------------------------|
| `Kind → Shape` (DAG bottom) | ✅ | `gen(K_S)` surjective onto 10 tags; `applyShape` ends in `never`. Well-founded base — no morphism through itself. |
| `Value → Shape` / `Value → Kind` | ⚠️ **mis-cut** | Each `𝕍` arm carries a `Shape` got once via `FromClrType`. But `whole-payload`/`whole-element` ride `member === "responseBody"`/`"elementValue"` (`evaluate.ts:199,203`, verified) — `read_value`'s domain is **not** partitioned by `kind` alone. Re-cut = §6.1. |
| `Condition → Value` | ✅ | Compare's `Left` is a `𝕍` resolved by the **same** `evaluateValue` (`compare-engine.ts:8`). The 21 `CompareOp` tokens feed both lanes; `confirm` is the only async lift. Dual evaluators are a duplication smell (§6.4), not a type mis-cut. |
| `Reaction → {Value, Condition, Request, Slot, Component}` | ⚠️ **D3** | `lower` stamps the lane structurally (Request/Parallel distinct arms); `read` routes on `kind` ending in `assertNever`. But `crossedAsyncBoundary = result instanceof Promise` (`execute.ts:287`, verified) re-detects a carried fact. Re-cut = §6.2. |
| `Trigger → Reaction` / `Trigger → Component` | ✅ (cosmetic gap) | `StartsWhen` is kind-tagged. `Behavior` itself is an internal class with **no** `Kind`, reflection-serialized (`Behavior.cs:5`) — asymmetric, composes only because `BehaviorGraph` is a positional list. Re-cut is cosmetic (public-sealed symmetric `Behavior`). |
| `Request → {Value, Condition, Component, Shape}` | ⚠️ **mis-cut (authoring)** | Egress shape-once + exact-then-any routing are clean. But `Include`'s intake is typed to concrete `TypedComponentSource`/`TypedPluginSource`, so `cod(AsSource) = TypedSource ⊄ dom(Include)` — a real domain hole (`01:410-419`). Re-cut = §6.3. |
| `Component → {Value, Shape}` + vendor sub-seam | ✅ (perf flag) | `IdGenerator.For` deterministic ⇒ single-valued join key; same-id ⇒ same member ⇒ same `FromClrType` shape ⇒ the M1/C2 fast path is the only branch. `RuntimeObject` rebuilt per read today (`01:530`) — memoize (D3-preserving). |
| `Slot → Plan` / `Slot → Component` (downward only) | ✅ (with fresh-doc rule) | `composeBootPlanInto`/`composeSlotPlanInto` are the one MergePolicy. With the fresh-document rule, `recompose ∘ recompose = recompose`. Replaces the `boot ↔ browser-plans` cycle with a well-founded downward edge. |
| `Validation → {Condition, Component, Value, Plan}` | ✅ | WhenField reuses the **one** `CompareEngine`; peer read reuses the Value spine. The 18→6 map must be **generated** from C# `RuleName` so the three independent enumerations collapse to one. Composes cleanly once the generator owns the narrowing. |
| `Plugin → {Value, Component, Shape}` | ✅ | Every arity overload reaches the same `(member, returns, argShapes)` triple ⇒ single-valued. `resolve(unknown) = ⊥` is the one true external edge. Duplication is a merge-by-equality opportunity (§6.4), not a type mis-cut. |
| `Plan → {Trigger, Reaction, Component, Slot, Kind}` (root) | ⚠️ **D3** | `ser` pure (byte-identical), `gen` byte-identical to committed, `typecheck` exits 0. But `setActivePlan` + `runtimePlanFor` singleton fallback (`execute.ts:40`, `boot.ts:42`, verified) is a D3 violation, removable since `boot.ts:58` already carries the plan. The topmost clean cut over the well-founded DAG. |

---

## 5. The Determinism Theorem

> **Theorem (whole-pipeline determinism).** Let `𝓜` be the module category over the acyclic
> 12-module DAG (Kind → Shape at the bottom, Plan at the top, Slot → Plan downward-only).
> Define
> ```
> render  = ser ∘ buildPlan ∘ (⨾_C lower_C)  : DSL → PlanJSON
> execute = (⨾_C read_C) ∘ discover           : (PlanJSON × DOM) → 𝔼
> ```
> **Claim.** `render` is a deterministic **total** function `DSL → PlanJSON`, and `execute`
> is deterministic given DOM state, with all surviving non-determinism confined to
> explicitly named external boundaries (`getElementById` miss, non-iterable array source,
> missing payload scope, network presence, mounted-input set, live JS array source) — never
> to a DSL feature.

### Proof sketch

**(1) Each module morphism is deterministic** by its instance of D1/D2/D3:

- **D1 single-valued = law X(3) distinctness.** Every kind-tagged family —
  `K_S`(10), `K_V`(5 + 2), `K_C`(5), `K_R`(9), `K_T`(5), `RuleName`(18), `PlanScope`(2) —
  has pairwise-disjoint arms emitted verbatim as the `kind` string, so no input lowers to
  two outputs and no two inputs collapse to one arm.
- **D2 total-on-domain = X(1) + X(2).** Every `lower_C` surjects onto its live tags (the
  verified 375/375 census) and every `read_C` is defined on each arm ending in
  `assertNever`, with partiality `⊥` permitted **only** at the named external boundaries
  above.
- **D3 referential transparency.** `ser` renders byte-identical twice (07:20, verified) and
  `FromClrType` is referentially transparent (F2).

**(2) The composition lemma** (§2.4): if `f` and `g` are deterministic, `g ∘ f` is
deterministic, with `dom(g ∘ f) = { a ∈ dom(f) : f(a) ∈ dom(g) }`. Proof: (D1) unique
`f(a)` ⇒ unique `g(f(a))`; (D2) the composite is `⊥` exactly outside that set and `⊥`
absorbs under Kleisli composition (`⊥ · e = ⊥`), so a boundary failure deterministically
halts before `g` with no speculative recovery; (D3) `g(f(a))` is built only from `a` via
two referentially-transparent steps.

**(3) Acyclicity** makes the composite well-founded: because the DAG has no cycle, no
morphism is defined in terms of itself, so `render` and `execute` are each a *finite chain*
of deterministic morphisms — the lemma applies end-to-end.

**Injectivity-up-to-equality at each seam.** `ser` is injective-up-to-`≅` (S3: the
`additional` byte disambiguates open vs closed-empty objects), and at every seam
`cod(lower_C) = dom(read_C)` up to `⊥`-lift with no many-to-one collision — *the one current
counterexample* (`responseBody`/`elementValue`, a genuine many-to-one = D1 violation,
**verified live** at `ValueExpression.cs:379-380` / `evaluate.ts:199,203`) is closed by
Fix 1 (a `kind` can never collide with a camelCased member path), restoring
injectivity-up-to-`≅`.

Therefore `render` is a total deterministic function `DSL → PlanJSON`, and `execute` is
deterministic given DOM. **∎**

**The two non-deterministic-LOOKING runtime facts** — `crossedAsyncBoundary = result
instanceof Promise` (`execute.ts:287`) and the `activeRuntimePlan` singleton
(`execute.ts:40`) — do **not** break D1–D3 (the lane is recoverable from the carried kind,
and every wire path already carries the plan at `boot.ts:58`). They are *redundancies* the
redesign removes to make the carried fact the sole source of truth.

---

## 6. Design Discoveries — Ranked

The math points at four changes. Each is grounded in a specific law failing with a
verified witness, and each makes the codebase *simpler and more correct*, not merely
"refactored." Ranked by determinism impact.

### 6.1 — Promote whole-payload / whole-element to distinct kinds (closes the only live D1 violation)

**Finding.** `WholePayload`/`WholeElement` reads collide with a DSL property *literally
named* `ResponseBody`/`ElementValue`: both produce the same wire member and the same
runtime behavior (return the whole object unwalked). This is the **single live many-to-one
collision in shipped source.**

**Math basis.** D1 single-valued violation = failure of X(3) distinctness. **Verified this
pass:** `evaluate.ts:199,203` discriminates on `expression.member === "responseBody"` /
`"elementValue"`; `ValueExpression.cs:379-380` stamps those reserved member strings.
`CamelCase` lowercases only the first char, so a property `ResponseBody` ↦ exactly
`responseBody`. Two distinct inputs ↦ one wire member ↦ one behavior — not a function up to
equality.

**Simplification.** Promote whole reads to distinct `ValueExpression` node **kinds**
(`kind: "whole-payload"` / `"whole-element"`, carrying no member). The runtime routes on
`kind` (one switch arm), never on a member string; a property named `ResponseBody` lowers to
an ordinary `kind: "read"`, structurally disjoint. A `kind` can never equal a camelCased
member path, so the collision becomes **unrepresentable** — the textbook
sentinel-is-really-a-distinct-variant fix.

### 6.2 — Stamp the lane, pass the plan explicitly (restores D3 at the runtime root)

**Finding.** The runtime re-detects the sync/async lane everywhere via
`result instanceof Promise`, and a hidden mutable `activeRuntimePlan` singleton is consulted
as a fallback — yet the lane is already determined by the reaction kind and the plan is
already passed explicitly to `wireTrigger`.

**Math basis.** D3 referential transparency + lane-as-D1-over-the-kind-tag. The lane is a
deterministic projection of `K_R`: `{set, call, dispatch, inject, show-validation-errors}
→ void`, `{request, parallel} → Promise` (branch/sequence inherit from their reachable
arms). So `instanceof Promise` (**verified**, `execute.ts:287`) recomputes a fact the plan
already carries — two extensionally-equal ways to know the lane ⇒ merge them. The singleton
(**verified**, `execute.ts:28-42`, `runtimePlanFor`) makes `execute` depend on ambient
mutable state ("whichever plan booted last"), breaking D3; but `boot.ts:58` (**verified**)
already threads `plan` into `wireTrigger`.

**Simplification.** Stamp `ReactionLane` onto each node at lower-time (the draft sequencer
already structurally separates lanes — Request/Parallel are distinct `K_R` arms,
`ReactionPipelineDraft.cs:14-30`) and route on the carried tag; delete
`crossedAsyncBoundary`'s `instanceof` probe. Pass `ActivePlan` explicitly to
`executeReaction` and delete the `activeRuntimePlan` singleton + `resetActivePlanForTests`.
Both are redundant-operations-provably-equal-to-the-carried-fact — merge into the single
carried truth, restoring D3.

### 6.3 — Widen `Include`'s intake to `TypedSource` (re-cuts a mis-cut seam)

**Finding.** The gather `Include` intake is typed to concrete `TypedComponentSource` /
`TypedPluginSource`, so a `ReactiveValue`/`ReactiveArray.AsSource()` (which yields the
abstract `TypedSource`) cannot be gathered into a request — the "one `ValueExpression` reads
all values" spine has a real hole at exactly one boundary.

**Math basis.** Seam type-mismatch `cod(f) ≠ dom(g)`: `cod(AsSource) = TypedSource`, but
`dom(Include) = TypedComponentSource ⊎ TypedPluginSource ⊊ TypedSource`
(`GatherBuilder.cs:206,266`; `01:410-419`). The morphism `AsSource ⨾ Include` does **not**
compose — the codomain is wider than the gather intake's domain. A mis-cut seam, not a
determinism break (nothing collides), but a domain hole that forces an awkward workaround.

**Simplification.** Widen `Include`'s intake from the concrete source families to the
abstract `TypedSource<TProp>` (every concrete source already lowers to
`ValueExpression.Read`/`Invoke`, so the reader needs no change). This re-cuts the seam so
`cod(AsSource) = dom(Include)`, closing the hole and making the value spine genuinely
one-write-path for **all** readable values including array-op results.

### 6.4 — Merge extensionally-equal morphisms (collapse duplication that can drift)

**Finding.** Two condition evaluators (`conditions.ts` async-recursive `all`/`any`/`not` +
`compare-engine` sync), three independent enumerations of the validation rule set (C#
`RuleName.Known`(18), the TS `plan.ts` union, the `rule-engine.ts` switch), and two
~95%-identical plugin builders (`PluginReadBuilder`/`PluginCallBuilder`) plus two
plugin-declaration APIs can each **drift**.

**Math basis.** Extensional equality of morphisms ⇒ merge them (the module-category
discovery rule). The two condition evaluators compute the same boolean over the same 21
`CompareOp` tokens (one op source) — `∀` input they agree, so they are one morphism written
twice (`confirm` is the only genuine async lift). The two plugin builders are extensionally
equal on `(member, returns, argShapes)`. The three rule-name enumerations are three
encodings of one finite set (`|Known| = 18`) that **must** agree — a drift here is a latent
D1 failure across the C#→TS seam.

**Simplification.** (a) **One** `CompareEngine` consumed by both lanes (`confirm` wraps it
async); delete the duplicated `all`/`any`/`not` recursion. (b) **One** reflection-driven
generator owns the 18→6 `RuleName` narrowing so the TS union and the `rule-engine` switch
are *derived* from the single C# `RuleName`, with a build-time drift gate. (c) **One**
args-builder + one declaration spine for plugins.

> **Critically, do NOT over-merge.** `notEqual` (literal-only) / `notEqualTo` (peer-only)
> stay distinct tokens — collapsing them would *create* a many-to-one D1 violation
> (04-validation:132). And `Shape.merge` stays a closed **union**, not a lattice join: M5
> non-associativity is a quirk under the collision-free-id invariant
> (`ShapeContractCompatibility.cs:20`, the `existing == incoming` fast path is the only
> branch exercised, **verified**), so strengthening it to a join would *break* partial-plan
> accumulation, not fix a bug.

---

## Appendix — Verification Status

| Claim | Status |
|-------|--------|
| `merge` annihilator-before-Any + single-level nullable collapse | **Verified** — `ShapeContractCompatibility.cs:12-29` read this pass |
| `read_value` member-string D1 collision | **Verified** — `evaluate.ts:199,203` read this pass |
| `crossedAsyncBoundary = result instanceof Promise` | **Verified** — `execute.ts:287` read this pass |
| `activeRuntimePlan` singleton + `runtimePlanFor` fallback | **Verified** — `execute.ts:28-42` read this pass |
| `routeResponseRoutes = find(exact) ?? find(any)` | **Verified** — `http.ts:263` read this pass |
| `MergePolicy.composeBootPlanInto` behaviors-append | **Verified** — `merge-policy.ts:9-18` read this pass |
| 21 `CompareOp` tokens | **Verified** — `CompareOp.cs:11-42` counted this pass |
| 375/375 census, 36 byte-identical plans, 55 kinds, spy = 1 | **Carried** from `05-determinism-proof.md` / `07-determinism-certificate.md` (not independently re-run this pass) |

All other source-line anchors are **cited from the framework artifacts** (the module
algebras, `01`–`07` redesign docs) and not independently re-opened in this pass; treat them
as carried evidence, not freshly re-executed.
