﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SPLITTR_Uwp.Core.DataHandler.Contracts;
using SPLITTR_Uwp.Core.DataHandler.ServiceObjects;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.Core.Services.SqliteConnection;
using SPLITTR_Uwp.Core.Splittr_Uwp_BLogics.Blogic;

namespace SPLITTR_Uwp.Core.DataHandler
{
    public class ExpenseHistoryManager :UseCaseBase, IExpenseHistoryManager
    {
        private readonly ISqlDataServices _sqlDataServices;

        private readonly ConcurrentDictionary<string, ExpenseHistory> _expenseHistory = new ConcurrentDictionary<string, ExpenseHistory>(); 

        public ExpenseHistoryManager(ISqlDataServices sqlDataServices)
        {
            _sqlDataServices = sqlDataServices;
            _sqlDataServices.CreateTable<ExpenseHistory>();

        }
        public Task RecordExpenseMarkedAsPaid(string expenseId)
        {
            return RunAsynchronously(() =>
            {
                var recordExpenseHistory = new ExpenseHistory(expenseId);

                 _sqlDataServices.InsertObj(recordExpenseHistory);

            });
        }
        public async Task<bool> IsExpenseMarkedAsPaid(string expenseId)
        {
            var isExpenseMarkedAsPaid = _expenseHistory.ContainsKey(expenseId);

            if (isExpenseMarkedAsPaid)
            {
                return true;
            }
            var expenseHistory =await _sqlDataServices.FetchTable<ExpenseHistory>().FirstOrDefaultAsync(h => h.ExpenseUniqueId.Equals(expenseId)).ConfigureAwait(false);

            //if null(no history record) then that expense is not marked as Paid 
            return  _expenseHistory.GetOrAdd(expenseId,expenseHistory) is null;
        }

    }
}
