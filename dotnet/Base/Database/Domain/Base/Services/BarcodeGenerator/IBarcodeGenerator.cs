// <copyright file="IBarcodeGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public interface IBarcodeGenerator
    {
        byte[] Generate(string content, BarcodeType type, int? height = null, int? width = null, int? margin = null, bool? pure = false);
    }
}
