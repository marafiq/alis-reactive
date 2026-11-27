using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffKeepPhotoWorksheets(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F10.M21_X1312_0)]
    public async Task closing_the_signed_preview_releases_the_enhanced_checkbox()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.PhotoWorksheetPreview);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close signed preview", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
        await Expect(closed.Page.GetByText("Signed photo release", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var retained = await NewWorld();
        await retained.Page.GotoAsync(CarePages.PhotoWorksheetPreview);
        await retained.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep signed preview", Exact = true }).ClickAsync();
        await Expect(retained.Page.GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync();
        await Expect(retained.Page.GetByText("Signed photo release", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F10.M21_X1313_0)]
    public async Task a_marked_pending_worksheet_returns_only_when_this_workstation_remembers_it()
    {
        var remembered = await NewWorld();
        await remembered.Page.GotoAsync(CarePages.PhotoWorksheet);
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remember on this workstation", Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark for confirmation", Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage choice", Exact = true }).ClickAsync();
        await Expect(remembered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Worksheet storage", Exact = true })).ToHaveTextAsync("Remember on this workstation");
        await remembered.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Reopen worksheet", Exact = true }).ClickAsync();
        await remembered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review worksheet", Exact = true }).ClickAsync();
        await Expect(remembered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Worksheet selection", Exact = true })).ToHaveTextAsync("Marked release awaiting confirmation");

        var visit = await NewWorld();
        await visit.Page.GotoAsync(CarePages.PhotoWorksheet);
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use this visit only", Exact = true }).ClickAsync();
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark for confirmation", Exact = true }).ClickAsync();
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review storage choice", Exact = true }).ClickAsync();
        await Expect(visit.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Worksheet storage", Exact = true })).ToHaveTextAsync("This visit only");
        await visit.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Reopen worksheet", Exact = true }).ClickAsync();
        await visit.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review worksheet", Exact = true }).ClickAsync();
        await Expect(visit.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Worksheet selection", Exact = true })).ToHaveTextAsync("Unmarked release");
    }
}
