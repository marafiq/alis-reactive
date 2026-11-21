using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextBodyNamesItsCharset(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.M6_X2_0)]
    public async Task the_text_is_decoded_by_the_charset_its_content_type_names()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.DietNoteInWindows1252);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load note", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#diet-note")).ToHaveTextAsync("café");

        var utf8 = await NewWorld();
        await utf8.Page.GotoAsync(CarePages.DietNoteInUtf8);
        await utf8.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load note", Exact = true }).ClickAsync();
        await Expect(utf8.Page.Locator("#diet-note")).ToHaveTextAsync("café");
    }
}
