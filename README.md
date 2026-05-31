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

A fresh clone has no installed dependencies and no built bundles. Run these
three commands from the repo root, **in order**:

```bash
npm ci                                          # 1. install JS dependencies
npm run build:all                               # 2. build the JS/CSS bundles
dotnet run --project Alis.Reactive.SandboxApp    # 3. start the sandbox
```

Open **http://localhost:5220** — the sandbox home page is the index of component demos.

Order matters: the sandbox serves the bundles produced by `build:all` and
refuses to start without them. If you skip step 2, startup throws with a
message telling you to run `npm run build:all`.

### One-shot scripts

`scripts/` wraps the canonical commands so you do not have to remember the order:

| Script | What it does |
|--------|--------------|
| `scripts/build.sh` | JS deps (if missing) → framework bundles → both-TFM C# build |
| `scripts/run.sh` | bundles → start the sandbox at `http://localhost:5220` |
| `scripts/test.sh` | full gate: vitest → both-TFM build → contract drift typecheck → Playwright (`--no-e2e` skips the browser leg) |
| `scripts/pack.sh <version>` | delivery: bundles → Release build → pack the six library NuGets to `./nupkgs` |

Each script is a thin, order-correct wrapper over the commands documented in
**[CLAUDE.md → Build & Run](CLAUDE.md#build--run)** — no hidden behavior.

## Developing

Three terminals give a live edit/refresh loop:

```bash
npm run watch:runtime                            # framework JS  — rebuild on .ts edit
npm run watch:design-system                      # framework CSS — rebuild on .css edit
dotnet watch --project Alis.Reactive.SandboxApp  # Razor + C# hot reload
```

- Edit `.ts` / `.css` under `Alis.Reactive.Assets/` → save → **browser refresh**. No restart.
- Edit `.cshtml` / `.cs` → `dotnet watch` hot-reloads automatically.

The full command reference — every build, test, and pack command — lives in
**[CLAUDE.md → Build & Run](CLAUDE.md#build--run)**, the canonical guide.

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

1. **Reference the package.** Add `AlisReactive.DesignSystem`. On build, the shipped
   `AlisReactive.targets` copies `design-system.<version>.css` into your app's
   `wwwroot/css/` (the version is baked into the filename). Add `AlisReactive.Fusion`
   too if you use Syncfusion components — it ships `syncfusion.<version>.css` the same way.

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
