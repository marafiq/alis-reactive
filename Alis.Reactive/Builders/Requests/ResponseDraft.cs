using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class ResponseDraft
    {
        private readonly List<ResponseHandler> _successHandlers = new List<ResponseHandler>();
        private readonly List<ResponseHandler> _errorHandlers = new List<ResponseHandler>();
        private RequestChain _chain = RequestChain.Terminal;

        internal IReadOnlyList<ResponseHandler> SuccessHandlers => _successHandlers;
        internal IReadOnlyList<ResponseHandler> ErrorHandlers => _errorHandlers;
        internal RequestChain Chain => _chain;

        internal void HandleSuccess(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _successHandlers.Add(ResponseHandler.AnyStatus(reaction));
        }

        internal void HandleError(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _errorHandlers.Add(ResponseHandler.AnyStatus(reaction));
        }

        internal void HandleError(int statusCode, Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _errorHandlers.Add(ResponseHandler.ForStatus(reaction, statusCode));
        }

        internal void ContinueWith(Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _chain = _chain.AttachFollowUp(request);
        }
    }
}
