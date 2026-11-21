# What does the admission Form Builder publish?

The .NET 10 admission designer loads the official Syncfusion ASP.NET Core Form Builder and Form Renderer 35.2.12. A coordinator can save a nurse requirement to the staff form and reopen a device draft with an edited transfer-note visibility rule. Only the requirement goes to the application server. [Open the designer](http://localhost:5292/care/admission-clinical/form-design)

## How do I try the two edits?

1. Select the **Receiving nurse** field on the canvas. A focused field also opens with Enter. Open **Validation** and toggle **Required** with Space. After “Receiving nurse is required.” appears, open **Preview saved form** and choose **Submit admission**. The blank field displays “Enter the receiving nurse.” Enter a nurse and submit again. The staff page displays “Hospital admission passed.”
2. From the designer select **Transfer notes**, **Conditions**, then **Edit Conditional Visibility**. Replace Hospital with Planned, press Tab and choose **Save**. Open **Reopen device draft**, then **Preview form** using Enter. Hospital hides Transfer notes. Change Arrival route to Planned and leave the field to show Transfer notes. Respite hides it again.

After the focused browser run has built the app, start it from the worktree with the following command. A fresh browser navigation creates its own conformance world.

```bash
. scripts/toolchain-env.sh
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --no-restore -c Release --project tests/Alis.Reactive.Conformance.App -- --host net10 --urls http://127.0.0.1:5292
```

## Which packages supply the controls?

The common Fusion pin is 33.1.47. The NuGet releases for the separate Form Builder start at 35.1.37 and the Form Renderer releases start at 34.2.2. The admission app downloads the official 35.2.12 assets and imports each package's static-web-asset manifest. The Form Builder wrapper at `staticwebassets/scripts/sf-form-builder.min.js:73–75` loads its own dependency URLs under `/_content/Syncfusion.AspNetCore.*/scripts/`. The browser served 50 distinct scripts, including the entry point, from 18 packages. The Bootstrap 5.3 stylesheet comes from a nineteenth package. The staff page loads the exact-version `sf-form-renderer.min.js` entry point. [Form Builder package](https://www.nuget.org/packages/Syncfusion.AspNetCore.FormBuilder/35.2.12) · [Form Renderer package](https://www.nuget.org/packages/Syncfusion.AspNetCore.FormRenderer/35.2.12)

The shared managed lock still resolves Syncfusion.Licensing 33.1.47 and Newtonsoft.Json 13.0.3. Direct managed references to the 35.2.12 controls brought transitive managed dependencies into that graph. NuGet records `PackageDownload` under `downloadDependencies` in the app's restored assets file. Its ordinary project lock omits these entries. The vendor probe verifies every downloaded NUPKG's SHA-512 against the companion 19-package content lock and checks the project declares the same versions. [Asset declaration](../../../tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/FormDesignAssets.props) · [Package content lock](form-design-packages.json) · [Managed lock](../../../tests/Alis.Reactive.Conformance.App/packages.lock.json)

## Which identities does the schema use?

Both controls accept the same unified `Schema`. It has a string `version`, a keyed map `properties`, an ordered `layout` tree and optional `settings`. The example uses version `0.1.0`. Layout nodes can be fields, tables, panels, tabs or cards. A field node's `propertyId` references a key in `properties`. Field definitions carry a semantic data type, a widget, a label, a field name and settings including requiredness and conditions. The exporter generates unique semantic keys from labels. A label edit can therefore affect an exported layout reference. [Schema model](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/form-renderer/types/form-schema.d.ts) · [Field and layout types, lines 32–88 and 679–692](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/common/json-converter.d.ts) · [Converter, lines 354–385](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/common/json-converter.js)

| In the sample | Where the value travels |
|---|---|
| `receivingNurse` | The definition key and layout's `propertyId` |
| `handover-field` | The nurse's component ID, used internally to hold its current answer |
| `handover` | The name in the editor's change event and the renderer's submitted answers |
| `Receiving nurse` | The label the coordinator and staff see |
| `route-field` | The arrival component ID used as the condition leaf's `field` |
| `arrivalRoute` | The arrival field's submitted name |

The per-member inventory covers 37 public instance names at Form Builder, 35 at Form Renderer, and the declared fields of eight event payloads. Each row gives the exact declaration and line and its admission use or exclusion. EJ2 adds a `name` to observed events. The Builder payloads extend the vendor's `@private BaseEventArgs` in `@syncfusion/ej2-base/src/event-handler.d.ts:65–69`. The raw transcript records the actual names. The mount helper also disposes each widget through its lifecycle method. The package annotates the component’s override of `destroy` as private. Drag events, custom buttons, `getComponent(name): object | null` and `setFieldValue(name, value): void` have no admission view door. [Per-member crosswalk](form-design-members.tsv) · [Recorded observations](form-design-observations.json)

## What can the application save?

The Form Builder's `onFieldPropertyChanged` callback emits `fieldName: "handover"`, `propertyName: "required"`, and a boolean `propertyValue` for this switch. The probe saw both false and true. An ordinary `addEventListener` alone heard zero changes because the control checks its callback property before triggering. The C# view discriminates both field and property, then posts the typed value. The endpoint checks its field name and presence and records the flag in the current conformance world. Each request to the separate staff form constructs a new renderer from that saved flag. [Builder, lines 369–376](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/form-builder.js) · [C# view](../../../tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/FormDesign.cshtml) · [Endpoint and admission schema](../../../tests/Alis.Reactive.Conformance.App/Care/AdmissionClinical/FormDesignController.cs)

Editing a condition calls the editor's save handler and emits no field-property change in this interaction. The public `schema` still reports the original input after edits. The exported JSON contained the new boolean and predicate. The device restores its edit when the designer mounts with the same ID, persistence enabled and the input schema omitted. The public `schema` then reads null while the stock preview uses the edited rule. The private editing tree and the persistence envelope are outside the typed C# surface. A user can export their full schema through the vendor's **Export** button. [Right pane, lines 435–455](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/components/layout/right-pane.js) · [Builder, lines 112–135 and 1117–1177](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/form-builder.js) · [Public and private declarations, lines 162–200 and 283–300](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/form-builder.d.ts)

## How is a visibility rule authored and executed?

The condition dialog creates a real EJ2 QueryBuilder. The columns use the component ID first. The editor takes `getValidRules(qb.rule)` and assigns the result to the selected action. A condition can be a leaf or a recursive `and` or `or` group. A leaf carries `field`, `operator` and `value`, plus optional `label` and `type`. The declared value allows object, string, number, boolean or null. The recorded export contains one `and` group with `{field:"route-field", label:"Arrival route", operator:"equal", type:"string", value:"Planned"}`. [Columns, lines 164–177](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/utils/conditional-editor-utils.js) · [QueryBuilder and valid rules, lines 563–574 and 683–703](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/utils/conditions-editor.js) · [Condition types, lines 114–198](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/form-renderer/types/form-schema.d.ts)

The renderer evaluates `visibleWhen`, `hideWhen`, `disabledWhen`, `readOnlyWhen` and `requiredWhen` against the current form values by component ID. `setValueWhen` carries a condition and a separate assigned value. Group evaluation recurses and string equality compares strictly. Dependent field changes schedule a rerender. The designer preview creates a real FormRenderer. The admission test covers conditional visibility across three values. [Predicate engine, lines 26–40, 63–82 and 210–237](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/common/conditional-rule-engine.js) · [Renderer, lines 1459–1468](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/form-renderer/form-renderer.js) · [Preview, lines 240–266](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/components/layout/preview-pane.js)

## Where do validation and submission run?

| Mechanism | Behavior in 35.2.12 |
|---|---|
| Built-in required | The browser computes static or conditional requiredness. For an eligible, visible, enabled, editable field it registers the EJ2 required rule with its `validationMessage` or the label followed by “ is required”. The independent staff renderer refused an empty nurse and accepted a completed answer. |
| Custom field validation | The Builder's visual custom-validation editor emits an expression inside a rule array. Entries have `expression` and optional authoring `condition` metadata. The validation engine parses the `valid = condition ? true : 'message'` form, supplies `input` and the current form values, and runs the expression through its own evaluator. The ordinary renderer checks each expression, uses the first expression's captured message, then the field's message or a label-based fallback. These are vendor source observations. |
| Embedded Grid column validation | The Renderer Grid adapter collects current editable row values by column field, converts numeric fields and evaluates the first rule with an expression. Its loop returns that first verdict. Later expressions in the array are skipped by this adapter. This is vendor source evidence. |
| Submit | EJ2 FormValidator checks before invoking the form callback. An invalid blank submission displayed the validation message and emitted zero submit events. A valid sample emitted `data: {arrivalRoute:"Hospital", handover:"Elena Ruiz"}` and `isValid: true`. A previously answered transfer note that became hidden suppressed the entire next submit event. The loop in `handleSubmitData` returns on that excluded key. |
| Server boundary | The submission preview displays its result through typed C#. It does not post admission answers. The only POST carries the nurse requirement. The browser predicates and vendor validation expressions do not run as application server validators. |

The required-plus-visibility combination has a falsifying case. When a required nurse is initially visible because the arrival default is Hospital, a new renderer accepted the blank visible nurse. The validation rules are collected before the initial values are copied into form state. The demo keeps the coordinator's unconditional nurse requirement and transfer-note visibility on their respective fields. [Renderer, lines 183–205, 476–550, 655–666 and 2101–2146](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/form-renderer/form-renderer.js) · [Custom expression engine, lines 34–63](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/common/custom-validation-engine.js) · [Validation editor, lines 180–185](https://unpkg.com/@syncfusion/ej2-form-builder@35.2.12/src/form-builder/utils/custom-validation-editor.js) · [Grid adapter, lines 46–73](https://unpkg.com/@syncfusion/ej2-form-renderer@35.2.12/src/form-renderer/common/datagrid-component.js)

## How do I replay the evidence?

Run these commands at the worktree root. The inventory extracts integrity-checked npm source at `obj/form-design-inventory`. The browser probe uses the actual restored NuGet JavaScript, gestures, export download, device persistence and renderer events. It records vendor observations separately from the two typed C# workflow tests.

```bash
. scripts/toolchain-env.sh
form_audit_lock="$HOME/.local/share/alis-toolchain/build.lock"
mkdir -p obj/form-design-inventory
node spec/FusionComponents/audit/form-design-inventory.mjs > obj/form-design-inventory/members.tsv
cmp spec/FusionComponents/audit/form-design-members.tsv obj/form-design-inventory/members.tsv
/usr/bin/lockf -k "$form_audit_lock" caffeinate -i scripts/playwright.sh --screen admission-form-design < /dev/null
/usr/bin/lockf -k "$form_audit_lock" caffeinate -i node spec/probes/ej2-form-design.mjs > obj/form-design-inventory/observations.json < /dev/null
cmp spec/FusionComponents/audit/form-design-observations.json obj/form-design-inventory/observations.json
```
