# ValidationForms: every validation use case, its complete form

Base 830817f4b: `git -C ../validation-forms rev-parse --short=9 HEAD` printed "830817f4b". A cite is a file and line at the base, read with
`git show`. DESIGN is spec/Core/DESIGN.md, schema is spec/Core/schema.md, API is
Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt, App is tests/Alis.Reactive.Conformance.App, Tests is
tests/Alis.Reactive.Conformance.PlaywrightTests, facts is spec/probes/facts.json, forms.ts, model.ts and request.ts are
Alis.Reactive.Assets/runtime/Validation/forms.ts, runtime/InputsAndIds/model.ts and
runtime/RequestsAndResponses/request.ts. A count a script of mine made from a command's lines is marked "parse"; every
other count is the line its command printed. Nothing here was run in a browser, and no build or test ran: each new
promise's plant is unrun. What was read whole and what in part is section 11.

## 1. Every case on one screen

| Case | Stands | What the developer writes | What the plan carries (schema.md) | Proof |
|---|---|---|---|---|
| F1 | built | the request alone | `let`, `if`, `stop` and a `call` of `validate` over plugin data `form` (:146) | M9.X66, M9.X46. The test use-cases.md cites, X16.2, proves F1e |
| F1e | built | `.Validate<TValidator>()` | the same | X16.2 |
| F2 | built | `.Validate<TValidator>(ruleSet)` | the same, its registration keyed by the rule set | X16.22; M9.X20 with M9.X21 |
| F3 | built as two doors no request of the app writes together | `.NoValidation()`; `ShowValidationErrors` | no check; a `call` of `showErrors` | M9.X225 (its plant unrun); X16.16, X8.3, X10.12. The two on one request: unproved |
| F4 | built | `pipeline.Validate(container)` | `let`, `call`, `if`, `stop` | M9.X21 |
| F5 | new door | `pipeline.Validate(container, ruleSet)` | the same nodes, the registration keyed by the rule set | X16.48, to write; TLC on a copy |
| F6 | built | nothing | `Container.live` (:165) | X16.20, X16.41; "typing clears" has no test I found |
| F7 | designed, not built: no case of this document shows a difference in meaning | (its form, section 4.5) | `Container.live`, which holds none or a `Live` today | none |
| F8 | built | `Must`, `MustAsync`, `.ServerOnly()`, `WhenOnServer` | no browser rule | X8.3 beside the input; M9.X47 in the notice |
| P1 | built | `Html.PartialFor` and the parent's `SetValidator` | nothing of the partial's own | M9.X21, X14.8 |
| P2 | built | the partial's own plan, container and request | as F1 | X16.5, M9.X9, M9.X66 |
| B1 | built | F1 or F1e over the form | as F1 | Cells.validation_listItem, X13.3 |
| B1s | new refusal; the validator's own line | F2's door, and the rule for each row declared in the set | as F2 | X16.50, X16.49, to write |
| B2 | new meaning, no new door | `request.Inputs(shift.Container)` in the row's partial | `Container` gains `prefix`; `SentInput` gains `path` | X16.51, to write; TLC on a copy |
| B2s | B2 with F2's door | `.Validate<ShiftRules>(ShiftRules.Approval)` | as B2 | X16.52, to write |
| B2c | B2 with F4's door, F5's under a set | `pipeline.Validate(shift.Container)` | as F4 | X16.53, to write |
| B3 | built for the form; the row's own container by B2's meaning | nothing | `Container.live` | X16.20 for two containers of one view; X16.54, to write |
| B4 | built | a container of the edit's model, the item's validator on the new member | as F1 | M9.X40, M9.X41; the new row's own failing rule has no case |
| B5 | built | the row's action dispatches its id; `.Validate<TValidator>(ruleSet)` | as F2 | M9.X50, M9.X51 |
| G1, G2 | no core door: module 21's | the grid's own editor (built) and `FusionGridValidation` (specified, not built); the save by landed doors | as F1 over a `Set` member | none checks a grid's edit today |

New, in all: one door (F5), one meaning with two fields of the plan's contract (B2), one refusal (B1s), one defect
fixed by the same field (section 2.4). No node of the grammar is added.

## 2. What the source states, each read before a case was decided

### 2.1 How a rule set reaches a child validator

A check carries one selection, the rule set's name. A rule runs when the selection is "*", when it is declared in no
set and the selection is "default", or when its sets hold the selection, case ignored
(Alis.Reactive/Validation/Rules.cs:22-27); an unselected rule returns before its checks (:111). A rule that holds a
child validator hands the child the same run, so the same name selects the child's rules (:384-385, :34-39). The
browser's side builds its registration with the same selection: an unselected rule writes no group
(Rules.cs:121), and a child's scope keeps the parent's selection (Alis.Reactive/Validation/BrowserRules.cs:89,
:121-122, :326). `ChildRules` differs from `SetValidator` in one thing: its inline validator takes the holding
rule's sets (Alis.Reactive/Validation/RuleBuilderExtensions.cs:439-442), where a validator given to `SetValidator` keeps
the sets its own rules declare. A set's name is split on a comma or a semicolon and trimmed (Rules.cs:51).

So `.Validate<RosterRules>(ShiftRules.Approval)` over use-cases.md's `RosterRules`, whose rule for each row is declared
in no set, runs no rule: the holding rule is not selected, so `ShiftRules` is never reached, on both sides alike. The
two verdicts agree, on a pass. The app met this (spec/Reference/move-in.md:1424: "A nested validator's rules run under
the rule set the check selects ... My first version missed it and M9.X21 caught it in the full run."), and
spec/CLAUDE.md:1452-1453 holds it open: "the refusal of a rule set the validator does not declare, which needs its X
line before U17". Section 4.3 closes it once, for every door that names a rule set.

### 2.2 What a check of a part reads for a member the part does not hold, and where its message goes

`pipeline.Validate(container)` builds the registration for the default rule set, which the code fixes
(Alis.Reactive/Validation/FormChecks.cs:30), checks the container's captured inputs as form data (:36, :47), places
its failures in that container's summary (:51-55) and stops its pipeline on any failure (:40-41). The browser binds
the captured inputs over the model's start (forms.ts:202-216), so a member no input of the container holds reads its
start: D16.5, DESIGN:3945, "A member the body lacks reads its starting value (D16.6)". A failure is named by the sent
input at its path, and by "" where none is (forms.ts:186-188); every message goes to the summary (forms.ts:100,
:107-112) and only a failure that names a sent input shows beside one (forms.ts:101-104).

So F4's door over the identity fieldset fails on Bed: "Choose a bed." shows in the fieldset's summary, beside no
input, and the pipeline stops. That is D16.13 as written (DESIGN:4065-4066, "Every failure of a check ... shows in the
request's summary place: the summary of the container it sends"). A check of a part therefore needs one of two
things: the part as its own model with its own validator, which D16.2 states (DESIGN:3901-3902, "A part of a model
saved alone is its own model with its own validator"), or a rule set that holds the part's rules alone. The second is
F5's door. The app met the gap (move-in.md:1440: "`pipeline.Validate(container)` takes none, so MI.3.1 checks the whole
container under the default set, the location's rules with it; M9.X21 chooses a bed first").

### 2.3 A container under a field prefix, today

An input's binding carries MVC's full name and a path that opens with its view's prefix
(Alis.Reactive/InputsAndIds/BoundInput.cs:53, :151-157; D14.3, DESIGN:3582-3584). A container's send gathers every
input inside its element, whichever plan rendered it, and writes each under its full path (model.ts:112, :128), and a
form-data body names each by its full name (request.ts:187-200). A container's block is typed by its view's model
(D14.8, DESIGN:3683-3686; Alis.Reactive/RenderingAndDelivery/RequestBlocks.Net10.cs:26-28), and its `Read` takes its
steps from the lambda alone (Alis.Reactive/InputsAndIds/ContainerReads.cs:18).

So a container block written in a partial of `ShiftRow` that `Html.PartialFor(m => m.Rows[at], ..)` renders is a
`RequestContainer<ShiftRow>` that sends "Rows[2].Carer": an endpoint that binds a `ShiftRow` binds nothing of it, the
check under `ShiftRules` reads every member at its start, and `shift.Container.Read(m => m.Carer)` reads the start
and never the input. The two verdicts agree, on the start, and what the user typed reaches neither. No line of DESIGN
states this case. No page reaches it, by a script over the app's 915 views (`git ls-tree -r` of App, the Net48 twin's
views among them): 33 are rendered under a field prefix, the views a `PartialFor(` or an `EditorFor(` names, the 3
that set `HtmlFieldPrefix` themselves, and the 1 one of those renders by `PartialAsync`
(Care/MoveIn/ConfirmedDiet.cshtml, from Diet.cshtml), followed until none was added; none of the 33 holds a
`BeginRequestContainer(` or a `BeginRequestForm(`. The script reads a view named by a text in the call: all 30
`PartialFor(` lines and all 6 `EditorFor(` lines name one, no view writes `<partial `, and no .cs file sets
`HtmlFieldPrefix`. A partial a slot loads is rendered by its own request, under no prefix. The two Consumer apps
and tests/Alis.Reactive.Poc.App hold no container block at all (`git grep -l` printed 0 files). The app's 147 block
lines each write a literal id, so none is rendered once a row.
ContainerRead.tla does not hold the case either: its six invariants pass today's reading (section 6).

### 2.4 An `Input` sent under a member whose path differs from its own: a defect, read and not run

D9.15 (DESIGN:1921-1922): "`Input(r => r.Member, pipeline.Input(m => m.Member))`: one input of the request's own view,
wherever it sits, sent under the request member's binding name." D16.13 (DESIGN:4071-4073) shows a failure "at the
message element of the framework input the request sent under that name ... an input inside its container, or its
`Input`", and D16.7 (DESIGN:3988-3990) fails a text the member's type cannot hold before its rules.

The code records such an input under the request member's name
(Alis.Reactive/RequestsAndResponses/RequestPartsWriter.cs:91, :103) and its check entry under the request member's
path (:307-311), and the browser's check then keys both the typed text and the failure's name by the input's own
binding path (forms.ts:152-153, :186-188). Where the two paths are one, every page of the app, they meet. Where they
differ, as `request.Input(shift => shift.Carer, pipeline.Input(m => m.Rows[at].Carer))` differs, the browser's own
failure is named "" and shows in the summary place alone, and a text the member cannot hold is not refused, against
both lines. The server's 400 does show beside the input, since it is matched by name (forms.ts:101-103).

The app's reach: none. `git grep -n -E "\.Input\(" 830817f4b -- tests/*.cshtml`, less the reads of a component, printed
44 lines, all read: in each request `Input` the two lambdas name one path (the two under a prefix,
App/Refusals/InputsAndIds/MoveInPrefixAfterItsPlan.cshtml:20 and MoveInPrefixBeforeItsPlan.cshtml:20, send
`r => r.NextOfKin.Name` from an input named NextOfKin.Name). Not read: the module tests and every .ts test. The cause is
read from the four places cited; no test was run on it. The field B2 adds, the path an input is sent under, fixes it
(section 4.2), and X16.55 holds it.

### 2.5 A list whose first index is not 0

No form of this design sends one: a row saved alone is sent as its own model, under no index. The facts stand for the
form a developer must not write, a container of the page's model around one row, which sends "Rows[2].Carer" alone.
MVC 5 ends a list at the first index no key names (facts:1597, `mvc5-list-ends-at-missing-index`,
spec/probes/aspnetwebstack/DefaultModelBinder.cs.txt:683), and it reads form, route and query through one binder
(facts:1948). net10 is pinned for a query alone (facts:1237, `core-query-list-of-objects-gap`: "Lines[1].Name=B binds
Lines=[]"); facts:103 pins a form's two indexes from 0. A net10 form body whose first index is not 0 has no fact.
Its probe, if a unit ever rests on it: statement "A form's list of objects ends at the first missing index, and one
whose first index is not 0 binds empty.", host net10, the line it must print
"core-form-list-of-objects-gap: Lines[0].Name=A&Lines[2].Name=C binds Lines=[A] | Lines[1].Name=B binds Lines=[]".
No page writes that form (the 147 literal ids of 2.3: no block is rendered once a row).

### 2.6 What the app's views write

- `git grep -c` over App's views: `.Validate<TValidator>()` on 62 lines, `.Validate<` of either form in 55 views; `.Validate<TValidator>(ruleSet)` on
  9 lines (Care/MoveIn/DraftCommands.cshtml:45, :77, :122; PharmacyReview.cshtml:43; ResidentDesk.cshtml:32;
  Care/Record/CareRota.cshtml:21, :25; EarlyCareRota.cshtml:21, :25); `NoValidation()` on 16; a check with no
  request on 1, Care/MoveIn/ResidentDesk.cshtml:13.
- Parse: of the 295 calls written `.Verb<Model>(` on one line in App's views, 78 are over a model one of the 62
  `Add<TValidator>()` lines registers a validator for; 71 name a validator, 3 name no check, and 4 take the
  condensed default: Care/Admission/ArrivalDesk.cshtml:48, Care/MoveIn/AllocationDesk.cshtml:24,
  Care/MoveIn/DraftCommands.cshtml:148 and Care/MoveIn/WaitlistEntry.cshtml:21. Of the 16 requests that name no check,
  none holds `ShowValidationErrors` in its statement. The parse reads a call's statement up to the next verb call or
  25 lines; it does not read the Net48 twin's views, the Consumer apps or tests/Alis.Reactive.Poc.App.

### 2.7 Where use-cases.md differs from the source

- F1's test. `WhenAJsonSaveLeavesOutTheLeadsName` opens App/Care/Admission/HomeLead.cshtml, whose two requests each
  name `.Validate<HomeLeadRules>()` (:25, :29). It proves F1e's door. F1's proof is M9.X66 and M9.X46.
- F3. App/Care/MoveIn/AdmissionHandover.cshtml:44's `.NoValidation()` sits on a GET of a query with no validator, and
  App/Care/Admission/AddressedMoveIn.cshtml:26 writes neither door.
- F8. The case it cites, M9.X47, shows the server's message in the notice, since its request sends no container
  (Tests/PartialsAndSlots/MoveIn/WhenAContactsRoleNeedsItsAuthority.cs:98; move-in.md:1458). X8.3 shows one beside its
  input.
- G1, G2. "A search of tests/Alis.Reactive.Conformance.App for 'EditTemplate' and 'FusionGridEditTemplates' printed no
  file" does not hold: `git grep -n -E "EditTemplate|FusionGridEditTemplates" 830817f4b -- tests` printed
  App/Care/BedBoard/StatusCorrections.cshtml:19 and StatusCorrectionDialog.cshtml:11 (2 files by `git grep -c`), and
  Care/BedBoard/ChargeCorrections.cshtml:16 edits in batch.
- "The doors a check is written with": B2's open question "which door sends a container's inputs as a request's whole
  body" has its answer at the base, `request.Inputs(container)` (API:846), which
  App/Care/MoveIn/DraftCommands.cshtml:212 writes.

## 3. The cases, in use-cases.md's order

The models, the registration and the endpoint's check are use-cases.md's, unchanged, with one line changed in
`RosterRules` (B1s). Every example is an illustration over them; the exemplar each door is quoted from is named beside
it.

### F1. Save, checked by the model's validator under its default rule set: built

```cshtml illustration
@(Html.NativeButton("save", "Save").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Details, request => request.Form(resident.Form))
    .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("saved").Text.Set(saved.Body))))
```

- Meaning: D16.2, DESIGN:3908-3910, "a request whose model has a registered validator is checked by it under the
  default rule set, whatever it sends, a container or none, with its failures placed by D16.13."
  `RequestChecks.Default` writes it, and writes no check for a model with no registered validator
  (Alis.Reactive/Validation/RequestChecks.cs:38-46).
- Sends: the form's submission set as multipart fields, Name, Email, EmailConfirmation and Bed (D9.15, DESIGN:1911-1918).
- Sees: with no bed chosen, "Choose a bed." beside Bed and in the form's summary, and nothing is sent.
- Proof: `WhenACoordinatorJoinsTheWaitlist.unchecked_terms_refuse_the_join_and_preserve_the_empty_waitlist` (M9.X66). Its
  page, App/Care/MoveIn/WaitlistEntry.cshtml:21-28, names no validator and sends its container. Gesture: click Join
  waitlist with the terms unchecked. Outcome: "Accept the waitlist terms." beside its input and no request at the
  waitlist's address; its opposite joins. It proves the default check and the place beside the input; it does not
  read the summary. For a request with no container,
  `WhenAContactsRoleNeedsItsAuthority.an_authority_that_ends_before_it_starts_is_refused_before_it_is_sent` (M9.X46,
  App/Care/MoveIn/DraftCommands.cshtml:148): the notice reads the message and nothing is sent.

### F1e. Save, the validator named, its default rule set: built

`.Validate<ResidentDetailsRules>()` after the request. One check, two doors, and they differ in one state: a model with no
registered validator. The default then writes no check and says nothing (RequestChecks.cs:42-43); the named door is
refused where the view renders, "{Model} has no registered {Validator} validator. Register it with ..."
(Alis.Reactive/Validation/ValidatorCatalog.cs:37-46; X16.42 at build), and a validator of another model does not
compile (X16.1). Since a model has one validator (ValidatorCatalog.cs:55-56), the named door can name only the one
the default takes. Shown first: F1, the least a developer types; F1e is for the page that must not render unchecked.
Proof: `WhenAJsonSaveLeavesOutTheLeadsName` (X16.2), on App/Care/Admission/HomeLead.cshtml:23-30: Save with the phone
alone shows "Lead name is required." in the summary and saves nothing; with both, "Ruth, 555-0100" is saved.

### F2. Save under a named rule set: built

```cshtml illustration
@(Html.NativeButton("save-identity", "Save identity").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Identity, request => request.Form(resident.Form))
    .Validate<ResidentDetailsRules>(ResidentDetailsRules.Identity)
    .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("saved").Text.Set(saved.Body))))
```

```csharp illustration
var validation = this.Validate<ResidentDetails, ResidentDetailsRules>(resident, ResidentDetailsRules.Identity);
```

- Meaning: D16.2 (DESIGN:3910-3912); API:513 for the request, API:881 for the endpoint
  (Alis.Reactive/Validation/EndpointChecks.Net10.cs:61-68).
- Sends: as F1. Sees: with no bed and equal emails the save is sent; with emails that differ, "The email addresses
  must match." beside Confirm email and in the summary.
- Proof: `WhenAFlagsEnumIsCheckedInTheBrowser` (X16.22), App/Care/Record/CareRota.cshtml:21, :25: Save care, under the
  set Care, is saved though Rota's rule fails on its start; Save rota, under Rota, shows Rota's message and sends
  nothing. On the move-in, M9.X20 sends Verify identity under `MoveInRules.Identity` with no bed chosen
  (App/Care/MoveIn/ResidentDesk.cshtml:32), and M9.X21 refuses it for emails that differ.

### F3. Save with no browser check; the endpoint's refusal shown: built, as two doors

```cshtml illustration
@(Html.NativeButton("save-unchecked", "Save").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Details, request => request.Form(resident.Form))
    .NoValidation()
    .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))))
```

- Meaning: D16.2, "Naming a rule set, or no check (`NoValidation()`), overrides it"; D16.13, DESIGN:4079-4080, places
  a `ValidationProblem` as a check's failures. Under a host whose request defaults name `ShowValidationErrors()`
  (API:580) the `OnError` line is the default's and is not written (D10.17, DESIGN:2381-2383).
- Sends: as F1, whatever the inputs hold. Sees: the endpoint's messages, each beside the input the request sent under
  its key and all in the form's summary.
- Proof, door by door. `NoValidation()`: `WhenACoordinatorDiscardsTheDraft` (M9.X225) sends its DELETE of a
  `MoveInPage` from the page as it opens (App/Care/MoveIn/DraftActions.cshtml:15-20); that the default set would have
  refused it there is move-in.md:1440's statement, and no run with the door taken out was made. The 400 shown:
  `WhenARequestSendsAnInputOutsideEveryContainer` (X16.16, beside the input and in the notice),
  `WhenTheServersConditionHoldsARule` (X8.3) and `WhenACondensedRequestIsRefusedWithErrors` (X10.12, the default
  route). The two doors on one request: unproved. The search: the parse of 2.6 printed 16 requests that name no
  check, none with `ShowValidationErrors` in its statement.

### F4. Check without saving, under the default rule set: built

```cshtml illustration
@(Html.NativeButton("review", "Review details").Reactive(plan, button => button.Click, (clicked, pipeline) =>
{
    pipeline.Validate(resident.Form);
    pipeline.Element("saved").Text.Set("Details ready");
}))
```

- Meaning: N16.3, DESIGN:4200-4202. Sends nothing. Sees: each failure beside its input and in the form's summary, and
  "Details ready" only when none fails.
- Proof: `WhenACoordinatorChecksAResidentsDetails.an_email_confirmation_that_differs_is_refused_in_the_browser_and_no_review_leaves`
  (M9.X21): Review resident details shows "The email addresses must match." beside Email confirmation and in
  `#move-in_summary`, and resident-status stays empty; with equal emails it reads "Resident details ready". Both sides
  choose a bed first, for 2.2's reason.

### F5. Check without saving, under a named rule set: new door

```cshtml illustration
@(Html.NativeButton("next", "Next").Reactive(plan, button => button.Click, (clicked, pipeline) =>
{
    pipeline.Validate(identity.Container, ResidentDetailsRules.Identity);
    pipeline.Element("step").Text.Set("Step 2: the bed");
}))
```

- Decided: use-cases.md's candidate, weighed against the two forms the base already gives.
  1. F4's door over the fieldset. Refused by 2.2: "Choose a bed." fails in the fieldset's summary, beside no input.
  2. The step as its own model, an `Identity` member rendered by `Html.PartialFor` with its own validator and its own
     container, checked by F4's door. It is D16.2's own sentence and needs no door once B2's meaning stands. It does
     not serve a part whose inputs stay flat members of the page's model, the move-in's case, and it does not serve one
     model checked under two policies, B1s and B2s, which D16.2 gives to rule sets (X16.43's message, "put its
     alternate checks in rule sets").
  3. The rule set named on the check. It is the form each other check already has: the validator's `RuleSet` (API:647),
     a request (API:513) and the endpoint (API:881) name one, and this check alone cannot. The owner's rule for a new
     form (spec/CLAUDE.md:17, "a concrete case and a shown difference in meaning") is met twice: the move-in's
     ResidentDesk.cshtml:13 against its own request at :32, and the brief's row and roster checked under Approval
     without saving.
- Why the validator is not named: a model has one registered validator (D16.2, X16.43), so the container's model
  names it, as F4's door already reads it (FormChecks.cs:21). A third type argument would make the developer write all
  three.
- Sends nothing. Sees: with equal emails and no bed, "Step 2: the bed"; with emails that differ, "The email addresses
  must match." beside Confirm email and in identity's summary, and the step does not change.
- A set under which the validator runs no rule is refused where the view renders (4.3).
- Its door, lines and promise: 4.1, X16.48.

### F6. Live check on typing, blur and commit: built

Nothing is written: the container's block carries it (D16.15, DESIGN:4088-4095; Alis.Reactive/Validation/LiveChecks.cs:17-32,
written by the block at Alis.Reactive/InputsAndIds/RequestBlock.cs:23).
- Proof: `WhenTwoLiveContainersHoldOneInput` (X16.20): leaving the empty Name that a form and a fieldset both hold shows
  "Name is required." once; with Ann typed it stays hidden. `WhenALiveContainerHoldsTwoMembers` (X16.41): leaving one
  input shows its own member's message and not the other's.
- Not proven by a test I found: "Typing clears an input's message", and that a live check "changes no summary and no
  notice". The search: `git grep -n -E "typing|Typing" 830817f4b -- spec/Validation/features.json` printed 4 lines, each
  the opposite of another promise, and `git grep -l -E "typing_|_typing|Typing"` over Tests/Validation printed no file. A
  search by the word "clears" was not made.

### F7. Live check under a named rule set, or none: designed, and not built

D16.15 (DESIGN:4089-4091): "a bare block carries the first, and the door that names a rule set or none is declared
with the first view that names one." use-cases.md offers F5's wizard as that view. It is not: every rule over an
input of the identity fieldset is declared in both "default" and "Identity", so a blur under either set shows the
same message, and the enclosing form's own default live check still runs for each of those inputs (model.ts:134-143
runs the live check of every container that holds the input). No page of this document, and none of the app, shows a
blur whose message differs by rule set, so the owner's rule (spec/CLAUDE.md:17) is not met and nothing is built. The
form it takes when a view does show it is 4.5, so the choice is made once.

### F8. A rule only the server decides: built

`Must`, `MustAsync`, `Custom`, a rule under `WhenOnServer`, `.ServerOnly()` (D16.9, DESIGN:4001-4006). The browser
sends, the endpoint refuses, and the 400 is placed as D16.13 places it.
- Proof: `WhenTheServersConditionHoldsARule.the_request_is_sent_and_the_endpoints_message_shows_beside_the_input` (X8.3,
  App/Care/Admission/VisitorSignIn.cshtml:24-25): the request leaves and the endpoint's message shows beside Visitor.
  `WhenAContactsRoleNeedsItsAuthority.a_revoked_authority_reference_is_refused_by_the_server_with_its_reason` (M9.X47):
  one request reaches the contacts' address and the notice reads "This authority reference was revoked.", since that
  request sends no container.

### P1. An attached partial checked with its parent: built

```cshtml
@await Html.PartialFor(m => m.Rows[at], "/Care/MoveIn/Contact.cshtml")
```

```csharp
partial void TheContacts() => RuleForEach(moveIn => moveIn.Contacts.Rows).SetValidator(new ContactRules(reference => false));
```

(App/Care/MoveIn/Contacts.cshtml:12; App/Care/MoveIn/MoveInRules.Contacts.cs:7.) D16.3, DESIGN:3915-3920.
- Proof: M9.X21, whose page renders the resident's partials "with Html.PartialFor at Resident, Resident.Identity and
  Resident.Address, each with its own plan" (spec/PartialsAndSlots/features.json:1580): the page's check shows the
  two-member rule's message beside a partial's input and in the page container's summary. For `EditorFor`,
  `WhenAnEditorTemplateRendersAnInputThePagesRuleNames` (X14.8), named and not opened.
- For a row partial of a list, the message beside Alice Ward's input in M9.X41
  (Tests/PartialsAndSlots/MoveIn/WhenACoordinatorAddsAContact.cs:71) is the page container's live check, by
  move-in.md:1458 ("beside an input only by the main container's live check"), which runs the rule `TheContacts` declares over
  the row partial's input. A request's check that shows a row partial's failure beside its input was not found among the
  tests opened; M9.X41's request sends no container.

### P2. An independent partial in a slot, checked alone: built

Its own model, plan, validator, container and request (D16.3, DESIGN:3921-3923; D12.4).
- View: App/Care/MoveIn/WaitlistEntry.cshtml:9-36, a `WaitlistPage` loaded into a slot, whose request sends its own
  container under the condensed default. Proof: M9.X66 (F1).
- `WhenEachStatesPartialHasItsValidator` (X16.5): with the Texas partial loaded Save shows "Give the Texas license.";
  after Massachusetts loads, Save shows "Give the Massachusetts permit." alone, so the first registration left with
  its slot. `WhenAPartialHoldsItsOwnValidation` (M9.X9): the slot's save is checked by its own rules and none of the
  page's.

### B1. An action on all rows, "Save all": every row is checked: built

The roster's page, for every bulk case. Its rows are rendered as the move-in renders its contacts
(App/Care/MoveIn/Contacts.cshtml:7-14), a partial a row:

```cshtml illustration
@model Roster
@{
    var plan = Html.ReactivePlan<Roster>();
}
@using (var roster = Html.BeginRequestForm(plan, "roster"))
{
    @for (var position = 0; position < Model.Rows.Count; position++)
    {
        var at = position;
        <fieldset>
            <legend>Shift @Model.Rows[at].Id</legend>
            @await Html.PartialFor(m => m.Rows[at], "/Roster/Shift.cshtml")
        </fieldset>
    }
    @(Html.NativeButton("save-all", "Save all").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
        .Put<Roster>(RosterEndpoints.Shifts, request => request.Form(roster.Form))
        .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("roster-status").Text.Set(saved.Body))))
}
<p id="roster-status"></p>
@Html.RenderPlan(plan)
```

- Meaning: D13.5, DESIGN:3488-3490, "A check runs it on every item row its body holds, the rows inside the container
  it sends, whoever rendered the row".
- Sends: every row, "Rows[0].Id", "Rows[0].Carer", "Rows[0].Hours" and on, as a `Roster`; an outer container sends an
  inner one's inputs too (X9.16), under their full names, which B2 leaves as they are.
- Binds: net10, facts:103 (`core-form-binds-indexed-name`) and facts:769 (`core-form-nested-start`); MVC 5, facts:4
  (`mvc5-sub-index-name`), facts:823 (`mvc5-list-item-new`) and facts:832 (`mvc5-list-replaced`).
- Sees: each failure beside its own row's input and in the form's summary; nothing is sent while a row fails.
- Proof: `WhenEachEntryOfAListHasItsRule` (Cells.validation_listItem, App/Bench/Validation/EntriesForm.cshtml:26-27,
  F1e's door): "Pick a code." beside the second entry alone, nothing shown as saved; with both picked it saves.
  `WhenARowLoadsIntoTheRoundsContainer` (X13.3): the rule for each item checks a row a slot loaded. A list saved under
  the condensed default with no validator named has no test among those opened.
- All rows checked without saving is F4's door over the form, `pipeline.Validate(roster.Form)`.

### B1s. An action on all rows under a rule set, "Approve all": the validator's line, and a refusal

```csharp illustration
public RosterRules() => RuleSet("default, " + ShiftRules.Approval, () => RuleForEach(roster => roster.Rows).SetValidator(new ShiftRules()));
```

```cshtml illustration
@(Html.NativeButton("approve-all", "Approve all").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Put<Roster>(RosterEndpoints.Approvals, request => request.Form(roster.Form))
    .Validate<RosterRules>(ShiftRules.Approval)
    .OnSuccess(Content.Text, (approved, pipeline) => pipeline.Element("roster-status").Text.Set(approved.Body))))
```

- Meaning: 2.1. The rule that holds the row's validator is declared in each set it must run under; the set's one
  name then selects the row's own rules, on both sides. The endpoint names the same set,
  `this.Validate<Roster, RosterRules>(roster, ShiftRules.Approval)`.
- What runs under Approval is `ShiftRules`' Approval rule alone: a shift of 0 hours passes Approve all, since
  "A shift is 1 to 12 hours." is declared in no set. A developer who wants both declares that rule in
  "default, Approval", as `ResidentDetailsRules` declares its identity rules.
- With use-cases.md's `RosterRules` unchanged the page is refused where it renders (4.3), where today it renders and
  approves every roster.
- Sends: as B1. Sees: a 10-hour shift shows "An approved shift is 8 hours at most." beside its row's Hours and in the
  form's summary, and nothing is sent.
- All rows checked under the set without saving: `pipeline.Validate(roster.Form, ShiftRules.Approval)`, F5's door.
- Promises: X16.50, X16.49.

### B2. An action in a row, its "Save": that row alone is checked: a new meaning, no new door

The row's view, the partial B1 renders:

```cshtml illustration
@* /Roster/Shift.cshtml *@
@model ShiftRow
@{
    var plan = Html.ReactivePlan<ShiftRow>();
}
@using (var shift = Html.BeginRequestContainer(plan, "shift-" + Model.Id))
{
    @(Html.NativeHiddenField(plan, m => m.Id))
    @(Html.InputField(plan, m => m.Carer, options => options.Label("Carer")).NativeTextBox())
    @(Html.InputField(plan, m => m.Hours, options => options.Label("Hours")).NativeTextBox())
    @(Html.NativeButton("save-shift-" + Model.Id, "Save").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
        .Put<ShiftRow>(RosterEndpoints.Shift, request => request.Inputs(shift.Container).Set(sent => sent.Id, Model.Id))
        .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("roster-status").Text.Set(saved.Body))))
}
@Html.RenderPlan(plan)
```

```csharp illustration
public const string Shift = "/roster/shifts/{Id}";

[HttpPut(RosterEndpoints.Shift)]
public IActionResult SaveShift([FromForm] ShiftRow shift)
{
    var validation = this.Validate<ShiftRow, ShiftRules>(shift);
    if (!validation.IsValid) return validation.ToErrorResponse();
    return Content($"Shift {shift.Id} saved", "text/plain");
}
```

- Every door is landed: `Html.PartialFor` (API:810), `Html.BeginRequestContainer` (API:817), `Inputs` (API:846), the
  condensed `Set` after it (Alis.Reactive/TriggersAndPipelines/CondensedRequests.cs:211, as
  App/Care/MoveIn/DraftCommands.cshtml:212-213 writes the pair), and the condensed default, since `ShiftRules` is
  registered for `ShiftRow`.
- The meaning that is new (4.2): the container, rendered by a view under the prefix "Rows[2]", is its view's model. It
  sends, reads and checks the inputs inside it under the names past that prefix.
- Sends: `PUT /roster/shifts/7` with the multipart fields Id, Carer and Hours (`Inputs` on a body is multipart,
  Alis.Reactive/TriggersAndPipelines/RequestParts.cs:212). `Id` fills the placeholder from the `Set` and is in the body
  from the row's hidden input, which B1's body needs; where an input and a set member send one name the input's value
  is sent (D9.15, DESIGN:1931-1932; request.ts:188).
- Binds: a `[FromForm] ShiftRow` from its own three keys on both hosts, an empty Carer as null (facts:877,
  `core-form-implicit-required`; facts:940, `mvc5-empty-text-null`), the path's Id beside the body's (facts:1912,
  facts:1948; D9.9). It rests on no list index (2.5) and on none of the seven unrun probes of
  form-data-boundary-design.md: its one member outside the body is also in it.
- Sees: with row 2's Carer empty, "Choose a carer." beside row 2's Carer and in row 2's own summary, the one its
  container's block renders (RequestBlock.cs:26), and nothing is sent. Row 3's empty carer is not read: it is outside
  the element. The roster's form and its summary do not change.
- Weighed, the landed-door form: one `Input` a member, in the row's partial,
  `request.Input(sent => sent.Carer, pipeline.Input(m => m.Carer)).Input(sent => sent.Hours, pipeline.Input(m => m.Hours))`.
  It needs no new meaning and is D9.15's own door. It loses on three counts: its failures' summary place is the
  page's notice, since it sends no container (D16.13; D13.6); it has no form for B2c, since a check with no request
  takes a container; and a member the developer forgets is checked at its start. At the base it also meets 2.4's
  defect, since each input's own path, under the prefix, differs from the member's. It stays valid for a row whose
  inputs share no element, and 4.2's field makes it work as D9.15 states.
- Weighed, candidate A, `activated.Inputs` on a row action declared once: it needs a container named by a value read
  when the step runs, which the owner's table of spec/NOTES.md places after 1.0 ("An element or a slot named by a value
  read when the step runs ... a row's control reached by a handler another event started"). B2's meaning is what that
  door would resolve to, so nothing here blocks it.
- Weighed, a container of `Roster` around one row: refused by 2.5.
- Promise: X16.51.

### B2s. An action in a row under a rule set, its "Approve"

```cshtml illustration
@(Html.NativeButton("approve-shift-" + Model.Id, "Approve").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Put<ShiftRow>(RosterEndpoints.ShiftApproval, request => request.Inputs(shift.Container).Set(sent => sent.Id, Model.Id))
    .Validate<ShiftRules>(ShiftRules.Approval)
    .OnSuccess(Content.Text, (approved, pipeline) => pipeline.Element("roster-status").Text.Set(approved.Body))))
```

B2 with F2's door; the endpoint checks `this.Validate<ShiftRow, ShiftRules>(shift, ShiftRules.Approval)`. Sees: a 10-hour
row 2 shows "An approved shift is 8 hours at most." beside row 2's Hours and in row 2's summary; an empty carer does
not block it, since that rule is declared in no set. Promise: X16.52.

### B2c. An action in a row that checks without saving, its "Check"

```cshtml illustration
@(Html.NativeButton("check-shift-" + Model.Id, "Check").Reactive(plan, button => button.Click, (clicked, pipeline) =>
{
    pipeline.Validate(shift.Container);
    pipeline.Element("roster-status").Text.Set("Shift " + Model.Id + " is ready");
}))
```

F4's door over the row's own container; under a set, `pipeline.Validate(shift.Container, ShiftRules.Approval)`. Sends
nothing. Sees: row 2's failures beside row 2's inputs and in row 2's summary, and the status only when none fails.
Unproven, and named: move-in.md:1428 records "with Review resident details in the attached partial's own plan, one
run showed the failure beside its input and stopped, with the container's summary left empty ... I did not isolate
that." That check named the page's container from an attached partial's plan; this one names the partial's own. The
code path was read (FormChecks.cs:16-42; forms.ts:97-112) and shows no cause; none was isolated. X16.53 is the probe,
written first: it reads the row's summary.

### B3. A live check in a row

Built for the form: a row's input sits inside the roster's form, whose block carries `RosterRules`' default live
check, and a blur shows the member's first failure beside it (D16.15). With B2 the row's own container is a second
live container, under `ShiftRules`, and "a message two of them give shows once" (X16.20). One line of the browser's
code changes with B2: a live check names its failures as its own container sends them, "Carer", where the code
matches a failure to the input by the input's full name (forms.ts:82), so the row container's own failure is matched
by the name that check sent the input under (4.2). On this page the form's check gives the same message either way;
a row in no checked form is the page that shows the difference, and X16.54 is written on it.

### B4. An action that adds a row: built

```cshtml
@using (var fresh = Html.BeginRequestContainer(plan, "new-contact"))
```

```csharp
RuleFor(edit => edit.NewContact!).SetValidator(aContact);
```

(App/Care/MoveIn/NewContact.cshtml:11, a view of `ContactEdit`; App/Care/MoveIn/MoveInRules.Contacts.cs:39; the request
at App/Care/MoveIn/DraftCommands.cshtml:148-156, under the condensed default.) The new row is a container of the
edit's model, read whole into the command (NewContact.cshtml:23), and the item's validator is held on the new
member.
- Proof: `WhenACoordinatorAddsAContact`: M9.X40, a contact added joins the rows; M9.X41, a malformed email in a row
  already there refuses the add before it is sent, in the notice. The file's five cases (M9.X40 to M9.X44) hold none
  in which the new contact's own rule fails, so that side is unproved by them. The message shows in the notice, since
  the request sends no container (move-in.md:1458).

### B5. An action in a row that removes it: built

The row's control dispatches its id (App/Care/MoveIn/ContactRows.cshtml:12-19), and the removal is checked under its
own set, `.Validate<ContactEditRules>(ContactEditRules.Removal)` (DraftCommands.cshtml:113-122), which holds one rule,
"Choose a contact." (MoveInRules.Contacts.cs:40).
- Proof: `WhenACoordinatorRemovesAContact`: M9.X50, the contact leaves and the other rows keep what was typed; M9.X51,
  a row with no id shows "Choose a contact." in the notice and sends nothing, and with its id the removal is sent
  once.

### G1. A cell edited in a grid row, saved with its row. G2. A row edited in a grid's edit dialog

Verdict: no core door. Both are module 21's, FusionComponents, whose spec already holds their form, and their
validation is not built at the base.
- Built today, with no validator anywhere on them: a cell editor, App/Care/BedBoard/StatusCorrections.cshtml:14-19
  (`FusionGridEditTemplates.Select`); the dialog, StatusCorrectionDialog.cshtml:11-16 (`DialogForm<TRow>`); a batch
  edit saved by a request, ChargeCorrections.cshtml:16, :28,
  `request.Json(body => body.Set(r => r.Changed, pipeline.Component<FusionGrid>("charge-corrections").GetBatchChanges<CorrectedCharge>().ChangedRecords))`.
  `git grep -n -E "ReactiveValidator<(SavedCharges|CorrectedCharge|CorrectedStatus|StatusUnderCorrection|SavedBatch)>"`
  over tests printed no line.
- The check before the endpoint keeps a row, by landed doors: that request's model gets a validator,
  `RuleForEach(saved => saved.Changed).SetValidator(new ShiftRules())`, and the condensed default checks each changed
  row before it is sent and at the endpoint (D16.5's decision C, DESIGN:3950-3951, "A request that sends no container
  is checked on its `Set` members"). Its failures show in the notice: a `Set` member has no message element
  (D16.13, DESIGN:4073-4075). What the grid hands is pinned: facts:2808 (`ej2-grid-get-batch-changes`) and facts:4311
  to 4341 (spec/probes/ej2-grid-batch-rows.mjs: a changed row is a plain object of the row's own members).
- "Before the grid keeps it, its messages beside the cell or the dialog's field" is the grid's own editor, fed the
  validator's rules: spec/FusionComponents/module.md:311-312, "`FusionGridValidation.From<TRow>(rules).Field(..)`, a
  column's rules from the validator (N16.2)", and spec/FusionComponents/features.json:9149 with its refusal M21.X203.
  It is not built: `git grep -l FusionGridValidation 830817f4b` printed 8 files, each under spec/.
- Why no core door now. A grid's editor is the vendor's input, created when a row is edited: the page's input table
  holds no entry for it, so no container sends it and D16.13 has no message element for it. The two things a core
  door could add, a check over a value a source hands (the event's row) and a write of the event's cancel, have no
  page and no probe; root CLAUDE.md's rule is that a gap is raised with its use case and no member is invented. They
  are named for the grid's unit.
- The owner's words of 2026-11-20 (HANDOFF.md, the notes block): "if consumer need something specific, we should
  design it like row level action which has same framework permitives" and "adapter is not default choice".
  `FusionGridValidation` is an adapter from the validator's rules to EJ2's column rules, so M21.F23's form and those
  words meet when the grid's unit is briefed: line 5 of section 10.

## 4. The new pieces

### 4.1 F5's door: `pipeline.Validate(container, ruleSet)`

- Door, one line in each target's PublicAPI.Unshipped.txt, beside API:879:
  `static Alis.Reactive.TriggersAndPipelines.RequestSteps.Validate<TModel, TContainer>(this Alis.Reactive.TriggersAndPipelines.Pipeline<TModel>! pipeline, Alis.Reactive.RequestsAndResponses.RequestContainer<TContainer>! container, string! ruleSet) -> void`
- Typed: `ruleSet` is the parameter API:513 and API:881 already hold, "a text as the validator's own `RuleSet` takes it"
  (spec/CLAUDE.md:1449). It names no member, and a developer passes the validator's own constant.
- Nodes, none new: the `let` of the container's capture, the `let` of a `call` of `validate` on the plugin object of
  `alis.form`, the `if` and the `stop`, and the plugin data `form` (schema:146), as
  Alis.Reactive/Validation/FormChecks.cs:34-41 writes them today. The registration's key is the validator's key and
  the rule set's name (FormChecks.cs:31), which a request's named check already writes
  (Alis.Reactive/Validation/RequestChecks.cs:62).
- C#: `FormChecks.WithNoRequest` takes the rule set where FormChecks.cs:30 fixes it;
  Alis.Reactive/TriggersAndPipelines/RequestSteps.cs gains the overload with its XML doc, and the doc of the first
  door (RequestSteps.cs:29) keeps "the validator's default rule set".
- Browser: nothing changes. forms.ts:65-71 runs the registration the check names.
- N16.3, DESIGN:4200-4203, becomes: "`N16.3` new: `pipeline.Validate(container)`, the check with no request: the
  container model's registered validator under the default rule set, over the container's inputs as it sends them; a
  failure shows as D16.13 places it and stops its pipeline. `pipeline.Validate(container, ruleSet)` runs that
  validator under the rule set it names, a text as the validator's own `RuleSet` takes it, as a request's
  `Validate<TValidator>(ruleSet)` does (D16.2). It carries `Validate<TValidator>(formId)` of both lines."
- D16.5's bullet, DESIGN:3953-3954, gains the second door's name: "The check with no request, `Validate(container)` and
  `Validate(container, ruleSet)` (N16.3), and live validation read their container's inputs, as the container sends
  them."

### 4.2 B2's meaning: a container under a field prefix is its view's model

No door. Two fields join two records of the plan's contract, and the browser reads them in three files.

The contract (schema:161-165, :497-499; BRIEF.md's principle 6 makes this the owner's: line 1 of section 10):
- `Container` gains `prefix`: the field prefix its view was rendered under, its `name` ("Rows[2]") and its `path`, the
  steps that name writes, as an input's binding writes them (Alis.Reactive/InputsAndIds/BoundInput.cs:151-157). A view
  with no prefix writes an empty text and an empty list, so the field is on every container and is never absent.
- `SentInput` gains `path`: the path the input is sent under. For a container's input it is the binding's path
  past the container's prefix; for an `Input` it is the request member's path, which C# hands beside the name
  (RequestPartsWriter.cs:103, :307-311).
- `InputBinding` does not change.

C#: the block writes the prefix from the plan's start prefix
(Alis.Reactive/RenderingAndDelivery/ReactivePlan.cs:32, which RequestBlocks.Net10.cs:26-28 already reads for R14.2;
Alis.Reactive/InputsAndIds/ModelPlugin.cs:19-23), and `ModelPlugin.Input` (ModelPlugin.cs:33) hands the member's
path.

The browser, by file:
- model.ts, `captured` (:105-132): an input inside the element whose path the container's prefix does not open stops
  the capture; each other input is written at its path past the prefix and named by its name past it;
  `sentInput` (:233-235) carries the path; `input` (:161-165) takes the path C# hands; two renderings of one id
  under two prefixes stop, as two models stop (:110-111); `liveChecksOf` (:134-143), which lists a container's inputs
  on its own today (:139), takes them from the same capture, so a live check names them as its container sends them.
- forms.ts: `#typed` (:151-159) and `#nameOf` (:186-188) read the sent path, which fixes 2.4; `checkLive` (:78-85)
  matches a failure by the name its own check sent the entry under.
- request.ts: `setInput` (:187) and `withContainer` (:225) read the captured body at the sent path; in a native form's
  submission set `setInput` deletes the entries under the input's own binding name, the name the page's control holds,
  where it deletes by the sent name today (:188), and appends under the sent name.

The one text rule the browser runs, stated by C#: an input's name past the prefix is its binding name after the
prefix's name, less the dot that joins them. MVC joins a prefix and a member with a dot and an index with none
(facts:391, `core-template-prefix-name`; facts:4, `mvc5-sub-index-name`), and the paths agree step by step before
the name is cut, so "Row" never opens "Rows[2]". request.ts:200 reads a name as dotted text today. The alternative
that carries it, each binding holding where each step starts in its name, adds a field to `InputBinding` for every
input of every plan; the owner declined a change of `InputBinding` on its own (spec/NOTES.md, 2026-11-23), so it is
named in line 1 and not recommended.

Each state, with its verdict in C#'s words:

| The page | What the container does |
|---|---|
| A container under no prefix: each of the app's 147 | as today: an empty prefix cuts nothing |
| An input of the view itself, "Rows[2].Carer" | sent, read and checked as "Carer" |
| An input of a partial deeper under the prefix, "Rows[2].Authority.Reference" | sent as "Authority.Reference" |
| An input inside the element that the prefix does not open: a slot's content or a partial rendered with no prefix (DESIGN:3765-3768) | the send, the read and the check stop, with one error naming the input, the container and the prefix |
| The same inputs inside an outer container under no prefix, the roster's form | sent by it under their full names (X9.16), unchanged |
| `shift.Container.Read(m => m.Carer)` | reads the input, since the lambda's steps meet the body past the prefix (ContainerReads.cs:18-21) |
| `pipeline.Container<ShiftRow>("shift-7")` from another view (D14.8) | the same: the prefix is the container's entry's, whoever asks |
| Blur in an input that the row's container and the form both hold | each live check reads and names by its own container; the message shows once (D16.15) |
| The endpoint's 400 | matched by the names the request sent (forms.ts:93-105): a `ShiftRow` endpoint's "Carer" lands beside row 2's Carer |
| A native form's block under a prefix, sent by `Form(form)` | each framework input the form owns replaces its entries, D9.15's own rule (DESIGN:1916-1918): the entries under the name the control holds leave and its wire form goes under the name past the prefix, so one name is sent; a hand-written control is sent under the name the developer wrote, as today |
| Blur in a container that holds an input its prefix does not open | that blur's live checks stop with the same error and show no message, as a blur stops today in a container that holds two inputs of one name (model.ts:116, read by :141) |
| N rows | N plans, each carrying the registration "ShiftRules:default"; the browser takes the first under a key (forms.ts:66), and the copies are one validator, rule set, culture and JSON options (Alis.Reactive/Validation/BrowserRegistrations.cs:20-33). Their weight is objection 2 |
| A row that is a table row | not expressible: the block writes a `form` or a `fieldset` (RequestBlock.cs:20), where D9.15 names "a table row" among a container's elements (DESIGN:1919). A gap, named in section 8 |

The lines, each with its full text:
- D14.8, DESIGN:3683-3689, gains: "A container's block in a view rendered under a field prefix, a partial for a
  member among them (D14.4), is its view's model: it sends, reads and checks the inputs inside its element under the
  names past that prefix, each input's path the steps after the prefix's and its name the text after the prefix's,
  less the dot that joins them (`probe:core-template-prefix-name`, `probe:mvc5-sub-index-name`), so an endpoint that
  binds the container's model binds what it sends. An input inside it that the prefix does not open stops the
  container's send, read and check, with one error naming the input, the container and the prefix. C# writes the
  prefix into the container's entry, its name and its path, each empty for a view with none. A container under no
  prefix sends the same inputs under their full names (X9.16)."
- D9.15's bullet, DESIGN:1919-1920, becomes: "`Inputs(container)`: the framework inputs inside one element of any kind
  (a form, a fieldset, a table row, a drawer's body), rendered by any plan, nested elements included, each under its
  binding name past the prefix the container's view was rendered under (D14.8), in any body." Its last bullet,
  DESIGN:1934, becomes: "The run records each input it sent, under the binding name and the path it sent it by
  (D10.18)."
- D13.3, DESIGN:3468-3470, gains after "as the container sends it": "under a prefix, the member past it (D14.8)".
- D13.1's row, DESIGN:3460, "the inputs a request sent, by binding name, each as the inputs table held it" becomes
  "the inputs a request sent, each by the binding name and the path it sent it by, as the inputs table held it"; D10.18,
  DESIGN:2399-2400, "the run's record of each input it sent" gains "with the name and the path it sent it by".
- D16.15, DESIGN:4092, "each over its own container's inputs" becomes "each over its own container's inputs, under
  the names that container sends them by (D14.8)".
- schema:121-124, "under its `InputBinding.path`" becomes "under its `InputBinding.path` past the container's
  `prefix`"; schema:466-467, "each its binding name" becomes "each the binding name and the path it was sent under";
  schema:161-165 gains "`prefix`: the field prefix its view was rendered under, its `name` and its
  `path`, each empty for a view with none (D14.8)"; schema:497, "A `SentInput` is its binding `name`" becomes "A
  `SentInput` is the `name` and the `path` it is sent under".

What carries the two records, each a line a command printed: `SentInput` is named in 3 C# files of the core, 4 files
of the runtime and 2 of its test files, which hold 9 lines of a sent input's `entry:`, and in 1 file of
tests/Alis.Reactive.ModuleTests; the generated contract declares it at
Alis.Reactive.Assets/runtime/Schema/plan.ts:539 and `Container` at :317; one line of the core writes
`new Container(` (ModelPlugin.cs:21); 2 runtime test files hold 12 lines of a container's `summary:`, and 9 files
of the module tests name `containers`. Those are the files a builder reads again; none was opened here.

### 4.3 The refusal: a named rule set under which the validator runs no rule

One decision for the family 2.1 names: F2, F5, B1s, B2s and the endpoint's door. A check that names a rule set under
which its validator runs no rule passes on both sides and says nothing; spec/CLAUDE.md:1452-1453 holds its refusal
open.

- D16.2, DESIGN:3908-3913, gains: "A rule set a check names, by a request, by `Validate(container, ruleSet)` or at
  the endpoint's door, under which the validator runs no rule, a child validator's rules among them, is refused where
  the view renders and where the endpoint checks (X16.49). A rule that holds a child validator runs only under a set
  it is declared in, and hands the set's name to the child."
- X16.49, new: "`X16.49` refusal (D16.2): a check under a rule set its validator runs no rule under: "{Validator}
  runs no rule under the rule set '{ruleSet}': declare the rules it checks there inside RuleSet("{ruleSet}", ..), a
  rule that holds a child validator among them."" The default rule set, which a check takes when it names none, is
  not refused: D16.2 gives that check to every request of a registered model, whatever sets its validator declares.
- C#: a walk of the validator's own rules, each with its sets and its child validator, with no reflection: the data
  is the lists `ReactiveValidator<T>` already holds (Alis.Reactive/Validation/ReactiveValidator.cs:70-71;
  Rules.cs:103-106, :379-386). A child that is no `ReactiveValidator` counts as a rule, since its sets cannot be
  read. It runs where a named registration is built (RequestChecks.cs:48-55; FormChecks.cs) and in the endpoint's
  named doors (Alis.Reactive/Validation/EndpointChecks.Net10.cs:61-68, :85-92, and the Net48 file's).
- No node, no browser code, no door.
- Its rung is BRIEF.md's third, a refusal where the view renders. The rung above, an analyzer at build, was not
  designed: .claude/rules/analyzers.md is no file of this brief's Rules and was not read.

### 4.4 The defect of 2.4

It breaks D16.13 (DESIGN:4071-4073) and D16.7 (DESIGN:3988-3990); no page of the app reaches it; it is fixed by
`SentInput.path` (4.2) and held by X16.55. It lands with B2's unit or alone: its fix is the field and the two
reads of forms.ts.

### 4.5 F7's form, recorded and not proposed for building

Where a view first shows a blur whose message differs by rule set, the block names its live check by a closed choice,
as `HandlerPolicy.Replace` and `Content.Text` name theirs: `Html.BeginRequestContainer(plan, "approvals", LiveCheck.Under(ShiftRules.Approval))`
and `LiveCheck.None`, the same on `BeginRequestForm`. It writes `Container.live`, which already holds none or a
`Live` with its rule set (schema:165-169; LiveChecks.cs:24, :28). Two adjacent texts, an id and a rule set, are why
it is no bare string. It is a sketch: no PublicAPI line is proposed until its view exists.

## 5. The binding facts, and the probes the build runs first

| Fact | Its statement | The case that rests on it |
|---|---|---|
| facts:391 `core-template-prefix-name` (net10) | "Under a template's field prefix, MVC names a member by the prefix and the member." | B2: what the prefix's name opens |
| facts:4 `mvc5-sub-index-name` (net48) | "MVC 5 names a list item of a member by the member's name and the index in brackets, with no dot." | B2: the joining dot |
| facts:481 `mvc5-template-full-id` (net48) | "MVC 5 derives a template's element id from its full field name." | B2: one id a member (D14.3) |
| facts:103 `core-form-binds-indexed-name`; facts:769 `core-form-nested-start` (net10) | form keys bind a list in index order; each sent list item is new | B1, B1s |
| facts:823 `mvc5-list-item-new`; facts:832 `mvc5-list-replaced` (net48) | each sent item is a new object; a sent list's contents are replaced | B1, B1s |
| facts:877 `core-form-implicit-required`; facts:940 `mvc5-empty-text-null` | an empty text binds null on both hosts | B2: "Choose a carer." |
| facts:1912 `core-route-value-binds-member`; facts:1948 `mvc5-value-provider-order` | a value the body sends wins over the route's | B2: Id in the path and in the body (D9.9) |
| facts:877 `core-form-implicit-required` (net10), its second clause | "a text not sent keeps its start" | F5: Bed, absent from the fieldset's body, with D16.6's table, DESIGN:3965-3970 |
| facts:1237, facts:1597 | a list ends at the first missing index | none: 2.5's form is not written |
| facts:2808, facts:4311 to 4341 | what EJ2's grid hands as a changed row | G1's save |

Not looked up: the net48 fact for a text member that no key names (F5 on MVC 5). D16.6 states the meaning
(DESIGN:3967); the unit that builds X16.48 on the twin cites its fact or adds its probe.

Probes named: `core-form-list-of-objects-gap` (2.5), which no form of this design needs; and X16.53, the browser test
that reads the row's summary, written and seen failing before B2c's code (B2c).

None of the seven probes of form-data-boundary-design.md's section 7 carries a form here. Each new request sends a
body that binds members; its one member outside the body, B2's Id in the path, is in the body too, the case that
design's table reads alike under every row.

## 6. The orderings, their specs, and what TLC printed

| The ordering | Spec, action | Invariant | Mutant that must fail |
|---|---|---|---|
| A failed check sends nothing (F1, F2, B1, B1s, B2, B2s) | spec/RequestsAndResponses/Request.tla, `Check` (:178-183) | `FailedCheckSendsNothing` (:294) | "FailedCheckSends" |
| A container's read is what its send binds (F4, F5, B2, B2c, B3) | spec/InputsAndIds/ContainerRead.tla, `Read` | `ReadIsWhatTheEndpointBinds`, `ReadStopsWhenItsSendStops`; new: `ATypedTextReachesTheEndpoint`, `AnInputOutsideThePrefixStopsTheSend` | new: "PrefixKept", "OutsideThePrefixSent" |
| A member's input keeps its name under each door that gives it a prefix, and a page container's send binds it (P1, B1, B2) | spec/PartialsAndSlots/Partials.tla, `Render`, `LoadSlot` | `AttachedInputBindsItsMember`, `EverySentValueIsTheOneBound` | "NameWithoutPrefix" |
| The rule set a check names is the one both sides run (F2, F5, B1s, B2s) | spec/Validation/Validation.tla, which holds no rule set at the base | new: `TheNamedSetIsTheOneRun`, `ANamedSetSelectsARule`, beside `Agreement` | new: "CheckRunsTheDefaultSet", "ANamedSetNoRuleDeclaresRenders" |
| A failure reaches its summary, and shows beside the input the request sent (every case) | spec/OwnershipAndState/Ownership.tla, `ShowsFailures` | `FailureReachesSummary` (:432), `InlineOnlyAtTheSentInput` (:434) | "SummaryOnlyWithoutInline", "InlineByName" |
| A slot's rules leave with it (P2) | Ownership.tla | `RulesLeaveWithTheirOwner` (:415) | "ReleaseLeavesRules" |
| A second click on one row's Save while its first is in flight (B2) | spec/Execution/Lanes.tla, `FireSecond` | `SkippedTriggerStartsNothing` (:199) under skip; `ReplacedRunRunsNoRoute` (:195) under replace | "SkipStartsAnyway", "ReplaceKeepsOld" |
| Row 3 acting while row 2's request is in flight (B2) | Lanes.tla: no action joins them | none: a lane is one handler's (move-in.md:1394, citing runtime/Execution/start.ts:58 and lane.ts:16), and each row's partial writes a handler of its own | none |

The last row is a consequence the owner reads before a yes: with a row's action in its own partial, two rows save
side by side and Save all runs beside both, each a lane of its own, and the endpoint orders them, as the move-in's
endpoints do by a version header. One lane across every row is one handler declared once, candidate A's shape, which
waits on a container named when the step runs (B2).

Partials.tla is where B2's meaning meets a spec beside ContainerRead.tla, and it was not run. It models one send, the
page's own container under no prefix (`Sent`, `InputName`, `LandsIn`), which 4.2 leaves as it is: an empty prefix
cuts nothing. It holds no send of a partial's own container. Its change, the next step: a send of the kin's own
container whose inputs' names are cut at `KinPrefix`; an invariant that an input of the kin's view, and of an address
attached under it, lands in the kin's own member; a mutant that keeps the prefix. Two states of it are already named
there. net48's partial rendered with a model starts with no prefix (`AddressPrefix` is empty where `nested` is
"PartialWithModel" on net48), so inside the kin's container the kin's prefix does not open it: 4.2's stop. That is
the candidate "OwnPlanStops" (DESIGN:3783-3786, "That stop is the owner's to take") taken for a container under a
prefix alone; for the page's own container, whose empty prefix opens every name, that candidate stays the owner's,
untouched. net10's partial rendered with a model keeps the prefix around it
(`ReachInheritedPrefixMisnames`), so its input is cut to a name of the kin's model that is not its member's, the
misname the page's send already has there; this design does not change it and does not close it.

No claim is made that Request.tla, Lanes.tla or Ownership.tla stand unchanged: none was run, and this design adds no
action to any of them. Run.tla was not read; a check with no request and its stop are one step with no await
(FormChecks.cs:35-41).

Two specs were run, each on a copy under .landing/ValidationForms/scratch/, by the brief's command. They show the
case and its mutant behave; the spec's own change is the next step.

scratch/tlc-prefix/ContainerRead.tla, the base's spec with `prefixed` and `slotOutside`, 197 lines against 176 by `wc -l`:

| Config | Mutant | TLC printed |
|---|---|---|
| ContainerRead.cfg, the six invariants and the two new | none | "Model checking completed. No error has been found." "344736 states generated, 25056 distinct states found, 0 states left on queue." |
| ContainerRead.PrefixKept.sixOnly.cfg, the base's six alone | "PrefixKept", today's reading of 2.3 | "Model checking completed. No error has been found." "344736 states generated, 25056 distinct states found, 0 states left on queue." |
| ContainerRead.violates.ATypedTextReachesTheEndpoint.cfg | "PrefixKept" | "Error: Invariant ATypedTextReachesTheEndpoint is violated by the initial state:" |
| ContainerRead.violates.AnInputOutsideThePrefixStopsTheSend.cfg | "OutsideThePrefixSent" | "Error: Invariant AnInputOutsideThePrefixStopsTheSend is violated." |
| ContainerRead.violates.ReachATextBoundUnderAPrefix.cfg, a witness | none | "Error: Invariant ReachATextBoundUnderAPrefix is violated.": the state is reached |
| ContainerRead.violates.ReachStopOnAnInputOutsideThePrefix.cfg, a witness | none | "Error: Invariant ReachStopOnAnInputOutsideThePrefix is violated.": the state is reached |

The second row is the finding: the six invariants the base checks pass a container that keeps its prefix, since its
read and its endpoint agree on the start. The spec does not hold the case, and `ATypedTextReachesTheEndpoint` is the
line that does. The base's own run stands beside it (scratch/probe-tlc/tlc.log, the lead's): "114912 states
generated, 8352 distinct states found, 0 states left on queue."

scratch/tlc-ruleset/Validation.tla, the base's spec with `asked`, `inDefault` and `inNamed` on its check plane, 221 lines against 202:

| Config | Mutant | TLC printed |
|---|---|---|
| Validation.cfg, the base's twelve and the two new | none | "Model checking completed. No error has been found." "2655504 states generated, 1327752 distinct states found, 0 states left on queue." |
| Validation.violates.TheNamedSetIsTheOneRun.cfg | "CheckRunsTheDefaultSet", FormChecks.cs:30 kept | "Error: Invariant TheNamedSetIsTheOneRun is violated by the initial state:" |
| Validation.violates.Agreement.CheckRunsTheDefaultSet.cfg | the same | "Error: Invariant Agreement is violated by the initial state:" |
| Validation.violates.ANamedSetSelectsARule.cfg | "ANamedSetNoRuleDeclaresRenders", 4.3's refusal left out | "Error: Invariant ANamedSetSelectsARule is violated by the initial state:" |
| two witnesses, Validation.violates.Reach... | none | each "is violated by the initial state:": the state is reached |
| Validation.CheckRunsTheDefaultSet.baseOnly.cfg, the base's twelve less `ItemPeerAgrees` and `Agreement` | "CheckRunsTheDefaultSet" | "Model checking completed. No error has been found.": only the two invariants that read the check's verdict see the mutant |

What these copies do not model: a rule that holds a child validator and its own sets (the copy holds one rule, in or
out of each set), a nested prefix, and the name's cut. The base's 21 other ContainerRead configs and 16 other
Validation configs (22 and 17 by `git ls-tree`, each less its own) were not run on the copies.

## 7. The promises, in features.json's form

Each plant is unrun. The numbers follow X16.47, the last of Q16 (`git grep -c -E "X16\.(4[89]|[5-9][0-9])" 830817f4b -- spec | wc -l`
printed 0); the unit that lands each reads its number again. The roster page of each is B1's, its rows shifts 5, 6
and 7 at Rows[0] to Rows[2], shift 6 starting with Ann and 8 hours and shift 7 with no carer; its endpoints store
what they bind and the page reads the stored roster at its next open (App/CLAUDE.md, "none answers a constant").

- X16.48 (N16.3; F5).
  - page: A resident details page whose form resident holds a fieldset container identity with Name, Email and
    Confirm email, and Bed outside it; Next checks identity under the rule set Identity, then writes "Step 2: the
    bed" into step; Review checks identity under the default set, then writes "Details ready" into saved.
  - gesture: With Ada typed in Name and ada@example.org in both emails, Bed empty, click Next.
  - outcome: step reads "Step 2: the bed" and identity's summary shows no message.
  - opposite: With Confirm email changed to ada@example.com, "The email addresses must match." shows beside Confirm
    email and alone in identity's summary, and step stays empty.
  - defect: The check runs the default set, so "Choose a bed." fills identity's summary and Next stops; or it runs no
    rule, so emails that differ pass.
- X16.49 (D16.2; 4.3), a refusal.
  - page: The roster page whose Approve all names `.Validate<RosterRules>(ShiftRules.Approval)`, over a `RosterRules`
    that declares its rule for each row in no set.
  - gesture: Open the page.
  - outcome: The page is refused where the view renders with "RosterRules runs no rule under the rule set
    'Approval': declare the rules it checks there inside RuleSet("Approval", ..), a rule that holds a child validator
    among them."
  - opposite: With the rule declared in "default, Approval" the page renders, and X16.50 holds.
  - defect: The page renders and Approve all sends a 10-hour shift.
- X16.50 (D16.2; B1s).
  - page: The roster page, `RosterRules` declaring its rule for each row in "default, Approval".
  - gesture: Type 10 into shift 6's Hours and click Approve all.
  - outcome: "An approved shift is 8 hours at most." shows beside shift 6's Hours and in the roster's summary, and no
    request reaches the approvals address.
  - opposite: With 8 hours, and shift 7's carer still empty, the roster is approved and its next open reads it
    approved.
  - defect: The row's validator is not reached under the set, so 10 hours is approved; or the default set runs, so
    shift 7's empty carer refuses the approval.
- X16.51 (D14.8, D9.15; B2).
  - page: The roster page, each row a view of `ShiftRow` with its own container and a Save that PUTs the container's
    inputs to the shift's address.
  - gesture: In shift 6 type Ben into Carer and click its Save.
  - outcome: roster-status reads "Shift 6 saved"; the request's body holds the fields Id, Carer and Hours and no
    field named Rows; the roster's next open reads Ben for shift 6; shift 7 shows no message.
  - opposite: With shift 6's Carer cleared, "Choose a carer." shows beside shift 6's Carer and in shift 6's summary,
    no request reaches the shift's address, and shift 7 shows no message.
  - defect: The container sends "Rows[1].Carer", so the endpoint binds no member and Ben's save is refused with
    "Choose a carer."; or the check reads every input of the form, so shift 7's empty carer refuses shift 6's save.
- X16.52 (D16.2, D14.8; B2s).
  - page: The roster page, each row with an Approve that PUTs its container under `ShiftRules.Approval`.
  - gesture: Type 10 into shift 6's Hours and click its Approve.
  - outcome: "An approved shift is 8 hours at most." shows beside shift 6's Hours and in shift 6's summary, and no
    request reaches the approval's address.
  - opposite: With 8 hours and shift 6's Carer cleared, the approval is sent and stored.
  - defect: The row's request runs the default set, so the cleared carer refuses it and 10 hours passes.
- X16.53 (N16.3, D14.8; B2c).
  - page: The roster page, each row with a Check that checks its container, then writes "Shift 6 is ready" into
    roster-status.
  - gesture: Clear shift 6's Carer and click its Check.
  - outcome: "Choose a carer." shows beside shift 6's Carer and in shift 6's summary, roster-status stays empty, and
    no request leaves the page.
  - opposite: With Ann in shift 6's Carer, roster-status reads "Shift 6 is ready".
  - defect: The check stops with its summary empty (move-in.md:1428's observation); or it reads the container under
    its full names, so Ann's row fails with "Choose a carer.".
- X16.54 (D16.15, D14.8; B3).
  - page: A rota page whose model has no registered validator and whose rows are views of `ShiftRow`, each with its
    own container, in no form.
  - gesture: Click into shift 6's Carer, clear it and press Tab.
  - outcome: "Choose a carer." shows once beside shift 6's Carer, and shift 6's summary stays hidden.
  - opposite: With Ann typed, no message shows beside it.
  - defect: The live check names its failure "Carer" and the browser looks for "Rows[1].Carer", so nothing shows.
- X16.55 (D16.13, D16.7; 2.4).
  - page: A roster page whose shift 6 Save sends `Input(sent => sent.Hours, pipeline.Input(m => m.Rows[1].Hours))` and
    the carer the same way, as a `ShiftRow`.
  - gesture: Type 12a into shift 6's Hours and click its Save.
  - outcome: "The value '12a' is not valid for Hours." shows beside shift 6's Hours and in the notice, and no request
    reaches the shift's address.
  - opposite: With 20 typed, "A shift is 1 to 12 hours." shows beside shift 6's Hours and in the notice; with 8 the
    shift is saved.
  - defect: The check keys the input by its own path, so 12a is not refused and the start is sent, and the range's
    message shows in the notice alone.
- X16.56 (D14.8; 4.2).
  - page: The roster page, whose shift 6 holds a slot that loads a partial rendered with no prefix, with an input of
    its own.
  - gesture: Load the slot, then click shift 6's Save.
  - outcome: No request reaches the shift's address, and the console names the input, the container shift-6 and the
    prefix "Rows[1]".
  - opposite: With the slot not loaded the shift is saved.
  - defect: The input is sent under its bare name and the endpoint binds it as the shift's own member.

G1's check has no number here: it is a promise of module 21's, on App/Care/BedBoard/ChargeCorrections.cshtml with a
validator on `SavedCharges`, written by the grid's unit.

## 8. What this design names and leaves

- A row that is a table row: the block renders no `tr` (4.2). Its use case is a bulk edit laid out as a table.
- A row action declared once that sends its row's inputs, candidate A: after 1.0, with the container named when the
  step runs.
- F7, until a view shows its difference.
- A build-time analyzer for 4.3.
- The two things a grid's row edit could ask of the core (G1, G2).
- Tests the built cases lack, each found by a search that printed nothing: F3's two doors on one request; F6's
  "typing clears"; B4's new row failing its own rule; a list saved under the condensed default.

## 9. The two strongest objections

1. "B2 changes what `Inputs(container)` sends, D9.15's 'each under its binding name', and adds two fields to the plan's
   contract, for a page no app has written. The landed `Input` already saves one row: fix its defect and stop."
   The answer. The fields are not saved by stopping there: the defect's own fix is `SentInput.path` (2.4), so the
   landed-door form needs one of the two. What `Input` alone cannot give is the brief's matrix: a row checked
   without saving has no door, since N16.3 takes a container, and a row's failures have no summary of their own
   (D16.13). The owner's table of spec/NOTES.md already names the second as a missing door ("A request that sends
   values alone and still shows its failures in a container's summary"); B2 answers it with the container the row
   already is, and adds no door. The meaning it changes has no reader at the base: no page renders a container under
   a prefix (2.3), the state's behavior today is a typed handle that reads the start and a send no endpoint of its
   model binds, and TLC shows the base's spec passes that reading (section 6). D14.3 stands: an input keeps its one
   name and one id; only what a prefixed container's body is rooted at is stated.
2. "A partial a row writes a plan a row: a handler and a copy of the registration for each, the weight D17.15 was
   written against ('The handler is written once per action, where both lines write a whole plan per link',
   DESIGN:4762-4763). The owner asked for 'row level action' with the framework's own primitives." The answer. The
   weight is real and linear: a row's plan holds its inputs, which it holds today wherever a row is a partial
   (App/Care/MoveIn/Contact.cshtml:12-40 writes a handler a row), its container's entry, one registration and its
   actions. It was not measured. The door that removes it, a row action that reaches its row's container, needs an
   element named by a value read when the step runs, which the owner's table places after 1.0; what that door will
   send is this design's meaning, so the work is not redone. Until then the page that edits hundreds of rows saves
   them with Save all, one plan, and a row's own action is for the page that shows a screen of rows.

## 10. The lines for the owner

1. A container under a field prefix (B2, B2s, B2c, B3). What already holds: a container's block is typed by its
   view's model (D14.8), `Inputs` sends every input inside it under its full name (D9.15), and none of the app's
   33 views rendered under a prefix holds one (2.3), where its handle reads the start and its send binds nothing of
   its model. The one
   thing a yes changes: such a container sends, reads and checks its inputs under the names past its view's prefix,
   so a row's partial saves, approves and checks its own row with the doors F1 to F5 use. Its cost: `Container`
   gains `prefix` and `SentInput` gains `path` in schema.md and the generated contract (principle 6 of spec/BRIEF.md
   makes that yours), D14.8, D9.15, D13.3 and D16.15 gain the sentences of 4.2, ContainerRead.tla gains two
   variables, two invariants and two mutants, model.ts, forms.ts and request.ts change where 4.2 names, and six
   promises are written (X16.51 to X16.56); every plan's container entry changes by one field, and 4.2 counts the files that carry
   the two records (9 files of the module tests name `containers`; the runtime's tests hold 9 sent inputs). Its risk: the browser cuts a name by one stated rule, the prefix and its joining
   dot, where carrying the cut adds a field to every `InputBinding`; a row's plan a row is the page's weight; and
   B2c stands on a path one unisolated observation touches (move-in.md:1428). Recommendation: yes, with the name's
   cut as stated and `InputBinding` left alone.
2. F5's door, yours by G13 (spec/CLAUDE.md:89-91). What already holds: a request and the endpoint name a rule set, and
   the check with no request runs the default set alone (FormChecks.cs:30), so the move-in's review chooses a bed
   first. The one thing a yes changes: `pipeline.Validate(container, ruleSet)` stands beside
   `pipeline.Validate(container)`. Its cost: one PublicAPI line a target, one XML doc, N16.3's and D16.5's
   sentences, Validation.tla's rule set with two invariants and two mutants, and X16.48. Its risk: a second form of
   one check; the difference in meaning is shown on ResidentDesk.cshtml:13 against :32 and by the row checked under
   Approval. Recommendation: yes.
3. The refusal of 4.3, held open at spec/CLAUDE.md:1452-1453. What already holds: a check under a rule set its
   validator runs no rule under passes on both sides, and a rule that holds a child validator must itself be declared
   in the set (Rules.cs:22-27, :111). The one thing a yes changes: such a check is refused where the view renders and
   where the endpoint checks, with X16.49's sentence. Its cost: one walk of a validator's rules in the core, D16.2's
   sentence, X16.49 and its test. Its risk: a page that names a set no rule is in on purpose stops rendering. None of
   the app's does, by reading: each of its 9 named checks in a view and its 5 at an endpoint names a set a `RuleSet(`
   of its validator declares (Draft, Submit and Pharmacy at App/Care/MoveIn/MoveInRules.Medications.cs:53-55,
   MoveInRules.Payer.cs:9-11 and MoveInRules.cs:21; Identity at MoveInRules.Resident.cs:9-11; Removal at
   MoveInRules.Contacts.cs:40; Care and Rota at App/Care/Record/CareRotaController.cs:76-77, :89-90). The walk itself
   was not run. Recommendation: yes, with F5's unit, since B1s is silent
   without it.
4. F7. What already holds: D16.15 declares the door "with the first view that names one". The one thing a yes to this
   line changes: nothing is built for F7 now, and use-cases.md's reading of F5's wizard as that view is set aside,
   since its blur shows one message under either set. Its cost: none. Its risk: a view that needs it waits a unit.
   Recommendation: yes, leave it, with 4.5's form recorded.
5. G1 and G2. What already holds: the grid's cell editor, dialog and batch save are built with no validator on any
   (App/Care/BedBoard), the save's check is writable today with landed doors, its failures in the notice, and
   module 21 specifies `FusionGridValidation` for the messages beside a cell, not built. The one thing a yes changes:
   this design adds no core door for a grid, and the grid's own unit decides between M21.F23's adapter and your words
   of 2026-11-20 ("adapter is not default choice"). Its cost: none now. Its risk: that unit lands after the core's
   freeze and asks the core for a check over a value a source hands; the need is named in section 8.
   Recommendation: yes.

## 11. What was read, and what did not run

- Read whole at the base: .claude/agents/designer.md; CLAUDE.md; .claude/rules/docs.md, process.md, views.md,
  csharp.md and tests.md; Alis.Reactive/CLAUDE.md; Alis.Reactive.Assets/CLAUDE.md; App/CLAUDE.md; spec/MISTAKES.md;
  spec/Validation/module.md; spec/Validation/Validation.tla; spec/InputsAndIds/ContainerRead.tla;
  spec/PartialsAndSlots/Partials.tla; HANDOFF.md; spec/Validation/browser-validation-proposal.md; the three files of
  this folder; and the source files section 2 cites, each whole but RequestPartsWriter.cs, RequestParts.cs,
  CondensedRequests.cs and request.ts, read at the lines cited.
- Read in part, by the lines cited and by search: spec/Core/DESIGN.md (Q14 and Q16 whole, :3564-3830 and :3894-4391;
  :1785-1830, :1909-1934, :2215-2234, :2378-2400, :3239-3244, :3449-3500, :4752-4800); spec/Core/schema.md (:119-130,
  :144-170, :466-470, :494-499); spec/CLAUDE.md (:1-94, :1439-1456 of 2109 lines); spec/NOTES.md (:1612 and the
  lines cad8e3581 added, of 1645); spec/BRIEF.md (section 3, :61-145); spec/REWRITE.md (section 2.6, :431-460);
  spec/Validation/features.json, spec/FusionComponents/module.md and features.json, spec/probes/facts.json and
  spec/Reference/move-in.md (:1356-1470), each by search.
- Not read: the rest of those files; spec/Execution/Run.tla; Lanes.tla, Ownership.tla and Request.tla past the lines
  cited; .claude/rules/analyzers.md; the module tests; spec/api; the Net48 twin's views.
- Read outside the base, as a lead and for the owner's "1" alone: ../core-leads/.landing/CoreLeads/design.md:54-70
  and :108-133, whose lead-4 form is `pipeline.Validate(container, ruleSet)`. What "1" numbered is not in it, so
  F5 was weighed against both forms the base gives and not assumed.
- Tests opened: the five use-cases.md cites, and WhenARequestSendsNoContainer, WhenARequestSendsAnInputOutsideEveryContainer,
  WhenALiveContainerHoldsTwoMembers, WhenACondensedRequestIsRefusedWithErrors, WhenACoordinatorJoinsTheWaitlist,
  WhenAPartialHoldsItsOwnValidation, WhenEachStatesPartialHasItsValidator, WhenACoordinatorAddsAContact,
  WhenACoordinatorRemovesAContact, WhenACoordinatorDiscardsTheDraft, WhenAFlagsEnumIsCheckedInTheBrowser,
  WhenTheServersConditionHoldsARule, WhenTheServerKeysAFailureToAListItem and WhenARowLoadsIntoTheRoundsContainer,
  each by its gestures and assertions; a helper's body was not opened. No test was run.
- Did not run: any browser test, build, pure test or plant; TLC on any spec but the two copies of section 6,
  Partials.tla among those not run; the probe of 2.5; the walk of 4.3.
- `git status --short` in the worktree printed " M spec/MISTAKES.md" alone, the change the worktree held at its start.
