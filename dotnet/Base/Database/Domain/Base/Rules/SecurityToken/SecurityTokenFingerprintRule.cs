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

    public class SecurityTokenFingerprintRule : Rule<SecurityToken, SecurityTokenIndex>
    {
        public SecurityTokenFingerprintRule(IMetaIndex m) : base(m, m.SecurityToken, new Guid("0C788305-AD7E-4722-B03C-83B5DE3E881A")) =>
            this.Patterns =
            [
                this.Builder.Pattern(v=>v.Grants),
                this.Builder.Pattern<Grant>(m.Grant.EffectiveUsers, v=>v.SecurityTokensWhereGrant),
                this.Builder.Pattern<Grant>(m.Grant.EffectivePermissions, v=>v.SecurityTokensWhereGrant),
            ];

        public override void Derive(ICycle cycle, IEnumerable<SecurityToken> matches)
        {
            var validation = cycle.Validation;

            foreach (var securityToken in matches)
            {
                securityToken.DeriveSecurityTokenFingerprintRule(validation);
            }
        }
    }

    public static class SecurityTokenFingerprintRuleExtensions
    {
        public static void DeriveSecurityTokenFingerprintRule(this SecurityToken @this, IValidation validation) => @this.Fingerprint = Guid.NewGuid();
    }
}
