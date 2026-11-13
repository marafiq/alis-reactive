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
  Dependency injection is one. Every dependency is pinned to one version; at gate 1 (C7) the
  DI abstractions take one exact version per target framework, set beside System.Text.Json's 10.0.12. The owner's
  rule of 2026-11-11 moves both to the 10.0 line's latest patch, section 2.7's.
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
  bundle always holds and no server dependency. The owner, 2026-11-10, raised a core template engine at gate 2 and
  reversed it the same hour, so the rewrite proceeds (verbatim in `spec/NOTES.md`): Syncfusion's engine stays through
  the rewrite, and a core engine returns only as the owner's own decision.
- The owner, 2026-11-10, choosing G6's default plus a view name (verbatim in `spec/NOTES.md`): MVC's design of its
  views must not fall apart, and a view name beats the boilerplate of a type per view. A door keeps MVC's own design
  of its views working: `Html.PartialFor(m => m.Member)` names the view by the member's type, and a view name, a
  string kind, names it when a type has several.
- The owner, 2026-11-10: "u made it look like serval time that it was easy and SF will work without any issues". A
  new direction is put to the owner with its cost and its risks beside its evidence, never with the evidence alone.
- The owner, 2026-11-10, at gate 2: "run few experiments with software devs about their authoring experience and
  consider but core design don't take 90 degree turn - it's about bringing more finished and polished questions to
  me". A form goes to the owner after authoring experiments have polished it, and no experiment turns the core design.
- The owner, 2026-11-10, gate 2's G13: a form the spec deferred to gate 2 is approved at what its line writes, the form
  or default it states, else the door as the POC or spec/api compiles it; an item with neither comes back to the owner
  with a form before the unit that builds it.
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
- The owner, 2026-11-10, after MVC's rendering forms were tried: "I am fine with prefix as long as MVC permitives work",
  and a partial needs to know only whether it is its own plan or attached to another plan. The prefix design stays
  (D14.3): every MVC door that renders a member's view under its prefix attaches it (D16.3); a view rendered with a
  model and no prefix is its own plan, named as MVC names it.
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
  changes either is the analyzer's to refuse at build (N18.5, built in POC 13f as ALIS025) (D18.3). The owner
  approved the solution the same day.
- Chosen within the owner's yes, 2026-11-07: a member System.Text.Json writes unlike the app's options (its own
  `[JsonConverter]`, or `[JsonNumberHandling]` on it or its class whose writing bits differ) is held in that form. In
  a list's lambda, a value compared with it by `==` or `!=` is written in that form, so an enum with its own
  `JsonStringEnumConverter` compares as it is written; every other computation with it is refused, and a path past
  its own converter is refused on every data read (D4.2, D6.12, POC 13f-ii). This departs from D6.12's earlier
  "refuses it in a comparison", because the probe wrote the compared value through the member's form with no
  reflection. A `float` or `double` written as text is refused in equality too (-0 writes "-0", NaN equals its own
  text). Chosen within the owner's yes, 2026-11-09, gate 1's P15: a `decimal` the options or number handling write as
  text is compared by `==` and `!=` by value through the operator `eqDecimal`, since 0.10m and 0.1m write "0.10" and
  "0.1"; its ordering stays refused, and the POC refuses its equality until the rewrite builds it (D6.12).
- Chosen within the owner's yes, 2026-11-07: a source carries the form its value is held in. A value written beside
  it (a condition's literal, `IfNull`'s fallback, `Choose`'s literal arm) is written in that form (D7.7, D8.10, POC
  13f-iii). Chosen within the owner's yes, 2026-11-09, gate 1's P8: who wrote a value decides its base form, the
  app's for what the server's JSON wrote and the live form for what a live object holds or hands, a payload declared
  as data included; the live form keeps the app's naming and nothing else its options set (every number a JSON
  number, every null written, no converter of the app's), so a live object takes a source in the app's form as held
  where the forms agree, a number held as text converted, an enum mapped by D9.13's rows, whose reads gate 1 granted
  (C37), and refuses anything else (X7.10); a literal JavaScript would read as another value is refused (X7.13); and
  the type check requires `[JsonPropertyName]` on a member it checks against a name the vendor declares (X17.48).
- Chosen within the owner's yes, 2026-11-07: a view reads past a nullable member in a data lambda with no `!` where
  the browser reads nothing past null. CS8602, and CS8629 on `.Value`, are suppressed on an expression that reads the
  item, in the predicate of `Count`, `Where`, `Any`, `All` and `Find`, the field of `Select` and `Find`, and the path of
  `Read`, and on a cast from a nullable there, which reads as its `Value` does (D6.14, POC 13g-ii). The warning stays
  where null stops the pipeline (a `Sum` term, an `OrderBy` key), on a value the view holds, and on an input's member
  path, which keeps the warning MVC's own helpers give (N6.3, POC 13g). Chosen within the owner's yes, 2026-11-09,
  gate 1's P6: a `Select` field whose type holds no null stops the pipeline where it reads nothing, so its warning
  stays, and one whose type holds null is null in its list (D6.16).
- Chosen within the owner's yes, 2026-11-08, 17c(a3-i): a request's member lambda in `Query` and `Set` names the key it
  sends and is never run, so CS8602 past a holder that holds null is suppressed there (ALIS036, N9.5) and a view writes
  `r => r.Handover.Round` with no `!`. The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: POC 14 builds one request, `Get(url).Into(container)`, since a slot
  loads only through a request (D10.11). The gate 1 flag that the approved list holds no request POC is amended to
  name it: a GET whose success body loads a slot, and nothing else of the HTTP module.
- Chosen within the owner's yes, 2026-11-07, POC 14: a declaration's unsubscribe is looked up with its subscribe when
  its handler starts, and a handler whose object lacks it, or whose returned subscribe gives back no remover, does not
  start (the second's listener runs nothing), since no release could remove what it added. Plans start in the order
  the page holds their scripts. A slot announces the unmount of its content whether or not a plan gave it an owner
  (D12.3). `HttpSend` is a literal in the POC, which has no `record` node. Chosen within the owner's yes, 2026-11-09,
  gate 1's P13: the build checks the same listening members against the declaration's type wherever it reads the
  listening (a vendor or `EventListening` in the declaration's own compilation, its names constants); a vendor a
  referenced assembly defines is checked by that assembly's build, and on another build's declaration, as one computed
  at run time, it is met where a handler starts (D17.5, X17.45).
- Chosen within the owner's yes, 2026-11-07, POC 14d: a `listen` block reads the bindings of the blocks around it, and binds
  its payload and arguments under names of its own; a write to the payload of the event around it is refused where the
  view renders, and so is a listen on an object declared optional or nullable. An object whose events a pipeline listens
  to is an `EventedObject`, which declares its `EventListening` (once named PluginEvents: one name for one concept with two
  holders), so a listen on an object that declares no listening does not compile. The forms are gate 2's.
- Chosen within the owner's yes, 2026-11-07, POC 14e: D12.9 follows D14.6, since D14.1 refuses a record shared by the
  views of a response: a plan that sets the text or HTML of its own slot is refused where the view renders, and across
  plans the step stops in the browser, on a held slot's element or on an element that holds it. A slot is held while a
  live plan names it and while content a load or the server put in it has a live owner. An element's text and HTML
  mark their `set` as a content write.
- Chosen within the owner's yes, 2026-11-07, POC 15b-ii: a payload declared as a `Component` is that component, whose
  setter takes a write whenever the step runs, so D11.4's refusals (after an awaited step, and inside a `listen`) do
  not apply to it. The meaning is keyed on the declaration, never on the expression's shape; every other payload is
  refused as before.
- Chosen within the owner's yes, 2026-11-07, POC 15b-iii: a `Component` is an `EventedObject`, whose listening is its
  vendor's; an `EventedObject` declares its `EventListening` by overriding its listening (14d took it in the
  constructor), so neither holds a nullable. A listen on a component a payload hands over is removed at release only
  while the element it held when it listened still holds it where the vendor's `locate` finds it, the rule a handler's
  release follows; the vendor names where its object holds its element with an element path. Refused where the view
  renders: a listen on one whose vendor names no element path. Chosen within the owner's yes, 2026-11-09, gate 1's P5,
  answering 14a-i's flag (3): a listen on a component reached by its element's id is the door a slot's plan has to a
  component the page rendered, released by the element it reached, so its vendor needs no element path (D12.8).
- Chosen within the owner's yes, 2026-11-07, unit 17a, and moved to N10.7's form in 17c(a3-ii), 2026-11-08: a request's
  routes are written on the request, `pipeline.Get<TRequest>(url, request => request.Set(r => r.Member, value or
  source)).OnSuccess<T>((reply, pipeline) => ..).OnError<T>(status, (reply, pipeline) => ..).OnError((failed, pipeline)
  => ..)`; the body is `reply.Body`, a source read through the data path `MethodResult.Read` uses, and a header is
  `reply.Headers.Get(name)`, on every reply, read by the browser's `Headers.get` (N10.7). The form is gate 2's.
- Chosen within the owner's yes, 2026-11-07, unit 16a: a declaration's value type is checked against its vendor's
  TypeScript at build (ALIS037, the type check, which POC 16a numbered ALIS025, the override analyzer's id, D17.5), the rule
  `spec/Core/DESIGN.md` states for a payload ("The member's C# type is the API's value type") and the analyzer
  checked only for null: a property's value, both ways, a method's result, awaited or
  not, and the data past them member by member by their JSON names. The closed set: text (string, char, Guid, an
  enum whose type carries a string converter, and on net10 DateOnly and TimeOnly, by gate 1's P9), a number (the
  numeric types, any other enum, by gate 1's P8), a boolean, a
  date (DateTime, DateTimeOffset, read from text too, since data carries a date as text), a list of these, and an
  object whose members the vendor's type declares, or takes through a string index signature as the index's type. A
  member
  the vendor's type declares and the data lacks reads its start (D4.2 unchanged); a member it does not declare is
  refused, on the declaration's line, with its path. The vendor's untyped shapes (any, unknown, object, Object) take
  any type. Refused, each a gap to name: a member that holds a promise, a function, nothing, or more than one kind
  of value. A component reached where its vendor hands a base of the type it mirrors is refused on the reaching line
  (ALIS021). Chosen within the owner's yes, 2026-11-09, gate 1's P12: a method's arguments, a callback's answer, a
  listener's argument at a position and the result of a call of the object itself are checked against the same
  contract, built in the rewrite's analyzer unit (D17.5, X17.40 to X17.42).
- The owner, 2026-11-07, walking the SDK map and a native input: "please take my input about keeping the quality high, as we
  have to onboard 100 of components". Onboarding is judged as one of hundreds: a rough edge in a declaration is
  repeated in every component that meets it. Approved on the walk: a payload, and what it holds, nests in its component's
  class as that event's own slice, and two events that hand the same shape each declare it ("as payloads grow in their
  own way so duplication is fine there"); a component an event hands stays its own class. The walk's findings (where a
  reached declaration's error lands, a handler's way to its own component, a text's length in a view, the id string a
  view repeats, `name:`'s null) are in `spec/NOTES.md`, 2026-11-07, each for gate 2.
- The owner, 2026-11-07, after trying a component on a native input whose event hands promises: "yes, covering this
  is necessary along with harmony with other modules", then "Ok I let you complete the work as POC and improve design
  once ready I will try and I will try other features as well". The native input and the promise payload are completed
  as POC pages with their proof, the design gaps the walk found are closed in the POC where the core states the meaning,
  and the owner tries the result before gate 1.
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
  shares a base class only for abstract members it enforces. README.md and the component onboarding skill teach
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
  and the plan states it: each step of a `data` read carries `start`, `nothing` or `unbound` (D4.2). The root CLAUDE.md's
  "Data reads as nothing when absent" describes main and the redesign, and changes when the rewrite lands.
- Chosen within the owner's yes, 2026-11-06: a data read's start is System.Text.Json's binding under the app's options,
  on both hosts, the contract that wrote and names the data (D2.9). The owner's words were "as the endpoint binds it";
  on net48 the endpoint is MVC 5's binder, which fills the nested objects a model starts with and cannot create a type
  with no public parameterless constructor (a positional record), so on net48 the browser reads a start that endpoint
  would not bind. One meaning on both hosts is the framework absorbing the host difference; a form's check still
  follows the endpoint that binds its body, by host (D16.6). The owner can overrule it.
- Chosen within Decision 3 (a), 2026-11-06: the plan carries the collation once, in its culture (D3.11), and C# picks
  each sort's comparison from how .NET compares under the view's culture: `"a"` after `"B"` is invariant globalization
  mode, ordered by code unit; a culture whose sort is an alternate one ("de-DE_phoneb") is refused, because .NET's
  order for it is not one order across hosts (on macOS .NET 10 orders it as "de-DE") and the browser's tag for it is
  not certain; `StringComparer.CurrentCulture` orders as no comparer does. The owner can overrule each.
- Chosen within Decision 2, 2026-11-06, on a probe: D16.11's reader refuses `IgnoreCase`, which D16.11 admitted with
  `CultureInvariant`. The sweep of the BMP's case pairs found no JavaScript flag that folds case as .NET does (166 pairs
  differ under "i", 100 under "iu", among them "s" against U+017F), and the "u" flag reads a negated class by code
  point. The door is both cases spelled ("[Aa]spirin"). The browser's copy carries no flag; its closed set of
  constructs, lookbehind among them, agreed in both engines on every probed input. The owner can overrule it.
- The owner, 2026-11-07, after the MVC views POC: "go always remember, we are holding quality bar high, developer
  experience matter". Each door is judged by what a developer types in a real MVC view and what they meet when it
  goes wrong: a mistake surfaces where the view renders when the framework can see it, and otherwise the browser's
  error names the member and the cure.
- Chosen within the owner's yes, 2026-11-07: after a failed post, a component's typed starting value is the model's
  value as the action leaves it, not the text the browser sent, on both hosts: net10 keeps no typed form of an
  attempted value, MVC 5 converts one only by a `System.Type`, and a typed component shows what the server decided;
  a text box keeps MVC's attempted text (D17.13).
  The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: the vendor contract (D17.5) is a table of every type each module of the
  app's TypeScript program declares, written by an MSBuild target into obj/ before CoreCompile and never committed,
  because lib.dom alone holds 60,695 members and a stale committed copy could compile green. A component or plugin
  declaration names its type with `[Mirrors(module, type)]`; nullability follows variance (what the member reads fits
  the C# type, what the C# type writes fits the member); a drift a browser test arranges on purpose carries
  `SuppressMessage` naming the test. The owner can overrule it. Chosen within the owner's yes, 2026-11-09, gate 1's
  P11: a member's JSDoc `@default null` is part of its API, so the contract reads the member as holding null and cites
  the tag's d.ts file and line, and a `T?` declaration of it (NumericTextBox's `value: number`) needs no suppression;
  the suppression set stays the one above (D5.3, D17.5, X17.44).
- Chosen within the owner's yes, 2026-11-07, reversing D7.3's `OrNull()` at a struct's set on a new fact (the contract
  makes every `T | null` member a `T?`, and the POC's census broke 178 writes): a set on `Property<T?>` or
  `WriteOnly<T?>` takes a `T` source as C# assignment does (`NullableWrites`, N7.6), a door added to the public surface.
  Its cost: it is an extension in `Alis.Reactive.Components`, and a view that does not import it meets a bare CS1503.
  The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: a declaration a root reaches is checked at the type the vendor's API gives
  what reaches it, with no attribute of its own (D17.5, X17.29). A drift is cured where the vendor's types under-state
  what it hands (the current pump typed `InfusionPump<PumpChannel>`), and suppressed naming its test only where a
  browser test arranges it (an older build's bed or alarm). A positional door on a declaration no listener or callback
  hands is refused at the build, not first where the view renders (X17.30). Root checks stay live and reached checks
  run at the compilation's end. The owner can overrule it. Chosen within the owner's yes, 2026-11-09, gate 1's P10: a
  callable property typed by an alias with one type parameter its own type does not name, whose function types its
  first parameter `any`, hands that type argument as its listener's first parameter, read from the declared type node,
  so Syncfusion's `EmitType<ChangeEventArgs>` hands `ChangeEventArgs` (D17.5, X17.43).
- Chosen within the owner's yes, 2026-11-07: presence is the member's mark, not its holder's (D5.3). An optional member
  that its holder lacks or holds as undefined is absent, since TypeScript's `?` admits both (a class field compiled to
  ES2022 holds undefined): it reads nothing, a call of it runs nothing, and a write sets it. An optional member of a
  live object declared neither optional nor nullable stops when that object holds nothing, as any member of it does.
  The plan's read presence is two halves, the holder and the member, so it gains `optionalMember` beside `optional`.
  The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: a live object a member holds or a method returns, which its API types
  `T | null` or `T | undefined`, is declared `nullable: true`, and its members read nothing while it holds nothing.
  ALIS019 refuses the declaration without it, by variance as a value's nullability (a `nullable: true` the API does not
  need is allowed). `optional:` and `nullable:` are named arguments in the POC, and their form goes to gate 2 with
  N5.4's. The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: a declaration door is the body of the property or method it declares
  (ALIS023), and the arguments the build reads are constants (ALIS024). A public member that returns a private
  member's door is not refused: the private member declares its own JavaScript name, which only the contract checks,
  on roots and reached types, and ALIS016 then points at the private member. The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: a date or an instant that reads nothing writes no text, and a request sends
  it by D9.10's table: a `DateTimeOffset` is left out and binds its start, a `DateTimeOffset?` binds `null` (D3.8,
  `spec/PathsAndWire/DateWire.tla` NothingWritesNoText). The redesign's earliest moment was the start of a non-nullable member with no
  initializer, which leaving it out binds; sent always, it bound a nullable as that moment and overrode an initializer.
  The owner can overrule it.
- Chosen within the owner's yes, 2026-11-07: a list item that reads nothing is sent as an empty field when its type admits
  nothing (`string`, a nullable scalar) and stops the request otherwise, decided by a closed set of types, never by a
  type's members (`ComparedTypes.AdmitsNothing`). A present value not in its member's form stops the request. The owner
  can overrule it.
- The owner, 2026-11-07, on 17a's report: "when you say not exercise, this is a hatchword to make mistakes or drift". A
  unit's report carries no "not exercised" line: what the spec decides for the unit's door is built and proven in that
  unit, or the unit is not reported done. 17a reported done with `Set` taking text only; unit 17b builds the members
  D9.6, D9.10, D9.19 and D9.4 decide. An environment the POC lacks (net48) is a work item with its unit, not a caveat.
- The owner, 2026-11-07, on an object's field names in a query: "I would prefer source generated because that makes
  the code more stable, internal, performant. ... If that goes beyond it, we can enforce a standard on the query string
  itself ... Reflection is ... there to help you out to drive the quality of the framework. It's not to hinder your
  progress." A request member that holds an object or a list of objects is keyed by its fields' C# names (D2.5) from a
  table a source generator writes at build: for each complex parameter of a controller action and each type reachable
  from it, typed member lambdas the core names by expression text and by `JsonPath`, with no run-time reflection. Views
  are compiled by Razor's own generator, which no other generator sees, so the endpoint's parameter is the trigger.
  The query standard (keys as the JSON names, read back on the server by the naming policy) is the fallback.
- Chosen within the owner's yes, 2026-11-07, 17b-ii's probe (31 facts, D9.10, D9.13, D9.19, D9.20): an object that reads
  nothing is left out of a query or form data, since an empty field binds null on MVC 5 and keeps the start on net10; an
  item of a list of objects that reads nothing stops the request; a list member is sent only when declared `List<T>`; a
  member net10 binds under another name, a `[BindNever]` member and a member of another list type are marked in the
  generated table and refused where a view sends them, never at build, so an app's endpoints no framework view sends still
  build. A parameter prefix and a parameter named like its type's member bind none of the keys, accepted at gate 1 as
  MVC's binding, which the request door's XML doc names (C25). Request.tla checks the first two over both hosts. The owner can overrule
  each.
- The owner, 2026-11-07 (verbatim in `spec/NOTES.md`): the official default is C# 7.3, most of C# 8 works on net48 with
  few gotchas, so assume C# 8. An MVC 5 app is assumed at C# 8, so nothing the framework writes into an app needs
  C# 9 (no module initializer); generated code is written in C# 7.3 syntax, which compiles in an app left at the default.
- The owner, 2026-11-07: "yeah one call is fine". An app hands the generated request tables to the framework with one
  startup call, the same on both hosts (net48 `Application_Start`, net10's startup file); no host registers by itself.
- The owner, 2026-11-07, on the generators: "I think for validators it does make sense. Compile time JSOn i would defer
  it". A generator writes the app's validator list in the validation unit, replacing `Add<TValidator>()` per validator
  (D16's gate 1 exception); compile-time plan JSON is deferred, so 17b-ii measures no render-cost baseline for it.
  On `Set`: "Ok if Set is better than keep it. I will work with u on one example when ready"; the name stays until then.
- The owner, 2026-11-07, on sending an object in a query: "instead of making it complex ... burden is on the dev to know what
  query string name is and what type of value", then "I am fine for lambda, but you have to evaluate the value it is
  bring". A request sends each field the view names, typed by its member lambda (`r => r.Handover.Round`), keyed by the
  lambda's C# path; no generator, table or reflection serves requests. The validator generator and its startup call
  stay. D9.19 and D9.20 are rewritten once the owner has seen the complete example.
- The owner, 2026-11-07: "we really do not want take control mvc binding - must have solid reasons to do it". The framework
  writes the keys and values MVC's own binder reads, pinned by probes on both hosts, and adds no value provider, binder
  or model of the binder without a reason the owner accepts.
- Chosen within the owner's yes, 2026-11-09, by precedent ("well solve 17c like you solve similar problems, it do not
  need me to give you decision"): D9.13's rows are written through HeldForm from the enum's values, never from a
  generated table, and the reads that list the values are D18.1's, granted at gate 1 (C37); a source's combination of
  an enum the app does not name as flags stops; a JSON body's enum is the form its endpoint binds. The owner can
  overrule each. Chosen within the owner's yes, 2026-11-09, gate 1's P2: an enum is flags to the framework only when
  the startup names it, `Flags<TEnum>()` (N16.4), never by a `FlagsAttribute` read; a source's value of it is split as
  `Enum.ToString` splits it and sent as that text, and `IsInEnum` over it is FluentValidation's flags meaning on both
  sides; ALIS038 admits the door only over a `[Flags]` enum whose values are from 0 to 2^53 - 1 and disjoint or nested
  (D9.13, D16.10, N16.5). The form is gate 2's. The owner can overrule it.
- The owner, 2026-11-07: a request is the HTTP message, and the core holds its parts: the method, the path, the query,
  the headers and a body with its media type, JSON and multipart form data (a form that uploads files among them);
  the response's status and its body as JSON, text or HTML. The methods are GET, HEAD, OPTIONS, POST, PUT and DELETE (OPTIONS by the owner's "OPTIONS support is
  required as well"). PATCH is
  not a method of 1.0, by the owner's leave: its two body standards (RFC 7396, RFC 6902) carry their own media types
  and give a member left out and a member sent as null different meanings, which D9.10's table does not state. Every
  part takes every source (Q7), and every response body is a source for the modules after it: the page's query read
  in a condition inside an event, a chained request's URL from the first response or the page's query. `Set` and the
  other condensed doors write the plan the full form writes, so they add no plan shape; a door the view calls with only
  its own type argument is an instance member, since an extension of a generic receiver binds that call in neither the
  classic form nor a C# 14 block (N10.7).
- The owner, 2026-11-07: "You can only skip Patch", because a content type or a primitive done wrong breaks
  encapsulation everywhere above it. The request bodies are the Fetch Standard's: none, JSON, URL-encoded, multipart,
  text and one file as the whole body. The response bodies are read as none, JSON, text, HTML or a file the browser saves.
  Text is decoded by its `charset` parameter, because fetch's `text()` decodes UTF-8 whatever the header says. Streams
  stay out by the owner's 2026-06-10 decision.
- The owner, 2026-11-07: "multi part body think in permtives, multiple files with other payload is right complexity".
  A multipart body is an ordered list of parts, each a name and either text or a file (its bytes, file name and media
  type); several files and fields go in one body, under one name or nested by C# path (`Documents[0].File` beside
  `Documents[0].Title`). The host's file types (`IFormFile` on net10, MVC 5's posted-file base, and their lists) only type the
  member.
- The owner, 2026-11-08: the framework ships its spinner and toast, and the condensed form depends on contracts the core
  owns, a busy indicator and a notice, never on their CSS or markup; an app puts its own component behind either, and
  the full form takes full control. The core counts each target's requests and calls its show at the first and its
  hide at the last, so a component with a plain show and hide is correct under Parallel, two pipelines at once, retry,
  replace and a slot's unload (`spec/RequestsAndResponses/Composition.tla`).
- The owner, 2026-11-08: the default loader shows on the target a request updates, right for parallel requests of 5
  and 1 seconds on one target or on two (verbatim in `spec/NOTES.md`). Busy is a state of a target, as WAI-ARIA's aria-busy is:
  the elements its success route writes outside a condition, the element it loads, the container it sends, or the elements
  `Busy` names in place of those, else the page; the C# derives them, the plan carries them, and the core counts each
  target on its own (D10.21).
- The owner, 2026-11-08, on schema.md left behind the HTTP decisions and N10.7 drifted from D10.14's retry: a drift is a
  big mistake, caught or not. A decision changes in one commit with every place that carries its meaning: its D and N
  lines, its question's schema nodes, `spec/Core/schema.md`, the TLA+ specs, `spec/api` and this file; the unit's report
  names each one it touched, and why each other one stands. A green check proves only what it compares.
- The owner, 2026-11-08: a page holds many forms or none, and a request works without a form on the full form's
  parts; no door, default or check assumes one form per page or a form behind every request.
- The owner, 2026-11-08: forms nest, sit out of band, or are containers of any kind; the concept is the request
  container, and no type is named PageForm. The native `<form>` stays the browser's way to send a form's body (`Form`), and the core
  sends any container's framework inputs (`Inputs`) and the view's own input (`Input`) beside it, losing no feature.
- The owner, 2026-11-08: a rule the source states is the compiler's. A type stages the door; where no type can, an
  analyzer refuses it; a refusal where the view renders names what only the render knows (the app's options, a value
  read at run time, another view). Every compiler rule of both lines carries a verdict in the core design.
- The owner, 2026-11-08: parallel names are prohibited. One concept has one door, and a slice's door that repeats a
  core door (a link's own `Post` beside the pipeline's) is refused.
- The owner, 2026-11-08: row actions serve any grid-like component, never one vendor's. An action is declared once,
  outside its rows, as one request in the condensed form with a payload of any type, and each row renders it with its
  own values; declared once is no license for a weaker design (types, one request module, every module, no guessing).
- The owner, 2026-11-08: every module works with every other, each HTTP thing is a typed JavaScript object whose members
  are sources and dot-walk (the response's status, headers and body, the page's URL, the request's fields), and main is
  the floor of features, never the bar of quality.
- The owner, 2026-11-08: every stage is a pipeline, and each holds several statements of every module: a handler, each
  branch, each route, `WhileLoading`, `Finally`, the block `Chained` runs, each `Parallel` branch and the defaults' blocks.
  Conditions, the page's URL, every reply's status, headers and body, and the request as sent dot-walk in each, and a
  chained request reads every reply and request around it. A step that waits holds what follows it, a route's tail
  included (D11.10, D11.5, `spec/Execution/Run.tla`).
- Chosen within the owner's yes, 2026-11-08: a row action is one handler per action and one entry per rendered
  control, and the entry holds only the row members the action's block reads; a button takes any block and a link a
  block whose one request is a GET, its href; the handler's policy spans the action's rows, and a link reads nothing
  another script did with the click (D17.15). Both lines write a whole plan per link. The owner can overrule it.
- The owner, 2026-11-08, amending G2: "I can give you go ahead for amend, but you have to make sure the quality bar stay
  high, and think through and tell me if this make sense or not." HTTP adds one statement, `request`, whose executor is
  `spec/RequestsAndResponses/Request.tla`'s state machine, and a pipeline is one run, `spec/Execution/Run.tla`'s, whose driver alone touches a
  promise (D10.23, D11.11, D11.12). The bar the owner set with it: the machines simplify the runtime's mix of sync and
  async, and an async call in a stack that started sync is proven by TLA+ and then in the browser.
- The owner, 2026-11-09: "the synfusion TS is loose ... for event payload, hook up in the code with javascript in
  browser, and see what they are - I would not go after fullest chain of dom calls there but only public props and
  methods or promises", and "focus on stable high quality framework, since depenancy is inverted and component can be
  onbarded, for the featuers where there is low confidence, developers can request it and onboard, but I would love to
  make it automated but with high confidence." A shipped declaration holds a vendor member only with high confidence:
  the vendor's TypeScript declares it public (neither `@private` nor `@hidden`) with a concrete type, or a browser probe
  (`spec/probes/ej2-payloads.mjs`) pins what the browser hands or takes where that TypeScript says `any`, `Object` or a
  loose union. Any other member is refused with its reason, carried by the doors an app onboards it with (M21.N2,
  M12.N1), and the probes rerun on every vendor upgrade.
- The owner, 2026-11-09, on a Fusion slice's class name: "I prefixed with Fusion*" "to avoid name collision", and
  chose Syncfusion's name after the prefix. A slice's class is `Fusion` and the EJ2 class it renders, so an app that
  imports Syncfusion's own C# types (its MaskedTextBox in Syncfusion.EJ2.Inputs) meets no ambiguous name, and Syncfusion's
  documentation names the same component: FusionInputMask becomes FusionMaskedTextBox, and FusionFileUpload becomes
  FusionUploader; the other 48 slices already follow it.
- The owner, 2026-11-09: "html browser proof will actually be part of rewrite with high confidence where public
  members are not clear enough". Each browser fact a declaration rests on names the proof the rewrite ships with it:
  a sandbox page that shows the pinned member through the framework's own read (a payload member set as an
  element's text), and a Playwright test that makes the real gesture and reads that text. It reruns on every
  Syncfusion upgrade, so a vendor that changes a shape fails the upgrade before an app meets it. The probe's own
  listener (`spec/probes/ej2-payloads.mjs`) is the spec's oracle, never the rewrite's proof.
- The owner, 2026-11-09, in plain words: "Write a TS custom component with all the complex features, framework has
  the ability to onboard and use that component to behavior test the framework where tests stay stable and catch
  real issues." It is BRIEF.md's native reference: a TypeScript component the framework writes as an app writes one,
  onboarded only through the core's public surface (its own vendor data, `alis:mount`), holding every conformance
  cell, and replacing the 16 partial components of today's framework test app. Framework behavior is proven on it
  once per cell, FusionMultiSelect proving that a real vendor's contract onboards the same way (the owner's choice
  over two custom components). The owner's examples, "promise in payload, or a async method 3 level deep in complex
  pay load", and every payload door the POCs proved are cells (BRIEF.md section 8). With the owner's
  yes, a shipped slice's proofs cover only what its declaration adds (its members' names and shapes, its browser
  facts, its own defects) and never re-prove a cell through another slice.
- The owner, 2026-11-10, confirming the reference component (verbatim in `spec/NOTES.md`): the framework is held stable at
  several levels, the analyzers as a harness and the browser tests. The browser tests run on one custom TypeScript
  component whose features are wide enough to cover the surface of every feature the framework supports: each
  variation at the sizes that show its rule (a payload walked one level, then two), with TLA+ carrying the rest, so no
  test is bound by a vendor's speed or limited by its capability. Syncfusion's onboarding is proven by one component,
  FusionMultiSelect, exercising the same kinds of features on its own members and what it brings of its own. The
  custom component's name says it covers the framework's features, never a domain word such as a ward: the owner took
  ConformanceComponent, the component the conformance matrix runs on, the same day.
- The owner, 2026-11-10, on the conformance app (verbatim in `spec/NOTES.md`): ConformanceComponent tests the surface of
  invoking a JavaScript object in all its variations, and the HTTP and partials lifecycles need more, so the browser
  tests run on one conformance app built from several components, some of them Syncfusion's, that covers 100% of the
  framework's features. It is a common-sense app, whose screens, hierarchy and behavior read as a real app's, its
  feedback loop fast and in a real browser, and the TLA+ specs' actions list the variations it exercises.
- The owner, 2026-11-10, yes to the care home app (verbatim in `spec/NOTES.md`), with the quality bar held on its test
  data: no test is false confidence, brittle or slow, since those breed one-off hacks and the quality falls, as on main.
  Before the rewrite, a plan for test quality, repeatability and shards, and the exact solution structure, its tools,
  the TLA+ specs and the folder hierarchy as vertical slices, with the 10,000-foot view, so the rewrite starts equipped.
- The owner, 2026-11-10: no relying on Syncfusion to cover the framework, the trap main fell into. A core module's
  browser test runs on ConformanceComponent, native inputs and the app-level objects, never through a Syncfusion
  component; a Syncfusion component on a care home screen is tested only by its own slice's tests, which prove what its
  declaration adds. The app's check refuses a core module's test whose screen part is a Fusion slice.
- The owner, 2026-11-10: two custom components show the framework onboards any vendor that follows its rules: a second
  joins ConformanceComponent with vendor data different in every part (where its object lives, how it subscribes and
  unsubscribes, what a listener receives), beside FusionMultiSelect, Syncfusion's real vendor. The tests are vertical
  slices too, each in its module's folder and named for its behavior, so the test tree never becomes the next fight.
- The owner, 2026-11-10: the TLA+ specs and every tool the spec built carry over to the rewrite, so a feature asked for a
  month later starts from what the repo holds, never from rediscovery. The spec is the repo's feature workflow: its
  living checks run in CI, its test specs are bound to the tests and its TLA+ actions to the harness's gates by name.
- The owner, 2026-11-10: publishing the NuGet packages, and how a developer uses them in an app (the framework's
  browser assets reaching both hosts), is part of the plan before the rewrite, at the same bar. The package ids stay the
  same, but for AlisReactive.FluentValidator's (below); Alis.Reactive.NativeTagHelpers ships as an empty package until it is written, after the rewrite, and so does
  Alis.Reactive.DesignSystem, since the key principle is the inverted dependency: nothing depends on a design system.
  Both were scope creep and need a redesign later, or leave: the structure reserves nothing for them, and their future is
  the owner's decision.
- The owner, 2026-11-10, asked what AlisReactive.FluentValidator ships, since the core implements the validation grammar
  without the FluentValidation library: "if not needed remove it that is better". The rewrite packs five of main's six
  ids. Of main's 113 FluentValidator doors on each target, 8 moved into AlisReactive and M8.F1 gives the other 105 their
  verdicts, 103 refused with their carriers and 2 kept (`spec/inventory/doors.tsv`). Main's `ReactiveValidator<T>` derives
  from FluentValidation's `AbstractValidator<T>`, so an app on main's release candidates drops the reference and moves
  each validator onto the core's `ReactiveValidator<T>`, whose `RuleFor` rules reach the browser in place of main's ClientRule calls.
- The owner, 2026-11-10, asked whether main's docs site (docs-site, deployed to GitHub Pages on each push to main, and
  tools/ApiDocGenerator, which writes its API page) survives the rewrite, after the redesign's line had dropped it:
  "Retire the site". The docs are README.md, packed as every package's readme, and the XML docs on the shipped surface;
  a line that says the docs carry something names the one that does, and `spec/REWRITE.md` names the unit that writes it.
- The owner, 2026-11-10, while the lead was restoring main's packages.config layout: "I'm not in favor of taking
  garbage from main. Like, do the right thing, keep the quality high, drop if you need to." Then: "quality pincipals
  must not degrade anywhere". This narrows 2026-11-05's "no feature lost" for main, whose lines are evidence of
  features and not of the design: what main ships stays when it holds the rewrite's bar, and leaves with its reason
  and evidence in `spec/REWRITE.md` section 3 when it does not; a door that leaves keeps its verdict in features_lost.
  Main's packages.config layout leaves.
- The lead, 2026-11-10, within gate 1's C8 and gate 2's G13: N20.2's default ships the OpenTelemetry adapter as one more
  id, AlisReactive.OpenTelemetry, beside the five of main's six that stay, since D20.2 keeps it out of the core, and the
  core's net48 references take System.Diagnostics.DiagnosticSource (D20.8). Every pin in `spec/REWRITE.md` section 2.7 is
  one exact version: the net48 references confirmed by a restore at those versions, the adapter's three npm packages by
  an install with every peer met.
- The owner, 2026-11-10: the Native components are great consumers and are written for real, with the dependency
  inverted: NativeToast, NativeDrawer (main's panel) and the common components (text box, check box, drop-down, radio
  group, check list, text area, button, hidden field, action link) are production components on the core's public
  surface only.
  The app-level components serve a legacy app's layout, where a drawer, toast or confirm appears in a target the layout
  holds, as main designed them; the rewrite keeps that meaning and decides its form from the facts. The Native
  components are written to the highest bar: a component-writing expert's review, and the strictest TypeScript lint
  the repo can hold, so the code is held to it.
- The owner, 2026-11-10: consumer packages stay separate, each kind of consumer its own package on the core alone and on
  no other consumer, since they differ (plain-DOM native elements against shadow-DOM web components). The line between
  coverage and consumers: the test consumers, ConformanceComponent and ConformanceElement, carry the framework's
  coverage in the conformance app and never ship; a shipped consumer's tests prove only what its declaration adds. A
  web components package is one more package id, the owner's decision after the rewrite.
- The lead, 2026-11-10, on the owner's "I let you decide now": `spec/REWRITE.md` holds the repository the rewrite
  builds (tree, slices, SliceTree, naming, harness, TLA+ owners, tools, shards, CI, packages, what retires) and its
  units, 26 then and 27 since the order of 2026-11-11 below. Two independent designs fed it; where they differed:
  vitest and PlanTests stayed as feedback, which the owner's decision of 2026-11-11 below replaces with a module's
  pure tests, no CSS ships, a fresh MVC 5 app proves delivery apart from the twin, only the Fusion pool gets the
  Syncfusion key, and every wait, budget and parallelism is measured before it is set. The units run in the order pages
  need doors after a walking skeleton, since a first-row unit has no page to prove it on; check.py places every feature
  in one unit and lands each test in the unit that brings the last door its screen parts need.
- The lead, 2026-11-10, within the owner's two components: a declaration carries one vendor, so the conformance
  library is six declarations, and between them they hold every kind of D17.3's closed sets: ConformanceComponent (a
  path from its element, a method pair), ConformancePeer (the component its payloads hand over, with an element path),
  ConformanceElement (the custom element that is its own object, ComponentVendor.Native), ConformanceRegistered (a
  plugin lookup, the pair on and off), ConformanceStore (a returned unsubscriber) and ConformanceCallbacks (callback
  properties, and a payload read by argument position). A test of a vendor kind runs on the declaration that holds it;
  the app check refuses a kind no placed test reaches, and counts no Syncfusion declaration toward it.
- The owner, 2026-11-09: "TLA+ ... help design this out because main badly failed at slow tests and broken ones
  that can not be fully relied". The lead's design within it: the TLC specs carry every ordering of the async lanes,
  so no browser test provokes a race by timing. A promise on an async lane names the one action of a spec's `Next`
  it observes (`Request.tla` Route, `Run.tla`, `Composition.tla`, `Ownership.tla`, `Lanes.tla`, `Realtime.tla`,
  `Tracing.tla`, `CallbackProperty.tla`), the test app's endpoint holds the machine at that action until the
  test releases it through the harness, a request of the test's own and never a control on the page under test,
  and every action is named by one promise: the browser proves the code refines each action once, and the browser test count is the actions plus the cells plus each slice's own surface. A timer the
  plan carries (a toast's time, a debounce, a retry's wait) runs on Playwright's clock (`Page.Clock`), never real
  time, and a collection the design's weak holds depend on is forced by Playwright's Page.RequestGCAsync, confirmed
  by the page's own weak hold before the test goes on (X12.18). An action that fires under no mutant-free run leaves its
  spec's `Next` (Tracing.tla's Finish and Send, Run.tla's ChainedSecond, 2026-11-10). Step 9 writes the actions
  into the promises; step 10's check.py refuses an action no promise names and a promise naming an action its spec lacks.
- The owner, 2026-11-10 (verbatim in `spec/NOTES.md`): the tests' quality checks live in a project of their own, apart
  from the code, since a test written beside its implementation mimics it and the quality falls slowly. The lead's
  design, `spec/REWRITE.md` section 2.10: a Playwright project references no assembly of the framework or the app,
  only the world contract, Alis.Reactive.Conformance.World, so a test finds an element as a user does and expects a
  literal; Alis.Reactive.TestRules holds the test rules TEST001 to TEST013, Roslyn analyzers with their cases, which
  with check.py's test_rules hold every rule and smell of `.claude/rules/tests.md`; a unit writes its tests from their
  promises before the doors they need, and U2 lands the rules before the first feature test. The evidence: 51 test
  files of the redesign find their inputs with IdGenerator.For, which rule 7 of tests.md forbade in prose.
- The owner, 2026-11-11, ending a session that lost the owner's trust (verbatim in `spec/NOTES.md`): "you have TLA plus.
  You have proved everything"; "no low quality and low confidence even medium stuff goes through"; an analyzer is
  written at the highest quality, "not a throw against the wall"; "this whole weeks of effort is to produce quality", so
  the spec's work is used "unless there is bug fix with the knowledge and quality bar being held"; "i can not repeat all
  the feedback i have given over the days"; the rewrite goes on "but not like this". That session put 4,307 of the 4,828
  lines it added into `spec/tools/check.py` and its plants, wrote three of U2's four harness tests against rules the
  same file states, and called a question the specs answer low confidence. So: the spec's work is used as it stands, and
  a change to it is a bug fix made with its knowledge and at its bar; an analyzer and a test rule are written to the
  highest bar, as 2026-11-06's decision states it; the owner's feedback is read from this section, `spec/NOTES.md` and
  the lead's memory, and is never asked for again. The lead's rules from it, each the owner's to change: a line enters a
  spec file or a test only after the rules its file states are read against it; a question the TLA+ specs, the
  principles or the owner's decisions answer is decided from them and held; `spec/tools/check.py` and its plants change
  only where a check reports wrongly on correct content or passes a defect met in content, each fix with a refused and
  an accepted plant, and a new check waits for the owner's yes; no agent decides or words anything, and a fork takes one
  bounded task and is landed or dropped in its session; a unit is committed on its own full check and its own rules
  test; progress is the open queue shrinking toward U1's cut.
- The owner, 2026-11-11, as the next session read the POC's analyzers (verbatim in `spec/NOTES.md`): "i never said use
  poc analyzer as verbatim, do you know how to write good analyzers?" The handoff had asked for authoring rules read
  "from what the POC's analyzers already do"; that clause is void. An analyzer, a suppressor and a test rule are written
  as .NET's own are, and `.claude/rules/analyzers.md` holds how, loaded where an analyzer or its cases are opened: one
  id for one line of the spec, tracked in release files the build compares with every descriptor; semantic reads only;
  cases on Microsoft.CodeAnalysis.Testing, a refused case for each message and an accepted case one fact away for each
  condition. The analyzers of main, of the redesign and of the POC are evidence of the rules and their cases, and the
  unit that lands a rule writes it to that file.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`): "use latest stable versions sir except where sf is pinned for a
  reason", and of the tools, "i m shocked that tooling quality was not addressed". Section 2.7's pins had been copied
  from what the tree held. So: a pin is the latest stable version on the day its unit lands it, looked up at its own
  registry and proven by a restore or an install and a run; a pin below the latest names what holds it in its row of
  `spec/REWRITE.md` section 2.7, and Syncfusion's hold is the owner's. The lead's holds, each the owner's to lift: the
  analyzers compile against Roslyn 4.14.0, Visual Studio 2022's last compiler, since a compiler below an analyzer's
  Roslyn refuses to load it; what runs in a host follows the net10 host's 10.0 line (the net48 references, the hub's
  client); TypeScript and Microsoft.AspNetCore.Mvc.Testing are held by a peer range and by net10.0. The SDK is
  11.0.100 and Node 26.13.0. Dependabot proposes every newer stable version, majors among them, and the nightly
  workflow compares the tools it does not read; a finding of either package audit fails the checks.
- The lead, 2026-11-11, within the owner's decision of 2026-11-06 that each lint rule enforces one line of the spec
  and the line names its rule: the lint beyond its two presets is the closed list of `spec/REWRITE.md` section 2.7
  (an exhaustive switch for S4, strict boolean conditions for S2, no `==`, an element found by its id, imports by the
  slice table), each named at its line and each proven by a refused and an accepted file; TypeScript runs with every
  strictness option on, erasableSyntaxOnly holding S8's no enum on the compiler's rung. The strict lint and options
  were run over the POC's runtime, which had never been linted: one compiler finding and 22 lint findings in 2,241
  lines. A selector for `instanceof`, a catch or `??` is not on the list, since that runtime holds each by design;
  one joins only with the spec line that makes it exact. The owner can overrule each.
- The lead, 2026-11-11, within the owner's decision of 2026-11-10 on the test rules: TEST002, TEST004, TEST005, TEST006
  and TEST013 are stated over closed lists of what a test takes (`spec/REWRITE.md` section 2.10), never over lists of
  what it must not: a refused list misses every member it does not name, and Playwright 1.64.1 holds members the old rows
  did not name that run script in the page, raise an event no gesture raised, read the page once or match a snapshot. The lists are
  read from the gestures and outcomes of the spec's own tests and from the pinned packages, each name compiled
  against them; `Assert.That` reads what the browser reports beside the page, the console's errors among it, which
  those tests read and TEST006 had refused. The owner can overrule each list.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`): "put these in project local claude.md as per best practice,
  tighten the quality repeatable loop". A rule for one kind of file lives in a path-scoped file under `.claude/rules`,
  loaded where it applies (`.claude/rules/analyzers.md`); what every unit keeps is root CLAUDE.md's, which a session
  re-reads after a compaction: the pins rule, and one loop of seven steps in its Spec-first section, each exit a
  line a tool printed. The loop replaces Workflow's five states and holds what a prompt had to repeat: read before
  writing, decide from the proof in hand, one meaning with every carrier, a rule proven by a refused and an accepted
  case, the full check at the committed head before the push, a slip said in one line. The cut rewrites that file
  within Claude Code's 200-line guide; until then it holds the redesign's sections too.
- The lead, 2026-11-11, within the owner's decisions of 2026-11-06 (an analyzer is written as .NET's own are and
  enforces one line of the spec) and of 2026-11-08 (a rule the source states is the compiler's): ALIS009's conditions
  are M20.D7's. `dynamic` is refused as `object` is, and a delegate's and an expression's type arguments are read. A
  string is read by the words of its parameter's name, as .NET's CA1054 reads a URI parameter's, over a closed list of
  sixteen words that holds every name both lines matched whole and "param", from main's doors that key a request by a
  string. The build reads the name and never what the string means, so a member-naming string under another name
  passes it and is read in the package's PublicAPI text. The exemption is
  `Alis.Reactive.Declarations.TypedDslExemptionAttribute`, on the row below Vendors and Plugins. The rule's reach to a
  declaration and a render helper in a compilation with no slice table lands in U13, with the types it reads. The owner
  can overrule each.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`): "With regard to merge, rewrite wins. I mean, I don't understand
  the confusion here now." The lead had ended a turn before section 2.7's runner, since it touches the path the POC's
  suites run through, and had read the conformance project as keeping the POC's descriptor until `poc/` leaves. So:
  where the rewrite's design and an older line meet, the POC's runner and scripts today and main at the pull request
  that lands the rewrite, the rewrite's piece is written as its spec states it and the older line gives way. The POC,
  main and the redesign stay evidence; nothing of the rewrite is shaped to keep one of them running, and the lead
  stops for none of them. That "merge" is every place the lines meet is the lead's reading, the owner's to correct.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`): "The schema is done. Only bugs will be addressed if found." The
  rewrite runs outside in: "once the CSHTML is there, all the open doors as per agreed schema, then you can write
  tests, the playwright ones. So those tests would fail, and then you can keep bringing one module at a time in the
  mix", a piece tested alone against the schema, and "when all the scenes or Lego pieces comes together, eventually
  the playwright test will pass". To the lead's three conditions on it: "why would u think otherwise? why we did all
  the TLA+ work for ?" The TLA+ specs fixed every ordering and named every step so that a test is written from the
  spec before the code, and the lead attaches no condition. The order: the schema's nodes and generated contract with
  the public surface as signatures (`spec/api` compiles it); every screen's view; every Playwright test from its
  promise, failing; then one module at a time in the slice table's order, Schema first, each half tested alone
  against the schema; a module is done when its tests pass and each fails on its planted defect. `spec/REWRITE.md`
  section 4 states the order as the decision of the same day below settles it. TEST013 is built to section 2.10's list, which counts a navigation as a gesture, and
  section 2.5's bindings check is U2's, by the owner's decision of 2026-11-10.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`), on the lead's word for a test written ahead of its code: "Test
  is not a draft, its failing test that actually proves behavior when turned green. Draft is hedge word to drift and
  write sloppy code so u can argue later because of it was draft." A test is written once, at the full bar of
  `.claude/rules/tests.md`: it tests behavior and never internals, is named for its promise inside the test
  architecture of `spec/REWRITE.md` sections 2.4 and 2.5, and runs by one repeatable command. It fails until its code
  lands and proves the behavior when it turns green. "TLA+ is central part of the rewrite not a throw away": a held
  step of a test names the action of its spec, and the specs stay the design's proof through every unit. Before the
  tests are written, the Playwright project's own CLAUDE.md holds bad tests of main and the redesign as examples,
  each beside the rule it breaks: "You can get plenty of bad tests examples to put it in test project claude.md for
  the test quality."
- The owner, 2026-11-11 (five messages, verbatim in `spec/NOTES.md`): the rewrite states its standards so that each
  question has one answer in one place. For each kind of test: what it is, what it proves, what it prevents, its
  quality standard, the one repeatable command that runs it and when it applies. The kinds are the architecture tests,
  the Playwright tests with their project's own enforced standard, and "a module level pure tests for public surface";
  "Tests will never ever test implementation or internals (intenal exposure is forbidden)." The TypeScript runtime
  answers the same: its standards, its lint rules, its pure tests, its folder's own rule file. "every project is
  vertically sliced namespaced logically." "TLA+ proved it, it is central governing pience of re-write." Root
  CLAUDE.md says how it governs encapsulation, TLA+, the map and the doors, and how each bad practice is stopped; the
  tree's view from above shows the architecture on one screen; each standard says how drift is stopped and how the
  confidence of a piece of work is determined. "now all these things are not meant to cause paranoie and waste and
  un-necessery waits and fear based engineering": a standard is held by the tool on its rung, a piece of work is
  proven once on the rung that holds it, and no guard is added for a defect nobody met. A module's pure tests take
  the place the plan gave PlanTests and vitest as feedback. The plan's first page, `spec/REWRITE.md` section 1, root
  CLAUDE.md and each project's own rule file state it. The owner's order for it, the same hour: "Please write a structured list of every aspect of
  re-write, with project, quality enforcement, session handof when and why, how to begin..how to reorient", and
  "Answer every single question with certainity and have the setup in place, test the whole govervance of re-write
  then only talk to me. You are 100% allowed to disagree with me when you have a genuine argument". So the lead
  answers each question in the tree, puts each standard's enforcement in place, proves each enforcement by a refused
  and an accepted case, and reports to the owner once, when all of it stands; a disagreement is stated with its
  argument where it arises.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`): governance is closed in a session of its own. "Prepare a
  handoff for a dedicated session where the govervance become a closed topic. Governance must establish feedback look
  as in flip notes, u corrected forks mistakes", each "linked to claude. so that we are not going in circles". So
  every correction has one home where the work happens: a reversal of the lead's ("flips must be linked to
  claude.mds...as in if flip apply to tests then there..."), a fork's or an agent's mistake the lead corrected, a
  slip of a session and an owner's correction each become a rule line in the rule file that owns the subject, and
  the record of it names that file. The framework's projects hold one simple rule for code that compiles for net48
  and net10.0 in their own rule file ("a simple rule will solve that for ever in local project"). When and where a
  fork is used is stated once, since "all of that is known at this point after extensive sessions, pocs, tla+ work".
  A gap is closed and proven, or it is open: "do not read closing the gaps as if you can or use this an execure as
  best effort. Prove your goverance work". The delivery loop is this Mac's: "you will use local mac for faster
  delivery loop, once rewrite is done CI can pass its results, no need to hedge and buy time into that"; a run of CI
  and a number measured there gate no unit. A fork's result is proven by the command its task names, run once where
  it lands, and of a check that costs what the work costs: "if you going to spend same amount of time checking then
  do it yourself".
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`), after the lead argued the order of the rewrite: "ok i concede my
  position." The order after U2: U3 lands the seam and the surface whole and makes one thin path real (a plan
  renders, the runtime boots, a trigger runs a pipeline that sets an element's text); then one module a unit in the
  slice table's order, each unit owning its module's two halves and its pure tests, its browser tests turning green
  and failing on their planted defects in that unit; then Native; then Fusion. The argument that decided it: every
  door that renders a plan lives in the last core row, so in a strict row-by-row order no framework browser test
  passes before that row; the runtime's code that touches the DOM has no proof but Playwright, since its pure tests
  run in node with no DOM; and a module's done, its tests green and each failing on its planted defect, is then
  checkable only once, at the end, for everything. This is the order the outside-in decision above runs in, and
  `spec/REWRITE.md` section 4 states it with its units.
- The owner, 2026-11-11 (verbatim in `spec/NOTES.md`), on the governance session's cold read and on every review:
  "don't make it so that every review or whatever the strategy you are using keep bringing issues and becomes an
  infinite loop, and then it drift, and then it becomes its own work." A review is bounded before it starts: a
  fixed list of questions, each with the answer that passes. A finding is an answer that fails, and nothing else. A
  failed answer is fixed in the page it was read from and asked once more; after that second reading the review
  ends, and what still fails goes to the owner in one line. What a reviewer raises outside its list is not worked:
  it is one line to the owner. The governance session's work list closes when its inventory is written; an item
  joins it after that only for a defect met while doing a listed item. The one round is no net for the writer: "tell
  the first agent, either you or fork whoever writes that don't throw over the wall to one round we have, because
  that is all bad behaviors that that agent will check and I will fix. I expect quality work on the first run."
  Whoever writes a page or a piece of code, the lead or a fork, finishes it to the full bar before it is read.
- The lead, 2026-11-11, within the decision above on the closed lists of the test rules: the harness's list takes
  four more members of Playwright 1.64.1, which section 2.5's measured waits need and the list had left out.
  `Assertions.SetDefaultExpectTimeout`, since an expectation's wait is a wait, and every wait is the harness's, set
  from measurement; and a context's `Tracing` with its `StartAsync` and `StopAsync`, since Playwright's .NET surface
  gives a call's start and end nowhere but in its trace, and the waits are four times the slowest step. A test
  takes none of the four. Beside the same measurement, the lead's reading of section 2.8's parallelism: the count of
  tests at once is measured apart from the budget, since four settings of 20 runs each cost four measurements, and
  the budget is measured at the count the parallelism measurement kept. The owner can overrule each.
- The lead, 2026-11-11, landing the grammar (U3) within D19.1 to D19.9 and the owner's decision of the same day
  that the schema is done and only its bugs are addressed. The grammar's classes are `Alis.Reactive/Schema`'s, the
  module table's name, where N19.1 and N19.2 name the redesign's `Grammar`. A kind's class is its kind and then its
  union (`SetStatement`, `LitExpression`), which the contract generator holds. A path that takes no `each` is a list
  of `PathStep`, a member or an index, and a registration's path is a list of `RegistrationStep`, which adds `each`:
  one type for both would give `each` a second handler in every path walk (S4, S5). Two gaps met in schema.md are
  the owner's to settle, each built at the reading that adds no kind and no null: a `lit`'s value is the JSON C#
  wrote, since schema.md's `includes` writes the view's list as a `lit` where the `lit` row says a scalar or null;
  and a container's `live`, "none, or the `Live`", is a list of at most one `Live` (S2). The owner can overrule each.
- The lead, 2026-11-11, within the owner's order of the same day (the surface as signatures, then one module a
  unit) and the owner's decision of 2026-11-06 (fail loudly, no fallback): a door U3 declares ahead of its module's
  unit throws `NotImplementedException` until that unit builds it. An inert door, one that renders its page and
  writes no plan, is a default no line states. Its cost: a screen whose doors are not built answers 500, so the
  gate's sweep of the app states that case when U3 writes the views. A signature's proof is the doc ids of the
  built core compared with the ids the fluent probe prints for `spec/api`, namespaces mapped by REWRITE 2.2. The
  owner can overrule it.
- The owner, 2026-11-12 (verbatim in `spec/NOTES.md`): "keep the log of mistaks sir, and make sure u have a way a to
  continusuly improve as we progress". So every mistake is an entry of `spec/MISTAKES.md`, written the turn it is
  found: what was written or done, where it was caught, what it cost, its cause as a class, and what holds the
  class. The way the work improves is the class: a class met a second time is held too low, so in that unit it moves
  up a rung, from a rule line to a step whose result a commit shows, from that step to a tool with its cases, and a
  rung that needs the owner's yes goes to the owner in one line with its cost. A piece begins by reading the
  classes, and a handoff reports the entries since the last one by class. It replaces the line of root CLAUDE.md
  that let a correction a rule already stated add nothing: two mistakes of 2026-11-11 and 2026-11-12 had one cause,
  and that line recorded the second as no news.
- The owner, 2026-11-12 (verbatim in `spec/NOTES.md`), when the lead asked for the C# form of the plan's block: "there
  were more than 100+ POCs done hows that possible - if its miss, its bug", and of a `using` block, "using will nest
  the whole html". Each of the 266 views of the POC's app that open a plan, and the one of `spec/api`, opens it with
  `Html.ReactivePlan<TModel>()` and writes it with `Html.RenderPlan(plan)` as its last line, its markup flat. D14.7 and N14.2 had replaced that by a block no POC
  built: the miss is the spec's bug, fixed in every line that carried it. The owner chose how a view that forgets
  `RenderPlan` is caught: one analyzer rule, ALIS041 (N18.15), which refuses at build a plan a view opens and never
  renders, landed with the thin path. The lead's line within that choice, the owner's to overrule: the same rule
  refuses a plan rendered twice and a door that takes the plan after it rendered, the two defects the block was
  written for (M9.X1). A design line that no POC and no compiled sentence carries is a bug, raised as one with the
  search that found it, and never a form to invent.
- The lead, 2026-11-12, building the thin path's load trigger within gate 2's G13 and the owner's decision of the
  same day that a design line no POC and no compiled sentence carries is a bug, never a form to invent. Two lines
  were missing. How the plan carries on load: no line stated it and the POC has no on load, so it is as the
  redesign writes it at the State SHA, which `spec/Core/schema.md` is a diff against and changes in nothing here: a
  handler of the event `ready` on the document plugin's object, `$document` (M1.D1 and schema.md state it now, with
  the redesign's lines). The name of the type M1.F1's sentence hands its `trigger`: the sentence fixes the two
  doors, `Html.On` and `DomReady`, and names no type; it is `Trigger<TModel>`, as both lines' pipeline builder is
  `Pipeline<TModel>` in every door `spec/api` compiles. The owner can overrule each.
- The lead, 2026-11-12, writing the runtime's dispatch within the owner's order of 2026-11-11 and the decision of
  the same day on a door that is not built. The plan's words are "the runtime's dispatch, a case for every kind".
  The lint refuses a switch that lacks a kind, so a switch is whole from its first line: the evaluator's two
  switches, over the statements and the expressions, are written whole, and a kind whose module's unit has not
  built it throws there, as a door does. A nested union's switch (the shapes, the absences, the zones, the trace, a
  listen's release) is written whole by the unit that builds the handler which reaches it: before that it would be
  a function nothing calls. The object locators', a vendor's listening's and a handler's policy's are written,
  since the thin path reaches them. The owner can overrule it.
- The lead, 2026-11-12, within the decision of 2026-11-11 on the test rules' closed lists: TEST004's list for the
  harness takes one more member, an attribute that is a test action of NUnit's, applied from a file of
  `tests/Alis.Reactive.Conformance.PlaywrightTests/Harness`. The list named a setup fixture, which NUnit runs for its own namespace alone, and ALIS011 keeps a
  harness file in the harness's namespace, so the first test of a module's namespace found no browser open: the
  harness's own tests, U2's, all sit in its namespace. A test action on the assembly opens the run once for every
  fixture and closes it at the assembly's end. Outside the harness the rule refuses it as it refuses a setup
  attribute. The owner can overrule it.
- The lead, 2026-11-12, stating the next cases of WhenAnAppRendersItsPages within the decision of 2026-11-11 on a
  door that is not built, which sent a page that answers 500 for one to the sweep. The plan names the cases and no
  line says how the sweep reads each. A page whose door is not built is listed apart from a problem, until U27
  builds the last door; the page's browser test stays red meanwhile. A route's parameter takes its value from a
  link a page holds, and a route no page links to is refused as unopened. A page of the refusals area that
  answers 500 with an `InvalidOperationException` is refused by design, and the same answer on any other page is
  a problem. The sweep's own host hears what a page threw; nothing of the app changes. The owner can overrule each.
- The owner, 2026-11-12 (verbatim in `spec/NOTES.md`), after the lead had put ALIS041's reach on MVC 5 to the owner
  three times: "I think it's a non-issue at this point", "I am not understanding why you are fussing over it. The
  MVC5 is compiled the way the documentation says." On MVC 5 ASP.NET compiles a view where it first renders, so no
  analyzer reads it at build. A view there that never renders its plan shows a page whose handlers do nothing, the
  first time its developer opens it, as on both older lines today; README says so with the net48 delivery, and it
  is no work of the rewrite. The lead's rule from it: a limit that shows itself the first time a developer opens
  the page, and that both older lines share, is stated once, in the notes and in the unit that documents it, and
  is no item for the owner.
- The lead, 2026-11-12, on the clause REWRITE 2.5 had U3 add to the bindings check, a view's module namespaces held
  to its test's feature and parts: it is left out. As worded it refuses the view of 430 of the 452 placed tests,
  since every view that opens a plan names the two namespaces P2 and P3 fill with every module's doors, and
  app.json names no view for a test, so it reads a screen and one test's part clears every view. A test landed
  before a door its page calls fails its unit's run, since a door that is not built throws, and the cure is a part
  in its placement. It was put to the owner as one line and not taken up, and it is no principle, closed rule or
  scope: a guard no view needs yet. The exact form, a page named by each placement and a reader of the doors each
  view calls, is a new check and waits for the owner's yes. The owner can overrule it.
- The owner, 2026-11-12 (verbatim in `spec/NOTES.md`), after the lead had called the conformance app many sessions
  of work: "My expectation is that TLA plus proves the schema, POCs, main, everything is in place, and you sort of
  taking hell of a time." Then: "Quality has to stay. Quality comes from action, not too much thinking. Too much
  thinking just slows you down. That is what fear-based behavior is." The design is in place and what is left is
  execution: a line is read, the code is written and its proof is run. No status counts sessions or calls the size
  of the work a problem, and a question the lines answer is not turned over twice. The gate, the proofs and the
  commit's discipline stay as they are.
- The lead, 2026-11-12, where two lines of the plan meet: U3 writes every screen's view, and U13's paragraph lands
  the conformance library's declarations with the doors that reach them. A view that holds a component does not
  compile without its declaration, so U3 writes each declaration's C#, member for member from
  `spec/Reference/native.json`, with the app's data classes and enums its members name, and U13 writes its
  TypeScript, builds the doors and turns its cells green. A payload is a class of its own beside its component,
  since native.json names it as its event is named and C# refuses a nested type of a member's name; a type one
  component reaches sits in that component's folder, and one several reach sits in
  `tests/Alis.Reactive.Conformance.App/Components`. `Field` and
  `Field<TValue>`, M11.F1's types, are declared in `Declarations` with `Field.Of`; the redesign's public `Name` is
  not, since M11.X2 gives the name to the app's options, known where the view renders. The owner can overrule each.
- The lead, 2026-11-12, declaring the app event family, which `spec/api` compiles no sentence with, from M1.N2 to
  M1.N5 and M1.F2's proposed sentences: `AppEvent.Declare()` and `AppEvent.Declare<TPayload>()`, each with the
  `name:` its line states and no constraint on the payload's class, since no line states one;
  `trigger.CustomEvent`, its payload a `Data<TPayload>` as M1.N3 words it, a class of `Declarations` with `Read`
  (N5.6, D5.2); `pipeline.Dispatch` with a literal or a source; and `Html.Raises`, which returns what a view
  writes inside a tag on each host. `pipeline.Object<T>` (N7.1) waits for the first view that builds a payload
  from sources. A door a module's page names and `spec/api` does not compile is declared the same way, from its
  line and its sentence, with the first view that calls it. The owner can overrule each.
- The lead, 2026-11-12, declaring the door five bench cells, M5.F1's sentences and X3.8's page call, an element's
  text set from a source that is no text: `Text` and `Html` are each an `ElementContent`, a class of `Elements`
  that reads as a source of text and whose `Set` takes a text literal or a source of any type. M5.N1 named each a
  string property "taking a literal or any source of D7.2", and a string property takes a source of text alone
  (D7.3), so its own sentence did not compile; both older lines take a source of any type there, and an overload
  on the string property would reach every component's. D7.3 and M5.N1 state it. A literal that is no text at an
  element's text, X3.13's and X3.14's dates, is declared with the first of their pages. The same commit gives
  the bench its component's builds: a world names one (`ComponentVersion` of the world contract, the versions
  `spec/Reference/native.json` lists) and the page starts its component as that build. The owner can overrule each.
- The lead, 2026-11-12, on the three callback cells of event.poc15, whose promises set an answer that joined an
  item's name and a literal: no door joins text, the grammar's operators hold no join, and no line names the
  capability, so the sentence could not be written. The promises are reworded to an answer that reads a source
  alone, `item => item.Name`, as every other answer sentence of the spec does (D17.12), and the lines they read
  are restated to match: the held line reads "Held: acted for One". A door that joins text is a new production
  and waits for the owner's word; nothing is built toward it. The owner can overrule the rewording.
- The lead, 2026-11-12, on three source cells, source.PropertySet, source.WriteOnlySet and source.MethodArgument,
  whose promises set a text without null from the component's Value, a nullable text: the compiler refuses the
  sentence (CS8620, which D7.3 and N18.14 keep an error), and D7.3 names the cure, so each promise reads the
  value through `IfNull("none")`. The position still receives the component's member. The owner can overrule it.
- The lead, 2026-11-12, on four promises of the bench's requests that stated what the design does not: three
  wrote a query's key in lower case, where D2.5 makes a key the member's C# name, so each reads `?Code=` and
  `&Day=`; and http.whileLoading's opposite stopped the request "in its guard" with the heading unchanged, where
  D10.23 runs a request's WhileLoading before its guard, so the opposite is a request whose argument stops when
  it is read. The owner can overrule each.
- The lead, 2026-11-12, declaring `pipeline.Object<T>` (N7.1), which `spec/api` compiles no sentence with, from
  its line, D7.4 and M1.F2's proposed sentence, with source.DispatchPayloadMember's view: the door is the
  pipeline's and returns a source of the object; its members are an `ObjectMembers<T>`, a class of
  `ValuesAndSources`, whose `Set` takes a literal or a source and returns the members, the three forms the JSON
  body's `Set` has. Its callback's parameter is the app's word for what it holds. The owner can overrule it.
- The lead, 2026-11-12, where two more lines of the plan meet: U3 writes every screen's view, and U23 writes the
  Native components. A view that holds a Native component does not compile without its declaration and its
  render helper, so U3 declares each slice with the first view that holds it: the declaration member for member
  from its line of `spec/NativeComponents/module.md` and its feature's proposed sentences, the render helper as
  a signature that throws until U23 builds it, as a core door does. `NativeTextBox` is the first (M24.N1, M24.N9),
  with the core's doors it stands on: `IInputComponent` and `ComponentInput.Of` (M7.N3) and the field's options,
  `InputFieldOptions` (M7.N2), in `InputsAndIds`, placed by P1; a field's `Render<TComponent>` takes an input
  only (M7.X2). The text box's `Type` is not declared: N18.4 sent its form to gate 2, its line writes none and
  neither the POC nor `spec/api` compiles one, so by G13 it comes to the owner with a form before U23. The owner
  can overrule each.
- The lead, 2026-11-12, declaring M3.N1's text tests with the first views that call them: `Contains`,
  `StartsWith` and `EndsWith`, each with a literal or a source (N7.3), `Matches` with a literal pattern alone
  (D7.5), and `MinLength`, each an extension on `ITypedSource<string?>`, so a text without null is tested as it
  is, as a set takes one (D7.3 states it). The value tests and the membership tests are declared with the first view that
  calls each. The owner can overrule it.
- The lead, 2026-11-12, on the cell source.MatchesPattern, whose promise read the pattern from the component:
  D7.5 gives `Matches` a literal pattern alone and X7.4 puts a source there on the type rung, so no sentence
  writes it. The cell is refused by the type, as the matrix's two cells ALIS032 refuses are refused at build: it
  keeps its row of `spec/Reference/matrix.json` with D7.5 and X7.4, and holds no promise, no placement and no
  browser test. The text `Matches` tests is a condition's left operand, which source.ConditionLeft proves. The
  bench places ninety-eight cells. The owner can overrule it.
- The lead, 2026-11-12, on the C# form of a view's request container, which D14.8 states as a block and N14.3
  sent to gate 2. No line writes it, and no sentence of `spec/api`, of a module's features, of the POC or of the
  redesign compiles it: the handles are parameters in `spec/api`, and the POC's nine forms are MVC's own
  BeginForm helper, none sent by a pipeline. By G13 the form is the owner's before the unit that builds it. Seven
  bench cells and every care screen's form are views U3 writes, so U3 declares the lead's proposal as signatures
  and puts it to the owner in the handoff: `Html.BeginRequestForm(plan, containerId)` and
  `Html.BeginRequestContainer(plan, containerId)`, doors of `RenderingAndDelivery` on each host, each returning a
  block a view ends by `using`, as it ends MVC's own BeginForm helper: `RequestFormBlock<TModel>`, whose `Form` is
  the `RequestForm<TModel>`, and `RequestContainerBlock<TModel>`, whose `Container` is the
  `RequestContainer<TModel>`. A pipeline that sends the form is written inside the block, and another view
  reaches the container by `pipeline.Container<TModel>(containerId)`. The live check a block names (D16.15) is
  declared with the first view that names one. The cost of another form: one line of each view that holds a
  container. The owner decides it.
- The lead, 2026-11-12, on the cell http.beside.hidden, whose promise read the form data as it leaves the page:
  `Form(form)` on a request with a body sends the submission set as multipart, whose text holds a boundary the
  browser makes, so no test reads it whole as a literal. The promise reads what the endpoint bound, which the
  page shows: the test app's batch notes endpoint answers the batch and the code, and shown reads
  "B-2026-11-10: a-1". The conformance component is an input by `spec/Reference/native.json`'s `input`, so its
  declaration is an `IInputComponent`, and a second render helper renders it for a field. The owner can overrule
  it.
- The lead, 2026-11-12, settling what three forks read into while writing twenty refusal pages, each form fixed
  by its test. A message is its X line's text with each placeholder filled as the view writes the thing: `{term}`
  is the lambda's body ("d.Seal == seal"), `{member}` and `{read}` a lambda's path ("d.Seal", "owed.Due"),
  `{value}` the view's own expression ("Model.Cover.Name"), `{type}` the C# keyword ("decimal"), `{view}` the
  view's path from the app's root, unquoted, and `{cause}` and `{base}` the words D6.12 lists ("the app's options"
  for a list the server wrote). A list a refusal page's lambda reads is a response body of the page's own
  endpoint, so its base is the app's. X6.48's sentence is written `(int)none!`, since CS8629 stays on a value the
  view holds (D6.14). X14.5's promise named its model in the redesign's test app; it names the conformance
  app's namespace now, and five more promises of module 7 that name that app are corrected as their tests are
  written. An endpoint a test names is a constant of the world contract (`RefusalEndpoints`, a part a module); one
  only its page names is a constant beside its controller. M1.X8's and M1.X9's messages are the redesign's whole
  text, as their lines say, and name `p.Dispatch`, where the rewrite's locked parameter name is `pipeline`: the
  wording is the owner's, put in the handoff. The owner can overrule each.
- The lead, 2026-11-12, landing the validator grammar Q16's Doors keep as signatures of `Alis.Reactive/Validation`,
  a fork's work read at the source, each door throwing until U17, with `AddReactiveFluentValidation` in
  `RenderingAndDelivery` by P3. The redesign's source at 65fdf118d is the evidence of each signature, and where
  its docs and a line of Q16 differ the docs follow the line (`Matches`, `IsInEnum`, `PrecisionScale`). Four
  readings, each the owner's to overrule. The kept list's check `Add` names no door of either line's public
  surface, so nothing is declared for it. `Matches` over a `Regex` is not declared, since the core bans the type
  and the pattern's text carries it (D16.11). `RuleLevelCascadeMode`, a door of the redesign the inventory
  holds and the list does not name, is declared, so no door is lost; `ValidationFailure`'s `ToString` waits for
  U17, since a text that throws serves no reader. The untyped `ReactiveValidator` keeps a constructor no app
  reaches, as both older lines have it, so RS0022 is suppressed on the typed validator's constructor with its
  reason: the first suppression in the rewrite's shipped code, which the owner approves or replaces by an open
  constructor or by no untyped base.
- The lead, 2026-11-12, landing seven more refusal pages, a fork's work, with one correction of the conformance
  library. X4.13 refuses a path past a member its own converter writes, "`[JsonConverter]` on the member", and D4.2
  gives a converter on the member's type another refusal; the library carried `ConvertedSpot`'s converter on the
  type, so X4.13's pages would have met the other message. `Binding`'s `Converted` carries the converter now, and
  `spec/Reference/native.json` says so. The forms the fork read into stand as it wrote them: X8.22's line holds
  no placeholder, so its message names no read; where two lists name one member, each lambda's parameter is the
  page's word for its item ("charted.Status"); the page of X4.13's and X18.8's opposite case reads the same
  shape from a response body of its own endpoint. The owner can overrule each.
- The lead, 2026-11-12, landing the seven remaining component slices of module 24 as signatures, a fork's work
  read at the source: `NativeButton`, `NativeCheckBox`, `NativeCheckList`, `NativeDropDown`, `NativeHiddenField`,
  `NativeRadioGroup` and `NativeTextArea`, each from its line of `spec/NativeComponents/module.md`, to the text
  box's exemplar. They are declared ahead of their first views, since the validation cells, the refusal pages
  that wait and every care screen hold one. A member whose parameters no line states takes the form both older
  lines share, a parameter named as the text box and the core name theirs. Three things are left out, each
  reported, none invented: `Type` of the button and of the text box (N18.4, the owner's form); the three methods
  of the click's payload, whose declared form no line gives; and `[Mirrors]` on the check list, whose element
  type no line gives a TypeScript module. The click payload's numbers are `int` where the DOM
  standards type a long and `double` elsewhere, lib.dom's `number`, to be held by the contract check in U23. The
  owner can overrule each.
- The lead, 2026-11-12, landing twenty-six care pages, three forks' work read at the source, and settling what
  they read into. The net10 host's JSON options are `spec/Reference/app.json`'s, camel-case members and enum
  names, which `tests/Alis.Reactive.Conformance.App/Program.cs` now sets: it is ASP.NET's own configuration and needs
  no door. The lead
  had told the forks the host sets no JSON options, from the app's code and not from that definition, so four
  promises that need enum names stood unwritten (X6.12, X6.16, X6.26, X6.28); they are pages to write now. The
  rest of that host's definition (Fusion, the busy and reconnect components, the request defaults, the flags
  enums, a request culture) lands with the doors and the pages that need each. A data lambda that reads a
  member past one that holds null is written with `!` until D6.14's suppressor is built, and loses the mark
  then. X6.32's first item of an order is written `Find(d => true, d => d.Name)`: no door reads a list's first
  item, a gap raised with that use. X7.6's fourth promise named a text and a number as `Choose`'s two arms, which
  the type refuses; it names 1000 and `Total()` now, shown before the unit. The conformance component's start
  gains `Raises` and a text that holds null, as its behaviors state. The owner can overrule each.
- The lead, 2026-11-12, landing sixteen more pages, three forks' work, and settling what they read into. A
  request that sends the member an input binds is written `request.Input(r => r.Member, pipeline.Input(m =>
  m.Member))` (N9.3): X9.20's picker is a `NativeDropDown` bound to the page's enum, since a text source at an
  enum member does not compile. A promise that says the network holds no request is read by the element the
  answer would fill keeping what it showed, beside the opposite case that fills it, until the harness holds a
  page's requests for a test to read. A route that takes a parameter is linked from a plain page of its screen,
  so the sweep of the app's pages opens it. A page reaches another origin as the same host under its other
  loopback name. A seed is added where a test changes what an endpoint does while its page is open
  (`Seed.TheBedsEndpointDropsItsConnection`). The container block names no element kind, so a promise's fieldset
  is the block's element with a heading inside; the kind is part of the form the owner decides. One fact for
  U11: which `Contains` C# 14 binds for an array inside an expression lambda is not read yet, and the identity
  table keys what the tree carries. The owner can overrule each.
- The lead, 2026-11-12, landing thirty-six more pages, three forks' work, and settling what they read into. A
  promise that says the console holds one error whose words no line states is read by the count of the
  console's errors, one, beside its visible outcome; its words join the assertion when a line states them. A
  container's inputs sent alone as JSON, and a form's sent URL-encoded, are written with an empty body,
  `request.Json(container, body => { })`: no door sends either with nothing beside it, a gap raised with these
  two uses. X17.22's error line is read as holding a message; its words are fixed by its test when U13 builds
  the component. The four other conformance declarations have a render helper each, and the component's
  start classes hold what the pages start them with (`RaiseStart`, `RecordStart`, `ItemStart`, `ElementStart`,
  `RegisteredStart`); what each of those components shows (the statuses Threshold, Alarm and Subscribers, the
  lists Log and Names, an answer line of several answers as its items) is the forks' reading of
  `spec/Reference/native.json`, to be stated among its behaviors when U13 writes their TypeScript. A bench page's
  own plugin sits beside the page that calls it (`PageTally`, `BenchClock`). The owner can overrule each.
- The lead, 2026-11-12, landing forty-five more pages, three forks' work, and settling what they read into. The
  conformance component's start holds what each page starts it with, a member added with the first page that
  needs it and written by the render helper as the plan writes a literal (records, items that lack a declared
  member, a nullable list that holds null, bindings, raise buttons); two forks that add one member are joined
  by hand. The world's record endpoint reaches the settle gate, and four seeds change what the bench's
  component holds. A bench page whose plugins are scripts names them through its own layout,
  `tests/Alis.Reactive.Conformance.App/Bench/Plugins/_ScriptedLayout.cshtml`, which links the files U13 writes. A
  null literal at a `Set` or an argument is ambiguous to the compiler, so a view passes a null it holds as a
  typed value. Three gaps are raised, none worked around: the result of a list operation, an
  `IReadOnlyList<T>`, does not fit a property declared `List<T>` (M4.X3's sentence); M4.X2's promise needs a
  record whose code holds null, which `spec/Reference/native.json` types a text; and no door writes a list as an
  element's rows (X4.9). The owner can overrule each.
- The lead, 2026-11-12, writing the six validation cells of the bench, read against D16.2, D16.13, D13.6 and G13.
  The forms of a field's message and of a form's summary are MVC's own pattern, as D16.13 writes them (a summary
  that lists the property errors, a message for each field), approved at that by G13; a test reads each in its own
  scope, as the Playwright project's file says, and a page groups each field it checks in a fieldset or a region of
  its own.
  No door names a rule set (D16.2 sends the sentence to gate 2; neither spec/api nor the POC compiles one), so the
  plain rule and the rule under `When` are two validators of one form, each named by the request's
  `Validate<TValidator>()`; whether U17 lets two registered validators of one model stand beside the condensed
  default is U17's question, raised here. The validators are written and not registered, since the startup call
  throws until U17; the form endpoints answer what they bound, since N16.1's door is not declared. The owner can
  overrule each.
- The lead, 2026-11-12, landing twenty-five more pages, three forks' work, and settling what they read into. A care
  page's plugins are the test app's, declared on the core's doors beside the page (a bell, a tally, a radio whose
  unsubscribe throws, each script written by U13), and a care screen whose pages name scripts keeps its own layout,
  since the bench's sits on its row of the app's table. M9.X2's vendor helper writes an element and a script, as its
  promise states: the content a slot loads holds a vendor's script when a vendor helper renders it, which the
  views rule does not reach in a helper. A plan that is not JSON is served only by the page's own endpoint, written by hand with the marker
  `Alis.Reactive/RenderingAndDelivery/ViewPlans.cs` writes, since the framework never writes one. An element a page
  writes in a request's `Finally` shows a test that a stopped step has finished, since a stop runs each `Finally` of
  the requests it happens inside (D10.23). A listen from a partial's or a card's plan on another plan's component is
  written on that plan's load. X13.4 reads its summary as the page's list items, as the validation cells do. Raised
  for the owner: TEST009 refuses an expected value the test typed, and M24.X10's outcome is the value the server
  renders back into the box after a refused bind; the promise waits, and so does its net48 leg on the harness.
- The lead, 2026-11-12, declaring the reference Fusion slice as signatures (M21.N1, M21.N2), the exemplar of the
  other Fusion slices, each name read from EJ2 33.1.47's TypeScript. `FusionComponent` carries `FusionVendor` as a
  property, since `ComponentVendor.Define` throws until module 12's unit builds it and a static field would fail
  every Fusion type that loads. Both lines' `Fields<TItem>(text, value)` takes `Expression<Func<TItem, object?>>`,
  which ALIS009 refuses, so each member it selects is a type parameter of its own, `Fields<TItem, TText, TItemValue>`
  and `Fields<TItem, TText, TItemValue, TGroup>`, still an extension of EJ2's `MultiSelectBuilder`, which `build`
  hands (M21.D1); EJ2's builder carries no item type, so a view names the item's class in the first lambda's
  parameter. The items the view hands and the items `UpdateData` shows are an `IReadOnlyList<TItem>`,
  and `DataSource<TItem>()` a `Property<IReadOnlyList<TItem>>`. The tagging payload's `ItemData<TItem>()` reads the
  app's item, as the cell event.object reads `Read(d => d.Name)`, in place of EJ2's `FieldSettingsModel`. No member
  carries `name:`. No analyzer reads `[Mirrors]` until N17.2's is built, so the build proves these names compile and
  no more. The owner can overrule each.
- The lead, 2026-11-12, landing fifteen pages of module 8, two forks' work, and settling what they read into. A page
  shows a 400 by `ShowValidationErrors` in the request's error scope for status 400, as the admission screen's
  addressed move-in does, since the condensed default needs a validator registered at startup. An answer a
  placement has the harness seed is the endpoint's own rule (room 13 is closed), with no seed. X16.7's tabs are two
  exclusive native disclosure panels inside the container, since a tab its slot loads on a click holds no message
  element while closed and no door shows or hides an element (M5.N1); the placement's tab slot waits on that door.
  X16.2's container holds the Phone input alone and the lead's name stands outside it, so both saves send the
  container whose summary the promise reads (D13.6, D16.13). X16.3's validator names the deputy with the
  null-forgiving operator: FluentValidation 12.1.1 warns CS8620 on the promise's sentence over a nullable member
  and runs no child rule on a null one (a scratch probe), so the form is FluentValidation's own. "No message shows"
  reads as no summary item once the answer line shows. The owner can overrule each.
- The lead, 2026-11-12, landing twelve Fusion slices of the admission-clinical screen as signatures, two forks'
  work, and settling what they read into. The multi-column combo box's helper is written with every type argument,
  `FusionMultiColumnComboBox<TModel, TMember, TValue>`, since C# takes all of a method's type arguments or none and
  its line writes TValue alone. An event whose payload EJ2 types as an object of no shape (a drop-down's focus and
  blur) is the core's `Event`, with no payload declared. A payload member EJ2 marks optional whose line gives no
  nullable type is optional and not nullable (the check box's checked, the radio button's value). A slice keeps
  its line's members and no more: the combo box declares no items property, and the multi-column combo box's value
  and text are text that admits null, as M21.X511 states. A render helper that takes the view's HTML helper is
  split by host (the radio button's, the stepper's, the smart paste button's); the uploader's take the file members
  `MultipartBody.File` takes on each host (M21.X710), and the date range picker's take a list of dates that admits
  null alone, since two overloads cannot differ by an annotation. The uploader's files-changed event and its
  payload are the framework's (M21.D701) and mirror no EJ2 type. A gap, raised for the core: the uploader's input
  is its files, the browser files EJ2's public list of files holds (M21.D701), and `ComponentInput.Of` takes a
  declared `ReadOnly`, while no door reads one from the members of a returned list, so the uploader's `Files`
  throws until module 7's unit designs that door. A promise's "FusionRadioButtons" is the plural of the radio
  button, no door. The owner can overrule each.
- The lead, 2026-11-12, landing twenty Fusion slices of the record-contacts and handover screens as signatures,
  three forks' work, and settling what they read into. A data class a slice hands EJ2 or reads from a payload (a
  split button's or a drop-down button's item, a Kanban column, an in-place editor's action data) names each member
  by `JsonPropertyName` with EJ2's name (X17.48), and a member EJ2 marks optional admits null; a Kanban column's key
  is required, typed by the cards' key (M21.X303). An enum a slice hands EJ2 (the text box's icon place, the
  button's icon position, the Kanban's dialog action) carries a string enum converter and each member's EJ2 name
  by `JsonStringEnumMemberName`, which an app's own enum converter honors too (M21.X602, M21.X301). A method keeps
  the arguments its line or the older lines' door gives: the Kanban's card and dialog methods as the redesign's,
  the accordion's expand with both its arguments; an overload for an argument EJ2 marks optional waits for a
  promise that needs it. A member EJ2's TypeScript does not type and a probe pins (the in-place editor's payload
  names and action data) is declared from the probe. A render helper's element id is `elementId` in every slice,
  where a line writes id; a position in pixels is a double. A slice's component code under the runtime's
  components folder (the mention's) is its unit's. The owner can overrule each.
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
| `spec/MISTAKES.md` | the log of mistakes: how it is kept and read, the classes with their counts and what holds each, what waits for the owner, and one entry a mistake | appended by hand, the turn a mistake is found |
| `spec/tools/check.py` | every check the brief's section 1 names, and a brief mode for the compact hook | hand |
| `spec/STATE.json` | the phase, the step, each check's result and counts, the next action | `spec/tools/check.py` only |
| `spec/Elements/module.md`, one per module folder, named as the module table names it in PascalCase, hyphens dropped | the core questions the module uses; its doors, "M5.N1" for module 5's first, each with what it replaces, and each core door it keeps, so every core door is named by a module; the improvements it proposes, each with its evidence; every cite at the SHA it names ("at 623dd4a27"), or at HEAD, the redesign or main when it names none | hand |
| `spec/Elements/features.json`, one per module folder | one entry per feature, "M5.F1" for module 5's first: its declaring types as "package:type"; the doors of another type it claims, as a module claims its entry doors on the pipeline builder; its sentences of main and the redesign, each found at the lines it cites at its line's SHA, and the proposed ones with their doors; its schema nodes; the C# that writes it and the runtime that executes it, each cite tagged with its line, and the redesign's at the State SHA wherever the feature is the redesign's; its core questions; its own meanings, "M5.D1", only where no core answer gives one; its oracle; its checker, "core:Q4" or a checked TLC invariant; its defects, a core "X7.8" or its own "M5.X1", each with its rung, every core defect in exactly one feature; a promise for each refusal and behavior defect, one for each place its line names; its status, by the lines its doors are on, or internal for a feature of an internal module that owns no door, or new for a feature of any other module that owns no door, one of whose sentences names a door its module.md's Doors section marks new; and a verdict for each main-only door: kept, renamed to doors, or refused with its reason and its carriers, each a core or module door | hand; the six feature checks of `spec/tools/check.py` read it |
| `spec/Reference/matrix.json`, `spec/Reference/syncfusion.json`, `spec/Reference/native.json` | the conformance matrix: BRIEF.md section 8's bullets word for word, one cell per item, each with the D and X lines it rests on, the schema kinds its run writes and whether it runs or is refused; for every cell, FusionMultiSelect's member by its line in EJ2's installed TypeScript declarations, or the scan that finds none, and the cells MultiSelect tests as its own, each with the reason Syncfusion's onboarding needs it; and the conformance library's declarations, ConformanceComponent and the five beside it, each component's vendor in D17.3's closed sets, their types, members and versions, with the member path that runs each cell | hand; the matrix check of `spec/tools/check.py` reads them |
| `spec/REWRITE.md` | the repository the rewrite builds and its units, each with its features | hand; the rewrite check of `spec/tools/check.py` reads it |
| `spec/Reference/app.json` | the care home conformance app: its areas, harness verbs, part kinds and hosts; each screen with what a user does there and its parts, the Syncfusion ones on a page under it; and the screen, hosts, parts and harness verbs of every feature test and every cell test | hand; the app check of `spec/tools/check.py` reads it |

## How a step runs

A spec unit closes the loop of root CLAUDE.md's Spec-first section: re-read the step's section of `spec/BRIEF.md`,
change it, append what was decided to `spec/NOTES.md`, commit by path, run `python3 spec/tools/check.py` at that head,
and push the spec branch the one way section 16 of the brief allows, then `spec/STATE.json` as that run wrote it.
A plant names the text it replaces, so a commit that rewrites a line of the spec moves the anchor of each plant that
names it in the same commit, and the plants of the checks that read the line run before the full check (the lead's
slips, 2026-11-11).
