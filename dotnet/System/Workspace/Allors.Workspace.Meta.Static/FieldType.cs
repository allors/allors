﻿// <copyright file="RecordType.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the RoleType type.</summary>

namespace Allors.Workspace.Meta
{
    public abstract class FieldType : MetaIdentifiableObject, IFieldType
    {
        protected FieldType(MetaPopulation metaPopulation, string tag)
            : base(metaPopulation, tag)
        {
        }
    }
}
