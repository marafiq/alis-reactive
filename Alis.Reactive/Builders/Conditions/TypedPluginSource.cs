using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// A typed source that reads a method return value from a registered plugin.
    /// Returned by <c>PipelineBuilder.Plugin&lt;T&gt;()</c> via implicit conversion from <see cref="PluginReadBuilder{TReturn, TModel}"/>.
    /// </summary>
    public sealed class TypedPluginSource<TProp> : TypedSource<TProp>
    {
        private readonly string _pluginName;
        private readonly string _member;
        private readonly List<ValueProducer> _args;

        internal TypedPluginSource(string pluginName, string member, List<ValueProducer> args = null)
        {
            _pluginName = pluginName;
            _member = member;
            _args = args;
        }

        internal override ValueProducer ToValueProducer() =>
            ValueProducer.Read(PluginSource.Of(_pluginName), _member, shape: Shape, args: _args);
    }
}
