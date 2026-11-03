# net48 hand test on Windows

CI's `verify-net48` proves on every run what `examples/net48-mvc-smoke/README.md` lists: the build and
pack on .NET Framework 4.8, each package's files landing in `Content\{PackageId}\`, their versioned
links, and the runtime booting under IIS Express with one reactive interaction. That fixture is
native-only and registers no validator. This checklist covers the rest, by hand, on Windows with
.NET Framework 4.8 and Visual Studio (or MSBuild and IIS Express). A macOS run is not a .NET Framework
host and proves none of it.

## Setup

1. Clone the repository on Windows and check out the commit to test.
2. Pack the packages: `npm ci`, `npm run build:all`, `scripts/pack.sh <version>` (writes `./nupkgs`).
3. Create a classic ASP.NET MVC 5 web application (.NET Framework 4.8, `packages.config`): in Visual
   Studio, "ASP.NET Web Application (.NET Framework)", then MVC.
4. Add `./nupkgs` as a local NuGet source and install `AlisReactive`, `AlisReactive.Native`,
   `AlisReactive.Fusion` and `AlisReactive.DesignSystem` at `<version>`.

## The checks

1. **Fusion's files land.** After a build, `Content\AlisReactive.Fusion\css\fusion.css` is there beside
   the files the smoke already proves (README, "Browser assets", lists every file).
2. **Syncfusion renders on net48.** The net48 package depends on `Syncfusion.EJ2.MVC5`, not
   `Syncfusion.EJ2.AspNet.Core`. A view with a Fusion component (a `FusionDropDownList`) renders with no
   exception, and the component opens and shows its options in the browser.
3. **Validation through MVC 5's `DependencyResolver`.** Register a `ReactiveValidator<T>` in
   `Global.asax` `Application_Start`, submit invalid data, and see the browser's messages, then the
   server's for a rule only the server checks.
4. **Native markup.** A rendered `Html.InputField(...).NativeTextBox(...)` carries the `id` and `name`
   the plan names, and a reactive interaction on it (a live clear, a conditional show) works.
5. **Binding redirects under `packages.config`.** The app runs with no `FileLoadException` for
   `System.Text.Json` or its dependencies; if NuGet's `Add-BindingRedirect` was needed, compare the
   redirects with the smoke's `Web.config` (README of the smoke, gotcha 2).

A release's net48 sign-off is these five checks done by hand in a browser on Windows, together with
a green `verify-net48` run on the same commit.
