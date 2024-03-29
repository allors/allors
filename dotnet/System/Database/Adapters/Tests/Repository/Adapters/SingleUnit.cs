﻿// <copyright file="SingleUnit.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>

namespace Allors.Repository;

using Allors.Repository.Attributes;

#region Allors
[Id("c3e82ab0-f586-4913-acb0-838ffd6701f8")]
#endregion
public class SingleUnit : Object
{
    #region Allors
    [Id("acf7d284-2480-4a09-a13b-ba4ba96e0892")]
    #endregion
    public int AllorsInteger { get; set; }

    #region inherited properties
    #endregion

    #region inherited methods
    public void OnBuild()
    {
    }

    public void OnPostBuild()
    {
    }

    #endregion
}
