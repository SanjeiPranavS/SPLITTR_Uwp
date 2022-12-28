﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.DataRepository;
using SPLITTR_Uwp.Services;
using System.Collections;
using SPLITTR_Uwp.ViewModel.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SPLITTR_Uwp.Views
{
    public sealed partial class MainPageButtonControl : UserControl
    {

        private readonly UserViewModel _currentUserViewModel;

        public MainPageButtonControl()
        {
            this.InitializeComponent();
            // fetching current user details to avoid showing user as participants in each and every group
            var store = ActivatorUtilities.GetServiceOrCreateInstance<DataStore>(App.Container);
            _currentUserViewModel = new UserViewModel(store?.UserBobj);
            OnArrowButtonClicked += MainPageButtonControl_OnArrowButtonClicked;
        }

        #region SingularityOPenLogic
        private static event Action<MainPageButtonControl> OnArrowButtonClicked;

        private void MainPageButtonControl_OnArrowButtonClicked(MainPageButtonControl obj)
        {
            if (this == obj)
            {
                PerformOpenListview();// shows or hides listview if passed obj is matched
            }
            else
            {
                PerformCloseListView();//hides listview if passed obj is not matched
            }

        }
        private void PerformOpenListview()
        {
            if (UserListView.Visibility == Visibility.Collapsed)
            {
                UserListView.Visibility = Visibility.Visible;
                ArrowImage.Rotation = 90;

            }
            else
            {
                UserListView.Visibility = Visibility.Collapsed;
                ArrowImage.Rotation = 0;

            }
        }
        private void PerformCloseListView()
        {
            UserListView.Visibility = Visibility.Collapsed;
            ArrowImage.Rotation = 0;
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //For internal Single open Behaviour
            OnArrowButtonClicked?.Invoke(this);
        }


        #endregion


        public readonly static DependencyProperty TitleNameProperty = DependencyProperty.Register(
            nameof(TitleName), typeof(string), typeof(MainPageButtonControl), new PropertyMetadata(default(string)));

        public string TitleName
        {
            get
            {
                return (string)GetValue(TitleNameProperty);
            }
            set
            {
                SetValue(TitleNameProperty, value);
            }
        }

        //public readonly static DependencyProperty UserListSourceProperty = DependencyProperty.Register(
        //    nameof(UserListSource), typeof(IList<User>), typeof(MainPageButtonControl), new PropertyMetadata(default(IList<User>)));


        public readonly static DependencyProperty SelectionModeProperty = DependencyProperty.Register(
            nameof(SelectionMode), typeof(ListViewSelectionMode), typeof(MainPageButtonControl), new PropertyMetadata(default(ListViewSelectionMode)));

        public ListViewSelectionMode SelectionMode
        {
            get
            {
                return (ListViewSelectionMode)GetValue(SelectionModeProperty);
            }
            set
            { 
                SetValue(SelectionModeProperty, value);
            }
        }
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {   
                var listWithCurrentUserBsObject = AlterInputListWithCurrentUserBobj(value);
                SetValue(ItemsSourceProperty, listWithCurrentUserBsObject);
            }
        }

        //replacing ordinary current  user obj with userVm so Support Change 
        private IEnumerable AlterInputListWithCurrentUserBobj(IEnumerable value)
        {
            if (value is  ObservableCollection<User> )
            {
                return value;
            }
            
            if (value is IList<User> users)
            {
                return users.Select(((user) =>
                {
                    if (user.Equals(_currentUserViewModel))
                    {
                        return (User)_currentUserViewModel;
                    }
                    return user;
                }));
            }
            return value;
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
                typeof(MainPageButtonControl), new PropertyMetadata(null));

        public event Action<User> UserSelectedFromTheList;
        private void UserListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             if(e.AddedItems.Any())
             {
                 var selectedUser = (User)e.AddedItems[0];
                 UserSelectedFromTheList?.Invoke(selectedUser);
             }
        }
    }
}