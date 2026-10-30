namespace Alis.Reactive.Validation
{
    /// <summary>
    /// The summary of a form: the element that shows the messages no field of the form can show (a hidden field's,
    /// a member no input renders), found by the form's element id. The server renders it once per request: where the
    /// developer places it, else after the first plan that validates the form or shows its errors.
    /// </summary>
    internal static class FormSummaries
    {
        /// <summary>The summary's element id: <c>{formId}_validation_summary</c>.</summary>
        internal static string IdOf(string formId) => formId + "_validation_summary";

        /// <summary>The summary's markup, with the classes the page gives it: hidden until a message goes into it.</summary>
        internal static string Markup(string formId, string? cssClass = null)
        {
            var encodedFormId = System.Net.WebUtility.HtmlEncode(formId);
            var encodedSummaryId = System.Net.WebUtility.HtmlEncode(IdOf(formId));
            var classes = string.IsNullOrWhiteSpace(cssClass) ? "" : $" class=\"{System.Net.WebUtility.HtmlEncode(cssClass)}\"";
            return $"<div id=\"{encodedSummaryId}\"{classes} data-reactive-validation-summary=\"{encodedFormId}\" hidden></div>";
        }
    }
}
