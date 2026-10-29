using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
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
        private readonly RenderedInputs _renderedInputs = new RenderedInputs();

        private readonly PlanId _planId;
        private readonly PlanBuildContext _context;
        private readonly IServiceProvider? _services;

        private readonly ReactivePlanScope _scope;

        internal ReactivePlan(ReactivePlanScope scope, IServiceProvider? services, PlanId planId, string fieldPrefix)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));

            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _scope = scope;
            _services = services;
            _context = new PlanBuildContext(_planId.Value, _renderedInputs, fieldPrefix);
        }

        /// <summary>The plan's identifier: the model type's full name, or the element's id for an element plan.</summary>
        public string PlanId => _planId.Value;
        /// <summary>Whether this plan is rendered by a partial view and merges into the owning view's Active Plan.</summary>
        public bool IsPartial => _scope.IsPartial;
        internal bool RendersValidationSummary => _scope.RendersValidationSummary;
        internal PlanBuildContext Context => _context;

        /// <summary>Registers a plugin with the plan; the returned plugin's members are used directly.</summary>
        /// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
        /// <returns>The plugin: <c>slugify.Slug(planner.Wing)</c>.</returns>
        public TPlugin RegisterPlugin<TPlugin>()
            where TPlugin : Plugin, new()
        {
            var plugin = new TPlugin();
            _context.RegisterPlugin(plugin.Name);
            plugin.BindTo(new ModuleBinding(Target.Plugin(plugin.Name), _context));
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

        internal void AddRenderedInput(RenderedInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _renderedInputs.Add(input);
        }

        /// <summary>
        /// The browser rules <typeparamref name="TValidator"/> declares, resolved from the services the plan
        /// renders with, for a component whose own JavaScript validates: a grid's cell editor.
        /// </summary>
        /// <example><c>FusionGridValidation.From&lt;ResidentCareItem&gt;(plan.BrowserRules&lt;ResidentCareItemValidator&gt;())</c></example>
        /// <typeparam name="TValidator">The validator registered with <c>AddReactiveFluentValidation</c>.</typeparam>
        /// <returns>Each field the validator names, with the rules the browser can evaluate.</returns>
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>() where TValidator : class =>
            RequireBrowserRuleSource(_services, typeof(TValidator).Name, "This view")
                .GetBrowserRules<TValidator>();

        /// <summary>Registers components, resolves validation, and serializes the Reactive Plan as compact JSON.</summary>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render()
        {
            ResolveAll(_services);
            return GrammarSerializer.Serialize(_context.Write());
        }

        /// <summary>Registers components, resolves validation with the supplied services, and serializes the Reactive Plan as compact JSON.</summary>
        /// <param name="services">Services used to resolve validation rule sources required by queued validation rules.</param>
        /// <returns>Compact Reactive Plan JSON ready for a <c>data-reactive-plan</c> script.</returns>
        public string Render(IServiceProvider services)
        {
            ResolveAll(services);
            return GrammarSerializer.Serialize(_context.Write());
        }

        // Every rendered input joins the plan, then each validated form takes its one validator's rules: every
        // request that validates a form names the same validator, so the form is bound once.
        private void ResolveAll(IServiceProvider? services)
        {
            _context.DeclareRenderedInputs();

            var validated = _context.ValidatedRequests;
            if (validated.Count == 0) return;

            var ruleSource = RequireBrowserRuleSource(
                services, validated[0].Validation.Validator.Name, $"Request at '{validated[0].Request.Url}'");
            var renderedInputs = _renderedInputs.Snapshot();
            var forms = validated
                .Select(request => request.Validation)
                .GroupBy(validation => validation.Form, StringComparer.Ordinal)
                .Select(requests => requests.First());
            foreach (var form in forms)
            {
                var fields = form.Rules(ruleSource);
                _context.Check(form.Form, CheckedField.Bind(fields, renderedInputs, typeof(TModel), _context.FieldPrefix).ToList());
            }
        }

        private static IBrowserRuleSource RequireBrowserRuleSource(
            IServiceProvider? services,
            string validatorName,
            string requiredBy)
        {
            // ASP.NET Core flows the source through the per-request IServiceProvider.
            var source = services?.GetService(typeof(IBrowserRuleSource)) as IBrowserRuleSource;

#if NET48
            // net48 / System.Web has no per-request IServiceProvider. MVC5's idiomatic
            // service locator is DependencyResolver, bridged at startup via SetResolver.
            source ??= System.Web.Mvc.DependencyResolver.Current?.GetService(typeof(IBrowserRuleSource))
                as IBrowserRuleSource;
#endif

            if (source != null) return source;

#if NET48
            throw new InvalidOperationException(
                $"{requiredBy} specifies validation source '{validatorName}', " +
                "but no IBrowserRuleSource could be resolved. " +
                "Register the validators with AddReactiveFluentValidation(...) and bridge it to " +
                "MVC5 by calling DependencyResolver.SetResolver(...) in Application_Start.");
#else
            throw new InvalidOperationException(
                $"{requiredBy} specifies validation source '{validatorName}', " +
                "but no IBrowserRuleSource is registered in DI. " +
                "Register the validators with services.AddReactiveFluentValidation(...), " +
                "or register your own IBrowserRuleSource.");
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
    }

    internal sealed class RootViewPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => false;

        internal override bool RendersValidationSummary => true;
    }

    internal sealed class PartialViewPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => true;

        internal override bool RendersValidationSummary => false;
    }

    // A plan of its own for one element (a link in a row template): it composes with no other plan
    // and has no form, so it renders no validation summary.
    internal sealed class ElementPlanScope : ReactivePlanScope
    {
        internal override bool IsPartial => false;

        internal override bool RendersValidationSummary => false;
    }
}
