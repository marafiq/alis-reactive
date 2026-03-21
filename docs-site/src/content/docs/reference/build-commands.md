---
title: Build & Run
description: All build, test, and run commands for the Reactive framework.
sidebar:
  order: 3
---

All commands run from the repository root.

---

## How do I build the JS runtime?

The runtime is bundled as a single ESM module via esbuild. Entry point: `Scripts/root.ts`.

```bash
npm run build            # Bundle -> wwwroot/js/alis-reactive.js
```

## How do I build the CSS?

Tailwind v4, compiled via `@tailwindcss/cli`:

```bash
npm run build:css        # Compile -> wwwroot/css/design-system.css
```

## How do I build everything?

```bash
npm run build:all        # JS runtime + test-widget + CSS
```

This runs `build`, `build:test-widget`, and `build:css` in sequence.

## How do I watch for changes?

```bash
npm run watch            # Rebuild JS on file change
npm run watch:css        # Rebuild CSS on file change
```

## How do I typecheck?

```bash
npm run typecheck        # tsc --noEmit against Scripts/tsconfig.json
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

Starts Kestrel on `http://localhost:5220`. The app serves bundled JS and CSS from `wwwroot/`.

---

## What is the development feedback loop?

After making changes, follow this order. Each step depends on the one before it.

### 1. Rebuild bundles

```bash
npm run build:all
```

Regenerates JS and CSS in `wwwroot/`. The ASP.NET `asp-append-version="true"` tag helper computes a SHA256 hash, so browsers always get the latest build.

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
