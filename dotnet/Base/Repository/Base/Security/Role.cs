﻿// <copyright file="Role.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>

namespace Allors.Repository;

using System;
using Allors.Repository.Attributes;

#region Allors
[Id("af6fe5f4-e5bc-4099-bcd1-97528af6505d")]
#endregion
public partial class Role : UniquelyIdentifiable
{
    #region Allors
    [Id("51e56ae1-72dc-443f-a2a3-f5aa3650f8d2")]
    #endregion
    [Indexed]
    public Permission[] Permissions { get; set; }

    #region Allors
    [Id("934bcbbe-5286-445c-a1bd-e2fcc786c448")]
    #endregion
    [Required]
    [Size(256)]
    public string Name { get; set; }

    #region inherited

    public DelegatedAccess AccessDelegation { get; set; }
    public Revocation[] Revocations { get; set; }


    public SecurityToken[] SecurityTokens { get; set; }

    public Guid UniqueId { get; set; }

    public void OnBuild()
    {
        
    }

    public void OnPostBuild() { }

    public void OnInit()
    {
    }

    public void OnPostDerive() { }

    #endregion
}
