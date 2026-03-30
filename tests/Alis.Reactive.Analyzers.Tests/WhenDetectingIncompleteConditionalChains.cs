using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using Alis.Reactive.Analyzers;

namespace Alis.Reactive.Analyzers.Tests;

[TestFixture]
public class WhenDetectingIncompleteConditionalChains
{
    /// <summary>
    /// Minimal stubs for the types the analyzer detects. Using source stubs avoids
    /// net10.0 vs net8.0 assembly version conflicts in the Roslyn test harness.
    /// </summary>
    private const string TypeStubs = @"
using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Builders.Conditions
{
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        public GuardBuilder<TModel> And() => this;
        public BranchBuilder<TModel> Then(Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> pipeline) => new BranchBuilder<TModel>();
    }

    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        public GuardBuilder<TModel> Eq(TProp value) => new GuardBuilder<TModel>();
        public GuardBuilder<TModel> Gt(TProp value) => new GuardBuilder<TModel>();
    }

    public sealed class BranchBuilder<TModel> where TModel : class
    {
        public void Else(Action<Alis.Reactive.Builders.PipelineBuilder<TModel>> pipeline) { }
    }
}

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        public Conditions.ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
            => new Conditions.ConditionSourceBuilder<TModel, TProp>();

        public Conditions.GuardBuilder<TModel> Confirm(string message)
            => new Conditions.GuardBuilder<TModel>();

        public ElementBuilder<TModel> Element(string id) => new ElementBuilder<TModel>(this);
        public PipelineBuilder<TModel> Dispatch(string name) => this;
    }

    public class ElementBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pb;
        public ElementBuilder(PipelineBuilder<TModel> pb) { _pb = pb; }
        public PipelineBuilder<TModel> Show() => _pb;
    }
}
";

    private static CSharpAnalyzerTest<IncompleteConditionalChainAnalyzer, DefaultVerifier> CreateTest(
        string source, string fileName = "Test0.cs", params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<IncompleteConditionalChainAnalyzer, DefaultVerifier>
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

    private static DiagnosticResult ExpectALIS001(int markupKey)
        => new DiagnosticResult(IncompleteConditionalChainAnalyzer.DiagnosticId, DiagnosticSeverity.Error)
            .WithLocation(markupKey);

    [Test]
    public async Task Dangling_When_Eq_in_plain_cs_does_not_report()
    {
        const string source = @"
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;

public class Payload { public string Status { get; set; } = """"; }

public class MyClass
{
    public void Example()
    {
        var pb = new PipelineBuilder<MyClass>();
        var payload = new Payload();
        pb.When(payload, x => x.Status).Eq(""ok"");
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Dangling_Confirm_in_plain_cs_does_not_report()
    {
        const string source = @"
using Alis.Reactive.Builders;

public class MyClass
{
    public void Example()
    {
        var pb = new PipelineBuilder<MyClass>();
        pb.Confirm(""Are you sure?"");
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Complete_chain_does_not_report()
    {
        const string source = @"
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;

public class Payload { public string Status { get; set; } = """"; }

public class MyClass
{
    public void Example()
    {
        var pb = new PipelineBuilder<MyClass>();
        var payload = new Payload();
        pb.When(payload, x => x.Status).Eq(""ok"")
          .Then(t => t.Dispatch(""done""));
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Plain_commands_do_not_report()
    {
        const string source = @"
using Alis.Reactive.Builders;

public class MyClass
{
    public void Example()
    {
        var pb = new PipelineBuilder<MyClass>();
        pb.Element(""x"").Show();
        pb.Dispatch(""ready"");
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Dangling_When_Eq_in_generated_file_reports_ALIS001()
    {
        const string source = @"
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;

public class Payload { public string Value { get; set; } = """"; }

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<GeneratedView>();
        var args = new Payload();
        {|#0:p.When(args, x => x.Value).Eq(""active"")|};
    }
}
";
        await CreateTest(source, "ReactiveConditions.g.cs", ExpectALIS001(0)).RunAsync();
    }

    [Test]
    public async Task Dangling_Confirm_in_generated_file_reports_ALIS001()
    {
        const string source = @"
using Alis.Reactive.Builders;

public class GeneratedView
{
    public void Execute()
    {
        var p = new PipelineBuilder<GeneratedView>();
        {|#0:p.Confirm(""Are you sure?"")|};
    }
}
";
        await CreateTest(source, "ReactiveConditions.g.cs", ExpectALIS001(0)).RunAsync();
    }
}
