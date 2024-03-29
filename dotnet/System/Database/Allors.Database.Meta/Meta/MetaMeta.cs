﻿// <copyright file="MetaMeta.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Database.Meta;

using Embedded.Meta;

public class MetaMeta
{
    public MetaMeta(EmbeddedMeta meta)
    {
        this.ObjectTypeSingularName = meta.AddUnit<ObjectType, string>(nameof(ObjectType.SingularName));
        this.ObjectTypeAssignedPluralName = meta.AddUnit<ObjectType, string>(nameof(ObjectType.AssignedPluralName));
        this.ObjectTypePluralName = meta.AddUnit<ObjectType, string>(nameof(ObjectType.PluralName));

        this.RoleTypeObjectType = meta.AddUnit<RoleType, ObjectType>(nameof(RoleType.ObjectType));
        this.RoleTypeIsDerived = meta.AddUnit<RoleType, bool>(nameof(RoleType.IsDerived));
    }

    public EmbeddedRoleType ObjectTypeSingularName { get; set; }

    public EmbeddedRoleType ObjectTypeAssignedPluralName { get; set; }

    public EmbeddedRoleType ObjectTypePluralName { get; set; }

    public EmbeddedRoleType RoleTypeObjectType { get; set; }

    public EmbeddedRoleType RoleTypeIsDerived { get; set; }
}
