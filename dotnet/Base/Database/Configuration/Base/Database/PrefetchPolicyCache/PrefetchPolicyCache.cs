﻿// <copyright file="IPreparedSelects.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Allors.Database.Data;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Services;

    public class PrefetchPolicyCache : IPrefetchPolicyCache
    {
        private readonly IMetaIndex m;
        private readonly IDictionary<string, IDictionary<Class, PrefetchPolicy>> prefetchPolicyByClassByWorkspace;

        public PrefetchPolicyCache(IDatabase database, IMetaCache metaCache)
        {
            this.m = database.Services.Get<IMetaIndex>();

            this.PermissionsWithClass = new PrefetchPolicyBuilder()
                    .WithRule(this.m.Permission.ClassPointer)
                    .Build();

            this.Security = new PrefetchPolicyBuilder().WithSecurityRules(this.m).Build();

            this.prefetchPolicyByClassByWorkspace = new Dictionary<string, IDictionary<Class, PrefetchPolicy>>();
            foreach (var workspaceName in this.m.MetaPopulation.WorkspaceNames)
            {
                var roleTypesByClass = metaCache.GetWorkspaceRoleTypesByClass(workspaceName);

                var prefetchPolicyByClass = new Dictionary<Class, PrefetchPolicy>();
                foreach (var @class in metaCache.GetWorkspaceClasses(workspaceName))
                {
                    var prefetchPolicyBuilder = new PrefetchPolicyBuilder();
                    prefetchPolicyBuilder.WithWorkspaceRules(this.m, roleTypesByClass[@class]);
                    var prefetchPolicy = prefetchPolicyBuilder.Build();
                    prefetchPolicyByClass[@class] = prefetchPolicy;
                }

                this.prefetchPolicyByClassByWorkspace[workspaceName] = prefetchPolicyByClass;
            }
        }

        public PrefetchPolicy PermissionsWithClass { get; }

        public PrefetchPolicy Security { get; }

        public IDictionary<Class, PrefetchPolicy> WorkspacePrefetchPolicyByClass(string workspaceName) => this.prefetchPolicyByClassByWorkspace[workspaceName];

        public PrefetchPolicy ForNodes(Node[] nodes)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(this.m);
            foreach (var node in nodes)
            {
                var relationEndType = node.RelationEndType;
                if (relationEndType.ObjectType.IsComposite)
                {
                    builder.WithRule(relationEndType, this.Security);
                }
                else
                {
                    builder.WithRule(relationEndType);
                }
            }

            return builder.Build();
        }

        public PrefetchPolicy ForDependency(Composite composite, ISet<RelationEndType> relationEndTypes)
        {
            var builder = new PrefetchPolicyBuilder();
            builder.WithSecurityRules(this.m);
            foreach (var relationEndType in relationEndTypes)
            {
                if (relationEndType.ObjectType.IsComposite)
                {
                    builder.WithRule(relationEndType, this.Security);
                }
                else
                {
                    builder.WithRule(relationEndType);
                }
            }

            return builder.Build();
        }
    }
}
