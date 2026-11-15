using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Conditions;
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

    private const string OnCart =
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
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string Doses = "{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}";

    private const string Counted = "{\"kind\":\"set\",\"path\":[\"counted\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":";

    private const string Charged = "{\"kind\":\"set\",\"path\":[\"charged\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":";

    private const string SelectedCharts =
        "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[" + ItsCharting + "]}}]}";

    private const string SelectedRebates =
        "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[" + ItsRebate + "]}}]}";

    [Test]
    public void a_selected_chart_is_compared_in_its_converters_form_and_its_count_is_live()
    {
        const string expected = OnCart + Counted +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + SelectedCharts + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + GivenAsItsConverterWritesIt + "]}}]}}}]}]}";

        Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Select(d => d.Charted).Count(s => s == DoseStatus.Given))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void selecting_a_chart_again_keeps_its_converters_form()
    {
        const string selectedAgain =
            "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + SelectedCharts + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[" + Item + "]}}]}";
        const string expected = OnCart + Counted +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + selectedAgain + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + GivenAsItsConverterWritesIt + "]}}]}}}]}]}";

        Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Select(d => d.Charted).Select(s => s).Count(s => s == DoseStatus.Given))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_decimal_sum_reads_its_members_number_handling_text()
    {
        const string expected = OnCart + Charged +
            "{\"kind\":\"op\",\"op\":\"sumDecimal\",\"args\":[{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + ItsRebate + "}]}]}}]}]}";

        Assert.That(Rendered(cart => cart.Charged.Set(cart.Doses.Sum(d => d.Rebate))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_decimal_sum_reads_selected_items_as_their_number_handling_text()
    {
        const string expected = OnCart + Charged +
            "{\"kind\":\"op\",\"op\":\"sumDecimal\",\"args\":[{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + SelectedRebates + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + Item + "}]}]}}]}]}";

        Assert.That(Rendered(cart => cart.Charged.Set(cart.Doses.Select(d => d.Rebate).Sum(rebate => rebate))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void selected_decimal_text_is_compared_by_value()
    {
        const string expected = OnCart + Counted +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + SelectedRebates + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eqDecimal\",\"args\":[" + Item + "," + ATenthAsText + "]}}]}}}]}]}";

        Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Select(d => d.Rebate).Count(rebate => rebate == 0.10m))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_selected_whole_number_written_as_text_is_not_a_sum_term()
        => Assert.That(
            () => Rendered(cart => cart.Counted.Set(cart.Doses.Select(d => d.Supply.Vials).Sum(v => v))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'v' is written by its class's JSON number handling, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_selected_decimal_written_by_its_converter_is_not_a_sum_term()
        => Assert.That(
            () => Rendered(cart => cart.Charged.Set(cart.Doses.Select(d => d.Tariff).Sum(tariff => tariff))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'tariff' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

    [Test]
    public void a_selected_object_written_by_its_converter_is_not_compared()
    {
        var seal = new Seal { Tag = "S4" };

        Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Select(d => d.Tagged).Any(s => s == seal))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'s == seal' compares 's', which its own JSON converter writes unlike JavaScript's value, and .NET compares the object it holds; compare it on the server."));
    }

    [Test]
    public void a_value_the_view_holds_beside_a_selected_chart_is_written_as_javascripts_value()
    {
        const string expected = OnCart +
            "{\"kind\":\"set\",\"path\":[\"units\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
            "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + SelectedCharts + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" +
            "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + GivenAsItsConverterWritesIt + "]}," +
            "{\"kind\":\"lit\",\"value\":1},{\"kind\":\"lit\",\"value\":2}]}]}}]}}]}]}";

        Assert.That(Rendered(cart => cart.Units.Set(cart.Doses.Select(d => d.Charted).Select(s => s == DoseStatus.Given ? 1 : 2))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_selected_chart_its_converter_reads_only_as_text_is_compared_with_that_text()
    {
        const string selectedSignatures =
            "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[" +
            "{\"kind\":\"data\",\"path\":[{\"name\":\"signed\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"Due\"}}],\"target\":" + Item + "}]}}]}";
        const string expected = OnCart + Counted +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + selectedSignatures + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + GivenAsItsConverterWritesIt + "]}}]}}}]}]}";

        Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Select(d => d.Signed).Count(s => s == DoseStatus.Given))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_found_selected_rebate_carries_its_read_into_a_conditions_refusal()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(cart.Doses.Select(d => d.Rebate).Find(rebate => true).Gt(5m))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition orders 'd.Rebate', which its own JSON number handling writes unlike JavaScript's value, so the browser does not order it as .NET does; order it on the server."));

    [Test]
    public void a_number_in_a_list_its_number_handling_writes_as_text_is_compared_with_that_text()
    {
        const string foundCounts =
            "{\"kind\":\"data\",\"path\":[{\"name\":\"counts\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":[]}}],\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"find\"],\"members\":\"requiredIfHeld\",\"target\":" + Doses + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
            "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}," +
            "{\"kind\":\"lit\",\"value\":\"Digoxin\"}]}}]}}";
        const string expected = OnCart + Counted +
            "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" +
            "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + foundCounts + ",\"args\":[" +
            "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + TwoAsText + "]}}]}}}]}]}";

        Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Counts).Count(c => c == 2))), Is.EqualTo(ScriptOpen + expected + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(expected), Is.Empty);
    }

    [Test]
    public void a_lambda_reading_past_a_selected_item_its_converter_writes_is_refused()
        => Assert.That(
            () => Refused(cart => cart.Counted.Set(cart.Doses.Select(d => d.Tagged).Count(s => s.Tag == "S4"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'s.Tag' reads past 's', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 's' itself, or read past it on the server."));

    [Test]
    public void a_found_field_past_a_selected_item_its_converter_writes_is_refused()
        => Assert.That(
            () => Refused(cart => cart.Flagged.Set(cart.Doses.Select(d => d.Tagged).Find(s => true, s => s.Tag).Eq("S4"))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'s.Tag' reads past 's', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 's' itself, or read past it on the server."));

    [Test]
    public void a_list_operation_over_a_list_its_own_converter_writes_whole_is_refused()
        => Assert.That(
            () => Refused(cart => cart.Counted.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Lots).Count())),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Lots' is written by its own JSON converter, unlike JavaScript's value, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server."));

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

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
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
