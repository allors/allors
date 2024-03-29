﻿// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;

    public class GrantEffectiveUsersRule : Rule<Grant, GrantIndex>
    {
        public GrantEffectiveUsersRule(IMetaIndex m) : base(m, m.Grant, new Guid("2D3F4F02-7439-48E7-9E5B-363F4A4384F0")) =>
            this.Patterns =
            [
                this.Builder.Pattern(v=>v.Subjects),
                this.Builder.Pattern(v=>v.SubjectGroups),
                this.Builder.Pattern<UserGroup>(m.UserGroup.Members, v=>v.GrantsWhereSubjectGroup),
            ];

        public override void Derive(ICycle cycle, IEnumerable<Grant> matches)
        {
            foreach (var grant in matches)
            {
                grant.EffectiveUsers = grant
                    .SubjectGroups.SelectMany(v => v.Members)
                    .Union(grant.Subjects)
                    .ToArray();
            }
        }
    }
}
