# Rendering and delivery

How a view's plan and the framework's browser files reach the page on each host: the plan rendered as one script of
plan data, every file the packages ship linked by a URL that carries its content hash, a package that writes into an
app's project only into a folder git ignores, and the app's registrations a view reads on net48 and on net10. Module
19 of `spec/CLAUDE.md`.

## Core questions

- Q14: each view's plan is its own, started in the view and carried between views by no server state (D14.1); the
  plan is a block of its view, whose end writes the plan (D14.7, N14.2); plans compose in the browser without merging
  (D14.2). This module says what the block's end writes into the page, and how the page gets the files that read it.

## Boundaries

The plan's own types, `ReactivePlan<TModel>` and `PlanExtensions`, with the verdicts on `Html.RenderPlan`, `Render`
and `ResolvePlan`, are module 9's (M9.F1), and the plan block's rows are module 7's (N14.2). The plan's grammar and
its version are module 14's, and so is M14.D1's refusal of a script that is no plan; the boot that finds each plan
script is module 17's. Markup raise is module 1's (M1.D3), and a plan's trace context module 26's (N19.3). Each door's
two host forms, `HtmlHelper<TModel>` and `IHtmlString` on net48 and `IHtmlHelper<TModel>` and `IHtmlContent` on net10,
are its type's rows, in its own module. A startup call, and its refusal where a view finds none, are its door's:
`AddReactiveRequests` and X10.28 module 6's, the validators' registration module 8's. The browser files of
Alis.Reactive.Fusion and Alis.Reactive.Native follow M19.D1 and M19.D2, and each package's own link is its module's,
21 and 24; Alis.Reactive.DesignSystem and Alis.Reactive.NativeTagHelpers are out of scope.

This module owns the redesign's `UrlVersionExtensions` and holds M19.D1 to M19.D4 and their defects, M19.X1 to M19.X4.

## Doors

Each name is gate 2's; each door is the capability it carries.

- `M19.N1` kept: the redesign's `Url.AppendVersion(contentPath)` on net48, `UrlVersionExtensions`, which writes the
  URL of a file under `~/Content/{PackageId}/` with the file's content hash as `v`, the hash net10's own
  `asp-append-version` writes (M19.D1). On net10 the core adds no door: a view writes ASP.NET Core's
  `asp-append-version="true"` on `~/_content/{PackageId}/`, the platform's own, and a framework door beside it would
  be a second door for one concept.

No main-only door is this module's: main delivered its files by its build alone, through no door.

The meanings M19.D1 to M19.D4 and the defects M19.X1 to M19.X4 are in `spec/RenderingAndDelivery/features.json`, on
the feature they give.

## Improvements

- Every framework file reaches the browser by a URL that carries its content hash, and a view's link stays the same
  across upgrades (M19.D1, M19.X1, M19.X2). Main copies each file under a name holding the package's version, and a
  view links that name (`examples/net48-mvc-smoke/Views/Shared/_Layout.cshtml:18` at 623dd4a27), so every upgrade
  edits each layout that links a file. The redesign links the copy under `Content\{PackageId}\` on net48 by
  `Url.AppendVersion` (`examples/net48-mvc-smoke/Views/Shared/_Layout.cshtml:15` at 65fdf118d) and the static web
  asset under `_content/{PackageId}/` on net10 by `asp-append-version`
  (`Alis.Reactive.SandboxApp/Views/Shared/_Layout.cshtml:7` at 65fdf118d), and its net48 smoke checks each link's `v`
  against the SHA-256 of the bytes served (`examples/net48-mvc-smoke/ci/verify.mjs:24-25` at 65fdf118d), the design
  this module keeps.
- A package writes into an app's project only on net48, into the folder it owns, beside a `.gitignore` holding `*`
  (M19.D2). Main copies into `Content\alisreactive\` on net48 and `wwwroot\scripts\` on net10 under versioned names,
  with no ignore file, so the app's git lists new files after every upgrade (`build/AlisReactiveAssets.targets:58-71`,
  `examples/net48-mvc-smoke/README.md:19` at 623dd4a27). The redesign's net48 copy writes the `.gitignore` beside the
  files (`build/AlisReactiveNetFrameworkContent.targets:10-14` at 65fdf118d), and its CI fails on any file git lists in
  the smoke app (`.github/workflows/verify-net48.yml:102-103` at 65fdf118d); on net10 it ships the files as static web
  assets and writes nothing into the project (`Alis.Reactive/Alis.Reactive.csproj:28` at 65fdf118d).
- A plan's script holds no text that ends it (M19.D3, M19.X3). Both lines write the plan through System.Text.Json's
  default encoder (`Alis.Reactive/PlanModel/Serialization/PlanSerializer.cs:11-14` at 623dd4a27,
  `Alis.Reactive/Grammar/GrammarSerializer.cs:12` at 65fdf118d), and neither states why or proves it. Main also names
  each script by its plan id (`Alis.Reactive/Razor/Extensions/PlanExtensions.cs:98` at 623dd4a27), which D13.2
  removes, since a plan id keys nothing.
- A view reads the app's registrations by one read per host (M19.D4, M19.X4). Both lines read the request's services
  on net10 (`Alis.Reactive/Razor/PlanExtensions.cs:48` at 65fdf118d) and MVC 5's `DependencyResolver` on net48
  (`Alis.Reactive/PlanAuthoring/ReactivePlan.cs:155` at 623dd4a27, `Alis.Reactive/PlanAuthoring/ReactivePlan.cs:158`
  at 65fdf118d), each service read again in its own place, and no spec line states the rule.
