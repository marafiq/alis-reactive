---
title: Build & Run
description: All build, test, and run commands for the Reactive framework.
sidebar:
  order: 3
---

All commands run from the repository root.

---

## How do I build the JS runtime?

The runtime is bundled as a single IIFE via esbuild. Entry point: `Alis.Reactive.Assets/Scripts/root.ts`.

```bash
npm run build            # Bundle -> Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js
```

The sandbox serves this file at `/scripts/alis-reactive.dev.js` — the same URL
shape a net10 NuGet consumer sees (with their package version in place of `dev`).

## How do I build the CSS?

Tailwind v4, compiled via `@tailwindcss/cli`:

```bash
npm run build:css        # Compile -> Alis.Reactive.Assets/dist/css/design-system.dev.css
```

## How do I build everything?

```bash
npm run build:all        # framework JS + framework CSS + sandbox-plugins + sandbox.css
```

This runs `build`, `build:css`, `build:sandbox-plugins`, and `build:sandbox-css` in sequence.

## How do I watch for changes?

```bash
npm run watch                  # Rebuild framework JS on file change
npm run watch:css              # Rebuild framework CSS on file change
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

NUnit + Verify.NUnit + JsonSchema.Net. Tests plan rendering, schema conformance, component vertical slices.

```bash
dotnet test tests/Alis.Reactive.UnitTests                   # Core
dotnet test tests/Alis.Reactive.Native.UnitTests            # Native components
dotnet test tests/Alis.Reactive.Fusion.UnitTests            # Fusion components
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests   # Validation extraction
```

### Playwright browser tests

Playwright.NUnit against the live SandboxApp. The test fixture starts the app automatically on port 5220.

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests             # ~483 tests, ~75 seconds
```

### All tests in sequence

```bash
npm test
dotnet test tests/Alis.Reactive.UnitTests
dotnet test tests/Alis.Reactive.Native.UnitTests
dotnet test tests/Alis.Reactive.Fusion.UnitTests
dotnet test tests/Alis.Reactive.FluentValidator.UnitTests
dotnet test tests/Alis.Reactive.PlaywrightTests
```

All must pass before every commit. No exceptions.

---

## How do I run the sandbox app?

```bash
dotnet run --project Alis.Reactive.SandboxApp
```

Starts Kestrel on `http://localhost:5220`. The sandbox serves framework bundles
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

Catches serialization regressions and schema violations.

### 4. Build all C# projects

```bash
dotnet build
```

Ensures everything compiles, including the sandbox app.

### 5. Run Playwright tests

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests
```

The test fixture starts the app automatically. These tests navigate real pages, interact with components, and assert DOM state.

---

## Why do my Playwright tests fail after a code change?

The most common cause is stale bundles. The browser loads old JavaScript because the bundles were not rebuilt.

Fix:

```bash
npm run build:all          # Rebuild JS + CSS
dotnet build               # Rebuild C# (refreshes asp-append-version hash)
dotnet test tests/Alis.Reactive.PlaywrightTests
```

If you skip `npm run build:all`, the browser loads old JS. If you skip `dotnet build`, the server computes hashes on old files. Both cause confusing failures.

---

## Quick reference

| Task | Command |
|------|---------|
| Bundle JS | `npm run build` |
| Compile CSS | `npm run build:css` |
| Bundle JS + CSS | `npm run build:all` |
| Watch JS | `npm run watch` |
| Watch CSS | `npm run watch:css` |
| TypeScript typecheck | `npm run typecheck` |
| Lint | `npm run lint` |
| TS unit tests | `npm test` |
| C# core tests | `dotnet test tests/Alis.Reactive.UnitTests` |
| Native component tests | `dotnet test tests/Alis.Reactive.Native.UnitTests` |
| Fusion component tests | `dotnet test tests/Alis.Reactive.Fusion.UnitTests` |
| Validation tests | `dotnet test tests/Alis.Reactive.FluentValidator.UnitTests` |
| Browser tests | `dotnet test tests/Alis.Reactive.PlaywrightTests` |
| Build all C# | `dotnet build` |
| Run sandbox | `dotnet run --project Alis.Reactive.SandboxApp` |
