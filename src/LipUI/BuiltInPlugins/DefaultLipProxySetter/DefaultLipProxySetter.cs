﻿using LipUI.Models.Lip;
using LipUI.Models.Plugin;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LipUI.BuiltInPlugins.DefaultLipProxySetter;

[LipUIModule]
internal class DefaultLipProxySetter : IPlugin
{
    private static class Strings
    {
        public const string PluginName = "Lip默认代理配置器";
        public const string ConfirmButtonContent = "是";
        public const string CancelButtonContent = "否";
        public const string InfoBarMessage = "检测到系统默认语言为简体中文，是否设置默认代理？";
        public const string NullLipConsoleInstance = "无法获取LipConsole";
        public const string SettingUp = "设置中";
        public const string Complete = "设置完成";
    }

    public string PluginName => Strings.PluginName;

    public bool DefaultEnabled => System.Globalization.CultureInfo.CurrentCulture.Name.ToLower() is "zh-cn";

    public Guid Guid => new("4A909307-3571-5020-681D-2491B045A8B5");


    private static class DefaultProxies
    {
        public const string Github_LipInstaller = "https://mirror.ghproxy.com";
        public const string Github_Lip = "https://github.bibk.top";
        public const string GoProxy_Lip = "https://goproxy.cn";
    }

    void IPlugin.OnInitlalize(LipuiServices services)
    {
        var config = services.GetPluginConfig(this);

        var selfDisabled = config["SelfDisabled"];
        if (selfDisabled.IsNull)
            config["SelfDisabled"] = false;
    }

    void IPlugin.OnEnable(LipuiServices services)
    {
        var config = services.GetPluginConfig(this);
        if (config["SelfDisabled"].As<bool>())
            return;

        config["SelfDisabled"] = true;

        if (System.Globalization.CultureInfo.CurrentCulture.Name.ToLower() is "zh-cn")
        {
            services.DispatcherQueue.TryEnqueue(async () =>
            {
                var cts = new CancellationTokenSource();
                var confirmButton = new Button()
                {
                    Content = Strings.ConfirmButtonContent,
                };
                var cancelButton = new Button()
                {
                    Content = Strings.CancelButtonContent,
                };

                confirmButton.Click += async (sender, _) =>
                {
                    services.LipuiConfig.GeneralSettings.GithubProxy = DefaultProxies.Github_LipInstaller;
                    var lip = await services.CreateLipConsoleAsync((sender as Button)!.XamlRoot, string.Empty);

                    if (lip is null)
                    {
                        cts.Cancel();
                        services.ShowInfoBar(
                            message: Strings.NullLipConsoleInstance,
                            severity: InfoBarSeverity.Error);
                        return;
                    }

                    cts.Cancel();

                    services.ShowInfoBar(
                        message: Strings.SettingUp,
                        severity: InfoBarSeverity.Informational,
                        interval: TimeSpan.FromSeconds(2));

                    await lip.Run(LipCommand.CreateCommand() + LipCommand.Config + $"GitHubMirrorURL {DefaultProxies.Github_Lip}");

                    await lip.Run(LipCommand.CreateCommand() + LipCommand.Config + $"GoModuleProxyURL {DefaultProxies.GoProxy_Lip}");

                    services.ShowInfoBar(
                        message: Strings.Complete,
                        severity: InfoBarSeverity.Success,
                        interval: TimeSpan.FromSeconds(3));
                };
                cancelButton.Click += (_, _) => cts.Cancel();

                var stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children = { confirmButton, cancelButton },
                    Margin = new(4),
                    Spacing = 8,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                await services.ShowInfoBarAsync(
                    severity: InfoBarSeverity.Warning,
                    message: Strings.InfoBarMessage,
                    interval: TimeSpan.FromSeconds(30),
                    barContent: stackPanel,
                    cancellationToken: cts.Token);
            });
        }
    }
}
