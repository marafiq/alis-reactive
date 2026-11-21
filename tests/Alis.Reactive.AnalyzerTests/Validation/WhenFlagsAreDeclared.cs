using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Validation;

[TestFixture]
public sealed class WhenFlagsAreDeclared
{
    [RefusedCase]
    public Task an_enum_without_the_flags_mark_is_refused()
        => Declares("public enum Shift { Early = 1, Late = 2 }", "validation.Flags<{|#0:Shift|}>();",
            "Flags<Shift>() names Shift as a flags enum, and Shift has no [Flags], so FluentValidation and System.Text.Json read it as one value; mark Shift [Flags], or take the call out");

    [AcceptedCase]
    public Task the_flags_mark_accepts_the_same_enum()
        => Declares("[Flags] public enum Shift { Early = 1, Late = 2 }", "validation.Flags<Shift>();");

    [RefusedCase]
    public Task intersecting_values_that_hold_neither_other_are_refused()
        => Declares("[Flags] public enum Overlap { X = 5, Y = 6 }", "validation.Flags<{|#0:Overlap|}>();",
            "Flags<Overlap>() names Overlap, whose values X (5) and Y (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other");

    [AcceptedCase]
    public Task disjoint_values_are_accepted()
        => Declares("[Flags] public enum Overlap { X = 1, Y = 6 }", "validation.Flags<Overlap>();");

    [AcceptedCase]
    public Task a_value_inside_the_other_is_accepted()
        => Declares("[Flags] public enum Overlap { X = 5, Y = 7 }", "validation.Flags<Overlap>();");

    [AcceptedCase]
    public Task the_other_value_inside_the_first_is_accepted()
        => Declares("[Flags] public enum Overlap { X = 7, Y = 6 }", "validation.Flags<Overlap>();");

    [AcceptedCase]
    public Task aliases_and_zero_are_accepted()
        => Declares("[Flags] public enum Overlap { None = 0, X = 5, Y = 5 }", "validation.Flags<Overlap>();");

    [RefusedCase]
    public Task a_value_above_the_exact_integer_range_is_refused()
        => Declares("[Flags] public enum Access : ulong { Archive = 9007199254740992 }", "validation.Flags<{|#0:Access|}>();",
            "Flags<Access>() names Access, whose value Archive is 9007199254740992, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [RefusedCase]
    public Task a_negative_value_is_refused()
        => Declares("[Flags] public enum Access : long { Archive = -1 }", "validation.Flags<{|#0:Access|}>();",
            "Flags<Access>() names Access, whose value Archive is -1, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [RefusedCase]
    public Task the_unsigned_maximum_is_refused_without_overflow()
        => Declares("[Flags] public enum Access : ulong { Archive = ulong.MaxValue }", "validation.Flags<{|#0:Access|}>();",
            "Flags<Access>() names Access, whose value Archive is 18446744073709551615, outside 0 to 9007199254740991, the range the browser's split reads, since JavaScript holds no larger integer exactly and a held number does not carry a negative value's bits; keep every value of a flags enum inside it, and a negative one as the union of the values it stands for");

    [AcceptedCase]
    public Task the_largest_exact_integer_is_accepted()
        => Declares("[Flags] public enum Access : ulong { Archive = 9007199254740991 }", "validation.Flags<Access>();");

    [AcceptedCase]
    public Task zero_is_accepted()
        => Declares("[Flags] public enum Access : long { Archive = 0 }", "validation.Flags<Access>();");

    [RefusedCase]
    public Task a_type_parameter_names_no_concrete_enum()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Startup.cs", """
                using System;
                using Alis.Reactive.Validation;
                [Flags] public enum Shift { Early = 1, Late = 2 }
                public static class Startup
                {
                    public static void Register<TEnum>(ReactiveFluentValidationBuilder validation) where TEnum : struct, Enum
                        => validation.Flags<{|#0:TEnum|}>();
                }
                """)
            .Expecting(Refused("Flags<TEnum>() names a type parameter, so the build cannot read its flags values; name a concrete [Flags] enum at startup"))
            .RunAsync();

    [AcceptedCase]
    public Task a_generic_helper_can_name_a_concrete_enum()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Startup.cs", """
                using System;
                using Alis.Reactive.Validation;
                [Flags] public enum Shift { Early = 1, Late = 2 }
                public static class Startup
                {
                    public static void Register<TEnum>(ReactiveFluentValidationBuilder validation) where TEnum : struct, Enum
                        => validation.Flags<Shift>();
                }
                """)
            .RunAsync();

    [RefusedCase]
    public Task a_flags_rule_must_be_named_by_startup()
        => Uses("services.AddReactiveFluentValidation(validation => { });",
            "RuleFor(r => r.Care).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, which [Flags] marks and the startup does not name, as one value, where FluentValidation reads it as flags; name it with Flags<CareDays>() at startup");

    [RefusedCase]
    public Task a_nullable_flags_rule_must_be_named_by_startup()
        => Uses("services.AddReactiveFluentValidation(validation => { });",
            "RuleFor(r => r.OptionalCare).{|#0:IsInEnum|}();", "",
            "IsInEnum reads CareDays, which [Flags] marks and the startup does not name, as one value, where FluentValidation reads it as flags; name it with Flags<CareDays>() at startup");

    [AcceptedCase]
    public Task naming_flags_after_the_rule_in_another_file_accepts_the_rule()
        => Uses("services.AddReactiveFluentValidation(validation => validation.Flags<CareDays>());",
            "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task a_rule_in_an_assembly_without_startup_keeps_one_value_meaning()
        => Uses("", "RuleFor(r => r.Care).IsInEnum();", "");

    [AcceptedCase]
    public Task an_unmarked_enum_needs_no_flags_registration()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "RuleFor(r => r.Care).IsInEnum();", "", mark: "");

    [RefusedCase]
    public Task a_source_sending_flags_must_be_named_by_startup()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "",
            "query.Set({|#0:r => r.Care|}, source);",
            "'r => r.Care' sends CareDays, which [Flags] marks and the startup does not name, so a combination its source holds stops the request; name it with Flags<CareDays>() at startup");

    [RefusedCase]
    public Task a_nullable_request_member_sending_flags_must_be_named()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "",
            "json.Set({|#0:r => r.OptionalCare|}, source);",
            "'r => r.OptionalCare' sends CareDays, which [Flags] marks and the startup does not name, so a combination its source holds stops the request; name it with Flags<CareDays>() at startup");

    [RefusedCase]
    public Task a_request_input_sending_flags_must_be_named()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "",
            "json.Input({|#0:r => r.Care|}, input);",
            "'r => r.Care' sends CareDays, which [Flags] marks and the startup does not name, so a combination its source holds stops the request; name it with Flags<CareDays>() at startup");

    [RefusedCase]
    public Task a_generated_view_sending_flags_is_checked()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "",
            "query.Query({|#0:r => r.Care|}, source);",
            "'r => r.Care' sends CareDays, which [Flags] marks and the startup does not name, so a combination its source holds stops the request; name it with Flags<CareDays>() at startup",
            path: "Microsoft.CodeAnalysis.Razor.Compiler/Microsoft.NET.Sdk.Razor.SourceGenerators.RazorSourceGenerator/Admission_cshtml.g.cs");

    [AcceptedCase]
    public Task naming_flags_accepts_the_source()
        => Uses("services.AddReactiveFluentValidation(validation => validation.Flags<CareDays>());", "",
            "query.Set(r => r.Care, source);");

    [AcceptedCase]
    public Task a_literal_needs_no_flags_registration()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "",
            "query.Set(r => r.Care, CareDays.Early | CareDays.Late);");

    [AcceptedCase]
    public Task a_source_in_an_assembly_without_startup_keeps_one_value_meaning()
        => Uses("", "", "query.Set(r => r.Care, source);");

    [AcceptedCase]
    public Task an_unmarked_source_needs_no_flags_registration()
        => Uses("services.AddReactiveFluentValidation(validation => { });", "", "query.Set(r => r.Care, source);", mark: "");

    [AcceptedCase]
    public Task an_unrelated_flags_method_is_not_the_framework_door()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Other.cs", """
                public enum Shift { Early = 1 }
                public sealed class Other
                {
                    public void Flags<T>() { }
                    public void Register() => Flags<Shift>();
                }
                """).RunAsync();

    [AcceptedCase]
    public Task a_project_without_the_core_is_accepted()
        => new Case<ALIS038FlagsAreDeclared>()
            .WithFile("Validation/Other.cs", "public enum Shift { Early = 1 }")
            .WithoutTheCore().RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => Declares("", "validation.Flags<{|CS0246:Missing|}>();");

    private static Task Declares(string declaration, string statements, string? message = null)
    {
        var test = new Case<ALIS038FlagsAreDeclared>().WithFile("Validation/Startup.cs", $$"""
            using System;
            using Alis.Reactive.Validation;
            {{declaration}}
            public static class Startup
            {
                public static void Register(ReactiveFluentValidationBuilder validation) { {{statements}} }
            }
            """);
        if (message is not null) test.Expecting(Refused(message));
        return test.RunAsync();
    }

    private static Task Uses(string startup, string rules, string sends, string? message = null, string mark = "[System.Flags]", string path = "Validation/Admission.cs")
    {
        var test = new Case<ALIS038FlagsAreDeclared>()
            .WithFile(path, $$"""
                using Alis.Reactive.Validation;
                using Alis.Reactive.TriggersAndPipelines;
                using Alis.Reactive.ValuesAndSources;
                using Alis.Reactive.RequestsAndResponses;
                {{mark}} public enum CareDays { Early = 1, Late = 2 }
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
                """)
            .WithFile("Validation/Startup.cs", $$"""
                using Alis.Reactive.RenderingAndDelivery;
                using Microsoft.Extensions.DependencyInjection;
                public static class Startup
                {
                    public static void Register(IServiceCollection services) { {{startup}} }
                }
                """);
        if (message is not null) test.Expecting(Refused(message));
        return test.RunAsync();
    }

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS038FlagsAreDeclared.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
