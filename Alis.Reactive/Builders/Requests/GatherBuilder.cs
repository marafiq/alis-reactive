using System.Collections.Generic;
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
        /// Gathers all fields from a form via FormData.
        /// </summary>
        public GatherBuilder<TModel> IncludeAll(string formId)
        {
            Items.Add(new AllGather(formId));
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
