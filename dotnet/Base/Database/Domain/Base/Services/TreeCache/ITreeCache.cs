// <copyright file="ITreeCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Allors.Database.Data;
    using Allors.Database.Meta;

    // TODO: Remove
    public interface ITreeCache
    {
        Node[] Get(Composite composite);

        void Set(Composite composite, Node[] tree);
    }
}
