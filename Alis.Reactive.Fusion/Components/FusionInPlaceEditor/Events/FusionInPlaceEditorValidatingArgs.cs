using System.Collections.Generic;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered while validating a <see cref="FusionInPlaceEditor"/> commit.</summary>
    /// <remarks>
    /// Fires only when the builder configures <c>validationRules</c>. SF populates
    /// <see cref="ErrorMessage"/> with the localized default message; setting it in a handler overwrites
    /// what SF renders in the <c>.e-editable-error</c> slot. Setting <see cref="Cancel"/> via
    /// <see cref="FusionInPlaceEditorValidatingArgsExtensions.PreventDefault"/> keeps the editor open
    /// with the error visible.
    /// </remarks>
    public class FusionInPlaceEditorValidatingArgs
    {
        /// <summary>The payload prepared for submission (shape: <c>{ name, primaryKey, value }</c>).</summary>
        public IDictionary<string, object>? Data { get; set; }

        /// <summary>Whether the commit is cancelled.</summary>
        public bool Cancel { get; set; }

        /// <summary>Inline validation error message Syncfusion renders below the inner editor.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>The SF event name.</summary>
        public string? Name { get; set; }

        /// <summary>Creates a new instance. Framework-internal: instances are created by the event descriptor.</summary>
        public FusionInPlaceEditorValidatingArgs() { }
    }

    /// <summary>Typed mutations on the validating event args for <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorValidatingArgsExtensions
    {
        /// <summary>Cancels the commit so the editor stays open for the user to retry.</summary>
        /// <param name="args">The validating event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorValidatingArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(Reaction.Set(PayloadSource.Event(), "cancel", ValueProducer.Literal(true)));
        }

        /// <summary>
        /// Writes <c>args.errorMessage</c> so Syncfusion renders the text in its native
        /// <c>.e-editable-error</c> slot below the inner editor.
        /// </summary>
        /// <remarks>
        /// Use together with <see cref="PreventDefault"/> to both block the commit and show a custom
        /// message. The editor stays open, the inline error renders in SF's native slot.
        /// </remarks>
        /// <param name="args">The validating event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        /// <param name="message">The error text to render.</param>
        public static void SetErrorMessage(
            this FusionInPlaceEditorValidatingArgs args,
            IReactionEmitter pipeline,
            string message)
        {
            pipeline.AddStep(Reaction.Set(PayloadSource.Event(), "errorMessage", ValueProducer.Literal(message)));
        }
    }
}
