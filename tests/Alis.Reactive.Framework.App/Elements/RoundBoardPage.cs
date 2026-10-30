namespace Alis.Reactive.Framework.App.Elements;

/// <summary>
/// The round board: a slate on its own element, hidden by a step on the element and retitled by a step on the
/// slate. <see cref="First"/> says which of the two steps the view writes first.
/// </summary>
public sealed class RoundBoardPage
{
    public BoardStep First { get; set; }
}

/// <summary>The two steps the board's buttons write: one on the element, one on the slate it holds.</summary>
public enum BoardStep
{
    Element,
    Component,
}
