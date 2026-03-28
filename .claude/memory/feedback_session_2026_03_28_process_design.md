---
name: feedback_session_2026_03_28_process_design
description: Critical session feedback — Claude exhibited the exact patterns it documented. Flat checklists are not enough. Need layered system design, not rushing to edit files.
type: feedback
---

## Session Feedback — 2026-03-28 Process Flows Design

### What Claude Did Wrong This Session

1. **Wrote process-flows.md before doing deep research** — produced a flat checklist document without reading git history. Memory files are summaries after the fact; the git log is the actual crime scene.

2. **Created 6 execution tasks and immediately started** — same "design by coding" pattern (M2) that the git history shows. Should have planned, gotten alignment, THEN executed.

3. **Did not save the master index** — hours of forensic research would be lost if session compacted. User had to point this out.

4. **Did not save user feedback** — user gave multiple rounds of nuanced direction during the session, none was persisted.

5. **Flat list, no system thinking** — listed 32 patterns without analyzing HOW they connect. The C# → Schema → TS → Browser pipeline is the natural flow. Breakdowns happen at layer boundaries.

6. **No way to verify skills were used** — no audit trail exists. User may have been babysitting the whole time.

7. **Task list mimics past behavior** — "update each of 11 flows" is micro-commit streaming (M4). Should batch intelligently.

### What the User Wants

**Two goals, not one:**
- Goal 1: Drive quality, correctness, and value (per-layer harnesses)
- Goal 2: Prevent repeated mistakes via process (cross-layer verification)

**Layered harness concept:**
- Each layer (C#, schema, TS, browser, docs) needs its OWN automated harness
- Collectively, the system needs a harness that catches cross-layer drift
- The process must not become outdated in a day
- The process must not harm Claude's ability to think deeply with its own trained data

**Key principle:** "Correctness over sugar rush" — speed is the root cause of most mistakes.

### How to Apply

- ALWAYS save research and feedback BEFORE executing
- ALWAYS present a system-level analysis before editing
- ALWAYS get user alignment on the DESIGN before writing
- NEVER create flat checklists without analyzing connections between items
- NEVER start execution without a plan the user has seen
- Think in LAYERS (C# → Schema → TS → Browser → Docs), not in flat task types