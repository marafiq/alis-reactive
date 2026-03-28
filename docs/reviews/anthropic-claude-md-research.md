# Anthropic CLAUDE.md Research — Consolidated Findings

Generated 2026-03-28 from 3 research agents reading Anthropic's official docs and blogs.

## Key Anthropic Sources

- [Memory docs](https://code.claude.com/docs/en/memory) — CLAUDE.md files, `.claude/rules/`, `@import`, size limits
- [Best Practices](https://code.claude.com/docs/en/best-practices) — Include/exclude table, 200-line target, pruning test
- [Context Engineering](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents) — "Right altitude", minimal instruction set
- [How Anthropic Teams Use Claude Code](https://claude.com/blog/how-anthropic-teams-use-claude-code) — Internal patterns
- Anthropic's own `anthropic-cookbook` CLAUDE.md — 66 lines

## The Rules

1. **Under 200 lines per CLAUDE.md** — "Bloated CLAUDE.md files cause Claude to ignore your actual instructions"
2. **The pruning test**: "Would removing this cause Claude to make mistakes? If not, cut it."
3. **Use `.claude/rules/`** for path-scoped instructions that only load when relevant
4. **Use skills** for domain knowledge and workflows (loaded on-demand)
5. **Remove anything Claude can discover from code** — file maps, test listings, API docs
6. **Include only**: commands Claude can't guess, code style rules, architectural decisions, common gotchas

## What Our CLAUDE.md Had Wrong

- 534 lines (2.7x over budget)
- Architecture deep-dives (discoverable from code)
- Test file catalogs (discoverable from directories)
- Runtime file maps (discoverable from code)
- Code examples (discoverable from .cshtml files)
- Component architecture tutorial (128 lines — belongs in skill)
- Playwright workflow (60 lines — belongs in `.claude/rules/`)
- Debugging methodology (belongs in `.claude/rules/`)
- Enumeration tables that duplicate the JSON schema

## Restructuring Applied

Content moved to `.claude/rules/` (path-scoped, loads only when relevant):
- `playwright-workflow.md` — Rule 13 content
- `debugging.md` — Rule 9 content
- `api-surface-frozen.md` — Rule 12 detailed tables

Content already in skills (removed from CLAUDE.md):
- Component architecture details → `onboard-fusion-component` skill
- DSL entry points → `reactive-dsl` skill
- HTTP pipeline details → `http-pipeline` skill

Content removed entirely (discoverable from code):
- TS runtime file map
- Test directory listings
- Code examples
- HTML bootstrap example
- Projects table (discoverable from .slnx)
