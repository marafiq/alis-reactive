using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Resolvers;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior so it executes in the browser.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Create a plan at the top of a view with <c>Html.ReactivePlan()</c>, pass it to
    /// <see cref="Alis.Reactive.Builders.TriggerBuilder{TModel}"/> via <c>Html.On(plan, ...)</c>
    /// to define behavior, and call <c>Html.RenderPlan(plan)</c> at the bottom to activate it.
    /// </para>
    /// <para>
    /// Partial views that share the same <typeparamref name="TModel"/> use
    /// <c>Html.ResolvePlan()</c> instead. Both plans merge and execute as a single unit.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
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

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly Dictionary<string, ComponentRegistration> _componentsMap = new Dictionary<string, ComponentRegistration>();

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ReactivePlan(bool isPartial = false) { IsPartial = isPartial; }

        /// <summary>
        /// Gets the unique plan identifier, derived from the model's full type name.
        /// </summary>
        /// <remarks>
        /// Used to scope validation summary elements. Each view's summary is tagged
        /// with this ID so errors route to the correct view.
        /// </remarks>
        public string PlanId { get; } = typeof(TModel).FullName!;

        /// <summary>
        /// Gets whether this plan belongs to a partial view.
        /// </summary>
        /// <remarks>
        /// Partial plans merge into the owning view's plan in the browser.
        /// The view emits the validation summary. Partial views do not.
        /// </remarks>
        public bool IsPartial { get; }

        /// <summary>
        /// Gets the registered components keyed by model binding path.
        /// </summary>
        /// <remarks>
        /// Populated when component builders (e.g. <c>Html.InputField(...).NativeTextBox()</c>)
        /// register themselves. Used by validation and gather resolvers to map model
        /// properties to their component IDs, vendors, and read expressions.
        /// </remarks>
        public IReadOnlyDictionary<string, ComponentRegistration> ComponentsMap => _componentsMap;

        /// <summary>
        /// Registers a trigger-reaction pair in the plan. Called by
        /// <see cref="Builders.TriggerBuilder{TModel}"/>, not intended for direct use in views.
        /// </summary>
        internal void AddEntry(Entry entry)
        {
            _entries.Add(entry);
        }

        /// <summary>
        /// Registers a component for a model property so validation and gather can find it.
        /// Called by component builders, not intended for direct use in views.
        /// </summary>
        /// <param name="bindingPath">The model property path (e.g. <c>"FacilityId"</c>, <c>"Address.City"</c>).</param>
        /// <param name="entry">The component registration describing ID, vendor, and read expression.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a different component is already registered for <paramref name="bindingPath"/>.
        /// Each model property maps to exactly one component.
        /// </exception>
        internal void AddToComponentsMap(string bindingPath, ComponentRegistration entry)
        {
            if (_componentsMap.TryGetValue(bindingPath, out var existing))
            {
                if (existing.ComponentId == entry.ComponentId
                    && existing.Vendor == entry.Vendor
                    && existing.ReadExpr == entry.ReadExpr
                    && existing.ComponentType == entry.ComponentType
                    && existing.CoerceAs == entry.CoerceAs)
                    return;

                throw new InvalidOperationException(
                    $"Duplicate component registration for binding path '{bindingPath}': " +
                    $"existing [{existing.ComponentId}, {existing.Vendor}, {existing.ReadExpr}, {existing.ComponentType}, {existing.CoerceAs}] vs " +
                    $"new [{entry.ComponentId}, {entry.Vendor}, {entry.ReadExpr}, {entry.ComponentType}, {entry.CoerceAs}]. " +
                    "Each binding path must map to exactly one component.");
            }

            _componentsMap[bindingPath] = entry;
        }

        /// <summary>
        /// Renders the plan for embedding in the page.
        /// </summary>
        /// <remarks>
        /// Called by <c>Html.RenderPlan(plan)</c>, not called directly in views.
        /// Resolves validation rules and component enrichment before rendering.
        /// </remarks>
        /// <returns>The rendered plan string consumed by the browser.</returns>
        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new
            {
                planId = PlanId,
                components = SerializeComponentsMap(),
                entries = _entries
            }, CompactOptions);
        }

        /// <summary>
        /// Renders the plan with indentation for debugging and test snapshots.
        /// </summary>
        /// <returns>The rendered plan string with indentation for readability.</returns>
        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(new
            {
                planId = PlanId,
                components = SerializeComponentsMap(),
                entries = _entries
            }, FormattedOptions);
        }

        // Flatten ComponentRegistration to anonymous objects for JSON serialization
        private Dictionary<string, object> SerializeComponentsMap()
        {
            var result = new Dictionary<string, object>();
            foreach (var kvp in _componentsMap)
            {
                result[kvp.Key] = new
                {
                    id = kvp.Value.ComponentId,
                    vendor = kvp.Value.Vendor,
                    readExpr = kvp.Value.ReadExpr,
                    componentType = kvp.Value.ComponentType,
                    coerceAs = kvp.Value.CoerceAs
                };
            }
            return result;
        }

        // Resolve validation rules and component enrichment before serialization.
        // Must run before every Render/RenderFormatted call.
        private void ResolveAll()
        {
            var extractor = ReactivePlanConfig.Extractor;
            if (extractor != null)
            {
                ValidationResolver.Resolve(_entries, extractor, _componentsMap);
            }
            else if (ValidationResolver.HasValidatorTypes(_entries))
            {
                throw new InvalidOperationException(
                    "One or more requests use Validate<TValidator>() but no validation extractor is registered. " +
                    "Call ReactivePlanConfig.UseValidationExtractor(...) at app startup.");
            }
            else if (_componentsMap.Count > 0)
            {
                ValidationResolver.EnrichFromComponents(_entries, _componentsMap);
            }

            // Stamp planId on all validation descriptors for summary div scoping
            ValidationResolver.StampPlanId(_entries, PlanId);
        }
    }
}
