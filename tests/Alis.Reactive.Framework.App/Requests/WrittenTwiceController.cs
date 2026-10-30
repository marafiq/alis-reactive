using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A booking whose request writes one part twice. Routed under the Requests slice, its view lives in the slice's folder.</summary>
[Route("Requests/WrittenTwice")]
public sealed class WrittenTwiceController : Controller
{
    [HttpGet("")]
    public IActionResult Index(RequestPart part) => View("/Requests/WrittenTwice.cshtml", new WrittenTwicePage { Part = part });
}
