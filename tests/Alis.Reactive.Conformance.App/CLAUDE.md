# Alis.Reactive.Conformance.App

The care home conformance app: every browser test runs on one of its screens (`spec/Reference/app.json`). It is
built on the shipped projects as an app builds on the packages, and `.claude/rules/views.md` holds the standard of
its views.

- A screen reads as a product's: it holds what its user does, a model of its own that no other screen shares, and
  nothing that exists only to be asserted.
- `SliceTree.txt` orders its slices, so no screen reaches into another's model, and the framework's analyzers read
  its controllers and models as they read an app's.
- The app starts under the host its argument `--host` names, a host of `spec/Reference/app.json`, and `AppHost.cs`
  sets the JSON options that definition gives it: net10's camel-case members and enum names, and for each of
  numbersAsText, dateConverter, snakeCase, pascalCase and enumsAsNumbers, those options with the one change its
  definition names; jsonContract's are net10's that ignore a member holding its default and respect required
  constructor parameters, and its resolver lands with the page that reads an admission's pharmacy; siteHeader's
  request defaults are net10's with the app's header Care-Site beside them, and no host's defaults send
  X-Requested-With (the owner, 2026-11-15). The app's cookie authentication answers a challenge as net10 does, 401 at
  an API endpoint and a redirect to the record's sign-in page elsewhere (`Care/Record/SessionNotesController.cs`). Every host's
  startup names the flags enums its pages read, as net10's definition names them, in `AppHost.cs`: the conformance
  library's `CareDays` and the care rota's `CareDays` and `RotaDays`. An app started under no host, or under one it does not serve, stops and names the hosts it serves.
  The harness passes the host a fixture runs under, `scripts/run.sh` passes net10, and the page sweep sets it. What
  else a host's definition names lands with the door or the page that needs it.
- A request names the culture the app binds and renders it under in its query "culture": "en-US", "de-DE",
  "tr-TR", "haw-US", "de-DE_phoneb" or "invariant", the invariant culture, whose plan collation is "en" (D3.11). A
  request with none takes "en-US", so no page reads the machine's culture, and one that names another, or two,
  answers 400. `RequestCultures.cs` sets both current cultures from that closed list before MVC reads the request:
  ASP.NET Core's request localization picks a culture by its name, and the German phonebook sort is named "de-DE" and
  the invariant culture "". Each request names its own, a slot's load and a form's send among them; the world's seed
  and the browser's locale choose none. Invariant globalization mode is a process setting the query cannot reach, and
  no host of `spec/Reference/app.json` runs it.
- A request names the time zone the app renders it under in its query "zone": "America/New_York" or
  "Asia/Kolkata". A request with none chooses none, so its plans show and send each instant on the browser's
  clock, and one that names another, or two, answers 400. `RequestZones.cs` chooses it by
  `RequestTimeZone.RendersUnder` before MVC reads the request; each request names its own, a slot's load among them.
- The World area (`World/`) scopes data, gates and caches to a test's world by the cookie `"world"`, in Development
  alone. A document's request that carries no world gets a new one, so a person runs the app as a test does; any
  other request with no world gets none. Its host endpoints stand for a component's host (`World/HostOrders.cs`):
  `Components/component-host.ts`, on every page the conformance script runs, waits for an element's id at
  `WorldEndpoints.HostOrders`, restarts the component the conformance library mounted there, destroying the object,
  emptying the element and building a new object from the element's start one generation on, and reports to
  `WorldEndpoints.HostDone`, an empty report when the element holds the new object and the sentence of what failed
  otherwise; `TestWorld.Restart` posts the order to `WorldEndpoints.HostRestart`, which answers when the report comes,
  409 with its sentence when it names a failure, and the world's end answers a waiting page with no order.
- `Components/` holds the conformance library (`spec/Reference/native.json`), a folder for each component with its
  declaration, its payloads, its live objects and the data classes its members name; what several components
  reach sits in `Components/` itself. Its busy indicator and its notice are no declarations
  (`Components/ConformanceBusy/`, `Components/ConformanceNotice/`): each is a render helper a layout calls once and a
  script that provides its contract's plugin, "busy" or "notice", when the helper's element mounts, since the core
  alone calls a contract's members (D10.21); each shows what the core told it in one status, Busy or Notices, as
  `spec/Reference/native.json` states it. A declaration mirrors its component member for member, each by its C# name and
  its door as that file states them; none is edited by hand to differ from it. A data class's member starts at what
  that file's type and every promise that reads the member lacking state, and a reference member the file holds
  non-null that starts at null is written null-forgiving, as `Components/ConformanceComponent/Binding.cs` writes its
  sealed spot; the nullable compiler chooses no start (the lead's mistake, 2026-11-13).
- A component's render helper sits beside its declaration and starts it from a class of its own
  (`Components/ConformanceComponent/ConformanceStart.cs`), written as the plan writes a literal; the class gains a
  member when a page first needs the component to start with it, each member named for the component's code by
  `[JsonPropertyName]`, so a start reads the same under every host's naming (D17.13), and what its payload's held object holds is
  `Components/ConformanceComponent/HeldStart.cs`, what its record holds `Components/ConformanceComponent/RecordStart.cs`. The component's code
  holds the data its start holds as that JSON holds it, adding and dropping no member, so a data member a promise needs absent is
  written absent in C#, `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]` on a member whose null must read its start
  (`Components/ConformanceComponent/BindingStart.cs`, X4.8) (the lead, 2026-11-15). Its `Version` is the build of the component a
  world names, `ComponentVersion` of the world contract, one of the versions `spec/Reference/native.json` lists, and its endpoint members name the endpoints of
  `World/WorldArea.cs` its methods await, each at the settle gate and answering as a seed says: a 500, a count or the
  check's answer. A page's layout is `Shared/_Layout.cshtml`, which
  links the framework's script, unless the page renders something of its own beside its body, as the ward's does. A
  layout whose pages send a request renders `Html.ConformanceBusy()` and `Html.ConformanceNotice()` after its body,
  since the host's request defaults name both contracts and a request whose page lacks either stops before it sends
  (D10.21); the shell's layout renders the Native ones in their place, and the page of X10.20 takes
  `Care/Residents/_NoBusyLayout.cshtml`, which renders the notice alone.
- `Bench/` holds the component bench, a folder for each family of cells of `spec/Reference/native.json`
  (`Bench/Properties/` the first; `Bench/Poc15/` holds a controller for each member the event.poc15 cells reach; a
  page reaches another family's endpoint by its constant of `BenchEndpoints` alone, as `Bench/Lanes/` loads the card):
  a controller with an action for each cell's page, and for each endpoint of the test app its cells call, named in
  `BenchEndpoints` of the world contract (`Bench/Http/HttpCellsController.cs`), a plugin of the bench a cell calls
  beside the cell that calls it (`Bench/Sources/PageLog.cs`), the page's model, the app
  events its buttons raise, declared as static members, and a view for each cell, named for it. A page starts its
  component from what its world holds, so one page serves a cell and its opposite case. A cell that holds a
  Native input binds it to a member of the page's model through `Html.InputField`, with the label a person finds
  it by (`Bench/Sources/SourceConditionRight.cshtml`). A cell that sends a form renders it as a block,
  `Html.BeginRequestForm`, ended by `using`, with the pipeline that sends it inside the block
  (`Bench/Http/HttpBesideHidden.cshtml`); the conformance component bound to a member is rendered by its second
  render helper, for a field (`Components/ConformanceComponent/ConformanceComponentHtml.cs`). `Bench/Vendors/`
  holds the pages of module 12's feature tests, each on the conformance declaration that holds its vendor kind;
  each of those declarations has its render helper beside it (`Components/ConformanceStore/ConformanceStoreHtml.cs`).
  `Bench/Declarations/`, `Bench/Plugins/` and `Bench/Execution/` hold the pages of the bench's other feature
  tests, a folder a module; a page whose plugins are scripts takes its own folder's scripted layout,
  `Bench/Plugins/_ScriptedLayout.cshtml` the first, since each folder under `Bench/` and `Care/` is a slice of its own
  (ALIS010), and the layout links the conformance script, then the scripts the page names, then the framework's. A
  page's plugin script sits beside the page, a TypeScript file named as its address names it
  (`Bench/Plugins/visitor-desk.ts`, at the address `DeskScripts` gives it), imports nothing, and provides its plugins
  on `window.__alisPlugins`; `build.mjs` bundles each into the app's wwwroot, at its path in lower case, where git
  ignores it. `Bench/Schema/` holds the bench's page of module 14, two plans that handle one element's events. `Care/Shell/` holds the shell screen's pages, which take
  `Care/Shell/_ShellLayout.cshtml`: it renders the drawer and the confirm beside the body, as an app's layout
  renders its app-level objects. A page whose request sends no container shows that request's failed check in the
  notice its layout renders (D16.13), and it needs no pipeline door for it (the lead's reversal, 2026-11-13). A shell page that styles the drawer writes a style
  element in the layout's Styles section, which the layout renders in its head (`Care/Shell/ShiftNotes.cshtml`).
  A care page whose plugins are scripts takes its
  screen's own layout, `Care/Record/_ScriptedLayout.cshtml`, which links the scripts the page names before the
  framework's: the bench's
  sits on the same row of the table, so each keeps its own. `Bench/Validation/` holds the validation cells: their
  forms, their validators (`Bench/Validation/ValidationRules.cs`, not registered until the request's check reads them, D16.2;
  a request names its validator by `Validate<TValidator>()`) and the form endpoints, which answer what they bound;
  a page groups each field it checks in a fieldset or a region that names it, the scope a test reads its message
  in.
- `Refusals/Index/` is the refusals index, a page of links: a refusal page a promise reaches from the index has its
  link there, and so does the page that renders beside it. A module's refusal pages sit in a folder named for it
  (`Refusals/ArrayLambdas/` the first): a controller for each screen with its models, the refused page and the page of
  its opposite case, one view where only the data differs. A refusal whose opposite case holds under another host's
  JSON options is one page at one address, opened in both worlds, since the host changes the app's options and nothing
  else (`Refusals/ArrayLambdas/FridgeReadingsAtZero.cshtml`). An endpoint a page alone names is a constant beside its
  controller (`Refusals/ArrayLambdas/ArrayLambdaEndpoints.cs`); one a test names is a constant of `RefusalEndpoints`
  in the world contract.
- `Care/` holds the care screens, a folder for each screen of `spec/Reference/app.json` (`Care/Residents/` the
  first): a promise placed on a screen is a page of its own there, as a refusal's is, with its controller, its
  models and its view, named for what a person does on it (`Care/Residents/CensusController.cs`,
  `Care/Residents/Census.cshtml`). Where the promise changes what the page or its server holds, the opposite
  case is a second address of the same view; a seed is for what a test changes while its page is open. A list a
  page reads is a response body of the page's own endpoint, named beside its controller
  (`Care/Residents/ResidentsEndpoints.cs`) unless a test names it. A Fusion screen's page takes
  `Shared/_FusionLayout.cshtml`, which holds what `Shared/_Layout.cshtml` holds until U25 links EJ2's script and
  theme in it, before the framework's (`Care/AdmissionClinical/AllergyIntake.cshtml`). A page that reads a value a
  person types or chooses, typed as its member, renders the field inside a container block and reads it by the
  container's `Read` (`Care/Record/PainAssessment.cshtml`).
- A screen's files sit together in its folder, `Refusals/Schema/` the first: its controller, its model, its view
  and its layout, a view named by its path. A layout links the framework's script from the core's static web
  assets, last; the static files middleware serves it, which adds no route.
- `World/WorldData.cs` keeps a world's gates by action and endpoint: an endpoint's filter reaches its gate with
  its own address, and takes the first hold not yet held that names it before one that names none; a second arm at
  one action and endpoint queues a hold behind the first, and the arm's answer numbers it. An MVC action a test holds at the
  answer gate carries the filter `World/AtTheAnswerGateAttribute.cs`, which reaches the gate with the action's own
  address (`World/WordDeskController.cs`); another gate gets its filter with the first page whose test holds it.
  The word slip, an action of the same controller, writes the world's word with MVC's JSON options, which the host
  sets, since the World area's minimal endpoints write with the HTTP options, which no host sets.
- The host `tracing` (`AppHost.cs`, `World/CollectorArea.cs`) exports the page's spans through the OpenTelemetry
  adapter and the server's activities of the source Alis.Reactive to `World/Collector.cs`, which keeps them by
  world. `World/RendersSampler.cs` records a request that carries no traceparent only when it renders a page of a
  world whose seed leaves its render recorded, so every fetch follows its own header's flag. The host listens on
  two addresses, and the second is another origin (`World/SecondOrigin.cs`) that allows the page's origin and the
  trace headers by CORS and logs each request it receives. The collector's page lists a world's spans as a tree,
  a line a span with its outcome, status and resend count, and redraws as spans arrive (`World/collector.ts`); the
  echo page lists each attempt's traceparent by the span it names, and baggage.
- `World/WorldCache.cs` is the app's distributed cache in Development, each key under its world, and
  `World/CachedRounds.cs` is the fragment a seed puts there, as a release of the framework rendered it.
- The app holds no static a request can change, since every world shares a static: none that can be assigned, and
  no read-only one that holds an array, a collection that is neither a read-only view nor an immutable or frozen
  one, or an object of the app's own that holds a member a request can assign. What a test changes lives in its
  world; a list that never changes is declared as a read-only view. `WhenTheAppHoldsNoStaticState` refuses each by
  its name.
- Every address the app answers to a GET, and every link its pages hold, answers the person who opens it:
  `WhenAnAppRendersItsPages` runs the app inside the architecture tests and opens each as a browser opens a
  document. `Program` is public for that run alone.
- An endpoint is written as an app writes one. What a test needs the server to decide, when an answer leaves or what
  it holds, goes through the harness's verbs, never through a control on a page.

```bash
scripts/run.sh
dotnet build tests/Alis.Reactive.Conformance.App -c Release
```
