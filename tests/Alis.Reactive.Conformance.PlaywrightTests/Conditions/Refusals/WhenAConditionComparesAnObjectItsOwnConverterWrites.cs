using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAnObjectItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_20_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_and_its_converter()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ResidentAtAnAddressItsConverterWrites);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A condition compares 'r.Address', which its own JSON converter writes unlike the app's options, "
                + "and .NET compares the object it holds; compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.ResidentAtAPostcode);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the address", Exact = true }).ClickAsync();
        await Expect(rendered.Page.Locator("#address")).ToHaveTextAsync("the postcode on file");
    }
}
