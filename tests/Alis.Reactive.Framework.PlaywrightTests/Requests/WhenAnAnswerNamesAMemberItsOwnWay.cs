namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenAnAnswerNamesAMemberItsOwnWay : AppPageTest
{
    [Test]
    public async Task the_member_is_read_by_the_name_the_server_writes()
    {
        await Open("/Requests/Booking");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Book the open room", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#booked-room")).ToHaveTextAsync("4");
    }
}
