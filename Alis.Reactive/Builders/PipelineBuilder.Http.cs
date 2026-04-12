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
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>(Context);
            _httpBuilder.SetVerb("GET").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts an HTTP POST request to the specified URL.</summary>
        /// <param name="url">The request URL, which may contain template placeholders.</param>
        /// <returns>An HTTP request builder for configuring gather, validation, and response handling.</returns>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>(Context);
            _httpBuilder.SetVerb("POST").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts an HTTP POST with inline gather configuration.</summary>
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>(Context);
            _httpBuilder.SetVerb("POST").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts an HTTP PUT with inline gather configuration.</summary>
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>(Context);
            _httpBuilder.SetVerb("PUT").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts an HTTP DELETE request to the specified URL.</summary>
        /// <param name="url">The request URL, which may contain template placeholders.</param>
        /// <returns>An HTTP request builder for configuring gather, validation, and response handling.</returns>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>(Context);
            _httpBuilder.SetVerb("DELETE").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Executes multiple HTTP requests concurrently.</summary>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            SetMode(PipelineMode.Parallel);
            _parallelBuilder = new ParallelBuilder<TModel>(Context);
            foreach (var branch in branches)
                _parallelBuilder.AddBranch(branch);
            return _parallelBuilder;
        }

        private void SetMode(PipelineMode mode)
        {
            var isFirstNonSequentialSegment = _mode == PipelineMode.Sequential;
            if (isFirstNonSequentialSegment)
            {
                _mode = mode;
                return;
            }

            FlushSegment();
            _mode = mode;
        }
    }
}
