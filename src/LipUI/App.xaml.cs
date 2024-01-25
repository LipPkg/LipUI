﻿using LipUI.Models;
using Microsoft.UI.Xaml;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LipUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Current.RequestedTheme = InternalServices.ApplicationTheme = Main.Config.PersonalizationSettings.ColorTheme switch
            {
                ElementTheme.Dark => ApplicationTheme.Dark,
                ElementTheme.Light => ApplicationTheme.Light,
                ElementTheme.Default or _ => Current.RequestedTheme
            };
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {

            m_window = new MainWindow();
            m_window.Activate();

            UnhandledException += App_UnhandledException;
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            static async ValueTask _exp(Exception ex)
            {
                await InternalServices.ShowInfoBarAsync(ex, containsStacktrace: true);
                if (ex.InnerException is not null)
                    await _exp(ex.InnerException);
            }

            if (InternalServices.MainWindow is not null)
            {
                e.Handled = true;
                await _exp(e.Exception);
            }
        }

        internal Window? m_window;
    }
}
