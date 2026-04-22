# Alis.Reactive

Plan-driven reactive framework for ASP.NET MVC. C# fluent builders produce JSON plans executed by a browser runtime. Zero JavaScript in views.

## Packages

| Package | Description |
|---------|-------------|
| `Alis.Reactive` | Core plan model, builders, and serialization |
| `Alis.Reactive.Native` | Native HTML components (TextBox, CheckBox, DropDown, Button, etc.) |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.FluentValidator` | FluentValidation rule extraction to client-side validation |
| `Alis.Reactive.NativeTagHelpers` | ASP.NET Core Tag Helpers for native components |

## Target Frameworks

All library packages target both `net48` and `net10.0` (except NativeTagHelpers which is `net10.0` only).

## Repo Layout

```
Alis.Reactive/                    C# library (packed as AlisReactive NuGet)
Alis.Reactive.Native/             C# native-component library
Alis.Reactive.Fusion/             C# Syncfusion-component library
Alis.Reactive.FluentValidator/    C# validator-extraction library
Alis.Reactive.NativeTagHelpers/   C# tag helpers (net10 only)
Alis.Reactive.Analyzers/          Roslyn analyzers (shipped with AlisReactive)

Alis.Reactive.Assets/             Framework JS + CSS source (pure npm package, no csproj)
├── Scripts/                      TypeScript source (runtime)
├── Styles/                       Tailwind input + component CSS
└── dist/                         esbuild + tailwind output (gitignored)
    ├── scripts/alis-reactive.dev.js
    └── css/design-system.dev.css

Alis.Reactive.SandboxApp/         Dev harness + breathing example (consumer of AlisReactive)
├── Scripts/sandbox-plugins.ts    Sandbox-only bundle
├── Styles/sandbox.css            Sandbox-only Tailwind utilities
├── Views/                        Razor views demonstrating the framework
└── wwwroot/                      sandbox bundles land here; framework bundles are served
                                  directly from Alis.Reactive.Assets/dist/ via a
                                  CompositeFileProvider in Program.cs

tests/                            All test projects (NUnit + vitest)
```

The framework's JS runtime lives in `Alis.Reactive.Assets/` — a sibling folder with its
own `package.json`. No csproj. No MSBuild orchestration of npm. This mirrors how
`dotnet/aspnetcore` ships Blazor's `Web.JS` and SignalR's TypeScript client.

## Prerequisites

- .NET SDK 10.0.x
- Node.js 22+ and npm
- (First run only) `pwsh tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium`

## Quickstart: run the sandbox

Three commands, in any order, each in its own terminal. The sandbox serves the
framework JS/CSS directly from `Alis.Reactive.Assets/dist/` via a
`CompositeFileProvider` (no copy to `wwwroot`). Same URL structure the NuGet
consumer experience produces on their side via `AlisReactive.targets`.

```bash
# Terminal 1 — install JS deps + build bundles once
npm ci
npm run build:all

# Terminal 2 — run the sandbox
dotnet run --project Alis.Reactive.SandboxApp
# → http://localhost:5220
```

The sandbox layout loads:

```html
<link rel="stylesheet" href="~/css/design-system.dev.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/css/sandbox.css"         asp-append-version="true"/>
<script src="~/scripts/alis-reactive.dev.js"            asp-append-version="true"></script>
<script src="~/js/sandbox-plugins.js"                   asp-append-version="true"></script>
```

A net10 NuGet consumer's layout uses the same pattern — only the version token
differs (`~/scripts/alis-reactive.{pkg-version}.js`, `~/css/design-system.{pkg-version}.css`).

## Watch mode: fast edit/refresh loop

For framework development, run three parallel watchers (one per terminal):

```bash
# Terminal 1 — esbuild --watch rebuilds alis-reactive.dev.js on every TS edit (~50ms)
npm run watch

# Terminal 2 — tailwind --watch rebuilds design-system.dev.css on every CSS/Razor edit
npm run watch:css

# Terminal 3 — dotnet watch rebuilds Razor/C# and reloads the browser via ASP.NET Core hot reload
dotnet watch --project Alis.Reactive.SandboxApp
# → http://localhost:5220
```

Edit loop:

- **Edit `.ts` in `Alis.Reactive.Assets/Scripts/`** → `npm run watch` rewrites the bundle → browser refresh serves new bytes.
- **Edit `.css` in `Alis.Reactive.Assets/Styles/`** → `npm run watch:css` rewrites `design-system.dev.css` → browser refresh.
- **Edit `.cshtml` or `.cs`** → `dotnet watch` hot-reloads (or rebuilds + re-serves).

Sandbox-only bundles also have watchers if you edit them:

```bash
npm run watch:sandbox-plugins   # rebuilds SandboxApp/wwwroot/js/sandbox-plugins.js
npm run watch:sandbox-css       # rebuilds SandboxApp/wwwroot/css/sandbox.css
```

## Build everything once

```bash
npm ci
npm run build:all            # TS → dist/scripts, CSS → dist/css, sandbox bundles → wwwroot
dotnet build                 # all C# projects
```

## Run tests

Always kill any lingering dev sandbox first (`lsof -ti:5220 | xargs kill -9`)
so Playwright's Kestrel port selection works.

```bash
# .NET unit tests
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.Analyzers.Tests
dotnet test tests/Alis.Reactive.DesignSystem.Tests
dotnet test tests/Alis.Reactive.NativeTagHelpers.Tests

# Playwright (end-to-end, browser-driven)
dotnet build                 # Playwright fixture starts SandboxApp via `dotnet run`; prebuild keeps startup under 30s
dotnet test tests/Alis.Reactive.PlaywrightTests \
    --logger "console;verbosity=detailed"

# TypeScript
npm run typecheck            # both tsconfigs (framework + sandbox)
npm run lint
npm test                     # vitest
```

## Pack the NuGet

```bash
npm ci
npm run build:all            # writes Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js + dist/css/design-system.dev.css
dotnet build --configuration Release
dotnet pack Alis.Reactive/Alis.Reactive.csproj \
    --configuration Release --no-build \
    --output ./nupkgs
# → nupkgs/AlisReactive.<version>.nupkg
```

`dotnet pack` does **not** invoke npm. If the bundles are missing,
`VerifyBundlesExistBeforePack` fails fast with a clear message. CI or the
developer runs `npm run build:all` first; pack just packages the output.

The NuGet ships unversioned bundle names (`build/assets/js/alis-reactive.js`,
`build/assets/css/design-system.css`, with the same files mirrored under
`buildTransitive/`). The shipped `AlisReactive.targets` file copies them into
the consumer's `wwwroot/scripts/` (net10) or `Content/alisreactive/` (net48)
with the package version baked into the filename.

## Gitignore hygiene

After any `npm run build:all`, `git status` must stay clean. The ignored output paths:

- `Alis.Reactive.Assets/dist/` (framework bundles)
- `Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js` (sandbox bundle)
- `Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css` (sandbox bundle)

If any of the above shows up in `git status`, something in the build pipeline
is writing to a tracked path — file an issue; do not `git add`.

## License

MIT
