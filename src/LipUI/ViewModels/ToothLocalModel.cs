﻿using CommunityToolkit.Mvvm.ComponentModel;
using LipUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using LipNETWrapper.Class;
using Wpf.Ui.Common.Interfaces;

namespace LipUI.ViewModels
{
    public partial class ToothLocalModel : ObservableObject, INavigationAware
    {
        //protected bool _isInitialized = false;
        [ObservableProperty]
        ToothInfoPanelViewModel _currentInfo = null;
        [ObservableProperty]
        ToothItemViewModel _currentSelected = null;
        [ObservableProperty]
        bool _isShowingDetail = false;
        [ObservableProperty]
        ObservableCollection<ToothItemViewModel> _toothItems
            = new();
        [ObservableProperty]
        bool _loading = true;
        public void OnNavigatedTo()
        {
            Task.Run(LoadAllPackages);//初始化加载所以包
            //if (!_isInitialized)
            //    InitializeViewModel();
        }
        public void OnNavigatedFrom()
        {
        }
        [RelayCommand(CanExecute = nameof(Loading))]
        protected async Task LoadAllPackages()
        {
            await Global.DispatcherInvokeAsync(() => ToothItems.Clear());
            var (packages, message) = await Global.Lip.GetAllPackagesAsync();
            foreach (var package in packages)
            {
                await Global.DispatcherInvokeAsync(() => ToothItems.Add(new ToothItemViewModel(ShowInfo)
                {
                    Version = package.Version,
                    Tooth = package.Tooth
                }));
                await Task.Delay(100);//100毫秒显示一个，假装很丝滑
            }
        }

        protected Task<(bool success, LipPackage? package, string message)> FetchPackageInfo(string tooth)
        {
            return Global.Lip.GetLocalPackageInfoAsync(tooth);
        }
        protected async Task ShowInfo(ToothItemViewModel toothItem)
        {
            CurrentSelected = toothItem;
            var (success, package, message) = await FetchPackageInfo(toothItem.Tooth);
            if (success)
            {
                CurrentInfo = new ToothInfoPanelViewModel(package!);
            }
            else
            {
                //todo 读取失败
            }
            IsShowingDetail = true;
        }
    }
}
