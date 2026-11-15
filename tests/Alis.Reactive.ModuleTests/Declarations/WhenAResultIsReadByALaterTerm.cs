using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAResultIsReadByALaterTerm
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";
    private const string AtRead = """{"kind":"let","name":"result1","await":false,"binds":"read","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}""";
    private const string AtLine = """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}""";
    private const string Flow = """{"kind":"op","op":"eq","args":[{"kind":"get","path":["flowRate"],"members":"required","target":{"kind":"obj","id":"pump"}},{"kind":"lit","value":0}]}""";
    private const string Total = """{"kind":"op","op":"eq","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":100}]}""";
    private const string Branch = """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}}""";
    private const string ReadsTotal = """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""";
    private const string Either = """{"kind":"op","op":"or","args":[""" + Flow + "," + Total + "]}";
    private const string Both = """{"kind":"op","op":"and","args":[""" + Flow + "," + Total + "]}";
    private const string OrPlan = OnLoad + AtRead + "," + """{"kind":"if","when":""" + Either + ""","then":[""" + Branch + "],\"else\":[]}" + Ends;
    private const string AndPlan = OnLoad + AtRead + "," + """{"kind":"if","when":""" + Both + ""","then":[""" + Branch + "],\"else\":[]}" + Ends;
    private const string FallbackPlan = OnLoad + AtRead + "," +
        """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"op","op":"coalesce","args":[{"kind":"get","path":["limit"],"members":"required","target":{"kind":"obj","id":"pump"}},{"kind":"ref","name":"result1"}]}}""" + Ends;
    private const string TrueChoicePlan = OnLoad + AtRead + "," +
        """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"op","op":"cond","args":[""" + Flow + """,{"kind":"ref","name":"result1"},{"kind":"lit","value":10}]}}""" + Ends;
    private const string FalseChoicePlan = OnLoad + AtRead + "," +
        """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"op","op":"cond","args":[""" + Flow + """,{"kind":"lit","value":10},{"kind":"ref","name":"result1"}]}}""" + Ends;
    private const string AfterAWrite = OnLoad + AtLine + "," + Branch + "," + """{"kind":"if","when":""" + Either + ",\"then\":[" + Branch + "],\"else\":[]}" + Ends;
    private const string ReadByTheBranch = OnLoad + AtLine + "," + """{"kind":"if","when":""" + Either + ",\"then\":[" + Branch + "," + ReadsTotal + "],\"else\":[]}" + Ends;
    private const string ReadByALaterStep = OnLoad + AtLine + "," + """{"kind":"if","when":""" + Either + ",\"then\":[" + Branch + "],\"else\":[]}," + ReadsTotal + Ends;
    private const string ReadOnlyInTheBranch = OnLoad + AtLine + "," + """{"kind":"if","when":""" + Flow + ",\"then\":[" + ReadsTotal + "],\"else\":[]}" + Ends;
    private const string ReadUnderALaterTermInTheBranch = OnLoad + AtLine + "," + """{"kind":"if","when":""" + Flow + ",\"then\":[{\"kind\":\"if\",\"when\":" + Either + ",\"then\":[" + Branch + "],\"else\":[]}],\"else\":[]}" + Ends;
    private const string EchoPlan = OnLoad + AtRead + "," +
        """{"kind":"let","name":"result2","await":false,"binds":"read","expr":{"kind":"call","path":["echo"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"ref","name":"result1"}]}},{"kind":"if","when":{"kind":"op","op":"or","args":[""" + Flow +
        """,{"kind":"op","op":"eq","args":[{"kind":"ref","name":"result2"},{"kind":"lit","value":100}]}]},"then":[""" + Branch + "],\"else\":[]}" + Ends;
    private const string FirstTermPlan = OnLoad + AtLine + "," + """{"kind":"if","when":{"kind":"op","op":"or","args":[""" + Total + "," + Flow + "]},\"then\":[" + Branch + "],\"else\":[]}" + Ends;
    private const string DeepTermPlan = OnLoad + AtRead + "," +
        """{"kind":"if","when":{"kind":"op","op":"or","args":[""" + Flow + """,{"kind":"op","op":"not","args":[{"kind":"op","op":"and","args":[""" + Total + "," + Flow + "]}]}]},\"then\":[" + Branch + "],\"else\":[]}" + Ends;

    [Test]
    public void or_calls_the_result_only_when_its_right_term_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).Or(pump.Total().Eq(100))).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + OrPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(OrPlan), Is.Empty);
    }

    [Test]
    public void and_calls_the_result_only_when_its_right_term_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).And(pump.Total().Eq(100))).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + AndPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(AndPlan), Is.Empty);
    }

    [Test]
    public void if_null_calls_the_result_only_when_its_fallback_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Limit.IfNull(pump.Total()));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + FallbackPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(FallbackPlan), Is.Empty);
    }

    [Test]
    public void choose_calls_the_result_only_when_its_true_branch_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.FlowRate.Eq(0).Choose(pump.Total(), 10));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + TrueChoicePlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(TrueChoicePlan), Is.Empty);
    }

    [Test]
    public void choose_calls_the_result_only_when_its_false_branch_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.FlowRate.Eq(0).Choose(10, pump.Total()));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + FalseChoicePlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(FalseChoicePlan), Is.Empty);
    }

    [Test]
    public void a_step_between_the_call_and_the_term_keeps_the_value_at_the_call()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pump.Rate.Set(999);
            pipeline.When(pump.FlowRate.Eq(0).Or(total.Eq(100))).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + AfterAWrite + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(AfterAWrite), Is.Empty);
    }

    [Test]
    public void a_term_and_its_branch_read_the_same_value_from_the_call()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pipeline.When(pump.FlowRate.Eq(0).Or(total.Eq(100))).Then(pipeline =>
            {
                pump.Rate.Set(999);
                pump.Rate.Set(total);
            });
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadByTheBranch + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadByTheBranch), Is.Empty);
    }

    [Test]
    public void a_later_step_reads_the_same_value_as_the_term()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pipeline.When(pump.FlowRate.Eq(0).Or(total.Eq(100))).Then(pipeline => pump.Rate.Set(999));
            pump.Rate.Set(total);
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadByALaterStep + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadByALaterStep), Is.Empty);
    }

    [Test]
    public void a_read_only_in_a_branch_keeps_the_call_before_the_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(total));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadOnlyInTheBranch + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadOnlyInTheBranch), Is.Empty);
    }

    [Test]
    public void a_later_term_inside_a_branch_keeps_the_call_before_the_branch()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var total = pump.Total();
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline =>
                pipeline.When(pump.FlowRate.Eq(0).Or(total.Eq(100))).Then(pipeline => pump.Rate.Set(999)));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadUnderALaterTermInTheBranch + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadUnderALaterTermInTheBranch), Is.Empty);
    }

    [Test]
    public void a_deferred_calls_argument_is_called_when_that_call_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).Or(pump.Echo(pump.Total()).Eq(100))).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + EchoPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(EchoPlan), Is.Empty);
    }

    [Test]
    public void the_first_term_keeps_the_call_at_its_line()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.Total().Eq(100).Or(pump.FlowRate.Eq(0))).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + FirstTermPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(FirstTermPlan), Is.Empty);
    }

    [Test]
    public void a_read_deep_inside_a_later_term_runs_where_that_term_is_reached()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).Or(pump.Total().Eq(100).And(pump.FlowRate.Eq(0)).Not())).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + DeepTermPlan + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(DeepTermPlan), Is.Empty);
    }

    [Test]
    public void an_async_result_cannot_wait_inside_a_later_term()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0).Or(pump.TotalAsync().Eq(100))).Then(pipeline => pump.Rate.Set(999));
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An async method is called in a term that runs only when it is reached (the right term of And, Or or IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached."));
    }
}
