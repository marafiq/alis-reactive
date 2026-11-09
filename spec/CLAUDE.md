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
  DI abstractions take one exact version per target framework, set beside System.Text.Json's 10.0.12.
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
  while the element it held when it listened still holds it at the vendor's object path, the rule a handler's release
  follows; the vendor names where its object holds its element with an element path. Refused where the view renders: a
  listen on one whose vendor names no element path. Chosen within the owner's yes, 2026-11-09, gate 1's P5, answering
  14a-i's flag (3): a listen on a component reached by its element's id is the door a slot's plan has to a component
  the page rendered, released by the element it reached, so its vendor needs no element path (D12.8).
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
  `spec/Core/DateWire.tla` NothingWritesNoText). The redesign's earliest moment was the start of a non-nullable member with no
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
  MVC's binding, which the docs name (C25). Request.tla checks the first two over both hosts. The owner can overrule
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
  replace and a slot's unload (`spec/Core/Composition.tla`).
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
  included (D11.10, D11.5, `spec/Core/Run.tla`).
- Chosen within the owner's yes, 2026-11-08: a row action is one handler per action and one entry per rendered
  control, and the entry holds only the row members the action's block reads; a button takes any block and a link a
  block whose one request is a GET, its href; the handler's policy spans the action's rows, and a link reads nothing
  another script did with the click (D17.15). Both lines write a whole plan per link. The owner can overrule it.
- The owner, 2026-11-08, amending G2: "I can give you go ahead for amend, but you have to make sure the quality bar stay
  high, and think through and tell me if this make sense or not." HTTP adds one statement, `request`, whose executor is
  `spec/Core/Request.tla`'s state machine, and a pipeline is one run, `spec/Core/Run.tla`'s, whose driver alone touches a
  promise (D10.23, D11.11, D11.12). The bar the owner set with it: the machines simplify the runtime's mix of sync and
  async, and an async call in a stack that started sync is proven by TLA+ and then in the browser.
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
| `spec/Elements/module.md`, one per module folder, named as the module table names it in PascalCase, hyphens dropped | the core questions the module uses; its doors, "M5.N1" for module 5's first, each with what it replaces; the improvements it proposes, each with its evidence | hand |
| `spec/Elements/features.json`, one per module folder | one entry per feature, "M5.F1" for module 5's first: its declaring types as "package:type"; the doors of another type it claims, as a module claims its entry doors on the pipeline builder; its sentences of main and the redesign, each found at the lines it cites at its line's SHA, and the proposed ones with their doors; its schema nodes; the C# that writes it and the runtime that executes it, each cite tagged with its line, and the redesign's at the State SHA wherever the feature is the redesign's; its core questions; its own meanings, "M5.D1", only where no core answer gives one; its oracle; its checker, "core:Q4" or a checked TLC invariant; its defects, a core "X7.8" or its own "M5.X1", each with its rung; a promise for each refusal and behavior defect; its status; and a verdict for each main-only door: kept, renamed to doors, or refused with its reason and its carriers, each a core or module door | hand; the six feature checks of `spec/tools/check.py` read it |

## How a step runs

Re-read the step's section of `spec/BRIEF.md`, do it, run `python3 spec/tools/check.py`, append what was
decided to `spec/NOTES.md`, then commit by path and push the spec branch the one way section 16 of the brief allows.
