using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestMembersEnumWritesTwoValuesInOneForm(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_20_0)]
    public async Task the_page_is_refused_where_it_renders_naming_the_member_and_the_two_values_in_one_form()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.WardBedsOfTwoWardsInOneForm);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'r => r.Ward' holds EastWing and EastAnnex in one form, \"east\", under the app's options, so the browser cannot tell "
                + "which one it holds; give one of them a JsonStringEnumMemberName of its own.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.WardBedsOfWardsInTheirOwnForms);
        await rendered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East Annex");
        var sent = await rendered.Page.RunAndWaitForRequestAsync(() => rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find beds", Exact = true }).ClickAsync(), "**/ward-beds**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/refusals/requests-and-responses/ward-beds?Ward=EastAnnex"));
    }
}
