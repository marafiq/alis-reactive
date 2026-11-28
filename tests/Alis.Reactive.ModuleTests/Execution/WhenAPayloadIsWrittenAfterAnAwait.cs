using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Execution;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAPayloadIsWrittenAfterAnAwait
{
    private const string AfterAnAwait = "A write to the event's payload after an awaited step reaches no vendor: write it before the first request, Confirm or async method.";
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string Opens =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Execution.AlarmDesk","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"panel":{"kind":"element","id":"panel","vendor":"native"}},"handlers":[""";
    private const string HearsRaised = """{"on":"panel","path":[],"event":"raised","payload":[0],"members":"required","policy":"overlap","do":[""";
    private const string OnRaised = Opens + HearsRaised;
    private const string OnHandedOver = Opens + """{"on":"panel","path":[],"event":"handedOver","payload":[0],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";
    private const string Settles = """{"kind":"do","await":true,"expr":{"kind":"call","path":["settle"],"members":"required","target":{"kind":"obj","id":"panel"},"args":[]}}""";
    private const string SettlesItself = """{"kind":"do","await":true,"expr":{"kind":"call","path":["settle"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}}""";
    private const string WritesTheNote = """{"kind":"set","path":["note"],"members":"required","content":false,"target":{"kind":"ref","name":"e"},"value":{"kind":"lit","value":"Kappa"}}""";
    private const string LogsWatching = """{"kind":"set","path":["entry"],"members":"required","content":false,"target":{"kind":"get","path":["log"],"members":"required","target":{"kind":"ref","name":"e"}},"value":{"kind":"lit","value":"Watching"}}""";
    private const string LogsKappa = """{"kind":"set","path":["entry"],"members":"required","content":false,"target":{"kind":"get","path":["log"],"members":"required","target":{"kind":"ref","name":"e"}},"value":{"kind":"lit","value":"Kappa"}}""";
    private const string LogsTheWard = """{"kind":"set","path":["entry"],"members":"required","content":false,"target":{"kind":"get","path":["log"],"members":"required","target":{"kind":"ref","name":"e"}},"value":{"kind":"get","path":["ward"],"members":"required","target":{"kind":"ref","name":"e"}}}""";
    private const string Acknowledges = """{"kind":"do","await":false,"expr":{"kind":"call","path":["acknowledge"],"members":"required","target":{"kind":"ref","name":"e"},"args":[]}}""";
    private const string Flagged = """{"kind":"get","path":["flag"],"members":"required","target":{"kind":"ref","name":"e"}}""";
    private const string CountedPastTwo = """{"kind":"op","op":"gt","args":[{"kind":"get","path":["count"],"members":"required","target":{"kind":"ref","name":"e"}},{"kind":"lit","value":2}]}""";

    private const string ElseBesideAnAwaitingThen = OnRaised + """{"kind":"if","when":""" + Flagged + ",\"then\":[" + Settles + "],\"else\":[" + WritesTheNote + "]}" + Ends;
    private const string ElseAfterAnElseIfThatAwaitsNothing = OnRaised + """{"kind":"if","when":""" + CountedPastTwo + ",\"then\":[" + Settles + "],\"else\":[" +
        """{"kind":"if","when":""" + Flagged + ",\"then\":[" + LogsWatching + "],\"else\":[" + WritesTheNote + "]}]}" + Ends;
    private const string WriteAfterAWhenThatAwaitsNothing = OnRaised + """{"kind":"if","when":""" + Flagged + ",\"then\":[" + LogsWatching + "],\"else\":[]}," + WritesTheNote + Ends;
    private const string WriteBeforeTheAwait = OnRaised + WritesTheNote + "," + Settles + Ends;
    private const string HeldObjectAfterTheAwait = OnRaised + Settles + "," + LogsKappa + Ends;
    private const string ComponentAfterTheAwait = OnHandedOver + SettlesItself + "," + WritesTheNote + Ends;
    private const string ReadAndCallAfterTheAwait = OnRaised + Settles + "," + Acknowledges + "," + LogsTheWard + Ends;
    private const string NextHandlerBeforeItsAwait = OnRaised + Settles + "]}," + HearsRaised + WritesTheNote + Ends;

    [Test]
    public void a_write_after_an_awaited_result_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            var note = pipeline.Component<AlarmPanel>("panel").SettleNote();
            Noted(raised, note);
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_after_a_when_whose_then_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => pipeline.Component<AlarmPanel>("panel").Settle());
            Noted(raised);
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_after_a_when_whose_then_awaits_beside_an_else_that_does_not_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => pipeline.Component<AlarmPanel>("panel").Settle())
                .Else(pipeline => raised.Log.Entry.Set("Watching"));
            Noted(raised);
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_after_a_when_whose_else_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => raised.Log.Entry.Set("Watching"))
                .Else(pipeline => pipeline.Component<AlarmPanel>("panel").Settle());
            Noted(raised);
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_after_a_when_whose_elseif_term_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => raised.Log.Entry.Set("Watching"))
                .ElseIf(pipeline.Component<AlarmPanel>("panel").SettleCheck()).Then(pipeline => raised.Log.Entry.Set("Settled"));
            Noted(raised);
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_in_the_then_of_a_when_whose_term_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(pipeline.Component<AlarmPanel>("panel").SettleCheck()).Then(pipeline => Noted(raised));
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_in_the_then_of_an_elseif_whose_term_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => raised.Log.Entry.Set("Watching"))
                .ElseIf(pipeline.Component<AlarmPanel>("panel").SettleCheck()).Then(pipeline => Noted(raised));
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void a_write_in_an_else_after_an_elseif_whose_term_awaits_is_refused()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        Assert.That(() => plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Count.Gt(2)).Then(pipeline => pipeline.Component<AlarmPanel>("panel").Settle())
                .ElseIf(pipeline.Component<AlarmPanel>("panel").SettleCheck()).Then(pipeline => raised.Log.Entry.Set("Watching"))
                .Else(pipeline => Noted(raised));
        }), Throws.InvalidOperationException.With.Message.EqualTo(AfterAnAwait));
        html.RenderPlan(plan);
    }

    [Test]
    public void an_else_beside_a_then_that_awaits_writes_the_payload()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => pipeline.Component<AlarmPanel>("panel").Settle())
                .Else(pipeline => raised.Note.Set("Kappa"));
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ElseBesideAnAwaitingThen + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ElseBesideAnAwaitingThen), Is.Empty);
    }

    [Test]
    public void an_else_after_an_elseif_whose_term_awaits_nothing_writes_the_payload()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Count.Gt(2)).Then(pipeline => pipeline.Component<AlarmPanel>("panel").Settle())
                .ElseIf(raised.Flag).Then(pipeline => raised.Log.Entry.Set("Watching"))
                .Else(pipeline => raised.Note.Set("Kappa"));
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ElseAfterAnElseIfThatAwaitsNothing + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ElseAfterAnElseIfThatAwaitsNothing), Is.Empty);
    }

    [Test]
    public void a_write_after_a_when_that_awaits_nothing_writes_the_payload()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.When(raised.Flag).Then(pipeline => raised.Log.Entry.Set("Watching"));
            raised.Note.Set("Kappa");
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + WriteAfterAWhenThatAwaitsNothing + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(WriteAfterAWhenThatAwaitsNothing), Is.Empty);
    }

    [Test]
    public void a_write_before_the_first_await_writes_the_payload()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            raised.Note.Set("Kappa");
            pipeline.Component<AlarmPanel>("panel").Settle();
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + WriteBeforeTheAwait + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(WriteBeforeTheAwait), Is.Empty);
    }

    [Test]
    public void a_write_to_an_object_the_payload_holds_runs_after_the_await()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.Component<AlarmPanel>("panel").Settle();
            raised.Log.Entry.Set("Kappa");
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + HeldObjectAfterTheAwait + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(HeldObjectAfterTheAwait), Is.Empty);
    }

    [Test]
    public void a_payload_that_is_a_component_takes_a_write_after_the_await()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.HandedOver, (handed, pipeline) =>
        {
            handed.Settle();
            handed.Note.Set("Kappa");
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ComponentAfterTheAwait + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ComponentAfterTheAwait), Is.Empty);
    }

    [Test]
    public void a_read_and_a_call_of_the_payload_run_after_the_await()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty).Reactive(panel => panel.Raised, (raised, pipeline) =>
        {
            pipeline.Component<AlarmPanel>("panel").Settle();
            raised.Acknowledge();
            raised.Log.Entry.Set(raised.Ward);
        });
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadAndCallAfterTheAwait + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadAndCallAfterTheAwait), Is.Empty);
    }

    [Test]
    public void each_handler_writes_its_payload_before_its_own_first_await()
    {
        var html = Views.Of(new AlarmDesk("North"));
        var plan = html.ReactivePlan<AlarmDesk>();
        plan.Render<AlarmPanel>("panel", HtmlString.Empty)
            .Reactive(panel => panel.Raised, (raised, pipeline) => pipeline.Component<AlarmPanel>("panel").Settle())
            .Reactive(panel => panel.Raised, (raised, pipeline) => raised.Note.Set("Kappa"));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + NextHandlerBeforeItsAwait + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(NextHandlerBeforeItsAwait), Is.Empty);
    }

    private static void Noted(AlarmRaised raised) => raised.Note.Set("Kappa");

    private static void Noted(AlarmRaised raised, MethodResult<string> note) => raised.Note.Set(note);
}
