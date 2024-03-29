﻿// <copyright file="DatabaseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsStrategySql type.
// </summary>

namespace Allors.Database.Adapters;

using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Allors.Database.Adapters.Sql.Npgsql;

public class DatabaseBuilder
{
    private readonly int? commandTimeout;
    private readonly IConfiguration configuration;
    private readonly IsolationLevel? isolationLevel;
    private readonly IObjectFactory objectFactory;
    private readonly IDatabaseServices scope;

    public DatabaseBuilder(IDatabaseServices scope, IConfiguration configuration, IObjectFactory objectFactory,
        IsolationLevel? isolationLevel = null, int? commandTimeout = null)
    {
        this.scope = scope;
        this.configuration = configuration;
        this.objectFactory = objectFactory;
        this.isolationLevel = isolationLevel;
        this.commandTimeout = commandTimeout;
    }

    public IDatabase Build()
    {
        var adapter = this.configuration["adapter"]?.Trim().ToUpperInvariant();
        var connectionString = this.configuration["ConnectionStrings:DefaultConnection"];

        return adapter switch
        {
            "MEMORY" => throw new NotImplementedException(),
            "NPGSQL" => new Database(this.scope,
                new Sql.Configuration
                {
                    ObjectFactory = this.objectFactory,
                    ConnectionString = connectionString,
                    IsolationLevel = this.isolationLevel,
                    CommandTimeout = this.commandTimeout,
                }),
            "SQLCLIENT" => new Sql.SqlClient.Database(this.scope,
                new Sql.Configuration
                {
                    ObjectFactory = this.objectFactory,
                    ConnectionString = connectionString,
                    IsolationLevel = this.isolationLevel,
                    CommandTimeout = this.commandTimeout,
                }),
            _ => throw new ArgumentOutOfRangeException(adapter),
        };
    }
}
