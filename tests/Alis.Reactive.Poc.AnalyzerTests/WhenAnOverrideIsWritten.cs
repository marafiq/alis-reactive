using Alis.Reactive.Poc.Analyzers;
using NUnit.Framework;

namespace Alis.Reactive.Poc.AnalyzerTests;

[TestFixture]
public sealed class WhenAnOverrideIsWritten
{
    private static string Modeled(string declaration, string overriding) => $$"""
        using System.Text.Json.Serialization;

        namespace Ward;

        public abstract class Staff
        {
            {{declaration}}
        }

        public sealed class Nurse : Staff
        {
            {{overriding}}
        }
        """;

    [Test]
    public Task an_override_that_keeps_its_declarations_name_and_ignore_is_reported_by_none()
        => Fixture.ReportsNothing<OverridesAnalyzer>("""
            using System.Text.Json.Serialization;

            namespace Ward;

            public abstract class Staff
            {
                public abstract string Role { get; set; }

                [JsonPropertyName("pager_line")]
                public abstract string Pager { get; set; }

                [JsonIgnore]
                public abstract string Ward { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public abstract string? Note { get; set; }
            }

            public sealed class Nurse : Staff
            {
                public override string Role { get; set; } = "";

                [JsonPropertyName("pager_line")]
                public override string Pager { get; set; } = "";

                [JsonIgnore]
                public override string Ward { get; set; } = "";

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                public override string? Note { get; set; }
            }
            """, null);

    [Test]
    public Task an_override_that_names_a_member_its_declaration_leaves_unnamed_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Pager' is written as 'pager_line', and 'Staff.Pager', which it overrides, as its C# name: System.Text.Json writes the override under both names; give it the JsonPropertyName of the member it overrides, or none where that member has none",
            Modeled("public abstract string Pager { get; set; }", """[JsonPropertyName("pager_line")] public override string [|Pager|] { get; set; } = "";"""),
            null);

    [Test]
    public Task an_override_that_drops_its_declarations_name_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Pager' is written as its C# name, and 'Staff.Pager', which it overrides, as 'pager_line': System.Text.Json writes the override under both names; give it the JsonPropertyName of the member it overrides, or none where that member has none",
            Modeled("""[JsonPropertyName("pager_line")] public abstract string Pager { get; set; }""", """public override string [|Pager|] { get; set; } = "";"""),
            null);

    [Test]
    public Task an_override_that_renames_its_declarations_name_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Pager' is written as 'bleep', and 'Staff.Pager', which it overrides, as 'pager_line': System.Text.Json writes the override under both names; give it the JsonPropertyName of the member it overrides, or none where that member has none",
            Modeled("""[JsonPropertyName("pager_line")] public abstract string Pager { get; set; }""", """[JsonPropertyName("bleep")] public override string [|Pager|] { get; set; } = "";"""),
            null);

    [Test]
    public Task an_override_that_ignores_a_member_its_declaration_writes_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Station' is ignored, and 'Staff.Station', which it overrides, is written: give the override the JsonIgnore of the member it overrides",
            Modeled("public abstract string Station { get; set; }", """[JsonIgnore] public override string [|Station|] { get; set; } = "";"""),
            null);

    [Test]
    public Task an_override_that_writes_a_member_its_declaration_ignores_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Station' is written, and 'Staff.Station', which it overrides, is ignored: give the override the JsonIgnore of the member it overrides",
            Modeled("[JsonIgnore] public abstract string Station { get; set; }", """public override string [|Station|] { get; set; } = "";"""),
            null);

    [Test]
    public Task an_override_that_ignores_on_a_condition_its_declaration_does_not_is_reported()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Nurse.Note' is ignored when it holds null, and 'Staff.Note', which it overrides, is written: give the override the JsonIgnore of the member it overrides",
            Modeled("public abstract string? Note { get; set; }", """[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public override string? [|Note|] { get; set; }"""),
            null);

    [Test]
    public Task an_override_of_an_override_is_checked_against_the_member_it_overrides()
        => Fixture.Reports<OverridesAnalyzer>(
            "ALIS025",
            "'Charge.Pager' is written as its C# name, and 'Nurse.Pager', which it overrides, as 'pager_line': System.Text.Json writes the override under both names; give it the JsonPropertyName of the member it overrides, or none where that member has none",
            """
            using System.Text.Json.Serialization;

            namespace Ward;

            public abstract class Staff
            {
                [JsonPropertyName("pager_line")]
                public abstract string Pager { get; set; }
            }

            public class Nurse : Staff
            {
                [JsonPropertyName("pager_line")]
                public override string Pager { get; set; } = "";
            }

            public sealed class Charge : Nurse
            {
                public override string [|Pager|] { get; set; } = "";
            }
            """,
            null);
}
