using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As the night nurse, I want notes loaded into the drawer to hear the desk only while they are in the
/// drawer, so that closing it, or unloading its content, leaves nothing behind that fails later.
/// </summary>
[TestFixture]
public class WhenTheDrawerReleasesWhatItHeld : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer";

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator DrawerContent => Page.Locator("#alis-drawer-content");
    private ILocator NotesStatus => Page.Locator("#drawer-round-status");
    private ILocator RoundCallStatus => Page.Locator("#round-call-status");
    private ILocator OpenNotes => Page.Locator("#btn-open-notes");
    private ILocator CallRound => Page.Locator("#btn-call-round");
    private ILocator UnloadDrawer => Page.Locator("#btn-unload-drawer");
    private ILocator CloseDrawer => Page.Locator("#alis-drawer-close");

    private async Task OpenTheNotes()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await ClickWhenStable(OpenNotes);
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(NotesStatus).ToHaveTextAsync("Waiting for the round.", new() { Timeout = 5000 });
    }

    [Test]
    public async Task the_notes_in_the_drawer_hear_the_desk_call_the_round()
    {
        await OpenTheNotes();

        await ClickWhenStable(CallRound);

        await Expect(NotesStatus).ToHaveTextAsync("The round was called; the notes heard it.");
        await Expect(RoundCallStatus).ToHaveTextAsync("The desk called the round.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_drawer_releases_the_notes_with_their_html()
    {
        await OpenTheNotes();
        await ClickWhenStable(CloseDrawer);
        await Expect(Drawer).Not.ToHaveClassAsync(new Regex("alis-drawer--visible"), new() { Timeout = 5000 });
        await Expect(DrawerContent).ToBeEmptyAsync(new() { Timeout = 5000 });

        await ClickWhenStable(CallRound);

        await Expect(RoundCallStatus).ToHaveTextAsync("The desk called the round.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unloading_the_drawer_content_from_the_plan_releases_the_notes_too()
    {
        await OpenTheNotes();
        await ClickWhenStable(UnloadDrawer);
        await Expect(DrawerContent).ToBeEmptyAsync(new() { Timeout = 5000 });

        await ClickWhenStable(CallRound);

        await Expect(RoundCallStatus).ToHaveTextAsync("The desk called the round.");
        await Expect(Drawer).ToHaveClassAsync(new Regex("alis-drawer--visible"));
        AssertNoConsoleErrors();
    }
}
