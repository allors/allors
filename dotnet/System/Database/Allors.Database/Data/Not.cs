﻿// <copyright file="Not.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data;

public class Not(IPredicate operand = null) : ICompositePredicate
{
    public IPredicate Operand { get; set; } = operand;

    bool IPredicate.ShouldTreeShake(IArguments arguments) => this.Operand == null || this.Operand.ShouldTreeShake(arguments);

    bool IPredicate.HasMissingArguments(IArguments arguments) => this.Operand?.HasMissingArguments(arguments) == true;

    void IPredicateContainer.AddPredicate(IPredicate predicate) => this.Operand = predicate;

    void IPredicate.Build(ITransaction transaction, IArguments arguments, Database.ICompositePredicate compositePredicate)
    {
        var not = compositePredicate.AddNot();

        if (this.Operand != null && !this.Operand.ShouldTreeShake(arguments))
        {
            this.Operand?.Build(transaction, arguments, not);
        }
    }

    public void Accept(IVisitor visitor) => visitor.VisitNot(this);
}
