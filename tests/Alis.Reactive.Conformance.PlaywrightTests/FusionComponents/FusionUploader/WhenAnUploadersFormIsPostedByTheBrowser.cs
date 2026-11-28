using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnUploadersFormIsPostedByTheBrowser(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F22.M21_X1040_0)]
    public async Task the_chosen_file_binds_under_the_members_name()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.CarePlanFiling);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care plan", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Uploader", Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "care-plan.pdf", MimeType = "application/pdf", Buffer = [37, 80, 68, 70] });
        await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#filed")).ToHaveTextAsync("Care plan filed: care-plan.pdf");

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.CarePlanFiling);
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "File", Exact = true }).ClickAsync();

        await Expect(none.Page.Locator("#filed")).ToHaveTextAsync("No care plan filed");
    }
}
