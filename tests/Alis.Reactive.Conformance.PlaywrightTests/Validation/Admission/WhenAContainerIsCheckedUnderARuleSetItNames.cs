using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenAContainerIsCheckedUnderARuleSetItNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_48_0)]
    public async Task the_sets_rules_run_so_an_empty_bed_passes_and_emails_that_differ_stop_the_step()
    {
        var matching = await NewWorld();
        await matching.Page.GotoAsync(CarePages.ResidentDetails);
        await matching.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await matching.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("ada@example.org");
        await matching.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Confirm email", Exact = true }).FillAsync("ada@example.org");
        await matching.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();
        await Expect(matching.Page.Locator("#step")).ToHaveTextAsync("Step 2: the bed");
        await Expect(matching.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Identity", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var differing = await NewWorld();
        await differing.Page.GotoAsync(CarePages.ResidentDetails);
        await differing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await differing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Email", Exact = true }).FillAsync("ada@example.org");
        await differing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Confirm email", Exact = true }).FillAsync("ada@example.com");
        await differing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();
        await Expect(differing.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirm email", Exact = true })
            .GetByText("The email addresses must match.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(differing.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Identity", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The email addresses must match." });
        await Expect(differing.Page.Locator("#step")).ToHaveTextAsync("");
    }
}
