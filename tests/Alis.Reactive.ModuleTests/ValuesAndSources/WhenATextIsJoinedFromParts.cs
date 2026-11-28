using System;
using System.Text.Json;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenATextIsJoinedFromParts
{
    private const string Board = "{\"kind\":\"obj\",\"id\":\"duty\"}";
    private const string NoText = "{\"kind\":\"lit\",\"value\":\"\"}";
    private const string NurseShown =
        "{\"kind\":\"op\",\"op\":\"coalesce\",\"args\":[{\"kind\":\"as\",\"expr\":{\"kind\":\"get\",\"path\":[\"nurse\"],\"members\":\"required\",\"target\":" + Board + "}," +
        "\"shape\":{\"kind\":\"string\",\"date\":\"none\",\"negativeZero\":\"0\"}}," + NoText + "]}";

    [Test]
    public void each_part_reaches_the_join_as_a_text_in_the_order_written()
    {
        var rendered = Rendered((pipeline, board) => board.Line.Set(pipeline.Concat("Nurse [", board.Nurse, "] on duty")));

        Assert.That(SetValueOf(rendered), Is.EqualTo(
            "{\"kind\":\"call\",\"path\":[\"join\"],\"members\":\"requiredIfHeld\",\"target\":{\"kind\":\"list\",\"items\":[" +
            "{\"kind\":\"lit\",\"value\":\"Nurse [\"}," + NurseShown + ",{\"kind\":\"lit\",\"value\":\"] on duty\"}]},\"args\":[" + NoText + "]}"));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void a_null_text_and_a_null_part_add_nothing()
    {
        string? missing = null;

        var rendered = Rendered((pipeline, board) => board.Line.Set(pipeline.Concat("Nurse", missing, null!)));

        Assert.That(SetValueOf(rendered), Is.EqualTo(
            "{\"kind\":\"call\",\"path\":[\"join\"],\"members\":\"requiredIfHeld\",\"target\":{\"kind\":\"list\",\"items\":[" +
            "{\"kind\":\"lit\",\"value\":\"Nurse\"}," + NoText + "," + NoText + "]},\"args\":[" + NoText + "]}"));
    }

    [Test]
    public void a_null_list_of_parts_is_refused()
        => Assert.That(() => Rendered((pipeline, board) => board.Line.Set(pipeline.Concat(null!))),
            Throws.ArgumentNullException.With.Message.EqualTo("Value cannot be null. (Parameter 'parts')"));

    private static string Rendered(Action<Pipeline<DutyDesk>, DutyPanel> write)
    {
        var html = Views.Of(new DutyDesk("North"));
        var plan = html.ReactivePlan<DutyDesk>();
        html.On(plan, trigger => trigger.DomReady(pipeline => write(pipeline, pipeline.Component<DutyPanel>("duty"))));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string SetValueOf(string rendered)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText();
    }
}

public sealed record DutyDesk(string Wing);

public sealed class DutyPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ReadOnly<string?> Nurse => ReadOnly<string?>();

    public Property<string> Line => Property<string>();
}
