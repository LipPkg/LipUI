﻿using System.Windows;
using LipUI.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace LipUI.Views.Pages
{
    /// <summary>
    /// Interaction logic for UninstallPage.xaml
    /// </summary>
    public partial class UninstallPage : INavigableView<UninstallPageViewModel>
    {
        public UninstallPageViewModel ViewModel
        {
            get;
        }
        public UninstallPage(UninstallPageViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Global.Navigate<ToothLocalPage, ToothLocalModel>();
        }
    }
}
