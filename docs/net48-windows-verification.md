# net48 Runtime Verification — Windows (RC1)

**Status: AUTOMATED in CI.** The classic ASP.NET MVC 5 / .NET Framework 4.8 runtime proof now runs on
`windows-latest` in `.github/workflows/verify-net48.yml` (`net48-runtime-iis` job): it builds
`examples/net48-mvc-smoke`, hosts it under **IIS Express**, drives it with **Playwright**, and uploads
a screenshot. See `examples/net48-mvc-smoke/README.md` for the known net48 consumer gotchas this
surfaced and how each is fixed (asset landing, System.Text.Json 9 binding redirects,
`WebApplication.targets`/`VSToolsPath`, Native Html-helpers vs Tag Helpers, Razor `@` in comments).
Mono/macOS is **not** a faithful .NET Framework host and must not be used to claim this done — that is
why it runs on a real Windows runner.

The manual checklist below remains the reference for the surfaces the automated smoke does not yet
cover (Syncfusion `.Render()`, the validation bridge).

This checklist closes the five net48 runtime items the RC1 audit surfaced. Run it on Windows with
Visual Studio (or MSBuild + IIS Express) and .NET Framework 4.8 installed.

## Setup

1. `git clone` the repo on Windows, check out the release tag/branch.
2. Build the packages locally so you have an `rc.1` feed:
   ```bash
   npm ci && npm run build:all
   scripts/pack.sh 1.0.0-rc.1      # produces ./nupkgs/*.1.0.0-rc.1.nupkg
   ```
3. Create a **classic ASP.NET MVC 5 Web Application** (.NET Framework 4.8, `packages.config` — *not*
   an SDK-style project). In Visual Studio: New Project → "ASP.NET Web Application (.NET Framework)"
   → MVC.
4. Add `./nupkgs` as a local NuGet source and install (these pull the `net48` lib + MVC5 deps):
   ```
   AlisReactive, AlisReactive.DesignSystem, AlisReactive.Fusion, AlisReactive.Native,
   AlisReactive.FluentValidator   (all 1.0.0-rc.1)
   ```

## The five checks

### 1. Asset landing (Task 3a — cross-platform-ish, confirm on Windows)
After build, confirm the shipped targets copied the version-stamped assets to a servable path:
- [ ] `Content\alisreactive\alis-reactive.1.0.0-rc.1.js`
- [ ] `Content\alisreactive\design-system.1.0.0-rc.1.css`
- [ ] `Content\alisreactive\syncfusion.1.0.0-rc.1.css`

(SDK-style net10 consumers instead get `wwwroot\scripts\*.js` + `wwwroot\css\*.css` — verify that
path separately on the net10 consumer.) If the classic Web Application Project does not auto-include
`Content\` files on publish, record the exact consumer-side include step needed (feeds the install docs).

### 2. Syncfusion `.Render()` executes on net48
The net48 package depends on `Syncfusion.EJ2.MVC5` (a *different* assembly than net10's
`Syncfusion.EJ2.AspNet.Core`). Render a view with a Fusion component (e.g. `FusionDropDownList`):
- [ ] The `Html.EJS()....Render()` chain produces markup with no runtime exception.
- [ ] The component initializes in the browser (dropdown opens, options show).

### 3. Client validation via the MVC5 `DependencyResolver` bridge
`ReactiveValidator<T>` metadata resolution on net48 goes through MVC5's
`DependencyResolver.SetResolver` bridge (no test/sandbox covers this today):
- [ ] Register the validator in `Global.asax`/DI per the install docs.
- [ ] Submit invalid data; confirm client-side validation errors render from the recorded metadata.

### 4. Native input HTML markup on net48
Native inputs on net48 are emitted by MVC5 `HtmlHelper.TextBoxFor`-family helpers (different markup
than net10 Tag Helpers — name/value encoding, `data-val-*` attributes):
- [ ] Inspect a rendered `Html.InputField(...).NativeTextBox(...)` — confirm the element `id`/`name`
      match what the runtime expects (plan IDs resolve via `getElementById`).
- [ ] Confirm a reactive interaction (e.g. live-clear, conditional show) works against that markup.

### 5. `System.Text.Json` 9.x binding redirects
STJ 9 on .NET Framework 4.8 commonly needs assembly binding redirects in `web.config`; classic
`packages.config` apps do not always add them automatically:
- [ ] App runs without a `FileLoadException`/binding error on `System.Text.Json` (or its deps).
- [ ] If a redirect is required, capture the exact `web.config` `<bindingRedirect>` and add it to the
      net48 install docs.

## Sign-off
RC1 net48 sign-off requires all five boxes checked in a browser on Windows, OR an explicit, recorded
decision to ship RC1 with net48 runtime marked "build/pack verified, runtime boot pending" and the
gap disclosed in the release notes. Do not report this done from a macOS run.
