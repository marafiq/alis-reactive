# Next Session Prompt

We're on branch `refactor/api-surface-xml-docs`. Run `git log --oneline -10` to see recent work.

Read before starting:
- `docs/todo-skill-updates.md` — full skill/hook todo list with priorities
- `docs/cs1591-xml-docs-remaining.md` — 758 XML doc warnings by module
- `docs/reviews/anthropic-claude-md-research.md` — Anthropic best practices research
- `.claude/memory/MEMORY.md` — session memory index

## Tasks (in priority order)

### 1. CLAUDE.md Rules — Positive Language Rewrite
Rules still use "Never", "No", "Don't". Research says positive directives cut violations ~50%.
Rewrite each rule as what TO DO. Example: "No manual JS in views" → "All browser behavior flows
through the reactive plan." Read current CLAUDE.md (227 lines, target under 200).

### 2. Fix `reactive-dsl` Skill (WIP)
Expand scope: merge InputField + component rendering + SSE/SignalR triggers.
Verify all code examples against current API. Keep under 500 lines.
Use `references/` for depth. Run `/verify-skill-top-10-things reactive-dsl` after.

### 3. Fix `onboard-fusion-component` Skill (6 errors)
- `ReactiveWiringHelper.Wire<>()` does not exist — code inlines wiring
- `FusionGatherExtensions` does not exist — actual: `GatherExtensions` in core
- Gather constraint wrong — `IComponent` not `FusionComponent`
- `PreventDefault`/`UpdateData` param type — `ICommandEmitter` not `PipelineBuilder<TModel>`
- `UpdateData` signature — `<TResponse>` not `<TModel, TResponse>`
- File count — 6-8 per component, not fixed 7

### 4. Fix `validation-rules-alis-reactive` Skill (5 gaps from blind test)
- Missing `OnError(400, e => e.ValidationErrors("form"))` pattern
- Missing `.WithMessage()` — every real validator uses custom messages
- No numeric threshold docs — WhenField only supports truthy/eq/neq
- No Gather + Validate relationship
- No component selection guidance by data type

### 5. Build Schema Drift Hook (HIGH PRIORITY)
Descriptors and schema have drifted in the past. Build a hook or build-time script
that validates C# descriptor JSON output against `reactive-plan.schema.json`.
See `docs/todo-skill-updates.md` for options.

### 6. Create Missing Skills
- `design-system` — layout primitives: vstack, hstack, card, grid, heading, text
- `technical-documentation-writing` — docs-site pages, architecture guides

### 7. CS1591 XML Doc Warnings (758 across 81 files)
Module-by-module approach. Start with DesignSystem (276 warnings) — check if types
should be `internal` first. See `docs/cs1591-xml-docs-remaining.md` for full breakdown.

### 8. Run `/verify-skill-top-10-things` on Each Skill
Audit every skill's YAML `description` against Anthropic's top 10 best practices.
Descriptions must say WHAT + WHEN with trigger phrases front-loaded in first 250 chars.

### 9. API Doc Generator Improvements
- Multiline summaries run together in output
- Add `<remarks>` as collapsible sections
- Wire into CI pipeline
