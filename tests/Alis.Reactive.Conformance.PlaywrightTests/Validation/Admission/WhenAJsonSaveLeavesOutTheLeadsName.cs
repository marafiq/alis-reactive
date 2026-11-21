using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonSaveLeavesOutTheLeadsName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X16_2_0)]
    public async Task the_lead_name_reads_as_the_endpoint_binds_a_new_lead_so_the_save_is_held()
    {
        var phoneAlone = await NewWorld();
        await phoneAlone.Page.GotoAsync(CarePages.HomeLead);
        await phoneAlone.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555-0100");
        await phoneAlone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(phoneAlone.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Lead name is required." });
        await Expect(phoneAlone.Page.Locator("#saved")).ToBeEmptyAsync();

        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.HomeLead);
        await both.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Lead name", Exact = true }).FillAsync("Ruth");
        await both.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555-0100");
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save all", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#saved")).ToHaveTextAsync("Ruth, 555-0100");
        await Expect(both.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
