using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Represents the value produced by a plan-registered plugin method call in conditions,
    /// reactions, or gather.
    /// </summary>
    /// <typeparam name="TProp">The CLR type returned by the plugin call.</typeparam>
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

    /// <summary>Represents a readable plan-registered plugin property in conditions, reactions, or gather.</summary>
    /// <typeparam name="TProp">The CLR type exposed by the readable plugin property.</typeparam>
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
