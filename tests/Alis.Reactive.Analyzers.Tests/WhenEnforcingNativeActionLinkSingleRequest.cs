using Alis.Reactive.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests;

[TestFixture]
public class WhenEnforcingNativeActionLinkSingleRequest
{
    private const string TypeStubs = @"
using System;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public interface IHtmlHelper<TModel> where TModel : class { }
}

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        public Requests.HttpRequestBuilder<TModel> Get(string url) => new Requests.HttpRequestBuilder<TModel>();
        public Requests.HttpRequestBuilder<TModel> Post(string url) => new Requests.HttpRequestBuilder<TModel>();
        public Requests.HttpRequestBuilder<TModel> Put(string url) => new Requests.HttpRequestBuilder<TModel>();
        public Requests.HttpRequestBuilder<TModel> Delete(string url) => new Requests.HttpRequestBuilder<TModel>();
        public ParallelBuilder<TModel> Parallel(params Action<Requests.HttpRequestBuilder<TModel>>[] branches) => new ParallelBuilder<TModel>();
        public GuardBuilder<TModel> Confirm(string message) => new GuardBuilder<TModel>();
        public ElementBuilder<TModel> Element(string id) => new ElementBuilder<TModel>();
    }

    public sealed class GuardBuilder<TModel> where TModel : class
    {
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> configure) => new BranchBuilder<TModel>();
    }

    public sealed class BranchBuilder<TModel> where TModel : class
    {
        public BranchBuilder<TModel> Else(Action<PipelineBuilder<TModel>> configure) => this;
    }

    public sealed class ParallelBuilder<TModel> where TModel : class
    {
        public void OnAllSettled(Action<PipelineBuilder<TModel>> configure) { }
    }

    public sealed class ElementBuilder<TModel> where TModel : class
    {
        public PipelineBuilder<TModel> Show() => new PipelineBuilder<TModel>();
    }
}

namespace Alis.Reactive.Builders.Requests
{
    public class GatherBuilder<TModel> where TModel : class
    {
        public GatherBuilder<TModel> Static(string key, object value) => this;
        public GatherBuilder<TModel> IncludeAll() => this;
    }

    namespace Validation
    {
        public sealed class ValidationDescriptor
        {
        }
    }

    public class HttpRequestBuilder<TModel> where TModel : class
    {
        public HttpRequestBuilder<TModel> Get(string url) => this;
        public HttpRequestBuilder<TModel> Post(string url) => this;
        public HttpRequestBuilder<TModel> Put(string url) => this;
        public HttpRequestBuilder<TModel> Delete(string url) => this;
        public HttpRequestBuilder<TModel> Gather(Action<GatherBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> WhileLoading(Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> configure) => this;
        public HttpRequestBuilder<TModel> Validate<TValidator>(string formId) where TValidator : class => this;
        public HttpRequestBuilder<TModel> Validate(Validation.ValidationDescriptor validation) => this;
        public HttpRequestBuilder<TModel> Response(Action<ResponseBuilder<TModel>> configure) => this;
    }

    public class ResponseBuilder<TModel> where TModel : class
    {
        public ResponseBuilder<TModel> OnSuccess(Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> OnError(int statusCode, Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> configure) => this;
        public ResponseBuilder<TModel> Chained(Action<HttpRequestBuilder<TModel>> configure) => this;
    }
}

namespace Alis.Reactive.Native.Components
{
    public sealed class NativeActionLinkBuilder<TModel> where TModel : class
    {
        public NativeActionLinkBuilder<TModel> CssClass(string css) => this;
    }

    public static class NativeActionLinkHtmlExtensions
    {
        public static NativeActionLinkBuilder<TModel> NativeActionLink<TModel>(
            this Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> html,
            string linkText,
            string url,
            Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> configure)
            where TModel : class
            => new NativeActionLinkBuilder<TModel>();
    }
}";

    private static CSharpAnalyzerTest<NativeActionLinkSingleRequestAnalyzer, DefaultVerifier> CreateTest(
        string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<NativeActionLinkSingleRequestAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = string.Empty,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        test.TestState.Sources.Add(("NativeActionLink.g.cs", source));
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    private static DiagnosticResult ExpectALIS002(int markupKey)
        => new DiagnosticResult(NativeActionLinkSingleRequestAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    [Test]
    public async Task Single_request_chain_does_not_report()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Delete"", ""/orders/delete/42"", p =>
        {
            p.Post(""/orders/delete/42"")
             .Response(r => r.OnSuccess(x => x.Element(""result"").Show()));
        })
            .CssClass(""row-action"");
    }
}";

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Parallel_reports_ALIS002()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Delete"", ""/orders/delete/42"", {|#0:p =>
        {
            p.Parallel(
                a => a.Post(""/a""),
                b => b.Post(""/b""))
             .OnAllSettled(x => x.Element(""done"").Show());
            p.Post(""/orders/delete/42"");
        }|});
    }
}";

        await CreateTest(source, ExpectALIS002(0)).RunAsync();
    }

    [Test]
    public async Task Chained_reports_ALIS002()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Delete"", ""/orders/delete/42"", {|#0:p =>
        {
            p.Post(""/orders/delete/42"")
             .Response(r => r.Chained(x => x.Get(""/orders/after-delete"")));
        }|});
    }
}";

        await CreateTest(source, ExpectALIS002(0)).RunAsync();
    }

    [Test]
    public async Task Nested_http_in_response_handler_reports_ALIS002()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Delete"", ""/orders/delete/42"", {|#0:p =>
        {
            p.Post(""/orders/delete/42"")
             .Response(r => r.OnSuccess(x => x.Post(""/orders/after-delete"")));
        }|});
    }
}";

        await CreateTest(source, ExpectALIS002(0)).RunAsync();
    }

    [Test]
    public async Task Confirm_wrapped_single_request_chain_does_not_report()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Delete"", ""/orders/delete/42"", p =>
        {
            p.Confirm(""Delete row?"")
             .Then(t => t.Delete(""/orders/delete/42"")
                 .Response(r => r.OnSuccess(x => x.Element(""done"").Show())));
        });
    }
}";

        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Include_all_gather_reports_ALIS002()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Search"", ""/orders/search"", {|#0:p =>
        {
            p.Get(""/orders/search"")
             .Gather(g => g.IncludeAll());
        }|});
    }
}";

        await CreateTest(source, ExpectALIS002(0)).RunAsync();
    }

    [Test]
    public async Task Validation_reports_ALIS002()
    {
        const string source = @"
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

public class PageModel { }
public class MyValidator { }

public class GeneratedView
{
    public IHtmlHelper<PageModel> Html { get; set; } = default!;

    public void Execute()
    {
        Html.NativeActionLink(""Save"", ""/orders/save"", {|#0:p =>
        {
            p.Post(""/orders/save"")
             .Validate<MyValidator>(""orders-form"");
        }|});
    }
}";

        await CreateTest(source, ExpectALIS002(0)).RunAsync();
    }
}
