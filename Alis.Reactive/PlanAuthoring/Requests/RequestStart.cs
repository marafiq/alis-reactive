using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>
    /// Where a request starts: its method and URL, after which it is configured with gather, response
    /// routes, loading and cleanup. A pipeline starts one with <c>p.Get(url)</c>; a branch of
    /// <c>p.Parallel(..)</c> and a <c>Chained</c> request start theirs the same way.
    /// </summary>
    /// <typeparam name="TModel">The view model.</typeparam>
    public sealed class RequestStart<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly Action<RequestStep> _place;

        internal RequestStart(PlanBuildContext context, Action<RequestStep> place)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _place = place ?? throw new ArgumentNullException(nameof(place));
        }

        /// <summary>A GET request; gathered values go to the query string.</summary>
        /// <param name="url">The URL, built with <c>Url.Action</c> or a route template.</param>
        public HttpRequestBuilder<TModel> Get(string url) => Start("GET", url);

        /// <summary>A POST request; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Post(string url) => Start("POST", url);

        /// <summary>A PUT request; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Put(string url) => Start("PUT", url);

        /// <summary>A DELETE request; gathered values go to the body.</summary>
        /// <param name="url">The URL.</param>
        public HttpRequestBuilder<TModel> Delete(string url) => Start("DELETE", url);

        private HttpRequestBuilder<TModel> Start(string method, string url)
        {
            var step = new RequestStep(method, url);
            _place(step);
            return new HttpRequestBuilder<TModel>(_context, step);
        }
    }
}
