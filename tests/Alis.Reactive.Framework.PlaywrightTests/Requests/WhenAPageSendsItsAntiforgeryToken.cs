namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A request to an endpoint that checks the antiforgery token (<c>[ValidateAntiForgeryToken]</c>), sent with the
/// token the page was given (<c>IAntiforgery</c> in the view) as the header ASP.NET Core reads by default,
/// <c>g.Header("RequestVerificationToken", token)</c>: accepted with it, refused without it.
/// </summary>
[TestFixture]
public sealed class WhenAPageSendsItsAntiforgeryToken : AppPageTest
{
    private ILocator HandedOver => Page.Locator("#handed-over");

    private Task HandOver() => Page.GetByRole(AriaRole.Button, new() { Name = "Hand over the shift", Exact = true }).ClickAsync();

    [Test]
    public async Task a_request_carrying_the_pages_token_in_the_header_aspnet_core_reads_is_accepted()
    {
        await Open("/Requests/Handover?token=Sent");

        await HandOver();

        await Expect(HandedOver).ToHaveTextAsync("'Room 4 needs a check at two', accepted");
    }

    [Test]
    public async Task a_request_without_the_token_is_refused_as_a_bad_request()
    {
        await Open("/Requests/Handover?token=LeftOut");

        await HandOver();

        await Expect(HandedOver).ToHaveTextAsync("refused as a bad request");
    }
}
