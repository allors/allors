// <copyright file="RequiredTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Xunit;

    public class RequiredTest : DomainTest, IClassFixture<Fixture>
    {
        public RequiredTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostBuild()
        {
            var before = this.Transaction.Now().AddMilliseconds(-1);

            var units = this.Transaction.Build<UnitSample>();

            var after = this.Transaction.Now().AddMilliseconds(+1);

            Assert.False(units.ExistRequiredBinary);
            Assert.False(units.ExistRequiredString);

            Assert.True(units.ExistRequiredBoolean);
            Assert.True(units.ExistRequiredDateTime);
            Assert.True(units.ExistRequiredDecimal);
            Assert.True(units.ExistRequiredDouble);
            Assert.True(units.ExistRequiredInteger);
            Assert.True(units.ExistRequiredUnique);

            Assert.False(units.RequiredBoolean);
            Assert.True(units.RequiredDateTime > before && units.RequiredDateTime < after);
            Assert.Equal(0m, units.RequiredDecimal);
            Assert.Equal(0d, units.RequiredDouble);
            Assert.Equal(0, units.RequiredInteger);
            Assert.NotEqual(Guid.Empty, units.RequiredUnique);
        }
    }
}
