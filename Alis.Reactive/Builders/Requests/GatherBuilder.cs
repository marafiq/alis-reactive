using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Collects values from form components, event payloads, and static data to build the
    /// HTTP request body or URL parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Gather" is the framework term for collecting input values at request time.
    /// Each gather item resolves to a key/value pair in the request payload.
    /// </para>
    /// <para>
    /// Component-specific gather methods (e.g., <c>g.Include(m =&gt; m.Name)</c>) are provided
    /// by vendor extensions in <c>Alis.Reactive.Native</c> and <c>Alis.Reactive.Fusion</c>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class GatherBuilder<TModel> where TModel : class
    {
        internal List<GatherItem> Items { get; } = new List<GatherItem>();

        /// <summary>
        /// Adds a pre-built gather item. Used by vendor extension methods
        /// (Fusion, Native) to add their own component gather descriptors.
        /// </summary>
        public GatherBuilder<TModel> AddItem(GatherItem item)
        {
            Items.Add(item);
            return this;
        }

        /// <summary>
        /// Gathers the current value of every input component created via
        /// <c>Html.InputField(plan, ...)</c> in this plan. Each component's value is sent
        /// using the model property name as the key.
        /// </summary>
        public GatherBuilder<TModel> IncludeAll()
        {
            Items.Add(new AllGather());
            return this;
        }

        /// <summary>
        /// Adds a static key/value pair to the request.
        /// </summary>
        /// <param name="param">The key name used in the request payload.</param>
        /// <param name="value">The constant value to include.</param>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            Items.Add(new StaticGather(param, value));
            return this;
        }

        /// <summary>
        /// Gathers a value from the event that triggered this pipeline.
        /// </summary>
        /// <param name="args">The event args instance (used for type inference, not evaluated).</param>
        /// <param name="path">Expression selecting the payload property to gather (e.g., <c>x =&gt; x.Text</c>).</param>
        /// <param name="param">The key name in the request payload.</param>
        public GatherBuilder<TModel> FromEvent<TArgs, TProp>(
            TArgs args,
            Expression<Func<TArgs, TProp>> path,
            string param)
        {
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            Items.Add(new EventGather(param, eventPath));
            return this;
        }
    }
}
