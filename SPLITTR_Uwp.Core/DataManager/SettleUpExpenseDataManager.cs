﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPLITTR_Uwp.Core.DataManager.Contracts;
using SPLITTR_Uwp.Core.DbHandler.SqliteConnection;
using SPLITTR_Uwp.Core.EventArg;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.ModelBobj.Enum;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.Core.UseCase;
using SPLITTR_Uwp.Core.UseCase.SettleUpExpense;

namespace SPLITTR_Uwp.Core.DataManager
{

    public class SettleUpExpenseDataManager : ISettleUpSplitDataManager
    {
        private readonly IUserDataManager _userDataManager;
        private readonly ISqlDataServices _sqlDataServices;
        private readonly IExpenseDataManager _expenseDataManager;

        public SettleUpExpenseDataManager(IUserDataManager userDataManager, ISqlDataServices sqlDataServices, IExpenseDataManager expenseDataManager)
        {
            _userDataManager = userDataManager;
            _sqlDataServices = sqlDataServices;
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
                throw new NotSupportedException();
            }
        }

        /// <exception cref="NotSupportedException">thrown if Insufficient Wallet Balance</exception>
        public async void SettleUpExpenses(ExpenseBobj settleExpenseRef, UserBobj currentUser, IUseCaseCallBack<SettleUpExpenseResponseObj> callBack, bool isWalletTransaction = false)
        {

            try
            {
                ValidateInputs(settleExpenseRef, currentUser);

                var toBeSettledExpenseObj = currentUser.Expenses.FirstOrDefault(ex => ex.ExpenseUniqueId.Equals(settleExpenseRef.ExpenseUniqueId));

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


                await _sqlDataServices.RunInTransaction(() =>
                {
                    _userDataManager.UpdateUserBobjAsync(requestedOwner);
                    _expenseDataManager.UpdateExpenseAsync(toBeSettledExpenseObj);
                }).ConfigureAwait(false);

                currentUser.Expenses.RemoveAndAdd(toBeSettledExpenseObj);

                callBack?.OnSuccess(new SettleUpExpenseResponseObj(toBeSettledExpenseObj));

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

    }
}