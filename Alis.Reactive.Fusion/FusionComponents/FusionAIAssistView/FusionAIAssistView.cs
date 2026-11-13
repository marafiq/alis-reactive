using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// An AI assist view: EJ2's AIAssistView, a conversation of prompts and the responses the app gives them.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="PromptRequest"/> when a person sends a prompt; a handler of it requests the answer from the
/// app's endpoint and hands it to <see cref="AddPromptResponse(Arg{string})"/>. A response is shown whole, as it
/// arrives.
/// </remarks>
[Mirrors("@syncfusion/ej2-interactive-chat", "AIAssistView")]
public sealed class FusionAIAssistView : FusionComponent
{
    /// <summary>Gets the prompt text the view's input holds.</summary>
    public Property<string> Prompt => Property<string>();

    /// <summary>Gets the zero-based index of the view shown.</summary>
    public Property<int> ActiveView => Property<int>();

    /// <summary>Sends a prompt, as a person who typed it does.</summary>
    /// <param name="prompt">The prompt's text.</param>
    public void ExecutePrompt(Arg<string> prompt) => Method([prompt]);

    /// <summary>Answers the prompt the view waits on with a whole response.</summary>
    /// <param name="outputResponse">The response's text.</param>
    public void AddPromptResponse(Arg<string> outputResponse) => Method([outputResponse]);

    /// <summary>Adds a prompt with its response; the prompt the view waits on takes the response as its answer.</summary>
    /// <param name="outputResponse">The prompt and its response.</param>
    public void AddPromptResponse(Arg<PromptModel> outputResponse) => Method([outputResponse]);

    /// <summary>Scrolls the conversation to its latest response.</summary>
    public void ScrollToBottom() => Method();

    /// <summary>Gets the event raised when a person sends a prompt, which the view shows before it waits on its answer.</summary>
    public Event<PromptRequestEventArgs> PromptRequest => Event<PromptRequestEventArgs>();

    /// <summary>Gets the event raised when the text of the view's input changes.</summary>
    public Event<PromptChangedEventArgs> PromptChanged => Event<PromptChangedEventArgs>();

    /// <summary>Gets the event raised when a person stops the response the view waits on.</summary>
    public Event<StopRespondingEventArgs> StopRespondingClick => Event<StopRespondingEventArgs>();

    /// <summary>
    /// What a listener of <see cref="PromptRequest"/> receives.
    /// </summary>
    /// <remarks>The view reads nothing back from it, so a handler declines a prompt by answering it.</remarks>
    [Mirrors("@syncfusion/ej2-interactive-chat", "PromptRequestEventArgs")]
    public sealed class PromptRequestEventArgs : JsObject
    {
        /// <summary>Gets the prompt the person sent.</summary>
        public ReadOnly<string?> Prompt => ReadOnly<string?>(optional: true);

        /// <summary>Gets the suggestions the view offers for the next prompt.</summary>
        public ReadOnly<string[]?> PromptSuggestions => ReadOnly<string[]?>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="PromptChanged"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-interactive-chat", "PromptChangedEventArgs")]
    public sealed class PromptChangedEventArgs : JsObject
    {
        /// <summary>Gets the input's text after the change.</summary>
        public ReadOnly<string?> Value => ReadOnly<string?>(optional: true);

        /// <summary>Gets the input's text before the change.</summary>
        public ReadOnly<string?> PreviousValue => ReadOnly<string?>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="StopRespondingClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-interactive-chat", "StopRespondingEventArgs")]
    public sealed class StopRespondingEventArgs : JsObject
    {
        /// <summary>Gets the prompt whose response was stopped.</summary>
        public ReadOnly<string> Prompt => ReadOnly<string>(optional: true);

        /// <summary>Gets the index of that prompt in the conversation.</summary>
        public ReadOnly<int> DataIndex => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// A prompt with its response, which <see cref="AddPromptResponse(Arg{PromptModel})"/> adds to the conversation.
    /// </summary>
    [Mirrors("@syncfusion/ej2-interactive-chat", "PromptModel")]
    public sealed class PromptModel
    {
        /// <summary>Gets or sets the prompt's text.</summary>
        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }

        /// <summary>Gets or sets the response's text.</summary>
        [JsonPropertyName("response")]
        public string Response { get; set; } = "";
    }
}
