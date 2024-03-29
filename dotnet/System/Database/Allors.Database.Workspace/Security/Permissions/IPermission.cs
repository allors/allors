// <copyright file="Permission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Security;

using Allors.Database.Meta;

public interface IPermission : IObject
{
    Class Class { get; }

    bool InWorkspace(string workspaceName);
}
