using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Alis.Reactive.Analyzers.PartialsAndSlots;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ALIS041PlanIsRenderedOnceLast : DiagnosticAnalyzer
{
    public const string Id = "ALIS041";

    private const string Category = "Alis.Reactive.Analyzers";
    private const string PlanDoors = "Alis.Reactive.RenderingAndDelivery.PlanHtml";
    private const string OpensAPlan = "ReactivePlan";
    private const string RendersAPlan = "RenderPlan";

    private static readonly DiagnosticDescriptor NeverRendered = new(
        Id,
        "A view writes the plan it opens with Html.RenderPlan",
        "'{0}', the plan of '{1}', is opened and never handed to Html.RenderPlan; write Html.RenderPlan({0}) as the view's last line",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RenderedTwice = new(
        Id,
        "A view renders its plan once",
        "The plan of '{0}' is rendered twice, so every handler it holds would run twice; render it once, as the view's last line",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TakenAfterItRendered = new(
        Id,
        "A door takes a plan before it renders",
        "A door takes the plan of '{0}' after it rendered, so the page never runs what the door writes; write every door above Html.RenderPlan, and where a @section holds one, render the plan in a section the layout renders after it",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(NeverRendered, RenderedTwice, TakenAfterItRendered);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(Start);
    }

    private static void Start(CompilationStartAnalysisContext start)
    {
        var doors = start.Compilation.GetTypeByMetadataName(PlanDoors);
        if (doors is null) return;

        var opens = doors.GetMembers(OpensAPlan).OfType<IMethodSymbol>().ToImmutableArray();
        var renders = doors.GetMembers(RendersAPlan).OfType<IMethodSymbol>().ToImmutableArray();
        if (opens.IsEmpty || renders.IsEmpty) return;

        start.RegisterOperationBlockAction(body => Read(body, opens, renders));
    }

    private static void Read(OperationBlockAnalysisContext body, ImmutableArray<IMethodSymbol> opens, ImmutableArray<IMethodSymbol> renders)
    {
        var read = new HashSet<IOperation>();
        foreach (var block in body.OperationBlocks)
        {
            var whole = Whole(block);
            if (!read.Add(whole)) continue;

            var plans = FollowedPlans(whole, opens);
            if (plans.IsEmpty) continue;

            var handed = whole.DescendantsAndSelf().OfType<IInvocationOperation>()
                .Where(call => Is(call, renders))
                .SelectMany(call => PlansTakenBy(call, plans))
                .ToImmutableHashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            foreach (var plan in plans.Where(plan => !handed.Contains(plan)).OrderBy(plan => plan.Locations[0].SourceSpan.Start))
                body.ReportDiagnostic(Diagnostic.Create(NeverRendered, plan.Locations[0], plan.Name, ModelOf(plan)));

            var graph = body.GetControlFlowGraph(block);
            var walk = new Walk(body, plans, renders);
            var noneRendered = plans.Clear();
            walk.Follow(graph, noneRendered);
            foreach (var local in graph.LocalFunctions) walk.Follow(graph.GetLocalFunctionControlFlowGraph(local, body.CancellationToken), noneRendered);
        }
    }

    private static IOperation Whole(IOperation block)
    {
        var whole = block;
        while (whole.Parent is { } around) whole = around;
        return whole;
    }

    private static ImmutableHashSet<ILocalSymbol> FollowedPlans(IOperation whole, ImmutableArray<IMethodSymbol> opens)
    {
        var opened = whole.DescendantsAndSelf().OfType<IVariableDeclaratorOperation>()
            .Where(declared => Held(declared.GetVariableInitializer()?.Value) is IInvocationOperation call && Is(call, opens))
            .Where(declared => declared.Symbol.Type is INamedTypeSymbol { TypeArguments.Length: 1 } && !declared.Symbol.Locations.IsEmpty)
            .Select(declared => declared.Symbol)
            .ToImmutableHashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        if (opened.IsEmpty) return opened;

        var escaped = whole.DescendantsAndSelf().OfType<ILocalReferenceOperation>()
            .Where(reference => opened.Contains(reference.Local) && !IsHandedToACall(reference))
            .Select(reference => reference.Local);
        return opened.Except(escaped);
    }

    private static bool IsHandedToACall(ILocalReferenceOperation reference)
    {
        IOperation held = reference;
        while (held.Parent is IConversionOperation or IParenthesizedOperation) held = held.Parent;
        return held.Parent switch
        {
            IArgumentOperation { Parameter.RefKind: RefKind.None, Parent: IInvocationOperation } => true,
            IInvocationOperation call => ReferenceEquals(call.Instance, held),
            _ => false,
        };
    }

    private static IOperation? Held(IOperation? value)
    {
        var held = value;
        while (held is IConversionOperation or IParenthesizedOperation)
            held = held is IConversionOperation converted ? converted.Operand : ((IParenthesizedOperation)held).Operand;
        return held;
    }

    private static bool Is(IInvocationOperation call, ImmutableArray<IMethodSymbol> doors)
        => doors.Contains(call.TargetMethod.OriginalDefinition, SymbolEqualityComparer.Default);

    private static IEnumerable<ILocalSymbol> PlansTakenBy(IInvocationOperation call, ImmutableHashSet<ILocalSymbol> plans)
    {
        var taken = call.Arguments.Select(argument => argument.Value);
        if (call.Instance is { } receiver) taken = taken.Prepend(receiver);
        foreach (var value in taken)
        {
            if (Held(value) is ILocalReferenceOperation reference && plans.Contains(reference.Local))
                yield return reference.Local;
        }
    }

    private static string ModelOf(ILocalSymbol plan) => ((INamedTypeSymbol)plan.Type).TypeArguments[0].ToDisplayString();

    private sealed class Walk
    {
        private readonly OperationBlockAnalysisContext _body;
        private readonly ImmutableHashSet<ILocalSymbol> _plans;
        private readonly ImmutableArray<IMethodSymbol> _renders;

        internal Walk(OperationBlockAnalysisContext body, ImmutableHashSet<ILocalSymbol> plans, ImmutableArray<IMethodSymbol> renders)
        {
            _body = body;
            _plans = plans;
            _renders = renders;
        }

        internal void Follow(ControlFlowGraph graph, ImmutableHashSet<ILocalSymbol> renderedOnEntry)
        {
            var entering = new ImmutableHashSet<ILocalSymbol>?[graph.Blocks.Length];
            entering[0] = renderedOnEntry;
            var pending = new Queue<BasicBlock>();
            pending.Enqueue(graph.Blocks[0]);
            while (pending.Count > 0)
            {
                _body.CancellationToken.ThrowIfCancellationRequested();
                var block = pending.Dequeue();
                var leaving = Leave(block, entering[block.Ordinal]!, graph: null);
                foreach (var next in After(block))
                {
                    var known = entering[next.Ordinal];
                    var merged = known is null ? leaving : known.Union(leaving);
                    if (known is not null && known.Count == merged.Count) continue;
                    entering[next.Ordinal] = merged;
                    pending.Enqueue(next);
                }
            }

            foreach (var block in graph.Blocks)
            {
                if (entering[block.Ordinal] is { } rendered) Leave(block, rendered, graph);
            }
        }

        private static IEnumerable<BasicBlock> After(BasicBlock block)
        {
            if (block.FallThroughSuccessor?.Destination is { } next) yield return next;
            if (block.ConditionalSuccessor?.Destination is { } other) yield return other;
        }

        private ImmutableHashSet<ILocalSymbol> Leave(BasicBlock block, ImmutableHashSet<ILocalSymbol> rendered, ControlFlowGraph? graph)
        {
            foreach (var operation in block.Operations) rendered = Run(operation, rendered, graph);
            return block.BranchValue is { } decided ? Run(decided, rendered, graph) : rendered;
        }

        private ImmutableHashSet<ILocalSymbol> Run(IOperation operation, ImmutableHashSet<ILocalSymbol> rendered, ControlFlowGraph? graph)
        {
            foreach (var part in operation.ChildOperations) rendered = Run(part, rendered, graph);
            switch (operation)
            {
                case IFlowAnonymousFunctionOperation lambda when graph is not null:
                    Follow(graph.GetAnonymousFunctionControlFlowGraph(lambda, _body.CancellationToken), rendered);
                    return rendered;
                case ISimpleAssignmentOperation { Target: ILocalReferenceOperation opened } when _plans.Contains(opened.Local):
                    return rendered.Remove(opened.Local);
                case IInvocationOperation call:
                    return Take(call, rendered, reports: graph is not null);
                default:
                    return rendered;
            }
        }

        private ImmutableHashSet<ILocalSymbol> Take(IInvocationOperation call, ImmutableHashSet<ILocalSymbol> rendered, bool reports)
        {
            var renders = Is(call, _renders);
            foreach (var plan in PlansTakenBy(call, _plans))
            {
                if (reports && rendered.Contains(plan))
                    _body.ReportDiagnostic(Diagnostic.Create(renders ? RenderedTwice : TakenAfterItRendered, call.Syntax.GetLocation(), ModelOf(plan)));
                if (renders) rendered = rendered.Add(plan);
            }

            return rendered;
        }
    }
}
