using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Plugins;

public sealed class PluginsController : Controller
{
    public IActionResult Tally() => View(new TallyPage());

    public IActionResult Ledger() => View(new LedgerPage());

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

public enum SecondBundle
{
    SecondTally,

    MemberlessLantern,
}
