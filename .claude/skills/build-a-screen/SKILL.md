---
name: build-a-screen
description: >-
  Choose the typed Alis.Reactive doors for an ASP.NET MVC screen from its values, events and ordering.
  Use when building or changing a screen, its requests, validation, partial views or shared app objects.
---

# Build a screen

## Where does the screen's plan live?

`spec/Fluent/sentences.json` maps the names to `spec/Core/DESIGN.md` and each module's design.
`spec/Core/schema.md` states their meaning; the public signatures are in
`Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt` and
`Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt`.

- Open the plan? `ReactivePlan` takes the view's model type; every view keeps its own plan.
  `tests/Alis.Reactive.Conformance.App/Care/Record/RoundBell.cshtml`.
- Finish the plan? `RenderPlan` takes that plan once, as the view's last line after every door that writes it.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.

## When does it run?

- When the plan starts? `DomReady` takes a pipeline through `On`; a loaded partial's plan starts on its load.
  `tests/Alis.Reactive.Conformance.App/Care/Record/RoundBell.cshtml`.
- When a component raises an event? `Reactive` takes its event selector and a pipeline, with the typed payload.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/RoomChoice.cshtml`.
- When the app raises a declared event? `CustomEvent` takes an `AppEvent` and a pipeline, with its data if declared.
  `tests/Alis.Reactive.Conformance.App/Care/Record/BedsideBell.cshtml`.
- From a button? `Raises` takes the plan, declared event and optional typed payload; `Dispatch` raises it in a step.
  `tests/Alis.Reactive.Conformance.App/Care/Record/BedsideBell.cshtml`.
- On an object reached during a step? `Listen` takes the object, event selector and handler for this plan's owner.
  `tests/Alis.Reactive.Conformance.App/Bench/Lanes/LaneListeningCard.cshtml`.
- Keep both runs? `HandlerPolicy.Overlap` starts each event's run independently and is the default.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/RoomChoice.cshtml`.
- Keep the latest run? `HandlerPolicy.Replace` aborts the old run so its late reply runs no route.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Ignore another event until finished? `HandlerPolicy.Skip` starts nothing while this handler's run is in flight.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ChartSave.cshtml`.

A drop-down whose change replaces the run before it and loads the next list, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`:

```cshtml
            @(Html.InputField(plan, m => m.Ward, options => options.Label("Ward")).NativeDropDown(build => build.Placeholder("Choose a ward").Items(ArrivalDeskLists.Wards()))
                .Reactive(ward => ward.Change, HandlerPolicy.Replace, (changed, pipeline) => pipeline
                    .Get<ArrivalRoomsQuery>(ArrivalDeskEndpoints.Rooms, request => request.Input(rooms => rooms.Ward, pipeline.Input(m => m.Ward)))
                    .Into("arrival-room")))
```

A handler stays in flight through its awaited tails and cleanup: `spec/Execution/Lanes.tla`,
SkippedTriggerStartsNothing, ReplacedRunRunsNoRoute and ReplacedRunRunsNothingAfter.
Replacement preserves entered cleanup: `spec/Execution/Run.tla`, ReplacedFinallyGoesOn.

## What does a step do?

- Write a declared property? `Set` takes a literal or source on a `Property` or `WriteOnly` member.
  `tests/Alis.Reactive.Conformance.App/Bench/Sources/SourcePropertySet.cshtml`.
- Show text or HTML? `Element` takes an element id; its `Text` or `Html` takes `Set`, while `Append` retains children.
  `tests/Alis.Reactive.Conformance.App/Care/Record/RecordCard.cshtml`.
- Change classes? `Classes` offers `Add`, `Remove` and `Toggle`, each taking a class name or text source.
  `tests/Alis.Reactive.Conformance.App/Care/Record/RecordCard.cshtml`.
- Call an object's operation? `Plugin` takes its declaration type, then the declared method takes typed arguments.
  `tests/Alis.Reactive.Conformance.App/Bench/Plugins/ShiftLogDesk.cshtml`.
- Answer a component's callback? `Callback` takes `Set` with an expression that returns its answer, without steps.
  `tests/Alis.Reactive.Conformance.App/Bench/Plugins/ShiftLogDesk.cshtml`.

Steps run in authored order; an awaited step holds every step after its whole block.
A stop or error ends that pipeline and runs its owed cleanup: `spec/Execution/Run.tla`,
NothingRunsWhileItWaits, NothingRunsAfterAStop, NothingRunsAfterAnError and FinallyRunsOnce.

## Where does this value come from?

Any member that returns a value is a source wherever a source of that type is taken.
Reads, writes, comparisons and arguments keep their declared types.

- A value known when the view renders? `Set` accepts the literal directly, including a value from the view's model.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/BadgeDesk.cshtml`.
- A component property? `Component` takes its declaration type and element id; `Property` and `ReadOnly` are sources.
  `tests/Alis.Reactive.Conformance.App/Bench/Sources/SourcePropertySet.cshtml`.
- An input's component? `Input` takes the model member, then `Component` takes the input component's type.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- A plugin member? `Plugin` takes its declaration type without an element id.
  `tests/Alis.Reactive.Conformance.App/Bench/Plugins/ShiftLogDesk.cshtml`.
- A method's result? `MethodResult` is the source returned by the declared call; an async call waits before use.
  `tests/Alis.Reactive.Conformance.App/Bench/Methods/MethodAsyncValue.cshtml`.
- Event or pushed data? `Read` takes a member lambda; a live event payload exposes its declared members directly.
  `tests/Alis.Reactive.Conformance.App/Care/Record/BedsideBell.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveDoses.cshtml`.
- A returned live object? Call its declared method on `Component`, then read the returned object's members directly.
  `tests/Alis.Reactive.Conformance.App/Bench/Methods/MethodObjectMemberRead.cshtml`.
- Response data? `Body` exposes the declared body; `Read` takes its typed member path, including nested members.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- A container's current body? `Container` takes its model type and id; its `Read` takes the member as it would bind.
  `tests/Alis.Reactive.Conformance.App/Care/Record/NotePreview.cshtml`.
- The page's address? `PageUrl` takes the page type and optional path template; `Read` selects a path or query member.
  `tests/Alis.Reactive.Conformance.App/Care/Record/Chart.cshtml`.
- An object assembled from sources? `Object` takes its type and a block of `Set` calls with member lambdas.
  `tests/Alis.Reactive.Conformance.App/Care/Record/WardTransfer.cshtml`.
- Text assembled from values? `Concat` takes text and sources in order.
  `tests/Alis.Reactive.Conformance.App/Care/Round/DoseLookup.cshtml`.
- A fallback? `IfNull` takes a literal or source read only when the first source holds null or reads nothing.
  `tests/Alis.Reactive.Conformance.App/Care/Record/PumpLog.cshtml`.
- One of two values? `Choose` takes the true and false values on a boolean source and reads only the chosen value.
  `tests/Alis.Reactive.Conformance.App/Care/Record/FluidOrders.cshtml`.
- The same value at a nullable position? `OrNull` widens the source's type without choosing a replacement.
  `tests/Alis.Reactive.Conformance.App/Care/Record/IntakeRecord.cshtml`.
- One field per list item? `Select` takes a field lambda and returns the projected list.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/CensusCodes.cshtml`.
- Ordered items? `OrderBy` or `OrderByDescending` takes a key lambda and returns a sorted copy.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/CensusSort.cshtml`.
- A matching item? `Find` takes a predicate and optionally a field selector; `At` selects a zero-based position.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/CensusSort.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Residents/CensusItemPositions.cshtml`.
- A count or predicate result? `Count`, `Any` and `All` take item predicates; these results are sources too.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/DoseChecks.cshtml`.
- A numeric total? `Sum` takes the item's numeric member and adds it with that number type's meaning.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/Billing.cshtml`.

A present data holder's absent member reads its binding start; a null holder reads nothing through later members.
The rules are D4.2 in `spec/Core/DESIGN.md`; use the declared nullability, without inventing a fallback.
A list assembled from separate sources has no public construction door here; raise that gap.

## Which branch runs?

- Choose steps? `When` takes a boolean source, `Then` its true block, `ElseIf` another condition and `Else` the rest.
  `tests/Alis.Reactive.Conformance.App/Care/Record/PainAssessment.cshtml`.
- Compare values? `Eq`, `NotEq`, `Gt`, `Gte`, `Lt` and `Lte` take a literal or source on the right of a source.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/OpenDayBooking.cshtml`.
- Combine conditions? `And` and `Or` read the right term only when needed; `Not` negates the boolean source.
  `tests/Alis.Reactive.Conformance.App/Care/Record/PainReliefCheckedInItsTerms.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Round/WardBoard.cshtml`.

A branch holds every step it starts, including requests, before later steps run: `spec/Execution/Run.tla`,
NothingRunsWhileItWaits. Keep the continuation of a condition together, before another step.

## How do I send it and handle the answer?

- Read a resource? `Get` takes the URL, plus a request type and parts lambda when sending typed members.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Create or submit? `Post` takes the endpoint's request type, URL and parts lambda.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Replace a resource? `Put` has the same typed parts form.
  `tests/Alis.Reactive.Conformance.App/Bench/Http/HttpPut.cshtml`.
- Delete a resource? `Delete` has the same typed parts form.
  `tests/Alis.Reactive.Conformance.App/Bench/Http/HttpDelete.cshtml`.
- Read only status and headers? `Head` has no body in its request or routes.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/PhotoBadge.cshtml`.
- Fill a URL placeholder? `Path` takes the request member and its value; every placeholder needs a scalar value.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/FreeBed.cshtml`.
- Add query data? `Query` takes the request member and value, including on a request that sends a body.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Add a header? `Header` takes a header name and text or source; a per-request header overrides its default.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Send named values as JSON? `Json` takes a body block whose `Set` calls select request members.
  `tests/Alis.Reactive.Conformance.App/Care/Round/DoseLines.cshtml`.
- Send URL-encoded fields? `UrlEncoded` takes the body block, preceded by the typed form or container if sent.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/IntakeAdmit.cshtml`.
- Send one text body? `Text` takes that text source instead of a body with named members.
  `tests/Alis.Reactive.Conformance.App/Care/Round/NightHandover.cshtml`.
- Send native fields and files? `Form` takes the form handle; the condensed POST body is multipart.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/IntakeDocuments.cshtml`.
- Send a member only under a condition? `When` on the parts or body takes the condition, then `Then` adds the part.
  `tests/Alis.Reactive.Conformance.App/Care/Round/DoseLines.cshtml`.
- Check before sending? `Validate` takes the request model's validator type and optional rule-set name.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LeastDoseRound.cshtml`.
- Decode a reply? `OnSuccess` takes its JSON body type, or a content kind such as `Content.Text` or `Content.None`.
  `tests/Alis.Reactive.Conformance.App/Care/Record/SessionNotes.cshtml`.
- Handle a known refusal? `OnError` takes the status, its body type or content kind, and the reply's pipeline.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Handle other failures? `OnError` without a status receives `Failure`; its `Outcome` distinguishes the failure.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/BedCommitWithAConflictRoute.cshtml`.
- Which failure occurred? `Outcome` reports `FailureOutcome.Status`, `FailureOutcome.Network`,
  `FailureOutcome.Redirect` or `FailureOutcome.Content`; only the status outcome has a response body.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/BedCommitWithAConflictRoute.cshtml`.
- Read response metadata? `Status` is a source and `Headers` supplies `Get` with a header name.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/PhotoBadge.cshtml`.
- Save a response as a file? `Download` takes no argument and uses the response's filename.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/ResidentExports.cshtml`.
- Show work and clean up? `WhileLoading` and `Finally` each take a pipeline; their order is fixed below.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/FreeBed.cshtml`.
- Continue only after success? `Chained` takes the typed reply and its next pipeline, after the first cleanup.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/BadgeDesk.cshtml`.
- Start independent work together? `Parallel` takes pipeline branches; following steps wait for every branch's tail.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Mark one section busy? `Busy` takes element ids instead of the targets derived from the request's writes.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/FreeBed.cshtml`.
- Disable retry for this request? `NoRetry` takes no argument; retry's policy is configured in the defaults.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/CensusRetriedOnce.cshtml`.
- Omit the defaults entirely? `NoDefaults` takes no argument and removes their routes, loading, cleanup and busy.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/BedAdmission.cshtml`.

A POST that sends a header, a query member and its container's inputs, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`:

```cshtml
        Html.On(plan, trigger => trigger.CustomEvent(ArrivalDeskButtons.Arrive, pipeline => pipeline
            .Post<ArrivalDesk>(ArrivalDeskEndpoints.Arrivals, request => request
                .Header(ArrivalDeskEndpoints.WardHeader, pipeline.Input(m => m.Ward).Component<NativeDropDown>().Value)
                .Query(sent => sent.Rehearsal, pipeline.Input(m => m.Rehearsal).Component<NativeCheckBox>().Checked)
                .Inputs(arrival.Container))
```

Its refusal read by type and status, its cleanup and the request chained after its success, in the same view:

```cshtml
            .OnError<ArrivalConflict>(409, (taken, pipeline) =>
            {
                pipeline.Element("arrival-conflict").Text.Set(taken.Body.Read(conflict => conflict.Holder.Resident));
                pipeline.When(taken.Body.Read(conflict => conflict.Holder.AtThisDesk)).Then(pipeline => pipeline
                    .Get(ArrivalDeskEndpoints.Holder)
                    .OnSuccess(Content.Text, (holder, pipeline) => pipeline.Element("arrival-holder").Text.Set(holder.Body)));
            })
            .Finally(pipeline => pipeline.Element("arrival-desk-state").Text.Set("Ready for the next arrival"))
            .Chained((arrived, pipeline) => pipeline
                .Get<ArrivalBedCardQuery>(ArrivalDeskEndpoints.BedCard, request => request.Query(card => card.Room, arrived.Body.Read(reply => reply.Bed.Room.Number)))
                .Into("arrival-bed-card"))));
```

The condensed body is multipart; GET members go to the query; choose one body format per request.
Read the argument and check; run defaults' `WhileLoading`, then the request's; check boundaries and hold busy;
send and select one route; run the request's `Finally`, then defaults'; release busy; run `Chained` on success;
then run following steps. `spec/RequestsAndResponses/Request.tla`: StepsAfterRunUnlessStopped,
EachLayerPairs and ChainedOnlyAfterSuccess. A failed check sends nothing and enters neither loading nor cleanup:
FailedCheckSendsNothing. A loading layer that stops earns no cleanup of its own: FinallyRunsWhenLoadingRan.
A status route on the request wins, then the defaults' status route, request catch-all, defaults' catch-all:
OneRouteRuns and FailureRunsTheCatchAll. A failed part condition omits its value: FailedConditionLeavesThePartOut.
Retry spans one lifecycle and never retries POST: RetriesOnlyWhatIsRetryable and OneLifecycleAcrossAttempts.
`spec/Execution/Lanes.tla`: BranchStopEndsOnlyItself and JoinAfterEveryBranchTail hold parallel work.
`spec/RequestsAndResponses/Composition.tla`: HoldOncePerRequest and IndicatorNeverHidesEarly keep each target busy
until all requests holding it finish; IndicatorNeverStaysShown releases the last hold.

Two requests started together, each with its own route, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`:

```cshtml
    Html.On(plan, trigger => trigger
        .CustomEvent(ArrivalDeskButtons.AddKin, pipeline => pipeline.Get(ArrivalDeskEndpoints.Kin).Into("arrival-kin"))
        .CustomEvent(ArrivalDeskButtons.RefreshBoard, pipeline => pipeline.Parallel(
            pipeline => pipeline.Get(ArrivalDeskEndpoints.Census).OnSuccess(Content.Text, (census, pipeline) => pipeline.Element("board-census").Text.Set(census.Body)),
            pipeline => pipeline.Get(ArrivalDeskEndpoints.Alerts).OnSuccess(Content.Text, (alerts, pipeline) => pipeline.Element("board-alerts").Text.Set(alerts.Body)))));
```

## Who holds the inputs?

- Group framework inputs without a form? `BeginRequestContainer` takes the plan and chosen id and owns the summary.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Send that group? `Inputs` takes its `Container` handle and includes framework inputs inside it from every plan.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Include the browser's form submission set? `BeginRequestForm` supplies a `Form` handle for native controls too.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/IntakeAdmit.cshtml`.
- Include an input outside both? Request `Input` takes the request member and the view's typed `Input` handle.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/RoomChoice.cshtml`.
- Hide a bound input? `InputField` takes the plan and member; its options' `Hidden` keeps the binding and send.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Choose ids? `InputField` gives each input its framework id; other elements use ids chosen by the view.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.

A container that holds the screen's inputs, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`:

```cshtml
    @using (var arrival = Html.BeginRequestContainer(plan, "arrival"))
    {
        <fieldset>
            <legend>Resident</legend>
            @(Html.InputField(plan, m => m.Resident.Name, options => options.Label("Name")).NativeTextBox())
        </fieldset>
```

A request sends at most one container. An input wins over a `Set` of the same member;
two inputs with one scalar binding name stop the send.
`spec/InputsAndIds/ContainerRead.tla`: ReadIsWhatTheEndpointBinds, ReadFindsWhatItsContainerSends and
ReadSeesTheSlotThatJoinedOrLeftBeforeIt make a container read include the slots present at that read.
Query and form keys are C# member paths; JSON uses the endpoint's contract, as `.claude/rules/views.md` states.

## What does the validator decide?

`spec/Validation/module.md` and Q16 of `spec/Core/DESIGN.md` define one grammar for browser and server.

- A field rule? `ReactiveValidator` uses `RuleFor` with a member lambda, a typed check and `WithMessage`.
  `tests/Alis.Reactive.Conformance.App/Bench/Validation/CodeForm.cshtml`.
- A conditional rule? `When` or `Unless` takes a model expression; a group condition continues with `Otherwise`.
  `tests/Alis.Reactive.Conformance.App/Bench/Validation/StrictCodeForm.cshtml`.
- A nested model? `SetValidator` takes its validator after `RuleFor`; `ChildRules` writes its rules inline.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/DeputyAppointment.cshtml`.
- Each item? `RuleForEach` takes the collection member, followed by checks, `SetValidator` or `ChildRules`.
  `tests/Alis.Reactive.Conformance.App/Bench/Validation/EntriesForm.cshtml`.
- Compare two members? `GreaterThanOrEqualTo` takes the other member's lambda; an item rule reads its owning model.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LeastDoseRound.cshtml`.
- Keep a rule on the server? `Must`, `MustAsync`, `Custom`, `CustomAsync` and `ServerOnly` produce no browser rule.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- A server condition? `WhenOnServer`, `UnlessOnServer`, `WhenAsync` and `UnlessAsync` keep its rules on the server.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/VisitorSignIn.cshtml`.
- Return the server's verdict? `EndpointChecks.Validate` adds binding errors; `ToErrorResponse` returns 400 errors.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/VisitorSignIn.cshtml`.
- Place the server's messages? `ShowValidationErrors` takes the `ValidationProblem` body in a 400 route.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/AddressedMoveIn.cshtml`.

The registered model validator checks the request by default; an explicit `Validate` chooses its rule set.
Browser checks read the sent body and the binding starts of unsent members, never all inputs on the page.
Server-only rules, model-built messages and non-reactive child validators still need the endpoint's check.
Rules that wait require `ValidateAsync` on the server.
`spec/Validation/Validation.tla`: Agreement, OnlyTheNamedValidator, UnsentInputsUnread, ChildSkipAgrees,
NoTypedValueLost and ItemPeerAgrees hold those decisions.
Every failure reaches the sent container's summary, or the notice without a container; the first per member also
reaches its original sent input if it still lives. Hidden inputs and native controls have summary messages.

A save that checks its container by a validator of its own before it sends, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDeskKin.cshtml`:

```cshtml
    <button type="button" @Html.Raises(plan, ArrivalDeskButtons.SaveKin)>Save next of kin</button>
    Html.On(plan, trigger => trigger.CustomEvent(ArrivalDeskButtons.SaveKin, pipeline => pipeline
        .Post<ArrivalKin>(ArrivalDeskEndpoints.KinSaves, request => request.Inputs(kin.Container))
        .Validate<ArrivalKinRules>()
        .OnSuccess(Content.Text, (saved, pipeline) => pipeline.Element("arrival-kin-saved").Text.Set(saved.Body))));
```

## How does a partial join or leave the screen?

- Attach a partial's model to a member? `PartialFor` takes the member and optional view name, carrying its prefix.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/MoveInWithKin.cshtml`.
- Give a section independent work? `ReactivePlan` opens its own model's plan, with its own validator and requests.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDeskKin.cshtml`.
- Load a section under a condition? `When` guards the pipeline whose request's `Into` names the slot element id.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Remove that section? `Unload` takes the slot id and releases its content; hiding an element does not unload it.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.
- Include loaded inputs in the page's save? `Inputs` selects its container; the page's validator owns that check.
  `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`.

A partial loaded while a box is ticked and unloaded when it is not, in
`tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml`:

```cshtml
            @(Html.InputField(plan, m => m.Funded, options => options.Label("Publicly funded")).NativeCheckBox()
                .Reactive(funded => funded.Change, (changed, pipeline) => pipeline
                    .When(pipeline.Input(m => m.Funded).Component<NativeCheckBox>().Checked)
                    .Then(pipeline => pipeline.Get(ArrivalDeskEndpoints.Funding).Into("arrival-funding"))
                    .Else(pipeline => pipeline.Unload("arrival-funding"))))
```

Every partial still has its own plan; plans compose without merging or passing plan state between views.
`spec/PartialsAndSlots/Partials.tla`: AttachedInputBindsItsMember and PageReadFindsEachAttachedInput keep prefixes.
Load and unload end inner runs and cleanup before unmount, then remove listeners, connection holds and fields;
a load then starts the new content. `spec/Execution/Lanes.tla`: FinallyRunsBeforeRelease;
`spec/OwnershipAndState/Ownership.tla`: RulesLeaveWithTheirOwner and ListenersDoNotAccumulate.
The page's validator stays; unloading another plan does not remove its rules.
Use slot doors to replace slot content: `spec/OwnershipAndState/SlotHolds.tla`,
AStepOverANamedSlotThatHoldsNoneStops and AStepOverAnElementHoldingAHeldSlotStops forbid a `Text` or `Html` overwrite.

## How do pushed messages join the same plan?

- Receive server events on net10? `ServerPush` takes the declared stream, message selector and typed data pipeline.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveDoses.cshtml`.
- Receive hub messages on net10? `SignalR` takes the declared hub, push selector and typed data pipeline.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveStatusBoard.cshtml`.
- Call the hub for a value? `Hub` takes its declared source; a method declared with `Invoke` returns a source.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveWardLine.cshtml`.
- Read a connection? `Stream` takes its declared source; `State` is a value and `Close` ends a stream deliberately.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveWardCard.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveAlerts.cshtml`.
- React to connection changes? `Reconnecting`, `Reconnected` and `Closed` are lifecycle event selectors.
  `tests/Alis.Reactive.Conformance.App/Care/Round/LiveWardLine.cshtml`.

A connection opens at its first owner and closes at its last release: `spec/OwnershipAndState/Ownership.tla`,
HubKeptWhileHeld and OfferOnlyWhileHeld. Messages use the same handler policies and awaited-step order.
Connection recovery holds no busy indicator; requests started by messages take request defaults.
`spec/Realtime/Realtime.tla`: IndicatorNeverHidesEarly and IndicatorNeverStaysShown govern those requests;
OfferNeverHidesEarly and OfferNeverStaysShown keep the offer until every failed held connection opens or leaves.

## How do I use the app-level objects?

- Render shared objects? `NativeDrawer`, `NativeConfirm`, `NativeLoader` and `NativeToast` have layout helpers.
  `tests/Alis.Reactive.Conformance.App/Care/Shell/_ShellLayout.cshtml`.
- Ask before continuing? `NativeConfirm.Ask` takes text or a source; its awaited boolean answer feeds `When`.
  `tests/Alis.Reactive.Conformance.App/Care/Shell/DrawerDischargeNotes.cshtml`.
- Open a drawer? `Component` with `NativeDrawer` reaches `Open`, `Close` and `Size`; `Into` loads its `ContentId`.
  `tests/Alis.Reactive.Conformance.App/Care/Shell/DrawerDischarge.cshtml`.
- Decide whether to close? Handle `BeforeClose` with `Reactive` and write `Cancel` before the first awaited step.
  `tests/Alis.Reactive.Conformance.App/Care/Shell/CarePlanDrawer.cshtml`.
- Offer reconnection? `NativeReconnectOffer` takes the message and retry text in the layout.
  `tests/Alis.Reactive.Conformance.App/Bench/Lanes/_LiveLayout.cshtml`.
- Show a notice? `Notice` offers `Success`, `Error`, `Warning` and `Info`, each taking text or a text source.
  `tests/Alis.Reactive.Conformance.App/Care/Shell/RoundNotices.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Shell/DoseToasts.cshtml`;
  `tests/Alis.Reactive.Conformance.App/Care/Shell/WetBedNote.cshtml`.
- Act on a row? `Rows` takes the row type and list id; `NativeActionButton` declares the action once.
  The handler exposes `Row` and `Element`; `Render` takes each row and the button text.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/ResidentDischarges.cshtml`.
- Navigate from a row? `NativeActionLink` declares a GET action with the same row and render form.
  `tests/Alis.Reactive.Conformance.App/Care/Residents/ResidentLinks.cshtml`.

An event payload is writable only before the first await: `spec/Execution/Run.tla`, WritesAfterTheTurnAreRefused.

## What if no door fits?

Name the missing decision with its use case and the nearest public door.
Never invent a member, pass a string naming a member, or put JavaScript in a view.

A component's declaration belongs to `.claude/skills/onboard-component/SKILL.md`.
Proving a test belongs to `.claude/skills/prove-behavior/SKILL.md`.
