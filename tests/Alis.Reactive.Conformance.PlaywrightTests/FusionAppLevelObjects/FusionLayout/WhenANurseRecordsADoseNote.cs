using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.FusionShell))]
[Category(Hosts.FusionShellName)]
public sealed class WhenANurseRecordsADoseNote(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M23_F1.M23_X1_0)]
    public async Task the_toasts_show_the_notes_as_text_and_the_ward_board_stays_put()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.DoseLogWithFusionToast);
        var note = world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true });
        var record = world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record", Exact = true });
        var toasts = world.Page.GetByRole(AriaRole.Alert);
        var main = world.Page.GetByRole(AriaRole.Main);

        await note.FillAsync("#ward-board");
        await record.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["#ward-board"]);
        await Expect(main.GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Ward board", Exact = true })).ToBeVisibleAsync();

        await note.FillAsync("<b>Ada</b> 5 mg");
        await record.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["<b>Ada</b> 5 mg", "#ward-board"]);

        await note.FillAsync("b[title='${window.location.protocol}']");
        await record.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["b[title='${window.location.protocol}']", "<b>Ada</b> 5 mg", "#ward-board"]);

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.DoseLogWithFusionToast);
        await plain.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Ada 5 mg");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Alert)).ToHaveTextAsync(["Ada 5 mg"]);
        await Expect(plain.Page.GetByRole(AriaRole.Main).GetByRole(AriaRole.Region, new LocatorGetByRoleOptions { Name = "Ward board", Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M23_F1.M23_X4_0)]
    public async Task dismissing_the_oldest_notice_leaves_the_newer_one_visible()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.DoseLogWithFusionToast);
        var note = world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true });
        var record = world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record", Exact = true });
        var dismiss = world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Dismiss oldest notice", Exact = true });
        var toasts = world.Page.GetByRole(AriaRole.Alert);

        await note.FillAsync("Morning dose recorded");
        await record.ClickAsync();
        await note.FillAsync("Evening dose recorded");
        await record.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["Evening dose recorded", "Morning dose recorded"]);

        await dismiss.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["Evening dose recorded"]);

        await dismiss.ClickAsync();
        await Expect(toasts).ToHaveCountAsync(0);
        await note.FillAsync("Night dose recorded");
        await record.ClickAsync();
        await Expect(toasts).ToHaveTextAsync(["Night dose recorded"]);
    }
}
