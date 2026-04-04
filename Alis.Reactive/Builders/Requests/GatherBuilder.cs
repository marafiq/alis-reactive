using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Collects values from form components, event payloads, and static data to build the
    /// HTTP request body or URL parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// "Gather" is the DSL term for authoring request values at request time.
    /// Each configured value resolves to a key/value pair in the request payload.
    /// </para>
    /// <para>
    /// Component-specific gather methods (e.g., <c>g.Include(m =&gt; m.Name)</c>) are provided
    /// by vendor extensions in <c>Alis.Reactive.Native</c> and <c>Alis.Reactive.Fusion</c>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class GatherBuilder<TModel> where TModel : class
    {
        private readonly PlanAuthoringContext _authoring;
        private readonly WorkflowScope _scope;

        internal GatherBuilder(PlanAuthoringContext authoring, WorkflowScope scope)
        {
            _authoring = authoring;
            _scope = scope;
        }

        internal List<RequestValuePart> RequestValues { get; } = new List<RequestValuePart>();

        /// <summary>
        /// Adds a component value to the authored request payload.
        /// </summary>
        internal GatherBuilder<TModel> IncludeComponentValue(
            string key,
            string componentId,
            ComponentMetadata component,
            CapabilityProperty binding,
            ValueShape? shape = null)
        {
            RequestValues.Add(new ComponentRequestValue(key, componentId, component, binding, shape));
            return this;
        }

        /// <summary>
        /// Gathers the current value of every input component created via
        /// <c>Html.InputField(plan, ...)</c> in this plan. Each component's value is sent
        /// using the model property name as the key.
        /// </summary>
        public GatherBuilder<TModel> IncludeAll()
        {
            RequestValues.Add(new IncludeAllBindingsRequestValue());
            return this;
        }

        /// <summary>
        /// Adds a static key/value pair to the request.
        /// </summary>
        /// <param name="param">The key name used in the request payload.</param>
        /// <param name="value">The constant value to include.</param>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            RequestValues.Add(new LiteralRequestValue(param, value));
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
            RequestValues.Add(new ContextRequestValue(param, _authoring.Values.DescribeEventPayload(_scope, path).Expression));
            return this;
        }
    }
}
