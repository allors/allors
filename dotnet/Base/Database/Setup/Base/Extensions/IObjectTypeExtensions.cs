﻿// <copyright file="IObjectTypeExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain
{
    using System;
    using Allors.Database.Meta;

    public static class IObjectTypeExtensions
    {
        public static IObjects GetObjects(this ObjectType objectType, ITransaction transaction)
        {
            var objectFactory = transaction.Database.ObjectFactory;
            var type = typeof(Setup).Assembly.GetType(objectFactory.Namespace + "." + objectType.PluralName);
            return (IObjects)Activator.CreateInstance(type, transaction);
        }
    }
}
