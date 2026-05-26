using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class RegisteredInputComponents
    {
        private readonly Dictionary<string, ComponentRegistration> _registrations =
            new Dictionary<string, ComponentRegistration>(System.StringComparer.Ordinal);

        internal IReadOnlyDictionary<string, ComponentRegistration> Snapshot() =>
            new Dictionary<string, ComponentRegistration>(_registrations, System.StringComparer.Ordinal);

        internal IReadOnlyList<KeyValuePair<string, ComponentRegistration>> Entries =>
            new List<KeyValuePair<string, ComponentRegistration>>(_registrations);

        internal bool Contains(BindingPath bindingPath) =>
            _registrations.ContainsKey(bindingPath.Value);

        internal void Add(BindingPath bindingPath, ComponentRegistration registration)
        {
            if (_registrations.TryGetValue(bindingPath.Value, out var existing))
            {
                var duplicateRegistrationIsIdempotent = existing.HasSameContractAs(registration);
                if (duplicateRegistrationIsIdempotent)
                    return;

                throw DuplicateRegistration(bindingPath, existing, registration);
            }

            _registrations[bindingPath.Value] = registration;
        }

        internal bool TryFindForComponent(
            ComponentId componentId,
            [NotNullWhen(true)] out ComponentRegistration? registration)
        {
            foreach (var candidate in _registrations.Values)
            {
                if (candidate.ComponentId == componentId.Value)
                {
                    registration = candidate;
                    return true;
                }
            }

            registration = null;
            return false;
        }

        private static System.InvalidOperationException DuplicateRegistration(
            BindingPath bindingPath,
            ComponentRegistration existing,
            ComponentRegistration incoming) =>
            new System.InvalidOperationException(
                $"Duplicate component registration for binding path '{bindingPath.Value}': " +
                $"existing {existing.DescribeContract()} vs " +
                $"new {incoming.DescribeContract()}.");
    }
}
