﻿using System;
using System.Windows;
using System.Windows.Controls;
using LipUI.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace LipUI.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel
        {
            get;
        }

        public MainWindow(MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            SetPageService(pageService);
             
            navigationService.SetNavigationControl(RootNavigation);

            //从配置获取主题
            if (Global.Config.Theme is not ThemeType.Unknown) Theme.Apply(Global.Config.Theme);
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;
        public INavigation GetNavigation()
            => RootNavigation;
        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);
        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;
        public void ShowWindow()
            => Show();
        public void CloseWindow()
            => Close();
        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }
    }
}