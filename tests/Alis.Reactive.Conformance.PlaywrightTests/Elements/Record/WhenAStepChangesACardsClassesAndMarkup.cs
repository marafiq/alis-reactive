using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepChangesACardsClassesAndMarkup(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X7_8_0)]
    public async Task a_class_is_added_removed_and_toggled_and_a_name_or_markup_that_reads_nothing_adds_nothing()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.RecordCard);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Flag", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card flagged");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear flag", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Toggle flag", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card flagged");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Toggle flag", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark the round", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#mark-status")).ToHaveTextAsync("Round checked");
        await Expect(record.Page.Locator("#record-card")).ToHaveClassAsync("card");
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#record-summary")).ToHaveTextAsync("Stable overnight");

        var unsummarised = await NewWorld();
        await unsummarised.Page.GotoAsync(CarePages.RecordCardWithNoSummary);
        await unsummarised.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load summary", Exact = true }).ClickAsync();
        await Expect(unsummarised.Page.Locator("#record-summary")).ToHaveTextAsync("");
    }
}
