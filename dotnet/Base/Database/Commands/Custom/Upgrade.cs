﻿// <copyright file="Upgrade.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Upgrade
    {
        private readonly HashSet<Guid> excludedObjectTypes = new HashSet<Guid>
        {
        };

        private readonly HashSet<Guid> excludedRelationTypes = new HashSet<Guid>
        {
        };

        private readonly HashSet<Guid> movedRelationTypes = new HashSet<Guid>
        {
        };

        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "Backup file")]
        public string FileName { get; set; } = "population.xml";

        public int OnExecute(CommandLineApplication app)
        {
            var fileInfo = new FileInfo(this.FileName);

            this.Logger.Info("Begin");

            var nonRestoredObjectTypeIds = new HashSet<Guid>();
            var nonRestoredRelationTypeIds = new HashSet<Guid>();

            var nonRestoredObjects = new HashSet<long>();

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Parent.Database.ObjectNotRestored += (sender, args) =>
                {
                    if (!this.excludedObjectTypes.Contains(args.ObjectTypeId))
                    {
                        nonRestoredObjectTypeIds.Add(args.ObjectTypeId);
                    }
                    else
                    {
                        var id = args.ObjectId;
                        nonRestoredObjects.Add(id);
                    }
                };

                this.Parent.Database.RelationNotRestored += (sender, args) =>
                {
                    if (!this.excludedRelationTypes.Contains(args.RelationTypeId))
                    {
                        if (!nonRestoredObjects.Contains(args.AssociationId))
                        {
                            nonRestoredRelationTypeIds.Add(args.RelationTypeId);
                        }
                    }
                };

                this.Logger.Info("Restoring {file}", fileInfo.FullName);
                this.Parent.Database.Restore(reader);
            }

            if (nonRestoredObjectTypeIds.Count > 0)
            {
                var notRestored = nonRestoredObjectTypeIds
                    .Aggregate("Could not restore following ObjectTypeIds: ", (current, objectTypeId) => current + "- " + objectTypeId);

                this.Logger.Error(notRestored);
                return 1;
            }

            if (nonRestoredRelationTypeIds.Count > 0)
            {
                var notRestored = nonRestoredRelationTypeIds
                    .Aggregate("Could not restore following RelationTypeIds: ", (current, relationTypeId) => current + "- " + relationTypeId);

                this.Logger.Error(notRestored);
                return 1;
            }

            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Parent.Database.Services.Get<IPermissions>().Sync(transaction);

                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();
                transaction.Commit();

                new Security(transaction).Apply();

                transaction.Commit();
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
