﻿// <copyright file="AssociationEquals.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using Allors.Database.Meta;

internal sealed class AssociationEquals : Equals
{
    private readonly IObject allorsObject;
    private readonly AssociationType association;

    internal AssociationEquals(IInternalExtentFiltered extent, AssociationType association, IObject allorsObject)
    {
        extent.CheckAssociation(association);
        PredicateAssertions.AssertAssociationEquals(association, allorsObject);
        this.association = association;
        this.allorsObject = allorsObject;
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        var schema = statement.Mapping;
        if ((this.association.IsMany && this.association.RoleType.IsMany) ||
            !this.association.RoleType.ExistExclusiveClasses)
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForAssociation + " IS NOT NULL AND ");
            statement.Append(" " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForAssociation + "=" +
                             this.allorsObject.Strategy.ObjectId + ")");
        }
        else if (this.association.RoleType.IsMany)
        {
            statement.Append(" (" + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + " IS NOT NULL AND ");
            statement.Append(" " + alias + "." + schema.ColumnNameByRoleType[this.association.RoleType] + "=" +
                             this.allorsObject.Strategy.ObjectId + ")");
        }
        else
        {
            statement.Append(" (" + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " IS NOT NULL AND ");
            statement.Append(" " + this.association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " =" +
                             this.allorsObject.Strategy.ObjectId + ")");
        }

        return this.Include;
    }

    internal override void Setup(ExtentStatement statement) => statement.UseAssociation(this.association);
}
