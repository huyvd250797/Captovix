using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private FrameworkElement BuildAppearanceSettings()
    {
        var saved = AppearanceManager.Current;
        var panel = new StackPanel { Margin = new Thickness(12) };
        panel.Children.Add(new TextBlock { Text = "Giao diện & màu sắc", FontSize = 22, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock
        {
            Text = "Xem trước thay đổi ngay khi chỉnh màu.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(3, 8, 3, 12)
        });
        var modes = new ComboBox { ItemsSource = new[] { "Theo hệ thống", "Sáng", "Tối" }, SelectedIndex = (int)saved.Mode };
        panel.Children.Add(modes);

        var accent = new TextBox { Text = saved.Accent };
        var accentEnd = new TextBox { Text = saved.AccentEnd };
        var icon = new TextBox { Text = saved.IconColor };
        var iconEnd = new TextBox { Text = saved.IconEnd };
        var iconBackground = new TextBox { Text = saved.IconBackground };
        var gradient = new CheckBox { Content = "Gradient màu nhấn", IsChecked = saved.AccentGradient, Margin = new Thickness(3, 8, 0, 3) };
        var follow = new CheckBox { Content = "Màu hình logo theo màu nhấn", IsChecked = saved.IconUsesAccent, Margin = new Thickness(3, 8, 0, 3) };
        var iconGradient = new CheckBox { Content = "Gradient cho hình logo", IsChecked = saved.IconGradient, Margin = new Thickness(3, 8, 0, 3) };

        var angle = new Slider { Minimum = 0, Maximum = 360, Value = saved.GradientAngle, TickFrequency = 15, IsSnapToTickEnabled = true, Margin = new Thickness(5) };
        var angleText = new TextBlock();
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(3, 10, 3, 6) };
        var sample = new Border { Padding = new Thickness(16), CornerRadius = new CornerRadius(8), Margin = new Thickness(2, 10, 2, 10) };
        var sampleRow = new StackPanel { Orientation = Orientation.Horizontal };
        var brand = new Image { Width = 48, Height = 48, Margin = new Thickness(0, 0, 14, 0) };
        brand.SetResourceReference(Image.SourceProperty, "BrandMark");
        var sampleButton = new Button { Content = "Captovix · Màu nhấn", Padding = new Thickness(14, 8, 14, 8) };
        sampleButton.SetResourceReference(Control.BackgroundProperty, "Accent");
        sampleButton.SetResourceReference(Control.ForegroundProperty, "OnAccent");
        sampleRow.Children.Add(brand);
        sampleRow.Children.Add(sampleButton);
        sample.Child = sampleRow;
        sample.SetResourceReference(Border.BackgroundProperty, "Soft");
        panel.Children.Add(sample);

        string? Pick() => NativeColorPicker.Pick(_settingsDialog ?? this);
        panel.Children.Add(PropertyVisuals.ColorField("Màu nhấn", accent, Pick));
        panel.Children.Add(gradient);
        var endField = PropertyVisuals.ColorField("Màu kết thúc", accentEnd, Pick);
        panel.Children.Add(endField);
        panel.Children.Add(new TextBlock
        {
            Text = "Logo Captovix",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(3, 16, 0, 4)
        });
        panel.Children.Add(PropertyVisuals.ColorField("Màu nền logo", iconBackground, Pick));
        panel.Children.Add(follow);
        var iconFields = new StackPanel();
        iconFields.Children.Add(PropertyVisuals.ColorField("Màu hình logo", icon, Pick));
        iconFields.Children.Add(iconGradient);
        var iconEndField = PropertyVisuals.ColorField("Màu kết thúc hình logo", iconEnd, Pick);
        iconFields.Children.Add(iconEndField);
        panel.Children.Add(iconFields);
        panel.Children.Add(new TextBlock { Text = "Hướng gradient", Margin = new Thickness(3, 12, 0, 0) });
        panel.Children.Add(angle);
        panel.Children.Add(angleText);
        panel.Children.Add(status);

        void RefreshControls()
        {
            iconFields.IsEnabled = follow.IsChecked != true;
            endField.Visibility = gradient.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            iconEndField.Visibility = iconGradient.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            angleText.Text = $"{angle.Value:0}°";
        }

        bool sync = false;
        void Apply()
        {
            if (sync) return;
            RefreshControls();
            try
            {
                var value = new AppearanceSettings(
                    Mode: (AppearanceMode)modes.SelectedIndex, Accent: accent.Text.Trim(),
                    AccentGradient: gradient.IsChecked == true, AccentEnd: accentEnd.Text.Trim(),
                    IconUsesAccent: follow.IsChecked == true, IconColor: icon.Text.Trim(),
                    IconGradient: iconGradient.IsChecked == true, IconEnd: iconEnd.Text.Trim(),
                    GradientAngle: angle.Value, IconBackground: iconBackground.Text.Trim());
                AppearanceManager.Set(value);
                status.Text = "Đã lưu thiết lập giao diện.";
            }
            catch (Exception ex) { status.Text = ex.Message; }
        }

        foreach (var field in new[] { accent, accentEnd, icon, iconEnd, iconBackground })
            field.TextChanged += (_, _) => Apply();
        foreach (var field in new[] { gradient, follow, iconGradient })
            field.Click += (_, _) => Apply();
        modes.SelectionChanged += (_, _) => Apply();
        angle.ValueChanged += (_, _) => Apply();
        var reset = new Button { Content = "Khôi phục giao diện Captovix", Margin = new Thickness(3, 15, 3, 3) };
        reset.Click += (_, _) =>
        {
            sync = true;
            var defaults = new AppearanceSettings();
            accent.Text = defaults.Accent;
            accentEnd.Text = defaults.AccentEnd;
            icon.Text = defaults.IconColor;
            iconEnd.Text = defaults.IconEnd;
            iconBackground.Text = defaults.IconBackground;
            gradient.IsChecked = defaults.AccentGradient;
            follow.IsChecked = defaults.IconUsesAccent;
            iconGradient.IsChecked = defaults.IconGradient;
            modes.SelectedIndex = (int)defaults.Mode;
            angle.Value = defaults.GradientAngle;
            sync = false;
            Apply();
        };
        panel.Children.Add(reset);
        RefreshControls();
        return panel;
    }
}
