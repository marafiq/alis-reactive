using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionRating;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionRating inside a field wrapper, bound to a numeric model property.
    /// </summary>
    public static class FusionRatingHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionRating bound to the field's model property.
        /// </summary>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial Rating options before rendering.</param>
        public static void FusionRating<TModel>(
            this InputBoundField<TModel, double> setup,
            Action<RatingBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().Rating(setup.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
