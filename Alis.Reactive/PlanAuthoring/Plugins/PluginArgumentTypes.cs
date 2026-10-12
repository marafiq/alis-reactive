using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>The ordered argument types a plugin member accepts, declared on a <see cref="Plugin"/>.</summary>
    public sealed class PluginArgumentTypes
    {
        private readonly List<Shape> _shapes = new List<Shape>();

        internal IReadOnlyList<Shape> Shapes => _shapes;

        /// <summary>Appends one argument type.</summary>
        /// <typeparam name="T">Argument type accepted by the plugin member.</typeparam>
        public PluginArgumentTypes Arg<T>()
        {
            _shapes.Add(Shape.FromClrType(typeof(T)));
            return this;
        }
    }
}
