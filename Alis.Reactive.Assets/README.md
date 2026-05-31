# Alis.Reactive.Assets

The framework's **browser assets** — everything that ships to and runs in the
browser. One npm workspace, one toolchain. C# never builds these; the C# NuGet
packages only *ship* the bundles built here.

## Layout

| Folder | What | Bundle (`dist/`) | Ships in NuGet |
|--------|------|------------------|----------------|
| `runtime/` | TypeScript reactive runtime | `dist/scripts/alis-reactive.dev.js` | `AlisReactive` |
| `design-system/` | Design-system CSS — tokens, layout, components | `dist/css/design-system.dev.css` | `AlisReactive.DesignSystem` |
| `fusion/` | Syncfusion EJ2 theme CSS | `dist/css/syncfusion.dev.css` | `AlisReactive.Fusion` |

The matching **C#** lives in sibling projects — design-system token/layout
helpers in `Alis.Reactive.DesignSystem/`, Fusion components in
`Alis.Reactive.Fusion/`. This workspace owns the *build*; those projects own the
*package*. (`runtime/__tests__/` holds vitest tests — none on this branch yet.)

## Commands

Run from the repo root (npm-workspace passthroughs) or from this folder.

| Command | Does |
|---------|------|
| `npm run build:all` | build every bundle (this workspace + the sandbox) |
| `npm run watch:runtime` | rebuild the runtime JS on every `.ts` change |
| `npm run watch:design-system` | rebuild the design-system CSS on every change |
| `npm run watch:fusion` | rebuild the Syncfusion CSS |
| `npm run typecheck` | `tsc --noEmit` over `runtime/` |
| `npm test` | vitest |

Build configs live here: `esbuild.config.mjs` (runtime),
`vite.design-system.config.ts`, `vite.fusion.config.ts`. Each pins vite/esbuild's
working directory to the repo root so output is identical regardless of where the
command is invoked.

## You do not edit the `.csproj`

`Alis.Reactive.Assets.csproj` is a `NoTargets` project — it compiles nothing. It
exists only so this workspace is a navigable node in the solution. Packaging is
wired once in the three C# `.csproj` files via the `$(AlisAssetsDist)` MSBuild
property (`Directory.Build.props`) and is automatic — a framework dev never
touches it.
