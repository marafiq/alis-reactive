using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAWholeObjectsMemberIsNamedPastItsCaseOnMvc5(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_48_0)]
    public async Task the_page_is_refused_where_the_view_renders_and_a_name_that_differs_by_case_alone_is_bound()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.KinContactsNamedPastTheirCase);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Daughter' holds 'given', which MVC 5 binds by its C# name GivenName, "
                + "and a JSON body sends an object's members by their JSON names; give it a JSON name that differs from GivenName by case alone.",
            Exact = true,
        })).ToBeVisibleAsync();

        var bound = await NewWorld();
        await bound.Page.GotoAsync(RefusalPages.KinContactsNamedByTheirCase);
        await bound.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save the next of kin", Exact = true }).ClickAsync();
        await Expect(bound.Page.Locator("#saved")).ToHaveTextAsync("Ada");
    }
}
