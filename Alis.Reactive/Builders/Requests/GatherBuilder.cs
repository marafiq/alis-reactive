using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    public class GatherBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        internal List<GatherField> Fields { get; } = new List<GatherField>();
        internal List<StaticField> StaticFields { get; } = new List<StaticField>();
        internal List<EventField> EventFields { get; } = new List<EventField>();
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
            Fields.Add(GatherField.Of(componentId, propertyName, shape));
            return this;
        }

        /// <summary>
        /// Returns true if IncludeAll() was called.
        /// Used at build time to expand to all registered components.
        /// </summary>
        internal bool IsIncludeAll => _includeAll;
    }
}
