﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGridControlFactory.cs" company="PropertyTools">
//   Copyright (c) 2014 PropertyTools contributors
// </copyright>
// <summary>
//   Creates display and edit controls for the DataGrid.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PropertyTools.Wpf
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using HorizontalAlignment = System.Windows.HorizontalAlignment;

    /// <summary>
    /// Creates display and edit controls for the DataGrid.
    /// </summary>
    public class DataGridControlFactory : IDataGridControlFactory
    {
        /// <summary>
        /// Creates the display control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// The control.
        /// </returns>
        public FrameworkElement CreateDisplayControl(CellDefinition d)
        {
            var element = this.CreateDisplayControlOverride(d);
            this.ApplyCommonDisplayControlProperties(d, element);
            return element;
        }

        /// <summary>
        /// Creates the edit control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// The control.
        /// </returns>
        public virtual FrameworkElement CreateEditControl(CellDefinition d)
        {
            var element = this.CreateEditControlOverride(d);
            return element;
        }

        /// <summary>
        /// Creates the display control.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>The display control.</returns>
        protected virtual FrameworkElement CreateDisplayControlOverride(CellDefinition d)
        {
            var cb = d as CheckCellDefinition;
            if (cb != null)
            {
                return this.CreateCheckBoxControl(cb);
            }

            var co = d as ColorCellDefinition;
            if (co != null)
            {
                return this.CreateColorPreviewControl(co);
            }

            var td = d as TemplateCellDefinition;
            if (td != null)
            {
                return this.CreateTemplateControl(td, td.DisplayTemplate);
            }

            return this.CreateTextBlockControl(d);
        }

        /// <summary>
        /// Creates the edit control.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// The control.
        /// </returns>
        protected virtual FrameworkElement CreateEditControlOverride(CellDefinition d)
        {
            var co = d as SelectCellDefinition;
            if (co != null)
            {
                return this.CreateComboBox(co);
            }

            var cb = d as CheckCellDefinition;
            if (cb != null)
            {
                return null;
            }

            var col = d as ColorCellDefinition;
            if (col != null)
            {
                return this.CreateColorPickerControl(col);
            }

            var td = d as TemplateCellDefinition;
            if (td != null)
            {
                return this.CreateTemplateControl(td, td.EditTemplate ?? td.DisplayTemplate);
            }

            var te = d as TextCellDefinition;
            if (te != null)
            {
                return this.CreateTextBox(te);
            }

            return this.CreateDisplayControl(d);
        }

        /// <summary>
        /// Creates a binding.
        /// </summary>
        /// <param name="d">The cd.</param>
        /// <returns>
        /// A binding.
        /// </returns>
        protected Binding CreateBinding(CellDefinition d)
        {
            var bindingMode = d.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            var formatString = d.FormatString;
            if (formatString != null && !formatString.StartsWith("{"))
            {
                formatString = "{0:" + formatString + "}";
            }

            var binding = new Binding(d.BindingPath)
            {
                Mode = bindingMode,
                Converter = d.Converter,
                ConverterParameter = d.ConverterParameter,
                StringFormat = formatString,
                ValidatesOnDataErrors = true,
                ValidatesOnExceptions = true,
                NotifyOnSourceUpdated = true
            };
            if (d.ConverterCulture != null)
            {
                binding.ConverterCulture = d.ConverterCulture;
            }

            return binding;
        }

        /// <summary>
        /// Creates the one way binding.
        /// </summary>
        /// <param name="cd">The cell definition.</param>
        /// <returns>
        /// A binding.
        /// </returns>
        protected Binding CreateOneWayBinding(CellDefinition cd)
        {
            var b = this.CreateBinding(cd);
            b.Mode = BindingMode.OneWay;
            return b;
        }

        /// <summary>
        /// Creates a check box control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A CheckBox.
        /// </returns>
        protected virtual FrameworkElement CreateCheckBoxControl(CellDefinition d)
        {
            if (d.IsReadOnly)
            {
                var cm = new CheckMark
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = d.HorizontalAlignment,
                    Background = d.Background
                };
                cm.SetBinding(CheckMark.IsCheckedProperty, this.CreateBinding(d));
                return cm;
            }

            var c = new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = d.HorizontalAlignment,
                Background = d.Background,
                IsEnabled = !d.IsReadOnly
            };
            c.SetBinding(ToggleButton.IsCheckedProperty, this.CreateBinding(d));
            return c;
        }

        /// <summary>
        /// Creates a color picker control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A color picker.
        /// </returns>
        protected virtual FrameworkElement CreateColorPickerControl(CellDefinition d)
        {
            var c = new ColorPicker
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = d.Background,
                Focusable = false
            };
            c.SetBinding(ColorPicker.SelectedColorProperty, this.CreateBinding(d));
            return c;
        }

        /// <summary>
        /// Creates a color preview control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A preview control.
        /// </returns>
        protected virtual FrameworkElement CreateColorPreviewControl(ColorCellDefinition d)
        {
            var c = new Rectangle
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Width = 12,
                Height = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var binding = this.CreateBinding(d);
            binding.Converter = new ColorToBrushConverter();
            c.SetBinding(Shape.FillProperty, binding);

            var grid = new Grid { Background = d.Background };
            grid.Children.Add(c);
            return grid;
        }

        /// <summary>
        /// Creates a combo box with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A ComboBox.
        /// </returns>
        protected virtual FrameworkElement CreateComboBox(SelectCellDefinition d)
        {
            var c = new ComboBox
            {
                IsEditable = d.IsEditable,
                Focusable = false,
                Margin = new Thickness(1, 1, 0, 0),
                HorizontalContentAlignment = d.HorizontalAlignment,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = d.Background,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0)
            };
            if (d.ItemsSource != null)
            {
                c.ItemsSource = d.ItemsSource;
            }
            else
            {
                if (d.ItemsSourceProperty != null)
                {
                    var itemsSourceBinding = new Binding(d.ItemsSourceProperty);
                    c.SetBinding(ItemsControl.ItemsSourceProperty, itemsSourceBinding);
                }
            }

            c.DropDownClosed += (s, e) => FocusParentDataGrid(c);
            var binding = this.CreateBinding(d);
            binding.NotifyOnSourceUpdated = true;
            c.SetBinding(d.IsEditable ? ComboBox.TextProperty : Selector.SelectedValueProperty, binding);
            c.SelectedValuePath = d.SelectedValuePath;

            return c;
        }

        /// <summary>
        /// Creates a text block control with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A TextBlock.
        /// </returns>
        protected virtual FrameworkElement CreateTextBlockControl(CellDefinition d)
        {
            var tb = new TextBlock
            {
                HorizontalAlignment = d.HorizontalAlignment,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(4, 0, 4, 0)
            };

            tb.SetBinding(TextBlock.TextProperty, this.CreateOneWayBinding(d));
            var grid = new Grid { Background = d.Background };
            grid.Children.Add(tb);
            return grid;
        }

        /// <summary>
        /// Creates a text box with data binding.
        /// </summary>
        /// <param name="d">The cell definition.</param>
        /// <returns>
        /// A TextBox.
        /// </returns>
        protected virtual FrameworkElement CreateTextBox(TextCellDefinition d)
        {
            var tb = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = d.HorizontalAlignment,
                MaxLength = d.MaxLength,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(1, 1, 0, 0),
                Background = d.Background
            };

            tb.SetBinding(TextBox.TextProperty, this.CreateBinding(d));
            return tb;
        }

        /// <summary>
        /// Creates the template control.
        /// </summary>
        /// <param name="d">The definition.</param>
        /// <param name="template">The data template.</param>
        /// <returns>A content control.</returns>
        protected virtual FrameworkElement CreateTemplateControl(TemplateCellDefinition d, DataTemplate template)
        {
            var element = (FrameworkElement)template.LoadContent();
            var binding = this.CreateBinding(d);
            binding.Mode = BindingMode.OneWay;
            var contentControl = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = d.Background,
                Content = element
            };
            element.SetBinding(FrameworkElement.DataContextProperty, binding);
            return contentControl;
        }

        /// <summary>
        /// Applies common display control properties.
        /// </summary>
        /// <param name="cd">The cell definition.</param>
        /// <param name="element">The element.</param>
        protected virtual void ApplyCommonDisplayControlProperties(CellDefinition cd, FrameworkElement element)
        {
            if (cd.IsEnabledByProperty != null)
            {
                element.SetIsEnabledBinding(cd.IsEnabledByProperty, cd.IsEnabledByValue);
            }
        }

        /// <summary>
        /// Focuses on the parent data grid.
        /// </summary>
        /// <param name="obj">The <see cref="DependencyObject" />.</param>
        private static void FocusParentDataGrid(DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && !(parent is DataGrid))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var u = parent as UIElement;
            if (u != null)
            {
                u.Focus();
            }
        }
    }
}
