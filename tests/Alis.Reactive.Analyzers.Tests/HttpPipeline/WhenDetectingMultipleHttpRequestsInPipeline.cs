using Alis.Reactive.Analyzers.HttpPipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.HttpPipeline;

[TestFixture]
public class WhenDetectingMultipleHttpRequestsInPipeline
{
    private const string TypeStubs = @"
using System;

namespace Alis.Reactive.Builders
{
    public class TriggerBuilder<TModel> where TModel : class
    {
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> configure) => this;
        public TriggerBuilder<TModel> CustomEvent(string name, Action<PipelineBuilder<TModel>> configure) => this;
    }

    public class PipelineBuilder<TModel> where TModel : class
    {
        public HttpRequestBuilder<TModel> Get(string url) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Post(string url) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather) => new HttpRequestBuilder<TModel>();
        public HttpRequestBuilder<TModel> Delete(string url) => new HttpRequestBuilder<TModel>();
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches) => new ParallelBuilder<TModel>();
        public ElementBuilder<TModel> Element(string id) => new ElementBuilder<TModel>(this);
        public PipelineBuilder<TModel> Dispatch(string name) => this;
    }

    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pb;
        public ElementBuilder(PipelineBuilder<TModel> pb) { _pb = pb; }
        public PipelineBuilder<TModel> Show() => _pb;
        public PipelineBuilder<TModel> Hide() => _pb;
    }

    public class ParallelBuilder<TModel> where TModel : class
    {
        public PipelineBuilder<TModel> Done() => new PipelineBuilder<TModel>();
    }

    public class HttpRequestBuilder<TModel> where TModel : class
    {
        public HttpRequestBuilder<TModel> Get(string url) => this;
        public HttpRequestBuilder<TModel> Post(string url) => this;
        public HttpRequestBuilder<TModel> Put(string url) => this;
        public HttpRequestBuilder<TModel> Delete(string url) => this;
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> WhileLoading(Action<PipelineBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> configure) => this;
    }

    public class ResponseBuilder<TModel> where TModel : class
    {
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> configure) => this;
    }

    public class GatherBuilder<TModel> where TModel : class
    {
        public GatherBuilder<TModel> IncludeAll() => this;
        public GatherBuilder<TModel> Static(string key, object value) => this;
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

    private static CSharpAnalyzerTest<MultipleHttpRequestsInPipelineAnalyzer, DefaultVerifier> CreateTest(
        string source, string fileName = "Test0.cs", params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<MultipleHttpRequestsInPipelineAnalyzer, DefaultVerifier>
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

    private static DiagnosticResult ExpectALIS008(int markupKey)
        => new DiagnosticResult(MultipleHttpRequestsInPipelineAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    // ── Clean: single HTTP request ───────────────────────────────

    [Test]
    public async Task Single_HTTP_request_does_not_report()
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
            p.Post(""/api/save"", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s => s.Dispatch(""saved"")));
        }));
    }
}
";
        await CreateTest(source, "View.g.cs").RunAsync();
    }

    // ── Flagged: two HTTP requests ───────────────────────────────

    [Test]
    public async Task Two_HTTP_requests_in_same_lambda_reports_ALIS008_on_second()
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
            p.Post(""/api/save"", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s => s.Dispatch(""saved"")));

            {|#0:p.Post(""/api/other"", g => g.IncludeAll());|}
        }));
    }
}
";
        await CreateTest(source, "View.g.cs", ExpectALIS008(0)).RunAsync();
    }

    // ── Clean: Parallel requests ─────────────────────────────────

    [Test]
    public async Task Parallel_requests_do_not_report()
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
            p.Parallel(
                a => a.Get(""/api/one""),
                b => b.Get(""/api/two"")
            );
        }));
    }
}
";
        await CreateTest(source, "View.g.cs").RunAsync();
    }

    // ── Clean: Chained request ───────────────────────────────────

    [Test]
    public async Task Chained_request_does_not_report()
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
            p.Post(""/api/first"", g => g.IncludeAll())
             .Response(r => r.Chained(c => c.Post(""/api/second"")));
        }));
    }
}
";
        await CreateTest(source, "View.g.cs").RunAsync();
    }

    // ── Scope: plain .cs file ────────────────────────────────────

    [Test]
    public async Task HTTP_in_plain_cs_file_does_not_report()
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
            p.Post(""/api/save"", g => g.IncludeAll());
            p.Post(""/api/other"", g => g.IncludeAll());
        }));
    }
}
";
        // Default fileName = "Test0.cs" — NOT a generated file
        await CreateTest(source).RunAsync();
    }

    // ── Mixed: HTTP after non-HTTP commands ──────────────────────

    [Test]
    public async Task HTTP_after_non_HTTP_commands_then_another_HTTP_reports_correctly()
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
             .Response(r => r.OnSuccess(s => s.Element(""result"").Show()));

            p.Element(""spinner"").Hide();

            {|#0:p.Delete(""/api/cleanup"");|}
        }));
    }
}
";
        await CreateTest(source, "View.g.cs", ExpectALIS008(0)).RunAsync();
    }

    // ── Multiple: three HTTP requests reports on second and third ─

    [Test]
    public async Task Three_HTTP_requests_reports_on_second_and_third()
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
            p.Get(""/api/one"");

            {|#0:p.Post(""/api/two"", g => g.IncludeAll());|}

            {|#1:p.Delete(""/api/three"");|}
        }));
    }
}
";
        await CreateTest(source, "View.g.cs", ExpectALIS008(0), ExpectALIS008(1)).RunAsync();
    }

    // ── Clean: HTTP inside nested lambdas not counted ────────────

    [Test]
    public async Task HTTP_inside_OnSuccess_lambda_does_not_count_as_top_level()
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
            p.Post(""/api/save"", g => g.IncludeAll())
             .Response(r => r.OnSuccess(s =>
             {
                 s.Dispatch(""saved"");
             }));
        }));
    }
}
";
        await CreateTest(source, "View.g.cs").RunAsync();
    }

    // ── Mixed HTTP methods ───────────────────────────────────────

    [Test]
    public async Task Mixed_Get_and_Put_reports_ALIS008()
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
            p.Get(""/api/data"");

            {|#0:p.Put(""/api/update"", g => g.IncludeAll());|}
        }));
    }
}
";
        await CreateTest(source, "View.g.cs", ExpectALIS008(0)).RunAsync();
    }
}
