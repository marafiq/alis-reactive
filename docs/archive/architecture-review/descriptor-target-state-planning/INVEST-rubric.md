# INVEST rubric — 5-point scale (shared)

**What counts as a “task”?** **Any** mergeable unit of work—not only the umbrella labels “Issue A” … “Issue F.” Examples: **Task F1**, **Task F2**, **Task F3**, **Task Pre-C**, **Task C1**, a single **B** extraction PR, a **Conditions**-only fix. **Each task is an INVEST task:** the same **six letters** scored **every time**, with the same pass threshold.

**Purpose:** Before a task merges, it **must** have a **full INVEST scorecard** (all six letters, 1–5 each). Umbrella docs ([`issue-A-…md`](issue-A-mutable-descriptors.md) … [`issue-F-…md`](issue-f-entry-segments.md)) give **templates** and **depth**; they do **not** replace per-task scoring for smaller slices. **Pass threshold:** **≥ 4** on every letter **or** documented waiver with maintainer sign-off.

**SonarQube Community (C#):** [CODE-SMELLS.md §5](CODE-SMELLS.md#sonar-community-csharp) maps **SOLID / encapsulation / arity / dead code / swallow** smells to **sonar-dotnet** rule keys. Strengthens **V**, **T**, and **S** when the project runs Sonar (or compatible analyzers) on touched files — triage new issues in the PR or waive in the issue **Discussion** table.

**Scale**

| Score | Meaning |
|-------|---------|
| **1** | Not addressed — no evidence, or actively violates the criterion. |
| **2** | Fragmentary — partial intent, no test binding, or unclear scope. |
| **3** | Adequate for spike — enough to prototype; **not** enough to merge to `main`. |
| **4** | **Pass** — meets criterion with evidence (tests, diagrams, PR checklist). |
| **5** | Exceeds — reusable patterns, automation (analyzers), or docs that prevent regression class-wide. |

---

## I — Independent

| Score | Evidence required |
|-------|-------------------|
| 1 | Blocked on unspecified other work; no vertical slice. |
| 2 | Could ship but still coupled to “big bang” PR. |
| 3 | Delivers value in one branch; some follow-ups assumed. |
| 4 | **Mergeable alone**; dependencies on other issues are **explicit** and **optional** or **ordered** in README dependency graph. |
| 5 | Feature flags / phased API + **documented** rollback path per slice. |

---

## N — Negotiable

| Score | Evidence required |
|-------|-------------------|
| 1 | Fixed design with no room for implementation tradeoffs. |
| 2 | Only cosmetic negotiation. |
| 3 | One alternative (e.g. throw vs obsolete) listed, not decided. |
| 4 | **Acceptance criteria fixed**; implementation strategy **documented** with chosen option + rationale. |
| 5 | ADR-style decision write-up; alternatives rejected with **test** or **measurement** reason. |

---

## V — Valuable

| Score | Evidence required |
|-------|-------------------|
| 1 | Refactor for aesthetics only. |
| 2 | Theoretical SOLID benefit, no failure mode removed. |
| 3 | Removes **one** known pain (documented). |
| 4 | **Named** user/maintainer outcome: bug class, merge conflict rate, or reasoning clarity. |
| 5 | Quantified or **before/after** trace (e.g. silent-loss class eliminated). |

---

## E — Estimable

| Score | Evidence required |
|-------|-------------------|
| 1 | No file list, no test count. |
| 2 | Rough guess only. |
| 3 | File list + **order of magnitude** (days). |
| 4 | **Anchored** estimate: grep counts, test inventory, **risk buffer** named. |
| 5 | Historical velocity or similar past PR used for calibration. |

---

## S — Small

| Score | Evidence required |
|-------|-------------------|
| 1 | Single PR > ~800 LOC touched without split plan. |
| 2 | Large PR with “we’ll split if needed.” |
| 3 | Fits one sprint with focus. |
| 4 | **Fits one PR** or **explicit** B1/B2 with separate merges. |
| 5 | Each commit is reviewable; mechanical vs semantic commits separated. |

---

## T — Testable

| Score | Evidence required |
|-------|-------------------|
| 1 | “We’ll add tests later.” |
| 2 | Happy-path test only. |
| 3 | Unit tests for core path. |
| 4 | **Eval criteria written first** (issue file § Test catalog); unit + schema **minimum**; TS/Playwright when wire/DOM. **Recommended:** no **new** Sonar **Critical** / profile “blockers” on touched code ([§5 mapping](CODE-SMELLS.md#sonar-community-csharp)), or listed with waiver. |
| 5 | Regression suite extended; **negative** cases; snapshot or hash lock where applicable. **Sonar-clean** on changed files **or** every finding explained + linked waiver. |

---

## Merge gate

A PR for issue **A–F** **must**:

1. Fill the INVEST table in the corresponding `issue-*.md` (append scores to PR description or link).
2. **No score below 4** without **Risk accepted** block: owner, reason, follow-up issue link.
3. **Static analysis:** If SonarQube / SonarCloud / IDE SonarLint runs on the repo, **address or triage** new issues on **changed** lines for rules in [CODE-SMELLS.md §5](CODE-SMELLS.md#sonar-community-csharp) (S107, S1144, S1200, S138, S2486, S3215, S3358, S3776, S1104). **Suppress** only with team policy + PR comment (and **Discussion** row if policy-level).
4. **Language:** Core libraries ([`Alis.Reactive.csproj`](../../../Alis.Reactive/Alis.Reactive.csproj) and vertical slices that pin `<LangVersion>8</LangVersion>`) — **no** C# 9+ features (`record`, `init`, primary constructors) in implementation unless a **dedicated** issue raises `LangVersion`. See [CODE-SMELLS.md — C# 8 constraint](CODE-SMELLS.md#csharp-language-version).
