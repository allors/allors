﻿// <copyright file="FromJson.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json;

using System.Collections.Generic;
using System.Linq;
using Allors.Protocol.Json;
using Allors.Database.Data;
using Allors.Database.Meta;

public class FromJson
{
    private readonly IList<IResolver> resolvers;
    private readonly ITransaction transaction;

    public FromJson(ITransaction transaction, IUnitConvert unitConvert)
    {
        this.transaction = transaction;
        this.MetaPopulation = this.transaction.Database.MetaPopulation;
        this.UnitConvert = unitConvert;

        this.resolvers = new List<IResolver>();
    }

    public MetaPopulation MetaPopulation { get; }

    public IUnitConvert UnitConvert { get; }

    public void Resolve(Has has, long objectId) => this.resolvers.Add(new HasResolver(has, objectId));

    public void Resolve(In @in, long[] objectIds) => this.resolvers.Add(new InResolver(@in, objectIds));

    public void Resolve(Intersects intersects, long[] objectIds) => this.resolvers.Add(new IntersectsResolver(intersects, objectIds));

    public void Resolve(Equals equals, long objectId) => this.resolvers.Add(new EqualsResolver(equals, objectId));

    public void Resolve(Pull pull, long objectId) => this.resolvers.Add(new PullResolver(pull, objectId));

    public void Resolve()
    {
        var objectIds = new HashSet<long>();
        foreach (var resolver in this.resolvers)
        {
            resolver.Prepare(objectIds);
        }

        var objectById = this.transaction.Instantiate(objectIds).ToDictionary(v => v.Id);

        foreach (var resolver in this.resolvers)
        {
            resolver.Resolve(objectById);
        }
    }
}
