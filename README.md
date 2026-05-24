# Alis.Reactive

Plan-driven reactive framework for ASP.NET MVC. C# fluent builders produce JSON plans executed by a browser runtime. Zero JavaScript in views.

## Packages

| Package | Description |
|---------|-------------|
| `Alis.Reactive` | Core plan model, builders, and serialization |
| `Alis.Reactive.Native` | Native HTML components (TextBox, CheckBox, DropDown, Button, etc.) |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.FluentValidator` | FluentValidation client-validation projection adapter |
| `Alis.Reactive.DesignSystem` | Design-system tokens, layout helpers, and stylesheet |
| `Alis.Reactive.NativeTagHelpers` | ASP.NET Core Tag Helpers for native components |

All library packages target `net10.0`.

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

## Repo Layout

```
Alis.Reactive/                    C# core library (packed as AlisReactive NuGet)
Alis.Reactive.Native/             C# native-component library
Alis.Reactive.Fusion/             C# Syncfusion-component library
Alis.Reactive.FluentValidator/    C# client-validation projection adapter
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
