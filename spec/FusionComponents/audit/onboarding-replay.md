# Replay the component onboarding skill

A fresh reader followed the revised skill from 27139f914 with the npm-pinned TypeScript and JavaScript. This
checkout precedes the new examples. A separate comparison used the integrated declarations and both API
manifests. The tests remain in each example's fixture.

## What did the skill reveal?

The original revision discussed emitted nullable payloads and omitted the documented default of a public
member. EJ2 declares ComboBox.query as Query and documents a null default. NumericTextBox.value is declared
number with the same default. The revised skill now asks for documented defaults as well as observed absence
and nullability. Both are required members.

The fresh browser probe found both defaults null. It rendered NumericTextBox, edited it to 17 and cleared
it. The two change events held current and previous values of (17, null) and (null, 17). The same comparison
found the public getText method absent in Fusion and excluded the private isInteraction field. On ComboBox's
first selection the previous row and element were null. Clearing made the current row and element null,
kept the prior selection and emitted event and e with undefined. The cancel key was absent in these changes.

The raw browser replay uses a loopback HTTP origin and checks for browser errors. A supplemental rehearsal
on about:blank hit EJ2's invalid origin error. A matching JSON shape alone did not catch that defect.
[Replay script](../../probes/ej2-combo-box-onboarding-presence.mjs) ·
[Captured result](../../probes/ej2-combo-box-onboarding-presence.json)

The baseline inventory resolved 52 EJ2 roots. For each of 832 C# declarations it found a matching signature in
both host manifests. The vendor-to-C# comparison still named ComboBox's absent inherited dataSource,
change.itemData and filtering.updateData. MultiSelect's same-named dataSource was present. The baseline's
present change.value was found and suppressing that entry failed with the exact vendor path.

## Which revised doors were verified?

| Example | Declaration checks | Typed browser proof |
|---|---|---|
| Four values | Required nullable current and previous row reads. A nullable HTMLLIElement holder for item. | The row supplies a booking team and the DOM item supplies its formatted label. Distinct branches change the guidance and both panels. |
| Interpreter search | Filtering and its own payload. Synchronous updateData, showSpinner and hideSpinner. | Held requests expose the spinner. Only the active query updates the list. Success and refusal clear the spinner. |
| Live catalogue | The local array write through dataSource and optional FieldSettings.text/groupBy. | Actual weekend choices and shift headings replace weekdays and the cleared choice changes the submitted result. |
| Equipment query | A nullable live Query. Its execute uses MethodAsync and reads the typed result array. | A held vendor request leaves Checking stock visible. After release the ready row's nested dispatch instruction appears. |
| Supplier DataManager | The manager view of dataSource and the scoped executeQuery promise method with a typed Query source. | Each chosen supplier's real Query crosses into the vendor method. The held response releases a distinct delivery window. |

The current comparison accepted the listed operations on both hosts, including the manager read and
executeQuery. Deleting DataSource from C# and both manifests made the staged source comparison fail at
FusionComboBox.dataSource. Replacing MethodAsync with Method
kept the name and API signature and failed at DataQuery.execute. Both changes were restored byte for byte. The
equipment fixture independently caught the synchronous substitution and the wrong row predicate through
visible outcomes.

## Which cases remain separate?

The declarations cover the lanes exercised by the views. The real row type from a catalogue selection differs
from the declared FieldSettingsModel. Custom values, primitive-array row wrappers and a missing previous row
have their own evidence. Filtering.updateData also accepts a DataManager and optional query and fields.
Those argument forms and filtering event native data need their own typed proof. The replay neither infers
behavior from inventory counts nor shares one component's declarations with another.
