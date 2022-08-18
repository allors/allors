namespace Allors.Workspace.Sync.Response
{
    using System.Collections.Generic;

    public interface IPullRequestGrant
    {
        long Version { get; }

        ISet<string> ReadTags { get; }

        ISet<string> WriteTags { get; }

        ISet<string> ExecuteTags { get; }
    }
}