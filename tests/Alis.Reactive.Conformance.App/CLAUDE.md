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
  other request with no world gets none. The app holds no static mutable state: what a test changes lives in its
  world.
- An endpoint is written as an app writes one. What a test needs the server to decide, when an answer leaves or what
  it holds, goes through the harness's verbs, never through a control on a page.

```bash
scripts/run.sh
dotnet build tests/Alis.Reactive.Conformance.App -c Release
```
