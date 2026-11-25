# Designer notes (scratch, not the answer) - base 830817f4b

## Read so far (whole unless marked)
- .claude/agents/designer.md, CLAUDE.md (root, via session), .claude/rules/{docs,process,views,csharp,tests}.md, Alis.Reactive/CLAUDE.md,
  Alis.Reactive.Assets/CLAUDE.md, tests/Alis.Reactive.Conformance.App/CLAUDE.md, spec/MISTAKES.md, spec/Validation/module.md,
  spec/Validation/Validation.tla, spec/InputsAndIds/ContainerRead.tla, brief.md, use-cases.md, form-data-boundary-design.md
- spec/Core/DESIGN.md: Q16 3894-4391 whole, Q14 3564-3830 whole (rest: pending parts)
- Source whole: Validation/{RequestChecks,FormCheck,FormChecks,FormPlace,LiveChecks,ValidatorCatalog,Rules,ReactiveValidator,BrowserRegistrations,BrowserRules}.cs,
  TriggersAndPipelines/RequestSteps.cs, RequestsAndResponses/RequestContainer.cs, InputsAndIds/{RequestBlock,SentInputs,ContainerReads,ModelPlugin,FieldPrefixes}.cs,
  runtime InputsAndIds/model.ts, Validation/forms.ts
- Views whole: Care/Admission/{Circle,CircleMemberCard,KinCards,KinCopyCard}.cshtml, Care/MoveIn/{Contacts,Contact,ContactRows,DraftCommands}.cshtml

## Facts found (file:line at base)
- Rule set selection, server: Rules.cs:22-27 Selects: "*" all; sets empty -> include || Selection=="default"; else sets contains Selection (OrdinalIgnoreCase).
  Rules.cs:111 a rule not selected returns before Check. ChildPart.Validate Rules.cs:384-385 -> Run.Child Rules.cs:34-39 runs child's RunRules with THE SAME run (same Selection).
  => a child validator's rules are selected by the same rule set name, and only if the holding rule (RuleFor/RuleForEach..SetValidator) is itself selected.
- Browser: BrowserRules.cs:89 scope.Selects -> same ValidationRun; Rules.cs:121 BrowserGroups returns [] when not selected; Child scope BrowserRules.cs:121-122 keeps _selection. Same on both sides.
- RuleSets.Named Rules.cs:51 splits on , and ; and trims. ReactiveValidator.RuleSet ReactiveValidator.cs:116-123.
- => B1s: .Validate<RosterRules>(ShiftRules.Approval) with RuleForEach in default set runs NOTHING (both sides agree, silently passes). Dev writes RuleSet("default, Approval", () => RuleForEach(..).SetValidator(new ShiftRules())).
- FormChecks.WithNoRequest FormChecks.cs:16-42: rule set hard-coded DefaultRuleSet (:30), registration by catalog.Find(typeof(TContainer)) (:21), body = ModelPlugin.Captured(plan, containerId, modelKey) (:36), format urlEncoded (:47), place summary of containerId (:51-55), Stop on failure (:41).
- RequestChecks.Named RequestChecks.cs:48-55 (validator + ruleSet), Default :38-46. key = validatorKey + ":" + ruleSet (:62).
- LiveChecks.Of LiveChecks.cs:17-32: default rule set hard-coded (:24), per container, Live(validatorKey, ruleSet, {format urlEncoded, value: ModelPlugin.Sent(plan, containerId, plan.Model)}).
- RequestBlock.cs:23 ModelPlugin.Contains(plan,id,kind,summary,live) -> Container(plan.Model, kind, summary, live).
- model.ts captured() :105-132: inputs = every page input inside the container element (DOM), whatever plan holds them; body written by input.binding.path; two inputs binding one name -> Stop (:116); model check :111.
- BoundInput.cs:53 path starts with PrefixSteps(names.Prefix) => an input rendered in a partial under prefix Rows[2] has path [Rows,2,Carer] and name "Rows[2].Carer" (D14.3 DESIGN:3582-3584).
  => a container of ShiftRow inside PartialFor(m => m.Rows[at]) captures {Rows:[,,{Carer}]}: its send posts "Rows[2].Carer" and a ShiftRow endpoint binds nothing. Candidate B as sketched does not work at base.
- forms.ts: Run.failures :145-149; #nameOf(path) :186-188 gives "" for a member no sent input holds; #show :97-105 clears + summary + inline for inputs sent; checkLive :78-85 inline only, filter failure.name === entry.binding.name.
- forms.ts checkedBody :202-216: none/text/file -> start; urlEncoded/multipart -> bound(form); json.
- forms.ts boundList :235-240: items = sent.map(...) -> a sparse array [empty,empty,{..}] : Array.prototype.map skips holes but keeps length => items has holes at 0,1 (undefined) -> reach() over items uses flatMap (skips holes?) CHECK.
- PublicAPI net10 Unshipped: 879 RequestSteps.Validate<TModel,TContainer>(Pipeline<TModel>, RequestContainer<TContainer>); 458 Pipeline<TModel>.Container<TContainer>(string);
  817/818 BeginRequestContainer/BeginRequestForm; 836/837 CondensedRequests.Form(BodyParts|QueryParts<TRequest>, RequestForm<TRequest>); 846/847 CondensedRequests.Inputs(BodyParts|QueryParts<TRequest>, RequestContainer<TRequest>);
  280-294 BodyParts.Json/Multipart/UrlEncoded(container|form, body); 167 ReactivePlan<TModel>.Rows<TRow>(string rowsId); 809/810 PartialFor; 512/513 PostRequest.Validate<TValidator>() / (string ruleSet); 503 NoValidation; 880-883 EndpointChecks.Validate(Async)<TRequest,TValidator>(controller, request[, ruleSet]).
- D lines: D16.2 DESIGN:3900-3913; D16.3 :3914-3928; D16.5 :3941-3960; D16.13 :4063-4080; D16.15 :4088-4095; N16.3 :4200-4203; D14.1 :3567; D14.3 :3582; D14.4 :3632-3645; D14.8 :3683-3689.
- App: move-in rows: ContactRows.cshtml:11-19 plan.Rows<ContactInput>("contacts") + NativeActionButton((activated, pipeline) => ...activated.Row.Read(row => row.Id)); DraftCommands.cshtml:113-122 Delete<ContactEdit> ... .Validate<ContactEditRules>(ContactEditRules.Removal); :207-221 Post<MedicationEdit> .Inputs(editor) ... .Validate<MedicationEditRules>(); :45 .Validate<MoveInRules>(MoveInRules.Draft).


## More facts (2)
- facts.json: core-query-list-of-objects-gap facts:1237-1244 (net10, query: "Lines[1].Name=B binds Lines=[]"); mvc5-list-ends-at-missing-index facts:1597-1604 (DefaultModelBinder.cs.txt:683);
  core-form-binds-indexed-name facts:103-109 (form keys Members[1].Name and Members[0].Name bind in index order). NO fact: net10 FORM body whose first index is not 0 -> name probe core-form-list-of-objects-gap.
  DESIGN:1820 cites both gap facts: "an item left out ends the list there on both hosts".
- D9.15 DESIGN:1909-1934: Set / Form / Inputs(container) "each under its binding name" / Input(r => r.Member, pipeline.Input(m => m.Member)) "sent under the request member's binding name"; one container per request; "A container is typed by its model, and a request takes only a container of its own model (N18.3)".
- D13.3 :3468-3481 container.Read; D13.5 :3488-3490 each-item rule on every row its body holds; D13.6 :3491-3493 summary; D12.7 :3239-3244 live; D17.15 :4752-4790 rows: RowActivated<TRow> Row + Element; control id {rowsId}-{position}-{n}.
- RequestPartsWriter.Input RPW:89-105: individual capture ModelPlugin.Input(plan, inputId, name) name = REQUEST member name; check entries path = BodySteps(member) RPW:307-311.
- DEFECT CANDIDATE (read, not run): forms.ts:151-159 #typed keys the scalar by input.entry.binding.path (the input's OWN path) and forms.ts:186-188 #nameOf finds the sent input by its own binding path;
  for an Input sent under a request member whose path differs (Rows[2].Carer sent as Carer) the typed-text refusal (D16.7 DESIGN:3988-3990) and the inline place (D16.13 :4071-4073) are missed in the browser's verdict; the server's 400 does show inline (failure name === input.name, forms.ts:101-103).
  App reach: git grep -n -E "\.Input\(" tests/*.cshtml minus component reads: 44 lines read; every request Input names the same path on both lambdas (ArrivalDesk:29, DeputyCode:28, GivenName:18, HomeLead:28, RoomChoice:15, WardAssignment:18, AssessmentFiling:20, Refusals/InputsAndIds/* 8 lines incl. MoveInPrefix* r.NextOfKin.Name vs m.Name under prefix NextOfKin). No page reaches it.
- Owner: spec/CLAUDE.md:17 (2026-11-20) "preserve existing use cases and establish the one shared operation before proposing a new overload. Each additional form needs a concrete case and a shown difference in meaning."
  NOTES.md:1612 verbatim: "Remember framework in browser do not guess." "existing use cases must still work, i m not looking for patch like thinking, say add overload, why not have one way to do things, if need more bring concrete reasons, examples and actuall design thinking".
  HANDOFF notes 2026-11-20: "The validation is built to work regardlessof consumers. But if consumer need something specific, we should design it like row level action which has same framework permitives"; "adapter is not default choice".
- spec/Validation/browser-validation-proposal.md (parked): typed model+validator+container; bulk save = one array in one container, row validator each item; shared-key collision: forms.ts:65-66 takes first registration matching key.
- NOTES after-1.0 table (cad8e3581): row 2 "An element or a slot named by a value read when the step runs ... activated row" ; row 3 "A request that sends values alone and still shows its failures in a container's summary | D16.13 and D13.6 place them in the notice | ... then a door that names the place".
- CoreLeads design (outside base, lane's ../core-leads/.landing/CoreLeads/design.md:54-70, a lead): lead 4 form pipeline.Validate(container, ruleSet); promise X16.48 on ResidentDesk.cshtml:13 naming MoveInRules.Identity; highest X16 is 47.
- move-in.md:1440 gap "A rule set for a pipeline's check"; :1424 nested validator's rules run under the rule set the check selects so declared in "default, Identity"; :1458 E8/E9 send no container -> failures in notice; :1463 new-contact container of ContactEdit inside main container, outer sends inner (X9.16).


## More facts (3)
- App's validation doors by count (commands printed): views with .Validate<: 55; lines .Validate<TValidator>(): 62; lines .Validate<TValidator>(named): 9 (DraftCommands:45,77,122; PharmacyReview:43; ResidentDesk:32; CareRota:21,25; EarlyCareRota:21,25); lines NoValidation(): 16; lines [a-z]+.Validate( : 1 (ResidentDesk.cshtml:13).
- Parse (my script over git grep of ".Verb<Model>(" lines in app views, span to next verb call or 25 lines): 295 typed verb calls; 78 over a model with a registered validator (62 Add<> lines, 84 validator classes); 71 name a validator, 3 NoValidation, 4 condensed default:
  Care/Admission/ArrivalDesk.cshtml:48 Post<ArrivalDesk> (.Inputs(arrival.Container)); Care/MoveIn/AllocationDesk.cshtml:24 Post<AllocationRequest>; Care/MoveIn/DraftCommands.cshtml:148 Post<ContactEdit>; Care/MoveIn/WaitlistEntry.cshtml:21 Post<WaitlistPage> (.Json(waitlist.Container,..)).
  16 NoValidation requests; none holds ShowValidationErrors in its statement; 3 over a registered model: Deposit.cshtml:50 Get<DepositPage>, DraftActions.cshtml:15 Delete<MoveInPage>, QuoteSection.cshtml:41 Post<QuoteRequest>.
- Tests opened:
  * Validation/Admission/WhenAJsonSaveLeavesOutTheLeadsName.cs (X16.2): page HomeLead.cshtml names .Validate<HomeLeadRules>() (:25,:29) => proves F1e's door, NOT the condensed default; outcome: summary listitem "Lead name is required.", #saved empty; opposite saved "Ruth, 555-0100".
  * PartialsAndSlots/MoveIn/WhenACoordinatorJoinsTheWaitlist.cs M9.X66 (WaitlistEntry.cshtml:21, no Validate): message beside "Accept waitlist terms", SentTo(MoveInWaitlist)==0; opposite joins => proves F1 condensed default (and P2: slot partial own model/validator/container).
  * PartialsAndSlots/MoveIn/WhenAContactsRoleNeedsItsAuthority.cs M9.X46 (DraftCommands.cshtml:148 no Validate, no container): notice "An authority cannot end before it starts.", SentTo==0; M9.X47 revoked: notice "This authority reference was revoked.", SentTo==1 (server-only Must) => F8 proven in the NOTICE, not beside the input.
  * Validation/Admission/WhenARequestSendsAnInputOutsideEveryContainer.cs X16.16 (WardAssignment.cshtml:18-21 Validate<WardAssignmentRules>() + OnError<ValidationProblem>(400, ShowValidationErrors)): server's message beside the input and in the notice.
  * RequestsAndResponses/Admission/WhenACondensedRequestIsRefusedWithErrors.cs X10.12 (AdmissionDesk.cshtml:22 no Validate; AdmissionRequest has no browser check: under host noShowValidationErrors the 400 reaches the catch-all).
  * Validation/Admission/WhenTwoLiveContainersHoldOneInput.cs X16.20: blur shows "Name is required." once in group Resident; opposite Ann hidden. Validation/Admission/WhenALiveContainerHoldsTwoMembers.cs X16.41.
  * PartialsAndSlots/MoveIn/WhenACoordinatorChecksAResidentsDetails.cs M9.X21: Review resident details -> message beside Email confirmation + #move-in_summary, status empty; opposite "Resident details ready"; both choose a bed first. M9.X20 Verify identity (ResidentDesk.cshtml:32 Identity set) sent with no bed chosen.
  * Validation/Record/WhenAFlagsEnumIsCheckedInTheBrowser.cs X16.22 (CareRota.cshtml:21,25 named sets Care/Rota): Save care saved "Ada, Mon, Tue"; Save rota shows Rota message, saved unchanged => F2 proof.
  * Vendors/Bench/WhenEachEntryOfAListHasItsRule.cs (Cells.validation_listItem; EntriesForm.cshtml:26-27 .Validate<EntriesRules>()): "Pick a code." beside Second entry only, #shown empty; opposite both picked => B1 (named validator form).
  * Validation/Admission/WhenEachStatesPartialHasItsValidator.cs X16.5; PartialsAndSlots/Admission/WhenAPartialHoldsItsOwnValidation.cs M9.X9 => P2.
  * PartialsAndSlots/MoveIn/WhenACoordinatorAddsAContact.cs M9.X40 (add), M9.X41 (a row's malformed email refuses the add: notice + SentTo==0 + beside Alice's input) => B4: rows checked; the NEW contact's own rule failing has no case here.
  * PartialsAndSlots/MoveIn/WhenACoordinatorRemovesAContact.cs M9.X50, M9.X51 (row with no id: notice "Choose a contact.", nothing sent; opposite removed) => B5.
  * PartialsAndSlots/MoveIn/WhenACoordinatorDiscardsTheDraft.cs M9.X225 (DraftActions.cshtml:20 NoValidation) discard sent on the page as it opens.
- 26 views the app's 30 PartialFor( lines render: none holds BeginRequestContainer/BeginRequestForm => no app page renders a container block under a field prefix (EditorFor templates and <partial for> not read).
- request.ts:187-200 setInput reads the captured body at input.entry.binding.path and appends under input.name; :200 write(request, input.name.split("."), ..); :224-227 withContainer same path.
- TLA: Request.tla:178 Check action, :294 FailedCheckSendsNothing; Lanes.tla:199 SkippedTriggerStartsNothing, :195 ReplacedRunRunsNoRoute; Ownership.tla:415 RulesLeaveWithTheirOwner, :432 FailureReachesSummary, :434 InlineOnlyAtTheSentInput; ContainerRead.tla invariants in cfg (6).
- Base TLC (lead's run, scratch/probe-tlc/tlc.log): "Model checking completed. No error has been found." "114912 states generated, 8352 distinct states found, 0 states left on queue."
- RequestBlock.cs:20 renders "form" or "fieldset" only; D9.15 DESIGN:1919 names "a table row" as a container element: GAP (no door for a <tr> container).

## Design direction (before advisor)
- B2/B2s/B2c: row = partial of ShiftRow (PartialFor(m => m.Rows[at], view)) holding BeginRequestContainer(plan, "shift-"+id) -> RequestContainer<ShiftRow>; request.Inputs(shift.Container); .Validate<ShiftRules>(ShiftRules.Approval); pipeline.Validate(shift.Container[, ruleSet]).
  New MEANING (no new door): a container sends/reads its inputs under the names past the prefix its own view was rendered under. Plan: Container gains prefix {name, path}; SentInput gains path (sent path). Runtime: model.ts captured()/input(), forms.ts #typed/#nameOf/checkLive, request.ts setInput/withContainer.
  Also fixes the Input-door defect (sent path != binding path).
- F5: pipeline.Validate(container, ruleSet) overload (N16.3), rule set = validator's constant; registered validator of the container's model.
- F7: live check rule set: door on the container block? decide.
- B1s: developer writes RuleSet("default, Approval", () => RuleForEach(..).SetValidator(new ShiftRules())) ; no framework change; maybe refusal of a rule set no rule declares.
- Candidate A (activated.Inputs) waits on after-1.0 "element named by a run-time value".
