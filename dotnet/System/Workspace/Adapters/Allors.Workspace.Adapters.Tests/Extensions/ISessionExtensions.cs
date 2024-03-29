﻿// <copyright file="Test.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Meta;

    public static class ISessionExtensions
    {
        public static async Task<T> PullObject<T>(this IWorkspace @this, string name) where T : class, IObject
        {
            var objectType = (IComposite)@this.Services.Get<IObjectFactory>().GetObjectTypeForObject<T>();
            var roleType = objectType.RoleTypes.First(v => v.Name.Equals("Name"));
            var pull = new Pull { Extent = new Filter(objectType) { Predicate = new Equals(roleType) { Value = name } } };
            var result = await @this.PullAsync(pull);
            var collection = result.GetCollection<T>();
            return collection[0];
        }
    }
}
