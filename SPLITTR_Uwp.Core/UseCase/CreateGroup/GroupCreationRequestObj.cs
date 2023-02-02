﻿using System.Collections.Generic;
using System.Threading;
using SPLITTR_Uwp.Core.ModelBobj;
using SPLITTR_Uwp.Core.Models;

namespace SPLITTR_Uwp.Core.UseCase.CreateGroup;

/// <summary>
/// Group Creation Request Object
/// </summary>
public class GroupCreationRequestObj : IRequestObj<GroupCreationResponseObj>
{
    public CancellationToken Cts { get; }

    public IPresenterCallBack<GroupCreationResponseObj> PresenterCallBack { get; }

    public IEnumerable<User> GroupParticiPants { get; set; }

    public UserBobj CurrentUser { get; }

    public string GroupName { get; }

    public GroupCreationRequestObj(CancellationToken cts, IPresenterCallBack<GroupCreationResponseObj> presenterCallBack, UserBobj currentUser, IEnumerable<User> groupParticiPants, string groupName)
    {
        Cts = cts;
        PresenterCallBack = presenterCallBack;
        CurrentUser = currentUser;
        GroupParticiPants = groupParticiPants;
        GroupName = groupName;

    }
}
