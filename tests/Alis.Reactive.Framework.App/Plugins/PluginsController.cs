using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plugins;

/// <summary>
/// A plugin an app registers by name, reached by a page's pipelines (capability G), and the plugins the
/// runtime refuses: one no script registered, one name registered twice, one registered as no object.
/// </summary>
public sealed class PluginsController : Controller
{
    public IActionResult Tally() => View(new TallyPage());

    public IActionResult Ledger() => View(new LedgerPage());

    /// <summary>A page loaded with a second bundle of the app (Plugins/Bundles) that queues a plugin the runtime refuses.</summary>
    public IActionResult Refused(SecondBundle bundle)
    {
        ViewData["SecondBundle"] = bundle switch
        {
            SecondBundle.SecondTally => "second-tally",
            SecondBundle.MemberlessLantern => "memberless-lantern",
            _ => throw new ArgumentOutOfRangeException(nameof(bundle), bundle, "No such second bundle."),
        };
        return View(new RefusedPluginPage());
    }
}

/// <summary>The second bundles a page can load.</summary>
public enum SecondBundle
{
    /// <summary>Queues a tally, a name the app's first bundle already registered.</summary>
    SecondTally,

    /// <summary>Queues a lantern that is a number, which holds no members.</summary>
    MemberlessLantern,
}
