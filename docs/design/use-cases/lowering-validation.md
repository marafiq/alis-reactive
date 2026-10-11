> Lowering of `validation.md` into the grammar, read at `195d3a1`. The verdicts were recorded before [`../grammar.md`](../grammar.md) settled the gaps and ambiguities; that file's sections 8 and 9 give the decisions and the final counts. Each table was produced by one reader against the code and spot-checked.

# Validation use cases lowered into architecture.md (v6)

Target read: branch `feature/component-redesign`, HEAD `195d3a1`. Inventory: `docs/design/use-cases/validation.md`
(read at `0452a4e`; `git diff --stat 0452a4e HEAD` over the sandbox models, `runtime/validation`, `Alis.Reactive/Validation`,
`Alis.Reactive.FluentValidator` and `FusionGrid` is empty, so every cited line still holds).
FluentValidation source checked: tag `12.1.1` (net10) and branch `11.x` (= 11.12.0, net48; its changelog's top entry is 11.12.0).
`IValidationRule`, `IRuleComponent`, `IComparisonValidator`, `ILengthValidator`, `IRegularExpressionValidator`,
`IBetweenValidator`, `IInclusiveBetweenValidator`, `INotEmptyValidator`, `INotNullValidator`, `IEmailValidator`,
`ICreditCardValidator`, `IChildValidatorAdaptor`, `IIncludeRule` and `AbstractValidator.OnRuleAdded` are identical in both.

## Notation

- `v`: the checked input's value after the CLR-type empty rule (architecture.md:741-743): string, empty → `null`;
  non-nullable number, empty → `0`; `Nullable<T>`, empty → `null`; `bool`, unchecked → `false`; `bool?`, unanswered → `null`;
  `DateTime?`, empty → `null`; a collection with nothing selected → `[]`.
- `m.a.b`: a read of `form.values` (the model). `T(x)` = `toTime`, `N(x)` = `toNumber`.
- `empty(x)`: FluentValidation's empty: null/undefined, whitespace-only text, a length-0 array, or the non-nullable type's
  default (`0`, `false`, default date).
- In table cells, `or` is JavaScript `||` (a pipe would break the table). `when true` means no condition.
- Obligation codes (O-…) are defined exactly after the table.

## Table

| # | Use case (short) | FluentValidation statement | Projected test (check / when) | Verdict |
|---|---|---|---|---|
| 1 | required: empty text (Native) | `RuleFor(x => x.Name).NotEmpty().WithMessage("'Name' is required.")` | `!empty(v)` / when true | EXPRESSED |
| 2 | required rejects whitespace-only | `RuleFor(x => x.RoomName).NotEmpty().WithMessage("Enter the room name.")` | `!empty(v)` (whitespace is FV-empty) | EXPRESSED |
| 3 | required, `decimal`: 0 and cleared fail | `RuleFor(x => x.MonthlyRate).NotEmpty()…` | `!empty(v)`; cleared reads 0 = default | EXPRESSED |
| 4 | required, `decimal?`: blank fails, 0 passes | `RuleFor(x => x.CommunityFee).NotEmpty()…` | `v != null` (default of `decimal?` is null) | EXPRESSED |
| 5 | required, `int?` select on placeholder | `RuleFor(x => x.CommunityId).NotEmpty()…` | `v != null` (placeholder "" reads null) | EXPRESSED |
| 6 | required, `bool` | `RuleFor(x => x.ObservedWalkingInPerson).NotEmpty()…` | `v !== false` | EXPRESSED |
| 7 | required, `bool?`: false is an answer | `RuleFor(x => x.FellInLast90Days).NotEmpty()…` | `v != null` | EXPRESSED |
| 8 | required, `DateTime?` (DatePicker) | `RuleFor(x => x.AdmissionDate).NotEmpty()…` | `v != null` | EXPRESSED |
| 9 | required, `DateTime[]?` (DateRangePicker) | `RuleFor(x => x.StayPeriod).NotEmpty()…` | `v != null && v.length > 0` | EXPRESSED |
| 10 | required reads Fusion values | `NotEmpty()` per field | `v` read via each declaration's `Input<T>.Value` path+shape (4.2) | EXPRESSED |
| 11 | required, Tom Select | `RuleFor(x => x.SuiteType).NotEmpty().WithMessage("Choose a suite type.")` | `!empty(v)`, value path `input.value` | EXPRESSED |
| 12 | `Empty` on text | `RuleFor(x => x.Nickname).Empty().WithMessage(…)` | `empty(v)` | OBLIGATION O-ID (EmptyValidator has no interface) |
| 13 | `Empty` on number/bool | `Empty()` on `int`/`bool` | `empty(v)`: non-nullable 0/false empty, `int?` 0 not | OBLIGATION O-ID |
| 14 | email | `RuleFor(x => x.Email).EmailAddress().WithMessage(…)` | `v == null or matches(v, ^[^\s@]+@[^\s@]+\.[^\s@]+$)` | OBLIGATION O-ID (IEmailValidator is a marker with no pattern; FV default is AspNetCoreCompatible, EmailValidator.cs:68-85) |
| 15 | url (http/https) | `RuleFor(x => x.Website).Url().WithMessage(…)` (framework `IBrowserRule`) | `v == null or matches(v, ^https?://.+)` | OBLIGATION O-BROWSER |
| 16 | creditCard (Luhn) | `RuleFor(x => x.CardNumber).CreditCard().WithMessage(…)` | `v == null or luhn(v)` | OBLIGATION O-LUHN |
| 17 | at least one (CheckList, MultiSelect, FileUpload) | `RuleFor(x => x.DietaryNeeds).NotEmpty().WithMessage("Select at least one …")` | `v != null && v.length > 0` | EXPRESSED (architecture.md:729; uploader value = files, :473) |
| 18 | minLength | `RuleFor(x => x.Password).MinimumLength(8)…` | `v == null or v.length >= Min` (ILengthValidator Min=8, Max=-1) | EXPRESSED |
| 19 | maxLength | `RuleFor(x => x.Nickname).MaximumLength(50)…` | `v == null or v.length <= Max` | EXPRESSED |
| 20 | regex | `RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$")…` | `v == null or matches(v, Expression)` | EXPRESSED (JS dialect checked at render, :752) |
| 21 | inclusive range on numbers | `RuleFor(x => x.Age).InclusiveBetween(0, 120)…` | `v == null or (N(v) >= From && N(v) <= To)` | EXPRESSED |
| 22 | exclusiveRange | `RuleFor(x => x.Score).ExclusiveBetween(0m, 100m)…` | `v == null or (N(v) > From && N(v) < To)` | GAP: `Score` is `decimal` (NewRuleTypesModel.cs:6): empty reads 0 and fails; today empty passes (rule-engine.ts:209). Also O-ID (no interface) |
| 23 | min literal on a number | `RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m)…` (`decimal?`) | `v == null or N(v) >= 0` | EXPRESSED |
| 24 | min date literal | `RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020,1,1))…` | `v == null or T(v) >= T("2020-01-01")` | EXPRESSED |
| 25 | max literal on a number | `RuleFor(x => x.Salary).LessThanOrEqualTo(500000m)…` | `v == null or N(v) <= 500000` | EXPRESSED |
| 26 | max date literal (today / now) | `RuleFor(x => x.AdmissionDate).LessThanOrEqualTo(DateTime.Today)…`; `…(DateTime.UtcNow)` | `v == null or T(v) <= T(lit)` | OBLIGATION O-DATE |
| 27 | gt literal, `decimal`: empty fails | `RuleFor(x => x.MonthlyRate).GreaterThan(0m)…` | `N(v) > 0` (v never null; empty → 0) | EXPRESSED (:742-743) |
| 28 | gt date literal | `GreaterThan(<date>)` on `DateTime?` | `v == null or T(v) > T(lit)` | GAP: nullable empty now passes; today `gt` fails on empty (rule-engine.ts:193-196). Design-accepted (:744-747); no sandbox use |
| 29 | lt literal, equal fails | `RuleFor(x => x.MaxDeposit).LessThan(1000000m)…` (`decimal`) | `N(v) < 1000000` (empty → 0 passes, as today) | EXPRESSED |
| 30 | lt date literal (before today) | `RuleFor(x => x.Value).LessThan(DateTime.Today)…` (`DateTime?`) | `v == null or T(v) < T(lit)` | OBLIGATION O-DATE |
| 31 | gt peer date; equal fails | `RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)…` | `v == null or (m.admissionDate != null && T(v) > T(m.admissionDate))` (FV fails on a null peer, AbstractComparisonValidator.cs:77) | EXPRESSED (empty v is caught first by NotEmpty, DateValidationValidator.cs:13-14) |
| 32 | ordered peer, numbers | `GreaterThan(x => x.Other)` on numbers | `v == null or (m.other != null && N(v) > N(m.other))` | EXPRESSED |
| 33 | lt/min/max/equalTo/notEqualTo against peer dates | `LessThan(x => x.R)`, `GreaterThanOrEqualTo(x => x.A)`, `LessThanOrEqualTo(x => x.R)`, `Equal(x => x.A)`, `NotEqual(x => x.R)` | ordered as 31; Equal `T(v) === T(m.a)` (null equals null); NotEqual negated | GAP: FV Equal/NotEqual do not skip null (EqualValidator.cs:47-58, NotEqualValidator.cs:44-54): empty v vs a set peer now fails Equal; both empty now fails NotEqual. Today both skip when nothing is entered (rule-engine.ts:251, 271-273) |
| 34 | equalTo literal | `Equal(5)` | `N(v) === 5` | GAP: empty now fails; today skipped (rule-engine.ts:251) |
| 35 | equalTo peer text (confirm email) | `RuleFor(x => x.ConfirmEmail).NotEmpty()…` then `.Equal(x => x.Email).WithMessage(…)` | `v === m.email` | EXPRESSED (NotEmpty precedes, so empty never reaches Equal) |
| 36 | equalTo peer in a nested child | `RuleFor(x => x.Address).SetValidator(new NestedBugAddressValidator())`; child `RuleFor(x => x.ConfirmCity).Equal(x => x.City)` | field `…__Address_ConfirmCity`; `v === m.address.city` | OBLIGATION O-PEER, O-PFX (both-empty: null equals null, unchanged) |
| 37 | equalTo peer inside a collection item | `RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator())`; item `RuleFor(x => x.ConfirmSku).Equal(x => x.Sku)` | per rendered i: field `…Lines_i__ConfirmSku`; `v === m.lines[i].sku` | GAP: ConfirmSku has no NotEmpty (ValidationShowcaseValidator.cs:143-144): Sku set + ConfirmSku empty now fails; today passes (rule-engine.ts:251). Needs O-IDX, O-PEER |
| 38 | notEqual literal text | `RuleFor(x => x.Status).NotEqual("deleted")…` | `v !== "deleted"` (null passes in FV too) | EXPRESSED |
| 39 | notEqualTo peer text | `RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email)…` | `v !== m.email` | GAP: both empty now fails (FV `Equals(null,null)`); today passes (rule-engine.ts:271-273); NewRuleTypesValidator.cs:24-25 |
| 40 | non-required rules skip empty | `Matches`, `GreaterThanOrEqualTo`, `LessThanOrEqualTo` on `string?`/`decimal?` | every such check starts `v == null or …` | EXPRESSED |
| 41 | first failing rule per field wins | two `RuleFor(x => x.DischargeDate)` statements | tests in rule order × component order | OBLIGATION O-FIRST |
| 42 | `WhenField(bool)` | `When(x => x.IsEmployed, () => RuleFor(x => x.JobTitle).NotEmpty()…)` | when `m.conditional.isEmployed` (via O-PFX) | EXPRESSED |
| 43 | `WhenField(field, value)` | `When(x => x.CareLevel == "Memory Care", …)` | when `m.careLevel === "Memory Care"` | EXPRESSED |
| 44 | `WhenFieldNot(bool)` | `Unless(x => x.HasEmergencyContact, …)` | when `!m.hasEmergencyContact` | EXPRESSED |
| 45 | `WhenFieldNot(field, value)` | `When(x => x.CareLevel != "Independent", …)` | when `m.careLevel !== "Independent"` (null → true, as today) | EXPRESSED |
| 46 | `WhenFieldGt` | `When(x => x.F > k, …)` | when `m.f > k` | OBLIGATION O-LIFT |
| 47 | `WhenFieldGte` (`int Age`) | `When(x => x.Age >= 18, …)` | when `m.age >= 18` (empty → 0 → false, as today) | EXPRESSED |
| 48 | `WhenFieldLt` (`int Age`) | `When(x => x.Age < 18, …)` | when `m.age < 18` | GAP: `int Age` (OperatorConditionModels.cs:5) reads 0 when empty, so the rule fires on an empty Age; today a missing value compares false (compare-engine.ts:344, 386). Frozen tests still pass (WhenNewOperatorConditionsWork.cs fills Age where asserted) |
| 49 | `WhenFieldLte` | `When(x => x.F <= k, …)` | when `m.f <= k` | OBLIGATION O-LIFT |
| 50 | `WhenFieldNull` | `When(x => x.F == null, …)` | when `m.f == null` (loose) | OBLIGATION O-LIFT |
| 51 | `WhenFieldNotNull` | `When(x => x.F != null, …)` | when `m.f != null` (loose) | OBLIGATION O-LIFT |
| 52 | `WhenFieldEmpty` | `When(x => string.IsNullOrEmpty(x.F), …)` | when `m.f == null or m.f === ""` | OBLIGATION O-STR |
| 53 | `WhenFieldNotEmpty` (top level and nested) | `When(x => !string.IsNullOrEmpty(x.Email), …)`; in `NestedBugAddressValidator` `When(x => !string.IsNullOrEmpty(x.City), …)` | when `!(m.email == null or m.email === "")`; nested `m.address.city` | OBLIGATION O-STR, O-PFX |
| 54 | `WhenFieldIn` | `When(x => new[] { "memory-care", "skilled-nursing" }.Contains(x.CareLevel), …)` | when `["memory-care","skilled-nursing"].includes(m.careLevel)` | OBLIGATION O-STR |
| 55 | `WhenFieldNotIn` | `When(x => !new[] { … }.Contains(x.F), …)` | negation of 54 | OBLIGATION O-STR |
| 56 | `WhenFieldBetween` | `When(x => x.F >= lo && x.F <= hi, …)` | when `m.f >= lo && m.f <= hi` | OBLIGATION O-LIFT |
| 57 | `WhenFieldContains` | `When(x => x.Notes != null && x.Notes.Contains("urgent"), …)` | when `m.notes != null && m.notes.includes("urgent")` | EXPRESSED (§6 string table; the null guard is now the author's: today null-safe on both sides, FieldConditionBuilder.cs:111-118) |
| 58 | `WhenFieldStartsWith` | `When(x => x.F != null && x.F.StartsWith(p), …)` | `startsWith` | EXPRESSED |
| 59 | `WhenFieldEndsWith` | `When(x => x.F != null && x.F.EndsWith(s), …)` | `endsWith` | EXPRESSED |
| 60 | `WhenFieldMatches` | `When(x => x.F != null && Regex.IsMatch(x.F, p), …)` | when `matches(m.f, p)` | OBLIGATION O-STR |
| 61 | `WhenFieldMinLength` | `When(x => x.F != null && x.F.Length >= n, …)` | when `m.f.length >= n` | EXPRESSED |
| 62 | `WhenFieldArrayContains` | `When(x => x.Tags != null && x.Tags.Contains("fall-risk"), …)` | when `m.tags.includes("fall-risk")` | EXPRESSED (§4.3 `Contains`) |
| 63 | `WhenFields(..).And` | `When(x => x.TakesPainMedication && x.PainLevel > 7m, …)` | when `m.takesPainMedication && m.painLevel > 7` (`decimal`, empty → 0, as today) | EXPRESSED |
| 64 | `.Or` / `.Not` | `When(x => a or b, …)`, `When(x => !(…), …)` | `op` | EXPRESSED |
| 65 | section hidden by a reaction, rule on the same flag | `When(x => x.IsVeteran, () => RuleFor(x => x.VeteranId).NotEmpty()…)` | when `m.isVeteran` | EXPRESSED |
| 66 | activation re-evaluated every validation | n/a | `when(m)` evaluated per evaluation | OBLIGATION O-EVAL |
| 67 | parent condition around child with its own condition | `When(x => x.ParentFlag, () => RuleFor(x => x.Child).SetValidator(new ChildSectionValidator()))`; child `When(x => x.ChildFlag, …)` | when `m.parentFlag && m.child.childFlag` | OBLIGATION O-WHEN, O-PFX |
| 68 | rules under FV `When`/`Unless`/`WhenAsync` stay server-only | `When(model => model.ServerFlag, () => RuleFor(..).NotEmpty())` (harness :257); `When(x => x.CareLevel == "Memory Care", …)` (FusionStepperWizardModels.cs:109) | now projected: when `m.serverFlag` / `m.careLevel === "Memory Care"` | GAP: an expression `When` projects any translatable predicate, so today's server-only rules reach the browser (ServerFlag has no input → `undefined`; `!x.ServerFlag` would fire in the browser). Server-only now needs `WhenAsync` or the `(x, ctx)` overload; an untranslatable `When` fails at render (:822) instead of staying server-only (:731) |
| 69 | `ClientRule` inside FV `When` throws with guidance | n/a (no `ClientRule`) | none | RENDER: §6 render-time failure naming an untranslatable construct; analyzer on rule-level `.When` (:731-733) |
| 70 | child validator, prefixed paths | `RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator())` | fields `…__Address_Street` etc.; checks as rows 1/20 | OBLIGATION O-CHILD, O-PFX |
| 71 | `RuleForEach..SetValidator` onto every rendered item | `RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator())` | one test set per rendered item i | RENDER: O-IDX expansion over rendered item inputs |
| 72 | nested `RuleForEach` in an item validator + per-item condition | item `When(x => x.GiftWrapped, …)`, `RuleForEach(x => x.Deliveries).SetValidator(..)`; delivery `When(x => x.RequiresAccessCode, …)` | when `m.lines[i].giftWrapped`; `m.lines[i].deliveries[j].requiresAccessCode` | OBLIGATION O-IDX (nested), O-WHEN |
| 73 | `RuleForEach` inside a condition | `When(x => x.HasContacts, () => RuleForEach(x => x.Contacts).SetValidator(new EmergencyContactValidator()))` | item tests, when `m.hasContacts` | OBLIGATION O-WHEN, O-IDX |
| 74 | at least one on the collection itself | `RuleFor(x => x.Contacts).NotEmpty()` | needs `m.contacts.length > 0`, but a test is keyed by an input (`field`, `value`) | GAP: a collection of complex items has no input (architecture.md:756-757, :893-896); the only lowering is an always-unmounted field that blocks with a summary entry, which is today's same defect (ReactiveClientRuleBuilder.cs:60-65 binds to a nonexistent `…__Contacts` id) |
| 75 | same-type rules imported inside a condition | `When(x => x.IsEmployed, () => Include(new SharedEmploymentRulesValidator()))` | included tests, when `m.isEmployed` | OBLIGATION O-CHILD (IncludeRule), O-WHEN |
| 76 | rules imported from a base-type validator | `Include(new BasicSectionValidator())` (`IValidator<in T>`; `AllRulesSection : BasicSection`) | Name/Email tests at the section prefix | OBLIGATION O-CHILD; the server now also enforces them (today `ClientRulesFrom<TSource>` adds no server rule, ReactiveValidator.cs:92-101) |
| 77 | app-level rules via `AddReactiveClientValidation` | none: deleted (:758-760) | none | GAP: removed; rules must be rewritten as a `ReactiveValidator<T>`, and client validation without FluentValidation disappears (ReactiveClientValidationServiceCollectionExtensions.cs:13, ClientValidationRulesBuilder.cs:34) |
| 78 | assembly-scanned validators resolved through DI | `AddReactiveFluentValidation(..)` | none | RENDER: `Validate<TValidator>` resolves TValidator from DI at render and projects it |
| 79 | DI validator with an injected service | same | none | RENDER: same resolution, children through O-CHILD |
| 80 | registrations merge; plain `AbstractValidator` stays server-only | same | none | RENDER: only `ReactiveValidator<T>` is projected (a plain validator's descriptors are equally readable, so the restriction must be explicit) |
| 81 | unregistered source fails with guidance | n/a | none | RENDER: render-time throw naming the type (ClientValidationRuleSource.cs:34) |
| 82 | one statement is also the server rule | the FV statement itself | n/a | EXPRESSED |
| 83 | `Must` stays server-only; the 400 is shown | `RuleFor(x => x.RoomCode).Must(..).WithMessage(..)` | not projected; `form.showErrors(res.body)` | EXPRESSED |
| 84 | `MustAsync` server-only beside a client rule | `RuleFor(x => x.Code).NotEmpty().MustAsync(..)` | NotEmpty component projected; async component skipped | EXPRESSED (unrecognized component types are not projected) |
| 85 | `Validate` blocks the request | `.Validate<FallRiskScreeningValidator>("fall-risk-form")` → `if (form.validate()) {…}` | n/a | OBLIGATION O-ORDER |
| 86 | passing validation sends once | same | n/a | EXPRESSED |
| 87 | several forms on one plan | one `builtin(form, key)` per `Validate` key | each form runs only its tests | OBLIGATION O-SCOPE |
| 88 | message in `{id}_error`, `alis-has-error` on the input | n/a | n/a | OBLIGATION O-DISPLAY |
| 89 | correcting removes `alis-has-error` | n/a | n/a | OBLIGATION O-LIVE, O-DISPLAY |
| 90 | resubmitting clears the previous attempt | n/a | n/a | OBLIGATION O-DISPLAY (clear at start of validate) |
| 91 | server errors inline by dotted name | `.OnError(400, e => e.ValidationErrors(form))` → `do(form.showErrors(res.body))` | n/a | OBLIGATION O-SRV |
| 92 | server error with no client-ruled slot → summary; replaced, not duplicated; hidden after success | same | n/a | OBLIGATION O-SRV, O-DISPLAY |
| 93 | server error, client-ruled field, hidden slot → summary | same | n/a | OBLIGATION O-SRV |
| 94 | server error inline adds `alis-has-error` | same | n/a | OBLIGATION O-SRV |
| 95 | several server messages joined | same | n/a | OBLIGATION O-SRV |
| 96 | server errors keyed `Lines[0].Sku` land on the item | same | n/a | OBLIGATION O-SRV (exact input `name`); its only proof parses the old plan JSON (see ambiguity 7) |
| 97 | client error, hidden slot → summary | n/a | n/a | OBLIGATION O-DISPLAY |
| 98 | corrected summary entry removed, empty summary hidden; revealed-but-invalid moves inline | n/a | n/a | OBLIGATION O-LIVE |
| 99 | fields from same-planId server partials validated inline | n/a | n/a | OBLIGATION O-SCOPE, O-OWN |
| 100 | fields from a later AJAX partial validated once loaded | n/a | n/a | OBLIGATION O-OWN, O-MOUNT |
| 101 | unmounted field, all tests inactive → skipped | n/a | n/a | OBLIGATION O-MOUNT |
| 102 | unmounted field, an active test → blocks, first message in summary | n/a | n/a | OBLIGATION O-MOUNT |
| 103 | field outside the validated container → skipped | n/a | n/a | OBLIGATION O-SCOPE |
| 104 | missing container → blocks | n/a | n/a | OBLIGATION O-SCOPE |
| 105 | slot partial with its own plan and `Validate` | its own form, owned by the slot (law 4) | n/a | EXPRESSED |
| 106 | re-injected partial re-wires live validation | n/a | n/a | OBLIGATION O-LIVE, O-OWN |
| 107 | slot-owned tests removed on unload | n/a | n/a | OBLIGATION O-OWN |
| 108 | typing clears the message (Native), no revalidation | n/a | n/a | OBLIGATION O-LIVE |
| 109 | typing clears the message (Fusion) | n/a | n/a | OBLIGATION O-LIVE (DOM `input` on `getElementById(id)`) |
| 110 | leaving a Native field revalidates | `NativeTextBox.Committed` | n/a | OBLIGATION O-COMMIT |
| 111 | leaving a Fusion text field revalidates | `FusionTextBox.Committed` | n/a | OBLIGATION O-COMMIT |
| 112 | radio group change revalidates (hidden root) | `NativeRadioGroup.Committed` = change | n/a | EXPRESSED |
| 113 | Fusion DropDownList/DatePicker popup change revalidates | `Committed` = change | n/a | EXPRESSED |
| 114 | Tom Select live validation | `Committed` = change | n/a | EXPRESSED |
| 115 | wired from boot: leaving an invalid field before any submit shows it | n/a | n/a | GAP: needs O-LIVE (subscribe at boot), and a single `Committed` per input cannot carry today's blur + change pair on every field (live-clear.ts:85-86, 99-105): an input whose `Committed` is `change` (112-114, architecture.md:893) no longer revalidates when left untouched |
| 116 | grid column rules from the same validator block an in-cell edit | `RuleFor(r => r.OpenTasks).InclusiveBetween(0, 7).WithMessage(..)` | EJ2 `range: [[0,7], msg]` | RENDER: O-GRID |
| 117 | grid mapping: only EJ2-equivalent rules; conditional rules never always-on | same | EJ2 `validationRules` | RENDER: O-GRID |

Counts: EXPRESSED 47, OBLIGATION 51, RENDER 8, GAP 11 (total 117).

## Obligations, exactly

- **O-ID (descriptor recognition).** Per component, test in this order: `IChildValidatorAdaptor` → recurse; `INotNullValidator`;
  `INotEmptyValidator`; `IPropertyValidator.Name == "EmptyValidator"` (EmptyValidator.cs:27 has no interface);
  `ILengthValidator` (skip when built from `MinFunc`/`MaxFunc`); `IEmailValidator` before `IRegularExpressionValidator`
  (`EmailValidator<T>` implements both); `IRegularExpressionValidator` (skip when `Expression` is null); `ICreditCardValidator`;
  `IComparisonValidator`; `IInclusiveBetweenValidator`; `IBetweenValidator` not `IInclusiveBetweenValidator` = exclusive
  (ExclusiveBetweenValidator.cs:26 has no interface); framework `IBrowserRule<T>`; anything else → not projected.
  Message = `IRuleComponent.GetUnformattedErrorMessage()`.
- **O-BROWSER.** `IBrowserRule<TProperty>` exposes its check as an expression the §6 compiler lowers (Url:
  `v == null or matches(v, "^https?://.+")`, rule-engine.ts:164); the server half stays `IsEmptyOrHttpUrl`
  (ReactiveClientRuleBuilder.cs:374-379).
- **O-LUHN.** `ICreditCardValidator` projects to one fixed expression: `d = v.split("").filter(c => c >= "0" && c <= "9")`,
  `d.length >= 13 && d.reverse().reduce((s, c, i) => s + (i % 2 ? (2*N(c) > 9 ? 2*N(c) - 9 : 2*N(c)) : N(c)), 0) % 10 === 0`
  (rule-engine.ts:168-171, 280-296; no operator does Luhn).
- **O-DATE.** A date literal is `ValueToCompare` as captured when the validator was constructed, serialized exactly as
  `ValidationDateLiteral` does (`yyyy-MM-dd` at midnight, else `"s"` with no offset; ValidationDateLiteral.cs:18-34). When
  projection runs must be stated: today it is once per app (singleton,
  ReactiveFluentValidationServiceCollectionExtensions.cs:136-146), so `DateTime.Today` is frozen at first resolve.
- **O-PEER.** Peer read = rule prefix (including concrete item indexes) + `MemberToCompare.Name`; render throws when
  `MemberToCompare.DeclaringType` is not assignable from the validator's `T` (a `MemberInfo` carries no path,
  AbstractComparisonValidator.cs:114). Ordered comparison fails when the peer is null (FV, :77).
- **O-FIRST.** Tests are emitted in validator rule order (an `Include`/`SetValidator` inlined at its position), then component
  order. Per field: skip tests whose `when(m)` is false; the first whose `check(v, m)` is false decides and later tests are not
  evaluated (orchestrator.ts:303-316).
- **O-WHEN.** `ReactiveValidator<T>` declares `When(Expression<Func<T,bool>>, Action)` and `Unless(..)`, pushes the expression
  during `action`, overrides `OnRuleAdded(IValidationRule<T>)` (AbstractValidator.cs:393-399, present in 11.12 and 12.1.1) to
  tag each rule with the AND of the stack, and calls `base.When(compiled, action)` for the server. Projection skips a rule with
  `HasAsyncCondition`, a rule with `HasCondition` and no tag, and any component with `HasCondition`/`HasAsyncCondition`
  (rule-level delegate). Today's `new When(Func…)` overloads (ReactiveValidator.cs:189-219) must go, or a lambda is
  ambiguous between them (CS0121; not compiled here).
- **O-PFX.** A child reached through `SetValidator`/`Include` at member path p: field id = `IdGenerator.For(rootModel, p.path)`;
  each `x.M` read in check, when or peer becomes `m.<p>.m`; the test's when = AND of every enclosing when, outer first.
- **O-IDX.** `RuleForEach(x => x.C).SetValidator(V)`: recognized by `rule.Expression.ReturnType != rule.TypeToValidate`
  (CollectionPropertyRule.cs:66); for each input in the render whose binding name starts with `C[i]`
  (ClientValidationFieldBinding.cs:108-134), emit V's tests with prefix `C[i]` (O-PFX), recursively; the index is literal in
  the field id and in every model path.
- **O-CHILD.** For `IChildValidatorAdaptor` (it exposes only `ValidatorType`, ChildValidatorAdaptor.cs:8-13), resolve
  `ValidatorType` from the request's `IServiceProvider` (validators are registered transient by concrete type,
  ReactiveFluentValidationServiceCollectionExtensions.cs:94-100); unresolvable → render throws naming it. `IIncludeRule`'s one
  component is a `ChildValidatorAdaptor`; prefix "".
- **O-LIFT.** If an operand's C# type is `Nullable<T>`, `a < b` (and `<=`, `>`, `>=`) lowers to `a != null && b != null && a < b`
  (JavaScript gives `null >= 0 === true`). `== null`/`!= null` are loose (null and undefined, as compare-engine.ts:280-284).
- **O-STR.** Compiler entries missing from §6: `string.IsNullOrEmpty(s)` → `s == null or s === ""`;
  `Regex.IsMatch(s, constant)` → `matches`; `Enumerable.Contains` on a closure-rooted `new[] {…}` → literal `.includes(x)`.
- **O-EVAL.** `when(m)` and `check(v, m)` are evaluated against current values on every `validate()` and every field
  revalidation; nothing is cached.
- **O-ORDER.** The request builder emits `if (form.validate())` outermost, before WhileLoading statements
  (WhenAdmittingNewResident.cs:154).
- **O-SCOPE.** Form k's tests = `forms[k].tests` of every composed plan. Its model `m` = values of all inputs of the composed
  plan by binding name (not only inputs tagged `form: k`, which a partial cannot know). `validate()`, revalidation and live
  wiring apply a test only when its field element is inside `getElementById(k)` (orchestrator.ts:233). `validate()` returns
  false when `getElementById(k)` is null and the form has tests (orchestrator.ts:95-103).
- **O-MOUNT.** A test field with no input or no element at evaluation: if every test of that field has a false `when(m)` (a
  `when` that throws counts as false), skip it; otherwise the form is invalid and the summary gets one entry keyed by the field
  with the message of that field's first test in plan order, active or not (orchestrator.ts:250-287). Live wiring waits for
  the element.
- **O-DISPLAY.** Inline is renderable when `getElementById(id + "_error")` has a parent and neither the parent nor an
  ancestor has the `hidden` attribute or inline `style.display === "none"` (orchestrator.ts:294-301, 414-422). Failing and
  renderable: slot text = message, remove `hidden`, clear `display`, add `alis-has-error` to `getElementById(id)`, remove the
  field's summary entry. Failing and not renderable: summary entry (one `div[data-valmsg-summary-for=key]` per key, replaced,
  appended) and summary shown. Summary = `getElementById(planId with "." and "+" → "_" + "_validation_summary")` of the plan
  that declared the form (error-display.ts:57-60; PlanExtensions.cs:155-156); missing summary is a no-op. `validate()` first
  clears every test field's inline slot and class, then empties and hides the summary (orchestrator.ts:394-402); it evaluates
  every field and returns the AND.
- **O-SRV.** `showErrors(body)`: the same clearing first; body not an object → nothing; `body.errors` not an object → warn,
  nothing; per `[name, messages]`: message = array ? `join(", ")` : string; target = a test field of this form whose input
  `name === name` exactly; no target, no element or slot not renderable → summary entry keyed by `name`; else slot text and
  `alis-has-error` on the element (orchestrator.ts:132-182, 434-454; error-display.ts:62-75).
- **O-LIVE.** At form creation (boot or slot load, not first `validate()`), per test field with an element, once per element:
  DOM `input` on `getElementById(id)` → clear its inline slot and class only (live-clear.ts:62, 85); the input's `Committed`
  event → revalidate: clear inline, remove its summary entry and hide an empty summary, stop if the container is missing, else
  evaluate that field's tests only (orchestrator.ts:184-220). Peers and dependants are not revalidated.
- **O-COMMIT.** Text-like inputs declare `Committed` = blur (Native DOM `blur`; EJ2 TextBox `blur`), since today's blur
  revalidation is what 110/111 show and `change` does not fire on leaving an unchanged field.
- **O-OWN.** Composition concatenates `forms[k].tests` and `inputs` per owner; a slot unload removes exactly that slot's tests,
  inputs and the form's subscriptions on them, and keeps boot-owned ones (applied-plans.ts:55-100); a reload subscribes to the
  new elements.
- **O-GRID.** `FusionGridValidation` reads FV descriptors directly, not the form's tests: unconditional components only;
  NotEmpty → `required`, IEmailValidator → `email`, Url `IBrowserRule` → `url`, ILengthValidator → `minLength`/`maxLength`,
  IRegularExpressionValidator → `regex`, GreaterThanOrEqual/LessThanOrEqual literal numbers → `min`/`max`,
  IInclusiveBetweenValidator numbers → `range`; the rest emit nothing (FusionGridValidation.cs:104-154). Its source today,
  `IClientValidationRuleSource`, is deleted by :758-760.
