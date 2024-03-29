﻿// <copyright file="NotAssociationInEnumerable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System.Collections.Generic;
using System.Text;
using Allors.Database.Meta;

internal sealed class NotAssociationIntersectsEnumerable : In
{
    private readonly AssociationType association;
    private readonly IEnumerable<IObject> enumerable;

    internal NotAssociationIntersectsEnumerable(IInternalExtentFiltered extent, AssociationType association, IEnumerable<IObject> enumerable)
    {
        extent.CheckAssociation(association);
        PredicateAssertions.AssertAssociationIn(association, this.enumerable);
        this.association = association;
        this.enumerable = enumerable;
    }

    internal override bool IsNotFilter => true;

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;

        var inStatement = new StringBuilder("0");
        foreach (var inObject in this.enumerable)
        {
            inStatement.Append(",");
            inStatement.Append(inObject.Id.ToString());
        }

        if (this.association.RoleType.IsMany || !this.association.RoleType.ExistExclusiveClasses)
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForRole + " IS NULL OR ");
            statement.Append(" NOT " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForRole + " IN (\n");
            statement.Append(" SELECT " + Mapping.ColumnNameForRole + " FROM " +
                             schema.TableNameForRelationByRoleType[this.association.RoleType] + " WHERE " +
                             Mapping.ColumnNameForAssociation + " IN (");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }
        else if (this.association.RoleType.IsMany)
        {
            statement.Append(" (" + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + " IS NULL OR ");
            statement.Append(" NOT " + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + " IN (\n");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }
        else
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " IS NULL OR ");
            statement.Append(" NOT " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " IN (\n");
            statement.Append(inStatement.ToString());
            statement.Append(" ))\n");
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseAssociation(this.association);
}
