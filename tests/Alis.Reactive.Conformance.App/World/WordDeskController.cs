using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class WordDeskController : Controller
{
    [HttpGet(WorldPages.WordDesk)]
    public ContentResult Desk()
        => Content(WorldArea.Page("The word desk", $"<a href=\"{WorldEndpoints.WordDeskAnswer}\" target=\"answer\">Ask the desk</a><iframe id=\"answer\" name=\"answer\" title=\"The desk's answer\"></iframe>"), WorldArea.Html);

    [HttpGet(WorldEndpoints.WordDeskAnswer)]
    [AtTheAnswerGate]
    public ContentResult Answer()
        => Content(WorldArea.Page("The word", $"<p>{WorldArea.Text(HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Word)}</p>"), WorldArea.Html);
}
