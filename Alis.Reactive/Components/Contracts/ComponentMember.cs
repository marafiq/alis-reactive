using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A readable or writable JavaScript property of a component, typed in C#.
    /// Declare one with <c>Property&lt;T&gt;("jsName")</c> on a <see cref="Component"/>; use it with
    /// <c>Set</c> and <c>Get</c> on a component reference.
    /// </summary>
    /// <typeparam name="TValue">The C# type of the property value.</typeparam>
    public sealed class ComponentProperty<TValue>
    {
        private readonly MemberName _member;
        private readonly Path _path;
        private readonly Shape _shape;

        private ComponentProperty(string member, string pathExpression, Shape shape)
        {
            _member = MemberName.Of(member);
            if (string.IsNullOrWhiteSpace(pathExpression))
            {
                throw new ArgumentException("Property path required.", nameof(pathExpression));
            }

            _path = Path.Parse(pathExpression);
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal string Member => _member.Value;

        internal Path Path => _path;

        internal Shape Shape => _shape;

        internal ObjectPropertyContract ContractFor(MemberAccess access)
        {
            if (access == null) throw new ArgumentNullException(nameof(access));
            return ObjectPropertyContract.Create(_member, _path, _shape, access);
        }

        /// <summary>Declares a property whose plan member and JavaScript path are the same.</summary>
        internal static ComponentProperty<TValue> Named(string member) =>
            new ComponentProperty<TValue>(member, member, Shape.FromClrType(typeof(TValue)));

        /// <summary>Declares a property whose plan member maps to a different JavaScript path.</summary>
        internal static ComponentProperty<TValue> Mapped(string member, string pathExpression) =>
            new ComponentProperty<TValue>(member, pathExpression, Shape.FromClrType(typeof(TValue)));
    }

    /// <summary>
    /// A JavaScript method of a component. Declare one with <c>Method("jsName")</c> on a
    /// <see cref="Component"/>; use it with <c>Call</c> on a component reference.
    /// </summary>
    public sealed class ComponentMethod
    {
        private readonly string _name;
        private readonly string _pathExpression;
        private readonly Path _path;
        private readonly IReadOnlyList<Shape> _argumentShapes;
        private readonly IReadOnlyList<ValueExpression> _passed;
        private readonly bool _awaits;

        private ComponentMethod(
            string name,
            string pathExpression,
            IReadOnlyList<Shape> argumentShapes,
            IReadOnlyList<ValueExpression> passed,
            bool awaits = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Method name required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(pathExpression))
            {
                throw new ArgumentException("Method path required.", nameof(pathExpression));
            }

            _name = name;
            _pathExpression = pathExpression;
            _path = Path.Parse(pathExpression);
            _argumentShapes = argumentShapes;
            _passed = passed;
            _awaits = awaits;
        }

        /// <summary>
        /// The plan key: the member's name, followed by its argument shapes when it takes any. Each
        /// signature of one JavaScript method is its own member, so a generic member called with two
        /// type arguments, or two members typing one method, never conflict.
        /// </summary>
        internal string Member => _argumentShapes.Count == 0
            ? _name
            : _name + "(" + string.Join(",", _argumentShapes.Select(shape => shape.DescribeContract())) + ")";

        /// <summary>The member's own name: what a read of it is called in a gathered body.</summary>
        internal string Name => _name;

        /// <summary>The JavaScript name the method has on its object, which an event payload is called by.</summary>
        internal string JavaScriptName => _pathExpression;

        /// <summary>The arguments a call passes: the ones the call supplies, then the ones the declaration fixes.</summary>
        internal IReadOnlyList<ValueExpression> ArgumentsWith(IReadOnlyList<ValueExpression> supplied) =>
            _passed.Count == 0 ? supplied : supplied.Concat(_passed).ToList();

        internal ObjectMethodContract ContractReturning(Shape returns)
        {
            if (returns == null) throw new ArgumentNullException(nameof(returns));
            return ObjectMethodContract.Create(
                MemberName.Of(Member),
                _path,
                MethodSignature.WithArguments(MethodArgumentContract.Exact(_argumentShapes), returns, _awaits));
        }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run, and a read of it is the value it settles with.
        /// </summary>
        /// <example><c>public ComponentMethod SaveDraft =&gt; Method("saveDraft").Awaited();</c></example>
        public ComponentMethod Awaited() =>
            new ComponentMethod(_name, _pathExpression, _argumentShapes, _passed, awaits: true);

        /// <summary>Declares a method whose plan member and JavaScript path are the same.</summary>
        internal static ComponentMethod Named(string member) =>
            Mapped(member, member);

        /// <summary>Declares a method whose plan member maps to a different JavaScript path.</summary>
        internal static ComponentMethod Mapped(string member, string pathExpression) =>
            new ComponentMethod(member, pathExpression, Array.Empty<Shape>(), Array.Empty<ValueExpression>());

        internal ComponentMethod WithArgs<T1>() =>
            WithArgs(Shape.FromClrType(typeof(T1)));

        internal ComponentMethod WithArgs<T1, T2>() =>
            WithArgs(Shape.FromClrType(typeof(T1)), Shape.FromClrType(typeof(T2)));

        internal ComponentMethod WithArgs<T1, T2, T3>() =>
            WithArgs(Shape.FromClrType(typeof(T1)), Shape.FromClrType(typeof(T2)), Shape.FromClrType(typeof(T3)));

        /// <summary>Fixes literal arguments the JavaScript method always takes after the supplied ones.</summary>
        internal ComponentMethod Passing(IReadOnlyList<object> trailing)
        {
            var shapes = new List<Shape>(_argumentShapes);
            var passed = new List<ValueExpression>(_passed);
            foreach (var value in trailing)
            {
                var shape = Shape.FromValue(value);
                shapes.Add(shape);
                passed.Add(ValueExpression.LiteralFor(value, shape));
            }

            return new ComponentMethod(_name, _pathExpression, shapes, passed, _awaits);
        }

        private ComponentMethod WithArgs(params Shape[] args) =>
            new ComponentMethod(_name, _pathExpression, args, Array.Empty<ValueExpression>(), _awaits);
    }

    /// <summary>
    /// A JavaScript method that takes one typed argument. Declare one with
    /// <c>Method&lt;TArg&gt;("jsName")</c> on a <see cref="JsObject"/>; use it with <c>Call</c>.
    /// </summary>
    /// <typeparam name="TArg">The C# type of the argument.</typeparam>
    public sealed class ComponentMethod<TArg>
    {
        internal ComponentMethod(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run.
        /// </summary>
        public ComponentMethod<TArg> Awaited() => new ComponentMethod<TArg>(Method.Awaited());

        /// <summary>
        /// Fixes arguments the JavaScript method always takes after the typed one, such as the kind of
        /// key it is given, so a call passes only the typed argument.
        /// </summary>
        /// <example>
        /// <c>public ComponentMethod&lt;Field&gt; ShowColumn =&gt; Method&lt;Field&gt;("showColumns").Passing("field");</c>
        /// </example>
        /// <param name="trailing">Literal arguments, in the order the method takes them.</param>
        public ComponentMethod<TArg> Passing(params object[] trailing)
        {
            if (trailing == null) throw new ArgumentNullException(nameof(trailing));
            return new ComponentMethod<TArg>(Method.Passing(trailing));
        }
    }

    /// <summary>
    /// A JavaScript method that takes two typed arguments. Declare one with
    /// <c>Method&lt;T1, T2&gt;("jsName")</c> on a <see cref="JsObject"/>; use it with <c>Call</c>.
    /// </summary>
    /// <typeparam name="T1">The C# type of the first argument.</typeparam>
    /// <typeparam name="T2">The C# type of the second argument.</typeparam>
    public sealed class ComponentMethod<T1, T2>
    {
        internal ComponentMethod(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run.
        /// </summary>
        public ComponentMethod<T1, T2> Awaited() => new ComponentMethod<T1, T2>(Method.Awaited());
    }

    /// <summary>
    /// A JavaScript method that takes three typed arguments. Declare one with
    /// <c>Method&lt;T1, T2, T3&gt;("jsName")</c> on a <see cref="JsObject"/>; use it with <c>Call</c>.
    /// </summary>
    /// <typeparam name="T1">The C# type of the first argument.</typeparam>
    /// <typeparam name="T2">The C# type of the second argument.</typeparam>
    /// <typeparam name="T3">The C# type of the third argument.</typeparam>
    public sealed class ComponentMethod<T1, T2, T3>
    {
        internal ComponentMethod(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run.
        /// </summary>
        public ComponentMethod<T1, T2, T3> Awaited() => new ComponentMethod<T1, T2, T3>(Method.Awaited());
    }

    /// <summary>
    /// A JavaScript method that returns a value. Declare one with <c>Function&lt;TResult&gt;("jsName")</c>
    /// on a <see cref="JsObject"/>; read it with <c>Get(c =&gt; c.Name)</c> wherever the DSL takes a source.
    /// </summary>
    /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
    public sealed class ComponentFunction<TResult>
    {
        internal ComponentFunction(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run, and a read of it is the value it settles with.
        /// </summary>
        public ComponentFunction<TResult> Awaited() => new ComponentFunction<TResult>(Method.Awaited());
    }

    /// <summary>
    /// A JavaScript method that takes one typed argument and returns a value. Declare one with
    /// <c>Function&lt;TArg, TResult&gt;("jsName")</c>; read it with <c>Get(c =&gt; c.Name, argument)</c>.
    /// </summary>
    /// <typeparam name="TArg">The C# type of the argument.</typeparam>
    /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
    public sealed class ComponentFunction<TArg, TResult>
    {
        internal ComponentFunction(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run, and a read of it is the value it settles with.
        /// </summary>
        public ComponentFunction<TArg, TResult> Awaited() => new ComponentFunction<TArg, TResult>(Method.Awaited());
    }

    /// <summary>
    /// A JavaScript method that takes two typed arguments and returns a value. Declare one with
    /// <c>Function&lt;T1, T2, TResult&gt;("jsName")</c>; read it with <c>Get(c =&gt; c.Name, first, second)</c>.
    /// </summary>
    /// <typeparam name="T1">The C# type of the first argument.</typeparam>
    /// <typeparam name="T2">The C# type of the second argument.</typeparam>
    /// <typeparam name="TResult">The C# type of the returned value.</typeparam>
    public sealed class ComponentFunction<T1, T2, TResult>
    {
        internal ComponentFunction(ComponentMethod method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        internal ComponentMethod Method { get; }

        /// <summary>
        /// Declares that the JavaScript method returns a promise: a call waits for it to settle before
        /// the steps after it run, and a read of it is the value it settles with.
        /// </summary>
        public ComponentFunction<T1, T2, TResult> Awaited() => new ComponentFunction<T1, T2, TResult>(Method.Awaited());
    }

    /// <summary>
    /// DOM element contract used by <c>p.Element(...)</c>.
    /// Native DOM onboarding stays explicit without changing the public DSL.
    /// </summary>
    internal static class BrowserElementMembers
    {
        internal static ComponentMethod AddClass { get; } =
            ComponentMethod.Mapped("classAdd", "classList.add").WithArgs<string>();

        internal static ComponentMethod RemoveClass { get; } =
            ComponentMethod.Mapped("classRemove", "classList.remove").WithArgs<string>();

        internal static ComponentMethod ToggleClass { get; } =
            ComponentMethod.Mapped("classToggle", "classList.toggle").WithArgs<string>();

        internal static ComponentProperty<string> Text { get; } =
            ComponentProperty<string>.Mapped("text", "textContent");

        internal static ComponentProperty<string> Html { get; } =
            ComponentProperty<string>.Mapped("html", "innerHTML");

        internal static ComponentProperty<bool> Hidden { get; } =
            ComponentProperty<bool>.Named("hidden");
    }
}
