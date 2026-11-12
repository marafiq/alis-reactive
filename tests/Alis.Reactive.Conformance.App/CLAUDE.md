# Alis.Reactive.Conformance.App

The care home conformance app: every browser test runs on one of its screens (`spec/Reference/app.json`). It is
built on the shipped projects as an app builds on the packages, and `.claude/rules/views.md` holds the standard of
its views.

- A screen reads as a product's: it holds what its user does, a model of its own that no other screen shares, and
  nothing that exists only to be asserted.
- `SliceTree.txt` orders its slices, so no screen reaches into another's model, and the framework's analyzers read
  its controllers and models as they read an app's.
- The World area (`World/`) scopes data, gates and caches to a test's world by the cookie `"world"`, in Development
  alone. A document's request that carries no world gets a new one, so a person runs the app as a test does; any
  other request with no world gets none.
- `Components/` holds the conformance library (`spec/Reference/native.json`), a folder for each component with its
  declaration, its payloads, its live objects and the data classes its members name; what several components
  reach sits in `Components/` itself. A declaration mirrors its component member for member, each by its C# name and
  its door as that file states them; none is edited by hand to differ from it.
- A component's render helper sits beside its declaration and starts it from a class of its own
  (`Components/ConformanceComponent/ConformanceStart.cs`), written as the plan writes a literal; the class gains a
  member when a page first needs the component to start with it. Its `Version` is the build of the component a
  world names, `ComponentVersion` of the world contract, one of the versions `spec/Reference/native.json` lists, and its three endpoint members name the settle, count
  and item endpoints of `World/WorldArea.cs`, which the component's methods await: each reaches the settle gate
  and answers a 500 where a seed says so. A page's layout is `Shared/_Layout.cshtml`, which
  links the framework's script, unless the page renders something of its own beside its body, as the ward's does.
- `Bench/` holds the component bench, a folder for each family of cells of `spec/Reference/native.json`
  (`Bench/Properties/` the first): a controller with an action for each cell's page, the page's model, the app
  events its buttons raise, declared as static members, and a view for each cell, named for it. A page starts its
  component from what its world holds, so one page serves a cell and its opposite case.
- `Refusals/Index/` is the refusals index, a page of links: a refusal page a promise reaches from the index has
  its link there, and so does the page that renders beside it.
- A screen's files sit together in its folder, `Refusals/Schema/` the first: its controller, its model, its view
  and its layout, a view named by its path. A layout links the framework's script from the core's static web
  assets, last; the static files middleware serves it, which adds no route.
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
