using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Grammar.Lowering
{
    /// <summary>
    /// Lowers a built plan to the grammar the runtime executes: each declared component becomes an
    /// element locator, each trigger a handler of an object's event, the inputs <c>alis.model</c> data
    /// and each validation container <c>alis.form</c> data.
    /// </summary>
    internal static class PlanLowering
    {
        private const string EventParam = "e";

        internal static GrammarPlan Lower(PlanModel.PlanDocument document)
        {
            var context = new LoweringContext(document);
            var values = new ValueLowering(context);
            var reactions = new ReactionLowering(context, values);

            var inputs = ModelInputs(context);
            if (inputs.Count > 0) context.Objects.Model(new ModelPluginData(inputs));

            var forms = new FormLowering(context, values);
            foreach (var component in document.Components.Values)
                if (component.Container is ScopedValidationContainer container)
                    context.Objects.Form(component.Id, forms.Form(container.ValidationRules));

            var handlers = document.Behaviors
                .Select(behavior => Handler(behavior, context, reactions))
                .ToList();

            return new GrammarPlan(document.PlanId, context.Objects.Vendors(), context.Objects.Snapshot(), handlers);
        }

        /// <summary>Lowers one reaction as the body of a handler of the given object's event.</summary>
        internal static GrammarPlan LowerHandler(
            PlanModel.PlanDocument document,
            Func<LoweringContext, string> on,
            string eventName,
            Func<LoweringContext, ReactionLowering, LoweringScope, IReadOnlyList<Statement>> body)
        {
            var context = new LoweringContext(document);
            var reactions = new ReactionLowering(context, new ValueLowering(context));
            var handler = new Handler(on(context), eventName, EventParam, body(context, reactions, LoweringScope.ForHandler(EventParam)));
            return new GrammarPlan(document.PlanId, context.Objects.Vendors(), context.Objects.Snapshot(), new[] { handler });
        }

        private static Handler Handler(Behavior behavior, LoweringContext context, ReactionLowering reactions)
        {
            var body = reactions.Block(behavior.Reaction, LoweringScope.ForHandler(EventParam));
            switch (behavior.StartsWhen)
            {
                case PageReadyTrigger _:
                    return new Handler(context.Objects.Document(), "ready", EventParam, body);
                case DocumentEventTrigger documentEvent:
                    return new Handler(context.Objects.Document(), documentEvent.Event, EventParam, body);
                case ComponentEventTrigger componentEvent:
                    var channel = context.ContractOf(componentEvent.Component).Events[componentEvent.Event].Channel;
                    return new Handler(context.Objects.Element(componentEvent.Component), channel, EventParam, body);
                case ServerPushTrigger serverPush:
                    var message = serverPush.EventFilter is NamedServerPushEvent named ? named.Event : "message";
                    return new Handler(context.Objects.Stream(serverPush.Url), message, EventParam, body);
                case SignalRTrigger signalR:
                    return new Handler(context.Objects.Hub(signalR.HubUrl), signalR.Method, EventParam, body);
                default:
                    throw new InvalidOperationException("No lowering for trigger " + behavior.StartsWhen.GetType().Name + ".");
            }
        }

        // Each input this plan rendered: its MVC name, its JSON body path, its value read and the event
        // its component commits a value with.
        private static IReadOnlyDictionary<string, InputBinding> ModelInputs(LoweringContext context)
        {
            var inputs = new Dictionary<string, InputBinding>(StringComparer.Ordinal);
            foreach (var component in context.Document.Components.Values)
            {
                if (!(component.Binding is PlanModel.RegisteredInputBinding binding)) continue;

                var property = context.Property(context.ContractOf(component.Id), binding.ValueMember, component.Id);
                var value = context.Read(G.Obj(context.Objects.Element(component.Id)), property.Path, binding.ModelShape);
                inputs.Add(component.Id, new InputBinding(binding.BindingPath, BodyPath(binding.Path), value, binding.ModelShape, binding.CommittedBy));
            }

            return inputs;
        }

        private static IReadOnlyList<object> BodyPath(Path path) =>
            path.Segments
                .Select(segment => segment.Kind == "index"
                    ? (object)int.Parse(segment.ToPathText(), CultureInfo.InvariantCulture)
                    : segment.ToPathText())
                .ToList();
    }
}
