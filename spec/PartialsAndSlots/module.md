# Partials and slots

A view's plan, started in a page, a partial, an editor template or a slot's content, and how the plans of a page
compose in the browser without merging; a slot, the element a plan loads into or empties, and what its load and its
unload release. Module 9 of `spec/CLAUDE.md`.

## Core questions

- Q10: `Into(element)` loads the success body's HTML into the element as a slot, and what the element held unloads
  first (D10.11); the body is of kind HTML (D10.6), and N10.7 writes the load as `Into(elementId)` or
  `Into(elementId, source)`.
- Q11: a run belongs to the owner of the plan that holds its handler, and its owner's release ends it, each `Finally`
  running before the unmount (D11.8).
- Q12: the owners are the page and a slot, a slot named in the plan by its container id (D12.1); what a plan asks for
  and what HTML carries belong to the owner of the innermost slot around it (D12.2, D12.3); a load or an unload
  releases every owner inside the content, innermost first, each in a closed order and past a throw, and the step that
  asked stops with one error when a release threw (D12.4); a slot's content is what its loads put there (D12.9); a
  slot's plan listens to a component the page rendered through the component's element id (D12.8, gate 1's P5).
- Q13: the plan id keys nothing (D13.2).
- Q14: each view's plan is its own, and nothing carries plan state between views on the server (D14.1); plans compose
  in the browser without merging (D14.2); the plan is a block of its view (D14.7, N14.2); a step that writes over a
  slot its own plan loads or unloads is refused where the view renders (R14.4), and across plans it stops in the
  browser (R14.7).
- Q16: a container's live check runs on the inputs a load puts in it, since nothing is wired per element (D12.7,
  D16.15).

## Boundaries

`Into` is the request's door, claimed by module 6 (M6.F2); this module carries its slot meaning (D10.11). The plan's
own types, `ReactivePlan<TModel>` and `PlanExtensions`, are this module's, and they also hold doors other modules own,
each claimed there: `RegisterPlugin<TPlugin>()` and main's two `RegisterPlugin` overloads in M13.F1, carried by N17.11;
`BrowserRules` in M8.F3; `Render<TComponent>` in M12.F2. `Component<TComponent>()`, an app-level object's door, stays
in M9.F1 until module 25 claims it, and `FormSummary` stays, refused and carried by the container's block (N14.3).

What a release removes from a component and its vendor (D12.5: X12.7, X12.13, X12.16) is module 12's (M12.F1, M12.F2),
and what an owner holds module 18's; a hub or a stream a slot holds (X12.5, X12.6) is module 10's; a `listen` on an
object a payload or a method hands over (X12.9, X12.10) belongs with N12.1's listening kinds, module 12's; whether a
slot's own rule holds the page's save (X12.4) is module 8's, since the check that holds a request is its validator's
(D16.2); a step that writes over a slot across plans (X12.12) is module 5's; an input id two live plans hold (X14.7) and
the partial for a member (N14.1) are module 7's.

## Doors

Each name is gate 2's; each door is the capability it carries.

This module adds no door. Its doors are the core's: `Html.ReactivePlan`, kept; the plan as a block of its view, whose
end writes the plan and which replaces `Html.RenderPlan` (N14.2); a request's `Into` (N10.7, D10.11);
`Unload(element)`, kept (D12.9); and `listen` (N12.1).

The meanings `M9.D1` and `M9.D2` are in `spec/PartialsAndSlots/features.json`, on the feature they give.

## Improvements

- Each view's plan is its own, and nothing carries plan state on the server (D14.1). Main starts a partial's plan to be
  merged by plan id, the model's full name (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:57-65` at 623dd4a27), and
  its browser recomposes every plan of one id from the boot snapshot and each loaded slot's plans
  (`Alis.Reactive.Assets/runtime/lifecycle/applied-plans.ts:88-107`,
  `Alis.Reactive.Assets/runtime/lifecycle/merge-policy.ts:8-37` at 623dd4a27). The redesign starts each view's plan
  alone (`Alis.Reactive/Razor/PlanExtensions.cs:46-48` at 65fdf118d) and keeps a per-request record in
  `HttpContext.Items` of ids, summaries, slots and contents set (`Alis.Reactive/Razor/RenderedPage.cs:27-38` at
  65fdf118d), which D14.1 refuses; its checks move to where the page composes its plans in the browser (R14.6, R14.7)
  and to the container's block (N14.3).
- A plan renders once by type (M9.X1). The redesign refuses a second render and a write after the render where the
  view renders (`Alis.Reactive/PlanModel/Document/PlanBuildContext.cs:50-75` at 65fdf118d); main renders what the plan
  holds when `RenderPlan` runs and refuses neither (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:110-125` at
  623dd4a27). The block of N14.2 writes the plan at its end, so neither compiles.
- A load releases what the slot held before it replaces the content (D12.4). Main empties the container and appends the
  new content before it aborts the slot's old listeners
  (`Alis.Reactive.Assets/runtime/execution/partials/inject.ts:30-42` at 623dd4a27); the redesign announces the unmount
  of the old content first (`Alis.Reactive.Assets/runtime/plugins/slot.ts:24-38` at 65fdf118d), the order the rewrite
  keeps.
- A release that meets a throw releases every owner and stops the step that asked for it (D12.4, X12.15, X12.17). The
  redesign logs each disposer's error and goes on as if the release had been clean
  (`Alis.Reactive.Assets/runtime/objects/owner.ts:32-40` at 65fdf118d).
- A loaded plan belongs to the innermost slot around its script (M9.X3). The redesign gives content the server rendered
  inside a slot its slot's owner at boot (`Alis.Reactive.Assets/runtime/root.ts:58-89` at 65fdf118d) and starts every
  plan of loaded content under the slot it loaded into (`Alis.Reactive.Assets/runtime/plugins/slot.ts:30-39` at
  65fdf118d), so a slot inside loaded content releases nothing at its first load or unload.
- A load runs its content's own scripts whatever vendor the page holds (M9.D1, M9.X2). Main runs them through
  Syncfusion's `append`, which evaluates the scripts when its third argument is true and which ej2-base's typings mark
  private, only when the page holds Syncfusion (`Alis.Reactive.Assets/runtime/execution/partials/inject.ts:31-36` at
  623dd4a27), a dependency on one consumer; the
  redesign runs them itself (`Alis.Reactive.Assets/runtime/plugins/slot.ts:79-88` at 65fdf118d).
- A load reads its plans before it releases anything (M9.D2). Both lines parse each plan before they change the slot
  (`Alis.Reactive.Assets/runtime/execution/partials/inject.ts:21-30` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/plugins/slot.ts:28-33` at 65fdf118d); D12.4's order starts at the release, so M9.D2
  states the read before it.
- A slot empties by its own door (`Unload`, D12.9). Main has none: its browser releases a slot's plans when a load
  brings content with no plan (`Alis.Reactive.Assets/runtime/execution/partials/inject.ts:38-42` at 623dd4a27), so a
  view empties a slot by loading empty content from the server.
