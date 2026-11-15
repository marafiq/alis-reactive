using System;
using System.Text.Json;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenDataIsReadPastADeclaredMember
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

    private const string ThePrescription = "{\"kind\":\"get\",\"path\":[\"currentPrescription\"],\"members\":\"required\",\"target\":" + Pump + "}";

    private const string SetsTheRate = "{\"kind\":\"set\",\"path\":[\"rate\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":";

    private const string SetsTheLabel = "{\"kind\":\"set\",\"path\":[\"label\"],\"members\":\"optional\",\"content\":false,\"target\":" + Pump + ",\"value\":";

    private const string DosesUnderTheAppsNaming = OnLoad + SetsTheRate +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"max_daily_doses\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":4}}],\"target\":" + ThePrescription + "}}" + Ends;

    private const string RouteByItsOwnName = OnLoad + SetsTheLabel +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"Route\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"oral\"}}],\"target\":" + ThePrescription + "}}" + Ends;

    private const string UnitsPastTheDose = OnLoad + SetsTheRate +
        "{\"kind\":\"data\",\"path\":[" +
        "{\"name\":\"dose\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}," +
        "{\"name\":\"units\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":1}}],\"target\":" + ThePrescription + "}}" + Ends;

    private const string NamePastThePrescriber = OnLoad + SetsTheLabel +
        "{\"kind\":\"data\",\"path\":[" +
        "{\"name\":\"prescriber\",\"requires\":[],\"absent\":{\"kind\":\"nothing\"}}," +
        "{\"name\":\"name\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"unnamed\"}}],\"target\":" + ThePrescription + "}}" + Ends;

    private const string TheSealAsItsConverterWritesIt = OnLoad +
        "{\"kind\":\"set\",\"path\":[\"seal\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"seal\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"\"}}],\"target\":" + ThePrescription + "}}" + Ends;

    [Test]
    public void a_member_of_the_data_is_named_by_the_apps_naming_where_the_declared_member_keeps_camel_case()
    {
        var html = Views.Of(new Infusion("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.CurrentPrescription.Read(prescription => prescription.MaxDailyDoses));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + DosesUnderTheAppsNaming + ScriptClose));
    }

    [Test]
    public void a_member_the_type_names_for_itself_keeps_that_name_under_the_apps_naming()
    {
        var html = Views.Of(new Infusion("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.CurrentPrescription.Read(prescription => prescription.Route));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + RouteByItsOwnName + ScriptClose));
    }

    [Test]
    public void each_step_reads_the_start_a_new_object_binds_when_its_member_is_absent()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.CurrentPrescription.Read(prescription => prescription.Dose.Units));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + UnitsPastTheDose + ScriptClose));
    }

    [Test]
    public void a_member_past_a_holder_that_starts_null_reads_nothing_when_the_holder_is_absent()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.CurrentPrescription.Read(prescription => prescription.Prescriber!.Name));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + NamePastThePrescriber + ScriptClose));
    }

    [Test]
    public void a_member_its_own_converter_writes_is_read_whole_with_its_start_as_the_converter_writes_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Seal.Set(pump.CurrentPrescription.Read(prescription => prescription.Seal));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + TheSealAsItsConverterWritesIt + ScriptClose));
    }

    [Test]
    public void the_plans_of_each_read_are_valid_by_the_plan_schema()
    {
        Assert.That(PlanSchema.ProblemsOf(DosesUnderTheAppsNaming), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(RouteByItsOwnName), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(UnitsPastTheDose), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(NamePastThePrescriber), Is.Empty);
        Assert.That(PlanSchema.ProblemsOf(TheSealAsItsConverterWritesIt), Is.Empty);
    }

    [Test]
    public void a_read_past_a_member_its_own_converter_writes_is_refused_since_the_browser_holds_what_the_converter_writes()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Label.Set(pump.CurrentPrescription.Read(prescription => prescription.Seal.Tag));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'prescription.Seal.Tag' reads past 'prescription.Seal', which its own JSON converter writes, so the browser holds what the converter writes, not its members; " +
                "read 'prescription.Seal' itself, or read past it on the server."));
    }

    [Test]
    public void a_read_of_a_member_the_apps_json_options_do_not_write_is_refused_since_the_browser_never_holds_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Label.Set(pump.CurrentPrescription.Read(prescription => prescription.Note));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'prescription => prescription.Note' reads a member of 'Prescription' that the app's JSON options do not write, so the browser never holds it."));
    }

    [Test]
    public void a_read_whose_lambda_names_no_member_path_is_refused_since_the_browser_has_no_member_to_read()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.Rate.Set(pump.CurrentPrescription.Read(prescription => prescription.MaxDailyDoses * 2));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'prescription => (prescription.MaxDailyDoses * 2)' names no member path, so the browser has no member to read; " +
                "read a member and the members past it, such as record => record.Detail.Name."));
    }
}
