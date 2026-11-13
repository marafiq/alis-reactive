using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASmartTextAreaStartsAtItsMembersText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F43.M21_X605_0)]
    public async Task the_area_shows_the_text_the_member_holds()
    {
        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.ShiftNoteWritten);

        await Expect(written.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift note", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("Resident slept through the night.");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.ShiftNoteBlank);

        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift note", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");
    }
}
