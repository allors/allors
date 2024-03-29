﻿// <copyright file="Initialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql;

using System.Text;
using global::Npgsql;
using Allors.Database.Meta;

public class Initialization
{
    private readonly Database database;
    private readonly Mapping mapping;

    private Validation validation;

    internal Initialization(Database database)
    {
        this.database = database;
        this.mapping = (Mapping)database.Mapping;
    }

    internal void Execute()
    {
        this.validation = new Validation(this.database);

        if (this.validation.IsValid)
        {
            this.TruncateTables();
        }
        else
        {
            this.CreateSchema();

            this.DropProcedures();

            this.DropTables();

            this.CreateTables();

            this.CreateProcedures();

            this.CreateIndeces();
        }
    }

    private void CreateSchema()
    {
        if (!this.validation.Schema.Exists)
        {
            // CREATE SCHEMA must be in its own batch
            using (var connection = new NpgsqlConnection(this.database.ConnectionString))
            {
                connection.Open();
                try
                {
                    var cmdText = @"
CREATE SCHEMA " + this.database.SchemaName;
                    using (var command = new NpgsqlCommand(cmdText, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }

    private void DropProcedures()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                foreach (var name in this.validation.Schema.ProcedureByName.Keys)
                {
                    using (var command = new NpgsqlCommand("DROP FUNCTION " + name, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void TruncateTables()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                this.TruncateTable(connection, this.mapping.TableNameForObjects);

                foreach (var @class in this.mapping.Database.MetaPopulation.Classes)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];

                    this.TruncateTable(connection, tableName);
                }

                foreach (var roleType in this.mapping.Database.MetaPopulation.RoleTypes)
                {
                    var associationType = roleType.AssociationType;

                    if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !roleType.ExistExclusiveClasses))
                    {
                        var tableName = this.mapping.TableNameForRelationByRoleType[roleType];
                        this.TruncateTable(connection, tableName);
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void DropTables()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                this.DropTable(connection, this.mapping.TableNameForObjects);

                foreach (var @class in this.mapping.Database.MetaPopulation.Classes)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];

                    this.DropTable(connection, tableName);
                }

                foreach (var roleType in this.mapping.Database.MetaPopulation.RoleTypes)
                {
                    var associationType = roleType.AssociationType;

                    if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !roleType.ExistExclusiveClasses))
                    {
                        var tableName = this.mapping.TableNameForRelationByRoleType[roleType];
                        this.DropTable(connection, tableName);
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void CreateTables()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                {
                    var sql = new StringBuilder();
                    sql.Append("CREATE TABLE " + this.mapping.TableNameForObjects + "\n");
                    sql.Append("(\n");
                    sql.Append(Sql.Mapping.ColumnNameForObject + " BIGINT GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,\n");
                    sql.Append(Sql.Mapping.ColumnNameForClass + " " + Mapping.SqlTypeForClass + ",\n");
                    sql.Append(Sql.Mapping.ColumnNameForVersion + " " + Mapping.SqlTypeForVersion + "\n");
                    sql.Append(")\n");

                    using (var command = new NpgsqlCommand(sql.ToString(), connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                foreach (var @class in this.mapping.Database.MetaPopulation.Classes)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];

                    var sql = new StringBuilder();
                    sql.Append("CREATE TABLE " + tableName + "\n");
                    sql.Append("(\n");
                    sql.Append(Sql.Mapping.ColumnNameForObject + " " + Mapping.SqlTypeForObject + " PRIMARY KEY,\n");
                    sql.Append(Sql.Mapping.ColumnNameForClass + " " + Mapping.SqlTypeForClass);

                    foreach (var associationType in @class.AssociationTypes)
                    {
                        var roleType = associationType.RoleType;
                        if (!(associationType.IsMany && roleType.IsMany) && associationType.RoleType.ExistExclusiveClasses && roleType.IsMany)
                        {
                            sql.Append(",\n" + this.mapping.ColumnNameByRoleType[associationType.RoleType] + " " + Mapping.SqlTypeForObject);
                        }
                    }

                    foreach (var roleType in @class.RoleTypes)
                    {
                        var associationType = roleType.AssociationType;
                        if (roleType.ObjectType.IsUnit)
                        {
                            sql.Append(
                                ",\n" + this.mapping.ColumnNameByRoleType[roleType] + " " + this.mapping.GetSqlType(roleType));
                        }
                        else if (!(associationType.IsMany && roleType.IsMany) && roleType.ExistExclusiveClasses && !roleType.IsMany)
                        {
                            sql.Append(",\n" + this.mapping.ColumnNameByRoleType[roleType] + " " + Mapping.SqlTypeForObject);
                        }
                    }

                    sql.Append(")\n");

                    using (var command = new NpgsqlCommand(sql.ToString(), connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                foreach (var roleType in this.mapping.Database.MetaPopulation.RoleTypes)
                {
                    var associationType = roleType.AssociationType;
                   
                    if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !roleType.ExistExclusiveClasses))
                    {
                        var tableName = this.mapping.TableNameForRelationByRoleType[roleType];

                        var primaryKeyName = $"pk_{roleType.SingularFullName.ToLowerInvariant()}";

                        var sql =
                            $@"CREATE TABLE {tableName}(
    {Sql.Mapping.ColumnNameForAssociation} {Mapping.SqlTypeForObject},
    {Sql.Mapping.ColumnNameForRole} {Mapping.SqlTypeForObject},
    {(roleType.IsOne
        ? $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Sql.Mapping.ColumnNameForAssociation})\n"
        : $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Sql.Mapping.ColumnNameForAssociation}, {Sql.Mapping.ColumnNameForRole})\n")}

)";

                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void CreateProcedures()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                foreach (var dictionaryEntry in this.mapping.ProcedureDefinitionByName)
                {
                    var definition = dictionaryEntry.Value;
                    using (var command = new NpgsqlCommand(definition, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void CreateIndeces()
    {
        using (var connection = new NpgsqlConnection(this.database.ConnectionString))
        {
            connection.Open();
            try
            {
                foreach (var @class in this.mapping.Database.MetaPopulation.Classes)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];
                    foreach (var associationType in @class.AssociationTypes)
                    {
                        var roleType = associationType.RoleType;
                        if (roleType.IsIndexed)
                        {
                            if (!(associationType.IsMany && roleType.IsMany) && roleType.ExistExclusiveClasses && roleType.IsMany)
                            {
                                var indexName = "idx_" + @class.SingularName.ToLowerInvariant() + "_" +
                                                roleType.AssociationType.SingularFullName.ToLowerInvariant();
                                this.CreateIndex(connection, indexName, roleType, tableName);
                            }
                        }
                    }

                    foreach (var roleType in @class.RoleTypes)
                    {
                        if (roleType.IsIndexed)
                        {
                            if (roleType.ObjectType.IsUnit)
                            {
                                var unit = (Unit)roleType.ObjectType;
                                if (unit.IsString || unit.IsBinary)
                                {
                                    if (roleType.Size == -1 || roleType.Size > 4000)
                                    {
                                        continue;
                                    }
                                }
                            }

                            var associationType = roleType.AssociationType;
                            if (roleType.ObjectType.IsUnit)
                            {
                                var indexName = "idx_" + @class.SingularName.ToLowerInvariant() + "_" +
                                                roleType.SingularFullName.ToLowerInvariant();
                                this.CreateIndex(connection, indexName, roleType, tableName);
                            }
                            else if (!(associationType.IsMany && roleType.IsMany) && roleType.ExistExclusiveClasses && !roleType.IsMany)
                            {
                                var indexName = "idx_" + @class.SingularName.ToLowerInvariant() + "_" +
                                                roleType.SingularFullName.ToLowerInvariant();
                                this.CreateIndex(connection, indexName, roleType, tableName);
                            }
                        }
                    }
                }

                foreach (var roleType in this.mapping.Database.MetaPopulation.RoleTypes)
                {
                    if (roleType.IsIndexed)
                    {
                        var associationType = roleType.AssociationType;

                        if (!roleType.ObjectType.IsUnit &&
                            ((associationType.IsMany && roleType.IsMany) || !roleType.ExistExclusiveClasses))
                        {
                            var tableName = this.mapping.TableNameForRelationByRoleType[roleType];
                            var indexName = "idx_" + roleType.SingularFullName.ToLowerInvariant() + "_" + Sql.Mapping.ColumnNameForRole.ToLowerInvariant();
                            var sql = new StringBuilder();
                            sql.Append("CREATE INDEX " + indexName + "\n");
                            sql.Append("ON " + tableName + " (" + Sql.Mapping.ColumnNameForRole + ")");
                            using (var command = new NpgsqlCommand(sql.ToString(), connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }

    private void TruncateTable(NpgsqlConnection connection, string tableName)
    {
        var cmdText = @"TRUNCATE TABLE " + tableName + @";";
        using (var command = new NpgsqlCommand(cmdText, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private void DropTable(NpgsqlConnection connection, string tableName)
    {
        if (!this.validation.MissingTableNames.Contains(tableName))
        {
            using (var command = new NpgsqlCommand("DROP TABLE " + tableName, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    private void CreateIndex(NpgsqlConnection connection, string indexName, RoleType roleType, string tableName)
    {
        var sql = new StringBuilder();
        sql.Append("CREATE INDEX " + indexName + "\n");
        sql.Append("ON " + tableName + " (" + this.mapping.ColumnNameByRoleType[roleType] + ")");
        using (var command = new NpgsqlCommand(sql.ToString(), connection))
        {
            command.ExecuteNonQuery();
        }
    }
}
