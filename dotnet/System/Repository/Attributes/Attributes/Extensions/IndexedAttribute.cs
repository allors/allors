﻿// <copyright file="IndexedAttribute.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// </copyright>


namespace Allors.Repository.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class IndexedAttribute : RepositoryAttribute, IExtensionAttribute
{
    public IndexedAttribute(bool value = true)
    {
        this.Value = value ? "true" : "false";
    }

    public string Name => "Indexed";

    public string Value { get; }
}