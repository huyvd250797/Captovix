using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captovix;

public partial class MainWindow
{
    private FrameworkElement BuildAppearanceSettings()
    {
        var saved = AppearanceManager.Current;
        var layout = new DockPanel { Margin = new Thickness(12) };
        var panel = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        panel.Children.Add(new TextBlock { Text = "Giao diện & màu sắc", FontSize = 22, FontWeight = FontWeights.SemiBold });
        panel.Children.Add(new TextBlock
        {
            Text = "Thay đổi hiển thị ngay. Bấm Lưu để giữ màu hoặc đóng Thiết lập để quay về màu đã lưu.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(3, 8, 3, 12)
        });
        var modes = new ComboBox { ItemsSource = new[] { "Theo hệ thống", "Sáng", "Tối" }, SelectedIndex = (int)saved.Mode };
        StackPanel Group(string title, string description)
        {
            var fields = new StackPanel();
            var card = new Border { Child = fields, Padding = new Thickness(12), Margin = new Thickness(0, 0, 0, 10),
                BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(8) };
            card.SetResourceReference(Border.BorderBrushProperty, "Line");
            fields.Children.Add(new TextBlock { Text = title, FontSize = 15, FontWeight = FontWeights.SemiBold });
            fields.Children.Add(new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 8) });
            panel.Children.Add(card);
            return fields;
        }
        Group("Chế độ giao diện", "Điều khiển màu nền và chữ của Captovix.").Children.Add(modes);
        var accentGroup = Group("Màu nhấn", "Áp dụng cho nút chính, vùng chọn và trạng thái nổi bật.");
        var logoGroup = Group("Logo Captovix", "Chỉnh riêng nền và hình logo trên cửa sổ, khay hệ thống và taskbar.");

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
        var brand = new Image { Width = 48, Height = 48, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 14, 0) };
        brand.SetResourceReference(Image.SourceProperty, "BrandMark");
        var sampleButton = new Button { Content = "Captovix · Màu nhấn", Padding = new Thickness(14, 8, 14, 8) };
        sampleButton.SetResourceReference(Control.BackgroundProperty, "Accent");
        sampleButton.SetResourceReference(Control.ForegroundProperty, "OnAccent");
        accentGroup.Children.Add(sampleButton);
        logoGroup.Children.Add(brand);

        string? Pick() => NativeColorPicker.Pick(_settingsDialog ?? this);
        accentGroup.Children.Add(PropertyVisuals.ColorField("Màu chính", accent, Pick));
        accentGroup.Children.Add(gradient);
        var endField = PropertyVisuals.ColorField("Màu kết thúc", accentEnd, Pick);
        accentGroup.Children.Add(endField);
        logoGroup.Children.Add(PropertyVisuals.ColorField("Màu nền logo", iconBackground, Pick));
        logoGroup.Children.Add(follow);
        var iconFields = new StackPanel();
        iconFields.Children.Add(PropertyVisuals.ColorField("Màu hình logo", icon, Pick));
        iconFields.Children.Add(iconGradient);
        var iconEndField = PropertyVisuals.ColorField("Màu kết thúc hình logo", iconEnd, Pick);
        iconFields.Children.Add(iconEndField);
        logoGroup.Children.Add(iconFields);
        var direction = Group("Hướng chuyển màu", "Dùng chung cho các gradient đang bật.");
        direction.Children.Add(angle);
        direction.Children.Add(angleText);

        var footer = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(footer, Dock.Bottom);
        layout.Children.Add(footer);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancel = new Button { Content = "Hủy" };
        var save = new Button { Content = "Lưu", Style = (Style)Application.Current.FindResource("Primary"), IsEnabled = false };
        actions.Children.Add(cancel);
        actions.Children.Add(save);
        DockPanel.SetDock(actions, Dock.Right);
        footer.Children.Add(actions);
        footer.Children.Add(status);
        layout.Children.Add(new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        cancel.Click += (_, _) => _settingsDialog?.Close();

        void RefreshControls()
        {
            iconFields.IsEnabled = follow.IsChecked != true;
            endField.Visibility = gradient.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            iconEndField.Visibility = iconGradient.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            direction.Visibility = gradient.IsChecked == true || (follow.IsChecked != true && iconGradient.IsChecked == true)
                ? Visibility.Visible : Visibility.Collapsed;
            angleText.Text = $"{angle.Value:0}°";
        }

        bool sync = false;
        AppearanceSettings? draft = null;
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
                AppearanceManager.Preview(value);
                draft = value;
                save.IsEnabled = value != saved;
                status.Text = save.IsEnabled ? "Đang xem trước · Chưa lưu" : "";
            }
            catch (Exception ex)
            {
                draft = null;
                save.IsEnabled = false;
                status.Text = ex.Message;
            }
        }

        save.Click += (_, _) =>
        {
            if (draft is not { } value) return;
            try
            {
                AppearanceManager.Save(value);
                saved = value;
                save.IsEnabled = false;
                status.Text = "Đã lưu giao diện.";
            }
            catch (Exception ex) { status.Text = ex.Message; }
        };

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
        return layout;
    }
}
