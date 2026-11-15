using System;
using Alis.Reactive.ArrayLambdas;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Conditions;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAValueIsLookedUpInAListThePageHolds
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

    private const string Item = "{\"kind\":\"ref\",\"name\":\"item1\"}";

    private const string Status = "{\"kind\":\"get\",\"path\":[\"status\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string Allowed = "{\"kind\":\"get\",\"path\":[\"allowed\"],\"members\":\"required\",\"target\":" + Cart + "}";

    private const string StatusIsRead = "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Status + "," + Status + "]}";

    private const string StatusInAllowed =
        "{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + StatusIsRead + "," +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" + Allowed + ",\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + Item + "," + Status + "]}}]}," +
        StatusIsRead + "]}";

    private const string DueKey = "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[{\"kind\":\"get\",\"path\":[\"due\"],\"members\":\"required\",\"target\":" + Cart + "}]}";

    private const string DueIsRead = "{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" + DueKey + "," + DueKey + "]}";

    private const string AllowedStatus = OnLoad + SetsFlagged + StatusInAllowed + "}" + Ends;

    private const string NoAllowedStatus = OnLoad + SetsFlagged + "{\"kind\":\"op\",\"op\":\"not\",\"args\":[" + StatusInAllowed + "]}}" + Ends;

    private const string DueOnARoundDay = OnLoad + SetsFlagged +
        "{\"kind\":\"op\",\"op\":\"cond\",\"args\":[" + DueIsRead + "," +
        "{\"kind\":\"call\",\"path\":[\"some\"],\"members\":\"requiredIfHeld\",\"target\":" +
        "{\"kind\":\"get\",\"path\":[\"roundDays\"],\"members\":\"required\",\"target\":" + Cart + "},\"args\":[" +
        "{\"kind\":\"fn\",\"params\":[\"item1\"],\"body\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"op\",\"op\":\"dateKey\",\"args\":[" + Item + "]}," + DueKey + "]}}]}," +
        DueIsRead + "]}}" + Ends;

    [Test]
    public void a_status_in_the_list_a_component_holds_is_any_item_equal_to_it_and_reads_nothing_when_it_reads_nothing()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Status.In(cart.Allowed))), Is.EqualTo(ScriptOpen + AllowedStatus + ScriptClose));

    [Test]
    public void a_status_not_in_the_list_a_component_holds_is_the_opposite_of_in()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Status.NotIn(cart.Allowed))), Is.EqualTo(ScriptOpen + NoAllowedStatus + ScriptClose));

    [Test]
    public void a_date_in_a_list_of_dates_is_compared_by_keys()
        => Assert.That(Rendered(cart => cart.Flagged.Set(cart.Due.In(cart.RoundDays))), Is.EqualTo(ScriptOpen + DueOnARoundDay + ScriptClose));

    [Test]
    public void a_value_in_its_own_form_is_not_looked_up_in_a_list_the_page_holds()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Doses.Find(d => d.Drug == "Digoxin", d => d.Charted).In(cart.Allowed))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares two values, and its own JSON converter writes 'd.Charted' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));

    [Test]
    public void a_value_is_not_looked_up_in_a_list_whose_items_are_in_their_own_form()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Status.In(cart.Doses.Select(d => d.Charted)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares two values, and its own JSON converter writes 'd.Charted' unlike JavaScript's value, so the browser does not compare them as .NET does; compare them on the server."));

    [Test]
    public void an_object_is_not_looked_up_in_a_list_of_objects()
        => Assert.That(
            () => Rendered(cart => cart.Flagged.Set(cart.Seal.In(cart.Seals))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A condition compares objects, and the browser compares objects by reference; compare a member of each."));

    [Test]
    public void the_plans_of_each_lookup_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(AllowedStatus), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(NoAllowedStatus), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(DueOnARoundDay), Is.Empty);
    }

    private static string Rendered(Action<DoseCart> step)
    {
        var html = Views.Of(new DoseRound("North"));
        var plan = html.ReactivePlan<DoseRound>();
        html.On(plan, trigger => trigger.DomReady(pipeline => step(pipeline.Component<DoseCart>("cart"))));
        return Views.TextOf(html.RenderPlan(plan));
    }
}
