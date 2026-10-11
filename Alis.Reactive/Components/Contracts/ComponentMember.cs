using System;
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

        private ComponentProperty(MemberName member, Path path, Shape shape)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal string Member => _member.Value;

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

        /// <summary>Specializes the property shape when render-time registration discovered it.</summary>
        internal ComponentProperty<TValue> WithShape(Shape shape) =>
            new ComponentProperty<TValue>(_member, _path, shape);
    }

    /// <summary>
    /// A JavaScript method of a component. Declare one with <c>Method("jsName")</c> on a
    /// <see cref="Component"/>; use it with <c>Call</c> on a component reference.
    /// </summary>
    public sealed class ComponentMethod
    {
        private readonly MemberName _member;
        private readonly string _pathExpression;
        private readonly Path _path;
        private readonly MethodArgumentContract _arguments;

        private ComponentMethod(string member, string pathExpression, MethodArgumentContract arguments)
            : this(MemberName.Of(member), pathExpression, arguments)
        {
        }

        private ComponentMethod(MemberName member, string pathExpression, MethodArgumentContract arguments)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            if (string.IsNullOrWhiteSpace(pathExpression))
            {
                throw new ArgumentException("Method path required.", nameof(pathExpression));
            }

            _pathExpression = pathExpression;
            _path = Path.Parse(pathExpression);
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        internal string Member => _member.Value;

        /// <summary>The JavaScript name the method has on its object, which an event payload is called by.</summary>
        internal string JavaScriptName => _pathExpression;

        internal ObjectMethodContract ContractReturning(Shape returns)
        {
            if (returns == null) throw new ArgumentNullException(nameof(returns));
            return ObjectMethodContract.Create(
                _member,
                _path,
                MethodSignature.WithArguments(_arguments, returns));
        }

        /// <summary>Declares a method whose plan member and JavaScript path are the same.</summary>
        internal static ComponentMethod Named(string member) =>
            new ComponentMethod(member, member, MethodArgumentContract.NoArguments);

        /// <summary>Declares a method whose plan member maps to a different JavaScript path.</summary>
        internal static ComponentMethod Mapped(string member, string pathExpression) =>
            new ComponentMethod(member, pathExpression, MethodArgumentContract.NoArguments);

        internal ComponentMethod WithArgs<T1>() =>
            WithArgs(Shape.FromClrType(typeof(T1)));

        internal ComponentMethod WithArgs<T1, T2>() =>
            WithArgs(Shape.FromClrType(typeof(T1)), Shape.FromClrType(typeof(T2)));

        internal ComponentMethod WithArgs<T1, T2, T3>() =>
            WithArgs(Shape.FromClrType(typeof(T1)), Shape.FromClrType(typeof(T2)), Shape.FromClrType(typeof(T3)));

        private ComponentMethod WithArgs(params Shape[] args) =>
            new ComponentMethod(_member, _pathExpression, MethodArgumentContract.Exact(args));
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
