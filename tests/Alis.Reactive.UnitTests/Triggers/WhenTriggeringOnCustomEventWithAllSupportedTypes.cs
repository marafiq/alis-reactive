namespace Alis.Reactive.UnitTests;

public class CustomEventWithAllSupportedTypes
{
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public string? StringValue { get; set; }
    public DateTime DateValue { get; set; }
    public bool BoolValue { get; set; }
}

[TestFixture]
public class WhenTriggeringOnCustomEventWithAllSupportedTypes : PlanTestBase
{
    [Test]
    public Task Payload_contains_all_primitive_types()
    {
        var plan = new ReactivePlan<CustomEventWithAllSupportedTypes>();
        var trigger = new Builders.TriggerBuilder<CustomEventWithAllSupportedTypes>(plan);

        trigger.CustomEvent<CustomEventWithAllSupportedTypes>("data-sync", (payload, p) =>
        {
            p.Dispatch("synced", new CustomEventWithAllSupportedTypes
            {
                IntValue = 42,
                LongValue = 9007199254740991L,
                DoubleValue = 3.14159,
                FloatValue = 2.718f,
                DateTimeValue = new DateTime(2026, 3, 8, 14, 30, 0),
                StringValue = "hello",
                DateValue = new DateTime(2026, 3, 8),
                BoolValue = true,
            });
        });

        return VerifyJson(plan.Render());
    }
}
