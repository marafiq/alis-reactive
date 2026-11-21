using Alis.Reactive.Declarations;
using Alis.Reactive.Native.NativeComponents;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

/// <summary>
/// The page's confirm: a modal dialog the layout renders once, which asks the person a question and answers yes or no.
/// </summary>
/// <remarks>
/// A view reaches it with no id, <c>pipeline.Component&lt;NativeConfirm&gt;()</c>, and branches on its answer:
/// <c>pipeline.When(confirm.Ask("Discharge Ada?"))</c>. Asks take their turn in the order they are made, one question
/// at a time. When the steps that asked end, their question closes, or leaves its turn unshown, and answers no.
/// </remarks>
public sealed class NativeConfirm : NativeComponent, IAppLevelComponent
{
    /// <summary>The id of the confirm's dialog on every page.</summary>
    public const string ElementId = "alis-confirm";

    /// <summary>Gets the id of the confirm's dialog, <see cref="ElementId"/>.</summary>
    public string DefaultId => ElementId;

    /// <summary>Asks the person a question, shown as text, and waits for the answer: true for OK, false for cancel or Escape.</summary>
    /// <param name="message">The question; one that reads nothing shows no text.</param>
    /// <returns>The answer, which a condition reads once the person has answered.</returns>
    public MethodResult<bool> Ask(Arg<string?> message) => MethodAsync<bool>([message, AbortSignal()]);
}
