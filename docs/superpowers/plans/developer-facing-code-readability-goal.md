# Developer-Facing Code Readability Goal

Date: 2026-06-03

Current branch at creation: `tiny-safe-but-important-refactorings`.

This is a goal/reference file for a future implementation pass. It does not
authorize broad cleanup by itself. The implementation goal is to improve
developer-facing readability for framework developers without changing framework
behavior, test intent, public DSL behavior, generated plan shape, or runtime
execution.

Starting evidence:

- `AGENTS.md`
- `CLAUDE.md`
- `docs/developer-cli.md`
- `docs/handoff-code-readability.md`
- `tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs`
- `tests/Alis.Reactive.PlaywrightTests/Conditions/HttpMixing/WhenTriggerDrivenConditionsMixWithHttp.cs`
- `tests/Alis.Reactive.PlaywrightTests/Patterns/Cascading/WhenParentSelectionFiltersDependentList.cs`
- `tests/Alis.Reactive.PlaywrightTests/Patterns/ReactiveWiring/WhenGuardsControlReactiveFlow.cs`

## Pass Goal

Plan-only pass goal:

```text
Close matrix row: readability planning only -> framework developer cleanup rubric -> runtime behavior unchanged
```

First implementation slice goal:

```text
Close matrix row: Html.On(plan, t => t.CustomEvent<ScorePayload>("set-score", ... p.When(args, x => x.Score).Gte(90).ElseIf(...))) -> condition guard branch proof -> Playwright grade assertions remain browser-visible and unchanged
```

The first implementation commit should target Playwright test readability only.
Keep XML documentation cleanup separate unless the Playwright slice is completed,
verified, and committed.

## Readability Rubric

### What To Keep

Keep comments when they explain one of these developer-facing facts:

- DSL intent that is not obvious from the fluent call alone.
- Browser/runtime boundary behavior, including DOM lookup, network, browser API,
  malformed external JSON, or confirm/user-decision boundaries.
- Syncfusion or other vendor-specific behavior, such as duplicated inputs,
  popup sequencing, formatted display values, or required keyboard gestures.
- `net48` or `net10` compatibility constraints.
- Non-obvious test timing, ordering, or state leakage that a future maintainer
  could accidentally break.
- Invariants that protect public DSL behavior, generated plan shape, or runtime
  execution.
- Public API behavior that should be visible in IntelliSense or generated API
  docs.
- Public DSL XML documentation that follows normal .NET XML doc shape:
  `<summary>`, `<typeparam>`, `<param>`, `<returns>`, and targeted
  `<remarks>` or `<exception>` entries when they communicate contract behavior.
- Editorial value: a retained comment should prevent concrete confusion or drift
  for a framework developer. Visiting a file during this pass is not enough
  reason to preserve, expand, or standardize comments that do not carry that
  weight.

### What To Delete

Delete comments when they only:

- Narrate the next line of code.
- Repeat the method, parameter, locator, or assertion name.
- Act as decorative section banners where the test name already carries the
  behavior.
- Preserve stale vocabulary that no longer matches the DSL graph or domain
  terms.
- Try to compensate for unclear test flow that should instead be fixed through
  better names, tighter ordering, or a follow-up note.
- Provide long examples better suited for docs, sandbox pages, or focused
  examples.
- Strip standard XML documentation elements from public DSL members only because
  the existing wording is repetitive. Rewrite those elements into useful API
  contract language, or defer the API-doc slice.
- Add technically correct XML documentation that only repeats a fluent builder
  return type, parameter name, or obvious method name without improving
  IntelliSense or generated docs.

### When To Replace Comments With Helper Methods

In Playwright tests, helper extraction is the exception. Indirection is a
readability cost unless the helper names a genuinely reusable operation that a
framework developer should not have to re-parse in every test.

Replace comments with helper methods only when the comment describes a repeated
developer action or behavior step and the inline mechanics are already
secondary to the behavior, for example:

- "click active and expect the badge to show"
- "select country and wait for cities"
- "submit first, fix field, submit again"
- "open Syncfusion popup and choose item"

The helper name should describe the reusable behavior, not the mechanics of
Playwright. Prefer names such as `SelectCountryAndWaitForCities`,
`AssertGradeBranch`, `AssertTrialBadgeVisible`, or `ConfirmDialogOk` only when
the helper removes repeated noise without hiding the proof.

Do not extract a helper for a one-off sequence, a straightforward action plus
assertion, or a comment-only cleanup. Do not extract a helper if it would hide
the assertion that makes the test valuable. Keep the test body inline when the
reader benefits from seeing the action and proof in one place.

### When To Stop And Write A Follow-Up Note

Stop and write a follow-up note instead of refactoring when:

- The comment reveals unclear DSL behavior rather than unclear test structure.
- The cleanup would require changing sandbox markup, route behavior, public DSL
  names, generated TS terms, or runtime names.
- A comment points to a flaky browser/vendor timing issue that needs diagnosis,
  not cosmetic cleanup.
- Several tests share the same confusion and need a larger fixture design.
- XML documentation cleanup starts to require API wording decisions outside the
  Playwright slice.

Follow-up notes should name the file, the confusing code, why it matters for
framework developers, and the smallest later slice that could address it.

### Better For Framework Developers

In this repo, "better for framework developers" means a developer can:

- Open a Playwright test and quickly map its body to the DSL behavior being
  protected.
- See the condition/request/component/runtime vocabulary in test names, helper
  names, assertion names, and public API docs.
- Distinguish behavior proof from test mechanics without relying on step-by-step
  comments.
- Preserve real browser and vendor-boundary knowledge where comments are the
  right tool.
- Safely modify DSL, builders, plan domain, runtime, or tests without stale
  names or decorative comments obscuring the behavior.

## Reviewer Simulation

Reviewer:
Mid-level framework developer
Task attempted:
Understand why the guard Playwright test fails and identify the behavior under test.
Files inspected:
`tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs`; `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/Guards/Index.cshtml`; `docs/handoff-code-readability.md`
Concrete confusion:
`WhenGuardsControlExecution.cs` uses many banner comments such as `// -- int (ElseIf grade ladder) --`, `// -- long --`, and `// -- Direct Or syntax --`. The banners repeat or only slightly expand the test method names, while some genuinely important inline comments explain null leaf behavior and confirm-dialog behavior. Because all comments have similar visual weight, it is harder to spot the comments that protect actual DSL or browser-boundary intent.
Why it matters for developers:
A developer debugging a failed guard test needs to scan from DSL source in `Guards/Index.cshtml` to the matching Playwright proof. Decorative banners slow that scan and make meaningful comments less visible.
Recommended action:
For the first commit, remove decorative section banners from `WhenGuardsControlExecution.cs` where the test name already carries the behavior. Keep or rewrite comments that explain null/undefined coercion, branch state that intentionally persists across clicks, and confirm dialog browser behavior.
Keep / rewrite / delete / defer:
delete decorative banners; keep/rewrite null and confirm boundary comments; defer deeper helper extraction if it would change the test structure beyond one small slice.

Reviewer:
C# language/API reviewer
Task attempted:
Evaluate whether public XML docs and names communicate the API contract clearly.
Files inspected:
`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs`; `Alis.Reactive/PlanModel/Values/ValueExpression.cs`; `docs/handoff-code-readability.md`
Concrete confusion:
`PipelineBuilder<TModel>` has useful class-level XML docs explaining declaration order and trigger callbacks, but several member comments mostly restate names, such as `Dispatches a custom browser event by name`, `References a component by explicit ID`, and parameter docs that repeat `eventName` or `pluginName`. `ValueExpression` has internal XML docs that are useful when they explain runtime path behavior, but simple operation summaries such as array count/filter/map risk becoming comment noise if handled mechanically.
Why it matters for developers:
Public XML docs are IntelliSense surface for framework developers. Repetitive wording makes important contract details harder to find, especially around runtime-resolved values, typed payloads, plugins, and component references.
Recommended action:
Do not mix XML cleanup into the first Playwright slice. Later, clean one public API slice at a time, keeping concise .NET XML docs that explain contract differences and rewriting repetitive summaries, `typeparam`, `param`, and `returns` text into useful IntelliSense/API-doc language.
Keep / rewrite / delete / defer:
defer XML cleanup; later keep class-level and contract-specific docs, rewrite ambiguous or repetitive API docs, and defer accidental-public-surface questions unless the slice explicitly covers API visibility.

Reviewer:
Test readability reviewer
Task attempted:
Map the test body to user-visible behavior without relying on step comments.
Files inspected:
`tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs`; `tests/Alis.Reactive.PlaywrightTests/Conditions/HttpMixing/WhenTriggerDrivenConditionsMixWithHttp.cs`; `tests/Alis.Reactive.PlaywrightTests/Patterns/Cascading/WhenParentSelectionFiltersDependentList.cs`; `tests/Alis.Reactive.PlaywrightTests/Patterns/ReactiveWiring/WhenGuardsControlReactiveFlow.cs`
Concrete confusion:
`WhenTriggerDrivenConditionsMixWithHttp.cs` uses section banners and line comments like "HTTP response sets saved name" and "Outer condition evaluates active=true", while the test names already state the scenario. `WhenParentSelectionFiltersDependentList.cs` has comments before direct actions such as "Select US first", but it also has valuable Syncfusion comments about real keyboard gestures and duplicate/popup behavior. `WhenGuardsControlReactiveFlow.cs` has valuable comments explaining duplicated Syncfusion numeric inputs, but phase banners inside lifecycle tests could become helper names if the flow is later simplified.
Why it matters for developers:
Playwright tests are behavior documentation. If comments narrate every action, future maintainers learn to ignore them and may miss the comments that explain vendor timing, browser boundaries, or state leakage.
Recommended action:
Start with one small guard file slice. Prefer deleting decorative comments first. Only extract helpers when repeated steps are already obvious and the helper name can express behavior more clearly than the inline sequence.
Keep / rewrite / delete / defer:
delete narrating and decorative comments in the selected slice; keep Syncfusion/browser timing comments; defer broader helper extraction across HttpMixing, Cascading, and ReactiveWiring.

## First Implementation Slice

Recommended first candidate:

`tests/Alis.Reactive.PlaywrightTests/Conditions/Guards/WhenGuardsControlExecution.cs`

DSL source files to read before editing:

- `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Conditions/Guards/Index.cshtml`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Conditions/Guards/ConditionsShowcaseModel.cs`
- `Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Conditions/GuardsController.cs`

Sync/async lane expectation:

- Most guard rows are sync: custom event payload -> condition graph -> branch
  mutation.
- Confirm rows are async by nature: custom event -> confirm user decision ->
  selected branch mutation.

Code to delete or simplify:

- Decorative section banners in the guard Playwright test where method names
  already describe the behavior.
- Step comments that repeat an immediately following click or assertion.
- Do not delete comments that explain null versus missing payload behavior,
  branch state persistence, or confirm dialog browser boundaries unless they are
  rewritten into clearer helper names.

Behavior proof before commit:

```bash
scripts/playwright.sh --filter "FullyQualifiedName~Conditions.Guards.WhenGuardsControlExecution"
scripts/test.sh --no-e2e
```

Exact commit boundary:

- Commit only the first guard Playwright readability slice.
- Preferred commit message:

```text
test: simplify guard Playwright test readability
```

Do not include XML documentation cleanup, sandbox markup cleanup, runtime
cleanup, generated TS changes, or product refactoring in this commit.

## Full-Repo Adaptation

The goal now applies across all C# projects, Razor/sandbox DSL files, tests,
tools, examples, and TypeScript sources in the repository.

Pass goal for the expanded work:

```text
Close matrix row: readability-only cleanup across C# and TS surfaces -> framework developer navigation and API clarity -> runtime behavior unchanged
```

Allowed changes:

- Delete comments that repeat nearby code, test names, method names, parameter
  names, or obvious assertion mechanics.
- Rewrite comments so they explain the DSL, public API contract, runtime
  boundary, browser timing, Syncfusion/vendor behavior, compatibility constraint,
  or invariant.
- Rename local variables, private helper parameters, and test helper names when
  the new name makes the behavior easier to follow and does not change public
  API, generated plan JSON, routes, selectors, test names, or runtime contract.
- Extract very small private helpers only when repeated mechanics are making
  multiple tests harder to read and the helper name captures a reusable
  behavior, not just a wrapper around obvious Playwright calls.

Disallowed changes:

- Public DSL/API renames.
- Generated TypeScript contract edits unless produced by the normal generator.
- Runtime behavior changes, validation behavior changes, route changes, selector
  changes, test intent changes, or sandbox product refactors.
- Broad style churn, formatter-only commits, or comment deletion that removes
  useful browser/vendor/domain context.

Initial broad inventory on this branch:

| Surface | Comment lines found | Highest-noise examples |
| --- | ---: | --- |
| `Alis.Reactive.Fusion` | 4431 | Fusion template/component XML docs |
| `tests` | 3375 | Playwright section banners and step narration |
| `Alis.Reactive` | 1610 | public builder/domain XML docs |
| `Alis.Reactive.Native` | 1147 | native builder XML docs |
| `Alis.Reactive.SandboxApp` | 816 | sandbox Razor DSL section banners |
| `Alis.Reactive.Assets` TypeScript | 314 | runtime value/validation comments |
| `Alis.Reactive.NativeTagHelpers` | 199 | public XML docs |
| `tools` | 61 | small generator/tool docs |
| `examples` | 21 | example Razor comments |
| `Alis.Reactive.DesignSystem` | 20 | minimal |
| `Alis.Reactive.FluentValidator` | 0 | no current target |

Expanded commit boundaries:

1. Playwright tests: one behavior area per commit, verified with the matching
   `scripts/playwright.sh --filter ...` only when selectors, assertions,
   timing, helper extraction, or test flow changes. Pure comment removal does not
   warrant e2e; `git diff --check` is enough, with a build only if syntax or XML
   documentation could be affected.
2. Public C# XML docs: one API/component family per commit, verified with
   `dotnet build` or `scripts/test.sh --no-e2e` depending on scope. Public DSL
   docs must remain valid .NET XML documentation suitable for IntelliSense and
   generated docs. Keep `typeparam`, `param`, and `returns` entries when they
   explain contract behavior, generated output, security, vendor behavior, or
   non-obvious fluent semantics; omit mechanically repetitive fluent-return
   entries when the signature already communicates the chain.
3. Sandbox Razor DSL comments: one route or feature page per commit, verified
   by the matching build or Playwright route only when the edit can affect
   rendering, selectors, timing, or browser-visible behavior. Pure comment
   removal does not warrant e2e.
4. Runtime TypeScript: one runtime module per commit, verified with
   `npm run typecheck` when TS syntax, names, or comments that can affect
   documentation tooling change. Add `npm test` or `npm run build:all` only for
   executable runtime edits. Add focused Playwright only when browser-visible
   runtime behavior changes.
5. Tools/examples/native/design-system: one project-sized cleanup per commit
   when the project is small, verified with a build covering the project.

Review cycle before every commit:

- Re-run `rg -n "^\\s*//|^\\s*///|/\\*|^\\s*\\*"` on touched files.
- Classify remaining comments as kept because they explain domain/API/runtime
  context, or note a deferred concern.
- For public C# XML docs, confirm standard XML elements remain present and carry
  contract value rather than repeating names.
- Ask the editorial question explicitly: what future confusion or drift does
  this comment prevent? If the answer is only "it is public" or "it is standard",
  rewrite it into API contract language or leave a follow-up note.
- If a public type appears to be implementation surface rather than DSL/API,
  record the file and rationale as a follow-up unless the selected slice is
  explicitly an API-visibility change.
- Run `git diff --check`.
- Confirm `git diff --name-only` matches the intended commit boundary.

## Execution Checklist For The Future Pass

1. Confirm the branch is still `tiny-safe-but-important-refactorings`.
2. Read the DSL source files listed for the selected slice.
3. Inventory comments in the chosen test file.
4. Classify each comment as keep, delete, rewrite, or follow-up. Only classify
   as helper-worthy when repeated mechanics are the actual readability problem.
5. Apply the smallest useful cleanup.
6. Run verification that matches the actual edit: diff hygiene for comment-only
   cleanup, scoped build/typecheck for syntax/XML/TS edits, and focused
   Playwright only for behavior-visible test, Razor, selector, timing, or
   runtime changes.
7. Do not run e2e for pure comment removal.
8. Commit one logical readability slice.
9. Report which comments were kept, removed, rewritten, or deferred.
10. Add deferred concerns to a follow-up note instead of expanding the cleanup.

## Deferred Follow-Up Notes

- `Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs`: this public
  expression converter looks like implementation support for typed Syncfusion
  template builders, not necessarily a DSL surface developers should call
  directly. Do not change visibility in a readability-only pass. A later
  API-visibility slice should verify cross-assembly usage, generated-doc intent,
  and binary/API compatibility before deciding whether it should remain public.
- `tests/Alis.Reactive.PlaywrightTests/Patterns/Cascading/WhenParentSelectionFiltersDependentList.cs`:
  keep the removed rapid country-switch scenario out of this readability pass.
  The note is a real behavior-test design concern, not a reason to keep brittle
  inline test history. Syncfusion keyboard navigation can emit intermediate
  change events while moving through items, which creates racing cascade HTTP
  requests. A later flakiness slice should design behavior-focused coverage for
  "latest selection wins" or request cancellation/staleness handling from the
  DSL/runtime boundary, then prove it with stable Playwright or lower-level
  runtime tests. Do not reintroduce rapid browser interaction as a timing race.
- `tests/Alis.Reactive.Playwright.Extensions`: the component locators are
  revealing repeated browser interaction patterns (`Fill`, `Clear`, `Focus`,
  `Blur`, popup selection, blur-to-commit, wrapper lookup) mixed with
  component-specific Syncfusion DOM quirks. Do not extract helpers opportunistically
  during readability cleanup. A dedicated Playwright-pattern session should
  inventory which behaviors are genuinely reusable for this framework and which
  are current test hacks, then design extensions that would be useful beyond this
  repo without hiding the browser-visible proof.

## Out Of Scope For The First Slice

- Framework behavior changes.
- Test intent changes.
- Public API or XML documentation cleanup.
- DSL graph/domain model changes.
- Runtime executor changes.
- Generated TypeScript contract changes.
- Sandbox product refactoring.
- Broad comment deletion across many files.
