using System;

namespace Alis.Reactive.Fusion.FusionTemplates;

/// <summary>
/// The door of a view to the typed HTML a Fusion component renders for each of its items.
/// </summary>
public static class FusionTemplate
{
    /// <summary>Starts a template over the item a component renders: a grid's row, a card or a list item.</summary>
    /// <typeparam name="TModel">The class of the item, as the app's JSON writes it.</typeparam>
    /// <returns>The template's builder, whose doors write the template's elements.</returns>
    public static FusionTemplateBuilder<TModel> Create<TModel>() => throw new NotImplementedException();
}
