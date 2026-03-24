using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionAutoComplete.Filtering (SF "filtering" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Text).Contains("asp")
    /// ExpressionPathHelper resolves x => x.Text to "evt.text".
    ///
    /// SF FilteringEventArgs also exposes methods on the event callback parameter:
    ///   e.preventDefaultAction (set-prop) — suppresses SF's internal client-side filter
    ///   e.updateData(data) (call) — feeds server-filtered data into the popup
    /// These are exposed as typed extensions below via MutateEventCommand.
    ///
    /// Verified manually:
    ///   - showSpinner/hideSpinner: no visible effect on AutoComplete (SF spinner is a standalone utility, not built into dropdown inputs)
    ///   - refresh(): causes focus loss mid-typing — not usable during filtering
    ///   - preventDefaultAction: suppresses "No records found" flash during async server fetch
    ///   - updateData(data): the ONLY correct SF API for async server-filtered data (experiment-verified)
    /// </summary>
    public class FusionAutoCompleteFilteringArgs
    {
        /// <summary>The search text the user typed.</summary>
        public string Text { get; set; } = "";

        public FusionAutoCompleteFilteringArgs() { }
    }

    /// <summary>
    /// Typed extensions for FusionAutoCompleteFilteringArgs.
    /// These emit MutateEventCommand (set-prop or call on ctx.evt).
    /// Only available when the event args are filtering args — compile-time correct.
    ///
    /// The pipeline parameter is required because args is a phantom type marker
    /// shared across the entire reactive lambda — it doesn't carry pipeline context.
    /// Unlike ComponentRef (created per-context via p.Component/s.Component),
    /// args comes from the outer scope. Passing the pipeline builder is necessary.
    /// </summary>
    public static class FusionAutoCompleteFilteringArgsExtensions
    {
        /// <summary>
        /// Sets e.preventDefaultAction = true on the filtering event args.
        /// Suppresses SF's internal client-side filtering so only server results appear.
        /// Without this, SF shows "No records found" flash while the async HTTP is in-flight.
        /// Usage: args.PreventDefault(p)
        /// </summary>
        public static void PreventDefault(
            this FusionAutoCompleteFilteringArgs args,
            ICommandEmitter pipeline)
        {
            pipeline.AddCommand(new MutateEventCommand(
                new SetPropMutation("preventDefaultAction"), value: true));
        }

        /// <summary>
        /// Calls SF's updateData() on the filtering event args with data from the response body.
        /// This is the correct SF API for server-side filtering — sets the DataSource
        /// and renders the popup in one call. Experiment-verified: dataSource alone,
        /// dataSource+dataBind, and dataSource+showPopup all fail for async filtering.
        /// Only updateData() works because it re-enters SF's popup rendering lifecycle.
        /// Usage: args.UpdateData(s, json, j => j.Medications)
        /// </summary>
        public static void UpdateData<TResponse>(
            this FusionAutoCompleteFilteringArgs args,
            ICommandEmitter pipeline,
            ResponseBody<TResponse> source,
            Expression<Func<TResponse, object?>> path)
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
