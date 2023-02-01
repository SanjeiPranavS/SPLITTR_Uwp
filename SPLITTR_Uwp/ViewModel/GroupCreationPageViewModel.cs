﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using SPLITTR_Uwp.Core.ExtensionMethod;
using SPLITTR_Uwp.Core.Models;
using SPLITTR_Uwp.Core.Splittr_Uwp_BLogics.Blogic;
using SPLITTR_Uwp.Core.Splittr_Uwp_BLogics.Blogic.contracts;
using SPLITTR_Uwp.DataRepository;
using SPLITTR_Uwp.Services;
using SPLITTR_Uwp.ViewModel.Contracts;
using SPLITTR_Uwp.ViewModel.Models;
using SQLite;

namespace SPLITTR_Uwp.ViewModel
{
    internal class GroupCreationPageViewModel : ObservableObject,IValueConverter,IViewModel
    {
        private readonly IGroupUseCase _groupCreationUseCase;
        private readonly IUserUseCase _updateUserUseCase;
        private string _groupName;


        public UserViewModel User { get;}
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public string GetCurrentUserInitial
        {
            get => User.UserName.GetUserInitial();
        }

        public ObservableCollection<User> UserSuggestionList { get; } = new ObservableCollection<User>();

        public ObservableCollection<User> GroupParticipants { get; } = new ObservableCollection<User>();

        public GroupCreationPageViewModel(IUserUseCase updateUserUseCase,IGroupUseCase groupCreationUseCase)
        {
            
            _updateUserUseCase = updateUserUseCase;
            _groupCreationUseCase = groupCreationUseCase;
            Store.CurreUserBobj.ValueChanged += UserBobj_ValueChanged;
            User = new UserViewModel(Store.CurreUserBobj);

        }


        private User _dummyUser = new User()
        {
            UserName = "No results Found"
        };
        public void PopulateSuggestionList(string userName)
        { 
            _updateUserUseCase.GetUsersSuggestionAsync(userName.ToLower(), async (suggestions) =>
           {
               await UiService.RunOnUiThread((() =>
               {
                   foreach (var suggestedUser in suggestions)
                   {
                       if(GroupParticipants.Contains(suggestedUser)) continue; //suggestion is not showed if the user is already added to group participants
                       UserSuggestionList.Add(suggestedUser);
                   }
                   if (!UserSuggestionList.Any())
                   {
                       UserSuggestionList.Add(_dummyUser);
                   }

               }));
           });
        }
        public void GroupCreateButtonClicked(object sender, RoutedEventArgs e)
        {
            var groupName = _groupName.Trim();

            if (_groupCreationUseCase is IUseCase useCase) //On Failed doing Respective Actions
            {
                useCase.OnError += UseCase_OnError;
            }

            _groupCreationUseCase.CreateSplittrGroup(GroupParticipants,Store.CurreUserBobj,groupName, async () =>
            {
                await UiService.RunOnUiThread(async () =>
                { 
                    await UiService.ShowContentAsync($"{_groupName} Group Created SuccessFull", "SuccessFully Created !! ");

                    //clearing groupAdding PAge controls 
                    GroupParticipants.Clear();
                    GroupName = String.Empty;
                });
                Debug.WriteLine("******************************Group Created Successfully *******************************************************");
            });
        }

        private void UseCase_OnError(Exception arg, string arg2)
        {
            switch (arg)
            {
                case ArgumentException or ArgumentNullException:
                    ExceptionHandlerService.HandleException(arg);
                    break;
                case SQLiteException:
                    //Retry Code Logic Here
                    break;
            }
        }

      
        

        private async void UserBobj_ValueChanged(string property)
        {
            //since this Will be called by Worker thread it needs to invoked by Ui thread so calling dispatcher to user it
            
          await UiService.RunOnUiThread((() =>
            {
                      BindingUpdateInvoked?.Invoke();

            }));

        }



        #region UserInitialConvertRegion
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var name = (string)value;
            return name.GetUserInitial();
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }


        #endregion


        public event Action BindingUpdateInvoked;
    }
}
