// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Allors.Database.Tracing;
using Allors.Database.Adapters.Sql.Caching;
using Allors.Database.Meta;

public abstract class Database : IDatabase
{
    public static readonly IsolationLevel DefaultIsolationLevel = System.Data.IsolationLevel.Snapshot;

    private readonly Dictionary<ObjectType, HashSet<ObjectType>> concreteClassesByObjectType;

    private readonly Dictionary<ObjectType, RoleType[]> sortedUnitRolesByObjectType;

    private ICacheFactory cacheFactory;

    protected Database(IDatabaseServices state, Configuration configuration)
    {
        this.Services = state;
        if (this.Services == null)
        {
            throw new Exception("Services is missing");
        }

        this.ObjectFactory = configuration.ObjectFactory;
        if (!this.ObjectFactory.MetaPopulation.IsValid)
        {
            throw new ArgumentException("Domain is invalid");
        }

        this.MetaPopulation = this.ObjectFactory.MetaPopulation;

        this.ConnectionString = configuration.ConnectionString;

        this.concreteClassesByObjectType = new Dictionary<ObjectType, HashSet<ObjectType>>();

        this.CommandTimeout = configuration.CommandTimeout;
        this.IsolationLevel = configuration.IsolationLevel;

        this.sortedUnitRolesByObjectType = new Dictionary<ObjectType, RoleType[]>();

        this.CacheFactory = configuration.CacheFactory;
        this.Cache = this.CacheFactory.CreateCache();

        this.SchemaName = (configuration.SchemaName ?? "allors").ToLowerInvariant();
    }

    public abstract IConnectionFactory ConnectionFactory
    {
        get;
        set;
    }

    public abstract IConnectionFactory ManagementConnectionFactory
    {
        get;
        set;
    }

    public ICacheFactory CacheFactory
    {
        get => this.cacheFactory;

        set => this.cacheFactory = value ?? (this.cacheFactory = new DefaultCacheFactory());
    }

    public string SchemaName { get; }

    public abstract bool IsValid
    {
        get;
    }

    public string ConnectionString { get; set; }

    protected internal ICache Cache { get; }

    public int? CommandTimeout { get; }

    public IsolationLevel? IsolationLevel { get; }

    public abstract Mapping Mapping
    {
        get;
    }

    public abstract string ValidationMessage { get; }

    public abstract event ObjectNotRestoredEventHandler ObjectNotRestored;

    public abstract event RelationNotRestoredEventHandler RelationNotRestored;

    public IDatabaseServices Services { get; }

    public abstract string Id { get; }

    public IObjectFactory ObjectFactory { get; }

    public MetaPopulation MetaPopulation { get; }

    public bool IsShared => true;

    public abstract void Init();

    public abstract void Restore(XmlReader reader);

    public abstract void Backup(XmlWriter writer);

    public ISink Sink { get; set; }

    ITransaction IDatabase.CreateTransaction() => this.CreateTransaction();

    public ITransaction CreateTransaction()
    {
        var connection = this.ConnectionFactory.Create();
        return this.CreateTransaction(connection);
    }

    public ITransaction CreateTransaction(IConnection connection)
    {
        if (!this.IsValid)
        {
            throw new Exception(this.ValidationMessage);
        }

        return new Transaction(this, connection, this.Services.CreateTransactionServices());
    }

    public override string ToString() => "Population[driver=Sql, type=Connected, id=" + this.GetHashCode() + "]";

    public bool ContainsClass(ObjectType container, ObjectType containee)
    {
        if (container.IsClass)
        {
            return container.Equals(containee);
        }

        if (!this.concreteClassesByObjectType.TryGetValue(container, out var concreteClasses))
        {
            concreteClasses = new HashSet<ObjectType>(((Interface)container).Classes);
            this.concreteClassesByObjectType[container] = concreteClasses;
        }

        return concreteClasses.Contains(containee);
    }


    internal Type GetDomainType(ObjectType objectType) => this.ObjectFactory.GetType(objectType);

    public RoleType[] GetSortedUnitRolesByObjectType(ObjectType objectType)
    {
        if (!this.sortedUnitRolesByObjectType.TryGetValue(objectType, out var sortedUnitRoles))
        {
            var sortedUnitRoleList = new List<RoleType>(((Composite)objectType).RoleTypes.Where(r => r.ObjectType.IsUnit));
            sortedUnitRoleList.Sort();
            sortedUnitRoles = [.. sortedUnitRoleList];
            this.sortedUnitRolesByObjectType[objectType] = sortedUnitRoles;
        }

        return sortedUnitRoles;
    }
}
