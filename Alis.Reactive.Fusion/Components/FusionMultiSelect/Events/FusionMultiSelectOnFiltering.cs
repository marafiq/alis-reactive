using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Mutations;
using Alis.Reactive.Descriptors.Sources;

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
        /// <summary>Gets or sets the search text the user typed.</summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionMultiSelectFilteringArgs() { }
    }

    /// <summary>
    /// Typed mutations on the filtering event args for <see cref="FusionMultiSelect"/>.
    /// </summary>
    /// <remarks>
    /// These extensions modify the filtering event object in the browser (e.g. suppressing
    /// client-side filtering or feeding server results). The pipeline parameter is required
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
            ICommandEmitter pipeline)
        {
            pipeline.AddCommand(new MutateEventCommand(
                new SetPropMutation("preventDefaultAction"), value: true));
        }

        /// <summary>
        /// Feeds server-filtered data into the dropdown popup from an HTTP response.
        /// </summary>
        /// <remarks>
        /// This is the only correct approach for async server-side filtering. Setting the
        /// data source directly does not work because the popup rendering lifecycle must
        /// be re-entered via <c>updateData()</c>.
        /// </remarks>
        /// <typeparam name="TResponse">The HTTP response body type.</typeparam>
        /// <param name="args">The filtering event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        /// <param name="source">The response body instance.</param>
        /// <param name="path">Expression selecting the items collection from the response.</param>
        public static void UpdateData<TResponse>(
            this FusionMultiSelectFilteringArgs args,
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
