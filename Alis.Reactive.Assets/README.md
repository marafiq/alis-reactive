# Alis.Reactive.Assets

The framework's **runtime** and its own component code: the TypeScript that runs in the browser. One
npm workspace, one toolchain (esbuild). C# never builds it; the `AlisReactive` NuGet only *ships* the
bundle built here.

## Layout

| Folder | What | Output (`dist/`) | Ships in NuGet, served at `_content/{PackageId}/` |
|--------|------|------------------|---------------------------------------------------|
| `runtime/`, `components/` | TypeScript runtime and the framework's own component code | `dist/scripts/alis-reactive.dev.js` | `AlisReactive`: `js/alis-reactive.js` |

Stylesheets live with their packages, not here: the core's, Native's, the tag helpers' and Fusion's in
each package's `css/` folder, shipped as written; the design system's theme and compiled default in
`Alis.Reactive.DesignSystem/` (its own npm workspace builds the default with Tailwind's CLI). The
Syncfusion theme and script are an app's own (the sandbox's `copy-vendor.mjs`); no package ships them.

## Commands

Run from the repo root (npm-workspace passthroughs) or from this folder.

| Command | Does |
|---------|------|
| `npm run build:packages` | build the runtime bundle and the design system's compiled default |
| `npm run build:all` | the packages' assets and the apps' own (the sandbox, the framework test app); the sandbox's stylesheet imports the theme that `dotnet build` copies into it, so build once first |
| `npm run watch:runtime` | rebuild the runtime JS on every `.ts` change |
| `npm run watch:design-system` | rebuild the design system's compiled default on every change |
| `npm run typecheck` | `tsc --noEmit` over `runtime/` |
| `npm test` | vitest |

`esbuild.config.mjs` pins esbuild's working directory to the repo root so output is identical
regardless of where the command is invoked.

## You do not edit the `.csproj`

`Alis.Reactive.Assets.csproj` is a `NoTargets` project — it compiles nothing. It
exists only so this workspace is a navigable node in the solution. Packaging is
wired in each package's `.csproj` as `AlisReactiveStaticWebAsset` items (the bundle
via the `$(AlisAssetsDist)` MSBuild property in `Directory.Build.props`) and is
automatic — a framework dev never touches it.
