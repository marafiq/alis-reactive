# Plan: Azure Sub-App Deployment — Sandbox as Virtual Application

## Context

Deploy the Sandbox app as a virtual application (sub-app) under an existing Azure App Service
at a path like `/sandbox`. Locally and during Playwright tests, it runs at root (`/`) as today.

All URLs are already PathBase-safe:
- Navigation links use `asp-area`/`asp-controller`/`asp-action` tag helpers
- Static assets use `~/` tilde paths with `asp-append-version`
- API URLs in plan JSON use MVC route resolution

## Change: `Alis.Reactive.SandboxApp/Program.cs`

Add after `var app = builder.Build();` (line 33), **before** all other middleware:

```csharp
var pathBase = app.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
    app.UsePathBase(pathBase);
```

When `PathBase` is absent or empty (local dev, Playwright), no PathBase is applied — zero behavior change.

## Deployment Configuration (Azure)

Set `PathBase` via any of:
- **Azure App Service → Configuration → Application settings**: `PathBase` = `/sandbox`
- **Or** `appsettings.Production.json`: `{ "PathBase": "/sandbox" }`
- **Or** environment variable: `PathBase=/sandbox`

ASP.NET Configuration merges all three sources automatically.

## Azure Virtual Application Setup

1. Azure Portal → App Service → **Configuration** → **Path mappings**
2. Add virtual application: Virtual path `/sandbox`, Physical path `site\sandbox`, check **Application**
3. Set `PathBase` = `/sandbox` in Application settings

## What UsePathBase handles

- Page routing: `/sandbox/Home/Index` → strips prefix → routes to `Home/Index`
- Static files: `~/css/design-system.css` → resolves to `/sandbox/css/design-system.css`
- Navigation links: tag helpers include PathBase automatically
- SignalR hubs: `MapHub("/hubs/notifications")` → accessible at `/sandbox/hubs/notifications`

## Verification

1. **Local dev**: `dotnet run` → app at `localhost:5220/` (no change)
2. **With PathBase**: `PathBase=/sandbox dotnet run` → app at `localhost:5220/sandbox/`
3. **Playwright tests**: Unchanged — no PathBase configured
