using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatheredComponentValue
    {
        private readonly RegisteredComponentIdentity _identity;
        private readonly BindingPath _bindingPath;
        private readonly HttpPayloadPath _payloadPath;
        private readonly InputValueContract _valueContract;

        private GatheredComponentValue(
            RegisteredComponentIdentity identity,
            BindingPath bindingPath,
            HttpPayloadPath payloadPath,
            InputValueContract valueContract)
        {
            _identity = identity ?? throw new System.ArgumentNullException(nameof(identity));
            _bindingPath = bindingPath ?? throw new System.ArgumentNullException(nameof(bindingPath));
            _payloadPath = payloadPath ?? throw new System.ArgumentNullException(nameof(payloadPath));
            _valueContract = valueContract ?? throw new System.ArgumentNullException(nameof(valueContract));
        }

        internal InputComponentPlanBinding PlanBinding =>
            InputComponentPlanBinding.For(
                _identity.ComponentId,
                _identity.Vendor,
                _bindingPath,
                _valueContract);

        internal GatherPayloadField Field
        {
            get
            {
                var componentValue = ValueProducer.Read(
                    ComponentSource.Of(_identity.ComponentId.Value),
                    _valueContract.ValueMember,
                    shape: _valueContract.Shape);

                return GatherPayloadField.Of(_payloadPath.Value, componentValue);
            }
        }

        internal static GatheredComponentValue For(
            RegisteredComponentIdentity identity,
            BindingPath bindingPath,
            HttpPayloadPath payloadPath,
            InputValueContract valueContract) =>
            new GatheredComponentValue(
                identity,
                bindingPath,
                payloadPath,
                valueContract);
    }
}
