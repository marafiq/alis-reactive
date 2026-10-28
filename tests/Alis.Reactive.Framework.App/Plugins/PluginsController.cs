using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plugins;

/// <summary>A plugin an app registers by name, reached by a page's pipelines (capability G).</summary>
public sealed class PluginsController : Controller
{
    public IActionResult Tally() => View(new TallyPage());
}

/// <summary>A page that counts on the tally plugin.</summary>
public sealed class TallyPage;
