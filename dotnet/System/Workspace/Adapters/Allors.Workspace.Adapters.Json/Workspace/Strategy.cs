﻿// <copyright file="Object.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Json
{
    using Allors.Protocol.Json.Api.Push;
    using Allors.Shared.Ranges;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public sealed class Strategy : Adapters.Strategy
    {
        internal Strategy(Adapters.Workspace workspace, IClass @class, long id) : base(workspace, @class, id)
        {
        }

        internal PushRequestNewObject PushNew() => new PushRequestNewObject
        {
            w = this.Id,
            t = this.Class.Tag,
            r = this.PushRoles()
        };

        internal PushRequestObject PushExisting() => new PushRequestObject
        {
            d = this.Id,
            v = this.Version,
            r = this.PushRoles()
        };

        private PushRequestRole[] PushRoles()
        {
            if (this.ChangesByRoleType?.Count > 0)
            {
                var database = this.Workspace.Connection;
                var roles = new List<PushRequestRole>();

                foreach (var keyValuePair in this.ChangesByRoleType)
                {
                    var roleType = keyValuePair.Key;
                    var changes = keyValuePair.Value;

                    var pushRequestRole = new PushRequestRole { t = roleType.Tag };

                    if (roleType.ObjectType.IsUnit)
                    {
                        var setUnit = (SetUnitChange)changes[0];
                        pushRequestRole.u = ((Connection)database).UnitConvert.ToJson(setUnit.Role);
                    }
                    else if (roleType.IsOne)
                    {

                        var setComposite = (SetCompositeChange)changes[0];
                        if (setComposite.Dependee != null)
                        {
                            continue;
                        }

                        pushRequestRole.c = setComposite.Role?.Id;
                    }
                    else
                    {
                        var addIds = changes.OfType<AddCompositeChange>().Where(v => v.Dependee == null).Select(v => v.Role.Id).ToArray();
                        var removeIds = changes.OfType<RemoveCompositeChange>().Where(v => v.Dependee == null).Select(v => v.Role.Id).ToArray();

                        if (addIds.Length == 0 && removeIds.Length == 0)
                        {
                            continue;
                        }

                        var addRange = ValueRange<long>.Load(addIds);

                        if (!this.ExistRecord)
                        {
                            pushRequestRole.a = addRange.Save();
                        }
                        else
                        {
                            if (!addRange.IsEmpty)
                            {
                                pushRequestRole.a = addRange.Save();
                            }

                            var removeRange = ValueRange<long>.Load(removeIds);
                            if (!removeRange.IsEmpty)
                            {
                                pushRequestRole.r = removeRange.Save();
                            }
                        }
                    }

                    roles.Add(pushRequestRole);
                }

                return [.. roles];
            }

            return null;
        }
    }
}
