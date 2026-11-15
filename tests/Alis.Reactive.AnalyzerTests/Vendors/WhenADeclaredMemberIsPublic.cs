using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenADeclaredMemberIsPublic
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Value = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\n";

    [RefusedCase]
    public Task an_absent_member_is_reported_at_its_declaration()
        => InGauge("public Property<int> {|#0:Value|} => Property<int>();", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task a_public_member_is_accepted()
        => InGauge("public Property<int> Value => Property<int>();", Value);

    [AcceptedCase]
    public Task no_table_means_no_member_check()
        => InGauge("public Property<int> Value => Property<int>();", null);

    [RefusedCase]
    public Task an_explicit_name_is_reported_at_its_argument()
        => InGauge("public Property<int> Value => Property<int>(name: {|#0:\"missing\"|});", Value, "Declaration member 'Gauge.Value' names 'missing', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task an_explicit_name_resolves_the_public_member()
        => InGauge("public Property<int> Anything => Property<int>(name: \"value\");", Value);

    [AcceptedCase]
    public Task a_null_name_uses_the_declarations_camel_case()
        => InGauge("public Property<int> Value => Property<int>(name: null);", Value);

    [RefusedCase]
    public Task a_member_name_keeps_its_case()
        => InGauge("public Property<int> Value => Property<int>(name: {|#0:\"Value\"|});", Value, "Declaration member 'Gauge.Value' names 'Value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task a_nonconstant_name_is_left_to_alis024()
        => InGauge("private string Name => \"value\"; public Property<int> Value => Property<int>(name: Name);", Type);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public Property<int> Value => Property<int>(name: {|CS0103:Unknown|});", Type);

    [AcceptedCase]
    public Task a_missing_root_is_left_to_alis015()
        => InGauge("public Property<int> Value => Property<int>();", "format\t1\n");

    [AcceptedCase]
    public Task an_ambiguous_root_is_left_to_alis015()
        => InGauge("public Property<int> Value => Property<int>();", Type + "type\tother\tapi\tGauge\tinterface\tGauge\n");

    [AcceptedCase]
    public Task an_unknown_contract_format_is_not_read()
        => InGauge("public Property<int> Value => Property<int>();", "format\t2\n");

    [RefusedCase]
    public Task readonly_also_requires_a_public_member()
        => InGauge("public ReadOnly<int> {|#0:Value|} => ReadOnly<int>();", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task readonly_can_name_a_public_member()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>();", Value);

    [RefusedCase]
    public Task writeonly_also_requires_a_public_member()
        => InGauge("public WriteOnly<int> {|#0:Value|} => WriteOnly<int>();", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task writeonly_can_name_a_public_member()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Value);

    [RefusedCase]
    public Task method_also_requires_a_public_member()
        => InGauge("public void {|#0:Value|}() => Method();", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task method_can_name_a_public_member()
        => InGauge("public void Value() => Method();", Value);

    [RefusedCase]
    public Task methodasync_also_requires_a_public_member()
        => InGauge("public void {|#0:Value|}() => MethodAsync();", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task methodasync_can_name_a_public_member()
        => InGauge("public void Value() => MethodAsync();", Value);

    [AcceptedCase]
    public Task a_reached_declaration_without_its_own_identity_is_not_a_root()
        => Source("public sealed class Gauge : JsObject { public Property<int> Value => Property<int>(); }", Type);

    [AcceptedCase]
    public Task a_derived_declaration_does_not_inherit_a_root_identity()
        => Source("[Mirrors(\"api\", \"Gauge\")] public class Root : JsObject { } public sealed class Gauge : Root { public Property<int> Value => Property<int>(); }", Type);

    [AcceptedCase]
    public Task an_unrelated_method_named_like_a_door_is_not_checked()
        => InGauge("private int Property<T>() => 1; public int Value => Property<int>();", Type);

    [AcceptedCase]
    public Task a_door_on_another_object_is_left_to_alis023()
        => InGauge("public Property<int> Value => new Gauge().Property<int>();", Type);

    [AcceptedCase]
    public Task a_door_inside_a_lambda_is_left_to_alis023()
        => InGauge("public System.Func<Property<int>> Value => () => Property<int>();", Type);

    [AcceptedCase]
    public Task a_block_body_and_an_explicit_this_are_read()
        => InGauge("public Property<int> Value { get { return this.Property<int>(); } }", Value);

    [AcceptedCase]
    public Task camel_case_keeps_an_acronyms_last_capital_with_the_next_word()
        => InGauge("public Property<int> URLValue => Property<int>();", Value.Replace("value", "urlValue"));

    [AcceptedCase]
    public Task camel_case_lowers_a_whole_acronym()
        => InGauge("public Property<int> URL => Property<int>();", Value.Replace("value", "url"));

    [AcceptedCase]
    public Task camel_case_keeps_a_leading_underscore()
        => InGauge("public Property<int> _Value => Property<int>();", Value.Replace("value", "_Value"));

    [RefusedCase]
    public Task a_private_helper_declares_its_own_member()
        => InGauge("private Property<int> {|#0:Helper|} => Property<int>(); public Property<int> Value => Helper;", Value, "Declaration member 'Gauge.Helper' names 'helper', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [RefusedCase]
    public Task a_partial_member_is_reported_once_at_its_definition()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Property<int> {|#0:Value|} { get; } } public sealed partial class Gauge { public partial Property<int> Value => Property<int>(); }", Type, "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_declaration_doors()
    {
        var test = new Case<ALIS016PublicMemberExists>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public int Value => 1; }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Type));
        return test.RunAsync();
    }

    [RefusedCase]
    public async Task a_private_vendor_member_is_absent_from_the_extracted_contract()
    {
        using var program = new VendorProgram("export class Gauge { private value = 1; }");
        var table = await program.Extract();
        await InGauge("public Property<int> {|#0:Value|} => Property<int>();", string.Join("\n", table), "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");
    }

    [AcceptedCase]
    public async Task a_public_vendor_member_is_present_in_the_extracted_contract()
    {
        using var program = new VendorProgram("export class Gauge { public value = 1; }");
        var table = await program.Extract();
        await InGauge("public Property<int> Value => Property<int>();", string.Join("\n", table));
    }

    [AcceptedCase]
    public async Task a_generic_base_supplies_its_public_member()
    {
        using var program = new VendorProgram("export interface Base<T> { value: T; } export interface Gauge extends Base<number> { }");
        var table = await program.Extract();
        await InGauge("public Property<int> Value => Property<int>();", string.Join("\n", table));
    }

    [RefusedCase]
    public Task a_member_of_an_unrelated_type_is_not_inherited()
        => InGauge("public Property<int> {|#0:Value|} => Property<int>();", Type + "type\tbase\tapi\tBase\tinterface\tBase\nmember\tbase.value\tbase\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\n", "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [AcceptedCase]
    public Task a_member_of_a_named_base_is_inherited()
        => InGauge("public Property<int> Value => Property<int>();", Type + "type\tbase\tapi\tBase\tinterface\tBase\nmember\tbase.value\tbase\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\nbase\tapi:Gauge@1\t0\tbase\n");

    [RefusedCase]
    public Task a_cycle_of_bases_terminates_without_inventing_a_member()
        => InGauge("public Property<int> {|#0:Value|} => Property<int>();", Type + "base\tapi:Gauge@1\t0\tapi:Gauge@1\n", "Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type");

    [RefusedCase]
    public Task ids_in_another_table_do_not_supply_a_member()
    {
        var test = new Case<ALIS016PublicMemberExists>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; [Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { public Property<int> {|#0:Value|} => Property<int>(); }");
        test.TestState.AdditionalFiles.Add(("/TestProject/first/vendor-contract.tsv", Type));
        test.TestState.AdditionalFiles.Add(("/TestProject/second/vendor-contract.tsv", Value.Replace("\tapi\tGauge", "\tother\tGauge")));
        return test.Expecting(new DiagnosticResult(ALIS016PublicMemberExists.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage("Declaration member 'Gauge.Value' names 'value', which type 'Gauge' in module 'api' and its bases do not make public; name a public member of that type")).RunAsync();
    }

    [AcceptedCase]
    public Task member_names_are_unescaped_from_format_one()
        => InGauge("public Property<int> Value => Property<int>(name: \"a\\tb\\nc\\r\\\\d\");", Value.Replace("\tvalue\t", "\ta\\tb\\nc\\r\\\\d\t"));

    [AcceptedCase]
    public Task non_b2_doors_are_left_to_their_own_rules()
        => InGauge("public Gauge Other => LiveObject<Gauge>(); public Event Raised => Event(); public Callback<Gauge, int> Answer => Callback<Gauge, int>();", Type);

    [AcceptedCase]
    public Task a_partial_property_can_name_a_public_member()
        => Source("""[Mirrors("api", "Gauge")] public sealed partial class Gauge : JsObject { public partial Property<int> Value { get; } } public sealed partial class Gauge { public partial Property<int> Value => Property<int>(); }""", Value);

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS016PublicMemberExists>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(ALIS016PublicMemberExists.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
