# Browser validation follow-up

Status: documentation approved in the read-only architect review at `901319ae9`. Implementation is parked.

## What I propose

Use the existing typed model, validator, and request container. When two routes or views have different
validation rules, give each its own model and validator type. For a bulk save, use one array in one
request container and apply the same row validator to every item.

If a supported case still reproduces the shared-key collision, fix the browser lookup internally.
`Validate(container)` and live validation use the rules emitted by the plan that rendered that container.
An HTTP request uses the rules emitted by the plan that declared the request. The C# calls stay the same.
The lookup uses the running plan contribution, validator and rule set. The contribution is the instance
that supplied the rules, even when plans share an owner or tracing ID.

## Why

Different validator types already get different keys. The browser probe showed that one batch validator
can report errors at `Rows[0].Amount` and `Rows[1].Amount` independently. Fixing one row clears its error
while the other can still block the batch.

The North/South probe reused one validator key for two different policies. South's amount of 20 passed
North's minimum of 5 while North was open, then failed South's minimum of 30 after North closed.
The browser takes the first registration matching the key. The reproduction conflicts with the
typed-model rule above. A supported reuse case must reproduce the failure before a change goes ahead.
[Current lookup](../../Alis.Reactive.Assets/runtime/Validation/forms.ts#L65) ·
[Archived proposal and reproduction](browser-validation-proposal.archived.md)

## Samples

**Two views with different rules.** The North view opens `Html.ReactivePlan<NorthEdit>()` with
`NorthEditRules : ReactiveValidator<NorthEdit>`. The South view opens `Html.ReactivePlan<SouthEdit>()`
with `SouthEditRules : ReactiveValidator<SouthEdit>`. Each view validates its own typed container.

```csharp
pipeline.Validate(edit.Container);
```

**One bulk array.** The batch validator applies the same rules to each row. When the user clicks
**Save all**, the existing validation call checks the whole array.

```csharp
RuleForEach(batch => batch.Rows).SetValidator(new ChargeDraftRules());
```

```csharp
pipeline.Validate(batch.Container);
```

## What would reopen the fix?

Demonstrate a supported typed use case with two simultaneous registrations sharing a key. If it
reproduces the collision, prove that closing or replacing one editor leaves the other's verdict stable.
For a cross-plan check, let North validate South's container as a standalone action and show South's
error for 20. Have a separate North request send the same input and accept 20 under North's rules.
Restoring the first-registration lookup must fail the proof. Keep the indexed-array, nested-container
and partial replacement cases as controls.

The architect approved recording the existing patterns. The review did not approve a change to the
lookup under the owner's no-TLA+-change condition. `OnlyTheNamedValidator` checks validator identity,
and `RulesLeaveWithTheirOwner` checks release. Neither distinguishes two live contributions with
the same validator and rule-set key. Return to the owner with the supported reproduction and that
proof obligation before implementation.
[Validation model](Validation.tla) · [Ownership model](../OwnershipAndState/Ownership.tla)
