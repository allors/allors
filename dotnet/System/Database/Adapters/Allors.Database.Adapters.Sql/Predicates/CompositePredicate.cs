﻿// <copyright file="CompositePredicate.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;

internal abstract class CompositePredicate : Predicate, ICompositePredicate
{
    protected CompositePredicate(IInternalExtentFiltered extent)
    {
        this.Extent = extent;
        this.Filters = new List<Predicate>(4);
    }

    internal override bool Include
    {
        get
        {
            foreach (var filter in this.Filters)
            {
                if (filter.Include)
                {
                    return true;
                }
            }

            return false;
        }
    }

    protected IInternalExtentFiltered Extent { get; }

    protected List<Predicate> Filters { get; }

    public ICompositePredicate AddAnd(Action<ICompositePredicate> init = null)
    {
        var and = new And(this.Extent);
        init?.Invoke(and);

        this.Extent.FlushCache();
        this.Filters.Add(and);
        return and;
    }

    public IPredicate AddBetween(RoleType role, object firstValue, object secondValue)
    {
        this.Extent.FlushCache();

        Between between;
        if (firstValue is RoleType betweenRoleA && secondValue is RoleType betweenRoleB)
        {
            between = new RoleBetweenRole(this.Extent, role, betweenRoleA, betweenRoleB);
        }
        else if (firstValue is AssociationType || secondValue is AssociationType)
        {
            throw new NotImplementedException();
        }
        else
        {
            between = new RoleBetweenValue(this.Extent, role, firstValue, secondValue);
        }
        this.Extent.FlushCache();
        this.Filters.Add(between);
        return between;
    }

    public IPredicate AddIntersects(RoleType role, Allors.Database.IExtent<IObject> containingExtent)
    {
        In containedIn = role.IsMany
            ? new RoleIntersectsExtent(this.Extent, role, containingExtent)
            : new RoleInExtent(this.Extent, role, containingExtent);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIntersects(RoleType role, IEnumerable<IObject> containingEnumerable)
    {
        In containedIn = role.IsMany
            ? new RoleIntersectsEnumerable(this.Extent, role, containingEnumerable)
            : new RoleInEnumerable(this.Extent, role, containingEnumerable);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIntersects(AssociationType association, Allors.Database.IExtent<IObject> containingExtent)
    {
        In containedIn = association.IsMany
            ? new AssociationIntersectsExtent(this.Extent, association, containingExtent)
            : new AssociationInExtent(this.Extent, association, containingExtent);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIntersects(AssociationType association, IEnumerable<IObject> containingEnumerable)
    {
        In containedIn = association.IsMany
            ? new AssociationIntersectsEnumerable(this.Extent, association, containingEnumerable)
            : new AssociationInEnumerable(this.Extent, association, containingEnumerable);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }


    public IPredicate AddIn(RoleType role, Allors.Database.IExtent<IObject> containingExtent)
    {
        In containedIn = role.IsMany
            ? new RoleIntersectsExtent(this.Extent, role, containingExtent)
            : new RoleInExtent(this.Extent, role, containingExtent);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIn(RoleType role, IEnumerable<IObject> containingEnumerable)
    {
        In containedIn = role.IsMany
            ? new RoleIntersectsEnumerable(this.Extent, role, containingEnumerable)
            : new RoleInEnumerable(this.Extent, role, containingEnumerable);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIn(AssociationType association, Allors.Database.IExtent<IObject> containingExtent)
    {
        In containedIn = association.IsMany 
            ? new AssociationIntersectsExtent(this.Extent, association, containingExtent)
            : new AssociationInExtent(this.Extent, association, containingExtent);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddIn(AssociationType association, IEnumerable<IObject> containingEnumerable)
    {
        In containedIn = association.IsMany
            ? new AssociationIntersectsEnumerable(this.Extent, association, containingEnumerable)
            : new AssociationInEnumerable(this.Extent, association, containingEnumerable);

        this.Extent.FlushCache();
        this.Filters.Add(containedIn);
        return containedIn;
    }

    public IPredicate AddHas(RoleType role, IObject containedObject)
    {
        var contains = new RoleHas(this.Extent, role, containedObject);

        this.Extent.FlushCache();
        this.Filters.Add(contains);
        return contains;
    }

    public IPredicate AddHas(AssociationType association, IObject containedObject)
    {
        var contains = new AssociationHas(this.Extent, association, containedObject);

        this.Extent.FlushCache();
        this.Filters.Add(contains);
        return contains;
    }

    public IPredicate AddEquals(IObject allorsObject)
    {
        var equals = new EqualsComposite(allorsObject);

        this.Extent.FlushCache();
        this.Filters.Add(equals);
        return equals;
    }

    public IPredicate AddEquals(RoleType role, object obj)
    {
        Equals equals = obj switch
        {
            AssociationType => throw new NotImplementedException(),
            RoleType equalsRole => new RoleEqualsRole(this.Extent, role, equalsRole),
            _ => new RoleEqualsValue(this.Extent, role, obj)
        };

        this.Extent.FlushCache();
        this.Filters.Add(equals);
        return equals;
    }

    public IPredicate AddEquals(AssociationType association, IObject allorsObject)
    {
        var equals = new AssociationEquals(this.Extent, association, allorsObject);

        this.Extent.FlushCache();
        this.Filters.Add(equals);
        return equals;
    }

    public IPredicate AddExists(RoleType role)
    {
        var exists = new RoleExists(this.Extent, role);

        this.Extent.FlushCache();
        this.Filters.Add(exists);
        return exists;
    }

    public IPredicate AddExists(AssociationType association)
    {
        var exists = new AssociationExists(this.Extent, association);

        this.Extent.FlushCache();
        this.Filters.Add(exists);
        return exists;
    }

    public IPredicate AddGreaterThan(RoleType role, object value)
    {
        GreaterThan greaterThan = value switch
        {
            AssociationType greaterThanAssociation => throw new NotImplementedException(),
            RoleType greaterThanRole => new RoleGreaterThanRole(this.Extent, role, greaterThanRole),
            _ => new RoleGreaterThanValue(this.Extent, role, value)
        };

        this.Extent.FlushCache();
        this.Filters.Add(greaterThan);
        return greaterThan;
    }

    public IPredicate AddInstanceOf(Composite type)
    {
        var instanceOf = new InstanceOfComposite(type, GetConcreteSubClasses(type));

        this.Extent.FlushCache();
        this.Filters.Add(instanceOf);
        return instanceOf;
    }

    public IPredicate AddInstanceOf(RoleType role, Composite type)
    {
        var instanceOf = new RoleInstanceof(this.Extent, role, type, GetConcreteSubClasses(type));

        this.Extent.FlushCache();
        this.Filters.Add(instanceOf);
        return instanceOf;
    }

    public IPredicate AddInstanceOf(AssociationType association, Composite type)
    {
        var instanceOf = new AssociationInstanceOf(this.Extent, association, type, GetConcreteSubClasses(type));

        this.Extent.FlushCache();
        this.Filters.Add(instanceOf);
        return instanceOf;
    }

    public IPredicate AddLessThan(RoleType role, object value)
    {
        LessThan lessThan = value switch
        {
            AssociationType => throw new NotImplementedException(),
            RoleType lessThanRole => new RoleLessThanRole(this.Extent, role, lessThanRole),
            _ => new RoleLessThanValue(this.Extent, role, value)
        };

        this.Extent.FlushCache();
        this.Filters.Add(lessThan);
        return lessThan;
    }

    public IPredicate AddLike(RoleType role, string value)
    {
        var like = new Like(this.Extent, role, value);

        this.Extent.FlushCache();
        this.Filters.Add(like);
        return like;
    }

    public ICompositePredicate AddNot(Action<ICompositePredicate> init = null)
    {
        var not = new Not(this.Extent);
        init?.Invoke(not);

        this.Extent.FlushCache();
        this.Filters.Add(not);
        return not;
    }

    public ICompositePredicate AddOr(Action<ICompositePredicate> init = null)
    {
        var or = new Or(this.Extent);
        init?.Invoke(or);

        this.Extent.FlushCache();
        this.Filters.Add(or);
        return or;
    }

    internal static ObjectType[] GetConcreteSubClasses(ObjectType type)
    {
        if (type.IsInterface)
        {
            return ((Interface)type).Classes.ToArray();
        }

        var concreteSubclasses = new ObjectType[1];
        concreteSubclasses[0] = type;
        return concreteSubclasses;
    }

    internal override void Setup(ExtentStatement statement)
    {
        foreach (var filter in this.Filters)
        {
            filter.Setup(statement);
        }
    }
}
