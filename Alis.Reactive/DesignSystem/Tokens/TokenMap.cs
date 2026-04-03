using System;
using System.Collections.Generic;

namespace Alis.Reactive.DesignSystem.Tokens
{
    /// <summary>
    /// Maps design-system tokens to the concrete CSS utility classes rendered in markup.
    /// </summary>
    public static class TokenMap
    {
        private static readonly Dictionary<SpacingScale, string> GapMap = new Dictionary<SpacingScale, string>
        {
            { SpacingScale.None, "gap-0" },
            { SpacingScale.Xs, "gap-1" },
            { SpacingScale.Sm, "gap-2" },
            { SpacingScale.Base, "gap-4" },
            { SpacingScale.Md, "gap-6" },
            { SpacingScale.Lg, "gap-8" },
            { SpacingScale.Xl, "gap-10" },
            { SpacingScale.Xxl, "gap-12" },
            { SpacingScale.Max, "gap-16" }
        };

        private static readonly Dictionary<SpacingScale, string> GapXMap = new Dictionary<SpacingScale, string>
        {
            { SpacingScale.None, "gap-x-0" },
            { SpacingScale.Xs, "gap-x-1" },
            { SpacingScale.Sm, "gap-x-2" },
            { SpacingScale.Base, "gap-x-4" },
            { SpacingScale.Md, "gap-x-6" },
            { SpacingScale.Lg, "gap-x-8" },
            { SpacingScale.Xl, "gap-x-10" },
            { SpacingScale.Xxl, "gap-x-12" },
            { SpacingScale.Max, "gap-x-16" }
        };

        private static readonly Dictionary<SpacingScale, string> GapYMap = new Dictionary<SpacingScale, string>
        {
            { SpacingScale.None, "gap-y-0" },
            { SpacingScale.Xs, "gap-y-1" },
            { SpacingScale.Sm, "gap-y-2" },
            { SpacingScale.Base, "gap-y-4" },
            { SpacingScale.Md, "gap-y-6" },
            { SpacingScale.Lg, "gap-y-8" },
            { SpacingScale.Xl, "gap-y-10" },
            { SpacingScale.Xxl, "gap-y-12" },
            { SpacingScale.Max, "gap-y-16" }
        };

        private static readonly Dictionary<SpacingScale, string> PyMap = new Dictionary<SpacingScale, string>
        {
            { SpacingScale.None, "py-0" },
            { SpacingScale.Xs, "py-1" },
            { SpacingScale.Sm, "py-2" },
            { SpacingScale.Base, "py-4" },
            { SpacingScale.Md, "py-6" },
            { SpacingScale.Lg, "py-8" },
            { SpacingScale.Xl, "py-10" },
            { SpacingScale.Xxl, "py-12" },
            { SpacingScale.Max, "py-16" }
        };

        private static readonly Dictionary<AlignItems, string> ItemsMap = new Dictionary<AlignItems, string>
        {
            { AlignItems.Start, "items-start" },
            { AlignItems.Center, "items-center" },
            { AlignItems.End, "items-end" },
            { AlignItems.Stretch, "items-stretch" },
            { AlignItems.Baseline, "items-baseline" }
        };

        private static readonly Dictionary<JustifyContent, string> JustifyMap = new Dictionary<JustifyContent, string>
        {
            { JustifyContent.Start, "justify-start" },
            { JustifyContent.Center, "justify-center" },
            { JustifyContent.End, "justify-end" },
            { JustifyContent.Between, "justify-between" },
            { JustifyContent.Around, "justify-around" },
            { JustifyContent.Evenly, "justify-evenly" }
        };

        private static readonly Dictionary<TextColor, string> ColorMap = new Dictionary<TextColor, string>
        {
            { TextColor.Primary, "text-text-primary" },
            { TextColor.Secondary, "text-text-secondary" },
            { TextColor.Muted, "text-text-muted" },
            { TextColor.Inverse, "text-white" },
            { TextColor.Accent, "text-accent" },
            { TextColor.Success, "text-success" },
            { TextColor.Warning, "text-warning" },
            { TextColor.Error, "text-error" },
            { TextColor.Inherit, "text-inherit" }
        };

        private static readonly Dictionary<TextSize, string> SizeMap = new Dictionary<TextSize, string>
        {
            { TextSize.Xs, "text-xs" },
            { TextSize.Sm, "text-sm" },
            { TextSize.Base, "text-base" },
            { TextSize.Lg, "text-lg" },
            { TextSize.Xl, "text-xl" }
        };

        private static readonly Dictionary<GridCols, string> ColsMap = new Dictionary<GridCols, string>
        {
            { GridCols.C1, "grid-cols-1" },
            { GridCols.C2, "grid-cols-2" },
            { GridCols.C3, "grid-cols-3" },
            { GridCols.C4, "grid-cols-4" },
            { GridCols.C5, "grid-cols-5" },
            { GridCols.C6, "grid-cols-6" }
        };

        private static readonly Dictionary<AccentColor, string> AccentMap = new Dictionary<AccentColor, string>
        {
            { AccentColor.Primary, "border-accent" },
            { AccentColor.Secondary, "border-text-secondary" },
            { AccentColor.Success, "border-success" },
            { AccentColor.Warning, "border-warning" },
            { AccentColor.Error, "border-error" },
            { AccentColor.Info, "border-accent" },
            { AccentColor.Muted, "border-text-muted" }
        };

        /// <summary>Gets the gap utility class for a spacing token.</summary>
        /// <param name="scale">The spacing token to map.</param>
        /// <returns>The corresponding gap utility class.</returns>
        public static string Gap(SpacingScale scale) => GapMap[scale];

        /// <summary>Gets the horizontal gap utility class for a spacing token.</summary>
        /// <param name="scale">The spacing token to map.</param>
        /// <returns>The corresponding horizontal gap utility class.</returns>
        public static string GapX(SpacingScale scale) => GapXMap[scale];

        /// <summary>Gets the vertical gap utility class for a spacing token.</summary>
        /// <param name="scale">The spacing token to map.</param>
        /// <returns>The corresponding vertical gap utility class.</returns>
        public static string GapY(SpacingScale scale) => GapYMap[scale];

        /// <summary>Gets the vertical padding utility class for a spacing token.</summary>
        /// <param name="scale">The spacing token to map.</param>
        /// <returns>The corresponding vertical padding utility class.</returns>
        public static string Py(SpacingScale scale) => PyMap[scale];

        /// <summary>Gets the flex alignment utility class for an alignment token.</summary>
        /// <param name="align">The alignment token to map.</param>
        /// <returns>The corresponding alignment utility class.</returns>
        public static string Items(AlignItems align) => ItemsMap[align];

        /// <summary>Gets the flex justification utility class for a justification token.</summary>
        /// <param name="justify">The justification token to map.</param>
        /// <returns>The corresponding justification utility class.</returns>
        public static string Justify(JustifyContent justify) => JustifyMap[justify];

        /// <summary>Gets the text color utility class for a text color token.</summary>
        /// <param name="color">The text color token to map.</param>
        /// <returns>The corresponding text color utility class.</returns>
        public static string Color(TextColor color) => ColorMap[color];

        /// <summary>Gets the text size utility class for a text size token.</summary>
        /// <param name="size">The text size token to map.</param>
        /// <returns>The corresponding text size utility class.</returns>
        public static string Size(TextSize size) => SizeMap[size];

        /// <summary>Gets the grid column utility class for a column token.</summary>
        /// <param name="cols">The grid column token to map.</param>
        /// <returns>The corresponding grid column utility class.</returns>
        public static string Cols(GridCols cols) => ColsMap[cols];

        /// <summary>Gets the accent border utility class for an accent token.</summary>
        /// <param name="accent">The accent token to map.</param>
        /// <returns>The corresponding accent utility class.</returns>
        public static string Accent(AccentColor accent) => AccentMap[accent];
    }
}
