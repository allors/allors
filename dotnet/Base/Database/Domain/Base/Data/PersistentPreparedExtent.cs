﻿// <copyright file="PersistentPreparedExtent.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.IO;
    using System.Text;
    using Allors.Database.Data;
    using Allors.Protocol.Json.SystemText;
    using Protocol.Json;


    public partial class PersistentPreparedExtent
    {
        public IExtent Extent
        {
            get
            {
                using TextReader reader = new StringReader(this.Content);
                var protocolExtent = (Allors.Protocol.Json.Data.Extent)XmlSerializer.Deserialize(reader);
                return protocolExtent.FromJson(this.Transaction(), new UnitConvert());
            }

            set
            {
                var stringBuilder = new StringBuilder();
                using TextWriter writer = new StringWriter(stringBuilder);
                XmlSerializer.Serialize(writer, value.ToJson(new UnitConvert()));
                this.Content = stringBuilder.ToString();
            }
        }

        private static System.Xml.Serialization.XmlSerializer XmlSerializer => new System.Xml.Serialization.XmlSerializer(typeof(Allors.Protocol.Json.Data.Extent));
    }
}
