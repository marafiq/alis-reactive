# Archived validation ownership proposal

Superseded by the [browser validation follow-up](browser-validation-proposal.md). The candidate and replay details below are historical. The browser replay paths under `.work/validation-browser-proof` refer to disposable local artifacts. The TypeScript reproduction is preserved below.

Different routes or views with different validation rules get their own model and validator types. The
framework derives the validator key from those types. The probe below reproduces a shared-key collision.
A framework change needs a failing case that follows the typed-model rule.
[View model rule](../../.claude/rules/views.md) · [Validation contract](../Core/DESIGN.md#q16-validation)

A bulk edit is one array in one request container. Every item uses the same row validator. The index tells
the framework which field receives an error. The North and South example below concerns independently
rendered editor plans.

## What goes wrong on screen?

The probe renders the same charge view twice. It shares one C# model and validator across two different
amount fields and rule configurations. The server emits the minimum for each wing.

| Editor | Minimum | Result for an amount of 20 |
| --- | ---: | --- |
| North | 5 | Accept |
| South | 30 | Show “South requires at least 30.” |

I reproduced this in Chromium. Open North first, then enter **20 in South**. South shows no field error and
**Stage** accepts the charge. **Save** sends it and the server rejects it with HTTP 400. Close North and click
**Stage** again. The same South value now shows “South requires at least 30.”
[Browser results](../../.work/validation-browser-proof/browser.log)

## Why do the shared registrations collide?

Both plans already carry their own rules. The browser puts them in one page-wide list indexed by validator
name and rule set. North and South have the same key, so the browser takes whichever entry arrived first.
Distinct validator types get distinct keys, and the existing TypeScript control passes for those keys.
[Distinct-key control](../../Alis.Reactive.Assets/runtime/Validation/forms.test.ts#L228) · [Shared-key reproduction](#typescript-reproduction)

## What is the candidate fix if a supported case needs it?

**Look up the rules inside the right plan.** When South's field changes or you call
`Validate(southContainer)`, the browser uses the plan that rendered South's container. An HTTP request uses
the rules declared by the plan that wrote that request. This matters when a page action sends inputs from a
different plan's container. The container supplies the inputs and the place to show errors. The request
supplies the rules for its own endpoint model.
[Existing contract, D13.1 and D16.2](../Core/DESIGN.md#q16-validation) · [Current lookup](../../Alis.Reactive.Assets/runtime/Validation/forms.ts)

Inside the framework, keep a single lookup by the actual running plan, validator, and rule set. For a
standalone check, the C# plan passes the container's existing input capture to Validation. The browser
keeps the source plan with that capture. Live checks already know which container they came from. The
request check already names its rules in the sending plan. Closing a plan removes its own registrations.
This works even when two plans share a tracing ID. Ship the C# change and its browser bundle together.
[Standalone check](../../Alis.Reactive/Validation/FormChecks.cs) ·
[Request check](../../Alis.Reactive/Validation/RequestChecks.cs) ·
[Container capture](../../Alis.Reactive.Assets/runtime/InputsAndIds/model.ts)

## Why keep the current C# call?

I opened a real charge editor through a typed row action and bound its JavaScript-owned amount as a typed
`InputField` inside a request container. This was enough to validate the live value.

```csharp
pipeline.Validate(edit.Container);
```

- **One row.** Rose's current edit of 0 failed even though her posted row still showed 40. Correcting the edit to
  25 staged 25. A planted read of the old 40 made the expected error disappear and failed the proof.
- **One array, one row validator.** The batch applies the same `ChargeDraftRules` to every index.

  ```csharp
  RuleForEach(batch => batch.Rows).SetValidator(new ChargeDraftRules());
  ```

  I marked both rows as paid and set both amounts to 0. The same error appeared beside `Rows[0].Amount`
  and `Rows[1].Amount`. Correcting row 0 cleared only its error, and row 1 still blocked the batch.
  Correcting row 1 staged Rose 25 and Ada 40 together. In a separate run, Ada's due status made her 0
  valid under the same conditional rule.

The component exposes its value and commit event once. The application uses the same container call for one
row or several. This proof covers typed registered inputs. An unbound result exposed only by a widget is a
separate question. Syncfusion's Query Builder uses conditions to match records, and Form Renderer uses them
to control fields and check entries. Each keeps those rules in its own component. Alis.Reactive keeps its
rules in its C# validators and its inputs and errors in the existing containers.
[Typed input contract](../../Alis.Reactive/InputsAndIds/ComponentInput.cs) · [Probe views](../../.work/validation-browser-proof/Views/Charge) ·
[Query Builder audit](https://github.com/marafiq/alis-reactive/blob/feature/fusion-query-builder/spec/FusionComponents/audit/query-builder.md) ·
[Form Builder audit](https://github.com/marafiq/alis-reactive/blob/feature/fusion-form-builder/spec/FusionComponents/audit/form-design.md)

## What proof would earn the fix?

First reproduce the failure with a reusable editor that follows the typed-model rule. The checks below
apply if that case establishes the need.

1. Open both editors in either order. North accepts 20. South shows its own error at 20 and blocks Stage and
   Save. Closing or replacing North leaves South's result the same. South accepts 35. Restoring the current
   first-entry lookup must make this test fail.
2. Have a North page action call `Validate(southContainer)` with 20. It must show South's error. Have a
   separate North request send that same 20 from the same container. It must run the rules declared by the
   North request and send 20. This catches fixes that always choose the caller or always choose the container.
3. Check that two plans sharing a tracing ID still get their own rules. Keep the existing browser tests that
   check nested containers, row errors after a slot changes, and batch saves.

The current behavior above is measured in .NET 10 Chromium. The candidate stays parked pending a supported
reproduction. Before landing an approved fix, model the two live registrations in `Validation.tla` and run
the mutant that picks the first match. Then run
the failing browser cases, implement the fix, and run the established serial gates. MVC 5 browser verification
follows the Windows lane. [Ownership models](../OwnershipAndState/Ownership.tla) · [Validation model](Validation.tla)

## How can I replay the browser proof?

Start the built probe app from `.work/validation-browser-proof` with the pinned .NET SDK.

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet bin/Release/net10.0/ChargeProof.dll --urls http://127.0.0.1:5293
```

From the rewrite checkout, with `scripts/toolchain-env.sh` sourced, run:

```bash
/usr/bin/lockf -k "$HOME/.local/share/alis-toolchain/build.lock" node .work/validation-browser-proof/browser.cjs row batch indices owner
```

[Probe](../../.work/validation-browser-proof/browser.cjs) · [Planted stale read](../../.work/validation-browser-proof/stale-plant.log)

## TypeScript reproduction

This is a temporary failing probe, preserved out of the test suite. Paste it inside the validation tests in [forms.test.ts](../../Alis.Reactive.Assets/runtime/Validation/forms.test.ts#L239), then run the command below with the pinned toolchain. With both plans live, `withNorthOpen` is empty. Releasing North makes `afterNorthCloses` report South's error.

```ts
  test("a South check uses its own charge rules while North's plan is open", () => {
    const charges = (minimum: number, message: string): Registration => ({
      validator: "ChargeRules", ruleSet: "default", start: { amount: 0 },
      scalars: [], objects: [], lists: [],
      rules: group([field(["amount"], message, fn(value => typeof value === "number" && value >= minimum))]),
    });
    const north = new Visit();
    const page = new PageForms(new InputsTable({}, { south: "south_summary" }), new Page());
    page.join({ ...plan([charges(5, "North requires at least 5.")]), planId: "North" }, north, read);
    page.join({ ...plan([charges(30, "South requires at least 30.")]), planId: "South" }, new Visit(), read);
    const southCheck = check("ChargeRules", { amount: 20 }, [sent("Amount", ["amount"])],
      "urlEncoded", { destination: { kind: "summary", containerId: "south" } });

    const withNorthOpen = page.validate(southCheck);
    north.release();
    const afterNorthCloses = page.validate(southCheck);

    expect({ withNorthOpen, afterNorthCloses }).toStrictEqual({
      withNorthOpen: [{ name: "Amount", message: "South requires at least 30." }],
      afterNorthCloses: [{ name: "Amount", message: "South requires at least 30." }],
    });
  });
```

```bash
npx vitest run Alis.Reactive.Assets/runtime/Validation/forms.test.ts -t "a South check uses its own charge rules while North's plan is open"
```
