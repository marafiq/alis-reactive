# Full component API audit and proof

## Which version and object are in scope?

Read the version from the lock and the installed package. Read the public declaration and its implementation at
that pin. When a server helper renders options, also verify its package version and the output it actually renders.
Official examples identify supported interactions. Use the installed source for their exact signatures and behavior.
For an app-owned object, start from the code that constructs the instance and the public contract its callers use.

Identify how Reactive finds the live object, attaches a listener or callback, and releases it. Inventory its own
and inherited public instance members. Use the compiler's resolved symbols where declarations are available.
Follow methods, property reads and writes, callbacks and events through their arguments, payloads and returns.
Follow an object again under a different use site when the type arguments, available fields or lifetime differ.
Reuse a known shape for cycles and retain the calling path in the record.

Keep the render options in a separate part of the record. An option used at construction says what initial markup
and data are available. A public live member with the same name still needs its own read or write evidence.
The same applies to vendor UI gestures. A toolbar action or keyboard gesture can have a real effect even if it
has no matching public method.

## How do I find the gaps in a Fusion slice?

Run the [per-operation inventory](../../../../spec/FusionComponents/audit/README.md#which-call-forms-and-nested-objects-does-a-slice-expose)
against the installed package. Start a new slice with `@syncfusion/package:Export`, then replace that target
with its Fusion slice name after the typed declaration exists. Read the package's actual version rather than
assuming it matches the umbrella bundle. Read the root object and follow the edges for arguments, returns,
event payloads and nested properties. Scope a C# holder to the vendor type and the use site that reaches it. Compare each read,
write and emitted call arity with the vendor's full signature, then open both source locations to verify types
and defaults. Search the C# declarations list for aliases and fixed arguments the vendor row does not explain.

Grid's `selectCellsByRange` has one- and two-argument forms. Its coordinate and the result of
`getSelectedRowCellIndexes` reach different interfaces, even though EJ2 marks both type names hidden.
ComboBox's filtering `updateData` accepts a data source plus optional query and fields. Its C# array argument
leaves the additional arities and the DataManager alternative open. These examples show where a name match
leaves work. Use the same coverage record for app-owned components.

## What belongs in the coverage record?

Keep the coverage and evidence with the component's spec. Use the following fields, in the repository's existing
format. The unit of coverage is a public path and a specific read, write, call signature, or payload variant.

| Field | What to record |
|---|---|
| Vendor contract | Exact package and version, object path, JavaScript name, declaring source line and public access |
| Shape | Every supported overload, omitted argument and vendor default, value and return types, getter or setter, event or callback, optionality and nullability |
| Runtime evidence | The gesture, data or module state that produced the shape and any case where it differs |
| Typed declaration | C# symbol, operation kind, JSON names, argument or result types and matching lines in every shipped host manifest |
| Browser proof | The conformance page, real action, precise effect, failing planted defect and restored result |
| Verdict | Verified, open with the specific evidence still needed, or excluded with its public contract or owner decision |

A matched name and a compiled declaration each count as their own evidence. Split a row where a writable
property, optional argument, async return, overload or alternative payload gives the caller distinct behavior.
Share a browser case when it exercises every cited form, and name the forms it proves. For an app-owned slice,
check its actual C# source and app build. Package API manifests apply to shipped packages.

Reconcile the record in both directions. Match each vendor member to its C# operations, and account for each
public C# operation as a vendor member, a render option, or a deliberate projection. Report aliases and fixed
arguments at the signature where they matter.

An audit accounts for every row, including open rows. Full onboarding verifies every eligible row and records
every exclusion with its reason and source. Keep open rows in the handoff and report root and reachable payload
coverage separately. On an upgrade, replay the inventory, compare signatures and source, and rerun the proofs
for changed contracts. Preserve existing C# callers and their behavior.

## How do I prepare the cases before the first build?

Carry forward landed evidence for the same pinned contract, typed path, host and named behavior after reading
its proof and defect. For example, [Switch coverage](../../../../spec/FusionComponents/audit/switch-coverage.md)
credits M21.X1027 for initial checked state and keeps the live variants separate. Reuse a read-only vendor
audit at the same pin after confirming the source and any changes to the slice at the assigned base.

Take the new rows to the lead together. For each row name the actual view and gesture, the exact visible result
or server receipt, its opposite on the same path, and the compiling substitution for each defect. Find a landed
locator and assertion form before writing the test. The Switch pilot lost a build to locator and assertion
members refused by TEST002 and a computed accessible name refused by TEST007. Use the app's accessible names and
literal assertions through the permitted API. The lead places the promises and owns the generated files. The
builder then takes the new cases from expected red to green, runs their own plants and links the restored run.

The Switch pilot took about 62 minutes from brief to push, about 37 of them in proof and landing checks. Compare
the next wave's brief-to-push and check times using the existing logs. Look for duplicate discovery and time
spent waiting for the build lane between checks.

## Which runtime details change the declaration?

- Check omitted values, explicit null, false, zero and empty collections independently. For weak data, trace the
  construction or consumption site and observe representative real gestures. Keep the observation alongside the
  typed test. A vendor event name can carry different data in different modes.
- Preserve argument order, vendor defaults, accepted value types and whether a real live object reaches the call.
  Distinguish data the browser can serialize from a vendor instance with methods and identity.
- Use the actual return contract. A method that starts work and returns void is a synchronous call. A method that
  returns a Promise uses the framework's async call. Read which outcome that Promise settles and prove that outcome
  before using it to order subsequent UI work.
  A data mutation and its rendered result can happen at different points. When a consumer does both, prove
  the read of the actual data and the visible result separately. EJ2 Grid exports and deferred batch updates
  give examples of each ordering.
- Identify whether the vendor consumes a callback's return. A cancellation or a callback answer that the vendor
  consumes in the same turn needs synchronous proof. Keep post-await steps separate from those answers.
- Record public nested shapes used by a public call even if the vendor marks their type names hidden in its docs.
  Include the supported fields needed by that call. Evaluate a hidden or private method itself under the owner's
  existing boundary. Preserve typed paths and expose gaps requiring a framework decision with their real use case.

## How do I prove the order of an async effect?

Follow each overload from the typed call through the vendor implementation to the effect the promise names.
Record what settles the returned Promise, what the vendor's completion event carries and when the browser takes
the action. In EJ2 33.1.47, Grid's default XLSX export can resolve while archive work is still pending. Its
default CSV and PDF saves dispatch a download link during the save call in Chromium. A filename and a status read after Playwright receives a
download notification establish the file and the later status. To prove the status at initiation, find an
observable boundary at initiation or a named action the harness can hold. Keep the ordering case open when
that evidence is missing.

Read the planted call through the current framework before using it to prove an ordering. Under D11.3, declaring
a Promise-returning method with `Method` stops the pipeline. A failure because its final status never appears
establishes that stop. A case about a final status appearing too early needs evidence of that order.

## How do the proofs expose different kinds of gaps?

| Reference | What to replay |
|---|---|
| [Fusion pinned inventory](../../../../spec/FusionComponents/audit/README.md) | Reuse the installed-package inventory and compare its named rows with the live declarations and .NET target manifests. Use the name table for orientation and the operation inventory to resolve each signature, access direction and reachable shape. |
| [ComboBox onboarding replay](../../../../spec/FusionComponents/audit/onboarding-replay.md) | Follow nested current and previous records, null and absent keys, configured Query identity, and the vendor Promise. Read the pinned probes alongside the typed view and browser cases. |
| [Conformance component render helper](../../../../tests/Alis.Reactive.Conformance.App/Components/ConformanceComponent/ConformanceComponentHtml.cs) and [component lifecycle](../../../../tests/Alis.Reactive.Conformance.App/Components/component-view.ts) | Follow an app-owned component from typed initial options to its live instance, listeners, and teardown. |
| [Native input reference](../../../../spec/Reference/native.json) | Trace a browser-owned input through a real rendered field, user gesture and observable outcome. |
| [Grid charge corrections](../../../../tests/Alis.Reactive.Conformance.App/Care/BedBoard/ChargeCorrections.cshtml) and [its browser test](../../../../tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionGrid/WhenAGridsEditedRowsAreSaved.cs) | Follow the live changed-record list into the typed request. The test observes an actual changed date and amount and the empty batch in a fresh world. |

Use [build-a-screen](../../build-a-screen/SKILL.md) to build a realistic view and
[prove-behavior](../../prove-behavior/SKILL.md) to verify the failure each case can catch. Read the existing
checks and quality gates in [Rewrite orientation](../../../../spec/REWRITE.md) and
[the framework design](../../../../spec/Core/DESIGN.md). Run builds serially and name the host that ran each
browser proof.

## What does a clipboard proof need?

Grant clipboard read permission in a fresh browser context for the case that uses it. Use the browser's copy
gesture on visible source text. Keep that source outside any form the component scans for its target fields.
Check the pinned implementation for the way it identifies those fields. Smart Paste reads a normal input's id
before its name and skips a field with no description.

For a protected request, check the host's antiforgery setup and make the endpoint validate the request. Have the
endpoint answer from the copied text and named fields. Prove the visible change and plant the omitted header.
For a button in a slot, open the page before loading the form and plant an initial-document-only mount.
The [Smart Paste browser cases](../../../../tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionSmartPasteButton/WhenStaffPasteAResidentReferral.cs)
exercise both paths.
