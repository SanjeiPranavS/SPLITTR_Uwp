﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using SPLITTR_Uwp.Core.Splittr_Uwp_BLogics.Blogic;
using SPLITTR_Uwp.DataRepository;
using SPLITTR_Uwp.Services;
using SPLITTR_Uwp.ViewModel.Models;
using System;
using SPLITTR_Uwp.Core.Splittr_Uwp_BLogics.Blogic.contracts;

namespace SPLITTR_Uwp.ViewModel
{
    public class WalletBalanceUpdateViewModel : ObservableObject
    {
        private readonly IUserUseCase _updateUserUseCase;
        private string _moneyTextBoxText;
        private bool _invalidInputTextBlockVisibility;

        public event Action CloseButtonClicked;

        public UserViewModel UserViewModel { get; }

        public WalletBalanceUpdateViewModel(IUserUseCase updateUserUseCase)
        {
            _updateUserUseCase = updateUserUseCase;
        
            UserViewModel = new UserViewModel(Store.CurreUserBobj);
        }

        public string MoneyTextBoxText
        {
            get => _moneyTextBoxText;
            set => SetProperty(ref _moneyTextBoxText, value);
        }

        public bool InvalidInputTextBlockVisibility
        {
            get => _invalidInputTextBlockVisibility;
            set => SetProperty(ref _invalidInputTextBlockVisibility, value);
        }


        public  void AddMoneyToWalletButtonClicked()
        {
            //Wallet adding money should be non negative and a number
            if (double.TryParse(MoneyTextBoxText, out var newWalletBalance) && newWalletBalance > -1)
            {
                InvalidInputTextBlockVisibility = false;
                _updateUserUseCase.UpdateUserObjAsync(Store.CurreUserBobj, newWalletBalance,(async () =>
                {
                  await  UiService.RunOnUiThread(async () =>
                    {
                        await UiService.ShowContentAsync("Amount Added to Wallet SuccessFully", "Payment SuccessFull!!");
                      CloseButtonClicked?.Invoke();
                    });

                }));
                
            }
            else
            {
                InvalidInputTextBlockVisibility = true;
            }

        }



    }
}
