using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>A declared property: its JavaScript path and the shape of its value.</summary>
    /// <typeparam name="TValue">The C# type of the property value.</typeparam>
    internal sealed class ComponentProperty<TValue>
    {
        private ComponentProperty(string pathExpression)
        {
            if (string.IsNullOrWhiteSpace(pathExpression))
                throw new ArgumentException("Property path required.", nameof(pathExpression));

            JavaScriptPath = pathExpression;
            Path = Path.Parse(pathExpression);
            Shape = Shape.FromClrType(typeof(TValue));
        }

        /// <summary>The property's name on its object, a dotted path for a nested one.</summary>
        internal string JavaScriptPath { get; }

        internal Path Path { get; }

        internal Shape Shape { get; }

        internal static ComponentProperty<TValue> At(string pathExpression) => new ComponentProperty<TValue>(pathExpression);
    }

    /// <summary>
    /// A declared method: the C# member that declares it, its JavaScript path, the shapes its arguments are
    /// converted to, and whether it returns a promise the steps after it wait for.
    /// </summary>
    internal sealed class ComponentMethod
    {
        private ComponentMethod(string name, string pathExpression, IReadOnlyList<Shape> argumentShapes, bool awaits)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name required.", nameof(name));

            Name = name;
            JavaScriptName = pathExpression ?? throw new ArgumentNullException(nameof(pathExpression));
            Path = Path.Parse(pathExpression);
            ArgumentShapes = argumentShapes ?? throw new ArgumentNullException(nameof(argumentShapes));
            Awaits = awaits;
        }

        /// <summary>The C# member that declares the method, named in errors.</summary>
        internal string Name { get; }

        /// <summary>The JavaScript name the method has on its object; empty for a plugin that is itself a function.</summary>
        internal string JavaScriptName { get; }

        internal Path Path { get; }

        internal IReadOnlyList<Shape> ArgumentShapes { get; }

        internal bool Awaits { get; }

        /// <summary>A method taking arguments of the given shapes.</summary>
        internal static ComponentMethod Of(string name, string pathExpression, IReadOnlyList<Shape> argumentShapes, bool awaits) =>
            new ComponentMethod(name, pathExpression, argumentShapes, awaits);
    }

    /// <summary>The members of an element <c>p.Element(...)</c> reaches: its text, HTML, hidden attribute and class list.</summary>
    internal static class BrowserElementMembers
    {
        private static readonly IReadOnlyList<Shape> OneText = new[] { Shape.String };

        internal static ComponentMethod AddClass { get; } = ComponentMethod.Of("ClassList.Add", "classList.add", OneText, awaits: false);

        internal static ComponentMethod RemoveClass { get; } = ComponentMethod.Of("ClassList.Remove", "classList.remove", OneText, awaits: false);

        internal static ComponentMethod ToggleClass { get; } = ComponentMethod.Of("ClassList.Toggle", "classList.toggle", OneText, awaits: false);

        internal static ComponentProperty<string> Text { get; } = ComponentProperty<string>.At("textContent");

        internal static ComponentProperty<string> Html { get; } = ComponentProperty<string>.At("innerHTML");

        internal static ComponentProperty<bool> Hidden { get; } = ComponentProperty<bool>.At("hidden");
    }
}
