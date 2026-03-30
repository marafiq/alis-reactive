# Code review protocol (compact)

**Law:** [CLAUDE.md](../../CLAUDE.md) wins over this sheet.  
**Rule:** Every finding = **Verified** | **Not verified** | **Hypothesis** + next evidence step. No *probably/seems/likely* on behavior or APIs unless labeled Hypothesis.

---

## Agent workflow (follow in order)

```
0. Scope + base/head SHA + list files you OPENED (not diff-only).
1. Trust or run: npm run typecheck && npm run lint && npm test && dotnet build
   + layer tests from harness table below.
2. Map change → owning layer. Unexpected layer? Stop; trace C# → schema → TS → browser before LGTM.
3. If new/changed plan primitive: complete §Primitive checklist or list explicit deferrals.
4. Scan §CLAUDE gates if diff touches views, runtime, vendors, IDs, API visibility, slices.
5. For **every** finding (Blocking, Gap, **or** Polish): emit §Comment criteria — all fields filled; no empty Evidence / “nit” without cites.
6. Bug (blocking)? **Extra mandatory:** §Repro artifact **in this comment** (minimal red test + paste/link+excerpt). Non-blocking still needs §Comment criteria; only blocking bugs need pasted repro in-thread.
7. Doc/API/SOLID claims? Match §Evidence-by-category.
8. Consolidate thread; withdraw wrong claims; GitHub comment ends with — Cursor.
9. Default: reviewer-only; no drive-by refactors. Load matching .claude/skills/ for DSL reviews.
```

---

## Comment criteria (every severity — copy per item)

**Blocking, Gap, and Polish** all require evidence-backed comments. Polish is not a free pass for opinion without cites.

```
Issue:        <what is wrong or unclear — one concrete statement>
Why:          <why you believe that — tie to path:line, symbol, diff hunk, or test; no vibes>
Change:       <what should change (or “no code change — doc only”)>
Benefit:      <what the change improves: correctness, clarity, risk, maintainability>
Files read:   <list every file you opened to form this comment>
Self-check:   <yes/no — I re-read the cited lines after writing; quote adjusted if mismatch>
Category:     Bug | Doc | API | Design | Process
Severity:     Blocking | Gap | Polish
Evidence:     <same as Why, or Not verified + next step>
```

**Blocking bug** (Category Bug + Severity Blocking): **also** satisfy §Repro (minimal red test + proof **in this GitHub comment**). Other severities: **no** pasted repro required unless you are demonstrating behavior — but **Issue / Why / Evidence / Files read / Self-check** are still mandatory.

---

## Harness (pick rows that match the diff)

| Layer | Command / artifact |
|--------|-------------------|
| C# plan / descriptors | `dotnet test tests/Alis.Reactive.UnitTests`, `dotnet build` |
| JSON contract | `Alis.Reactive/Schemas/reactive-plan.schema.json` + schema test |
| TS runtime | `npm test`, `npm run typecheck`, `npm run lint`; tests under `Alis.Reactive.SandboxApp/Scripts/__tests__/` ([vitest.config.ts](../../vitest.config.ts)) |
| JS/CSS bundles | `npm run build:all` then `dotnet build` |
| Native / Fusion / FV | `dotnet test tests/Alis.Reactive.Native.UnitTests` (+ Fusion, FluentValidator) as touched |
| Browser | `dotnet test tests/Alis.Reactive.PlaywrightTests` + `TestResults/` per CLAUDE.md |

---

## Primitive checklist (wire / command / trigger change)

1. C# descriptor + JSON polymorphism as repo does (`Serialization/`, `[JsonDerivedType]`, …)  
2. Builder surface  
3. Schema + `AssertSchemaValid` (or equivalent)  
4. TS types (`…/Scripts/types/` — layout per branch)  
5. Runtime handler + exhaustiveness (`assertNever` or repo pattern)  
6. C# `VerifyJson` + schema test  
7. TS `boot()` / vitest  
8. Playwright if user-visible  
9. Sandbox demo if applicable  

---

## CLAUDE gates (flag if violated)

| Gate | Check |
|------|--------|
| Plan = contract | No manual JS / `addEventListener` in `.cshtml` / `window.alis` / inline script; boot = esbuild entry ([root.ts](../../Alis.Reactive.SandboxApp/Scripts/root.ts) on this layout). |
| Vendor | Vendor logic in **one** canonical module per CLAUDE; `rg` before accusing; no sprawl. |
| IDs | Plan-driven IDs; no new DOM-wide scans / `querySelectorAll` for discovery. |
| Fail fast | No silent fallbacks for missing registration/vendor (Rule 10) without waiver. |
| Slices | No new shared **behavioral** base classes across vertical slices. |
| C# | Libraries = **C# 8.0**; apps/tests may be newer. |
| Public API | High risk; respect `.claude/hookify.protect-api-surface.local.md` **if present**. |

---

## Evidence by category

Applies inside **§Comment criteria** (`Why` + `Evidence`). **Blocking bugs** additionally need §Repro pasted in-comment.

| Category | Minimum evidence |
|----------|------------------|
| **Bug** | For **blocking:** failing test + **repro in this comment** (paste minimal test, or CLI red output, or link + pasted excerpt). Not on reviewed PR unless author asked. Non-blocking bug-shaped notes still need Issue/Why/Files read/Self-check. Else → Hypothesis only. |
| **Doc / XML** | Doc anchor + `path:line` or symbol; contradictions = side-by-side doc + code. |
| **API surface** | `git diff` / build / analyzer; “breaking” = compile fail, test fail, schema break, or **named** in-repo consumers (`grep` paths). |
| **SOLID / design** | Letter (S/O/L/I/D) + code cite + rule/metric (e.g. ctor arity). SRP needs 2 reasons-to-change tied to code — else non-blocking or Hypothesis. |

---

## Repro artifact (blocking bugs only) — required in comment

**Only for Blocking + Bug.** Other severities use §Comment criteria (no pasted repro unless illustrating a point).

**Reviewer obligation:** Do not post a **blocking** bug without proof **in that GitHub comment**. Acceptable: (1) full minimal failing test as fenced code, (2) exact CLI + red output excerpt, (3) external link (gist / fork PR) **and** pasted minimal test or failure excerpt so readers need not leave GitHub to understand repro.

Still **do not** push repro commits **onto the PR under review** unless the author explicitly invites a fix branch.

---

## GitHub (agents)

End review comments with: `— Cursor`

---

## Extra depth (if branch has them)

`docs/archive/architecture-review/.../issue-review-protocol.md`, `CODE-SMELLS.md`, `descriptor-solid-analysis-plan.md` — confirm path exists.

---

*Process doc: `docs/process/`. PR outcomes: `docs/reviews/`.*
