# Code review protocol — evidence first, zero speculation

**Audience:** Human reviewers and agent-assisted review (Cursor, etc.).  
**Goal:** Every finding is **proven** or explicitly **Not verified** / **Hypothesis**. No time spent on ungrounded debate.

**Law of the repo:** [CLAUDE.md](../../CLAUDE.md) — architecture, pipeline, and rules override generic review habits.

---

## 1. Principle

- **Evidence-first** for every category: bugs, documentation/XML, public API surface, SOLID/design, security-relevant behavior.
- **Speculation** is allowed only as a labeled **Hypothesis** with a concrete **next evidence step** (command, file to open, test to add).
- Tri-state language for contracts and behavior: **Verified** | **Not verified (unread)** | **Hypothesis (needs evidence)**.
- Do **not** use *probably, likely, seems, obviously, clearly, might break, could be wrong* for behavior, APIs, or serialization — unless scoped as **Hypothesis** and followed by how to verify.

---

## 2. When this applies

- Pull requests (any size).
- Doc-only, API/XML-doc, TS/runtime, and cross-layer changes.
- Architecture / SOLID / coupling threads.
- External review comments (e.g. GitHub).

---

## 3. Pre-flight (stop if missing)

1. **Scope** — What is in / out (paths, layers, PR number).
2. **Refs** — Base and head **commit SHAs** (or PR equivalent).
3. **Opened files** — Explicit list of files you **read** for this review (not “skimmed the diff”).
4. **Automation** — Note CI green/red or commands you ran (see §6).

---

## 4. Universal output shape

For each finding, use:

| Field | Content |
|--------|---------|
| **Claim** | One factual sentence. |
| **Evidence** | `path:line`, test name, `git diff` hunk, command output, or **Not verified**. |
| **Category** | Bug / Doc / API / Design / Process. |
| **Severity** | Blocking / Gap / Polish (definitions in §11). |
| **Next step** | If not verified: exact step to verify; if Hypothesis: experiment or artifact. |

No row without **Evidence** or **Not verified**.

---

## 5. Layer → verification harness (Alis.Reactive)

Map the change to the **owning layer**, then require proof from the **right** harness. Pipeline: **C# descriptors → JSON plan → TS runtime → browser** ([CLAUDE.md](../../CLAUDE.md) — SOLID loop).

| Layer touched | Primary evidence |
|----------------|------------------|
| C# descriptors, builders, serialization | `dotnet test tests/Alis.Reactive.UnitTests` (Verify + `AssertSchemaValid` on plan JSON), `dotnet build` |
| JSON plan contract | Plan output vs [Alis.Reactive/Schemas/reactive-plan.schema.json](../../Alis.Reactive/Schemas/reactive-plan.schema.json); failing schema test if shape changed |
| TS types / runtime | `npm test`, `npm run typecheck`, `npm run lint`; handler coverage under **`Alis.Reactive.SandboxApp/Scripts/__tests__/`** (see [vitest.config.ts](../../vitest.config.ts) `include`) |
| Bundles + CSS | After TS/CSS change: **`npm run build:all` then `dotnet build`** (cache-busting / `asp-append-version`) |
| Native / Fusion / FluentValidator slices | `dotnet test tests/Alis.Reactive.Native.UnitTests`, `tests/Alis.Reactive.Fusion.UnitTests`, `tests/Alis.Reactive.FluentValidator.UnitTests` as applicable |
| Browser / sandbox | `dotnet test tests/Alis.Reactive.PlaywrightTests` — prefer TRX/HTML logger and `TestResults/` per [CLAUDE.md](../../CLAUDE.md) Playwright workflow |

**Wrong-layer stop:** If a PR touches an unexpected layer, pause and map impact across **C# → schema → TS types → runtime → tests → docs** before approving. Failing tests at boundaries are mandatory signals ([CLAUDE.md](../../CLAUDE.md) feedback loop).

---

## 6. Automation first

Run or trust CI for:

- `npm run typecheck`, `npm run lint`, `npm test`
- `dotnet build`
- Layer-specific `dotnet test` from §5

Reserve human/agent time for architecture, naming, and trade-offs — not for formatter output.

---

## 7. New or changed plan primitive (cross-layer checklist)

If the change adds or alters a **command kind, trigger kind, polymorphic descriptor, or wire shape**, the following must be complete or explicitly deferred with tracking:

1. C# descriptor (typically `internal` ctor, sealed where appropriate) + polymorphic JSON registration (**`[JsonDerivedType]`** / converter pattern **as used in repo** — verify in `Serialization/` before citing a specific converter type name).
2. Builder entry point (`PipelineBuilder` / `ElementBuilder` / `TriggerBuilder` / component API).
3. **JSON schema** update — [reactive-plan.schema.json](../../Alis.Reactive/Schemas/reactive-plan.schema.json) + failing-then-green `AssertSchemaValid` (or equivalent) in C# tests.
4. **TS types** — discriminated union in runtime `types/` (e.g. `Alis.Reactive.SandboxApp/Scripts/types/` — follow repo layout).
5. **Runtime handler** — switch / module case + exhaustiveness helper (**`assertNever`** or equivalent used in repo).
6. **C# unit test** — `VerifyJson` snapshot + schema validation.
7. **TS unit test** — `boot()` / jsdom behavior.
8. **Playwright** — browser behavior when user-visible.
9. **Sandbox** — demonstrable view when applicable.

(Aligned with [CLAUDE.md](../../CLAUDE.md) Rule 3 — all three layers; extend with schema + `assertNever` as the repo evolves.)

---

## 8. Repo-specific reviewer checklist (CLAUDE.md rules)

Use as **blocking** gates when the diff touches these areas:

| Topic | Reviewer action |
|--------|------------------|
| **Plan is the only contract** | No manual JS in views; no `document.addEventListener` in `.cshtml`; no `window.alis`; no inline `<script>` — boot via the **esbuild entry** (on this repo: [Alis.Reactive.SandboxApp/Scripts/root.ts](../../Alis.Reactive.SandboxApp/Scripts/root.ts); [CLAUDE.md](../../CLAUDE.md) may say `Scripts/` — paths follow checkout layout). |
| **Vendor isolation** | Per [CLAUDE.md](../../CLAUDE.md): vendor-specific resolution stays in **one** canonical module (often named `component.ts` in newer layouts). **Flag** new `vendor ===` / `ej2` branching outside that boundary — confirm the canonical file with `rg` on your branch before claiming a violation. |
| **Plan-driven IDs** | No new broad **`querySelectorAll`** / DOM scanning for IDs; runtime uses plan-carried IDs / `getElementById`. |
| **Fail fast** | Flag silent fallbacks, swallowed errors, or guessed defaults when data is missing — [CLAUDE.md](../../CLAUDE.md) Rule 10; require waiver + evidence if intentional. |
| **Vertical slices** | Flag new **shared behavioral base classes** across slices; duplication is often intentional. |
| **C# language version** | **Library** projects: C# **8.0** per csproj; apps/tests may use newer — invalid syntax in the wrong project is blocking. |
| **API surface** | Treat **public API / visibility** changes as high risk; if the repo uses hookify **`.claude/hookify.protect-api-surface.local.md`**, respect it when present. |

---

## 9. Bugs (blocking correctness)

- **Actual bug** = **minimal failing test** at the owning layer (**NUnit** / **Vitest** / **Playwright**).
- **Reproduction artifact — not part of the reviewed PR’s commits** unless the author explicitly asks for a fix branch. Acceptable: link to **separate branch/fork PR**, **gist**, **pasted self-contained test** in the comment, **CI log**, or **command transcript** showing red test.
- Without a red test (or equivalent observable failure), classify as **Hypothesis** or **Gap**, not **Blocking bug**.

---

## 10. Documentation & XML / API docs

- Each substantive claim: **doc anchor** + **source** (`path:line` or public symbol).
- Contradiction: **side-by-side** excerpt from doc + code.
- Code samples in review comments: **compile** on the branch under review, or mark **Example not executed**.
- Doc quality dimensions (accuracy, completeness, consistency) per industry practice — e.g. [documentation review checklist](https://deepdocs.dev/documentation-review-checklist/), [arc42 documentation principles](https://arc42.org/principles-of-technical-documentation).

---

## 11. Public API surface

- Assertions require **diff or symbol proof**: `git diff`, IDE outline, analyzer/`dotnet build` errors.
- **Breaking** requires a **concrete** failure: compile error, failing test, schema mismatch, or named in-repo consumer broken (list **file paths** from `grep`).
- Do **not** claim external consumers will break without naming them or showing a contract test failure.

API design stability checklist inspiration: [DX / API review dimensions](https://ozimmer.ch/patterns/2023/03/20/DXChecklist.html); contract discipline: [contract testing in code review](https://www.propelcode.ai/blog/microservices-api-contract-code-review-guide).

---

## 12. SOLID / design

- **No vibe-based smells.** A **violation** claim needs: **which letter (S/O/L/I/D)** + **code citation** + **rule** (team doc or cited principle) + **metric** where useful (e.g. ctor arity, dependency count).
- **SRP example bar:** one stated responsibility + **two conflicting reasons-to-change** tied to code — else **Design suggestion (non-blocking)** or **Hypothesis**.

**Supplementary deep dives (when present on the branch):**

- Descriptor initiative: `docs/archive/architecture-review/descriptor-target-state-planning/issue-review-protocol.md` — line-by-line issue reviews.
- `docs/archive/architecture-review/descriptor-target-state-planning/CODE-SMELLS.md` — constructor arity, SOLID tables.
- `docs/archive/architecture-review/descriptor-solid-analysis-plan.md` — analysis plan.

Paths are **repository-relative from repo root**; not every branch contains `docs/archive/` — confirm the file exists before relying on it.

---

## 13. Severity rubric

| Severity | When |
|----------|------|
| **Blocking** | Wrong behavior, security issue, contract break, or missing required layer proof (§7) — each with §4 evidence. **Bug blocking** requires §9 repro artifact. |
| **Gap** | Factual doc/typing error, missing `<param>`, incorrect statement — with doc + source cites. |
| **Polish** | Wording, optional examples, cross-links — label explicitly. |

---

## 14. Corrections & thread hygiene

- On counter-evidence, **withdraw** the claim immediately; do not defend without new proof.
- Prefer **one consolidated** follow-up over many partial comments.
- **Supersede** or edit GitHub comments that still contain false information so readers are not misled.

---

## 15. GitHub comments (agents)

When posting review feedback on GitHub, **sign** the comment for attribution, e.g.:

```text
— Cursor
```

(or `Signed: Cursor` if the thread prefers that form).

---

## 16. Agent-specific defaults

- **Reviewer-only** — do not edit the tracked tree unless the user explicitly requests implementation.
- No drive-by refactors or unrelated files.
- **Skills:** Load applicable `.claude/skills/` when reviewing DSL usage — skills must match source ([CLAUDE.md](../../CLAUDE.md) Skills section).

---

## 17. Small PRs and session limits

- Prefer **< ~400 LOC** per PR where practical; time-box review sessions (~60 minutes) to reduce fatigue misses.
- PR size guidance is industry heuristic, not a repo CI gate — use judgment.

---

## 18. Further reading (optional)

- Code review effectiveness overview: [DEV Community — code review practices](https://dev.to/rahulxsingh/code-review-best-practices-the-complete-guide-for-engineering-teams-2026-52a4)

---

*This document lives under `docs/process/` (process). Review **outcomes** for specific PRs belong under `docs/reviews/` when the team records them.*
