﻿// <copyright file="IMetaPopulation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta;

public interface IStaticRelationType : IStaticMetaIdentifiableObject, IRelationType
{
    new IStaticAssociationType AssociationType { get; internal set; }

    new IStaticRoleType RoleType { get; internal set; }

    internal string Name { get; }

    internal void DeriveWorkspaceNames();
}