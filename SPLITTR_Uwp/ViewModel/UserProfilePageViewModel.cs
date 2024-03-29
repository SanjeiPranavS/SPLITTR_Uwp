﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.Core.ModelBobj.Enum;
using SPLITTR_Uwp.Core.SplittrEventArgs;
using SPLITTR_Uwp.Core.SplittrExceptions;
using SPLITTR_Uwp.Core.SplittrNotifications;
using SPLITTR_Uwp.Core.UseCase;
using SPLITTR_Uwp.Core.UseCase.UpdateUser;
using SPLITTR_Uwp.DataRepository;
using SPLITTR_Uwp.Services;
using SPLITTR_Uwp.ViewModel.Contracts;
using SPLITTR_Uwp.ViewModel.Vobj;

namespace SPLITTR_Uwp.ViewModel;

public class UserProfilePageViewModel : ObservableObject
{


    public UserVobj User { get; }


    public UserProfilePageViewModel()
    {
        User = new UserVobj(Store.CurrentUserBobj);
        SplittrNotification.UserObjUpdated += SplittrNotification_UserObjUpdated;
    }

    private void SplittrNotification_UserObjUpdated(UserBobjUpdatedEventArgs obj)
    {
        _ = UiService.RunOnUiThread(() =>
        {
           OnPropertyChanged(nameof(UserInitial));

        }).ConfigureAwait(false);
    }

    public void ViewDisposed()
    {
        SplittrNotification.UserObjUpdated -= SplittrNotification_UserObjUpdated;
    }

    public string UserInitial
    {
        get => User.UserName.GetUserInitial();
    }

    public string CurrencyPreference
    {
        get => Store.CurrentUserBobj?.CurrencyPreference.ToString();
    }

    public bool IsEditUserProfileVisible
    {
        get => _isEditUserProfileVisible;
        set => SetProperty(ref _isEditUserProfileVisible, value);
    }

    public List<string> _currencyList = new List<string>
    {
        "Rupees ₹",
        "Dollar $",
        "Euro   €",
        "Yen    ¥"
    };

    private bool _isEditUserProfileVisible;

    public void EditUserProfileClicked()
    {
        IsEditUserProfileVisible = true;
    }

    private string _currentUserName = String.Empty;
    public string CurrentUserName
    {
        get
        {
            _currentUserName = User.UserName;
            return _currentUserName;
        }
        set => _currentUserName = value;
    }

    private int _preferedCurrencyIndex;
    private bool _isUserNameEmptyIndicatorVisible;

    public int PreferendCurrencyIndex
    {
        get => User.CurrencyIndex;
        set => _preferedCurrencyIndex = value;
    }

    public bool IsUserNameEmptyIndicatorVisible
    {
        get => _isUserNameEmptyIndicatorVisible;
        set => SetProperty(ref _isUserNameEmptyIndicatorVisible, value);
    }

    public void CancelButtonClicked()
    {
        CurrentUserName = "";
        _preferedCurrencyIndex = -1;
        IsEditUserProfileVisible = false;
        IsUserNameEmptyIndicatorVisible = false;
    }
    public  void SaveButtonClicked()
    {
        if (string.IsNullOrEmpty(_currentUserName.Trim()))
        {
            IsUserNameEmptyIndicatorVisible = true;
            return;
        }
        IsUserNameEmptyIndicatorVisible = false;
        var cts = new CancellationTokenSource().Token;
        //useCase classes is updating the UserObj and its related data's
        var updateUserRequestOBj = new UpdateUserRequestObj(cts,new UserProfileVmPresenterCallBack(this),Store.CurrentUserBobj,_currentUserName,(Currency)_preferedCurrencyIndex);

        var updateUserUseCaseObj = InstanceBuilder.CreateInstance<UpdateUser>(updateUserRequestOBj);

        updateUserUseCaseObj.Execute();


    }
   
    public async void InvokeOnUserObjectUpdated(UpdateUserResponseObj result)
    { 

        await UiService.ShowContentAsync("Account Updated SuccessFully", "SuccessFull!!").ConfigureAwait(false);
        _ = UiService.RunOnUiThread(() =>
        {
            //showing Update successFull messageBox
            IsEditUserProfileVisible = false;
        }).ConfigureAwait(false);
    }
    public class UserProfileVmPresenterCallBack : IPresenterCallBack<UpdateUserResponseObj>
    {
        private readonly UserProfilePageViewModel _viewModel;
        public UserProfileVmPresenterCallBack(UserProfilePageViewModel viewModel)
        {
            _viewModel = viewModel;

        }
        public void OnSuccess(UpdateUserResponseObj result)
        {
            _viewModel.InvokeOnUserObjectUpdated(result);
        }
        public void OnError(SplittrException ex)
        {
            if (ex.InnerException is SqlException)
            {
                ExceptionHandlerService.HandleException(ex);
            }
        }
    }
}