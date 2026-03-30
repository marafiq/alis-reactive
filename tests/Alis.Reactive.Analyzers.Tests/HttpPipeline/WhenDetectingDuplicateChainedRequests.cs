using Alis.Reactive.Analyzers.HttpPipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.HttpPipeline;

[TestFixture]
public class WhenDetectingDuplicateChainedRequests
{
    private const string TypeStubs = @"
using System;

namespace Alis.Reactive.Builders.Requests
{
    public class ResponseBuilder<TModel> where TModel : class
    {
        public ResponseBuilder<TModel> OnSuccess(Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> OnError(int statusCode, Action<PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> configure) => this;
    }

    public class HttpRequestBuilder<TModel> where TModel : class
    {
        public HttpRequestBuilder<TModel> Post(string url) => this;
        public HttpRequestBuilder<TModel> Get(string url) => this;
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure) => this;
    }
}

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        public Requests.HttpRequestBuilder<TModel> Post(string url) => new Requests.HttpRequestBuilder<TModel>();
        public Requests.HttpRequestBuilder<TModel> Get(string url) => new Requests.HttpRequestBuilder<TModel>();
    }
}
";

    private static CSharpAnalyzerTest<DuplicateChainedRequestAnalyzer, DefaultVerifier> CreateTest(
        string source, string fileName = "View.cshtml.g.cs", params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DuplicateChainedRequestAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        if (fileName == "View.cshtml.g.cs")
        {
            test.TestCode = string.Empty;
            test.TestState.Sources.Add((fileName, source));
        }
        else
        {
            test.TestCode = source;
        }
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    private static DiagnosticResult ExpectALIS007(int markupKey)
        => new DiagnosticResult(DuplicateChainedRequestAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    // -- Clean cases ----------------------------------------------------------

    [Test]
    public async Task Single_Chained_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .OnSuccess(s => {})
             .Chained(c => c.Post(""/api/step-2"")));
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task No_Chained_at_all_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .OnSuccess(s => {})
             .OnError(400, e => {}));
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Chained_in_plain_cs_file_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class NotRazor
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-1""))
             .Chained(c => c.Post(""/api/step-2"")));
    }
}
";
        await CreateTest(source, fileName: "Service.cs").RunAsync();
    }

    [Test]
    public async Task Chained_on_different_ResponseBuilder_chains_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();

        p.Post(""/api/first"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-a"")));

        p.Post(""/api/second"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-b"")));
    }
}
";
        await CreateTest(source).RunAsync();
    }

    // -- Flagged cases --------------------------------------------------------

    [Test]
    public async Task Duplicate_Chained_on_same_chain_reports_ALIS007()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-1""))
             .{|#0:Chained|}(c => c.Post(""/api/step-2"")));
    }
}
";
        await CreateTest(source, expected: ExpectALIS007(0)).RunAsync();
    }

    [Test]
    public async Task OnSuccess_between_two_Chained_still_reports()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-1""))
             .OnSuccess(s => {})
             .{|#0:Chained|}(c => c.Post(""/api/step-2"")));
    }
}
";
        await CreateTest(source, expected: ExpectALIS007(0)).RunAsync();
    }

    [Test]
    public async Task Three_Chained_reports_on_second_and_third()
    {
        const string source = @"
using System;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Requests;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<MyModel>();
        p.Post(""/api/start"")
         .Response(r => r
             .Chained(c => c.Post(""/api/step-1""))
             .{|#0:Chained|}(c => c.Post(""/api/step-2""))
             .{|#1:Chained|}(c => c.Post(""/api/step-3"")));
    }
}
";
        await CreateTest(source, expected: new[] { ExpectALIS007(0), ExpectALIS007(1) }).RunAsync();
    }
}
