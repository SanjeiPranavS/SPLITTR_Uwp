﻿using SPLITTR_Uwp.Core.DataManager;
using SPLITTR_Uwp.Core.DependencyInjector;

namespace SPLITTR_Uwp.Core.UseCase.UpdateUser;

public class UpdateUser : UseCaseBase<UpdateUserResponseObj>
{
    private readonly IUserProfileUpdateDataManager _dataManager;

    private readonly UpdateUserRequestObj _requestObj;

    public UpdateUser( UpdateUserRequestObj requestObj) : base(requestObj.PresenterCallBack,requestObj.Cts)
    {
        _dataManager = SplittrDependencyService.GetInstance<IUserProfileUpdateDataManager>();
        _requestObj = requestObj;
    }
    protected override void Action()
    {
        _dataManager.UpdateUserObjAsync(_requestObj.CurrentUser,_requestObj.NewUserName,_requestObj.CurrencyPreference,new UseCaseCallBackBase<UpdateUserResponseObj>(this));
    }
     
}