namespace Alis.Reactive.UnitTests.Http;

[TestFixture]
public class WhenBuildingHttpRequests : PlanTestBase
{
    [Test]
    public void static_null_payload_is_an_explicit_unshaped_literal()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/profile")
                .Gather(g => g.Static("optional", null!))
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = System.Text.Json.JsonDocument.Parse(planJson);
        var literal = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input")
            .GetProperty("value")
            .GetProperty("fields")
            .GetProperty("optional");

        Assert.That(literal.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(literal.GetProperty("value").ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Null));
        Assert.That(literal.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("none"));
    }

    [Test]
    public void string_component_gather_requires_registered_shape()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/profile")
                    .Gather(g => g.Include("missing-editor", "fusion", "PhoneNumber", "value"))
                    .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });

        Assert.That(ex!.Message, Does.Contain("Component 'missing-editor' is not registered"));
        Assert.That(ex.Message, Does.Contain("registered input helper"));
        Assert.That(ex.Message, Does.Contain("typed component source"));
    }

    [Test]
    public void string_component_gather_must_match_the_registered_value_member()
    {
        var plan = CreatePlan();
        RegisterInput(plan, "Id", "notify-input", "checked", Alis.Reactive.PlanModel.Shape.Boolean);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/profile")
                    .Gather(g => g.Include("notify-input", "native", "ReceiveNotifications", "value"))
                    .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });

        Assert.That(ex!.Message, Does.Contain("notify-input"));
        Assert.That(ex.Message, Does.Contain("checked"));
        Assert.That(ex.Message, Does.Contain("value"));
    }

    [Test]
    public void include_all_does_not_duplicate_a_component_read_with_an_explicit_payload_key()
    {
        var plan = CreatePlan();
        RegisterTextInput(plan, "Id", "id-input");
        var idSource = new Alis.Reactive.Builders.Conditions.TypedComponentSource<string>("id-input", "value");

        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/profile", g => g
                    .Include(idSource, "selectedId")
                    .IncludeAll())
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = System.Text.Json.JsonDocument.Parse(planJson);
        var fields = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input")
            .GetProperty("payloadFields")
            .EnumerateArray()
            .Select(field => field.GetProperty("key").GetString())
            .ToList();

        Assert.That(fields, Is.EqualTo(new[] { "selectedId" }));
    }

    [Test]
    public void include_all_does_not_duplicate_a_registered_input_claimed_by_static_payload()
    {
        var plan = CreatePlan();
        RegisterTextInput(plan, "Id", "id-input");

        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/profile", g => g
                    .Static("Id", "manual")
                    .IncludeAll())
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = System.Text.Json.JsonDocument.Parse(planJson);
        var input = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input");

        Assert.That(input.GetProperty("kind").GetString(), Is.EqualTo("gather"));
        Assert.That(input.GetProperty("payloadFields").GetArrayLength(), Is.EqualTo(0));
        Assert.That(input.GetProperty("selection").GetProperty("kind").GetString(),
            Is.EqualTo("all-registered-inputs"));
        Assert.That(input.GetProperty("supplementalFields")
                .GetProperty("kind")
                .GetString(),
            Is.EqualTo("declared"));
        Assert.That(input.GetProperty("supplementalFields")
                .GetProperty("value")
                .GetProperty("fields")
                .TryGetProperty("Id", out _),
            Is.True);
    }

    [Test]
    public void include_all_does_not_add_registered_input_when_static_payload_claims_a_nested_path()
    {
        var plan = CreatePlan();
        RegisterTextInput(plan, "Address", "address-input");

        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/profile", g => g
                    .Static("Address.City", "Seattle")
                    .IncludeAll())
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = System.Text.Json.JsonDocument.Parse(planJson);
        var components = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input")
            .GetProperty("payloadFields");

        Assert.That(components.GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public void include_all_without_build_time_fields_still_emits_gather_input_for_runtime_partials()
    {
        var plan = CreatePlan();

        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/profile", g => g.IncludeAll())
                .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = System.Text.Json.JsonDocument.Parse(planJson);
        var input = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request")
            .GetProperty("input");

        Assert.That(input.GetProperty("kind").GetString(), Is.EqualTo("gather"));
        Assert.That(input.GetProperty("payloadFields").GetArrayLength(), Is.EqualTo(0));
        Assert.That(input.GetProperty("selection").GetProperty("kind").GetString(),
            Is.EqualTo("all-registered-inputs"));
    }

    [Test]
    public void parallel_branch_must_select_http_endpoint()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Parallel(_ => { });
            });
        });

        Assert.That(ex!.Message, Does.Contain("HTTP request endpoint was not selected"));
    }

    [Test]
    public void parallel_requires_at_least_one_http_branch()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Parallel();
            });
        });

        Assert.That(ex!.Message, Does.Contain("at least one HTTP request branch"));
    }

    [Test]
    public void chained_request_must_select_http_endpoint()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/first")
                 .Response(r => r.Chained(_ => { }));
            });
        });

        Assert.That(ex!.Message, Does.Contain("HTTP request endpoint was not selected"));
    }

    [Test]
    public void response_can_declare_only_one_chained_request()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/start")
                 .Response(r => r
                    .Chained(c => c.Post("/api/step-1"))
                    .Chained(c => c.Post("/api/step-2")));
            });
        });

        Assert.That(ex!.Message, Does.Contain("only one chained request"));
        Assert.That(ex.Message, Does.Contain("existing follow-up request"));
    }

    [TestCase(99)]
    [TestCase(600)]
    public void response_status_match_must_be_standard_http_status_code(int statusCode)
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/profile")
                 .Response(r => r.OnError(statusCode, e => e.Element("error").Show()));
            });
        });

        Assert.That(ex!.Message, Does.Contain("between 100 and 599"));
        Assert.That(ex.Message, Does.Contain("without a status code"));
    }

    private static void RegisterTextInput(
        ReactivePlan<TestModel> plan,
        string bindingPath,
        string componentId)
    {
        RegisterInput(plan, bindingPath, componentId, "value", Alis.Reactive.PlanModel.Shape.String);
    }

    private static void RegisterInput(
        ReactivePlan<TestModel> plan,
        string bindingPath,
        string componentId,
        string valueMember,
        Alis.Reactive.PlanModel.Shape shape)
    {
        var identity = RegisteredComponentIdentity.For(componentId, "native");
        var binding = RegisteredComponentBinding.For(bindingPath, valueMember);
        var componentKind = Alis.Reactive.PlanModel.ComponentKind.Of("textbox");

        plan.RegisterInputComponent(ComponentRegistration.RegisteredInput(
                identity,
                binding,
                componentKind,
                shape));
    }
}
