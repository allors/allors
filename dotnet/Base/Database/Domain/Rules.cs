﻿// <copyright file="ObjectsBase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Reflection;

namespace Allors.Database.Domain
{
    using System.Linq;
    using System;
    using Allors.Database.Domain.Derivations.Rules;

    public static class Rules
    {
        public static Rule[] Create(IMetaIndex m)
        {
            var assembly = typeof(Rules).Assembly;

            var types = assembly.GetTypes()
                .Where(type => type.Namespace != null &&
                               !type.IsAbstract &&
                               type.GetTypeInfo().IsSubclassOf(typeof(Rule)))
                .ToArray();

            var rules = types.Select(v => Activator.CreateInstance(v, m))
                .Cast<Rule>()
                .ToArray();

            var duplicates = rules.GroupBy(v => v.Id).Where(g => g.Count() > 1).ToArray();

            if (duplicates.Length != 0)
            {
                throw new ArgumentException("Duplicate rules detected: " + string.Join(", ", duplicates.Select(v => v.Key)));
            }

            return rules;
        }
    }
}
