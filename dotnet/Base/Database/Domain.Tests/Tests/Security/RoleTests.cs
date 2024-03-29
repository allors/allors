﻿// <copyright file="RoleTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Configuration.Derivations.Default;
    using Xunit;

    public class RoleTests : DomainTest, IClassFixture<Fixture>
    {
        public RoleTests(Fixture fixture) : base(fixture) { }

        public override Config Config => base.Config with { SetupSecurity = true };

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutANameThenRoleIsInvalid()
        {
            this.Transaction.Build<Role>();

            var validation = this.Transaction.Derive(false);

            Assert.True(validation.HasErrors);
            Assert.Single(validation.Errors);

            var derivationError = validation.Errors[0];

            Assert.Single(derivationError.Relations);
            Assert.Equal(typeof(DerivationErrorRequired), derivationError.GetType());
            Assert.Equal(this.M.Role.Name, derivationError.Relations[0].RoleType);
        }

        [Fact]
        public void GivenNoRolesWhenCreatingARoleWithoutAUniqueIdThenRoleIsValid()
        {
            var role = this.BuildRole("Role");

            Assert.True(role.ExistUniqueId);

            var validation = this.Transaction.Derive(false);

            Assert.False(validation.HasErrors);
        }
    }
}
