// <copyright file="Localized.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>


namespace Allors.Repository;

using Attributes;
using static Workspaces;

#region Allors
[Id("7979a17c-0829-46df-a0d4-1b01775cfaac")]
#endregion
public interface Localized : Object
{
    #region Allors
    [Id("8c005a4e-5ffe-45fd-b279-778e274f4d83")]
    #endregion
    [Indexed]
    [Workspace(Default)]
    Locale Locale { get; set; }
}