﻿// <copyright file="Transaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Transaction type.</summary>

namespace Allors.Database.Adapters.Sql;

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;

public sealed class Transaction : ITransaction
{
    private bool busyCommittingOrRollingBack;

    private Dictionary<string, object> properties;

    private readonly Dictionary<Type, IScoped> scopedByType;

    internal Transaction(Database database, IConnection connection, ITransactionServices scope)
    {
        this.Database = database;
        this.Connection = connection;
        this.Services = scope;

        this.State = new State(this);
        this.scopedByType = new Dictionary<Type, IScoped>();

        if (this.Database.Sink != null)
        {
            this.Prefetcher = new TraceablePrefetcher(this);
            this.Commands = new TraceableCommands(this, connection);
        }
        else
        {
            this.Prefetcher = new UntraceablePrefetcher(this);
            this.Commands = new UntraceableCommands(this, connection);
        }

        this.Services.OnInit(this);
    }

    public IConnection Connection { get; }

    public Commands Commands { get; }

    public State State { get; }

    public Database Database { get; }

    private Prefetcher Prefetcher { get; }

    public object this[string name]
    {
        get
        {
            if (this.properties == null)
            {
                return null;
            }

            this.properties.TryGetValue(name, out var value);
            return value;
        }

        set
        {
            if (this.properties == null)
            {
                this.properties = new Dictionary<string, object>();
            }

            if (value == null)
            {
                this.properties.Remove(name);
            }
            else
            {
                this.properties[name] = value;
            }
        }
    }

    IDatabase ITransaction.Database => this.Database;

    public ITransactionServices Services { get; }

    public T Build<T>() where T : IObject
    {
        var objectType = this.Database.ObjectFactory.GetObjectType(typeof(T));

        if (objectType is not Class @class)
        {
            throw new ArgumentException("IObjectType should be a class");
        }

        var newObject = (T)this.CreateWithoutOnBuild(@class);
        newObject.OnBuild();
        newObject.OnPostBuild();

        return newObject;
    }

    public T[] Build<T>(int count) where T : IObject
    {
        var @class = (Class)this.Database.ObjectFactory.GetObjectType(typeof(T));
        var references = this.Commands.CreateObjects(@class, count);

        var domainObjects = new T[count];
        for (var i = 0; i < references.Count; i++)
        {
            var reference = references[i];
            this.State.ReferenceByObjectId[reference.ObjectId] = reference;
            this.State.ChangeLog.OnCreated(reference.Strategy);

            T newObject = (T)reference.Strategy.GetObject();
            newObject.OnBuild();
            newObject.OnPostBuild();
            domainObjects[i] = newObject;
        }

        return domainObjects;
    }

    public T Build<T>(params Action<T>[] builders) where T : IObject
    {
        var objectType = this.Database.ObjectFactory.GetObjectType(typeof(T));

        if (objectType is not Class @class)
        {
            throw new ArgumentException("IObjectType should be a class");
        }

        var newObject = (T)this.CreateWithoutOnBuild(@class);

        if (builders != null)
        {
            foreach (var builder in builders)
            {
                builder?.Invoke(newObject);
            }
        }

        newObject.OnBuild();
        newObject.OnPostBuild();

        return newObject;
    }

    public T Build<T>(IEnumerable<Action<T>> builders, params Action<T>[] extraBuilders) where T : IObject
    {
        var objectType = this.Database.ObjectFactory.GetObjectType(typeof(T));

        if (objectType is not Class @class)
        {
            throw new ArgumentException("IObjectType should be a class");
        }

        var newObject = (T)this.CreateWithoutOnBuild(@class);

        if (builders != null)
        {
            foreach (var builder in builders)
            {
                builder?.Invoke(newObject);
            }
        }

        foreach (var extraBuilder in extraBuilders)
        {
            extraBuilder?.Invoke(newObject);
        }

        newObject.OnBuild();
        newObject.OnPostBuild();

        return newObject;
    }

    public IObject Build(Class @class)
    {
        var newObject = this.CreateWithoutOnBuild(@class);
        newObject.OnBuild();
        newObject.OnPostBuild();
        return newObject;
    }

    public IObject Build(Class @class, params Action<IObject>[] builders)
    {
        var newObject = this.CreateWithoutOnBuild(@class);

        if (builders != null)
        {
            foreach (var builder in builders)
            {
                builder?.Invoke(newObject);
            }
        }

        newObject.OnBuild();
        newObject.OnPostBuild();

        return newObject;
    }

    public IObject Build(Class @class, IEnumerable<Action<IObject>> builders, params Action<IObject>[] extraBuilders)
    {
        var newObject = this.CreateWithoutOnBuild(@class);

        if (builders != null)
        {
            foreach (var builder in builders)
            {
                builder?.Invoke(newObject);
            }
        }

        foreach (var extraBuilder in extraBuilders)
        {
            extraBuilder?.Invoke(newObject);
        }

        newObject.OnBuild();
        newObject.OnPostBuild();

        return newObject;
    }

    public IObject[] Build(Class @class, int count)
    {
        if (!@class.IsClass)
        {
            throw new ArgumentException("Can not create non concrete composite type " + @class);
        }

        var references = this.Commands.CreateObjects(@class, count);

        var arrayType = this.Database.ObjectFactory.GetType(@class);
        var domainObjects = (IObject[])Array.CreateInstance(arrayType, count);

        for (var i = 0; i < references.Count; i++)
        {
            var reference = references[i];
            this.State.ReferenceByObjectId[reference.ObjectId] = reference;
            this.State.ChangeLog.OnCreated(reference.Strategy);

            var newObject = reference.Strategy.GetObject();
            newObject.OnBuild();
            newObject.OnPostBuild();
            domainObjects[i] = newObject;
        }

        return domainObjects;
    }

    public TObject[] Build<TObject, TArgument>(IEnumerable<TArgument> args, Action<TObject, TArgument> builder) where TObject : IObject
    {
        var objectType = this.Database.ObjectFactory.GetObjectType(typeof(TObject));

        if (objectType is not Class @class)
        {
            throw new ArgumentException("IObjectType should be a class");
        }

        var materializedArgs = args as IReadOnlyCollection<TArgument> ?? args.ToArray();

        var count = materializedArgs.Count;

        var references = this.Commands.CreateObjects(@class, count);

        var domainObjects = new TObject[count];

        var i = 0;
        foreach (var arg in materializedArgs)
        {
            var reference = references[i];
            this.State.ReferenceByObjectId[reference.ObjectId] = reference;
            this.State.ChangeLog.OnCreated(reference.Strategy);

            var newObject = (TObject)reference.Strategy.GetObject();
            builder?.Invoke(newObject, arg);
            newObject.OnBuild();
            newObject.OnPostBuild();
            domainObjects[i] = newObject;
            i++;
        }

        return domainObjects;
    }

    public IObject Instantiate(IObject obj) => this.Instantiate(obj.Strategy.ObjectId);

    public IObject Instantiate(string objectId) => long.TryParse(objectId, out var id) ? this.Instantiate(id) : null;

    public IObject Instantiate(long objectId)
    {
        var strategy = this.InstantiateStrategy(objectId);
        return strategy?.GetObject();
    }

    public IStrategy InstantiateStrategy(long objectId)
    {
        if (!this.State.ReferenceByObjectId.TryGetValue(objectId, out var reference))
        {
            reference = this.Commands.InstantiateObject(objectId);
            if (reference != null)
            {
                this.State.ReferenceByObjectId[objectId] = reference;
            }
        }

        if (reference == null || !reference.Exists)
        {
            return null;
        }

        return reference.Strategy;
    }

    public IObject[] Instantiate(IEnumerable<string> objectIdStrings) =>
        objectIdStrings != null ? this.Instantiate(objectIdStrings.Select(long.Parse)) : Array.Empty<IObject>();

    public IObject[] Instantiate(IEnumerable<IObject> objects) =>
        objects != null ? this.Instantiate(objects.Select(v => v.Id)) : Array.Empty<IObject>();

    public IObject[] Instantiate(IEnumerable<long> objectIds)
    {
        var emptyObjects = Array.Empty<IObject>();
        if (objectIds == null)
        {
            return emptyObjects;
        }

        var objectIdArray = objectIds.ToArray();

        if (objectIdArray.Length == 0)
        {
            return emptyObjects;
        }

        var references = new List<Reference>(objectIdArray.Length);

        List<long> nonCachedObjectIds = null;
        foreach (var objectId in objectIdArray)
        {
            if (!this.State.ReferenceByObjectId.TryGetValue(objectId, out var reference))
            {
                if (nonCachedObjectIds == null)
                {
                    nonCachedObjectIds = new List<long>();
                }

                nonCachedObjectIds.Add(objectId);
            }
            else if (!reference.Strategy.IsDeleted)
            {
                references.Add(reference);
            }
        }

        if (nonCachedObjectIds != null)
        {
            var nonCachedReferences = this.Commands.InstantiateReferences(nonCachedObjectIds);
            references.AddRange(nonCachedReferences);

            // Return objects in the same order as objectIds
            var referenceById = references.ToDictionary(v => v.ObjectId);
            references = new List<Reference>();
            foreach (var objectId in objectIdArray)
            {
                if (referenceById.TryGetValue(objectId, out var reference))
                {
                    references.Add(reference);
                }
            }
        }

        var allorsObjects = new IObject[references.Count];
        for (var i = 0; i < allorsObjects.Length; i++)
        {
            allorsObjects[i] = references[i].Strategy.GetObject();
        }

        return allorsObjects;
    }

    public void Prefetch<T>(PrefetchPolicy prefetchPolicy, params T[] objects) where T : IObject =>
        this.Prefetch(prefetchPolicy, objects.Select(x => x.Strategy.ObjectId));

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IObject> objects) =>
        this.Prefetch(prefetchPolicy, objects.Select(x => x.Strategy.ObjectId));

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IStrategy> strategies) =>
        this.Prefetch(prefetchPolicy, strategies.Select(v => v.ObjectId));

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<string> objectIdStrings) =>
        this.Prefetch(prefetchPolicy, objectIdStrings.Select(long.Parse));

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<long> objectIds)
    {
        var references = this.Prefetcher.GetReferencesForPrefetching(objectIds);

        if (references.Count != 0)
        {
            this.State.Flush();

            var prefetcher = new Prefetch(this.Prefetcher, prefetchPolicy, references);
            prefetcher.Execute();
        }
    }

    public T Scoped<T>() where T : class, IScoped
    {
        if (this.scopedByType.TryGetValue(typeof(T), out var scoped))
        {
            return (T)scoped;
        }

        scoped = (IScoped)Activator.CreateInstance(typeof(T), new[] { this });
        this.scopedByType.Add(typeof(T), scoped);
        return (T)scoped;
    }

    public IChangeSet Checkpoint()
    {
        try
        {
            return this.State.ChangeLog.Checkpoint();
        }
        finally
        {
            this.State.ChangeLog.Reset();
        }
    }

    public IFilter<T> Filter<T>(Action<ICompositePredicate> filter = null) where T : class, IObject
    {
        if (!(this.Database.ObjectFactory.GetObjectType(typeof(T)) is Composite compositeType))
        {
            throw new Exception("type should be a CompositeType");
        }

        var extent = (IFilter<T>)this.Filter(compositeType, filter);
        return extent;
    }

    public IFilter<IObject> Filter(Composite objectType, Action<ICompositePredicate> filter = null)
    {
        Type type = typeof(ExtentFilter<>);
        Type[] typeArgs = [objectType.BoundType];
        Type constructed = type.MakeGenericType(typeArgs);
        var instance = Activator.CreateInstance(constructed, this, objectType);

        var extent = (IFilter<IObject>)instance;
        filter?.Invoke(extent);
        return extent;
    }

    public IExtent<IObject> Union(IExtent<IObject> firstOperand, IExtent<IObject> secondOperand) =>
        this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Union);

    public IExtent<IObject> Intersect(IExtent<IObject> firstOperand, IExtent<IObject> secondOperand) =>
        this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Intersect);

    public IExtent<IObject> Except(IExtent<IObject> firstOperand, IExtent<IObject> secondOperand) =>
        this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Except);

    public void Commit()
    {
        if (!this.busyCommittingOrRollingBack)
        {
            try
            {
                this.busyCommittingOrRollingBack = true;

                var accessed = new List<long>(this.State.ReferenceByObjectId.Keys);
                this.State.Flush();

                var changed = this.State.ModifiedRolesByReference?.Select(dictionaryEntry => dictionaryEntry.Key.ObjectId).ToArray() ??
                              Array.Empty<long>();
                if (changed.Length > 0)
                {
                    this.Commands.UpdateVersion(changed);
                }

                this.Connection.Commit();

                this.State.ModifiedRolesByReference = null;

                var referencesWithStrategy = new HashSet<Reference>();
                foreach (var reference in this.State.ReferenceByObjectId.Values)
                {
                    reference.Commit(referencesWithStrategy);
                }

                this.State.ExistingObjectIdsWithoutReference = new HashSet<long>();
                this.State.ReferencesWithoutVersions = referencesWithStrategy;

                this.State.ReferenceByObjectId = new Dictionary<long, Reference>();
                foreach (var reference in referencesWithStrategy)
                {
                    this.State.ReferenceByObjectId[reference.ObjectId] = reference;
                }

                this.State.AssociationByRoleByAssociationType = new Dictionary<AssociationType, Dictionary<Reference, Reference>>();
                this.State.AssociationsByRoleByAssociationType = new Dictionary<AssociationType, Dictionary<Reference, long[]>>();

                this.State.ChangeLog.Reset();

                this.Database.Cache.OnCommit(accessed, changed);

                this.Prefetcher.ResetCommands();
                this.Commands.ResetCommands();
            }
            finally
            {
                this.busyCommittingOrRollingBack = false;
            }
        }
    }

    public void Rollback()
    {
        if (!this.busyCommittingOrRollingBack)
        {
            try
            {
                this.busyCommittingOrRollingBack = true;

                var accessed = new List<long>(this.State.ReferenceByObjectId.Keys);

                this.Connection.Rollback();

                var referencesWithStrategy = new HashSet<Reference>();
                foreach (var reference in this.State.ReferenceByObjectId.Values)
                {
                    reference.Rollback(referencesWithStrategy);
                }

                this.State.ExistingObjectIdsWithoutReference = new HashSet<long>();
                this.State.ReferencesWithoutVersions = referencesWithStrategy;

                this.State.ReferenceByObjectId = new Dictionary<long, Reference>();
                foreach (var reference in referencesWithStrategy)
                {
                    this.State.ReferenceByObjectId[reference.ObjectId] = reference;
                }

                this.State.UnflushedRolesByReference = null;
                this.State.ModifiedRolesByReference = null;
                this.State.TriggersFlushRolesByAssociationType = null;

                this.State.AssociationByRoleByAssociationType = new Dictionary<AssociationType, Dictionary<Reference, Reference>>();
                this.State.AssociationsByRoleByAssociationType = new Dictionary<AssociationType, Dictionary<Reference, long[]>>();

                this.State.ChangeLog.Reset();

                this.Database.Cache.OnRollback(accessed);

                this.Prefetcher.ResetCommands();
                this.Commands.ResetCommands();
            }
            finally
            {
                this.busyCommittingOrRollingBack = false;
            }
        }
    }

    public void Dispose() => this.Rollback();

    private IObject CreateWithoutOnBuild(Class objectType)
    {
        var reference = this.Commands.CreateObject(objectType);
        this.State.ReferenceByObjectId[reference.ObjectId] = reference;

        this.Database.Cache.SetObjectType(reference.ObjectId, objectType);

        this.State.ChangeLog.OnCreated(reference.Strategy);

        return reference.Strategy.GetObject();
    }

    public override string ToString() => "Transaction[id=" + this.GetHashCode() + "] " + this.Database;

    internal Reference[] GetOrCreateReferencesForExistingObjects(IEnumerable<long> objectIds) =>
        this.State.GetOrCreateReferencesForExistingObjects(objectIds, this);

    internal long[] GetAssociations(Strategy roleStrategy, AssociationType associationType)
    {
        var associationsByRole = this.State.GetAssociationsByRole(associationType);

        if (!associationsByRole.TryGetValue(roleStrategy.Reference, out var associations))
        {
            this.State.FlushConditionally(roleStrategy.ObjectId, associationType);
            associations = this.Commands.GetCompositesAssociation(roleStrategy, associationType);
            associationsByRole[roleStrategy.Reference] = associations;
        }

        return associations;
    }

    internal void RemoveAssociation(Reference association, Reference role, AssociationType associationType)
    {
        var associationsByRole = this.State.GetAssociationsByRole(associationType);

        if (associationsByRole.TryGetValue(role, out var associations))
        {
            associationsByRole[role] = associations.Remove(association.ObjectId);
        }
    }

    internal void AddReferenceWithoutVersionOrExistsKnown(Reference reference) => this.State.ReferencesWithoutVersions.Add(reference);

    internal void GetVersionAndExists()
    {
        if (this.State.ReferencesWithoutVersions.Count > 0)
        {
            var versionByObjectId = this.Commands.GetVersions(this.State.ReferencesWithoutVersions);
            foreach (var association in this.State.ReferencesWithoutVersions)
            {
                if (versionByObjectId.TryGetValue(association.ObjectId, out var version))
                {
                    association.Version = version;
                    association.Exists = true;
                }
                else
                {
                    association.Exists = false;
                }
            }

            this.State.ReferencesWithoutVersions = new HashSet<Reference>();
        }
    }

    internal void InstantiateReferences(IEnumerable<long> objectIds)
    {
        var forceEvaluation = this.Commands.InstantiateReferences(objectIds).ToArray();
    }

    private IExtent<IObject> CreateExtentOperation(IExtent<IObject> firstOperand, IExtent<IObject> secondOperand, ExtentOperations extentOperations)
    {
        try
        {
            Type type = typeof(ExtentOperation<>);
            Type[] typeArgs = [firstOperand.ObjectType.BoundType];
            Type constructed = type.MakeGenericType(typeArgs);
            var instance = Activator.CreateInstance(constructed, firstOperand, secondOperand, extentOperations);
            var extent = (IExtent<IObject>)instance;
            return extent;
        }
        catch (Exception e)
        {
            if (e.InnerException != null)
            {
                throw e.InnerException;
            }

            throw;
        }
    }

}
