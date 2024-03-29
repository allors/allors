﻿// <copyright file="AndPredicate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

internal sealed class And : CompositePredicate
{
    internal And(IInternalExtentFiltered extent) : base(extent)
    {
    }

    internal override bool BuildWhere(ExtentStatement statement, string alias)
    {
        if (this.Include)
        {
            var root = this.Extent.Predicate == null || this.Extent.Predicate.Equals(this);
            if (root)
            {
                var wherePresent = this.Extent.ObjectType.ExclusiveClass == null;
                statement.Append(wherePresent ? " AND " : " WHERE ");
            }
            else
            {
                statement.Append("(");
            }

            var atLeastOneChildIncluded = false;
            foreach (var filter in this.Filters)
            {
                if (atLeastOneChildIncluded)
                {
                    statement.Append(" AND ");
                }

                if (filter.BuildWhere(statement, alias))
                {
                    atLeastOneChildIncluded = true;
                }
            }

            if (!root)
            {
                statement.Append(")");
            }

            return atLeastOneChildIncluded;
        }

        return false;
    }
}
