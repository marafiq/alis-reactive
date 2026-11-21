# Values and sources

A value a step reads: the one source type every value position takes, each kind of source a declared member is, and
the page's own URL read as a source. Module 2 of `spec/CLAUDE.md`.

## Core questions

- Q2: every value is read by the one read, its path named from a typed lambda, never from text (D2.1, D2.3).
- Q3: the one conversion, `as` with a shape, carries a value held in another form to the position's type by one table
  (D3.3, D3.4), and a second conversion cannot be written (N3.1). Each shape's forms per host, a date's at a live
  member among them (D3.13), are module 15's.
- Q7: the positions and the sources are closed sets (D7.1, D7.2); every position takes every source, typed by
  `where TSource : TValue` (X7.1), and a literal but at a condition operator's left operand, the source it tests (D7.3,
  X7.16); a source whose nullability differs from an argument's or an operand's is refused in every app (N18.14,
  X7.15); a source in the app's form meets a live object as held, converted, mapped or refused (D7.7, gate 1's P8).
- Q10: the page's URL is a typed object, `pipeline.PageUrl<TPage>()` or `PageUrl<TPage>(template)`, whose `Read(q =>
  q.A.B)` reads a placeholder member from the path and any other from the query by its C# path, as the action that
  rendered the page bound it, the member's start when the key is absent or its text does not convert, keys compared
  ignoring case (N10.7).

## Doors

Each name is gate 2's; each door is the capability it carries.

Kept from the core (Q7): a list built from sources, `p.List<T>([first, second])` (N7.2); a text joined from texts and
sources, `pipeline.Concat(first, second, ..)` (N7.7); `IfNull` on a source of a nullable type and `Choose` on a
`TypedSource<bool>` (N7.4); and `OrNull()`, a source read as its nullable type at an argument or in a comparison,
where C# offers no widening overload (N7.5, D7.3).

- `M2.N1` kept: `TypedSource<T>`, the one source type of D7.2, abstract with an internal constructor, so a view reads
  a value only through D7.2's doors. A declared member is a source itself: a property's `ReadOnly<T>` and
  `Property<T>`, a method's `MethodResult<T>`, a payload's or a body's `Data<T>.Read` (N5.6), so no kind of source
  keeps a class of its own. It replaces main's `TypedComponentSource`, `TypedPluginSource`,
  `TypedPluginPropertySource` and `PayloadTypedSource`, and main's constructor, which the surface lists although no
  subclass outside the assembly compiles.

The page's URL needs no door of its own: N10.7's `PageUrl<TPage>()` carries it, typed by the page's own class.

## Improvements

- The page's query keys are compared ignoring case, as both hosts bind them (N10.7; `probe:core-query-key-case`,
  `probe:mvc5-query-key-case`). Main reads a key with `URLSearchParams.get`, which matches case
  (`Alis.Reactive.Assets/runtime/values/evaluate.ts:178-183` at 623dd4a27, `probe:js-query-get-case`), so a page opened
  at `?TAB=medications` reads nothing for `FromUrl("tab")`; the redesign compares ignoring case
  (`Alis.Reactive.Assets/runtime/plugins/document.ts:32-39` at 65fdf118d).
- The page's URL is typed by the page's class and read by a member lambda, never by a string that names the key.
  Main's `FromUrl("tab")` names it as text (`Alis.Reactive/PlanAuthoring/Pipelines/PipelineBuilder.cs:147-159` at
  623dd4a27), which ALIS009 refuses on the public surface.
- One source type. Main keeps a class per kind of source
  (`Alis.Reactive/PlanAuthoring/Conditions/TypedComponentSource.cs:6`,
  `Alis.Reactive/PlanAuthoring/Conditions/TypedPluginSource.cs:11`,
  `Alis.Reactive/PlanAuthoring/Conditions/TypedPluginSource.cs:32`,
  `Alis.Reactive/PlanAuthoring/Conditions/PayloadTypedSource.cs:11`,
  `Alis.Reactive/PlanAuthoring/Conditions/TypedUrlSource.cs:11` at 623dd4a27); the redesign and the POC make a declared
  member a source itself (`Alis.Reactive/Components/JsMembers.cs:17` at 65fdf118d,
  `poc/Alis.Reactive.Poc/Components/Members.cs:9`).
- D7.2 and D9.18 named the redesign's `p.Url<TQuery>()` and `p.Bound`, which N10.7's `PageUrl<TPage>()` and N13.1's
  container read replaced; this unit aligns both lines with those doors.
