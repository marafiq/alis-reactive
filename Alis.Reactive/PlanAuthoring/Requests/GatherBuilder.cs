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
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
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

        /// <summary>Adds literal request body field.</summary>
        /// <param name="param">Request body field that receives the literal value.</param>
        /// <param name="value">Literal body value captured into the generated request plan.</param>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            var bodyPath = BindingPath.Of(param);
            _draft.AddPayload(
                bodyPath,
                ValueExpression.LiteralFromValue(value));
            return this;
        }

        /// <summary>Adds triggering event payload value to the request body.</summary>
        /// <typeparam name="TPayload">Trigger payload contract.</typeparam>
        /// <typeparam name="TProp">Selected event-payload body value type.</typeparam>
        /// <param name="args">Trigger payload placeholder.</param>
        /// <param name="path">Event payload path.</param>
        /// <param name="param">Request body field that receives the payload value.</param>
        public GatherBuilder<TModel> FromEvent<TPayload, TProp>(
            TPayload args,
            Expression<Func<TPayload, TProp>> path,
            string param)
        {
            var bodyPath = BindingPath.Of(param);
            var payloadPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            _draft.AddPayload(
                bodyPath,
                ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath, shape));
            return this;
        }

        /// <summary>Adds literal string header to the HTTP request.</summary>
        /// <param name="name">HTTP header name.</param>
        /// <param name="value">Non-null header value serialized into the generated plan.</param>
        public GatherBuilder<TModel> Header(string name, string value)
        {
            var header = HeaderName.Of(name);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Header '{name}' value must not be null. Literal headers require a concrete value. " +
                    "Use the TypedSource<T> or event-payload overload for dynamic/nullable values.");
            _draft.AddHeader(header, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Adds scalar HTTP header value from a typed source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="name">HTTP header name.</param>
        /// <param name="source">Typed value source evaluated before the request is sent.</param>
        public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source)
        {
            var header = HeaderName.Of(name);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.Header<TProp>(header);
            _draft.AddHeader(header, source.ToValueExpression());
            return this;
        }

        /// <summary>Adds scalar HTTP header value from the triggering event payload.</summary>
        /// <typeparam name="TPayload">Trigger payload contract.</typeparam>
        /// <typeparam name="TProp">Selected event-payload header value type.</typeparam>
        /// <param name="name">HTTP header name.</param>
        /// <param name="args">Trigger payload placeholder.</param>
        /// <param name="path">Event payload header path.</param>
        public GatherBuilder<TModel> Header<TPayload, TProp>(
            string name,
            TPayload args,
            Expression<Func<TPayload, TProp>> path)
        {
            var header = HeaderName.Of(name);
            var shape = RequestScalarTarget.Header<TProp>(header);
            var payloadPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddHeader(header, ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath, shape));
            return this;
        }

        /// <summary>Binds a route template parameter to an int literal.</summary>
        /// <param name="paramName">Route template placeholder name without braces.</param>
        /// <param name="value">Route value captured into the generated request plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, int value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a non-null string literal.</summary>
        /// <param name="paramName">Route template placeholder name without braces.</param>
        /// <param name="value">Route value captured into the generated request plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, string value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Route param '{paramName}' value must not be null. Literal route params require a concrete value. " +
                    "Use the TypedSource<T> or event-payload overload for dynamic/nullable values.");
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a long literal.</summary>
        /// <param name="paramName">Route template placeholder name without braces.</param>
        /// <param name="value">Route value captured into the generated request plan.</param>
        public GatherBuilder<TModel> RouteParam(string paramName, long value)
        {
            var routeParam = RouteParameterName.Of(paramName);
            _draft.AddRouteParameter(routeParam, ValueExpression.Literal(value));
            return this;
        }

        /// <summary>Binds a route template parameter to a scalar typed source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="paramName">Route template placeholder name without braces.</param>
        /// <param name="source">Typed value source evaluated before the request is sent.</param>
        public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source)
        {
            var routeParam = RouteParameterName.Of(paramName);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequestScalarTarget.RouteParameter<TProp>(routeParam);
            _draft.AddRouteParameter(routeParam, source.ToValueExpression());
            return this;
        }

        /// <summary>Binds a route template parameter to a scalar value from the triggering event payload.</summary>
        /// <typeparam name="TPayload">Trigger payload contract.</typeparam>
        /// <typeparam name="TProp">Selected event-payload route value type.</typeparam>
        /// <param name="paramName">Route template placeholder name without braces.</param>
        /// <param name="args">Trigger payload placeholder.</param>
        /// <param name="path">Event payload route path.</param>
        public GatherBuilder<TModel> RouteParam<TPayload, TProp>(
            string paramName,
            TPayload args,
            Expression<Func<TPayload, TProp>> path)
        {
            var routeParam = RouteParameterName.Of(paramName);
            var shape = RequestScalarTarget.RouteParameter<TProp>(routeParam);
            var payloadPath = ExpressionPathHelper.ToEventPath(path);
            _draft.AddRouteParameter(routeParam, ValueExpression.ReadPayload(PayloadSource.Event(), payloadPath, shape));
            return this;
        }

        /// <summary>Reads URL query parameter into a request body field with the same name.</summary>
        /// <param name="paramName">URL query parameter and request body field.</param>
        public GatherBuilder<TModel> FromUrl(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var bodyPath = BindingPath.Of(urlParam.Value);
            var urlValue = ValueExpression.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, urlValue));
            return this;
        }

        /// <summary>Reads URL query parameter into an explicit request body field.</summary>
        /// <param name="paramName">URL query parameter name to read.</param>
        /// <param name="asParam">Request body field that receives the URL value.</param>
        public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var bodyPath = BindingPath.Of(asParam);
            var urlValue = ValueExpression.ReadUrl(urlParam.Value);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, urlValue));
            return this;
        }

        /// <summary>Reads typed URL query parameter into a request body field with the same name.</summary>
        /// <param name="paramName">URL query parameter and request body field.</param>
        public GatherBuilder<TModel> FromUrl<T>(string paramName)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var bodyPath = BindingPath.Of(urlParam.Value);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var urlValue = ValueExpression.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, urlValue));
            return this;
        }

        /// <summary>Reads typed URL query parameter into an explicit request body field.</summary>
        /// <param name="paramName">URL query parameter name to read.</param>
        /// <param name="asParam">Request body field that receives the typed URL value.</param>
        public GatherBuilder<TModel> FromUrl<T>(string paramName, string asParam)
        {
            var urlParam = UrlParameterName.Of(paramName);
            var bodyPath = BindingPath.Of(asParam);
            var shape = RequestScalarTarget.UrlQueryParameter<T>(urlParam);
            var urlValue = ValueExpression.ReadUrl(urlParam.Value, shape);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, urlValue));
            return this;
        }

        /// <summary>Adds plan-registered plugin method result to the request body.</summary>
        /// <typeparam name="T">The CLR type returned by the plugin call.</typeparam>
        /// <param name="source">Typed plugin value source, including configured arguments.</param>
        /// <param name="paramName">Request body field that receives the plugin result.</param>
        public GatherBuilder<TModel> Plugin<T>(Conditions.TypedPluginSource<T> source, string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            var bodyPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, source.ToValueExpression()));
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
            var bodyPath = BindingPath.Of(paramName);
            _draft.AddAssignment(RequestInputAssignment.Payload(bodyPath, source.ToValueExpression()));
            return this;
        }

    }

}
