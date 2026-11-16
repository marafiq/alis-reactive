using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5RequestRendersWithNoResolverSet(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X10_28_1)]
    public async Task the_page_is_refused_where_it_renders_naming_the_startup_call_and_the_resolver()
    {
        var noResolver = await NewWorld();
        await noResolver.Page.GotoAsync(RefusalPages.RoundsBoard);
        await Expect(noResolver.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'GET /Rounds' is rendered in an app that never called AddReactiveRequests or set MVC's resolver; "
                + "call services.AddReactiveRequests(requests => ..) once at startup and set the resolver over the container with DependencyResolver.SetResolver in Application_Start.",
            Exact = true,
        })).ToBeVisibleAsync();
    }
}
