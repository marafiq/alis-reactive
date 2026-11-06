# Session: figure out the core design of Alis.Reactive, capture it formally, and map every module onto it

Repo: /Users/muhammadadnanrafiq/Documents/Code/alis-reactive-framework-1-0/Alis.Reactive. Read root CLAUDE.md and the
memory index first, then this whole brief before acting.

## 1. Mission
**The problem.**
- The thesis is proven beyond proofs of concept. Stability is the problem.
- Each parity gap and each MVC-partial case cost a change to the core's structure.
- The schema (the plan grammar) is the friction: it was patched case by case instead of designed.
- Tests followed the code instead of the meaning.

**The owner's decision (2026-11-05).** Verify formally first. A clean-slate rewrite with a new, stable test suite is welcome and expected to be quicker. What cannot be skipped is the core design.

**This session's job:**
- Figure out the core design completely: the schema, the object model, reads, paths, shapes, absence, lanes, ownership and requests.
- Capture it formally, checked by TLC.
- Map every module onto it. Each module's API can be improved, but each one must be expressed in the core design without inventing anything.
- Persist all of it on a branch, structured and checked by a script, so the rewrite is mechanical and closes within days.

**Done is one command's output.** `python3 spec/tools/check.py` prints all of these:
- every core design question answered;
- every TLC spec green;
- every public door mapped to a feature;
- every feature mapped to the core;
- every defect prevented on the highest rung that can hold it, and no test for one a type or analyzer prevents;
- every module's sentences consistent;
- every conformance cell promised for both reference components;
- every regression SHA tied to a spec line;
- every module and every feature on either line carried into the rewrite: features lost 0 (the owner, 2026-11-05: "No module and feature must be lost in the process");
- zero hedge words.

Nothing else counts as done.

## 2. The thesis
**What it is.** Alis.Reactive lets a team on ASP.NET MVC write dynamic UI entirely in typed, fluent C# in its views, with no hand-written JavaScript. Both hosts are first-class:
- MVC 5 on .NET Framework 4.8;
- ASP.NET Core MVC on .NET 10 and later.

The owner tested the redesign on net48 and it works flawlessly there. The compiler catches mistakes before the browser does, and defaults mean less typing, for developers and agents alike.

**What it targets.** Everything the browser runs (a Syncfusion control, a native input, a plugin, Toast, Drawer) is a JavaScript object with properties, methods and events. The model is recursive:
- A property holds a scalar, an object, or a list of objects.
- Each object a property holds has its own properties, methods and events, and so on down.
- A method returns nothing, a value, an object, or a promise of one.
- An event's payload, a returned object and each list item are read the same way.

**Sources.** Any member that returns a value is a source wherever the DSL takes one: reaction values, conditions, gather payloads, route params, query, headers, dispatch payloads and plugin arguments.

**Lanes.** Work runs in two kinds of lane:
- **Sync**, by default.
- **Async**, with five lanes: HTTP, parallel HTTP, remote triggers, user decisions (Confirm) and partial injection. A method that returns a promise (`MethodAsync`) waits like an async lane.

**Inversion.** A developer brings any component by declaring its members and events by their JavaScript names, and every feature works for it unchanged, with zero runtime code. The framework's own components are built the same way.

**What makes it hard.** These are big deals, each a first-class part of the design:
- **In .cshtml:** array lambdas, conditions, and dotted members at any depth.
- **In requests:** chained and parallel requests; headers; hidden inputs; static values; query string; route params.
- **Composition:** MVC partials: a prefix, the declaring class, an editor template, a list item, a dictionary entry, a slot loaded later.

## 3. Principles: closed, no exceptions an agent decides
0. **Make bugs impossible with types. Those need no test.**
   - Every possible defect is prevented on the highest rung of this ladder that can hold it:
     1. **The type.** The compiler refuses the wrong program: a generic constraint, a typed lambda, a sealed hierarchy, a `TypedSource<T>` where `T` must match, an internal constructor. No test is written for it; the type is the proof.
     2. **An analyzer.** The build refuses it (the ALIS rules). The analyzer's own tests prove the rule once, not per feature.
     3. **A refusal where the view renders,** with a message that names the fix. One test per refusal message.
     4. **Behavior in the browser.** A Playwright promise.
   - A defect a type can prevent is never left to a lower rung.
   - Today's examples:
     - G8: `TSource : TValue` makes a mismatched source a compile error.
     - B10: `Form(params Expression<Func<TModel, object?>>[])` erases the type and leaves the mismatch to a render-time check.
   - The fluent API (step 7) and the schema (Q19) are designed to push every defect up the ladder.
1. **One model.** C# states every meaning, including what happens when the page does not match its declaration: an absent member, a missing value, a promise where a value was declared. The plan carries it. The runtime executes it and invents nothing.
2. **Inversion.**
   - The core names no component library.
   - Its one built-in vendor is the DOM element itself: `ComponentVendor.Native` (`Alis.Reactive/Vendors/ComponentVendor.cs:59-63`), the vendor of every plain element.
   - A component, shipped or brought, is:
     - one declaration (members and events by their JavaScript names, plus `ComponentVendor` data);
     - a render helper;
     - its own browser code, when it has any (for example the custom element at `Alis.Reactive.Assets/components/native/loader.ts:86`).
   - It is built on the core's public surface.
   - Its browser code reaches the runtime only through `alis:mount`, `alis:unmount` and `window.__alisPlugins`.
3. **Encapsulation is the heart of the design.**
   - Each concept lives in one place that owns its data and its meaning, and exposes only what its callers need: `internal` by default, sealed, no public setters.
   - Grammar and plan-model classes are sealed, with `internal` constructors and `internal set`.
   - Developers author through builders.
   - The public surface is the PublicAPI text.
   - A slice uses only the rows before it in its package's `SliceTree.txt`. The first line is the foundation, and ALIS010 refuses a use of a later row.
   - **Separation of concerns, by what changes together.** The concerns are:
     - authoring: the fluent builders;
     - meaning: the plan model that writes the grammar;
     - the schema: the grammar;
     - execution: the runtime;
     - rendering: Razor.
   - **Not dogmatic.**
     - No interface without a second implementation.
     - No layer that only forwards.
     - No pattern for its own sake.
     - A small class that owns one thing beats a hierarchy of abstractions.
     - Component slices are self-contained, and duplication between slices beats a shared base class.
4. **No reflection.**
   - The shipped packages discover and invoke nothing at run time:
     - no `GetProperty`, `GetProperties`, `GetField` or `GetMethods` by name or by scan;
     - no `GetValue` or `SetValue`;
     - no `Activator.CreateInstance`, `GetConstructors` or `ConstructorInfo.Invoke`;
     - no custom-attribute reads;
     - no assembly scanning.
   - Type knowledge comes from three sources:
     - the typed lambda's expression tree, the DSL's own door;
     - generics and their constraints (`where T : new()`);
     - the first-party System.Text.Json contract (`JsonSerializerOptions.GetTypeInfo`), which names and encodes members as the net10 endpoint does.
   - On net48 the endpoint reads a JSON body through `JavaScriptSerializer`, matching CLR names and ignoring case (aspnetwebstack `JsonValueProviderFactory.cs:59`, `:77`). Q2 settles one member naming per host.
   - Each exception is the owner's decision, listed by file:line with its reason in `spec/CLAUDE.md`.
   - The rewrite adds an analyzer that fails the build on any other use.
   - Today's sites are in evidence B11, and Q18 settles each one.
5. **Typed.** No `object`, no `System.Type`, no string that names a member (ALIS009). Strings appear only for text, URLs, developer-chosen ids, header names, CSS classes, and JavaScript names at the vendor and plugin boundary.
6. **The schema is designed once.** This session designs the schema the rewrite needs, and the owner approves it as one decision. After that, a unit never changes the grammar: a behavior the nodes cannot express stops the unit and goes to the owner with its use case.
7. **One of each concept:** one read, one path type, one conversion, one encoding per value per host, one item-type finder, and one name per concept in the fluent API.
8. **No fallbacks and no guessing, in C# or in the runtime. They breed bugs.**
   - **In C#:**
     - A value the plan cannot carry is refused where the view renders.
     - Missing data never gets a default the developer did not state.
   - **In the runtime, never guess:**
     - no reading intent from the DOM: visibility, `closest`, ARIA, or id conventions;
     - no `instanceof` sniffing of what a listener received;
     - no `??` or `||` default for plan data;
     - no conversion that passes the unconverted value on;
     - no catch-and-continue;
     - no retrying another way.
   - What the runtime needs to know, the plan carries.
   - The runtime fails loud only at real boundaries: DOM lookup, browser APIs, the network, component or plugin lookup, and JSON the framework did not produce.
   - The rewrite enforces this with ESLint `no-restricted-syntax` AST selectors over `runtime/` (the rule already exists at `eslint.config.js:37`).
   - Today's sites are in evidence B5 and B12.
9. **An oracle per meaning.** Correct means agreeing with the oracle on the same input. A parity gap is a checker counterexample, never an argument.
10. **Proof.**
   - TLC's green result is the proof of a design.
   - A Playwright run in a real browser is the proof of behavior.
   - The `verify-net48` CI run is the proof on net48.
   - Vitest, PlanTests and goldens are feedback. They never assert copied output.

**Overrides by the owner's decision**, written into CLAUDE.md in step 12:
- For `spec/`, the line "Do not build other gates, coverage maps, checklists or proof artifacts" no longer applies.
- The rule against an upfront grid no longer applies to the matrices.
- The tests.md line "no component library appears on a framework test's page" gives way to the two reference components.

## 4. Scope
**In: these packages.**
- `Alis.Reactive`
- `Alis.Reactive.Fusion`
- `Alis.Reactive.Native`
- `Alis.Reactive.Assets` (runtime and components)
- `Alis.Reactive.Analyzers`
- main's `Alis.Reactive.FluentValidator`, whose meaning the redesign carries in `Alis.Reactive/Validation`

**Out, by the owner.** `Alis.Reactive.NativeTagHelpers` and `Alis.Reactive.DesignSystem`, on both lines, including main's layout helpers (`CardCss`, `GridCss` and the rest).

**Lines.**
- **The redesign at the State SHA is the base.** It is the evidence of the design that works: a component is a declaration, a vendor is data, one runtime driver.
- **main (623dd4a27) is the evidence of intent for features.** Every door main has gets one verdict: kept, renamed to a named door, or refused with a reason. A refused door names the doors that carry its capability, so no feature is lost.

**Both hosts in every meaning.**
- A meaning with a server oracle is stated for MVC 5 on net48 and for ASP.NET Core on net10.
- Where the two hosts differ, the spec names the difference and the one meaning the plan carries. Example: on net48 IIS refuses a date in a path segment, so the date goes in the query (#237).

## 5. The core design: the questions this session answers
This is the spine. Answer each question in `spec/Core/DESIGN.md`, one section per question. A section has:
- the decision, as closed statements;
- the schema nodes it uses;
- the TLA+ invariant or probe fact that checks it;
- the C# doors it implies;
- the evidence (section 6) it resolves;
- the defects it prevents, each with its rung (principle 0).

`check.py` counts the answered questions: 19 of 19.

1. **Q1. Paths.** Decide one typed path step, `member(name) | index(n) | each`, used for every path the plan carries:
   - get, call and set;
   - input bindings;
   - form fields;
   - request bodies;
   - dictionary keys.

   MVC name text appears only where the server speaks it (error keys, form-data names). Resolves B1, B2 and P1.
2. **Q2. One read.** Every value read is `get` or `call` over typed paths, with required or optional members (G3). No plugin parses a member name (B3). The four names of a member (binding name, body path, JSON name, element id) come from one derivation (P1).
3. **Q3. Shapes and conversion.**
   - One conversion node, `as` (B4).
   - What a blank input binds as is a literal the C# writes, never a runtime default (B5, `runtime/grammar/shape.ts:33-43`).
4. **Q4. Absence and promises.** Each of these is a TLA+ invariant:
   - a required member that is absent stops its pipeline;
   - data past a declared member reads as nothing;
   - a promise where a value was declared stops its pipeline (G3, G4);
   - a missing value set as text shows nothing;
   - a missing class name changes nothing (G6);
   - a route value that reads nothing stops its request, and its `Finally` still runs.
     - Today this is stated in `Finally`'s XML doc (`PlanAuthoring/Requests/HttpRequestBuilder.cs:69-73`) and pinned by `a_route_value_that_is_missing_sends_nothing_and_stops_the_steps_after_it`.
     - The plan does not carry it; `runtime/plugins/http.ts:83` throws.
5. **Q5. The object model, recursive.**
   - Decide exactly what is a live object (`JsObject`: properties, methods, events, `Object<T>`, `Objects<T>`, `MethodAsync`) and what is data (`Data<T>`: event payloads, response bodies, pushed messages). Resolves B7.
   - Dotted members at any depth.
   - Lists of objects.
   - A returned object whose methods are called.
   - A payload that carries a method.
6. **Q6. Arrays.**
   - Array lambdas in .cshtml (`Where`, `Any`, `All`, `Count`, `Find`, `Select`, …) over items with dotted members.
   - One name and one plan form per operation. Resolves B8: `Each` against `Select`, and `Count` against `Count()`.
   - The .NET members of an item's type (Trim, Length, Year) keep the meanings root CLAUDE.md states.
7. **Q7. Every value position takes every source.**
   - The positions:
     - reaction values;
     - conditions (every operator, B9);
     - gather (typed per field, B10);
     - route;
     - query;
     - headers;
     - dispatch payloads;
     - plugin arguments.
   - The sources: a component member, a method result, a URL param, an event payload, a response body, a literal, an object, an array.
   - The matrix is closed. A hole is closed, or refused with a reason the owner approves.
8. **Q8. Conditions: three kinds.**
   - Pipeline conditions (`p.When`, `ElseIf`, `Else`, guards, groups).
   - Validator conditions (`When`, `Unless`, `BrowserCondition`).
   - Template conditionals (`FusionConditionalBuilder`).

   Decide between one condition core (the same nodes, operators and sources, behind three authoring doors) and a stated list of exactly where they differ, and why.
9. **Q9. The request.** One typed request, carried as a typed plugin argument. Resolves B6.
   - A typed plugin argument is a contract, not a node: G2 holds, and the grammar gains no statement or expression kind.
   - Q19 decides where the argument contracts live and how `PlanContractGenerator` emits them. Today it walks one root, `PlanDocument`.
   - The parts:
     - **The method.** Closed at GET, POST, PUT and DELETE, unless the owner adds PATCH.
     - **The URL.** Route placeholders are filled by members of the same name. Decide a fragment, and a date in a placeholder (#237).
     - **The query.** A GET's members, by binding name, one key per list item.
     - **The body.** JSON or form data (`AsJson`, `AsFormData`). Decide the body of a request that sets nothing, a DELETE's body, and a body format on a GET.
     - **Headers,** static or from a source (`Header`). Decide duplicate names, `Content-Type`, and a header that reads nothing.
     - **Static values** (`Set(member, value)`).
     - **Hidden inputs.** A hidden input is an input like any other; `NativeHiddenField` is one example.
     - **What it gathers.** The form (`Form()`, and whether it requires `Validate`), an element (`Form(id)`), named fields (`Form(fields)`), or a typed request model (`Gather<TRequest>`, `Set`). Decide which wins when an input and a `Set` member send the same name.
     - **Files.** Decide whether a file is its own shape.
     - **Member names per host** (principle 4). The request body is written with CLR names (`Members/ExpressionPathHelper.cs:105`), while JSON reads use `JsonMemberName` (`:101`).
     - **The net48 and net10 differences** (#237).
   - `p.Url<TQuery>` reads the page's own URL. It is a source (Q7), not a part of the request.
10. **Q10. The request's flow.** A TLA+ spec of ordering, settling, the error paths and an always-run `Finally`, covering:
    - `Response` with `OnSuccess` and `OnError(status)`, each with its typed `Data<T>`;
    - `Chained`, which reads the previous response;
    - `Parallel` with `OnAllSettled`;
    - `WhileLoading`;
    - `Finally`;
    - `Into` a slot;
    - `Validate` before send.
11. **Q11. Lanes and time.**
    - Sync is the default.
    - The five async lanes and `MethodAsync`.
    - The order steps run in.
    - What happens to a lane in flight when its owner is disposed.
12. **Q12. Ownership.**
    - The owners: the page, a plan, a slot.
    - What each holds: listeners, hub holds, validation rules, live-validation wiring, nested slots, and server-rendered content inside a slot's container.
    - Dispose releases exactly what the owner holds, including when its host already destroyed the component (`stillHolds`, acd5f47fe).

    Resolves every row of P3.
13. **Q13. Where state lives.** For each concept, decide whether it belongs to the page, the form, the owner or the request. Resolves P2.
14. **Q14. MVC partials and ids.** Covers:
    - the prefix;
    - the declaring class;
    - `EditorFor` templates;
    - list-item partials;
    - dictionary entries;
    - the same partial rendered twice;
    - net48's shared `TemplateInfo`;
    - a plan written in a partial's `@section`;
    - an element id rendered once;
    - `PlanId` composition.

    Also decide the closed list of render-time refusals. Resolves P1 and P4.
15. **Q15. Templates.**
    - What the Syncfusion template engine evaluates.
    - How a template reads values and conditions.
    - What the plan carries.
    - What the native reference does for the same cells.
16. **Q16. Validation.** Covers:
    - the sent-body meaning (65fdf118d);
    - the owner's decisions B (defaults carried as the endpoint's own encoding) and C (a request that sends no form element is checked only on what it sends);
    - `FormField` designed once (P5);
    - placement (inline or summary) carried in the plan, not inferred from DOM visibility (B5);
    - validator conditions as decided in Q8.

    The browser's verdict on the body it sends equals the server's verdict on the model it binds, on both hosts.
17. **Q17. Vendors and inversion.**
    - A vendor is data (G5).
    - Every component path goes through the core.
    - It is proven on paper here by the two reference components' declarations against the schema (step 8), and later in the browser.
18. **Q18. Type knowledge without reflection.**
    - How the framework knows a model's members, their JSON names, shapes, item types and starting values (defaults): from expression trees, generics and the System.Text.Json contract.
    - The answer settles one item-type finder (today three: `Inputs/FieldReference.cs:50`, `Members/ClrShape.cs:82`, `PlanAuthoring/Requests/RequestMembers.cs:66`) and every site in B11.
    - Default values follow decision B, the endpoint's own encoding: a `new TModel()` serialized by System.Text.Json, with no hand reflection.
19. **Q19. The schema.** The plan grammar as a recursive set, with:
    - every node;
    - what it carries;
    - which nodes it holds.

    It is derived from Q1 to Q18, and its node set equals the kinds the C# writes and the runtime executes. The diff from the grammar at the State SHA goes to the owner as one decision.
20. **Q20. Tracing** (the owner's correction, 2026-11-06: "Let's bake OpenTelemetry in the design. It's in scope now").
    - How the framework records a request from start to end, in the browser and on the server, by OpenTelemetry.
    - It is optional and never always on, and it helps while developing as it does in production.

## 6. Evidence: verified at the State SHA on 2026-11-05
Every file:line below was read at HEAD. Use these as calibration: G is what good looks like, B is what the design ends.

How paths are written in this brief:
- `runtime/` means `Alis.Reactive.Assets/runtime/`, and `components/` means `Alis.Reactive.Assets/components/`.
- A C# path with no project folder is under `Alis.Reactive/`.

**Good**
- **G1.** `runtime/grammar/evaluate.ts:25-37`: 10 expressions and 6 statements, each in one switch ending in `assertNever`. A new C# node fails typecheck.
- **G2.** HTTP, validation, slots and dispatch add no node.
  - HTTP is `let res = await $http.send({...})`, then `if res.ok … else` (`PlanModel/Requests/RequestStep.cs:101`, `:219-220`).
  - The same holds for `$form.validate`, `$slot.load` and `$document.raise` (`PlanModel/Reactions/Steps.cs`).
- **G3.** A missing member is decided in C# by the kind of holder:
  - on a component or plugin it is required (`PlanModel/Values/Target.cs:57`);
  - on event, response or item data it is optional (`:80`).

  The plan carries it as `members`, and `runtime/grammar/paths.ts:24` only follows it.
- **G4.** A promise is declared (`MethodAsync`) and carried as `await` on `do`/`let` (`Grammar/Statements.cs:33`). A promise where a value was declared throws (`evaluate.ts:65`).
- **G5.** A vendor is data. `runtime/objects/vendor-driver.ts:28-33` switches only on `listen.kind` (`method` or `assignment`).
- **G6.** Missing values have meanings the C# writes:
  - a missing text shows `coalesce(v, "")` (`PlanAuthoring/Elements/HtmlElement.cs:104`);
  - a missing class name changes nothing, written as `if c != null` (`PlanModel/Reactions/Steps.cs:139-140`).
- **G7.** One recursive vocabulary: `Object<T>()` on a property and on a method, `Objects<T>()` for lists, `MethodAsync<int>()` (`tests/Alis.Reactive.Framework.App/Components/Slate/Slate.cs:20-28`).
- **G8.** `GatherBuilder.Set<TValue, TSource>(member, TypedSource<TSource>) where TSource : TValue` (`PlanAuthoring/Requests/GatherBuilder.cs:70-73`): any source fills a request member, checked by the compiler.
- **G9.** `AppEvent.Declare<TPayload>()` (`PlanAuthoring/Events/AppEvent.cs:41-43`): declared once and shared by the dispatcher and the listener.
- **G10.** Rules are written once in FluentValidation's grammar. `NotEmpty` reaches the browser, and `Must` stays on the server by what it is (`EmergencyContactsValidator.cs:9-13`).
- **The typed request against main's strings.**
  - main: `GatherBuilder.Static(string param, object value)`, `RouteParam(string paramName, …)`, `FromUrl(string paramName, string asParam)`.
  - Redesign: `Gather<TRequest>(g => g.Set(r => r.Member, source))`, with route placeholders filled by the member of the same name.

**Bad**
- **B1. "Each item" is spelled three ways:**
  - `[]` in `WireTerms/Path.cs:31` and `paths.ts:5`;
  - `[*]` in `PlanModel/Validation/ModelDefaults.cs:202`, `FormChecks.cs:54` and `runtime/plugins/model.ts:75`;
  - `*` in `Path.cs:129`.

  The kind of path step is erased into strings.
- **B2. One input binding carries one fact twice** (`Grammar/PlanDocument.cs:154`, `:165`):
  - its path, as `"name": "NextOfKin.Name"` and `"body": ["NextOfKin","Name"]`;
  - its shape, in `value` and in `shape`;
  - `list`, which `shape` already gives.

  A position is the text `"0"` in a path and the number `0` in a body (`Path.cs:28`, `Grammar/BodyStep.cs:42`).
- **B3. A second resolver beside `get`.** The member path is passed as a string literal to a plugin that parses it with a regex:
  - `PlanModel/Validation/FormRead.cs:36` (`sentValueOf`);
  - `PlanModel/Values/ValueExpression.cs:323` (`valueOf`);
  - the parse is at `runtime/plugins/model.ts:112-114`.
- **B4. Two conversions.** The `ToNumber`, `ToDate`, `ToBoolean` and `ToText` operators (`Grammar/Operator.cs:27-30`, `evaluate.ts:112-115`) duplicate `as`. Validation converts through them (`PlanModel/Validation/BrowserRule.cs:193-198`), and reads convert through `as`.
- **B5. The runtime infers from the DOM.**
  - Inline or summary is decided by DOM visibility (`runtime/plugins/form/display.ts:67-69`).
  - The message element is found by the `_error` id convention (`display.ts:74`, `InputField/InputFieldBuilder.cs:42`).
  - What a blank input binds as is chosen by shape kind in `runtime/grammar/shape.ts:33-43`.
- **B6. Plugin arguments are untyped.**
  - `$http.send`'s argument is a `G.Record` of string-named fields (`PlanModel/Requests/RequestStep.cs:140-147`).
  - `HttpRequest` is typed by hand in `runtime/plugins/http.ts:10-18`, so drift between the two passes typecheck.
  - Every plugin argument is such a record: `$form.validate`, `$slot.load` and `$document.raise` too (`PlanModel/Reactions/Steps.cs`).
- **B7. One kind of data, two shapes.**
  - A custom event or response is `Data<T>` (`PlanAuthoring/Pipelines/TriggerBuilder.cs:50`).
  - Server-pushed JSON is a `JsObject` with `ReadOnly<T>` members (`TriggerBuilder.cs:109-111`, `RealTimeModel.cs:8-10`).
- **B8. Two names per array concept.**
  - `Each` and the property `Count` on a declared list (`Components/JsMembers.cs:135`, `:140`).
  - `Select` and `Count()` on an array (`PlanAuthoring/Arrays/ReactiveArray.cs:44`, `:93`).
  - Two plan forms: the `[]` marker and `map`.
- **B9. Condition operators that take literals only.**
  - `Eq`, `Gt` and the rest take a literal or a source.
  - `Contains`, `StartsWith`, `EndsWith`, `Matches`, `MinLength`, `Between`, `In` and `NotIn` take literals only (`PlanAuthoring/Conditions/ConditionOperators.cs:16`, `:106`, `:126`; `PlanModel/Conditions/Condition.cs:331`, `:343`).
- **B10. One concept, three meanings and a lost type.**
  - `g.Form(params Expression<Func<TModel, object?>>[])` erases the type that `p.Bound<TProp>` keeps (`GatherBuilder.cs:167`, `:176-178`; `PipelineBuilder.cs:142`).
  - `Form` names three things: the validated form, an element, and named fields.
- **B11. Reflection in the shipped core**, each read at HEAD:
  - `PlanModel/Validation/ModelDefaults.cs:76-154` builds a new model through `GetValue`, `Activator.CreateInstance`, `GetConstructors` and `Invoke`.
  - `PlanModel/Values/ValueExpression.cs:76-104` writes object and enum literals through `GetValue`, `GetField` and attribute reads.
  - `Razor/MarkupRaise.cs:27` reads a payload with `GetValue`.
  - `PlanAuthoring/Requests/RequestMembers.cs:34` finds a member with `GetProperty` by name, ignoring case.
  - `Members/ClrShape.cs:40-43` lists members with `GetProperties` and reads `JsonIgnore`.
  - `Members/JsonMemberName.cs:11` reads `JsonPropertyName`.
  - `PlanAuthoring/Arrays/ElementExpressionCompiler.cs:51-55` scans methods for `PureMethodAttribute`.
  - `Validation/ReactiveFluentValidationServiceCollectionExtensions.cs:66` scans an assembly with `GetTypes()`.
  - `Components/JsObject.cs:368` scans members with `GetMembers`, non-public ones included.
  - `Validation/Checks.cs:420` reads `FlagsAttribute` with `IsDefined`.

- **B12. The runtime guesses or falls back**, each read in context:
  - `runtime/grammar/shape.ts:17-21`: a failed conversion passes the raw value on (`asText(value) ?? value`).
  - `runtime/objects/vendor-driver.ts:89-92`: what a listener receives is sniffed by `instanceof`. A `CustomEvent` with no detail yields the event; another `Event` yields `currentTarget ?? event`.
  - `runtime/plugins/form/form.ts:180`: the server message's shape is guessed: an array, else text, else `""`.
  - `components/native/loader.ts:14`, `:22` and `components/native/drawer.ts:33`: `?? ""` and `?? 0` defaults for missing data.
  - `runtime/grammar/execute.ts:42-48`: when an unawaited call's promise rejects, the error is logged and the pipeline continues.
  - `runtime/plugins/model.ts:117`: what a hole or a position past the end of a sent list reads is decided in the runtime, not stated by C#.
  - `runtime/plugins/http.ts:83`: a missing route value throws, with no meaning stated by C#.
- **B13. Strings outside principle 5's list, with no exemption** (`git grep TypedDslExemption -- Alis.Reactive.Native` has no hit):
  - `NativeActionLinkBuilder.Attr(string name, string value)`;
  - `NativeButtonBuilder.Type(string type)`;
  - `NativeTextBoxBuilder.Type(string type)`.

  These are `Alis.Reactive.Native/PublicAPI/net48/PublicAPI.Unshipped.txt:20`, `:52` and `:176`.
- **B14. Leads, not yet observed. The session settles each one.**
  - **A request member with `[JsonPropertyName]`** is sent under its CLR name (`ExpressionPathHelper.cs:105`), so a net10 `[FromBody]` endpoint would not bind it. No test sends such a member.
  - **ALIS008's message** says "only the last request survives" (`Alis.Reactive.Analyzers/HttpPipeline/MultipleHttpRequestsInPipelineAnalyzer.cs:17`). A pilot read each request as its own step (`RequestExtensions.cs:58-62`) and did not run it.
- **B15. A declaration drifts from its vendor's contract.** `FusionGrid` declares `ExcelExport`, `CsvExport` and `PdfExport` as `Method()`, but `grid.d.ts:3849` and `:3870` return a Promise. Root CLAUDE.md requires `MethodAsync` for a method that returns one.
- **B16. The vendor version floats.**
  - Only the meta-package `@syncfusion/ej2` is 33.1.47.
  - Its sub-packages follow tilde ranges: `ej2-calendars` and `ej2-popups` are 33.1.44, and `ej2-base` is 33.1.45.
  - The oracle's version is exact, so the rewrite pins every sub-package (`scripts/pin-syncfusion.mjs`).

**What partials cost: the regression list (P).** Each SHA was checked with `git show --stat`. "Both" means the commit is on main and on the redesign; "redesign" means only the redesign has it.
- `spec/history.tsv` lists every SHA below, with the spec line (a DESIGN.md section, a TLC invariant, a probe fact or a feature row) that states its meaning.
- `check.py` fails on a row without one.

**P1. The four names of a member drifted under a partial.** Ids and binding names were derived from the expression or the view's model type, ignoring the prefix and the declaring class:

| SHA | Branch | What broke |
| --- | --- | --- |
| 31aacf71e | both | a rule on a field a partial renders fell back to `IdGenerator.For(typeof(TModel), field)`, with no prefix |
| 0090a6adc | redesign | only `Html.InputField` knew `HtmlFieldPrefix` |
| aba543d9c | redesign | the same partial under two prefixes gave both inputs one id, and one card posted the other's values |
| a82264ab8 | redesign | an `EditorFor` template rendered `KinCard__NextOfKin_Name`, while the page looked for `DeskPage__NextOfKin_Name` |
| 45259f0eb | redesign | `m => m` rendered `System_String__Visitor`, and is now refused |
| a0a95e0bd | redesign | a field named `Name` while the server reports `NextOfKin.Name` |
| 57f173f58 | redesign | `Members[1]` was parsed as one member |
| 8046bc4c5 | redesign | a request was checked for the unprefixed name |
| b24c65775 | redesign | a dictionary keyed by a number cannot be expressed in JSON; only documented |

**P2. State that belongs to the page was keyed per plan.**
- the plan-id input table (053b50d84);
- item rules over rows only the page's plan rendered (34c99d608);
- message placement by the plan's own fields (01d00553a);
- one summary per plan (main: a893ca7aa, 0d32aa743, f38d0d278), replaced in 76ca89036 and 532de0002;
- a read by name that read another partial's input. The chain is 92dce45ba, then a019367c5, then fda82e5b4.

**P3. Cleanup on unload came back as runtime patches.**
- 885652213: a slot guessed from `container.id.length > 0`. It moved into C# with `SlotId` in 527138248.
- 0b47eafd7: a shared hub was stopped by one listener's abort. It became plan data, `HubPluginData`, in d07a89928.
- bc45c3b9e: a retry offer stayed after unload.
- 6478b4a4a, e02294ae4 and a168e014d: orphaned rules and stale wiring after a reload, with one decision reversed.
- d6251825e: listeners accumulated on every slot load.
- f26146b8f and e83040182: nested slots, and server-rendered content inside a slot's container.
- 893ed624a (main): checks Syncfusion's `isDestroyed`, a vendor name in the runtime. The redesign answers it with the vendor-neutral `stillHolds` in acd5f47fe.

**P4. What MVC does silently became render-time refusals.**
- c5421ac4b: setting a slot's HTML is refused.
- 58c12947c: an element id renders once per page.
- eecfb7336: a prefix taken late is refused.
- A plan in a partial's `@section` was refused, narrowed and then removed as a guess (f3c2a6e10, a0bd5f0d2, 2227c4eb5). Today it is not detected.

**P5. The grammar churned.** `FormField` had three shapes in a week:
- with an id and a value;
- by binding name only (34c99d608);
- with `items` and `value` functions (65fdf118d).

## 7. The known modules (from the redesign at the State SHA)
Every module gets the same treatment:
- feature rows;
- sentences;
- promises;
- the core design questions it uses;
- the improvements it proposes.

Core, `Alis.Reactive`:
1. **Triggers and reactions:** `Html.On`, `.Reactive`, `TriggerBuilder`, `DomReady`, element events, `AppEvent` dispatch.
2. **Elements:** `HtmlElement` (text, HTML, classes, visibility).
3. **ValueExpression**, the one read. Sources: a component member, a method result, a URL param (`p.Url<TQuery>`), an event payload, a response body, a literal, an object, an array.
4. **Array lambdas:** `ReactiveArray`, `ElementExpressionCompiler`, `PureMethod`.
5. **Pipeline conditions:** `p.When`, `ElseIf`, `Else`, guards and groups.
6. **HTTP pipeline:**
   - Get, Post, Put and Delete (`RequestStart`);
   - `Gather` (`Form()`, `Form(id)`, `Form(fields)`, `Gather<TRequest>`, `Set`, `Header`);
   - `AsJson` and `AsFormData`;
   - `Response` (`OnSuccess`, `OnError`);
   - `Chained`;
   - `Parallel` (`OnAllSettled`);
   - `WhileLoading`, `Finally`, `Into` and `Validate`.
7. **The server validation grammar:** `ReactiveValidator<T>`, `Checks`, `RuleSets`, `Messages`, `ChildRules`, `SetValidator`, `Include`, `ServerOnly` (`Alis.Reactive/Validation`). It replaces main's `Alis.Reactive.FluentValidator`.
8. **Browser validation:** `BrowserRuleSet`, the form plugin, the summary, live re-validation and `ValidationErrors`.
9. **Validator conditions:** `When` and `Unless` on rules, and `BrowserCondition`.
10. **Inputs and ids:**
    - `Html.InputField`, `IdGenerator`, binding names, `HtmlFieldPrefix`, `ReactiveClassNames`, the model plugin.
    - A member has four names that must agree.
11. **Partials and slots:** `PlanId` composition, `SlotId`, `Into`, `Unload`.
12. **Realtime:** `ServerPushStream`, `SignalRHub`, remote triggers.
13. **Plugins:** `Plugin(name)`, `window.__alisPlugins`.
14. **Components and vendors:**
    - The declaration vocabulary: `Property`, `ReadOnly`, `WriteOnly`, `Method`, `Method<T>`, `MethodAsync`, `MethodAsync<T>`, `Object<T>`, `Objects<T>`, `Event<T>`, `Arg<T>`.
    - `ComponentVendor`, `runtime/objects/vendor-driver.ts`, and `alis:mount` and `alis:unmount`.
15. **Razor rendering and delivery:** `Html.RenderPlan`, markup raise, and content-hash asset URLs on net48 and net10.
16. **Analyzers:** ALIS001, ALIS003-ALIS005, ALIS007-ALIS013.

Fusion, `Alis.Reactive.Fusion`:

17. **Fusion templates:** `FusionTemplateBuilder`, `FusionTemplateExpression`, `TemplateElements`, `FusionConditionalBuilder`.
18. **Fusion components:** 51 declarations under `Alis.Reactive.Fusion/Components`.
19. **Fusion app-level objects:** FusionConfirm (the user-decision lane) and FusionToast.

Native, `Alis.Reactive.Native`:

20. **Native components:** NativeActionLink, NativeButton, NativeCheckBox, NativeCheckList, NativeDropDown, NativeHiddenField, NativeRadioGroup, NativeTextArea, NativeTextBox.
21. **Native app-level objects:** NativeDrawer (`BeforeClose`) and NativeLoader.

The session corrects this list from the inventory. `spec/CLAUDE.md` holds the corrected list and replaces this one.

## 7a. Leads from three blind pilots, 2026-11-05
Each pilot ran one part of this brief on its own. Their drafts are in `~/.claude/projects/-Users-muhammadadnanrafiq-Documents-Code-alis-reactive-framework-1-0-Alis-Reactive/handoff/2026-11-05-spec-first/`. They are leads, not evidence:
- start from them;
- re-read every file:line they cite before using it;
- the findings that changed this brief are already in it.

**`pilot-q9/DESIGN-Q9.md`:** a draft of Q9.
- It has 37 closed statements, R9.1 to R9.37, and owner items O1 to O12.
- `pilot-q9/aspnetwebstack/` holds MVC 5's `JsonValueProviderFactory.cs` and `DefaultModelBinder.cs` (aspnetwebstack main at 2106565bf, not the 5.3.0 tag).
- Its ten probes (numbered `P9.x` in the draft; they are not the partials list P) have not run.

**`pilot-ref/matrix.json` and `pilot-ref/syncfusion.json`:** 62 cells, with every candidate's count and cites. It also measures what each shipped declaration covers:
- FusionMultiSelect 38 of 62;
- FusionDropDownList 40;
- FusionGrid 37.

**`pilot-native/`:** step 3 and step 6 run for `Alis.Reactive.Native`.
- 23 features over 112 declaring types.
- main's Native surface was derived by hand. Step 2's RS0016 output replaces it: 233 lines on net48 and 224 on net10.0.

## 8. Definitions
- **A door** is one line of a PublicAPI text (redesign), or one line of main's surface written the same way (step 2).
- **A feature** is one of these, and every door belongs to exactly one:
  - **A component or an app-level object:**
    - its declaration class and its builder;
    - its payload and argument types;
    - its enums and item types;
    - its `Html` extension class;
    - its browser code.
  - **A core builder family:** one builder type, with the extension classes that extend it.
  - **The declaration vocabulary** (module 14): the base types (`JsObject`, `NativeComponent`) and the member types.
  - **One grammar node kind.**
  - **One analyzer rule.** This includes main-only ALIS002 and ALIS006.
  - **One runtime plugin.**

  A main type joins the redesign feature it maps to. A main extension class joins the type it extends or produces.
- **A sentence** is one form a developer writes in a view or a validator. It has:
  - its context: trigger, pipeline, condition, gather, response, validator, declaration or render helper;
  - its verb;
  - its arguments, each a typed lambda, a value source, or a builder callback with its locked parameter name;
  - its feature;
  - the core questions it rests on.
- **A meaning** is what a feature does when the page matches its declaration and when it does not.
  - Meanings come from the core design (section 5).
  - A feature row states a meaning in its own words only where no core answer gives it.
- **An oracle** decides what is correct for a meaning, independently of our code. It is one or more of:
  - a server: the MVC 5 binder, the ASP.NET Core binder, or FluentValidation 12.1.1;
  - a vendor's source: EJ2 33.1.47;
  - the DOM;
  - the spec itself, where the framework is the authority.
- **The conformance matrix** is the closed list of paths a component takes through the framework. The cells:
  - **Properties:**
    - read and write a scalar;
    - read and write a dotted member (`fields.text`, `pageSettings.pageSize`);
    - an object a property holds, its members read and written;
    - a list of objects a property holds, its items read through an array lambda;
    - read-only and write-only.
  - **Methods:**
    - a call that returns nothing;
    - a returned value (`Method<T>`);
    - a returned object, its members read and its methods called (`Object<T>`);
    - a returned promise (`MethodAsync`, `MethodAsync<T>`);
    - arguments that are a scalar, an object or a list.
  - **Events:**
    - a scalar payload;
    - a nested-object payload;
    - a list payload;
    - a payload that carries a method the pipeline calls.
  - **Absence:**
    - a declared member the object lacks;
    - data past a declared member;
    - a promise where a value was declared.
  - **As a source:** the component's member at every value position of Q7.
  - **HTTP:**
    - Get, Post, Put and Delete gathering from the component;
    - route, query, header, static and hidden values beside it;
    - `Into`;
    - `WhileLoading`;
    - `Response` success and error, reading the body into the component;
    - `Chained`;
    - `Parallel`;
    - `Finally`.
  - **Other lanes:**
    - Confirm before a write;
    - a server push into the component;
    - the component inside a partial loaded into a slot, and unloaded: its listener removed, unless its host destroyed it.
  - **Templates:** a template the component renders, with a template conditional.
  - **Validation:**
    - the component as a validated input: a rule, a validator condition on its member, live re-validation on its commit event, and the summary;
    - inside a list item;
    - under `HtmlFieldPrefix` in a partial, where its four names agree.
- **A reference component** is declared on the core's public surface, the way an app brings a component. There are two:
  - **The Syncfusion reference.** One installed EJ2 component, chosen by the matrix.
    - **Counting:** one cell per item the matrix names.
    - **Its contract:** its documented-public `.d.ts` members under `node_modules/@syncfusion/<package>/src/`, following `extends` and member types, without `@private` or `@hidden`.
    - **An input:** a public `value`, and a `change` event whose payload declares `value`.
    - **The choice:** the component that covers the most cells. Among equals, the input the framework already ships as an input (`IInputComponent`).
    - **A cell its contract lacks** cites a scan of the chain.
    - **When the `.d.ts` and the JavaScript disagree,** the JavaScript is the oracle, and the disagreement is recorded.
    - **Evidence from a 2026-11-05 pilot:**
      - MultiSelect and ListBox both cover 56 of 62 cells.
      - Only `FusionMultiSelect` is an `IInputComponent` (`Alis.Reactive.Fusion/Components/FusionMultiSelect/FusionMultiSelect.cs:16`, against `FusionListBox.cs:20`), so the choice is MultiSelect.
      - No input's contract has a read-only member, a write-only member, a method returning a promise, or a returned object with a method. The native reference covers those cells.
  - **"Its host destroyed it"** means the component has a public `destroy()` and `isDestroyed`.
  - **The native reference.** A plain-DOM component the framework builds itself, with its own vendor data, covering every cell.

  The two replace the 16 partial-coverage components the framework test app brings today: Banner, Bell, Button, Choice, ConfirmDialog, DayPicker, Dispenser, Entry, FileInput, FilePicker, Meter, Roster, Slate, Stamp, TagPicker, Tally.
- **A defect** is one way a feature can go wrong. Each feature lists its defects. Each defect names the rung of principle 0 that prevents it:
  - `type`, with the type or constraint, file:line;
  - `analyzer`, with the rule id;
  - `refusal`, with the message;
  - `behavior`, with its promise.
- **A promise** is one Playwright test's specification. It exists only for a `refusal` or a `behavior` defect. It has:
  - the page, as a sentence;
  - one gesture;
  - one observable outcome;
  - the opposite case;
  - the defect it fails for.

## 9. Checkers: assigned, not chosen
**TLA+, checked by TLC:**
- Q4, absence and promises;
- Q9 and Q10 together in `spec/Core/Request.tla`: which part each member goes to (route, query, body or header), the request's flow, and its refusals;
- Q11, lanes and time;
- Q12, ownership;
- Q16, validation agreement on both hosts;
- the date wire form;
- Q19, the schema as a recursive set.

**Probes.** Each server fact a spec assumes is pinned by a probe:
- **net10:** a console under `spec/probes/` prints it from the installed package: System.Text.Json, ASP.NET Core binding, or FluentValidation 12.1.1 (`~/.nuget/packages/fluentvalidation/12.1.1`).
- **net48:** the fact is pinned from the MVC 5 source (aspnetwebstack, `DefaultModelBinder` and `JsonValueProviderFactory`, by file:line) and proven by `verify-net48`.

**`check.py`** checks the rest: the inventory, the mappings, the fluent consistency, the matrix, the regression list, and the hedge words.

## 10. Stability rules for every promise
- **The server decides when async things happen.** For an async lane (HTTP, a server push, a hub, Confirm, a slot load), the test app's endpoint decides when the response, push or reconnect happens, and the test triggers it with a gesture. A promise never races a timer.
  - What not to do: `HttpPipeline/RealTime/WhenLiveConnectionDrops.losing_the_hub_connection_...` failed under parallel load in the 2026-11-05 gate ("Locator expected to contain text 'Helen Martinez (Manual)' But was: '—'") and passed alone 5/5.
- **Wait for the observable state the promise names,** never for time.
- **One world per test:** its own page, its own data and its own server state.
- **Assert literal outcomes** read from the meaning, never copied output, plan JSON or call counts.
- **Each promise names the defect it fails for,** and the opposite case is its own promise.

## 11. Surviving compaction: the protocol
The session will compact several times. The summary loses detail, so everything that matters lives on disk:
- **`spec/BRIEF.md`:** this brief, copied in step 1 from `~/.claude/projects/-Users-muhammadadnanrafiq-Documents-Code-alis-reactive-framework-1-0-Alis-Reactive/handoff/2026-11-05-spec-first/BRIEF.md`, which holds the same text. Until the branch exists, that copy is the brief. After a compaction, re-read the section you are working in.
- **`spec/STATE.json`:** written only by `check.py`, never by hand. It holds the current phase and step, each check's pass or fail with counts, and the next action.
- **`spec/NOTES.md`:** append-only, at most two lines per entry. Each entry is a decision, a fact, or an owner answer, with its evidence (a command result or a file:line). Nothing else goes in it.
- **A SessionStart hook with matcher `compact|resume`,** in `.claude/settings.local.json` at the repo root. The file exists, holds only permissions, and is ignored by git globally. Its command:
  ```bash
  cd "$CLAUDE_PROJECT_DIR/.worktrees/spec-first"; python3 spec/tools/check.py --brief; tail -20 spec/NOTES.md; echo "Re-read spec/BRIEF.md for the current step before acting."
  ```
  - The commands are joined with `;`, so a failing check never hides the notes. `check.py` exits nonzero while checks fail.
  - A SessionStart hook's plain stdout is added to the context.
  - Prove the hook once by running its command by hand while a check fails, and show its output. The project's tracked `.claude/settings.json` already has its own SessionStart hook; leave it as it is.
- **Commit and push after every step,** and after every module in steps 6 to 9. A step is never half on disk.
- **Context hygiene:**
  - read code by file:line range, never whole large files;
  - extract by script;
  - give wide reads to agents;
  - never hold the inventory in context: `check.py` holds it.

## 12. Agents: blind, small, verified
- At most two at a time.
- **Each brief** is one small task with one output file under `spec/`. It contains:
  - the section of `spec/BRIEF.md` the task needs;
  - the exact input files;
  - the output path;
  - the `check.py` check the output must pass.

  It never contains your conclusions, so the agent judges the code, not your hypothesis.
- **Good tasks:**
  - the feature rows and sentences for one module;
  - one Syncfusion component's `.d.ts` against the matrix;
  - one probe console;
  - mining one module's history for regression SHAs.
- **Accept an agent's output only when** its `check.py` check passes and you have re-read three of its file:line cites at the source. An agent's claim is a lead until then.
- No review agents.

## 13. Phases and steps
**Phase A: durable start**

**0. Restate and wait.** Restate the mission and the five phases in at most eight lines, and wait for the owner's yes.

**1. Branches and survival.**
- Create the spec branch:
  ```bash
  git worktree add .worktrees/spec-first -b feature/spec-first 65fdf118d
  ```
- Copy the brief: `cp ~/.claude/projects/-Users-muhammadadnanrafiq-Documents-Code-alis-reactive-framework-1-0-Alis-Reactive/handoff/2026-11-05-spec-first/BRIEF.md spec/BRIEF.md`.
- Write an empty `spec/NOTES.md`, `spec/CLAUDE.md` (the module list and the file map) and a first `spec/tools/check.py`. The first `check.py` prints which checks exist and writes `STATE.json`.
- Install the compact hook and prove it by running its command by hand.
- Commit, and push with `git push origin HEAD:refs/heads/feature/spec-first`.
- Read main at 623dd4a27 in a detached worktree, `.worktrees/main-read`.

**Phase B: facts, by script**

**2. Main's surface as text.**
- In `.worktrees/main-read`, give each in-scope project the redesign's wiring (`Alis.Reactive/Alis.Reactive.csproj:44-46`):
  - `Microsoft.CodeAnalysis.PublicApiAnalyzers` 5.6.0;
  - `AdditionalFiles` for `PublicAPI/$(TargetFramework)/PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`, both empty.
- Run it one target framework at a time.
  - Set `<TargetFrameworks>` to that one framework, then run `dotnet format analyzers <project> --diagnostics RS0016 --severity info`.
  - On the multi-targeted project, the fix writes only the first framework's text.
  - This was proven on 2026-11-05 on main's Native project: net48 wrote 233 lines; net10.0 wrote 0 lines multi-targeted and 224 lines alone. A hand derivation was off by one line on each.
- Copy the texts to `spec/inventory/main/`.
- Print the line count per package and target framework.
- Remove the worktree.

**3. Inventory.** `spec/tools/inventory.py` reads:
- the redesign's PublicAPI texts for net48 and net10;
- main's texts;
- the kinds in `runtime/types/plan.schema.json`;
- the runtime's `case` labels;
- the analyzer IDs;
- both lines' Playwright tests, as fixture path and test name.

It writes `spec/inventory/*.tsv`, one row per door, with these columns:
- package;
- target framework;
- declaring type, which is the feature id;
- line status: `both`, `main-only` or `redesign-only`, decided per target framework.

How doors are matched:
- They are matched with namespaces stripped, because main moved Native types from `.Native.Components` and `.Native.AppLevel` to `.Native`.
- A `#nullable enable` line is no door.
- The target framework folders are `net48` and `net10.0`.
- A door belongs to its project's package.

The schema kinds, `case` labels and analyzer IDs go to their own TSVs. The tests go to `tests.tsv`, each with the route it opens. A Native fixture sits flat in `Components/Native/`, so a fixture's folder names no feature. Step 9 maps tests to features.

**Phase C: the core design. This phase is the session's center.**

**4. Answer Q1 to Q18** in `spec/Core/DESIGN.md`, each with its evidence and its checker. Then:
- write the TLA+ specs and the probes, all TLC green:
  - Q4;
  - Q9 with Q10, in `Request.tla`;
  - Q11;
  - Q12;
  - the date wire form;
- write `spec/history.tsv`, every regression SHA with its spec line.

**5. Answer Q19, the schema**, and the validation spec.
- **The schema:**
  - `spec/Core/Schema.tla` and `spec/Core/schema.md`: every node, what it carries, and which nodes it holds.
  - `check.py` checks that the kinds in `Alis.Reactive/Grammar`, the kinds in `plan.schema.json` and the spec's node set are equal.
  - Every kind has exactly one handler in the runtime: a `case` label, or, for plugin data (`form`, `hub`, `model`), the plugin the plan names (`runtime/plans/start.ts:36-40`). `none` has no handler by design.
  - The proposed set's difference from today's is the diff.
- **The validation spec:**
  - Model the design at bff9eb6c0. TLC prints a counterexample for each of the four gaps found by hand:
    - an unrendered member with an initializer is refused;
    - a list beside the form is read though the request does not send it;
    - a condition on an object the page has no input for;
    - a list rule inside an item whose list is not sent.
  - Then model the decided design (the sent-body read, with B and C), which must pass TLC.
- **Owner gate 1:** present the core design as one decision:
  - the 20 answers in one line each;
  - the schema diff;
  - the TLC result lines.

  Wait for the owner's yes.

**Phase D: every module onto the core**

**6. Feature rows**, in `spec/<Module>/features.json`, one entry per feature. Each entry has:
- id;
- its doors (the inventory rows, by declaring type);
- its sentences;
- its schema nodes;
- the C# that writes it and the runtime that executes it today (file:line at the State SHA). A builder that only renders markup cites its render method;
- the core questions it rests on;
- its own meaning, only where no core answer gives it;
- its oracle;
- its checker: the TLC spec, or `core:Qn` when a core answer gives its meaning;
- its defects, each with its rung;
- its promises;
- its status: `both`, `main-only` or `redesign-only`;
- a verdict for every main-only door, one of:
  - `kept`, when it survives moved;
  - `renamed`, to the doors it lists (core doors are allowed);
  - `refused`, with its reason and the doors that carry its capability; a refusal with no carrier loses a feature and fails the check.

  A verdict differs per target framework only where the two frameworks' texts differ.

A door with no sentence in any view gets a proposed sentence.
`spec/<Module>/` uses the module's name from section 7 in PascalCase, for example `spec/TriggersAndReactions/`.

Each module also gets `spec/<Module>/module.md`: the core questions it uses, and the improvements it proposes, each with its evidence.

**7. The fluent API, `spec/Fluent/sentences.json`.** Every sentence, today's and proposed. `check.py` checks:
- **One concept, one name, in every context.**
- **Every value position takes every source** (the Q7 matrix). Each cell is a sentence, or refused with a reason the owner approves.
- **The same meaning has the same shape** across modules.
- **Callback parameter names** follow the locked table in `.claude/rules/csharp.md`.
- **No string names a member.**

**Owner gate 2:** present one decision. It holds:
- each changed sentence, as today's form and the proposed form, with the counts;
- every `refused` door;
- every callback parameter name the locked table lacks (for example `request` and `confirmed`).

Wait for the owner's yes.

**8. The reference components, `spec/Reference/`.**
- `matrix.json` lists the cells.
- `syncfusion.json` and `native.json` each give the declaration against the schema and, for every cell, the member that runs it.
- The Syncfusion choice prints its count: cells covered, out of the total, from the `.d.ts`.

**9. The promises.** Every feature's `refusal` and `behavior` defects get one, and so does every matrix cell, against both reference components. A defect on the `type` or `analyzer` rung gets no promise. Each promise has:
- the page;
- the gesture;
- the outcome;
- the opposite case;
- the defect;
- the existing test that already proves it, when one meets the stability rules.

**Phase E: check and plan**

**10. `check.py`, complete.** It prints:
- Q1 to Q20 answered, as 20 of 20;
- each TLC spec green, with its result line;
- the kind sets and their handlers;
- doors: mapped, unmapped 0, doubled 0;
- main-only verdicts;
- every feature mapped to the core;
- every defect assigned a rung;
- no promise for a `type` or `analyzer` defect;
- every `type` defect citing its type at file:line;
- the fluent checks;
- matrix cells promised for both references;
- history SHAs tied to a spec line;
- hedge words 0.

**11. `spec/REWRITE.md`.**
- Units go in `SliceTree.txt` order, first line first. The first line is the foundation; ALIS010 refuses a use of a later row (`Alis.Reactive.Analyzers/SliceTree/SliceTreeAnalyzer.cs:308-311`).
- Each unit lists its feature ids, its core answers, its schema nodes, its sentences, the counterexamples it closes and its promises.
- `check.py` checks that every feature id is in exactly one unit.

**12. Root CLAUDE.md on spec-first.**
- Add:
  - the thesis;
  - these principles;
  - the stability rules;
  - the order a unit runs in: spec, TLC green, code, Playwright promises, commit;
  - the module table, pointing at `spec/CLAUDE.md`.
- Replace five lines, each found word for word first:
  - "(rare: the grammar is closed)";
  - the "rare, deliberate, justified exception" line in `.claude/rules/csharp.md`;
  - "Do not build other gates, coverage maps, checklists or proof artifacts";
  - the upfront-grid clause in the root-cause rule;
  - "no component library appears on a framework test's page" in `.claude/rules/tests.md`.

## 14. Tools: installed and proven 2026-11-05
- **Java:** `/opt/homebrew/opt/openjdk/bin/java`, OpenJDK 27.0.1. `/usr/bin/java` is the macOS stub and fails.
- **TLA+:** `~/.local/share/tlaplus/tla2tools.jar`.
- **Run TLC with:**
  ```bash
  /opt/homebrew/opt/openjdk/bin/java -cp ~/.local/share/tlaplus/tla2tools.jar tlc2.TLC -deadlock <Spec>
  ```
  A smoke spec printed "TLC2 Version 2.19 of 08 August 2024" and "Model checking completed. No error has been found."
- **Also available:** Python 3.14.7, Node 24.19.0, and the installed EJ2 `.d.ts` files under `node_modules/@syncfusion/` (meta-package 33.1.47; see B16).
- Spec tools live under `spec/`. Nothing goes into `scripts/` or the build.
- **Spec data files are JSON.** Python's standard `json` reads and writes them. `import yaml` fails here with "ModuleNotFoundError: No module named 'yaml'".
- **No file carries a comment.** `check.py` greps `.py` for comment lines and `.tla` for `\*` and `(*`. WhenCodeIsWritten scans `.json`. Prose goes in `.md`.

## 15. No excuses
- **Never offer these as a reason to stop:** "out of scope", "could not install", "the tool is unavailable", "needs more time", "would require", or a list of problems. Investigate, read the source, write the spec.
- Root CLAUDE.md asks a report to name what was not exercised. In this session that list is empty, because each item is closed before the report.
- **Every claim** carries its command and result line, or its file:line.
- **A gap you find** is settled in this session, in DESIGN.md or as a REWRITE unit carrying its counterexample.
- **The only things you bring to the owner:**
  - step 0;
  - gate 1, the core design;
  - gate 2, the fluent API;
  - a decision on a principle, a closed rule or the scope.

  Each comes as a yes or no, with your recommendation and the evidence.
- **No open doors.** Never write rare, usually, may, when needed or where possible into a rule, a spec, a sentence or a feature row.
  - `check.py` greps the prose the spec authors: DESIGN.md, schema.md, every module.md, and the prose fields of the JSON files.
  - It does not grep quoted code or test names.
  - Write "nullable" or "non-nullable" where the code says "may be empty".
- **Module names** come only from this brief, and from `spec/CLAUDE.md` once it exists.
- **Caffeinate** around every long run. After every run, check `lsof -nP -iTCP -sTCP:LISTEN | grep -i dotnet` and kill what is yours. Other sessions run their own gates in other worktrees; leave those alone.

## 16. Hard constraints
- Never commit or push to main.
- Push the spec branch only with `git push origin HEAD:refs/heads/feature/spec-first`.
- `git stash` is never run, `git stash list` included. stash@{0} holds work that stays put.
- `examples/resident-intake/` is never committed. Stage by path.
- Never read secrets.

## 17. State at handoff
**Lines**
- **State SHA:** `feature/component-redesign` at 65fdf118d (PR #177): parity module 5, validation, the sent-body read.
  - Gate: `scripts/test.sh --parallel` passed every leg except one realtime test, which fails under parallel load (`WhenLiveConnectionDrops`, the push after a reconnect). That shard passed alone: "Total tests: 299 Passed: 299". CI: `gh run list --commit 65fdf118d1498a8285db84a43c4da81dfaa0ea3e` listed no run on 2026-11-05. The last runs on the branch are bff9eb6c0's: CI success, `verify-net48` success. Read 65fdf118d's runs by full SHA before citing them.
- **main:** 623dd4a27 (2026-11-05). main has no PublicAPI text.

**Rough denominators**, from `git grep` of public declarations; step 2 replaces them with exact counts:

| Package | main: public members | main: public types | Redesign: public types | Redesign: PublicAPI lines |
| --- | --- | --- | --- | --- |
| Alis.Reactive | 655 | 59 | 112 | 596 (net48), 594 (net10) |
| Fusion | 1685 | 465 | 277 | 1597 per framework |
| Native | 210 | 66 | 47 | 208 (net48), 200 (net10) |
| FluentValidator (main only) | 89 | 9 | none | none |

**Schema and runtime**
- **Grammar:** 33 kinds and 47 sealed classes in `Alis.Reactive/Grammar`, carried by `runtime/types/plan.schema.json`.
- **Runtime `case` labels per file:**
  - `evaluate.ts`: 32
  - `shape.ts`: 18
  - `execute.ts`: 6
  - `operators.ts`: 4
  - `plan-objects.ts`: 4
  - `vendor-driver.ts`: 4

**Tests and pages**
- **Playwright `[Test]` count:** 2,801 on the redesign, 1,334 on main.
- **Views:** 268 in the sandbox and 199 in the framework test app.

**Binder facts, read from source**
- **ASP.NET Core with System.Text.Json:**
  - keeps a member's initializer when the body lacks the member;
  - binds an explicit null as null;
  - makes a fresh holder for a sent object.
- **MVC 5 (`DefaultModelBinder.cs`):**
  - skips an absent property (lines 215-220);
  - binds into the existing value (line 224).

**Owner decisions, 2026-11-05**
- Verify formally first. TLA+ is the formalism.
- FluentValidation 12.1.1 is a test-only oracle.
- The upfront-grid rule is amended for the fluent matrix.
- B: defaults are carried as the endpoint's own encoding.
- C: a request that sends no form element is checked only on what it sends.
- A request that saves part of a model is a typed model with its own validator (the sandbox's `EmergencyContactsSave`).
- A clean slate, with a new and stable suite, is allowed.
- TagHelpers and DesignSystem are out of scope.
- net48 and net10+ are both first-class; the owner tested the redesign on net48.
- The framework is proven by two reference components that run every path: one Syncfusion component over its public contract, and one native component the framework builds itself.

**Raised after the fact**
- The validation unit changed `Alis.Reactive/Grammar/PlanDocument.cs`:
  - `FormField.items` became `fn(sent)` and `FormField.value` became `fn(i, sent)`;
  - `FormPluginData` gained `live`.
- The schema step decides it.

**Workspace**
- `examples/resident-intake/` is untracked and is never committed.
- stash@{0} stays put.
- Tools are installed: section 14.

## 18. End of session
- The report's first lines are `check.py`'s output, verbatim.
- Then the two owner gates with their answers.
- Then steps 1 to 12, each with its evidence line.
- Then copy the handoff and the next session's prompt to the clipboard with pbcopy, and read them back with `pbpaste | head`. The next session's prompt is the first unit in `spec/REWRITE.md`.
