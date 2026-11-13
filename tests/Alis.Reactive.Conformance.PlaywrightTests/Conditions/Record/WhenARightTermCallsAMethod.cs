using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARightTermCallsAMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_13_0)]
    public async Task the_method_runs_only_when_the_left_term_leaves_the_answer_open()
    {
        var assessed = await NewWorld();
        await assessed.Page.GotoAsync(CarePages.PainReliefCheckedInItsTerms);
        var pain = assessed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Pain", Exact = true });
        var paracetamol = assessed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Give paracetamol", Exact = true });
        var doctor = assessed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the doctor", Exact = true });

        await pain.FillAsync("5");
        await paracetamol.ClickAsync();
        await doctor.ClickAsync();
        await Expect(assessed.Page.Locator("#relief-given")).ToHaveTextAsync("Doctor called");
        await Expect(assessed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("0");

        await pain.FillAsync("1");
        await paracetamol.ClickAsync();
        await Expect(assessed.Page.Locator("#relief-given")).ToHaveTextAsync("Paracetamol given");
        await doctor.ClickAsync();
        await Expect(assessed.Page.Locator("#relief-given")).ToHaveTextAsync("Doctor called");
        await Expect(assessed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("2");

        var checkedFirst = await NewWorld();
        await checkedFirst.Page.GotoAsync(CarePages.PainReliefCheckedBeforeItsWhen);
        await checkedFirst.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Pain", Exact = true }).FillAsync("5");
        await checkedFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Give paracetamol", Exact = true }).ClickAsync();
        await Expect(checkedFirst.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("1");
        await checkedFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the doctor", Exact = true }).ClickAsync();
        await Expect(checkedFirst.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Calls", Exact = true })).ToHaveTextAsync("2");
    }
}
