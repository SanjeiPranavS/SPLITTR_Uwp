﻿using System;
using Microsoft.Extensions.DependencyInjection;
using SPLITTR_Uwp.Core.Adapters.NetAdapter;
using SPLITTR_Uwp.Core.Adapters.SqlAdapter;
using SPLITTR_Uwp.Core.CurrencyCoverter;
using SPLITTR_Uwp.Core.CurrencyCoverter.Factory;
using SPLITTR_Uwp.Core.DataManager;
using SPLITTR_Uwp.Core.DataManager.Contracts;
using SPLITTR_Uwp.Core.DataManager.Services;
using SPLITTR_Uwp.Core.DbHandler;
using SPLITTR_Uwp.Core.DbHandler.Contracts;
using SPLITTR_Uwp.Core.NetHandler;
using SPLITTR_Uwp.Core.UseCase.LoginUser;
using SPLITTR_Uwp.Core.UseCase.MarkAsPaid;

namespace SPLITTR_Uwp.Core.DependencyInjector;

public class SplittrDependencyService
{
    private  static IServiceProvider _serviceProvider;

    private static IServiceProvider Container
    {
        get
        {
            _serviceProvider ??= RegisterServices(new ServiceCollection());
            return _serviceProvider;
        } 
    }

    public static T GetInstance<T>()
    {
        return Container.GetService<T>();
    }


    private static IServiceProvider RegisterServices(IServiceCollection container)
    {
        container.AddSingleton<ISqlDataAdapter, SqlDataBaseAdapter>()
            .AddSingleton<IUserDbHandler, UserDbHandler>()
            .AddSingleton<IExpenseDbHandler, ExpenseDbHandler>()
            .AddSingleton<IGroupDbHandler, GroupDbHandler>()
            .AddSingleton<IGroupToUserDbHandler, GroupToUserDbHandler>()
            .AddSingleton<IUserDataManager, UserDataManagerBase>()
            .AddSingleton<IGroupDataManager, GroupDataManagerBase>()
            .AddSingleton<IExpenseDataManager, ExpenseDataManager>()
            .AddSingleton<IGroupCreationDataManager, GroupCreationDataManager>()
            .AddSingleton<ICurrencyCalcFactory, CalculatorFactory>()
            .AddSingleton<IExpenseHistoryManager, ExpenseHistoryManager>()
            .AddSingleton<IUserProfileUpdateDataManager, UserUpdateDataManager>()
            .AddSingleton<IAddWalletBalanceDataManager, UserUpdateDataManager>()
            .AddSingleton<IEditExpenseDataManager,EditExpenseDataManager>()
            .AddSingleton<IUserSuggestionDataManager, UserUpdateDataManager>()
            .AddSingleton<IRelatedExpenseDataManager, ExpenseFetchDataManager>()
            .AddSingleton<ISplitExpenseDataManager, ExpenseStatusDataManager>()
            .AddSingleton<IMarkExpensePaidDataManager, ExpenseStatusDataManager>()
            .AddSingleton<IExpenseCancellationDataManager, ExpenseStatusDataManager>()
            .AddSingleton<ISettleUpSplitDataManager, SettleUpExpenseDataManager>()
            .AddSingleton<ISignUpDataManager, AuthenticationManager>()
            .AddSingleton<ICategoryUpdateDataManager,CategoryUpdateDataManager>()
            .AddSingleton<IGroupDetailManager,GroupDetailManager>()
            .AddSingleton<IExpenseFetchDataManager,ExpenseFetchDataManager>()
            .AddSingleton<IAuthenticationManager, AuthenticationManager>()
            .AddSingleton<IExpenseCategoryManager,ExpenseCategoryManager>()
            .AddSingleton<INetAdapter,HttpDataService>()
            .AddSingleton<IExpenseCategoryNetHandler,ExpenseCategoryNetHandler>()
            .AddTransient<IExpenseCategoryJsonToPoCoConverter,ExpenseCategoriesDeserializer>()
            .AddTransient<RupessConverter>()
            .AddTransient<DollarConverter>()
            .AddTransient<YenConverter>()
            .AddTransient<EuroConverter>();
        return container.BuildServiceProvider();

    }
        
}