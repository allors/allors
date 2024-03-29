﻿// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using Allors;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.SystemText;
    using Xunit;

    public class PullSecurityTests : ApiTest, IClassFixture<Fixture>
    {
        public PullSecurityTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public void SameWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = this.Transaction.Build<WorkspaceXObject1>();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Filter(m.WorkspaceXObject1.Composite) };
                var pullRequest = new PullRequest { l = [pull.ToJson(this.UnitConvert)], };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1s = pullResponse.c["WorkspaceXObject1s"];

                Assert.Single(wx1s);

                var wx1 = wx1s.First();

                Assert.Equal(x1.Id, wx1);
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l =
                                [
                    new Allors.Protocol.Json.Data.Pull
                    {
                        o = x1.Id,
                    },
                ],
                };

                var api = new Api(this.Transaction, "X", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                var wx1 = pullResponse.o["WorkspaceXObject1"];

                Assert.Equal(x1.Id, wx1);
            }
        }

        [Fact]
        public void DifferentWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = this.Transaction.Build<WorkspaceXObject1>();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Filter(m.WorkspaceXObject1.Composite) };

                var pullRequest = new PullRequest { l = [pull.ToJson(this.UnitConvert)] };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l =
                    [
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    ],
                };

                var api = new Api(this.Transaction, "Y", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);
                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void NoneWorkspace()
        {
            var m = this.M;
            this.SetUser("jane@example.com");

            var x1 = this.Transaction.Build<WorkspaceXObject1>();

            this.Transaction.Commit();

            // Extent
            {
                var pull = new Pull { Extent = new Filter(m.WorkspaceXObject1.Composite) };
                var pullRequest = new PullRequest { l = [pull.ToJson(this.UnitConvert)], };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.False(pullResponse.c.ContainsKey("WorkspaceXObject1s"));
            }

            // Instantiate
            {
                var pullRequest = new PullRequest
                {
                    l =
                    [
                        new Allors.Protocol.Json.Data.Pull
                        {
                            o = x1.Id,
                        },
                    ],
                };

                var api = new Api(this.Transaction, "None", CancellationToken.None);
                var pullResponse = api.Pull(pullRequest);

                Assert.Empty(pullResponse.o);
            }
        }

        [Fact]
        public void WithDeniedPermissions()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var data = this.Transaction.Build<Data>(v => v.String = "First");
            var permissions = this.Transaction.Filter<Permission>();
            var permission = permissions.First(v => Equals(v.Class, this.M.Data.Class) && v.InWorkspace("Default"));
            var revocation = this.Transaction.Build<Revocation>(v => v.AddDeniedPermission(permission));
            data.AddRevocation(revocation);

            this.Transaction.Commit();

            var pull = new Pull { Extent = new Filter(m.Data.Composite) };
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
            Assert.Equal(acl.Revocations.Select(v => v.Id), @object.r);
        }

        [Fact]
        public void WithDeniedPermissionsFromDatabaseAndOtherWorkspace()
        {
            var m = this.M;
            var user = this.SetUser("jane@example.com");

            var pull = new Pull { Extent = new Filter(m.Denied.Composite) };

            var pullRequest = new PullRequest
            {
                l =
                [
                    pull.ToJson(this.UnitConvert),
                ],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            var pullResponse = api.Pull(pullRequest);

            var pullResponseObject = pullResponse.p[0];

            var databaseWrite = this.Transaction.Filter<Permission>().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DatabaseProperty));
            var defaultWorkspaceWrite = this.Transaction.Filter<Permission>().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.DefaultWorkspaceProperty) && v.Operation == Operations.Write);
            var workspaceXWrite = this.Transaction.Filter<Permission>().First(v => v.Operation == Operations.Write && v.OperandType.Equals(m.Denied.WorkspaceXProperty) && v.Operation == Operations.Write);

            // TODO: Koen
            //Assert.Single(pullResponseObject.d);

            //Assert.Contains(defaultWorkspaceWrite.Id, pullResponseObject.d);
            //Assert.DoesNotContain(databaseWrite.Id, pullResponseObject.d);
            //Assert.DoesNotContain(workspaceXWrite.Id, pullResponseObject.d);
        }

        [Fact]
        public async void WithoutDeniedPermissions()
        {
            var user = this.SetUser("jane@example.com");

            var data = this.Transaction.Build<Data>(v => v.String = "First");

            this.Transaction.Derive();
            this.Transaction.Commit();

            var uri = new Uri(@"allors/pull", UriKind.Relative);

            var pull = new Pull { Extent = new Filter(this.M.Data.Composite) };

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
    }
}
