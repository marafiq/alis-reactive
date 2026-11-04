using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.Components
{
    internal sealed class ComponentProperty<TValue>
    {
        private ComponentProperty(Path path)
        {
            if (path.IsNone) throw new ArgumentException("Property path required.", nameof(path));

            Path = path;
            Shape = ArgumentShape.Of(typeof(TValue));
        }

        internal Path Path { get; }

        internal Shape Shape { get; }

        internal static ComponentProperty<TValue> At(Path path) => new ComponentProperty<TValue>(path);
    }

    internal sealed class ComponentMethod
    {
        private ComponentMethod(string name, Path path, IReadOnlyList<Shape> argumentShapes, bool awaits)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Method name required.", nameof(name));

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
