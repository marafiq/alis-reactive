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
    /// <summary>
    /// The Syncfusion AIAssistView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionAIAssistView(plan, "care-ai", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionAIAssistView&gt;("care-ai")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Call(c =&gt; c.ExecutePrompt, "Summarize the medication changes")</c>.
    /// </remarks>
    public sealed class FusionAIAssistView : FusionComponent
    {
        /// <summary>The prompt text in the input.</summary>
        public ComponentProperty<string> Prompt => Property<string>("prompt");

        /// <summary>The zero-based index of the shown view.</summary>
        public ComponentProperty<int> ActiveView => Property<int>("activeView");

        /// <summary>Sends a prompt as if the user typed it.</summary>
        public ComponentMethod<string> ExecutePrompt => Method<string>("executePrompt");

        /// <summary>Adds response text to the latest prompt; the second argument says whether it is the final update.</summary>
        public ComponentMethod<string, bool> AddPromptResponse => Method<string, bool>("addPromptResponse");

        /// <summary>Adds a prompt with its response; the second argument says whether it is the final update.</summary>
        public ComponentMethod<FusionAIAssistViewPromptModel, bool> AddPromptResponseModel => Method<FusionAIAssistViewPromptModel, bool>("addPromptResponse");

        /// <summary>Scrolls the conversation to the latest response.</summary>
        public ComponentMethod ScrollToBottom => Method("scrollToBottom");

        /// <summary>Raised when the user sends a prompt.</summary>
        public TypedEvent<FusionAIAssistViewPromptRequestArgs> PromptRequest =>
            Event<FusionAIAssistViewPromptRequestArgs>("promptRequest");

        /// <summary>Raised when the prompt text changes.</summary>
        public TypedEvent<FusionAIAssistViewPromptChangedArgs> PromptChanged =>
            Event<FusionAIAssistViewPromptChangedArgs>("promptChanged");

        /// <summary>Raised when the user stops a response.</summary>
        public TypedEvent<FusionAIAssistViewStopRespondingClickArgs> StopRespondingClick =>
            Event<FusionAIAssistViewStopRespondingClickArgs>("stopRespondingClick");
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
    public class FusionAIAssistViewPromptRequestArgs
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>The prompt the user sent.</summary>
        public string Prompt { get; set; } = "";

        /// <summary>Suggestions offered for the next prompt.</summary>
        public string[] PromptSuggestions { get; set; } = Array.Empty<string>();
    }

    /// <summary>Payload of <see cref="FusionAIAssistView.PromptChanged"/>.</summary>
    public class FusionAIAssistViewPromptChangedArgs
    {
        /// <summary>The prompt text after the change.</summary>
        public string Value { get; set; } = "";

        /// <summary>The prompt text before the change.</summary>
        public string PreviousValue { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionAIAssistView.StopRespondingClick"/>.</summary>
    public class FusionAIAssistViewStopRespondingClickArgs
    {
        /// <summary>The prompt whose response stopped.</summary>
        public string Prompt { get; set; } = "";

        /// <summary>Index of the prompt in the conversation.</summary>
        public int DataIndex { get; set; }
    }

    /// <summary>
    /// Factory extension for rendering Syncfusion AIAssistView with reactive event support.
    /// Static component configuration stays on Syncfusion's MVC builder.
    /// </summary>
    public static class FusionAIAssistViewHtmlExtensions
    {
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
