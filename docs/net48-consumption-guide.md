# Consuming Alis.Reactive from .NET Framework 4.8 (Old-Style .csproj + packages.config)

> **Constraint:** The consuming project is an old-style .csproj with packages.config.
> No migration to SDK-style, no PackageReference, no porting to modern .NET.

## Why Project References Fail

Old-style MSBuild does NOT call `GetTargetFrameworks` / `GetTargetFrameworkProperties` —
the protocol SDK-style projects use to negotiate which TFM to pick from a multi-targeted
library. The old project system looks for output at `bin/Debug/` (root), but multi-targeted
SDK projects produce output under `bin/Debug/net48/` and `bin/Debug/net10.0/`.

**Result:** "namespace not found" (can't find the assembly) or "Windows identifier" error
(accidentally picks the net10.0 output, which may reference Windows-only APIs).

**Evidence:**
- [dotnet/project-system#2488](https://github.com/dotnet/project-system/issues/2488) — Microsoft confirms cross-style refs are "best effort"
- [dotnet/sdk#1151](https://github.com/dotnet/sdk/issues/1151) — TFM resolution failure for non-SDK consumers
- [dotnet/msbuild#4183](https://github.com/dotnet/msbuild/issues/4183) — TFM negotiation is SDK-to-SDK only

**Fix:** Use NuGet packages instead of project references.

---

## Step-by-Step: NuGet Packages via packages.config (Windows)

packages.config correctly consumes multi-targeted `.nupkg` files. NuGet picks the `lib/net48/`
folder (exact TFM match). No TFM negotiation needed — it's just file-folder matching.

### Step 1: Build the NuGet packages

From the Alis.Reactive repo root on the `feature/nuget-packaging` branch:

```powershell
dotnet pack Alis.Reactive\Alis.Reactive.csproj -c Release -o C:\NuGetLocal
dotnet pack Alis.Reactive.Native\Alis.Reactive.Native.csproj -c Release -o C:\NuGetLocal
dotnet pack Alis.Reactive.Fusion\Alis.Reactive.Fusion.csproj -c Release -o C:\NuGetLocal
dotnet pack Alis.Reactive.FluentValidator\Alis.Reactive.FluentValidator.csproj -c Release -o C:\NuGetLocal
```

Each project packs per-project (not blanket `dotnet pack`) to avoid packing test/sandbox projects.
The `-o C:\NuGetLocal` creates a flat local NuGet feed.

> **Note:** `Microsoft.NETFramework.ReferenceAssemblies` is in the csproj, so no .NET Framework
> Targeting Pack install is needed — `dotnet pack` works on any machine.

### Step 2: Add local feed in Visual Studio

**Tools** > **NuGet Package Manager** > **Package Manager Settings** > **Package Sources**

Click the green `+`, set:
- Name: `Local`
- Source: `C:\NuGetLocal`

Click **Update**, then **OK**.

> **PREREQUISITE:** nuget.org must remain as a configured source. NuGet resolves transitive
> dependencies (System.Text.Json, etc.) from nuget.org. If it's removed, installation fails.

### Step 3: Install packages

Open **Package Manager Console** (Tools > NuGet Package Manager > Package Manager Console):

```powershell
Install-Package Alis.Reactive -Version 1.0.0-preview.1 -Source "C:\NuGetLocal" -IncludePrerelease
Install-Package Alis.Reactive.Native -Version 1.0.0-preview.1 -Source "C:\NuGetLocal" -IncludePrerelease
```

> **`-IncludePrerelease` is required** for preview versions. Without it, NuGet reports "package not found."
>
> **Use the full path** (`C:\NuGetLocal`) instead of the source name (`Local`) to avoid
> "Unable to find source" errors if the name was registered differently.

NuGet will:
- Add `<package>` entries to `packages.config` (flat list, includes all transitives)
- Add `<Reference>` with `<HintPath>` to the `.csproj`
- Auto-upgrade any existing lower-version transitive dependencies (e.g., `CompilerServices.Unsafe` 4.x → 6.1.0)

### Step 4: Copy JS and CSS assets

.NET Framework 4.8 does **NOT** get static web assets automatically (that's an ASP.NET Core feature).
Copy these files manually into your web project:

- `alis-reactive.js` → your project's `Scripts/` or `js/` folder
- `design-system.css` → your project's `Content/` or `css/` folder

Source files are in `Alis.Reactive.SandboxApp/wwwroot/js/` and `wwwroot/css/`.

### Step 5: Add binding redirects

**Do NOT rely on `Add-BindingRedirect`** — it misses `System.IO.Pipelines` (discovered in commit `4680c58`).

Replace your **entire** `<runtime><assemblyBinding>` section in `Web.config` with the verified block below.
Only one `<assemblyBinding>` element is allowed per `<runtime>` section.

> **If you already have binding redirects** for MVC, WebPages, Newtonsoft.Json, etc., the block
> below includes those. Keep your own versions if they differ (e.g., you're on MVC 5.2.x instead of 5.3.0).
> The Alis.Reactive-specific redirects (System.Text.Json ecosystem) are clearly marked.

```xml
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">

    <!-- ═══ Standard MVC 5 redirects (keep your existing versions if they differ) ═══ -->
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
    <!-- NuGet 9.0.14 → Assembly 9.0.0.14 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Text.Json" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <!-- NuGet 6.1.0 → Assembly 6.0.1.0 (TRAP: NuGet version != assembly version!) -->
    <dependentAssembly>
      <assemblyIdentity name="System.Runtime.CompilerServices.Unsafe" publicKeyToken="b03f5f7f11d50a3a" />
      <bindingRedirect oldVersion="0.0.0.0-6.0.1.0" newVersion="6.0.1.0" />
    </dependentAssembly>
    <!-- NuGet 4.5.1 → Assembly 4.0.3.0 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Buffers" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
    </dependentAssembly>
    <!-- NuGet 4.6.0 → Assembly 4.0.2.0 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Memory" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.2.0" newVersion="4.0.2.0" />
    </dependentAssembly>
    <!-- NuGet 9.0.14 → Assembly 9.0.0.14 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Text.Encodings.Web" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <!-- NuGet 9.0.14 → Assembly 9.0.0.14 -->
    <dependentAssembly>
      <assemblyIdentity name="Microsoft.Bcl.AsyncInterfaces" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>
    <!-- NuGet 4.5.4 → Assembly 4.2.0.1 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Threading.Tasks.Extensions" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.2.0.1" newVersion="4.2.0.1" />
    </dependentAssembly>
    <!-- NuGet 4.5.0 → Assembly 4.1.4.0 -->
    <dependentAssembly>
      <assemblyIdentity name="System.Numerics.Vectors" publicKeyToken="b03f5f7f11d50a3a" />
      <bindingRedirect oldVersion="0.0.0.0-4.1.4.0" newVersion="4.1.4.0" />
    </dependentAssembly>
    <!-- NuGet 4.5.0 → Assembly 4.0.3.0 -->
    <dependentAssembly>
      <assemblyIdentity name="System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
    </dependentAssembly>
    <!-- NuGet 9.0.14 → Assembly 9.0.0.14 -->
    <dependentAssembly>
      <assemblyIdentity name="System.IO.Pipelines" publicKeyToken="cc7b13ffcd2ddd51" />
      <bindingRedirect oldVersion="0.0.0.0-9.0.0.14" newVersion="9.0.0.14" />
    </dependentAssembly>

  </assemblyBinding>
</runtime>
```

> **Source:** Every binding redirect was verified by running `[System.Reflection.AssemblyName]::GetAssemblyName()`
> on the actual DLLs (commit `b6195f1`). The `System.IO.Pipelines` redirect was the final missing
> piece (commit `4680c58`).

---

## NuGet Version vs Assembly Version — The #1 Trap

The NuGet package version and the DLL's assembly version are **different**. If your binding
redirect uses the NuGet version instead of the assembly version, you get `FileLoadException`
at runtime.

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

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `FileNotFoundException: Could not load 'X'` | Missing binding redirect | Add the redirect — most likely `System.IO.Pipelines` |
| `FileLoadException: assembly version mismatch` | Wrong version in `newVersion` | Use assembly version (e.g., `9.0.0.14`), NOT NuGet version (e.g., `9.0.14`) |
| `Unable to find package` | Preview version | Add `-IncludePrerelease` to `Install-Package` |
| `Unable to find source 'Local'` | Source name mismatch | Use full path `C:\NuGetLocal` instead of name |
| `Could not install package` | Version conflict with existing package | NuGet auto-upgrades lower versions; check for `allowedVersions` pins |
| Namespace still not found | Wrong DLL (net10.0 instead of net48) | Verify the `lib/net48/` DLL is referenced |
| `EntryPointNotFoundException` with `Unsafe.As` | Known issue with `CompilerServices.Unsafe` 6.1.0 on net48 Release builds | Downgrade to NuGet 6.0.0 ([dotnet/maintenance-packages#184](https://github.com/dotnet/maintenance-packages/issues/184)) |
| Missing JS/CSS in browser | net48 doesn't get static web assets | Copy `alis-reactive.js` and `design-system.css` manually |

---

## Reference Links

| Topic | URL |
|-------|-----|
| Why project refs fail (tracking issue) | https://github.com/dotnet/project-system/issues/2488 |
| TFM resolution failure | https://github.com/dotnet/sdk/issues/1151 |
| MSBuild cross-targeting | https://github.com/dotnet/msbuild/issues/4183 |
| Local NuGet feeds | https://learn.microsoft.com/en-us/nuget/hosting-packages/local-feeds |
| NuGet dependency resolution | https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution |
| Multi-targeting for NuGet packages | https://learn.microsoft.com/en-us/nuget/create-packages/supporting-multiple-target-frameworks |
| Install-Package reference | https://learn.microsoft.com/en-us/nuget/reference/ps-reference/ps-ref-install-package |
| packages.config reference | https://learn.microsoft.com/en-us/nuget/reference/packages-config |
| Binding redirects | https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions |
| bindingRedirect element | https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/bindingredirect-element |
| assemblyBinding element | https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/assemblybinding-element-for-runtime |
| Target frameworks reference | https://learn.microsoft.com/en-us/dotnet/standard/frameworks |
| .NET Standard compatibility | https://learn.microsoft.com/en-us/dotnet/standard/net-standard |
| CompilerServices.Unsafe known issue | https://github.com/dotnet/maintenance-packages/issues/184 |

---

## Verified Reference Implementation

A complete working example exists at `tests/Alis.Reactive.Net48.SmokeTest/`:
- Old-style `.csproj` with `packages.config`
- MVC 5.3, .NET Framework 4.8
- All 16 binding redirects verified via DLL inspection
- Demonstrates: DomReady, CustomEvent, InputField, NativeButton, NativeDrawer, HTTP GET
- Tested on IIS Express (Windows)
