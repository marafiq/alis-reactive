using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.ValuesAndSources;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnObjectIsBuiltFromMembers
{
    private const string Panel = "{\"kind\":\"obj\",\"id\":\"desk\"}";
    private const string NameRead = "{\"kind\":\"get\",\"path\":[\"name\"],\"members\":\"required\",\"target\":" + Panel + "}";
    private const string CountRead = "{\"kind\":\"get\",\"path\":[\"count\"],\"members\":\"required\",\"target\":" + Panel + "}";
    private const string NoMember = "'order => order' names no object member; set a member and the members past it, such as order => order.Destination.Ward.";

    [Test]
    public void literals_and_sources_fill_nested_members_by_the_apps_wire_names_in_written_order()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.ResidentName, panel.Name)
            .Set(x => x.Destination.WardName, "North")
            .Set(x => x.Reason, "fall")
            .Set(x => x.Destination.BedNumber, 4)
            .Set(x => x.Count, panel.Count)
            .Set(x => x.OptionalCount, panel.Count))), json => json.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"resident_name\",\"value\":" + NameRead + "}," +
            "{\"name\":\"destination\",\"value\":{\"kind\":\"record\",\"fields\":[{\"name\":\"ward_name\",\"value\":{\"kind\":\"lit\",\"value\":\"North\"}}," +
            "{\"name\":\"bed_number\",\"value\":{\"kind\":\"lit\",\"value\":4}}]}}," +
            "{\"name\":\"why.now\",\"value\":{\"kind\":\"lit\",\"value\":\"fall\"}}," +
            "{\"name\":\"count\",\"value\":" + CountRead + "},{\"name\":\"optional_count\",\"value\":" + CountRead + "}]}"));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void no_naming_policy_keeps_csharp_names_and_a_resolver_can_name_a_member()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.ResidentName, "Ada").Set(x => x.Count, 3))), json =>
        {
            json.PropertyNamingPolicy = null;
            json.TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(type =>
            {
                if (type.Type != typeof(TransferOrder)) return;
                foreach (var property in type.Properties)
                    if (property.Name == "Count") property.Name = "total.count";
            });
        });

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"ResidentName\",\"value\":{\"kind\":\"lit\",\"value\":\"Ada\"}}," +
            "{\"name\":\"total.count\",\"value\":{\"kind\":\"lit\",\"value\":3}}]}"));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void literals_keep_the_live_form_and_the_members_own_writing()
    {
        string? none = null;
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.ResidentName, none)
            .Set(x => x.Count, 3)
            .Set(x => x.Mode, PumpMode.Bolus)
            .Set(x => x.NamedMode, PumpMode.Bolus)
            .Set(x => x.TextCount, 5))), json =>
        {
            json.NumberHandling = JsonNumberHandling.WriteAsString;
            json.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"residentName\",\"value\":{\"kind\":\"lit\",\"value\":null}}," +
            "{\"name\":\"count\",\"value\":{\"kind\":\"lit\",\"value\":3}}," +
            "{\"name\":\"mode\",\"value\":{\"kind\":\"lit\",\"value\":1}}," +
            "{\"name\":\"namedMode\",\"value\":{\"kind\":\"lit\",\"value\":\"Bolus\"}}," +
            "{\"name\":\"textCount\",\"value\":{\"kind\":\"lit\",\"value\":\"5\"}}]}"));
        Assert.That(PlanSchema.ProblemsOf(rendered), Is.Empty);
    }

    [Test]
    public void a_literal_is_captured_when_set_and_the_completed_object_keeps_its_fields()
    {
        var destination = new TransferDestination { WardName = "North", BedNumber = 4 };
        var rendered = Rendered((pipeline, panel) =>
        {
            ObjectMembers<TransferOrder>? retained = null;
            var order = pipeline.Object<TransferOrder>(members =>
            {
                retained = members;
                members.Set(x => x.Destination, destination);
            });
            destination.WardName = "East";
            retained!.Set(x => x.Count, 9);
            panel.Order.Set(order);
        });

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"destination\",\"value\":{\"kind\":\"lit\",\"value\":{\"wardName\":\"North\",\"bedNumber\":4}}}]}"));
    }

    [Test]
    public void an_object_with_no_supplied_members_is_an_empty_record()
        => Assert.That(RecordOf(Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(_ => { })))),
            Is.EqualTo("{\"kind\":\"record\",\"fields\":[]}"));

    [Test]
    public void a_source_in_the_same_members_own_form_keeps_that_form()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.NamedMode, panel.Previous.Read(x => x.NamedMode)))));

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"namedMode\",\"value\":{\"kind\":\"data\",\"path\":[{\"name\":\"namedMode\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"Continuous\"}}]," +
            "\"target\":{\"kind\":\"get\",\"path\":[\"previous\"],\"members\":\"required\",\"target\":" + Panel + "}}}]}"));
    }

    [Test]
    public void a_number_source_written_as_text_is_converted_for_an_ordinary_member()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.OptionalCount, panel.Previous.Read(x => x.TextCount)))));

        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"optionalCount\",\"value\":{\"kind\":\"as\",\"expr\":{\"kind\":\"data\",\"path\":[{\"name\":\"textCount\",\"requires\":[],\"absent\":{\"kind\":\"start\",\"value\":\"0\"}}]," +
            "\"target\":{\"kind\":\"get\",\"path\":[\"previous\"],\"members\":\"required\",\"target\":" + Panel + "}},\"shape\":{\"kind\":\"number\",\"of\":\"integer\",\"text\":\"invariant\"}}}]}"));
    }

    [Test]
    public void a_literal_path_that_names_no_member_is_refused_with_its_cure()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(members => members.Set(order => order, new TransferOrder())))),
            Throws.InvalidOperationException.With.Message.EqualTo(NoMember));

    [Test]
    public void a_source_path_that_names_no_member_is_refused_with_its_cure()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(members => members.Set(order => order, panel.Previous)))),
            Throws.InvalidOperationException.With.Message.EqualTo(NoMember));

    [Test]
    public void a_nullable_source_path_that_computes_instead_of_naming_a_member_is_refused()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(members => members.Set(order => order.OptionalCount + 1, panel.Count)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'order => (order.OptionalCount + Convert(1, Nullable`1))' names no object member; set a member and the members past it, such as order => order.Destination.Ward."));

    [Test]
    public void an_ignored_member_is_refused_with_the_member_and_a_cure()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Ignored, "private")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.Ignored' cannot be set: 'x => x.Ignored' reads a member of 'TransferOrder' that the app's JSON options do not write, so the browser never holds it. Set a member the app's JSON contract writes, or pass the containing object as a value from the server."));

    [Test]
    public void a_path_past_a_members_converter_is_refused_with_the_member_and_a_cure()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Converted.WardName, "North")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.Converted.WardName' cannot be set: 'x.Converted.WardName' reads past 'x.Converted', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'x.Converted' itself, or read past it on the server. Set a member the app's JSON contract writes, or pass the containing object as a value from the server."));

    [Test]
    public void a_source_written_by_a_converter_cannot_fill_an_ordinary_member()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Mode, panel.Previous.Read(x => x.NamedMode))))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "A live object takes 'x.NamedMode' as JavaScript holds its type, and its own JSON converter writes it otherwise; pass the value from the server."));

    [Test]
    public void an_ordinary_source_cannot_fill_a_member_written_in_another_form()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.TextCount, panel.Count)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.TextCount' is written by its own JSON number handling, unlike the source; pass the member's value from the server, or a source in the member's own form."));

    [Test]
    public void a_repeated_member_is_refused_instead_of_discarding_a_source()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Count, panel.Count).Set(x => x.Count, 7)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.Count' overlaps a member already supplied; set each member once, and set either the containing object or its members."));

    [Test]
    public void a_member_and_its_child_cannot_both_be_supplied()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Destination, new TransferDestination()).Set(x => x.Destination.WardName, "North")))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.Destination.WardName' overlaps a member already supplied; set each member once, and set either the containing object or its members."));

    [Test]
    public void a_child_and_its_containing_member_cannot_both_be_supplied()
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Destination.WardName, "North").Set(x => x.Destination, new TransferDestination())))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object member 'x.Destination' overlaps a member already supplied; set each member once, and set either the containing object or its members."));

    [TestCase(double.NaN, "NaN")]
    [TestCase(double.PositiveInfinity, "Infinity")]
    [TestCase(double.NegativeInfinity, "-Infinity")]
    public void a_number_no_json_number_holds_is_refused(double number, string written)
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Ratio, number)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'x.Ratio' holds a JavaScript number, and " + written + " is NaN or an infinity, which no JSON number holds, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_decimal_past_fifteen_significant_digits_is_refused()
    {
        var number = 1.234567890123456m;
        Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Amount, number)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'x.Amount' holds a JavaScript number, and 1.234567890123456 is a decimal of more than 15 significant digits, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));
    }

    [TestCase(9007199254740992L, "9007199254740992")]
    [TestCase(-9007199254740992L, "-9007199254740992")]
    public void a_whole_number_past_javascripts_safe_integer_is_refused(long number, string written)
        => Assert.That(() => Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.Ticket, number)))),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "'x.Ticket' holds a JavaScript number, and " + written + " is a whole number past 9007199254740991, which JavaScript reads as another value; write a finite number of at most 15 significant digits."));

    [Test]
    public void a_members_own_form_keeps_numbers_written_as_text()
    {
        var amount = 1.234567890123456m;
        var ratio = double.PositiveInfinity;
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.TextAmount, amount).Set(x => x.NamedRatio, ratio))));
        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"textAmount\",\"value\":{\"kind\":\"lit\",\"value\":\"1.234567890123456\"}}," +
            "{\"name\":\"namedRatio\",\"value\":{\"kind\":\"lit\",\"value\":\"Infinity\"}}]}"));
    }

    [Test]
    public void a_number_on_each_precision_boundary_is_written()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order
            .Set(x => x.Amount, 1.2345678901234500m).Set(x => x.Ticket, 9007199254740991L).Set(x => x.Ratio, 0.5))));
        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"amount\",\"value\":{\"kind\":\"lit\",\"value\":1.2345678901234500}}," +
            "{\"name\":\"ticket\",\"value\":{\"kind\":\"lit\",\"value\":9007199254740991}}," +
            "{\"name\":\"ratio\",\"value\":{\"kind\":\"lit\",\"value\":0.5}}]}"));
    }

    [Test]
    public void a_nullable_source_keeps_its_expression()
    {
        var rendered = Rendered((pipeline, panel) => panel.Order.Set(pipeline.Object<TransferOrder>(order => order.Set(x => x.OptionalCount, panel.OptionalCount))));
        Assert.That(RecordOf(rendered), Is.EqualTo(
            "{\"kind\":\"record\",\"fields\":[{\"name\":\"optionalCount\",\"value\":{\"kind\":\"get\",\"path\":[\"optionalCount\"],\"members\":\"required\",\"target\":" + Panel + "}}]}"));
    }

    [Test]
    public void a_scalar_cannot_be_built_as_an_object()
        => Assert.That(() => Rendered((pipeline, _) => pipeline.Object<int>(_ => { })),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object<T> builds named members of a JSON object; pass a scalar, a list or a value written by a converter as a literal or a source of its own type."));

    [Test]
    public void a_list_cannot_be_built_as_an_object()
        => Assert.That(() => Rendered((pipeline, _) => pipeline.Object<string[]>(_ => { })),
            Throws.InvalidOperationException.With.Message.EqualTo(
                "Object<T> builds named members of a JSON object; pass a scalar, a list or a value written by a converter as a literal or a source of its own type."));

    private static string Rendered(Action<Pipeline<ObjectDesk>, ObjectPanel> write, Action<JsonSerializerOptions>? options = null)
    {
        var html = Views.Of(new ObjectDesk("North"), options ?? (_ => { }));
        var plan = html.ReactivePlan<ObjectDesk>();
        html.On(plan, trigger => trigger.DomReady(pipeline => write(pipeline, pipeline.Component<ObjectPanel>("desk"))));
        var rendered = Views.TextOf(html.RenderPlan(plan));
        return rendered["<script type=\"application/json\" data-reactive-plan>".Length..^"</script>".Length];
    }

    private static string RecordOf(string rendered)
    {
        using var document = JsonDocument.Parse(rendered);
        return document.RootElement.GetProperty("handlers")[0].GetProperty("do")[0].GetProperty("value").GetRawText();
    }
}

public sealed record ObjectDesk(string Name);

public sealed class ObjectPanel : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<TransferOrder> Order => Property<TransferOrder>("order");

    public ReadOnly<TransferOrder> Previous => ReadOnly<TransferOrder>("previous");

    public ReadOnly<string?> Name => ReadOnly<string?>("name");

    public ReadOnly<int> Count => ReadOnly<int>("count");

    public ReadOnly<int?> OptionalCount => ReadOnly<int?>("optionalCount");
}

public sealed class TransferOrder
{
    public string? ResidentName { get; init; }

    public TransferDestination Destination { get; init; } = new();

    [JsonPropertyName("why.now")]
    public string Reason { get; init; } = "";

    public int Count { get; init; }

    public int? OptionalCount { get; init; }

    public PumpMode Mode { get; init; }

    public double Ratio { get; init; }

    public decimal Amount { get; init; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public decimal TextAmount { get; init; }

    [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
    public double NamedRatio { get; init; }

    public long Ticket { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<PumpMode>))]
    public PumpMode NamedMode { get; init; }

    [JsonNumberHandling(JsonNumberHandling.WriteAsString)]
    public int TextCount { get; init; }

    [JsonIgnore]
    public string Ignored { get; init; } = "";

    [JsonConverter(typeof(DestinationConverter))]
    public TransferDestination Converted { get; init; } = new();
}

public sealed class TransferDestination
{
    public string WardName { get; set; } = "";

    public int BedNumber { get; init; }
}

public sealed class DestinationConverter : JsonConverter<TransferDestination>
{
    public override TransferDestination Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new() { WardName = reader.GetString()! };

    public override void Write(Utf8JsonWriter writer, TransferDestination value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.WardName);
}
