using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a <see cref="FusionInPlaceEditor"/> commits its value.
    /// </summary>
    /// <remarks>
    /// Syncfusion fires <c>actionBegin</c> immediately before issuing its built-in <c>UrlAdaptor</c>
    /// POST. Call <see cref="FusionInPlaceEditorActionBeginArgsExtensions.PreventDefault"/> to suppress
    /// that default submit so the Reactive Plan pipeline owns the commit flow.
    /// </remarks>
    public class FusionInPlaceEditorActionBeginArgs
    {
        /// <summary>The payload Syncfusion prepared for its built-in submit, keyed by the editor's <c>Name</c>.</summary>
        public IDictionary<string, object>? Data { get; set; }

        /// <summary>Whether Syncfusion's built-in submit has been cancelled. Set via <see cref="FusionInPlaceEditorActionBeginArgsExtensions.PreventDefault"/>.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Typed event-payload operations for the actionBegin event args of <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorActionBeginArgsExtensions
    {
        /// <summary>
        /// Cancels Syncfusion's built-in commit so the reactive HTTP pipeline owns the submit.
        /// </summary>
        /// <remarks>
        /// Call before issuing <c>p.Post(...)...</c>. Without this, the editor performs its configured
        /// <c>UrlAdaptor</c> POST alongside the Reactive Plan pipeline, duplicating the request.
        /// </remarks>
        /// <param name="pipeline">The pipeline that receives the event-arg cancellation mutation.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorActionBeginArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
