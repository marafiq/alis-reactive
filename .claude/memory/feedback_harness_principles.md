---
name: feedback_harness_principles
description: Two critical harness principles — unexpected boundary = wrong plan, and one task CAN touch all layers but process guides it across sessions
type: feedback
---

## Harness Principles (2026-03-28)

### Principle 1: Unexpected Boundary = Wrong Plan

If you're touching an area you didn't plan for, the PLAN is wrong or the TASK is wrong (bad INVEST, missing system thinking).

**Action:** STOP. Save learnings. If course correction requires sudden plan changes, go back to the drawing board. Revert commits but NOT without lessons learned.

**Why:** Surprises during execution mean the planning was insufficient. Pushing through a wrong plan creates the patch-fix cascades (M1). Better to lose a few commits than corrupt the architecture.

### Principle 2: One Task Can Touch All Boundaries

A new primitive touches C# → Schema → TS Types → TS Runtime → Browser → Docs. That's one task, all layers. It may complete across multiple sessions. The process acts as HARNESS across sessions — a guiding light that maintains coherence even when the work is split.

**Why:** Atomic doesn't mean "done in one commit." It means "verified end-to-end." The process tracks which layers are verified and which still need work.

### Principle 3: Pattern Match, Don't Predict

It's impossible to predict all task types. But with enough pattern matching in the repo, design a system that gets you to the right answer. When confused, explain the problem step by step to the user — do NOT dump walls of text.

### Principle 4: User Is Final Guider, Not First Resort

Claude should do the deep thinking. Present problems step by step. Ask specific questions. The user guides when Claude is genuinely confused, not when Claude is lazy.

**How to apply:**
- When confused: explain what you know, what you don't know, what you think the options are
- Do NOT dump 500 lines of analysis — present the crux in 3-5 sentences
- Do NOT ask "what should I do?" — propose and ask "is this direction right?"
