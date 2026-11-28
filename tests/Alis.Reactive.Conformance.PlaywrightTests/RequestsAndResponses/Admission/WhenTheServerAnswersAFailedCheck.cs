using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheServerAnswersAFailedCheck(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_5_0)]
    public async Task each_message_shows_beside_the_input_its_key_names_and_in_the_summary()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.AdmitResident);
        await refused.Page.GetByLabel("Name", new PageGetByLabelOptions { Exact = true }).FillAsync("Ada Lee");
        await refused.Page.GetByLabel("Room number", new PageGetByLabelOptions { Exact = true }).FillAsync("12");
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByText("Name is taken", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Room", Exact = true })
            .GetByText("Room is full", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name is taken", "Room is full" });
        await Expect(refused.Page.Locator("#admission-status")).ToHaveTextAsync("Not admitted");

        var admitted = await NewWorld();
        await admitted.Page.GotoAsync(CarePages.AdmitResident);
        await admitted.Page.GetByLabel("Name", new PageGetByLabelOptions { Exact = true }).FillAsync("Grace Hopper");
        await admitted.Page.GetByLabel("Room number", new PageGetByLabelOptions { Exact = true }).FillAsync("14");
        await admitted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(admitted.Page.Locator("#admission-status")).ToHaveTextAsync("Admitted");
        await Expect(admitted.Page.GetByText("Name is taken", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(admitted.Page.GetByText("Room is full", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
