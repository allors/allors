﻿// <copyright file="Deletable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>

namespace Allors.Repository;

using Allors.Repository.Attributes;

#region Allors
[Id("9279e337-c658-4086-946d-03c75cdb1ad3")]
#endregion
public partial interface Deletable : Object
{
    #region Allors
    [Id("430702D2-E02B-45AD-9B22-B8331DC75A3F")]
    #endregion
    void Delete();
}
