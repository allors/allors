﻿// <copyright file="DerivationRelation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration.Derivations.Default
{
    using System.Text;
    using Allors.Database.Derivations;
    using Allors.Database.Meta;

    public class DerivationRelation : IDerivationRelation
    {
        public DerivationRelation(IObject association, RoleType roleType)
        {
            this.Association = association;
            this.RoleType = roleType;
        }

        public DerivationRelation(IObject role, AssociationType associationType)
        {
            this.Role = role;
            this.RoleType = associationType.RoleType;
        }

        public RoleType RoleType { get; }

        public IObject Association { get; }

        public IObject Role { get; }

        public static DerivationRelation[] Create(IObject association, params RoleType[] roleTypes)
        {
            var derivationRoles = new DerivationRelation[roleTypes.Length];
            for (var i = 0; i < derivationRoles.Length; i++)
            {
                derivationRoles[i] = new DerivationRelation(association, roleTypes[i]);
            }

            return derivationRoles;
        }

        public static DerivationRelation[] Create(IObject role, params AssociationType[] associationTypes)
        {
            var derivationRoles = new DerivationRelation[associationTypes.Length];
            for (var i = 0; i < derivationRoles.Length; i++)
            {
                derivationRoles[i] = new DerivationRelation(role, associationTypes[i]);
            }

            return derivationRoles;
        }

        public static string ToString(IDerivationRelation[] relations)
        {
            var stringBuilder = new StringBuilder();
            var first = true;
            foreach (var relation in relations)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(relation);
            }

            return stringBuilder.ToString();
        }

        public override string ToString()
        {
            if (this.Association != null)
            {
                if (this.RoleType != null)
                {
                    return this.Association.Strategy.Class.SingularName + "." + this.RoleType.Name;
                }

                return this.Association.Strategy.Class.SingularName;
            }

            if (this.RoleType != null)
            {
                return this.Role.Strategy.Class.SingularName + "." + this.RoleType.AssociationType.Name;
            }

            return this.Role.Strategy.Class.SingularName;
        }
    }
}
