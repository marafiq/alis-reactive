using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Builds the HTTP request payload by gathering values from components, static data, event args, plugins, headers, route params, and URL query params.
    /// </summary>
    /// <remarks>
    /// Obtained via <c>.Gather(g =&gt; g.Include(m =&gt; m.Name).Header("X-Key", val))</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public class GatherBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly RequestInputProjectionDraft _draft;

        internal GatherBuilder(PlanBuildContext context, RequestInputProjectionDraft draft)
        {
            _context = context;
            _draft = draft;
        }

        /// <summary>Includes all registered input component values in the request payload.</summary>
        /// <returns>This builder for chaining.</returns>
        public GatherBuilder<TModel> IncludeAll()
        {
            _draft.IncludeAllRegisteredInputs();
            return this;
        }

        /// <summary>Includes a static key-value pair in the request payload.</summary>
        /// <param name="param">The HTTP parameter name.</param>
        /// <param name="value">The constant value to send.</param>
        /// <returns>This builder for chaining.</returns>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            var payloadPath = BindingPath.Of(param);
            _draft.AddPayload(
                payloadPath,
                ValueProducer.LiteralFromValue(value));
            return this;
        }

        /// <summary>Includes a value from the triggering event payload in the request.</summary>
        /// <typeparam name="TArgs">The event args type.</typeparam>
        /// <typeparam name="TProp">The property type to extract.</typeparam>
        /// <param name="args">The event args instance.</param>
        /// <param name="path">Expression selecting the property from the event args.</param>
        /// <param name="param">The HTTP parameter name.</param>
        /// <returns>This builder for chaining.</returns>
        public GatherBuilder<TModel> FromEvent<TArgs, TProp>(
            TArgs args,
            Expression<Func<TArgs, TProp>> path,
            string param)
        {
            var payloadPath = BindingPath.Of(param);
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            _draft.AddPayload(
                payloadPath,
                ValueProducer.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        /// <summary>Adds a literal string header to the HTTP request. Value must not be null — use a typed source overload for dynamic/nullable values.</summary>
        public GatherBuilder<TModel> Header(string name, string value)
        {
            var header = HeaderName.Of(name);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Header '{name}' value must not be null. Literal headers require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            _draft.AddHeader(header, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Adds a header from a typed source. HTTP headers are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source)
        {
            var header = HeaderName.Of(name);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.Header<TProp>(header);
            _draft.AddHeader(header, source.ToValueProducer());
            return this;
        }

        /// <summary>Adds a header from an event arg expression. HTTP headers are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> Header<TArgs, TProp>(string name, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var header = HeaderName.Of(name);
            var shape = RequestScalarTarget.Header<TProp>(header);
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddHeader(header, ValueProducer.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        // ── Route Params ─────────────────────────────────────

        /// <summary>Adds a route param from a static int.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, int value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Adds a route param from a static string. Value must not be null.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, string value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Route param '{paramName}' value must not be null. Literal route params require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            _draft.AddRouteParameter(routeParam, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Adds a route param from a static long.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, long value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueProducer.Literal(value));
            return this;
        }

        /// <summary>Adds a route param from a typed source. Route params are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.RouteParameter<TProp>(routeParam);
            _draft.AddRouteParameter(routeParam, source.ToValueProducer());
            return this;
        }

        /// <summary>Adds a route param from an event arg expression. Route params are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> RouteParam<TArgs, TProp>(
            string paramName, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var routeParam = RouteParameterName.Of(paramName);
            var shape = RequestScalarTarget.RouteParameter<TProp>(routeParam);
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddRouteParameter(routeParam, ValueProducer.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        // ── URL Query Params ──────────────────────────────────

        /// <summary>
        /// Includes a URL query parameter value in the gather.
        /// The parameter name is used as both the URL param to read and the HTTP payload path.
        /// </summary>
        public GatherBuilder<TModel> FromUrl(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var value = ValueProducer.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(urlParam.Value, value));
            return this;
        }

        /// <summary>
        /// Includes a URL query parameter with an explicit HTTP request parameter name.
        /// </summary>
        public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(asParam);
            var value = ValueProducer.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        /// <summary>
        /// Includes a typed URL query parameter in the gather with shape conversion.
        /// </summary>
        public GatherBuilder<TModel> FromUrl<T>(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var value = ValueProducer.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(urlParam.Value, value));
            return this;
        }

        /// <summary>
        /// Includes a typed URL query parameter with an explicit HTTP request parameter name.
        /// </summary>
        public GatherBuilder<TModel> FromUrl<T>(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(asParam);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var value = ValueProducer.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        // ── Plugin ────────────────────────────────────────────

        /// <summary>Includes a plugin method result in the gather. Accepts TypedPluginSource which may carry args.</summary>
        public GatherBuilder<TModel> Plugin<T>(Conditions.TypedPluginSource<T> source, string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            var payloadPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, source.ToValueProducer()));
            return this;
        }

        /// <summary>
        /// Includes a specific component's value in the gather.
        /// Used by vendor extension methods (Fusion, Native).
        /// </summary>
        public GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember)
        {
            var valueRead = RegisteredInputValueRead.ForGatherValueRead(componentId, valueMember);
            var registration = _context.RequireRegistrationById(componentId, valueRead);
            var valueContract = registration.RequireValueContract(
                valueRead.ValueMember);
            return Include(componentId, vendor, propertyName, valueContract);
        }

        internal GatherBuilder<TModel> Include(
            string componentId,
            string vendor,
            string propertyName,
            string valueMember,
            Shape shape)
        {
            if (shape == null) throw new System.ArgumentNullException(nameof(shape));
            return Include(
                componentId,
                vendor,
                propertyName,
                InputValueContract.For(valueMember, shape));
        }

        private GatherBuilder<TModel> Include(
            string componentId,
            string vendor,
            string propertyName,
            InputValueContract valueContract)
        {
            if (valueContract == null) throw new System.ArgumentNullException(nameof(valueContract));
            var componentIdentity = RegisteredComponentIdentity.For(componentId, vendor);
            var planBindingPath = BindingPath.Of(propertyName);
            var componentValue = ValueProducer.Read(
                ComponentSource.Of(componentIdentity.ComponentId.Value),
                valueContract.ValueMember,
                shape: valueContract.Shape);
            var planBinding = InputComponentPlanBinding.For(
                componentIdentity.ComponentId,
                componentIdentity.Vendor,
                planBindingPath,
                valueContract);
            _context.EnsureInputComponent(planBinding);
            _draft.AddAssignment(RequestInputAssignment.Payload(planBindingPath, componentValue));
            return this;
        }

        internal GatherBuilder<TModel> Include<TProp>(
            TypedComponentSource<TProp> source,
            string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            var payloadPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, source.ToValueProducer()));
            return this;
        }

    }

}
