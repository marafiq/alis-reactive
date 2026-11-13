using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextAreaIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F48.M21_X1029_0)]
    public async Task the_area_starts_at_the_text_the_member_holds()
    {
        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.ProfileNoteWritten);

        await Expect(written.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care note", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("Prefers tea at seven");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.ProfileNoteBlank);

        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care note", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");
    }
}
