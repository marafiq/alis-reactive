using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Forms;

/// <summary>A form is an element: a request sends the inputs inside the form it names, and none outside it.</summary>
public sealed class FormsController : Controller
{
    public IActionResult Outing() => View(new OutingPage());

    /// <summary>A page whose request sends its form's inputs but checks no form, so no form is named.</summary>
    public IActionResult Unnamed() => View(new OutingPage());

    /// <summary>Binds the outing as any endpoint does and says what it bound.</summary>
    [HttpPost]
    public IActionResult Bound([FromBody] OutingPage outing) => Json(new FormsEcho
    {
        Summary = $"resident {outing.Resident ?? "none"}, companion {outing.Companion ?? "none"}, note {outing.Note ?? "none"}",
    });
}
