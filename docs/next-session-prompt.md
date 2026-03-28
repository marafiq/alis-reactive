# Next Session Prompt

Copy-paste this into Claude Code:

---

We're on branch `refactor/api-surface-xml-docs`. Run `git log --oneline -10` to see recent work.

Read these before starting:
- `docs/next-session-api-docs.md` — full briefing with file paths and line numbers
- `docs/reviews/api-surface-code-review.md` — 5 blocking constructor issues
- `docs/reviews/dev-experience-review.md` — 7 dev experience gaps
- Memory: `feedback_api_surface_frozen.md` — API surface is frozen, read before any changes

## Tasks (in order)

1. **Fix 5 pre-existing public constructors** — make internal on AllGuard, AnyGuard, ConditionalReaction, SequentialReaction, HttpReaction. Grep call sites first, run all tests after.

2. **Fix 2 grammar issues + 7 dev experience doc gaps** — "an FusionAutoComplete" -> "a FusionAutoComplete" (2 files). Then add missing class-level summaries on HttpRequestBuilder, ResponseBuilder, GatherBuilder and the other 4 gaps from the dev experience review.

3. **Build the API Doc Generator tool** — enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in Core/Native/Fusion csproj files. Create `tools/ApiDocGenerator/` C# console app that reads the 3 XML doc files, filters to public members, groups by namespace/class, and outputs `docs-site/src/content/docs/reference/api-reference.md` in our existing format. Add `npm run build:api-docs`. Then update CLAUDE.md and skills to reference the generated XML as API source of truth.

4. **CLAUDE.md cleanup** — Remove stale references: `IReactivePlan` mentions, old test counts (now 1,744+ non-Playwright, 742 Playwright), stale "Remaining Onboarding" list (many already onboarded). Verify every file path and class name mentioned in CLAUDE.md still exists. Remove any section that duplicates what the XML docs now cover.

5. **Skills cleanup** — Read each skill file at `~/.claude/skills/` and verify code examples match current API (Fusion prefix, `build`/`pipeline` params, `InputBoundField`). Stale skills: `reactive-dsl` (partially updated this session, verify all examples), `http-pipeline` (check parameter names in examples), `conditions-dsl` (verify), `onboard-fusion-component` (partially updated), `validation-rules` (check InputFieldSetup references), `bdd-testing` (check component method names).

6. **Memory cleanup** — Read `MEMORY.md` index. Remove or update stale entries: `project_isp_refactor_plan.md` (IReactivePlan deleted, ISP plan is moot), stale test counts, stale "Remaining Onboarding" list, any reference to `InputFieldSetup`. Update `project_xml_docs_dev_audit.md` with current state (gaps closed this session).

Load the `dotnet-xml-docs` skill before any doc work. Run all tests before committing.
