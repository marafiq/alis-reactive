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

        internal GatherBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        public GatherBuilder<TModel> AddField(GatherField field)
        {
            Fields.Add(field);
            return this;
        }

        public GatherBuilder<TModel> IncludeAll()
        {
            // At render time, PlanBuildContext resolves all registered input components
            // and expands this into explicit GatherFields. Marker for now.
            Fields.Add(GatherField.Of("*", "*"));
            return this;
        }

        public GatherBuilder<TModel> Static(string param, object value)
        {
            // Static values become literal fields in the gather
            Fields.Add(GatherField.Of("$static:" + param, param));
            return this;
        }

        public GatherBuilder<TModel> FromEvent<TArgs, TProp>(
            TArgs args,
            Expression<Func<TArgs, TProp>> path,
            string param)
        {
            var eventPath = ExpressionPathHelper.ToEventPath(path);
            Fields.Add(GatherField.Of("$event:" + eventPath, param));
            return this;
        }

        /// <summary>
        /// Includes a specific component's value in the gather.
        /// Used by vendor extension methods (Fusion, Native).
        /// </summary>
        public GatherBuilder<TModel> Include(string componentId, string vendor, string propertyName, string valueMember)
        {
            _context.EnsureInputComponent(componentId, vendor, valueMember, Shape.Any);
            Fields.Add(GatherField.Of(componentId, propertyName));
            return this;
        }
    }
}
