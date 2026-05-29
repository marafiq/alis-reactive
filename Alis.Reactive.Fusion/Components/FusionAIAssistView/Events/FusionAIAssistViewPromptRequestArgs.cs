using System;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for AIAssistView promptRequest.
    /// </summary>
    public sealed class FusionAIAssistViewPromptRequestArgs
    {
        public bool Cancel { get; set; }
        public string Prompt { get; set; } = "";
        public string[] PromptSuggestions { get; set; } = Array.Empty<string>();
    }

    public static class FusionAIAssistViewPromptRequestArgsExtensions
    {
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
