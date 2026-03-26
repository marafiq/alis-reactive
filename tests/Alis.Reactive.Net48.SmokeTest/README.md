# Alis.Reactive — .NET Framework 4.8 Smoke Test

Old-style .csproj + packages.config MVC 5.3 app that consumes Alis.Reactive via local NuGet packages.
Proves the framework works end-to-end on .NET Framework 4.8 with IIS Express.

## Prerequisites

- Windows 10/11
- Visual Studio 2022 (any edition) — includes IIS Express x64
- .NET 10 SDK — needed to `dotnet pack` the library projects

## Build

```powershell
cd tests\Alis.Reactive.Net48.SmokeTest

# 1. Restore NuGet packages (nuget.config points to local nupkgs/ feed)
nuget restore

# 2. Build — MSBuild auto-runs pack-local.ps1 on first build to create nupkgs/
msbuild Alis.Reactive.Net48.SmokeTest.csproj /p:Configuration=Release
```

The `PackLocalNuGetFeed` MSBuild target runs `pack-local.ps1` automatically before build.
It packs all 4 library projects into `../../nupkgs/`. Skips if packages already exist —
delete the `nupkgs/` folder to force a re-pack.

## Run with IIS Express (x64)

IIS Express x64 is required for .NET Framework 4.8 apps. The x64 version lives at:

```
C:\Program Files\IIS Express\iisexpress.exe
```

> **Do NOT use the x86 version** at `C:\Program Files (x86)\IIS Express\iisexpress.exe` —
> it may fail with assembly loading errors on 64-bit systems.

### Option A: Command line (recommended)

```powershell
# From the smoke test root directory:
& "C:\Program Files\IIS Express\iisexpress.exe" /path:"%CD%" /port:5221
```

Then open `http://localhost:5221` in your browser.

### Option B: With an applicationhost.config

Create a file `Properties\applicationhost.config` (or use the VS-generated one):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.applicationHost>
    <sites>
      <site name="Alis.Reactive.Net48.SmokeTest" id="1">
        <application path="/" applicationPool="Clr4IntegratedAppPool">
          <virtualDirectory path="/" physicalPath="%SMOKE_TEST_DIR%" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:5221:localhost" />
        </bindings>
      </site>
    </sites>
  </system.applicationHost>
</configuration>
```

Then run:

```powershell
& "C:\Program Files\IIS Express\iisexpress.exe" /config:Properties\applicationhost.config /site:"Alis.Reactive.Net48.SmokeTest"
```

### Option C: Visual Studio

1. Open `Alis.Reactive.Net48.SmokeTest.csproj` in Visual Studio 2022
2. Right-click project > Properties > Web > check "Use Local IIS Web server" or "IIS Express"
3. Set port to `5221`
4. Press F5 (or Ctrl+F5 for without debugger)

VS automatically uses IIS Express x64 on 64-bit systems.

## What it tests

| Feature | How it's tested |
|---------|----------------|
| DomReady trigger | Sets text + adds CSS class on page load |
| CustomEvent trigger | Button click → Dispatch → Show/Hide section |
| InputField (NativeTextBox) | Label + validation slot rendering |
| NativeButton + .Reactive() | Click event wiring via plan |
| NativeDrawer | Open/Close + HTTP GET with Into() |
| Plan JSON rendering | Raw JSON displayed on page for inspection |

## Binding redirects

All 16 binding redirects in `Web.config` were verified by running
`[System.Reflection.AssemblyName]::GetAssemblyName()` on each DLL.
See `docs/net48-consumption-guide.md` for the full version mapping table.
