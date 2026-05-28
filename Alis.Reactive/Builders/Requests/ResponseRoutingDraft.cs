using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class ResponseRoutingDraft
    {
        private readonly List<ResponseRoute> _successRoutes = new List<ResponseRoute>();
        private readonly List<ResponseRoute> _errorRoutes = new List<ResponseRoute>();
        private RequestPlan? _followUpRequest;

        internal IReadOnlyList<ResponseRoute> SuccessRoutes => _successRoutes;
        internal IReadOnlyList<ResponseRoute> ErrorRoutes => _errorRoutes;
        internal RequestChain Chain =>
            _followUpRequest is null
                ? RequestChain.Terminal
                : RequestChain.ContinueWith(_followUpRequest);

        internal ResponseRouting BuildRouting() =>
            ResponseRouting.From(
                _successRoutes,
                _errorRoutes,
                Chain);

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
            var responseAlreadyHasFollowUpRequest = _followUpRequest is not null;
            if (responseAlreadyHasFollowUpRequest)
                throw new InvalidOperationException(
                    "A response can declare only one chained request. " +
                    "To continue the sequence, attach the next Chained request to the existing follow-up request.");

            _followUpRequest = request;
        }
    }
}
