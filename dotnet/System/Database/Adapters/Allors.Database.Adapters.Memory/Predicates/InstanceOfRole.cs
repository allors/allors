﻿// <copyright file="RoleInstanceOf.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using Allors.Database.Meta;

internal sealed class InstanceOfRole : InstanceOf
{
    private readonly Composite objectType;
    private readonly RoleType roleType;

    internal InstanceOfRole(IInternalExtent extent, RoleType roleType, Composite objectType)
    {
        extent.CheckForRoleType(roleType);
        PredicateAssertions.ValidateRoleInstanceOf(roleType, objectType);

        this.roleType = roleType;
        this.objectType = objectType;
    }

    internal override ThreeValuedLogic Evaluate(Strategy strategy)
    {
        var role = strategy.GetCompositeRole(this.roleType);

        if (role == null)
        {
            return ThreeValuedLogic.False;
        }

        // TODO: Optimize
        var roleObjectType = role.Strategy.Class;
        if (roleObjectType.Equals(this.objectType))
        {
            return ThreeValuedLogic.True;
        }

        var metaCache = strategy.Transaction.Database.MetaCache;

        return this.objectType is Interface @interface && metaCache.GetSupertypesByComposite(roleObjectType).Contains(@interface)
            ? ThreeValuedLogic.True
            : ThreeValuedLogic.False;
    }
}
