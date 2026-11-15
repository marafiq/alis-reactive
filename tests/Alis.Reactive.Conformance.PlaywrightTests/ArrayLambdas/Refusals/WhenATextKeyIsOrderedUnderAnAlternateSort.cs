using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextKeyIsOrderedUnderAnAlternateSort(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_36_0)]
    public async Task the_page_refuses_the_alternate_sort_and_lists_the_carers_under_German()
    {
        var refused = await NewWorld();
        await refused.Seed(Seed.Carer("Müller").AndCarer("Muhr"));
        await refused.Page.GotoAsync(RefusalPages.CarersByGermanPhonebook);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'c.Name' orders text by the alternate sort 'de-DE_phoneb', which the browser has no tag for; "
                + "pass StringComparer.Ordinal, or order on the server.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Seed(Seed.Carer("Müller").AndCarer("Muhr"));
        await rendered.Page.GotoAsync(RefusalPages.CarersByGermanName);
        await Expect(rendered.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Carers", Exact = true })).ToBeVisibleAsync();
        await Expect(rendered.Page.Locator("#carers").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Records", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(["Muhr", "Müller"]);
    }
}
