using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.InputsAndIds;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnInputHasANumberBlank
{
    [TestCase(JsonNumberHandling.Strict)]
    [TestCase(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public void a_blank_is_a_live_number_under_the_apps_number_handling(JsonNumberHandling handling)
    {
        var html = Views.Of(new NumberCard(), options => options.NumberHandling = handling);
        var plan = html.ReactivePlan<NumberCard>();
        html.InputField(plan, m => m.Nights).Render<IntakeBox>(new HtmlString("<input>"));
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Nights).Component<IntakeBox>().Value.Set("ready")));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs").GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Nights").GetProperty("converted").GetRawText(),
            Is.EqualTo("""{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Nights"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}}"""));
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs").GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Nights").GetProperty("blank").GetRawText(), Is.EqualTo("""{"kind":"lit","value":5}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_number_members_own_number_handling_still_gives_its_blank_as_a_json_number()
    {
        var html = Views.Of(new NumberCard());
        var plan = html.ReactivePlan<NumberCard>();
        html.InputField(plan, m => m.Extra).Render<IntakeBox>(new HtmlString("<input>"));
        html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Input(m => m.Extra).Component<IntakeBox>().Value.Set("ready")));
        var json = MoveInPlans.JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs").GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Extra").GetProperty("converted").GetRawText(),
            Is.EqualTo("""{"kind":"as","expr":{"kind":"get","path":["value"],"members":"required","target":{"kind":"obj","id":"Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Extra"}},"shape":{"kind":"nullable","inner":{"kind":"number","of":"integer","text":"view","overflow":"nothing"}}}"""));
        Assert.That(document.RootElement.GetProperty("objects").GetProperty("$model").GetProperty("data").GetProperty("inputs").GetProperty("Alis_Reactive_ModuleTests_InputsAndIds_NumberCard__Extra").GetProperty("blank").GetRawText(), Is.EqualTo("""{"kind":"lit","value":7}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }
}

public sealed class NumberCard
{
    public int Nights { get; set; } = 5;
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
    public int Extra { get; set; } = 7;
}

public sealed class IntakeNumber : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<int> Value => Property<int>();
}
