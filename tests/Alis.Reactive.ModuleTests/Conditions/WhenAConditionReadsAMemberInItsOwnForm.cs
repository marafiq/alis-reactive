using System;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConditionReadsAMemberInItsOwnForm
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

    private const string Doses = "{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Flagged = "{\"kind\":\"get\",\"path\":[\"flagged\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string SetsFlagged = "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":" + Cart + ",\"value\":";

    private const string Digoxin =
        "{\"kind\":\"call\",\"path\":[\"find\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":{\"kind\":\"ref\",\"name\":\"item1\"}}," +
        "{\"kind\":\"lit\",\"value\":\"Digoxin\"}]}}]}";

    private const string Aspirin =
        "{\"kind\":\"call\",\"path\":[\"find\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":{\"kind\":\"ref\",\"name\":\"item1\"}}," +
        "{\"kind\":\"lit\",\"value\":\"Aspirin\"}]}}]}";

    private const string DigoxinCharted = "{\"kind\":\"data\",\"path\":[{\"name\":\"charted\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"Due\"}}],\"target\":" + Digoxin + "}";

    private const string AspirinCharted = "{\"kind\":\"data\",\"path\":[{\"name\":\"charted\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"Due\"}}],\"target\":" + Aspirin + "}";

    private const string DigoxinEarlierCharted = "{\"kind\":\"data\",\"path\":[{\"name\":\"earlierCharted\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Digoxin + "}";

    private const string DigoxinRebate = "{\"kind\":\"data\",\"path\":[{\"name\":\"rebate\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0\"}}],\"target\":" + Digoxin + "}";

    private const string Given = "{\"kind\":\"lit\",\"value\":\"Given\"}";

    private const string Due = "{\"kind\":\"lit\",\"value\":\"Due\"}";

    private const string ATenthAsText = "{\"kind\":\"lit\",\"value\":\"0.10\"}";

    private const string GivenWhenFound = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + DigoxinCharted + "," + Given + "]}," +
        "\"then\":[" + SetsFlagged + "{\"kind\":\"lit\",\"value\":true}}],\"else\":[]}" + Ends;

    private const string NotGivenWhenFound = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + DigoxinCharted + "," + Given + "]}}" + Ends;

    private const string ATenthByValue = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"eqDecimal\",\"args\":[" + DigoxinRebate + "," + ATenthAsText + "]}}" + Ends;

    private const string NotATenthByValue = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"not\",\"args\":[{\"kind\":\"op\",\"op\":\"eqDecimal\",\"args\":[" + DigoxinRebate + "," + ATenthAsText + "]}]}}" + Ends;

    private const string FallbackInTheChartsForm = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[" + DigoxinEarlierCharted + "," + Given + "]}," + Given + "]}}" + Ends;

    private const string LiteralArmInTheChartsForm = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + Flagged + "," + Given + "," + DigoxinCharted + "]}," + Given + "]}}" + Ends;

    private const string TwoChartsJoined = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + Flagged + "," + DigoxinCharted + "," + AspirinCharted + "]}," + Given + "]}}" + Ends;

    private const string ChartInGivenOrDue = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + DigoxinCharted + "," + Given + "]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + DigoxinCharted + "," + Due + "]}]}}" + Ends;

    private const string RebateInATenth = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"eqDecimal\",\"args\":[" + DigoxinRebate + "," + ATenthAsText + "]}}" + Ends;

    [Test]
    public void a_condition_writes_its_literal_in_the_found_charts_form()
    {
        var html = Views.Of(new DoseRound("North"));
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            pipeline.When(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted).Eq(DoseStatus.Given))
                .Then(pipeline => cart.Flagged.Set(true));
        }));

        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + GivenWhenFound + ScriptClose));
    }

    [Test]
    public void a_chart_that_is_not_given_is_compared_with_the_text_its_converter_writes()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted).NotEq(DoseStatus.Given))),
            Is.EqualTo(ScriptOpen + NotGivenWhenFound + ScriptClose));

    [Test]
    public void a_decimal_its_number_handling_writes_as_text_equals_by_value()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate).Eq(0.10m))),
            Is.EqualTo(ScriptOpen + ATenthByValue + ScriptClose));

    [Test]
    public void a_decimal_its_number_handling_writes_as_text_differs_by_value()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate).NotEq(0.10m))),
            Is.EqualTo(ScriptOpen + NotATenthByValue + ScriptClose));

    [Test]
    public void an_ordering_of_a_member_in_its_own_form_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate).Gt(5m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition orders 'd.Rebate', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void an_ordering_of_a_member_in_its_own_form_against_a_value_read_from_the_page_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate).Gte(cart.Charged))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition orders 'd.Rebate', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void an_ordering_against_a_member_in_its_own_form_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Charged.Lt(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition orders 'd.Rebate', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void an_object_its_own_converter_writes_is_not_compared()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Tagged).Eq(new Seal { Tag = "S4" }))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares 'd.Tagged', which its own JSON converter writes unlike JavaScript's value, and .NET compares the object it holds; compare it on the server."));

    [Test]
    public void two_values_where_one_is_in_its_own_form_are_not_compared()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Status)
                .Eq(cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Charted)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares two values, and its own JSON converter writes 'd.Charted' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));

    [Test]
    public void a_double_its_number_handling_writes_as_text_is_not_compared()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Fill).Eq(0.5))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares a float or double the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; compare it on the server."));

    [Test]
    public void a_decimal_its_own_converter_writes_as_text_is_not_compared()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Tariff).Eq(1m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares a decimal its own converter writes as text, whose text the converter decides; compare it on the server."));

    [Test]
    public void a_date_its_own_converter_writes_is_not_compared()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Stamped).Eq(new DateTime(2026, 11, 15)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares a date or a time its own converter writes, whose text the converter decides; compare it on the server."));

    [Test]
    public void a_fallback_is_written_in_the_found_charts_form()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.EarlierCharted).IfNull(DoseStatus.Given).Eq(DoseStatus.Given))),
            Is.EqualTo(ScriptOpen + FallbackInTheChartsForm + ScriptClose));

    [Test]
    public void a_literal_arm_is_written_in_the_found_charts_form()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Flagged.Choose(DoseStatus.Given, cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted)).Eq(DoseStatus.Given))),
            Is.EqualTo(ScriptOpen + LiteralArmInTheChartsForm + ScriptClose));

    [Test]
    public void a_fallback_read_from_the_page_is_not_joined_with_a_member_in_its_own_form()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.EarlierCharted).IfNull(cart.Status).Eq(DoseStatus.Given))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A choice joins 'd.EarlierCharted', which its own JSON converter writes unlike JavaScript's value, with another value read from the page; choose between it and a value the view writes, or choose on the server."));

    [Test]
    public void a_choice_of_a_member_in_its_own_form_and_a_value_read_from_the_page_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Flagged.Choose(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted), cart.Status).Eq(DoseStatus.Given))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A choice joins 'd.Charted', which its own JSON converter writes unlike JavaScript's value, with another value read from the page; choose between it and a value the view writes, or choose on the server."));

    [Test]
    public void a_choice_names_the_base_form_of_the_member_its_own_converter_writes_when_it_is_read_second()
        => Assert.That(
            () => RenderedOnAReply(pipeline => pipeline.Get("/dose").OnSuccess<CartDose>((dose, pipeline) =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(cart.Flagged.Choose(cart.Status, dose.Body.Read(d => d.Charted)).Eq(DoseStatus.Given));
            })),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A choice joins 'd.Charted', which its own JSON converter writes unlike the app's options, with another value read from the page; choose between it and a value the view writes, or choose on the server."));

    [Test]
    public void a_choice_of_two_reads_of_one_member_keeps_its_form()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Flagged.Choose(
                cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted),
                cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Charted)).Eq(DoseStatus.Given))),
            Is.EqualTo(ScriptOpen + TwoChartsJoined + ScriptClose));

    [Test]
    public void a_chart_in_literal_values_compares_each_in_its_converters_form()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted).In(DoseStatus.Given, DoseStatus.Due))),
            Is.EqualTo(ScriptOpen + ChartInGivenOrDue + ScriptClose));

    [Test]
    public void a_decimal_text_in_literal_values_compares_by_value()
        => Assert.That(
            Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Rebate).In(0.10m))),
            Is.EqualTo(ScriptOpen + RebateInATenth + ScriptClose));

    [Test]
    public void a_text_test_of_a_text_its_own_converter_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).StartsWith("Ad"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void an_emptiness_test_of_a_list_its_own_converter_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Lots).IsEmpty())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Lots' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_contains_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).Contains("Ad"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void an_ends_with_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).EndsWith("Ad"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_pattern_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).Matches("[Aa]d"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_length_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).MinLength(2))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void an_emptiness_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).IsEmpty())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_nonempty_test_refuses_a_text_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Note).NotEmpty())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_nonempty_test_refuses_a_list_its_own_converter_writes()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Lots).NotEmpty())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Lots' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_contains_operand_its_own_converter_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Drug).Contains(cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Note)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_starts_with_operand_its_own_converter_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Drug).StartsWith(cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Note)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void an_ends_with_operand_its_own_converter_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Drug).EndsWith(cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Note)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Note' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_length_operand_its_own_number_handling_writes_is_refused()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Drug).MinLength(cart.Doses.Find(d => d.Drug == "Aspirin", d => d.Minimum)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Minimum' is written by its own JSON number handling, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_length_operand_in_the_live_form_is_read_as_a_number()
    {
        var expected = OnLoad + SetsFlagged +
            "{\"kind\":\"op\",\"op\":\"gte\",\"args\":[{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Digoxin + "}}," +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + "}]}}" + Ends;
        Assert.That(Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Drug).MinLength(cart.Doses.Count()))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void the_plans_of_each_condition_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(GivenWhenFound), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(NotGivenWhenFound), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ATenthByValue), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(NotATenthByValue), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(FallbackInTheChartsForm), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(LiteralArmInTheChartsForm), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(TwoChartsJoined), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ChartInGivenOrDue), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(RebateInATenth), Is.Empty);
    }

    private static string RenderedOnAReply(Action<Pipeline<DoseRound>> write)
    {
        var html = Views.Of(new DoseRound("North"), _ => { }, app => app.AddReactiveRequests(requests => requests.NoAntiforgery()));
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(write));
        return Views.TextOf(html.RenderPlan(plan));
    }

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new DoseRound("North"));
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
