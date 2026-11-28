using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextAreaRendersItsMembersValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F8.M24_X9_0)]
    public async Task the_area_shows_the_note_its_member_holds()
    {
        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.CareNoteRecordWithANote);
        await Expect(noted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Care notes", Exact = true })).ToHaveValueAsync("Prefers tea at seven");

        var unnoted = await NewWorld();
        await unnoted.Page.GotoAsync(CarePages.CareNoteRecordWithNoNote);
        await Expect(unnoted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Care notes", Exact = true })).ToHaveValueAsync("");
    }
}
