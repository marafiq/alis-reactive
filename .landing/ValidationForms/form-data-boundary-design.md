# The form-data boundary: what a check reads for a member sent outside the body

Base 31c256b0f, HANDOFF.md:115-120. A cite is a file and line at the base; a count a script made from a command's
lines is marked "parse". DESIGN is spec/Core/DESIGN.md, RPW is Alis.Reactive/RequestsAndResponses/RequestPartsWriter.cs,
forms.ts and request.ts are Alis.Reactive.Assets/runtime/Validation/forms.ts and
Alis.Reactive.Assets/runtime/RequestsAndResponses/request.ts, facts is spec/probes/facts.json, App is
tests/Alis.Reactive.Conformance.App. The decision: what a check reads for a member its request sends in its path or its
query, by host and body format, so "its verdict equals the door's on the model the endpoint binds" (DESIGN:3941-3942).

## 1. What the check reads today, from the code

| The request | Its check reads | Read at |
|---|---|---|
| GET, HEAD, OPTIONS | the members its path and query send, as form data | RPW:232-236, :304 |
| POST, PUT, DELETE with a JSON, URL-encoded or multipart body | the body, which also holds each path member; a `Query` member is not read | RPW:234, :407, :418, :423 |
| POST, PUT, DELETE with no body, a text or a file | every member at its start; the path's entries are not read | RPW:304, forms.ts:203-207 |

C# knows, where the view renders: the host (Alis.Reactive/Validation/RequestChecks.cs:64), the body's format
(RPW:297-305), each member and its place (RPW:486-488, :501-504, :516-520), the URL's own query text (RPW:246) and the
rules it writes into the registration. It does not know the action, its parameter's attribute or its route template:
DESIGN:1957-1958, "the core reads no controller action (the owner, 2026-11-07)", and :1969-1970, "a view names its
action only by URL text, so no build check refuses them, since none links a request to its action".

## 2. The family, each case with the line that gives it its meaning today

| # | A member sent | Its meaning today | The browser reads |
|---|---|---|---|
| 1 | in a body that binds members (JSON, URL-encoded, multipart) | DESIGN:3943, "A sent member reads as the endpoint binds it" | the body |
| 2 | in the path, with such a body | DESIGN:1785-1788, "A member that fills a placeholder is also sent in the body of a POST, PUT or DELETE ... so the body carries the same value the path does" | the body's copy |
| 3 | in the query, beside such a body | no line: DESIGN:3950-3951 names "its `Set` members and its `Input`s"; DESIGN:1745 states the door as `Query<TQuery>` over a type of its own, where the built door takes a member of the request's type (Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt:272-274, :288-290) | its start |
| 4 | in the path, with no body on POST, PUT or DELETE, a text or a file | DESIGN:3946-3949, "its check reads every member at its start: a member sent in the path or the query is not read, as an endpoint that binds its model `[FromForm]` leaves it. An endpoint that binds a plain model binds that member from the path or the query, and does not bind from that body (X16.46)" | its start |
| 5 | in the query, with such a body | the same line | its start |
| 6 | in the path or query of a GET, HEAD or OPTIONS | no line: a search of DESIGN:3894-4391 for "GET", "query" or "path" printed lines 3947, 3948, 3981, 3982, 4082, 4359, 4360 and 4387, none of a GET's check, and DESIGN:3946 says "as no body does" of every request with none | the path and the query (RPW:304) |
| 7 | by the URL alone: its own query text (DESIGN:1746, "The URL's own query text is kept") or a literal segment the endpoint's route names | no line | its start |
| 8 | outside the body and by an input of the sent container | DESIGN:1931-1932, "When an input and a set member send the same binding name, the input's value is sent, as the body the check reads" | the input (request.ts:188, :231; Alis.Reactive.Assets/runtime/InputsAndIds/model.ts:188-196) |
| 9 | twice by the request's own doors | refused where the view renders, "overlaps a member already supplied" (RPW:579-582) | nothing |

## 3. What each endpoint binds, and what pins it

| The endpoint | Its model binds | Pinned by | No probe |
|---|---|---|---|
| MVC 5, every model | the form, a JSON body, the route, the query, in that order | facts:1948, spec/probes/aspnetwebstack/ValueProviderFactories.cs.txt:10-14 | that the first provider holding a key wins (`git grep ValueProviderCollection 31c256b0f -- spec/probes` printed nothing); a text or a file body |
| net10, a plain model | the form, then the route | facts:1912, spec/probes/AspNetCoreBinding/HttpProbes.cs:57-60 and :197-198: a multipart body lacking Ward binds the route's, and the body's wins; a JSON body binds nothing, facts:1930; a GET's query, facts:1966 | no body, a text or a file; a query member beside a form body |
| net10, `[FromBody]` | the JSON body alone | facts:1912, "a [FromBody] model with {} binds 200 Ward null"; facts:1921; facts:211 | none |
| net10, `[FromForm]` | the form alone | for the route, X16.46's tests on net10 (spec/Reference/app.json:9687-9689; 6da8232ec's message, "M8_F4.X16_46_0 1/1: 1 selected, 1 ran, 1 passed") | every cell: HttpProbes.cs holds no `[FromForm]` action, and the two facts that name it, facts:85 and :94, are about names |
| net10, a member's own `[FromRoute]` or `[FromQuery]` | that place | nothing | every cell |
| an action that sets the member before its check | what it sets | one action of the app, App/Care/MoveIn/MoveInDeskController.Payer.cs:57, `page.Reference = reference;`, before the check at :58 | it is a fact of an app, not of a host |

## 4. Where the two verdicts differ, and what the app reaches

Both differences break DESIGN:3941-3942, and DESIGN:3948-3949 states the first itself.

- Cases 4 and 5 on MVC 5 and on a net10 plain model: the browser reads the start, the endpoint binds what was sent.
  Reached by no page. Six requests send a path member with such a body under a check, all of
  App/Care/Admission/BedNotes.cshtml:18-44; their six actions bind `[FromForm]`
  (App/Care/Admission/BedNotesController.cs:20-35) and answer the model's start, never the path's ward (:37-45;
  spec/Validation/features.json:2351, "noted reads Admissions: Bed by the window" for a note sent to North).
  6da8232ec's message: "Not run: MVC 5 ... an MVC 5 action's model binds from the route and the query".
- Case 3 on MVC 5, on a net10 plain model and where an action sets the member. Reached by no page with a rule the
  browser decides. Five requests send a query member beside such a body under a check:
  App/Care/Admission/ArrivalDesk.cshtml:48 (Rehearsal; ArrivalDeskController.cs:116-123 holds no rule over it),
  App/Care/MoveIn/DraftCommands.cshtml:34 (Referral, Rehearsal) and PharmacyReview.cshtml:32 (Reference), with no rule
  over those members of MoveInPage, ResidentDesk.cshtml:26 (ResidentId, ruled at MoveInRules.Resident.cs:13 and also
  an input of the sent container, MoveIn.cshtml:18-22, case 8) and PayerVerification.cshtml:34 (Reference, ruled at
  MoveInRules.Payer.cs:62 under `WhenOnServer` at :61, and set by its action at MoveInDeskController.Payer.cs:57).
- MVC 5: the twin serves 11 requests (the views its csproj links); one is under a check,
  App/Care/Admission/RoomNumbering.cshtml:20, and it sends inputs alone (parse).
- Case 6: none of the app's 104 typed GET, HEAD and OPTIONS requests is under a check (parse), so no page shows
  what RPW:304 reads.

The search. A request is written with six verbs of `Pipeline<TModel>` (PublicAPI.Unshipped.txt:460-480), their
parts' `Path`, `Query`, `Header` and `When`, the bodies `Json`, `UrlEncoded`, `Multipart`, `Text` and `File`, and
the condensed `Set`, `Form`, `Inputs` and `Input` (Alis.Reactive/TriggersAndPipelines/CondensedRequests.cs:19-240);
`git grep` of the five parts types over every PublicAPI.Unshipped.txt matched the core's two files alone, 91 lines
each. Read: the 430 files `git grep -l` lists for a verb call under App, its Net48 twin and both Consumer apps, each
call matched to its closing parenthesis by a script: 620 calls, 298 with a request type, 75 under a check (parse).
A check is read from the 62 lines `git grep` printed for `Add<TValidator>()`: a type with no registered validator
writes no check (RequestChecks.cs:42-43; Alis.Reactive/Validation/ValidatorCatalog.cs:37-41 throws for a named one).
Not read: tests/Alis.Reactive.Poc.App, the module tests, spec/api, every .ts file. The parse reads `Validate` and
`NoValidation` in the request's own statement alone (DraftCommands.cshtml:34-45 holds its request in a variable).

The app's actions, by `git grep -c` over the same roots: `[FromForm]` 111 lines in 88 files, `[FromBody]` 48 in 40. Of
the model parameters of its POST, PUT and DELETE actions 111 are `[FromForm]`, 48 `[FromBody]`, 3 `[FromQuery]` and 7
plain: six in ".Net48.cs" controllers, one on net10, App/Refusals/PathsAndWire/OccupancyController.cs:18 (parse).

## 5. The alternatives, each run under TLC

A copy of spec/Validation/Validation.tla gained where the member is sent (the body, the path, the query) and what the
endpoint's model binds: "everyPlace" (MVC 5; net10's plain model), "bodyAlone" (net10's `[FromForm]` or `[FromBody]`) and
"copied" (bodyAlone, the action setting the member where its body bound none). Each cell is one run of the invariant
Agreement, 48 runs (40 shown): "=" is "Model checking completed. No error has been found.", "≠" is "Invariant
Agreement is violated". Of a pair, the first is a body that binds members, the second one that binds none.

| | The check reads a member sent outside the body | MVC 5 | net10 plain | net10 bodyAlone | net10 copied | ≠ of 8 |
|---|---|---|---|---|---|---|
| A | never (today) | ≠ ≠ | ≠ ≠ | = = | ≠ ≠ | 6 |
| B | on MVC 5 alone | = = | ≠ ≠ | = = | ≠ ≠ | 4 |
| C | on MVC 5, and on net10 where the body binds no member | = = | ≠ = | = ≠ | ≠ = | 3 |
| D | everywhere | = = | = = | ≠ ≠ | = = | 2 |
| E | as C, and a body that binds members carries each query member | = = | = = | = ≠ | = = | 1 |

No read agrees with every endpoint: the cell E leaves is a model bound to a form the request never sends.

| | A developer writes | It costs | It reverses |
|---|---|---|---|
| A | on MVC 5 and for a plain model, `.ServerOnly()` on each rule over such a member, or the member in the body | nothing | nothing |
| B | on net10 the same as A | the read on net48 | nothing; it rests on facts:1948 and X16.46's green tests |
| C | nothing; the endpoint of a request whose body binds no member binds a plain model, or sets the member | section 7 | X16.46's three promises and the six actions of BedNotesController.cs |
| D | in a `[FromForm]` or `[FromBody]` action, one line a query member, as Payer.cs:57 | C's, and the read of the five requests of section 4 | C's; it makes the app's 159 `[FromForm]` and `[FromBody]` parameters the endpoints that differ |
| E | nothing | C's and the wire, section 8 | C's, and the body five landed requests send |

Two more, not modelled. F, a door that names the endpoint's binding: one call a request on net10, a new public door
and a plan field, and a wrong one is silent (DESIGN:1969-1970); spec/CLAUDE.md:17 asks of a new form "a concrete case
and a shown difference in meaning". G, refuse where the view renders a browser rule over such a member: the rule is
marked `.ServerOnly()` for every request of its validator, and the browser's check of the member is lost, against
spec/CLAUDE.md:22, "no module and no feature on either line is lost".

## 6. The recommended read, C, as its spec lines and its TLC runs

DESIGN:3946-3949 becomes: "A member the request sends outside its body is read where MVC's binder reads it. On MVC 5
one binder reads the form, a JSON body, the route and the query (`probe:mvc5-value-provider-order`), so the check reads
the body, then the path, then the query. On net10 a body that binds members is read alone, as an endpoint that binds
its model `[FromForm]` or `[FromBody]` binds it. A text or a file sent as the whole body binds no member, as no body on a
POST, PUT or DELETE does, so the check reads the members the path and the query send, as form data, and every other
at its start, as a GET's check does and as an endpoint that binds a plain model binds them; an endpoint that binds
`[FromForm]` there binds nothing the request sends (X16.46)." What stays of X16.46: a member the request sends nowhere
reads its start.

Why C over B: under B a net10 request whose body binds no member is checked on nothing it sends, so its verdict is
fixed where the view renders, and a DELETE and a GET to one address read one path member two ways (RPW:304). Why C
over D: row D differs on bodyAlone beside a body that binds members, where today's read agrees.

The copy, scratch/design/Validation.tla (256 lines against 201, by wc): variables nPlace, nPlaceText, binding; ServerN
(:94 at the base) and BrowserN (:120-124) gain the outside branches; the invariant SentOutsideAgrees replaces
UnboundBodyChecksTheStarts (:184), whose mutant UnboundBodyReadsBeside (:8, :123) and witness (:200) leave, since
reading "a member sent beside it" becomes the design. Each line as TLC printed it:

| Config | Its mutant | TLC printed |
|---|---|---|
| Validation.cfg | none | "Model checking completed. No error has been found.", "5308524 distinct states found" |
| violates.SentOutsideAgrees | OutsideReadAtItsStart: today's read of cases 4 and 5 | "Invariant SentOutsideAgrees is violated"; with Agreement alone, "Invariant Agreement is violated", at net48, none, path, everyPlace |
| violates.SentOutsideAgrees.QueryBesideUnreadOnNet48 | today's read of case 3 on MVC 5 | the same two lines, at net48, json, query, everyPlace |
| violates.SentOutsideAgrees.QueryBesideReadOnNet10 | row D | the same two, at net10, json, query, bodyAlone |
| violates.SentOutsideAgrees.OutsideReadOnNet48Alone | row B | the same two, at net10, none, path, everyPlace |
| violates.SentOutsideAgrees.OutsideEmptyStaysText | an empty query text read as text | the same two, at net48, none, query, everyPlace |
| violates.SentOutsideAgrees.ConfinedEndpointLeavesWhatWasSent | an endpoint C does not serve | the same two, at net10, none, path, bodyAlone |
| violates.SentOutsideAgrees.ModelTakesTheQueryBesideItsBody | the endpoints C does not serve | the same two, at net10, json, query, copied |
| five witnesses, violates.Reach... | none | each "Invariant Reach... is violated": its state is reached |
| holds.QueryCarriedInTheBody | row E | "No error has been found.", "5750892 distinct states found" |
| the base's 14 other violates configs | their own | each its own invariant violated |

The last two mutants are endpoints, not reads: Init leaves them out, and each run shows the verdicts differ with them
in. The clean run rests on section 3's last column where no probe stands: row B's counterexample and C's gain on
net10 both rest on a plain net10 model binding the route with no body. spec/InputsAndIds/ContainerRead.tla was not
run: no variable of it holds a path or a query (:24) and its Bind binds the container's body alone (:56-58), so it
cannot express the new case, and no claim that it stands is made by a run.

## 7. What a yes to C adds and costs

Probes, written before any code, each a stop if it prints otherwise: on net10 a plain model binds a route value and
a query value with no body, with a text body and with a file body; a `[FromForm]` model binds neither; an empty query
text binds a text member null; on MVC 5 the first provider that holds a key wins, and a text or a file body leaves
the form provider empty.

Promises, the plant of each unrun:
- X16.46's three, rewritten, on net10 and net48. Page: a bed notes page whose model starts Ward null, under
  RuleFor(n => n.Ward).MaximumLength(5).WithMessage("A ward's name holds 5 letters at most."), whose buttons post
  the note Bed by the window as the whole text body, Ward set in the path to North or to Northeast; the endpoint
  binds a plain model and answers "{ward}: {note}". Gesture: click Send the note to Northeast. Outcome: the notices
  status reads "Notices: error: A ward's name holds 5 letters at most." and no request reaches the notes address.
  Opposite: Send the note to North is sent, and noted reads "North: Bed by the window". Defect: the check reads Ward
  at its start, null, which the rule passes on both sides (Alis.Reactive/Validation/Checks.cs:62, :71), so Northeast
  is sent; or it reads the address for the ward, so North is refused. The same for a file body and a bodiless DELETE.
- New, both hosts: a DELETE with no body, Shift in the query as Night or Evening, under MaximumLength(5): Evening
  shows its message and sends nothing; Night is sent and noted reads "North, Night: notes cleared". Defect: the
  query's member read at its start; or the field read with its key, "Shift=Night", so Night is refused.
- New, net48: a multipart form, Shift in the query beside it: Evening is refused in the browser, Night is saved.
  Defect: on MVC 5 the query beside a body left unread.
- New, net10: that form to an endpoint that binds `[FromForm]`: with Evening in the query the save is sent and saved
  reads no shift; on the page whose model starts Shift as Evening it is refused though Night is sent. Defect: on
  net10 the query read beside the body. A yes to section 8 reverses this one.
- New, both hosts: a GET under a check, its member in the path, refused and sent: case 6 gets its line and its test.

Carriers: DESIGN's D16.5 bullet, X16.46 and the checker's line (DESIGN:4168-4169); Validation.tla, Validation.cfg, the
14 other violates configs (one line each), two configs that leave and twelve that come; spec/Validation/features.json,
spec/Reference/app.json and page-doors.json (X16.46 gains net48); RPW:234 (the query's entries where the body binds none,
and beside a body that binds members on net48) and forms.ts:204-207 (none, text and file read the entries over the
start, as form data); the bed notes page, its six actions and a ".Net48.cs" controller; X16.46's two fixtures. No
grammar node and no contract field changes: `BodyFormat` and `FormCheck` stay
(Alis.Reactive.Assets/runtime/Schema/plan.ts:531-535). Not counted: the module tests of a bodiless request's check.

## 8. The second change, its own yes: a body that binds members carries each query member

DESIGN:1785-1788 carries the path member "so the body carries the same value the path does"; the same for a query
member makes every endpoint bind it alike (row E). It changes what is sent, which this decision does not ask. Its
cost: the bodies of the five requests of section 4 change, and their tests were not read; a member the app's JSON
options leave out cannot be written into a JSON body (spec/CLAUDE.md:1887, "it can not be in plan"); a nested or a
list member meets DESIGN:1752-1756's rule on positions; and the net10 promise of section 7 reverses.

## 9. Limits

- Case 8 is modelled with the input winning (request.ts:188, :231); where that input is left out, request.ts:188-189
  deletes the path's copy and model.ts:188 still reads it: today's D9.9. "copied" sets a member its body left unbound.
- Case 7 stays: C# never holds the route template. Of the 58 URL constants of the 75 checked requests one holds a
  query, IntakesInGerman's "?culture=de-DE" (App/Care/Admission/IntakeAssessmentController.cs:36), the app's own
  culture key (parse). The cure is the door: the member written as `Path` or `Query`. A net10 model bound
  `[FromQuery]` leaves a GET's path member: no probe, no page.

## 10. The one line for the owner

What already holds: a GET's check reads the members its path and query send, a body that binds members carries each
path member (D9.9), and since 6da8232ec a POST, PUT or DELETE with no body, a text or a file is checked with every
member at its start, proven on net10 alone against endpoints that bind `[FromForm]`, where MVC 5's one binder reads
the route and the query too (facts:1948). The one thing a yes changes: a member sent outside the body is checked
where MVC's binder reads it, on MVC 5 always and on net10 where the body binds no member, so such a request is
checked on its path and query members on both hosts. Its cost: X16.46's three promises and the six bed notes actions
reverse, four promises and seven probes are written, and Validation.tla changes as section 6 ran it. Its risk: the
net10 cells it gains rest on a plain model binding a route or a query value with no body, which no probe pins yet,
and a net10 endpoint that binds `[FromForm]` to a request with no form is then checked on what it never binds.
Recommendation: yes to C; B if nothing landed may reverse; E's carry as its own decision after C.
