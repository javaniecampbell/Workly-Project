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

        public async IAsyncEnumerable<Commit> GetAllCommits()
        {
            string[] repos = new string[] {
                "dc1e47d0-4b54-4bec-a60c-6dd6ef15df91",
                "946fd4ff-c9b0-43ff-bc8e-9ee68b2dcc6f",
                "fee96637-9fc7-428c-8d25-bd50d3961325",
                "9993424a-e46b-47d7-a00c-f128402cbb71",
                "0970eff7-d8ea-4678-b02d-97c7486802aa"
            };
            var connection = GetVssConnection();
            using (var git = await connection.GetClientAsync<GitHttpClient>())
            {
                foreach (var repo in repos)
                {
                    var commits = await git.GetCommitsAsync("40ef9a6b-d17b-4454-b6a6-3de33e04504f", repo, new GitQueryCommitsCriteria
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
