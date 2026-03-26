# Consuming Alis.Reactive from .NET Framework 4.8

> **Definitive guide for old-style .csproj + packages.config projects.**
> No migration to SDK-style, no PackageReference, no porting to modern .NET.
>
> Verified reference: `tests/Alis.Reactive.Net48.SmokeTest/`

---

## Why Project References Fail

Old-style MSBuild does NOT call `GetTargetFrameworks` / `GetTargetFrameworkProperties` --
the SDK-to-SDK protocol for negotiating which TFM to pick. The old project system looks for
output at `bin/Debug/` (root), but multi-targeted SDK projects produce output under
`bin/Debug/net48/` and `bin/Debug/net10.0/`.

**Result:** "namespace not found" or accidentally loads the wrong TFM output.

**Fix:** Use NuGet packages instead of project references. `packages.config` correctly
resolves the `lib/net48/` folder from `.nupkg` files -- exact TFM matching, no negotiation.

---

## Prerequisites

| Tool | Why | How to Get |
|------|-----|-----------|
| **Visual Studio 2022** | MSBuild 17.x for building .csproj | Already installed |
| **nuget.exe** | `packages.config` restore (not `dotnet restore`) | `curl -o ~/.dotnet/tools/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe` |
| **.NET SDK 10+** | Building the library NuGet packages | `dotnet --version` to check |
| **.NET 4.8 Targeting Pack** | Not needed if libraries include `Microsoft.NETFramework.ReferenceAssemblies` | Auto-downloaded by NuGet |

### Critical: `dotnet restore` Does NOT Work

`dotnet restore` and `msbuild -restore` only handle SDK-style projects with `<PackageReference>`.
For `packages.config`, you **must** use `nuget.exe restore`. This is the #1 gotcha.

---

## Step-by-Step Setup

### Step 1: Build and Pack the NuGet Packages

From the Alis.Reactive repo root:

```powershell
# Pack all library projects (order matters -- core first)
dotnet pack Alis.Reactive/Alis.Reactive.csproj -c Release -o nupkgs
dotnet pack Alis.Reactive.Native/Alis.Reactive.Native.csproj -c Release -o nupkgs
dotnet pack Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj -c Release -o nupkgs
dotnet pack Alis.Reactive.FluentValidator/Alis.Reactive.FluentValidator.csproj -c Release -o nupkgs
```

Or use the automated script: `powershell -File tests/Alis.Reactive.Net48.SmokeTest/pack-local.ps1`

### Step 2: Configure NuGet Sources

Create or update `nuget.config` next to your `.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <!-- Restore packages next to .csproj, not a global location -->
    <add key="repositoryPath" value="packages" />
  </config>
  <packageSources>
    <!-- Local feed FIRST (your packed .nupkg files) -->
    <add key="Local" value="<path-to-nupkgs-folder>" />
    <!-- nuget.org for transitive dependencies -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
</configuration>
```

### Step 3: Install Packages

**Option A: Visual Studio Package Manager Console**

```powershell
Install-Package Alis.Reactive -Version 1.0.0-preview.1 -IncludePrerelease
Install-Package Alis.Reactive.Native -Version 1.0.0-preview.1 -IncludePrerelease
Install-Package Alis.Reactive.Fusion -Version 1.0.0-preview.1 -IncludePrerelease
Install-Package Alis.Reactive.FluentValidator -Version 1.0.0-preview.1 -IncludePrerelease
```

> `-IncludePrerelease` is required for preview versions. Without it, NuGet reports "not found."

**Option B: nuget.exe CLI (CI / headless)**

```bash
nuget.exe restore packages.config \
  -SolutionDirectory . \
  -ConfigFile nuget.config
```

### Step 4: Install Syncfusion EJ2 MVC5

If your views use `@Html.EJS().ScriptManager()` or any Syncfusion component HTML helpers:

```powershell
# Via Package Manager Console
Install-Package Syncfusion.EJ2.MVC5 -Version 32.2.8
```

This installs three packages:
- `Syncfusion.EJ2.MVC5` -- the MVC helpers (`Html.EJS()`)
- `Syncfusion.Licensing` -- required runtime dependency
- `Syncfusion.EJ2.JavaScript` -- client scripts (optional if using CDN)

**Add assembly references to .csproj** (if not added automatically):

```xml
<Reference Include="Syncfusion.EJ2">
  <HintPath>packages\Syncfusion.EJ2.MVC5.32.2.8\lib\net462\Syncfusion.EJ2.dll</HintPath>
</Reference>
<Reference Include="Syncfusion.Licensing">
  <HintPath>packages\Syncfusion.Licensing.32.2.8\lib\net462\Syncfusion.Licensing.dll</HintPath>
</Reference>
```

> **Trap:** The MVC5 package bundles `Syncfusion.Licensing.dll` inside its own `lib/` folder,
> but MSBuild may not copy it to `bin/`. Point the HintPath to the standalone
> `Syncfusion.Licensing.32.2.8` package folder instead.

### Step 5: Add Roslyn Compiler Provider

Required for runtime Razor compilation with C# 7.3+ features:

```powershell
Install-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -Version 4.1.0
```

In your root `Web.config`, add the `system.codedom` section:

```xml
<system.codedom>
  <compilers>
    <compiler language="c#;cs;csharp" extension=".cs"
              type="Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider, Microsoft.CodeDom.Providers.DotNetCompilerPlatform, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
              warningLevel="4" compilerOptions="/langversion:default /nowarn:1659;1699;2008;2049" />
  </compilers>
</system.codedom>
```

Copy the Roslyn compiler to `bin/roslyn` after build (add to `.csproj`):

```xml
<Target Name="CopyRoslynCompiler" AfterTargets="Build">
  <ItemGroup>
    <RoslynFiles Include="packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.4.1.0\tools\Roslyn-4.1.0\*.*" />
  </ItemGroup>
  <MakeDir Directories="$(OutputPath)roslyn" />
  <Copy SourceFiles="@(RoslynFiles)" DestinationFolder="$(OutputPath)roslyn" SkipUnchangedFiles="true" />
</Target>
```

### Step 6: Configure Views/Web.config

Register namespaces for Razor IntelliSense and implicit `@using`:

```xml
<system.web.webPages.razor>
  <pages pageBaseType="System.Web.Mvc.WebViewPage">
    <namespaces>
      <add namespace="System.Web.Mvc" />
      <add namespace="System.Web.Mvc.Ajax" />
      <add namespace="System.Web.Mvc.Html" />
      <add namespace="System.Web.Routing" />
      <!-- Alis.Reactive framework -->
      <add namespace="Alis.Reactive" />
      <add namespace="Alis.Reactive.Native.Extensions" />
      <add namespace="Alis.Reactive.Native.Components" />
      <add namespace="Alis.Reactive.Native.AppLevel" />
      <!-- Only if using Syncfusion components -->
      <add namespace="Syncfusion.EJ2" />
    </namespaces>
  </pages>
</system.web.webPages.razor>
```

### Step 7: Copy JS and CSS Assets

.NET Framework 4.8 does NOT get static web assets automatically (ASP.NET Core feature).
Copy manually or automate with an MSBuild target:

```xml
<Target Name="CopyAssets" BeforeTargets="BeforeBuild">
  <MakeDir Directories="css;js" />
  <Copy SourceFiles="<repo>\Alis.Reactive.SandboxApp\wwwroot\css\design-system.css"
        DestinationFolder="css" SkipUnchangedFiles="true" />
  <Copy SourceFiles="<repo>\Alis.Reactive.SandboxApp\wwwroot\js\alis-reactive.js"
        DestinationFolder="js" SkipUnchangedFiles="true" />
</Target>
```

### Step 8: Add Binding Redirects

**Do NOT rely on `Add-BindingRedirect`** -- it misses `System.IO.Pipelines` and gets
assembly versions wrong.

Replace your **entire** `<runtime><assemblyBinding>` section in the root `Web.config`:

```xml
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">

    <!-- ═══ Standard MVC 5 redirects ═══ -->
    <dependentAssembly>
      <assemblyIdentity name="System.Web.Helpers" publicKeyToken="31bf3856ad364e35" />
      <bindingRedirect oldVersion="1.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Web.Mvc" publicKeyToken="31bf3856ad364e35" />
      <bindingRedirect oldVersion="1.0.0.0-5.3.0.0" newVersion="5.3.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Web.WebPages" publicKeyToken="31bf3856ad364e35" />
      <bindingRedirect oldVersion="1.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Web.WebPages.Razor" publicKeyToken="31bf3856ad364e35" />
      <bindingRedirect oldVersion="1.0.0.0-3.0.0.0" newVersion="3.0.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" />
      <bindingRedirect oldVersion="0.0.0.0-13.0.0.0" newVersion="13.0.0.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="Microsoft.Web.Infrastructure" publicKeyToken="31bf3856ad364e35" />
      <bindingRedirect oldVersion="0.0.0.0-2.0.0.0" newVersion="2.0.0.0" />
    </dependentAssembly>

    <!-- ═══ Alis.Reactive dependency redirects (ADD THESE) ═══ -->
    <dependentAssembly>
      <assemblyIdentity name="System.Text.Json" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" />
      <bindingRedirect oldVersion="0.0.0.0-6.0.1.0" newVersion="6.0.1.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Buffers" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Memory" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.2.0" newVersion="4.0.2.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Text.Encodings.Web" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="Microsoft.Bcl.AsyncInterfaces" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Threading.Tasks.Extensions" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.2.0.1" newVersion="4.2.0.1" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.Numerics.Vectors" publicKeyToken="b03f5f7f11d50a3a" />
      <bindingRedirect oldVersion="0.0.0.0-4.1.4.0" newVersion="4.1.4.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
    </dependentAssembly>
    <dependentAssembly>
      <assemblyIdentity name="System.IO.Pipelines" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>

  </assemblyBinding>
</runtime>
```

---

## Building (Two Build Systems)

**You need MSBuild (VS) to build, not `dotnet build`.**

`dotnet build` targets SDK-style projects. Old-style `.csproj` with `packages.config` requires
the full .NET Framework MSBuild from Visual Studio.

```bash
# 1. Restore packages (packages.config style -- NOT dotnet restore)
nuget.exe restore packages.config -SolutionDirectory . -ConfigFile nuget.config

# 2. Build with Visual Studio MSBuild
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" \
  YourProject.csproj -v:minimal
```

> **From Visual Studio:** just build normally (F5 / Ctrl+Shift+B). VS handles the restore
> and MSBuild invocation internally.

---

## NuGet Version vs Assembly Version -- The #1 Trap

The NuGet package version and the DLL's assembly version are **different**. If your binding
redirect uses the NuGet version instead of the assembly version, you get `FileLoadException`.

| Assembly | NuGet Version | Assembly Version | publicKeyToken |
|---|---|---|---|
| System.Text.Json | 9.0.14 | **9.0.0.14** | cc7b13ffcd2ddd51 |
| System.Text.Encodings.Web | 9.0.14 | **9.0.0.14** | cc7b13ffcd2ddd51 |
| System.IO.Pipelines | 9.0.14 | **9.0.0.14** | cc7b13ffcd2ddd51 |
| Microsoft.Bcl.AsyncInterfaces | 9.0.14 | **9.0.0.14** | cc7b13ffcd2ddd51 |
| System.Runtime.CompilerServices.Unsafe | 6.1.0 | **6.0.1.0** | b03f5f7f11d50a3a |
| System.Buffers | 4.5.1 | **4.0.3.0** | cc7b13ffcd2ddd51 |
| System.Memory | 4.6.0 | **4.0.2.0** | cc7b13ffcd2ddd51 |
| System.Threading.Tasks.Extensions | 4.5.4 | **4.2.0.1** | cc7b13ffcd2ddd51 |
| System.Numerics.Vectors | 4.5.0 | **4.1.4.0** | b03f5f7f11d50a3a |
| System.ValueTuple | 4.5.0 | **4.0.3.0** | cc7b13ffcd2ddd51 |

To find the real assembly version of any DLL:

```powershell
[System.Reflection.AssemblyName]::GetAssemblyName("C:\path\to\Assembly.dll").Version
```

---

## Complete packages.config Reference

All packages needed for a full Alis.Reactive + Syncfusion MVC5 project:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <!-- ASP.NET MVC 5 -->
  <package id="Microsoft.AspNet.Mvc" version="5.3.0" targetFramework="net48" />
  <package id="Microsoft.AspNet.Razor" version="3.3.0" targetFramework="net48" />
  <package id="Microsoft.AspNet.WebPages" version="3.3.0" targetFramework="net48" />
  <package id="Microsoft.Web.Infrastructure" version="2.0.0" targetFramework="net48" />
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net48" />
  <!-- System.Text.Json ecosystem (Alis.Reactive dependency) -->
  <package id="System.Text.Json" version="9.0.14" targetFramework="net48" />
  <package id="System.Text.Encodings.Web" version="9.0.14" targetFramework="net48" />
  <package id="System.Buffers" version="4.5.1" targetFramework="net48" />
  <package id="System.Memory" version="4.6.0" targetFramework="net48" />
  <package id="System.Runtime.CompilerServices.Unsafe" version="6.1.0" targetFramework="net48" />
  <package id="System.Numerics.Vectors" version="4.5.0" targetFramework="net48" />
  <package id="Microsoft.Bcl.AsyncInterfaces" version="9.0.14" targetFramework="net48" />
  <package id="System.Threading.Tasks.Extensions" version="4.5.4" targetFramework="net48" />
  <package id="System.ValueTuple" version="4.5.0" targetFramework="net48" />
  <package id="System.IO.Pipelines" version="9.0.14" targetFramework="net48" />
  <!-- FluentValidation -->
  <package id="FluentValidation" version="11.11.0" targetFramework="net48" />
  <!-- Roslyn compiler for runtime Razor -->
  <package id="Microsoft.CodeDom.Providers.DotNetCompilerPlatform" version="4.1.0" targetFramework="net48" />
  <!-- Syncfusion EJ2 MVC5 -->
  <package id="Syncfusion.EJ2.MVC5" version="32.2.8" targetFramework="net48" />
  <package id="Syncfusion.Licensing" version="32.2.8" targetFramework="net48" />
  <package id="Syncfusion.EJ2.JavaScript" version="32.2.8" targetFramework="net48" />
  <!-- Alis.Reactive framework (from local feed) -->
  <package id="Alis.Reactive" version="1.0.0-preview.1" targetFramework="net48" />
  <package id="Alis.Reactive.Native" version="1.0.0-preview.1" targetFramework="net48" />
  <package id="Alis.Reactive.Fusion" version="1.0.0-preview.1" targetFramework="net48" />
  <package id="Alis.Reactive.FluentValidator" version="1.0.0-preview.1" targetFramework="net48" />
</packages>
```

---

## Layout Template

```html
@using Alis.Reactive.Fusion.AppLevel
@using Alis.Reactive.Native.AppLevel
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>@ViewBag.Title</title>
    <!-- Syncfusion CSS (match your version) -->
    <link rel="stylesheet" href="https://cdn.syncfusion.com/ej2/32.2.8/tailwind3.css"/>
    <!-- Your design system CSS -->
    <link rel="stylesheet" href="/css/design-system.css" />
</head>
<body>
    @RenderBody()

    <!-- Syncfusion JS runtime (match your version) -->
    <script src="https://cdn.syncfusion.com/ej2/32.2.8/dist/ej2.min.js"></script>

    <!-- App-level components (order matters) -->
    @Html.FusionToast()
    @Html.EJS().ScriptManager()
    @Html.FusionConfirmDialog()
    @Html.NativeLoader()
    @Html.NativeDrawer()

    <!-- Alis.Reactive runtime (ESM) -->
    <script type="module" src="/js/alis-reactive.js"></script>
</body>
</html>
```

---

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `FileNotFoundException: Syncfusion.Licensing` | HintPath points to MVC5 bundled copy | Point to `packages/Syncfusion.Licensing.32.2.8/lib/net462/` |
| `CS0246: 'Syncfusion' not found` | Missing namespace in Views/Web.config | Add `<add namespace="Syncfusion.EJ2" />` |
| `CS1061: 'HtmlHelper' has no 'EJS'` | Missing Syncfusion.EJ2.MVC5 package | Install `Syncfusion.EJ2.MVC5` |
| `FileNotFoundException: Could not load 'X'` | Missing binding redirect | Add redirect -- most likely `System.IO.Pipelines` |
| `FileLoadException: assembly version mismatch` | Wrong `newVersion` in redirect | Use assembly version (e.g. `9.0.0.14`), NOT NuGet version (`9.0.14`) |
| `Unable to find package` | Preview version | Add `-IncludePrerelease` to `Install-Package` |
| `Nothing to do. None of the projects specified contain packages to restore` | Used `dotnet restore` instead of `nuget.exe restore` | Use `nuget.exe restore packages.config` |
| `CS0246: 'IHtmlHelper' not found` | Component missing `#if NET48` | File a bug -- component needs multi-target conditionals |
| Missing JS/CSS in browser | net48 doesn't get static web assets | Copy `alis-reactive.js` and `design-system.css` manually |
| Razor compilation errors at runtime | Missing Roslyn compiler provider | Install `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` + copy to bin/roslyn |

---

## Multi-Target Architecture (For Library Developers)

All four library projects target `net48;net10.0`. The pattern:

```csharp
// Usings
#if NET48
using System.Web;
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

// Class declaration
#if NET48
public class MyBuilder<TModel> : IHtmlString where TModel : class
#else
public class MyBuilder<TModel> : IHtmlContent where TModel : class
#endif

// Extension method parameter
#if NET48
public static void On<TModel>(this HtmlHelper<TModel> html, ...)
#else
public static void On<TModel>(this IHtmlHelper<TModel> html, ...)
#endif
```

**Key differences between frameworks:**

| Concept | net48 (MVC 5) | net10.0 (ASP.NET Core) |
|---------|---------------|----------------------|
| HTML helper | `HtmlHelper<TModel>` | `IHtmlHelper<TModel>` |
| HTML output | `IHtmlString` (+ `ToHtmlString()`) | `IHtmlContent` (+ `WriteTo()`) |
| Expression helper | `ExpressionHelper.GetExpressionText()` | `html.NameFor()` |
| HttpContext items | `.Contains()` + cast | `.TryGetValue()` |
| Null guard | `if (x == null) throw` | `ArgumentNullException.ThrowIfNull()` |
| DateOnly/TimeOnly | N/A (use `#if NET6_0_OR_GREATER`) | Available |
| Syncfusion | `Syncfusion.EJ2.MVC5` | `Syncfusion.EJ2.AspNet.Core` |

**Component types that DON'T need conditionals:** Input components that flow through
`InputFieldSetup<TModel, TProp>` (the abstraction handles it). Only standalone builders
with direct `HtmlHelper`/`IHtmlHelper` parameters need `#if NET48`.

---

## Component Inventory (All Multi-Target Verified)

### Native Components (11)

| Component | Type | Multi-Target |
|-----------|------|:---:|
| NativeTextBox | IInputComponent | OK |
| NativeCheckBox | IInputComponent | OK |
| NativeCheckList | IInputComponent | OK |
| NativeRadioGroup | IInputComponent | OK |
| NativeDropDown | IInputComponent | OK |
| NativeTextArea | IInputComponent | OK |
| NativeHiddenField | IInputComponent | OK |
| NativeButton | IComponent | OK |
| NativeActionLink | IComponent | OK |
| NativeDrawer | IAppLevelComponent | OK |
| NativeLoader | IAppLevelComponent | OK |

### Fusion Components (18)

| Component | Type | Multi-Target |
|-----------|------|:---:|
| FusionDropDownList | IInputComponent | OK |
| FusionDatePicker | IInputComponent | OK |
| FusionDateTimePicker | IInputComponent | OK |
| FusionTimePicker | IInputComponent | OK |
| FusionNumericTextBox | IInputComponent | OK |
| FusionAutoComplete | IInputComponent | OK |
| FusionMultiSelect | IInputComponent | OK |
| FusionInputMask | IInputComponent | OK |
| FusionFileUpload | IInputComponent | OK |
| FusionSwitch | IInputComponent | OK |
| FusionMultiColumnComboBox | IInputComponent | OK |
| FusionRichTextEditor | IInputComponent | OK |
| FusionDateRangePicker | IInputComponent | OK |
| FusionColorPicker | IInputComponent | OK |
| FusionTab | IComponent | OK |
| FusionAccordion | IComponent | OK |
| FusionToast | IAppLevelComponent | OK |
| FusionConfirm | IAppLevelComponent | OK |

---

## Reference Links

| Topic | URL |
|-------|-----|
| Why project refs fail | https://github.com/dotnet/project-system/issues/2488 |
| Local NuGet feeds | https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds |
| packages.config reference | https://learn.microsoft.com/en-us/nuget/reference/packages-config |
| Binding redirects | https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions |
| Multi-targeting for NuGet | https://learn.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks |
