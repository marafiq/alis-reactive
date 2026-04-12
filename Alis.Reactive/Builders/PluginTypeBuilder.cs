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

        /// <summary>Declares a method that returns a typed value. Shape inferred from T.</summary>
        public PluginTypeBuilder Method<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Method name required.", nameof(name));
            var returns = Shape.FromClrType(typeof(T));
            _plan.MutableTypes[_typeKey].WithMethod(name, Path.Parse(name), returns: returns);
            return this;
        }

        /// <summary>Declares a void method (no return value).</summary>
        public PluginTypeBuilder Void(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Method name required.", nameof(name));
            _plan.MutableTypes[_typeKey].WithMethod(name, Path.Parse(name));
            return this;
        }
    }
}
