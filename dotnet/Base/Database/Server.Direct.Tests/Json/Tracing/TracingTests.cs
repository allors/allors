﻿// <copyright file="ContentTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ContentTests type.</summary>

namespace Tests
{
    using System.Linq;
    using System.Threading;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Adapters.Sql.Tracing;
    using Allors.Database.Domain;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Data;
    using Allors.Protocol.Json.SystemText;
    using Xunit;

    public class TracingTests : ApiTest, IClassFixture<Fixture>
    {
        private TraceX[] x;
        private TraceY[] y;
        private TraceZ[] z;

        public TracingTests(Fixture fixture) : base(fixture) => this.UnitConvert = new UnitConvert();

        public IUnitConvert UnitConvert { get; }

        [Fact]
        public void PullManyObjects()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            //sink.Breaker = v =>
            //{
            //    return v.Kind == EventKind.CommandsInstantiateObject;
            //};

            var pullRequest = new PullRequest
            {
                l = Enumerable.Range(0, 100).Select(v => new Pull
                {
                    o = this.x[v].Id,
                }).ToArray(),
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);

            tree.Clear();
            api.Pull(pullRequest);

            var nodes = tree.Nodes[0].Nodes;

            Assert.Empty(nodes.Where(v => v.Event is SqlInstantiateObjectEvent));
            Assert.Equal(1, nodes.Count(v => v.Event is SqlInstantiateReferencesEvent));
            Assert.All(nodes, v => Assert.Empty(v.Nodes));

            this.Transaction.Rollback();

            tree.Clear();

            api.Pull(pullRequest);

            nodes = tree.Nodes[0].Nodes;

            Assert.Single(nodes.Where(v => v.Event is SqlGetVersionsEvent));
            Assert.Empty(nodes.Where(v => v.Event is SqlInstantiateObjectEvent));
            Assert.Empty(nodes.Where(v => v.Event is SqlInstantiateReferencesEvent));
            Assert.All(nodes, v => Assert.Empty(v.Nodes));
        }

        [Fact]
        public void PullInclude()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            var pullRequest = new PullRequest
            {
                l = Enumerable.Range(0, 100).Select(v => new Pull
                {
                    o = this.x[v].Id,
                }).ToArray(),
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            api.Pull(pullRequest);

            tree.Clear();
            api.Pull(pullRequest);

            Assert.Single(tree.Nodes);

            var pullEventNode = tree.Nodes[0];

            Assert.True(pullEventNode.Event is PullEvent);

            var nodes = pullEventNode.Nodes;

            Assert.All(nodes, v => Assert.StartsWith("SqlPrefetch", v.Event.GetType().Name));
            Assert.All(nodes, v => Assert.Empty(v.Nodes));

            this.Transaction.Rollback();
            tree.Clear();

            //sink.Breaker = v =>
            //{
            //    return v.Kind == EventKind.PrefetcherPrefetchCompositesRoleRelationTable;
            //};

            api.Pull(pullRequest);
        }

        [Fact]
        public void Sync()
        {
            this.Populate();

            var sink = new Sink();
            var database = (Database)this.Transaction.Database;
            database.Sink = sink;

            this.Transaction = database.CreateTransaction();
            this.SetUser("jane@example.com");

            var tree = sink.TreeByTransaction[this.Transaction];

            tree.Clear();

            var syncRequest = new SyncRequest
            {
                o = [this.x[0].Id],
            };

            var api = new Api(this.Transaction, "Default", CancellationToken.None);
            api.Sync(syncRequest);

            tree.Clear();
            api.Sync(syncRequest);

            var syncEventNode = tree.Nodes[0];

            Assert.True(syncEventNode.Event is SyncEvent);

            var nodes = syncEventNode.Nodes;

            Assert.All(nodes, v => Assert.StartsWith("SqlPrefetch", v.Event.GetType().Name));
            Assert.All(nodes, v => Assert.Empty(v.Nodes));
        }

        private void Populate()
        {
            this.x = new TraceX[100];
            this.y = new TraceY[100];
            this.z = new TraceZ[100];

            for (var i = 0; i < 100; i++)
            {
                this.x[i] = this.Transaction.Build<TraceX>(v => v.AllorsString = $"X{i}");

                this.y[i] = this.Transaction.Build<TraceY>(v =>
                {
                    v.AllorsString = $"Y{i}";
                });

                this.z[i] = this.Transaction.Build<TraceZ>(v => v.AllorsString = $"Z{i}");
            }

            for (var i = 0; i < 100; i++)
            {
                this.x[i].One2One = this.y[i];
                this.y[i].One2One = this.z[i];
            }

            this.Transaction.Commit();
        }
    }
}
