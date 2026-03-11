using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Builders.Requests
{
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
        /// Gathers a native HTML input bound to a model property.
        /// Assumes vendor="native", readExpr="value".
        /// </summary>
        public GatherBuilder<TModel> Include(Expression<Func<TModel, object?>> expr)
        {
            var componentId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            Items.Add(new ComponentGather(componentId, "native", propertyName, "value"));
            return this;
        }

        /// <summary>
        /// Gathers all registered components. Expanded at render time into explicit
        /// ComponentGather items from the plan's component registry.
        /// </summary>
        public GatherBuilder<TModel> IncludeAll()
        {
            Items.Add(new AllGather());
            return this;
        }

        /// <summary>
        /// Adds a static key/value pair to the request.
        /// </summary>
        public GatherBuilder<TModel> Static(string param, object value)
        {
            Items.Add(new StaticGather(param, value));
            return this;
        }
    }
}
