namespace Alis.Reactive.PlaywrightTests.HttpPipeline.FamilyCall;

/// <summary>
/// As a night nurse calling a resident's family after a fall, I want the family portal to tell me who the
/// emergency contact is and the number to call, so that I reach the right person at once, at any hour.
/// </summary>
/// <remarks>
/// The family portal answers with the resident, and within the resident the emergency contact: Walter Cho,
/// room 118, whose daughter Grace Cho is reached at (617) 555-0142. The contact's details sit two levels
/// down in the answer and the resident's one level down, and the two are named differently, so a read of
/// the wrong level shows the wrong name. The portal has no error answer here: a portal that does not answer
/// is not part of this journey. Everything the page renders is exercised.
/// </remarks>
[TestFixture]
public class WhenNightNurseCallsTheFamily : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/FamilyCall";
    private const string PortalUrl = "**/Sandbox/HttpPipeline/FamilyCall/EmergencyContact";

    private ILocator LookUpTheContact => Page.Locator("#look-up-contact-btn");
    private ILocator Asking => Page.Locator("#contact-loading");
    private ILocator ContactCard => Page.Locator("#contact-card");
    private ILocator ContactName => Page.Locator("#contact-name");
    private ILocator Relationship => Page.Locator("#contact-relationship");
    private ILocator Resident => Page.Locator("#contact-resident");
    private ILocator Room => Page.Locator("#contact-room");
    private ILocator Phone => Page.Locator("#contact-phone");

    // The request is held at the browser until the test releases it, then sent on unchanged, so what the
    // page shows before the answer arrives can be seen.
    private async Task<SemaphoreSlim> HoldUntilReleased(string url)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            await released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        return released;
    }

    [Test]
    public async Task looking_up_the_emergency_contact_shows_whom_to_call_and_the_number()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(ContactCard).ToBeHiddenAsync();

        await ClickWhenStable(LookUpTheContact);

        await Expect(ContactCard).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ContactName).ToHaveTextAsync("Grace Cho");
        await Expect(Relationship).ToHaveTextAsync("daughter");
        await Expect(Resident).ToHaveTextAsync("Walter Cho");
        await Expect(Room).ToHaveTextAsync("118");
        await Expect(Phone).ToHaveTextAsync("(617) 555-0142");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_says_it_is_asking_the_family_portal_until_the_contact_arrives()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Asking).ToBeHiddenAsync();
        var portalAnswers = await HoldUntilReleased(PortalUrl);

        await ClickWhenStable(LookUpTheContact);

        await Expect(Asking).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ContactCard).ToBeHiddenAsync();
        portalAnswers.Release();
        await Expect(ContactName).ToHaveTextAsync("Grace Cho", new() { Timeout = 5000 });
        await Expect(ContactCard).ToBeVisibleAsync();
        await Expect(Asking).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
