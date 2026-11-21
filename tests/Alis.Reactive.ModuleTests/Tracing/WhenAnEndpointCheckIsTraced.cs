using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.Tracing;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.Tracing;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnEndpointCheckIsTraced
{
    [Test]
    public void a_refused_check_records_its_validator_its_rule_set_and_its_failures_under_the_request()
    {
        var stopped = new List<Activity>();
        using var listener = AListenerOf(stopped);
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        using var controller = AControllerOfTheRound();

        var result = controller.Validate<DoseOrder, RoundValidator>(new DoseOrder { Resident = "Ruth", Strength = 900 });

        Assert.That(result.Errors.Count, Is.EqualTo(1));
        Assert.That(stopped.ConvertAll(Recorded), Is.EqualTo(new (string, string, object?, object?, object?)[] { ("alis.validate", request.SpanId.ToHexString(), "RoundValidator", "default", 1) }));
    }

    [Test]
    public void an_accepted_check_records_no_failure()
    {
        var stopped = new List<Activity>();
        using var listener = AListenerOf(stopped);
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        using var controller = AControllerOfTheRound();

        controller.Validate<DoseOrder, RoundValidator>(new DoseOrder { Resident = "Ruth", Strength = 90 });

        Assert.That(stopped.ConvertAll(Recorded), Is.EqualTo(new (string, string, object?, object?, object?)[] { ("alis.validate", request.SpanId.ToHexString(), "RoundValidator", "default", 0) }));
    }

    [Test]
    public async Task an_asynchronous_check_records_its_rule_set_once_it_has_run()
    {
        var stopped = new List<Activity>();
        using var listener = AListenerOf(stopped);
        using var request = RoundPlans.ARequest(ActivityTraceFlags.Recorded);
        using var controller = AControllerOfTheRound();

        var checking = controller.ValidateAsync<DoseOrder, RoundValidator>(new DoseOrder { Resident = "", Strength = 90 }, "Night");
        await checking;

        Assert.That(stopped.ConvertAll(Recorded), Is.EqualTo(new (string, string, object?, object?, object?)[] { ("alis.validate", request.SpanId.ToHexString(), "RoundValidator", "Night", 1) }));
    }

    private static (string, string, object?, object?, object?) Recorded(Activity activity)
        => (activity.OperationName, activity.ParentSpanId.ToHexString(), activity.GetTagItem("alis.validator"), activity.GetTagItem("alis.rule_set"), activity.GetTagItem("alis.failure_count"));

    private static Controller AControllerOfTheRound()
    {
        var services = Views.Of(new Round("North"), _ => { }, app => app.AddReactiveFluentValidation(validation => validation.Add<RoundValidator>())).ViewContext.HttpContext.RequestServices;
        return new RoundController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services },
                ActionDescriptor = new ControllerActionDescriptor(),
            },
        };
    }

    private static ActivityListener AListenerOf(List<Activity> stopped)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == ReactiveTracing.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = stopped.Add,
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }

    private sealed class RoundController : Controller;
}
