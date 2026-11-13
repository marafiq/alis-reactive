using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADictionaryIsKeyedByAClass(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X1_4_0)]
    public async Task the_view_is_refused_where_it_renders_and_told_to_key_it_by_text_or_a_number()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.Index);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the occupancy view", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A dictionary keyed by RoomKey cannot be sent: System.Text.Json writes no property name for its keys; "
                + "key it by text or a number.",
            Exact = true,
        })).ToBeVisibleAsync();

        var saved = await NewWorld();
        await saved.Page.GotoAsync(RefusalPages.Index);
        await saved.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the occupancy by room code", Exact = true }).ClickAsync();
        await saved.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Occupied", Exact = true }).FillAsync("2");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(saved.Page.Locator("#beds-saved")).ToHaveTextAsync("E4: 2");
    }
}
