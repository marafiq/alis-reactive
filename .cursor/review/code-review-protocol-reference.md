# Code review — expanded reference (Cursor)

**Binding rule (agents):** [`.cursor/rules/code-review-protocol.mdc`](../rules/code-review-protocol.mdc) — includes **Reviewer role** (default), runnable repro-in-comment, sign `— Cursor`.

**`@` paths:** Use only files under **`.cursor/`** (e.g. this file and the `.mdc`). Paths like `.worktrees/...` are local `git worktree` directories and are not a second copy of the protocol in the repo.

This file adds **harness**, **primitive checklist**, **repo gates**, and **evidence tables** only. Do not duplicate §Reviewer role or §Repro here; keep them in the `.mdc`.

---

**Tri-state:** **Verified** | **Not verified** | **Hypothesis** + next step. No *probably/seems/likely* on contracts unless Hypothesis.

## Agent workflow (follow in order)

```
0. Scope + base/head SHA + list files you OPENED (not diff-only).
1. Trust or run: npm run typecheck && npm run lint && npm test && dotnet build
   + layer tests from harness table below.
2. Map change → owning layer. Unexpected layer? Stop; trace C# → schema → TS → browser before LGTM.
3. If new/changed plan primitive: complete §Primitive checklist or list explicit deferrals.
4. Scan §Repo gates if diff touches views, runtime, vendors, IDs, API visibility, slices.
5. For every finding (Blocking, Gap, or Polish): Comment criteria from .mdc — all fields.
6. Blocking bug: .mdc §Blocking bug + runnable repro in comment.
7. Doc/API/SOLID: §Evidence by category below.
8. Consolidate thread; withdraw wrong claims; GitHub — Cursor.
9. Reviewer-only on the PR per .mdc — no commits on reviewed branch unless author invites.
```

## Comment criteria (copy per item)

See fenced block in [code-review-protocol.mdc](../rules/code-review-protocol.mdc).

**Blocking bug:** also §Repro below + .mdc.

---

## Harness (pick rows that match the diff)

| Layer | Command / artifact |
|--------|-------------------|
| C# plan / descriptors | `dotnet test tests/Alis.Reactive.UnitTests`, `dotnet build` |
| JSON contract | `Alis.Reactive/Schemas/reactive-plan.schema.json` + schema test |
| TS runtime | `npm test`, `npm run typecheck`, `npm run lint`; tests under `Alis.Reactive.SandboxApp/Scripts/__tests__/` ([vitest.config.ts](../../vitest.config.ts)) |
| JS/CSS bundles | `npm run build:all` then `dotnet build` |
| Native / Fusion / FV | `dotnet test tests/Alis.Reactive.Native.UnitTests` (+ Fusion, FluentValidator) as touched |
| Browser | `dotnet test tests/Alis.Reactive.PlaywrightTests` + `TestResults/` (TRX/HTML per repo scripts) |

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

## Repo gates (flag if violated)

| Gate | Check |
|------|--------|
| Plan = contract | No manual JS / `addEventListener` in `.cshtml` / `window.alis` / inline script; boot = esbuild entry ([root.ts](../../Alis.Reactive.SandboxApp/Scripts/root.ts) on this layout). |
| Vendor | Vendor logic in **one** canonical module; `rg` to find boundary before accusing; no sprawl. |
| IDs | Plan-driven IDs; no new DOM-wide scans / `querySelectorAll` for discovery. |
| Fail fast | No silent fallbacks for missing registration/vendor without explicit waiver + rationale. |
| Slices | No new shared **behavioral** base classes across vertical slices. |
| C# | Libraries = **C# 8.0** (per csproj); apps/tests may use newer language version. |
| Public API | Treat visibility/surface changes as high risk; honor any repo hook or policy that blocks API edits **if present**. |

---

## Evidence by category

| Category | Minimum evidence |
|----------|------------------|
| **Bug** | **Blocking:** failing test + repro in comment (runnable). Not on reviewed PR unless author asked. Non-blocking: Issue/Why/Files read/Self-check. Else → Hypothesis. |
| **Doc / XML** | Doc anchor + `path:line` or symbol; contradictions = side-by-side doc + code. |
| **API surface** | `git diff` / build / analyzer; “breaking” = compile fail, test fail, schema break, or **named** in-repo consumers (`grep` paths). |
| **SOLID / design** | Letter (S/O/L/I/D) + code cite + rule/metric. SRP needs 2 reasons-to-change tied to code — else non-blocking or Hypothesis. |

---

## Repro artifact (blocking bugs)

Align with [.mdc](../rules/code-review-protocol.mdc): **extracted**, **runnable** from the comment (paste-ready test or exact CLI + full output); gist/fork only with same body pasted in-thread. No commits on reviewed PR unless author invites.

---

## GitHub (agents)

End review comments with: `— Cursor`
