// <copyright file="IPredicateContainer.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Request
{
    using Visitor;

    public interface IPredicateContainer : IVisitable
    {
        void AddPredicate(IPredicate predicate);
    }
}