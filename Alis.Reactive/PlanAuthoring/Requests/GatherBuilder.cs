using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Configures HTTP request input: body fields, headers, and route template
    /// parameters.
    /// </summary>
    /// <remarks>
    /// Obtained from request builders through <c>.Gather(g =&gt; ...)</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs.</typeparam>
    public class GatherBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly GatherInputDraft _draft;

        internal GatherBuilder(PlanBuildContext context, GatherInputDraft draft)
        {
            _context = context;
            _draft = draft;
        }

        /// <summary>
        /// Selects all registered input components for the request body. Only
        /// mounted registered inputs are read at runtime.
        /// </summary>
        public GatherBuilder<TModel> IncludeAll()
        {
            _draft.IncludeAllRegisteredInputs();
            return this;
        }

        /// <summary>Adds a literal request body field.</summary>
        /// <param name="param">The request body field name.</param>
        /// <param name="value">The constant value serialized into the generated plan.</param>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            var payloadPath = BindingPath.Of(param);
            _draft.AddPayload(
                payloadPath,
                ValueExpression.LiteralFromValue(value));
            return this;
        }

        /// <summary>Adds a value from the triggering event payload to the request body.</summary>
        /// <typeparam name="TArgs">The event payload type.</typeparam>
        /// <typeparam name="TProp">The event value type copied into the request body.</typeparam>
        /// <param name="args">The typed event payload parameter from the trigger callback.</param>
        /// <param name="path">The event payload property path to read.</param>
        /// <param name="param">The request body field name.</param>
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
                ValueExpression.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        /// <summary>Adds a literal string header to the HTTP request.</summary>
        /// <param name="name">The HTTP header name.</param>
        /// <param name="value">The non-null header value serialized into the generated plan.</param>
        public GatherBuilder<TModel> Header(string name, string value)
        {
            var header = HeaderName.Of(name);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Header '{name}' value must not be null. Literal headers require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            _draft.AddHeader(header, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Adds a scalar HTTP header value from a typed source.</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="name">The HTTP header name.</param>
        /// <param name="source">The typed value source to evaluate before the request is sent.</param>
        public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source)
        {
            var header = HeaderName.Of(name);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.Header<TProp>(header);
            _draft.AddHeader(header, source.ToValueExpression());
            return this;
        }

        /// <summary>Adds a scalar HTTP header value from the triggering event payload.</summary>
        /// <typeparam name="TArgs">The event payload type.</typeparam>
        /// <typeparam name="TProp">The event value type sent as the header value.</typeparam>
        /// <param name="name">The HTTP header name.</param>
        /// <param name="args">The typed event payload parameter from the trigger callback.</param>
        /// <param name="path">The event payload property path to send as a header.</param>
        public GatherBuilder<TModel> Header<TArgs, TProp>(string name, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var header = HeaderName.Of(name);
            var shape = RequestScalarTarget.Header<TProp>(header);
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddHeader(header, ValueExpression.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        /// <summary>Binds a route template parameter to an int literal.</summary>
        /// <param name="paramName">The route template placeholder name without braces.</param>
        /// <param name="value">The route value serialized into the generated plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, int value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a non-null string literal.</summary>
        /// <param name="paramName">The route template placeholder name without braces.</param>
        /// <param name="value">The route value serialized into the generated plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, string value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Route param '{paramName}' value must not be null. Literal route params require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a long literal.</summary>
        /// <param name="paramName">The route template placeholder name without braces.</param>
        /// <param name="value">The route value serialized into the generated plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, long value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a scalar typed source.</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="paramName">The route template placeholder name without braces.</param>
        /// <param name="source">The typed value source to evaluate before the request is sent.</param>
        public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.RouteParameter<TProp>(routeParam);
            _draft.AddRouteParameter(routeParam, source.ToValueExpression());
            return this;
        }

        /// <summary>Binds a route template parameter to a scalar value from the triggering event payload.</summary>
        /// <typeparam name="TArgs">The event payload type.</typeparam>
        /// <typeparam name="TProp">The event value type sent as the route value.</typeparam>
        /// <param name="paramName">The route template placeholder name without braces.</param>
        /// <param name="args">The typed event payload parameter from the trigger callback.</param>
        /// <param name="path">The event payload property path to use as the route value.</param>
        public GatherBuilder<TModel> RouteParam<TArgs, TProp>(
            string paramName, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            var routeParam = RouteParameterName.Of(paramName);
            var shape = RequestScalarTarget.RouteParameter<TProp>(routeParam);
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddRouteParameter(routeParam, ValueExpression.ReadPayload(PayloadSource.Event(), eventPath, shape));
            return this;
        }

        /// <summary>Reads a URL query parameter into a request body field with the same name.</summary>
        /// <param name="paramName">The URL query parameter name and request body field name.</param>
        public GatherBuilder<TModel> FromUrl(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(urlParam.Value);
            var value = ValueExpression.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        /// <summary>Reads a URL query parameter into an explicit request body field.</summary>
        /// <param name="paramName">The URL query parameter name to read.</param>
        /// <param name="asParam">The request body field name to send.</param>
        public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(asParam);
            var value = ValueExpression.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        /// <summary>Reads a typed URL query parameter into a request body field with the same name.</summary>
        /// <typeparam name="T">The expected query parameter value type.</typeparam>
        /// <param name="paramName">The URL query parameter name and request body field name.</param>
        public GatherBuilder<TModel> FromUrl<T>(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(urlParam.Value);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var value = ValueExpression.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        /// <summary>Reads a typed URL query parameter into an explicit request body field.</summary>
        /// <typeparam name="T">The expected query parameter value type.</typeparam>
        /// <param name="paramName">The URL query parameter name to read.</param>
        /// <param name="asParam">The request body field name to send.</param>
        public GatherBuilder<TModel> FromUrl<T>(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var payloadPath = BindingPath.Of(asParam);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var value = ValueExpression.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, value));
            return this;
        }

        /// <summary>Adds a plan-registered plugin method result to the request body.</summary>
        /// <typeparam name="T">The plugin method return type.</typeparam>
        /// <param name="source">The typed plugin value source, including any arguments already configured.</param>
        /// <param name="paramName">The request body field name.</param>
        public GatherBuilder<TModel> Plugin<T>(Conditions.TypedPluginSource<T> source, string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            var payloadPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, source.ToValueExpression()));
            return this;
        }

        /// <summary>
        /// Includes a specific component's value in the gather.
        /// Used by vendor extension methods (Fusion, Native).
        /// </summary>
        internal GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember)
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
            var componentValue = ValueExpression.Read(
                ComponentSource.Of(componentIdentity.ComponentId.Value),
                valueContract.ValueMember,
                shape: valueContract.Shape);
            var planBinding = InputComponentPlanBinding.For(
                componentIdentity.ComponentId,
                componentIdentity.Vendor,
                planBindingPath,
                valueContract);
            _context.DeclareInputComponent(planBinding);
            _draft.AddAssignment(RequestInputAssignment.Payload(planBindingPath, componentValue));
            return this;
        }

        internal GatherBuilder<TModel> Include<TProp>(
            TypedComponentSource<TProp> source,
            string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            var payloadPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(payloadPath, source.ToValueExpression()));
            return this;
        }

    }

}
