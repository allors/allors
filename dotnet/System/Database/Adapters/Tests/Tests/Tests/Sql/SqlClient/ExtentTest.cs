﻿// <copyright file="ExtentTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient;

using System.Linq;
using Allors.Database.Domain;
using Meta;
using Xunit;

public class ExtentTest : Adapters.ExtentTest, IClassFixture<Fixture<ExtentTest>>
{
    private readonly Profile profile;

    public ExtentTest() => this.profile = new Profile(this.GetType().Name);

    protected override IProfile Profile => this.profile;

    public override void Dispose() => this.profile.Dispose();

    protected override ITransaction CreateTransaction() => this.profile.CreateTransaction();

    [Fact]
    public override void SortOne()
    {
        foreach (var init in this.Inits)
        {
            foreach (var marker in this.Markers)
            {
                init();
                this.Populate();
                var m = this.Transaction.Database.Services.Get<IMetaIndex>();

                this.Transaction.Commit();

                this.c1B.C1AllorsString = "3";
                this.c1C.C1AllorsString = "1";
                this.c1D.C1AllorsString = "2";

                this.Transaction.Commit();

                marker();

                var extent = this.Transaction.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString);

                var sortedObjects = extent.Cast<C1>().ToArray();
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1C, sortedObjects[1]);
                Assert.Equal(this.c1D, sortedObjects[2]);
                Assert.Equal(this.c1B, sortedObjects[3]);

                marker();

                extent = this.Transaction.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                sortedObjects = extent.Cast<C1>().ToArray();
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1C, sortedObjects[1]);
                Assert.Equal(this.c1D, sortedObjects[2]);
                Assert.Equal(this.c1B, sortedObjects[3]);

                marker();

                extent = this.Transaction.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                sortedObjects = extent.Cast<C1>().ToArray();
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1A, sortedObjects[0]);
                Assert.Equal(this.c1C, sortedObjects[1]);
                Assert.Equal(this.c1D, sortedObjects[2]);
                Assert.Equal(this.c1B, sortedObjects[3]);

                marker();

                extent = this.Transaction.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);

                sortedObjects = extent.Cast<C1>().ToArray();
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1B, sortedObjects[0]);
                Assert.Equal(this.c1D, sortedObjects[1]);
                Assert.Equal(this.c1C, sortedObjects[2]);
                Assert.Equal(this.c1A, sortedObjects[3]);

                marker();

                extent = this.Transaction.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);

                sortedObjects = extent.Cast<C1>().ToArray();
                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(this.c1B, sortedObjects[0]);
                Assert.Equal(this.c1D, sortedObjects[1]);
                Assert.Equal(this.c1C, sortedObjects[2]);
                Assert.Equal(this.c1A, sortedObjects[3]);

                foreach (var useOperator in this.UseOperator)
                {
                    if (useOperator)
                    {
                        marker();

                        var firstExtent = this.Transaction.Filter(m.C1.Composite);
                        firstExtent.AddLike(m.C1.C1AllorsString, "1");
                        var secondExtent = this.Transaction.Filter(m.C1.Composite);
                        var union = this.Transaction.Union(firstExtent, secondExtent);
                        secondExtent.AddLike(m.C1.C1AllorsString, "3");
                        union.AddSort(m.C1.C1AllorsString);

                        sortedObjects = union.Cast<C1>().ToArray();
                        Assert.Equal(2, sortedObjects.Length);
                        Assert.Equal(this.c1C, sortedObjects[0]);
                        Assert.Equal(this.c1B, sortedObjects[1]);
                    }
                }
            }
        }
    }

    [Fact]
    public override void SortTwo()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            this.Populate();

            this.c1B.C1AllorsString = "a";
            this.c1C.C1AllorsString = "b";
            this.c1D.C1AllorsString = "a";

            this.c1B.C1AllorsInteger = 2;
            this.c1C.C1AllorsInteger = 1;
            this.c1D.C1AllorsInteger = 0;

            this.Transaction.Commit();

            var extent = this.Transaction.Filter(m.C1.Composite);
            extent.AddSort(m.C1.C1AllorsString);
            extent.AddSort(m.C1.C1AllorsInteger);

            var sortedObjects = extent.Cast<C1>().ToArray();
            Assert.Equal(4, sortedObjects.Length);
            Assert.Equal(this.c1A, sortedObjects[0]);
            Assert.Equal(this.c1D, sortedObjects[1]);
            Assert.Equal(this.c1B, sortedObjects[2]);
            Assert.Equal(this.c1C, sortedObjects[3]);

            extent = this.Transaction.Filter(m.C1.Composite);
            extent.AddSort(m.C1.C1AllorsString);
            extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Ascending);

            sortedObjects = extent.Cast<C1>().ToArray();
            Assert.Equal(4, sortedObjects.Length);
            Assert.Equal(this.c1A, sortedObjects[0]);
            Assert.Equal(this.c1D, sortedObjects[1]);
            Assert.Equal(this.c1B, sortedObjects[2]);
            Assert.Equal(this.c1C, sortedObjects[3]);

            extent = this.Transaction.Filter(m.C1.Composite);
            extent.AddSort(m.C1.C1AllorsString);
            extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Descending);

            sortedObjects = extent.Cast<C1>().ToArray();
            Assert.Equal(4, sortedObjects.Length);
            Assert.Equal(this.c1A, sortedObjects[0]);
            Assert.Equal(this.c1B, sortedObjects[1]);
            Assert.Equal(this.c1D, sortedObjects[2]);
            Assert.Equal(this.c1C, sortedObjects[3]);

            extent = this.Transaction.Filter(m.C1.Composite);
            extent.AddSort(m.C1.C1AllorsString, SortDirection.Descending);
            extent.AddSort(m.C1.C1AllorsInteger, SortDirection.Descending);

            sortedObjects = extent.Cast<C1>().ToArray();
            Assert.Equal(4, sortedObjects.Length);
            Assert.Equal(this.c1C, sortedObjects[0]);
            Assert.Equal(this.c1B, sortedObjects[1]);
            Assert.Equal(this.c1D, sortedObjects[2]);
            Assert.Equal(this.c1A, sortedObjects[3]);
        }
    }

    [Fact]
    public override void SortDifferentTransaction()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var c1A = this.Transaction.Build<C1>();
            var c1B = this.Transaction.Build<C1>();
            var c1C = this.Transaction.Build<C1>();
            var c1D = this.Transaction.Build<C1>();

            c1A.C1AllorsString = "2";
            c1B.C1AllorsString = "1";
            c1C.C1AllorsString = "3";

            var extent = this.Transaction.Filter(m.C1.Composite);
            extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

            var sortedObjects = extent.Cast<C1>().ToArray();

            Assert.Equal(4, sortedObjects.Length);
            Assert.Equal(c1D, sortedObjects[0]);
            Assert.Equal(c1B, sortedObjects[1]);
            Assert.Equal(c1A, sortedObjects[2]);
            Assert.Equal(c1C, sortedObjects[3]);

            var c1AId = c1A.Id;

            this.Transaction.Commit();

            using (var transaction2 = this.CreateTransaction())
            {
                c1A = (C1)transaction2.Instantiate(c1AId);

                extent = transaction2.Filter(m.C1.Composite);
                extent.AddSort(m.C1.C1AllorsString, SortDirection.Ascending);

                sortedObjects = extent.Cast<C1>().ToArray();

                Assert.Equal(4, sortedObjects.Length);
                Assert.Equal(c1D, sortedObjects[0]);
                Assert.Equal(c1B, sortedObjects[1]);
                Assert.Equal(c1A, sortedObjects[2]);
                Assert.Equal(c1C, sortedObjects[3]);
            }
        }
    }
}
