using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NoticeboardController : Controller
{
    private const string TornNoticeHtml =
        "<h2>Notice</h2><p>Visiting hours chan</p><script type=\"application/json\" data-reactive-plan>{\"version\":</script>";

    [HttpGet(CarePages.Noticeboard)]
    public IActionResult OfTheHall() => View("/Care/Record/Noticeboard.cshtml", new Noticeboard("Main hall"));

    [HttpGet(NoticeboardEndpoints.Notice)]
    public IActionResult Notice() => PartialView("/Care/Record/NoticeboardNotice.cshtml", new NoticeboardNotice("Visiting hours change on Sunday."));

    [HttpPost(NoticeboardEndpoints.TornNotice)]
    public IActionResult TornNotice() => Content(TornNoticeHtml, "text/html");
}

public sealed record Noticeboard(string Hall);

public sealed record NoticeboardNotice(string Text);

public static class NoticeboardEndpoints
{
    private const string Root = CarePages.Area + "record/noticeboard/";

    public const string Notice = Root + "notice";

    public const string TornNotice = Root + "torn-notice";
}

public static class NoticeboardButtons
{
    public static readonly AppEvent PinTheNotice = AppEvent.Declare();

    public static readonly AppEvent PinTheTornNotice = AppEvent.Declare();

    public static readonly AppEvent MarkTheNoticeRead = AppEvent.Declare();
}
