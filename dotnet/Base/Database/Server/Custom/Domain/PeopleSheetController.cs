﻿// <copyright file="PeopleSheetController.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Server.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Services;
    using Domain;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Database;
    using Meta;
    using Protocol.Json;


    public class PeopleSheetController : Controller
    {
        public PeopleSheetController(ITransactionService sessionService, IWorkspaceService workspaceService)
        {
            this.WorkspaceService = workspaceService;
            this.Transaction = sessionService.Transaction;
            this.TreeCache = this.Transaction.Database.Services.Get<ITreeCache>();
        }

        private ITransaction Transaction { get; }

        public IWorkspaceService WorkspaceService { get; }

        public ITreeCache TreeCache { get; }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Pull(CancellationToken cancellationToken)
        {
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var api = new Api(this.Transaction, this.WorkspaceService.Name, cancellationToken);
            var response = api.CreatePullResponseBuilder();
            response.AddCollection("people", m.Person.Composite, this.Transaction.Filter<Person>());
            return this.Ok(response.Build());
        }
    }
}
