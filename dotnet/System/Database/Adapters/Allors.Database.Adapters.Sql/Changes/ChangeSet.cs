// <copyright file="ChangeSet.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsChangeSetMemory type.
// </summary>

namespace Allors.Database.Adapters.Sql;

using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;

internal sealed class ChangeSet : IChangeSet
{
    private ISet<IObject> associations;
    private IDictionary<RoleType, ISet<IObject>> associationsByRoleType;
    private ISet<IObject> roles;
    private IDictionary<AssociationType, ISet<IObject>> rolesByAssociationType;

    internal ChangeSet(ISet<IObject> created, ISet<IStrategy> deleted, IDictionary<IObject, ISet<RoleType>> roleTypesByAssociation,
        IDictionary<IObject, ISet<AssociationType>> associationTypesByRole)
    {
        this.Created = created;
        this.Deleted = deleted;
        this.RoleTypesByAssociation = roleTypesByAssociation;
        this.AssociationTypesByRole = associationTypesByRole;
    }

    public ISet<IObject> Created { get; }

    public ISet<IStrategy> Deleted { get; }

    public IDictionary<IObject, ISet<RoleType>> RoleTypesByAssociation { get; }

    public IDictionary<IObject, ISet<AssociationType>> AssociationTypesByRole { get; }

    public ISet<IObject> Associations => this.associations ??= new HashSet<IObject>(this.RoleTypesByAssociation.Keys);

    public ISet<IObject> Roles => this.roles ??= new HashSet<IObject>(this.AssociationTypesByRole.Keys);

    public IDictionary<RoleType, ISet<IObject>> AssociationsByRoleType => this.associationsByRoleType ??=
        (from kvp in this.RoleTypesByAssociation
            from value in kvp.Value
            group kvp.Key by value)
        .ToDictionary(grp => grp.Key, grp => new HashSet<IObject>(grp) as ISet<IObject>);

    public IDictionary<AssociationType, ISet<IObject>> RolesByAssociationType => this.rolesByAssociationType ??=
        (from kvp in this.AssociationTypesByRole
            from value in kvp.Value
            group kvp.Key by value)
        .ToDictionary(grp => grp.Key, grp => new HashSet<IObject>(grp) as ISet<IObject>);
}
