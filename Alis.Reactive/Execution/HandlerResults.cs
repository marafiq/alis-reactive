using System;
using System.Collections.Generic;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Execution;

internal sealed class HandlerResults
{
    private readonly Dictionary<string, ResultBinding> _results = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Site> _sites = new(StringComparer.Ordinal);
    private readonly Dictionary<Statement, Site> _calls = new();
    private readonly List<Site> _ordered = [];

    internal bool Closed { get; set; }

    internal ResultBinding Add(string name)
    {
        var result = new ResultBinding(name, () => Closed);
        _results.Add(name, result);
        return result;
    }

    internal bool IsResult(LetStatement statement) => _results.ContainsKey(statement.Name);

    internal void MovedToElseIf(LetStatement statement) => _results[statement.Name].FromElseIf = true;

    internal RefExpression ReferenceOf(LetStatement statement) => _results[statement.Name].Reference;

    internal IReadOnlyList<Statement> Complete(IReadOnlyList<Statement> statements)
    {
        var written = Walk(statements, []);
        for (var index = _ordered.Count - 1; index >= 0; index--)
        {
            var site = _ordered[index];
            if (site.Reads != 1 || site.Use is not { Read.InFunction: false } use) continue;
            var readByTheNextStep = ReferenceEquals(site.Run.Next, use.Statement);
            var readByALaterResult = _calls.TryGetValue(use.Statement, out var later) && ReferenceEquals(site.Run, later.Run);
            var deferred = use.Read.Later && readByTheNextStep || readByALaterResult && later!.Deferred;
            if (!deferred) continue;
            if (site.Let.Await)
                throw new InvalidOperationException("An async method is called in a term that runs only when it is reached (the right term of And, Or or IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached.");
            site.Deferred = true;
            site.Block[site.Index] = new LetStatement(site.Let.Name, false, LetBinds.Read, site.Let.Expr);
        }

        return written;
    }

    private List<Statement> Walk(IReadOnlyList<Statement> statements, HashSet<RefExpression> enclosing)
    {
        var visible = new HashSet<RefExpression>(enclosing);
        var written = new List<Statement>();
        Run? run = null;
        foreach (var statement in statements)
        {
            if (statement is LetStatement let && _results.TryGetValue(let.Name, out var result))
            {
                Read(statement, let.Expr, visible);
                run ??= new Run();
                var site = new Site(let, written, written.Count, run);
                _sites.Add(let.Name, site);
                _calls.Add(statement, site);
                _ordered.Add(site);
                written.Add(statement);
                visible.Add(result.Reference);
                continue;
            }

            if (run is not null) run.Next = statement;
            run = null;
            switch (statement)
            {
                case SetStatement set:
                    Read(statement, set.Target, visible);
                    Read(statement, set.Value, visible);
                    written.Add(statement);
                    break;
                case DoStatement step:
                    Read(statement, step.Expr, visible);
                    written.Add(statement);
                    break;
                case LetStatement binding:
                    Read(statement, binding.Expr, visible);
                    written.Add(statement);
                    break;
                case IfStatement branch:
                    Read(statement, branch.When, visible);
                    written.Add(new IfStatement(branch.When, Walk(branch.Then, visible), Walk(branch.Else, visible)));
                    break;
                case ListenStatement listen:
                    Read(statement, listen.Object, visible);
                    written.Add(new ListenStatement(listen.Event, listen.Listen, listen.Release, listen.Payload, listen.Members,
                        listen.Policy, listen.PayloadName, listen.ArgumentsName, listen.Object, Walk(listen.Do, visible)));
                    break;
                case StopStatement:
                    written.Add(statement);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return written;
    }

    private void Read(Statement statement, Expression expression, HashSet<RefExpression> visible)
    {
        foreach (var read in ExpressionReads.In(expression))
        {
            var reference = read.Reference;
            if (!_results.TryGetValue(reference.Name, out var result))
            {
                if (reference.Name.StartsWith(ResultBinding.NameStem, StringComparison.Ordinal)) throw ResultBinding.OutsideBlock();
                continue;
            }

            if (!ReferenceEquals(reference, result.Reference)) throw ResultBinding.OutsideBlock();
            if (!visible.Contains(result.Reference))
                throw result.FromElseIf && _sites.ContainsKey(reference.Name) ? ResultBinding.AfterWhen() : ResultBinding.OutsideBlock();
            var site = _sites[reference.Name];
            site.Reads++;
            site.Use ??= new Use(statement, read);
        }
    }

    private sealed class Run
    {
        internal Statement? Next { get; set; }
    }

    private sealed class Site(LetStatement let, List<Statement> block, int index, Run run)
    {
        internal LetStatement Let { get; } = let;
        internal List<Statement> Block { get; } = block;
        internal int Index { get; } = index;
        internal Run Run { get; } = run;
        internal int Reads { get; set; }
        internal Use? Use { get; set; }
        internal bool Deferred { get; set; }
    }

    private sealed record Use(Statement Statement, ExpressionReads.Read Read);
}
