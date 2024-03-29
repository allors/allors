﻿// <copyright file="LanguageTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Xunit;

    public class LanguageTests : DomainTest, IClassFixture<Fixture>
    {
        public LanguageTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenLanguageWhenValidatingThenRequiredRelationsMustExist()
        {
            this.Transaction.Build<Language>();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            this.Transaction.Build<Language>(v => v.Key = "XX");

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            this.Transaction.Build<Language>(v =>
            {
                v.Key = "XX";
            });

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            this.Transaction.Build<Language>(v =>
            {
                v.Key = "XX";
                v.NativeName = "XXXX";
            });

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
