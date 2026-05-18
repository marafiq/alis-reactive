using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as a Plan document for browser execution.
    /// </summary>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private readonly Dictionary<string, ComponentRegistration> _componentsMap =
            new Dictionary<string, ComponentRegistration>();

        private readonly PlanBuildContext _context;

        internal ReactivePlan(bool isPartial = false)
        {
            IsPartial = isPartial;
            _context = new PlanBuildContext(PlanId, isPartial ? PlanId : null, _componentsMap);
        }

        /// <summary>Gets the unique plan identifier, derived from the model type's full name.</summary>
        public string PlanId { get; } = typeof(TModel).FullName!;
        /// <summary>Gets whether this plan represents a partial view that merges into a parent plan.</summary>
        public bool IsPartial { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;
        internal PlanBuildContext Context => _context;

        /// <summary>Registers a plugin's type metadata in the plan. Must be called before any p.Plugin() reference.</summary>
        public void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                throw new ArgumentException("Plugin name required.", nameof(pluginName));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            _context.RegisterPlugin(pluginName, configure);
        }

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
                    $"Duplicate component registration for binding path '{bindingPath}': " +
                    $"existing [{existing.ComponentId}, {existing.Vendor}, {existing.ValueMember}, {existing.ComponentType}, {existing.Shape.Kind}] vs " +
                    $"new [{entry.ComponentId}, {entry.Vendor}, {entry.ValueMember}, {entry.ComponentType}, {entry.Shape.Kind}].");
            }

            _componentsMap[bindingPath] = entry;
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as compact JSON.</summary>
        public string Render()
        {
            ResolveAll();
            return ReactivePlanSerializer.Serialize(_context.BuildPlan());
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as indented JSON for debugging.</summary>
        public string RenderFormatted()
        {
            ResolveAll();
            return ReactivePlanSerializer.SerializeFormatted(_context.BuildPlan());
        }

        private void ResolveAll()
        {
            _context.RegisterInputComponents();
            new ValidationResolver(_context, _componentsMap, typeof(TModel)).Resolve();
        }
    }

    internal static class ReactivePlanSerializer
    {
        private static readonly JsonSerializerOptions Compact = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions Formatted = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        internal static string Serialize(Plan plan) => JsonSerializer.Serialize(plan, Compact);
        internal static string SerializeFormatted(Plan plan) => JsonSerializer.Serialize(plan, Formatted);
    }
}
