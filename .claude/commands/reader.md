---
description: Continue Reactive Reader v3 development — SQLite backend, INVEST stories, collaboration hub
---

Read memory file `project_reactive_reader.md` for full context on the Reactive Reader.

## Current State

Reactive Reader v1 is at `tools/md-viewer/` — Node.js app with theme-based navigation, inline editing, block comments, hot reload. Running at http://localhost:4400.

Architecture review docs are at `docs/architecture-review/` with existing comment threads.

## What to Build (V3)

### 1. SQLite Backend
Replace inline `<!-- @comment -->` tags and `themes.json` with `tools/md-viewer/reader.db`:
- Plans table (master plans with goals, D2 diagram references)
- Stories table (INVEST-validated, linked to master plan)
- Comments table (threaded conversations, replaces inline HTML comments)
- Statuses table (transitions: open → in-progress → review → done)
- Dependencies table (story blocked-by graph)
- Agent work log table

### 2. Two Work Streams
- **Planning** — master plan → goals → D2 diagrams → INVEST-validated stories
- **Execution** — pick story → dispatch agent → review → iterate → done

### 3. INVEST Gate (Business Logic)
No story starts work until validated:
- **I**ndependent — no unresolved blockers
- **N**egotiable — acceptance criteria define what, not how
- **V**aluable — clear user-facing value statement
- **E**stimable — has size estimate (S/M/L)
- **S**mall — fits one focused session
- **T**estable — concrete acceptance criteria with verification commands

### 4. Master Plan Structure
Goals (not prose), D2 architecture diagram, structured story decomposition. Each story linked to master plan.

### 5. UI Updates
Story board view, INVEST validation indicators, dependency graph, threaded comments from database.

## Approach
Evolve the existing `tools/md-viewer/` — don't rewrite. The viewer at http://localhost:4400 should keep working throughout. Start by designing the SQLite schema, then build incrementally.
