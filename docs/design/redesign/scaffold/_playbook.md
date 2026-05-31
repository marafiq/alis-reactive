# Implementation Playbook — Coding the Redesign

> **What this is.** The single answer to *"where do I start, and what do I type
> next?"* for the 12-module redesign. It does not re-explain the design — the
> design lives in [`00-design.md`](../00-design.md), the decomposition in
> [`02-micro-modules.md`](../02-micro-modules.md), the names in
> [`03-naming.md`](../03-naming.md), and the proof in the four
> [`04-matrix-*`](../04-matrix-triggers-reactions-conditions.md) docs. This file
> is the **build order, the per-module loop, and the gates**. Open it, do the next
> module, run the gates, commit, repeat.
>
> **The promise the design makes — and this playbook cashes.** *One public DSL
> input → one plan-JSON shape → one browser behavior, and the choice made when the
> developer says nothing is the right one.* Because that holds (proven 120/120 in
> [`05-determinism-proof.md`](../05-determinism-proof.md)), coding is **mechanical**:
> open a module spec, read its §Public Surface + §Acceptance Fixtures, paste the
> fixtures, fill the §Compile-Ready Skeleton until the fixtures go green. No
> judgement calls. If you hit one, the spec is incomplete — stop and read source
> ([`CLAUDE.md`](../../../../CLAUDE.md) Rule 1: DSL source before code).

---

## 0. Before the first line — orient (10 minutes, once)

| Read | Why |
|---|---|
| [`00-design.md`](../00-design.md) §2 (module map + dependency graph) | The 12 modules and the acyclic order this playbook enforces. |
| [`02-micro-modules.md`](../02-micro-modules.md) (Module List + dependency graph) | What each module *owns* and *replaces*; the authoritative edge list §1 verifies against. |
| [`03-naming.md`](../03-naming.md) (Key Concepts tables) | The exact type name you type for every concept — never invent a name. |
| The module spec you are about to code, e.g. [`scaffold/Shape.md`](./Shape.md) | Its §Public Surface, §File Layout, §Compile-Ready Skeleton, §Acceptance Fixtures. |

**Ground rule (non-negotiable, from root `CLAUDE.md`).** Every DSL verb you wire
must exist in actual public source — `Alis.Reactive/Builders/**`,
`Alis.Reactive/Razor/Extensions/**`, `Alis.Reactive/Razor/InputBoundField.cs`.
Do not code a call the source does not have. When in doubt, `grep` the builder
folder before you type. The verified source roots, by module, are in each spec's
§1 and §File Layout.

---

## 1. The build order (dependency-respecting, verified against the graph)

The dependency graph in [`02-micro-modules.md`](../02-micro-modules.md) (and
mirrored in `00-design.md` §2) is **acyclic and layered**. Topologically sorting
it gives the build order: a module is coded only after everything it depends on
already compiles and is green. The single apparent cycle —
`Slot → Plan`, `Reaction → Slot`, `Plan → Slot` — is **not** a cycle: per the
design's `Slot → Plan` note, Slot depends only on the `PlanDocument` *type* (a
data shape), and Plan reaches Slot's behavior only through the Reaction `inject`
handler at runtime, never by importing Slot. So `PlanDocument`-as-a-type is
available to Slot from Wave 5's first stub; Plan's *boot/recompose wiring* is the
last thing finished.

**The seven waves.** Modules in the same wave have no dependency between them and
may be coded in any order (or in parallel by separate developers / worktrees).

| Wave | Modules | Depends only on | Spec |
|---|---|---|---|
| **1** | **Shape** | nothing | [`Shape.md`](./Shape.md) |
| **2** | **Kind** | Shape | [`Kind.md`](./Kind.md) |
| **3** | **Value** | Shape, Kind | [`Value.md`](./Value.md) |
| **4** | **Condition** · **Request**¹ | Value (+ Shape, Kind) | [`Condition.md`](./Condition.md) · [`Request.md`](./Request.md) |
| **5** | **Reaction** · **Trigger** | Condition, Request, Value, Component-stub² (+ Kind) | [`Reaction.md`](./Reaction.md) · [`Trigger.md`](./Trigger.md) |
| **6** | **Component** · **Slot** · **Validation** · **Plugin** | Value, Condition, Component, Shape, Kind, `PlanDocument`-type | [`Component.md`](./Component.md) · [`Slot.md`](./Slot.md) · [`Validation.md`](./Validation.md) · [`Plugin.md`](./Plugin.md) |
| **7** | **Plan** | Trigger, Reaction, Component, Slot (+ Kind) | [`Plan.md`](./Plan.md) |

**Ordered single-thread list (if you are one developer, do exactly this):**

```
1. Shape
2. Kind
3. Value
4. Condition
5. Request
6. Component        ← pulled forward; see ¹ ²
7. Reaction
8. Trigger
9. Slot
10. Validation
11. Plugin
12. Plan
```

> **¹ The `Request → Component` edge.** `Request` reads component values in its
> gather (`target <- componentSource`) and routes the `WhileLoading`/`Finally`
> effects onto component objects, so it leans on `Component`'s `BrowserObjectId` +
> `BrowserObjectContract` *types*. **The single-thread list resolves this by
> building Component (step 6) before Reaction/Trigger** — Component depends only on
> Value/Shape/Kind, so it is legal as early as Wave 3-and-after. The banded table
> keeps Component in Wave 6 only to group it with the other browser-object
> modules; if you build banded, **stub Component's `BrowserObjectId` /
> `BrowserObjectContract` first** (their §Public Surface in
> [`Component.md`](./Component.md) §2) so Request/Reaction compile, then finish
> Component's runtime (`RuntimeObject` memoization, `ComponentDriver`) in Wave 6.
> Either way: the *type surface* Component owns must compile before Request.
>
> **² The `Component`-stub in Wave 5.** Reaction's `set`/`call`/`dispatch`
> handlers target a `BrowserObject`; they need its id/contract types, not its
> runtime resolution. Same rule: the type surface compiles first, the memoized
> `RuntimeObject` lands with Component proper.

**Verification against the graph (do this before trusting the order).** For each
module, confirm every arrow out of it in `02-micro-modules.md`'s mermaid graph
points to a module **earlier** in your chosen order. The one edge that needs the
note above is `Request → Component`; `Slot → Plan` is the documented
type-only edge; `Plan → {Trigger,Reaction,Component,Slot}` all point backward
because Plan is last. There are **no other** forward edges — if your order
introduces one, your order is wrong, not the graph.

---

## 2. The per-module loop — "paste the fixtures, fill until green"

Every module spec has the same spine: **§Public Surface · §Input→Output Contract
· §File Layout · §Compile-Ready Skeleton · §Acceptance Fixtures**. The loop is
identical for all 12. Do it in this order, every time.

```
┌─ for each module, in build order ───────────────────────────────────────────┐
│ 0. Open scaffold/<Module>.md. Read §1 (Responsibility/Owns/Depends) and      │
│    confirm every "Depends on" module is already green. If not, you are out   │
│    of order — stop.                                                          │
│                                                                              │
│ 1. WRITE THE PASS-PROTOCOL ROW (CLAUDE.md). One line, at the top of the      │
│    commit you are about to make:                                             │
│      Close matrix row: <DSL source call> -> <domain term> -> <runtime>       │
│    Several specs (Component §8, Reaction §7) pre-fill this — paste theirs.    │
│                                                                              │
│ 2. PASTE THE FIXTURES FIRST (tests before bodies). Copy the named fixtures   │
│    from scaffold/_fixtures.md (the aggregated index) — or, equivalently,     │
│    from the module spec's own §Acceptance Fixtures, which is the source of   │
│    truth those fixtures are drawn from — into the test files named in        │
│    §File Layout. Each fixture row = one test, named exactly as written       │
│    (e.g. clr_int_is_number, apply_date_only_is_local_midnight). They fail    │
│    (red) because no body exists yet. That red is your spec.                  │
│                                                                              │
│ 3. CREATE THE FILES from §File Layout — the exact paths, no new folders.     │
│    Mirror the existing source tree (the specs name real paths under          │
│    Alis.Reactive/** and Alis.Reactive.Assets/runtime/**). Kernels and        │
│    surviving names are NOT relocated (Shape stays in PlanModel/Shape.cs).    │
│                                                                              │
│ 4. PASTE THE COMPILE-READY SKELETON from §5/§6 into those files. It already  │
│    has the right namespaces, signatures, internal/public visibility          │
│    (Rule 8: internal ctors, internal set), and `// TODO (fixture: <name>)`   │
│    markers tying each hole to the red test that proves it.                   │
│                                                                              │
│ 5. FILL EACH TODO until its named fixture goes green. Type the OBVIOUS body  │
│    — the spec's §Input→Output Contract states the exact rule (e.g. "Any is   │
│    identity; None conflicts with everything"). There is no design decision   │
│    in a fill; if you reach for one, the spec is incomplete — re-read source. │
│                                                                              │
│ 6. WHEN C# PLAN SHAPE CHANGED: regenerate the TS contract and typecheck      │
│    (Kind kernel owns this — §Gates). The drift gate, not your memory, keeps  │
│    plan.ts honest.                                                           │
│                                                                              │
│ 7. RUN THE GATES (§3). All green → commit the closed row. One module, one    │
│    (or few) commits. Do NOT start the next module before this one's gates    │
│    pass and the commit lands (CLAUDE.md: progress only from committed work). │
└──────────────────────────────────────────────────────────────────────────────┘
```

> **On `scaffold/_fixtures.md`.** The per-module §Acceptance Fixtures sections are
> the authoritative fixture source today (Shape §6, Value §7, Condition §6, …).
> `_fixtures.md` is the convenience aggregation — one flat, named index across all
> 12 modules so the "paste the fixtures" step is a single copy. If `_fixtures.md`
> is absent or stale, the module spec's §Acceptance Fixtures wins; they are the
> same fixtures, pulled by name from the `04-matrix-*` docs. Never invent a
> fixture the matrix does not name.

### What to write in what order *within* a module

The within-module order follows the same dependency logic as the build order —
types before the things that read them, contract before runtime:

1. **C# value objects / node family first** (§Public Surface, `→` side). These
   carry `Shape` and `Kind`; they have `internal` constructors and factory entry
   points (Rule 8). Invalid states are made unrepresentable *here* (real variants,
   never magic sentinels) — not defended in TS.
2. **C# authoring builder(s)** — the fluent surface a developer chains
   (`When/Then`, `.Get(...).Gather(...)`, `Html.On`). It only *emits* the node
   family from step 1; it carries no behavior.
3. **C# serialization** — handled by the **Kind** kernel's `PlanSerializer` once a
   node carries its `kind`; you write no hand converter (the 11 hand converters
   and the 1,165-line mirror are *deleted*, not reproduced).
4. **Regenerate `plan.ts`** via `PlanContractGenerator`; the `ContractDriftGate`
   proves C# and TS agree. (Kernel work; the slice does nothing by hand.)
5. **TS runtime reader** (`⇒` side) — the one reader for the one writer:
   `evaluateValue`, `CompareEngine`, `executeReaction`, etc. Switch on `kind`
   with a final `assertNever` arm so a missing variant is a compile error. Route
   sync/async on the **plan-carried lane**, never `instanceof Promise`.
6. **C# domain tests then TS runtime tests** — the fixtures you pasted in loop
   step 2 now go green from both directions (one write path proves the node, one
   read path proves the behavior).

The kernels (**Shape**, **Kind**) and **Plan** spine deviate only in emphasis:
Shape/Kind have *no* authoring builder (they are cross-cutting machinery), and
Plan has *no* node family of its own (it is the document the slices write into +
the discover/boot runtime). Their specs reflect this; the loop is otherwise
identical.

---

## 3. The gates — what "green" means before every commit

Run these in order. **All must pass before the commit and before starting the
next module** (CLAUDE.md "Before every push"; counts grow each session).

| # | Gate | Command | Catches |
|---|---|---|---|
| **G1** | **Build both TFMs** | `dotnet build` | The framework targets `net48;net10.0` (`Alis.Reactive.csproj`). A change that compiles on net10 but breaks the net48 `#if` shims fails here — both TFMs are first-class. |
| **G2** | **net10 unit tests** | `dotnet test` (the net10 unit suite; the spec's §File Layout names the target test paths) | The fixtures you pasted — C# domain behavior (one write path). Each named fixture is one test. |
| **G3** | **TS typecheck (drift gate)** | `npm run typecheck` | **Run whenever the C# plan shape changed.** Confirms the *generated* `plan.ts` agrees with the C# node families; the `ContractDriftGate` fails the build on a renamed property the runtime would silently disagree with. This is the correctness fix the whole Kind kernel exists for. |
| **G4** | **TS runtime tests** | `npm test` (vitest, jsdom) | The runtime fixtures (one read path) — `applyShape`, `evaluateValue`, `executeReaction`, `CompareEngine`. |
| **G5** | **Byte-stability** | re-serialize the same plan twice → identical bytes; (CLAUDE.md `git status` stays clean after a build) | The shape-once invariant and the single `PlanSerializer` ownership: the same domain model serializes to byte-identical camelCase JSON every time. A nondeterministic field order or a re-derived shape breaks this — it is the proof that "one input → one plan-JSON shape" actually holds. Build outputs are gitignored, so a `dist/`/`wwwroot/` file in `git status` is a write-to-tracked-path bug, not something to `git add`. |

**When to run which.** G1 + G2 on every C# change. G3 the moment a plan *shape*
changes (new node, renamed field, new variant) — never defer it. G4 on every TS
runtime change. G5 before the commit that closes the row. Browser-visible DSL
behavior additionally gets a Playwright slice against *freshly built* runtime
assets (`npm run build:all` first) — but that is per-feature, layered on after
the module's own gates are green, not part of the inner loop.

> **net48 reality check.** Only `Alis.Reactive` and `Alis.Reactive.DesignSystem`
> multi-target net48; `Alis.Reactive.Fusion` is net10-only. The redesign's plan
> domain lives in `Alis.Reactive`, so **G1 builds both TFMs by default** — do not
> add a net10-only API to the plan domain without a net48 `#if` shim, or G1 goes
> red on the net48 leg.

---

## 4. Done-when (per module) — the closing checklist

A module is **done** only when all of the following hold (CLAUDE.md Pass +
Post-Flight protocol). Specs with an explicit "Done When" (Reaction §7) or
"Pass Protocol Row" (Component §8) section restate this with the module's exact
rows — paste theirs.

- [ ] Every fixture in the spec's §Acceptance Fixtures is a passing test, named exactly.
- [ ] **Coverage gate** (CLAUDE.md "Coverage Completeness Gate"): every variant /
      axis value the spec enumerates maps to a fixture *by name* — not "tests
      pass," but "every item covered or justified." Shape's coverage gate, for
      instance, requires every P-SHAPE variant in an A/C fixture **and** an
      `applyShape` arm in an E fixture.
- [ ] G1–G5 all green.
- [ ] When the C# plan shape changed: `plan.ts` regenerated, G3 green, drift gate satisfied.
- [ ] No stale vocabulary, dead code, defensive plan-validators, magic sentinels,
      singletons, or schema-as-contract references left behind (the things the
      module's §1 "Replaces"/"Dissolves" column promised to delete are actually gone).
- [ ] The closed-row commit message names the behavior row.

---

## 5. Where to start, right now

1. Open [`scaffold/Shape.md`](./Shape.md).
2. Write its pass-protocol row.
3. Paste its §6 fixtures (A CLR-inference, B construction invariants, C
   serialization, D equality+algebra, E runtime conversion) into the test files
   in its §File Layout — they go red.
4. Create the files; paste the §5 Compile-Ready Skeleton; fill each
   `// TODO (fixture: …)` until that named fixture is green.
5. Run G1, G2, G4, G5 (Shape changes plan shape → also G3). All green → commit
   `Close matrix row: Shape.FromClrType -> Shape value object -> ShapeConverter.applyShape`.
6. Go to [`scaffold/Kind.md`](./Kind.md). Repeat.

There is never a "where do I start" again: the next module is the next row in §1's
ordered list, and the next action inside it is the next step of §2's loop.

---

## Appendix — the ordered module list (one line, for scripts)

```
Shape, Kind, Value, Condition, Request, Component, Reaction, Trigger, Slot, Validation, Plugin, Plan
```
