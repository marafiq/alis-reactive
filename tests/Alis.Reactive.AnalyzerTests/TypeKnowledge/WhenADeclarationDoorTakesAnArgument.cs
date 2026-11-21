using System.Threading.Tasks;
using Alis.Reactive.Analyzers.TypeKnowledge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.TypeKnowledge;

[TestFixture]
public sealed class WhenADeclarationDoorTakesAnArgument
{
    [RefusedCase]
    public Task a_name_that_is_no_constant_is_refused_at_its_argument()
        => InABed(
            "public ReadOnly<string> Resident => ReadOnly<string>({|#0:name: Named()|});",
            NameIsNoConstant(0, "WardBed.Resident", "Named()", "ReadOnly"));

    [RefusedCase]
    public Task a_name_passed_by_its_place_that_is_no_constant_is_refused_at_its_argument()
        => InABed(
            "public ReadOnly<string> Resident => ReadOnly<string>({|#0:Named()|});",
            NameIsNoConstant(0, "WardBed.Resident", "Named()", "ReadOnly"));

    [RefusedCase]
    public Task an_optional_mark_that_is_no_constant_is_refused_at_its_argument()
        => InABed(
            "public ReadOnly<string> Resident => ReadOnly<string>({|#0:optional: Older|});",
            OptionalIsNoConstant(0, "WardBed.Resident", "Older", "ReadOnly"));

    [RefusedCase]
    public Task a_nullable_mark_that_is_no_constant_is_refused_at_its_argument()
        => InABed(
            "public WardBed Partner => LiveObject<WardBed>({|#0:nullable: Older|});",
            NullableIsNoConstant(0, "WardBed.Partner", "Older", "LiveObject"));

    [RefusedCase]
    public Task a_position_that_is_no_constant_is_refused_at_its_argument()
        => InABed(
            "public ReadOnly<string> Caller => Argument<string>({|#0:Older ? 1 : 0|});",
            PositionIsNoConstant(0, "WardBed.Caller", "Older ? 1 : 0", "Argument"));

    [RefusedCase]
    public Task each_argument_of_one_door_that_is_no_constant_is_refused_at_its_own_place()
        => InABed(
            "public ReadOnly<string> Resident => ReadOnly<string>({|#0:name: Named()|}, {|#1:optional: Older|});",
            NameIsNoConstant(0, "WardBed.Resident", "Named()", "ReadOnly"),
            OptionalIsNoConstant(1, "WardBed.Resident", "Older", "ReadOnly"));

    [RefusedCase]
    public Task an_explicit_interface_member_is_named_by_its_simple_name()
        => new Case<ALIS024DeclarationDoorTakesConstants>()
            .WithFile("Ward/TurningBed.cs",
                """
                using Alis.Reactive.Declarations;

                namespace TestProject.Ward;

                public interface ITurning
                {
                    ReadOnly<string> Label { get; }
                }

                public sealed class TurningBed : JsObject, ITurning
                {
                    private static string Named() => "label";

                    ReadOnly<string> ITurning.Label => ReadOnly<string>({|#0:name: Named()|});
                }
                """)
            .Expecting(NameIsNoConstant(0, "TurningBed.Label", "Named()", "ReadOnly"))
            .RunAsync();

    [AcceptedCase]
    public Task a_name_written_as_a_text_is_accepted()
        => InABed("""public ReadOnly<string> Resident => ReadOnly<string>(name: "resident");""");

    [AcceptedCase]
    public Task a_name_a_constant_of_the_class_holds_is_accepted()
        => InABed("public ReadOnly<string> Resident => ReadOnly<string>(name: Known);");

    [AcceptedCase]
    public Task a_name_written_as_null_is_accepted()
        => InABed("public ReadOnly<string> Resident => ReadOnly<string>(name: null);");

    [AcceptedCase]
    public Task an_optional_mark_written_as_true_is_accepted()
        => InABed("public ReadOnly<string> Resident => ReadOnly<string>(optional: true);");

    [AcceptedCase]
    public Task a_nullable_mark_written_as_true_is_accepted()
        => InABed("public WardBed Partner => LiveObject<WardBed>(nullable: true);");

    [AcceptedCase]
    public Task a_position_written_as_a_number_is_accepted()
        => InABed("public ReadOnly<string> Caller => Argument<string>(0);");

    [AcceptedCase]
    public Task the_arguments_a_method_passes_on_are_not_read_as_constants()
        => InABed("public void Ring(Arg<string> to) => Method([to]);");

    [AcceptedCase]
    public Task a_name_that_is_no_constant_given_to_a_method_that_is_no_door_is_accepted()
        => InABed(
            """
            private static string Labelled(string name) => name;

            public string Label() => Labelled(name: Named());
            """);

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_read_by_no_rule()
        => new Case<ALIS024DeclarationDoorTakesConstants>()
            .WithFile("Ward/WardChart.cs", "namespace TestProject.Ward; public sealed class WardChart { public int Resident => ReadOnly(name: Named()); private static int ReadOnly(string name) => name.Length; private static string Named() => \"resident\"; }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InABed("public ReadOnly<string> Resident => ReadOnly<string>(name: {|CS0103:Missing|});");

    private static Task InABed(string members, params DiagnosticResult[] reports)
        => new Case<ALIS024DeclarationDoorTakesConstants>()
            .WithFile("Ward/WardBed.cs",
                $$"""
                using System;
                using Alis.Reactive.Declarations;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Ward;

                public sealed class WardBed : JsObject
                {
                    private const string Known = "resident";

                    private static readonly bool Older = DateTime.Now.Year > 2000;

                    private static string Named() => "resident";

                    {{members}}
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult NameIsNoConstant(int at, string member, string passed, string door)
        => Refused(at, $"'{member}' passes '{passed}' to {door} as name:, and the build reads the JavaScript name a door declares from a constant alone; write the name as a constant text");

    private static DiagnosticResult OptionalIsNoConstant(int at, string member, string passed, string door)
        => Refused(at, $"'{member}' passes '{passed}' to {door} as optional:, and the build reads whether a member is optional from a constant alone; write true or false");

    private static DiagnosticResult NullableIsNoConstant(int at, string member, string passed, string door)
        => Refused(at, $"'{member}' passes '{passed}' to {door} as nullable:, and the build reads whether an object holds null from a constant alone; write true or false");

    private static DiagnosticResult PositionIsNoConstant(int at, string member, string passed, string door)
        => Refused(at, $"'{member}' passes '{passed}' to {door} as position:, and the build reads the position of an argument from a constant alone; write the position as a constant number");

    private static DiagnosticResult Refused(int at, string message)
        => new DiagnosticResult(ALIS024DeclarationDoorTakesConstants.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
