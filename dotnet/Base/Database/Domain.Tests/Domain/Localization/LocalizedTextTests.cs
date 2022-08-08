// <copyright file="LocalizedTextTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class LocalizedTextTests : DomainTest, IClassFixture<Fixture>
    {
        public LocalizedTextTests(Fixture fixture) : base(fixture) { }

        [Fact(Skip = "TODO: Koen  Locale is required")]
        public void RequiredRoleTypes()
        {
            var @class = this.M.LocalizedText;

            var requiredRoleTypes = @class.RequiredRoleTypes;

            Assert.Equal(2, requiredRoleTypes.Length);

            Assert.Contains(@class.Locale, requiredRoleTypes);
            Assert.Contains(@class.Text, requiredRoleTypes);
        }
    }
}