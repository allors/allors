﻿// <copyright file="ExtentStatement.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql;

using System.Collections.Generic;
using Allors.Database.Meta;

public abstract class ExtentStatement
{
    private readonly List<AssociationType> referenceAssociationInstances;
    private readonly List<AssociationType> referenceAssociations;
    private readonly List<RoleType> referenceRoleInstances;
    private readonly List<RoleType> referenceRoles;

    protected ExtentStatement(IInternalExtent extent)
    {
        this.Extent = extent;

        this.referenceRoles = new List<RoleType>();
        this.referenceAssociations = new List<AssociationType>();

        this.referenceRoleInstances = new List<RoleType>();
        this.referenceAssociationInstances = new List<AssociationType>();
    }

    internal Mapping Mapping => this.Transaction.Database.Mapping;

    internal ExtentSort Sorter => this.Extent.Sorter;

    protected Transaction Transaction => this.Extent.Transaction;

    internal IInternalExtent Extent { get; }

    internal abstract bool IsRoot { get; }

    protected ObjectType Type => this.Extent.ObjectType;

    internal void AddJoins(ObjectType rootClass, string alias)
    {
        foreach (var role in this.referenceRoles)
        {
            var association = role.AssociationType;

            if (!role.ObjectType.IsUnit)
            {
                if ((association.IsMany && role.IsMany) || !role.ExistExclusiveClasses)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForRelationByRoleType[role] + " " +
                                role.SingularFullName + "_R");
                    this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + role.SingularFullName + "_R." +
                                Mapping.ColumnNameForAssociation);
                }
                else if (role.IsMany)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjectByClass[((Composite)role.ObjectType).ExclusiveClass] +
                                " " + role.SingularFullName + "_R");
                    this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + role.SingularFullName + "_R." +
                                this.Mapping.ColumnNameByRoleType[role]);
                }
            }
        }

        foreach (var role in this.referenceRoleInstances)
        {
            if (!role.ObjectType.IsUnit && role.IsOne)
            {
                if (!role.ExistExclusiveClasses)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(role));
                    this.Append(" ON " + this.GetJoinName(role) + "." + Mapping.ColumnNameForObject + "=" + role.SingularFullName + "_R." +
                                Mapping.ColumnNameForRole + " ");
                }
                else
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(role));
                    this.Append(" ON " + this.GetJoinName(role) + "." + Mapping.ColumnNameForObject + "=" + alias + "." +
                                this.Mapping.ColumnNameByRoleType[role] + " ");
                }
            }
        }

        foreach (var association in this.referenceAssociations)
        {
            var roleType = association.RoleType;

            if ((association.IsMany && roleType.IsMany) || !roleType.ExistExclusiveClasses)
            {
                this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForRelationByRoleType[roleType] + " " +
                            association.SingularFullName + "_A");
                this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + association.SingularFullName + "_A." +
                            Mapping.ColumnNameForRole);
            }
            else if (!roleType.IsMany)
            {
                this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjectByClass[association.Composite.ExclusiveClass] + " " +
                            association.SingularFullName + "_A");
                this.Append(" ON " + alias + "." + Mapping.ColumnNameForObject + "=" + association.SingularFullName + "_A." +
                            this.Mapping.ColumnNameByRoleType[roleType]);
            }
        }

        foreach (var association in this.referenceAssociationInstances)
        {
            var roleType = association.RoleType;

            if (!association.ObjectType.IsUnit && association.IsOne)
            {
                if (!roleType.ExistExclusiveClasses)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                    this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" +
                                association.SingularFullName + "_A." + Mapping.ColumnNameForAssociation + " ");
                }
                else if (roleType.IsOne)
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                    this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" +
                                association.SingularFullName + "_A." + Mapping.ColumnNameForObject + " ");
                }
                else
                {
                    this.Append(" LEFT OUTER JOIN " + this.Mapping.TableNameForObjects + " " + this.GetJoinName(association));
                    this.Append(" ON " + this.GetJoinName(association) + "." + Mapping.ColumnNameForObject + "=" + alias + "." +
                                this.Mapping.ColumnNameByRoleType[roleType] + " ");
                }
            }
        }
    }

    internal abstract string AddParameter(object obj);

    internal bool AddWhere(ObjectType rootClass, string alias)
    {
        var useWhere = this.Extent.ObjectType.ExclusiveClass == null;

        if (useWhere)
        {
            this.Append(" WHERE ( ");
            if (!this.Type.IsInterface)
            {
                this.Append(" " + alias + "." + Mapping.ColumnNameForClass + "=" + this.AddParameter(this.Type.Id));
            }
            else
            {
                var first = true;
                foreach (var subClass in ((Interface)this.Type).Classes)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        this.Append(" OR ");
                    }

                    this.Append(" " + alias + "." + Mapping.ColumnNameForClass + "=" + this.AddParameter(subClass.Id));
                }
            }

            this.Append(" ) ");
        }

        return useWhere;
    }

    internal abstract void Append(string part);

    internal abstract string CreateAlias();

    internal abstract ExtentStatement CreateChild(IInternalExtent extent, AssociationType association);

    internal abstract ExtentStatement CreateChild(IInternalExtent extent, RoleType role);

    internal string GetJoinName(AssociationType association) => association.SingularFullName + "_AC";

    internal string GetJoinName(RoleType role) => role.SingularFullName + "_RC";

    internal void UseAssociation(AssociationType association)
    {
        if (!association.ObjectType.IsUnit && !this.referenceAssociations.Contains(association))
        {
            this.referenceAssociations.Add(association);
        }
    }

    internal void UseAssociationInstance(AssociationType association)
    {
        if (!this.referenceAssociationInstances.Contains(association))
        {
            this.referenceAssociationInstances.Add(association);
        }
    }

    internal void UseRole(RoleType role)
    {
        if (!role.ObjectType.IsUnit && !this.referenceRoles.Contains(role))
        {
            this.referenceRoles.Add(role);
        }
    }

    internal void UseRoleInstance(RoleType role)
    {
        if (!this.referenceRoleInstances.Contains(role))
        {
            this.referenceRoleInstances.Add(role);
        }
    }
}
