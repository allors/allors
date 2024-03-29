﻿// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;

    public class OrganizationPostDeriveRule : Rule<Organization, OrganizationIndex>
    {
        public OrganizationPostDeriveRule(IMetaIndex m) : base(m, m.Organization, new Guid("755E60CF-1D5E-4D24-8FDE-396FF7C3030B")) =>
            this.Patterns =
            [
                this.Builder.Pattern(v=>v.PostDeriveTrigger),
            ];

        public override void Derive(ICycle cycle, IEnumerable<Organization> matches)
        {
            foreach (var organization in matches)
            {
                organization.PostDeriveTriggered = organization.PostDeriveTrigger;
            }
        }
    }
}
