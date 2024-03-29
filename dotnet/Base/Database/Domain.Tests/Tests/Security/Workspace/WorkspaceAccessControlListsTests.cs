﻿// <copyright file="DatabaseAccessControlListTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Allors.Database.Configuration;
    using Meta;
    using Xunit;

    public class WorkspaceAccessControlListsTests : DomainTest, IClassFixture<Fixture>
    {
        private string workspaceName = "Default";

        public WorkspaceAccessControlListsTests(Fixture fixture) : base(fixture) { }

        public override Config Config => base.Config with { SetupSecurity = true };

        [Fact]
        public void InitialWithoutAccessControl()
        {
            var person = this.BuildPerson("John", "Doe");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var organization = this.BuildOrganization("Organization");

            var aclService = new WorkspaceAclsService(this.Security, new WorkspaceMask(this.M), person);
            var acl = aclService.Create(this.workspaceName)[organization];

            Assert.False(acl.CanRead(this.M.Organization.Name));
        }

        [Fact]
        public void Initial()
        {
            var permission = this.FindPermission(this.M.Organization.Name, Operations.Read);
            var role = this.BuildRole("Role", permission);
            var person = this.BuildPerson("John", "Doe");
            var grant = this.BuildGrant(person, role);

            var initialSecurityToken = this.Transaction.Scoped<SecurityTokenByUniqueId>().InitialSecurityToken;
            initialSecurityToken.AddGrant(grant);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var organization = this.BuildOrganization("Organization");

            var aclService = new WorkspaceAclsService(this.Security, new WorkspaceMask(this.M), person);
            var acl = aclService.Create(this.workspaceName)[organization];

            Assert.True(acl.CanRead(this.M.Organization.Name));
        }

        [Fact]
        public void GivenAWorkspaceAccessControlListsThenADatabaseDeniedPermissionsIsNotPresent()
        {
            var administrator = this.BuildPerson("administrator");
            var administrators = this.Transaction.Scoped<UserGroupByUniqueId>().Administrators;
            administrators.AddMember(administrator);

            var databaseOnlyPermissions = this.Transaction.Filter<Permission>().Where(v => v.ExistOperandType && v.OperandType.Equals(this.M.Person.DatabaseOnlyField));
            var databaseOnlyReadPermission = databaseOnlyPermissions.First(v => v.Operation == Operations.Read);

            var revocation = this.BuildRevocation(databaseOnlyReadPermission);

            administrator.AddRevocation(revocation);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var aclService = new WorkspaceAclsService(this.Security, new WorkspaceMask(this.M), administrator);
            var acl = aclService.Create(this.workspaceName)[administrator];

            Assert.DoesNotContain(revocation.Id, acl.Revocations.Select(v => v.Id));
        }

        [Fact]
        public void GivenAWorkspaceAccessControlListsThenAWorkspaceDeniedPermissionsIsNotPresent()
        {
            var administrator = this.BuildPerson("administrator");
            var administrators = this.Transaction.Scoped<UserGroupByUniqueId>().Administrators;
            administrators.AddMember(administrator);

            var workspacePermissions = this.Transaction.Filter<Permission>().Where(v => v.ExistOperandType && v.OperandType.Equals(this.M.Person.DefaultWorkspaceField));
            var workspaceReadPermission = workspacePermissions.First(v => v.Operation == Operations.Read);
            var revocation = this.BuildRevocation(workspaceReadPermission);

            administrator.AddRevocation(revocation);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var aclService = new WorkspaceAclsService(this.Security, new WorkspaceMask(this.M), administrator);
            var acl = aclService.Create(this.workspaceName)[administrator];

            Assert.Contains(revocation.Id, acl.Revocations.Select(v => v.Id));
        }

        [Fact]
        public void GivenAWorkspaceAccessControlListsThenAnotherWorkspaceDeniedPermissionsIsNotPresent()
        {
            var administrator = this.BuildPerson("administrator");
            var administrators = this.Transaction.Scoped<UserGroupByUniqueId>().Administrators;
            administrators.AddMember(administrator);

            var workspacePermissions = this.Transaction.Filter<Permission>().Where(v => v.ExistOperandType && v.OperandType.Equals(this.M.Person.DefaultWorkspaceField));
            var workspaceReadPermission = workspacePermissions.First(v => v.Operation == Operations.Read);
            var revocation = this.BuildRevocation(workspaceReadPermission);

            administrator.AddRevocation(revocation);

            this.Transaction.Derive();
            this.Transaction.Commit();

            var aclService = new WorkspaceAclsService(this.Security, new WorkspaceMask(this.M), administrator);
            var acl = aclService.Create("X")[administrator];

            Assert.DoesNotContain(revocation.Id, acl.Revocations.Select(v => v.Id));
        }
    }
}
