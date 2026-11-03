# Alis.Reactive

Plan-driven reactive framework for ASP.NET MVC. C# fluent builders produce Reactive Plan JSON executed by the runtime. Zero JavaScript in views.

## Packages

| Package | Description |
|---------|-------------|
| `Alis.Reactive` | Core plan model, builders, serialization, and validation rules in FluentValidation's grammar (server and browser) |
| `Alis.Reactive.Native` | Native HTML components (TextBox, CheckBox, DropDown, Button, etc.) |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.DesignSystem` | The look an app chooses: tokens, stylesheets, and C# layout class helpers. No component package depends on it |
| `Alis.Reactive.NativeTagHelpers` | ASP.NET Core Tag Helpers for native components |

`Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.Fusion`, and `Alis.Reactive.DesignSystem`
multi-target **`net48` and `net10.0`** — the *same* DSL on both runtimes (bridged by `#if` shims, never a divergent API). Only `Alis.Reactive.NativeTagHelpers` is
**`net10.0`-only** (ASP.NET Core Tag Helpers).

### In your application

Register the packages you use in `Program.cs` (MVC 5 on net48: in `Global.asax` or your container's
startup). `examples/resident-intake/Program.cs` is the worked example.

```csharp
using Alis.Reactive.Fusion;

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());   // your ReactiveValidator<T> classes
builder.Services.AddReactiveFusion();                   // only with Alis.Reactive.Fusion
```

`AddReactiveFusion()` sets the Newtonsoft naming Syncfusion renders with: your data sources reach the
browser in camel case, so `Fields<TItem>(...)` finds each item's members, and the names Syncfusion pins
(the toast's and dialog's `X`/`Y` position) keep their spelling. Call it after any line of yours that
assigns `JsonConvert.DefaultSettings`; it keeps the rest of those settings. Without it, a plain
camel-case resolver renames the pinned names and the toast falls back to the top left.

## Getting Started

Prerequisites: **.NET SDK 10.0.x** and **Node.js 22+**.

A fresh clone has no installed dependencies and no built bundles. Run these from
the repo root:

```bash
scripts/doctor.sh
scripts/build.sh
scripts/run.sh
```

Open **http://localhost:5220** — the sandbox home page is the index of component demos.

`scripts/build.sh` installs npm dependencies if needed, builds the browser
bundles, and runs `dotnet build`. `scripts/run.sh` rebuilds bundles before
starting the sandbox so it does not serve stale browser assets.

### CLI Commands

`docs/developer-cli.md` is the canonical command guide. The root wrappers keep
build, test, Playwright, and packaging order explicit:

| Script | What it does |
|--------|--------------|
| `scripts/doctor.sh` | read-only CLI preflight: tools, script executability, restore/build-output hints, git status |
| `scripts/build.sh` | JS deps (if missing) → framework bundles → both-TFM C# build |
| `scripts/run.sh` | bundles → start the sandbox at `http://localhost:5220` |
| `scripts/test.sh` | full gate: contract drift typecheck -> browser asset build -> vitest -> both-TFM build -> non-Playwright dotnet tests -> observable Playwright (`--no-e2e` skips the browser leg) |
| `scripts/playwright.sh` | observable Playwright runner with filter support, live logs, TRX, diagnostics, active-test progress markers, stale `--no-build` detection, and stale browser asset detection |
| `scripts/pack.sh <version>` | delivery: bundles → Release build → pack the five library NuGets to `./nupkgs` (local; releases are cut by tag, see [docs/RELEASING.md](docs/RELEASING.md)) |

Every wrapper supports `--help`. See **[docs/developer-cli.md](docs/developer-cli.md)**
for filtered Playwright examples, first-time browser install, packaging, and the
command to run for each change type.

## Developing

For UI work, use the sandbox as the visual workbench. Build once, then start the
sandbox:

```bash
scripts/build.sh
scripts/run.sh
```

For a live edit/refresh loop, run the watcher that owns the asset you are
editing:

```bash
npm run watch:runtime                            # framework runtime TS
npm run watch:design-system                      # framework design-system CSS
npm run watch:fusion                             # framework Fusion CSS
npm run watch:sandbox-plugins                    # sandbox-only plugin JS
npm run watch:sandbox-css                        # sandbox-only CSS
dotnet watch --project Alis.Reactive.SandboxApp  # Razor + C# hot reload
```

Refresh the browser after TS/CSS watcher output changes. Razor and C# changes
flow through `dotnet watch`. UI proof usually starts with a narrow browser test,
for example:

```bash
scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion"
```

The full command reference lives in **[docs/developer-cli.md](docs/developer-cli.md)**.

## How the bundles ship

`npm run build:all` builds the browser bundles into `Alis.Reactive.Assets/dist/` (gitignored).
Each package ships its own files the way a Razor class library does: as static web assets.

| Consumer | How it gets the files |
|----------|-----------------------|
| **Sandbox** and `examples/resident-intake` | Project references. In Development the app serves each file at `~/_content/{PackageId}/...` from where npm wrote it, so a watcher's rebuild shows on the next request |
| **ASP.NET Core app (net10.0)** | Package references. The same `~/_content/{PackageId}/...` paths; publish copies the files under `wwwroot/_content/` |
| **ASP.NET MVC 5 app (net48)** | Package references. Each package's `build\net48\{PackageId}.targets` copies its files into the web project at `Content\{PackageId}\` after every build |

`dotnet pack` never runs npm, and refuses to pack a package whose bundle is missing.

## Browser assets: what each package ships and where it lands

Components ship the styling their own markup needs. The look (color, type, the brand) is the
design system's: an app references `AlisReactive.DesignSystem`, or another design system that sets
the same tokens (`--color-border`, `--color-accent`, `--font-display`, ...), and the components
follow it. Without one, their stylesheets fall back to neutral colors and the page's font.

| Package | File | net10.0 URL | net48 file in the web project |
|---------|------|-------------|-------------------------------|
| `AlisReactive` | runtime script | `~/_content/AlisReactive/js/alis-reactive.js` | `Content\AlisReactive\js\alis-reactive.js` |
| `AlisReactive.Native` | drawer, loader, check list, radio group | `~/_content/AlisReactive.Native/css/native.css` | `Content\AlisReactive.Native\css\native.css` |
| `AlisReactive.NativeTagHelpers` | `<native-*>` layout tag helpers | `~/_content/AlisReactive.NativeTagHelpers/css/native-tag-helpers.css` | (net10.0 only) |
| `AlisReactive.Fusion` | Syncfusion Tailwind3 theme, grid dialog form | `~/_content/AlisReactive.Fusion/css/fusion.css` | `Content\AlisReactive.Fusion\css\fusion.css` |
| `AlisReactive.DesignSystem` | tokens, page base, field and validation look | `~/_content/AlisReactive.DesignSystem/css/design-system.css` | `Content\AlisReactive.DesignSystem\css\design-system.css` |
| `AlisReactive.DesignSystem` | the brand on Syncfusion components | `~/_content/AlisReactive.DesignSystem/css/design-system-fusion.css` | `Content\AlisReactive.DesignSystem\css\design-system-fusion.css` |

Link them in this order: the design system, the component stylesheets, Fusion's, then the design
system's Fusion layer (its values must follow the Syncfusion theme's), then your own CSS.

```html
<link rel="stylesheet" href="~/_content/AlisReactive.DesignSystem/css/design-system.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.Native/css/native.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.NativeTagHelpers/css/native-tag-helpers.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.Fusion/css/fusion.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.DesignSystem/css/design-system-fusion.css" asp-append-version="true"/>
...
<script src="~/_content/AlisReactive/js/alis-reactive.js" asp-append-version="true"></script>
```

The app serves them with `app.UseStaticFiles()`, as the sandbox does. Running from build
output outside Development needs `builder.WebHost.UseStaticWebAssets()`; a published app does not.

On net48, link the copied files with `Url.Content("~/Content/AlisReactive/js/alis-reactive.js")`.
They are build output: keep `Content\AlisReactive*\` out of source control, and include the folder
as `Content` in the web project so publish deploys it. MVC 5 has no `asp-append-version`; bundle
them with System.Web.Optimization or add the package version to the URL to bust caches on upgrade.

## For UI developers — the design system

Bringing the design system into a consuming app is three steps; you do **not** need to
touch the framework internals.

1. **Reference the package.** Add a reference to `AlisReactive.DesignSystem` next to the
   component packages you use. The component packages do not bring it: it is the app's choice.

2. **Link the stylesheets and mark the root** in `_Layout.cshtml`, in the order shown above, and
   put the **`alis-root`** class on the element wrapping your content; the design system's page
   styles are scoped to it so they never leak into the rest of the host app. (The Inter web font is
   the design default; see the sandbox `_Layout` for the `preconnect`/`font` links.)

   ```html
   <body class="alis-root h-full">
   ```

3. **Build class strings from C#, not by hand.** `Alis.Reactive.DesignSystem` exposes
   small static layout helpers that return the design system's utility classes for the current
   tokens, so spacing, elevation, and color stay consistent without memorizing utility names.
   Each takes typed token enums:

   | Helper (in `DesignSystem/Layout`) | Returns classes for | Example tokens |
   |---|---|---|
   | `CardCss`, `ContainerCss`, `DividerCss` | cards, page containers, dividers | `CardElevation`, `CardPadding`, `CardDivider` |
   | `GridCss`, `HStackCss`, `VStackCss` | grid + flex stacks | `GridCols`, `JustifyContent`, `AlignItems`, `SpacingScale` |
   | `HeadingCss`, `TextCss`, `KvCss` | headings, body text, key/value rows | `HeadingLevel`, `TextSize`, `TextColor`, `AccentColor` |

   ```razor
   <div class="@CardCss.CardClasses(CardElevation.Low)">
     <div class="@CardCss.BodyClasses(CardPadding.Standard)">…</div>
   </div>
   ```

   The `<native-*>` tag helpers do not use these: they write their own class names, styled by
   `native-tag-helpers.css`, and take their own enums (`Alis.Reactive.NativeTagHelpers.*`).

## Repo Layout

```
Alis.Reactive/                    C# core library (packed as AlisReactive NuGet)
Alis.Reactive.Native/             C# native-component library
Alis.Reactive.Fusion/             C# Syncfusion-component library
Alis.Reactive.NativeTagHelpers/   C# tag helpers (net10 only)
Alis.Reactive.Analyzers/          Roslyn analyzers (shipped inside AlisReactive)
Alis.Reactive.DesignSystem/       C# design-system tokens + layout class helpers; ships the design-system CSS
Alis.Reactive.Assets/             Framework browser assets — npm workspace (runtime, components, CSS)
Alis.Reactive.SandboxApp/         Dev harness + live component demos
examples/resident-intake/         Source-referenced consumer example
tests/                            Test projects (NUnit + vitest + Playwright)
```

`Alis.Reactive.Assets/` is the single home for all framework browser assets — an
npm workspace holding the runtime TypeScript, the design-system CSS, the Syncfusion
CSS, and the stylesheets the component packages ship as written. esbuild and vite
build the bundles into `dist/`; the C# packages ship those files as static web assets
(`build/AlisReactiveStaticWebAssets.targets`) but never build them. A `NoTargets`
`.csproj` makes the workspace visible in the solution and compiles nothing.

## License

MIT
