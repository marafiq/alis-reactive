using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The steps a callback runs, in the order they were written. A step is placed where it is written,
    /// and a step still being configured (a request, a condition's arms) stays in its place.
    /// </summary>
    internal sealed class Block
    {
        private readonly List<Step> _steps = new List<Step>();

        internal void Add(Step step) => _steps.Add(step ?? throw new ArgumentNullException(nameof(step)));

        internal IReadOnlyList<Statement> Write(Scope scope)
        {
            var block = new List<Statement>();
            var inner = scope.ForBlock();
            foreach (var step in _steps) step.WriteTo(block, inner);
            inner.WriteDeferred(block);
            return block;
        }
    }

    /// <summary>
    /// The line a method that returns a value is written on. A method that returns its value runs here,
    /// in the order the C# wrote it. A method that returns a promise is awaited by the statement that
    /// first reads its result, wherever in the block or its branches that is, so a condition that waits
    /// for it asks only when it is reached; it runs here only when no statement reads it.
    /// </summary>
    internal sealed class MethodResultStep : Step
    {
        private readonly ValueExpression _result;

        internal MethodResultStep(ValueExpression result)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
        {
            if (_result.ReadsAwaited)
                scope.DeferOwnLine(_result, block.Count, () => OwnLine(scope));
            else
                block.AddRange(OwnLine(scope));
        }

        private IReadOnlyList<Statement> OwnLine(Scope scope) =>
            scope.WithHoisted(() =>
            {
                _result.Write(scope);
                return G.NoStatements;
            });
    }

    internal abstract class Step
    {
        private protected Step() { }

        internal abstract void WriteTo(List<Statement> block, Scope scope);
    }

    /// <summary>Writes a property: of an element or a component, of a plugin, or of data the event handed its callback.</summary>
    internal sealed class SetStep : Step
    {
        private readonly Receiver _target;
        private readonly IReadOnlyList<string> _path;
        private readonly Shape _shape;
        private readonly ValueExpression _value;

        internal SetStep(Receiver target, IReadOnlyList<string> path, Shape shape, ValueExpression value)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override void WriteTo(List<Statement> block, Scope scope) =>
            block.AddRange(scope.WithHoisted(() =>
            {
                var value = _value.Write(scope);
                var (root, prefix) = _target.Write(scope);
                return new[] { G.Set(root, prefix.Concat(_path).ToList(), _shape, value) };
            }));
    }

    /// <summary>Calls a method; one declared to return a promise holds the steps after it until it settles.</summary>
    internal sealed class CallStep : Step
    {
        private readonly Receiver _target;
        private readonly IReadOnlyList<string> _path;
        private readonly IReadOnlyList<ValueExpression> _args;
        private readonly IReadOnlyList<Shape> _argShapes;
        private readonly bool _awaits;

        internal CallStep(Receiver target, IReadOnlyList<string> path, IReadOnlyList<ValueExpression> args, IReadOnlyList<Shape> argShapes, bool awaits)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _argShapes = argShapes ?? throw new ArgumentNullException(nameof(argShapes));
            _awaits = awaits;
        }

        internal override void WriteTo(List<Statement> block, Scope scope) =>
            block.AddRange(scope.WithHoisted(() =>
            {
                var args = _args.Select(arg => arg.Write(scope)).ToList();
                var (root, prefix) = _target.Write(scope);
                return new[] { G.Do(G.Call(root, prefix.Concat(_path).ToList(), args, _argShapes, Shape.None), _awaits) };
            }));
    }

    /// <summary>Raises a custom event on the document, with its payload as the event's detail.</summary>
    internal sealed class DispatchStep : Step
    {
        private readonly string _event;
        private readonly ValueExpression _payload;

        internal DispatchStep(string eventName, ValueExpression payload)
        {
            if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("Event name required.", nameof(eventName));
            _event = eventName;
            _payload = payload ?? throw new ArgumentNullException(nameof(payload));
        }

        internal override void WriteTo(List<Statement> block, Scope scope) =>
            block.AddRange(scope.WithHoisted(() =>
                new[] { G.Do(G.Call(G.Obj(scope.Objects.Document()), "raise", G.Lit(_event), _payload.Write(scope))) }));
    }

    /// <summary>Loads the response into a slot: <c>Into(elementId)</c>.</summary>
    internal sealed class InjectStep : Step
    {
        private readonly string _slot;
        private readonly ValueExpression _content;

        internal InjectStep(string slot, ValueExpression content)
        {
            _slot = slot ?? throw new ArgumentNullException(nameof(slot));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        internal override void WriteTo(List<Statement> block, Scope scope) =>
            block.AddRange(scope.WithHoisted(() => new[]
            {
                G.Do(G.Call(G.Obj(scope.Objects.Slot(_slot)), "load", _content.Write(scope))),
            }));
    }

    /// <summary>In an error route, the form shows the problems the server sent (grammar.md F8).</summary>
    internal sealed class ShowErrorsStep : Step
    {
        private readonly string _form;

        internal ShowErrorsStep(string form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        // The errors are the response's body: the step belongs inside an error route of a request.
        internal override void WriteTo(List<Statement> block, Scope scope)
        {
            var form = G.Obj(scope.Objects.Form(_form));
            var response = scope.Response(ResponseHandle.Innermost);
            block.Add(G.Do(G.Call(form, "showErrors", G.Get(G.Ref(response), new[] { "body" }, Shape.None))));
        }
    }

    /// <summary>
    /// When, ElseIf and Else: the first arm whose condition holds runs. Synchronous conditions share one
    /// <c>if</c>; a condition that asks the user or waits for a promise guards the arms after it, so a
    /// question is asked only when reached.
    /// </summary>
    internal sealed class BranchStep : Step
    {
        private readonly List<(ConditionGraph Condition, Block Then)> _arms = new List<(ConditionGraph, Block)>();
        private Block _else = new Block();
        private bool _hasElse;

        internal BranchStep(ConditionGraph condition, Block then)
        {
            AddArm(condition, then);
        }

        internal void AddArm(ConditionGraph condition, Block then)
        {
            if (_hasElse) throw new InvalidOperationException("Cannot add branches after Else.");
            _arms.Add((condition ?? throw new ArgumentNullException(nameof(condition)), then ?? throw new ArgumentNullException(nameof(then))));
        }

        internal bool HasElse => _hasElse;

        internal void SetElse(Block otherwise)
        {
            if (_hasElse) throw new InvalidOperationException("Else already called.");
            _else = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
            _hasElse = true;
        }

        // A branch is one statement: a method result its conditions read is bound before it.
        internal override void WriteTo(List<Statement> block, Scope scope) =>
            block.AddRange(scope.WithHoisted(() => Arms(0, scope)));

        private IReadOnlyList<Statement> Arms(int start, Scope scope)
        {
            var arms = new List<Arm>();
            var index = start;
            while (index < _arms.Count && _arms[index].Condition.IsSynchronous)
            {
                arms.Add(new Arm(_arms[index].Condition.Write(scope), _arms[index].Then.Write(scope)));
                index++;
            }

            var rest = Rest(index, scope);
            return arms.Count == 0 ? rest : new[] { G.If(arms, rest) };
        }

        // What runs when no synchronous arm before `index` matched: the arm that waits, guarding the arms
        // after it, or the Else.
        private IReadOnlyList<Statement> Rest(int index, Scope scope)
        {
            if (index >= _arms.Count) return _else.Write(scope);
            var (condition, then) = _arms[index];
            return condition.Guard(then.Write(scope), Arms(index + 1, scope), scope);
        }
    }

    /// <summary>Requests that start together; the steps after them run once every one has settled.</summary>
    internal sealed class ParallelStep : Step
    {
        private readonly List<RequestStep> _requests = new List<RequestStep>();
        private Block _settled = new Block();

        internal void Add(RequestStep request) => _requests.Add(request ?? throw new ArgumentNullException(nameof(request)));

        internal void OnAllSettled(Block settled) => _settled = settled ?? throw new ArgumentNullException(nameof(settled));

        internal override void WriteTo(List<Statement> block, Scope scope)
        {
            if (_requests.Count == 0)
                throw new InvalidOperationException("Parallel requires at least one HTTP request branch.");

            block.Add(G.All(_requests.Select(request =>
            {
                var branch = new List<Statement>();
                request.WriteTo(branch, scope);
                return (IReadOnlyList<Statement>)branch;
            }).ToList()));
            block.AddRange(_settled.Write(scope));
        }
    }
}
