# Alis.Reactive.Conformance.App

The care home conformance app: every browser test runs on one of its screens (`spec/Reference/app.json`). It is
built on the shipped projects as an app builds on the packages, and `.claude/rules/views.md` holds the standard of
its views.

- A screen reads as a product's: it holds what its user does, a model of its own that no other screen shares, and
  nothing that exists only to be asserted.
- `SliceTree.txt` orders its slices, so no screen reaches into another's model, and the framework's analyzers read
  its controllers and models as they read an app's.
- `Program.cs` sets the JSON options `spec/Reference/app.json` gives the net10 host, camel-case members and enum
  names; what else that host's definition names lands with the door or the page that needs it.
- The World area (`World/`) scopes data, gates and caches to a test's world by the cookie `"world"`, in Development
  alone. A document's request that carries no world gets a new one, so a person runs the app as a test does; any
  other request with no world gets none.
- `Components/` holds the conformance library (`spec/Reference/native.json`), a folder for each component with its
  declaration, its payloads, its live objects and the data classes its members name; what several components
  reach sits in `Components/` itself. A declaration mirrors its component member for member, each by its C# name and
  its door as that file states them; none is edited by hand to differ from it. A data class's member starts at what
  that file's type and every promise that reads the member lacking state, and a reference member the file holds
  non-null that starts at null is written null-forgiving, as `Components/ConformanceComponent/Binding.cs` writes its
  sealed spot; the nullable compiler chooses no start (the lead's slip, 2026-11-13).
- A component's render helper sits beside its declaration and starts it from a class of its own
  (`Components/ConformanceComponent/ConformanceStart.cs`), written as the plan writes a literal; the class gains a
  member when a page first needs the component to start with it, and what its payload's held object holds is
  `Components/ConformanceComponent/HeldStart.cs`, what its record holds `Components/ConformanceComponent/RecordStart.cs`. Its `Version` is the build of the component a
  world names, `ComponentVersion` of the world contract, one of the versions `spec/Reference/native.json` lists, and its three endpoint members name the settle, count
  and item endpoints of `World/WorldArea.cs`, which the component's methods await: each reaches the settle gate
  and answers a 500 where a seed says so. A page's layout is `Shared/_Layout.cshtml`, which
  links the framework's script, unless the page renders something of its own beside its body, as the ward's does.
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
  tests, a folder a module; a page whose plugins are scripts takes `Bench/Plugins/_ScriptedLayout.cshtml`, which
  links the scripts the page names before the framework's. `Bench/Schema/` holds the bench's page of module 14,
  two plans that handle one element's events. `Care/Shell/` holds the shell screen's pages, which take
  `Care/Shell/_ShellLayout.cshtml`: it renders the drawer and the confirm beside the body, as an app's layout
  renders its app-level objects. A page whose request sends no container renders the notice region,
  `Html.NativeToast`, itself, since the core shows that request's failed check there (D16.13), and it needs no
  pipeline door for it (the lead's reversal, 2026-11-13). A shell page that styles the drawer writes a style
  element in the layout's Styles section, which the layout renders in its head (`Care/Shell/ShiftNotes.cshtml`).
  A care page whose plugins are scripts takes its
  screen's own layout, `Care/Record/_ScriptedLayout.cshtml`, which links the scripts the page names before the
  framework's: the bench's
  sits on the same row of the table, so each keeps its own. `Bench/Validation/` holds the validation cells: their
  forms, their validators (`Bench/Validation/ValidationRules.cs`, not registered until U17 builds the startup call;
  a request names its validator by `Validate<TValidator>()`) and the form endpoints, which answer what they bound;
  a page groups each field it checks in a fieldset or a region that names it, the scope a test reads its message
  in.
- `Refusals/Index/` is the refusals index, a page of links: a refusal page a promise reaches from the index has
  its link there, and so does the page that renders beside it. A module's refusal pages sit in a folder named for
  it (`Refusals/ArrayLambdas/` holds ten): a controller for each screen with its models, the refused page and the
  page of its opposite case, one view where only the data differs. An endpoint a page alone names is a constant
  beside its controller (`Refusals/ArrayLambdas/ArrayLambdaEndpoints.cs`); one a test names is a constant of
  `RefusalEndpoints` in the world contract.
- `Care/` holds the care screens, a folder for each screen of `spec/Reference/app.json` (`Care/Residents/` the
  first): a promise placed on a screen is a page of its own there, as a refusal's is, with its controller, its
  models and its view, named for what a person does on it (`Care/Residents/CensusController.cs`,
  `Care/Residents/Census.cshtml`). Where the promise changes what the page or its server holds, the opposite
  case is a second address of the same view; a seed is for what a test changes while its page is open. A list a
  page reads is a response body of the page's own endpoint, named beside its controller
  (`Care/Residents/ResidentsEndpoints.cs`) unless a test names it. A Fusion screen's page takes
  `Shared/_FusionLayout.cshtml`, which holds what `Shared/_Layout.cshtml` holds until U25 links EJ2's script and
  theme in it, before the framework's (`Care/AdmissionClinical/AllergyIntake.cshtml`).
- A screen's files sit together in its folder, `Refusals/Schema/` the first: its controller, its model, its view
  and its layout, a view named by its path. A layout links the framework's script from the core's static web
  assets, last; the static files middleware serves it, which adds no route.
- `World/WorldData.cs` keeps a world's gates by action and endpoint: an endpoint's filter reaches its gate with
  its own address, and takes the hold that names it before one that names none. An MVC action a test holds at the
  answer gate carries the filter `World/AtTheAnswerGateAttribute.cs`, which reaches the gate with the action's own
  address (`World/WordDeskController.cs`); another gate gets its filter with the first page whose test holds it.
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
