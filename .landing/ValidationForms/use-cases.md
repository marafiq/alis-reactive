# Validation: every use case and the form a developer writes

The owner, 2026-11-25: "write these use cases a md document, take this to fable xhigh designer, ask it to deliver on
correctness, grammer consistency, quality, typed and your document must contain small but complete use examples. so
design from fable is expected to be complete, then it gets tla+ and then it lands without loosing quality". On bulk
edit: "Bulk Edit is not about checking one row alone, it has two use cases, if action is in row, then row validate, if
its applying to all rows then all validate." And: "Its very critical to get this right with high quality code and keep
grammer and dev experience."

Base 830817f4b. Every example is an illustration over the two models below; each door it calls is quoted from the view
named beside it at the base. **Built** means that view writes the form today; a test named beside it is cited by the
file its promise constant names, unopened by the lead (the designer opens each and states whether it proves the case),
and passed in the land that made c3aeae654 ("836 selected, 836 ran, 836 passed, 0 failed, 0 over budget",
.work/logs/land-unit-31c256b0f-28029.log in the lane). **To design** states what the developer and the user need; its
form is the designer's, and a shape marked "candidate" is the lead's sketch, never a door that exists.

## The models

A resident's details, one form, and a rule set "Identity" that leaves the bed out:

```csharp illustration
public sealed class ResidentDetails
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? EmailConfirmation { get; set; }
    public string? Bed { get; set; }
}

public sealed class ResidentDetailsRules : ReactiveValidator<ResidentDetails>
{
    public const string Identity = "Identity";

    public ResidentDetailsRules()
    {
        RuleSet("default, " + Identity, () =>
        {
            RuleFor(resident => resident.Name).NotEmpty().WithMessage("Enter the name.");
            RuleFor(resident => resident.EmailConfirmation).Equal(resident => resident.Email).WithMessage("The email addresses must match.");
        });
        RuleFor(resident => resident.Bed).NotEmpty().WithMessage("Choose a bed.");
    }
}
```

A week's roster, edited in bulk, a row a shift, and a rule set "Approval" on the row:

```csharp illustration
public sealed class Roster
{
    public List<ShiftRow> Rows { get; set; } = [];
}

public sealed class ShiftRow
{
    public int Id { get; set; }
    public string? Carer { get; set; }
    public int Hours { get; set; }
}

public sealed class ShiftRules : ReactiveValidator<ShiftRow>
{
    public const string Approval = "Approval";

    public ShiftRules()
    {
        RuleFor(shift => shift.Carer).NotEmpty().WithMessage("Choose a carer.");
        RuleFor(shift => shift.Hours).InclusiveBetween(1, 12).WithMessage("A shift is 1 to 12 hours.");
        RuleSet(Approval, () => RuleFor(shift => shift.Hours).LessThanOrEqualTo(8).WithMessage("An approved shift is 8 hours at most."));
    }
}

public sealed class RosterRules : ReactiveValidator<Roster>
{
    public RosterRules() => RuleForEach(roster => roster.Rows).SetValidator(new ShiftRules());
}
```

Registered once each, a model one validator (D16.2), as tests/Alis.Reactive.Conformance.App/AppHost.cs registers:

```csharp illustration
services.AddReactiveFluentValidation(validators => validators
    .Add<ResidentDetailsRules>()
    .Add<RosterRules>()
    .Add<ShiftRules>());
```

An endpoint checks with the validator and rule set the browser ran (N16.1), as
tests/Alis.Reactive.Conformance.App/Care/Admission/AdmissionWardController.cs does:

```csharp illustration
[HttpPost(ResidentEndpoints.Details)]
public IActionResult Save([FromForm] ResidentDetails resident)
{
    var validation = this.Validate<ResidentDetails, ResidentDetailsRules>(resident);
    if (!validation.IsValid) return validation.ToErrorResponse();
    return Content("Saved", "text/plain");
}
```

## The doors a check is written with

Every request of every verb takes three, PublicAPI.Unshipped.txt lines 310 to 547 (`PostRequest<TModel, TRequest>` at
503, 512 and 513):

```csharp illustration
.Validate<TValidator>()
.Validate<TValidator>(string ruleSet)
.NoValidation()
```

With none of them, a request whose model has a registered validator is checked under its default rule set (D16.2).
A check with no request has one door, line 879: `RequestSteps.Validate<TModel, TContainer>(this Pipeline<TModel>,
RequestContainer<TContainer>)`.

## One form

The page, for every case of this section (the shape of
tests/Alis.Reactive.Conformance.App/Care/Admission/AdmissionDesk.cshtml, with the inner container of X9.16):

```cshtml illustration
@model ResidentDetails
@{
    var plan = Html.ReactivePlan<ResidentDetails>();
}
@using (var resident = Html.BeginRequestForm(plan, "resident"))
{
    @using (var identity = Html.BeginRequestContainer(plan, "identity"))
    {
        @(Html.InputField(plan, m => m.Name, options => options.Label("Name")).NativeTextBox())
        @(Html.InputField(plan, m => m.Email, options => options.Label("Email")).NativeTextBox())
        @(Html.InputField(plan, m => m.EmailConfirmation, options => options.Label("Confirm email")).NativeTextBox())
    }
    @(Html.InputField(plan, m => m.Bed, options => options.Label("Bed")).NativeTextBox())
    @* the buttons of F1 to F8 *@
}
<p id="saved"></p>
@Html.RenderPlan(plan)
```

### F1. Save, checked by the model's validator under its default rule set: built

Nothing beyond the request (D16.2's condensed default). With no bed chosen the user sees "Choose a bed." beside Bed and
in the form's summary, and nothing is sent. Test: `WhenAJsonSaveLeavesOutTheLeadsName` (X16.2).

```cshtml illustration
@(Html.NativeButton("save", "Save").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Details, request => request.Form(resident.Form))
    .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("saved").Text.Set(saved.Body))))
```

### F1e. Save, the validator named, its default rule set: built

`.Validate<EntriesRules>()`, as tests/Alis.Reactive.Conformance.App/Bench/Validation/EntriesForm.cshtml writes it. The designer
states whether F1 and F1e are one meaning with two doors, and which a developer is shown first.

### F2. Save under a named rule set: built

Only the set's rules run: with no bed chosen and the emails equal, the save is sent. Written at
tests/Alis.Reactive.Conformance.App/Care/MoveIn/DraftCommands.cshtml, `.Validate<ContactEditRules>(ContactEditRules.Removal)`.

```cshtml illustration
@(Html.NativeButton("save-identity", "Save identity").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Identity, request => request.Form(resident.Form))
    .Validate<ResidentDetailsRules>(ResidentDetailsRules.Identity)
    .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("saved").Text.Set(saved.Body))))
```

### F3. Save with no browser check; the endpoint's refusal shown: built

`.NoValidation()` (tests/Alis.Reactive.Conformance.App/Care/MoveIn/AdmissionHandover.cshtml); the endpoint's 400 shown by
`ShowValidationErrors` (tests/Alis.Reactive.Conformance.App/Care/Admission/AddressedMoveIn.cshtml).

```cshtml illustration
@(Html.NativeButton("save-unchecked", "Save").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Post<ResidentDetails>(ResidentEndpoints.Details, request => request.Form(resident.Form))
    .NoValidation()
    .OnError<ValidationProblem>(400, (refused, pipeline) => pipeline.ShowValidationErrors(refused.Body))))
```

### F4. Check without saving, under the default rule set: built

N16.3. The failures show and the pipeline stops before "Details ready". Written at
tests/Alis.Reactive.Conformance.App/Care/MoveIn/ResidentDesk.cshtml; test `WhenACoordinatorChecksAResidentsDetails` (M9.X21).

```cshtml illustration
@(Html.NativeButton("review", "Review details").Reactive(plan, button => button.Click, (clicked, pipeline) =>
{
    pipeline.Validate(resident.Form);
    pipeline.Element("saved").Text.Set("Details ready");
}))
```

(`RequestForm<TModel>` is a `RequestContainer<TModel>`, Alis.Reactive/RequestsAndResponses/RequestContainer.cs, so the
form's handle and `pipeline.Container<ResidentDetails>("resident")`, the door another view uses (D14.8), both compile.)

### F5. Check without saving, under a named rule set: to design (lead 4)

A wizard's step: "Next" checks the identity fieldset alone, under "Identity", so the bed, which the fieldset does not
hold and a later step asks for, neither blocks it nor shows a message. The owner's leaning of 2026-11-25 for lead 4:
"1 seems more aligned". Candidate:

```cshtml illustration
@(Html.NativeButton("next", "Next").Reactive(plan, button => button.Click, (clicked, pipeline) =>
{
    pipeline.Validate(identity.Container, ResidentDetailsRules.Identity);
    pipeline.Element("step").Text.Set("Step 2: the bed");
}))
```

The question under it: F4's check of the fieldset under the default set reads Bed as the endpoint binds it, absent,
and "Choose a bed." fails on a member with no input in the checked container (D16.5, D16.13); the designer states
where that message goes, or why a check of a part needs its rule set.

### F6. Live check on typing, blur and commit: built

Nothing: the container's block carries it (D16.15). Typing clears the input's message, and blur checks the member;
the message shows beside the input alone. Test: `WhenTwoLiveContainersHoldOneInput` (X16.20).

### F7. Live check under a named rule set, or none: to design

D16.15: "the door that names a rule set or none is declared with the first view that names one." F5's wizard is that
view: a step's inputs checked live under the step's set.

### F8. A rule only the server decides: built

`Must`, `MustAsync`, `.ServerOnly()` (D16.9): the browser sends, the endpoint refuses, and F3's `OnError` shows the
message beside the input. Test: `WhenAContactsRoleNeedsItsAuthority`, its case
"a_revoked_authority_reference_is_refused_by_the_server_with_its_reason".

## Partials

### P1. An attached partial checked with its parent: built (D16.3)

The parent's validator holds the member's validator; the partial adds nothing. Quoted from
tests/Alis.Reactive.Conformance.App/Care/MoveIn/Contacts.cshtml and MoveInRules.Contacts.cs at the base:

```cshtml
@await Html.PartialFor(m => m.Rows[at], "/Care/MoveIn/Contact.cshtml")
```

```csharp
partial void TheContacts() => RuleForEach(moveIn => moveIn.Contacts.Rows).SetValidator(new ContactRules(reference => false));
```

The designer cites its test.

### P2. An independent partial in a slot, checked alone: built (D16.3, D12.4)

Its own model, plan, validator, requests and form; its registrations leave with its slot. The designer cites its view
and test.

## Bulk edit: the action decides what is checked

The roster renders each row under the list's index, inside one form:

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
            @(Html.InputField(plan, m => m.Rows[at].Carer, options => options.Label("Carer")).NativeTextBox())
            @(Html.InputField(plan, m => m.Rows[at].Hours, options => options.Label("Hours")).NativeTextBox())
            @* B2's row actions *@
        </fieldset>
    }
    @* B1's actions for all rows *@
}
<p id="roster-status"></p>
@Html.RenderPlan(plan)
```

### B1. An action on all rows, "Save all": every row is checked: built

`RosterRules` checks each row by `ShiftRules`; each failure shows beside its own row's input, and nothing is sent while
any row fails. Test: `WhenEachEntryOfAListHasItsRule` (its page tests/Alis.Reactive.Conformance.App/Bench/Validation/EntriesForm.cshtml,
which names its validator, `.Validate<EntriesRules>()`, F1e's form).

```cshtml illustration
@(Html.NativeButton("save-all", "Save all").Reactive(plan, button => button.Click, (clicked, pipeline) => pipeline
    .Put<Roster>(RosterEndpoints.Shifts, request => request.Form(roster.Form))
    .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("roster-status").Text.Set(saved.Body))))
```

### B1s. An action on all rows under a rule set, "Approve all": to design

Every row under `ShiftRules.Approval`: a 10-hour shift is refused with "An approved shift is 8 hours at most." beside
its row. The question: `.Validate<RosterRules>(ShiftRules.Approval)` runs only `RosterRules`' rules in that set, and its
`RuleForEach` is in the default set; the designer reads at the source (`Alis.Reactive/Validation/Rules.cs`) how a rule
set reaches a child validator, on both sides, and what the developer writes.

### B2. An action in a row, its "Save": that row alone is checked: to design

Row 2's Save checks row 2 by `ShiftRules` and sends row 2. Row 3 holding an empty carer neither blocks it nor shows a
message. Row 2's failures show beside row 2's inputs. The endpoint binds one row:

```csharp illustration
[HttpPut(RosterEndpoints.Shift)]
public IActionResult SaveShift(int id, [FromForm] ShiftRow shift)
{
    var validation = this.Validate<ShiftRow, ShiftRules>(shift);
    if (!validation.IsValid) return validation.ToErrorResponse();
    return Content($"Shift {id} saved", "text/plain");
}
```

What B2 meets at the base, read at the source:
- Row 2's inputs are named "Rows[2].Carer" and "Rows[2].Hours". `request.Form(..)` takes a `RequestForm<TRequest>` of the
  request's own model (PublicAPI.Unshipped.txt, `CondensedRequests.Form`), so `roster.Form` sends every row as a
  `Roster`.
- A container of `Roster` around one row would send "Rows[2].Carer" alone. The probe fact
  `core-query-list-of-objects-gap` (spec/probes/facts.json) states, for a query on net10: "Lines[1].Name=B binds
  Lines=[]". A search of spec/probes/facts.json for a statement naming a form or a body with an index or a list of
  objects printed 2: facts:104, "Form keys Members[1].Name and Members[0].Name bind the list in index order.", and
  facts:1697, a multipart body on net10. Neither states a form body whose first index is not 0. If such a body binds as
  the query does, D16.5's check reads an empty list and passes row 2 whatever it holds. MVC 5's binder is source under
  spec/probes/aspnetwebstack.
- A row action of `plan.Rows<T>` (D17.15; tests/Alis.Reactive.Conformance.App/Care/MoveIn/ContactRows.cshtml) hands its
  block `Row`, the row it was rendered with, read as data, and `Element`, the control activated. Neither is the row's
  inputs.

Two candidates, the lead's sketches, neither a door that exists. Candidate A, a row action that hands over its own
row's inputs (`Inputs` is no member of `RowActivated<TRow>` at the base):

```cshtml illustration
@{
    var shifts = plan.Rows<ShiftRow>("shifts");
    var save = shifts.NativeActionButton((activated, pipeline) => pipeline
        .Put<ShiftRow>(RosterEndpoints.Shift, request => request.Form(activated.Inputs))
        .Validate<ShiftRules>());
}
```

Candidate B, a row a partial of `ShiftRow` holding a container of its own model, its action inside it:

```cshtml illustration
@* /Roster/Shift.cshtml, @model ShiftRow, rendered by Html.PartialFor(m => m.Rows[at], "/Roster/Shift.cshtml") *@
@using (var shift = Html.BeginRequestContainer(plan, "shift-" + Model.Id))
{
    @(Html.InputField(plan, m => m.Carer, options => options.Label("Carer")).NativeTextBox())
    @(Html.InputField(plan, m => m.Hours, options => options.Label("Hours")).NativeTextBox())
    @* Save: a PUT of this container's inputs as a ShiftRow, checked by ShiftRules *@
}
```

Its open questions: how a container of `ShiftRow` reads inputs named under the list's prefix and sends them as the
endpoint binds a `ShiftRow`; which door sends a container's inputs as a request's whole body; and how B1's form and
B2's containers hold the same inputs, B1 sending every row and B2 one.

### B2s. An action in a row under a rule set, its "Approve": to design

Row 2 under `ShiftRules.Approval` alone; the same page as B2.

### B2c. An action in a row that checks without saving, its "Check": to design

Row 2 checked as B2 checks it, nothing sent; F4's form for a row.

### B3. A live check in a row: built for the form

F6 over the roster's form: a row's input shows its own message (D16.15). A row's container, if B2 gives one, is a
second live container holding the input, and X16.20 shows such a message once.

### B4. An action that adds a row: built

The new row's inputs, a container of the edit's model inside the page's form, are checked by the item's validator held
on the new member: `RuleFor(edit => edit.NewContact!).SetValidator(aContact)`, in `ContactEditRules`,
tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveInRules.Contacts.cs; the save at
tests/Alis.Reactive.Conformance.App/Care/MoveIn/DraftCommands.cshtml. The designer cites its test.

### B5. An action in a row that removes it: built

The row's control dispatches its id (ContactRows.cshtml); the removal is checked under its own set,
`.Validate<ContactEditRules>(ContactEditRules.Removal)` (DraftCommands.cshtml), which checks only what the removal sends.

## A grid's row edit

### G1. A cell edited in a grid row, saved with its row: to design

### G2. A row edited in a grid's edit dialog: to design

`FusionGridEditTemplates` (Alis.Reactive.Fusion/FusionComponents/FusionGrid/FusionGridEditTemplates.cs) writes a
column's cell editor (`Select`, `DateInput`) and the dialog's form (`DialogForm<TRow>`). A search of
tests/Alis.Reactive.Conformance.App for "EditTemplate" and "FusionGridEditTemplates" printed no file. The developer
needs the edited row checked by `ShiftRules` before the grid keeps it, its messages beside the cell or the dialog's
field, as B2 does for a row of inputs.

## What every form keeps

- One door for one meaning, and every door typed: no string that names a member (ALIS009). A rule set is passed as the
  validator's own constant, as F2 does.
- The closed grammar: no new node where a landed one carries the meaning; a new production is the owner's.
- D16.5: the browser's verdict equals the endpoint's on the model the endpoint binds, on net10 and net48, under the
  owner's choice C of 2026-11-25 for a member sent outside the body (form-data-boundary-design.md in this folder).
- A failure shows where D16.13 places it: beside its input, and in the summary of the container the request sends,
  else the notice.
- Every ordering a form meets names its TLA+ spec and invariant: the owner's sequence is the design, then TLA+, then
  the build.
