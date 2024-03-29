﻿using System;
using System.Collections.Generic;
using System.Linq;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.ModelBobj.Enum;

namespace SPLITTR_Uwp.Core.ExtensionMethod;

public static class ExtensionMethod
{
    /// <summary>
    /// Adds the IEnumerable of T to the Existing ICollection of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static ICollection<T> AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
    {
        if (target is List<T> list)
        {
            list.AddRange(source);
            return list;
        }
        foreach (var obj in source)
        {
            target.Add(obj);
        }

        return target;

    }


    public static  string GetUserInitial(this string userName)
    {

        var names = userName.Split(' ');
        var initials = "";
        foreach (var name in names)
        {
            initials += name[0];
            if (initials.Length == 2)
            {
                break;
            }
        }
        return initials;
    }



    public static bool ContainsString(this string text, IEnumerable<string> words)
    {
        if (text == null)
        {
            return false;
        }
        text = text.ToLower();
        foreach (var word in words)
        {
            var comparisonString = word.ToLower();
            if (text.Contains(comparisonString))
            {
                return true;
            }
        }
        return false;
    }
    public static string ExpenseAmount(this double amount, UserBobj user)
    {
        if (user is null)
        {
            return string.Empty;
        }

        var resultString = amount.ToString("##,###.000");
        var symbol = user.CurrencyPreference switch
        {
            Currency.Rupee => " ₹",
            Currency.Dollar => " $",
            Currency.Euro => " €",
            Currency.Yen => " ¥",
            _ => throw new Exception("Data Handling problem by Db")
        };
        return symbol+" "+resultString;

    }
    public static string ExpenseSymbol(this double amount, UserBobj user)
    {
        if (user is null)
        {
            return string.Empty;
        }

        var symbol = user.CurrencyPreference switch
        {
            Currency.Rupee => " ₹",
            Currency.Dollar => " $",
            Currency.Euro => " €",
            Currency.Yen => " ¥",
            _ => throw new Exception("Data Handling problem by Db")
        };
        return symbol;

    }

    public static void RemoveAndAdd<T>(this ICollection<T> target, T obj)
    {
        var removedAndAddedObj = target.FirstOrDefault(e => e.Equals(obj));

        if (removedAndAddedObj is null)
        {
            return;
        }
        target.Remove(removedAndAddedObj);
        target.Add(obj);
    }


}