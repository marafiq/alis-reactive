# FusionRadioButton public API coverage

The radio offers one option in a native group. Staff can act on a single option and read the selected value from any member of its group. The feature is M21.F36, the component door is M21.N36 and the net10 admission screen places it at `admission-clinical/roomType`.

## Pin and boundaries

The npm lock resolves the EJ2 browser bundle to 33.1.47, Buttons to 33.1.44 and Base to 33.1.45. The net10 and MVC 5 wrappers are both 33.1.47. The operation inventory from TypeScript 6.0.3 resolved 37 root names, 63 root operations and five vendor shapes including the root. `operations.json` and `operations-final.json` in the FusionRadioButtonWave packet retain both host manifest locations for each matched declaration and the complete source graph.

The final inventory finds both net48 and net10 manifest entries for all 19 declared root members, all three ChangeArgs members and both SelectionEvent members. The public enum and both literal values are listed in both manifests under FusionRadioButtonLabelPosition. The internal converter preserves the exact vendor strings and rejects integer enum values.

The table uses R for `@syncfusion/ej2-buttons/src/radio-button/radio-button.d.ts`, RJ for its JS, C for `@syncfusion/ej2-base/src/component.d.ts` and B for `@syncfusion/ej2-base/src/base.d.ts`. CJ and BJ refer to their respective JS files. The exact version of each referenced file is given above. All live methods listed here are synchronous. The radio itself owns no asynchronous lane. D11.3 and D11.4 govern the ordered writes, immediate native call and same-turn event reads. The existing `Run.tla/WritesAfterTheTurnAreRefused` remains in force.

## Root operations

Nineteen names expose 29 concrete radio operations. The two public vendor subscription methods also serve the inherited Fusion listener contract. The remaining 32 root operations have the contract and scope recorded below. A read and a write on the same row have their own evidence or reason.

| Vendor operation | Typed use, actual effect and evidence |
|---|---|
| `addEventListener(eventName: string, handler: Function): void`, B86 | `FusionComponent.FusionVendor` supplies the method in the D17.3 subscription contract. `Rendered.Reactive(r => r.Change, ...)` receives the actual callback in X1402 and X1414. A standalone raw string-and-function listener would bypass the typed event and ownership contract. BJ157–159. |
| `allowServerDataBinding: boolean`, B26, read/write | The default is true. BJ123–134 uses it in the Blazor server-change path. The MVC radio has no such transport. Reading or changing this flag cannot provide a radio booking operation. |
| `appendTo(selector?: string \| HTMLElement): void`, C116 | Zero arguments preserve the current target, a CSS selector picks a target and a supplied element becomes the target. The host render helper owns initial mounting at the developer's chosen id. Retargeting the live instance would detach it from the plan's D17.13 identity. CJ209–232, 264–269. |
| `change: EmitType<ChangeArgs>`, R36 | `Change` receives the native MVC radio selection. X1402 selects either room by label. X1414 checks that native `Click` supplies the receipt and direct `Checked.Set` leaves it unchanged. RJ56–69, 449–470. |
| `checked: boolean`, R49, read/write | Default false. X1400 applies true or false, checks the real radio, and reads `Checked` and the group's value for the assignment review. X1414 distinguishes assigning from an event-producing activation. RJ56–82, 198–200, 285–290. |
| `click(): void`, R187 | `Click()` activates the native input. X1401 attempts it after locking or reopening, X1414 observes the selection it actually raises. RJ472–474. |
| `created: EmitType<Event>`, R42 | The owner excludes EJ2 created across Fusion. The host's ScriptManager creates EJ2's instance before plan subscription. CJ264–269. |
| `cssClass: string`, R56, read/write | Default empty string. X1406 writes the card's classes, selects the visible localized caption and reads the class in a separate print review. The English path replaces an already-applied Arabic class. X1415 exercises the initial builder option. RJ229–231, 302–309. |
| `currentContext: { calls?: Function, args?: any }`, C45, read/write | CJ396–405 stores React template context. Neither direction has a consumer meaning in the MVC radio. The nested optional `calls` and `args` fields share this scope. |
| `dataBind(): void`, C129 | `DataBind()` flushes pending changes now. X1401 attempts a native click immediately after changing the disabled state. The independent bind plant leaves EJ2's eventual automatic flush intact and changes the reservation. BJ111–121, 135–148. |
| `destroy(): void`, R124 | `Destroy()` removes the wrapper, clears the checked state and strips the name, value and disabled attributes from the released input. X1410 asserts the original checked input and actual caption before or after terminal teardown. RJ91–122. |
| `disabled: boolean`, R63, read/write | Default false. X1401 writes both values, reads `Disabled` in the reservation decision, checks the native disabled attribute and exercises the vendor's native click behavior on each side. RJ291–301, 385–387. |
| `ej2StatePersistenceVersion: string`, C20, read/write | CJ166–172, 413–434 reads a per-component version only when EJ2's separate global version-based persistence mode is enabled. The MVC radio stack uses the ordinary component-and-id key. Neither direction configures the missing platform-wide version policy. |
| `element: HTMLInputElement`, C17, read/write | The shallow read is `Element` as `ChildElement`. X1407 checks `HasAttribute("disabled")`, reads the attribute as empty or null, and reads the available input's own value. A replacement setter could orphan the component at the render helper's id. D17.13, RJ351–358. |
| `enableHtmlSanitizer: boolean`, R99, read/write | Default true. X1411 writes and reads this setting and recreates the label with a trusted static print-stock style. The two policies give different visible first-party legends. X1415 applies the setting at construction. RJ388–404, Base sanitize-helper.js6–19, 126–145. |
| `enablePersistence: boolean`, C26, read/write | X1409 writes true, calls `Refresh`, reads the policy, activates the radio and opens the next visit. The false setting on a fresh workstation leaves the referral default. X1413 sets false after true, reads false, then proves the already-attached handler still saves the latest checked choice. The page explains that existing tab behavior when staff change the policy. CJ105–115, 128–140, 181–199, 413–434. RJ162–164 persists checked. |
| `enableRtl: boolean`, C32, read/write | The component inherits the global fallback initially. X1406 writes and reads both directions, and the resident's print instruction uses the current direction with the caption order. X1415 sets direction in the host builder. CJ74–76, RJ225–228, 310–316. |
| `focusIn(): void`, R195 | X1403 moves keyboard focus to the actual radio named by the single or companion recommendation. RJ482–484. |
| `getRootElement(): HTMLElement`, C81 | `GetRootElement()` returns the MVC radio's input. X1408 obtains the method result before reading native attributes for the transfer decision. The JSX hybrid's alternate element belongs to that foreign host. CJ152–159. |
| `getSelectedValue(): string`, R134 | X1022 reads the actual value for either selected option. X1400 also covers no chosen button, which returns the empty string. X1404 follows a live group-name change. X1416 distinguishes a checked, uncoded option (native group value `on`) from an actual inventory code. The same default was independently observed after removing only the selected option's explicit Value from the canonical X1022 page. See default-selection-probe.log and RJ142–155. The second canonical check read `Native [on] live [] attribute absent` on that same selected input (default-contract-probe.log). |
| `htmlAttributes: { [key: string]: string }`, R106–108, read/write | Default empty map. X1405 supplies a value attribute through the host builder only. For a live map, `Property<T>.Set(ITypedSource<T>)` carries a live map intact, including an `id` that changes the native id and label target in RJ406–431. Even a narrowed title record could forward those extra live keys. `Read(map => map["title"])` goes outside `MemberPath.cs:86–101`. Whole-map assignment and arbitrary indexed reads/writes stay outside the typed surface. This unbound radio still depends on its developer-chosen element id. |
| `ignoreCollectionWatch: boolean`, B41, read/write | Default false. This flag governs complex EJ2 collection change tracking. The radio has no decorated collection on which an MVC user could observe that operation. BJ30 and Base notify-property-change.js150–162. |
| `isAngular`, `isReact`, `isVue: boolean`, C51–53, each read/write | Three wrapper-identity flags. The MVC control has no Angular form-control binding, Vue coercion or React render context. Altering the flags would misidentify the host. CJ68–73, RJ56–69, 79–83, 186–197, 363–370. |
| `isDestroyed: boolean`, B22, read/write | BJ252–271 removes the live instance from the element. After `Destroy()`, lookup through the rendered id cannot retrieve that instance to query its terminal flag. Writing the flag could skip actual cleanup. X1410 observes the original input after cleanup. |
| `isServerRendered: boolean`, B25, read/write | Default false at BJ22. The radio's MVC path exposes no independent action on this infrastructure flag. |
| `label: string`, R69, read/write | Default empty. X1406 writes the resident-facing caption and reads it for edition review. X1412 clears an existing caption then restores an alias and checks both the actual caption and the live read. X1411 exercises fresh caption markup through a refresh, X1415 at construction. RJ232–234, 317–319, 388–404. Updating an existing span assigns its textContent. |
| `labelPosition: "After" \| "Before"`, R11, 78, read/write | `LabelPosition` uses `FusionRadioButtonLabelPosition` and its string converter on both targets. Default After. X1406 changes Before to After on an already formatted card, reads the enum back and uses that read in the staff print instruction. X1415 supplies the vendor's builder enum before mount. RJ320–326, 398–404. |
| `locale: string`, C38, read/write | CJ77–79 sets the global culture fallback. The rendered radio takes the authored caption and has no locale-specific resource or onPropertyChanged branch. X1415 supplies the builder setting while the translated text comes from its explicitly configured label. A field snapshot alone would establish no separate localized radio behavior. |
| `name: string`, R85, read/write | Default empty. M21.N36 renders by a developer-chosen id without binding a model input. In X1404, `Name.Set(group.Name)` joins or leaves a group, the native gestures change the actual group selection, and `Name.Eq(primary.Name)` and `GetSelectedValue()` determine the placement. RJ153–155, 187–189, 327–329. |
| `portals: any`, C54, and `root: any`, C18, each read/write | React wrapper portal and root state lack concrete MVC radio contracts. The public mounted DOM root is reached through `GetRootElement()`. CJ152–159, 396–405, 436–443. |
| `refresh(): void`, C74 | `Refresh()` rebuilds from present settings. X1409 distinguishes installing the page-exit handler from an ordinary bind, X1411 exercises caption recreation. CJ128–140, RJ91–122. |
| `removeEventListener(eventName: string, handler: Function): void`, B94 | The inherited Fusion vendor names this method for owner-scoped release under D12.5 and D17.3. The framework-owned callback identity is necessary to unsubscribe. The slice introduces no raw removal door or separate release ordering. BJ167–169. |
| `serverDataBind(newChanges?: { [key: string]: any }): void`, B74–76 | Omitted argument defaults to an empty change object. With a supplied object or no argument, BJ123–134 enters its transport only for Blazor. Its weak nested change map has no MVC radio use. |
| `value: string`, R92, read/write | Default empty. X1405 writes the garden wing and reads the live proposed booking. The server receives the new native value on the ordinary booking and the original native value on a contract booking with a populated render-time value attribute. X1416 keeps a provisional bed's configured Value empty while its actual native selection is `on`. RJ190–197, 330–338, 406–431 and 508–512. |

## Change data and reachable values

The Buttons declaration extends Base's `BaseEventArgs`. The original pinned browser observation, `spec/probes/facts.json` under `ej2-payload-radiobutton-change`, saw an event object, a string name and a string value. RJ56–69 constructs the payload. Base observer.js84–102 supplies the EJ2 event name before notifying listeners. The CLR declaration preserves the three optional members. In the exercised MVC routes all three are present.

| Public path | Declaration, direction and evidence |
|---|---|
| `change.value?: string`, R202 | `ChangeArgs.Value` is a read-only typed source. X1402 distinguishes the real `single` and `companion` values in separate browser worlds. X1416 uses the actual empty string supplied with a selected option that has no configured inventory code, opposite the coded option. EJ2 has already selected the radio when the payload is constructed. It consumes no replacement payload value. |
| `change.name?: string`, Base event-handler.d.ts69 | `ChangeArgs.Name` is read-only. X1402 admits the actual EJ2 `change` category before recording either placement. A different category produces no receipt. Assigning a new name would change the notification object without reclassifying the firing EJ2 event. |
| `change.event?: Event`, R204 | `ChangeArgs.OriginalEvent` maps exactly to the optional `event` property. In MVC, RJ56–69 passes the original browser event for both the user and the desk-initiated native activation. The declaration admits absence. The real MVC handler takes an Event, and no tested activation supplied null. EJ2 never consumes a replacement event reference after dispatch. |
| `change.event.type`, TypeScript lib.dom.d.ts14239 | `SelectionEvent.Type` reads the native string. X1402 requires a native `change`, with a plant expecting `click` instead. The nested type mirrors `lib.dom/Event`. |
| `change.event.isTrusted`, lib.dom.d.ts14207 | `SelectionEvent.IsTrusted` reads the browser-owned boolean. Chromium dispatched a trusted change for both direct label selection and a native `click()` issued by the desk. X1402 and X1414 exercise those origins. Assigning `Checked` emits no event. The false value is outside these real routes. |
| `element` and `getRootElement()` | These independent paths return the actual MVC input. `ChildElement.HasAttribute` and `GetAttribute` cover present-and-empty disabled, absent-and-null disabled, and a populated native value in X1407 and X1408. Each path has its own cases and plants. lib.dom.d.ts13681 and 13755 declare the nullable string and boolean results. |
| `currentContext.{calls,args}`, C45–48 | Optional `Function` and `any`, with read/write access through the React context. The MVC radio never configures or reads a React renderer. |
| `htmlAttributes[key: string]: string`, R106–108 | Whole-map and dynamic indexed read/write variants are scoped by the id-rewriting behavior described in the root table. The real native booking consumes a safe render-time string value in X1405. |
| `serverDataBind(newChanges)[key: string]: any`, B74–76 | The optional argument is a Blazor changes map. The MVC path has no send, acknowledgement or application-level data contract for either direction. |

The owner excludes full DOM method chains at `audit/README.md` lines 50–52. The radio's typed surface reaches the public native selection information and the actual shallow input and method return described above. Constructor, `getPersistData`, `onPropertyChanged`, private and protected initialization/event methods, Base `trigger` and `setProperties` are outside the vendor's public instance contract. `Component.Inject` is static. No Vue, React or Angular event variant is reported as MVC behavior.

## Render helper and options

Both helpers keep the developer-chosen id, call the matching host's `EJS().RadioButton`, execute the supplied builder and return `plan.Render<FusionRadioButton>`. A radio is unbound in M21.N36. X1022 carries the original direct-render demonstration across two selected room choices. The net10 wrapper XML documents the builder at lines 61291–61363, the MVC 5 wrapper at lines 41203–41275. `builder-net10.txt` and `builder-net48.txt` retain the exact member excerpts in the packet.

Both wrapper assemblies also declare the builder's Checked member, absent from their documented XML entries. RadioButtonBuilder.Checked, Disabled, EnableHtmlSanitizer, EnablePersistence and EnableRtl take an optional boolean whose CLR metadata constant is true in every case (net10 methods 26861 and 26863–26866, net48 methods 32520 and 32522–32525, with the matching constant-table extracts in wrapper-metadata-net10.txt and wrapper-metadata-net48.txt). C# substitutes that true value at the call site. Calling one of those methods without its argument therefore uses the same vendor startup branch as passing true. Omitting the builder call leaves EJ2's own property default in force. X1400/1410 cover checked, X1401/1407 disabled, X1415 sanitizer and RTL and X1409/1413 persistence. The only public Fusion render helper takes the developer id and Action<RadioButtonBuilder>. Render, HTML output and builder identity are host infrastructure.

| Host construction setting | Proof or scope |
|---|---|
| Label, group name, raw value, developer id | X1022 reads each selected value through the same designated component. X1404 starts with both group configurations. X1405 submits the actual named, checked radio. X1416 renders a named option with no configured value. The unbound ASP.NET Core wrapper's RadioButtonBuilder.Render (RadioButtonBuilder-wrapper.cs:125–130) emits the developer id, provided HtmlAttributes and the radio type. The default CLR Value is empty (RadioButton-wrapper.cs:77–80). EJ2 initialization writes a value attribute only for a nonempty configured Value in MVC (RJ190–197). A checked unvalued native input therefore yields `on` to GetSelectedValue, while the live value and actual notification yield the empty string. |
| Checked, disabled | X1400 releases a previously checked room. X1401 opens an initially disabled room and attempts native activation in the same turn. X1407 and X1408 read the actual initially enabled or disabled input. |
| HTML attributes | X1405 supplies a value attribute only in the contracted booking. Its independently changed live `Value` still reads garden while the native submission retains the contract code. The ordinary booking submits garden. A retained research run also observed that supplying a group role and accessible name through HtmlAttributes writes them to both the initial native input and the EJ2 wrapper under ASP.NET Core. It overwrote the input’s native role. The final room-card views use an ordinary fieldset and preserve the radio. See red-card-aria.log. |
| EnablePersistence | X1409 and X1413 navigate to a separately rendered reader configured with persistence already enabled. Only a workstation with recorded state restores the room. |
| CssClass, EnableRtl, LabelPosition, EnableHtmlSanitizer | X1415 opens two saved editions built before any plan action. The actual localized caption is asserted directly. The staff print instruction reads the configured class, direction and position from the newly constructed radio. The house or imported template changes the real inert print legend according to the initial filtering policy. X1406 and X1411 cover live settings separately. |
| Locale | The wrapper accepts the culture. The radio has no localized resource read or live locale reaction. X1415 configures culture and translated text independently. |
| Change and Created callback strings | The MVC wrappers document string callback names. The typed `Reactive` subscription is used for change. Created is skipped by the owner. |

The room-card checks preserve the native radio and its accessible caption under ordinary vendor markup. They observe the visible caption, selection, printed direction and caption-order instruction, and house stock notice. The .NET tests do not directly inspect the generated wrapper class, the generated label classes or their pixel geometry. The selector rule permits an authored id, role, label or visible text. EJ2 creates its class-bearing label without its own id or accessible role. RJ225–233 and 301–326 supply those internal styling details.

The .NET 10 browser exercises the actual ASP.NET Core render helper. The matching MVC 5 helper has its own platform-specific source and documented package signatures. A Windows MVC 5 browser result is separate from the net10 cases.

## Browser evidence

The net10 fixture is WhenAdmissionStaffArrangeRoomChoices. Each row names the saved production plant log, assertion and mismatch. The shared full restored run also included the pre-existing X1022.

| Case | Plant | Assertion line | Observed mismatch |
|---|---|---|---|
| M21.X1400 | plant-1400-opposite.log | 28 | Locator expected not to be checked - actual "checked" |
| M21.X1400 | plant-1400-primary.log | 21 | Locator expected to be checked - actual "unchecked" |
| M21.X1401 | plant-1401-bind.log | 39 | Locator expected to have text 'Held for cleaning' actual 'Reservation not attempted' |
| M21.X1401 | plant-1401-opposite.log | 45 | Locator expected to have text 'Single room reserved' actual 'Held for cleaning' |
| M21.X1401 | plant-1401-primary.log | 39 | Locator expected to have text 'Held for cleaning' actual 'Single room reserved' |
| M21.X1402 | plant-1402-opposite.log | 61 | Locator expected to have text 'Companion suite requested' actual 'No request recorded' |
| M21.X1402 | plant-1402-original.log | 56 | Locator expected to have text 'Single room requested' actual 'No request recorded' |
| M21.X1402 | plant-1402-primary.log | 56 | Locator expected to have text 'Single room requested' actual 'No request recorded' |
| M21.X1402 | plant-1402-vendor.log | 56 | Locator expected to have text 'Single room requested' actual 'No request recorded' |
| M21.X1403 | plant-1403-opposite.log | 76 | Locator expected to be focused - actual "inactive" |
| M21.X1403 | plant-1403-primary.log | 71 | Locator expected to be focused - actual "inactive" |
| M21.X1404 | plant-1404-opposite.log | 97 | Locator expected to have text 'Single room with independent respite' actual 'Short stay replaces the single room' |
| M21.X1404 | plant-1404-primary.log | 89 | Locator expected to have text 'Short stay replaces the single room' actual 'Single room with independent respite' |
| M21.X1405 | plant-1405-opposite.log | 116 | Locator expected to have text 'Contract bed reserved' actual 'Garden wing reserved' |
| M21.X1405 | plant-1405-primary.log | 107 | Locator expected to have text 'Garden wing' actual 'Wing awaiting allocation' |
| M21.X1406 | plant-1406-opposite.log | 140 | Locator expected to have text 'Print left to right with the room name last' actual 'Check the room card layout before printing' |
| M21.X1406 | plant-1406-primary.log | 130 | Locator expected to have text 'Print right to left with the room name first' actual 'Check the room card layout before printing' |
| M21.X1407 | plant-1407-opposite.log | 155 | Locator expected to have text 'Garden suite' actual 'placement' |
| M21.X1407 | plant-1407-primary.log | 150 | Locator expected to have text 'Hold for housekeeping' actual 'Slip not issued' |
| M21.X1408 | plant-1408-opposite.log | 170 | Locator expected to have text 'Garden suite' actual 'placement' |
| M21.X1408 | plant-1408-primary.log | 165 | Locator expected to have text 'Transfer on hold' actual 'Transfer awaiting review' |
| M21.X1409 | plant-1409-opposite.log | 190 | Locator expected to have text 'Use referral default' actual 'Remember this room' |
| M21.X1409 | plant-1409-primary.log | 184 | Locator expected to be checked - actual "unchecked" |
| M21.X1410 | plant-1410-opposite.log | 209 | Locator expected to be checked - actual "unchecked" |
| M21.X1410 | plant-1410-primary.log | 203 | Locator expected not to be checked - actual "checked" |
| M21.X1411 | plant-1411-opposite.log | 226 | Locator expected to be visible - actual "hidden" |
| M21.X1411 | plant-1411-primary.log | 220 | Locator expected not to be visible - actual "visible" |
| M21.X1412 | plant-1412-opposite.log | 246 | Locator expected to have text 'Garden suite' actual 'Room alias cleared' |
| M21.X1412 | plant-1412-primary.log | 238 | Locator expected to have text 'Room alias cleared' actual 'Referral room' |
| M21.X1413 | plant-1413-opposite.log | 271 | Locator expected not to be checked - actual "checked" |
| M21.X1413 | plant-1413-primary.log | 262 | Locator expected to be checked - actual "unchecked" |
| M21.X1414 | plant-1414-opposite.log | 288 | Locator expected to have text 'No request recorded' actual 'Single room requested' |
| M21.X1414 | plant-1414-primary.log | 282 | Locator expected to have text 'Single room requested' actual 'No request recorded' |
| M21.X1414 | plant-1414-trusted.log | 282 | Locator expected to have text 'Single room requested' actual 'No request recorded' |
| M21.X1415 | plant-1415-opposite.log | 308 (now 309) | Locator expected not to be visible - actual "visible" |
| M21.X1415 | plant-1415-primary.log | 299 (now 300) | Locator expected to have text 'Print right to left with the room name first' actual 'Check the room card layout before printing' |
| M21.X1416 | plant-1416-opposite.log | 326 | Locator expected to have text 'G-14' actual 'G-15' |
| M21.X1416 | plant-1416-primary.log | 321 | Locator expected to have text 'Request the next available bed' actual 'Choose a placement before booking' |

The packet retains red.log for the initial 14 expected failures, red-1414.log for the native-activation case and red-1415.log for the saved-card construction case. The corrected room-card cases failed at their two print instructions under ordinary rendering in red-card-preview.log while the other 15 cases passed. The 17-case room-card run is in green-cards.log. The final omitted-value case has its own red-1416.log and green-1416.log, and the complete 18-case result is in restored.log. The coverage excludes the discarded guide-copy and role-based prototype observations.

```text
[playwright:result] M21.F36@net10 1/1: 18 selected, 18 ran, 18 passed, 0 failed, 0 over budget, slowest 2.67s Alis.Reactive.Conformance.PlaywrightTests.FusionComponents.WhenAdmissionStaffArrangeRoomChoices("net10").a_contract_bed_keeps_its_reserved_code_when_the_garden_wing_is_requested
```
