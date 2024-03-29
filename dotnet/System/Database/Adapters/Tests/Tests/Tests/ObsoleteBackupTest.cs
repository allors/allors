﻿// <copyright file="ObsoleteBackupTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Allors.Database.Domain;
using Allors.Database.Meta;
using Xunit;

public abstract class ObsoleteBackupTest : IDisposable
{
    protected static readonly bool[] TrueFalse = [true, false];
    private static readonly string GuidString = Guid.NewGuid().ToString();

    protected virtual bool EmptyStringIsNull => false;

    protected abstract IProfile Profile { get; }

    protected IDatabase Population => this.Profile.Database;

    protected ITransaction Transaction => this.Profile.Transaction;

    protected Action[] Markers => this.Profile.Markers;

    protected Action[] Inits => this.Profile.Inits;

    public abstract void Dispose();

    [Fact]
    public void DifferentVersion()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var otherPopulation = this.CreatePopulation();
            using (var otherTransaction = otherPopulation.CreateTransaction())
            {
                this.Populate(otherTransaction);
                otherTransaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    otherPopulation.Backup(writer);
                }

                var xml = stringWriter.ToString();
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                var populationElement = (XmlElement)xmlDocument.SelectSingleNode("//population");
                populationElement.SetAttribute("version", "0");
                xml = xmlDocument.OuterXml;

                try
                {
                    using (var stringReader = new StringReader(xml))
                    {
                        using (var reader = XmlReader.Create(stringReader))
                        {
                            this.Population.Restore(reader);
                        }
                    }

                    Assert.True(false); // Fail
                }
                catch (ArgumentException)
                {
                }

                populationElement.SetAttribute("version", "2");
                xml = xmlDocument.OuterXml;

                try
                {
                    using (var stringReader = new StringReader(xml))
                    {
                        using (var reader = XmlReader.Create(stringReader))
                        {
                            this.Population.Restore(reader);
                        }
                    }

                    Assert.True(false); // Fail
                }
                catch (ArgumentException)
                {
                }

                populationElement.SetAttribute("version", "a");
                xml = xmlDocument.OuterXml;

                var exception = false;
                try
                {
                    using (var stringReader = new StringReader(xml))
                    {
                        using (var reader = XmlReader.Create(stringReader))
                        {
                            this.Population.Restore(reader);
                        }
                    }
                }
                catch (Exception)
                {
                    exception = true;
                }

                Assert.True(exception);

                populationElement.SetAttribute("version", string.Empty);
                xml = xmlDocument.OuterXml;

                var exceptionThrown = false;
                try
                {
                    using (var stringReader = new StringReader(xml))
                    {
                        using (var reader = XmlReader.Create(stringReader))
                        {
                            this.Population.Restore(reader);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    exceptionThrown = true;
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }

                Assert.True(exceptionThrown);
            }
        }
    }

    [Fact]
    public void Restore()
    {
        foreach (var indentation in TrueFalse)
        {
            foreach (var init in this.Inits)
            {
                init();
                var m = this.Transaction.Database.Services.Get<IMetaIndex>();

                var otherPopulation = this.CreatePopulation();
                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    this.Populate(otherTransaction);
                    otherTransaction.Commit();

                    var stringWriter = new StringWriter();
                    var xmlWriterSettings = new XmlWriterSettings { Indent = indentation };
                    using (var writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
                    {
                        otherPopulation.Backup(writer);
                    }

                    var xml = stringWriter.ToString();
                    //File.WriteAllText(@"c:\temp\population.xml", xml);
                    // Console.Out.WriteLine(xml);
                    var stringReader = new StringReader(xml);
                    using (var reader = XmlReader.Create(stringReader))
                    {
                        this.Population.Restore(reader);
                    }

                    using (var transaction = this.Population.CreateTransaction())
                    {
                        var x = (C1)transaction.Instantiate(1);
                        var str = x.C1AllorsString;

                        this.AssertPopulation(transaction);
                    }
                }
            }
        }
    }

    [Fact]
    public void RestoreVersions()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var otherPopulation = this.CreatePopulation();
            using (var otherTransaction = otherPopulation.CreateTransaction())
            {
                // Initial
                var otherC1 = otherTransaction.Build<C1>();

                otherTransaction.Commit();

                var initialObjectVersion = otherC1.Strategy.ObjectVersion;

                var xml = DoBackup(otherPopulation);
                DoRestore(this.Population, xml);

                using (var transaction = this.Population.CreateTransaction())
                {
                    var c1 = transaction.Instantiate(otherC1.Id);

                    Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                }

                // Change
                otherC1.C1AllorsString = "Changed";

                otherTransaction.Commit();

                var changedObjectVersion = otherC1.Strategy.ObjectVersion;

                xml = DoBackup(otherPopulation);
                DoRestore(this.Population, xml);

                using (var transaction = this.Population.CreateTransaction())
                {
                    var c1 = transaction.Instantiate(otherC1.Id);

                    Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                    Assert.NotEqual(initialObjectVersion, c1.Strategy.ObjectVersion);
                }

                // Change again
                otherC1.C1AllorsString = "Changed again";

                otherTransaction.Commit();

                xml = DoBackup(otherPopulation);
                DoRestore(this.Population, xml);

                using (var transaction = this.Population.CreateTransaction())
                {
                    var c1 = transaction.Instantiate(otherC1.Id);

                    Assert.Equal(otherC1.Strategy.ObjectVersion, c1.Strategy.ObjectVersion);
                    Assert.NotEqual(initialObjectVersion, c1.Strategy.ObjectVersion);
                    Assert.NotEqual(changedObjectVersion, c1.Strategy.ObjectVersion);
                }
            }
        }
    }

    [Fact]
    public void RestoreRollback()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var otherPopulation = this.CreatePopulation();
            using (var otherTransaction = otherPopulation.CreateTransaction())
            {
                this.Populate(otherTransaction);
                otherTransaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    otherPopulation.Backup(writer);
                }

                var xml = stringWriter.ToString();
                // Console.Out.WriteLine(xml);
                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Restore(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    transaction.Rollback();

                    this.AssertPopulation(transaction);
                }
            }
        }
    }

    [Fact]
    public void RestoreDifferentMode()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var population = this.CreatePopulation();
            var transaction = population.CreateTransaction();

            try
            {
                this.Populate(transaction);
                transaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    population.Backup(writer);
                }

                Dump(population);

                var stringReader = new StringReader(stringWriter.ToString());
                var reader = XmlReader.Create(stringReader);

                try
                {
                    this.Population.Restore(reader);
                    Assert.True(false); // Fail
                }
                catch
                {
                }
            }
            finally
            {
                transaction.Commit();
            }
        }
    }

    [Fact]
    public void RestoreDifferentCultureInfos()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var writeCultureInfo = new CultureInfo("en-US");
            var readCultureInfo = new CultureInfo("en-GB");

            CultureInfo.CurrentCulture = writeCultureInfo;
            CultureInfo.CurrentUICulture = writeCultureInfo;

            var restorePopulation = this.CreatePopulation();
            var restoreTransaction = restorePopulation.CreateTransaction();
            this.Populate(restoreTransaction);

            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                restoreTransaction.Database.Backup(writer);
            }

            CultureInfo.CurrentCulture = readCultureInfo;
            CultureInfo.CurrentUICulture = readCultureInfo;

            var xml = stringWriter.ToString();
            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            using (var transaction = this.Population.CreateTransaction())
            {
                this.AssertPopulation(transaction);
            }

            restoreTransaction.Rollback();
        }
    }

    [Fact]
    public void RestoreDifferentVersion()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var population = this.CreatePopulation();
            var transaction = population.CreateTransaction();

            try
            {
                this.Populate(transaction);
                transaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    population.Backup(writer);
                }

                Dump(population);

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(stringWriter.ToString());
                var allorsElement = (XmlElement)xmlDocument.SelectSingleNode("/allors");
                allorsElement.SetAttribute("version", "0.9");

                var stringReader = new StringReader(xmlDocument.InnerText);
                var reader = XmlReader.Create(stringReader);

                try
                {
                    this.Population.Restore(reader);
                    Assert.True(false); // Fail
                }
                catch
                {
                }
            }
            finally
            {
                transaction.Commit();
            }
        }
    }

    [Fact]
    public void RestoreSpecial()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var backupPopulation = this.CreatePopulation();
            var backupTransaction = backupPopulation.CreateTransaction();

            try
            {
                this.c1A = backupTransaction.Build<C1>();
                this.c1A.C1AllorsString = "> <";
                this.c1A.I12AllorsString = "< >";
                this.c1A.I1AllorsString = "& &&";
                this.c1A.S1AllorsString = "' \" ''";

                this.c1Empty = backupTransaction.Build<C1>();

                backupTransaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    backupPopulation.Backup(writer);
                }

                // writer = XmlWriter.Create(@"population.xml", Encoding.UTF8);
                // backupTransaction.Population.Backup(writer);
                // writer.Close();
                var stringReader = new StringReader(stringWriter.ToString());
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Restore(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    var copyValues = (C1)transaction.Instantiate(this.c1A.Strategy.ObjectId);

                    Assert.Equal(this.c1A.C1AllorsString, copyValues.C1AllorsString);
                    Assert.Equal(this.c1A.I12AllorsString, copyValues.I12AllorsString);
                    Assert.Equal(this.c1A.I1AllorsString, copyValues.I1AllorsString);
                    Assert.Equal(this.c1A.S1AllorsString, copyValues.S1AllorsString);

                    var c1EmptyRestored = (C1)transaction.Instantiate(this.c1Empty.Strategy.ObjectId);
                    Assert.NotNull(c1EmptyRestored);
                }
            }
            finally
            {
                backupTransaction.Rollback();
            }
        }
    }

    [Fact]
    public void Backup()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            using (var transaction = this.Population.CreateTransaction())
            {
                this.Populate(transaction);

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    this.Population.Backup(writer);
                }

                //using (var writer = XmlWriter.Create("population.xml"))
                //{
                //    this.Population.Backup(writer);
                //    writer.Close();
                //}

                var xml = stringWriter.ToString();

                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader))
                {
                    var backupPopulation = this.CreatePopulation();
                    backupPopulation.Restore(reader);

                    using (var backupTransaction = backupPopulation.CreateTransaction())
                    {
                        this.AssertPopulation(backupTransaction);
                    }
                }
            }
        }
    }

    [Fact]
    public void BackupVersions()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            using (var transaction = this.Population.CreateTransaction())
            {
                // Initial
                var c1 = transaction.Build<C1>();

                transaction.Commit();

                var initialObjectVersion = c1.Strategy.ObjectVersion;

                var xml = DoBackup(this.Population);

                var otherPopulation = this.CreatePopulation();
                DoRestore(otherPopulation, xml);

                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    var otherC1 = otherTransaction.Instantiate(c1.Id);

                    Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                }

                // Change
                c1.C1AllorsString = "Changed";

                transaction.Commit();

                var changedObjectVersion = c1.Strategy.ObjectVersion;

                xml = DoBackup(this.Population);

                otherPopulation = this.CreatePopulation();
                DoRestore(otherPopulation, xml);

                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    var otherC1 = otherTransaction.Instantiate(c1.Id);

                    Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                    Assert.NotEqual(initialObjectVersion, otherC1.Strategy.ObjectVersion);
                }

                // Change again
                c1.C1AllorsString = "Changed again";

                transaction.Commit();

                xml = DoBackup(this.Population);

                otherPopulation = this.CreatePopulation();
                DoRestore(otherPopulation, xml);

                using (var otherTransaction = otherPopulation.CreateTransaction())
                {
                    var otherC1 = otherTransaction.Instantiate(c1.Id);

                    Assert.Equal(c1.Strategy.ObjectVersion, otherC1.Strategy.ObjectVersion);
                    Assert.NotEqual(initialObjectVersion, otherC1.Strategy.ObjectVersion);
                    Assert.NotEqual(changedObjectVersion, otherC1.Strategy.ObjectVersion);
                }
            }
        }
    }

    [Fact]
    public void BackupDifferentCultureInfos()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var writeCultureInfo = new CultureInfo("en-US");
            var readCultureInfo = new CultureInfo("en-GB");

            CultureInfo.CurrentCulture = writeCultureInfo;
            CultureInfo.CurrentUICulture = writeCultureInfo;

            using (var transaction = this.CreatePopulation().CreateTransaction())
            {
                this.Populate(transaction);

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    transaction.Database.Backup(writer);
                }

                CultureInfo.CurrentCulture = readCultureInfo;
                CultureInfo.CurrentUICulture = readCultureInfo;

                var stringReader = new StringReader(stringWriter.ToString());
                using (var reader = XmlReader.Create(stringReader))
                {
                    var backupPopulation = this.CreatePopulation();
                    backupPopulation.Restore(reader);

                    var backupTransaction = backupPopulation.CreateTransaction();

                    this.AssertPopulation(backupTransaction);

                    backupTransaction.Rollback();
                }
            }
        }
    }

    [Fact]
    public void RestoresBinary()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var otherPopulation = this.CreatePopulation();
            var otherTransaction = otherPopulation.CreateTransaction();

            try
            {
                this.c1A = otherTransaction.Build<C1>();
                this.c1B = otherTransaction.Build<C1>();
                this.c1C = otherTransaction.Build<C1>();

                this.c1A.C1AllorsBinary = Array.Empty<byte>();
                this.c1B.C1AllorsBinary = [1, 2, 3, 4];
                this.c1C.C1AllorsBinary = null;

                otherTransaction.Commit();

                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    otherPopulation.Backup(writer);
                }

                var xml = stringWriter.ToString();

                var stringReader = new StringReader(stringWriter.ToString());
                using (var reader = XmlReader.Create(stringReader))
                {
                    this.Population.Restore(reader);
                }

                using (var transaction = this.Population.CreateTransaction())
                {
                    var c1ACopy = (C1)transaction.Instantiate(this.c1A.Strategy.ObjectId);
                    var c1BCopy = (C1)transaction.Instantiate(this.c1B.Strategy.ObjectId);
                    var c1CCopy = (C1)transaction.Instantiate(this.c1C.Strategy.ObjectId);

                    Assert.Equal(this.c1A.C1AllorsBinary, c1ACopy.C1AllorsBinary);
                    Assert.Equal(this.c1B.C1AllorsBinary, c1BCopy.C1AllorsBinary);
                    Assert.Equal(this.c1C.C1AllorsBinary, c1CCopy.C1AllorsBinary);
                }
            }
            finally
            {
                otherTransaction.Commit();
            }
        }
    }

    [Fact]
    public void EnsureObjectId()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">2:1</ot>
      </database>
    </objects>
  </population>
</allors>";
            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);
                this.c2A = (C2)transaction.Instantiate(2);

                Assert.Equal(2, this.c1A.Strategy.ObjectVersion);
                Assert.Equal(2, this.c2A.Strategy.ObjectVersion);
            }
        }
    }

    [Fact]
    public void CantRestoreObjects()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0</ot>
        <ot i=""71000000000000000000000000000000"">3:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">2:0</ot>
      </database>
    </objects>
  </population>
</allors>";
            var notRestoredEventArgs = new List<ObjectNotRestoredEventArgs>();
            this.Population.ObjectNotRestored += (o, args) =>
                notRestoredEventArgs.Add(args);

            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            Assert.Single(notRestoredEventArgs);
            var notRestoredEventArg = notRestoredEventArgs.First();
            Assert.Equal(3, notRestoredEventArg.ObjectId);
            Assert.Equal(new Guid("71000000000000000000000000000000"), notRestoredEventArg.ObjectTypeId);

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);
                this.c2A = (C2)transaction.Instantiate(2);

                Assert.NotNull(this.c1A);
                Assert.NotNull(this.c2A);
            }
        }
    }

    [Fact]
    public void CantRestoreUnitRelation()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i=""207138608abd4d718ccc2b4d1b88bce3"">
          <r a=""1"">QSBTdHJpbmc=</r>
        </rtu>
        <rtu i=""40000000000000000000000000000000"">
          <r a=""2"">T29wcw==</r>
        </rtu>
        <rtu i=""b4ee673fbba04e249cda3cf993c79a0a"">
          <r a=""3"">true</r>
        </rtu>
        <rtu i=""cef13620b7d74bfe8d3bc0f826da5989"">
          <r a=""1"">537f6823-d22c-4b3b-ab3c-e15a6b61b9d6</r>
        </rtu>
      </database>
    </relations>
  </population>
</allors>";

            var notRestoredEventArgs = new List<RelationNotRestoredEventArgs>();
            this.Population.RelationNotRestored += (o, args) =>
                notRestoredEventArgs.Add(args);

            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            Assert.Single(notRestoredEventArgs);
            var notRestoredEventArg = notRestoredEventArgs.First();
            Assert.Equal(2, notRestoredEventArg.AssociationId);
            Assert.Equal(new Guid("40000000000000000000000000000000"), notRestoredEventArg.RelationTypeId);
            Assert.Equal("T29wcw==", notRestoredEventArg.RoleContents);

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);
                this.c1C = (C1)transaction.Instantiate(3);

                Assert.Equal("A String", this.c1A.C1AllorsString);
                Assert.Equal(true, this.c1C.C1AllorsBoolean);
                Assert.Equal(new Guid("537f6823-d22c-4b3b-ab3c-e15a6b61b9d6"), this.c1A.C1AllorsUnique);
            }
        }
    }

    [Fact]
    public void CantRestoreUnitRole()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtu i=""207138608abd4d718ccc2b4d1b88bce3"">
            <r a=""1"">QSBTdHJpbmc=</r>
        </rtu>
        <rtu i=""87eb0d1973a74aaeaeed66dc9163233c"">
            <r a=""99"">1.1</r>
        </rtu>
        <rtu i=""b4ee673fbba04e249cda3cf993c79a0a"">
            <r a=""1"">true</r>
        </rtu>
        <rtu i=""cef13620b7d74bfe8d3bc0f826da5989"">
          <r a=""1"">537f6823-d22c-4b3b-ab3c-e15a6b61b9d6</r>
        </rtu>
     </database>
    </relations>
  </population>
</allors>";

            var notRestoredEventArgs = new List<RelationNotRestoredEventArgs>();
            this.Population.RelationNotRestored += (o, args) =>
                notRestoredEventArgs.Add(args);

            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            Assert.Single(notRestoredEventArgs);
            var notRestoredEventArg = notRestoredEventArgs.First();
            Assert.Equal(99, notRestoredEventArg.AssociationId);
            Assert.Equal(new Guid("87eb0d1973a74aaeaeed66dc9163233c"), notRestoredEventArg.RelationTypeId);
            Assert.Equal("1.1", notRestoredEventArg.RoleContents);

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);

                Assert.Equal("A String", this.c1A.C1AllorsString);
                Assert.Equal(true, this.c1A.C1AllorsBoolean);
                Assert.Equal(new Guid("537f6823-d22c-4b3b-ab3c-e15a6b61b9d6"), this.c1A.C1AllorsUnique);
            }
        }
    }

    [Fact]
    public void CantRestoreCompositeRelation()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtc i=""2ff1c9ba0017466e9f11776086e6d0b0"">
          <r a=""1"">2</r>
        </rtc>
        <rtc i=""30000000000000000000000000000000"">
          <r a=""2"">3</r>
        </rtc>
        <rtc i=""4c77650277d745d9b10162dee27c0c2e"">
          <r a=""3"">4</r>
        </rtc>
        <rtc i=""ab6d11ccec86482888752e9a779ba627"">
          <r a=""1"">4</r>
        </rtc>
    </database>
    </relations>
  </population>
</allors>";

            var notRestoredEventArgs = new List<RelationNotRestoredEventArgs>();
            this.Population.RelationNotRestored += (o, args) =>
                notRestoredEventArgs.Add(args);

            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            Assert.Single(notRestoredEventArgs);
            var notRestoredEventArg = notRestoredEventArgs.First();
            Assert.Equal(2, notRestoredEventArg.AssociationId);
            Assert.Equal(new Guid("30000000000000000000000000000000"), notRestoredEventArg.RelationTypeId);
            Assert.Equal("3", notRestoredEventArg.RoleContents);

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);
                this.c1B = (C1)transaction.Instantiate(2);
                this.c1C = (C1)transaction.Instantiate(3);
                this.c1D = (C1)transaction.Instantiate(4);

                Assert.Single(this.c1A.C1C1many2manies);
                Assert.Contains(this.c1B, this.c1A.C1C1many2manies);
                Assert.Equal(this.c1D, this.c1C.C1C1one2one);
                Assert.Single(this.c1A.C1C1one2manies);
                Assert.Contains(this.c1D, this.c1A.C1C1one2manies);
            }
        }
    }

    [Fact]
    public void CantRestoreCompositeRole()
    {
        foreach (var init in this.Inits)
        {
            init();
            var m = this.Transaction.Database.Services.Get<IMetaIndex>();

            var xml =
                @"<?xml version=""1.0"" encoding=""utf-16""?>
<allors>
  <population version=""1"">
    <objects>
      <database>
        <ot i=""7041c691d89646288f501c24f5d03414"">1:0,2:0,3:0,4:0</ot>
        <ot i=""72c07e8a03f54da8ab37236333d4f74e"">5:0,6:0,7:0,8:0</ot>
      </database>
    </objects>
    <relations>
      <database>
        <rtc i=""2ff1c9ba0017466e9f11776086e6d0b0"">
          <r a=""1"">2</r>
        </rtc>
        <rtc i=""2cd8b843-f1f5-413d-9d6d-0d2b9b3c5cf6"">
          <r a=""99"">3</r>
        </rtc>
        <rtc i=""4c776502-77d7-45d9-b101-62dee27c0c2e"">
          <r a=""3"">4</r>
        </rtc>
        <rtc i=""ab6d11ccec86482888752e9a779ba627"">
          <r a=""1"">4</r>
        </rtc>
    </database>
    </relations>
  </population>
</allors>";

            var notRestoredEventArgs = new List<RelationNotRestoredEventArgs>();
            this.Population.RelationNotRestored += (o, args) =>
                notRestoredEventArgs.Add(args);

            var stringReader = new StringReader(xml);
            using (var reader = XmlReader.Create(stringReader))
            {
                this.Population.Restore(reader);
            }

            Assert.Single(notRestoredEventArgs);
            var notRestoredEventArg = notRestoredEventArgs.First();
            Assert.Equal(99, notRestoredEventArg.AssociationId);
            Assert.Equal(new Guid("2cd8b843-f1f5-413d-9d6d-0d2b9b3c5cf6"), notRestoredEventArg.RelationTypeId);
            Assert.Equal("3", notRestoredEventArg.RoleContents);

            using (var transaction = this.Population.CreateTransaction())
            {
                this.c1A = (C1)transaction.Instantiate(1);
                this.c1B = (C1)transaction.Instantiate(2);
                this.c1C = (C1)transaction.Instantiate(3);
                this.c1D = (C1)transaction.Instantiate(4);

                Assert.Single(this.c1A.C1C1many2manies);
                Assert.Contains(this.c1B, this.c1A.C1C1many2manies);
                Assert.Equal(this.c1D, this.c1C.C1C1one2one);
                Assert.Single(this.c1A.C1C1one2manies);
                Assert.Contains(this.c1D, this.c1A.C1C1one2manies);
            }
        }
    }

    protected abstract IDatabase CreatePopulation();

    private static string DoBackup(IDatabase otherPopulation)
    {
        var stringWriter = new StringWriter();
        using (var writer = XmlWriter.Create(stringWriter))
        {
            otherPopulation.Backup(writer);
        }

        return stringWriter.ToString();
    }

    private static void DoRestore(IDatabase database, string xml)
    {
        var stringReader = new StringReader(xml);
        using (var reader = XmlReader.Create(stringReader))
        {
            database.Restore(reader);
        }
    }

    private void AssertPopulation(ITransaction transaction)
    {
        var m = transaction.Database.Services.Get<IMetaIndex>();

        Assert.Equal(4, transaction.Filter(m.C1.Composite).Count);
        Assert.Equal(4, transaction.Filter(m.C2.Composite).Count);
        Assert.Equal(4, transaction.Filter(m.C3.Composite).Count);
        Assert.Equal(4, transaction.Filter(m.C4.Composite).Count);

        var c1ACopy = (C1)transaction.Instantiate(this.c1A.Strategy.ObjectId);
        var c1BCopy = (C1)transaction.Instantiate(this.c1B.Strategy.ObjectId);
        var c1CCopy = (C1)transaction.Instantiate(this.c1C.Strategy.ObjectId);
        var c1DCopy = (C1)transaction.Instantiate(this.c1D.Strategy.ObjectId);
        var c2ACopy = (C2)transaction.Instantiate(this.c2A.Strategy.ObjectId);
        var c2BCopy = (C2)transaction.Instantiate(this.c2B.Strategy.ObjectId);
        var c2CCopy = (C2)transaction.Instantiate(this.c2C.Strategy.ObjectId);
        var c2DCopy = (C2)transaction.Instantiate(this.c2D.Strategy.ObjectId);
        var c3ACopy = (C3)transaction.Instantiate(this.c3A.Strategy.ObjectId);
        var c3BCopy = (C3)transaction.Instantiate(this.c3B.Strategy.ObjectId);
        var c3CCopy = (C3)transaction.Instantiate(this.c3C.Strategy.ObjectId);
        var c3DCopy = (C3)transaction.Instantiate(this.c3D.Strategy.ObjectId);
        var c4ACopy = (C4)transaction.Instantiate(this.c4A.Strategy.ObjectId);
        var c4BCopy = (C4)transaction.Instantiate(this.c4B.Strategy.ObjectId);
        var c4CCopy = (C4)transaction.Instantiate(this.c4C.Strategy.ObjectId);
        var c4DCopy = (C4)transaction.Instantiate(this.c4D.Strategy.ObjectId);

        IObject[] everyC1 = [c1ACopy, c1BCopy, c1CCopy, c1DCopy];
        IObject[] everyC2 = [c2ACopy, c2BCopy, c2CCopy, c2DCopy];
        IObject[] everyC3 = [c3ACopy, c3BCopy, c3CCopy, c3DCopy];
        IObject[] everyC4 = [c4ACopy, c4BCopy, c4CCopy, c4DCopy];
        IObject[] everyObject =
        [
            c1ACopy, c1BCopy, c1CCopy, c1DCopy, c2ACopy, c2BCopy, c2CCopy, c2DCopy, c3ACopy, c3BCopy, c3CCopy, c3DCopy, c4ACopy,
            c4BCopy, c4CCopy, c4DCopy,
        ];

        foreach (var allorsObject in everyObject)
        {
            Assert.NotNull(allorsObject);
        }

        if (this.EmptyStringIsNull)
        {
            Assert.False(c1ACopy.ExistC1AllorsString);
        }
        else
        {
            Assert.Equal(string.Empty, c1ACopy.C1AllorsString);
        }

        Assert.Equal(-1, c1ACopy.C1AllorsInteger);
        Assert.Equal(1.1m, c1ACopy.C1AllorsDecimal);
        Assert.Equal(1.1d, c1ACopy.C1AllorsDouble);
        Assert.True(c1ACopy.C1AllorsBoolean);
        Assert.Equal(new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc), c1ACopy.C1AllorsDateTime);
        Assert.Equal(new Guid(GuidString), c1ACopy.C1AllorsUnique);

        Assert.Equal(Array.Empty<byte>(), c1ACopy.C1AllorsBinary);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, c1BCopy.C1AllorsBinary);
        Assert.Null(c1CCopy.C1AllorsBinary);

        Assert.Equal("c1b", c2ACopy.C1WhereC1C2one2one.C1AllorsString);
        Assert.Equal("c1b", c2ACopy.C1WhereC1C2one2many.C1AllorsString);
        Assert.Equal("c1b", c2BCopy.C1WhereC1C2one2many.C1AllorsString);

        Assert.Equal("c3a", c3ACopy.I34AllorsString);
        Assert.Equal("c4a", c4ACopy.I34AllorsString);

        Assert.Equal(2, c2ACopy.C1sWhereC1C2many2one.Count());
        Assert.Empty(c2BCopy.C1sWhereC1C2many2one);
        Assert.Single(c2ACopy.C1sWhereC1C2many2many);
        Assert.Single(c2BCopy.C1sWhereC1C2many2many);

        foreach (S1234 allorsObject in everyObject)
        {
            Assert.Equal(everyObject.Length, allorsObject.S1234many2manies.Count());
            foreach (S1234 addObject in everyObject)
            {
                var objects = allorsObject.S1234many2manies.ToArray();
                Assert.Contains(addObject, objects);
            }
        }
    }

    private void Populate(ITransaction transaction)
    {
        this.c1A = transaction.Build<C1>();
        this.c1B = transaction.Build<C1>();
        this.c1C = transaction.Build<C1>();
        this.c1D = transaction.Build<C1>();
        this.c2A = transaction.Build<C2>();
        this.c2B = transaction.Build<C2>();
        this.c2C = transaction.Build<C2>();
        this.c2D = transaction.Build<C2>();
        this.c3A = transaction.Build<C3>();
        this.c3B = transaction.Build<C3>();
        this.c3C = transaction.Build<C3>();
        this.c3D = transaction.Build<C3>();
        this.c4A = transaction.Build<C4>();
        this.c4B = transaction.Build<C4>();
        this.c4C = transaction.Build<C4>();
        this.c4D = transaction.Build<C4>();

        IObject[] allObjects =
        [
            this.c1A, this.c1B, this.c1C, this.c1D, this.c2A, this.c2B, this.c2C, this.c2D, this.c3A, this.c3B, this.c3C, this.c3D,
            this.c4A, this.c4B, this.c4C, this.c4D,
        ];

        this.c1A.C1AllorsString = string.Empty; // emtpy string
        this.c1A.C1AllorsInteger = -1;
        this.c1A.C1AllorsDecimal = 1.1m;
        this.c1A.C1AllorsDouble = 1.1d;
        this.c1A.C1AllorsBoolean = true;
        this.c1A.C1AllorsDateTime = new DateTime(1973, 3, 27, 12, 1, 2, 3, DateTimeKind.Utc);
        this.c1A.C1AllorsUnique = new Guid(GuidString);
        this.c1A.C1AllorsBinary = Array.Empty<byte>();

        this.c1B.C1AllorsString = "c1b";
        this.c1B.C1AllorsBinary = [0, 1, 2, 3];
        this.c1B.C1C2one2one = this.c2A;
        this.c1B.C1C2many2one = this.c2A;
        this.c1C.C1C2many2one = this.c2A;
        this.c1B.AddC1C2one2many(this.c2A);
        this.c1B.AddC1C2one2many(this.c2B);
        this.c1B.AddC1C2one2many(this.c2C);
        this.c1B.AddC1C2one2many(this.c2D);
        this.c1B.AddC1C2many2many(this.c2A);
        this.c1B.AddC1C2many2many(this.c2B);
        this.c1B.AddC1C2many2many(this.c2C);
        this.c1B.AddC1C2many2many(this.c2D);

        this.c1C.C1AllorsString = "c1c";
        this.c1C.C1AllorsBinary = null;

        this.c3A.I34AllorsString = "c3a";
        this.c4A.I34AllorsString = "c4a";

        foreach (S1234 allorsObject in allObjects)
        {
            foreach (S1234 addObject in allObjects)
            {
                allorsObject.AddS1234many2many(addObject);
            }
        }

        transaction.Commit();
    }

    private static void Dump(IDatabase population)
    {
        using (var stream = File.Create(@"population.xml"))
        {
            using (var writer = XmlWriter.Create(stream))
            {
                population.Backup(writer);
            }
        }
    }
    
    #region population
    private C1 c1A;
    private C1 c1B;
    private C1 c1C;
    private C1 c1D;
    private C1 c1Empty;
    private C2 c2A;
    private C2 c2B;
    private C2 c2C;
    private C2 c2D;
    private C3 c3A;
    private C3 c3B;
    private C3 c3C;
    private C3 c3D;
    private C4 c4A;
    private C4 c4B;
    private C4 c4C;
    private C4 c4D;
    #endregion
}
