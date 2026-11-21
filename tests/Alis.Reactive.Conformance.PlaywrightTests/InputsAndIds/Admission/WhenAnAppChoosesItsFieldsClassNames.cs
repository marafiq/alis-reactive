using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.ClassNames))]
[Category(Hosts.ClassNamesName)]
public sealed class WhenAnAppChoosesItsFieldsClassNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X7_0)]
    public async Task an_invalid_field_takes_the_apps_classes_until_it_is_corrected()
    {
        var styled = await NewWorld();
        await styled.Page.GotoAsync(CarePages.ResidentIntake);
        await styled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(styled.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToHaveClassAsync("invalid-feedback");
        await Expect(styled.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true })).ToHaveClassAsync("is-invalid");
        await styled.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await styled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(styled.Page.Locator("#intake-status")).ToHaveTextAsync("Saved Ada");
        await Expect(styled.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true })).ToHaveClassAsync("");
        await Expect(styled.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var withTheFrameworksNames = await NewWorldUnder(Hosts.Net10Name);
        await withTheFrameworksNames.Page.GotoAsync(CarePages.ResidentIntake);
        await withTheFrameworksNames.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(withTheFrameworksNames.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToHaveClassAsync("alis-field__message");
        await Expect(withTheFrameworksNames.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true })).ToHaveClassAsync("alis-has-error");
    }
}
