using System.Text.Json;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests.Http;

/// <summary>
/// Verifies that URL query parameter reads serialize correctly, validate at build time,
/// and compose with headers, route params, and conditions.
/// </summary>
[TestFixture]
public class WhenReadingUrlParams : PlanTestBase
{
    public class EventArgs
    {
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class MyDto
    {
        public string Name { get; set; } = "";
    }

    // ── Per-overload: PipelineBuilder.FromUrl ─────────────────

    [Test]
    public void from_url_string_produces_url_source_read()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("result").SetText(p.FromUrl("tab"));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
        Assert.That(planJson, Does.Contain("\"member\": \"tab\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"string\""));
    }

    [Test]
    public void from_url_typed_int_carries_number_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.When(p.FromUrl<int>("page")).Gt(1)
             .Then(t => t.Element("prev").Show());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
        Assert.That(planJson, Does.Contain("\"member\": \"page\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"number\""));
    }

    [Test]
    public void from_url_typed_bool_carries_boolean_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.When(p.FromUrl<bool>("active")).Truthy()
             .Then(t => t.Element("badge").Show());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"kind\": \"boolean\""));
    }

    [Test]
    public void from_url_typed_datetime_carries_date_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("since").SetText(p.FromUrl<DateTime>("since"));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"kind\": \"date\""));
    }

    // ── Per-overload: GatherBuilder.FromUrl ───────────────────

    [Test]
    public void from_url_gather_produces_gather_field()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.FromUrl("facilityId"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"key\": \"facilityId\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
    }

    [Test]
    public void from_url_gather_with_alias_uses_alias_as_key()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.FromUrl("unitId", "unit"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"key\": \"unit\""));
    }

    // ── Conditions ───────────────────────────────────────────

    [Test]
    public void from_url_in_condition_produces_compare()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.When(p.FromUrl("tab")).Eq("meds")
             .Then(t => t.Element("panel").Show());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"compare\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
        Assert.That(planJson, Does.Contain("\"op\": \"eq\""));
    }

    [Test]
    public void from_url_typed_in_condition_produces_shaped_compare()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.When(p.FromUrl<int>("page")).Gt(1)
             .Then(t => t.Element("prev").Show());
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        // The condition's left ValueProducer carries number shape from FromUrl<int>
        Assert.That(planJson, Does.Contain("\"op\": \"gt\""));
        Assert.That(planJson, Does.Contain("\"member\": \"page\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"number\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
    }

    // ── Pipeline ─────────────────────────────────────────────

    [Test]
    public void from_url_in_set_text_produces_set_reaction()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("display").SetText(p.FromUrl("tab"));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        Assert.That(planJson, Does.Contain("\"kind\": \"set\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
    }

    // ── Absence ──────────────────────────────────────────────

    [Test]
    public void plan_without_url_source_has_no_url_kind()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Element("result").Show();
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Not.Contain("\"kind\": \"url\""));
    }

    // ── Composition ──────────────────────────────────────────

    [Test]
    public void from_url_composes_with_route_params_and_headers()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data/{id}")
             .Gather(g => g
                 .RouteParam("id", 42)
                 .Header("X-Tab", p.FromUrl("tab"))
                 .FromUrl("facilityId"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"routeParams\""));
        Assert.That(planJson, Does.Contain("\"headers\""));
        Assert.That(planJson, Does.Contain("\"kind\": \"url\""));
    }

    [Test]
    public void from_url_as_route_param_value()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data/{facilityId}")
             .Gather(g => g.RouteParam("facilityId", p.FromUrl<int>("facilityId")))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var routeParam = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request")
            .GetProperty("routeParams").GetProperty("facilityId");
        Assert.That(routeParam.GetProperty("kind").GetString(), Is.EqualTo("read"));
        Assert.That(routeParam.GetProperty("from").GetProperty("kind").GetString(), Is.EqualTo("url"));
    }

    [Test]
    public void from_url_as_header_value()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.Header("X-Tab", p.FromUrl("tab")))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var header = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request")
            .GetProperty("headers").GetProperty("X-Tab");
        Assert.That(header.GetProperty("from").GetProperty("kind").GetString(), Is.EqualTo("url"));
    }

    // ── Typed gather ─────────────────────────────────────────

    [Test]
    public void from_url_typed_int_gather_carries_number_shape()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.FromUrl<int>("page"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);

        using var doc = JsonDocument.Parse(planJson);
        var field = doc.RootElement.GetProperty("behaviors")[0]
            .GetProperty("reaction").GetProperty("request")
            .GetProperty("input").GetProperty("components")[0];
        Assert.That(field.GetProperty("value").GetProperty("shape")
            .GetProperty("kind").GetString(), Is.EqualTo("number"));
    }

    [Test]
    public void from_url_typed_gather_with_alias()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p =>
        {
            p.Get("/api/data")
             .Gather(g => g.FromUrl<int>("page", "pageNum"))
             .Response(r => r.OnSuccess(s => s.Element("result").Show()));
        });

        var planJson = plan.RenderFormatted();
        AssertSchemaValid(planJson);
        Assert.That(planJson, Does.Contain("\"key\": \"pageNum\""));
    }

    // ── Guard tests ──────────────────────────────────────────

    [Test]
    public void empty_param_name_throws_in_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Element("x").SetText(p.FromUrl(""));
            });
        });
    }

    [Test]
    public void whitespace_param_name_throws_in_pipeline()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Element("x").SetText(p.FromUrl("  "));
            });
        });
    }

    [Test]
    public void empty_param_name_throws_in_gather()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.FromUrl(""))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void whitespace_param_name_throws_in_gather()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.FromUrl("  "))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void empty_alias_throws_in_gather()
    {
        var plan = CreatePlan();
        Assert.Throws<ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.FromUrl("tab", ""))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
    }

    [Test]
    public void array_type_throws_in_from_url()
    {
        var plan = CreatePlan();
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Element("x").SetText(p.FromUrl<string[]>("tags"));
            });
        });
        Assert.That(ex!.Message, Does.Contain("not supported"));
    }

    [Test]
    public void object_type_throws_in_from_url()
    {
        var plan = CreatePlan();
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Element("x").SetText(p.FromUrl<MyDto>("data"));
            });
        });
        Assert.That(ex!.Message, Does.Contain("not supported"));
    }

    [Test]
    public void array_type_throws_in_gather_from_url()
    {
        var plan = CreatePlan();
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.FromUrl<string[]>("tags"))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("scalar"));
    }

    [Test]
    public void object_type_throws_in_gather_from_url()
    {
        var plan = CreatePlan();
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Get("/api/data")
                 .Gather(g => g.FromUrl<MyDto>("data"))
                 .Response(r => r.OnSuccess(s => s.Element("result").Show()));
            });
        });
        Assert.That(ex!.Message, Does.Contain("scalar"));
    }
}
