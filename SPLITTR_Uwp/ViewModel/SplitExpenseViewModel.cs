﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.Core.SplittrExceptions;
using SPLITTR_Uwp.Core.UseCase;
using SPLITTR_Uwp.Core.UseCase.GetUserGroups;
using SPLITTR_Uwp.Core.UseCase.SplitExpenses;
using SPLITTR_Uwp.Core.UseCase.UserSuggestion;
using SPLITTR_Uwp.DataRepository;
using SPLITTR_Uwp.DataTemplates.Controls;
using SPLITTR_Uwp.Services;
using SPLITTR_Uwp.ViewModel.Contracts;
using SPLITTR_Uwp.ViewModel.Vobj;
using SPLITTR_Uwp.Views;
using SQLite;

namespace SPLITTR_Uwp.ViewModel;

public class SplitExpenseViewModel : ObservableObject
{

    private ISplitExpenseView View { get; set; }

    private UserVobj User { get; }

    /// <summary>
    /// Contains Expenses obj which will be processed and inserted into db
    /// </summary>
    public readonly ObservableCollection<ExpenseBobj> _expensesToBeSplitted = new ObservableCollection<ExpenseBobj>();

    #region UserAutoSUggestionBox Logic Region
    private bool _isUserSuggestionListOpen;
    private string _splittingUsersName = string.Empty;

    public string SplittingUsersName
    {
        get => _splittingUsersName;
        set => SetProperty(ref _splittingUsersName, value);
    }

    public ObservableCollection<User> UsersList { get; } = new ObservableCollection<User>();


    public void TextBoxLostFocus()
    {
        ExpenseTextBoxValueChanged();
        if (_selectedUser == _dummyUser || _selectedUser == null && !_usersToBeSplitted.Any())
        {
            _isInnerInvokationOfTextChanged = true;
            SplittingUsersName = "";
        }

    }


    public bool IsUserSuggestionListOpen
    {
        get => _isUserSuggestionListOpen;
        set
        {
            _isUserSuggestionListOpen = value;
            OnPropertyChanged();
        }
    }



    private User _selectedUser;
    private readonly IList<User> _usersToBeSplitted = new List<User>();
    private int _selectedGroupIndex;
    private string _selectedGroupName;
    private bool _isNameTextBoxReadOnly;
    private bool _isInnerInvokationOfTextChanged;


    public void TextBoxTextChanged()
    {
        //no Recommendation operation Should be done If TextBox is non editable for user interaction
        if (IsNameTextBoxReadOnly || _isInnerInvokationOfTextChanged)
        {
            //Changing ExpenseViewModels in Unequal Split Taeaching Tip ListView
            SplittingUserPreferenceChanged();
            _isInnerInvokationOfTextChanged = false;
            IsUserSuggestionListOpen = false;
            return;
        }
        if (SplittingUsersName.Trim().Length < 1)
        {
            _selectedUser = null;
            IsUserSuggestionListOpen = false;
            return;
        }
        UsersList.Clear();

        //to be Made static Cancel previous Request if Another Made 
        var cts = new CancellationTokenSource().Token;
        var fetchSuggestionReqObj = new UserSuggestionRequestObject(new SplitExpenseVmPresenterCallBack(this), cts, SplittingUsersName.Trim().ToLower());

        var suggestionFetchUseCase = InstanceBuilder.CreateInstance<UserSuggestion>(fetchSuggestionReqObj);

        suggestionFetchUseCase.Execute();

    }
    public void InvokeOnUserSuggestionReceived(UserSuggestionResponseObject result)
    {
        _ = UiService.RunOnUiThread(() =>
        {
            foreach (var user in result.UserSuggestions)
            {
                UsersList.Add(user);
            }
            if (!UsersList.Any())
            {
                UsersList.Add(_dummyUser);
            }

            IsUserSuggestionListOpen = true;

        }, View.Dispatcher);
    }

    //if the splitting is successfull showing split completed text box and reset the page 
    public void InvokeOnSplitExpenseCompleted(SplitExpenseResponseObj result)
    {

        _ = UiService.RunOnUiThread(() =>
        {
            UiService.ShowContentAsync("Spitting SuccessFull", "Expenses Splitted Successfully",View.VisualRoot,View.Dispatcher);
            ResetPage();
        }, View.Dispatcher);
    }

    private readonly User _dummyUser = new User
    {
        UserName = "No Results Found"
    };

    private int _selectedUserIndex = -1;



    public void ListViewOnSelected()
    {
        if (SelectedUserIndex == -1) return;

        _selectedUser = UsersList[SelectedUserIndex];

        //checking whether the selected user obj is dummy obj for showing no results found and clearing text box
        if (string.IsNullOrEmpty(_selectedUser.EmailId))
        {
            _selectedUser = null;
            SplittingUsersName = "";
            return;

        }
        //clearing splitting user's list when single user is selected
        _usersToBeSplitted.Clear();

        _isInnerInvokationOfTextChanged = true;
        SplittingUsersName = _selectedUser?.UserName;
        UsersList.Clear();
        IsUserSuggestionListOpen = false;
    }



    #endregion

    #region GroupSelection logic region

    public int SelectedGroupIndex
    {
        get => _selectedGroupIndex;
        set => SetProperty(ref _selectedGroupIndex, value);
    }


    private ObservableCollection<GroupBobj> _userParticipatingGroups;
    public ObservableCollection<GroupBobj> UserParticipatingGroup
    {
        get
        {
            return _userParticipatingGroups ??= GetUserParticipatingGroup();
        }
    }


    public string SelectedGroupName
    {
        get => _selectedGroupName;
        set => SetProperty(ref _selectedGroupName, value);
    }

    public bool IsNameTextBoxReadOnly
    {
        get => _isNameTextBoxReadOnly;
        set => SetProperty(ref _isNameTextBoxReadOnly, value);
    }


    public User SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public int SelectedUserIndex
    {
        get => _selectedUserIndex;
        set => SetProperty(ref _selectedUserIndex, value);
    }

    private readonly GroupBobj _dummyGroup = new GroupBobj
    {
        GroupName = "Non Group Expense"
    };



    private ObservableCollection<GroupBobj> GetUserParticipatingGroup()
    {
        //dummy GroupBobj to display default value

        var observableGroupObj = new ObservableCollection<GroupBobj>
        {
            _dummyGroup
        };

        //Calling UseCase To PopulateGroups
        CallUserGroupsUseCase();

        return observableGroupObj;

        void CallUserGroupsUseCase()
        {
            var getUSerGroupReqObj = new GetUserGroupReq(CancellationToken.None, new SplitExpenseVmPresenterCallBack(this), Store.CurrentUserBobj);

            var getGroupsUseCase = InstanceBuilder.CreateInstance<GetUserGroups>(getUSerGroupReqObj);

            getGroupsUseCase.Execute();
        }
    }


    public void GroupItemSelectionChanged()
    {
        if (SelectedGroupIndex < 0)
        {
            return;
        }
        SelectedGroupName = UserParticipatingGroup[SelectedGroupIndex].GroupName;

        //if dummy is selected Clearing Users to be Splitted list so other UI Usecases work Properly

        SelectedGroupName = SelectedGroupName;

        if (SelectedGroupIndex == 0)
        {
            _isInnerInvokationOfTextChanged = true;
            SplittingUsersName = String.Empty;
            IsNameTextBoxReadOnly = false;
            return;
        }

        IsNameTextBoxReadOnly = true;
        _selectedUser = null;
        var splittingGroup = UserParticipatingGroup[SelectedGroupIndex];
        SplittingUsersName = String.Empty;
        _usersToBeSplitted.Clear();
        foreach (var user in splittingGroup.GroupParticipants)
        {
            _isInnerInvokationOfTextChanged = true;

            _usersToBeSplitted.Add(user);

            //ignoring current users name in the GROUP MEMBERS SPLITTING NAME
            if (user.EmailId == User.EmailId)
                continue;

            //CONCATING REMAINING USERS NAME TO THE USERS NAME TEXT BOX
            SplittingUsersName += "," + user.UserName;
        }
    }




    #endregion

    #region ExpenseAmountLogicRegion Single/Multiple  User


    private string _singleUserExpenseShareAmount;
    private bool _amountFormatIncorrectIndicatorVisibility;

    private bool _splitEquallyPanelVisibility;

    //By default the Selected option is SplitEqully
    private int _selectedSplitPreferenceIndex;
    /*       private bool _singleUserSelectionComboBoxVisibility;*/

    public string SingleUserExpenseShareAmount
    {
        get => _singleUserExpenseShareAmount;
        set => SetProperty(ref _singleUserExpenseShareAmount, value);
    }

    public bool AmountFormatIncorrectIndicatorVisibility
    {
        get => _amountFormatIncorrectIndicatorVisibility;
        set => SetProperty(ref _amountFormatIncorrectIndicatorVisibility, value);
    }

    public bool SplitEquallyPanelVisibility
    {
        get => _splitEquallyPanelVisibility;
        set => SetProperty(ref _splitEquallyPanelVisibility, value);
    }

    public int SelectedSplitPreferenceIndex
    {
        get => _selectedSplitPreferenceIndex;
        set => SetProperty(ref _selectedSplitPreferenceIndex, value);
    }


    private double _equalSplitAmount;

    public void ExpenseTextBoxValueChanged()
    {
        if (string.IsNullOrEmpty(SingleUserExpenseShareAmount))
        {
            return;
        }
        if (double.TryParse(SingleUserExpenseShareAmount, out _equalSplitAmount) && _equalSplitAmount > -1)
        {
            //not showing indicator if parsing is Successfull 
            AmountFormatIncorrectIndicatorVisibility = false;

        }
        else
        {
            //showing indicator if parsing is Successfull 
            AmountFormatIncorrectIndicatorVisibility = true;
        }
    }
    public void SplitPreferenceChanged()
    {
        if (SelectedSplitPreferenceIndex == 0)
        {
            //NOt showing teaching tip if unequal split option is selected
            IsUnEqualSplitPopUpVisible = false;

            SplitEquallyPanelVisibility = true;
            return;
        }
        //showing teaching tip if unequal split option is selected
        IsUnEqualSplitPopUpVisible = true;
        SplitEquallyPanelVisibility = false;

    }




    #endregion

    #region ExpenseDateSelectionRegion

    private DateTime _expenditureDate = DateTime.Now;

    public DateTimeOffset ExpenditureDate
    {
        get => _expenditureDate;
        set => SetProperty(ref _expenditureDate, value.DateTime);
    }

    public DateTimeOffset MaxYearDisplayLimit
    {
        get => DateTimeOffset.Now;
    }


    #endregion

    #region UnEqualSplitPopUpLogicRegion
    private bool _isUnEqualSplitPopUpVisible;

    public bool IsUnEqualSplitPopUpVisible
    {
        get => _isUnEqualSplitPopUpVisible;
        set => SetProperty(ref _isUnEqualSplitPopUpVisible, value);
    }

    public void UnequalSplitTeachingSplitClosed()
    {
        SelectedSplitPreferenceIndex = 0;
    }


    //Manipulates ExpenseViewModels for Unequal Splitting in teaching tip 
    private void SplittingUserPreferenceChanged()
    {
        _expensesToBeSplitted.Clear();

        //cheching whether it is dummy groupobj
        if (_selectedGroupIndex <= 0)
        {
            if (_selectedUser != null)//Individual Split
            {

                _expensesToBeSplitted.Add(GenerateExpenseViewModel(Store.CurrentUserBobj, null));//current User
                _expensesToBeSplitted.Add(GenerateExpenseViewModel(_selectedUser, null));//Spiltting user

            }
        }
        else//Group Split 
        {

            var group = UserParticipatingGroup[_selectedGroupIndex];

            foreach (var participant in group.GroupParticipants)
            {
                _expensesToBeSplitted.Add(GenerateExpenseViewModel(participant, group.GroupUniqueId));
            }
        }

    }

    private ExpenseBobj GenerateExpenseViewModel(User user, string groupUid)
    {
        var newExpenseObj = new ExpenseBobj(Store.CurrentUserBobj.CurrencyConverter)
        {
            RequestedOwner = Store.CurrentUserBobj.EmailId,
            SplitRaisedOwner = Store.CurrentUserBobj, //Currently by default current user is splitRaiseOwner , 
            UserEmailId = user.EmailId,
            CorrespondingUserObj = user,
            StrExpenseAmount = 0.0,
            GroupUniqueId = groupUid
        };
        if (_preferedCategory is not null) // if Category is Been set Explicitly set setting Currency Preference
        {
            newExpenseObj.CategoryId = _preferedCategory.Id;
        }
        return newExpenseObj;
    }

    #endregion

    #region ExpensesSplitFunctionality region

    private bool _isSplitButtonEnabled;
    private string _expenseNote;
    private string _expenseDescription;


    public bool IsSplitButtonEnabled
    {
        get => _isSplitButtonEnabled;
        set => SetProperty(ref _isSplitButtonEnabled, value);
    }

    public string ExpenseNote
    {
        get => _expenseNote;
        set => SetProperty(ref _expenseNote, value);
    }

    public string ExpenseDescription
    {
        get => _expenseDescription;
        set => SetProperty(ref _expenseDescription, value);
    }

    //event raises when collection item changes
    private void ExpensesToBeSplittedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var expensesBobjCount = _expensesToBeSplitted.Count;

        if (expensesBobjCount < 1)
        {
            IsSplitButtonEnabled = false;
            return;
        }

        //split button works if split is done for more than person 
        IsSplitButtonEnabled = true;

    }

    public void SplitPopulatedExpenses()
    {
        var expenseNote = ExpenseNote != null ? ExpenseNote.Trim() : string.Empty;
        var dateOfExpense = ExpenditureDate.DateTime;
        var expenseDescription = ExpenseDescription?.Trim() ?? string.Empty;

        var splittingType = SelectedSplitPreferenceIndex;// 0 if equal split or >0 for unequal split

        var ctk = new CancellationTokenSource().Token;

        var splitExpenseRequestObj = new SplitExpenseRequestObj(expenseDescription, Store.CurrentUserBobj, _expensesToBeSplitted, expenseNote, dateOfExpense, _equalSplitAmount, splittingType, ctk, new SplitExpenseVmPresenterCallBack(this));

        var splitExpenseUseCaseObj = InstanceBuilder.CreateInstance<SplitExpenses>(splitExpenseRequestObj);

        splitExpenseUseCaseObj.Execute();
    }

    private void ResetPage()//resets UserControl to initial stage
    {
        SplittingUsersName = string.Empty;
        _isInnerInvokationOfTextChanged = true;
        ExpenseNote = string.Empty;
        SelectedGroupIndex = 0;
        ExpenseDescription = string.Empty;
        SingleUserExpenseShareAmount = string.Empty;
        ExpenditureDate = new DateTimeOffset(DateTime.Today);
        SelectedSplitPreferenceIndex = 0;

    }



    #endregion

    #region ExpenseCategoryPreference

    private ExpenseCategory _preferedCategory;

    public void PreferedExpenseCategoryChanged(ExpenseCategory category)
    {
        if (category is null)
        {
            return;
        }
        _preferedCategory = category;
        foreach (var expense in _expensesToBeSplitted)
        {
            expense.CategoryId = category.Id;
        }
    }

    #endregion

    public string GetUserCurrencyPreference()
    {
        var preference = User.CurrencyPreference;
        return preference.ToString().ToUpper();
    }


    public SplitExpenseViewModel(ISplitExpenseView view)
    {
        View = view;
        User = new UserVobj(Store.CurrentUserBobj);
        _expensesToBeSplitted.CollectionChanged += ExpensesToBeSplittedOnCollectionChanged;

    }

    private class SplitExpenseVmPresenterCallBack : IPresenterCallBack<UserSuggestionResponseObject>, IPresenterCallBack<SplitExpenseResponseObj>, IPresenterCallBack<GetUserGroupResponse>
    {
        private readonly SplitExpenseViewModel _viewModel;
        public SplitExpenseVmPresenterCallBack(SplitExpenseViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public void OnSuccess(UserSuggestionResponseObject result)
        {
            _viewModel.InvokeOnUserSuggestionReceived(result);
        }
        public void OnSuccess(SplitExpenseResponseObj result)
        {
            _viewModel.InvokeOnSplitExpenseCompleted(result);
        }
        public void OnSuccess(GetUserGroupResponse result)
        {
            _ = UiService.RunOnUiThread(() =>
            {
                _viewModel.UserParticipatingGroup?.AddRange(result.UserParticipatingGroups);

            }, _viewModel.View.Dispatcher).ConfigureAwait(false);
        }


        #region ErrorHAndlingRegion

        public void OnError(SplittrException ex)
        {
            HandleError(ex);
        }

        private void HandleError(SplittrException ex)
        {
            switch (ex.InnerException)
            {
                case ArgumentException or ArgumentNullException:
                    ExceptionHandlerService.HandleException(ex.InnerException);
                    break;
                case SQLiteException:
                    //Retry Code Logic Here
                    break;
            }
        }

        #endregion


    }
}