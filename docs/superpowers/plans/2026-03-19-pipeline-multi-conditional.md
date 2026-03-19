# Pipeline Multi-Conditional Fix — Implementation Plan (DRAFT)

> **Status:** DRAFT — needs full plan with TDD steps before execution.

**Goal:** Fix the pipeline builder so multiple `When().Then().Else()` blocks and post-condition commands work correctly in a single `.Reactive()` call.

**The Bug:** `GuardBuilder.Then()` line 142-143 creates a NEW branch list and calls `SetConditionalBranches()` which REPLACES the existing branches. Second `When()` overwrites the first.

---

## Proven Bug (from experiment)

```csharp
p.Element("echo").SetText("before");           // command 1
p.When(args, x => x.Value).Eq("hello")         // condition 1
    .Then(t => t.Element("r1").SetText("yes"))
    .Else(e => e.Element("r1").SetText("no"));
p.When(args, x => x.Count).Gt(5)               // condition 2
    .Then(t => t.Element("r2").SetText(">5"))
    .Else(e => e.Element("r2").SetText("<=5"));
p.Element("footer").SetText("after");           // command 2
```

**Expected:** Both conditions evaluate independently. Both r1 and r2 get set.
**Actual:** Only condition 2's branches exist. r1 is never set. "after" lands in pre-commands.

---

## Root Cause Trace

| Step | What happens | Problem |
|------|-------------|---------|
| `p.Element("echo").SetText("before")` | Adds command to `Commands` list | OK |
| `p.When(args, x => x.Value)` | Calls `SetMode(Conditional)`, returns ConditionSourceBuilder | OK |
| `.Eq("hello")` | Creates ValueGuard, returns GuardBuilder | OK |
| `.Then(t => ...)` | Creates new branch list `[Branch1]`, calls `SetConditionalBranches([Branch1])` | **REPLACES** ConditionalBranches |
| `.Else(e => ...)` | Adds to the list → `[Branch1, ElseBranch1]` | OK (same list) |
| `p.When(args, x => x.Count)` | Calls `SetMode(Conditional)` — already Conditional, passes | OK |
| `.Gt(5)` | Creates ValueGuard | OK |
| `.Then(t => ...)` | Creates NEW branch list `[Branch2]`, calls `SetConditionalBranches([Branch2])` | **OVERWRITES** — `[Branch1, ElseBranch1]` is GONE |
| `.Else(e => ...)` | Adds to new list → `[Branch2, ElseBranch2]` | OK (but first condition lost) |
| `p.Element("footer").SetText("after")` | Adds command to `Commands` list | Lands in pre-commands, not post |
| `BuildReaction()` | Creates `ConditionalReaction(Commands, ConditionalBranches)` | Only has condition 2 branches |

---

## Design Decision: How to Fix

### Option A: Multiple entries from one .Reactive() call
The pipeline builder segments its content into multiple entries when When() creates boundaries:
- Entry 1: SequentialReaction → `[command "before"]`
- Entry 2: ConditionalReaction → condition 1 branches
- Entry 3: ConditionalReaction → condition 2 branches
- Entry 4: SequentialReaction → `[command "after"]`

All share the same trigger. Runtime already supports multiple entries per trigger.

**Pro:** Zero runtime changes. Zero descriptor changes. Zero schema changes. Builder-only fix.
**Con:** Changes `BuildReaction()` to return multiple reactions. `.Reactive()` extension needs to handle multiple entries.

### Option B: New "composite" reaction type
A reaction kind that holds a sequence of sub-reactions:
```json
{ "kind": "composite", "steps": [
  { "kind": "sequential", "commands": [...] },
  { "kind": "conditional", "branches": [...] },
  { "kind": "conditional", "branches": [...] },
  { "kind": "sequential", "commands": [...] }
]}
```

**Pro:** Single entry per trigger. Clean plan shape.
**Con:** New reaction kind in descriptor, schema, types.ts, execute.ts. More changes.

### Option C: Accumulate branches (simple but wrong)
Just append second When's branches to first's list.

**Pro:** Minimal change.
**Con:** WRONG — runtime does first-match-wins across all branches. If condition 1 matches, condition 2 never evaluates. The two conditions are NOT alternatives — they're independent.

### Recommendation: Option A
Zero runtime/descriptor/schema changes. The builder produces multiple entries from one pipeline. The `.Reactive()` extension already calls `plan.AddEntry()` — it just needs to call it multiple times. The `PipelineBuilder` needs to track "segments" and `BuildReactions()` (plural) returns a list.

---

## Files That Change

| File | Change |
|------|--------|
| `Alis.Reactive/Builders/PipelineBuilder.cs` | Track segments. `BuildReactions()` returns `List<Reaction>` |
| `Alis.Reactive/Builders/PipelineBuilder.Conditions.cs` | `When()` flushes current segment before starting new conditional |
| `Alis.Reactive/Builders/Conditions/GuardBuilder.cs` | `Then()` adds to current segment's branches, not replaces |
| Every `*ReactiveExtensions.cs` | Call `BuildReactions()` and add multiple entries |
| `Alis.Reactive/Builders/PipelineBuilder.Http.cs` | Verify HTTP mode still works (should be unaffected) |

**NOT changed:** Descriptors, schema, types.ts, execute.ts, resolver.ts, conditions.ts, gather.ts

---

## Test Plan (TDD — write failing tests FIRST)

### New C# Unit Tests
1. `Two_independent_when_blocks_both_produce_branches` — both conditions present in plan JSON
2. `Commands_before_first_when_are_pre_commands` — "before" is in first entry
3. `Commands_after_last_when_are_in_trailing_entry` — "after" is in last entry
4. `Single_when_still_produces_one_entry` — no regression
5. `Commands_only_still_produces_sequential` — no regression
6. `When_elseif_else_is_one_block_not_multiple` — ElseIf stays in same conditional

### New Playwright Tests
1. Fix sandbox views to merge `.Reactive()` (13 files from ALIS003 analyzer)
2. All existing Playwright tests must pass without changes

### Existing Tests
ALL must pass — this is a builder change, not a runtime change.

---

## ALIS003 Analyzer
Currently committed as Error severity. The analyzer is CORRECT — developers should write one `.Reactive()` per event. Once this fix lands, the merged pipeline will work correctly and the analyzer prevents the old pattern of duplicate entries.

---

## Scope
- Fix PipelineBuilder to support: command → condition → command → condition → command
- Fix sandbox views (merge duplicate .Reactive() calls)
- ALIS003 analyzer stays as Error
- NO runtime changes
- NO descriptor changes
- NO schema changes
- All 1,546 tests pass
