﻿// <copyright file="ExtentOperation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System;
using System.Collections.Generic;
using Allors.Database.Meta;

internal sealed class ExtentOperation<T> : Extent<T> where T : class, IObject
{
    private readonly IExtentOperand firstOperand;
    private readonly IExtentOperand secondOperand;
    private readonly ExtentOperations operations;

    public ExtentOperation(Transaction transaction, IExtentOperand firstOperand, IExtentOperand secondOperand, ExtentOperations operations)
        : base(transaction)
    {
        if (!firstOperand.ObjectType.IsAssignableFrom(secondOperand.ObjectType))
        {
            throw new ArgumentException("Both extents in a Union, Intersect or Except must be from the same type");
        }

        this.operations = operations;

        this.firstOperand = firstOperand;
        this.secondOperand = secondOperand;

        firstOperand.Parent = this;
        secondOperand.Parent = this;
    }

    public override ICompositePredicate Predicate => null;

    public override Composite ObjectType => this.firstOperand.ObjectType;

    protected override void Evaluate()
    {
        if (this.Strategies == null)
        {
            var firstOperandStrategies = new List<Strategy>(this.firstOperand.GetEvaluatedStrategies());
            var secondOperandStrategies = new List<Strategy>(this.secondOperand.GetEvaluatedStrategies());

            switch (this.operations)
            {
            case ExtentOperations.Union:
                this.Strategies = firstOperandStrategies;
                foreach (var strategy in secondOperandStrategies)
                {
                    if (!this.Strategies.Contains(strategy))
                    {
                        this.Strategies.Add(strategy);
                    }
                }

                break;

            case ExtentOperations.Intersect:
                this.Strategies = new List<Strategy>();
                foreach (var strategy in firstOperandStrategies)
                {
                    if (secondOperandStrategies.Contains(strategy))
                    {
                        this.Strategies.Add(strategy);
                    }
                }

                break;

            case ExtentOperations.Except:
                this.Strategies = firstOperandStrategies;
                foreach (var strategy in secondOperandStrategies)
                {
                    if (this.Strategies.Contains(strategy))
                    {
                        this.Strategies.Remove(strategy);
                    }
                }

                break;

            default:
                throw new Exception("Unknown extent operation");
            }

            if (this.Sorter != null)
            {
                this.Strategies.Sort(this.Sorter);
            }
        }
    }
}
