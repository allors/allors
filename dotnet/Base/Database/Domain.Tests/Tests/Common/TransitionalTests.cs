﻿// <copyright file="TransitionalTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Domain;
    using Xunit;

    public class TransitionalTests : DomainTest, IClassFixture<Fixture>
    {
        public TransitionalTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void SingleObjectState()
        {
            var orderStates = this.Transaction.Scoped<OrderStateByKey>();

            var initial = orderStates.Initial;
            var confirmed = orderStates.Confirmed;
            var cancelled = orderStates.Cancelled;

            var order = this.Transaction.Build<Order>();

            this.Transaction.Derive();

            Assert.False(order.ExistOrderState);
            Assert.False(order.ExistLastOrderState);
            Assert.False(order.ExistPreviousOrderState);

            Assert.False(order.ExistObjectStates);
            Assert.False(order.ExistLastObjectStates);
            Assert.False(order.ExistPreviousObjectStates);

            order.OrderState = initial;

            this.Transaction.Derive();

            Assert.Equal(initial, order.OrderState);
            Assert.Equal(initial, order.LastOrderState);
            Assert.False(order.ExistPreviousOrderState);

            Assert.Single(order.ObjectStates);
            Assert.Contains(initial, order.ObjectStates);
            Assert.Single(order.LastObjectStates);
            Assert.Contains(initial, order.LastObjectStates);
            Assert.False(order.ExistPreviousObjectStates);

            order.OrderState = confirmed;

            this.Transaction.Derive();

            Assert.Equal(confirmed, order.OrderState);
            Assert.Equal(confirmed, order.LastOrderState);
            Assert.Equal(initial, order.PreviousOrderState);

            Assert.Single(order.ObjectStates);
            Assert.Contains(confirmed, order.ObjectStates);
            Assert.Single(order.LastObjectStates);
            Assert.Contains(confirmed, order.LastObjectStates);
            Assert.Single(order.PreviousObjectStates);
            Assert.Contains(initial, order.PreviousObjectStates);
        }

        [Fact]
        public void MultipleObjectStates()
        {
            var orderStates = this.Transaction.Scoped<OrderStateByKey>();
            var shipmentStates = this.Transaction.Scoped<ShipmentStateByKey>();

            var initial = orderStates.Initial;
            var confirmed = orderStates.Confirmed;
            var cancelled = orderStates.Cancelled;

            var notShipped = shipmentStates.NotShipped;
            var partiallyShipped = shipmentStates.PartiallyShipped;
            var shipped = shipmentStates.Shipped;

            var order = this.Transaction.Build<Order>();

            order.OrderState = initial;

            this.Transaction.Derive();

            order.OrderState = confirmed;

            this.Transaction.Derive();

            order.ShipmentState = notShipped;

            this.Transaction.Derive();

            Assert.Equal(notShipped, order.ShipmentState);
            Assert.Equal(notShipped, order.LastShipmentState);
            Assert.False(order.ExistPreviousShipmentState);

            Assert.Equal(2, order.ObjectStates.Count());
            Assert.Contains(confirmed, order.ObjectStates);
            Assert.Contains(notShipped, order.ObjectStates);
            Assert.Equal(2, order.LastObjectStates.Count());
            Assert.Contains(confirmed, order.LastObjectStates);
            Assert.Contains(notShipped, order.LastObjectStates);
            Assert.Single(order.PreviousObjectStates);
            Assert.Contains(initial, order.PreviousObjectStates);

            order.ShipmentState = partiallyShipped;

            this.Transaction.Derive();

            Assert.Equal(2, order.ObjectStates.Count());
            Assert.Contains(confirmed, order.ObjectStates);
            Assert.Contains(partiallyShipped, order.ObjectStates);
            Assert.Equal(2, order.LastObjectStates.Count());
            Assert.Contains(confirmed, order.LastObjectStates);
            Assert.Contains(partiallyShipped, order.LastObjectStates);
            Assert.Equal(2, order.PreviousObjectStates.Count());
            Assert.Contains(initial, order.PreviousObjectStates);
            Assert.Contains(notShipped, order.PreviousObjectStates);
        }
    }
}
