using System;
using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// Collects reactive behavior for a view: triggers, reactions, and component registrations.
    /// Renders the collected behavior as a plan document for browser execution.
    /// </summary>
    public sealed class ReactivePlan<TModel> where TModel : class
    {
        private readonly RegisteredInputComponents _registeredInputComponents =
            new RegisteredInputComponents();

        private readonly PlanId _planId = Alis.Reactive.PlanModel.PlanId.ForModel(typeof(TModel));
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
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));

            _scope = scope;
            _services = services;
            var planIdentity = _scope.CreateIdentity(_planId);
            _context = new PlanBuildContext(planIdentity, _registeredInputComponents);
        }

        /// <summary>Gets the unique plan identifier, derived from the model type's full name.</summary>
        public string PlanId => _planId.Value;
        /// <summary>Gets whether this plan represents a partial view that merges into a parent plan.</summary>
        public bool IsPartial => _scope.IsPartial;
        internal bool RendersValidationSummary => _scope.RendersValidationSummary;
        internal IReadOnlyDictionary<string, ComponentRegistration> RegisteredInputComponents =>
            _registeredInputComponents.Snapshot();
        internal PlanBuildContext Context => _context;

        /// <summary>Registers a plugin's type metadata in the plan. Must be called before any p.Plugin() reference.</summary>
        public void RegisterPlugin(string pluginName, Action<Builders.PluginTypeBuilder> configure)
        {
            if (string.IsNullOrWhiteSpace(pluginName))
                throw new ArgumentException("Plugin name required.", nameof(pluginName));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var builder = new Builders.PluginTypeBuilder(pluginName);
            configure(builder);
            _context.RegisterPlugin(builder.Build());
        }

        /// <summary>Registers a typed browser plugin contract in the plan.</summary>
        public void RegisterPlugin(ReactivePlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            _context.RegisterPlugin(plugin.ToContract());
        }

        /// <summary>Creates and registers a typed browser plugin contract in the plan.</summary>
        public TPlugin RegisterPlugin<TPlugin>()
            where TPlugin : ReactivePlugin, new()
        {
            var plugin = new TPlugin();
            RegisterPlugin(plugin);
            return plugin;
        }

        internal void RegisterInputComponent(ComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            _registeredInputComponents.Add(registration.RegisteredBindingPath, registration);
        }

        internal bool HasRegisteredInputComponent(BindingPath bindingPath) =>
            _registeredInputComponents.Contains(bindingPath);

        /// <summary>Registers all components and resolves validation, then serializes the plan as compact JSON.</summary>
        public string Render()
        {
            ResolveAll(_services);
            return ReactivePlanSerializer.Serialize(_context.BuildPlan());
        }

        public string Render(IServiceProvider services)
        {
            ResolveAll(services);
            return ReactivePlanSerializer.Serialize(_context.BuildPlan());
        }

        /// <summary>Registers all components and resolves validation, then serializes the plan as indented JSON for debugging.</summary>
        public string RenderFormatted()
        {
            ResolveAll(_services);
            return ReactivePlanSerializer.SerializeFormatted(_context.BuildPlan());
        }

        public string RenderFormatted(IServiceProvider services)
        {
            ResolveAll(services);
            return ReactivePlanSerializer.SerializeFormatted(_context.BuildPlan());
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

        internal static string Serialize(PlanDocument plan) => JsonSerializer.Serialize(plan, Compact);
        internal static string SerializeFormatted(PlanDocument plan) => JsonSerializer.Serialize(plan, Formatted);
    }
}
