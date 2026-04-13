using System.Linq;
using System.Text.Json;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests.Http;

/// <summary>
/// Verifies that URL template route parameters serialize correctly, validate at build time,
/// and compose with headers and body gather.
/// </summary>
[TestFixture]
public class WhenSettingRouteParams : PlanTestBase
{
    public class EventArgs
    {
        public int ResidentId { get; set; }
        public string? Name { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class NullableArgs
    {
        public int? Count { get; set; }
    }

    // ── Per-overload tests ───────────────────────────────

    [Test]
    public void literal_int_route_param_produces_correct_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{id}")
             .Gather(g => g.RouteParam("id", 42))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request");
        var routeParam = request.GetProperty("routeParams").GetProperty("id");
        Assert.That(routeParam.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(routeParam.GetProperty("value").GetInt32(), Is.EqualTo(42));
        Assert.That(routeParam.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("number"));
    }

    [Test]
    public void literal_string_route_param_produces_correct_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data/{slug}")
             .Gather(g => g.RouteParam("slug", "hello-world"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"hello-world\""));
    }

    [Test]
    public void literal_long_route_param_produces_number_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/records/{recordId}")
             .Gather(g => g.RouteParam("recordId", 9999999999L))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"kind\": \"number\""));
    }

    [Test]
    public void typed_source_route_param_produces_component_read()
    {
        var plan = CreatePlan();
        var source = new TypedComponentSource<int>("resident-ddl", "fusion", "value");

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{id}")
             .Gather(g => g.RouteParam("id", source))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"read\""));
        Assert.That(planJson, Does.Contain("\"component\": \"resident-ddl\""));
        Assert.That(planJson, Does.Contain("\"member\": \"value\""));
    }

    [Test]
    public void event_arg_route_param_carries_shape()
    {
        var plan = CreatePlan();
        var args = default(EventArgs)!;

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{residentId}")
             .Gather(g => g
                 .RouteParam("residentId", args, a => a.ResidentId)
                 .Static("dummy", "x"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request");
        var routeParam = request.GetProperty("routeParams").GetProperty("residentId");
        Assert.That(routeParam.GetProperty("kind").GetString(), Is.EqualTo("read"));
        Assert.That(routeParam.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("number"));
    }

    // ── Absence + composition ────────────────────────────

    [Test]
    public void plan_without_route_params_emits_empty_object()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request");
        var routeParams = request.GetProperty("routeParams");
        Assert.That(routeParams.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(routeParams.EnumerateObject().Any(), Is.False,
            "RouteParams should be present as an empty object");
    }

    [Test]
    public void multiple_route_params_all_appear()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/facilities/{facilityId}/residents/{residentId}")
             .Gather(g => g
                 .RouteParam("facilityId", 7)
                 .RouteParam("residentId", 99))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"facilityId\""));
        Assert.That(planJson, Does.Contain("\"residentId\""));
    }

    [Test]
    public void route_params_and_headers_coexist()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{id}")
             .Gather(g => g
                 .RouteParam("id", 42)
                 .Header("X-Version", "v2"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"routeParams\""));
        Assert.That(planJson, Does.Contain("\"headers\""));
    }

    [Test]
    public void route_params_with_body_gather_coexist()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{id}")
             .Gather(g => g
                 .RouteParam("id", 42)
                 .Static("filter", "active"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"routeParams\""));
        Assert.That(planJson, Does.Contain("\"filter\""));
    }

    [Test]
    public void route_params_on_chained_requests_independent()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/residents/{id}")
             .Gather(g => g.RouteParam("id", 42))
             .Response(r => r
                .OnSuccess(s => s.Element("step1").Show())
                .Chained(c => c
                    .Get("/api/facilities/{facilityId}")
                    .Gather(g2 => g2.RouteParam("facilityId", 7))
                    .Response(r2 => r2.OnSuccess(s2 => s2.Element("step2").Show()))
                )
             );
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"id\""));
        Assert.That(planJson, Does.Contain("\"facilityId\""));
    }

    // ── Guard tests ──────────────────────────────────────

    [Test]
    public void array_route_param_throws_at_build_time()
    {
        var plan = CreatePlan();
        var args = default(EventArgs)!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{tags}")
                 .Gather(g => g.RouteParam("tags", args, a => a.Tags))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("scalar"));
        Assert.That(ex.Message, Does.Contain("tags"));
    }

    [Test]
    public void null_string_route_param_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentNullException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{slug}")
                 .Gather(g => g.RouteParam("slug", (string)null!))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void empty_param_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{}")
                 .Gather(g => g.RouteParam("", 42))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void whitespace_param_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{id}")
                 .Gather(g => g.RouteParam("  ", 42))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void hyphenated_param_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{resident-id}")
                 .Gather(g => g.RouteParam("resident-id", 42))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("invalid characters"));
        Assert.That(ex.Message, Does.Contain("resident-id"));
    }

    [Test]
    public void mismatched_param_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/residents/{residentId}")
                 .Gather(g => g.RouteParam("residnetId", 42))  // typo!
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("residnetId"));
        Assert.That(ex.Message, Does.Contain("does not match"));
    }

    [Test]
    public void orphaned_placeholder_throws_at_build_time()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/facilities/{facilityId}/residents/{residentId}")
                 .Gather(g => g.RouteParam("facilityId", 7))  // missing residentId!
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("residentId"));
        Assert.That(ex.Message, Does.Contain("no matching"));
    }

    [Test]
    public void nullable_int_route_param_accepted()
    {
        var plan = CreatePlan();
        var args = default(NullableArgs)!;

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data/{count}")
             .Gather(g => g
                 .RouteParam("count", args, a => a.Count)
                 .Static("dummy", "x"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"count\""));
    }

    [Test]
    public void datetime_typed_source_carries_date_shape()
    {
        var plan = CreatePlan();
        var source = new TypedComponentSource<DateTime>("date-picker", "fusion", "value");

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data/{date}")
             .Gather(g => g.RouteParam("date", source))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request");
        var routeParam = request.GetProperty("routeParams").GetProperty("date");
        Assert.That(routeParam.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("date"));
    }

    [Test]
    public void null_typed_source_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentNullException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{id}")
                 .Gather(g => g.RouteParam("id", (TypedSource<int>)null!))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void duplicate_route_param_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data/{id}")
                 .Gather(g => g
                     .RouteParam("id", 42)
                     .RouteParam("id", 99))  // duplicate!
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("already defined"));
    }
}
