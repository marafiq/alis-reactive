using System.Linq;
using System.Text.Json;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests.Http;

/// <summary>
/// Verifies that custom headers on HTTP requests serialize correctly.
/// </summary>
[TestFixture]
public class WhenSettingCustomHeaders : PlanTestBase
{
    public class EventArgs
    {
        public string CorrelationId { get; set; } = "";
        public int StatusCode { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    [Test]
    public void literal_header_produces_correct_plan_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.Header("X-Api-Version", "2024-01-15"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"headers\""));
        Assert.That(planJson, Does.Contain("\"X-Api-Version\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"literal\""));
        Assert.That(planJson, Does.Contain("\"2024-01-15\""));
    }

    [Test]
    public void multiple_headers_all_appear_in_plan_json()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/save")
             .Gather(g => g
                 .Header("X-Api-Version", "v2")
                 .Header("X-Tenant-Id", "tenant-42")
                 .Header("X-Request-Id", "req-abc")
                 .Static("name", "test"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"X-Api-Version\""));
        Assert.That(planJson, Does.Contain("\"X-Tenant-Id\""));
        Assert.That(planJson, Does.Contain("\"X-Request-Id\""));
    }

    [Test]
    public void plan_without_headers_emits_empty_headers_object()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request");
        var headers = request.GetProperty("headers");
        Assert.That(headers.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(headers.EnumerateObject().Any(), Is.False,
            "Headers should be present as an empty object");
    }

    [Test]
    public void headers_on_chained_requests_serialize_independently()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/first")
             .Gather(g => g.Header("X-Step", "one"))
             .Response(r => r
                .OnSuccess(s => s.Element("step1").Show())
                .Chained(c => c
                    .Post("/api/second")
                    .Gather(g2 => g2.Header("X-Step", "two"))
                    .Response(r2 => r2.OnSuccess(s2 => s2.Element("step2").Show()))
                )
             );
        });

        var planJson = plan.RenderFormatted();

        // Both "one" and "two" header values must be present
        Assert.That(planJson, Does.Contain("\"one\""));
        Assert.That(planJson, Does.Contain("\"two\""));
    }

    [Test]
    public void header_value_is_valueproducer_with_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.Header("X-Version", "1.0"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request");

        Assert.That(request.TryGetProperty("headers", out var headers), Is.True);
        Assert.That(headers.TryGetProperty("X-Version", out var headerValue), Is.True);
        Assert.That(headerValue.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(headerValue.GetProperty("value").GetString(), Is.EqualTo("1.0"));
        Assert.That(headerValue.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void event_arg_header_carries_shape_from_expression()
    {
        var plan = CreatePlan();
        var args = default(EventArgs)!;
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Gather(g => g
                 .Header("X-Correlation", args, a => a.CorrelationId)
                 .Static("name", "test"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        // Event arg header reads from payload scope "event"
        Assert.That(planJson, Does.Contain("\"X-Correlation\""));
        Assert.That(planJson, Does.Contain("\"scope\": \"event\""));
        Assert.That(planJson, Does.Contain("\"correlationId\""));
        // Shape should be string (from typeof(string))
        Assert.That(planJson, Does.Contain("\"kind\": \"string\""));
    }

    [Test]
    public void event_arg_header_with_int_carries_number_shape()
    {
        var plan = CreatePlan();
        var args = default(EventArgs)!;
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Gather(g => g
                 .Header("X-Status", args, a => a.StatusCode)
                 .Static("name", "test"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        using var doc = JsonDocument.Parse(planJson);
        var request = doc.RootElement
            .GetProperty("behaviors")[0]
            .GetProperty("reaction")
            .GetProperty("request");

        var headerValue = request.GetProperty("headers").GetProperty("X-Status");
        Assert.That(headerValue.GetProperty("kind").GetString(), Is.EqualTo("read"));
        Assert.That(headerValue.GetProperty("shape").GetProperty("kind").GetString(), Is.EqualTo("number"));
    }

    [Test]
    public void array_typed_header_throws_at_build_time()
    {
        var plan = CreatePlan();
        var args = default(EventArgs)!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Post("/api/data")
                 .Gather(g => g.Header("X-Tags", args, a => a.Tags))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("scalar"));
        Assert.That(ex.Message, Does.Contain("X-Tags"));
    }

    [Test]
    public void empty_header_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.Header("", "value"))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void whitespace_header_name_throws_at_build_time()
    {
        var plan = CreatePlan();

        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.Header("  ", "value"))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void nullable_int_header_is_accepted_as_scalar()
    {
        // Nullable<int> should pass the scalar check — it's nullable(number)
        var plan = CreatePlan();
        var args = default(NullableArgs)!;
        Trigger(plan).DomReady(p =>
        {
            p.Post("/api/data")
             .Gather(g => g
                 .Header("X-Count", args, a => a.Count)
                 .Static("name", "test"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"X-Count\""));
    }

    [Test]
    public void null_literal_header_value_throws_at_build_time()
    {
        var plan = CreatePlan();

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.Header("X-Token", (string)null!))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("X-Token"));
    }

    [Test]
    public void typed_source_header_produces_component_read()
    {
        // TypedComponentSource<T> is the production path for component.Value() reads
        var plan = CreatePlan();
        var source = new Alis.Reactive.Builders.Conditions.TypedComponentSource<string>(
            "tenant-ddl", "value");

        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.Header("X-Tenant", source))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();

        Assert.That(planJson, Does.Contain("\"X-Tenant\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"read\""));
        Assert.That(planJson, Does.Contain("\"component\": \"tenant-ddl\""));
        Assert.That(planJson, Does.Contain("\"member\": \"value\""));
    }

    public class NullableArgs
    {
        public int? Count { get; set; }
    }
}
