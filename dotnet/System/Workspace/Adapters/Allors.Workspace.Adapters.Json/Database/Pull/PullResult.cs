﻿// <copyright file="RemotePullResult.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Pull;

    public class PullResult : Result, IPullResultInternals
    {
        private IDictionary<string, IStrategy> objects;

        private IDictionary<string, IStrategy[]> collections;

        private IDictionary<string, object> values;

        private readonly PullResponse pullResponse;

        public PullResult(Adapters.Workspace workspace, PullResponse response) : base(workspace, response)
        {
            this.Workspace = workspace;
            this.pullResponse = response;
        }

        private IWorkspace Workspace { get; }

        public IDictionary<string, IStrategy> Objects => this.objects ??= this.pullResponse.o.ToDictionary(pair => pair.Key, pair => base.Workspace.Instantiate(pair.Value));

        public IDictionary<string, IStrategy[]> Collections => this.collections ??= this.pullResponse.c.ToDictionary(pair => pair.Key, pair => pair.Value.Select(base.Workspace.Instantiate).ToArray());

        public IDictionary<string, object> Values => this.values ??= this.pullResponse.v.ToDictionary(pair => pair.Key, pair => pair.Value);

        public IStrategy[] GetCollection(Meta.IComposite objectType)
        {
            var key = objectType.PluralName;
            return this.GetCollection(key);
        }

        public IStrategy[] GetCollection(string key) => this.Collections.TryGetValue(key.ToUpperInvariant(), out var collection) ? collection?.ToArray() : null;

        public IStrategy GetObject(Meta.IComposite objectType)
        {
            var key = objectType.SingularName;
            return this.GetObject(key);
        }

        public IStrategy GetObject(string key) => this.Objects.TryGetValue(key.ToUpperInvariant(), out var @object) ? @object : null;

        public object GetValue(string key) => this.Values[key.ToUpperInvariant()];

        public T GetValue<T>(string key) => (T)this.GetValue(key.ToUpperInvariant());
    }
}
