﻿// <copyright file="RoleEqualsValue.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System;
using Allors.Database.Meta;

internal sealed class RoleEqualsValue : Equals
{
    private readonly object obj;
    private readonly RoleType roleType;

    internal RoleEqualsValue(IInternalExtentFiltered extent, RoleType roleType, object obj)
    {
        extent.CheckRole(roleType);
        PredicateAssertions.ValidateRoleEquals(roleType, obj);
        this.roleType = roleType;
        if (obj is Enum enumeration)
        {
            if (((Unit)roleType.ObjectType).IsInteger)
            {
                this.obj = Convert.ToInt32(enumeration);
            }
            else
            {
                throw new Exception("Role Object Type " + roleType.ObjectType.SingularName + " doesn't support non int enumerations.");
            }
        }
        else
        {
            this.obj = roleType.ObjectType.IsUnit ? roleType.Normalize(obj) : obj;
        }
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;
        if (this.roleType.ObjectType.IsUnit)
        {
            statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.roleType] + "=" +
                             statement.AddParameter(this.obj));
        }
        else
        {
            var allorsObject = (IObject)this.obj;

            if (this.roleType.ExistExclusiveClasses)
            {
                statement.Append(" (" + alias + "." + schema.ColumnNameByRoleType[this.roleType] + " IS NOT NULL AND ");
                statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.roleType] + "=" +
                                 allorsObject.Strategy.ObjectId + ")");
            }
            else
            {
                statement.Append(" (" + this.roleType.SingularFullName + "_R." + Mapping.ColumnNameForRole + " IS NOT NULL AND ");
                statement.Append(" " + this.roleType.SingularFullName + "_R." + Mapping.ColumnNameForRole + "=" +
                                 allorsObject.Strategy.ObjectId + ")");
            }
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseRole(this.roleType);
}
