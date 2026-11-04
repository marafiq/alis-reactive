using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Reactions
{
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

        private IReadOnlyList<Statement> OwnLine(Scope scope)
            => scope.WithHoisted(() =>
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

    internal sealed class SetStep : Step
    {
        private readonly Target _target;
        private readonly IReadOnlyList<string> _path;
        private readonly Shape _shape;
        private readonly ValueExpression _value;

        internal SetStep(Target target, IReadOnlyList<string> path, Shape shape, ValueExpression value)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() =>
            {
                var value = _value.Write(scope);
                var (root, prefix) = _target.Write(scope);
                return new[] { G.Set(root, prefix.Concat(_path).ToList(), _shape, value, _target.Members) };
            }));
    }

    internal sealed class CallStep : Step
    {
        private readonly Target _target;
        private readonly IReadOnlyList<string> _path;
        private readonly IReadOnlyList<ValueExpression> _args;
        private readonly IReadOnlyList<Shape> _argShapes;
        private readonly bool _awaits;

        internal CallStep(Target target, IReadOnlyList<string> path, IReadOnlyList<ValueExpression> args, IReadOnlyList<Shape> argShapes, bool awaits)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _argShapes = argShapes ?? throw new ArgumentNullException(nameof(argShapes));
            _awaits = awaits;
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() =>
            {
                var args = _args.Select(arg => arg.Write(scope)).ToList();
                var (root, prefix) = _target.Write(scope);
                return new[] { G.Do(G.Call(root, prefix.Concat(_path).ToList(), args, _argShapes, Shape.Any, PathMembers.Required), _awaits) };
            }));
    }

    internal sealed class ClassChangeStep : Step
    {
        private readonly Target _element;
        private readonly IReadOnlyList<string> _path;
        private readonly Shape _nameShape;
        private readonly ValueExpression _name;

        internal ClassChangeStep(Target element, IReadOnlyList<string> path, Shape nameShape, ValueExpression name)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _nameShape = nameShape ?? throw new ArgumentNullException(nameof(nameShape));
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() =>
            {
                var name = scope.Fresh("c");
                var (root, prefix) = _element.Write(scope);
                var change = G.Do(G.Call(root, prefix.Concat(_path).ToList(), new[] { G.Ref(name) }, new[] { _nameShape }, Shape.Any, PathMembers.Required));
                var nameIsMissing = G.Eq(G.Ref(name), G.Null);
                return new[] { G.Let(name, _name.Write(scope)), G.If(G.Not(nameIsMissing), new[] { change }, G.NoStatements) };
            }));
    }

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

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() =>
                new[] { G.Do(G.Call(G.Obj(scope.Objects.Document()), PathMembers.Required, "raise", G.Lit(_event), _payload.Write(scope))) }));
    }

    internal sealed class InjectStep : Step
    {
        private readonly string _slot;
        private readonly ValueExpression _content;

        internal InjectStep(string slot, ValueExpression content)
        {
            _slot = slot ?? throw new ArgumentNullException(nameof(slot));
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() => new[]
            {
                G.Do(G.Call(G.Obj(scope.Objects.Slot(_slot)), PathMembers.Required, "load", _content.Write(scope))),
            }));
    }

    internal sealed class UnloadStep : Step
    {
        private readonly string _slot;

        internal UnloadStep(string slot)
        {
            _slot = slot ?? throw new ArgumentNullException(nameof(slot));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.Add(G.Do(G.Call(G.Obj(scope.Objects.Slot(_slot)), PathMembers.Required, "unload")));
    }

    internal sealed class ShowErrorsStep : Step
    {
        private readonly string _form;

        internal ShowErrorsStep(string form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
        {
            var form = G.Obj(scope.Objects.Form(_form));
            var response = scope.Response(ResponseHandle.Innermost);
            block.Add(G.Do(G.Call(form, PathMembers.Required, "showErrors", G.Get(G.Ref(response), new[] { "body" }, Shape.Any, PathMembers.Optional))));
        }
    }

    internal sealed class BranchStep : Step
    {
        private readonly List<(Condition Condition, Block Then)> _arms = new List<(Condition, Block)>();
        private Block _else = new Block();
        private bool _hasElse;

        internal BranchStep(Condition condition, Block then)
        {
            AddArm(condition, then);
        }

        internal void AddArm(Condition condition, Block then)
        {
            if (_hasElse) throw new InvalidOperationException("An ElseIf cannot follow the Else of its When.");
            _arms.Add((condition ?? throw new ArgumentNullException(nameof(condition)), then ?? throw new ArgumentNullException(nameof(then))));
        }

        internal void SetElse(Block otherwise)
        {
            if (_hasElse) throw new InvalidOperationException("A When has one Else.");
            _else = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
            _hasElse = true;
        }

        internal override void WriteTo(List<Statement> block, Scope scope)
            => block.AddRange(scope.WithHoisted(() => Arms(0, scope)));

        private IReadOnlyList<Statement> Arms(int start, Scope scope)
        {
            var arms = new List<Arm>();
            var index = start;
            while (index < _arms.Count && _arms[index].Condition.IsSynchronous)
            {
                arms.Add(new Arm(_arms[index].Condition.Write(scope), _arms[index].Then.Write(scope)));
                index++;
            }

            var rest = Rest(index, arms.Count == 0 ? scope : scope.ForBlock());
            return arms.Count == 0 ? rest : new[] { G.If(arms, rest) };
        }

        private IReadOnlyList<Statement> Rest(int index, Scope scope)
        {
            if (index >= _arms.Count) return _else.Write(scope);
            var (condition, then) = _arms[index];
            return condition.Guard(then.Write(scope), Arms(index + 1, scope.ForBlock()), scope);
        }
    }
}
