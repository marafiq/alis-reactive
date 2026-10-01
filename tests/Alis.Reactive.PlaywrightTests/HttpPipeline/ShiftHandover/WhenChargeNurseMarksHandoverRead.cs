namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftHandover;

/// <summary>
/// As the night charge nurse, I want marking a handover note read from its link to record my shift and
/// to give me the page back afterwards, so that I can move straight on to the next note.
/// </summary>
/// <remarks>
/// Each "Mark Read" link sends its request with an X-Shift header, covers the page with the loader while
/// it is sent, and lifts the loader once the request ends, whatever the answer and even with none. Arthur
/// Lee's note was already marked read by the day shift, so the server refuses it.
/// </remarks>
[TestFixture]
public class WhenChargeNurseMarksHandoverRead : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ShiftHandover";
    private const string MarkReadUrl = "**/Sandbox/HttpPipeline/ShiftHandover/MarkRead/*";

    private SemaphoreSlim _markReadReleased = null!;

    private ILocator MarkRead(string resident) => Page.GetByTestId($"mark-read-{resident}");
    private ILocator HandoverStatus => Page.Locator("#handover-status");
    private ILocator Loader => Page.Locator("#alis-loader");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    // The request is held at the browser until the test releases it, then sent on unchanged, so the
    // loader can be seen covering the page before any answer arrives.
    private async Task HoldMarkReadUntilReleased()
    {
        _markReadReleased = new SemaphoreSlim(0);
        await Page.RouteAsync(MarkReadUrl, async route =>
        {
            await _markReadReleased.WaitAsync();
            await route.ContinueAsync();
        });
    }

    [Test]
    public async Task marking_a_note_read_tells_the_server_which_shift_read_it()
    {
        await NavigateAndBoot();

        await ClickWhenStable(MarkRead("helen"));

        await Expect(HandoverStatus).ToHaveTextAsync("Helen Park's handover note is marked read by the night shift.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_loader_lifts_when_the_note_cannot_reach_the_server()
    {
        await Page.RouteAsync(MarkReadUrl, route => route.AbortAsync("connectionrefused"));
        await NavigateAndBoot();

        await ClickWhenStable(MarkRead("helen"));

        await Expect(Loader).ToBeAttachedAsync();
        await Expect(Loader).ToBeHiddenAsync();
        await Expect(HandoverStatus).ToHaveTextAsync("No note marked read yet.");
        AssertNoConsoleErrorsExcept("net::ERR_CONNECTION_REFUSED", "[alis:http] fetch.network-error");
    }

    [Test]
    public async Task a_note_already_marked_read_is_refused_and_the_loader_still_lifts()
    {
        await HoldMarkReadUntilReleased();
        await NavigateAndBoot();

        await ClickWhenStable(MarkRead("arthur"));

        await Expect(Loader).ToBeVisibleAsync();
        _markReadReleased.Release();
        await Expect(Loader).ToBeAttachedAsync();
        await Expect(Loader).ToBeHiddenAsync();
        await Expect(HandoverStatus).ToHaveTextAsync("Arthur Lee's note was already marked read by the day shift.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }
}
