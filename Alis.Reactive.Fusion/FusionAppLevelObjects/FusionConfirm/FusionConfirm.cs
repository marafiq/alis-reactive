using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Fusion.FusionAppLevelObjects;

/// <summary>
/// The page's FusionConfirm: a question asked in EJ2's modal Dialog, made once on the element the layout renders, answered yes or no.
/// </summary>
/// <remarks>
/// A view reaches it with no id, <c>pipeline.Component&lt;FusionConfirm&gt;()</c>, and branches on its answer:
/// <c>pipeline.When(confirm.Ask("Discharge Ada?"))</c>. Asks take their turn in the order they are made, one question
/// at a time. When the steps that asked end, their question closes, or leaves its turn unshown, and answers no.
/// </remarks>
public sealed class FusionConfirm : Component, IAppLevelComponent
{
    /// <summary>The id of the confirm's host element on every page.</summary>
    public const string ElementId = "alisConfirmDialog";

    /// <summary>Gets the confirm's vendor: the question object the Fusion package's confirm code writes on the host.</summary>
    public override ComponentVendor Vendor => ComponentVendor.Define("fusion-confirm", objectPath: "alisConfirm", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    /// <summary>Gets the id of the confirm's host element, <see cref="ElementId"/>.</summary>
    public string DefaultId => ElementId;

    /// <summary>Asks the person a question, shown as text, and waits for the answer: true for OK, false for cancel or Escape.</summary>
    /// <param name="message">The question; one that reads nothing shows no text.</param>
    /// <returns>The answer, which a condition reads once the person has answered.</returns>
    public MethodResult<bool> Ask(Arg<string?> message) => MethodAsync<bool>([message, AbortSignal()]);
}
