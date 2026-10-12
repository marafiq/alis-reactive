using Alis.Reactive.Builders.Requests;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.ResponseConditions
{
    public class ResponseConditionsModel { }

    public class ApprovalResponse
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class ErrorDetailResponse
    {
        public string Message { get; set; } = "";
        public int Code { get; set; }
    }

    /// <summary>
    /// Razor cannot parse generic type parameters (e.g. OnError&lt;ErrorDetailResponse&gt;)
    /// because it confuses &lt;TypeName&gt; with HTML tags. This helper wraps the
    /// generic call so the view stays clean.
    /// </summary>
    public static class ResponseConditionsHelper
    {
        public static void ConfigureTypedErrorHandler(ResponseBuilder<ResponseConditionsModel> r)
        {
            r.OnSuccess(s => s.Element("s5-result").SetText("success"));
            r.OnError<ErrorDetailResponse>((err, e) =>
            {
                e.When(err, x => x.Code).Eq(422)
                 .Then(then => then.Element("s5-result").SetText("validation error"))
                 .Else(el => el.Element("s5-result").SetText("other error"));
            });
        }
    }

    /// <summary>The document events the ResponseConditions page raises and hears.</summary>
    public static class ResponseConditionsEvents
    {
        public static readonly AppEvent S1WhenApproved = AppEvent.Named("s1-when-approved");
        public static readonly AppEvent S1WhenDenied = AppEvent.Named("s1-when-denied");
        public static readonly AppEvent S2ElseifApprove = AppEvent.Named("s2-elseif-approve");
        public static readonly AppEvent S2ElseifDeny = AppEvent.Named("s2-elseif-deny");
        public static readonly AppEvent S2ElseifPending = AppEvent.Named("s2-elseif-pending");
        public static readonly AppEvent S3AndApprove = AppEvent.Named("s3-and-approve");
        public static readonly AppEvent S3AndDeny = AppEvent.Named("s3-and-deny");
        public static readonly AppEvent S4ErrorCatchall = AppEvent.Named("s4-error-catchall");
        public static readonly AppEvent S5ErrorTyped = AppEvent.Named("s5-error-typed");
    }
}
