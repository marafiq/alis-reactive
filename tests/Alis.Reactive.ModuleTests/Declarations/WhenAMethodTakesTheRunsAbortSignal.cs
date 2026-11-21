using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAMethodTakesTheRunsAbortSignal
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    private const string Asks = OnLoad +
        """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["ask"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"lit","value":"Silence the alarm?"},{"kind":"signal"}]}},{"kind":"set","path":["label"],"members":"optional","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;

    private const string Purges = OnLoad +
        """{"kind":"do","await":false,"expr":{"kind":"call","path":["purge"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"signal"},{"kind":"lit","value":5}]}}""" + Ends;

    [Test]
    public void an_awaited_method_receives_the_abort_signal_at_the_position_its_declaration_lists_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.AskToSilence("Silence the alarm?"));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Asks + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Asks), Is.Empty);
    }

    [Test]
    public void a_method_receives_the_abort_signal_before_the_argument_the_view_passes()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Purge(5)));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Purges + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Purges), Is.Empty);
    }
}
