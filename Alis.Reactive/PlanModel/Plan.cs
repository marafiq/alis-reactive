using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Plan
    {
        public int Version => 3;
        public string PlanId { get; }
        public string PartId { get; set; }
        public Dictionary<string, JsType> Types { get; }
        public Dictionary<string, Component> Components { get; }
        public List<Behavior> Behaviors { get; }

        private Plan(string planId)
        {
            PlanId = planId;
            Types = new Dictionary<string, JsType>();
            Components = new Dictionary<string, Component>();
            Behaviors = new List<Behavior>();
        }

        internal static Plan Create(string planId, string partId = null)
        {
            return new Plan(planId) { PartId = partId };
        }

        internal Plan WithType(string key, JsType type)
        {
            Types[key] = type;
            return this;
        }

        internal Plan WithComponent(string key, Component component)
        {
            Components[key] = component;
            return this;
        }

        internal Plan WithBehavior(Behavior behavior)
        {
            Behaviors.Add(behavior);
            return this;
        }
    }
}
