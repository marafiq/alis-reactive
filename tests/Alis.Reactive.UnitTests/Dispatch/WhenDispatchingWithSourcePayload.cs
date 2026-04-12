using System.Text.Json;

namespace Alis.Reactive.UnitTests.Dispatch;

public class NestedPayload
{
    public string Name { get; set; } = "";
    public NestedAddress Address { get; set; }
}

public class NestedAddress
{
    public string City { get; set; } = "";
    public string Zip { get; set; } = "";
}

/// <summary>
/// Verifies that source-backed dispatch payloads serialize correctly,
/// including nested property paths that expand to nested JSON objects.
/// </summary>
[TestFixture]
public class WhenDispatchingWithSourcePayload : PlanTestBase
{
    [Test]
    public void flat_literal_field_produces_object_with_literal_producer()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Dispatch<TestModel>("transfer", d => d
                .Set(x => x.Id, "abc-123")
            );
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"dispatch\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"object\""));
        Assert.That(planJson, Does.Contain("\"abc-123\""));
    }

    [Test]
    public void nested_expression_produces_nested_json_object_not_flat_key()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Dispatch<NestedPayload>("transfer", d => d
                .Set(x => x.Name, "John")
                .Set(x => x.Address.City, "Seattle")
                .Set(x => x.Address.Zip, "98101")
            );
        });

        var planJson = plan.RenderFormatted();
        TestContext.Out.WriteLine(planJson);
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        // Reaction may be wrapped in a sequence — find the dispatch step
        var reaction = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction");
        var dispatch = reaction.GetProperty("kind").GetString() == "dispatch"
            ? reaction
            : reaction.GetProperty("steps")[0];
        var data = dispatch.GetProperty("data");

        Assert.That(data.GetProperty("kind").GetString(), Is.EqualTo("object"));

        var fields = data.GetProperty("fields");

        // Top-level field
        Assert.That(fields.TryGetProperty("name", out _), Is.True, "Expected top-level 'name'");

        // Nested: address must be an object, not a flat "address.city" key
        Assert.That(fields.TryGetProperty("address.city", out _), Is.False,
            "Dotted key 'address.city' must NOT exist — should be nested");
        Assert.That(fields.TryGetProperty("address", out var address), Is.True,
            "Expected nested 'address' object");
        Assert.That(address.GetProperty("kind").GetString(), Is.EqualTo("object"));

        var addressFields = address.GetProperty("fields");
        Assert.That(addressFields.TryGetProperty("city", out _), Is.True);
        Assert.That(addressFields.TryGetProperty("zip", out _), Is.True);
    }

    [Test]
    public void empty_payload_throws()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Dispatch<TestModel>("transfer", d => { });
            });
        });
    }
}
