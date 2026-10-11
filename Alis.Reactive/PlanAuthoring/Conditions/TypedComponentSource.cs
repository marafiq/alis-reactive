using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>Typed value source produced by a registered component member.</summary>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly ValueExpression _value;
        private readonly string _readMember;

        internal TypedComponentSource(string componentId, string valueMember)
            : this(
                valueMember,
                ValueExpression.Read(PlanModel.ComponentSource.Of(componentId), valueMember, shape: Shape.FromClrType(typeof(TProp))))
        {
        }

        private TypedComponentSource(string readMember, ValueExpression value)
        {
            _readMember = readMember;
            _value = value;
        }

        internal override ValueExpression ToValueExpression() => _value;

        internal string DefaultBodyFieldName => _readMember;

        /// <param name="component">The component whose method is read.</param>
        /// <param name="method">The method's plan key.</param>
        /// <param name="name">The method's own name, which a gathered body calls the read by default.</param>
        /// <param name="args">The arguments the read passes.</param>
        internal static TypedComponentSource<TProp> FromMethod(
            PlanModel.ComponentSource component,
            string method,
            string name,
            System.Collections.Generic.IReadOnlyList<ValueExpression> args)
        {
            return new TypedComponentSource<TProp>(
                name,
                ValueExpression.Invoke(
                    component,
                    method,
                    Shape.FromClrType(typeof(TProp)),
                    args));
        }
    }
}
