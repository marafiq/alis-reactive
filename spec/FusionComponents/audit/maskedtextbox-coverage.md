# MaskedTextBox on the next-of-kin contact page

The phone field uses the existing EJ2 mask `000-000-0000`. Staff can prepare a contact slip for a family call or a digits-only entry for the dialer. The page reads the live editor when the person clicks the relevant button.

| EJ2 33.1.47 contract | Typed operation | Care-page evidence |
|---|---|---|
| `getMaskedValue(): string` in `@syncfusion/ej2-inputs/src/maskedtextbox/maskedtextbox/maskedtextbox.d.ts:278` | `FusionMaskedTextBox.GetMaskedValue()` returns `MethodResult<string>` on net10.0 and net48. The public method at `maskedtextbox.js:519` reads the input’s displayed value through `base/mask-base.js:212`. | M21.X2000 changes the saved home number in the editor. Preparing the contact slip shows `212-555-0146`. |
| `value: string` with a documented null default at `maskedtextbox.d.ts:137–144` | Existing `FusionMaskedTextBox.Value` read as `Property<string?>`. | M21.X2000 prepares the mobile contact for the dialer and shows `5559876543`. M21.X1016 retains the existing bound-value case for the home and mobile routes. |

The pinned inventory lists the root and three event payloads in [slice-rows.tsv](slice-rows.tsv). The following candidate forms still need evidence.

| Vendor form | Typed path checked and missing evidence |
|---|---|
| `width: number \| string` at `maskedtextbox.d.ts:58` | The slice has no live width accessor. Pixel and CSS-length representations need separate observed uses. |
| `customCharacters: { [x: string]: Object }` at `maskedtextbox.d.ts:156` | The slice has no typed character map. The validator calls `split(',')` on a configured map value in `base/mask-base.js`. The live write and null reset still need a representative use. |
| `prependTemplate` and `appendTemplate`, both `string \| Function`, at `maskedtextbox.d.ts:169,180` | The slice has no corresponding template members. The string and function branches require evidence of their inputs and visible rendering. |
| `change.maskedValue?: string` at `maskedtextbox.d.ts:304`, and the focus and blur payloads at lines 319–341 | `MaskChangeEventArgs` currently exposes `Value` and `IsInteracted`. The page reads the current editor on demand. Callback values and the optional event unions need their own gesture. |

EJ2 `created` follows the Fusion-wide exclusion. The remaining candidates keep their pinned inventory placements.

## Browser evidence

The unwired contact slip displayed `No contact slip prepared`. Reading the raw property for the slip failed at `2125550146` where `212-555-0146` was expected. Calling the display method for the dialer failed at `555-987-6543` where `5559876543` was expected. Each substitution compiled and was restored. The final M21.F25 selection on net10 passed both cases with zero over budget.

Brief to the affected feature passing on the final gesture took 7m53s. Both planted defects and the restored feature finished at 19:14:39 EST, 10m47s from the brief.

Replacing the contact-slip read with the raw value produced `2125550146`. Replacing the dialer read with the formatted value produced `555-987-6543`. After restoring the page, both M21.F25 net10 cases passed. The two assertion failures and the restored run took 10m47s from assignment.
