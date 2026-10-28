# Framework test components

The framework is proven through the components it was built to drive, and only through its public
extension point. Five small components, written in TypeScript with no library, and a plain button are
brought into a
test app the way any app brings one (`docs/design/bring-your-own-component.md`): a declaration class
of members by their JavaScript names, a render helper, and code that constructs on `alis:mount`.
Their only purpose is to carry every capability a JavaScript object can offer the framework, so a
framework test exercises the framework and nothing else. Fusion, Native and an app's own components
are consumers with test projects of their own; none of them appears on a framework test's page.

Where they live: `tests/Alis.Reactive.Framework.App/Components/<Name>/` (declaration, render helper
and TypeScript together). Their tests: `tests/Alis.Reactive.Framework.PlaywrightTests`, one vertical
slice per framework area, mirroring the app's slice folders. A component grows the members a slice needs
when that slice lands, so no member exists without the test that proves it. The rules every test follows:
`.claude/memory/framework-test-principles.md`.

## What the components must be

- **Real behavior, shown in their own markup.** A method changes what the component holds and shows;
  an event is raised by a gesture inside the component (a real button). A test reads the outcome off
  the page. A component whose methods only record that they were called is a spy, and a test on it
  proves the spy.
- **Built on the public surface only.** The declaration uses `Component`, `JsObject`, `ComponentVendor`
  and the member kinds; the TypeScript knows nothing of the framework beyond `alis:mount`,
  `alis:unmount` and `window.__alisPlugins`.
- **The tripwire.** Bringing them in needs no change to the core or the runtime. A change it needs is
  a gap in the framework's promise, fixed in the framework with its own test.

## The components

| Component | How the framework reaches it | What it is |
|---|---|---|
| `Slate` | a method pair (`on`/`off`), the object under `slate` on its element, the listener's argument as the payload | A board that holds one value of every shape the plan converts, a header object, rows, and the events of changing them. Every property, method, object and event kind is on it. |
| `Bell` | callback properties (`bell.onRung = listener`) | A bell that rings and counts its rings. |
| `Meter` | the element itself, `addEventListener`, the DOM event as the payload (`CustomEvent.detail`) | A custom element `<fw-meter>` with a level and a maximum. |
| `Entry` | as `Slate`, declared an input (`IInputComponent`) | A text entry whose value a form gathers and validates, committed when the user leaves it. |
| `Tally` | a plugin registered by name on `window.__alisPlugins`, no element | A page-wide counter with a total, an add method and an event when it passes a mark. |
| `Button` | the element itself, through the core's native vendor (`ComponentVendor.Native`), a click as its event | A plain `<button>`: the gesture that starts most framework pipelines. It needs no TypeScript. |

## The capabilities

One row per capability. Each row is proven by one slice test that shows its outcome and fails when the
capability breaks (rule 5); a row with no passing test is an open gap.

| # | Capability | Member that exercises it | What the user sees |
|---|---|---|---|
| P1 | Read a read-only property | `Slate.Count` | the count the slate shows, copied into the page |
| P2 | Write a write-only property | `Slate.Note` | the note appears on the slate |
| P3 | Write, then read, a read-write property | `Slate.Title` | the slate shows the new title; the page shows the title read back |
| P4 | A value converts to its member's shape when read and written: text, number, yes/no, date, instant, maybe-number, list, record | `Slate.Text`, `Number`, `Flag`, `Day`, `Moment`, `Maybe`, `Tags`, `Card` | each value shown in the form its shape gives it (a number added, a day as the date picked, a list as its items) |
| O1 | A nested object's members | `Slate.Header` (`Object<T>()`): `Header.Caption`, `Header.Rename(..)` | the header's caption changes by a call and by a write, and reads back |
| O2 | An object a method returns, and its members | `Slate.RowAt(index)` (`Object<T>(arg)`): `Row.Label`, `Row.Mark()` | the row at that position is marked, and only it |
| O3 | A list of objects: an item by position, a field of each item | `Slate.Rows` (`Objects<T>`): `Rows.At(2)`, `Rows.Each(r => r.Label)` into `Slate.Summarize(labels)` | the third row's label; every label, joined on the slate |
| O4 | A list of objects a method returns | `Slate.MarkedRows()` (`Objects<T>()` on a method): `.Count` | how many rows are marked when the pipeline runs |
| M1 | A method with no argument | `Slate.Clear()` | the slate is empty |
| M2 | A method with up to four arguments | `Slate.Place(text, number, flag, day)` | the four values on the slate |
| M3 | A record argument converts to the record the method declares (a date inside it arrives as a date) | `Slate.Fill(card)` | the card's fields on the slate, its date as the day picked |
| M4 | A method's result read where any value is read: a condition, a set, a gather, a route value | `Slate.Has(text)`, `Slate.Total()` | the branch taken; the total in the page; the total in the request |
| M5 | An async method awaited, and an async result | `Slate.Save()`, `Slate.Lookup(key)` | "saved" after the slate finishes; the looked-up value |
| M6 | A method that throws or rejects stops its own handler only | `Slate.Fail()`, `Slate.FailLater()` | the next handler's outcome still shows |
| E1 | An event without a payload runs its pipeline | `Slate.Cleared` | the page reacts to the slate being cleared |
| E2 | A payload's members read | `Slate.Changed` (`Event<TArgs>`): `Previous`, `Current` | the old and new values in the page |
| E3 | A payload's member written before the component reads it back: a cancel | `Slate.Changing`: `Cancel`, `Reason` | the slate keeps its value and shows the reason |
| E4 | A payload's method called | `Slate.Changing`: `Clamp()` | the clamped value on the slate |
| E5 | An event of a nested object | `Slate.Header.Renamed` | the page reacts to the header being renamed |
| E6 | A handler stops listening when its owner is released | a `Slate` in a slot, unloaded | a gesture after the unload changes nothing |
| V1 | Listening through a method pair | `Slate` | (every Slate event row) |
| V2 | Listening through callback properties | `Bell.Rung`, `Bell.Ring()`, `Bell.Rings` | the page counts the rings |
| V3 | The element as the object, the DOM event as the payload | `Meter.Level`, `Meter.LevelChanged` | the level the meter shows; the page reacts to its change |
| V4 | The object under a path on its element | `Slate` (`slate`) | (every Slate row) |
| I1 | An input's value gathered into a request under its member name | `Entry` in `Gather(g => g.Form())` | the server's answer, which echoes what it bound |
| I2 | A validation message on the input's own error slot | `Entry` under a validator | the message beside the entry |
| I3 | The commit event clears a corrected message | `Entry`, corrected | the message goes, with no submit |
| I4 | An input in a partial binds under the partial's prefix | `Entry` in a partial for a member | the server's answer shows the prefixed member bound |
| G1 | A plugin's property read, method called, event heard | `Tally.Total`, `Tally.Add(n)`, `Tally.Passed` | the total in the page; the page reacts to the mark |
| L1 | Constructed on `alis:mount` before handlers subscribe, at boot and in injected HTML | `Slate` on the page and in a loaded partial | its first event is heard in both |
| L2 | Destroyed on `alis:unmount` | a `Slate` in a slot, unloaded | the slate's markup is gone, and its listeners with it (E6) |
| B1 | A missing element fails only the handler that names it | a pipeline naming an absent `Slate` | the other handler's outcome shows |

The framework's other promises (triggers, conditions, requests, validation, slots, realtime,
dispatch, the URL) are tested in their own slices with these components as the page's arrange; their
promises come from the DSL's XML docs and `docs/design/grammar.md`, not from this table.
