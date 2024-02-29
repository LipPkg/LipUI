﻿using LipUI.Models;
using LipUI.Models.Plugin;
using Microsoft.UI.Xaml.Controls;

namespace LipUI;

internal partial class MainWindow
{
    public static void InitEventHandlers()
    {
        PluginSystem.PluginEnabled += PluginSystem_PluginEnabled;
        PluginSystem.PluginDisabled += PluginSystem_PluginDisabled;
    }

    private static readonly HashSet<IUIPlugin> enabledModules = [];
    private static void PluginSystem_PluginEnabled(IPlugin obj)
    {
        lock (enabledModules)
        {
            if (obj is IUIPlugin uiPlugin)
            {
                enabledModules.Add(uiPlugin);
                enabled?.Invoke(uiPlugin);
            }
        }
    }

    private static void PluginSystem_PluginDisabled(IPlugin obj)
    {
        lock (enabledModules)
        {
            if (obj is IUIPlugin uiPlugin)
            {
                enabledModules.Remove(uiPlugin);
                disabled?.Invoke(uiPlugin);
            }
        }
    }

    private static Action<IUIPlugin>? enabled;
    private static Action<IUIPlugin>? disabled;

    private readonly object _lock = new();
    private uint navViewBarEnabledPluginsCount;

    private readonly Dictionary<IUIPlugin, NavigationViewItemBase> views = [];

    private void PluginEnabled(IUIPlugin plugin)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                lock (_lock)
                {
                    IList<object> items;

                    //bool addIntoNavViewBar = false;
                    //if (navViewBarEnabledPluginsCount < 4)
                    //{
                    items = NavView.MenuItems;
                    //addIntoNavViewBar = true;
                    //}
                    //else
                    //    items = NavigationViewItem_More.MenuItems;

                    var view = new NavigationViewItem()
                    {
                        Icon = plugin.NavigatonBarIcon,
                        Content = plugin.NavigationBarContent,
                        Tag = plugin
                    };
                    views.Add(plugin, view);

                    items.Add(view);

                    //if (addIntoNavViewBar)
                    navViewBarEnabledPluginsCount++;

                }
            }
            catch (Exception ex)
            {
                await InternalServices.ShowInfoBarAsync(ex);
            }
        });
    }

    private void PluginDisabled(IUIPlugin plugin)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                lock (_lock)
                {
                    if (NavView.MenuItems.Remove(views[plugin]))
                    {
                        navViewBarEnabledPluginsCount--;
                    }
                    //else
                    //{
                    //    NavigationViewItem_More.MenuItems.Remove(views[plugin]);
                    //}

                    views.Remove(plugin);
                }
            }
            catch (Exception ex)
            {
                await InternalServices.ShowInfoBarAsync(ex);
            }
        });
    }
}
