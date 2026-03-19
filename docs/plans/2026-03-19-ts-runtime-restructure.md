# TS Runtime Module Restructure — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the flat 30-file `Scripts/` directory into a screaming architecture that makes SOLID violations impossible by structure, mapping TS directories to C# projects.

**Architecture:** Move files into domain directories (`types/`, `core/`, `resolution/`, `execution/`, `conditions/`, `http/`, `validation/`, `lifecycle/`, `components/{native,fusion,lab}/`). Split `types.ts` (285 lines) into 9 domain files with barrel re-export. Add `assertNever` exhaustiveness to all 8 discriminated union switches. Update all import paths in 30 source files and 56 test files. Update esbuild entry path in `package.json`.

**Tech Stack:** TypeScript, esbuild, vitest

**Constraints:**
- ZERO logic changes — only file moves + import path updates
- 779 BDD tests must pass after every task
- esbuild bundle must produce identical output (same exports, same auto-boot behavior)
- `npm run build`, `npm run build:test-widget`, `npm run typecheck`, `npm test` must all pass

---

## File Map — Before → After

| Before (flat) | After (structured) |
|---|---|
| `Scripts/types.ts` | `Scripts/types/plan.ts`, `triggers.ts`, `reactions.ts`, `commands.ts`, `guards.ts`, `sources.ts`, `http.ts`, `validation.ts`, `context.ts`, `index.ts` |
| `Scripts/walk.ts` | `Scripts/core/walk.ts` |
| `Scripts/trace.ts` | `Scripts/core/trace.ts` |
| *(new)* | `Scripts/core/assert-never.ts` |
| `Scripts/resolver.ts` | `Scripts/resolution/resolver.ts` |
| `Scripts/component.ts` | `Scripts/resolution/component.ts` |
| `Scripts/execute.ts` | `Scripts/execution/execute.ts` |
| `Scripts/commands.ts` | `Scripts/execution/commands.ts` |
| `Scripts/element.ts` | `Scripts/execution/element.ts` |
| `Scripts/conditions.ts` | `Scripts/conditions/conditions.ts` |
| `Scripts/pipeline.ts` | `Scripts/http/pipeline.ts` |
| `Scripts/http.ts` | `Scripts/http/http.ts` |
| `Scripts/gather.ts` | `Scripts/http/gather.ts` |
| `Scripts/validation/` | `Scripts/validation/` *(unchanged)* |
| `Scripts/validation.ts` | `Scripts/validation.ts` *(barrel, unchanged)* |
| `Scripts/boot.ts` | `Scripts/lifecycle/boot.ts` |
| `Scripts/enrichment.ts` | `Scripts/lifecycle/enrichment.ts` |
| `Scripts/merge-plan.ts` | `Scripts/lifecycle/merge-plan.ts` |
| `Scripts/inject.ts` | `Scripts/lifecycle/inject.ts` |
| `Scripts/drawer.ts` | `Scripts/components/native/drawer.ts` |
| `Scripts/loader.ts` | `Scripts/components/native/loader.ts` |
| `Scripts/checklist.ts` | `Scripts/components/native/checklist.ts` |
| `Scripts/native-action-link.ts` | `Scripts/components/native/native-action-link.ts` |
| `Scripts/confirm.ts` | `Scripts/components/fusion/confirm.ts` |
| `Scripts/test-widget.ts` | `Scripts/components/lab/test-widget.ts` |
| `Scripts/auto-boot.ts` | `Scripts/auto-boot.ts` *(rename to `root.ts`, stays at root — the entry point)* |

---

## Task 1: Create `core/assert-never.ts` + Add Exhaustiveness to All Switches

**Why first:** Zero-risk strict improvement. No file moves. Establishes the pattern before restructure.

**Files:**
- Create: `Scripts/core/assert-never.ts`
- Modify: `Scripts/execute.ts`, `Scripts/commands.ts`, `Scripts/element.ts`, `Scripts/conditions.ts`, `Scripts/trigger.ts`, `Scripts/resolver.ts`, `Scripts/boot.ts`

- [ ] **Step 1: Create `Scripts/core/assert-never.ts`**

```typescript
/** Exhaustiveness check for discriminated union switches. Compile-time error if a case is missing. */
export function assertNever(value: never, context: string): never {
  throw new Error(`[alis] Unhandled ${context}: ${(value as any).kind ?? value}`);
}
```

- [ ] **Step 2: Add `assertNever` to `commands.ts` switch on `cmd.kind`**

Add import: `import { assertNever } from "./core/assert-never";`
Add after `case "into"` block: `default: assertNever(cmd, "command kind");`

- [ ] **Step 3: Add `assertNever` to `execute.ts` — both sync and async switches on `reaction.kind`**

Add import. Add `default: assertNever(reaction, "reaction kind");` in both switches.

- [ ] **Step 4: Add `assertNever` to `element.ts` switch on `cmd.mutation.kind`**

- [ ] **Step 5: Add `assertNever` to `conditions.ts` switch on `guard.kind`** (both sync and async)

- [ ] **Step 6: Add `assertNever` to `trigger.ts` switch on `trigger.kind`**

- [ ] **Step 7: Add `assertNever` to `resolver.ts` switch on `source.kind`**

- [ ] **Step 8: Run `npm run typecheck && npm test`**

Expected: ALL pass. No logic changed — only default cases added.

- [ ] **Step 9: Commit**

```
feat: add assertNever exhaustiveness checks to all discriminated union switches
```

---

## Task 2: Split `types.ts` into Domain Files with Barrel Re-Export

**Why second:** Kills Divergent Change. Barrel re-export means NO other files need updating yet.

**Files:**
- Create: `Scripts/types/plan.ts`, `triggers.ts`, `reactions.ts`, `commands.ts`, `guards.ts`, `sources.ts`, `http.ts`, `validation.ts`, `context.ts`, `index.ts`
- Delete: `Scripts/types.ts` (after barrel is in place)

- [ ] **Step 1: Create directory `Scripts/types/`**

- [ ] **Step 2: Create `Scripts/types/context.ts`**

Extract: `ExecContext`, `CoercionType`, `Vendor`, `EventPayload`

- [ ] **Step 3: Create `Scripts/types/sources.ts`**

Extract: `BindSource`, `EventSource`, `ComponentSource`

- [ ] **Step 4: Create `Scripts/types/triggers.ts`**

Extract: `Trigger`, `DomReadyTrigger`, `CustomEventTrigger`, `ComponentEventTrigger`
Import `Vendor` from `./context`

- [ ] **Step 5: Create `Scripts/types/guards.ts`**

Extract: `Guard`, `ValueGuard`, `AllGuard`, `AnyGuard`, `InvertGuard`, `ConfirmGuard`, `GuardOp`, `Branch`
Import `BindSource` from `./sources`, `CoercionType` from `./context`

- [ ] **Step 6: Create `Scripts/types/commands.ts`**

Extract: `Command`, `DispatchCommand`, `MutateElementCommand`, `ValidationErrorsCommand`, `IntoCommand`, `Mutation`, `SetPropMutation`, `CallMutation`, `MethodArg`, `LiteralArg`, `SourceArg`
Import from `./sources`, `./context`, `./guards`

- [ ] **Step 7: Create `Scripts/types/reactions.ts`**

Extract: `Reaction`, `SequentialReaction`, `ConditionalReaction`, `HttpReaction`, `ParallelHttpReaction`
Import from `./commands`, `./guards`, `./http`

- [ ] **Step 8: Create `Scripts/types/http.ts`**

Extract: `RequestDescriptor`, `StatusHandler`, `GatherItem`, `ComponentGather`, `StaticGather`, `AllGather`
Import from `./commands`, `./context`, `./sources`, `./guards`

- [ ] **Step 9: Create `Scripts/types/validation.ts`**

Extract: `ValidationDescriptor`, `ValidationField`, `ValidationRule`, `ValidationRuleType`, `ValidationCondition`

- [ ] **Step 10: Create `Scripts/types/plan.ts`**

Extract: `Plan`, `ComponentEntry`, `Entry`
Import from `./triggers`, `./reactions`

- [ ] **Step 11: Create `Scripts/types/index.ts` — barrel re-export**

```typescript
export * from "./plan";
export * from "./triggers";
export * from "./reactions";
export * from "./commands";
export * from "./guards";
export * from "./sources";
export * from "./http";
export * from "./validation";
export * from "./context";
```

- [ ] **Step 12: Delete `Scripts/types.ts`**

Since `import ... from "./types"` resolves to `./types/index.ts`, ALL existing imports work unchanged.

- [ ] **Step 13: Run `npm run typecheck && npm test && npm run build`**

Expected: ALL pass. Barrel re-export makes this transparent.

- [ ] **Step 14: Commit**

```
refactor: split types.ts into 9 domain files with barrel re-export
```

---

## Task 3: Move Core Utilities — `walk.ts`, `trace.ts`

**Files:**
- Move: `Scripts/walk.ts` → `Scripts/core/walk.ts`
- Move: `Scripts/trace.ts` → `Scripts/core/trace.ts`
- Update imports in: every file that imports from `./walk` or `./trace`

- [ ] **Step 1: `git mv Scripts/walk.ts Scripts/core/walk.ts`**

- [ ] **Step 2: `git mv Scripts/trace.ts Scripts/core/trace.ts`**

- [ ] **Step 3: Update all import paths**

Source files importing `./trace`: auto-boot, boot, commands, component, conditions, confirm, element, enrichment, execute, gather, http, native-action-link, pipeline, resolver, trigger
Source files importing `./walk`: component, resolver, trigger, validation/orchestrator

Test files importing `../trace` or `../walk`: update to `../core/trace`, `../core/walk`

- [ ] **Step 4: Update `assert-never.ts` path if needed** (already in `core/`, no change)

- [ ] **Step 5: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 6: Commit**

```
refactor: move walk.ts and trace.ts to core/
```

---

## Task 4: Move Resolution Modules — `resolver.ts`, `component.ts`

**Files:**
- Move: `Scripts/resolver.ts` → `Scripts/resolution/resolver.ts`
- Move: `Scripts/component.ts` → `Scripts/resolution/component.ts`
- Update imports in: conditions, element, gather, trigger, validation/orchestrator, and test files

- [ ] **Step 1: Create `Scripts/resolution/` and git mv both files**

- [ ] **Step 2: Update all import paths**

Files importing `./resolver`: conditions, element
Files importing `./component`: element, gather, resolver, trigger, validation/orchestrator
Test files importing `../resolver`, `../component`: update paths

- [ ] **Step 3: Update internal imports** within resolver.ts (`./walk` → `../core/walk`, `./component` → `./component` stays same)

- [ ] **Step 4: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 5: Commit**

```
refactor: move resolver.ts and component.ts to resolution/
```

---

## Task 5: Move Execution Modules — `execute.ts`, `commands.ts`, `element.ts`

**Files:**
- Move: `Scripts/execute.ts` → `Scripts/execution/execute.ts`
- Move: `Scripts/commands.ts` → `Scripts/execution/commands.ts`
- Move: `Scripts/element.ts` → `Scripts/execution/element.ts`
- Update imports in: trigger, http, native-action-link, boot, pipeline, and test files

- [ ] **Step 1: Create `Scripts/execution/` and git mv all three**

- [ ] **Step 2: Update all import paths**

- [ ] **Step 3: Update internal imports** within each moved file

- [ ] **Step 4: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 5: Commit**

```
refactor: move execute.ts, commands.ts, element.ts to execution/
```

---

## Task 6: Move Conditions Module — `conditions.ts`

**Files:**
- Move: `Scripts/conditions.ts` → `Scripts/conditions/conditions.ts`
- Update imports in: execute, commands, and test files

- [ ] **Step 1: Create `Scripts/conditions/` and git mv**

- [ ] **Step 2: Update all import paths**

- [ ] **Step 3: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 4: Commit**

```
refactor: move conditions.ts to conditions/
```

---

## Task 7: Move HTTP Modules — `pipeline.ts`, `http.ts`, `gather.ts`

**Files:**
- Move: `Scripts/pipeline.ts` → `Scripts/http/pipeline.ts`
- Move: `Scripts/http.ts` → `Scripts/http/http.ts`
- Move: `Scripts/gather.ts` → `Scripts/http/gather.ts`
- Update imports in: execute, and test files

- [ ] **Step 1: Create `Scripts/http/` and git mv all three**

- [ ] **Step 2: Update all import paths**

- [ ] **Step 3: Update internal imports** (pipeline imports http, http imports gather)

- [ ] **Step 4: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 5: Commit**

```
refactor: move pipeline.ts, http.ts, gather.ts to http/
```

---

## Task 8: Move Lifecycle Modules — `boot.ts`, `enrichment.ts`, `merge-plan.ts`, `inject.ts`

**Files:**
- Move: `Scripts/boot.ts` → `Scripts/lifecycle/boot.ts`
- Move: `Scripts/enrichment.ts` → `Scripts/lifecycle/enrichment.ts`
- Move: `Scripts/merge-plan.ts` → `Scripts/lifecycle/merge-plan.ts`
- Move: `Scripts/inject.ts` → `Scripts/lifecycle/inject.ts`
- Update imports in: auto-boot, trigger, commands, and test files

- [ ] **Step 1: Create `Scripts/lifecycle/` and git mv all four**

- [ ] **Step 2: Update all import paths**

- [ ] **Step 3: Update internal imports** (boot imports enrichment, merge-plan; inject imports boot)

- [ ] **Step 4: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 5: Commit**

```
refactor: move boot.ts, enrichment.ts, merge-plan.ts, inject.ts to lifecycle/
```

---

## Task 9: Move Component Modules — native, fusion, lab

**Files:**
- Move: `Scripts/drawer.ts` → `Scripts/components/native/drawer.ts`
- Move: `Scripts/loader.ts` → `Scripts/components/native/loader.ts`
- Move: `Scripts/checklist.ts` → `Scripts/components/native/checklist.ts`
- Move: `Scripts/native-action-link.ts` → `Scripts/components/native/native-action-link.ts`
- Move: `Scripts/confirm.ts` → `Scripts/components/fusion/confirm.ts`
- Move: `Scripts/test-widget.ts` → `Scripts/components/lab/test-widget.ts`
- Update imports in: auto-boot.ts, test files

- [ ] **Step 1: Create `Scripts/components/native/`, `Scripts/components/fusion/`, `Scripts/components/lab/`**

- [ ] **Step 2: git mv all six files**

- [ ] **Step 3: Update `auto-boot.ts` imports:**

```typescript
import { init as initConfirm } from "./components/fusion/confirm";
import { initNativeActionLinks } from "./components/native/native-action-link";
import "./components/native/drawer";
import "./components/native/loader";
import "./components/native/checklist";
```

- [ ] **Step 4: Update `package.json` build:test-widget path:**

```json
"build:test-widget": "esbuild Alis.Reactive.SandboxApp/Scripts/components/lab/test-widget.ts --bundle --format=iife --outfile=Alis.Reactive.SandboxApp/wwwroot/js/test-widget.js"
```

- [ ] **Step 5: Update test file imports** (`../test-widget` → `../components/lab/test-widget`, etc.)

- [ ] **Step 6: Run `npm run typecheck && npm test && npm run build && npm run build:test-widget`**

- [ ] **Step 7: Commit**

```
refactor: move drawer, loader, checklist, confirm, test-widget to components/{native,fusion,lab}/
```

---

## Task 10: Rename `auto-boot.ts` → `root.ts`

**Why:** The entry point lives at the root of `Scripts/`. Naming it `root.ts` makes its purpose scream.

**Files:**
- Rename: `Scripts/auto-boot.ts` → `Scripts/root.ts`
- Modify: `package.json` (esbuild entry point paths)

- [ ] **Step 1: `git mv Scripts/auto-boot.ts Scripts/root.ts`**

- [ ] **Step 2: Add comment at top of `root.ts`**

```typescript
// root.ts — ESM entry point for alis-reactive runtime
// esbuild bundles from here. Auto-discovers [data-alis-plan] elements on page load.
// Lives at Scripts/ root by design — everything else is organized in subdirectories.
```

- [ ] **Step 3: Update `package.json` build and watch scripts**

```json
"build": "esbuild Alis.Reactive.SandboxApp/Scripts/root.ts --bundle --outfile=Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js --format=esm --minify",
"watch": "esbuild Alis.Reactive.SandboxApp/Scripts/root.ts --bundle --outfile=Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js --format=esm --watch",
```

- [ ] **Step 4: Update test file imports** (`../auto-boot` → find and update if any test imports it — check `when-auto-booting-plans.test.ts`)

- [ ] **Step 5: Run `npm run typecheck && npm test && npm run build`**

- [ ] **Step 6: Commit**

```
refactor: rename auto-boot.ts to root.ts — entry point lives at root
```

---

## Task 11: Update `validation.ts` Barrel + Verify Final State

**Files:**
- Modify: `Scripts/validation.ts` (barrel — import paths may need updating)
- Verify: all builds, typecheck, tests

- [ ] **Step 1: Verify `Scripts/validation.ts` barrel paths still resolve**

The validation/ subdirectory didn't move, so `./validation/orchestrator` should still work. Confirm.

- [ ] **Step 2: Run full verification**

```bash
npm run typecheck
npm test
npm run build
npm run build:test-widget
npm run build:css
dotnet build
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

- [ ] **Step 3: Verify directory structure matches plan**

```bash
find Scripts -name "*.ts" -not -path "*__tests__*" -not -path "*__experiments__*" -not -path "*node_modules*" | sort
```

- [ ] **Step 4: Clean up `Scripts/__experiments__/` directory** (created during research)

```bash
rm -rf Scripts/__experiments__/
```

- [ ] **Step 5: Commit**

```
chore: verify restructure complete — all tests pass, clean up experiments
```

---

## Verification Checklist

After ALL tasks complete:

- [ ] `npm run typecheck` — zero errors
- [ ] `npm test` — 779+ tests pass
- [ ] `npm run build` — produces `alis-reactive.js`
- [ ] `npm run build:test-widget` — produces `test-widget.js`
- [ ] `dotnet build` — all C# projects compile
- [ ] `dotnet test tests/Alis.Reactive.PlaywrightTests` — all browser tests pass
- [ ] No `.ts` files remain in `Scripts/` root except `root.ts` and `validation.ts`
- [ ] `Scripts/types.ts` no longer exists (replaced by `Scripts/types/index.ts`)

## Risk Mitigation

- **Barrel re-export** in `types/index.ts` means existing `import from "./types"` resolves automatically
- Each task is independently committable — if one fails, prior commits are safe
- git mv preserves history
- esbuild follows imports from entry point — directory structure is transparent to bundling
- vitest resolves imports at test time — paths just need to be correct
