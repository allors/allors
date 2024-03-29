﻿// <copyright file="RoleExists.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using Allors.Database.Meta;

internal sealed class RoleExists : Exists
{
    private readonly RoleType role;

    internal RoleExists(IInternalExtentFiltered extent, RoleType role)
    {
        extent.CheckRole(role);
        PredicateAssertions.ValidateRoleExists(role);
        this.role = role;
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;
        if (this.role.ObjectType.IsUnit)
        {
            statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.role] + " IS NOT NULL");
        }
        else if ((this.role.IsMany && this.role.AssociationType.IsMany) || !this.role.ExistExclusiveClasses)
        {
            statement.Append(" " + this.role.SingularFullName + "_R." + Mapping.ColumnNameForRole + " IS NOT NULL");
        }
        else if (this.role.IsMany)
        {
            statement.Append(" " + this.role.SingularFullName + "_R." + Mapping.ColumnNameForObject + " IS NOT NULL");
        }
        else
        {
            statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.role] + " IS NOT NULL");
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseRole(this.role);
}
