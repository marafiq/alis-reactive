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

        internal IReadOnlyList<ResponseRoute> SuccessRoutes => _successRoutes;
        internal IReadOnlyList<ResponseRoute> ErrorRoutes => _errorRoutes;
        internal RequestChain Chain => _chain;

        internal void AddSuccessRoute(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _successRoutes.Add(ResponseRoute.AnyStatus(reaction));
        }

        internal void AddErrorRoute(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _errorRoutes.Add(ResponseRoute.AnyStatus(reaction));
        }

        internal void AddErrorRoute(int statusCode, Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _errorRoutes.Add(ResponseRoute.ForStatus(reaction, statusCode));
        }

        internal void ContinueWith(Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _chain = _chain.AttachFollowUp(request);
        }
    }
}
