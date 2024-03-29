﻿// <copyright file="DerivationChangeSet.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Meta;

    public class AccumulatedChangeSet : IAccumulatedChangeSet
    {
        private IDictionary<RoleType, ISet<IObject>> associationsByRoleType;

        private IDictionary<AssociationType, ISet<IObject>> rolesByAssociationType;

        internal AccumulatedChangeSet()
        {
            this.Created = new HashSet<IObject>();
            this.Deleted = new HashSet<IStrategy>();
            this.Associations = new HashSet<IObject>();
            this.Roles = new HashSet<IObject>();
            this.RoleTypesByAssociation = new Dictionary<IObject, ISet<RoleType>>();
            this.AssociationTypesByRole = new Dictionary<IObject, ISet<AssociationType>>();
        }

        public ISet<IObject> Created { get; }

        public ISet<IStrategy> Deleted { get; }

        public ISet<IObject> Associations { get; }

        public ISet<IObject> Roles { get; }

        public IDictionary<IObject, ISet<RoleType>> RoleTypesByAssociation { get; }

        public IDictionary<IObject, ISet<AssociationType>> AssociationTypesByRole { get; }

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

        public void Add(IChangeSet changeSet)
        {
            this.Created.UnionWith(changeSet.Created);
            this.Deleted.UnionWith(changeSet.Deleted);
            this.Associations.UnionWith(changeSet.Associations);
            this.Roles.UnionWith(changeSet.Roles);

            foreach ((IObject key, ISet<RoleType> value) in changeSet.RoleTypesByAssociation)
            {
                if (this.RoleTypesByAssociation.TryGetValue(key, out var roleTypes))
                {
                    roleTypes.UnionWith(value);
                }
                else
                {
                    this.RoleTypesByAssociation[key] = new HashSet<RoleType>(changeSet.RoleTypesByAssociation[key]);
                }
            }

            foreach ((IObject key, ISet<AssociationType> value) in changeSet.AssociationTypesByRole)
            {
                if (this.AssociationTypesByRole.TryGetValue(key, out var associationTypes))
                {
                    associationTypes.UnionWith(value);
                }
                else
                {
                    this.AssociationTypesByRole[key] = new HashSet<AssociationType>(changeSet.AssociationTypesByRole[key]);
                }
            }

            this.associationsByRoleType = null;
            this.rolesByAssociationType = null;
        }
    }
}
