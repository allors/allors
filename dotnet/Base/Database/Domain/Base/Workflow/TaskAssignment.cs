﻿// <copyright file="TaskAssignment.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class TaskAssignment
    {
        public void BaseOnPostDerive(ObjectOnPostDerive _)
        {
            if (!this.ExistSecurityTokens)
            {
                var cache = this.Transaction().Scoped<SecurityTokenByUniqueId>();
                var defaultSecurityToken = cache.DefaultSecurityToken;
                this.SecurityTokens = [defaultSecurityToken, this.User?.OwnerSecurityToken];
            }
        }

        public void BaseDelete(DeletableDelete _) => this.Notification?.CascadingDelete();
    }
}
