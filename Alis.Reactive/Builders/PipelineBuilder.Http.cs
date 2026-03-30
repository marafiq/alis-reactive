using System;
using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        // ── HTTP Request Methods ──

        /// <summary>Starts a GET request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Get(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("GET").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts a POST request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Post(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("POST").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts a POST request with a gather configuration.</summary>
        /// <remarks>
        /// <code>
        /// p.Post("/api/save", gather: g =&gt; g.IncludeAll())
        ///  .Response(response: r =&gt; r.OnSuccess(pipeline: s =&gt; s.Into("result")));
        /// </code>
        /// </remarks>
        public HttpRequestBuilder<TModel> Post(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("POST").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts a PUT request with a gather configuration.</summary>
        public HttpRequestBuilder<TModel> Put(string url, Action<GatherBuilder<TModel>> gather)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("PUT").SetUrl(url);
            _httpBuilder.Gather(gather);
            return _httpBuilder;
        }

        /// <summary>Starts a DELETE request to the given URL.</summary>
        public HttpRequestBuilder<TModel> Delete(string url)
        {
            SetMode(PipelineMode.Http);
            _httpBuilder = new HttpRequestBuilder<TModel>();
            _httpBuilder.SetVerb("DELETE").SetUrl(url);
            return _httpBuilder;
        }

        /// <summary>Starts parallel HTTP requests that fire concurrently.</summary>
        public ParallelBuilder<TModel> Parallel(params Action<HttpRequestBuilder<TModel>>[] branches)
        {
            SetMode(PipelineMode.Parallel);
            _parallelBuilder = new ParallelBuilder<TModel>();
            foreach (var branch in branches)
            {
                _parallelBuilder.AddBranch(branch);
            }
            return _parallelBuilder;
        }

        private void SetMode(PipelineMode mode)
        {
            if (_mode == mode || _mode == PipelineMode.Sequential)
            {
                _mode = mode;
                return;
            }

            // Transitioning between non-Sequential modes — flush current segment first
            FlushSegment();
            _mode = mode;
        }
    }
}
