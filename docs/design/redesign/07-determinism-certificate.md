# 07 — Determinism Certificate (math proof of the redesign)

> The redesign treated as a **mathematical question**: if the model is deterministic,
> it is a well-defined function and an algebra, so it can be proven by **universal laws**
> (∀, machine-checked over the generated input space with shrunk counterexamples) rather
> than examples. Every result below was produced by property harnesses **compiled against
> the real source** (`Alis.Reactive/PlanModel/Shape.cs`, `ShapeContractCompatibility.cs`,
> the runtime, the real generator), cross-validated by **two independent harnesses + an
> adversarial judge**. A finding is a reproducible witness, not an opinion.
>
> Harnesses (committed, runnable): `dogfood/shape-math-a` (CsCheck), `dogfood/shape-math-b`
> (hand-rolled + small-exhaustive), `dogfood/shape-math-a-repro`, `dogfood/determinism-domain`.

## 1. The whole pipeline is PROVEN deterministic + faithful

`redesignProvenDeterministic = TRUE` (workflow: 4 parallel provers + judge, all reproduced):

| Seam (data flow) | Verdict | Evidence (reproducible) |
|---|---|---|
| C# domain → serialize | clean cut | `Render()` (`ReactivePlan.cs:94`) is a pure function of the `PlanDocument`; 36 public-DSL plans render **byte-identical twice**; camelCase members; `kind` verbatim. |
| serialize → `plan.ts` contract | clean cut | Regenerate `plan.ts` via the **real generator** → **byte-identical** to committed (22,861 b, hash `373be63b`). 18 `WriteOnlyPolymorphicConverter<T>` bases → 18 TS unions; **all 55 C# `Kind` literals present in TS** (`comm -23` empty); union counts 1:1. Enum families render from the C# `.Values`. |
| `plan.ts` → TS runtime | clean cut | `npm run typecheck` exits 0 across both tsconfigs against the regenerated contract → the runtime's discriminated-union switches are **exhaustive** against the contract. |
| evaluate → applyShape → formatForWire | clean cut | **shape-once**: `applyShape` spy-counted at exactly **1** on a full date-literal egress; the wire-format step never re-derives shape. |

**Runtime executor**: deterministic (referentially transparent over thousands of generated
nodes), **total** (never throws except at real boundaries: `getElementById` null, non-iterable
source, missing payload scope), **correctly laned** (sync reactions return `void`; only
request/parallel/confirm cross to async, decided by the plan-carried `kind`).

**"One DSL input → one plan JSON"** is therefore a proven function-determinism law, end to end,
and **every seam is a clean cut (no stage re-derives shape)** — which validates the micro-module
decomposition as cut at the right joints.

Open hardening (minor, not a defect): `execution/execute.ts:159` (`executeSet`) and `:177`
(`executeCall`) have `switch (reaction.on.kind)` without an `assertNever` default — correct today
(they enumerate the full union) but not compile-protected against a future variant.

## 2. The Shape domain algebra — proven structure

Compiled against the real `Shape.cs` + `ShapeContractCompatibility.cs`:

**Holds (an equivalence + a clean egress engine + a faithful wire contract):**
- `Equals` is a genuine **equivalence relation** (reflexive/symmetric/transitive) and a
  **congruence**: `GetHashCode` and `Serialize` both respect it (E1–E4, S1–S2).
- `Serialize` is **deterministic** and **injective-up-to-structural-equality** (S3) — open vs
  closed-empty objects are disambiguated by the `additional` byte. The wire JSON is unambiguous.
- `applyShape` / `convertByShape` is **total, idempotent (shape-once), and coherent** (P1–P3).
- `FromClrType` is **total and deterministic** (F1–F2).
- merge is **commutative** and **idempotent** with `merge(s,s)=s` (M1–M2, C2).

**Structural properties the math surfaced (true, with reproduced witnesses):**
- merge is **not a lattice join / upper bound** (C1): `merge(object{a}, object{b}) = object{a,b}`
  (closed union), which does not satisfy `accepts(merged, operand)`.
- merge is **not associative** on nested nullables (M5): bracketing changes the result, because
  the nullable-collapse rule is single-level (`Shape.cs:427`).
- `accept` is **reflexive but not transitive** (A4): `Any` is simultaneously top and bottom, so it
  bridges two otherwise-incompatible scalars.
- `Any` is identity/top for every shape **except `None`** (M3/A2): `None` short-circuits to
  conflict/false before the `Any` branch.

## 3. Why those structural properties are NOT bugs (domain resolution)

The adversarial judge initially labelled C1/M5 "BUG" because it applied lattice laws (join,
associativity) without the domain invariant. **The owner supplied it:** `TryMergeContracts` is
**partial-plan merging**. Its only consumers are `BrowserObjectContract` (method return/argument/
property merge), invoked when partials compose by `PlanId` or a slot injects and the **same member**
is registered from multiple partials.

Because `IdGenerator` produces **deterministic, collision-free IDs from the model expression**,
**same-id ⟹ genuinely-same-member ⟹ same `FromClrType` shape ⟹ merge hits the `==` fast path.**
Therefore:

- **C1 — union is correct, not a bug.** Merge's contract is *accumulate the full member contract
  across partial-plan registrations*, not *compute a join*. The "accepts both operands" law is the
  wrong lens. The collision-free IDs guarantee no distinct members are ever conflated.
- **M5 — not a real bug.** The order-sensitive non-equal branch is never exercised with differing
  shapes, because same-id always yields the same shape (the `==` path).
- **A4 — quirk** (predicted): `accept` is used pointwise at one call site (`AcceptInvocationArgument`,
  `BrowserObjectContract.cs:311`), never as a transitive closure.
- **M3/A2/M4 — quirks**: `None` is the rejection sentinel (never a registered member shape), so its
  short-circuit ahead of `Any` is the intended guard, not a contradiction.

**Lesson:** math correctly characterizes *structure*; whether a structural property is a *bug* depends
on the **domain invariant** the rest of the system maintains (here: collision-free deterministic IDs +
partial-merge accumulation). Read the consumer and confirm the invariant before "fixing."

## 4. Carry-forward into the clean redesign

- The merge spec MUST document the invariant: *collision-free deterministic IDs → same-id → same
  shape → merge is union accumulation across partial merges; the non-`==` branches are robustness-only.*
  The rewrite must not "fix" the union into a join — that would break partial-plan accumulation.
- Keep the proven properties as invariants of the rewrite: equivalence + congruent hashing,
  serialize determinism + injectivity-up-to-equality, applyShape totality + shape-once idempotence,
  FromClrType totality, the four clean pipeline seams.
- Optional hardening to fold in: `assertNever` on the two `execute.ts` `on.kind` switches.
