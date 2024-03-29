﻿// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Allors.Database.Configuration;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemText;
    using Xunit;
    using Pull = Allors.Protocol.Json.Data.Pull;
    using Result = Allors.Database.Data.Result;

    public class PullExtentTests : ApiTest, IClassFixture<Fixture>
    {
        public PullExtentTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public async void ExtentRef()
        {
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l =
                [
                    new Pull
                    {
                        er = PreparedExtents.OrganizationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var organizations = pullResponse.c["Organizations"];

            Assert.Single(organizations);
        }

        [Fact]
        public async void SelectRef()
        {
            // TODO: Not implemented
            this.SetUser("jane@example.com");

            var pullRequest = new PullRequest
            {
                l =
                [
                    new Pull
                    {
                        er = PreparedExtents.OrganizationByName,
                        a = new Dictionary<string, object> { ["name"] = "Acme" },
                    },
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var organizations = pullResponse.c["Organizations"];

            Assert.Single(organizations);
        }

        [Fact]
        public async void NamedResult()
        {
            var user = this.SetUser("jane@example.com");

            var data = this.Transaction.Build<Data>(v => v.String = "First");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Filter(this.M.Data.Composite),
                Results =
                [
                    new  Result { Name = "Datas" },
                ],
            };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var namedCollection = pullResponse.c["Datas"];

            Assert.Single(namedCollection);

            var namedObject = namedCollection.First();

            Assert.Equal(data.Id, namedObject);

            var objects = pullResponse.p;

            Assert.Single(objects);

            var @object = objects[0];

            var acls = new DatabaseAccessControl(this.Security, user);
            var acl = acls[data];

            Assert.NotNull(@object);

            Assert.Equal(data.Strategy.ObjectId, @object.i);
            Assert.Equal(data.Strategy.ObjectVersion, @object.v);
            Assert.Equal(acl.Grants.Select(v => v.Id), @object.g);
        }

        [Fact]
        public async void IncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Filter(this.M.C1.Composite)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" },
                },
                Results =
                [
                    new  Result
                    {
                        Include =
                        [
                            new Node(this.M.C1.C1C2One2One),
                        ],
                    },
                ],
            };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = this.Transaction.Filter<C1>().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
        }

        [Fact]
        public async void IncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Filter(this.M.C2.Composite)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" },
                },
                Results =
                [
                    new  Result
                    {
                        Include =
                        [
                            new Node(this.M.C2.C1WhereC1C2One2One),
                        ],
                    },
                ],
            };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = this.Transaction.Filter<C2>().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
        }

        [Fact]
        public async void SelectRoleOne2OneIncludeRoleOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Filter(this.M.C1.Composite)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1B" },
                },
                Results =
                [
                    new  Result
                    {
                        Select = new Select
                        {
                            RelationEndType = this.M.C1.C1C2One2One,
                            Include =
                            [
                                new Node(this.M.C2.C2C2One2One),
                            ],
                        },
                    },
                ],
            };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c1b = this.Transaction.Filter<C1>().First(v => "c1B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c1b.C1C2One2One.C2C2One2One.Id);
        }

        [Fact]
        public async void SelectAssociationOne2OneIncludeAssociationOne2One()
        {
            var user = this.SetUser("jane@example.com");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var pull = new Allors.Database.Data.Pull
            {
                Extent = new Filter(this.M.C2.Composite)
                {
                    Predicate = new Equals(this.M.C2.Name) { Value = "c2B" },
                },
                Results =
                [
                    new  Result
                    {
                        Select = new Select
                        {
                            RelationEndType = this.M.C2.C1WhereC1C2One2One,
                            Include =
                            [
                                new Node(this.M.C1.C1WhereC1C1One2One),
                            ],
                        },
                    },
                ],
            };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pool = pullResponse.p;

            Assert.Equal(2, pool.Length);

            var c2b = this.Transaction.Filter<C2>().First(v => "c2B".Equals(v.Name));

            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.Id);
            Assert.Contains(pool, v => v.i == c2b.C1WhereC1C2One2One.C1WhereC1C1One2One.Id);
        }
    }
}
