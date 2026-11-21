using System;
using System.Text.Json;
using Alis.Reactive.Conditions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenADeclaredMemberIsRead
{
    private const string ScriptOpen = "<script type=\"application/json\" data-reactive-plan>";
    private const string ScriptClose = "</script>";

    private const string OnLoad =
        "{\"version\":7,\"planId\":\"Alis.Reactive.ModuleTests.Declarations.Infusion\"," +
        "\"culture\":{\"name\":\"en-US\",\"collation\":\"en-US\",\"decimal\":\".\",\"group\":\",\"," +
        "\"integer\":{\"group\":false,\"exponent\":false},\"fraction\":{\"group\":true,\"exponent\":true}}," +
        "\"timeZone\":{\"kind\":\"clock\"},\"trace\":{\"kind\":\"off\"}," +
        "\"vendors\":{\"native\":{\"locate\":{\"kind\":\"fn\",\"params\":[\"element\"],\"body\":{\"kind\":\"ref\",\"name\":\"element\"}}," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"}}}," +
        "\"objects\":{" +
        "\"$document\":{\"kind\":\"plugin\",\"name\":\"alis.document\",\"key\":\"\"," +
        "\"listen\":{\"kind\":\"method\",\"subscribe\":\"addEventListener\",\"unsubscribe\":\"removeEventListener\"},\"data\":{\"kind\":\"none\"}}," +
        "\"pump\":{\"kind\":\"element\",\"id\":\"pump\",\"vendor\":\"native\"}}," +
        "\"handlers\":[{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[";

    private const string Ends = "]}]}";

    private const string Pump = "{\"kind\":\"obj\",\"id\":\"pump\"}";

    private const string SetsTheRateTo40 =
        "{\"kind\":\"set\",\"path\":[\"rate\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":{\"kind\":\"lit\",\"value\":40}}";

    private const string ReadsTheFlowRate = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"get\",\"path\":[\"flowRate\"],\"members\":\"required\",\"target\":" + Pump + "},{\"kind\":\"lit\",\"value\":5}]}," +
        "\"then\":[" + SetsTheRateTo40 + "],\"else\":[]}" + Ends;

    private const string ReadsTheVolumeByItsName = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"get\",\"path\":[\"VTBI\"],\"members\":\"required\",\"target\":" + Pump + "},{\"kind\":\"lit\",\"value\":500}]}," +
        "\"then\":[" + SetsTheRateTo40 + "],\"else\":[]}" + Ends;

    private const string ReadsTheOptionalAlarm = OnLoad +
        "{\"kind\":\"if\",\"when\":{\"kind\":\"op\",\"op\":\"eq\",\"args\":[" +
        "{\"kind\":\"get\",\"path\":[\"alarm\"],\"members\":\"optionalMember\",\"target\":" + Pump + "},{\"kind\":\"lit\",\"value\":\"Occlusion\"}]}," +
        "\"then\":[" + SetsTheRateTo40 + "],\"else\":[]}" + Ends;

    [Test]
    public void a_member_the_door_names_nothing_for_reads_its_csharp_name_in_camel_case_whatever_the_apps_naming()
    {
        var html = Views.Of(new Infusion("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.FlowRate.Eq(5)).Then(pipeline => pump.Rate.Set(40));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ReadsTheFlowRate + ScriptClose));
    }

    [Test]
    public void a_member_the_door_names_reads_that_name_as_written()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.Volume.Eq(500)).Then(pipeline => pump.Rate.Set(40));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ReadsTheVolumeByItsName + ScriptClose));
    }

    [Test]
    public void an_optional_member_of_a_component_the_pipeline_reaches_by_its_id_reads_nothing_when_absent_and_its_holder_is_required()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pipeline.When(pump.Alarm.Eq("Occlusion")).Then(pipeline => pump.Rate.Set(40));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ReadsTheOptionalAlarm + ScriptClose));
    }

    [Test]
    public void the_plans_of_each_read_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(ReadsTheFlowRate), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ReadsTheVolumeByItsName), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(ReadsTheOptionalAlarm), Is.Empty);
    }

    [Test]
    public void a_member_of_a_declaration_made_with_new_is_refused_since_no_pipeline_reached_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pipeline.When(new InfusionPump().FlowRate.Eq(5)).Then(pipeline => pump.Rate.Set(40));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A declaration made with new is no object of the page. A pipeline reaches one: pipeline.Component<T>(id), or the payload an event hands its pipeline."));
    }
}
