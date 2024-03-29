﻿// <copyright file="ToJsonVisitor.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database;
using Allors.Database.Data;
using Allors.Database.Meta;
using Allors.Workspace.Data;
using And = Allors.Workspace.Data.And;
using Between = Allors.Workspace.Data.Between;
using Equals = Allors.Workspace.Data.Equals;
using Except = Allors.Workspace.Data.Except;
using Exists = Allors.Workspace.Data.Exists;
using Extent = Allors.Workspace.Data.Extent;
using GreaterThan = Allors.Workspace.Data.GreaterThan;
using IAssociationType = Allors.Workspace.Meta.IAssociationType;
using Instanceof = Allors.Workspace.Data.Instanceof;
using Intersect = Allors.Workspace.Data.Intersect;
using IPredicate = Allors.Database.Data.IPredicate;
using IRoleType = Allors.Workspace.Meta.IRoleType;
using LessThan = Allors.Workspace.Data.LessThan;
using Like = Allors.Workspace.Data.Like;
using Node = Allors.Database.Data.Node;
using Not = Allors.Workspace.Data.Not;
using Or = Allors.Workspace.Data.Or;
using Pull = Allors.Database.Data.Pull;
using Result = Allors.Database.Data.Result;
using Select = Allors.Database.Data.Select;
using Sort = Allors.Database.Data.Sort;
using Union = Allors.Workspace.Data.Union;

namespace Allors.Workspace.Protocol.Direct
{
    using Filter = Data.Filter;
    using Has = Data.Has;
    using In = Data.In;
    using Intersects = Data.Intersects;

    public class ToDatabaseVisitor
    {
        private readonly ITransaction transaction;
        private readonly MetaPopulation metaPopulation;

        public ToDatabaseVisitor(ITransaction transaction)
        {
            this.transaction = transaction;
            this.metaPopulation = transaction.Database.MetaPopulation;
        }

        public Pull Visit(Data.Pull ws) =>
            new Pull
            {
                ExtentRef = ws.ExtentRef,
                Extent = this.Visit(ws.Extent),
                ObjectType = this.Visit(ws.ObjectType),
                Object = this.Visit(ws.Object) ?? this.Visit(ws.ObjectId),
                Results = this.Visit(ws.Results),
                Arguments = this.Visit(ws.Arguments)
            };

        private IExtent Visit(Extent ws) =>
            ws switch
            {
                Filter filter => this.Visit(filter),
                Except except => this.Visit(except),
                Intersect intersect => this.Visit(intersect),
                Union union => this.Visit(union),
                null => null,
                _ => throw new Exception($"Unknown implementation of IExtent: {ws.GetType()}")
            };

        private Database.Data.Filter Visit(Filter ws) => new Database.Data.Filter(this.Visit(ws.ObjectType))
        {
            Predicate = this.Visit(ws.Predicate),
            Sorting = this.Visit(ws.Sorting)
        };

        private IPredicate Visit(Data.IPredicate ws) =>
            ws switch
            {
                And and => this.Visit(and),
                Between between => this.Visit(between),
                In containedIn => this.Visit(containedIn),
                Intersects intersects => this.Visit(intersects),
                Has contains => this.Visit(contains),
                Equals equals => this.Visit(equals),
                Exists exists => this.Visit(exists),
                GreaterThan greaterThan => this.Visit(greaterThan),
                Instanceof instanceOf => this.Visit(instanceOf),
                LessThan lessThan => this.Visit(lessThan),
                Like like => this.Visit(like),
                Not not => this.Visit(not),
                Or or => this.Visit(or),
                null => null,
                _ => throw new Exception($"Unknown implementation of IExtent: {ws.GetType()}")
            };

        private IPredicate Visit(And ws) => new Database.Data.And(ws.Operands?.Select(this.Visit).ToArray());

        private IPredicate Visit(Between ws) => new Database.Data.Between(this.Visit(ws.RoleType))
        {
            Parameter = ws.Parameter,
            Values = ws.Values,
            Paths = this.Visit(ws.Paths)
        };

        private IPredicate Visit(In ws) => new Database.Data.In(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
            Objects = this.Visit(ws.Objects),
            Extent = this.Visit(ws.Extent),
        };

        private IPredicate Visit(Intersects ws) => new Database.Data.Intersects(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
            Objects = this.Visit(ws.Objects),
            Extent = this.Visit(ws.Extent),
        };

        private IPredicate Visit(Has ws) => new Database.Data.Has(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
            Object = this.Visit(ws.Object)
        };

        private IPredicate Visit(Equals ws) => new Database.Data.Equals(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
            Object = this.Visit(ws.Object) ?? this.Visit(ws.ObjectId),
            Value = ws.Value,
            Path = this.Visit(ws.Path),
        };

        private IPredicate Visit(Exists ws) => new Database.Data.Exists(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
        };

        private IPredicate Visit(GreaterThan ws) => new Database.Data.GreaterThan(this.Visit(ws.RoleType))
        {
            Parameter = ws.Parameter,
            Value = ws.Value,
            Path = this.Visit(ws.Path)
        };

        private IPredicate Visit(Instanceof ws) => new Database.Data.Instanceof(this.Visit(ws.RelationEndType))
        {
            Parameter = ws.Parameter,
            ObjectType = this.Visit(ws.ObjectType)
        };

        private IPredicate Visit(LessThan ws) => new Database.Data.LessThan(this.Visit(ws.RoleType))
        {
            Parameter = ws.Parameter,
            Value = ws.Value,
            Path = this.Visit(ws.Path)
        };

        private IPredicate Visit(Like ws) => new Database.Data.Like(this.Visit(ws.RoleType))
        {
            Parameter = ws.Parameter,
            Value = ws.Value,
        };

        private IPredicate Visit(Not ws) => new Database.Data.Not(this.Visit(ws.Operand));

        private IPredicate Visit(Or ws) => new Database.Data.Or(ws.Operands?.Select(this.Visit).ToArray());

        private Database.Data.Except Visit(Except ws) => new Database.Data.Except(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private Database.Data.Intersect Visit(Intersect ws) => new Database.Data.Intersect(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private Database.Data.Union Visit(Union ws) => new Database.Data.Union(ws.Operands?.Select(this.Visit).ToArray()) { Sorting = this.Visit(ws.Sorting) };

        private Database.IObject Visit(IStrategy ws) => ws != null ? this.transaction.Instantiate(ws.Id) : null;

        private Database.IObject Visit(long? id) => id != null ? this.transaction.Instantiate(id.Value) : null;

        private Result[] Visit(Data.Result[] ws) =>
            ws?.Select(v => new Result
            {
                Name = v.Name,
                Select = this.Visit(v.Select),
                SelectRef = v.SelectRef,
                Skip = v.Skip,
                Take = v.Take,
                Include = this.Visit(v.Include)
            }).ToArray();

        private Select Visit(Data.Select ws) => ws != null ? new Select { Include = this.Visit(ws.Include), RelationEndType = this.Visit(ws.RelationEndType), Next = this.Visit(ws.Next) } : null;

        private Node[] Visit(IEnumerable<Data.Node> ws) => ws?.Select(this.Visit).ToArray();

        private Node Visit(Data.Node ws) => ws != null ? new Node(this.Visit(ws.RelationEndType), ws.Nodes?.Select(this.Visit).ToArray()) : null;

        private Sort[] Visit(Data.Sort[] ws) => ws?.Select(v =>
        {
            return new Sort
            {
                RoleType = this.Visit(v.RoleType),
                SortDirection = v.SortDirection ?? SortDirection.Ascending
            };
        }).ToArray();

        private ObjectType Visit(Meta.IObjectType ws) => ws != null ? (ObjectType)this.metaPopulation.FindByTag(ws.Tag) : null;

        private Composite Visit(Meta.IComposite ws) => ws != null ? (Composite)this.metaPopulation.FindByTag(ws.Tag) : null;

        private RelationEndType Visit(Meta.IRelationEndType ws) =>
            ws switch
            {
                IAssociationType associationType => this.Visit(associationType),
                IRoleType roleType => this.Visit(roleType),
                null => null,
                _ => throw new ArgumentException("Invalid property type")
            };

        private Database.Meta.AssociationType Visit(IAssociationType ws) => ws != null ? ((RoleType)this.metaPopulation.FindByTag(ws.OperandTag)).AssociationType : null;

        private Database.Meta.RoleType Visit(IRoleType ws) => ws != null ? ((RoleType)this.metaPopulation.FindByTag(ws.OperandTag)) : null;

        private Database.Meta.RoleType[] Visit(IEnumerable<IRoleType> ws) => ws?.Select(v => ((RoleType)this.metaPopulation.FindByTag(v.OperandTag))).ToArray();

        private Database.IObject[] Visit(IEnumerable<IStrategy> ws) => ws != null ? this.transaction.Instantiate(ws.Select(v => v.Id)) : null;

        private IArguments Visit(IDictionary<string, object> ws) => new Arguments(ws);
    }
}
