# Plan Tests

Root CLAUDE.md is authoritative.

**Status:** these tests do not meet `.claude/rules/tests.md`: no browser runs
the plan, and the goldens assert the plan JSON, which a behavior-keeping refactor changes. They are
feedback while coding; the refusal tests are the closest to the rules, since a refusal's message is
what a developer sees.

This project authors the plan a page carries the way a view does, with Razor left out. `Support/Page.cs`
starts the app's host (`WebApplication.CreateBuilder`, `AddControllersWithViews`, what the app
registered) and hands back the view's own `IHtmlHelper<TModel>` and the plan `Html.ReactivePlan` starts;
the view is named, never run. A test then writes `html.On(plan, ...)`, `html.TextInput(plan, m => m.Name)`
(a brought component and its render helper over `Html.InputField`), `plan.Render<Button>(...)`, and reads
`html.Rendered(plan)`, the JSON `Html.RenderPlan` embeds. The core grants this project no internals. No
component library, no browser, no sandbox: a change to what a page carries is a diff to read here, in
seconds.

## What a test is

One promise of the grammar (`docs/design/grammar.md`), stated in the test's name as the sentence a
developer would say, with the plan it writes kept beside it as a golden under
`Goldens/<Fixture>/<test>.json`. The assertions name the part of the plan that carries the promise;
the golden pins the rest. A refusal test authors the mistake through the typed builders the way a
developer would meet it and asserts the message names the thing and the way out; it has no golden.

- Fixtures are named for the situation (`WhenAPageAuthorsRequests`, `WhenARequestCannotBeSent`),
  tests for the promise. A test that only lists what exists is a checkbox: do not write it.
- Models and components live in `Support/`: one admission page (`Admission`, `AdmissionRules`,
  `Desk`), one page whose members convert (`Vitals`), and the brought components a developer can
  declare wrong (`Brought.cs`). Add a member or a component there when a promise needs one; do not
  grow a model to make a test pass.
- The plain DOM is declared on `ComponentVendor.Native` (`TextInput`, `Button`): a page's elements,
  brought like any component.

## Goldens

Every golden on disk is checked against `plan.schema.json`, the schema generated from the grammar
(`Contract/WhenAPlanIsEmbedded.cs`); a plan the grammar's classes could not have written fails there.

A golden is reviewed, never just regenerated. `ALIS_UPDATE_GOLDENS=1 dotnet test tests/Alis.Reactive.PlanTests`
rewrites every golden; read each diff against `grammar.md` before committing it, and say in the
commit what changed in what a page carries and why.

The runtime runs these goldens: `Alis.Reactive.Assets/runtime/__tests__/authored-plans.test.ts`
reads them by fixture and test name, puts the elements they name on the page, and proves the
behavior with a gesture. A golden a bed test names is a contract between the two projects; rename
both together.

## Run

```bash
dotnet test tests/Alis.Reactive.PlanTests                     # the gate runs it with every non-Playwright test project
ALIS_UPDATE_GOLDENS=1 dotnet test tests/Alis.Reactive.PlanTests  # rewrite the goldens, then review the diff
```
