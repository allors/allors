﻿// <copyright file="Class.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Repository.Domain;

using System.Collections.Generic;
using System.Linq;
using Inflector;

public class Class : Composite
{
    public Class(Inflector inflector, ISet<RepositoryObject> objects, string name, Domain domain)
        : base(inflector, objects, name, domain)
    {
    }

    public override Interface[] Interfaces
    {
        get
        {
            var interfaces = new HashSet<Interface>(this.ImplementedInterfaces);
            foreach (var implementedInterface in this.ImplementedInterfaces)
            {
                interfaces.UnionWith(implementedInterface.Interfaces);
            }

            return [.. interfaces];
        }
    }

    public Property GetImplementedProperty(Property property)
    {
        foreach (var @interface in this.ImplementedInterfaces)
        {
            var implementedProperty = @interface.GetImplementedProperty(property);
            if (implementedProperty != null)
            {
                return implementedProperty;
            }
        }

        return null;
    }

    public Method GetImplementedMethod(Method method)
    {
        foreach (var @interface in this.ImplementedInterfaces)
        {
            var implementedProperty = @interface.GetImplementedMethod(method);
            if (implementedProperty != null)
            {
                return implementedProperty;
            }
        }

        return null;
    }
}
