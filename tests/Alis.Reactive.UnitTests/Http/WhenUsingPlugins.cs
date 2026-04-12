using System.Text.Json;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive.UnitTests.Http;

[TestFixture]
public class WhenUsingPlugins : PlanTestBase
{
    [Test]
    public void plugin_read_produces_plugin_source()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count");
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
        Assert.That(json, Does.Contain("\"name\": \"array\""));
    }

    [Test]
    public void plugin_string_carries_shape()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("auth", "getToken");
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"string\""));
    }

    [Test]
    public void plugin_int_carries_shape()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count");
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"number\""));
    }

    [Test]
    public void plugin_bool_carries_shape()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<bool>("isAdmin"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<bool> src = p.Plugin<bool>("auth", "isAdmin");
            p.When(src).Truthy().Then(t => t.Element("x").Show());
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"boolean\""));
    }

    [Test]
    public void plugin_in_condition()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<bool>("isAdmin"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<bool> src = p.Plugin<bool>("auth", "isAdmin");
            p.When(src).Truthy().Then(t => t.Element("panel").Show());
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"compare\""));
    }

    [Test]
    public void plugin_in_set_text()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("prefs", p => p.Method<string>("getTheme"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("prefs", "getTheme");
            p.Element("theme").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"set\""));
    }

    [Test]
    public void plugin_in_header()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("auth", "getToken");
            p.Get("/api/data")
             .Gather(g => g.Header("Authorization", src))
             .Response(r => r.OnSuccess(s => s.Element("x").Show()));
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"Authorization\""));
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
    }

    [Test]
    public void plugin_in_route_param()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<int>("getTenantId"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("auth", "getTenantId");
            p.Get("/api/tenants/{tenantId}")
             .Gather(g => g.RouteParam("tenantId", src))
             .Response(r => r.OnSuccess(s => s.Element("x").Show()));
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"routeParams\""));
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
    }

    [Test]
    public void plugin_read_with_typed_source_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        var component = new TypedComponentSource<string>("ddl", "fusion", "value");
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count").Arg(component);
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"args\""));
        Assert.That(json, Does.Contain("\"component\": \"ddl\""));
    }

    [Test]
    public void plugin_read_with_literal_string_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count").Arg("test");
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"args\""));
        Assert.That(json, Does.Contain("\"test\""));
    }

    [Test]
    public void plugin_read_with_literal_int_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count").Arg(42);
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("42"));
    }

    [Test]
    public void plugin_void_call_fire()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("logger", p => p.Void("flush"));
        Trigger(plan).DomReady(p =>
        {
            p.Plugin("logger", "flush").Fire();
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"call\""));
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
        Assert.That(json, Does.Contain("\"method\": \"flush\""));
    }

    [Test]
    public void plugin_void_call_with_arg_fire()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("analytics", p => p.Void("track"));
        Trigger(plan).DomReady(p =>
        {
            p.Plugin("analytics", "track").Arg("pageView").Fire();
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"kind\": \"call\""));
        Assert.That(json, Does.Contain("\"pageView\""));
    }

    [Test]
    public void plugin_gather_from_typed_source()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("auth", "getToken");
            p.Get("/api/data")
             .Gather(g => g.Plugin(src, "token"))
             .Response(r => r.OnSuccess(s => s.Element("x").Show()));
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"key\": \"token\""));
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
    }

    [Test]
    public void plugin_auto_registers_jstype()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("auth", "getToken");
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Contain("\"plugin.auth\""));
    }

    [Test]
    public void plan_without_plugins_clean()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("x").Show());
        var json = plan.RenderFormatted();
        AssertSchemaValid(json);
        Assert.That(json, Does.Not.Contain("\"plugin\""));
    }

    [Test]
    public void empty_plugin_name_throws()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Assert.Throws<System.ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Plugin<string>("", "getToken");
            });
        });
    }

    [Test]
    public void empty_member_throws()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Method<string>("getToken"));
        Assert.Throws<System.ArgumentException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                p.Plugin<string>("auth", "");
            });
        });
    }
}
