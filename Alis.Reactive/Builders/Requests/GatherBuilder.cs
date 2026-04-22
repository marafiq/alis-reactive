using System;
using System.Collections.Generic;
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
        internal List<GatherField> Fields { get; } = new List<GatherField>();
        internal List<StaticField> StaticFields { get; } = new List<StaticField>();
        internal List<EventField> EventFields { get; } = new List<EventField>();
        internal Dictionary<string, ValueProducer> HeaderFields { get; } = new Dictionary<string, ValueProducer>();
        internal Dictionary<string, ValueProducer> RouteParamFields { get; } = new Dictionary<string, ValueProducer>();
        private bool _includeAll;

        internal GatherBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal GatherBuilder<TModel> AddField(GatherField field)
        {
            Fields.Add(field);
            return this;
        }

        /// <summary>Includes all registered input component values in the request payload.</summary>
        /// <returns>This builder for chaining.</returns>
        public GatherBuilder<TModel> IncludeAll()
        {
            _includeAll = true;
            return this;
        }

        /// <summary>Includes a static key-value pair in the request payload.</summary>
        /// <param name="param">The HTTP parameter name.</param>
        /// <param name="value">The constant value to send.</param>
        /// <returns>This builder for chaining.</returns>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            StaticFields.Add(new StaticField(param, value));
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
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            EventFields.Add(new EventField(param, eventPath, shape));
            return this;
        }

        /// <summary>Adds a literal string header to the HTTP request. Value must not be null — use a typed source overload for dynamic/nullable values.</summary>
        public GatherBuilder<TModel> Header(string name, string value)
        {
            ValidateHeaderName(name);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Header '{name}' value must not be null. Literal headers require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            HeaderFields[name] = ValueProducer.Literal(value);
            return this;
        }

        /// <summary>Adds a header from a typed source. HTTP headers are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> Header<TProp>(string name, TypedSource<TProp> source)
        {
            ValidateHeaderName(name);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequireScalarShape<TProp>(name, "header");
            HeaderFields[name] = source.ToValueProducer();
            return this;
        }

        /// <summary>Adds a header from an event arg expression. HTTP headers are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> Header<TArgs, TProp>(string name, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            ValidateHeaderName(name);
            RequireScalarShape<TProp>(name, "header");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            HeaderFields[name] = ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape);
            return this;
        }

        /// <summary>Rejects non-scalar shapes for string-destination values (headers, route params).</summary>
        internal static void RequireScalarShape<TProp>(string paramName, string context)
        {
            var shape = Shape.FromClrType(typeof(TProp));
            if (!shape.IsScalar)
                throw new System.InvalidOperationException(
                    $"{context} '{paramName}' requires a scalar type, but got shape '{shape.Kind}'. " +
                    "Use string, int, bool, DateTime, or their nullable variants.");
        }

        private static void ValidateHeaderName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Header name must not be null or whitespace.", nameof(name));
        }

        // ── Route Params ─────────────────────────────────────

        /// <summary>Adds a route param from a static int.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, int value)
        {
            ValidateRouteParamName(paramName);
            RouteParamFields[paramName] = ValueProducer.Literal(value);
            return this;
        }

        /// <summary>Adds a route param from a static string. Value must not be null.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, string value)
        {
            ValidateRouteParamName(paramName);
            if (value == null)
                throw new System.ArgumentNullException(nameof(value),
                    $"Route param '{paramName}' value must not be null. Literal route params require a concrete value. " +
                    "Use the TypedSource<T> or event-arg overload for dynamic/nullable values.");
            RouteParamFields[paramName] = ValueProducer.Literal(value);
            return this;
        }

        /// <summary>Adds a route param from a static long.</summary>
        public GatherBuilder<TModel> RouteParam(string paramName, long value)
        {
            ValidateRouteParamName(paramName);
            RouteParamFields[paramName] = ValueProducer.Literal(value);
            return this;
        }

        /// <summary>Adds a route param from a typed source. Route params are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> RouteParam<TProp>(string paramName, TypedSource<TProp> source)
        {
            ValidateRouteParamName(paramName);
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            RequireScalarShape<TProp>(paramName, "route param");
            RouteParamFields[paramName] = source.ToValueProducer();
            return this;
        }

        /// <summary>Adds a route param from an event arg expression. Route params are scalar — arrays and objects are rejected at build time.</summary>
        public GatherBuilder<TModel> RouteParam<TArgs, TProp>(
            string paramName, TArgs args, Expression<Func<TArgs, TProp>> path)
        {
            ValidateRouteParamName(paramName);
            RequireScalarShape<TProp>(paramName, "route param");
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            var shape = Shape.FromClrType(typeof(TProp));
            RouteParamFields[paramName] = ValueProducer.Read(PayloadSource.Event(), eventPath, shape: shape);
            return this;
        }


        private void ValidateRouteParamName(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException("Route param name must not be null or whitespace.", nameof(paramName));

            var hasInvalidCharacters = !GatherValidation.IsValidRouteParamName(paramName);
            if (hasInvalidCharacters)
                throw new System.ArgumentException(
                    $"Route param name '{paramName}' contains invalid characters. " +
                    "Names must match [a-zA-Z0-9_] (ASCII only) to align with the runtime {{placeholder}} regex.",
                    nameof(paramName));

            var isDuplicate = RouteParamFields.ContainsKey(paramName);
            if (isDuplicate)
                throw new System.InvalidOperationException(
                    $"Route param '{paramName}' is already defined. Each route param can only be set once.");
        }

        // ── URL Query Params ──────────────────────────────────

        /// <summary>
        /// Includes a URL query parameter value in the gather.
        /// The parameter name is used as both the URL param to read and the HTTP request key.
        /// </summary>
        public GatherBuilder<TModel> FromUrl(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException(
                    "URL param name must not be null or whitespace.", nameof(paramName));
            var value = ValueProducer.ReadUrl(paramName);
            Fields.Add(GatherField.Of(paramName, value));
            return this;
        }

        /// <summary>
        /// Includes a URL query parameter with an explicit HTTP request parameter name.
        /// </summary>
        public GatherBuilder<TModel> FromUrl(string paramName, string asParam)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException(
                    "URL param name must not be null or whitespace.", nameof(paramName));
            if (string.IsNullOrWhiteSpace(asParam))
                throw new System.ArgumentException(
                    "HTTP parameter name must not be null or whitespace.", nameof(asParam));
            var value = ValueProducer.ReadUrl(paramName);
            Fields.Add(GatherField.Of(asParam, value));
            return this;
        }

        /// <summary>
        /// Includes a typed URL query parameter in the gather with shape conversion.
        /// </summary>
        public GatherBuilder<TModel> FromUrl<T>(string paramName)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException(
                    "URL param name must not be null or whitespace.", nameof(paramName));
            RequireScalarShape<T>(paramName, "URL param");
            var shape = Shape.FromClrType(typeof(T));
            var value = ValueProducer.ReadUrl(paramName, shape);
            Fields.Add(GatherField.Of(paramName, value));
            return this;
        }

        /// <summary>
        /// Includes a typed URL query parameter with an explicit HTTP request parameter name.
        /// </summary>
        public GatherBuilder<TModel> FromUrl<T>(string paramName, string asParam)
        {
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException(
                    "URL param name must not be null or whitespace.", nameof(paramName));
            if (string.IsNullOrWhiteSpace(asParam))
                throw new System.ArgumentException(
                    "HTTP parameter name must not be null or whitespace.", nameof(asParam));
            RequireScalarShape<T>(paramName, "URL param");
            var shape = Shape.FromClrType(typeof(T));
            var value = ValueProducer.ReadUrl(paramName, shape);
            Fields.Add(GatherField.Of(asParam, value));
            return this;
        }

        // ── Plugin ────────────────────────────────────────────

        /// <summary>Includes a plugin method result in the gather. Accepts TypedPluginSource which may carry args.</summary>
        public GatherBuilder<TModel> Plugin<T>(Conditions.TypedPluginSource<T> source, string paramName)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(paramName))
                throw new System.ArgumentException("HTTP param name required.", nameof(paramName));
            Fields.Add(GatherField.Of(paramName, source.ToValueProducer()));
            return this;
        }

        /// <summary>
        /// Includes a specific component's value in the gather.
        /// Used by vendor extension methods (Fusion, Native).
        /// </summary>
        public GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember)
        {
            var isAlreadyRegistered = _context.TryFindRegistrationById(componentId, out var reg);
            var resolvedShape = isAlreadyRegistered ? reg.Shape : Shape.Any;
            _context.EnsureInputComponent(componentId, vendor, valueMember, resolvedShape, propertyName);
            var componentValue = ValueProducer.Read(ComponentSource.Of(componentId), valueMember, shape: resolvedShape);
            Fields.Add(GatherField.Of(propertyName, componentValue));
            return this;
        }

        /// <summary>
        /// Returns true if IncludeAll() was called.
        /// Used at build time to expand to all registered components.
        /// </summary>
        internal bool IsIncludeAll => _includeAll;
    }

    internal static class GatherValidation
    {
        private static readonly System.Text.RegularExpressions.Regex RouteParamNamePattern =
            new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9_]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        internal static bool IsValidRouteParamName(string name) => RouteParamNamePattern.IsMatch(name);
    }
}
