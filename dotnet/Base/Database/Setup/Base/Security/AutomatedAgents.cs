﻿// <copyright file="People.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class AutomatedAgents
    {
        protected override void BasePrepare(Setup setup)
        {
            base.BasePrepare(setup);

            setup.AddDependency(this.ObjectType, this.M.UserGroup.Composite);
            setup.AddDependency(this.ObjectType, this.M.SecurityToken.Composite);
        }

        protected override void BaseSetup(Setup setup)
        {
            base.BaseSetup(setup);

            var merge = this.Transaction.Caches().AutomatedAgentByUniqueId().Merger().Function();

            var guest = merge(AutomatedAgent.GuestId, v => v.UserName = "Guest");
            merge(AutomatedAgent.SystemId, v => v.UserName = "System");

            var userGroups = this.Transaction.Scoped<UserGroupByUniqueId>();
            userGroups.Guests.AddMember(guest);
        }
    }
}
