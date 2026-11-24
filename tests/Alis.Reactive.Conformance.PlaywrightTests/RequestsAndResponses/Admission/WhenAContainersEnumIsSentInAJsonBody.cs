using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.EnumNamesAlone))]
[Category(Hosts.EnumNamesAloneName)]
public sealed class WhenAContainersEnumIsSentInAJsonBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_38_0)]
    public async Task the_enum_is_sent_in_the_form_its_endpoint_binds()
    {
        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.ShiftHandover);
        await night.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Quiet night");
        await night.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Shift", Exact = true }).SelectOptionAsync("Night");
        var sent = await night.Page.RunAndWaitForRequestAsync(() => night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync(), "**/shift-handover/handovers");
        Assert.That(sent.PostData, Is.EqualTo("{\"note\":\"Quiet night\",\"shift\":\"night\"}"));
        await Expect(night.Page.Locator("#handed-over")).ToHaveTextAsync("Handed over to the Night shift");

        var day = await NewWorld();
        await day.Page.GotoAsync(CarePages.ShiftHandover);
        await day.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Quiet night");
        await day.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Shift", Exact = true }).SelectOptionAsync("Day");
        var dayShift = await day.Page.RunAndWaitForRequestAsync(() => day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hand over", Exact = true }).ClickAsync(), "**/shift-handover/handovers");
        Assert.That(dayShift.PostData, Is.EqualTo("{\"note\":\"Quiet night\",\"shift\":\"day\"}"));
        await Expect(day.Page.Locator("#handed-over")).ToHaveTextAsync("Handed over to the Day shift");
    }
}
