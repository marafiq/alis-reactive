using Alis.Reactive.Builders.Conditions;
using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

/// <summary>
/// Stub fusion-vendor component for testing.
/// Implements IComponent + IInputComponent from core — mimics the
/// FusionNumericTextBox pattern without requiring the Fusion project reference.
/// </summary>
public sealed class StubFusionNumericTextBox : IComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}

/// <summary>
/// Vertical slice extension for the stub — typed read, same pattern as real components.
/// </summary>
public static class StubFusionNumericTextBoxExtensions
{
    private static readonly StubFusionNumericTextBox _component = new();

    public static TypedComponentSource<decimal> Value<TModel>(
        this ComponentRef<StubFusionNumericTextBox, TModel> self)
        where TModel : class
        => new TypedComponentSource<decimal>(self.TargetId, _component.Vendor, _component.ReadExpr);
}

public class AmountModel
{
    public decimal Amount { get; set; }
}

[TestFixture]
public class WhenConditionReadsComponent : PlanTestBase
{
    [Test]
    public Task Component_source_gt_with_then_else()
    {
        var plan = new ReactivePlan<AmountModel>();
        var trigger = new Builders.TriggerBuilder<AmountModel>(plan);

        trigger.DomReady(p =>
        {
            var comp = p.Component<StubFusionNumericTextBox>(m => m.Amount);
            p.When(comp.Value()).Gt(0m)
                .Then(then => then.Element("status").SetText("positive"))
                .Else(else_ => else_.Element("status").SetText("zero or negative"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Component_source_eq_string_ref()
    {
        var plan = new ReactivePlan<AmountModel>();
        var trigger = new Builders.TriggerBuilder<AmountModel>(plan);

        trigger.DomReady(p =>
        {
            var comp = p.Component<StubFusionNumericTextBox>("my-numeric");
            p.When(comp.Value()).Eq(100m)
                .Then(then => then.Element("result").SetText("exact match"));
        });

        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
