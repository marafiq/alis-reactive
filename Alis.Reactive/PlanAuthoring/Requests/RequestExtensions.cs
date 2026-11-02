using System;

namespace Alis.Reactive
{
    /// <summary>
    /// The requests a pipeline sends. Each extends <see cref="PipelineBuilder{TModel}"/>, so the pipeline
    /// knows nothing of requests and the request builders depend on it.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>Sends a GET request from this step; gathered values go to the query string.</summary>
        /// <param name="pipeline">The pipeline the request is written in.</param>
        /// <param name="url">The URL, built with <c>Url.Action</c> or a route template.</param>
        public static HttpRequestBuilder<TModel> Get<TModel>(this PipelineBuilder<TModel> pipeline, string url) where TModel : class =>
            Request(pipeline).Get(url);

        /// <summary>Sends a POST request from this step; gathered values go to the body.</summary>
        /// <param name="pipeline">The pipeline the request is written in.</param>
        /// <param name="url">The URL.</param>
        public static HttpRequestBuilder<TModel> Post<TModel>(this PipelineBuilder<TModel> pipeline, string url) where TModel : class =>
            Request(pipeline).Post(url);

        /// <summary>Sends a PUT request from this step; gathered values go to the body.</summary>
        /// <param name="pipeline">The pipeline the request is written in.</param>
        /// <param name="url">The URL.</param>
        public static HttpRequestBuilder<TModel> Put<TModel>(this PipelineBuilder<TModel> pipeline, string url) where TModel : class =>
            Request(pipeline).Put(url);

        /// <summary>Sends a DELETE request from this step; gathered values go to the body.</summary>
        /// <param name="pipeline">The pipeline the request is written in.</param>
        /// <param name="url">The URL.</param>
        public static HttpRequestBuilder<TModel> Delete<TModel>(this PipelineBuilder<TModel> pipeline, string url) where TModel : class =>
            Request(pipeline).Delete(url);

        /// <summary>
        /// Sends several requests at once, each branch starting with its method:
        /// <c>p.Parallel(a =&gt; a.Get(url1).Response(..), b =&gt; b.Post(url2).Response(..)).OnAllSettled(s =&gt; ..)</c>.
        /// </summary>
        /// <param name="pipeline">The pipeline the requests are written in.</param>
        /// <param name="first">The first request, started from its branch.</param>
        /// <param name="more">The other requests, each started from its branch.</param>
        public static ParallelBuilder<TModel> Parallel<TModel>(
            this PipelineBuilder<TModel> pipeline,
            Func<RequestStart<TModel>, HttpRequestBuilder<TModel>> first,
            params Func<RequestStart<TModel>, HttpRequestBuilder<TModel>>[] more) where TModel : class
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (more == null) throw new ArgumentNullException(nameof(more));
            var builder = new ParallelBuilder<TModel>(pipeline.Context);
            pipeline.Context.AddStep(builder.Step);
            builder.AddBranch(first);
            foreach (var branch in more)
                builder.AddBranch(branch);
            return builder;
        }

        // A request is placed where it is written and configured until the plan is written.
        private static RequestStart<TModel> Request<TModel>(PipelineBuilder<TModel> pipeline) where TModel : class
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return new RequestStart<TModel>(pipeline.Context, pipeline.Context.AddStep);
        }
    }
}
