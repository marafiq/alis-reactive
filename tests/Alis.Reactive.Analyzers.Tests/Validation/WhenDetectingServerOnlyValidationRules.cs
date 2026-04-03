using Alis.Reactive.Analyzers.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.Analyzers.Tests.Validation;

[TestFixture]
public class WhenDetectingServerOnlyValidationRules
{
    private const string TypeStubs = @"
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace FluentValidation
{
    public abstract class AbstractValidator<T>
    {
        public IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(
            Expression<Func<T, TProperty>> expression) => default!;
    }

    public interface IRuleBuilderInitial<T, TProperty> : IRuleBuilderOptions<T, TProperty> { }

    public interface IRuleBuilderOptions<T, TProperty>
    {
        IRuleBuilderOptions<T, TProperty> NotEmpty();
        IRuleBuilderOptions<T, TProperty> MaximumLength(int max);
        IRuleBuilderOptions<T, TProperty> IsInEnum();
        IRuleBuilderOptions<T, TProperty> Must(Func<TProperty, bool> predicate);
        IRuleBuilderOptions<T, TProperty> MustAsync(
            Func<TProperty, CancellationToken, Task<bool>> predicate);
        IRuleBuilderOptions<T, TProperty> Custom(Action<TProperty, object> action);
        IRuleBuilderOptions<T, TProperty> CustomAsync(
            Func<TProperty, object, CancellationToken, Task> action);
        IRuleBuilderOptions<T, TProperty> SetValidator(AbstractValidator<TProperty> validator);
    }
}

namespace Alis.Reactive.FluentValidator
{
    public abstract class ReactiveValidator<T> : FluentValidation.AbstractValidator<T> { }
}
";

    private static CSharpAnalyzerTest<ServerOnlyValidationRuleAnalyzer, DefaultVerifier> CreateTest(
        string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ServerOnlyValidationRuleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = string.Empty,
        };

        test.TestState.Sources.Add(("TypeStubs.cs", TypeStubs));
        test.TestState.Sources.Add(("Test0.cs", source));
        test.ExpectedDiagnostics.AddRange(expected);
        return test;
    }

    private static DiagnosticResult ExpectALIS005(int markupKey)
        => new DiagnosticResult(ServerOnlyValidationRuleAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithLocation(markupKey);

    // ── Clean cases ───────────────────────────────────────────

    [Test]
    public async Task Extractable_rules_in_ReactiveValidator_do_not_report()
    {
        const string source = @"
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100);
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task Server_only_rules_in_AbstractValidator_do_not_report()
    {
        const string source = @"
using System;
using FluentValidation;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : AbstractValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).Must(x => x.Length > 0);
    }
}
";
        await CreateTest(source).RunAsync();
    }

    [Test]
    public async Task SetValidator_in_ReactiveValidator_does_not_report()
    {
        // SetValidator is handled by FluentValidationRuleExtractor (recurses into nested validator).
        // It is not server-only — the extractor materializes nested rules.
        const string source = @"
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class Address
{
    public string Street { get; set; } = """";
}

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator() { RuleFor(x => x.Street).NotEmpty(); }
}

public class MyModel
{
    public Address Address { get; set; } = new Address();
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Address).SetValidator(new AddressValidator());
    }
}
";
        await CreateTest(source).RunAsync();
    }

    // ── Flagged cases ─────────────────────────────────────────

    [Test]
    public async Task IsInEnum_in_ReactiveValidator_reports_ALIS005()
    {
        const string source = @"
using FluentValidation;
using Alis.Reactive.FluentValidator;

public enum CareLevel { Independent, Assisted, Memory }

public class MyModel
{
    public CareLevel Level { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Level).{|#0:IsInEnum|}();
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task Must_in_ReactiveValidator_reports_ALIS005()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).{|#0:Must|}(x => x.StartsWith(""A""));
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task MustAsync_in_ReactiveValidator_reports_ALIS005()
    {
        const string source = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Email { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Email).{|#0:MustAsync|}((email, ct) => Task.FromResult(true));
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task Custom_in_ReactiveValidator_reports_ALIS005()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).{|#0:Custom|}((name, ctx) => { });
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task CustomAsync_in_ReactiveValidator_reports_ALIS005()
    {
        const string source = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).{|#0:CustomAsync|}((name, ctx, ct) => Task.CompletedTask);
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task Server_only_method_after_extractable_chain_reports_ALIS005()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public class MyModel
{
    public string Name { get; set; } = """";
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().{|#0:Must|}(x => x.StartsWith(""A""));
    }
}
";
        await CreateTest(source, ExpectALIS005(0)).RunAsync();
    }

    [Test]
    public async Task Multiple_server_only_calls_report_multiple_diagnostics()
    {
        const string source = @"
using System;
using FluentValidation;
using Alis.Reactive.FluentValidator;

public enum Status { Active, Inactive }

public class MyModel
{
    public string Name { get; set; } = """";
    public Status Status { get; set; }
}

public class MyValidator : ReactiveValidator<MyModel>
{
    public MyValidator()
    {
        RuleFor(x => x.Name).{|#0:Must|}(x => x.Length > 0);
        RuleFor(x => x.Status).{|#1:IsInEnum|}();
    }
}
";
        await CreateTest(source, ExpectALIS005(0), ExpectALIS005(1)).RunAsync();
    }
}
