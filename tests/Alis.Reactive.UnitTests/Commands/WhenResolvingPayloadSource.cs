using static VerifyNUnit.Verifier;

namespace Alis.Reactive.UnitTests;

public class PayloadModel
{
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public double DoubleValue { get; set; }
    public string? StringValue { get; set; }
    public bool BoolValue { get; set; }
    public PayloadAddress? Address { get; set; }
}

public class PayloadAddress
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Zip { get; set; }
}

[TestFixture]
public class WhenResolvingPayloadSource : PlanTestBase
{
    [Test]
    public Task SetText_from_flat_string_property() =>
        VerifyJson(BuildWithPayload((payload, p) =>
            p.Element("name").SetText(payload, x => x.StringValue)
        ).Render());

    [Test]
    public Task SetText_from_flat_int_property() =>
        VerifyJson(BuildWithPayload((payload, p) =>
            p.Element("count").SetText(payload, x => x.IntValue)
        ).Render());

    [Test]
    public Task SetText_from_flat_bool_property() =>
        VerifyJson(BuildWithPayload((payload, p) =>
            p.Element("active").SetText(payload, x => x.BoolValue)
        ).Render());

    [Test]
    public Task SetText_from_nested_property() =>
        VerifyJson(BuildWithPayload((payload, p) =>
            p.Element("city").SetText(payload, x => x.Address!.City)
        ).Render());

    [Test]
    public Task SetText_from_deeply_nested_resolves_full_path() =>
        VerifyJson(BuildWithPayload((payload, p) =>
        {
            p.Element("street").SetText(payload, x => x.Address!.Street);
            p.Element("city").SetText(payload, x => x.Address!.City);
            p.Element("zip").SetText(payload, x => x.Address!.Zip);
        }).Render());

    [Test]
    public Task Mixed_static_and_source_values() =>
        VerifyJson(BuildWithPayload((payload, p) =>
        {
            p.Element("label").SetText("Name:");
            p.Element("value").SetText(payload, x => x.StringValue);
            p.Element("status").AddClass("text-green");
        }).Render());

    [Test]
    public Task SetHtml_from_source() =>
        VerifyJson(BuildWithPayload((payload, p) =>
            p.Element("content").SetHtml(payload, x => x.StringValue)
        ).Render());

    [Test]
    public Task All_primitive_types_from_source() =>
        VerifyJson(BuildWithPayload((payload, p) =>
        {
            p.Element("int-value").SetText(payload, x => x.IntValue);
            p.Element("long-value").SetText(payload, x => x.LongValue);
            p.Element("double-value").SetText(payload, x => x.DoubleValue);
            p.Element("string-value").SetText(payload, x => x.StringValue);
            p.Element("bool-value").SetText(payload, x => x.BoolValue);
        }).Render());

    private static IReactivePlan<TestModel> BuildWithPayload(
        Action<PayloadModel, Builders.PipelineBuilder<TestModel>> configure)
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("test-event", configure);
        return plan;
    }
}
