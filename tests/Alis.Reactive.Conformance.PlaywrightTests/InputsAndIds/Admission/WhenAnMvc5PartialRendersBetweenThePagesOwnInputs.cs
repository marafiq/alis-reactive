using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5PartialRendersBetweenThePagesOwnInputs(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_11_0)]
    public async Task the_input_after_the_partial_posts_under_the_pages_own_name()
    {
        var onMvc5 = await NewWorld();
        await onMvc5.Page.GotoAsync(CarePages.ContactMoveIn);
        await onMvc5.Page.GetByLabel("Phone", new PageGetByLabelOptions { Exact = true }).FillAsync("0101");
        await onMvc5.Page.GetByLabel("Next of kin's name", new PageGetByLabelOptions { Exact = true }).FillAsync("Ada");
        await onMvc5.Page.GetByLabel("Email", new PageGetByLabelOptions { Exact = true }).FillAsync("a@b.c");
        await onMvc5.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(onMvc5.Page.Locator("#bound")).ToHaveTextAsync("phone 0101, next of kin Ada and email a@b.c");

        var onNet10 = await NewWorldUnder(Hosts.Net10Name);
        await onNet10.Page.GotoAsync(CarePages.ContactMoveIn);
        await onNet10.Page.GetByLabel("Phone", new PageGetByLabelOptions { Exact = true }).FillAsync("0101");
        await onNet10.Page.GetByLabel("Next of kin's name", new PageGetByLabelOptions { Exact = true }).FillAsync("Ada");
        await onNet10.Page.GetByLabel("Email", new PageGetByLabelOptions { Exact = true }).FillAsync("a@b.c");
        await onNet10.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(onNet10.Page.Locator("#bound")).ToHaveTextAsync("phone 0101, next of kin Ada and email a@b.c");
    }
}
