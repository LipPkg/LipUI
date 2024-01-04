﻿using LipUI.VIews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LipUI.Models;

public interface ILipUIModulesNonGeneric
{
    public string ModuleName { get; }

    public Type PageType { get; }

    public FrameworkElement? IconContent { get; }

    public Brush? IconBackground { get; }

    public void OnIconInitialze(ModuleIcon icon) { }
}

public interface ILipUIModules<TSelf> : ILipUIModulesNonGeneric
    where TSelf : ILipUIModules<TSelf>, new()
{
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class LipUIModuleAttribute : Attribute
{
}
