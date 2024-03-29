﻿// <copyright file="Contains.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Data;

using Allors.Database.Meta;

public class Has(RelationEndType relationEndType = null) : IPropertyPredicate
{
    public IObject Object { get; set; }

    public string Parameter { get; set; }

    public RelationEndType RelationEndType { get; set; } = relationEndType;

    bool IPredicate.ShouldTreeShake(IArguments arguments) => ((IPredicate)this).HasMissingArguments(arguments);

    bool IPredicate.HasMissingArguments(IArguments arguments) => this.Parameter != null && arguments?.HasArgument(this.Parameter) != true;

    void IPredicate.Build(ITransaction transaction, IArguments arguments, Database.ICompositePredicate compositePredicate)
    {
        var containedObject = this.Parameter != null ? transaction.GetObject(arguments.ResolveObject(this.Parameter)) : this.Object;

        if (this.RelationEndType is RoleType roleType)
        {
            compositePredicate.AddHas(roleType, containedObject);
        }
        else
        {
            var associationType = (AssociationType)this.RelationEndType;
            compositePredicate.AddHas(associationType, containedObject);
        }
    }

    public void Accept(IVisitor visitor) => visitor.VisitHas(this);
}
