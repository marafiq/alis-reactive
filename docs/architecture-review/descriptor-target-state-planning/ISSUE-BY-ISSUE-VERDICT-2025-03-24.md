# Architecture review — issue-by-issue verdict (code-verified)

**Date:** 2025-03-24  
**Method:** Claims in issue docs and planning README cross-checked against source (`Alis.Reactive`, schema, SandboxApp `Scripts/`).  
**Purpose:** Single place for **confidence**, **priority**, **risk**, **nuances**, and **recommended execution order** after multi-reviewer pass.

**Related:** [ACCURACY-REVIEW-2025-03-24.md](ACCURACY-REVIEW-2025-03-24.md), per-issue files `issue-A` … `issue-F`, [README.md](README.md).

### Reciprocal links (verdict ↔ issue docs)

| Issue | Problem, tests, dependencies |
|-------|-------------------------------|
| **A** | [issue-A-mutable-descriptors.md](issue-A-mutable-descriptors.md) |
| **B** | [issue-B-pipeline-builder.md](issue-B-pipeline-builder.md) |
| **C** | [issue-c-http-validation.md](issue-c-http-validation.md) |
| **D** | [issue-d-status-handler.md](issue-d-status-handler.md) — anti-patterns below ↔ §1 Problem + §5 tests |
| **E** | [issue-e-command-emitter.md](issue-e-command-emitter.md) |
| **F** | [issue-f-entry-segments.md](issue-f-entry-segments.md) |

---

## Cross-cutting highlights (apply to all issues)

1. **Confidence:** Issues **A–F** are **High** on factual claims (constructors, call sites, schema `oneOf`, `TriggerBuilder` → `BuildReactions`, etc.).
2. **Issue C** understates constructor arity: [`RequestDescriptor`](../../../Alis.Reactive/Descriptors/Requests/RequestDescriptor.cs) uses **9** positional parameters (not merely “5+”) — smell is **worse than** the minimum threshold in [CODE-SMELLS.md](CODE-SMELLS.md).
3. **Issue D — serialization:** [`StatusHandler`](../../../Alis.Reactive/Descriptors/Requests/StatusHandler.cs) is **not** polymorphic today (no `JsonDerivedType` on this type). Adding a **`kind`** field or **two concrete types** requires **explicit** C# + schema + TS wiring — same **pattern** as other polymorphic plan types, but **not** “zero new code” unless you only add **required `kind`** on the **existing** class and serialize it.
4. **Issue F3:** [`TriggerBuilder`](../../../Alis.Reactive/Builders/TriggerBuilder.cs) uses **`BuildReactions()`** only — **main `Html.On` path is safe**. [`BuildReaction()`](../../../Alis.Reactive/Builders/PipelineBuilder.cs) is still a **public footgun** (identical branches return `reactions[0]` when `Count > 1`). Treat as **P0 contract bug** (throw / obsolete / rename), not only “silent production bug” — no need to prove a live incident.
5. **Issue E** is **independently shippable** from **B** — only **four** Fusion filtering extensions + `ComponentRef` call `AddCommand`; they do not depend on PipelineBuilder façade extraction.
6. **[descriptor-encapsulation-review.md](../descriptor-encapsulation-review.md):** “**34** classes” re-count → **~40** `class` declarations under `Descriptors/` (methodology differs by inclusion rules) — **conclusions unchanged**. “**7** mutations / **3** classes” → **descriptor API:** **`GuardWith`** + **`RequestDescriptor`** internal attach/enrich; **resolver** mutates nested validation fields — count **methods** vs **call sites** separately.

---

## Issues A–F (summary table)

| Issue | Claims accurate? | Will fix do what it says? | Priority | Risk |
|-------|------------------|---------------------------|----------|------|
| **A** Mutable `Command` / `GuardWith` | Yes | Yes | P2 | Low |
| **B** `PipelineBuilder` façade | Yes | Yes (organizational) | P3 | Low |
| **C** HTTP + validation resolve | Yes (**9**-param ctor) | Yes, **Pre-C** baseline mandatory | P1 | Medium–high |
| **D** `StatusHandler` union + TS | Yes | Yes; **wire-breaking**, atomic PR | P3 | Low (if lockstep) |
| **E** `ICommandEmitter` / `AddCommand` | Yes | Yes | P2 | Low |
| **F1** `Entry` null checks | Yes | Yes | P1 (trivial) | ~Zero |
| **F2** Document V1–V4 | Yes | Yes | P3 | ~Zero |
| **F3** `BuildReaction` | Yes | Yes | **P0** | Low |
| **F4** Duplicate trigger JSON | Yes | Deferred | P4 | N/A |

---

## Overview issues (original five — from broader `docs/architecture-review/`)

| Topic | Verdict | Priority |
|-------|---------|----------|
| Command guard mutability | Covered by **A** | — |
| `IReactivePlan` breadth (ISP) | Accurate; **low value** (single implementation) | P3 |
| Component registration enforcement | Accurate; **valuable** (fail-fast) | P1 |
| Condition guard composition duplication | Accurate; DRY helper | P2 |
| Gather extensions vendor duplication | Accurate; core constraint | P2 |

---

## Recommended execution order (risk / dependency)

1. **F3** — `BuildReaction` must not silently drop segments (`Count > 1` → throw or API change).
2. **F1** — `Entry` null checks (fail-fast).
3. **Overview #3** — registration enforcement (developer foot-gun).
4. **Overview #4 + #5** — DRY guard composition + gather (quick wins).
5. **E** — `ICommandEmitter` (mechanical).
6. **A** — command immutability / `WithGuard` style (after call-site inventory).
7. **C** — return-new + VO (**after Pre-C** snapshots locked).
8. **D**, **B**, remaining overview **as opportunity**.

**Note:** [README.md](README.md) § “Issue dependency” mermaid shows **merge prerequisites** (e.g. F-tier → Pre-C). **B → E** is **dashed / soft** only (not “E blocked on B”). **C** and **D** sit in a **parallel** subgraph (same train; coordinate `StatusHandler` wire with **C** when needed). This **execution order** is **recommended sprint order** and overrides a strict reading of topology when **E** ships before **B**.

---

## Issue D — anti-patterns (do not ship)

**Full spec:** [issue-d-status-handler.md](issue-d-status-handler.md) (problem statement, INVEST, test IDs, §6 dependencies).

- **`kind` optional** with runtime fallback from `reaction`/`commands` presence — masks invalid JSON; violates fail-fast.
- **Schema** accepting **old and new** `StatusHandler` shapes indefinitely — backwards-compat shim; avoid unless explicit migration program.
- **C# emits `kind`** but **TS** still treats both payloads as optional — **mismatched contract**.

**Pre-D (same idea as Pre-C):** Snapshot every `Render()` output that includes `onSuccess` / `onError`; after change, diff — **only** intended discriminant / shape delta.

---

## Sign-off

Re-run this verdict doc after major churn to issue files or `PipelineBuilder` / `RequestDescriptor` / `StatusHandler` shapes.
