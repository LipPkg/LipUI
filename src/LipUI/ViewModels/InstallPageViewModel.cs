﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Common.Interfaces;
using Hyperlink = Wpf.Ui.Controls.Hyperlink;

namespace LipUI.ViewModels
{
    public record InstallInfo(string Tooth, ToothInfoPanelViewModel data, string? Version);
    public partial class InstallPageViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        ObservableCollection<string> _outPut = new();
        [ObservableProperty]
        string _toothName;
        partial void OnToothNameChanged(string _)
        {
            ToothInfoPanel = null;
            OutPut.Clear();
        }
        [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
        [NotifyPropertyChangedFor(nameof(InfoLoaded))]
        [ObservableProperty]
        ToothInfoPanelViewModel? _toothInfoPanel;
        public bool InfoLoaded => _toothInfoPanel is not null;
        [ObservableProperty] private bool _installing = false;
        [RelayCommand(CanExecute = nameof(InfoLoaded))]
        public async Task Install()
        {
            OutPut.Clear();
            Ctk = new CancellationTokenSource();
            Installing = true;
            try
            {
                var fullname = ToothName;
                if (!string.IsNullOrWhiteSpace(SelectedVersion))
                    fullname += "@" + SelectedVersion;
                var exitCode = await Global.Lip.InstallPackageAsync(fullname, Ctk.Token, (x, input) =>
                {
                    if (!string.IsNullOrWhiteSpace(x))
                    {
                        if (x.StartsWith("======"))
                        {
                            Task.Delay(1000).ContinueWith(async _ =>
                            {
                                var fullEula = string.Join(Environment.NewLine, OutPut).Replace("http", Environment.NewLine + "http");
                                //remove str before === 
                                {
                                    var index = fullEula.IndexOf("======\r\n", StringComparison.Ordinal);
                                    if (index != -1) { fullEula = fullEula[(index + 8)..]; }
                                }
                                _ = Global.ShowDialog("提示", await Global.DispatcherInvokeAsync(() =>
                                    {
                                        try
                                        {
                                            StackPanel content = new();
                                            //foreach (var data in fullEula.Split(new[] { "https" }, StringSplitOptions.None))
                                            //{
                                            //    content.Children.Add(new TextBlock() { Text = data });
                                            //}
                                            //highlight all http url in eula
                                            while (Regex.Match(fullEula, @"https?://[^\s]+") is { Success: true } match)
                                            {
                                                var text = match.Value;
                                                var index = match.Index;
                                                var length = match.Length;
                                                var before = fullEula[..index];
                                                var after = fullEula[(index + length)..];
                                                var link = new Hyperlink { NavigateUri = text, Content = text };
                                                link.Click += (sender, e) => { System.Diagnostics.Process.Start(text); };
                                                content.Children.Add(new TextBlock() { Text = before });
                                                content.Children.Add(link);
                                                fullEula = after;
                                            }
                                            content.Children.Add(new TextBlock() { Text = fullEula });
                                            return content;
                                        }
                                        catch
                                        {
                                            return new StackPanel
                                            {
                                                Children =
                                                {
                                                    new TextBlock
                                                    {
                                                        Text = fullEula,
                                                        TextWrapping = TextWrapping.WrapWithOverflow
                                                    }
                                                }
                                            };
                                        }
                                    }), ("好的", hide =>
                                    {
                                        hide();
                                        input("y");
                                    }
                                ), modify: dialog =>
                                {
                                    Global.DispatcherInvoke(() =>
                                    {
                                        dialog.DialogHeight = 600;
                                        dialog.DialogHeight = 400;
                                    });
                                });
                            });
                            Global.DispatcherInvoke(() => OutPut.Add(x));
                        }
                        else if (x.Trim().EndsWith("|"))
                        {
                            Percentage = x.Replace("|", "").Trim();
                        }
                        else
                        {
                            Global.DispatcherInvoke(() => OutPut.Add(x));
                        }
                    }
                });
                OutPut.Add($"ExitCode：{exitCode}");
            }
            catch (Exception ex)
            {
                OutPut.Add(ex.ToString());
            }
            Ctk = null;
            Installing = false;
        }
        [RelayCommand]
        public async Task FetchInfo()
        {
            OutPut.Clear();
            Ctk = new CancellationTokenSource();
            try
            {
                var (success, package, message) = await Global.Lip.GetPackageInfoAsync(ToothName, _ctk.Token, x =>
                {
                    if (x is not null)
                    {
                        if (!x.StartsWith("{"))
                        {
                            Global.DispatcherInvoke(() => OutPut.Add(x));
                        }
                    }
                });
                if (success)
                {
                    ToothInfoPanel = new ToothInfoPanelViewModel(package!)
                    {
                        Tooth = ToothName
                    };
                }
                else
                {
                    //todo 获取失败
                }
            }
            catch (Exception ex)
            {
                OutPut.Add(ex.ToString());
            }
            Ctk = null;
        }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanCancel))]
        [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
        CancellationTokenSource? _ctk = null;
        [ObservableProperty]
        string? _selectedVersion;
        [ObservableProperty]
        string _percentage = string.Empty;
        public bool CanCancel => Ctk is not null;
        [RelayCommand(CanExecute = nameof(CanCancel))]
        public void Cancel()
        {
            Ctk?.Cancel();
            Ctk = null;
        }
        public void OnNavigatedTo()
        {
            if (Global.TryDequeueItem<InstallInfo>(out var item))
            {
                ToothName = item.Tooth;
                ToothInfoPanel = item.data;
                SelectedVersion = item.Version ?? item.data.Versions.FirstOrDefault();
            }
        }
        public void OnNavigatedFrom()
        {
        }
    }
}
