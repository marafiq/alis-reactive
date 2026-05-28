using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class BrowserObjectContracts
    {
        private readonly Dictionary<string, BrowserObjectContract> _byTypeKey = new Dictionary<string, BrowserObjectContract>();

        internal IReadOnlyDictionary<string, BrowserObjectContract> Snapshot() =>
            new Dictionary<string, BrowserObjectContract>(_byTypeKey);

        internal bool Contains(TypeKey key) =>
            _byTypeKey.ContainsKey(key.Value);

        internal BrowserObjectContract Require(TypeKey key)
        {
            if (!_byTypeKey.TryGetValue(key.Value, out var objectContract))
                throw new InvalidOperationException($"browser object contract '{key.Value}' is not registered in this plan.");

            return objectContract;
        }

        internal void AddOrReplace(TypeKey key, BrowserObjectContract objectContract) =>
            _byTypeKey[key.Value] = objectContract;

        internal void EnsureInputValueContract(TypeKey key, InputValueContract contract)
        {
            EnsureEmpty(key);
            contract.Enrich(Require(key));
        }

        internal void EnsureEmpty(TypeKey key)
        {
            if (!_byTypeKey.ContainsKey(key.Value))
                _byTypeKey[key.Value] = new BrowserObjectContract();
        }

        internal void EnsureProperty(TypeKey typeKey, ObjectPropertyContract contract) =>
            Require(typeKey).Declare(contract);

        internal ObjectMethod EnsureMethod(TypeKey typeKey, ObjectMethodContract contract) =>
            Require(typeKey).Declare(contract);

        internal void EnsureEvent(TypeKey typeKey, ObjectEventContract contract) =>
            Require(typeKey).Declare(contract);

        internal void RegisterPlugin(PluginContract contract)
        {
            if (Contains(contract.TypeKey))
                throw new InvalidOperationException($"Plugin '{contract.Name.Value}' is already registered.");

            AddOrReplace(contract.TypeKey, contract.ToBrowserObjectContract());
        }

        internal MethodSignature EnsurePluginMethod(PluginMethodRequirement methodRead)
        {
            var typeKey = TypeKey.Plugin(methodRead.PluginName);

            if (!Contains(typeKey))
                throw new InvalidOperationException(
                    $"Plugin '{methodRead.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{methodRead.PluginName.Value}\", ...) first.");

            var method = EnsureMethod(
                typeKey,
                methodRead.ToObjectMethodContract());

            return method.Signature;
        }

        internal void EnsurePluginProperty(PluginPropertyRequirement propertyRead)
        {
            var typeKey = TypeKey.Plugin(propertyRead.PluginName);

            if (!Contains(typeKey))
                throw new InvalidOperationException(
                    $"Plugin '{propertyRead.PluginName.Value}' is not registered. " +
                    $"Call plan.RegisterPlugin(\"{propertyRead.PluginName.Value}\", ...) first.");

            EnsureProperty(typeKey, propertyRead.ToObjectPropertyContract());
        }
    }
}
