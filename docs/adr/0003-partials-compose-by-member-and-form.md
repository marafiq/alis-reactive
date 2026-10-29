# ADR-0003: A partial view composes with the page by its members and its form, not by a plan id

**Status:** accepted
**Date:** 2026-10-29
**Domain:** plans | partials | validation | gather
**Supersedes:** composition by `planId` (`ResolvePlan`, `ElementPlan`, grammar.md 3.5 step 1, 6.3 "keyed by
its planId", F9 "plans that share a planId")

## Context

A view composes in MVC by two things: field names, which `ViewData.TemplateInfo.HtmlFieldPrefix` makes
(EditorFor, `<partial for>`, `PartialAsync` with a prefix, rows in a loop), and the `<form>` element that
holds the inputs. The framework composed by a third thing, the plan id, which is the view's model type:
`ResolvePlan<T>()` joined the plan of `T`, `alis.model` joined inputs by plan id, the summary id came from
the plan id, and an input's id was scoped by the view's model type.

That broke the composition an MVC developer writes most (a mapped matrix of 86 cases at `2ed37935e`: 38
covered by a browser test, 24 broken by reading the code):

- An editor template (a partial of a member's type under a prefix) rendered `KinCard__NextOfKin_Name`
  while the page's validator looked for `DeskPage__NextOfKin_Name` (`Validation/FormChecks.cs:166-171`),
  so the page's rule failed on a typed value and held the save. The page's `g.Form()` omitted the
  template's inputs and its server errors missed them.
- Repeated cards of one model shared a plan id and merged into one save; a partial typed to a base class
  never joined; a `ResolvePlan` with no root of its model dropped messages no field shows; two slots
  holding one partial shared an input entry; the page's `RuleForEach` was not checked for rows a partial
  rendered.
- Server-rendered content inside a slot belonged to the page forever; `Html.Set("")` on a slot left its
  handlers and rules running; on net48 the copied-ViewData prefix idiom re-prefixed the page's own inputs.

Two independent designs were made (an MVC-native lens and an explicit-minimal lens). This ADR takes the
MVC-native design, which deletes more than it adds, with one change: the runtime never creates the
summary element.

## Decision

Three rules replace composition by plan id.

1. **An input's id follows its member.** The id is the scope of the class that declares the bound member
   (by its base definition) plus `__` plus the full MVC name, prefix included:
   `KinCard__NextOfKin_Name` whether the page derives it (a validator, an accessor, a named gather), the
   editor template renders it, or a slot answer renders it. Both sides reach the same id without knowing
   about each other, across requests. A generic declarer's scope is its namespace and C# name with
   anything outside `[A-Za-z0-9_]` made `_`. A member an interface declares is refused at render; a partial
   typed to a base class works.
2. **A form is an element.** Checks, sends, server-error placement and the summary work by containment, on
   every plan. `g.Form()` sends the mounted inputs inside the form its request validates; `g.Form(formId)`
   sends the inputs inside the element named; a request that sends `Form()` and validates nothing is
   refused naming the fix. `validate(x)` checks the tests written for `x` (one container is one validator,
   one server decision) and first clears every inline message inside `x`. A server message goes beside the
   mounted input with that binding name inside the container, else into the form's summary.
3. **Every plan stands alone under its owner.** `alis.model` holds one page-wide table of mounted inputs by
   id; the plan id keys nothing and names a plan in traces only. There is one plan call,
   `Html.ReactivePlan<TModel>()`; a partial has a model, as before, and starts its own plan with it.

Around them:

- **The summary** of a form is `{formId}_validation_summary`. The framework renders it on the server: where
  the developer places it (the tag helper), else after the script of the first plan of the request that
  validates or shows the form's errors. The runtime never creates it.
- **A per-request record** (`ViewContext.HttpContext.Items`, the same member on both targets) records the
  inputs rendered (element id, view path) and the summaries rendered. It refuses an id rendered twice (a
  field rendered twice with the same component is refused too: two elements with one id). A plan seals when
  it renders: behavior added after it, and a second rendering, are refused. A plan that never renders (a
  partial's `RenderPlan` inside its own `@section`, which Razor never renders) is not detected: detecting it
  needs a guess from render order, which misfired on a helper that renders its plan when its markup is
  written, so the rule is documented instead.
- **The page's model governs every row.** The page's collection-item rules (`RuleForEach`, `ChildRules`,
  `SetValidator` on a list) are written once in the page's plan by item name and apply, when the form
  checks, to every row mounted at that moment, whoever rendered it: the page, a partial in the same
  request, or a row loaded later into a slot. A row partial carries no rules of its own, as MVC's
  unobtrusive validation picks up rows added later. An unmounted item field is not checked (the server sees
  no missing item); an unmounted member field is, as F5 says.
- **A plan whose script sits inside an element some plan loads into** starts under that slot's owner at
  boot, so its first replacement or Unload releases it. Writing a slot's HTML with `Html.Set` in a plan that
  loads into it is refused naming `Unload`. Live-validation wiring follows the element and is released on
  `alis:unmount`.
- **Reading what a partial may not render:** a component read or step still fails at the DOM boundary; the
  typed read of what MVC would bind is `p.Bound(m => m.Member)`, missing when no input renders it.
- **Dynamic rows:** a list is one list partial in a slot; Add and Remove post the list and the server
  renders it again with contiguous indexes. No append, no `.Index`, no JSON holes.
- **net48:** the prefix captured when a plan starts is checked against the prefix an input renders under;
  a change (MVC 5 shares `TemplateInfo` when ViewData is copied) is refused naming `EditorFor` or a fresh
  `TemplateInfo`.

## Consequences

- Deleted: `ResolvePlan`, `ElementPlan`, `ReactivePlanScope` and its subclasses, `RendersValidationSummary`,
  public `IsPartial`, public `PlanId`, the plan script's `id`, `PlanElementId`, `CheckedField.Rendered`,
  `ShowErrorsOn`'s empty forms, the I8 tolerance for a field rendered twice, `alis.model`'s plan-id key,
  `form.ts`'s `summaries` map, the plan-wide meaning of `Form()`, and F9's page-owned slot content.
- Every view that sent `Form()` without validating names its form; every `ResolvePlan` and `ElementPlan`
  view starts `ReactivePlan`; the id of every nested member changes (tests that derive ids through
  `IdGenerator` follow).
- grammar.md 3.5, 6.3 and 6.4 are rewritten, and a developer guide to partial composition is written from
  the proven pages.
- Each rule is proven by framework Playwright tests that fail first, in `Partials/`, `Forms/`, `Slots/`
  and `Rendering/`, plus the verify-net48 smoke page.

## Sequence

- Wave 1, in parallel: ids follow the member and `p.Bound` (rule 1); server-rendered content in a slot
  belongs to the slot; the per-request record and its refusals.
- Wave 2: forms send and show what they hold (rule 2, rule 3's model table, the summary).
- Wave 3: one plan call, the deletions and migrations, item rules over every mounted row, the slot-write
  refusal, dynamic rows, the spec and the guide, net48.
