using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class ResponseRoutingDraft
    {
        private readonly List<ResponseRoute> _successRoutes = new List<ResponseRoute>();
        private readonly List<ResponseRoute> _errorRoutes = new List<ResponseRoute>();
        private RequestChain _chain = RequestChain.Terminal;

        internal ResponseRouting BuildRouting() =>
            ResponseRouting.From(
                _successRoutes,
                _errorRoutes,
                _chain);

        internal void AddSuccessRoute(ReactionGraph reaction)
        {
            _successRoutes.Add(ResponseRoute.AnyStatus(reaction));
        }

        internal void AddErrorRoute(ReactionGraph reaction)
        {
            _errorRoutes.Add(ResponseRoute.AnyStatus(reaction));
        }

        internal void AddErrorRoute(int statusCode, ReactionGraph reaction)
        {
            _errorRoutes.Add(ResponseRoute.ForStatus(reaction, statusCode));
        }

        internal void ContinueWith(RequestPlan request)
        {
            var responseAlreadyHasFollowUpRequest = !_chain.CanContinue;
            if (responseAlreadyHasFollowUpRequest)
                throw new InvalidOperationException(
                    "A response can declare only one chained request. " +
                    "To continue the sequence, attach the next Chained request to the existing follow-up request.");

            _chain = RequestChain.ContinueWith(request);
        }
    }
}
