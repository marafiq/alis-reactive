# Issue review protocol — inputs, outputs, line-by-line (no surface-level)

**Purpose:** Every review of an **issue file**, a **PR** for that issue, or a **task** inside it must use this protocol so **target state**, **actual code**, and **agreed discussion** stay aligned—**no** skim reviews.

**Related:** [INVEST-rubric.md](INVEST-rubric.md) (per-task scores), [README.md](README.md) (master target state), [descriptor-design-target-state.md](../descriptor-design-target-state.md) (policy + inventory), [CODE-SMELLS.md](CODE-SMELLS.md) (includes **C# 8** constraint for core projects per `Alis.Reactive.csproj`), [IMPLEMENTATION-GUARDRAILS.md](IMPLEMENTATION-GUARDRAILS.md) (anti-drift checklist before merge).

---

## Inputs (reviewer must have before saying “LGTM”)

| Input | Why |
|-------|-----|
| **Issue file** (`issue-A-…md` … `issue-F-…md`) | **Target state** + **discussion log** + INVEST + test IDs + diagrams for **this** slice. |
| **Master** [README.md](README.md) | Bigger picture: system diagram, `Render` flow, dependency graph. |
| **Analysis plan** [descriptor-solid-analysis-plan.md](../descriptor-solid-analysis-plan.md) — **Issue §** uses anchors `#issue-a` … `#issue-f` (HTML ids before each `### IssueX` in Part 3) | **What / Why / How / Because** — line-level rationale vs code. |
| **Actual code** | Open the **cited** `.cs` (and `.ts` / schema if wire changes) — **same paths** as the issue file links (`../../../Alis.Reactive/...`). |
| **Tests** | The issue’s **test catalog** (A-T1, C-T1, …) + **existing** `tests/` / `Scripts/__tests__/` files that cover the behavior. |
| **Discussion log** table in the issue file | What the team **already decided**; review must not contradict without **new** row. |
| **[CODE-SMELLS.md](CODE-SMELLS.md)** + issue-specific §2 table | Constructor **>4** params (**5+**), SOLID, dead code, fallbacks — **per task**; waiver only via Discussion log. |

---

## Outputs (what a review must produce)

| Output | Required? |
|--------|-----------|
| **Line-by-line findings** | **Yes** — numbered list mapping **issue doc line** or **§** → **code line** → **verdict** (match / wrong / outdated). |
| **INVEST table** | Filled 1–5 per letter for **this** PR/task, or **waiver** with owner. |
| **Test coverage map** | Each test ID in the issue **green** or **explicitly** deferred with issue link. |
| **Doc follow-ups** | If code drifted from issue file, **either** update issue **or** fix code—note which. |
| **Blockers** | Anything that is **surface-level** approval (e.g. “looks fine”) without opening files — **invalid** sign-off. |

**Anti-pattern:** “Approved” with no file:line citations when the PR touches contract or public API.

### Documentation-only PRs

When the change is **only** markdown/planning (no product code):

- **Line-by-line** applies to **doc claims** vs **referenced sources** (issue file rows vs linked `.cs` / schema / tests **as cited**), not a requirement to re-diff unrelated modules.
- **Outputs** still require: which issue sections were verified, list of **files opened**, and **verdict** per claim (aligned / outdated / wrong link).
- **INVEST** and **test IDs** are **N/A** unless the doc change adds or renames a mergeable task.

---

## Line-by-line review (mandatory depth)

Do **not** stop at headings. For **each** row in the issue’s problem statement, test catalog, and INVEST pass rows:

1. **Doc claim** — Quote or point to the exact bullet/table row.
2. **Actual** — Open the **file** and **line range** (e.g. `PipelineBuilder.cs:137-142`).
3. **Task** — State which **task ID** (F1, Pre-C, …) implements the claim.
4. **Verdict** — **Aligned** | **Doc wrong** | **Code wrong** | **Needs test**.

Repeat until **every** actionable row is covered or explicitly **out of scope** for this PR.

### Surface-level (reject as incomplete)

- Approving from memory without opening **linked** sources.
- Checking only the PR diff without **issue test IDs**.
- Ignoring **discussion log** when behavior matches an **old** agreement.

---

## Mapping: issue file sections → review focus

| Section | Line-by-line check |
|---------|---------------------|
| **Target state (bigger picture)** | Matches [README](README.md) North star; no contradiction with [target-state doc](../descriptor-design-target-state.md). |
| **Discussion & decisions** | PR matches latest **Outcome** rows. |
| **Problem statement table** | Each **Location** path exists; **Today** matches current source. |
| **INVEST** | Each **Pass when** has evidence in PR or linked tests. |
| **Code smells** | [CODE-SMELLS.md](CODE-SMELLS.md) + issue §2: no new **5+** param ctor without waiver; no SOLID/dead-code/fallback regressions per issue table. |
| **Test catalog** | Each ID has a **test method** or **Verify** file; **old** tests still run unless replaced with **same** behavior proof. |
| **Diagrams** | Still accurate after code change (update diagram if behavior changes). |

---

## Sign-off template (paste in PR)

```text
## Review protocol (issue-review-protocol.md)

Inputs used: [list: issue file, README, analysis §, files opened]
Line-by-line: [link to comment thread or attach numbered findings]
INVEST: I=_ N=_ V=_ E=_ S=_ T=_ (each ≥4 or waiver: ___)
Test IDs verified: [list]
Blockers: none | [list]
```
