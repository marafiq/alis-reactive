# Plan: Consolidate the Asset Pipeline — one workspace, one build, obvious to every dev

Status: PROPOSED — revised after Gate 1 review rounds 1 and 2. Round 2: code-reviewer
SIGN-OFF, independent CHANGES REQUESTED (2 real findings), Codex review did not return a
verdict. The round-2 findings — non-existent CI test projects and a dropped sandbox vitest
path — are fixed here and were re-verified by hand against `git ls-files`. See the two
"resolution" tables at the end.
Date: 2026-05-16
Layer: 1 (C# packaging) + build pipeline (npm/esbuild/vite) + CI/CD. No schema / TS runtime
logic / plan-shape changes — files move, build wiring changes, behavior is preserved.

## Review round 1 — what changed in this revision

- **Phasing rebuilt.** CI/CD fixes are now **Phase 0**, landing first and independently. Bundle
  output paths stay put until a single **Phase 3 cutover**, so `Program.cs` is edited exactly
  once and every intermediate phase is genuinely green.
- **`build:all` is kept, not renamed.** Both `nuget-publish.yml` and `deploy-docs.yml` call it;
  renaming churned two workflows for no gain. The name stays.
- **`@source` path correction.** `app.css` moves to the same folder depth — its four `@source`
  lines are **unchanged**. Round 1's `../../../` proposal was wrong.
- **`@import` chains added.** `syncfusion.entry.css` and `sandbox.css` both `@import` the
  design-system `brand.css`; both recomputes are now explicit.
- Complete **17-row npm script table**, **dependency-destination table**, all **3 edit sites**
  in each of `Alis.Reactive.csproj` and `eslint.config.js`, `deploy-docs.yml`, and
  `package-lock.json` added to scope.
- The non-existent `runtime/__tests__/` corrected; `tsconfig.json` "moved from root" corrected
  (there is no root `tsconfig.json`); the NoTargets-vs-solution-folder choice justified; the
  design-system C#/CSS cohesion cost stated honestly.

## Why this plan

The `2026-05-15-design-system-extraction` correctly split the design system into its own NuGet
package, but cut cohesion on the **package** axis and left the **build** axis fragmented:

- `Alis.Reactive.Assets/` holds the TS runtime but is not a real project — an orphan folder.
  `Alis.Reactive.csproj` reaches *sideways* (`..\Alis.Reactive.Assets\dist\...`) to pack it.
- `Alis.Reactive.DesignSystem/` and `Alis.Reactive.Fusion/` each hold their own CSS source +
  `dist/` and pack themselves — a third, different shape.
- Build configs are scattered loose at the repo root (`vite.config.ts`, `vite.fusion.config.ts`,
  `vite.sandbox.config.ts`, `vitest.config.ts`), invisible in the solution; esbuild has no
  config file at all — an inline CLI string in `package.json`.
- A front-end dev cannot answer "where do I change the design system, and how do I build it"
  without reading three projects and the root `package.json`.

Three CI/CD gaps shipped with the extraction (evidence: `.github/workflows/nuget-publish.yml`):

1. `pack-and-publish` packs 6 projects — **not `Alis.Reactive.DesignSystem`** (lines 176-181).
   `AlisReactive.NativeTagHelpers` now depends on the `AlisReactive.DesignSystem` package
   (`Alis.Reactive.NativeTagHelpers.csproj` `ProjectReference`), so as published it has a
   **dangling dependency NuGet.org never received** — a consumer `dotnet restore` fails today.
2. The `test` job runs 5 of the 7 tracked unit-test projects under `tests/` (lines 54-58) —
   missing exactly `Alis.Reactive.DesignSystem.Tests` and `Alis.Reactive.NativeTagHelpers.Tests`.
   (Verified: `git ls-files 'tests/**/*.csproj'` lists 9 test csprojs — 7 unit, plus
   `PlaywrightTests` and the `Playwright.Extensions` helper lib. `tests/` also has
   `DriftDetection.Tests/` and `Net48.SmokeTest/` directories, but they hold only stale
   `bin/obj` output — no tracked `.csproj`, absent from `.slnx`, not real projects.)
3. The `paths:` push trigger omits `Alis.Reactive.DesignSystem/**` (lines 9-19) — editing the
   design-system C# does not trigger a publish. `deploy-docs.yml` has the same omission.

## Naming

The workspace is **`Alis.Reactive.Assets`** — reused, not invented. "Frontend" is wrong: the
whole framework is a frontend solution. "Assets" is precise — the shippable browser bundles —
and non-redundant. We restore that folder to wholeness, we do not rename.

## Core principle — two axes, kept separate

The extraction's mistake was forcing two independent axes to align. They must not.

- **Packaging axis** — the runtime, the design system, and Fusion ship as *separate* NuGet
  packages, for real dependency hygiene (a tag-helper consumer wants the CSS, not the runtime).
  This split is correct and **stays**.
- **Build axis** — there is exactly one frontend toolchain: npm, esbuild, vite, Tailwind,
  one `node_modules`. It must live in **one place**.

> A C# project is pure C#. It *ships* a browser bundle; it does not *build* one.
> **Framework** assets build in one workspace (`Alis.Reactive.Assets`); **each app** owns its
> own asset build (the sandbox workspace builds sandbox-only assets). The rule is *one
> workspace per build-owning unit* — not *one workspace, full stop*. Each NuGet package packs
> the one bundle it is responsible for, from the asset workspace's `dist/`, by one shared rule.

## Target structure

### Before

```
repo/
├── package.json                  17 npm scripts; esbuild inline as a CLI string
├── vite.config.ts                builds DesignSystem CSS   ─┐
├── vite.fusion.config.ts         builds Fusion CSS          ├─ loose at root,
├── vite.sandbox.config.ts        builds Sandbox CSS         │  invisible in .slnx
├── vitest.config.ts              runtime + sandbox tests   ─┘
├── eslint.config.js
├── Alis.Reactive.Assets/
│   ├── Scripts/                  TS runtime — orphan folder, no owning project
│   │   └── tsconfig.json
│   └── dist/scripts/
├── Alis.Reactive/                C# core; csproj packs ..\Alis.Reactive.Assets\dist\  ← sideways
├── Alis.Reactive.DesignSystem/   Tokens/ Layout/ (C#)  +  Styles/ (CSS)  +  dist/css/
├── Alis.Reactive.Fusion/         *.cs                  +  Styles/ (CSS)  +  dist/css/
└── Alis.Reactive.SandboxApp/     Scripts/ Styles/ wwwroot/
```

### After

```
repo/
├── package.json                  workspaces array + thin orchestration scripts ONLY
├── eslint.config.js              repo-wide lint policy
├── Directory.Build.props         defines $(AlisAssetsDist)
├── build/AlisReactiveAssets.targets
│                                 └─ root build config surfaced in a /build/ .slnx folder
│
├── Alis.Reactive.Assets/             ◄── npm workspace: ALL framework browser assets
│   ├── package.json                  build · build:runtime · build:design-system ·
│   │                                 build:fusion · watch:* · test · typecheck
│   ├── tsconfig.json                 TS config for runtime/   (moved from Scripts/tsconfig.json)
│   ├── vitest.config.ts              runtime unit tests       (moved from repo root)
│   ├── esbuild.config.mjs            runtime/root.ts        → dist/scripts/alis-reactive.dev.js
│   ├── vite.design-system.config.ts  design-system/app.css  → dist/css/design-system.dev.css
│   ├── vite.fusion.config.ts         fusion/syncfusion…css  → dist/css/syncfusion.dev.css
│   ├── README.md                     ◄── the map: what is here, how to build, where it ships
│   ├── Alis.Reactive.Assets.csproj   NoTargets — makes the workspace a visible solution node
│   │
│   ├── runtime/        TS runtime — root.ts, core/, execution/, resolution/, conditions/,
│   │                   lifecycle/, validation/, components/, types/
│   │                   (vitest is configured; there are no test files on this branch yet)
│   ├── design-system/  app.css  base.css  brand.css  components/  validation.css
│   ├── fusion/         syncfusion.entry.css  syncfusion.overrides.css
│   └── dist/           build output — gitignored:  scripts/*.js   css/*.css
│
├── Alis.Reactive/              → NuGet AlisReactive             pure C#
├── Alis.Reactive.DesignSystem/ → NuGet AlisReactive.DesignSystem pure C# (Tokens/, Layout/)
├── Alis.Reactive.Fusion/       → NuGet AlisReactive.Fusion       pure C#
│        each csproj packs its one bundle from $(AlisAssetsDist) — nothing else frontend
│
└── Alis.Reactive.SandboxApp/       ◄── npm workspace: the app, with its own scripts
    ├── package.json                build · watch · test · typecheck
    ├── vite.config.ts              Styles/sandbox.css      → wwwroot/css/sandbox.css
    ├── esbuild.config.mjs          Scripts/sandbox-plugins → wwwroot/js/sandbox-plugins.js
    ├── vitest.config.ts            sandbox tests (the second include from the old root config)
    ├── Scripts/tsconfig.json       (stays — the sandbox's own TS config)
    ├── Scripts/  Styles/  wwwroot/
    └── Program.cs                  ONE CompositeFileProvider over Alis.Reactive.Assets/dist/
```

## The asset pipeline — inputs and outputs

Each stage is a pure `input → process → output`. No stage reaches into another project's tree.

| Stage | Input | Process | Output | Consumed by |
|---|---|---|---|---|
| Build runtime | `Alis.Reactive.Assets/runtime/root.ts` (+ imports) | `esbuild.config.mjs` — `build:runtime` | `dist/scripts/alis-reactive.dev.js` | sandbox · `AlisReactive` pack |
| Build design system | `design-system/app.css` + `@source` C# scan | `vite.design-system.config.ts` — `build:design-system` | `dist/css/design-system.dev.css` | sandbox · `AlisReactive.DesignSystem` pack |
| Build Fusion | `fusion/syncfusion.entry.css` (+ EJ2 theme + `@import` brand.css) | `vite.fusion.config.ts` — `build:fusion` | `dist/css/syncfusion.dev.css` | sandbox · `AlisReactive.Fusion` pack |
| Serve locally | `Alis.Reactive.Assets/dist/**` | one `CompositeFileProvider` (`Program.cs`) | `/scripts/*.js`, `/css/*.css` | browser at `localhost:5220` |
| Pack a package | `dist/<bundle>` + `<Pkg>.dll` | `dotnet pack <Pkg>.csproj` | `<Pkg>.nupkg` | NuGet.org |
| Deliver to consumer | `<Pkg>.nupkg` | `build/AlisReactiveAssets.targets` on consumer build | `wwwroot/{scripts,css}/<name>.<version>.<ext>` | the consumer app |

`$(AlisAssetsDist)` — set once in `Directory.Build.props` to `Alis.Reactive.Assets/dist` — is
the single named handoff point between npm output and `dotnet pack`. One definition, three
consumers. It is target-framework invariant, so `net48` and `net10.0` packs both resolve it.

## The design-system cohesion cost (stated honestly)

This plan moves the design-system **CSS** out of `Alis.Reactive.DesignSystem/` (which keeps
only the C# `Tokens/` and `Layout/` helpers). After this plan, "the design system" spans two
folders: C# in `Alis.Reactive.DesignSystem/`, CSS in `Alis.Reactive.Assets/design-system/`.
The `app.css` `@source "../../Alis.Reactive.DesignSystem/**/*.cs"` scan is the proof they stay
coupled — the CSS build reads the C# project.

This reverses the cohesion choice the `2026-05-15` extraction made one day earlier. It is a
deliberate, user-directed trade — **build cohesion is chosen over folder cohesion** because the
build toolchain is what a developer edits and must be confident about, and the C# helpers are a
thin layer. The cost is real: two folders for one concept. **Mitigation:** `Alis.Reactive.Assets/README.md`
and a short note in `Alis.Reactive.DesignSystem/`'s own docs each cross-link to the other, so
either entry point leads to the whole picture.

## Visibility — what shows in the solution, and why two mechanisms

- **`Alis.Reactive.Assets.csproj`** (a `NoTargets` project, already added to `Alis.Reactive.slnx`
  on 2026-05-16) globs `runtime/**`, `design-system/**`, `fusion/**`, `*.json`, `*.mjs`,
  `*.config.ts`, `*.md`. The whole asset workspace is one expandable node in Rider; new files
  appear automatically.
- A **`/build/` solution folder** in `Alis.Reactive.slnx` surfaces the genuinely repo-global
  files: `package.json`, `eslint.config.js`, `Directory.Build.props`, `build/AlisReactiveAssets.targets`.

**Why two mechanisms.** A `.slnx` `<Folder>` holds individual `<File>` entries — it cannot
glob, so it *drifts* (the existing `docs-site` folder in `Alis.Reactive.slnx` already lists a
stale subset). The asset workspace has many files that change often → it needs a globbing
**project**; the NoTargets cost is one no-op restore. The `/build/` set is four fixed files
that never drift → an explicit `<File>` list is correct and lighter there.

## Developer experience

The test of this plan: a developer knows what to do without asking.

### Root passthrough scripts — no `-w` flag needed

The root `package.json` keeps thin passthrough scripts so every command works from the repo
root with no workspace flag: `npm run watch:design-system`, `npm run build:all`, `npm test`,
`npm run typecheck` all run from root. `-w` is an implementation detail a dev never types.

### "I am a front-end dev and I need to change the design system"

1. `npm ci` at the repo root once — installs every workspace into one hoisted `node_modules`.
2. Solution view → expand **`Alis.Reactive.Assets`** → **`design-system/`**. Every stylesheet
   is there: `app.css`, `brand.css`, `base.css`, `components/`, `validation.css`.
   `Alis.Reactive.Assets/README.md` is the one-screen map.
3. Edit the CSS.
4. Terminal: `npm run watch:design-system`. vite rebuilds `dist/css/design-system.dev.css` on
   every save.
5. Another terminal: `dotnet watch --project Alis.Reactive.SandboxApp`. The sandbox serves the
   rebuilt CSS through its `CompositeFileProvider` — no copy step.
6. Refresh `localhost:5220`. The change is on screen — exactly as if the design system were
   built inside the sandbox app.
7. `npm run lint`, commit. **They never opened a `.csproj` and never thought about NuGet.**

### "I am a runtime (TS) dev"

Identical loop, in `Alis.Reactive.Assets/runtime/`. `npm run watch:runtime`, `npm test`
(vitest), `npm run typecheck`.

### "I am a C# dev"

Work in `Alis.Reactive/`, `Alis.Reactive.DesignSystem/`, etc. — pure C#. Need bundles to run
the sandbox? `npm run build:all` once. Packing is automatic: each csproj points at
`$(AlisAssetsDist)` and a `Verify…BeforePack` target fails fast if the bundle is missing.

### The README (`Alis.Reactive.Assets/README.md`, created in Phase 4)

One screen: what the workspace is; the `runtime/ design-system/ fusion/` layout; the
`build` / `watch:*` / `test` / `typecheck` commands; which bundle ships in which NuGet; a
cross-link to `Alis.Reactive.DesignSystem/` for the C# half; and "you do not edit the `.csproj`."

## npm scripts — complete re-home map (all 17)

| Current root script | Destination | New name |
|---|---|---|
| `build` (runtime esbuild) | `Alis.Reactive.Assets` | `build:runtime` |
| `build:css` (DS vite) | `Alis.Reactive.Assets` | `build:design-system` |
| `build:fusion-css` | `Alis.Reactive.Assets` | `build:fusion` |
| `build:sandbox-plugins` | `Alis.Reactive.SandboxApp` | `build:plugins` |
| `build:sandbox-css` | `Alis.Reactive.SandboxApp` | `build:css` |
| `build:all` | **stays at root** | `build:all` = `npm run build -w Alis.Reactive.Assets && npm run build -w Alis.Reactive.SandboxApp` — name kept (workflows call it); an explicit ordered chain, not `--workspaces`, so build order is deterministic |
| `watch` | `Alis.Reactive.Assets` | `watch:runtime` (+ root passthrough) |
| `watch:css` | `Alis.Reactive.Assets` | `watch:design-system` (+ root passthrough) |
| `watch:fusion-css` | `Alis.Reactive.Assets` | `watch:fusion` (+ root passthrough) |
| `watch:sandbox-plugins` | `Alis.Reactive.SandboxApp` | `watch:plugins` (part of root `watch:sandbox`) |
| `watch:sandbox-css` | `Alis.Reactive.SandboxApp` | `watch:css` (part of root `watch:sandbox`) |
| `typecheck` | **root, delegates** | `npm run typecheck --workspaces --if-present`; each workspace has `typecheck` |
| `lint` | **stays at root** | repo-wide eslint, unchanged |
| `lint:fix` | **stays at root** | repo-wide eslint, unchanged |
| `build:api-docs` | **stays at root** | `dotnet run --project tools/ApiDocGenerator` — not a frontend build |
| `test` | **root, delegates** | → both workspaces' `test` (vitest) — Assets runtime tests + SandboxApp tests |
| `test:watch` | `Alis.Reactive.Assets` | `test:watch` |

Each workspace `build` runs its sub-builds: Assets `build` = `build:runtime && build:design-system
&& build:fusion`; SandboxApp `build` = `build:plugins && build:css`. Each workspace also owns a
`test` script (`vitest run --passWithNoTests`) and a `typecheck` script.

## npm dependencies — destination of every package

| Package | Current type | Destination workspace | Type there |
|---|---|---|---|
| `esbuild` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `vite` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `@tailwindcss/vite` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `tailwindcss` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `vitest` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `jsdom` | devDependency | `Alis.Reactive.Assets` + `Alis.Reactive.SandboxApp` | devDependency |
| `@syncfusion/ej2` | devDependency | `Alis.Reactive.Assets` (Fusion CSS sources the EJ2 theme) | devDependency |
| `typescript` | devDependency | **root** (shared tooling, hoisted) | devDependency |
| `eslint`, `@eslint/js`, `typescript-eslint` | devDependency | **root** (repo-wide lint) | devDependency |
| `@microsoft/signalr` | **dependency** | `Alis.Reactive.Assets` (bundled into runtime JS by esbuild) | **dependency** |
| `@tailwindcss/forms` | **dependency** | `Alis.Reactive.Assets` (`app.css` `@plugin "@tailwindcss/forms"`) | **dependency** |

`package-lock.json` is regenerated by the workspace conversion and is in scope (see "Files
touched" and the lockfile risk).

## esbuild config — exact option parity

`Alis.Reactive.Assets/esbuild.config.mjs` must reproduce today's inline CLI exactly:
`entryPoints: ['runtime/root.ts']`, `bundle: true`, `format: 'iife'`,
`globalName: '__alisReactive'`, `outfile: 'dist/scripts/alis-reactive.dev.js'`. The `build`
path sets `minify: true`; the `watch` path sets `minify: false` and uses an esbuild
`context().watch()` — matching today's `build` script (has `--minify`) vs `watch` script
(no `--minify`). Verified by the bundle byte-compare in Phase 1.

## CI — continuous integration

| Concern | Today | After Phase 0 |
|---|---|---|
| `paths:` trigger (`nuget-publish.yml`, `deploy-docs.yml`) | omits `Alis.Reactive.DesignSystem/**` | add it to both workflows |
| `test` job | runs 5 of 7 tracked unit-test projects | add the two missing — `Alis.Reactive.DesignSystem.Tests`, `Alis.Reactive.NativeTagHelpers.Tests` |
| `npm typecheck` / `test` | never run in CI | add an npm-checks step to the `test` job. `lint` held back — 10 pre-existing eslint errors would make CI red; separate cleanup |
| Playwright | non-blocking (`continue-on-error: true`) | unchanged by this plan — recorded as a known gap, out of scope |

## CD — continuous delivery (`pack-and-publish`)

| Step | Input | Process | Output |
|---|---|---|---|
| Version | branch (`main` → stable, `release/*` → `preview.N`) | existing version step | `--version-suffix` |
| Pack | each `dist/` bundle + each `*.dll` | `dotnet pack` per package — **add `Alis.Reactive.DesignSystem`** | 6 `*.nupkg` |
| Publish | `*.nupkg` | `dotnet nuget push --skip-duplicate` | packages on NuGet.org |
| Deliver | consumer references a package | `build/AlisReactiveAssets.targets` auto-imported | versioned `wwwroot/` files in the consumer app |

Adding `Alis.Reactive.DesignSystem` to the pack list closes the dangling-dependency bug.

## Migration — phases 0–4, each independently green

Each phase ends with: `npm run build:all` clean, `npm test` clean, `dotnet build` clean,
sandbox verified in a browser, `git status` clean. A phase that cannot be left green is not
done. Each phase is one reviewable commit.

### Phase 0 — CI/CD bug fixes (independent, lands first)

- **Input:** `nuget-publish.yml` and `deploy-docs.yml` with the three gaps above.
- **Steps:** add `Alis.Reactive.DesignSystem/**` to both `paths:` filters; add `dotnet pack
  Alis.Reactive.DesignSystem/Alis.Reactive.DesignSystem.csproj` to `pack-and-publish`; add the
  two missing test projects (`Alis.Reactive.DesignSystem.Tests`,
  `Alis.Reactive.NativeTagHelpers.Tests`) to the `test` job; add an npm `typecheck` + `test`
  step. (`lint` is deliberately **not** added: the runtime has 10 pre-existing eslint errors —
  adding it would make CI red. Flagged as a separate cleanup; CI gains `lint` once cleared.)
- **Output:** CI builds/tests every project; CD publishes all 6 packages; the dangling
  dependency is gone. Zero dependency on the workspace consolidation.
- **Verify:** workflow file lints; a dry-run (`workflow_dispatch`) produces 6 `.nupkg`.
  (`Alis.Reactive.Analyzers` is `IsPackable=false` — it ships *inside* `AlisReactive`,
  not as a standalone package; the CI `dotnet pack` line on it is a harmless no-op.)

### Phase 1 — npm workspace structure + runtime

- **Input:** TS runtime at `Alis.Reactive.Assets/Scripts/`; esbuild inline; root `package.json`
  with 17 scripts; `vite.sandbox.config.ts` at root.
- **Steps:**
  1. `git mv Alis.Reactive.Assets/Scripts/* Alis.Reactive.Assets/runtime/`.
  2. `git mv Alis.Reactive.Assets/Scripts/tsconfig.json Alis.Reactive.Assets/tsconfig.json`;
     set `include` to `runtime/**/*.ts`. (There is no repo-root `tsconfig.json`.)
  3. `vitest.config.ts` currently has **two** `include` globs — one for the runtime, one for
     the sandbox. Split them, do not drop either: `git mv vitest.config.ts
     Alis.Reactive.Assets/vitest.config.ts` with `include: ["runtime/__tests__/**/*.test.ts"]`;
     create `Alis.Reactive.SandboxApp/vitest.config.ts` with
     `include: ["Scripts/__tests__/**/*.test.ts"]`. Both workspaces get a `test` script.
     Neither location has test files on this branch — `--passWithNoTests` keeps both green.
  4. Create `Alis.Reactive.Assets/package.json` (workspace) and
     `Alis.Reactive.SandboxApp/package.json` (workspace) per the script + dependency tables.
  5. Create `Alis.Reactive.Assets/esbuild.config.mjs` (exact parity, above).
  6. `git mv vite.sandbox.config.ts Alis.Reactive.SandboxApp/vite.config.ts`; create
     `Alis.Reactive.SandboxApp/esbuild.config.mjs` for `sandbox-plugins.ts`.
  7. Root `package.json`: `"workspaces": ["Alis.Reactive.Assets", "Alis.Reactive.SandboxApp"]`,
     keep `build:all`/`lint`/`lint:fix`/`build:api-docs`, add thin passthroughs.
  8. `eslint.config.js` — repoint the **three `Alis.Reactive.Assets/Scripts/` sites** (lines
     25, 27, 78) to `runtime/`. The three `Alis.Reactive.SandboxApp/Scripts/` sites (lines 59,
     61, 79) are **untouched** — that directory does not move.
  9. `Alis.Reactive.Assets.csproj` (NoTargets): glob `Scripts/**` → `runtime/**`; fix the
     comment block that says "TypeScript runtime under Scripts/".
- **Output paths are unchanged** — runtime still builds to `Alis.Reactive.Assets/dist/scripts/`,
  sandbox still to `wwwroot/`. So `Program.cs`, the three packaging csprojs, and `.gitignore`
  are **untouched** this phase.
- **Verify:** delete `node_modules`, `npm ci`, `npm run build:all`; **byte-compare**
  `alis-reactive.dev.js` and both CSS bundles against a pre-change build — identical bytes
  prove no dependency/lockfile drift. `npm test`, `npm run typecheck`, `dotnet build`, sandbox
  serves JS, `git status` clean.

### Phase 2 — design-system + Fusion CSS into the workspace

DS and Fusion CSS move **together** — `fusion/syncfusion.entry.css` `@import`s the design-system
`brand.css`, so they are coupled and each file is touched once.

- **Steps:**
  1. `git mv Alis.Reactive.DesignSystem/Styles/* Alis.Reactive.Assets/design-system/`.
  2. `git mv Alis.Reactive.Fusion/Styles/* Alis.Reactive.Assets/fusion/`.
  3. `git mv vite.config.ts Alis.Reactive.Assets/vite.design-system.config.ts` and
     `git mv vite.fusion.config.ts Alis.Reactive.Assets/vite.fusion.config.ts`; update each
     config's `input` path. **`outDir` is left at the current location** this phase.
  4. `app.css` `@source` lines: **unchanged** — the new location
     `Alis.Reactive.Assets/design-system/app.css` is the same folder depth from the repo root
     as `Alis.Reactive.DesignSystem/Styles/app.css`, so `../../Alis.Reactive.*/**/*.cs` still
     resolves. `@import "./brand.css"` is unchanged (same-dir, moves together).
  5. `@import` recomputes:
     - `fusion/syncfusion.entry.css`: `../../Alis.Reactive.DesignSystem/Styles/brand.css`
       → `../design-system/brand.css`.
     - `Alis.Reactive.SandboxApp/Styles/sandbox.css` (does not move):
       `../../Alis.Reactive.DesignSystem/Styles/brand.css`
       → `../../Alis.Reactive.Assets/design-system/brand.css`.
  6. `design-system/brand.css` header comment (lines 3-4) names two paths that go stale on
     the move — `Styles/app.css` and `Alis.Reactive.Fusion/Styles/syncfusion.entry.css`.
     Update both (`app.css` stays same-dir; syncfusion becomes `../fusion/syncfusion.entry.css`).
  7. `Alis.Reactive.Assets.csproj`: add `design-system/**` and `fusion/**` globs.
- **Output paths still unchanged** → `Program.cs` and the csprojs remain untouched.
- **Verify:** `npm run build:all` produces the same CSS bytes; sandbox renders styled;
  confirm a known design-system utility class (e.g. a `TokenMap` grid class) is present in the
  built CSS — proves the `@source` C# scan still works. `dotnet test
  tests/Alis.Reactive.DesignSystem.Tests`.

### Phase 3 — output cutover + packaging + `Program.cs` (touched once)

- **Steps:**
  1. Flip the three bundler `outDir`/`outfile` paths to `Alis.Reactive.Assets/dist/{scripts,css}/`.
  2. `Directory.Build.props`: define
     `<AlisAssetsDist>$(MSBuildThisFileDirectory)Alis.Reactive.Assets\dist</AlisAssetsDist>`.
  3. `Alis.Reactive.csproj` — **all three edit sites**: the `build\` `<None>`, the
     `buildTransitive\` `<None>`, and the `<Error>` in `VerifyBundlesExistBeforePack` —
     repoint to `$(AlisAssetsDist)\scripts\alis-reactive.dev.js`.
  4. `Alis.Reactive.DesignSystem.csproj` — same three sites → `$(AlisAssetsDist)\css\design-system.dev.css`.
  5. `Alis.Reactive.Fusion.csproj` — same three sites → `$(AlisAssetsDist)\css\syncfusion.dev.css`.
  6. `Program.cs` — **edited once, only here**: collapse the three-element `assetDistDirs`
     array to a single `CompositeFileProvider` over `Alis.Reactive.Assets/dist`.
  7. `.gitignore`: remove `Alis.Reactive.DesignSystem/dist/` and `Alis.Reactive.Fusion/dist/`;
     keep `Alis.Reactive.Assets/dist/`.
- **Output:** all three bundles build into one `dist/`; all three packages pack from
  `$(AlisAssetsDist)`; the sandbox serves from one provider.
- **Verify:** `npm run build:all`; `dotnet pack` × 6 → 6 `.nupkg`; inspect the
  `AlisReactive.NativeTagHelpers` nuspec — its `AlisReactive.DesignSystem` dependency now
  matches a package that is actually produced; sandbox boots and serves JS + both CSS bundles;
  Playwright smoke passes.

### Phase 4 — visibility + docs

- `Alis.Reactive.slnx`: add the `/build/` solution folder; confirm `Alis.Reactive.Assets.csproj`
  globs surface `runtime/ design-system/ fusion/`, the configs, and `README.md` in Rider.
- Create `Alis.Reactive.Assets/README.md` (content above) and the cross-link note in
  `Alis.Reactive.DesignSystem/`.
- Docs: root `CLAUDE.md` — the Build & Run section, the watcher list, the bundle table, and
  specifically the "`npm run build:all` runs 5 steps" sentence and its 5-row table, which
  become two workspace builds; `Alis.Reactive/CLAUDE.md`,
  `docs-site/src/content/docs/reference/build-commands.md` (apply the 17-row script table —
  every renamed/re-homed script), `README.md`, `scripts/sonar-analyze.sh` (two `Scripts/`
  mentions — the header comment at line 7 and the log string at line 98, which names both the
  Assets and Sandbox `Scripts/` paths; the Assets one becomes `runtime/`. Sonar
  `sources`/`exclusions` are unaffected by the move).

## Files touched (complete)

- **Moved:** `Alis.Reactive.Assets/Scripts/**` → `runtime/**`;
  `Alis.Reactive.Assets/Scripts/tsconfig.json` → `Alis.Reactive.Assets/tsconfig.json`;
  `vitest.config.ts` → `Alis.Reactive.Assets/`;
  `Alis.Reactive.DesignSystem/Styles/**` → `Alis.Reactive.Assets/design-system/**`;
  `Alis.Reactive.Fusion/Styles/**` → `Alis.Reactive.Assets/fusion/**`;
  `vite.config.ts` → `Alis.Reactive.Assets/vite.design-system.config.ts`;
  `vite.fusion.config.ts` → `Alis.Reactive.Assets/vite.fusion.config.ts`;
  `vite.sandbox.config.ts` → `Alis.Reactive.SandboxApp/vite.config.ts`.
- **New:** `Alis.Reactive.Assets/package.json`, `esbuild.config.mjs`, `README.md`;
  `Alis.Reactive.SandboxApp/package.json`, `esbuild.config.mjs`, `vitest.config.ts`.
- **Edited:** root `package.json`; `package-lock.json` (regenerated by the workspace
  conversion); `Directory.Build.props`; `eslint.config.js` (3 `Scripts/` sites); `.gitignore`;
  `Alis.Reactive.csproj` (3 sites), `Alis.Reactive.DesignSystem.csproj` (3 sites),
  `Alis.Reactive.Fusion.csproj` (3 sites); `Alis.Reactive.Assets.csproj` (NoTargets globs +
  comment); `Program.cs` (once, Phase 3); `design-system/app.css` (`@source` unchanged —
  listed for the reviewer's confirmation); `design-system/brand.css` (header-comment paths);
  `fusion/syncfusion.entry.css` (`@import`);
  `Alis.Reactive.SandboxApp/Styles/sandbox.css` (`@import`);
  `Alis.Reactive.SandboxApp/Scripts/tsconfig.json` (stays; `typecheck` script re-homed);
  `Alis.Reactive.slnx`; `.github/workflows/nuget-publish.yml`;
  `.github/workflows/deploy-docs.yml`; docs listed in Phase 4.
- **Deleted:** none — `Alis.Reactive.Assets/` is kept and made whole.

## Risks and rollback

| Risk | Mitigation |
|---|---|
| `@source` / `@import` paths wrong after the move → Tailwind drops C#-emitted classes, or vite fails to resolve `brand.css` | Phase 2 verifies a known utility class is in the built CSS *and* that the Fusion bundle resolves; the `2026-05-15` extraction already hit this exact class |
| npm workspaces re-resolves the dependency tree | Phase 1 deletes `node_modules`, `npm ci` from the committed lockfile, then **byte-compares the built bundles** against a pre-change build. A byte difference must be *diagnosed* — a legitimate `^`-caret tool patch bump vs. real drift — not assumed to be failure |
| A runtime `dependency` (`@microsoft/signalr`, `@tailwindcss/forms`) lands in the wrong workspace | Both are named explicitly in the dependency table as `Alis.Reactive.Assets` `dependencies`; the bundle byte-compare catches a miss |
| `dotnet pack` runs before `npm run build:all` | The `Verify…BeforePack` targets fail fast; they are repointed in Phase 3, not removed |
| A phase left half-done | Each phase is one commit, independently green, `git revert`-able; output paths stay stable until the Phase 3 cutover so Phases 1–2 are fully reversible without touching `Program.cs` |

## Verification matrix

| Concern | Check |
|---|---|
| Runtime builds | `npm run build:runtime` → `dist/scripts/alis-reactive.dev.js` exists |
| Design system builds | `npm run build:design-system` → `dist/css/design-system.dev.css` exists |
| Fusion builds | `npm run build:fusion` → `dist/css/syncfusion.dev.css` exists |
| No dependency drift | post-Phase-1 bundle bytes identical to pre-change |
| Runtime tests | `npm test` green (`--passWithNoTests`) |
| C# unit tests | all runnable unit-test projects green |
| Sandbox dev loop | edit `design-system/brand.css` → `watch:design-system` → browser refresh shows it |
| Pack — all 6 | `dotnet pack` × 6 incl. `AlisReactive.DesignSystem` → 6 `.nupkg` (Analyzers is `IsPackable=false`, ships inside Core) |
| No dangling dependency | `AlisReactive.NativeTagHelpers` nuspec's `AlisReactive.DesignSystem` dependency maps to a produced package |
| Visibility | Rider shows `Alis.Reactive.Assets` with `runtime/ design-system/ fusion/` + configs + README |
| `git status` clean | after a full `npm run build:all` + `dotnet build` |

## Review round 1 — resolution

| # | Finding (reviewer) | Resolution |
|---|---|---|
| 1 | Phase 1 not green — sandbox assets (Codex) | Phase 1 now creates **both** workspaces; sandbox build scripts move with it |
| 2 | `build:all` rename breaks CI; `deploy-docs.yml` missed (Codex, code-rev) | `build:all` **kept**, not renamed; `deploy-docs.yml` added to scope |
| 3 | `@source` recompute wrong (Codex, code-rev, indep) | Corrected — paths **unchanged**, same folder depth |
| 4 | `brand.css` `@import` chains missed (code-rev, indep) | Phase 2 step 5 recomputes both explicitly |
| 5 | CI test-project gap wider (code-rev) | Re-verified by `git ls-files`: exactly 2 tracked projects are missing. The round-1 "DriftDetection.Tests / Net48.SmokeTest exist" claim was **wrong** — those are dead untracked dirs. Corrected in round 2 |
| 6 | `Program.cs` edited 3× (indep) | Output paths stable until Phase 3; `Program.cs` edited **once** |
| 7 | Dependency destinations unspecified (Codex, indep) | Full dependency table incl. the two runtime `dependencies`; `package-lock.json` in scope |
| 8 | esbuild config under-specified (Codex) | Exact option-parity section added |
| 9 | CI never runs npm test/typecheck/lint (Codex) | Phase 0 adds an npm-checks step |
| 10 | `runtime/__tests__/` does not exist (code-rev, indep) | Diagram corrected; exact `vitest` `include` value stated |
| 11 | `tsconfig.json` not at root (code-rev, indep) | Corrected — it is `Scripts/tsconfig.json`; sandbox tsconfig handled |
| 12 | `eslint.config.js` 3 sites (code-rev) | All three enumerated in Phase 1 step 8 |
| 13 | `Alis.Reactive.Assets.csproj` glob set incomplete (code-rev) | Globs added across Phase 1 + Phase 2; comment fixed |
| 14 | `Alis.Reactive.csproj` 3 edit sites (code-rev) | All three enumerated in Phase 3 step 3 |
| 15 | 17-script map incomplete (code-rev) | Full 17-row table added |
| 16 | `build-commands.md` needs script mapping (code-rev) | Phase 4 applies the 17-row table |
| 17 | `sonar-analyze.sh` scope (code-rev) | Phase 4 notes "log string only" |
| 18 | CI fixes should land first (indep) | They are Phase 0 |
| 19 | DS C#/CSS split honesty (indep) | "Design-system cohesion cost" section added; user-directed |
| 20 | NoTargets vs solution folder (indep) | "Why two mechanisms" justification added |
| 21 | "one build" principle violated by sandbox (indep) | Principle restated: one workspace per build-owning unit |
| 22 | `-w` flag noise (indep) | Root passthrough scripts added |
| 23 | Lockfile eyeball-diff unreviewable (indep) | Replaced with a bundle byte-compare |

## Review round 2 — resolution

Round 2: code-reviewer **SIGN-OFF**; independent **CHANGES REQUESTED** (2 real findings);
Codex did not return a verdict. Every finding below was re-verified by hand against
`git ls-files` / the actual files before this revision was written.

| Finding (reviewer) | Resolution |
|---|---|
| Phase 0 added non-existent CI test projects `DriftDetection.Tests` / `Net48.SmokeTest` (indep) | `git ls-files` confirms both are untracked dead dirs. Removed from scope; the CI gap is exactly 2 real, tracked projects |
| Phase 1 dropped the second `vitest.config.ts` include — the SandboxApp test path (indep) | The SandboxApp workspace now gets its own `vitest.config.ts` + `test` script; both test paths are preserved |
| `build:all` `--workspaces` order is non-deterministic; `CLAUDE.md` "5 steps" goes stale (indep) | `build:all` is now an explicit ordered chain; Phase 4 rewrites the `CLAUDE.md` "5 steps" sentence + table |
| `eslint.config.js` wording imprecise — 6 `Scripts/` sites total, 3 move (indep) | Phase 1 step 8 now names the 3 Assets sites that move and the 3 SandboxApp sites that stay |
| `scripts/sonar-analyze.sh` has 2 `Scripts/` mentions, not 1 (code-rev N1) | Phase 4 corrected |
| `design-system/brand.css` header comment goes stale after the move (code-rev N2) | Phase 2 step 6 added |

## Open question for plan review

**npm workspaces** vs. one root `package.json` with namespaced scripts. This plan chooses
workspaces — it is the npm-native way to give the sandbox its own scripts and keep the asset
workspace self-contained, and the user explicitly endorsed "the sandbox can have its own
scripts." Confirm at sign-off.
