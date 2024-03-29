﻿// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System;
using System.Collections.Generic;
using System.Xml;
using Allors.Database.Services;
using Allors.Database.Tracing;
using Allors.Database.Meta;

public class Database : IDatabase
{
    private readonly Dictionary<ObjectType, object> concreteClassesByObjectType;
    private Transaction transaction;

    public Database(IDatabaseServices services, Configuration configuration)
    {
        this.Services = services;
        if (this.Services == null)
        {
            throw new Exception("Services is missing");
        }

        this.ObjectFactory = configuration.ObjectFactory;
        if (this.ObjectFactory == null)
        {
            throw new Exception("Configuration.ObjectFactory is missing");
        }

        this.MetaPopulation = this.ObjectFactory.MetaPopulation;

        this.concreteClassesByObjectType = new Dictionary<ObjectType, object>();

        this.Id = string.IsNullOrWhiteSpace(configuration.Id) ? Guid.NewGuid().ToString("N").ToLowerInvariant() : configuration.Id;

        this.Services.OnInit(this);

        this.MetaCache = this.Services.Get<IMetaCache>();
    }

    internal bool IsRestoring { get; private set; }

    internal IMetaCache MetaCache { get; set; }

    protected virtual Transaction Transaction => this.transaction ??= new Transaction(this, this.Services.CreateTransactionServices());

    public event ObjectNotRestoredEventHandler ObjectNotRestored;

    public event RelationNotRestoredEventHandler RelationNotRestored;

    public string Id { get; }

    public bool IsShared => false;

    public IObjectFactory ObjectFactory { get; }

    public MetaPopulation MetaPopulation { get; }

    public IDatabaseServices Services { get; }

    ITransaction IDatabase.CreateTransaction() => this.CreateDatabaseTransaction();

    public void Restore(XmlReader reader)
    {
        this.Init();

        try
        {
            this.IsRestoring = true;

            var restore = new Restore(this.Transaction, reader);
            restore.Execute();

            this.Transaction.Commit();
        }
        finally
        {
            this.IsRestoring = false;
        }
    }

    public void Backup(XmlWriter writer) => this.Transaction.Backup(writer);

    public ISink Sink { get; set; }

    public virtual void Init()
    {
        this.Transaction.Init();

        this.transaction = null;

        this.Services.OnInit(this);
    }

    public ITransaction CreateTransaction() => this.CreateDatabaseTransaction();

    public ITransaction CreateDatabaseTransaction() => this.Transaction;

    public bool ContainsClass(Composite objectType, ObjectType concreteClass)
    {
        if (!this.concreteClassesByObjectType.TryGetValue(objectType, out var concreteClassOrClasses))
        {
            if (objectType.ExclusiveClass != null)
            {
                concreteClassOrClasses = objectType.ExclusiveClass;
                this.concreteClassesByObjectType[objectType] = concreteClassOrClasses;
            }
            else
            {
                concreteClassOrClasses = new HashSet<ObjectType>(objectType.Classes);
                this.concreteClassesByObjectType[objectType] = concreteClassOrClasses;
            }
        }

        if (concreteClassOrClasses is ObjectType)
        {
            return concreteClass.Equals(concreteClassOrClasses);
        }

        var concreteClasses = (HashSet<ObjectType>)concreteClassOrClasses;
        return concreteClasses.Contains(concreteClass);
    }

    public void UnitRoleChecks(IStrategy strategy, RoleType roleType)
    {
        if (!this.ContainsClass(roleType.AssociationType.Composite, strategy.Class))
        {
            throw new ArgumentException(strategy.Class + " is not a valid association object type for " + roleType + ".");
        }

        if (roleType.ObjectType is Composite)
        {
            throw new ArgumentException(roleType.ObjectType + " on roleType " + roleType + " is not a unit type.");
        }
    }

    public void CompositeRoleChecks(IStrategy strategy, RoleType roleType) => this.CompositeSharedChecks(strategy, roleType, null);

    public void CompositeRoleChecks(IStrategy strategy, RoleType roleType, Strategy roleStrategy)
    {
        this.CompositeSharedChecks(strategy, roleType, roleStrategy);
        if (!roleType.IsOne)
        {
            throw new ArgumentException("RelationType " + roleType + " has multiplicity many.");
        }
    }

    public Strategy CompositeRolesChecks(IStrategy strategy, RoleType roleType, Strategy roleStrategy)
    {
        this.CompositeSharedChecks(strategy, roleType, roleStrategy);
        if (!roleType.IsMany)
        {
            throw new ArgumentException("RelationType " + roleType + " has multiplicity one.");
        }

        return roleStrategy;
    }

    internal void OnObjectNotRestored(Guid metaTypeId, long allorsObjectId)
    {
        var args = new ObjectNotRestoredEventArgs(metaTypeId, allorsObjectId);
        if (this.ObjectNotRestored != null)
        {
            this.ObjectNotRestored(this, args);
        }
        else
        {
            throw new Exception("Object not restored: " + args);
        }
    }

    internal void OnRelationNotRestored(Guid relationTypeId, long associationObjectId, string roleContents)
    {
        var args = new RelationNotRestoredEventArgs(relationTypeId, associationObjectId, roleContents);
        if (this.RelationNotRestored != null)
        {
            this.RelationNotRestored(this, args);
        }
        else
        {
            throw new Exception("RelationType not restored: " + args);
        }
    }

    private void CompositeSharedChecks(IStrategy strategy, RoleType roleType, Strategy roleStrategy)
    {
        if (!this.ContainsClass(roleType.AssociationType.Composite, strategy.Class))
        {
            throw new ArgumentException(strategy.Class + " has no roleType with role " + roleType + ".");
        }

        if (roleStrategy != null)
        {
            if (!strategy.Transaction.Equals(roleStrategy.Transaction))
            {
                throw new ArgumentException(roleStrategy + " is from different transaction");
            }

            if (roleStrategy.IsDeleted)
            {
                throw new ArgumentException(roleType + " on object " + strategy + " is removed.");
            }

            if (!(roleType.ObjectType is Composite compositeType))
            {
                throw new ArgumentException(roleStrategy + " has no CompositeType");
            }

            if (!compositeType.IsAssignableFrom(roleStrategy.Class))
            {
                throw new ArgumentException(roleStrategy.Class + " is not compatible with type " + roleType.ObjectType + " of role " +
                                            roleType + ".");
            }
        }
    }
}
