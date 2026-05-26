using System;
using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PendingValidationProjections
    {
        private readonly List<ValidationJob> _jobs = new List<ValidationJob>();

        internal IReadOnlyList<ValidationJob> Jobs => _jobs;

        internal void Enqueue(Request request, ComponentId container, Type validationSourceType) =>
            _jobs.Add(new ValidationJob(request.Url, container, validationSourceType));
    }
}
