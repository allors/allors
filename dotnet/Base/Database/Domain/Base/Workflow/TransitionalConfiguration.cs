﻿// <copyright file="TransitionalConfiguration.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    using Meta;

    public partial class TransitionalConfiguration
    {
        public TransitionalConfiguration(Class objectType, RoleType roleType)
        {
            var previousObjectState = objectType.RoleTypes.FirstOrDefault(v => v.Name.Equals("Previous" + roleType.Name));
            var lastObjectState = objectType.RoleTypes.FirstOrDefault(v => v.Name.Equals("Last" + roleType.Name));

            this.ObjectState = roleType;
            this.PreviousObjectState = previousObjectState ?? throw new Exception("Previous ObjectState is not defined for " + roleType.Name + " in type " + roleType.AssociationType.ObjectType.SingularName);
            this.LastObjectState = lastObjectState ?? throw new Exception("Last ObjectState is not defined for " + roleType.Name + " in type " + roleType.AssociationType.ObjectType.SingularName);
        }

        public RoleType ObjectState { get; set; }

        public RoleType PreviousObjectState { get; set; }

        public RoleType LastObjectState { get; set; }
    }
}
