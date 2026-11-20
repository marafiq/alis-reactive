# Alis.Reactive.Conformance.App.Net48

The twin: the conformance app's MVC 5 host, an SDK-style net48 project outside the solution on the packed packages,
which serves the pages `spec/Reference/app.json` places under the host net48.

- It compiles the world contract and the conformance app's net48 files by link, each ".Net48.cs" controller and
  ".Net48.cshtml" view under the app's screen folders and the models and views a page both hosts serve
  (`Alis.Reactive.Conformance.App.Net48.csproj`). Its build lays the site out under the folder
  `tests/Alis.Reactive.Conformance.World/TwinSite.cs` names on Windows, the one the harness serves from IIS, and under
  "obj/site" elsewhere, with the required conformance script `build.mjs` bundles before the twin builds.
- `TwinApplication.cs` is its startup: `AddReactiveRequests` with the antiforgery token in a header and no other
  default, `AddReactiveJson` with camel-case enum names, `AddReactiveFluentValidation` with `RoomNumberingRules`,
  and MVC's resolver set over the container
  (`TwinResolver.cs`). A world seeded with `Seed.TheResolverLeftUnset` answers through MVC's own resolver alone, as
  an app that never set one does.
- A request names its culture and its time zone as the conformance app's does, from the same closed lists and with
  the same 400 pages (`RequestCulturesAndZones.cs`). `World/` keeps each world by the cookie "world" and answers
  the seed, its end and the host orders `tests/Alis.Reactive.Conformance.App/Components/component-host.ts` waits on.
- An attribute route of MVC 5 starts with no "/", so `AddressAttribute.cs` takes a page's address as the world
  contract names it and trims its leading "/".
- An exception a page throws answers with its type and message as the page's heading (`ErrorPage.cs`), the
  conformance app's error page.
- `Web.config` holds MVC 5's Razor configuration for views outside a Views folder, and no binding redirect, since
  the build suggests none.
- It builds on the packs the command below writes, at the version its build reads from the property
  PACKAGE_VERSION, as the fresh apps' builds do, with every warning an error.

```bash
scripts/pack.sh 0.0.0-twin.1
```
