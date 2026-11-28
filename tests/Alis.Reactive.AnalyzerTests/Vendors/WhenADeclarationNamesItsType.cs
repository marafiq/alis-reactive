using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenADeclarationNamesItsType
{
    [RefusedCase]
    public Task a_component_names_the_type_it_mirrors()
        => Source("public sealed class {|#0:Gauge|} : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }", true, Refused());

    [AcceptedCase]
    public Task a_component_with_its_own_mirrors_is_accepted()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }");

    [RefusedCase]
    public Task a_plugin_names_the_type_it_mirrors()
        => Source("public sealed class {|#0:Gauge|} : Plugin { public Gauge() : base(\"gauge\") { } }", true, Refused());

    [AcceptedCase]
    public Task a_plugin_with_its_own_mirrors_is_accepted()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }");

    [AcceptedCase]
    public Task a_component_without_a_contract_table_has_not_opted_in()
        => Source("public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }", false);

    [AcceptedCase]
    public Task a_plugin_without_a_contract_table_has_not_opted_in()
        => Source("public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }", false);

    [AcceptedCase]
    public Task an_unrelated_additional_file_does_not_opt_in()
    {
        var test = SourceCase("public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }");
        test.TestState.AdditionalFiles.Add(("/TestProject/not-vendor-contract.tsv", "format\t1\n"));
        return test.RunAsync();
    }

    [RefusedCase]
    public Task a_derived_component_cannot_inherit_its_bases_mirrors()
        => Source("[Mirrors(\"api\", \"Base\")] public abstract class Base<T> : Component { public override ComponentVendor Vendor => ComponentVendor.Native; } public sealed class {|#0:Gauge|} : Base<int>;", true, Refused());

    [AcceptedCase]
    public Task a_derived_component_names_its_own_type()
        => Source("[Mirrors(\"api\", \"Base\")] public abstract class Base<T> : Component { public override ComponentVendor Vendor => ComponentVendor.Native; } [Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Base<int>;");

    [RefusedCase]
    public Task an_abstract_component_declaration_names_its_type()
        => Source("public abstract class {|#0:Gauge|} : Component;", true, Refused());

    [RefusedCase]
    public Task a_derived_plugin_names_its_own_type()
        => Source("[Mirrors(\"api\", \"Base\")] public abstract class Base<T> : Plugin { protected Base() : base(\"gauge\") { } } public sealed class {|#0:Gauge|} : Base<int>;", true, Refused());

    [AcceptedCase]
    public Task a_plain_object_needs_no_root_identity()
        => Source("public sealed class Gauge : JsObject;");

    [AcceptedCase]
    public Task an_evented_object_needs_no_root_identity()
        => Source("public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.EventTarget; }");

    [AcceptedCase]
    public Task an_unrelated_component_is_not_a_framework_declaration()
        => Source("public abstract class Component; public sealed class Gauge : Component;");

    [RefusedCase]
    public Task a_same_named_attribute_does_not_name_the_vendor_type()
        => Source("public sealed class MirrorsAttribute : System.Attribute; [Mirrors] public sealed class {|#0:Gauge|} : Plugin { public Gauge() : base(\"gauge\") { } }", true, Refused());

    [AcceptedCase]
    public Task an_attribute_alias_names_the_framework_attribute()
        => Source("using Mirror = Alis.Reactive.Declarations.MirrorsAttribute; [Mirror(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }");

    [RefusedCase]
    public Task a_partial_declaration_is_reported_once_at_its_name()
    {
        var test = SourceCase("public sealed partial class {|#0:Gauge|} : Plugin { public Gauge() : base(\"gauge\") { } }");
        test.WithFile("Gauge.More.cs", "public sealed partial class Gauge;");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", "format\t1\n"));
        return test.Expecting(Refused()).RunAsync();
    }

    [AcceptedCase]
    public Task a_partial_declarations_attribute_can_be_on_another_part()
    {
        var test = SourceCase("public sealed partial class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }");
        test.WithFile("Gauge.More.cs", "[Alis.Reactive.Declarations.Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge;");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", "format\t1\n"));
        return test.RunAsync();
    }

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_framework_declarations()
    {
        var test = new Case<ALIS014DeclarationNamesItsType>().WithoutTheCore()
            .WithFile("Gauge.cs", "public abstract class Component; public sealed class Gauge : Component;");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", "format\t1\n"));
        return test.RunAsync();
    }

    [AcceptedCase]
    public Task an_incomplete_base_keeps_its_compiler_error()
        => Source("public sealed class Gauge : {|CS0246:Missing|};");

    private static Task Source(string source, bool table = true, params DiagnosticResult[] reports)
    {
        var test = SourceCase(source);
        if (table) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", "format\t1\n"));
        return test.Expecting(reports).RunAsync();
    }

    private static Case<ALIS014DeclarationNamesItsType> SourceCase(string source)
        => new Case<ALIS014DeclarationNamesItsType>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.Vendors; using Alis.Reactive.Plugins; " + source);

    private static DiagnosticResult Refused()
        => new DiagnosticResult(ALIS014DeclarationNamesItsType.Id, DiagnosticSeverity.Error).WithLocation(0)
            .WithMessage("Declaration 'Gauge' names no TypeScript type; add [Mirrors(module, name)] for the type it mirrors");
}
