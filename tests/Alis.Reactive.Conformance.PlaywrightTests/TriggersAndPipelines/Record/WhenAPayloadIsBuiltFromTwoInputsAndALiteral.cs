using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPayloadIsBuiltFromTwoInputsAndALiteral(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X3_0)]
    public async Task its_handler_reads_each_input_as_it_stands_at_the_click_the_ward_inside_its_destination_and_the_literal()
    {
        var transfer = await NewWorld();
        await transfer.Page.GotoAsync(CarePages.WardTransfer);
        await transfer.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await transfer.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Destination ward", Exact = true }).FillAsync("East");
        await transfer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Transfer", Exact = true }).ClickAsync();
        await Expect(transfer.Page.Locator("#transfer-name")).ToHaveTextAsync("Ada");
        await Expect(transfer.Page.Locator("#transfer-ward")).ToHaveTextAsync("East");
        await Expect(transfer.Page.Locator("#transfer-reason")).ToHaveTextAsync("fall");
        await transfer.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Destination ward", Exact = true }).FillAsync("West");
        await transfer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Transfer", Exact = true }).ClickAsync();
        await Expect(transfer.Page.Locator("#transfer-ward")).ToHaveTextAsync("West");
        await Expect(transfer.Page.Locator("#transfer-name")).ToHaveTextAsync("Ada");
        await Expect(transfer.Page.Locator("#transfer-reason")).ToHaveTextAsync("fall");

        var wardless = await NewWorld();
        await wardless.Page.GotoAsync(CarePages.WardTransfer);
        await wardless.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await wardless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Transfer", Exact = true }).ClickAsync();
        await Expect(wardless.Page.Locator("#transfer-name")).ToHaveTextAsync("Ada");
        await Expect(wardless.Page.Locator("#transfer-ward")).ToHaveTextAsync("");
    }
}
