﻿// <copyright file="IMetaPopulation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Domain type.</summary>

namespace Allors.Workspace.Meta
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class MetaPopulation : IMetaPopulation
    {
        public IReadOnlyList<IUnit> Units { get; protected set; }

        public IReadOnlyList<IInterface> Interfaces { get; protected set; }

        public IReadOnlyList<IClass> Classes { get; protected set; }

        public IReadOnlyList<IRoleType> RoleTypes { get; set; }

        public IReadOnlyList<IMethodType> MethodTypes { get; set; }

        public IReadOnlyDictionary<string, IMetaIdentifiableObject> MetaObjectByTag { get; private set; }

        public IReadOnlyList<IComposite> Composites { get; private set; }

        public IReadOnlyDictionary<string, IComposite> CompositeByLowercaseName { get; private set; }

        public IMetaIdentifiableObject FindByTag(string tag)
        {
            this.MetaObjectByTag.TryGetValue(tag, out var metaObject);
            return metaObject;
        }

        public IComposite FindByName(string name)
        {
            this.CompositeByLowercaseName.TryGetValue(name.ToLowerInvariant(), out var composite);
            return composite;
        }

        public void Bind(Type[] types)
        {
            var typeByName = types.ToDictionary(type => type.Name, type => type);

            foreach (Unit unit in this.Units)
            {
                unit.Bind();
            }

            foreach (var @interface in this.Interfaces)
            {
                @interface.Bind(typeByName);
            }

            foreach (var @class in this.Classes)
            {
                @class.Bind(typeByName);
            }
        }

        public void Init()
        {
            this.MetaObjectByTag =
                this.Units.Cast<IMetaIdentifiableObject>()
                    .Union(this.Classes)
                    .Union(this.Interfaces)
                    .Union(this.RoleTypes)
                    .Union(this.MethodTypes)
                    .ToDictionary(v => v.Tag, v => v);

            this.Composites = this.Interfaces.Cast<IComposite>().Union(this.Classes).ToArray();
            this.CompositeByLowercaseName = this.Composites.ToDictionary(v => v.SingularName.ToLowerInvariant());

            // DirectSubtypes
            foreach (Interface @interface in this.Interfaces)
            {
                @interface.InitializeDirectSubtypes();
            }

            // Supertypes
            foreach (Composite composite in this.Composites)
            {
                composite.InitializeSupertypes();
            }

            // Subtypes
            foreach (Interface @interface in this.Interfaces)
            {
                static IEnumerable<IComposite> RecurseDirectSubtypes(Interface @interface)
                {
                    if (@interface.DirectSubtypes != null)
                    {
                        foreach (var directSubtype in @interface.DirectSubtypes)
                        {
                            yield return directSubtype;

                            if (directSubtype is Interface directSubinterface)
                            {
                                foreach (var directSubSubtype in RecurseDirectSubtypes(directSubinterface))
                                {
                                    yield return directSubSubtype;
                                }
                            }
                        }
                    }
                }

                @interface.Subtypes = RecurseDirectSubtypes(@interface).ToArray();
                @interface.Classes = @interface.Subtypes.Where(v => v.IsClass).Cast<IClass>().ToArray();
            }

            // RoleTypes
            {
                foreach (RoleType roleType in this.RoleTypes)
                {
                    roleType.InitializeSizeScaleAndPrecision();
                }

                var exclusiveRoleTypesObjectType = this.RoleTypes
                .GroupBy(v => v.AssociationType.ObjectType)
                .ToDictionary(g => g.Key, g => g.ToArray());

                foreach (Composite objectType in this.Composites)
                {
                    exclusiveRoleTypesObjectType.TryGetValue(objectType, out var exclusiveRoleTypes);
                    objectType.ExclusiveRoleTypes = exclusiveRoleTypes ?? Array.Empty<RoleType>();
                }
            }

            // AssociationTypes
            {
                var exclusiveAssociationTypesByObjectType = this.RoleTypes
                    .GroupBy(v => v.ObjectType)
                    .ToDictionary(g => g.Key, g => g.Select(v => v.AssociationType).ToArray());

                foreach (Composite objectType in this.Composites)
                {
                    exclusiveAssociationTypesByObjectType.TryGetValue(objectType, out var exclusiveAssociationTypes);
                    objectType.ExclusiveAssociationTypes = exclusiveAssociationTypes ?? Array.Empty<AssociationType>();
                }
            }

            // MethodTypes
            {
                var exclusiveMethodTypeByObjectType = this.MethodTypes
                    .Cast<MethodType>()
                    .GroupBy(v => v.ObjectType)
                    .ToDictionary(g => g.Key, g => g.ToArray());

                foreach (Composite objectType in this.Composites)
                {
                    exclusiveMethodTypeByObjectType.TryGetValue(objectType, out var exclusiveMethodTypes);
                    objectType.ExclusiveMethodTypes = exclusiveMethodTypes ?? Array.Empty<MethodType>();
                }
            }

            // RoleTypes & AssociationTypes
            var roleTypesByAssociationTypeObjectType = this.RoleTypes
                .GroupBy(v => (Composite)v.AssociationType.ObjectType)
                .ToDictionary(g => g.Key, g => new HashSet<RoleType>(g.Select(v => (RoleType)v)));

            var associationTypesByRoleTypeObjectType = this.RoleTypes
                .GroupBy(v => (ObjectType)v.ObjectType)
                .ToDictionary(g => g.Key, g => new HashSet<AssociationType>(g.Select(v => (AssociationType)v.AssociationType)));

            // RoleTypes
            foreach (Composite composite in this.Composites)
            {
                composite.InitializeRoleTypes(roleTypesByAssociationTypeObjectType);
            }

            // AssociationTypes
            foreach (Composite composite in this.Composites)
            {
                composite.InitializeAssociationTypes(associationTypesByRoleTypeObjectType);
            }

            // MethodTypes
            var methodTypeByClass = this.MethodTypes
                .GroupBy(v => (Composite)v.ObjectType)
                .ToDictionary(g => g.Key, g => new HashSet<IMethodType>(g));

            foreach (Composite composite in this.Composites)
            {
                composite.InitializeMethodTypes(methodTypeByClass);
            }
        }
    }
}
