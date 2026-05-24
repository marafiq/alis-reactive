using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class GatheredComponentValue
    {
        private readonly RegisteredComponentIdentity _identity;
        private readonly BindingPath _bindingPath;
        private readonly HttpPayloadKey _payloadKey;
        private readonly InputValueContract _valueContract;

        private GatheredComponentValue(
            RegisteredComponentIdentity identity,
            BindingPath bindingPath,
            HttpPayloadKey payloadKey,
            InputValueContract valueContract)
        {
            _identity = identity ?? throw new System.ArgumentNullException(nameof(identity));
            _bindingPath = bindingPath ?? throw new System.ArgumentNullException(nameof(bindingPath));
            _payloadKey = payloadKey ?? throw new System.ArgumentNullException(nameof(payloadKey));
            _valueContract = valueContract ?? throw new System.ArgumentNullException(nameof(valueContract));
        }

        internal InputComponentPlanBinding PlanBinding =>
            InputComponentPlanBinding.For(
                _identity.ComponentId,
                _identity.Vendor,
                _bindingPath,
                _valueContract);

        internal GatherField Field
        {
            get
            {
                var componentValue = ValueProducer.Read(
                    ComponentSource.Of(_identity.ComponentId.Value),
                    _valueContract.ValueMember,
                    shape: _valueContract.Shape);

                return GatherField.Of(_payloadKey.Value, componentValue);
            }
        }

        internal static GatheredComponentValue For(
            RegisteredComponentIdentity identity,
            BindingPath bindingPath,
            HttpPayloadKey payloadKey,
            InputValueContract valueContract) =>
            new GatheredComponentValue(
                identity,
                bindingPath,
                payloadKey,
                valueContract);
    }
}
