﻿// <copyright file="Equals.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

internal sealed class EqualsComposite : Equals
{
    private readonly IObject equals;

    internal EqualsComposite(IObject equals)
    {
        PredicateAssertions.ValidateEquals(equals);
        this.equals = equals;
    }

    internal override ThreeValuedLogic Evaluate(Strategy strategy)
    {
        if (this.equals == null)
        {
            return ThreeValuedLogic.False;
        }

        return this.equals.Equals(strategy.GetObject())
            ? ThreeValuedLogic.True
            : ThreeValuedLogic.False;
    }
}
