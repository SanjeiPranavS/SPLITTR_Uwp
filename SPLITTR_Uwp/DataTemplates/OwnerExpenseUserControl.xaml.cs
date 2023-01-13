﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.ViewModel;
using SPLITTR_Uwp.ViewModel.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SPLITTR_Uwp.DataTemplates
{
    public sealed partial class OwnerExpenseUserControl : UserControl
    {
       

        public ExpenseViewModel ExpenseObj
        {
            get=> this.DataContext as ExpenseViewModel;
        }

        private OwnerExpenseControlViewModel _viewModel;

        public OwnerExpenseUserControl()
        {
            this.InitializeComponent();
            DataContextChanged += OwnerExpenseUserControl_DataContextChanged;
            _viewModel = ActivatorUtilities.CreateInstance<OwnerExpenseControlViewModel>(App.Container);
        }

        private void OwnerExpenseUserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            Bindings.Update();
            if (ExpenseObj is null)
            {
                return;
            }
            _viewModel.DataContextLoaded(ExpenseObj);
        }
        private void MarkAsPaidButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ExpenseObj is not null)
            {
                _viewModel.OnExpenseMarkedAsPaid(ExpenseObj);
            }
        }
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ExpenseObj is not null)
            {
                _viewModel.OnExpenseCancellation(ExpenseObj);
            }
        }
    }
}
