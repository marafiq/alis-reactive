# Plan: Extract `Alis.Reactive.DesignSystem` — one design system, honored everywhere

Status: IMPLEMENTED — all three phases complete and verified (2026-05-15).
Date: 2026-05-15
Layer: 1 (C#) + build pipeline (npm/vite) + packaging. No schema / TS runtime / plan changes.

Verified: dotnet build green; npm run build:all green; npm run typecheck clean;
590 C# unit tests pass; 852 Playwright tests pass; git status clean. The shared
asset-delivery targets was consume-tested with single- and two-package consumers.

Codex review (pre-PR) returned BLOCK with 2 findings; both fixed:
1. `--color-sf-primary` was set to the raw RGB triplet, but the Syncfusion
   tailwind3 theme uses the token directly as a color 300+ times and ships a
   hex default. Corrected to `var(--alis-brand-primary)` (a color); TOKENS.md's
   wrong "must be a triplet" guidance corrected. This was a latent pre-existing
   bug that the extraction surfaced.
2. The "brand single source" claim was verified only for `.css`/`.cs`; the
   sandbox views used `text-[#7A2E3B]` arbitrary classes. All 11 sandbox view
   files converted to the `primary` design-system utility. Remaining literal:
   `examples/resident-intake` (a downstream consumer demo) — follow-up.

## Goal

Make the design system a real, cohesive deliverable:

1. `AlisReactive.NativeTagHelpers` ships with the design system as a clean pair, with **zero dependency on the reactive Core** (plan engine, schema, TS runtime).
2. The brand (`#7A2E3B`, Inter, geometry) has **one source of truth**.
3. **Syncfusion honors the design system** — `syncfusion.overrides.css` consumes the design-system brand tokens instead of hardcoding them.
4. Asset delivery is **one authored mechanism**, not three duplicated ones.

## Current state (evidence)

- `Alis.Reactive/DesignSystem/` — 17 `.cs` files, namespace `Alis.Reactive.DesignSystem.*`. Self-contained: imports only `System` / `System.Collections.Generic`. Used in C# **only** by `NativeTagHelpers` (verified: Native and Fusion C# do not reference the namespace). Lives inside Core, so it inherits `LangVersion 8` and is awkwardly `public` inside a project whose rule is "everything internal."
- `Alis.Reactive.Assets/` — a plain asset folder (no `.csproj`). Holds the TS runtime (`Scripts/`, `dist/scripts/`) **and** the core CSS (`Styles/`, `dist/css/design-system.dev.css`). The two bundles share `dist/`; `vite.config.ts` uses `emptyOutDir: false` for that reason.
- `design-system.dev.css` — built by `build:css` (vite) from `Alis.Reactive.Assets/Styles/app.css`, which `@source`-scans `Alis.Reactive/DesignSystem/**`, `NativeTagHelpers/**`, `Native/**`, `Fusion/**`. So the CSS bundle is a **shared foundation** for all three component families.
- `Alis.Reactive.csproj` packs both `alis-reactive.dev.js` and `design-system.dev.css` into `AlisReactive`; `AlisReactive.targets` copies both into a consumer's `wwwroot`; `VerifyBundlesExistBeforePack` checks both.
- `AlisReactive.targets` and `AlisReactive.Fusion.targets` are **~90% identical** — same version-from-folder derivation, same net48/net10 path split, same `AfterTargets="Build"` copy. The copy mechanism is already duplicated twice today.
- `Alis.Reactive.NativeTagHelpers.csproj` references `Alis.Reactive` (all of Core) only to reach `DesignSystem`.
- Brand `#7A2E3B` is duplicated across `app.css:30`, `Alis.Reactive.SandboxApp/Styles/sandbox.css:12`, `syncfusion.overrides.css:16,24,29` (in two value formats — RGB triplet and hex), and `loader.css:33` (fallback literal). `syncfusion.entry.css`'s header comment claims it reads the core `--color-*` tokens; it does not.

## Target state

New project `Alis.Reactive.DesignSystem` → NuGet `AlisReactive.DesignSystem`, owning the C# token/class helpers, the core CSS source, the `design-system.dev.css` bundle, the brand token contract (`brand.css`), and its packaging.

```
Alis.Reactive.DesignSystem      (no project deps; packs C# dll + design-system.dev.css)
        ▲           ▲        ▲
        │           │        │
NativeTagHelpers   Native   Fusion
(DesignSystem      (Core +  (Core + DesignSystem;
 ONLY — drops      Design-   overrides.css consumes
 Core entirely)    System)   design-system brand tokens)
```

`Alis.Reactive` (Core) keeps the TS runtime + `alis-reactive.dev.js`; it does **not** reference the design system (Core C# never used it).

## Asset delivery model

Three assets, three lifecycles, delivered by **one** authored mechanism.

- **Runtime JS** (`alis-reactive.js`) — stays with `AlisReactive` (Core). The runtime executes reactive plans: Core's concern. Tag-helper-only consumers never need it; keeping it in Core means they never get it. Core's targets become **JS-only** — simpler than today.
- **Design CSS** (`design-system.css`) — ships from `AlisReactive.DesignSystem`.
- **Syncfusion CSS** (`syncfusion.css`) — ships from `AlisReactive.Fusion`, unchanged.

Each is a genuinely different thing with an independent version — that is a feature, not fragmentation. Merging JS into the design-system package would force tag-helper consumers to pull a runtime they never execute: the coupling we are removing, reintroduced.

**One shared targets file.** Author a single generic `build/AlisReactiveAssets.targets` in the repo: it globs `$(MSBuildThisFileDirectory)assets/**`, derives the version from the package folder name (the trailing version segment, no hardcoded package-id prefix), and copies every asset into `wwwroot` (net10) / `Content\alisreactive` (net48). It does not care which asset. Each asset-bearing package packs that **same source file** under its own `build/{PackageId}.targets` and `buildTransitive/{PackageId}.targets` name (NuGet auto-imports by package-id name). Result: 3 packages, 1 authored delivery mechanism — the existing 2-file duplication is eliminated, not tripled.

**Consumer experience (end user).** Install any component package; its design-system / Syncfusion / runtime dependencies flow transitively; the shared targets copy `design-system.{version}.css`, `syncfusion.{version}.css`, `alis-reactive.{version}.js` into `wwwroot`. Consumer `<link>` / `<script>` paths are unchanged from today.

**Sandbox.** No NuGet. `Program.cs` `CompositeFileProvider` gains a third provider — `Alis.Reactive.DesignSystem/dist/` — alongside `Alis.Reactive.Assets/dist/` (JS) and `Alis.Reactive.Fusion/dist/`. `_Layout.cshtml` is unchanged. `vite.config.ts` (`build:css`) outputs to `Alis.Reactive.DesignSystem/dist/css/`. The `npm run build:all` / `watch:css` loop is unchanged in surface.

## Phase 1 — New project, move the C#  (low risk, behavior-preserving)

1. Create `Alis.Reactive.DesignSystem/Alis.Reactive.DesignSystem.csproj`: `PackageId` `AlisReactive.DesignSystem`, `IsPackable true`, `GenerateDocumentationFile true`, `LangVersion 8`, `Nullable enable`.
2. `git mv Alis.Reactive/DesignSystem/Tokens/*` → `Alis.Reactive.DesignSystem/Tokens/`, `Layout/*` → `Alis.Reactive.DesignSystem/Layout/`. Namespaces stay `Alis.Reactive.DesignSystem.*` → **zero `using` changes** anywhere.
3. `Alis.Reactive.NativeTagHelpers.csproj`: replace the `Alis.Reactive.csproj` reference with `Alis.Reactive.DesignSystem.csproj`.
4. `tests/Alis.Reactive.DesignSystem.Tests.csproj`: same reference swap.
5. Add `Alis.Reactive.DesignSystem` to `Alis.Reactive.slnx`.
6. Add `Alis.Reactive.DesignSystem` to the `enforce-csharp8` hookify rule's project list.
7. Update `app.css` `@source` path for the C#: `../../Alis.Reactive/DesignSystem/**/*.cs` → `../../Alis.Reactive.DesignSystem/**/*.cs`.

Verify: `dotnet build`; NativeTagHelpers tests (54); DesignSystem tests (58). `git status` clean.

## Phase 2 — CSS ownership + unified delivery  (medium risk)

1. `git mv Alis.Reactive.Assets/Styles/` → `Alis.Reactive.DesignSystem/Styles/`. (`Alis.Reactive.Assets/` keeps only the TS runtime.)
2. `vite.config.ts`: input `Alis.Reactive.DesignSystem/Styles/app.css`; `outDir` `Alis.Reactive.DesignSystem/dist/css`. Filename `design-system.dev.css` unchanged.
3. `app.css` `@source` relative paths recomputed from the new location.
4. Author `build/AlisReactiveAssets.targets` (the generic asset-copy mechanism described above).
5. `Alis.Reactive.DesignSystem.csproj`: pack `dist/css/design-system.dev.css` + the shared targets (as `build|buildTransitive/AlisReactive.DesignSystem.targets`); add `VerifyDesignSystemBundleExistsBeforePack`.
6. `Alis.Reactive.csproj`: remove the `design-system.dev.css` pack entries and the CSS check in `VerifyBundlesExistBeforePack`; pack the shared targets as `AlisReactive.targets` (JS-only behaviour now).
7. `Alis.Reactive.Fusion.csproj`: pack the shared targets as `AlisReactive.Fusion.targets`. Delete the bespoke `AlisReactive.Fusion.targets` and `AlisReactive.targets` files.
8. `Alis.Reactive.Native.csproj` and `Alis.Reactive.Fusion.csproj`: add `ProjectReference … Alis.Reactive.DesignSystem.csproj` so the design-system CSS flows transitively.
9. `Alis.Reactive.SandboxApp/Program.cs`: add `Alis.Reactive.DesignSystem/dist/` to the `CompositeFileProvider` and its fail-fast existence check.
10. Example app(s): add a `PackageReference` to `AlisReactive.DesignSystem` once it publishes (or rely on the transitive flow via the component package they already reference).

Verify: `npm run build:all`; `dotnet build`; all C# unit suites; sandbox serves `~/css/design-system.dev.css`; Playwright smoke.

## Phase 3 — One brand, honored by Syncfusion  (needs browser verification)

1. New `Alis.Reactive.DesignSystem/Styles/brand.css` — declares the brand **once** at `:root`, colors as RGB triplets with `rgb()` derivations:
   ```css
   :root {
     --alis-brand-primary-rgb: 122, 46, 59;
     --alis-brand-primary: rgb(var(--alis-brand-primary-rgb));
     /* surfaces, status, accent, font, radius … */
   }
   ```
2. `app.css`: `@import "./brand.css"`; `@theme` consumes the brand vars; remove the literal `#7A2E3B` etc. SPIKE — confirm Tailwind v4 `@theme` accepts `var()`/`rgb(var())`; resolve before implementing Phase 3.
3. `syncfusion.entry.css`: `@import` the design-system brand tokens. `syncfusion.overrides.css`: brand `--color-sf-*` tokens reference `var(--alis-brand-primary-rgb)` (triplet slots) / `rgb(var(--alis-brand-primary-rgb))` (hex slots). Fix the inaccurate header comment.
4. `sandbox.css`: remove the duplicated `--color-primary`. `loader.css`: drop the `#7A2E3B` fallback literal.
5. `brand.css` is `@import`-ed into `app.css`, so vite inlines it into `design-system.dev.css` — not a separately shipped file. The `AlisReactive.Fusion → AlisReactive.DesignSystem` dependency guarantees `design-system.css` is present wherever `syncfusion.css` is.

Verify: browser — sandbox and a Fusion-component view show one consistent brand; changing `--alis-brand-primary-rgb` once moves native and Syncfusion surfaces together; Playwright smoke.

## Outcomes

- **NativeTagHelpers stops dragging the framework** — it pulls only the design system, not the plan engine / schema / TS runtime.
- **The design system becomes one nameable, versioned artifact** instead of a folder in Core + a CSS folder + override CSS in Fusion.
- **Tech debt addressed**: brand duplicated in 4 files / 2 formats → one declaration; the false `entry.css` comment → true; `public` design-system types stranded in an internal-only project → moved to a project meant to be a public API; NativeTagHelpers' false whole-Core dependency → truthful; `Alis.Reactive.Assets/` mixing runtime + CSS → split; 2 duplicated targets files → 1 authored mechanism.
- **Not addressed (deliberate)**: the C# "generate Tailwind class strings" model is unchanged; the Syncfusion hex-vs-triplet split is worked around, not eliminated; no no-build workflow for designers (`brand.css` still needs `npm run build:css`).
- **Designer workflow**: one plain-CSS file (`brand.css`) is the brand surface; change a token, rebuild, every surface re-skins together.
- **Quality**: package-scale cohesion (Core = behavior, DesignSystem = appearance, component packages = components); NativeTagHelpers coupling drops from all-of-Core to the design system; Fusion's brand coupling becomes explicit and directional; the dependency graph becomes truthful.

## Risks & decisions

- **Tailwind v4 `@theme` + `var()`** — the one technical unknown; Phase 3 spike. Does not block Phases 1–2.
- **DesignSystem dll TFM** — `net10.0` (mirrors NativeTagHelpers) recommended; the CSS-copy targets serve net48 consumers regardless of the dll TFM.
- **`LangVersion`** — keep `8`, register with `enforce-csharp8`.
- **Shared-targets version derivation** — generalized to detect the trailing version segment (begins with a digit) rather than stripping a hardcoded package-id prefix.
- **Merge risk** — `git mv` of `DesignSystem/` and `Styles/` conflicts with branches touching those trees; schedule when worktree activity is low; land Phase 1 fast.
- **No schema / TS / plan impact** — confirmed Layer 1 + build only. If a phase appears to touch the runtime, the plan is wrong: stop and re-plan.

## Definition of Done

The effort is **done** only when every item below is satisfied, verified, and reviewed.

Structure
- [ ] `Alis.Reactive.DesignSystem` project + `AlisReactive.DesignSystem` package exist; the 17 `.cs` files moved; namespace unchanged; zero `using` changes anywhere.
- [ ] `NativeTagHelpers` references **only** `AlisReactive.DesignSystem`; the Core reference is gone.
- [ ] `Native` and `Fusion` reference `AlisReactive.DesignSystem`; Core neither contains nor references the design system.
- [ ] `design-system.dev.css` is built and packed by the design-system project; Core packs only the runtime JS.

Brand & Syncfusion
- [ ] `brand.css` declares the brand once; `#7A2E3B` is gone from all framework
      source, all framework CSS, and the sandbox views. The only remaining
      literal is the example app's own `_Layout.cshtml` (downstream demo) — follow-up.
- [ ] `syncfusion.overrides.css` consumes design-system brand tokens; the `entry.css` comment is accurate.
- [ ] Changing `--alis-brand-primary-rgb` once re-skins native **and** Syncfusion surfaces together (browser-verified).

Delivery
- [ ] One authored `AlisReactiveAssets.targets`; the bespoke per-package targets files are gone.
- [ ] Runtime JS delivery unchanged for consumers; design CSS + Syncfusion CSS flow transitively; consumer `<link>`/`<script>` paths unchanged.
- [ ] Sandbox serves all three assets; `_Layout.cshtml` unchanged; `npm run build:all` + watch loop intact.

Quality & verification
- [ ] `dotnet build` green; `npm run build:all` succeeds; `npm run typecheck` clean.
- [ ] All C# unit suites green — NativeTagHelpers (54), DesignSystem (58), Core, Native, Fusion, FluentValidator, Analyzers — with **no test edits** for Phases 1–2.
- [ ] Playwright suite green.
- [ ] `git status` clean after a build (no leaked bundle artifacts).
- [ ] Phase 3 brand consistency verified in a real browser, not only by tests.
- [ ] No schema / TS runtime / plan-shape change occurred.

Process
- [ ] Each phase passed Gate 1 (plan) and Gate 2 (implementation) — 3 sign-offs each.
- [ ] Coverage matrix produced per phase before review.

## Test & coverage

- Phases 1–2 are behavior-preserving: existing suites stay green with no test edits — that is the proof.
- Phase 3 changes rendered CSS: verified in-browser, then Playwright smoke. No unit harness for CSS values.
- Coverage matrix per phase produced before review sign-off.

## Phasing rationale

Each phase builds + tests green on its own and can be a separate PR / review gate. Phase 1 (C# project) is low-risk and fast; Phase 2 (CSS ownership + unified delivery) is the packaging move; Phase 3 (brand unification) delivers "Syncfusion honors the design system" and needs eyes in a browser. Per Gate 1, this plan needs 3 sign-offs before any code.
