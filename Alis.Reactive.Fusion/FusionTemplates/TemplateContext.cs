using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Fusion.FusionTemplates;

internal abstract class TemplateContext
{
    internal abstract IReadOnlyList<string> Path<TItem, TMember>(Expression<Func<TItem, TMember>> member);

    internal abstract string Raises<TPayload, TMember>(AppEvent<TPayload> appEvent, Expression<Func<TPayload, TMember>> member, string template);
}

internal sealed class PlanTemplateContext<TViewModel>(ReactivePlan<TViewModel> plan) : TemplateContext
{
    internal override IReadOnlyList<string> Path<TItem, TMember>(Expression<Func<TItem, TMember>> member) => plan.JsonPath(member);

    internal override string Raises<TPayload, TMember>(AppEvent<TPayload> appEvent, Expression<Func<TPayload, TMember>> member, string template)
        => plan.Raises(appEvent, member, template);
}
