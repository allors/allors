﻿// <copyright file="Login.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>


namespace Allors.Repository;

using Allors.Repository.Attributes;

#region Allors
[Id("ad7277a8-eda4-4128-a990-b47fe43d120a")]
#endregion
public partial class Login : Deletable
{
    #region Allors
    [Id("18262218-a14f-48c3-87a5-87196d3b5974")]
    #endregion
    [Indexed]
    [Size(256)]
    public string Key { get; set; }

    #region Allors
    [Id("7a82e721-d0b7-4567-aaef-bd3987ae6d01")]
    #endregion
    [Indexed]
    [Size(256)]
    public string Provider { get; set; }

    #region Allors
    [Id("B7957FD9-A43C-4E1E-844E-CA115DD33B23")]
    #endregion
    [Indexed]
    [Size(256)]
    public string DisplayName { get; set; }

    #region inherited

    public DelegatedAccess AccessDelegation { get; set; }
    public Revocation[] Revocations { get; set; }
    
    public SecurityToken[] SecurityTokens { get; set; }

    public void OnBuild() { }

    public void OnPostBuild() { }

    public void OnInit() { }

    public void OnPostDerive() { }

    public void Delete() { }

    #endregion
}
