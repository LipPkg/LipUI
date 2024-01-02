using LipUI.Models;
using LipUI.Pages.HomePageModules;
using LipUI.VIews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LipUI.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    private void ServerIconImage_Loading(FrameworkElement sender, object args)
        => RefreshIcon();

    private async void RefreshIcon()
    {
        ServerInstance? instance = Main.Config.SelectedServer;
        ServerIconImage.Source = await ServerIcon.GetIcon(instance);
    }

    private void SelectServerButton_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SelectServerPage), () => { DispatcherQueue.TryEnqueue(() => { RefreshIcon(); }); });

    private void Page_Loaded(object sender, RoutedEventArgs e)
        => ShowLipInstallerPageIfNotExist();

    private void ShowLipInstallerPageIfNotExist()
    {
        if (File.Exists(Main.Config.Settings.LipPath))
        {
            return;
        }

        var dialog = new ContentDialog()
        {
            XamlRoot = XamlRoot,
            Content = new LipInstallerView()
        };

        DispatcherQueue.TryEnqueue(async () =>
        {
            await dialog.ShowAsync();
        });
    }

    private void StartServerButton_Click(object sender, RoutedEventArgs e)
    {
        if (Main.Config.SelectedServer is null)
            return;

        var dir = Main.Config.SelectedServer.WorkingDirectory;
        var path = Path.Combine(dir, "bedrock_server_mod.exe");

        if (File.Exists(path))
        {
            Process.Start(path);
            return;
        }

        path = Path.Combine(dir, "bedrock_server.exe");
        if (File.Exists(path))
        {
            Process.Start(path);
        }
    }

    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        => throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

    private void ContentFrame_Loading(FrameworkElement sender, object args)
        => ContentFrame.Navigate(typeof(ModulesPage));

    private void BackButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        => AnimatedIcon.SetState(BackAnimatedIcon, "PointerOver");

    private void BackButton_PointerExited(object sender, PointerRoutedEventArgs e)
        => AnimatedIcon.SetState(BackAnimatedIcon, "Normal");

    private void BackButton_Click(object sender, RoutedEventArgs e)
    => ContentFrame.GoBack();

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        => BackButton.IsEnabled = ContentFrame.CanGoBack;
}
