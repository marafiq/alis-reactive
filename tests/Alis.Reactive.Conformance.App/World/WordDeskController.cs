using System.Text.Json;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    [HttpGet(WorldPages.WordSlip)]
    public ContentResult Slip()
    {
        var word = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Word;
        var json = HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        return Content(WorldArea.Page("The word slip", $"<p>{WorldArea.Text(JsonSerializer.Serialize(new WordSlip(word, word.Length), json))}</p>"), WorldArea.Html);
    }
}

public sealed record WordSlip(string Word, int Letters);
