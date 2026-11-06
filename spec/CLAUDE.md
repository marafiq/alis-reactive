# spec

This folder is the design of the Alis.Reactive rewrite: the core design, its TLA+ specs, the modules mapped
onto it, and the units the rewrite runs in. `spec/BRIEF.md` is the session brief and the authority on what
goes here; `spec/NOTES.md` holds the decisions, facts and owner answers, append-only.

## Standing decisions

- The owner, 2026-11-05: this is a complete rewrite, not a rebuild, and it has to be right.
- The owner, 2026-11-05: no module and no feature on either line is lost, and the session improves every
  aspect. A door whose form is refused names the doors that carry its capability; `spec/tools/check.py`
  prints the features lost, and done is zero.
- The owner, 2026-11-05: Alis.Reactive.NativeTagHelpers and Alis.Reactive.DesignSystem are out of scope on
  both lines (BRIEF.md section 4). Their inventory rows stay, marked out, and no check counts them.
- The owner, 2026-11-05: neither line's public surface is the contract. Main opened the core's internals to
  its own packages, and the redesign repeats the same mistakes; both lines are evidence of features, not of
  the design. A door is public only when a view author, a validator author or a component author needs it.
  A door that is an implementation detail is refused with that reason and the doors that carry its feature.
- The owner, 2026-11-05: any component can be onboarded on the core's public surface. Fusion and Native get
  nothing an app does not.
- The owner, 2026-11-05: the core takes as few dependencies as it can, and each one is the owner's decision.
  Dependency injection is one. Every dependency is pinned to one version.
- The owner, 2026-11-05: the HTTP module is requests and responses. What comes back from the server belongs
  to it: the status, the body, how the browser decodes it, and the error body each host sends.
- The owner, 2026-11-05: work survives compaction. A correction is written here and in `spec/NOTES.md` the
  turn it is given, and the compact hook prints this section.
- The owner, 2026-11-06: the request lifecycle keeps every feature and has a condensed default shape that
  covers most requests; a developer overrides any part. `.cshtml` MVC views made parameters and conditions
  complicated, and a more condensed typed syntax that loses no feature is welcome (step 7, gate 2).
- The owner, 2026-11-06: a request runs in any event or callback; conditions work uniformly in every block
  of a request.
- The owner, 2026-11-06: conditions have had two syntaxes in views; there is one uniform syntax, a branch
  holds several statements, and a branch is a simple lexical scope, as a C# `{}` block is.
- The owner, 2026-11-06: dates and time zones were not done right or to the standards; the spec states each
  date form by the standard it follows and pins each host's reading and writing of it by a probe.
- The owner, 2026-11-06: validation that agrees on the server and in the browser is a must, and much time was
  lost on it. The module uses no reflection, has one uniform syntax, and supports conditional and complex
  validation on both sides.
- The owner, 2026-11-06: culture is the server's, never the browser's: the server is the truth, and every
  culture-dependent read or write of a number or a date in the browser follows the culture the view rendered
  under.
- The owner, 2026-11-06: Syncfusion is one consumer of the framework, its dependency inverted. Syncfusion
  works with camel-case JSON and with other naming, so the framework supports the JSON naming an app
  configures, not camel case alone.
- The owner, 2026-11-06: MVC partial semantics get special attention. No weakly typed passing (ViewData)
  carries plan state. ResolvePlan and the plan primitives were designed to stop poisoning the HTTP request;
  a design that stores plan state on the request is the easy route, and it is refused.
- The owner, 2026-11-06: plugins are an integral part of the design, where customization is needed and a
  better developer experience can be offered; the request lifecycle calls into a plugin in a typed way,
  as every block does.
- The owner, 2026-11-06: "follows the server" means the framework allows the server to decide. Example to
  think through, never a framework concept: a company has many communities, each with its own configured
  time zone. The framework offers the time zone a view renders under as the app's choice, as culture is.
- The owner, 2026-11-06: we design a framework, not a one-off case. A host difference is the framework's to
  absorb behind one door on both hosts; the net48 time zone maps its Windows id to IANA by a table made once.
- The owner, 2026-11-06: the framework's views are `.cshtml` MVC views, on MVC 5 and ASP.NET Core MVC, never
  Razor Pages; the spec says "view" or "MVC view", and "Razor" only inside a code path.
- The owner, 2026-11-06: the framework controls every input's id. Without it the browser starts guessing,
  validation does not find its inputs, and views of different partials that share an id conflict; the
  declaring-class id is that decision.
- The owner, 2026-11-06: templates have a typed way to write HTML, with much room to grow; keep it. It relies on
  Syncfusion's compile, and it stays the Fusion module's, not rewritten: it brings a JavaScript dependency the
  bundle always holds and no server dependency.
- The owner, 2026-11-06, to be made unmistakable: the framework depends on no consumer. The dependency is
  inverted: Fusion and Native are consumers built on the core's public surface, and the core names none of them.
- The owner, 2026-11-06: bad patterns start small and spiral, so each mechanism stays limited to its exact use
  case. Browser and server validation work from one fact, a rule written once and never twice.
- The owner, 2026-11-06: MVC partials are a great feature for dynamic UI: a partial loads where a condition
  holds and leaves when it changes (a state whose form needs 10 fields, another whose form needs 20). Each
  partial has its own tight model, and validation rules are written for that model; more models, decoupled,
  and the plan stays tight, never carried on the HTTP request or by weak typing. The request lifecycle with all
  its features and its condensed default form is the game changer.
- The owner, 2026-11-06: a partial is attached to its parent's plan (one model with several associations) or
  independent (loaded where a condition holds, its own thing), and both work. Partials nest: inside a plan a
  view renders another, entirely different view with its own plan and its own children. Main merged plans by
  an algorithm on purpose; the rewrite improves on it from the framework's core.
- The owner, 2026-11-06, said several times: never just agree. An example the owner gives is an example, not the
  rule ("my 500 was an example"). Think each piece of advice and each gap through against the evidence, and
  disagree with the reason where it does not hold: that is the job. This feedback is never lost.
- The owner, 2026-11-06: Syncfusion and the native components are consumers of the framework, the dependency
  inverted. The core exposes only what onboarding a component needs, takes no vendor library (Syncfusion's
  DataManager is named: the framework's own typed request lifecycle feeds components), and onboards no new
  component itself: any developer onboards one. After each compaction, reread these decisions, `spec/NOTES.md`
  and the root vision, which the compact hook prints, so the design does not drift.
- The owner, 2026-11-06: any JavaScript API can return a promise, so sync and async are decided by the
  declaration, never by what the browser finds.
- The owner, 2026-11-06: OpenTelemetry tracing is in scope and baked into the design (Q20): it records how the
  framework processes a request from start to end, helps while developing, and is optional, never always on.
- The owner, 2026-11-06: null and undefined are never a state, in C#, in TypeScript, anywhere; a value that can be absent is a closed
  union, and null is only a value the data holds. The nuance, the owner's words: "That is about the framework, not about the
  value." A contract whose field is nullable (a birth country typed as a string or null) is valid, and its declaration says so (`string?`).
- The owner, 2026-11-06: "when we onboard a component, we onboard the typed API exactly what that component has. So if the typed
  API is not available, then how would you even call via framework?" A declaration mirrors the typed API member for member, on
  every live object whatever holds it, a payload included: presence and nullability come from that API (D5.3).
- The owner, 2026-11-06: "Object which collide with the built-in C# object." No door takes the name of a .NET type:
  `LiveObject<T>()` and its list form replace `Object<T>()` and `Objects<T>()` (N5.5), since a member named
  `Object` on `JsObject` shadows `System.Object` in every declaration. CA1720 refused it in the POC's build
  (latest-recommended analysis, warnings as errors).
- The owner, 2026-11-06: "In the model, in the CSHTML, let's have a rule that both has to be same. Override cannot
  change it." An override keeps the JSON name and the write of the member it overrides. A read names the member as
  the class that declares it names it, and the class the read starts from must write that name. An override that
  changes either is the analyzer's to refuse at build (N18.5, built in POC 13; not built yet) (D18.3). The owner
  approved the solution the same day.
- The owner, 2026-11-06: "You understand runtime reflection versus compile time reflection. So system.type is a
  reflection, but it is not a runtime reflection in a sense where you are getting a list of methods and properties
  and all kind of stuff. That is bad." The framework refuses run-time discovery and use of members; a type's
  identity and the declaring class of a lambda's member are within the rule (D18.1, D18.4). Of the alternates the
  owner chose the declaring class: "I'm good with the declaring type... Use caution really, really good so we
  don't trip over." Every read of type information in the core is on the owner's list in `WhenTheCoreIsCompiled`,
  which fails on any other.
- The owner, 2026-11-06: "capturing the model value is important to keep our framework handle dynamic cases,
  otherwise it will become pointless. ... I would allow that" (reflection, over refusing captured values). An array
  lambda reads a value the view holds (`r => r.Ward == Model.Ward`, or a local it captures) when the view renders,
  by reading each field and property of its member chain with reflection (D6.7). The two reads go on the owner's
  list in `WhenTheCoreIsCompiled`.
- Chosen within the owner's yes, 2026-11-06: the member-chain read over compiling or caching compiled accessors,
  because it generates no code, holds no cache, needs no warm-up and is the same API on net48 and net10; it reads
  a value in 0.02 µs (a local) to 0.04 µs (`Model.Ward`) on net10, against 32 to 39 µs to compile per value (the
  redesign's way). A view value that calls a method or computes (`Model.Ward.ToUpper()`) is refused, naming its
  door, a local computed before the lambda. The owner, 2026-11-06: "I am totally fine as long as its captured as
  view render so dynmic stuff.. but if var works i do not see any downside. I am always hestiant reaching to
  reflection, or even compile" (D6.7).
- The owner, 2026-11-06, chose the identity table for the .NET members an array lambda calls (`Trim`, `Length`,
  `Year`, LINQ's own operations inside a lambda): one closed table keyed by the identity the expression tree carries,
  built from typed lambdas, one file and one entry on the owner's list (D6.5, D18.1).
- The owner, 2026-11-06, chose that an `int` total past the range of `int` stops its pipeline, as .NET's checked
  `Sum` refuses it (D6.11).
- The owner, 2026-11-06: "for decimal use normal finance standard bascially what banks use or have it configured
  at global level". A `decimal` is summed in the browser as decimal arithmetic, never binary floating point: the
  terms are added as whole numbers at the largest scale among them and the point is placed once, exact as .NET's
  `decimal` addition is (D6.11). Then: "imo its 2 digits after decimal point enough for this framework with knob on
  how to roudn". Two app-wide knobs, places (2) and rounding (.NET's banker's rounding, half to even, unless the
  app chooses half away from zero). They apply only where an operation must round (a division, an average, a value fitted to its
  places); sums and comparisons stay exact, because doses in this domain carry three places (digoxin 0.125 mg) and
  a browser that rounds them to 0.12 disagrees with .NET. The owner: "ah i see", "good point".
- The owner, 2026-11-06: branches that nest to any depth (D8.7, D8.8) are "golden".
- The owner, 2026-11-06: the slices are vertical in an app as in the framework (the owner's words are in
  `spec/NOTES.md`). An action that needs a model has a model of its own, plain, shared with no other view; an app
  shares a base class only for abstract members it enforces. The docs and the component onboarding skill teach
  it as the app's standard; the framework refuses nothing for it.
- The owner, 2026-11-06: no defensive code where a type or a check stops the bug; no closest match; fail loudly, so a
  bug surfaces and challenges the design. Tests are Playwright in a real browser, never jsdom, which is brittle.
  The reference components are built around JavaScript object complexity (dot notation, properties, callbacks,
  events) in several versions, to match Syncfusion's complexity when it is onboarded as a consumer. Analyzers are a
  harness written as .NET's open-source analyzers are, never another surface; a test's name, its arranged data and
  its assertion are each crucial, and their absence has cost months.
- The owner, 2026-11-06: "Let's drop real time from .NET 4.8." Realtime is net10's alone: its doors are on net10's
  surface only, the net48 realtime rows of the inventory stay, marked out, and no check counts them as lost.
- The owner, 2026-11-06: strict quality is enforced by tools, in the browser's TypeScript by lint rules and in C# by
  analyzers, and the analyzers are written so rigorously that they never become another surface that drifts: a
  harness, not extra work. The tests and the custom components are what proves the framework's coverage. Each lint
  rule and each analyzer enforces one line of this spec, the line names its rule, and a rule with no line leaves.
- The owner, 2026-11-06: before implementation, a proved plan. The core's modules with their coupling and cohesion
  found early; tools chosen so they are no burden; the spec's own work reviewed so it does not become another layer
  that breeds; a greenfield plan proven by POCs. The domain model that captures intent is sound, and onboarding is
  proven for a writable property, a readable one, dot notation, complex arrays, an event, a callback and the vendor's
  way of finding its JavaScript object. The fluent builder's naming is close and is improved. Gate 1 waits for the POCs.
- The owner, 2026-11-06: the TLA+ work must be good in code, not hypothetical. A model invariant that restates its
  own action proves nothing; a model earns its place only over interleavings, and each of its invariants is a named
  Playwright test on the code it models.
- The owner, 2026-11-06: "I am totally fine with one assembly, but namespacing is logical [modularization]." The core
  is one assembly; each module is one namespace and one folder of the same name, its public and internal types
  together, and the namespace is the module boundary.
- The owner, 2026-11-06, on the POC 4 report: "i will with your recommondations - decide for quality". So: a data
  member absent from a present item reads its starting value, as the endpoint binds it (Decision 1, A); a holder that
  is null or absent still reads nothing (D4.2). Text ordered with no comparer orders by the view's culture with the
  browser's collator, the invariant culture as `en`, invariant globalization mode as ordinal, and a culture the
  browser lacks stops the pipeline (Decision 3, a). `Regex.IsMatch` in a list's lambda is keyed in the identity table
  and its pattern passes D16.11's one reader, the rules `Matches` takes; the core runs no `Regex`, and the key is the
  ban's one exception (Decision 2, decided for quality: the redesign has the member, and a second regex meaning is
  worse than either option). The CA1304 and CA1311 suppressions on the identity table and the POC app's request
  culture are approved, and so is the list of fifteen POCs in `spec/NOTES.md`.
- The owner, 2026-11-06, confirming the scope of Decision 1: "that observation is correct", then "so having some sort of
  good explicit solution is needed". Every typed data read, not only a list's lambda, reads an absent member's start,
  and the plan states it: each step of a `data` read carries `start`, `nothing` or `unbound` (D4.2). A data read follows
  the contract that wrote the data, System.Text.Json under the app's options on both hosts; a form's check follows the
  endpoint that binds its body (D16.6). The root CLAUDE.md's "Data reads as nothing when absent" describes main and the
  redesign, and changes when the rewrite lands.
- For this folder, the root rule against other gates, coverage maps, checklists and proof artifacts does
  not apply, and the matrices are an upfront grid by the owner's decision.
- Facts here come from the code at a SHA, a command's result line, or an oracle's source. A pilot's or
  an agent's draft is a lead until its cites are re-read at the source.

## Modules

The owner, 2026-11-05: "The modules are close to right", with requests and responses as one module. Each
module's folder is its name in PascalCase. A core module's C# is in Alis.Reactive and its browser code in
Alis.Reactive.Assets.

| # | Module | Who sees it | What it owns | Questions |
|---|--------|-------------|--------------|-----------|
| 1 | Triggers and pipelines | view author | on load, on an event, declared app events, dispatch, the steps a pipeline runs | Q11 |
| 2 | Values and sources | view author | the one read, literals, the one conversion, every source at every value position, the page's URL | Q2, Q3, Q7 |
| 3 | Conditions | view and validator author | one condition core behind the pipeline, validator and template doors | Q8 |
| 4 | Array lambdas | view author | Where, Select, Any and the rest over items with dotted members | Q6 |
| 5 | Elements | view author | an element's text, HTML, classes and attributes, by id | Q4 |
| 6 | Requests and responses | view author | the typed request; the response, its status, its body and how the browser decodes it, the error body per host; the flow: success, error, chained, parallel, loading, finally, into | Q9, Q10 |
| 7 | Inputs and ids | view author | an input bound to a member under MVC partials, and the refusals at render | Q14 |
| 8 | Validation | validator author | a rule written once, the server's verdict and the browser's rule in agreement, registration through dependency injection | Q16 |
| 9 | Partials and slots | view author | plan composition by plan id, slot load and unload | Q12, Q14 |
| 10 | Realtime | view author | server-sent events and SignalR on net10 only: holds, lifecycle triggers, typed messages, the hub's own asset | Q5, Q11, Q21 |
| 11 | Declarations | component author | the live-object and data vocabulary, and which members are required | Q5 |
| 12 | Vendors | component author | vendor data, the one driver, mount and unmount, the bridge to a component's browser code | Q17 |
| 13 | Plugins | component author | the escape hatch: a plugin by name with typed members | Q7 |
| 14 | Schema | internal | the closed node set of the plan grammar and the generated TypeScript contract | Q19 |
| 15 | Paths and wire | internal | the typed path step, a member's four names, encoding and dates per host | Q1, Q2, Q3 |
| 16 | Type knowledge | internal | members, shapes, item types and defaults, without reflection | Q18 |
| 17 | Execution | internal | the evaluator, lanes and time, boot | Q4, Q11 |
| 18 | Ownership and state | internal | the page, a plan and a slot, what each holds, dispose, where state lives | Q12, Q13 |
| 19 | Rendering and delivery | host | rendering a plan, assets by content hash, the net48 and net10 host splits | Q14 |
| 20 | Analyzers | build | the ALIS rules, and the new rules for reflection and vendor contract drift | Q18 |
| 21 | Fusion components | built on the core | the Syncfusion slices, with the reference FusionMultiSelect | Q17 |
| 22 | Fusion templates | built on the core | the Syncfusion template engine and its conditionals | Q15 |
| 23 | Fusion app-level objects | built on the core | FusionConfirm and FusionToast | Q11 |
| 24 | Native components | built on the core | the plain-DOM components, with the native reference | Q17 |
| 25 | Native app-level objects | built on the core | NativeDrawer, NativeLoader, NativeConfirm, NativeToast and the action link, the framework's own, styled by the app | Q11, Q17 |
| 26 | Tracing | view author and operator | the diagnostics port, the plan's trace context, spans and events by a closed vocabulary, propagation to the server, the server's activity source, the OpenTelemetry adapter built on the port | Q20 |

## Files

| File | What it holds | Written by |
|------|---------------|------------|
| `spec/BRIEF.md` | the session brief, with the owner's corrections | the owner's words |
| `spec/NOTES.md` | one entry per decision, fact or owner answer, two lines at most, with its evidence | appended by hand |
| `spec/tools/check.py` | every check the brief's section 1 names, and a brief mode for the compact hook | hand |
| `spec/STATE.json` | the phase, the step, each check's result and counts, the next action | `spec/tools/check.py` only |

## How a step runs

Re-read the step's section of `spec/BRIEF.md`, do it, run `python3 spec/tools/check.py`, append what was
decided to `spec/NOTES.md`, then commit by path and push the spec branch the one way section 16 of the brief allows.
