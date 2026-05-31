using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a method return value from a registered plugin.
    /// Returned by <c>PipelineBuilder.Plugin&lt;T&gt;()</c> via implicit conversion from <see cref="PluginMemberBuilder{TReturn, TModel}"/>.
    /// </summary>
    public sealed class TypedPluginSource<TProp> : TypedSource<TProp>
    {
        private readonly PluginOperationId _operation;
        private readonly List<ValueExpression> _args;

        internal TypedPluginSource(PluginOperationId operation, List<ValueExpression> args)
        {
            _operation = operation ?? throw new System.ArgumentNullException(nameof(operation));
            _args = args ?? throw new System.ArgumentNullException(nameof(args));
        }

        internal override ValueExpression ToValueExpression() =>
            ValueExpression.Invoke(
                PluginSource.Of(_operation.PluginNameValue),
                _operation.PlanMethodNameValue,
                Shape,
                _args);
    }

    /// <summary>A typed source that reads a property from a registered plugin object.</summary>
    public sealed class TypedPluginPropertySource<TProp> : TypedSource<TProp>
    {
        private readonly PluginPropertyId _property;

        internal TypedPluginPropertySource(PluginPropertyId property)
        {
            _property = property ?? throw new System.ArgumentNullException(nameof(property));
        }

        internal override ValueExpression ToValueExpression() =>
            ValueExpression.Read(
                PluginSource.Of(_property.PluginNameValue),
                _property.PlanMemberNameValue,
                Shape);
    }
}
