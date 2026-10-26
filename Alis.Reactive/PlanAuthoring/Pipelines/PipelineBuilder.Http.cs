using System;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Sends a GET request from this step; gathered values go to the query string.</summary>
        /// <param name="url">The URL, built with <c>Url.Action</c> or a route template.</param>
        public HttpRequestBuilder<TModel> Get(string url) => Request().Get(url);

        /// <summary>Sends a POST request from this step; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Post(string url) => Request().Post(url);

        /// <summary>Sends a PUT request from this step; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Put(string url) => Request().Put(url);

        /// <summary>Sends a DELETE request from this step; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Delete(string url) => Request().Delete(url);

        /// <summary>
        /// Sends several requests at once, each branch starting with its method:
        /// <c>p.Parallel(a =&gt; a.Get(url1).Response(..), b =&gt; b.Post(url2).Response(..)).OnAllSettled(s =&gt; ..)</c>.
        /// </summary>
        /// <param name="branches">The requests, each started from its branch.</param>
        public ParallelBuilder<TModel> Parallel(params Func<RequestStart<TModel>, HttpRequestBuilder<TModel>>[] branches)
        {
            if (branches == null) throw new ArgumentNullException(nameof(branches));
            var builder = new ParallelBuilder<TModel>(Context);
            Context.AddStep(builder.Step);
            foreach (var branch in branches)
                builder.AddBranch(branch);
            return builder;
        }

        // A request is placed where it is written and configured until the plan is written.
        private RequestStart<TModel> Request() => new RequestStart<TModel>(Context, Context.AddStep);
    }
}
