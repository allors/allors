// <copyright file="AccessControlList.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Security;

using Allors.Database.Domain;
using Allors.Database.Meta;

/// <summary>
///     List of permissions for an object/user combination.
/// </summary>
public interface IAccessControlList
{
    IVersionedGrant[] Grants { get; }

    IVersionedRevocation[] Revocations { get; }

    bool CanRead(RoleType roleType);

    bool CanWrite(RoleType roleType);

    bool CanExecute(MethodType methodType);

    bool IsMasked();
}
