using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PhotoBadgeController : Controller
{
    private const string TheSeventhVersion = "\"v7\"";

    [HttpGet(CarePages.PhotoBadge)]
    public IActionResult Badge() => View("/Care/Residents/PhotoBadge.cshtml", new PhotoBadge("Ada Hart"));

    [HttpGet(PhotoBadgeEndpoints.Photo)]
    [HttpHead(PhotoBadgeEndpoints.Photo)]
    public IActionResult Photo()
    {
        Response.Headers.ETag = TheSeventhVersion;
        return Json(new PhotoFacts(2048));
    }

    [HttpGet(PhotoBadgeEndpoints.OtherPhoto)]
    public IActionResult OtherPhoto() => Json(new PhotoFacts(1024));
}

public sealed record PhotoBadge(string Resident);

public sealed record PhotoFacts(int Size);

public static class PhotoBadges
{
    public static readonly AppEvent CheckPhoto = AppEvent.Declare();

    public static readonly AppEvent CheckTheOther = AppEvent.Declare();

    public static readonly AppEvent LoadPhoto = AppEvent.Declare();
}

public static class PhotoBadgeEndpoints
{
    private const string Root = CarePages.Area + "residents/photo-badge/";

    public const string Photo = Root + "photo";

    public const string OtherPhoto = Root + "other-photo";
}
