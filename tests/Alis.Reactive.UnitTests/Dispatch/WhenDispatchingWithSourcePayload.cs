using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

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
            p.DispatchWith<TestModel>("transfer", d => d
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
            p.DispatchWith<NestedPayload>("transfer", d => d
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

    /// <summary>
    /// Direct test of ExpandNestedPaths via reflection — the typed DSL prevents
    /// parent/child collisions at compile time, but the guard protects against
    /// future builder surface expansions that might allow scalar overloads for
    /// complex-typed parent properties.
    /// </summary>
    [Test]
    public void expand_nested_paths_throws_when_leaf_overwrites_existing_nested_object()
    {
        // Construct a flat dictionary that simulates the collision:
        // "address.city" added first (creates nested), then "address" added as leaf.
        var flat = new Dictionary<string, ValueProducer>
        {
            ["address.city"] = ValueProducer.Literal("Seattle"),
            ["address"] = ValueProducer.Literal("flat-value-collides")
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => InvokeExpandNestedPaths(flat));
        Assert.That(ex!.Message, Does.Contain("conflict"));
        Assert.That(ex.Message, Does.Contain("address"));
    }

    [Test]
    public void expand_nested_paths_throws_when_deep_leaf_overwrites_existing_nested_object()
    {
        // Deep nesting collision: "address.region.country" nested first, then
        // "address.region" attempted as a leaf — must throw, not overwrite.
        var flat = new Dictionary<string, ValueProducer>
        {
            ["address.region.country"] = ValueProducer.Literal("US"),
            ["address.region"] = ValueProducer.Literal("pacific-northwest")
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => InvokeExpandNestedPaths(flat));
        Assert.That(ex!.Message, Does.Contain("conflict"));
    }

    [Test]
    public void expand_nested_paths_throws_when_leaf_already_set_then_used_as_parent()
    {
        // Reverse ordering: "address" leaf first, then "address.city" nested.
        var flat = new Dictionary<string, ValueProducer>
        {
            ["address"] = ValueProducer.Literal("flat-value"),
            ["address.city"] = ValueProducer.Literal("Seattle")
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => InvokeExpandNestedPaths(flat));
        Assert.That(ex!.Message, Does.Contain("conflict"));
    }

    /// <summary>Invokes the private ExpandNestedPaths method via reflection for direct testing.</summary>
    private static Dictionary<string, ValueProducer> InvokeExpandNestedPaths(
        Dictionary<string, ValueProducer> flat)
    {
        var method = typeof(DispatchPayloadBuilder<NestedPayload, TestModel>)
            .GetMethod("ExpandNestedPaths",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        try
        {
            return (Dictionary<string, ValueProducer>)method!.Invoke(null, new object[] { flat })!;
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }
    }

    [Test]
    public void empty_payload_throws()
    {
        var plan = CreatePlan();
        Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.DispatchWith<TestModel>("transfer", d => { });
            });
        });
    }
}
