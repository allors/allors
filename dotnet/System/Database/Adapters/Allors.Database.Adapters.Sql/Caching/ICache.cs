// <copyright file="ICache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Caching;

using System.Collections.Generic;
using Allors.Database.Meta;

/// <summary>
///     The Cache holds a CachedObject and/or IObjectType by ObjectId.
/// </summary>
public interface ICache
{
    ICachedObject GetOrCreateCachedObject(Class concreteClass, long objectId, long version);

    Class GetObjectType(long objectId);

    void SetObjectType(long objectId, Class objectType);

    void OnCommit(IList<long> accessedObjectIds, IList<long> changedObjectIds);

    void OnRollback(List<long> accessedObjectIds);

    /// <summary>
    ///     Invalidates the Cache.
    /// </summary>
    void Invalidate();
}
