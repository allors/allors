﻿// <copyright file="ObjectExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Allors.Database.Services;
    using Allors.Database.Meta;

    public static partial class ObjectExtensions
    {
        public static void BaseOnPostBuild(this Object @this, ObjectOnPostBuild method)
        {
            var metaCache = @this.Transaction().Database.Services.Get<IMetaCache>();
            var requiredCompositeRoleTypes = metaCache.GetRequiredCompositeRoleTypesByClass(@this.Strategy.Class);

            foreach (var compositeRoleType in requiredCompositeRoleTypes)
            {
                var roleType = compositeRoleType.RoleType;

                if (roleType.ObjectType is Unit unit && !@this.Strategy.ExistRole(roleType))
                {
                    switch (unit.Tag)
                    {
                        case UnitTags.Boolean:
                            @this.Strategy.SetUnitRole(roleType, false);
                            break;

                        case UnitTags.Decimal:
                            @this.Strategy.SetUnitRole(roleType, 0m);
                            break;

                        case UnitTags.Float:
                            @this.Strategy.SetUnitRole(roleType, 0d);
                            break;

                        case UnitTags.Integer:
                            @this.Strategy.SetUnitRole(roleType, 0);
                            break;

                        case UnitTags.Unique:
                            @this.Strategy.SetUnitRole(roleType, Guid.NewGuid());
                            break;

                        case UnitTags.DateTime:
                            @this.Strategy.SetUnitRole(roleType, @this.Transaction().Now());
                            break;
                    }
                }
            }
        }
    }
}
