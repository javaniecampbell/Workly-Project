using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Workly.Core.Models;

namespace Workly.Core
{
    public class AzureDevOpsApiService
    {
        private readonly string organizationUrl;
        private readonly string personalAccessToken;

        public AzureDevOpsApiService(string organizationUrl, string personalAccessToken)
        {
            this.organizationUrl = organizationUrl;
            this.personalAccessToken = personalAccessToken;
        }

        public async IAsyncEnumerable<Commit> GetAllCommits(string projectIdOrName,string[] repositoryIds)
        {

            var connection = GetVssConnection();
            using (var git = await connection.GetClientAsync<GitHttpClient>())
            {
                foreach (var repo in repositoryIds)
                {
                    var commits = await git.GetCommitsAsync(projectIdOrName, repo, new GitQueryCommitsCriteria
                    {
                        Top = Int32.MaxValue,
                    });

                    await foreach (Commit commit in TransformToModel(commits, repo))
                    {
                        yield return commit;
                    }

                }
            }
        }

        public async IAsyncEnumerable<Commit> TransformToModel(List<GitCommitRef> commits, string repo)
        {
            var connection = GetVssConnection();
            using (var client = await connection.GetClientAsync<GitHttpClient>())
            {
                foreach (var commitRef in commits)
                {
                    var commit = await client.GetCommitAsync(commitRef.CommitId, repo);
                    var repository = await client.GetRepositoryAsync(repo);
                   
                    yield return new Commit
                    {
                        CommitId = commitRef.CommitId,
                        AuthorName = commitRef.Author.Name.Equals("=") ? commit.Push.PushedBy.DisplayName + " *MP" : commitRef.Author.Name,
                        AuthorEmail = commitRef.Author.Email.Equals("=") ? commit.Push.PushedBy.UniqueName + " *MP" : commitRef.Author.Email,
                        AuthorDate = commitRef.Author.Date,
                        CommitterName = commitRef.Committer.Name.Equals("=") ? commit.Push.PushedBy.DisplayName + " *MP" : commitRef.Committer.Name,
                        Subject = commitRef.Comment.Split("\n")[0],
                        CommitterEmail = commitRef.Committer.Email.Equals("=") ? commit.Push.PushedBy.UniqueName + " *MP" : commitRef.Committer.Email,
                        CommitterDate = commitRef.Committer.Date,
                        Body = commit.Comment,
                        CommitMessage = commitRef.Comment,
                        FileChangeAdded = commitRef.ChangeCounts[VersionControlChangeType.Add],
                        FileChangeDeleted = commitRef.ChangeCounts[VersionControlChangeType.Delete],
                        FileChangeEdited = commitRef.ChangeCounts[VersionControlChangeType.Edit],
                        RepositoryName = repository.Name,
                        RepositoryId = repository.Id.ToString(),
                    };
                }
            }
        }
        private VssConnection GetVssConnection()
        {
            var basicCredential = new VssBasicCredential(string.Empty, personalAccessToken);
            var orgUri = new Uri(organizationUrl);
            VssConnection connection = new VssConnection(orgUri, basicCredential);
            return connection;
        }
    }
}
