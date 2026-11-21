using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesAnObjectItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_41_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_its_own_converter()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.SealedDosesComparedByTheirSeal);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'd.Seal == seal' compares 'd.Seal', which its own JSON converter writes unlike the app's options, "
                + "and .NET compares the object it holds; compare it on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.SealedDosesComparedByTheirTag);
        await Expect(rendered.Page.Locator("#count")).ToHaveTextAsync("1");
    }
}
