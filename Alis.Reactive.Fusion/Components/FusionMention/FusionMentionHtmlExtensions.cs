using System;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    public static class FusionMentionHtmlExtensions
    {
        public static FusionMentionBuilder<TModel> FusionMention<TModel>(
            this IHtmlHelper<TModel> html,
            ReactivePlan<TModel> plan,
            string elementId,
            Action<MentionBuilder> build)
            where TModel : class
        {
            var hostId = elementId + "-host";
            var builder = html.EJS().Mention(hostId);
            build(builder);

            var content = new HtmlContentBuilder();
            content.AppendHtml(RenderBridge(elementId, hostId, builder.model.Target));
            content.AppendHtml(builder.Render());

            return new FusionMentionBuilder<TModel>(plan, elementId, content);
        }

        public static MentionBuilder Fields<TItem>(
            this MentionBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MentionFieldSettings
            {
                Text = ToCamelCase(GetMemberName(text)),
                Value = ToCamelCase(GetMemberName(value))
            });
        }

        private static string GetMemberName<T>(Expression<Func<T, object?>> expr)
        {
            var body = expr.Body;
            if (body is UnaryExpression unary) body = unary.Operand;
            if (body is MemberExpression member) return member.Member.Name;
            throw new ArgumentException("Expression must be a member access.");
        }

        private static string ToCamelCase(string name) =>
            string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);

        private static string RenderBridge(string elementId, string hostId, string targetSelector)
        {
            var encodedElementId = JavaScriptEncoder.Default.Encode(elementId);
            var encodedHostId = JavaScriptEncoder.Default.Encode(hostId);
            var encodedTargetSelector = JavaScriptEncoder.Default.Encode(targetSelector);

            return "<span id=\"" + HtmlEncoder.Default.Encode(elementId) + "\" hidden></span>"
                + "<script>(function(){document.addEventListener('DOMContentLoaded',function(){"
                + "var bridge=document.getElementById('" + encodedElementId + "');"
                + "var host=document.getElementById('" + encodedHostId + "');"
                + "var target=document.querySelector('" + encodedTargetSelector + "');"
                + "var source=(host&&host.ej2_instances)?host:target;"
                + "if(bridge&&source&&source.ej2_instances){bridge.ej2_instances=source.ej2_instances;}"
                + "});})();</script>";
        }
    }
}
