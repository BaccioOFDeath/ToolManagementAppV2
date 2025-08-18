using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;
using ComboBox = System.Windows.Controls.ComboBox;

namespace ToolManagementAppV2.Utilities.Extensions
{
    /// <summary>
    /// Extension helpers for working with <see cref="ComboBox"/> controls.
    /// </summary>
    public static class ComboBoxExtensions
    {
        /// <summary>
        /// Applies the application's default <see cref="Style"/> to the provided <see cref="ComboBox"/>.
        /// </summary>
        /// <param name="comboBox">The <see cref="ComboBox"/> to style.</param>
        public static void ApplyDefaultStyle(this ComboBox comboBox)
        {
            if (comboBox is null) return;

            var styleReference = Application.Current?.FindResource(typeof(ComboBox));
            if (styleReference is Style style)
                comboBox.Style = style;
        }

        /// <summary>
        /// Creates a new <see cref="ComboBox"/> with the application's default style applied.
        /// </summary>
        /// <returns>A newly created, styled <see cref="ComboBox"/>.</returns>
        public static ComboBox CreateWithDefaultStyle()
        {
            var comboBox = new ComboBox();
            comboBox.ApplyDefaultStyle();
            return comboBox;
        }
    }
}

