using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// Lowers the plan model's reactions to statements: sets and calls, dispatch, branches, requests
    /// and parallel groups. Plan order is evaluation order, and a block awaits only where a request or a
    /// user decision does (grammar.md 3.3).
    /// </summary>
    internal sealed class ReactionLowering
    {
        private readonly LoweringContext _context;
        private readonly ValueLowering _values;

        internal ReactionLowering(LoweringContext context, ValueLowering values)
        {
            _context = context;
            _values = values;
        }

        internal IReadOnlyList<Statement> Block(ReactionGraph reaction, LoweringScope scope)
        {
            var block = new List<Statement>();
            Append(reaction, scope, block);
            return block;
        }

        internal IReadOnlyList<Statement> Block(IEnumerable<ReactionGraph> reactions, LoweringScope scope)
        {
            var block = new List<Statement>();
            foreach (var reaction in reactions) Append(reaction, scope, block);
            return block;
        }

        private void Append(ReactionGraph reaction, LoweringScope scope, List<Statement> block)
        {
            switch (reaction)
            {
                case SequenceReaction sequence:
                    foreach (var step in sequence.Steps) Append(step, scope, block);
                    return;
                case SetReaction set:
                    block.AddRange(_values.WithAwaitedReads(() => new[] { Set(set, scope) }));
                    return;
                case CallReaction call:
                    block.AddRange(_values.WithAwaitedReads(() => new[] { Call(call, scope) }));
                    return;
                case DispatchReaction dispatch:
                    block.AddRange(_values.WithAwaitedReads(() => new[] { G.Do(Dispatch(dispatch, scope)) }));
                    return;
                case InjectReaction inject:
                    block.AddRange(_values.WithAwaitedReads(() => new[]
                    {
                        G.Do(G.Call(G.Obj(_context.Objects.Slot(inject.Slot)), "load", _values.Value(inject.Value, scope))),
                    }));
                    return;
                case ShowValidationErrorsReaction show:
                    block.Add(G.Do(ShowValidationErrors(show, scope)));
                    return;
                case BranchReaction branch:
                    block.AddRange(Cases(branch.Cases, 0, scope));
                    return;
                case RequestReaction request:
                    block.AddRange(new RequestLowering(_context, _values, this).Request(request.Request, scope));
                    return;
                case ParallelReaction parallel:
                    block.Add(G.All(parallel.Steps.Select(step => Block(step, scope)).ToList()));
                    if (parallel.Completion is SettledParallelCompletion settled)
                        Append(settled.Reaction, scope, block);
                    return;
                default:
                    throw new InvalidOperationException("No lowering for reaction " + reaction.GetType().Name + ".");
            }
        }

        // ── members ──────────────────────────────────────────────────────────────

        private Statement Set(SetReaction set, LoweringScope scope)
        {
            var value = _values.Value(set.Value, scope);
            switch (set.On)
            {
                case ComponentSource component:
                    var property = _context.Property(_context.ContractOf(component.Component), set.Property, component.Component);
                    return G.Set(G.Obj(_context.Objects.Element(component.Component)), G.PathOf(property.Path), property.Shape, value);
                case PayloadSource payload:
                    var (root, path) = _values.PayloadRoot(payload, scope);
                    return G.Set(root, path.Concat(new[] { set.Property }).ToList(), Shape.None, value);
                default:
                    throw new InvalidOperationException("A set cannot target " + set.On.GetType().Name + ".");
            }
        }

        // A method declared with CallAsync is waited for before the next step runs.
        private Statement Call(CallReaction call, LoweringScope scope)
        {
            var args = _values.Arguments(call.Args, scope);
            switch (call.On)
            {
                case ComponentSource component:
                    var method = _context.Method(_context.ContractOf(component.Component), call.Method, component.Component);
                    var invocation = G.Call(G.Obj(_context.Objects.Element(component.Component)), G.PathOf(method.Path), args, ValueLowering.ArgumentShapes(method), Shape.None);
                    return G.Do(invocation, awaits: method.Signature.Awaits);
                case PluginSource plugin:
                    var declared = _context.Method(_context.PluginContractOf(plugin.Name), call.Method, "plugin " + plugin.Name);
                    return G.Do(G.Call(G.Obj(_context.Objects.Plugin(plugin.Name)), G.PathOf(declared.Path), args, ValueLowering.ArgumentShapes(declared), Shape.None));
                case PayloadSource payload:
                    var (root, path) = _values.PayloadRoot(payload, scope);
                    return G.Do(G.Call(root, path.Concat(new[] { call.Method }).ToList(), args, G.NoShapes, Shape.None));
                default:
                    throw new InvalidOperationException("A call cannot target " + call.On.GetType().Name + ".");
            }
        }

        private Expr Dispatch(DispatchReaction dispatch, LoweringScope scope)
        {
            var detail = dispatch.Payload is PresentDispatchPayload present
                ? _values.Value(present.Data, scope)
                : G.Record();
            return G.Call(G.Obj(_context.Objects.Document()), "raise", G.Lit(dispatch.Event), detail);
        }

        // In a response route the response is the problem record the server sent; elsewhere the form
        // checks itself (grammar.md F8).
        private Expr ShowValidationErrors(ShowValidationErrorsReaction show, LoweringScope scope)
        {
            var form = G.Obj(_context.Objects.Form(show.Container));
            return scope.Response == null
                ? G.Call(form, "validate")
                : G.Call(form, "showErrors", G.Get(G.Ref(scope.Response), new[] { "body" }, Shape.None));
        }

        // ── branches ─────────────────────────────────────────────────────────────

        // The first case whose guard holds runs. Synchronous guards share one `if`; a guard that asks the
        // user is lowered as nested `if`s in short-circuit order, so a question is asked only when reached.
        private IReadOnlyList<Statement> Cases(IReadOnlyList<BranchCase> cases, int start, LoweringScope scope)
        {
            if (start >= cases.Count) return G.NoStatements;

            var arms = new List<Arm>();
            var index = start;
            while (index < cases.Count
                   && cases[index].Guard.TryGetCondition(out var condition)
                   && _values.IsSynchronous(condition))
            {
                arms.Add(new Arm(_values.Condition(condition, scope), Block(cases[index].Reaction, scope)));
                index++;
            }

            var rest = Rest(cases, index, scope);
            return arms.Count == 0 ? rest : new[] { G.If(arms, rest) };
        }

        // What runs when no synchronous arm before `index` matched: the default case, or a guard that
        // asks the user, then the cases after it.
        private IReadOnlyList<Statement> Rest(IReadOnlyList<BranchCase> cases, int index, LoweringScope scope)
        {
            if (index >= cases.Count) return G.NoStatements;
            var branchCase = cases[index];
            if (!branchCase.Guard.TryGetCondition(out var condition)) return Block(branchCase.Reaction, scope);
            return Guarded(condition, Block(branchCase.Reaction, scope), Cases(cases, index + 1, scope), scope);
        }

        /// <summary>Runs <paramref name="then"/> when the condition holds, else <paramref name="otherwise"/>.</summary>
        internal IReadOnlyList<Statement> Guarded(
            ConditionGraph condition,
            IReadOnlyList<Statement> then,
            IReadOnlyList<Statement> otherwise,
            LoweringScope scope)
        {
            if (_values.IsSynchronous(condition))
                return new[] { G.If(_values.Condition(condition, scope), then, otherwise) };

            switch (condition)
            {
                case ConfirmCondition confirm:
                    var accepted = _context.Fresh("ok");
                    return new[]
                    {
                        G.Let(accepted, G.CallTyped(G.Obj(_context.Objects.Confirm()), "ask", Shape.Boolean, G.Lit(confirm.Message)), awaits: true),
                        G.If(G.Ref(accepted), then, otherwise),
                    };
                case AllCondition all:
                    return AllOf(all.Terms, 0, then, otherwise, scope);
                case AnyCondition any:
                    return AnyOf(any.Terms, 0, then, otherwise, scope);
                case NotCondition not:
                    return Guarded(not.Term, otherwise, then, scope);
                case CompareCondition _:
                    // A comparison that reads a promise: its awaited reads settle, then the `if` runs.
                    return _values.WithAwaitedReads(() => new[] { G.If(_values.Condition(condition, scope), then, otherwise) });
                default:
                    throw new InvalidOperationException("No lowering for condition " + condition.GetType().Name + ".");
            }
        }

        private IReadOnlyList<Statement> AllOf(
            IReadOnlyList<ConditionGraph> terms, int start, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, LoweringScope scope)
        {
            if (start >= terms.Count) return then;
            var synchronous = terms.Skip(start).TakeWhile(_values.IsSynchronous).ToList();
            if (synchronous.Count > 0)
            {
                var guard = G.And(synchronous.Select(term => _values.Condition(term, scope)).ToList());
                return new[] { G.If(guard, AllOf(terms, start + synchronous.Count, then, otherwise, scope), otherwise) };
            }

            return Guarded(terms[start], AllOf(terms, start + 1, then, otherwise, scope), otherwise, scope);
        }

        private IReadOnlyList<Statement> AnyOf(
            IReadOnlyList<ConditionGraph> terms, int start, IReadOnlyList<Statement> then, IReadOnlyList<Statement> otherwise, LoweringScope scope)
        {
            if (start >= terms.Count) return otherwise;
            var synchronous = terms.Skip(start).TakeWhile(_values.IsSynchronous).ToList();
            if (synchronous.Count > 0)
            {
                var guard = G.Or(synchronous.Select(term => _values.Condition(term, scope)).ToList());
                return new[] { G.If(guard, then, AnyOf(terms, start + synchronous.Count, then, otherwise, scope)) };
            }

            return Guarded(terms[start], then, AnyOf(terms, start + 1, then, otherwise, scope), scope);
        }
    }
}
