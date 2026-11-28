using System;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaComparesAnItem
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
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"set\",\"path\":[\"flagged\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\"," +
        "\"target\":{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":";

    private const string Ends = "}]}}]}]}";

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string ItsUnits = "{\"kind\":\"data\",\"path\":[{\"name\":\"units\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}],\"target\":" + Item + "}";

    private const string ItsDrug = "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsWard = "{\"kind\":\"data\",\"path\":[{\"name\":\"ward\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsVolume = "{\"kind\":\"data\",\"path\":[{\"name\":\"volume\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string ItsPriority = "{\"kind\":\"data\",\"path\":[{\"name\":\"priority\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":" + Item + "}";

    private const string ItsInitial = "{\"kind\":\"data\",\"path\":[{\"name\":\"initial\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"A\"}}],\"target\":" + Item + "}";

    private const string ItsStatus = "{\"kind\":\"data\",\"path\":[{\"name\":\"status\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string ItsDueKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"due\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":" + Item + "}]}";

    private const string ItsGivenKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"given\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":" + Item + "}]}";

    private const string MoreThanTwoUnits = "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + ItsUnits + ",{\"kind\":\"lit\",\"value\":2}]}";

    private const string GroupedAsCSharpGroupsThem = OnLoad +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"and\",\"args\":[" + MoreThanTwoUnits + "," +
        "{\"kind\":\"op\",\"op\":\"not\",\"args\":[{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsDrug + ",{\"kind\":\"lit\",\"value\":\"Digoxin\"}]}]}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsPriority + ",{\"kind\":\"lit\",\"value\":null}]}]}" + Ends;

    private const string TwoMembersCompared = OnLoad + "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + ItsUnits + "," + ItsPriority + "]}" + Ends;

    private const string DueBeforeTheCutoff = OnLoad +
        "{\"kind\":\"op\",\"op\":\"lt\",\"args\":[" + ItsDueKey + ",{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-11-14T08:30:00\"}]}]}" + Ends;

    private const string DueAfterGiven = OnLoad + "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + ItsDueKey + "," + ItsGivenKey + "]}" + Ends;

    private const string InitialD = OnLoad + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsInitial + ",{\"kind\":\"lit\",\"value\":\"D\"}]}" + Ends;

    private const string TheRoundsWard = OnLoad + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsWard + ",{\"kind\":\"lit\",\"value\":\"North\"}]}" + Ends;

    private const string PriorityOrNone = OnLoad +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[" + ItsPriority + ",{\"kind\":\"lit\",\"value\":0}]},{\"kind\":\"lit\",\"value\":1}]}" + Ends;

    private const string VolumeOrUnits = OnLoad +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + MoreThanTwoUnits + "," + ItsVolume + "," + ItsUnits + "]}," +
        "{\"kind\":\"lit\",\"value\":1.5}]}" + Ends;

    private const string PrioritysValue = OnLoad +
        "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[{\"kind\":\"op\",\"op\":\"nullableValue\",\"args\":[" + ItsPriority + "]},{\"kind\":\"lit\",\"value\":2}]}" + Ends;

    private const string UnitsOverAFraction = OnLoad + "{\"kind\":\"op\",\"op\":\"gt\",\"args\":[" + ItsUnits + ",{\"kind\":\"lit\",\"value\":2.5}]}" + Ends;

    private const string StatusDue = OnLoad + "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsStatus + ",{\"kind\":\"lit\",\"value\":1}]}" + Ends;

    [Test]
    public void and_or_and_not_keep_the_grouping_csharp_gives_them()
        => Assert.That(Rendered(d => d.Units > 2 && !(d.Drug == "Digoxin") || d.Priority == null), Is.EqualTo(ScriptOpen + GroupedAsCSharpGroupsThem + ScriptClose));

    [Test]
    public void two_members_of_an_item_are_compared_as_each_reads()
        => Assert.That(Rendered(d => d.Units > d.Priority), Is.EqualTo(ScriptOpen + TwoMembersCompared + ScriptClose));

    [Test]
    public void a_date_is_compared_by_its_key_with_a_date_the_view_holds()
    {
        var cutoff = new DateTime(2026, 11, 14, 8, 30, 0);

        Assert.That(Rendered(d => d.Due < cutoff), Is.EqualTo(ScriptOpen + DueBeforeTheCutoff + ScriptClose));
    }

    [Test]
    public void two_dates_of_one_type_are_ordered_by_their_keys()
        => Assert.That(Rendered(d => d.Due > d.Given), Is.EqualTo(ScriptOpen + DueAfterGiven + ScriptClose));

    [Test]
    public void a_char_is_compared_with_the_char_the_view_wrote_and_not_with_its_code()
        => Assert.That(Rendered(d => d.Initial == 'D'), Is.EqualTo(ScriptOpen + InitialD + ScriptClose));

    [Test]
    public void a_member_of_the_views_model_is_written_as_it_is_when_the_view_renders()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Flagged.Set(cart.Doses.Any(d => d.Ward == html.ViewData.Model.Ward));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + TheRoundsWard + ScriptClose));
    }

    [Test]
    public void a_coalesce_reads_its_second_side_where_its_first_holds_null()
        => Assert.That(Rendered(d => (d.Priority ?? 0) > 1), Is.EqualTo(ScriptOpen + PriorityOrNone + ScriptClose));

    [Test]
    public void a_choice_reads_the_branch_its_test_picks()
        => Assert.That(Rendered(d => (d.Units > 2 ? d.Volume : d.Units) > 1.5), Is.EqualTo(ScriptOpen + VolumeOrUnits + ScriptClose));

    [Test]
    public void a_cast_that_takes_a_nullables_value_reads_its_value_and_nothing_where_it_holds_null()
        => Assert.That(Rendered(d => (int)d.Priority! > 2), Is.EqualTo(ScriptOpen + PrioritysValue + ScriptClose));

    [Test]
    public void a_whole_number_compared_with_a_fraction_is_compared_with_the_fraction_itself()
        => Assert.That(Rendered(d => d.Units > 2.5), Is.EqualTo(ScriptOpen + UnitsOverAFraction + ScriptClose));

    [Test]
    public void a_whole_number_compared_with_a_decimal_reads_the_member_since_its_conversion_is_exact()
        => Assert.That(Rendered(d => d.Units > 2.5m), Is.EqualTo(ScriptOpen + UnitsOverAFraction + ScriptClose));

    [Test]
    public void an_enum_is_compared_with_the_number_the_live_form_writes_for_the_value_the_view_named()
        => Assert.That(Rendered(d => d.Status == DoseStatus.Due), Is.EqualTo(ScriptOpen + StatusDue + ScriptClose));

    [Test]
    public void the_plans_of_each_comparison_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(GroupedAsCSharpGroupsThem), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(TwoMembersCompared), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(DueBeforeTheCutoff), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(DueAfterGiven), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(InitialD), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(TheRoundsWard), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(PriorityOrNone), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(VolumeOrUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(PrioritysValue), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(UnitsOverAFraction), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(StatusDue), Is.Empty);
    }

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
}
