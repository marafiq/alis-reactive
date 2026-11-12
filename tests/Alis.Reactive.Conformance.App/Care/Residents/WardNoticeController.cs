using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardNoticeController : Controller
{
    private const string TheSeventhVersion = "\"v7\"";

    private const string WardHeader = "X-Ward";

    [HttpGet(CarePages.WardNoticeOfTwoWards)]
    public IActionResult OfTwoWards() => View("/Care/Residents/WardNotice.cshtml", new WardNoticePage(WardNoticeEndpoints.NoticeOfTwoWards));

    [HttpGet(CarePages.WardNoticeOfOneWard)]
    public IActionResult OfOneWard() => View("/Care/Residents/WardNotice.cshtml", new WardNoticePage(WardNoticeEndpoints.NoticeOfOneWard));

    [HttpGet(WardNoticeEndpoints.NoticeOfTwoWards)]
    public IActionResult NoticeOfTwoWards()
    {
        Response.Headers.ETag = TheSeventhVersion;
        Response.Headers.Append(WardHeader, "East");
        Response.Headers.Append(WardHeader, "West");
        return NoContent();
    }

    [HttpGet(WardNoticeEndpoints.NoticeOfOneWard)]
    public IActionResult NoticeOfOneWard()
    {
        Response.Headers.ETag = TheSeventhVersion;
        Response.Headers.Append(WardHeader, "East");
        return NoContent();
    }
}

public sealed record WardNoticePage(string Notice);

public static class WardNotices
{
    public static readonly AppEvent Load = AppEvent.Declare();
}

public static class WardNoticeEndpoints
{
    private const string Root = CarePages.Area + "residents/ward-notice/";

    public const string NoticeOfTwoWards = Root + "notice/two-wards";

    public const string NoticeOfOneWard = Root + "notice/one-ward";
}
