---
title: Build & Run
description: All build, test, and run commands for the Reactive framework.
sidebar:
  order: 3
---

All commands run from the repository root.

---

## How do I build the JS runtime?

The runtime is bundled as a single IIFE via esbuild. Entry point: `Alis.Reactive.Assets/runtime/root.ts`.

```bash
npm run build:runtime    # Bundle -> Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js
```

The sandbox serves this file at `/scripts/alis-reactive.dev.js` — the same URL
shape a net10 NuGet consumer sees (with their package version in place of `dev`).

## How do I build the CSS?

Tailwind v4, compiled via `@tailwindcss/cli`:

```bash
npm run build:design-system   # Compile -> Alis.Reactive.Assets/dist/css/design-system.dev.css
npm run build:fusion          # Compile -> Alis.Reactive.Assets/dist/css/syncfusion.dev.css
```

## How do I build everything?

```bash
npm run build:all        # framework JS + CSS (runtime, design-system, Fusion) + sandbox
```

This builds the two npm workspaces in order: `Alis.Reactive.Assets` (`build:runtime`,
`build:design-system`, `build:fusion`) then `Alis.Reactive.SandboxApp` (sandbox plugins + CSS).

## How do I watch for changes?

```bash
npm run watch:runtime          # Rebuild framework JS on file change
npm run watch:design-system    # Rebuild design-system CSS on file change
npm run watch:fusion           # Rebuild Syncfusion CSS on file change
npm run watch:sandbox-plugins  # Rebuild sandbox-plugins on change
npm run watch:sandbox-css      # Rebuild sandbox.css on change
```

## How do I typecheck?

```bash
npm run typecheck        # tsc against both tsconfigs (framework + sandbox)
```

## How do I lint?

```bash
npm run lint             # ESLint on Scripts/
npm run lint:fix         # ESLint with auto-fix
```

## How do I build the C# projects?

```bash
dotnet build             # All projects: core, native, fusion, sandbox, tests
```

---

## How do I run the tests?

### TypeScript unit tests

Vitest + jsdom. Tests runtime execution: boot, triggers, commands, resolver, conditions, validation.

```bash
npm test                 # ~944 tests, runs in seconds
```

### C# unit tests

NUnit + Verify.NUnit. Tests plan rendering, generated-contract behavior, and component vertical slices.

```bash
dotnet test tests/Alis.Reactive.UnitTests                   # Core
dotnet test tests/Alis.Reactive.Native.UnitTests            # Native components
dotnet test tests/Alis.Reactive.Fusion.UnitTests            # Fusion components
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests   # Validation extraction
```

### Playwright browser tests

Playwright.NUnit against the live SandboxApp. The test fixture starts the app automatically on port 5220.

```bash
scripts/playwright.sh                                      # observable browser test run
```

### All tests in sequence

```bash
scripts/test.sh
```

Run the full gate before push or release work. For a focused edit, use the
narrow proof from `docs/developer-cli.md`, such as `scripts/test.sh --no-e2e`
when browser behavior is intentionally out of scope.

---

## How do I run the sandbox app?

```bash
scripts/run.sh
```

Builds browser assets, then starts Kestrel on `http://localhost:5220`. The
sandbox serves framework bundles
(`/scripts/alis-reactive.dev.js`, `/css/design-system.dev.css`) directly from
`Alis.Reactive.Assets/dist/` via a `CompositeFileProvider` configured in
`Alis.Reactive.SandboxApp/Program.cs` — no copy into sandbox `wwwroot/` required.
Sandbox-specific bundles (`sandbox-plugins.js`, `sandbox.css`) live in the
standard `wwwroot/`.

---

## What is the development feedback loop?

After making changes, follow this order. Each step depends on the one before it.

### 1. Rebuild bundles

```bash
npm run build:all
```

Regenerates framework bundles in `Alis.Reactive.Assets/dist/` and sandbox-specific
bundles in `Alis.Reactive.SandboxApp/wwwroot/`. The ASP.NET `asp-append-version="true"`
tag helper computes a SHA256 hash from each served file, so browsers always
get the latest build without cache collisions.

### 2. Run TypeScript tests

```bash
npm test
```

Catches runtime logic errors without a browser.

### 3. Run C# unit tests

```bash
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
```

Catches serialization regressions and plan-domain drift.

### 4. Build all C# projects

```bash
dotnet build
```

Ensures everything compiles, including the sandbox app.

### 5. Run Playwright tests

```bash
scripts/playwright.sh --no-build
```

The test fixture starts the app automatically. These tests navigate real pages, interact with components, and assert DOM state.

---

## Why do my Playwright tests fail after a code change?

The most common cause is stale bundles. The browser loads old JavaScript because the bundles were not rebuilt.

Fix:

```bash
npm run build:all          # Rebuild JS + CSS
dotnet build               # Rebuild C# (refreshes asp-append-version hash)
scripts/playwright.sh --no-build
```

If you skip `npm run build:all`, the browser loads old JS. If you skip `dotnet build`, the server computes hashes on old files. Both cause confusing failures.

---

## Quick reference

| Task | Command |
|------|---------|
| Bundle JS | `npm run build:runtime` |
| Compile CSS | `npm run build:design-system`, `npm run build:fusion` |
| Build everything | `npm run build:all` |
| Watch JS | `npm run watch:runtime` |
| Watch CSS | `npm run watch:design-system` |
| TypeScript typecheck | `npm run typecheck` |
| Lint | `npm run lint` |
| TS unit tests | `npm test` |
| C# core tests | `dotnet test tests/Alis.Reactive.UnitTests` |
| Native component tests | `dotnet test tests/Alis.Reactive.Native.UnitTests` |
| Fusion component tests | `dotnet test tests/Alis.Reactive.Fusion.UnitTests` |
| Validation tests | `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests` |
| Browser tests | `scripts/playwright.sh` |
| Build all C# | `dotnet build` |
| Run sandbox | `scripts/run.sh` |
