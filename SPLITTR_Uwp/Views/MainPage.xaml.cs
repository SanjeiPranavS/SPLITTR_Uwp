﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.DataTemplates.Controls;
using SPLITTR_Uwp.Services;
using SPLITTR_Uwp.ViewModel;
using SPLITTR_Uwp.ViewModel.Contracts;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemBase = Microsoft.UI.Xaml.Controls.NavigationViewItemBase;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using SplitButton = Microsoft.UI.Xaml.Controls.SplitButton;
using SplitButtonClickEventArgs = Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs;
using Windows.UI.Core;
using SPLITTR_Uwp.DataTemplates.Controls.ExpenseListControl;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SPLITTR_Uwp.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly IMainPageViewModel _viewModel;
    private static MainPage _instance;
    
   

   
    public MainPage()
    {
        _viewModel = ActivatorUtilities.CreateInstance<MainPageViewModel>(App.Container);
        _instance = this;
        InitializeComponent();
        NavigationService.Frame = InnerFrame;
        _viewModel.UserGroups.CollectionChanged += UserGroups_CollectionChanged;
        SystemNavigationManager.GetForCurrentView().BackRequested += OnDefaultViewRequested;
    }

    private void PageOnPaneButtonOnClick()
    {
        var isMainPaneOpen = MainPageNavigationView.IsPaneOpen;
        MainPageNavigationView.IsPaneOpen = !isMainPaneOpen;
        Unloaded += MainPage_Unloaded;
    }

    private void MainPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _viewModel.ViewDisposed();
    }
    #region NavigationViewGroupsPopulating 


    private void UserGroups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //filtering GroupBobs From NavigationView Collection
        var groupNavigationViewItems = MainPageNavigationView.MenuItems
            .Where(i => i is NavigationViewItemBase { Content: GroupBobj })
            .Select(menuItem =>
            {
                var navigationViewItem = (NavigationViewItemBase)menuItem;
                return navigationViewItem.Content as GroupBobj;
            });

        if (sender is not ObservableCollection<GroupBobj> userGroups)
        {
            return;
        }
        foreach (var group in userGroups)
        {
            if (group != null && CheckIfGroupNotExistInNavigationView(group))
            {
                AddGroupToNavigationView(group);
            }

            //return true if Group obj is Already in NavigationView menu item list
            bool CheckIfGroupNotExistInNavigationView(GroupBobj group)
            {
                return !groupNavigationViewItems.Contains(group);
            }
        }

    }

    private void AddGroupToNavigationView(GroupBobj group)
    {
        MainPageNavigationView.MenuItems.Add(new NavigationViewItem
        {
            Content = group,
            Style = UserGroupNavigationItemStyle,
        });
    }

    #endregion

    #region NavigationLogic

    public static MainPage GetForCurrentView
    {
        get => _instance;
    }


    public void OnDefaultViewRequested(object sender, BackRequestedEventArgs e)
    {
        NavigationRequested();
    }

    private void NavigationRequested()
    {
        App.IsAppTitleBarButtonVisible = false;
        MainPageNavigationView.IsPaneOpen = true;
        NavigationService.Navigated += NavigationService_Navigated;
        NavigationService.Navigate(typeof(ExpensesListAndDetailViewPage));
    }
    
    private void NavigationService_Navigated(object sender, NavigationEventArgs e)
    {

        if (InnerFrame.Content is not ExpensesListAndDetailViewPage page)
        {
            return;
        }

        page.PaneButtonOnClick += PageOnPaneButtonOnClick;
        page.OnGroupInfoIconClicked += GroupInfoButton_onClick;

        NavigationService.Navigated -= NavigationService_Navigated;
    }

    private void PersonProfileClicked()
    {
        App.IsAppTitleBarButtonVisible = true;
        MainPageNavigationView.IsPaneOpen = false;
        NavigationService.Navigate<UserProfilePage>();
    }
    private void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
    {
        //closing main Page pane
        MainPageNavigationView.IsPaneOpen = false;
        var selectedItem = sender as MenuFlyoutItem;
        ToggleButtonTextBlock.Text = selectedItem?.Text ?? string.Empty;
        var title = selectedItem?.Text;
        NavigateWithRespectToGivenString(title);
       
    }
    private void NavigateWithRespectToGivenString(string title)
    {
        App.IsAppTitleBarButtonVisible = true;
        NavigationService.Navigate(string.Compare(title, "New Expense", StringComparison.InvariantCultureIgnoreCase) == 0 ?
            typeof(AddExpenseTestPage) :
            typeof(GroupCreationPage),null,new DrillInNavigationTransitionInfo());
    }
    private void AddToggleFlyOutButton_OnClick(SplitButton sender, SplitButtonClickEventArgs args)
    {
        MainPageNavigationView.IsPaneOpen = false;
        NavigateWithRespectToGivenString(ToggleButtonTextBlock.Text);
    }
    #endregion

    #region ExpenseSelection Control Redirection 

    private void UserSelectedFromIndividualSplitList(User selectedUser)
    {
        if (selectedUser is null)
        {
            return;
        }
        UserExpensesListControl.FilterExpense(new ExpenseListFilterObj(null, selectedUser, ExpenseListFilterObj.ExpenseFilter.UserExpense));

    }

    private void MainPageNavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {

        if (args?.SelectedItemContainer?.Content is GroupBobj selectedGroup)
        {
            UserExpensesListControl.FilterExpense(new ExpenseListFilterObj(selectedGroup, null, ExpenseListFilterObj.ExpenseFilter.GroupExpense));
            return;
        }
        if (args.SelectedItem is not StackPanel stackPanel)
        {
            return;
        }
        //setting Title and Calling respective viewModel To Polulate the Respective Expenses
        switch (stackPanel.Name)
        {
            case nameof(AllExpense):
                UserExpensesListControl.FilterExpense(new ExpenseListFilterObj(null, null, ExpenseListFilterObj.ExpenseFilter.AllExpenses));
                break;
            case nameof(RequestToMe):
                UserExpensesListControl.FilterExpense(new ExpenseListFilterObj(null, null, ExpenseListFilterObj.ExpenseFilter.RequestToMe));
                break;
            case nameof(RequestedByMe):
                UserExpensesListControl.FilterExpense(new ExpenseListFilterObj(null, null, ExpenseListFilterObj.ExpenseFilter.RequestByMe));
                break;
        }

    }

    #endregion

    #region DashBoardSplitViewLogicRegion
    private void DashBoardButton_OnClick(object sender, RoutedEventArgs e)
    {
        DashBoardSplitView.IsPaneOpen = !DashBoardSplitView.IsPaneOpen;
    }
    private void DashBoardSplitView_OnPaneChanged(SplitView sender, object args)
    {
        AssignDashBoardState();
    }
    private void AssignDashBoardState()
    {
        if (DashBoardSplitView.IsPaneOpen)
        {
            VisualStateManager.GoToState(this, nameof(CloseDashBoardState), false);
            return;
        }
        VisualStateManager.GoToState(this, nameof(OpenDashBoardState), false);
    }

    #endregion


    private void CloseErrorMessageButton_OnClick(object sender, RoutedEventArgs e)
    {
        InAppNotification.Dismiss(true);
    }
    private void GroupInfoButton_onClick(Group obj)
    {
        //Setting Navigation View corresponding group item is selected true
        var groupNavigationViewItem = MainPageNavigationView.MenuItems.FirstOrDefault(i => i is NavigationViewItemBase { Content: Group group } && group.GroupUniqueId.Equals(obj?.GroupUniqueId));
        if (groupNavigationViewItem is NavigationViewItemBase navigationViewItem)
        {
            navigationViewItem.IsSelected = true;
        }
    }

    private void AddWalletBalanceButtonClicked()
    {
        WalletBalanceUpdateTeachingTip.IsOpen = !WalletBalanceUpdateTeachingTip.IsOpen;
    }

    
}