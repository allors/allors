﻿// <copyright file="Security.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Graph;
    using Allors.Database.Meta;

    public partial class Security
    {
        private static readonly Operations[] ReadWriteExecute = [Operations.Read, Operations.Write, Operations.Execute];

        private readonly Dictionary<Guid, Dictionary<OperandType, Permission>> deniablePermissionByOperandTypeByObjectTypeId;
        private readonly Dictionary<Guid, Dictionary<OperandType, Permission>> executePermissionsByObjectTypeId;
        private readonly Dictionary<Guid, Dictionary<OperandType, Permission>> readPermissionsByObjectTypeId;
        private readonly Dictionary<Guid, Dictionary<OperandType, Permission>> writePermissionsByObjectTypeId;

        private readonly Dictionary<Guid, Role> roleById;
        private readonly ITransaction transaction;

        private readonly Dictionary<ObjectType, IObjects> objectsByObjectType;
        private readonly Graph<IObjects> objectsGraph;

        // TODO: Koen
        public Security(ITransaction transaction)
        {
            this.transaction = transaction;

            this.objectsByObjectType = new Dictionary<ObjectType, IObjects>();
            foreach (ObjectType objectType in transaction.Database.MetaPopulation.Composites)
            {
                this.objectsByObjectType[objectType] = objectType.GetObjects(transaction);
            }

            this.objectsGraph = new Graph<IObjects>();

            this.roleById = new Dictionary<Guid, Role>();
            foreach (Role role in transaction.Filter<Role>())
            {
                if (!role.ExistUniqueId)
                {
                    throw new Exception("Role " + role + " has no unique id");
                }

                this.roleById[role.UniqueId] = role;
            }

            this.readPermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<OperandType, Permission>>();
            this.writePermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<OperandType, Permission>>();
            this.executePermissionsByObjectTypeId = new Dictionary<Guid, Dictionary<OperandType, Permission>>();

            this.deniablePermissionByOperandTypeByObjectTypeId = new Dictionary<Guid, Dictionary<OperandType, Permission>>();

            foreach (var permission in transaction.Filter<ReadPermission>().Cast<Permission>().Union(transaction.Filter<WritePermission>()).Union(transaction.Filter<ExecutePermission>()))
            {
                if (!permission.ExistClassPointer || !permission.ExistOperation)
                {
                    throw new Exception("Permission " + permission + " has no concrete class, operand type and/or operation");
                }

                var objectId = permission.ClassPointer;

                if (permission.Operation != Operations.Read)
                {
                    var operandType = permission.OperandType;

                    if (!this.deniablePermissionByOperandTypeByObjectTypeId.TryGetValue(objectId, out var deniablePermissionByOperandTypeId))
                    {
                        deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();
                        this.deniablePermissionByOperandTypeByObjectTypeId[objectId] = deniablePermissionByOperandTypeId;
                    }

                    deniablePermissionByOperandTypeId.Add(operandType, permission);
                }

                Dictionary<Guid, Dictionary<OperandType, Permission>> permissionByOperandTypeByObjectTypeId =
                    permission.Operation switch
                    {
                        Operations.Read => this.readPermissionsByObjectTypeId,
                        Operations.Write => this.writePermissionsByObjectTypeId,
                        Operations.Execute => this.executePermissionsByObjectTypeId,
                        _ => throw new Exception("Unkown operation: " + permission.Operation),
                    };

                if (!permissionByOperandTypeByObjectTypeId.TryGetValue(objectId, out var permissionByOperandType))
                {
                    permissionByOperandType = new Dictionary<OperandType, Permission>();
                    permissionByOperandTypeByObjectTypeId[objectId] = permissionByOperandType;
                }

                if (permission.OperandType == null)
                {
                    permission.Delete();
                }
                else
                {
                    permissionByOperandType.Add(permission.OperandType, permission);
                }
            }
        }

        public void Apply()
        {
            this.OnPreSetup();

            foreach (var objects in this.objectsByObjectType.Values)
            {
                objects.Prepare(this);
            }

            this.objectsGraph.Invoke(objects => objects.Secure(this));

            this.OnPostSetup();

            this.transaction.Derive();
        }

        public void Add(IObjects objects) => this.objectsGraph.Add(objects);

        public void AddDependency(ObjectType dependent, ObjectType dependee) => this.objectsGraph.AddDependency(this.objectsByObjectType[dependent], this.objectsByObjectType[dependee]);

        public void Grant(Guid roleId, ObjectType objectType, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
            {
                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                    case Operations.Read:
                        this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Write:
                        this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Execute:
                        this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    default:
                        throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null)
                    {
                        foreach (var dictionaryEntry in permissionByOperandType)
                        {
                            role.AddPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Grant(Guid roleId, ObjectType objectType, OperandType operandType, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
            {
                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                    case Operations.Read:
                        this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Write:
                        this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Execute:
                        this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    default:
                        throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null && permissionByOperandType.TryGetValue(operandType, out var permission))
                    {
                        role.AddPermission(permission);
                    }
                }
            }
        }

        public void GrantAdministrator(ObjectType objectType, params Operations[] operations) => this.Grant(Role.AdministratorId, objectType, operations);

        public void GrantCreator(ObjectType objectType, params Operations[] operations) => this.Grant(Role.CreatorId, objectType, operations);

        public void GrantExcept(Guid roleId, ObjectType objectType, ICollection<OperandType> excepts, params Operations[] operations)
        {
            if (this.roleById.TryGetValue(roleId, out var role))
            {
                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                    case Operations.Read:
                        this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Write:
                        this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    case Operations.Execute:
                        this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                        break;

                    default:
                        throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null)
                    {
                        foreach (var dictionaryEntry in permissionByOperandType.Where(v => !excepts.Contains(v.Key)))
                        {
                            role.AddPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void GrantGuest(ObjectType objectType, params Operations[] operations) => this.Grant(Role.GuestId, objectType, operations);

        public void GrantGuestCreator(ObjectType objectType, params Operations[] operations) => this.Grant(Role.GuestCreatorId, objectType, operations);

        public void GrantOwner(ObjectType objectType, params Operations[] operations) => this.Grant(Role.OwnerId, objectType, operations);

        private void BaseOnPreSetup()
        {
            foreach (Role role in this.transaction.Filter<Role>())
            {
                role.RemovePermissions();
                role.RemoveRevocations();
            }

            foreach (var revocation in this.transaction.Filter<Revocation>().Where(v => v.ExistObjectStatesWhereObjectRevocation))
            {
                revocation.RemoveDeniedPermissions();
            }
        }

        private void BaseOnPostSetup()
        {
        }

        public void Deny(ObjectType objectType, ObjectState objectState, params Operations[] operations)
        {
            if (objectState != null)
            {
                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                        case Operations.Read:
                            this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Write:
                            this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Execute:
                            this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        default:
                            throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null)
                    {
                        foreach (var dictionaryEntry in permissionByOperandType)
                        {
                            objectState.ObjectRevocation.AddDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Deny(ObjectType objectType, ObjectState objectState, params OperandType[] operandTypes) => this.Deny(objectType, objectState, (IEnumerable<OperandType>)operandTypes);

        public void Deny(ObjectType objectType, ObjectState objectState, IEnumerable<OperandType> operandTypes)
        {
            if (objectState != null)
            {
                if (this.deniablePermissionByOperandTypeByObjectTypeId.TryGetValue(objectType.Id, out var deniablePermissionByOperandTypeId))
                {
                    foreach (var operandType in operandTypes)
                    {
                        if (deniablePermissionByOperandTypeId.TryGetValue(operandType, out var permission))
                        {
                            objectState.ObjectRevocation.AddDeniedPermission(permission);
                        }
                    }
                }
            }
        }

        public void DenyExcept(ObjectType objectType, ObjectState objectState, IEnumerable<OperandType> excepts, params Operations[] operations)
        {
            if (objectState != null)
            {

                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                        case Operations.Read:
                            this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Write:
                            this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Execute:
                            this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        default:
                            throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null)
                    {
                        foreach (var dictionaryEntry in permissionByOperandType.Where(v => !excepts.Contains(v.Key)))
                        {
                            objectState.ObjectRevocation.AddDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }

        public void Grant(ObjectType objectType, ObjectState objectState, IEnumerable<OperandType> grants, params Operations[] operations)
        {
            if (objectState != null)
            {
                foreach (var operation in operations ?? ReadWriteExecute)
                {
                    Dictionary<OperandType, Permission> permissionByOperandType;
                    switch (operation)
                    {
                        case Operations.Read:
                            this.readPermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Write:
                            this.writePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        case Operations.Execute:
                            this.executePermissionsByObjectTypeId.TryGetValue(objectType.Id, out permissionByOperandType);
                            break;

                        default:
                            throw new Exception("Unkown operation: " + operations);
                    }

                    if (permissionByOperandType != null)
                    {
                        foreach (var dictionaryEntry in permissionByOperandType.Where(v => grants.Contains(v.Key)))
                        {
                            objectState.ObjectRevocation.RemoveDeniedPermission(dictionaryEntry.Value);
                        }
                    }
                }
            }
        }
    }
}
