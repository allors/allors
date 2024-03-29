﻿// <copyright file="ChangeSetTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Workspace.Adapters.Tests
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class MergeTests : Test
    {
        protected MergeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void DatabaseUnitMergeError()
        {
            await this.Login("administrator");

            var workspace1 = this.Workspace;
            var workspace2 = this.Profile.CreateExclusiveWorkspace();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };

            var result = await workspace1.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            result = await workspace2.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString.Value = "X";
            c1a_2.C1AllorsString.Value = "Y";

            await workspace2.PushAsync();

            Assert.Equal("X", c1a_1.C1AllorsString.Value);

            result = await workspace1.PullAsync(pull);

            Assert.Equal("Y", c1a_1.C1AllorsString.Value);

            Assert.True(result.HasErrors);
            Assert.Single(result.MergeErrors);

            var mergeError = result.MergeErrors.First();

            Assert.Equal(c1a_1.Strategy, mergeError.Association);
        }

        [Fact]
        public async void DatabaseOne2OneMergeError()
        {
            await this.Login("administrator");

            var workspace1 = this.Workspace;
            var workspace2 = this.Profile.CreateExclusiveWorkspace();

            Pull[] pull = [
                new Pull
                {
                    Extent = new Filter(this.M.C1),
                },
            ];

            var result_1 = await workspace1.PullAsync(pull);
            var c1_1 = result_1.GetCollection<C1>();

            var c1a_1 = c1_1.First(v => v.Name.Value == "c1A");
            var c1b_1 = c1_1.First(v => v.Name.Value == "c1B");
            var c1c_1 = c1_1.First(v => v.Name.Value == "c1C");
            var c1d_1 = c1_1.First(v => v.Name.Value == "c1D");

            c1b_1.C1C1One2One.Value = c1d_1;

            var result_2 = await workspace2.PullAsync(pull);
            var c1_2 = result_2.GetCollection<C1>();

            var c1a_2 = c1_2.First(v => v.Name.Value == "c1A");
            var c1b_2 = c1_2.First(v => v.Name.Value == "c1B");
            var c1c_2 = c1_2.First(v => v.Name.Value == "c1C");
            var c1d_2 = c1_2.First(v => v.Name.Value == "c1D");

            c1b_2.C1C1One2One.Value = c1a_2;

            await workspace2.PushAsync();

            result_1 = await workspace1.PullAsync(pull);

            c1a_1 = c1_1.First(v => v.Name.Value == "c1A");
            c1b_1 = c1_1.First(v => v.Name.Value == "c1B");
            c1c_1 = c1_1.First(v => v.Name.Value == "c1C");
            c1d_1 = c1_1.First(v => v.Name.Value == "c1D");

            Assert.Equal(c1a_1, c1b_1.C1C1One2One.Value);
            Assert.Equal(c1d_1, c1c_1.C1C1One2One.Value);

            Assert.True(result_1.HasErrors);
            Assert.Single(result_1.MergeErrors);

            var mergeError = result_1.MergeErrors.First();

            Assert.Equal(c1b_1.Strategy, mergeError.Association);
        }

        [Fact]
        public async void DatabaseOne2ManyMergeError()
        {
            await this.Login("administrator");

            var workspace1 = this.Workspace;
            var workspace2 = this.Profile.CreateExclusiveWorkspace();

            Pull[] pull = [
                new Pull
                {
                    Extent = new Filter(this.M.C1),
                },
            ];

            var result_1 = await workspace1.PullAsync(pull);
            var c1_1 = result_1.GetCollection<C1>();

            var c1b_1 = c1_1.First(v => v.Name.Value == "c1B");
            var c1c_1 = c1_1.First(v => v.Name.Value == "c1C");
            var c1d_1 = c1_1.First(v => v.Name.Value == "c1D");

            c1b_1.C1C1One2Manies.Add(c1d_1);

            var result_2 = await workspace2.PullAsync(pull);
            var c1_2 = result_2.GetCollection<C1>();

            var c1a_2 = c1_2.First(v => v.Name.Value == "c1A");
            var c1b_2 = c1_2.First(v => v.Name.Value == "c1B");
            var c1c_2 = c1_2.First(v => v.Name.Value == "c1C");
            var c1d_2 = c1_2.First(v => v.Name.Value == "c1D");

            c1b_2.C1C1One2Manies.Add(c1c_2);

            await workspace2.PushAsync();

            result_1 = await workspace1.PullAsync(pull);

            c1b_1 = c1_1.First(v => v.Name.Value == "c1B");
            c1c_1 = c1_1.First(v => v.Name.Value == "c1C");
            c1d_1 = c1_1.First(v => v.Name.Value == "c1D");

            Assert.Equal(2, c1b_1.C1C1One2Manies.Value.Count());
            Assert.Contains(c1b_1, c1b_1.C1C1One2Manies.Value);
            Assert.Contains(c1c_1, c1b_1.C1C1One2Manies.Value);

            Assert.Single(c1c_1.C1C1One2Manies.Value);
            Assert.Contains(c1d_1, c1c_1.C1C1One2Manies.Value);

            Assert.True(result_1.HasErrors);
            Assert.Single(result_1.MergeErrors);

            var mergeError = result_1.MergeErrors.First();

            Assert.Equal(c1b_1.Strategy, mergeError.Association);
        }

    }
}
