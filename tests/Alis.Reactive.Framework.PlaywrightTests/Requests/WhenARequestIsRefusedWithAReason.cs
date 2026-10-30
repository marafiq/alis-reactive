namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>An error route that takes any status reads the error body by its type, whatever status refused it.</summary>
[TestFixture]
public sealed class WhenARequestIsRefusedWithAReason : AppPageTest
{
    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task an_error_route_for_any_status_reads_the_error_body_by_its_type()
    {
        await Open("/Requests/Booking");

        await Click("Book the taken room");
        await Expect(Page.Locator("#refused")).ToHaveTextAsync("room 2 is taken");

        await Click("Book in the closed wing");
        await Expect(Page.Locator("#refused")).ToHaveTextAsync("the east wing is closed");
    }
}
