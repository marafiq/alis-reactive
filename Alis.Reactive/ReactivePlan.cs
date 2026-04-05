using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as a Plan document for browser execution.
    /// </summary>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions FormattedOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private readonly Dictionary<string, ComponentRegistration> _componentsMap =
            new Dictionary<string, ComponentRegistration>();

        private readonly PlanBuildContext _context;

        internal ReactivePlan(bool isPartial = false)
        {
            IsPartial = isPartial;
            var plan = Plan.Create(PlanId, isPartial ? PlanId : null);
            _context = new PlanBuildContext(plan, _componentsMap);
        }

        public string PlanId { get; } = typeof(TModel).FullName!;
        public bool IsPartial { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;
        internal PlanBuildContext Context => _context;

        internal void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
            if (_componentsMap.TryGetValue(bindingPath, out var existing))
            {
                if (existing.ComponentId == entry.ComponentId
                    && existing.Vendor == entry.Vendor
                    && existing.ValueMember == entry.ValueMember
                    && existing.ComponentType == entry.ComponentType
                    && existing.Shape == entry.Shape)
                    return;

                throw new InvalidOperationException(
                    $"Duplicate component registration for binding path '{bindingPath}'.");
            }

            _componentsMap[bindingPath] = entry;
        }

        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_context.Plan, CompactOptions);
        }

        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_context.Plan, FormattedOptions);
        }

        private void ResolveAll()
        {
            _context.RegisterInputComponents();

            // TODO: Validation resolution — extract rules from FluentValidators
            // and attach to ContainerScope on the appropriate component.
            // This will be implemented when the validation module is updated.
        }
    }
}
