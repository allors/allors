﻿// <copyright file="Permission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors.Database.Security;
    using Allors.Database.Meta;

    public partial interface Permission : IPermission
    {
        bool ExistClass { get; }

        bool ExistOperation { get; }

        Operations Operation { get; }

        bool ExistOperandType { get; }

        OperandType OperandType { get; }

    }
}
