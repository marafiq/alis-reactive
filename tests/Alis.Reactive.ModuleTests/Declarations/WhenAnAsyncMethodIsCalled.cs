using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnAsyncMethodIsCalled
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    private const string Flushes = OnLoad +
        """{"kind":"do","await":true,"expr":{"kind":"call","path":["flushAsync"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}""" + Ends;
    private const string Delivers = OnLoad +
        """{"kind":"do","await":true,"expr":{"kind":"call","path":["deliver-async"],"members":"optionalMember","target":{"kind":"obj","id":"pump"},"args":[{"kind":"lit","value":250}]}}""" + Ends;
    private const string Totals = OnLoad +
        """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["totalAsync"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;
    private const string Estimates = OnLoad +
        """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["estimate-async"],"members":"optionalMember","target":{"kind":"obj","id":"pump"},"args":[{"kind":"lit","value":250}]}},{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""" + Ends;

    [Test]
    public void a_method_without_arguments_is_awaited()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.FlushAsync();
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Flushes + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Flushes), Is.Empty);
    }

    [Test]
    public void a_method_with_arguments_is_awaited_at_its_named_optional_member()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.DeliverAsync(250);
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Delivers + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Delivers), Is.Empty);
    }

    [Test]
    public void a_result_without_arguments_is_awaited_before_it_is_read()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.TotalAsync());
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Totals + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Totals), Is.Empty);
    }

    [Test]
    public void a_result_with_arguments_is_awaited_before_it_is_read()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.EstimateAsync(250));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + Estimates + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Estimates), Is.Empty);
    }
}
