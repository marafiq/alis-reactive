using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.PartialsAndSlots;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS044DoorTakesNoDynamic : DiagnosticAnalyzer
{
    public const string Id = "ALIS044";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string Title = "A door of the framework takes no dynamic";
    private const string PlanDoors = "Alis.Reactive.RenderingAndDelivery.PlanHtml";
    private const string OpensAPlan = "ReactivePlan";
    private const string Dynamic = "dynamic";
    private const string Object = "object";

    private static readonly DiagnosticDescriptor HandedAnUntypedType = new(
        Id,
        Title,
        "{0} is handed {1} as {2}: a door is typed by the type it is handed, and {1} leaves the compiler nothing to check; name a class the app wrote, the view's @model for a plan",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor BoundAtRunTime = new(
        Id,
        Title,
        "{0} is bound at run time because {1} is dynamic, so the compiler checks none of it; give the view its model with @model, and its Model is typed",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(HandedAnUntypedType, BoundAtRunTime);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var planDoors = start.Compilation.GetTypeByMetadataName(PlanDoors);
        if (planDoors is null) return;
        if (start.Compilation.GetCompilationNamespace(planDoors.ContainingNamespace.ContainingNamespace) is not { } framework) return;

        var doors = new Doors(start.Compilation, framework, planDoors.GetMembers(OpensAPlan).OfType<IMethodSymbol>().ToImmutableArray());
        start.RegisterOperationAction(call => Called(call, doors), OperationKind.Invocation);
        start.RegisterOperationAction(creation => Created(creation, doors), OperationKind.ObjectCreation);
        start.RegisterOperationAction(bound => BoundLate(bound, doors), OperationKind.DynamicInvocation, OperationKind.DynamicObjectCreation);
    }

    private static void Called(OperationAnalysisContext context, Doors doors)
    {
        var call = (IInvocationOperation)context.Operation;
        var door = call.TargetMethod;
        if (!doors.Declared(door) || call.Syntax is not InvocationExpressionSyntax { Expression: var invoked } || NameOf(invoked) is not { } name) return;

        var written = name as GenericNameSyntax;
        var opensAPlan = doors.OpensAPlan(door);
        for (var at = 0; at < door.TypeArguments.Length; at++)
        {
            if (Untyped(door.TypeArguments[at], opensAPlan) is not { } untyped) continue;

            var location = written is null ? name.Identifier.GetLocation() : written.TypeArgumentList.Arguments[at].GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(HandedAnUntypedType, location, AsWritten(invoked, name), untyped, door.TypeParameters[at].Name));
        }
    }

    private static void Created(OperationAnalysisContext context, Doors doors)
    {
        var creation = (IObjectCreationOperation)context.Operation;
        if (creation.Constructor is not { } door || !doors.Declared(door)) return;

        var type = door.ContainingType;
        var written = creation.Syntax is ObjectCreationExpressionSyntax { Type: var named } ? GenericNameOf(named) : null;
        for (var at = 0; at < type.TypeArguments.Length; at++)
        {
            if (Untyped(type.TypeArguments[at], opensAPlan: false) is not { } untyped) continue;

            var location = written is null ? creation.Syntax.GetLocation() : written.TypeArgumentList.Arguments[at].GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(HandedAnUntypedType, location, type.Name, untyped, type.TypeParameters[at].Name));
        }
    }

    private static void BoundLate(OperationAnalysisContext context, Doors doors)
    {
        var bound = context.Operation;
        if (bound.SemanticModel is not { } model) return;

        var info = model.GetSymbolInfo(bound.Syntax, context.CancellationToken);
        var candidates = (info.Symbol is { } one ? ImmutableArray.Create(one) : info.CandidateSymbols).OfType<IMethodSymbol>();
        if (candidates.FirstOrDefault(doors.Declared) is not { } door) return;

        var name = door.MethodKind == MethodKind.Constructor ? door.ContainingType.Name : door.Name;
        var arguments = bound is IDynamicInvocationOperation call ? call.Arguments : ((IDynamicObjectCreationOperation)bound).Arguments;
        foreach (var argument in arguments.Where(argument => argument.Type?.TypeKind == TypeKind.Dynamic))
            context.ReportDiagnostic(Diagnostic.Create(BoundAtRunTime, argument.Syntax.GetLocation(), name, argument.Syntax.ToString()));
    }

    private static string? Untyped(ITypeSymbol handed, bool opensAPlan)
    {
        if (handed.TypeKind == TypeKind.Dynamic) return Dynamic;
        return opensAPlan && handed.SpecialType == SpecialType.System_Object ? Object : null;
    }

    private static SimpleNameSyntax? NameOf(ExpressionSyntax invoked) => invoked switch
    {
        MemberAccessExpressionSyntax access => access.Name,
        SimpleNameSyntax name => name,
        _ => null,
    };

    private static string AsWritten(ExpressionSyntax invoked, SimpleNameSyntax name)
        => invoked is MemberAccessExpressionSyntax access ? access.Expression + "." + name.Identifier.ValueText : name.Identifier.ValueText;

    private static GenericNameSyntax? GenericNameOf(TypeSyntax named) => named switch
    {
        GenericNameSyntax generic => generic,
        QualifiedNameSyntax qualified => GenericNameOf(qualified.Right),
        _ => null,
    };

    private sealed class Doors
    {
        private readonly Compilation _compilation;
        private readonly INamespaceSymbol _framework;
        private readonly ImmutableArray<IMethodSymbol> _opensAPlan;

        internal Doors(Compilation compilation, INamespaceSymbol framework, ImmutableArray<IMethodSymbol> opensAPlan)
        {
            _compilation = compilation;
            _framework = framework;
            _opensAPlan = opensAPlan;
        }

        internal bool OpensAPlan(IMethodSymbol door) => _opensAPlan.Contains(door.OriginalDefinition, SymbolEqualityComparer.Default);

        internal bool Declared(ISymbol declared)
        {
            if (SymbolEqualityComparer.Default.Equals(declared.ContainingAssembly, _compilation.Assembly)) return false;
            for (var held = declared.ContainingNamespace; held is not null; held = held.ContainingNamespace)
            {
                if (SymbolEqualityComparer.Default.Equals(_compilation.GetCompilationNamespace(held), _framework)) return true;
            }

            return false;
        }
    }
}
