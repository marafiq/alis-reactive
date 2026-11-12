# Elements

An element of the page, found by the id the view gave it: its text, its HTML, its classes, whether it shows, and
its children read as a list. Module 5 of `spec/CLAUDE.md`.

## Core questions

- Q4: an element the page lacks stops the pipeline that reads it (D4.1, schema.md's `element` locator); a missing
  value set as text or HTML shows nothing, and a missing class name changes no class (D4.5).
- Q5: the element is a declaration that mirrors lib.dom's `HTMLElement` member for member (D5.3), and its children
  and classes are live objects that declare the iterator their types have (D5.13).
- Q6: a list an iterator yields is read by `toArray` (D6.17); a lambda over child elements is a delegate over a live
  item, and one over class names an expression over text (D6.8).
- Q7: an element's text, HTML and class name are value positions that take every source (D7.1, D7.6).
- Q12: `Text` and `Html` mark their `set` a content write, refused over a slot (D12.9).
- Q17: the build checks the declaration against lib.dom's types (D17.5).

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M5.N1` new: `Element`, a component declaration that mirrors lib.dom's `HTMLElement`: `Text` and `Html`, each an
  `ElementContent` over `textContent` and `innerHTML`, a source of that text whose `Set` takes a text literal or a
  source of D7.2 of any type, shown as the string table of `spec/Core/conversions.md` writes it (D7.3, D3.11, X3.8);
  `Classes`, the `ClassList` its `classList` holds; `Hidden`, a `Property<bool>` over `hidden`; `Children`, the
  `ElementChildren` its `children` holds; and `Show()` and `Hide()`, which set `Hidden` to false and true, the names
  both lines carry. `ElementContent` carries main's `SetText<TProp>(TypedSource<TProp>)` and the redesign's type of
  the same name, each of which shows a number or a date as an element's text; the POC's declaration holds `Text`,
  `Html` and `Classes` (`poc/Alis.Reactive.Poc/Elements/Element.cs`), each content a `Property<string?>`, which takes a
  source of text alone.
- `M5.N2` new: `ClassList`, mirroring lib.dom's `DOMTokenList`: `Add`, `Remove` and `Toggle`, each taking a literal
  or, as a set does, an `ITypedSource<string?>` (D7.3), and changing no class when it reads nothing (D7.6), and its
  iterator, which yields `string` (N5.7).
  It replaces main's `AddClass`, `RemoveClass` and `ToggleClass`, which take a literal only, and the redesign's
  `ElementClassList` with its `Items`.
- `M5.N3` new: `ElementChildren`, mirroring lib.dom's `HTMLCollection`, whose iterator yields `ChildElement` (N5.7). It
  replaces the redesign's `HtmlElement.Children` and main's `FromDom<TElement>(elementId, "children")`.
- `M5.N4` new: `ChildElement`, mirroring lib.dom's `Element`: `GetAttribute`, a `Method<string?>`, and `HasAttribute`,
  a `Method<bool>`, each taking the attribute's name. The redesign's `ChildElement` has the same two members, read
  through its `[PureMethod]` scan, which leaves (B11).
- `M5.N5` new: `pipeline.Element(id)`, the extension method in Elements that locates the element by the id the view
  gave it, replacing `PipelineBuilder<TModel>.Element` on both lines.

## Improvements

- The element is a declaration the build checks against lib.dom (D5.3, D17.5). Main maps its members as text
  (`Alis.Reactive/Components/Contracts/ComponentMember.cs:123-140` at 623dd4a27), and the redesign reads `children` and
  `classList` with no declaration (`Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:52-53`,
  `Alis.Reactive/PlanAuthoring/Elements/HtmlElement.cs:120-121` at 65fdf118d).
- A text or HTML set from a payload's or a response's field takes the field as a typed source (D7.2). Main's
  `SetText<TPayload>`, `SetHtml<TPayload>` and `SetText<TResponse>` take a path typed `object`
  (`Alis.Reactive/PlanAuthoring/Pipelines/ElementBuilder.cs:61-76`, `Alis.Reactive/PlanAuthoring/Pipelines/ElementBuilder.cs:99-103`
  at 623dd4a27), which erases the value's type and which ALIS009 refuses on the public surface.
- An element's lists are typed by the declaration's iterator (N5.7). Main reads them by a string that names the
  member, `p.FromDom("dom-card", "classList")`
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Native/DomOps/Index.cshtml:12-23` at 623dd4a27).
- A class name read when the step runs is a value position (D7.1). Main's class doors take a literal only
  (`Alis.Reactive/PlanAuthoring/Pipelines/ElementBuilder.cs:31-48` at 623dd4a27).
- A list that reads nothing reads nothing (D6.17). Both lines read it as a list of no item, so a count over it shows
  0, and read a text or a number as a list of one (`Alis.Reactive.Assets/runtime/values/array-op-engine.ts:107-115` at
  623dd4a27, `Alis.Reactive.Assets/runtime/grammar/operators.ts:86-93` at 65fdf118d).
