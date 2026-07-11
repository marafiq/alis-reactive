---
title: Build and Verify
description: Commands used to build the docs site and verify framework changes.
---

Use the root script wrappers for framework work.

```bash
scripts/doctor.sh
scripts/build.sh
scripts/run.sh
scripts/test.sh
scripts/test.sh --no-e2e
scripts/pack.sh <version>
```

`scripts/test.sh` is the full gate. It runs typecheck, browser asset builds,
Vitest, .NET build, non-Playwright .NET tests, and Playwright.

## Docs Site

Run docs commands from `docs-site/`.

```bash
npm ci
npm run build
npm run dev
```

The docs deploy workflow runs `npm ci` and `npm run build` in `docs-site/`.

## Browser Proof

For page-visible framework behavior, build runtime assets and use the wrapper.

```bash
scripts/playwright.sh --filter "FullyQualifiedName~Todo"
```

Do not run Playwright through raw `dotnet test` during local framework work. The
wrapper checks browser assets and writes live diagnostics.
