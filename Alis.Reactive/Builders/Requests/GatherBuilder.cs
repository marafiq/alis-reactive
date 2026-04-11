using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    public class GatherBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        internal List<GatherField> Fields { get; } = new List<GatherField>();
        internal List<StaticField> StaticFields { get; } = new List<StaticField>();
        internal List<EventField> EventFields { get; } = new List<EventField>();
        internal Dictionary<string, ValueProducer> HeaderFields { get; } = new Dictionary<string, ValueProducer>();
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

        public GatherBuilder<TModel> IncludeAll()
        {
            _includeAll = true;
            return this;
        }

        public GatherBuilder<TModel> Static(string param, object value)
        {
            StaticFields.Add(new StaticField(param, value));
            return this;
        }

        public GatherBuilder<TModel> FromEvent<TArgs, TProp>(
            TArgs args,
            Expression<Func<TArgs, TProp>> path,
            string param)
        {
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            EventFields.Add(new EventField(param, eventPath));
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

        /// <summary>
        /// Includes a specific component's value in the gather.
        /// Used by vendor extension methods (Fusion, Native).
        /// </summary>
        public GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember)
        {
            Shape shape = null;
            if (_context.TryFindRegistrationById(componentId, out var reg))
                shape = reg.Shape;
            _context.EnsureInputComponent(componentId, vendor, valueMember, shape ?? Shape.Any, propertyName);
            var value = ValueProducer.Read(ComponentSource.Of(componentId), valueMember, shape: shape ?? Shape.Any);
            Fields.Add(GatherField.Of(propertyName, value));
            return this;
        }

        /// <summary>
        /// Returns true if IncludeAll() was called.
        /// Used at build time to expand to all registered components.
        /// </summary>
        internal bool IsIncludeAll => _includeAll;
    }
}
