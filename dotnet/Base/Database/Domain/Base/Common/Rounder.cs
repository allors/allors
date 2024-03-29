// <copyright file="Rounder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public static partial class Rounder
    {
        public static decimal RoundDecimal(decimal value, int digits) => Math.Round(value, digits, MidpointRounding.AwayFromZero);
    }
}
