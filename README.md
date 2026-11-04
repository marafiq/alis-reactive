# Alis.Reactive

Reactive pages for ASP.NET MVC, written in your views as fluent, typed C#. The C# says what a page
does (react to an event, send a request, check a form on both sides, load a partial, show a branch);
the framework renders it as a plan that its browser runtime runs. Views carry no JavaScript.

It is for teams building server-rendered MVC applications, on ASP.NET Core MVC (.NET 10) or ASP.NET
MVC 5 (.NET Framework 4.8), who want dynamic pages without hand-written scripts, and who want to bring
any JavaScript component (Syncfusion, native inputs, their own) through one typed declaration.

**Start here.**

- **Using the packages:** reference them and register them ([In your application](#in-your-application));
  `examples/resident-intake` is a complete app built only from the packed packages.
- **Working on the framework:** `scripts/doctor.sh`, `scripts/build.sh`, `scripts/run.sh`, then open
  http://localhost:5220 ([Getting started](#getting-started)).

**Look next:** [the map of this repository](#the-repository) and `CLAUDE.md` (the rules every
contributor and coding agent follows).

## Packages

| Package | Description |
|---------|-------------|
| `Alis.Reactive` | Core plan model, builders, serialization, and validation rules in FluentValidation's grammar (server and browser) |
| `Alis.Reactive.Native` | Native HTML components (TextBox, CheckBox, DropDown, Button, etc.) |
| `Alis.Reactive.Fusion` | Syncfusion EJ2 component integration |
| `Alis.Reactive.DesignSystem` | The look an app chooses: a Tailwind theme to import into its own build, and the compiled default for an app with none. No component package depends on it |
| `Alis.Reactive.NativeTagHelpers` | ASP.NET Core Tag Helpers for native components |

`Alis.Reactive`, `Alis.Reactive.Native`, `Alis.Reactive.Fusion`, and `Alis.Reactive.DesignSystem`
multi-target **`net48` and `net10.0`** — the *same* DSL on both runtimes (bridged by `#if` shims, never a divergent API). Only `Alis.Reactive.NativeTagHelpers` is
**`net10.0`-only** (ASP.NET Core Tag Helpers).

### In your application

Register the packages you use in `Program.cs` (MVC 5 on net48: in `Global.asax` or your container's
startup). `examples/resident-intake/Program.cs` is the worked example.

```csharp
using Alis.Reactive.Fusion;

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());   // your ReactiveValidator<T> classes
builder.Services.AddReactiveFusion();                   // only with Alis.Reactive.Fusion
```

`AddReactiveFusion()` sets the Newtonsoft naming Syncfusion renders with: your data sources reach the
browser in camel case, so `Fields<TItem>(...)` finds each item's members, and the names Syncfusion pins
(the toast's and dialog's `X`/`Y` position) keep their spelling. Call it after any line of yours that
assigns `JsonConvert.DefaultSettings`; it keeps the rest of those settings. Without it, a plain
camel-case resolver renames the pinned names and the toast falls back to the top left.

## Getting started

Prerequisites: the .NET SDK `global.json` names (10.0.x) and Node.js 22 or newer. A fresh clone has no
installed dependencies and no built bundles; from the repository root:

```bash
scripts/doctor.sh   # read-only check of tools and build outputs
scripts/build.sh    # npm dependencies, browser bundles, dotnet build
scripts/run.sh      # the sandbox at http://localhost:5220
```

The sandbox's home page is the index of its pages. Each script prints its options with `--help`.

## How the bundles ship

`npm run build:packages` builds the runtime bundle into `Alis.Reactive.Assets/dist/` and the design
system's compiled default into `Alis.Reactive.DesignSystem/dist/` (both gitignored); `npm run build:all`
also builds the sandbox's own assets. Every app builds its own in its `dotnet build`, after the design
system copies the theme in: the sandbox, the framework test app and the example. Each package ships
its own files the way a Razor class library does: as static web assets.

| Consumer | How it gets the files |
|----------|-----------------------|
| **Sandbox** | Project references. In Development the app serves each file at `~/_content/{PackageId}/...` from where npm wrote it, so a watcher's rebuild shows on the next request |
| `examples/resident-intake` | Package references from `./nupkgs`, as an app outside this repository: `scripts/example.sh` packs, builds it against that version and runs it; `--smoke` drives it in a browser, as the gate's `example` job does |
| **ASP.NET Core app (net10.0)** | Package references. The same `~/_content/{PackageId}/...` paths; publish copies the files under `wwwroot/_content/` |
| **ASP.NET MVC 5 app (net48)** | Package references. Each package's `build\net48\{PackageId}.targets` copies its files into the web project at `Content\{PackageId}\` after every build |

`dotnet pack` never runs npm, and refuses to pack a package whose bundle is missing.

## Browser assets: what each package ships and where it lands

The core and the components ship the styling their own markup needs, plain and complete. The look
(color, type, the brand) is the design system's: an app references `AlisReactive.DesignSystem`, or
sets the same tokens itself (`--color-border`, `--color-accent`, `--font-display`, ...), and the
components follow it. Without one, their stylesheets fall back to neutral colors and the page's font.

| Package | File | net10.0 URL | net48 file in the web project |
|---------|------|-------------|-------------------------------|
| `AlisReactive` | runtime script | `~/_content/AlisReactive/js/alis-reactive.js` | `Content\AlisReactive\js\alis-reactive.js` |
| `AlisReactive` | the field wrapper, its message, the form summary and the refused-input mark, plain | `~/_content/AlisReactive/css/alis-reactive.css` | `Content\AlisReactive\css\alis-reactive.css` |
| `AlisReactive.Native` | drawer, loader, check list, radio group | `~/_content/AlisReactive.Native/css/native.css` | `Content\AlisReactive.Native\css\native.css` |
| `AlisReactive.NativeTagHelpers` | `<native-*>` layout tag helpers | `~/_content/AlisReactive.NativeTagHelpers/css/native-tag-helpers.css` | (net10.0 only) |
| `AlisReactive.Fusion` | grid dialog form, the refused-input mark on Syncfusion wrappers | `~/_content/AlisReactive.Fusion/css/fusion.css` | `Content\AlisReactive.Fusion\css\fusion.css` |
| `AlisReactive.DesignSystem` | the theme: one `@theme` block of tokens, for an app's own Tailwind build | `~/_content/AlisReactive.DesignSystem/css/theme.css` | `Content\AlisReactive.DesignSystem\css\theme.css` |
| `AlisReactive.DesignSystem` | the compiled default: the tokens, the page base, the field, validation and Syncfusion look, under `.alis-root` | `~/_content/AlisReactive.DesignSystem/css/design-system.css` | `Content\AlisReactive.DesignSystem\css\design-system.css` |

With Fusion, the Syncfusion theme and `ej2.min.js` are the app's: no framework package ships them. Take
`@syncfusion/ej2` at the version Fusion pins (`node scripts/pin-syncfusion.mjs --check` prints it) and copy
its `tailwind3.css` and `dist/ej2.min.js` into your `wwwroot`, as the sandbox's `copy-vendor.mjs` does, or
link Syncfusion's CDN at that version.

Link the framework's stylesheets before your own; among them the order is free, and the Syncfusion
theme goes anywhere among them.

```html
<link rel="stylesheet" href="~/_content/AlisReactive/css/alis-reactive.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.DesignSystem/css/design-system.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.Native/css/native.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.NativeTagHelpers/css/native-tag-helpers.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/vendor/syncfusion/tailwind3.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/_content/AlisReactive.Fusion/css/fusion.css" asp-append-version="true"/>
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true"/>
...
<script src="~/vendor/syncfusion/ej2.min.js" asp-append-version="true"></script>
@Html.EJS().ScriptManager()
<script src="~/_content/AlisReactive/js/alis-reactive.js" asp-append-version="true"></script>
```

The app serves them with `app.UseStaticFiles()`, as the sandbox does. Running from build
output outside Development needs `builder.WebHost.UseStaticWebAssets()`; a published app does not.

Link every framework file with `asp-append-version="true"`, as above: the URL carries the file's content
hash, so the first page after a package upgrade fetches the new files and no browser runs a cached
runtime against a newer plan.

On net48, link the copied files with `Url.AppendVersion("~/Content/AlisReactive/js/alis-reactive.js")`, the
core's equivalent of `asp-append-version` for MVC 5: the same URL with the same content hash.
They are build output, and each `Content\{PackageId}\` folder carries a `.gitignore` the package
writes, so git never lists them; include the folders as `Content` in the web project so publish
deploys them. A copy an app committed before stays tracked, since an ignore never untracks a file:
`git rm -r --cached` it once.

## For UI developers — the design system

The design system is two stylesheets and no code. `css/theme.css` is one `@theme static` block of
tokens (`--color-primary`, `--color-surface`, `--font-display`, ...): the brand. `css/design-system.css`
is compiled from it: the tokens on `:root`, the page base, the look of the core's field and validation
markup, and the brand on Syncfusion components, all scoped to `.alis-root`.

1. **Reference the package** beside the component packages you use; they do not bring it.

2. **Link the compiled default and mark the root.** The `<link>` above, and `class="alis-root"` on
   `<body>`: the page styles never leak outside it, and Syncfusion popups append to `body`, so the
   root must hold them. An app with no CSS build stops here; it re-brands with `:root { --color-primary: ... }`
   in a stylesheet linked after the design system's.

   ```html
   <body class="alis-root">
   ```

3. **With your own Tailwind build, import the theme.** On every `dotnet build` the package copies
   `theme.css` into `Styles/AlisReactive.DesignSystem/` in your project, a folder the package owns and
   marks ignored with its own `.gitignore` (`<AlisReactiveThemeFolder>` names another folder; `none`
   turns the copy off). Your stylesheet imports it after Tailwind, so your utilities (`bg-primary`, `text-text-muted`, `border-border`) resolve to the
   tokens the components read, and a later `@theme` block re-brands:

   ```css
   @import "tailwindcss";
   @import "./AlisReactive.DesignSystem/theme.css";
   @theme { --color-primary: #0f766e; }
   ```

   Build with Tailwind's CLI (`npx @tailwindcss/cli -i Styles/app.css -o wwwroot/css/app.css --minify`)
   after the first `dotnet build`, and link the output last. The sandbox, the framework test app and
   `examples/resident-intake` do exactly this.

The cascade: every framework stylesheet opens with `@layer theme, base, alis, components, utilities;`
and keeps its looks in `alis`, so Tailwind's Preflight sits below them and your utilities above; a state
mark (`alis-has-error`) stays unlayered and wins over a utility. Linking the framework's stylesheets
before your own is what fixes that order.

**Your own class names.** An input field renders with five class names: the wrapper (`alis-field`), its
label, the required marker, the message slot and the mark a refused input carries (`alis-has-error`).
An app with a stylesheet of its own, Bootstrap's say, chooses them once at startup, and every page
renders with them; a name left unset keeps its default:

```csharp
builder.Services.AddReactiveClassNames(names =>
{
    names.Field = "mb-3";
    names.FieldLabel = "form-label";
    names.FieldRequired = "text-danger";
    names.FieldMessage = "invalid-feedback";
    names.InvalidInput = "is-invalid";
});
```

The framework's stylesheets style the defaults; an app that renames styles its own.
`tests/Alis.Reactive.ClassNames.App` is such an app, with no framework stylesheet linked.

## The repository

| Entry | What it is |
|-------|------------|
| `Alis.Reactive/` | The core package (`AlisReactive`): the plan model, the fluent builders, the Razor helpers, and validation in FluentValidation's grammar |
| `Alis.Reactive.Analyzers/` | The Roslyn analyzers (ALIS001 to ALIS013) that ship inside `AlisReactive` and check views and declarations at build time |
| `Alis.Reactive.Native/` | The native-component package: HTML inputs, buttons and action links, the drawer and the loader |
| `Alis.Reactive.Fusion/` | The Syncfusion package: one slice per EJ2 component, plus the app-wide Confirm and Toast |
| `Alis.Reactive.NativeTagHelpers/` | ASP.NET Core tag helpers for native components (net10 only) |
| `Alis.Reactive.DesignSystem/` | The design system: a Tailwind theme and its compiled default stylesheet, built in its own npm workspace, no C# |
| `Alis.Reactive.Assets/` | The TypeScript runtime and the framework's own component code, an npm workspace that esbuild bundles into `AlisReactive` (its `.csproj` compiles nothing; it shows the workspace in the solution) |
| `Alis.Reactive.SandboxApp/` | The development workbench: one page per primitive or component, driven by the Playwright tests |
| `examples/` | `resident-intake`, an app built only from the packed packages; `net48-mvc-smoke`, the MVC 5 app CI runs under IIS Express |
| `tests/` | The test projects (Playwright, NUnit, analyzers, architecture) and the apps they drive |
| `tools/` | `PlanContractGenerator`, which writes the runtime's `plan.ts` and `plan.schema.json` from the C# grammar |
| `scripts/` | The command-line wrappers: doctor, build, run, test, Playwright, pack, example; each prints its options with `--help` |
| `build/` | MSBuild targets: the two the packages ship (static web assets, the net48 `Content` copy) and the one the repo's apps use to build their browser assets |
| `.github/` | CI workflows, Dependabot and release notes |
| `.claude/`, `CLAUDE.md`, `AGENTS.md` | The rules, principles and hooks every contributor and coding agent follows (`AGENTS.md` links to `CLAUDE.md`) |
| `Alis.Reactive.slnx` | The solution: every project except the two examples |
| `Directory.Build.props`, `global.json` | Shared build settings (version, language, package metadata, public-surface checks) and the pinned .NET SDK |
| `package.json`, `package-lock.json`, `eslint.config.js` | The npm workspaces (runtime, design system, sandbox, framework app), their lock, and the lint rules |
| `.gitignore` | Build output and local files git never lists |

## License

MIT
