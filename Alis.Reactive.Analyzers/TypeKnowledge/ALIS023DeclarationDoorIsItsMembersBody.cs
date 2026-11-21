using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.TypeKnowledge;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS023DeclarationDoorIsItsMembersBody : DiagnosticAnalyzer
{
    public const string Id = "ALIS023";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string IsTheBody = "A declaration door is the body of the member it declares";
    private const string TheCompilersArgument = "declaredBy";

    private static readonly DiagnosticDescriptor InAConstructor = new(
        Id,
        IsTheBody,
        "The constructor of '{0}' calls {1}, and a constructor declares no JavaScript member; call {1} as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InAFinalizer = new(
        Id,
        IsTheBody,
        "The finalizer of '{0}' calls {1}, and a finalizer declares no JavaScript member; call {1} as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InASetter = new(
        Id,
        IsTheBody,
        "The setter of '{0}' calls {1}, and a setter declares no JavaScript member; call {1} as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InAnIndexer = new(
        Id,
        IsTheBody,
        "The indexer of '{0}' calls {1}, and an indexer declares no JavaScript member; call {1} as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InAnEventAccessor = new(
        Id,
        IsTheBody,
        "An accessor of the event '{0}' calls {1}, and an event accessor declares no JavaScript member; call {1} as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OnAnotherObject = new(
        Id,
        IsTheBody,
        "'{0}' calls {1} on another object, and a door declares a member of the object it is called on; call {1} on the declaration's own object, as the body of the property or method that declares the member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InACondition = new(
        Id,
        IsTheBody,
        "'{0}' calls {1} inside a condition, and a door is the whole body of the member it declares; make the call of {1} the whole body of '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InALambda = new(
        Id,
        IsTheBody,
        "'{0}' calls {1} inside a lambda, and a door is the whole body of the member it declares; make the call of {1} the whole body of '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InAnotherDoorsArguments = new(
        Id,
        IsTheBody,
        "'{0}' calls {1} inside the arguments of {2}, and a door is the whole body of the member it declares; call {1} as the body of a member of its own, and pass that member to {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor BesideMoreInItsBody = new(
        Id,
        IsTheBody,
        "The body of '{0}' holds more than its call of {1}, and a door is the whole body of the member it declares; make the call of {1} the whole body of '{0}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NamedByTheDeveloper = new(
        Id,
        "A declaration door's declaredBy: is the compiler's",
        "'{0}' passes declaredBy: to {1}, and the compiler supplies the name of the member that declares it; remove the argument, and write name: for a JavaScript name that is not the C# member's name in camel case",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(
        InAConstructor,
        InAFinalizer,
        InASetter,
        InAnIndexer,
        InAnEventAccessor,
        OnAnotherObject,
        InACondition,
        InALambda,
        InAnotherDoorsArguments,
        BesideMoreInItsBody,
        NamedByTheDeveloper);

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

        var member = context.ContainingSymbol;
        if (Misplaced(call, member, declarations) is { } misplaced) context.ReportDiagnostic(misplaced);

        foreach (var argument in call.Arguments)
        {
            if (argument is { ArgumentKind: ArgumentKind.Explicit, Parameter.Name: TheCompilersArgument })
                context.ReportDiagnostic(Diagnostic.Create(NamedByTheDeveloper, argument.Syntax.GetLocation(), DeclarationDoor.Named(member), call.TargetMethod.Name));
        }
    }

    private static Diagnostic? Misplaced(IInvocationOperation call, ISymbol member, INamedTypeSymbol declarations)
        => member switch
        {
            IMethodSymbol { MethodKind: MethodKind.Constructor } => Refused(InAConstructor, call, member.ContainingType),
            IMethodSymbol { MethodKind: MethodKind.Destructor } => Refused(InAFinalizer, call, member.ContainingType),
            IMethodSymbol { AssociatedSymbol: IEventSymbol } => Refused(InAnEventAccessor, call, member),
            IMethodSymbol { AssociatedSymbol: IPropertySymbol { IsIndexer: true } } => Refused(InAnIndexer, call, member.ContainingType),
            IMethodSymbol { AssociatedSymbol: IPropertySymbol property } when IsTheSetter(member, property) => Refused(InASetter, call, member),
            _ when !IsOnItsOwnObject(call) => Refused(OnAnotherObject, call, member),
            _ when !IsTheWholeBody(call) => HeldInItsBody(call, member, declarations),
            _ => null,
        };

    private static bool IsTheSetter(ISymbol accessor, IPropertySymbol property)
        => SymbolEqualityComparer.Default.Equals(property.SetMethod, accessor);

    private static bool IsOnItsOwnObject(IInvocationOperation call)
        => call.Instance is IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance };

    private static bool IsTheWholeBody(IInvocationOperation call)
    {
        IOperation held = call;
        while (held.Parent is IConversionOperation converted) held = converted;

        return held.Parent is IReturnOperation or IExpressionStatementOperation
               && held.Parent.Parent is IBlockOperation { Operations.Length: 1, Parent: null or IMethodBodyOperation };
    }

    private static Diagnostic? HeldInItsBody(IInvocationOperation call, ISymbol member, INamedTypeSymbol declarations)
    {
        for (var around = call.Parent; around is not null; around = around.Parent)
        {
            switch (around)
            {
                case IInvalidOperation:
                    return null;
                case IAnonymousFunctionOperation:
                    return Refused(InALambda, call, member);
                case IConditionalOperation:
                    return Refused(InACondition, call, member);
                case IArgumentOperation { Parent: IInvocationOperation taking } when DeclarationDoor.Is(taking.TargetMethod, declarations):
                    return Diagnostic.Create(InAnotherDoorsArguments, call.Syntax.GetLocation(), DeclarationDoor.Named(member), call.TargetMethod.Name, taking.TargetMethod.Name);
            }
        }

        return Refused(BesideMoreInItsBody, call, member);
    }

    private static Diagnostic Refused(DiagnosticDescriptor rule, IInvocationOperation call, ISymbol named)
        => Diagnostic.Create(rule, call.Syntax.GetLocation(), DeclarationDoor.Named(named), call.TargetMethod.Name);
}
