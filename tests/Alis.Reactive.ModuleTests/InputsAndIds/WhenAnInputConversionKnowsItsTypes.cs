using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputConversionKnowsItsTypes
{
    [Test]
    public void overflow_is_the_hosts_binding_of_each_declared_number_type()
    {
        var html = Views.Of(new ConversionInputCard());
        var plan = html.ReactivePlan<ConversionInputCard>();
        html.InputField(plan, m => m.Double).Render<IntakeBox>(HtmlString.Empty);
        html.InputField(plan, m => m.Single).Render<IntakeBox>(HtmlString.Empty);
        html.InputField(plan, m => m.Decimal).Render<IntakeBox>(HtmlString.Empty);
        html.InputField(plan, m => m.Integer).Render<IntakeBox>(HtmlString.Empty);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var inputs = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs");

        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__Double").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"infinity"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__Single").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"infinity"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__Decimal").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"number","of":"fraction","text":"view","overflow":"nothing"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__Integer").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_text_input_states_its_components_declared_date_and_number_type()
    {
        var html = Views.Of(new ConversionInputCard());
        var plan = html.ReactivePlan<ConversionInputCard>();
        html.InputField(plan, m => m.WallText).Render<WallChoice>(HtmlString.Empty);
        html.InputField(plan, m => m.MomentText).Render<MomentChoice>(HtmlString.Empty);
        html.InputField(plan, m => m.NumberText).Render<DoubleChoice>(HtmlString.Empty);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var inputs = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs");

        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__WallText").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"string","date":"date","negativeZero":"0"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__MomentText").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"string","date":"instant","negativeZero":"0"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__NumberText").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"string","date":"none","negativeZero":"-0"}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void an_instant_input_states_whether_its_component_hands_wall_time_a_moment_or_text()
    {
        var html = Views.Of(new ConversionInputCard());
        var plan = html.ReactivePlan<ConversionInputCard>();
        html.InputField(plan, m => m.WallChoice).Render<WallChoice>(HtmlString.Empty);
        html.InputField(plan, m => m.MomentChoice).Render<MomentChoice>(HtmlString.Empty);
        html.InputField(plan, m => m.TextChoice).Render<IntakeBox>(HtmlString.Empty);
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var inputs = document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs");

        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__WallChoice").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"instant","from":"date"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__MomentChoice").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"instant","from":"instant"}}"""));
        Assert.That(inputs.GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_ConversionInputCard__TextChoice").GetProperty("value").GetProperty("args")[0].GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"nullable","inner":{"kind":"instant","from":"none"}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}

public sealed class ConversionInputCard
{
    public double Double { get; set; }
    public float? Single { get; set; }
    public decimal Decimal { get; set; }
    public int Integer { get; set; }
    public DateTimeOffset WallChoice { get; set; }
    public DateTimeOffset MomentChoice { get; set; }
    public DateTimeOffset TextChoice { get; set; }
    public string WallText { get; set; } = "";
    public string MomentText { get; set; } = "";
    public string NumberText { get; set; } = "";
}

public sealed class WallChoice : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<DateTime?> Value => Property<DateTime?>();
    public Event Change => Event();
    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

public sealed class MomentChoice : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<DateTimeOffset?> Value => Property<DateTimeOffset?>();
    public Event Change => Event();
    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}

public sealed class DoubleChoice : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<double?> Value => Property<double?>();
    public Event Change => Event();
    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
}
