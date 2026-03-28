---
name: project_reactive_reader
description: Reactive Reader v3 — collaboration hub with SQLite, 6-agent INVEST review, connected knowledge. Running at localhost:4500.
type: project
---

## Reactive Reader v3 (tools/md-viewer/)

**Current state (v3 — BUILT):** Collaboration hub at http://localhost:4500 (port changed from 4400 which is docs site).

### Architecture
```
Browser → Express (Node.js) → SQLite (reader.db)
                ↓
        Claude CLI (agent dispatch)
```

### Files
| File | Purpose | Lines |
|------|---------|-------|
| `tools/md-viewer/server.mjs` | Express + SQLite + WebSocket + agent dispatch API | ~330 |
| `tools/md-viewer/db.mjs` | Schema (11 tables), seed data, query helpers | ~560 |
| `tools/md-viewer/agents.mjs` | 6 role prompts, context assembly, CLI dispatch | ~220 |
| `tools/md-viewer/invest.mjs` | INVEST validation + status transition logic | ~55 |
| `tools/md-viewer/public/app.js` | SPA client — all views, API-driven | ~750 |
| `tools/md-viewer/public/styles.css` | Design system — warm editorial + review panels | ~1650 |
| `tools/md-viewer/public/index.html` | Shell with sidebar, review panel, verdict bar | ~94 |
| `tools/md-viewer/reader.db` | SQLite database | — |

### SQLite Schema (11 tables)
plans, stories, dependencies, reviews, votes, conflict_summaries, human_verdicts, comments, concepts, concept_links, decision_log, agent_work_log

### Views
1. **Plan view** — master prompt, goals (togglable), constraints, D2 placeholder, story table
2. **Board** — kanban (draft/ready/in-progress/review/done) with INVEST-badged cards
3. **Story detail** — rendered markdown, INVEST badges, dependency pills, concept tags
4. **Review summary** — consensus bar, 6 agent cards, blocker attention section
5. **Review panel** — slide-in with executive summary, tiered findings, artifacts
6. **Knowledge** — concept cards sorted by reference count, drill-through timeline
7. **Verdict bar** — fixed bottom with stats + approve/changes/defer buttons
8. **CRUD** — New Plan form, New Story form, togglable goal checkboxes

### 6 Agent Roles (parallel dispatch via `claude --print`)
1. Architect — SOLID, boundaries, D2 diagrams. Veto power.
2. C# Expert — API correctness, type safety, code signatures.
3. BDD Tester — Testability, acceptance criteria, test cases. Veto power.
4. PM/Collaborator — Scope, value, size estimation, scope tables.
5. UI Expert — MVC + Tailwind + Syncfusion, .cshtml snippets.
6. Human Proxy (Adnan) — Hard rules, quality gate, scales-to-100. Override veto.

### Review Protocol
- Round 1: Independent parallel review (no cross-visibility)
- Round 2: Rebuttal if conflict (agents see objections)
- Round 3: Secret ballot if still conflicted
- Then: human review with approve/changes/defer

### Live Dispatch Tested
- V-004 received real PM review: "AC3 is scope creep — extract to V-004b"
- Evidence-based with file references (Scripts/validation/orchestrator.ts)
- Structured output: verdict, executive, findings (blocker/concern/observation), artifacts

### What's Next (UI Iteration)
- D2 diagram rendering (currently placeholder text)
- Story body editing inline
- File browser tab (currently placeholder)
- Round 2/3 review support
- More robust error handling for agent dispatch timeouts

**Why:** User needs a visual tool optimized for readability — plans, stories, agent reviews with evidence. Connected knowledge prevents losing context as docs evolve.

**How to apply:** The system works end-to-end. Next iterations should focus on UI polish and real-world usage with actual framework stories.
