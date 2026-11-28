using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5ViewReadsTheAppsRegistrations(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X4_0)]
    public async Task the_page_sends_through_the_requests_the_app_registered_behind_mvcs_resolver()
    {
        var registered = await NewWorld();
        await registered.Page.GotoAsync(CarePages.NameIntake);
        await Expect(registered.Page.Locator("#intake-status")).ToHaveTextAsync("Not saved");
        await registered.Page.GetByLabel("Name", new PageGetByLabelOptions { Exact = true }).FillAsync("Ada Lee");
        await registered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(registered.Page.Locator("#intake-status")).ToHaveTextAsync("Saved Ada Lee");

        var noResolver = await NewWorld();
        await noResolver.Seed(Seed.TheResolverLeftUnset());
        await noResolver.Page.GotoAsync(CarePages.NameIntake);
        await Expect(noResolver.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'POST " + CareEndpoints.NameIntakeNames + "' is rendered in an app that never called AddReactiveRequests or set MVC's resolver; "
                + "call services.AddReactiveRequests(requests => ..) once at startup and set the resolver over the container with DependencyResolver.SetResolver in Application_Start.",
            Exact = true,
        })).ToBeVisibleAsync();
    }
}
