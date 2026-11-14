using System.Threading.Tasks;
using Alis.Reactive.Analyzers.TypeKnowledge;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.TypeKnowledge;

[TestFixture]
public sealed class WhenADeclarationDoorIsCalled
{
    [RefusedCase]
    public Task a_door_called_in_a_constructor_is_refused_at_its_call()
        => InABed(
            "public WardBed() { {|#0:ReadOnly<string>()|}; }",
            InAConstructor(0, "WardBed", "ReadOnly"));

    [RefusedCase]
    public Task a_door_called_in_a_finalizer_is_refused_at_its_call()
        => InABed(
            "~WardBed() { {|#0:Method()|}; }",
            InAFinalizer(0, "WardBed", "Method"));

    [RefusedCase]
    public Task a_door_called_in_a_setter_is_refused_at_its_call()
        => InABed(
            """
            private ReadOnly<string> _resident;

            public ReadOnly<string> Resident { get => _resident; set => _resident = {|#0:ReadOnly<string>()|}; }
            """,
            InASetter(0, "WardBed.Resident", "ReadOnly"));

    [RefusedCase]
    public Task a_door_called_in_an_indexer_is_refused_at_its_call()
        => InABed(
            "public ReadOnly<string> this[int bed] => {|#0:ReadOnly<string>()|};",
            InAnIndexer(0, "WardBed", "ReadOnly"));

    [RefusedCase]
    public Task a_door_called_in_an_event_accessor_is_refused_at_its_call()
        => InABed(
            "public event Action Turned { add => {|#0:Method()|}; remove { } }",
            InAnEventAccessor(0, "WardBed.Turned", "Method"));

    [RefusedCase]
    public Task a_door_called_on_another_object_is_refused_at_its_call()
        => InABed(
            "public ReadOnly<string> Resident => {|#0:new WardBed().ReadOnly<string>()|};",
            OnAnotherObject(0, "WardBed.Resident", "ReadOnly"));

    [RefusedCase]
    public Task a_door_a_static_member_calls_on_an_object_it_makes_is_refused_at_its_call()
        => InABed(
            "public static ReadOnly<string> Shared() => {|#0:new WardBed().ReadOnly<string>()|};",
            OnAnotherObject(0, "WardBed.Shared", "ReadOnly"));

    [RefusedCase]
    public Task a_door_in_each_arm_of_a_condition_is_refused_at_its_call()
        => InABed(
            """public ReadOnly<string> Resident => Older ? {|#0:ReadOnly<string>(name: "resident")|} : {|#1:ReadOnly<string>(name: "patient")|};""",
            InACondition(0, "WardBed.Resident", "ReadOnly"),
            InACondition(1, "WardBed.Resident", "ReadOnly"));

    [RefusedCase]
    public Task a_door_an_if_statement_calls_is_refused_at_its_call()
        => InABed(
            "public void Turn() { if (Older) {|#0:Method()|}; }",
            InACondition(0, "WardBed.Turn", "Method"));

    [RefusedCase]
    public Task a_door_called_in_a_lambda_of_its_member_is_refused_at_its_call()
        => InABed(
            "public Func<ReadOnly<string>> Later => () => {|#0:ReadOnly<string>()|};",
            InALambda(0, "WardBed.Later", "ReadOnly"));

    [RefusedCase]
    public Task a_door_inside_another_doors_arguments_is_refused_at_its_call()
        => InABed(
            "public void Ring() => Method([(Arg<string>){|#0:Argument<string>(0)|}]);",
            InAnotherDoorsArguments(0, "WardBed.Ring", "Argument", "Method"));

    [RefusedCase]
    public Task a_door_beside_another_statement_of_its_member_is_refused_at_its_call()
        => InABed(
            "public void Turn() { Console.WriteLine(); {|#0:Method()|}; }",
            BesideMoreInItsBody(0, "WardBed.Turn", "Method"));

    [RefusedCase]
    public Task a_door_whose_result_its_member_reads_further_is_refused_at_its_call()
        => InABed(
            "public TypedSource<int> Letters => {|#0:ReadOnly<string>()|}.Read(text => text.Length);",
            BesideMoreInItsBody(0, "WardBed.Letters", "ReadOnly"));

    [RefusedCase]
    public Task a_door_whose_result_a_switch_reads_is_refused_at_its_call()
        => InABed(
            "public void Turn() { switch ({|#0:Method<string>()|}) { default: break; } }",
            BesideMoreInItsBody(0, "WardBed.Turn", "Method"));

    [RefusedCase]
    public Task a_declared_by_the_developer_names_is_refused_at_its_argument()
        => InABed(
            """public ReadOnly<string> Resident => ReadOnly<string>({|#0:declaredBy: "Room"|});""",
            NamedByTheDeveloper(0, "WardBed.Resident", "ReadOnly"));

    [RefusedCase]
    public Task a_declared_by_the_developer_passes_by_its_place_is_refused_at_its_argument()
        => InABed(
            """public ReadOnly<string> Resident => ReadOnly<string>(null, false, {|#0:"Room"|});""",
            NamedByTheDeveloper(0, "WardBed.Resident", "ReadOnly"));

    [RefusedCase]
    public Task an_explicit_interface_member_is_named_by_its_simple_name()
        => InATurningBed(
            """
            void ITurning.Turn() => Method();

            ReadOnly<string> ITurning.Label => ReadOnly<string>({|#0:declaredBy: "Title"|});
            """,
            NamedByTheDeveloper(0, "TurningBed.Label", "ReadOnly"));

    [AcceptedCase]
    public Task a_door_that_is_its_propertys_whole_body_is_accepted()
        => InABed("public ReadOnly<string> Resident => ReadOnly<string>();");

    [AcceptedCase]
    public Task a_door_the_one_statement_of_a_getter_returns_is_accepted()
        => InABed("""public ReadOnly<string> Room { get { return ReadOnly<string>(name: "room"); } }""");

    [AcceptedCase]
    public Task a_door_that_is_the_one_statement_of_its_method_is_accepted()
        => InABed("public void Turn() { Method(); }");

    [AcceptedCase]
    public Task a_door_in_the_getter_of_a_property_with_a_setter_is_accepted()
        => InABed(
            """
            private ReadOnly<string> _resident;

            public ReadOnly<string> Resident { get => ReadOnly<string>(); set => _resident = value; }
            """);

    [AcceptedCase]
    public Task a_door_called_on_its_own_object_by_name_is_accepted()
        => InABed("public ReadOnly<string> Resident => this.ReadOnly<string>();");

    [AcceptedCase]
    public Task a_member_the_declarations_base_holds_for_the_core_is_no_door()
        => new Case<ALIS023DeclarationDoorIsItsMembersBody>()
            .WithFile("Ward/WardBed.cs",
                """
                namespace Alis.Reactive.Declarations
                {
                    public abstract class JsObject
                    {
                        internal void Reach() { }
                    }
                }

                namespace TestProject.Ward
                {
                    public sealed class WardBed : Alis.Reactive.Declarations.JsObject
                    {
                        public WardBed() { Reach(); }
                    }
                }
                """)
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task a_door_written_in_parentheses_is_accepted()
        => InABed("public ReadOnly<string> Wing => (ReadOnly<string>());");

    [AcceptedCase]
    public Task a_door_whose_member_is_typed_wider_than_its_result_is_accepted()
        => InABed("public TypedSource<string> Wing => ReadOnly<string>();");

    [AcceptedCase]
    public Task an_argument_maker_inside_a_doors_arguments_is_accepted()
        => InABed("public void Ring() => Method([Fixed(1)]);");

    [AcceptedCase]
    public Task a_name_the_developer_writes_is_accepted()
        => InABed("""public ReadOnly<string> Resident => ReadOnly<string>(name: "resident");""");

    [AcceptedCase]
    public Task an_explicit_interface_member_whose_body_is_its_door_is_accepted()
        => InATurningBed(
            """
            void ITurning.Turn() => Method();

            ReadOnly<string> ITurning.Label => ReadOnly<string>();
            """);

    [AcceptedCase]
    public Task a_method_named_as_a_door_on_a_class_that_is_no_declaration_is_accepted()
        => new Case<ALIS023DeclarationDoorIsItsMembersBody>()
            .WithFile("Ward/WardChart.cs",
                """
                namespace TestProject.Ward;

                public abstract class Chart
                {
                    protected int ReadOnly() => 1;
                }

                public sealed class WardChart : Chart
                {
                    public WardChart() { ReadOnly(); }
                }
                """)
            .RunAsync();

    [AcceptedCase]
    public Task a_project_that_references_no_core_is_read_by_no_rule()
        => new Case<ALIS023DeclarationDoorIsItsMembersBody>()
            .WithFile("Ward/WardChart.cs", "namespace TestProject.Ward; public sealed class WardChart { public WardChart() { ReadOnly(); } private static int ReadOnly() => 1; }")
            .WithoutTheCore()
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InABed("public ReadOnly<string> Resident => {|CS0103:Missing|}(ReadOnly<string>());");

    private static Task InABed(string members, params DiagnosticResult[] reports)
        => new Case<ALIS023DeclarationDoorIsItsMembersBody>()
            .WithFile("Ward/WardBed.cs",
                $$"""
                using System;
                using Alis.Reactive.Declarations;
                using Alis.Reactive.ValuesAndSources;

                namespace TestProject.Ward;

                public sealed class WardBed : JsObject
                {
                    private static readonly bool Older = DateTime.Now.Year > 2000;

                    {{members}}
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static Task InATurningBed(string members, params DiagnosticResult[] reports)
        => new Case<ALIS023DeclarationDoorIsItsMembersBody>()
            .WithFile("Ward/TurningBed.cs",
                $$"""
                using Alis.Reactive.Declarations;

                namespace TestProject.Ward;

                public interface ITurning
                {
                    void Turn();

                    ReadOnly<string> Label { get; }
                }

                public sealed class TurningBed : JsObject, ITurning
                {
                    {{members}}
                }
                """)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult InAConstructor(int at, string declaration, string door)
        => Refused(at, $"The constructor of '{declaration}' calls {door}, and a constructor declares no JavaScript member; call {door} as the body of the property or method that declares the member");

    private static DiagnosticResult InAFinalizer(int at, string declaration, string door)
        => Refused(at, $"The finalizer of '{declaration}' calls {door}, and a finalizer declares no JavaScript member; call {door} as the body of the property or method that declares the member");

    private static DiagnosticResult InASetter(int at, string property, string door)
        => Refused(at, $"The setter of '{property}' calls {door}, and a setter declares no JavaScript member; call {door} as the body of the property or method that declares the member");

    private static DiagnosticResult InAnIndexer(int at, string declaration, string door)
        => Refused(at, $"The indexer of '{declaration}' calls {door}, and an indexer declares no JavaScript member; call {door} as the body of the property or method that declares the member");

    private static DiagnosticResult InAnEventAccessor(int at, string raised, string door)
        => Refused(at, $"An accessor of the event '{raised}' calls {door}, and an event accessor declares no JavaScript member; call {door} as the body of the property or method that declares the member");

    private static DiagnosticResult OnAnotherObject(int at, string member, string door)
        => Refused(at, $"'{member}' calls {door} on another object, and a door declares a member of the object it is called on; call {door} on the declaration's own object, as the body of the property or method that declares the member");

    private static DiagnosticResult InACondition(int at, string member, string door)
        => Refused(at, $"'{member}' calls {door} inside a condition, and a door is the whole body of the member it declares; make the call of {door} the whole body of '{member}'");

    private static DiagnosticResult InALambda(int at, string member, string door)
        => Refused(at, $"'{member}' calls {door} inside a lambda, and a door is the whole body of the member it declares; make the call of {door} the whole body of '{member}'");

    private static DiagnosticResult InAnotherDoorsArguments(int at, string member, string door, string taking)
        => Refused(at, $"'{member}' calls {door} inside the arguments of {taking}, and a door is the whole body of the member it declares; call {door} as the body of a member of its own, and pass that member to {taking}");

    private static DiagnosticResult BesideMoreInItsBody(int at, string member, string door)
        => Refused(at, $"The body of '{member}' holds more than its call of {door}, and a door is the whole body of the member it declares; make the call of {door} the whole body of '{member}'");

    private static DiagnosticResult NamedByTheDeveloper(int at, string member, string door)
        => Refused(at, $"'{member}' passes declaredBy: to {door}, and the compiler supplies the name of the member that declares it; remove the argument, and write name: for a JavaScript name that is not the C# member's name in camel case");

    private static DiagnosticResult Refused(int at, string message)
        => new DiagnosticResult(ALIS023DeclarationDoorIsItsMembersBody.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
