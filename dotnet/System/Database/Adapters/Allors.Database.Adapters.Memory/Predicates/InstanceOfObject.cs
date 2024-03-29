﻿// <copyright file="InstanceOf.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using Allors.Database.Meta;

internal sealed class InstanceOfObject : InstanceOf
{
    private readonly ObjectType objectType;

    internal InstanceOfObject(ObjectType objectType)
    {
        PredicateAssertions.ValidateInstanceof(objectType);

        this.objectType = objectType;
    }

    internal override ThreeValuedLogic Evaluate(Strategy strategy)
    {
        if (strategy.UncheckedObjectType.Equals(this.objectType))
        {
            return ThreeValuedLogic.True;
        }

        var metaCache = strategy.Transaction.Database.MetaCache;

        return this.objectType is Interface @interface && metaCache.GetSupertypesByComposite(strategy.UncheckedObjectType).Contains(@interface)
            ? ThreeValuedLogic.True
            : ThreeValuedLogic.False;
    }
}
