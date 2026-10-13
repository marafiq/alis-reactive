using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;
using Alis.Reactive.Grammar.Lowering;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as generated Reactive Plan JSON.
    /// </summary>
    /// <typeparam name="TModel">The view's model, whose members name the plan's inputs.</typeparam>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private readonly RegisteredInputComponents _registeredInputComponents =
            new RegisteredInputComponents();

        private readonly PlanId _planId;
        private readonly PlanBuildContext _context;
        private readonly IServiceProvider? _services;

        private readonly ReactivePlanScope _scope;

        internal ReactivePlan()
            : this(ReactivePlanScope.RootView, services: null)
        {
        }

        internal ReactivePlan(ReactivePlanScope scope)
            : this(scope, services: null)
        {
        }

        internal ReactivePlan(ReactivePlanScope scope, IServiceProvider? services)
            : this(scope, services, Alis.Reactive.PlanModel.PlanId.ForModel(typeof(TModel)))
        {
        }

        internal ReactivePlan(ReactivePlanScope scope, IServiceProvider? services, PlanId planId)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));

            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _scope = scope;
            _services = services;
            var planIdentity = _scope.CreateIdentity(_planId);
            _context = new PlanBuildContext(planIdentity, _registeredInputComponents);
        }

        /// <summary>The plan's identifier: the model type's full name, or the element's id for an element plan.</summary>
        public string PlanId => _planId.Value;
        /// <summary>Whether this plan is rendered by a partial view and merges into the owning view's Active Plan.</summary>
        public bool IsPartial => _scope.IsPartial;
        internal bool RendersValidationSummary => _scope.RendersValidationSummary;
        internal IReadOnlyDictionary<string, ComponentRegistration> RegisteredInputComponents =>
            _registeredInputComponents.Snapshot();
        internal PlanBuildContext Context => _context;

        /// <summary>Registers a plugin with the plan; the returned plugin's members are used directly.</summary>
        /// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
        /// <returns>The plugin: <c>slugify.Slug(planner.Wing)</c>.</returns>
        public TPlugin RegisterPlugin<TPlugin>()
            where TPlugin : Plugin, new()
        {
            var plugin = new TPlugin();
            _context.RegisterPlugin(plugin.ToContract());
            plugin.BindTo(new PluginBinding(plugin.Name, _context));
            return plugin;
        }

        /// <summary>
        /// Joins the markup a component helper rendered for <paramref name="elementId"/> to this plan,
        /// so behavior can hang off it with <c>.Reactive(...)</c>; the view writes the returned
        /// component where the markup belongs. Inputs render into their field instead:
        /// <c>field.Render&lt;TComponent&gt;(markup)</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var builder = html.EJS().Dialog(elementId);
        /// build(builder);
        /// return plan.Render&lt;FusionDialog&gt;(elementId, builder.Render());
        /// </code>
        /// </example>
        /// <typeparam name="TComponent">The component declaration the markup renders.</typeparam>
        /// <param name="elementId">The ID the markup gives the component's element.</param>
        /// <param name="markup">The component's markup.</param>
        /// <returns>The rendered component, ready for <c>.Reactive(...)</c>.</returns>
        public RenderedComponent<TComponent, TModel> Render<TComponent>(
            string elementId,
#if NET48
            System.Web.IHtmlString markup)
#else
            Microsoft.AspNetCore.Html.IHtmlContent markup)
#endif
            where TComponent : IComponent, new()
        {
            if (string.IsNullOrWhiteSpace(elementId))
                throw new ArgumentException("Element ID required.", nameof(elementId));
            if (markup == null) throw new ArgumentNullException(nameof(markup));
            return new RenderedComponent<TComponent, TModel>(this, elementId, markup);
        }

        internal void RegisterInputComponent(ComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            _registeredInputComponents.Add(registration.RegisteredBindingPath, registration);
        }

        internal bool HasRegisteredInputComponent(BindingPath bindingPath) =>
            _registeredInputComponents.Contains(bindingPath);

        /// <summary>Registers components, resolves validation, and serializes the Reactive Plan as compact JSON.</summary>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render()
        {
            ResolveAll(_services);
            return GrammarSerializer.Serialize(PlanLowering.Lower(_context.BuildPlan()));
        }

        /// <summary>Registers components, resolves validation with the supplied services, and serializes the Reactive Plan as compact JSON.</summary>
        /// <param name="services">Services used to resolve validation rule sources required by queued validation rules.</param>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render(IServiceProvider services)
        {
            ResolveAll(services);
            return GrammarSerializer.Serialize(PlanLowering.Lower(_context.BuildPlan()));
        }

        /// <summary>Registers components, resolves validation, and serializes the Reactive Plan as indented JSON for debugging.</summary>
        /// <returns>Indented Reactive Plan JSON for diagnostics or sandbox display.</returns>
        public string RenderFormatted()
        {
            ResolveAll(_services);
            return GrammarSerializer.SerializeFormatted(PlanLowering.Lower(_context.BuildPlan()));
        }

        /// <summary>Registers components, resolves validation with the supplied services, and serializes the Reactive Plan as indented JSON.</summary>
        /// <param name="services">Services used to resolve validation rule sources required by queued validation rules.</param>
        /// <returns>Indented Reactive Plan JSON for diagnostics or sandbox display.</returns>
        public string RenderFormatted(IServiceProvider services)
        {
            ResolveAll(services);
            return GrammarSerializer.SerializeFormatted(PlanLowering.Lower(_context.BuildPlan()));
        }

        private void ResolveAll(IServiceProvider? services)
        {
            _context.RegisterInputComponents();

            if (_context.ValidationJobs.Count == 0)
                return;

            new ClientValidationRuleBinder(
                    _context,
                    _registeredInputComponents.Snapshot(),
                    typeof(TModel),
                    RequireClientValidationRuleSource(services, _context.ValidationJobs[0]))
                .BindQueuedJobs();
        }

        private static IClientValidationRuleSource RequireClientValidationRuleSource(
            IServiceProvider? services,
            ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            // ASP.NET Core flows the source through the per-request IServiceProvider.
            var source = services?.GetService(typeof(IClientValidationRuleSource)) as IClientValidationRuleSource;

#if NET48
            // net48 / System.Web has no per-request IServiceProvider. MVC5's idiomatic
            // service locator is DependencyResolver, bridged at startup via SetResolver.
            source ??= System.Web.Mvc.DependencyResolver.Current?.GetService(typeof(IClientValidationRuleSource))
                as IClientValidationRuleSource;
#endif

            if (source != null) return source;

#if NET48
            throw new InvalidOperationException(
                $"Request at '{job.RequestUrl}' specifies validation source '{job.ValidationSourceType.Name}', " +
                "but no IClientValidationRuleSource could be resolved. " +
                "Register the FluentValidation integration with AddReactiveFluentValidation(...) and bridge it to " +
                "MVC5 by calling DependencyResolver.SetResolver(...) in Application_Start.");
#else
            throw new InvalidOperationException(
                $"Request at '{job.RequestUrl}' specifies validation source '{job.ValidationSourceType.Name}', " +
                "but no IClientValidationRuleSource is registered in DI. " +
                "Register the FluentValidation integration with services.AddReactiveFluentValidation(...), " +
                "or register your own IClientValidationRuleSource.");
#endif
        }
    }

    internal abstract class ReactivePlanScope
    {
        internal static ReactivePlanScope RootView { get; } =
            new RootViewPlanScope();

        internal static ReactivePlanScope PartialView { get; } =
            new PartialViewPlanScope();

        internal static ReactivePlanScope Element { get; } =
            new ElementPlanScope();

        internal abstract bool IsPartial { get; }

        internal abstract bool RendersValidationSummary { get; }

        internal abstract PlanIdentity CreateIdentity(PlanId planId);
    }

    internal sealed class RootViewPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => false;

        internal override bool RendersValidationSummary => true;

        internal override PlanIdentity CreateIdentity(PlanId planId)
        {
            if (planId == null) throw new ArgumentNullException(nameof(planId));
            return PlanIdentity.Root(planId);
        }
    }

    internal sealed class PartialViewPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => true;

        internal override bool RendersValidationSummary => false;

        internal override PlanIdentity CreateIdentity(PlanId planId)
        {
            if (planId == null) throw new ArgumentNullException(nameof(planId));
            return PlanIdentity.Partial(planId);
        }
    }

    // A plan of its own for one element (a link in a row template): it composes with no other plan
    // and has no form, so it renders no validation summary.
    internal sealed class ElementPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => false;

        internal override bool RendersValidationSummary => false;

        internal override PlanIdentity CreateIdentity(PlanId planId)
        {
            if (planId == null) throw new ArgumentNullException(nameof(planId));
            return PlanIdentity.Root(planId);
        }
    }
}
