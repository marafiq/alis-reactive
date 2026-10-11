using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The plugins a plan registered, kept while the plan is authored. A plugin read or call is checked
    /// against its registration here and then carries its own path and argument shapes into the plan,
    /// so nothing of this registry is written to the plan.
    /// </summary>
    internal sealed class PluginRegistry
    {
        private readonly Dictionary<string, BrowserObjectContract> _byPluginName =
            new Dictionary<string, BrowserObjectContract>(StringComparer.Ordinal);

        internal void RegisterPlugin(PluginContract contract)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            if (_byPluginName.ContainsKey(contract.Name.Value))
                throw new InvalidOperationException($"Plugin '{contract.Name.Value}' is already registered.");

            _byPluginName[contract.Name.Value] = contract.ToBrowserObjectContract();
        }

        internal MethodSignature DeclarePluginMethod(PluginMethodRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            return Require(requirement.PluginName).Declare(requirement.ToObjectMethodContract()).Signature;
        }

        internal void DeclarePluginProperty(PluginPropertyRequirement requirement)
        {
            if (requirement == null) throw new ArgumentNullException(nameof(requirement));
            Require(requirement.PluginName).Declare(requirement.ToObjectPropertyContract());
        }

        private BrowserObjectContract Require(PluginName pluginName)
        {
            if (_byPluginName.TryGetValue(pluginName.Value, out var registered)) return registered;

            throw new InvalidOperationException(
                $"Plugin '{pluginName.Value}' is not registered. " +
                $"Call plan.RegisterPlugin(\"{pluginName.Value}\", ...) first.");
        }
    }
}
