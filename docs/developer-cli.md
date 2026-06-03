# Developer CLI

Canonical command reference for framework developers and LLM agents. Run every
command from the repository root.

## Quick Start

```bash
scripts/doctor.sh   # read-only environment and stale-output hints
scripts/build.sh    # npm deps -> browser assets -> dotnet build
scripts/run.sh      # browser assets -> sandbox at http://localhost:5220
```

Prerequisites: .NET SDK from `global.json`, Node.js 22 or newer, npm, and `pwsh`
for the first Playwright browser install.

First Playwright browser install:

```bash
dotnet build tests/Alis.Reactive.PlaywrightTests
pwsh tests/Alis.Reactive.PlaywrightTests/bin/Debug/net10.0/playwright.ps1 install --with-deps chromium
```

## Command Table

| Task | Command | Notes |
|------|---------|-------|
| Check CLI environment | `scripts/doctor.sh` | Read-only. Reports missing tools, missing bundles, missing test DLL, and dirty git state. |
| Build everything | `scripts/build.sh` | Installs npm deps if needed, builds all JS/CSS bundles, then runs `dotnet build`. |
| Run sandbox | `scripts/run.sh` | Builds assets first, then starts `http://localhost:5220`. Stop with `Ctrl+C`. |
| Full verification gate | `scripts/test.sh` | Typecheck -> assets -> vitest -> `dotnet build` -> non-Playwright dotnet tests -> observable Playwright. |
| Non-browser gate | `scripts/test.sh --no-e2e` | Same as full gate without Playwright. |
| Full Playwright | `scripts/playwright.sh` | Use this instead of raw `dotnet test` for browser tests. |
| Filtered Playwright | `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion.Grid"` | Supports any VSTest filter. |
| Pack NuGets | `scripts/pack.sh <version>` | Builds assets and Release binaries, clears old packages from `./nupkgs`, then packs the six shipped NuGets. |

Every wrapper supports `--help`.

## UI Developer Workflows

Use the sandbox as the visual workbench. Start from:

```bash
scripts/build.sh
scripts/run.sh
```

Open `http://localhost:5220`.

For live UI work, run the relevant watcher plus the sandbox:

| UI change | Watch command | Also run |
|-----------|---------------|----------|
| Framework runtime TypeScript | `npm run watch:runtime` | `dotnet watch --project Alis.Reactive.SandboxApp` |
| Framework design-system CSS | `npm run watch:design-system` | `dotnet watch --project Alis.Reactive.SandboxApp` |
| Framework Fusion CSS | `npm run watch:fusion` | `dotnet watch --project Alis.Reactive.SandboxApp` |
| Sandbox-only plugin JS | `npm run watch:sandbox-plugins` | `dotnet watch --project Alis.Reactive.SandboxApp` |
| Sandbox-only CSS | `npm run watch:sandbox-css` | `dotnet watch --project Alis.Reactive.SandboxApp` |
| Razor or C# view/demo changes | none | `dotnet watch --project Alis.Reactive.SandboxApp` |

Browser refresh is enough after TS/CSS watcher output changes. Restart the
sandbox only when the process exits or a non-hot-reloadable C# change requires it.

Asset ownership:

| Asset | Source | Output | Ships to consumers |
|-------|--------|--------|--------------------|
| Runtime JS | `Alis.Reactive.Assets/runtime/` | `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js` | yes, `AlisReactive` |
| Design-system CSS | `Alis.Reactive.Assets/design-system/` | `Alis.Reactive.Assets/dist/css/design-system.dev.css` | yes, `AlisReactive.DesignSystem` |
| Fusion CSS | `Alis.Reactive.Assets/fusion/` | `Alis.Reactive.Assets/dist/css/syncfusion.dev.css` | yes, `AlisReactive.Fusion` |
| Sandbox plugin JS | `Alis.Reactive.SandboxApp/Scripts/` | `Alis.Reactive.SandboxApp/wwwroot/js/sandbox-plugins.js` | no |
| Sandbox CSS | `Alis.Reactive.SandboxApp/Styles/` | `Alis.Reactive.SandboxApp/wwwroot/css/sandbox.css` | no |

UI proof commands:

```bash
npm run build:all
scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion"
scripts/playwright.sh --filter "FullyQualifiedName~Validation"
scripts/test.sh --no-e2e
```

Pick the narrow Playwright filter that matches the view or component you changed,
then run the full `scripts/test.sh` before push or release work.

## Test Rules

Use `scripts/test.sh` before push. It runs the gates in this order:

```text
npm run typecheck -> npm run build:all -> npm test -> dotnet build -> non-Playwright dotnet tests -> scripts/playwright.sh --no-build
```

Use `scripts/test.sh --no-e2e` only when the browser leg is intentionally out of
scope. Before merge or release work, run the full gate.

Set `CONFIGURATION=Release` when you need the .NET build/test legs to match the
GitHub publish gate:

```bash
CONFIGURATION=Release scripts/test.sh --no-e2e
```

Use `scripts/playwright.sh` for all browser runs. The wrapper prints the active
filter, logs the exact `dotnet test` command, writes live output, TRX, and VSTest
diagnostics under:

```text
tests/Alis.Reactive.PlaywrightTests/TestResults/observable/
```

Progress markers identify the active test:

```text
[playwright:start] ... Fully.Qualified.Test.Name
[playwright:end]   ... Status ... Fully.Qualified.Test.Name
```

If output appears stuck, the most recent `[playwright:start]` line is the test
to inspect or re-run with `--filter`.

## Change-To-Command Matrix

| Change | Minimum focused proof | Before push |
|--------|------------------------|-------------|
| C# DSL, plan domain, or generated TS shape | `npm run typecheck` plus the relevant C# or Playwright slice | `scripts/test.sh` |
| Runtime TypeScript | `npm test`, `npm run build:all`, then relevant `scripts/playwright.sh --filter ...` | `scripts/test.sh` |
| Validation behavior | `scripts/playwright.sh --filter "FullyQualifiedName~Validation"` after assets and build are fresh | `scripts/test.sh` |
| Fusion component or grid behavior | `scripts/playwright.sh --filter "FullyQualifiedName~Components.Fusion"` or narrower | `scripts/test.sh` |
| Packaging or asset delivery | `scripts/pack.sh <version>` | `CONFIGURATION=Release scripts/test.sh --no-e2e`, full Playwright when behavior changed, plus package inspection when publishing |

## Stale-Output Rules

- Do not run raw `dotnet test` for Playwright. It is not observable enough.
- Do not use Playwright `--no-build` after editing C#, Razor, project files, or
  browser assets unless a fresh build has already completed.
- `scripts/playwright.sh --no-build` fails if C#/Razor sources are newer than
  the Playwright test DLL.
- `scripts/playwright.sh` fails if runtime, validation, CSS, or sandbox asset
  sources are newer than their built outputs.
- `dotnet pack` never runs npm. Use `scripts/pack.sh <version>` so assets are
  built before packages are created.

For a stale manual sandbox on port 5220, stop its terminal with `Ctrl+C`. On
macOS/Linux:

```bash
lsof -ti:5220 | xargs kill -9
```
