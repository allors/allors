﻿// <copyright file="RoleLessThanValue.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using Allors.Database.Meta;

internal sealed class RoleLessThanValue : LessThan
{
    private readonly object obj;
    private readonly RoleType roleType;

    internal RoleLessThanValue(IInternalExtentFiltered extent, RoleType roleType, object obj)
    {
        extent.CheckRole(roleType);
        PredicateAssertions.ValidateRoleLessThan(roleType, obj);
        this.roleType = roleType;
        this.obj = roleType.Normalize(obj);
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;
        statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.roleType] + " < " + statement.AddParameter(this.obj));
        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseRole(this.roleType);
}
