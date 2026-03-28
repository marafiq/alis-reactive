---
name: session_2026_03_28_todo
description: Persistent todo list from 2026-03-28 process-flows session — tasks need follow-up, each requires a plan first
type: project
---

# Session 2026-03-28 — Persistent Todo List

## Completed This Session
- [x] Forensic git analysis (9 agents, 32 patterns, commit hashes)
- [x] process-flows v1 → v2 (flat checklists → layered harness)
- [x] CLAUDE.md tightened (323 → 160 lines)
- [x] Moved process-flows to `.claude/rules/` (3 files, auto-loaded every session)
- [x] Saved forensic master index + 5 feedback memories
- [x] Expert review (4 reviewers with evidence-based output)
- [x] Removed redundant rules (ESM, Snapshots, Two-Phase Boot, BDD — enforceable elsewhere)

## Open — Each Requires a Plan Before Execution

### Hooks & Enforcement (Layer: Process)

**1. API Surface hook — enhance to catch `internal` → `public`**
- Plan first: read current hookify rule, identify what it catches today, design the `internal` → `public` detection across 3 library projects (Alis.Reactive, Native, Fusion), test with a deliberate violation
- Layers: 1 (C#)
- Input: current `.claude/hookify.protect-api-surface.local.md`
- Output: hook catches `internal` → `public` changes, verified with test

**2. BDD test enforcement — skill in agent prompt + post-hook**
- Plan first: design what the post-hook checks (test file patterns, naming conventions, `page.evaluate()` usage), how to inject `bdd-testing` skill into agent prompts automatically
- Layers: 4 (Browser/Tests)
- Input: `feedback_bdd_constitution`, current test patterns
- Output: hook warns on BDD violations, skill loads automatically for test-writing agents

**3. BDD public API only — analyzer or hook**
- Plan first: decide Roslyn analyzer vs hookify rule. Analyzer catches at compile time (stronger). Hook catches at edit time (faster feedback). 53 existing violations to handle (allow-list? gradual migration?)
- Layers: 1 (C#), 4 (Tests)
- Input: grep of 53 violations, `InternalsVisibleTo` usage
- Output: new violations blocked, existing ones tracked for migration

### Skills (Layer: Process)

**4. Review all skills for accuracy + test effectiveness**
- Plan first: which skills to review (prioritize by usage — reactive-dsl, onboard-fusion-component, bdd-testing, conditions-dsl, http-pipeline), design the effectiveness test (agent WITH skill vs WITHOUT, compare output quality), verify descriptions trigger at the right time
- Known issues: onboard-fusion-component has 6 errors, validation-rules has 5 gaps
- Input: `docs/todo-skill-updates.md`, current skill files
- Output: each skill either verified accurate or corrected, effectiveness tested

### Review Findings (Layer: 5 Docs)

**5. Apply remaining Claude optimization findings**
- Finding 3: Reduce NEVER/MUST emphasis (Claude 4.6 overtriggers)
- Finding 5: Add "why" to critical rules (Anthropic recommends motivation)
- Finding 7: XML tags for truly inviolable rules
- Finding 9: Questions → commands in process-layers.md
- Plan first: read the 3 rules files, identify all instances, batch the changes
- Input: current `.claude/rules/process-*.md` files
- Output: language tightened, verified against Anthropic guidance

### Docs Cleanup (Layer: 5)

**6. docs/ folder cleanup**
- Plan first: verify the 14-delete and 28-archive lists are still accurate (files may have changed since forensic analysis), design archive structure
- Input: forensic docs audit findings
- Output: clean docs/ folder with index

**7. docs-site drift fixes**
- Plan first: list all 5 IReactivePlan references + 3 wrong API names with file:line, verify each against current code, batch the fixes
- Input: docs-site drift reviewer output
- Output: zero stale references, test counts updated

### Code Issues (Layer: varies)

**8. SonarQube quality gate — 3 CRITICALs from CoerceResult**
- Plan first: read issue #54, understand each complexity hotspot, design extraction refactors
- Layers: 3 (TS Runtime)
- Input: issue #54, current `conditions.ts`, `commands.ts`, `rule-engine.ts`
- Output: all 3 under complexity 15, quality gate passes

**9. Vendor isolation leaks — trigger.ts:45, live-clear.ts:44**
- Plan first: design how to move vendor logic into `component.ts` exports without changing behavior
- Layers: 3 (TS Runtime)
- Input: current trigger.ts, live-clear.ts, component.ts
- Output: zero vendor checks outside component.ts, all tests pass

**10. Schema drift detection tool**
- Plan first: decide approach (build-time MSBuild target vs standalone tool vs hook), design what it validates (every descriptor class → serialize sample → validate against schema)
- Layers: 1→2 boundary
- Input: CLAUDE.md TODO, current schema tests
- Output: automated tool runs on build, catches drift before commit

**11. TS-to-schema validation**
- Plan first: research approaches (generate TS types from schema? validate existing types against schema? manual conformance test suite?)
- Layers: 2→3 boundary
- Input: current TS types, schema, the 4 known discrepancies
- Output: automated check that TS types match schema
