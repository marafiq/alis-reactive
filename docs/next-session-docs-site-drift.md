# Session: docs-site Drift Fixes

Branch: create `fix/docs-site-drift` from `refactor/api-surface-xml-docs`

## Goal

Fix all stale references, wrong API names, and outdated test counts in the docs-site (Starlight at `docs-site/`).

## Known Issues

1. **5 pages reference deleted `IReactivePlan`** — interface was deleted, all references are wrong
2. **3 pages use wrong API names** — NativeHiddenField and others renamed
3. **Test counts 30-54% stale** — docs say lower numbers than actual test suite

## Plan Before Execution

This touches content accuracy, code examples, and potentially sandbox verification. Plan the scope:

1. Grep `docs-site/src/` for `IReactivePlan` — list all 5 files with line numbers
2. Grep for `NativeHiddenField` and other known renames
3. Grep for test count strings (e.g., "310", "345", etc.) — compare against actual `dotnet test` output
4. For each stale reference: determine the correct replacement from current code
5. For code examples: verify against actual sandbox pages in browser

## Layers Touched

- Layer 5 (Documentation)
- May touch Layer 4 (Browser verification of examples)

## Context

- docs-site at `docs-site/` — Starlight + astro-d2
- Previous docs-site audit: `.claude/memory/project_docs_site.md`
- docs/ folder already cleaned (78 → 24 files) — this is about docs-SITE, not docs/ folder
- Session todo: `.claude/memory/session_2026_03_28_todo.md` — Task 7
