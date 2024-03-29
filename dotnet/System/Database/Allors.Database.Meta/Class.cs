﻿// <copyright file="Class.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IObjectType type.</summary>

namespace Allors.Database.Meta;

using System;
using System.Collections.Generic;
using Allors.Embedded.Meta;

public sealed class Class : Composite
{
    public Class(MetaPopulation metaPopulation, EmbeddedObjectType embeddedObjectType)
        : base(metaPopulation, embeddedObjectType)
    {
        this.Composites = new[] { this };
        this.Classes = new[] { this };
        this.DirectSubtypes = Array.Empty<Composite>();
        this.Subtypes = Array.Empty<Composite>();

        this.MetaPopulation.OnCreated(this);
    }

    public override bool IsInterface => false;

    public override bool IsClass => true;

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(this.SingularName))
        {
            return this.SingularName;
        }

        return this.Tag;
    }

    public override void Validate(ValidationLog validationLog)
    {
        this.ValidateObjectType(validationLog);
        this.ValidateComposite(validationLog);
    }

    public string[] AssignedWorkspaceNames { get; set; } = Array.Empty<string>();

    public override IReadOnlyList<Composite> Composites { get; }

    public override IReadOnlyList<Class> Classes { get; }

    public override Class ExclusiveClass => this;

    public override IReadOnlyList<Composite> DirectSubtypes { get; }

    public override IReadOnlyList<Composite> Subtypes { get; }

    public override IEnumerable<string> WorkspaceNames => this.AssignedWorkspaceNames;

    public override bool IsAssignableFrom(Composite objectType) => this.Equals(objectType);

    // Security
    public long CreatePermissionId { get; set; }
    
    public IReadOnlyDictionary<Guid, long> ReadPermissionIdByRelationTypeId { get; set; }

    public IReadOnlyDictionary<Guid, long> WritePermissionIdByRelationTypeId { get; set; }

    public IReadOnlyDictionary<Guid, long> ExecutePermissionIdByMethodTypeId { get; set; }
}
