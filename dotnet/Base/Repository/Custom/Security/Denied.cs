﻿// <copyright file="Denied.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>

namespace Allors.Repository;

using Allors.Repository.Attributes;

#region Allors

[Id("437A152B-09EF-4DAD-BD35-E629F46A9249")]

#endregion

public class Denied : Object
{
    #region Allors

    [Id("449C1F0C-A63C-47B0-ABAC-3EE3511C6B23")]

    #endregion

    public string DatabaseProperty { get; set; }

    #region Allors

    [Id("58A94BD3-8784-4A51-8CC0-219889B4561E")]

    #endregion

    public string DefaultWorkspaceProperty { get; set; }

    #region Allors

    [Id("E48302E5-184C-40A8-AEDB-C7B38A515906")]

    #endregion

    public string WorkspaceXProperty { get; set; }

    #region inherited

    public DelegatedAccess AccessDelegation { get; set; }
    public Revocation[] Revocations { get; set; }

    public SecurityToken[] SecurityTokens { get; set; }

    public void OnBuild()
    {
        throw new System.NotImplementedException();
    }

    public void OnPostBuild()
    {
    }

    public void OnInit()
    {
    }

    public void OnPostDerive()
    {
    }

    #endregion
}
