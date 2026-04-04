using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a user types in a <see cref="FusionAutoComplete"/> to filter suggestions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Text).Contains("asp")</c>.
    /// </para>
    /// <para>
    /// For server-side filtering, call <see cref="FusionAutoCompleteFilteringArgsExtensions.PreventDefault"/>
    /// to suppress the default client-side filter, then call <c>UpdateData(...)</c> to feed
    /// server results into the popup.
    /// </para>
    /// </remarks>
    public class FusionAutoCompleteFilteringArgs
    {
        /// <summary>Gets or sets the search text the user typed.</summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the component event surface.
        /// </summary>
        internal FusionAutoCompleteFilteringArgs() { }
    }

    /// <summary>
    /// Typed mutations on the filtering event args for <see cref="FusionAutoComplete"/>.
    /// </summary>
    /// <remarks>
    /// These extensions modify the filtering event object in the browser (e.g. suppressing
    /// client-side filtering or feeding server results). The pipeline parameter is required
    /// because args does not carry pipeline context. Pass the current <c>p</c> or <c>s</c>.
    /// </remarks>
    public static class FusionAutoCompleteFilteringArgsExtensions
    {
        private static readonly CapabilityProperty PreventDefaultProperty =
            CapabilityProperty.FromSegments("preventDefault", new[] { PathSegment.FromProp("preventDefaultAction") });
        private static readonly CapabilityMethod UpdateDataMethod = CapabilityMethod.Named("updateData");

        /// <summary>
        /// Suppresses the default client-side filtering so only server results appear.
        /// </summary>
        /// <remarks>
        /// Without this, the component briefly shows "No records found" while the
        /// server request is in flight. Call before issuing an HTTP request.
        /// </remarks>
        /// <param name="args">The filtering event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        public static void PreventDefault<TModel>(
            this FusionAutoCompleteFilteringArgs args,
            PipelineBuilder<TModel> pipeline)
            where TModel : class
        {
            pipeline.SetEventProperty(PreventDefaultProperty, true);
        }

        /// <summary>
        /// Feeds server-filtered data into the dropdown popup from an HTTP response.
        /// </summary>
        /// <remarks>
        /// This is the only correct approach for async server-side filtering. Setting the
        /// data source directly does not work because the popup rendering lifecycle must
        /// be re-entered via <c>updateData()</c>.
        /// </remarks>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TResponse">The HTTP response body type.</typeparam>
        /// <param name="args">The filtering event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        public static void UpdateData<TModel, TResponse>(
            this FusionAutoCompleteFilteringArgs args,
            PipelineBuilder<TModel> pipeline,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var payload = pipeline.Authoring.Values.DescribeResponsePayload(path);
            pipeline.CallEventMember(
                UpdateDataMethod,
                new[] { payload.Expression },
                new[] { payload.Shape });
        }
    }
}
