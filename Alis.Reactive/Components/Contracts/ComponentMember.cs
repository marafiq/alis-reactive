using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>A declared property: its plan key, its JavaScript path and the shape of its value.</summary>
    /// <typeparam name="TValue">The C# type of the property value.</typeparam>
    internal sealed class ComponentProperty<TValue>
    {
        private readonly MemberName _member;

        private ComponentProperty(string member, string pathExpression, Shape shape)
        {
            _member = MemberName.Of(member);
            if (string.IsNullOrWhiteSpace(pathExpression))
                throw new ArgumentException("Property path required.", nameof(pathExpression));

            JavaScriptPath = pathExpression;
            Path = Path.Parse(pathExpression);
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal string Member => _member.Value;

        /// <summary>The property's name on its object, a dotted path for a nested one.</summary>
        internal string JavaScriptPath { get; }

        internal Path Path { get; }

        internal Shape Shape { get; }

        internal ObjectPropertyContract ContractFor(MemberAccess access)
        {
            if (access == null) throw new ArgumentNullException(nameof(access));
            return ObjectPropertyContract.Create(_member, Path, Shape, access);
        }

        /// <summary>Declares a property whose plan member and JavaScript path are the same.</summary>
        internal static ComponentProperty<TValue> Named(string member) =>
            new ComponentProperty<TValue>(member, member, Shape.FromClrType(typeof(TValue)));

        /// <summary>Declares a property whose plan member maps to a different JavaScript path.</summary>
        internal static ComponentProperty<TValue> Mapped(string member, string pathExpression) =>
            new ComponentProperty<TValue>(member, pathExpression, Shape.FromClrType(typeof(TValue)));
    }

    /// <summary>
    /// A declared method: its plan key (its name, then its argument shapes), its JavaScript path, and
    /// whether it returns a promise the steps after it wait for.
    /// </summary>
    internal sealed class ComponentMethod
    {
        private readonly string _name;
        private readonly Path _path;
        private readonly IReadOnlyList<Shape> _argumentShapes;
        private readonly bool _awaits;

        private ComponentMethod(string name, string pathExpression, IReadOnlyList<Shape> argumentShapes, bool awaits)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name required.", nameof(name));

            _name = name;
            JavaScriptName = pathExpression ?? throw new ArgumentNullException(nameof(pathExpression));
            _path = Path.Parse(pathExpression);
            _argumentShapes = argumentShapes ?? throw new ArgumentNullException(nameof(argumentShapes));
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

        /// <summary>The JavaScript name the method has on its object; empty for a plugin that is itself a function.</summary>
        internal string JavaScriptName { get; }

        internal MethodSignature SignatureReturning(Shape returns)
        {
            if (returns == null) throw new ArgumentNullException(nameof(returns));
            return MethodSignature.WithArguments(MethodArgumentContract.Exact(_argumentShapes), returns, _awaits);
        }

        internal ObjectMethodContract ContractReturning(Shape returns) =>
            ObjectMethodContract.Create(MemberName.Of(Member), _path, SignatureReturning(returns));

        /// <summary>A method taking arguments of the given shapes.</summary>
        internal static ComponentMethod Of(string member, string pathExpression, IReadOnlyList<Shape> argumentShapes, bool awaits) =>
            new ComponentMethod(member, pathExpression, argumentShapes, awaits);

        /// <summary>Declares a method whose plan member maps to a different JavaScript path.</summary>
        internal static ComponentMethod Mapped(string member, string pathExpression) =>
            new ComponentMethod(member, pathExpression, Array.Empty<Shape>(), awaits: false);

        internal ComponentMethod WithArgs<T1>() =>
            new ComponentMethod(_name, JavaScriptName, new[] { Shape.FromClrType(typeof(T1)) }, _awaits);
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
