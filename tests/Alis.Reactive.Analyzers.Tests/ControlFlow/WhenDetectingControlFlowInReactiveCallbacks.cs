using Alis.Reactive.Analyzers.ControlFlow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.ControlFlow;

[TestFixture]
public class WhenDetectingControlFlowInReactiveCallbacks
{
    private const string TypeStubs = @"
using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Builders
{
    public class TriggerBuilder<TModel> where TModel : class
    {
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> configure) => this;
        public TriggerBuilder<TModel> CustomEvent(string name, Action<PipelineBuilder<TModel>> configure) => this;
        public TriggerBuilder<TModel> CustomEvent<T>(string name, Action<T, PipelineBuilder<TModel>> configure) => this;
    }

    public class PipelineBuilder<TModel> where TModel : class
    {
        public ElementBuilder<TModel> Element(string id) => new ElementBuilder<TModel>(this);
        public PipelineBuilder<TModel> Dispatch(string name) => this;
        public PipelineBuilder<TModel> Dispatch(string name, object payload) => this;
        public ComponentBuilder<TModel> Component<TComponent>(Expression<Func<TModel, object>> expr) => new ComponentBuilder<TModel>();
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(TPayload p, Expression<Func<TPayload, TProp>> path) => new ConditionSourceBuilder<TModel, TProp>();
        public GuardBuilder<TModel> Confirm(string msg) => new GuardBuilder<TModel>();
        public HttpRequestBuilder<TModel> Get(string url) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Post(string url) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather) => new HttpRequestBuilder<TModel>();
    }

    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pb;
        public ElementBuilder(PipelineBuilder<TModel> pb) { _pb = pb; }
        public PipelineBuilder<TModel> Show() => _pb;
        public PipelineBuilder<TModel> Hide() => _pb;
        public PipelineBuilder<TModel> AddClass(string cls) => _pb;
        public PipelineBuilder<TModel> SetText(string text) => _pb;
        public PipelineBuilder<TModel> SetText<TPayload, TProp>(TPayload payload, Expression<Func<TPayload, TProp>> path) => _pb;
    }

    public class ComponentBuilder<TModel> where TModel : class
    {
        public PipelineBuilder<TModel> SetValue(string value) => new PipelineBuilder<TModel>();
        public ComponentValueAccessor<TModel> Value() => new ComponentValueAccessor<TModel>();
    }

    public class ComponentValueAccessor<TModel> where TModel : class { }

    public sealed class GuardBuilder<TModel> where TModel : class
    {
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> configure) => new BranchBuilder<TModel>();
    }

    public sealed class BranchBuilder<TModel> where TModel : class
    {
        public BranchBuilder<TModel> Else(Action<PipelineBuilder<TModel>> configure) => this;
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(TPayload p, Expression<Func<TPayload, TProp>> path) => new ConditionSourceBuilder<TModel, TProp>();
    }

    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        public GuardBuilder<TModel> Eq(TProp value) => new GuardBuilder<TModel>();
        public GuardBuilder<TModel> Gte(TProp value) => new GuardBuilder<TModel>();
        public GuardBuilder<TModel> NotEmpty() => new GuardBuilder<TModel>();
        public GuardBuilder<TModel> Truthy() => new GuardBuilder<TModel>();
    }

    public class GatherBuilder<TModel> where TModel : class
    {
        public GatherBuilder<TModel> Static(string key, object value) => this;
        public GatherBuilder<TModel> IncludeAll() => this;
    }

    public class HttpRequestBuilder<TModel> where TModel : class
    {
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure) => this;
    }

    public class ResponseBuilder<TModel> where TModel : class
    {
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> OnSuccess<TResponse>(Action<TResponse, PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> configure) => this;
    }
}

namespace Alis.Reactive
{
    public class ReactivePlan<TModel> where TModel : class { }

    public static class HtmlOnExtensions
    {
        public static void On<TModel>(
            object html,
            ReactivePlan<TModel> plan,
            Action<Alis.Reactive.Builders.TriggerBuilder<TModel>> configure)
            where TModel : class
        { }
    }
}
";

    private static CSharpAnalyzerTest<ControlFlowInReactiveCallbackAnalyzer, DefaultVerifier> CreateTest(
        string source, string fileName = "Test0.cs", params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ControlFlowInReactiveCallbackAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        if (fileName == "Test0.cs")
        {
            test.TestCode = source;
        }
        else
        {
            test.TestCode = string.Empty;
            test.TestState.Sources.Add((fileName, source));
        }
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    private static DiagnosticResult ExpectALIS004(int markupKey)
        => new DiagnosticResult(ControlFlowInReactiveCallbackAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    // ── Flagged: Statement types ──────────────────────────────────

    [Test]
    public async Task If_inside_DomReady_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:if (true) p.Dispatch(""x"");|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Switch_inside_CustomEvent_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.CustomEvent(""evt"", p =>
        {
            var x = ""a"";
            {|#0:switch (x) { case ""a"": break; }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task For_loop_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:for (int i = 0; i < 3; i++) { }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Foreach_loop_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;
using System.Collections.Generic;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var items = new List<string>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:foreach (var x in items) { }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task While_loop_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:while (true) { break; }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Do_while_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:do { } while (false);|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Goto_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:goto end;|}
            {|#1:end:
            p.Dispatch(""x"");|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0), ExpectALIS004(1)).RunAsync();
    }

    [Test]
    public async Task Try_catch_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:try { p.Dispatch(""x""); } catch { }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Throw_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;
using System;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:throw new Exception();|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Lock_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var obj = new object();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:lock (obj) { p.Dispatch(""x""); }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Using_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;
using System;

public class Disposable : IDisposable { public void Dispose() { } }

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:using (var d = new Disposable()) { p.Dispatch(""x""); }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Flagged: Expression types ─────────────────────────────────

    [Test]
    public async Task Ternary_in_argument_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var x = true;
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Dispatch({|#0:x ? ""a"" : ""b""|});
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Switch_expression_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var y = 1;
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            var label = {|#0:y switch { 1 => ""one"", _ => ""other"" }|};
            p.Dispatch(label);
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Flagged: Scenarios ────────────────────────────────────────

    [Test]
    public async Task Multiple_violations_reports_multiple_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:if (true) p.Dispatch(""x"");|}
            {|#1:for (int i = 0; i < 3; i++) { }|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0), ExpectALIS004(1)).RunAsync();
    }

    [Test]
    public async Task If_on_model_property_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { public bool IsAdmin { get; set; } }

public class GeneratedView
{
    public MyModel Model { get; set; } = new MyModel();

    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:if (Model.IsAdmin) p.Element(""admin"").Show();|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task If_inside_TriggerBuilder_callback_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { public bool ShowEvents { get; set; } }

public class GeneratedView
{
    public MyModel Model { get; set; } = new MyModel();

    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t =>
        {
            {|#0:if (Model.ShowEvents) t.DomReady(p => p.Dispatch(""x""));|}
        });
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Ternary_in_expression_bodied_lambda_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var x = true;
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
            p.Dispatch({|#0:x ? ""a"" : ""b""|})));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Allowed: DSL usage ────────────────────────────────────────

    [Test]
    public async Task DSL_method_calls_do_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Element(""step-1"").AddClass(""active"");
            p.Element(""step-1"").SetText(""loaded"");
            p.Dispatch(""ready"");
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Variable_declarations_do_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;
using System.Linq.Expressions;

public class MyComponent { }

public class MyModel { public string Name { get; set; } = """"; }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            var comp = p.Component<MyComponent>(m => m.Name);
            comp.SetValue(""test"");
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task When_Then_Else_chain_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class Payload { public string Status { get; set; } = """"; }

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.CustomEvent<Payload>(""evt"", (args, p) =>
        {
            p.When(args, x => x.Status).Eq(""ok"")
                .Then(then => then.Element(""status"").SetText(""good""))
                .Else(else_ => else_.Element(""status"").SetText(""bad""));
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Http_pipeline_chain_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Post(""/api/save"", g => g.Static(""name"", ""John""))
             .WhileLoading(l => l.Element(""spinner"").Show())
             .Response(r => r.OnSuccess(s =>
             {
                 s.Element(""spinner"").Hide();
                 s.Element(""result"").SetText(""saved"");
             }));
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Empty_block_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Expression_bodied_DSL_call_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
            p.Dispatch(""ready"")));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Two_parameter_callback_with_DSL_only_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class Payload { public string Value { get; set; } = """"; }

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.CustomEvent<Payload>(""evt"", (args, p) =>
        {
            p.Element(""display"").SetText(args, x => x.Value);
            p.Dispatch(""done"");
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    // ── Scope: Nested lambdas ─────────────────────────────────────

    [Test]
    public async Task Control_flow_in_GatherBuilder_lambda_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Post(""/api/save"", g =>
            {
                if (true) g.Static(""key"", ""val"");
            });
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Control_flow_in_ResponseBuilder_lambda_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Get(""/api/data"").Response(r =>
            {
                if (true) r.OnSuccess(s => s.Dispatch(""done""));
            });
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs").RunAsync();
    }

    [Test]
    public async Task Control_flow_in_Then_lambda_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class Payload { public string Status { get; set; } = """"; }

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.CustomEvent<Payload>(""evt"", (args, p) =>
        {
            p.When(args, x => x.Status).Eq(""ok"")
                .Then(then =>
                {
                    {|#0:if (true) then.Dispatch(""x"");|}
                });
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Control_flow_in_OnSuccess_lambda_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Get(""/api/data"").Response(r => r.OnSuccess(s =>
            {
                {|#0:if (true) s.Dispatch(""x"");|}
            }));
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Scope: File type ──────────────────────────────────────────

    [Test]
    public async Task Control_flow_in_plain_cs_file_does_not_report()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class PlainClass
{
    public void Example()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            if (true) p.Dispatch(""x"");
        }));
    }
}
";
        // Default fileName = "Test0.cs" — NOT a generated file
        await CreateTest(source).RunAsync();
    }

    // ── Review fixes: double-reporting ────────────────────────────

    [Test]
    public async Task Ternary_inside_if_reports_only_if_not_both()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var x = true;
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:if (x) p.Dispatch(x ? ""a"" : ""b"");|}
        }));
    }
}
";
        // Only 1 diagnostic — the if statement. The ternary inside is NOT double-reported.
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Switch_expression_inside_switch_statement_reports_only_statement()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        var x = ""a"";
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:switch (x) { case ""a"": var y = x switch { ""a"" => ""1"", _ => ""2"" }; break; }|}
        }));
    }
}
";
        // Only 1 diagnostic — the switch statement. The nested switch expression is NOT double-reported.
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Review fixes: additional statement types ──────────────────

    [Test]
    public async Task Return_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Dispatch(""x"");
            {|#0:return;|}
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Local_function_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            {|#0:void Helper() { p.Dispatch(""x""); }|}
            Helper();
        }));
    }
}
";
        // LocalFunctionStatement is flagged; the Helper() call is an ExpressionStatement (allowed)
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    // ── Review fixes: additional scope tests ──────────────────────

    [Test]
    public async Task Control_flow_in_WhileLoading_lambda_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.DomReady(p =>
        {
            p.Get(""/api/data"")
             .WhileLoading(l =>
             {
                 {|#0:if (true) l.Element(""x"").Show();|}
             });
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }

    [Test]
    public async Task Control_flow_in_Else_lambda_reports_ALIS004()
    {
        const string source = @"
using Alis.Reactive;
using Alis.Reactive.Builders;

public class Payload { public string Status { get; set; } = """"; }

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        HtmlOnExtensions.On(this, plan, t => t.CustomEvent<Payload>(""evt"", (args, p) =>
        {
            p.When(args, x => x.Status).Eq(""ok"")
                .Then(then => then.Dispatch(""good""))
                .Else(else_ =>
                {
                    {|#0:if (true) else_.Dispatch(""bad"");|}
                });
        }));
    }
}
";
        await CreateTest(source, "View.cshtml.g.cs", ExpectALIS004(0)).RunAsync();
    }
}
