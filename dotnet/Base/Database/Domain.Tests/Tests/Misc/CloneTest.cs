﻿// <copyright file="BuilderTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors.Database.Data;
    using Xunit;

    public class CloneTest : DomainTest, IClassFixture<Fixture>
    {
        public CloneTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Shallow()
        {
            var c2A = this.BuildC2("c2A");
            var c2B = this.BuildC2("c2C");
            var c2C = this.BuildC2("c2B");
            var c2D = this.BuildC2("c2D");

            var c1A = this.BuildC1("c1A", v =>
            {
                v.C1C2One2One = c2A;
                v.AddC1C2One2Many(c2B);
                v.C1C2Many2One = c2C;
                v.AddC1C2Many2Many(c2D);
            });

            var cloned = c1A.Clone();

            Assert.NotEqual(c1A, cloned);
            Assert.Equal("c1A", cloned.C1AllorsString);
            Assert.Null(cloned.C1C2One2One);
            Assert.Empty(cloned.C1C2One2Manies);
            Assert.Equal(c2C, cloned.C1C2Many2One);
            Assert.Contains(c2D, cloned.C1C2Many2Manies);
        }

        [Fact]
        public void DeepLevelOne()
        {
            var c2A = this.BuildC2("c2A");
            var c2B = this.BuildC2("c2B");
            var c2C = this.BuildC2("c2C");
            var c2D = this.BuildC2("c2D");

            var c1A = this.BuildC1("c1A", v =>
            {
                v.C1C2One2One = c2A;
                v.AddC1C2One2Many(c2B);
                v.C1C2Many2One = c2C;
                v.AddC1C2Many2Many(c2D);
            });


            var deepClone = new C1TreeBuilder
            {
                C1C2One2One = new(this.M),
                C1C2One2Manies = new(this.M),
                C1C2Many2One = new(this.M),
                C1C2Many2Manies = new(this.M),
            }.Build();



            var cloned = c1A.Clone(deepClone);

            Assert.NotEqual(c1A, cloned);
            Assert.Equal("c1A", cloned.C1AllorsString);
            Assert.NotNull(cloned.C1C2One2One);
            Assert.NotEmpty(cloned.C1C2One2Manies);
            Assert.NotNull(cloned.C1C2Many2One);
            Assert.NotEmpty(cloned.C1C2Many2Manies);

            Assert.NotEqual(c2A, cloned.C1C2One2One);
            Assert.DoesNotContain(c2B, cloned.C1C2One2Manies);
            Assert.NotEqual(c2C, cloned.C1C2Many2One);
            Assert.DoesNotContain(c2D, cloned.C1C2Many2Manies);
        }

        [Fact]
        public void DeepLevelTwo()
        {
            var c2C = this.BuildC2("c2C");
            var c2B = this.BuildC2("c2B");

            var c2A = this.BuildC2("c2A", v =>
            {
                v.C2C2One2One = c2B;
                v.C2C2Many2One = c2C;
            });

            var c1A = this.BuildC1("c1A", v => v.C1C2One2One = c2A);

            var deepClone = new C1TreeBuilder
            {
                C1C2One2One = new C1C1C2One2OneNodeBuilder(this.M)
                {
                    C2 = new C2TreeBuilder()
                    {
                        C2C2One2One = new C2C2C2One2OneNodeBuilder(this.M)
                    }
                }
            }.Build();

            var cloned1A = c1A.Clone(deepClone);
            var cloned = cloned1A.C1C2One2One;

            Assert.NotNull(cloned.C2C2One2One);
            Assert.NotNull(cloned.C2C2Many2One);

            Assert.NotEqual(c2B, cloned.C2C2One2One);
            Assert.Equal(c2C, cloned.C2C2Many2One);
        }
    }
}
