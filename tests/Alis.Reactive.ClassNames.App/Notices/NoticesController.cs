using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.ClassNames.App.Notices;

public sealed class NoticesController : Controller
{
    public IActionResult Index() => View(new Notice());

    /// <summary>Checks the notice with the rules the page carries; 400 with every message by field, or saved.</summary>
    [HttpPost("/Notices/Save")]
    public IActionResult Save([FromBody] Notice notice, [FromServices] NoticeRules rules)
    {
        var result = rules.Validate(notice);
        if (!result.IsValid)
            return BadRequest(new { errors = result.ErrorsByField });
        return Json(new Saved { Summary = "saved" });
    }
}
