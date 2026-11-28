using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionOrdersAValueThePageHolds
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Conditions.DoseRound\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"cart\":{\"kind\":\"element\",\"id\":\"cart\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string Ends = "]}]}";

    private const string Cart = "{\"kind\":\"obj\",\"id\":\"cart\"}";

    private const string SetsFlagged = "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":" + Cart + ",\"value\":";

    private const string Charged = "{\"kind\":\"get\",\"path\":[\"charged\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Ceiling = "{\"kind\":\"get\",\"path\":[\"ceiling\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Copay = "{\"kind\":\"get\",\"path\":[\"copay\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Five = "{\"kind\":\"lit\",\"value\":5}";

    private const string ChargedAboveFive = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + Charged + "," + Five + "]}}" + Ends;

    private const string ChargedAtLeastFive = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"gte\",\"args\":[" + Charged + "," + Five + "]}}" + Ends;

    private const string ChargedBelowFive = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" + Charged + "," + Five + "]}}" + Ends;

    private const string ChargedAtMostFive = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"lte\",\"args\":[" + Charged + "," + Five + "]}}" + Ends;

    private const string ChargedAtLeastTheCeiling = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"gte\",\"args\":[" + Charged + "," + Ceiling + "]}}" + Ends;

    private const string CopayAboveFive = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + Copay + "," + Five + "]}}" + Ends;

    private const string DueBeforeTheRound = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"get\",\"path\":[\"due\"],\"members\":\"required\",\"target\":" + Cart + "}]}," +
        "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-11-15T09:30:00\"}]}]}}" + Ends;

    private const string ChargedBetweenOneAndFive = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"gte\",\"args\":[" + Charged + ",{\"kind\":\"lit\",\"value\":1}]}," +
        "{\"kind\":\"op\",\"op\":\"lte\",\"args\":[" + Charged + "," + Five + "]}]}}" + Ends;

    [Test]
    public void a_number_above_a_literal_is_gt()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Gt(5m))), Is.EqualTo(ScriptOpen + ChargedAboveFive + ScriptClose));

    [Test]
    public void a_number_at_least_a_literal_is_gte()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Gte(5m))), Is.EqualTo(ScriptOpen + ChargedAtLeastFive + ScriptClose));

    [Test]
    public void a_number_below_a_literal_is_lt()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Lt(5m))), Is.EqualTo(ScriptOpen + ChargedBelowFive + ScriptClose));

    [Test]
    public void a_number_at_most_a_literal_is_lte()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Lte(5m))), Is.EqualTo(ScriptOpen + ChargedAtMostFive + ScriptClose));

    [Test]
    public void a_number_is_ordered_against_another_value_the_page_holds()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Gte(cart.Ceiling))), Is.EqualTo(ScriptOpen + ChargedAtLeastTheCeiling + ScriptClose));

    [Test]
    public void a_nullable_number_is_ordered_against_a_literal()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Copay.Gt(5m))), Is.EqualTo(ScriptOpen + CopayAboveFive + ScriptClose));

    [Test]
    public void a_date_is_ordered_by_its_key()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Due.Lt(new DateTime(2026, 11, 15, 9, 30, 0)))), Is.EqualTo(ScriptOpen + DueBeforeTheRound + ScriptClose));

    [Test]
    public void a_number_between_two_literals_is_at_least_the_low_and_at_most_the_high()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Charged.Between(1m, 5m))), Is.EqualTo(ScriptOpen + ChargedBetweenOneAndFive + ScriptClose));

    [Test]
    public void the_plans_of_each_ordering_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(ChargedAboveFive), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChargedAtLeastFive), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChargedBelowFive), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChargedAtMostFive), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChargedAtLeastTheCeiling), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(CopayAboveFive), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(DueBeforeTheRound), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChargedBetweenOneAndFive), Is.Empty);
    }

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new DoseRound("North"));
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
