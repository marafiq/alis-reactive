using Alis.Reactive.Analyzers.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.Validation;

[TestFixture]
public class WhenDetectingServerOnlyConditions
{
    private const string TypeStubs = @"
using System;
using System.Linq.Expressions;

namespace FluentValidation
{
    public abstract class AbstractValidator<T>
    {
        public IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(
            Expression<Func<T, TProperty>> expression) => default!;
        public IRuleBuilderInitial<T, TProperty> RuleForEach<TProperty>(
            Expression<Func<T, TProperty>> expression) => default!;
    }

    public interface IRuleBuilderInitial<T, TProperty> : IRuleBuilderOptions<T, TProperty> { }

    public interface IRuleBuilderOptions<T, TProperty>
    {
        IRuleBuilderOptions<T, TProperty> NotEmpty();
        IRuleBuilderOptions<T, TProperty> MaximumLength(int max);
        IRuleBuilderOptions<T, TProperty> When(Func<T, bool> predicate);
        IRuleBuilderOptions<T, TProperty> Unless(Func<T, bool> predicate);
    }
}

namespace Alis.Reactive.FluentValidator
{
    public abstract class ReactiveValidator<T> : FluentValidation.AbstractValidator<T> { }
}

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
            => new ConditionSourceBuilder<TModel, TProp>();
    }

    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        public GuardBuilder<TModel> Eq(TProp value) => new GuardBuilder<TModel>();
    }

    public sealed class GuardBuilder<TModel> where TModel : class
    {
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
            => new BranchBuilder<TModel>();
    }

    public sealed class BranchBuilder<TModel> where TModel : class { }
}
";

    private static CSharpAnalyzerTest<ServerOnlyConditionAnalyzer, DefaultVerifier> CreateTest(
        string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ServerOnlyConditionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = string.Empty,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        test.TestState.Sources.Add(("Test0.cs", source));
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    private static DiagnosticResult ExpectALIS006(int markupKey)
        => new DiagnosticResult(ServerOnlyConditionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithLocation(markupKey);

    // ── Clean cases ───────────────────────────────────────────

    [Test]
    public async Task When_in_AbstractValidator_does_not_report()
    {
        const string source = @"
using System;
using FluentValidation;

public class MyModel
{
    public string Name { get; set; } = """";
    public bool IsActive { get; set; }
}

public class MyValidator : AbstractValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().When(x => x.IsActive);
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task PipelineBuilder_When_does_not_report()
    {
        const string source = @"
using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.FluentValidator;

public class Payload { public string Status { get; set; } = """"; }
public class MyModel { }

public class MyValidator : ReactiveValidator<MyModel>
{
    public void SomeMethod()
    {
        var pb = new PipelineBuilder<MyModel>();
        var payload = new Payload();
        pb.When(payload, x => x.Status).Eq(""active"");
    }
}
";
        await CreateTest(source).RunAsync();
    }

    // ── Flagged cases ─────────────────────────────────────────

    [Test]
    public async Task When_on_RuleFor_chain_in_ReactiveValidator_reports_ALIS006()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
    public bool IsActive { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().{|#0:When|}(x => x.IsActive);
    }
}
";
        await CreateTest(source, ExpectALIS006(0)).RunAsync();
    }

    [Test]
    public async Task Unless_on_RuleFor_chain_in_ReactiveValidator_reports_ALIS006()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
    public bool IsAdmin { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().{|#0:Unless|}(x => x.IsAdmin);
    }
}
";
        await CreateTest(source, ExpectALIS006(0)).RunAsync();
    }

    [Test]
    public async Task When_on_deeply_chained_rule_reports_ALIS006()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
    public bool IsActive { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).{|#0:When|}(x => x.IsActive);
    }
}
";
        await CreateTest(source, ExpectALIS006(0)).RunAsync();
    }

    [Test]
    public async Task When_on_RuleForEach_chain_reports_ALIS006()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Tags { get; set; } = """";
    public bool RequireTags { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleForEach(x => x.Tags).NotEmpty().{|#0:When|}(x => x.RequireTags);
    }
}
";
        await CreateTest(source, ExpectALIS006(0)).RunAsync();
    }

    [Test]
    public async Task Multiple_When_calls_report_multiple_diagnostics()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
    public string Email { get; set; } = """";
    public bool IsActive { get; set; }
    public bool RequireEmail { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().{|#0:When|}(x => x.IsActive);
        RuleFor(x => x.Email).NotEmpty().{|#1:When|}(x => x.RequireEmail);
    }
}
";
        await CreateTest(source, ExpectALIS006(0), ExpectALIS006(1)).RunAsync();
    }
}
