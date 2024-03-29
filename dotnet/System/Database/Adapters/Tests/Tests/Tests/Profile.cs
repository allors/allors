﻿// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters;

using System;
using System.Collections.Generic;
using Allors.Database.Meta.Configuration;
using Allors.Database.Domain;
using Allors.Database.Adapters.Memory;
using Database.Configuration;

public abstract class Profile : IProfile
{
    public ITransaction Transaction { get; private set; }

    public IDatabase Database { get; private set; }

    public abstract Action[] Markers { get; }

    public virtual Action[] Inits
    {
        get
        {
            var inits = new List<Action> { this.Init };

            if (Settings.ExtraInits)
            {
                inits.Add(this.Init);
            }

            return [.. inits];
        }
    }

    public virtual void Dispose()
    {
        this.Transaction?.Rollback();

        this.Transaction = null;
        this.Database = null;
    }

    public abstract IDatabase CreateDatabase();

    public void SwitchDatabase()
    {
        this.Transaction.Rollback();
        this.Database = this.CreateDatabase();
        this.Transaction = this.Database.CreateTransaction();
        this.Transaction.Commit();
    }

    public IDatabase CreateMemoryDatabase()
    {
        var metaPopulation = new MetaBuilder().Build();
        var scope = new DatabaseServices();
        return new Database(scope, new Memory.Configuration { ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)) });
    }

    internal ITransaction CreateTransaction() => this.Database.CreateTransaction();

    protected internal void Init()
    {
        try
        {
            this.Transaction?.Rollback();

            this.Database = this.CreateDatabase();
            this.Database.Init();
            this.Transaction = this.Database.CreateTransaction();
            this.Transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw;
        }
    }
}
