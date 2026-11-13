using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnUploadersFileIsSentWithItsForm(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F22.M21_X1014_0)]
    public async Task the_chosen_file_leaves_with_the_form_and_not_when_it_is_chosen()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.CarePlanUpload);
        var first = await chosen.Page.RunAndWaitForRequestAsync(async () =>
        {
            await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care plan", Exact = true })
                .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Uploader", Exact = true })
                .SetInputFilesAsync(new FilePayload { Name = "care-plan.pdf", MimeType = "application/pdf", Buffer = [37, 80, 68, 70] });
            await Expect(chosen.Page.Locator("#chosen")).ToHaveTextAsync("1");
            await chosen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        }, request => request.Method == "POST");

        Assert.That(first.Url, Does.EndWith("/care/admission-clinical/care-plan-upload/care-plans"));
        await Expect(chosen.Page.Locator("#saved")).ToHaveTextAsync("care-plan.pdf");

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.CarePlanUpload);
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();

        await Expect(none.Page.Locator("#saved")).ToHaveTextAsync("no file");
    }
}
