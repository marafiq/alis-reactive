namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Model for the DomOps sandbox: array operations over native DOM collections.</summary>
    public class DomOpsModel
    {
    }

    /// <summary>
    /// Typed stub for a DOM child element, so the DSL lambda <c>x =&gt; x.GetAttribute("data-risk")</c>
    /// type-checks in C#. At runtime the element is the live DOM node; the DSL calls its getAttribute
    /// via RuntimePath.call. The stub bodies are never executed.
    /// </summary>
    public sealed class DomChild
    {
        public string? GetAttribute(string name) => null;
    }
}
