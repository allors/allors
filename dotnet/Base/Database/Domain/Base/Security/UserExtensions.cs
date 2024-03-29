﻿// <copyright file="UserExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public static partial class UserExtensions
    {
        public static bool IsAdministrator(this User @this)
        {
            var cache = @this.Transaction().Scoped<UserGroupByUniqueId>();
            var administrators = cache.Administrators;
            return administrators.Members.Contains(@this);
        }

        public static T SetPassword<T>(this T @this, string clearTextPassword)
            where T : User
        {
            if (clearTextPassword == null)
            {
                @this.RemoveUserPasswordHash();
            }
            else
            {
                var passwordService = @this.Transaction().Database.Services.Get<IPasswordHasher>();
                @this.UserPasswordHash = passwordService.HashPassword(@this.UserName, clearTextPassword);
            }

            return @this;
        }

        public static bool VerifyPassword(this User @this, string clearTextPassword)
        {
            if (string.IsNullOrWhiteSpace(clearTextPassword))
            {
                return false;
            }

            var passwordService = @this.Transaction().Database.Services.Get<IPasswordHasher>();
            return passwordService.VerifyHashedPassword(@this.UserName, @this.UserPasswordHash, clearTextPassword);
        }

        public static void BaseOnPostBuild(this User @this, ObjectOnPostBuild method)
        {
            if (!@this.ExistOwnerGrant)
            {
                var cache = @this.Transaction().Scoped<RoleByUniqueId>();
                var ownerRole = cache.Owner;
                @this.OwnerGrant = @this.Transaction().Build<Grant>(grant =>
                {
                    grant.Role = ownerRole;
                    grant.AddSubject(@this);
                });
            }

            if (!@this.ExistOwnerSecurityToken)
            {
                @this.OwnerSecurityToken = @this.Transaction().Build<SecurityToken>(securityToken =>
                {
                    securityToken.AddGrant(@this.OwnerGrant);
                });
            }

            if (!@this.ExistUserSecurityStamp)
            {
                @this.UserSecurityStamp = Guid.NewGuid().ToString();
            }

            if (!@this.ExistNotificationList)
            {
                @this.NotificationList = @this.Transaction().Build<NotificationList>();
            }
        }

        public static void BaseDelete(this User @this, DeletableDelete method)
        {
            @this.OwnerGrant?.Delete();
            @this.OwnerSecurityToken?.Delete();

            foreach (var login in @this.Logins)
            {
                login.Delete();
            }

            @this.NotificationList?.CascadingDelete();
        }
    }
}
