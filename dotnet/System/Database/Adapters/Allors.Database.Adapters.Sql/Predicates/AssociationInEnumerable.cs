﻿// <copyright file="AssociationInEnumerable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System.Collections.Generic;
using System.Text;
using Allors.Database.Meta;

internal sealed class AssociationInEnumerable : In
{
    private readonly AssociationType association;
    private readonly IEnumerable<IObject> enumerable;

    internal AssociationInEnumerable(IInternalExtentFiltered extent, AssociationType association, IEnumerable<IObject> enumerable)
    {
        extent.CheckAssociation(association);
        PredicateAssertions.AssertAssociationIn(association, this.enumerable);
        this.association = association;
        this.enumerable = enumerable;
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;

        var inStatement = new StringBuilder("0");
        foreach (var inObject in this.enumerable)
        {
            inStatement.Append(",");
            inStatement.Append(inObject.Id.ToString());
        }

        if (!this.association.RoleType.ExistExclusiveClasses)
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForAssociation + " IS NOT NULL AND ");
            statement.Append(" " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForAssociation + " IN (\n");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }
        else if (this.association.RoleType.IsMany)
        {
            statement.Append(" (" + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + " IS NOT NULL AND ");
            statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + " IN (\n");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }
        else
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " IS NOT NULL AND ");
            statement.Append(" " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " IN (\n");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseAssociation(this.association);
}
