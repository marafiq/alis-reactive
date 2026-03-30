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
5. For each issue: emit §Finding block (no empty Evidence).
6. Bug (blocking)? **Mandatory:** minimal RED test exists AND the **same review comment** includes the repro artifact (no shortcuts). Paste full minimal test **or** paste failing command + output **or** gist/branch link **plus** pasted excerpt so the thread is self-contained. Do not claim blocking bug with only “add a test” / theory.
7. Doc/API/SOLID claims? Match §Evidence-by-category.
8. Consolidate thread; withdraw wrong claims; GitHub comment ends with — Cursor.
9. Default: reviewer-only; no drive-by refactors. Load matching .claude/skills/ for DSL reviews.
```

---

## Finding (copy per item)

```
Claim:   <one sentence>
Evidence: <path:line | test name | diff | command output | Not verified>
Category: Bug | Doc | API | Design | Process
Severity: Blocking | Gap | Polish
Next:    <verify step | repro plan | n/a>
```

**Severity:** Blocking = wrong/unsafe/contract break with evidence above. **Bug blocking** = red test + **artifact in this comment** (§Repro). Gap = factual doc/API typo with cites. Polish = optional.

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

| Category | Minimum evidence |
|----------|------------------|
| **Bug** | Failing test at owning layer + **repro submitted in the review comment** (paste minimal test, or paste `dotnet test` / `npm test` failure output, or link + required pasted excerpt). Artifact must **not** be a commit on the reviewed PR unless author asked. **No** blocking bug without in-comment artifact. Else → Hypothesis/Gap only. |
| **Doc / XML** | Doc anchor + `path:line` or symbol; contradictions = side-by-side doc + code. |
| **API surface** | `git diff` / build / analyzer; “breaking” = compile fail, test fail, schema break, or **named** in-repo consumers (`grep` paths). |
| **SOLID / design** | Letter (S/O/L/I/D) + code cite + rule/metric (e.g. ctor arity). SRP needs 2 reasons-to-change tied to code — else non-blocking or Hypothesis. |

---

## Repro artifact (bugs) — required in comment

**Reviewer obligation:** Do not post a blocking bug without attaching proof **in that GitHub comment**. Acceptable: (1) full minimal failing test as fenced code, (2) exact CLI + red output excerpt, (3) external link (gist / fork PR) **and** pasted minimal test or failure excerpt so readers need not leave GitHub to understand repro.

Still **do not** push repro commits **onto the PR under review** unless the author explicitly invites a fix branch.

---

## GitHub (agents)

End review comments with: `— Cursor`

---

## Extra depth (if branch has them)

`docs/archive/architecture-review/.../issue-review-protocol.md`, `CODE-SMELLS.md`, `descriptor-solid-analysis-plan.md` — confirm path exists.

---

*Process doc: `docs/process/`. PR outcomes: `docs/reviews/`.*
