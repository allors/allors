﻿// <copyright file="AccessControlList.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Security;
    using Allors.Database.Meta;

    /// <summary>
    /// List of permissions for an object/user combination.
    /// </summary>
    public class WorkspaceAccessControlList : IAccessControlList
    {
        private readonly WorkspaceAccessControl accessControl;
        private readonly IVersionedGrant[] grants;
        private readonly IVersionedRevocation[] revocations;

        private readonly IReadOnlyDictionary<Guid, long> readPermissionIdByRelationTypeId;
        private readonly IReadOnlyDictionary<Guid, long> writePermissionIdByRelationTypeId;
        private readonly IReadOnlyDictionary<Guid, long> executePermissionIdByMethodTypeId;

        internal WorkspaceAccessControlList(WorkspaceAccessControl accessControl, IObject @object, IVersionedGrant[] grants, IVersionedRevocation[] revocations)
        {
            this.accessControl = accessControl;
            this.grants = grants;
            this.revocations = revocations;
            this.Object = @object;

            if (this.Object != null)
            {
                var @class = this.Object.Strategy.Class;
                this.readPermissionIdByRelationTypeId = @class.ReadPermissionIdByRelationTypeId;
                this.writePermissionIdByRelationTypeId = @class.WritePermissionIdByRelationTypeId;
                this.executePermissionIdByMethodTypeId = @class.ExecutePermissionIdByMethodTypeId;
            }
        }

        public IObject Object { get; }

        IVersionedGrant[] IAccessControlList.Grants => this.grants;

        IVersionedRevocation[] IAccessControlList.Revocations => this.revocations;

        public bool CanRead(RoleType roleType) => this.readPermissionIdByRelationTypeId?.TryGetValue(roleType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool CanWrite(RoleType roleType) => !roleType.IsDerived && this.writePermissionIdByRelationTypeId?.TryGetValue(roleType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool CanExecute(MethodType methodType) => this.executePermissionIdByMethodTypeId?.TryGetValue(methodType.Id, out var permissionId) == true && this.IsPermitted(permissionId);

        public bool IsMasked() => this.accessControl.IsMasked(this.Object);

        private bool IsPermitted(long permissionId)
        {
            if (this.grants.Any(v => v.PermissionSet.Contains(permissionId)))
            {
                return this.revocations?.Any(v => v.PermissionSet.Contains(permissionId)) != true;
            }

            return false;
        }
    }
}
