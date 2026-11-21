using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANoticeHoldsMarkup(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F5.M25_X8_0)]
    public async Task the_toast_shows_the_note_as_typed()
    {
        var marked = await NewWorld();
        await marked.Page.GotoAsync(CarePages.WetBedNote);
        await marked.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("<b>Bed 4</b> wet");
        await marked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Report", Exact = true }).ClickAsync();
        await Expect(marked.Page.GetByRole(AriaRole.Alert).GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("<b>Bed 4</b> wet");

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.WetBedNote);
        await plain.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Bed 4 wet");
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Report", Exact = true }).ClickAsync();
        await Expect(plain.Page.GetByRole(AriaRole.Alert).GetByRole(AriaRole.Paragraph)).ToHaveTextAsync("Bed 4 wet");
    }
}
