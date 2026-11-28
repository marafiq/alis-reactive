using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.EnumNamesAlone))]
[Category(Hosts.EnumNamesAloneName)]
public sealed class WhenEditedRowsHoldAnEnumTheEndpointReadsByNameAlone(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F23.M21_X210_0)]
    public async Task the_batch_page_is_refused_where_it_renders_naming_the_enum_and_the_form_it_is_held_in()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ChargeBatch);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Changed' holds 'status', whose enum a live object holds as 0 for Due, a form its endpoint does not read as Due; "
                + "let the endpoint read the form the live object holds, or send the rows from JSON the server writes.",
            Exact = true,
        })).ToBeVisibleAsync();

        var readingNumbers = await NewWorldUnder(Hosts.Net10Name);
        await readingNumbers.Page.GotoAsync(RefusalPages.ChargeBatch);
        await readingNumbers.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the batch", Exact = true }).ClickAsync();
        await Expect(readingNumbers.Page.Locator("#saved")).ToHaveTextAsync("No charges changed");
    }
}
