using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Consumer.Net48.Components
{
    public static class AppButtonHtml
    {
        public static Rendered<AppButton, TModel> AppButton<TModel>(this ReactivePlan<TModel> plan, string id, string text)
        {
            var button = new TagBuilder("button");
            button.MergeAttribute("id", id);
            button.MergeAttribute("type", "button");
            button.SetInnerText(text);
            return plan.Render<AppButton>(id, new MvcHtmlString(button.ToString(TagRenderMode.Normal)));
        }
    }
}
