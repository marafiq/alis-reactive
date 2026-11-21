using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAValueTestReadsARecordThatMayBeMissing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.M3_X1_0)]
    public async Task a_null_name_is_null_a_name_past_a_null_record_is_neither_null_nor_named_and_both_are_empty()
    {
        var gate = await NewWorld();
        await gate.Page.GotoAsync(CarePages.RecordNameGate);
        await gate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Unnamed record", Exact = true }).ClickAsync();
        await Expect(gate.Page.Locator("#gate")).ToHaveTextAsync("No name");
        await Expect(gate.Page.Locator("#badge")).ToHaveTextAsync("Blank");
        await gate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No record", Exact = true }).ClickAsync();
        await Expect(gate.Page.Locator("#gate")).ToHaveTextAsync("Unknown");
        await Expect(gate.Page.Locator("#badge")).ToHaveTextAsync("Blank");
        await gate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        await Expect(gate.Page.Locator("#gate")).ToHaveTextAsync("Named");
        await Expect(gate.Page.Locator("#badge")).ToHaveTextAsync("Filled");

        var nameless = await NewWorld();
        await nameless.Page.GotoAsync(CarePages.RecordNameGate);
        await nameless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Nameless record", Exact = true }).ClickAsync();
        await Expect(nameless.Page.Locator("#gate")).ToHaveTextAsync("No name");
    }
}
