using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Document;
using Alis.Reactive.Components;
using Alis.Reactive.Grammar;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Validation;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.WireTerms;

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

        private readonly PlanBuildContext _context;
        private readonly IServiceProvider? _services;

        // The plan id names the plan in traces only: the model's type, which many plans of a page may share.
        internal ReactivePlan(IServiceProvider? services, string fieldPrefix, string view)
        {
            _services = services;
            ClassNames = ResolveClassNames(services);
            _context = new PlanBuildContext(PlanId.ForModel(typeof(TModel)).Value, _renderedInputs, fieldPrefix, view, ClassNames);
        }

        /// <summary>The forms this plan validates or shows the server's errors on, by container id.</summary>
        internal IReadOnlyList<string> Forms => _context.Forms;
        internal PlanBuildContext Context => _context;

        /// <summary>The class names the fields of this plan render with: the app's choice, else the framework's.</summary>
        internal ReactiveClassNames ClassNames { get; }

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
            _context.AddInput(input);
        }

        /// <summary>
        /// The browser rules <typeparamref name="TValidator"/> declares, resolved from the services the plan
        /// renders with, for a component whose own JavaScript validates: a grid's cell editor.
        /// </summary>
        /// <example><c>FusionGridValidation.From&lt;ResidentCareItem&gt;(plan.BrowserRules&lt;ResidentCareItemValidator&gt;())</c></example>
        /// <typeparam name="TValidator">The validator registered with <c>AddReactiveFluentValidation</c>.</typeparam>
        /// <returns>Each field the validator names, with the rules the browser can evaluate.</returns>
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>() where TValidator : class =>
            RequireBrowserRuleSource(_services, CSharpTypeName.Of(typeof(TValidator)), "This view")
                .GetBrowserRules<TValidator>();

        /// <summary>The browser rules <typeparamref name="TValidator"/> declares in one set (<c>RuleSet("Issue", ..)</c>).</summary>
        /// <typeparam name="TValidator">The validator registered with <c>AddReactiveFluentValidation</c>.</typeparam>
        /// <param name="ruleSet">The set, as the validator's <c>RuleSet</c> named it; a set it does not declare is refused.</param>
        /// <returns>Each field the set names, with the rules the browser can evaluate.</returns>
        public IReadOnlyList<BrowserField> BrowserRules<TValidator>(string ruleSet) where TValidator : class =>
            RequireBrowserRuleSource(_services, CSharpTypeName.Of(typeof(TValidator)), "This view")
                .GetBrowserRules<TValidator>(ruleSet);

        // The plan as its script carries it, once, after everything written to it: the rendered inputs join it, each
        // validated form is bound to its validator's rules from the services the plan started with, and the grammar is
        // serialized. Html.RenderPlan is the one caller, so a plan always renders with the page's record.
        internal string Serialize()
        {
            _context.Seal();
            ResolveAll();
            return GrammarSerializer.Serialize(_context.Write());
        }

        // Every rendered input joins the plan, then each validated form is bound to its one validator's rules:
        // every request that validates a form names the same validator, so the form is bound once. A form only the
        // server checks has no fields: its messages land beside the mounted inputs inside it (grammar.md F8).
        private void ResolveAll()
        {
            _context.DeclareRenderedInputs();

            var validated = _context.ValidatedRequests;
            var oneByForm = validated
                .Select(request => request.Validation)
                .GroupBy(validation => validation.Form, StringComparer.Ordinal)
                .Select(sameForm => sameForm.First())
                .ToList();
            if (oneByForm.Count > 0)
            {
                var ruleSource = RequireBrowserRuleSource(
                    _services, CSharpTypeName.Of(validated[0].Validation.Validator), $"Request at '{validated[0].Request.Url}'");
                foreach (var validation in oneByForm)
                {
                    var fields = validation.Rules(ruleSource);
                    _context.Check(validation.Form, CheckedField.Bind(validation.Form, fields, _context.FieldPrefix).ToList());
                }
            }
        }

        // The names the app registered with AddReactiveClassNames, else the framework's own: an app that chose none
        // renders the names the core's stylesheet styles, which is the promise and not a gap.
        private static ReactiveClassNames ResolveClassNames(IServiceProvider? services)
        {
            var names = services?.GetService(typeof(ReactiveClassNames)) as ReactiveClassNames;

#if NET48
            // MVC5's DependencyResolver, bridged over the app's container at startup. Its default resolver creates
            // only types with a public parameterless constructor, and ReactiveClassNames has none, so an app that
            // registered nothing gets null here and the framework's names below.
            names ??= System.Web.Mvc.DependencyResolver.Current?.GetService(typeof(ReactiveClassNames)) as ReactiveClassNames;
#endif

            return names ?? new ReactiveClassNames();
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
                "but no validators could be resolved. " +
                "Register the validators with AddReactiveFluentValidation(...) and bridge it to " +
                "MVC5 by calling DependencyResolver.SetResolver(...) in Application_Start.");
#else
            throw new InvalidOperationException(
                $"{requiredBy} specifies validation source '{validatorName}', " +
                "but no validators are registered in DI. " +
                "Register them with services.AddReactiveFluentValidation(...).");
#endif
        }
    }
}
