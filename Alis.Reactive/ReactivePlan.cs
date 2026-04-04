using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive workflows and registered components for a view, then
        /// renders the execution plan for the browser runtime.
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

        private readonly PlanAuthoringContext _authoring;

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on framework-owned contract types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ReactivePlan(bool isPartial = false, string? sourceId = null)
        {
            IsPartial = isPartial;
            _authoring = new PlanAuthoringContext(PlanId, sourceId);
        }

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
        /// register themselves. Used when shaping bindings and request values so model
        /// properties map to the correct component object and readable member path.
        /// </remarks>
        internal IReadOnlyDictionary<string, ComponentRegistration> RegisteredComponents => _authoring.Components;
        internal PlanAuthoringContext Authoring => _authoring;

        internal void EnsureComponentRegistered(string bindingPath)
        {
            if (_authoring.Components.ContainsKey(bindingPath))
                return;

            throw new InvalidOperationException(
                $"Component for '{bindingPath}' was rendered without calling " +
                $"plan.RegisterComponent(). Validation and gather will not work. " +
                $"Add plan.RegisterComponent(\"{bindingPath}\", ...) in your HtmlExtensions factory.");
        }

        /// <summary>
        /// Registers a component for a model property so validation and gather can find it.
        /// Called by component builders, not intended for direct use in views.
        /// </summary>
        /// <param name="bindingPath">The model property path (e.g. <c>"FacilityId"</c>, <c>"Address.City"</c>).</param>
        /// <param name="registration">The component registration describing ID, vendor, and value path.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a different component is already registered for <paramref name="bindingPath"/>.
        /// Each model property maps to exactly one component.
        /// </exception>
        internal void RegisterComponent(string bindingPath, ComponentRegistration registration)
        {
            if (_authoring.Components.TryGetValue(bindingPath, out var existing))
            {
                if (existing.ComponentId == registration.ComponentId
                    && existing.Component.Vendor == registration.Component.Vendor
                    && CapabilityPath.Same(existing.Binding.Path, registration.Binding.Path)
                    && string.Equals(existing.Component.Kind, registration.Component.Kind, StringComparison.Ordinal)
                    && ValueShapeFactory.AreEquivalent(existing.BindingShape, registration.BindingShape))
                    return;

                throw new InvalidOperationException(
                    $"Duplicate component registration for binding path '{bindingPath}': " +
                    $"existing [{existing.ComponentId}, {existing.Component.Vendor}, {CapabilityPath.Format(existing.Binding.Path)}, {existing.Component.Kind}, {ValueShapeFactory.Describe(existing.BindingShape)}] vs " +
                    $"new [{registration.ComponentId}, {registration.Component.Vendor}, {CapabilityPath.Format(registration.Binding.Path)}, {registration.Component.Kind}, {ValueShapeFactory.Describe(registration.BindingShape)}]. " +
                    "Each binding path must map to exactly one component.");
            }

            _authoring.RegisterComponent(bindingPath, registration);
        }

        internal void AddWorkflow(WorkflowScope scope, PlanAction action)
        {
            _authoring.AddWorkflow(scope, action);
        }

        internal void AddWorkflow(WorkflowScope scope, Builders.PipelineBuilder<TModel> pipeline)
        {
            foreach (var action in pipeline.BuildActions())
                _authoring.AddWorkflow(scope, action);
        }

        /// <summary>
        /// Renders the plan for embedding in the page.
        /// </summary>
        /// <remarks>
        /// Called by <c>Html.RenderPlan(plan)</c>, not called directly in views.
        /// Resolves validation rules before rendering the plan document.
        /// </remarks>
        /// <returns>The rendered plan string consumed by the browser.</returns>
        public string Render()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_authoring.Plan, CompactOptions);
        }

        /// <summary>
        /// Renders the plan with indentation for debugging and test snapshots.
        /// </summary>
        /// <returns>The rendered plan string with indentation for readability.</returns>
        public string RenderFormatted()
        {
            ResolveAll();
            return JsonSerializer.Serialize(_authoring.Plan, FormattedOptions);
        }

        // Resolve validation rules before serialization.
        // Must run before every Render/RenderFormatted call.
        private void ResolveAll()
        {
            _authoring.ResolveValidation(ReactivePlanConfig.FormValidationExtractor);
        }
    }
}
