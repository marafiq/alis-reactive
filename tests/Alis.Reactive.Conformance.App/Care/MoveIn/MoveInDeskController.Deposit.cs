using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.Deposits)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay([FromRoute] string draftId, [FromBody] DepositPage sent,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<DepositPage, DepositRules>(sent);
        if (!check.IsValid) return check.ToErrorResponse();
        await World.Reach(GateName.Of(RequestGate.Attempt), Request.Path);
        var taken = Desk.AuthoriseDeposit(draftId, sent, coordinator, operation);
        if (taken.Reply is not null && DropsItsConnection()) return new EmptyResult();
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return taken.Reply is { } reply ? Payment(taken.Status, reply) : StatusCode(taken.Status, MoveInErrors.Refused("deposit-refused", taken.Reason));
    }

    [HttpGet(MoveInEndpoints.DepositReceipt)]
    public IActionResult ReadDeposit([FromQuery] string? draftId, [FromQuery] Guid operationId)
    {
        if (draftId is null) return Content("Choose the deposit operation to read.");
        var receipt = Desk.ReadDeposit(draftId, operationId);
        if (receipt.Status == 403) return StatusCode(403, MoveInErrors.Forbidden());
        return receipt.Reply is { } reply ? Payment(200, reply) : NotFound(MoveInErrors.Refused("no-deposit", "No authorised receipt for this operation."));
    }

    private IActionResult Payment(int status, PaymentReply reply)
    {
        Response.Headers[MoveInEndpoints.PaidAmount] = reply.Deposit.Receipt.Amount.ToString("0.00", CultureInfo.InvariantCulture);
        Response.Headers[MoveInEndpoints.BalanceDue] = reply.Deposit.Balance.Due.ToString("0.00", CultureInfo.InvariantCulture);
        return StatusCode(status, reply);
    }
}

public static partial class MoveInEndpoints
{
    public const string Deposits = Page + "/deposits";
    public const string DepositReceipt = CarePages.MoveIns + "deposit-receipt";
    public const string PaidAmount = "Deposit-Amount";
    public const string BalanceDue = "Deposit-Due";
}
