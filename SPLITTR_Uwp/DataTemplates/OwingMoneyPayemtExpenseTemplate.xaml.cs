﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.ViewModel;
using SPLITTR_Uwp.ViewModel.Vobj;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SPLITTR_Uwp.DataTemplates;

public sealed partial class OwingMoneyPayemtExpenseTemplate : UserControl
{
    private OwingMoneyPaymentExpenseViewModel _viewModel;
        
    public OwingMoneyPayemtExpenseTemplate()
    {
        _viewModel = ActivatorUtilities.GetServiceOrCreateInstance<OwingMoneyPaymentExpenseViewModel>(App.Container);
        InitializeComponent();
        DataContextChanged += OwingMoneyPayemtExpenseTemplate_DataContextChanged;
    }

    private void OwingMoneyPayemtExpenseTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is not ExpenseVobj expense)
        {
            return;
        }

        InvertPaymentControlVisibility(false);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {

        if (DataContext is not ExpenseVobj expenseObj)
        {
            return;
        }
          
        InvertPaymentControlVisibility(true);

        _viewModel.PaymetForExpenseButtonClicked(expenseObj);
           
    }


    private void InvertPaymentControlVisibility(bool isVisible)
    {
        if (isVisible)
        {
            SettleUpButton.Visibility = Visibility.Collapsed;
            PaymentControl.Visibility = Visibility.Visible;
            return;
        }
        SettleUpButton.Visibility = Visibility.Visible;
        PaymentControl.Visibility = Visibility.Collapsed;

    }
    public event Action<object, RoutedEventArgs> BackButtonClicked;

    private void ExpenseDetailedViewUserControl_OnBackButtonClicked(object arg1, RoutedEventArgs arg2)
    {
        BackButtonClicked?.Invoke(arg1,arg2);
    }
}