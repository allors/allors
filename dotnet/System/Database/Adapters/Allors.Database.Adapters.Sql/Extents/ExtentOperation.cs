﻿// <copyright file="ExtentOperation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System;
using System.Collections.Generic;
using Allors.Database.Meta;

public class ExtentOperation<T> : Extent<T> where T : class, IObject
{
    private readonly IInternalExtent first;
    private readonly ExtentOperations operationType;
    private readonly IInternalExtent second;

    public ExtentOperation(IInternalExtent first, IInternalExtent second, ExtentOperations operationType)
    {
        if (!first.ObjectType.IsAssignableFrom(second.ObjectType))
        {
            throw new ArgumentException("Both extents in a Union, Intersect or Except must be from the same type");
        }

        this.first = first;
        this.second = second;
        this.operationType = operationType;

        first.ParentOperationExtent = this;
        second.ParentOperationExtent = this;
    }

    public override ICompositePredicate Predicate => null;

    public override Transaction Transaction => this.first.Transaction;

    public override Composite ObjectType => this.first.ObjectType;

    public override string BuildSql(ExtentStatement statement)
    {
        statement.Append("(");
        this.first.BuildSql(statement);
        statement.Append(")");

        switch (this.operationType)
        {
        case ExtentOperations.Union:
            statement.Append("\nUNION\n");
            break;

        case ExtentOperations.Intersect:
            statement.Append("\nINTERSECT\n");
            break;

        case ExtentOperations.Except:
            statement.Append("\n" + "EXCEPT" + "\n");
            break;
        }

        statement.Append("(");
        this.second.BuildSql(statement);
        statement.Append(")");

        return null;
    }

    protected override IList<long> GetObjectIds()
    {
        this.Transaction.State.Flush();

        var statement = new ExtentStatementRoot(this);
        var alias = this.BuildSql(statement);

        var objectIds = new List<long>();
        using (var command = statement.CreateDbCommand(alias))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var objectId = this.Transaction.State.GetObjectIdForExistingObject(reader.GetValue(0).ToString());
                    objectIds.Add(objectId);
                }
            }
        }

        return objectIds;
    }

    // TODO: Refactor this
    protected override void LazyLoadFilter()
    {
    }
}
