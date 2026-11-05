using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.MedicationOrder;

[TestFixture]
public class WhenNurseSavesMedicationOrder : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/MedicationOrder";

    private ILocator Field(Expression<Func<MedicationOrderModel, object?>> member)
        => Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator SaveOrder => Page.Locator("#save-medication-order-btn");
    private ILocator OrderStatus => Page.Locator("#medication-order-status");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    [Test]
    public async Task saving_the_order_sends_every_medication_line_to_the_pharmacy()
    {
        await NavigateAndBoot();
        await Field(m => m.ResidentName).FillAsync("Helen Park");
        await Field(m => m.Lines[0].Sku).FillAsync("RX-100");
        await Field(m => m.Lines[0].Quantity).FillAsync("2");
        await Field(m => m.Lines[1].Sku).FillAsync("RX-200");
        await Field(m => m.Lines[1].Quantity).FillAsync("1");

        await ClickWhenStable(SaveOrder);

        await Expect(OrderStatus).ToHaveTextAsync("Order saved for Helen Park: RX-100 × 2, RX-200 × 1.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_order_with_the_second_line_left_empty_confirms_only_the_first_medication()
    {
        await NavigateAndBoot();
        await Field(m => m.ResidentName).FillAsync("Helen Park");
        await Field(m => m.Lines[0].Sku).FillAsync("RX-100");
        await Field(m => m.Lines[0].Quantity).FillAsync("2");

        await ClickWhenStable(SaveOrder);

        await Expect(OrderStatus).ToHaveTextAsync("Order saved for Helen Park: RX-100 × 2.");
        AssertNoConsoleErrors();
    }
}
