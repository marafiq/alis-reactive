using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenMarkupRaisesAPayloadThatHoldsARecord(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X8_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_and_dispatch()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.GateOfAnEscortedVisitor);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'EscortedVisitor.Escort' is a Escort, and markup carries each member of the payload it raises as the text of one "
                + "attribute, so a member it carries is text, a number, a bool, a date, a Guid or an enum. "
                + "Raise a payload that holds a record or a list from a pipeline, with p.Dispatch.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.GateOfAVisitorsType);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ada Hart" }).ClickAsync();
        await Expect(rendered.Page.Locator("#announced-party")).ToHaveTextAsync("3");
    }
}
