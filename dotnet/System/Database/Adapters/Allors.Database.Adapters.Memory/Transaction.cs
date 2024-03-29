﻿// <copyright file="Transaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Allors.Database.Meta;

public class Transaction : ITransaction
{
    private static readonly HashSet<Strategy> EmptyStrategies = new();

    private readonly Dictionary<ObjectType, ObjectType[]> concreteClassesByObjectType;
    private bool busyCommittingOrRollingBack;

    private long currentId;
    private Dictionary<ObjectType, HashSet<Strategy>> strategiesByObjectType;

    private Dictionary<long, Strategy> strategyByObjectId;

    private readonly Dictionary<Type, object> scopedByType;

    internal Transaction(Database database, ITransactionServices services)
    {
        this.Database = database;
        this.Services = services;

        this.busyCommittingOrRollingBack = false;

        this.concreteClassesByObjectType = new Dictionary<ObjectType, ObjectType[]>();
        this.scopedByType = new Dictionary<Type, object>();

        this.ChangeLog = new ChangeLog();

        this.Reset();

        this.Services.OnInit(this);
    }

    public ITransactionServices Services { get; }

    IDatabase ITransaction.Database => this.Database;

    internal Database Database { get; }

    internal ChangeLog ChangeLog { get; private set; }

    public void Commit()
    {
        if (!this.busyCommittingOrRollingBack)
        {
            try
            {
                this.busyCommittingOrRollingBack = true;

                IList<Strategy> strategiesToDelete = null;
                foreach (var dictionaryEntry in this.strategyByObjectId)
                {
                    var strategy = dictionaryEntry.Value;

                    strategy.Commit();

                    if (strategy.IsDeleted)
                    {
                        strategiesToDelete ??= new List<Strategy>();
                        strategiesToDelete.Add(strategy);
                    }
                }

                if (strategiesToDelete != null)
                {
                    foreach (var strategy in strategiesToDelete)
                    {
                        this.strategyByObjectId.Remove(strategy.ObjectId);

                        if (this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
                        {
                            strategies.Remove(strategy);
                        }
                    }
                }

                this.ChangeLog = new ChangeLog();
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

                foreach (var strategy in new List<Strategy>(this.strategyByObjectId.Values))
                {
                    strategy.Rollback();
                    if (strategy.IsDeleted)
                    {
                        this.strategyByObjectId.Remove(strategy.ObjectId);

                        if (this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
                        {
                            strategies.Remove(strategy);
                        }
                    }
                }

                this.ChangeLog = new ChangeLog();
            }
            finally
            {
                this.busyCommittingOrRollingBack = false;
            }
        }
    }

    public void Dispose() => this.Rollback();

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
        var allorsObjects = new T[count];
        for (var i = 0; i < count; i++)
        {
            allorsObjects[i] = this.Build<T>();
        }

        return allorsObjects;
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

    public virtual IObject Build(Class @class)
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
        var arrayType = this.Database.ObjectFactory.GetType(@class);
        var allorsObjects = (IObject[])Array.CreateInstance(arrayType, count);
        for (var i = 0; i < count; i++)
        {
            var newObject = this.CreateWithoutOnBuild(@class);
            newObject.OnBuild();
            newObject.OnPostBuild();
            allorsObjects[i] = newObject;
        }

        return allorsObjects;
    }

    public TObject[] Build<TObject, TArgument>(IEnumerable<TArgument> args, Action<TObject, TArgument> builder) where TObject : IObject
    {
        var objectType = this.Database.ObjectFactory.GetObjectType(typeof(TObject));

        if (objectType is not Class @class)
        {
            throw new ArgumentException("IObjectType should be a class");
        }

        var materializedArgs = args as IReadOnlyCollection<TArgument> ?? args.ToArray();

        var newObjects = new TObject[materializedArgs.Count];

        var index = 0;
        foreach (var arg in materializedArgs)
        {
            var newObject = (TObject)this.CreateWithoutOnBuild(@class);
            builder?.Invoke(newObject, arg);
            newObject.OnBuild();
            newObject.OnPostBuild();
            newObjects[index++] = newObject;
        }

        return newObjects;
    }

    public IObject Instantiate(string objectIdString) => long.TryParse(objectIdString, out var id) ? this.Instantiate(id) : null;

    public IObject Instantiate(IObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        return this.Instantiate(obj.Strategy.ObjectId);
    }

    public IObject Instantiate(long objectId)
    {
        var strategy = this.InstantiateMemoryStrategy(objectId);
        return strategy?.GetObject();
    }

    public IStrategy InstantiateStrategy(long objectId) => this.InstantiateMemoryStrategy(objectId);

    public IObject[] Instantiate(IEnumerable<string> objectIdStrings) =>
        objectIdStrings != null ? this.Instantiate(objectIdStrings.Select(long.Parse)) : Array.Empty<IObject>();

    public IObject[] Instantiate(IEnumerable<IObject> objects) =>
        objects != null ? this.Instantiate(objects.Select(v => v.Id)) : Array.Empty<IObject>();

    public IObject[] Instantiate(IEnumerable<long> objectIds) => objectIds != null
        ? objectIds.Select(v => this.InstantiateMemoryStrategy(v)?.GetObject()).Where(v => v != null).ToArray()
        : Array.Empty<IObject>();

    public void Prefetch<T>(PrefetchPolicy prefetchPolicy, params T[] objects) where T : IObject
    {
        // nop
    }

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<string> objectIds)
    {
        // nop
    }

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<long> objectIds)
    {
        // nop
    }

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IStrategy> strategies)
    {
        // nop
    }

    public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IObject> objects)
    {
        // nop
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
            return this.ChangeLog.Checkpoint();
        }
        finally
        {
            this.ChangeLog = new ChangeLog();
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

    public virtual IFilter<IObject> Filter(Composite objectType, Action<ICompositePredicate> filter = null)
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

    private IObject CreateWithoutOnBuild(Class objectType)
    {
        var strategy = new Strategy(this, objectType, ++this.currentId, Allors.Version.DatabaseInitial);
        this.AddStrategy(strategy);

        this.ChangeLog.OnCreated(strategy);

        return strategy.GetObject();
    }

    internal void Init() => this.Reset();

    internal Type GetTypeForObjectType(ObjectType objectType) => this.Database.ObjectFactory.GetType(objectType);

    internal virtual Strategy InsertStrategy(Class objectType, long objectId, long objectVersion)
    {
        var strategy = this.GetStrategy(objectId);
        if (strategy != null)
        {
            throw new Exception("Duplicate id error");
        }

        if (this.currentId < objectId)
        {
            this.currentId = objectId;
        }

        strategy = new Strategy(this, objectType, objectId, objectVersion);
        this.AddStrategy(strategy);

        this.ChangeLog.OnCreated(strategy);

        return strategy;
    }

    internal virtual Strategy InstantiateMemoryStrategy(long objectId) => this.GetStrategy(objectId);

    internal Strategy GetStrategy(IObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        return this.GetStrategy(obj.Id);
    }

    internal Strategy GetStrategy(long objectId)
    {
        if (!this.strategyByObjectId.TryGetValue(objectId, out var strategy))
        {
            return null;
        }

        return strategy.IsDeleted ? null : strategy;
    }

    internal void AddStrategy(Strategy strategy)
    {
        this.strategyByObjectId.Add(strategy.ObjectId, strategy);

        if (!this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
        {
            strategies = new HashSet<Strategy>();
            this.strategiesByObjectType.Add(strategy.UncheckedObjectType, strategies);
        }

        strategies.Add(strategy);
    }

    internal virtual HashSet<Strategy> GetStrategiesForExtentIncludingDeleted(ObjectType type)
    {
        if (!this.concreteClassesByObjectType.TryGetValue(type, out var concreteClasses))
        {
            var sortedClassAndSubclassList = new List<ObjectType>();

            if (type is Class)
            {
                sortedClassAndSubclassList.Add(type);
            }

            if (type is Interface)
            {
                foreach (var subClass in ((Interface)type).Classes)
                {
                    sortedClassAndSubclassList.Add(subClass);
                }
            }

            concreteClasses = [.. sortedClassAndSubclassList];

            this.concreteClassesByObjectType[type] = concreteClasses;
        }

        switch (concreteClasses.Length)
        {
        case 0:
            return EmptyStrategies;

        case 1:
            {
                var objectType = concreteClasses[0];
                if (this.strategiesByObjectType.TryGetValue(objectType, out var strategies))
                {
                    return strategies;
                }

                return EmptyStrategies;
            }

        default:
            {
                var strategies = new HashSet<Strategy>();

                foreach (var objectType in concreteClasses)
                {
                    if (this.strategiesByObjectType.TryGetValue(objectType, out var objectTypeStrategies))
                    {
                        strategies.UnionWith(objectTypeStrategies);
                    }
                }

                return strategies;
            }
        }
    }

    internal void Backup(XmlWriter writer)
    {
        var sortedNonDeletedStrategiesByObjectType = new Dictionary<ObjectType, List<Strategy>>();
        foreach (var dictionaryEntry in this.strategyByObjectId)
        {
            var strategy = dictionaryEntry.Value;
            if (!strategy.IsDeleted)
            {
                var objectType = strategy.UncheckedObjectType;

                if (!sortedNonDeletedStrategiesByObjectType.TryGetValue(objectType, out var sortedNonDeletedStrategies))
                {
                    sortedNonDeletedStrategies = new List<Strategy>();
                    sortedNonDeletedStrategiesByObjectType[objectType] = sortedNonDeletedStrategies;
                }

                sortedNonDeletedStrategies.Add(strategy);
            }
        }

        foreach (var dictionaryEntry in sortedNonDeletedStrategiesByObjectType)
        {
            var sortedNonDeletedStrategies = dictionaryEntry.Value;
            sortedNonDeletedStrategies.Sort(new Strategy.ObjectIdComparer());
        }

        var backup = new Backup(this, writer, sortedNonDeletedStrategiesByObjectType);
        backup.Execute();
    }

    private void Reset()
    {
        // Strategies
        this.strategyByObjectId = new Dictionary<long, Strategy>();
        this.strategiesByObjectType = new Dictionary<ObjectType, HashSet<Strategy>>();
    }

    private IExtent<IObject> CreateExtentOperation(IExtent<IObject> firstOperand, IExtent<IObject> secondOperand, ExtentOperations extentOperations)
    {
        try
        {
            Type type = typeof(ExtentOperation<>);
            Type[] typeArgs = [firstOperand.ObjectType.BoundType];
            Type constructed = type.MakeGenericType(typeArgs);
            var instance = Activator.CreateInstance(constructed, this, firstOperand, secondOperand, extentOperations);
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
