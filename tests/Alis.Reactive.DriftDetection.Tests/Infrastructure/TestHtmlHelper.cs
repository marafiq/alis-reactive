using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Alis.Reactive.DriftDetection.Tests.Infrastructure;

/// <summary>
/// Minimal IHtmlHelper stub for drift detection tests.
/// PlanExtensions.ReactivePlan() and HtmlExtensions.On() never dereference the html parameter.
/// InputFieldExtensions.InputField() needs NameFor() and ViewContext.Writer.
/// All other members throw — tests should not use them.
/// </summary>
public sealed class TestHtmlHelper<TModel> : IHtmlHelper<TModel>
{
    private static readonly ViewContext TestViewContext = CreateViewContext();

    public ViewContext ViewContext => TestViewContext;

    // InputFieldExtensions uses NameFor to resolve binding path
    public string NameFor<TResult>(Expression<Func<TModel, TResult>> expression)
    {
        // Extract property path from expression (e.g., m => m.Address.Street → "Address.Street")
        return GetExpressionPath(expression);
    }

    private static string GetExpressionPath<TResult>(Expression<Func<TModel, TResult>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression unary)
            body = unary.Operand;

        var parts = new List<string>();
        while (body is MemberExpression member)
        {
            parts.Insert(0, member.Member.Name);
            body = member.Expression!;
        }
        return string.Join(".", parts);
    }

    private static ViewContext CreateViewContext()
    {
        var ctx = new ViewContext { Writer = new StringWriter() };
        return ctx;
    }

    // ── Not implemented — tests should not reach these ──

    public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent AntiForgeryToken() => throw new NotImplementedException();
    public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
    public MvcForm BeginRouteForm(string routeName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent CheckBox(string expression, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
    public string DisplayName(string expression) => throw new NotImplementedException();
    public string DisplayText(string expression) => throw new NotImplementedException();
    public IHtmlContent DropDownList(string expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
    public string Encode(string value) => HtmlEncoder.Default.Encode(value);
    public string Encode(object value) => HtmlEncoder.Default.Encode(value?.ToString() ?? "");
    public void EndForm() => throw new NotImplementedException();
    public string FormatValue(object value, string format) => throw new NotImplementedException();
    public string GenerateIdFromName(string fullName) => fullName.Replace(".", "_");
    public IEnumerable<SelectListItem> GetEnumSelectList<TEnum>() where TEnum : struct => throw new NotImplementedException();
    public IEnumerable<SelectListItem> GetEnumSelectList(Type enumType) => throw new NotImplementedException();
    public IHtmlContent Hidden(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
    public string Id(string expression) => throw new NotImplementedException();
    public IHtmlContent Label(string expression, string labelText, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ListBox(string expression, IEnumerable<SelectListItem> selectList, object htmlAttributes) => throw new NotImplementedException();
    public string Name(string expression) => expression;
    public Task<IHtmlContent> PartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
    public IHtmlContent Password(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent RadioButton(string expression, object value, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent Raw(string value) => new HtmlString(value);
    public IHtmlContent Raw(object value) => new HtmlString(value?.ToString());
    public Task RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
    public IHtmlContent RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextArea(string expression, string value, int rows, int columns, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextBox(string expression, object value, string format, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
    public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
    public string Value(string expression, string format) => throw new NotImplementedException();

    // IHtmlHelper<TModel> typed members
    public IHtmlContent CheckBoxFor(Expression<Func<TModel, bool>> expression, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent DisplayFor<TResult>(Expression<Func<TModel, TResult>> expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
    public string DisplayNameFor<TResult>(Expression<Func<TModel, TResult>> expression) => throw new NotImplementedException();
    public string DisplayNameForInnerType<TModelItem, TResult>(Expression<Func<TModelItem, TResult>> expression) => throw new NotImplementedException();
    public string DisplayTextFor<TResult>(Expression<Func<TModel, TResult>> expression) => throw new NotImplementedException();
    public IHtmlContent DropDownListFor<TResult>(Expression<Func<TModel, TResult>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent EditorFor<TResult>(Expression<Func<TModel, TResult>> expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
    public IHtmlContent HiddenFor<TResult>(Expression<Func<TModel, TResult>> expression, object htmlAttributes) => throw new NotImplementedException();
    public string IdFor<TResult>(Expression<Func<TModel, TResult>> expression) => NameFor(expression).Replace(".", "_");
    public IHtmlContent LabelFor<TResult>(Expression<Func<TModel, TResult>> expression, string labelText, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ListBoxFor<TResult>(Expression<Func<TModel, TResult>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent PasswordFor<TResult>(Expression<Func<TModel, TResult>> expression, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent RadioButtonFor<TResult>(Expression<Func<TModel, TResult>> expression, object value, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextAreaFor<TResult>(Expression<Func<TModel, TResult>> expression, int rows, int columns, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextBoxFor<TResult>(Expression<Func<TModel, TResult>> expression, string format, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ValidationMessageFor<TResult>(Expression<Func<TModel, TResult>> expression, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
    public string ValueFor<TResult>(Expression<Func<TModel, TResult>> expression, string format) => throw new NotImplementedException();

    public Html5DateRenderingMode Html5DateRenderingMode { get; set; }
    public string IdAttributeDotReplacement => "_";
    public IModelMetadataProvider MetadataProvider => throw new NotImplementedException();
    public dynamic ViewBag => throw new NotImplementedException();
    public ViewDataDictionary<TModel> ViewData => throw new NotImplementedException();
    ViewDataDictionary IHtmlHelper.ViewData => throw new NotImplementedException();
    public ITempDataDictionary TempData => throw new NotImplementedException();
    public UrlEncoder UrlEncoder => UrlEncoder.Default;
}
