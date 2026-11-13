using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheContainersElementIsMissing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_10_0)]
    public async Task the_read_stops_until_the_page_holds_the_container()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.NotePreview);
        var preview = record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Preview", Exact = true });

        await preview.ClickAsync();
        await Expect(record.Page.Locator("#preview")).ToHaveTextAsync("-");

        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the note", Exact = true }).ClickAsync();
        await record.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Ate well");
        await preview.ClickAsync();
        await Expect(record.Page.Locator("#preview")).ToHaveTextAsync("Ate well");
    }
}
