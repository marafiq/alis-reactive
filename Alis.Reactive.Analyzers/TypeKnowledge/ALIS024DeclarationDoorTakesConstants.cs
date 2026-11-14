using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.TypeKnowledge;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS024DeclarationDoorTakesConstants : DiagnosticAnalyzer
{
    public const string Id = "ALIS024";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string AreConstants = "A declaration door's arguments are constants the build reads";
    private const string TheJavaScriptName = "name";
    private const string ThePresence = "optional";
    private const string TheNullability = "nullable";
    private const string ThePosition = "position";

    private static readonly DiagnosticDescriptor NameIsNoConstant = new(
        Id,
        AreConstants,
        "'{0}' passes '{1}' to {2} as name:, and the build reads the JavaScript name a door declares from a constant alone; write the name as a constant text",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OptionalIsNoConstant = new(
        Id,
        AreConstants,
        "'{0}' passes '{1}' to {2} as optional:, and the build reads whether a member is optional from a constant alone; write true or false",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NullableIsNoConstant = new(
        Id,
        AreConstants,
        "'{0}' passes '{1}' to {2} as nullable:, and the build reads whether an object holds null from a constant alone; write true or false",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor PositionIsNoConstant = new(
        Id,
        AreConstants,
        "'{0}' passes '{1}' to {2} as position:, and the build reads the position of an argument from a constant alone; write the position as a constant number",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(
        NameIsNoConstant,
        OptionalIsNoConstant,
        NullableIsNoConstant,
        PositionIsNoConstant);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var declarations = start.Compilation.GetTypeByMetadataName(DeclarationDoor.Declarations);
        if (declarations is null) return;

        start.RegisterOperationAction(call => Read(call, declarations), OperationKind.Invocation);
    }

    private static void Read(OperationAnalysisContext context, INamedTypeSymbol declarations)
    {
        var call = (IInvocationOperation)context.Operation;
        if (!DeclarationDoor.Is(call.TargetMethod, declarations)) return;

        foreach (var argument in call.Arguments)
        {
            if (argument.Value.ConstantValue.HasValue || RuleOf(argument.Parameter) is not { } rule) continue;

            context.ReportDiagnostic(Diagnostic.Create(rule, argument.Syntax.GetLocation(), DeclarationDoor.Named(context.ContainingSymbol), argument.Value.Syntax.ToString(), call.TargetMethod.Name));
        }
    }

    private static DiagnosticDescriptor? RuleOf(IParameterSymbol? taken)
        => taken?.Name switch
        {
            TheJavaScriptName => NameIsNoConstant,
            ThePresence => OptionalIsNoConstant,
            TheNullability => NullableIsNoConstant,
            ThePosition => PositionIsNoConstant,
            _ => null,
        };
}
