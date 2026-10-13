using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates typed <see cref="FusionGrid"/> cell editors rendered as Syncfusion
    /// <c>EditTemplate</c> markup.
    /// </summary>
    /// <remarks>
    /// Select templates own their option data, so they work under custom server binding
    /// where Syncfusion's built-in <c>dropdownedit</c> cannot read distinct values from
    /// the remote data source.
    /// Example: <c>FusionGridEditTemplates.Select((ResidentRow r) =&gt; r.CareLevel, careLevels)</c>.
    /// </remarks>
    public static class FusionGridEditTemplates
    {
        /// <summary>
        /// Creates a single-select cell editor bound to a typed row field and populated from a string list.
        /// </summary>
        /// <typeparam name="TRow">Your row class.</typeparam>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>(ResidentRow r) =&gt; r.CareLevel</c>.</param>
        /// <param name="options">The option texts; each is also the option's value.</param>
        /// <returns>The editor markup for the column's <c>EditTemplate</c>.</returns>
        public static string Select<TRow, TField>(
            Expression<Func<TRow, TField>> field,
            IEnumerable<string> options)
            where TRow : class
        {
            var fieldName = Field.Of(field).Name;
            return BuildSelect(fieldName, options.Select(o => (o, o)));
        }

        /// <summary>
        /// Creates a single-select cell editor over a typed option list with text/value selectors.
        /// </summary>
        /// <typeparam name="TRow">Your row class.</typeparam>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <typeparam name="TItem">The type of an option item.</typeparam>
        /// <param name="field">The edited member, for example <c>(ResidentRow r) =&gt; r.CareLevel</c>.</param>
        /// <param name="items">The option items.</param>
        /// <param name="text">Reads an option's display text from its item.</param>
        /// <param name="value">Reads an option's value from its item.</param>
        /// <returns>The editor markup for the column's <c>EditTemplate</c>.</returns>
        public static string Select<TRow, TField, TItem>(
            Expression<Func<TRow, TField>> field,
            IEnumerable<TItem> items,
            Func<TItem, string> text,
            Func<TItem, string> value)
            where TRow : class
        {
            var fieldName = Field.Of(field).Name;
            return BuildSelect(fieldName, items.Select(i => (text(i), value(i))));
        }

        /// <summary>
        /// Creates a native date cell editor bound to a typed row field.
        /// </summary>
        /// <typeparam name="TRow">Your row class.</typeparam>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>(ResidentRow r) =&gt; r.NextReviewDate</c>.</param>
        /// <returns>The editor markup for the column's <c>EditTemplate</c>.</returns>
        public static string DateInput<TRow, TField>(Expression<Func<TRow, TField>> field)
            where TRow : class
        {
            var fieldName = Field.Of(field).Name;
            var name = HtmlEncoder.Default.Encode(fieldName);
            // value="${field}" binds the editing row's current value through the Syncfusion
            // template engine — the built-in editor leaves native date inputs unset.
            return "<input type=\"date\" name=\"" + name + "\" value=\"${" + fieldName
                + "}\" class=\"e-field e-input\" style=\"width:100%\" />";
        }

        /// <summary>
        /// Creates a typed dialog-edit form template for <c>GridEditSettings.Template</c>
        /// in <see cref="Syncfusion.EJ2.Grids.EditMode"/> Dialog mode.
        /// </summary>
        /// <remarks>
        /// Each field is declared from a typed row expression, avoiding stringly field names
        /// and raw HTML in the view.
        /// Example: <c>FusionGridEditTemplates.DialogForm&lt;ResidentRow&gt;(d =&gt; d.Text(r =&gt; r.Name, "Name"))</c>.
        /// </remarks>
        /// <typeparam name="TRow">Your row class.</typeparam>
        /// <param name="build">Adds the form's labeled fields.</param>
        /// <returns>The form markup for <c>GridEditSettings.Template</c>.</returns>
        public static string DialogForm<TRow>(Action<FusionGridDialogFormBuilder<TRow>> build)
            where TRow : class
        {
            if (build == null) throw new ArgumentNullException(nameof(build));
            var builder = new FusionGridDialogFormBuilder<TRow>();
            build(builder);
            return builder.Render();
        }

        private static string BuildSelect(string fieldName, IEnumerable<(string Text, string Value)> options)
        {
            var encoder = HtmlEncoder.Default;
            var html = new StringBuilder();
            html.Append("<select name=\"")
              .Append(encoder.Encode(fieldName))
              .Append("\" class=\"e-field e-input\" style=\"width:100%\">");

            foreach (var (text, value) in options)
            {
                html.Append("<option value=\"")
                  .Append(encoder.Encode(value))
                  .Append("\">")
                  .Append(encoder.Encode(text))
                  .Append("</option>");
            }

            html.Append("</select>");
            return html.ToString();
        }
    }

    /// <summary>
    /// Builds a typed dialog-edit form for a <see cref="FusionGrid"/> row. Each labeled
    /// field is declared from a typed row expression; the editing row's current value
    /// binds through the Syncfusion <c>${field}</c> template token.
    /// </summary>
    /// <typeparam name="TRow">Your row class.</typeparam>
    public sealed class FusionGridDialogFormBuilder<TRow>
        where TRow : class
    {
        private readonly List<string> _fields = new List<string>();

        internal FusionGridDialogFormBuilder() { }

        /// <summary>Adds a labeled text field bound to a typed row field.</summary>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>r =&gt; r.Name</c>.</param>
        /// <param name="label">The label shown above the field.</param>
        /// <returns>This builder, for the next field.</returns>
        public FusionGridDialogFormBuilder<TRow> Text<TField>(Expression<Func<TRow, TField>> field, string label) =>
            AddField(field, label, name => "<input name=\"" + name + "\" value=\"${" + name + "}\" class=\"e-field e-input\" />");

        /// <summary>Adds a labeled numeric field bound to a typed row field.</summary>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>r =&gt; r.OpenTasks</c>.</param>
        /// <param name="label">The label shown above the field.</param>
        /// <returns>This builder, for the next field.</returns>
        public FusionGridDialogFormBuilder<TRow> Number<TField>(Expression<Func<TRow, TField>> field, string label) =>
            AddField(field, label, name => "<input type=\"number\" name=\"" + name + "\" value=\"${" + name + "}\" class=\"e-field e-input\" />");

        /// <summary>Adds a labeled date field bound to a typed row field.</summary>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>r =&gt; r.NextReviewDate</c>.</param>
        /// <param name="label">The label shown above the field.</param>
        /// <returns>This builder, for the next field.</returns>
        public FusionGridDialogFormBuilder<TRow> Date<TField>(Expression<Func<TRow, TField>> field, string label) =>
            AddField(field, label, name => "<input type=\"date\" name=\"" + name + "\" value=\"${" + name + "}\" class=\"e-field e-input\" />");

        /// <summary>Adds a labeled single-select field bound to a typed row field.</summary>
        /// <typeparam name="TField">The type of the edited member.</typeparam>
        /// <param name="field">The edited member, for example <c>r =&gt; r.CareLevel</c>.</param>
        /// <param name="label">The label shown above the field.</param>
        /// <param name="options">The option texts; each is also the option's value, and empty texts are skipped.</param>
        /// <returns>This builder, for the next field.</returns>
        public FusionGridDialogFormBuilder<TRow> Select<TField>(
            Expression<Func<TRow, TField>> field, string label, IEnumerable<string> options) =>
            AddField(field, label, name => BuildSelect(name, options));

        private FusionGridDialogFormBuilder<TRow> AddField<TField>(
            Expression<Func<TRow, TField>> field, string label, Func<string, string> editor)
        {
            var name = HtmlEncoder.Default.Encode(Field.Of(field).Name);
            _fields.Add(
                "<label class=\"grid gap-1\"><span class=\"font-medium text-text\">"
                + HtmlEncoder.Default.Encode(label) + "</span>" + editor(name) + "</label>");
            return this;
        }

        private static string BuildSelect(string name, IEnumerable<string> options)
        {
            var encoder = HtmlEncoder.Default;
            var html = new StringBuilder();
            html.Append("<select name=\"").Append(name).Append("\" class=\"e-field e-input\">");
            foreach (var option in options)
            {
                if (string.IsNullOrEmpty(option)) continue;
                var encoded = encoder.Encode(option);
                html.Append("<option value=\"").Append(encoded).Append("\">").Append(encoded).Append("</option>");
            }
            html.Append("</select>");
            return html.ToString();
        }

        internal string Render()
        {
            var html = new StringBuilder();
            html.Append("<div class=\"grid grid-cols-1 gap-3 p-2 text-sm\">");
            foreach (var field in _fields) html.Append(field);
            html.Append("</div>");
            return html.ToString();
        }
    }
}
