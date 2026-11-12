using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.TestRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TEST004TestIsNeverSkippedOrRepeated : DiagnosticAnalyzer
{
    public const string Id = "TEST004";

    private const string Title = "A test is never skipped, repeated, ordered or parameterized";
    private const string NUnit = "NUnit.Framework.";
    private const string Attribute = "Attribute";
    private const string That = "That";
    private const string LegacyAssert = "NUnit.Framework.Legacy.ClassicAssert";
    private const string TestAction = "NUnit.Framework.ITestAction";

    private static readonly string[] OfATest = ["Test", "TestFixtureSource", "Category"];
    private static readonly string[] OfTheHarness = ["SetUp", "TearDown", "OneTimeSetUp", "OneTimeTearDown", "SetUpFixture", "Parallelizable"];
    private static readonly string[] Asserting = ["Assert", "Assume", "Warn"];

    private static readonly DiagnosticDescriptor NoAttributeOfATest = new(
        Id,
        Title,
        "'[{0}]' is no attribute a browser test takes; a test carries [Test] and its fixture [TestFixtureSource] and [Category], so remove it",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SetUpOutsideTheHarness = new(
        Id,
        Title,
        "'[{0}]' sets a run up, which is the harness's alone; move it to a file under Harness",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NoAssertionOfATest = new(
        Id,
        Title,
        "'{0}' is no assertion a browser test takes; a test asserts with Assert.That and Expect alone, so remove it",
        TestCode.Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(NoAttributeOfATest, SetUpOutsideTheHarness, NoAssertionOfATest);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        if (start.Compilation.GetTypeByMetadataName(TestCode.NUnitsTest) is not { } test) return;

        var nunit = test.ContainingAssembly;
        var ofATest = TypesOf(start.Compilation, OfATest, Attribute);
        var ofTheHarness = TypesOf(start.Compilation, OfTheHarness, Attribute);
        var asserting = TypesOf(start.Compilation, Asserting, "");
        var assert = start.Compilation.GetTypeByMetadataName(NUnit + Asserting[0]);
        var legacy = start.Compilation.GetTypeByMetadataName(LegacyAssert)?.ContainingAssembly;
        var action = start.Compilation.GetTypeByMetadataName(TestAction);

        start.RegisterOperationAction(applied => ReadAttribute(applied, nunit, ofATest, ofTheHarness, action), OperationKind.Attribute);
        start.RegisterOperationAction(called => ReadCall(called, asserting, assert, legacy), OperationKind.Invocation);
        start.RegisterOperationAction(named => ReadName(named, asserting, assert, legacy), OperationKind.MethodReference);
    }

    private static ImmutableHashSet<INamedTypeSymbol> TypesOf(Compilation compilation, string[] names, string suffix)
        => names.Select(name => compilation.GetTypeByMetadataName(NUnit + name + suffix)).OfType<INamedTypeSymbol>()
            .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

    private static void ReadAttribute(
        OperationAnalysisContext context,
        IAssemblySymbol nunit,
        ImmutableHashSet<INamedTypeSymbol> ofATest,
        ImmutableHashSet<INamedTypeSymbol> ofTheHarness,
        INamedTypeSymbol? action)
    {
        var applied = (IAttributeOperation)context.Operation;
        if (applied.Operation is not IObjectCreationOperation { Constructor.ContainingType: { } attribute }) return;
        if (!IsOfNUnit(attribute, nunit) || ofATest.Contains(attribute)) return;

        var ofTheHarnessAlone = ofTheHarness.Contains(attribute) || IsATestAction(attribute, action);
        if (ofTheHarnessAlone && TestCode.IsInTheHarness(applied.Syntax.SyntaxTree)) return;

        var written = Last(((AttributeSyntax)applied.Syntax).Name);
        context.ReportDiagnostic(Diagnostic.Create(ofTheHarnessAlone ? SetUpOutsideTheHarness : NoAttributeOfATest, written.GetLocation(), written.Identifier.ValueText));
    }

    private static bool IsATestAction(INamedTypeSymbol attribute, INamedTypeSymbol? action)
        => action is not null && attribute.AllInterfaces.Contains(action, SymbolEqualityComparer.Default);

    private static bool IsOfNUnit(INamedTypeSymbol attribute, IAssemblySymbol nunit)
    {
        for (var declared = attribute; declared is not null; declared = declared.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(declared.ContainingAssembly, nunit)) return true;
        }

        return attribute.AllInterfaces.Any(taken => SymbolEqualityComparer.Default.Equals(taken.ContainingAssembly, nunit));
    }

    private static void ReadCall(OperationAnalysisContext context, ImmutableHashSet<INamedTypeSymbol> asserting, INamedTypeSymbol? assert, IAssemblySymbol? legacy)
    {
        var called = (IInvocationOperation)context.Operation;
        Read(context, called.TargetMethod, TestCode.NameOf(called), asserting, assert, legacy);
    }

    private static void ReadName(OperationAnalysisContext context, ImmutableHashSet<INamedTypeSymbol> asserting, INamedTypeSymbol? assert, IAssemblySymbol? legacy)
    {
        var named = (IMethodReferenceOperation)context.Operation;
        var written = named.Syntax is MemberAccessExpressionSyntax reached ? reached.Name : named.Syntax;
        Read(context, named.Method, written.GetLocation(), asserting, assert, legacy);
    }

    private static void Read(
        OperationAnalysisContext context, IMethodSymbol method, Location written, ImmutableHashSet<INamedTypeSymbol> asserting, INamedTypeSymbol? assert, IAssemblySymbol? legacy)
    {
        var holder = method.ContainingType.OriginalDefinition;
        var isALegacyAssertion = legacy is not null && SymbolEqualityComparer.Default.Equals(holder.ContainingAssembly, legacy);
        if (!asserting.Contains(holder) && !isALegacyAssertion) return;
        if (method.Name == That && SymbolEqualityComparer.Default.Equals(holder, assert)) return;

        context.ReportDiagnostic(Diagnostic.Create(NoAssertionOfATest, written, holder.Name + "." + method.Name));
    }

    private static SimpleNameSyntax Last(NameSyntax name) => name switch
    {
        QualifiedNameSyntax qualified => qualified.Right,
        AliasQualifiedNameSyntax aliased => aliased.Name,
        _ => (SimpleNameSyntax)name,
    };
}
