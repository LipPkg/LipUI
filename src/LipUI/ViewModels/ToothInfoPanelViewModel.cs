﻿using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LipNETWrapper.Class;

namespace LipUI.ViewModels
{
    public partial class ToothInfoPanelViewModel : ObservableObject
    {
        private readonly LipPackage? _info;
        private readonly LipRegistry.LipRegistryItem? _registryItem;
        private readonly LipPackageVersions _ver;
        public ToothInfoPanelViewModel(LipPackage info) : this((LipPackageVersions)info)
        {
            Tooth = info.Tooth;
        }
        public ToothInfoPanelViewModel(LipPackageVersions info, LipRegistry.LipRegistryItem item) : this(info)
        {
            _registryItem = item;
            Tooth = item.Tooth;
            Author = item.Author;
            Description = item.Description;
            Homepage = item.Homepage;
            Name = item.Name;
            License = item.License;
            //Version = item.Version;
        }
        public ToothInfoPanelViewModel(LipPackageVersions ver)
        {
            _ver = ver;
            if (ver is LipPackage info)
            {
                _info = info;
                Author = info.Author;
                Description = info.Description;
                Homepage = info.Homepage;
                Name = info.Name;
                License = info.License;
                Version = info.Version;
            }
            if (Versions?.FirstOrDefault() is not null and var v)
            {
                SelectedVersion = v;
            }
        }
        [ObservableProperty] string _author = string.Empty;
        [ObservableProperty] string _description = string.Empty;
        [ObservableProperty] string _homepage = string.Empty;
        [ObservableProperty] string _name = string.Empty;
        [ObservableProperty] string _license = string.Empty;
        [ObservableProperty] string _version = string.Empty;
        [ObservableProperty] string _tooth = string.Empty;
        public string[]? Versions => _ver.Versions;
        [RelayCommand]
        void CopyToothButton()
        {
            System.Windows.Clipboard.SetText(Tooth);
            Global.PopupSnackbar("复制成功", Tooth);
        }
        [ObservableProperty] private string? _selectedVersion;
    }
}
