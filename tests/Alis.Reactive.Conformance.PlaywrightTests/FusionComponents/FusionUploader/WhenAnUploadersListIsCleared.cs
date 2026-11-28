using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnUploadersListIsCleared(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F22.M21_X1039_0)]
    public async Task the_count_of_its_files_follows_the_list_its_clear_button_empties()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.CarePlanDraft);
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care plan", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Uploader", Exact = true })
            .SetInputFilesAsync(new FilePayload { Name = "care-plan.pdf", MimeType = "application/pdf", Buffer = [37, 80, 68, 70] });
        await Expect(cleared.Page.Locator("#chosen")).ToHaveTextAsync("1");
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care plan", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Clear", Exact = true })
            .ClickAsync();

        await Expect(cleared.Page.Locator("#chosen")).ToHaveTextAsync("0");
    }
}
