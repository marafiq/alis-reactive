# Paths and wire

How a value is named and written where it crosses a boundary: the typed path step, a member's four names, and each
shape's form per host, a number under the view's culture, a date and an instant under its time zone, a file as the
file itself, and a date at a live member as a JavaScript `Date`. Module 15 of `spec/CLAUDE.md`.

## Core questions

- Q1: a path is a list of steps, each a member, an index or `each`, written as a string, an integer or
  `{"kind":"each"}`, so no member name is a marker (D1.1, D1.2); a dictionary entry is a member step named as
  System.Text.Json writes the key, and a key it writes no property name for is refused where the view renders
  (D1.4); `each` appears only in a registration's paths (D1.5); the binding name is one C# function (D1.6, D1.7).
- Q2: a member's binding name, body path, data name and element id come from one derivation (D2.3); a body names a
  member as the endpoint's host binds it (D2.4), form and query keys are binding names (D2.5), data names follow the
  app's JSON options (D2.6, D2.9), and a component's member keeps its JavaScript name (D2.7).
- Q3: the shapes are closed, `file` among them (D3.1); a number goes as a JSON number, as text under the view's
  culture in form data, and as invariant text in a query or route (D3.7); a date and an instant are shown and sent as
  `spec/PathsAndWire/DateWire.tla` states, ISO 8601 and RFC 3339 (D3.8, D3.9); a JSON response is System.Text.Json's on both
  hosts (D3.10); culture is the server's (D3.11) and the time zone the app's (D3.12); a date reaches a live member as a
  `Date` (D3.13).

## Boundaries

The one read, `get` and `call`, and the conversion `as` at a value position are module 2's (Q2, Q7, X2.1, X3.1). A
comparison's and a sort's date keys are modules 3, 4 and 8's (D6.12, D16.16). The request's wire, its body formats,
its parts and how each part is sent, is module 6's (Q9): this module states what a value is in each form, and module
6 which form a part takes. An input's binding, `coalesce(as(read, shape), blank)` (N3.3), is module 7's, and an
element id's derivation module 7's (D14.3). The plan's culture and time zone fields are module 14's door (N19.3);
their meaning is this module's (D3.11, D3.12). A server error key shown at its input is module 8's (X1.5). A plugin
handed no member path as text is module 13's (X2.2). Fusion's `AddReactiveFusion`, which sets the naming Syncfusion's
server helpers write with, is module 21's row, under D2.9.

This module owns `BrowserFile`, the C# type of the `file` shape (D3.1, N3.2), since its whole meaning is a value's
form: what the page reads of a file the person chose, and the file itself in form data alone (X3.2). It holds the
defects of D3.13, a date at a live member (X3.10, X3.11, X3.12).

## Doors

Each name is gate 2's; each door is the capability it carries.

Module 15 adds no door of its own: Q1 to Q3 give every capability its door. The derivations are internal on both
lines and in the rewrite: `PathStep` (N1.1), `BindingName` (N1.2), `MemberPath` (N2.1), `WireNames` (N2.2) and
`FileShape` (N3.2). `Alis.Reactive.WireTerms.Path` is on no PublicAPI text of either line, and N1.1 replaces it. The
view's culture (N3.4) and time zone (N3.5) are the two the app sees; N3.5's form goes to gate 2.

- The redesign's `BrowserFile`, a data class with a setter on each member: kept as N10.7's `BrowserFile`, a live
  object that mirrors lib.dom's `File`, whose `Name`, `Size`, `Type` and `LastModified` are read-only sources, since
  the browser alone holds the file. A request sends it only in form data or as the whole body (D9.16, X3.2).

No main-only door is this module's: main's path and naming types are internal, and its public
`IdGenerator.For(Type, string)` is module 7's (M7.N1).

## Improvements

- A path step is typed (D1.1, D1.2, X1.2). The redesign marks each item with the member name `[]`
  (`Alis.Reactive.Assets/runtime/grammar/paths.ts:5` at 65fdf118d), so a member of that name reads as a marker.
- A dictionary key is the key's text, or a refusal that names the key's type (D1.4, X1.4). Both lines read a key
  with `Convert.ToInt32` (`Alis.Reactive/PlanAuthoring/ExpressionPaths/ExpressionPathHelper.cs:234` at 623dd4a27,
  `Alis.Reactive/Members/ExpressionPathHelper.cs:204` at 65fdf118d), so a dictionary keyed by text or a class fails
  where the view renders with a conversion's exception that names no member and no cure.
- A data name follows the app's JSON options (D2.6, D2.9). Main camel-cases every name and reads no
  `JsonPropertyName` (`Alis.Reactive/PlanAuthoring/ExpressionPaths/JsonMemberName.cs:9` at 623dd4a27); the redesign
  reads `JsonPropertyName`, else camel case (`Alis.Reactive/Members/JsonMemberName.cs:11` at 65fdf118d), whatever
  policy the app's options set.
- A body names a member as its host binds it (D2.4, X2.3). The redesign writes the body path by CLR name
  (`Alis.Reactive/Members/ExpressionPathHelper.cs:105` at 65fdf118d) and the data path by JSON name (`:100`), so a
  `[JsonPropertyName]` member a net10 `[FromBody]` endpoint reads binds nothing
  (`probe:core-frombody-ignores-clr-name`).
- A conversion that fails reads nothing (D3.3), so an input's text its member's type cannot hold reads the member's
  start, and its check fails the member with the binder's message (N3.3, D16.7, X3.3). The redesign passes the raw
  value on
  (`Alis.Reactive.Assets/runtime/grammar/shape.ts:18` at 65fdf118d) and binds a blank number as 0 (`:36`); main reads
  a missing number as 0 (`Alis.Reactive.Assets/runtime/shared/shape-convert.ts:161` at 623dd4a27).
- A number reads and shows under the view's culture (D3.11, X3.5, X3.6, X3.8). Both lines parse text with `Number`
  (`Alis.Reactive.Assets/runtime/shared/shape-convert.ts:250` at 623dd4a27,
  `Alis.Reactive.Assets/runtime/grammar/shape.ts:59` at 65fdf118d) and show a number as JavaScript writes it
  (`Alis.Reactive.Assets/runtime/shared/shape-convert.ts:154` at 623dd4a27).
- A `DateTime` is its wall fields with no offset (D3.9, X3.13). Main sends every date with the browser clock's offset
  (`Alis.Reactive.Assets/runtime/execution/requests/request-wire-format.ts:1` and `:37` at 623dd4a27) and shows a
  date as its UTC text (`Alis.Reactive.Assets/runtime/shared/shape-convert.ts:155` at 623dd4a27); the redesign reads
  any text `new Date` parses, a `Z` text as a moment (`Alis.Reactive.Assets/runtime/grammar/shape.ts:93` at
  65fdf118d, `probe:js-date-utc-text-as-moment`).
- An instant takes the view's zone (D3.12, X3.9). The redesign gives an instant the browser clock's offset
  (`Alis.Reactive.Assets/runtime/grammar/shape.ts:110` at 65fdf118d).
- A file is its own shape, refused in JSON and a GET before the page runs (D3.1, X3.2). Main throws when a JSON body's
  list holds one (`Alis.Reactive.Assets/runtime/execution/requests/request-payload-writer.ts:133` at 623dd4a27) and the
  redesign when a GET does (`Alis.Reactive.Assets/runtime/plugins/http.ts:100` at 65fdf118d), each when it is sent.
- A date reaches a live member as a `Date` through a shape that ends (D3.13, X3.12). The redesign shapes a type that
  holds itself as `any` past its first level (`Alis.Reactive/Components/ArgumentShape.cs:14` at 65fdf118d), so a date
  in it reaches the vendor as text.
