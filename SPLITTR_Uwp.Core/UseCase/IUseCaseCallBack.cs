﻿namespace SPLITTR_Uwp.Core.UseCase;

public interface IUseCaseCallBack<in T>
{
    public void OnSuccess(T responseObj);
    public void OnError(SplittrException.SplittrException error);

}