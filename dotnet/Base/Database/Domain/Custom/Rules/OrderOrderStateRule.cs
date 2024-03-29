﻿// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using Database.Derivations;
    using Derivations.Rules;

    // TODO: Martien
    public class OrderOrderStateRule : Rule<Order, OrderIndex>
    {
        public OrderOrderStateRule(IMetaIndex m) : base(m, m.Order, new Guid("C9895CF4-98B2-4023-A3EA-582107C7D80D")) =>
            this.Patterns =
            [
                this.Builder.Pattern(v=>v.OrderState),
            ];

        public override void Derive(ICycle cycle, IEnumerable<Order> matches)
        {
            foreach (var @this in matches)
            {
                if (@this.ExistAmount && @this.Amount == -1)
                {
                    var cache = @this.Transaction().Scoped<OrderStateByKey>();
                    @this.OrderState = cache.Cancelled;
                }
            }
        }
    }
}
