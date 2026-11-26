using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Validation;

[TestFixture]
public sealed class WhenFlagsAreDeclared
{
    private const string Overlapping = "[System.Flags] public enum CareDays { Early = 5, Late = 6 }";

    [RefusedCase]
    public Task a_rule_over_values_that_share_a_bit_with_neither_holding_the_other_is_refused()
        => Uses(Overlapping, "RuleFor(r => r.Care).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [RefusedCase]
    public Task a_nullable_rule_over_values_that_share_a_bit_is_refused()
        => Uses(Overlapping, "RuleFor(r => r.OptionalCare).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [AcceptedCase]
    public Task a_rule_over_disjoint_values_is_accepted()
        => Uses("[System.Flags] public enum CareDays { Early = 1, Late = 6 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_over_a_value_inside_the_other_is_accepted()
        => Uses("[System.Flags] public enum CareDays { Early = 5, Late = 7 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_over_the_other_value_inside_the_first_is_accepted()
        => Uses("[System.Flags] public enum CareDays { Early = 7, Late = 6 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_over_aliases_and_zero_is_accepted()
        => Uses("[System.Flags] public enum CareDays { None = 0, Early = 5, Late = 5 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_over_an_enum_without_the_flags_mark_is_accepted()
        => Uses("public enum CareDays { Early = 5, Late = 6 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [RefusedCase]
    public Task a_source_sending_values_that_share_a_bit_is_refused()
        => Uses(Overlapping, "", "query.Set({|#0:r => r.Care|}, source);",
            "'r => r.Care' sends CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [RefusedCase]
    public Task a_nullable_request_member_sending_values_that_share_a_bit_is_refused()
        => Uses(Overlapping, "", "json.Set({|#0:r => r.OptionalCare|}, source);",
            "'r => r.OptionalCare' sends CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [RefusedCase]
    public Task a_request_input_sending_values_that_share_a_bit_is_refused()
        => Uses(Overlapping, "", "json.Input({|#0:r => r.Care|}, input);",
            "'r => r.Care' sends CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [RefusedCase]
    public Task a_generated_view_sending_values_that_share_a_bit_is_refused()
        => Uses(Overlapping, "", "query.Query({|#0:r => r.Care|}, source);",
            "'r => r.Care' sends CareDays, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other",
            path: "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Admission_cshtml.g.cs");

    [AcceptedCase]
    public Task a_source_sending_disjoint_values_is_accepted()
        => Uses("[System.Flags] public enum CareDays { Early = 1, Late = 6 }", "", "query.Set(r => r.Care, source);");

    [AcceptedCase]
    public Task a_literal_of_values_that_share_a_bit_is_accepted()
        => Uses(Overlapping, "", "query.Set(r => r.Care, CareDays.Early | CareDays.Late);");

    [AcceptedCase]
    public Task a_source_sending_an_enum_without_the_flags_mark_is_accepted()
        => Uses("public enum CareDays { Early = 5, Late = 6 }", "", "query.Set(r => r.Care, source);");

    [RefusedCase]
    public Task a_rule_over_a_value_above_the_exact_integer_range_is_refused()
        => Uses("[System.Flags] public enum CareDays : ulong { Early = 1, Archive = 9007199254740992 }", "RuleFor(r => r.Care).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, whose value Archive is 9007199254740992, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [RefusedCase]
    public Task a_rule_over_a_negative_value_is_refused()
        => Uses("[System.Flags] public enum CareDays : long { Early = 1, Archive = -1 }", "RuleFor(r => r.Care).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, whose value Archive is -1, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [RefusedCase]
    public Task a_rule_over_the_unsigned_maximum_is_refused_without_overflow()
        => Uses("[System.Flags] public enum CareDays : ulong { Early = 1, Archive = ulong.MaxValue }", "RuleFor(r => r.Care).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, whose value Archive is 18446744073709551615, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [AcceptedCase]
    public Task a_rule_over_the_largest_exact_integer_is_accepted()
        => Uses("[System.Flags] public enum CareDays : ulong { Early = 1, Archive = 9007199254740991 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_over_zero_is_accepted()
        => Uses("[System.Flags] public enum CareDays : long { Early = 1, Archive = 0 }", "RuleFor(r => r.Care).IsInEnum();", "");

    [RefusedCase]
    public Task a_source_sending_a_value_above_the_exact_integer_range_is_refused()
        => Uses("[System.Flags] public enum CareDays : ulong { Early = 1, Archive = 9007199254740992 }", "", "query.Set({|#0:r => r.Care|}, source);",
            "'r => r.Care' sends CareDays, whose value Archive is 9007199254740992, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [AcceptedCase]
    public Task a_source_sending_the_largest_exact_integer_is_accepted()
        => Uses("[System.Flags] public enum CareDays : ulong { Early = 1, Archive = 9007199254740991 }", "", "query.Set(r => r.Care, source);");

    [AcceptedCase]
    public Task an_unrelated_is_in_enum_method_is_not_the_framework_rule()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Other.cs", """
                [System.Flags] public enum Shift { Early = 5, Late = 6 }
                public sealed class Other
                {
                    public void IsInEnum<T>() { }
                    public void Register() => IsInEnum<Shift>();
                }
                """).RunAsync();

    [AcceptedCase]
    public Task a_project_without_the_core_is_accepted()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Other.cs", "[System.Flags] public enum Shift { Early = 5, Late = 6 }")
            .WithoutTheCore().RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => Uses(Overlapping, "", "query.Set(r => r.{|CS1061:Missing|}, source);");

    private static Task Uses(string declaration, string rules, string sends, string? message = null, string path = "Validation/Admission.cs")
    {
        var test = new Case<ALIS038FlagsAreDeclared>()
            .WithFile(path, $$"""
                using Alis.Reactive.Validation;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;
                using Alis.Reactive.RequestsAndResponses;
                {{declaration}}
                public sealed class Admission
                {
                    public CareDays Care { get; set; }
                    public CareDays? OptionalCare { get; set; }
                }
                public sealed class AdmissionRules : ReactiveValidator<Admission>
                {
                    public AdmissionRules() { {{rules}} }
                }
                public static class View
                {
                    public static void Send(QueryParts<Admission> query, JsonBody<Admission> json, ITypedSource<CareDays> source, BoundInput<CareDays> input)
                    {
                        {{sends}}
                    }
                }
                """);
        if (message is not null) test.Expecting(Refused(message));
        return test.RunAsync();
    }

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS038FlagsAreDeclared.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
