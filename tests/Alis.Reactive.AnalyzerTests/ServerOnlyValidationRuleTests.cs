using Alis.Reactive.Analyzers.Validation;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests;

/// <summary>ALIS005: a C# predicate in a ReactiveValidator runs on the server only; the analyzer says so where it is written.</summary>
[TestFixture]
public class ServerOnlyValidationRuleTests
{
    private const string Validator = "Validators/AdmissionValidator.cs";

    private static string Message(string check) =>
        $"'{check}' is server-only — the browser cannot run C#, so ReactiveValidator keeps it on the server";

    [Test]
    public Task must_is_reported() =>
        Fixture.Reports<ServerOnlyValidationRuleAnalyzer>("ALIS005", Message("Must"), Validator, ValidatorWith(
            """
            RuleFor(x => x.Physician).[|Must|](physician => physician != "Panel full");
            """));

    [Test]
    public Task must_async_is_reported() =>
        Fixture.Reports<ServerOnlyValidationRuleAnalyzer>("ALIS005", Message("MustAsync"), Validator, ValidatorWith(
            """
            RuleFor(x => x.Physician).[|MustAsync|]((physician, cancellation) => Task.FromResult(physician != "Panel full"));
            """));

    [Test]
    public Task a_check_the_browser_runs_too_is_not_reported() =>
        Fixture.ReportsNothing<ServerOnlyValidationRuleAnalyzer>(Validator, ValidatorWith(
            """
            RuleFor(x => x.Physician).NotEmpty();
            """));

    [Test]
    public Task a_predicate_outside_a_validator_is_left_alone() =>
        Fixture.ReportsNothing<ServerOnlyValidationRuleAnalyzer>("Validators/PhysicianRules.cs",
            """
            using Alis.Reactive.FluentValidator;

            public class Admission
            {
                public string Physician { get; set; } = "";
            }

            public static class PhysicianRules
            {
                public static IRuleBuilderOptions<Admission, string> OnThePanel(this IRuleBuilder<Admission, string> rule) =>
                    rule.Must(physician => physician != "Panel full");
            }
            """);

    private static string ValidatorWith(string rule) => $$"""
        using System.Threading;
        using System.Threading.Tasks;
        using Alis.Reactive.FluentValidator;

        public class Admission
        {
            public string Physician { get; set; } = "";
        }

        public sealed class AdmissionValidator : ReactiveValidator<Admission>
        {
            public AdmissionValidator()
            {
                {{rule}}
            }
        }
        """;
}
