using System.Threading.Tasks;
using Alis.Reactive.Analyzers.TypeKnowledge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.TypeKnowledge;

[TestFixture]
public sealed class WhenAnOverrideIsWrittenToJson
{
    [RefusedCase]
    public Task an_override_that_renames_the_member_it_overrides_is_refused()
        => Overrides(
            "[JsonPropertyName(\"room\")] public abstract string Room { get; }",
            "[JsonPropertyName(\"bed_room\")] public override string {|#0:Room|} { get; } = \"\";",
            Renamed(0, "Resident.Room", "[JsonPropertyName(\"bed_room\")]", "Person.Room", "[JsonPropertyName(\"room\")]"));

    [RefusedCase]
    public Task an_override_with_no_name_over_a_named_declaration_is_refused()
        => Overrides(
            "[JsonPropertyName(\"room\")] public abstract string Room { get; }",
            "public override string {|#0:Room|} { get; } = \"\";",
            Renamed(0, "Resident.Room", "no JsonPropertyName", "Person.Room", "[JsonPropertyName(\"room\")]"));

    [RefusedCase]
    public Task an_override_that_names_a_declaration_written_by_its_csharp_name_is_refused()
        => Overrides(
            "public abstract string Room { get; }",
            "[JsonPropertyName(\"room\")] public override string {|#0:Room|} { get; } = \"\";",
            Renamed(0, "Resident.Room", "[JsonPropertyName(\"room\")]", "Person.Room", "no JsonPropertyName"));

    [RefusedCase]
    public Task an_override_that_ignores_what_its_declaration_writes_is_refused()
        => Overrides(
            "public abstract string Room { get; }",
            "[JsonIgnore] public override string {|#0:Room|} { get; } = \"\";",
            IgnoredOtherwise(0, "Resident.Room", "[JsonIgnore]", "Person.Room", "no JsonIgnore"));

    [RefusedCase]
    public Task an_override_that_writes_what_its_declaration_ignores_is_refused()
        => Overrides(
            "[JsonIgnore] public abstract string Room { get; }",
            "public override string {|#0:Room|} { get; } = \"\";",
            IgnoredOtherwise(0, "Resident.Room", "no JsonIgnore", "Person.Room", "[JsonIgnore]"));

    [RefusedCase]
    public Task an_override_whose_ignore_condition_is_not_its_declarations_is_refused()
        => Overrides(
            "[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public abstract string? Room { get; }",
            "[JsonIgnore] public override string? {|#0:Room|} { get; }",
            IgnoredOtherwise(0, "Resident.Room", "[JsonIgnore]", "Person.Room", "[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]"));

    [RefusedCase]
    public Task each_link_of_a_chain_is_held_to_the_member_it_overrides()
        => new Case<ALIS025OverrideKeepsItsDeclarationsJson>()
            .WithFile("Residents.cs",
                """
                using System.Text.Json.Serialization;

                namespace TestProject;

                public abstract class Person
                {
                    [JsonPropertyName("room")] public abstract string Room { get; }
                }

                public class Resident : Person
                {
                    [JsonPropertyName("room")] public override string Room { get; } = "";
                }

                public sealed class Respite : Resident
                {
                    [JsonPropertyName("bed")] public override string {|#0:Room|} { get; } = "";
                }
                """)
            .Expecting(Renamed(0, "Respite.Room", "[JsonPropertyName(\"bed\")]", "Resident.Room", "[JsonPropertyName(\"room\")]"))
            .RunAsync();

    [RefusedCase]
    public Task a_partial_override_is_refused_once_where_it_is_defined()
        => new Case<ALIS025OverrideKeepsItsDeclarationsJson>()
            .WithFile("Residents.cs",
                """
                using System.Text.Json.Serialization;

                namespace TestProject;

                public abstract class Person
                {
                    [JsonPropertyName("room")] public abstract string Room { get; }
                }

                public sealed partial class Resident : Person
                {
                    public override partial string {|#0:Room|} { get; }
                }

                public sealed partial class Resident
                {
                    public override partial string Room => "";
                }
                """)
            .Expecting(Renamed(0, "Resident.Room", "no JsonPropertyName", "Person.Room", "[JsonPropertyName(\"room\")]"))
            .RunAsync();

    [AcceptedCase]
    public Task an_override_that_keeps_the_name_of_the_member_it_overrides_is_accepted()
        => Overrides(
            "[JsonPropertyName(\"room\")] public abstract string Room { get; }",
            "[JsonPropertyName(\"room\")] public override string Room { get; } = \"\";");

    [AcceptedCase]
    public Task an_override_of_a_declaration_written_by_its_csharp_name_that_names_nothing_is_accepted()
        => Overrides(
            "public abstract string Room { get; }",
            "public override string Room { get; } = \"\";");

    [AcceptedCase]
    public Task an_override_that_keeps_the_ignore_and_its_condition_is_accepted()
        => Overrides(
            "[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public abstract string? Room { get; }",
            "[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public override string? Room { get; }");

    [AcceptedCase]
    public Task an_ignore_that_names_always_is_the_ignore_that_names_no_condition()
        => Overrides(
            "[JsonIgnore] public abstract string Room { get; }",
            "[JsonIgnore(Condition = JsonIgnoreCondition.Always)] public override string Room { get; } = \"\";");

    [AcceptedCase]
    public Task a_member_that_overrides_nothing_is_accepted_whatever_it_carries()
        => new Case<ALIS025OverrideKeepsItsDeclarationsJson>()
            .WithFile("Residents.cs",
                """
                using System.Text.Json.Serialization;

                namespace TestProject;

                public sealed class Resident
                {
                    [JsonPropertyName("bed_room")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Room { get; init; }
                }
                """)
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => Overrides(
            "[JsonPropertyName(\"room\")] public abstract string Room { get; }",
            "[JsonPropertyName(\"room\")] public override string Room { get; } = \"\"; [JsonPropertyName(\"ward\")] public override string {|CS0115:Ward|} { get; } = \"\";");

    private static Task Overrides(string declared, string written, params DiagnosticResult[] reports)
        => new Case<ALIS025OverrideKeepsItsDeclarationsJson>()
            .WithFile("Residents.cs",
                $$"""
                using System.Text.Json.Serialization;

                namespace TestProject;

                public abstract class Person
                {
                    {{declared}}
                }

                public sealed class Resident : Person
                {
                    {{written}}
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult Renamed(int at, string member, string written, string declared, string declaredText)
        => new DiagnosticResult(ALIS025OverrideKeepsItsDeclarationsJson.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"The override '{member}' carries {written} where '{declared}', the member it overrides, carries {declaredText}; an override keeps the JSON name of the member it overrides, so give it {declaredText}");

    private static DiagnosticResult IgnoredOtherwise(int at, string member, string written, string declared, string declaredText)
        => new DiagnosticResult(ALIS025OverrideKeepsItsDeclarationsJson.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"The override '{member}' carries {written} where '{declared}', the member it overrides, carries {declaredText}; an override keeps the ignore of the member it overrides, so give it {declaredText}");
}
