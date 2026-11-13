using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A smart text area: EJ2's SmartTextArea, a text area that suggests how to go on with the text a person types, as the
/// app's endpoint writes the suggestion.
/// </summary>
/// <remarks>
/// An area bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when the area loses focus after its text changed, which commits the text.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "SmartTextArea")]
public sealed class FusionSmartTextArea : FusionComponent, IInputComponent
{
    /// <summary>Gets the text the area holds, null until set.</summary>
    public Property<string?> Value => Property<string?>();

    /// <summary>Moves the keyboard's focus to the area.</summary>
    public void FocusIn() => Method();

    /// <summary>Gets the event raised when the area loses focus after its text changed.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    /// <summary>Gets the event raised before the area shows a suggestion; a handler that sets <see cref="BeforeSuggestionInsertArgs.Cancel"/> keeps the suggestion out.</summary>
    public Event<BeforeSuggestionInsertArgs> BeforeSuggestionInsert => Event<BeforeSuggestionInsertArgs>();

    /// <summary>Gets the event raised after a person takes a suggestion into the text.</summary>
    public Event<AfterSuggestionInsertArgs> AfterSuggestionInsert => Event<AfterSuggestionInsertArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the text the area holds after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>(optional: true);

        /// <summary>Gets the text the area held before the change.</summary>
        public ReadOnly<string> PreviousValue => ReadOnly<string>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="BeforeSuggestionInsert"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "BeforeSuggestionInsertArgs")]
    public sealed class BeforeSuggestionInsertArgs : JsObject
    {
        /// <summary>Gets the suggestion the area is about to show.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets whether the suggestion is kept out; a write of true keeps it out.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="AfterSuggestionInsert"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "AfterSuggestionInsertArgs")]
    public sealed class AfterSuggestionInsertArgs : JsObject
    {
        /// <summary>Gets the suggestion the person took into the text.</summary>
        public ReadOnly<string> InsertedText => ReadOnly<string>();
    }
}

/// <summary>
/// What a view sets on a smart text area it renders.
/// </summary>
public sealed class FusionSmartTextAreaOptions
{
    /// <summary>Gets or sets who is typing and why, which the suggestions are written for.</summary>
    public string UserRole { get; set; } = "";

    /// <summary>Gets or sets the phrases the person writes most, which the suggestions draw on.</summary>
    public IReadOnlyList<string> UserPhrases { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the URL of the app's endpoint that writes the suggestion for the text the area holds.</summary>
    public string SuggestionEndpoint { get; set; } = "";

    /// <summary>Gets or sets where the area shows a suggestion.</summary>
    public FusionSmartSuggestionMode SuggestionMode { get; set; } = FusionSmartSuggestionMode.None;

    /// <summary>Gets or sets the CSS classes of the area's element, separated by spaces.</summary>
    public string CssClass { get; set; } = "";

    /// <summary>Gets or sets the number of lines of text the area shows.</summary>
    public int Rows { get; set; } = 4;
}
