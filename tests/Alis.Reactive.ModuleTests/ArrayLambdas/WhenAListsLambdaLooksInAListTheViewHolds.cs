using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ArrayLambdas;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAListsLambdaLooksInAListTheViewHolds
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

    private const string ItsWard = "{\"kind\":\"data\",\"path\":[{\"name\":\"ward\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"none\"}}],\"target\":" + Item + "}";

    private const string ItsStatus = "{\"kind\":\"data\",\"path\":[{\"name\":\"status\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string ItsCopay = "{\"kind\":\"data\",\"path\":[{\"name\":\"copay\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":0}}],\"target\":" + Item + "}";

    private const string ItsSeenKey = "{\"kind\":\"op\",\"op\":\"instantKey\",\"args\":[" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"seen\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0001-01-01T00:00:00\\u002B00:00\"}}],\"target\":" + Item + "}]}";

    private const string InNorthOrSouth = AnyDose + "{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[\"North\",\"South\"]}," + ItsWard + "]}" + Ends;

    private const string InNorth = AnyDose + "{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[\"North\"]}," + ItsWard + "]}" + Ends;

    private const string DueOrGiven = AnyDose + "{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[1,2]}," + ItsStatus + "]}" + Ends;

    private const string ATenthOrAQuarter = AnyDose + "{\"kind\":\"op\",\"op\":\"includes\",\"args\":[{\"kind\":\"lit\",\"value\":[0.10,0.25]}," + ItsCopay + "]}" + Ends;

    private const string SeenAtEitherInstant = AnyDose +
        "{\"kind\":\"op\",\"op\":\"or\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsSeenKey + ",{\"kind\":\"op\",\"op\":\"instantKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-11-09T18:00:00\\u002B00:00\"}]}]}," +
        "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + ItsSeenKey + ",{\"kind\":\"op\",\"op\":\"instantKey\",\"args\":[{\"kind\":\"lit\",\"value\":\"2026-11-09T13:30:00-05:00\"}]}]}]}" + Ends;

    private const string SeenAtNoInstant = AnyDose + "{\"kind\":\"op\",\"op\":\"neq\",\"args\":[" + ItsSeenKey + "," + ItsSeenKey + "]}" + Ends;

    [Test]
    public void an_array_the_view_holds_is_looked_in_for_the_items_value()
    {
        var wards = new[] { "North", "South" };

        Assert.That(Rendered(d => wards.Contains(d.Ward)), Is.EqualTo(ScriptOpen + InNorthOrSouth + ScriptClose));
    }

    [Test]
    public void a_list_the_view_holds_is_looked_in_for_the_items_value()
    {
        var wards = new List<string> { "North" };

        Assert.That(Rendered(d => wards.Contains(d.Ward)), Is.EqualTo(ScriptOpen + InNorth + ScriptClose));
    }

    [Test]
    public void an_array_typed_as_a_sequence_is_looked_in_for_the_items_value()
    {
        IEnumerable<string> wards = new[] { "North" };

        Assert.That(Rendered(d => wards.Contains(d.Ward)), Is.EqualTo(ScriptOpen + InNorth + ScriptClose));
    }

    [Test]
    public void an_enum_array_is_written_in_the_form_the_list_holds_the_member_it_is_looked_in_for()
    {
        var statuses = new[] { DoseStatus.Due, DoseStatus.Given };

        Assert.That(Rendered(d => statuses.Contains(d.Status)), Is.EqualTo(ScriptOpen + DueOrGiven + ScriptClose));
    }

    [Test]
    public void a_fraction_the_apps_json_writes_as_a_number_is_looked_up_by_its_number()
    {
        var copays = new[] { 0.10m, 0.25m };

        Assert.That(Rendered(d => copays.Contains(d.Copay)), Is.EqualTo(ScriptOpen + ATenthOrAQuarter + ScriptClose));
    }

    [Test]
    public void a_date_is_looked_up_by_its_key_one_equality_a_value()
    {
        var seen = new[] { new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 11, 9, 13, 30, 0, TimeSpan.FromHours(-5)) };

        Assert.That(Rendered(d => seen.Contains(d.Seen)), Is.EqualTo(ScriptOpen + SeenAtEitherInstant + ScriptClose));
    }

    [Test]
    public void a_date_looked_up_in_an_empty_array_holds_on_no_item_and_reads_nothing_where_its_date_does()
    {
        var seen = Array.Empty<DateTimeOffset>();

        Assert.That(Rendered(d => seen.Contains(d.Seen)), Is.EqualTo(ScriptOpen + SeenAtNoInstant + ScriptClose));
    }

    [Test]
    public void a_list_that_is_null_when_the_view_renders_is_refused()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        string[]? none = null;

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(cart.Doses.Any(d => none!.Contains(d.Ward)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo("'none.Contains(d.Ward)' looks in a list that is null when the view renders."));
    }

    [Test]
    public void a_set_is_refused_since_it_compares_by_its_own_comparer()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var wards = new HashSet<string> { "North" };

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => wards.Contains(d.Ward)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'wards.Contains(d.Ward)' looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();."));
    }

    [Test]
    public void a_collection_typed_as_a_sequence_that_is_no_array_is_refused_since_it_can_compare_by_its_own_rules()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        IEnumerable<string> wards = new List<string> { "North" };

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => wards.Contains(d.Ward)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'wards.Contains(d.Ward)' looks in a collection that can compare by its own rules, as a HashSet's comparer does; copy it into an array before the lambda, such as var wards = Model.Wards.ToArray();."));
    }

    [Test]
    public void a_list_the_item_holds_is_refused_naming_any()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => d.Hours.Contains(8)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo("'d.Hours.Contains(8)' looks for a value in a list the item holds; use Any, such as d.Hours.Any(hour => hour == 8)."));
    }

    [Test]
    public void a_list_of_objects_is_refused_since_the_browser_compares_objects_by_reference()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var leads = new[] { new Carer() };

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => leads.Contains(d.Carer)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'leads.Contains(d.Carer)' looks for an object, and the browser compares objects by reference; look for a member, such as names.Contains(d.Carer.Name)."));
    }

    [Test]
    public void a_lookup_given_an_equality_comparer_is_refused()
    {
        var html = Views.Of(new MedicationRound("North", null));
        var plan = html.ReactivePlan<MedicationRound>();
        var wards = new[] { "North" };

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var cart = pipeline.Component<DoseCart>("cart");
                cart.Flagged.Set(AnyOf(cart.Doses, d => wards.Contains(d.Ward, StringComparer.OrdinalIgnoreCase)));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'wards.Contains(d.Ward, StringComparer.OrdinalIgnoreCase)' passes an equality comparer, which the browser does not have; look up without it, or move the work to the server."));
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

    private static TypedSource<bool> AnyOf(ITypedSource<IEnumerable<CartDose>> doses, Expression<Func<CartDose, bool>> predicate) => doses.Any(predicate);
}
