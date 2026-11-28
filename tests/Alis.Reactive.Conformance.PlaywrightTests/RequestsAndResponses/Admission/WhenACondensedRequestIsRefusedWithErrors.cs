using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACondensedRequestIsRefusedWithErrors(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_12_0)]
    public async Task the_defaults_show_each_error_at_the_requests_own_form()
    {
        var shown = await NewWorld();
        await shown.Page.GotoAsync(CarePages.AdmissionDesk);
        await shown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(shown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(shown.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Admission", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the resident." });

        var withoutShowValidationErrors = await NewWorldUnder(Hosts.NoShowValidationErrorsName);
        await withoutShowValidationErrors.Page.GotoAsync(CarePages.AdmissionDesk);
        await withoutShowValidationErrors.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(withoutShowValidationErrors.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(withoutShowValidationErrors.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
