﻿// <copyright file="ITime.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Signals
{
    public interface ICompositeRoleWrapper<T>
    {
        T Value { get; set; }
    }
}