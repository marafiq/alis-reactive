> **History, not re-checked at HEAD.** The table's verdicts, its projected tests, the obligations and every `file:line` in this file are as recorded: verdicts at `195d3a1`, references re-anchored at `f36f62e` and, for the C#, at `6ac4198`. They were not re-anchored at `e837ad94`: `git diff --stat 6ac4198..e837ad94` over the framework's runtime, its components and its C# (the core, the FluentValidator, the link and grid-validation code, the analyzers) lists 66 changed files (among them `form.ts`, `FormWriter.cs`, `ClientRule.cs` and `ClientCondition.cs`), and the test lines cited (row 48's `WhenNewOperatorConditionsWork.cs`, O-ORDER's `WhenAdmittingNewResident.cs:154`, a fixture rewritten in `17420df1`) are as they were then. The current proof of each row that was weak or missing is the "Status at HEAD" table of [`validation.md`](validation.md); what was decided and what is built are [`../grammar.md`](../grammar.md) sections 7, 8 and 11. The `FormChecks.cs:34-35, 129-133` and `ValueExpression.cs:268-269` lines named below were checked at `e837ad94` and still hold, `Checks.cs` holds each check's two halves, and the four files the next note names are deleted.
>
> Lowering of `validation.md` into the grammar, read at `195d3a1`. The verdicts were recorded before [`../grammar.md`](../grammar.md) settled the gaps and ambiguities; that file's sections 8 and 9 give the decisions and the final counts. Each table was produced by one reader against the code and spot-checked.
>
> References re-anchored at `f36f62e`, and the C# ones again at `6ac4198`, where the lowering pass is gone: the FluentValidator builds the core's public `ClientRule` and `ClientCondition`, and the form writes its own grammar. The validation rework (`b966c57`) implemented this lowering, so verdicts and obligations stay as recorded at `195d3a1` and each now cites the code that carries it: `ClientRuleProjection.cs` and `BrowserCondition.cs` (`ClientCondition.cs` in the FluentValidator before `6ac4198`) read the FluentValidation rules and the `When` lambdas (O-ID, O-WHEN, O-PFX, O-IDX, O-CHILD), `FormWriter.cs` writes each rule as an `alis.form` test, and `Alis.Reactive.Assets/runtime/modules/form/form.ts` runs the tests; where the outcome changed, the row says how. The deleted rule engine (`rule-engine.ts`, `compare-engine.ts`, `orchestrator.ts`, `live-clear.ts`, `error-display.ts`) and C# builders (`ReactiveClientRuleBuilder.cs`, `FieldConditionBuilder.cs`, the app-level builders) are named without line numbers. Validation does not pass through `ObjectBinding`: a rule reads each input as MVC binds it, through `alis.model` (FormChecks.cs:34-35, 129-133; ValueExpression.cs:268-269), not through a component's members.
>
> Later, the framework implemented FluentValidation's grammar itself and dropped the package
> ([`../grammar.md`](../grammar.md) section 7). The `ClientRuleProjection.cs`, `RecordingValidator.cs`,
> `BrowserRule.cs` and `ReactiveRuleBuilderExtensions.cs` anchors below point at the commits named
> above; those files are deleted, and each check's browser half now sits beside its server half in
> `Checks.cs`.

# Validation use cases lowered into architecture.md (v6)

Target read: branch `feature/component-redesign`, HEAD `195d3a1`. Inventory: `docs/design/use-cases/validation.md`
(read at `0452a4e`; `git diff --stat 0452a4e HEAD` over the sandbox models, `runtime/validation`, `Alis.Reactive/Validation`,
`Alis.Reactive.FluentValidator` and `FusionGrid` is empty, so every cited line still holds).
FluentValidation source checked: tag `12.1.1` (net10) and branch `11.x` (= 11.12.0, net48; its changelog's top entry is 11.12.0).
`IValidationRule`, `IRuleComponent`, `IComparisonValidator`, `ILengthValidator`, `IRegularExpressionValidator`,
`IBetweenValidator`, `IInclusiveBetweenValidator`, `INotEmptyValidator`, `INotNullValidator`, `IEmailValidator`,
`ICreditCardValidator`, `IChildValidatorAdaptor`, `IIncludeRule` and `AbstractValidator.OnRuleAdded` are identical in both.

## Notation

- `v`: the checked input's value after the CLR-type empty rule (architecture.md:769-771): string, empty → `null`;
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
| 17 | at least one (CheckList, MultiSelect, FileUpload) | `RuleFor(x => x.DietaryNeeds).NotEmpty().WithMessage("Select at least one …")` | `v != null && v.length > 0` | EXPRESSED (architecture.md:757; uploader value = files, :486) |
| 18 | minLength | `RuleFor(x => x.Password).MinimumLength(8)…` | `v == null or v.length >= Min` (ILengthValidator Min=8, Max=-1) | EXPRESSED |
| 19 | maxLength | `RuleFor(x => x.Nickname).MaximumLength(50)…` | `v == null or v.length <= Max` | EXPRESSED |
| 20 | regex | `RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{3}-\d{4}$")…` | `v == null or matches(v, Expression)` | EXPRESSED (JS dialect checked at render, architecture.md:780; at `f36f62e` a pattern JavaScript reads differently stays on the server instead, ClientRuleProjection.cs:139-161) |
| 21 | inclusive range on numbers | `RuleFor(x => x.Age).InclusiveBetween(0, 120)…` | `v == null or (N(v) >= From && N(v) <= To)` | EXPRESSED |
| 22 | exclusiveRange | `RuleFor(x => x.Score).ExclusiveBetween(0m, 100m)…` | `v == null or (N(v) > From && N(v) < To)` | GAP: `Score` is `decimal` (NewRuleTypesModel.cs:6): empty reads 0 and fails; at `195d3a1` empty passed (`rule-engine.ts`). At `f36f62e` an empty `decimal` binds as 0 (shape.ts:42-53) and the range fails on it, as on the server (FormWriter.cs:170-187). Also O-ID (no interface) |
| 23 | min literal on a number | `RuleFor(x => x.Salary).GreaterThanOrEqualTo(0m)…` (`decimal?`) | `v == null or N(v) >= 0` | EXPRESSED |
| 24 | min date literal | `RuleFor(x => x.AdmissionDate).GreaterThanOrEqualTo(new DateTime(2020,1,1))…` | `v == null or T(v) >= T("2020-01-01")` | EXPRESSED |
| 25 | max literal on a number | `RuleFor(x => x.Salary).LessThanOrEqualTo(500000m)…` | `v == null or N(v) <= 500000` | EXPRESSED |
| 26 | max date literal (today / now) | `RuleFor(x => x.AdmissionDate).LessThanOrEqualTo(DateTime.Today)…`; `…(DateTime.UtcNow)` | `v == null or T(v) <= T(lit)` | OBLIGATION O-DATE |
| 27 | gt literal, `decimal`: empty fails | `RuleFor(x => x.MonthlyRate).GreaterThan(0m)…` | `N(v) > 0` (v never null; empty → 0) | EXPRESSED (architecture.md:770-771) |
| 28 | gt date literal | `GreaterThan(<date>)` on `DateTime?` | `v == null or T(v) > T(lit)` | GAP: nullable empty now passes; at `195d3a1` `gt` failed on empty (`rule-engine.ts`). Design-accepted (architecture.md:772-775); no sandbox use. At `f36f62e` an ordered rule passes on nothing bound (FormWriter.cs:158-168) |
| 29 | lt literal, equal fails | `RuleFor(x => x.MaxDeposit).LessThan(1000000m)…` (`decimal`) | `N(v) < 1000000` (empty → 0 passes, as today) | EXPRESSED |
| 30 | lt date literal (before today) | `RuleFor(x => x.Value).LessThan(DateTime.Today)…` (`DateTime?`) | `v == null or T(v) < T(lit)` | OBLIGATION O-DATE |
| 31 | gt peer date; equal fails | `RuleFor(x => x.DischargeDate).GreaterThan(x => x.AdmissionDate)…` | `v == null or (m.admissionDate != null && T(v) > T(m.admissionDate))` (FV fails on a null peer, AbstractComparisonValidator.cs:77) | EXPRESSED (empty v is caught first by NotEmpty, DateValidationValidator.cs:12-13) |
| 32 | ordered peer, numbers | `GreaterThan(x => x.Other)` on numbers | `v == null or (m.other != null && N(v) > N(m.other))` | EXPRESSED |
| 33 | lt/min/max/equalTo/notEqualTo against peer dates | `LessThan(x => x.R)`, `GreaterThanOrEqualTo(x => x.A)`, `LessThanOrEqualTo(x => x.R)`, `Equal(x => x.A)`, `NotEqual(x => x.R)` | ordered as 31; Equal `T(v) === T(m.a)` (null equals null); NotEqual negated | GAP: FV Equal/NotEqual do not skip null (EqualValidator.cs:47-58, NotEqualValidator.cs:44-54): empty v vs a set peer now fails Equal; both empty now fails NotEqual. At `195d3a1` both skipped when nothing was entered (`rule-engine.ts`); at `f36f62e` Equal and NotEqual compare with no skip, as FluentValidation does (FormWriter.cs:216-220) |
| 34 | equalTo literal | `Equal(5)` | `N(v) === 5` | GAP: empty now fails; skipped at `195d3a1` (`rule-engine.ts`), compared with no skip at `f36f62e` (FormWriter.cs:216-220) |
| 35 | equalTo peer text (confirm email) | `RuleFor(x => x.ConfirmEmail).NotEmpty()…` then `.Equal(x => x.Email).WithMessage(…)` | `v === m.email` | EXPRESSED (NotEmpty precedes, so empty never reaches Equal) |
| 36 | equalTo peer in a nested child | `RuleFor(x => x.Address).SetValidator(new NestedBugAddressValidator())`; child `RuleFor(x => x.ConfirmCity).Equal(x => x.City)` | field `…__Address_ConfirmCity`; `v === m.address.city` | OBLIGATION O-PEER, O-PFX (both-empty: null equals null, unchanged) |
| 37 | equalTo peer inside a collection item | `RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator())`; item `RuleFor(x => x.ConfirmSku).Equal(x => x.Sku)` | per rendered i: field `…Lines_i__ConfirmSku`; `v === m.lines[i].sku` | GAP: ConfirmSku has no NotEmpty (ValidationShowcaseValidator.cs:130): Sku set + ConfirmSku empty now fails; passed at `195d3a1` (`rule-engine.ts`), fails at `f36f62e` (FormWriter.cs:216-220). Needs O-IDX, O-PEER |
| 38 | notEqual literal text | `RuleFor(x => x.Status).NotEqual("deleted")…` | `v !== "deleted"` (null passes in FV too) | EXPRESSED |
| 39 | notEqualTo peer text | `RuleFor(x => x.AlternateEmail).NotEqual(x => x.Email)…` | `v !== m.email` | GAP: both empty now fails (FV `Equals(null,null)`); passed at `195d3a1` (`rule-engine.ts`), fails at `f36f62e` (FormWriter.cs:70-71, 216-220); NewRuleTypesValidator.cs:20 |
| 40 | non-required rules skip empty | `Matches`, `GreaterThanOrEqualTo`, `LessThanOrEqualTo` on `string?`/`decimal?` | every such check starts `v == null or …` | EXPRESSED |
| 41 | first failing rule per field wins | two `RuleFor(x => x.DischargeDate)` statements | tests in rule order × component order | OBLIGATION O-FIRST |
| 42 | `When(x => x.Flag)` | `When(x => x.IsEmployed, () => RuleFor(x => x.JobTitle).NotEmpty()…)` | when `m.conditional.isEmployed` (via O-PFX) | EXPRESSED |
| 43 | `When(x => x.Field == value)` | `When(x => x.CareLevel == "Memory Care", …)` | when `m.careLevel === "Memory Care"` | EXPRESSED |
| 44 | `When(x => !x.Flag)` | `Unless(x => x.HasEmergencyContact, …)` | when `!m.hasEmergencyContact` | EXPRESSED |
| 45 | `When(x => x.Field != value)` | `When(x => x.CareLevel != "Independent", …)` | when `m.careLevel !== "Independent"` (null → true, as today) | EXPRESSED |
| 46 | `When(x => x.Field > value)` | `When(x => x.F > k, …)` | when `m.f > k` | OBLIGATION O-LIFT |
| 47 | `When(x => x.Field >= value)` (`int Age`) | `When(x => x.Age >= 18, …)` | when `m.age >= 18` (empty → 0 → false, as today) | EXPRESSED |
| 48 | `When(x => x.Field < value)` (`int Age`) | `When(x => x.Age < 18, …)` | when `m.age < 18` | GAP: `int Age` (OperatorConditionModels.cs:5) reads 0 when empty, so the rule fires on an empty Age; at `195d3a1` a missing value compared false (`compare-engine.ts`). At `f36f62e` a condition reads its members as bound (Alis.Reactive/Validation/ClientCondition.cs:253-258), so an empty `int Age` is 0 (shape.ts:47) and the rule fires, as on the server. Frozen tests still pass (WhenNewOperatorConditionsWork.cs fills Age where asserted) |
| 49 | `When(x => x.Field <= value)` | `When(x => x.F <= k, …)` | when `m.f <= k` | OBLIGATION O-LIFT |
| 50 | `When(x => x.Field == null)` | `When(x => x.F == null, …)` | when `m.f == null` (loose) | OBLIGATION O-LIFT |
| 51 | `When(x => x.Field != null)` | `When(x => x.F != null, …)` | when `m.f != null` (loose) | OBLIGATION O-LIFT |
| 52 | `When(x => string.IsNullOrEmpty(x.Field))` | `When(x => string.IsNullOrEmpty(x.F), …)` | when `m.f == null or m.f === ""` | OBLIGATION O-STR |
| 53 | `When(x => !string.IsNullOrEmpty(x.Field))` (top level and nested) | `When(x => !string.IsNullOrEmpty(x.Email), …)`; in `NestedBugAddressValidator` `When(x => !string.IsNullOrEmpty(x.City), …)` | when `!(m.email == null or m.email === "")`; nested `m.address.city` | OBLIGATION O-STR, O-PFX |
| 54 | in a fixed set (`values.Contains(x.Field)`) | `When(x => new[] { "memory-care", "skilled-nursing" }.Contains(x.CareLevel), …)` | when `["memory-care","skilled-nursing"].includes(m.careLevel)` | OBLIGATION O-STR |
| 55 | not in a fixed set | `When(x => !new[] { … }.Contains(x.F), …)` | negation of 54 | OBLIGATION O-STR |
| 56 | between (`>=` and `<=` joined by `&&`) | `When(x => x.F >= lo && x.F <= hi, …)` | when `m.f >= lo && m.f <= hi` | OBLIGATION O-LIFT |
| 57 | text contains | `When(x => x.Notes != null && x.Notes.Contains("urgent"), …)` | when `m.notes != null && m.notes.includes("urgent")` | EXPRESSED (§6 string table; the null guard is now the author's, as OperatorConditionValidators.cs:25 writes it; `WhenFieldContains` was null-safe on both sides at `195d3a1`, in `FieldConditionBuilder.cs`, deleted at `b966c57`) |
| 58 | text starts with | `When(x => x.F != null && x.F.StartsWith(p), …)` | `startsWith` | EXPRESSED |
| 59 | text ends with | `When(x => x.F != null && x.F.EndsWith(s), …)` | `endsWith` | EXPRESSED |
| 60 | text matches a pattern | `When(x => x.F != null && Regex.IsMatch(x.F, p), …)` | when `matches(m.f, p)` | OBLIGATION O-STR |
| 61 | a length condition (`WhenFieldMinLength` at `195d3a1`) | `When(x => x.F != null && x.F.Length >= n, …)` | when `m.f.length >= n` | EXPRESSED. At `f36f62e` `x.F.Length` is a member .NET defines, not a field the page binds, so the condition has no browser form and its rules stay on the server (BrowserCondition.cs:253-268; ClientScope.cs:29-33) |
| 62 | a collection field holds an item | `When(x => x.Tags != null && x.Tags.Contains("fall-risk"), …)` | when `m.tags.includes("fall-risk")` | EXPRESSED (§4.3 `Contains`) |
| 63 | conditions joined with `&&` | `When(x => x.TakesPainMedication && x.PainLevel > 7m, …)` | when `m.takesPainMedication && m.painLevel > 7` (`decimal`, empty → 0, as today) | EXPRESSED |
| 64 | conditions joined with `or`, or negated with `!` | `When(x => a or b, …)`, `When(x => !(…), …)` | `op` | EXPRESSED |
| 65 | section hidden by a reaction, rule on the same flag | `When(x => x.IsVeteran, () => RuleFor(x => x.VeteranId).NotEmpty()…)` | when `m.isVeteran` | EXPRESSED |
| 66 | activation re-evaluated every validation | n/a | `when(m)` evaluated per evaluation | OBLIGATION O-EVAL |
| 67 | parent condition around child with its own condition | `When(x => x.ParentFlag, () => RuleFor(x => x.Child).SetValidator(new ChildSectionValidator()))`; child `When(x => x.ChildFlag, …)` | when `m.parentFlag && m.child.childFlag` | OBLIGATION O-WHEN, O-PFX |
| 68 | rules under FV `When`/`Unless`/`WhenAsync` stay server-only | `When(model => model.ServerFlag, () => RuleFor(..).NotEmpty())` (the plan harness at `195d3a1`; `When(IsServerFlagged, ..)` at ClientValidationRulePlanHarness.cs:245 at `f36f62e`); `When(x => x.CareLevel == "Memory Care", …)` (FusionStepperWizardModels.cs:100) | now projected: when `m.serverFlag` / `m.careLevel === "Memory Care"` | GAP: an expression `When` projects any translatable predicate, so the rules `195d3a1` kept on the server reach the browser (ServerFlag has no input → `undefined`; `!x.ServerFlag` would fire in the browser). Server-only then needed `WhenAsync` or the `(x, ctx)` overload, and an untranslatable `When` was to fail at render (architecture.md:848) instead of staying server-only (architecture.md:759-761). At `f36f62e` a method group, a context or an asynchronous condition keeps its rules on the server (RecordingValidator.cs:31-61), and so does a lambda with no browser form (ClientScope.cs:29-33); nothing fails at render |
| 69 | `ClientRule` inside FV `When` throws with guidance | n/a (no `ClientRule`) | none | RENDER: §6 render-time failure naming an untranslatable construct; analyzer on rule-level `.When` (architecture.md:759-761). At `f36f62e` an untranslatable construct stays on the server (ClientScope.cs:29-33) and ALIS006 warns on a rule-level `.When` (ServerOnlyConditionAnalyzer.cs:17-22) |
| 70 | child validator, prefixed paths | `RuleFor(x => x.Address).SetValidator(new ResidentAddressValidator())` | fields `…__Address_Street` etc.; checks as rows 1/20 | OBLIGATION O-CHILD, O-PFX |
| 71 | `RuleForEach..SetValidator` onto every rendered item | `RuleForEach(x => x.Lines).SetValidator(new ValidationOrderLineValidator())` | one test set per rendered item i | RENDER: O-IDX expansion over rendered item inputs |
| 72 | nested `RuleForEach` in an item validator + per-item condition | item `When(x => x.GiftWrapped, …)`, `RuleForEach(x => x.Deliveries).SetValidator(..)`; delivery `When(x => x.RequiresAccessCode, …)` | when `m.lines[i].giftWrapped`; `m.lines[i].deliveries[j].requiresAccessCode` | OBLIGATION O-IDX (nested), O-WHEN |
| 73 | `RuleForEach` inside a condition | `When(x => x.HasContacts, () => RuleForEach(x => x.Contacts).SetValidator(new EmergencyContactValidator()))` | item tests, when `m.hasContacts` | OBLIGATION O-WHEN, O-IDX |
| 74 | at least one on the collection itself | `RuleFor(x => x.Contacts).NotEmpty()` | needs `m.contacts.length > 0`, but a test is keyed by an input (`field`, `value`) | GAP: a collection of complex items has no input (architecture.md:784-785, 915-918); the only lowering is an always-unmounted field that blocks with a summary entry, which was the same defect at `195d3a1` (`ReactiveClientRuleBuilder.cs` bound it to a nonexistent `…__Contacts` id). At `f36f62e` the rule projects `atLeastOne` onto the collection's own id (ClientRuleProjection.cs:86-88; ClientRule.cs:170-173), which no element carries, so the form judges it as an unmounted field (form.ts:125-137): the gap stands |
| 75 | same-type rules imported inside a condition | `When(x => x.IsEmployed, () => Include(new SharedEmploymentRulesValidator()))` | included tests, when `m.isEmployed` | OBLIGATION O-CHILD (IncludeRule), O-WHEN |
| 76 | rules imported from a base-type validator | `Include(new BasicSectionValidator())` (`IValidator<in T>`; `AllRulesSection : BasicSection`) | Name/Email tests at the section prefix | OBLIGATION O-CHILD; the server now also enforces them (at `195d3a1` `ClientRulesFrom<TSource>` added no server rule; at `f36f62e` the statement is `Include(new BasicSectionValidator())`, ValidationShowcaseValidator.cs:40, which FluentValidation runs on the server too) |
| 77 | app-level rules via `AddReactiveClientValidation` | none: deleted (architecture.md:786-788) | none | GAP: removed (`ReactiveClientValidationServiceCollectionExtensions.cs` and `ClientValidationRulesBuilder.cs` were deleted at `b966c57`); rules must be rewritten as a `ReactiveValidator<T>` registered through `AddReactiveFluentValidation` (ReactiveFluentValidationServiceCollectionExtensions.cs:14), and client validation without FluentValidation disappears |
| 78 | assembly-scanned validators resolved through DI | `AddReactiveFluentValidation(..)` | none | RENDER: `Validate<TValidator>` resolves TValidator from DI at render and projects it |
| 79 | DI validator with an injected service | same | none | RENDER: same resolution, children through O-CHILD |
| 80 | registrations merge; plain `AbstractValidator` stays server-only | same | none | RENDER: only `ReactiveValidator<T>` is projected (a plain validator's descriptors are equally readable, so the restriction must be explicit) |
| 81 | unregistered source fails with guidance | n/a | none | RENDER: render-time throw naming the type (ReactiveFluentValidationServiceCollectionExtensions.cs:130-133) |
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
| 115 | wired from boot: leaving an invalid field before any submit shows it | n/a | n/a | GAP: needs O-LIVE (subscribe at boot), and a single `Committed` per input cannot carry the blur + change pair `195d3a1` wired on every field (`live-clear.ts`): an input whose `Committed` is `change` (112-114, architecture.md:916) no longer revalidates when left untouched. At `f36f62e` the form wires DOM `blur` on every field beside the input's re-raised `Committed` event (form.ts:211-228; model.ts:160-175), when the form is created (form.ts:248-255) |
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
  `v == null or matches(v, "^https?://.+")`, from `rule-engine.ts` at `195d3a1`); the server half stays `IsEmptyOrHttpUrl`
  (`ReactiveClientRuleBuilder.cs` at `195d3a1`). At `f36f62e` a non-generic `IBrowserRule` names a `BrowserRule`
  (BrowserRule.cs:22-40); `Url()` is one validator for both halves (ReactiveRuleBuilderExtensions.cs:15-19, 36-47), and its
  `url` check is written as that expression (FormWriter.cs:61).
- **O-LUHN.** `ICreditCardValidator` projects to one fixed expression: `d = v.split("").filter(c => c >= "0" && c <= "9")`,
  `d.length >= 13 && d.reverse().reduce((s, c, i) => s + (i % 2 ? (2*N(c) > 9 ? 2*N(c) - 9 : 2*N(c)) : N(c)), 0) % 10 === 0`
  (`rule-engine.ts` at `195d3a1`; no operator does Luhn). At `f36f62e` the form builds it as FluentValidation
  checks a card (FormWriter.cs:126-156): dashes and spaces dropped, every other character a digit, the Luhn sum a multiple of ten.
- **O-DATE.** A date literal is `ValueToCompare` as captured when the validator was constructed, serialized exactly as
  `ValidationDateLiteral` does (`yyyy-MM-dd` at midnight, else `"s"` with no offset; ValidationDateLiteral.cs:18-35). When
  projection runs must be stated: at `195d3a1` it was once per app (a singleton), so `DateTime.Today` was frozen at
  first resolve. At `f36f62e` the rules are read from the validator resolved when the page renders
  (ReactiveFluentValidationServiceCollectionExtensions.cs:115-137), and validators are registered transient
  (ReactiveFluentValidationServiceCollectionExtensions.cs:87-93).
- **O-PEER.** Peer read = rule prefix (including concrete item indexes) + `MemberToCompare.Name`; render throws when
  `MemberToCompare.DeclaringType` is not assignable from the validator's `T` (a `MemberInfo` carries no path,
  AbstractComparisonValidator.cs:114). Ordered comparison fails when the peer is null (FV, :77). At `f36f62e`
  ClientRuleProjection.cs:203-217 reads the peer as a sibling and refuses any other at render.
- **O-FIRST.** Tests are emitted in validator rule order (an `Include`/`SetValidator` inlined at its position), then component
  order. Per field: skip tests whose `when(m)` is false; the first whose `check(v, m)` is false decides and later tests are not
  evaluated (form.ts:109-121 at `f36f62e`).
- **O-WHEN.** `ReactiveValidator<T>` declares `When(Expression<Func<T,bool>>, Action)` and `Unless(..)`, pushes the expression
  during `action`, overrides `OnRuleAdded(IValidationRule<T>)` (AbstractValidator.cs:393-399, present in 11.12 and 12.1.1) to
  tag each rule with the AND of the stack, and calls `base.When(compiled, action)` for the server. Projection skips a rule with
  `HasAsyncCondition`, a rule with `HasCondition` and no tag, and any component with `HasCondition`/`HasAsyncCondition`
  (rule-level delegate). The `new When(Func…)` overloads `ReactiveValidator<T>` declared at `195d3a1` must go, or a lambda is
  ambiguous between them (CS0121; not compiled here). At `f36f62e` those overloads live on the base class
  (RecordingValidator.cs:31-61), so a lambda binds to ReactiveValidator.cs:58-63 (C# prefers the most derived method,
  RecordingValidator.cs:16-19), and the projection's skips are ClientRuleProjection.cs:276-288.
- **O-PFX.** A child reached through `SetValidator`/`Include` at member path p: field id = `IdGenerator.For(rootModel, p.path)`;
  each `x.M` read in check, when or peer becomes `m.<p>.m`; the test's when = AND of every enclosing when, outer first.
- **O-IDX.** `RuleForEach(x => x.C).SetValidator(V)`: recognized by `rule.Expression.ReturnType != rule.TypeToValidate`
  (CollectionPropertyRule.cs:66; at `f36f62e` ClientRuleProjection.cs:290-292); for each input in the render whose binding name starts with `C[i]`
  (FormChecks.cs:79-105), emit V's tests with prefix `C[i]` (O-PFX), recursively; the index is literal in
  the field id and in every model path.
- **O-CHILD.** For `IChildValidatorAdaptor` (it exposes only `ValidatorType`, ChildValidatorAdaptor.cs:8-13), resolve
  `ValidatorType` from the request's `IServiceProvider` (validators are registered transient by concrete type,
  ReactiveFluentValidationServiceCollectionExtensions.cs:87-93); unresolvable → render throws naming it. `IIncludeRule`'s one
  component is a `ChildValidatorAdaptor`; prefix "". At `f36f62e` the child validator is read as `SetValidator` or `Include`
  holds it, with no service lookup, and one a factory chooses per model stays on the server instead of failing the render
  (ClientRuleProjection.cs:236-274).
- **O-LIFT.** If an operand's C# type is `Nullable<T>`, `a < b` (and `<=`, `>`, `>=`) lowers to `a != null && b != null && a < b`
  (JavaScript gives `null >= 0 === true`). `== null`/`!= null` are loose (null and undefined, as `compare-engine.ts` was at
  `195d3a1`). At `f36f62e` the operator table gives both answers: an ordering against a missing value is false and a
  missing value equals only another (operators.ts:1-3, 9-13, 29-38).
- **O-STR.** Compiler entries missing from §6: `string.IsNullOrEmpty(s)` → `s == null or s === ""`;
  `Regex.IsMatch(s, constant)` → `matches`; `Enumerable.Contains` on a closure-rooted `new[] {…}` → literal `.includes(x)`.
- **O-EVAL.** `when(m)` and `check(v, m)` are evaluated against current values on every `validate()` and every field
  revalidation; nothing is cached.
- **O-ORDER.** The request builder emits `if (form.validate())` outermost, before WhileLoading statements
  (WhenAdmittingNewResident.cs:154).
- **O-SCOPE.** Form k's tests = `forms[k].tests` of every composed plan. Its model `m` = values of all inputs of the composed
  plan by binding name (not only inputs tagged `form: k`, which a partial cannot know). `validate()`, revalidation and live
  wiring apply a test only when its field element is inside `getElementById(k)` (form.ts:102-105). `validate()` returns
  false when `getElementById(k)` is null and the form has tests (form.ts:140-147).
- **O-MOUNT.** A test field with no input or no element at evaluation: if every test of that field has a false `when(m)` (a
  `when` that throws counts as false), skip it; otherwise the form is invalid and the summary gets one entry keyed by the field
  with the message of that field's first test in plan order, active or not (`orchestrator.ts` at `195d3a1`). At `f36f62e`
  the active tests run against the missing value, a length or pattern test passes on it, and the first that fails gives the
  summary message (form.ts:125-137). Live wiring waits for the element (form.ts:230-241).
- **O-DISPLAY.** Inline is renderable when `getElementById(id + "_error")` has a parent and neither the parent nor an
  ancestor has the `hidden` attribute or inline `style.display === "none"` (display.ts:57-64). Failing and
  renderable: slot text = message, remove `hidden`, clear `display`, add `alis-has-error` to `getElementById(id)`, remove the
  field's summary entry. Failing and not renderable: summary entry (one `div[data-valmsg-summary-for=key]` per key, replaced,
  appended) and summary shown. Summary = `getElementById(planId with "." and "+" → "_" + "_validation_summary")` of the plan
  that declared the form (form.ts:62-65; PlanExtensions.cs:170-171); missing summary is a no-op. `validate()` first
  clears every test field's inline slot and class, then empties and hides the summary (form.ts:149-151); it evaluates
  every field and returns the AND.
- **O-SRV.** `showErrors(body)`: the same clearing first; body not an object → nothing; `body.errors` not an object → warn,
  nothing; per `[name, messages]`: message = array ? `join(", ")` : string; target = a test field of this form whose input
  `name === name` exactly; no target, no element or slot not renderable → summary entry keyed by `name`; else slot text and
  `alis-has-error` on the element (form.ts:183-207; display.ts:66-79). At `f36f62e` a body that is not an object runs client
  validation instead (form.ts:176-181).
- **O-LIVE.** At form creation (boot or slot load, not first `validate()`), per test field with an element, once per element:
  DOM `input` on `getElementById(id)` → clear its inline slot and class only (form.ts:216, 218); the input's `Committed`
  event → revalidate: clear inline, remove its summary entry and hide an empty summary, stop if the container is missing, else
  evaluate that field's tests only (form.ts:161-173). Peers and dependants are not revalidated. At `f36f62e` DOM `blur`
  revalidates too (form.ts:219).
- **O-COMMIT.** Text-like inputs declare `Committed` = blur (Native DOM `blur`; EJ2 TextBox `blur`), since today's blur
  revalidation is what 110/111 show and `change` does not fire on leaving an unchanged field.
- **O-OWN.** Composition concatenates `forms[k].tests` and `inputs` per owner; a slot unload removes exactly that slot's tests,
  inputs and the form's subscriptions on them, and keeps boot-owned ones (slot.ts:54-60; form.ts:257-261; model.ts:149-153); a reload subscribes to the
  new elements.
- **O-GRID.** `FusionGridValidation` reads FV descriptors directly, not the form's tests: unconditional components only;
  NotEmpty → `required`, IEmailValidator → `email`, Url `IBrowserRule` → `url`, ILengthValidator → `minLength`/`maxLength`,
  IRegularExpressionValidator → `regex`, GreaterThanOrEqual/LessThanOrEqual literal numbers → `min`/`max`,
  IInclusiveBetweenValidator numbers → `range`; the rest emit nothing (FusionGridValidation.cs:104-154). Its source at `195d3a1`,
  `IClientValidationRuleSource`, was to be deleted (architecture.md:786-788); at `f36f62e` it remains, and
  `FusionGridValidation.From` read the projected rules through it; at `6ac4198` it still remains, and `From` takes the
  rules `plan.ClientRules<TValidator>()` reads through it (FusionGridValidation.cs:33-38; ReactivePlan.cs:120-122; IClientValidationRuleSource.cs:11-16).
