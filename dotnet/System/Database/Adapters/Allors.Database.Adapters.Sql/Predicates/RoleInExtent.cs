﻿// <copyright file="RoleInExtent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using Allors.Database.Meta;

internal sealed class RoleInExtent : In
{
    private readonly IInternalExtent inExtent;
    private readonly RoleType role;

    internal RoleInExtent(IInternalExtentFiltered extent, RoleType role, Allors.Database.IExtent<IObject> inExtent)
    {
        extent.CheckRole(role);
        PredicateAssertions.ValidateRoleIn(role, inExtent);
        this.role = role;
        this.inExtent = ((IInternalExtent)inExtent).InExtent;
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;
        var inStatement = statement.CreateChild(this.inExtent, this.role);

        inStatement.UseAssociation(this.role.AssociationType);

        if (!this.role.ExistExclusiveClasses)
        {
            statement.Append(" (" + this.role.SingularFullName + "_R." + Mapping.ColumnNameForRole + " IS NOT NULL AND ");
            statement.Append(" " + this.role.SingularFullName + "_R." + Mapping.ColumnNameForAssociation + " IN (");
            this.inExtent.BuildSql(inStatement);
            statement.Append(" ))");
        }
        else if (this.role.IsMany)
        {
            statement.Append(" (" + this.role.SingularFullName + "_R." + Mapping.ColumnNameForObject + " IS NOT NULL AND ");
            statement.Append(" " + this.role.SingularFullName + "_R." + Mapping.ColumnNameForObject + " IN (");
            this.inExtent.BuildSql(inStatement);
            statement.Append(" ))");
        }
        else
        {
            statement.Append(" (" + schema.ColumnNameByRoleType[this.role] + " IS NOT NULL AND ");
            statement.Append(" " + schema.ColumnNameByRoleType[this.role] + " IN (");
            this.inExtent.BuildSql(inStatement);
            statement.Append(" ))");
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseRole(this.role);
}
