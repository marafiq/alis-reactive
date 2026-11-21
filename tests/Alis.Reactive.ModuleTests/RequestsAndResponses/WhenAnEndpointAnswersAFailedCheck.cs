using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;

namespace Alis.Reactive.ModuleTests.RequestsAndResponses;

[TestFixture]
[SetCulture("en-US")]
public sealed class WhenAnEndpointAnswersAFailedCheck
{
    [Test]
    public async Task the_answer_is_MVCs_validation_problem_with_every_message_under_the_member_it_names()
    {
        var answer = new ActionContext(new DefaultHttpContext { RequestServices = Views.Of(new Discharge(), _ => { }, _ => { }).ViewContext.HttpContext.RequestServices, Response = { Body = new MemoryStream() } }, new RouteData(), new ActionDescriptor());
        var check = new DischargeRules().Validate(new Discharge { Resident = "Ada Lee", Room = new DischargeRoom { Number = "12" } });
        await check.ToErrorResponse().ExecuteResultAsync(answer);
        var response = answer.HttpContext.Response;
        response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(response.Body);
        Assert.That((response.StatusCode, MediaTypeHeaderValue.Parse(response.ContentType).MediaType.Value), Is.EqualTo((400, "application/problem+json")));
        Assert.That(body.RootElement.GetProperty("errors").GetRawText(), Is.EqualTo("""{"Resident":["Name is taken","Write the resident's full name"],"Room.Number":["Room is full"]}"""));
    }
}

public sealed class Discharge
{
    public string? Resident { get; set; }

    public DischargeRoom Room { get; set; } = new();
}

public sealed class DischargeRoom
{
    public string? Number { get; set; }
}

public sealed class DischargeRules : ReactiveValidator<Discharge>
{
    public DischargeRules()
    {
        RuleFor(discharge => discharge.Resident).Must(name => name != "Ada Lee").WithMessage("Name is taken");
        RuleFor(discharge => discharge.Resident).MinimumLength(10).WithMessage("Write the resident's full name");
        RuleFor(discharge => discharge.Room.Number).Must(number => number != "12").WithMessage("Room is full");
    }
}
