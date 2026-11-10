# Validation

For the validator author: a rule written once on `ReactiveValidator<T>` in FluentValidation's grammar, registered
through dependency injection, which the server runs and from which C# derives the browser's rule where the view
renders, so the two give one verdict on the body each request sends; and where each failure shows. Module 8 of
`spec/CLAUDE.md`.

## Core questions

- Q16: a rule is written once (D16.1) and a check is one validator under one rule set (D16.2). The browser decides
  on the body its request sends, a member it lacks read at the start the endpoint's binder leaves (D16.5, D16.6). A
  text its member's type cannot hold fails first, with the binder's message (D16.7). What stays on the server is
  decided by the door alone (D16.9). A failure shows where the plan places it (D16.13). A date or a time compares by
  its key (D16.16).
- Q8: a validator's `When`, `Unless` and `Otherwise` and a rule's `When` and `Unless` write the one condition core
  over the checked body (D16.8), through the one translator (N8.1).
- Q6: a check's comparison of a date or a time reads each operand through its type's key, as a list's lambda does
  (D6.12, D16.16).
- Q12: a slot's own registration leaves with its slot, and the page's rule over a member the slot rendered holds the
  page's save (D16.3, X12.4).
- Q13: each container keeps its own summary, which outlives a slot that joined it (D13.6, X13.5); a rule for each
  item checks every row its container sends (D13.5, X13.3).
- Q18: the registration names each validator and each flags enum once, by a type argument, with no assembly scan and
  no reflection (D18.1, N16.4, N16.5).
- Q19: the browser's rules are the plan's `form` and `model` data, keyed by validator and rule set (N16.2).

## Boundaries

A condition's value, text and membership tests in a pipeline are module 3's (M3.N1), and a validator's condition
reads the same terms as a lambda (N8.1). A request's `Validate`, which names the request's validator, its body and
the error notice are module 6's (D16.2, D10.21); the container a request sends is N14.3's, and the ids and binding
names of the inputs it holds are module 7's (M7.N2, M7.N3). The summary a view renders, `FormSummary`, is module 9's,
on the plan's own type. This module holds what a check means: its validator, its rules, its starts, its messages and
the place of each failure.

D8.3 and X8.3 follow D16.8 and X16.13: a validator condition the lambda door cannot translate is refused unless
`WhenOnServer` or `UnlessOnServer` declares it the server's, a typed declaration in place of a split the page cannot
see. X8.3 is the declared condition's behavior, in M8.F4.

## Doors

No door of this module's own: Q16's Doors keep the validator grammar, and N16.1 to N16.5 carry the endpoint's door,
the registration's data, the check with no request and the flags enums.

- Main's `When(Func<T, bool>, Action)` and `Unless(Func<T, bool>, Action)` are conditions only the server decides,
  and a `ClientRule` inside one throws (`Alis.Reactive.FluentValidator/ReactiveValidator.cs:189-190`, `:266-270` at
  623dd4a27). They are refused: the grammar's `When` and `Unless` take an `Expression<Func<T, bool>>`, which the one
  translator writes for both sides (N8.1), and `WhenOnServer` and `UnlessOnServer` declare a condition the server
  alone decides (D16.9). `WhenAsync` and `UnlessAsync` are kept as they are.
- Main's `ClientRule`, `ClientRuleEach`, `ReactiveClientRules` and the core's `AddReactiveClientValidation` with its
  rule builders leave for the grammar's own checks (N16.2), and main's guard vocabulary (`WhenField`, `FieldStart`,
  `FieldGuard`) for the lambda (N8.1). Each verdict is in `spec/Validation/features.json`.
- `AddFromAssembly` and `AddFromAssemblyContaining<T>` stay refused (Q16's Doors); `Add<TValidator>()` registers
  each validator.

The meaning `M8.D1`, a check's message, is in `spec/Validation/features.json`, on the feature it gives.

## Improvements

- One rule, one text. Main's pages write a rule twice, once in FluentValidation's grammar and once in main's
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion/FileUpload/FileUploadValidator.cs:10-11` at
  623dd4a27), and `ClientRule` adds the server's rule beside the browser's from its own vocabulary
  (`Alis.Reactive.FluentValidator/ReactiveClientRuleBuilder.cs:87` at 623dd4a27). The redesign derives the browser's
  rule from `RuleFor` (`Alis.Reactive/Validation/ReactiveValidator.cs:142` at 65fdf118d), which the rewrite keeps
  (D16.1).
- One condition core. Main writes a rule's condition through `WhenField` and a builder of its own
  (`Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Conditions/AdmissionAssessment/AdmissionAssessmentValidator.cs:19`
  at 623dd4a27); the redesign translates a closed list of lambda terms
  (`Alis.Reactive/Validation/Rules.cs:76-80` at 65fdf118d), which D16.8 keeps, a comparison of two members joining
  it, written by the one translator (N8.1).
- A date or a time compared to the tick. Both lines order a date by a `Date`'s `getTime`, to the millisecond
  (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:194-196` at 65fdf118d,
  `Alis.Reactive.Assets/runtime/shared/shape-convert.ts:167` at 623dd4a27), and the redesign orders only numbers,
  booleans and dates (`Alis.Reactive/PlanModel/Validation/BrowserRule.cs:179-183`,
  `Alis.Reactive/PlanModel/Validation/BrowserRules.cs:268` at 65fdf118d), so a `TimeOnly` and a `TimeSpan`, written
  as text (`Alis.Reactive/Members/ClrShape.cs:75-80` at 65fdf118d), have no browser rule. The keys give each one
  that agrees with FluentValidation's, null passing an ordering (D16.16, X16.27, `probe:fv-date-comparisons`).
- One registration. Main registers rules for the browser alone, keyed by a `System.Type`, beside the validator
  (`Alis.Reactive/Validation/ReactiveClientValidationServiceCollectionExtensions.cs:13-15` and `:39-41` at
  623dd4a27), and registers its validators by an assembly scan (`Alis.Reactive.SandboxApp/Program.cs:18-19` at
  623dd4a27). The redesign registers each validator by its type argument
  (`Alis.Reactive/Validation/ReactiveFluentValidationServiceCollectionExtensions.cs:50` at 65fdf118d), which the
  rewrite keeps (N16.2).
- A slot's rule leaves with its slot, and the page's stays. The redesign's test holds the page's save once the
  partial that rendered the room unloads
  (`tests/Alis.Reactive.Framework.PlaywrightTests/Partials/WhenAPartialOfThePagesModelIsConditionalOrLoadedLater.cs:94`
  at 65fdf118d), which X12.4 now states (D16.3).
