using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads the current value of a component in the browser.
    /// Returned by each component's Value() extension method.
    /// </summary>
    /// <summary>A typed value source produced by a registered component member.</summary>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly ValueProducer _value;
        private readonly string _readMember;

        internal TypedComponentSource(string componentId, string valueMember)
            : this(
                valueMember,
                ValueProducer.Read(PlanModel.ComponentSource.Of(componentId), valueMember, shape: Shape.FromClrType(typeof(TProp))))
        {
        }

        private TypedComponentSource(string readMember, ValueProducer value)
        {
            _readMember = readMember;
            _value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal override ValueProducer ToValueProducer() => _value;

        internal override string ReadMember => _readMember;

        internal static TypedComponentSource<TProp> FromMethod(
            PlanModel.ComponentSource component,
            string method,
            System.Collections.Generic.IReadOnlyList<ValueProducer> args)
        {
            return new TypedComponentSource<TProp>(
                method,
                ValueProducer.Invoke(
                    component,
                    method,
                    Shape.FromClrType(typeof(TProp)),
                    args));
        }
    }
}
