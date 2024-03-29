﻿namespace Allors.Workspace.Adapters.Tests
{
    using System.Linq;
    using Allors.Workspace;
    using Xunit;

    public class PullResultCollectionAssert<T> where T : class, IObject
    {
        private readonly T[] collection;

        public PullResultCollectionAssert(IPullResult pullResult, string name = null) => this.collection = name != null ? pullResult.GetCollection<T>(name) : pullResult.GetCollection<T>();

        public void Single() => Assert.Single(this.collection);

        public void Equal(params string[] expected)
        {
            var actual = this.collection.Select(v => (string)((dynamic)v).Name.Value).OrderBy(v => v);
            Assert.Equal(expected.OrderBy(v => v), actual);
        }
    }
}
