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
        Assert.That(json, Does.Contain("\"routeParams\""));
        Assert.That(json, Does.Contain("\"kind\": \"plugin\""));
    }

    [Test]
    public void plugin_read_with_typed_source_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("count"));
        var component = new TypedComponentSource<string>("ddl", "value");
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count").Arg(component);
            p.Element("x").SetText(src);
        });
        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"args\""));
        Assert.That(json, Does.Contain("\"component\": \"ddl\""));
    }

    [Test]
    public void plugin_nested_member_uses_same_path_for_registration_and_reference()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int>("stats.count"));

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "stats.count").Arg("active");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"member\": \"stats.count\""));
        Assert.That(json, Does.Contain("\"name\": \"stats\""));
        Assert.That(json, Does.Contain("\"name\": \"count\""));
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
        Assert.That(json, Does.Contain("42"));
    }

    [Test]
    public void plugin_read_with_literal_decimal_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("pricing", p => p.Method<int, decimal>("rank"));
        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("pricing", "rank").Arg(12.5m);
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("12.5"));
        Assert.That(json, Does.Contain("\"kind\": \"number\""));
    }

    [Test]
    public void plugin_read_with_shaped_literal_array_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "array",
            p => p.Method<int>("rank", args => args.Arg<string[]>()));

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "rank")
                .ArgValue(new[] { "active", "pending" });
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        using var document = JsonDocument.Parse(json);
        Assert.That(TryFindLiteralWithShape(document.RootElement, "array", out var argument), Is.True);

        Assert.That(argument.GetProperty("kind").GetString(), Is.EqualTo("literal"));
        Assert.That(argument.GetProperty("value")[0].GetString(), Is.EqualTo("active"));
        Assert.That(argument.GetProperty("value")[1].GetString(), Is.EqualTo("pending"));
        var shape = argument.GetProperty("shape");
        Assert.That(shape.GetProperty("kind").GetString(), Is.EqualTo("array"));
        Assert.That(shape.GetProperty("item").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void plugin_read_with_shaped_literal_array_arg_preserves_exact_shape_validation()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "array",
            p => p.Method<int>("rank", args => args.Arg<string[]>()));

        var exception = Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin<int>("array", "rank")
                    .ArgValue(new[] { 1, 2 });
                p.Element("x").SetText(src);
            });
        });

        Assert.That(exception!.Message, Does.Contain("array<string>"));
        Assert.That(exception.Message, Does.Contain("array<number>"));
    }

    [Test]
    public void plugin_read_with_any_array_contract_accepts_specific_literal_array_shape()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "array",
            p => p.Method<int>("count", args => args.Arg<object[]>()));

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "count")
                .ArgValue(new[] { "active", "pending" });
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        using var document = JsonDocument.Parse(json);
        Assert.That(TryFindLiteralWithShape(document.RootElement, "array", out var argument), Is.True);
        Assert.That(argument.GetProperty("shape").GetProperty("item").GetProperty("kind").GetString(), Is.EqualTo("string"));
    }

    [Test]
    public void plugin_command_with_literal_date_arg()
    {
        var plan = CreatePlan();
        var scheduledFor = new System.DateTime(2026, 1, 2, 3, 4, 5, System.DateTimeKind.Utc);
        plan.RegisterPlugin("calendar", p => p.Void<System.DateTime>("mark"));
        Trigger(plan).DomReady(p =>
        {
            p.Plugin("calendar", "mark").Arg(scheduledFor).Fire();
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain(scheduledFor.ToString("O")));
        Assert.That(json, Does.Contain("\"kind\": \"date\""));
    }

    [Test]
    public void plugin_command_with_shaped_literal_array_arg()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "analytics",
            p => p.Void("record", args => args.Arg<string[]>()));

        Trigger(plan).DomReady(p =>
        {
            p.Plugin("analytics", "record")
                .ArgValue(new[] { "pageView", "signup" })
                .Fire();
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"method\": \"record\""));
        Assert.That(json, Does.Contain("\"pageView\""));
        Assert.That(json, Does.Contain("\"kind\": \"array\""));
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
        Assert.That(json, Does.Contain("\"payloadPath\": \"token\""));
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
        Assert.That(json, Does.Contain("\"plugin.auth\""));
    }

    [Test]
    public void root_plugin_function_uses_declared_contract()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("slugify", p => p.Function<string, string>());

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin<string>("slugify").Arg("John Doe");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.slugify\""));
        Assert.That(json, Does.Contain("\"member\": \"$call\""));
        Assert.That(json, Does.Contain("\"path\": []"));
    }

    [Test]
    public void root_plugin_command_fires_with_declared_contract()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("track", p => p.Void<string>());

        Trigger(plan).DomReady(p =>
        {
            p.Plugin("track").Arg("pageView").Fire();
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.track\""));
        Assert.That(json, Does.Contain("\"method\": \"$call\""));
        Assert.That(json, Does.Contain("\"pageView\""));
    }

    [Test]
    public void typed_root_plugin_function_uses_declared_contract()
    {
        var plan = CreatePlan();
        var slugify = new SlugifyPlugin();
        plan.RegisterPlugin(slugify);

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<string> src = p.Plugin(slugify.Invoke).Arg("John Doe");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.slugify\""));
        Assert.That(json, Does.Contain("\"member\": \"$call\""));
        Assert.That(json, Does.Contain("\"John Doe\""));
    }

    [Test]
    public void typed_plugin_function_uses_declared_contract()
    {
        var plan = CreatePlan();
        var arrays = new ArrayPlugin();
        plan.RegisterPlugin(arrays);

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin(arrays.Count).Arg("active");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.array\""));
        Assert.That(json, Does.Contain("\"member\": \"count\""));
        Assert.That(json, Does.Contain("\"active\""));
    }

    [Test]
    public void plugin_property_read_uses_declared_property_contract()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("auth", p => p.Property<string>("token"));

        Trigger(plan).DomReady(p =>
        {
            TypedPluginPropertySource<string> src = p.PluginProperty<string>("auth", "token");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.auth\""));
        Assert.That(json, Does.Contain("\"properties\""));
        Assert.That(json, Does.Contain("\"token\""));
        Assert.That(json, Does.Contain("\"kind\": \"property\""));
    }

    [Test]
    public void typed_plugin_property_uses_declared_contract()
    {
        var plan = CreatePlan();
        var auth = new AuthPlugin();
        plan.RegisterPlugin(auth);

        Trigger(plan).DomReady(p =>
        {
            TypedPluginPropertySource<string> src = p.Plugin(auth.Token);
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.auth\""));
        Assert.That(json, Does.Contain("\"token\""));
        Assert.That(json, Does.Contain("\"kind\": \"property\""));
    }

    [Test]
    public void plugin_property_and_method_cannot_share_same_member()
    {
        var plan = CreatePlan();

        Assert.Throws<System.InvalidOperationException>(() =>
            plan.RegisterPlugin("auth", p =>
            {
                p.Property<string>("token");
                p.Method<string>("token");
            }));
    }

    [Test]
    public void string_plugin_member_cannot_be_declared_twice()
    {
        var plan = CreatePlan();

        var exception = Assert.Throws<System.InvalidOperationException>(() =>
            plan.RegisterPlugin("auth", p =>
            {
                p.Method<string>("token");
                p.Method<string>("token");
            }));

        Assert.That(exception!.Message, Does.Contain("already declares member"));
        Assert.That(exception.Message, Does.Contain("auth.token"));
    }

    [Test]
    public void plugin_member_paths_reject_empty_segments()
    {
        var plan = CreatePlan();

        var exception = Assert.Throws<System.ArgumentException>(() =>
            plan.RegisterPlugin("auth", p => p.Method<string>("stats..count")));

        Assert.That(exception!.Message, Does.Contain("stats..count"));
        Assert.That(exception.Message, Does.Contain("empty segment"));
    }

    [Test]
    public void typed_plugin_registration_returns_descriptor()
    {
        var plan = CreatePlan();
        var arrays = plan.RegisterPlugin<ArrayPlugin>();

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin(arrays.Count).Arg("active");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.array\""));
        Assert.That(json, Does.Contain("\"member\": \"count\""));
        Assert.That(json, Does.Contain("\"active\""));
    }

    [Test]
    public void typed_plugin_function_rejects_wrong_argument_shape()
    {
        var plan = CreatePlan();
        var arrays = new ArrayPlugin();
        plan.RegisterPlugin(arrays);

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin(arrays.Count).Arg(42);
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void typed_plugin_function_requires_declared_argument_count()
    {
        var plan = CreatePlan();
        var arrays = new ArrayPlugin();
        plan.RegisterPlugin(arrays);

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin(arrays.Count);
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void string_plugin_function_uses_registered_argument_signature()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int, string>("count"));

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin<int>("array", "count").Arg(42);
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void string_plugin_function_requires_registered_argument_count()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin("array", p => p.Method<int, string>("count"));

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin<int>("array", "count");
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void string_plugin_function_can_declare_more_than_three_arguments()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "array",
            p => p.Method<int>(
                "rank",
                args => args
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>()));

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin<int>("array", "rank")
                .Arg("items")
                .Arg("score")
                .Arg("desc")
                .Arg("active");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"member\": \"rank\""));
        Assert.That(json, Does.Contain("\"active\""));
    }

    [Test]
    public void typed_plugin_function_can_declare_more_than_three_arguments()
    {
        var plan = CreatePlan();
        var ranking = new RankingPlugin();
        plan.RegisterPlugin(ranking);

        Trigger(plan).DomReady(p =>
        {
            TypedPluginSource<int> src = p.Plugin(ranking.Rank)
                .Arg("items")
                .Arg("score")
                .Arg("desc")
                .Arg("active");
            p.Element("x").SetText(src);
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.ranking\""));
        Assert.That(json, Does.Contain("\"member\": \"rank\""));
        Assert.That(json, Does.Contain("\"active\""));
    }

    [Test]
    public void typed_plugin_argument_builder_preserves_exact_shape_validation()
    {
        var plan = CreatePlan();
        var ranking = new RankingPlugin();
        plan.RegisterPlugin(ranking);

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin(ranking.Rank)
                    .Arg("items")
                    .Arg("score")
                    .Arg("desc")
                    .Arg(1);
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void argument_type_builder_preserves_exact_shape_validation()
    {
        var plan = CreatePlan();
        plan.RegisterPlugin(
            "array",
            p => p.Method<int>(
                "rank",
                args => args
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>()));

        Assert.Throws<System.InvalidOperationException>(() =>
        {
            Trigger(plan).DomReady(p =>
            {
                TypedPluginSource<int> src = p.Plugin<int>("array", "rank")
                    .Arg("items")
                    .Arg("score")
                    .Arg("desc")
                    .Arg(1);
                p.Element("x").SetText(src);
            });
        });
    }

    [Test]
    public void typed_plugin_command_uses_declared_contract()
    {
        var plan = CreatePlan();
        var analytics = new AnalyticsPlugin();
        plan.RegisterPlugin(analytics);

        Trigger(plan).DomReady(p =>
        {
            p.Plugin(analytics.Track).Arg("pageView").Fire();
        });

        var json = plan.RenderFormatted();
        Assert.That(json, Does.Contain("\"plugin.analytics\""));
        Assert.That(json, Does.Contain("\"method\": \"track\""));
        Assert.That(json, Does.Contain("\"pageView\""));
    }

    [Test]
    public void plan_without_plugins_clean()
    {
        var plan = CreatePlan();
        Trigger(plan).DomReady(p => p.Element("x").Show());
        var json = plan.RenderFormatted();
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

    private sealed class ArrayPlugin : ReactivePlugin
    {
        public ArrayPlugin() : base("array")
        {
            Count = Function<int, string>("count");
        }

        public PluginFunction<int> Count { get; }
    }

    private sealed class AuthPlugin : ReactivePlugin
    {
        public AuthPlugin() : base("auth")
        {
            Token = Property<string>("token");
        }

        public PluginProperty<string> Token { get; }
    }

    private sealed class AnalyticsPlugin : ReactivePlugin
    {
        public AnalyticsPlugin() : base("analytics")
        {
            Track = Command<string>("track");
        }

        public PluginCommand Track { get; }
    }

    private sealed class RankingPlugin : ReactivePlugin
    {
        public RankingPlugin() : base("ranking")
        {
            Rank = Function<int>(
                "rank",
                args => args
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>()
                    .Arg<string>());
        }

        public PluginFunction<int> Rank { get; }
    }

    private sealed class SlugifyPlugin : ReactivePlugin
    {
        public SlugifyPlugin() : base("slugify")
        {
            Invoke = Function<string, string>();
        }

        public PluginFunction<string> Invoke { get; }
    }

    private static bool TryFindLiteralWithShape(JsonElement element, string shapeKind, out JsonElement literal)
    {
        if (IsLiteralWithShape(element, shapeKind))
        {
            literal = element;
            return true;
        }

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (TryFindLiteralWithShape(property.Value, shapeKind, out literal))
                    return true;
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryFindLiteralWithShape(item, shapeKind, out literal))
                    return true;
            }
        }

        literal = default;
        return false;
    }

    private static bool IsLiteralWithShape(JsonElement element, string shapeKind)
    {
        var isObject = element.ValueKind == JsonValueKind.Object;
        if (!isObject) return false;
        var hasLiteralKind = element.TryGetProperty("kind", out var kind)
                             && kind.GetString() == "literal";
        if (!hasLiteralKind) return false;
        if (!element.TryGetProperty("shape", out var shape))
            return false;
        if (!shape.TryGetProperty("kind", out var kindProperty))
            return false;
        return kindProperty.GetString() == shapeKind;
    }
}
