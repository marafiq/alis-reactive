# net48 MVC5 runtime smoke

A classic, non-SDK **ASP.NET MVC 5** web app on **real .NET Framework 4.8**, consuming the shipped
`AlisReactive` net48 NuGet packages. It exists to **prove the runtime boots in a browser on .NET
Framework** — not just that the libraries compile. CI (`.github/workflows/verify-net48.yml`,
`net48-runtime-iis` job) builds it on `windows-latest`, hosts it under **IIS Express**, drives it with
**Playwright** (toggle a checkbox → a reactive conditional reveals a proof element), and uploads a
screenshot artifact.

It is intentionally **not** in `Alis.Reactive.slnx` — `dotnet`/`.slnx` does not build classic web apps;
CI builds it with MSBuild.

## Known net48 consumer gotchas this fixture documents

A real .NET Framework 4.8 consumer hits the same issues. Each is captured here with its fix:

| # | Problem | Fix (see file) |
|---|---------|----------------|
| 1 | **Asset delivery.** The package targets copy the runtime JS + design-system CSS to `Content\alisreactive\{name}.{version}.{ext}` (this fixture is **native-only**, so it does not pull the Fusion/Syncfusion CSS). There is no `asp-append-version` Tag Helper on net48. | Reference the version-stamped files directly: `@Url.Content("~/Content/alisreactive/alis-reactive.<version>.js")` (`Views/Shared/_Layout.cshtml`). |
| 2 | **Binding redirects.** `System.Text.Json` 9 and its transitive chain (`System.Buffers`, `System.Memory`, `System.Runtime.CompilerServices.Unsafe`, `System.Numerics.Vectors`, `System.Threading.Tasks.Extensions`, `Microsoft.Bcl.AsyncInterfaces`) need assembly binding redirects on .NET Framework, or you get `FileLoadException: ... manifest definition does not match the assembly reference`. Classic `packages.config` apps do **not** auto-add them. | `Web.config` `<runtime><assemblyBinding>`. A real consumer runs NuGet's `Add-BindingRedirect` or adds them by hand; this fixture's CI **auto-generates** them from the actual `bin\` assembly versions, so they are always exact. |
| 3 | **`WebApplication.targets` / `VSToolsPath`.** A classic web app imports `$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets`; on a bare build agent `$(VSToolsPath)` defaults wrong (`MSB4226`). | Set `VisualStudioVersion` + `VSToolsPath` in the `.csproj`. |
| 4 | **Native components are Html helpers, not Tag Helpers.** `<native-*>` Tag Helpers live in the net10-only `AlisReactive.NativeTagHelpers` package and do not exist under System.Web MVC 5. | Use `Html.InputField(plan, m => m.X).NativeTextBox(...)` / `.NativeCheckBox(...)` (`Views/Home/Index.cshtml`). |
| 5 | **Razor parses `@` inside HTML comments.** `<!-- ... @Html.RenderPlan ... -->` is compiled as code (`CS1502`). | Use Razor comments `@* ... *@` instead. |
| 6 | **Client validation** on net48 resolves `ReactiveValidator<T>` through MVC5's `DependencyResolver` — a real consumer registers it in `Global.asax` `Application_Start`. | **Not exercised by this native-only smoke** (no validator is registered here); tracked in `docs/net48-windows-verification.md`. |

## Run it locally on Windows

```powershell
npm ci; npm run build:all
bash scripts/pack.sh 1.0.0-rc.1            # builds the local rc.1 feed in ./nupkgs
msbuild examples/net48-mvc-smoke/Net48MvcSmoke.csproj /p:Configuration=Release /restore /t:Build
& "$env:ProgramFiles\IIS Express\iisexpress.exe" /path:(Resolve-Path examples/net48-mvc-smoke) /port:5000
# browse http://localhost:5000/ , tick the checkbox, the green proof box appears
```

## What it proves

Asset landing (#1) and build are green in CI today. The full browser boot + reactive interaction is
the `net48-runtime-iis` screenshot. The remaining net48 surfaces a fuller fixture should add are in
`docs/net48-windows-verification.md` (Syncfusion `.Render()`, the validation bridge, Native HtmlHelper
markup specifics).
