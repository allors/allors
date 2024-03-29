﻿// <copyright file="ChangedRoles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Database.Domain.Derivations.Rules
{
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Allors.Database.Meta;

    public class AssociationPattern : IAssociationPattern
    {
        public AssociationPattern(AssociationType associationType)
        {
            this.AssociationType = associationType;
        }

        public AssociationType AssociationType { get; }

        public IEnumerable<IObject> Eval(IObject role)
        {
            // TODO: Type check
            if (role != null)
            {
                yield return role;
            }
        }
    }
}
