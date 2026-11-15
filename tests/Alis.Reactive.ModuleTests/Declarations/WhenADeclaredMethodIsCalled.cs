using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Declarations;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenADeclaredMethodIsCalled
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
    private const string FirstResult = "{\"kind\":\"ref\",\"name\":\"result1\"}";
    private const string SecondResult = "{\"kind\":\"ref\",\"name\":\"result2\"}";
    private const string Prescription = "{\"kind\":\"get\",\"path\":[\"currentPrescription\"],\"members\":\"required\",\"target\":" + Pump + "}";
    private const string SetsTheRate = "{\"kind\":\"set\",\"path\":[\"rate\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":";
    private const string SetsTheLabel = "{\"kind\":\"set\",\"path\":[\"label\"],\"members\":\"optional\",\"content\":false,\"target\":" + Pump + ",\"value\":";

    private const string Starts = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"start\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}" + Ends;

    private const string Silences = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"silence\"],\"members\":\"optionalMember\",\"target\":" + Pump + ",\"args\":[]}}" + Ends;

    private const string Delivers = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"deliver-dose\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}" + Ends;

    private const string Programs = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"program\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"lit\",\"value\":40}," +
        "{\"kind\":\"get\",\"path\":[\"VTBI\"],\"members\":\"required\",\"target\":" + Pump + "}," +
        "{\"kind\":\"lit\",\"value\":null}]}}" + Ends;

    private const string Schedules = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"schedule\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"as\",\"expr\":{\"kind\":\"lit\",\"value\":\"2026-11-14T08:30:00\"},\"shape\":{\"kind\":\"date\"}}]}}" + Ends;

    private const string SetsTheWindow = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"window\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"as\",\"expr\":{\"kind\":\"lit\",\"value\":{\"starts_at\":\"2026-11-14T08:30:00\"}}," +
        "\"shape\":{\"kind\":\"object\",\"fields\":{\"starts_at\":{\"kind\":\"date\"}}}}]}}" + Ends;

    private const string Primes = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"prime\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"lit\",\"value\":2},{\"kind\":\"lit\",\"value\":\"saline\"}]}}" + Ends;

    private const string Refills = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"refill\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"as\",\"expr\":{\"kind\":\"data\",\"path\":[{\"name\":\"refills\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"2\"}}]," +
        "\"target\":" + Prescription + "},\"shape\":{\"kind\":\"number\",\"of\":\"integer\",\"text\":\"invariant\"}}]}}" + Ends;

    private const string Repeats = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"repeat\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"as\",\"expr\":{\"kind\":\"data\",\"path\":[{\"name\":\"refills\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"2\"}}]," +
        "\"target\":" + Prescription + "},\"shape\":{\"kind\":\"number\",\"of\":\"integer\",\"text\":\"invariant\"}}]}}" + Ends;

    private const string Reviews = OnLoad +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"review\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"get\",\"path\":[\"pendingOrder\"],\"members\":\"required\",\"target\":" + Pump + "}]}}" + Ends;

    private const string Measures =
        "{\"kind\":\"let\",\"name\":\"result1\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"measuredRate\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}";

    private const string MeasuresAgain =
        "{\"kind\":\"let\",\"name\":\"result2\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"measuredRate\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}";

    private const string Summarizes =
        "{\"kind\":\"let\",\"name\":\"result1\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"summary\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}";

    private const string KeepsTheMeasurement = OnLoad + Measures + "," +
        SetsTheRate + "{\"kind\":\"lit\",\"value\":999}}," +
        SetsTheRate + FirstResult + "}," +
        "{\"kind\":\"set\",\"path\":[\"bolus\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + FirstResult + "}" + Ends;

    private const string MeasuresTwice = OnLoad + Measures + "," + MeasuresAgain + "," +
        SetsTheRate + FirstResult + "}," +
        "{\"kind\":\"set\",\"path\":[\"bolus\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + SecondResult + "}" + Ends;

    private const string MeasuresAcrossHandlers = OnLoad + Measures +
        "]},{\"on\":\"$document\",\"path\":[],\"event\":\"ready\",\"payload\":[],\"members\":\"required\",\"policy\":\"overlap\",\"do\":[" +
        MeasuresAgain + Ends;

    private const string ReadsTheSummary = OnLoad + Summarizes + "," + SetsTheRate +
        "{\"kind\":\"data\",\"path\":[{\"name\":\"max_daily_doses\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":4}}],\"target\":" + FirstResult + "}}" + Ends;

    private const string RefillsFromTheSummary = OnLoad + Summarizes + "," +
        "{\"kind\":\"do\",\"await\":false,\"expr\":{\"kind\":\"call\",\"path\":[\"refill\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[" +
        "{\"kind\":\"as\",\"expr\":{\"kind\":\"data\",\"path\":[{\"name\":\"refills\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"2\"}}]," +
        "\"target\":" + FirstResult + "},\"shape\":{\"kind\":\"number\",\"of\":\"integer\",\"text\":\"invariant\"}}]}}" + Ends;

    private const string Estimates = OnLoad +
        "{\"kind\":\"let\",\"name\":\"result1\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"estimate-dose\"],\"members\":\"optionalMember\",\"target\":" + Pump + ",\"args\":[{\"kind\":\"lit\",\"value\":250}]}}," +
        SetsTheRate + FirstResult + "}" + Ends;

    private const string ReadsTheLastAlarm = OnLoad +
        "{\"kind\":\"let\",\"name\":\"result1\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"last-alarm\"],\"members\":\"optionalMember\",\"target\":" + Pump + ",\"args\":[]}}," +
        SetsTheLabel + FirstResult + "}" + Ends;

    private const string KeepsTheLiveDate = OnLoad +
        "{\"kind\":\"let\",\"name\":\"result1\",\"await\":false,\"binds\":\"line\",\"expr\":" +
        "{\"kind\":\"call\",\"path\":[\"nextStart\"],\"members\":\"required\",\"target\":" + Pump + ",\"args\":[]}}," +
        "{\"kind\":\"set\",\"path\":[\"startsAt\"],\"members\":\"required\",\"content\":false,\"target\":" + Pump + ",\"value\":" + FirstResult + "}" + Ends;

    [Test]
    public void a_method_with_no_argument_runs_at_its_line()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Start()));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Starts + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Starts), Is.Empty);
    }

    [Test]
    public void an_optional_method_can_be_absent_on_its_holder()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Silence()));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Silences + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Silences), Is.Empty);
    }

    [Test]
    public void a_method_uses_its_explicit_javascript_name()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Deliver()));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Delivers + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Delivers), Is.Empty);
    }

    [Test]
    public void literals_sources_and_a_null_literal_keep_their_argument_positions()
    {
        var html = Views.Of(new Infusion("North"), json => json.NumberHandling = JsonNumberHandling.WriteAsString);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Program(40, pump.Volume, null!);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Programs + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Programs), Is.Empty);
    }

    [Test]
    public void a_typed_null_value_is_written_as_null()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        string? label = null;
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Program(40, pump.Volume, label);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Programs + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Programs), Is.Empty);
    }

    [Test]
    public void a_date_argument_reaches_the_method_as_a_date()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Component<InfusionPump>("pump").Schedule(new DateTime(2026, 11, 14, 8, 30, 0, DateTimeKind.Utc))));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Schedules + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Schedules), Is.Empty);
    }

    [Test]
    public void an_argument_uses_the_json_of_the_plan_that_calls_it()
    {
        Arg<DeliveryWindow> window = new DeliveryWindow(new DateTime(2026, 11, 14, 8, 30, 0));
        var html = Views.Of(new Infusion("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Window(window)));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + SetsTheWindow + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(SetsTheWindow), Is.Empty);
    }

    [Test]
    public void a_number_written_as_text_is_converted_for_its_argument()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Refill(pump.CurrentPrescription.Read(prescription => prescription.Refills));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Refills + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Refills), Is.Empty);
    }

    [Test]
    public void fixed_arguments_are_literals_in_the_live_form()
    {
        var html = Views.Of(new Infusion("North"), json => json.NumberHandling = JsonNumberHandling.WriteAsString);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").Prime()));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Primes + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Primes), Is.Empty);
    }

    [Test]
    public void pass_converts_a_sources_own_number_text()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Repeat(pump.CurrentPrescription.Read(prescription => prescription.Refills));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Repeats + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Repeats), Is.Empty);
    }

    [Test]
    public void pass_keeps_a_derived_live_orders_date_without_reshaping_it()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Review(pump.PendingOrder);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Reviews + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Reviews), Is.Empty);
    }

    [Test]
    public void two_reads_keep_one_result_across_a_later_write()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var rate = pump.MeasuredRate();
            pump.Rate.Set(999);
            pump.Rate.Set(rate);
            pump.Bolus.Set(rate);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + KeepsTheMeasurement + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(KeepsTheMeasurement), Is.Empty);
    }

    [Test]
    public void two_calls_keep_two_results_in_the_same_plan()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            var first = pump.MeasuredRate();
            var second = pipeline.Component<InfusionPump>("pump").MeasuredRate();
            pump.Rate.Set(first);
            pump.Bolus.Set(second);
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + MeasuresTwice + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(MeasuresTwice), Is.Empty);
    }

    [Test]
    public void result_names_are_counted_across_the_plans_handlers()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").MeasuredRate()));
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Component<InfusionPump>("pump").MeasuredRate()));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + MeasuresAcrossHandlers + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(MeasuresAcrossHandlers), Is.Empty);
    }

    [Test]
    public void a_results_members_are_read_as_data_under_the_apps_naming()
    {
        var html = Views.Of(new Infusion("North"), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Summary().Read(summary => summary.MaxDailyDoses));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ReadsTheSummary + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadsTheSummary), Is.Empty);
    }

    [Test]
    public void a_results_member_keeps_its_own_number_handling()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Refill(pump.Summary().Read(summary => summary.Refills));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + RefillsFromTheSummary + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(RefillsFromTheSummary), Is.Empty);
    }

    [Test]
    public void a_named_optional_method_with_arguments_returns_a_source()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Rate.Set(pump.Estimate(250));
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + Estimates + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(Estimates), Is.Empty);
    }

    [Test]
    public void a_named_optional_method_without_arguments_returns_a_source()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.Label.Set(pump.LastAlarm());
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + ReadsTheLastAlarm + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(ReadsTheLastAlarm), Is.Empty);
    }

    [Test]
    public void a_returned_date_is_already_in_the_live_form()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var pump = pipeline.Component<InfusionPump>("pump");
            pump.StartsAt.Set(pump.NextStart());
        }));

        var rendered = Views.TextOf(html.RenderPlan(plan));

        Assert.That(rendered, Is.EqualTo(ScriptOpen + KeepsTheLiveDate + ScriptClose));
        Assert.That(PlanSchema.ProblemsOf(KeepsTheLiveDate), Is.Empty);
    }

    [Test]
    public void a_source_its_own_converter_writes_is_refused_as_an_argument()
    {
        var html = Views.Of(new Infusion("North"));
        var plan = html.ReactivePlan<Infusion>();

        Action theView = () =>
        {
            html.On(plan, trigger => trigger.DomReady(pipeline =>
            {
                var pump = pipeline.Component<InfusionPump>("pump");
                pump.SealDose(pump.CurrentPrescription.Read(prescription => prescription.Seal));
            }));
            html.RenderPlan(plan);
        };

        Assert.That(
            () => theView(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'prescription.Seal' as JavaScript holds its type, and its own JSON converter writes it otherwise; pass the value from the server."));
    }

    [Test]
    public void a_method_on_a_declaration_made_with_new_is_refused()
    {
        var pump = new InfusionPump();

        Assert.That(
            () => pump.Start(),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A declaration made with new is no object of the page. A pipeline reaches one: pipeline.Component<T>(id), or the payload an event hands its pipeline."));
    }
}
