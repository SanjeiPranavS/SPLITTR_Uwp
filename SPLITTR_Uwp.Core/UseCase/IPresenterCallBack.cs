﻿namespace SPLITTR_Uwp.Core.UseCase;

public interface IPresenterCallBack<in T>
{
    void OnSuccess(T result);
    void OnError(SplittrException.SplittrException ex);
}