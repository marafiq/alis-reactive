# Fusion templates

The typed HTML a Fusion slice hands Syncfusion's string template engine: `FusionTemplate.Create<T>()` with its element
doors, its conditions and its event button, rendered to the text the engine compiles once per component and runs on
each row, card or list item. Module 22 of `spec/CLAUDE.md`.

## Core questions

- Q15: templates are the Fusion module's and stay as they are (D15.2); the core has no template node and depends on
  no consumer (D15.1). The engine's behavior is pinned, and the module's three fixes are a read through the Fusion
  text helper, a read past a holder inside the engine's condition on it, and member names under the app's options
  (D15.3), none of which reaches the core (D15.4). M22.D1 to M22.D3 state each fix exactly, and X15.1 to X15.5 are
  placed here.
- Q8: a template's conditions are the engine's, not the core's (D8.4); a term over a source that reads nothing does
  not hold (D8.5), which M22.D2's guard keeps for a condition's term.
- Q4: data past a missing holder reads nothing (D4.2), which M22.D2 keeps where the engine would throw.
- Q2: a member is named by the app's JSON options (D2.9), which the template takes from the plan (M22.D3).
- Q6: a view value a condition captures is read when the view renders, as D6.7 reads one (M22.D4).
- Q17: the event button raises its event by markup's form (M1.D3), and the handler reads its payload at
  `[0, "detail"]` (D17.4).
- Q18: `Expression.Compile` in the template writer is not listed (D18.3); M22.D4 reads a view value with none, and
  the writer's exception for D6.7's read goes to the owner at gate 2.

## Boundaries

Each slice's door that takes a template stays in module 21: the column, card and item templates a component
declares, and `FusionGridEditTemplates` with the grid (M21.N23: a slice stays whole). This module writes the text such
a door takes. Markup's raise, its attributes, the document plugin that reads them on a click and their refusals are
module 1's (M1.D3, M1.N5), and the button fills them; app events and their handlers are module 1's too (M1.N2,
M1.N3). A plugin for work the DSL does not express is module 13's (N17.11). Conditions in a pipeline are module 8's
(D8.4). The text helper is the Fusion module's browser code, a classic script of the Fusion package's own that the
layout loads after `ej2.min.js` and before the components' scripts, since a component whose starting options hold its
data runs its template there, and on both lines the framework's scripts load after that line
(`Alis.Reactive.SandboxApp/Views/Shared/_Layout.cshtml:61-78` at 65fdf118d,
`Alis.Reactive.SandboxApp/Views/Shared/_Layout.cshtml:56-65` at 623dd4a27). Like an app's code it reaches no runtime
door: the core's runtime holds no template code (D15.1).

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M22.N1` kept: `FusionTemplate.Create<T>()` and `FusionTemplateBuilder<TModel>` with its element doors, `Id`,
  `Class`, `Attr`, `Text` (a read and literal text), `Span`, `Img`, `Div`, `Badge`, `Icon`, `Link` and `Raw` (D15.2),
  each read a typed lambda of the item, written as M22.D1 to M22.D3 state. Main's public `FusionTemplateExpression`,
  the writer, and its three statics are refused as an implementation detail; the redesign makes it internal
  (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:9` at 65fdf118d). `Render()` and the `ToString()`
  override, on both lines, leave for `M22.N4`.
- `M22.N2` kept: `When(condition, then)` and `When(condition, then, else)`, and `FusionConditionalBuilder<TModel>`
  with its element doors `Span`, `Badge`, `Icon`, `Div`, `Img` and `Raw`, which When makes and hands to each branch's
  lambda (D8.4, M22.D4). Main's `ShowIf` is When with no else
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:408-411` at 623dd4a27) and is renamed to it; main's implicit
  public constructor of `FusionConditionalBuilder` is refused. Its `Render()`, which When calls, is an implementation
  detail on both lines and leaves for When.
- `M22.N3` kept: `EventButton(text, raises, member, from)` and `EventButton(text, raises, member, from, css)` on
  `FusionTemplateBuilder<TModel>` and `FusionConditionalBuilder<TModel>`, the redesign's doors
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:240-281` at 65fdf118d): a button that raises a declared
  `AppEvent<TPayload>` with one payload member filled from the item, by markup's form (M22.D5). Main's
  `EventButton(text, eventName, idProperty)` overloads, which name the event by a string and write its dispatch into
  onclick, are renamed to it; main's `Button` and `ButtonFor`, which take the onclick text or a function's name, are
  refused, carried by this door, `M1.N3` and `N17.11`.
- `M22.N4` new: `FusionTemplateBuilder<TModel>.Render(plan)`, which writes the template under the JSON options the
  plan carries on both hosts (M22.D3), as `Html.Raises(plan, appEvent)` takes them (M1.N5). It replaces `Render()` and
  the `ToString()` override, which have no options to name by.

The meanings `M22.D1` to `M22.D5` are in `spec/FusionTemplates/features.json`, each on the feature it gives.

## Improvements

- A read is text (D15.3, X15.1, M22.X3). Both lines write a read raw, `${path}`
  (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:25-29` at 623dd4a27,
  `Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:11-15` at 65fdf118d), which the engine writes into the
  HTML unencoded, so markup in data becomes live elements, and null and a missing member show the words null and
  undefined (`probe:ej2-grid-template-unencoded`, `probe:ej2-grid-template-missing-raw`). Main's builder says so of
  its literal text and attributes, which it emits as supplied
  (`Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:26-27` at 623dd4a27), which the rewrite keeps for the
  view's own text, a gate 2 item with the default that literal text is written as HTML text and `Attr` refuses a name
  that starts with on. The rewrite writes each read through
  the Fusion text helper with the item's prefix written (M22.D1), since the engine passes a bare argument that holds the
  word window or data as a global (`@syncfusion/ej2-base/src/template.js:151-153`).
- A read past a missing holder shows nothing, and the component's other rows render (D15.3, X15.2). Neither line
  guards a holder, and the engine stops the grid at the first row that lacks one
  (`probe:ej2-grid-template-unguarded-holder`); the rewrite guards each holder of a read's path (M22.D2).
- Member names are the app's (D2.9, X15.5). Main names every member in camel case
  (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:71-72` at 623dd4a27) and so does the redesign
  (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:44` at 65fdf118d), whatever the options; `Render(plan)`
  names them from the plan's (M22.N4).
- A condition is written in forms the engine reads back (M22.X1, M22.X2). The engine prefixes each bare word of a
  condition with the item and tests a number with a global regular expression whose state one test leaves for the
  next (`@syncfusion/ej2-base/src/template.js:13`, `@syncfusion/ej2-base/src/template.js:127-131`,
  `@syncfusion/ej2-base/src/template.js:209-211`). Both lines write each constant as it is and each comparison in its
  authored order (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:91-132` at 623dd4a27,
  `Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:63-104` at 65fdf118d), so a decimal, an apostrophe or a
  constant written first can fail the compile and leave the grid with no row, and so can their arithmetic, which both
  lines write as it is (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:146-149` at 623dd4a27): `+` and
  `-` of two members throw a ReferenceError (`probe:ej2-grid-template-arithmetic`). The rewrite writes the member
  first and refuses a constant or arithmetic the engine is not pinned to read back (M22.D4, M22.X4).
- A captured view value is read with no compiled expression (D18.3, M22.D4). Both lines compile it with
  `Expression.Compile` and call it with `DynamicInvoke`
  (`Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:113-119` at 623dd4a27,
  `Alis.Reactive.Fusion/Templates/FusionTemplateExpression.cs:85-91` at 65fdf118d).
- A template's button raises a declared app event and carries no JavaScript (X15.4). Main writes the dispatch into
  onclick with the item's id as bare code (`Alis.Reactive.Fusion/Templates/TemplateElements.cs:46-50` at 623dd4a27),
  so a text id throws a ReferenceError on a click, and its `Button` and `ButtonFor` write the view's own JavaScript
  (`Alis.Reactive.Fusion/Templates/TemplateElements.cs:36-39` at 623dd4a27,
  `Alis.Reactive.Fusion/Templates/FusionTemplateBuilder.cs:280-284` at 623dd4a27). The redesign writes markup's
  attributes with the member raw and named in camel case
  (`Alis.Reactive.Fusion/Templates/TemplateElements.cs:34-68` at 65fdf118d); the rewrite fills them through the text
  helper under the app's names (M22.D5).
