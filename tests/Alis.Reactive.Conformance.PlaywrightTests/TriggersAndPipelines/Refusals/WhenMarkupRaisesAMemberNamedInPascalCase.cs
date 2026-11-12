using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenMarkupRaisesAMemberNamedInPascalCase(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X9_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_member_its_name_and_dispatch()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.SignedVisitNamedInPascalCase);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'PascalNamedSignature.Visitor' is named 'VisitorName', and markup carries each member of the payload it raises as an "
                + "attribute whose name the browser lowercases, so a name it carries is camel case: a lowercase first letter, then letters, digits or underscores. "
                + "Rename it, or raise the payload from a pipeline, with p.Dispatch.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.SignedVisitNamedInCamelCase);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign the visit" }).ClickAsync();
        await Expect(rendered.Page.Locator("#signed-by")).ToHaveTextAsync("Ada Hart");
    }
}
