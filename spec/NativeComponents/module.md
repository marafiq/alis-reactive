# Native components

The plain HTML inputs and the button of Alis.Reactive.Native: `NativeTextBox`, `NativeTextArea`, `NativeCheckBox`,
`NativeDropDown`, `NativeHiddenField`, `NativeRadioGroup`, `NativeCheckList` and `NativeButton`, each one declaration
class and a render helper built on the core's public surface, as an app's component is, and `NativeComponent`, the
base they share. The vendor is the DOM: each declaration mirrors the lib.dom interface of the element it renders and
names each member as that interface does, and each input renders through MVC's own helper under the framework's id.
Module 24 of `spec/CLAUDE.md`.

## Core questions

- Q17: a component, shipped or brought, is one declaration and a render helper (D17.2); the native vendor is the
  element itself, listened to by `addEventListener` and `removeEventListener` (D17.3), and Native gets nothing an app
  does not (D17.1). A declaration mirrors the type its vendor's TypeScript declares, here lib.dom's, so ALIS014 to
  ALIS020 check it (D17.5, N17.2); a listener's payload is its first argument (D17.4), lib.dom's event object, which
  each slice declares as a class nested in its component that mirrors that lib.dom type (M24.D1). The browser code three slices need, the check list's, the radio group's and
  the hidden field's, is the Native package's own component code, which reaches the runtime only through `alis:mount`
  (D17.2, M24.D2 to M24.D4).
- Q14: an input binds one member under MVC's binding name and the framework's id (D14.3), rendered through the field
  (M7.N2), and a pipeline reaches it by its member (N14.4).
- Q9: a hidden input is an input like any other (D9.14), and a request sends a member by the input its bound
  component declares (D9.15, N9.3).
- Q5: a member the declaration names is there on the element, or the pipeline that reaches it stops (D5.1).
- Q7: a Native input's declared value is a source at every value position (D7.2).
- Q18: the check list renders by the member's kind, read from its JSON contract (D18.2), with no reflection of its own.

## Boundaries

`Component`, `ComponentVendor.Native`, `RenderedComponent<TComponent, TModel>` and `pipeline.Component<TComponent>(id)`
are module 12's (M12.N1 to M12.N3); the field, `IInputComponent.Input` and the ids are module 7's (M7.N1 to M7.N3). A
text box's and a button's type are module 16's typed doors (N18.4, D18.5, X18.4), which claim both builders' `Type`.
The native app-level objects, NativeDrawer, NativeLoader, NativeConfirm, NativeToast and the action link with its row
actions, are module 25's (N17.4, N17.12). The framework's native reference component, which proves every conformance
cell in the browser, is step 8's (`spec/Reference`), and this module's proofs cover only what its declarations add:
their names, the shapes the browser hands and the three facts of `spec/probes/dom-native-m24.mjs`, and its own
defects. A Native slice shares no type with another: the check list and the radio group each declare their own item,
and their builders and browser code are their own.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M24.N1` kept: `NativeComponent`, declared on `Component` (M12.N1) with its vendor sealed to
  `ComponentVendor.Native`. Main's `NativeGatherExtensions.Include(m => m.Member)`, which reads the element's `value`
  whatever component the member binds (M24.X1), is renamed to the request's input door (N9.3, N14.4); main's string
  `Vendor` is refused, as module 21 refuses Fusion's (M12.N1).
- `M24.N2` kept: `NativeButton`, mirroring lib.dom's `HTMLButtonElement`: `TextContent` as `Property<string>`, `Focus()`
  and `Click` as `Event<Clicked>`, `Clicked` nested in it and mirroring lib.dom's `PointerEvent` (M24.D1). The render helper `Html.NativeButton(id, text)` returns the builder with
  `CssClass`, its `Type` module 16's, and the button's `Reactive(plan, b => b.Click, pipeline)` (M12.N3). Both lines'
  `NativeButtonClickArgs`, which holds no member, is renamed to `Clicked`; main's events, extensions and Reactive extension are renamed
  to this door, M12.N2 and M12.N3.
- `M24.N3` kept: `NativeCheckBox`, mirroring `HTMLInputElement`: `Checked` as `Property<bool>`, `Focus()` and
  `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, an input whose `Checked`, committed by `Change`, a form sends (M7.N3). The render helpers
  `field.NativeCheckBox()` and `field.NativeCheckBox(build => ..)` over a member of bool render MVC's `CheckBoxFor`.
- `M24.N4` kept: `NativeCheckList`, mirroring `NativeCheckListElement`, the type the Native package's check list code
  declares (M24.D2): `Value` as `Property<string[]>`, `Focus()` and `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, an
  input. The render helpers `field.NativeCheckList()` and `field.NativeCheckList(build => ..)` take `Items(IEnumerable<CheckListItem>)`,
  the three `Option` overloads, `CssClass` and `OptionCssClass`; `CheckListItem` is the check list's own option, and
  main's `Items(IEnumerable<RadioButtonItem>)`, which takes the radio group's type, is renamed to it.
- `M24.N5` kept: `NativeDropDown`, mirroring `HTMLSelectElement`: `Value` as `Property<string>`, `Focus()` and
  `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, an input. The render helpers render MVC's `DropDownListFor` with `Items(IEnumerable<SelectListItem>)`
  of each host's MVC, `Placeholder`, `Enabled` and `CssClass`.
- `M24.N6` kept: `NativeHiddenField`, mirroring `HTMLInputElement`: `Value` as `Property<string>` and
  `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, which its code raises on a write that changes the value (M24.D3), an input. The render helper
  `Html.NativeHiddenField(plan, m => m.Member)` renders MVC's `HiddenFor` through the field and returns the rendered
  component. Main's `HiddenFieldFor` is renamed to it, and main's public `NativeHiddenFieldBuilder` is refused as an
  implementation detail, as the redesign makes it internal.
- `M24.N7` kept: `NativeRadioGroup`, mirroring `HTMLInputElement`, the group's input under the field's id: `Value` as
  `Property<string>`, `Focus()` and `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, which its code raises when a person picks a radio
  (M24.D4),
  an input. The render helpers render a radio per option by MVC's `RadioButtonFor`, with
  `Items(IEnumerable<RadioButtonItem>)`, the three `Option` overloads, `CssClass` and `OptionCssClass`; `RadioButtonItem`
  is the radio group's own option.
- `M24.N8` kept: `NativeTextArea`, mirroring `HTMLTextAreaElement`: `Value` as `Property<string>`, `Focus()` and
  `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, an input. The render helpers render MVC's `TextAreaFor` with `Rows`, `Placeholder` and `CssClass`.
- `M24.N9` kept: `NativeTextBox`, mirroring `HTMLInputElement`: `Value` as `Property<string>`, `Focus()` and
  `Change` as `Event<Changed>`, `Changed` nested in it and mirroring lib.dom's `Event`, an input. The render helpers render MVC's `TextBoxFor` with `Placeholder` and `CssClass`, its `Type`
  module 16's.

On each input slice, main's `ValueMember` is refused (M7.N3); main's events class, its getters and `Instance` are
renamed to the slice's door; main's extensions over `ComponentRef` are renamed to the slice's door and M12.N2; main's
`Reactive` extension on the builder to M12.N3; and main's render helper that returns nothing to the slice's door and
M7.N2. Both lines' change payloads, `NativeTextBoxChangeArgs` and its kin, leave for each component's nested `Changed`,
which mirrors lib.dom's `Event` with `Type`, `IsTrusted` and `TimeStamp`; their `Value` or `Checked` is renamed to the
component's own member, read by N14.4 (M24.D1), and main's payload setters are refused.

The meanings `M24.D1` to `M24.D4` are in `spec/NativeComponents/features.json`, each on the feature it gives.

## Improvements

- A native event's payload is what lib.dom hands (M24.D1). Both lines read a change payload's `Value` or `Checked` off
  the element: main hands a listener the event's `currentTarget` (`Alis.Reactive.Assets/runtime/events/event-native.ts:28-32`
  at 623dd4a27), the redesign the same by `instanceof` (`Alis.Reactive.Assets/runtime/objects/vendor-driver.ts:89-92`
  at 65fdf118d). The rewrite reads a payload at `[0]` (D17.4), where lib.dom hands an `Event` for change and a
  `PointerEvent` for click (`probe:dom-change-listener-event`, `probe:dom-click-listener-event`). Each slice declares
  that payload as a class nested in its component, `Changed` or `Clicked`, mirroring the lib.dom type with the members
  lib.dom types concretely: `Type`, `IsTrusted` and `TimeStamp`, and on a click the pointer, mouse and UI members, never
  `Target`, whose type is `EventTarget | null`. A handler reads the component's value from the component (N14.4), as the
  owner's walk of the native text box built and found (`spec/NOTES.md:515`), and `ComponentInput.Of(value, committedBy:
  Change)` takes `Event<Changed>` as M7.N3 states it. Both lines' change payloads leave for the nested `Changed`, a
  gate 2 item with this default.
- A request sends a member by the input its component declares (M24.X1). Main's shorthand reads every Native input by
  the name `value` (`Alis.Reactive.Native/Extensions/NativeGatherExtensions.cs:46` at 623dd4a27), and a check box
  declares `value` as a second name for `checked`
  (`Alis.Reactive/PlanModel/BrowserObjects/BrowserObject.cs:198-204` at 623dd4a27), so a view that sends a check box's
  member by the shorthand throws where it renders, two paths for one name
  (`Alis.Reactive/PlanModel/BrowserObjects/BrowserObjectContract.cs:343-346` at 623dd4a27); N9.3 sends its `Checked`,
  typed by the member.
- A hidden field's change runs (M24.D3, M24.X2). Main gives the hidden field no browser code, and the DOM raises no
  change on a script's write (`probe:dom-hidden-write-no-change`), so its `Changed` never runs; the redesign's code
  raises it on a write that changes the value (`Alis.Reactive.Assets/components/native/hidden-field.ts:9-16` at
  65fdf118d). It is the one Native input whose script write raises change; a gate 2 item, with that default.
- A radio group's write checks its radio (M24.D4, M24.X3), and a check list's write checks its boxes (M24.D2, M24.X4).
  Main writes the value where no radio or box reads it (`Alis.Reactive.Native/Components/NativeRadioGroup/NativeRadioGroupExtensions.cs:31`,
  `Alis.Reactive.Native/Components/NativeCheckList/NativeCheckListExtensions.cs:34` at 623dd4a27); the redesign's code
  checks them (`Alis.Reactive.Assets/components/native/radio-group.ts:10-18`,
  `Alis.Reactive.Assets/components/native/check-list.ts:9-16` at 65fdf118d).
- A check list sends each checked value once in a form's own submission (M24.D2, M24.X5). Both lines render the
  joined text under the binding name beside a box of that name per option
  (`Alis.Reactive.Native/Components/NativeCheckList/NativeCheckListBuilder.cs:169`, `:180` at 623dd4a27,
  `Alis.Reactive.Native/Components/NativeCheckList/NativeCheckListBuilder.cs:160`, `:171` at 65fdf118d), so the form
  sends both (`probe:dom-check-list-form-data`); the rewrite renders one carrier by the member's kind.
- Views carry no JavaScript. Main writes the check list's and the radio group's code as an inline script into the view
  (`Alis.Reactive.Native/Components/NativeCheckList/NativeCheckListBuilder.cs:200`,
  `Alis.Reactive.Native/Components/NativeRadioGroup/NativeRadioGroupBuilder.cs:196` at 623dd4a27), which a page whose
  content security policy refuses inline script never runs; the redesign's code is the Native package's, run on
  `alis:mount`, which the rewrite keeps.
- Each declaration is checked against the DOM's own types (N17.2). Neither line names the type a Native declaration
  mirrors; the rewrite's declarations mirror lib.dom's interfaces and the check list's declared element, so a member
  lib.dom does not make public fails the build.
- One file per slice, each event's payload and each option its slice's own. Main spreads a slice over seven files
  and its check list takes the radio group's item (`Alis.Reactive.Native/Components/NativeCheckList/NativeCheckListBuilder.cs:45`
  at 623dd4a27); the redesign's slice is its declaration and its builder, with `CheckListItem` its own
  (`Alis.Reactive.Native/Components/NativeCheckList/CheckListItem.cs:12` at 65fdf118d), which the rewrite keeps.
