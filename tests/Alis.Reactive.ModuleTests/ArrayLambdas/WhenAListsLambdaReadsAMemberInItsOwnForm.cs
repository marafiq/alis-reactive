using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaReadsAMemberInItsOwnForm
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string AnyDose =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.ArrayLambdas.MedicationRound\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"cart\":{\"kind\":\"element\",\"id\":\"cart\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\"," +
        "\"target\":{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":";

    private const string Ends = "}]}}]}]}";

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string ItsCharting = "{\"kind\":\"data\",\"path\":[{\"name\":\"charted\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"Due\"}}],\"target\":" + Item + "}";

    private const string ItsVials =
        "{\"kind\":\"data\",\"path\":[{\"name\":\"supply\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0\"}}," +
        "{\"name\":\"vials\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0\"}}],\"target\":" + Item + "}";

    private const string ItsLot =
        "{\"kind\":\"data\",\"path\":[{\"name\":\"supply\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}," +
        "{\"name\":\"lot\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsRebate = "{\"kind\":\"data\",\"path\":[{\"name\":\"rebate\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0\"}}],\"target\":" + Item + "}";

    private const string GivenAsItsConverterWritesIt = "{\"kind\":\"lit\",\"value\":\"Given\"}";

    private const string TwoAsText = "{\"kind\":\"lit\",\"value\":\"2\"}";

    private const string ATenthAsText = "{\"kind\":\"lit\",\"value\":\"0.10\"}";

    private const string HeldInJavaScriptsValueByItsClass =
        "'d.Supply.Vials' is written by its class's JSON number handling, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server.";

    [Test]
    public void an_enum_its_own_converter_writes_is_compared_with_the_value_written_by_that_converter()
        => Assert.That(Rendered(d => d.Charted == DoseStatus.Given), Is.EqualTo(Plan(Op("eq", ItsCharting, GivenAsItsConverterWritesIt))));

    [Test]
    public void an_enum_its_own_converter_writes_is_told_apart_from_the_value_written_by_that_converter()
        => Assert.That(Rendered(d => d.Charted != DoseStatus.Given), Is.EqualTo(Plan(Op("neq", ItsCharting, GivenAsItsConverterWritesIt))));

    [Test]
    public void a_number_its_class_writes_as_text_is_compared_with_the_value_as_that_text()
        => Assert.That(Rendered(d => d.Supply.Vials == 2), Is.EqualTo(Plan(Op("eq", ItsVials, TwoAsText))));

    [Test]
    public void a_value_written_before_the_member_keeps_its_place()
        => Assert.That(Rendered(d => 2 == d.Supply.Vials), Is.EqualTo(Plan(Op("eq", TwoAsText, ItsVials))));

    [Test]
    public void a_decimal_its_own_number_handling_writes_as_text_is_compared_by_value()
        => Assert.That(Rendered(d => d.Rebate == 0.10m), Is.EqualTo(Plan(Op("eqDecimal", ItsRebate, ATenthAsText))));

    [Test]
    public void a_decimal_its_own_number_handling_writes_as_text_is_told_apart_by_value()
        => Assert.That(Rendered(d => d.Rebate != 0.10m), Is.EqualTo(Plan("{\"kind\":\"op\",\"op\":\"not\",\"args\":[" + Op("eqDecimal", ItsRebate, ATenthAsText) + "]}")));

    [Test]
    public void a_text_beside_numbers_its_class_writes_as_text_computes_as_dotnet_does()
        => Assert.That(
            Rendered(d => d.Supply.Lot.StartsWith("L", StringComparison.Ordinal)),
            Is.EqualTo(Plan(Op("startsWith", ItsLot, "{\"kind\":\"lit\",\"value\":\"L\"}"))));

    [Test]
    public void the_plans_of_each_comparison_in_an_own_form_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(AnyDose + Op("eq", ItsCharting, GivenAsItsConverterWritesIt) + Ends), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(AnyDose + Op("eq", TwoAsText, ItsVials) + Ends), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(AnyDose + "{\"kind\":\"op\",\"op\":\"not\",\"args\":[" + Op("eqDecimal", ItsRebate, ATenthAsText) + "]}" + Ends), Is.Empty);
    }

    [Test]
    public void a_number_its_class_writes_as_text_is_not_ordered()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Supply.Vials > 2))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Supply.Vials > 2' orders 'd.Supply.Vials', which its class's JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void a_decimal_its_own_number_handling_writes_as_text_is_not_ordered()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Rebate > 1m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Rebate > 1' orders 'd.Rebate', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void an_object_its_own_converter_writes_is_not_compared()
    {
        var seal = new Seal { Tag = "S4" };

        Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Tagged == seal))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Tagged == seal' compares 'd.Tagged', which its own JSON converter writes unlike JavaScript's value, and .NET compares the object it holds; compare it on the server."));
    }

    [Test]
    public void a_member_in_its_own_form_is_not_compared_with_another_member()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Supply.Vials == d.Units))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Supply.Vials == d.Units' compares two members, and its class's JSON number handling writes 'd.Supply.Vials' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));

    [Test]
    public void a_double_its_class_writes_as_text_is_not_compared()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Supply.Fill == 1.5))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Supply.Fill == 1.5' compares a float or double the JSON writes as text, whose text does not equal as the number does, as -0 and 0 or NaN and NaN; compare it on the server."));

    [Test]
    public void a_decimal_its_own_converter_writes_as_text_is_not_compared()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Tariff == 1.5m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Tariff == 1.5' compares a decimal its own converter writes as text, whose text the converter decides; compare it on the server."));

    [Test]
    public void a_date_its_own_converter_writes_is_not_compared()
    {
        var stamped = new DateTime(2026, 11, 14, 8, 0, 0, DateTimeKind.Unspecified);

        Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => d.Stamped == stamped))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Stamped == stamped' compares a date or a time its own converter writes, whose text the converter decides; compare it on the server."));
    }

    [Test]
    public void a_number_its_class_writes_as_text_is_no_order_key()
        => Assert.That(
            () => Refused(cart => cart.Shown.Set(cart.Doses.OrderBy(d => d.Supply.Vials))),
            Throws.InvalidOperationException.With.Message.EqualTo(HeldInJavaScriptsValueByItsClass));

    [Test]
    public void a_number_its_class_writes_as_text_is_no_term_of_a_sum_through_a_cast()
        => Assert.That(
            () => Refused(cart => cart.Charged.Set(cart.Doses.Sum(d => (decimal)d.Supply.Vials))),
            Throws.InvalidOperationException.With.Message.EqualTo(HeldInJavaScriptsValueByItsClass));

    [Test]
    public void a_number_its_class_writes_as_text_is_not_looked_up_in_a_list_the_view_holds()
    {
        var vials = new[] { 1, 2 };

        Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => vials.Contains(d.Supply.Vials)))),
            Throws.InvalidOperationException.With.Message.EqualTo(HeldInJavaScriptsValueByItsClass));
    }

    [Test]
    public void an_enum_its_own_converter_writes_is_not_looked_up_in_a_list_the_view_holds()
    {
        var charted = new[] { DoseStatus.Given };

        Assert.That(
            () => Refused(cart => cart.Flagged.Set(AnyOf(cart.Doses, d => charted.Contains(d.Charted)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Charted' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));
    }

    private static string Op(string op, string left, string right) => "{\"kind\":\"op\",\"op\":\"" + op + "\",\"args\":[" + left + "," + right + "]}";

    private static string Plan(string body) => ScriptOpen + AnyDose + body + Ends + ScriptClose;

    private static string Rendered(Expression<Func<CartDose, bool>> predicate)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Flagged.Set(cart.Doses.Any(predicate));
        }));
        return Views.TextOf(html.RenderPlan(plan));
    }

    private static void Refused(Action<DoseCart> step)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        html.RenderPlan(plan);
    }

    private static TypedSource<bool> AnyOf(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, bool>> predicate) => doses.Any(predicate);
}
