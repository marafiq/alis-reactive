# Fusion app-level objects

The page's Syncfusion toast and the confirm that asks through a Syncfusion Dialog: `FusionToast` and `FusionConfirm`,
each one declaration class and a render helper the layout calls once, reached by its fixed id and built on the core's
public surface, as an app's component is (D17.10). Module 23 of `spec/CLAUDE.md`.

## Core questions

- Q11: a user's decision is an awaited step (D11.2). `FusionConfirm.Ask` is a `MethodAsync<bool>`, so its read is
  awaited where it is written, before the branch that reads it (D4.7), and an ask whose run ends is closed through the
  block's signal (D17.11). A toast's time is its own property, `TimeOut`, and no step waits for it (D11.9).
- Q17: the core names neither object (D17.9). Each is the Fusion module's, a declaration and a render helper
  (D17.2, D17.10); the browser code that makes each Syncfusion object is the Fusion package's own component code,
  which reaches the runtime only through `alis:mount` and `alis:unmount`. The confirm's answer is read in a condition
  as any source is (D8.6), by N17.3.
- Q5: each object is found on the element of its fixed id (D5.1) through M12.N2, and a member its declaration names
  is there or the pipeline that reaches it stops (D4.1).
- Q4: a title or a message that reads nothing shows nothing, as a missing value set as an element's text does
  (D4.5).

## Boundaries

`IAppLevelComponent` and the reach with no id, `Component<TComponent>()`, are module 25's with the native app-level
objects (`spec/Declarations/module.md`); this module keeps each declaration's `ElementId` and `DefaultId`, and a view
reaches either object by its id through M12.N2. NativeConfirm and NativeToast, their markup and their defects X17.6 to
X17.9 are module 25's (N17.4). The condition over an awaited `MethodAsync<bool>` is the core's (N17.3), and the
`Confirm` doors it replaces leave the core (D17.9). An app's own confirm, which the redesign lets a page put in the
layout's place as a plugin named `alis.confirm` (`Alis.Reactive.SandboxApp/Scripts/house-confirm.ts:33-37` at
65fdf118d), is a component the app declares with its own `MethodAsync<bool>` (M12.N1, N17.3). A dialog a view shows
and hides itself is the Fusion Dialog slice of module 21. The notice contract of the request defaults, `show(text,
kind)`, is fulfilled by NativeToast or by an app's own object (D10.21); FusionToast fulfils no contract.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M23.N1` kept: `FusionToast`, declared on `FusionComponent` (M21.N2) by the names EJ2's Toast uses
  (`@syncfusion/ej2-notifications/src/toast/toast.d.ts:250`): `Title` and `Content` as `Property<string?>`, each
  shown as text (M23.D1); `TimeOut` as `Property<int>`; `ShowCloseButton` and `ShowProgressBar` as `Property<bool>`;
  `CssClass` as `Property<FusionToastTone>`; `Show()` and `Hide()` (M23.D2); `ElementId`, "alisFusionToast", and
  `DefaultId`. `FusionToastTone` is the closed set `Success`, `Warning`, `Danger` and `Info`, each written as EJ2's
  class, "e-toast-success", "e-toast-warning", "e-toast-danger" and "e-toast-info". The render helper
  `Html.FusionToast()` renders the host element, and the Fusion package's toast code makes the Toast on it (M23.D2).
  The declaration mirrors EJ2's `Toast` of `@syncfusion/ej2-notifications`, so ALIS014 to ALIS019 check it (N17.2);
  EJ2 documents `title` and `content` as null by default, so each is `string?` (D17.5).
  The redesign's `Tone` is EJ2's `cssClass` under another name and is renamed to it, with its `name:` argument gone
  (`Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:51` at 65fdf118d). Main's `FusionToastExtensions`, its
  setters, its four tone doors, `Show`, `Hide` and its render helper are renamed to this door. Each JavaScript name is
  EJ2 33.1.44's public member (`@syncfusion/ej2-notifications/src/toast/toast.d.ts:295`, `:304`, `:322`, `:345`,
  `:351`, `:359`, `:509`, `:538`).
- `M23.N2` kept: `FusionConfirm`, declared on `Component` (M12.N1) with a vendor of its own,
  `ComponentVendor.Define("fusion-confirm", objectPath: "alisConfirm", subscribe: "addEventListener", unsubscribe:
  "removeEventListener")`, since its object is the Fusion package's confirm object on the host, an `EventTarget`, and
  not EJ2's Dialog, which holds no `ask`: `Ask(message)`, a `MethodAsync<bool>` whose message is any source, declared
  with the block's abort signal as its second argument (N17.5), answered as M23.D3 states; `ElementId`,
  "alisConfirmDialog", and `DefaultId`. It mirrors the class `FusionConfirm` that the confirm code exports, whose
  `ask(message: string | null, signal: AbortSignal)` returns `Promise<boolean>`, so ALIS014 to ALIS019 check it as any
  declaration (N17.2).
  The render helpers `Html.FusionConfirmDialog()` and `Html.FusionConfirmDialog(title, ok, cancel)` render the host
  with the app's title and button texts (M23.D4); their form goes to gate 2. EJ2 has no Confirm class, and its
  `DialogUtility.confirm` is a function (`@syncfusion/ej2-popups/src/dialog/dialog.d.ts:869`); both lines make a
  `Dialog` of their own (`Alis.Reactive.Assets/runtime/components/fusion/confirm.ts:90-98` at 623dd4a27,
  `Alis.Reactive.Assets/components/fusion/confirm.ts:75-83` at 65fdf118d). The name stays FusionConfirm: its object is
  the framework's question, not EJ2's Dialog, and FusionDialog is module 21's slice over the Dialog itself. Main's
  class, its id, its `DefaultId` and its constructor are kept here, and main's `FusionConfirmExtensions` and its render
  helper are renamed to this door. Main's `Show`, `Hide` and `SetContent` are refused: they show, hide and write the
  confirm's dialog outside an ask, where no button answers.

The meanings `M23.D1` to `M23.D4` are in `spec/FusionAppLevelObjects/features.json`, each on the feature it gives.

## Improvements

- A toast shows its title and its content as text (M23.D1, M23.X1). Both lines hand EJ2 the string as it is
  (`Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:52`, `:60` at 623dd4a27,
  `Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:36`, `:39` at 65fdf118d), and EJ2 reads a string content
  three ways (`@syncfusion/ej2-notifications/src/toast/toast.js:409`, `:413`, `:425`, `:429`, `:449`): one that a
  page element matches as a selector moves that element into the toast (`probe:ej2-toast-content-selector`), one that
  parses as a selector and matches none is compiled as a template, whose `${...}` threw a TypeError and showed no
  toast (`probe:ej2-toast-content-selector-template`), and any other is written as sanitized HTML, so markup in it is
  live (`probe:ej2-toast-content-markup`, `probe:ej2-toast-title-markup`). Main's setters take a C# literal only; the
  redesign's properties take any source, so a resident's name or a server's message reaches all three. EJ2 documents
  the selector reading (`@syncfusion/ej2-notifications/src/toast/toast.d.ts:290`). The rewrite's toast code answers
  EJ2's `beforeSanitizeHtml` (`@syncfusion/ej2-notifications/src/toast/toast.js:348`, `:350`) with the text inside a
  `<span>`, which no selector parses and no template compiles (`probe:ej2-toast-text-selector`,
  `probe:ej2-toast-text-template`, `probe:ej2-toast-text-markup`), and leaves `enableHtmlSanitizer` at its default,
  true, which that answer needs (`@syncfusion/ej2-notifications/src/toast/toast.js:341`).
- A view's own markup in a toast or a confirm shows as text (M23.D1, M23.D3). On both lines a view's literal with
  markup shows as elements, and no page on either line writes one; the rewrite writes every string as text, the view's
  literals included. A gate 2 item, with that default: a toast or a confirm that shows markup is an app's own
  declaration on M12.N1, as module 22 writes a template's literal text as HTML text and keeps `Raw` as its one door
  for markup.
- A confirm shows its message, its title and its buttons as text (M23.D3, M23.X2). Both lines set the Dialog's content
  to the message string (`Alis.Reactive.Assets/runtime/components/fusion/confirm.ts:58` at 623dd4a27,
  `Alis.Reactive.Assets/components/fusion/confirm.ts:51` at 65fdf118d), which EJ2 sanitizes and writes as HTML
  (`@syncfusion/ej2-popups/src/dialog/dialog.js:852`, `:859`; `probe:ej2-dialog-ask-markup`). The rewrite's confirm code
  hands EJ2 an element whose `textContent` is the text, which EJ2 appends as it is
  (`@syncfusion/ej2-popups/src/dialog/dialog.js:815`, `:1230`; `probe:ej2-dialog-ask-text`). This fix is in the
  confirm code, which writes the Dialog's content itself; the toast's is in its `beforeSanitizeHtml` answer, since a
  view's pipeline writes the Toast's content.
- The confirm's title and button texts are the app's (M23.D4, M23.X3). Both lines write "Confirm", "OK" and "Cancel"
  (`Alis.Reactive.Assets/runtime/components/fusion/confirm.ts:57`, `:62`, `:66` at 623dd4a27,
  `Alis.Reactive.Assets/components/fusion/confirm.ts:50`, `:55`, `:59` at 65fdf118d), so a page in another language
  asks in English.
- A confirm is a declared component and no plugin a view names. The redesign's C# writes a call to the plugin
  `alis.confirm` (`Alis.Reactive/PlanModel/Conditions/Condition.cs:504` at 65fdf118d) and main's runtime reads a
  global, `window.alis.confirm` (`Alis.Reactive.Assets/runtime/conditions/conditions.ts:119-124` at 623dd4a27), so a
  page without Fusion's confirm code fails at the ask (D17.9); the rewrite's `Ask` is found on its element as any
  declared method is (D5.1).
- An ask whose run ends closes (M23.D3). Neither line passes an ask a signal
  (`Alis.Reactive.Assets/components/fusion/confirm.ts:87-89` at 65fdf118d); `Ask` declares the block's signal (N17.5,
  D17.11), and the confirm code closes the open ask on its abort and answers false.
- A toast shows with one call (M23.D2). Main's `Show` calls `dataBind` and then `show`
  (`Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToastExtensions.cs:125-126` at 623dd4a27); the Toast reads its
  properties when it shows, so a toast shown right after a write holds the written content and class
  (`probe:ej2-toast-show-without-databind`), and `Show` writes `show` alone, as the redesign does
  (`Alis.Reactive.Fusion/AppLevel/FusionToast/FusionToast.cs:54` at 65fdf118d).
