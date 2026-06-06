using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries a submitted prompt before the AIAssistView request is sent.
    /// </summary>
    public sealed class FusionAIAssistViewPromptRequestArgs
    {
        /// <summary>Whether the request should be canceled.</summary>
        public bool Cancel { get; set; }

        /// <summary>Submitted prompt text.</summary>
        public string Prompt { get; set; } = "";

        /// <summary>Prompt suggestions available when the request was submitted.</summary>
        public string[] PromptSuggestions { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// Reactive Plan helpers for mutating an AIAssistView prompt request payload.
    /// </summary>
    public static class FusionAIAssistViewPromptRequestArgsExtensions
    {
        /// <summary>
        /// Cancels the pending AIAssistView request by setting the event payload's cancel flag.
        /// </summary>
        /// <param name="args">Prompt request payload selected by the event pipeline.</param>
        /// <param name="pipeline">Reaction pipeline receiving the payload mutation.</param>
        public static void CancelRequest(
            this FusionAIAssistViewPromptRequestArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }
    }
}
