﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using LipNETWrapper;
using LipUI.Views.Windows;
using Wpf.Ui.Common.Interfaces;

namespace LipUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private string _appVersion = String.Empty;
        [ObservableProperty]
        private string _lipVersion = String.Empty;
        [ObservableProperty]
        private Wpf.Ui.Appearance.ThemeType _currentTheme = Wpf.Ui.Appearance.ThemeType.Unknown;


        [ObservableProperty]
        public string _lipPath;
        partial void OnLipPathChanged(string path)
        {
            if (File.Exists(path))
            {
                Global.Lip.ExecutablePath = path;
            }
            else if (File.Exists(Path.Combine(path, "lip.exe")))
            {
                Global.Lip.ExecutablePath = Path.Combine(path, "lip.exe");
            }
        }
        [ObservableProperty]
        public string _workingDir;
        partial void OnWorkingDirChanged(string path)
        {
            if (Directory.Exists(path))
            {
                Global.Lip.WorkingPath = path;
            }
        }
        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }
        public void OnNavigatedFrom()
        {
        }
        private void InitializeViewModel()
        {
            CurrentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
            AppVersion = $"LipUI - {GetAssemblyVersion()}";
            LipVersion = "loading ... ";
            Task.Run(async () =>
            {
                var version = await Global.Lip.GetLipVersion();
                LipVersion = version;
            }).ConfigureAwait(false);
            _isInitialized = true;
        }
        [RelayCommand]
        private void OnOpenLipUrl()
        {
            //https://github.com/LiteLDev/Lip
            System.Diagnostics.Process.Start("https://github.com/LiteLDev/Lip");
        }
        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? String.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Light)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Light;

                    break;

                default:
                    if (CurrentTheme == Wpf.Ui.Appearance.ThemeType.Dark)
                        break;

                    Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
                    CurrentTheme = Wpf.Ui.Appearance.ThemeType.Dark;

                    break;
            }
        }
    }
}
