﻿using System;
using System.Collections.Concurrent;
using SPLITTR_Uwp.Core.Adapters.SqlAdapter;
using SPLITTR_Uwp.Core.DataManager.Contracts;
using SPLITTR_Uwp.Core.DataManager.Services;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.Core.SplittrExceptions;
using SPLITTR_Uwp.Core.UseCase;
using SPLITTR_Uwp.Core.UseCase.VerifyPaidExpense;
using SQLite;

namespace SPLITTR_Uwp.Core.DataManager;

public class ExpenseHistoryManager : IExpenseHistoryManager 
{
    private readonly ISqlDataAdapter _sqlDataAdapter;

    private readonly ConcurrentDictionary<string, ExpenseHistory> _expenseHistory = new ConcurrentDictionary<string, ExpenseHistory>();

    public ExpenseHistoryManager(ISqlDataAdapter sqlDataAdapter)
    {
        _sqlDataAdapter = sqlDataAdapter;
        _sqlDataAdapter.CreateTable<ExpenseHistory>();

    }
    public void RecordExpenseMarkedAsPaid(string expenseId)
    {

        var recordExpenseHistory = new ExpenseHistory(expenseId);

        _sqlDataAdapter.InsertObj(recordExpenseHistory);

    }

    private readonly object _lock = new object();

    public async void IsExpenseMarkedAsPaid(string expenseId,IUseCaseCallBack<VerifyPaidExpenseResponseObj> callBack)
    {
        try
        {
            lock (_lock)
            {
                var isExpenseMarkedAsPaid = _expenseHistory.ContainsKey(expenseId);

                if (isExpenseMarkedAsPaid)
                {
                    callBack.OnSuccess(new VerifyPaidExpenseResponseObj(true));
                    return;
                }

            }

            var expenseHistory = await _sqlDataAdapter.FetchTable<ExpenseHistory>().FirstOrDefaultAsync(h => h.ExpenseUniqueId.Equals(expenseId)).ConfigureAwait(false);

            switch (expenseHistory)
            {
                //if null(no history record) then that expense is not marked as Paid 
                case null:
                    callBack.OnSuccess(new VerifyPaidExpenseResponseObj(false));
                    return;

                default:
                    var result = _expenseHistory.GetOrAdd(expenseId, expenseHistory) is null;
                    callBack.OnSuccess(new VerifyPaidExpenseResponseObj(result));
                    break;

            }

        }
        catch (SQLiteException ex)
        {
            callBack?.OnError(new SplittrException(ex, "Db Fetch Error"));
        }
        catch (Exception ex)
        {
            callBack?.OnError(new SplittrException(ex,ex.Message));
        }
    }
}