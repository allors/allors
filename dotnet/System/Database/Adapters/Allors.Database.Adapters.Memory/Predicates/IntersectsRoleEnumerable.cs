﻿// <copyright file="InRoleManyEnumerable.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;

internal sealed class IntersectsRoleEnumerable : In
{
    private readonly IEnumerable<IObject> containingEnumerable;
    private readonly RoleType roleType;

    internal IntersectsRoleEnumerable(IInternalExtent extent, RoleType roleType, IEnumerable<IObject> containingEnumerable)
    {
        extent.CheckForRoleType(roleType);
        PredicateAssertions.ValidateRoleIn(roleType, containingEnumerable);

        this.roleType = roleType;
        this.containingEnumerable = containingEnumerable;
    }

    internal override ThreeValuedLogic Evaluate(Strategy strategy)
    {
        var containing = new HashSet<IObject>(this.containingEnumerable);

        return strategy.GetCompositesRole<IObject>(this.roleType).Any(role => containing.Contains(role))
            ? ThreeValuedLogic.True
            : ThreeValuedLogic.False;
    }
}
