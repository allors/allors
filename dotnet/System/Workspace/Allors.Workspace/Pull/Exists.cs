﻿// <copyright file="Exists.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    
    using Meta;

    public class Exists(IRelationEndType propertyType = null) : IPropertyPredicate
    {
        public string[] Dependencies { get; set; }

        public string Parameter { get; set; }

        public IRelationEndType RelationEndType { get; set; } = propertyType;

        public void Accept(IVisitor visitor) => visitor.VisitExists(this);
    }
}
