using System;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListADeclaredMemberHoldsIsOperatedOn
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
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

    private const string Ends = "}]}]}";

    private const string Cart = "{\"kind\":\"obj\",\"id\":\"cart\"}";

    private const string TheDoses = "{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string ItsUnits = "{\"kind\":\"data\",\"path\":[{\"name\":\"units\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}],\"target\":" + Item + "}";

    private const string ItsDrug = "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsVolume = "{\"kind\":\"data\",\"path\":[{\"name\":\"volume\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string ItsPrice = "{\"kind\":\"data\",\"path\":[{\"name\":\"price\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string MoreThanTwoUnits = "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + ItsUnits + ",{\"kind\":\"lit\",\"value\":2}]}}";

    private const string Digoxin = "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"Digoxin\"}]}}";

    private const string Kept = "{\"kind\":\"call\",\"path\":[\"filter\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" + MoreThanTwoUnits + "]}";

    private const string FoundDigoxin = "{\"kind\":\"call\",\"path\":[\"find\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" + Digoxin + "]}";

    private const string Sets = "{\"kind\":\"set\",\"path\":[\"";

    private const string OnTheCart = "\"],\"members\":\"required\",\"content\":false,\"target\":" + Cart + ",\"value\":";

    private const string CountsEveryDose = OnLoad + Sets + "counted" + OnTheCart +
        "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + "}" + Ends;

    private const string CountsTheDosesKept = OnLoad + Sets + "counted" + OnTheCart +
        "{\"kind\":\"get\",\"path\":[\"length\"],\"members\":\"requiredIfHeld\",\"target\":" + Kept + "}" + Ends;

    private const string KeepsTheDoses = OnLoad + Sets + "shown" + OnTheCart + Kept + Ends;

    private const string SelectsEachDosesUnits = OnLoad + Sets + "units" + OnTheCart +
        "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"nonNull\",\"args\":[" + ItsUnits + "]}}]}" + Ends;

    private const string SelectsEachDosesDrug = OnLoad + Sets + "drugs" + OnTheCart +
        "{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + ItsDrug + "}]}" + Ends;

    private const string AnyDose = OnLoad + Sets + "flagged" + OnTheCart +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"lit\",\"value\":true}}]}" + Ends;

    private const string AnyDoseOfMoreThanTwoUnits = OnLoad + Sets + "flagged" + OnTheCart +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" + MoreThanTwoUnits + "]}" + Ends;

    private const string EveryDoseOfMoreThanTwoUnits = OnLoad + Sets + "flagged" + OnTheCart +
        "{\"kind\":\"call\",\"path\":[\"every\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" + MoreThanTwoUnits + "]}" + Ends;

    private const string FindsTheDigoxinDose = OnLoad + Sets + "chosen" + OnTheCart + FoundDigoxin + Ends;

    private const string ReadsTheUnitsOfTheDigoxinDose = OnLoad + Sets + "counted" + OnTheCart +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"units\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}],\"target\":" + FoundDigoxin + "}" + Ends;

    private const string SumsTheUnits = OnLoad + Sets + "counted" + OnTheCart +
        "{\"kind\":\"op\",\"op\":\"sumInt\",\"args\":[{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + ItsUnits + "}]}]}" + Ends;

    private const string SumsTheVolumes = OnLoad + Sets + "measured" + OnTheCart +
        "{\"kind\":\"op\",\"op\":\"sumDouble\",\"args\":[{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + ItsVolume + "}]}]}" + Ends;

    private const string SumsThePrices = OnLoad + Sets + "charged" + OnTheCart +
        "{\"kind\":\"op\",\"op\":\"sumDecimal\",\"args\":[{\"kind\":\"call\",\"path\":[\"map\"],\"members\":\"requiredIfHeld\",\"target\":" + TheDoses + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":" + ItsPrice + "}]}]}" + Ends;

    [Test]
    public void count_reads_the_length_of_the_list_reading_nothing_where_the_list_reads_nothing()
        => Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Count())), Is.EqualTo(ScriptOpen + CountsEveryDose + ScriptClose));

    [Test]
    public void count_with_a_condition_reads_the_length_of_the_items_the_condition_keeps()
        => Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Count(d => d.Units > 2))), Is.EqualTo(ScriptOpen + CountsTheDosesKept + ScriptClose));

    [Test]
    public void where_keeps_the_items_its_condition_holds_for_as_the_lists_filter()
        => Assert.That(Rendered(cart => cart.Shown.Set(cart.Doses.Where(d => d.Units > 2))), Is.EqualTo(ScriptOpen + KeepsTheDoses + ScriptClose));

    [Test]
    public void select_of_a_field_whose_type_holds_no_null_stops_the_pipeline_where_the_field_reads_nothing()
        => Assert.That(Rendered(cart => cart.Units.Set(cart.Doses.Select(d => d.Units))), Is.EqualTo(ScriptOpen + SelectsEachDosesUnits + ScriptClose));

    [Test]
    public void select_of_a_field_whose_type_holds_null_reads_null_where_the_field_reads_nothing()
        => Assert.That(Rendered(cart => cart.Drugs.Set(cart.Doses.Select(d => d.Drug))), Is.EqualTo(ScriptOpen + SelectsEachDosesDrug + ScriptClose));

    [Test]
    public void any_is_true_when_the_list_holds_an_item_as_some_over_every_item()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Doses.Any())), Is.EqualTo(ScriptOpen + AnyDose + ScriptClose));

    [Test]
    public void any_with_a_condition_is_true_when_the_condition_holds_for_an_item()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Doses.Any(d => d.Units > 2))), Is.EqualTo(ScriptOpen + AnyDoseOfMoreThanTwoUnits + ScriptClose));

    [Test]
    public void all_is_true_when_the_condition_holds_for_every_item()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Doses.All(d => d.Units > 2))), Is.EqualTo(ScriptOpen + EveryDoseOfMoreThanTwoUnits + ScriptClose));

    [Test]
    public void find_is_the_first_item_the_condition_keeps()
        => Assert.That(Rendered(cart => cart.Chosen.Set(cart.Doses.Find(d => d.Drug == "Digoxin"))), Is.EqualTo(ScriptOpen + FindsTheDigoxinDose + ScriptClose));

    [Test]
    public void find_with_a_field_reads_the_fields_path_from_the_item_found_as_data_is_read()
        => Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Units))), Is.EqualTo(ScriptOpen + ReadsTheUnitsOfTheDigoxinDose + ScriptClose));

    [Test]
    public void a_sum_of_whole_numbers_is_one_int_sum_over_the_map_of_its_term()
        => Assert.That(Rendered(cart => cart.Counted.Set(cart.Doses.Sum(d => d.Units))), Is.EqualTo(ScriptOpen + SumsTheUnits + ScriptClose));

    [Test]
    public void a_sum_of_doubles_is_one_double_sum_over_the_map_of_its_term()
        => Assert.That(Rendered(cart => cart.Measured.Set(cart.Doses.Sum(d => d.Volume))), Is.EqualTo(ScriptOpen + SumsTheVolumes + ScriptClose));

    [Test]
    public void a_sum_of_decimals_is_one_decimal_sum_over_the_map_of_its_term()
        => Assert.That(Rendered(cart => cart.Charged.Set(cart.Doses.Sum(d => d.Price))), Is.EqualTo(ScriptOpen + SumsThePrices + ScriptClose));

    [Test]
    public void the_plans_of_each_operation_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(CountsEveryDose), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(CountsTheDosesKept), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(KeepsTheDoses), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SelectsEachDosesUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SelectsEachDosesDrug), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(AnyDose), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(AnyDoseOfMoreThanTwoUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(EveryDoseOfMoreThanTwoUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(FindsTheDigoxinDose), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ReadsTheUnitsOfTheDigoxinDose), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SumsTheUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SumsTheVolumes), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(SumsThePrices), Is.Empty);
    }

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new MedicationRound("North", new Cover("Ruth")));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
