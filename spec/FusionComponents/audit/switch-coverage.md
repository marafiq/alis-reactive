# FusionSwitch public API coverage

The source inventory starts at cc05d5de4. The proof table records the implemented cases.
Exact logs and both inventory exports are retained under the FusionSwitchFull landing folder in the
fusion-switch-full worktree.

## Pin, rendering and existing proof

The browser umbrella @syncfusion/ej2 is 33.1.47 and depends on Buttons ~33.1.44 and Base ~33.1.45. The lock and installed packages resolve @syncfusion/ej2-buttons 33.1.44 and @syncfusion/ej2-base 33.1.45 (package-lock.json:1327–1337, 1398–1406, 1430–1438). Both NuGet wrappers are 33.1.47 (Directory.Packages.props:26–27). build.mjs:91 copies the pinned umbrella bundle and _FusionLayout.cshtml:17–23 loads it and calls EJ2's ScriptManager before the framework runtime. Installed vendor source is audited below. The existing checked/change probe ran on the umbrella bundle.

FusionSwitchHtml.cs:17–28 forwards to the configurable helper and sets the model's checked start and binding name before the builder callback. Its platform-specific files both call field.Html.EJS().Switch(field.Id) and field.Render<FusionSwitch>(toggle.Render()). The two NuGet packages' SwitchBuilder XML document class, disabled, persistence, RTL, attributes, locale, name, labels, value and vendor callback options (ASP.NET Core net10 XML:62185–62256, MVC5 net462 XML:40314–40386). The builder callback has separate render semantics from live behavior. No new host rendering run occurred in this phase.

FusionComponent.cs:18–22 defines lookup at ej2_instances.0, with addEventListener and removeEventListener. FusionSwitch.cs:17–32 and both manifests:1317–1323 expose Checked R/W, Change, and optional ChangeEventArgs.Checked as a read. Both manifests:1598–1599 expose the render helpers. The typed input gathers Checked and commits by Change. WhenASwitchIsRenderedForAMember.cs:14–25 proves M21.X1027 true/false at render. It never exercises live reads/writes, reactions, calls or configured option effects. spec/probes/facts.json:3279–3292 records one real click with checked:boolean, event:object, name:string and no isInteracted. That vendor probe does not prove a typed handler.

Inventory printed “FusionSwitch: 6 reachable vendor shapes; root @syncfusion/ej2-buttons/src/switch/switch.d.ts:15:1#Switch”. operations.json records all declaration locations, type alternatives, arities and manifest candidates. Root has 37 names. S refers to Buttons src/switch/switch.d.ts, SJ its JS, E Buttons common.d.ts, C Base component.d.ts, CJ its JS, B Base base.d.ts and BJ its JS. R/W is vendor-declared access. Case numbers refer to proposals further below.

## Root operations

| Name, declaration | Exact shape and effect | Verdict |
|---|---|---|
| addEventListener, B:86 | (string, Function) -> void. BJ:157–159 forwards to observer. | Typed vendor subscription carrier, exercised via C2/C4. Raw door excluded by AGENTS.md:40–44 and D17.3. |
| allowServerDataBinding, B:26 | bool R/W, BJ:23 and 123–134. Used under isBlazor. | Exclude Blazor transport wiring in MVC. |
| appendTo, C:116 | optional string/HTMLElement -> void. Omission uses current element, string is selected, element is used directly. CJ:209–232, 264–269 installs and renders. | Render-owned in all three forms. Exclude remounting existing bound input by raw selector or alternate holder, D14.3/D17.13. |
| beforeChange, S:32 | Event optional payload members cancel, checked and original DOM event. | Eligible synchronous event, C4/C5/C6. |
| change, S:38 | Event delivered after successful native/programmatic flip. SJ:92–98, 432–440, 470–472. A property bind does not emit it, SJ:221–224. | Existing door, live proof C2/C3/C6/C7. |
| checked, S:51 | bool R/W false default SJ:492–493. Updates actual input and active track SJ:59–83 and 222–224. | Start proven M21.X1027, live read/write and gather C1/C7. |
| click, S:184 | () -> void. Native input click SJ:470–472. | Eligible Click() C3, native disabled semantics and original DOM click. |
| created, S:44 | Event declared EmitType<Event>. MVC appendTo triggers without argument CJ:264–269. Refresh does not re-emit CJ:128–140. | **Owner exclusion, 2026-11-27, all Fusion created events.** Initial appendTo precedes typed subscription installation (_FusionLayout.cshtml:19–23, runtime/index.ts:3–4). Raw callback string and synthetic ready are distinct lanes. No typed door or payload test proposed. |
| cssClass, S:57 | string R/W empty default. Wrapper gets normalized tokens and live change removes prior tokens SJ:201–207, 259–265, 495–496. | Eligible Property<string>, C9. |
| currentContext, C:45 | R/W object with optional Function calls and any args. CJ:396–405 writes React-render context. | Exclude wrapper plumbing. Nest below. |
| dataBind, C:129, B:73 | () -> void. Applies pending change immediately. Normal set also schedules a deferred auto-bind BJ:111–121, 135–148. | Eligible DataBind() C8. Eventual UI appearance alone cannot prove the explicit call. |
| destroy, S:117 | () -> void. Unwires and unwraps, resets an input and removes instance (SJ:106–123, Buttons common.js:63–78, BJ:252–271). | Eligible terminal app use C12. Landed pattern: FusionMultiSelect.cs:48, AllergyRemoval.cshtml:24–25 and WhenACardsMultiSelectIsDestroyedByItsHost.cs. Stop using the destroyed input locator. |
| disabled, S:64 | bool R/W false default. Native disabled, wrapper class/ARIA and listener changes SJ:225–239, 329–334, 498–499. | Eligible Property<bool>, C8. |
| ej2StatePersistenceVersion, C:20 | string R/W only used in persistence key with separate vendor global version mode CJ:44–48, 166–172, 413–434. | Exclude MVC slice door without an independently scoped global persistence policy. Existing standalone MVC render and typed APIs do not turn on global mode, and the property is inert there (audit/README.md:51). |
| element, C:17 | R/W, instantiated HTMLInputElement, set on preRender SJ:279–284. | Eligible shallow read as ChildElement C13. Replace-holder write would bypass D14.3/D17.13. |
| enablePersistence, C:26 | bool R/W false default CJ:520–522. Mount/refresh attaches pagehide. Saves checked, then merges on mount (CJ:128–140, 181–193, 209–232, 413–434; SJ:146–148). DataBind alone has no persistence case. | Eligible supported read and enable-with-refresh C11. Live disable-after-enable remains open because destroy only detaches when flag is true. |
| enableRtl, C:32 | bool R/W, starts with global fallback CJ:74–76. Wrapper class path SJ:201–204, 251–258. | Eligible Property<bool>, C10. |
| focusIn, S:191 | () -> void, native focus SJ:479–480. | Eligible FocusIn() C14. |
| getRootElement, C:81 | () -> HTMLElement, returns element in MVC, actualElement for React hybrid CJ:152–159. | Eligible ReturnedObject<ChildElement>() C13. |
| htmlAttributes, S:97 | String-to-string map R/W empty default. Wrapper title/class/style and other attrs, and input id, name, role, disabled and more SJ:30, 267–269, 344–373, 512–514. | Safe whole-map title use C15. Property<IReadOnlyDictionary<string,string>> is a candidate requiring shape testing and D14.3 guard. Runtime field identity and direct indexed access stay bounded, below. |
| ignoreCollectionWatch, B:41 | bool R/W false default BJ:30. Internal complex collection tracking, notify-property-change.js:150–162. Switch has no such decorated collection. | Exclude implementation flag with no useful Switch effect. |
| isAngular, C:51 | bool R/W false at CJ:68–73. Switch has Angular-only keyup path SJ:375–383. | Exclude framework-host override in MVC. |
| isDestroyed, B:22 | bool R/W. BJ:252–271 removes its instance before true. | Exclude this lifecycle bookkeeping flag in the current MVC slice, both R/W. The live bound instance only supplies false. After teardown the typed direct locator loses it and author observes the terminal UI via C12. No independent MVC use case established (audit/README.md:51). |
| isReact, C:52 | bool R/W false CJ:68–73, no Switch MVC use. | Exclude foreign host flag. |
| isServerRendered, B:25 | bool R/W false BJ:22, no Switch behavior. | Exclude integration flag. |
| isVue, C:53 | bool R/W false CJ:68–73, no Switch MVC use. | Exclude foreign host flag. |
| locale, C:38 | string R/W, global culture default CJ:77–79. Switch has no locale case and writes authored label text literally SJ:217–271, 335–343. | Initial builder option exists. Exclude live door with no distinct Switch UI effect. Use explicit translated labels. |
| name, S:71 | string R/W empty default; copied into native form key initially and on bind SJ:162–164, 244–246 and 501–502. | Existing binding carrier and receipt C7/C16. Raw field-name reads for wiring and writes are excluded by typed field identity D14.3. Use typed model/member binding. |
| offLabel, S:83 | string R/W empty default, then nonempty changes visible off span SJ:247–250, 335–343, 506–508. Empty write leaves old label text in existing DOM. | Eligible Property<string>, C9. |
| onLabel, S:77 | string R/W empty default, nonempty changes on span SJ:247–250, 335–343, 503–505. | Eligible Property<string>, C9. |
| portals, C:54 | any R/W. React wrapper material CJ:399–405, 436–443, no Switch consumer concrete API. | Exclude weak host plumbing. |
| refresh, C:74 | () -> void, re-renders same object, preserving registered instance on this Switch. Does not raise created (CJ:128–140, SJ:106–123). | Eligible Refresh() C17. Needs a result different from auto-bind. |
| removeEventListener, B:94 | (string, Function) -> void, forwards to observer BJ:167–169. | Typed subscription release carrier D17.3/D12.5. Same exclusion for raw API as add. |
| root, C:18 | any R/W, getRootElement reads element or actualElement, not root CJ:152–159. | Exclude weak foreign-host plumbing. |
| serverDataBind, B:74 | ({ [key:string]:any })? -> void, omission uses empty changes. BJ:123–134 exits immediately in MVC. | Exclude both forms, Blazor wiring. Nest below. |
| toggle, S:174 | () -> void, invokes click handler with no event SJ:84–99, 432–434. | Eligible Toggle() C3/C4/C6, distinct from native Click. |
| value, S:90 | string R/W empty default, writes native value at init and bind SJ:165–167, 241–243, 509–511. | Eligible Property<string>, C16. Typed Reactive input still gathers bool. |

The report excludes JSDoc-private Switch constructor, getPersistData and onPropertyChanged (S:99–110, 129–133, 142–150), protected render/preRender and named private methods; Base setProperties and trigger (B:50–58, 95–106); Component getLocalData/unload, on/off/notify, createElement, triggerStateChange (C:82–109, 130–158, 180–197). Component.Inject (C:172) is static and not an instance API. CJ:118 raises an undeclared destroyed event. The observer adds beforeChange.name at runtime but BeforeChangeEventArgs has no such advertised member. D17.5 excludes these declarations. Existing M21.X711 refusal keeps IsInteracted out.

## Reachable payloads and objects

| Path, declaration | Reachable operation and disposition |
|---|---|
| beforeChange.cancel, E:114 | Optional bool, starts false and vendor reads it after handlers return. Eligible Property<bool>(optional:true), C4 set and independent same-turn read. |
| beforeChange.checked, E:122 | Optional bool R/W, holds prior state. EJ2 negates the exact post-handler field (SJ:92–95). C5 separate read and write. |
| beforeChange.event, E:107 | Optional Event. Event key can hold undefined on Toggle. Real input click and Click() supply a DOM event. C6 shallow optional read. Replace excluded because vendor never rereads event and the caller would need actual DOM identity. |
| change.checked, E:93 | Optional bool, supplied false/true after flip. Existing read C2. Vendor has completed write and ignores any write of this payload member. |
| change.event, E:91 | Optional Event. Same undefined distinction. C6 shallow read. Writes ignored by vendor. |
| change.name, Base event-handler.d.ts:69 | Optional string. Base Observer sets name before notifying at observer.js:84–102, existing Chromium probe confirms. C6 per-event route read. Vendor ignores write. |
| created declared Event | MVC append has no argument CJ:264–269. Root owner exclusion, no payload declaration or proof. |
| DOM Event under beforeChange/change | Start at vendor-facing live boundary. NativeCheckBox.cs:28–39 supplies a grounded lib.dom Event Type/IsTrusted exemplar. Proposed C6 shallow read distinguishes the absent, actual user, and programmatic native-click paths. Do not use trust for authorization. Full DOM chains excluded by owner in audit/README.md. |
| currentContext.calls and args, C:46–47 | Optional Function and any R/W, same excluded React plumbing. |
| htmlAttributes string index, S:98 | string-valued R/W. Proposed typed whole-map assignment C15. Individual indexed R/W open: MemberPath.cs:86–101 only walks member expressions, and a typed whole dictionary is different from arbitrary JS property strings. |
| serverDataBind(newChanges) string index, B:75 | any-valued R/W, untyped and irrelevant to MVC host. |
| HTMLInputElement/HTMLElement reached above | Boundary with existing public ChildElement.GetAttribute in Element.cs:205–215. C13 shallow read. No DOM descendant reproduction. |

## Concrete net10 cases and proposed plants

Use a resident alert preferences panel in record-contacts, keeping the existing bound-bed-rails cases. Its controls, form receipts and settings describe real workflows. Run each changed assertion red, green and with its independent one-expression plant. The options demonstration distinguishes initial render through the configurable helper from live reads and writes. Labels in quotes below are proposed exact assertions. Case rows grouping related operations are not permission to use one test for different independent effects.

| Case | Gesture, outcome and opposite | Proposed defect |
|---|---|---|
| C1 live Checked | In fresh off screen select **Enable night alerts**. The switch becomes checked and Review routing reports “Send to night desk” using the actual property. Fresh on screen **Pause night alerts** shows unchecked and “Do not send alerts”. | Change normal live property write to the opposite bool. For the read, substitute the secondary opposite setting in the review, yielding the wrong receipt. |
| C2 Change/Checked | Click actual switch from off or on. Typed payload yields “Night alert enabled” or “Night alert paused”. The separate property-configure path changes the control and retains “No change recorded”. | Read the prior state instead of the post-change payload, or wrong payload member. For no-event path substitute a public Toggle in place of a Checked bind. |
| C3 Toggle/Click | **Enable from panel** calls Toggle and records “Panel adjustment”. **Activate alert control** calls Click and records “Control activation”. Both actually flip, including from on. In a fresh managed disabled world native activation keeps “Managed and paused”. | Swap the method bodies, which preserves visual flip but changes the receipt. For disabled Click, plant Toggle to expose an unintended flip. |
| C4 BeforeChange.Cancel | A button calling Toggle for a ward-managed preference keeps the off state and shows “Managed by the ward”. Resident-owned setting allows “Alerts enabled”. For a second ordered listener that reads the elected veto, “Blocked” vs “Allowed”. | Set cancel false or consult wrong flag. Use panel Toggle for veto proof: native checkbox default can toggle the input before bubbled click, and the cancelled vendor branch has no input-reset write. Native click/keyboard veto-state parity requires a separate pinned probe before a promise. |
| C5 BeforeChange.Checked | Live prior-state reader reports “Replacing enabled setting” or “Replacing paused setting” according to start. In a separate approved **Keep alerts enabled** command from on, writing prior checked false synchronously yields on. An ordinary Toggle from on yields off. | Read wrong prior bool, or write true at the authoring expression, causing vendor to turn off. |
| C6 DOM and event name | Toggle's missing original event reports “Panel request” in beforeChange. Click reports “Control request”. For Change, actual person clicking switch reports “Resident activation”, programmatic Click reports “Panel activated control”, Toggle reports “Panel adjustment”. Common activity routing using Change.Name shows “Alert setting changed” vs “Pending approval”. | Wrong event mapping loses actual provenance. Wrong Type/IsTrusted read erases positive receipt. Wrong Change.Name read routes to pending case. Independent plants for each output. |
| C7 bound input | Actual flip then **Save alerts** produces server receipt “Night alerts saved as enabled”. Flip from on to off yields “Night alerts saved as paused”. | Use wrong carrier such as Disabled in the typed input declaration. A raw native submitted string is covered separately in C16. |
| C8 Disabled/DataBind | **Lock and apply** writes Disabled true, explicitly binds and calls Click in the same pipeline. Actual input becomes disabled and receipt “Locked and paused”. **Unlock and apply** on initial locked off yields “Editable and enabled”. Review on two starts reads disabled separately. | Change DataBind to a benign synchronous operation, such as FocusIn. Auto-bind happens afterward and allows a flip too early. Wrong writes/read have their own resulting receipt/state error. |
| C9 CSS/label properties | **Use night display** sets wrapper from wide-alert to compact-alert. With the selected on and off states, show “Night dispatch on” and “Night dispatch paused”. Default/alternate shows “Day dispatch on” or “Day dispatch paused”. An actual review uses live values. Separate empty string variation keeps last visible label as source requires. Include positive builder-start option effect in separate configured render case. | Wrong CSS preset or swap sibling label mappings. A stale read gives day receipt after change. Empty-label difference requires separate exact proof rather than assuming clearing. |
| C10 RTL | **Use right-to-left layout** binds true, shows e-rtl wrapper token and review “Right to left”. Opposite in initially-RTL world applies false and says “Left to right”. | Flip authored direction or read prior live state. |
| C11 persistence | **Remember on this browser** enables and refreshes. Flip on and reload same guarded test world, initialized off by server. It restores on and Review storage shows “Remembered here”. Fresh disabled persistence world reloads off and says “Use server default”. | Wrong flag or omit refresh at production call so the unload handle never attaches. Live disable-after-enable is open for separate lifecycle evidence. |
| C12 destroy | **Close preview** calls terminal Destroy. From checked, underlying input remains unchecked and no enhanced track remains in its group. **Open preview** in a fresh world shows one enhanced track and the input checked. No subsequent component lookup after terminal step. | Substitute FocusIn for destroy. Enhanced wrapper remains and required fallback condition is missing. |
| C13 input/returned root | **Review alert channel** reads actual input's aria-label via ChildElement. Report “Night alert channel” vs “Day alert channel” with different configured input selected. Repeat for GetRootElement to exercise an actual call. | Select the wrong rendered holder or map return to incompatible method (e.g. FocusIn) if compiler allows mutation, so the exact positive receipt does not emerge. Existing ChildElement supplies the typed DOM attribute read. |
| C14 focus | **Focus alert control** puts focus on actual switch role. **Review only** leaves focused review button and preserves choice in fresh world. | Map FocusIn to benign DataBind. Switch never receives focus. |
| C15 attribute map | **Set night help** replaces author-provided day title with “Night desk” on switch wrapper. Fresh **Use day help** shows “Day desk”. A separate typed settings receipt reads the entire approved map if shape permits, rather than asserting raw JS state. | Map the wrong whole-map value at normal authoring call, leaving “Day desk”. For candidate read, send prior map to real receipt. |
| C16 native value | **Use night destination** writes and binds raw value “night” on a checked switch. Ordinary HTML submit to dedicated endpoint yields “Night desk selected”. Fresh default checked control submits “day” to “Day desk selected”. A semantic live review can also select the channel without altering typed checked state. | Keep prior production value and see wrong server receipt. |
| C17 refresh | **Restore default preview** writes empty attribute map and refreshes. New enhanced wrapper lacks old title while showing default “Alerts paused” label. Ordinary title update C15 retains existing wrapper and shows new title. | Swap Refresh for DataBind. SJ:344–373 only visits listed keys, so old title survives on the original wrapper. |

D11.3 governs synchronous call choice. D11.4 (Core/DESIGN.md:2882–2895) and Run.tla:477 WritesAfterTheTurnAreRefused govern the cancellable event's immediate writes. Ownership.tla:442 HandlersHearInTheOrderTheySubscribed applies to C4's separate beforeChange observers. D17.3 applies to typed subscription, D12.5/D12.8 apply to release, and D14.3/D17.13 for binding and first render. No new TLC, build or browser run occurred in phase one. Case titles and mutations are proposals for spec placement.

## Phase two results

The lead placed M21.X1120 through M21.X1132. The declaration and both manifests now carry those operations. The C# name OriginalEvent maps to the pinned JavaScript event member. Three distinct application receipts prove the optional holder and read of isTrusted: Toggle supplies no event, native Click supplies an untrusted one, and direct labeled input activation supplies a trusted one.

| Proof | Exact operation exercised | Individual defects and restoration evidence |
|---|---|---|
| M21.X1120 | Checked live assignment and live read; DataBind then actual enable and pause | `plant-primary-1120.log` at test line 20, `plant-opposite-1120.log` at test line 26, `plant-payload-1120.log` at test line 21 |
| M21.X1121 | Change subscription; optional Checked false and true from direct labeled clicks | `plant-primary-1121.log` at test line 37, `plant-opposite-1121.log` at test line 42 |
| M21.X1122 | Toggle as synchronous switch action; no original DOM event and distinct receipt | `plant-primary-1122.log` at test line 52, `plant-opposite-1122.log` at test line 58 |
| M21.X1123 | Click as native activation; Change.OriginalEvent.IsTrusted false or true by source | `plant-primary-1123.log` at test line 68, `plant-opposite-1123.log` at test line 74, `plant-payload-1123.log` at test line 68 |
| M21.X1124 | BeforeChange subscription; synchronous Cancel write on Toggle vs allowed action | `plant-primary-1124.log` at test line 85, `plant-opposite-1124.log` at test line 90 |
| M21.X1125 | BeforeChange.Checked reads old on state and writes the inverted payload before the vendor uses it | `plant-primary-1125.log` at test line 101, `plant-opposite-1125.log` at test line 106, `plant-payload-1125.log` at test line 100 |
| M21.X1126 | Disabled lock and unlock; immediate DataBind affects the very next native Click | `plant-primary-1126.log` at test line 116, `plant-opposite-1126.log` at test line 123 |
| M21.X1127 | OnLabel and OffLabel writes, each in its active switch state | `plant-primary-1127.log` at test line 135, `plant-opposite-1127.log` at test line 140 |
| M21.X1128 | CssClass write adds compact and then replaces compact by wide in a fresh configured start | `plant-primary-1128.log` at test line 150, `plant-opposite-1128.log` at test line 155 |
| M21.X1129 | EnableRtl live true and false; review text visibility depends on the actual e-rtl wrapper class | `plant-primary-1129.log` at test line 165, `plant-opposite-1129.log` at test line 170 |
| M21.X1130 | FocusIn focuses the rendered switch; Review only retains its own focus | `plant-primary-1130.log` at test line 180, `plant-opposite-1130.log` at test line 185 |
| M21.X1131 | Destroy removes the enhanced track and unchecks the same original input | `plant-primary-1131.log` at test line 195, `plant-opposite-1131.log` at test line 201 |
| M21.X1132 | Value write reaches native MVC form submission without changing boolean input binding | `plant-primary-1132.log` at test line 212, `plant-opposite-1132.log` at test line 217 |

```text
[playwright:result] WhenStaffConfigureNightAlerts@net10 1/1: 13 selected, 13 ran, 0 passed, 13 failed, 0 over budget, slowest 5.36s Alis.Reactive.Conformance.PlaywrightTests.FusionComponents.WhenStaffConfigureNightAlerts("net10").a_ward_managed_preference_declines_the_same_choice_a_resident_can_apply
[playwright:result] M21.F46@net10 1/1: 14 selected, 14 ran, 14 passed, 0 failed, 0 over budget, slowest 2.41s Alis.Reactive.Conformance.PlaywrightTests.FusionComponents.WhenASwitchIsRenderedForAMember("net10").the_switch_starts_on_or_off_as_the_member_holds
```

The `operations-final.json` reconciles the declarations against the same six vendor shapes and retains unimplemented paths. Both public manifests were compiled with their own Release target. See `build-net10.0.log` and `build-net48.log`.

### Remaining open variants

- C11 persistence remains open, both the enable-and-refresh path and live disable. The vendor only attaches its pagehide listener on mount or refresh, and detaches it only while the flag remains true.
- C13 shallow element and GetRootElement remain open. The public DOM-value carrier exists, but no placed Switch case uses it.
- C15 whole-map HtmlAttributes remains open. Emission and the title effect need proof. Individual per-key updates have no typed member path, and arbitrary map writes can change field identity.
- C17 Refresh remains open. A distinct wrapper rebuild effect needs proof beyond DataBind.
- The shallow BeforeChange original event, Change payload name, Cancel read and prior Checked false receipt remain outside the placed cases. Direct native checkbox or keyboard cancellation remains open because the pinned vendor veto branch does not restore the input after native activation.
- The live getters for Value, Disabled, CssClass, EnableRtl and both label properties lack their own named browser assertions. Empty label writes retain old rendered text in the pinned implementation and still need exact proof. The separate Reactive bool-gather path in C7 and a no-Change receipt after an assigned Checked stay open.
- The owner excluded created for the whole Fusion family. The renderer and weak host plumbing exclusions in the inventory still apply.
