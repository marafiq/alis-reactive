namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the developer of the wing's waiting list, I want a row button that fills an event's payload from a member
/// that may be missing refused as the view renders, naming the fix, because the template engine writes a missing
/// bed as the text "null" and the front desk would be asked to keep bed "null".
/// </summary>
[TestFixture]
public class WhenARowButtonFillsAMemberThatMayBeMissing : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/CareReviewWaitingList";

    [Test]
    public async Task the_page_does_not_render_and_names_the_member_and_the_fix()
    {
        var response = await Page.GotoAsync($"{BaseUrl}{Path}", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(500), "A row button filled from a member that may be missing does not render.");
        await Expect(Page.Locator("body")).ToContainTextAsync(
            "Template button 'Keep the bed' fills 'WingReviewBedHold.Bed' of type Int32?, and the template engine writes a " +
            "missing value as the text \"null\", which the listener would read as that text. Fill it from a member of " +
            "type Int32, which always holds a value.");
    }
}
