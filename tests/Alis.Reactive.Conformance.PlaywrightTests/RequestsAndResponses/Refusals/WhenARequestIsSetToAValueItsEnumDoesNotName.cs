using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestIsSetToAValueItsEnumDoesNotName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_22_0)]
    public async Task the_page_is_refused_where_it_renders_naming_the_member_and_the_value_no_name_holds()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.ShiftNoValueNames);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Shift' is set to 5, which no value of its enum names: net10 refuses it and MVC 5 binds it, "
                + "so the hosts bind it apart; set a value its enum names.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        var sent = await rendered.Page.RunAndWaitForRequestAsync(() => rendered.Page.GotoAsync(RefusalPages.ShiftAValueNames), "**/rota**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/refusals/requests-and-responses/rota?Shift=Night"));
    }
}
