﻿// <copyright file="TaskExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Services;

    public static partial class TaskExtensions
    {
        public static void BaseOnBuild(this Task @this, ObjectOnBuild method)
        {
            if (!@this.ExistDateCreated)
            {
                @this.DateCreated = @this.Transaction().Now();
            }
        }

        public static void BaseDelete(this Task @this, DeletableDelete _)
        {
            foreach (var taskAssignment in @this.TaskAssignmentsWhereTask)
            {
                taskAssignment.CascadingDelete();
            }
        }

        public static void AssignPerformer(this Task @this) => @this.Performer = @this.Transaction().Services.Get<IUserService>().User as Person;

        public static void AssignParticipants(this Task @this, IEnumerable<User> participants)
        {
            var transaction = @this.Transaction();

            var participantSet = new HashSet<User>(participants.Where(v => v != null).Distinct());

            @this.Participants = participantSet.ToArray();

            // Manage Security
            var cache = @this.Transaction().Scoped<SecurityTokenByUniqueId>();
            var defaultSecurityToken = cache.DefaultSecurityToken;
            var securityTokens = new HashSet<SecurityToken> { defaultSecurityToken };
            var ownerSecurityTokens = participantSet.Where(v => v.ExistOwnerSecurityToken).Select(v => v.OwnerSecurityToken);
            securityTokens.UnionWith(ownerSecurityTokens);
            @this.SecurityTokens = securityTokens.ToArray();

            // Manage TaskAssignments
            foreach (var currentTaskAssignment in @this.TaskAssignmentsWhereTask.ToArray())
            {
                var user = currentTaskAssignment.User;
                if (!participantSet.Contains(user))
                {
                    if (currentTaskAssignment.ExistNotification)
                    {
                        currentTaskAssignment.Notification.Confirm();
                    }

                    currentTaskAssignment.Delete();

                }
                else
                {
                    participantSet.Remove(user);
                }
            }

            foreach (var user in participantSet)
            {
                transaction.Build<TaskAssignment>(v =>
                {
                    v.Task = @this;
                    v.User = user;
                });
            }
        }
    }
}
