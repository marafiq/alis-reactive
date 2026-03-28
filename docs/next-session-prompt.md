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

Load the `dotnet-xml-docs` skill before any doc work. Run all tests before committing.
