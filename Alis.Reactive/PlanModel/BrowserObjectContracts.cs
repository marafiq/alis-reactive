using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BrowserObjectContracts
    {
        private readonly Dictionary<string, BrowserObjectContract> _byTypeKey = new Dictionary<string, BrowserObjectContract>();

        internal IReadOnlyDictionary<string, BrowserObjectContract> Snapshot() =>
            new Dictionary<string, BrowserObjectContract>(_byTypeKey);

        internal BrowserObjectContract Require(BrowserObjectId key)
        {
            if (!_byTypeKey.TryGetValue(key.Value, out var objectContract))
                throw new InvalidOperationException($"browser object contract '{key.Value}' is not registered in this plan.");

            return objectContract;
        }

        internal void DeclareInputValueContract(BrowserObjectId key, InputValueContract contract)
        {
            DeclareObject(key);
            contract.Enrich(Require(key));
        }

        internal void DeclareObject(BrowserObjectId key)
        {
            if (!_byTypeKey.ContainsKey(key.Value))
                _byTypeKey[key.Value] = new BrowserObjectContract();
        }

        internal void DeclareProperty(BrowserObjectId typeKey, ObjectPropertyContract contract) =>
            Require(typeKey).Declare(contract);

        internal ObjectMethod DeclareMethod(BrowserObjectId typeKey, ObjectMethodContract contract) =>
            Require(typeKey).Declare(contract);

        internal void DeclareEvent(BrowserObjectId typeKey, ObjectEventContract contract) =>
            Require(typeKey).Declare(contract);

        internal void RegisterPlugin(PluginContract contract)
        {
            if (_byTypeKey.ContainsKey(contract.BrowserObjectId.Value))
                throw new InvalidOperationException($"Plugin '{contract.Name.Value}' is already registered.");

            _byTypeKey[contract.BrowserObjectId.Value] = contract.ToBrowserObjectContract();
        }

        internal MethodSignature DeclarePluginMethod(PluginMethodRequirement methodRead)
        {
            var typeKey = BrowserObjectId.Plugin(methodRead.PluginName);

            if (!_byTypeKey.ContainsKey(typeKey.Value))
                throw new InvalidOperationException(
                    $"Plugin '{methodRead.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{methodRead.PluginName.Value}\", ...) first.");

            var method = DeclareMethod(
                typeKey,
                methodRead.ToObjectMethodContract());

            return method.Signature;
        }

        internal void DeclarePluginProperty(PluginPropertyRequirement propertyRead)
        {
            var typeKey = BrowserObjectId.Plugin(propertyRead.PluginName);

            if (!_byTypeKey.ContainsKey(typeKey.Value))
                throw new InvalidOperationException(
                    $"Plugin '{propertyRead.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{propertyRead.PluginName.Value}\", ...) first.");

            DeclareProperty(typeKey, propertyRead.ToObjectPropertyContract());
        }
    }
}
