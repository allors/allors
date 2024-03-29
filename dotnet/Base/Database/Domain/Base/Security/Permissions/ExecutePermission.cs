﻿// <copyright file="Permission.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using System.Text;
    using Allors.Database.Security;
    using Allors.Database.Meta;

    public partial class ExecutePermission : IExecutePermission
    {
        Class IPermission.Class => this.Class;
        public Class Class
        {
            get => (Class)this.Transaction().Database.MetaPopulation.FindById(this.ClassPointer);

            set
            {
                if (value == null)
                {
                    this.RemoveClassPointer();
                }
                else
                {
                    this.ClassPointer = value.Id;
                }
            }
        }

        public bool ExistClass => this.Class != null;

        public bool ExistOperandType => this.ExistMethodTypePointer;

        public bool ExistOperation => true;

        public OperandType OperandType => this.MethodType;

        MethodType IExecutePermission.MethodType => this.MethodType;
        public MethodType MethodType
        {
            get => (MethodType)this.Transaction().Database.MetaPopulation.FindById(this.MethodTypePointer);

            set
            {
                if (value == null)
                {
                    this.RemoveMethodTypePointer();
                }
                else
                {
                    this.MethodTypePointer = value.Id;
                }
            }
        }

        public Operations Operation => Operations.Execute;

        public bool InWorkspace(string workspaceName) => this.Class.WorkspaceNames.Contains(workspaceName) && this.MethodType.WorkspaceNames.Contains(workspaceName);

        public override string ToString()
        {
            var toString = new StringBuilder();
            if (this.ExistOperation)
            {
                var operation = this.Operation;
                toString.Append(operation);
            }
            else
            {
                toString.Append("[missing operation]");
            }

            toString.Append(" for ");

            toString.Append(this.ExistOperandType ? this.OperandType.GetType().Name + ":" + this.OperandType : "[missing operand]");

            return toString.ToString();
        }
    }
}
