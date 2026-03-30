using Alis.Reactive.Analyzers.ReactiveEvent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.ReactiveEvent;

[TestFixture]
public class WhenDetectingDuplicateReactiveEvents
{
    private const string TypeStubs = @"
using System;

namespace Alis.Reactive
{
    public class ReactivePlan<TModel> where TModel : class { }
}

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        public PipelineBuilder<TModel> Dispatch(string name) => this;
    }
}

namespace Alis.Reactive.Components
{
    public class ChangedArgs { }
    public class ClickArgs { }
    public class FocusArgs { }

    public class TextBoxEvents
    {
        public static readonly TextBoxEvents Instance = new TextBoxEvents();
        public ChangedArgs Changed => new ChangedArgs();
        public FocusArgs Focus => new FocusArgs();
    }

    public class ButtonEvents
    {
        public static readonly ButtonEvents Instance = new ButtonEvents();
        public ClickArgs Click => new ClickArgs();
    }

    public class TextBoxBuilder<TModel> where TModel : class
    {
        public TextBoxBuilder<TModel> Placeholder(string text) => this;

        public TextBoxBuilder<TModel> Reactive<TArgs>(
            ReactivePlan<TModel> plan,
            Func<TextBoxEvents, TArgs> eventSelector,
            Action<TArgs, Builders.PipelineBuilder<TModel>> pipeline) => this;
    }

    public class ButtonBuilder<TModel> where TModel : class
    {
        public ButtonBuilder<TModel> Reactive<TArgs>(
            ReactivePlan<TModel> plan,
            Func<ButtonEvents, TArgs> eventSelector,
            Action<TArgs, Builders.PipelineBuilder<TModel>> pipeline) => this;
    }
}
";

    private static CSharpAnalyzerTest<DuplicateReactiveEventAnalyzer, DefaultVerifier> CreateTest(
        string source, string fileName = "View.g.cs", params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<DuplicateReactiveEventAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        if (fileName == "View.g.cs")
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

    private static DiagnosticResult ExpectALIS003(int markupKey)
        => new DiagnosticResult(DuplicateReactiveEventAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    // -- Clean cases ----------------------------------------------------------

    [Test]
    public async Task Single_Reactive_call_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""x""); });
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Different_events_on_same_builder_do_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); })
            .Reactive(plan, evt => evt.Focus, (args, p) => { p.Dispatch(""b""); });
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Same_event_in_plain_cs_file_does_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class NotRazor
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); })
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""b""); });
    }
}
";
        await CreateTest(source, fileName: "Service.cs").RunAsync();
    }

    [Test]
    public async Task Different_builders_with_same_event_do_not_report()
    {
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();

        new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); });

        new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""b""); });
    }
}
";
        await CreateTest(source).RunAsync();
    }

    // -- Flagged cases --------------------------------------------------------

    [Test]
    public async Task Duplicate_Reactive_for_same_event_reports_ALIS003()
    {
        // The diagnostic spans the entire InvocationExpressionSyntax (receiver chain + call).
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        {|#0:new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); })
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""b""); })|};
    }
}
";
        await CreateTest(source, expected: ExpectALIS003(0)).RunAsync();
    }

    [Test]
    public async Task Multiple_duplicates_report_multiple_diagnostics()
    {
        // Second and third .Reactive(Changed) both flag. The third invocation
        // encompasses the entire chain, the second encompasses the first two calls.
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        {|#1:{|#0:new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); })
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""b""); })|}
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""c""); })|};
    }
}
";
        await CreateTest(source, expected: new[] { ExpectALIS003(0), ExpectALIS003(1) }).RunAsync();
    }

    [Test]
    public async Task Duplicate_separated_by_different_event_reports_ALIS003()
    {
        const string source = @"
using System;
using Alis.Reactive;
using Alis.Reactive.Builders;
using Alis.Reactive.Components;

public class MyModel { }

public class GeneratedView
{
    public void Execute()
    {
        var plan = new ReactivePlan<MyModel>();
        {|#0:new TextBoxBuilder<MyModel>()
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""a""); })
            .Reactive(plan, evt => evt.Focus, (args, p) => { p.Dispatch(""b""); })
            .Reactive(plan, evt => evt.Changed, (args, p) => { p.Dispatch(""c""); })|};
    }
}
";
        await CreateTest(source, expected: ExpectALIS003(0)).RunAsync();
    }
}
