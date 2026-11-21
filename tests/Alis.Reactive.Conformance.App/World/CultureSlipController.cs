using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class CultureSlipController : Controller
{
    private const decimal Written = 1234.5m;

    [HttpGet(WorldPages.CultureSlip)]
    public ContentResult Slip()
    {
        var culture = CultureInfo.CurrentCulture;
        return Content(WorldArea.Page("The culture slip", $"<p>{WorldArea.Text(culture.CompareInfo.Name + " " + Written.ToString("N1", culture))}</p>"), WorldArea.Html);
    }
}
