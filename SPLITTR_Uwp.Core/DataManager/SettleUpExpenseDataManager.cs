﻿using System;
using System.Threading.Tasks;
using SPLITTR_Uwp.Core.Adapters.SqlAdapter;
using SPLITTR_Uwp.Core.DataManager.Contracts;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.ModelBobj.Enum;
using SPLITTR_Uwp.Core.SplittrEventArgs;
using SPLITTR_Uwp.Core.SplittrExceptions;
using SPLITTR_Uwp.Core.SplittrNotifications;
using SPLITTR_Uwp.Core.UseCase;
using SPLITTR_Uwp.Core.UseCase.SettleUpExpense;

namespace SPLITTR_Uwp.Core.DataManager;

public class SettleUpExpenseDataManager : ISettleUpSplitDataManager
{
    private readonly IUserDataManager _userDataManager;
    private readonly ISqlDataAdapter _sqlDataAdapter;
    private readonly IExpenseDataManager _expenseDataManager;

    public SettleUpExpenseDataManager(IUserDataManager userDataManager, ISqlDataAdapter sqlDataAdapter, IExpenseDataManager expenseDataManager)
    {
        _userDataManager = userDataManager;
        _sqlDataAdapter = sqlDataAdapter;
        _expenseDataManager = expenseDataManager;

    }


    private void ValidateInputs(ExpenseBobj settleExpenseRef, UserBobj currentUser)
    {

        if (settleExpenseRef is null || currentUser is null)
        {
            throw new ArgumentException("One Or more Passed Argument is Null");
        }
        //current user cannot pay their own Expense 
        if (settleExpenseRef.SplitRaisedOwner.Equals(currentUser))
        {
            throw new NotSupportedException("Not the Owner of the Expense");
        }
    }

    /// <exception cref="NotSupportedException">thrown if Insufficient Wallet Balance</exception>
    public async void SettleUpExpenses(ExpenseBobj settleExpenseRef, UserBobj currentUser, IUseCaseCallBack<SettleUpExpenseResponseObj> callBack, bool isWalletTransaction = false)
    {

        try
        {
            ValidateInputs(settleExpenseRef, currentUser);

            var toBeSettledExpenseObj = settleExpenseRef;

            if (toBeSettledExpenseObj is null)
            {
                return;
            }
            var requestedOwner = toBeSettledExpenseObj.SplitRaisedOwner;

            if (isWalletTransaction)
            {
                if (currentUser.StrWalletBalance < toBeSettledExpenseObj.StrExpenseAmount)
                {
                    throw new NotSupportedException("Insufficient Wallet Balance");

                }
                currentUser.WalletBalance -= toBeSettledExpenseObj.ExpenseAmount;

                await _userDataManager.UpdateUserBobjAsync(currentUser).ConfigureAwait(false);

            }
            requestedOwner.WalletBalance += toBeSettledExpenseObj.ExpenseAmount;
            toBeSettledExpenseObj.ExpenseStatus = ExpenseStatus.Paid;

            await _sqlDataAdapter.RunInTransaction(async () =>
            {
                await UpdateCreditDetails(toBeSettledExpenseObj, currentUser).ConfigureAwait(false);
                await _expenseDataManager.UpdateExpenseAsync(toBeSettledExpenseObj).ConfigureAwait(false);
            }).ConfigureAwait(false);

            callBack?.OnSuccess(new SettleUpExpenseResponseObj(toBeSettledExpenseObj));

            SplittrNotification.InvokeExpenseStatusChanged(new ExpenseStatusChangedEventArgs(ExpenseStatus.Paid,toBeSettledExpenseObj));
        }
        catch (NotSupportedException ex)
        {
            callBack?.OnError(new SplittrException(ex, "Insufficient Wallet Balance"));
        }
        catch (Exception ex)
        {
            callBack?.OnError(new SplittrException(ex, ex.Message));
        }
    }

    private async Task UpdateCreditDetails(ExpenseBobj toBeSettleExpenseBobj, UserBobj currentUser)
    {
        var requestOwner = toBeSettleExpenseBobj.SplitRaisedOwner;
        var correspondingUser = toBeSettleExpenseBobj.CorrespondingUserObj;

        if (currentUser.Equals(requestOwner))//assign to Current USerBobj So Change Notification raised
        {
            currentUser.StrLentAmount -= toBeSettleExpenseBobj.ExpenseAmount;
            requestOwner = currentUser;
        }
        else
        {
            requestOwner.LentAmount -= toBeSettleExpenseBobj.ExpenseAmount;
        }
        if (currentUser.Equals(correspondingUser))
        {
            currentUser.StrOwingAmount -= toBeSettleExpenseBobj.ExpenseAmount;
            correspondingUser = currentUser;
        }
        else
        {
            correspondingUser.OwingAmount -= toBeSettleExpenseBobj.ExpenseAmount;
        }


        await _userDataManager.UpdateUserBobjAsync(requestOwner).ConfigureAwait(false);
        await _userDataManager.UpdateUserBobjAsync(correspondingUser).ConfigureAwait(false);
    }
}