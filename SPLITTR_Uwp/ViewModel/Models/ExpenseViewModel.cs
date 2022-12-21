﻿using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SPLITTR_Uwp.Core.CurrencyCoverter;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.ModelBobj.Enum;
using SPLITTR_Uwp.Core.Models;

namespace SPLITTR_Uwp.ViewModel.Models
{
    public class ExpenseViewModel : ExpenseBobj,INotifyPropertyChanged
    {
        private readonly ExpenseBobj _expense;

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override  string Note
        {
            get => _expense.Note;
            set
            {
                base.Note = value;
                _expense.Note = value;
                OnPropertyChanged();
            }
        }

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override double ExpenseAmount
        {
            get => _expense.ExpenseAmount;
            set
            {
                base.ExpenseAmount = value;
                _expense.ExpenseAmount = value;
                OnPropertyChanged();
            }
        }

        [System.Diagnostics.DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override  ExpenseStatus ExpenseStatus
        {
            get => _expense.ExpenseStatus;
        }



        public ExpenseViewModel(ExpenseBobj expense) : base(expense)
        {
            _expense = expense;
            _expense.ValueChanged += InnerObjValueChanged;
        }



        public void InnerObjValueChanged()
        {
            OnPropertyChanged(nameof(ExpenseAmount));
            OnPropertyChanged(nameof(ExpenseStatus));
            OnPropertyChanged(nameof(Note));

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
