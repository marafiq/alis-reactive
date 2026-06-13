# RC1 Release Readiness — Multi-Target + Packaging Audit

Status: **AUDIT COMPLETE — task list below is definitive and verified.**
Revision (review pass): Task 1 probe independently reproduced on this machine (170 errors, only
CS0234+CS0246, zero Syncfusion errors, **zero language-version errors**); file count corrected 57→56 and
~29→27; the C# 14-on-net48 dependency (`LangVersion 14.0` + PolySharp) made explicit with its own
acceptance gate; Task 3 split into 3a (cross-platform asset-landing, achievable now) and 3b (net48 classic
web app boot — **Windows-only**).
Scope: prepare the six shipped NuGet packages for Release Candidate 1 so every package
(except `AlisReactive.NativeTagHelpers`, which is `net10.0`-only by design) targets and installs
on **both `net48` and `net10.0`**, with runtime + design-system + Syncfusion assets packaged
correctly for both project styles, and comment noise removed from internal classes.

This document is persisted (uncommitted) so a new session can execute the tasks. Every claim
below was verified by reading source and/or compiling; speculation is excluded. Where a fact
could not be compile-verified in this environment, it is marked **VERIFY AT IMPLEMENTATION** with
the exact check to run.

Verification method per the repo standard: a finding is reported only from a direct read, grep,
or build I ran myself. Build/compile evidence is cited inline.

---

## Verified Baseline (do not re-litigate — already correct)

| Package (PackageId) | csproj TargetFramework(s) | Build status I observed | Notes |
|---|---|---|---|
| `AlisReactive` (`Alis.Reactive`) | `net48;net10.0` | net48 ✅ 0/0, net10 ✅ | Razor layer already `#if NET48` (System.Web.Mvc) vs ASP.NET Core. Packs runtime JS + analyzer. |
| `AlisReactive.Native` | `net48;net10.0` | net48 ✅ 0/0, net10 ✅ | Builders already dual `IHtmlString`/`IHtmlContent` via `#if NET48`. **Reference pattern for Fusion.** |
| `AlisReactive.FluentValidator` | `net48;net10.0` | net48 ✅ 0/0, net10 ✅ | net48→FluentValidation 11; net10→FluentValidation 12. |
| `AlisReactive.DesignSystem` | `net48;net10.0` | net48 ✅ 0/0, net10 ✅ | Packs design-system CSS. |
| `AlisReactive.NativeTagHelpers` | `net10.0` | net10 ✅ | **Correct exception** — ASP.NET Core Tag Helpers, `net10.0` only by design. |
| `AlisReactive.Fusion` | `net10.0` **(GAP)** | net10 ✅ | **Must add `net48`.** This is the one packable project not yet multi-targeting. See Task 1. |

Not packable (correct): `Alis.Reactive.Analyzers` (`IsPackable=false`, packed *into* `AlisReactive`),
`Alis.Reactive.Assets` (NoTargets, build-only). Tools/tests/sandbox/examples are not packable.

Other baseline facts I verified:
- **Zero** `TODO`/`FIXME`/`HACK`/`#warning`/`NotImplementedException` in shipped source.
- Asset-delivery mechanism `build/AlisReactiveAssets.targets` already branches on
  `'$(TargetFrameworkIdentifier)' == '.NETFramework'` → `Content\alisreactive`, else → `wwwroot\scripts`/`wwwroot\css`,
  and its package-version regex already handles both the PackageReference (`{Version}/build/`) and
  packages.config (`{PackageId}.{Version}/build/`) folder layouts.
- Syncfusion package facts (nuget.org): `Syncfusion.EJ2.MVC5` **32.2.8** exists, exact version parity
  with net10's `Syncfusion.EJ2.AspNet.Core` **32.2.8**; both expose the same `Syncfusion.EJ2[.*]`
  builder namespaces and the same `Html.EJS()....Render()` chain. The *only* difference is the
  ASP.NET host types — which is why the port is `#if`-only (Task 1).

---

## Task 1 — Multi-target `Alis.Reactive.Fusion` to `net48;net10.0`  ⟵ RC1 blocker

**Why:** Goal requires all packages except NativeTagHelpers to target net48 + net10. Fusion is the
only one still `net10.0`-only.

**Verification that this is conditional-compilation only (no per-TFM runtime behavior differs), done by
compile probe — reproduced twice (original audit + an independent re-run on this machine, identical result):**
Temporarily set `<TargetFrameworks>net48;net10.0</TargetFrameworks>` on Fusion, added the net48
ItemGroup (`Syncfusion.EJ2.MVC5` 32.2.8 + `Microsoft.AspNet.Mvc` 5.3.0 + `Microsoft.NETFramework.ReferenceAssemblies`
1.0.3 + `PolySharp` + `Reference System.Web`), restored, and built `-f net48`. Result:
**170 errors, 0 warnings, across 56 files** — and *every error* is one of only two codes:
- `CS0234` "namespace `AspNetCore` does not exist in `Microsoft`" (120) — the `using Microsoft.AspNetCore.*` lines
- `CS0246` "`IHtmlContent`" (162) / "`IHtmlHelper<>`" (54) / "`IHtmlHelper`" (4) — the host types

**Zero Syncfusion API errors.** The EJ2 builders (`html.EJS().Button(...)`, `.Render()`, etc.) from the
MVC5 package resolved. This confirms the work is the same mechanical conditional-compilation already
applied in `Alis.Reactive.Native`. The probe csproj was reverted byte-for-byte (sha verified, git clean).

**C# language version on net48 (the one thing the "no logic change" framing must NOT hide):**
net48's BCL targets an older language surface, but this repo forces `<LangVersion>14.0</LangVersion>`
solution-wide (`Directory.Build.props:14`). Two things make modern C# compile against net48: `Microsoft.NETFramework.ReferenceAssemblies`
supplies the net48 BCL reference surface, and `PolySharp` polyfills the compiler-required runtime types for
features that need them (init-only setters, `required` members, records, ranges, etc.). Fusion source uses
modern C# that only compiles under this configuration — verified concretely: **8 files in `Alis.Reactive.Fusion`
use collection expressions** (`= [ … ]`); there are **0 record types in Fusion** (the wider codebase's
records/init-only usage is what makes the PolySharp dependency repo-wide). The probe build emitted **zero**
language-version errors (no `CS8xxx` "feature is not available", no `CS90xx`), which proves this configuration
already covers Fusion's current feature usage — exactly as it already does for the four net48 packages that
ship today. **This is why net48 compiles, and it is a hard dependency of Task 1, not an incidental detail.**
If a future Fusion component adopts a C# feature this configuration cannot polyfill, the net48 build will
fail with a `CS8xxx`/`CS90xx` error — see the added acceptance criterion below.

**Reference pattern (copy this exactly):**
- Helper type: `Alis.Reactive.Native/.../HtmlExtensions.cs` and `Alis.Reactive/Razor/Extensions/HtmlExtensions.cs`
  — `#if NET48 using System.Web.Mvc; #else using Microsoft.AspNetCore.Mvc.Rendering; #endif` and
  `this HtmlHelper<TModel>` vs `this IHtmlHelper<TModel>`.
- Builder content type: `Alis.Reactive.Native/Components/NativeButton/NativeButtonBuilder.cs` —
  `#if NET48 : IHtmlString` (implements `string ToHtmlString()`) `#else : IHtmlContent` (implements
  `WriteTo(TextWriter, HtmlEncoder)`) `#endif`, with `using System.Web` vs `using Microsoft.AspNetCore.Html`.
- Wrapping Syncfusion output: on net10 Syncfusion `.Render()` returns `IHtmlContent`; on MVC5 it returns
  `IHtmlString` (`MvcHtmlString`). Store and re-emit through the matching interface — same shape as
  `Alis.Reactive/Razor/InputBoundField.cs` (`Render(IHtmlString)` vs `Render(IHtmlContent)`).

**Steps:**
1. `Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj`:
   - `<TargetFramework>net10.0</TargetFramework>` → `<TargetFrameworks>net48;net10.0</TargetFrameworks>`.
   - Replace the single `<ItemGroup>` holding `Syncfusion.EJ2.AspNet.Core` + `FrameworkReference` with
     two conditional groups:
     - `Condition="'$(TargetFramework)' == 'net10.0'"`: `Syncfusion.EJ2.AspNet.Core` 32.2.8 + `FrameworkReference Include="Microsoft.AspNetCore.App"`.
     - `Condition="'$(TargetFramework)' == 'net48'"`: `Syncfusion.EJ2.MVC5` 32.2.8 + `Microsoft.AspNet.Mvc` 5.3.0
       + `Microsoft.NETFramework.ReferenceAssemblies` 1.0.3 (`PrivateAssets="all"`) + `PolySharp` 1.* (`PrivateAssets="all"`)
       + `<Reference Include="System.Web" />`. (Mirrors the net48 group in `Alis.Reactive.csproj`.)
   - The `<None ... PackagePath="build\assets\css\syncfusion.css">`, the `*.targets` `<None>` items, and the
     `VerifyFusionBundleExistsBeforePack` target are TFM-agnostic — **leave unchanged** (they pack once and
     apply to both TFMs).
2. Add the `#if NET48 / #else / #endif` host-namespace block to the **56 files** the probe flagged
   (all `Alis.Reactive.Fusion/**/*Builder.cs`, `**/*HtmlExtensions.cs`, `AppLevel/*/*Extensions.cs` that
   `using Microsoft.AspNetCore.Html` and/or `Microsoft.AspNetCore.Mvc.Rendering`). Enumerate precisely with
   (this command returns 56 — it is the authoritative file list):
   `grep -rln "Microsoft.AspNetCore" --include=*.cs Alis.Reactive.Fusion | grep -v /obj/ | grep -v /bin/`
   - 27 `*HtmlExtensions.cs` also swap the helper parameter `IHtmlHelper<TModel>`→`HtmlHelper<TModel>`
     (2 of these use the non-generic `IHtmlHelper`, accounting for the 4 `IHtmlHelper` errors above).
   - 26 `*Builder.cs` implement `IHtmlContent` → add the `IHtmlString`/`ToHtmlString()` branch.
3. Update the stale comment in `scripts/pack.sh:57` (`# ... (net10.0 only)` on the Fusion line).

**Acceptance criteria (all must hold):**
- `dotnet build Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj -f net48` → 0 errors, 0 warnings.
- `dotnet build Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj -f net10.0` → 0 errors, 0 warnings.
- **No language-version errors on net48:** the net48 build emits zero `CS8xxx` ("feature is not available
  in C# X") and zero `CS90xx` errors. This is the explicit gate for the C# 14-on-net48 dependency described
  above — it confirms `LangVersion 14.0` + `PolySharp` still cover every C# feature the Fusion source uses.
  If this fails, the failing feature must be reworked to one PolySharp polyfills (or guarded behind `#if`),
  not worked around — the "no functional change" guarantee depends on it.
- The `net10.0` compiled output is unchanged from today — i.e. every `#else` branch is byte-identical to the
  current code, so no existing net10 consumer behavior changes. (This is what makes the task "no functional change".)
- **VERIFY AT IMPLEMENTATION:** confirm the MVC5 `.Render()` return type is `IHtmlString`/`MvcHtmlString`
  by reaching compile-green on the builder `#if NET48` branch (the compiler is the proof; do not assume).
  This builder `#if` branch is the one place Task 1 is more than a namespace swap: net10 stores/re-emits
  `IHtmlContent`, net48 stores/re-emits `IHtmlString` — same shape, no behavior change per TFM, but it is
  conditional *logic*, so do not describe Task 1 as "only changing `using` lines".

**Independence:** self-contained to the Fusion project + one pack.sh comment. Touches no other package's source.
**No functional change:** net10 path preserved exactly; net48 is additive new capability.

---

## Task 2 — Pack the six NuGets and verify package contents for both targets

**Why:** Goal requires packages to "contain the runtime, and styling design system assets … packaged
correct and able to target & Install on both." The packaging is authored correctly (verified by reading
the csproj + targets); this task is the **observation that the produced `.nupkg` files actually contain it.**

**Steps:**
1. `npm run build:all` (produces `Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js`,
   `dist/css/design-system.dev.css`, `dist/css/syncfusion.dev.css` — the three `VerifyBundlesExistBeforePack`
   gates depend on these).
2. `scripts/pack.sh <rc-version>` (e.g. `1.0.0-rc.1`).
3. Unzip / inspect each `.nupkg` under `./nupkgs` and confirm:
   - `AlisReactive`: `lib/net48/` + `lib/net10.0/` assemblies; `build/AlisReactive.targets` +
     `buildTransitive/AlisReactive.targets`; `build/assets/js/alis-reactive.js` +
     `buildTransitive/assets/js/alis-reactive.js`; `analyzers/dotnet/cs/Alis.Reactive.Analyzers.dll`;
     per-TFM dependency groups (net48: `System.Text.Json` 9, `Microsoft.AspNet.Mvc` 5.3.0; net10: framework ref).
   - `AlisReactive.DesignSystem`: `lib/net48/`+`lib/net10.0/`; `build/`+`buildTransitive/AlisReactive.DesignSystem.targets`;
     `build/assets/css/design-system.css` (+ buildTransitive).
   - `AlisReactive.Fusion` (after Task 1): `lib/net48/`+`lib/net10.0/`;
     `build/`+`buildTransitive/AlisReactive.Fusion.targets`; `build/assets/css/syncfusion.css` (+ buildTransitive);
     dependency groups — net48 → `Syncfusion.EJ2.MVC5` 32.2.8; net10 → `Syncfusion.EJ2.AspNet.Core` 32.2.8.
   - `AlisReactive.Native`: `lib/net48/`+`lib/net10.0/`.
   - `AlisReactive.FluentValidator`: `lib/net48/`+`lib/net10.0/`; net48 → FluentValidation 11, net10 → 12.
   - `AlisReactive.NativeTagHelpers`: `lib/net10.0/` only (correct).

**Acceptance criteria:** every bullet above observed in the actual `.nupkg` (list contents, do not infer).
**Independence:** read-only verification after Task 1. **No functional change.**

---

## Task 3 — Verify asset delivery on a real **old-style non-SDK net48 web** consumer

**Why:** Goal explicitly notes "NET48 projects may have old style non-sdk .net web projects." The
`AlisReactiveAssets.targets` is *written* to support packages.config (the version regex handles the
`{PackageId}.{Version}` folder), but this end-to-end path is **not yet observed** on a real consumer.
This is a verification task, not a framework change.

> **Environment gate — read before starting.** A net48 *class library* builds fine cross-platform
> (verified: `dotnet build -f net48` is 0/0 on macOS via `Microsoft.NETFramework.ReferenceAssemblies`).
> But a **classic non-SDK ASP.NET MVC5 Web Application** is a different project type: its
> `Microsoft.WebApplication.targets` and, more importantly, *running* it (System.Web hosting under
> IIS Express + the .NET Framework 4.8 runtime) are **Windows-only**. Mono on macOS is not a faithful
> .NET Framework host and must not be treated as the proof. Task 3 is therefore split: **3a is
> cross-platform and achievable now; 3b requires a Windows machine** and should be deferred there if the
> session is not on Windows. Do not report 3b as done from a Mono/macOS run.

### Task 3a — Asset-landing proof (cross-platform, achievable now)
1. Create a non-SDK `.csproj` (or restore into one) that uses **packages.config** and targets `net48`.
2. Install `AlisReactive`, `AlisReactive.DesignSystem`, `AlisReactive.Fusion` (+ `.Native`, `.FluentValidator`)
   from the local `./nupkgs` feed.
3. Build, and confirm the `CopyAlisReactiveAssets` target produced, at a servable path:
   - `Content\alisreactive\alis-reactive.<version>.js`
   - `Content\alisreactive\design-system.<version>.css`
   - `Content\alisreactive\syncfusion.<version>.css`
   (version = the owning package's version, baked per-file by the targets.)
4. Cross-check an SDK-style **net10** web consumer gets `wwwroot\scripts\*.js` + `wwwroot\css\*.css`.
   (The net10 SDK consumer can additionally be *run* and browser-verified on macOS — do that here.)

**3a acceptance:** the three `Content\alisreactive\*.<version>.*` files exist after build for the
packages.config consumer, and the `wwwroot\…` files exist for the net10 SDK consumer. File-landing only;
no app run required. If the non-SDK Web Application Project does not auto-include `Content\` files in
publish, record the exact consumer-side step needed (this informs the install docs — docs are out of scope
here, but the finding is not).

### Task 3b — Runtime boot proof (Windows-only, defer if not on Windows)
1. On a **Windows** machine, open the packages.config MVC5 / .NET Framework 4.8 Web Application in
   IIS Express (or `dotnet`-incompatible classic MSBuild + IIS Express).
2. Run the app; confirm the runtime JS + CSS load in the browser and a simple reactive view boots.

**3b acceptance:** runtime + CSS load and a reactive view boots in the browser on the net48 classic web
consumer. **Mark blocked, not done, when no Windows environment is available.**

**Independence:** external consumer projects only. **No functional change** to the framework.

---

## Task 4 — Surgical comment-noise removal in internal classes (strict criteria)

**Verified finding (important — calibrates the scope):** I read **all 97** own-line `//` comments across
the shipped source and a representative sample of the internal-type XML docs. They are **overwhelmingly
rationale ("why"), not restatement noise** — e.g. net48/net10 host differences
(`PlanExtensions.cs:68`, `ReactivePlan.cs:153`), Syncfusion quirks
(`FusionAutoCompleteExtensions.cs:122`, `FusionRichTextEditorHtmlExtensions.cs:37`), runtime Shape/coercion
contracts (`ElementExpressionCompiler.cs:105`, `ReactiveArray.cs:52`), and encapsulation reasons
(`InputBoundField.cs:33`). Internal-type summaries sampled (`ValidationJob`, `ComponentRegistration`,
`InputFieldBuilder`, `RuleName`) likewise explain lifecycle/intent, not the type name. **The codebase is
already well-tended; there is little to remove. Do not invent removals to satisfy the task.**

**REMOVE only** a comment or XML summary that *restates the adjacent identifier or the literal next
statement and adds no "why"* (no domain reason, no cross-TFM nuance, no Syncfusion behavior, no runtime
contract, no encapsulation rationale).

**KEEP (never strip):** anything explaining *why*; net48 vs net10/ASP.NET-Core host differences; Syncfusion
EJ2 behavior; runtime Shape/coercion/serialization contracts; model-binding nuances; `// Internal because …`
encapsulation rationale; the packaging comments in the `.csproj`/`.targets` files (they document the asset
mechanism).

**Acceptance criteria:** build stays green for all packages on both TFMs; no rationale removed; `git diff`
shows only deletions of genuinely tautological comments. **If zero comments meet the REMOVE criterion,
record "no noise found" and close the task** — that is a valid, honest outcome given the audit above.
**Independence:** comment-only edits, no code change. **No functional change.**

---

## Observations (NOT tasks — out of scope / would be a change, listed for awareness)

- **Per-package README:** `Directory.Build.props` packs the repo-root `README.md` into all six packages, so
  every nuget.org listing shows the same root readme. Per-package READMEs would improve listing quality.
  This is a packaging-presentation choice, not a behavior issue — left out of the task list intentionally.
- **Analyzer flow:** only `AlisReactive` bundles the analyzer; it does not flow transitively to consumers of
  `.Fusion`/`.Native` who don't also reference `AlisReactive` directly. Existing behavior; not an RC1 blocker.

---

## How the next session should proceed

1. Task 1 first (it is the only code change and the RC1 blocker). Build both TFMs green, with the
   net48 build showing zero language-version errors (the explicit C# 14-on-net48 gate).
2. Task 2 (pack + inspect) — depends on Task 1.
3. Task 3a (asset-landing for packages.config + net10 consumers) — depends on Task 2's local feed;
   achievable on this macOS machine. Task 3b (net48 classic web app boots in browser) is **Windows-only** —
   defer to a Windows environment and mark blocked if none is available; do not fake it under Mono.
4. Task 4 anytime (independent, comment-only) — lowest value; the audit's own expected outcome is
   "no noise found / near-zero removals", so closing it as such is a valid result, not a miss.

All tasks are independent in source footprint and none change net10 functional behavior. Nothing in this
branch is to be committed unless the user asks.
