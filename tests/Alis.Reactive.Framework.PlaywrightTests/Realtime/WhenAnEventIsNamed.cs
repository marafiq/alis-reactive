namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A declared document event reads as the name it is raised under, with or without a payload: its member's
/// name camel-cased, or the name given. An event declared with a blank name, or by no member, is refused when
/// the page renders, with the message the developer reads.
/// </summary>
[TestFixture]
public sealed class WhenAnEventIsNamed : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task an_event_reads_as_the_name_it_is_raised_under()
    {
        await Open("/Realtime/Names");

        await Expect(Page.Locator("#hung-up")).ToHaveTextAsync("hungUp");
    }

    [Test]
    public async Task an_event_with_a_payload_reads_as_the_name_it_is_raised_under()
    {
        await Open("/Realtime/Names");

        await Expect(Page.Locator("#welcomed")).ToHaveTextAsync("welcomed");
    }

    [Test]
    public async Task an_event_declared_with_a_name_reads_as_that_name()
    {
        await Open("/Realtime/Names");

        await Expect(Page.Locator("#answered")).ToHaveTextAsync("line:answered");
    }

    [Test]
    public async Task an_event_declared_with_a_blank_name_is_refused()
    {
        await Render("/Realtime/BlankEventName");

        await Expect(Page.GetByText("An event needs a name.").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_event_declared_by_no_member_is_refused()
    {
        await Render("/Realtime/EventNamedByNoMember");

        await Expect(Page.GetByText("Declare an event or a hub method as a member of a class").First).ToBeVisibleAsync();
    }
}
