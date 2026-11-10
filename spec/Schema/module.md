# Schema

The plan grammar: its closed node set, how C# writes it and the TypeScript contract generated from it. The schema is
`spec/Core/schema.md`; this module owns the set, its closure, the plan's version and the contract. Module 14 of
`spec/CLAUDE.md`.

## Core questions

- Q19: one C# model writes the grammar, with no lowering pass (D19.1); the set is closed at 51 kinds, `try` leaving
  for a `request`'s layers (D19.2); every field is required and none holds null as a state (D19.3); a closed choice
  with no data is a sealed class, never an enum, so `Operator` and `PathMembers` become such classes (D19.4, N19.1);
  each kind has one handler (D19.5); a handler carries its payload's place, so the vendors map is shared by name
  across plans (D19.6); the form's registration and a container are nodes (D19.7, N19.2); a number shape carries `of`
  and `text` (D19.8); the plan carries its culture, time zone and trace (N19.3); `HttpSend`, `HttpResponse`,
  `FormCheck` and `FormPlace` are typed contracts generated beside the grammar, not kinds (D19.9); the plan's version
  is 7, and a plan of another version is refused at boot with one error (D19.10, X19.3).
- Q14: a plan is a block of its view, whose end writes the plan (D14.7, N14.2); plans compose in the browser without
  merging, each its owner's (D14.2).
- Q17: a listener's arguments and the payload's place in them are each handler's (D17.4), which D19.6 rests on.

## Boundaries

What a node means belongs to the module whose feature writes it: a statement's, an expression's and an operator's
meaning is the module of the door that writes it, the conversion `as` module 2's (N3.1), a shape's forms and the number
text module 15's (D3.1, D3.7), the path step `each` module 15's (Q1), the listening and release kinds module 12's, the
form's registration module 8's, a container's block N14.3's, which module 7's rows carry, the `request` statement and
its contracts module 6's, the time zone kinds module 15's, with dates per host, and the trace kinds module 26's. This
module owns the set and its closure (D19.2, D19.5), its fields' rule (D19.3), the rule that no choice is an enum
(D19.4), the version and its refusal (D19.10), the vendors map shared by name (D19.6, X19.4), and the generated
contract. Boot's order is module 1's (M1.D1, D17.3), which module 17 runs as one pass (D11.12); M14.D1 states only what
a refused plan script is and what it leaves started. A slot's load is module 9's.

## Doors

Each name is gate 2's; each door is the capability it carries.

Kept from the core (Q19): the sealed choice classes (N19.1); the registration, its bound members, its rule tree and
the container (N19.2); the plan's culture, time zone and trace (N19.3). The plan's writer is the plan block's end
(N14.2), whose rows module 7 carries. This module adds no door: every door it needs is a core door.

Refused: main's public `PlanNodeDiscriminator<T>`, its constructor, `Write` and `Read`, the plan writer's inside, which
no view, validator or component author writes or reads; their verdicts and carriers are in
`spec/Schema/features.json`.

The meaning `M14.D1` and the defect `M14.X1` are in `spec/Schema/features.json`, on the feature they give.

## Improvements

- The set is closed and each kind has one handler (D19.2, D19.5). The redesign gives each shape a second switch that
  picks a default by shape (`Alis.Reactive.Assets/runtime/grammar/shape.ts:30-45` at 65fdf118d), and each object
  locator two switches (`Alis.Reactive.Assets/runtime/objects/plan-objects.ts:14-18`,
  `Alis.Reactive.Assets/runtime/objects/plan-objects.ts:27-31` at 65fdf118d).
- The browser checks the plan's version (D19.10, X19.3). Main writes version 3
  (`Alis.Reactive/PlanModel/Document/PlanDocument.cs:16` at 623dd4a27) and its runtime writes it again where it
  merges plans (`Alis.Reactive.Assets/runtime/lifecycle/merge-policy.ts:47` at 623dd4a27) but checks none; the
  redesign writes version 6 (`Alis.Reactive/Grammar/PlanDocument.cs:22` at 65fdf118d) and starts any plan that parses
  (`Alis.Reactive.Assets/runtime/plans/discover.ts:19-24` at 65fdf118d).
- A refused plan script stops no other plan (M14.D1, M14.X1). Both lines throw out of the whole boot on a script that
  does not parse (`Alis.Reactive.Assets/runtime/root.ts:64-70` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/plans/discover.ts:19-24` at 65fdf118d), so one bad script starts no plan of the page.
- The payload's place is the handler's, so two plans agree on a vendor by name (D19.6, X19.4). The redesign carries a
  payload kind on the vendor and merges plans' vendors by it (`Alis.Reactive/Grammar/PlanDocument.cs:43-46` at
  65fdf118d), refusing two that differ under one name (`Alis.Reactive/PlanModel/Writing/Scope.cs:270` at 65fdf118d), so
  one component's events cannot hand their handlers an argument and a detail.
- No public converter (D19.4). Main's `PlanNodeDiscriminator<T>` is public
  (`Alis.Reactive/Serialization/PlanNodeDiscriminator.cs:13` at 623dd4a27), and only its internal node bases name it
  (`Alis.Reactive/PlanModel/Reactions/ReactionGraph.cs:12-13` at 623dd4a27 among them); the redesign made it internal
  (`Alis.Reactive/Grammar/PlanNodeDiscriminator.cs:7` at 65fdf118d).
- The contract is generated, kept from both lines. Each generates `plan.ts` from the C# grammar
  (`Alis.Reactive.Assets/package.json:9` at 623dd4a27 and at 65fdf118d), and the redesign's hook refuses a hand edit
  of it (`.claude/hooks/protect-generated.mjs:15` at 65fdf118d), as the root `CLAUDE.md` states.
