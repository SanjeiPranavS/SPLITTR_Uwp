﻿using System;
using Windows.UI.Xaml.Data;
using SPLITTR_Uwp.Core.ExtensionMethod;

namespace SPLITTR_Uwp.Converters;

internal class UserInitialFormatter : IValueConverter 
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var name = (string)value;
        return name.GetUserInitial();
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }

}