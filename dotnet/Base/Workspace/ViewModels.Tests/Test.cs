﻿namespace Workspace.ViewModels.Tests
{
    using Allors.Database;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Adapters.Tests;
    using Allors.Workspace.Meta.Static;
    using Configuration = Allors.Workspace.Adapters.Direct.Configuration;
    using Connection = Allors.Workspace.Adapters.Direct.Connection;
    using Person = Allors.Workspace.Domain.Person;

    [TestFixture]
    public abstract class Test
    {
        private Fixture fixture;

        private Func<IWorkspaceServices> servicesBuilder;
        private Configuration configuration;

        public Database Database { get; private set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.fixture = new Fixture();
        }

        [SetUp]
        public void TestSetup()
        {
            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));

            this.servicesBuilder = () => new WorkspaceServices(objectFactory, metaPopulation);
            this.configuration = new Configuration("Default", metaPopulation);

            this.Database = new Database(
                new DefaultDatabaseServices(fixture.Engine),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(Allors.Database.Domain.Person)),
                });

            this.Database.Init();

            var config = new Config();
            new Setup(this.Database, config).Apply();

            using var transaction = this.Database.CreateTransaction();

            var administrator = transaction.Build<Allors.Database.Domain.Person>(v => v.UserName = "administrator");
            new UserGroups(transaction).Administrators.AddMember(administrator);
            transaction.Services.Get<IUserService>().User = administrator;

            transaction.Derive();
            transaction.Commit();
        }

        public Connection Connect(string userName)
        {
            using var transaction = this.Database.CreateTransaction();
            var user = new Users(transaction).Extent().ToArray().First(v => v.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
            transaction.Services.Get<IUserService>().User = user;
            return new Connection(this.configuration, this.Database, this.servicesBuilder) { UserId = user.Id };
        }
    }
}