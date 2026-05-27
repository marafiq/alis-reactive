using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Preserves the property type through the condition and mutation pipeline
    /// for compile-time type safety.
    /// </summary>
    public abstract class TypedSource<TProp>
    {
        /// <summary>
        /// Produces a ValueProducer that reads this source's value.
        /// </summary>
        internal abstract ValueProducer ToValueProducer();

        /// <summary>
        /// The member name to read on the resolved source.
        /// </summary>
        internal virtual string ReadMember => throw new System.InvalidOperationException("Not a component source.");

        /// <summary>
        /// Shape inferred from TProp.
        /// </summary>
        internal Shape Shape => Shape.FromClrType(typeof(TProp));

        /// <summary>
        /// Element shape for array types (e.g., Shape.String for string[]).
        /// </summary>
        internal Shape ElementShape =>
            Shape.CollectionItemShapeOrNone(typeof(TProp));
    }
}
