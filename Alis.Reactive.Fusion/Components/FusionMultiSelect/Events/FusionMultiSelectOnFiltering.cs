using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a user types in a <see cref="FusionMultiSelect"/> to filter suggestions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Text).Contains("pea")</c>.
    /// </para>
    /// <para>
    /// For server-side filtering, call <see cref="FusionMultiSelectFilteringArgsExtensions.PreventDefault"/>
    /// to suppress the default client-side filter, then use
    /// <see cref="FusionMultiSelectFilteringArgsExtensions.UpdateData{TResponse}"/> to feed
    /// server results into the popup.
    /// </para>
    /// </remarks>
    public class FusionMultiSelectFilteringArgs
    {
        /// <summary>Search text the user typed.</summary>
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// Typed event-payload operations for the filtering event args of <see cref="FusionMultiSelect"/>.
    /// </summary>
    /// <remarks>
    /// These extensions mutate Syncfusion's filtering event object, such as suppressing
    /// client-side filtering or feeding server results. The pipeline parameter is required
    /// because args does not carry pipeline context. Pass the current <c>p</c> or <c>s</c>.
    /// </remarks>
    public static class FusionMultiSelectFilteringArgsExtensions
    {
        /// <summary>
        /// Suppresses the default client-side filtering so only server results appear.
        /// </summary>
        /// <remarks>
        /// Without this, the component briefly shows "No records found" while the
        /// server request is in flight. Call before issuing an HTTP request.
        /// </remarks>
        /// <param name="args">The filtering event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        public static void PreventDefault(
            this FusionMultiSelectFilteringArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "preventDefaultAction", ValueExpression.Literal(true)));
        }

        /// <summary>
        /// Feeds server-filtered data into the dropdown popup from an HTTP response.
        /// </summary>
        /// <remarks>
        /// For async server-side filtering, route returned items through this method.
        /// Assigning the data source directly does not re-enter Syncfusion's popup
        /// rendering lifecycle.
        /// </remarks>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <param name="args">The filtering event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        public static void UpdateData<TResponse>(
            this FusionMultiSelectFilteringArgs args,
            IReactionEmitter pipeline,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            pipeline.AddStep(ReactionGraph.Call(PayloadSource.Event(), "updateData",
                new System.Collections.Generic.List<ValueExpression> { ValueExpression.Read(source.Scope, sourcePath) }));
        }
    }
}
