# Alis.Reactive.Assets

The framework's **browser assets** — everything that ships to and runs in the
browser. One npm workspace, one toolchain. C# never builds these; the C# NuGet
packages only *ship* the bundles built here.

## Layout

| Folder | What | Output (`dist/`) | Ships in NuGet, served at `_content/{PackageId}/` |
|--------|------|------------------|---------------------------------------------------|
| `runtime/`, `components/` | TypeScript runtime and the framework's own component code | `dist/scripts/alis-reactive.dev.js` | `AlisReactive`: `js/alis-reactive.js` |
| `design-system/` | Design-system CSS: tokens, page base, field and validation look; `fusion.css`, its look for Syncfusion | `dist/css/design-system.dev.css`, `dist/css/design-system-fusion.dev.css` | `AlisReactive.DesignSystem`: `css/design-system.css`, `css/design-system-fusion.css` |
| `fusion/` | The Syncfusion Tailwind3 theme and the grid dialog form | `dist/css/syncfusion.dev.css` | `AlisReactive.Fusion`: `css/fusion.css` |
| `native/` | Drawer, loader, check list, radio group | none: shipped as written | `AlisReactive.Native`: `css/native.css` |
| `native-tag-helpers/` | The `<native-*>` layout tag helpers | none: shipped as written | `AlisReactive.NativeTagHelpers`: `css/native-tag-helpers.css` |

The component stylesheets (`native/`, `native-tag-helpers/`, `fusion/grid-dialog-form.css`) style
the class names their package writes and read the design system's tokens (`--color-*`,
`--font-display`) with neutral fallbacks; no component stylesheet imports the design system.

The matching **C#** lives in sibling projects: design-system token/layout helpers in
`Alis.Reactive.DesignSystem/`, components in `Alis.Reactive.Native/`, `Alis.Reactive.Fusion/` and
`Alis.Reactive.NativeTagHelpers/`. This workspace owns the *build*; those projects own the
*package*, each listing its files as `AlisReactiveStaticWebAsset` items
(`build/AlisReactiveStaticWebAssets.targets`).

## Commands

Run from the repo root (npm-workspace passthroughs) or from this folder.

| Command | Does |
|---------|------|
| `npm run build:all` | build every bundle (this workspace + the sandbox) |
| `npm run watch:runtime` | rebuild the runtime JS on every `.ts` change |
| `npm run watch:design-system` | rebuild both design-system stylesheets on every change |
| `npm run watch:fusion` | rebuild the Fusion (Syncfusion) CSS |
| `npm run typecheck` | `tsc --noEmit` over `runtime/` |
| `npm test` | vitest |

Build configs live here: `esbuild.config.mjs` (runtime),
`vite.design-system.config.ts`, `vite.fusion.config.ts`. Each pins vite/esbuild's
working directory to the repo root so output is identical regardless of where the
command is invoked.

## You do not edit the `.csproj`

`Alis.Reactive.Assets.csproj` is a `NoTargets` project — it compiles nothing. It
exists only so this workspace is a navigable node in the solution. Packaging is
wired in each package's `.csproj` as `AlisReactiveStaticWebAsset` items (bundles
via the `$(AlisAssetsDist)` MSBuild property in `Directory.Build.props`) and is
automatic — a framework dev never touches it.
