using System;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Appends an async HTTP GET reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>A request builder for route values, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Get(string url)
        {
            return _draft.BeginHttp(Context).Get(url);
        }

        /// <summary>Appends an async HTTP POST reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>A request builder for gathered input, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            return _draft.BeginHttp(Context).Post(url);
        }

        /// <summary>Appends an async HTTP POST reaction and configures gathered input inline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <param name="gather">Collects request body, header, or route-template values before the request is sent.</param>
        /// <returns>An HTTP request builder for configuring validation and response routing.</returns>
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
        {
            return _draft.BeginHttp(Context).Post(url).Gather(gather);
        }

        /// <summary>Appends an async HTTP PUT reaction and configures gathered input inline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <param name="gather">Collects request body, header, or route-template values before the request is sent.</param>
        /// <returns>An HTTP request builder for configuring validation and response routing.</returns>
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
        {
            return _draft.BeginHttp(Context).Put(url).Gather(gather);
        }

        /// <summary>Appends an async HTTP DELETE reaction to this Reactive Plan pipeline.</summary>
        /// <param name="url">Request URL, which may contain <c>{placeholder}</c> template parameters.</param>
        /// <returns>A request builder for route values, validation, loading/finally reactions, and response routing.</returns>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            return _draft.BeginHttp(Context).Delete(url);
        }

        /// <summary>Appends an async reaction whose HTTP request branches execute concurrently.</summary>
        /// <param name="branches">Request branches to build before they run in parallel.</param>
        /// <returns>A parallel request builder for configuring the optional all-settled reaction.</returns>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            var builder = _draft.BeginParallel(Context);
            foreach (var branch in branches)
                builder.AddBranch(branch);
            return builder;
        }
    }
}
