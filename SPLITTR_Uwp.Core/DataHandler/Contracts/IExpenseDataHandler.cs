﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SPLITTR_Uwp.Core.ModelBobj;

namespace SPLITTR_Uwp.Core.DataHandler.Contracts
{
    public interface IExpenseDataHandler
    {
        Task InsertExpenseAsync(ExpenseBobj expenseBobj);
        Task UpdateExpenseAsync(ExpenseBobj expenseBobj);
        Task<IEnumerable<ExpenseBobj>> GetUserExpensesAsync(string userEmailId);
    }
}
