using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonSaveSendsADeputyWhoseCodeTheContractLeavesOut(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X18_10_0)]
    public async Task the_code_reads_as_the_endpoint_binds_a_new_deputy_so_the_save_is_held()
    {
        var phone = await NewWorld();
        await phone.Page.GotoAsync(CarePages.DeputyCode);
        await phone.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("555-0101");
        await phone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(phone.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Code the deputy A1." });
        await Expect(phone.Page.Locator("#saved")).ToBeEmptyAsync();

        var note = await NewWorld();
        await note.Page.GotoAsync(CarePages.DeputyCode);
        await note.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Note", Exact = true }).FillAsync("Quiet");
        await note.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save note", Exact = true }).ClickAsync();
        await Expect(note.Page.Locator("#saved")).ToHaveTextAsync("Quiet, A1");
        await Expect(note.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
