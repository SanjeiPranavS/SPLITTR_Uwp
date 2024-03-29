﻿using System;
using Windows.UI.Xaml.Data;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.DataRepository;

namespace SPLITTR_Uwp.Converters;

public class ExpenseAmountFormatter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var amount = (double)value;
        return amount.ExpenseAmount(Store.CurrentUserBobj);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
