using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Configures a plugin's JsType members during plan construction.
    /// Used by <c>plan.RegisterPlugin("name", p => p.Method&lt;string&gt;("getToken"))</c>.
    /// </summary>
    public sealed class PluginTypeBuilder
    {
        private readonly Plan _plan;
        private readonly string _typeKey;

        internal PluginTypeBuilder(Plan plan, string typeKey)
        {
            _plan = plan;
            _typeKey = typeKey;
        }

        /// <summary>Declares a method with a return type on the plugin.</summary>
        public PluginTypeBuilder Method(string name, Shape returns)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Method name required.", nameof(name));
            _plan.MutableTypes[_typeKey].WithMethod(name, Path.Parse(name), returns: returns);
            return this;
        }
    }
}
