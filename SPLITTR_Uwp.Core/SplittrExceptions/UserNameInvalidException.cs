﻿using System;

namespace SPLITTR_Uwp.Core.SplittrExceptions;

public class UserNameInvalidException : Exception
{
    public UserNameInvalidException(string message) :base(message)
    {

    }
}