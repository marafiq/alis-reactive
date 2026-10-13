using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// A JavaScript object the page registers by name, for work the DSL does not model (URL, DOM and array
    /// APIs). Declared like any JavaScript object; its name is its only string.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class SlugPlugin : Plugin
    /// {
    ///     public SlugPlugin() : base("slugify") { }
    ///     public TypedSource&lt;string&gt; Slug(Arg&lt;string&gt; text) =&gt; Invoke&lt;string&gt;(text, name: "");   // the plugin is itself a function
    ///     public TypedSource&lt;int&gt; Count(Arg&lt;object[]&gt; items) =&gt; Invoke&lt;int&gt;(items);              // a member function
    ///     public void Mark(Arg&lt;string&gt; step) =&gt; Call(step);                                           // a member called for its effect
    ///     public ReadOnly&lt;string&gt; Version =&gt; ReadOnly&lt;string&gt;();                                     // a member read
    /// }
    ///
    /// var slugify = plan.RegisterPlugin&lt;SlugPlugin&gt;();
    /// p.Element("board-code").Text.Set(slugify.Slug(planner.Wing));
    /// </code>
    /// </example>
    public abstract class Plugin : JsObject
    {
        private readonly PluginName _name;

        /// <summary>Names the plugin; the page registers its object under this name.</summary>
        /// <param name="name">The name the page's script registers the plugin under.</param>
        protected Plugin(string name)
        {
            _name = PluginName.Of(name);
        }

        /// <summary>The name the page registers the plugin under.</summary>
        public string Name => _name.Value;

        // The plugin's members are declared where they are used: each read or call adds its own.
        internal PluginContract ToContract() =>
            PluginContract.Create(_name, Array.Empty<PluginPropertyContract>(), Array.Empty<PluginOperationContract>());
    }
}
