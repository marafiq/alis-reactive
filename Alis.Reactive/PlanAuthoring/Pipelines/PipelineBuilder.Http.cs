using System;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Appends an async HTTP GET reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>Request builder for route values, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Get(string url)
        {
            return Request().Get(url);
        }

        /// <summary>Appends an async HTTP POST reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>Request builder for gathered input, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            return Request().Post(url);
        }

        /// <summary>Appends an async HTTP PUT reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>Request builder for gathered input, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Put(string url)
        {
            return Request().Put(url);
        }

        /// <summary>Appends an async HTTP DELETE reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>Request builder for route values, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            return Request().Delete(url);
        }

        /// <summary>Appends an async reaction whose HTTP request branches execute concurrently.</summary>
        /// <param name="branches">Request branches to build before they run in parallel.</param>
        /// <returns>Parallel request builder for configuring the optional all-settled reaction.</returns>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            var builder = new ParallelBuilder<TModel>(Context);
            Context.AddStep(builder.Step);
            foreach (var branch in branches)
                builder.AddBranch(branch);
            return builder;
        }

        // A request is placed where it is written and configured until the plan is written.
        private HttpRequestBuilder<TModel> Request()
        {
            var builder = new HttpRequestBuilder<TModel>(Context);
            Context.AddStep(builder.Step);
            return builder;
        }
    }
}
