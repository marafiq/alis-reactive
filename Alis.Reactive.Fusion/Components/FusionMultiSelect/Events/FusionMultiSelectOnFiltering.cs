using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionMultiSelect.Filtering (SF "filtering" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Text).Contains("pea")
    /// ExpressionPathHelper resolves x => x.Text to "evt.text".
    ///
    /// SF FilteringEventArgs also exposes methods on the event callback parameter:
    ///   e.preventDefaultAction (set-prop) — suppresses SF's internal client-side filter
    ///   e.updateData(data) (call) — feeds server-filtered data into the popup
    /// These are exposed as typed extensions below via MutateEventCommand.
    ///
    /// Verified: SF MultiSelect uses the same FilteringEventArgs as AutoComplete.
    /// The filtering event fires on each keystroke when AllowFiltering(true) is set.
    /// preventDefaultAction and updateData work identically to AutoComplete.
    /// </summary>
    public class FusionMultiSelectFilteringArgs
    {
        /// <summary>The search text the user typed.</summary>
        public string Text { get; set; } = "";

        public FusionMultiSelectFilteringArgs() { }
    }

    /// <summary>
    /// Typed extensions for FusionMultiSelectFilteringArgs.
    /// These emit MutateEventCommand (set-prop or call on ctx.evt).
    /// Only available when the event args are filtering args — compile-time correct.
    ///
    /// The pipeline parameter is required because args is a phantom type marker
    /// shared across the entire reactive lambda — it doesn't carry pipeline context.
    /// Unlike ComponentRef (created per-context via p.Component/s.Component),
    /// args comes from the outer scope. Passing the pipeline builder is necessary.
    /// </summary>
    public static class FusionMultiSelectFilteringArgsExtensions
    {
        /// <summary>
        /// Sets e.preventDefaultAction = true on the filtering event args.
        /// Suppresses SF's internal client-side filtering so only server results appear.
        /// Without this, SF shows "No records found" flash while the async HTTP is in-flight.
        /// Usage: args.PreventDefault(p)
        /// </summary>
        public static void PreventDefault<TModel>(
            this FusionMultiSelectFilteringArgs args,
            PipelineBuilder<TModel> pipeline)
            where TModel : class
        {
            pipeline.AddCommand(new MutateEventCommand(
                new SetPropMutation("preventDefaultAction"), value: true));
        }

        /// <summary>
        /// Calls SF's updateData() on the filtering event args with data from the response body.
        /// This is the correct SF API for server-side filtering — sets the DataSource
        /// and renders the popup in one call. Experiment-verified on AutoComplete;
        /// MultiSelect uses the same FilteringEventArgs.
        /// Usage: args.UpdateData(s, json, j => j.Items)
        /// </summary>
        public static void UpdateData<TModel, TResponse>(
            this FusionMultiSelectFilteringArgs args,
            PipelineBuilder<TModel> pipeline,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            pipeline.AddCommand(new MutateEventCommand(
                new CallMutation("updateData", args: new MethodArg[]
                {
                    new SourceArg(new EventSource(sourcePath))
                })));
        }
    }
}
