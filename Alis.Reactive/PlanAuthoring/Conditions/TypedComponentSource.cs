using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>Typed value source produced by a registered component member.</summary>
    public sealed class TypedComponentSource<TProp> : TypedSource<TProp>
    {
        private readonly ValueExpression _value;
        private readonly string _readMember;

        private TypedComponentSource(string readMember, ValueExpression value)
        {
            _readMember = readMember;
            _value = value;
        }

        internal override ValueExpression ToValueExpression() => _value;

        internal string DefaultBodyFieldName => _readMember;

        /// <param name="component">The component whose property is read.</param>
        /// <param name="name">The property's own name, which a gathered body calls the read by default.</param>
        /// <param name="path">The JavaScript path of the property on the component's object.</param>
        internal static TypedComponentSource<TProp> FromProperty(
            PlanModel.ComponentSource component,
            string name,
            Path path) =>
            new TypedComponentSource<TProp>(
                name,
                ValueExpression.Read(component, name, path, Shape.FromClrType(typeof(TProp))));

        /// <param name="component">The component whose method is read.</param>
        /// <param name="name">The method's own name, which a gathered body calls the read by default.</param>
        /// <param name="path">The JavaScript path of the method on the component's object.</param>
        /// <param name="arguments">The declared shape of each argument.</param>
        /// <param name="args">The arguments the read passes.</param>
        internal static TypedComponentSource<TProp> FromMethod(
            PlanModel.ComponentSource component,
            string name,
            Path path,
            MethodArgumentContract arguments,
            IReadOnlyList<ValueExpression> args) =>
            new TypedComponentSource<TProp>(
                name,
                ValueExpression.Invoke(component, name, path, arguments, Shape.FromClrType(typeof(TProp)), args));
    }
}
