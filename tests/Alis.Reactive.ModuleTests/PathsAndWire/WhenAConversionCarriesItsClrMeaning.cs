using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Html;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.PathsAndWire;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAConversionCarriesItsClrMeaning
{
    [Test]
    public void a_dates_text_names_the_declared_type_even_inside_a_nullable_list()
    {
        var html = Views.Of(new ConversionCard());
        var plan = html.ReactivePlan<ConversionCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var meter = pipeline.Component<ConversionMeter>("meter");
            pipeline.Element("wall").Text.Set(meter.Wall);
            pipeline.Element("instant").Text.Set(meter.Instant);
            pipeline.Element("visits").Text.Set(meter.Visits);
        }));
        var json = JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var steps = document.RootElement.GetProperty("handlers")[0].GetProperty("do");

        Assert.That(steps[0].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"date","negativeZero":"0"}"""));
        Assert.That(steps[1].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"instant","negativeZero":"0"}"""));
        Assert.That(steps[2].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"instant","negativeZero":"0"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void negative_zero_text_is_the_hosts_text_for_the_sources_number_type()
    {
        var html = Views.Of(new ConversionCard());
        var plan = html.ReactivePlan<ConversionCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
        {
            var meter = pipeline.Component<ConversionMeter>("meter");
            pipeline.Element("double").Text.Set(meter.Double);
            pipeline.Element("single").Text.Set(meter.Single);
            pipeline.Element("decimal").Text.Set(meter.Decimal);
            pipeline.Element("integer").Text.Set(meter.Integer);
            pipeline.Element("samples").Text.Set(meter.Samples);
        }));
        var json = JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);
        var steps = document.RootElement.GetProperty("handlers")[0].GetProperty("do");

        Assert.That(steps[0].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"-0"}"""));
        Assert.That(steps[1].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"-0"}"""));
        Assert.That(steps[2].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"0"}"""));
        Assert.That(steps[3].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"0"}"""));
        Assert.That(steps[4].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"-0"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    [SetCulture("fa-IR")]
    public void negative_zero_text_keeps_the_views_cultures_negative_sign()
    {
        var html = Views.Of(new ConversionCard());
        var plan = html.ReactivePlan<ConversionCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Element("reading").Text.Set(pipeline.Component<ConversionMeter>("meter").Double)));
        var json = JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);

        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"string","date":"none","negativeZero":"\u200E\u22120"}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_numbers_own_text_form_is_converted_before_it_is_shown()
    {
        var html = Views.Of(new ConversionCard());
        var plan = html.ReactivePlan<ConversionCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Element("reading").Text.Set(pipeline.Component<ConversionMeter>("meter").Numbers.Read(numbers => numbers.Ratio))));
        var json = JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);

        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText(),
            Is.EqualTo("""{"kind":"as","expr":{"kind":"as","expr":{"kind":"data","path":[{"name":"ratio","requires":[],"absent":{"kind":"start","value":"0"}}],"target":{"kind":"get","path":["numbers"],"members":"required","target":{"kind":"obj","id":"meter"}}},"shape":{"kind":"number","of":"fraction","text":"invariant","overflow":"infinity"}},"shape":{"kind":"string","date":"none","negativeZero":"-0"}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    [Test]
    public void a_live_objects_conversion_states_each_fields_binding_of_nothing_and_each_instants_origin()
    {
        var html = Views.Of(new ConversionCard());
        var plan = html.ReactivePlan<ConversionCard>();
        html.On(plan, trigger => trigger.DomReady(pipeline =>
            pipeline.Component<ConversionMeter>("meter").Visit.Set(new ConversionVisit())));
        var json = JsonOf(html.RenderPlan(plan));
        using var document = JsonDocument.Parse(json);

        Assert.That(document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetProperty("shape").GetRawText(),
            Is.EqualTo("""{"kind":"object","fields":{"wall":{"shape":{"kind":"date"},"nothing":"omit"},"instant":{"shape":{"kind":"instant","from":"instant"},"nothing":"omit"},"maybe":{"shape":{"kind":"nullable","inner":{"kind":"date"}},"nothing":"null"},"visits":{"shape":{"kind":"array","item":{"kind":"instant","from":"instant"}},"nothing":"omit"},"child":{"shape":{"kind":"object","fields":{"at":{"shape":{"kind":"date"},"nothing":"omit"}}},"nothing":"null"},"stamp":{"shape":{"kind":"object","fields":{"at":{"shape":{"kind":"date"},"nothing":"omit"}}},"nothing":"omit"}}}"""));
        Assert.That(PlanSchema.ProblemsOf(json), Is.Empty);
    }

    private static string JsonOf(IHtmlContent content)
        => Views.TextOf(content)["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
}

public sealed class ConversionCard;

public sealed class ConversionMeter : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Property<DateTime> Wall => Property<DateTime>();
    public Property<DateTimeOffset> Instant => Property<DateTimeOffset>();
    public Property<IReadOnlyList<IReadOnlyList<DateTimeOffset?>>> Visits => Property<IReadOnlyList<IReadOnlyList<DateTimeOffset?>>>();
    public Property<double> Double => Property<double>();
    public Property<float?> Single => Property<float?>();
    public Property<decimal> Decimal => Property<decimal>();
    public Property<int> Integer => Property<int>();
    public Property<IReadOnlyList<double?>> Samples => Property<IReadOnlyList<double?>>();
    public Property<ConversionVisit> Visit => Property<ConversionVisit>();
    public Property<ConversionNumbers> Numbers => Property<ConversionNumbers>();
}

public sealed class ConversionVisit
{
    public DateTime Wall { get; set; } = new(2026, 11, 9);
    public DateTimeOffset Instant { get; set; } = new(2026, 11, 9, 13, 30, 0, TimeSpan.Zero);
    public DateTime? Maybe { get; set; } = new(2026, 7, 9);
    public IReadOnlyList<DateTimeOffset> Visits { get; set; } = [];
    public ConversionChild Child { get; set; } = new();
    public ConversionStamp Stamp { get; set; }
}

public sealed class ConversionChild
{
    public DateTime At { get; set; } = new(2026, 7, 9);
}

public readonly record struct ConversionStamp(DateTime At);

public sealed class ConversionNumbers
{
    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public double Ratio { get; set; }
}
