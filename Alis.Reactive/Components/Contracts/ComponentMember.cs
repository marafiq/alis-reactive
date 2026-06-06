using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Describes a readable or writable JavaScript property on a reactive component.
    /// Component onboarding uses these members to declare the Reactive Plan contract
    /// that the runtime can read or update.
    /// </summary>
    internal sealed class ComponentProperty<TValue>
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
    /// Describes a JavaScript method on a reactive component.
    /// </summary>
    internal sealed class ComponentMethod
    {
        private readonly MemberName _member;
        private readonly Path _path;
        private readonly MethodArgumentContract _arguments;

        private ComponentMethod(string member, string pathExpression, MethodArgumentContract arguments)
        {
            _member = MemberName.Of(member);
            if (string.IsNullOrWhiteSpace(pathExpression))
            {
                throw new ArgumentException("Method path required.", nameof(pathExpression));
            }

            _path = Path.Parse(pathExpression);
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        private ComponentMethod(MemberName member, Path path, MethodArgumentContract arguments)
        {
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        internal string Member => _member.Value;

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
            new ComponentMethod(_member, _path, MethodArgumentContract.Exact(args));
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
