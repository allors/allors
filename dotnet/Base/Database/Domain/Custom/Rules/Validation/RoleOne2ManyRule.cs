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

    public class RoleOne2ManyRule : Rule<AA, AAIndex>
    {
        public RoleOne2ManyRule(IMetaIndex m) : base(m, m.AA, new Guid("d40ab5c5-c248-4455-bad4-8c825f48e080")) =>
            this.Patterns =
            [
                this.Builder.Pattern<CC>(m.CC.Assigned, v=>v.BBWhereOne2Many.AAWhereOne2Many),
                this.Builder.Pattern<CC>(m.CC.Assigned, v=>v.BBWhereUnusedOne2Many.AAWhereUnusedOne2Many),
            ];

        public override void Derive(ICycle cycle, IEnumerable<AA> matches)
        {
            foreach (var aa in matches)
            {
                aa.Derived = aa.One2Many.FirstOrDefault()?.One2Many.FirstOrDefault()?.Assigned;
            }
        }
    }
}
