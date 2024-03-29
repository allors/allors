// <copyright file="Places.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Places
    {
        public IExtent<Place> ExtentByPostalCode()
        {
            var places = this.Transaction.Filter<Place>();
            places.AddSort(this.M.Place.PostalCode);

            return places;
        }
    }
}
