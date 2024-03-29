﻿// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Tests
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class ManyToOneTests : Test
    {
        protected ManyToOneTests(Fixture fixture) : base(fixture)
        {

        }

        public override async System.Threading.Tasks.Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void SetRole()
        {
            /*   [Before]           [Set]           [After]
            *
            *  c1A      c1A     c1A      c1A     c1A      c1A
            *
            *                                             
            *  c1B ---- c1B     c1B      c1B     c1B ---- c1B
            *                         
            *                           
            *  c1C ---- c1C     c1C ----  c1C     c1C ---- c1C
            *        /                                 /   
            *       /                                 /
            *  c1D      c1D     c1D      c1D     c1D     c1D
            */
            {
                var workspace = this.Profile.Workspace;

                var c1A = workspace.Create<C1>();
                var c1B = workspace.Create<C1>();
                var c1C = workspace.Create<C1>();
                var c1D = workspace.Create<C1>();

                c1B.C1C1Many2One.Value = c1B;
                c1C.C1C1Many2One.Value = c1C;
                c1D.C1C1Many2One.Value = c1C;

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1C, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1B);
                c1C.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1C, c1D]);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                c1C.C1C1Many2One.Value = c1C;

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1C, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1B);
                c1C.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1C, c1D]);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                c1C.C1C1Many2One.Value = c1C;

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1C, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1B);
                c1C.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1C, c1D]);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                workspace.Reset();

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Null(c1B.C1C1Many2One.Value);
                Assert.Null(c1C.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                Assert.Empty(c1B.C1sWhereC1C1Many2One.Value);
                Assert.Empty(c1C.C1sWhereC1C1Many2One.Value);
            }

            /*   [Before]           [Set]           [After]
            *
            *  c1A      c1A     c1A      c1A     c1A      c1A
            *
            *                                             
            *  c1B ---- c1B     c1B      c1B     c1B ---- c1B
            *                         /                /
            *                        /                /  
            *  c1C ---- c1C     c1C      c1C     c1C      c1C
            *        /                                 /   
            *       /                                 /
            *  c1D      c1D     c1D      c1D     c1D     c1D
            */
            {
                var workspace = this.Profile.Workspace;

                var result = await workspace.PullAsync(new Pull { Extent = new Filter(this.M.C1) });
                var c1s = result.GetCollection<C1>();
                var c1A = c1s.Single(v => v.Name.Value == "c1A");
                var c1B = c1s.Single(v => v.Name.Value == "c1B");
                var c1C = c1s.Single(v => v.Name.Value == "c1C");
                var c1D = c1s.Single(v => v.Name.Value == "c1D");

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1C, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1B);
                c1C.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1C, c1D]);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                c1C.C1C1Many2One.Value = c1B;

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1B, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1B, c1C]);
                c1C.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1D);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                c1C.C1C1Many2One.Value = c1B;

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1B, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1B, c1C]);
                c1C.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1D);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);

                workspace.Reset();

                // Role
                Assert.Null(c1A.C1C1Many2One.Value);
                Assert.Equal(c1B, c1B.C1C1Many2One.Value);
                Assert.Equal(c1C, c1C.C1C1Many2One.Value);
                Assert.Equal(c1C, c1D.C1C1Many2One.Value);

                // Association
                Assert.Empty(c1A.C1sWhereC1C1Many2One.Value);
                c1B.C1sWhereC1C1Many2One.Value.ShouldContainSingle(c1B);
                c1C.C1sWhereC1C1Many2One.Value.ShouldHaveSameElements([c1C, c1D]);
                Assert.Empty(c1D.C1sWhereC1C1Many2One.Value);
            }
        }

        //[Fact]
        //public async void RemoveRole()
        //{
        //    foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
        //    {
        //        foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
        //        {
        //            foreach (var contextFactory in this.contextFactories)
        //            {
        //                var ctx = contextFactory();
        //                var (workspace1, workspace2) = ctx;

        //                var c1x_1 = await ctx.Create<C1>(workspace1, mode1);
        //                var c1y_2 = await ctx.Create<C1>(workspace2, mode2);

        //                await workspace2.PushAsync();
        //                var result = await workspace1.PullAsync(new Pull { Object = c1y_2.Strategy });

        //                var c1y_1 = result.Objects.Values.First().Cast<C1>();

        //                c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

        //                if (!c1x_1.C1C1Many2One.CanWrite)
        //                {
        //                    await workspace1.PullAsync(new Pull { Object = c1x_1.Strategy });
        //                }

        //                c1x_1.C1C1Many2One.Value = c1y_1;
        //                c1x_1.C1C1Many2One.Value.ShouldEqual(c1y_1, ctx, mode1, mode2);
        //                c1y_1.C1sWhereC1C1Many2One.Value.ShouldContain(c1x_1, ctx, mode1, mode2);

        //                c1x_1.C1C1Many2One.Value = null;
        //                c1x_1.C1C1Many2One.Value.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
        //                c1y_1.C1sWhereC1C1Many2One.Value.ShouldNotContain(c1x_1, ctx, mode1, mode2);
        //            }
        //        }
        //    }
        //}
    }
}
