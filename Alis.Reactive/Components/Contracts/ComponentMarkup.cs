using System;

namespace Alis.Reactive
{
    /// <summary>
    /// The plan finds a rendered component by its element id, so the markup a component helper renders
    /// gives that id to an element. A helper that forgets fails here, at render, naming the component,
    /// instead of in the browser when the plan looks for it.
    /// </summary>
    internal static class ComponentMarkup
    {
        internal static string Identified(string markup, string elementId, Type component)
        {
            var carriesTheId = markup.Contains("id=\"" + elementId + "\"") || markup.Contains("id='" + elementId + "'");
            if (carriesTheId) return markup;

            throw new InvalidOperationException(
                $"{component.Name} rendered no element with id \"{elementId}\". The plan finds a component by its " +
                "element id: render the component's element with that id (field.ElementId for an input).");
        }
    }
}
