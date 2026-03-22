namespace Alis.Reactive.UnitTests;

[TestFixture]
public class AllPlansConformToSchema : PlanTestBase
{
    [Test]
    public void Dispatch_event()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Dispatch("test"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Dispatch_with_payload()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Dispatch("test", new TestModel { Id = "x" }));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Custom_event_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent("evt", p => p.Dispatch("out"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Typed_custom_event_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<TestModel>("evt", (_, p) => p.Dispatch("out"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Empty_plan()
    {
        var plan = CreatePlan();
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Multiple_entries()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .DomReady(p => p.Dispatch("a"))
            .CustomEvent("b", p => p.Dispatch("c"));
        AssertSchemaValid(plan.Render());
    }

    // -- MutateElement commands --

    [Test]
    public void Element_add_class()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("status").AddClass("active"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_remove_class()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("status").RemoveClass("hidden"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_toggle_class()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("menu").ToggleClass("expanded"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_set_text()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("output").SetText("hello"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_set_html()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("container").SetHtml("<b>done</b>"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_show()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("loader").Show());
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Element_hide()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("content").Hide());
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Mixed_dispatch_and_mutate()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("step").AddClass("done");
            p.Dispatch("step-complete");
            p.Element("next").SetText("ready");
        });
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Mutate_in_custom_event_handler()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent("loaded", p =>
        {
            p.Element("spinner").Hide();
            p.Element("content").Show();
            p.Element("status").SetText("loaded");
        });
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Multiple_mutations_same_element()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("panel").RemoveClass("text-muted");
            p.Element("panel").AddClass("text-green");
            p.Element("panel").SetText("success");
        });
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Full_event_chain_with_mutations()
    {
        var plan = CreatePlan();
        Trigger(plan)
            .DomReady(p =>
            {
                p.Element("step-1").AddClass("complete");
                p.Dispatch("init");
            })
            .CustomEvent("init", p =>
            {
                p.Element("step-2").AddClass("complete");
                p.Element("step-2").SetText("init received");
                p.Dispatch("done");
            })
            .CustomEvent("done", p =>
            {
                p.Element("result").RemoveClass("hidden");
                p.Element("result").Show();
                p.Element("result").SetText("all done");
            });
        AssertSchemaValid(plan.Render());
    }

    // -- Source-based payload access --

    [Test]
    public void SetText_with_flat_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("evt", (payload, p) =>
            p.Element("name").SetText(payload, x => x.StringValue));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void SetText_with_nested_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("evt", (payload, p) =>
            p.Element("city").SetText(payload, x => x.Address!.City));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void SetHtml_with_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("evt", (payload, p) =>
            p.Element("content").SetHtml(payload, x => x.StringValue));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void All_primitive_types_from_source()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("evt", (payload, p) =>
        {
            p.Element("int-val").SetText(payload, x => x.IntValue);
            p.Element("long-val").SetText(payload, x => x.LongValue);
            p.Element("double-val").SetText(payload, x => x.DoubleValue);
            p.Element("string-val").SetText(payload, x => x.StringValue);
            p.Element("bool-val").SetText(payload, x => x.BoolValue);
        });
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Mixed_static_and_source_values()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<PayloadModel>("evt", (payload, p) =>
        {
            p.Element("label").SetText("Name:");
            p.Element("value").SetText(payload, x => x.StringValue);
            p.Element("city").SetText(payload, x => x.Address!.City);
            p.Element("status").AddClass("done");
        });
        AssertSchemaValid(plan.Render());
    }

    // -- Real-time triggers --

    [Test]
    public void Server_push_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).ServerPush("/api/stream", p => p.Dispatch("update"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void Server_push_trigger_with_event_type()
    {
        var plan = CreatePlan();
        Trigger(plan).ServerPush("/api/stream", "notification", p =>
            p.Element("msg").SetText("received"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void SignalR_trigger()
    {
        var plan = CreatePlan();
        Trigger(plan).SignalR("/hubs/data", "ReceiveUpdate", p => p.Dispatch("update"));
        AssertSchemaValid(plan.Render());
    }

    [Test]
    public void SignalR_trigger_with_typed_payload()
    {
        var plan = CreatePlan();
        Trigger(plan).SignalR<TestModel>("/hubs/data", "ReceiveData", (payload, p) =>
            p.Element("value").SetText(payload, x => x.Id));
        AssertSchemaValid(plan.Render());
    }
}
