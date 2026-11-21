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
public sealed class WhenAListADeclaredMemberHoldsIsOrdered
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string PlanOpen = "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.ArrayLambdas.MedicationRound\",";

    private const string EnUs = "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\",";

    private const string DeDe = "\"culture\":{\"name\":\"de-DE\",\"collation\":\"de-DE\",\"decimal\":\",\",\"group\":\".\",";

    private const string OnLoad =
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"cart\":{\"kind\":\"element\",\"id\":\"cart\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        "{\"kind\":\"set\",\"path\":[\"shown\"],\"members\":\"required\",\"content\":false,\"target\":{\"kind\":\"obj\",\"id\":\"cart\"},\"value\":" +
        "{\"kind\":\"call\",\"path\":[\"toSorted\"],\"members\":\"requiredIfHeld\"," +
        "\"target\":{\"kind\":\"get\",\"path\":[\"doses\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"cart\"}}," +
        "\"args\":[{\"kind\":\"fn\",\"params\":[\"left1\",\"right1\"],\"body\":{\"kind\":\"op\",\"op\":";

    private const string Ends = "]}}]}}]}]}";

    private const string Left = "{\"kind\":\"ref\",\"name\":\"left1\"}";

    private const string Right = "{\"kind\":\"ref\",\"name\":\"right1\"}";

    private const string Units = "{\"kind\":\"data\",\"path\":[{\"name\":\"units\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}],\"target\":";

    private const string Priority = "{\"kind\":\"data\",\"path\":[{\"name\":\"priority\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":null}}],\"target\":";

    private const string Late = "{\"kind\":\"data\",\"path\":[{\"name\":\"late\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":false}}],\"target\":";

    private const string Initial = "{\"kind\":\"data\",\"path\":[{\"name\":\"initial\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"A\"}}],\"target\":";

    private const string Status = "{\"kind\":\"data\",\"path\":[{\"name\":\"status\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":";

    private const string Drug = "{\"kind\":\"data\",\"path\":[{\"name\":\"drug\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":";

    private const string DueKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"data\",\"path\":[{\"name\":\"due\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\"}}],\"target\":";

    private const string ByUnits = PlanOpen + EnUs + OnLoad + "\"compareNumbers\",\"args\":[" + Units + Left + "}," + Units + Right + "}" + Ends;

    private const string ByUnitsGreatestFirst = PlanOpen + EnUs + OnLoad + "\"compareNumbers\",\"args\":[" + Units + Right + "}," + Units + Left + "}" + Ends;

    private const string ByPriority = PlanOpen + EnUs + OnLoad + "\"compareNumbers\",\"args\":[" + Priority + Left + "}," + Priority + Right + "}" + Ends;

    private const string ByPriorityGreatestFirst = PlanOpen + EnUs + OnLoad + "\"compareNumbers\",\"args\":[" + Priority + Right + "}," + Priority + Left + "}" + Ends;

    private const string ByLateness = PlanOpen + EnUs + OnLoad + "\"compareBooleans\",\"args\":[" + Late + Left + "}," + Late + Right + "}" + Ends;

    private const string ByInitial = PlanOpen + EnUs + OnLoad + "\"compareOrdinal\",\"args\":[" + Initial + Left + "}," + Initial + Right + "}" + Ends;

    private const string ByStatus = PlanOpen + EnUs + OnLoad + "\"compareNumbers\",\"args\":[" + Status + Left + "}," + Status + Right + "}" + Ends;

    private const string ByDue = PlanOpen + EnUs + OnLoad + "\"compareOrdinal\",\"args\":[" + DueKey + Left + "}]}," + DueKey + Right + "}]}" + Ends;

    private const string ByDrugUnderTheCulture = PlanOpen + EnUs + OnLoad + "\"compareCulture\",\"args\":[" + Drug + Left + "}," + Drug + Right + "}" + Ends;

    private const string ByDrugUnderGerman = PlanOpen + DeDe + OnLoad + "\"compareCulture\",\"args\":[" + Drug + Left + "}," + Drug + Right + "}" + Ends;

    private const string ByDrugCodeUnits = PlanOpen + EnUs + OnLoad + "\"compareOrdinal\",\"args\":[" + Drug + Left + "}," + Drug + Right + "}" + Ends;

    private const string ByDrugCodeUnitsLastFirst = PlanOpen + EnUs + OnLoad + "\"compareOrdinal\",\"args\":[" + Drug + Right + "}," + Drug + Left + "}" + Ends;

    [Test]
    public void a_number_key_orders_as_numbers_each_item_read_by_its_own_parameter()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Units)), Is.EqualTo(ScriptOpen + ByUnits + ScriptClose));

    [Test]
    public void descending_compares_the_two_items_swapped_so_equal_keys_keep_their_order()
        => Assert.That(Rendered(cart => cart.Doses.OrderByDescending(d => d.Units)), Is.EqualTo(ScriptOpen + ByUnitsGreatestFirst + ScriptClose));

    [Test]
    public void a_nullable_number_key_orders_as_numbers_with_null_first()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Priority)), Is.EqualTo(ScriptOpen + ByPriority + ScriptClose));

    [Test]
    public void a_nullable_number_key_descending_compares_the_two_items_swapped_so_null_comes_last()
        => Assert.That(Rendered(cart => cart.Doses.OrderByDescending(d => d.Priority)), Is.EqualTo(ScriptOpen + ByPriorityGreatestFirst + ScriptClose));

    [Test]
    public void a_bool_key_orders_false_first()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Late)), Is.EqualTo(ScriptOpen + ByLateness + ScriptClose));

    [Test]
    public void a_char_key_orders_by_its_code_unit()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Initial)), Is.EqualTo(ScriptOpen + ByInitial + ScriptClose));

    [Test]
    public void an_enum_key_the_live_form_writes_as_a_number_orders_as_numbers()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Status)), Is.EqualTo(ScriptOpen + ByStatus + ScriptClose));

    [Test]
    public void a_date_key_orders_its_key_by_code_unit()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Due)), Is.EqualTo(ScriptOpen + ByDue + ScriptClose));

    [Test]
    public void a_text_key_with_no_comparer_orders_by_the_culture_the_view_renders_under()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Drug)), Is.EqualTo(ScriptOpen + ByDrugUnderTheCulture + ScriptClose));

    [Test]
    public void a_text_key_with_the_comparer_of_the_views_culture_orders_as_no_comparer_does()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Drug, StringComparer.CurrentCulture)), Is.EqualTo(ScriptOpen + ByDrugUnderTheCulture + ScriptClose));

    [Test]
    public void a_text_key_with_the_ordinal_comparer_orders_by_code_unit()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Drug, StringComparer.Ordinal)), Is.EqualTo(ScriptOpen + ByDrugCodeUnits + ScriptClose));

    [Test]
    public void a_text_key_with_the_ordinal_comparer_descending_compares_the_two_items_swapped()
        => Assert.That(Rendered(cart => cart.Doses.OrderByDescending(d => d.Drug, StringComparer.Ordinal)), Is.EqualTo(ScriptOpen + ByDrugCodeUnitsLastFirst + ScriptClose));

    [Test]
    [SetCulture("de-DE")]
    public void a_text_key_under_a_culture_with_no_alternate_sort_orders_by_that_culture()
        => Assert.That(Rendered(cart => cart.Doses.OrderBy(d => d.Drug)), Is.EqualTo(ScriptOpen + ByDrugUnderGerman + ScriptClose));

    [Test]
    [SetCulture("de-DE_phoneb")]
    public void a_text_key_under_a_culture_with_an_alternate_sort_is_refused_since_the_browser_has_no_tag_for_it()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Shown.Set(cart.Doses.OrderBy(d => d.Drug));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Drug' orders text by the alternate sort 'de-DE_phoneb', which the browser has no tag for; pass StringComparer.Ordinal, or order on the server."));
    }

    [Test]
    public void a_key_the_json_writes_as_text_is_refused_since_its_text_does_not_order_as_the_value_does()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Shown.Set(cart.Doses.OrderBy(d => d.Batch));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Batch' orders a value the app's JSON writes as text, which does not order as the value does in .NET; order by a member the JSON writes as a number, or order on the server."));
    }

    [Test]
    public void an_object_key_is_refused_since_the_browser_does_not_order_objects()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Shown.Set(OrderedBy(cart.Doses, d => d.Carer));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Carer' orders objects, which the browser does not order; order by a member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal)."));
    }

    [Test]
    public void a_comparer_the_browser_does_not_have_is_refused()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Shown.Set(OrderedBy(cart.Doses, d => d.Drug, StringComparer.OrdinalIgnoreCase));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'d.Drug' orders text by a comparer the browser does not have; pass StringComparer.Ordinal, or no comparer to order by the view's culture, or order on the server."));
    }

    [Test]
    public void the_plans_of_each_order_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(ByUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByUnitsGreatestFirst), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByPriority), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByPriorityGreatestFirst), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByLateness), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByInitial), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByStatus), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByDue), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByDrugUnderTheCulture), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByDrugUnderGerman), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByDrugCodeUnits), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ByDrugCodeUnitsLastFirst), Is.Empty);
    }

    private static TypedSource<IReadOnlyList<CartDose>> OrderedBy<TKey>(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, TKey>> key) => doses.OrderBy(key);

    private static TypedSource<IReadOnlyList<CartDose>> OrderedBy(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, string?>> key, StringComparer comparer)
        => doses.OrderBy(key, comparer);

    private static string Rendered(Func<DoseCart, TypedSource<IReadOnlyList<CartDose>>> ordered)
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var cart = pipeline.Component<DoseCart>("cart");
            cart.Shown.Set(ordered(cart));
        }));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
