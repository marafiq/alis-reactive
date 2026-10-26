using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>A declared property: its path from the object a page holds, and the shape of its value.</summary>
    /// <typeparam name="TValue">The C# type of the value.</typeparam>
    internal sealed class ComponentProperty<TValue>
    {
        private ComponentProperty(Path path)
        {
            if (path.IsNone) throw new ArgumentException("Property path required.", nameof(path));

            Path = path;
            Shape = Shape.FromClrType(typeof(TValue));
        }

        internal Path Path { get; }

        internal Shape Shape { get; }

        internal static ComponentProperty<TValue> At(Path path) => new ComponentProperty<TValue>(path);
    }

    /// <summary>
    /// A declared method: the C# member that declares it, its path from the object a page holds, the shapes
    /// its arguments are converted to, and whether it returns a promise the steps after it wait for.
    /// </summary>
    internal sealed class ComponentMethod
    {
        private ComponentMethod(string name, Path path, IReadOnlyList<Shape> argumentShapes, bool awaits)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name required.", nameof(name));

            // An empty path calls the object itself: a plugin that is a function.
            Name = name;
            Path = path;
            ArgumentShapes = argumentShapes ?? throw new ArgumentNullException(nameof(argumentShapes));
            Awaits = awaits;
        }

        internal string Name { get; }

        internal Path Path { get; }

        internal IReadOnlyList<Shape> ArgumentShapes { get; }

        internal bool Awaits { get; }

        internal static ComponentMethod Of(string name, Path path, IReadOnlyList<Shape> argumentShapes, bool awaits) =>
            new ComponentMethod(name, path, argumentShapes, awaits);
    }

    /// <summary>The members of an element <c>p.Element(...)</c> reaches: its text, HTML, hidden attribute and class list.</summary>
    internal static class BrowserElementMembers
    {
        private static readonly IReadOnlyList<Shape> OneText = new[] { Shape.String };

        private static readonly Path ClassList = Path.None.Then("classList");

        internal static ComponentMethod AddClass { get; } = ComponentMethod.Of("ClassList.Add", ClassList.Then("add"), OneText, awaits: false);

        internal static ComponentMethod RemoveClass { get; } = ComponentMethod.Of("ClassList.Remove", ClassList.Then("remove"), OneText, awaits: false);

        internal static ComponentMethod ToggleClass { get; } = ComponentMethod.Of("ClassList.Toggle", ClassList.Then("toggle"), OneText, awaits: false);

        internal static ComponentProperty<string> Text { get; } = ComponentProperty<string>.At(Path.None.Then("textContent"));

        internal static ComponentProperty<string> Html { get; } = ComponentProperty<string>.At(Path.None.Then("innerHTML"));

        internal static ComponentProperty<bool> Hidden { get; } = ComponentProperty<bool>.At(Path.None.Then("hidden"));
    }
}
