using System;
using Alis.Reactive.Conditions;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnElseIfCallsAMethod
{
    private MethodResult<int> _carried = null!;
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";
    private const string OnLoad =
        """{"version":7,"planId":"Alis.Reactive.ModuleTests.Declarations.Infusion","culture":{"name":"en-US","collation":"en-US","decimal":".","group":",","integer":{"group":false,"exponent":false},"fraction":{"group":true,"exponent":true}},"timeZone":{"kind":"clock"},"trace":{"kind":"off"},"vendors":{"native":{"locate":{"kind":"fn","params":["element"],"body":{"kind":"ref","name":"element"}},"listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"}}},"objects":{"$document":{"kind":"plugin","name":"alis.document","key":"","listen":{"kind":"method","subscribe":"addEventListener","unsubscribe":"removeEventListener"},"data":{"kind":"none"}},"pump":{"kind":"element","id":"pump","vendor":"native"}},"handlers":[{"on":"$document","path":[],"event":"ready","payload":[],"members":"required","policy":"overlap","do":[""";
    private const string Ends = "]}]}";

    private const string Flow = """{"kind":"op","op":"eq","args":[{"kind":"get","path":["flowRate"],"members":"required","target":{"kind":"obj","id":"pump"}},{"kind":"lit","value":0}]}""";
    private const string Total = """{"kind":"op","op":"eq","args":[{"kind":"ref","name":"result1"},{"kind":"lit","value":100}]}""";
    private const string TotalAgain = """{"kind":"op","op":"eq","args":[{"kind":"ref","name":"result2"},{"kind":"lit","value":200}]}""";
    private const string CallsTotal = """{"kind":"let","name":"result1","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}""";
    private const string CallsTotalAgain = """{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["total"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}}""";
    private const string Branch = """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"lit","value":999}}""";
    private const string ReadsTotal = """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result1"}}""";
    private const string FirstIf = """{"kind":"if","when":""" + Flow + ",\"then\":[" + Branch + "],\"else\":[";
    private const string TwoConditions = OnLoad + FirstIf + CallsTotal + "," + """{"kind":"if","when":""" + Total + ",\"then\":[" + Branch + "],\"else\":[" + CallsTotalAgain + "," + """{"kind":"if","when":""" + TotalAgain + ",\"then\":[" + Branch + "],\"else\":[]}]}]}" + Ends;
    private const string ReadsWithinTheElse = OnLoad + FirstIf + CallsTotal + "," + """{"kind":"if","when":""" + Total + ",\"then\":[" + ReadsTotal + "],\"else\":[{\"kind\":\"if\",\"when\":" + Total + ",\"then\":[" + ReadsTotal + "],\"else\":[" + ReadsTotal + "]}]}]}" + Ends;
    private const string CallsEcho = """{"kind":"let","name":"result2","await":false,"binds":"line","expr":{"kind":"call","path":["echo"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[{"kind":"ref","name":"result1"}]}}""";
    private const string EchoCondition = OnLoad + FirstIf + CallsTotal + "," + CallsEcho + "," + """{"kind":"if","when":""" + TotalAgain + ",\"then\":[" + Branch + "],\"else\":[]}]}" + Ends;
    private const string AsyncCondition = OnLoad + FirstIf +
        """{"kind":"let","name":"result1","await":true,"binds":"line","expr":{"kind":"call","path":["totalAsync"],"members":"required","target":{"kind":"obj","id":"pump"},"args":[]}},{"kind":"if","when":""" + Total + ",\"then\":[" + Branch + "],\"else\":[]}]}" + Ends;
    private const string UnmovedSuffix = OnLoad + FirstIf + "]}," + CallsTotal + "," + CallsTotalAgain + "," +
        """{"kind":"set","path":["rate"],"members":"required","content":false,"target":{"kind":"obj","id":"pump"},"value":{"kind":"ref","name":"result2"}}""" + Ends;

    [Test]
    public void each_conditions_call_stays_in_the_else_that_reaches_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999))
                .ElseIf(pump.Total().Eq(100)).Then(pipeline => pump.Rate.Set(999))
                .ElseIf(pump.Total().Eq(200)).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + TwoConditions + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(TwoConditions), Is.Empty);
    }

    [Test]
    public void the_condition_result_is_readable_in_its_branch_and_every_later_arm()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var branches = pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999));
            _carried = pump.Total();
            branches.ElseIf(_carried.Eq(100)).Then(pipeline => pump.Rate.Set(_carried))
                .ElseIf(_carried.Eq(100)).Then(pipeline => pump.Rate.Set(_carried))
                .Else(pipeline => pump.Rate.Set(_carried));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + ReadsWithinTheElse + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadsWithinTheElse), Is.Empty);
    }

    [Test]
    public void a_conditions_argument_calls_move_with_it_in_their_order()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999))
                .ElseIf(pump.Echo(pump.Total()).Eq(200)).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + EchoCondition + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(EchoCondition), Is.Empty);
    }

    [Test]
    public void an_async_condition_waits_at_its_line_in_the_else_that_reaches_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999))
                .ElseIf(pump.TotalAsync().Eq(100)).Then(pipeline => pump.Rate.Set(999));
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + AsyncCondition + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(AsyncCondition), Is.Empty);
    }

    [Test]
    public void an_unread_result_refuses_the_whole_suffix_before_any_of_it_moves()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var branches = pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999));
            pump.Total();
            _carried = pump.Total();
            Assert.That(() => branches.ElseIf(_carried.Eq(200)).Then(pipeline =>
                throw new InvalidOperationException("The branch ran before its suffix was checked.")),
                Throws.InvalidOperationException.With.Message.EqualTo(
                    "An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence."));
            pump.Rate.Set(_carried);
        }));
        Assert.That(Views.TextOf(html.RenderPlan(plan)), Is.EqualTo(ScriptOpen + UnmovedSuffix + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(UnmovedSuffix), Is.Empty);
    }

    [Test]
    public void a_result_from_the_condition_cannot_be_read_after_the_when()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var branches = pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999));
            _carried = pump.Total();
            branches.ElseIf(_carried.Eq(100)).Then(pipeline => pump.Rate.Set(999));
            pump.Rate.Set(_carried);
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "A method's result called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call the method before the When to read its result after it."));
    }

    [Test]
    public void an_else_does_not_take_a_result_suffix()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var branches = pipeline.When(pump.FlowRate.Eq(0)).Then(pipeline => pump.Rate.Set(999));
            _carried = pump.Total();
            branches.Else(pipeline => pump.Rate.Set(_carried));
        }));
            html.RenderPlan(plan);
        };
        Assert.That(() => theView(), Throws.InvalidOperationException.With.Message.EqualTo(
            "An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence."));
    }
}
