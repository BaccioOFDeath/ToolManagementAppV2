using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.DependencyInjection;
using TextBox = System.Windows.Controls.TextBox;

namespace InventoryManagementApp.Views.Pages
{
    public class ThemeDesignerControl : UserControl
    {
        private readonly TextBlock _statusText;
        private readonly StackPanel _editorPanel;
        private ISettingsService? _settingsService;
        private IThemeService? _themeService;
        private IFileDialogService? _fileDialogService;
        private AppThemeSettings _settings = AppThemeSettings.CreateDefault();
        private bool _loading;

        public ThemeDesignerControl()
        {
            _statusText = new TextBlock
            {
                Text = "Theme designer ready",
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            _statusText.SetResourceReference(StyleProperty, "CaptionTextBlock");

            _editorPanel = new StackPanel();
            Content = BuildLayout();
            Loaded += ThemeDesignerControl_Loaded;
        }

        private async void ThemeDesignerControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_settingsService != null)
                return;

            if (Application.Current is not App app)
            {
                _statusText.Text = "Theme services are not available until the application host has started.";
                return;
            }

            _settingsService = app.Host.Services.GetService<ISettingsService>();
            _themeService = app.Host.Services.GetService<IThemeService>();
            _fileDialogService = app.Host.Services.GetService<IFileDialogService>();
            await LoadThemeAsync();
        }

        private UIElement BuildLayout()
        {
            var root = new DockPanel();
            var header = CreateActionStrip();
            DockPanel.SetDock(header, Dock.Top);
            root.Children.Add(header);

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var grid = new Grid { Margin = new Thickness(14) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 520 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star), MinWidth = 300 });

            _editorPanel.Children.Add(CreatePaletteSection());
            _editorPanel.Children.Add(CreateTransparencySection());
            _editorPanel.Children.Add(CreateShapeSection());
            _editorPanel.Children.Add(CreateTypographySection());
            _editorPanel.Children.Add(CreateBackgroundSection());
            Grid.SetColumn(_editorPanel, 0);
            grid.Children.Add(_editorPanel);

            var preview = CreatePreviewPanel();
            Grid.SetColumn(preview, 2);
            grid.Children.Add(preview);

            scroll.Content = grid;
            root.Children.Add(scroll);
            return root;
        }

        private Border CreateActionStrip()
        {
            var border = new Border();
            border.SetResourceReference(StyleProperty, "DesktopSectionActionStrip");

            var dock = new DockPanel { LastChildFill = false };
            var title = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var heading = new TextBlock { Text = "Theme Designer", TextWrapping = TextWrapping.Wrap };
            heading.SetResourceReference(StyleProperty, "SubheadingTextBlock");
            var caption = new TextBlock { Text = "Redesign app colors, backgrounds, transparency, borders, corners, shadows, type, density, and interaction feel from one admin surface.", TextWrapping = TextWrapping.Wrap };
            caption.SetResourceReference(StyleProperty, "CaptionTextBlock");
            title.Children.Add(heading);
            title.Children.Add(caption);
            DockPanel.SetDock(title, Dock.Left);
            dock.Children.Add(title);

            var actions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            DockPanel.SetDock(actions, Dock.Right);
            actions.Children.Add(CreateButton("Apply", (_, _) => ApplyCurrentTheme(), "GhostButton"));
            actions.Children.Add(CreateButton("Save Theme", async (_, _) => await SaveThemeAsync(), "PrimaryButton"));
            actions.Children.Add(CreateButton("Reset", (_, _) => ResetTheme(), "GhostButton"));
            dock.Children.Add(actions);
            border.Child = dock;
            return border;
        }

        private Border CreatePaletteSection()
        {
            var panel = CreateSection("Colors", "Use ARGB hex values such as #FF2563EB. The alpha channel is also supported for transparent colors.");
            var form = (Grid)((StackPanel)panel.Child).Children[2];
            AddCombo(form, "Base theme", () => _settings.BaseTheme, value => _settings.BaseTheme = value, new[] { "Light", "Dark" });
            AddTextBox(form, "App background", () => _settings.BackgroundColor, value => _settings.BackgroundColor = value);
            AddTextBox(form, "Surface", () => _settings.SurfaceColor, value => _settings.SurfaceColor = value);
            AddTextBox(form, "Surface alternate", () => _settings.SurfaceAltColor, value => _settings.SurfaceAltColor = value);
            AddTextBox(form, "Navigation", () => _settings.NavigationColor, value => _settings.NavigationColor = value);
            AddTextBox(form, "Inputs", () => _settings.InputColor, value => _settings.InputColor = value);
            AddTextBox(form, "Buttons", () => _settings.ButtonColor, value => _settings.ButtonColor = value);
            AddTextBox(form, "Borders", () => _settings.BorderColor, value => _settings.BorderColor = value);
            AddTextBox(form, "Text", () => _settings.TextColor, value => _settings.TextColor = value);
            AddTextBox(form, "Muted text", () => _settings.MutedTextColor, value => _settings.MutedTextColor = value);
            AddTextBox(form, "Accent", () => _settings.AccentColor, value => _settings.AccentColor = value);
            AddTextBox(form, "Success", () => _settings.SuccessColor, value => _settings.SuccessColor = value);
            AddTextBox(form, "Warning", () => _settings.WarningColor, value => _settings.WarningColor = value);
            AddTextBox(form, "Error", () => _settings.ErrorColor, value => _settings.ErrorColor = value);
            AddTextBox(form, "Shadow", () => _settings.ShadowColor, value => _settings.ShadowColor = value);
            return panel;
        }

        private Border CreateTransparencySection()
        {
            var panel = CreateSection("Transparency", "Lower opacity values let the app background image or color show through surfaces and controls.");
            var form = (Grid)((StackPanel)panel.Child).Children[2];
            AddSlider(form, "Background opacity", () => _settings.BackgroundOpacity, value => _settings.BackgroundOpacity = value, 0, 1);
            AddSlider(form, "Overlay opacity", () => _settings.BackgroundOverlayOpacity, value => _settings.BackgroundOverlayOpacity = value, 0, 1);
            AddSlider(form, "Surface opacity", () => _settings.SurfaceOpacity, value => _settings.SurfaceOpacity = value, 0, 1);
            AddSlider(form, "Surface alt opacity", () => _settings.SurfaceAltOpacity, value => _settings.SurfaceAltOpacity = value, 0, 1);
            AddSlider(form, "Input opacity", () => _settings.InputOpacity, value => _settings.InputOpacity = value, 0, 1);
            AddSlider(form, "Button opacity", () => _settings.ButtonOpacity, value => _settings.ButtonOpacity = value, 0, 1);
            AddSlider(form, "Navigation opacity", () => _settings.NavigationOpacity, value => _settings.NavigationOpacity = value, 0, 1);
            AddSlider(form, "Header opacity", () => _settings.HeaderOpacity, value => _settings.HeaderOpacity = value, 0, 1);
            AddSlider(form, "Menu opacity", () => _settings.MenuOpacity, value => _settings.MenuOpacity = value, 0, 1);
            AddSlider(form, "Footer opacity", () => _settings.FooterOpacity, value => _settings.FooterOpacity = value, 0, 1);
            AddSlider(form, "Dialog opacity", () => _settings.DialogOpacity, value => _settings.DialogOpacity = value, 0, 1);
            AddCheckBox(form, "Glass surfaces", () => _settings.UseGlassSurfaces, value => _settings.UseGlassSurfaces = value);
            return panel;
        }

        private Border CreateShapeSection()
        {
            var panel = CreateSection("Borders, Corners, and Shadows", "Remove borders completely, square the app off, or push it toward softer raised surfaces.");
            var form = (Grid)((StackPanel)panel.Child).Children[2];
            AddCheckBox(form, "Show borders", () => _settings.BordersVisible, value => _settings.BordersVisible = value);
            AddSlider(form, "Border opacity", () => _settings.BorderOpacity, value => _settings.BorderOpacity = value, 0, 1);
            AddSlider(form, "Border thickness", () => _settings.BorderThickness, value => _settings.BorderThickness = value, 0, 6);
            AddSlider(form, "Control border", () => _settings.ControlBorderThickness, value => _settings.ControlBorderThickness = value, 0, 6);
            AddSlider(form, "Divider strength", () => _settings.DividerOpacity, value => _settings.DividerOpacity = value, 0, 1);
            AddSlider(form, "Card corners", () => _settings.CardCornerRadius, value => _settings.CardCornerRadius = value, 0, 32);
            AddSlider(form, "Panel corners", () => _settings.PanelCornerRadius, value => _settings.PanelCornerRadius = value, 0, 32);
            AddSlider(form, "Button corners", () => _settings.ButtonCornerRadius, value => _settings.ButtonCornerRadius = value, 0, 32);
            AddSlider(form, "Input corners", () => _settings.InputCornerRadius, value => _settings.InputCornerRadius = value, 0, 32);
            AddCheckBox(form, "Surface shadows", () => _settings.EnableSurfaceShadows, value => _settings.EnableSurfaceShadows = value);
            AddCheckBox(form, "Control shadows", () => _settings.EnableControlShadows, value => _settings.EnableControlShadows = value);
            AddSlider(form, "Shadow blur", () => _settings.ShadowBlurRadius, value => _settings.ShadowBlurRadius = value, 0, 48);
            AddSlider(form, "Shadow depth", () => _settings.ShadowDepth, value => _settings.ShadowDepth = value, 0, 16);
            AddSlider(form, "Shadow opacity", () => _settings.ShadowOpacity, value => _settings.ShadowOpacity = value, 0, 1);
            AddSlider(form, "Shadow direction", () => _settings.ShadowDirection, value => _settings.ShadowDirection = value, 0, 360);
            AddSlider(form, "Surface shadow scale", () => _settings.SurfaceShadowScale, value => _settings.SurfaceShadowScale = value, 0, 3);
            AddSlider(form, "Control shadow scale", () => _settings.ControlShadowScale, value => _settings.ControlShadowScale = value, 0, 3);
            return panel;
        }

        private Border CreateTypographySection()
        {
            var panel = CreateSection("Type and Density", "Tune compact workbench layouts or create a larger touch-friendly workstation setup.");
            var form = (Grid)((StackPanel)panel.Child).Children[2];
            AddTextBox(form, "Font family", () => _settings.FontFamily, value => _settings.FontFamily = value);
            AddSlider(form, "Body font scale", () => _settings.FontScale, value => _settings.FontScale = value, 0.75, 1.4);
            AddSlider(form, "Heading scale", () => _settings.HeadingFontScale, value => _settings.HeadingFontScale = value, 0.75, 1.6);
            AddSlider(form, "Page padding", () => _settings.PagePadding, value => _settings.PagePadding = value, 0, 28);
            AddSlider(form, "Card padding", () => _settings.CardPadding, value => _settings.CardPadding = value, 0, 32);
            AddSlider(form, "Control height", () => _settings.ControlHeight, value => _settings.ControlHeight = value, 22, 44);
            AddSlider(form, "Grid row height", () => _settings.DataGridRowHeight, value => _settings.DataGridRowHeight = value, 22, 52);
            AddSlider(form, "Grid header height", () => _settings.DataGridHeaderHeight, value => _settings.DataGridHeaderHeight = value, 24, 56);
            AddSlider(form, "Grid line opacity", () => _settings.GridLineOpacity, value => _settings.GridLineOpacity = value, 0, 1);
            AddSlider(form, "Focus ring", () => _settings.FocusRingOpacity, value => _settings.FocusRingOpacity = value, 0, 1);
            AddSlider(form, "Interaction intensity", () => _settings.InteractionIntensity, value => _settings.InteractionIntensity = value, 0, 2);
            AddSlider(form, "Motion intensity", () => _settings.MotionIntensity, value => _settings.MotionIntensity = value, 0, 2);
            AddSlider(form, "Disabled opacity", () => _settings.DisabledOpacity, value => _settings.DisabledOpacity = value, 0.15, 1);
            return panel;
        }

        private Border CreateBackgroundSection()
        {
            var panel = CreateSection("Backgrounds", "Use a color-only shell or choose a workstation image that shows through transparent surfaces.");
            var form = (Grid)((StackPanel)panel.Child).Children[2];
            AddTextBox(form, "Background image", () => _settings.BackgroundImagePath, value => _settings.BackgroundImagePath = value);
            AddCombo(form, "Image fit", () => _settings.BackgroundImageStretch, value => _settings.BackgroundImageStretch = value, new[] { "UniformToFill", "Uniform", "Fill", "None" });
            AddTextBox(form, "Overlay color", () => _settings.BackgroundOverlayColor, value => _settings.BackgroundOverlayColor = value);

            var row = AddRow(form, "Browse image");
            var button = CreateButton("Choose Image", (_, _) => BrowseBackgroundImage(), "GhostButton");
            Grid.SetRow(button, row);
            Grid.SetColumn(button, 1);
            form.Children.Add(button);
            return panel;
        }

        private Border CreatePreviewPanel()
        {
            var panel = new StackPanel();
            var card = new Border { Margin = new Thickness(0, 0, 0, 12), Padding = new Thickness(18) };
            card.SetResourceReference(StyleProperty, "DesktopSummaryCard");
            var stack = new StackPanel();
            var title = new TextBlock { Text = "Live preview", TextWrapping = TextWrapping.Wrap };
            title.SetResourceReference(StyleProperty, "HeadingTextBlock");
            var caption = new TextBlock { Text = "The preview uses the same app resources as the rest of the workstation. Apply updates immediately; save when the design is right.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 10) };
            caption.SetResourceReference(StyleProperty, "CaptionTextBlock");
            var sampleButton = new Button { Content = "Sample action", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8) };
            sampleButton.SetResourceReference(StyleProperty, "PrimaryButton");
            var sampleInput = new TextBox { Text = "Transparent input surface", Margin = new Thickness(0, 0, 0, 8) };
            var note = new Border { Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8) };
            note.SetResourceReference(StyleProperty, "DesktopNoteCard");
            note.Child = new TextBlock { Text = "Cards, buttons, inputs, menus, grids, and navigation all read these shared theme tokens.", TextWrapping = TextWrapping.Wrap };
            stack.Children.Add(title);
            stack.Children.Add(caption);
            stack.Children.Add(sampleButton);
            stack.Children.Add(sampleInput);
            stack.Children.Add(note);
            card.Child = stack;
            panel.Children.Add(card);

            var handoff = new Border { Padding = new Thickness(14) };
            handoff.SetResourceReference(StyleProperty, "DesktopNoteCard");
            var handoffStack = new StackPanel();
            var h = new TextBlock { Text = "Theme coverage", TextWrapping = TextWrapping.Wrap };
            h.SetResourceReference(StyleProperty, "SectionHeader");
            handoffStack.Children.Add(h);
            handoffStack.Children.Add(new TextBlock { Text = "Colors, transparent surfaces, app background image, border removal, button and input roundness, shadow depth, font scale, grid density, and focus/interaction strength are all saved into the shared AppThemeSettings profile.", TextWrapping = TextWrapping.Wrap });
            handoff.Child = handoffStack;
            panel.Children.Add(handoff);
            panel.Children.Add(_statusText);

            return new Border { Child = panel };
        }

        private Border CreateSection(string title, string caption)
        {
            var border = new Border { Margin = new Thickness(0, 0, 0, 12) };
            border.SetResourceReference(StyleProperty, "DesktopInsetCard");
            var stack = new StackPanel();
            var heading = new TextBlock { Text = title, TextWrapping = TextWrapping.Wrap };
            heading.SetResourceReference(StyleProperty, "PageTitleTextBlock");
            var description = new TextBlock { Text = caption, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 10) };
            description.SetResourceReference(StyleProperty, "CaptionTextBlock");
            var form = new Grid();
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
            form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            stack.Children.Add(heading);
            stack.Children.Add(description);
            stack.Children.Add(form);
            border.Child = stack;
            return border;
        }

        private int AddRow(Grid grid, string label)
        {
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 8) };
            text.SetResourceReference(StyleProperty, "LabelTextBlock");
            Grid.SetRow(text, row);
            grid.Children.Add(text);
            return row;
        }

        private void AddTextBox(Grid grid, string label, Func<string> get, Action<string> set)
        {
            var row = AddRow(grid, label);
            var box = new TextBox { Text = get(), Margin = new Thickness(8, 0, 0, 8) };
            box.LostFocus += (_, _) => { set(box.Text); ApplyCurrentTheme(); };
            box.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    set(box.Text);
                    ApplyCurrentTheme();
                    e.Handled = true;
                }
            };
            RegisterRefresh(() => box.Text = get());
            Grid.SetRow(box, row);
            Grid.SetColumn(box, 1);
            grid.Children.Add(box);
        }

        private void AddCombo(Grid grid, string label, Func<string> get, Action<string> set, string[] options)
        {
            var row = AddRow(grid, label);
            var combo = new ComboBox { ItemsSource = options, SelectedItem = get(), Margin = new Thickness(8, 0, 0, 8) };
            combo.SetResourceReference(ItemsControl.ItemContainerStyleProperty, "DropdownItemStyle");
            combo.SelectionChanged += (_, _) =>
            {
                if (combo.SelectedItem is string selected)
                {
                    set(selected);
                    ApplyCurrentTheme();
                }
            };
            RegisterRefresh(() => combo.SelectedItem = get());
            Grid.SetRow(combo, row);
            Grid.SetColumn(combo, 1);
            grid.Children.Add(combo);
        }

        private void AddCheckBox(Grid grid, string label, Func<bool> get, Action<bool> set)
        {
            var row = AddRow(grid, label);
            var checkBox = new CheckBox { IsChecked = get(), Margin = new Thickness(8, 0, 0, 8), VerticalAlignment = VerticalAlignment.Center };
            checkBox.Checked += (_, _) => { set(true); ApplyCurrentTheme(); };
            checkBox.Unchecked += (_, _) => { set(false); ApplyCurrentTheme(); };
            RegisterRefresh(() => checkBox.IsChecked = get());
            Grid.SetRow(checkBox, row);
            Grid.SetColumn(checkBox, 1);
            grid.Children.Add(checkBox);
        }

        private void AddSlider(Grid grid, string label, Func<double> get, Action<double> set, double min, double max)
        {
            var row = AddRow(grid, label);
            var dock = new DockPanel { Margin = new Thickness(8, 0, 0, 8) };
            var valueText = new TextBlock { Width = 48, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right };
            var slider = new Slider { Minimum = min, Maximum = max, Value = get(), TickFrequency = (max - min) / 8, IsSnapToTickEnabled = false, VerticalAlignment = VerticalAlignment.Center };
            BindingOperations.SetBinding(valueText, TextBlock.TextProperty, new Binding("Value") { Source = slider, StringFormat = "{0:0.##}" });
            slider.ValueChanged += (_, _) =>
            {
                if (_loading)
                    return;
                set(Math.Round(slider.Value, 2));
                ApplyCurrentTheme();
            };
            RegisterRefresh(() => slider.Value = get());
            DockPanel.SetDock(valueText, Dock.Right);
            dock.Children.Add(valueText);
            dock.Children.Add(slider);
            Grid.SetRow(dock, row);
            Grid.SetColumn(dock, 1);
            grid.Children.Add(dock);
        }

        private event Action? RefreshEditors;

        private void RegisterRefresh(Action refresh) => RefreshEditors += refresh;

        private Button CreateButton(string content, RoutedEventHandler click, string styleKey)
        {
            var button = new Button { Content = content, Margin = new Thickness(0, 0, 6, 0), MinWidth = 96 };
            button.SetResourceReference(StyleProperty, styleKey);
            button.Click += click;
            return button;
        }

        private async Task LoadThemeAsync(CancellationToken token = default)
        {
            if (_settingsService == null)
                return;

            _loading = true;
            try
            {
                _settings = await _settingsService.GetAppThemeSettingsAsync(token).ConfigureAwait(true);
                RefreshEditors?.Invoke();
                _loading = false;
                ApplyCurrentTheme();
                _statusText.Text = "Loaded saved app theme profile.";
            }
            catch (Exception ex)
            {
                _settings = AppThemeSettings.CreateDefault();
                RefreshEditors?.Invoke();
                _statusText.Text = $"Theme profile could not be loaded: {ex.Message}";
            }
            finally
            {
                _loading = false;
            }
        }

        private void ApplyCurrentTheme()
        {
            if (_loading)
                return;

            _settings.Normalize();
            _themeService?.ApplyCustomTheme(_settings);
            _statusText.Text = "Preview updated. Save Theme keeps this design for the app.";
        }

        private async Task SaveThemeAsync(CancellationToken token = default)
        {
            if (_settingsService == null)
            {
                _statusText.Text = "Settings service is not available.";
                return;
            }

            try
            {
                _settings.Normalize();
                await _settingsService.SaveThemeAsync(_settings.BaseTheme, token).ConfigureAwait(true);
                await _settingsService.SaveAppThemeSettingsAsync(_settings, token).ConfigureAwait(true);
                _themeService?.ApplyCustomTheme(_settings);
                _statusText.Text = "Theme saved and applied across the app.";
            }
            catch (Exception ex)
            {
                _statusText.Text = $"Theme could not be saved: {ex.Message}";
            }
        }

        private void ResetTheme()
        {
            _loading = true;
            _settings = AppThemeSettings.CreateDefault(_settings.BaseTheme);
            RefreshEditors?.Invoke();
            _loading = false;
            ApplyCurrentTheme();
            _statusText.Text = "Theme reset to the selected base defaults. Save Theme to keep it.";
        }

        private void BrowseBackgroundImage()
        {
            var path = _fileDialogService?.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif");
            if (!string.IsNullOrWhiteSpace(path))
            {
                _settings.BackgroundImagePath = path;
                RefreshEditors?.Invoke();
                ApplyCurrentTheme();
            }
        }
    }
}
