﻿// <copyright file="Organization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>

namespace Allors.Database.Domain
{
    public partial class Organizations
    {
        protected override void CustomPrepare(Security security)
        {
            base.CustomPrepare(security);

            security.AddDependency(this.ObjectType, this.M.Revocation.Composite);
        }

        protected override void CustomSecure(Security security)
        {
            base.CustomSecure(security);

            var revocations = this.Transaction.Scoped<RevocationByUniqueId>();
            var permissions = this.Transaction.Scoped<PermissionByMeta>();

            revocations.ToggleRevocation.DeniedPermissions =
            [
                permissions.Get(this.Meta.Class, this.Meta.Name, Operations.Write),
                permissions.Get(this.Meta.Class, this.Meta.Owner, Operations.Write),
                permissions.Get(this.Meta.Class, this.Meta.Employees, Operations.Write),
            ];
        }
    }
}
