# Alis.Reactive

Plan-driven reactive framework for ASP.NET MVC. C# fluent builders produce JSON plans executed by a browser runtime. Zero JavaScript in views.

## Packages

| Package | Description |
|---------|-------------|
| `Alis.Reactive` | Core plan model, builders, and serialization |
| `Alis.Reactive.Native` | Native HTML components (TextBox, CheckBox, DropDown, Button, etc.) |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.FluentValidator` | FluentValidation client-validation metadata adapter |
| `Alis.Reactive.DesignSystem` | Design-system tokens, layout helpers, and stylesheet |
| `Alis.Reactive.NativeTagHelpers` | ASP.NET Core Tag Helpers for native components |

`Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.FluentValidator`, and
`Alis.Reactive.DesignSystem` multi-target **`net48` and `net10.0`** — the *same* DSL on both
runtimes (bridged by `#if` shims, never a divergent API). `Alis.Reactive.Fusion` and
`Alis.Reactive.NativeTagHelpers` are **`net10.0`-only**.

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
| `scripts/test.sh` | full gate: contract drift typecheck -> browser asset build -> vitest -> both-TFM build -> observable Playwright (`--no-e2e` skips the browser leg) |
| `scripts/playwright.sh` | observable Playwright runner with filter support, live logs, TRX, diagnostics, active-test progress markers, stale `--no-build` detection, and stale browser asset detection |
| `scripts/pack.sh <version>` | delivery: bundles → Release build → pack the six library NuGets to `./nupkgs` |

Every wrapper supports `--help`. See **[docs/developer-cli.md](docs/developer-cli.md)**
for filtered Playwright examples, first-time browser install, packaging, and the
change-to-command matrix.

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

`npm run build:all` produces JS/CSS bundles that three places consume. Every
bundle output path is gitignored — `git status` stays clean after a build.

| Consumer | How it gets the bundles |
|----------|-------------------------|
| **Sandbox** | `SandboxApp/Program.cs` serves `Alis.Reactive.Assets/dist/` directly via a `CompositeFileProvider` — no copy into `wwwroot/` |
| **NuGet** | Each asset-bearing csproj packs its bundle from `Alis.Reactive.Assets/dist/` — `AlisReactive` the runtime JS, `AlisReactive.DesignSystem` the design-system CSS, `AlisReactive.Fusion` the Syncfusion CSS. `dotnet pack` never runs npm |
| **Example app** (`examples/resident-intake/`) | Uses local project references so solution builds exercise the current source contract |

## For UI developers — the design system

Bringing the design system into a consuming app is four steps; you do **not** need to
touch the framework internals.

1. **Reference the NuGet package.** Add a **package** reference to `AlisReactive.DesignSystem`.
   Each asset package ships a per-package MSBuild targets file (`{PackageId}.targets`, generated
   from `build/AlisReactiveAssets.targets`) that, on build, copies its versioned CSS into your
   app's `wwwroot/css/` — `design-system.<version>.css` here, and `syncfusion.<version>.css` if
   you also add the `AlisReactive.Fusion` package. The version is baked into the filename.
   *(This copy fires for **package** references only. The in-repo `examples/resident-intake`
   uses local **project** references for source-contract testing, so it does not auto-copy the
   CSS that way — rebuild it against the published packages with `scripts/rebuild-example-app.sh`
   to see the packaged design system end to end.)*

2. **Link the stylesheet and mark the root** in `_Layout.cshtml` — exactly as the
   example app and sandbox do:

   ```html
   <link rel="stylesheet" href="~/css/design-system.1.0.0-preview.2.css" asp-append-version="true"/>
   <link rel="stylesheet" href="~/css/syncfusion.1.0.0-preview.2.css" asp-append-version="true"/>  <!-- if using Fusion -->
   ...
   <body class="alis-root h-full">   <!-- the design system is scoped under .alis-root -->
   ```

   The design system expects the **`alis-root`** class on the element wrapping your
   content; styles are scoped to it so they never leak into the rest of the host app.
   (The Inter web font is the design default — see the example `_Layout` for the
   `preconnect`/`font` links.)

3. **Build class strings from C#, not by hand.** `Alis.Reactive.DesignSystem` exposes
   small static layout helpers that return the right scoped classes for the current
   tokens — so spacing, elevation, and color stay consistent without memorizing utility
   names. Each takes typed token enums:

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

4. **Go deeper** in the docs site (`docs-site/`) and the
   `Alis.Reactive.DesignSystem/Layout` + `Tokens` source — the enums there are the full,
   typed vocabulary; there is no untyped string surface to discover.

## Repo Layout

```
Alis.Reactive/                    C# core library (packed as AlisReactive NuGet)
Alis.Reactive.Native/             C# native-component library
Alis.Reactive.Fusion/             C# Syncfusion-component library
Alis.Reactive.FluentValidator/    C# client-validation metadata adapter
Alis.Reactive.NativeTagHelpers/   C# tag helpers (net10 only)
Alis.Reactive.Analyzers/          Roslyn analyzers (shipped inside AlisReactive)
Alis.Reactive.DesignSystem/       C# design-system tokens + layout helpers
Alis.Reactive.Assets/             Framework browser assets — npm workspace (runtime, design-system, Fusion)
Alis.Reactive.SandboxApp/         Dev harness + live component demos
examples/resident-intake/         Source-referenced consumer example
tests/                            Test projects (NUnit + vitest + Playwright)
```

`Alis.Reactive.Assets/` is the single home for all framework browser assets — an
npm workspace holding the runtime TypeScript, the design-system CSS, and the
Syncfusion CSS. esbuild and vite build each into `dist/`; the C# packages ship
those bundles but never build them. A `NoTargets` `.csproj` makes the workspace
visible in the solution and compiles nothing.

## License

MIT
