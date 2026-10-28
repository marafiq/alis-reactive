# Framework test components

The framework is proven through the components it was built to drive, and only through its public
extension point. Small components, written in TypeScript with no library, and a plain button are
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
| `Bell` | callback properties (`bell.onRung = listener`), the property already holding the bell's own callback | A bell that rings and counts its rings. Destroyed, it takes its callback property away, so a handler assigned to it can no longer be released (the Slots slice). |
| `Meter` | the element itself, `addEventListener`, the DOM event as the payload (`CustomEvent.detail`), or the element when the event has no detail | A custom element `<fw-meter>` with a level and a maximum, which raises `levelChanged` with a detail and then `change` with none. |
| `Entry` | as `Slate`, declared an input (`IInputComponent`) | A text entry whose value a form gathers and validates, committed when the user leaves it. |
| `Tally` | a plugin registered by name on `window.__alisPlugins`, no element | A page-wide counter with a total, a mark to count towards, an add method and a question whether it passed the mark. |
| `Roster` | callback properties with no events, the object under `roster` on its element | A list that shows the names it is given, in the order given: the method a transformed array is passed to (`Roster.Show(names)`, the Arrays slice). |
| `Button` | the element itself, through the core's native vendor (`ComponentVendor.Native`), a click as its event | A plain `<button>`: the gesture that starts most framework pipelines. It needs no TypeScript. |
| `DayPicker` | as `Slate`, the object under `dayPicker` | A few days to pick one from, holding no day (a `DateTime?` that is null) until the user picks one, and a check, a moment later, whether the picked day is free, shown on its own line. |
| `ConfirmDialog` | the app's own `alis.confirm`, registered by name on `window.__alisPlugins`, no element and no declaration | The question a page's `p.Confirm(..)` asks, with a Yes and a No. |
| `FilePicker` | as `Slate`, the object under `filePicker`, declared an input (`IInputComponent`) | A file input holding the files the person chose as the browser's own `File` objects (`BrowserFile`), committed when they choose, and listing them as the browser does (`Listed`, the input's own `FileList`): what a page reads of a file, and the file a form-data request sends (the Requests slice). |
| `Stamp` | none: no plan reaches it; the script its render helper writes after its element builds it, as a vendor's render helper writes the script that builds its widget | A line of text its own script writes: the script a slot runs when it loads HTML (the Slots slice). |
| `Choice<TEnum>` | as `Slate`, the object under `choice`, declared an input committed by its own `changed` event; its render helper writes its markup to the field's writer | A select of an enum's members by name, with a blank option for a member that may hold none: an enum member gathered, checked, compared and set (the Inputs slice). |

## The capabilities

One row per capability. Each row is proven by one slice test that shows its outcome and fails when the
capability breaks (rule 5); a row with no passing test is an open gap.

| # | Capability | Member that exercises it | What the user sees |
|---|---|---|---|
| P1 | Read a read-only property | `Slate.Count` | the count the slate shows, copied into the page |
| P2 | Write a write-only property | `Slate.Note` | the note appears on the slate |
| P3 | Write, then read, a read-write property | `Slate.Title` | the slate shows the new title; the page shows the title read back |
| P4 | A value converts to its member's shape where it is read and where it is written | an `Entry`'s text read as a number, a maybe-number, a date and an instant (with I1); `Slate.Place` and `Slate.Fill` arguments (M2, M3) | dates typed as a person writes them reach the endpoint as dates; a blank maybe-number is posted as nothing |
| P5 | A member of a data value the component holds | `Slate.LastCard` (`ReadOnly<SlateCard>`): `.Get(c => c.Name)` | the name of the card filled last |
| P6 | One field of each item of a list of data | `Slate.Cards` (`ReadOnly<IReadOnlyList<SlateCard>>`): `.Each(c => c.Name)` into `Slate.Summarize` | every card's name, in order, on the slate |
| P7 | A nullable property written from a value that is never missing | `Slate.Limit` (`Property<int?>`) from `Slate.Count` | the slate's limit is its row count |
| P8 | A write-only property written from a value read when the step runs | `Slate.Note` from `Slate.Title` | the note repeats the title |
| P9 | A member read as its declared type when the component keeps another kind of value | `Slate.Limited` (the limit, a number), `Slate.HoldsCards` (the cards, a list), `Slate.HasPlaced` (the day placed last, a date) read as flags; `Slate.PlacedFlags` (a flag) read as a number; `Slate.StartedAt` (a timestamp) read as a day | the branch each flag takes; the count shown; the day the slate shows for a line placed on it |
| O1 | A nested object's members | `Slate.Header` (`Object<T>()`): `Header.Caption`, `Header.Rename(..)` | the header's caption changes by a call and by a write, and reads back |
| O2 | An object a method returns, and its members | `Slate.RowAt(index)` (`Object<T>(arg)`): `Row.Label`, `Row.Mark()` | the row at that position is marked, and only it |
| O3 | A list of objects: an item by position, a field of each item | `Slate.Rows` (`Objects<T>`): `Rows.At(2)`, `Rows.Each(r => r.Label)` into `Slate.Summarize(labels)` | the third row's label; every label, joined on the slate |
| O4 | A list of objects a method returns | `Slate.MarkedRows()` (`Objects<T>()` on a method): `.Count` | how many rows are marked when the pipeline runs |
| M1 | A method with no argument | `Slate.Clear()` | the slate is empty |
| M2 | A method with up to four arguments, each arriving as its declared type | `Slate.Place(text, number, flag, day)` | the four values on the slate, the number formatted and the day read from a real date |
| M3 | A record argument converts to the record the method declares (a date inside it arrives as a date) | `Slate.Fill(card)` | the card's fields on the slate, its date as the day picked |
| M4 | A method's result read where any value is read: a condition, a set, a gather, a route value | `Slate.Has(text)`, `Slate.Total()` | the branch taken; the total as it stands; the total in a request's path and in its body |
| M5 | An async method awaited, and an async result | `Slate.Save()`, `Slate.Lookup(key)` | "saved" after the slate finishes; the looked-up value |
| M6 | A method that throws or rejects stops the rest of its pipeline, and the page goes on | `Slate.Fail()`, `Slate.FailLater()` | the step before it ran, the steps after it did not, and the next gesture works |
| M7 | A field of the page's own class passed by its JavaScript name, with a value of its type | `Slate.SetCard(index, Field.Of((SlateCard c) => c.Name), "Bea")` | the first card is renamed |
| M8 | A field passed alone | `Slate.SortCards(Field.Of((SlateCard c) => c.Name))`, and by `Due` | the cards in name order, then in due order |
| M9 | An argument the declaration always passes | `Slate.AddRowBy(label)` with `Fixed("page")` | the row says it was added by the page |
| M10 | A value passed as the wider type the method declares | `Slate.SummarizeAny(rows.Each(r => r.Label))` with `Pass<IEnumerable<object>, TItems>` | the rows' labels as the summary |
| M11 | A method declared plain whose JavaScript returns a promise runs without the pipeline waiting for it | `Slate.StartSaving()`, the slate's `save` declared with `Method` | the step after it reads the slate still saving |
| E1 | An event without a payload runs its pipeline | `Slate.Cleared`, raised when a method cleared it | the page reacts to the slate being cleared |
| E2 | A payload's members read | `Slate.RowAdded` (`Event<TArgs>`), raised by the slate's own Add row: `Label`, `Count` | the new row's label and the new count in the page |
| E3 | A payload's member written before the component reads it back: a cancel | `Slate.Changing`, raised by the slate's own retitle: `Cancel`, `Reason` | the slate keeps its title and shows the reason |
| E4 | A payload's method called before the component reads the payload | `Slate.Changing`: `Clamp()` | the slate takes the clamped title |
| E5 | An event of a nested object | `Slate.Header.Renamed` | the page reacts to the header being renamed |
| E6 | A handler stops listening when its owner is released | a loaded card's handler of a page event, the card's slot unloaded | the page still hears the bell; the unloaded card does not |
| V1 | Listening through a method pair | `Slate` | (every Slate event row) |
| V2 | Listening through callback properties, beside the component's own callback | `Bell.OnRung`, `Bell.Ring()`, `Bell.Rings` | the page counts the rings, and the bell still shows its own count |
| V3 | The element as the object, the DOM event's detail as the payload, or the element for an event with no detail | `Meter.Level`, `Meter.Max`, `Meter.LevelChanged`, `Meter.Change` | the level the meter shows and reads back; the page shows the change's detail, and the level read off the meter its `change` hands over |
| V4 | The object under a path on its element | `Slate` (`slate`) | (every Slate row) |
| I1 | An input's value gathered into a request under its member name, as its member's type | `Entry` for text, number, maybe-number, date and instant members, in `Gather(g => g.Form())` | the endpoint's answer, which says what it bound |
| I2 | A rule the browser can check holds the request and shows its message on the input's own slot | `Entry` under a validator | the message beside the entry, and no request; a number rule compares numbers, not text |
| I3 | Typing clears a message; leaving an input (its commit) re-checks it | `Entry` after a refused submit | the message goes as the user types; leaving with a value that breaks a rule shows that rule's message, with no submit |
| I4 | An input in a partial binds under the partial's prefix | `Entry` in a card loaded for the desk's `NextOfKin` member | the endpoint binds the name under `NextOfKin` |
| I5 | An input of an enum member: gathered by name, checked by `NotEmpty` and under conditions on the member, committed by the component's own event, compared and set by a pipeline | `Choice<CareLevel>`, `Choice<Shift>` (no member numbered 0) and a `CareLevel?` member | the member the endpoint binds; the message beside the choice and no request; the branch taken; the option shown |
| I6 | The field wrapper by its options: a label, a required marker, none, hidden; and the component a field renders, ready for `.Reactive(..)` | `Entry` with `Label`, `Required`, no options, and `HiddenEntry` with `Hidden`; `Entry.Committed`, `Choice.Changed` | the label naming its input and its star; the message beside a field and, for a hidden one, in the summary; the line the input's own event writes |
| I7 | An input's id is its model's full name, two underscores and its member path, and `IdGenerator.For` gives it | labels the page writes for `Entry` inputs of a nested model type | the endpoint binds what was typed into the input each label names |
| I8 | A member rendered twice on one plan: one input with the same component, refused with another | `Entry` twice; `Entry` then `FilePicker` | the endpoint binds the value; the message the page is refused with |
| I9 | A partial rendered for two members of the page sends, checks and reaches its own inputs, each named by its binding path | a contact card for `Primary` and for `Alternate`, inside a plain form | each card's save binds its own member; the message beside its own field; the name read back from its own input; what a plain form post binds |
| I10 | Files the browser lists in its own `FileList` are posted as files | `FilePicker.Listed` in a form-data request | the endpoint names the file it received |
| G1 | A plugin's property read and written, a method called, a result read | `Tally.Total`, `Tally.Mark`, `Tally.Add(n)`, `Tally.HasPassedTheMark()` | the total in the page; the branch the mark decides. An app's plugin has no events: only the framework's own plugins are subscribed to (`runtime/objects/plugins.ts`, `pluginOf`) |
| L1 | Constructed on `alis:mount` before handlers subscribe, at boot and in injected HTML | `Slate` on the page (every slice) and in a loaded card | the loaded slate's own event is heard by the card's pipeline |
| L2 | An unloaded slot lets go of what it held, and loads fresh again | a card with a `Slate`, unloaded and loaded again | the slot is empty; the reloaded slate starts with its own rows |
| L3 | A script in loaded HTML runs, as a vendor's script that builds its widget does | a `Stamp` in a note loaded into a slot | the stamp shows the text its own script wrote |
| B1 | A step naming an element that is not on the page fails only its own pipeline | a page pipeline writing into the unloaded card | the step before it ran, the step after it did not, and the next gesture works |

The framework's other promises (triggers, conditions, requests, validation, slots, realtime,
dispatch, the URL) are tested in their own slices with these components as the page's arrange; their
promises come from the DSL's XML docs and `docs/design/grammar.md`, not from this table.
