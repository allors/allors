﻿// <copyright file="Users.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;

    public partial class Users
    {
        public void SavePasswords(XmlWriter writer)
        {
            var usersWithPassword = this.Transaction.Filter<User>();
            usersWithPassword.AddExists(this.Meta.UserPasswordHash);

            var records = new List<Credentials.Record>();
            foreach (User user in usersWithPassword)
            {
                records.Add(new Credentials.Record
                {
                    UserName = user.UserName,
                    PasswordHash = user.UserPasswordHash,
                });
            }

            var credentials = new Credentials { Records = [.. records] };
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Credentials));
            xmlSerializer.Serialize(writer, credentials);
        }

        public void LoadPasswords(XmlReader reader)
        {
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Credentials));
            var credentials = (Credentials)xmlSerializer.Deserialize(reader);
            foreach (var credential in credentials.Records)
            {
                var user = this.Transaction.Filter<User>().FindBy(this.Meta.UserName, credential.UserName);
                if (user != null)
                {
                    user.UserPasswordHash = credential.PasswordHash;
                }
            }
        }

        [XmlRoot("Credentials")]
        public class Credentials
        {
            [XmlElement("Credential", typeof(Record))]
            public Record[] Records { get; set; }

            public class Record
            {
                public string UserName { get; set; }

                public string PasswordHash { get; set; }
            }
        }
    }
}
