using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests;

// Reuse existing stubs from WhenConditionReadsComponent.cs: StubFusionNumericTextBox
// Add one more stub for string comparison

public sealed class StubDropDown : IComponent, IInputComponent
{
    public string Vendor => "fusion";
    public string ReadExpr => "value";
}

public static class StubDropDownExtensions
{
    private static readonly StubDropDown _c = new();
    public static TypedComponentSource<string> Value<TModel>(
        this ComponentRef<StubDropDown, TModel> self) where TModel : class
        => new TypedComponentSource<string>(self.TargetId, _c.Vendor, _c.ReadExpr);
}

public class TwoSourceModel
{
    public decimal Rate { get; set; }
    public decimal Budget { get; set; }
    public string? Primary { get; set; }
    public string? Secondary { get; set; }
}

public class InvoiceArgs
{
    public decimal Amount { get; set; }
}

[TestFixture]
public class WhenComparingTwoSources : PlanTestBase
{
    // ── 6 operators: Syntax 1 (direct chain) ──

    [Test]
    public Task Decimal_lte() => TwoComponents((rate, budget) =>
        rate.Lte(budget));

    [Test]
    public Task Decimal_lt() => TwoComponents((rate, budget) =>
        rate.Lt(budget));

    [Test]
    public Task Decimal_gt() => TwoComponents((rate, budget) =>
        rate.Gt(budget));

    [Test]
    public Task Decimal_gte() => TwoComponents((rate, budget) =>
        rate.Gte(budget));

    [Test]
    public Task Decimal_eq() => TwoComponents((rate, budget) =>
        rate.Eq(budget));

    [Test]
    public Task Decimal_neq() => TwoComponents((rate, budget) =>
        rate.NotEq(budget));

    // ── String source-vs-source ──

    [Test]
    public Task String_eq()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var a = p.Component<StubDropDown>(m => m.Primary);
            var b = p.Component<StubDropDown>(m => m.Secondary);
            p.When(a.Value()).Eq(b.Value())
                .Then(t => t.Element("match").Show())
                .Else(e => e.Element("match").Hide());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Event payload vs component ──

    [Test]
    public Task Event_vs_component()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan)
            .CustomEvent<InvoiceArgs>("invoice", (args, p) =>
            {
                var max = p.Component<StubFusionNumericTextBox>(m => m.Rate);
                p.When(args, x => x.Amount).Gt(max.Value())
                    .Then(t => t.Element("warn").Show())
                    .Else(e => e.Element("warn").Hide());
            });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Source-vs-source in AND chain (Syntax 1) ──

    [Test]
    public Task In_direct_and()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var rate = p.Component<StubFusionNumericTextBox>(m => m.Rate);
            var budget = p.Component<StubFusionNumericTextBox>(m => m.Budget);
            p.When(rate.Value()).Gt(0m)
                .And(rate.Value()).Lte(budget.Value())
                .Then(t => t.Element("ok").Show())
                .Else(e => e.Element("ok").Hide());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Source-vs-source in lambda AND (Syntax 2) ──

    [Test]
    public Task In_lambda_and()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var rate = p.Component<StubFusionNumericTextBox>(m => m.Rate);
            var budget = p.Component<StubFusionNumericTextBox>(m => m.Budget);
            p.When(rate.Value()).Gt(0m)
                .And(g => g.When(rate.Value()).Lte(budget.Value()))
                .Then(t => t.Element("ok").Show())
                .Else(e => e.Element("ok").Hide());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Source-vs-source in ElseIf ──

    [Test]
    public Task In_elseif()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var rate = p.Component<StubFusionNumericTextBox>(m => m.Rate);
            var budget = p.Component<StubFusionNumericTextBox>(m => m.Budget);
            p.When(rate.Value()).Lt(budget.Value())
                .Then(t => t.Element("s").SetText("Under"))
                .ElseIf(rate.Value()).Eq(budget.Value())
                .Then(t => t.Element("s").SetText("Exact"))
                .Else(e => e.Element("s").SetText("Over"));
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Source-vs-source with Confirm ──

    [Test]
    public Task With_confirm()
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var rate = p.Component<StubFusionNumericTextBox>(m => m.Rate);
            var budget = p.Component<StubFusionNumericTextBox>(m => m.Budget);
            p.When(rate.Value()).Gt(budget.Value())
                .And(g => g.Confirm("Exceeds budget. Continue?"))
                .Then(t => t.Dispatch("submit"))
                .Else(e => e.Element("status").SetText("Cancelled"));
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }

    // ── Helper ──

    private Task TwoComponents(
        Func<ConditionSourceBuilder<TwoSourceModel, decimal>,
             TypedComponentSource<decimal>,
             GuardBuilder<TwoSourceModel>> op)
    {
        var plan = new ReactivePlan<TwoSourceModel>();
        new Builders.TriggerBuilder<TwoSourceModel>(plan).DomReady(p =>
        {
            var rate = p.Component<StubFusionNumericTextBox>(m => m.Rate);
            var budget = p.Component<StubFusionNumericTextBox>(m => m.Budget);
            op(p.When(rate.Value()), budget.Value())
                .Then(t => t.Element("result").Show())
                .Else(e => e.Element("result").Hide());
        });
        AssertSchemaValid(plan.Render());
        return VerifyJson(plan.Render());
    }
}
