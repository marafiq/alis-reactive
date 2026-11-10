# Ownership and state

Who owns what the browser holds, and where each piece of browser state lives: the page and a slot, what each holds,
what a plan asks for and what HTML carries, and the one place of each concept. Module 18 of `spec/CLAUDE.md`. The
module is internal and owns no door: no door names an owner, since a developer chooses where a plan renders and
ownership follows (Q12's doors).

## Core questions

- Q12: the owners are closed, the page and a slot, a slot named in the plan by its container id, and the page is
  never released (D12.1); what a plan asks for, its handlers' listeners, its holds, its registrations and containers
  and its runs, belongs to its plan's owner (D12.2); what HTML carries, its components, its slots and its plans,
  belongs to the owner of the innermost slot around it, content the server rendered in a slot's container included
  (D12.3); a release removes exactly what its owner holds (D12.5); a connection is held by every owner whose plans
  name it (D12.6); live validation is one listener per page, so nothing per element is released (D12.7).
- Q13: each concept of browser state lives in exactly one of four places, the page, an owner, a container and a
  request (D13.1): the page holds the inputs and the containers by
  element id, each entry under the owner that contributed it, the connections by URL, the app-level contracts and the
  one live-validation listener; an owner holds its listeners, holds, registrations and runs, and a plan's culture and
  time zone, by its plan; a container holds the inputs inside it and its one summary (D13.6); a request holds what it
  sent until it settles, and places each failure in its summary place and at the input it sent under that key. A
  plan's id keys nothing (D13.2).

## Boundaries

A slot's load and unload, the release's closed order past a throw (D12.4), the step that writes over a slot (D12.9)
and the plans' composition (Q14) are module 9's, with X12.1 to X12.3, X12.8, X12.11, X12.14, X12.15 and X12.17; the
plan id's verdict, main's `ReactivePlan<TModel>.PlanId`, and `FormSummary` stay in M9.F1. The driver's side of a
release, the check that an element still holds the object (D12.5), and `listen` (D12.8, N12.1) are module 12's, with
X12.7, X12.9, X12.10, X12.13 and X12.16. A hub's and a stream's holds are module 10's (X12.5, X12.6). A registration,
its rules and the live check are module 8's (D12.7, D16.15), with X12.4, X13.3 and X13.5. The inputs table's reads by
id and by container, two inputs under one name and the page-wide read the core refuses are module 7's (D13.3, D13.4,
N13.1, N14.4, X13.1, X13.2). A request's state until it settles is its `request` statement's in its run (D10.23,
D11.11), module 6's; a run that its owner's release ends is Q11's, module 17's, with X11.5 and X11.23. What a culture
and a time zone mean is module 15's (D3.11, D3.12); this module says where each lives. A step that writes over a
slot across plans is module 5's (X12.12).

This module holds X13.4, where a failure the server sends shows, and M18.X1, where a plan's culture and time zone live.

## Doors

Each name is gate 2's; each door is the capability it carries.

This module adds no door, and every door it rests on is a core door: the plan as a block of its view (N14.2), a
request's `Into` (N10.7), `listen` (N12.1), a container's read of a member (N13.1), the container's block and its
summary (N14.3) and the view's time zone (N3.5). It owns no door of either line, so it gives no verdict, and its
feature's status is `internal`.

The defect M18.X1 is in `spec/OwnershipAndState/features.json`, on the feature it gives.

## Improvements

- The owners are closed, the page and a slot, and a plan belongs to the innermost slot around its script (D12.1,
  D12.3). Main's owner is a slot's abort controller keyed by the slot id a load names, and the page's one controller
  (`Alis.Reactive.Assets/runtime/lifecycle/applied-plans.ts:18-26`, `Alis.Reactive.Assets/runtime/lifecycle/boot.ts:22`
  at 623dd4a27); a load aborts that slot's controller alone (`applied-plans.ts:73-80`), so a slot inside the content
  it replaces keeps its own. The redesign names its owners page and slot
  (`Alis.Reactive.Assets/runtime/objects/owner.ts:12-18` at 65fdf118d), a slot in the plan by `alis.slot`
  (`Alis.Reactive/PlanModel/Writing/Scope.cs:234` at 65fdf118d), and gives a plan at boot the innermost slot around its
  script (`Alis.Reactive.Assets/runtime/root.ts:58-89` at 65fdf118d), the design this module keeps.
- A plan's id keys nothing (D13.2). Main composes every plan of one id from the boot snapshot and the loaded slots
  (`Alis.Reactive.Assets/runtime/lifecycle/applied-plans.ts:24` and `:88-107` at 623dd4a27), writes a plan's summary
  under an id derived from its plan id (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:128-144` at 623dd4a27) and
  clears the summary of every plan a load or an unload touches (`Alis.Reactive.Assets/runtime/lifecycle/boot.ts:73-75`
  at 623dd4a27), so two renderings of one template share one state. The redesign keeps one page-wide table of inputs
  by element id, each id a list of contributions (`Alis.Reactive.Assets/runtime/plugins/model.ts:42` at 65fdf118d),
  and one capture-phase listener per page (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:206-221` at 65fdf118d).
- A failure the server sends shows at the input the request sent under its key, whoever rendered the input (D13.1,
  X13.4). Main places one at its input only when its own plan's container rules name the member and its own plan holds
  the input (`Alis.Reactive.Assets/runtime/validation/orchestrator.ts:166-173` at 623dd4a27), so a member an editor
  template's plan rendered shows its message in the summary alone. The redesign places it among the inputs inside the
  container by binding name (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:179-187` at 65fdf118d).
- A plan's culture and time zone live with its owner, by its plan (D13.1, M18.X1), so content a slot loads under
  another culture or zone reads its numbers and shows its instants by its own. Neither line carries either in a plan:
  both parse a number with `Number` (`Alis.Reactive.Assets/runtime/grammar/shape.ts:59` at 65fdf118d) and read an
  instant at the browser clock's offset (`Alis.Reactive.Assets/runtime/grammar/shape.ts:110` at 65fdf118d,
  `Alis.Reactive.Assets/runtime/execution/requests/request-wire-format.ts:37` at 623dd4a27).
- A registration leaves with its owner (D12.2, D12.7). The redesign drops a slot's entries from its form at the slot's
  release (`Alis.Reactive.Assets/runtime/plugins/form/form.ts:240-243` at 65fdf118d); main wires live validation per
  input of each plan under the slot's signal (`Alis.Reactive.Assets/runtime/lifecycle/boot.ts:62-67` at 623dd4a27).
