using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS010SliceUsesEarlierRows : DiagnosticAnalyzer
{
    public const string Id = "ALIS010";

    private const string ALaterRow = "a later row";
    private const string ItsOwnRow = "its own row";

    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "A slice uses only the rows before its own",
        "'{0}' uses '{1}' of slice '{2}', {3} in SliceTree.txt: a slice uses only the rows before its own",
        "Alis.Reactive.Analyzers",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(Rule);

    private static readonly SymbolDisplayFormat AsTheDeveloperNamesIt = new(
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var table = SliceTable.Read(start.Options, start.CancellationToken);
        if (table is null) return;

        var project = start.Compilation.Assembly;
        start.RegisterSymbolAction(
            context => Declared(context, table, project),
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event);
        start.RegisterSyntaxNodeAction(context => Imported(context, table, project), SyntaxKind.UsingDirective);
        start.RegisterOperationAction(
            context => Written(context, table, project),
            OperationKind.Invocation,
            OperationKind.ObjectCreation,
            OperationKind.Attribute,
            OperationKind.FieldReference,
            OperationKind.PropertyReference,
            OperationKind.EventReference,
            OperationKind.MethodReference,
            OperationKind.TypeOf,
            OperationKind.IsType,
            OperationKind.SizeOf,
            OperationKind.NameOf,
            OperationKind.DefaultValue,
            OperationKind.ArrayCreation,
            OperationKind.TypePattern,
            OperationKind.DeclarationPattern,
            OperationKind.RecursivePattern,
            OperationKind.VariableDeclarator,
            OperationKind.LocalReference,
            OperationKind.AnonymousFunction,
            OperationKind.LocalFunction,
            OperationKind.Conversion,
            OperationKind.BinaryOperator,
            OperationKind.UnaryOperator,
            OperationKind.Increment,
            OperationKind.Decrement,
            OperationKind.CompoundAssignment);
    }

    private static void Declared(SymbolAnalysisContext context, SliceTable table, IAssemblySymbol project)
    {
        var symbol = context.Symbol;
        if (symbol.IsImplicitlyDeclared || symbol is IMethodSymbol { AssociatedSymbol: not null }) return;
        if (NameOf(symbol) is not { } name) return;

        var used = new List<ISymbol>();
        switch (symbol)
        {
            case INamedTypeSymbol type:
                Collect(type.BaseType, used);
                foreach (var contract in type.Interfaces) Collect(contract, used);
                CollectConstraints(type.TypeParameters, used);
                if (type.DelegateInvokeMethod is { } invoke)
                {
                    Collect(invoke.ReturnType, used);
                    ReportParameters(context.ReportDiagnostic, table, project, invoke.Parameters);
                }

                break;
            case IMethodSymbol method:
                Collect(method.ReturnType, used);
                CollectConstraints(method.TypeParameters, used);
                foreach (var contract in method.ExplicitInterfaceImplementations) Collect(contract.ContainingType, used);
                ReportParameters(context.ReportDiagnostic, table, project, method.Parameters);
                break;
            case IPropertySymbol property:
                Collect(property.Type, used);
                foreach (var contract in property.ExplicitInterfaceImplementations) Collect(contract.ContainingType, used);
                ReportParameters(context.ReportDiagnostic, table, project, property.Parameters);
                break;
            case IFieldSymbol field:
                Collect(field.Type, used);
                break;
            case IEventSymbol raised:
                Collect(raised.Type, used);
                foreach (var contract in raised.ExplicitInterfaceImplementations) Collect(contract.ContainingType, used);
                break;
        }

        Report(context.ReportDiagnostic, table, project, name, used);
    }

    private static void Imported(SyntaxNodeAnalysisContext context, SliceTable table, IAssemblySymbol project)
    {
        var directive = (UsingDirectiveSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(directive.NamespaceOrType, context.CancellationToken).Symbol is not ITypeSymbol type) return;

        var used = new List<ISymbol>();
        Collect(type, used);
        Report(context.ReportDiagnostic, table, project, directive.NamespaceOrType.GetLocation(), used);
    }

    private static void Written(OperationAnalysisContext context, SliceTable table, IAssemblySymbol project)
    {
        var operation = context.Operation;
        if (operation.IsImplicit && operation.Kind != OperationKind.Conversion) return;

        switch (operation)
        {
            case IAnonymousFunctionOperation lambda:
                ReportParameters(context.ReportDiagnostic, table, project, lambda.Symbol.Parameters);
                break;
            case ILocalFunctionOperation function:
                ReportParameters(context.ReportDiagnostic, table, project, function.Symbol.Parameters);
                break;
        }

        var used = new List<ISymbol>();
        CollectUsed(operation, used);
        Report(context.ReportDiagnostic, table, project, WhereItIsWritten(operation), used);
    }

    private static void CollectUsed(IOperation operation, List<ISymbol> used)
    {
        switch (operation)
        {
            case IInvocationOperation { TargetMethod.MethodKind: MethodKind.Constructor } call:
                Collect(call.TargetMethod.ContainingType, used);
                break;
            case IInvocationOperation call:
                used.Add((call.TargetMethod.ReducedFrom ?? call.TargetMethod).OriginalDefinition);
                CollectEach(call.TargetMethod.TypeArguments, used);
                break;
            case INameOfOperation named when operation.SemanticModel?.GetSymbolInfo(named.Argument.Syntax).Symbol is ITypeSymbol type:
                Collect(type, used);
                break;
            case IObjectCreationOperation creation when creation.Parent is not IAttributeOperation:
                Collect(creation.Type, used);
                break;
            case IAttributeOperation { Operation: IObjectCreationOperation applied }:
                Collect(applied.Type, used);
                break;
            case IMemberReferenceOperation reference:
                used.Add(reference.Member.OriginalDefinition);
                if (reference.Member is IMethodSymbol group) CollectEach(group.TypeArguments, used);
                break;
            case ITypeOfOperation typeOf:
                Collect(typeOf.TypeOperand, used);
                break;
            case IIsTypeOperation isType:
                Collect(isType.TypeOperand, used);
                break;
            case ISizeOfOperation sizeOf:
                Collect(sizeOf.TypeOperand, used);
                break;
            case IDefaultValueOperation when operation.Syntax is DefaultExpressionSyntax:
            case IArrayCreationOperation when operation.Syntax is ArrayCreationExpressionSyntax:
                Collect(operation.Type, used);
                break;
            case ITypePatternOperation pattern:
                Collect(pattern.MatchedType, used);
                break;
            case IDeclarationPatternOperation pattern when operation.Syntax is DeclarationPatternSyntax:
                Collect(pattern.MatchedType, used);
                break;
            case IRecursivePatternOperation pattern when operation.Syntax is RecursivePatternSyntax { Type: not null }:
                Collect(pattern.MatchedType, used);
                if (pattern.DeconstructSymbol is { } deconstruct) used.Add(deconstruct.OriginalDefinition);
                break;
            case IVariableDeclaratorOperation declarator:
                Collect(declarator.Symbol.Type, used);
                break;
            case ILocalReferenceOperation { IsDeclaration: true } declared:
                Collect(declared.Local.Type, used);
                break;
            case IAnonymousFunctionOperation lambda when operation.Syntax is ParenthesizedLambdaExpressionSyntax { ReturnType: not null }:
                Collect(lambda.Symbol.ReturnType, used);
                break;
            case ILocalFunctionOperation function:
                Collect(function.Symbol.ReturnType, used);
                CollectConstraints(function.Symbol.TypeParameters, used);
                break;
            case IConversionOperation conversion:
                if (conversion.OperatorMethod is { } converter) used.Add(converter.OriginalDefinition);
                if (!conversion.IsImplicit) Collect(conversion.Type, used);
                break;
            case IBinaryOperation { OperatorMethod: { } declared }:
                used.Add(declared.OriginalDefinition);
                break;
            case IUnaryOperation { OperatorMethod: { } declared }:
                used.Add(declared.OriginalDefinition);
                break;
            case IIncrementOrDecrementOperation { OperatorMethod: { } declared }:
                used.Add(declared.OriginalDefinition);
                break;
            case ICompoundAssignmentOperation { OperatorMethod: { } declared }:
                used.Add(declared.OriginalDefinition);
                break;
        }
    }

    private static Location WhereItIsWritten(IOperation operation) => operation switch
    {
        IInvocationOperation or IMemberReferenceOperation => NameOf(operation.Syntax),
        INameOfOperation named => NameOf(named.Argument.Syntax),
        IVariableDeclaratorOperation declarator => NameOf(declarator.Symbol) ?? operation.Syntax.GetLocation(),
        ILocalReferenceOperation declared => NameOf(declared.Local) ?? operation.Syntax.GetLocation(),
        ILocalFunctionOperation function => NameOf(function.Symbol) ?? operation.Syntax.GetLocation(),
        IConversionOperation { IsImplicit: true } => operation.Syntax.GetLocation(),
        _ => operation.Syntax switch
        {
            ObjectCreationExpressionSyntax creation => creation.Type.GetLocation(),
            ImplicitObjectCreationExpressionSyntax creation => creation.NewKeyword.GetLocation(),
            AttributeSyntax attribute => attribute.Name.GetLocation(),
            TypeOfExpressionSyntax typeOf => typeOf.Type.GetLocation(),
            SizeOfExpressionSyntax sizeOf => sizeOf.Type.GetLocation(),
            DefaultExpressionSyntax value => value.Type.GetLocation(),
            ArrayCreationExpressionSyntax array => array.Type.ElementType.GetLocation(),
            DeclarationPatternSyntax pattern => pattern.Type.GetLocation(),
            RecursivePatternSyntax { Type: { } type } => type.GetLocation(),
            CastExpressionSyntax cast => cast.Type.GetLocation(),
            ParenthesizedLambdaExpressionSyntax { ReturnType: { } returned } => returned.GetLocation(),
            BinaryExpressionSyntax binary when operation is IIsTypeOperation or IConversionOperation => binary.Right.GetLocation(),
            BinaryExpressionSyntax binary => binary.OperatorToken.GetLocation(),
            PrefixUnaryExpressionSyntax unary => unary.OperatorToken.GetLocation(),
            PostfixUnaryExpressionSyntax unary => unary.OperatorToken.GetLocation(),
            AssignmentExpressionSyntax assignment => assignment.OperatorToken.GetLocation(),
            var written => written.GetLocation(),
        },
    };

    private static void ReportParameters(Action<Diagnostic> report, SliceTable table, IAssemblySymbol project, ImmutableArray<IParameterSymbol> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.IsImplicitlyDeclared || NameOf(parameter) is not { } name) continue;
            var used = new List<ISymbol>();
            Collect(parameter.Type, used);
            Report(report, table, project, name, used);
        }
    }

    private static void Report(Action<Diagnostic> report, SliceTable table, IAssemblySymbol project, Location at, List<ISymbol> used)
    {
        if (used.Count == 0 || at.SourceTree is not { } tree || table.Place(tree).Slice is not { } user) return;

        var reported = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var symbol in used)
        {
            if (!reported.Add(symbol) || SliceOf(symbol, table, project) is not { } its) continue;
            if (its.Row < user.Row || string.Equals(its.Name, user.Name, StringComparison.Ordinal)) continue;
            report(Diagnostic.Create(Rule, at, user.Name, symbol.ToDisplayString(AsTheDeveloperNamesIt), its.Name, its.Row == user.Row ? ItsOwnRow : ALaterRow));
        }
    }

    private static Slice? SliceOf(ISymbol symbol, SliceTable table, IAssemblySymbol project)
    {
        if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, project)) return null;

        var declared = symbol.IsImplicitlyDeclared && symbol.ContainingType is { } holder ? holder : symbol;
        Slice? found = null;
        foreach (var part in declared.DeclaringSyntaxReferences)
        {
            if (table.Place(part.SyntaxTree).Slice is not { } slice) return null;
            if (found is not null && !string.Equals(found.Name, slice.Name, StringComparison.Ordinal)) return null;
            found = slice;
        }

        return found;
    }

    private static void CollectConstraints(ImmutableArray<ITypeParameterSymbol> parameters, List<ISymbol> used)
    {
        foreach (var parameter in parameters)
        {
            foreach (var constraint in parameter.ConstraintTypes) Collect(constraint, used);
        }
    }

    private static void CollectEach(ImmutableArray<ITypeSymbol> types, List<ISymbol> used)
    {
        foreach (var type in types) Collect(type, used);
    }

    private static void Collect(ITypeSymbol? type, List<ISymbol> used)
    {
        switch (type)
        {
            case IArrayTypeSymbol array:
                Collect(array.ElementType, used);
                break;
            case IPointerTypeSymbol pointer:
                Collect(pointer.PointedAtType, used);
                break;
            case IFunctionPointerTypeSymbol function:
                Collect(function.Signature.ReturnType, used);
                foreach (var parameter in function.Signature.Parameters) Collect(parameter.Type, used);
                break;
            case INamedTypeSymbol named:
                used.Add(named.OriginalDefinition);
                foreach (var argument in named.TypeArguments) Collect(argument, used);
                break;
        }
    }

    private static Location? NameOf(ISymbol symbol)
        => symbol.Locations.Where(location => location.IsInSource)
            .OrderBy(location => location.SourceTree!.FilePath, StringComparer.Ordinal)
            .ThenBy(location => location.SourceSpan.Start)
            .FirstOrDefault();

    private static Location NameOf(SyntaxNode written) => written switch
    {
        InvocationExpressionSyntax call => NameOf(call.Expression),
        MemberAccessExpressionSyntax access => access.Name.GetLocation(),
        MemberBindingExpressionSyntax binding => binding.Name.GetLocation(),
        ConditionalAccessExpressionSyntax access => NameOf(access.WhenNotNull),
        ConstructorInitializerSyntax initializer => initializer.ThisOrBaseKeyword.GetLocation(),
        PrimaryConstructorBaseTypeSyntax inherited => inherited.Type.GetLocation(),
        _ => written.GetLocation(),
    };
}
