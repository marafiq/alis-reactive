using System.Text.Json;

namespace Alis.Reactive.UnitTests;

[TestFixture]
public class WhenSerializingUnifiedCommandValues : PlanTestBase
{
    private sealed class CommandValuePayload
    {
        public string? Name { get; set; }
    }

    [Test]
    public void set_prop_literal_value_is_carried_by_the_mutation()
    {
        var json = Build(p => p.Element("status").SetText("loaded")).Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var value = Navigate(doc.RootElement, "entries[0].reaction.commands[0].mutation.value");

        Assert.That(value.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(value.GetProperty("value").GetString(), Is.EqualTo("loaded"));
    }

    [Test]
    public void set_prop_source_value_is_carried_by_the_mutation()
    {
        var json = BuildWithPayload((payload, p) => p.Element("echo").SetText(payload, x => x.Name)).Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var value = Navigate(doc.RootElement, "entries[0].reaction.commands[0].mutation.value");

        Assert.That(value.GetProperty("kind").GetString(), Is.EqualTo("source"));
        Assert.That(value.GetProperty("source").GetProperty("kind").GetString(), Is.EqualTo("event"));
        Assert.That(value.GetProperty("source").GetProperty("path").GetString(), Is.EqualTo("evt.name"));
    }

    [Test]
    public void dispatch_payload_fields_are_described_as_command_values()
    {
        var json = Build(p => p.Dispatch("saved", new { status = "ok", count = 5 })).Render();
        AssertSchemaValid(json);

        using var doc = JsonDocument.Parse(json);
        var status = Navigate(doc.RootElement, "entries[0].reaction.commands[0].payload.status");
        var count = Navigate(doc.RootElement, "entries[0].reaction.commands[0].payload.count");

        Assert.That(status.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(status.GetProperty("value").GetString(), Is.EqualTo("ok"));
        Assert.That(count.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(count.GetProperty("value").GetInt32(), Is.EqualTo(5));
    }

    private static ReactivePlan<TestModel> Build(Action<Builders.PipelineBuilder<TestModel>> pipeline)
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(pipeline);
        return plan;
    }

    private static ReactivePlan<TestModel> BuildWithPayload(
        Action<CommandValuePayload, Builders.PipelineBuilder<TestModel>> pipeline)
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent("changed", p => pipeline(new CommandValuePayload(), p));
        return plan;
    }

    private static JsonElement Navigate(JsonElement root, string path)
    {
        var current = root;
        foreach (var segment in path.Split('.'))
        {
            if (segment.Contains('['))
            {
                var bracketIndex = segment.IndexOf('[');
                var property = segment[..bracketIndex];
                var index = int.Parse(segment[(bracketIndex + 1)..^1]);
                if (property.Length > 0)
                {
                    current = current.GetProperty(property);
                }

                current = current[index];
                continue;
            }

            current = current.GetProperty(segment);
        }

        return current;
    }
}
