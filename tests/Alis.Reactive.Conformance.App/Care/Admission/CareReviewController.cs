using System.Globalization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareReviewController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.CareReview)]
    public IActionResult Review() => View("/Care/Admission/CareReview.cshtml", new CareReview());

    [HttpPost(CareReviewEndpoints.Reviews)]
    public IActionResult Save([FromForm] CareReview review)
    {
        if (!ModelState.IsValid) return BadRequest();
        return Content($"{review.Note}: level {((int)review.Level).ToString(CultureInfo.InvariantCulture)}", PlainText);
    }
}

public enum ReviewLevel
{
    Independent = 1,
    Assisted = 2,
}

public sealed class CareReview
{
    public string? Note { get; set; }

    public ReviewLevel Level { get; set; }
}

public static class CareReviewEndpoints
{
    public const string Reviews = CarePages.CareReview + "/reviews";
}
