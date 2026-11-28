using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATemplateConditionComputesTheViewsValue(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X9_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_a_local_to_compute_it_into()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.WardResidentsByAComputedWard);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'Model.Ward.ToUpper()' computes a value inside a template condition; "
                + "compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.WardResidentsByALocalWard);
        await Expect(rendered.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Gridcell)).ToHaveTextAsync(new[] { "Ada", "this ward", "Ruth", "elsewhere" });
    }
}
