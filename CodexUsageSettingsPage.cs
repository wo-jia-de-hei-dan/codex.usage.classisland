using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using System.Diagnostics;

namespace CodexUsageClassIsland;

[SettingsPageInfo("codex.usage.settings", "Codex 使用量")]
public sealed class CodexUsageSettingsPage : SettingsPageBase
{
    public CodexUsageSettingsPage(PluginSettings settings, PluginSettingsStore store, UsageService service)
    {
        var authPath = new TextBox
        {
            Text = settings.AuthFilePath ?? string.Empty,
            Watermark = "留空则自动读取 %USERPROFILE%\\.codex\\auth.json",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var interval = new NumericUpDown { Minimum = 1, Maximum = 60, Value = settings.RefreshIntervalMinutes };
        var threshold = new NumericUpDown { Minimum = 1, Maximum = 99, Value = settings.LowThresholdPercent };
        var notify = new CheckBox { Content = "低于阈值时使用 ClassIsland 提醒", IsChecked = settings.EnableLowUsageNotification };
        var status = new TextBlock { Text = "修改后点击“保存设置”才会写入配置文件。", Opacity = 0.7, TextWrapping = TextWrapping.Wrap };

        var save = new Button { Content = "保存设置", HorizontalAlignment = HorizontalAlignment.Left };
        save.Click += (_, _) =>
        {
            settings.AuthFilePath = string.IsNullOrWhiteSpace(authPath.Text) ? null : authPath.Text.Trim();
            if (interval.Value is { } i) settings.RefreshIntervalMinutes = (int)i;
            if (threshold.Value is { } t) settings.LowThresholdPercent = (int)t;
            settings.EnableLowUsageNotification = notify.IsChecked == true;
            store.Save(new PluginSettingsData(
                settings.AuthFilePath,
                settings.RefreshIntervalMinutes,
                settings.LowThresholdPercent,
                settings.EnableLowUsageNotification,
                settings.NotifyOnlyOnThresholdCrossing));
            status.Text = $"已保存：{DateTime.Now:HH:mm:ss}";
            status.Foreground = Brushes.LightGreen;
        };

        var reset = new Button { Content = "恢复默认", HorizontalAlignment = HorizontalAlignment.Left };
        reset.Click += (_, _) =>
        {
            authPath.Text = string.Empty;
            interval.Value = 5;
            threshold.Value = 20;
            notify.IsChecked = false;
            status.Text = "已恢复表单默认值，请点击“保存设置”确认。";
            status.Foreground = Brushes.Orange;
        };

        var refresh = new Button { Content = "立即刷新", HorizontalAlignment = HorizontalAlignment.Left };
        refresh.Click += async (_, _) => await service.RefreshAsync();
        var open = new Button { Content = "打开官方 Usage 页面", HorizontalAlignment = HorizontalAlignment.Left };
        open.Click += (_, _) => Process.Start(new ProcessStartInfo("https://chatgpt.com/codex/settings/usage") { UseShellExecute = true });

        Content = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = new StackPanel
            {
                Spacing = 14,
                Margin = new Avalonia.Thickness(0, 0, 16, 20),
                Children =
                {
                    new TextBlock { Text = "Codex 使用量", FontSize = 24 },
                    new TextBlock { Text = "读取本机 Codex 登录状态，设置只保存在本机。不会上传密码、Token 或 auth.json 内容。", Opacity = 0.75, TextWrapping = TextWrapping.Wrap },
                    Section("登录状态", new Control[]
                    {
                        Label("Codex auth.json 路径"), authPath,
                        new TextBlock { Text = "留空时使用默认路径。插件只读取文件，不会把认证内容写入日志。", Opacity = 0.65, TextWrapping = TextWrapping.Wrap }
                    }),
                    Section("刷新与提醒", new Control[]
                    {
                        Label("刷新间隔（分钟）"), interval,
                        Label("低额度阈值（百分比）"), threshold,
                        notify,
                        new TextBlock { Text = "自动化触发器：Codex 剩余额度低于阈值。", Opacity = 0.65, TextWrapping = TextWrapping.Wrap }
                    }),
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        Children = { save, reset, refresh, open }
                    },
                    status,
                    new TextBlock { Text = "免责声明：本插件为社区项目，与 OpenAI、ChatGPT 或 ClassIsland 官方无关。用量接口属于客户端内部接口，可能随服务更新而变化。请勿将 auth.json、Token、日志或个人配置提交到公开仓库。", Opacity = 0.62, TextWrapping = TextWrapping.Wrap }
                }
            }
        };
    }

    private static TextBlock Label(string text) => new() { Text = text, FontWeight = FontWeight.Bold };

    private static Border Section(string title, IEnumerable<Control> children) => new()
    {
        Padding = new Avalonia.Thickness(16),
        CornerRadius = new Avalonia.CornerRadius(8),
        Background = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
        Child = CreateSectionPanel(title, children)
    };

    private static StackPanel CreateSectionPanel(string title, IEnumerable<Control> children)
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 16, FontWeight = FontWeight.Bold });
        foreach (var child in children) panel.Children.Add(child);
        return panel;
    }
}
