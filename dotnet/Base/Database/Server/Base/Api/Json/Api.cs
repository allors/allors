﻿// <copyright file="PullExtent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemText;
    using Data;
    using Derivations;
    using Domain;
    using Meta;
    using Security;
    using Services;
    using Tracing;

    public class Api
    {
        public Api(ITransaction transaction, string workspaceName, CancellationToken cancellationToken)
        {
            this.Transaction = transaction;
            this.WorkspaceName = workspaceName;
            this.CancellationToken = cancellationToken;
            this.Sink = transaction.Database.Sink;

            var transactionServices = transaction.Services;
            var databaseServices = transaction.Database.Services;
            var metaCache = databaseServices.Get<IMetaCache>();

            this.PrefetchPolicyCache = databaseServices.Get<IPrefetchPolicyCache>();
            this.User = transactionServices.Get<IUserService>().User;
            this.AccessControl = transactionServices.Get<IWorkspaceAclsService>().Create(this.WorkspaceName);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(this.WorkspaceName);
            this.RoleTypesByClass = metaCache.GetWorkspaceRoleTypesByClass(this.WorkspaceName);
            this.M = databaseServices.Get<IMetaIndex>();
            this.PreparedSelects = databaseServices.Get<IPreparedSelects>();
            this.PreparedExtents = databaseServices.Get<IPreparedExtents>();
            this.Build = @class => transaction.Build(@class);
            this.Derive = () => this.Transaction.Derive(false);
            this.Security = databaseServices.Get<ISecurity>();

            this.UnitConvert = new UnitConvert();
        }


        public ITransaction Transaction { get; }

        public ISecurity Security { get; }

        public string WorkspaceName { get; set; }

        public CancellationToken CancellationToken { get; }

        public ISink Sink { get; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IUser User { get; }

        public IAccessControl AccessControl { get; }

        public IReadOnlySet<Class> AllowedClasses { get; }

        public IDictionary<Class, IReadOnlySet<RoleType>> RoleTypesByClass { get; }

        public IMetaIndex M { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        public Func<Class, IObject> Build { get; }

        public Func<IValidation> Derive { get; }

        public UnitConvert UnitConvert { get; }

        public InvokeResponse Invoke(InvokeRequest invokeRequest)
        {
            var @event = this.Sink?.OnInvoke(this.Transaction, invokeRequest);
            this.Sink?.OnBefore(@event);

            var invokeResponseBuilder = new InvokeResponseBuilder(this.Transaction, this.Derive, this.AccessControl, this.AllowedClasses);
            var invokeResponse = invokeResponseBuilder.Build(invokeRequest);

            if (@event != null)
            {
                @event.InvokeResponse = invokeResponse;
                this.Sink?.OnAfter(@event);
            }

            return invokeResponse;
        }

        public PullResponse Pull(PullRequest pullRequest)
        {
            var @event = this.Sink?.OnPull(this.Transaction, pullRequest);
            this.Sink?.OnBefore(@event);

            var pullResponseBuilder = new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.PrefetchPolicyCache, this.CancellationToken);
            var pullResponse = pullResponseBuilder.Build(pullRequest);

            if (@event != null)
            {
                @event.PullResponse = pullResponse;
                this.Sink?.OnAfter(@event);
            }

            return pullResponse;
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            var @event = this.Sink?.OnPush(this.Transaction, pushRequest);
            this.Sink?.OnBefore(@event);

            var pushResponseBuilder = new PushResponseBuilder(this.Transaction, this.Derive, this.M.MetaPopulation, this.AccessControl, this.AllowedClasses, this.Build, this.UnitConvert);
            var pushResponse = pushResponseBuilder.Build(pushRequest);

            if (@event != null)
            {
                @event.PushResponse = pushResponse;
                this.Sink?.OnAfter(@event);
            }

            return pushResponse;
        }

        public SyncResponse Sync(SyncRequest syncRequest)
        {
            var @event = this.Sink?.OnSync(this.Transaction, syncRequest);
            this.Sink?.OnBefore(@event);

            var prefetchPolicyByClass = this.PrefetchPolicyCache.WorkspacePrefetchPolicyByClass(this.WorkspaceName);
            var syncResponseBuilder = new SyncResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.RoleTypesByClass, prefetchPolicyByClass, this.UnitConvert);
            var syncResponse = syncResponseBuilder.Build(syncRequest);

            if (@event != null)
            {
                @event.SyncResponse = syncResponse;
                this.Sink?.OnAfter(@event);
            }

            return syncResponse;
        }

        public AccessResponse Access(AccessRequest accessRequest)
        {
            var responseBuilder = new AccessResponseBuilder(this.Transaction, this.Security, this.User, this.WorkspaceName);
            return responseBuilder.Build(accessRequest);
        }

        public PermissionResponse Permission(PermissionRequest permissionRequest)
        {
            var responseBuilder = new PermissionResponseBuilder(this.Transaction, this.AllowedClasses);
            return responseBuilder.Build(permissionRequest);
        }

        // TODO: Delete
        public PullResponseBuilder CreatePullResponseBuilder(string dependencyId = null)
        {
            // TODO: Dependencies
            return new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.PrefetchPolicyCache, this.CancellationToken);
        }
    }
}
