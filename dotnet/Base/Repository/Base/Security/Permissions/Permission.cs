// <copyright file="Permission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>


namespace Allors.Repository;

using System;
using Allors.Repository.Attributes;

#region Allors
[Id("7fded183-3337-4196-afb0-3266377944bc")]
#endregion
public partial interface Permission : Deletable
{
    #region Allors
    [Id("29b80857-e51b-4dec-b859-887ed74b1626")]
    #endregion
    [Indexed]
    [Required]
    public Guid ClassPointer { get; set; }
}
