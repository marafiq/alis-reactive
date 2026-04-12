using System;
using Alis.Reactive.Builders;
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
}
