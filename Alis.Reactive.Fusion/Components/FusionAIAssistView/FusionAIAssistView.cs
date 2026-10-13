using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.InteractiveChat;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion AIAssistView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionAIAssistView(plan, "care-ai", b =&gt; ...)</c>, then reach it in a callback
    /// and use its members directly:
    /// <code>
    /// var assist = p.Component&lt;FusionAIAssistView&gt;("care-ai");
    /// assist.ExecutePrompt("Summarize the medication changes");
    /// </code>
    /// </remarks>
    public sealed class FusionAIAssistView : FusionComponent
    {
        /// <summary>The prompt text in the input.</summary>
        public Prop<string> Prompt => Prop<string>();

        /// <summary>The zero-based index of the shown view.</summary>
        public Prop<int> ActiveView => Prop<int>();

        /// <summary>Sends a prompt as if the user typed it.</summary>
        /// <param name="prompt">The prompt text to send; it must not be empty.</param>
        public void ExecutePrompt(Arg<string> prompt) => Call(prompt);

        /// <summary>Adds response text to the latest prompt; the second argument says whether it is the final update.</summary>
        /// <param name="outputResponse">The response text for the latest prompt.</param>
        /// <param name="isFinalUpdate"><see langword="true"/> when this is the last update, which hides the stop-responding button.</param>
        public void AddPromptResponse(Arg<string> outputResponse, Arg<bool> isFinalUpdate) => Call(outputResponse, isFinalUpdate);

        /// <summary>Adds a prompt with its response; the second argument says whether it is the final update.</summary>
        /// <param name="outputResponse">The prompt and its response; when the prompt is the one being answered it fills that response, otherwise both are added to the conversation.</param>
        /// <param name="isFinalUpdate"><see langword="true"/> when this is the last update, which hides the stop-responding button.</param>
        public void AddPromptResponseModel(Arg<FusionAIAssistViewPromptModel> outputResponse, Arg<bool> isFinalUpdate) => Call(outputResponse, isFinalUpdate, name: "addPromptResponse");

        /// <summary>Scrolls the conversation to the latest response.</summary>
        public void ScrollToBottom() => Call();

        /// <summary>Raised when the user sends a prompt.</summary>
        public TypedEvent<FusionAIAssistViewPromptRequestArgs> PromptRequest =>
            Event<FusionAIAssistViewPromptRequestArgs>();

        /// <summary>Raised when the prompt text changes.</summary>
        public TypedEvent<FusionAIAssistViewPromptChangedArgs> PromptChanged =>
            Event<FusionAIAssistViewPromptChangedArgs>();

        /// <summary>Raised when the user stops a response.</summary>
        public TypedEvent<FusionAIAssistViewStopRespondingClickArgs> StopRespondingClick =>
            Event<FusionAIAssistViewStopRespondingClickArgs>();
    }

    /// <summary>A prompt with its response, for <see cref="FusionAIAssistView.AddPromptResponseModel"/>.</summary>
    public class FusionAIAssistViewPromptModel
    {
        /// <summary>The prompt text.</summary>
        public string Prompt { get; set; } = "";

        /// <summary>The response text.</summary>
        public string Response { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionAIAssistView.PromptRequest"/>.</summary>
    public class FusionAIAssistViewPromptRequestArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The prompt the user sent.</summary>
        public ReadOnly<string> Prompt => ReadOnly<string>();

        /// <summary>Suggestions offered for the next prompt.</summary>
        public ReadOnly<string[]> PromptSuggestions => ReadOnly<string[]>();
    }

    /// <summary>Payload of <see cref="FusionAIAssistView.PromptChanged"/>.</summary>
    public class FusionAIAssistViewPromptChangedArgs : JsObject
    {
        /// <summary>The prompt text after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>The prompt text before the change.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionAIAssistView.StopRespondingClick"/>.</summary>
    public class FusionAIAssistViewStopRespondingClickArgs : JsObject
    {
        /// <summary>The prompt whose response stopped.</summary>
        public ReadOnly<string> Prompt => ReadOnly<string>();

        /// <summary>Index of the prompt in the conversation.</summary>
        public ReadOnly<int> DataIndex => ReadOnly<int>();
    }
#nullable restore annotations

    /// <summary>
    /// Factory extension for rendering Syncfusion AIAssistView with reactive event support.
    /// Static component configuration stays on Syncfusion's MVC builder.
    /// </summary>
    public static class FusionAIAssistViewHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion AIAssistView and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan receiving AI assist view event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionAIAssistView, TModel> FusionAIAssistView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<AIAssistViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().AIAssistView(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionAIAssistView>(elementId, builder.Render());
        }
    }
}
