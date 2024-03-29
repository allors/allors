﻿// <copyright file="IMetaPopulation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta;

public static class MetaPopulationExtensions
{
    public static void RegisterDerivations(this MetaPopulation @this)
    {
        @this.EmbeddedDerivationById[nameof(ObjectTypePluralNameDerivation)] = new ObjectTypePluralNameDerivation();
    }
}
