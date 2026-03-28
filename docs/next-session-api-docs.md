# Next Session: API Doc Generator + Remaining Fixes

## 1. API Doc Generator Tool (Main Work)

Build `tools/ApiDocGenerator/` — C# console app that reads XML doc files and generates
`docs-site/src/content/docs/reference/api-reference.md`.

### Setup
- Add `<GenerateDocumentationFile>true</GenerateDocumentationFile>` to 3 csproj files:
  - `Alis.Reactive/Alis.Reactive.csproj`
  - `Alis.Reactive.Native/Alis.Reactive.Native.csproj`
  - `Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj`
- Create `tools/ApiDocGenerator/ApiDocGenerator.csproj` (console app)
- Add `"build:api-docs"` to package.json

### Tool Requirements
- Read 3 XML files from build output (bin/Debug/net10.0/)
- Filter to `public` members only (skip `internal`)
- Group by: namespace -> class -> members
- Output markdown matching existing docs-site format (frontmatter + code blocks)
- Resolve `cref` links to markdown anchors
- Reference the XML in CLAUDE.md and skills for relevant portions

### Current api-reference.md
At `docs-site/src/content/docs/reference/api-reference.md` — hand-maintained, currently
up to date after this session's work but will drift again without automation.

## 2. Pre-Existing Constructor Visibility Issues (5 Files)

Found by code review agent (`docs/reviews/api-surface-code-review.md`). These are
public constructors with "NEVER make public" docs — should be made internal.

| File | Line | Issue |
|------|------|-------|
| `Alis.Reactive/Descriptors/Guards/AllGuard.cs` | 30 | public ctor, doc says "NEVER make public" |
| `Alis.Reactive/Descriptors/Guards/AnyGuard.cs` | 30 | public ctor, doc says "NEVER make public" |
| `Alis.Reactive/Descriptors/Reactions/ConditionalReaction.cs` | 44 | public ctor, doc says "NEVER make public" |
| `Alis.Reactive/Descriptors/Reactions/SequentialReaction.cs` | 14 | public ctor, no doc |
| `Alis.Reactive/Descriptors/Reactions/HttpReaction.cs` | 18 | public ctor, no doc |

**Note:** These are descriptor types, not event args. Making them `internal` should be safe
since they're constructed by builders only. Verify with grep for `new AllGuard(` etc.

## 3. Grammar Fixes (2 Files)

| File | Line | Fix |
|------|------|-----|
| `Alis.Reactive.Fusion/Components/FusionAutoComplete/FusionAutoCompleteReactiveExtensions.cs` | 32 | "an FusionAutoComplete" -> "a FusionAutoComplete" |
| `Alis.Reactive.Fusion/Components/FusionInputMask/FusionInputMaskReactiveExtensions.cs` | 29 | "an FusionInputMask" -> "a FusionInputMask" |

## 4. Dev Experience Gaps (7 Items)

Found by dev experience agent (`docs/reviews/dev-experience-review.md`). Rating: 7/10.

| Gap | Fix |
|-----|-----|
| No "hello world" at Html.On entry point | Add remarks with minimal plan example |
| HttpRequestBuilder missing class-level summary | Add summary + usage example |
| ResponseBuilder missing class-level summary | Add summary |
| GatherBuilder uses unexplained "gather" jargon | Add remarks explaining the concept |
| HTTP convenience verbs undocumented | Add summary to Get/Post/Put/Delete |
| Component<T>() lacks component type guidance | Add remarks listing available types |
| Into()/ValidationErrors() relationship implicit | Add see-also cross-references |

## 5. Skills and CLAUDE.md XML Reference

Once the XML files are generated, update:
- CLAUDE.md: reference the XML files as authoritative API source
- reactive-dsl skill: point to TriggerBuilder/PipelineBuilder XML sections
- http-pipeline skill: point to HttpRequestBuilder/ResponseBuilder XML sections
- conditions-dsl skill: point to GuardBuilder/BranchBuilder XML sections
- onboard-fusion-component skill: point to component XML pattern

## Branch State

Branch: `refactor/api-surface-xml-docs` (pushed, up to date with origin)
All tests pass: 1,744 non-Playwright + 740/742 Playwright (2 flaky, pass on re-run)
Hook active: `.claude/hookify.protect-api-surface.local.md` blocks API surface changes
