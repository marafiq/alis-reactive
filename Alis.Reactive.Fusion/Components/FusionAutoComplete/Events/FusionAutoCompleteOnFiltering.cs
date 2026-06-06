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
    /// For server-side filtering, call <see cref="FusionAutoCompleteFilteringArgsExtensions.PreventDefault"/>
    /// to suppress the default client-side filter, then use
    /// <see cref="FusionAutoCompleteFilteringArgsExtensions.UpdateData{TResponse}"/> to feed
    /// server results into the popup.
    /// </remarks>
    public class FusionAutoCompleteFilteringArgs
    {
        /// <summary>Search text the user typed.</summary>
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// Typed event-payload operations for the filtering event args of <see cref="FusionAutoComplete"/>.
    /// </summary>
    /// <remarks>
    /// These extensions mutate Syncfusion's filtering event object, such as suppressing
    /// client-side filtering or feeding server results. The pipeline parameter is required
    /// because args does not carry pipeline context. Pass the current <c>p</c> or <c>s</c>.
    /// </remarks>
    public static class FusionAutoCompleteFilteringArgsExtensions
    {
        /// <summary>
        /// Suppresses the default client-side filtering so only server results appear.
        /// </summary>
        /// <remarks>
        /// Without this, the component briefly shows "No records found" while the
        /// server request is in flight. Call before issuing an HTTP request.
        /// </remarks>
        public static void PreventDefault(
            this FusionAutoCompleteFilteringArgs args,
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
        public static void UpdateData<TResponse>(
            this FusionAutoCompleteFilteringArgs args,
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
