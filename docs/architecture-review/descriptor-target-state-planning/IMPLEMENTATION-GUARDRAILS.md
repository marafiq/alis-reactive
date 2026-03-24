# Implementation guardrails — anti-drift (final review checklist)

**Purpose:** Keep **implementation** aligned with everything agreed in planning — **no silent drift** from target state, policy, or review discipline. Use this **before merge** on every PR that touches descriptor/plan/builder/runtime contract work.

**When to use:** (1) Author — self-check before opening PR. (2) Reviewer — alongside [issue-review-protocol.md](issue-review-protocol.md). (3) Maintainer — spot audit when scope looks fuzzy.

---

## 1. Non-negotiables (repo + planning)

| Constraint | Source |
|------------|--------|
| **Plan JSON is the only C#↔JS contract** — runtime does not invent behavior | [CLAUDE.md](../../../CLAUDE.md), [descriptor-design-target-state.md](../descriptor-design-target-state.md) |
| **No silent fallbacks** — missing registration / vendor / `readExpr` → **throw**, not guess | [descriptor-design-target-state.md](../descriptor-design-target-state.md), [CODE-SMELLS §4](CODE-SMELLS.md#fallbacks-fail-fast) |
| **North star** — one schema; `Render` → `ResolveAll` → serialize; no post-hoc patch of done descriptors (**A**+**C**); no silent multi-segment loss (**F3**); no impossible `StatusHandler` (**D**) | [README — North star](README.md#north-star-all-issues) |
| **Explicit defaults** — OK when **named** (factory, VO builder, policy object); **not** hidden mutation or order-dependent patch | [§ below](#defaults-and-explicit-policy) |

---

## 2. Language & static analysis (per PR)

| Check | Pass when |
|-------|-----------|
| **C# 8** on core libs | [`Alis.Reactive.csproj`](../../../Alis.Reactive/Alis.Reactive.csproj) and slices that pin `<LangVersion>8</LangVersion>` — **no** `record` / `init` / primary constructors unless a **dedicated** issue raises language version | [CODE-SMELLS — C# 8](CODE-SMELLS.md#csharp-language-version) |
| **Constructor arity** | **No** new **5+** positional parameters on public descriptor/builder surfaces without **Discussion** waiver — use VO / nested types per [CODE-SMELLS §1](CODE-SMELLS.md#1-constructor-arity) |
| **CODE-SMELLS** | Issue **§2** *Code smells* table + canonical list satisfied | [CODE-SMELLS.md](CODE-SMELLS.md) |
| **Sonar (if CI)** | [CODE-SMELLS §5](CODE-SMELLS.md#sonar-community-csharp) rules triaged; suppress only with policy + comment | [INVEST-rubric — merge gate](INVEST-rubric.md#merge-gate) |

---

## 3. Process (every mergeable task)

| Check | Pass when |
|-------|-----------|
| **INVEST** | All six letters **≥ 4** or **Risk accepted** with owner + follow-up + link | [INVEST-rubric.md](INVEST-rubric.md) |
| **Merge gate** | All items in **Merge gate** + **Language** (item 4) satisfied | [INVEST-rubric.md](INVEST-rubric.md) |
| **Tests** | Issue file **§5 Test case catalog** IDs for this slice **green** (or **deferred** with issue link) | Issue `issue-*.md` |
| **Review depth** | Not surface-level — [issue-review-protocol.md](issue-review-protocol.md) inputs/outputs | Protocol |
| **Waiver** | Anything below bar → row in issue **Discussion & decisions** | Issue `issue-*.md` |

---

## 4. Umbrella issues A–F — drift risks to re-check

| Issue | Target state (one line) | **Drift risk** — do **not** ship |
|-------|-------------------------|----------------------------------|
| **A** | Immutable command leaves; remove `GuardWith` hot path | New mutators “just for tests”; **LangVersion** bump without issue |
| **B** | Thin façade; collaborators own segments/mode/buffer | **Default mode** fallbacks; public collaborators; **JSON** change without migration issue |
| **C** | Return-new wire; **VO** for `RequestDescriptor`; fluent DSL unchanged | In-place `Enrich*`; **9-param ctor** preserved; **Pre-C** skipped |
| **D** | `StatusHandler` union / discriminant; schema+TS+C# lockstep | Nullable dual branch + silent precedence; **LangVersion** without issue |
| **E** | `ICommandEmitter`; narrow emit | **Public** `AddCommand` left; **fat** emitter API; `InternalsVisibleTo` without Discussion |
| **F** | F1 null checks; F2 docs; F3 throw/rename — **no** silent segment drop; F4 deferred | **F3** still returns `reactions[0]` when `Count>1`; **F4** started without program |

**Issue files:** [issue-A](issue-A-mutable-descriptors.md) … [issue-F](issue-f-entry-segments.md).

---

## 5. Named tasks — map to tests & constraints

| Label | Issue | Re-read | Test IDs (issue §5) |
|-------|-------|---------|----------------------|
| **Pre-C** | [C](issue-c-http-validation.md) | Baseline locked before resolver refactor | C-T1… (snapshots) |
| **A1** / **A2** | [A](issue-A-mutable-descriptors.md) | A vs C scope | A-T1… |
| **B1** / **B2** | [B](issue-B-pipeline-builder.md) | B before E if stable | B-T1… |
| **C1** / **C2** | [C](issue-c-http-validation.md) | Split size & Pre-C | C-T1…, **C-T8** (VO round-trip) |
| **F1–F4** | [F](issue-f-entry-segments.md) | F3 ↔ B coordination | F-T1… |

**Every** row: **INVEST** + **CODE-SMELLS** + **C# 8** + issue **§5** where applicable.

---

## Defaults and explicit policy

<a id="defaults-and-explicit-policy"></a>

Target state **does not** block “defaults” in DSLs. It **requires** them to be **explicit** (factory, VO builder, named policy) — **not** hidden mutation. That is **intentional** and **good** for consistency across Native, Fusion, and future DSLs.

---

## Sign-off (paste in PR)

```text
## Guardrails (IMPLEMENTATION-GUARDRAILS.md)

- [ ] §1 Non-negotiables — no silent fallbacks; contract unchanged unless migration issue
- [ ] §2 C# 8 + arity + CODE-SMELLS + Sonar triage (if CI)
- [ ] §3 INVEST ≥4, merge gate, test IDs, protocol review
- [ ] §4 Umbrella issue(s) — **Drift risk** rows checked for this PR
- [ ] §5 Named task label(s) — if applicable, test IDs listed
```
