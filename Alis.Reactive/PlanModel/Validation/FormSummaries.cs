namespace Alis.Reactive.PlanModel.Validation
{
    internal static class FormSummaries
    {
        internal static string IdOf(string formId) => formId + "_validation_summary";

        internal static string Markup(string formId, string? cssClass = null)
        {
            var encodedFormId = System.Net.WebUtility.HtmlEncode(formId);
            var encodedSummaryId = System.Net.WebUtility.HtmlEncode(IdOf(formId));
            var classes = string.IsNullOrWhiteSpace(cssClass) ? "" : $" class=\"{System.Net.WebUtility.HtmlEncode(cssClass)}\"";
            return $"<div id=\"{encodedSummaryId}\"{classes} data-reactive-validation-summary=\"{encodedFormId}\" hidden></div>";
        }
    }
}
