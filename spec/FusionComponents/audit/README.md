# Fusion's live object contract at 9c77fd7d8

All 51 control classes and the app-level Toast resolve to EJ2 types. The second app-level object, Confirm, has its own vendor. Across the 52 EJ2 roots, 434 of 3,746 public instance member names have a direct declaration in both Fusion host manifests. An inherited member counts once on each slice where a developer can reach it. These counts measure named doors. They leave signature compatibility and behavior to separate evidence.

| At an EJ2 root | Member names |
|---|---:|
| Direct Fusion door on both hosts | 434 |
| No Fusion door, typed declaration candidate | 2,410 |
| No Fusion door, generic, heterogeneous or weak shape to review | 846 |
| No Fusion door, DOM-valued member | 56 |

[The per-slice table](slice-rows.tsv) names each member at each root, each concrete event payload alternative, and each separately resolved C# nested declaration. Search its first two columns for a slice and a site, such as `FusionComboBox` and `event.change`. The evidence file produced below adds the vendor file and line, full type and all call signatures, C# declarations and both host signatures. The 1,290 vendor object records share source declarations. C# holders and matches are scoped separately to every slice and event. Repeated vendor types never supply another slice's doors.

## Which ComboBox members are missing?

| JavaScript path | Pinned declaration | What a Fusion view can reach at this revision |
|---|---|---|
| `dataSource` | `DropDownBase.dataSource`, `ej2-dropdowns/src/drop-down-base/drop-down-base.d.ts:390`. The base accepts object-row arrays, string, number or boolean arrays, and a `DataManager`. | No live property. The render helper accepts initial items. MultiSelect has its own live array door at `FusionMultiSelect.cs:26`. |
| `change.itemData` | `SelectEventArgs.itemData`, `drop-down-base.d.ts:79`, through `ChangeEventArgs extends SelectEventArgs` at `drop-down-list/drop-down-list.d.ts:9`. Declared `FieldSettingsModel`. | Missing. The selected row held the application's fields in the object-backed keyboard selection. |
| `change.previousItemData` | `ChangeEventArgs.previousItemData`, `drop-down-list.d.ts:24`. Also declared `FieldSettingsModel`. | Missing. The prior row held its own application fields in the same interaction. |
| `change.item` | `SelectEventArgs.item: HTMLLIElement`, `drop-down-base.d.ts:74`. | Missing typed list element. Its text is independently observable. |
| `change.previousItem` | `ChangeEventArgs.previousItem: HTMLLIElement`, `drop-down-list.d.ts:19`. | Missing typed prior list element. The DOM traversal boundary applies after the element. |
| `change.cancel` | Optional boolean on `ChangeEventArgs`, `drop-down-list.d.ts:42`. | Missing declaration. Its effect on the interaction requires behavioral evidence. |

The chain starts in `@syncfusion/ej2-dropdowns/src/combo-box/combo-box.d.ts:25`, then `drop-down-list/drop-down-list.d.ts:59`. The entire shipped `FusionComboBox<TValue>.ChangeEventArgs : JsObject` exposes `ReadOnly<TValue> Value` and `ReadOnly<bool> IsInteracted`. Both host manifests list exactly these two payload properties. M21.N13 currently specifies this narrower surface. [Declaration](../../../Alis.Reactive.Fusion/FusionComponents/FusionComboBox/FusionComboBox.cs) · [net10](../../../Alis.Reactive.Fusion/PublicAPI/net10.0/PublicAPI.Unshipped.txt) · [net48](../../../Alis.Reactive.Fusion/PublicAPI/net48/PublicAPI.Unshipped.txt) · [M21.N13](../module.md)

**Proof obligation for the typed Change handler.** A view using `.Reactive(box => box.Change, ...)` must be able to select the current and previous application's row types, then read their own nested fields through the handler's payload. In the recorded keyboard selection English → Punjabi, the visible input became Punjabi. The current row contained `label = Punjabi`, `code = pa` and `details.interpreter = true`. The prior row contained `label = English`, `code = en` and `details.interpreter = false`. The event carried `value = pa`, `isInteracted = true`, and two list elements. A browser proof through the typed handler must show a user-visible effect of reading **both** rows, including the nested values. The observation also confirms that `FieldSettingsModel`, whose fields describe the text/value mapping, is insufficient as a model of these application rows.

The lead's pinned gesture probes also exercised selected values of string, numeric zero, boolean false and object type. An object-backed row can have a primitive value. Each answer about a primitive-row data source, custom entry, clear, and first selection with no prior choice requires its own data-source shape and gesture. The repeated four-value probe records object rows with fields named `id`, `label` and `support.interpreter`, and the text of the selected list item. [Probe](probe-combo-box.mjs) · [Observed output](probe-combo-box.json)

## What does each filtering handler receive?

The public `FilteringEventArgs` interface at `@syncfusion/ej2-dropdowns/src/drop-down-base/drop-down-base.d.ts:822–849` declares `preventDefaultAction: boolean`, `baseEventArgs: Object`, `cancel: boolean`, `text: string` and `updateData(...): void`. Every slice owns its own outward C# event and `JsObject` payload.

| Slice and vendor event line | Filter event and nested C# surface | Keyboard gesture on this version |
|---|---|---|
| ComboBox · `combo-box/combo-box.d.ts:113` | Event and nested payload missing | Typing `lat` supplied `baseEventArgs` as a KeyboardEvent. |
| DropDownList · `drop-down-list/drop-down-list.d.ts:357` | Event and nested payload missing | No observation recorded here. |
| AutoComplete · `auto-complete/auto-complete.d.ts:140` | Its own `FilteringEventArgs` exposes read-only Text, writable PreventDefaultAction and one UpdateData overload. Cancel and BaseEventArgs missing. | Typing `lat` supplied neither `baseEventArgs` nor `event`. |
| MultiSelect · `multi-select/multi-select.d.ts:615` | Its own `FilteringEventArgs` exposes the same three doors. Cancel and BaseEventArgs missing. | Focus, ArrowDown, then typing `lat` supplied `event` as a KeyboardEvent and omitted `baseEventArgs`. |

All three observed filtering objects supplied boolean `cancel` and `preventDefaultAction`, string `text` and `name`, and callable `updateData`. The declared `baseEventArgs: Object` cannot support one mandatory KeyboardEvent door across all slices. Presence on other gestures remains open. The captured member set concerns one event occurrence on each component. [Probe](probe-combo-box.mjs) · [Observed output](probe-combo-box.json)

**UpdateData arguments.** EJ2 permits `dataSource` as an array of object rows, strings, numbers or booleans, or as a `DataManager`. The second argument is optional `Query`; the third is optional `FieldSettingsModel`. AutoComplete and MultiSelect expose only `UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource)`. Their source and both public APIs confirm the absence of a DataManager, query or fields argument. ComboBox and DropDownList lack the event that would grant access to any variant. Separate static signature evidence from a real remote manager's query, mapping and results. [AutoComplete](../../../Alis.Reactive.Fusion/FusionComponents/FusionAutoComplete/FusionAutoComplete.cs) · [MultiSelect](../../../Alis.Reactive.Fusion/FusionComponents/FusionMultiSelect/FusionMultiSelect.cs)

One existing typed example sends the filter text to the application's allergy endpoint and calls `UpdateData` with the returned list. Its net10 browser test types `lat` and expects the single option Latex, then types `xyz` in a fresh world and expects “No records found”. This audit read the view and test. It did not execute that browser test or credit it to the DataManager, query or fields variants. [View](../../../tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/AllergySearch.cshtml) · [Test](../../../tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionMultiSelect/WhenAMultiSelectsFilteringAnswersAfterARequest.cs)

## Where do the remaining rows lead?

- Follow each graph member's `value.objects`, `payload.objects`, each signature's parameter/result `objects`, or an object's `indexes`. An ID contains an exact package-relative file, line, column and symbol. The ID can reappear on a cycle. The graph includes inheritance, array items and reachable named or anonymous vendor objects. A generic type definition is stored once, with the use site's instantiated type text. A member with an opaque payload can have an empty object list. The 183 root event entries with no concrete vendor payload object include weak and scalar types.
- The 536 per-slice table sites include the separate Confirm entry. The graph contains 14,249 rows across reusable vendor objects. A C# name match establishes public exposure only. Compare its captured source operation, optional/nullable marks, aliases, return and arguments with the vendor signatures. The audit does not run an all-member variance, method-kind, async, callback-result, overload-equivalence or runtime-presence proof. Types with weak, generic or heterogeneous positions stay open to probes. Typed candidates include narrowly typed DOM arguments and implementation-facing methods that EJ2 marks public. Their eligibility still needs a use case. Specialized generic substitution, synthesized members without source locations, callable-object properties and DOM descendant members are outside the recursive graph's guarantee.
- The owner excludes hidden/private package internals and full DOM method chains. The public DOM-valued member itself is listed. AutoComplete's private `text` and PivotView's hidden `currentView` are checked exclusions. Grid's `DataResult<TRow>` mirrors an interface explicitly marked `@hidden` at `@syncfusion/ej2-grids/src/grid/base/interface.d.ts:2903–2905`. Its typed custom-binding page door is recorded; a public source or the existing browser contract must qualify its shape. [Owner decision, November 9](../../CLAUDE.md) · [Module](../module.md)
- Nine C# classes have no public EJ2 object context. The catalog lists them under each slice's `separate` field. They comprise Grid's DataResult, InPlaceEditor's three DOM classes and ActionBeginData, the two Smart render-options classes, Uploader.FilesChangedEventArgs, and Confirm. Uploader's `Files` is a projection through `GetFilesData` and `RawFile`. Its own `FilesChanged` event carries `filesData`; the component code calls EJ2's private trigger under the owner's November 17 exception. [Uploader declaration](../../../Alis.Reactive.Fusion/FusionComponents/FusionUploader/FusionUploader.cs) · [Component code](../../../Alis.Reactive.Fusion/FusionComponents/FusionUploader/fusion-uploader.ts)
- Confirm exposes its own asynchronous `Ask(message)` with the run's abort signal. Its two host render helpers throw at this base. Toast does resolve to a pinned EJ2 object and exposes eight names; its render helpers also throw here. A public declaration or vendor catalog entry cannot establish an available end-to-end app object. [App-level contract](../../FusionAppLevelObjects/module.md) · [Confirm](../../../Alis.Reactive.Fusion/FusionAppLevelObjects/FusionConfirm/FusionConfirm.cs)

## How is the inventory reproduced?

Run from the repository root using the pinned toolchain. At 9c77fd7d8, the npm lock pins the EJ2 umbrella and dropdowns at 33.1.47; other locked subpackages range from 33.1.44 through 33.1.47. The inventory checks each installed Syncfusion version against its own lock entry and records those versions in the graph. Run without `--verify-baseline` to inventory a changed slice or a new vendor pin. Review that slice’s rows and the sources they reference. The checked-in table records 9c77fd7d8.

```bash
. scripts/toolchain-env.sh
audit_lock="$HOME/.local/share/alis-toolchain/build.lock"
/usr/bin/lockf "$audit_lock" caffeinate -i npm ci --no-audit --no-fund < /dev/null
/usr/bin/lockf "$audit_lock" caffeinate -i node spec/FusionComponents/audit/inventory.mjs .landing/FusionAudit/catalog.json .landing/FusionAudit/slice-rows.tsv --verify-baseline < /dev/null
python3 -c 'from pathlib import Path as P; assert P(".landing/FusionAudit/slice-rows.tsv").read_bytes() == P("spec/FusionComponents/audit/slice-rows.tsv").read_bytes()'
```

On a later checkout, replay that comparison with the Fusion source and PublicAPI manifests at 9c77fd7d8. The current inventory remains available with the baseline option omitted.

The TypeScript compiler resolves the package symbols and effective inherited members. The pinned SDK's Roslyn reads C# member ownership, arguments, return types, operations and JS/JSON names. For all 832 inventoried members, the audit checks a one-to-one getter or method match against each host’s shipped and unshipped API files, comparing member name, type parameters, parameter names and types, and return type after normalizing qualification and manifest syntax. Constructors, framework metadata and render helpers sit outside the live-door count. The catalog includes the matching manifest rows and their locations. The compiler and source reader run under the same exclusive lock.

After the conformance browser runner has restored its pinned browser package, replay the raw vendor observations
with this command. Review changes in the graph or payloads after a vendor upgrade alongside the
typed tests named by the affected slice.

```bash
PLAYWRIGHT_PACKAGE="$PWD/tests/Alis.Reactive.Conformance.PlaywrightTests/bin/Release/net10.0/.playwright/package" \
  /usr/bin/lockf "$audit_lock" caffeinate -i node spec/FusionComponents/audit/probe-combo-box.mjs > .landing/FusionAudit/probe-combo-box.json
cmp spec/FusionComponents/audit/probe-combo-box.json .landing/FusionAudit/probe-combo-box.json
```

At this revision MSBuild returns an empty `VendorContractTsConfig` on both Fusion hosts. ALIS016 starts from a C# operation and explicitly skips events in its ordinary member check. Neither can discover a vendor member that no C# operation mentions. The existing event-args probe independently lists all five FilteringEventArgs members. The stock whole-program contract extractor exits at `vendor-contract.mjs:148` on the dropdowns program: a synthesized TypeScript symbol has neither a value declaration nor a declarations array, and `containsFree` reads `member.declarations[0]`. The audit uses a source-backed walk of the compiler's symbols and makes no change to the production extractor. [Extractor](../../../Alis.Reactive.Analyzers/Vendors/vendor-contract.mjs) · [ALIS016](../../../Alis.Reactive.Analyzers/Vendors/ALIS016PublicMemberExists.cs)

```bash
for framework in net10.0 net48; do
  /usr/bin/lockf "$audit_lock" caffeinate -i dotnet msbuild Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj -nologo -p:TargetFramework="$framework" -getProperty:VendorContractTsConfig < /dev/null
done
/usr/bin/lockf "$audit_lock" caffeinate -i node spec/probes/ej2-event-args.mjs < /dev/null
/usr/bin/lockf "$audit_lock" caffeinate -i node Alis.Reactive.Analyzers/Vendors/vendor-contract.mjs spec/FusionComponents/audit/dropdowns.tsconfig.json .landing/FusionAudit/vendor.tsv < /dev/null
```
