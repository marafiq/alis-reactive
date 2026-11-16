using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenADateIsReadFromALiveObject
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string Opens =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Declarations.Infusion\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}},";

    private const string OnAlarmed = Opens +
        "\"objects\":{\"pump\":{\"kind\":\"element\",\"id\":\"pump\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"pump\",\"path\":[],\"event\":\"alarmed\",\"payload\":[0],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string OnLoadWithStandby = Opens +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"pump\":{\"kind\":\"element\",\"id\":\"pump\",\"vendor\":\"native\"}," +
        "\"standby\":{\"kind\":\"element\",\"id\":\"standby\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string Ends = "]}]}";

    private const string Pump = "{\"kind\":\"obj\",\"id\":\"pump\"}";

    private const string PayloadsDateAndInstant = OnAlarmed +
        "{\"kind\":\"set\",\"path\":[\"startsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"get\",\"path\":[\"raisedAt\"],\"members\":\"required\",\"target\":{\"kind\":\"ref\",\"name\":\"e\"}}," +
        "\"shape\":{\"kind\":\"date\"}}}," +
        "{\"kind\":\"set\",\"path\":[\"pausedUntil\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"get\",\"path\":[\"clearedAt\"],\"members\":\"required\",\"target\":{\"kind\":\"ref\",\"name\":\"e\"}}," +
        "\"shape\":{\"kind\":\"instant\",\"from\":\"instant\"}}}" + Ends;

    private const string ArgumentsDate = OnAlarmed +
        "{\"kind\":\"set\",\"path\":[\"stopsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"get\",\"path\":[1],\"members\":\"required\",\"target\":{\"kind\":\"ref\",\"name\":\"arguments\"}}," +
        "\"shape\":{\"kind\":\"nullable\",\"inner\":{\"kind\":\"date\"}}}}" + Ends;

    private const string ComponentsDateAsHeld = OnLoadWithStandby +
        "{\"kind\":\"set\",\"path\":[\"startsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + "," +
        "\"value\":{\"kind\":\"get\",\"path\":[\"startsAt\"],\"members\":\"required\",\"target\":{\"kind\":\"obj\",\"id\":\"standby\"}}}" + Ends;

    [Test]
    public void a_date_and_an_instant_the_payload_of_an_event_holds_are_read_as_their_types_from_text_too()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        plan.Render<InfusionPump>("pump", HtmlString.Empty).Reactive(infusion => infusion.Alarmed, (alarm, pipeline) =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.StartsAt.Set(alarm.RaisedAt);
            pump.PausedUntil.Set(alarm.ClearedAt);
        });

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + PayloadsDateAndInstant + ScriptClose));
    }

    [Test]
    public void a_date_the_listener_receives_as_an_argument_is_read_as_a_date_from_text_too()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        plan.Render<InfusionPump>("pump", HtmlString.Empty).Reactive(infusion => infusion.Alarmed, (alarm, pipeline) =>
            pipeline.Component<InfusionPump>("pump").StopsAt.Set(alarm.SilencedAt));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ArgumentsDate + ScriptClose));
    }

    [Test]
    public void a_date_a_components_own_property_holds_is_set_as_it_is_held()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var standby = pipeline.Component<InfusionPump>("standby");
            pump.StartsAt.Set(standby.StartsAt);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ComponentsDateAsHeld + ScriptClose));
    }

    [Test]
    public void the_plans_of_each_read_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(PayloadsDateAndInstant), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ArgumentsDate), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ComponentsDateAsHeld), Is.Empty);
    }
}
