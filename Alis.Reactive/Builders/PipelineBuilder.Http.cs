using System;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts an HTTP GET request to the specified URL.</summary>
        /// <param name="url">The request URL, which may contain template placeholders.</param>
        /// <returns>An HTTP request builder for configuring gather, validation, and response handling.</returns>
        public HttpRequestBuilder<TModel> Get(string url)
        {
            return _draft.BeginHttp(Context).Get(url);
        }

        /// <summary>Starts an HTTP POST request to the specified URL.</summary>
        /// <param name="url">The request URL, which may contain template placeholders.</param>
        /// <returns>An HTTP request builder for configuring gather, validation, and response handling.</returns>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            return _draft.BeginHttp(Context).Post(url);
        }

        /// <summary>Starts an HTTP POST with inline gather configuration.</summary>
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
        {
            return _draft.BeginHttp(Context).Post(url).Gather(gather);
        }

        /// <summary>Starts an HTTP PUT with inline gather configuration.</summary>
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
        {
            return _draft.BeginHttp(Context).Put(url).Gather(gather);
        }

        /// <summary>Starts an HTTP DELETE request to the specified URL.</summary>
        /// <param name="url">The request URL, which may contain template placeholders.</param>
        /// <returns>An HTTP request builder for configuring gather, validation, and response handling.</returns>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            return _draft.BeginHttp(Context).Delete(url);
        }

        /// <summary>Executes multiple HTTP requests concurrently.</summary>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            var builder = _draft.BeginParallel(Context);
            foreach (var branch in branches)
                builder.AddBranch(branch);
            return builder;
        }
    }
}
