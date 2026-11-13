using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoFormsOfOnePageAreRefused(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X13_5_0)]
    public async Task each_form_keeps_its_own_summary_which_outlives_the_line_that_left()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.IntakeAndKin);
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the line", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true })).ToBeVisibleAsync();
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save intake", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Intake", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Choose a bed." });
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the line", Exact = true }).ClickAsync();
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save next of kin", Exact = true }).ClickAsync();
        await Expect(both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give the relationship." });
        await Expect(both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Intake", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Choose a bed." });

        var kinAlone = await NewWorld();
        await kinAlone.Page.GotoAsync(CarePages.IntakeAndKin);
        await kinAlone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save next of kin", Exact = true }).ClickAsync();
        await Expect(kinAlone.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give the relationship." });
        await Expect(kinAlone.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Intake", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
