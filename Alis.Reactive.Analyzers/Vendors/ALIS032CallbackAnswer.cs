using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.Vendors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS032CallbackAnswer : DiagnosticAnalyzer
{
    public const string Id = "ALIS032";

    private const string Category = "Alis.Reactive.Analyzers";

    private static readonly DiagnosticDescriptor WritesAStep = new(
        Id,
        "A callback answers with one value",
        "A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline.",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor Waits = new(
        Id,
        "A callback answers at once",
        "A callback answers its vendor at once, so its answer cannot wait for an async method or a request",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor WritesADeferredStep = AtEnd(WritesAStep);
    private static readonly DiagnosticDescriptor WaitsForADeclaration = AtEnd(Waits);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(WritesAStep, Waits, WritesADeferredStep, WaitsForADeclaration);

    private static DiagnosticDescriptor AtEnd(DiagnosticDescriptor rule)
        => new(rule.Id, rule.Title, rule.MessageFormat, rule.Category, rule.DefaultSeverity, rule.IsEnabledByDefault, customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var declaration = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.JsObject");
        var callback = start.Compilation.GetTypeByMetadataName("Alis.Reactive.Declarations.Callback`2");
        if (declaration is null || callback is null) return;

        var steps = new Dictionary<IMethodSymbol, DiagnosticDescriptor>(SymbolEqualityComparer.Default);
        foreach (var method in declaration.GetMembers("MethodAsync").OfType<IMethodSymbol>()) steps.Add(method, Waits);
        foreach (var member in new[] { "Method", "MethodAt", "CallItself" })
        {
            foreach (var method in declaration.GetMembers(member).OfType<IMethodSymbol>().Where(method => method.ReturnsVoid)) steps.Add(method, WritesAStep);
        }

        var writes = new[]
        {
            ("Alis.Reactive.Declarations.Property`1", "Set"),
            ("Alis.Reactive.Declarations.WriteOnly`1", "Set"),
            ("Alis.Reactive.Declarations.NullableWrites", "Set"),
            ("Alis.Reactive.Declarations.Callback`2", "Set"),
            ("Alis.Reactive.Elements.ElementContent", "Set"),
            ("Alis.Reactive.Elements.ElementContent", "Append"),
            ("Alis.Reactive.TriggersAndPipelines.Guard`1", "Then"),
            ("Alis.Reactive.TriggersAndPipelines.Branches`1", "Else"),
        };
        foreach (var (typeName, member) in writes)
        {
            var type = start.Compilation.GetTypeByMetadataName(typeName);
            if (type is null) continue;
            foreach (var method in type.GetMembers(member).OfType<IMethodSymbol>()) steps.Add(method, WritesAStep);
        }

        var pipeline = start.Compilation.GetTypeByMetadataName("Alis.Reactive.TriggersAndPipelines.Pipeline`1");
        if (pipeline is not null)
        {
            foreach (var method in pipeline.GetMembers().OfType<IMethodSymbol>().Where(method => method is { MethodKind: MethodKind.Ordinary, ReturnsVoid: true }))
                steps.Add(method, WritesAStep);
            foreach (var member in new[] { "Get", "Head", "Options", "Post", "Put", "Delete" })
            {
                foreach (var method in pipeline.GetMembers(member).OfType<IMethodSymbol>()) steps.Add(method, Waits);
            }
        }

        var requestSteps = start.Compilation.GetTypeByMetadataName("Alis.Reactive.TriggersAndPipelines.RequestSteps");
        if (requestSteps is not null)
        {
            foreach (var method in requestSteps.GetMembers().OfType<IMethodSymbol>().Where(method => method.MethodKind == MethodKind.Ordinary))
                steps.Add(method, WritesAStep);
        }

        var answerParameters = callback.GetMembers("Set").Concat(declaration.GetMembers("Answer")).OfType<IMethodSymbol>()
            .Select(method => method.Parameters[0]).ToImmutableHashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var declarationDoors = declaration.GetMembers().OfType<IMethodSymbol>().ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var summaries = new ConcurrentDictionary<IMethodSymbol, Declaration>(SymbolEqualityComparer.Default);
        var pending = new ConcurrentBag<AnswerUse>();
        start.RegisterOperationBlockAction(context =>
        {
            if (context.OwningSymbol is not IMethodSymbol method || !IsDeclaration(method.ContainingType, declaration)) return;
            var call = context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf()).OfType<IInvocationOperation>().FirstOrDefault(IsWholeBody);
            if (call is null || !declarationDoors.Contains(call.TargetMethod.OriginalDefinition)) return;
            steps.TryGetValue(call.TargetMethod.OriginalDefinition, out var effect);
            var parameters = method.Parameters.Where(parameter =>
            {
                var uses = call.DescendantsAndSelf().OfType<IParameterReferenceOperation>()
                    .Where(use => SymbolEqualityComparer.Default.Equals(use.Parameter, parameter)).ToArray();
                return uses.Length != 0 && uses.All(use => PassedAsAnswer(use, answerParameters));
            }).Select(parameter => parameter.Ordinal).ToImmutableHashSet();
            summaries.TryAdd(Original(method), new Declaration(effect, parameters));
        });
        start.RegisterOperationAction(context =>
        {
            var invocation = (IInvocationOperation)context.Operation;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not { } parameter || Unwrap(argument.Value) is not IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda }) continue;
                var known = answerParameters.Contains(parameter.OriginalDefinition);
                if (!known && !IsDeclaration(invocation.TargetMethod.ContainingType, declaration)) continue;
                var body = new Body(context.CancellationToken);
                body.Visit(lambda.Body);
                var deferred = ImmutableArray.CreateBuilder<Call>();
                foreach (var call in body.Calls)
                {
                    if (known && steps.TryGetValue(call.Method, out var effect)) context.ReportDiagnostic(Diagnostic.Create(effect, call.Location));
                    else deferred.Add(call);
                }

                if (deferred.Count != 0) pending.Add(new AnswerUse(parameter, known, deferred.ToImmutable()));
            }
        }, OperationKind.Invocation);
        start.RegisterCompilationEndAction(context =>
        {
            foreach (var answer in pending)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                if (!answer.Known && (!summaries.TryGetValue(Original((IMethodSymbol)answer.Parameter.ContainingSymbol), out var owner) || !owner.Answers.Contains(answer.Parameter.Ordinal))) continue;
                foreach (var call in answer.Calls)
                {
                    if (!steps.TryGetValue(call.Method, out var effect) && summaries.TryGetValue(call.Method, out var called)) effect = called.Effect;
                    if (effect is not null) context.ReportDiagnostic(Diagnostic.Create(effect == Waits ? WaitsForADeclaration : WritesADeferredStep, call.Location));
                }
            }
        });
    }

    private static bool IsWholeBody(IInvocationOperation call)
    {
        IOperation held = call;
        while (held.Parent is IConversionOperation converted) held = converted;
        return held.Parent is IReturnOperation or IExpressionStatementOperation
            && held.Parent.Parent is IBlockOperation { Operations.Length: 1, Parent: null or IMethodBodyOperation };
    }

    private static bool PassedAsAnswer(IOperation value, ImmutableHashSet<IParameterSymbol> parameters)
    {
        while (value.Parent is IConversionOperation or IParenthesizedOperation) value = value.Parent;
        return value.Parent is IArgumentOperation { Parameter: { } parameter } && parameters.Contains(parameter.OriginalDefinition);
    }

    private static IMethodSymbol Original(IMethodSymbol method)
        => (method.PartialDefinitionPart ?? method.ReducedFrom ?? method).OriginalDefinition;

    private static bool IsDeclaration(INamedTypeSymbol type, INamedTypeSymbol declaration)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, declaration)) return true;
        }

        return false;
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation or IParenthesizedOperation)
            operation = operation is IConversionOperation converted ? converted.Operand : ((IParenthesizedOperation)operation).Operand;
        return operation;
    }

    private sealed class Declaration
    {
        internal Declaration(DiagnosticDescriptor? effect, ImmutableHashSet<int> answers)
        {
            Effect = effect;
            Answers = answers;
        }

        internal DiagnosticDescriptor? Effect { get; }

        internal ImmutableHashSet<int> Answers { get; }
    }

    private sealed class AnswerUse
    {
        internal AnswerUse(IParameterSymbol parameter, bool known, ImmutableArray<Call> calls)
        {
            Parameter = parameter;
            Known = known;
            Calls = calls;
        }

        internal IParameterSymbol Parameter { get; }

        internal bool Known { get; }

        internal ImmutableArray<Call> Calls { get; }
    }

    private sealed class Call
    {
        internal Call(IMethodSymbol method, Location location)
        {
            Method = method;
            Location = location;
        }

        internal IMethodSymbol Method { get; }

        internal Location Location { get; }
    }

    private sealed class Body : OperationWalker
    {
        private readonly CancellationToken _cancellationToken;

        internal Body(CancellationToken cancellationToken) => _cancellationToken = cancellationToken;

        internal List<Call> Calls { get; } = [];

        public override void VisitInvocation(IInvocationOperation operation)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            var location = operation.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax member } ? member.Name.GetLocation()
                : operation.Syntax is InvocationExpressionSyntax invocation ? invocation.Expression.GetLocation() : operation.Syntax.GetLocation();
            Calls.Add(new Call(Original(operation.TargetMethod), location));
            base.VisitInvocation(operation);
        }

        public override void VisitAnonymousFunction(IAnonymousFunctionOperation operation)
        {
        }

        public override void VisitLocalFunction(ILocalFunctionOperation operation)
        {
        }

        public override void VisitInvalid(IInvalidOperation operation)
        {
        }
    }
}
