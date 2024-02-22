﻿// <copyright file="ObjectType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Database.Meta;

using System.Collections.Generic;
using System.Linq;

public static class ICompositeExtensions
{
    internal static void ValidateComposite(this Composite @this, ValidationLog validationLog)
    {
        if (@this.RoleTypes.Count(v => v.RelationType.IsKey) > 1)
        {
            var message = @this.ValidationName() + " has more than 1 key";
            validationLog.AddError(message, @this, ValidationKind.Multiplicity, "IComposite.KeyRoleType");
        }
    }

    internal static void InitializeSupertypes(this Composite @this)
    {
        var supertypes = new HashSet<Interface>();
        @this.InitializeSupertypesRecursively(@this, supertypes);
        @this.Supertypes = supertypes.ToArray();
    }

    internal static void InitializeRoleTypes(this Composite @this, Dictionary<Composite, HashSet<RoleType>> roleTypesByAssociationObjectType)
    {
        var roleTypes = new HashSet<RoleType>();

        if (roleTypesByAssociationObjectType.TryGetValue(@this, out var directRoleTypes))
        {
            roleTypes.UnionWith(directRoleTypes);
        }

        foreach (var superType in @this.Supertypes.Cast<Composite>())
        {
            if (roleTypesByAssociationObjectType.TryGetValue(superType, out var inheritedRoleTypes))
            {
                roleTypes.UnionWith(inheritedRoleTypes);
            }
        }

        @this.RoleTypes = roleTypes.ToArray();
    }

    internal static void InitializeAssociationTypes(this Composite @this, Dictionary<ObjectType, HashSet<AssociationType>> relationTypesByRoleObjectType)
    {
        var associationTypes = new HashSet<AssociationType>();

        if (relationTypesByRoleObjectType.TryGetValue(@this, out var classAssociationTypes))
        {
            associationTypes.UnionWith(classAssociationTypes);
        }

        foreach (var superType in @this.Supertypes.Cast<Interface>())
        {
            if (relationTypesByRoleObjectType.TryGetValue(superType, out var interfaceAssociationTypes))
            {
                associationTypes.UnionWith(interfaceAssociationTypes);
            }
        }

        @this.AssociationTypes = associationTypes.ToArray();
    }

    internal static void InitializeMethodTypes(this Composite @this, Dictionary<Composite, HashSet<MethodType>> methodTypeByClass)
    {
        var methodTypes = new HashSet<MethodType>();

        if (methodTypeByClass.TryGetValue(@this, out var directMethodTypes))
        {
            methodTypes.UnionWith(directMethodTypes);
        }

        foreach (var superType in @this.Supertypes.Cast<Interface>())
        {
            if (methodTypeByClass.TryGetValue(superType, out var inheritedMethodTypes))
            {
                methodTypes.UnionWith(inheritedMethodTypes);
            }
        }

        @this.MethodTypes = methodTypes.ToArray();
    }

    internal static void InitializeCompositeRoleTypes(this Composite @this, Dictionary<Composite, HashSet<CompositeRoleType>> compositeRoleTypesByComposite)
    {
        var compositeRoleTypes = compositeRoleTypesByComposite[@this];
        @this.CompositeRoleTypeByRoleType = compositeRoleTypes.ToDictionary(v => v.RoleType, v => v);
    }

    internal static void InitializeCompositeMethodTypes(this Composite @this, Dictionary<Composite, HashSet<CompositeMethodType>> compositeMethodTypesByComposite)
    {
        var compositeMethodTypes = compositeMethodTypesByComposite[@this];
        @this.CompositeMethodTypeByMethodType = compositeMethodTypes.ToDictionary(v => v.MethodType, v => v);
    }

    internal static void InitializeSupertypesRecursively(this Composite @this, ObjectType type, ISet<Interface> superTypes)
    {
        foreach (var directSupertype in @this.DirectSupertypes.Cast<Interface>())
        {
            if (!Equals(directSupertype, type))
            {
                superTypes.Add(directSupertype);
                directSupertype.InitializeSupertypesRecursively(type, superTypes);
            }
        }
    }

    internal static void DeriveKeyRoleType(this Composite @this)
    {
        @this.DerivedKeyRoleType = @this.RoleTypes.FirstOrDefault(v => v.RelationType.IsKey);
    }
}